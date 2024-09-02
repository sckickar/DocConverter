using System;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.I1)]
internal unsafe delegate bool FontGetFontExtentsProxyDelegate(IntPtr font, void* font_data, FontExtents* extents, void* user_data);
