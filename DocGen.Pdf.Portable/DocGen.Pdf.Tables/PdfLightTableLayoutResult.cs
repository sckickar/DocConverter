using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Tables;

public class PdfLightTableLayoutResult : PdfLayoutResult
{
	private PdfStringLayoutResult[] m_cellResults;

	private int m_rowIndex;

	internal PdfStringLayoutResult[] CellResults => m_cellResults;

	public int LastRowIndex => m_rowIndex;

	internal PdfLightTableLayoutResult(PdfPage page, RectangleF bounds, int rowIndex, PdfStringLayoutResult[] cellResults)
		: base(page, bounds)
	{
		m_rowIndex = rowIndex;
		m_cellResults = cellResults;
	}
}
