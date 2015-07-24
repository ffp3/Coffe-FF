using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Sugar
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {


            // The path to the key where Windows looks for startup applications
            RegistryKey rkApp = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            // Add the value in the registry so that the application runs at startup

             // Check to see the current state (running at startup or not)

            if (rkApp.GetValue("Coffee") == null)
            {

                // The value doesn't exist, the application is not set to run at startup

                rkApp.SetValue("Coffee", Application.StartupPath + @"\Coffee_FF.exe /hide");
                label1.Text = "Coffee WILL start with windows";

            }

            else
            {

                // The value exists, the application is set to run at startup

                rkApp.DeleteValue("Coffee");
                label1.Text = "Coffee will NOT start with windows";

            }


        }
    }
}
