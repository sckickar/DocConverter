using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

public class PdfSection : IPdfWrapper, IEnumerable
{
	private struct PdfPageEnumerator : IEnumerator
	{
		private PdfSection m_section;

		private int m_index;

		public object Current
		{
			get
			{
				CheckIndex();
				return m_section[m_index];
			}
		}

		internal PdfPageEnumerator(PdfSection section)
		{
			if (section == null)
			{
				throw new ArgumentNullException("section");
			}
			m_section = section;
			m_index = -1;
		}

		public bool MoveNext()
		{
			m_index++;
			return m_index < m_section.Count;
		}

		public void Reset()
		{
			m_index = -1;
		}

		private void CheckIndex()
		{
			if (m_index < 0 || m_index >= m_section.Count)
			{
				throw new IndexOutOfRangeException();
			}
		}
	}

	private List<PdfPageBase> m_pages = new List<PdfPageBase>();

	private PdfArray m_pagesReferences;

	private PdfDictionary m_section;

	private PdfNumber m_count;

	private PdfSectionCollection m_parent;

	private PdfDictionary m_resources;

	private PdfPageSettings m_settings;

	private PdfSectionTemplate m_pageTemplate;

	private PdfPageLabel m_pageLabel;

	private bool m_isProgressOn;

	private PdfPageSettings m_initialSettings;

	private PdfPageTransition m_savedTransition;

	private bool m_isTransitionSaved;

	private PdfSectionPageCollection m_pagesCollection;

	internal PdfDocumentBase m_document;

	private bool m_isNewPageSection;

	internal bool m_importedSection;

	public PdfSectionPageCollection Pages
	{
		get
		{
			if (m_pagesCollection == null)
			{
				m_pagesCollection = new PdfSectionPageCollection(this);
			}
			return m_pagesCollection;
		}
	}

	public PdfPageLabel PageLabel
	{
		get
		{
			return m_pageLabel;
		}
		set
		{
			if (value != null)
			{
				Parent.PageLabelsSet();
			}
			m_pageLabel = value;
		}
	}

