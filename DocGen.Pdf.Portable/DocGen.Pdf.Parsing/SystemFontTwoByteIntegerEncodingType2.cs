namespace DocGen.Pdf.Parsing;

internal class SystemFontTwoByteIntegerEncodingType2 : SystemFontByteEncoding
{
	public SystemFontTwoByteIntegerEncodingType2()
		: base(251, 254)
	{
	}

	public override object Read(SystemFontEncodedDataReader reader)
	{
		byte num = reader.Read();
		byte b = reader.Read();
		return (short)(-(num - 251) * 256 - b - 108);
	}
}
