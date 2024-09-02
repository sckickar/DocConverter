namespace DocGen.OfficeChart.Parser.Biff_Records;

internal class GradStops
{
	private ushort m_colorType;

	private int m_gradColorValue;

	private long m_gradPostition;

	private long m_gradTint;

	public int ParseGradStops(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_colorType = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_gradColorValue = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_gradPostition = provider.ReadInt64(iOffset);
		iOffset += 8;
		m_gradTint = provider.ReadInt64(iOffset);
		iOffset += 8;
		return iOffset;
	}

	public int InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_colorType);
		iOffset += 2;
		provider.WriteInt32(iOffset, m_gradColorValue);
		iOffset += 4;
		provider.WriteInt64(iOffset, m_gradPostition);
		iOffset += 8;
		provider.WriteInt64(iOffset, m_gradTint);
		iOffset += 8;
		return iOffset;
	}
}
