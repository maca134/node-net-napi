setlocal enabledelayedexpansion
for /f "usebackq tokens=*" %%i in (`bin\vswhere -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do (
	set MSBUILD="%%i"
)
%MSBUILD% managed-src\NetNapi.sln /property:Configuration=Release /property:Platform=x64 /target:Rebuild /verbosity:normal /nologo
%MSBUILD% managed-src\NetNapi.sln /property:Configuration=Release /property:Platform=x86 /target:Rebuild /verbosity:normal /nologo
rmdir /s /q managed
mkdir managed
copy managed-src\NetNapi.Managed\bin\x86\Release\net-napi.net.dll managed\net-napi.net_x86.dll
copy managed-src\NetNapi.Managed\bin\x64\Release\net-napi.net.dll managed\net-napi.net_x64.dll
