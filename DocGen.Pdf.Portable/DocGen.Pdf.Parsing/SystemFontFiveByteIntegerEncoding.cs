namespace DocGen.Pdf.Parsing;

internal class SystemFontFiveByteIntegerEncoding : SystemFontByteEncoding
{
	public SystemFontFiveByteIntegerEncoding()
		: base(29, 29)
	{
	}

	public override object Read(SystemFontEncodedDataReader reader)
	{
		reader.Read();
		byte num = reader.Read();
		byte b = reader.Read();
		byte b2 = reader.Read();
		byte b3 = reader.Read();
		return (num << 24) | (b << 16) | (b2 << 8) | b3;
	}
}
