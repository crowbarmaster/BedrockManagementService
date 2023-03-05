#include "pch.h"
#include "global.h"

#include <filesystem>
#include <fstream>
#include <iostream>
#include <regex>
#include <set>
#include <string>
#include <string_view>

#include <algorithm>
#include <fstream>
#include <unordered_set>

#include "skipFunctions.h"

#include <llvm/Demangle/Demangle.h>
#include <llvm/Demangle/MicrosoftDemangle.h>

#include "pdb.h"

#include "../PortableExecutable/pe_bliss.h"

#include <windows.h>

#include "PeEdit.h"

using namespace pe_bliss;
namespace LLPE {
	std::deque<PdbSymbol> FunctionList;
	std::deque<PdbSymbol> FilteredFunctionList;
	std::string bedrockExeName = "bedrock_server.exe";
	std::string LiteModExeName = "bedrock_server_mod.exe";

	std::wstring str2wstr(const std::string& str, UINT codePage = CP_UTF8) {
		auto len = MultiByteToWideChar(codePage, 0, str.c_str(), -1, nullptr, 0);
		auto* buffer = new wchar_t[len + 1];
		MultiByteToWideChar(codePage, 0, str.c_str(), -1, buffer, len + 1);
		buffer[len] = L'\0';

		std::wstring result(buffer);
		delete[] buffer;
		return result;
	}

	inline void Pause(bool Pause) {
		if (Pause)
			system("pause");
	}

	bool InitializeFunctionList(std::string pdbName, std::string symbolName, bool genSymList = false) {
		std::ofstream BDSSymList;
		std::string bedrockPdbName = pdbName != "" ? pdbName : "bedrock_server.pdb";
		if (FunctionList.size() == 0) {
			FunctionList = *loadPDB(str2wstr(pdbName).c_str());
			if (FunctionList.size() == 0) {
				std::cout << "[Error] Could not open PDB!" << std::endl;
				std::cout << "PDB name: " << pdbName << std::endl;
				return false;
			}
		}
		if (genSymList) {
			std::cout << "[LLPeService] Generating symbol list, please wait..." << std::endl;
			std::string SymFileName =  symbolName != "" ? symbolName : "bedrock_server_symlist.txt";
			BDSSymList.open(SymFileName, std::ios::ate | std::ios::out);
			if (!BDSSymList) {
				std::cout << "[Error][LLPeService] Cannot create " << SymFileName << std::endl;
				return false;
			}

			for (const auto& fn : FunctionList) {
				bool keepFunc = true;
				char tmp[11];
				sprintf_s(tmp, 11, "[%08d]", fn.Rva);

				auto demangledName = llvm::microsoftDemangle(
					fn.Name.c_str(), nullptr, nullptr, nullptr, nullptr,
					(llvm::MSDemangleFlags)(llvm::MSDF_NoCallingConvention | llvm::MSDF_NoAccessSpecifier));
				if (demangledName) {
					BDSSymList << demangledName << std::endl;
					free(demangledName);
				}
				BDSSymList << tmp << fn.Name << std::endl << std::endl;
			}
			try {
				BDSSymList.flush();
				BDSSymList.close();
				std::cout << "[LLPeService] [DEV]Symbol List File              Created" << std::endl;
				return true;
			}
			catch (...) {
				std::cout << "[Error] Failed to Create " << SymFileName << std::endl;
				return false;
			}
		}
		if (FilteredFunctionList.size() == 0) {
		std::cout << "[LLPeService] Filtering PDB symbols, please wait..." << std::endl;
			for (const auto& fn : FunctionList) {
			bool keepFunc = true;
			if (fn.Name[0] != '?') {
				keepFunc = false;
			}
			for (const std::string a : LLTool::SkipPerfix) {
				if (fn.Name.starts_with(a)) {
					keepFunc = false;
				}
			}
			for (const auto& reg : LLTool::SkipRegex) {
				std::smatch result;
				if (std::regex_match(fn.Name, result, reg)) {
					keepFunc = false;
				}
			}
			if (keepFunc) {
				FilteredFunctionList.push_back(fn);
			}
		}
			return true;
		}
		return true;
	}

