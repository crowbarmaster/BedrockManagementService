#pragma once
#include "pch.h"
#include <Windows.h>

namespace LLPE {
	bool LLPEAPI ProcessFunctionList(const char* workingDir, const char* pdbFile);
	bool LLPEAPI CreateSymbolList(const char* workingDir, const char* symFileName, const char* pdbFile);
	bool LLPEAPI ProcessLibFile(const char* workingDir, const char* libName, const char* modExeName);
	bool LLPEAPI ProcessLibDirectory(const char* directoryName, const char* modExeName);
	bool LLPEAPI ProcessPlugins(const char* workingDir, const char* modExeName);
	bool LLPEAPI GenerateDefinitionFiles(const char* workingDir, const char* pdbName, const char* apiName, const char* varName);
	bool LLPEAPI CreateModifiedExecutable(const char* workingDir, const char* bedrockName, const char* liteModName, const char* pdbName);
	int  LLPEAPI GetFilteredFunctionListCount(const char* workingDir, const char* pdbName);
}
