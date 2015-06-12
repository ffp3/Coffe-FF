using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace SleepFix
{
    static class Program
    {

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {


          bool createdNew = true;
          using (Mutex mutex = new Mutex(true, "Coffee_SC", out createdNew))

     // Test and Understand how DateTime works (Make sure 5 minutes pass or more before reset idle key press)

    
    // DateTime FirstDate = DateTime.Now;
    // DateTime compare_Date = DateTime.Today.AddMinutes(5);
    // Console.WriteLine(system_Date);
    // Console.WriteLine(compare_Date);

          {
              if (createdNew)
              {
                  Application.EnableVisualStyles();
                  Application.SetCompatibleTextRenderingDefault(false);
                  Application.Run(new Form1());
              }
              else
              {
                  MessageBox.Show("Coffee is already running, check the notification area.","Already Running", MessageBoxButtons.OK, MessageBoxIcon.Warning);

              }
          }


            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
        }
    }
}
