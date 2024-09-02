using System;
using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

public class PdfLayoutFormat
{
	private bool m_boundsSet;

	private RectangleF m_paginateBounds;

	private PdfLayoutType m_layout;

	private PdfLayoutBreakType m_break;

	public PdfLayoutType Layout
	{
		get
		{
			return m_layout;
		}
		set
		{
			m_layout = value;
		}
	}

	public PdfLayoutBreakType Break
	{
		get
		{
			return m_break;
		}
		set
		{
			m_break = value;
		}
	}

	public RectangleF PaginateBounds
	{
		get
		{
			return m_paginateBounds;
		}
		set
		{
			m_paginateBounds = value;
			m_boundsSet = true;
		}
	}

	internal bool UsePaginateBounds => m_boundsSet;

	public PdfLayoutFormat()
	{
	}

	public PdfLayoutFormat(PdfLayoutFormat baseFormat)
		: this()
	{
		if (baseFormat == null)
		{
			throw new ArgumentNullException("baseFormat");
		}
		Break = baseFormat.Break;
		Layout = baseFormat.Layout;
		PaginateBounds = baseFormat.PaginateBounds;
		m_boundsSet = baseFormat.UsePaginateBounds;
	}
}
