using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartTick)]
[CLSCompliant(false)]
internal class ChartTickRecord : BiffRecordRaw
{
	public const ushort DEF_ROTATION_MASK = 28;

	public const ushort DEF_FIRST_ROTATION_BIT = 2;

	public const int DEF_RECORD_SIZE = 30;

	private const int DEF_MAX_ANGLE = 90;

	private const int ReservedFieldSize = 16;

	[BiffRecordPos(0, 1)]
	private byte m_MajorMark = 2;

	[BiffRecordPos(1, 1)]
	private byte m_MinorMark;

	[BiffRecordPos(2, 1)]
	private byte m_labelPos = 3;

	[BiffRecordPos(3, 1)]
	private byte m_BackgroundMode = 1;

	[BiffRecordPos(4, 4)]
	private uint m_uiTextColor;

	[BiffRecordPos(24, 0, TFieldType.Bit)]
	private bool m_bAutoTextColor = true;

	[BiffRecordPos(24, 2)]
	private ushort m_usFlags;

	[BiffRecordPos(24, 1, TFieldType.Bit)]
	private bool m_bAutoTextBack = true;

	[BiffRecordPos(24, 5, TFieldType.Bit)]
	private bool m_bAutoRotation = true;

	[BiffRecordPos(26, 2)]
	private ushort m_usTickColorIndex;

	[BiffRecordPos(28, 2, true)]
	private short m_sRotationAngle;

	[BiffRecordPos(25, 6, TFieldType.Bit)]
	private bool m_bIsLeftToRight;

	[BiffRecordPos(25, 7, TFieldType.Bit)]
	private bool m_bIsRightToLeft;

	public OfficeTickMark MajorMark
	{
		get
		{
			return (OfficeTickMark)m_MajorMark;
		}
		set
		{
			m_MajorMark = (byte)value;
		}
	}

	public OfficeTickMark MinorMark
	{
		get
		{
			return (OfficeTickMark)m_MinorMark;
		}
		set
		{
			m_MinorMark = (byte)value;
		}
	}

	public OfficeTickLabelPosition LabelPos
	{
		get
		{
			return (OfficeTickLabelPosition)m_labelPos;
		}
		set
		{
			m_labelPos = (byte)value;
		}
	}

