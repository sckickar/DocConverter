using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

public class PdfSectionPageCountField : PdfMultipleNumberValueField
{
	public PdfSectionPageCountField()
	{
	}

	public PdfSectionPageCountField(PdfFont font)
		: base(font)
	{
	}

	public PdfSectionPageCountField(PdfFont font, PdfBrush brush)
		: base(font, brush)
	{
	}

	public PdfSectionPageCountField(PdfFont font, RectangleF bounds)
		: base(font, bounds)
	{
	}

	protected internal override string GetValue(PdfGraphics graphics)
	{
		string result = null;
		if (graphics.Page is PdfPage)
		{
			result = PdfNumbersConvertor.Convert(PdfDynamicField.GetPageFromGraphics(graphics).Section.Count, base.NumberStyle);
		}
		else if (graphics.Page is PdfLoadedPage)
		{
			PdfLoadedPage loadedPageFromGraphics = PdfDynamicField.GetLoadedPageFromGraphics(graphics);
			PdfLoadedPage pdfLoadedPage = graphics.Page as PdfLoadedPage;
			_ = pdfLoadedPage.Document;
			PdfDictionary catalog = pdfLoadedPage.Document.Catalog;
			PdfArray pdfArray = (pdfLoadedPage.CrossTable.GetObject(catalog["Pages"]) as PdfDictionary)["Kids"] as PdfArray;
			for (int i = 0; i < pdfArray.Count; i++)
			{
				new PdfReferenceHolder(pdfLoadedPage);
				PdfDictionary pdfDictionary = (pdfArray[i] as PdfReferenceHolder).Object as PdfDictionary;
				if (!(pdfDictionary["Type"].ToString() == "/Pages"))
				{
					continue;
				}
				PdfArray pdfArray2 = pdfLoadedPage.CrossTable.GetObject(pdfDictionary["Kids"]) as PdfArray;
				for (int j = 0; j < pdfArray2.Count; j++)
				{
					PdfReferenceHolder pointer = pdfArray2[j] as PdfReferenceHolder;
					if ((loadedPageFromGraphics.CrossTable.GetObject(pointer) as PdfDictionary).Equals(loadedPageFromGraphics.Dictionary))
					{
						result = PdfNumbersConvertor.Convert(pdfArray2.Count, base.NumberStyle);
					}
				}
			}
		}
		return result;
	}
}
