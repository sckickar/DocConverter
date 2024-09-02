using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;
using DocGen.Pdf.Security;

namespace DocGen.Pdf;

public abstract class PdfDocumentBase
{
	internal delegate void DocumentSavedEventHandler(object sender, DocumentSavedEventArgs args);

	public delegate void ProgressEventHandler(object sender, ProgressEventArgs arguments);

	internal delegate void AnnotationPropertyChangedEventHandler(object sender, AnnotationPropertyChangedEventArgs args);

	private class NodeInfo
	{
		public int Index;

		public PdfBookmarkBase Base;

		public List<PdfBookmarkBase> Kids;

		public NodeInfo(PdfBookmarkBase bookmarkBase, List<PdfBookmarkBase> kids)
		{
			if (bookmarkBase == null)
			{
				throw new ArgumentNullException("bookmarkBase");
			}
			if (kids == null)
			{
				throw new ArgumentNullException("kids");
			}
			Base = bookmarkBase;
			Kids = kids;
		}
	}

	private PdfMainObjectCollection m_objects;

	internal PdfArray m_lock = new PdfArray();

	internal static object s_licenseLock = new object();

	internal string m_licenseURI = string.Empty;

	private int m_progressPageIndex = -1;

	private List<PdfNamedDestination> m_namedDetinations = new List<PdfNamedDestination>();

	private List<PdfImage> m_imageCollection;

	private PdfSecurity m_security;

	private PdfReference m_currentSavingObj;

	private PdfCatalog m_catalog;

	private PdfCrossTable m_crossTable;

	private PdfDocumentInformation m_documentInfo;

	private string m_password;

	private PdfCompressionLevel m_compression = PdfCompressionLevel.Normal;

	private PdfFileStructure m_fileStructure;

	private List<IDisposable> m_disposeObjects;

	private bool m_enableMemoryOptimization;

	private bool m_wcagPDF;

	private PdfPortfolioInformation m_portfolio;

	internal PdfArray primitive = new PdfArray();

	internal int m_positon;

	internal int m_orderposition;

	internal int m_onpositon;

	internal int m_offpositon;

	internal PdfArray m_order = new PdfArray();

	internal PdfArray m_on = new PdfArray();

	internal PdfArray m_off = new PdfArray();

	internal PdfArray m_sublayer = new PdfArray();

	internal int m_sublayerposition;

	internal PdfArray m_printLayer = new PdfArray();

	internal bool m_isStreamCopied;

	internal bool m_isImported;

	private int m_annotCount;

	private bool m_isKidsPage;

	internal bool isCompressed;

	private bool m_isMerging;

	private PdfDocumentLayerCollection m_layers;

	internal ProgressEventHandler progressDelegate;

	private int m_pageProcessed = -1;

	private int m_pageCount;

	internal int m_changedPages;

	private Dictionary<string, PdfField> m_fieldKids = new Dictionary<string, PdfField>();

	private List<string> m_addedField = new List<string>();

	internal Dictionary<string, IPdfPrimitive> m_resourceCollection;

	internal Dictionary<PdfReferenceHolder, PdfDictionary> documentLayerCollection;

	private string m_baseUri = string.Empty;

	internal bool m_licensingAdded;

	private Dictionary<string, IPdfPrimitive> m_annotationCollection;

	private Dictionary<string, IPdfPrimitive> m_contentCollection;

	private Dictionary<string, IPdfPrimitive> m_extGstateCollection;

	private Dictionary<string, IPdfPrimitive> m_widgetAnnotationCollection;

	private Dictionary<string, IPdfPrimitive> m_fontCollection;

	private Dictionary<string, IPdfPrimitive> m_annotationReferenceCollection;

	internal bool m_isMergingdocument;

	internal PdfDictionary m_structElemnt = new PdfDictionary();

	internal bool m_isFirstDocument = true;

	internal List<PdfImage> ImageCollection
	{
		get
		{
			if (m_imageCollection == null)
			{
				m_imageCollection = new List<PdfImage>();
			}
			return m_imageCollection;
		}
	}

	public PdfSecurity Security
	{
		get
		{
			if (m_security == null)
			{
				m_security = new PdfSecurity();
			}
			return m_security;
		}
	}

	internal static bool IsSecurityGranted => false;

	public virtual PdfDocumentInformation DocumentInformation
	{
		get
		{
			if (m_documentInfo == null)
			{
				m_documentInfo = new PdfDocumentInformation(Catalog);
				if (((m_documentInfo != null && m_documentInfo.Dictionary.Count > 0) || PdfDocument.ConformanceLevel != 0) && PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_A4 && PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_A4E && PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_A4F)
				{
					CrossTable.Trailer["Info"] = new PdfReferenceHolder(m_documentInfo);
				}
			}
			return m_documentInfo;
		}
	}

