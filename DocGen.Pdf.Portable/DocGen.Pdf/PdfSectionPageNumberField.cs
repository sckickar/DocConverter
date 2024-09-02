using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

public class PdfSectionPageNumberField : PdfMultipleNumberValueField
{
	public PdfSectionPageNumberField()
	{
	}

	public PdfSectionPageNumberField(PdfFont font)
		: base(font)
	{
	}

	public PdfSectionPageNumberField(PdfFont font, PdfBrush brush)
		: base(font, brush)
	{
	}

	public PdfSectionPageNumberField(PdfFont font, RectangleF bounds)
		: base(font, bounds)
	{
	}

	protected internal override string GetValue(PdfGraphics graphics)
	{
		string result = null;
		if (graphics.Page is PdfPage)
		{
			PdfPage pageFromGraphics = PdfDynamicField.GetPageFromGraphics(graphics);
			result = PdfNumbersConvertor.Convert(pageFromGraphics.Section.IndexOf(pageFromGraphics) + 1, base.NumberStyle);
		}
		else if (graphics.Page is PdfLoadedPage)
		{
			PdfDynamicField.GetLoadedPageFromGraphics(graphics);
			PdfLoadedPage pdfLoadedPage = graphics.Page as PdfLoadedPage;
			_ = pdfLoadedPage.Document;
			PdfDictionary catalog = pdfLoadedPage.Document.Catalog;
			PdfArray pdfArray = (pdfLoadedPage.CrossTable.GetObject(catalog["Pages"]) as PdfDictionary)["Kids"] as PdfArray;
			for (int i = 0; i < pdfArray.Count; i++)
			{
				PdfReferenceHolder pdfReferenceHolder = new PdfReferenceHolder(pdfLoadedPage);
				PdfDictionary pdfDictionary = (pdfArray[i] as PdfReferenceHolder).Object as PdfDictionary;
				if (!(pdfDictionary["Type"].ToString() == "/Pages"))
				{
					continue;
				}
				PdfArray pdfArray2 = pdfLoadedPage.CrossTable.GetObject(pdfDictionary["Kids"]) as PdfArray;
				for (int j = 0; j < pdfArray2.Count; j++)
				{
					PdfReferenceHolder pdfReferenceHolder2 = pdfArray2[j] as PdfReferenceHolder;
					if (pdfReferenceHolder.Object.Equals(pdfReferenceHolder2.Object))
					{
						result = PdfNumbersConvertor.Convert(j + 1, base.NumberStyle);
					}
				}
			}
		}
		return result;
	}
}
