using System;
using System.Runtime.InteropServices;

namespace SkiaSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal unsafe delegate void SKGlyphPathProxyDelegate(IntPtr pathOrNull, SKMatrix* matrix, void* context);
