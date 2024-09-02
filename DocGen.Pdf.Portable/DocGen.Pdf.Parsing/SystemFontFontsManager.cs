using System;
using System.Collections.Generic;
using DocGen.Drawing;

namespace DocGen.Pdf.Parsing;

internal class SystemFontFontsManager
{
	private const string RegistryFontPath = "Software\\Microsoft\\Windows NT\\CurrentVersion\\Fonts";

	private readonly Dictionary<SystemFontFontDescriptor, SystemFontOpenTypeFontSourceBase> fonts;

	public SystemFontFontsManager()
	{
		fonts = new Dictionary<SystemFontFontDescriptor, SystemFontOpenTypeFontSourceBase>();
	}

	internal SystemFontOpenTypeFontSourceBase GetFontSource(SystemFontFontDescriptor descr)
	{
		string fontFamily = descr.FontFamily;
		FontStyle fontStyle = descr.FontStyle;
		if (!fonts.TryGetValue(descr, out SystemFontOpenTypeFontSourceBase value))
		{
			value = GetOpenTypeFontSource(fontFamily, fontStyle);
			fonts[descr] = value;
		}
		return value;
	}

	internal SystemFontOpenTypeFontSource GetOpenTypeFontSource(string fontName, FontStyle style)
	{
		if (string.IsNullOrEmpty(fontName))
		{
			fontName = "Arial";
		}
		fontName = fontName.ToLower();
		if ((style & FontStyle.Bold) == FontStyle.Bold)
		{
			fontName += " Bold";
		}
		if ((style & FontStyle.Italic) == FontStyle.Italic)
		{
			fontName += " Italic";
		}
		_ = string.Empty;
		return new SystemFontOpenTypeFontSource(null);
	}

	internal string FontFallback(string fontFamily)
	{
		fontFamily = fontFamily.ToLower();
		fontFamily.Split(new string[4] { " ", ",", "(", ")" }, StringSplitOptions.RemoveEmptyEntries);
		return "arial.ttf";
	}
}
