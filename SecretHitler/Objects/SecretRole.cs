using SecretHitler.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Objects
{
    abstract class SecretRole : Card
    {
        private static Image BACK = Properties.Resources.role_cards_backcover.CutToSize(DEFAULTCARDSIZE);
        public abstract bool IsFascist { get; }
        public abstract bool IsHitler { get; }
        public override Image Back
        {
            get { return BACK; }
        }
    }
    class SecretRoleFascist : SecretRole
    {
        private int ID = -1;
        private static Image[] FRONTS;
        static SecretRoleFascist()
        {
            var bmp = Properties.Resources.role_cards;
            int spriteWidth = 4, spriteHeight = 3;
            FRONTS = new Image[]
            {
                bmp.FromSprite(DEFAULTCARDSIZE, spriteWidth, spriteHeight, 0),
                bmp.FromSprite(DEFAULTCARDSIZE, spriteWidth, spriteHeight, 1),
                bmp.FromSprite(DEFAULTCARDSIZE, spriteWidth, spriteHeight, 2),
                bmp.FromSprite(DEFAULTCARDSIZE, spriteWidth, spriteHeight, 3)
            };
        }
        public override Image Front
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

        public SecretRoleFascist(int id)
        {
            if (id < 0 || id >= FRONTS.Length)
                throw new ArgumentException("Invalid ID given to fascist role.", nameof(id));
            ID = id;
        }
    }
    class SecretRoleLiberal : SecretRole
    {
        private int ID = -1;
        private static Image[] FRONTS;
        static SecretRoleLiberal()
        {
            var bmp = Properties.Resources.role_cards;
            int spriteWidth = 4, spriteHeight = 3;
            FRONTS = new Image[]
            {
                bmp.FromSprite(DEFAULTCARDSIZE, spriteWidth, spriteHeight, 4),
                bmp.FromSprite(DEFAULTCARDSIZE, spriteWidth, spriteHeight, 5),
                bmp.FromSprite(DEFAULTCARDSIZE, spriteWidth, spriteHeight, 6),
                bmp.FromSprite(DEFAULTCARDSIZE, spriteWidth, spriteHeight, 7),
                bmp.FromSprite(DEFAULTCARDSIZE, spriteWidth, spriteHeight, 8),
                bmp.FromSprite(DEFAULTCARDSIZE, spriteWidth, spriteHeight, 9)
            };
        }
        public override Image Front
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

        public SecretRoleLiberal(int id)
        {
            if (id < 0 || id >= FRONTS.Length)
                throw new ArgumentException("Invalid ID given to fascist role.", nameof(id));
            ID = id;
        }
    }
}
