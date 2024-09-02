using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

public class PdfTextLayoutResult : PdfLayoutResult
{
	private string m_remainder;

	private RectangleF m_lastLineBounds;

	public string Remainder => m_remainder;

	public RectangleF LastLineBounds => m_lastLineBounds;

	internal PdfTextLayoutResult(PdfPage page, RectangleF bounds, string remainder, RectangleF lastLineBounds)
		: base(page, bounds)
	{
		m_remainder = remainder;
		m_lastLineBounds = lastLineBounds;
	}
}
