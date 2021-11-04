using BedrockService.Shared.Interfaces;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace BedrockService.Service.Logging
{
    public class ServiceLogger : ILogger
    {
        private readonly IServiceConfiguration _serviceConfiguration;
        private StringBuilder _outputString = new StringBuilder();
        [NonSerialized]
        private readonly StreamWriter _logWriter;
        private readonly bool _logToFile = false;
        private readonly bool _logToConsole = false;
        private readonly string _parent = "Service";
        private readonly string _logPath;

        public ServiceLogger(IProcessInfo processInfo, IServiceConfiguration serviceConfiguration)
        {
            _serviceConfiguration = serviceConfiguration;
            _logPath = $@"{processInfo.GetDirectory()}\Service\Logs";
            _logToFile = bool.Parse(serviceConfiguration.GetProp("LogServiceToFile").ToString());
            _logToConsole = true;
            if (_logToFile)
            {
                if (!Directory.Exists(_logPath))
                    Directory.CreateDirectory(_logPath);
                _logWriter = new StreamWriter($@"{_logPath}\ServiceLog_{_parent}_{DateTime.Now:yyyymmddhhmmss}.log", true);
            }
        }

        [JsonConstructor]
        public ServiceLogger(IServiceConfiguration serviceConfiguration, string serverName)
        {
            _serviceConfiguration = serviceConfiguration;
            _parent = serverName;
            _logToFile = false;
            _logToConsole = false;
        }

        public void AppendLine(string text)
        {
            try
            {
                _serviceConfiguration.GetLog().Add(text);
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
                _serviceConfiguration.GetLog().Add(text);
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
            return _serviceConfiguration.GetLog().Count;
        }

        public string FromIndex(int index)
        {
            return _serviceConfiguration.GetLog()[index];
        }

        public override string ToString()
        {
            _outputString = new StringBuilder();
            foreach (string s in _serviceConfiguration.GetLog())
            {
                _outputString.Append(s);
            }
            return _outputString.ToString();
        }
    }
}
