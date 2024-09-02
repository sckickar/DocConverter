using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

public class PdfDocumentPageCollection : IEnumerable
{
	private struct PdfPageEnumerator : IEnumerator
	{
		private PdfDocumentPageCollection m_pageCollection;

		private int m_currentIndex;

		public object Current
		{
			get
			{
				CheckIndex();
				return m_pageCollection[m_currentIndex];
			}
		}

		internal PdfPageEnumerator(PdfDocumentPageCollection pageCollection)
		{
			if (pageCollection == null)
			{
				throw new ArgumentNullException("pageCollection");
			}
			m_pageCollection = pageCollection;
			m_currentIndex = -1;
		}

		public bool MoveNext()
		{
			m_currentIndex++;
			return m_currentIndex < m_pageCollection.Count;
		}

		public void Reset()
		{
			m_currentIndex = -1;
		}

		private void CheckIndex()
		{
			if (m_currentIndex < 0 || m_currentIndex >= m_pageCollection.Count)
			{
				throw new IndexOutOfRangeException();
			}
		}
	}

	private PdfDocument m_document;

	private Dictionary<PdfPage, int> m_pageCollectionIndex = new Dictionary<PdfPage, int>();

	internal int count;

	public int Count => CountPages();

	public PdfPage this[int index] => GetPageByIndex(index);

	internal Dictionary<PdfPage, int> PageCollectionIndex => m_pageCollectionIndex;

	public event PageAddedEventHandler PageAdded;

