using System;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal unsafe delegate uint FontGetNominalGlyphsProxyDelegate(IntPtr font, void* font_data, uint count, uint* first_unicode, uint unicode_stride, uint* first_glyph, uint glyph_stride, void* user_data);
