using System;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfDocumentActions : IPdfWrapper
{
	private PdfDictionary m_dictionary = new PdfDictionary();

	private PdfAction m_afterOpen;

	private PdfAction m_beforeClose;

	private PdfJavaScriptAction m_beforeSave;

	private PdfJavaScriptAction m_afterSave;

	private PdfJavaScriptAction m_beforePrint;

	private PdfJavaScriptAction m_afterPrint;

	private PdfCatalog m_catalog;

	public PdfAction AfterOpen
	{
		get
		{
			return GetAfterOpenDictionary();
		}
		set
		{
			if (value == null)
			{
				m_afterOpen = value;
				if (m_catalog.ContainsKey("OpenAction"))
				{
					m_catalog.Remove("OpenAction");
				}
				RemoveJavaScriptAction();
			}
			else if (value != GetAfterOpenDictionary())
			{
				m_afterOpen = value;
				PdfDictionary.SetProperty(m_catalog, "OpenAction", m_afterOpen);
			}
		}
	}

	public PdfJavaScriptAction BeforeClose
	{
		get
		{
			return GetBeforeCloseDictionary();
		}
		set
		{
			if (value != GetBeforeCloseDictionary())
			{
				m_beforeClose = value;
				if (PdfCrossTable.Dereference(m_catalog["AA"]) is PdfDictionary pdfDictionary)
				{
					PdfCrossTable.Dereference(pdfDictionary["WC"]);
					PdfDictionary.SetProperty(pdfDictionary, "WC", m_beforeClose);
				}
				else
				{
					m_dictionary.SetProperty("WC", m_beforeClose);
				}
			}
		}
	}

	public PdfJavaScriptAction BeforeSave
	{
		get
		{
			return GetBeforeSaveDictionary();
		}
		set
		{
			if (value != GetBeforeSaveDictionary())
			{
				m_beforeSave = value;
				if (PdfCrossTable.Dereference(m_catalog["AA"]) is PdfDictionary pdfDictionary)
				{
					PdfCrossTable.Dereference(pdfDictionary["WS"]);
					PdfDictionary.SetProperty(pdfDictionary, "WS", m_beforeSave);
				}
				else
				{
					m_dictionary.SetProperty("WS", m_beforeSave);
				}
			}
		}
	}

	public PdfJavaScriptAction AfterSave
	{
		get
		{
			return GetAfterSaveDictionary();
		}
		set
		{
			if (value != GetAfterSaveDictionary())
			{
				m_afterSave = value;
				if (PdfCrossTable.Dereference(m_catalog["AA"]) is PdfDictionary pdfDictionary)
				{
					PdfCrossTable.Dereference(pdfDictionary["DS"]);
					PdfDictionary.SetProperty(pdfDictionary, "DS", m_afterSave);
				}
				else
				{
					m_dictionary.SetProperty("DS", m_afterSave);
				}
			}
		}
	}

	public PdfJavaScriptAction BeforePrint
	{
		get
		{
			return GetBeforePrintDictionary();
		}
		set
		{
			if (value != GetBeforePrintDictionary())
			{
				m_beforePrint = value;
				if (PdfCrossTable.Dereference(m_catalog["AA"]) is PdfDictionary pdfDictionary)
				{
					PdfCrossTable.Dereference(pdfDictionary["WP"]);
					PdfDictionary.SetProperty(pdfDictionary, "WP", m_beforePrint);
				}
				else
				{
					m_dictionary.SetProperty("WP", m_beforePrint);
				}
			}
		}
	}

	public PdfJavaScriptAction AfterPrint
	{
		get
		{
			return GetAfterPrintDictionary();
		}
		set
		{
			if (value != GetAfterPrintDictionary())
			{
				m_afterPrint = value;
				if (PdfCrossTable.Dereference(m_catalog["AA"]) is PdfDictionary pdfDictionary)
				{
					PdfCrossTable.Dereference(pdfDictionary["DP"]);
					PdfDictionary.SetProperty(pdfDictionary, "DP", m_afterPrint);
				}
				else
				{
					m_dictionary.SetProperty("DP", m_afterPrint);
				}
			}
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	internal PdfDocumentActions(PdfCatalog catalog)
	{
		if (catalog == null)
		{
			throw new ArgumentNullException("catalog");
		}
		if (PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A1B || PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A1A)
		{
			throw new PdfConformanceException("Usage of Javascript are not allowed by the PDF/A1-B or PDF/A1-A standard");
		}
		m_catalog = catalog;
	}

	private PdfJavaScriptAction GetAfterOpenDictionary()
	{
		PdfDictionary pdfDictionary = PdfCrossTable.Dereference(m_catalog["OpenAction"]) as PdfDictionary;
		string empty = string.Empty;
		if (pdfDictionary != null && pdfDictionary.ContainsKey("JS") && PdfCrossTable.Dereference(pdfDictionary["JS"]) is PdfString pdfString)
		{
			empty = pdfString.Value.ToString();
			m_afterOpen = new PdfJavaScriptAction(empty);
		}
		return m_afterOpen as PdfJavaScriptAction;
	}

	private void RemoveJavaScriptAction()
	{
		if (m_catalog.ContainsKey("Names") && PdfCrossTable.Dereference(m_catalog["Names"]) is PdfDictionary pdfDictionary)
		{
			if (pdfDictionary.ContainsKey("JavaScript"))
			{
				pdfDictionary.Remove("JavaScript");
			}
			else if (pdfDictionary.ContainsKey("JS"))
			{
				pdfDictionary.Remove("JS");
			}
		}
	}

	private PdfJavaScriptAction GetBeforeCloseDictionary()
	{
		PdfDictionary pdfDictionary = PdfCrossTable.Dereference(m_catalog["AA"]) as PdfDictionary;
		string empty = string.Empty;
		if (pdfDictionary != null && pdfDictionary.ContainsKey("WC") && PdfCrossTable.Dereference(pdfDictionary["WC"]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("Type") && pdfDictionary2.ContainsKey("JS"))
		{
			empty = (PdfCrossTable.Dereference(pdfDictionary2["JS"]) as PdfString).Value.ToString();
			m_beforeClose = new PdfJavaScriptAction(empty);
		}
		return m_beforeClose as PdfJavaScriptAction;
	}

	private PdfJavaScriptAction GetBeforeSaveDictionary()
	{
		PdfDictionary pdfDictionary = PdfCrossTable.Dereference(m_catalog["AA"]) as PdfDictionary;
		string empty = string.Empty;
		if (pdfDictionary != null && pdfDictionary.ContainsKey("WS") && PdfCrossTable.Dereference(pdfDictionary["WS"]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("Type") && pdfDictionary2.ContainsKey("JS"))
		{
			empty = (PdfCrossTable.Dereference(pdfDictionary2["JS"]) as PdfString).Value.ToString();
			m_beforeSave = new PdfJavaScriptAction(empty);
		}
		return m_beforeSave;
	}

	private PdfJavaScriptAction GetAfterSaveDictionary()
	{
		PdfDictionary pdfDictionary = PdfCrossTable.Dereference(m_catalog["AA"]) as PdfDictionary;
		string empty = string.Empty;
		if (pdfDictionary != null && pdfDictionary.ContainsKey("DS") && PdfCrossTable.Dereference(pdfDictionary["DS"]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("Type") && pdfDictionary2.ContainsKey("JS"))
		{
			empty = (PdfCrossTable.Dereference(pdfDictionary2["JS"]) as PdfString).Value.ToString();
			m_afterSave = new PdfJavaScriptAction(empty);
		}
		return m_afterSave;
	}

	private PdfJavaScriptAction GetBeforePrintDictionary()
	{
		PdfDictionary pdfDictionary = PdfCrossTable.Dereference(m_catalog["AA"]) as PdfDictionary;
		string empty = string.Empty;
		if (pdfDictionary != null && pdfDictionary.ContainsKey("WP") && PdfCrossTable.Dereference(pdfDictionary["WP"]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("Type") && pdfDictionary2.ContainsKey("JS"))
		{
			empty = (PdfCrossTable.Dereference(pdfDictionary2["JS"]) as PdfString).Value.ToString();
			m_beforePrint = new PdfJavaScriptAction(empty);
		}
		return m_beforePrint;
	}

	private PdfJavaScriptAction GetAfterPrintDictionary()
	{
		PdfDictionary pdfDictionary = PdfCrossTable.Dereference(m_catalog["AA"]) as PdfDictionary;
		string empty = string.Empty;
		if (pdfDictionary != null && pdfDictionary.ContainsKey("DP") && PdfCrossTable.Dereference(pdfDictionary["DP"]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("Type") && pdfDictionary2.ContainsKey("JS"))
		{
			empty = (PdfCrossTable.Dereference(pdfDictionary2["JS"]) as PdfString).Value.ToString();
			m_afterPrint = new PdfJavaScriptAction(empty);
		}
		return m_afterPrint;
	}
}
