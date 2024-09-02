namespace DocGen.Pdf.Parsing;

internal class SystemFontSingleByteIntegerEncoding : SystemFontByteEncoding
{
	public SystemFontSingleByteIntegerEncoding()
		: base(32, 246)
	{
	}

	public override object Read(SystemFontEncodedDataReader reader)
	{
		return (sbyte)(reader.Read() - 139);
	}
}
