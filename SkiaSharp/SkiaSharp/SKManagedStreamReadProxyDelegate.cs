using System;
using System.Runtime.InteropServices;

namespace SkiaSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal unsafe delegate IntPtr SKManagedStreamReadProxyDelegate(IntPtr s, void* context, void* buffer, IntPtr size);
