using System;
using System.IO;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.CF)]
[CLSCompliant(false)]
internal class CFRecord : BiffRecordRaw, ICloneable
{
	private const ushort DEF_MINIMUM_RECORD_SIZE = 12;

	private const int DEF_FONT_FIRST_RESERVED_SIZE = 64;

	private const int DEF_FONT_SECOND_RESERVED_SIZE = 3;

	private const int DEF_FONT_THIRD_RESERVED_SIZE = 16;

	private const uint DEF_FONT_POSTURE_MASK = 2u;

	private const uint DEF_FONT_CANCELLATION_MASK = 128u;

	private const uint DEF_FONT_STYLE_MODIFIED_MASK = 2u;

	private const uint DEF_FONT_CANCELLATION_MODIFIED_MASK = 128u;

	private const ushort DEF_BORDER_LEFT_MASK = 15;

	private const ushort DEF_BORDER_RIGHT_MASK = 240;

	private const ushort DEF_BORDER_TOP_MASK = 3840;

	private const ushort DEF_BORDER_BOTTOM_MASK = 61440;

	private const uint DEF_BORDER_LEFT_COLOR_MASK = 127u;

	private const uint DEF_BORDER_RIGHT_COLOR_MASK = 16256u;

	private const uint DEF_BORDER_TOP_COLOR_MASK = 8323072u;

	private const uint DEF_BORDER_BOTTOM_COLOR_MASK = 1065353216u;

	private const int DEF_BORDER_LEFT_COLOR_START = 0;

	private const int DEF_BORDER_RIGHT_COLOR_START = 7;

	private const int DEF_BORDER_TOP_COLOR_START = 16;

	private const int DEF_BORDER_BOTTOM_COLOR_START = 23;

	private const ushort DEF_PATTERN_MASK = 64512;

	private const ushort DEF_PATTERN_COLOR_MASK = 127;

	private const ushort DEF_PATTERN_BACKCOLOR_MASK = 16256;

	private const int DEF_PATTERN_START = 10;

	private const int DEF_PATTERN_BACKCOLOR_START = 7;

	private const int DEF_FONT_BLOCK_SIZE = 118;

	private const int DEF_BORDER_BLOCK_SIZE = 8;

	private const int DEF_PATTERN_BLOCK_SIZE = 4;

	private const int DEF_NUMBER_FORMAT_BLOCK_SIZE = 2;

	public const uint DefaultColorIndex = uint.MaxValue;

	[BiffRecordPos(0, 1)]
	private byte m_formatingType = 1;

	[BiffRecordPos(1, 1)]
	private byte m_compareOperator = 1;

	[BiffRecordPos(2, 2)]
	private ushort m_usFirstFormulaSize;

	[BiffRecordPos(4, 2)]
	private ushort m_usSecondFormulaSize;

	[BiffRecordPos(6, 4)]
	private uint m_uiOptions;

	[BiffRecordPos(10, 2)]
	private ushort m_usReserved;

	[BiffRecordPos(7, 2, TFieldType.Bit)]
	private bool m_bLeftBorder = true;

	[BiffRecordPos(7, 3, TFieldType.Bit)]
	private bool m_bRightBorder = true;

	[BiffRecordPos(7, 4, TFieldType.Bit)]
	private bool m_bTopBorder = true;

	[BiffRecordPos(7, 5, TFieldType.Bit)]
	private bool m_bBottomBorder = true;

	[BiffRecordPos(8, 0, TFieldType.Bit)]
	private bool m_bPatternStyle = true;

	[BiffRecordPos(8, 1, TFieldType.Bit)]
	private bool m_bPatternColor = true;

	[BiffRecordPos(8, 2, TFieldType.Bit)]
	private bool m_bPatternBackColor = true;

	[BiffRecordPos(8, 3, TFieldType.Bit)]
	private bool m_bNumberFormatModified = true;

	[BiffRecordPos(9, 1, TFieldType.Bit)]
	private bool m_bNumberFormatPresent;

