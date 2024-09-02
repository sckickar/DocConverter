namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class ListLevelProperties
{
	private string m_numberSufix;

	private string m_numberPrefix;

	private float m_spaceBefore;

	private float m_minimumLabelWidth;

	private float m_leftMargin;

	private float m_textIndent;

	private ListNumberFormat m_numberFormat;

	private string m_bulletCharacter;

	private ODFStyle m_odfStyle;

	private TextProperties m_TextProperties;

	private bool m_isPictureBullet;

	private string m_href;

	private OPicture m_pictureBullet;

	private TextAlign m_textAlign;

	internal string PictureHRef
	{
		get
		{
			return m_href;
		}
		set
		{
			m_href = value;
		}
	}

	internal OPicture PictureBullet
	{
		get
		{
			return m_pictureBullet;
		}
		set
		{
			m_pictureBullet = value;
		}
	}

	internal bool IsPictureBullet
	{
		get
		{
			return m_isPictureBullet;
		}
		set
		{
			m_isPictureBullet = value;
		}
	}

	internal ODFStyle Style
	{
		get
		{
			return m_odfStyle;
		}
		set
		{
			m_odfStyle = value;
		}
	}

	internal TextProperties TextProperties
	{
		get
		{
			return m_TextProperties;
		}
		set
		{
			m_TextProperties = value;
		}
	}

	internal string BulletCharacter
	{
		get
		{
			return m_bulletCharacter;
		}
		set
		{
			m_bulletCharacter = value;
		}
	}

	internal string NumberSufix
	{
		get
		{
			return m_numberSufix;
		}
		set
		{
			m_numberSufix = value;
		}
	}

	internal string NumberPrefix
	{
		get
		{
			return m_numberPrefix;
		}
		set
		{
			m_numberPrefix = value;
		}
	}

	internal float SpaceBefore
	{
		get
		{
			return m_spaceBefore;
		}
		set
		{
			m_spaceBefore = value;
		}
	}

	internal float MinimumLabelWidth
	{
		get
		{
			return m_minimumLabelWidth;
		}
		set
		{
			m_minimumLabelWidth = value;
		}
	}

	internal float LeftMargin
	{
		get
		{
			return m_leftMargin;
		}
		set
		{
			m_leftMargin = value;
		}
	}

	internal float TextIndent
	{
		get
		{
			return m_textIndent;
		}
		set
		{
			m_textIndent = value;
		}
	}

	internal ListNumberFormat NumberFormat
	{
		get
		{
			return m_numberFormat;
		}
		set
		{
			m_numberFormat = value;
		}
	}

	internal TextAlign TextAlignment
	{
		get
		{
			return m_textAlign;
		}
		set
		{
			m_textAlign = value;
		}
	}

	internal void Close()
	{
		if (m_odfStyle != null)
		{
			m_odfStyle.Close();
			m_odfStyle = null;
		}
	}
}
