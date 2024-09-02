using System;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal unsafe delegate uint UnicodeEastasianWidthProxyDelegate(IntPtr ufuncs, uint unicode, void* user_data);
