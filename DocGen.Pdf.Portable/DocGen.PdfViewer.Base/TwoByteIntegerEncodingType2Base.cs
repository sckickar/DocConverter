namespace DocGen.PdfViewer.Base;

internal class TwoByteIntegerEncodingType2Base : ByteEncodingBase
{
	public TwoByteIntegerEncodingType2Base()
		: base(251, 254)
	{
	}

	public override object Read(EncodedDataParser reader)
	{
		byte num = reader.Read();
		byte b = reader.Read();
		return (short)(-(num - 251) * 256 - b - 108);
	}
}
