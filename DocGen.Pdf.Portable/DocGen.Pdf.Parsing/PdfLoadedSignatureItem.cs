using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedSignatureItem : PdfLoadedFieldItem
{
	internal PdfLoadedSignatureItem(PdfLoadedStyledField field, int index, PdfDictionary dictionary)
		: base(field, index, dictionary)
	{
	}

	internal PdfLoadedSignatureItem Clone()
	{
		return (PdfLoadedSignatureItem)MemberwiseClone();
	}
}
