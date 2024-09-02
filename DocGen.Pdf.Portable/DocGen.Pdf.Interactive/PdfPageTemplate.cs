using System;

namespace DocGen.Pdf.Interactive;

public class PdfPageTemplate
{
	private string m_pageTemplateName;

	private bool m_isVisible = true;

	private PdfPageBase m_pageBase;

	public string Name
	{
		get
		{
			return m_pageTemplateName;
		}
		set
		{
			if (value == null && value != null && value.Length == 0)
			{
				throw new ArgumentNullException("The PageTemplate name can't be null/empty");
			}
			m_pageTemplateName = value;
		}
	}

	public bool IsVisible
	{
		get
		{
			return m_isVisible;
		}
		set
		{
			m_isVisible = value;
		}
	}

	internal PdfPageBase PdfPageBase
	{
		get
		{
			return m_pageBase;
		}
		set
		{
			m_pageBase = value;
		}
	}

	internal PdfPageTemplate()
	{
	}

	public PdfPageTemplate(PdfPageBase page, string name, bool isVisible)
	{
		if (page == null)
		{
			throw new ArgumentNullException("The Page can't be null");
		}
		if (name == null || (name != null && name.Length == 0))
		{
			throw new ArgumentNullException("PdfPageTemplate name can't be null/empty");
		}
		m_pageBase = page;
		m_pageTemplateName = name;
		m_isVisible = isVisible;
	}

	public PdfPageTemplate(PdfPageBase page)
	{
		if (page == null)
		{
			throw new ArgumentNullException("The Page can't be null");
		}
		m_pageBase = page;
	}
}
