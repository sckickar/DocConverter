using System;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Lists;

public class PdfOrderedList : PdfList
{
	private PdfOrderedMarker m_marker;

	private bool m_useHierarchy;

	public PdfOrderedMarker Marker
	{
		get
		{
			return m_marker;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("marker");
			}
			m_marker = value;
		}
	}

	public bool MarkerHierarchy
	{
		get
		{
			return m_useHierarchy;
		}
		set
		{
			m_useHierarchy = value;
		}
	}

	public PdfOrderedList()
		: this(CreateMarker(PdfNumberStyle.Numeric))
	{
	}

	public PdfOrderedList(PdfFont font)
		: base(font)
	{
		CreateMarker(PdfNumberStyle.Numeric);
	}

	public PdfOrderedList(PdfNumberStyle style)
	{
		Marker = CreateMarker(style);
	}

	public PdfOrderedList(PdfListItemCollection items)
		: this(items, CreateMarker(PdfNumberStyle.Numeric))
	{
	}

	public PdfOrderedList(PdfOrderedMarker marker)
	{
		Marker = marker;
	}

	public PdfOrderedList(PdfListItemCollection items, PdfOrderedMarker marker)
		: base(items)
	{
		Marker = marker;
	}

	public PdfOrderedList(string text)
		: this(text, CreateMarker(PdfNumberStyle.Numeric))
	{
	}

	public PdfOrderedList(string text, PdfOrderedMarker marker)
		: this(PdfList.CreateItems(text), marker)
	{
	}

	private static PdfOrderedMarker CreateMarker(PdfNumberStyle style)
	{
		return new PdfOrderedMarker(style, null);
	}
}
