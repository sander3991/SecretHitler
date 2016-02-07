using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecretHitler.Networking;
using SecretHitler.Logic;
using SecretHitler.Objects;

namespace SecretHitlerUnitTests
{
    [TestClass]
    public class Networking
    {
        [TestMethod]
        public void NetworkObject()
        {
            var decoder = new NetworkObject.DefaultObjectReader();

            foreach (var tuple in new Tuple<ServerCommands, string>[]
            {
                new Tuple<ServerCommands, string>(ServerCommands.OK, "Hi!"),
                new Tuple<ServerCommands, string>(ServerCommands.Message, "Bye!")
            }
            )
            {
                var obj = new NetworkObject(tuple.Item1, tuple.Item2);
                var bytes = decoder.GenerateByteStream(obj);
                var generateObj = decoder.GenerateObject(bytes, false);
                CompareDefaultObject(obj, generateObj);
            }
        }
        private void CompareDefaultObject(NetworkObject obj, NetworkObject generateObj)
        {
            Assert.IsTrue(obj.ID == generateObj.ID);
            Assert.IsTrue(obj.Message == generateObj.Message);
            Assert.IsTrue(obj.Command == generateObj.Command);
        }
        [TestMethod]
        public void NetworkCardObject()
        {
            var networkObj = new NetworkCardObject(ServerCommands.AnnounceCard, new CardBallotNo(), new CardBallotYes());
            var decoder = new NetworkCardObject.CardObjectReader();
            var bytes = decoder.GenerateByteStream(networkObj);
            var generatedObj = decoder.GenerateObject(bytes, false);
            Assert.IsInstanceOfType(generatedObj, typeof(NetworkCardObject));
            var generatedCardObj = generatedObj as NetworkCardObject;
            Assert.IsTrue(generatedCardObj.Cards.Length == networkObj.Cards.Length);
            for(var i = 0; i < generatedCardObj.Cards.Length; i++)
                Assert.AreSame(generatedCardObj.Cards[i].GetType(), networkObj.Cards[i].GetType());
            CompareDefaultObject(networkObj, generatedCardObj);
        }
        [TestMethod]
        public void NetworkGameStateObject()
        {
            var gamestate = new ServerGameState(null);
            gamestate.SeatedPlayers = new Player[10];
            var player1 = Player.GetPlayerServerSide("Sander");
            var player2 = Player.GetPlayerServerSide("Stefan");
            gamestate.SeatPlayer(player1);
            gamestate.SeatPlayer(player2);
            var obj = new NetworkGameStateObject(ServerCommands.SendGameState, gamestate);
            var decoder = new NetworkGameStateObject.GameStateObjectReader();
            var bytes = decoder.GenerateByteStream(obj);
            var generatedObj = decoder.GenerateObject(bytes, false);
            Assert.IsInstanceOfType(generatedObj, typeof(NetworkGameStateObject));
            var generatedGamestate = generatedObj as NetworkGameStateObject;
            Assert.IsTrue(generatedGamestate.GameState.PlayerCount == gamestate.PlayerCount);
            for(var i = 0; i < gamestate.SeatedPlayers.Length; i++)
            {
                if (gamestate.SeatedPlayers[i] == null)
                    Assert.IsNull(generatedGamestate.GameState.SeatedPlayers[i]);
                else
                {
                    Assert.IsNotNull(generatedGamestate.GameState.SeatedPlayers[i]);
                    Assert.AreEqual(generatedGamestate.GameState.SeatedPlayers[i].Name, gamestate.SeatedPlayers[i].Name);
                }
            }
            CompareDefaultObject(obj, generatedGamestate);
        }
        [TestMethod]
        public void NetworkMessageObject()
        {
            var messages = new string[,]
            {
                { "Hallo", "Sander" },
                { "ükn³³‘rtï", "Stefan" },
                { "1235",  "Maikel"},
                { "Hallo met spaties enzo!", "Cheeki Breeki" }
            };
            var decoder = new NetworkMessageObject.MessageObjectReader();
            for(var i = 0; i < messages.GetLength(0); i++)
            {
                var obj = new NetworkMessageObject(messages[i, 1], messages[i, 0]);
                var bytes = decoder.GenerateByteStream(obj);
                var generatedObj = decoder.GenerateObject(bytes, false);
                Assert.IsInstanceOfType(generatedObj, typeof(NetworkMessageObject));
                var msgObj = generatedObj as NetworkMessageObject;
                Assert.AreEqual(msgObj.Username, obj.Username);
                Assert.AreEqual(msgObj.Message, obj.Message);
                CompareDefaultObject(obj, msgObj);
            }
        }
        [TestMethod]
        public void TestMultipleObjectSender()
        {
            var player = Player.GetPlayer("Sander");
            var player2 = Player.GetPlayer("Stefan");
            player.Hand = new PlayerHand(new CardSecretRoleFascist(1), new CardMembershipFascist());
            player2.Hand = new PlayerHand(new CardSecretRoleFascist(0), new CardMembershipFascist());
            var msg = new NetworkMultipleObject(
                new NetworkRevealRoleObject(player), 
                new NetworkRevealRoleObject(player2),
                new NetworkMessageObject("Sander", "Hallo ik ben een vis"),
                new NetworkNewPlayerObject(ServerCommands.PlayerConnected, player, 1),
                new NetworkPlayerObject(ServerCommands.AnnouncePresident, player)
            );
            var decoder = ServerCommands.Multiple.GetDecoder();
            var bytes = decoder.GenerateByteStream(msg);
            var decoded = decoder.GenerateObject(bytes, false);
            Assert.IsInstanceOfType(decoded, typeof(NetworkMultipleObject));
            var receivedMsg = decoded as NetworkMultipleObject;
            Assert.IsTrue(receivedMsg.Objects.Length == msg.Objects.Length);
            for (var i = 0; i < receivedMsg.Objects.Length; i++)
                CompareDefaultObject(receivedMsg.Objects[i], msg.Objects[i]);
            CompareDefaultObject(msg, receivedMsg);
        }
        [TestMethod]
        public void NetworkNewPlayerObject()
        {
            var obj = new NetworkNewPlayerObject(ServerCommands.PlayerConnected, Player.GetPlayer("Sander"), 1);
            var decoder = new NetworkNewPlayerObject.NewPlayerObjectReader();
            var bytes = decoder.GenerateByteStream(obj);
            var generatedObj = decoder.GenerateObject(bytes, false);
            Assert.IsInstanceOfType(generatedObj, typeof(NetworkNewPlayerObject));
            var newPlayerObj = generatedObj as NetworkNewPlayerObject;
            Assert.AreEqual(newPlayerObj.SeatPos, obj.SeatPos);
            Assert.AreEqual(newPlayerObj.Player, obj.Player);
            CompareDefaultObject(obj, newPlayerObj);
        }
        [TestMethod]
        public void NetworkPlayerObject()
        {
            var player = Player.GetPlayer("Sander");
            var decoder = new NetworkPlayerObject.PlayerObjectReader();
            var obj = new NetworkPlayerObject(ServerCommands.AnnouncePresident, player);
            var bytes = decoder.GenerateByteStream(obj);
            var playerObj = decoder.GenerateObject(bytes, false);
            Assert.IsInstanceOfType(playerObj, typeof(NetworkPlayerObject));
            Assert.AreSame(player, (playerObj as NetworkPlayerObject).Player);
            CompareDefaultObject(obj, playerObj);
        }
        [TestMethod]
        public void NetworkRevealRoleObject()
        {
            var player = Player.GetPlayerServerSide("Sander");
            player.Hand = new PlayerHand(new CardSecretRoleFascist(1), new CardMembershipFascist());
            var obj = new NetworkRevealRoleObject(player);
            var decoder = new NetworkRevealRoleObject.RevealRoleObjectReader();
            var bytes = decoder.GenerateByteStream(obj);
            var generatedObj = decoder.GenerateObject(bytes, false);
            Assert.IsInstanceOfType(generatedObj, typeof(NetworkRevealRoleObject));
            var newPlayerObj = generatedObj as NetworkRevealRoleObject;
            Assert.AreEqual(newPlayerObj.Player.Name, obj.Player.Name);
            Assert.AreEqual(newPlayerObj.Player.Hand.Role.IsFascist, obj.Player.Hand.Role.IsFascist);
            Assert.AreEqual(newPlayerObj.Player.Hand.Role.ID, obj.Player.Hand.Role.ID);
            Assert.AreEqual(newPlayerObj.Player.Hand.Membership.IsFascist, obj.Player.Hand.Membership.IsFascist);
            CompareDefaultObject(obj, generatedObj);
        }
        [TestMethod]
        public void NetworkBoolObject()
        {
            var boolObj = new NetworkBoolObject(ServerCommands.CastVote, true) { Message = "Test"};
            var decoder = new NetworkBoolObject.BoolObjectReader();
            var bytes = decoder.GenerateByteStream(boolObj);
            var generated = decoder.GenerateObject(bytes, false) as NetworkBoolObject;
            Assert.IsTrue(generated.Value);
            Assert.AreEqual("Test", generated.Message);
            boolObj = new NetworkBoolObject(ServerCommands.CastVote, false);
            decoder = new NetworkBoolObject.BoolObjectReader();
            bytes = decoder.GenerateByteStream(boolObj);
            generated = decoder.GenerateObject(bytes, false) as NetworkBoolObject;
            Assert.IsFalse(generated.Value);
            CompareDefaultObject(boolObj, generated);
        }

