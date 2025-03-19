using System;
using System.Windows.Forms;

namespace EBScan
{
    static class Program
    {

        public static MainForm mainForm;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Start the program without opening the form by default.
            mainForm = new MainForm();
            Application.Run();
        }
    }
}
