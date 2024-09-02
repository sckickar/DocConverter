using System;
using System.Runtime.InteropServices;

namespace SkiaSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal unsafe delegate void SKManagedTraceMemoryDumpDumpNumericValueProxyDelegate(IntPtr d, void* context, void* dumpName, void* valueName, void* units, ulong value);
