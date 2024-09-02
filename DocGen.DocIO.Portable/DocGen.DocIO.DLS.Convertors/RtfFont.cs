namespace DocGen.DocIO.DLS.Convertors;

internal class RtfFont
{
	private int m_fontNumber;

	private string m_fontID;

	private string m_fontName;

	private short m_fontCharSet = 1;

	private string m_alternateFontName;

	internal int FontNumber
	{
		get
		{
			return m_fontNumber;
		}
		set
		{
			m_fontNumber = value;
		}
	}

	internal string AlternateFontName
	{
		get
		{
			return m_alternateFontName;
		}
		set
		{
			m_alternateFontName = value;
		}
	}

	internal string FontID
	{
		get
		{
			return m_fontID;
		}
		set
		{
			m_fontID = value;
		}
	}

	internal string FontName
	{
		get
		{
			return m_fontName;
		}
		set
		{
			m_fontName = value;
		}
	}

	internal short FontCharSet
	{
		get
		{
			return m_fontCharSet;
		}
		set
		{
			m_fontCharSet = value;
		}
	}

	internal RtfFont Clone()
	{
		return (RtfFont)MemberwiseClone();
	}
}
