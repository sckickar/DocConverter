namespace DocGen.DocIO.DLS;

internal class XAttribute
{
	private string m_name;

	private string m_value;

	internal string LocalName
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
		}
	}

	internal string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
		}
	}

	internal string Value
	{
		get
		{
			return m_value;
		}
		set
		{
			m_value = value;
		}
	}
}