	[BiffRecordPos(9, 2, TFieldType.Bit)]
	private bool m_bFontFormat;

	[BiffRecordPos(9, 4, TFieldType.Bit)]
	private bool m_bBorderFormat;

	[BiffRecordPos(9, 5, TFieldType.Bit)]
	private bool m_bPatternFormat;

	[BiffRecordPos(10, 0, TFieldType.Bit)]
	private bool m_numberFormatIsUserDefined;

	private uint m_uiFontHeight = uint.MaxValue;

	private uint m_uiFontOptions;

	private ushort m_usFontWeight = 400;

	private ushort m_usEscapmentType;

	private byte m_Underline;

	private uint m_uiFontColorIndex = uint.MaxValue;

	private uint m_uiModifiedFlags = 15u;

	private uint m_uiEscapmentModified = 1u;

	private uint m_uiUnderlineModified = 1u;

	private ushort m_usBorderLineStyles;

	private uint m_uiBorderColors;

	private ushort m_usPatternStyle;

	private ushort m_usPatternColors;

	private ushort m_unUsed;

	private ushort m_numFormatIndex;

	private byte[] m_arrFirstFormula = new byte[0];

	private byte[] m_arrSecondFormula = new byte[0];

	private Ptg[] m_arrFirstFormulaParsed;

	private Ptg[] m_arrSecondFormulaParsed;

	public ExcelCFType FormatType
	{
		get
		{
			return (ExcelCFType)m_formatingType;
		}
		set
		{
			m_formatingType = (byte)value;
		}
	}

	public ExcelComparisonOperator ComparisonOperator
	{
		get
		{
			return (ExcelComparisonOperator)m_compareOperator;
		}
		set
		{
			m_compareOperator = (byte)value;
		}
	}

	public ushort FirstFormulaSize => m_usFirstFormulaSize;

	public ushort SecondFormulaSize => m_usSecondFormulaSize;

	public uint Options
	{
		get
		{
			return m_uiOptions;
		}
		internal set
		{
			m_uiOptions = value;
		}
	}

	public ushort Reserved
	{
		get
		{
			return m_usReserved;
		}
		internal set
		{
			m_usReserved = value;
		}
	}

	public bool IsLeftBorderModified
	{
		get
		{
			return !m_bLeftBorder;
		}
		set
		{
			m_bLeftBorder = !value;
		}
	}

	public bool IsRightBorderModified
	{
		get
		{
			return !m_bRightBorder;
		}
		set
		{
			m_bRightBorder = !value;
		}
	}

	public bool IsTopBorderModified
	{
		get
		{
			return !m_bTopBorder;
		}
		set
		{
			m_bTopBorder = !value;
		}
	}

	public bool IsBottomBorderModified
	{
		get
		{
			return !m_bBottomBorder;
		}
		set
		{
			m_bBottomBorder = !value;
		}
	}

	public bool IsPatternStyleModified
	{
		get
		{
			return !m_bPatternStyle;
		}
		set
		{
			m_bPatternStyle = !value;
		}
	}

	public bool IsPatternColorModified
	{
		get
		{
			return !m_bPatternColor;
		}
		set
		{
			m_bPatternColor = !value;
		}
	}

	public bool IsPatternBackColorModified
	{
		get
		{
			return !m_bPatternBackColor;
		}
		set
		{
			m_bPatternBackColor = !value;
		}
	}

	public bool IsNumberFormatModified
	{
		get
		{
			return !m_bNumberFormatModified;
		}
		set
		{
			m_bNumberFormatModified = !value;
		}
	}

	public bool IsFontFormatPresent
	{
		get
		{
			return m_bFontFormat;
		}
		set
		{
			m_bFontFormat = value;
		}
	}

	public bool IsBorderFormatPresent
	{
		get
		{
			return m_bBorderFormat;
		}
		set
		{
			m_bBorderFormat = value;
		}
	}

	public bool IsPatternFormatPresent
	{
		get
		{
			return m_bPatternFormat;
		}
		set
		{
			m_bPatternFormat = value;
		}
	}

