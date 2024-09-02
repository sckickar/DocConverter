namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class OMergeField : OParagraphItem
{
	private string m_fieldName;

	private string m_textBefore;

	private string m_textAfter;

	private string m_text;

	internal string FieldName
	{
		get
		{
			return m_fieldName;
		}
		set
		{
			m_fieldName = value;
		}
	}

	internal string TextBefore
	{
		get
		{
			return m_textBefore;
		}
		set
		{
			m_textBefore = value;
		}
	}

	internal string TextAfter
	{
		get
		{
			return m_textAfter;
		}
		set
		{
			m_textAfter = value;
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
