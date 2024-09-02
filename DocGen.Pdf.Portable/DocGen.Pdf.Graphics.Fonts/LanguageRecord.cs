namespace DocGen.Pdf.Graphics.Fonts;

internal class LanguageRecord
{
	private string m_tag;

	private int[] m_records;

	internal string LanguageTag
	{
		get
		{
			return m_tag;
		}
		set
		{
			m_tag = value;
		}
	}

	internal int[] Records
	{
		get
		{
			return m_records;
		}
		set
		{
			m_records = value;
		}
	}
}
