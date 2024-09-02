using System.Collections.Generic;

namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class DocumentContent
{
	private List<FontFace> m_fontFaceDecls;

	private AutomaticStyles m_automaticStyles;

	private OBody m_body;

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

	internal AutomaticStyles AutomaticStyles
	{
		get
		{
			if (m_automaticStyles == null)
			{
				m_automaticStyles = new AutomaticStyles();
			}
			return m_automaticStyles;
		}
		set
		{
			m_automaticStyles = value;
		}
	}

	internal OBody Body
	{
		get
		{
			if (m_body == null)
			{
				m_body = new OBody();
			}
			return m_body;
		}
		set
		{
			m_body = value;
		}
	}
}
