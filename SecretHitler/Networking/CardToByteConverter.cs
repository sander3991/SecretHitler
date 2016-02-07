using SecretHitler.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Networking
{
    public static class CardToByteConverter
    {
        private enum CardType : byte
        {
            Unsupported = 0, Ballot = 1, Membership = 2, NotHitler = 3, Policy = 4, SecretRole = 5
        }
        public static byte ToByte(this Card card)
        {
            CardType identifier = CardType.Unsupported;
            if (card is CardMembership)
                identifier = CardType.Membership;
            else if (card is CardNotHitler)
                identifier = CardType.NotHitler;
            else if (card is CardPolicy)
                identifier = CardType.Policy;
            else if (card is CardSecretRole)
                identifier = CardType.SecretRole;
            else if (card is CardBallot)
                identifier = CardType.Ballot;
            if (identifier == CardType.Unsupported)
                throw new CardUnsupportedException();
            //First 3 xxxx x000, the 000 are the CardType, the other 5 bits may be filled by the user
            byte b = (byte)identifier;
            byte add = 0;
            switch (identifier)
            {
                case CardType.Ballot:
                    add = (card as CardBallot).Yes ? (byte)1 : (byte)0;
                    break;
                case CardType.Membership:
                    add = (card as CardMembership).IsFascist ? (byte)1 : (byte)0;
                    break;
                case CardType.Policy:
                    add = card is CardPolicyVeto ? (byte)2 : (card as CardPolicy).IsFascist ? (byte)1 : (byte)0;
                    break;
                case CardType.SecretRole:
                    var secretRole = card as CardSecretRole;
                    int info = (secretRole.ID << 1) | (secretRole.IsFascist ? 1 : 0);
                    add = (byte)info;
                    break;
            }
            if (add > 31)
                throw new ArgumentOutOfRangeException("Add may not be higher then 31! It must fit in 5 bits!", nameof(add));
            b |= (byte)(add << 3);
            return b;
        }

        public static Card ToCard(this byte b)
        {
            var cardType = (CardType)(b & 7); //7 = 0000 0111
            var add = (byte)(b >> 3);
            switch (cardType)
            {
                case CardType.Ballot:
                    return add == 1 ? (CardBallot)new CardBallotYes() : new CardBallotNo();
                case CardType.Membership:
                    return add == 1 ? (CardMembership)new CardMembershipFascist() : new CardMembershipLiberal();
                case CardType.NotHitler:
                    return new CardNotHitler();
                case CardType.Policy:
                    return add == 2 ? new CardPolicyVeto() : add == 1 ? (CardPolicy)new CardPolicyFascist() : new CardPolicyLiberal();
                case CardType.SecretRole:
                    var fascist = (add & 1) == 1;
                    var ID = add >> 1;
                    return fascist ? (CardSecretRole)new CardSecretRoleFascist(ID) : new CardSecretRoleLiberal(ID);
            }
            throw new CardUnsupportedException("Couldn't convert byte to Card");
        }

        public class CardUnsupportedException : Exception
        {
            public CardUnsupportedException(string error = "This card type is not supported!") : base(error) { }
        }
    }
}
