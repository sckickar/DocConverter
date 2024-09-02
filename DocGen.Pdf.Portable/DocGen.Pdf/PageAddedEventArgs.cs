using System;

namespace DocGen.Pdf;

public class PageAddedEventArgs : EventArgs
{
	private PdfPage m_page;

	public PdfPage Page => m_page;

	private PageAddedEventArgs()
	{
	}

	public PageAddedEventArgs(PdfPage page)
	{
		m_page = page;
	}
}
