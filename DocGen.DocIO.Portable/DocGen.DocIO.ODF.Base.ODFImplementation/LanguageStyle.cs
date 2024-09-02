namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class LanguageStyle
{
	private string m_country;

	private string m_language;

	private string m_rFCLanguageTag;

	private string m_script;

	internal string Country
	{
		get
		{
			return m_country;
		}
		set
		{
			m_country = value;
		}
	}

	internal string Language
	{
		get
		{
			return m_language;
		}
		set
		{
			m_language = value;
		}
	}

	internal string RFCLanguageTag
	{
		get
		{
			return m_rFCLanguageTag;
		}
		set
		{
			m_rFCLanguageTag = value;
		}
	}

	internal string Script
	{
		get
		{
			return m_script;
		}
		set
		{
			m_script = value;
		}
	}
}
