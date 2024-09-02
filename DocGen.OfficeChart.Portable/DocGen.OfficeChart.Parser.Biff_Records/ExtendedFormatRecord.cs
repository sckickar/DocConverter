using System;
using System.IO;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.ExtendedFormat)]
[CLSCompliant(false)]
internal class ExtendedFormatRecord : BiffRecordRaw
{
	public enum TXFType
	{
		XF_CELL,
		XF_STYLE
	}

	private const ushort DEF_INDENT_MASK = 15;

	private const ushort DEF_READ_ORDER_MASK = 192;

	private const ushort DEF_READ_ORDER_START_BIT = 6;

	private const ushort DEF_PARENT_INDEX_MASK = 65520;

	private const ushort DEF_ROTATION_MASK = 65280;

	private const uint DEF_TOP_BORDER_PALLETE_MASK = 127u;

	private const uint DEF_BOTTOM_BORDER_PALLETE_MASK = 16256u;

	private const uint DEF_DIAGONAL_MASK = 2080768u;

	private const uint DEF_DIAGONAL_LINE_MASK = 31457280u;

	private const uint DEF_FILL_PATTERN_MASK = 4227858432u;

	private const ushort DEF_BORDER_LEFT_MASK = 15;

	private const ushort DEF_BORDER_RIGTH_MASK = 240;

	private const ushort DEF_BORDER_TOP_MASK = 3840;

	private const ushort DEF_BORDER_BOTTOM_MASK = 61440;

	private const ushort DEF_HOR_ALIGNMENT_MASK = 7;

	private const ushort DEF_VER_ALIGNMENT_MASK = 112;

	private const ushort DEF_BACKGROUND_MASK = 127;

	private const ushort DEF_FOREGROUND_MASK = 16256;

	private const ushort DEF_LEFT_BORDER_PALLETE_MASK = 127;

	private const ushort DEF_RIGHT_BORDER_PALLETE_MASK = 16256;

	private const int DEF_RIGHT_BORDER_START_MASK = 7;

	private const int DEF_RECORD_SIZE = 20;

	private const int DEF_FILL_FOREGROUND_MASK = 16256;

	public const int DEF_DEFAULT_COLOR_INDEX = 65;

	public const int DEF_DEFAULT_PATTERN_COLOR_INDEX = 64;

	private const int DEF_XF_MAX_INDEX = 4095;

	private const int HALIGN_JUSTIFY = 5;

	private const int VALIGN_JUSTIFY = 3;

	[BiffRecordPos(0, 2)]
	private ushort m_usFontIndex;

	[BiffRecordPos(2, 2)]
	private ushort m_usFormatIndex;

	[BiffRecordPos(4, 2)]
	private ushort m_usCellOptions;

	[BiffRecordPos(4, 0, TFieldType.Bit)]
	private bool m_bLocked = true;

	[BiffRecordPos(4, 1, TFieldType.Bit)]
	private bool m_bHidden;

	[BiffRecordPos(4, 2, TFieldType.Bit)]
	private bool m_xfType;

	[BiffRecordPos(4, 3, TFieldType.Bit)]
	private bool m_b123Prefix;

	[BiffRecordPos(6, 2)]
	private ushort m_usAlignmentOptions = 32;

	[BiffRecordPos(6, 3, TFieldType.Bit)]
	private bool m_bWrapText;

	[BiffRecordPos(6, 7, TFieldType.Bit)]
	private bool m_bJustifyLast;

	[BiffRecordPos(8, 2)]
	private ushort m_usIndentOptions;

	[BiffRecordPos(8, 4, TFieldType.Bit)]
	private bool m_bShrinkToFit;

	[BiffRecordPos(8, 5, TFieldType.Bit)]
	private bool m_bMergeCells;

	[BiffRecordPos(9, 2, TFieldType.Bit)]
	private bool m_bIndentNotParentFormat;

	[BiffRecordPos(9, 3, TFieldType.Bit)]
	private bool m_bIndentNotParentFont;

	[BiffRecordPos(9, 4, TFieldType.Bit)]
	private bool m_bIndentNotParentAlignment;

	[BiffRecordPos(9, 5, TFieldType.Bit)]
	private bool m_bIndentNotParentBorder;

	[BiffRecordPos(9, 6, TFieldType.Bit)]
	private bool m_bIndentNotParentPattern;

