namespace DocGen.Office.Markdown;

internal class MdListFormat
{
	private bool m_isNumbered;

	private string m_numberedListMarker = "1.";

	private const char m_bulletedListMarker = '-';

	private string m_listValue = string.Empty;

	private int m_listLevel;

	internal bool IsNumbered
	{
		get
		{
			return m_isNumbered;
		}
		set
		{
			m_isNumbered = value;
		}
	}

	internal string BulletedListMarker => '-' + " ";

	internal string NumberedListMarker
	{
		get
		{
			return m_numberedListMarker + " ";
		}
		set
		{
			m_numberedListMarker = value;
		}
	}

	internal string ListValue
	{
		get
		{
			return m_listValue;
		}
		set
		{
			m_listValue = value;
		}
	}

	internal int ListLevel
	{
		get
		{
			return m_listLevel;
		}
		set
		{
			m_listLevel = value;
		}
	}
}
