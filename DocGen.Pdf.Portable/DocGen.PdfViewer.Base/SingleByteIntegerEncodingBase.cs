namespace DocGen.PdfViewer.Base;

internal class SingleByteIntegerEncodingBase : ByteEncodingBase
{
	public SingleByteIntegerEncodingBase()
		: base(32, 246)
	{
	}

	public override object Read(EncodedDataParser reader)
	{
		return (sbyte)(reader.Read() - 139);
	}
}
