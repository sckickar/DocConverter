using System;
using System.Runtime.InteropServices;

namespace SkiaSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal unsafe delegate IntPtr SKManagedStreamGetLengthProxyDelegate(IntPtr s, void* context);
