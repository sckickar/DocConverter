namespace DocGen.PdfViewer.Base;

internal class TwoByteIntegerEncodingType1Base : ByteEncodingBase
{
	public TwoByteIntegerEncodingType1Base()
		: base(247, 250)
	{
	}

	public override object Read(EncodedDataParser reader)
	{
		byte num = reader.Read();
		byte b = reader.Read();
		return (short)((num - 247) * 256 + b + 108);
	}
}
