namespace DocGen.DocIO.DLS;

public class ContentControlListItem
{
	private string m_displayText;

	private string m_value;

	public string DisplayText
	{
		get
		{
			return m_displayText;
		}
		set
		{
			m_displayText = value;
		}
	}

	public string Value
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
