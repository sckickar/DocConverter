namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class NumberFormat : LanguageStyle
{
	private string m_title;

	private string m_transliterationCountry;

	private string m_transliterationFormat;

	private string m_transliterationLanguage;

	private string m_transliterationStyle;

	internal string Title
	{
		get
		{
			return m_title;
		}
		set
		{
			m_title = value;
		}
	}

	internal string TransliterationCountry
	{
		get
		{
			return m_transliterationCountry;
		}
		set
		{
			m_transliterationCountry = value;
		}
	}

	internal string TransliterationFormat
	{
		get
		{
			return m_transliterationFormat;
		}
		set
		{
			m_transliterationFormat = value;
		}
	}

	internal string TransliterationLanguage
	{
		get
		{
			return m_transliterationLanguage;
		}
		set
		{
			m_transliterationLanguage = value;
		}
	}

	internal string TransliterationStyle
	{
		get
		{
			return m_transliterationStyle;
		}
		set
		{
			m_transliterationStyle = value;
		}
	}
}
