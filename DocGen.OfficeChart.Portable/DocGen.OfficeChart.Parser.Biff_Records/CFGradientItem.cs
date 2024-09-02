namespace DocGen.OfficeChart.Parser.Biff_Records;

internal class CFGradientItem
{
	private double m_numGradientRange;

	private uint m_colorType = 2u;

	private uint m_colorValue;

	private long m_tintShade;

	public double NumGradientRange
	{
		get
		{
			return m_numGradientRange;
		}
		set
		{
			m_numGradientRange = value;
		}
	}

	public ColorType ColorType
	{
		get
		{
			return (ColorType)m_colorType;
		}
		set
		{
			m_colorType = 2u;
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

	public int ParseCFGradient(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_numGradientRange = provider.ReadDouble(iOffset);
		iOffset += 8;
		m_colorType = provider.ReadUInt32(iOffset);
		iOffset += 4;
		m_colorValue = provider.ReadUInt32(iOffset);
		iOffset += 4;
		m_tintShade = provider.ReadInt64(iOffset);
		iOffset += 8;
		return iOffset;
	}

	public int SerializeCFGradient(DataProvider provider, int iOffset, OfficeVersion version, double numValue, bool isParsed)
	{
		if (!isParsed)
		{
			provider.WriteDouble(iOffset, numValue);
		}
		else
		{
			provider.WriteDouble(iOffset, m_numGradientRange);
		}
		iOffset += 8;
		provider.WriteUInt32(iOffset, m_colorType);
		iOffset += 4;
		provider.WriteUInt32(iOffset, m_colorValue);
		iOffset += 4;
		provider.WriteInt64(iOffset, m_tintShade);
		iOffset += 8;
		return iOffset;
	}

	public int GetStoreSize(OfficeVersion version)
	{
		return 24;
	}
}
