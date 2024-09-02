using System;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Lists;

public class PdfUnorderedList : PdfList
{
	private PdfUnorderedMarker m_marker;

	public PdfUnorderedMarker Marker
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

	public PdfUnorderedList()
		: this(CreateMarker(PdfUnorderedMarkerStyle.Disk))
	{
	}

	public PdfUnorderedList(PdfListItemCollection items)
		: this(items, CreateMarker(PdfUnorderedMarkerStyle.Disk))
	{
	}

	public PdfUnorderedList(PdfFont font)
		: base(font)
	{
		CreateMarker(PdfUnorderedMarkerStyle.Disk);
	}

	public PdfUnorderedList(PdfUnorderedMarker marker)
	{
		Marker = marker;
	}

	public PdfUnorderedList(PdfListItemCollection items, PdfUnorderedMarker marker)
		: base(items)
	{
		Marker = marker;
	}

	public PdfUnorderedList(string text)
		: this(text, CreateMarker(PdfUnorderedMarkerStyle.Disk))
	{
	}

	public PdfUnorderedList(string text, PdfUnorderedMarker marker)
		: this(PdfList.CreateItems(text), marker)
	{
	}

	private static PdfUnorderedMarker CreateMarker(PdfUnorderedMarkerStyle style)
	{
		return new PdfUnorderedMarker(style);
	}
}
