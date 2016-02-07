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
                    response.AddObject(new NetworkPlayerObject(ServerCommands.KillPlayer, killPlayer.Player));
                    gameState.KillPlayer(killPlayer.Player);
                    if (killPlayer.Player.Hand.Role.IsHitler)
                    {
                        AnnounceWin(response, false, "Hitler was killed");
                        break;
                    }
                    else
                        response.AddObject(new NetworkPlayerObject(ServerCommands.NotHitler, killPlayer.Player));
                    GetNextPresident(response);
                    break;

                case ServerCommands.PresidentActionChoosePresidentResponse:
                    if (player != gameState.President || gameState.AwaitingPresidentAction != request.Command) break;
                    var presObj = request as NetworkPlayerObject;
                    if (presObj.Player == gameState.President) break;
                    var presidentAfterChoice = gameState.GetNextPresident();
                    SetPresident(response, presObj.Player);
                    gameState.SetNextPresident(presidentAfterChoice);
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

        private void AnnounceWin(ServerResponse response, bool fascistWin, string reason)
        {
            var playerCount = gameState.PlayerCount;
            for (var i = 0; i < playerCount; i++)
                response.AddObject(new NetworkRevealRoleObject(gameState.SeatedPlayers[i]));
            response.AddObject(new NetworkObject(fascistWin ? ServerCommands.FascistWin : ServerCommands.LiberalWin, reason));
            gameState.EndGame();
        }

        internal void SetPresident(ServerResponse response, Player president)
        {
            gameState.SetPresident(president);
            gameState.SetChancellor(null);
            response.AddObject(new NetworkPlayerObject(ServerCommands.AnnouncePresident, president));
        }
        private void GetNextPresident(ServerResponse response)
        {
            var president = gameState.GetNextPresident();
            SetPresident(response, president);
        }
        private void AnnounceVotes(ServerResponse response)
        {
            bool passed = gameState.VotePassed();
            response.AddObject(new NetworkVoteResultObject(ServerCommands.AnnounceVotes, gameState.GetVotes(), passed ? Vote.Ja : Vote.Nein));
            gameState.PreviousGovernmentElected = passed;
            if (gameState.ElectionTracker == 3)
            {
                response.AddObject(new NetworkObject(ServerCommands.ResetElectionTracker));
                gameState.ResetElectionTracker();
            }
            if (passed)
            {
                if (gameState.ElectionTracker != 0)
                {
                    response.AddObject(new NetworkObject(ServerCommands.ResetElectionTracker));
                    gameState.ResetElectionTracker();
                }
                if (gameState.FascistsCardsPlayed >= 3)
                {
                    if (gameState.Chancellor.Hand.Role.IsHitler)
                    {
                        AnnounceWin(response, true, "Hitler is chancellor");
                        return; //no point in doing the rest, the fascists have won anyway
                    }
                    response.AddObject(new NetworkPlayerObject(ServerCommands.NotHitler, gameState.Chancellor));
                }
                var cards = gameState.GetPolicyCards();
                gameState.CurrentlyPicked = cards;
                response.AddObject(new NetworkByteObject(ServerCommands.PolicyCardsDrawn, 3));
                response.AddObject(new NetworkCardObject(ServerCommands.PresidentPickPolicyCard, cards), gameState.President);
                gameState.PresidentPicking = true;
            }
            else
            {
                gameState.IncrementElectionTracker();
                response.AddObject(new NetworkObject(ServerCommands.IncrementElectionTracker));
                if (gameState.ElectionTracker == 3)
                {
                    //gameState.ResetElectionTracker();
                    response.AddObject(new NetworkByteObject(ServerCommands.PolicyCardsDrawn, 1));
                    var card = gameState.GetPolicyCards(1);
                    //PlayPolicy(card[0], response);
                    response.AddObject(new NetworkCardObject(ServerCommands.CardPlayed, card));
                    gameState.PlayPolicy(card[0]);
                    if (CheckCardPlayedWinCondition(response))
                        return;
                }
                GetNextPresident(response);
            }
        }
        private bool CheckCardPlayedWinCondition(ServerResponse response)
        {

            if (gameState.LiberalCardsPlayed == 5)
            {
                AnnounceWin(response, false, "The liberals have played 5 liberal policies");
                return true;
            }
            if (gameState.FascistsCardsPlayed == 6)
            {
                AnnounceWin(response, true, "The fascists have played 6 fascist policies");
                return true;
            }
            return false;
        }
        private void PlayPolicy(CardPolicy policy, ServerResponse response)
        {
            gameState.PlayPolicy(policy);
            response.AddObject(new NetworkCardObject(ServerCommands.ChancellorDiscarded));
            response.AddObject(new NetworkCardObject(ServerCommands.CardPlayed, policy));
            if(policy is CardPolicyLiberal)
            {
                if (!CheckCardPlayedWinCondition(response))
                    GetNextPresident(response);
            }
            else
            {
                if (!CheckCardPlayedWinCondition(response))
                {
                    if (gameState.FascistActions[gameState.FascistsCardsPlayed - 1] != null)
                    {
                        var fascistId = gameState.FascistsCardsPlayed - 1;
                        var fascistAction = gameState.FascistActions[fascistId];
                        response.AddObject(new NetworkByteObject(ServerCommands.PresidentAction, (byte)fascistId));
                        response.AddObject(fascistAction.GetPresidentObject(gameState), gameState.President);
                        gameState.AwaitingPresidentAction = fascistAction.ServerResponse;
                    }
                    else
                        GetNextPresident(response);
                }
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
