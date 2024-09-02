namespace DocGen.Pdf.Lists;

public class EndItemLayoutEventArgs
{
	private PdfListItem m_item;

	private PdfPage m_page;

	public PdfListItem Item => m_item;

	public PdfPage Page => m_page;

	internal EndItemLayoutEventArgs(PdfListItem item, PdfPage page)
	{
		m_item = item;
		m_page = page;
	}
}
