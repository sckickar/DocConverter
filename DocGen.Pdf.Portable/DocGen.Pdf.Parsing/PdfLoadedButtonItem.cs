using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedButtonItem : PdfLoadedFieldItem
{
	internal PdfLoadedButtonItem(PdfLoadedStyledField field, int index, PdfDictionary dictionary)
		: base(field, index, dictionary)
	{
	}

	internal PdfLoadedButtonItem Clone()
	{
		return (PdfLoadedButtonItem)MemberwiseClone();
	}
}
