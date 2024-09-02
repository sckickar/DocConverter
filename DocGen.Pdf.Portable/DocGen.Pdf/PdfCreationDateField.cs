using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Parsing;

namespace DocGen.Pdf;

public class PdfCreationDateField : PdfSingleValueField
{
	private string m_formatString = "dd'/'MM'/'yyyy";

	public string DateFormatString
	{
		get
		{
			return m_formatString;
		}
		set
		{
			m_formatString = value;
		}
	}

	public PdfCreationDateField()
	{
	}

	public PdfCreationDateField(PdfFont font)
		: base(font)
	{
	}

	public PdfCreationDateField(PdfFont font, PdfBrush brush)
		: base(font, brush)
	{
	}

	public PdfCreationDateField(PdfFont font, RectangleF bounds)
		: base(font, bounds)
	{
	}

	protected internal override string GetValue(PdfGraphics graphics)
	{
		string result = null;
		if (graphics.Page is PdfPage)
		{
			PdfPage pageFromGraphics = PdfDynamicField.GetPageFromGraphics(graphics);
			result = ((!(pageFromGraphics.Section.m_document is PdfLoadedDocument)) ? pageFromGraphics.Document.DocumentInformation.CreationDate.ToString(m_formatString) : (pageFromGraphics.Section.m_document as PdfLoadedDocument).DocumentInformation.CreationDate.ToString(m_formatString));
		}
		else if (graphics.Page is PdfLoadedPage)
		{
			result = (PdfDynamicField.GetLoadedPageFromGraphics(graphics).Document as PdfLoadedDocument).DocumentInformation.CreationDate.ToString(m_formatString);
		}
		return result;
	}
}
