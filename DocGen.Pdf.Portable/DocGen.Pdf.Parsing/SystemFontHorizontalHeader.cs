namespace DocGen.Pdf.Parsing;

internal class SystemFontHorizontalHeader : SystemFontTrueTypeTableBase
{
	internal override uint Tag => SystemFontTags.HHEA_TABLE;

	public short Ascender { get; private set; }

	public short Descender { get; private set; }

	public short LineGap { get; private set; }

	public ushort NumberOfHMetrics { get; private set; }

	public SystemFontHorizontalHeader(SystemFontOpenTypeFontSourceBase fontFile)
		: base(fontFile)
	{
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		reader.ReadFixed();
		Ascender = reader.ReadShort();
		Descender = reader.ReadShort();
		LineGap = reader.ReadShort();
		reader.ReadUShort();
		reader.ReadShort();
		reader.ReadShort();
		reader.ReadShort();
		reader.ReadShort();
		reader.ReadShort();
		reader.ReadShort();
		reader.ReadShort();
		reader.ReadShort();
		reader.ReadShort();
		reader.ReadShort();
		reader.ReadShort();
		NumberOfHMetrics = reader.ReadUShort();
	}

	internal override void Write(SystemFontFontWriter writer)
	{
		writer.WriteShort(Ascender);
		writer.WriteShort(Descender);
		writer.WriteShort(LineGap);
		writer.WriteUShort(NumberOfHMetrics);
	}

	internal override void Import(SystemFontOpenTypeFontReader reader)
	{
		Ascender = reader.ReadShort();
		Descender = reader.ReadShort();
		LineGap = reader.ReadShort();
		NumberOfHMetrics = reader.ReadUShort();
	}
}
