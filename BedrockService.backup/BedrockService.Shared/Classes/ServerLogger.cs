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
        private readonly StreamWriter _logWriter;
        private readonly bool _logToFile = false;
        private readonly bool _logToConsole = false;
        private readonly string _parent;
        private readonly string _logDir;
        private readonly IServerConfiguration _serverConfiguration;

        public ServerLogger(IProcessInfo processInfo, IServiceConfiguration serviceConfiguration, IServerConfiguration serverConfiguration, string parent)
        {
            _serverConfiguration = serverConfiguration;
            _logDir = $@"{processInfo.GetDirectory()}\Server\Logs";
            _logToFile = bool.Parse(serviceConfiguration.GetProp("LogServersToFile").ToString());
            _parent = parent;
            _logToConsole = true;
            if (_logToFile)
            {
                if (!Directory.Exists(_logDir))
                    Directory.CreateDirectory(_logDir);
                _logWriter = new StreamWriter($@"{_logDir}\ServerLog_{parent}_{DateTime.Now:yyyymmddhhmmss}.log", true);
            }
        }

        [JsonConstructor]
        public ServerLogger(string serverName)
        {
            _parent = serverName;
            _logToFile = false;
            _logToConsole = false;
        }

        public void AppendLine(string text)
        {
            try
            {
                _serverConfiguration.GetLog().Add(text);
                if (_logToFile && _logWriter != null)
                {
                    _logWriter.WriteLine(text);
                    _logWriter.Flush();
                }
                if (_logToConsole)
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
                _serverConfiguration.GetLog().Add(text);
                if (_logToFile && _logWriter != null)
                {
                    _logWriter.Write(text);
                    _logWriter.Flush();
                }
                if (_logToConsole)
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
            return _serverConfiguration.GetLog().Count;
        }

        public string FromIndex(int index)
        {
            return _serverConfiguration.GetLog()[index];
        }

        public override string ToString()
        {
            OutString = new StringBuilder();
            foreach (string s in _serverConfiguration.GetLog())
            {
                OutString.Append(s);
            }
            return OutString.ToString();
        }
    }
}