	bool LLPEAPI ProcessFunctionList(const char* pdbFile = "") {
		std::string pdbStr = std::string(pdbFile);
		if (!InitializeFunctionList(pdbStr, "")) {
			std::cout << "[Error][LLPeService] Error processing function list from PDB!" << std::endl;
			return false;
		}
		return true;
	}

	bool LLPEAPI CreateSymbolList(const char* symFileName, const char* pdbFile) {
		if (!InitializeFunctionList(pdbFile, symFileName, true)) {
			std::cout << "[Error][LLPeService] Error processing function list from PDB!" << std::endl;
			return false;
		}
		return true;
	}

	bool LLPEAPI ProcessLibFile(const char* libName, const char* modExeName) {
		std::string LiteModExe = modExeName != "" ? std::string(modExeName) : "bedrock_server_mod.exe";
		std::ifstream libFile;
		std::ofstream outLibFile;

		libFile.open(libName, std::ios::in | std::ios::binary);
		if (!libFile) {
			std::cout << "[Error] Cannot open " << libName << std::endl;
			return false;
		}
		bool saveFlag = false;
		pe_base* LiteLib_PE = new pe_base(pe_factory::create_pe(libFile));
		try {
			imported_functions_list imports(get_imported_functions(*LiteLib_PE));
			for (int i = 0; i < imports.size(); i++) {
				if (imports[i].get_name() == "bedrock_server_mod.exe") {
					imports[i].set_name(std::string(LiteModExe));
					std::cout << "[LLPeService] Modding dll file " << libName << std::endl;
					saveFlag = true;
				}
			}

			if (saveFlag) {
				section ImportSection;
				ImportSection.get_raw_data().resize(1);
				ImportSection.set_name("ImpFunc");
				ImportSection.readable(true).writeable(true);
				section& attachedImportedSection = LiteLib_PE->add_section(ImportSection);
				pe_bliss::rebuild_imports(*LiteLib_PE, imports, attachedImportedSection, import_rebuilder_settings(true, false));

				outLibFile.open("LiteLoader.tmp", std::ios::out | std::ios::binary | std::ios::trunc);
				if (!outLibFile) {
					std::cout << "[Error] Cannot create LiteLoader.tmp!" << std::endl;
					return false;
				}
				std::cout << "[LLPeService] Writting dll file " << libName << std::endl;
				pe_bliss::rebuild_pe(*LiteLib_PE, outLibFile);
				libFile.close();
				std::filesystem::remove(std::filesystem::path(libName));
				outLibFile.close();
				std::filesystem::rename(std::filesystem::path("LiteLoader.tmp"), std::filesystem::path(libName));
			}
			if (libFile.is_open()) libFile.close();
			if (outLibFile.is_open()) outLibFile.close();
		}
		catch (const pe_exception& e) {
			std::cout << "[Err] Set ImportName Failed: " << e.what() << std::endl;
			return false;
		}
		return true;
	}

	bool LLPEAPI ProcessLibDirectory(const char* directoryName, const char* modExeName = "") {
		std::string LiteModExe = modExeName != "" ? modExeName : "bedrock_server_mod.exe";
		std::filesystem::directory_iterator directory(directoryName);

		for (auto& file : directory) {
			if (!file.is_regular_file()) {
				continue;
			}
			std::filesystem::path path = file.path();
			std::u8string u8Name = path.filename().u8string();
			std::u8string u8Ext = path.extension().u8string();
			std::string name = reinterpret_cast<std::string&>(u8Name);
			std::string ext = reinterpret_cast<std::string&>(u8Ext);

			// Check is dll
			if (ext != ".dll") {
				continue;
			}

			std::string pluginName;
			pluginName.append(directoryName).append("\\").append(name);
			if (!ProcessLibFile(pluginName.c_str(), LiteModExe.c_str())) return false;
		}
		return true;
	}

