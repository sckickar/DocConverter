using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

public class PdfSectionNumberField : PdfMultipleNumberValueField
{
	public PdfSectionNumberField()
	{
	}

	public PdfSectionNumberField(PdfFont font)
		: base(font)
	{
	}

	public PdfSectionNumberField(PdfFont font, PdfBrush brush)
		: base(font, brush)
	{
	}

	public PdfSectionNumberField(PdfFont font, RectangleF bounds)
		: base(font, bounds)
	{
	}

	protected internal override string GetValue(PdfGraphics graphics)
	{
		string result = null;
		if (graphics.Page is PdfPage)
		{
			PdfPage pdfPage = graphics.Page as PdfPage;
			if (pdfPage.Section.m_document is PdfLoadedDocument)
			{
				PdfReferenceHolder pointer = pdfPage.Dictionary["Parent"] as PdfReferenceHolder;
				PdfDictionary obj = pdfPage.Section.m_document.CrossTable.GetObject(pointer) as PdfDictionary;
				PdfLoadedDocument obj2 = pdfPage.Section.m_document as PdfLoadedDocument;
				PdfDictionary catalog = obj2.Catalog;
				PdfArray pdfArray = (obj2.CrossTable.GetObject(catalog["Pages"]) as PdfDictionary)["Kids"] as PdfArray;
				for (int i = 0; i < pdfArray.Count; i++)
				{
					if (((pdfArray[i] as PdfReferenceHolder).Object as PdfDictionary).Equals(obj))
					{
						result = PdfNumbersConvertor.Convert(i + 1, base.NumberStyle);
					}
				}
			}
			else
			{
				result = PdfNumbersConvertor.Convert(pdfPage.Document.Sections.IndexOf(pdfPage.Section) + 1, base.NumberStyle);
			}
		}
		else if (graphics.Page is PdfLoadedPage)
		{
			PdfLoadedPage obj3 = graphics.Page as PdfLoadedPage;
			_ = obj3.Document;
			PdfDictionary catalog2 = obj3.Document.Catalog;
			PdfArray pdfArray2 = (obj3.CrossTable.GetObject(catalog2["Pages"]) as PdfDictionary)["Kids"] as PdfArray;
			int num = (int)(obj3.Dictionary["Parent"] as PdfReferenceHolder).Reference.ObjNum;
			for (int j = 0; j < pdfArray2.Count; j++)
			{
				PdfReferenceHolder pdfReferenceHolder = pdfArray2[j] as PdfReferenceHolder;
				if (pdfReferenceHolder.Reference != null && (int)pdfReferenceHolder.Reference.ObjNum == num)
				{
					result = PdfNumbersConvertor.Convert(j + 1, base.NumberStyle);
				}
			}
		}
		return result;
	}
}
