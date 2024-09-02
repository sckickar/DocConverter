namespace DocGen.Chart;

internal class ChartPointWithIndex
{
	private ChartPoint m_point;

	private int m_index;

	public ChartPoint Point
	{
		get
		{
			return m_point;
		}
		set
		{
			if (m_point != value)
			{
				m_point = value;
			}
		}
	}

	public int Index
	{
		get
		{
			return m_index;
		}
		set
		{
			if (m_index != value)
			{
				m_index = value;
			}
		}
	}

	public ChartPointWithIndex(ChartPoint point, int index)
	{
		m_point = point;
		m_index = index;
	}
}
