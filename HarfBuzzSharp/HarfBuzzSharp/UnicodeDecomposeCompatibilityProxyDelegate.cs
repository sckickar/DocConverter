using System;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal unsafe delegate uint UnicodeDecomposeCompatibilityProxyDelegate(IntPtr ufuncs, uint u, uint* decomposed, void* user_data);
