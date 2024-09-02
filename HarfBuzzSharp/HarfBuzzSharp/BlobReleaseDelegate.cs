using System;
using System.ComponentModel;

namespace HarfBuzzSharp;

[EditorBrowsable(EditorBrowsableState.Never)]
[Obsolete("Use ReleaseDelegate instead.")]
public delegate void BlobReleaseDelegate(object context);
