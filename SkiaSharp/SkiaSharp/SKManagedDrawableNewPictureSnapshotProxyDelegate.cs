using System;
using System.Runtime.InteropServices;

namespace SkiaSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal unsafe delegate IntPtr SKManagedDrawableNewPictureSnapshotProxyDelegate(IntPtr d, void* context);
