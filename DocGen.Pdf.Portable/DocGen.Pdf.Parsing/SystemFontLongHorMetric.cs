namespace DocGen.Pdf.Parsing;

internal class SystemFontLongHorMetric
{
	public ushort AdvanceWidth { get; private set; }

	public short LSB { get; private set; }

	public void Read(SystemFontOpenTypeFontReader reader)
	{
		AdvanceWidth = reader.ReadUShort();
		LSB = reader.ReadShort();
	}

	public void Write(SystemFontFontWriter writer)
	{
		writer.WriteUShort(AdvanceWidth);
		writer.WriteShort(LSB);
	}
}
