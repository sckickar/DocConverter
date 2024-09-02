namespace DocGen.Pdf.Parsing;

internal class SystemFontFiveByteFixedEncoding : SystemFontByteEncoding
{
	public SystemFontFiveByteFixedEncoding()
		: base(byte.MaxValue, byte.MaxValue)
	{
	}

	public override object Read(SystemFontEncodedDataReader reader)
	{
		reader.Read();
		byte num = reader.Read();
		byte b = reader.Read();
		byte b2 = reader.Read();
		byte b3 = reader.Read();
		int num2 = (num << 8) | b;
		int num3 = (b2 << 8) | b3;
		return num2 / num3;
	}
}
