using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

public class PdfViewerPreferences : IPdfWrapper
{
	private bool m_centerWindow;

	private bool m_displayDocTitle;

	private bool m_fitWindow;

	private bool m_hideMenubar;

	private bool m_hideToolbar;

	private bool m_hideWindowUI;

	private PdfPageMode m_pageMode;

	private PdfPageLayout m_pageLayout;

	private PdfCatalog m_catalog;

	private PdfDictionary m_dictionary = new PdfDictionary();

	private PageScalingMode m_pageScaling;

	private DuplexMode m_duplex = DuplexMode.None;

	public bool CenterWindow
	{
		get
		{
			if (m_catalog.LoadedDocument != null)
			{
				m_dictionary = m_catalog;
				if (m_dictionary["ViewerPreferences"] is PdfReferenceHolder)
				{
					PdfDictionary pdfDictionary = (m_dictionary["ViewerPreferences"] as PdfReferenceHolder).Object as PdfDictionary;
					if (pdfDictionary.ContainsKey("CenterWindow"))
					{
						m_centerWindow = bool.Parse((pdfDictionary["CenterWindow"] as PdfBoolean).Value.ToString());
					}
				}
				else if (m_dictionary["ViewerPreferences"] is PdfDictionary)
				{
					PdfDictionary pdfDictionary2 = m_dictionary["ViewerPreferences"] as PdfDictionary;
					if (pdfDictionary2.ContainsKey("CenterWindow"))
					{
						m_centerWindow = bool.Parse((pdfDictionary2["CenterWindow"] as PdfBoolean).Value.ToString());
					}
				}
			}
			return m_centerWindow;
		}
		set
		{
			m_centerWindow = value;
			m_dictionary = m_catalog;
			if (m_dictionary["ViewerPreferences"] is PdfReferenceHolder)
			{
				((m_dictionary["ViewerPreferences"] as PdfReferenceHolder).Object as PdfDictionary).SetBoolean("CenterWindow", m_centerWindow);
			}
			else if (m_dictionary["ViewerPreferences"] is PdfDictionary)
			{
				(m_dictionary["ViewerPreferences"] as PdfDictionary).SetBoolean("CenterWindow", m_centerWindow);
			}
		}
	}

	public bool DisplayTitle
	{
		get
		{
			if (m_catalog.LoadedDocument != null)
			{
				m_dictionary = m_catalog;
				if (m_dictionary["ViewerPreferences"] is PdfReferenceHolder)
				{
					PdfDictionary pdfDictionary = (m_dictionary["ViewerPreferences"] as PdfReferenceHolder).Object as PdfDictionary;
					if (pdfDictionary.ContainsKey("DisplayDocTitle"))
					{
						m_displayDocTitle = bool.Parse((pdfDictionary["DisplayDocTitle"] as PdfBoolean).Value.ToString());
					}
				}
				else if (m_dictionary["ViewerPreferences"] is PdfDictionary)
				{
					PdfDictionary pdfDictionary2 = m_dictionary["ViewerPreferences"] as PdfDictionary;
					if (pdfDictionary2.ContainsKey("DisplayDocTitle"))
					{
						m_displayDocTitle = bool.Parse((pdfDictionary2["DisplayDocTitle"] as PdfBoolean).Value.ToString());
					}
				}
			}
			return m_displayDocTitle;
		}
		set
		{
			m_displayDocTitle = value;
			m_dictionary = m_catalog;
			if (m_dictionary["ViewerPreferences"] is PdfReferenceHolder)
			{
				((m_dictionary["ViewerPreferences"] as PdfReferenceHolder).Object as PdfDictionary).SetBoolean("DisplayDocTitle", m_displayDocTitle);
			}
			else if (m_dictionary["ViewerPreferences"] is PdfDictionary)
			{
				(m_dictionary["ViewerPreferences"] as PdfDictionary).SetBoolean("DisplayDocTitle", m_displayDocTitle);
			}
		}
	}

	public bool FitWindow
	{
		get
		{
			if (m_catalog.LoadedDocument != null)
			{
				m_dictionary = m_catalog;
				if (m_dictionary["ViewerPreferences"] is PdfReferenceHolder)
				{
					PdfDictionary pdfDictionary = (m_dictionary["ViewerPreferences"] as PdfReferenceHolder).Object as PdfDictionary;
					if (pdfDictionary.ContainsKey("FitWindow"))
					{
						m_fitWindow = bool.Parse((pdfDictionary["FitWindow"] as PdfBoolean).Value.ToString());
					}
				}
				else if (m_dictionary["ViewerPreferences"] is PdfDictionary)
				{
					PdfDictionary pdfDictionary2 = m_dictionary["ViewerPreferences"] as PdfDictionary;
					if (pdfDictionary2.ContainsKey("FitWindow"))
					{
						m_fitWindow = bool.Parse((pdfDictionary2["FitWindow"] as PdfBoolean).Value.ToString());
					}
				}
			}
			return m_fitWindow;
		}
		set
		{
			m_fitWindow = value;
			m_dictionary = m_catalog;
			if (m_dictionary["ViewerPreferences"] is PdfReferenceHolder)
			{
				((m_dictionary["ViewerPreferences"] as PdfReferenceHolder).Object as PdfDictionary).SetBoolean("FitWindow", m_fitWindow);
			}
			else if (m_dictionary["ViewerPreferences"] is PdfDictionary)
			{
				(m_dictionary["ViewerPreferences"] as PdfDictionary).SetBoolean("FitWindow", m_fitWindow);
			}
		}
	}

