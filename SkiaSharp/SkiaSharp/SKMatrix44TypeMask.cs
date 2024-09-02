using System;

namespace SkiaSharp;

[Flags]
public enum SKMatrix44TypeMask
{
	Identity = 0,
	Translate = 1,
	Scale = 2,
	Affine = 4,
	Perspective = 8
}
