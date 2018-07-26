using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Configuration;
using System.Collections.Specialized;
namespace appTimer2
{

    public partial class InvisibleForm : Form
    {
        NotifyIcon timer;
        Icon timerIcon;
        Thread timerWorker;

        Stopwatch stopwatch = new Stopwatch();
        
        string format = "MM/dd/yyyy HH:mm";

        protected override void OnLoad(EventArgs e)
        {
            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.

            base.OnLoad(e);
        }
        public InvisibleForm()

        {
            #region TimerIcons
            // Load Icons 
            timerIcon = new Icon("timer.ico");
            timer = new NotifyIcon();
            timer.Icon = timerIcon;
            timer.Visible = true;
            #endregion

            #region MenuItemContext

            // Create All Context menu
            MenuItem progName = new MenuItem("Timer for X Application");
            MenuItem setApplication = new MenuItem("Set Application Path");
            MenuItem setWindow = new MenuItem("Set Window");
            MenuItem setLogDir = new MenuItem("Set Log Directory");
            MenuItem setDelay = new MenuItem("Set Delay (If No Window Detected)");

            ContextMenu contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add(progName);
            contextMenu.MenuItems.Add(setApplication);
            contextMenu.MenuItems.Add(setWindow);
            contextMenu.MenuItems.Add(setLogDir);
            contextMenu.MenuItems.Add(setDelay);

            timer.ContextMenu = contextMenu;

            // Wireup setApplicaiton

            setApplication.Click += SetApplication_Click;
            setWindow.Click += SetWindow_Click;
            setLogDir.Click += SetLogDir_Click;
            setDelay.Click += SetDelay_Click;
            #endregion

            #region Invis stuff

            // Initial window as hidden for taskbar app
            InitializeComponent();
            this.WindowState = FormWindowState.Minimized;
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            #endregion

            #region Intial Boxes
            string researchGroup = Microsoft.VisualBasic.Interaction.InputBox("Please Enter your research group", "Research Group", "", -1, -1);
            string dept = Microsoft.VisualBasic.Interaction.InputBox("Please Enter you department", "Department", "", -1, -1);
            // Catch blank Fields
            if (researchGroup == "" || dept == "")
            {
                timer.Dispose();
                Environment.Exit(0);
            }
            #endregion

            #region Start Application
            // Start Application to track usage on
            ProcessStartInfo start_info = new ProcessStartInfo(ConfigurationManager.AppSettings["application"]);

            Process proc = new Process();
            proc.StartInfo = start_info;
            proc.Start();
            #endregion

            #region Write Date to Log

            // Write Date and time to log
            DateTime date = DateTime.Now;
            string textToWrite = date.ToString(format) + Environment.NewLine;
            string final = string.Format("START: {0} {1} ", researchGroup, dept) + textToWrite;
            File.AppendAllText(ConfigurationManager.AppSettings["logDir"], final);
            #endregion

            #region Stopwatch Calculator
            stopwatch.Start();
            ThreadStart starter = delegate { getProcessThread(researchGroup, dept); };
            timerWorker = new Thread(new ThreadStart(starter));
            timerWorker.Start();
            #endregion
        }



        #region Click Wiring
        private void SetLogDir_Click(object sender, EventArgs e)
        {
            string logDirPath = Microsoft.VisualBasic.Interaction.InputBox("Please Enter destination path of log file", "Log Dir", "", -1, -1);
            Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
            config.AppSettings.Settings.Remove("logDir");
            config.AppSettings.Settings.Add("logDir", logDirPath);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private void SetWindow_Click(object sender, EventArgs e)
        {
            string window = Microsoft.VisualBasic.Interaction.InputBox("Please Enter Window name of application", "Window Name", "", -1, -1);
            Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
            config.AppSettings.Settings.Remove("window");
            config.AppSettings.Settings.Add("window", window);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private void SetApplication_Click(object sender, EventArgs e)
        {
            string path = Microsoft.VisualBasic.Interaction.InputBox("Please Enter path to application", "Application Path", "", -1, -1);
            Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
            config.AppSettings.Settings.Remove("application");
            config.AppSettings.Settings.Add("application", path);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
        private void SetDelay_Click(object sender, EventArgs e)
        {
            string delay = Microsoft.VisualBasic.Interaction.InputBox("Please Enter delay (Integers only)", "Delay", "", -1, -1);
            Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
            config.AppSettings.Settings.Remove("delay");
            config.AppSettings.Settings.Add("delay", delay);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
        #endregion

        public void getProcessThread(string researchGroup, string dept)
        {
            try
            {
                int x = Int32.Parse(ConfigurationManager.AppSettings["delay"]);
                Thread.Sleep(x); // Longer delay to compensate for file opening
                while (true)
                {
                    Thread.Sleep(100);
                    Process[] pname = Process.GetProcessesByName(ConfigurationManager.AppSettings["window"]);
                    if (pname.Length == 0)
                    {
                        stopwatch.Stop();
                        string elapsed_time = string.Format("Elapsed Time: {0}", stopwatch.Elapsed.ToString("mm\\:ss"));
                        if (elapsed_time == "Elapsed Time: 00:00")
                        {
                            MessageBox.Show("Program Never launched. If it did, window name is wrong");
                            timer.Dispose();
                            Environment.Exit(0);
                        }
                        else
                        {
                            DateTime date = DateTime.Now;
                            string textToWrite = date.ToString(format);
                            string final = string.Format("END:   {0} {1} ", researchGroup, dept) + textToWrite + " " + elapsed_time + Environment.NewLine + Environment.NewLine;
                            File.AppendAllText(ConfigurationManager.AppSettings["logDir"], final);
                            timer.Dispose();
                            Environment.Exit(0);
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            catch (ThreadAbortException tbe)
            {
            }
            
        }
    }
}
