using System.Collections.Generic;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS.Rendering;

internal class PageResult
{
	private Image m_image;

	private List<Dictionary<string, RectangleF>> m_hyperLinks;

	private List<Dictionary<string, DocumentLayouter.BookmarkHyperlink>> m_bookmarkHyperlinks;

	public Image PageImage
	{
		get
		{
			return m_image;
		}
		set
		{
			m_image = value;
		}
	}

	public List<Dictionary<string, RectangleF>> Hyperlinks
	{
		get
		{
			return m_hyperLinks;
		}
		set
		{
			m_hyperLinks = value;
		}
	}

	public List<Dictionary<string, DocumentLayouter.BookmarkHyperlink>> BookmarkHyperlinks
	{
		get
		{
			return m_bookmarkHyperlinks;
		}
		set
		{
			m_bookmarkHyperlinks = value;
		}
	}

	public PageResult()
	{
	}

	public PageResult(Image image, List<Dictionary<string, RectangleF>> hyperlinks, List<Dictionary<string, DocumentLayouter.BookmarkHyperlink>> bookmarkHyperlinks)
	{
		m_image = image;
		m_hyperLinks = hyperlinks;
		m_bookmarkHyperlinks = bookmarkHyperlinks;
	}
}
