using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Straddle
{
    class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {           
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            _form = new AppMain();
            _form.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            _form.Location = new System.Drawing.Point(0, 0);
            Application.Run(_form);       
        }
        public static AppMain _form;
    }
}
