namespace DocGen.Pdf.Parsing;

internal class SystemFontTwoByteIntegerEncodingType1 : SystemFontByteEncoding
{
	public SystemFontTwoByteIntegerEncodingType1()
		: base(247, 250)
	{
	}

	public override object Read(SystemFontEncodedDataReader reader)
	{
		byte num = reader.Read();
		byte b = reader.Read();
		return (short)((num - 247) * 256 + b + 108);
	}
}
