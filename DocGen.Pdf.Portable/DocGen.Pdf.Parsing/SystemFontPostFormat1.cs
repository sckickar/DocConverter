namespace DocGen.Pdf.Parsing;

internal class SystemFontPostFormat1 : SystemFontPost
{
	public SystemFontPostFormat1(SystemFontOpenTypeFontSourceBase fontSource)
		: base(fontSource)
	{
	}

	public override ushort GetGlyphId(string name)
	{
		if (SystemFontPost.macintoshStandardOrderGlyphIds.TryGetValue(name, out var value))
		{
			return value;
		}
		return 0;
	}
}
