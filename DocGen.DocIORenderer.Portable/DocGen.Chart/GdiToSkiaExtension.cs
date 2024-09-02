using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal static class GdiToSkiaExtension
{
	internal static string GetFontName(this Font font)
	{
		return font.FontFamilyName;
	}

	internal static FontFamily GetFontFamily(this Font font)
	{
		return new FontFamily(font.FontFamilyName);
	}
}
