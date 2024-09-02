using System;
using System.Text;

namespace DocGen.OfficeChart.Parser.Biff_Records;

internal class DXFN
{
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

	private byte m_uiOptions;

	private byte m_usReserved;

	private bool m_bLeftBorder = true;

	private bool m_bRightBorder = true;

	private bool m_bTopBorder = true;

	private bool m_bBottomBorder = true;

	private bool m_bPatternStyle = true;

	private bool m_bPatternColor = true;

	private bool m_bPatternBackColor = true;

	private bool m_bNumberFormatModified = true;

	private bool m_bNumberFormatPresent;

	private bool m_bFontFormat;

	private bool m_bBorderFormat;

	private bool m_bPatternFormat;

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

	private ushort m_userdefNumFormatSize;

	private ushort m_charCount;

	private bool m_isHighByte;

	private string m_strValue;

	public int ParseDXFN(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_uiOptions = provider.ReadByte(iOffset);
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
		m_usReserved = provider.ReadByte(iOffset);
		iOffset++;
		iOffset = (m_numberFormatIsUserDefined ? ParseUserdefinedNumberFormatBlock(provider, ref iOffset) : ParseNumberFormatBlock(provider, ref iOffset));
		iOffset = ParseFontBlock(provider, ref iOffset);
		iOffset = ParseBorderBlock(provider, ref iOffset);
		iOffset = ParsePatternBlock(provider, ref iOffset);
		return iOffset;
	}

	public int SerializeDXFN(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteByte(iOffset, m_uiOptions);
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
		provider.WriteByte(iOffset, m_usReserved);
		iOffset++;
		iOffset = (m_numberFormatIsUserDefined ? SerializeUserdefinedNumberFormatBlock(provider, ref iOffset) : SerializeNumberFormatBlock(provider, ref iOffset));
		iOffset = SerializeFontBlock(provider, ref iOffset);
		iOffset = SerializeBorderBlock(provider, ref iOffset);
		iOffset = SerializePatternBlock(provider, ref iOffset);
		return iOffset;
	}

	public int ParseFontBlock(DataProvider provider, ref int iOffset)
	{
		if (!m_bFontFormat)
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
		if (!m_bBorderFormat)
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
		if (!m_bPatternFormat)
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
		if (!m_bNumberFormatPresent)
		{
			return iOffset;
		}
		m_unUsed = provider.ReadByte(iOffset);
		iOffset++;
		m_numFormatIndex = provider.ReadByte(iOffset);
		iOffset++;
		return iOffset;
	}

	public int ParseUserdefinedNumberFormatBlock(DataProvider provider, ref int iOffset)
	{
		if (!m_bNumberFormatPresent)
		{
			return iOffset;
		}
		m_userdefNumFormatSize = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_charCount = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_isHighByte = provider.ReadBit(iOffset, 0);
		iOffset++;
		int num = 0;
		num = (m_isHighByte ? (2 * m_charCount) : m_charCount);
		m_strValue = provider.ReadString(iOffset, num, Encoding.UTF8, isUnicode: true);
		iOffset += num;
		return iOffset;
	}

	public int SerializeFontBlock(DataProvider provider, ref int iOffset)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (!m_bFontFormat)
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
		if (!m_bBorderFormat)
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
		if (!m_bPatternFormat)
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
		if (!m_bNumberFormatPresent)
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

	public int SerializeUserdefinedNumberFormatBlock(DataProvider provider, ref int iOffset)
	{
		if (!m_bNumberFormatPresent)
		{
			return iOffset;
		}
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		provider.WriteUInt16(iOffset, m_userdefNumFormatSize);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_charCount);
		iOffset += 2;
		if (!m_isHighByte)
		{
			provider.WriteByte(iOffset, 0);
		}
		else
		{
			provider.WriteByte(iOffset, 1);
		}
		iOffset++;
		byte[] bytes = Encoding.UTF8.GetBytes(m_strValue);
		int num = bytes.Length;
		provider.WriteBytes(iOffset, bytes, 0, num);
		iOffset += num;
		return iOffset;
	}

	public int GetStoreSize(OfficeVersion version)
	{
		int num = 6;
		if (m_bFontFormat)
		{
			num += 118;
		}
		if (m_bBorderFormat)
		{
			num += 8;
		}
		if (m_bPatternFormat)
		{
			num += 4;
		}
		if (m_bNumberFormatPresent)
		{
			num += 2;
		}
		return num;
	}

	public new int GetHashCode()
	{
		return m_uiOptions.GetHashCode() ^ m_usReserved.GetHashCode() ^ m_bLeftBorder.GetHashCode() ^ m_bRightBorder.GetHashCode() ^ m_bTopBorder.GetHashCode() ^ m_bBottomBorder.GetHashCode() ^ m_bPatternStyle.GetHashCode() ^ m_bPatternColor.GetHashCode() ^ m_bPatternBackColor.GetHashCode() ^ m_bNumberFormatModified.GetHashCode() ^ m_bNumberFormatPresent.GetHashCode() ^ m_bFontFormat.GetHashCode() ^ m_bBorderFormat.GetHashCode() ^ m_bPatternFormat.GetHashCode() ^ m_uiFontHeight.GetHashCode() ^ m_uiFontOptions.GetHashCode() ^ m_usFontWeight.GetHashCode() ^ m_usEscapmentType.GetHashCode() ^ m_Underline.GetHashCode() ^ m_uiFontColorIndex.GetHashCode() ^ m_uiModifiedFlags.GetHashCode() ^ m_uiEscapmentModified.GetHashCode() ^ m_uiUnderlineModified.GetHashCode() ^ m_usBorderLineStyles.GetHashCode() ^ m_uiBorderColors.GetHashCode() ^ m_usPatternStyle.GetHashCode() ^ m_usPatternColors.GetHashCode();
	}
}
