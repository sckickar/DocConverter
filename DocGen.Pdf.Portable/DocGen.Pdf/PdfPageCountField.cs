using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Parsing;

namespace DocGen.Pdf;

public class PdfPageCountField : PdfSingleValueField
{
	private PdfNumberStyle m_numberStyle = PdfNumberStyle.Numeric;

	public PdfNumberStyle NumberStyle
	{
		get
		{
			return m_numberStyle;
		}
		set
		{
			m_numberStyle = value;
		}
	}

	public PdfPageCountField()
	{
	}

	public PdfPageCountField(PdfFont font)
		: base(font)
	{
	}

	public PdfPageCountField(PdfFont font, PdfBrush brush)
		: base(font, brush)
	{
	}

	public PdfPageCountField(PdfFont font, RectangleF bounds)
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
				_ = pageFromGraphics.Section.m_document;
				result = (pageFromGraphics.Section.m_document as PdfLoadedDocument).Pages.Count.ToString();
			}
			else
			{
				result = PdfNumbersConvertor.Convert(pageFromGraphics.Section.Parent.Document.Pages.Count, NumberStyle);
			}
		}
		else if (graphics.Page is PdfLoadedPage)
		{
			result = PdfNumbersConvertor.Convert((PdfDynamicField.GetLoadedPageFromGraphics(graphics).Document as PdfLoadedDocument).Pages.Count, NumberStyle);
		}
		return result;
	}
}
