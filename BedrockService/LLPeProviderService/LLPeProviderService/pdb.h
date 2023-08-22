#pragma once

#include <string>
#include <deque>

struct PdbSymbol {
	std::string Name;
	uint32_t Rva;
	bool IsFunction;
	PdbSymbol(std::string mName, uint32_t mRva, bool mIsFunction) :
		Name(mName), Rva(mRva), IsFunction(mIsFunction) {}
};

std::deque<PdbSymbol>* loadPDB(const wchar_t* pdbPath);