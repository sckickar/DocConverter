using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf;

public class PdfDestinationPageNumberField : PdfPageNumberField
{
	private PdfPage m_page;

	private PdfLoadedPage m_loadedPage;

	public PdfLoadedPage LoadedPage
	{
		get
		{
			return m_loadedPage;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Page");
			}
			m_loadedPage = value;
		}
	}

	public PdfPage Page
	{
		get
		{
			return m_page;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Page");
			}
			m_page = value;
		}
	}

	public PdfDestinationPageNumberField()
	{
	}

	public PdfDestinationPageNumberField(PdfFont font)
		: base(font)
	{
	}

	public PdfDestinationPageNumberField(PdfFont font, PdfBrush brush)
		: base(font, brush)
	{
	}

	public PdfDestinationPageNumberField(PdfFont font, RectangleF bounds)
		: base(font, bounds)
	{
	}

	protected internal override string GetValue(PdfGraphics graphics)
	{
		if (m_loadedPage != null)
		{
			return InternalLoadedGetValue(m_loadedPage);
		}
		return InternalGetValue(m_page);
	}
}
