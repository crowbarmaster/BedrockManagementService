using Newtonsoft.Json;
using System.Text;
using BedrockService.Shared.Interfaces;
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
            _logToFile = bool.Parse(_serviceConfiguration.GetProp("LogApplicationOutput").ToString());
            _logOwner = _processInfo.DeclaredType();
            if (_processInfo.DeclaredType() == "Service") {
                if (_serverConfiguration != null && bool.Parse(_serviceConfiguration.GetProp("LogServerOutput").ToString())) {
                    _logOwner = _serverConfiguration.GetServerName();
                    string serverLogPath = $@"{_processInfo.GetDirectory()}\Logs\Servers\{_logOwner}";
                    if (!Directory.Exists(serverLogPath))
                        Directory.CreateDirectory(serverLogPath);
                    _logWriter = new StreamWriter($@"{serverLogPath}\{_serverConfiguration.GetServerName()}_{DateTime.Now:MM-dd-yy_hh.mm}.log", true);
                    return;
                }
            }
            if (!Directory.Exists(_logPath))
                Directory.CreateDirectory(_logPath);
            _logWriter = new StreamWriter($@"{_logPath}\{_processInfo.DeclaredType()}_{DateTime.Now:MM-dd-yy_hh.mm}.log", true);
            return;
        }

        public void AppendLine(string text) {
            try {
                string newText = $"{_logOwner}: {text}";
                if (_serverConfiguration != null) {
                    _serverConfiguration.GetLog().Add(new LogEntry(newText));
                }
                else {
                    _serviceConfiguration.GetLog().Add(new LogEntry(newText));
                }
                    if (_logToFile && _logWriter != null) {
                    _logWriter.WriteLine(newText);
                    _logWriter.Flush();
                }
                Console.WriteLine(newText);
            }
            catch {
            }
        }

        public void AppendPreReturnedLine(string text) {
            try {
                string newText = $"{_logOwner}: {text}";
                if (_serverConfiguration != null) {
                    _serverConfiguration.GetLog().Add(new LogEntry(newText));
                }
                else {
                    _serviceConfiguration.GetLog().Add(new LogEntry(newText));
                }
                if (_logToFile && _logWriter != null) {
                    _logWriter.Write(newText);
                    _logWriter.Flush();
                }
                Console.Write(newText);
                Console.Out.Flush();
            }
            catch {
            }
        }

        public void AppendError(Exception exception) {
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
            string output;
            if (_serverConfiguration != null) {
                return string.Join("", _serverConfiguration.GetLog().Select(x => x.Text).ToList());
            }
            return string.Join("", _serviceConfiguration.GetLog().Select(x => x.Text).ToList());
        }
    }
}
