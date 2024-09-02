using System;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.I1)]
internal unsafe delegate bool FontGetGlyphOriginProxyDelegate(IntPtr font, void* font_data, uint glyph, int* x, int* y, void* user_data);
