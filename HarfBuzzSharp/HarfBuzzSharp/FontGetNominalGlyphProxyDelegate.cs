using System;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.I1)]
internal unsafe delegate bool FontGetNominalGlyphProxyDelegate(IntPtr font, void* font_data, uint unicode, uint* glyph, void* user_data);
