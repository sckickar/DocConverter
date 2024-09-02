namespace DocGen.OfficeChart.Parser.Biff_Records;

internal class CFExAverageTemplateParameter
{
	private ushort m_numberOfStandardDeviation;

	public ushort NumberOfDeviations
	{
		get
		{
			return m_numberOfStandardDeviation;
		}
		set
		{
			m_numberOfStandardDeviation = value;
		}
	}

	public void ParseAverageTemplateParameter(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_numberOfStandardDeviation = provider.ReadUInt16(iOffset);
		iOffset += 2;
		provider.ReadInt64(iOffset);
		iOffset += 14;
	}

	public void SerializeAverageTemplateParameter(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_numberOfStandardDeviation);
		iOffset += 2;
		provider.WriteInt64(iOffset, 0L);
		iOffset += 14;
	}
}
