using BedrockService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static BedrockService.Shared.Classes.SharedStringBase;

namespace BedrockService.Shared.Classes {
    public static class LiteLoaderPECore {
		[DllImport("LLPeProviderService.dll", EntryPoint = "?ProcessFunctionList@LLPE@@YA_NPEBD@Z")]
		static extern bool ProcessFunctionList(string pdbFile);

		[DllImport("LLPeProviderService.dll", EntryPoint = "?CreateSymbolList@LLPE@@YA_NPEBD0@Z")]
		static extern bool CreateSymbolList(string symFileName, string pdbFile);

		[DllImport("LLPeProviderService.dll", EntryPoint = "?ProcessLibFile@LLPE@@YA_NPEBD0@Z")]
		static extern bool ProcessLibFile(string libName, string modExeName);

		[DllImport("LLPeProviderService.dll", EntryPoint = "?ProcessLibDirectory@LLPE@@YA_NPEBD0@Z")]
		static extern bool ProcessLibDirectory(string directoryName, string modExeName);

		[DllImport("LLPeProviderService.dll", EntryPoint = "?ProcessPlugins@LLPE@@YA_NPEBD@Z")]
		static extern bool ProcessPlugins(string modExeName);

		[DllImport("LLPeProviderService.dll", EntryPoint = "?GenerateDefinitionFiles@LLPE@@YA_NPEBD00@Z")]
		static extern bool GenerateDefinitionFiles(string pdbName, string apiName, string varName);

		[DllImport("LLPeProviderService.dll", EntryPoint = "?CreateModifiedExecutable@LLPE@@YA_NPEBD00@Z")]
		static extern bool CreateModifiedExecutable(string bedrockName, string liteModName, string pdbName);

		[DllImport("LLPeProviderService.dll", EntryPoint = "?GetFilteredFunctionListCount@LLPE@@YAHPEBD@Z")]
		static extern int GetFilteredFunctionListCount(string pdbName);

		public static bool FixServerPlugins(IServerConfiguration server) {
			if(ProcessLibFile($@"{server.GetSettingsProp(ServerPropertyKeys.ServerPath)}\LiteLoader.dll", $@"{server.GetSettingsProp(ServerPropertyKeys.ServerPath)}\{server.GetSettingsProp(ServerPropertyKeys.ServerExeName)}")) {
				FixPluginsRecusively(new DirectoryInfo($@"{server.GetSettingsProp(ServerPropertyKeys.ServerPath)}\plugins"), $@"{server.GetSettingsProp(ServerPropertyKeys.ServerPath)}\{server.GetSettingsProp(ServerPropertyKeys.ServerExeName)}");
				return true;
            }
			return false;
        }

		public static void FixPluginsRecusively(DirectoryInfo directory, string liteModExeName) {
			foreach(DirectoryInfo dir in directory.GetDirectories()) {
				ProcessLibDirectory(dir.FullName, liteModExeName);
				FixPluginsRecusively(dir, liteModExeName);
			}
        }

		public static bool BuildLLExe(IServerConfiguration server, string liteVersion) {
			if(CreateModifiedExecutable($@"{server.GetSettingsProp(ServerPropertyKeys.ServerPath)}\bedrock_server.exe", $@"{server.GetSettingsProp(ServerPropertyKeys.ServerPath)}\{server.GetSettingsProp(ServerPropertyKeys.ServerExeName)}", $@"{server.GetSettingsProp(ServerPropertyKeys.ServerPath)}\bedrock_server.pdb") &&
				FixServerPlugins(server)) {
				return true;
            }
			return false;
		}
	}
}
