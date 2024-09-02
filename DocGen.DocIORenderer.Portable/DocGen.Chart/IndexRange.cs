namespace DocGen.Chart;

internal struct IndexRange
{
	private int m_from;

	private int m_to;

	public int From
	{
		get
		{
			return m_from;
		}
		set
		{
			m_from = value;
		}
	}

	public int To
	{
		get
		{
			return m_to;
		}
		set
		{
			m_to = value;
		}
	}

	public IndexRange(int from, int to)
	{
		m_from = from;
		m_to = to;
	}
}
