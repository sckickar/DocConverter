namespace DocGen.PdfViewer.Base;

internal class StdHeader
{
	private readonly long offset;

	private readonly int headerLength;

	public long Offset => offset;

	public int HeaderLength => headerLength;

	public uint NextHeaderOffset { get; private set; }

	public StdHeader(long offset, int headerLength)
	{
		this.offset = offset;
		this.headerLength = headerLength;
	}

	public void Read(StdFontReader reader)
	{
		reader.Read();
		reader.Read();
		NextHeaderOffset = reader.ReadUInt();
	}

	public bool IsPositionInside(long position)
	{
		if (Offset <= position)
		{
			return position < Offset + HeaderLength;
		}
		return false;
	}
}
