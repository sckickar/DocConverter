using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfGoToAction : PdfAction
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
			if (value != m_destination)
			{
				m_destination = value;
			}
		}
	}

	public PdfGoToAction(PdfDestination destination)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		m_destination = destination;
	}

	public PdfGoToAction(PdfPage page)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		m_destination = new PdfDestination(page);
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.BeginSave += Dictionary_BeginSave;
		base.Dictionary.SetProperty("S", new PdfName("GoTo"));
	}

	private void Dictionary_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		base.Dictionary.SetProperty("D", m_destination);
	}
}
