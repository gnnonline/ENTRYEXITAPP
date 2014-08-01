using System;
using System.IO;

namespace AMSAPP
{
    public static class Logger
    {
        public static void Log(string logMessage, LogType logType = LogType.Critical)
        {
            if(Properties.Settings.Default.Logging)
            {
                Console.WriteLine(logMessage);
                writeLog(logMessage);
            }
            

        }

        public static void Log(Exception exception, LogType logType = LogType.Critical)
        {
            if (Properties.Settings.Default.Logging)
            {
                Console.WriteLine(exception.StackTrace);
                writeLog(exception.StackTrace);
            }
        }

        private static void writeLog(string message)
        {
            try
            {
                string path = @"AMSAPPLog.txt";
                FileStream file = new FileStream(path, FileMode.Append | FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter sw = new StreamWriter(file);
                // Write a string to the file
                sw.WriteLine(DateTime.Now + "---->" + message);
                // Close StreamWriter
                sw.Close();
                // Close file
                file.Close();
            }
            catch (Exception ex)
            {
            }
        }
    }

    public enum LogType
    {
        Critical = 0,
        Info = 1,
        Alert = 2,
    }
}