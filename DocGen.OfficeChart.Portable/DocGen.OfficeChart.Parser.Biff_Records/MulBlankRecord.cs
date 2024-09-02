using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Implementation.Exceptions;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.MulBlank)]
[CLSCompliant(false)]
internal class MulBlankRecord : CellPositionBase, IMultiCellRecord, ICellPositionFormat
{
	public const int DEF_FIXED_SIZE = 6;

	private const int DEF_MINIMUM_SIZE = 6;

	public const int DEF_SUB_ITEM_SIZE = 2;

	private List<ushort> m_arrExtFormatIndexes;

	private int m_iLastCol;

	public int FirstColumn
	{
		get
		{
			return m_iColumn;
		}
		set
		{
			m_iColumn = value;
		}
	}

	public List<ushort> ExtendedFormatIndexes
	{
		get
		{
			return m_arrExtFormatIndexes;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_arrExtFormatIndexes = value;
		}
	}

	public int LastColumn
	{
		get
		{
			return m_iLastCol;
		}
		set
		{
			m_iLastCol = value;
		}
	}

	public override int MinimumRecordSize => 6;

	public int SubRecordSize => 2;

	public TBIFFRecord SubRecordType => TBIFFRecord.Blank;

	protected override void ParseCellData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		iOffset -= 2;
		if (m_iLength % 2 != 0)
		{
			throw new WrongBiffRecordDataException("( Length - 6 ) % 2 != 0");
		}
		int num = m_iLength - 6;
		if (version != 0)
		{
			num -= 6;
		}
		num /= 2;
		m_arrExtFormatIndexes = new List<ushort>(num);
		for (int i = 0; i < num; i++)
		{
			m_arrExtFormatIndexes.Add(provider.ReadUInt16(iOffset));
			iOffset += 2;
		}
		if (version == OfficeVersion.Excel97to2003)
		{
			m_iLastCol = provider.ReadUInt16(iOffset);
		}
		else
		{
			m_iLastCol = provider.ReadInt32(iOffset);
		}
		InternalDataIntegrityCheck();
	}

	protected override void InfillCellData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		iOffset -= 2;
		int count = m_arrExtFormatIndexes.Count;
		for (int i = 0; i < count; i++)
		{
			provider.WriteUInt16(iOffset, m_arrExtFormatIndexes[i]);
			iOffset += 2;
		}
		provider.WriteUInt16(iOffset, (ushort)m_iLastCol);
	}

	private void InternalDataIntegrityCheck()
	{
		if (m_iLastCol - m_iColumn + 1 != m_arrExtFormatIndexes.Count)
		{
			throw new WrongBiffRecordDataException("m_usLastCol - m_usFirstCol + 1 != m_arrExtFormatIndexes.Length");
		}
	}

	public BlankRecord GetBlankRecord(int iColumnIndex)
	{
		if (iColumnIndex < m_iColumn || iColumnIndex > m_iLastCol)
		{
			throw new ArgumentOutOfRangeException("iColumnIndex", "Value cannot be less m_usFirstCol and greater than m_usLastCol");
		}
		int index = iColumnIndex - m_iColumn;
		ushort extendedFormatIndex = m_arrExtFormatIndexes[index];
		BlankRecord obj = (BlankRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Blank);
		obj.Row = m_iRow;
		obj.Column = iColumnIndex;
		obj.ExtendedFormatIndex = extendedFormatIndex;
		return obj;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		int num = m_arrExtFormatIndexes.Count * 2 + 6;
		if (version != 0)
		{
			num += 6;
		}
		return num;
	}

	public static void IncreaseLastColumn(DataProvider provider, int recordStart, int iLength, OfficeVersion version, int columnDelta)
	{
		int num = recordStart + iLength + 4;
		switch (version)
		{
		case OfficeVersion.Excel97to2003:
		{
			num -= 2;
			int num2 = provider.ReadInt16(num) + columnDelta;
			provider.WriteInt16(num, (short)num2);
			break;
		}
		case OfficeVersion.Excel2007:
		case OfficeVersion.Excel2010:
		case OfficeVersion.Excel2013:
		{
			num -= 4;
			int num2 = provider.ReadInt32(num) + columnDelta;
			provider.WriteInt32(num, (short)num2);
			break;
		}
		default:
			throw new ArgumentOutOfRangeException("version");
		}
	}

	public int GetSeparateSubRecordSize(OfficeVersion version)
	{
		int num = 10;
		if (version != 0)
		{
			num += 4;
		}
		return num;
	}

	public void Insert(ICellPositionFormat cell)
	{
		int column = cell.Column;
		int row = cell.Row;
		ushort extendedFormatIndex = cell.ExtendedFormatIndex;
		if (base.Row != row || m_iColumn > column || m_iLastCol < column)
		{
			throw new ArgumentOutOfRangeException("cell.Column");
		}
		m_arrExtFormatIndexes[column - m_iColumn] = extendedFormatIndex;
	}

	public ICellPositionFormat[] Split(int iColumnIndex)
	{
		if (iColumnIndex >= m_iColumn && iColumnIndex <= m_iLastCol)
		{
			_ = m_iColumn;
			_ = m_iLastCol;
			ICellPositionFormat cellPositionFormat = null;
			ICellPositionFormat cellPositionFormat2 = null;
			cellPositionFormat = CreateRecord(m_iColumn, iColumnIndex - 1);
			cellPositionFormat2 = CreateRecord(iColumnIndex + 1, m_iLastCol);
			return new ICellPositionFormat[2] { cellPositionFormat, cellPositionFormat2 };
		}
		return new ICellPositionFormat[1] { this };
	}

	private ICellPositionFormat CreateRecord(int iFirstCol, int iLastCol)
	{
		if (iFirstCol > iLastCol)
		{
			return null;
		}
		if (iFirstCol == iLastCol)
		{
			return CreateBlankRecord(iFirstCol);
		}
		MulBlankRecord mulBlankRecord = (MulBlankRecord)BiffRecordFactory.GetRecord(TBIFFRecord.MulBlank);
		mulBlankRecord.m_iColumn = iFirstCol;
		mulBlankRecord.m_iLastCol = iLastCol;
		mulBlankRecord.m_iRow = m_iRow;
		int num = iLastCol - iFirstCol + 1;
		List<ushort> list = (mulBlankRecord.m_arrExtFormatIndexes = new List<ushort>(num));
		int num2 = 0;
		int num3 = iFirstCol - m_iColumn;
		while (num2 < num)
		{
			list[num2] = m_arrExtFormatIndexes[num3];
			num2++;
			num3++;
		}
		return mulBlankRecord;
	}

	private ICellPositionFormat CreateBlankRecord(int iColumnIndex)
	{
		BlankRecord obj = (BlankRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Blank);
		obj.ExtendedFormatIndex = m_arrExtFormatIndexes[iColumnIndex - m_iColumn];
		obj.Row = base.Row;
		obj.Column = iColumnIndex;
		return obj;
	}

	public BiffRecordRaw[] Split(bool bIgnoreStyles)
	{
		BiffRecordRaw[] array = new BiffRecordRaw[m_iLastCol - m_iColumn + 1];
		int num = m_iColumn;
		int num2 = 0;
		while (num <= m_iLastCol)
		{
			ICellPositionFormat cellPositionFormat = CreateBlankRecord(num);
			array[num2] = (BiffRecordRaw)cellPositionFormat;
			num++;
			num2++;
		}
		return array;
	}
}
