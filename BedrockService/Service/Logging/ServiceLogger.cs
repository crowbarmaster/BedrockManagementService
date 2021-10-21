using BedrockService.Shared.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BedrockService.Service.Logging
{
    public class ServiceLogger : ILogger
    {
        IServiceConfiguration serviceConfiguration;
        private StringBuilder OutString = new StringBuilder();
        [NonSerialized]
        private StreamWriter LogWriter;
        private bool LogToFile = false;
        private bool LogToConsole = false;
        private readonly string parent = "Service";
        private readonly string LogDir;

        public ServiceLogger(IProcessInfo processInfo, IServiceConfiguration serviceConfiguration)
        {
            this.serviceConfiguration = serviceConfiguration;
            LogDir = $@"{processInfo.GetDirectory()}\Service\Logs";
            LogToFile = bool.Parse(serviceConfiguration.GetProp("LogServiceToFile").ToString());
            LogToConsole = true;
            if (LogToFile)
            {
                if (!Directory.Exists(LogDir))
                    Directory.CreateDirectory(LogDir);
                LogWriter = new StreamWriter($@"{LogDir}\ServiceLog_{parent}_{DateTime.Now:yyyymmddhhmmss}.log", true);
            }
        }

        [JsonConstructor]
        public ServiceLogger(IServiceConfiguration serviceConfiguration, string serverName)
        {
            this.serviceConfiguration = serviceConfiguration;
            parent = serverName;
            LogToFile = false;
            LogToConsole = false;
        }

        public void AppendLine(string text)
        {
            try
            {
                serviceConfiguration.GetLog().Add(text);
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
                serviceConfiguration.GetLog().Add(text);
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
            return serviceConfiguration.GetLog().Count;
        }

        public string FromIndex(int index)
        {
            return serviceConfiguration.GetLog()[index];
        }

        public override string ToString()
        {
            OutString = new StringBuilder();
            foreach (string s in serviceConfiguration.GetLog())
            {
                OutString.Append(s);
            }
            return OutString.ToString();
        }
    }
}
