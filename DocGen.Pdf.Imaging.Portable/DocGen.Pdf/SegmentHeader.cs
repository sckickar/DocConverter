namespace DocGen.Pdf;

internal class SegmentHeader
{
	private int m_segmentNumber;

	private int m_segmentType;

	private bool m_pageAssociationSizeSet;

	private bool m_deferredNonRetainSet;

	private int m_referredToSegmentCount;

	private short[] m_rententionFlags;

	private int[] m_referredToSegments;

	private int m_pageAssociation;

	private int m_dataLength;

	internal int SegmentNumber
	{
		get
		{
			return m_segmentNumber;
		}
		set
		{
			m_segmentNumber = value;
		}
	}

	internal int SegmentType
	{
		get
		{
			return m_segmentType;
		}
		set
		{
			m_segmentType = value;
		}
	}

	internal bool IsPageAssociationSizeSet => m_pageAssociationSizeSet;

	internal int ReferedToSegCount
	{
		get
		{
			return m_referredToSegmentCount;
		}
		set
		{
			m_referredToSegmentCount = value;
		}
	}

	internal int DataLength
	{
		get
		{
			return m_dataLength;
		}
		set
		{
			m_dataLength = value;
		}
	}

	internal int PageAssociation
	{
		get
		{
			return m_pageAssociation;
		}
		set
		{
			m_pageAssociation = value;
		}
	}

	internal short[] RententionFlags
	{
		get
		{
			return m_rententionFlags;
		}
		set
		{
			m_rententionFlags = value;
		}
	}

	internal bool IsDeferredNonRetainSet => m_deferredNonRetainSet;

	internal int[] ReferredToSegments
	{
		get
		{
			return m_referredToSegments;
		}
		set
		{
			m_referredToSegments = value;
		}
	}

	public void SetSegmentHeaderFlags(short SegmentHeaderFlags)
	{
		m_segmentType = SegmentHeaderFlags & 0x3F;
		m_pageAssociationSizeSet = (SegmentHeaderFlags & 0x40) == 64;
		m_deferredNonRetainSet = (SegmentHeaderFlags & 0x50) == 80;
	}
}
