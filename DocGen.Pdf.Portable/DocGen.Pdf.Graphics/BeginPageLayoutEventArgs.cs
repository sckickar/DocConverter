using System;
using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

public class BeginPageLayoutEventArgs : PdfCancelEventArgs
{
	private RectangleF m_bounds;

	private PdfPage m_page;

	public RectangleF Bounds
	{
		get
		{
			return m_bounds;
		}
		set
		{
			m_bounds = value;
		}
	}

	public PdfPage Page => m_page;

	public BeginPageLayoutEventArgs(RectangleF bounds, PdfPage page)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		m_bounds = bounds;
		m_page = page;
	}
}
