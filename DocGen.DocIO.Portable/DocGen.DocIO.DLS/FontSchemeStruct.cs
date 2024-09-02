namespace DocGen.DocIO.DLS;

internal struct FontSchemeStruct
{
	private string m_name;

	private string m_typeface;

	private byte m_charSet;

	private string m_panose;

	private byte m_pitchFamily;

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

	internal string Typeface
	{
		get
		{
			return m_typeface;
		}
		set
		{
			m_typeface = value;
		}
	}

	internal byte Charset
	{
		get
		{
			return m_charSet;
		}
		set
		{
			m_charSet = value;
		}
	}

	internal string Panose
	{
		get
		{
			return m_panose;
		}
		set
		{
			m_panose = value;
		}
	}

	internal byte PitchFamily
	{
		get
		{
			return m_pitchFamily;
		}
		set
		{
			m_pitchFamily = value;
		}
	}
}
