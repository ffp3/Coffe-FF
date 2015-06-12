using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace Milk
{
    public partial class mfMilk : Form
    {
        public mfMilk()
        {
            InitializeComponent();

        
        }


        private void doPowercfg()
        {

              Process powercfg = new Process();
            //   powercfg.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            powercfg.StartInfo.CreateNoWindow = true;
            powercfg.StartInfo.FileName = "powercfg.exe";
            powercfg.StartInfo.Arguments = "-requests";
            powercfg.StartInfo.UseShellExecute = false;
            powercfg.StartInfo.RedirectStandardOutput = true;
            powercfg.StartInfo.RedirectStandardError = true;

            powercfg.Start();
            powercfg.WaitForExit();

            string error = powercfg.StandardError.ReadToEnd();

            if (error == "")
            {

                textBox1.Text = powercfg.StandardOutput.ReadToEnd();
                textBox1.Select(textBox1.Text.Length, textBox1.Text.Length);
                toolStripStatusLabel1.Text = "Last Updated: " + System.DateTime.Now.ToLongTimeString() + " - F5 to Refresh";
            }
            else
            { 
                // has to be a messagebox or the user wont see it
                MessageBox.Show(error);
                Close();
            }

        }


        private void Form1_Load(object sender, EventArgs e)
        {
            
            this.Top = Screen.PrimaryScreen.Bounds.Height / 2 - 135;
            this.Left = Screen.PrimaryScreen.Bounds.Width / 2 + 180;

            doPowercfg();
        
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
           
              if (e.KeyData.ToString() == "F5" )
            {
                doPowercfg();
            }
        }


      
     
    }
}
