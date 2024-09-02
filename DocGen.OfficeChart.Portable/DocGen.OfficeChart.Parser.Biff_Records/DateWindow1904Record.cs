using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.DateWindow1904)]
[CLSCompliant(false)]
internal class DateWindow1904Record : BiffRecordRaw
{
	[BiffRecordPos(0, 2)]
	private ushort m_usWindow;

	[BiffRecordPos(0, 0, TFieldType.Bit)]
	private bool m_bIs1904Windowing;

	public ushort Windowing
	{
		get
		{
			return m_usWindow;
		}
		set
		{
			m_usWindow = value;
		}
	}

	public bool Is1904Windowing
	{
		get
		{
			return m_bIs1904Windowing;
		}
		set
		{
			m_bIs1904Windowing = value;
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public DateWindow1904Record()
	{
	}

	public DateWindow1904Record(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public DateWindow1904Record(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usWindow = provider.ReadUInt16(iOffset);
		m_bIs1904Windowing = provider.ReadBit(iOffset, 0);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usWindow);
		provider.WriteBit(iOffset, m_bIs1904Windowing, 0);
		m_iLength = 2;
	}
}
