namespace DocGen.OfficeChart.Parser.Biff_Records;

internal class CFInterpolationCurve
{
	private const ushort DEF_MINIMUM_SIZE = 8;

	private double m_numDomain;

	private CFVO m_cfvo;

	public double NumDomain
	{
		get
		{
			return m_numDomain;
		}
		set
		{
			m_numDomain = value;
		}
	}

	public CFVO CFVO
	{
		get
		{
			return m_cfvo;
		}
		set
		{
			m_cfvo = value;
		}
	}

	public CFInterpolationCurve()
	{
		m_cfvo = new CFVO();
	}

	public int ParseCFGradientInterp(DataProvider provider, int iOffset, OfficeVersion version)
	{
		iOffset = m_cfvo.ParseCFVO(provider, iOffset, version);
		m_numDomain = provider.ReadDouble(iOffset);
		iOffset += 8;
		return iOffset;
	}

	public int SerializeCFGradientInterp(DataProvider provider, int iOffset, OfficeVersion version, double numValue, bool isParsed)
	{
		iOffset = m_cfvo.SerializeCFVO(provider, iOffset, version);
		if (!isParsed)
		{
			provider.WriteDouble(iOffset, numValue);
		}
		else
		{
			provider.WriteDouble(iOffset, m_numDomain);
		}
		iOffset += 8;
		return iOffset;
	}

	public int GetStoreSize(OfficeVersion version)
	{
		return 8 + m_cfvo.GetStoreSize(version);
	}

	internal void ClearAll()
	{
		m_cfvo.ClearAll();
		m_cfvo = null;
	}
}
