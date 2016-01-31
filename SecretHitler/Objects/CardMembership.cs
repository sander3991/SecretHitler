using SecretHitler.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Objects
{
    public abstract class CardMembership : Card
    {
        private static Bitmap BACK = Properties.Resources.membership_card_backcover.CutToSize(DEFAULTCARDSIZE);
        public abstract bool IsFascist { get; }
        public override Bitmap Back
        {
            get
            {
                return BACK;
            }
        }
    }
    public class CardMembershipLiberal : CardMembership
    {
        private static Bitmap FRONT = Properties.Resources.membership_cards.FromSprite(DEFAULTCARDSIZE, 2, 2, 0);
        public override Bitmap Front
        {
            get { return FRONT; }
        }

        public override bool IsFascist
        {
            get { return false; }
        }
    }
    public class CardMembershipFascist : CardMembership
    {
        private static Bitmap FRONT = Properties.Resources.membership_cards.FromSprite(DEFAULTCARDSIZE, 2, 2, 1);
        public override Bitmap Front
        {
            get { return FRONT; }
        }

        public override bool IsFascist
        {
            get { return true; }
        }
    }
    public class CardMembershipUnknown : CardMembership
    {
        private static Bitmap FRONT = Properties.Resources.membership_cards.FromSprite(DEFAULTCARDSIZE, 2, 2, 3);
        public override Bitmap Front { get { return FRONT; } }
        public override bool IsFascist { get { return false; } }
    }
}
