using System;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[CLSCompliant(false)]
[Biff(TBIFFRecord.PageLayoutView)]
internal class PageLayoutView : BiffRecordRaw
{
	private const int DEF_FIXED_SIZE = 16;

	private ushort m_futureRecord = 2187;

	private ushort m_iScale;

	[BiffRecordPos(14, 0, TFieldType.Bit)]
	private bool m_bPageLayoutView;

	[BiffRecordPos(14, 1, TFieldType.Bit)]
	private bool m_bRulerVisible;

	[BiffRecordPos(14, 2, TFieldType.Bit)]
	private bool m_bWhiteSpaceHidden;

	internal ushort Scaling => m_iScale;

	internal bool LayoutView
	{
		get
		{
			return m_bPageLayoutView;
		}
		set
		{
			m_bPageLayoutView = value;
		}
	}

	internal bool WhiteSpaceHidden
	{
		get
		{
			return m_bWhiteSpaceHidden;
		}
		set
		{
			m_bWhiteSpaceHidden = value;
		}
	}

	internal bool RulerVisible
	{
		get
		{
			return m_bRulerVisible;
		}
		set
		{
			m_bRulerVisible = value;
		}
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		iOffset += 12;
		m_iScale = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_bPageLayoutView = provider.ReadBit(iOffset, 0);
		m_bRulerVisible = provider.ReadBit(iOffset, 1);
		m_bWhiteSpaceHidden = provider.ReadBit(iOffset, 2);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_futureRecord);
		provider.WriteUInt16(iOffset + 2, 0);
		provider.WriteInt64(iOffset + 4, 0L);
		iOffset += 12;
		provider.WriteUInt16(iOffset, m_iScale);
		iOffset += 2;
		provider.WriteBit(iOffset, m_bPageLayoutView, 0);
		provider.WriteBit(iOffset, m_bRulerVisible, 1);
		provider.WriteBit(iOffset, m_bWhiteSpaceHidden, 2);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 16;
	}
}
