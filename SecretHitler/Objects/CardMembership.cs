using SecretHitler.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Objects
{
    abstract class CardMembership : Card
    {
        private static Image BACK = Properties.Resources.membership_card_backcover.CutToSize(DEFAULTCARDSIZE);
        public abstract bool IsFascist { get; }
        public override Image Back
        {
            get
            {
                return BACK;
            }
        }
    }
    class CardMembershipLiberal : CardMembership
    {
        private static Image FRONT = Properties.Resources.membership_cards.FromSprite(DEFAULTCARDSIZE, 2, 2, 0);
        public override Image Front
        {
            get { return FRONT; }
        }

        public override bool IsFascist
        {
            get { return false; }
        }
    }
    class CardMembershipFascist : CardMembership
    {
        private static Image FRONT = Properties.Resources.membership_cards.FromSprite(DEFAULTCARDSIZE, 2, 2, 1);
        public override Image Front
        {
            get { return FRONT; }
        }

        public override bool IsFascist
        {
            get { return true; }
        }
    }
}
