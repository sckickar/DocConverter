using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedListFieldItem : PdfLoadedFieldItem
{
	internal PdfLoadedListFieldItem(PdfLoadedStyledField field, int index, PdfDictionary dictionary)
		: base(field, index, dictionary)
	{
	}

	internal PdfLoadedListFieldItem Clone()
	{
		return (PdfLoadedListFieldItem)MemberwiseClone();
	}
}
