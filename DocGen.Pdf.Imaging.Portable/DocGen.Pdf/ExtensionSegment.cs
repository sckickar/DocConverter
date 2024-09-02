namespace DocGen.Pdf;

internal class ExtensionSegment : JBIG2Segment
{
	public ExtensionSegment(JBIG2StreamDecoder streamDecoder)
		: base(streamDecoder)
	{
	}

	public override void readSegment()
	{
		for (int i = 0; i < m_segmentHeader.DataLength; i++)
		{
			m_decoder.ReadByte();
		}
	}
}
