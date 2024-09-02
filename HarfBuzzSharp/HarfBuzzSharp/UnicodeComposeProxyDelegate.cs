using System;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.I1)]
internal unsafe delegate bool UnicodeComposeProxyDelegate(IntPtr ufuncs, uint a, uint b, uint* ab, void* user_data);
