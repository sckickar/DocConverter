using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;
using DocGen.Pdf.Security;
using DocGen.Pdf.Xmp;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedDocument : PdfDocumentBase, IDisposable
{
	public delegate void OnPdfPasswordEventHandler(object sender, OnPdfPasswordEventArgs args);

	public delegate void RedactionProgressEventHandler(object sender, RedactionProgressEventArgs arguments);

	public delegate void PdfAConversionProgressEventHandler(object sender, PdfAConversionProgressEventArgs arguments);

	public delegate void PdfFontEventHandler(object sender, PdfFontEventArgs args);

	public delegate void PdfDocumentSplitEventHandler(object sender, PdfDocumentSplitEventArgs args);

	private class CurrentNodeInfo
	{
		public List<PdfBookmarkBase> Kids;

		public int Index;

		public CurrentNodeInfo(List<PdfBookmarkBase> kids)
		{
			Kids = kids;
			Index = 0;
		}

		public CurrentNodeInfo(List<PdfBookmarkBase> kids, int index)
			: this(kids)
		{
			Index = index;
		}
	}

	internal bool m_duplicatePage;

	internal int m_duplicatePageIndex = -1;

	internal bool m_duplicateAcroform;

	internal string m_password;

	internal Stream m_stream;

	private bool m_bWasEncrypted;

	private bool m_isPdfViewerDocumentDisable = true;

	private PdfLoadedForm m_form;

	private PdfLoadedPageCollection m_pages;

	private PdfBookmarkBase m_bookmark;

	private PdfNamedDestinationCollection m_namedDestinations;

	private bool m_bCloseStream;

	private bool m_isDisposed;

	internal PdfDocumentInformation m_documentInfo;

	private MemoryStream m_internalStream = new MemoryStream();

	private PdfColorSpace m_colorSpace;

	private PdfAttachmentCollection m_attachments;

	private string password;

	private PdfPageLabel m_pageLabel;

	private PdfLoadedPageLabelCollection m_pageLabelCollection;

	private bool isPageLabel;

	private bool m_isXFAForm;

	private bool m_isExtendedFeatureEnabled;

	internal string m_fileName;

	private PdfConformanceLevel m_conformance;

	private bool isLinearized;

	private bool isPortfolio;

	private List<long> unRepeatedReferenceCollections = new List<long>();

	internal bool m_isXfaDocument;

	private PdfPortfolioInformation m_portfolio;

	private static Stream m_openStream;

	internal List<PdfLoadedAnnotationType> annotationsTypeToBeIgnored;

	private bool isDispose;

	private bool isParsedForm;

	internal bool isOpenAndRepair;

	private int acroformFieldscount;

	private bool m_isOpenAndRepair;

	internal bool m_isNamedDestinationCall = true;

	internal bool IsSkipSaving;

	private bool isAttachmentOnlyEncryption;

	internal bool isCompressPdf;

	private PdfPageTemplateCollection m_pageTemplatesCollection;

	private bool m_enableInitialLoadingOptimization;

	internal Dictionary<string, string> currentFont = new Dictionary<string, string>();

	internal Dictionary<long, PdfFont> font = new Dictionary<long, PdfFont>();

	private DublinCoreSchema m_dublinschema;

	private Dictionary<PdfPageBase, object> m_bookmarkHashtable;

	internal bool IsOcredDocument;

	internal bool ConformanceEnabled;

	internal PdfConformanceLevel m_previousConformance;

	internal List<PdfLoadedPage> m_redactionPages = new List<PdfLoadedPage>();

	internal List<PdfException> pdfException = new List<PdfException>();

	internal bool validateSyntax;

	private bool m_isOptimizeIdentical;

	private PdfDocumentBase m_destinationDocument;

	private bool m_isExtendMargin;

	private bool isRedacted;

	private bool isConformanceApplied;

	internal List<PdfAnnotation> m_redactAnnotationCollection = new List<PdfAnnotation>();

	private PdfAnnotationExportSettings annotationExportSettings = new PdfAnnotationExportSettings();

	private bool m_isAllFontsEmbedded;

	internal PdfConformanceLevel existingConformanceLevel;

	private bool importCustomData;

	private PdfStructureElement m_structTreeRoot;

	private int m_elementOrder;

	private List<PdfRevision> m_revisions;

	private bool m_splitPDF;

	private bool m_mergePDF;

	private bool m_removeUnusedResourceSplit;

	private PdfDocumentSecureStore m_documentSecureStore;

	internal bool m_isPageMerging;

	private PdfFeatures feature;

	private PdfDocumentActions m_actions;

	internal bool IsXFAForm
	{
		get
		{
			return m_isXFAForm;
		}
		set
		{
			m_isXFAForm = value;
		}
	}

	internal bool IsPDFSplit
	{
		get
		{
			return m_splitPDF;
		}
		set
		{
			m_splitPDF = value;
		}
	}

	internal bool IsUnusedResourceSplit
	{
		get
		{
			return m_removeUnusedResourceSplit;
		}
		set
		{
			m_removeUnusedResourceSplit = value;
		}
	}

	internal bool IsPDFMerge
	{
		get
		{
			return m_mergePDF;
		}
		set
		{
			m_mergePDF = value;
		}
	}

	public bool IsExtendedFeatureEnabled => m_isExtendedFeatureEnabled;

	internal bool EnableInitialLoadingOptimization
	{
		get
		{
			return m_enableInitialLoadingOptimization;
		}
		set
		{
			m_enableInitialLoadingOptimization = value;
		}
	}

	internal DublinCoreSchema DublinSchema
	{
		get
		{
			return m_dublinschema;
		}
		set
		{
			m_dublinschema = value;
		}
	}

	public PdfPageLabel LoadedPageLabel
	{
		get
		{
			return m_pageLabel;
		}
		set
		{
			if (m_pageLabelCollection == null)
			{
				m_pageLabelCollection = new PdfLoadedPageLabelCollection();
			}
			isPageLabel = true;
			m_pageLabelCollection.Add(value);
		}
	}

	internal string Password
	{
		get
		{
			return password;
		}
		set
		{
			password = value;
		}
	}

	public PdfDocumentActions Actions
	{
		get
		{
			if (m_actions == null)
			{
				m_actions = new PdfDocumentActions(base.Catalog);
			}
			if (base.Catalog["AA"] == null)
			{
				base.Catalog["AA"] = ((IPdfWrapper)m_actions).Element;
			}
			return m_actions;
		}
	}

	public PdfAttachmentCollection Attachments
	{
		get
		{
			if (RaiseUserPassword && m_password == string.Empty)
			{
				OnPdfPasswordEventArgs onPdfPasswordEventArgs = new OnPdfPasswordEventArgs();
				this.OnPdfPassword(this, onPdfPasswordEventArgs);
				m_password = onPdfPasswordEventArgs.UserPassword;
			}
			if (isAttachmentOnlyEncryption)
			{
				CheckEncryption(isAttachmentOnlyEncryption);
			}
			if (m_attachments == null)
			{
				PdfDictionary attachmentDictionary = GetAttachmentDictionary();
				if (attachmentDictionary != null && attachmentDictionary.Count > 0)
				{
					if (attachmentDictionary.ContainsKey("EmbeddedFiles"))
					{
						m_attachments = new PdfAttachmentCollection(attachmentDictionary, base.CrossTable);
						if (m_attachments != null)
						{
							base.Catalog.Attachments = m_attachments;
						}
					}
					else
					{
						m_attachments = new PdfAttachmentCollection();
						if (base.Catalog.Names != null)
						{
							base.Catalog.Names.EmbeddedFiles = m_attachments;
						}
					}
				}
			}
			return m_attachments;
		}
	}

	public new PdfPortfolioInformation PortfolioInformation
	{
		get
		{
			if (m_portfolio == null)
			{
				PdfDictionary portfolioDictionary = GetPortfolioDictionary();
				if (portfolioDictionary != null)
				{
					m_portfolio = new PdfPortfolioInformation(portfolioDictionary);
					if (portfolioDictionary["D"] is PdfString pdfString && !string.IsNullOrEmpty(pdfString.Value))
					{
						foreach (PdfAttachment attachment in Attachments)
						{
							if (pdfString.Value == attachment.FileName)
							{
								m_portfolio.StartupDocument = attachment;
								break;
							}
						}
					}
				}
			}
			return m_portfolio;
		}
		set
		{
			base.Catalog.PdfPortfolio = value;
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

	public PdfLoadedForm Form
	{
		get
		{
			if (m_form == null)
			{
				UpdateFormFields();
			}
			return m_form;
		}
	}

	public PdfLoadedPageCollection Pages
	{
		get
		{
			if (m_pages == null)
			{
				m_pages = new PdfLoadedPageCollection(this, base.CrossTable);
			}
			return m_pages;
		}
	}

	public override PdfBookmarkBase Bookmarks
	{
		get
		{
			if (m_bookmark == null)
			{
				if (base.Catalog.ContainsKey("Outlines"))
				{
					if (PdfCrossTable.Dereference(base.Catalog["Outlines"]) is PdfDictionary pdfDictionary)
					{
						m_bookmark = new PdfBookmarkBase(pdfDictionary, base.CrossTable);
						if (pdfDictionary.ContainsKey("First"))
						{
							PdfReferenceHolder pdfReferenceHolder = pdfDictionary["First"] as PdfReferenceHolder;
							if (pdfReferenceHolder != null && pdfReferenceHolder.Reference != null)
							{
								m_bookmark.m_bookmarkReference.Add(pdfReferenceHolder.Reference.ObjNum);
							}
						}
						m_bookmark.ReproduceTree();
					}
					else
					{
						m_bookmark = CreateBookmarkRoot();
					}
				}
				else
				{
					m_bookmark = CreateBookmarkRoot();
				}
			}
			return m_bookmark;
		}
	}

	public PdfNamedDestinationCollection NamedDestinationCollection
	{
		get
		{
			if (m_namedDestinations == null)
			{
				if (base.Catalog.ContainsKey("Names") && m_namedDestinations == null)
				{
					PdfDictionary dictionary = PdfCrossTable.Dereference(base.Catalog["Names"]) as PdfDictionary;
					m_namedDestinations = new PdfNamedDestinationCollection(dictionary, base.CrossTable);
				}
				else if (m_namedDestinations == null && m_isNamedDestinationCall)
				{
					m_namedDestinations = CreateNamedDestinations();
				}
			}
			return m_namedDestinations;
		}
	}

	public PdfPageTemplateCollection PdfPageTemplates
	{
		get
		{
			if (m_pageTemplatesCollection == null)
			{
				if (base.Catalog.ContainsKey("Names") && m_pageTemplatesCollection == null)
				{
					if (PdfCrossTable.Dereference(base.Catalog["Names"]) is PdfDictionary dictionary)
					{
						m_pageTemplatesCollection = new PdfPageTemplateCollection(dictionary, base.CrossTable);
					}
					else
					{
						m_pageTemplatesCollection = CreatePageTemplates();
					}
				}
				else if (m_pageTemplatesCollection == null)
				{
					m_pageTemplatesCollection = CreatePageTemplates();
				}
			}
			return m_pageTemplatesCollection;
		}
	}

	public override int PageCount => Pages.Count;

	public PdfConformanceLevel Conformance
	{
		get
		{
			if (m_conformance == PdfConformanceLevel.None)
			{
				m_conformance = GetDocumentConformance(m_conformance);
			}
			return m_conformance;
		}
		set
		{
			if (value != PdfConformanceLevel.Pdf_A1B && value != PdfConformanceLevel.Pdf_A2B && value != PdfConformanceLevel.Pdf_A3B && value != 0 && value != PdfConformanceLevel.Pdf_A2U && value != PdfConformanceLevel.Pdf_A3U && value != PdfConformanceLevel.Pdf_A4 && value != PdfConformanceLevel.Pdf_A4E && value != PdfConformanceLevel.Pdf_A4F)
			{
				throw new PdfException("Pdf conformance level " + value.ToString() + " is not currently supported.");
			}
			m_conformance = value;
			if (value == PdfConformanceLevel.Pdf_A1B || value == PdfConformanceLevel.Pdf_A2B || value != PdfConformanceLevel.Pdf_A2B || value != PdfConformanceLevel.Pdf_A2U || value != PdfConformanceLevel.Pdf_A3U || value == PdfConformanceLevel.Pdf_A4 || value == PdfConformanceLevel.Pdf_A4E || value == PdfConformanceLevel.Pdf_A4F)
			{
				ConformanceEnabled = true;
				m_previousConformance = PdfDocument.ConformanceLevel;
				PdfDocument.ConformanceLevel = value;
			}
		}
	}

	public bool IsLinearized
	{
		get
		{
			isLinearized = CheckLinearization();
			return isLinearized;
		}
	}

	public bool IsPortfolio
	{
		get
		{
			if (GetPortfolioDictionary() != null)
			{
				isPortfolio = true;
				return isPortfolio;
			}
			isPortfolio = false;
			return isPortfolio;
		}
	}

	internal bool IsOptimizeIdentical
	{
		get
		{
			return m_isOptimizeIdentical;
		}
		set
		{
			m_isOptimizeIdentical = value;
		}
	}

	internal PdfDocumentBase DestinationDocument
	{
		get
		{
			return m_destinationDocument;
		}
		set
		{
			m_destinationDocument = value;
		}
	}

	internal bool IsExtendMargin
	{
		get
		{
			return m_isExtendMargin;
		}
		set
		{
			m_isExtendMargin = value;
		}
	}

	internal bool ImportCustomData
	{
		get
		{
			return importCustomData;
		}
		set
		{
			importCustomData = value;
		}
	}

	public PdfStructureElement StructureElement
	{
		get
		{
			if (m_structTreeRoot == null)
			{
				m_structTreeRoot = GetStructTreeRoot();
			}
			return m_structTreeRoot;
		}
	}

	public PdfRevision[] Revisions
	{
		get
		{
			if (m_revisions == null)
			{
				m_revisions = new List<PdfRevision>();
				GetRevisions();
			}
			return m_revisions.ToArray();
		}
	}

	public PdfDocumentSecureStore DocumentSecureStore
	{
		get
		{
			if (m_documentSecureStore == null)
			{
				m_documentSecureStore = new PdfDocumentSecureStore(base.Catalog);
			}
			return m_documentSecureStore;
		}
	}

	internal bool RaiseUserPassword => this.OnPdfPassword != null;

	internal bool RaiseTrackRedactionProgress => this.RedactionProgress != null;

	internal bool RaiseTrackPdfAConversionProgress => this.PdfAConversionProgress != null;

	internal bool RaisePdfFont => this.SubstituteFont != null;

	public override PdfDocumentInformation DocumentInformation
	{
		get
		{
			if (m_documentInfo == null)
			{
				if (PdfCrossTable.Dereference(base.CrossTable.Trailer["Info"]) is PdfDictionary dictionary)
				{
					m_documentInfo = new PdfDocumentInformation(dictionary, base.Catalog);
				}
				else
				{
					m_documentInfo = base.DocumentInformation;
				}
				ReadDocumentInfo();
			}
			return m_documentInfo;
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

	internal override bool WasEncrypted => m_bWasEncrypted;

	public bool IsEncrypted => m_bWasEncrypted;

	public event OnPdfPasswordEventHandler OnPdfPassword;

	public event RedactionProgressEventHandler RedactionProgress;

	public event PdfAConversionProgressEventHandler PdfAConversionProgress;

	public event PdfFontEventHandler SubstituteFont;

	public event PdfDocumentSplitEventHandler DocumentSplitEvent;

	public void RemoveConformance()
	{
		if (Conformance != 0)
		{
			RemovePDFConformanceInfo();
		}
	}

	public PdfLoadedDocument()
	{
	}

	public PdfLoadedDocument(byte[] file)
		: this(CreateStream(file))
	{
		isDispose = true;
		m_bCloseStream = true;
	}

	public PdfLoadedDocument(byte[] file, bool openAndRepair)
		: this(CreateStream(file), openAndRepair)
	{
		isDispose = true;
		m_bCloseStream = true;
	}

	public PdfLoadedDocument(byte[] file, string password)
		: this(CreateStream(file), password)
	{
		isDispose = true;
		Password = password;
		m_bCloseStream = true;
	}

	public PdfLoadedDocument(byte[] file, string password, bool openAndRepair)
		: this(CreateStream(file), password, openAndRepair)
	{
		isDispose = true;
		Password = password;
		m_bCloseStream = true;
	}

	public PdfLoadedDocument(Stream file)
	{
		if (file == null)
		{
			throw new ArgumentNullException("file");
		}
		if (file.Length == 0L)
		{
			throw new PdfException("Contents of file stream is empty");
		}
		if (file.Position != 0L)
		{
			file.Position = 0L;
		}
		LoadDocument(file);
	}

	public PdfLoadedDocument(Stream file, bool openAndRepair)
	{
		if (file == null)
		{
			throw new ArgumentNullException("file");
		}
		if (file.Length == 0L)
		{
			throw new PdfException("Contents of file stream is empty");
		}
		Stream stream = CheckIfValid(file);
		isOpenAndRepair = openAndRepair;
		if (stream.Position != 0L)
		{
			stream.Position = 0L;
		}
		LoadDocument(stream);
	}

	internal PdfLoadedDocument(Stream file, bool openAndRepair, bool isXfaDocument)
	{
		if (file == null)
		{
			throw new ArgumentNullException("file");
		}
		if (file.Length == 0L)
		{
			throw new PdfException("Contents of file stream is empty");
		}
		Stream stream = CheckIfValid(file);
		isOpenAndRepair = openAndRepair;
		m_isXfaDocument = isXfaDocument;
		if (stream.Position != 0L)
		{
			stream.Position = 0L;
		}
		LoadDocument(stream);
	}

	public PdfLoadedDocument(Stream file, string password)
	{
		if (file == null)
		{
			throw new ArgumentNullException("file");
		}
		if (file.Length == 0L)
		{
			throw new PdfException("Contents of file stream is empty");
		}
		if (password == null)
		{
			throw new ArgumentNullException("password");
		}
		m_password = password;
		LoadDocument(file);
	}

	public PdfLoadedDocument(Stream file, string password, bool openAndRepair)
	{
		if (file == null)
		{
			throw new ArgumentNullException("file");
		}
		if (file.Length == 0L)
		{
			throw new PdfException("Contents of file stream is empty");
		}
		if (password == null)
		{
			throw new ArgumentNullException("password");
		}
		isOpenAndRepair = openAndRepair;
		m_password = password;
		LoadDocument(file);
	}

	internal PdfLoadedDocument(Stream file, string password, bool openAndRepair, bool isXfaDocument)
	{
		if (file == null)
		{
			throw new ArgumentNullException("file");
		}
		if (file.Length == 0L)
		{
			throw new PdfException("Contents of file stream is empty");
		}
		if (password == null)
		{
			throw new ArgumentNullException("password");
		}
		isOpenAndRepair = openAndRepair;
		m_isXfaDocument = isXfaDocument;
		m_password = password;
		LoadDocument(file);
	}

	~PdfLoadedDocument()
	{
		Dispose(dispose: false);
	}

	internal PdfLoadedDocument(Stream file, string password, out List<PdfException> exceptions)
	{
		try
		{
			validateSyntax = true;
			if (file == null)
			{
				throw new ArgumentNullException("file");
			}
			if (file.Length == 0L)
			{
				throw new PdfException("Contents of file stream is empty");
			}
			m_password = password;
			Stream stream = CheckIfValid(file);
			if (stream.Position != 0L)
			{
				stream.Position = 0L;
			}
			LoadDocument(stream);
		}
		catch (Exception ex)
		{
			pdfException.Add(new PdfException(ex.Message));
		}
		exceptions = pdfException;
	}

	internal void PdfUserPassword(OnPdfPasswordEventArgs args)
	{
		this.OnPdfPassword(this, args);
	}

	internal void OnTrackProgress(RedactionProgressEventArgs arguments)
	{
		this.RedactionProgress(this, arguments);
	}

	internal void OnPdfAConversionTrackProgress(PdfAConversionProgressEventArgs arguments)
	{
		this.PdfAConversionProgress(this, arguments);
	}

	internal void PdfFontStream(PdfFontEventArgs args)
	{
		this.SubstituteFont(this, args);
	}

	private static Stream CreateStream(byte[] file)
	{
		if (file == null)
		{
			throw new ArgumentNullException("file");
		}
		return new MemoryStream(file);
	}

	public bool FindText(string text, int index, out List<RectangleF> matchRect)
	{
		feature = new PdfFeatures();
		CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		bool result = feature.FindTextMatches(index, this, text, TextSearchOptions.None, out matchRect);
		Thread.CurrentThread.CurrentCulture = currentCulture;
		return result;
	}

	internal bool FindText(string text, int index, TextSearchOptions textSearchOption, out List<RectangleF> matchRect)
	{
		feature = new PdfFeatures();
		CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		bool result = feature.FindTextMatches(index, this, text, textSearchOption, out matchRect);
		Thread.CurrentThread.CurrentCulture = currentCulture;
		return result;
	}

	public void SplitByFixedNumber(int numberToSplit)
	{
		PdfSplitOptions splitOptions = new PdfSplitOptions();
		SplitByFixedNumber(numberToSplit, splitOptions);
	}

	public void SplitByFixedNumber(int numberToSplit, PdfSplitOptions splitOptions)
	{
		int count = Pages.Count;
		if (count < numberToSplit || numberToSplit == 0)
		{
			throw new ArgumentException("Invalid split number. Split number should be greater than zero and less than or equal to page count.");
		}
		IsPDFSplit = splitOptions.SplitTags;
		IsUnusedResourceSplit = splitOptions.RemoveUnusedResources;
		int num = 1;
		int num2 = count;
		int num3 = 0;
		while (num2 > numberToSplit)
		{
			num3 = numberToSplit + num3;
			for (int i = num3 - numberToSplit; i < num3; i += numberToSplit)
			{
				ImportDocumentPages(i, num3 - 1, null, num);
				num++;
				num2 -= numberToSplit;
			}
		}
		if (num2 > 0)
		{
			int num4 = count - num2;
			ImportDocumentPages(num4, num4 + (num2 - 1), null, num);
			num++;
		}
	}

	public void SplitByRanges(int[,] ranges)
	{
		PdfSplitOptions splitOptions = new PdfSplitOptions();
		SplitByRanges(ranges, splitOptions);
	}

	public void SplitByRanges(int[,] ranges, PdfSplitOptions splitOptions)
	{
		int num = 1;
		int count = Pages.Count;
		int num2 = 0;
		int num3 = 0;
		IsPDFSplit = splitOptions.SplitTags;
		IsUnusedResourceSplit = splitOptions.RemoveUnusedResources;
		for (int i = 0; i < ranges.GetLength(0); i++)
		{
			for (int j = 0; j < ranges.GetLength(1); j++)
			{
				if (j == 0)
				{
					num2 = ranges[i, j];
				}
				else
				{
					num3 = ranges[i, j];
				}
			}
			if (num2 <= count - 1 && num3 <= count - 1)
			{
				for (int k = num2; k <= num3; k += num3 + 1)
				{
					ImportDocumentPages(num2, num3, null, num);
					num++;
				}
			}
		}
	}

	internal void ImportDocumentPages(int startIndex, int endIndex, string destFilePattern, int docNum)
	{
		PdfDocument pdfDocument = new PdfDocument();
		if (!IsUnusedResourceSplit)
		{
			pdfDocument.EnableMemoryOptimization = true;
		}
		pdfDocument.ImportPageRange(this, startIndex, endIndex);
		MemoryStream memoryStream = new MemoryStream();
		pdfDocument.Save(memoryStream);
		memoryStream.Position = 0L;
		if (this.DocumentSplitEvent != null)
		{
			PdfDocumentSplitEventArgs args = new PdfDocumentSplitEventArgs(memoryStream);
			this.DocumentSplitEvent(this, args);
		}
		memoryStream?.Dispose();
		pdfDocument.Close(completely: true);
	}

	public void CreateForm()
	{
		if (m_form == null)
		{
			m_form = new PdfLoadedForm(base.CrossTable);
			base.Catalog.SetProperty("AcroForm", new PdfReferenceHolder(m_form));
			base.Catalog.LoadedForm = m_form;
		}
	}

	public PdfAttachmentCollection CreateAttachment()
	{
		m_attachments = new PdfAttachmentCollection();
		base.Catalog.CreateNamesIfNone();
		base.Catalog.Names.EmbeddedFiles = m_attachments;
		return m_attachments;
	}

	public PdfBookmarkBase CreateBookmarkRoot()
	{
		m_bookmark = new PdfBookmarkBase();
		return m_bookmark;
	}

	private PdfNamedDestinationCollection CreateNamedDestinations()
	{
		m_namedDestinations = new PdfNamedDestinationCollection();
		if (base.Catalog.Names == null)
		{
			base.Catalog.CreateNamesIfNone();
		}
		PdfReferenceHolder pdfReferenceHolder = base.Catalog["Names"] as PdfReferenceHolder;
		if (pdfReferenceHolder != null)
		{
			if (pdfReferenceHolder.Object is PdfDictionary pdfDictionary)
			{
				pdfDictionary.SetProperty("Dests", new PdfReferenceHolder(m_namedDestinations));
			}
		}
		else
		{
			base.Catalog.SetProperty("Names", new PdfReferenceHolder(m_namedDestinations));
		}
		base.Catalog.Modify();
		return m_namedDestinations;
	}

	private PdfPageTemplateCollection CreatePageTemplates()
	{
		m_pageTemplatesCollection = new PdfPageTemplateCollection();
		if (base.Catalog.Names == null)
		{
			base.Catalog.CreateNamesIfNone();
		}
		base.Catalog.SetProperty("Names", new PdfReferenceHolder(m_pageTemplatesCollection));
		base.Catalog.Modify();
		return m_pageTemplatesCollection;
	}

	private void RemovePDFConformanceInfo()
	{
		try
		{
			XmpMetadata xmpMetadata = DocumentInformation.XmpMetadata;
			if (xmpMetadata == null || xmpMetadata.XmlData == null)
			{
				return;
			}
			XDocument xmlData = xmpMetadata.XmlData;
			XElement root = xmlData.Root;
			bool flag = false;
			if (root != null && root.NodeType == XmlNodeType.Element)
			{
				foreach (XElement item in root.Elements())
				{
					XName name = item.Name;
					if (!(name != null) || !(name.LocalName.ToLower() == "rdf") || !item.HasElements)
					{
						continue;
					}
					foreach (XElement item2 in item.Elements())
					{
						if (item2.NodeType != XmlNodeType.Element)
						{
							continue;
						}
						XName name2 = item2.Name;
						if (!(name2 != null) || !(name2.LocalName.ToLower() == "description") || !item2.HasAttributes)
						{
							continue;
						}
						foreach (XAttribute item3 in item2.Attributes())
						{
							if (item3 != null && item3.Name != null && item3.Name.LocalName != null && (item3.Name.LocalName == "pdfaid" || item3.Name.LocalName == "nonpdfaid" || item3.Name.LocalName == "pdfa"))
							{
								item2.Remove();
								flag = true;
								break;
							}
						}
					}
				}
			}
			if (flag)
			{
				DocumentInformation.XmpMetadata.Load(xmlData);
				Conformance = PdfConformanceLevel.None;
				ConformanceEnabled = false;
			}
		}
		catch
		{
		}
	}

	private string CheckLicense()
	{
		_ = string.Empty;
		return "";
	}

	public override void Save(Stream stream)
	{
		if (stream.CanSeek && stream.CanWrite && m_stream.CanWrite && m_stream.CanRead && stream == m_stream && stream.Length == m_stream.Length && PdfDocument.ConformanceLevel == PdfConformanceLevel.None)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				Save(memoryStream);
				stream.Position = 0L;
				memoryStream.Position = 0L;
				byte[] array = new byte[8190];
				int count;
				while ((count = memoryStream.Read(array, 0, array.Length)) != 0)
				{
					stream.Write(array, 0, count);
				}
				return;
			}
		}
		if (existingConformanceLevel != 0 && existingConformanceLevel != PdfConformanceLevel.Pdf_A3B && existingConformanceLevel != PdfConformanceLevel.Pdf_A3U && base.Catalog.ContainsKey("Names"))
		{
			base.Catalog.Remove("Names");
		}
		if (Conformance == PdfConformanceLevel.Pdf_A3B || Conformance == PdfConformanceLevel.Pdf_A3A || Conformance == PdfConformanceLevel.Pdf_A3U || ((Conformance == PdfConformanceLevel.Pdf_A4 || Conformance == PdfConformanceLevel.Pdf_A4E || Conformance == PdfConformanceLevel.Pdf_A4F) && base.Catalog != null && base.Catalog.Names != null && Attachments != null && Attachments.Count > 0))
		{
			PdfName key = new PdfName("AFRelationship");
			PdfArray pdfArray = new PdfArray();
			if (Attachments != null)
			{
				for (int i = 0; i < Attachments.Count; i++)
				{
					if (!Attachments[i].Dictionary.Items.ContainsKey(key))
					{
						Attachments[i].Dictionary.Items[key] = new PdfName(PdfAttachmentRelationship.Alternative);
					}
					pdfArray.Add(new PdfReferenceHolder(Attachments[i].Dictionary));
				}
			}
			if (base.Catalog["AF"] is PdfArray { Count: 0 })
			{
				base.Catalog.Items[new PdfName("AF")] = pdfArray;
			}
		}
		using PdfWriter pdfWriter = new PdfWriter(stream);
		pdfWriter.isCompress = isCompressPdf;
		if (m_isXfaDocument || (IsXFAForm && Form.EnableXfaFormFill && Form != null && Form.LoadedXfa != null && Form.LoadedXfa.Fields != null))
		{
			Form.LoadedXfa.Save(this);
		}
		if (!string.IsNullOrEmpty(CheckLicense()))
		{
			lock (PdfDocumentBase.s_licenseLock)
			{
				AddWatermark();
			}
		}
		if (isRedacted)
		{
			base.FileStructure.IncrementalUpdate = false;
		}
		if (m_bookmark != null && m_bookmark.Count > 0 && base.CrossTable != null && base.CrossTable.DocumentCatalog != null && !base.CrossTable.DocumentCatalog.ContainsKey("Outlines"))
		{
			base.Catalog.SetProperty("Outlines", new PdfReferenceHolder(m_bookmark));
		}
		if (base.Security.EncryptOnlyAttachment && !isAttachmentOnlyEncryption && base.Security.UserPassword == string.Empty)
		{
			throw new PdfException("User password cannot be empty for encrypt only attachment.");
		}
		if (base.Security.m_modifiedSecurity && isAttachmentOnlyEncryption)
		{
			PdfEncryptor encryptor = base.Security.Encryptor;
			bool encryptOnlyAttachment = base.Security.EncryptOnlyAttachment;
			PdfEncryptionOptions encryptionOptions = base.Security.EncryptionOptions;
			string userPassword = base.Security.UserPassword;
			_ = Attachments;
			foreach (PdfLoadedPage page in Pages)
			{
				_ = page.Annotations;
			}
			if (base.Security.Encryptor.EncryptOnlyAttachment)
			{
				base.FileStructure.IncrementalUpdate = false;
			}
			base.Security.UserPassword = userPassword;
			base.Security.EncryptOnlyAttachment = encryptOnlyAttachment;
			base.Security.Encryptor = encryptor;
			base.Security.EncryptionOptions = encryptionOptions;
			if (base.Security.Encryptor.EncryptOnlyAttachment && base.Security.Encryptor.UserPassword.Length == 0)
			{
				base.Security.Encryptor.EncryptOnlyAttachment = false;
			}
		}
		bool flag = false;
		if (base.CrossTable.DocumentCatalog != null && base.CrossTable.DocumentCatalog.ContainsKey("Perms"))
		{
			flag = (PdfCrossTable.Dereference(base.CrossTable.DocumentCatalog["Perms"]) as PdfDictionary).ContainsKey("UR3");
		}
		if (!base.Security.Enabled && base.CrossTable.Trailer.ContainsKey("Encrypt") && !base.Security.m_encryptOnlyAttachment)
		{
			base.FileStructure.IncrementalUpdate = false;
		}
		if (m_pageTemplatesCollection != null && m_pageTemplatesCollection.Count > 0)
		{
			Dictionary<PdfPageBase, PdfPageTemplate> dictionary = new Dictionary<PdfPageBase, PdfPageTemplate>();
			foreach (PdfPageTemplate item in (IEnumerable)m_pageTemplatesCollection)
			{
				dictionary[item.PdfPageBase] = item;
			}
			m_pageTemplatesCollection.Clear();
			foreach (KeyValuePair<PdfPageBase, PdfPageTemplate> item2 in dictionary)
			{
				m_pageTemplatesCollection.Add(item2.Value);
			}
			for (int j = 0; j < m_pageTemplatesCollection.Count; j++)
			{
				PdfPageTemplate pdfPageTemplate2 = m_pageTemplatesCollection[j];
				if (pdfPageTemplate2 != null && PageCount > 1 && !pdfPageTemplate2.IsVisible)
				{
					Pages.Remove(pdfPageTemplate2.PdfPageBase);
				}
				else if (pdfPageTemplate2.IsVisible && Pages.GetIndex(pdfPageTemplate2.PdfPageBase) < 0 && !pdfPageTemplate2.PdfPageBase.m_removedPage)
				{
					pdfPageTemplate2.PdfPageBase = Pages.Add(this, pdfPageTemplate2.PdfPageBase);
				}
			}
			dictionary.Clear();
		}
		if (base.Security.Enabled == m_bWasEncrypted && base.Security.OwnerPassword.Length == 0 && base.Security.UserPassword.Length == 0 && base.FileStructure.IncrementalUpdate && !base.CrossTable.isOpenAndRepair && (!base.Security.Enabled || !base.Security.m_modifiedSecurity))
		{
			if (pdfWriter.Length > 0)
			{
				if (!m_isImported)
				{
					m_isStreamCopied = true;
				}
				pdfWriter.Position = pdfWriter.Length;
				AppendDocument(pdfWriter);
				return;
			}
			CopyOldStream(pdfWriter);
			if (!m_isImported)
			{
				m_isStreamCopied = true;
			}
			if (base.Catalog.Changed)
			{
				ReadDocumentInfo();
			}
			if (!IsSkipSaving)
			{
				AppendDocument(pdfWriter);
			}
			return;
		}
		if (flag && !isCompressed && base.FileStructure.IncrementalUpdate)
		{
			CopyOldStream(pdfWriter);
			if (!m_isImported)
			{
				m_isStreamCopied = true;
			}
			if (!IsSkipSaving)
			{
				AppendDocument(pdfWriter);
			}
			return;
		}
		bool flag2 = false;
		if (base.FileStructure.Version <= PdfVersion.Version1_2)
		{
			base.FileStructure.Version = PdfVersion.Version1_4;
			flag2 = true;
		}
		if (base.Security.Enabled || !base.FileStructure.IncrementalUpdate)
		{
			ReadDocumentInfo();
		}
		PdfCrossTable crossTable = base.CrossTable;
		if (base.CrossTable.isOpenAndRepair)
		{
			SetCrossTable(new PdfCrossTable(crossTable.Count, crossTable.EncryptorDictionary, crossTable.DocumentCatalog, crossTable.CrossTable));
		}
		else if (!flag2 && base.FileStructure.IncrementalUpdate && !base.Security.m_modifiedSecurity)
		{
			CopyOldStream(pdfWriter);
			if (!m_isImported)
			{
				m_isStreamCopied = true;
			}
		}
		else
		{
			SetCrossTable(new PdfCrossTable(crossTable.Count, crossTable.EncryptorDictionary, crossTable.DocumentCatalog));
		}
		base.CrossTable.Document = this;
		if (DocumentInformation != null && existingConformanceLevel != PdfConformanceLevel.Pdf_A4 && existingConformanceLevel != PdfConformanceLevel.Pdf_A4E && existingConformanceLevel != PdfConformanceLevel.Pdf_A4F)
		{
			base.CrossTable.Trailer["Info"] = new PdfReferenceHolder(DocumentInformation);
		}
		AppendDocument(pdfWriter);
	}

	private void AddWatermark()
	{
		if (m_licensingAdded)
		{
			return;
		}
	}

	internal override void AddFields(PdfLoadedDocument ldDoc, PdfPageBase newPage, List<PdfField> fields)
	{
		if (fields.Count > 0 && Form == null)
		{
			CreateForm();
		}
		Form.Fields.m_isImported = true;
		int i = 0;
		for (int count = fields.Count; i < count; i++)
		{
			PdfField field = fields[i];
			Form.Fields.Add(field, newPage);
		}
	}

	internal override PdfPageBase ClonePage(PdfLoadedDocument ldDoc, PdfPageBase page, List<PdfArray> destinations)
	{
		return Pages.Add(ldDoc, page, destinations);
	}

	internal override PdfForm ObtainForm()
	{
		if (Form == null)
		{
			CreateForm();
		}
		return Form;
	}

	public void Dispose()
	{
		if (base.EnableMemoryOptimization)
		{
			Close(completely: true);
		}
		else
		{
			Dispose(dispose: true);
		}
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool dispose)
	{
		if (m_isDisposed)
		{
			return;
		}
		m_isDisposed = true;
		if (dispose && base.EnableMemoryOptimization)
		{
			if (m_bookmark != null)
			{
				m_bookmark.Dispose();
			}
			if (m_bookmarkHashtable != null)
			{
				m_bookmarkHashtable.Clear();
			}
			m_documentInfo = null;
			m_form = null;
			m_internalStream = null;
			m_openStream = null;
			m_pageLabel = null;
			m_pageLabelCollection = null;
			m_redactionPages.Clear();
			m_dublinschema = null;
		}
		m_stream = null;
		m_pages = null;
		m_form = null;
		m_bookmark = null;
		this.SubstituteFont = null;
	}

	public override void Close(bool completely)
	{
		if (completely && !PdfDocument.EnableCache)
		{
			isCompressed = true;
		}
		if (m_pages != null)
		{
			m_pages.m_closeCompletely = completely;
		}
		if (completely && m_pages != null && base.EnableMemoryOptimization)
		{
			m_pages.Clear(isCompressed);
		}
		else if (isCompressed && m_pages != null)
		{
			m_pages.Clear(isCompressed);
		}
		if (m_pages != null && m_pages.m_pageIndexCollection != null && m_pages.m_pageIndexCollection.Count > 0)
		{
			m_pages.m_pageIndexCollection.Clear();
		}
		if (m_pageTemplatesCollection != null)
		{
			m_pageTemplatesCollection.Clear();
		}
		base.Close(completely);
		if (base.EnableMemoryOptimization || isCompressed)
		{
			Dispose(completely);
		}
		else
		{
			Dispose();
		}
		if (m_redactAnnotationCollection != null)
		{
			m_redactAnnotationCollection.Clear();
		}
		PdfDocument.FontEmbeddingEnabled = false;
	}

	public object Clone()
	{
		return MemberwiseClone();
	}

	internal PdfConformanceLevel GetDocumentConformance(PdfConformanceLevel m_conformance)
	{
		PdfArray pdfArray = PdfCrossTable.Dereference(base.Catalog["OutputIntents"]) as PdfArray;
		if (pdfArray != null)
		{
			for (int i = 0; i < pdfArray.Count; i++)
			{
				if (PdfCrossTable.Dereference(pdfArray[i]) is PdfDictionary pdfDictionary)
				{
					PdfName pdfName = PdfCrossTable.Dereference(pdfDictionary["S"]) as PdfName;
					if (pdfName.Value == "GTS_PDFA1")
					{
						m_conformance = PdfConformanceLevel.Pdf_A1B;
						break;
					}
					if (pdfName.Value == "GTS_PDFX" && DocumentInformation.Dictionary.ContainsKey("GTS_PDFXConformance") && (PdfCrossTable.Dereference(DocumentInformation.Dictionary["GTS_PDFXConformance"]) as PdfString).Value == "PDF/X-1a:2001")
					{
						m_conformance = PdfConformanceLevel.Pdf_X1A2001;
						break;
					}
				}
			}
		}
		if (pdfArray == null || m_conformance == PdfConformanceLevel.Pdf_A1B)
		{
			string text = "pdfaid:part";
			string text2 = "pdfaid:conformance";
			DocumentInformation.isConformanceCheck = true;
			XElement xElement = null;
			IEnumerable<XElement> enumerable = null;
			if (DocumentInformation.XmpMetadata.XmlData.Root.Name.LocalName == "xmpmeta")
			{
				xElement = DocumentInformation.XmpMetadata.Xmpmeta;
			}
			if (DocumentInformation.XmpMetadata.DocumentInfo == null)
			{
				DocumentInformation.XmpMetadata.DocumentInfo = DocumentInformation;
			}
			DocumentInformation.isConformanceCheck = false;
			bool found = false;
			if (xElement != null)
			{
				enumerable = xElement.Elements();
			}
			if (xElement != null && enumerable != null)
			{
				string text3 = string.Empty;
				string text4 = string.Empty;
				foreach (XElement item in enumerable.Descendants())
				{
					foreach (XAttribute item2 in item.Attributes())
					{
						if (item2.Name.LocalName == "part")
						{
							text3 = item2.Value;
						}
						if (item2.Name.LocalName == "conformance")
						{
							text4 = item2.Value;
						}
						if (!string.IsNullOrEmpty(text3) && !string.IsNullOrEmpty(text4))
						{
							string conformanceValue = text3 + text4;
							m_conformance = GetConformanceLevel(conformanceValue, out found);
						}
						if (text3 == "4" && string.IsNullOrEmpty(text4))
						{
							string text5 = string.Empty;
							string empty = string.Empty;
							if (item2.NextAttribute != null && !string.IsNullOrEmpty(item2.NextAttribute.Value))
							{
								text5 = item2.NextAttribute.Value;
							}
							empty = (string.IsNullOrEmpty(text5) ? text3 : (text3 + text5));
							m_conformance = GetConformanceLevel(empty, out found);
						}
						if (found)
						{
							break;
						}
					}
					if (item.Element("pdfaid") != null)
					{
						if (item.Element(text) != null && item.Element(text).Value != null && item.Element(text2) != null && item.Element(text2).Value != null)
						{
							string conformanceValue2 = item.Element(text).Value + item.Element(text2).Value;
							m_conformance = GetConformanceLevel(conformanceValue2, out found);
							break;
						}
					}
					else if (item.Name != null && (item.Name.ToString().Contains("pdfaid") || item.Name.ToString().Contains("http://www.aiim.org/pdfa/ns/id/")))
					{
						if (item.Name.LocalName == "part")
						{
							text3 = item.Value;
						}
						if (item.Name.LocalName == "conformance")
						{
							text4 = item.Value;
						}
						if (!string.IsNullOrEmpty(text3) && !string.IsNullOrEmpty(text4))
						{
							string conformanceValue3 = text3 + text4;
							m_conformance = GetConformanceLevel(conformanceValue3, out found);
							break;
						}
					}
					if (found)
					{
						break;
					}
				}
				text3 = null;
				text4 = null;
			}
			if (!found)
			{
				m_conformance = PdfConformanceLevel.None;
			}
		}
		return m_conformance;
	}

	private PdfConformanceLevel GetConformanceLevel(string conformanceValue, out bool found)
	{
		PdfConformanceLevel result = PdfConformanceLevel.None;
		found = false;
		switch (conformanceValue)
		{
		case "1A":
			result = PdfConformanceLevel.Pdf_A1A;
			found = true;
			break;
		case "1B":
			result = PdfConformanceLevel.Pdf_A1B;
			found = true;
			break;
		case "2A":
			result = PdfConformanceLevel.Pdf_A2A;
			found = true;
			break;
		case "2B":
			result = PdfConformanceLevel.Pdf_A2B;
			found = true;
			break;
		case "2U":
			result = PdfConformanceLevel.Pdf_A2U;
			found = true;
			break;
		case "3A":
			result = PdfConformanceLevel.Pdf_A3A;
			found = true;
			break;
		case "3B":
			result = PdfConformanceLevel.Pdf_A3B;
			found = true;
			break;
		case "3U":
			result = PdfConformanceLevel.Pdf_A3U;
			found = true;
			break;
		case "4":
			result = PdfConformanceLevel.Pdf_A4;
			found = true;
			break;
		case "4E":
			result = PdfConformanceLevel.Pdf_A4E;
			found = true;
			break;
		case "4F":
			result = PdfConformanceLevel.Pdf_A4F;
			found = true;
			break;
		}
		return result;
	}

	internal void PageLabel()
	{
		PdfDictionary pdfDictionary = base.Catalog["PageLabels"] as PdfDictionary;
		if (pdfDictionary == null)
		{
			pdfDictionary = new PdfDictionary();
			base.Catalog["PageLabels"] = pdfDictionary;
		}
		PdfArray pdfArray = (PdfArray)(pdfDictionary["Nums"] = new PdfArray());
		IPdfPrimitive pointer = base.Catalog["Pages"];
		PdfArray pdfArray2 = (base.CrossTable.GetObject(pointer) as PdfDictionary)["Kids"] as PdfArray;
		Dictionary<int, PdfPageLabel> dictionary = new Dictionary<int, PdfPageLabel>();
		List<int> list = new List<int>();
		for (int i = 0; i < m_pageLabelCollection.Count; i++)
		{
			PdfPageLabel pdfPageLabel = m_pageLabelCollection[i];
			if (pdfPageLabel.StartPageIndex != -1 && !dictionary.ContainsKey(pdfPageLabel.StartPageIndex))
			{
				list.Add(pdfPageLabel.StartPageIndex);
				dictionary.Add(pdfPageLabel.StartPageIndex, pdfPageLabel);
			}
		}
		if (list.Count > 0)
		{
			list.Sort();
			if (!list.Contains(0))
			{
				PdfPageLabel pdfPageLabel2 = new PdfPageLabel();
				pdfPageLabel2.StartNumber = 1;
				pdfPageLabel2.StartPageIndex = 0;
				pdfArray.Add(new PdfNumber(0));
				pdfArray.Add(((IPdfWrapper)pdfPageLabel2).Element);
			}
			for (int j = 0; j < dictionary.Count; j++)
			{
				PdfPageLabel pdfPageLabel3 = dictionary[list[j]];
				pdfArray.Add(new PdfNumber(pdfPageLabel3.StartPageIndex));
				pdfArray.Add(((IPdfWrapper)pdfPageLabel3).Element);
			}
			list.Clear();
			dictionary.Clear();
			return;
		}
		int num = 0;
		for (int k = 0; k < pdfArray2.Count; k++)
		{
			if (k < m_pageLabelCollection.Count)
			{
				PdfPageLabel pdfPageLabel4 = m_pageLabelCollection[k];
				if (pdfPageLabel4 == null)
				{
					pdfPageLabel4 = new PdfPageLabel();
				}
				pdfArray.Add(new PdfNumber(num));
				if (PdfCrossTable.Dereference(pdfArray2[k]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("Kids") && PdfCrossTable.Dereference(pdfDictionary2["Kids"]) is PdfArray pdfArray3)
				{
					num += pdfArray3.Count;
				}
				pdfArray.Add(((IPdfWrapper)pdfPageLabel4).Element);
			}
		}
	}

	internal void UpdateFormFields()
	{
		PdfDictionary formDictionary = GetFormDictionary();
		if (formDictionary == null)
		{
			return;
		}
		m_form = new PdfLoadedForm(formDictionary, base.CrossTable);
		if (m_form == null || m_form.Fields.Count == 0)
		{
			return;
		}
		base.Catalog.LoadedForm = m_form;
		if (isParsedForm && !m_duplicateAcroform)
		{
			return;
		}
		List<long> list = null;
		if (m_form.CrossTable != null && m_form.CrossTable.Document != null && m_form.CrossTable.Document is PdfLoadedDocument pdfLoadedDocument)
		{
			for (int i = 0; i < pdfLoadedDocument.Pages.Count; i++)
			{
				if (pdfLoadedDocument.Pages[i] is PdfLoadedPage pdfLoadedPage)
				{
					if (list == null && pdfLoadedPage != null)
					{
						list = pdfLoadedPage.GetWidgetReferences();
					}
					m_form.CreateFormFields(pdfLoadedPage, list);
				}
			}
		}
		list?.Clear();
		acroformFieldscount = m_form.Fields.Count;
		m_form.Fields.CreateFormFieldsFromWidgets(acroformFieldscount);
		m_form.m_terminalAddedFieldsNames.Clear();
		isParsedForm = true;
	}

	internal Dictionary<PdfPageBase, object> CreateBookmarkDestinationDictionary()
	{
		PdfBookmarkBase bookmarks = Bookmarks;
		if (m_bookmarkHashtable == null && bookmarks != null)
		{
			m_bookmarkHashtable = new Dictionary<PdfPageBase, object>();
			Stack<CurrentNodeInfo> stack = new Stack<CurrentNodeInfo>();
			CurrentNodeInfo currentNodeInfo = new CurrentNodeInfo(bookmarks.List);
			while (true)
			{
				if (currentNodeInfo.Index < currentNodeInfo.Kids.Count)
				{
					bookmarks = currentNodeInfo.Kids[currentNodeInfo.Index];
					PdfNamedDestination namedDestination = (bookmarks as PdfBookmark).NamedDestination;
					if (namedDestination != null)
					{
						if (namedDestination.Destination != null)
						{
							PdfPageBase page = namedDestination.Destination.Page;
							List<object> list = (m_bookmarkHashtable.ContainsKey(page) ? (m_bookmarkHashtable[page] as List<object>) : null);
							if (list == null)
							{
								list = new List<object>();
								m_bookmarkHashtable[page] = list;
							}
							list.Add(bookmarks);
						}
					}
					else
					{
						PdfDestination destination = (bookmarks as PdfBookmark).Destination;
						if (destination != null)
						{
							PdfPageBase page2 = destination.Page;
							List<object> list2 = (m_bookmarkHashtable.ContainsKey(page2) ? (m_bookmarkHashtable[page2] as List<object>) : null);
							if (list2 == null)
							{
								list2 = new List<object>();
								m_bookmarkHashtable[page2] = list2;
							}
							list2.Add(bookmarks);
						}
					}
					currentNodeInfo.Index++;
					if (bookmarks.Count > 0)
					{
						stack.Push(currentNodeInfo);
						currentNodeInfo = new CurrentNodeInfo(bookmarks.List);
					}
					continue;
				}
				if (stack.Count > 0)
				{
					currentNodeInfo = stack.Pop();
					while (currentNodeInfo.Index == currentNodeInfo.Kids.Count && stack.Count > 0)
					{
						currentNodeInfo = stack.Pop();
					}
				}
				if (currentNodeInfo.Index >= currentNodeInfo.Kids.Count)
				{
					break;
				}
			}
		}
		return m_bookmarkHashtable;
	}

	internal PdfArray GetNamedDestination(PdfName name)
	{
		PdfDictionary destinations = base.Catalog.Destinations;
		PdfArray result = null;
		if (destinations != null)
		{
			result = ExtractDestination(destinations[name]);
		}
		return result;
	}

	internal PdfArray GetNamedDestination(PdfString name)
	{
		PdfCatalogNames names = base.Catalog.Names;
		PdfArray result = null;
		if (name != null && base.Catalog.Names != null)
		{
			PdfDictionary destinations = names.Destinations;
			result = ExtractDestination(names.GetNamedObjectFromTree(destinations, name));
		}
		return result;
	}

	private static PdfArray ExtractDestination(IPdfPrimitive obj)
	{
		PdfDictionary pdfDictionary = null;
		if (obj is PdfDictionary)
		{
			pdfDictionary = obj as PdfDictionary;
		}
		else if (obj is PdfReferenceHolder)
		{
			PdfReferenceHolder pdfReferenceHolder = obj as PdfReferenceHolder;
			if (pdfReferenceHolder.Object is PdfDictionary)
			{
				pdfDictionary = pdfReferenceHolder.Object as PdfDictionary;
			}
			else if (pdfReferenceHolder.Object is PdfArray)
			{
				obj = pdfReferenceHolder.Object as PdfArray;
			}
		}
		PdfArray result = obj as PdfArray;
		if (pdfDictionary != null)
		{
			obj = PdfCrossTable.Dereference(pdfDictionary["D"]);
			result = obj as PdfArray;
		}
		return result;
	}

	private void LoadDocument(Stream file)
	{
		if (!file.CanRead || !file.CanSeek)
		{
			throw new ArgumentException("Can't use the specified stream.", "file");
		}
		m_stream = file;
		PdfMainObjectCollection mainObjectCollection = new PdfMainObjectCollection();
		SetMainObjectCollection(mainObjectCollection);
		try
		{
			PdfCrossTable pdfCrossTable = (validateSyntax ? new PdfCrossTable(file, this) : (m_isOpenAndRepair ? new PdfCrossTable(file, isOpenAndRepair) : ((m_isOpenAndRepair || !isOpenAndRepair) ? new PdfCrossTable(file) : new PdfCrossTable(file, m_isOpenAndRepair, repair: true))));
			pdfCrossTable.Document = this;
			if (pdfCrossTable.StructureAltered)
			{
				pdfCrossTable.Document.FileStructure.IncrementalUpdate = false;
			}
			SetCrossTable(pdfCrossTable);
			m_bWasEncrypted = CheckEncryption(isAttachEncryption: false);
			PdfCatalog catalogValue = GetCatalogValue();
			if (catalogValue != null && catalogValue.ContainsKey("Pages") && !catalogValue.ContainsKey("Type"))
			{
				catalogValue.Items.Add(new PdfName("Type"), new PdfName("Catalog"));
			}
			if (catalogValue.ContainsKey("Type"))
			{
				if (!(catalogValue["Type"] as PdfName).Value.Contains("Catalog"))
				{
					catalogValue["Type"] = new PdfName("Catalog");
				}
				if ((catalogValue["Type"] as PdfName).Value.Contains("Catalog"))
				{
					if (catalogValue.ContainsKey("Perms"))
					{
						m_isExtendedFeatureEnabled = true;
						if (catalogValue["Perms"] is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("UR3"))
						{
							PdfDictionary pdfDictionary2 = pdfDictionary["UR3"] as PdfDictionary;
							if (pdfDictionary2 == null && pdfDictionary["UR3"] is PdfReferenceHolder)
							{
								pdfDictionary2 = (pdfDictionary["UR3"] as PdfReferenceHolder).Object as PdfDictionary;
							}
							if (pdfDictionary2 != null && pdfDictionary2.ContainsKey("Contents") && !(pdfDictionary2["Contents"] as PdfString).m_isHexString)
							{
								throw new PdfException("Cannot load the pdf file unexpected hex string");
							}
						}
					}
					SetCatalog(catalogValue);
					bool flag = false;
					if (catalogValue.ContainsKey("Version"))
					{
						PdfName pdfName = catalogValue["Version"] as PdfName;
						if (pdfName != null)
						{
							SetFileVersion("PDF-" + pdfName.Value);
							flag = true;
						}
					}
					if (!flag)
					{
						ReadFileVersion();
					}
					CheckIfTagged();
					return;
				}
				throw new PdfException("Cannot find the Pdf catalog information");
			}
			throw new PdfException("Cannot find the Pdf catalog information");
		}
		catch (Exception ex)
		{
			if (isOpenAndRepair && !m_isOpenAndRepair)
			{
				m_isOpenAndRepair = true;
				LoadDocument(file);
				return;
			}
			if (ex is PdfInvalidPasswordException)
			{
				throw new PdfInvalidPasswordException(ex.Message);
			}
			if (ex is PdfDocumentException)
			{
				throw new PdfDocumentException(ex.Message);
			}
			if (validateSyntax)
			{
				PdfException item = new PdfException(ex.Message);
				pdfException.Add(item);
				return;
			}
			throw new PdfException(ex.Message);
		}
	}

	private void CheckIfTagged()
	{
		if (base.CrossTable.DocumentCatalog["MarkInfo"] is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("Marked") && PdfCrossTable.Dereference(pdfDictionary["Marked"]) is PdfBoolean pdfBoolean)
		{
			base.FileStructure.TaggedPdf = pdfBoolean.Value;
		}
	}

	private void ReadFileVersion()
	{
		PdfReader pdfReader = new PdfReader(m_stream);
		pdfReader.Position = 0L;
		string nextToken = pdfReader.GetNextToken();
		if (nextToken.StartsWith("%"))
		{
			nextToken = pdfReader.GetNextToken();
			if (nextToken != null)
			{
				SetFileVersion(nextToken);
			}
		}
	}

	private void SetFileVersion(string token)
	{
		if (token == null)
		{
			return;
		}
		int length = token.Length;
		if (length != 7)
		{
			return;
		}
		switch (token[6])
		{
		case '4':
			if (token == "PDF-1.4")
			{
				base.FileStructure.Version = PdfVersion.Version1_4;
			}
			break;
		case '0':
			if (!(token == "PDF-1.0"))
			{
				if (token == "PDF-2.0")
				{
					base.FileStructure.Version = PdfVersion.Version2_0;
				}
			}
			else
			{
				base.FileStructure.Version = PdfVersion.Version1_0;
				base.FileStructure.IncrementalUpdate = false;
			}
			break;
		case '1':
			if (token == "PDF-1.1")
			{
				base.FileStructure.Version = PdfVersion.Version1_1;
				base.FileStructure.IncrementalUpdate = false;
			}
			break;
		case '2':
			if (token == "PDF-1.2")
			{
				base.FileStructure.Version = PdfVersion.Version1_2;
				base.FileStructure.IncrementalUpdate = false;
			}
			break;
		case '3':
			if (token == "PDF-1.3")
			{
				base.FileStructure.Version = PdfVersion.Version1_3;
				base.FileStructure.IncrementalUpdate = false;
			}
			break;
		case '5':
			if (token == "PDF-1.5")
			{
				base.FileStructure.Version = PdfVersion.Version1_5;
			}
			break;
		case '6':
			if (token == "PDF-1.6")
			{
				base.FileStructure.Version = PdfVersion.Version1_6;
			}
			break;
		case '7':
			if (token == "PDF-1.7")
			{
				base.FileStructure.Version = PdfVersion.Version1_7;
			}
			break;
		}
	}

	private PdfCatalog GetCatalogValue()
	{
		PdfCatalog pdfCatalog = new PdfCatalog(this, base.CrossTable.DocumentCatalog);
		base.PdfObjects.ReregisterReference(base.CrossTable.DocumentCatalog, pdfCatalog);
		if (!base.CrossTable.IsMerging)
		{
			pdfCatalog.Position = -1;
		}
		PdfDictionary pdfDictionary = pdfCatalog;
		if (pdfDictionary != null)
		{
			CheckNeedAppearence(pdfDictionary);
		}
		return pdfCatalog;
	}

	private void CheckNeedAppearence(PdfDictionary dictionary)
	{
		if (!dictionary.ContainsKey("AcroForm"))
		{
			return;
		}
		if (dictionary["AcroForm"] is PdfReferenceHolder)
		{
			PdfDictionary pdfDictionary = (dictionary["AcroForm"] as PdfReferenceHolder).Object as PdfDictionary;
			if (pdfDictionary != null && pdfDictionary.ContainsKey("XFA"))
			{
				IsXFAForm = true;
			}
			else if (pdfDictionary != null && pdfDictionary.ContainsKey("NeedAppearances"))
			{
				IsXFAForm = false;
			}
		}
		else if (dictionary["AcroForm"] is PdfDictionary)
		{
			PdfDictionary pdfDictionary2 = dictionary["AcroForm"] as PdfDictionary;
			if (pdfDictionary2.ContainsKey("XFA"))
			{
				IsXFAForm = true;
			}
			else if (pdfDictionary2.ContainsKey("NeedAppearances"))
			{
				IsXFAForm = false;
			}
		}
	}

	internal void ReadDocumentInfo()
	{
		PdfDictionary pdfDictionary = PdfCrossTable.Dereference(base.CrossTable.Trailer["Info"]) as PdfDictionary;
		if (pdfDictionary != null && m_documentInfo == null)
		{
			m_documentInfo = new PdfDocumentInformation(pdfDictionary, base.Catalog);
		}
		PdfReference pdfReference = null;
		_ = Conformance;
		if (pdfDictionary != null && base.Catalog.Metadata != null)
		{
			bool changed = pdfDictionary.Changed;
			bool flag = false;
			XmpMetadata metadata = base.Catalog.Metadata;
			if (m_bWasEncrypted)
			{
				if (base.CrossTable.Trailer["Info"] is PdfReferenceHolder)
				{
					pdfReference = (base.CrossTable.Trailer["Info"] as PdfReferenceHolder).Reference;
				}
				if (metadata.BasicSchema != null && !string.IsNullOrEmpty(metadata.BasicSchema.Label))
				{
					m_documentInfo.Label = metadata.BasicSchema.Label;
				}
				if (metadata.m_hasAttributes)
				{
					DecryptDocumentInfo(pdfDictionary, pdfReference);
					if (pdfDictionary != null)
					{
						if (pdfDictionary.ContainsKey("Producer"))
						{
							m_documentInfo.Dictionary.SetString("Producer", (PdfCrossTable.Dereference(pdfDictionary["Producer"]) as PdfString).Value);
						}
						if (pdfDictionary.ContainsKey("Subject"))
						{
							m_documentInfo.Dictionary.SetString("Subject", (PdfCrossTable.Dereference(pdfDictionary["Subject"]) as PdfString).Value);
						}
						if (pdfDictionary.ContainsKey("Title"))
						{
							m_documentInfo.Dictionary.SetString("Title", (PdfCrossTable.Dereference(pdfDictionary["Title"]) as PdfString).Value);
						}
						if (pdfDictionary.ContainsKey("Keywords"))
						{
							m_documentInfo.Dictionary.SetString("Keywords", (PdfCrossTable.Dereference(pdfDictionary["Keywords"]) as PdfString).Value);
						}
						if (pdfDictionary.ContainsKey("Author"))
						{
							m_documentInfo.Dictionary.SetString("Author", (PdfCrossTable.Dereference(pdfDictionary["Author"]) as PdfString).Value);
						}
					}
				}
				else
				{
					DecryptDocumentInfo(pdfDictionary, pdfReference);
					if (pdfDictionary.ContainsKey("Producer") && metadata.PDFSchema != null && metadata.PDFSchema.Producer != string.Empty && metadata.PDFSchema.Producer != (PdfCrossTable.Dereference(pdfDictionary["Producer"]) as PdfString).Value)
					{
						pdfDictionary["Producer"] = new PdfString(metadata.PDFSchema.Producer);
					}
					if (pdfDictionary.ContainsKey("Author") && PdfCrossTable.Dereference(pdfDictionary["Author"]) is PdfString pdfString && DublinSchema != null && metadata.DublinCoreSchema != null && DublinSchema.Creator.Items != null && DublinSchema.Creator.Items.Length != 0 && DublinSchema.Creator.Items[0] != string.Empty && metadata.DublinCoreSchema.Creator.Items[0] != pdfString.Value)
					{
						pdfDictionary["Author"] = new PdfString(DublinSchema.Creator.Items[0]);
					}
					if ((from e in metadata.XmlData.Elements()
						where e.Value.Contains("Title")
						select e) != null && pdfDictionary.ContainsKey("Title") && DublinSchema != null && DublinSchema.Title.DefaultText != string.Empty && metadata.DublinCoreSchema.Title.DefaultText != (pdfDictionary["Title"] as PdfString).Value)
					{
						pdfDictionary["Title"] = new PdfString(DublinSchema.Title.DefaultText);
					}
				}
			}
			if (pdfDictionary.ContainsKey("Creator"))
			{
				if (m_bWasEncrypted || base.Catalog.Changed)
				{
					if (metadata.m_hasAttributes)
					{
						PdfReferenceHolder pdfReferenceHolder = pdfDictionary["Creator"] as PdfReferenceHolder;
						if (pdfReferenceHolder != null)
						{
							m_documentInfo.Creator = (pdfReferenceHolder.Object as PdfString).Value;
						}
						else
						{
							m_documentInfo.Dictionary.SetString("Creator", (PdfCrossTable.Dereference(pdfDictionary["Creator"]) as PdfString).Value);
						}
					}
					else if (!changed)
					{
						string text = null;
						PdfReferenceHolder pdfReferenceHolder2 = pdfDictionary["Creator"] as PdfReferenceHolder;
						if (pdfReferenceHolder2 != null)
						{
							if (pdfReferenceHolder2.Object is PdfString pdfString2)
							{
								text = pdfString2.Value;
							}
						}
						else if (pdfDictionary["Creator"] is PdfString pdfString3)
						{
							text = pdfString3.Value;
						}
						if (metadata.BasicSchema != null && metadata.BasicSchema.CreatorTool != string.Empty && metadata.BasicSchema.CreatorTool != text)
						{
							pdfDictionary["Creator"] = new PdfString(metadata.BasicSchema.CreatorTool);
						}
					}
				}
				else
				{
					PdfReferenceHolder pdfReferenceHolder3 = pdfDictionary["Creator"] as PdfReferenceHolder;
					if (pdfReferenceHolder3 != null)
					{
						if (pdfReferenceHolder3.Object is PdfString pdfString4 && !string.IsNullOrEmpty(pdfString4.Value) && metadata.BasicSchema != null)
						{
							metadata.BasicSchema.CreatorTool = pdfString4.Value;
						}
					}
					else if (pdfDictionary["Creator"] is PdfString pdfString5 && !string.IsNullOrEmpty(pdfString5.Value) && metadata.BasicSchema != null)
					{
						metadata.BasicSchema.CreatorTool = pdfString5.Value;
					}
				}
			}
			if (pdfDictionary.ContainsKey("CreationDate") && metadata.BasicSchema != null)
			{
				if (m_bWasEncrypted || base.Catalog.Changed)
				{
					if (!changed)
					{
						XmpMetadata xmpMetadata = metadata;
						if (m_bWasEncrypted && pdfReference != null && pdfDictionary.ContainsKey("CreationDate"))
						{
							(PdfCrossTable.Dereference(pdfDictionary["CreationDate"]) as PdfString).Decrypt(base.CrossTable.Encryptor, pdfReference.ObjNum);
						}
						if (PdfCrossTable.Dereference(pdfDictionary["CreationDate"]) is PdfString pdfString6)
						{
							string value = pdfString6.Value;
							DateTime result = DateTime.Now;
							string text2 = "yyyyMMddHHmmss";
							if (value.Length > text2.Length)
							{
								result = new PdfDictionary().GetDateTime(pdfString6);
							}
							else
							{
								DateTime.TryParseExact(value, "yyyyMMddHHmmss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AllowLeadingWhite, out result);
							}
							string text3 = xmpMetadata.BasicSchema.CreateDate.ToString("yyyyMMddHHmmsstt");
							if (metadata.BasicSchema.m_externalCreationDate && text3 != result.ToString("yyyyMMddHHmmsstt"))
							{
								pdfDictionary["CreationDate"] = new PdfString(metadata.BasicSchema.CreateDate.ToString("yyyyMMddHHmmss"));
							}
							else if (metadata.BasicSchema != null && !ConformanceEnabled)
							{
								metadata.BasicSchema.CreateDate = result;
							}
						}
					}
				}
				else
				{
					PdfReferenceHolder pdfReferenceHolder4 = pdfDictionary["CreationDate"] as PdfReferenceHolder;
					string text4 = "";
					PdfString pdfString7 = null;
					if (pdfReferenceHolder4 != null)
					{
						pdfString7 = pdfReferenceHolder4.Object as PdfString;
						if (pdfString7 != null)
						{
							text4 = pdfString7.Value;
						}
					}
					else
					{
						pdfString7 = pdfDictionary["CreationDate"] as PdfString;
						if (pdfString7 != null)
						{
							text4 = pdfString7.Value;
						}
					}
					DateTime result2 = DateTime.Now;
					string text5 = "yyyyMMddHHmmss";
					if (text4.Length > text5.Length)
					{
						result2 = new PdfDictionary().GetDateTime(pdfString7);
					}
					else
					{
						DateTime.TryParseExact(text4, "yyyyMMddHHmmss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AllowLeadingWhite, out result2);
					}
					if (metadata.BasicSchema != null && !ConformanceEnabled)
					{
						metadata.BasicSchema.CreateDate = result2;
					}
				}
			}
			if (pdfDictionary.ContainsKey("ModDate") && metadata.BasicSchema != null)
			{
				if (m_bWasEncrypted || base.Catalog.Changed)
				{
					if (!changed)
					{
						XmpMetadata xmpMetadata2 = metadata;
						if (m_bWasEncrypted && pdfReference != null && pdfDictionary.ContainsKey("ModDate"))
						{
							(PdfCrossTable.Dereference(pdfDictionary["ModDate"]) as PdfString).Decrypt(base.CrossTable.Encryptor, pdfReference.ObjNum);
						}
						if (PdfCrossTable.Dereference(pdfDictionary["ModDate"]) is PdfString pdfString8)
						{
							string value2 = pdfString8.Value;
							DateTime result3 = DateTime.Now;
							string text6 = "yyyyMMddHHmmss";
							if (value2.Length > text6.Length)
							{
								result3 = new PdfDictionary().GetDateTime(pdfString8);
							}
							else
							{
								DateTime.TryParseExact(value2, "yyyyMMddHHmmss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AllowLeadingWhite, out result3);
							}
							string text7 = xmpMetadata2.BasicSchema.ModifyDate.ToString("yyyyMMddHHmmsstt");
							if (metadata.BasicSchema.m_externalModifyDate && text7 != result3.ToString("yyyyMMddHHmmsstt"))
							{
								pdfDictionary["ModDate"] = new PdfString(metadata.BasicSchema.ModifyDate.ToString("yyyyMMddHHmmss"));
							}
							else if (metadata.BasicSchema != null && !ConformanceEnabled)
							{
								metadata.BasicSchema.ModifyDate = result3;
							}
						}
					}
				}
				else
				{
					IPdfPrimitive pdfPrimitive = pdfDictionary["ModDate"] as PdfReferenceHolder;
					string text8 = "";
					PdfString dateTimeStringValue;
					if (pdfPrimitive != null)
					{
						pdfPrimitive = (pdfPrimitive as PdfReferenceHolder).Object;
						dateTimeStringValue = pdfPrimitive as PdfString;
						text8 = (pdfPrimitive as PdfString).Value;
					}
					else
					{
						if (pdfDictionary["ModDate"] is PdfString)
						{
							text8 = (pdfDictionary["ModDate"] as PdfString).Value;
						}
						dateTimeStringValue = pdfDictionary["ModDate"] as PdfString;
					}
					DateTime result4 = DateTime.Now;
					string text9 = "yyyyMMddHHmmss";
					if (text8.Length > text9.Length)
					{
						result4 = new PdfDictionary().GetDateTime(dateTimeStringValue);
					}
					else
					{
						DateTime.TryParseExact(text8, "yyyyMMddHHmmss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AllowLeadingWhite, out result4);
					}
					if (metadata.BasicSchema != null && !ConformanceEnabled)
					{
						metadata.BasicSchema.ModifyDate = result4;
					}
				}
			}
			if (changed && flag)
			{
				base.Catalog.SetProperty("Metadata", metadata);
			}
		}
		if (pdfDictionary != null && !pdfDictionary.Changed && base.CrossTable.Trailer["Info"] is PdfReferenceHolder)
		{
			m_documentInfo = new PdfDocumentInformation(pdfDictionary, base.Catalog);
			if (m_bWasEncrypted)
			{
				if (base.CrossTable.Trailer["Info"] is PdfReferenceHolder)
				{
					pdfReference = (base.CrossTable.Trailer["Info"] as PdfReferenceHolder).Reference;
				}
				DecryptDocumentInfo(pdfDictionary, pdfReference);
			}
			if (base.PdfObjects.IndexOf(((IPdfWrapper)m_documentInfo).Element) > -1)
			{
				base.PdfObjects.ReregisterReference(pdfDictionary, ((IPdfWrapper)m_documentInfo).Element);
				((IPdfWrapper)m_documentInfo).Element.Position = -1;
			}
		}
		if (base.Catalog.Metadata != null)
		{
			base.Catalog.Metadata.m_hasAttributes = false;
		}
	}

	internal bool CheckEncryption(bool isAttachEncryption)
	{
		bool result = false;
		PdfDictionary trailer = base.CrossTable.Trailer;
		PdfDictionary encryptorDictionary = base.CrossTable.EncryptorDictionary;
		bool flag = true;
		if (encryptorDictionary != null && flag)
		{
			if (m_password == null)
			{
				m_password = string.Empty;
			}
			IPdfPrimitive pdfPrimitive = trailer["ID"];
			if (pdfPrimitive == null)
			{
				pdfPrimitive = new PdfArray();
				PdfArray obj = pdfPrimitive as PdfArray;
				PdfString element = new PdfString(new byte[0]);
				obj.Add(element);
			}
			if (pdfPrimitive == null)
			{
				throw new PdfDocumentException("Unable to decrypt document without ID.");
			}
			PdfString key = (pdfPrimitive as PdfArray)[0] as PdfString;
			result = true;
			PdfEncryptor pdfEncryptor = new PdfEncryptor();
			if (encryptorDictionary != null && encryptorDictionary.ContainsKey("EncryptMetadata"))
			{
				pdfEncryptor.EncryptMetaData = (encryptorDictionary["EncryptMetadata"] as PdfBoolean).Value;
			}
			pdfEncryptor.ReadFromDictionary(encryptorDictionary);
			bool attachEncryption = true;
			if (!isAttachEncryption && pdfEncryptor.EncryptOnlyAttachment)
			{
				attachEncryption = false;
			}
			if (!pdfEncryptor.CheckPassword(m_password, key, attachEncryption))
			{
				if (isOpenAndRepair)
				{
					isOpenAndRepair = false;
				}
				throw new PdfInvalidPasswordException("Can't open an encrypted document. The password is invalid.");
			}
			encryptorDictionary.Encrypt = false;
			PdfSecurity pdfSecurity = new PdfSecurity();
			pdfSecurity.m_encryptOnlyAttachment = pdfEncryptor.EncryptOnlyAttachment;
			isAttachmentOnlyEncryption = pdfEncryptor.EncryptOnlyAttachment;
			if (isAttachmentOnlyEncryption)
			{
				pdfSecurity.m_encryptionOption = PdfEncryptionOptions.EncryptOnlyAttachments;
			}
			else if (!pdfEncryptor.EncryptMetaData)
			{
				pdfSecurity.m_encryptionOption = PdfEncryptionOptions.EncryptAllContentsExceptMetadata;
			}
			pdfSecurity.Encryptor = pdfEncryptor;
			SetSecurity(pdfSecurity);
			base.CrossTable.Encryptor = pdfEncryptor;
		}
		return result;
	}

	private PdfDictionary GetFormDictionary()
	{
		return PdfCrossTable.Dereference(base.Catalog["AcroForm"]) as PdfDictionary;
	}

	private PdfDictionary GetAttachmentDictionary()
	{
		return PdfCrossTable.Dereference(base.Catalog["Names"]) as PdfDictionary;
	}

	private PdfDictionary GetPortfolioDictionary()
	{
		return PdfCrossTable.Dereference(base.Catalog["Collection"]) as PdfDictionary;
	}

	private void AppendDocument(PdfWriter writer)
	{
		writer.Document = this;
		if (isPageLabel)
		{
			PageLabel();
		}
		if (base.FileStructure.IncrementalUpdate)
		{
			foreach (PdfPageBase page in Pages)
			{
				if (page != null)
				{
					PdfDictionary dictionary = page.Dictionary;
					if (dictionary != null && dictionary.Changed)
					{
						m_changedPages++;
					}
				}
			}
		}
		base.CrossTable.Save(writer);
		if (progressDelegate != null)
		{
			int count = Pages.Count;
			ProgressEventArgs arguments = new ProgressEventArgs(count, count);
			OnSaveProgress(arguments);
		}
		base.Security.Enabled = true;
		DocumentSavedEventArgs args = new DocumentSavedEventArgs(writer);
		OnDocumentSaved(args);
	}

	private void CopyOldStream(PdfWriter writer)
	{
		m_stream.Position = 0L;
		byte[] array = new byte[8190];
		int end;
		while ((end = m_stream.Read(array, 0, array.Length)) != 0)
		{
			writer.Write(array, end);
		}
	}

	private Stream CheckIfValid(Stream file)
	{
		file.Position = file.Length - 1;
		if (file.ReadByte() == 0)
		{
			byte[] array = new byte[file.Length];
			file.Position = 0L;
			file.Read(array, 0, array.Length);
			int num = array.Length - 1;
			while (array[num] == 0)
			{
				num--;
			}
			byte[] array2 = new byte[num + 1];
			Array.Copy(array, array2, num + 1);
			MemoryStream memoryStream = new MemoryStream();
			memoryStream.Write(array2, 0, array2.Length);
			return memoryStream;
		}
		file.Position = 0L;
		return file;
	}

	private bool CheckLinearization()
	{
		bool result = false;
		new PdfReader(m_stream);
		if (base.CrossTable != null && base.CrossTable.CrossTable != null && base.CrossTable.CrossTable.Parser != null)
		{
			PdfParser parser = base.CrossTable.CrossTable.Parser;
			Dictionary<long, ObjectInformation> newObjects = new Dictionary<long, ObjectInformation>();
			newObjects = parser.FindFirstObject(newObjects, base.CrossTable.CrossTable);
			foreach (KeyValuePair<long, ObjectInformation> item in newObjects)
			{
				ObjectInformation value = item.Value;
				if (value == null || value.Type != ObjectType.Normal)
				{
					continue;
				}
				IPdfPrimitive pdfPrimitive = parser.Parse(value.Offset);
				if (pdfPrimitive != null && PdfCrossTable.Dereference(pdfPrimitive) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("Linearized") && pdfDictionary.ContainsKey("L"))
				{
					long length = m_stream.Length;
					if (PdfCrossTable.Dereference(pdfDictionary["L"]) is PdfNumber pdfNumber)
					{
						result = pdfNumber.LongValue == length;
						break;
					}
				}
			}
		}
		return result;
	}

	public void FlattenAnnotations()
	{
		FlattenAnnotations(flattenPopups: false);
	}

	public void FlattenAnnotations(bool flattenPopups)
	{
		for (int i = 0; i < Pages.Count; i++)
		{
			int num = 0;
			PdfPageBase pdfPageBase = Pages[i];
			if (pdfPageBase == null)
			{
				continue;
			}
			pdfPageBase.isFlatten = true;
			if (pdfPageBase is PdfPage)
			{
				PdfPage pdfPage = pdfPageBase as PdfPage;
				num = pdfPage.Annotations.Count;
				for (int j = 0; j < num; j++)
				{
					PdfAnnotation pdfAnnotation = pdfPage.Annotations[0];
					if (pdfAnnotation != null)
					{
						pdfAnnotation.FlattenAnnot(flattenPopups);
						pdfAnnotation.Dictionary.isSkip = true;
					}
				}
				continue;
			}
			PdfLoadedPage pdfLoadedPage = pdfPageBase as PdfLoadedPage;
			num = pdfPageBase.Annotations.Count;
			for (int k = 0; k < num; k++)
			{
				PdfAnnotation pdfAnnotation2 = pdfPageBase.Annotations[0];
				if (pdfAnnotation2 != null)
				{
					pdfAnnotation2.FlattenAnnot(flattenPopups);
					pdfPageBase.Annotations.Remove(pdfAnnotation2);
					pdfAnnotation2.Dictionary.isSkip = true;
				}
			}
			if (!((pdfPageBase.Dictionary != null) & pdfPageBase.Dictionary.ContainsKey("Annots")))
			{
				continue;
			}
			int count = pdfLoadedPage.m_unsupportedAnnotation.Count;
			for (int l = 0; l < count; l++)
			{
				PdfAnnotation pdfAnnotation3 = pdfLoadedPage.m_unsupportedAnnotation[l];
				if (pdfAnnotation3 != null && pdfAnnotation3 is PdfLoadedTextMarkupAnnotation && pdfAnnotation3.unSupportedAnnotation)
				{
					(pdfAnnotation3 as PdfLoadedTextMarkupAnnotation).FlattenNonSupportAnnotation();
				}
			}
			pdfLoadedPage.m_unsupportedAnnotation.Clear();
		}
	}

	internal void FlattenAnnotations(List<PdfLoadedAnnotationType> annotsTypeToBeIgnored)
	{
		annotationsTypeToBeIgnored = new List<PdfLoadedAnnotationType>(annotsTypeToBeIgnored.ToArray());
		FlattenAnnotations(flattenPopups: false);
		annotationsTypeToBeIgnored.Clear();
	}

	public bool ExportAnnotations(Stream stream, AnnotationDataFormat format)
	{
		return ExportAnnotations(stream, format, string.Empty);
	}

	public bool ExportAnnotations(Stream stream, PdfAnnotationExportSettings settings)
	{
		annotationExportSettings = settings;
		return ExportAnnotations(stream, settings.DataFormat, string.Empty);
	}

	public bool ExportAnnotations(Stream stream, AnnotationDataFormat format, string targetFilePath)
	{
		bool result = true;
		PdfWriter writer = new PdfWriter(stream);
		switch (format)
		{
		case AnnotationDataFormat.Fdf:
			if (!ExportAnnotationsFDF(writer, targetFilePath, null))
			{
				result = true;
			}
			break;
		case AnnotationDataFormat.Json:
			ExportAnnotationsJSON(stream, targetFilePath);
			break;
		default:
			ExportAnnotationsXFDF(stream, targetFilePath);
			break;
		}
		return result;
	}

	public bool ExportAnnotations(Stream stream, AnnotationDataFormat format, PdfExportAnnotationCollection collection)
	{
		return ExportAnnotations(stream, format, string.Empty, collection);
	}

	public bool ExportAnnotations(Stream stream, AnnotationDataFormat format, string targetFilePath, PdfExportAnnotationCollection collection)
	{
		bool result = true;
		if (collection == null)
		{
			throw new ArgumentNullException("Collection");
		}
		if (collection.Count <= 0)
		{
			throw new ArgumentException("Empty collection");
		}
		PdfWriter writer = new PdfWriter(stream);
		switch (format)
		{
		case AnnotationDataFormat.Fdf:
			if (!ExportAnnotationsFDF(writer, targetFilePath, collection))
			{
				result = true;
			}
			break;
		case AnnotationDataFormat.Json:
			ExportAnnotationsJSON(stream, targetFilePath, collection);
			break;
		default:
			ExportAnnotationsXFDF(stream, targetFilePath, collection);
			break;
		}
		return result;
	}

	private void ExportAnnotationsXFDF(Stream stream, string fileName)
	{
		XFdfDocument xFdfDocument = new XFdfDocument(fileName);
		xFdfDocument.IsExportAnnotations = true;
		xFdfDocument.ExportAppearance = annotationExportSettings.ExportAppearance;
		xFdfDocument.AnnotationTypes = annotationExportSettings.AnnotationTypes;
		xFdfDocument.ExportAnnotations(stream, this);
		xFdfDocument.IsExportAnnotations = false;
	}

	private void ExportAnnotationsJSON(Stream stream, string fileName)
	{
		JsonDocument jsonDocument = new JsonDocument(fileName);
		jsonDocument.ExportAppearance = annotationExportSettings.ExportAppearance;
		jsonDocument.ExportAnnotations(stream, this);
	}

	private void ExportAnnotationsJSON(Stream stream, string fileName, PdfExportAnnotationCollection collection)
	{
		JsonDocument jsonDocument = new JsonDocument(fileName);
		jsonDocument.ExportAppearance = annotationExportSettings.ExportAppearance;
		jsonDocument.AnnotationCollection = collection;
		jsonDocument.ExportAnnotations(stream, this);
	}

	private void ExportAnnotationsXFDF(Stream stream, string fileName, PdfExportAnnotationCollection collection)
	{
		XFdfDocument xFdfDocument = new XFdfDocument(fileName);
		xFdfDocument.IsExportAnnotations = true;
		xFdfDocument.AnnotationCollection = collection;
		xFdfDocument.ExportAppearance = annotationExportSettings.ExportAppearance;
		xFdfDocument.ExportAnnotations(stream, this);
		xFdfDocument.IsExportAnnotations = false;
	}

	private bool ExportAnnotationsFDF(PdfWriter writer, string fileName, PdfExportAnnotationCollection collection)
	{
		string text = " 0 ";
		string text2 = "<</";
		writer.Write("%FDF-1.2" + Environment.NewLine);
		int currentID = 2;
		List<string> list = new List<string>();
		if (collection == null)
		{
			PdfLoadedAnnotationType[] annotationTypes = annotationExportSettings.AnnotationTypes;
			bool exportAppearance = annotationExportSettings.ExportAppearance;
			for (int i = 0; i < PageCount; i++)
			{
				foreach (PdfAnnotation annotation in (Pages[i] as PdfLoadedPage).Annotations)
				{
					if (!(annotation is PdfLoadedAnnotation) || !(annotation is PdfLoadedAnnotation pdfLoadedAnnotation) || pdfLoadedAnnotation is PdfLoadedFileLinkAnnotation || pdfLoadedAnnotation is PdfLoadedTextWebLinkAnnotation || pdfLoadedAnnotation is PdfLoadedDocumentLinkAnnotation || pdfLoadedAnnotation is PdfLoadedUriAnnotation)
					{
						continue;
					}
					if (pdfLoadedAnnotation is PdfLoadedRubberStampAnnotation || pdfLoadedAnnotation is PdfLoadedRectangleAnnotation)
					{
						pdfLoadedAnnotation.ExportAnnotation(ref writer, ref currentID, list, i, hasAppearance: true);
					}
					else if (annotationTypes.Length != 0)
					{
						for (int j = 0; j < annotationTypes.Length; j++)
						{
							if (pdfLoadedAnnotation.Type == annotationTypes[j])
							{
								pdfLoadedAnnotation.ExportAnnotation(ref writer, ref currentID, list, i, exportAppearance);
							}
						}
					}
					else
					{
						pdfLoadedAnnotation.ExportAnnotation(ref writer, ref currentID, list, i, exportAppearance);
					}
				}
			}
		}
		else
		{
			foreach (PdfLoadedAnnotation item in collection)
			{
				if (item == null || item is PdfLoadedFileLinkAnnotation || item is PdfLoadedTextWebLinkAnnotation || item is PdfLoadedDocumentLinkAnnotation || item is PdfLoadedUriAnnotation)
				{
					continue;
				}
				int num = Pages.IndexOf(item.Page);
				if (num >= 0)
				{
					if (item is PdfLoadedRubberStampAnnotation)
					{
						item.ExportAnnotation(ref writer, ref currentID, list, num, hasAppearance: true);
					}
					else
					{
						item.ExportAnnotation(ref writer, ref currentID, list, num, hasAppearance: false);
					}
				}
			}
		}
		if (currentID != 2)
		{
			string text3 = "1" + text;
			writer.Write(text3 + "obj\r\n" + text2 + "FDF" + text2 + "Annots[");
			for (int k = 0; k < list.Count - 1; k++)
			{
				writer.Write(list[k] + text + "R ");
			}
			writer.Write(list[list.Count - 1] + text + "R]/F(" + fileName + ")/UF(" + fileName + ")>>/Type/Catalog>>\r\nendobj\r\n");
			writer.Write("trailer\r\n" + text2 + "Root " + text3 + "R>>\r\n%%EOF\r\n");
			return true;
		}
		return false;
	}

	public void ImportAnnotations(Stream stream, AnnotationDataFormat format)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("Annotation Data Stream");
		}
		if (!stream.CanSeek || !stream.CanRead)
		{
			throw new Exception("Ivalid stream");
		}
		switch (format)
		{
		case AnnotationDataFormat.Fdf:
		{
			FdfParser fdfParser = new FdfParser(stream);
			fdfParser.ParseAnnotationData();
			fdfParser.ImportAnnotations(this);
			fdfParser.Dispose();
			break;
		}
		case AnnotationDataFormat.Json:
			new JsonParser(stream, this).ImportAnnotationData(stream);
			break;
		default:
			new XfdfParser(stream, this).ParseAndImportAnnotationData();
			break;
		}
	}

	internal void ImportAnnotations(Stream stream, AnnotationDataFormat format, bool importCustomData)
	{
		ImportCustomData = importCustomData;
		ImportAnnotations(stream, format);
	}

	private void DecryptDocumentInfo(PdfDictionary info, PdfReference infoReference)
	{
		if (info != null && infoReference != null)
		{
			if (info.ContainsKey("Producer") && PdfCrossTable.Dereference(info["Producer"]) is PdfString pdfString)
			{
				pdfString.Decrypt(base.CrossTable.Encryptor, infoReference.ObjNum);
			}
			if (info.ContainsKey("Creator") && PdfCrossTable.Dereference(info["Creator"]) is PdfString pdfString2)
			{
				pdfString2.Decrypt(base.CrossTable.Encryptor, infoReference.ObjNum);
			}
			if (info.ContainsKey("Author") && PdfCrossTable.Dereference(info["Author"]) is PdfString pdfString3)
			{
				pdfString3.Decrypt(base.CrossTable.Encryptor, infoReference.ObjNum);
			}
			if (info.ContainsKey("Title") && PdfCrossTable.Dereference(info["Title"]) is PdfString pdfString4)
			{
				pdfString4.Decrypt(base.CrossTable.Encryptor, infoReference.ObjNum);
			}
			if (info.ContainsKey("Subject") && PdfCrossTable.Dereference(info["Subject"]) is PdfString pdfString5)
			{
				pdfString5.Decrypt(base.CrossTable.Encryptor, infoReference.ObjNum);
			}
			if (info.ContainsKey("CreationDate") && PdfCrossTable.Dereference(info["CreationDate"]) is PdfString pdfString6)
			{
				pdfString6.Decrypt(base.CrossTable.Encryptor, infoReference.ObjNum);
			}
			if (info.ContainsKey("ModDate") && PdfCrossTable.Dereference(info["ModDate"]) is PdfString pdfString7)
			{
				pdfString7.Decrypt(base.CrossTable.Encryptor, infoReference.ObjNum);
			}
			if (info.ContainsKey("Keywords") && PdfCrossTable.Dereference(info["Keywords"]) is PdfString pdfString8)
			{
				pdfString8.Decrypt(base.CrossTable.Encryptor, infoReference.ObjNum);
			}
		}
	}

	private PdfStructureElement GetStructTreeRoot()
	{
		PdfStructureElement pdfStructureElement = null;
		if (PdfCrossTable.Dereference(base.Catalog["StructTreeRoot"]) is PdfDictionary structureDictionary)
		{
			if (IsSingleRootElement(structureDictionary))
			{
				pdfStructureElement = GetStructureElement(structureDictionary, null);
			}
			else
			{
				pdfStructureElement = new PdfStructureElement();
				GetStructureElement(structureDictionary, pdfStructureElement);
			}
		}
		unRepeatedReferenceCollections.Clear();
		return pdfStructureElement;
	}

	internal PdfStructureElement GetStructTreeRoot(PdfDictionary root)
	{
		PdfStructureElement pdfStructureElement = null;
		if (root != null)
		{
			if (IsSingleRootElement(root))
			{
				pdfStructureElement = GetStructureElement(root, null);
			}
			else
			{
				pdfStructureElement = new PdfStructureElement();
				GetStructureElement(root, pdfStructureElement);
			}
		}
		return pdfStructureElement;
	}

	private bool IsSingleRootElement(PdfDictionary structureDictionary)
	{
		bool result = true;
		if (PdfCrossTable.Dereference(structureDictionary["K"]) is PdfArray { Count: >1 })
		{
			result = false;
		}
		return result;
	}

	private PdfStructureElement GetStructureElement(PdfDictionary structureDictionary, PdfStructureElement parent)
	{
		PdfStructureElement pdfStructureElement = null;
		IPdfPrimitive @object = base.CrossTable.GetObject(structureDictionary["K"]);
		if (@object != null)
		{
			if (@object is PdfArray)
			{
				if (@object is PdfArray pdfArray)
				{
					foreach (IPdfPrimitive item in pdfArray)
					{
						PdfReferenceHolder pdfReferenceHolder = item as PdfReferenceHolder;
						if (pdfReferenceHolder != null && pdfReferenceHolder.Reference != null && unRepeatedReferenceCollections.Contains(pdfReferenceHolder.Reference.ObjNum))
						{
							continue;
						}
						if (pdfReferenceHolder != null && pdfReferenceHolder.Reference != null && !unRepeatedReferenceCollections.Contains(pdfReferenceHolder.Reference.ObjNum))
						{
							unRepeatedReferenceCollections.Add(pdfReferenceHolder.Reference.ObjNum);
						}
						IPdfPrimitive object2 = base.CrossTable.GetObject(item);
						if (object2 == null)
						{
							continue;
						}
						if (object2 is PdfDictionary)
						{
							if (!(object2 is PdfDictionary pdfDictionary))
							{
								continue;
							}
							pdfStructureElement = new PdfStructureElement(this, pdfDictionary, m_elementOrder, parent);
							m_elementOrder++;
							pdfStructureElement.IsTagSplitParser = IsPDFSplit;
							if (pdfDictionary.ContainsKey("K"))
							{
								PdfStructureElement structureElement = GetStructureElement(pdfDictionary, pdfStructureElement);
								if (structureElement != null)
								{
									pdfStructureElement.m_child.Add(structureElement);
								}
							}
							if (parent == null)
							{
								return pdfStructureElement;
							}
							parent.m_child.Add(pdfStructureElement);
							if (pdfDictionary.ContainsKey("Pg"))
							{
								PdfDictionary pageDictionary = PdfCrossTable.Dereference(pdfDictionary["Pg"]) as PdfDictionary;
								pdfStructureElement.InitializePageDictionary(pageDictionary);
							}
							if (pdfDictionary.ContainsKey("MCID") && PdfCrossTable.Dereference(pdfDictionary["MCID"]) is PdfNumber pdfNumber)
							{
								pdfStructureElement.m_mcids.Add(pdfNumber.IntValue);
							}
						}
						else if (object2 is PdfNumber && object2 is PdfNumber pdfNumber2)
						{
							parent?.m_mcids.Add(pdfNumber2.IntValue);
						}
					}
				}
				return null;
			}
			if (@object is PdfDictionary)
			{
				if (@object is PdfDictionary pdfDictionary2)
				{
					pdfStructureElement = new PdfStructureElement(this, pdfDictionary2, m_elementOrder, parent);
					m_elementOrder++;
					pdfStructureElement.IsTagSplitParser = IsPDFSplit;
					if (pdfDictionary2.ContainsKey("K"))
					{
						PdfStructureElement structureElement2 = GetStructureElement(pdfDictionary2, pdfStructureElement);
						if (structureElement2 != null)
						{
							pdfStructureElement.m_child.Add(structureElement2);
						}
					}
					if (pdfDictionary2.ContainsKey("Pg") && pdfStructureElement.TagType != PdfTagType.Document)
					{
						pdfStructureElement.InitializePageDictionary(base.CrossTable.GetObject(pdfDictionary2["Pg"]) as PdfDictionary);
					}
					if (pdfDictionary2.ContainsKey("MCID") && PdfCrossTable.Dereference(pdfDictionary2["MCID"]) is PdfNumber pdfNumber3)
					{
						pdfStructureElement.m_mcids.Add(pdfNumber3.IntValue);
					}
				}
			}
			else if (@object is PdfNumber && @object is PdfNumber pdfNumber4)
			{
				parent.m_mcids.Add(pdfNumber4.IntValue);
			}
		}
		return pdfStructureElement;
	}

	private void GetRevisions()
	{
		List<long> list = null;
		if (base.CrossTable == null || base.CrossTable.CrossTable == null)
		{
			return;
		}
		base.CrossTable.CrossTable.PrevOffset.Sort();
		if (base.CrossTable.CrossTable.Reader == null)
		{
			return;
		}
		long position = base.CrossTable.CrossTable.Reader.Position;
		list = base.CrossTable.CrossTable.EofOffset;
		if (list == null)
		{
			list = new List<long>();
			for (int i = 0; i < base.CrossTable.CrossTable.PrevOffset.Count; i++)
			{
				long position2 = base.CrossTable.CrossTable.PrevOffset[i];
				base.CrossTable.CrossTable.Reader.Position = position2;
				string empty = string.Empty;
				do
				{
					empty = base.CrossTable.CrossTable.Reader.ReadLine();
				}
				while (empty != "%%EOF");
				long position3 = base.CrossTable.CrossTable.Reader.Position;
				list.Add(position3);
			}
			list.Add(base.CrossTable.CrossTable.Reader.Stream.Length);
			list.Sort();
			base.CrossTable.CrossTable.EofOffset = list;
		}
		base.CrossTable.CrossTable.Reader.Position = position;
		if (list == null)
		{
			return;
		}
		foreach (long item in list)
		{
			PdfRevision pdfRevision = new PdfRevision();
			pdfRevision.StartPosition = item;
			m_revisions.Add(pdfRevision);
		}
	}
}
