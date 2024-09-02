namespace DocGen.Pdf.Parsing;

internal class SystemFontRangeRecord
{
	public ushort Start { get; private set; }

	public ushort End { get; private set; }

	public ushort StartCoverageIndex { get; private set; }

	public int GetCoverageIndex(ushort glyphIndex)
	{
		return StartCoverageIndex + glyphIndex - Start;
	}

	public void Read(SystemFontOpenTypeFontReader reader)
	{
		Start = reader.ReadUShort();
		End = reader.ReadUShort();
		StartCoverageIndex = reader.ReadUShort();
	}

	internal void Write(SystemFontFontWriter writer)
	{
		writer.WriteUShort(Start);
		writer.WriteUShort(End);
		writer.WriteUShort(StartCoverageIndex);
	}
}
