using System;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.I1)]
internal unsafe delegate bool FontGetGlyphExtentsProxyDelegate(IntPtr font, void* font_data, uint glyph, GlyphExtents* extents, void* user_data);
