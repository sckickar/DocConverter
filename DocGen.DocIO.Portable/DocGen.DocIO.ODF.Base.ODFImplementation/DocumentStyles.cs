using System.Collections.Generic;

namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class DocumentStyles
{
	private List<FontFace> m_fontFaceDecls;

	private CommonStyles m_commmonStyles;

	private AutomaticStyles m_autoStyles;

	private MasterPageCollection m_masterStyles;

	internal List<FontFace> FontFaceDecls
	{
		get
		{
			if (m_fontFaceDecls == null)
			{
				m_fontFaceDecls = new List<FontFace>();
			}
			return m_fontFaceDecls;
		}
		set
		{
			m_fontFaceDecls = value;
		}
	}

	internal CommonStyles CommmonStyles
	{
		get
		{
			if (m_commmonStyles == null)
			{
				m_commmonStyles = new CommonStyles();
			}
			return m_commmonStyles;
		}
		set
		{
			m_commmonStyles = value;
		}
	}

	internal AutomaticStyles AutoStyles
	{
		get
		{
			if (m_autoStyles == null)
			{
				m_autoStyles = new AutomaticStyles();
			}
			return m_autoStyles;
		}
		set
		{
			m_autoStyles = value;
		}
	}

	internal MasterPageCollection MasterStyles
	{
		get
		{
			if (m_masterStyles == null)
			{
				m_masterStyles = new MasterPageCollection();
			}
			return m_masterStyles;
		}
		set
		{
			m_masterStyles = value;
		}
	}
}
