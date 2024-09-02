using System;
using System.IO;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartAI)]
[CLSCompliant(false)]
internal class ChartAIRecord : BiffRecordRaw
{
	public enum LinkIndex
	{
		LinkToTitleOrText,
		LinkToValues,
		LinkToCategories,
		LinkToBubbles
	}

	public enum ReferenceType
	{
		DefaultCategories,
		EnteredDirectly,
		Worksheet,
		NotUsed,
		ErrorReported
	}

	[BiffRecordPos(0, 1)]
	private byte m_id;

	[BiffRecordPos(1, 1)]
	private byte m_ReferenceType;

	[BiffRecordPos(2, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(4, 2)]
	private ushort m_usNumIndex;

	[BiffRecordPos(6, 2)]
	private ushort m_usFormulaSize;

	[BiffRecordPos(2, 1, TFieldType.Bit)]
	private bool m_bCustomNumberFormat;

	private Ptg[] m_arrExpression;

	public LinkIndex IndexIdentifier
	{
		get
		{
			return (LinkIndex)m_id;
		}
		set
		{
			m_id = (byte)value;
		}
	}

	public ReferenceType Reference
	{
		get
		{
			return (ReferenceType)m_ReferenceType;
		}
		set
		{
			m_ReferenceType = (byte)value;
		}
	}

	public ushort Options => m_usOptions;

	public ushort NumberFormatIndex
	{
		get
		{
			return m_usNumIndex;
		}
		set
		{
			if (value != m_usNumIndex)
			{
				m_usNumIndex = value;
			}
		}
	}

	public ushort FormulaSize
	{
		get
		{
			return m_usFormulaSize;
		}
		set
		{
			if (value != m_usFormulaSize)
			{
				m_usFormulaSize = value;
			}
		}
	}

	public bool IsCustomNumberFormat
	{
		get
		{
			return m_bCustomNumberFormat;
		}
		set
		{
			m_bCustomNumberFormat = value;
		}
	}

	public Ptg[] ParsedExpression
	{
		get
		{
			return m_arrExpression;
		}
		set
		{
			m_arrExpression = value;
		}
	}

	public ChartAIRecord()
	{
	}

	public ChartAIRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartAIRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		int num = 8;
		if (m_arrExpression != null)
		{
			int i = 0;
			for (int num2 = m_arrExpression.Length; i < num2; i++)
			{
				Ptg ptg = m_arrExpression[i];
				num += ptg.GetSize(version);
				if (ptg is IAdditionalData additionalData)
				{
					num += additionalData.AdditionalDataSize;
				}
			}
		}
		return num;
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_id = provider.ReadByte(iOffset);
		iOffset++;
		m_ReferenceType = provider.ReadByte(iOffset);
		iOffset++;
		m_usOptions = provider.ReadUInt16(iOffset);
		m_bCustomNumberFormat = provider.ReadBit(iOffset, 1);
		iOffset += 2;
		m_usNumIndex = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usFormulaSize = provider.ReadUInt16(iOffset);
		iOffset += 2;
		if (m_usFormulaSize > 0)
		{
			m_arrExpression = FormulaUtil.ParseExpression(provider, iOffset, m_usFormulaSize, out var _, version);
		}
		else
		{
			m_arrExpression = null;
		}
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_usOptions &= 1;
		m_usFormulaSize = 0;
		byte[] array = null;
		if (m_arrExpression != null && m_arrExpression.Length != 0)
		{
			array = FormulaUtil.PtgArrayToByteArray(m_arrExpression, version);
			m_usFormulaSize = (ushort)array.Length;
		}
		provider.WriteByte(iOffset, m_id);
		iOffset++;
		provider.WriteByte(iOffset, m_ReferenceType);
		iOffset++;
		provider.WriteUInt16(iOffset, m_usOptions);
		provider.WriteBit(iOffset, m_bCustomNumberFormat, 1);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usNumIndex);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usFormulaSize);
		iOffset += 2;
		m_iLength = 8;
		if (array != null)
		{
			provider.WriteBytes(iOffset, array, 0, m_usFormulaSize);
			m_iLength += m_usFormulaSize;
		}
	}

	public override object Clone()
	{
		ChartAIRecord chartAIRecord = (ChartAIRecord)base.Clone();
		if (m_arrExpression == null)
		{
			return chartAIRecord;
		}
		int num = m_arrExpression.Length;
		chartAIRecord.m_arrExpression = new Ptg[num];
		for (int i = 0; i < num; i++)
		{
			chartAIRecord.m_arrExpression[i] = (Ptg)CloneUtils.CloneCloneable(m_arrExpression[i]);
		}
		return chartAIRecord;
	}
}
