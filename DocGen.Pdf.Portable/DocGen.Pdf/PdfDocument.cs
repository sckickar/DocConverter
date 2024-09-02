using System;
using System.Collections.Generic;
using System.IO;
using DocGen.Pdf.ColorSpace;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

public class PdfDocument : PdfDocumentBase, IDisposable
{
	internal const float DefaultMargin = 40f;

	private static PdfFont s_defaultFont;

	[ThreadStatic]
	private static PdfCacheCollection s_cache;

	private static object s_cacheLock;

	private PdfDocumentTemplate m_pageTemplate;

	private PdfAttachmentCollection m_attachments;

	private PdfDocumentPageCollection m_pages;

	private PdfNamedDestinationCollection m_namedDestinations;

	private bool m_isPdfViewerDocumentDisable = true;

	private PdfSectionCollection m_sections;

	private PdfPageSettings m_settings;

	private PdfBookmarkBase m_outlines;

	private bool m_bPageLabels;

	private bool m_bWasEncrypted;

	private PdfDocumentActions m_actions;

	private PdfColorSpace m_colorSpace;

	[ThreadStatic]
	internal static PdfConformanceLevel ConformanceLevel;

	private static bool m_enableCache;

	[ThreadStatic]
	private static bool m_enableUniqueResourceNaming;

	private static bool m_enableThreadSafe;

	private bool m_isDisposed;

	private bool m_isTaggedPdf;

	private PdfStructTreeRoot m_treeRoot;

	private bool m_isMergeDocHasSection;

	private ZugferdConformanceLevel m_zugferdConformanceLevel;

	private ZugferdVersion m_zugferdConformanceVersion;

	internal Dictionary<string, PdfImage> m_imageCollection;

	internal Dictionary<string, PdfDictionary> m_parnetTagDicitionaryCollection = new Dictionary<string, PdfDictionary>();

	private bool m_split = true;

	internal bool m_WordtoPDFTagged;

	internal static bool FontEmbeddingEnabled;

	internal bool SeparateTable
	{
		get
		{
			return m_split;
		}
		set
		{
			m_split = value;
		}
	}

	public ZugferdConformanceLevel ZugferdConformanceLevel
	{
		get
		{
			return m_zugferdConformanceLevel;
		}
		set
		{
			base.Catalog.Pages.Document.DocumentInformation.ZugferdConformanceLevel = value;
			m_zugferdConformanceLevel = value;
		}
	}

	public ZugferdVersion ZugferdVersion
	{
		get
		{
			return m_zugferdConformanceVersion;
		}
		set
		{
			base.Catalog.Pages.Document.DocumentInformation.ZugferdVersion = value;
			m_zugferdConformanceVersion = value;
		}
	}

	public PdfDocumentTemplate Template
	{
		get
		{
			if (m_pageTemplate == null)
			{
				m_pageTemplate = new PdfDocumentTemplate();
			}
			return m_pageTemplate;
		}
		set
		{
			m_pageTemplate = value;
		}
	}

	internal override bool IsPdfViewerDocumentDisable
	{
		get
		{
			return m_isPdfViewerDocumentDisable;
		}
		set
		{
			m_isPdfViewerDocumentDisable = value;
		}
	}

	public PdfDocumentActions Actions
	{
		get
		{
			if (m_actions == null)
			{
				m_actions = new PdfDocumentActions(base.Catalog);
				base.Catalog["AA"] = ((IPdfWrapper)m_actions).Element;
			}
			return m_actions;
		}
	}

	public bool AutoTag
	{
		get
		{
			return m_isTaggedPdf;
		}
		set
		{
			m_isTaggedPdf = value;
			base.FileStructure.TaggedPdf = m_isTaggedPdf;
			DocumentInformation.m_autoTag = value;
		}
	}

	public PdfDocumentPageCollection Pages => m_pages;