	public PdfPageSettings PageSettings
	{
		get
		{
			return m_settings;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("PageSettings");
			}
			m_settings = value;
		}
	}

	public PdfSectionTemplate Template
	{
		get
		{
			if (m_pageTemplate == null)
			{
				m_pageTemplate = new PdfSectionTemplate();
			}
			return m_pageTemplate;
		}
		set
		{
			m_pageTemplate = value;
		}
	}

	internal PdfPage this[int index]
	{
		get
		{
			if (0 > index || Count <= index)
			{
				throw new ArgumentOutOfRangeException("index", "The index can't be less then zero or greater then Count.");
			}
			return m_pages[index] as PdfPage;
		}
	}

	internal int Count
	{
		get
		{
			if (m_pagesReferences != null)
			{
				return m_pagesReferences.Count;
			}
			return 0;
		}
	}

	internal PdfSectionCollection Parent
	{
		get
		{
			return m_parent;
		}
		set
		{
			m_parent = value;
			if (value != null)
			{
				m_section["Parent"] = new PdfReferenceHolder(value);
			}
			else
			{
				m_section.Remove("Parent");
			}
		}
	}

	internal PdfDictionary Resources
	{
		get
		{
			if (m_resources == null)
			{
				m_resources = new PdfDictionary();
				m_section["Resources"] = m_resources;
			}
			return m_resources;
		}
	}

	internal PdfDocument Document => m_parent.Document;

	internal PdfDocumentBase ParentDocument => m_document;

	IPdfPrimitive IPdfWrapper.Element => m_section;

	public event PageAddedEventHandler PageAdded;

	private PdfSection()
	{
		Initialize();
	}

	internal PdfSection(PdfDocumentBase document, PdfPageSettings pageSettings)
		: this()
	{
		m_document = document;
		m_settings = (PdfPageSettings)pageSettings.Clone();
		m_initialSettings = (PdfPageSettings)m_settings.Clone();
	}

	internal PdfSection(PdfDocument document)
		: this(document, document.PageSettings)
	{
	}

	internal PdfPage Add()
	{
		PdfPage pdfPage = new PdfPage();
		m_isNewPageSection = true;
		Add(pdfPage);
		m_isNewPageSection = false;
		return pdfPage;
	}

	internal void Add(PdfPage page)
	{
		if (!m_isNewPageSection)
		{
			m_isNewPageSection = page.IsNewPage;
		}
		PdfReferenceHolder pdfReferenceHolder = null;
		pdfReferenceHolder = (m_isNewPageSection ? new PdfReferenceHolder(page) : CheckPresence(page));
		m_isNewPageSection = false;
		m_pages.Add(page);
		m_pagesReferences.Add(pdfReferenceHolder);
		page.AssignSection(this);
		if (m_isProgressOn)
		{
			page.SetProgress();
		}
		else
		{
			page.ResetProgress();
		}
		PageAddedMethod(page);
	}

	internal void Insert(int index, PdfPage page)
	{
		PdfReferenceHolder element = CheckPresence(page);
		m_pages.Insert(index, page);
		m_pagesReferences.Insert(index, element);
		page.AssignSection(this);
		if (m_isProgressOn)
		{
			page.SetProgress();
		}
		else
		{
			page.ResetProgress();
		}
		PageAddedMethod(page);
	}

	internal void Insert(int index, PdfPageBase loadedPage)
	{
		PdfReferenceHolder element = new PdfReferenceHolder(loadedPage);
		m_pages.Insert(index, loadedPage);
		if (loadedPage.Dictionary.ContainsKey("Parent"))
		{
			loadedPage.Dictionary.Items.Remove((PdfName)"Parent");
			loadedPage.Dictionary["Parent"] = new PdfReferenceHolder(this);
		}
		if (m_pagesReferences.Contains(element))
		{
			throw new ArgumentException("The page already exists in some section, it can't be contained by several sections", "loadedPage");
		}
		m_pagesReferences.Insert(index, element);
		m_count.IntValue = Count;
	}

	internal int IndexOf(PdfPage page)
	{
		if (m_pages.Contains(page))
		{
			return m_pages.IndexOf(page);
		}
		PdfReferenceHolder element = new PdfReferenceHolder(page);
		return m_pagesReferences.IndexOf(element);
	}

	internal bool Contains(PdfPage page)
	{
		int num = IndexOf(page);
		return 0 <= num;
	}

	internal void Remove(PdfPage page)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		PdfReferenceHolder element = new PdfReferenceHolder(page);
		m_pagesReferences.Remove(element);
		m_pages.Remove(page);
	}

	internal void RemoveAt(int index)
	{
		_ = this[index];
		m_pagesReferences.RemoveAt(index);
		m_pages.RemoveAt(index);
	}

	internal void Clear()
	{
		for (int num = m_pages.Count - 1; num > -1; num--)
		{
			PdfPage pdfPage = m_pages[num] as PdfPage;
			if (pdfPage.Dictionary != null)
			{
				Remove(pdfPage);
			}
			pdfPage.Clear();
			pdfPage = null;
		}
		if (m_pages != null)
		{
			m_pages.Clear();
		}
		if (m_pagesReferences != null)
		{
			m_pagesReferences.Clear();
		}
		if (m_resources != null)
		{
			m_resources.Clear();
		}
		if (m_section != null)
		{
			m_section.Clear();
		}
		if (m_pagesCollection != null)
		{
			m_pagesCollection.Clear();
		}
		while (Count > 0)
		{
			RemoveAt(Count - 1);
		}
		m_pages = null;
		m_resources = null;
		m_pagesReferences = null;
		m_initialSettings = null;
		m_settings = null;
		m_parent = null;
		m_document = null;
		m_pagesCollection = null;
		m_pageTemplate = null;
		m_section = null;
	}

	public IEnumerator GetEnumerator()
	{
		return new PdfPageEnumerator(this);
	}

	internal bool ContainsTemplates(PdfDocument document, PdfPage page, bool foreground)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		PdfPageTemplateElement[] documentTemplates = GetDocumentTemplates(document, page, headers: true, foreground);
		PdfPageTemplateElement[] documentTemplates2 = GetDocumentTemplates(document, page, headers: false, foreground);
		PdfPageTemplateElement[] sectionTemplates = GetSectionTemplates(page, headers: true, foreground);
		PdfPageTemplateElement[] sectionTemplates2 = GetSectionTemplates(page, headers: false, foreground);
		if (documentTemplates.Length == 0 && documentTemplates2.Length == 0 && sectionTemplates.Length == 0)
		{
			return sectionTemplates2.Length != 0;
		}
		return true;
	}

	internal void DrawTemplates(PdfPage page, PdfPageLayer layer, PdfDocument document, bool foreground)
	{
		if (layer == null)
		{
			throw new ArgumentNullException("layer");
		}
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		PdfPageTemplateElement[] documentTemplates = GetDocumentTemplates(document, page, headers: true, foreground);
		PdfPageTemplateElement[] documentTemplates2 = GetDocumentTemplates(document, page, headers: false, foreground);
		PdfPageTemplateElement[] sectionTemplates = GetSectionTemplates(page, headers: true, foreground);
		PdfPageTemplateElement[] sectionTemplates2 = GetSectionTemplates(page, headers: false, foreground);
		if (foreground)
		{
			DrawTemplates(layer, document, sectionTemplates);
			DrawTemplates(layer, document, sectionTemplates2);
			DrawTemplates(layer, document, documentTemplates);
			DrawTemplates(layer, document, documentTemplates2);
		}
		else
		{
			DrawTemplates(layer, document, documentTemplates);
			DrawTemplates(layer, document, documentTemplates2);
			DrawTemplates(layer, document, sectionTemplates);
			DrawTemplates(layer, document, sectionTemplates2);
		}
	}

	internal RectangleF GetActualBounds(PdfPage page, bool includeMargins)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		if (Parent != null)
		{
			PdfDocument document = Parent.Document;
			if (document == null)
			{
				throw new PdfDocumentException("The section should be added to the section collection before this operation");
			}
			return GetActualBounds(document, page, includeMargins);
		}
		SizeF size = (includeMargins ? PageSettings.GetActualSize() : PageSettings.Size);
		float x = (includeMargins ? PageSettings.Margins.Left : 0f);
		float y = (includeMargins ? PageSettings.Margins.Top : 0f);
		PointF location = new PointF(x, y);
		return new RectangleF(location, size);
	}

	internal RectangleF GetActualBounds(PdfDocument document, PdfPage page, bool includeMargins)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		RectangleF empty = RectangleF.Empty;
		empty.Size = (includeMargins ? PageSettings.Size : PageSettings.GetActualSize());
		float leftIndentWidth = GetLeftIndentWidth(document, page, includeMargins);
		float topIndentHeight = GetTopIndentHeight(document, page, includeMargins);
		float rightIndentWidth = GetRightIndentWidth(document, page, includeMargins);
		float bottomIndentHeight = GetBottomIndentHeight(document, page, includeMargins);
		empty.X += leftIndentWidth;
		empty.Y += topIndentHeight;
		empty.Width -= leftIndentWidth + rightIndentWidth;
		empty.Height -= topIndentHeight + bottomIndentHeight;
		return empty;
	}

	internal float GetLeftIndentWidth(PdfDocument document, PdfPage page, bool includeMargins)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		float num = (includeMargins ? PageSettings.Margins.Left : 0f);
		float num2 = ((Template.GetLeft(page) != null) ? Template.GetLeft(page).Width : 0f);
		float val = ((document.Template.GetLeft(page) != null) ? document.Template.GetLeft(page).Width : 0f);
		return num + (Template.ApplyDocumentLeftTemplate ? Math.Max(num2, val) : num2);
	}

	internal float GetTopIndentHeight(PdfDocument document, PdfPage page, bool includeMargins)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		float num = (includeMargins ? PageSettings.Margins.Top : 0f);
		float num2 = ((Template.GetTop(page) != null) ? Template.GetTop(page).Height : 0f);
		float val = ((document.Template.GetTop(page) != null) ? document.Template.GetTop(page).Height : 0f);
		return num + (Template.ApplyDocumentTopTemplate ? Math.Max(num2, val) : num2);
	}

	internal float GetRightIndentWidth(PdfDocument document, PdfPage page, bool includeMargins)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		float num = (includeMargins ? PageSettings.Margins.Right : 0f);
		float num2 = ((Template.GetRight(page) != null) ? Template.GetRight(page).Width : 0f);
		float val = ((document.Template.GetRight(page) != null) ? document.Template.GetRight(page).Width : 0f);
		return num + (Template.ApplyDocumentRightTemplate ? Math.Max(num2, val) : num2);
	}

	internal float GetBottomIndentHeight(PdfDocument document, PdfPage page, bool includeMargins)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		float num = (includeMargins ? PageSettings.Margins.Bottom : 0f);
		float num2 = ((Template.GetBottom(page) != null) ? Template.GetBottom(page).Height : 0f);
		float val = ((document.Template.GetBottom(page) != null) ? document.Template.GetBottom(page).Height : 0f);
		return num + (Template.ApplyDocumentBottomTemplate ? Math.Max(num2, val) : num2);
	}

	internal PointF PointToNativePdf(PdfPage page, PointF point)
	{
		RectangleF actualBounds = GetActualBounds(page, includeMargins: true);
		point.X += actualBounds.Left;
		point.Y = PageSettings.Height - (actualBounds.Top + point.Y);
		return point;
	}

	private void DrawTemplates(PdfPageLayer layer, PdfDocument document, PdfPageTemplateElement[] templates)
	{
		if (layer == null)
		{
			throw new ArgumentNullException("layer");
		}
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		if (templates != null && templates.Length != 0)
		{
			int i = 0;
			for (int num = templates.Length; i < num; i++)
			{
				templates[i].Draw(layer, document);
			}
		}
	}

	private PdfPageTemplateElement[] GetDocumentTemplates(PdfDocument document, PdfPage page, bool headers, bool foreground)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		List<PdfPageTemplateElement> list = new List<PdfPageTemplateElement>();
		if (headers)
		{
			if (Template.ApplyDocumentTopTemplate && document.Template.GetTop(page) != null && document.Template.GetTop(page).Foreground == foreground)
			{
				list.Add(document.Template.GetTop(page));
			}
			if (Template.ApplyDocumentBottomTemplate && document.Template.GetBottom(page) != null && document.Template.GetBottom(page).Foreground == foreground)
			{
				list.Add(document.Template.GetBottom(page));
			}
			if (Template.ApplyDocumentLeftTemplate && document.Template.GetLeft(page) != null && document.Template.GetLeft(page).Foreground == foreground)
			{
				list.Add(document.Template.GetLeft(page));
			}
			if (Template.ApplyDocumentRightTemplate && document.Template.GetRight(page) != null && document.Template.GetRight(page).Foreground == foreground)
			{
				list.Add(document.Template.GetRight(page));
			}
		}
		else if (Template.ApplyDocumentStamps)
		{
			int i = 0;
			for (int count = document.Template.Stamps.Count; i < count; i++)
			{
				PdfPageTemplateElement pdfPageTemplateElement = document.Template.Stamps[i];
				if (pdfPageTemplateElement.Foreground == foreground)
				{
					list.Add(pdfPageTemplateElement);
				}
			}
		}
		return list.ToArray();
	}

	private PdfPageTemplateElement[] GetSectionTemplates(PdfPage page, bool headers, bool foreground)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		List<PdfPageTemplateElement> list = new List<PdfPageTemplateElement>();
		if (headers)
		{
			if (Template.GetTop(page) != null && Template.GetTop(page).Foreground == foreground)
			{
				list.Add(Template.GetTop(page));
			}
			if (Template.GetBottom(page) != null && Template.GetBottom(page).Foreground == foreground)
			{
				list.Add(Template.GetBottom(page));
			}
			if (Template.GetLeft(page) != null && Template.GetLeft(page).Foreground == foreground)
			{
				list.Add(Template.GetLeft(page));
			}
			if (Template.GetRight(page) != null && Template.GetRight(page).Foreground == foreground)
			{
				list.Add(Template.GetRight(page));
			}
		}
		else
		{
			int i = 0;
			for (int count = Template.Stamps.Count; i < count; i++)
			{
				PdfPageTemplateElement pdfPageTemplateElement = Template.Stamps[i];
				if (pdfPageTemplateElement.Foreground == foreground)
				{
					list.Add(pdfPageTemplateElement);
				}
			}
		}
		return list.ToArray();
	}

	protected virtual void OnPageAdded(PageAddedEventArgs args)
	{
		if (this.PageAdded != null)
		{
			this.PageAdded(this, args);
		}
	}

	internal void SetProgress()
	{
		if (m_isProgressOn)
		{
			return;
		}
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				((PdfPage)enumerator.Current).SetProgress();
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
		m_isProgressOn = true;
	}

	internal void ResetProgress()
	{
		if (!m_isProgressOn)
		{
			return;
		}
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				((PdfPage)enumerator.Current).ResetProgress();
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
		m_isProgressOn = false;
	}

	internal void OnPageSaving(PdfPage page)
	{
		Parent.OnPageSaving(page);
	}

	private PdfReferenceHolder CheckPresence(PdfPage page)
	{
		PdfReferenceHolder pdfReferenceHolder = new PdfReferenceHolder(page);
		bool flag = false;
		if (m_parent != null)
		{
			foreach (PdfSection item in Parent)
			{
				flag |= item.Contains(page);
				if (flag)
				{
					break;
				}
			}
		}
		else
		{
			flag = m_pagesReferences.Contains(pdfReferenceHolder);
		}
		if (flag)
		{
			throw new ArgumentException("The page already exists in some section, it can't be contained by several sections", "page");
		}
		return pdfReferenceHolder;
	}

	private void SetPageSettings(PdfDictionary container, PdfPageSettings parentSettings)
	{
		if (container == null)
		{
			throw new ArgumentNullException("container");
		}
		if (parentSettings == null || PageSettings.Size != parentSettings.Size)
		{
			RectangleF rectangleF = default(RectangleF);
			rectangleF = ((parentSettings == null || !(PageSettings.Size != parentSettings.Size) || !m_importedSection || PageSettings.Rotate == parentSettings.Rotate || !Document.IsMergeDocHasSections) ? new RectangleF(PageSettings.Origin, PageSettings.Size) : new RectangleF(PageSettings.Origin, parentSettings.Size));
			container["MediaBox"] = PdfArray.FromRectangle(rectangleF);
		}
		if (parentSettings != null)
		{
			_ = PageSettings.Rotate;
		}
		int value = 0;
		if (m_parent != null)
		{
			if (PageSettings.m_isRotation && !Document.PageSettings.m_isRotation)
			{
				value = 90 * (int)PageSettings.Rotate;
			}
			else if (!Document.PageSettings.m_isRotation)
			{
				_ = PageSettings.Rotate;
				value = 90 * (int)PageSettings.Rotate;
			}
			else if (PageSettings.m_isRotation)
			{
				value = 90 * (int)PageSettings.Rotate;
			}
			else if (parentSettings != null)
			{
				value = 90 * (int)parentSettings.Rotate;
			}
		}
		else
		{
			value = 90 * (int)PageSettings.Rotate;
		}
		PdfNumber pdfNumber = new PdfNumber(value);
		if (pdfNumber.IntValue != 0)
		{
			container["Rotate"] = pdfNumber;
		}
		if (parentSettings != null && m_importedSection)
		{
			_ = PageSettings.Rotate;
			if ((PageSettings.Rotate != 0 || pdfNumber.IntValue != 0) && container.ContainsKey("Kids") && PdfCrossTable.Dereference(container["Kids"]) is PdfArray pdfArray)
			{
				for (int i = 0; i < pdfArray.Count; i++)
				{
					if (PdfCrossTable.Dereference(pdfArray[i]) is PdfDictionary pdfDictionary)
					{
						pdfDictionary["Rotate"] = pdfNumber;
					}
				}
			}
		}
		if (parentSettings == null || PageSettings.Unit != parentSettings.Unit)
		{
			float value2 = new PdfUnitConvertor().ConvertUnits(1f, PageSettings.Unit, PdfGraphicsUnit.Point);
			container["UserUnit"] = new PdfNumber(value2);
		}
	}

	private void Initialize()
	{
		m_pagesReferences = new PdfArray();
		m_section = new PdfDictionary();
		m_section.BeginSave += BeginSave;
		m_section.EndSave += EndSave;
		m_count = new PdfNumber(0);
		m_section["Count"] = m_count;
		m_section["Type"] = new PdfName("Pages");
		m_section["Kids"] = m_pagesReferences;
	}

	internal PdfPageTransition GetTransitionSettings()
	{
		if (!m_isTransitionSaved)
		{
			if (m_settings.AssignTransition() == null)
			{
				if (Document.PageSettings.AssignTransition() != null)
				{
					m_savedTransition = (PdfPageTransition)Document.PageSettings.Transition.Clone();
				}
			}
			else if (Document.PageSettings.AssignTransition() == null)
			{
				m_savedTransition = (PdfPageTransition)m_settings.Transition.Clone();
			}
			else
			{
				m_savedTransition = new PdfPageTransition();
				PdfPageTransition transition = m_initialSettings.Transition;
				PdfPageTransition transition2 = Document.PageSettings.Transition;
				bool flag = transition.PageDuration == m_settings.Transition.PageDuration;
				bool flag2 = transition2.PageDuration == m_settings.Transition.PageDuration;
				m_savedTransition.PageDuration = ((flag2 && !flag) ? transition2.PageDuration : transition.PageDuration);
				flag = transition.Dimension == m_settings.Transition.Dimension;
				flag2 = transition2.Dimension == m_settings.Transition.Dimension;
				m_savedTransition.Dimension = ((flag2 && !flag) ? transition2.Dimension : transition.Dimension);
				flag = transition.Direction == m_settings.Transition.Direction;
				flag2 = transition2.Direction == m_settings.Transition.Direction;
				m_savedTransition.Direction = ((flag2 && !flag) ? transition2.Direction : transition.Direction);
				flag = transition.Motion == m_settings.Transition.Motion;
				flag2 = transition2.Motion == m_settings.Transition.Motion;
				m_savedTransition.Motion = ((flag2 && !flag) ? transition2.Motion : transition.Motion);
				flag = transition.Scale == m_settings.Transition.Scale;
				flag2 = transition2.Scale == m_settings.Transition.Scale;
				m_savedTransition.Scale = ((flag2 && !flag) ? transition2.Scale : transition.Scale);
				flag = transition.Style == m_settings.Transition.Style;
				flag2 = transition2.Style == m_settings.Transition.Style;
				m_savedTransition.Style = ((flag2 && !flag) ? transition2.Style : transition.Style);
				flag = transition.Duration == m_settings.Transition.Duration;
				flag2 = transition2.Duration == m_settings.Transition.Duration;
				m_savedTransition.Duration = ((flag2 && !flag) ? transition2.Duration : transition.Duration);
			}
		}
		return m_savedTransition;
	}

	internal void DropCropBox()
	{
		SetPageSettings(m_section, null);
		m_section["CropBox"] = m_section["MediaBox"];
	}

	private void PageAddedMethod(PdfPage page)
	{
		PageAddedEventArgs args = new PageAddedEventArgs(page);
		OnPageAdded(args);
		Parent?.Document.Pages.OnPageAdded(args);
		m_count.IntValue = Count;
	}

	private void BeginSave(object sender, SavePdfPrimitiveEventArgs e)
	{
		m_count.IntValue = Count;
		PdfDocument pdfDocument = e.Writer.Document as PdfDocument;
		if (m_section != null)
		{
			if (pdfDocument != null)
			{
				SetPageSettings(m_section, pdfDocument.PageSettings);
			}
			else
			{
				SetPageSettings(m_section, null);
			}
		}
	}

	private void EndSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		m_savedTransition = null;
	}
}
