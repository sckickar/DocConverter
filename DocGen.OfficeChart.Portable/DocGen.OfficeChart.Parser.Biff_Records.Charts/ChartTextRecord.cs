using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartText)]
[CLSCompliant(false)]
internal class ChartTextRecord : BiffRecordRaw
{
	public const int DEF_ROTATION_MASK = 1792;

	public const int DEF_FIRST_ROTATION_BIT = 8;

	public const int DEF_DATA_LABEL_MASK = 15;

	public const int DEF_DATA_LABEL_FIRST_BIT = 0;

	public const int DEF_RECORD_SIZE = 32;

	[BiffRecordPos(0, 1)]
	private byte m_HorzAlign = 1;

	[BiffRecordPos(1, 1)]
	private byte m_VertAlign = 1;

	[BiffRecordPos(2, 2)]
	private ushort m_usBkgMode = 1;

	[BiffRecordPos(4, 4)]
	private uint m_uiTextColor;

	[BiffRecordPos(8, 4)]
	private uint m_uiXPos;

	[BiffRecordPos(12, 4)]
	private uint m_uiYPos;

	[BiffRecordPos(16, 4)]
	private uint m_uiXSize;

	[BiffRecordPos(20, 4)]
	private uint m_uiYSize;

	[BiffRecordPos(24, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(26, 2)]
	private ushort m_usColorIndex;

	[BiffRecordPos(28, 2)]
	private ushort m_usOptions2;

	[BiffRecordPos(24, 0, TFieldType.Bit)]
	private bool m_bAutoColor = true;

	[BiffRecordPos(24, 1, TFieldType.Bit)]
	private bool m_bShowKey;

	[BiffRecordPos(24, 2, TFieldType.Bit)]
	private bool m_bShowValue;

	[BiffRecordPos(24, 3, TFieldType.Bit)]
	private bool m_bVertical;

	[BiffRecordPos(24, 4, TFieldType.Bit)]
	private bool m_bAutoText = true;

	[BiffRecordPos(24, 5, TFieldType.Bit)]
	private bool m_bGenerated = true;

	[BiffRecordPos(24, 6, TFieldType.Bit)]
	private bool m_bDeleted;

	[BiffRecordPos(24, 7, TFieldType.Bit)]
	private bool m_bAutoMode = true;

	[BiffRecordPos(25, 3, TFieldType.Bit)]
	private bool m_bShowLabelPercent;

	[BiffRecordPos(25, 4, TFieldType.Bit)]
	private bool m_bShowPercent;

	[BiffRecordPos(25, 5, TFieldType.Bit)]
	private bool m_bShowBubbleSizes;

	[BiffRecordPos(25, 6, TFieldType.Bit)]
	private bool m_bShowLabel;

	[BiffRecordPos(30, 2, true)]
	private short? m_sRotation;

	public ExcelChartHorzAlignment HorzAlign
	{
		get
		{
			return (ExcelChartHorzAlignment)m_HorzAlign;
		}
		set
		{
			m_HorzAlign = (byte)value;
		}
	}

	public ExcelChartVertAlignment VertAlign
	{
		get
		{
			return (ExcelChartVertAlignment)m_VertAlign;
		}
		set
		{
			m_VertAlign = (byte)value;
		}
	}

	public OfficeChartBackgroundMode BackgroundMode
	{
		get
		{
			return (OfficeChartBackgroundMode)m_usBkgMode;
		}
		set
		{
			m_usBkgMode = (ushort)value;
		}
	}

	public uint TextColor
	{
		get
		{
			return m_uiTextColor;
		}
		set
		{
			m_uiTextColor = value;
		}
	}

	public uint XPos
	{
		get
		{
			return m_uiXPos;
		}
		set
		{
			m_uiXPos = value;
		}
	}

	public uint YPos
	{
		get
		{
			return m_uiYPos;
		}
		set
		{
			m_uiYPos = value;
		}
	}

	public uint XSize
	{
		get
		{
			return m_uiXSize;
		}
		set
		{
			m_uiXSize = value;
		}
	}

	public uint YSize
	{
		get
		{
			return m_uiYSize;
		}
		set
		{
			m_uiYSize = value;
		}
	}

	public ushort Options => m_usOptions;

	public OfficeKnownColors ColorIndex
	{
		get
		{
			return (OfficeKnownColors)m_usColorIndex;
		}
		set
		{
			m_usColorIndex = (ushort)value;
		}
	}

	public ushort Options2
	{
		get
		{
			return m_usOptions2;
		}
		set
		{
			m_usOptions2 = value;
		}
	}

	public bool IsAutoColor
	{
		get
		{
			return m_bAutoColor;
		}
		set
		{
			m_bAutoColor = value;
		}
	}

	public bool IsShowKey
	{
		get
		{
			return m_bShowKey;
		}
		set
		{
			m_bShowKey = value;
		}
	}

	public bool IsShowValue
	{
		get
		{
			return m_bShowValue;
		}
		set
		{
			m_bShowValue = value;
		}
	}

	public bool IsVertical
	{
		get
		{
			return m_bVertical;
		}
		set
		{
			m_bVertical = value;
		}
	}

	public bool IsAutoText
	{
		get
		{
			return m_bAutoText;
		}
		set
		{
			m_bAutoText = value;
		}
	}

	public bool IsGenerated
	{
		get
		{
			return m_bGenerated;
		}
		set
		{
			m_bGenerated = value;
		}
	}

	public bool IsDeleted
	{
		get
		{
			return m_bDeleted;
		}
		set
		{
			m_bDeleted = value;
		}
	}

	public bool IsAutoMode
	{
		get
		{
			return m_bAutoMode;
		}
		set
		{
			m_bAutoMode = value;
		}
	}

	public bool IsShowLabelPercent
	{
		get
		{
			return m_bShowLabelPercent;
		}
		set
		{
			m_bShowLabelPercent = value;
		}
	}

	public bool IsShowPercent
	{
		get
		{
			return m_bShowPercent;
		}
		set
		{
			m_bShowPercent = value;
		}
	}

	public bool IsShowBubbleSizes
	{
		get
		{
			return m_bShowBubbleSizes;
		}
		set
		{
			m_bShowBubbleSizes = value;
		}
	}

	public bool IsShowLabel
	{
		get
		{
			return m_bShowLabel;
		}
		set
		{
			m_bShowLabel = value;
		}
	}

	public TRotation Rotation
	{
		get
		{
			return (TRotation)(BiffRecordRaw.GetUInt16BitsByMask(m_usOptions, 1792) >> 8);
		}
		set
		{
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usOptions, 1792, (ushort)((ushort)value << 8));
		}
	}

