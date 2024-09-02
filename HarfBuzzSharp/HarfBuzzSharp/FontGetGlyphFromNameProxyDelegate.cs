using System;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.I1)]
internal unsafe delegate bool FontGetGlyphFromNameProxyDelegate(IntPtr font, void* font_data, void* name, int len, uint* glyph, void* user_data);
