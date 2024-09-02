using System;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.Formula)]
[CLSCompliant(false)]
internal class FormulaRecord : CellPositionBase, ICellPositionFormat, ICloneable, IDoubleValue, IFormulaRecord
{
	public const ulong DEF_FIRST_MASK = 18446462598732841215uL;

	public const ulong DEF_BOOL_MASK = 18446462598732840961uL;

	public const ulong DEF_ERROR_MASK = 18446462598732840962uL;

	public const ulong DEF_BLANK_MASK = 18446462598732840963uL;

	public const ulong DEF_STRING_MASK = 18446462598732840960uL;

	public const ulong DEF_STRING_MASK_VALUE = 18446470295314235392uL;

	private const int DEF_FIXED_SIZE = 22;

	private const ulong DEF_STRING_VALUE_ULONG = 18446471231618678784uL;

	private const ulong DEF_BLANK_VALUE_ULONG = 18446465898945477895uL;

	public static readonly long DEF_STRING_VALUE_LONG;

	public static readonly long DEF_BLANK_VALUE_LONG;

	public static readonly double DEF_STRING_VALUE;

	private const int FormulaValueOffset = 10;

	private const int DataSizeBeforeExpression = 16;

	[BiffRecordPos(6, 8, TFieldType.Float)]
	private double m_dbValue;

	[BiffRecordPos(14, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(14, 0, TFieldType.Bit)]
	private bool m_bRecalculateAlways;

	[BiffRecordPos(14, 1, TFieldType.Bit)]
	private bool m_bCalculateOnOpen;

	[BiffRecordPos(14, 3, TFieldType.Bit)]
	private bool m_bPartOfSharedFormula;

	[BiffRecordPos(16, 4, true)]
	private int m_iReserved;

	[BiffRecordPos(20, 2)]
	private ushort m_usExpressionLen;

	private byte[] m_expression;

	private Ptg[] m_arrParsedExpression;

	private bool m_bFillFromExpression;

	public double Value
	{
		get
		{
			return m_dbValue;
		}
		set
		{
			m_dbValue = value;
		}
	}

	public ushort Options
	{
		get
		{
			return m_usOptions;
		}
		set
		{
			m_usOptions = value;
		}
	}

	public bool RecalculateAlways
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

	public bool CalculateOnOpen
	{
		get
		{
			return m_bCalculateOnOpen;
		}
		set
		{
			m_bCalculateOnOpen = value;
		}
	}

	public bool PartOfSharedFormula
	{
		get
		{
			return m_bPartOfSharedFormula;
		}
		set
		{
			m_bPartOfSharedFormula = value;
		}
	}

	public Ptg[] ParsedExpression
	{
		get
		{
			return m_arrParsedExpression;
		}
		set
		{
			m_arrParsedExpression = value;
			if (value != null)
			{
				m_expression = FormulaUtil.PtgArrayToByteArray(value, out var formulaLen, OfficeVersion.Excel2007);
				m_usExpressionLen = (ushort)formulaLen;
			}
			else
			{
				m_expression = null;
				m_usExpressionLen = 0;
			}
		}
	}

	public int Reserved => m_iReserved;

	public override int MinimumRecordSize => 24;

	public bool IsFillFromExpression
	{
		get
		{
			return m_bFillFromExpression;
		}
		set
		{
			m_bFillFromExpression = value;
		}
	}

	public double DoubleValue => m_dbValue;

	public bool IsBool => (BitConverterGeneral.DoubleToInt64Bits(m_dbValue) & -281474976710401L) == -281474976710655L;

	public bool IsError => (BitConverterGeneral.DoubleToInt64Bits(m_dbValue) & -281474976710401L) == -281474976710654L;

	public bool IsBlank => (BitConverterGeneral.DoubleToInt64Bits(m_dbValue) & -281474976710401L) == -281474976710653L;

	public bool HasString
	{
		get
		{
			return (BitConverterGeneral.DoubleToInt64Bits(m_dbValue) & -281474976710401L) == -281474976710656L;
		}
		set
		{
			m_dbValue = DEF_STRING_VALUE;
		}
	}

	public bool BooleanValue
	{
		get
		{
			if (IsBool)
			{
				return (BitConverterGeneral.DoubleToInt64Bits(m_dbValue) & 0xFF0000) > 0;
			}
			return false;
		}
		set
		{
			SetBoolErrorValue(value ? ((byte)1) : ((byte)0), bIsError: false);
		}
	}

	public byte ErrorValue
	{
		get
		{
			if (IsError)
			{
				return (byte)((BitConverterGeneral.DoubleToInt64Bits(m_dbValue) & 0xFF0000) >> 16);
			}
			return 0;
		}
		set
		{
			SetBoolErrorValue(value, bIsError: true);
		}
	}

	public Ptg[] Formula
	{
		get
		{
			return ParsedExpression;
		}
		set
		{
			ParsedExpression = value;
		}
	}

	static FormulaRecord()
	{
		DEF_STRING_VALUE_LONG = -272842090872832L;
		DEF_STRING_VALUE = BitConverterGeneral.Int64BitsToDouble(DEF_STRING_VALUE_LONG);
		DEF_BLANK_VALUE_LONG = -278174764073721L;
	}

	protected override void ParseCellData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_dbValue = provider.ReadDouble(iOffset);
		iOffset += 8;
		m_usOptions = provider.ReadUInt16(iOffset);
		m_bPartOfSharedFormula = provider.ReadBit(iOffset, 3);
		m_bCalculateOnOpen = provider.ReadBit(iOffset, 1);
		m_bRecalculateAlways = provider.ReadBit(iOffset, 0);
		iOffset += 2;
		m_iReserved = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_usExpressionLen = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_expression = new byte[m_usExpressionLen];
		provider.ReadArray(iOffset, m_expression);
		ParseFormula(provider, iOffset, version);
	}

