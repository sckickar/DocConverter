using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Implementation.Exceptions;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.MulRK)]
[CLSCompliant(false)]
internal class MulRKRecord : CellPositionBase, IMultiCellRecord, ICellPositionFormat
{
	[CLSCompliant(false)]
	public class RkRec
	{
		private ushort m_usExtFormatIndex;

		private int m_iRk;

		public ushort ExtFormatIndex
		{
			get
			{
				return m_usExtFormatIndex;
			}
			set
			{
				m_usExtFormatIndex = value;
			}
		}

		public int Rk
		{
			get
			{
				return m_iRk;
			}
			set
			{
				m_iRk = value;
			}
		}

		public double RkNumber
		{
			get
			{
				bool num = (m_iRk & 2) == 2;
				bool flag = (m_iRk & 1) == 1;
				long num2 = m_iRk >> 2;
				if (num)
				{
					double num3 = num2;
					if (!flag)
					{
						return num3;
					}
					return num3 / 100.0;
				}
				double num4 = BitConverterGeneral.Int64BitsToDouble(num2 << 34);
				if (!flag)
				{
					return num4;
				}
				return num4 / 100.0;
			}
		}

		private RkRec()
		{
		}

		public RkRec(ushort xf, int rk)
		{
			m_usExtFormatIndex = xf;
			m_iRk = rk;
		}
	}

	public const int DEF_FIXED_SIZE = 6;

	public const int DEF_SUB_ITEM_SIZE = 6;

	private List<RkRec> m_arrRKs;

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

	public List<RkRec> Records
	{
		get
		{
			return m_arrRKs;
		}
		set
		{
			m_arrRKs = value;
		}
	}

	public override int MinimumRecordSize => 6;

	public int SubRecordSize => 6;

	public TBIFFRecord SubRecordType => TBIFFRecord.RK;

