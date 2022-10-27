using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BedrockService.Shared.LiteLoaderFileModels.JsonModels {
        public class AddonsHelper {
            public string autoInstallPath { get; set; }
            public bool enabled { get; set; }
        }

        public class AntiGive {
            public string command { get; set; }
            public bool enabled { get; set; }
        }

        public class CheckRunningBDS {
            public bool enabled { get; set; }
        }

        public class ClientChunkPreGeneration {
            public bool enabled { get; set; }
        }

        public class CrashLogger {
            public bool enabled { get; set; }
            public string path { get; set; }
        }

        public class EconomyCore {
            public bool enabled { get; set; }
        }

        public class ErrorStackTraceback {
            public bool cacheSymbol { get; set; }
            public bool enabled { get; set; }
        }

        public class FixBDSCrash {
            public bool enabled { get; set; }
        }

        public class FixDisconnectBug {
            public bool enabled { get; set; }
        }

        public class FixListenPort {
            public bool enabled { get; set; }
        }

        public class FixMcBug {
            public bool enabled { get; set; }
        }

        public class ForceUtf8Input {
            public bool enabled { get; set; }
        }

        public class OutputFilter {
            public bool enabled { get; set; }
            public IList<object> filterRegex { get; set; }
            public bool onlyFilterConsoleOutput { get; set; }
        }

        public class ParticleAPI {
            public bool enabled { get; set; }
        }

        public class PermissionAPI {
            public bool enabled { get; set; }
        }

        public class SimpleServerLogger {
            public bool enabled { get; set; }
        }

        public class TpdimCommand {
            public bool enabled { get; set; }
        }

        public class UnlockCmd {
            public bool enabled { get; set; }
        }

        public class UnoccupyPort19132 {
            public bool enabled { get; set; }
        }

        public class WelcomeText {
            public bool enabled { get; set; }
        }

        public class Modules {
            public AddonsHelper AddonsHelper { get; set; }
            public AntiGive AntiGive { get; set; }
            public CheckRunningBDS CheckRunningBDS { get; set; }
            public ClientChunkPreGeneration ClientChunkPreGeneration { get; set; }
            public CrashLogger CrashLogger { get; set; }
            public EconomyCore EconomyCore { get; set; }
            public ErrorStackTraceback ErrorStackTraceback { get; set; }
            public FixBDSCrash FixBDSCrash { get; set; }
            public FixDisconnectBug FixDisconnectBug { get; set; }
            public FixListenPort FixListenPort { get; set; }
            public FixMcBug FixMcBug { get; set; }
            public ForceUtf8Input ForceUtf8Input { get; set; }
            public OutputFilter OutputFilter { get; set; }
            public ParticleAPI ParticleAPI { get; set; }
            public PermissionAPI PermissionAPI { get; set; }
            public SimpleServerLogger SimpleServerLogger { get; set; }
            public TpdimCommand TpdimCommand { get; set; }
            public UnlockCmd UnlockCmd { get; set; }
            public UnoccupyPort19132 UnoccupyPort19132 { get; set; }
            public WelcomeText WelcomeText { get; set; }
        }

        public class ScriptEngine {
            public bool alwaysLaunch { get; set; }
            public bool enabled { get; set; }
        }

        public class LiteLoaderConfigJsonModel {
            public bool ColorLog { get; set; }
            public bool DebugMode { get; set; }
            public string Language { get; set; }
            public int LogLevel { get; set; }
            public Modules Modules { get; set; }
            public ScriptEngine ScriptEngine { get; set; }
            public int Version { get; set; }
        }
    }

