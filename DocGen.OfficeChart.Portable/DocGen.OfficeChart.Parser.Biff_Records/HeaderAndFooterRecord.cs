using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.HeaderFooter)]
internal class HeaderAndFooterRecord : BiffRecordRaw
{
	[BiffRecordPos(28, 0, TFieldType.Bit)]
	private bool m_bfHFDiffOddEven;

	[BiffRecordPos(28, 1, TFieldType.Bit)]
	private bool m_bfHFDiffFirst;

	[BiffRecordPos(28, 2, TFieldType.Bit)]
	private bool m_bfHFScaleWithDoc;

	[BiffRecordPos(28, 3, TFieldType.Bit)]
	private bool m_bfHFAlignMargins;

	private byte[] m_arrBytes = new byte[38];

	private int recordCode = 2204;

	private const int Record2003Length = 22;

	private const int Record2010Length = 38;

	public bool AlignHFWithPageMargins
	{
		get
		{
			return m_bfHFAlignMargins;
		}
		set
		{
			m_bfHFAlignMargins = value;
		}
	}

	public bool DifferentOddAndEvenPagesHF
	{
		get
		{
			return m_bfHFDiffOddEven;
		}
		set
		{
			m_bfHFDiffOddEven = value;
		}
	}

	public bool HFScaleWithDoc
	{
		get
		{
			return m_bfHFScaleWithDoc;
		}
		set
		{
			m_bfHFScaleWithDoc = value;
		}
	}

	public bool DifferentFirstPageHF
	{
		get
		{
			return m_bfHFDiffFirst;
		}
		set
		{
			m_bfHFDiffFirst = value;
		}
	}

	public override int MinimumRecordSize => 0;

	public HeaderAndFooterRecord()
	{
		m_bfHFScaleWithDoc = true;
	}

	public HeaderAndFooterRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public HeaderAndFooterRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		if (m_iLength > 0)
		{
			iOffset = ((m_iLength <= 22) ? (iOffset + 12) : (iOffset + 28));
			m_bfHFDiffOddEven = provider.ReadBit(iOffset, 0);
			m_bfHFDiffFirst = provider.ReadBit(iOffset, 1);
			m_bfHFScaleWithDoc = provider.ReadBit(iOffset, 2);
			m_bfHFAlignMargins = provider.ReadBit(iOffset, 3);
		}
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteBytes(4, m_arrBytes);
		provider.WriteInt32(4, recordCode);
		iOffset = 28;
		provider.WriteBit(iOffset, m_bfHFDiffOddEven, 0);
		provider.WriteBit(iOffset, m_bfHFDiffFirst, 1);
		provider.WriteBit(iOffset, m_bfHFScaleWithDoc, 2);
		provider.WriteBit(iOffset, m_bfHFAlignMargins, 3);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 42;
	}
}
