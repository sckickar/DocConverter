namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class NumberType : CommonType
{
	private string m_decimalReplacement;

	private string m_displayFactor;

	private EmbeddedTextType m_embeddedText;

	internal string DecimalReplacement
	{
		get
		{
			return m_decimalReplacement;
		}
		set
		{
			m_decimalReplacement = value;
		}
	}

	internal string DisplayFactor
	{
		get
		{
			return m_displayFactor;
		}
		set
		{
			m_displayFactor = value;
		}
	}

	internal EmbeddedTextType EmbeddedText
	{
		get
		{
			return m_embeddedText;
		}
		set
		{
			m_embeddedText = value;
		}
	}

	internal NumberType()
	{
		m_embeddedText = new EmbeddedTextType();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is NumberType numberType))
		{
			return false;
		}
		bool flag = false;
		flag = base.DecimalPlaces.Equals(numberType.DecimalPlaces);
		if (!flag)
		{
			return flag;
		}
		flag = base.MinIntegerDigits.Equals(numberType.MinIntegerDigits);
		if (!flag)
		{
			return flag;
		}
		return base.Grouping.Equals(numberType.Grouping);
	}

	internal void Dispose()
	{
		if (m_embeddedText != null)
		{
			m_embeddedText = null;
		}
	}
}
