using BedrockService.Shared.Interfaces;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace BedrockService.Shared.Classes
{
    public class ServerLogger : ILogger
    {
        private StringBuilder OutString = new StringBuilder();
        [NonSerialized]
        private StreamWriter LogWriter;
        private bool LogToFile = false;
        private bool LogToConsole = false;
        private readonly string parent;
        private readonly string LogDir;
        private readonly IServerConfiguration serverConfiguration;

        public ServerLogger(IProcessInfo processInfo, IServiceConfiguration serviceConfiguration, IServerConfiguration serverConfiguration, string parent)
        {
            this.serverConfiguration = serverConfiguration;
            LogDir = $@"{processInfo.GetDirectory()}\Server\Logs";
            LogToFile = bool.Parse(serviceConfiguration.GetProp("LogServersToFile").ToString());
            this.parent = parent;
            LogToConsole = true;
            if (LogToFile)
            {
                if (!Directory.Exists(LogDir))
                    Directory.CreateDirectory(LogDir);
                LogWriter = new StreamWriter($@"{LogDir}\ServerLog_{parent}_{DateTime.Now:yyyymmddhhmmss}.log", true);
            }
        }

        [JsonConstructor]
        public ServerLogger(string serverName)
        {
            parent = serverName;
            LogToFile = false;
            LogToConsole = false;
        }

        public void AppendLine(string text)
        {
            try
            {
                serverConfiguration.GetLog().Add(text);
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

        public void AppendText(string text)
        {
            try
            {
                serverConfiguration.GetLog().Add(text);
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
            return serverConfiguration.GetLog().Count;
        }

        public string FromIndex(int index)
        {
            return serverConfiguration.GetLog()[index];
        }

        public override string ToString()
        {
            OutString = new StringBuilder();
            foreach (string s in serverConfiguration.GetLog())
            {
                OutString.Append(s);
            }
            return OutString.ToString();
        }
    }
}