	protected override void ParseCellData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		iOffset -= 2;
		int num = base.Length - 6;
		if (version != 0)
		{
			num -= 6;
		}
		num /= 6;
		m_arrRKs = new List<RkRec>(num);
		if (base.Length % 6 != 0)
		{
			throw new WrongBiffRecordDataException();
		}
		int num2 = iOffset;
		int num3 = 0;
		while (num3 < num)
		{
			RkRec item = new RkRec(provider.ReadUInt16(num2), provider.ReadInt32(num2 + 2));
			m_arrRKs.Add(item);
			num3++;
			num2 += 6;
		}
		if (version == OfficeVersion.Excel97to2003)
		{
			m_iLastCol = provider.ReadUInt16(num2);
		}
		else
		{
			m_iLastCol = provider.ReadInt32(num2);
		}
	}

	protected override void InfillCellData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		iOffset -= 2;
		int num = 0;
		int count = m_arrRKs.Count;
		while (num < count)
		{
			RkRec rkRec = m_arrRKs[num];
			provider.WriteUInt16(iOffset, rkRec.ExtFormatIndex);
			provider.WriteInt32(iOffset + 2, rkRec.Rk);
			num++;
			iOffset += 6;
		}
		if (version == OfficeVersion.Excel97to2003)
		{
			provider.WriteUInt16(iOffset, (ushort)m_iLastCol);
		}
		else
		{
			provider.WriteInt32(iOffset, m_iLastCol);
		}
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		int num = m_arrRKs.Count * 6 + 6;
		if (version != 0)
		{
			num += 6;
		}
		return num;
	}

	public int GetSeparateSubRecordSize(OfficeVersion version)
	{
		int num = 14;
		if (version != 0)
		{
			num += 4;
		}
		return num;
	}

	public void Insert(ICellPositionFormat cell)
	{
		if (cell.TypeCode == base.TypeCode)
		{
			MergeRecords((MulRKRecord)cell);
		}
		else
		{
			InsertSubRecord(cell);
		}
	}

	private void MergeRecords(MulRKRecord mulRK)
	{
		if (mulRK == null)
		{
			throw new ArgumentNullException("mulRK");
		}
		if (mulRK.Row != m_iRow)
		{
			throw new ArgumentOutOfRangeException("Row", "Rows should be equal for both MulRK records.");
		}
		if (mulRK.FirstColumn == LastColumn + 1)
		{
			m_iLastCol = mulRK.LastColumn;
			m_arrRKs.AddRange(mulRK.m_arrRKs);
			return;
		}
		if (mulRK.LastColumn + 1 == FirstColumn)
		{
			m_iColumn = mulRK.m_iColumn;
			m_arrRKs.InsertRange(0, mulRK.m_arrRKs);
			return;
		}
		throw new ArgumentException("Two MulRK records doesn't correspond each other.");
	}

	public void InsertSubRecord(ICellPositionFormat cell)
	{
		if (cell.TypeCode != TBIFFRecord.RK)
		{
			throw new ArgumentOutOfRangeException("cell.TypeCode");
		}
		int column = cell.Column;
		int row = cell.Row;
		ushort extendedFormatIndex = cell.ExtendedFormatIndex;
		bool flag = m_arrRKs == null;
		if (flag || m_arrRKs.Count == 0)
		{
			if (flag)
			{
				m_arrRKs = new List<RkRec>();
			}
			RkRec item = CreateSubRecord((RKRecord)cell);
			m_arrRKs.Add(item);
			m_iRow = cell.Row;
			m_iColumn = (m_iLastCol = cell.Column);
			return;
		}
		if (base.Row != row)
		{
			throw new ArgumentOutOfRangeException("Row");
		}
		if (m_iColumn <= column && m_iLastCol >= column)
		{
			RKRecord rKRecord = (RKRecord)cell;
			int index = column - m_iColumn;
			RkRec rkRec = m_arrRKs[index];
			rkRec.ExtFormatIndex = extendedFormatIndex;
			rkRec.Rk = rKRecord.RKNumberInt;
			return;
		}
		if (column == m_iColumn - 1)
		{
			RkRec item2 = CreateSubRecord((RKRecord)cell);
			m_arrRKs.Insert(0, item2);
			m_iColumn--;
			return;
		}
		if (column == m_iLastCol + 1)
		{
			RkRec item3 = CreateSubRecord((RKRecord)cell);
			m_arrRKs.Add(item3);
			m_iLastCol++;
			return;
		}
		throw new ArgumentOutOfRangeException("cell.Column");
	}

	private RkRec CreateSubRecord(RKRecord rk)
	{
		if (rk == null)
		{
			throw new ArgumentNullException("rk");
		}
		return new RkRec(rk.ExtendedFormatIndex, rk.RKNumberInt);
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
			return CreateRkRecord(iFirstCol);
		}
		MulRKRecord mulRKRecord = (MulRKRecord)BiffRecordFactory.GetRecord(TBIFFRecord.MulRK);
		mulRKRecord.m_iColumn = iFirstCol;
		mulRKRecord.m_iLastCol = iLastCol;
		mulRKRecord.m_iRow = m_iRow;
		int num = iLastCol - iFirstCol + 1;
		List<RkRec> list = (mulRKRecord.m_arrRKs = new List<RkRec>(num));
		int num2 = 0;
		int num3 = iFirstCol - m_iColumn;
		while (num2 < num)
		{
			list[num2] = m_arrRKs[num3];
			num2++;
			num3++;
		}
		return mulRKRecord;
	}

	private ICellPositionFormat CreateRkRecord(int iColumnIndex)
	{
		RKRecord obj = (RKRecord)BiffRecordFactory.GetRecord(TBIFFRecord.RK);
		RkRec rkRec = m_arrRKs[iColumnIndex - m_iColumn];
		obj.ExtendedFormatIndex = rkRec.ExtFormatIndex;
		obj.RKNumberInt = rkRec.Rk;
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
			ICellPositionFormat cellPositionFormat = CreateRkRecord(num);
			array[num2] = (BiffRecordRaw)cellPositionFormat;
			num++;
			num2++;
		}
		return array;
	}
}
