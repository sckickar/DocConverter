namespace DocGen.Pdf.Primitives;

internal struct PdfPair
{
	public PdfName Key;

	public IPdfPrimitive Value;

	internal PdfPair(PdfName key, IPdfPrimitive value)
	{
		Key = key;
		Value = value;
	}
}
