using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Parsing;

namespace DocGen.Pdf;

public class PdfPageNumberField : PdfMultipleNumberValueField
{
	public PdfPageNumberField()
	{
	}

	public PdfPageNumberField(PdfFont font)
		: base(font)
	{
	}

	public PdfPageNumberField(PdfFont font, PdfBrush brush)
		: base(font, brush)
	{
	}

	public PdfPageNumberField(PdfFont font, RectangleF bounds)
		: base(font, bounds)
	{
	}

	protected internal override string GetValue(PdfGraphics graphics)
	{
		string result = null;
		if (graphics.Page is PdfPage)
		{
			PdfPage pageFromGraphics = PdfDynamicField.GetPageFromGraphics(graphics);
			if (pageFromGraphics.Section.m_document is PdfLoadedDocument)
			{
				PdfLoadedDocument pdfLoadedDocument = pageFromGraphics.Section.m_document as PdfLoadedDocument;
				int count = (pageFromGraphics.Section.m_document as PdfLoadedDocument).Pages.Count;
				for (int i = 0; i < count; i++)
				{
					if (pdfLoadedDocument.Pages[i] is PdfPage && (pdfLoadedDocument.Pages[i] as PdfPage).Dictionary.Equals(graphics.Page.Dictionary))
					{
						result = (i + 1).ToString();
					}
				}
			}
			else
			{
				result = InternalGetValue(pageFromGraphics);
			}
		}
		else if (graphics.Page is PdfLoadedPage)
		{
			PdfLoadedPage loadedPageFromGraphics = PdfDynamicField.GetLoadedPageFromGraphics(graphics);
			result = InternalLoadedGetValue(loadedPageFromGraphics);
		}
		return result;
	}

	protected string InternalGetValue(PdfPage page)
	{
		return PdfNumbersConvertor.Convert((((page.Section != null && page.Section.Parent != null && page.Section.Parent.Document != null) ? page.Section.Parent.Document : page.Document) ?? throw new ArgumentNullException("document instance not retrieved properly")).Pages.IndexOf(page) + 1, base.NumberStyle);
	}

	protected string InternalLoadedGetValue(PdfLoadedPage page)
	{
		return PdfNumbersConvertor.Convert((page.Document as PdfLoadedDocument).Pages.IndexOf(page) + 1, base.NumberStyle);
	}
}
