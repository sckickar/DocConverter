using System;
using SkiaSharp;

namespace DocGen.Drawing.SkiaSharpHelper;

internal class FontFamily : IFontFamily
{
	private SKPaint font = new SKPaint();

	public string FontFamilyName => font.Typeface.FamilyName;

	internal string Name => font.Typeface.FamilyName;

	public FontFamily(SKTypeface typeface, float fontSize)
	{
		font.Typeface = typeface;
		font.TextSize = fontSize;
	}

	public FontFamily(string fontName)
	{
		font.Typeface = SKTypeface.FromFamilyName(fontName);
	}

	public FontFamily(string fontName, float fontSize)
	{
		font.Typeface = SKTypeface.FromFamilyName(fontName);
		font.TextSize = fontSize;
	}

	public FontFamily(SKTypeface typeface)
	{
		font.Typeface = typeface;
	}

	public bool IsStyleAvailable(FontStyle style)
	{
		switch (style)
		{
		case FontStyle.Bold:
			return font.Typeface.Style == SKTypefaceStyle.Bold;
		case FontStyle.Italic:
			return font.Typeface.Style == SKTypefaceStyle.Italic;
		case (FontStyle)3:
			return font.Typeface.Style == SKTypefaceStyle.BoldItalic;
		case FontStyle.Regular:
			return font.Typeface.Style == SKTypefaceStyle.Normal;
		case FontStyle.Underline:
		case FontStyle.Strikeout:
			return false;
		default:
			return false;
		}
	}

	public float GetEmHeight(FontStyle style)
	{
		return font.Typeface.UnitsPerEm;
	}

	public float GetCellAscent(FontStyle style)
	{
		return Math.Abs(font.FontMetrics.Ascent);
	}

	public float GetCellDescent(FontStyle style)
	{
		return font.FontMetrics.Descent;
	}

	public float GetLineSpacing(FontStyle style)
	{
		SKFontMetrics metrics = default(SKFontMetrics);
		return font.GetFontMetrics(out metrics);
	}
}
