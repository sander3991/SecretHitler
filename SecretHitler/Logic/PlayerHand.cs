using SecretHitler.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Logic
{
    public class PlayerHand
    {
        public CardSecretRole Role { get; set; }
        public CardMembership Membership { get; set; }
        public CardBallotYes Yes { get; set; }
        public CardBallotNo No { get; set; }
        public CardBallotYes VoteCast { get; set; }

        public PlayerHand(CardSecretRole role, CardMembership membership, bool flipped = false)
        {
            Role = role;
            Membership = membership;
            Yes = new CardBallotYes() { Flipped = flipped };
            No = new CardBallotNo() { Flipped = flipped };
        }

        public PlayerHand(Card[] cards)
        {
            foreach(var card in cards)
            {
                if (card is CardSecretRole)
                    if (Role != null)
                        throw new ArgumentException("Multiple secret role cards!", nameof(cards));
                    else
                        Role = card as CardSecretRole;
                else if (card is CardMembership)
                    if (Membership != null)
                        throw new ArgumentException("Multiple membership cards!", nameof(cards));
                    else
                        Membership = card as CardMembership;
                else if (card is CardBallotYes)
                    Yes = card as CardBallotYes;
                else if (card is CardBallotNo)
                    No = card as CardBallotNo;
            }
            if (Role == null)
                throw new ArgumentException("Missing a secret role card!", nameof(cards));
            if (Membership == null)
                throw new ArgumentException("Missing a membership card!", nameof(cards));
            if (Yes == null)
                Yes = new CardBallotYes();
            if (No == null)
                No = new CardBallotNo();
        }

        public void SetUnkown()
        {
            Role = new CardSecretRoleUnknown() { Flipped = true};
            Membership = new CardMembershipUnknown() { Flipped = true};
        }
    }
}
