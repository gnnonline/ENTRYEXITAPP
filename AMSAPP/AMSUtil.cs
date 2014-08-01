using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using HtmlAgilityPack;

namespace AMSAPP
{
    public enum AccessEventType
    {
        Entry = 0,
        Exit = 1,
        Unknown = 2

    }

    internal static class AMSUtil
    {
        #region LastEventData

        public static AMSEvent GetLastEventAMS()
        {
            return ParseLastEventFromString(GetLastEventStringFromAMS());
        }

        private static string GetLastEventStringFromAMS()
        {
            WebClient wc = new WebClient();
            wc.UseDefaultCredentials = true;

            string data = wc.DownloadString(new Uri(Properties.Settings.Default.AMSWhereAMI));

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(data);

            HtmlNode node = doc.DocumentNode.SelectSingleNode("//span[@id='ctl00_ctl00_ASPxSplitter1_Content_ContentSplitter_MainContent_mapHimPanel_locationStatusLabel']");

            var mysds = node.InnerText;

            return mysds;
        }

        private static AMSEvent ParseLastEventFromString(string eventData)
        {
            AMSEvent accessEvent = new AMSEvent();
            accessEvent.EventType = (byte)GetEvenTTypeFromString(eventData);
            accessEvent.EventOn = GetEventDateFromString(eventData);
            accessEvent.Description = getEventDescriptionFromString(eventData);
            accessEvent.ElapsedTime = GetElapsedTimeFromAMS();
            return accessEvent;
        }

        private static AccessEventType GetEvenTTypeFromString(string eventData)
        {
            if (eventData.Contains("is currently inside EY premises"))
            {
                return AccessEventType.Entry;
            }
            else if (eventData.Contains("is currently outside EY premises"))
            {
                return AccessEventType.Exit;
            }
            return AccessEventType.Unknown;
        }

        private static string getEventDescriptionFromString(string eventData)
        {
            string retVal = "";
            if (eventData.Length > 24)
            {
                retVal = eventData.Substring(0, 24);
                eventData = eventData.Replace(retVal, "");
            }
            retVal = retVal + eventData.Split('.')[0];
            return retVal;
        }

        private static DateTime GetEventDateFromString(string eventData)
        {
            DateTime lasteventDate = new DateTime();
            try
            {
                int firstcolon = eventData.IndexOf(':');
                var datetimestring = eventData.Substring(firstcolon + 1, eventData.Length - (firstcolon + 1));
                var temp = datetimestring.Trim().Split(' ');
                lasteventDate = DateTime.ParseExact(temp[0] + " " + temp[2] + " " + temp[3],  "M/d/yyyy h:mm tt" , CultureInfo.InvariantCulture);

            }
            catch (Exception ex)
            {
                Logger.Log(ex.StackTrace);
            } 
            
            return lasteventDate;
        }

        #endregion LastEventData

        private static TimeSpan? GetElapsedTimeFromAMS()
        {
            TimeSpan? res = null;

            try
            {
                WebClient wc = new WebClient();
                wc.UseDefaultCredentials = true;
                var data = wc.DownloadString(new Uri(Properties.Settings.Default.AMSCalendar));
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                //
                var node = doc.DocumentNode.SelectSingleNode("//td[@class='dxeCalendarDay_DevEx dxeCalendarToday_DevEx']");
                if (node == null || string.IsNullOrWhiteSpace(node.InnerText))
                {
                    node = doc.DocumentNode.SelectSingleNode("//td[@class='dxeCalendarDay_DevEx dxeCalendarWeekend_DevEx dxeCalendarToday_DevEx']");
                }

                res = TimeSpan.Parse(node.Attributes["title"].Value);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.StackTrace);
            }

            return res;
        }
    }
}