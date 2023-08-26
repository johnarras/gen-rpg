using System;
using System.Windows.Forms;
using System.Threading;
using GameEditor;

namespace Genrpg.Editor
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Application.ThreadException += new ThreadExceptionEventHandler(new ThreadExceptionHandler().ApplicationThreadException);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
            catch (Exception e)
            {
                Console.WriteLine("EXC: " + e.Message);
            }
        }
    }

    public class ThreadExceptionHandler

    {

        public void ApplicationThreadException(object sender, ThreadExceptionEventArgs e)

        {

            MessageBox.Show(e.Exception.Message + " " + e.Exception.StackTrace);
        }

    }

}
