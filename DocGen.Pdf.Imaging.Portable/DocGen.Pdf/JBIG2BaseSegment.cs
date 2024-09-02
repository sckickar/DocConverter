namespace DocGen.Pdf;

internal abstract class JBIG2BaseSegment : JBIG2Segment
{
	protected internal int regionBitmapWidth;

	protected internal int regionBitmapHeight;

	protected internal int regionBitmapXLocation;

	protected internal int regionBitmapYLocation;

	protected internal RegionFlags regionFlags = new RegionFlags();

	private BitOperation m_bitOperation = new BitOperation();

	public JBIG2BaseSegment(JBIG2StreamDecoder streamDecoder)
		: base(streamDecoder)
	{
	}

	public override void readSegment()
	{
		short[] array = new short[4];
		m_decoder.ReadByte(array);
		regionBitmapWidth = m_bitOperation.GetInt32(array);
		array = new short[4];
		m_decoder.ReadByte(array);
		regionBitmapHeight = m_bitOperation.GetInt32(array);
		array = new short[4];
		m_decoder.ReadByte(array);
		regionBitmapXLocation = m_bitOperation.GetInt32(array);
		array = new short[4];
		m_decoder.ReadByte(array);
		regionBitmapYLocation = m_bitOperation.GetInt32(array);
		short flags = m_decoder.ReadByte();
		regionFlags.setFlags(flags);
	}
}
