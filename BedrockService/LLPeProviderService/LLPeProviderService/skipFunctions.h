#pragma once
#include <vector>
#include <string>
#include <regex>
namespace LLTool {
	const std::vector<std::string> SkipPerfix = {
		"_",
		"?__",
		"??_",
		"??@",
		"?$TSS",
		"??_C",
		"??3",
		"??2",
		"??_R4",
		"??_E",
		"??_G" };

	const std::vector<std::regex> SkipRegex = {
		std::regex(R"(\?+[a-zA-Z0-9_-]*([a-zA-Z0-9_-]*@)*std@@.*)", std::regex::icase),
		std::regex(R"(.*printf$)", std::regex::icase),
		std::regex(R"(.*no_alloc.*)", std::regex::icase) };
}