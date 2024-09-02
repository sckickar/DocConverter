using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfPageTemplateCollection : IEnumerable, IPdfWrapper
{
	private List<PdfPageTemplate> m_pageTemplatesCollection = new List<PdfPageTemplate>();

	private PdfDictionary m_dictionary = new PdfDictionary();

	private PdfCrossTable m_crossTable = new PdfCrossTable();

	private Dictionary<PdfPageBase, string> m_pages = new Dictionary<PdfPageBase, string>();

	private Dictionary<PdfPageBase, string> m_templates = new Dictionary<PdfPageBase, string>();

	private PdfArray m_namedPages = new PdfArray();

	private PdfArray m_namedTempates = new PdfArray();

	internal PdfDictionary Dictionary => m_dictionary;

	internal PdfCrossTable CrossTable => m_crossTable;

	public int Count => m_pageTemplatesCollection.Count;

	public PdfPageTemplate this[int index]
	{
		get
		{
			if (index < 0 || index > Count - 1)
			{
				throw new ArgumentOutOfRangeException("index", "Index is out of range.");
			}
			return m_pageTemplatesCollection[index];
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	internal PdfPageTemplateCollection()
	{
		Initialize();
	}

	internal PdfPageTemplateCollection(PdfDictionary dictionary, PdfCrossTable crossTable)
	{
		m_dictionary = dictionary;
		if (crossTable != null)
		{
			m_crossTable = crossTable;
		}
		PdfLoadedDocument pdfLoadedDocument = m_crossTable.Document as PdfLoadedDocument;
		if (Dictionary != null && Dictionary.ContainsKey("Pages") && PdfCrossTable.Dereference(Dictionary["Pages"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("Names") && PdfCrossTable.Dereference(pdfDictionary["Names"]) is PdfArray namedPages)
		{
			m_namedPages = namedPages;
			if (pdfLoadedDocument != null && m_namedPages.Count > 0)
			{
				ParsingExistingPageTemplates(pdfLoadedDocument.Pages, m_namedPages, isVisible: true);
			}
			m_namedPages.Clear();
		}
		if (Dictionary != null && Dictionary.ContainsKey("Templates") && PdfCrossTable.Dereference(Dictionary["Templates"]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("Names") && PdfCrossTable.Dereference(pdfDictionary2["Names"]) is PdfArray namedTempates)
		{
			m_namedTempates = namedTempates;
			if (pdfLoadedDocument != null && m_namedTempates.Count > 0)
			{
				ParsingExistingPageTemplates(pdfLoadedDocument.Pages, m_namedTempates, isVisible: false);
			}
			m_namedTempates.Clear();
		}
		CrossTable.Document.Catalog.BeginSave += Dictionary_BeginSave;
		CrossTable.Document.Catalog.Modify();
	}

	internal void Initialize()
	{
		Dictionary.BeginSave += Dictionary_BeginSave;
	}

	public void Add(PdfPageTemplate pdfPageTemplate)
	{
		if (pdfPageTemplate == null)
		{
			throw new ArgumentNullException("The PdfPageTemplate value can't be null");
		}
		m_pageTemplatesCollection.Add(pdfPageTemplate);
	}

	public bool Contains(PdfPageTemplate pdfPageTemplate)
	{
		if (pdfPageTemplate == null)
		{
			throw new ArgumentNullException("The PdfPageTemplate value can't be null");
		}
		return m_pageTemplatesCollection.Contains(pdfPageTemplate);
	}

	public void RemoveAt(int index)
	{
		if (index >= m_pageTemplatesCollection.Count)
		{
			throw new PdfException("The index value should not be greater than or equal to the count ");
		}
		m_pageTemplatesCollection.RemoveAt(index);
	}

	public void Remove(PdfPageTemplate pdfPageTemplate)
	{
		if (m_pageTemplatesCollection.Contains(pdfPageTemplate))
		{
			m_pageTemplatesCollection.Remove(pdfPageTemplate);
		}
	}

	public void Clear()
	{
		m_pageTemplatesCollection.Clear();
	}

	private void Dictionary_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		foreach (PdfPageTemplate item in m_pageTemplatesCollection)
		{
			if (item.IsVisible)
			{
				m_pages[item.PdfPageBase] = item.Name;
				continue;
			}
			m_templates[item.PdfPageBase] = item.Name;
			if (item.PdfPageBase is PdfLoadedPage { Document: not null } pdfLoadedPage)
			{
				_ = pdfLoadedPage.Document.PageCount;
			}
			else if (item.PdfPageBase is PdfPage { Document: not null } pdfPage)
			{
				_ = pdfPage.Document.PageCount;
			}
		}
		if (m_pages.Count > 0)
		{
			foreach (KeyValuePair<PdfPageBase, string> page in m_pages)
			{
				m_namedPages.Add(new PdfString(page.Value));
				m_namedPages.Add(new PdfReferenceHolder(page.Key));
			}
			PdfDictionary pdfDictionary = new PdfDictionary();
			pdfDictionary["Names"] = new PdfReferenceHolder(m_namedPages);
			Dictionary["Pages"] = new PdfReferenceHolder(pdfDictionary);
		}
		if (m_templates.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<PdfPageBase, string> template in m_templates)
		{
			m_namedTempates.Add(new PdfString(template.Value));
			m_namedTempates.Add(new PdfReferenceHolder(template.Key));
		}
		PdfDictionary pdfDictionary2 = new PdfDictionary();
		pdfDictionary2["Names"] = new PdfReferenceHolder(m_namedTempates);
		Dictionary["Templates"] = new PdfReferenceHolder(pdfDictionary2);
	}

	private void ParsingExistingPageTemplates(PdfLoadedPageCollection pageCollection, PdfArray pageTemplates, bool isVisible)
	{
		for (int i = 1; i <= pageTemplates.Count; i += 2)
		{
			if (!(PdfCrossTable.Dereference(pageTemplates[i]) is PdfDictionary dic))
			{
				continue;
			}
			PdfPageBase page = pageCollection.GetPage(dic);
			if (PdfCrossTable.Dereference(pageTemplates[i - 1]) is PdfString pdfString && page != null)
			{
				PdfPageTemplate pdfPageTemplate = new PdfPageTemplate();
				pdfPageTemplate.PdfPageBase = page;
				pdfPageTemplate.IsVisible = isVisible;
				pdfPageTemplate.Name = pdfString.Value;
				if (!m_pageTemplatesCollection.Contains(pdfPageTemplate))
				{
					m_pageTemplatesCollection.Add(pdfPageTemplate);
				}
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return m_pageTemplatesCollection.GetEnumerator();
	}
}
