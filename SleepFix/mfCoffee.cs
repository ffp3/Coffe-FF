using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security.Principal;

using System.Threading;
//for COM add-in
using System.Reflection;



namespace SleepFix
{
        public partial class Form1 : Form
    {


        public Form1()
        {
            InitializeComponent();
            

            if (!IsAdmin())
            {
                AddShieldToButton(button1); //Important
                AddShieldToButton(button3);
            }
        }


        Delay d = new Delay();
        ////
        // Test and Understand how DateTime works
        ////
        DateTime compareDate = DateTime.Now; 
        DateTime ClearStopDelay = DateTime.Now;
        //powercfg -requests

        
        List<NetworkInterface> Interfaces = new List<NetworkInterface>();
            
        long lngBytesSend = 0;
        long lngBtyesReceived = 0;
        bool firsttick = true;
        bool menuexit = false;

        [DllImport("user32")]
        public static extern UInt32 SendMessage
            (IntPtr hWnd, UInt32 msg, UInt32 wParam, UInt32 lParam);

        internal const int BCM_FIRST = 0x1600; //Normal button

        internal const int BCM_SETSHIELD = (BCM_FIRST + 0x000C); //Elevated button

        static internal bool IsAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal p = new WindowsPrincipal(id);
            return p.IsInRole(WindowsBuiltInRole.Administrator);
        }

        static internal void AddShieldToButton(Button b)
        {
            b.FlatStyle = FlatStyle.System;
            SendMessage(b.Handle, BCM_SETSHIELD, 0, 0xFFFFFFFF);
        }

        #region systemdllcrap

        //
        // Availability Request Functions
        //
        [DllImport("kernel32.dll")]
        static extern IntPtr PowerCreateRequest(
            ref POWER_REQUEST_CONTEXT Context
            );

        [DllImport("kernel32.dll")]
        static extern bool PowerSetRequest(
            IntPtr PowerRequestHandle,
            PowerRequestType RequestType
            );

        [DllImport("kernel32.dll")]
        static extern bool PowerClearRequest(
            IntPtr PowerRequestHandle,
            PowerRequestType RequestType
            );

        //
        // Availablity Request Enumerations and Constants
        //
        enum PowerRequestType
        {
            PowerRequestDisplayRequired = 0,
            PowerRequestSystemRequired,
            PowerRequestAwayModeRequired,
            PowerRequestMaximum
        }

        const int POWER_REQUEST_CONTEXT_VERSION = 0;
        const int POWER_REQUEST_CONTEXT_SIMPLE_STRING = 0x1;
        const int POWER_REQUEST_CONTEXT_DETAILED_STRING = 0x2;

        //
        // Availablity Request Structures
        //