	[BiffRecordPos(9, 7, TFieldType.Bit)]
	private bool m_bIndentNotParentCellOptions;

	private byte m_btIndent;

	[BiffRecordPos(10, 2)]
	private ushort m_usBorderOptions;

	[BiffRecordPos(12, 2)]
	private ushort m_usPaletteOptions;

	[BiffRecordPos(13, 6, TFieldType.Bit)]
	private bool m_bDiagnalFromTopLeft;

	[BiffRecordPos(13, 7, TFieldType.Bit)]
	private bool m_bDiagnalFromBottomLeft;

	[BiffRecordPos(14, 4)]
	private uint m_uiAddPaletteOptions;

	[BiffRecordPos(18, 2)]
	private ushort m_usFillPaletteOptions = 8257;

	private bool m_bHashValid;

	private int m_iHash;

	private ushort m_usParentXFIndex;

	private ushort m_usFillPattern;

	private WorkbookImpl m_book;

	private ushort m_fillIndex;

	private ushort m_borderIndex;

	public int CellOptions => (int)((((((((0u | (m_bLocked ? 1u : 0u)) << 1) | (m_bHidden ? 1u : 0u)) << 1) | (m_bMergeCells ? 1u : 0u)) << 1) | (m_bShrinkToFit ? 1u : 0u)) << 1);

	public int BorderOptions => (int)(((((0u | (m_bDiagnalFromTopLeft ? 1u : 0u)) << 1) | (m_bDiagnalFromBottomLeft ? 1u : 0u)) << 15) | m_usBorderOptions);

	public int AlignmentOptions => m_usAlignmentOptions;

	public ushort FontIndex
	{
		get
		{
			return m_usFontIndex;
		}
		set
		{
			if (value == 4)
			{
				throw new ArgumentException("FontIndex must be less than or higher than 4 for ExtendedFormatRecords.");
			}
			m_bHashValid = false;
			m_usFontIndex = value;
		}
	}

	internal ushort FillIndex
	{
		get
		{
			return m_fillIndex;
		}
		set
		{
			m_fillIndex = value;
		}
	}

	internal ushort BorderIndex
	{
		get
		{
			return m_borderIndex;
		}
		set
		{
			m_borderIndex = value;
		}
	}

	public ushort FormatIndex
	{
		get
		{
			return m_usFormatIndex;
		}
		set
		{
			m_bHashValid = false;
			m_usFormatIndex = value;
		}
	}

	public bool IsLocked
	{
		get
		{
			return m_bLocked;
		}
		set
		{
			m_bHashValid = false;
			m_bLocked = value;
		}
	}

	public bool IsHidden
	{
		get
		{
			return m_bHidden;
		}
		set
		{
			m_bHashValid = false;
			m_bHidden = value;
		}
	}

	public TXFType XFType
	{
		get
		{
			if (!m_xfType)
			{
				return TXFType.XF_STYLE;
			}
			return TXFType.XF_CELL;
		}
		set
		{
			m_bHashValid = false;
			m_xfType = value == TXFType.XF_CELL;
		}
	}

	public bool _123Prefix
	{
		get
		{
			return m_b123Prefix;
		}
		set
		{
			m_bHashValid = false;
			m_b123Prefix = value;
		}
	}

	public ushort ParentIndex
	{
		get
		{
			return m_usParentXFIndex;
		}
		set
		{
			m_usParentXFIndex = value;
		}
	}

	public bool WrapText
	{
		get
		{
			return m_bWrapText;
		}
		set
		{
			if (m_bWrapText != value)
			{
				m_bHashValid = false;
				m_bWrapText = value;
				SetBitInVar(ref m_usAlignmentOptions, m_bWrapText, 3);
			}
		}
	}

	public bool JustifyLast
	{
		get
		{
			return m_bJustifyLast;
		}
		set
		{
			if (m_bJustifyLast != value)
			{
				m_bHashValid = false;
				m_bJustifyLast = value;
				SetBitInVar(ref m_usAlignmentOptions, m_bJustifyLast, 7);
			}
		}
	}

	public byte Indent
	{
		get
		{
			return m_btIndent;
		}
		set
		{
			m_bHashValid = false;
			m_btIndent = value;
		}
	}

