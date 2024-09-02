using System;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal unsafe delegate int UnicodeCombiningClassProxyDelegate(IntPtr ufuncs, uint unicode, void* user_data);
