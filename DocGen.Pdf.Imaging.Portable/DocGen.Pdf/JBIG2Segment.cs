namespace DocGen.Pdf;

internal abstract class JBIG2Segment
{
	public const int SYMBOL_DICTIONARY = 0;

	public const int INTERMEDIATE_TEXT_REGION = 4;

	public const int IMMEDIATE_TEXT_REGION = 6;

	public const int IMMEDIATE_LOSSLESS_TEXT_REGION = 7;

	public const int PATTERN_DICTIONARY = 16;

	public const int INTERMEDIATE_HALFTONE_REGION = 20;

	public const int IMMEDIATE_HALFTONE_REGION = 22;

	public const int IMMEDIATE_LOSSLESS_HALFTONE_REGION = 23;

	public const int INTERMEDIATE_GENERIC_REGION = 36;

	public const int IMMEDIATE_GENERIC_REGION = 38;

	public const int IMMEDIATE_LOSSLESS_GENERIC_REGION = 39;

	public const int INTERMEDIATE_GENERIC_REFINEMENT_REGION = 40;

	public const int IMMEDIATE_GENERIC_REFINEMENT_REGION = 42;

	public const int IMMEDIATE_LOSSLESS_GENERIC_REFINEMENT_REGION = 43;

	public const int PAGE_INFORMATION = 48;

	public const int END_OF_PAGE = 49;

	public const int END_OF_STRIPE = 50;

	public const int END_OF_FILE = 51;

	public const int PROFILES = 52;

	public const int TABLES = 53;

	public const int EXTENSION = 62;

	public const int BITMAP = 70;

	protected internal SegmentHeader m_segmentHeader;

	protected internal HuffmanDecoder m_huffmanDecoder;

	protected internal ArithmeticDecoder m_arithmeticDecoder;

	protected internal MMRDecoder m_mmrDecoder;

	protected internal JBIG2StreamDecoder m_decoder;

	internal SegmentHeader SegHeader
	{
		get
		{
			return m_segmentHeader;
		}
		set
		{
			m_segmentHeader = value;
		}
	}

	public JBIG2Segment(JBIG2StreamDecoder streamDecoder)
	{
		m_decoder = streamDecoder;
		m_huffmanDecoder = m_decoder.HuffDecoder;
		m_arithmeticDecoder = m_decoder.ArithDecoder;
		m_mmrDecoder = m_decoder.MmrDecoder;
	}

	protected short ReadAtValue()
	{
		short num;
		if (((uint)(num = m_decoder.ReadByte()) & 0x80u) != 0)
		{
			num |= -256;
		}
		return num;
	}

	public abstract void readSegment();
}