	bool LLPEAPI ProcessPlugins(const char* modExeName) {
		std::string LiteModExeName = modExeName != "" ? modExeName : "bedrock_server_mod.exe";
		if (ProcessLibFile("LiteLoader.dll", modExeName) && ProcessLibDirectory("plugins", modExeName) && ProcessLibDirectory("plugins\\LiteLoader", modExeName)) {
			return true;
		}
		return false;
	}

	bool LLPEAPI GenerateDefinitionFiles(const char* pdbName, const char* apiName, const char* varName) {
		std::cout << "[LLPeService] Generating definition files... Please wait." << std::endl;
		std::string ApiFileName = apiName != "" ? std::string(apiName) : "bedrock_server_api.def";
		std::string VarFileName = varName != "" ? std::string(varName) : "bedrock_server_var.def";
		std::string PdbFileName = pdbName != "" ? std::string(pdbName) : "bedrock_server.pdb";
		std::ofstream BDSDef_API;
		std::ofstream BDSDef_VAR;
		if(!InitializeFunctionList(PdbFileName, "")) {
			return false;
		}

		BDSDef_API.open(ApiFileName, std::ios::ate | std::ios::out);
		if (!BDSDef_API) {
			std::cout << "[Error][LLPeService] Cannot create bedrock_server_api.def" << std::endl;
			return false;
		}
		BDSDef_API << "LIBRARY bedrock_server.dll\nEXPORTS\n";

		BDSDef_VAR.open(VarFileName, std::ios::ate | std::ios::out);
		if (!BDSDef_VAR) {
			std::cout << "[Error][LLPeService] Failed to create " << VarFileName << std::endl;
			return false;
		}
		BDSDef_VAR << "LIBRARY " << "bedrock_server.exe" << "\nEXPORTS\n";
		for (auto& fn : FilteredFunctionList) {
			if (fn.IsFunction) {
				BDSDef_API << "\t" << fn.Name << "\n";
			}
			else
				BDSDef_VAR << "\t" << fn.Name << "\n";
		}
		try {
			BDSDef_API.flush();
			BDSDef_API.close();
			BDSDef_VAR.flush();
			BDSDef_VAR.close();
			std::cout << "[LLPeService] [DEV]bedrock_server_var/api def    Created" << std::endl;
		}
		catch (...) {
			std::cout << "[Error][LLPeService] Failed to Create " << ApiFileName << std::endl;
			return false;
		}
		return true;
	}

