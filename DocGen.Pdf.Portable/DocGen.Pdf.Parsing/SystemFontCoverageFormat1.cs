namespace DocGen.Pdf.Parsing;

internal class SystemFontCoverageFormat1 : SystemFontCoverage
{
	public SystemFontCoverageFormat1(SystemFontOpenTypeFontSourceBase fontFile)
		: base(fontFile)
	{
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
	}

	public override int GetCoverageIndex(ushort glyphIndex)
	{
		return -1;
	}

	internal override void Write(SystemFontFontWriter writer)
	{
		writer.WriteUShort(1);
	}
}
