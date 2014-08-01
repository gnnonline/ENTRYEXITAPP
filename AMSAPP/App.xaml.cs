using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;

namespace AMSAPP
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public TaskbarIcon notifyIcon;
        public bool HelpWindowOpen;
        public bool SummaryWindowOpen;
        System.Windows.Threading.DispatcherTimer AMSLoadTimer = null;
        DataContext MyDataContext;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            this.MyDataContext = new DataContext();

            HelpWindowOpen = false;
            SummaryWindowOpen = false;
            //create the notifyicon (it's a resource declared in NotifyIconResources.xaml
            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");

            Application.Current.MainWindow = new MainWindow();
            //Application.Current.MainWindow.Show();

            CustomStartUp();

            Microsoft.Win32.SystemEvents.SessionSwitch +=
                    new SessionSwitchEventHandler(SystemEvents_SessionSwitch);

            InitializeAMSDataLoadTimer();
            LoadData();
            Application.Current.MainWindow.Show();
        }

        private void CustomStartUp()
        {

            MyDataContext.AddComputerEvent(new AMSAPP.ComputerEvent { EventOn = DateTime.Now, EventType = SessionSwitchReason.SessionLogon.ToString() });
            
           
        }

        private void InitializeAMSDataLoadTimer()
        {

            AMSLoadTimer = new System.Windows.Threading.DispatcherTimer();
            AMSLoadTimer.Tick += new EventHandler(AMSLoadTimer_Tick);
            AMSLoadTimer.Interval = new TimeSpan(0, AMSAPP.Properties.Settings.Default.RefreshInterval, 0);
            AMSLoadTimer.Start();
           
        }

        private void AMSLoadTimer_Tick(object sender, EventArgs e)
        {
            LoadData();
        }

        public void LoadData()
        {
            this.MainWindow.Cursor = Cursors.Wait;
            try
            {
                AMSEvent accessEvent = new AMSEvent();
                try
                {
                    accessEvent = AMSUtil.GetLastEventAMS();
                }
                catch (Exception exception)
                {

                    Logger.Log(exception.StackTrace);
                }

                if (accessEvent.EventOn != null)
                {
                    var rowCount = MyDataContext.AMSEvents.Where(x => x.EventType == accessEvent.EventType && x.EventOn == accessEvent.EventOn && x.ElapsedTimeTxt == accessEvent.ElapsedTimeTxt).Count();
                    if (rowCount == 0)
                    {

                        MyDataContext.AddAMSEvent(accessEvent);
                       
                    }
                }

                SyncWithComputerEvents();

            }
            catch (Exception exception)
            {

                Logger.Log(exception.StackTrace);
            }
            finally
            {
                this.MainWindow.Cursor = Cursors.Arrow;
            }

        }

        private void SyncWithComputerEvents()
        {
            var TodayAccessEvents = MyDataContext.AMSEvents.ToList().Where(x => x.EventOn.Date == DateTime.Now.Date);

            if (TodayAccessEvents.Count() == 0)
            {
                var firstComputerEvent = MyDataContext.ComputerEvents.ToList().FirstOrDefault(x => x.EventOn.Date == DateTime.Now.Date && (x.EventType == SessionSwitchReason.SessionLogon.ToString() || x.EventType == SessionSwitchReason.SessionUnlock.ToString()));
                if (firstComputerEvent != null)
                {
                    MyDataContext.AddAMSEvent(new AMSEvent
                    {
                        EventOn = firstComputerEvent.EventOn,
                        EventType = (byte)AccessEventType.Entry,
                        Description = "Approximate from system events -Currently Logged In",
                        ElapsedTime = TimeSpan.Zero,
                        IsActual = false
                    }
                                                 );
                   
                }
            }
            else
            {
                var latestId = MyDataContext.AMSEvents.ToList().Where(x => x.EventOn.Date == DateTime.Now.Date).Max(p => p.Id);
                var lastAMSEvent = MyDataContext.AMSEvents.ToList().SingleOrDefault(x => x.Id == latestId);
                if (lastAMSEvent.EventType == (byte)AccessEventType.Exit)
                {
                    latestId = MyDataContext.ComputerEvents.ToList().Where(x => x.EventOn > lastAMSEvent.EventOn.AddMinutes(2)).Min(p => p.Id);
                    if (latestId > 0)
                    {
                        var lastComputerEvent = MyDataContext.ComputerEvents.ToList().SingleOrDefault(x => x.Id == latestId);
                        if (lastComputerEvent.EventType == SessionSwitchReason.SessionUnlock.ToString() || lastComputerEvent.EventType == SessionSwitchReason.SessionLogon.ToString())
                        {
                            MyDataContext.AddAMSEvent(new AMSEvent
                           {
                               EventOn = lastComputerEvent.EventOn,
                               EventType = (byte)AccessEventType.Entry,
                               Description = "Approximate from system events - Currently Logged In",
                               ElapsedTime = lastAMSEvent.ElapsedTime,
                               IsActual = false
                           });
                           
                        }
                    }
                }
            }


        }

        public AMSEvent GetLastEvent()
        {
            var latestId = MyDataContext.AMSEvents.ToList().Where(x => x.EventOn.Date == DateTime.Now.Date).Max(p => p.Id);
            return MyDataContext.AMSEvents.ToList().SingleOrDefault(x => x.Id == latestId);
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            
            //SessionSwitchReason
            Logger.Log(e.Reason.ToString());

            MyDataContext.AddComputerEvent(new AMSAPP.ComputerEvent { EventOn = DateTime.Now, EventType = e.Reason.ToString() });
          

            if (e.Reason == SessionSwitchReason.SessionUnlock || e.Reason == SessionSwitchReason.SessionLogon)
            {
                LoadData();
            }

        }

        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner
            base.OnExit(e);
        }

        public void showbaloon()
        {
            try
            {
                this.notifyIcon.ShowBalloonTip("Done", "Good Job !!! ", notifyIcon.Icon);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.StackTrace);
            }
        }
    }
}
