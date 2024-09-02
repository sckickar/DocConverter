namespace DocGen.Pdf.Parsing;

internal class SystemFontEncodedDataReader : SystemFontReaderBase
{
	private readonly SystemFontByteEncodingCollection encodings;

	public SystemFontEncodedDataReader(byte[] data, SystemFontByteEncodingCollection encodings)
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
