using System;

namespace DocGen.Drawing.SkiaSharpHelper;

internal static class SystemFonts
{
	internal static Font DefaultFont
	{
		get
		{
			if (!Environment.OSVersion.VersionString.Contains("Windows"))
			{
				if (Environment.OSVersion.VersionString.Contains("Unix"))
				{
					return new Font("Tahoma", 8.25f);
				}
				return new Font("DejaVu Sans", 8.25f);
			}
			return new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0, gdiVerticalFont: false);
		}
	}
}
