namespace DocGen.OfficeChart.Parser.Biff_Records;

internal class CFExFilterParameter
{
	private bool m_isTopOrBottom;

	private bool m_isPercent;

	private ushort m_filterValue;

	public bool IsTopOrBottom
	{
		get
		{
			return m_isTopOrBottom;
		}
		set
		{
			m_isTopOrBottom = value;
		}
	}

	public bool IsPercent
	{
		get
		{
			return m_isPercent;
		}
		set
		{
			m_isPercent = value;
		}
	}

	public ushort FilterValue
	{
		get
		{
			return m_filterValue;
		}
		set
		{
			m_filterValue = value;
		}
	}

	public void ParseFilterTemplateParameter(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_isTopOrBottom = provider.ReadBit(iOffset, 0);
		m_isPercent = provider.ReadBit(iOffset, 1);
		iOffset++;
		m_filterValue = provider.ReadUInt16(iOffset);
		iOffset += 2;
		provider.ReadInt64(iOffset);
		iOffset += 13;
	}

	public void SerializeFilterParameter(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteBit(iOffset, m_isTopOrBottom, 0);
		provider.WriteBit(iOffset, m_isPercent, 1);
		iOffset++;
		provider.WriteUInt16(iOffset, m_filterValue);
		iOffset += 2;
		provider.WriteInt64(iOffset, 0L);
		iOffset += 13;
	}
}
