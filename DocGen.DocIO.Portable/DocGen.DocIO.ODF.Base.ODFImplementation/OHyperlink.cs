namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class OHyperlink : OParagraphItem
{
	private string m_fieldValue;

	private string m_text;

	internal string FieldValue
	{
		get
		{
			return m_fieldValue;
		}
		set
		{
			m_fieldValue = value;
		}
	}

	internal new string Text
	{
		get
		{
			return m_text;
		}
		set
		{
			m_text = value;
		}
	}
}
