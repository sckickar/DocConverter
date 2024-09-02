using System;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.I1)]
internal unsafe delegate bool UnicodeDecomposeProxyDelegate(IntPtr ufuncs, uint ab, uint* a, uint* b, void* user_data);
