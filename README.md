# NetNapi

Load CLR (.NET) dlls into Node. Still working on finishing the Napi bindings...

You will need [node-gyp](https://github.com/nodejs/node-gyp) up and running to compile this.

This uses [DllExport](https://github.com/3F/DllExport) to get CLR loaded.

Supports 32bit and 64bit.

**Windows Only** - until Core supports DLL exports better (it maybe possible to do this now, not sure)

## Installation

Install by `npm`

```sh
npm install --save @maca134/node-net-napi
```

**or** install with `yarn`

```sh
yarn add @maca134/node-net-napi
```

### Example

```typescript
import { loadClr } from '@maca134/node-net-napi';

const res = loadClr(
	'ExampleDll.dll',
	'ExampleDll.SomethingUseful',
	'Init'
);
console.log(res);
/*
{ str: 'hello World' }
*/
```

```csharp
namespace ExampleDll
{
    public static class SomethingUseful
    {
        public const ulong NapiAutoLength = ulong.MaxValue;

	[DllImport("node.exe", EntryPoint = "napi_create_string_utf8")]
        public static extern int napi_create_string_utf8(IntPtr env, [MarshalAs(UnmanagedType.LPStr)] string name, ulong length, out IntPtr res);

        [DllImport("node.exe", EntryPoint = "napi_create_object")]
        public static extern int napi_create_object(IntPtr env, out IntPtr res);

        [DllImport("node.exe", EntryPoint = "napi_set_named_property")]
        public static extern int napi_set_named_property(IntPtr env, IntPtr obj, [MarshalAs(UnmanagedType.LPStr)] string name, IntPtr val);
        		
        public static IntPtr Init(IntPtr env)
        {
            napi_create_object(env, out var obj);
            napi_create_string_utf8(env, "hello World", NapiAutoLength, out var str);
            napi_set_named_property(env, obj, "str", str);
            return obj;
        }
    }
}
```
