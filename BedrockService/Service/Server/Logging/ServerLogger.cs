using BedrockService.Service.Server.HostInfoClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BedrockService.Service.Server.Logging
{
    public class ServerLogger
    {
        private List<string> Log = new List<string>();
        private StringBuilder OutString = new StringBuilder();
        [NonSerialized]
        private StreamWriter LogWriter;
        private bool LogToFile = false;
        private bool LogToConsole = false;
        private readonly string ServerName = "";
        private string LogDir = $@"{Program.ServiceDirectory}\Server\Logs";

        public ServerLogger(bool enableWriteToFile, string serverName)
        {
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
            ServerName = serverName;
            LogToFile = false;
            LogToConsole = false;
        }

        public void AppendLine(string text)
        {
            Log.Add(text);
            if (LogToFile && LogWriter != null)
            {
                LogWriter.WriteLine(text);
                LogWriter.Flush();
            }
            if(LogToConsole)
                Console.WriteLine(text);
        }

        public void Append(string text)
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

        public int Count()
        {
            return Log.Count;
        }

        public string FromIndex(int index)
        {
            return Log[index];
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
