namespace DocGen.Pdf;

internal class RefinementRegionSegment : JBIG2BaseSegment
{
	private bool m_inlineImage;

	private int m_noOfReferedToSegments;

	private int[] m_referedToSegments;

	public RefinementRegionSegment(JBIG2StreamDecoder streamDecoder, bool inlineImage, int[] referedToSegments, int noOfReferedToSegments)
		: base(streamDecoder)
	{
		m_inlineImage = inlineImage;
		m_referedToSegments = referedToSegments;
		m_noOfReferedToSegments = noOfReferedToSegments;
	}
}
