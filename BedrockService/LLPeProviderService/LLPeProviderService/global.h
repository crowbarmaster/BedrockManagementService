#pragma once

#ifdef LLPEPROVIDERSERVICE_EXPORTS
#define LLPEAPI __declspec(dllexport)
#else
#define LLPEAPI __declspec(dllimport)
#endif
