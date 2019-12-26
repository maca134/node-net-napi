#include <windows.h>
#include <node_api.h>
#include <stdio.h>

EXTERN_C IMAGE_DOS_HEADER __ImageBase;

typedef napi_value(__cdecl* NapiInitProc)(napi_env, napi_callback_info);

napi_value init(napi_env env, napi_value exports)
{
	WCHAR dllPathW[MAX_PATH] = { 0 };
	GetModuleFileNameW((HINSTANCE)&__ImageBase, dllPathW, _countof(dllPathW));
	char dllPath[1024];
	wcstombs(dllPath, dllPathW, sizeof(dllPath));
	char assemblyFile[1024];
	size_t len = strlen(dllPath);
	strncpy(assemblyFile, dllPath + 4, len - 9);
	assemblyFile[len - 9] = '\0';
#ifdef _WIN64
	strcat(assemblyFile, ".net_x64.dll");
#else
	strcat(assemblyFile, ".net_x86.dll");
#endif
	assemblyFile[len + 3] = '\0';

	HMODULE handle = LoadLibrary(assemblyFile);
	if (handle == NULL)
	{
		napi_throw_error(env, NULL, assemblyFile);
		return NULL;
	}
	NapiInitProc loadClr = (NapiInitProc)GetProcAddress(handle, "clrLoader");
	if (loadClr == NULL)
	{
		napi_throw_error(env, NULL, "init not found");
		return NULL;
	}
	
	napi_value fn;
	napi_status status = napi_create_function(env, "LoadClr", NAPI_AUTO_LENGTH, loadClr, NULL, &fn);
	if (status != napi_ok)
	{
		napi_throw_error(env, NULL, "error creating js fnc");
		return NULL;
	}

	return fn;
}

NAPI_MODULE(nodenetnapi, init)