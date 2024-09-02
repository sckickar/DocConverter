using System;
using System.IO;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.SharedFormula2)]
[CLSCompliant(false)]
internal class SharedFormulaRecord : BiffRecordRaw, ISharedFormula
{
	private const int DEF_FIXED_SIZE = 10;

	[BiffRecordPos(0, 2)]
	private int m_iFirstRow;

	[BiffRecordPos(2, 2)]
	private int m_iLastRow;

	[BiffRecordPos(4, 1)]
	private int m_iFirstColumn;

	[BiffRecordPos(5, 1)]
	private int m_iLastColumn;

	[BiffRecordPos(6, 2)]
	private ushort m_usReserved;

	[BiffRecordPos(8, 2)]
	private ushort m_usExpressionLen;

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

	public ushort ExpressionLen => m_usExpressionLen;

	public byte[] Expression
	{
		get
		{
			return m_arrExpression;
		}
		set
		{
			m_arrExpression = value;
			m_usExpressionLen = (ushort)((value != null) ? ((ushort)value.Length) : 0);
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
			m_arrFormula = value;
		}
	}

	public ushort Reserved => m_usReserved;

	public override int MinimumRecordSize => 8;

	public SharedFormulaRecord()
	{
	}

	public SharedFormulaRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public SharedFormulaRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		iOffset = ArrayRecord.ParseDimensions(this, provider, iOffset, version);
		m_usReserved = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usExpressionLen = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_arrExpression = new byte[m_usExpressionLen];
		provider.ReadArray(iOffset, m_arrExpression);
		m_arrFormula = FormulaUtil.ParseExpression(provider, iOffset, m_usExpressionLen, out var _, version);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		iOffset = ArrayRecord.SerializeDimensions(this, provider, iOffset, version);
		provider.WriteUInt16(iOffset, m_usReserved);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usExpressionLen);
		iOffset += 2;
		provider.WriteBytes(iOffset, m_arrExpression, 0, m_usExpressionLen);
		m_iLength = iOffset + m_usExpressionLen;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		int num = 10 + m_usExpressionLen;
		if (version != 0)
		{
			num += 10;
		}
		return num;
	}
}
