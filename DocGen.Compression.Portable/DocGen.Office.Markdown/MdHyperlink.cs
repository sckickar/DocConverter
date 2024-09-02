namespace DocGen.Office.Markdown;

internal class MdHyperlink : IMdInline
{
	private string m_url;

	private string m_displayText;

	private string m_screenTip;

	private MdTextFormat m_textFormat;

	internal MdTextFormat TextFormat
	{
		get
		{
			return m_textFormat;
		}
		set
		{
			m_textFormat = value;
		}
	}

	internal string Url
	{
		get
		{
			return m_url;
		}
		set
		{
			m_url = value;
		}
	}

	internal string DisplayText
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

	internal string ScreenTip
	{
		get
		{
			return m_screenTip;
		}
		set
		{
			m_screenTip = value;
		}
	}

	public void Close()
	{
	}

	public MdHyperlink()
	{
		m_textFormat = new MdTextFormat();
	}
}
