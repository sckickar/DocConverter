using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfNamedAction : PdfAction
{
	private PdfActionDestination m_destination = PdfActionDestination.NextPage;

	public PdfActionDestination Destination
	{
		get
		{
			return m_destination;
		}
		set
		{
			if (m_destination != value)
			{
				m_destination = value;
				base.Dictionary.SetName("N", m_destination.ToString());
			}
		}
	}

	public PdfNamedAction(PdfActionDestination destination)
	{
		Destination = destination;
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.SetProperty("S", new PdfName("Named"));
		base.Dictionary.SetProperty("N", new PdfName(m_destination.ToString()));
	}
}
