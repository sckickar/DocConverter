using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedTexBoxItem : PdfLoadedFieldItem
{
	internal PdfLoadedTexBoxItem(PdfLoadedStyledField field, int index, PdfDictionary dictionary)
		: base(field, index, dictionary)
	{
	}

	internal PdfLoadedTexBoxItem Clone()
	{
		return (PdfLoadedTexBoxItem)MemberwiseClone();
	}
}
