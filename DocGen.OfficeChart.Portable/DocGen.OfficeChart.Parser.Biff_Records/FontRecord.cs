using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.Font)]
[CLSCompliant(false)]
internal class FontRecord : BiffRecordRaw
{
	[Flags]
	private enum FontAttributes : ushort
	{
		Italic = 2,
		Strikeout = 8,
		MacOutline = 0x10,
		MacShadow = 0x20,
		AllKnown = 0x3A
	}

	private const int DEF_INCORRECT_HASH = -1;

	private const int DEF_STRING_TYPE_OFFSET = 15;

	public const int DefaultFontColor = 32767;

	[BiffRecordPos(0, 2)]
	private ushort m_usFontHeight = 200;

	[BiffRecordPos(2, 2)]
	private FontAttributes m_attributes;

	[BiffRecordPos(4, 2)]
	private ushort m_usPaletteColorIndex = 32767;

	[BiffRecordPos(6, 2)]
	private ushort m_usBoldWeight = 400;

	[BiffRecordPos(8, 2)]
	private ushort m_SuperSubscript;

	[BiffRecordPos(10, 1)]
	private byte m_Underline;

	[BiffRecordPos(11, 1)]
	private byte m_Family;

	[BiffRecordPos(12, 1)]
	private byte m_Charset;

	[BiffRecordPos(13, 1)]
	private byte m_Reserved;

	[BiffRecordPos(14, TFieldType.String)]
	private string m_strFontName = "Arial";

	private int m_iHashCode = -1;

	private int m_baseLine;

	private bool m_isCapitalize;

	private double m_characterSpacingValue;

	private double m_kerningValue;

	public ushort Attributes => (ushort)m_attributes;

	public ushort FontHeight
	{
		get
		{
			return m_usFontHeight;
		}
		set
		{
			if (m_usFontHeight != value)
			{
				m_usFontHeight = value;
				m_iHashCode = -1;
			}
		}
	}

	public ushort PaletteColorIndex
	{
		get
		{
			return m_usPaletteColorIndex;
		}
		set
		{
			if (m_usPaletteColorIndex != value)
			{
				m_usPaletteColorIndex = value;
				m_iHashCode = -1;
			}
		}
	}

	public ushort BoldWeight
	{
		get
		{
			return m_usBoldWeight;
		}
		set
		{
			if (m_usBoldWeight != value)
			{
				m_usBoldWeight = value;
				m_iHashCode = -1;
			}
		}
	}

	public OfficeFontVerticalAlignment SuperSubscript
	{
		get
		{
			return (OfficeFontVerticalAlignment)m_SuperSubscript;
		}
		set
		{
			if (m_SuperSubscript != (ushort)value)
			{
				m_SuperSubscript = (ushort)value;
				m_iHashCode = -1;
			}
		}
	}

	public int Baseline
	{
		get
		{
			return m_baseLine;
		}
		set
		{
			m_baseLine = value;
		}
	}

