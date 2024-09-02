using System;
using System.Runtime.InteropServices;

namespace SkiaSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal unsafe delegate void SKManagedDrawableGetBoundsProxyDelegate(IntPtr d, void* context, SKRect* rect);
