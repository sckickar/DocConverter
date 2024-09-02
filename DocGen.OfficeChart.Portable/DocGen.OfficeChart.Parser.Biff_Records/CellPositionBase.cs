using System;
using System.Diagnostics;

namespace DocGen.OfficeChart.Parser.Biff_Records;

internal abstract class CellPositionBase : BiffRecordRaw, ICellPositionFormat
{
	protected int m_iRow;

	protected int m_iColumn;

	[CLSCompliant(false)]
	protected ushort m_usExtendedFormat;

	public int Row
	{
		[DebuggerStepThrough]
		get
		{
			return m_iRow;
		}
		[DebuggerStepThrough]
		set
		{
			m_iRow = value;
		}
	}

	public int Column
	{
		[DebuggerStepThrough]
		get
		{
			return m_iColumn;
		}
		[DebuggerStepThrough]
		set
		{
			m_iColumn = value;
		}
	}

	[CLSCompliant(false)]
	public ushort ExtendedFormatIndex
	{
		get
		{
			return m_usExtendedFormat;
		}
		set
		{
			m_usExtendedFormat = value;
		}
	}

	public CellPositionBase()
	{
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		switch (version)
		{
		case OfficeVersion.Excel97to2003:
			provider.WriteUInt16(iOffset, (ushort)m_iRow);
			iOffset += 2;
			provider.WriteInt16(iOffset, (short)m_iColumn);
			iOffset += 2;
			break;
		case OfficeVersion.Excel2007:
		case OfficeVersion.Excel2010:
		case OfficeVersion.Excel2013:
			provider.WriteInt32(iOffset, m_iRow);
			iOffset += 4;
			provider.WriteInt32(iOffset, m_iColumn);
			iOffset += 4;
			break;
		}
		provider.WriteUInt16(iOffset, m_usExtendedFormat);
		iOffset += 2;
		InfillCellData(provider, iOffset, version);
		m_iLength = GetStoreSize(version);
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		switch (version)
		{
		case OfficeVersion.Excel97to2003:
			m_iRow = provider.ReadUInt16(iOffset);
			iOffset += 2;
			m_iColumn = provider.ReadInt16(iOffset);
			iOffset += 2;
			break;
		case OfficeVersion.Excel2007:
		case OfficeVersion.Excel2010:
		case OfficeVersion.Excel2013:
			m_iRow = provider.ReadInt32(iOffset);
			iOffset += 4;
			m_iColumn = provider.ReadInt32(iOffset);
			iOffset += 4;
			break;
		}
		m_usExtendedFormat = provider.ReadUInt16(iOffset);
		iOffset += 2;
		ParseCellData(provider, iOffset, version);
	}

	protected abstract void ParseCellData(DataProvider provider, int iOffset, OfficeVersion version);

	protected abstract void InfillCellData(DataProvider provider, int iOffset, OfficeVersion version);

	public override int GetStoreSize(OfficeVersion version)
	{
		int num = base.GetStoreSize(version);
		if (version != 0)
		{
			num += 4;
		}
		return num;
	}
}
