using System;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf;

internal class PdfTemplateValuePair
{
	private PdfTemplate m_template;

	private string m_value = string.Empty;

	public PdfTemplate Template
	{
		get
		{
			return m_template;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Template");
			}
			m_template = value;
		}
	}

	public string Value
	{
		get
		{
			return m_value;
		}
		set
		{
			if (m_value == null)
			{
				throw new ArgumentNullException("Value");
			}
			m_value = value;
		}
	}

	public PdfTemplateValuePair()
	{
	}

	public PdfTemplateValuePair(PdfTemplate template, string value)
	{
		Template = template;
		Value = value;
	}
}
