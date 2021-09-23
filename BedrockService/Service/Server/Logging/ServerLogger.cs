using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BedrockService.Service.Server.Logging
{
    public class ServerLogger
    {
        public List<string> Log { get; set; }
        private StringBuilder OutString = new StringBuilder();
        [NonSerialized]
        private StreamWriter LogWriter;
        private bool LogToFile = false;
        private bool LogToConsole = false;
        private readonly string ServerName = "";
        private string LogDir = $@"{Program.ServiceDirectory}\Server\Logs";

        public ServerLogger(bool enableWriteToFile, string serverName)
        {
            Log = new List<string>();
            LogToFile = enableWriteToFile;
            ServerName = serverName;
            LogToConsole = true;
            if (LogToFile)
            {
                if (!Directory.Exists(LogDir))
                    Directory.CreateDirectory(LogDir);
                LogWriter = new StreamWriter($@"{LogDir}\ServerLog_{serverName}_{DateTime.Now:yyyymmddhhmmss}.log", true);
            }
        }

        [JsonConstructor]
        public ServerLogger(string serverName)
        {
            Log = new List<string>();
            ServerName = serverName;
            LogToFile = false;
            LogToConsole = false;
        }

        public void AppendLine(string text)
        {
            try
            {
                Log.Add(text);
                if (LogToFile && LogWriter != null)
                {
                    LogWriter.WriteLine(text);
                    LogWriter.Flush();
                }
                if (LogToConsole)
                    Console.WriteLine(text);
            }
            catch
            {
            }
        }

        public void Append(string text)
        {
            try
            {
                Log.Add(text);
                if (LogToFile && LogWriter != null)
                {
                    LogWriter.Write(text);
                    LogWriter.Flush();
                }
                if (LogToConsole)
                {
                    Console.Write(text);
                    Console.Out.Flush();
                }
            }
            catch
            {
            }
        }

        public int Count()
        {
            return Log.Count;
        }

        public string FromIndex(int index)
        {
            return Log[index];
        }

        public void AppendPreviousLog (List<string> logToAppend)
        {
            List<string> newList = new List<string>(logToAppend);
            if(Log.Count > 0)
            {
                newList.AddRange(Log);
            }
            Log = newList;
        }

        public override string ToString()
        {
            OutString = new StringBuilder();
            foreach (string s in Log)
            {
                OutString.Append(s);
            }
            return OutString.ToString();
        }
    }
}
