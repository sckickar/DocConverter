using System;
using System.IO;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.Array)]
[CLSCompliant(false)]
internal class ArrayRecord : BiffRecordRaw, ISharedFormula, ICloneable, IFormulaRecord
{
	private const int DEF_RECORD_MIN_SIZE = 14;

	private const int DEF_FORMULA_OFFSET = 14;

	[BiffRecordPos(0, 2)]
	private int m_iFirstRow;

	[BiffRecordPos(2, 2)]
	private int m_iLastRow;

	[BiffRecordPos(4, 1)]
	private int m_iFirstColumn;

	[BiffRecordPos(5, 1)]
	private int m_iLastColumn;

	[BiffRecordPos(6, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(6, 0, TFieldType.Bit)]
	private bool m_bRecalculateAlways;

	[BiffRecordPos(6, 1, TFieldType.Bit)]
	private bool m_bRecalculateOnOpen;

	[BiffRecordPos(8, 4, true)]
	private int m_iReserved;

	[BiffRecordPos(12, 2)]
	private ushort m_usExpressionLength;

	private byte[] m_arrExpression;

	private Ptg[] m_arrFormula;

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

	public int LastRow
	{
		get
		{
			return m_iLastRow;
		}
		set
		{
			m_iLastRow = value;
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

	public int LastColumn
	{
		get
		{
			return m_iLastColumn;
		}
		set
		{
			m_iLastColumn = value;
		}
	}

	public ushort ExpressionLen => m_usExpressionLength;

	public byte[] Expression
	{
		get
		{
			return m_arrExpression;
		}
		set
		{
			m_arrExpression = value;
			m_usExpressionLength = (ushort)((value != null) ? ((ushort)value.Length) : 0);
		}
	}

	public Ptg[] Formula
	{
		get
		{
			return m_arrFormula;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Formula");
			}
			m_arrExpression = FormulaUtil.PtgArrayToByteArray(value, out var formulaLen, OfficeVersion.Excel2007);
			m_usExpressionLength = (ushort)formulaLen;
			m_arrFormula = value;
		}
	}

	public int Reserved => m_iReserved;

	public override int MinimumRecordSize => 14;

	public bool IsRecalculateAlways
	{
		get
		{
			return m_bRecalculateAlways;
		}
		set
		{
			m_bRecalculateAlways = value;
		}
	}

	public bool IsRecalculateOnOpen
	{
		get
		{
			return m_bRecalculateOnOpen;
		}
		set
		{
			m_bRecalculateOnOpen = value;
		}
	}

	public ushort Options => m_usOptions;

	public ArrayRecord()
	{
	}

	public ArrayRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ArrayRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		iOffset = ParseDimensions(this, provider, iOffset, version);
		m_usOptions = provider.ReadUInt16(iOffset);
		m_bRecalculateAlways = provider.ReadBit(iOffset, 0);
		m_bRecalculateOnOpen = provider.ReadBit(iOffset, 1);
		iOffset += 2;
		m_iReserved = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_usExpressionLength = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_arrFormula = FormulaUtil.ParseExpression(provider, iOffset, m_usExpressionLength, out var finalOffset, version);
		m_arrExpression = new byte[finalOffset - iOffset];
		provider.ReadArray(iOffset, m_arrExpression);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iReserved = 0;
		int num = iOffset;
		m_arrExpression = FormulaUtil.PtgArrayToByteArray(m_arrFormula, out var formulaLen, version);
		m_usExpressionLength = (ushort)formulaLen;
		iOffset = SerializeDimensions(this, provider, iOffset, version);
		provider.WriteUInt16(iOffset, m_usOptions);
		provider.WriteBit(iOffset, m_bRecalculateAlways, 0);
		provider.WriteBit(iOffset, m_bRecalculateOnOpen, 1);
		iOffset += 2;
		provider.WriteInt32(iOffset, m_iReserved);
		iOffset += 4;
		provider.WriteUInt16(iOffset, m_usExpressionLength);
		iOffset += 2;
		m_iLength = iOffset - num;
		int num2 = m_arrExpression.Length;
		provider.WriteBytes(iOffset, m_arrExpression, 0, num2);
		m_iLength += num2;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		int num = 14 + DVRecord.GetFormulaSize(m_arrFormula, version, addAdditionalDataSize: true);
		if (version != 0)
		{
			num += 10;
		}
		return num;
	}

	public static int SerializeDimensions(ISharedFormula shared, DataProvider provider, int iOffset, OfficeVersion version)
	{
		if (version == OfficeVersion.Excel97to2003)
		{
			provider.WriteUInt16(iOffset, (ushort)shared.FirstRow);
			iOffset += 2;
			provider.WriteUInt16(iOffset, (ushort)shared.LastRow);
			iOffset += 2;
			provider.WriteByte(iOffset, (byte)shared.FirstColumn);
			iOffset++;
			provider.WriteByte(iOffset, (byte)shared.LastColumn);
			iOffset++;
		}
		else
		{
			if (version == OfficeVersion.Excel97to2003)
			{
				throw new ArgumentOutOfRangeException("version");
			}
			provider.WriteInt32(iOffset, shared.FirstRow);
			iOffset += 4;
			provider.WriteInt32(iOffset, shared.LastRow);
			iOffset += 4;
			provider.WriteInt32(iOffset, shared.FirstColumn);
			iOffset += 4;
			provider.WriteInt32(iOffset, shared.LastColumn);
			iOffset += 4;
		}
		return iOffset;
	}

	public static int ParseDimensions(ISharedFormula shared, DataProvider provider, int iOffset, OfficeVersion version)
	{
		if (version == OfficeVersion.Excel97to2003)
		{
			shared.FirstRow = provider.ReadUInt16(iOffset);
			iOffset += 2;
			shared.LastRow = provider.ReadUInt16(iOffset);
			iOffset += 2;
			shared.FirstColumn = provider.ReadByte(iOffset);
			iOffset++;
			shared.LastColumn = provider.ReadByte(iOffset);
			iOffset++;
		}
		else
		{
			if (version == OfficeVersion.Excel97to2003)
			{
				throw new ArgumentOutOfRangeException("version");
			}
			shared.FirstRow = provider.ReadInt32(iOffset);
			iOffset += 4;
			shared.LastRow = provider.ReadInt32(iOffset);
			iOffset += 4;
			shared.FirstColumn = provider.ReadInt32(iOffset);
			iOffset += 4;
			shared.LastColumn = provider.ReadInt32(iOffset);
			iOffset += 4;
		}
		return iOffset;
	}

	public override bool Equals(object obj)
	{
		if (obj is ArrayRecord arrayRecord)
		{
			if (arrayRecord.FirstColumn == FirstColumn && arrayRecord.FirstRow == FirstRow && arrayRecord.LastColumn == LastColumn && arrayRecord.LastRow == LastRow)
			{
				return Ptg.CompareArrays(arrayRecord.m_arrFormula, m_arrFormula);
			}
			return false;
		}
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return FirstColumn.GetHashCode() ^ FirstRow.GetHashCode() ^ LastColumn.GetHashCode() ^ LastRow.GetHashCode();
	}

	public new object Clone()
	{
		ArrayRecord obj = (ArrayRecord)base.Clone();
		obj.m_arrExpression = CloneUtils.CloneByteArray(m_arrExpression);
		obj.m_arrFormula = CloneUtils.ClonePtgArray(m_arrFormula);
		return obj;
	}
}
