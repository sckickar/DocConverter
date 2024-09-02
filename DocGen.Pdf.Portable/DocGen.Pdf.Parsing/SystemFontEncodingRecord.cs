namespace DocGen.Pdf.Parsing;

internal class SystemFontEncodingRecord
{
	public ushort PlatformId { get; set; }

	public ushort EncodingId { get; set; }

	public uint Offset { get; set; }

	public void Read(SystemFontOpenTypeFontReader reader)
	{
		PlatformId = reader.ReadUShort();
		EncodingId = reader.ReadUShort();
		Offset = reader.ReadULong();
	}
}
