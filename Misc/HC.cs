using Re_Hippocamp.Serializable;

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Re_Hippocamp.Misc
{
    public static class HC
    {
        public static string logFile = "logs.hlogs";
        public static string currentPath = "xxx";
        public static bool canWrite = true;
        private static bool canWriteBasic = true;
        public static long lastRamCheckMB = 0;
        public static void WriteLine(object content, ConsoleColor foregroundColor = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black, bool resetAfterWriting = true,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
        {
            getRam(); writeLineInLogFile(content, foregroundColor, backgroundColor, memberName, sourceFilePath, sourceLineNumber);
            if (!canWrite) return;

            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.WriteLine(content);
            getRam();
            if (resetAfterWriting)
                Console.ResetColor();
        }

        public static void writeLineInLogFile(object content, ConsoleColor foregroundColor, ConsoleColor backgroundColor, string memname, string sfilepath, int linenumber)
        {
            try
            {
                HLog hLog = new HLog()
                {
                    Color = (int)foregroundColor,
                    BColor = (int)backgroundColor,
                    Content = content.ToString(),
                    Ram = lastRamCheckMB.ToString(),
                    Date = DateTime.Now
,
                    MemberName = memname,
                    SourceFile = sfilepath,
                    Line = linenumber
                };

                if (!File.Exists(logFile)) File.WriteAllBytes("logs.hlogs", BO.ObjectToByteArray(new HLogs()));

                HLogs hLogs = BO.ByteArrayToObject(File.ReadAllBytes(logFile)) as HLogs;
                hLogs.Logs.Add(hLog);
                File.WriteAllBytes(logFile, BO.ObjectToByteArray(hLogs));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }



        public static void getRam()
        {
            if (!canWriteBasic) return;
            canWriteBasic = false;
            new Thread(() => 
            {
                var ramAllocation = Process.GetCurrentProcess().WorkingSet64;
                var allocationInMB = ramAllocation / (1024 * 1024);
                lastRamCheckMB = allocationInMB;
                //WriteLine(allocationInMB, ConsoleColor.Green);
                Thread.Sleep(250); canWriteBasic = true;
            }).Start();
        }

        public static void WriteAllBytes(string path, byte[] data)
        {
            WriteLine("writing data " + data.Length + "bytes to " + path + "...");
            try
            {
                File.WriteAllBytes(path, data);
                WriteLine("data " + data.Length + "bytes writed to " + path + ".", ConsoleColor.DarkGreen);
            }
            catch (Exception)
            {
                WriteLine("error when writing data " + data.Length + "bytes to " + path + ".", ConsoleColor.DarkGreen);
            }
        }
    }

}
