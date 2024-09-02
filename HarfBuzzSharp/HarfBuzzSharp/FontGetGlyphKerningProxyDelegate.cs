using System;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal unsafe delegate int FontGetGlyphKerningProxyDelegate(IntPtr font, void* font_data, uint first_glyph, uint second_glyph, void* user_data);
