using System;

namespace DocGen.Pdf.Graphics;

public class EndPageLayoutEventArgs : PdfCancelEventArgs
{
	private PdfLayoutResult m_result;

	private PdfPage m_nextPage;

	public PdfLayoutResult Result => m_result;

	public PdfPage NextPage
	{
		get
		{
			return m_nextPage;
		}
		set
		{
			m_nextPage = value;
		}
	}

	public EndPageLayoutEventArgs(PdfLayoutResult result)
	{
		if (result == null)
		{
			throw new ArgumentNullException("result");
		}
		m_result = result;
	}
}
