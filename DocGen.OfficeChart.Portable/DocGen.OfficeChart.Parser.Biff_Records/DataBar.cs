using DocGen.Drawing;

namespace DocGen.OfficeChart.Parser.Biff_Records;

internal class DataBar
{
	private const ushort DEF_MINIMUM_SIZE = 22;

	private ushort m_undefined;

	private bool m_isRightToLeft;

	private bool m_isShowValue = true;

	private byte m_minDatabarLen;

	private byte m_MaxDatabarLen;

	private uint m_colorType;

	private uint m_colorValue;

	private long m_tintShade;

	private CFVO m_cfvoMin;

	private CFVO m_cfvoMax;

	public ColorType ColorType
	{
		get
		{
			return (ColorType)m_colorType;
		}
		set
		{
			m_colorType = (byte)value;
		}
	}

	public uint ColorValue
	{
		get
		{
			return m_colorValue;
		}
		set
		{
			m_colorValue = value;
		}
	}

	public long TintShade
	{
		get
		{
			return m_tintShade;
		}
		set
		{
			m_tintShade = value;
		}
	}

	public CFVO MinCFVO
	{
		get
		{
			return m_cfvoMin;
		}
		set
		{
			m_cfvoMin = value;
		}
	}

	public CFVO MaxCFVO
	{
		get
		{
			return m_cfvoMax;
		}
		set
		{
			m_cfvoMax = value;
		}
	}

	public DataBar()
	{
		m_cfvoMin = new CFVO();
		m_cfvoMax = new CFVO();
	}

	private void CopyDataBar()
	{
	}

	private Color ConvertRGBAToARGB(Color colorValue)
	{
		byte a = colorValue.A;
		byte b = colorValue.B;
		byte g = colorValue.G;
		byte r = colorValue.R;
		colorValue = Color.FromArgb(a, b, g, r);
		return colorValue;
	}

	public int ParseDataBar(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_undefined = provider.ReadUInt16(iOffset);
		iOffset += 2;
		provider.ReadByte(iOffset);
		iOffset++;
		m_isRightToLeft = provider.ReadBit(iOffset, 0);
		m_isShowValue = provider.ReadBit(iOffset, 1);
		iOffset++;
		m_minDatabarLen = provider.ReadByte(iOffset);
		iOffset++;
		m_MaxDatabarLen = provider.ReadByte(iOffset);
		iOffset++;
		m_colorType = provider.ReadUInt32(iOffset);
		iOffset += 4;
		m_colorValue = provider.ReadUInt32(iOffset);
		iOffset += 4;
		m_tintShade = provider.ReadInt64(iOffset);
		iOffset += 8;
		m_cfvoMin = new CFVO();
		iOffset = m_cfvoMin.ParseCFVO(provider, iOffset, version);
		m_cfvoMax = new CFVO();
		iOffset = m_cfvoMax.ParseCFVO(provider, iOffset, version);
		CopyDataBar();
		return iOffset;
	}

	public int GetStoreSize(OfficeVersion version)
	{
		return 22 + m_cfvoMin.GetStoreSize(version) + m_cfvoMax.GetStoreSize(version);
	}

	private uint ColorToUInt(Color color)
	{
		return (uint)((color.A << 24) | (color.R << 16) | (color.G << 8) | color.B);
	}

	private Color UIntToColor(uint color)
	{
		byte alpha = (byte)(color >> 24);
		byte red = (byte)(color >> 16);
		byte green = (byte)(color >> 8);
		byte blue = (byte)color;
		return Color.FromArgb(alpha, red, green, blue);
	}

	private Color ConvertARGBToRGBA(Color colorValue)
	{
		byte b = colorValue.B;
		byte g = colorValue.G;
		byte r = colorValue.R;
		colorValue = Color.FromArgb(colorValue.A, b, g, r);
		return colorValue;
	}

	internal void ClearAll()
	{
		m_cfvoMax.ClearAll();
		m_cfvoMin.ClearAll();
		m_cfvoMin = null;
		m_cfvoMax = null;
	}
}