	bool LLPEAPI CreateModifiedExecutable(const char* bedrockName, const char* liteModName, const char* pdbName) {
		std::string bedrockExeName = bedrockName != "" ? bedrockName : "bedrock_server.exe";
		std::string liteModExeName = liteModName != "" ? liteModName : "bedrock_server_mod.exe";
		std::string bedrockPdbName = pdbName != "" ? pdbName : "bedrock_server.pdb";
		std::ifstream             OriginalBDS;
		std::ofstream             ModifiedBDS;
		pe_base* OriginalBDS_PE = nullptr;
		pe_base* LiteLib_PE = nullptr;
		export_info                OriginalBDS_ExportInf;
		exported_functions_list         OriginalBDS_ExportFunc;
		uint16_t                  ExportLimit = 0;
		int                       ExportCount = 1;
		int                       ApiDefCount = 1;
		int                       ApiDefFileCount = 1;

		if (!InitializeFunctionList(bedrockPdbName, "")) {
			return false;
		}
		OriginalBDS.open(bedrockExeName, std::ios::in | std::ios::binary);
		if (!OriginalBDS) {
			std::cout << "[Error][LLPeService] Cannot open bedrock_server.exe" << std::endl;
			return false;
		}
		ModifiedBDS.open(liteModExeName, std::ios::out | std::ios::binary | std::ios::trunc);
		if (!ModifiedBDS) {
			std::cout << "[Error][LLPeService] Cannot open " << LiteModExeName << std::endl;
			return false;
		}

		OriginalBDS_PE = new pe_base(pe_factory::create_pe(OriginalBDS));
		try {
			OriginalBDS_ExportFunc = get_exported_functions(*OriginalBDS_PE, OriginalBDS_ExportInf);
		}
		catch (const pe_exception& e) {
			std::cout << "[Error][pe_bliss] " << e.what() << std::endl;
			return false;
		}
		ExportLimit = get_export_ordinal_limits(OriginalBDS_ExportFunc).second;

		for (const auto& fn : FilteredFunctionList) {
			try {
				if (!fn.IsFunction) {
					exported_function func;
					func.set_name(fn.Name);
					func.set_rva(fn.Rva);
					func.set_ordinal(ExportLimit + ExportCount);
					ExportCount++;
					if (ExportCount > 65535) {
						std::cout << "[Error][pe_bliss] Too many Symbols are going to insert to ExportTable" << std::endl;
						return false;
					}
					OriginalBDS_ExportFunc.push_back(func);
				}
			}
			catch (const pe_exception& e) {
				std::cout << "[Error][pe_bliss] " << e.what() << std::endl;
				return false;
			}
			catch (...) {
				std::cout << "UnkErr" << std::endl;
				return false;
			}
		}
		try {
			section ExportSection;
			ExportSection.get_raw_data().resize(1);
			ExportSection.set_name("ExpFunc");
			ExportSection.readable(true);
			section& attachedExportedSection = OriginalBDS_PE->add_section(ExportSection);
			rebuild_exports(*OriginalBDS_PE, OriginalBDS_ExportInf, OriginalBDS_ExportFunc, attachedExportedSection);

			imported_functions_list imports(get_imported_functions(*OriginalBDS_PE));

			import_library preLoader;
			preLoader.set_name("LLPreLoader.dll");

			imported_function func;
			func.set_name("dlsym_real");
			func.set_iat_va(0x1);

			preLoader.add_import(func);
			imports.push_back(preLoader);

			section ImportSection;
			ImportSection.get_raw_data().resize(1);
			ImportSection.set_name("ImpFunc");
			ImportSection.readable(true).writeable(true);
			section& attachedImportedSection = OriginalBDS_PE->add_section(ImportSection);
			rebuild_imports(*OriginalBDS_PE, imports, attachedImportedSection, import_rebuilder_settings(true, false));

			rebuild_pe(*OriginalBDS_PE, ModifiedBDS);
			ModifiedBDS.close();
			std::cout << "[LLPeService] LiteLoader modifed executable has been created!" << std::endl;
			return true;
		}
		catch (pe_exception e) {
			std::cout << "[Error][LLPeService] Failed to rebuild " << LiteModExeName << std::endl;
			std::cout << "[Error][pe_bliss] " << e.what() << std::endl;
			ModifiedBDS.close();
			std::filesystem::remove(std::filesystem::path(LiteModExeName));
			return false;
		}
		catch (...) {
			std::cout << "[Error][LLPeService] Failed to rebuild " << LiteModExeName << " with unk err" << std::endl;
			ModifiedBDS.close();
			std::filesystem::remove(std::filesystem::path(LiteModExeName));
			return false;
		}
		return true;
	}

	int LLPEAPI GetFilteredFunctionListCount(const char* pdbName) {
		std::string bedrockPdbName = pdbName != "" ? pdbName : "bedrock_server.pdb";
		if (FilteredFunctionList.size() == 0) {
			if (InitializeFunctionList(pdbName, "")) {
				return 0;
			}
		}
		return (int)FilteredFunctionList.size();
	}
}