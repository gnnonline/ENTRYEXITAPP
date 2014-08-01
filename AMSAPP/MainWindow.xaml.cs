using System;
using System.Configuration;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AMSAPP
{
    public partial class MainWindow : Window
    {
        private DateTime DateInSite;
        private bool Inside;
        private string tooltipText;
        public bool shownBallon;

        public MainWindow()
        {
            InitializeComponent();
            shownBallon = false;
            Inside = false;
            ClearFields();

            InitializeScreenRefreshTimer();
            //InitializeAMSDataLoadTimer();

            SetToolTip();

           
        }

        private void InitializeScreenRefreshTimer()
        {
            System.Windows.Threading.DispatcherTimer ScreenRefreshTimer = new System.Windows.Threading.DispatcherTimer();
            ScreenRefreshTimer.Tick += new EventHandler(ScreenRefreshTimer_Tick);
            ScreenRefreshTimer.Interval = new TimeSpan(0, 0, 1);
            ScreenRefreshTimer.Start();
        }

        private void ScreenRefreshTimer_Tick(object sender, EventArgs e)
        {
            LoadData();
        }

      

        private void SetToolTip()
        {
            ((App)App.Current).notifyIcon.ToolTipText = tooltipText;
        }

        private void ClearFields()
        {
            Time_Remaining.Content = "";
            Last_Login_Time.Content = "";
            Time_In_Office_Logged.Content = "";
            Total_Time_In_Office.Content = "";
            Time_Remaining.Content = "";
            When_To_Leave.Content = "";
        }

        public void LoadData()
        {
            ClearFields();
            try
            {

                AMSEvent accessEvent = ((App)App.Current).GetLastEvent();
               
                tooltipText = accessEvent.Description;
                if (accessEvent.EventType != (byte)AccessEventType.Unknown)
                {
                    if (accessEvent.EventType == (byte)AccessEventType.Exit)
                    {
                        Inside = false;
                    }
                    else if (accessEvent.EventType == (byte)AccessEventType.Entry)
                    {
                        Inside = true;
                    }

                    
                    if (accessEvent.EventOn != null)
                    {
                        DateInSite = (DateTime)accessEvent.EventOn;
                    }

                    Last_Login_Time.Content = DateInSite.ToDisplayTimeString();
                    Time_In_Office_Logged.Content = accessEvent.ElapsedTime.ToDisplayString();

                    RefreshTimeRemaining();
                }

                SetToolTip();
            }
            catch (Exception ex)
            {
                tooltipText = "Not able to Access Site";
                Logger.Log(ex.StackTrace);
            }
        }

        private void RefreshTimeRemaining()
        {
            try
            {
                TimeSpan timeInOfficeLogged = TimeSpan.Parse(Time_In_Office_Logged.Content.ToString());
                TimeSpan timeInOffice = timeInOfficeLogged;
                if (Inside)
                {
                    timeInOffice = (DateTime.Now - DateInSite) + timeInOfficeLogged;
                }

                Total_Time_In_Office.Content = timeInOffice.ToDisplayString();

                TimeSpan timeRemaining = (TimeSpan.FromMinutes(Properties.Settings.Default.MinTimeSpan) - timeInOffice);

                Time_Remaining.Content = timeRemaining.ToDisplayString();
                if (timeRemaining > TimeSpan.Zero)
                {
                    When_To_Leave.Content = (DateTime.Now + TimeSpan.Parse(Time_Remaining.Content.ToString())).ToString("hh:mm:ss tt");
                }
                else
                {
                    if (!shownBallon)
                    {
                        ((App)App.Current).showbaloon();
                        shownBallon = true;
                    }
                    When_To_Leave.Content = "Done";
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.StackTrace);
            }
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((App)App.Current).LoadData();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width;
            this.Top = desktopWorkingArea.Bottom - this.Height;
        }

        private void Grid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Hide();
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AAFFFFFF"));
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Background = Brushes.Transparent;
        }
    }
}