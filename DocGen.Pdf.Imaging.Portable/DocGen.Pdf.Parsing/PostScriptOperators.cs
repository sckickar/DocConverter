namespace DocGen.Pdf.Parsing;

internal class PostScriptOperators
{
	private string m_value;

	private PostScriptOperatorTypes m_operatorType;

	internal string Operand
	{
		get
		{
			return m_value;
		}
		set
		{
			m_value = value;
		}
	}

	internal PostScriptOperatorTypes Operatortype
	{
		get
		{
			return m_operatorType;
		}
		set
		{
			m_operatorType = value;
		}
	}

	public PostScriptOperators(PostScriptOperatorTypes key, string value)
	{
		m_operatorType = key;
		m_value = value;
	}
}
