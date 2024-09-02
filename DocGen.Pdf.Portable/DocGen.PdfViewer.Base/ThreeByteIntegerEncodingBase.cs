namespace DocGen.PdfViewer.Base;

internal class ThreeByteIntegerEncodingBase : ByteEncodingBase
{
	public ThreeByteIntegerEncodingBase()
		: base(28, 28)
	{
	}

	public override object Read(EncodedDataParser reader)
	{
		reader.Read();
		byte num = reader.Read();
		byte b = reader.Read();
		return (short)((num << 8) | b);
	}
}
