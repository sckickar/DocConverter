using System;
using DocGen.Drawing;
using DocGen.Pdf.HtmlToPdf;

namespace DocGen.Pdf.Graphics;

internal abstract class ElementLayouter
{
	private PdfLayoutElement m_element;

	protected bool m_isImagePath;

	public PdfLayoutElement Element => m_element;

	internal bool IsImagePath
	{
		get
		{
			return m_isImagePath;
		}
		set
		{
			m_isImagePath = value;
		}
	}

	public ElementLayouter(PdfLayoutElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		m_element = element;
	}

	public PdfLayoutResult Layout(PdfLayoutParams param)
	{
		if (param == null)
		{
			throw new ArgumentNullException("param");
		}
		return LayoutInternal(param);
	}

	protected virtual PdfLayoutResult LayoutInternal(HtmlToPdfParams param)
	{
		return null;
	}

	internal PdfLayoutResult Layout(HtmlToPdfParams param)
	{
		if (param == null)
		{
			throw new ArgumentNullException("param");
		}
		return LayoutInternal(param);
	}

	public PdfLayoutResult Layout(HtmlToPdfLayoutParams param)
	{
		if (param == null)
		{
			throw new ArgumentNullException("param");
		}
		return LayoutInternal(param);
	}

	public PdfPage GetNextPage(PdfPage currentPage)
	{
		if (currentPage == null)
		{
			throw new ArgumentNullException("currentPage");
		}
		PdfSection section = currentPage.Section;
		PdfPage pdfPage = null;
		int num = section.IndexOf(currentPage);
		if (num == section.Count - 1)
		{
			return section.Add();
		}
		return section[num + 1];
	}

	protected abstract PdfLayoutResult LayoutInternal(PdfLayoutParams param);

	protected virtual PdfLayoutResult LayoutInternal(HtmlToPdfLayoutParams param)
	{
		return null;
	}

	protected RectangleF GetPaginateBounds(PdfLayoutParams param)
	{
		if (param == null)
		{
			throw new ArgumentNullException("param");
		}
		if (!param.Format.UsePaginateBounds)
		{
			return new RectangleF(param.Bounds.X, 0f, param.Bounds.Width, param.Bounds.Height);
		}
		return param.Format.PaginateBounds;
	}
}