	public OfficeUnderline Underline
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
				m_iHashCode = -1;
			}
		}
	}

	public byte Family
	{
		get
		{
			return m_Family;
		}
		set
		{
			if (m_Family != value)
			{
				m_Family = value;
				m_iHashCode = -1;
			}
		}
	}

	public byte Charset
	{
		get
		{
			return m_Charset;
		}
		set
		{
			if (m_Charset != value)
			{
				m_Charset = value;
				m_iHashCode = -1;
			}
		}
	}

	public string FontName
	{
		get
		{
			return m_strFontName;
		}
		set
		{
			if (m_strFontName != value)
			{
				m_strFontName = value;
				m_iHashCode = -1;
			}
		}
	}

	public bool IsItalic
	{
		get
		{
			return (m_attributes & FontAttributes.Italic) != 0;
		}
		set
		{
			if (value)
			{
				m_attributes |= FontAttributes.Italic;
			}
			else
			{
				m_attributes &= ~FontAttributes.Italic;
			}
			m_iHashCode = -1;
		}
	}

	public bool IsStrikeout
	{
		get
		{
			return (m_attributes & FontAttributes.Strikeout) != 0;
		}
		set
		{
			if (value)
			{
				m_attributes |= FontAttributes.Strikeout;
			}
			else
			{
				m_attributes &= ~FontAttributes.Strikeout;
			}
			m_iHashCode = -1;
		}
	}

	public bool IsMacOutline
	{
		get
		{
			return (m_attributes & FontAttributes.MacOutline) != 0;
		}
		set
		{
			if (value)
			{
				m_attributes |= FontAttributes.MacOutline;
			}
			else
			{
				m_attributes &= ~FontAttributes.MacOutline;
			}
			m_iHashCode = -1;
		}
	}

	public bool IsMacShadow
	{
		get
		{
			return (m_attributes & FontAttributes.MacShadow) != 0;
		}
		set
		{
			if (value)
			{
				m_attributes |= FontAttributes.MacShadow;
			}
			else
			{
				m_attributes &= ~FontAttributes.MacShadow;
			}
			m_iHashCode = -1;
		}
	}

	internal bool HasCapOrCharacterSpaceOrKerning { get; set; }

	internal bool IsCapitalize
	{
		get
		{
			return m_isCapitalize;
		}
		set
		{
			if (m_isCapitalize != value)
			{
				m_isCapitalize = value;
				m_iHashCode = -1;
			}
		}
	}

	internal double CharacterSpacingValue
	{
		get
		{
			return m_characterSpacingValue;
		}
		set
		{
			if (m_characterSpacingValue != value)
			{
				m_characterSpacingValue = value;
				m_iHashCode = -1;
			}
		}
	}

	internal double KerningValue
	{
		get
		{
			return m_kerningValue;
		}
		set
		{
			if (m_kerningValue != value)
			{
				m_kerningValue = value;
				m_iHashCode = -1;
			}
		}
	}

	public byte Reserved => m_Reserved;

	public override int MinimumRecordSize => 16;

	public FontRecord()
	{
	}

	public FontRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public FontRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usFontHeight = provider.ReadUInt16(iOffset);
		m_attributes = (FontAttributes)provider.ReadUInt16(iOffset + 2);
		m_usPaletteColorIndex = provider.ReadUInt16(iOffset + 4);
		m_usBoldWeight = provider.ReadUInt16(iOffset + 6);
		m_SuperSubscript = provider.ReadUInt16(iOffset + 8);
		m_Underline = provider.ReadByte(iOffset + 10);
		m_Family = provider.ReadByte(iOffset + 11);
		m_Charset = provider.ReadByte(iOffset + 12);
		m_Reserved = provider.ReadByte(iOffset + 13);
		m_strFontName = provider.ReadString8Bit(iOffset + 14, out iLength);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteUInt16(iOffset, m_usFontHeight);
		iOffset += 2;
		provider.WriteUInt16(iOffset, (ushort)m_attributes);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usPaletteColorIndex);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usBoldWeight);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_SuperSubscript);
		iOffset += 2;
		provider.WriteByte(iOffset, m_Underline);
		iOffset++;
		provider.WriteByte(iOffset, m_Family);
		iOffset++;
		provider.WriteByte(iOffset, m_Charset);
		iOffset++;
		provider.WriteByte(iOffset, m_Reserved);
		iOffset++;
		int length = m_strFontName.Length;
		provider.WriteByte(iOffset, (byte)length);
		iOffset++;
		if (length > 0)
		{
			provider.WriteStringNoLenUpdateOffset(ref iOffset, m_strFontName);
		}
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 16 + m_strFontName.Length * 2;
	}

	public override bool Equals(object obj)
	{
		FontRecord fontRecord = obj as FontRecord;
		if (fontRecord.m_strFontName == m_strFontName && fontRecord.m_usFontHeight == m_usFontHeight && fontRecord.m_usPaletteColorIndex == m_usPaletteColorIndex && fontRecord.m_usBoldWeight == m_usBoldWeight && fontRecord.m_Underline == m_Underline && fontRecord.m_SuperSubscript == m_SuperSubscript && fontRecord.m_Family == m_Family && fontRecord.m_Charset == m_Charset)
		{
			return (fontRecord.m_attributes & FontAttributes.AllKnown) == (m_attributes & FontAttributes.AllKnown);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (m_iHashCode == -1)
		{
			m_iHashCode = m_usFontHeight.GetHashCode() + (m_attributes & FontAttributes.AllKnown).GetHashCode() + m_usPaletteColorIndex.GetHashCode() + m_usBoldWeight.GetHashCode() + m_SuperSubscript.GetHashCode() + m_Underline.GetHashCode() + m_Family.GetHashCode() + m_Charset.GetHashCode() + m_strFontName.GetHashCode();
		}
		return m_iHashCode;
	}

	public override void CopyTo(BiffRecordRaw raw)
	{
		FontRecord obj = (raw as FontRecord) ?? throw new ArgumentNullException("twin");
		obj.m_usFontHeight = m_usFontHeight;
		obj.m_attributes = m_attributes;
		obj.m_usPaletteColorIndex = m_usPaletteColorIndex;
		obj.m_usBoldWeight = m_usBoldWeight;
		obj.m_SuperSubscript = m_SuperSubscript;
		obj.m_Underline = m_Underline;
		obj.m_Family = m_Family;
		obj.m_Charset = m_Charset;
		obj.m_strFontName = m_strFontName;
		obj.m_iHashCode = m_iHashCode;
	}

	public int CompareTo(FontRecord record)
	{
		if (record == null)
		{
			return 1;
		}
		int num = GetHashCode() - record.GetHashCode();
		if (num != 0)
		{
			return num;
		}
		num = m_usFontHeight - record.m_usFontHeight;
		if (num != 0)
		{
			return num;
		}
		num = m_strFontName.CompareTo(record.m_strFontName);
		if (num != 0)
		{
			return num;
		}
		num = (m_attributes & FontAttributes.AllKnown).CompareTo(record.m_attributes & FontAttributes.AllKnown);
		if (num != 0)
		{
			return num;
		}
		num = m_usPaletteColorIndex - record.m_usPaletteColorIndex;
		if (num != 0)
		{
			return num;
		}
		num = m_usBoldWeight - record.m_usBoldWeight;
		if (num != 0)
		{
			return num;
		}
		num = m_SuperSubscript - record.m_SuperSubscript;
		if (num != 0)
		{
			return num;
		}
		num = m_Underline - record.m_Underline;
		if (num != 0)
		{
			return num;
		}
		num = m_Family - record.m_Family;
		if (num != 0)
		{
			return num;
		}
		num = m_Charset - record.m_Charset;
		if (num != 0)
		{
			return num;
		}
		return 0;
	}
}
