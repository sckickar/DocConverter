namespace DocGen.Pdf;

internal class EndOfStripeSegment : JBIG2Segment
{
	public EndOfStripeSegment(JBIG2StreamDecoder streamDecoder)
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
