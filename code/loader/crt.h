// ReSharper disable CppClangTidyReadabilityRedundantDeclaration
// ReSharper disable IdentifierTypo
/*
 * Custom implementation for common C runtime functions
 * This makes the DLL essentially freestanding on Windows without having to rely on msvcrt.dll
 */
#pragma once

#include <windows.h>

#pragma function(wcslen)
inline size_t wcslen(wchar_t const* str) {
    size_t result = 0;
    while (*str++) result++;
    return result;
}

#pragma function(memset)
inline void* memset(void* dst, int c, size_t n) {
    char* d = dst;
    while (n--)
        *d++ = (char)c;
    return dst;
}

inline void *malloc(size_t size) { return HeapAlloc(GetProcessHeap(), HEAP_GENERATE_EXCEPTIONS, size); }
inline void *calloc(size_t num, size_t size) { return HeapAlloc(GetProcessHeap(), HEAP_ZERO_MEMORY, size * num); }
inline void free(void *mem) { HeapFree(GetProcessHeap(), 0, mem); }
