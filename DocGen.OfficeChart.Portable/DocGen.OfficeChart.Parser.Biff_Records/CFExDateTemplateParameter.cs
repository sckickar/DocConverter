namespace DocGen.OfficeChart.Parser.Biff_Records;

internal class CFExDateTemplateParameter
{
	private ushort m_dateComparisonType;

	public ushort DateComparisonOperator
	{
		get
		{
			return m_dateComparisonType;
		}
		set
		{
			m_dateComparisonType = value;
		}
	}

	public void ParseDateTemplateParameter(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_dateComparisonType = provider.ReadUInt16(iOffset);
		iOffset += 2;
		provider.ReadInt64(iOffset);
		iOffset += 14;
	}

	public void SerializeDateTemplateParameter(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_dateComparisonType);
		iOffset += 2;
		provider.WriteInt64(iOffset, 0L);
		iOffset += 14;
	}
}
