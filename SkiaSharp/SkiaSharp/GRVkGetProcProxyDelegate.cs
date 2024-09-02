using System;
using System.Runtime.InteropServices;

namespace SkiaSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal unsafe delegate IntPtr GRVkGetProcProxyDelegate(void* ctx, void* name, IntPtr instance, IntPtr device);
