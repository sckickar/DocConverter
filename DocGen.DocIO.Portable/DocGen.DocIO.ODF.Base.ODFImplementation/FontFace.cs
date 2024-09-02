namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class FontFace
{
	private FontFamilyGeneric m_fontfamilyGeneric;

	private FontPitch m_fontPitch;

	private string m_name;

	internal FontFamilyGeneric FontFamilyGeneric
	{
		get
		{
			return m_fontfamilyGeneric;
		}
		set
		{
			m_fontfamilyGeneric = value;
		}
	}

	internal FontPitch FontPitch
	{
		get
		{
			return m_fontPitch;
		}
		set
		{
			m_fontPitch = value;
		}
	}

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

	internal FontFace(string name)
	{
		m_name = name;
	}

	public override bool Equals(object obj)
	{
		FontFace fontFace = obj as FontFace;
		if (Name == fontFace.Name)
		{
			return true;
		}
		return false;
	}
}