	internal PdfDocumentPageCollection(PdfDocument document)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		m_document = document;
	}

	public PdfPage Add()
	{
		PdfPage pdfPage = new PdfPage();
		if (PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A4 || PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A4E || PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A4F || PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A3B || PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A3A || PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A3U || PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A2B || PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A2A || PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A2U)
		{
			PdfName key = new PdfName("Resources");
			pdfPage.Dictionary.Items.Add(key, new PdfDictionary());
		}
		pdfPage.IsNewPage = true;
		Add(pdfPage);
		pdfPage.IsNewPage = false;
		return pdfPage;
	}

	internal void Add(PdfPage page)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		PdfSection pdfSection = GetLastSection();
		if (GetLastSection().PageSettings.Orientation != m_document.PageSettings.Orientation)
		{
			pdfSection = m_document.Sections.Add();
			pdfSection.PageSettings.Orientation = m_document.PageSettings.Orientation;
		}
		if (!m_pageCollectionIndex.ContainsKey(page))
		{
			m_pageCollectionIndex.Add(page, count++);
		}
		pdfSection.Add(page);
	}

	public void Insert(int index, PdfPage page)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (index > Count)
		{
			throw new ArgumentOutOfRangeException("index", "Value can not be less 0, equal or more than number of pages in the document.");
		}
		if (page.m_section != null)
		{
			page.Section = null;
			for (int i = 0; i < m_document.Sections.Count; i++)
			{
				if (m_document.Sections[i].Pages.Contains(page))
				{
					m_document.Sections[i].Remove(page);
				}
			}
		}
		if (index == Count)
		{
			GetLastSection().Add(page);
			return;
		}
		int num = 0;
		int j = 0;
		for (int num2 = m_document.Sections.Count; j < num2; j++)
		{
			PdfSection pdfSection = m_document.Sections[j];
			for (int k = 0; k < pdfSection.Pages.Count; k++)
			{
				if (num == index)
				{
					pdfSection.Insert(k, page);
					return;
				}
				num++;
			}
		}
	}

	public void Insert(int index, PdfPageBase loadedPage)
	{
		if (loadedPage == null)
		{
			throw new ArgumentNullException("loadedPage");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", "Value cannot be less than zero");
		}
		if (index > Count)
		{
			throw new ArgumentOutOfRangeException("index", "Value can not be more than number of pages in the document.");
		}
		PdfDictionary dictionary = loadedPage.Dictionary;
		if (dictionary.ContainsKey("Parent"))
		{
			PdfDictionary pdfDictionary = PdfCrossTable.Dereference(dictionary["Parent"]) as PdfDictionary;
			if (pdfDictionary != null)
			{
				if (pdfDictionary.ContainsKey("MediaBox") && !dictionary.ContainsKey("MediaBox"))
				{
					dictionary.Items.Add((PdfName)"MediaBox", pdfDictionary["MediaBox"]);
				}
				if (pdfDictionary.ContainsKey("Rotate") && !dictionary.ContainsKey("Rotate"))
				{
					dictionary.Items.Add((PdfName)"Rotate", pdfDictionary["Rotate"]);
				}
			}
			if (!dictionary.ContainsKey("MediaBox") && PdfCrossTable.Dereference(pdfDictionary["Parent"]) is PdfDictionary pdfDictionary2 && pdfDictionary2["MediaBox"] != null)
			{
				dictionary.Items.Add((PdfName)"MediaBox", pdfDictionary2["MediaBox"]);
			}
		}
		if (dictionary.ContainsKey("Contents"))
		{
			PdfArray pdfArray = loadedPage.ReInitializeContentReference();
			if (pdfArray.Elements.Count > 0)
			{
				dictionary["Contents"] = pdfArray;
			}
		}
		if (dictionary.ContainsKey("Resources"))
		{
			PdfDictionary obj = loadedPage.ReinitializePageResources();
			if (dictionary["Resources"] as PdfReferenceHolder != null)
			{
				dictionary["Resources"] = new PdfReferenceHolder(obj);
			}
		}
		if (dictionary.ContainsKey("Annots"))
		{
			PdfCatalog catalog = (loadedPage as PdfLoadedPage).Document.Catalog;
			PdfDictionary pdfDictionary3 = null;
			if (catalog.ContainsKey("AcroForm"))
			{
				pdfDictionary3 = ((!(catalog["AcroForm"] as PdfReferenceHolder != null)) ? (catalog["AcroForm"] as PdfDictionary) : (PdfCrossTable.Dereference(catalog["AcroForm"]) as PdfDictionary));
			}
			loadedPage.ReInitializePageAnnotation(pdfDictionary3);
			if (pdfDictionary3 != null)
			{
				PdfCatalog catalog2 = m_document.Catalog;
				catalog2.Items.Add((PdfName)"AcroForm", new PdfReferenceHolder(pdfDictionary3));
				catalog2.Modify();
			}
		}
		if (dictionary.ContainsKey("Thumb"))
		{
			loadedPage.ReInitializeThumbnail();
		}
		int num = 0;
		int i = 0;
		for (int num2 = m_document.Sections.Count; i < num2; i++)
		{
			PdfSection pdfSection = m_document.Sections[i];
			for (int j = 0; j < pdfSection.Pages.Count; j++)
			{
				if (num == index)
				{
					pdfSection.Insert(j, loadedPage);
					return;
				}
				num++;
			}
		}
	}

	public int IndexOf(PdfPage page)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		int num = -1;
		int num2 = 0;
		int i = 0;
		for (int num3 = m_document.Sections.Count; i < num3; i++)
		{
			PdfSection pdfSection = m_document.Sections[i];
			num = pdfSection.IndexOf(page);
			if (num >= 0)
			{
				num += num2;
				break;
			}
			num2 += pdfSection.Count;
		}
		return num;
	}

	internal PdfSection Remove(PdfPage page)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		PdfSection pdfSection = null;
		int i = 0;
		for (int num = m_document.Sections.Count; i < num; i++)
		{
			pdfSection = m_document.Sections[i];
			if (pdfSection.Pages.Contains(page))
			{
				pdfSection.Pages.Remove(page);
				break;
			}
		}
		return pdfSection;
	}

	private void RemoveAndClearAllPages()
	{
		PdfSection pdfSection = null;
		int i = 0;
		for (int num = m_document.Sections.Count; i < num; i++)
		{
			pdfSection = m_document.Sections[i];
			for (int j = 0; j < pdfSection.Pages.Count; j++)
			{
				PdfPage pdfPage = pdfSection.Pages[j];
				if (pdfPage.Dictionary != null)
				{
					pdfSection.Pages.Remove(pdfPage);
				}
				pdfPage.Clear();
			}
		}
	}

	internal void Clear()
	{
		RemoveAndClearAllPages();
		m_pageCollectionIndex.Clear();
		m_pageCollectionIndex = null;
		m_document = null;
	}

	private int CountPages()
	{
		PdfSectionCollection sections = m_document.Sections;
		int num = 0;
		foreach (PdfSection item in sections)
		{
			num += item.Count;
		}
		return num;
	}

	private PdfPage GetPageByIndex(int index)
	{
		if (index < 0 || index >= Count)
		{
			throw new ArgumentOutOfRangeException("index", "Value can not be less 0, equal or more than number of pages in the document.");
		}
		PdfPage result = null;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int i = 0;
		for (int num4 = m_document.Sections.Count; i < num4; i++)
		{
			PdfSection pdfSection = m_document.Sections[i];
			num2 = pdfSection.Count;
			num3 = index - num;
			if (index >= num && num3 < num2)
			{
				result = pdfSection[num3];
				break;
			}
			num += num2;
		}
		return result;
	}

	private void Add(PdfLoadedPage page)
	{
		throw new NotImplementedException();
	}

	private PdfSection GetLastSection()
	{
		PdfSectionCollection sections = m_document.Sections;
		if (sections.Count == 0)
		{
			sections.Add();
		}
		return sections[sections.Count - 1];
	}

	internal void OnPageAdded(PageAddedEventArgs args)
	{
		if (this.PageAdded != null)
		{
			this.PageAdded(this, args);
		}
	}

	internal PdfPageBase Add(PdfLoadedDocument ldDoc, PdfPageBase page, List<PdfArray> destinations)
	{
		if (ldDoc == null)
		{
			throw new ArgumentNullException("ldDoc");
		}
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		PdfSection pdfSection;
		if (CanPageFitLastSection(page))
		{
			pdfSection = GetLastSection();
		}
		else
		{
			pdfSection = m_document.Sections.Add();
			PdfPageSettings pageSettings = pdfSection.PageSettings;
			pageSettings.Size = page.Size;
			pageSettings.Orientation = page.Orientation;
			if (m_document.PageSettings.Rotate != page.Rotation && page.Orientation != 0)
			{
				pageSettings.Rotate = m_document.PageSettings.Rotate;
			}
			else
			{
				pageSettings.Rotate = page.Rotation;
			}
			pageSettings.m_isRotation = false;
			if (!ldDoc.IsExtendMargin && m_document.PageSettings.Size != page.Size)
			{
				pageSettings.Margins.All = 0f;
			}
			pageSettings.Origin = page.Origin;
			pdfSection.m_importedSection = true;
		}
		PdfPage pdfPage = pdfSection.Add();
		m_pageCollectionIndex.Add(pdfPage, count++);
		if (PdfCrossTable.Dereference(page.Dictionary["CropBox"]) is PdfArray primitive)
		{
			pdfPage.Dictionary.SetProperty("CropBox", primitive);
		}
		_ = pdfPage.Size;
		if (page.Dictionary.ContainsKey("MediaBox") && page.Dictionary["MediaBox"] is PdfArray pdfArray)
		{
			pdfPage.Dictionary.SetProperty("MediaBox", pdfArray);
			float floatValue = (pdfArray[2] as PdfNumber).FloatValue;
			float floatValue2 = (pdfArray[3] as PdfNumber).FloatValue;
			new SizeF(floatValue, floatValue2);
		}
		PdfDictionary dictionary = page.Dictionary;
		if (!dictionary.ContainsKey("MediaBox") && dictionary.ContainsKey("Parent") && PdfCrossTable.Dereference(dictionary["Parent"]) is PdfDictionary pdfDictionary && pdfDictionary["MediaBox"] != null)
		{
			PdfArray primitive2 = pdfDictionary["MediaBox"] as PdfArray;
			pdfPage.Dictionary.SetProperty("MediaBox", primitive2);
		}
		PdfTemplate pdfTemplate = null;
		if (ldDoc.IsExtendMargin)
		{
			pdfTemplate = page.ContentTemplate;
		}
		if (pdfTemplate != null && pdfTemplate.m_content != null && pdfTemplate.m_content.Data.Length != 0)
		{
			pdfTemplate.isLoadedPageTemplate = m_document.EnableMemoryOptimization;
			pdfPage.Graphics.DrawPdfTemplate(pdfTemplate, PointF.Empty, pdfPage.GetClientSize());
			if (ldDoc.IsOptimizeIdentical)
			{
				pdfPage.repeatedReferenceCollection = new List<PdfReference>();
				PdfResources resources = pdfPage.GetResources();
				pdfPage.DestinationDocument = ldDoc.DestinationDocument;
				pdfPage.RemoveIdenticalResources(resources, pdfPage);
				pdfPage.repeatedReferenceCollection.Clear();
				pdfPage.repeatedReferenceCollection = null;
				pdfPage.Dictionary["Resources"] = resources;
				pdfPage.SetResources(resources);
			}
		}
		else if (page.Contents.Count > 0)
		{
			IPdfPrimitive pdfPrimitive = null;
			PdfResources pdfResources = null;
			foreach (IPdfPrimitive content in page.Contents)
			{
				pdfPrimitive = ((!m_document.EnableMemoryOptimization) ? content : content.Clone(m_document.CrossTable));
				pdfPage.Contents.Add(pdfPrimitive);
			}
			pdfResources = ((!m_document.EnableMemoryOptimization) ? page.GetResources() : new PdfResources(page.GetResources().Clone(m_document.CrossTable) as PdfDictionary));
			if (ldDoc.IsOptimizeIdentical)
			{
				pdfPage.repeatedReferenceCollection = new List<PdfReference>();
				pdfPage.DestinationDocument = ldDoc.DestinationDocument;
				pdfPage.RemoveIdenticalResources(pdfResources, pdfPage);
				pdfPage.RemoveIdeticalContentStreams(page.Contents, pdfPage);
				pdfPage.repeatedReferenceCollection.Clear();
				pdfPage.repeatedReferenceCollection = null;
			}
			pdfPage.Dictionary["Resources"] = pdfResources;
			pdfPage.SetResources(pdfResources);
		}
		if (!m_document.EnableMemoryOptimization)
		{
			pdfPage.ImportAnnotations(ldDoc, page, destinations);
		}
		if (ldDoc.IsOptimizeIdentical)
		{
			PdfArray annotArray = null;
			if (page.Dictionary.ContainsKey("Annots"))
			{
				annotArray = PdfCrossTable.Dereference(pdfPage.Dictionary["Annots"]) as PdfArray;
			}
			pdfPage.repeatedReferenceCollection = new List<PdfReference>();
			pdfPage.DestinationDocument = ldDoc.DestinationDocument;
			pdfPage.RemoveIdenticalAnnotations(annotArray, pdfPage);
			pdfPage.repeatedReferenceCollection.Clear();
			pdfPage.repeatedReferenceCollection = null;
		}
		return pdfPage;
	}

	private bool CanPageFitLastSection(PdfPageBase page)
	{
		return false;
	}

	public IEnumerator GetEnumerator()
	{
		return new PdfPageEnumerator(this);
	}
}
