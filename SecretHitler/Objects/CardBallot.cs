using SecretHitler.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Objects
{
    public abstract class CardBallot : Card
    {
        private static Bitmap BACK = Properties.Resources.ballot_card_backcover.CutToSize(DEFAULTCARDSIZE);
        public sealed override Bitmap Back
        {
            get { return BACK; }
        }
        public abstract bool Yes { get; }
    }
    public class CardBallotYes : CardBallot
    {
        private static Bitmap FRONT = Properties.Resources.ballot_cards.FromSprite(DEFAULTCARDSIZE, 2, 2, 0);
        public override Bitmap Front
        {
            get { return FRONT; }
        }

        public override bool Yes
        {
            get { return true; }
        }
    }
    public class CardBallotNo : CardBallot
    {
        private static Bitmap FRONT = Properties.Resources.ballot_cards.FromSprite(DEFAULTCARDSIZE, 2, 2, 1);
        public override Bitmap Front
        {
            get { return FRONT; }
        }

        public override bool Yes
        {
            get { return false; }
        }
    }
}
