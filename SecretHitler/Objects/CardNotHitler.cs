using SecretHitler.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Objects
{
    class CardNotHitler : Card
    {
        private static Image BACK = Properties.Resources.Not_Hitler_confirmed_card_backcover.CutToSize(DEFAULTCARDSIZE);
        private static Image FRONT = Properties.Resources.Not_Hitler_confirmed_cards.FromSprite(DEFAULTCARDSIZE, 2, 2, 0);
        public override Image Back
        {
            get { return BACK; }
        }

        public override Image Front
        {
            get { return FRONT; }
        }
    }
}
