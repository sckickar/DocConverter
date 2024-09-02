using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.Pane)]
[CLSCompliant(false)]
internal class PaneRecord : BiffRecordRaw
{
	private const int DefaultRecordSize = 10;

	[BiffRecordPos(0, 2)]
	private int m_iVertSplit;

	[BiffRecordPos(2, 2)]
	private int m_iHorizSplit;

	[BiffRecordPos(4, 2)]
	private int m_iFirstRow;

	[BiffRecordPos(6, 2)]
	private int m_iFirstColumn;

	[BiffRecordPos(8, 2)]
	private ushort m_usActivePane;

	public int VerticalSplit
	{
		get
		{
			return m_iVertSplit;
		}
		set
		{
			m_iVertSplit = value;
		}
	}

	public int HorizontalSplit
	{
		get
		{
			return m_iHorizSplit;
		}
		set
		{
			m_iHorizSplit = value;
		}
	}

	public int FirstRow
	{
		get
		{
			return m_iFirstRow;
		}
		set
		{
			m_iFirstRow = value;
		}
	}

	public int FirstColumn
	{
		get
		{
			return m_iFirstColumn;
		}
		set
		{
			m_iFirstColumn = value;
		}
	}

	public ushort ActivePane
	{
		get
		{
			return m_usActivePane;
		}
		set
		{
			m_usActivePane = value;
		}
	}

	public override int MaximumRecordSize => 10;

	public override int MinimumRecordSize => 10;

	public PaneRecord()
	{
	}

	public PaneRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public PaneRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_iVertSplit = provider.ReadUInt16(iOffset);
		m_iHorizSplit = provider.ReadUInt16(iOffset + 2);
		m_iFirstRow = provider.ReadUInt16(iOffset + 4);
		m_iFirstColumn = provider.ReadUInt16(iOffset + 6);
		m_usActivePane = provider.ReadUInt16(iOffset + 8);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, (ushort)m_iVertSplit);
		provider.WriteUInt16(iOffset + 2, (ushort)m_iHorizSplit);
		provider.WriteUInt16(iOffset + 4, (ushort)m_iFirstRow);
		provider.WriteUInt16(iOffset + 6, (ushort)m_iFirstColumn);
		provider.WriteUInt16(iOffset + 8, m_usActivePane);
		m_iLength = 10;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 10;
	}
}
