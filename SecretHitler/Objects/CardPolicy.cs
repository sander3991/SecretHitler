using SecretHitler.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Objects
{
    abstract class CardPolicy : Card
    {
        private static Bitmap BACK = Properties.Resources.policy_cards_backcover.CutToSize(DEFAULTCARDSIZE);
        public abstract bool IsFascist { get; }
        public override Bitmap Back
        {
            get { return BACK; }
        }
    }
    class CardPolicyFascist : CardPolicy
    {
        private static Bitmap FRONT = Properties.Resources.policy_cards_high_contrast.FromSprite(DEFAULTCARDSIZE, 2, 2, 1);
        public override Bitmap Front
        {
            get
            {
                return FRONT;
            }
        }

        public override bool IsFascist
        {
            get { return true; }
        }
    }
    class CardPolicyLiberal : CardPolicy
    {
        private static Bitmap FRONT = Properties.Resources.policy_cards_high_contrast.FromSprite(DEFAULTCARDSIZE, 2, 2, 0);
        public override Bitmap Front
        {
            get
            {
                return FRONT;
            }
        }

        public override bool IsFascist
        {
            get { return false; }
        }
    }
}
