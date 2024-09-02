using System.Runtime.InteropServices;

namespace HarfBuzzSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal unsafe delegate void DestroyProxyDelegate(void* user_data);
