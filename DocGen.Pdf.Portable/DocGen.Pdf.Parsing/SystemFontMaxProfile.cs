namespace DocGen.Pdf.Parsing;

internal class SystemFontMaxProfile : SystemFontTrueTypeTableBase
{
	internal override uint Tag => SystemFontTags.MAXP_TABLE;

	public float Version { get; private set; }

	public ushort NumGlyphs { get; private set; }

	public SystemFontMaxProfile(SystemFontOpenTypeFontSourceBase fontFile)
		: base(fontFile)
	{
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		Version = reader.ReadFixed();
		NumGlyphs = reader.ReadUShort();
	}
}