	public OfficeDataLabelPosition DataLabelPlacement
	{
		get
		{
			return (OfficeDataLabelPosition)BiffRecordRaw.GetUInt16BitsByMask(m_usOptions2, 15);
		}
		set
		{
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usOptions2, 15, (ushort)value);
		}
	}

	public short TextRotation
	{
		get
		{
			return m_sRotation.GetValueOrDefault();
		}
		set
		{
			m_sRotation = value;
		}
	}

	public short? TextRotationOrNull
	{
		get
		{
			return m_sRotation;
		}
		set
		{
			m_sRotation = value;
		}
	}

	public override int MinimumRecordSize => 32;

	public override int MaximumRecordSize => 32;

	public ChartTextRecord()
	{
	}

	public ChartTextRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartTextRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_HorzAlign = provider.ReadByte(iOffset);
		iOffset++;
		m_VertAlign = provider.ReadByte(iOffset);
		iOffset++;
		m_usBkgMode = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_uiTextColor = provider.ReadUInt32(iOffset);
		iOffset += 4;
		m_uiXPos = provider.ReadUInt32(iOffset);
		iOffset += 4;
		m_uiYPos = provider.ReadUInt32(iOffset);
		iOffset += 4;
		m_uiXSize = provider.ReadUInt32(iOffset);
		iOffset += 4;
		m_uiYSize = provider.ReadUInt32(iOffset);
		iOffset += 4;
		m_usOptions = provider.ReadUInt16(iOffset);
		m_bAutoColor = provider.ReadBit(iOffset, 0);
		m_bShowKey = provider.ReadBit(iOffset, 1);
		m_bShowValue = provider.ReadBit(iOffset, 2);
		m_bVertical = provider.ReadBit(iOffset, 3);
		m_bAutoText = provider.ReadBit(iOffset, 4);
		m_bGenerated = provider.ReadBit(iOffset, 5);
		m_bDeleted = provider.ReadBit(iOffset, 6);
		m_bAutoMode = provider.ReadBit(iOffset, 7);
		iOffset++;
		m_bShowLabelPercent = provider.ReadBit(iOffset, 3);
		m_bShowPercent = provider.ReadBit(iOffset, 4);
		m_bShowBubbleSizes = provider.ReadBit(iOffset, 5);
		m_bShowLabel = provider.ReadBit(iOffset, 6);
		iOffset++;
		m_usColorIndex = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usOptions2 = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_sRotation = provider.ReadInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteByte(iOffset, m_HorzAlign);
		provider.WriteByte(iOffset + 1, m_VertAlign);
		provider.WriteUInt16(iOffset + 2, m_usBkgMode);
		provider.WriteUInt32(iOffset + 4, m_uiTextColor);
		provider.WriteUInt32(iOffset + 8, m_uiXPos);
		provider.WriteUInt32(iOffset + 12, m_uiYPos);
		provider.WriteUInt32(iOffset + 16, m_uiXSize);
		provider.WriteUInt32(iOffset + 20, m_uiYSize);
		provider.WriteUInt16(iOffset + 24, m_usOptions);
		provider.WriteBit(iOffset + 24, m_bAutoColor, 0);
		provider.WriteBit(iOffset + 24, m_bShowKey, 1);
		provider.WriteBit(iOffset + 24, m_bShowValue, 2);
		provider.WriteBit(iOffset + 24, m_bVertical, 3);
		provider.WriteBit(iOffset + 24, m_bAutoText, 4);
		provider.WriteBit(iOffset + 24, m_bGenerated, 5);
		provider.WriteBit(iOffset + 24, m_bDeleted, 6);
		provider.WriteBit(iOffset + 24, m_bAutoMode, 7);
		provider.WriteBit(iOffset + 25, m_bShowLabelPercent, 3);
		provider.WriteBit(iOffset + 25, m_bShowPercent, 4);
		provider.WriteBit(iOffset + 25, m_bShowBubbleSizes, 5);
		provider.WriteBit(iOffset + 25, m_bShowLabel, 6);
		provider.WriteUInt16(iOffset + 26, m_usColorIndex);
		provider.WriteUInt16(iOffset + 28, m_usOptions2);
		provider.WriteInt16(iOffset + 30, TextRotation);
		m_iLength = 32;
	}
}
