using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf;

public class PdfDocumentAuthorField : PdfSingleValueField
{
	public PdfDocumentAuthorField()
	{
	}

	public PdfDocumentAuthorField(PdfFont font)
		: base(font)
	{
	}

	public PdfDocumentAuthorField(PdfFont font, PdfBrush brush)
		: base(font, brush)
	{
	}

	public PdfDocumentAuthorField(PdfFont font, RectangleF bounds)
		: base(font, bounds)
	{
	}

	protected internal override string GetValue(PdfGraphics graphics)
	{
		string result = null;
		if (graphics.Page is PdfPage)
		{
			result = PdfDynamicField.GetPageFromGraphics(graphics).Document.DocumentInformation.Author;
		}
		else if (graphics.Page is PdfLoadedPage)
		{
			result = PdfDynamicField.GetLoadedPageFromGraphics(graphics).Document.DocumentInformation.Author;
		}
		return result;
	}
}
