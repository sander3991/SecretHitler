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
        private static Image BACK = Properties.Resources.ballot_card_backcover.CutToSize(DEFAULTCARDSIZE);
        public sealed override Image Back
        {
            get { return BACK; }
        }
        public abstract bool Yes { get; }
    }
    public class BallotCardYes : CardBallot
    {
        private static Image FRONT = Properties.Resources.ballot_cards.FromSprite(DEFAULTCARDSIZE, 2, 2, 0);
        public override Image Front
        {
            get { return FRONT; }
        }

        public override bool Yes
        {
            get { return true; }
        }
    }
    public class BallotCardNo : CardBallot
    {
        private static Image FRONT = Properties.Resources.ballot_cards.FromSprite(DEFAULTCARDSIZE, 2, 2, 1);
        public override Image Front
        {
            get { return FRONT; }
        }

        public override bool Yes
        {
            get { return false; }
        }
    }
}
