namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class DateBase : TimeBase
{
	private string m_calender;

	internal string Calender
	{
		get
		{
			return m_calender;
		}
		set
		{
			m_calender = value;
		}
	}
}
