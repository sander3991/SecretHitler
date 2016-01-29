using SecretHitler.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecretHitler
{
    static class SecretHitlerGame
    {
        internal const int DEFAULTPORT = 23451;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var connectDialog = new ServerClientDialog();
            Application.Run(connectDialog);
            Application.Run(new Game(connectDialog));
        }
    }
}
