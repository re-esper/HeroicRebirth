#pragma once

#ifdef _VERBOSE
#include <windows.h>
HANDLE log_handle;
char buffer[4096];
inline void logger_init() {
	log_handle = CreateFileA("unityproxy.log", GENERIC_WRITE, FILE_SHARE_READ, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
}
inline void logger_done() { CloseHandle(log_handle); }
#define LOG(message, ...) \
	{ \
		size_t len = wsprintfA(buffer, message, __VA_ARGS__); \
		WriteFile(log_handle, buffer, len, NULL, NULL); \
        WriteFile(log_handle, "\n", 1, NULL, NULL); \
	}
#else
inline void logger_init() {}
inline void logger_done() {}
#define LOG(message, ...)
#endif

#define ASSERT_F(test, message, ...)												\
	if(!(test))																		\
	{																				\
		wchar_t *buff = (wchar_t*)malloc(sizeof(wchar_t) * 1024);					\
		wsprintfW(buff, message, __VA_ARGS__);										\
		MessageBox(NULL, buff, L"UnityModLoader: Fatal", MB_OK | MB_ICONERROR);			\
		free(buff);																	\
		ExitProcess(EXIT_FAILURE);													\
	}
#define ASSERT_RET(test, ...)  if(!(test)) return __VA_ARGS__;

#include "crt.h"

#define ARRAY_SIZE(arr) (sizeof(arr) / sizeof(arr[0]))

inline wchar_t* wmemcpy(wchar_t* dst, const wchar_t* src, size_t n) {
    wchar_t* d = dst;
    const wchar_t* s = src;
    while (n--)
        *d++ = *s++;
    return dst;
}

inline void* wmemset(wchar_t* dst, wchar_t c, size_t n) {
    wchar_t* d = dst;
    while (n--)
        *d++ = c;
    return dst;
}

inline wchar_t* widen(const char* str) {
    const int req_size = MultiByteToWideChar(CP_UTF8, 0, str, -1, NULL, 0);
    wchar_t* result = (wchar_t*)malloc(req_size * sizeof(wchar_t));
    MultiByteToWideChar(CP_UTF8, 0, str, -1, result, req_size);
    return result;
}

inline char* narrow(const wchar_t* str) {
    const int req_size = WideCharToMultiByte(CP_UTF8, 0, str, -1, NULL, 0, NULL, NULL);
    char* result = (char*)malloc(req_size * sizeof(char));
    WideCharToMultiByte(CP_UTF8, 0, str, -1, result, req_size, NULL, NULL);
    return result;
}

inline size_t get_module_path(HMODULE module, wchar_t** result, size_t* size, size_t free_space) {
    DWORD i = 0;
    DWORD len, s;
    *result = NULL;
    do {
        if (*result != NULL)
            free(*result);
        i++;
        s = i * MAX_PATH + 1;
        *result = (wchar_t*)malloc(sizeof(wchar_t) * s);
        len = GetModuleFileNameW(module, *result, s);
    } while (GetLastError() == ERROR_INSUFFICIENT_BUFFER || s - len < free_space);

    if (size != NULL)
        *size = s;
    return len;
}

inline wchar_t* get_file_name(wchar_t* str, size_t len, BOOL ext) {
    size_t ext_index = len;
    size_t i;
    for (i = len; i > 0; i--) {
        wchar_t c = *(str + i);
        if (c == L'.' && ext_index == len)
            ext_index = i;
        else if (c == L'\\' || c == L'/')
            break;
    }

    const size_t result_len = (ext ? len : ext_index) - i;
    wchar_t* result = (wchar_t*)calloc(result_len, sizeof(wchar_t));
    wmemcpy(result, str + i + 1, result_len - 1);
    return result;
}

inline wchar_t* get_full_path(wchar_t* str) {
    const DWORD needed = GetFullPathNameW(str, 0, NULL, NULL);
    wchar_t* res = malloc(sizeof(wchar_t) * needed);
    GetFullPathNameW(str, needed, res, NULL);
    return res;
}