using System.Globalization;

namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class OField : OParagraphItem
{
	private string m_formattingstring;

	private string m_fieldValue;

	private string m_text;

	private OFieldType m_oFieldType;

	private PageNumberFormat m_pageNumberFormat;

	private CultureInfo m_fieldCulture;

	internal string FormattingString
	{
		get
		{
			return m_formattingstring;
		}
		set
		{
			m_formattingstring = value;
		}
	}

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

	internal CultureInfo FieldCulture
	{
		get
		{
			return m_fieldCulture;
		}
		set
		{
			m_fieldCulture = value;
		}
	}

	internal OFieldType OFieldType
	{
		get
		{
			return m_oFieldType;
		}
		set
		{
			m_oFieldType = value;
		}
	}

	internal PageNumberFormat PageNumberFormat
	{
		get
		{
			return m_pageNumberFormat;
		}
		set
		{
			m_pageNumberFormat = value;
		}
	}
}
