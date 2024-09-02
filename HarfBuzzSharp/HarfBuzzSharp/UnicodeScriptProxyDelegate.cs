using System;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal unsafe delegate uint UnicodeScriptProxyDelegate(IntPtr ufuncs, uint unicode, void* user_data);
