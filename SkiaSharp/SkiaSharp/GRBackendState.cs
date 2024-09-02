using System;

namespace SkiaSharp;

[Flags]
public enum GRBackendState : uint
{
	None = 0u,
	All = uint.MaxValue
}
