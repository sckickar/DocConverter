using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Graphics.Images.Metafiles;

namespace DocGen.Pdf.HtmlToPdf;

internal class HtmlToPdfFormat
{
	private bool m_boundsSet;

	private RectangleF m_paginateBounds;

	private PdfLayoutType m_layout;

	private PdfLayoutBreakType m_break;

	private bool m_splitTextLines;

	private bool m_splitImages;

	internal float TotalPageLayoutSize;

	internal int PageCount;

	internal int PageNumber;

	internal double TotalPageSize;

	private ImageRegionManager m_imageRegionManager = new ImageRegionManager();

	private TextRegionManager m_textRegionManager = new TextRegionManager();

	private ImageRegionManager m_formRegionManager = new ImageRegionManager();

	private List<HtmlHyperLink> m_htmlHyperlinksCollection = new List<HtmlHyperLink>();

	private List<HtmlInternalLink> m_htmlInternalLinksCollection = new List<HtmlInternalLink>();

	public bool SplitTextLines
	{
		get
		{
			return m_splitTextLines;
		}
		set
		{
			m_splitTextLines = value;
		}
	}

	public bool SplitImages
	{
		get
		{
			return m_splitImages;
		}
		set
		{
			m_splitImages = value;
		}
	}

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

	internal TextRegionManager TextRegionManager
	{
		get
		{
			return m_textRegionManager;
		}
		set
		{
			m_textRegionManager = value;
		}
	}

	internal List<HtmlHyperLink> HtmlHyperlinksCollection
	{
		get
		{
			return m_htmlHyperlinksCollection;
		}
		set
		{
			m_htmlHyperlinksCollection = value;
		}
	}

	internal List<HtmlInternalLink> HtmlInternalLinksCollection
	{
		get
		{
			return m_htmlInternalLinksCollection;
		}
		set
		{
			m_htmlInternalLinksCollection = value;
		}
	}

	internal ImageRegionManager ImageRegionManager
	{
		get
		{
			return m_imageRegionManager;
		}
		set
		{
			m_imageRegionManager = value;
		}
	}

	internal ImageRegionManager FormRegionManager
	{
		get
		{
			return m_formRegionManager;
		}
		set
		{
			m_formRegionManager = value;
		}
	}

	internal bool UsePaginateBounds => m_boundsSet;
}
