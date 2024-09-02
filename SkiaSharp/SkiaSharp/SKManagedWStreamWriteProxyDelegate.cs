using System;
using System.Runtime.InteropServices;

namespace SkiaSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.I1)]
internal unsafe delegate bool SKManagedWStreamWriteProxyDelegate(IntPtr s, void* context, void* buffer, IntPtr size);
