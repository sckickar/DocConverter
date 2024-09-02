namespace DocGen.Chart;

internal class ChartAxisSegment
{
	private DoubleRange m_range;

	private double m_length;

	private double m_interval;

	public DoubleRange Range
	{
		get
		{
			return m_range;
		}
		set
		{
			m_range = value;
		}
	}

	public double Interval
	{
		get
		{
			return m_interval;
		}
		set
		{
			m_interval = value;
		}
	}

	public double Length
	{
		get
		{
			return m_length;
		}
		set
		{
			m_length = value;
		}
	}
}