        //
        // Note:
        //
        // Windows defines the POWER_REQUEST_CONTEXT structure with an
        // internal union of SimpleReasonString and Detailed information.
        // To avoid runtime interop issues, this version of 
        // POWER_REQUEST_CONTEXT only supports SimpleReasonString.  
        // To use the detailed information,
        // define the PowerCreateRequest function with the first 
        // parameter of type POWER_REQUEST_CONTEXT_DETAILED.
        //
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct POWER_REQUEST_CONTEXT
        {
            public UInt32 Version;
            public UInt32 Flags;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string
                SimpleReasonString;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PowerRequestContextDetailedInformation
        {
            public IntPtr LocalizedReasonModule;
            public UInt32 LocalizedReasonId;
            public UInt32 ReasonStringCount;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string[] ReasonStrings;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct POWER_REQUEST_CONTEXT_DETAILED
        {
            public UInt32 Version;
            public UInt32 Flags;
            public PowerRequestContextDetailedInformation DetailedInformation;
        }


        //
        // Create a system availablity request to keep the system from
        // automatically sleeping while downloading a file.
        //
        POWER_REQUEST_CONTEXT DownloadPowerRequestContext;
        POWER_REQUEST_CONTEXT UploadPowerRequestContext;
        POWER_REQUEST_CONTEXT ProcessPowerRequestContext;
        IntPtr DownloadPowerRequest;
        IntPtr UploadPowerRequest;
        IntPtr ProcessPowerRequest;
        bool DownloadPrevented = false;
        bool UploadPrevented = false;
        bool ProcessPrevented = false;
  
        #endregion


        #region miscfunctions

        private void SystemRequired(string Reason)
        {
            
            if (Reason == "Download")
            {
                if (DownloadPowerRequest.ToInt32() == 0)  //first time setup the power request
                {
                    //
                    // Set up the diagnostic string
                    //
                    DownloadPowerRequestContext.Version = POWER_REQUEST_CONTEXT_VERSION;
                    DownloadPowerRequestContext.Flags = POWER_REQUEST_CONTEXT_SIMPLE_STRING;
                    DownloadPowerRequestContext.SimpleReasonString = Reason + " trafic above threshold.";



                    //
                    // Create the request, get a handle
                    //
                    DownloadPowerRequest = PowerCreateRequest(ref DownloadPowerRequestContext);
                }


                if (DownloadPrevented == false)  //this stops it running when it is aready done
                {
                    
                    //
                    // Set a system request to prevent automatic sleep
                    //
                    PowerSetRequest(DownloadPowerRequest, PowerRequestType.PowerRequestSystemRequired);
                    if (checkBox2.Checked)
                    {
                        PowerSetRequest(DownloadPowerRequest, PowerRequestType.PowerRequestDisplayRequired);
                    }
                    DownloadPrevented = true;
                    // 
                    // Download the file...
                    //
                }
            }

            if (Reason == "Upload")
            {
                if (UploadPowerRequest.ToInt32() == 0)  //first time setup the power request
                {
                    //
                    // Set up the diagnostic string
                    //
                    UploadPowerRequestContext.Version = POWER_REQUEST_CONTEXT_VERSION;
                    UploadPowerRequestContext.Flags = POWER_REQUEST_CONTEXT_SIMPLE_STRING;
                    UploadPowerRequestContext.SimpleReasonString = Reason + " trafic above threshold.";

                    //
                    // Create the request, get a handle
                    //
                    UploadPowerRequest = PowerCreateRequest(ref UploadPowerRequestContext);
                }


                if (UploadPrevented == false)  //this stops it running when it is aready done
                {
                    //
                    // Set a system request to prevent automatic sleep
                    //
                    PowerSetRequest(UploadPowerRequest, PowerRequestType.PowerRequestSystemRequired);
                    if (checkBox2.Checked)
                    {
                        PowerSetRequest(UploadPowerRequest, PowerRequestType.PowerRequestDisplayRequired);
                    }
                    UploadPrevented = true;
                    // 
                    // Download the file...
                    //
                }
            }


            if (Reason == "Process")
            {
                if (ProcessPowerRequest.ToInt32() == 0)  //first time setup the power request
                {
                    //
                    // Set up the diagnostic string
                    //
                    ProcessPowerRequestContext.Version = POWER_REQUEST_CONTEXT_VERSION;
                    ProcessPowerRequestContext.Flags = POWER_REQUEST_CONTEXT_SIMPLE_STRING;
                    ProcessPowerRequestContext.SimpleReasonString = Reason + " an application that is ticked in coffee is open.";

                    //
                    // Create the request, get a handle
                    //
                    ProcessPowerRequest = PowerCreateRequest(ref ProcessPowerRequestContext);
                }


                if (ProcessPrevented == false)  //this stops it running when it is aready done
                {
                    //
                    // Set a system request to prevent automatic sleep
                    //
                    PowerSetRequest(ProcessPowerRequest, PowerRequestType.PowerRequestSystemRequired);
                    if (checkBox2.Checked)
                    {
                        PowerSetRequest(ProcessPowerRequest, PowerRequestType.PowerRequestDisplayRequired);
                    }
                    ProcessPrevented = true;
                    // 
                    // Download the file...
                    //
                }
            }

        }

        private void SystemNotRequired(string Reason)
        {
            
            if (Reason == "Download")
            {
                if (DownloadPrevented == true) //this stops it running when it is already cleared
                {
                    
                    //
                    // Clear the request
                    //
                    PowerClearRequest(DownloadPowerRequest, PowerRequestType.PowerRequestSystemRequired);
                    PowerClearRequest(DownloadPowerRequest, PowerRequestType.PowerRequestDisplayRequired);
                    DownloadPrevented = false;

                }
            }

            if (Reason == "Upload")
            {
                if (UploadPrevented == true) //this stops it running when it is already cleared
                {
                    //
                    // Clear the request
                    //
                    PowerClearRequest(UploadPowerRequest, PowerRequestType.PowerRequestSystemRequired);
                    PowerClearRequest(UploadPowerRequest, PowerRequestType.PowerRequestDisplayRequired);
                    UploadPrevented = false;

                }

            }

            if (Reason == "Process")
            {
                if (ProcessPrevented == true) //this stops it running when it is already cleared
                {
                    //
                    // Clear the request
                    //
                    PowerClearRequest(ProcessPowerRequest, PowerRequestType.PowerRequestSystemRequired);
                    PowerClearRequest(ProcessPowerRequest, PowerRequestType.PowerRequestDisplayRequired);
                    ProcessPrevented = false;

                }

            }

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            menuexit = true;
            Close();
        }

         // preventing program from exit
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {


            if (!menuexit && e.CloseReason != CloseReason.WindowsShutDown) // If system shuting down will close and save
            {
               
                notifyIcon1.Visible = true;  //dont actualy exit the program
                this.Hide();


                this.WindowState = FormWindowState.Minimized;

                notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
                notifyIcon1.BalloonTipTitle = "Attention";
                notifyIcon1.BalloonTipText = "Coffee_FF minimized to system tray." +
                            Environment.NewLine +
                            "Right-click on the icon for more options.";

                notifyIcon1.ShowBalloonTip(5000);


                e.Cancel = true;
            }
            else
            {

                try
                {
                 //   Coffee_FF.Properties.Settings.Default.NetworkAdaptor = comboBox1.SelectedItem.ToString();// = comboBox1.Items.IndexOf();
                 //   Coffee_FF.Properties.Settings.Default.DownloadThreshold = (int)numericUpDown1.Value;
                 //   Coffee_FF.Properties.Settings.Default.UploadThreshold = (int)numericUpDown2.Value;
                 //   Coffee_FF.Properties.Settings.Default.DelayRemoveSleep = (int)numericUpDown3.Value;
                 //   Coffee_FF.Properties.Settings.Default.PressKeyInMinutes = (int)numericUpDown4.Value;  // Send virtual key press every X minutes
                 //   Coffee_FF.Properties.Settings.Default.EnDisKeyPress = checkBox1.Checked;
                 //   Coffee_FF.Properties.Settings.Default.DisDisplayStandby = checkBox1.Checked;

                 //   Coffee_FF.Properties.Settings.Default.Save();
                      SaveState();
                }
                catch { }

                Environment.Exit(0);
                
            }

        }
       

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
            notifyIcon1.Visible = false;
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {

            this.Show();
            notifyIcon1.Visible = false;

        }

        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {

            foreach (var process in Process.GetProcesses())
            {
                clbProcess.Items.Add(process.ProcessName);

            }


            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
               // if (nic.OperationalStatus == OperationalStatus.Up)
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    if (!nic.Description.StartsWith("Software"))
                    {
                        if (!nic.Description.StartsWith("Teredo"))
                        {
                            comboBox1.Items.Add(nic.Description);
                            comboBox1.SelectedIndex = 0;
                            Interfaces.Add(nic);
                        }
                    }
                }
            }

            if (comboBox1.SelectedIndex < 0)
            {
                timer1.Enabled = false;
                MessageBox.Show("No active network connections detected, the program will now close");
                menuexit = true;
                //System.Threading.Thread.Sleep(1000);
                Close();
            }
            else
            {

                if (!(string.IsNullOrEmpty(Coffee_FF.Properties.Settings.Default.NetworkAdaptor)))
                    comboBox1.SelectedIndex = comboBox1.Items.IndexOf(Coffee_FF.Properties.Settings.Default.NetworkAdaptor);


                if (comboBox1.SelectedIndex < 0)
                {
                    comboBox1.SelectedIndex = 0;
                    MessageBox.Show("Error: " + Coffee_FF.Properties.Settings.Default.NetworkAdaptor + " not found.");
                }

                numericUpDown1.Value = Coffee_FF.Properties.Settings.Default.DownloadThreshold;
                numericUpDown2.Value = Coffee_FF.Properties.Settings.Default.UploadThreshold;
                numericUpDown3.Value = Coffee_FF.Properties.Settings.Default.DelayRemoveSleep; // Delay before remove sleep blocker
                numericUpDown4.Value = Coffee_FF.Properties.Settings.Default.PressKeyInMinutes; // Send virtual key press every X minutes
                checkBox1.Checked = Coffee_FF.Properties.Settings.Default.EnDisKeyPress; // Sending virutal key press enabeld when checked
                checkBox2.Checked = Coffee_FF.Properties.Settings.Default.DisDisplayStandby; // Keep Display Awake if checked
            }

    
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            IPv4InterfaceStatistics  interfaceStatistic = Interfaces[comboBox1.SelectedIndex].GetIPv4Statistics();
            

            int bytesSentSpeed = (int)(interfaceStatistic.BytesSent - lngBytesSend) / 1024;
            int bytesReceivedSpeed = (int)(interfaceStatistic.BytesReceived - lngBtyesReceived) / 1024;

            if (firsttick)
            {
                string[] args = Environment.GetCommandLineArgs();  //dirty but it works

                foreach (string arg in args)
                {

                    if (arg == "/hide")  
                    {
                        Close();
                    }
                }                                                  // above code hides the window if run with /hide command

                firsttick = false;
            }
            else
            {
                lblUpload.Text = "Upload: " + bytesSentSpeed.ToString() + " KB/s";
                lblDownLoad.Text = "Download: " + bytesReceivedSpeed.ToString() + " KB/s";

            }
            

            int CardIndx2 = 0;
                foreach (var nic2 in NetworkInterface.GetAllNetworkInterfaces())
                {

                    if (nic2.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || nic2.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    {
                        if (!nic2.Description.StartsWith("Software"))
                        {
                            if (!nic2.Description.StartsWith("Teredo"))
                            {
                                if (comboBox1.SelectedIndex == CardIndx2)
                                { 
                                    lblOP.Text = "Adapter Operational status:" + nic2.OperationalStatus;
                                }
                                CardIndx2++;
                            }
                        }
                    }
                }
             
            // Display when Delay will be removed
            if (DateTime.Now > ClearStopDelay) 
                {
                    Delay.Text = "Delay removed \n";
                }
            else
                {
                    Delay.Text = "Delay will be removed \n on: " + ClearStopDelay;
                }

            if (bytesReceivedSpeed > numericUpDown1.Value)
            {
                if (numericUpDown1.Value != 0)
                {
                    SystemRequired("Download");
                    ClearStopDelay = DateTime.Now.AddMinutes((int)numericUpDown3.Value); // Reset Delay every time Download detected
                    // Console.WriteLine(ClearStopDelay + "Set Delay Timer");

                    ////
                    // Test and Understand how DateTime works (Make sure 5 minutes pass or more before reset idle key press)
                    ////
                    //Console.WriteLine("Download reset Timer Before 1 minutes");
                    if (DateTime.Now > compareDate && checkBox1.Checked)
                    {
                        
                        compareDate = DateTime.Now.AddMinutes((int)numericUpDown4.Value); // Need change to 5 minutes
                        // Console.WriteLine(compareDate + "Download reset Timer");

                        d.delay();
                    }
                    
                }
            }
            else
            {
                if (DateTime.Now > ClearStopDelay) //Delay for X minutes before clear sleep block
                {
                    SystemNotRequired("Download");
                    //Console.WriteLine(DateTime.Now + " > " + ClearStopDelay + "Delay Timer Removed");
                }
            }

            
            if (bytesSentSpeed > numericUpDown2.Value)
            {
                if (numericUpDown2.Value != 0)
                {
                    SystemRequired("Upload");
                    ClearStopDelay = DateTime.Now.AddMinutes((int)numericUpDown3.Value); // Reset Delay every time Upload detected
                    // Console.WriteLine(ClearStopDelay + "Set Delay Timer");

                    ////
                    // Test and Understand how DateTime works (Make sure 5 minutes pass or more before reset idle key press)
                    ////
                    if (DateTime.Now > compareDate && checkBox1.Checked)
                    {
                        
                        compareDate = DateTime.Now.AddMinutes((int)numericUpDown4.Value);
                        // Console.WriteLine(compareDate + "Upload reset Timer");

                        d.delay();
                    }

                }
            }
            else
            {
                if (DateTime.Now > ClearStopDelay) //Delay for X minutes before clear sleep block
                {
                    SystemNotRequired("Upload");
                    // Console.WriteLine(DateTime.Now + " > " + ClearStopDelay + "Delay Timer Removed");
                }
            }

            lngBytesSend = interfaceStatistic.BytesSent;
            lngBtyesReceived = interfaceStatistic.BytesReceived;


            bool systemRequired = false;

            foreach (var process in Process.GetProcesses())
            {
                foreach (string processList in clbProcess.CheckedItems)
                {
                    if (processList == process.ProcessName)
                    {
                        systemRequired = true;
                    }
                }
            }

            if (systemRequired)
            {
                SystemRequired("Process");
                ClearStopDelay = DateTime.Now.AddMinutes((int)numericUpDown3.Value); // Reset Delay every time Process detected
                //Console.WriteLine(ClearStopDelay + "Set Delay Timer");

                ////
                // Test and Understand how DateTime works (Make sure 5 minutes pass or more before reset idle key press)
                ////
                if (DateTime.Now > compareDate && checkBox1.Checked)
                {
                    compareDate = DateTime.Now.AddMinutes((int)numericUpDown4.Value);
                    // Console.WriteLine(compareDate + "Process reset Timer!!! Virtual Key Pressed");

                    d.delay();
                }
                
            }
            else
            {
                if (DateTime.Now > ClearStopDelay) //Delay for X minutes before clear sleep block
                {
                    SystemNotRequired("Process");
                    // Console.WriteLine(DateTime.Now + " > " + ClearStopDelay + "Delay Timer Removed");
                }
            }

        }

     
        private void button1_Click(object sender, EventArgs e)
        {
            if (IsAdmin())
            {
                Process Milk = new Process();
                Milk.StartInfo.FileName = Application.StartupPath + @"\Milk.exe";
                Milk.Start();
            }
            else
            {
                try
                {
                    RestartMilkElevated();
                }
                catch
                {
                }
            }

        }

        internal static void RestartMilkElevated()
        {
            
            Process Milk = new Process();
            Milk.StartInfo.FileName = Application.StartupPath + @"\Milk.exe";
            Milk.StartInfo.Verb = "runas";
            Milk.Start();

        }

     
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }

        }

        private void numericUpDown1_Leave(object sender, EventArgs e)
        {
            if (numericUpDown1.Text == "")
            {
                numericUpDown1.Text = (10).ToString();
                numericUpDown1.Value = 10;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (IsAdmin())
            {
                Process Milk = new Process();
                Milk.StartInfo.FileName = Application.StartupPath + @"\Sugar.exe";
                Milk.Start();
            }
            else
            {
                try
                {
                    RestartSugarElevated();
                }
                catch
                {
                }
            }
        }

        internal static void RestartSugarElevated()
        {
            Process Milk = new Process();
            Milk.StartInfo.FileName = Application.StartupPath + @"\Sugar.exe";
            Milk.Start();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            string path = @"http://sourceforge.net/project/project_donations.php?group_id=540532";

            Process p = new Process();
            p.StartInfo.FileName = path;
            p.Start();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string path = @"http://coffeeff.sourceforge.net/";

            Process p = new Process();
            p.StartInfo.FileName = path;
            p.Start();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SaveState();

            Coffee_FF.Properties.Settings.Default.Save();
            Environment.Exit(0);
            // Application.Exit();
        }

        public void SaveState()
        {
            try
            {
                Coffee_FF.Properties.Settings.Default.NetworkAdaptor = comboBox1.SelectedItem.ToString();// = comboBox1.Items.IndexOf();
                Coffee_FF.Properties.Settings.Default.DownloadThreshold = (int)numericUpDown1.Value;
                Coffee_FF.Properties.Settings.Default.UploadThreshold = (int)numericUpDown2.Value;
                Coffee_FF.Properties.Settings.Default.DelayRemoveSleep = (int)numericUpDown3.Value;
                Coffee_FF.Properties.Settings.Default.PressKeyInMinutes = (int)numericUpDown4.Value;  // Send virtual key press every X minutes
                Coffee_FF.Properties.Settings.Default.EnDisKeyPress = checkBox1.Checked;
                Coffee_FF.Properties.Settings.Default.DisDisplayStandby = checkBox2.Checked;
                Coffee_FF.Properties.Settings.Default.Save();
            }
            catch { }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            // When checked/unchecked clear Downoad, Upload and Process...
            DownloadPrevented = true;
            UploadPrevented = true;
            ProcessPrevented = true;
            SystemNotRequired("Download");
            SystemNotRequired("Upload");
            SystemNotRequired("Process");
            ClearStopDelay = DateTime.Now;
            Console.WriteLine(DateTime.Now + "Cleared on Display (Downoad, Upload and Process) Check/UNCheck");
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            DownloadPrevented = true;
            UploadPrevented = true;
            ProcessPrevented = true;
            SystemNotRequired("Download");
            SystemNotRequired("Upload");
            SystemNotRequired("Process");
            ClearStopDelay = DateTime.Now;
            Console.WriteLine(DateTime.Now + "Cleared on Virtual Key Press (Downoad, Upload and Process) Check/UNCheck");
        }       
 
    }

    //here's the heart of it (Delay IDLE timer by simulating key press)
    class Delay
    {
        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
        [DllImport("user32.dll")]
        public static extern void keybd_event(
        byte bVk,
        byte bScan,
        uint dwFlags,
        uint dwExtraInfo
        );

        const int VK_CONTROL = 0x11;
        const uint KEYEVENTF_KEYUP = 0x2;
        bool ctrlPressed = false;

        public Delay()
        {

        }
        //we're actually triggering a keyboard event.
        public void delay()
        {
            keybd_event((byte)VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);
        }
        //you can access the timer used to monitor input events.  This timer gets reset when an event is triggered and delays things like locking of the screen.
        public int GetLastInputTime()
        {
            int idleTime = 0;
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = Marshal.SizeOf(lastInputInfo);
            lastInputInfo.dwTime = 0;

            int envTicks = Environment.TickCount;

            if (GetLastInputInfo(ref lastInputInfo))
            {
                int lastInputTick = lastInputInfo.dwTime;
                idleTime = envTicks - lastInputTick;
            }
            return ((idleTime > 0) ? (idleTime / 1000) : 0);
        }
    }



}

// Added (Simulate key press)
[StructLayout(LayoutKind.Sequential)]
struct LASTINPUTINFO
{
    public static readonly int SizeOf = Marshal.SizeOf(typeof(LASTINPUTINFO));

    [MarshalAs(UnmanagedType.U4)]
    public int cbSize;
    [MarshalAs(UnmanagedType.U4)]
    //public UInt32 dwTime;
    public int dwTime;
}