	public bool IsNumberFormatPresent
	{
		get
		{
			return m_bNumberFormatPresent;
		}
		set
		{
			m_bNumberFormatPresent = value;
		}
	}

	public override int MinimumRecordSize => 12;

	public uint FontHeight
	{
		get
		{
			return m_uiFontHeight;
		}
		set
		{
			if (m_uiFontHeight != value)
			{
				m_uiFontHeight = value;
			}
		}
	}

	public bool FontPosture
	{
		get
		{
			return (m_uiFontOptions & 2) != 0;
		}
		set
		{
			if (value != FontPosture)
			{
				m_uiFontOptions &= 4294967293u;
				if (value)
				{
					m_uiFontOptions += 2u;
				}
			}
		}
	}

	public bool FontCancellation
	{
		get
		{
			return (m_uiFontOptions & 0x80) != 0;
		}
		set
		{
			if (value != FontCancellation)
			{
				m_uiFontOptions &= 4294967167u;
				if (value)
				{
					m_uiFontOptions += 128u;
				}
			}
		}
	}

	public ushort FontWeight
	{
		get
		{
			return m_usFontWeight;
		}
		set
		{
			if (m_usFontWeight != value)
			{
				m_usFontWeight = value;
			}
		}
	}

	public OfficeFontVerticalAlignment FontEscapment
	{
		get
		{
			return (OfficeFontVerticalAlignment)m_usEscapmentType;
		}
		set
		{
			if (m_usEscapmentType != (ushort)value)
			{
				m_usEscapmentType = (ushort)value;
			}
		}
	}

	public OfficeUnderline FontUnderline
	{
		get
		{
			return (OfficeUnderline)m_Underline;
		}
		set
		{
			if (m_Underline != (byte)value)
			{
				m_Underline = (byte)value;
			}
		}
	}

	public uint FontColorIndex
	{
		get
		{
			return m_uiFontColorIndex;
		}
		set
		{
			if (m_uiFontColorIndex != value)
			{
				m_uiFontColorIndex = value;
			}
		}
	}

	public bool IsFontStyleModified
	{
		get
		{
			return (m_uiModifiedFlags & 2) == 0;
		}
		set
		{
			if (value != IsFontStyleModified)
			{
				m_uiModifiedFlags &= 4294967293u;
				if (!value)
				{
					m_uiModifiedFlags += 2u;
				}
			}
		}
	}

	public bool IsFontCancellationModified
	{
		get
		{
			return (m_uiModifiedFlags & 0x80) == 0;
		}
		set
		{
			if (value != IsFontCancellationModified)
			{
				m_uiModifiedFlags &= 4294967167u;
				if (!value)
				{
					m_uiModifiedFlags += 128u;
				}
			}
		}
	}

	public bool IsFontEscapmentModified
	{
		get
		{
			return m_uiEscapmentModified == 0;
		}
		set
		{
			m_uiEscapmentModified = ((!value) ? 1u : 0u);
		}
	}

	public bool IsFontUnderlineModified
	{
		get
		{
			return m_uiUnderlineModified == 0;
		}
		set
		{
			m_uiUnderlineModified = ((!value) ? 1u : 0u);
		}
	}

	public bool IsNumberFormatUserDefined
	{
		get
		{
			return m_numberFormatIsUserDefined;
		}
		set
		{
			m_numberFormatIsUserDefined = value;
		}
	}

	public ushort NumberFormatIndex
	{
		get
		{
			return m_numFormatIndex;
		}
		set
		{
			m_numFormatIndex = value;
		}
	}

