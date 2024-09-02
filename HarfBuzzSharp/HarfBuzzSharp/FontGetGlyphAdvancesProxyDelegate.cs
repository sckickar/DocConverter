using System;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal unsafe delegate void FontGetGlyphAdvancesProxyDelegate(IntPtr font, void* font_data, uint count, uint* first_glyph, uint glyph_stride, int* first_advance, uint advance_stride, void* user_data);
