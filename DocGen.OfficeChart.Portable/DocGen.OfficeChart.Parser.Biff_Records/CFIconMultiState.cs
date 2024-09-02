namespace DocGen.OfficeChart.Parser.Biff_Records;

internal class CFIconMultiState
{
	private const ushort DEF_MINIMUM_SIZE = 5;

	private CFVO m_cfvo;

	private byte m_isEqual;

	private uint m_undefined;

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

	public byte IsEqulal
	{
		get
		{
			return m_isEqual;
		}
		set
		{
			m_isEqual = value;
		}
	}

	public CFIconMultiState()
	{
		m_cfvo = new CFVO();
	}

	public int ParseCFIconMultistate(DataProvider provider, int iOffset, OfficeVersion version)
	{
		iOffset = m_cfvo.ParseCFVO(provider, iOffset, version);
		m_isEqual = provider.ReadByte(iOffset);
		iOffset++;
		m_undefined = provider.ReadUInt32(iOffset);
		iOffset += 4;
		return iOffset;
	}

	public int SerializeCFIconMultistate(DataProvider provider, int iOffset, OfficeVersion version)
	{
		iOffset = m_cfvo.SerializeCFVO(provider, iOffset, version);
		provider.WriteByte(iOffset, m_isEqual);
		iOffset++;
		provider.WriteUInt32(iOffset, m_undefined);
		iOffset += 4;
		return iOffset;
	}

	public int GetStoreSize(OfficeVersion version)
	{
		return 5 + m_cfvo.GetStoreSize(version);
	}

	internal void ClearAll()
	{
		m_cfvo.ClearAll();
		m_cfvo = null;
	}
}
