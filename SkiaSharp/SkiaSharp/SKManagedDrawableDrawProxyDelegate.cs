using System;
using System.Runtime.InteropServices;

namespace SkiaSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal unsafe delegate void SKManagedDrawableDrawProxyDelegate(IntPtr d, void* context, IntPtr ccanvas);
