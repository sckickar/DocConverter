using System;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal unsafe delegate IntPtr ReferenceTableProxyDelegate(IntPtr face, uint tag, void* user_data);
