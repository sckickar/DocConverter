namespace DocGen.Pdf.Parsing;

internal class SystemFontThreeByteIntegerEncoding : SystemFontByteEncoding
{
	public SystemFontThreeByteIntegerEncoding()
		: base(28, 28)
	{
	}

	public override object Read(SystemFontEncodedDataReader reader)
	{
		reader.Read();
		byte num = reader.Read();
		byte b = reader.Read();
		return (short)((num << 8) | b);
	}
}
