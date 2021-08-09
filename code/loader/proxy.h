#pragma once

#include <windows.h>
#include <Shlwapi.h>
#include "utils.h"

#define ALT_POSTFIX L"_alt.dll"
#define DLL_POSTFIX L".dll"

extern FARPROC originalFunctions[];
extern void load_functions(HMODULE dll);


// Load the proxy functions into memory
inline void load_proxy(wchar_t *module_name) {
    const size_t module_name_len = wcslen(module_name);

    const size_t alt_name_len = module_name_len + ARRAY_SIZE(ALT_POSTFIX);
    wchar_t *alt_name = (wchar_t*)malloc(sizeof(wchar_t) * alt_name_len);
    wmemcpy(alt_name, module_name, module_name_len + 1);
    wmemcpy(alt_name + module_name_len, ALT_POSTFIX, ARRAY_SIZE(ALT_POSTFIX));

    wchar_t *dll_path = NULL; // The final DLL path

    const int alt_full_path_len = GetFullPathNameW(alt_name, 0, NULL, NULL);
    wchar_t *alt_full_path = (wchar_t*)malloc(sizeof(wchar_t) * alt_full_path_len);
    GetFullPathNameW(alt_name, alt_full_path_len, alt_full_path, NULL);
    free(alt_name);

    LOG("Looking for original DLL from %S\n", alt_full_path);

    // Try to look for the alternative first in the same directory.
    HMODULE handle = LoadLibrary(alt_full_path);

    if (handle == NULL) {
        const UINT system_dir_len = GetSystemDirectoryW(NULL, 0);
        dll_path = (wchar_t*)malloc(sizeof(wchar_t) * (system_dir_len + module_name_len + ARRAY_SIZE(DLL_POSTFIX)));
        GetSystemDirectoryW(dll_path, system_dir_len);
        dll_path[system_dir_len - 1] = L'\\';
        wmemcpy(dll_path + system_dir_len, module_name, module_name_len);
        wmemcpy(dll_path + system_dir_len + module_name_len, DLL_POSTFIX, ARRAY_SIZE(DLL_POSTFIX));

        LOG("Looking for original DLL from %S\n", dll_path);

        handle = LoadLibraryW(dll_path);
    }

    ASSERT_F(handle != NULL, L"Unable to load the original %s.dll (looked from system directory and from %s_alt.dll)!",
             module_name, module_name);

    free(alt_full_path);
    free(dll_path);

    load_functions(handle);
}
