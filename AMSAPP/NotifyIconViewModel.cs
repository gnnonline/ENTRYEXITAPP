using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;

namespace AMSAPP
{
    /// <summary>
    /// Provides bindable properties and commands for the NotifyIcon. In this sample, the
    /// view model is assigned to the NotifyIcon in XAML. Alternatively, the startup routing
    /// in App.xaml.cs could have created this view model, and assigned it to the NotifyIcon.
    /// </summary>
    public class NotifyIconViewModel
    {
        /// <summary>
        /// Shows a window, if none is already open.
        /// </summary>
        public ICommand ShowWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => Application.Current.MainWindow == null || Application.Current.MainWindow.IsVisible == false,
                    CommandAction = () =>
                    {
                        if (Application.Current.MainWindow == null)
                        {
                            Application.Current.MainWindow = new MainWindow();
                        }                      
                        Application.Current.MainWindow.Show();
                    }
                };
            }
        }


        public ICommand RefreshCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => true,
                    CommandAction = () =>
                    {

                        ((App)Application.Current).LoadData();

                        
                    }
                };
            }
        }

       

        public ICommand SummaryCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => ((App)Application.Current).SummaryWindowOpen == false,
                    CommandAction = () =>
                    {
                        ((App)Application.Current).SummaryWindowOpen = true;
                        Window summary = new Summary();
                        summary.Show();

                    }
                };
            }
        }

      
        public ICommand ShowHideWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => Application.Current.MainWindow != null,
                    CommandAction = () =>
                    {
                        if (Application.Current.MainWindow.IsVisible == true)
                        {
                            Application.Current.MainWindow.Hide();
                        }else
                        {
                            Application.Current.MainWindow.Show();
                        
                        }
                        
                    }
                };
            }
        }
        

        /// <summary>
        /// Hides the main window. This command is only enabled if a window is open.
        /// </summary>
        public ICommand HideWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () => Application.Current.MainWindow.Hide(),
                    CanExecuteFunc = () => Application.Current.MainWindow != null && Application.Current.MainWindow.IsVisible == true
                };
            }
        }

        
        ///// <summary>
        ///// Hides the main window. This command is only enabled if a window is open.
        ///// </summary>
        //public ICommand HideWindowCommand
        //{
        //    get
        //    {
        //        return new DelegateCommand
        //        {
        //            CommandAction = () =>
        //            {
        //                var path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        //                RegistryKey key = Registry.CurrentUser.OpenSubKey(path, true);
        //                key.SetValue("MyApplication", Application.ExecutablePath.ToString());
        //            },
        //            CanExecuteFunc = () => Application.Current.MainWindow != null && Application.Current.MainWindow.IsVisible == true
        //        };
        //    }
        //}

       


        /// <summary>
        /// Shuts down the application.
        /// </summary>
        public ICommand ExitApplicationCommand
        {
            get
            {
                return new DelegateCommand {CommandAction = () => Application.Current.Shutdown()};
            }
        }
    }


    /// <summary>
    /// Simplistic delegate command for the demo.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        public Action CommandAction { get; set; }
        public Func<bool> CanExecuteFunc { get; set; }

        public void Execute(object parameter)
        {
            CommandAction();
        }

        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc == null  || CanExecuteFunc();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
