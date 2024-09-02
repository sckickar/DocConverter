using System;
using System.Runtime.InteropServices;

namespace SkiaSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal unsafe delegate void SKManagedWStreamDestroyProxyDelegate(IntPtr s, void* context);
