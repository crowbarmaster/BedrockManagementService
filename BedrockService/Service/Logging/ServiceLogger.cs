using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BedrockService.Service.Logging
{
    public class ServiceLogger
    {
        public List<string> Log = new List<string>();
        public StringBuilder OutString = new StringBuilder();
        public StreamWriter LogWriter;
        private bool IsServer;
        public string LogDir = $@"{Program.ServiceDirectory}\Service\ServiceLogs";

        public ServiceLogger(bool isServer)
        {
            IsServer = isServer;
            if (isServer)
            {
                if (!Directory.Exists(LogDir))
                    Directory.CreateDirectory(LogDir);
                LogWriter = new StreamWriter($@"{LogDir}\ServiceLog_{DateTime.Now:yyyymmddhhmmss}.log", true);
            }
        }

        public void AppendLine(string text)
        {
            string addText = $"Service: {text}\r\n";
            Log.Add(addText);
            LogWriter.WriteLine(text);
            LogWriter.Flush();
            if(IsServer)
            Console.WriteLine(text);
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
