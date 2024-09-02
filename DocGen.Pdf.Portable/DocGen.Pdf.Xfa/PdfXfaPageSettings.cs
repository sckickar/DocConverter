using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Xfa;

public class PdfXfaPageSettings
{
	private PdfXfaPageOrientation m_pageOrientation;

	private SizeF m_size = PdfPageSize.A4;

	private PdfMargins m_margins = new PdfMargins();

	public PdfXfaPageOrientation PageOrientation
	{
		get
		{
			return m_pageOrientation;
		}
		set
		{
			m_pageOrientation = value;
		}
	}

	public SizeF PageSize
	{
		get
		{
			return m_size;
		}
		set
		{
			if (value != SizeF.Empty)
			{
				m_size = value;
			}
		}
	}

	public PdfMargins Margins
	{
		get
		{
			return m_margins;
		}
		set
		{
			m_margins = value;
		}
	}
}
