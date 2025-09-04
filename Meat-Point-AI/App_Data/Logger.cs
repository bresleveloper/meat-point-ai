using System;
using System.Configuration;
using System.IO;
using System.Web;

namespace Meat_Point_AI.App_Data
{
    public class Logger
    {
        private static string logPathAppStng = ConfigurationManager.AppSettings["logPath"];
        private static string logPath = HttpContext.Current.Server.MapPath(logPathAppStng);

        static Logger()
        {
            string[] parts = logPath.Split('\\');
            string dirPath = logPath.Replace(parts[parts.Length - 1], "");
            //C:\Users\Administrator\source\repos\Testings\WindowsCleaner\WindowsCleaner\Log\log.txt
            Directory.CreateDirectory(dirPath);
        }

        public static void Log(string message)
        {
            try
            {
                File.AppendAllLines(logPath, new string[] { $"Log - {DateTime.Now.ToString()} - {message}" });
            }
            catch (Exception exLog)
            {
                //check folder permissions
                throw exLog;
            }
        }

        public static void Error(string message, Exception ex = null)
        {
            try
            {
                File.AppendAllLines(logPath, new string[] {
                    $"Error - {DateTime.Now.ToString()} - {message} :: {ex?.ToString()}"
                });
            }
            catch (Exception exLog)
            {
                //check folder permissions
                throw exLog;
            }
        }
    }
}