using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Tables;

public class LightTableEndPageLayoutEventArgs : EndPageLayoutEventArgs
{
	private int m_startRow;

	private int m_endRow;

	public int StartRowIndex => m_startRow;

	public int EndRowIndex => m_endRow;

	internal LightTableEndPageLayoutEventArgs(PdfLightTableLayoutResult result, int startRow, int endRow)
		: base(result)
	{
		m_startRow = startRow;
		m_endRow = endRow;
	}
}