	public OfficeLineStyle LeftBorderStyle
	{
		get
		{
			return (OfficeLineStyle)BiffRecordRaw.GetUInt16BitsByMask(m_usBorderLineStyles, 15);
		}
		set
		{
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usBorderLineStyles, 15, (ushort)value);
		}
	}

	public OfficeLineStyle RightBorderStyle
	{
		get
		{
			return (OfficeLineStyle)(BiffRecordRaw.GetUInt16BitsByMask(m_usBorderLineStyles, 240) >> 4);
		}
		set
		{
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usBorderLineStyles, 240, (ushort)((int)value << 4));
		}
	}

	public OfficeLineStyle TopBorderStyle
	{
		get
		{
			return (OfficeLineStyle)(BiffRecordRaw.GetUInt16BitsByMask(m_usBorderLineStyles, 3840) >> 8);
		}
		set
		{
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usBorderLineStyles, 3840, (ushort)((int)value << 8));
		}
	}

	public OfficeLineStyle BottomBorderStyle
	{
		get
		{
			return (OfficeLineStyle)(BiffRecordRaw.GetUInt16BitsByMask(m_usBorderLineStyles, 61440) >> 12);
		}
		set
		{
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usBorderLineStyles, 61440, (ushort)((int)value << 12));
		}
	}

	public uint LeftBorderColorIndex
	{
		get
		{
			return BiffRecordRaw.GetUInt32BitsByMask(m_uiBorderColors, 127u);
		}
		set
		{
			BiffRecordRaw.SetUInt32BitsByMask(ref m_uiBorderColors, 127u, value);
		}
	}

	public uint RightBorderColorIndex
	{
		get
		{
			return BiffRecordRaw.GetUInt32BitsByMask(m_uiBorderColors, 16256u) >> 7;
		}
		set
		{
			BiffRecordRaw.SetUInt32BitsByMask(ref m_uiBorderColors, 16256u, value << 7);
		}
	}

	public uint TopBorderColorIndex
	{
		get
		{
			return BiffRecordRaw.GetUInt32BitsByMask(m_uiBorderColors, 8323072u) >> 16;
		}
		set
		{
			BiffRecordRaw.SetUInt32BitsByMask(ref m_uiBorderColors, 8323072u, value << 16);
		}
	}

	public uint BottomBorderColorIndex
	{
		get
		{
			return BiffRecordRaw.GetUInt32BitsByMask(m_uiBorderColors, 1065353216u) >> 23;
		}
		set
		{
			BiffRecordRaw.SetUInt32BitsByMask(ref m_uiBorderColors, 1065353216u, value << 23);
		}
	}

	public OfficePattern PatternStyle
	{
		get
		{
			return (OfficePattern)(BiffRecordRaw.GetUInt16BitsByMask(m_usPatternStyle, 64512) >> 10);
		}
		set
		{
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usPatternStyle, 64512, (ushort)((int)value << 10));
		}
	}

	public ushort PatternColorIndex
	{
		get
		{
			return BiffRecordRaw.GetUInt16BitsByMask(m_usPatternColors, 127);
		}
		set
		{
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usPatternColors, 127, value);
		}
	}

	public ushort PatternBackColor
	{
		get
		{
			return (ushort)(BiffRecordRaw.GetUInt16BitsByMask(m_usPatternColors, 16256) >> 7);
		}
		set
		{
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usPatternColors, 16256, (ushort)(value << 7));
		}
	}

	public Ptg[] FirstFormulaPtgs
	{
		get
		{
			return m_arrFirstFormulaParsed;
		}
		set
		{
			m_arrFirstFormula = FormulaUtil.PtgArrayToByteArray(value, OfficeVersion.Excel2007);
			m_arrFirstFormulaParsed = value;
			m_usFirstFormulaSize = (ushort)m_arrFirstFormula.Length;
		}
	}

	public Ptg[] SecondFormulaPtgs
	{
		get
		{
			return m_arrSecondFormulaParsed;
		}
		set
		{
			m_arrSecondFormula = FormulaUtil.PtgArrayToByteArray(value, OfficeVersion.Excel2007);
			m_arrSecondFormulaParsed = value;
			m_usSecondFormulaSize = (ushort)m_arrSecondFormula.Length;
		}
	}

	public byte[] FirstFormulaBytes => m_arrFirstFormula;

	public byte[] SecondFormulaBytes => m_arrSecondFormula;

	public CFRecord()
	{
		m_uiOptions |= 3720191u;
	}

	public CFRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public CFRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_formatingType = provider.ReadByte(iOffset);
		iOffset++;
		m_compareOperator = provider.ReadByte(iOffset);
		iOffset++;
		m_usFirstFormulaSize = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usSecondFormulaSize = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_uiOptions = provider.ReadUInt32(iOffset);
		iOffset++;
		m_bLeftBorder = provider.ReadBit(iOffset, 2);
		m_bRightBorder = provider.ReadBit(iOffset, 3);
		m_bTopBorder = provider.ReadBit(iOffset, 4);
		m_bBottomBorder = provider.ReadBit(iOffset, 5);
		iOffset++;
		m_bPatternStyle = provider.ReadBit(iOffset, 0);
		m_bPatternColor = provider.ReadBit(iOffset, 1);
		m_bPatternBackColor = provider.ReadBit(iOffset, 2);
		m_bNumberFormatModified = provider.ReadBit(iOffset, 3);
		iOffset++;
		m_bNumberFormatPresent = provider.ReadBit(iOffset, 1);
		m_bFontFormat = provider.ReadBit(iOffset, 2);
		m_bBorderFormat = provider.ReadBit(iOffset, 4);
		m_bPatternFormat = provider.ReadBit(iOffset, 5);
		iOffset++;
		m_numberFormatIsUserDefined = provider.ReadBit(iOffset, 0);
		iOffset++;
		m_usReserved = provider.ReadUInt16(iOffset);
		iOffset++;
		if (!m_numberFormatIsUserDefined)
		{
			ParseNumberFormatBlock(provider, ref iOffset);
		}
		ParseFontBlock(provider, ref iOffset);
		ParseBorderBlock(provider, ref iOffset);
		ParsePatternBlock(provider, ref iOffset);
		m_arrFirstFormula = new byte[m_usFirstFormulaSize];
		provider.ReadArray(iOffset, m_arrFirstFormula);
		iOffset += m_usFirstFormulaSize;
		m_arrSecondFormula = new byte[m_usSecondFormulaSize];
		provider.ReadArray(iOffset, m_arrSecondFormula);
		m_arrFirstFormulaParsed = FormulaUtil.ParseExpression(new ByteArrayDataProvider(m_arrFirstFormula), m_usFirstFormulaSize, version);
		m_arrSecondFormulaParsed = FormulaUtil.ParseExpression(new ByteArrayDataProvider(m_arrSecondFormula), m_usSecondFormulaSize, version);
		if (version != OfficeVersion.Excel2007)
		{
			if (m_usFirstFormulaSize > 0)
			{
				m_arrFirstFormula = FormulaUtil.PtgArrayToByteArray(m_arrFirstFormulaParsed, OfficeVersion.Excel2007);
				m_usFirstFormulaSize = (ushort)m_arrFirstFormula.Length;
			}
			if (m_usSecondFormulaSize > 0)
			{
				m_arrSecondFormula = FormulaUtil.PtgArrayToByteArray(m_arrSecondFormulaParsed, OfficeVersion.Excel2007);
				m_usSecondFormulaSize = (ushort)m_arrSecondFormula.Length;
			}
		}
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		if (m_arrFirstFormulaParsed != null && m_arrFirstFormulaParsed.Length != 0)
		{
			m_arrFirstFormula = FormulaUtil.PtgArrayToByteArray(m_arrFirstFormulaParsed, version);
			m_usFirstFormulaSize = (ushort)m_arrFirstFormula.Length;
		}
		else
		{
			m_arrFirstFormula = null;
			m_usFirstFormulaSize = 0;
		}
		if (m_arrSecondFormulaParsed != null && m_arrSecondFormulaParsed.Length != 0)
		{
			m_arrSecondFormula = FormulaUtil.PtgArrayToByteArray(m_arrSecondFormulaParsed, version);
			m_usSecondFormulaSize = (ushort)m_arrSecondFormula.Length;
		}
		else
		{
			m_arrSecondFormula = null;
			m_usSecondFormulaSize = 0;
		}
		byte value = (byte)((m_formatingType == 1) ? 1 : 2);
		provider.WriteByte(iOffset, value);
		iOffset++;
		provider.WriteByte(iOffset, m_compareOperator);
		iOffset++;
		provider.WriteUInt16(iOffset, m_usFirstFormulaSize);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usSecondFormulaSize);
		iOffset += 2;
		provider.WriteUInt32(iOffset, m_uiOptions);
		iOffset++;
		provider.WriteBit(iOffset, m_bLeftBorder, 2);
		provider.WriteBit(iOffset, m_bRightBorder, 3);
		provider.WriteBit(iOffset, m_bTopBorder, 4);
		provider.WriteBit(iOffset, m_bBottomBorder, 5);
		iOffset++;
		provider.WriteBit(iOffset, m_bPatternStyle, 0);
		provider.WriteBit(iOffset, m_bPatternColor, 1);
		provider.WriteBit(iOffset, m_bPatternBackColor, 2);
		provider.WriteBit(iOffset, m_bNumberFormatModified, 3);
		iOffset++;
		provider.WriteBit(iOffset, m_bNumberFormatPresent, 1);
		provider.WriteBit(iOffset, m_bFontFormat, 2);
		provider.WriteBit(iOffset, m_bBorderFormat, 4);
		provider.WriteBit(iOffset, m_bPatternFormat, 5);
		iOffset++;
		provider.WriteBit(iOffset, m_numberFormatIsUserDefined, 0);
		iOffset++;
		provider.WriteUInt16(iOffset, m_usReserved);
		iOffset++;
		if (!m_numberFormatIsUserDefined)
		{
			SerializeNumberFormatBlock(provider, ref iOffset);
		}
		SerializeFontBlock(provider, ref iOffset);
		SerializeBorderBlock(provider, ref iOffset);
		SerializePatternBlock(provider, ref iOffset);
		_ = m_arrFirstFormula.Length;
		provider.WriteBytes(iOffset, m_arrFirstFormula, 0, m_usFirstFormulaSize);
		iOffset += m_usFirstFormulaSize;
		provider.WriteBytes(iOffset, m_arrSecondFormula, 0, m_usSecondFormulaSize);
		iOffset += m_usSecondFormulaSize;
	}

	public int ParseFontBlock(DataProvider provider, ref int iOffset)
	{
		if (!IsFontFormatPresent)
		{
			return iOffset;
		}
		iOffset += 64;
		m_uiFontHeight = provider.ReadUInt32(iOffset);
		iOffset += 4;
		m_uiFontOptions = provider.ReadUInt32(iOffset);
		iOffset += 4;
		m_usFontWeight = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usEscapmentType = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_Underline = provider.ReadByte(iOffset);
		iOffset++;
		iOffset += 3;
		m_uiFontColorIndex = provider.ReadUInt32(iOffset);
		iOffset += 4;
		iOffset += 4;
		m_uiModifiedFlags = provider.ReadUInt32(iOffset);
		iOffset += 4;
		m_uiEscapmentModified = provider.ReadUInt32(iOffset);
		iOffset += 4;
		m_uiUnderlineModified = provider.ReadUInt32(iOffset);
		iOffset += 4;
		iOffset += 16;
		iOffset += 2;
		return iOffset;
	}

	public int ParseBorderBlock(DataProvider provider, ref int iOffset)
	{
		if (!IsBorderFormatPresent)
		{
			return iOffset;
		}
		m_usBorderLineStyles = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_uiBorderColors = provider.ReadUInt32(iOffset);
		iOffset += 4;
		iOffset += 2;
		return iOffset;
	}

	public int ParsePatternBlock(DataProvider provider, ref int iOffset)
	{
		if (!IsPatternFormatPresent)
		{
			return iOffset;
		}
		m_usPatternStyle = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usPatternColors = provider.ReadUInt16(iOffset);
		iOffset += 2;
		return iOffset;
	}

	public int ParseNumberFormatBlock(DataProvider provider, ref int iOffset)
	{
		if (!IsNumberFormatPresent)
		{
			return iOffset;
		}
		m_unUsed = provider.ReadByte(iOffset);
		iOffset++;
		m_numFormatIndex = provider.ReadByte(iOffset);
		iOffset++;
		return iOffset;
	}

	public int SerializeFontBlock(DataProvider provider, ref int iOffset)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (!IsFontFormatPresent)
		{
			return iOffset;
		}
		int num = 0;
		while (num < 64)
		{
			provider.WriteByte(iOffset, 0);
			num++;
			iOffset++;
		}
		provider.WriteUInt32(iOffset, m_uiFontHeight);
		iOffset += 4;
		provider.WriteUInt32(iOffset, m_uiFontOptions);
		iOffset += 4;
		provider.WriteUInt16(iOffset, m_usFontWeight);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usEscapmentType);
		iOffset += 2;
		provider.WriteByte(iOffset, m_Underline);
		iOffset++;
		int num2 = 0;
		while (num2 < 3)
		{
			provider.WriteByte(iOffset, 0);
			num2++;
			iOffset++;
		}
		provider.WriteUInt32(iOffset, m_uiFontColorIndex);
		iOffset += 4;
		provider.WriteUInt32(iOffset, 0u);
		iOffset += 4;
		provider.WriteUInt32(iOffset, m_uiModifiedFlags);
		iOffset += 4;
		provider.WriteUInt32(iOffset, m_uiEscapmentModified);
		iOffset += 4;
		provider.WriteUInt32(iOffset, m_uiUnderlineModified);
		iOffset += 4;
		int num3 = 0;
		while (num3 < 16)
		{
			provider.WriteByte(iOffset, 0);
			num3++;
			iOffset++;
		}
		provider.WriteUInt16(iOffset, 1);
		iOffset += 2;
		return iOffset;
	}

	public int SerializeBorderBlock(DataProvider provider, ref int iOffset)
	{
		if (!IsBorderFormatPresent)
		{
			return iOffset;
		}
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		provider.WriteUInt16(iOffset, m_usBorderLineStyles);
		iOffset += 2;
		provider.WriteUInt32(iOffset, m_uiBorderColors);
		iOffset += 4;
		provider.WriteUInt16(iOffset, 0);
		iOffset += 2;
		return iOffset;
	}

	public int SerializePatternBlock(DataProvider provider, ref int iOffset)
	{
		if (!IsPatternFormatPresent)
		{
			return iOffset;
		}
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		provider.WriteUInt16(iOffset, m_usPatternStyle);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usPatternColors);
		iOffset += 2;
		return iOffset;
	}

	public int SerializeNumberFormatBlock(DataProvider provider, ref int iOffset)
	{
		if (!IsNumberFormatPresent)
		{
			return iOffset;
		}
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		provider.WriteUInt16(iOffset, m_unUsed);
		iOffset++;
		provider.WriteUInt16(iOffset, m_numFormatIndex);
		iOffset++;
		return iOffset;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		int num = 12;
		if (IsFontFormatPresent)
		{
			num += 118;
		}
		if (IsBorderFormatPresent)
		{
			num += 8;
		}
		if (IsPatternFormatPresent)
		{
			num += 4;
		}
		if (IsNumberFormatPresent)
		{
			num += 2;
		}
		num += DVRecord.GetFormulaSize(m_arrFirstFormulaParsed, version, addAdditionalDataSize: true);
		return num + DVRecord.GetFormulaSize(m_arrSecondFormulaParsed, version, addAdditionalDataSize: true);
	}

	public override object Clone()
	{
		CFRecord obj = (CFRecord)base.Clone();
		obj.m_arrFirstFormula = CloneUtils.CloneByteArray(m_arrFirstFormula);
		obj.m_arrSecondFormula = CloneUtils.CloneByteArray(m_arrSecondFormula);
		obj.m_arrFirstFormulaParsed = CloneUtils.ClonePtgArray(m_arrFirstFormulaParsed);
		obj.m_arrSecondFormulaParsed = CloneUtils.ClonePtgArray(m_arrSecondFormulaParsed);
		return obj;
	}

	public override int GetHashCode()
	{
		return m_formatingType.GetHashCode() ^ m_compareOperator.GetHashCode() ^ m_usFirstFormulaSize.GetHashCode() ^ m_usSecondFormulaSize.GetHashCode() ^ m_uiOptions.GetHashCode() ^ m_usReserved.GetHashCode() ^ m_bLeftBorder.GetHashCode() ^ m_bRightBorder.GetHashCode() ^ m_bTopBorder.GetHashCode() ^ m_bBottomBorder.GetHashCode() ^ m_bPatternStyle.GetHashCode() ^ m_bPatternColor.GetHashCode() ^ m_bPatternBackColor.GetHashCode() ^ m_bNumberFormatModified.GetHashCode() ^ m_bNumberFormatPresent.GetHashCode() ^ m_bFontFormat.GetHashCode() ^ m_bBorderFormat.GetHashCode() ^ m_bPatternFormat.GetHashCode() ^ m_uiFontHeight.GetHashCode() ^ m_uiFontOptions.GetHashCode() ^ m_usFontWeight.GetHashCode() ^ m_usEscapmentType.GetHashCode() ^ m_Underline.GetHashCode() ^ m_uiFontColorIndex.GetHashCode() ^ m_uiModifiedFlags.GetHashCode() ^ m_uiEscapmentModified.GetHashCode() ^ m_uiUnderlineModified.GetHashCode() ^ m_usBorderLineStyles.GetHashCode() ^ m_uiBorderColors.GetHashCode() ^ m_usPatternStyle.GetHashCode() ^ m_usPatternColors.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is CFRecord cFRecord))
		{
			return false;
		}
		bool flag = m_formatingType == cFRecord.m_formatingType && m_compareOperator == cFRecord.m_compareOperator && m_usFirstFormulaSize == cFRecord.m_usFirstFormulaSize && m_usSecondFormulaSize == cFRecord.m_usSecondFormulaSize && m_uiOptions == cFRecord.m_uiOptions && m_bLeftBorder == cFRecord.m_bLeftBorder && m_bRightBorder == cFRecord.m_bRightBorder && m_bTopBorder == cFRecord.m_bTopBorder && m_bBottomBorder == cFRecord.m_bBottomBorder && m_bPatternStyle == cFRecord.m_bPatternStyle && m_bPatternColor == cFRecord.m_bPatternColor && m_bPatternBackColor == cFRecord.m_bPatternBackColor && m_bNumberFormatModified == cFRecord.m_bNumberFormatModified && m_bNumberFormatPresent == cFRecord.m_bNumberFormatPresent && m_bFontFormat == cFRecord.m_bFontFormat && m_bBorderFormat == cFRecord.m_bBorderFormat && m_bPatternFormat == cFRecord.m_bPatternFormat && m_uiFontHeight == cFRecord.m_uiFontHeight && m_uiFontOptions == cFRecord.m_uiFontOptions && m_usFontWeight == cFRecord.m_usFontWeight && m_usEscapmentType == cFRecord.m_usEscapmentType && m_Underline == cFRecord.m_Underline && m_uiFontColorIndex == cFRecord.m_uiFontColorIndex && m_uiModifiedFlags == cFRecord.m_uiModifiedFlags && m_uiEscapmentModified == cFRecord.m_uiEscapmentModified && m_uiUnderlineModified == cFRecord.m_uiUnderlineModified && m_usBorderLineStyles == cFRecord.m_usBorderLineStyles && m_uiBorderColors == cFRecord.m_uiBorderColors && m_usPatternStyle == cFRecord.m_usPatternStyle && m_usPatternColors == cFRecord.m_usPatternColors;
		if (flag)
		{
			flag = BiffRecordRaw.CompareArrays(m_arrFirstFormula, cFRecord.m_arrFirstFormula) && BiffRecordRaw.CompareArrays(m_arrSecondFormula, cFRecord.m_arrSecondFormula);
		}
		return flag;
	}
}
