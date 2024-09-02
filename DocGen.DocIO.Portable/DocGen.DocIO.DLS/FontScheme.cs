namespace DocGen.DocIO.DLS;

internal class FontScheme
{
	private string m_fontSchemeName;

	private MajorMinorFontScheme m_majorFontScheme;

	private MajorMinorFontScheme m_minorFontScheme;

	internal string FontSchemeName
	{
		get
		{
			return m_fontSchemeName;
		}
		set
		{
			m_fontSchemeName = value;
		}
	}

	internal MajorMinorFontScheme MajorFontScheme
	{
		get
		{
			return m_majorFontScheme;
		}
		set
		{
			m_majorFontScheme = new MajorMinorFontScheme();
		}
	}

	internal MajorMinorFontScheme MinorFontScheme
	{
		get
		{
			return m_minorFontScheme;
		}
		set
		{
			m_minorFontScheme = new MajorMinorFontScheme();
		}
	}

	public FontScheme()
	{
		m_majorFontScheme = new MajorMinorFontScheme();
		m_minorFontScheme = new MajorMinorFontScheme();
	}

	internal void Close()
	{
		if (m_majorFontScheme != null)
		{
			m_majorFontScheme.Close();
			m_majorFontScheme = null;
		}
		if (m_minorFontScheme != null)
		{
			m_minorFontScheme.Close();
			m_minorFontScheme = null;
		}
	}
}
