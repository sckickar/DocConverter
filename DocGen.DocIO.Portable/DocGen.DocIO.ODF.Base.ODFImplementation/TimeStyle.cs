namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class TimeStyle : DataStyle
{
	private string m_ampm;

	private TimeBase m_hours;

	private TimeBase m_minutes;

	private Seconds m_seconds;

	internal string AMPM
	{
		get
		{
			return m_ampm;
		}
		set
		{
			m_ampm = value;
		}
	}

	internal TimeBase Hours
	{
		get
		{
			return m_hours;
		}
		set
		{
			m_hours = value;
		}
	}

	internal TimeBase Minutes
	{
		get
		{
			return m_minutes;
		}
		set
		{
			m_minutes = value;
		}
	}

	internal Seconds Seconds
	{
		get
		{
			return m_seconds;
		}
		set
		{
			m_seconds = value;
		}
	}

	internal TimeStyle()
	{
	}
}
