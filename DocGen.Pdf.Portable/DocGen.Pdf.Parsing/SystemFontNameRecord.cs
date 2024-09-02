namespace DocGen.Pdf.Parsing;

internal class SystemFontNameRecord
{
	public ushort PlatformID { get; private set; }

	public ushort EncodingID { get; private set; }

	public ushort LanguageID { get; private set; }

	public ushort NameID { get; private set; }

	public ushort Length { get; private set; }

	public ushort Offset { get; private set; }

	public override bool Equals(object obj)
	{
		if (obj is SystemFontNameRecord systemFontNameRecord && EncodingID == systemFontNameRecord.EncodingID && LanguageID == systemFontNameRecord.LanguageID && Length == systemFontNameRecord.Length && NameID == systemFontNameRecord.NameID)
		{
			return PlatformID == systemFontNameRecord.PlatformID;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((((17 * 23 + PlatformID.GetHashCode()) * 23 + EncodingID.GetHashCode()) * 23 + LanguageID.GetHashCode()) * 23 + NameID.GetHashCode()) * 23 + Length.GetHashCode();
	}

	public void Read(SystemFontOpenTypeFontReader reader)
	{
		PlatformID = reader.ReadUShort();
		EncodingID = reader.ReadUShort();
		LanguageID = reader.ReadUShort();
		NameID = reader.ReadUShort();
		Length = reader.ReadUShort();
		Offset = reader.ReadUShort();
	}
}
