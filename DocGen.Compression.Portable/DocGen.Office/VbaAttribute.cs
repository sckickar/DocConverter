namespace DocGen.Office;

internal class VbaAttribute
{
	private string m_name;

	private string m_value;

	private bool m_isText = true;

	internal string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
		}
	}

	internal string Value
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

	internal bool IsText
	{
		get
		{
			return m_isText;
		}
		set
		{
			m_isText = value;
		}
	}

	internal VbaAttribute Clone()
	{
		return (VbaAttribute)MemberwiseClone();
	}
}
