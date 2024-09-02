using System;
using System.Runtime.InteropServices;

namespace SkiaSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal unsafe delegate void SKManagedTraceMemoryDumpDumpStringValueProxyDelegate(IntPtr d, void* context, void* dumpName, void* valueName, void* value);
