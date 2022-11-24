using BedrockService.Shared.Interfaces;
using BedrockService.Shared.SerializeModels;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace BedrockService.Shared.Classes {
    public class BedrockLogger : IBedrockLogger {
        private readonly IBedrockConfiguration _serviceConfiguration;
        private readonly IProcessInfo _processInfo;
        private readonly IServerConfiguration _serverConfiguration;
        [NonSerialized]
        private StreamWriter _logWriter;
        private bool _logToFile = false;
        private bool _addTimestamps = false;
        private string _logPath;
        private string _logOwner = "Service";

        public BedrockLogger(IProcessInfo processInfo, IBedrockConfiguration serviceConfiguration, IServerConfiguration serverConfiguration = null) {
            _serviceConfiguration = serviceConfiguration;
            _serverConfiguration = serverConfiguration;
            _processInfo = processInfo;
        }

        [JsonConstructor]
        public BedrockLogger() {
            _logToFile = false;
        }

        public void Initialize() {
            _logPath = $@"{_processInfo.GetDirectory()}\Logs\{_processInfo.DeclaredType()}";
            _logToFile = _serviceConfiguration.GetProp("LogApplicationOutput").GetBoolValue();
            _addTimestamps = _serviceConfiguration.GetProp("TimestampLogEntries").GetBoolValue();
            _logOwner = _processInfo.DeclaredType();
            if (_processInfo.DeclaredType() == "Service") {
                if (_serverConfiguration != null && _serviceConfiguration.GetProp("LogServerOutput").GetBoolValue()) {
                    _logOwner = _serverConfiguration.GetServerName();
                    string serverLogPath = $@"{_processInfo.GetDirectory()}\Logs\Servers\{_logOwner}";
                    if (!Directory.Exists(serverLogPath))
                        Directory.CreateDirectory(serverLogPath);
                    _logWriter = new StreamWriter($@"{serverLogPath}\ServerLog-{DateTime.Now:yyyyMMdd_HHmmssff}.log", true);
                    return;
                }
            }
            if (_logWriter == null) {
                if (!Directory.Exists(_logPath))
                    Directory.CreateDirectory(_logPath);
                _logWriter = new StreamWriter($@"{_logPath}\{_processInfo.DeclaredType()}Log-{DateTime.Now:yyyyMMdd_HHmmssff}.log", true);
            }

        }

        public void AppendLine(string text) {
            string newText = $"{_logOwner}: {text}";
            LogEntry entry = new(newText);
            Console.WriteLine(newText);
            try {
                if (_serverConfiguration != null) {
                    _serverConfiguration.GetLog().Add(entry);
                } else {
                    if (_serviceConfiguration != null) {
                        _serviceConfiguration.GetLog().Add(entry);
                    }
                }
                if (_addTimestamps) {
                    newText = $"[{entry.TimeStamp.ToString("G")}] {newText}";
                }
                if (_logToFile && _logWriter != null) {
                    _logWriter.WriteLine(newText);
                    _logWriter.Flush();
                }
            } catch {
            }
        }

        public void AppendErrorFromException(Exception exception) {
            if (_processInfo.IsDebugEnabled()) {
                string addText = $"{exception.GetType().Name} occured in {exception.TargetSite.DeclaringType.Name}:{exception.TargetSite.Name}\n{exception.Message}\n{exception.StackTrace}";
                AppendLine(addText);
            }
        }


        public int Count() {
            return _serverConfiguration != null ?
            _serverConfiguration.GetLog().Count :
            _serviceConfiguration.GetLog().Count;
        }

        public string FromIndex(int index) {
            return _serverConfiguration != null ?
            _serverConfiguration.GetLog()[index].Text :
            _serviceConfiguration.GetLog()[index].Text;
        }

        public override string ToString() {
            if (_serverConfiguration != null) {
                return string.Join("", _serverConfiguration.GetLog().Select(x => x.Text).ToList());
            }
            return string.Join("", _serviceConfiguration.GetLog().Select(x => x.Text).ToList());
        }
    }
}
