using System;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.I1)]
internal unsafe delegate bool FontGetGlyphNameProxyDelegate(IntPtr font, void* font_data, uint glyph, void* name, uint size, void* user_data);
