namespace DocGen.OfficeChart.Parser.Biff_Records.ObjRecords;

internal class LbsDropData
{
	private short m_sOptions;

	private short m_sLinesNumber;

	private short m_sMinimum;

	private string m_strValue;

	public short Options
	{
		get
		{
			return m_sOptions;
		}
		set
		{
			m_sOptions = value;
		}
	}

	public short LinesNumber
	{
		get
		{
			return m_sLinesNumber;
		}
		set
		{
			m_sLinesNumber = value;
		}
	}

	public short Minimum
	{
		get
		{
			return m_sMinimum;
		}
		set
		{
			m_sMinimum = value;
		}
	}

	public string Value
	{
		get
		{
			return m_strValue;
		}
		set
		{
			m_strValue = value;
		}
	}

	public void Serialize(DataProvider provider, int offset)
	{
		int num = offset;
		provider.WriteInt16(offset, m_sOptions);
		offset += 2;
		provider.WriteInt16(offset, m_sLinesNumber);
		offset += 2;
		provider.WriteInt16(offset, m_sMinimum);
		offset += 2;
		provider.WriteString16BitUpdateOffset(ref offset, m_strValue);
		if ((offset - num) % 2 != 0)
		{
			provider.WriteByte(offset, 10);
		}
	}

	public int Parse(DataProvider provider, int offset)
	{
		int num = offset;
		m_sOptions = provider.ReadInt16(offset);
		offset += 2;
		m_sLinesNumber = provider.ReadInt16(offset);
		offset += 2;
		if (provider.Capacity > offset + 2)
		{
			m_sMinimum = provider.ReadInt16(offset);
			offset += 2;
			if (provider.Capacity > offset + 2)
			{
				m_strValue = provider.ReadString16BitUpdateOffset(ref offset);
			}
		}
		if ((offset - num) % 2 != 0)
		{
			offset++;
		}
		return offset;
	}

	public int GetStoreSize()
	{
		int num = 0;
		if (m_strValue != null)
		{
			num += m_strValue.Length * 2;
			num += 3;
		}
		num += 6;
		if (num % 2 != 0)
		{
			num++;
		}
		return num;
	}

	public LbsDropData Clone()
	{
		return (LbsDropData)MemberwiseClone();
	}
}
