namespace DocGen.Office;

internal struct TtfNameRecord
{
	public ushort PlatformID;

	public ushort EncodingID;

	public ushort LanguageID;

	public ushort NameID;

	public ushort Length;

	public ushort Offset;

	public string Name;
}
