namespace DocGen.OfficeChart.Parser.Biff_Records;

internal class CFExTextTemplateParameter
{
	private ushort m_textRuleType;

	public CFTextRuleType TextRuleType
	{
		get
		{
			return (CFTextRuleType)m_textRuleType;
		}
		set
		{
			m_textRuleType = (byte)value;
		}
	}

	public void ParseTextTemplateParameter(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_textRuleType = provider.ReadUInt16(iOffset);
		iOffset += 2;
		provider.ReadInt64(iOffset);
		iOffset += 14;
	}

	public void SerializeTextTemplateParameter(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_textRuleType);
		iOffset += 2;
		provider.WriteInt64(iOffset, 0L);
		iOffset += 14;
	}
}