	public PdfNamedDestinationCollection NamedDestinationCollection
	{
		get
		{
			if (m_namedDestinations == null)
			{
				m_namedDestinations = new PdfNamedDestinationCollection();
				if (base.Catalog.ContainsKey("Names"))
				{
					PdfReferenceHolder pdfReferenceHolder = base.Catalog["Names"] as PdfReferenceHolder;
					if (pdfReferenceHolder != null && pdfReferenceHolder.Object is PdfDictionary pdfDictionary)
					{
						pdfDictionary["Dests"] = new PdfReferenceHolder(m_namedDestinations);
					}
				}
				else
				{
					_ = base.Catalog.Names;
					if (base.Catalog.ContainsKey("Names"))
					{
						PdfReferenceHolder pdfReferenceHolder2 = base.Catalog["Names"] as PdfReferenceHolder;
						if (pdfReferenceHolder2 != null && pdfReferenceHolder2.Object is PdfDictionary pdfDictionary2)
						{
							pdfDictionary2["Dests"] = new PdfReferenceHolder(m_namedDestinations);
						}
					}
				}
			}
			return m_namedDestinations;
		}
	}

	public PdfSectionCollection Sections => m_sections;

	internal bool IsMergeDocHasSections
	{
		get
		{
			return m_isMergeDocHasSection;
		}
		set
		{
			m_isMergeDocHasSection = value;
		}
	}

