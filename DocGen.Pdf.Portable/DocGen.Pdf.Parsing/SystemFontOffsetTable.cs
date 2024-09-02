namespace DocGen.Pdf.Parsing;

internal class SystemFontOffsetTable
{
	public ushort NumTables { get; private set; }

	public bool HasOpenTypeOutlines { get; private set; }

	public void Read(SystemFontOpenTypeFontReader reader)
	{
		if (reader != null)
		{
			HasOpenTypeOutlines = SystemFontTags.OTTO_TAG == reader.ReadULong();
			NumTables = reader.ReadUShort();
			reader.ReadUShort();
			reader.ReadUShort();
			reader.ReadUShort();
		}
	}
}
