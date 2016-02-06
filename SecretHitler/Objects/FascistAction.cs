using SecretHitler.Logic;
using SecretHitler.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Objects
{
    public abstract class FascistAction
    {
        public string Description { get; }
        protected FascistAction(string description)
        {
            Description = description;
        }
        private static FascistActionExamine examineDeck;
        private static FascistActionKill killPlayer;
        private static FascistActionInvestigate investigateIdentity;
        private static FascistActionChoosePresident choosePresident;
        static FascistAction()
        {
            examineDeck = new FascistActionExamine();
            killPlayer = new FascistActionKill();
            investigateIdentity = new FascistActionInvestigate();
            choosePresident = new FascistActionChoosePresident();
        }
        public abstract NetworkObject GetPresidentObject(ServerGameState gameState);
        public static FascistAction[] GetActionsForPlayers(int players)
        {
            if (players <= 6)
                return new FascistAction[] { examineDeck, null, examineDeck, killPlayer, killPlayer, null };
            if (players <= 8)
                return new FascistAction[] { null, investigateIdentity, choosePresident, killPlayer, killPlayer, null };
            return new FascistAction[] { investigateIdentity, investigateIdentity, choosePresident, killPlayer, killPlayer, null };
        }
    }
    public class FascistActionExamine : FascistAction
    {
        public FascistActionExamine() : base("{0} may examine the top three cards") { }
        public override NetworkObject GetPresidentObject(ServerGameState gameState)
        {
            var cards = gameState.GetPolicyCards(3);
            gameState.ReturnPolicyCards(cards);
            return new NetworkCardObject(ServerCommands.PresidentActionExamine, cards);
        }
    }
    public class FascistActionKill : FascistAction
    {
        public FascistActionKill() : base("{0} may kill a player") { }
        public override NetworkObject GetPresidentObject(ServerGameState gameState)
        {
            return new NetworkObject(ServerCommands.PresidentActionKill);
        }
    }
    public class FascistActionInvestigate : FascistAction
    {
        public FascistActionInvestigate() : base("{0} may investigate a player's identity") { }
        public override NetworkObject GetPresidentObject(ServerGameState gameState)
        {
            return new NetworkObject(ServerCommands.PresidentActionInvestigate);
        }
    }
    public class FascistActionChoosePresident : FascistAction
    {
        public FascistActionChoosePresident() : base("{0} may choose the next presidential canditate") { }
        public override NetworkObject GetPresidentObject(ServerGameState gameState)
        {
            return new NetworkObject(ServerCommands.PresidentActionChoosePresident);
        }
    }
}
