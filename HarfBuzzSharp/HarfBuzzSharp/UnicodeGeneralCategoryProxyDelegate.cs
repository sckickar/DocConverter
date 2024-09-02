using System;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal unsafe delegate int UnicodeGeneralCategoryProxyDelegate(IntPtr ufuncs, uint unicode, void* user_data);
