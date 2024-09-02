namespace DocGen.Pdf.Parsing;

internal class SystemFontPostFormat3 : SystemFontPost
{
	public SystemFontPostFormat3(SystemFontOpenTypeFontSourceBase fontSource)
		: base(fontSource)
	{
	}

	public override ushort GetGlyphId(string name)
	{
		return 0;
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
	}
}
