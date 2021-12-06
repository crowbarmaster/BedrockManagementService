using BedrockService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BedrockService.Client.Management
{
    public class ClientLogger : IBedrockLogger
    {
        public List<string> Log = new List<string>();
        public StringBuilder OutString = new StringBuilder();
        public StreamWriter LogWriter;
        private readonly string LogDir;

        public ClientLogger(IProcessInfo processInfo)
        {
            LogDir = $@"{processInfo.GetDirectory()}\Client\ClientLogs";
            if (!Directory.Exists(LogDir))
            {
                Directory.CreateDirectory(LogDir);
            }
            LogWriter = new StreamWriter($@"{LogDir}\ClientLog_{DateTime.Now:yyyymmddhhmmss}.log", true);
        }

        public void AppendLine(string text)
        {
            string addText = $"Client: {text}\r\n";
            Log.Add(addText);
            LogWriter.WriteLine(addText);
            LogWriter.Flush();
            System.Diagnostics.Debug.WriteLine(text);
        }

        public void AppendText(string text) => AppendLine(text);

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