	public bool HideMenubar
	{
		get
		{
			if (m_catalog.LoadedDocument != null)
			{
				m_dictionary = m_catalog;
				if (m_dictionary["ViewerPreferences"] is PdfReferenceHolder)
				{
					PdfDictionary pdfDictionary = (m_dictionary["ViewerPreferences"] as PdfReferenceHolder).Object as PdfDictionary;
					if (pdfDictionary.ContainsKey("HideMenubar"))
					{
						m_hideMenubar = bool.Parse((pdfDictionary["HideMenubar"] as PdfBoolean).Value.ToString());
					}
				}
				else if (m_dictionary["ViewerPreferences"] is PdfDictionary)
				{
					PdfDictionary pdfDictionary2 = m_dictionary["ViewerPreferences"] as PdfDictionary;
					if (pdfDictionary2.ContainsKey("HideMenubar"))
					{
						m_hideMenubar = bool.Parse((pdfDictionary2["HideMenubar"] as PdfBoolean).Value.ToString());
					}
				}
			}
			return m_hideMenubar;
		}
		set
		{
			m_hideMenubar = value;
			m_dictionary = m_catalog;
			if (m_dictionary["ViewerPreferences"] is PdfReferenceHolder)
			{
				((m_dictionary["ViewerPreferences"] as PdfReferenceHolder).Object as PdfDictionary).SetBoolean("HideMenubar", m_hideMenubar);
			}
			else if (m_dictionary["ViewerPreferences"] is PdfDictionary)
			{
				(m_dictionary["ViewerPreferences"] as PdfDictionary).SetBoolean("HideMenubar", m_hideMenubar);
			}
		}
	}

	public bool HideToolbar
	{
		get
		{
			if (m_catalog.LoadedDocument != null)
			{
				m_dictionary = m_catalog;
				if (m_dictionary["ViewerPreferences"] is PdfReferenceHolder)
				{
					PdfDictionary pdfDictionary = (m_dictionary["ViewerPreferences"] as PdfReferenceHolder).Object as PdfDictionary;
					if (pdfDictionary.ContainsKey("HideToolbar"))
					{
						m_hideToolbar = bool.Parse((pdfDictionary["HideToolbar"] as PdfBoolean).Value.ToString());
					}
				}
				else if (m_dictionary["ViewerPreferences"] is PdfDictionary)
				{
					PdfDictionary pdfDictionary2 = m_dictionary["ViewerPreferences"] as PdfDictionary;
					if (pdfDictionary2.ContainsKey("HideToolbar"))
					{
						m_hideToolbar = bool.Parse((pdfDictionary2["HideToolbar"] as PdfBoolean).Value.ToString());
					}
				}
			}
			return m_hideToolbar;
		}
		set
		{
			m_hideToolbar = value;
			m_dictionary = m_catalog;
			if (m_dictionary["ViewerPreferences"] is PdfReferenceHolder)
			{
				((m_dictionary["ViewerPreferences"] as PdfReferenceHolder).Object as PdfDictionary).SetBoolean("HideToolbar", m_hideToolbar);
			}
			else if (m_dictionary["ViewerPreferences"] is PdfDictionary)
			{
				(m_dictionary["ViewerPreferences"] as PdfDictionary).SetBoolean("HideToolbar", m_hideToolbar);
			}
		}
	}

	public bool HideWindowUI
	{
		get
		{
			if (m_catalog.LoadedDocument != null)
			{
				m_dictionary = m_catalog;
				if (m_dictionary["ViewerPreferences"] is PdfReferenceHolder)
				{
					PdfDictionary pdfDictionary = (m_dictionary["ViewerPreferences"] as PdfReferenceHolder).Object as PdfDictionary;
					if (pdfDictionary.ContainsKey("HideWindowUI"))
					{
						m_hideWindowUI = bool.Parse((pdfDictionary["HideWindowUI"] as PdfBoolean).Value.ToString());
					}
				}
				else if (m_dictionary["ViewerPreferences"] is PdfDictionary)
				{
					PdfDictionary pdfDictionary2 = m_dictionary["ViewerPreferences"] as PdfDictionary;
					if (pdfDictionary2.ContainsKey("HideWindowUI"))
					{
						m_hideWindowUI = bool.Parse((pdfDictionary2["HideWindowUI"] as PdfBoolean).Value.ToString());
					}
				}
			}
			return m_hideWindowUI;
		}
		set
		{
			m_hideWindowUI = value;
			m_dictionary = m_catalog;
			if (m_dictionary["ViewerPreferences"] is PdfReferenceHolder)
			{
				((m_dictionary["ViewerPreferences"] as PdfReferenceHolder).Object as PdfDictionary).SetBoolean("HideWindowUI", m_hideWindowUI);
			}
			else if (m_dictionary["ViewerPreferences"] is PdfDictionary)
			{
				(m_dictionary["ViewerPreferences"] as PdfDictionary).SetBoolean("HideWindowUI", m_hideWindowUI);
			}
		}
	}

