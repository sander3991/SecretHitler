using SecretHitler.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Objects
{
    public abstract class CardSecretRole : Card
    {
        public abstract int ID { get; protected set; }
        private static Bitmap BACK = Properties.Resources.role_cards_backcover.CutToSize(DEFAULTCARDSIZE);
        public abstract bool IsFascist { get; }
        public abstract bool IsHitler { get; }
        public override Bitmap Back
        {
            get { return BACK; }
        }
    }
    public class CardSecretRoleFascist : CardSecretRole
    {
        public override int ID { get; protected set; } = -1;
        private static Bitmap[] FRONTS;
        static CardSecretRoleFascist()
        {
            var bmp = Properties.Resources.role_cards;
            int spriteWidth = 4, spriteHeight = 3;
            FRONTS = new Bitmap[]
            {
                bmp.FromSprite(DEFAULTCARDSIZE, spriteWidth, spriteHeight, 0),
                bmp.FromSprite(DEFAULTCARDSIZE, spriteWidth, spriteHeight, 1),
                bmp.FromSprite(DEFAULTCARDSIZE, spriteWidth, spriteHeight, 2),
                bmp.FromSprite(DEFAULTCARDSIZE, spriteWidth, spriteHeight, 3)
            };
        }
        public override Bitmap Front
        {
            get
            {
                return FRONTS[ID];
            }
        }

        public override bool IsFascist
        {
            get { return true; }
        }

        public override bool IsHitler
        {
            get
            {
                return ID == 0;
            }
        }

        public CardSecretRoleFascist(int id)
        {
            if (id < 0 || id >= FRONTS.Length)
                throw new ArgumentException("Invalid ID given to fascist role.", nameof(id));
            ID = id;
        }
    }
    public class CardSecretRoleLiberal : CardSecretRole
    {
        public override int ID { get; protected set; } = -1;
        private static Bitmap[] FRONTS;
        static CardSecretRoleLiberal()
        {
            var bmp = Properties.Resources.role_cards;
            int spriteWidth = 4, spriteHeight = 3;
            FRONTS = new Bitmap[]
            {
                bmp.FromSprite(DEFAULTCARDSIZE, spriteWidth, spriteHeight, 4),
                bmp.FromSprite(DEFAULTCARDSIZE, spriteWidth, spriteHeight, 5),
                bmp.FromSprite(DEFAULTCARDSIZE, spriteWidth, spriteHeight, 6),
                bmp.FromSprite(DEFAULTCARDSIZE, spriteWidth, spriteHeight, 7),
                bmp.FromSprite(DEFAULTCARDSIZE, spriteWidth, spriteHeight, 8),
                bmp.FromSprite(DEFAULTCARDSIZE, spriteWidth, spriteHeight, 9)
            };
        }
        public override Bitmap Front
        {
            get { return FRONTS[ID]; }
        }

        public override bool IsFascist
        {
            get { return false; }
        }

        public override bool IsHitler
        {
            get { return false; }
        }

        public CardSecretRoleLiberal(int id)
        {
            if (id < 0 || id >= FRONTS.Length)
                throw new ArgumentException("Invalid ID given to liberal role.", nameof(id));
            ID = id;
        }
    }

    public class CardSecretRoleUnknown : CardSecretRole
    {
        private static Bitmap FRONT = Properties.Resources.role_cards.FromSprite(DEFAULTCARDSIZE, 4, 3, 11);
        public override Bitmap Front { get { return FRONT; } }
        public override int ID { get { return -1; } protected set { } }
        public override bool IsFascist { get { return false; } }
        public override bool IsHitler { get { return false; } }
    }
}
