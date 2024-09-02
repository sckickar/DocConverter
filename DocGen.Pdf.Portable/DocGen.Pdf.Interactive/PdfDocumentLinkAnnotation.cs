using System;
using DocGen.Drawing;

namespace DocGen.Pdf.Interactive;

public class PdfDocumentLinkAnnotation : PdfLinkAnnotation
{
	private PdfDestination m_destination;

	public PdfDestination Destination
	{
		get
		{
			return m_destination;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Destination");
			}
			if (m_destination != value)
			{
				m_destination = value;
			}
		}
	}

	public PdfDocumentLinkAnnotation(RectangleF rectangle)
		: base(rectangle)
	{
	}

	public PdfDocumentLinkAnnotation(RectangleF rectangle, PdfDestination destination)
		: base(rectangle)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		Destination = destination;
	}

	protected override void Save()
	{
		base.Save();
		if (m_destination != null)
		{
			base.Dictionary.SetProperty("Dest", m_destination);
		}
	}
}
