using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Lists;

public class ListEndPageLayoutEventArgs : EndPageLayoutEventArgs
{
	private PdfList m_list;

	public PdfList List => m_list;

	internal ListEndPageLayoutEventArgs(PdfLayoutResult layoutResult, PdfList list)
		: base(layoutResult)
	{
		m_list = list;
	}
}
