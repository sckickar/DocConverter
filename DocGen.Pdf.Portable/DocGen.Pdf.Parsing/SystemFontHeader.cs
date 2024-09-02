namespace DocGen.Pdf.Parsing;

internal class SystemFontHeader : SystemFontCFFTable
{
	public byte HeaderSize { get; set; }

	public byte OffSize { get; set; }

	public SystemFontHeader(SystemFontCFFFontFile file)
		: base(file, 0L)
	{
	}

	public override void Read(SystemFontCFFFontReader reader)
	{
		reader.ReadCard8();
		reader.ReadCard8();
		HeaderSize = reader.ReadCard8();
		OffSize = reader.ReadOffSize();
	}
}
