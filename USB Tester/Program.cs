using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace USB_Tester
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        [STAThread]
        static void Main()
        {
            Logger.Info("Application started.", "Main");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }

    public static class Logger
    {
        public static void Error(string message, string module)
        {
            WriteEntry(message, "error", module);
        }

        public static void Error(Exception ex, string module)
        {
            WriteEntry(ex.Message, "error", module);
        }

        public static void Warning(string message, string module)
        {
            WriteEntry(message, "warning", module);
        }

        public static void Info(string message, string module)
        {
            WriteEntry(message, "info", module);
        }

        private static void WriteEntry(string message, string type, string module)
        {
            
            Trace.WriteLine(
                    string.Format("{0},{1},{2},{3}",
                                  DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                                  type,
                                  module,
                                  message));
        }
    }

}
