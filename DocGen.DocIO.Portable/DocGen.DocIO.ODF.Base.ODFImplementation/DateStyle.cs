namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class DateStyle : TimeStyle
{
	private bool m_automaticOrder;

	private DateBase m_day;

	private DateBase m_dayOfWeek;

	private DateBase m_era;

	private Month m_month;

	private DateBase m_quarter;

	private DateBase m_weekOfYear;

	private DateBase m_year;

	internal bool AutomaticOrder
	{
		get
		{
			return m_automaticOrder;
		}
		set
		{
			m_automaticOrder = value;
		}
	}

	internal DateBase Day
	{
		get
		{
			return m_day;
		}
		set
		{
			m_day = value;
		}
	}

	internal DateBase DayOfWeek
	{
		get
		{
			return m_dayOfWeek;
		}
		set
		{
			m_dayOfWeek = value;
		}
	}

	internal DateBase Era
	{
		get
		{
			return m_era;
		}
		set
		{
			m_era = value;
		}
	}

	internal Month Month
	{
		get
		{
			return m_month;
		}
		set
		{
			m_month = value;
		}
	}

	internal DateBase Quarter
	{
		get
		{
			return m_quarter;
		}
		set
		{
			m_quarter = value;
		}
	}

	internal DateBase WeekOfYear
	{
		get
		{
			return m_weekOfYear;
		}
		set
		{
			m_weekOfYear = value;
		}
	}

	internal DateBase Year
	{
		get
		{
			return m_year;
		}
		set
		{
			m_year = value;
		}
	}

	internal void Dispose()
	{
		if (m_day != null)
		{
			m_day = null;
		}
		if (m_dayOfWeek != null)
		{
			m_dayOfWeek = null;
		}
		if (m_era != null)
		{
			m_era = null;
		}
		if (m_month != null)
		{
			m_month = null;
		}
		if (m_quarter != null)
		{
			m_quarter = null;
		}
		if (m_weekOfYear != null)
		{
			m_weekOfYear = null;
		}
		if (m_year != null)
		{
			m_year = null;
		}
		Dispose1();
	}
}
