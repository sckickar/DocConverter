namespace DocGen.Pdf.Parsing;

internal class SystemFontNameFormat1 : SystemFontSystemFontName
{
	public override string FontFamily => "";

	public SystemFontNameFormat1(SystemFontOpenTypeFontSourceBase fontSource)
		: base(fontSource)
	{
	}

	internal override string ReadName(ushort languageID, ushort nameID)
	{
		return "";
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
	}
}
