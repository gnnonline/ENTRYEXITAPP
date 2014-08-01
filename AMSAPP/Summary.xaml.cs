using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using HtmlAgilityPack;
using MahApps.Metro.Controls;



namespace AMSAPP
{
    /// <summary>
    /// Interaction logic for Summary.xaml
    /// </summary>
    public partial class Summary : MetroWindow
    {
        private HtmlDocument doc = new HtmlDocument();
        
      

        public Summary()
        {
            try
            {
                InitializeComponent();
                Logger.Log("InitializeComponent done");
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

                MySlider.Value = Properties.Settings.Default.MinTimeSpan;
                LoadMonthsFromWeb();
                LoadName();

                //MySlider.IsEnabled = false;
                LoadToday();
                LoadSettings();
                CheckTimesheet();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void LoadToday()
        {
            try
            {
                DataContext tetst = new DataContext();
                dataGrid1.ItemsSource = tetst.ComputerEvents.ToList().Where(x => x.EventOn.Date == DateTime.Now.Date).ToList();
                dataGrid2.ItemsSource = tetst.AMSEvents.ToList().Where(x => x.EventOn.Date == DateTime.Now.Date).ToList();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }


        private void LoadName()
        {
            try
            {
                var headerNode = doc.DocumentNode.SelectSingleNode("//td[@class='dxrpHeader_DevEx']");
                if (headerNode != null)
                { 
                    var nameNode = headerNode.SelectSingleNode(".//span");
                    if (nameNode != null)
                    {
                        if (!string.IsNullOrWhiteSpace(nameNode.InnerText))
                        {
                            this.Title =  nameNode.InnerText.Replace("Attendance :", "");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void CheckTimesheet()
        {
            WebClient wc = new WebClient();
            wc.UseDefaultCredentials = true;
            var data = wc.DownloadString(new Uri(Properties.Settings.Default.TimesheetUrl));
            HtmlDocument doc1 = new HtmlDocument();
            doc1.LoadHtml(data);
            Logger.Log("Downloded data from " + Properties.Settings.Default.TimesheetUrl);
            var statusNode = doc1.DocumentNode.SelectSingleNode("//span[@id='ctl00_contentPH_ucTimesheetHeader_lblStatus']");
            if (statusNode != null)
            {
                lblTimesheetStatus.Content = statusNode.InnerText;
                if (string.IsNullOrWhiteSpace(statusNode.InnerText))
                {
                    lblTimesheetStatus.Content = "Time Sheet Not Submitted";
                }
            }
            else
            {
                lblTimesheetStatus.Content = "Time Sheet Not Submitted";
                Logger.Log("statusNode is null");
            }

            //MessageBox.Show(this, lblTimesheetStatus.Content.ToString(),"TimeSheet Entry");
        }
      

        private void LoadMonthsFromWeb()
        {
            try
            {
                WebClient wc = new WebClient();
                wc.UseDefaultCredentials = true;
                var data = wc.DownloadString(new Uri(Properties.Settings.Default.AMSCalendar));
                Logger.Log("Data Downloaded from " + Properties.Settings.Default.AMSCalendar);              
                doc.LoadHtml(data);
                var monthList = GetMonthNames();
                cbMonths.ItemsSource = monthList;
                cbMonths.SelectedIndex = monthList.Count - 1;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private List<string> GetMonthNames()
        {
            try
            {
                var monthList = new List<string>();
                var monthNodes = doc.DocumentNode.SelectNodes("//td[@class='dxeCalendarHeader_DevEx']");

                foreach (var monthNode in monthNodes)
                {
                    var month = monthNode.SelectSingleNode(".//span").InnerText;
                    monthList.Add(month);
                }
                Logger.Log("Loaded Months " + monthList.Count());
                return monthList;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return null;
            }
        }

        private void cbMonths_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // ... Get the ComboBox.
            try
            {
                var comboBox = sender as ComboBox;

                string month = comboBox.SelectedItem as string;
                Logger.Log("Selected Month " + month);
                var monthNode = GetMonthNode(month);

                var year = Convert.ToInt32(month.Split(',')[1]);
                Logger.Log("Year = " + year);
                var iMonth = DateTime.ParseExact(month.Split(',')[0], "MMMM", CultureInfo.InvariantCulture).Month;
                Logger.Log("MonthNumber = " + iMonth);

                if (monthNode != null)
                {
                    var monthGrid = monthNode.ParentNode.NextSibling.SelectSingleNode(".//td[@class='dxMonthGrid']");
                    if (monthGrid == null)
                    {
                        Logger.Log("monthGrid is null");
                    }
                    else
                    {
                        Logger.Log("monthGrid is not null");
                    }
                    var tds = monthGrid.SelectNodes(string.Format(".//*[contains(@class,'{0}')]", "dxeCalendarDay_DevEx"));

                    Logger.Log("Days found in monthGrid : " + tds.Count());
                    var dayLogs = new List<DayLog>();
                    foreach (var td in tds)
                    {
                        int day;
                        if (td.Attributes["Class"] != null && td.Attributes["Class"].Value.Contains("dxeCalendarOtherMonth_DevEx"))
                        {
                            Logger.Log("skipping other month date : " + td.InnerText);
                            continue;
                        }

                        if (Int32.TryParse(td.InnerText, out day))
                        {
                            if (td.Attributes["Class"] != null && td.Attributes["Class"].Value.Contains("dxeCalendarToday_DevEx"))
                            {
                                dayLogs.Add(new DayLog
                                            {
                                                Day = day,
                                                Title = "",
                                                Class = td.Attributes["Class"] != null ? td.Attributes["Class"].Value : "",
                                                Color = td.Attributes["bgcolor"] != null ? td.Attributes["bgcolor"].Value : "white",
                                                WeekDay = (new DateTime(year, iMonth, day)).DayOfWeek.ToString()
                                            }

                                        );
                                Logger.Log("Added Today");
                            }
                            else
                            {
                                dayLogs.Add(new DayLog
                                {
                                    Day = day,
                                    Title = td.Attributes["Title"] != null ? td.Attributes["Title"].Value : "",
                                    Class = td.Attributes["Class"] != null ? td.Attributes["Class"].Value : "",
                                    Color = td.Attributes["bgcolor"] != null ? td.Attributes["bgcolor"].Value : "white",
                                    WeekDay = (new DateTime(year, iMonth, day)).DayOfWeek.ToString()
                                }


                                        );
                                Logger.Log("Added DayLog " + day);
                            }
                        }
                        else
                        {
                            Logger.Log("Can not parse to integer : " + td.InnerText);
                        }
                    }
                     Logger.Log("dayLogs count" + dayLogs.Count());
                    var displayList = dayLogs.Select(x => GetGridValue(x)).ToList();
                    Logger.Log("displayList count" + displayList.Count());
                    monthDataGrid.ItemsSource = displayList;
                    ShowAverage(dayLogs);

                    Logger.Log("completed month selection change");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private SummaryGridRow GetGridValue(DayLog dayLog)
        {
            TimeSpan time = new TimeSpan();
            string duration = "";
            string comment = "";
            if (dayLog.Title.Length >= 8 && TimeSpan.TryParse(dayLog.Title.Substring(0, 8), out time))
            {
                duration = time.ToString();
                if (dayLog.Title.Length > 8)
                {
                    comment = dayLog.Title.Substring(8, dayLog.Title.Length - 8);
                } 
                if (comment.Trim().Length == 0)
                {
                    if (dayLog.Class.Contains("dxeCalendarWeekend_DevEx"))
                    {
                        comment = "Weekend";
                    }
                }
            }
            else
            {
                comment = dayLog.Title;
               
            }
            comment = comment.Replace(":", "").Trim();
            //var color = GetValueFromStyle(dayLog.Style, "BACKGROUND-COLOR");
            return new SummaryGridRow { WeekDay = dayLog.WeekDay, Day = dayLog.Day, Duration = duration, Comments = comment, Color = dayLog.Color };
        }

        public class SummaryGridRow 
        {
            public string WeekDay { get; set; }

            public int Day { get; set; }

            public string Duration { get; set; }

            public string Comments { get; set; }

            public string Color { get; set; }
        }

        private void ShowAverage(List<DayLog> dayLogs)
        {
            try { 
                TimeSpan sum = new TimeSpan();
                double count = 0;
                int daysRemaining = 0;
                foreach (var dayLog in dayLogs)
                {
                    TimeSpan time = new TimeSpan();
                    if (dayLog.Title.Trim().Length >= 8)
                    {
                        if (TimeSpan.TryParse(dayLog.Title.Substring(0, 8), out time))
                        {
                            sum += time;
                            if (!(dayLog.Class.Contains("dxeCalendarWeekend_DevEx")))
                            {
                                if (dayLog.Title.Contains("Leave Half Day"))
                                {
                                    count += 0.5;
                                }
                                else
                                {
                                    count++;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!(dayLog.Class.Contains("dxeCalendarWeekend_DevEx") || dayLog.Class.Contains("dxeCalendarOtherMonth_DevEx")))
                        {
                            daysRemaining++;
                        }
                    }
                }
                int average = 0;
                if (count > 0)
                {
                    average = (int)(sum.TotalMinutes / count);
                }
                TimeSpan averageTimespan = new TimeSpan(0, 0, average, 0, 0);
                lblAverage.Content = FormatTimeSpan(averageTimespan);
                lblTotalDays.Content = count.ToString();
                lblRemainingDays.Content = daysRemaining.ToString();
                if (daysRemaining > 0)
                {
                    MySlider.IsEnabled = true;
                    CalculateRequired((int)MySlider.Value);
                    lblrequired.Visibility = Visibility.Visible;
                }
                else
                {
                    MySlider.IsEnabled = false;
                    lblrequired.Visibility = Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            
        }

        private HtmlNode GetMonthNode(string month)
        {
            var monthNodes = doc.DocumentNode.SelectNodes("//td[@class='dxeCalendarHeader_DevEx']");

            foreach (var monthNode in monthNodes)
            {
                if (monthNode.SelectSingleNode(".//span").InnerText == month)
                {
                    Logger.Log("Month Node found for month " + month,LogType.Info);
                    return monthNode;
                }
            }
            Logger.Log("Month Node not found for month " + month,LogType.Critical);
            return null;
        }

        private class DayLog
        {
            public int Day { get; set; }

            public string WeekDay { get; set; }

            public string Title { get; set; }

            public string Class { get; set; }

            public string Color { get; set; }


        }

        private void MySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                var slider = sender as Slider;

                int selectedValue = (int)slider.Value;
                CalculateRequired(selectedValue);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void CalculateRequired(int selectedValue)
        {
            try
            {
                runExpected.Text = FormatTimeSpan(TimeSpan.FromMinutes(selectedValue));
                if (lblTotalDays.Content.ToString() != "0" && lblRemainingDays.Content.ToString() != "0")
                {
                    double totalDays = (Convert.ToDouble(lblTotalDays.Content) + Convert.ToDouble(lblRemainingDays.Content));
                    TimeSpan currentAverage = TimeSpan.Parse(lblAverage.Content.ToString());

                    int requiredMinutes = (int)((Convert.ToDouble((selectedValue * totalDays)) - (Convert.ToDouble(currentAverage.TotalMinutes) * Convert.ToDouble(lblTotalDays.Content))) / Convert.ToDouble(lblRemainingDays.Content));

                    lblrequired.Content = FormatTimeSpan(TimeSpan.FromMinutes(requiredMinutes));
                }else
                {
                    lblrequired.Content = FormatTimeSpan(TimeSpan.FromMinutes(selectedValue));
                }
            }
           
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
        private string FormatTimeSpan(TimeSpan time)
        {
            return ((time < TimeSpan.Zero) ? "-" : "") + time.ToString(@"hh\:mm");
        }
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).SummaryWindowOpen = false;
        }

        private string GetValueFromStyle(string style, string key)
        {
            if (!string.IsNullOrWhiteSpace(style))
            {
                var stylePairs = style.Split(';');
                foreach (var stylePair in stylePairs)
                {
                    if (stylePair.Contains(key))
                    {
                        var nameValues = stylePair.Split(':');
                        if (nameValues.Count() > 1)
                        {
                            return nameValues[1];
                        }
                    }
                }
            }
            return null;
        }

        private void monthDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            try
            {
                DataGridTextColumn textColumn = e.Column as DataGridTextColumn;
                if (textColumn != null)
                {
                    Style style = new Style(textColumn.ElementStyle.TargetType, textColumn.ElementStyle.BasedOn);
                    var bbSetter = new Setter(
                        DataGrid.HorizontalAlignmentProperty,
                        HorizontalAlignment.Center);
                    style.Setters.Add(bbSetter);
                    textColumn.ElementStyle = style;
                }
                if (e.Column.Header.ToString() == "Color")
                {
                    e.Cancel = true;
                }
                else if (e.Column.Header.ToString() == "Day")
                {
                    e.Column.Width = 50;
                }
                else if (e.Column.Header.ToString() == "WeekDay")
                {
                    e.Column.Width = 100;
                }
                else if (e.Column.Header.ToString() == "Duration")
                {
                    e.Column.Width = 100;
                }
                else if (e.Column.Header.ToString() == "Comment")
                {
                    e.Column.Width = 300;
                }
            }
            catch (Exception ex)
            {

                Logger.Log(ex);
            }
           
        }

        private void monthDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
           
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => AlterRow(e)));
            
                      
        }

        private void AlterRow(DataGridRowEventArgs e)
        {
            try
            {
                SummaryGridRow RowDataContaxt = new SummaryGridRow();
                RowDataContaxt = Cast(RowDataContaxt, e.Row.DataContext);
                if (RowDataContaxt != null)
                {
                    var cell = GetCell(monthDataGrid, e.Row, 2);
                    SolidColorBrush myBrush = (SolidColorBrush)new BrushConverter().ConvertFromString(RowDataContaxt.Color);
                    cell.Background = myBrush;

                    cell = GetCell(monthDataGrid, e.Row, 0);
                    var textBlock = cell.Content as TextBlock;
                    if (textBlock.Text == "Saturday" || textBlock.Text == "Sunday")
                    {
                        textBlock.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("Maroon");
                        //cell.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("Red");
                    }

                }
            }
            catch (Exception ex)
            {

                Logger.Log(ex);
            }
        }

        private static T Cast<T>(T typeHolder, Object x)
        {
            // typeHolder above is just for compiler magic
            // to infer the type to cast x to
            return (T)x;
        }

        public static DataGridRow GetRow(DataGrid grid, int index)
        {
            var row = grid.ItemContainerGenerator.ContainerFromIndex(index) as DataGridRow;

            if (row == null)
            {
                // may be virtualized, bring into view and try again
                grid.ScrollIntoView(grid.Items[index]);
                row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index);
            }
            return row;
        }

        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                var v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T ?? GetVisualChild<T>(v);
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        public static DataGridCell GetCell(DataGrid host, DataGridRow row, int columnIndex)
        {
            if (row == null) return null;

            var presenter = GetVisualChild<DataGridCellsPresenter>(row);
            if (presenter == null) return null;

            // try to get the cell but it may possibly be virtualized
            var cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);
            if (cell == null)
            {
                // now try to bring into view and retreive the cell
                host.ScrollIntoView(row, host.Columns[columnIndex]);
                cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);
            }
            return cell;

        }

        private void LoadSettings()
        {
            lblRefreshInterval.Content = Properties.Settings.Default.RefreshInterval;
            sliderRefreshInterval.Value = Properties.Settings.Default.RefreshInterval;
            lblMinHours.Content = TimeSpan.FromMinutes(Properties.Settings.Default.MinTimeSpan).ToString(); 
            sliderMinHours.Value = Properties.Settings.Default.MinTimeSpan;
        }

        private void sliderRefreshInterval_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
           
            var slider = sender as Slider;
            if (slider.Value == slider.Minimum)
            {
                return;
            }
            lblRefreshInterval.Content = (int)slider.Value;
            Properties.Settings.Default.RefreshInterval = (int)slider.Value;
            Properties.Settings.Default.Save();
        }

        private void sliderMinHours_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;
            if (slider.Value == slider.Minimum)
            {
                return;
            }
            lblMinHours.Content = TimeSpan.FromMinutes((int)slider.Value).ToString(); 
            Properties.Settings.Default.MinTimeSpan = (int)slider.Value;
            MySlider.Value = Properties.Settings.Default.MinTimeSpan;
            Properties.Settings.Default.Save();

        }

    }
}