	public PdfViewerPreferences ViewerPreferences
	{
		get
		{
			if (m_catalog.ViewerPreferences == null)
			{
				m_catalog.ViewerPreferences = new PdfViewerPreferences(m_catalog);
			}
			return m_catalog.ViewerPreferences;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("ViewerPreferences");
			}
			m_catalog.ViewerPreferences = value;
		}
	}

	public PdfCompressionLevel Compression
	{
		get
		{
			return m_compression;
		}
		set
		{
			m_compression = value;
		}
	}

	public PdfFileStructure FileStructure
	{
		get
		{
			if (m_fileStructure == null)
			{
				m_fileStructure = new PdfFileStructure();
				m_fileStructure.TaggedPdfChanged += m_fileStructure_TaggedPdfChanged;
			}
			return m_fileStructure;
		}
		set
		{
			m_fileStructure = value;
		}
	}

	public PdfPortfolioInformation PortfolioInformation
	{
		get
		{
			return m_portfolio;
		}
		set
		{
			m_portfolio = value;
			m_catalog.PdfPortfolio = m_portfolio;
		}
	}

	public abstract PdfBookmarkBase Bookmarks { get; }

	internal abstract bool WasEncrypted { get; }

	internal abstract bool IsPdfViewerDocumentDisable { get; set; }

	internal PdfMainObjectCollection PdfObjects => m_objects;

	internal PdfReference CurrentSavingObj
	{
		get
		{
			return m_currentSavingObj;
		}
		set
		{
			m_currentSavingObj = value;
		}
	}

	internal PdfCrossTable CrossTable => m_crossTable;

	internal PdfCatalog Catalog => m_catalog;

	internal List<IDisposable> DisposeObjects
	{
		get
		{
			if (m_disposeObjects == null)
			{
				m_disposeObjects = new List<IDisposable>();
			}
			return m_disposeObjects;
		}
	}

	public abstract int PageCount { get; }

	public bool EnableMemoryOptimization
	{
		get
		{
			return m_enableMemoryOptimization;
		}
		set
		{
			m_enableMemoryOptimization = value;
		}
	}

	public PdfDocumentLayerCollection Layers
	{
		get
		{
			if (m_layers == null)
			{
				m_layers = new PdfDocumentLayerCollection(this);
			}
			return m_layers;
		}
	}

	internal Dictionary<string, IPdfPrimitive> ResourceCollection
	{
		get
		{
			if (m_resourceCollection == null)
			{
				m_resourceCollection = new Dictionary<string, IPdfPrimitive>();
			}
			return m_resourceCollection;
		}
	}

	internal Dictionary<string, IPdfPrimitive> AnnotationCollection
	{
		get
		{
			if (m_annotationCollection == null)
			{
				m_annotationCollection = new Dictionary<string, IPdfPrimitive>();
			}
			return m_annotationCollection;
		}
	}

	internal Dictionary<string, IPdfPrimitive> AnnotationReferenceCollection
	{
		get
		{
			if (m_annotationReferenceCollection == null)
			{
				m_annotationReferenceCollection = new Dictionary<string, IPdfPrimitive>();
			}
			return m_annotationReferenceCollection;
		}
	}

	internal Dictionary<string, IPdfPrimitive> ContentCollection
	{
		get
		{
			if (m_contentCollection == null)
			{
				m_contentCollection = new Dictionary<string, IPdfPrimitive>();
			}
			return m_contentCollection;
		}
	}

	internal Dictionary<string, IPdfPrimitive> ExtGstateCollection
	{
		get
		{
			if (m_extGstateCollection == null)
			{
				m_extGstateCollection = new Dictionary<string, IPdfPrimitive>();
			}
			return m_extGstateCollection;
		}
	}

	internal Dictionary<string, IPdfPrimitive> WidgetAnnotationCollection
	{
		get
		{
			if (m_widgetAnnotationCollection == null)
			{
				m_widgetAnnotationCollection = new Dictionary<string, IPdfPrimitive>();
			}
			return m_widgetAnnotationCollection;
		}
	}

	internal Dictionary<string, IPdfPrimitive> FontCollection
	{
		get
		{
			if (m_fontCollection == null)
			{
				m_fontCollection = new Dictionary<string, IPdfPrimitive>();
			}
			return m_fontCollection;
		}
	}

	public string BaseUri
	{
		get
		{
			return ObtainBaseURI();
		}
		set
		{
			SetBaseURI(value);
		}
	}

	internal bool IsWCAGCloned => m_wcagPDF;

	internal event AnnotationPropertyChangedEventHandler AnnotationPropertyChanged;

	internal event DocumentSavedEventHandler DocumentSaved;

	public event ProgressEventHandler SaveProgress
	{
		add
		{
			progressDelegate = Delegate.Combine(progressDelegate, value) as ProgressEventHandler;
			if (progressDelegate != null)
			{
				SetProgress();
			}
		}
		remove
		{
			progressDelegate = Delegate.Remove(progressDelegate, value) as ProgressEventHandler;
			if (progressDelegate == null)
			{
				ResetProgress();
			}
		}
	}

	internal event EventHandler<AnnotationAddedArgs> AnnotationAdded;

	internal event EventHandler<AnnotationRemovedArgs> AnnotationRemoved;

	internal virtual void OnAnnotationAdded(AnnotationAddedArgs e)
	{
		this.AnnotationAdded?.Invoke(this, e);
	}

	internal virtual void OnAnnotationRemoved(AnnotationRemovedArgs e)
	{
		this.AnnotationRemoved?.Invoke(this, e);
	}

	internal void OnAnnotationPropertyChanged(PdfAnnotation annotation, string propertyName)
	{
		if (this.AnnotationPropertyChanged != null)
		{
			this.AnnotationPropertyChanged(this, new AnnotationPropertyChangedEventArgs(annotation, propertyName));
		}
	}

	private void m_fileStructure_TaggedPdfChanged(object sender, EventArgs e)
	{
		if (m_fileStructure.TaggedPdf)
		{
			Catalog.InitializeStructTreeRoot();
		}
	}

	public static PdfDocumentBase Merge(PdfDocumentBase dest, PdfMergeOptions options, params object[] sourceDocuments)
	{
		if (dest == null)
		{
			dest = new PdfDocument(isMerging: true);
		}
		if (dest is PdfDocument && (dest as PdfDocument).Sections.Count > 0)
		{
			(dest as PdfDocument).IsMergeDocHasSections = true;
		}
		int i = 0;
		for (int num = sourceDocuments.Length; i < num; i++)
		{
			object obj = sourceDocuments[i];
			string obj2 = obj as string;
			Stream stream = obj as Stream;
			byte[] array = obj as byte[];
			PdfLoadedDocument pdfLoadedDocument = obj as PdfLoadedDocument;
			bool flag = true;
			if (obj2 == null)
			{
				if (stream != null)
				{
					pdfLoadedDocument = new PdfLoadedDocument(stream);
				}
				else if (array != null)
				{
					pdfLoadedDocument = new PdfLoadedDocument(array);
				}
				else
				{
					if (pdfLoadedDocument == null)
					{
						throw new ArgumentException("Unsupported argument type: " + obj.GetType());
					}
					flag = false;
				}
			}
			pdfLoadedDocument.IsOptimizeIdentical = options.OptimizeResources;
			pdfLoadedDocument.DestinationDocument = dest;
			pdfLoadedDocument.IsPDFMerge = options.MergeAccessibilityTags;
			if (!options.MergeAccessibilityTags)
			{
				pdfLoadedDocument.IsExtendMargin = options.ExtendMargin;
			}
			dest.Append(pdfLoadedDocument);
			if (dest is PdfDocument && (dest as PdfDocument).Form != null && pdfLoadedDocument.Form != null)
			{
				if (pdfLoadedDocument.Form.IsXFAForm)
				{
					(dest as PdfDocument).Form.NeedAppearances = true;
					(dest as PdfDocument).Form.IsXFA = true;
				}
				else if (pdfLoadedDocument.IsXFAForm)
				{
					(dest as PdfDocument).Form.NeedAppearances = true;
					(dest as PdfDocument).Form.IsXFA = true;
				}
			}
			if (flag && dest.EnableMemoryOptimization)
			{
				pdfLoadedDocument.Close(completely: true);
			}
		}
		if (options.MergeAccessibilityTags && dest.m_structElemnt != null)
		{
			dest.Catalog["StructTreeRoot"] = new PdfReferenceHolder(dest.m_structElemnt);
		}
		if (dest is PdfDocument)
		{
			(dest as PdfDocument).IsMergeDocHasSections = true;
		}
		return dest;
	}

	public static PdfDocumentBase Merge(PdfDocumentBase dest, params object[] sourceDocuments)
	{
		PdfMergeOptions options = new PdfMergeOptions();
		return Merge(dest, options, sourceDocuments);
	}

	public static PdfDocumentBase Merge(PdfDocumentBase dest, PdfLoadedDocument src)
	{
		if (src == null)
		{
			throw new ArgumentNullException("src");
		}
		if (dest == null)
		{
			dest = new PdfDocument(isMerging: true);
		}
		dest.Append(src);
		if (dest is PdfDocument && (dest as PdfDocument).Sections.Count > 0)
		{
			(dest as PdfDocument).IsMergeDocHasSections = true;
		}
		if (dest is PdfDocument && (dest as PdfDocument).Form != null)
		{
			if (src.IsXFAForm)
			{
				(dest as PdfDocument).Form.NeedAppearances = true;
				(dest as PdfDocument).Form.IsXFA = true;
			}
			else if (src.Form != null && src.Form.IsXFAForm)
			{
				(dest as PdfDocument).Form.NeedAppearances = true;
				(dest as PdfDocument).Form.IsXFA = true;
			}
		}
		if (dest is PdfDocument)
		{
			(dest as PdfDocument).IsMergeDocHasSections = true;
		}
		return dest;
	}

	public void DisposeOnClose(IDisposable obj)
	{
		if (obj != null)
		{
			DisposeObjects.Add(obj);
		}
	}

	public void Close()
	{
		Close(completely: false);
	}

	public virtual void Close(bool completely)
	{
		m_objects = null;
		m_currentSavingObj = null;
		if (m_catalog != null && completely && EnableMemoryOptimization)
		{
			m_catalog.Clear();
			m_catalog = null;
		}
		if (m_crossTable != null)
		{
			m_crossTable.isDisposed = completely;
			m_crossTable.m_closeCompletely = completely;
			if (m_crossTable.PrevCloneReference != null)
			{
				m_crossTable.PrevCloneReference.Clear();
			}
		}
		if (EnableMemoryOptimization)
		{
			if (m_crossTable != null)
			{
				m_crossTable.Close(completely: true);
			}
		}
		else if (completely && m_crossTable != null)
		{
			m_crossTable.Dispose();
		}
		m_crossTable = null;
		m_fieldKids.Clear();
		m_addedField.Clear();
		if (documentLayerCollection != null)
		{
			documentLayerCollection.Clear();
			documentLayerCollection = null;
		}
		m_documentInfo = null;
		m_compression = PdfCompressionLevel.Normal;
		if (PdfCatalog.m_structTreeRoot != null)
		{
			PdfCatalog.m_structTreeRoot = null;
		}
		if (m_disposeObjects != null)
		{
			int i = 0;
			for (int count = m_disposeObjects.Count; i < count; i++)
			{
				m_disposeObjects[i]?.Dispose();
			}
			m_disposeObjects.Clear();
			m_disposeObjects = null;
		}
		if (m_resourceCollection != null)
		{
			m_resourceCollection.Clear();
		}
		if (m_annotationCollection != null)
		{
			m_annotationCollection.Clear();
		}
		if (m_widgetAnnotationCollection != null)
		{
			m_widgetAnnotationCollection.Clear();
		}
		if (m_extGstateCollection != null)
		{
			m_extGstateCollection.Clear();
		}
		if (m_contentCollection != null)
		{
			m_contentCollection.Clear();
		}
		if (m_annotationReferenceCollection != null)
		{
			m_annotationReferenceCollection.Clear();
		}
		if (m_fontCollection != null)
		{
			m_fontCollection.Clear();
		}
		if (ImageCollection != null)
		{
			foreach (PdfImage item in ImageCollection)
			{
				if (item != null)
				{
					if (item.ImageStream != null)
					{
						item.ImageStream.Dispose();
						item.ImageStream = null;
					}
					if (item is PdfBitmap pdfBitmap)
					{
						pdfBitmap.Close();
					}
				}
			}
			if (m_imageCollection != null)
			{
				m_imageCollection.Clear();
				m_imageCollection = null;
			}
		}
		PdfLoadedDocument pdfLoadedDocument = this as PdfLoadedDocument;
		if ((pdfLoadedDocument != null && pdfLoadedDocument.ConformanceEnabled && PdfDocument.ConformanceLevel != 0) || pdfLoadedDocument == null)
		{
			PdfDocument.ConformanceLevel = PdfConformanceLevel.None;
		}
		if (PdfDocument.EnableCache)
		{
			PdfDocument.Cache.Clear();
			PdfDocument.Cache = null;
		}
		if (PdfDocument.EnableCache || PdfDocument.Cache == null || PdfDocument.Cache.ReferenceObjects == null || PdfDocument.Cache.ReferenceObjects.Count <= 0)
		{
			return;
		}
		List<List<object>> referenceObjects = PdfDocument.Cache.ReferenceObjects;
		if (referenceObjects != null)
		{
			int j = 0;
			for (int count2 = referenceObjects.Count; j < count2; j++)
			{
				referenceObjects[j].Clear();
			}
			referenceObjects.Clear();
		}
	}

	public abstract void Save(Stream stream);

	public PdfPageBase ImportPage(PdfLoadedDocument ldDoc, PdfPageBase page)
	{
		if (ldDoc == null)
		{
			throw new ArgumentNullException("ldDoc");
		}
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		int pageIndex = ldDoc.Pages.IndexOf(page);
		return ImportPage(ldDoc, pageIndex);
	}

	public PdfPageBase ImportPage(PdfLoadedDocument ldDoc, int pageIndex)
	{
		if (ldDoc == null)
		{
			throw new ArgumentNullException("ldDoc");
		}
		if (pageIndex < 0 || pageIndex >= ldDoc.Pages.Count)
		{
			throw new ArgumentOutOfRangeException("pageIndex");
		}
		return ImportPageRange(ldDoc, pageIndex, pageIndex);
	}

	public PdfPageBase ImportPageRange(PdfLoadedDocument ldDoc, int startIndex, int endIndex)
	{
		if (ldDoc == null)
		{
			throw new ArgumentNullException("ldDoc");
		}
		if (startIndex > endIndex)
		{
			throw new ArgumentException("The start index is greater then the end index, which might indicate the error in the program.");
		}
		return ImportPageRange(ldDoc, startIndex, endIndex, importBookmarks: true);
	}

	public PdfPageBase ImportPageRange(PdfLoadedDocument ldDoc, int startIndex, int endIndex, bool importBookmarks)
	{
		if (ldDoc == null)
		{
			throw new ArgumentNullException("ldDoc");
		}
		if (startIndex > endIndex)
		{
			throw new ArgumentException("The start index is greater then the end index, which might indicate the error in the program.");
		}
		m_isImported = true;
		PdfPageBase pdfPageBase = null;
		PdfLoadedPageCollection pages = ldDoc.Pages;
		if (this is PdfLoadedDocument)
		{
			PdfLoadedDocument pdfLoadedDocument = this as PdfLoadedDocument;
			if (!pdfLoadedDocument.m_duplicatePage)
			{
				foreach (PdfPageBase item in pages)
				{
					if (pdfLoadedDocument.Pages.IndexOf(item) == startIndex && pdfLoadedDocument.Pages.IndexOf(item) >= 0)
					{
						return null;
					}
				}
			}
		}
		if (ldDoc.CrossTable.DocumentCatalog.ContainsKey("Pages"))
		{
			PdfDictionary pdfDictionary = (ldDoc.CrossTable.DocumentCatalog["Pages"] as PdfReferenceHolder).Object as PdfDictionary;
			PdfReferenceHolder pdfReferenceHolder = pdfDictionary["Kids"] as PdfReferenceHolder;
			if (pdfReferenceHolder != null)
			{
				_ = pdfReferenceHolder.Object;
			}
			else
			{
				_ = pdfDictionary["Kids"];
			}
		}
		if (endIndex >= pages.Count || startIndex >= pages.Count)
		{
			throw new ArgumentException("Either or both indices are out of range", "endIndex, startIndex");
		}
		ldDoc.CrossTable.IsPDFAppend = true;
		List<PdfField> list = new List<PdfField>();
		List<PdfBookmarkBase> list2 = new List<PdfBookmarkBase>();
		List<PdfArray> list3 = new List<PdfArray>();
		Dictionary<IPdfPrimitive, object> pageCorrespondance = ldDoc.CrossTable.PageCorrespondance;
		Dictionary<int, PdfPageBase> dictionary = new Dictionary<int, PdfPageBase>();
		Dictionary<PdfPageBase, object> dictionary2 = null;
		bool flag = false;
		if (importBookmarks)
		{
			dictionary2 = ldDoc.CreateBookmarkDestinationDictionary();
			flag = dictionary2 != null && dictionary2.Count > 0;
		}
		bool flag2 = false;
		ldDoc.m_isNamedDestinationCall = false;
		if (ldDoc.NamedDestinationCollection != null && ldDoc.NamedDestinationCollection.Count > 0)
		{
			flag2 = true;
		}
		ldDoc.m_isNamedDestinationCall = true;
		int num = 0;
		if ((ldDoc.IsPDFSplit || ldDoc.IsPDFMerge) && ldDoc.Catalog.ContainsKey("StructTreeRoot"))
		{
			if (PdfCrossTable.Dereference(ldDoc.Catalog["StructTreeRoot"]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("K"))
			{
				m_wcagPDF = true;
				ldDoc.CrossTable.IsTagged = m_wcagPDF;
			}
			else
			{
				m_wcagPDF = false;
			}
		}
		else
		{
			m_wcagPDF = false;
		}
		for (int i = startIndex; i <= endIndex; i++)
		{
			PdfPageBase pdfPageBase2 = pages[i];
			dictionary.Add(i, pdfPageBase2);
			pdfPageBase2.importPageStartIndex = startIndex;
			pdfPageBase2.importPageEndIndex = endIndex;
			pdfPageBase2.IsTagged = m_wcagPDF;
			ldDoc.m_isPageMerging = true;
			PdfPageBase pdfPageBase3 = ClonePage(ldDoc, pdfPageBase2, list3);
			pdfPageBase3.Imported = true;
			pdfPageBase3.Rotation = pdfPageBase2.Rotation;
			pageCorrespondance[((IPdfWrapper)pdfPageBase2).Element] = pdfPageBase3;
			num++;
			if (flag && importBookmarks && !ldDoc.m_duplicatePage)
			{
				List<object> list4 = (dictionary2.ContainsKey(pdfPageBase2) ? (dictionary2[pdfPageBase2] as List<object>) : null);
				if (list4 != null)
				{
					MarkBookmarks(list4, list2);
				}
			}
			if (pdfPageBase2.Dictionary.ContainsKey("Group") && PdfCrossTable.Dereference(pdfPageBase2.Dictionary["Group"]) is PdfDictionary pdfDictionary3)
			{
				pdfPageBase3.Dictionary.SetProperty("Group", pdfDictionary3);
			}
			if (ldDoc.m_duplicatePage && pdfPageBase2.Dictionary.ContainsKey("MediaBox") && pdfPageBase2.Dictionary["MediaBox"] is PdfArray pdfArray)
			{
				pdfPageBase3.Dictionary.SetProperty("MediaBox", pdfArray);
			}
			if (pdfPageBase2.Dictionary.ContainsKey("Resources"))
			{
				pdfPageBase = pdfPageBase3;
				if (!ldDoc.IsUnusedResourceSplit || dictionary[i].Dictionary == null || !dictionary[i].Dictionary.ContainsKey("Resources"))
				{
					continue;
				}
				PdfRecordCollection recordCollection = null;
				using (new MemoryStream())
				{
					PdfPageBase page2 = dictionary[i];
					Page page3 = new Page(page2);
					page3.Initialize(page2, needParsing: false);
					if (page3.RecordCollection == null)
					{
						page3.Initialize(dictionary[i], needParsing: true);
					}
					recordCollection = page3.RecordCollection;
				}
				PdfDictionary resources = PdfCrossTable.Dereference(dictionary[i].Dictionary["Resources"]) as PdfDictionary;
				PdfDictionary value = new RemoveUnusedResources().RemoveUnusedResource(recordCollection, resources);
				pdfPageBase.Dictionary["Resources"] = value;
			}
			else
			{
				if (!pdfPageBase2.Dictionary.ContainsKey("Parent"))
				{
					continue;
				}
				PdfDictionary pdfDictionary4 = (pdfPageBase2.Dictionary["Parent"] as PdfReferenceHolder).Object as PdfDictionary;
				if (!pdfDictionary4.ContainsKey("Resources"))
				{
					continue;
				}
				PdfResources pdfResources = null;
				if (pdfDictionary4["Resources"] is PdfReferenceHolder)
				{
					if ((pdfDictionary4["Resources"] as PdfReferenceHolder).Object is PdfDictionary)
					{
						pdfResources = new PdfResources((pdfDictionary4["Resources"] as PdfReferenceHolder).Object as PdfDictionary);
					}
				}
				else
				{
					pdfResources = new PdfResources(pdfDictionary4["Resources"] as PdfDictionary);
				}
				if (pdfResources == null || !(pdfPageBase3 as PdfPage).Dictionary.ContainsKey("Resources"))
				{
					continue;
				}
				(pdfPageBase3 as PdfPage).Dictionary.Remove("Resources");
				PdfDictionary pdfDictionary5 = null;
				pdfDictionary5 = ((!(pdfDictionary4["Resources"] is PdfReferenceHolder)) ? (pdfDictionary4["Resources"] as PdfDictionary) : ((pdfDictionary4["Resources"] as PdfReferenceHolder).Object as PdfDictionary));
				if (pdfDictionary5 != null)
				{
					PdfDictionary obj = (EnableMemoryOptimization ? (pdfDictionary5.Clone(CrossTable) as PdfDictionary) : pdfDictionary5);
					(pdfPageBase3 as PdfPage).Dictionary["Resources"] = new PdfReferenceHolder(obj);
				}
				pdfPageBase3.Contents.Clear();
				foreach (IPdfPrimitive content in pdfPageBase2.Contents)
				{
					if (EnableMemoryOptimization)
					{
						pdfPageBase3.Contents.Add(content.Clone(m_crossTable));
					}
					else
					{
						pdfPageBase3.Contents.Add(content);
					}
				}
				(pdfPageBase3 as PdfPage).Dictionary.Modify();
			}
		}
		for (int j = startIndex; j <= endIndex; j++)
		{
			list = new List<PdfField>();
			PdfPageBase pdfPageBase4 = dictionary[j];
			PdfPageBase pdfPageBase5 = pageCorrespondance[((IPdfWrapper)pdfPageBase4).Element] as PdfPageBase;
			if (EnableMemoryOptimization && pdfPageBase4.Dictionary.ContainsKey("Annots"))
			{
				(pdfPageBase5 as PdfPage).ImportAnnotations(ldDoc, pdfPageBase4, list3);
				m_annotCount = (pdfPageBase5 as PdfPage).FieldsCount;
			}
			if (pdfPageBase4.Dictionary.ContainsKey("Annots"))
			{
				CheckFields(ldDoc, pdfPageBase4, list, pdfPageBase5);
			}
			if (list.Count > 0)
			{
				AddFields(ldDoc, pdfPageBase5, list);
				list.Clear();
				PdfForm pdfForm = ObtainForm();
				if (pdfForm != null && !pdfForm.m_pageMap.ContainsKey(pdfPageBase4.Dictionary))
				{
					pdfForm.m_pageMap.Add(pdfPageBase4.Dictionary, pdfPageBase5);
				}
			}
			else if (m_isKidsPage)
			{
				PdfForm pdfForm2 = ObtainForm();
				if (pdfForm2 != null && !pdfForm2.m_pageMap.ContainsKey(pdfPageBase4.Dictionary))
				{
					pdfForm2.m_pageMap.Add(pdfPageBase4.Dictionary, pdfPageBase5);
				}
				m_isKidsPage = false;
			}
		}
		FixDestinations(pageCorrespondance, list3);
		if (flag && importBookmarks && !ldDoc.m_duplicatePage)
		{
			m_namedDetinations.Clear();
			ExportBookmarks(ldDoc, list2, num, dictionary2);
			Bookmarks.CrossTable.Document = this;
			if (m_namedDetinations.Count > 0)
			{
				if (Bookmarks.CrossTable.Document is PdfLoadedDocument)
				{
					PdfNamedDestinationCollection namedDestinationCollection = (Bookmarks.CrossTable.Document as PdfLoadedDocument).NamedDestinationCollection;
					foreach (PdfNamedDestination namedDetination in m_namedDetinations)
					{
						namedDestinationCollection.Add(namedDetination);
					}
				}
				else
				{
					PdfNamedDestinationCollection namedDestinationCollection2 = (Bookmarks.CrossTable.Document as PdfDocument).NamedDestinationCollection;
					foreach (PdfNamedDestination namedDetination2 in m_namedDetinations)
					{
						namedDestinationCollection2.Add(namedDetination2);
					}
				}
			}
		}
		if (flag2)
		{
			bool flag3 = false;
			if (ldDoc.Bookmarks != null)
			{
				foreach (PdfBookmark item2 in (IEnumerable)ldDoc.Bookmarks)
				{
					if (item2.List == null)
					{
						continue;
					}
					if (item2.List.Count > 0)
					{
						for (int k = 0; k < item2.List.Count; k++)
						{
							PdfBookmark pdfBookmark2 = item2.List[k] as PdfBookmark;
							if (pdfBookmark2.NamedDestination != null && pdfBookmark2.NamedDestination != null)
							{
								flag3 = true;
								break;
							}
						}
					}
					else if (item2.NamedDestination != null && item2.NamedDestination != null)
					{
						flag3 = true;
						break;
					}
				}
			}
			if (!flag3)
			{
				m_namedDetinations.Clear();
				ExportNamedDestination(ldDoc);
				if (CrossTable.Document is PdfLoadedDocument)
				{
					PdfNamedDestinationCollection namedDestinationCollection3 = (CrossTable.Document as PdfLoadedDocument).NamedDestinationCollection;
					foreach (PdfNamedDestination namedDetination3 in m_namedDetinations)
					{
						namedDestinationCollection3.Add(namedDetination3);
					}
				}
				else
				{
					PdfNamedDestinationCollection namedDestinationCollection4 = (CrossTable.Document as PdfDocument).NamedDestinationCollection;
					foreach (PdfNamedDestination namedDetination4 in m_namedDetinations)
					{
						namedDestinationCollection4.Add(namedDetination4);
					}
				}
			}
			PdfDocument pdfDocument = CrossTable.Document as PdfDocument;
			if (pdfDocument != null && ldDoc != null && ldDoc.Bookmarks != null && m_isMerging && flag3 && ldDoc.NamedDestinationCollection != null && m_namedDetinations.Count < ldDoc.NamedDestinationCollection.Count)
			{
				PdfNamedDestinationCollection namedDestinationCollection5 = pdfDocument.NamedDestinationCollection;
				foreach (PdfNamedDestination item3 in (IEnumerable)ldDoc.NamedDestinationCollection)
				{
					if (!namedDestinationCollection5.Contains(item3))
					{
						PdfNamedDestination pdfNamedDestination2 = item3;
						if (pdfNamedDestination2.Destination != null && pdfNamedDestination2.Destination.Page != null && pageCorrespondance.ContainsKey(pdfNamedDestination2.Destination.Page.Dictionary) && pageCorrespondance[pdfNamedDestination2.Destination.Page.Dictionary] is PdfPageBase page4)
						{
							pdfNamedDestination2 = GetNamedDestination(pdfNamedDestination2, page4);
						}
						namedDestinationCollection5.Add(pdfNamedDestination2);
					}
				}
			}
		}
		if (!m_isMerging && (!m_isImported || flag) && ldDoc.CrossTable.DocumentCatalog.ContainsKey("OCProperties"))
		{
			int num2 = 0;
			PdfArray pdfArray2 = new PdfArray();
			PdfDictionary pdfDictionary6 = new PdfDictionary();
			PdfArray pdfArray3 = new PdfArray();
			PdfArray pdfArray4 = new PdfArray();
			PdfDictionary pdfDictionary7 = new PdfDictionary();
			pdfDictionary7.SetProperty("Event", new PdfName("Print"));
			pdfDictionary6["Name"] = new PdfString("Layers");
			PdfArray pdfArray5 = new PdfArray();
			pdfArray5.Add(new PdfName("Print"));
			pdfDictionary7.SetProperty("Category", pdfArray5);
			for (int l = startIndex; l <= endIndex; l++)
			{
				PdfPageBase pdfPageBase6 = dictionary[l];
				if (pdfPageBase6.Dictionary.ContainsKey("Resources") && pdfPageBase6.Dictionary.Items[new PdfName("Resources")] is PdfDictionary pdfDictionary8 && pdfDictionary8.ContainsKey("Properties") && pdfDictionary8.Items[new PdfName("Properties")] is PdfDictionary pdfDictionary9)
				{
					PdfDictionary pdfDictionary10 = new PdfDictionary();
					foreach (KeyValuePair<PdfName, IPdfPrimitive> item4 in pdfDictionary9.Items)
					{
						if (!(item4.Value is PdfReferenceHolder) || !((item4.Value as PdfReferenceHolder).Object is PdfDictionary pdfDictionary11) || !pdfDictionary11.ContainsKey("Type"))
						{
							continue;
						}
						string value2 = (pdfDictionary11.Items[new PdfName("Type")] as PdfName).Value;
						if (value2 != null && value2.Equals("OCG"))
						{
							IPdfPrimitive pdfPrimitive2 = item4.Value;
							if (EnableMemoryOptimization)
							{
								pdfPrimitive2 = item4.Value.Clone(CrossTable);
							}
							pdfDictionary10.Items.Add(item4.Key, pdfPrimitive2);
							pdfArray2.Insert(num2, pdfPrimitive2);
							PdfArray pdfArray6 = new PdfArray();
							pdfArray3.Insert(num2, pdfPrimitive2);
							pdfArray4.Insert(num2, pdfPrimitive2);
							pdfDictionary6["Order"] = pdfArray3;
							pdfDictionary6["ON"] = pdfArray4;
							PdfArray value3 = new PdfArray();
							pdfDictionary6["OFF"] = value3;
							pdfDictionary7.SetProperty("OCGs", pdfArray2);
							pdfArray6.Add(new PdfReferenceHolder(pdfDictionary7));
							pdfDictionary6["AS"] = pdfArray6;
							num2++;
						}
					}
					foreach (KeyValuePair<PdfName, IPdfPrimitive> item5 in pdfDictionary10.Items)
					{
						if (pdfDictionary9.Items.ContainsKey(item5.Key))
						{
							pdfDictionary9.Items[item5.Key] = item5.Value;
						}
					}
				}
				if (!pdfPageBase6.Dictionary.ContainsKey("Annots") || !(PdfCrossTable.Dereference(pdfPageBase6.Dictionary.Items[new PdfName("Annots")]) is PdfArray pdfArray7))
				{
					continue;
				}
				for (int m = 0; m <= pdfArray7.Count - 1; m++)
				{
					if (!(PdfCrossTable.Dereference(pdfArray7[m]) is PdfDictionary pdfDictionary12))
					{
						continue;
					}
					PdfDictionary pdfDictionary13 = null;
					foreach (KeyValuePair<PdfName, IPdfPrimitive> item6 in pdfDictionary12.Items)
					{
						if (!(item6.Key.Value == "OC"))
						{
							continue;
						}
						pdfDictionary13 = (item6.Value as PdfReferenceHolder).Object as PdfDictionary;
						foreach (KeyValuePair<PdfName, IPdfPrimitive> item7 in pdfDictionary13.Items)
						{
							if (pdfDictionary13 == null || !(item7.Key.Value == "Type"))
							{
								continue;
							}
							string value4 = (pdfDictionary13.Items[new PdfName("Type")] as PdfName).Value;
							if (value4 != null && value4.Equals("OCG"))
							{
								IPdfPrimitive element = item6.Value;
								if (EnableMemoryOptimization)
								{
									element = item6.Value.Clone(CrossTable);
								}
								pdfArray2.Insert(num2, element);
								PdfArray pdfArray8 = new PdfArray();
								pdfArray3.Insert(num2, element);
								pdfArray4.Insert(num2, element);
								pdfDictionary6["Order"] = pdfArray3;
								pdfDictionary6["ON"] = pdfArray4;
								PdfArray value5 = new PdfArray();
								pdfDictionary6["OFF"] = value5;
								pdfDictionary7.SetProperty("OCGs", pdfArray2);
								pdfArray8.Add(new PdfReferenceHolder(pdfDictionary7));
								pdfDictionary6["AS"] = pdfArray8;
								num2++;
							}
						}
						break;
					}
				}
			}
			PdfDictionary pdfDictionary14 = new PdfDictionary();
			pdfDictionary14.Items.Add(new PdfName("OCGs"), pdfArray2);
			pdfDictionary14.Items.Add(new PdfName("D"), pdfDictionary6);
			if (CrossTable.Document is PdfDocument)
			{
				if (!(CrossTable.Document as PdfDocument).Catalog.ContainsKey("OCProperties"))
				{
					(CrossTable.Document as PdfDocument).Catalog.SetProperty("OCProperties", pdfDictionary14);
				}
				else
				{
					PdfDictionary ocPropertiesDictionary = (CrossTable.Document as PdfDocument).Catalog.Items[new PdfName("OCProperties")] as PdfDictionary;
					ImportLayers(ocPropertiesDictionary, pdfArray2, pdfDictionary6);
				}
			}
			else if (!(CrossTable.Document as PdfLoadedDocument).Catalog.ContainsKey("OCProperties"))
			{
				(CrossTable.Document as PdfLoadedDocument).Catalog.SetProperty("OCProperties", pdfDictionary14);
			}
			else
			{
				PdfDictionary ocPropertiesDictionary2 = (CrossTable.Document as PdfLoadedDocument).Catalog.Items[new PdfName("OCProperties")] as PdfDictionary;
				ImportLayers(ocPropertiesDictionary2, pdfArray2, pdfDictionary6);
			}
		}
		if (!m_isMerging && m_isImported && !flag)
		{
			MergeLayer(ldDoc);
		}
		if (m_wcagPDF && (ldDoc.IsPDFSplit || ldDoc.IsPDFMerge))
		{
			WCAGCloner wCAGCloner = new WCAGCloner(Catalog, this);
			wCAGCloner.ImportingStructureElements(startIndex, endIndex, ldDoc, pageCorrespondance, dictionary);
			wCAGCloner.Dispose();
		}
		list2.Clear();
		list3.Clear();
		pageCorrespondance = null;
		dictionary.Clear();
		CrossTable.PrevReference = null;
		return pdfPageBase;
	}

	private void ImportLayers(PdfDictionary ocPropertiesDictionary, PdfArray ocgArray, PdfDictionary optimalContentViewDictionary)
	{
		if (!ocPropertiesDictionary.ContainsKey("OCGs"))
		{
			return;
		}
		PdfArray pdfArray = ocPropertiesDictionary.Items[new PdfName("OCGs")] as PdfArray;
		if (!(ocPropertiesDictionary.Items[new PdfName("D")] is PdfDictionary pdfDictionary))
		{
			return;
		}
		if (pdfArray != null && ocgArray != null)
		{
			int num = pdfArray.Count;
			for (int i = 0; i < ocgArray.Count; i++)
			{
				pdfArray.Insert(num, ocgArray[i]);
				num++;
			}
		}
		PdfArray pdfArray2 = optimalContentViewDictionary["AS"] as PdfArray;
		pdfDictionary["Order"] = pdfArray;
		pdfDictionary["ON"] = pdfArray;
		if (!pdfDictionary.ContainsKey("AS"))
		{
			return;
		}
		PdfArray pdfArray3 = pdfDictionary["AS"] as PdfArray;
		if (pdfArray2 != null && pdfArray3 != null)
		{
			for (int j = 0; j < pdfArray2.Count; j++)
			{
				pdfArray3.Insert(pdfArray3.Count, pdfArray2[j]);
			}
		}
	}

	private PdfDictionary ParsePdfLayers(PdfDictionary lDocLayers)
	{
		PdfDictionary result = new PdfDictionary();
		PdfDictionary pdfDictionary = null;
		pdfDictionary = ((!(lDocLayers["OCProperties"] is PdfReferenceHolder)) ? (lDocLayers["OCProperties"] as PdfDictionary) : ((lDocLayers["OCProperties"] as PdfReferenceHolder).Object as PdfDictionary));
		if (pdfDictionary != null)
		{
			result = ((!EnableMemoryOptimization) ? pdfDictionary : (pdfDictionary.Clone(CrossTable) as PdfDictionary));
		}
		return result;
	}

	public void Append(PdfLoadedDocument ldDoc)
	{
		if (ldDoc == null)
		{
			throw new ArgumentNullException("ldDoc");
		}
		if (ldDoc.IsXFAForm && this is PdfDocument)
		{
			((PdfDocument)this).Form.IsXFA = true;
		}
		if (!ldDoc.IsXFAForm && ldDoc.Form != null)
		{
			_ = ldDoc.Form.IsXFAForm;
		}
		int startIndex = 0;
		int endIndex = ldDoc.Pages.Count - 1;
		m_isMerging = true;
		m_crossTable.Document.m_isMergingdocument = true;
		ImportPageRange(ldDoc, startIndex, endIndex);
		MergeLayer(ldDoc);
		MergeAttachments(ldDoc);
	}

	private void MergeLayer(PdfLoadedDocument ldDoc)
	{
		if (!ldDoc.CrossTable.DocumentCatalog.ContainsKey("OCProperties"))
		{
			return;
		}
		PdfDictionary pdfDictionary = null;
		PdfDictionary pdfDictionary2 = ParsePdfLayers(ldDoc.CrossTable.DocumentCatalog);
		if (this is PdfDocument)
		{
			PdfDocument pdfDocument = this as PdfDocument;
			if (!pdfDocument.Catalog.ContainsKey("OCProperties"))
			{
				pdfDocument.Catalog.SetProperty("OCProperties", pdfDictionary2);
			}
			else
			{
				pdfDictionary = PdfCrossTable.Dereference(pdfDocument.Catalog["OCProperties"]) as PdfDictionary;
			}
		}
		else
		{
			PdfLoadedDocument pdfLoadedDocument = this as PdfLoadedDocument;
			if (!pdfLoadedDocument.Catalog.ContainsKey("OCProperties"))
			{
				pdfLoadedDocument.Catalog.SetProperty("OCProperties", pdfDictionary2);
			}
			else
			{
				pdfDictionary = PdfCrossTable.Dereference(pdfLoadedDocument.Catalog["OCProperties"]) as PdfDictionary;
			}
			pdfDictionary?.Modify();
		}
		if (pdfDictionary == null || pdfDictionary2 == null)
		{
			return;
		}
		if (pdfDictionary.ContainsKey("OCGs") && pdfDictionary2.ContainsKey("OCGs"))
		{
			PdfArray pdfArray = PdfCrossTable.Dereference(pdfDictionary["OCGs"]) as PdfArray;
			PdfArray pdfArray2 = PdfCrossTable.Dereference(pdfDictionary2["OCGs"]) as PdfArray;
			if (pdfArray != null && pdfArray2 != null)
			{
				pdfArray.Elements.AddRange(pdfArray2.Elements);
			}
			else if (pdfArray2 != null)
			{
				pdfDictionary.SetProperty("OCGs", pdfArray2);
			}
		}
		else if (pdfDictionary2.ContainsKey("OCGs"))
		{
			pdfDictionary.SetProperty("OCGs", pdfDictionary2);
		}
		if (!pdfDictionary.ContainsKey("D") || !pdfDictionary2.ContainsKey("D"))
		{
			return;
		}
		PdfDictionary pdfDictionary3 = PdfCrossTable.Dereference(pdfDictionary["D"]) as PdfDictionary;
		PdfDictionary pdfDictionary4 = PdfCrossTable.Dereference(pdfDictionary2["D"]) as PdfDictionary;
		if (pdfDictionary3 != null && pdfDictionary4 != null)
		{
			if (pdfDictionary3.ContainsKey("Order") && pdfDictionary4.ContainsKey("Order"))
			{
				PdfArray pdfArray3 = PdfCrossTable.Dereference(pdfDictionary3["Order"]) as PdfArray;
				PdfArray pdfArray4 = PdfCrossTable.Dereference(pdfDictionary4["Order"]) as PdfArray;
				if (pdfArray3 != null && pdfArray4 != null)
				{
					pdfArray3.Elements.AddRange(pdfArray4.Elements);
				}
			}
			else if (pdfDictionary4.ContainsKey("Order"))
			{
				pdfDictionary3.SetProperty("D", pdfDictionary4["Order"] as PdfDictionary);
			}
			if (pdfDictionary3.ContainsKey("ON") && pdfDictionary4.ContainsKey("ON"))
			{
				PdfArray pdfArray5 = PdfCrossTable.Dereference(pdfDictionary3["ON"]) as PdfArray;
				PdfArray pdfArray6 = PdfCrossTable.Dereference(pdfDictionary4["ON"]) as PdfArray;
				if (pdfArray5 != null && pdfArray6 != null)
				{
					pdfArray5.Elements.AddRange(pdfArray6.Elements);
				}
			}
			else if (pdfDictionary4.ContainsKey("ON"))
			{
				pdfDictionary3.SetProperty("ON", pdfDictionary4["ON"] as PdfDictionary);
			}
			if (pdfDictionary3.ContainsKey("OFF") && pdfDictionary4.ContainsKey("OFF"))
			{
				PdfArray pdfArray7 = PdfCrossTable.Dereference(pdfDictionary3["OFF"]) as PdfArray;
				PdfArray pdfArray8 = PdfCrossTable.Dereference(pdfDictionary4["OFF"]) as PdfArray;
				if (pdfArray7 != null && pdfArray8 != null)
				{
					pdfArray7.Elements.AddRange(pdfArray8.Elements);
				}
			}
			else if (pdfDictionary4.ContainsKey("OFF"))
			{
				pdfDictionary3.SetProperty("OFF", pdfDictionary4["OFF"] as PdfDictionary);
			}
			if (pdfDictionary3.ContainsKey("AS") && pdfDictionary4.ContainsKey("AS"))
			{
				PdfArray pdfArray9 = PdfCrossTable.Dereference(pdfDictionary3["AS"]) as PdfArray;
				PdfArray pdfArray10 = PdfCrossTable.Dereference(pdfDictionary4["AS"]) as PdfArray;
				if (pdfArray9 == null || pdfArray10 == null || pdfArray10.Count <= 0 || pdfArray9.Count <= 0)
				{
					return;
				}
				PdfDictionary pdfDictionary5 = PdfCrossTable.Dereference(pdfArray10[0]) as PdfDictionary;
				PdfDictionary pdfDictionary6 = PdfCrossTable.Dereference(pdfArray9[0]) as PdfDictionary;
				if (pdfDictionary5 != null && pdfDictionary6 != null && pdfDictionary5.ContainsKey("OCGs"))
				{
					PdfArray pdfArray11 = PdfCrossTable.Dereference(pdfDictionary5["OCGs"]) as PdfArray;
					if (PdfCrossTable.Dereference(pdfDictionary6["OCGs"]) is PdfArray pdfArray12 && pdfArray11 != null)
					{
						pdfArray12.Elements.AddRange(pdfArray11.Elements);
					}
				}
			}
			else if (pdfDictionary4.ContainsKey("AS"))
			{
				pdfDictionary3.SetProperty("AS", pdfDictionary4["AS"] as PdfDictionary);
			}
		}
		else if (pdfDictionary4 != null)
		{
			pdfDictionary.SetProperty("D", pdfDictionary4);
		}
	}

	internal void AddLicenseWatermark(PdfPageBase page, bool isFirstIndex, bool isLastIndex)
	{
		PdfLoadedPage pdfLoadedPage = page as PdfLoadedPage;
		PdfPage pdfPage = page as PdfPage;
		string text = "Created with a trial version of DocGen PDF library.";
		PdfStringFormat format = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
		float size = 14f;
		if (page != null && page.Size.Width < 400f)
		{
			size = page.Size.Width * 0.035f;
		}
		PdfStandardFont pdfStandardFont = new PdfStandardFont(PdfFontFamily.Helvetica, size, PdfFontStyle.Regular, isLicense: true);
		if (pdfLoadedPage != null)
		{
			PdfGraphics graphics = pdfLoadedPage.Graphics;
			SizeF clientSize = graphics.ClientSize;
			if (isFirstIndex)
			{
				DrawWatermarkOnPage(pdfLoadedPage, pdfStandardFont, pdfLoadedPage.Graphics, lastPage: false, PdfPageRotateAngle.RotateAngle0);
			}
			else if (isLastIndex)
			{
				DrawWatermarkOnPage(pdfLoadedPage, pdfStandardFont, pdfLoadedPage.Graphics, lastPage: true, PdfPageRotateAngle.RotateAngle0);
			}
			SizeF sizeF = pdfStandardFont.MeasureString(text, clientSize.Width);
			float num = clientSize.Width / 2f;
			float offsetY = clientSize.Height / 2f;
			graphics.Save();
			graphics.TranslateTransform(num, offsetY);
			graphics.SetTransparency(0.5f);
			graphics.RotateTransform(-45f);
			float num2 = sizeF.Width / 2f;
			float num3 = sizeF.Height / 2f;
			float num4 = num - num2;
			graphics.DrawString(text, pdfStandardFont, PdfBrushes.Red, new RectangleF(0f - num2, 0f - num3 / 2f, graphics.ClientSize.Width - (num4 + num4), sizeF.Height), format);
			graphics.Restore();
		}
		else if (pdfPage != null)
		{
			PdfTemplate pdfTemplate = new PdfTemplate(pdfPage.Size.Width, pdfPage.Size.Height);
			PdfPageRotateAngle pdfPageRotateAngle = PdfPageRotateAngle.RotateAngle0;
			if (pdfPage.Section != null)
			{
				pdfPageRotateAngle = pdfPage.Section.PageSettings.Rotate;
			}
			else if (pdfPage.Document != null)
			{
				pdfPageRotateAngle = pdfPage.Document.PageSettings.Rotate;
			}
			if (isFirstIndex)
			{
				DrawWatermarkOnPage(page, pdfStandardFont, pdfTemplate.Graphics, lastPage: false, pdfPageRotateAngle);
			}
			else if (isLastIndex)
			{
				DrawWatermarkOnPage(page, pdfStandardFont, pdfTemplate.Graphics, lastPage: true, pdfPageRotateAngle);
			}
			SizeF sizeF2 = pdfStandardFont.MeasureString(text, pdfTemplate.Width);
			float num5 = pdfTemplate.Width / 2f;
			float offsetY2 = pdfTemplate.Height / 2f;
			pdfTemplate.Graphics.Save();
			pdfTemplate.Graphics.TranslateTransform(num5, offsetY2);
			pdfTemplate.Graphics.SetTransparency(0.5f);
			switch (pdfPageRotateAngle)
			{
			case PdfPageRotateAngle.RotateAngle90:
				pdfTemplate.Graphics.RotateTransform(-135f);
				break;
			case PdfPageRotateAngle.RotateAngle180:
				pdfTemplate.Graphics.RotateTransform(-225f);
				break;
			case PdfPageRotateAngle.RotateAngle270:
				pdfTemplate.Graphics.RotateTransform(-315f);
				break;
			default:
				pdfTemplate.Graphics.RotateTransform(-45f);
				break;
			}
			float num6 = sizeF2.Width / 2f;
			float num7 = sizeF2.Height / 2f;
			float num8 = num5 - num6;
			pdfTemplate.Graphics.DrawString(text, pdfStandardFont, PdfBrushes.Red, new RectangleF(0f - num6, 0f - num7 / 2f, pdfTemplate.Width - (num8 + num8), sizeF2.Height), format);
			pdfTemplate.Graphics.Restore();
			SetWaterMarkResources(pdfTemplate.m_resources, pdfPage.GetResources());
			PdfStream content = pdfTemplate.m_content;
			content.Items.Clear();
			PdfReferenceHolder element = new PdfReferenceHolder(content as PdfStream);
			if (page.Imported && page.Contents.Count > 0)
			{
				PdfStream pdfStream = new PdfStream();
				pdfStream.Write("q");
				pdfStream.Write("\r\n");
				page.Contents.Insert(0, new PdfReferenceHolder(pdfStream));
				PdfStream pdfStream2 = new PdfStream();
				pdfStream2.Write("Q");
				pdfStream2.Write("\r\n");
				page.Contents.Add(new PdfReferenceHolder(pdfStream2));
				page.Contents.Add(element);
			}
			else
			{
				page.Contents.Add(element);
			}
		}
	}

	private PdfStringLayoutResult GetLineResult(string text, PdfFont font, PdfStringFormat format, SizeF size)
	{
		return new PdfStringLayouter().Layout(text, font, format, size);
	}

	internal void DrawWatermarkOnPage(PdfPageBase page, PdfFont font, PdfGraphics graphics, bool lastPage, PdfPageRotateAngle rotationAngle)
	{
		string text = "Created with a trial version of DocGen PDF library or registered the wrong key in your application. Click ";
		string text2 = "here";
		string text3 = "to obtain the valid key.";
		bool flag = page is PdfLoadedPage;
		graphics.Save();
		SizeF clientSize = graphics.ClientSize;
		PdfBrush red = PdfBrushes.Red;
		PdfBrush blue = PdfBrushes.Blue;
		PdfMargins margin = GetMargin(page);
		SizeF size = new SizeF(clientSize.Width - 80f, clientSize.Height);
		if (rotationAngle == PdfPageRotateAngle.RotateAngle270 || rotationAngle == PdfPageRotateAngle.RotateAngle90)
		{
			size = new SizeF(clientSize.Height - 80f, clientSize.Width);
		}
		float num = 10f;
		float num2 = 40f;
		if (lastPage)
		{
			SizeF sizeF = font.MeasureString(text + text2 + text3, size.Width);
			num = size.Height - sizeF.Height - 10f;
		}
		PdfStringFormat format = new PdfStringFormat();
		PdfStringLayoutResult lineResult = GetLineResult(text, font, format, size);
		PdfStringLayoutResult lineResult2 = GetLineResult(text2, font, format, size);
		PdfStringLayoutResult lineResult3 = GetLineResult(text3, font, format, size);
		float width = font.MeasureString(" ").Width;
		if (flag)
		{
			graphics.DrawString(text, font, red, new RectangleF(num2, num, size.Width, size.Height), format);
		}
		else
		{
			DrawStringWithRotation(graphics, text, new RectangleF(num2, num, size.Width, size.Height), rotationAngle, font, red, format);
		}
		if (lineResult.Lines.Length == 0)
		{
			return;
		}
		LineInfo lineInfo = lineResult.Lines[lineResult.Lines.Length - 1];
		List<RectangleF> list = new List<RectangleF>();
		float num4;
		float num3;
		if (lineInfo.Width + lineResult2.ActualSize.Width < size.Width)
		{
			num3 = lineInfo.Width + width + num2;
			num4 = ((lineResult.Lines.Length > 1) ? (lineResult.LineHeight * (float)(lineResult.Lines.Length - 1) + num) : num);
			float width2 = lineResult2.ActualSize.Width;
			float lineHeight = lineResult2.LineHeight;
			if (flag)
			{
				list.Add(new RectangleF(num3, num4, width2, lineHeight));
				graphics.DrawString(text2, font, blue, new RectangleF(num3, num4, width2, size.Height));
			}
			else
			{
				switch (rotationAngle)
				{
				case PdfPageRotateAngle.RotateAngle270:
					list.Add(new RectangleF(size.Height - (num4 + lineHeight), num3, lineHeight, width2));
					break;
				case PdfPageRotateAngle.RotateAngle90:
					list.Add(new RectangleF(num4, size.Width - (num3 + width2 - (num2 + num2)), lineHeight, width2));
					break;
				case PdfPageRotateAngle.RotateAngle180:
					list.Add(new RectangleF(size.Width - (num3 + width2 - (num2 + num2)), size.Height - (num4 + lineHeight), width2, lineHeight));
					break;
				default:
					list.Add(new RectangleF(num3, num4, width2, lineHeight));
					break;
				}
				DrawStringWithRotation(graphics, text2, new RectangleF(num3, num4, width2, size.Height), rotationAngle, font, blue, format);
			}
			num3 += width2 + width;
		}
		else
		{
			num3 = lineInfo.Width + width + num2;
			num4 = ((lineResult.Lines.Length > 1) ? (lineResult.LineHeight * (float)(lineResult.Lines.Length - 1) + num) : num);
			string[] array = text2.TrimEnd().Split(new char[1] { ' ' });
			float num5 = 0f;
			float num6 = num3;
			for (int i = 0; i < array.Length; i++)
			{
				float num7 = width + font.MeasureString(array[i]).Width;
				if (num3 + num7 > size.Width)
				{
					if (flag)
					{
						list.Add(new RectangleF(num6, num4, num5, lineResult2.LineHeight));
					}
					else if (num5 != 0f)
					{
						switch (rotationAngle)
						{
						case PdfPageRotateAngle.RotateAngle270:
							list.Add(new RectangleF(size.Height - (num4 + lineResult2.LineHeight), num6, lineResult2.LineHeight, num5 - width));
							break;
						case PdfPageRotateAngle.RotateAngle90:
							list.Add(new RectangleF(num4, size.Width - (num6 + num5 - (num2 + num2)), lineResult2.LineHeight, num5));
							break;
						case PdfPageRotateAngle.RotateAngle180:
							list.Add(new RectangleF(size.Width - (num6 + num5 - (num2 + num2)), size.Height - (num4 + lineResult2.LineHeight), num5, lineResult2.LineHeight));
							break;
						default:
							list.Add(new RectangleF(num6, num4, num5, lineResult2.LineHeight));
							break;
						}
					}
					num6 = (num3 = num2);
					num4 += lineResult2.LineHeight;
					num5 = 0f;
				}
				if (flag)
				{
					graphics.DrawString(array[i], font, blue, new PointF(num3, num4));
				}
				else
				{
					DrawStringWithRotation(graphics, array[i], new RectangleF(num3, num4, num7 - width, font.Height), rotationAngle, font, blue, format);
				}
				num3 += num7;
				num5 += num7;
			}
			if (flag)
			{
				list.Add(new RectangleF(num6, num4, num5, lineResult2.LineHeight));
			}
			else
			{
				switch (rotationAngle)
				{
				case PdfPageRotateAngle.RotateAngle270:
					list.Add(new RectangleF(size.Height - (num4 + lineResult2.LineHeight), num6, lineResult2.LineHeight, num5 - width));
					break;
				case PdfPageRotateAngle.RotateAngle90:
					list.Add(new RectangleF(num4, size.Width - (num6 + num5 - width - (num2 + num2)), lineResult2.LineHeight, num5));
					break;
				case PdfPageRotateAngle.RotateAngle180:
					list.Add(new RectangleF(size.Width - (num6 + num5 - (num2 + num2)), size.Height - (num4 + lineResult2.LineHeight), num5, lineResult2.LineHeight));
					break;
				default:
					list.Add(new RectangleF(num6, num4, num5, lineResult2.LineHeight));
					break;
				}
			}
		}
		if (list.Count > 0)
		{
			PdfLoadedPage pdfLoadedPage = page as PdfLoadedPage;
			PdfPage pdfPage = page as PdfPage;
			foreach (RectangleF item in list)
			{
				PdfUriAnnotation pdfUriAnnotation = new PdfUriAnnotation(flag ? item : GetBoundsWidthoutMargin(item, margin, pdfPage));
				pdfUriAnnotation.Uri = m_licenseURI;
				pdfUriAnnotation.Border = new PdfAnnotationBorder(0f, 0f, 0f);
				if (pdfLoadedPage != null)
				{
					pdfLoadedPage.Annotations.Add(pdfUriAnnotation);
				}
				else
				{
					pdfPage.Annotations.Add(pdfUriAnnotation);
				}
			}
		}
		if (num3 + lineResult3.ActualSize.Width > size.Width)
		{
			string[] array2 = text3.Split(new char[1] { ' ' });
			for (int j = 0; j < array2.Length; j++)
			{
				float num8 = width + font.MeasureString(array2[j]).Width;
				if (num3 + num8 > size.Width)
				{
					num3 = num2;
					num4 += lineResult3.LineHeight;
				}
				if (flag)
				{
					graphics.DrawString(array2[j], font, red, new PointF(num3, num4));
				}
				else
				{
					DrawStringWithRotation(graphics, array2[j], new RectangleF(num3, num4, num8 - width, font.Height), rotationAngle, font, red, format);
				}
				num3 += num8;
			}
		}
		else if (flag)
		{
			graphics.DrawString(text3, font, PdfBrushes.Red, new RectangleF(num3, num4, lineResult3.ActualSize.Width, size.Height));
		}
		else
		{
			DrawStringWithRotation(graphics, text3, new RectangleF(num3, num4, lineResult3.ActualSize.Width, size.Height), rotationAngle, font, red, format);
		}
		graphics.Restore();
	}

	private PdfMargins GetMargin(PdfPageBase page)
	{
		if (!(page is PdfPage pdfPage))
		{
			return null;
		}
		if (pdfPage.Section != null)
		{
			return pdfPage.Section.PageSettings.Margins;
		}
		if (pdfPage.Document != null)
		{
			return pdfPage.Document.PageSettings.Margins;
		}
		return null;
	}

	private RectangleF GetBoundsWidthoutMargin(RectangleF bounds, PdfMargins margin, PdfPage newPage)
	{
		if (margin == null && newPage == null)
		{
			return bounds;
		}
		if (newPage != null && newPage.Section != null)
		{
			PdfDocument document = newPage.Document;
			PdfSection section = newPage.Section;
			float num = 0f;
			float num2 = 0f;
			if (document != null && section != null)
			{
				num = section.GetLeftIndentWidth(document, newPage, includeMargins: false);
				num2 = section.GetTopIndentHeight(document, newPage, includeMargins: false);
			}
			return new RectangleF(bounds.X - (margin.Left + num), bounds.Y - (margin.Left + num2), bounds.Width, bounds.Height);
		}
		return new RectangleF(bounds.X - margin.Left, bounds.Y - margin.Left, bounds.Width, bounds.Height);
	}

	private void DrawStringWithRotation(PdfGraphics graphics, string text, RectangleF bounds, PdfPageRotateAngle angle, PdfFont font, PdfBrush brush, PdfStringFormat format)
	{
		SizeF clientSize = graphics.ClientSize;
		switch (angle)
		{
		case PdfPageRotateAngle.RotateAngle90:
			graphics.Save();
			graphics.RotateTransform(-90f);
			graphics.DrawString(text, font, brush, new RectangleF(0f - (clientSize.Height - bounds.X), bounds.Y, bounds.Width, bounds.Height), format);
			graphics.Restore();
			break;
		case PdfPageRotateAngle.RotateAngle270:
			graphics.Save();
			graphics.RotateTransform(-270f);
			graphics.DrawString(text, font, brush, new RectangleF(bounds.X, 0f - (clientSize.Width - bounds.Y), bounds.Width, bounds.Height), format);
			graphics.Restore();
			break;
		case PdfPageRotateAngle.RotateAngle180:
			graphics.Save();
			graphics.RotateTransform(-180f);
			graphics.DrawString(text, font, brush, new RectangleF(0f - (clientSize.Width - bounds.X), 0f - (clientSize.Height - bounds.Y), bounds.Width, bounds.Height), format);
			graphics.Restore();
			break;
		default:
			graphics.DrawString(text, font, brush, bounds, format);
			break;
		}
	}

	internal void OnPageSave(PdfPageBase page)
	{
		if (progressDelegate == null)
		{
			return;
		}
		if (this is PdfDocument pdfDocument)
		{
			int current = ++m_progressPageIndex;
			if (m_pageCount == 0)
			{
				m_pageCount = pdfDocument.PageCount;
			}
			ProgressEventArgs arguments = new ProgressEventArgs(current, m_pageCount);
			OnSaveProgress(arguments);
		}
		else if (this is PdfLoadedDocument pdfLoadedDocument)
		{
			int index = pdfLoadedDocument.Pages.GetIndex(page);
			if (m_pageCount == 0)
			{
				m_pageCount = pdfLoadedDocument.PageCount;
			}
			if (index > -1)
			{
				ProgressEventArgs arguments2 = ((!pdfLoadedDocument.FileStructure.IncrementalUpdate || m_changedPages <= 0) ? new ProgressEventArgs(index, m_pageCount, ++m_pageProcessed) : new ProgressEventArgs(index, m_pageCount, ++m_pageProcessed, m_changedPages));
				OnSaveProgress(arguments2);
			}
		}
	}

	internal void OnSaveProgress(ProgressEventArgs arguments)
	{
		if (progressDelegate != null)
		{
			progressDelegate(this, arguments);
		}
	}

	internal void SetWaterMarkResources(PdfResources templateResources, PdfResources pageResources)
	{
		PdfName key = new PdfName("ExtGState");
		PdfName key2 = new PdfName("Font");
		if (templateResources.ContainsKey(key))
		{
			PdfDictionary pdfDictionary = templateResources.Items[key] as PdfDictionary;
			if (pageResources.Items.ContainsKey(key))
			{
				if (pageResources.Items[key] is PdfDictionary pdfDictionary2)
				{
					foreach (KeyValuePair<PdfName, IPdfPrimitive> item in pdfDictionary.Items)
					{
						if (!pdfDictionary2.Items.ContainsKey(item.Key))
						{
							pdfDictionary2.Items.Add(item.Key, item.Value);
							continue;
						}
						PdfName key3 = new PdfName(Guid.NewGuid().ToString());
						pdfDictionary2.Items.Add(key3, item.Value);
					}
				}
			}
			else
			{
				pageResources.Items.Add(key, pdfDictionary);
			}
		}
		if (!templateResources.ContainsKey(key2))
		{
			return;
		}
		PdfDictionary pdfDictionary3 = templateResources.Items[new PdfName("Font")] as PdfDictionary;
		if (pageResources.Items.ContainsKey(key2))
		{
			if (!(pageResources.Items[key2] is PdfDictionary pdfDictionary4))
			{
				return;
			}
			{
				foreach (KeyValuePair<PdfName, IPdfPrimitive> item2 in pdfDictionary3.Items)
				{
					if (!pdfDictionary4.Items.ContainsKey(item2.Key))
					{
						pdfDictionary4.Items.Add(item2.Key, item2.Value);
						continue;
					}
					PdfName key4 = new PdfName(Guid.NewGuid().ToString());
					pdfDictionary4.Items.Add(key4, item2.Value);
				}
				return;
			}
		}
		pageResources.Items.Add(key2, pdfDictionary3);
	}

	internal abstract PdfForm ObtainForm();

	internal void SetMainObjectCollection(PdfMainObjectCollection moc)
	{
		if (moc == null)
		{
			throw new ArgumentNullException("moc");
		}
		m_objects = moc;
	}

	internal void SetSecurity(PdfSecurity security)
	{
		if (security == null)
		{
			throw new ArgumentNullException("security");
		}
		m_security = security;
	}

	internal void SetCrossTable(PdfCrossTable cTable)
	{
		if (cTable == null)
		{
			throw new ArgumentNullException("cTable");
		}
		m_crossTable = cTable;
	}

	internal void SetCatalog(PdfCatalog catalog)
	{
		if (catalog == null)
		{
			throw new ArgumentNullException("catalog");
		}
		m_catalog = catalog;
		if (!m_catalog.ContainsKey("Outlines"))
		{
			return;
		}
		PdfReferenceHolder pdfReferenceHolder = m_catalog["Outlines"] as PdfReferenceHolder;
		PdfDictionary pdfDictionary = null;
		pdfDictionary = ((!(pdfReferenceHolder == null)) ? (pdfReferenceHolder.Object as PdfDictionary) : (m_catalog["Outlines"] as PdfDictionary));
		if (pdfDictionary != null && pdfDictionary.ContainsKey("First"))
		{
			PdfReferenceHolder pdfReferenceHolder2 = pdfDictionary["First"] as PdfReferenceHolder;
			if (pdfReferenceHolder2 != null && !(pdfReferenceHolder2.Object is PdfDictionary))
			{
				pdfDictionary.Remove("First");
			}
		}
	}

	internal void OnDocumentSaved(DocumentSavedEventArgs args)
	{
		if (args == null)
		{
			throw new ArgumentNullException("args");
		}
		if (this.DocumentSaved != null)
		{
			this.DocumentSaved(this, args);
		}
	}

	internal abstract void AddFields(PdfLoadedDocument ldDoc, PdfPageBase newPage, List<PdfField> fields);

	internal abstract PdfPageBase ClonePage(PdfLoadedDocument ldDoc, PdfPageBase page, List<PdfArray> destinations);

	protected virtual void CheckFields(PdfLoadedDocument ldDoc, PdfPageBase page, List<PdfField> fields, PdfPageBase importedPage)
	{
		PdfArray pdfArray = page.ObtainAnnotations();
		PdfLoadedForm form = ldDoc.Form;
		PdfName key = new PdfName("Kids");
		if (pdfArray == null || form == null)
		{
			return;
		}
		int i = 0;
		for (int count = form.Fields.Count; i < count; i++)
		{
			PdfField pdfField = form.Fields[i];
			if (pdfField.Dictionary.ContainsKey("removed") && pdfField.Dictionary["removed"] is PdfBoolean { Value: not false })
			{
				continue;
			}
			bool flag = false;
			PdfCollection pdfCollection = null;
			if (pdfField.Dictionary.ContainsKey(key) && (pdfField.Dictionary[key] as PdfArray).Count > 0 && !flag)
			{
				if (pdfField is PdfLoadedButtonField && (pdfField as PdfLoadedButtonField).Items.Count > 0)
				{
					pdfCollection = (pdfField as PdfLoadedButtonField).Items;
				}
				else if (pdfField is PdfLoadedCheckBoxField && (pdfField as PdfLoadedCheckBoxField).Items.Count > 0)
				{
					pdfCollection = (pdfField as PdfLoadedCheckBoxField).Items;
				}
				else if (pdfField is PdfLoadedComboBoxField && (pdfField as PdfLoadedComboBoxField).Items.Count > 0)
				{
					pdfCollection = (pdfField as PdfLoadedComboBoxField).Items;
				}
				else if (pdfField is PdfLoadedListBoxField && (pdfField as PdfLoadedListBoxField).Items.Count > 0)
				{
					pdfCollection = (pdfField as PdfLoadedListBoxField).Items;
				}
				else if (pdfField is PdfLoadedRadioButtonListField && (pdfField as PdfLoadedRadioButtonListField).Items.Count > 0)
				{
					pdfCollection = (pdfField as PdfLoadedRadioButtonListField).Items;
				}
				else if (pdfField is PdfLoadedTextBoxField && (pdfField as PdfLoadedTextBoxField).Items.Count > 0)
				{
					pdfCollection = (pdfField as PdfLoadedTextBoxField).Items;
				}
			}
			if (EnableMemoryOptimization)
			{
				if (pdfCollection != null)
				{
					foreach (PdfLoadedFieldItem item in pdfCollection)
					{
						if (item.Page != null && item.Page == page)
						{
							fields.Add(pdfField);
							break;
						}
					}
					if (m_annotCount != 0 && fields.Count == m_annotCount)
					{
						break;
					}
				}
				else if (pdfField.Page == page)
				{
					fields.Add(pdfField);
					if (m_annotCount != 0 && fields.Count == m_annotCount)
					{
						break;
					}
				}
			}
			else if (pdfField.Page == page)
			{
				if (pdfCollection != null)
				{
					foreach (PdfLoadedFieldItem item2 in pdfCollection)
					{
						if (item2.Page != null && item2.Page == page)
						{
							item2.Page = importedPage;
						}
					}
				}
				fields.Add(pdfField);
				m_addedField.Add(pdfField.Name);
				if (m_annotCount != 0 && fields.Count == m_annotCount)
				{
					break;
				}
			}
			else
			{
				if (!(pdfField is PdfLoadedTextBoxField))
				{
					continue;
				}
				PdfLoadedTextBoxField pdfLoadedTextBoxField = pdfField as PdfLoadedTextBoxField;
				if (pdfLoadedTextBoxField.Kids == null)
				{
					continue;
				}
				foreach (PdfLoadedTexBoxItem item3 in pdfLoadedTextBoxField.Items)
				{
					if (item3.Page != null && item3.Page == page)
					{
						item3.Page = importedPage;
						pdfField.Page = importedPage;
						if (!m_fieldKids.ContainsKey(pdfField.Name) && !m_addedField.Contains(pdfField.Name))
						{
							m_fieldKids.Add(pdfField.Name, pdfField);
							fields.Add(pdfField);
							m_addedField.Add(pdfField.Name);
						}
						m_isKidsPage = true;
					}
				}
			}
		}
	}

	private void MergeAttachments(PdfLoadedDocument ldDoc)
	{
		PdfCatalogNames names = ldDoc.Catalog.Names;
		if (names != null)
		{
			if (ldDoc.Security.EncryptOnlyAttachment)
			{
				_ = ldDoc.Attachments;
			}
			Catalog.CreateNamesIfNone();
			if (EnableMemoryOptimization)
			{
				Catalog.Names.MergeEmbedded(names, m_crossTable);
			}
			else
			{
				Catalog.Names.MergeEmbedded(names, null);
			}
		}
		m_crossTable.PrevReference = null;
	}

	private PdfNamedDestination GetNamedDestination(PdfNamedDestination nDest, PdfPageBase page)
	{
		return new PdfNamedDestination(nDest.Title)
		{
			Destination = GetDestination(page, nDest.Destination)
		};
	}

	private PdfDestination GetDestination(PdfPageBase page, PdfDestination dest)
	{
		return new PdfDestination(page, dest.Location)
		{
			Bounds = dest.Bounds,
			Mode = dest.Mode,
			Zoom = dest.Zoom,
			isModified = false
		};
	}

	private void ExportBookmarks(PdfLoadedDocument ldDoc, List<PdfBookmarkBase> bookmarks, int pageCount, Dictionary<PdfPageBase, object> bookmarkshash)
	{
		PdfBookmarkBase pdfBookmarkBase = Bookmarks;
		PdfBookmarkBase bookmarks2 = ldDoc.Bookmarks;
		List<string> list = null;
		if (bookmarks2 == null)
		{
			return;
		}
		if (pdfBookmarkBase == null)
		{
			pdfBookmarkBase = (this as PdfLoadedDocument).CreateBookmarkRoot();
		}
		Stack<NodeInfo> stack = new Stack<NodeInfo>();
		NodeInfo nodeInfo = new NodeInfo(pdfBookmarkBase, bookmarks2.List);
		if (ldDoc.Pages.Count != pageCount)
		{
			nodeInfo = new NodeInfo(pdfBookmarkBase, bookmarks);
			list = new List<string>();
		}
		while (true)
		{
			if (nodeInfo.Index < nodeInfo.Kids.Count)
			{
				bookmarks2 = nodeInfo.Kids[nodeInfo.Index];
				if (bookmarks.Contains(bookmarks2) && list != null && !list.Contains((bookmarks2 as PdfBookmark).Title))
				{
					PdfBookmark pdfBookmark = bookmarks2 as PdfBookmark;
					PdfBookmark pdfBookmark2 = pdfBookmarkBase.Add(pdfBookmark.Title);
					pdfBookmark2.TextStyle = pdfBookmark.TextStyle;
					pdfBookmark2.Color = pdfBookmark.Color;
					PdfDestination destination = pdfBookmark.Destination;
					PdfDestination pdfDestination = null;
					PdfPageBase pdfPageBase = null;
					PdfPageBase pdfPageBase2 = null;
					PdfNamedDestination namedDestination = pdfBookmark.NamedDestination;
					if (namedDestination != null)
					{
						if (namedDestination.Destination != null)
						{
							pdfPageBase2 = namedDestination.Destination.Page;
							if (ldDoc.CrossTable.PageCorrespondance.ContainsKey(pdfPageBase2.Dictionary) && ldDoc.CrossTable.PageCorrespondance[pdfPageBase2.Dictionary] != null && ldDoc.CrossTable.PageCorrespondance[pdfPageBase2.Dictionary] is PdfPageBase page)
							{
								PdfNamedDestination item = (pdfBookmark2.NamedDestination = GetNamedDestination(namedDestination, page));
								pdfBookmark2.Dictionary.Remove("C");
								m_namedDetinations.Add(item);
							}
						}
					}
					else if (destination != null && EnableMemoryOptimization)
					{
						pdfPageBase2 = destination.Page;
						if (ldDoc.CrossTable.PageCorrespondance.ContainsKey(pdfPageBase2.Dictionary) && ldDoc.CrossTable.PageCorrespondance[pdfPageBase2.Dictionary] != null)
						{
							if (ldDoc.CrossTable.PageCorrespondance[pdfPageBase2.Dictionary] is PdfPageBase page2)
							{
								pdfDestination = new PdfDestination(page2, destination.Location);
								pdfDestination.Mode = destination.Mode;
								pdfDestination.Bounds = destination.Bounds;
								pdfDestination.Zoom = destination.Zoom;
								pdfDestination.isModified = false;
								pdfBookmark2.Destination = pdfDestination;
							}
						}
						else
						{
							pdfBookmark2.Dictionary.Remove("A");
						}
					}
					else if (destination != null)
					{
						pdfPageBase2 = destination.Page;
						pdfPageBase = ldDoc.CrossTable.PageCorrespondance[((IPdfWrapper)pdfPageBase2).Element] as PdfPageBase;
						pdfDestination = new PdfDestination(pdfPageBase, destination.Location);
						pdfDestination.Mode = destination.Mode;
						pdfDestination.Bounds = destination.Bounds;
						pdfDestination.Zoom = destination.Zoom;
						pdfDestination.isModified = false;
						pdfBookmark2.Destination = pdfDestination;
					}
					pdfBookmarkBase = pdfBookmark2;
					list.Add(pdfBookmark2.Title);
				}
				else if (list == null || (list != null && !list.Contains((bookmarks2 as PdfBookmark).Title)))
				{
					PdfBookmark pdfBookmark3 = bookmarks2 as PdfBookmark;
					PdfDestination destination2 = pdfBookmark3.Destination;
					PdfDestination pdfDestination2 = null;
					PdfPageBase pdfPageBase3 = null;
					PdfPageBase pdfPageBase4 = null;
					PdfNamedDestination namedDestination3 = pdfBookmark3.NamedDestination;
					if (ldDoc.Pages.Count == pageCount)
					{
						PdfBookmark pdfBookmark4 = pdfBookmarkBase.Add(pdfBookmark3.Title);
						if (!EnableMemoryOptimization && pdfBookmark3.Dictionary.ContainsKey("A"))
						{
							pdfBookmark4.Dictionary.SetProperty("A", pdfBookmark3.Dictionary["A"]);
						}
						pdfBookmark4.TextStyle = pdfBookmark3.TextStyle;
						pdfBookmark4.Color = pdfBookmark3.Color;
						if (namedDestination3 != null)
						{
							if (namedDestination3.Destination != null)
							{
								pdfPageBase4 = namedDestination3.Destination.Page;
								if (ldDoc.CrossTable.PageCorrespondance.ContainsKey(pdfPageBase4.Dictionary) && ldDoc.CrossTable.PageCorrespondance[pdfPageBase4.Dictionary] != null && ldDoc.CrossTable.PageCorrespondance[pdfPageBase4.Dictionary] is PdfPageBase page3)
								{
									PdfNamedDestination namedDestination4 = GetNamedDestination(namedDestination3, page3);
									m_namedDetinations.Add(namedDestination4);
									pdfBookmark4.NamedDestination = namedDestination4;
									pdfBookmark4.Dictionary.Remove("C");
								}
							}
						}
						else if (destination2 != null)
						{
							pdfPageBase4 = destination2.Page;
							if (ldDoc.CrossTable.PageCorrespondance.ContainsKey(pdfPageBase4.Dictionary) && ldDoc.CrossTable.PageCorrespondance[pdfPageBase4.Dictionary] != null)
							{
								if (ldDoc.CrossTable.PageCorrespondance[pdfPageBase4.Dictionary] is PdfPageBase page4)
								{
									pdfDestination2 = new PdfDestination(page4, destination2.Location);
									pdfDestination2.Mode = destination2.Mode;
									pdfDestination2.Bounds = destination2.Bounds;
									pdfDestination2.Zoom = destination2.Zoom;
									pdfDestination2.isModified = false;
									pdfBookmark4.Destination = pdfDestination2;
									pdfBookmark4.Destination.PageIndex = destination2.PageIndex;
								}
							}
							else
							{
								pdfBookmark4.Dictionary.Remove("A");
							}
						}
						pdfBookmarkBase = pdfBookmark4;
					}
					else if (destination2 != null && destination2.Page != null && ldDoc.Pages.IndexOf(destination2.Page) < pageCount && ldDoc.CrossTable.PageCorrespondance.ContainsKey(destination2.Page.Dictionary) && ldDoc.CrossTable.PageCorrespondance[destination2.Page.Dictionary] != null)
					{
						pdfPageBase4 = destination2.Page;
						pdfPageBase3 = ldDoc.CrossTable.PageCorrespondance[destination2.Page.Dictionary] as PdfPageBase;
						PdfBookmark pdfBookmark5 = pdfBookmarkBase.Add(pdfBookmark3.Title);
						if (pdfBookmark3.Dictionary.ContainsKey("A"))
						{
							if (EnableMemoryOptimization)
							{
								IPdfPrimitive pdfPrimitive = pdfBookmark3.Dictionary["A"].Clone(m_crossTable);
								pdfBookmark5.Dictionary.SetProperty("A", pdfPrimitive);
							}
							else
							{
								pdfBookmark5.Dictionary.SetProperty("A", pdfBookmark3.Dictionary["A"]);
							}
						}
						if (pdfPageBase3 != null)
						{
							pdfBookmark5.TextStyle = pdfBookmark3.TextStyle;
							pdfBookmark5.Color = pdfBookmark3.Color;
							pdfDestination2 = new PdfDestination(pdfPageBase3, destination2.Location);
							pdfDestination2.Mode = destination2.Mode;
							pdfDestination2.Bounds = destination2.Bounds;
							pdfDestination2.Zoom = destination2.Zoom;
							pdfDestination2.isModified = false;
							pdfBookmark5.Destination = pdfDestination2;
							pdfBookmarkBase = pdfBookmark5;
						}
					}
				}
				nodeInfo.Index++;
				if (bookmarks2.Count > 0)
				{
					stack.Push(nodeInfo);
					nodeInfo = new NodeInfo(pdfBookmarkBase, bookmarks2.List);
				}
				else
				{
					pdfBookmarkBase = nodeInfo.Base;
				}
				continue;
			}
			if (stack.Count > 0)
			{
				nodeInfo = stack.Pop();
				while (nodeInfo.Index == nodeInfo.Kids.Count && stack.Count > 0)
				{
					nodeInfo = stack.Pop();
				}
				pdfBookmarkBase = nodeInfo.Base;
			}
			if (nodeInfo.Index >= nodeInfo.Kids.Count)
			{
				break;
			}
		}
		list?.Clear();
	}

	private void ExportNamedDestination(PdfLoadedDocument doc)
	{
		PdfPageBase pdfPageBase = null;
		PdfPageBase pdfPageBase2 = null;
		if (doc.NamedDestinationCollection != null)
		{
			foreach (object item in (IEnumerable)doc.NamedDestinationCollection)
			{
				if (item is PdfNamedDestination { Destination: not null } pdfNamedDestination)
				{
					pdfPageBase2 = pdfNamedDestination.Destination.Page;
					if (doc.CrossTable.PageCorrespondance.ContainsKey(pdfPageBase2.Dictionary) && doc.CrossTable.PageCorrespondance[pdfPageBase2.Dictionary] != null && doc.CrossTable.PageCorrespondance[((IPdfWrapper)pdfPageBase2).Element] != null && doc.CrossTable.PageCorrespondance[pdfPageBase2.Dictionary] is PdfPageBase pdfPageBase3 && pdfPageBase3.Equals(doc.CrossTable.PageCorrespondance[((IPdfWrapper)pdfPageBase2).Element]))
					{
						PdfNamedDestination namedDestination = GetNamedDestination(pdfNamedDestination, pdfPageBase3);
						m_namedDetinations.Add(namedDestination);
					}
				}
			}
		}
		doc.CrossTable.PageCorrespondance = null;
	}

	private void MarkBookmarks(List<object> pageBookmarks, List<PdfBookmarkBase> bookmarks)
	{
		if (pageBookmarks == null)
		{
			return;
		}
		foreach (object pageBookmark in pageBookmarks)
		{
			if (!(pageBookmark is PdfBookmarkBase))
			{
				throw new Exception("Type not specified properly");
			}
			bookmarks.Add(pageBookmark as PdfBookmarkBase);
		}
	}

	private void MarkBookmarks(PdfBookmarkBase bookmarkBase, List<PdfBookmarkBase> bookmarks)
	{
		bookmarks.Add(bookmarkBase);
	}

	private void FixDestinations(Dictionary<IPdfPrimitive, object> pageCorrespondance, List<PdfArray> destinations)
	{
		PdfNull element = new PdfNull();
		int i = 0;
		for (int count = destinations.Count; i < count; i++)
		{
			PdfArray pdfArray = destinations[i];
			if (pdfArray == null)
			{
				continue;
			}
			PdfReferenceHolder pdfReferenceHolder = pdfArray[0] as PdfReferenceHolder;
			if (!(pdfReferenceHolder != null))
			{
				continue;
			}
			PdfDictionary pdfDictionary = pdfReferenceHolder.Object as PdfDictionary;
			if (pdfDictionary != null && pageCorrespondance.ContainsKey(pdfDictionary) && pageCorrespondance[pdfDictionary] != null)
			{
				PdfPageBase pdfPageBase = pageCorrespondance[pdfDictionary] as PdfPageBase;
				pdfArray.RemoveAt(0);
				if (pdfPageBase != null)
				{
					pdfReferenceHolder = new PdfReferenceHolder(pdfPageBase);
					pdfArray.Insert(0, pdfReferenceHolder);
				}
				else
				{
					pdfArray.Insert(0, element);
				}
			}
			else if (pdfDictionary != null && pageCorrespondance.ContainsKey(pdfDictionary) && pageCorrespondance[pdfDictionary] == null)
			{
				pdfArray.RemoveAt(0);
				pdfArray.Insert(0, element);
			}
		}
	}

	private void ResetProgress()
	{
		if (this is PdfDocument)
		{
			(this as PdfDocument).Sections.ResetProgress();
		}
	}

	private void SetProgress()
	{
		if (this is PdfDocument)
		{
			(this as PdfDocument).Sections.SetProgress();
		}
	}

	internal string CreateHashFromStream(byte[] streamBytes)
	{
		byte[] array = new byte[32];
		IMessageDigest digest = new MessageDigestFinder().GetDigest("SHA256");
		digest.Update(streamBytes, 0, streamBytes.Length);
		digest.DoFinal(array, 0);
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(array[i].ToString("x2"));
		}
		array = null;
		return stringBuilder.ToString();
	}

	private string ObtainBaseURI()
	{
		if (string.IsNullOrEmpty(m_baseUri) && Catalog.ContainsKey("URI"))
		{
			IPdfPrimitive @object = CrossTable.GetObject(Catalog["URI"]);
			if (@object != null && ((@object is PdfReferenceHolder) ? (@object as PdfReferenceHolder).Object : @object) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("Base"))
			{
				m_baseUri = (pdfDictionary["Base"] as PdfString).Value;
			}
		}
		return m_baseUri;
	}

	private void SetBaseURI(string baseURIString)
	{
		if (string.IsNullOrEmpty(baseURIString))
		{
			m_baseUri = string.Empty;
			if (Catalog.ContainsKey("URI"))
			{
				Catalog.Remove("URI");
			}
		}
		else if (ObtainBaseURI() != baseURIString)
		{
			m_baseUri = baseURIString;
			PdfDictionary pdfDictionary = new PdfDictionary();
			pdfDictionary.SetProperty("Type", new PdfName("URI"));
			pdfDictionary.SetString("Base", m_baseUri);
			Catalog.SetProperty("URI", pdfDictionary);
		}
	}
}