	public PdfPageMode PageMode
	{
		get
		{
			if (m_catalog.LoadedDocument != null)
			{
				m_dictionary = m_catalog;
				if (m_dictionary["PageMode"] != null)
				{
					PdfName pdfName = m_dictionary["PageMode"] as PdfName;
					m_pageMode = (PdfPageMode)Enum.Parse(typeof(PdfPageMode), pdfName.Value, ignoreCase: true);
				}
			}
			return m_pageMode;
		}
		set
		{
			m_pageMode = value;
			PdfDictionary.SetName(m_catalog, "PageMode", m_pageMode.ToString());
		}
	}

	public PdfPageLayout PageLayout
	{
		get
		{
			if (m_catalog.LoadedDocument != null)
			{
				m_dictionary = m_catalog;
				if (m_dictionary.ContainsKey("PageLayout"))
				{
					if (Enum.IsDefined(typeof(PdfPageLayout), (m_dictionary["PageLayout"] as PdfName).Value.ToString()))
					{
						m_pageLayout = (PdfPageLayout)Enum.Parse(typeof(PdfPageLayout), (m_dictionary["PageLayout"] as PdfName).Value.ToString(), ignoreCase: true);
					}
					else
					{
						m_pageLayout = PdfPageLayout.SinglePage;
					}
				}
			}
			return m_pageLayout;
		}
		set
		{
			m_pageLayout = value;
			PdfDictionary.SetName(m_catalog, "PageLayout", m_pageLayout.ToString());
		}
	}

	public DuplexMode Duplex
	{
		get
		{
			if (m_catalog.LoadedDocument != null)
			{
				m_dictionary = m_catalog;
				if (m_dictionary.ContainsKey("Duplex"))
				{
					if (Enum.IsDefined(typeof(DuplexMode), (m_dictionary["Duplex"] as PdfName).Value.ToString()))
					{
						m_duplex = (DuplexMode)Enum.Parse(typeof(DuplexMode), (m_dictionary["Duplex"] as PdfName).Value.ToString(), ignoreCase: true);
					}
					else
					{
						m_duplex = DuplexMode.None;
					}
				}
			}
			return m_duplex;
		}
		set
		{
			m_duplex = value;
			PdfDictionary.SetName(m_catalog, "Duplex", m_duplex.ToString());
		}
	}

	public PageScalingMode PageScaling
	{
		get
		{
			if (m_catalog.LoadedDocument != null)
			{
				m_dictionary = m_catalog;
				if (m_dictionary["ViewerPreferences"] is PdfReferenceHolder)
				{
					PdfDictionary pdfDictionary = (m_dictionary["ViewerPreferences"] as PdfReferenceHolder).Object as PdfDictionary;
					if (pdfDictionary.ContainsKey("PrintScaling"))
					{
						m_pageScaling = (PageScalingMode)Enum.Parse(typeof(PageScalingMode), (pdfDictionary["PrintScaling"] as PdfName).Value.ToString(), ignoreCase: true);
					}
				}
				else if (m_dictionary["ViewerPreferences"] is PdfDictionary)
				{
					PdfDictionary pdfDictionary2 = m_dictionary["ViewerPreferences"] as PdfDictionary;
					if (pdfDictionary2.ContainsKey("PrintScaling"))
					{
						m_pageScaling = (PageScalingMode)Enum.Parse(typeof(PageScalingMode), (pdfDictionary2["PrintScaling"] as PdfName).Value.ToString(), ignoreCase: true);
					}
				}
			}
			return m_pageScaling;
		}
		set
		{
			m_pageScaling = value;
			m_dictionary = m_catalog;
			if (m_dictionary["ViewerPreferences"] is PdfReferenceHolder)
			{
				PdfDictionary pdfDictionary = (m_dictionary["ViewerPreferences"] as PdfReferenceHolder).Object as PdfDictionary;
				if (m_pageScaling != 0)
				{
					pdfDictionary.SetName("PrintScaling", m_pageScaling.ToString());
				}
				else if (pdfDictionary.ContainsKey("PrintScaling"))
				{
					pdfDictionary.Remove("PrintScaling");
				}
			}
			else if (m_dictionary["ViewerPreferences"] is PdfDictionary)
			{
				PdfDictionary pdfDictionary2 = m_dictionary["ViewerPreferences"] as PdfDictionary;
				if (m_pageScaling != 0)
				{
					pdfDictionary2.SetName("PrintScaling", m_pageScaling.ToString());
				}
				else if (pdfDictionary2.ContainsKey("PrintScaling"))
				{
					pdfDictionary2.Remove("PrintScaling");
				}
			}
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	internal PdfViewerPreferences()
	{
	}

	internal PdfViewerPreferences(PdfCatalog catalog)
	{
		if (catalog == null)
		{
			throw new ArgumentNullException("catalog");
		}
		m_catalog = catalog;
	}
}
