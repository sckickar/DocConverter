using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Lists;

public class ListBeginPageLayoutEventArgs : BeginPageLayoutEventArgs
{
	private PdfList m_list;

	public PdfList List => m_list;

	internal ListBeginPageLayoutEventArgs(RectangleF bounds, PdfPage page, PdfList list)
		: base(bounds, page)
	{
		m_list = list;
	}
}
