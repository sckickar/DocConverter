namespace DocGen.PdfViewer.Base;

internal class EncodedDataParser : FontFileParser
{
	private readonly ByteEncodingCollectionBase encodings;

	public EncodedDataParser(byte[] data, ByteEncodingCollectionBase encodings)
		: base(data)
	{
		this.encodings = encodings;
	}

	public object ReadOperand()
	{
		byte b = Peek(0);
		return encodings.FindEncoding(b).Read(this);
	}
}
