using MinecraftService.Shared.Interfaces;
using System.IO;
using System.Runtime.InteropServices;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Shared.Classes {
    public static class LiteLoaderPECore {
        [DllImport("LLPeProviderService.dll", EntryPoint = "?ProcessFunctionList@LLPE@@YA_NPEBD0@Z")]
        static extern bool ProcessFunctionList(string workingDir, string pdbFile);

        [DllImport("LLPeProviderService.dll", EntryPoint = "?CreateSymbolList@LLPE@@YA_NPEBD00@Z")]
        static extern bool CreateSymbolList(string workingDir, string symFileName, string pdbFile);

        [DllImport("LLPeProviderService.dll", EntryPoint = "?ProcessLibFile@LLPE@@YA_NPEBD00@Z")]
        static extern bool ProcessLibFile(string workingDir, string libName, string modExeName);

        [DllImport("LLPeProviderService.dll", EntryPoint = "?ProcessLibDirectory@LLPE@@YA_NPEBD0@Z")]
        static extern bool ProcessLibDirectory(string directoryName, string modExeName);

        [DllImport("LLPeProviderService.dll", EntryPoint = "?ProcessPlugins@LLPE@@YA_NPEBD0@Z")]
        static extern bool ProcessPlugins(string workingDir, string modExeName);

        [DllImport("LLPeProviderService.dll", EntryPoint = "?GenerateDefinitionFiles@LLPE@@YA_NPEBD000@Z")]
        static extern bool GenerateDefinitionFiles(string workingDir, string pdbName, string apiName, string varName);

        [DllImport("LLPeProviderService.dll", EntryPoint = "?CreateModifiedExecutable@LLPE@@YA_NPEBD000@Z")]
        static extern bool CreateModifiedExecutable(string workingDir, string bedrockName, string liteModName, string pdbName);

        [DllImport("LLPeProviderService.dll", EntryPoint = "?GetFilteredFunctionListCount@LLPE@@YAHPEBD@Z")]
        static extern int GetFilteredFunctionListCount(string workingDir, string pdbName);

        public static bool FixServerPlugins(IServerConfiguration server) {
            if (ProcessLibFile($@"{server.GetSettingsProp(ServerPropertyKeys.ServerPath)}", $@"{server.GetSettingsProp(ServerPropertyKeys.ServerPath)}\LiteLoader.dll", server.GetSettingsProp(ServerPropertyKeys.ServerExeName).StringValue)) {
                FixPluginsRecusively(new DirectoryInfo($@"{server.GetSettingsProp(ServerPropertyKeys.ServerPath)}\plugins"), server.GetSettingsProp(ServerPropertyKeys.ServerExeName).StringValue);
                return true;
            }
            return false;
        }

        public static void FixPluginsRecusively(DirectoryInfo directory, string liteModExeName) {
            ProcessLibDirectory(directory.FullName, liteModExeName);
            foreach (DirectoryInfo dir in directory.GetDirectories()) {
                ProcessLibDirectory(directory.FullName, liteModExeName);
                FixPluginsRecusively(dir, liteModExeName);
            }
        }

        public static bool BuildLLExe(IServerConfiguration server) {
            if (CreateModifiedExecutable($@"{server.GetSettingsProp(ServerPropertyKeys.ServerPath)}", "bedrock_server.exe", server.GetSettingsProp(ServerPropertyKeys.ServerExeName).StringValue, "bedrock_server.pdb") &&
                FixServerPlugins(server)) {
                return true;
            }
            return false;
        }
    }
}
