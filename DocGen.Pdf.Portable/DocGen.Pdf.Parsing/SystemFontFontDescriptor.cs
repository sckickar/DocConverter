using DocGen.Drawing;

namespace DocGen.Pdf.Parsing;

internal class SystemFontFontDescriptor
{
	private readonly string fontFamily;

	private readonly FontStyle fontStyle;

	public string FontFamily => fontFamily;

	public FontStyle FontStyle => fontStyle;

	public SystemFontFontDescriptor(string fontFamily)
		: this(fontFamily, FontStyle.Regular)
	{
	}

	public SystemFontFontDescriptor(string fontFamily, FontStyle fontStyle)
	{
		this.fontFamily = fontFamily;
		this.fontStyle = fontStyle;
	}

	public static bool operator ==(SystemFontFontDescriptor left, SystemFontFontDescriptor right)
	{
		return left?.Equals(right) ?? ((object)right == null);
	}

	public static bool operator !=(SystemFontFontDescriptor left, SystemFontFontDescriptor right)
	{
		return !(left == right);
	}

	private static FontStyle GetFontStyle(string styles, FontStyle baseStyle)
	{
		styles = styles.ToLower();
		if (styles.Contains("italic"))
		{
			return FontStyle.Italic;
		}
		return baseStyle;
	}

	public override int GetHashCode()
	{
		return (17 * 23 + ((FontFamily != null) ? FontFamily.GetHashCode() : 0)) * 23 + FontStyle.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		SystemFontFontDescriptor systemFontFontDescriptor = obj as SystemFontFontDescriptor;
		if (!(systemFontFontDescriptor == null) && FontFamily == systemFontFontDescriptor.FontFamily)
		{
			return FontStyle == systemFontFontDescriptor.FontStyle;
		}
		return false;
	}

	public override string ToString()
	{
		return FontFamily;
	}
}
