namespace DocGen.Pdf.Parsing;

internal class SystemFontTableRecord
{
	public uint Tag { get; set; }

	public uint CheckSum { get; set; }

	public uint Offset { get; set; }

	public uint Length { get; set; }

	public void Read(SystemFontOpenTypeFontReader reader)
	{
		Tag = reader.ReadULong();
		CheckSum = reader.ReadULong();
		Offset = reader.ReadULong();
		Length = reader.ReadULong();
	}

	public override string ToString()
	{
		return SystemFontTags.GetStringFromTag(Tag);
	}
}