        [TestMethod]
        public void NetworkVoteResultObject()
        {
            var votes = new Vote[]
            {
                Vote.Ja, Vote.Dead, Vote.Ja, Vote.Ja, Vote.Nein, Vote.Nein, Vote.Ja
            };
            var obj = new NetworkVoteResultObject(ServerCommands.AnnounceVotes, votes, Vote.Ja);
            var decoder = new NetworkVoteResultObject.VoteResultObjectDecoder();
            var bytes = decoder.GenerateByteStream(obj);
            var generated = decoder.GenerateObject(bytes, false) as NetworkVoteResultObject;
            Assert.AreEqual(votes.Length, generated.Votes.Length);
            for(var i = 0; i < votes.Length; i++)
            {
                Assert.AreEqual(votes[i], generated.Votes[i]);
            }
            Assert.AreEqual(obj.Passed, generated.Passed);
            CompareDefaultObject(obj, generated);
        }

        [TestMethod]
        public void ByteObjectDecoder()
        {
            var decoder = new NetworkByteObject.ByteObjectDecoder();
            for(byte i = 0; i < 10; i++)
            {
                var obj = new NetworkByteObject(ServerCommands.PolicyCardsDrawn, i);
                var bytes = decoder.GenerateByteStream(obj);
                var generated = decoder.GenerateObject(bytes, false) as NetworkByteObject;
                Assert.AreEqual(obj.Value, generated.Value);
                CompareDefaultObject(obj, generated);
            }
        }

        [TestMethod]
        public void NetworkFascistActionObject()
        {
            /*var decoder = new NetworkFascistActionObject.FascistActionObjectReader();
            for(byte i = 0; i < 10; i++)
            {
                var obj = new NetworkFascistActionObject(ServerCommands.PresidentAction, Player.GetPlayer("Player_" + i), i);
                var bytes = decoder.GenerateByteStream(obj);
                var generated = decoder.GenerateObject(bytes, false) as NetworkFascistActionObject;
                Assert.AreEqual(obj.Action, generated.Action);
                Assert.AreEqual(obj.Player, generated.Player);
                CompareDefaultObject(obj, generated);
            }*/
        }
    }
}
