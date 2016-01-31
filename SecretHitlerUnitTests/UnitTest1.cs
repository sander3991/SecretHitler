using System;
using SecretHitler.Networking;
using SecretHitler.Objects;
using SecretHitler.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SecretHitlerUnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMultipleObjectSender()
        {
            var player = Player.GetPlayer("Sander");
            var player2 = Player.GetPlayer("Stefan");
            player.Hand = new PlayerHand(new CardSecretRoleFascist(1), new CardMembershipFascist());
            player2.Hand = new PlayerHand(new CardSecretRoleFascist(0), new CardMembershipFascist());
            var msg = new NetworkMultipleObject(new NetworkRevealRoleObject(player), new NetworkRevealRoleObject(player2));
            var decoder = ServerCommands.Multiple.GetDecoder();
            var bytes = decoder.GenerateByteStream(msg);
            var decoded = decoder.GenerateObject(bytes);
            Assert.IsInstanceOfType(decoded, typeof(NetworkMultipleObject));
            var receivedMsg = decoded as NetworkMultipleObject;
            Assert.IsTrue(receivedMsg.Objects.Length == 2);
        }
    }
}
