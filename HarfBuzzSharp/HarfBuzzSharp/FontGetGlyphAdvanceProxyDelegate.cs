using System;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal unsafe delegate int FontGetGlyphAdvanceProxyDelegate(IntPtr font, void* font_data, uint glyph, void* user_data);
