using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace NetNapi.Managed
{
    public class ClrLoader
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport("node.exe", EntryPoint = "napi_get_cb_info")]
        public static extern int napi_get_cb_info(IntPtr env, IntPtr cbinfo, ref int argc, [In, Out] IntPtr[] argv, out IntPtr thisArg, out IntPtr data);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("node.exe", EntryPoint = "napi_get_value_string_utf16")]
        public static extern int napi_get_value_string_utf16(IntPtr env, IntPtr value, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder output, int size, out int result);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("node.exe", EntryPoint = "napi_typeof")]
        public static extern int napi_typeof(IntPtr env, IntPtr val, out int type);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("node.exe", EntryPoint = "napi_throw_error")]
        public static extern int napi_throw_error(IntPtr env, [MarshalAs(UnmanagedType.LPStr)] string code, [MarshalAs(UnmanagedType.LPStr)] string message);

       	[DllExport("clrLoader")]
        public static IntPtr Load(IntPtr envPtr, IntPtr infoPtr)
        {
            var argc = 0;
            var argv = new IntPtr[0];
            var status = napi_get_cb_info(envPtr, infoPtr, ref argc, argv, out _, out _);
            if (status != 0)
            {
                napi_throw_error(envPtr, null, $"napi_status: {status}");
                return IntPtr.Zero;
            }
            argv = new IntPtr[argc];
            status = napi_get_cb_info(envPtr, infoPtr, ref argc, argv, out _, out _);
            if (status != 0)
            {
                napi_throw_error(envPtr, null, $"napi_status: {status}");
                return IntPtr.Zero;
            }
            if (argc != 3)
            {
                napi_throw_error(envPtr, null, "wrong argument count");
                return IntPtr.Zero;
            }

            var args = new string[3];
            for (var i = 0; i < 3; i++)
            {
                status = napi_typeof(envPtr, argv[i], out var type);
                if (status != 0)
                {
                    napi_throw_error(envPtr, null, $"napi_status: {status}");
                    return IntPtr.Zero;
                }
                if (type != 4)
                {
                    napi_throw_error(envPtr, null, $"arg {i} is the wrong type");
                    return IntPtr.Zero;
                }
                napi_get_value_string_utf16(envPtr, argv[i], null, 0, out var length);
                var buf = new StringBuilder(length + 1);
                napi_get_value_string_utf16(envPtr, argv[i], buf, length + 1, out _);
                args[i] = buf.ToString();
            }

            var assemblyPath = args[0];
            var typeName = args[1];
            var methodName = args[2];

            AppDomain.CurrentDomain.AssemblyResolve += (sender, eventArgs) =>
            {
                var name = new AssemblyName(eventArgs.Name).Name;
                var filename = Path.Combine(Path.GetDirectoryName(assemblyPath), name + ".dll");
                return File.Exists(filename) ? Assembly.LoadFile(filename) : null;
            };

            if (!File.Exists(assemblyPath))
            {
                napi_throw_error(envPtr, null, $"assembly {assemblyPath} not found");
                return IntPtr.Zero;
            }

            Assembly assembly;
            try
            {
                assembly = Assembly.LoadFile(assemblyPath);
            }
            catch (Exception ex)
            {
                napi_throw_error(envPtr, null, $"assembly {assemblyPath} error - {ex.Message}");
                return IntPtr.Zero;
            }

            Type assemblytype;
            try
            {
                assemblytype = assembly.GetType(typeName, true);
            }
            catch (Exception ex)
            {
                napi_throw_error(envPtr, null, $"type {typeName} error - {ex.Message}");
                return IntPtr.Zero;
            }

            if (assemblytype == null)
            {
                napi_throw_error(envPtr, null, $"type {typeName} not found");
                return IntPtr.Zero;
            }

            MethodInfo method;
            try
            {
                method = assemblytype.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            }
            catch (Exception ex)
            {
                napi_throw_error(envPtr, null, $"method {methodName} error - {ex.Message}");
                return IntPtr.Zero;
            }

            if (method == null)
            {
                napi_throw_error(envPtr, null, $"method {methodName} not found");
                return IntPtr.Zero;
            }

            return (IntPtr)method.Invoke(null, new object[] { envPtr });
        }
    }
}
