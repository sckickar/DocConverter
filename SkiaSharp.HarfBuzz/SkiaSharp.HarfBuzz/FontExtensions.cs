using System;
using HarfBuzzSharp;

namespace SkiaSharp.HarfBuzz;

public static class FontExtensions
{
	public static SKSizeI GetScale(this Font font)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		font.GetScale(out var xScale, out var yScale);
		return new SKSizeI(xScale, yScale);
	}

	public static void SetScale(this Font font, SKSizeI scale)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		font.SetScale(scale.Width, scale.Height);
	}
}
