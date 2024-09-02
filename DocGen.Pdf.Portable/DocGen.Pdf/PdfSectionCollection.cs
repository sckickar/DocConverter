using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

public class PdfSectionCollection : IPdfWrapper, IEnumerable
{
	private struct PdfSectionEnumerator : IEnumerator
	{
		private PdfSectionCollection m_sectionCollection;

		private int m_currentIndex;

		public object Current
		{
			get
			{
				CheckIndex();
				return m_sectionCollection[m_currentIndex];
			}
		}

		internal PdfSectionEnumerator(PdfSectionCollection sectionCollection)
		{
			if (sectionCollection == null)
			{
				throw new ArgumentNullException("sectionCollection");
			}
			m_sectionCollection = sectionCollection;
			m_currentIndex = -1;
		}

		public bool MoveNext()
		{
			m_currentIndex++;
			return m_currentIndex < m_sectionCollection.Count;
		}

		public void Reset()
		{
			m_currentIndex = -1;
		}

		private void CheckIndex()
		{
			if (m_currentIndex < 0 || m_currentIndex >= m_sectionCollection.Count)
			{
				throw new IndexOutOfRangeException();
			}
		}
	}

	internal const int RotateFactor = 90;

	private PdfArray m_sectionCollection;

	private List<PdfSection> m_sections = new List<PdfSection>();

	private PdfDictionary m_pages;

	private PdfNumber m_count;

	private PdfDocument m_document;

	public PdfSection this[int index]
	{
		get
		{
			if (index < 0 || index >= Count)
			{
				throw new IndexOutOfRangeException();
			}
			return m_sections[index];
		}
	}

	public int Count => m_sections.Count;

	internal PdfDocument Document => m_document;

	IPdfPrimitive IPdfWrapper.Element => m_pages;

	internal PdfSectionCollection(PdfDocument document)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		m_document = document;
		Initialize();
	}

	public PdfSection Add()
	{
		PdfSection pdfSection = new PdfSection(m_document);
		AddSection(pdfSection);
		return pdfSection;
	}

	public int IndexOf(PdfSection section)
	{
		PdfReferenceHolder element = new PdfReferenceHolder(section);
		return m_sectionCollection.IndexOf(element);
	}

	public void Insert(int index, PdfSection section)
	{
		if (index < 0 || index >= Count)
		{
			throw new IndexOutOfRangeException();
		}
		PdfReferenceHolder element = CheckSection(section);
		m_sectionCollection.Insert(index, element);
		if (!m_sections.Contains(section))
		{
			m_sections.Add(section);
		}
	}

	public bool Contains(PdfSection section)
	{
		if (section == null)
		{
			throw new ArgumentNullException("section");
		}
		return IndexOf(section) >= 0;
	}

	public IEnumerator GetEnumerator()
	{
		return new PdfSectionEnumerator(this);
	}

	internal void PageLabelsSet()
	{
		Document.PageLabelsSet();
	}

	internal void ResetProgress()
	{
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				((PdfSection)enumerator.Current).ResetProgress();
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
	}

	internal void SetProgress()
	{
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				((PdfSection)enumerator.Current).SetProgress();
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
	}

	internal void OnPageSaving(PdfPage page)
	{
		Document.OnPageSave(page);
	}

	private void SetPageSettings(PdfDictionary container, PdfPageSettings pageSettings)
	{
		if (container == null)
		{
			throw new ArgumentNullException("container");
		}
		if (pageSettings == null)
		{
			throw new ArgumentNullException("pageSettings");
		}
		RectangleF rectangle = new RectangleF(PointF.Empty, pageSettings.Size);
		if (PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_X1A2001)
		{
			container["MediaBox"] = PdfArray.FromRectangle(rectangle);
		}
		_ = pageSettings.Rotate;
		PdfNumber value = new PdfNumber(90 * (int)pageSettings.Rotate);
		container["Rotate"] = value;
		if (pageSettings.Unit != PdfGraphicsUnit.Point)
		{
			float value2 = new PdfUnitConvertor().ConvertUnits(1f, pageSettings.Unit, PdfGraphicsUnit.Point);
			container["UserUnit"] = new PdfNumber(value2);
		}
	}

	private PdfReferenceHolder CheckSection(PdfSection section)
	{
		PdfReferenceHolder pdfReferenceHolder = new PdfReferenceHolder(section);
		bool num = m_sectionCollection.Contains(pdfReferenceHolder);
		if (section.Parent != null && section.Parent != this)
		{
			section.Parent = this;
		}
		if (num)
		{
			throw new ArgumentException("The object can't be added twice to the collection.", "section");
		}
		return pdfReferenceHolder;
	}

	private int CountPages()
	{
		int num = 0;
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				PdfSection pdfSection = (PdfSection)enumerator.Current;
				num += pdfSection.Count;
			}
			return num;
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
	}

	private int Add(PdfSection section)
	{
		if (section == null)
		{
			throw new ArgumentNullException("section");
		}
		PdfReferenceHolder pdfReferenceHolder = null;
		pdfReferenceHolder = ((!m_document.IsMergeDocHasSections) ? new PdfReferenceHolder(section) : CheckSection(section));
		m_sections.Add(section);
		section.Parent = this;
		m_sectionCollection.Add(pdfReferenceHolder);
		return m_sections.IndexOf(section);
	}

	private void AddSection(PdfSection section)
	{
		if (section == null)
		{
			throw new ArgumentNullException("section");
		}
		PdfReferenceHolder pdfReferenceHolder = null;
		pdfReferenceHolder = ((!m_document.IsMergeDocHasSections) ? new PdfReferenceHolder(section) : CheckSection(section));
		m_sections.Add(section);
		section.Parent = this;
		m_sectionCollection.Add(pdfReferenceHolder);
	}

	private void Initialize()
	{
		m_count = new PdfNumber(0);
		m_sectionCollection = new PdfArray();
		m_pages = new PdfDictionary();
		m_pages.BeginSave += BeginSave;
		m_pages["Type"] = new PdfName("Pages");
		m_pages["Kids"] = m_sectionCollection;
		m_pages["Count"] = m_count;
		m_pages["Resources"] = new PdfDictionary();
		SetPageSettings(m_pages, m_document.PageSettings);
	}

	internal void Clear()
	{
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				((PdfSection)enumerator.Current).Clear();
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
		if (m_pages != null)
		{
			m_pages.Clear();
		}
		if (m_sectionCollection != null)
		{
			m_sectionCollection.Clear();
		}
		if (m_sections != null)
		{
			m_sections.Clear();
		}
		m_pages = null;
		m_sectionCollection = null;
		m_sections = null;
		m_document = null;
	}

	private void BeginSave(object sender, SavePdfPrimitiveEventArgs e)
	{
		m_count.IntValue = CountPages();
		SetPageSettings(m_pages, m_document.PageSettings);
	}
}
