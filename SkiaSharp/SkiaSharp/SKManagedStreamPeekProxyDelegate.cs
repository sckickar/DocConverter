using System;
using System.Runtime.InteropServices;

namespace SkiaSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal unsafe delegate IntPtr SKManagedStreamPeekProxyDelegate(IntPtr s, void* context, void* buffer, IntPtr size);
