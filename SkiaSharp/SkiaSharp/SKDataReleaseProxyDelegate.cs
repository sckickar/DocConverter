using System.Runtime.InteropServices;

namespace SkiaSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal unsafe delegate void SKDataReleaseProxyDelegate(void* ptr, void* context);
