using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedComboBoxItem : PdfLoadedFieldItem
{
	internal PdfLoadedComboBoxItem(PdfLoadedStyledField field, int index, PdfDictionary dictionary)
		: base(field, index, dictionary)
	{
	}

	internal PdfLoadedComboBoxItem Clone()
	{
		return (PdfLoadedComboBoxItem)MemberwiseClone();
	}
}
