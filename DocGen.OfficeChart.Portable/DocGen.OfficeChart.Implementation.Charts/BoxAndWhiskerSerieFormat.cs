namespace DocGen.OfficeChart.Implementation.Charts;

internal struct BoxAndWhiskerSerieFormat
{
	private byte m_options;

	internal bool ShowMeanLine
	{
		get
		{
			return (m_options & 1) == 1;
		}
		set
		{
			if (value)
			{
				m_options |= 1;
			}
			else
			{
				m_options &= 254;
			}
		}
	}

	internal bool ShowMeanMarkers
	{
		get
		{
			return (m_options & 2) == 2;
		}
		set
		{
			if (value)
			{
				m_options |= 2;
			}
			else
			{
				m_options &= 253;
			}
		}
	}

	internal bool ShowInnerPoints
	{
		get
		{
			return (m_options & 4) == 4;
		}
		set
		{
			if (value)
			{
				m_options |= 4;
			}
			else
			{
				m_options &= 251;
			}
		}
	}

	internal bool ShowOutlierPoints
	{
		get
		{
			return (m_options & 8) == 8;
		}
		set
		{
			if (value)
			{
				m_options |= 8;
			}
			else
			{
				m_options &= 247;
			}
		}
	}

	internal QuartileCalculation QuartileCalculationType
	{
		get
		{
			if ((m_options & 0x10) != 16)
			{
				return QuartileCalculation.ExclusiveMedian;
			}
			return QuartileCalculation.InclusiveMedian;
		}
		set
		{
			if (value == QuartileCalculation.InclusiveMedian)
			{
				m_options |= 16;
			}
			else
			{
				m_options &= 239;
			}
		}
	}

	internal byte Options
	{
		get
		{
			return m_options;
		}
		set
		{
			m_options = value;
		}
	}
}
