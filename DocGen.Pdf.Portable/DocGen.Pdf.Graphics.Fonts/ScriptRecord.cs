namespace DocGen.Pdf.Graphics.Fonts;

internal struct ScriptRecord
{
	private string m_scriptTag;

	private LanguageRecord m_language;

	private LanguageRecord[] m_LanguageRecord;

	internal string ScriptTag
	{
		get
		{
			return m_scriptTag;
		}
		set
		{
			m_scriptTag = value;
		}
	}

	internal LanguageRecord Language
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

	internal LanguageRecord[] LanguageRecord
	{
		get
		{
			return m_LanguageRecord;
		}
		set
		{
			m_LanguageRecord = value;
		}
	}
}
