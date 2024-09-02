using DocGen.Pdf.Interactive;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

public class PdfPortfolioInformation : IPdfWrapper
{
	private PdfCatalog m_catalog;

	private PdfDictionary m_dictionary = new PdfDictionary();

	private PdfPortfolioSchema m_Schema;

	private PdfPortfolioViewMode m_viewMode;

	private PdfAttachment m_startupDocument;

	public PdfPortfolioSchema Schema
	{
		get
		{
			return m_Schema;
		}
		set
		{
			m_Schema = value;
			m_dictionary.SetProperty("Schema", m_Schema);
		}
	}

	public PdfPortfolioViewMode ViewMode
	{
		get
		{
			return m_viewMode;
		}
		set
		{
			m_viewMode = value;
			if (m_viewMode == PdfPortfolioViewMode.Details)
			{
				m_dictionary.SetProperty("View", new PdfName("D"));
			}
			else if (m_viewMode == PdfPortfolioViewMode.Hidden)
			{
				m_dictionary.SetProperty("View", new PdfName("H"));
			}
			else if (m_viewMode == PdfPortfolioViewMode.Tile)
			{
				m_dictionary.SetProperty("View", new PdfName("T"));
			}
		}
	}

	public PdfAttachment StartupDocument
	{
		get
		{
			return m_startupDocument;
		}
		set
		{
			m_startupDocument = value;
			m_dictionary.SetProperty("D", new PdfString(m_startupDocument.FileName));
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	public PdfPortfolioInformation()
	{
		Initialize();
	}

	internal PdfPortfolioInformation(PdfDictionary portfolioDictionary)
	{
		if (portfolioDictionary == null)
		{
			return;
		}
		m_dictionary = portfolioDictionary;
		PdfDictionary pdfDictionary = null;
		if (m_dictionary["Schema"] is PdfDictionary)
		{
			pdfDictionary = m_dictionary["Schema"] as PdfDictionary;
		}
		else if (m_dictionary["Schema"] is PdfReferenceHolder)
		{
			pdfDictionary = (m_dictionary["Schema"] as PdfReferenceHolder).Object as PdfDictionary;
		}
		if (pdfDictionary != null)
		{
			m_Schema = new PdfPortfolioSchema(pdfDictionary);
		}
		PdfName pdfName = m_dictionary["View"] as PdfName;
		if (pdfName != null)
		{
			if (pdfName.Value.Equals("D"))
			{
				ViewMode = PdfPortfolioViewMode.Details;
			}
			else if (pdfName.Value.Equals("T"))
			{
				ViewMode = PdfPortfolioViewMode.Tile;
			}
			else if (pdfName.Value.Equals("H"))
			{
				ViewMode = PdfPortfolioViewMode.Hidden;
			}
		}
	}

	private void Initialize()
	{
		m_dictionary.SetProperty("Type", new PdfName("Collection"));
	}
}
