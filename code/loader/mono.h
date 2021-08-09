#pragma once

#include <windows.h>

void* (*mono_thread_attach)(void*);
void* (*mono_get_root_domain)();
void* (*mono_domain_get)();

void * (*mono_thread_current)();
void (*mono_thread_set_main)(void *);
void *(*mono_jit_init_version)(const char *root_domain_name, const char *runtime_version);
void *(*mono_domain_assembly_open)(void *domain, const char *name);
void *(*mono_assembly_get_image)(void *assembly);
void *(*mono_runtime_invoke)(void *method, void *obj, void **params, void **exc);

void *(*mono_method_desc_new)(const char *name, int include_namespace);
void* (*mono_method_desc_search_in_image)(void* desc, void* image);
void *(*mono_method_desc_search_in_class)(void *desc, void *klass);
void (*mono_method_desc_free)(void *desc);
void *(*mono_method_signature)(void *method);
UINT32 (*mono_signature_get_param_count)(void *sig);

void (*mono_domain_set_config)(void *domain, char *base_dir, char *config_file_name);
void *(*mono_array_new)(void *domain, void *eclass, uintptr_t n);
void *(*mono_get_string_class)();

char *(*mono_assembly_getrootdir)();

inline void load_mono_functions(HMODULE mono_lib) {

#define GET_MONO_PROC(name) name = (void*)GetProcAddress(mono_lib, #name)

    GET_MONO_PROC(mono_thread_attach);
    GET_MONO_PROC(mono_get_root_domain);
    GET_MONO_PROC(mono_domain_get);

    GET_MONO_PROC(mono_domain_assembly_open);
    GET_MONO_PROC(mono_assembly_get_image);
    GET_MONO_PROC(mono_runtime_invoke);
    GET_MONO_PROC(mono_jit_init_version);
    GET_MONO_PROC(mono_method_desc_new);
    GET_MONO_PROC(mono_method_desc_search_in_class);
    GET_MONO_PROC(mono_method_desc_search_in_image);
    GET_MONO_PROC(mono_method_desc_free);
    GET_MONO_PROC(mono_method_signature);
    GET_MONO_PROC(mono_signature_get_param_count);
    GET_MONO_PROC(mono_array_new);
    GET_MONO_PROC(mono_get_string_class);
    GET_MONO_PROC(mono_assembly_getrootdir);
    GET_MONO_PROC(mono_thread_current);
    GET_MONO_PROC(mono_thread_set_main);
    GET_MONO_PROC(mono_domain_set_config);

#undef GET_MONO_PROC
}
