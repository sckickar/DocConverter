using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Graphics;

internal sealed class PdfExternalGraphicsState : IPdfWrapper
{
	private PdfDictionary m_stateDictionary = new PdfDictionary();

	public IPdfPrimitive Element => m_stateDictionary;
}