	public PdfPageSettings PageSettings
	{
		get
		{
			if (m_settings == null)
			{
				m_settings = new PdfPageSettings(40f);
			}
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

	public override PdfBookmarkBase Bookmarks
	{
		get
		{
			if (m_outlines == null)
			{
				m_outlines = new PdfBookmarkBase();
				base.Catalog["Outlines"] = new PdfReferenceHolder(m_outlines);
			}
			return m_outlines;
		}
	}

	public PdfAttachmentCollection Attachments
	{
		get
		{
			if (m_attachments == null)
			{
				m_attachments = new PdfAttachmentCollection();
				base.Catalog.Names.EmbeddedFiles = m_attachments;
			}
			return m_attachments;
		}
	}

	public PdfForm Form
	{
		get
		{
			if (base.Catalog.Form == null)
			{
				base.Catalog.Form = new PdfForm();
			}
			return base.Catalog.Form;
		}
	}

	public PdfColorSpace ColorSpace
	{
		get
		{
			if (m_colorSpace == PdfColorSpace.RGB || m_colorSpace == PdfColorSpace.CMYK || m_colorSpace == PdfColorSpace.GrayScale)
			{
				return m_colorSpace;
			}
			return PdfColorSpace.RGB;
		}
		set
		{
			if (value == PdfColorSpace.RGB || value == PdfColorSpace.CMYK || value == PdfColorSpace.GrayScale)
			{
				m_colorSpace = value;
			}
			else
			{
				m_colorSpace = PdfColorSpace.RGB;
			}
		}
	}

	internal static PdfCacheCollection Cache
	{
		get
		{
			lock (s_cacheLock)
			{
				if (s_cache == null)
				{
					s_cache = new PdfCacheCollection();
				}
				return s_cache;
			}
		}
		set
		{
			s_cache = value;
		}
	}

	internal static PdfFont DefaultFont
	{
		get
		{
			lock (s_cacheLock)
			{
				if (s_defaultFont == null)
				{
					s_defaultFont = new PdfStandardFont(PdfFontFamily.Helvetica, 8f);
				}
			}
			return s_defaultFont;
		}
	}

	internal override bool WasEncrypted => m_bWasEncrypted;

	internal new Dictionary<string, PdfImage> ImageCollection
	{
		get
		{
			if (m_imageCollection == null)
			{
				m_imageCollection = new Dictionary<string, PdfImage>();
			}
			return m_imageCollection;
		}
	}

	public override int PageCount => Pages.Count;

	internal PdfConformanceLevel Conformance => ConformanceLevel;

	public static bool EnableCache
	{
		get
		{
			return m_enableCache;
		}
		set
		{
			m_enableCache = value;
		}
	}

	public static bool EnableUniqueResourceNaming
	{
		get
		{
			return m_enableUniqueResourceNaming;
		}
		set
		{
			m_enableUniqueResourceNaming = value;
		}
	}

	public static bool EnableThreadSafe
	{
		get
		{
			return m_enableThreadSafe;
		}
		set
		{
			m_enableThreadSafe = value;
			if (value)
			{
				m_enableCache = false;
			}
			else
			{
				m_enableCache = true;
			}
		}
	}

	static PdfDocument()
	{
		s_defaultFont = null;
		s_cacheLock = new object();
		m_enableCache = true;
		m_enableUniqueResourceNaming = true;
		m_enableThreadSafe = false;
		FontEmbeddingEnabled = false;
		s_cache = new PdfCacheCollection();
	}

	public PdfDocument()
		: this(isMerging: false)
	{
		ConformanceLevel = PdfConformanceLevel.None;
	}

	internal PdfDocument(bool isMerging)
	{
		PdfMainObjectCollection pdfMainObjectCollection = new PdfMainObjectCollection();
		SetMainObjectCollection(pdfMainObjectCollection);
		SetCrossTable(new PdfCrossTable
		{
			Document = this
		});
		PdfCatalog pdfCatalog = new PdfCatalog();
		SetCatalog(pdfCatalog);
		pdfMainObjectCollection.Add(pdfCatalog);
		if (!isMerging)
		{
			pdfCatalog.Position = -1;
		}
		m_sections = new PdfSectionCollection(this);
		m_pages = new PdfDocumentPageCollection(this);
		pdfCatalog.Pages = m_sections;
	}

	public PdfDocument(PdfConformanceLevel conformance)
		: this()
	{
		ConformanceLevel = conformance;
		if (Conformance == PdfConformanceLevel.Pdf_A1B || Conformance == PdfConformanceLevel.Pdf_A1A || Conformance == PdfConformanceLevel.Pdf_A3B || Conformance == PdfConformanceLevel.Pdf_A2B || Conformance == PdfConformanceLevel.Pdf_A2A || Conformance == PdfConformanceLevel.Pdf_A2U || Conformance == PdfConformanceLevel.Pdf_A3A || Conformance == PdfConformanceLevel.Pdf_A3U)
		{
			base.FileStructure.CrossReferenceType = PdfCrossReferenceType.CrossReferenceTable;
			base.FileStructure.Version = PdfVersion.Version1_4;
			if (conformance == PdfConformanceLevel.Pdf_A1A || conformance == PdfConformanceLevel.Pdf_A2A || conformance == PdfConformanceLevel.Pdf_A3A)
			{
				AutoTag = true;
			}
			SetDocumentColorProfile();
			return;
		}
		switch (conformance)
		{
		case PdfConformanceLevel.Pdf_X1A2001:
			throw new PdfConformanceException("PDF/X-1a conformance is not supported");
		case PdfConformanceLevel.Pdf_A4:
		case PdfConformanceLevel.Pdf_A4E:
		case PdfConformanceLevel.Pdf_A4F:
			base.FileStructure.CrossReferenceType = PdfCrossReferenceType.CrossReferenceTable;
			base.FileStructure.Version = PdfVersion.Version2_0;
			SetDocumentColorProfile();
			break;
		}
	}

	private string CheckLicense()
	{
		_ = string.Empty;
		return "";
	}

	public override void Save(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (!string.IsNullOrEmpty(CheckLicense()))
		{
			lock (PdfDocumentBase.s_licenseLock)
			{
				AddWatermark();
			}
		}
		if (Form != null && Form.Xfa != null)
		{
			Form.Xfa.Save(this);
		}
		CheckPagesPresence();
		if (Conformance == PdfConformanceLevel.Pdf_A1B || Conformance == PdfConformanceLevel.Pdf_A1A || Conformance == PdfConformanceLevel.Pdf_A3B || Conformance == PdfConformanceLevel.Pdf_A2B || Conformance == PdfConformanceLevel.Pdf_A2A || Conformance == PdfConformanceLevel.Pdf_A2U || Conformance == PdfConformanceLevel.Pdf_A3A || Conformance == PdfConformanceLevel.Pdf_A3U || Conformance == PdfConformanceLevel.Pdf_A4 || Conformance == PdfConformanceLevel.Pdf_A4E || Conformance == PdfConformanceLevel.Pdf_A4F)
		{
			AutoTagRequiredElements();
			if (Conformance == PdfConformanceLevel.Pdf_A3B || Conformance == PdfConformanceLevel.Pdf_A3A || Conformance == PdfConformanceLevel.Pdf_A3U || ((Conformance == PdfConformanceLevel.Pdf_A4 || Conformance == PdfConformanceLevel.Pdf_A4E || Conformance == PdfConformanceLevel.Pdf_A4F) && base.Catalog != null && base.Catalog.Names != null && Attachments != null && Attachments.Count > 0))
			{
				PdfName key = new PdfName("AFRelationship");
				PdfArray pdfArray = new PdfArray();
				for (int i = 0; i < Attachments.Count; i++)
				{
					if (ZugferdConformanceLevel == ZugferdConformanceLevel.None)
					{
						pdfArray.Add(new PdfReferenceHolder(Attachments[i].Dictionary));
						continue;
					}
					if (!Attachments[i].Dictionary.Items.ContainsKey(key))
					{
						Attachments[i].Dictionary.Items[key] = new PdfName(PdfAttachmentRelationship.Alternative);
					}
					pdfArray.Add(new PdfReferenceHolder(Attachments[i].Dictionary));
				}
				base.Catalog.Items[new PdfName("AF")] = pdfArray;
			}
		}
		PdfWriter pdfWriter = new PdfWriter(stream);
		pdfWriter.Document = this;
		if (m_outlines != null && m_outlines.Count < 1)
		{
			base.Catalog.Remove("Outlines");
		}
		if (PdfCatalog.StructTreeRoot != null || base.IsWCAGCloned)
		{
			base.DocumentInformation.m_autoTag = true;
			AutoTagRequiredElements();
			if (base.DocumentInformation.Language != null)
			{
				base.Catalog["Lang"] = new PdfString(base.DocumentInformation.Language);
			}
			else
			{
				base.Catalog["Lang"] = new PdfString("en");
			}
			if (base.DocumentInformation.Title == string.Empty)
			{
				base.DocumentInformation.Title = "Tagged Pdf";
			}
			if (!base.Catalog.ContainsKey("StructTreeRoot"))
			{
				base.Catalog["StructTreeRoot"] = new PdfReferenceHolder(PdfCatalog.StructTreeRoot);
			}
			if (!base.Catalog.ContainsKey("MarkInfo"))
			{
				base.Catalog["MarkInfo"] = new PdfDictionary();
			}
			base.ViewerPreferences.DisplayTitle = true;
			(base.Catalog["MarkInfo"] as PdfDictionary)["Marked"] = new PdfBoolean(value: true);
			m_treeRoot = PdfCrossTable.Dereference(base.Catalog["StructTreeRoot"]) as PdfStructTreeRoot;
			if (m_treeRoot != null && m_treeRoot.HasOrder && m_treeRoot.ContainsKey("K") && PdfCrossTable.Dereference(m_treeRoot["K"]) is PdfArray pdfArray2)
			{
				if (m_WordtoPDFTagged)
				{
					m_treeRoot.m_WordtoPDFTaggedObject = m_WordtoPDFTagged;
				}
				if (m_treeRoot.OrderList.Count == pdfArray2.Count)
				{
					m_treeRoot.ReArrange(pdfArray2, m_treeRoot.OrderList);
				}
				else
				{
					m_treeRoot.GetChildElements(pdfArray2);
				}
			}
		}
		ProcessPageLabels();
		if (base.Security.EncryptOnlyAttachment && base.Security.UserPassword == string.Empty)
		{
			throw new PdfException("User password cannot be empty for encrypt only attachment.");
		}
		base.CrossTable.Save(pdfWriter);
		if (progressDelegate != null)
		{
			int count = Pages.Count;
			ProgressEventArgs arguments = new ProgressEventArgs(count, count);
			OnSaveProgress(arguments);
		}
		DocumentSavedEventArgs args = new DocumentSavedEventArgs(pdfWriter);
		OnDocumentSaved(args);
		pdfWriter.Close();
	}

	private void AddWatermark()
	{
		if (m_licensingAdded)
		{
			return;
		}
	}

	public override void Close(bool completely)
	{
		if (m_isDisposed)
		{
			return;
		}
		m_isDisposed = true;
		if (completely && Form != null && base.EnableMemoryOptimization)
		{
			Form.Clear();
		}
		if (completely && base.EnableMemoryOptimization)
		{
			m_off = null;
			m_on = null;
			m_order = null;
			if (m_outlines != null)
			{
				m_outlines.Dispose();
			}
			progressDelegate = null;
			m_sublayer = null;
			if (m_pages != null)
			{
				m_pages.Clear();
			}
			if (m_sections != null)
			{
				m_sections.Clear();
			}
			s_defaultFont = null;
		}
		base.Close(completely);
		ConformanceLevel = PdfConformanceLevel.None;
		m_pageTemplate = null;
		m_attachments = null;
		m_pages = null;
		m_sections = null;
		m_settings = null;
		m_outlines = null;
		m_bPageLabels = false;
		m_bWasEncrypted = false;
		m_actions = null;
		m_parnetTagDicitionaryCollection.Clear();
		PdfCatalog.m_structTreeRoot = null;
		if (m_treeRoot != null)
		{
			m_treeRoot.Dispose();
		}
		if (m_imageCollection != null)
		{
			m_imageCollection.Clear();
		}
	}

	public void Dispose()
	{
		if (base.EnableMemoryOptimization)
		{
			Close(completely: true);
		}
		else
		{
			Close(completely: false);
		}
		GC.SuppressFinalize(this);
	}

	public object Clone()
	{
		if (base.CrossTable.EncryptorDictionary != null)
		{
			throw new ArgumentException("Can't clone the Encrypted document");
		}
		return MemberwiseClone();
	}

	public static void ClearFontCache()
	{
		foreach (PdfFont item in Cache.FontCollection)
		{
			if (item is PdfTrueTypeFont pdfTrueTypeFont)
			{
				if (pdfTrueTypeFont.m_fontInternal != null)
				{
					pdfTrueTypeFont.m_fontInternal.Close();
				}
				PdfTrueTypeFont pdfTrueTypeFont2 = null;
			}
		}
		Cache.FontCollection.Clear();
		Cache.FontCollection = null;
	}

	private void AutoTagRequiredElements()
	{
		if (base.DocumentInformation.Language != null)
		{
			base.Catalog["Lang"] = new PdfString(base.DocumentInformation.Language);
		}
		else
		{
			base.DocumentInformation.Language = "en";
		}
		if (base.DocumentInformation.Title == string.Empty)
		{
			base.DocumentInformation.Title = "Tagged Pdf";
		}
		_ = base.DocumentInformation.XmpMetadata;
	}

	internal void PageLabelsSet()
	{
		m_bPageLabels = true;
	}

	private void CheckPagesPresence()
	{
		if (Pages.Count == 0)
		{
			Pages.Add();
		}
	}

	private void ProcessPageLabels()
	{
		if (!m_bPageLabels)
		{
			return;
		}
		PdfDictionary pdfDictionary = base.Catalog["PageLabels"] as PdfDictionary;
		if (pdfDictionary == null)
		{
			pdfDictionary = new PdfDictionary();
			base.Catalog["PageLabels"] = pdfDictionary;
		}
		PdfArray pdfArray = (PdfArray)(pdfDictionary["Nums"] = new PdfArray());
		int num = 0;
		int num2 = -1;
		foreach (PdfSection section in Sections)
		{
			PdfPageLabel pdfPageLabel = section.PageLabel;
			if (pdfPageLabel == null)
			{
				pdfPageLabel = new PdfPageLabel();
				if (num2 != -1)
				{
					pdfPageLabel.StartNumber = num2 + 1;
				}
				else
				{
					pdfPageLabel.StartNumber = num + 1;
				}
			}
			pdfArray.Add(new PdfNumber(num));
			if (pdfPageLabel.StartNumber < 0)
			{
				pdfPageLabel.StartNumber = num + 1;
			}
			pdfArray.Add(((IPdfWrapper)pdfPageLabel).Element);
			num += section.Count;
		}
	}

	private void SetDocumentColorProfile()
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		pdfDictionary["Info"] = new PdfString("sRGB IEC61966-2.1");
		pdfDictionary["S"] = new PdfName("GTS_PDFA1");
		pdfDictionary["OutputConditionIdentifier"] = new PdfString("custom");
		pdfDictionary["Type"] = new PdfName("OutputIntent");
		pdfDictionary["OutputCondition"] = new PdfString("");
		pdfDictionary["RegistryName"] = new PdfString("");
		PdfICCColorProfile wrapper = new PdfICCColorProfile();
		pdfDictionary["DestOutputProfile"] = new PdfReferenceHolder(wrapper);
		PdfArray pdfArray = new PdfArray();
		pdfArray.Add(pdfDictionary);
		base.Catalog["OutputIntents"] = pdfArray;
	}

	internal string GetImageHash(byte[] imageData)
	{
		string result = CreateHashFromStream(imageData);
		imageData = null;
		return result;
	}

	internal override PdfForm ObtainForm()
	{
		return Form;
	}

	internal override void AddFields(PdfLoadedDocument ldDoc, PdfPageBase newPage, List<PdfField> fields)
	{
		List<PdfDictionary> list = null;
		PdfArray pdfArray = null;
		if (ldDoc.Catalog.ContainsKey("AcroForm"))
		{
			CloneAcroFormFontResources(ldDoc);
			if (PdfCrossTable.Dereference(ldDoc.Catalog["AcroForm"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("CO") && PdfCrossTable.Dereference(pdfDictionary["CO"]) is PdfArray pdfArray2)
			{
				list = new List<PdfDictionary>();
				pdfArray = (Form.Dictionary.ContainsKey("CO") ? (PdfCrossTable.Dereference(Form.Dictionary["CO"]) as PdfArray) : new PdfArray());
				for (int i = 0; i < pdfArray2.Count; i++)
				{
					if (PdfCrossTable.Dereference(pdfArray2[i]) is PdfDictionary item && !list.Contains(item))
					{
						list.Add(item);
					}
				}
			}
		}
		int j = 0;
		for (int count = fields.Count; j < count; j++)
		{
			if (fields[j].Dictionary.ContainsKey("P"))
			{
				fields[j].Dictionary.Remove("P");
			}
			int index = Form.Fields.Add(fields[j], newPage);
			if (list != null && pdfArray != null && list.Contains(fields[j].Dictionary) && !pdfArray.Contains(new PdfReferenceHolder(Form.Fields[index])))
			{
				pdfArray.Add(new PdfReferenceHolder(Form.Fields[index]));
			}
		}
		if (base.EnableMemoryOptimization && Form != null && Form.Fields.Count > 0 && ldDoc.Form != null)
		{
			PdfReferenceHolder pdfReferenceHolder = ldDoc.Catalog["AcroForm"] as PdfReferenceHolder;
			if (pdfReferenceHolder != null && !ldDoc.CrossTable.PageCorrespondance.ContainsKey(pdfReferenceHolder.Reference))
			{
				PdfReference reference = base.CrossTable.GetReference(Form.Dictionary);
				ldDoc.CrossTable.PageCorrespondance.Add(pdfReferenceHolder.Reference, reference);
			}
		}
		if (pdfArray != null && pdfArray.Count > 0)
		{
			Form.Dictionary["CO"] = pdfArray;
			list?.Clear();
		}
	}

	internal override PdfPageBase ClonePage(PdfLoadedDocument ldDoc, PdfPageBase page, List<PdfArray> destinations)
	{
		return Pages.Add(ldDoc, page, destinations);
	}

	private void CloneAcroFormFontResources(PdfLoadedDocument ldDoc)
	{
		if (!(PdfCrossTable.Dereference(ldDoc.Catalog["AcroForm"]) is PdfDictionary pdfDictionary))
		{
			return;
		}
		if (PdfCrossTable.Dereference(pdfDictionary["DR"]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("Font"))
		{
			PdfDictionary pdfDictionary3 = PdfCrossTable.Dereference(pdfDictionary2["Font"]) as PdfDictionary;
			if (Form.Dictionary != null && pdfDictionary3 != null)
			{
				PdfDictionary pdfDictionary4 = null;
				if (Form.Dictionary.ContainsKey("DR"))
				{
					PdfDictionary pdfDictionary5 = PdfCrossTable.Dereference(Form.Dictionary["DR"]) as PdfDictionary;
					PdfDictionary pdfDictionary6 = PdfCrossTable.Dereference(pdfDictionary2["Font"]) as PdfDictionary;
					pdfDictionary4 = (base.EnableMemoryOptimization ? (pdfDictionary3.Clone(base.CrossTable) as PdfDictionary) : pdfDictionary3);
					if (pdfDictionary4 != null && pdfDictionary5 != null)
					{
						foreach (KeyValuePair<PdfName, IPdfPrimitive> item in pdfDictionary4.Items)
						{
							if (pdfDictionary6 != null && pdfDictionary6.Items != null && !pdfDictionary6.Items.ContainsKey(item.Key))
							{
								pdfDictionary6.Items.Add(item.Key, item.Value);
							}
							PdfDictionary pdfDictionary7 = PdfCrossTable.Dereference(pdfDictionary5.Items[new PdfName("Font")]) as PdfDictionary;
							if (pdfDictionary7 != null && !pdfDictionary7.Items.ContainsKey(item.Key))
							{
								if (PdfCrossTable.Dereference(pdfDictionary5.Items[new PdfName("Font")]) is PdfDictionary pdfDictionary8)
								{
									pdfDictionary8.Items.Add(item.Key, item.Value);
								}
							}
							else
							{
								if (pdfDictionary7 == null)
								{
									continue;
								}
								PdfDictionary pdfDictionary9 = PdfCrossTable.Dereference(pdfDictionary7.Items[item.Key]) as PdfDictionary;
								PdfName key = new PdfName("Encoding");
								if (pdfDictionary9 == null || pdfDictionary6 == null || pdfDictionary6.Items == null || !pdfDictionary9.Items.ContainsKey(key) || !pdfDictionary6.Items.ContainsKey(item.Key))
								{
									continue;
								}
								PdfName pdfName = PdfCrossTable.Dereference(pdfDictionary9.Items[key]) as PdfName;
								if (PdfCrossTable.Dereference(pdfDictionary6.Items[item.Key]) is PdfDictionary pdfDictionary10 && pdfDictionary10.ContainsKey(key))
								{
									PdfName pdfName2 = PdfCrossTable.Dereference(pdfDictionary10.Items[key]) as PdfName;
									if (pdfName != null && pdfName2 != null && pdfName.Equals(pdfName2))
									{
										pdfDictionary9.Items.Remove(new PdfName("Encoding"));
										pdfDictionary9.Items.Add(new PdfName("Encoding"), pdfName2);
										PdfResources resources = new PdfResources(pdfDictionary5);
										Form.Resources = resources;
										Form.Dictionary.SetProperty("DR", resources);
										Form.Dictionary.Modify();
									}
								}
							}
						}
					}
					else if (pdfDictionary4 != null)
					{
						foreach (KeyValuePair<PdfName, IPdfPrimitive> item2 in pdfDictionary4.Items)
						{
							if (pdfDictionary6 != null && pdfDictionary6.Items != null && !pdfDictionary6.Items.ContainsKey(item2.Key))
							{
								pdfDictionary6.Items.Add(item2.Key, item2.Value);
							}
						}
					}
					pdfDictionary6?.Modify();
				}
				else
				{
					pdfDictionary4 = ((!base.EnableMemoryOptimization) ? pdfDictionary2 : (pdfDictionary2.Clone(base.CrossTable) as PdfDictionary));
					PdfResources resources2 = new PdfResources(pdfDictionary4);
					Form.Resources = resources2;
					Form.Dictionary.SetProperty("DR", resources2);
					Form.Dictionary.Modify();
				}
			}
		}
		Form.SetAppearanceDictionary = ldDoc.Form.SetAppearanceDictionary;
		Form.NeedAppearances = ldDoc.Form.NeedAppearances;
		if (pdfDictionary.ContainsKey("NeedAppearances") && PdfCrossTable.Dereference(pdfDictionary["NeedAppearances"]) is PdfBoolean pdfBoolean)
		{
			_ = pdfBoolean.Value;
			if (pdfBoolean.Value)
			{
				Form.SetAppearanceDictionary = pdfBoolean.Value;
			}
		}
	}
}
