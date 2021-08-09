// dllmain.cpp : Defines the entry point for the DLL application.

#define WIN32_LEAN_AND_MEAN
#include <windows.h>

#define DEFAULT_TARGET_ASSEMBLY L"EsperToWP.dll"
//#define _VERBOSE

#include "utils.h"
#include "crt.h"

#include "proxy.h"
#include "mono.h"

void invoke(void* domain)
{
	wchar_t* target_assembly = get_full_path(DEFAULT_TARGET_ASSEMBLY);

	const int len = WideCharToMultiByte(CP_UTF8, 0, target_assembly, -1, NULL, 0, NULL, NULL);
	char* dll_path = malloc(sizeof(char) * len);
	WideCharToMultiByte(CP_UTF8, 0, target_assembly, -1, dll_path, len, NULL, NULL);

	LOG("Loading assembly: %s\n", dll_path);
	void* assembly = mono_domain_assembly_open(domain, dll_path);
	if (assembly == NULL) LOG("Failed to load assembly\n");
	free(dll_path);
	ASSERT_RET(assembly != NULL);

	// Get assembly's image that contains CIL code
	void* image = mono_assembly_get_image(assembly);
	ASSERT_RET(image != NULL);

	// Create a descriptor for a random Main method
	void* desc = mono_method_desc_new("*:Main", FALSE);
	void* method = mono_method_desc_search_in_image(desc, image);
	ASSERT_RET(method != NULL);

	void* signature = mono_method_signature(method);
	UINT32 params = mono_signature_get_param_count(signature);
	void** args = NULL;
	if (params == 1) {
		void* args_array = mono_array_new(domain, mono_get_string_class(), 0);
		args = malloc(sizeof(void*) * 1);
		args[0] = args_array;
	}

	LOG("Invoking method %p\n", method);
	void* exc = NULL;
	mono_runtime_invoke(method, NULL, args, &exc);

	mono_method_desc_free(desc);
	if (args != NULL) {
		free(args);
		args = NULL;
	}
	LOG("Done!\n");
}

#define WM_INVOKE	0x8000u
WNDPROC oldWndProc = NULL;
LRESULT APIENTRY SubclassProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	if (uMsg == WM_INVOKE) {
		invoke(mono_domain_get());
		return 0;
	}
	return CallWindowProc(oldWndProc, hwnd, uMsg, wParam, lParam);
}

#include <tlhelp32.h>
void work()
{
	HMODULE mono = NULL;
	MODULEENTRY32W me = { 0 };
	me.dwSize = sizeof(MODULEENTRY32W);
	while (TRUE)
	{
		mono = GetModuleHandleW(L"mono.dll");
		if (mono) break;
		DWORD pid = GetCurrentProcessId();
		HANDLE h = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE, pid);
		if (h != INVALID_HANDLE_VALUE)
		{			
			if (Module32FirstW(h, &me))
			{
				do {
					if (GetProcAddress(me.hModule, "mono_thread_attach") != NULL) {
						mono = me.hModule;
						break;
					}
				}
				while (Module32NextW(h, &me));
			}
			CloseHandle(h);
			if (mono) break;
		}
		Sleep(200);
	}

	load_mono_functions(mono);
	while (!mono_get_root_domain()) Sleep(200);
	
	HWND hwnd;
	while ((hwnd = FindWindowW(L"UnityWndClass", 0)) == NULL) Sleep(200);
	LOG("Unity window found %p", hwnd);
	oldWndProc = (WNDPROC)SetWindowLongW(hwnd, GWL_WNDPROC, (LONG)SubclassProc);
	LOG("SetWindowLongW %p", oldWndProc);
	PostMessageW(hwnd, WM_INVOKE, 0, 0);
}

void entry(HMODULE hModule)
{
	wchar_t* dll_path = NULL;
	const size_t dll_path_len = get_module_path(hModule, &dll_path, NULL, 0);
	LOG("DLL Path: %S", dll_path);
	wchar_t* dll_name = get_file_name(dll_path, dll_path_len, FALSE);
	LOG("Proxy DLL Name: %S", dll_name);
	load_proxy(dll_name);
	LOG("Proxy loaded");
	free(dll_name);
	free(dll_path);

	CloseHandle(CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)work, (LPVOID)NULL, 0, NULL));	
}

BOOL WINAPI DllEntry(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		logger_init();
		entry(hModule);
		break;
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
		break;
	case DLL_PROCESS_DETACH:
		logger_done();
		break;
	}
	return TRUE;
}

