namespace DocGen.Pdf.Parsing;

internal abstract class SystemFontFontSource
{
	public abstract string FontFamily { get; }

	public abstract bool IsBold { get; }

	public abstract bool IsItalic { get; }

	public abstract short Ascender { get; }

	public abstract short Descender { get; }

	public SystemFontFontSource()
	{
	}

	internal static SystemFontFontType GetFontType(SystemFontOpenTypeFontReader reader)
	{
		SystemFontFontType result = SystemFontFontType.Unknown;
		reader.BeginReadingBlock();
		uint num = reader.ReadULong();
		reader.EndReadingBlock();
		reader.BeginReadingBlock();
		double num2 = reader.ReadFixed();
		reader.EndReadingBlock();
		if (num == SystemFontTags.TRUE_TYPE_COLLECTION)
		{
			result = SystemFontFontType.TrueTypeCollection;
		}
		else if (num2 == 1.0)
		{
			result = SystemFontFontType.TrueType;
		}
		return result;
	}

	public virtual void GetAdvancedWidth(SystemFontGlyph glyph)
	{
	}

	public virtual void GetGlyphOutlines(SystemFontGlyph glyph, double fontSize)
	{
	}
}
