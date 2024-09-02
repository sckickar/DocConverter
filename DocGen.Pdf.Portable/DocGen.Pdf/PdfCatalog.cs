using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;
using DocGen.Pdf.Xmp;

namespace DocGen.Pdf;

internal class PdfCatalog : PdfDictionary
{
	private PdfSectionCollection m_sections;

	private PdfAttachmentCollection m_attachment;

	private PdfViewerPreferences m_viewerPreferences;

	private PdfCatalogNames m_names;

	private XmpMetadata m_metadata;

	private PdfForm m_form;

	private PdfLoadedForm m_loadedForm;

	private PdfLoadedDocument m_loadedDocument;

	private PdfDictionary m_dictionary = new PdfDictionary();

	private PdfPortfolioInformation m_pdfPortfolio;

	private bool m_noNames;

	[ThreadStatic]
	internal static PdfStructTreeRoot m_structTreeRoot;

	public PdfViewerPreferences ViewerPreferences
	{
		get
		{
			return m_viewerPreferences;
		}
		set
		{
			if (m_viewerPreferences == value)
			{
				return;
			}
			m_viewerPreferences = value;
			if (base["ViewerPreferences"] != null && LoadedDocument != null)
			{
				m_dictionary = base["ViewerPreferences"] as PdfDictionary;
				PdfReferenceHolder primitive = base["ViewerPreferences"] as PdfReferenceHolder;
				base["ViewerPreferences"] = new PdfReferenceHolder(value);
				if (m_dictionary != null)
				{
					SetProperty("ViewerPreferences", new PdfDictionary(m_dictionary));
				}
				else
				{
					SetProperty("ViewerPreferences", primitive);
				}
			}
			else
			{
				base["ViewerPreferences"] = new PdfReferenceHolder(value);
			}
		}
	}

	internal PdfPortfolioInformation PdfPortfolio
	{
		get
		{
			return m_pdfPortfolio;
		}
		set
		{
			m_pdfPortfolio = value;
			base["Collection"] = new PdfReferenceHolder(m_pdfPortfolio);
		}
	}

	public static PdfStructTreeRoot StructTreeRoot => m_structTreeRoot;

	public PdfForm Form
	{
		get
		{
			return m_form;
		}
		set
		{
			if (m_form != value)
			{
				m_form = value;
				base["AcroForm"] = new PdfReferenceHolder(m_form);
			}
		}
	}

	public PdfCatalogNames Names
	{
		get
		{
			if (!m_noNames)
			{
				CreateNamesIfNone();
			}
			return m_names;
		}
	}

	internal PdfDictionary Destinations
	{
		get
		{
			PdfDictionary result = null;
			if (ContainsKey("Dests"))
			{
				result = PdfCrossTable.Dereference(base["Dests"]) as PdfDictionary;
			}
			return result;
		}
	}

