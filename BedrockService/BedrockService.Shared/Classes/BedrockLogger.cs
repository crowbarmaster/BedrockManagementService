using BedrockService.Shared.Interfaces;
using BedrockService.Shared.SerializeModels;
using Newtonsoft.Json;
using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

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
        private Logger _nLogger;

        public BedrockLogger(IProcessInfo processInfo, IBedrockConfiguration serviceConfiguration, IServerConfiguration serverConfiguration = null) {
            _serviceConfiguration = serviceConfiguration;
            _serverConfiguration = serverConfiguration;
            _processInfo = processInfo;
            _nLogger = NLogManager.Instance.GetLogger("BMSLogger");
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
            if (_processInfo.IsDebugEnabled()) {
                _nLogger.Info(newText);
            } else {
                Console.WriteLine(newText);
            }
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

        public LogFactory GetNLogFactory() => NLogManager.Instance;
    }


    internal class NLogManager {
        // A Logger dispenser for the current assembly (Remember to call Flush on application exit)
        public static LogFactory Instance { get { return _instance.Value; } }
        private static Lazy<LogFactory> _instance = new Lazy<LogFactory>(BuildLogFactory);

        // 
        // Use a config file located next to our current assembly dll 
        // eg, if the running assembly is c:\path\to\MyComponent.dll 
        // the config filepath will be c:\path\to\MyComponent.nlog 
        // 
        // WARNING: This will not be appropriate for assemblies in the GAC 
        // 
        private static LogFactory BuildLogFactory() {
            // Use name of current assembly to construct NLog config filename 
            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            string logDir = $@"{Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)}\PreServiceLog.log";
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = logDir };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");


            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);

            // Apply config           
            NLog.LogManager.Configuration = config;

            LogFactory logFactory = new() {
                Configuration = config
            };
            return logFactory;
        }
    }
}
