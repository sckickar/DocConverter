using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Interactive;

namespace DocGen.Pdf.HtmlToPdf;

internal class HtmlInternalLink
{
	private string m_href;

	private string m_sourcePageNumber;

	private RectangleF m_bounds;

	private string m_headerTagLevel;

	private string m_id;

	private string m_headerContent;

	private int m_destinationPageNumber;

	private PointF m_destination;

	private PdfPageBase m_destinationPage;

	private float m_tocXcoordinate;

	private float m_tocRectangleHeight;

	private int m_tocPagecount;

	private float m_bottomMargin;

	private const int maxHeaderLevel = 6;

	internal string Href
	{
		get
		{
			return m_href;
		}
		set
		{
			m_href = value;
		}
	}

	internal string SourcePageNumber
	{
		get
		{
			return m_sourcePageNumber;
		}
		set
		{
			m_sourcePageNumber = value;
		}
	}

	internal RectangleF Bounds
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

	internal string HeaderTagLevel
	{
		get
		{
			return m_headerTagLevel;
		}
		set
		{
			m_headerTagLevel = value;
		}
	}

	internal string ID
	{
		get
		{
			return m_id;
		}
		set
		{
			m_id = value;
		}
	}

	internal string HeaderContent
	{
		get
		{
			return m_headerContent;
		}
		set
		{
			m_headerContent = value;
		}
	}

	internal int DestinationPageNumber
	{
		get
		{
			return m_destinationPageNumber;
		}
		set
		{
			m_destinationPageNumber = value;
		}
	}

	internal PdfPageBase DestinationPage
	{
		get
		{
			return m_destinationPage;
		}
		set
		{
			m_destinationPage = value;
		}
	}

	internal PointF Destination
	{
		get
		{
			return m_destination;
		}
		set
		{
			m_destination = value;
		}
	}

	internal float TocXcoordinate
	{
		get
		{
			return m_tocXcoordinate;
		}
		set
		{
			m_tocXcoordinate = value;
		}
	}

	internal float TocRectHeight
	{
		get
		{
			return m_tocRectangleHeight;
		}
		set
		{
			m_tocRectangleHeight = value;
		}
	}

	internal int TocPageCount
	{
		get
		{
			return m_tocPagecount;
		}
		set
		{
			m_tocPagecount = value;
		}
	}

	internal void AddBookmark(PdfPage page, PdfDocument lDoc, List<HtmlInternalLink> internalLinkCollection)
	{
		PdfBookmark[] array = new PdfBookmark[internalLinkCollection.Count];
		int num = 6;
		int num2 = 0;
		int index = 0;
		HtmlInternalLink htmlInternalLink = internalLinkCollection[index];
		for (index = 0; index < internalLinkCollection.Count; index++)
		{
			htmlInternalLink = internalLinkCollection[index];
			if (htmlInternalLink.HeaderTagLevel == null)
			{
				continue;
			}
			int num3 = int.Parse(htmlInternalLink.HeaderTagLevel.Split('H')[1]);
			if (num3 <= num)
			{
				PdfBookmark pdfBookmark = page.Document.Bookmarks.Add(htmlInternalLink.HeaderContent);
				pdfBookmark.Destination = new PdfDestination(lDoc.Pages[htmlInternalLink.DestinationPageNumber + TocPageCount - 1]);
				pdfBookmark.Destination.Location = new PointF(htmlInternalLink.Destination.X, htmlInternalLink.Destination.Y);
				num = num3;
				num2 = num3;
				array[index] = pdfBookmark;
				continue;
			}
			int num4 = 0;
			if (num3 > num2)
			{
				AddChildBookmark(index, lDoc, array, num4, internalLinkCollection);
				num2 = num3;
				continue;
			}
			do
			{
				num4++;
				htmlInternalLink = internalLinkCollection[index - num4];
			}
			while (htmlInternalLink.HeaderTagLevel == null || int.Parse(htmlInternalLink.HeaderTagLevel.Split('H')[1]) >= num3);
			num4--;
			AddChildBookmark(index, lDoc, array, num4, internalLinkCollection);
			num2 = num3;
		}
	}

	private void AddChildBookmark(int index, PdfDocument lDoc, PdfBookmark[] bookmarkCollection, int prevIndex, List<HtmlInternalLink> internalLinkCollection)
	{
		HtmlInternalLink htmlInternalLink = internalLinkCollection[index];
		PdfBookmark pdfBookmark = bookmarkCollection[index - (prevIndex + 1)].Add(htmlInternalLink.HeaderContent);
		pdfBookmark.Destination = new PdfDestination(lDoc.Pages[htmlInternalLink.DestinationPageNumber + TocPageCount - 1]);
		pdfBookmark.Destination.Location = new PointF(htmlInternalLink.Destination.X, htmlInternalLink.Destination.Y);
		bookmarkCollection[index] = pdfBookmark;
	}
}