	internal PdfLoadedForm LoadedForm
	{
		get
		{
			return m_loadedForm;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("LoadedForm");
			}
			m_loadedForm = value;
		}
	}

	internal PdfLoadedDocument LoadedDocument => m_loadedDocument;

	internal PdfSectionCollection Pages
	{
		get
		{
			return m_sections;
		}
		set
		{
			if (m_sections != value)
			{
				m_sections = value;
				base["Pages"] = new PdfReferenceHolder(value);
			}
		}
	}

	internal PdfAttachmentCollection Attachments
	{
		get
		{
			return m_attachment;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("LoadedForm");
			}
			m_attachment = value;
		}
	}

	internal XmpMetadata Metadata
	{
		get
		{
			return m_metadata;
		}
		set
		{
			if (value == null && !m_loadedDocument.ConformanceEnabled)
			{
				throw new ArgumentNullException("Metadata");
			}
			m_metadata = value;
		}
	}

	internal PdfCatalog()
	{
		base["Type"] = new PdfName("Catalog");
	}

	internal PdfCatalog(PdfLoadedDocument document, PdfDictionary catalog)
		: base(catalog)
	{
		m_loadedDocument = document;
		if (PdfCrossTable.Dereference(base["Names"]) is PdfDictionary root)
		{
			m_names = new PdfCatalogNames(root);
		}
		else
		{
			m_noNames = true;
		}
		ReadMetadata();
		FreezeChanges(this);
	}

	internal void CreateNamesIfNone()
	{
		if (m_names == null)
		{
			m_names = new PdfCatalogNames();
			PdfReferenceHolder value = new PdfReferenceHolder(m_names);
			base["Names"] = value;
		}
	}

	internal void InitializeStructTreeRoot()
	{
		if (!ContainsKey("StructTreeRoot") && m_structTreeRoot == null)
		{
			m_structTreeRoot = new PdfStructTreeRoot();
			base["StructTreeRoot"] = new PdfReferenceHolder(m_structTreeRoot);
		}
	}

	private void ReadMetadata()
	{
		if (!(PdfCrossTable.Dereference(base["Metadata"]) is PdfStream pdfStream))
		{
			return;
		}
		bool flag = false;
		if (pdfStream.ContainsKey("Filter"))
		{
			IPdfPrimitive pdfPrimitive = pdfStream["Filter"];
			if (pdfPrimitive is PdfReferenceHolder)
			{
				pdfPrimitive = (pdfPrimitive as PdfReferenceHolder).Object;
			}
			if (pdfPrimitive != null)
			{
				if (pdfPrimitive is PdfName)
				{
					if ((pdfPrimitive as PdfName).Value == "FlateDecode")
					{
						flag = true;
					}
				}
				else if (pdfPrimitive is PdfArray)
				{
					foreach (IPdfPrimitive item in pdfPrimitive as PdfArray)
					{
						if ((item as PdfName).Value == "FlateDecode")
						{
							flag = true;
						}
					}
				}
			}
		}
		byte[] buffer = pdfStream.Data;
		if (pdfStream.Compress || flag)
		{
			try
			{
				buffer = pdfStream.GetDecompressedData();
			}
			catch (Exception)
			{
			}
		}
		MemoryStream input = new MemoryStream(buffer);
		XDocument xDocument = new XDocument();
		try
		{
			xDocument = XDocument.Load(XmlReader.Create(input));
		}
		catch (XmlException)
		{
			buffer = pdfStream.GetDecompressedData();
			input = new MemoryStream(buffer);
			input.Position = 0L;
			try
			{
				xDocument = XDocument.Load(XmlReader.Create(input));
			}
			catch (XmlException)
			{
				return;
			}
		}
		m_metadata = new XmpMetadata(xDocument);
		LoadedDocument.DublinSchema = m_metadata.DublinCoreSchema;
		m_metadata.isLoadedDocument = true;
	}

	internal void ApplyPdfXConformance()
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		pdfDictionary["S"] = new PdfName("GTS_PDFX");
		pdfDictionary["OutputConditionIdentifier"] = new PdfString("CGATS TR 001");
		pdfDictionary["Info"] = new PdfString(string.Empty);
		pdfDictionary["OutputCondition"] = new PdfString("SWOP CGATS TR 001-1995");
		pdfDictionary["Type"] = new PdfName("OutputIntent");
		pdfDictionary["RegistryName"] = new PdfString("http://www.color.org");
		PdfArray pdfArray = new PdfArray();
		pdfArray.Insert(0, pdfDictionary);
		base["OutputIntents"] = pdfArray;
	}

	internal new void Clear()
	{
		if (m_names != null)
		{
			m_names.Clear();
			m_names = null;
		}
		if (m_viewerPreferences != null)
		{
			m_viewerPreferences = null;
			m_viewerPreferences = null;
		}
		if (m_attachment != null)
		{
			m_attachment.Clear();
			m_attachment = null;
		}
		if (m_structTreeRoot != null)
		{
			m_structTreeRoot.Clear();
			m_structTreeRoot = null;
		}
		m_form = null;
		m_loadedDocument = null;
		m_loadedForm = null;
		m_metadata = null;
		m_sections = null;
		base.Clear();
	}
}
