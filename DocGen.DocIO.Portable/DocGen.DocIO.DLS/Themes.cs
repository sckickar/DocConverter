using System.Collections.Generic;
using System.IO;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

internal class Themes
{
	private IWordDocument m_document;

	private FormatScheme m_fmtScheme;

	private FontScheme m_fontScheme;

	private Dictionary<string, Color> m_schemeColor;

	private string m_colorSchemeName;

	internal Dictionary<string, Stream> m_docxProps;

	internal Dictionary<string, Color> SchemeColor
	{
		get
		{
			if (m_schemeColor == null)
			{
				m_schemeColor = new Dictionary<string, Color>();
			}
			return m_schemeColor;
		}
	}

	internal FormatScheme FmtScheme
	{
		get
		{
			if (m_fmtScheme == null)
			{
				m_fmtScheme = new FormatScheme();
			}
			return m_fmtScheme;
		}
		set
		{
			m_fmtScheme = value;
		}
	}

	internal FontScheme FontScheme
	{
		get
		{
			if (m_fontScheme == null)
			{
				m_fontScheme = new FontScheme();
			}
			return m_fontScheme;
		}
		set
		{
			m_fontScheme = value;
		}
	}

	internal Dictionary<string, Stream> DocxProps
	{
		get
		{
			if (m_docxProps == null)
			{
				m_docxProps = new Dictionary<string, Stream>();
			}
			return m_docxProps;
		}
	}

	internal string ColorSchemeName
	{
		get
		{
			return m_colorSchemeName;
		}
		set
		{
			m_colorSchemeName = value;
		}
	}

	internal Themes(IWordDocument doc)
	{
		m_document = doc;
		m_schemeColor = new Dictionary<string, Color>();
		m_fmtScheme = new FormatScheme();
		m_fontScheme = new FontScheme();
	}

	internal void Close()
	{
		m_document = null;
		if (m_fmtScheme != null)
		{
			m_fmtScheme.Close();
			m_fmtScheme = null;
		}
		if (m_fontScheme != null)
		{
			m_fontScheme.Close();
			m_fontScheme = null;
		}
		if (m_schemeColor != null)
		{
			m_schemeColor.Clear();
			m_schemeColor = null;
		}
	}
}