	public OfficeChartBackgroundMode BackgroundMode
	{
		get
		{
			return (OfficeChartBackgroundMode)m_BackgroundMode;
		}
		set
		{
			m_BackgroundMode = (byte)value;
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

	public ushort Flags => m_usFlags;

	public ushort TickColorIndex
	{
		get
		{
			return m_usTickColorIndex;
		}
		set
		{
			m_usTickColorIndex = value;
		}
	}

	public short RotationAngle
	{
		get
		{
			if (m_sRotationAngle > 90)
			{
				return (short)(90 - m_sRotationAngle);
			}
			return m_sRotationAngle;
		}
		set
		{
			switch (value)
			{
			default:
				throw new ArgumentOutOfRangeException("value", "Value cannot be less -90 and greater than 90");
			case 0:
			case 1:
			case 2:
			case 3:
			case 4:
			case 5:
			case 6:
			case 7:
			case 8:
			case 9:
			case 10:
			case 11:
			case 12:
			case 13:
			case 14:
			case 15:
			case 16:
			case 17:
			case 18:
			case 19:
			case 20:
			case 21:
			case 22:
			case 23:
			case 24:
			case 25:
			case 26:
			case 27:
			case 28:
			case 29:
			case 30:
			case 31:
			case 32:
			case 33:
			case 34:
			case 35:
			case 36:
			case 37:
			case 38:
			case 39:
			case 40:
			case 41:
			case 42:
			case 43:
			case 44:
			case 45:
			case 46:
			case 47:
			case 48:
			case 49:
			case 50:
			case 51:
			case 52:
			case 53:
			case 54:
			case 55:
			case 56:
			case 57:
			case 58:
			case 59:
			case 60:
			case 61:
			case 62:
			case 63:
			case 64:
			case 65:
			case 66:
			case 67:
			case 68:
			case 69:
			case 70:
			case 71:
			case 72:
			case 73:
			case 74:
			case 75:
			case 76:
			case 77:
			case 78:
			case 79:
			case 80:
			case 81:
			case 82:
			case 83:
			case 84:
			case 85:
			case 86:
			case 87:
			case 88:
			case 89:
			case 90:
				m_sRotationAngle = value;
				break;
			case -90:
			case -89:
			case -88:
			case -87:
			case -86:
			case -85:
			case -84:
			case -83:
			case -82:
			case -81:
			case -80:
			case -79:
			case -78:
			case -77:
			case -76:
			case -75:
			case -74:
			case -73:
			case -72:
			case -71:
			case -70:
			case -69:
			case -68:
			case -67:
			case -66:
			case -65:
			case -64:
			case -63:
			case -62:
			case -61:
			case -60:
			case -59:
			case -58:
			case -57:
			case -56:
			case -55:
			case -54:
			case -53:
			case -52:
			case -51:
			case -50:
			case -49:
			case -48:
			case -47:
			case -46:
			case -45:
			case -44:
			case -43:
			case -42:
			case -41:
			case -40:
			case -39:
			case -38:
			case -37:
			case -36:
			case -35:
			case -34:
			case -33:
			case -32:
			case -31:
			case -30:
			case -29:
			case -28:
			case -27:
			case -26:
			case -25:
			case -24:
			case -23:
			case -22:
			case -21:
			case -20:
			case -19:
			case -18:
			case -17:
			case -16:
			case -15:
			case -14:
			case -13:
			case -12:
			case -11:
			case -10:
			case -9:
			case -8:
			case -7:
			case -6:
			case -5:
			case -4:
			case -3:
			case -2:
			case -1:
				m_sRotationAngle = (short)(90 - value);
				break;
			}
		}
	}

	public bool IsAutoTextColor
	{
		get
		{
			return m_bAutoTextColor;
		}
		set
		{
			m_bAutoTextColor = value;
		}
	}

	public bool IsAutoTextBack
	{
		get
		{
			return m_bAutoTextBack;
		}
		set
		{
			m_bAutoTextBack = value;
		}
	}

	public bool IsAutoRotation
	{
		get
		{
			return m_bAutoRotation;
		}
		set
		{
			m_bAutoRotation = value;
		}
	}

	public bool IsLeftToRight
	{
		get
		{
			return m_bIsLeftToRight;
		}
		set
		{
			m_bIsLeftToRight = value;
		}
	}

	public bool IsRightToLeft
	{
		get
		{
			return m_bIsRightToLeft;
		}
		set
		{
			m_bIsRightToLeft = value;
		}
	}

	public TRotation Rotation
	{
		get
		{
			return (TRotation)(BiffRecordRaw.GetUInt16BitsByMask(m_usFlags, 28) >> 2);
		}
		set
		{
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usFlags, 28, (ushort)((ushort)value << 2));
		}
	}

	public override int MinimumRecordSize => 30;

	public override int MaximumRecordSize => 30;

	public ChartTickRecord()
	{
	}

	public ChartTickRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartTickRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_MajorMark = provider.ReadByte(iOffset);
		iOffset++;
		m_MinorMark = provider.ReadByte(iOffset);
		iOffset++;
		m_labelPos = provider.ReadByte(iOffset);
		iOffset++;
		m_BackgroundMode = provider.ReadByte(iOffset);
		iOffset++;
		m_uiTextColor = provider.ReadUInt32(iOffset);
		iOffset += 4;
		int num = 0;
		while (num < 16)
		{
			provider.WriteByte(iOffset, 0);
			num++;
			iOffset++;
		}
		m_usFlags = provider.ReadUInt16(iOffset);
		m_bAutoTextColor = provider.ReadBit(iOffset, 0);
		m_bAutoTextBack = provider.ReadBit(iOffset, 1);
		m_bAutoRotation = provider.ReadBit(iOffset, 5);
		iOffset++;
		m_bIsLeftToRight = provider.ReadBit(iOffset, 6);
		m_bIsRightToLeft = provider.ReadBit(iOffset, 7);
		iOffset++;
		m_usTickColorIndex = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_sRotationAngle = provider.ReadInt16(iOffset);
		if (m_sRotationAngle != 0)
		{
			m_bAutoRotation = false;
		}
		iOffset += 2;
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteByte(iOffset, m_MajorMark);
		iOffset++;
		provider.WriteByte(iOffset, m_MinorMark);
		iOffset++;
		provider.WriteByte(iOffset, m_labelPos);
		iOffset++;
		provider.WriteByte(iOffset, m_BackgroundMode);
		iOffset++;
		provider.WriteUInt32(iOffset, m_uiTextColor);
		iOffset += 4;
		iOffset += 16;
		provider.WriteUInt16(iOffset, m_usFlags);
		provider.WriteBit(iOffset, m_bAutoTextColor, 0);
		provider.WriteBit(iOffset, m_bAutoTextBack, 1);
		provider.WriteBit(iOffset, m_bAutoRotation, 5);
		iOffset++;
		provider.WriteBit(iOffset, m_bIsLeftToRight, 6);
		provider.WriteBit(iOffset, m_bIsRightToLeft, 7);
		iOffset++;
		provider.WriteUInt16(iOffset, m_usTickColorIndex);
		iOffset += 2;
		provider.WriteInt16(iOffset, m_sRotationAngle);
		iOffset += 2;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 30;
	}
}
