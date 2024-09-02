using System;
using System.ComponentModel;

namespace SkiaSharp;

[EditorBrowsable(EditorBrowsableState.Never)]
[Obsolete("Use GRGlGetProcedureAddressDelegate instead.")]
public delegate IntPtr GRGlGetProcDelegate(object context, string name);