	protected override void InfillCellData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		PrepareExpression(version);
		provider.WriteDouble(iOffset, m_dbValue);
		iOffset += 8;
		provider.WriteUInt16(iOffset, m_usOptions);
		provider.WriteBit(iOffset, m_bPartOfSharedFormula, 3);
		provider.WriteBit(iOffset, m_bCalculateOnOpen, 1);
		provider.WriteBit(iOffset, m_bRecalculateAlways, 0);
		iOffset += 2;
		provider.WriteInt32(iOffset, m_iReserved);
		iOffset += 4;
		provider.WriteUInt16(iOffset, m_usExpressionLen);
		iOffset += 2;
		if (m_usExpressionLen != 0)
		{
			provider.WriteBytes(iOffset, m_expression, 0, m_expression.Length);
		}
	}

	private void ParseFormula(DataProvider provider, int iOffset, OfficeVersion version)
	{
		try
		{
			ParsedExpression = FormulaUtil.ParseExpression(provider, iOffset, m_usExpressionLen, out var _, version);
		}
		catch (Exception)
		{
			int finalOffset = 0;
		}
	}

	private void PrepareExpression(OfficeVersion version)
	{
		if (m_arrParsedExpression != null && m_arrParsedExpression.Length != 0)
		{
			m_expression = FormulaUtil.PtgArrayToByteArray(ParsedExpression, out var formulaLen, version);
			m_usExpressionLen = (ushort)formulaLen;
		}
		else
		{
			m_expression = null;
			m_usExpressionLen = 0;
		}
		m_bFillFromExpression = false;
	}

	private void SetBoolErrorValue(byte value, bool bIsError)
	{
		m_dbValue = GetBoolErrorValue(value, bIsError);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		int num = 22 + DVRecord.GetFormulaSize(m_arrParsedExpression, version, addAdditionalDataSize: true);
		if (version != 0)
		{
			num += 4;
		}
		return num;
	}

	public static double GetBoolErrorValue(byte value, bool bIsError)
	{
		return BitConverterGeneral.Int64BitsToDouble(((1099494850560L << 8) + value << 16) + ((!bIsError) ? 1 : 2));
	}

	public static void SetStringValue(DataProvider dataProvider, int iFormulaOffset, OfficeVersion version)
	{
		if (version != 0)
		{
			iFormulaOffset += 4;
		}
		dataProvider.WriteInt64(iFormulaOffset + 10, DEF_STRING_VALUE_LONG);
	}

	public static void SetBlankValue(DataProvider dataProvider, int iFormulaOffset, OfficeVersion version)
	{
		if (version != 0)
		{
			iFormulaOffset += 4;
		}
		dataProvider.WriteInt64(iFormulaOffset + 10, DEF_BLANK_VALUE_LONG);
	}

	public static Ptg[] ReadValue(DataProvider provider, int recordStart, OfficeVersion version)
	{
		recordStart += 10;
		if (version != 0)
		{
			recordStart += 4;
		}
		recordStart += 14;
		int iExpressionLength = provider.ReadUInt16(recordStart);
		recordStart += 2;
		int finalOffset;
		return FormulaUtil.ParseExpression(provider, recordStart, iExpressionLength, out finalOffset, version);
	}

	public static long ReadInt64Value(DataProvider provider, int recordStart, OfficeVersion version)
	{
		recordStart += 10;
		if (version != 0)
		{
			recordStart += 4;
		}
		return provider.ReadInt64(recordStart);
	}

	public static double ReadDoubleValue(DataProvider provider, int recordStart, OfficeVersion version)
	{
		recordStart += 10;
		if (version != 0)
		{
			recordStart += 4;
		}
		return provider.ReadDouble(recordStart);
	}

	public static void WriteDoubleValue(DataProvider provider, int recordStart, OfficeVersion version, double value)
	{
		recordStart += 10;
		if (version != 0)
		{
			recordStart += 4;
		}
		provider.WriteDouble(recordStart, value);
	}

	public static void UpdateOptions(DataProvider provider, int iOffset)
	{
		byte b = provider.ReadByte(iOffset + 18);
		b = (byte)(b | 3u);
		b = (byte)(b & 0xF7u);
		provider.WriteByte(iOffset + 18, b);
	}

	public new object Clone()
	{
		FormulaRecord formulaRecord = (FormulaRecord)base.Clone();
		if (m_expression != null)
		{
			int num = m_expression.Length;
			m_expression = new byte[num];
			for (int num2 = num - 1; num2 >= 0; num2--)
			{
				m_expression[num2] = formulaRecord.m_expression[num2];
			}
		}
		if (m_arrParsedExpression != null)
		{
			int num3 = m_arrParsedExpression.Length;
			m_arrParsedExpression = new Ptg[num3];
			for (int num4 = num3 - 1; num4 >= 0; num4--)
			{
				m_arrParsedExpression[num4] = (Ptg)formulaRecord.m_arrParsedExpression[num4].Clone();
			}
		}
		return formulaRecord;
	}

	public static void ConvertFormulaTokens(Ptg[] tokens, bool bFromExcel07To97)
	{
		if (tokens == null)
		{
			return;
		}
		for (int i = 0; i < tokens.Length; i++)
		{
			if (tokens[i] is AttrPtg attrPtg && (attrPtg.HasOptGoto || attrPtg.HasOptimizedIf))
			{
				ConvertFormulaGotoToken(tokens, i, bFromExcel07To97);
			}
			if (tokens[i] is AreaPtg areaPtg)
			{
				tokens[i] = areaPtg.ConvertFullRowColumnAreaPtgs(bFromExcel07To97);
				if (bFromExcel07To97 && tokens[i].TokenCode == FormulaToken.tArea3d2)
				{
					tokens[i].TokenCode = FormulaToken.tArea3d1;
				}
			}
		}
	}

	private static void ConvertFormulaGotoToken(Ptg[] formulaTokens, int iGotoTokenIndex, bool bFromExcel07To97)
	{
		ushort num = 0;
		AttrPtg attrPtg = (AttrPtg)formulaTokens[iGotoTokenIndex];
		ushort attrData = attrPtg.AttrData;
		int num2 = iGotoTokenIndex + 1;
		ushort num3 = 0;
		OfficeVersion version;
		OfficeVersion version2;
		if (bFromExcel07To97)
		{
			version = OfficeVersion.Excel2007;
			version2 = OfficeVersion.Excel97to2003;
		}
		else
		{
			version = OfficeVersion.Excel97to2003;
			version2 = OfficeVersion.Excel2007;
		}
		do
		{
			Ptg ptg = formulaTokens[num2];
			num3 += (ushort)ptg.GetSize(version2);
			num += (ushort)ptg.GetSize(version);
			num2++;
		}
		while (num < attrData);
		if (attrPtg.HasOptimizedIf)
		{
			attrPtg.AttrData = num3;
		}
		else
		{
			attrPtg.AttrData = (ushort)(num3 - 1);
		}
	}
}