	public bool ShrinkToFit
	{
		get
		{
			return m_bShrinkToFit;
		}
		set
		{
			m_bHashValid = false;
			m_bShrinkToFit = value;
		}
	}

	public bool MergeCells
	{
		get
		{
			return m_bMergeCells;
		}
		set
		{
			m_bHashValid = false;
			m_bMergeCells = value;
		}
	}

	public ushort ReadingOrder
	{
		get
		{
			return (ushort)(BiffRecordRaw.GetUInt16BitsByMask(m_usIndentOptions, 192) >> 6);
		}
		set
		{
			if (value > 3)
			{
				throw new ArgumentOutOfRangeException("Reading Order");
			}
			m_bHashValid = false;
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usIndentOptions, 192, (ushort)(value << 6));
		}
	}

	public ushort Rotation
	{
		get
		{
			return (ushort)(BiffRecordRaw.GetUInt16BitsByMask(m_usAlignmentOptions, 65280) >> 8);
		}
		set
		{
			if (value > 255)
			{
				throw new ArgumentOutOfRangeException("Rotation");
			}
			m_bHashValid = false;
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usAlignmentOptions, 65280, (ushort)(value << 8));
		}
	}

	public bool IsNotParentFormat
	{
		get
		{
			return m_bIndentNotParentFormat;
		}
		set
		{
			m_bHashValid = false;
			m_bIndentNotParentFormat = value;
		}
	}

	public bool IsNotParentFont
	{
		get
		{
			return m_bIndentNotParentFont;
		}
		set
		{
			m_bHashValid = false;
			m_bIndentNotParentFont = value;
		}
	}

	public bool IsNotParentAlignment
	{
		get
		{
			return m_bIndentNotParentAlignment;
		}
		set
		{
			m_bHashValid = false;
			m_bIndentNotParentAlignment = value;
		}
	}

	public bool IsNotParentBorder
	{
		get
		{
			return m_bIndentNotParentBorder;
		}
		set
		{
			m_bHashValid = false;
			m_bIndentNotParentBorder = value;
		}
	}

	public bool IsNotParentPattern
	{
		get
		{
			return m_bIndentNotParentPattern;
		}
		set
		{
			m_bHashValid = false;
			m_bIndentNotParentPattern = value;
		}
	}

	public bool IsNotParentCellOptions
	{
		get
		{
			return m_bIndentNotParentCellOptions;
		}
		set
		{
			m_bHashValid = false;
			m_bIndentNotParentCellOptions = value;
		}
	}

	public ushort TopBorderPaletteIndex
	{
		get
		{
			return (ushort)(m_uiAddPaletteOptions & 0x7Fu);
		}
		set
		{
			if (value > 127)
			{
				throw new ArgumentOutOfRangeException();
			}
			m_bHashValid = false;
			m_uiAddPaletteOptions &= 4294967168u;
			m_uiAddPaletteOptions += value;
		}
	}

	public ushort BottomBorderPaletteIndex
	{
		get
		{
			return (ushort)((m_uiAddPaletteOptions & 0x3F80) >> 7);
		}
		set
		{
			if (value > 127)
			{
				throw new ArgumentOutOfRangeException();
			}
			m_bHashValid = false;
			m_uiAddPaletteOptions &= 4294951039u;
			m_uiAddPaletteOptions += (uint)(value << 7);
		}
	}

	public ushort LeftBorderPaletteIndex
	{
		get
		{
			return (ushort)(m_usPaletteOptions & 0x7Fu);
		}
		set
		{
			if (value > 127)
			{
				throw new ArgumentOutOfRangeException();
			}
			m_bHashValid = false;
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usPaletteOptions, 127, value);
		}
	}

	public ushort RightBorderPaletteIndex
	{
		get
		{
			return (ushort)(BiffRecordRaw.GetUInt16BitsByMask(m_usPaletteOptions, 16256) >> 7);
		}
		set
		{
			if (value > 127)
			{
				throw new ArgumentOutOfRangeException();
			}
			m_bHashValid = false;
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usPaletteOptions, 16256, (ushort)(value << 7));
		}
	}

	public ushort DiagonalLineColor
	{
		get
		{
			return (ushort)((m_uiAddPaletteOptions & 0x1FC000) >> 14);
		}
		set
		{
			if (value > 127)
			{
				throw new ArgumentOutOfRangeException();
			}
			m_bHashValid = false;
			m_uiAddPaletteOptions &= 4292886527u;
			m_uiAddPaletteOptions |= (uint)(value << 14);
		}
	}

	public ushort DiagonalLineStyle
	{
		get
		{
			return (ushort)((m_uiAddPaletteOptions & 0x1E00000) >> 21);
		}
		set
		{
			if (value > 15)
			{
				throw new ArgumentOutOfRangeException();
			}
			m_bHashValid = false;
			m_uiAddPaletteOptions &= 4263510015u;
			m_uiAddPaletteOptions |= (uint)(value << 21);
		}
	}

	public bool DiagonalFromTopLeft
	{
		get
		{
			return m_bDiagnalFromTopLeft;
		}
		set
		{
			if (m_bDiagnalFromTopLeft != value)
			{
				m_bHashValid = false;
				m_bDiagnalFromTopLeft = value;
				SetBitInVar(ref m_usPaletteOptions, m_bDiagnalFromTopLeft, 14);
			}
		}
	}

	public bool DiagonalFromBottomLeft
	{
		get
		{
			return m_bDiagnalFromBottomLeft;
		}
		set
		{
			if (m_bDiagnalFromBottomLeft != value)
			{
				m_bHashValid = false;
				m_bDiagnalFromBottomLeft = value;
				SetBitInVar(ref m_usPaletteOptions, m_bDiagnalFromBottomLeft, 15);
			}
		}
	}

	public ushort AdtlFillPattern
	{
		get
		{
			return m_usFillPattern;
		}
		set
		{
			m_usFillPattern = value;
		}
	}

	public OfficeLineStyle BorderLeft
	{
		get
		{
			return (OfficeLineStyle)BiffRecordRaw.GetUInt16BitsByMask(m_usBorderOptions, 15);
		}
		set
		{
			m_bHashValid = false;
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usBorderOptions, 15, (ushort)value);
			if (value != 0 && LeftBorderPaletteIndex == 0)
			{
				LeftBorderPaletteIndex = 64;
			}
			else if (value == OfficeLineStyle.None)
			{
				LeftBorderPaletteIndex = 0;
			}
		}
	}

	public OfficeLineStyle BorderRight
	{
		get
		{
			return (OfficeLineStyle)(BiffRecordRaw.GetUInt16BitsByMask(m_usBorderOptions, 240) >> 4);
		}
		set
		{
			m_bHashValid = false;
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usBorderOptions, 240, (ushort)((ushort)value << 4));
			if (value != 0 && RightBorderPaletteIndex == 0)
			{
				RightBorderPaletteIndex = 64;
			}
			else if (value == OfficeLineStyle.None)
			{
				RightBorderPaletteIndex = 0;
			}
		}
	}

	public OfficeLineStyle BorderTop
	{
		get
		{
			return (OfficeLineStyle)(BiffRecordRaw.GetUInt16BitsByMask(m_usBorderOptions, 3840) >> 8);
		}
		set
		{
			m_bHashValid = false;
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usBorderOptions, 3840, (ushort)((ushort)value << 8));
			if (value != 0 && TopBorderPaletteIndex == 0)
			{
				TopBorderPaletteIndex = 64;
			}
			else if (value == OfficeLineStyle.None)
			{
				TopBorderPaletteIndex = 0;
			}
		}
	}

	public OfficeLineStyle BorderBottom
	{
		get
		{
			return (OfficeLineStyle)(BiffRecordRaw.GetUInt16BitsByMask(m_usBorderOptions, 61440) >> 12);
		}
		set
		{
			m_bHashValid = false;
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usBorderOptions, 61440, (ushort)((ushort)value << 12));
			if (value != 0 && BottomBorderPaletteIndex == 0)
			{
				BottomBorderPaletteIndex = 64;
			}
			else if (value == OfficeLineStyle.None)
			{
				BottomBorderPaletteIndex = 0;
			}
		}
	}

	public OfficeHAlign HAlignmentType
	{
		get
		{
			return (OfficeHAlign)BiffRecordRaw.GetUInt16BitsByMask(m_usAlignmentOptions, 7);
		}
		set
		{
			m_bHashValid = false;
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usAlignmentOptions, 7, (ushort)value);
		}
	}

	public OfficeVAlign VAlignmentType
	{
		get
		{
			return (OfficeVAlign)(BiffRecordRaw.GetUInt16BitsByMask(m_usAlignmentOptions, 112) >> 4);
		}
		set
		{
			m_bHashValid = false;
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usAlignmentOptions, 112, (ushort)((ushort)value << 4));
		}
	}

	public ushort FillBackground
	{
		get
		{
			return BiffRecordRaw.GetUInt16BitsByMask(m_usFillPaletteOptions, 127);
		}
		set
		{
			if (value > 127)
			{
				throw new ArgumentOutOfRangeException();
			}
			m_bHashValid = false;
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usFillPaletteOptions, 127, value);
		}
	}

	public ushort FillForeground
	{
		get
		{
			return (ushort)((m_usFillPaletteOptions & 0x3F80) >> 7);
		}
		set
		{
			if (value > 127)
			{
				throw new ArgumentOutOfRangeException("FillForeground", "Argument is too large");
			}
			m_bHashValid = false;
			m_usFillPaletteOptions &= 49279;
			m_usFillPaletteOptions |= (ushort)(value << 7);
		}
	}

	public override int MinimumRecordSize => 20;

	public override int MaximumRecordSize => 20;

	public ExtendedFormatRecord()
	{
	}

	public ExtendedFormatRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ExtendedFormatRecord(int iReserve)
		: base(iReserve)
	{
		m_iCode = 224;
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usFontIndex = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usFormatIndex = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usCellOptions = provider.ReadUInt16(iOffset);
		m_xfType = provider.ReadBit(iOffset, 2);
		m_b123Prefix = provider.ReadBit(iOffset, 3);
		m_bLocked = provider.ReadBit(iOffset, 0);
		m_bHidden = provider.ReadBit(iOffset, 1);
		iOffset += 2;
		m_usAlignmentOptions = provider.ReadUInt16(iOffset);
		m_bJustifyLast = provider.ReadBit(iOffset, 7);
		m_bWrapText = provider.ReadBit(iOffset, 3);
		iOffset += 2;
		int num = BiffRecordRaw.GetUInt16BitsByMask(m_usAlignmentOptions, 112) >> 4;
		if (BiffRecordRaw.GetUInt16BitsByMask(m_usAlignmentOptions, 7) == 5 || num == 3)
		{
			m_bWrapText = true;
		}
		m_usIndentOptions = provider.ReadUInt16(iOffset);
		m_bMergeCells = provider.ReadBit(iOffset, 5);
		m_bShrinkToFit = provider.ReadBit(iOffset, 4);
		m_btIndent = (byte)BiffRecordRaw.GetUInt16BitsByMask(m_usIndentOptions, 15);
		m_usIndentOptions = (ushort)(m_usIndentOptions & 0xFFFFFFF0u);
		iOffset++;
		m_bIndentNotParentBorder = provider.ReadBit(iOffset, 5);
		m_bIndentNotParentPattern = provider.ReadBit(iOffset, 6);
		m_bIndentNotParentCellOptions = provider.ReadBit(iOffset, 7);
		m_bIndentNotParentFormat = provider.ReadBit(iOffset, 2);
		m_bIndentNotParentFont = provider.ReadBit(iOffset, 3);
		m_bIndentNotParentAlignment = provider.ReadBit(iOffset, 4);
		iOffset++;
		m_usBorderOptions = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usPaletteOptions = provider.ReadUInt16(iOffset);
		iOffset++;
		m_bDiagnalFromBottomLeft = provider.ReadBit(iOffset, 7);
		m_bDiagnalFromTopLeft = provider.ReadBit(iOffset, 6);
		iOffset++;
		m_uiAddPaletteOptions = provider.ReadUInt32(iOffset);
		m_usFillPattern = (ushort)((m_uiAddPaletteOptions & 0xFC000000u) >> 26);
		iOffset += 4;
		m_usFillPaletteOptions = provider.ReadUInt16(iOffset);
		if (m_usFillPattern == 0 && FillForeground == 0)
		{
			m_usFillPaletteOptions = 8328;
		}
		m_iLength = 20;
		SwapColors();
		m_usParentXFIndex = (ushort)(BiffRecordRaw.GetUInt16BitsByMask(m_usCellOptions, 65520) >> 4);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		if (m_usParentXFIndex > 4095)
		{
			m_usParentXFIndex = 4095;
		}
		m_bHashValid = false;
		BiffRecordRaw.SetUInt16BitsByMask(ref m_usCellOptions, 65520, (ushort)(m_usParentXFIndex << 4));
		SwapColors();
		provider.WriteUInt16(iOffset, m_usFontIndex);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usFormatIndex);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usCellOptions);
		provider.WriteBit(iOffset, m_xfType, 2);
		provider.WriteBit(iOffset, m_b123Prefix, 3);
		provider.WriteBit(iOffset, m_bLocked, 0);
		provider.WriteBit(iOffset, m_bHidden, 1);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usAlignmentOptions);
		iOffset += 2;
		BiffRecordRaw.SetUInt16BitsByMask(ref m_usIndentOptions, 15, m_btIndent);
		provider.WriteUInt16(iOffset, m_usIndentOptions);
		provider.WriteBit(iOffset, m_bMergeCells, 5);
		provider.WriteBit(iOffset, m_bShrinkToFit, 4);
		iOffset++;
		provider.WriteBit(iOffset, m_bIndentNotParentBorder, 5);
		provider.WriteBit(iOffset, m_bIndentNotParentPattern, 6);
		provider.WriteBit(iOffset, m_bIndentNotParentCellOptions, 7);
		provider.WriteBit(iOffset, m_bIndentNotParentFormat, 2);
		provider.WriteBit(iOffset, m_bIndentNotParentFont, 3);
		provider.WriteBit(iOffset, m_bIndentNotParentAlignment, 4);
		iOffset++;
		provider.WriteUInt16(iOffset, m_usBorderOptions);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usPaletteOptions);
		iOffset++;
		provider.WriteBit(iOffset, m_bDiagnalFromBottomLeft, 7);
		provider.WriteBit(iOffset, m_bDiagnalFromTopLeft, 6);
		iOffset++;
		m_usFillPattern = (ushort)((m_usFillPattern == 4000) ? 1 : m_usFillPattern);
		if (m_usFillPattern > 63)
		{
			throw new ArgumentOutOfRangeException();
		}
		m_bHashValid = false;
		m_uiAddPaletteOptions &= 67108863u;
		m_uiAddPaletteOptions += (uint)(m_usFillPattern << 26);
		provider.WriteInt32(iOffset, (int)m_uiAddPaletteOptions);
		iOffset += 4;
		provider.WriteUInt16(iOffset, m_usFillPaletteOptions);
		m_iLength = 20;
		SwapColors();
	}

	public int CompareTo(ExtendedFormatRecord twin)
	{
		if (twin == null)
		{
			throw new ArgumentNullException("twin");
		}
		bool num = m_bLocked;
		byte b = (twin.m_bLocked ? ((byte)1) : ((byte)0));
		int num2 = (num ? 1 : 0) - (int)b;
		if (num2 != 0)
		{
			return num2;
		}
		bool num3 = m_bHidden;
		b = (twin.m_bHidden ? ((byte)1) : ((byte)0));
		num2 = (num3 ? 1 : 0) - (int)b;
		if (num2 != 0)
		{
			return num2;
		}
		num2 = m_xfType.CompareTo(twin.m_xfType);
		if (num2 != 0)
		{
			return num2;
		}
		num2 = m_usAlignmentOptions - twin.m_usAlignmentOptions;
		if (num2 != 0)
		{
			return num2;
		}
		num2 = Indent - twin.Indent;
		if (num2 != 0)
		{
			return num2;
		}
		num2 = ReadingOrder - twin.ReadingOrder;
		if (num2 != 0)
		{
			return num2;
		}
		bool num4 = m_bShrinkToFit;
		b = (twin.m_bShrinkToFit ? ((byte)1) : ((byte)0));
		num2 = (num4 ? 1 : 0) - (int)b;
		if (num2 != 0)
		{
			return num2;
		}
		bool num5 = m_bMergeCells;
		b = (twin.m_bMergeCells ? ((byte)1) : ((byte)0));
		num2 = (num5 ? 1 : 0) - (int)b;
		if (num2 != 0)
		{
			return num2;
		}
		bool num6 = m_bIndentNotParentFormat;
		b = (twin.m_bIndentNotParentFormat ? ((byte)1) : ((byte)0));
		num2 = (num6 ? 1 : 0) - (int)b;
		if (num2 != 0)
		{
			return num2;
		}
		bool num7 = m_bIndentNotParentFont;
		b = (twin.m_bIndentNotParentFont ? ((byte)1) : ((byte)0));
		num2 = (num7 ? 1 : 0) - (int)b;
		if (num2 != 0)
		{
			return num2;
		}
		bool num8 = m_bIndentNotParentAlignment;
		b = (twin.m_bIndentNotParentAlignment ? ((byte)1) : ((byte)0));
		num2 = (num8 ? 1 : 0) - (int)b;
		if (num2 != 0)
		{
			return num2;
		}
		bool num9 = m_bIndentNotParentBorder;
		b = (twin.m_bIndentNotParentBorder ? ((byte)1) : ((byte)0));
		num2 = (num9 ? 1 : 0) - (int)b;
		if (num2 != 0)
		{
			return num2;
		}
		bool num10 = m_bIndentNotParentPattern;
		b = (twin.m_bIndentNotParentPattern ? ((byte)1) : ((byte)0));
		num2 = (num10 ? 1 : 0) - (int)b;
		if (num2 != 0)
		{
			return num2;
		}
		bool num11 = m_bIndentNotParentCellOptions;
		b = (twin.m_bIndentNotParentCellOptions ? ((byte)1) : ((byte)0));
		num2 = (num11 ? 1 : 0) - (int)b;
		if (num2 != 0)
		{
			return num2;
		}
		num2 = m_usBorderOptions - twin.m_usBorderOptions;
		if (num2 != 0)
		{
			return num2;
		}
		num2 = m_usPaletteOptions - twin.m_usPaletteOptions;
		if (num2 != 0)
		{
			return num2;
		}
		long num12 = (long)m_uiAddPaletteOptions - (long)twin.m_uiAddPaletteOptions;
		if (num12 != 0L)
		{
			if (num12 <= 0)
			{
				return -1;
			}
			return 1;
		}
		num2 = m_usFillPaletteOptions - twin.m_usFillPaletteOptions;
		if (num2 != 0)
		{
			return num2;
		}
		num2 = m_usFillPattern - twin.m_usFillPattern;
		if (num2 != 0)
		{
			return num2;
		}
		bool num13 = m_b123Prefix;
		b = (twin.m_b123Prefix ? ((byte)1) : ((byte)0));
		num2 = (num13 ? 1 : 0) - (int)b;
		if (num2 != 0)
		{
			return num2;
		}
		num2 = m_usFormatIndex - twin.m_usFormatIndex;
		if (num2 != 0)
		{
			return num2;
		}
		num2 = m_usFontIndex - twin.m_usFontIndex;
		if (num2 != 0)
		{
			return num2;
		}
		return m_usParentXFIndex - twin.m_usParentXFIndex;
	}

	public override int GetHashCode()
	{
		if (!m_bHashValid)
		{
			m_iHash = m_usFontIndex.GetHashCode() ^ m_usFormatIndex.GetHashCode() ^ (m_usCellOptions & 0xFFF0).GetHashCode() ^ m_bLocked.GetHashCode() ^ m_bHidden.GetHashCode() ^ m_xfType.GetHashCode() ^ m_b123Prefix.GetHashCode() ^ m_usAlignmentOptions.GetHashCode() ^ m_bWrapText.GetHashCode() ^ m_bJustifyLast.GetHashCode() ^ m_usIndentOptions.GetHashCode() ^ m_bShrinkToFit.GetHashCode() ^ m_bMergeCells.GetHashCode() ^ m_bIndentNotParentFormat.GetHashCode() ^ m_bIndentNotParentFont.GetHashCode() ^ m_bIndentNotParentAlignment.GetHashCode() ^ m_bIndentNotParentBorder.GetHashCode() ^ m_bIndentNotParentPattern.GetHashCode() ^ m_bIndentNotParentCellOptions.GetHashCode() ^ m_usBorderOptions.GetHashCode() ^ m_usPaletteOptions.GetHashCode() ^ m_bDiagnalFromTopLeft.GetHashCode() ^ m_bDiagnalFromBottomLeft.GetHashCode() ^ m_uiAddPaletteOptions.GetHashCode() ^ m_usFillPattern.GetHashCode() ^ m_usFillPaletteOptions.GetHashCode();
			m_bHashValid = true;
		}
		return m_iHash;
	}

	private void SwapColors()
	{
		if (AdtlFillPattern != 1)
		{
			ushort fillBackground = FillBackground;
			ushort fillForeground = FillForeground;
			FillBackground = fillForeground;
			FillForeground = fillBackground;
		}
	}

	public void CopyBorders(ExtendedFormatRecord source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		m_usBorderOptions = source.m_usBorderOptions;
		BorderBottom = source.BorderBottom;
		BorderLeft = source.BorderLeft;
		BorderRight = source.BorderRight;
		BorderTop = source.BorderTop;
		BottomBorderPaletteIndex = source.BottomBorderPaletteIndex;
		LeftBorderPaletteIndex = source.LeftBorderPaletteIndex;
		RightBorderPaletteIndex = source.RightBorderPaletteIndex;
		TopBorderPaletteIndex = source.TopBorderPaletteIndex;
		DiagonalFromBottomLeft = source.DiagonalFromBottomLeft;
		DiagonalFromTopLeft = source.DiagonalFromTopLeft;
		DiagonalLineColor = source.DiagonalLineColor;
		DiagonalLineStyle = source.DiagonalLineStyle;
	}

	public void CopyAlignment(ExtendedFormatRecord source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		m_usAlignmentOptions = source.m_usAlignmentOptions;
		MergeCells = source.MergeCells;
		Rotation = source.Rotation;
		ShrinkToFit = source.ShrinkToFit;
		Indent = source.Indent;
	}

	public void CopyPatterns(ExtendedFormatRecord source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		AdtlFillPattern = source.AdtlFillPattern;
		FillBackground = source.FillBackground;
		FillForeground = source.FillForeground;
	}

	public void CopyProtection(ExtendedFormatRecord source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("m_extFormat");
		}
		IsLocked = source.IsLocked;
		IsHidden = source.IsHidden;
	}

	public void CopyTo(ExtendedFormatRecord twin)
	{
		twin.m_b123Prefix = m_b123Prefix;
		twin.m_bDiagnalFromBottomLeft = m_bDiagnalFromBottomLeft;
		twin.m_bDiagnalFromTopLeft = m_bDiagnalFromTopLeft;
		twin.m_bHashValid = m_bHashValid;
		twin.m_bHidden = m_bHidden;
		twin.m_bIndentNotParentAlignment = m_bIndentNotParentAlignment;
		twin.m_bIndentNotParentBorder = m_bIndentNotParentBorder;
		twin.m_bIndentNotParentCellOptions = m_bIndentNotParentCellOptions;
		twin.m_bIndentNotParentFont = m_bIndentNotParentFont;
		twin.m_bIndentNotParentFormat = m_bIndentNotParentFormat;
		twin.m_bIndentNotParentPattern = m_bIndentNotParentPattern;
		twin.m_bJustifyLast = m_bJustifyLast;
		twin.m_bLocked = m_bLocked;
		twin.m_bMergeCells = m_bMergeCells;
		twin.m_bShrinkToFit = m_bShrinkToFit;
		twin.m_bWrapText = m_bWrapText;
		twin.m_iCode = m_iCode;
		twin.m_iHash = m_iHash;
		twin.m_iLength = m_iLength;
		twin.m_uiAddPaletteOptions = m_uiAddPaletteOptions;
		twin.m_usAlignmentOptions = m_usAlignmentOptions;
		twin.m_usBorderOptions = m_usBorderOptions;
		twin.m_usCellOptions = m_usCellOptions;
		twin.m_usFillPaletteOptions = m_usFillPaletteOptions;
		twin.m_usFontIndex = m_usFontIndex;
		twin.m_usFormatIndex = m_usFormatIndex;
		twin.m_usIndentOptions = m_usIndentOptions;
		twin.m_usPaletteOptions = m_usPaletteOptions;
		twin.m_xfType = m_xfType;
		twin.m_usParentXFIndex = m_usParentXFIndex;
		twin.m_usFillPattern = m_usFillPattern;
	}

	internal void SetWorkbook(WorkbookImpl book)
	{
		m_book = book;
	}

	public override void CopyTo(BiffRecordRaw raw)
	{
		if (raw == null)
		{
			throw new ArgumentNullException("raw");
		}
		if (raw is ExtendedFormatRecord twin)
		{
			CopyTo(twin);
			return;
		}
		throw new ArgumentException("raw");
	}
}
