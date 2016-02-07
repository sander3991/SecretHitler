using SecretHitler.Logic;
using SecretHitler.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Networking
{
    public class ServerMessageHandler
    {
        public event Action<Player, NetworkObject> OnReceive;
        private Server server;
        private ServerGameState gameState;
        public ServerMessageHandler(Server server, ServerGameState gameState)
        {
            this.server = server;
            this.gameState = gameState;
        }
        internal void HandleMessage(NetworkObject request, TcpClient sentBy, Player player)
        {
            var response = new ServerResponse();
            response.AddObject(new NetworkObject(ServerCommands.OK, ID: request.ID), player);
            switch (request.Command)
            {
                case ServerCommands.Message:
                    var msgResponse = request as NetworkMessageObject;
                    response.AddObject(new NetworkMessageObject(msgResponse.Username, msgResponse.Message, sendToServer: false));
                    break;
                case ServerCommands.GetGameState:
                    response.AddObject(new NetworkGameStateObject(ServerCommands.SendGameState, gameState), player);
                    break;
                case ServerCommands.AnnounceChancellor:
                    var playerResponse = request as NetworkPlayerObject;
                    if (gameState.MayElectPlayer(playerResponse.Player))
                    {
                        gameState.ResetVotes();
                        gameState.AreVoting = true;
                        gameState.SetChancellor(playerResponse.Player);
                        response.AddObject(playerResponse);
                    }
                    break;
                case ServerCommands.CastVote:
                    if (!gameState.AreVoting) break;
                    var boolObj = request as NetworkBoolObject;
                    gameState.SetVote(player, boolObj.Value);
                    var playerVoted = new NetworkPlayerObject(ServerCommands.PlayerVoted, player);
                    response.AddObject(playerVoted);
                    //multObj.AddObject(playerVoted);
                    if (gameState.EveryoneVoted())
                    {
                        gameState.AreVoting = false;
                        //everyone voted
                        AnnounceVotes(response);
                    }
                    break;
                case ServerCommands.PresidentPolicyCardPicked:
                    if (player != gameState.President || !gameState.PresidentPicking) break;
                    gameState.PresidentPicking = false;
                    var pickPolicyCard = request as NetworkByteObject;
                    gameState.DiscardPile.AddCard(gameState.CurrentlyPicked[pickPolicyCard.Value]);
                    response.AddObject(new NetworkObject(ServerCommands.PresidentDiscarded));
                    var chancellorCards = new CardPolicy[2];
                    var j = 0;
                    for (var i = 0; i < 3; i++)
                        if (i != pickPolicyCard.Value)
                            chancellorCards[j++] = gameState.CurrentlyPicked[i];
                    gameState.CurrentlyPicked = chancellorCards;
                    response.AddObject(new NetworkCardObject(ServerCommands.ChancellorPickPolicyCard, chancellorCards), gameState.Chancellor);
                    gameState.ChancellorPicking = true;
                    break;
                case ServerCommands.ChancellorPolicyCardPicked:
                    if (player != gameState.Chancellor || !gameState.ChancellorPicking) break;
                    gameState.ChancellorPicking = false;
                    var pickCard = request as NetworkByteObject;
                    gameState.DiscardPile.AddCard(gameState.CurrentlyPicked[pickCard.Value]);
                    var playCard = gameState.CurrentlyPicked[Math.Abs(pickCard.Value - 1)];
                    gameState.CurrentlyPicked = null;
                    PlayPolicy(playCard, response);
                    break;
                case ServerCommands.PresidentActionExamineResponse:
                    if (player != gameState.President || gameState.AwaitingPresidentAction != request.Command) break;
                    GetNextPresident(response);
                    break;
                case ServerCommands.PresidentActionKillResponse:
                    if (player != gameState.President || gameState.AwaitingPresidentAction != request.Command) break;
                    var killPlayer = request as NetworkPlayerObject;
                    gameState.KillPlayer(killPlayer.Player);
                    response.AddObject(new NetworkPlayerObject(ServerCommands.KillPlayer, killPlayer.Player));
                    GetNextPresident(response);
                    break;

                case ServerCommands.PresidentActionChoosePresidentResponse:
                    if (player != gameState.President || gameState.AwaitingPresidentAction != request.Command) break;
                    var presObj = request as NetworkPlayerObject;
                    if (presObj.Player == gameState.President) break;
                    SetPresident(response, presObj.Player);
                    break;
                case ServerCommands.PresidentActionInvestigatePresidentResponse:
                    if (player != gameState.President || gameState.AwaitingPresidentAction != request.Command) break;
                    var investigate = request as NetworkPlayerObject;
                    if (investigate.Player == player) break;
                    response.AddObject(new NetworkNewPlayerObject(ServerCommands.RevealMembership, investigate.Player, investigate.Player.Hand.Membership.IsFascist ? 1 : 0), player);
                    GetNextPresident(response);
                    break;
            }
            server.SendResponse(response);
            OnReceive?.Invoke(player, request);
        }

        internal void SetPresident(ServerResponse response, Player president)
        {
            response.AddObject(new NetworkPlayerObject(ServerCommands.AnnouncePresident, president));
        }

        private void GetNextPresident(ServerResponse response)
        {
            var president = gameState.GetNextPresident();
            gameState.SetPresident(president);
            gameState.SetChancellor(null);
            SetPresident(response, president);
        }
        private void AnnounceVotes(ServerResponse response)
        {
            bool passed = gameState.VotePassed();
            response.AddObject(new NetworkVoteResultObject(ServerCommands.AnnounceVotes, gameState.GetVotes(), passed ? Vote.Ja : Vote.Nein));
            gameState.PreviousGovernmentElected = passed;
            if (passed)
            {
                var cards = gameState.GetPolicyCards();
                gameState.CurrentlyPicked = cards;
                response.AddObject(new NetworkByteObject(ServerCommands.PolicyCardsDrawn, 3));
                response.AddObject(new NetworkCardObject(ServerCommands.PresidentPickPolicyCard, cards), gameState.President);
                gameState.PresidentPicking = true;
            }
            else
                GetNextPresident(response);
        }
        private void PlayPolicy(CardPolicy policy, ServerResponse response)
        {
            gameState.PlayPolicy(policy);
            response.AddObject(new NetworkCardObject(ServerCommands.ChancellorDiscarded));
            response.AddObject(new NetworkCardObject(ServerCommands.CardPlayed, policy));
            if(policy is CardPolicyLiberal || gameState.FascistActions[gameState.FascistsCardsPlayed - 1] == null)
                GetNextPresident(response);
            else
            {
                var fascistId = gameState.FascistsCardsPlayed - 1;
                var fascistAction = gameState.FascistActions[fascistId];
                response.AddObject(new NetworkByteObject(ServerCommands.PresidentAction, (byte)fascistId));
                response.AddObject(fascistAction.GetPresidentObject(gameState), gameState.President);
                gameState.AwaitingPresidentAction = fascistAction.ServerResponse;
            }

        }

    }
    internal class ServerResponse
    {
        private Dictionary<Player, NetworkMultipleObject> objects;
        private NetworkMultipleObject all;
        private NetworkMultipleObject noDead;
        private bool responsePrepared;
        public void AddObject(NetworkObject obj, Player player = null, bool deadPlayers = true)
        {
            if (responsePrepared) throw new InvalidOperationException("Can't add objects when the object has been sent");
            if(player != null)
            {
                if (objects == null)
                    objects = new Dictionary<Player, NetworkMultipleObject>();
                if (!objects.ContainsKey(player))
                    objects.Add(player, obj is NetworkMultipleObject ? (obj as NetworkMultipleObject) : new NetworkMultipleObject(obj));
                else
                    objects[player].AddObject(obj);
            }
            else if (deadPlayers)
            {
                if (all == null)
                    all = obj is NetworkMultipleObject ? (obj  as NetworkMultipleObject) : new NetworkMultipleObject(obj);
                else
                    all.AddObject(obj);
            }
            else
            {
                if (noDead == null)
                    noDead = obj is NetworkMultipleObject ? (obj as NetworkMultipleObject) : new NetworkMultipleObject(obj);
                else
                    all.AddObject(obj);
            }
        }
        private void PrepareResponses()
        {
            if (noDead != null) //contains everything non dead people need
                noDead.AddObject(all);
            else
                noDead = all;
            if(objects != null)
                foreach(var player in objects.Keys)
                {
                    if (noDead != null && !player.Dead)
                        objects[player].AddObject(noDead);
                    else if (all != null)
                        objects[player].AddObject(all);
                }
            responsePrepared = true;
        }
        public NetworkMultipleObject GetResponse(Player player)
        {
            if (!responsePrepared)
                PrepareResponses();
            if (objects != null && objects.ContainsKey(player))
                return objects[player];
            return player.Dead ? all : noDead;
        }
    }
}
