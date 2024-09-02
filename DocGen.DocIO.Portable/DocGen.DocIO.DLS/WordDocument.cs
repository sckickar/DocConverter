using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using DocGen.CompoundFile.DocIO;
using DocGen.CompoundFile.DocIO.Net;
using DocGen.DocIO.DLS.Convertors;
using DocGen.DocIO.DLS.Rendering;
using DocGen.DocIO.DLS.XML;
using DocGen.DocIO.Drawing;
using DocGen.DocIO.ODTConvertion;
using DocGen.DocIO.ReaderWriter;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.Biff_Records.Structures;
using DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;
using DocGen.DocIO.ReaderWriter.DataStreamParser.OLEObject;
using DocGen.DocIO.ReaderWriter.Security;
using DocGen.DocIO.Rendering;
using DocGen.Drawing;
using DocGen.Layouting;
using DocGen.Office;
using DocGen.Office.Markdown;

namespace DocGen.DocIO.DLS;

public class WordDocument : WidgetContainer, IWordDocument, ICompositeEntity, IEntity, IDisposable, IWidgetContainer, IWidget
{
	private const string DEF_NORMAL_STYLE = "Normal";

	internal const string DEF_BULLETS_STYLE = "Bulleted";

	internal const string DEF_NUMBERING_STYLE = "Numbered";

	private const string DEF_NORMAL_TABLESTYLE = "Normal Table";

	private const int DEF_USER_STYLE_ID = 4094;

	private byte m_bFlags1 = 144;

	private byte m_bFlags;

	private byte m_bFlags2;

	private byte m_bFlags3;

	internal TextBodyItem m_prevClonedEntity;

	private FormatType m_actualFormatType;

	private ushort m_fibVersion;

	private string m_duplicateListStyleNames = string.Empty;

	internal char[] WordComparisonDelimiters = new char[33]
	{
		' ', '!', '"', '#', '$', '%', '&', '(', ')', '*',
		'+', ',', '-', '.', '/', ':', ';', '<', '=', '>',
		'?', '@', '[', '\\', ']', '^', '`', '{', '|', '}',
		'~', '”', '\u00a0'
	};

	private FontFamilyNameStringTable m_ffnStringTable;

	internal BuiltinDocumentProperties m_builtinProp = new BuiltinDocumentProperties();

	internal CustomDocumentProperties m_customProp = new CustomDocumentProperties();

	protected WSectionCollection m_sections;

	protected IStyleCollection m_styles;

	internal ListStyleCollection m_listStyles;

	internal Dictionary<string, int> m_sequenceFieldResults;

	private ListOverrideStyleCollection m_listOverrides;

	private BookmarkCollection m_bookmarks;

	private EditableRangeCollection m_editableRanges;

	private FieldCollection m_fields;

	private TextBoxCollection m_txbxItems;

	private new RevisionCollection m_revisions;

	internal MetaProperties m_contentTypeProperties;

	private CommentsCollection m_Comments;

	private CommentsExCollection m_CommentsEx;

	private float m_defaultTabWidth = 36f;

	private CharacterSpacingControl m_characterSpacingControl;

	private MailMerge m_mailMerge;

	private ViewSetup m_viewSetup;

	private Watermark m_watermark;

	private Background m_background;

	private DOPDescriptor m_dop;

	private GrammarSpelling m_grammarSpellingData;

	private EscherClass m_escher;

	private string m_password;

	private byte[] m_macrosData;

	private byte[] m_escherDataContainers;

	private byte[] m_escherContainers;

	private byte[] m_macroCommands;

	private int m_defShapeId = 1;

	private string m_standardAsciiFont;

	private string m_standardFarEastFont;

	private string m_standardNonFarEastFont;

	private string m_standardBidiFont;

	private static readonly object m_threadLocker = new object();

	private XHTMLValidationType m_htmlValidationOption = XHTMLValidationType.None;

	private Stream m_latentStyles;

	private MemoryStream m_latentStyles2010;

	private WCharacterFormat m_defCharFormat;

	internal WParagraphFormat m_defParaFormat;

	private Package m_docxPackage;

	private ImportOptions m_importOption = ImportOptions.UseDestinationStyles;

	private DocVariables m_variables;

	private DocProperties m_props;

	private ParagraphItem m_nextParaItem;

	private TextBodyItem m_prevBodyItem;

	private SaveOptions m_saveOptions;

	private RevisionOptions m_revisionOptions;

	private List<Stream> m_docxProps;

	private SttbfAssoc m_assocStrings;

	private Dictionary<string, string> m_styleNameIds;

	private int m_paraCount;

	private int m_wordCount;

	private int m_charCount;

	private Dictionary<string, string> m_fontSubstitutionTable;

	private string m_htmlBaseUrl = string.Empty;

	private Dictionary<WField, TableOfContent> m_tableOfContent;

	private List<DocGen.Drawing.Font> m_usedFonts;

	private List<string> m_usedFontNames;

	private Settings m_settings;

	private Themes m_themes;

	private Stream m_vbaProject;

	private Stream m_vbaProjectSignature;

	private Stream m_vbaProjectSignatureAgile;

	private PartContainer m_CustomUIPartContainer;

	private PartContainer m_UserCustomizationPartContainer;

	private PartContainer m_CustomXMLContainer;

	private List<MacroData> m_vbaData;

	private List<string> m_docEvents;

	private FormatType m_saveFormatType;

	private ushort m_wordVersion;

	private Stack<WField> m_clonedFields;

	private int m_altChunkCount;

	private ImageCollection m_imageCollection;

	private Footnote m_footnotes;

	private Endnote m_endnotes;

	private Dictionary<string, string> m_listStyleNames;

	private Dictionary<string, Storage> m_oleObjectCollection;

	private static bool m_EnablePartialTrustCode;

	private static bool m_disableDateTimeUpdating;

	private Dictionary<string, CustomXMLPart> m_customXMLParts;

	private PartContainer m_customXMLPartContainer;

	private HTMLImportSettings m_htmlImportSettings;

	private MdImportSettings m_mdImportSettings;

	private Dictionary<string, Dictionary<int, int>> m_lists;

	private Dictionary<string, Dictionary<int, int>> m_listNames;

	private Dictionary<string, int> m_previousListLevel;

	private List<string> m_previousListLevelOverrideStyle;

	internal int PageCount;

	private List<Shape> m_AutoShapeCollection = new List<Shape>();

	private byte[] m_sttbfRMark;

	private FontSettings m_fontSettings;

	private Hyphenator m_hyphenator;

	private List<Entity> m_FloatingItems;

	internal long m_notSupportedElementFlag;

	internal int m_supportedElementFlag_1;

	internal int m_supportedElementFlag_2;

	internal WordDocument m_AltChunkOwner;

	private int m_balloonCount;

	internal string m_metaXmlItem;

	private string syncfusionLicense;

	internal string claimLicenseKeyURL;

	private bool m_isWarnInserted;

	internal int m_tocBookmarkID;

	internal Revision cloneMoveRevision;

	internal long maxShapeId;

	internal Entity m_tocEntryLastEntity;

	internal int m_sectionIndex;

	internal int m_bodyItemIndex;

	internal int m_paraItemIndex;

	internal int m_startRangeIndex;

	internal int m_endRangeIndex;

	internal int m_textStartIndex;

	internal int m_textEndIndex = -1;

	internal string m_authorName;

	internal DateTime m_dateTime;

	internal int m_matchSectionIndex = -1;

	internal int m_matchBodyItemIndex = -1;

	internal int m_matchParaItemIndex = -1;

	internal bool HasNoPictureToCompare;

	private Comparison m_comparison;

	private ComparisonOptions m_comparisonOptions;

	internal int currSectionIndex;

	internal int currBodyItemIndex;

	internal int currParaItemIndex;

	private DocxLaTeXConverter m_docxLaTeXConveter;

	[ThreadStatic]
	internal static IHelper RenderHelper;

	public FontSettings FontSettings => m_fontSettings ?? (m_fontSettings = new FontSettings());

	public Hyphenator Hyphenator => m_hyphenator ?? (m_hyphenator = new Hyphenator());

	public Footnote Footnotes
	{
		get
		{
			if (m_footnotes == null)
			{
				m_footnotes = new Footnote(this);
			}
			return m_footnotes;
		}
		set
		{
			m_footnotes = value;
			if (m_footnotes != null)
			{
				m_footnotes.SetOwner(this);
			}
		}
	}

	public Endnote Endnotes
	{
		get
		{
			if (m_endnotes == null)
			{
				m_endnotes = new Endnote(this);
			}
			return m_endnotes;
		}
		set
		{
			m_endnotes = value;
			if (m_endnotes != null)
			{
				m_endnotes.SetOwner(this);
			}
		}
	}

	public float DefaultTabWidth
	{
		get
		{
			return m_defaultTabWidth;
		}
		set
		{
			m_defaultTabWidth = value;
		}
	}

	internal CharacterSpacingControl CharacterSpacingControl
	{
		get
		{
			return m_characterSpacingControl;
		}
		set
		{
			m_characterSpacingControl = value;
		}
	}

	internal ushort FIBVersion
	{
		get
		{
			return m_fibVersion;
		}
		set
		{
			m_fibVersion = value;
		}
	}

	public override EntityType EntityType => EntityType.WordDocument;

	public BuiltinDocumentProperties BuiltinDocumentProperties
	{
		get
		{
			if (m_builtinProp == null)
			{
				m_builtinProp = new BuiltinDocumentProperties(this);
			}
			return m_builtinProp;
		}
	}

	public Template AttachedTemplate => new Template(AssociatedStrings);

	public bool UpdateStylesOnOpen
	{
		get
		{
			return DOP.LinkStyles;
		}
		set
		{
			DOP.LinkStyles = value;
		}
	}

	public CustomDocumentProperties CustomDocumentProperties
	{
		get
		{
			if (m_customProp == null)
			{
				m_customProp = new CustomDocumentProperties();
			}
			return m_customProp;
		}
	}

	public WSectionCollection Sections
	{
		get
		{
			if (m_sections == null)
			{
				m_sections = new WSectionCollection(this);
			}
			return m_sections;
		}
	}

	public IStyleCollection Styles
	{
		get
		{
			if (m_styles == null)
			{
				m_styles = new StyleCollection(this);
			}
			return m_styles;
		}
	}

	public ListStyleCollection ListStyles
	{
		get
		{
			if (m_listStyles == null)
			{
				m_listStyles = new ListStyleCollection(this);
			}
			return m_listStyles;
		}
	}

	public ImportOptions ImportOptions
	{
		get
		{
			return m_importOption;
		}
		set
		{
			m_importOption = value;
			UpdateImportOption();
		}
	}

	public BookmarkCollection Bookmarks
	{
		get
		{
			if (m_bookmarks == null)
			{
				m_bookmarks = new BookmarkCollection(this);
			}
			return m_bookmarks;
		}
	}

	internal EditableRangeCollection EditableRanges
	{
		get
		{
			if (m_editableRanges == null)
			{
				m_editableRanges = new EditableRangeCollection(this);
			}
			return m_editableRanges;
		}
	}

	public CommentsCollection Comments
	{
		get
		{
			if (m_Comments == null)
			{
				m_Comments = new CommentsCollection(this);
			}
			return m_Comments;
		}
	}

	internal CommentsExCollection CommentsEx
	{
		get
		{
			if (m_CommentsEx == null)
			{
				m_CommentsEx = new CommentsExCollection(this);
			}
			return m_CommentsEx;
		}
	}

	public TextBoxCollection TextBoxes
	{
		get
		{
			if (m_txbxItems == null)
			{
				m_txbxItems = new TextBoxCollection(this);
			}
			return m_txbxItems;
		}
		set
		{
			m_txbxItems = value;
		}
	}

	public RevisionCollection Revisions
	{
		get
		{
			if (m_revisions == null)
			{
				m_revisions = new RevisionCollection(this);
			}
			return m_revisions;
		}
	}

	public MetaProperties ContentTypeProperties
	{
		get
		{
			if (m_contentTypeProperties == null)
			{
				m_contentTypeProperties = new MetaProperties();
			}
			return m_contentTypeProperties;
		}
	}

	public WSection LastSection
	{
		get
		{
			int count = Sections.Count;
			if (count > 0)
			{
				return Sections[count - 1];
			}
			return null;
		}
	}

	public WParagraph LastParagraph
	{
		get
		{
			WSection lastSection = LastSection;
			if (lastSection != null)
			{
				(lastSection.Body.Paragraphs as WParagraphCollection).ClearIndexes();
				int count = lastSection.Body.Paragraphs.Count;
				if (count > 0)
				{
					return lastSection.Body.Paragraphs[count - 1];
				}
			}
			return null;
		}
	}

	public FootEndNoteNumberFormat EndnoteNumberFormat
	{
		get
		{
			return (FootEndNoteNumberFormat)DOP.EndnoteNumberFormat;
		}
		set
		{
			DOP.EndnoteNumberFormat = (byte)value;
			if (IsOpening)
			{
				return;
			}
			foreach (WSection section in Sections)
			{
				section.PageSetup.EndnoteNumberFormat = value;
			}
		}
	}

	public FootEndNoteNumberFormat FootnoteNumberFormat
	{
		get
		{
			return (FootEndNoteNumberFormat)DOP.FootnoteNumberFormat;
		}
		set
		{
			DOP.FootnoteNumberFormat = (byte)value;
			if (IsOpening)
			{
				return;
			}
			foreach (WSection section in Sections)
			{
				section.PageSetup.FootnoteNumberFormat = value;
			}
		}
	}

	public EndnoteRestartIndex RestartIndexForEndnote
	{
		get
		{
			return (EndnoteRestartIndex)DOP.RestartIndexForEndnote;
		}
		set
		{
			DOP.RestartIndexForEndnote = (byte)value;
			if (IsOpening)
			{
				return;
			}
			foreach (WSection section in Sections)
			{
				section.PageSetup.RestartIndexForEndnote = value;
			}
		}
	}

	public EndnotePosition EndnotePosition
	{
		get
		{
			return (EndnotePosition)DOP.EndnotePosition;
		}
		set
		{
			DOP.EndnotePosition = (byte)value;
		}
	}

	public FootnoteRestartIndex RestartIndexForFootnotes
	{
		get
		{
			return (FootnoteRestartIndex)DOP.RestartIndexForFootnotes;
		}
		set
		{
			DOP.RestartIndexForFootnotes = (byte)value;
			if (IsOpening)
			{
				return;
			}
			foreach (WSection section in Sections)
			{
				section.PageSetup.RestartIndexForFootnotes = value;
			}
		}
	}

	public FootnotePosition FootnotePosition
	{
		get
		{
			return (FootnotePosition)DOP.FootnotePosition;
		}
		set
		{
			DOP.FootnotePosition = (byte)value;
			if (IsOpening)
			{
				return;
			}
			foreach (WSection section in Sections)
			{
				section.PageSetup.FootnotePosition = value;
			}
		}
	}

	public Watermark Watermark
	{
		get
		{
			if (m_watermark == null)
			{
				m_watermark = new Watermark(this, WatermarkType.NoWatermark);
			}
			return m_watermark;
		}
		set
		{
			ResetWatermark();
			m_watermark = value;
			WordDocument wordDocument = ((m_watermark != null) ? m_watermark.Document : null);
			if (m_watermark != null)
			{
				if (!base.Document.IsOpening)
				{
					UpdateHeaderWatermark(m_watermark);
				}
				if (m_watermark is PictureWatermark)
				{
					(m_watermark as PictureWatermark).UpdateImage();
				}
				else
				{
					(m_watermark as TextWatermark).SetDefaultSize();
				}
			}
			if (m_watermark is PictureWatermark && wordDocument != null && m_doc != wordDocument && wordDocument.ActualFormatType == FormatType.Doc && m_doc.Escher != null && m_doc.Escher.m_msofbtDggContainer != null && m_doc.Escher.m_msofbtDggContainer.BstoreContainer != null)
			{
				int count = m_doc.Escher.m_msofbtDggContainer.BstoreContainer.Children.Count;
				wordDocument.CloneShapeEscher(m_doc, (m_watermark as PictureWatermark).WordPicture);
				if (count != m_doc.Escher.m_msofbtDggContainer.BstoreContainer.Children.Count)
				{
					(m_watermark as PictureWatermark).OriginalPib = m_doc.Escher.m_msofbtDggContainer.BstoreContainer.Children.Count;
				}
			}
		}
	}

	public Background Background
	{
		get
		{
			if (m_background == null)
			{
				m_background = new Background(this, BackgroundType.NoBackground);
			}
			return m_background;
		}
	}

	public MailMerge MailMerge
	{
		get
		{
			if (m_mailMerge == null)
			{
				m_mailMerge = new MailMerge(this);
			}
			return m_mailMerge;
		}
	}

	public ProtectionType ProtectionType
	{
		get
		{
			return DOP.ProtectionType;
		}
		set
		{
			DOP.ProtectionType = value;
		}
	}

	public ViewSetup ViewSetup
	{
		get
		{
			if (m_viewSetup == null)
			{
				m_viewSetup = new ViewSetup(this);
			}
			return m_viewSetup;
		}
	}

	public bool ThrowExceptionsForUnsupportedElements
	{
		get
		{
			return (m_bFlags1 & 0x40) >> 6 != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xBFu) | ((value ? 1u : 0u) << 6));
		}
	}

	public int InitialFootnoteNumber
	{
		get
		{
			return DOP.InitialFootnoteNumber;
		}
		set
		{
			DOP.InitialFootnoteNumber = value;
			if (IsOpening)
			{
				return;
			}
			foreach (WSection section in Sections)
			{
				section.PageSetup.InitialFootnoteNumber = value;
			}
		}
	}

	public int InitialEndnoteNumber
	{
		get
		{
			return DOP.InitialEndnoteNumber;
		}
		set
		{
			DOP.InitialEndnoteNumber = value;
			if (IsOpening)
			{
				return;
			}
			foreach (WSection section in Sections)
			{
				section.PageSetup.InitialEndnoteNumber = value;
			}
		}
	}

	public EntityCollection ChildEntities => m_sections;

	public XHTMLValidationType XHTMLValidateOption
	{
		get
		{
			return m_htmlValidationOption;
		}
		set
		{
			m_htmlValidationOption = value;
		}
	}

	[Obsolete("This property has been deprecated. Use the Picture property of Background class to set the background image of the document")]
	public byte[] BackgroundImage
	{
		get
		{
			return GetBackGndImage();
		}
		set
		{
			SetBackgroundImageValue(value);
		}
	}

	public DocVariables Variables
	{
		get
		{
			if (m_variables == null)
			{
				m_variables = new DocVariables();
			}
			return m_variables;
		}
	}

	public DocProperties Properties
	{
		get
		{
			if (m_props == null)
			{
				m_props = new DocProperties(this);
			}
			return m_props;
		}
	}

	public bool HasChanges => HasTrackedChanges();

	public bool TrackChanges
	{
		get
		{
			return DOP.RevMarking;
		}
		set
		{
			DOP.RevMarking = value;
		}
	}

	public bool ReplaceFirst
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	public HTMLImportSettings HTMLImportSettings
	{
		get
		{
			if (m_htmlImportSettings == null)
			{
				m_htmlImportSettings = new HTMLImportSettings();
			}
			return m_htmlImportSettings;
		}
		set
		{
			m_htmlImportSettings = value;
		}
	}

	public MdImportSettings MdImportSettings
	{
		get
		{
			if (m_mdImportSettings == null)
			{
				m_mdImportSettings = new MdImportSettings();
			}
			return m_mdImportSettings;
		}
		internal set
		{
			m_mdImportSettings = value;
		}
	}

	public SaveOptions SaveOptions
	{
		get
		{
			if (m_saveOptions == null)
			{
				m_saveOptions = new SaveOptions();
			}
			return m_saveOptions;
		}
	}

	public RevisionOptions RevisionOptions
	{
		get
		{
			if (m_revisionOptions == null)
			{
				m_revisionOptions = new RevisionOptions();
			}
			return m_revisionOptions;
		}
	}

	[Obsolete("This property has been deprecated. Use the UpdateDocumentFields method of WordDocument class to update the fields in the document.")]
	public bool UpdateFields
	{
		get
		{
			return (m_bFlags & 8) >> 3 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	public FormatType ActualFormatType
	{
		get
		{
			return m_actualFormatType;
		}
		internal set
		{
			m_actualFormatType = value;
		}
	}

	public Dictionary<string, string> FontSubstitutionTable
	{
		get
		{
			if (m_fontSubstitutionTable == null)
			{
				m_fontSubstitutionTable = new Dictionary<string, string>();
			}
			return m_fontSubstitutionTable;
		}
		set
		{
			m_fontSubstitutionTable = value;
		}
	}

	public bool HasMacros
	{
		get
		{
			if (VbaProject == null && VbaData.Count <= 0 && DocEvents.Count <= 0)
			{
				return MacrosData != null;
			}
			return true;
		}
	}

	public static bool EnablePartialTrustCode
	{
		get
		{
			return m_EnablePartialTrustCode;
		}
		set
		{
			m_EnablePartialTrustCode = value;
		}
	}

	internal static bool DisableDateTimeUpdating
	{
		get
		{
			return m_disableDateTimeUpdating;
		}
		set
		{
			m_disableDateTimeUpdating = value;
		}
	}

	internal bool RestrictFormatting
	{
		get
		{
			return (m_bFlags3 & 0x20) >> 5 != 0;
		}
		set
		{
			m_bFlags3 = (byte)((m_bFlags3 & 0xDFu) | ((value ? 1u : 0u) << 5));
		}
	}

	internal bool Enforcement
	{
		get
		{
			return (m_bFlags3 & 0x10) >> 4 != 0;
		}
		set
		{
			m_bFlags3 = (byte)((m_bFlags3 & 0xEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	internal MultiplePage MultiplePage
	{
		get
		{
			if (DOP.MirrorMargins)
			{
				return MultiplePage.MirrorMargins;
			}
			if (DOP.Dop97.DopTypography.Print2on1)
			{
				return MultiplePage.TwoPagesPerSheet;
			}
			if (DOP.Dop2002.ReverseFolio)
			{
				return MultiplePage.ReverseBookFold;
			}
			if (DOP.Dop2002.FolioPrint)
			{
				return MultiplePage.BookFold;
			}
			return MultiplePage.Normal;
		}
		set
		{
			switch (value)
			{
			case MultiplePage.MirrorMargins:
				DOP.MirrorMargins = true;
				break;
			case MultiplePage.BookFold:
				DOP.Dop2002.FolioPrint = true;
				break;
			case MultiplePage.ReverseBookFold:
				DOP.Dop2002.ReverseFolio = true;
				DOP.Dop2002.FolioPrint = true;
				break;
			case MultiplePage.TwoPagesPerSheet:
				DOP.Dop97.DopTypography.Print2on1 = true;
				break;
			}
		}
	}

	internal int SheetsPerBooklet
	{
		get
		{
			return DOP.Dop2002.IFolioPages;
		}
		set
		{
			if (value != 0)
			{
				DOP.Dop2002.IFolioPages = (ushort)value;
			}
		}
	}

	internal FontFamilyNameStringTable FFNStringTable
	{
		get
		{
			return m_ffnStringTable;
		}
		set
		{
			m_ffnStringTable = value;
		}
	}

	internal bool IsComparing
	{
		get
		{
			return (m_bFlags3 & 0x40) >> 6 != 0;
		}
		set
		{
			m_bFlags3 = (byte)((m_bFlags3 & 0xBFu) | ((value ? 1u : 0u) << 6));
		}
	}

	internal Comparison Comparison
	{
		get
		{
			if (m_comparison == null)
			{
				m_comparison = new Comparison(this);
			}
			return m_comparison;
		}
	}

	internal bool UpdateRevisionOnComparing
	{
		get
		{
			return (m_bFlags3 & 0x80) >> 7 != 0;
		}
		set
		{
			m_bFlags3 = (byte)((m_bFlags3 & 0x7Fu) | ((value ? 1u : 0u) << 7));
		}
	}

	internal DocxLaTeXConverter DocxLaTeXConveter
	{
		get
		{
			if (m_docxLaTeXConveter == null)
			{
				m_docxLaTeXConveter = new DocxLaTeXConverter(m_doc);
			}
			return m_docxLaTeXConveter;
		}
	}

	internal bool HasStyleSheets
	{
		get
		{
			return (m_bFlags3 & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags3 = (byte)((m_bFlags3 & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal Dictionary<string, CustomXMLPart> CustomXmlParts
	{
		get
		{
			if (m_customXMLParts == null)
			{
				m_customXMLParts = new Dictionary<string, CustomXMLPart>();
			}
			return m_customXMLParts;
		}
	}

	internal PartContainer CustomXmlPartContainer
	{
		get
		{
			if (m_customXMLPartContainer == null)
			{
				m_customXMLPartContainer = new PartContainer();
			}
			return m_customXMLPartContainer;
		}
	}

	internal ImageCollection Images
	{
		get
		{
			if (m_imageCollection == null)
			{
				m_imageCollection = new ImageCollection(this);
			}
			return m_imageCollection;
		}
	}

	internal Dictionary<string, Storage> OleObjectCollection
	{
		get
		{
			if (m_oleObjectCollection == null)
			{
				m_oleObjectCollection = new Dictionary<string, Storage>();
			}
			return m_oleObjectCollection;
		}
	}

	internal Stack<WField> ClonedFields
	{
		get
		{
			if (m_clonedFields == null)
			{
				m_clonedFields = new Stack<WField>();
			}
			return m_clonedFields;
		}
	}

	internal ListOverrideStyleCollection ListOverrides
	{
		get
		{
			if (m_listOverrides == null)
			{
				m_listOverrides = new ListOverrideStyleCollection(this);
			}
			return m_listOverrides;
		}
	}

	internal GrammarSpelling GrammarSpellingData
	{
		get
		{
			return m_grammarSpellingData;
		}
		set
		{
			m_grammarSpellingData = value;
		}
	}

	internal DOPDescriptor DOP
	{
		get
		{
			if (m_dop == null)
			{
				m_dop = new DOPDescriptor();
			}
			return m_dop;
		}
		set
		{
			m_dop = value;
		}
	}

	internal EscherClass Escher
	{
		get
		{
			return m_escher;
		}
		set
		{
			m_escher = value;
		}
	}

	internal FormatType SaveFormatType
	{
		get
		{
			return m_saveFormatType;
		}
		set
		{
			m_saveFormatType = value;
		}
	}

	internal bool IsMacroEnabled
	{
		get
		{
			if (SaveFormatType != FormatType.Word2007Docm && SaveFormatType != FormatType.Word2010Docm && SaveFormatType != FormatType.Word2007Dotm)
			{
				return SaveFormatType == FormatType.Word2010Dotm;
			}
			return true;
		}
	}

	internal Stream VbaProject
	{
		get
		{
			return m_vbaProject;
		}
		set
		{
			m_vbaProject = value;
		}
	}

	internal Stream VbaProjectSignature
	{
		get
		{
			return m_vbaProjectSignature;
		}
		set
		{
			m_vbaProjectSignature = value;
		}
	}

	internal Stream VbaProjectSignatureAgile
	{
		get
		{
			return m_vbaProjectSignatureAgile;
		}
		set
		{
			m_vbaProjectSignatureAgile = value;
		}
	}

	internal PartContainer CustomUIPartContainer
	{
		get
		{
			return m_CustomUIPartContainer;
		}
		set
		{
			m_CustomUIPartContainer = value;
		}
	}

	internal PartContainer UserCustomizationPartContainer
	{
		get
		{
			return m_UserCustomizationPartContainer;
		}
		set
		{
			m_UserCustomizationPartContainer = value;
		}
	}

	internal PartContainer CustomXMLContainer
	{
		get
		{
			return m_CustomXMLContainer;
		}
		set
		{
			m_CustomXMLContainer = value;
		}
	}

	internal List<MacroData> VbaData
	{
		get
		{
			if (m_vbaData == null)
			{
				m_vbaData = new List<MacroData>();
			}
			return m_vbaData;
		}
		set
		{
			m_vbaData = value;
		}
	}

	internal List<string> DocEvents
	{
		get
		{
			if (m_docEvents == null)
			{
				m_docEvents = new List<string>();
			}
			return m_docEvents;
		}
		set
		{
			m_docEvents = value;
		}
	}

	internal byte[] MacrosData
	{
		get
		{
			return m_macrosData;
		}
		set
		{
			m_macrosData = value;
		}
	}

	internal byte[] MacroCommands
	{
		get
		{
			return m_macroCommands;
		}
		set
		{
			m_macroCommands = value;
		}
	}

	internal string StandardAsciiFont
	{
		get
		{
			return m_standardAsciiFont;
		}
		set
		{
			m_standardAsciiFont = value;
		}
	}

	internal string StandardFarEastFont
	{
		get
		{
			return m_standardFarEastFont;
		}
		set
		{
			m_standardFarEastFont = value;
		}
	}

	internal string StandardNonFarEastFont
	{
		get
		{
			return m_standardNonFarEastFont;
		}
		set
		{
			m_standardNonFarEastFont = value;
		}
	}

	internal string StandardBidiFont
	{
		get
		{
			return m_standardBidiFont;
		}
		set
		{
			m_standardBidiFont = value;
		}
	}

	internal string Password
	{
		get
		{
			return m_password;
		}
		set
		{
			m_password = value;
		}
	}

	internal MemoryStream LatentStyles2010
	{
		get
		{
			return m_latentStyles2010;
		}
		set
		{
			m_latentStyles2010 = value;
		}
	}

	internal Stream LatentStyles
	{
		get
		{
			return m_latentStyles;
		}
		set
		{
			m_latentStyles = value;
		}
	}

	internal Package DocxPackage
	{
		get
		{
			return m_docxPackage;
		}
		set
		{
			m_docxPackage = value;
		}
	}

	public bool ImportStyles
	{
		get
		{
			return (m_bFlags1 & 0x80) >> 7 != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0x7Fu) | ((value ? 1u : 0u) << 7));
		}
	}

	public bool ImportStylesOnTypeMismatch
	{
		get
		{
			return (m_bFlags3 & 1) != 0;
		}
		set
		{
			m_bFlags3 = (byte)((m_bFlags3 & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal WCharacterFormat DefCharFormat
	{
		get
		{
			return m_defCharFormat;
		}
		set
		{
			m_defCharFormat = value;
		}
	}

	internal WParagraphFormat DefParaFormat
	{
		get
		{
			if (m_defParaFormat == null && !IsOpening)
			{
				InitDefaultParagraphFormat();
			}
			return m_defParaFormat;
		}
		set
		{
			m_defParaFormat = value;
		}
	}

	internal SttbfAssoc AssociatedStrings
	{
		get
		{
			if (m_assocStrings == null)
			{
				m_assocStrings = new SttbfAssoc();
			}
			return m_assocStrings;
		}
	}

	internal bool IsEncrypted
	{
		get
		{
			return (m_bFlags & 4) >> 2 != 0;
		}
		private set
		{
			m_bFlags = (byte)((m_bFlags & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal bool HasPicture
	{
		get
		{
			return (m_bFlags & 0x40) >> 6 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xBFu) | ((value ? 1u : 0u) << 6));
		}
	}

	internal bool WriteWarning
	{
		get
		{
			if (!string.IsNullOrEmpty(syncfusionLicense))
			{
				return !m_isWarnInserted;
			}
			return false;
		}
	}

	internal bool WriteProtected
	{
		get
		{
			return (m_bFlags & 0x10) >> 4 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	internal bool UpdateAlternateChunk
	{
		get
		{
			return (m_bFlags2 & 0x10) >> 4 != 0;
		}
		set
		{
			m_bFlags2 = (byte)((m_bFlags2 & 0xEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	internal bool IsDeletingBookmarkContent
	{
		get
		{
			return (m_bFlags2 & 0x20) >> 5 != 0;
		}
		set
		{
			m_bFlags2 = (byte)((m_bFlags2 & 0xDFu) | ((value ? 1u : 0u) << 5));
		}
	}

	private Dictionary<string, Dictionary<int, int>> ListNames
	{
		get
		{
			if (m_listNames == null)
			{
				m_listNames = new Dictionary<string, Dictionary<int, int>>();
			}
			return m_listNames;
		}
	}

	private Dictionary<string, Dictionary<int, int>> Lists
	{
		get
		{
			if (m_lists == null)
			{
				m_lists = new Dictionary<string, Dictionary<int, int>>();
			}
			return m_lists;
		}
	}

	private Dictionary<string, int> PreviousListLevel
	{
		get
		{
			if (m_previousListLevel == null)
			{
				m_previousListLevel = new Dictionary<string, int>();
			}
			return m_previousListLevel;
		}
	}

	private List<string> PreviousListLevelOverrideStyle
	{
		get
		{
			if (m_previousListLevelOverrideStyle == null)
			{
				m_previousListLevelOverrideStyle = new List<string>();
			}
			return m_previousListLevelOverrideStyle;
		}
	}

	internal bool UseHangingIndentAsListTab
	{
		get
		{
			if (ActualFormatType != 0)
			{
				if (ActualFormatType == FormatType.Docx || ActualFormatType == FormatType.Word2007 || ActualFormatType == FormatType.Word2010 || ActualFormatType == FormatType.Word2013)
				{
					return !Settings.CompatibilityOptions[CompatibilityOption.DontUseIndentAsNumberingTabStop];
				}
				return true;
			}
			return false;
		}
	}

	internal bool UseHangingIndentAsTabPosition
	{
		get
		{
			if (Settings.CompatibilityMode != CompatibilityMode.Word2013)
			{
				return !Settings.CompatibilityOptions[CompatibilityOption.NoTabForInd];
			}
			return true;
		}
	}

	internal Themes Themes
	{
		get
		{
			if (m_themes == null)
			{
				m_themes = new Themes(this);
			}
			return m_themes;
		}
	}

	public Settings Settings
	{
		get
		{
			if (m_settings == null)
			{
				m_settings = new Settings(this);
			}
			return m_settings;
		}
	}

	internal int AlternateChunkCount => ++m_altChunkCount;

	internal bool IsOpening
	{
		get
		{
			return (m_bFlags1 & 1) != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal bool IsMailMerge
	{
		get
		{
			return (m_bFlags1 & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal bool IsCloning
	{
		get
		{
			return (m_bFlags1 & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal bool DocHasThemes
	{
		get
		{
			return (m_bFlags1 & 8) >> 3 != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal bool CreateBaseStyle
	{
		get
		{
			return (m_bFlags1 & 0x10) >> 4 != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	internal bool IsNormalStyleDefined
	{
		get
		{
			return (m_bFlags1 & 0x20) >> 5 != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xDFu) | ((value ? 1u : 0u) << 5));
		}
	}

	internal bool IsDefaultParagraphFontStyleDefined
	{
		get
		{
			return (m_bFlags2 & 8) >> 3 != 0;
		}
		set
		{
			m_bFlags2 = (byte)((m_bFlags2 & 0xF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal bool IsHTMLImport
	{
		get
		{
			return (m_bFlags2 & 1) != 0;
		}
		set
		{
			m_bFlags2 = (byte)((m_bFlags2 & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal bool IsSkipFieldDetach
	{
		get
		{
			return (m_bFlags2 & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags2 = (byte)((m_bFlags2 & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal bool IsFieldRangeAdding
	{
		get
		{
			return (m_bFlags2 & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags2 = (byte)((m_bFlags2 & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal bool IsReadOnly
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal bool BordersSurroundHeader
	{
		get
		{
			return m_doc.DOP.Dop97.IncludeHeader;
		}
		set
		{
			m_doc.DOP.Dop97.IncludeHeader = value;
		}
	}

	internal bool BordersSurroundFooter
	{
		get
		{
			return m_doc.DOP.Dop97.IncludeFooter;
		}
		set
		{
			m_doc.DOP.Dop97.IncludeFooter = value;
		}
	}

	internal bool DifferentOddAndEvenPages
	{
		get
		{
			return (m_bFlags & 0x80) >> 7 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0x7Fu) | ((value ? 1u : 0u) << 7));
		}
	}

	internal ushort WordVersion
	{
		get
		{
			return m_wordVersion;
		}
		set
		{
			m_wordVersion = value;
			m_fibVersion = value;
		}
	}

	internal List<DocGen.Drawing.Font> UsedFonts
	{
		get
		{
			if (m_usedFonts == null)
			{
				m_usedFonts = new List<DocGen.Drawing.Font>();
			}
			return m_usedFonts;
		}
		set
		{
			m_usedFonts = value;
		}
	}

	internal List<string> UsedFontNames
	{
		get
		{
			if (m_usedFontNames == null)
			{
				m_usedFontNames = new List<string>();
			}
			return m_usedFontNames;
		}
		set
		{
			m_usedFontNames = value;
		}
	}

	internal bool HasTOC => m_tableOfContent != null;

	internal Dictionary<WField, TableOfContent> TOC
	{
		get
		{
			if (HasTOC)
			{
				return m_tableOfContent;
			}
			return m_tableOfContent = new Dictionary<WField, TableOfContent>();
		}
		set
		{
			m_tableOfContent = value;
		}
	}

	internal string HtmlBaseUrl
	{
		get
		{
			return m_htmlBaseUrl;
		}
		set
		{
			m_htmlBaseUrl = value;
		}
	}

	internal FieldCollection Fields
	{
		get
		{
			if (m_fields == null)
			{
				m_fields = new FieldCollection(this);
			}
			return m_fields;
		}
	}

	internal List<Shape> AutoShapeCollection
	{
		get
		{
			if (m_AutoShapeCollection == null)
			{
				m_AutoShapeCollection = new List<Shape>();
			}
			return m_AutoShapeCollection;
		}
		set
		{
			m_AutoShapeCollection = value;
		}
	}

	internal List<Entity> FloatingItems
	{
		get
		{
			if (m_FloatingItems == null)
			{
				m_FloatingItems = new List<Entity>();
			}
			return m_FloatingItems;
		}
		set
		{
			m_FloatingItems = value;
		}
	}

	internal List<Stream> DocxProps
	{
		get
		{
			if (m_docxProps == null)
			{
				m_docxProps = new List<Stream>();
			}
			return m_docxProps;
		}
	}

	internal Dictionary<string, string> ListStyleNames
	{
		get
		{
			if (m_listStyleNames == null)
			{
				m_listStyleNames = new Dictionary<string, string>();
			}
			return m_listStyleNames;
		}
	}

	internal bool HasDocxProps => m_docxProps != null;

	internal bool IsClosing
	{
		get
		{
			return (m_bFlags & 0x20) >> 5 != 0;
		}
		private set
		{
			m_bFlags = (byte)((m_bFlags & 0xDFu) | ((value ? 1u : 0u) << 5));
		}
	}

	internal Dictionary<string, string> StyleNameIds
	{
		get
		{
			if (m_styleNameIds == null)
			{
				m_styleNameIds = new Dictionary<string, string>();
			}
			return m_styleNameIds;
		}
	}

	internal bool HasCoverPage
	{
		get
		{
			return (m_bFlags2 & 0x40) >> 6 != 0;
		}
		set
		{
			m_bFlags2 = (byte)((m_bFlags2 & 0xBFu) | ((value ? 1u : 0u) << 6));
		}
	}

	internal int TrackChangesBalloonCount
	{
		get
		{
			return m_balloonCount;
		}
		set
		{
			m_balloonCount = value;
		}
	}

	internal Dictionary<string, int> SequenceFieldResults
	{
		get
		{
			return m_sequenceFieldResults;
		}
		set
		{
			m_sequenceFieldResults = value;
		}
	}

	internal ComparisonOptions ComparisonOptions
	{
		get
		{
			return m_comparisonOptions;
		}
		set
		{
			m_comparisonOptions = value;
		}
	}

	protected override IEntityCollectionBase WidgetCollection => Sections;

	internal byte[] SttbfRMark
	{
		get
		{
			return m_sttbfRMark;
		}
		set
		{
			m_sttbfRMark = value;
		}
	}

	internal void SetDefaultSectionFormatting(WSection destination)
	{
		destination.SectionFormat.PageSetup.PageSize = new SizeF(612f, 792f);
		destination.SectionFormat.PageSetup.HeaderDistance = 36f;
		destination.SectionFormat.PageSetup.FooterDistance = 36f;
		destination.SectionFormat.PageSetup.LinePitch = 18f;
		destination.SectionFormat.PageSetup.EqualColumnWidth = true;
		destination.SectionFormat.PageSetup.PageNumberStyle = PageNumberStyle.Arabic;
		destination.SectionFormat.PageSetup.RestartPageNumbering = false;
		destination.SectionFormat.PageSetup.Margins.Left = 72f;
		destination.SectionFormat.PageSetup.Margins.Right = 72f;
		destination.SectionFormat.PageSetup.Margins.Top = 72f;
		destination.SectionFormat.PageSetup.Margins.Bottom = 72f;
		destination.SectionFormat.PageSetup.VerticalAlignment = PageAlignment.Top;
		destination.SectionFormat.PageSetup.PageNumbers.ChapterPageSeparator = ChapterPageSeparatorType.Hyphen;
		destination.SectionFormat.PageSetup.PageNumbers.HeadingLevelForChapter = HeadingLevel.None;
		destination.SectionFormat.PageSetup.Margins = destination.SectionFormat.PageSetup.Margins;
		destination.SectionFormat.PageSetup.PageNumbers = destination.SectionFormat.PageSetup.PageNumbers;
		destination.SectionFormat.PageSetup.Borders = new Borders();
		destination.SectionFormat.PageSetup = destination.SectionFormat.PageSetup;
	}

	public WordDocument(Stream stream, FormatType type, XHTMLValidationType validationType)
		: this()
	{
		Open(stream, type, validationType);
	}

	public WordDocument()
		: base(null, null)
	{
		m_doc = this;
		if (!IsOpening)
		{
			m_actualFormatType = FormatType.Docx;
		}
		Init();
	}

	internal WordDocument(Stream stream)
		: this()
	{
		FormatType formatType = FormatType.Doc;
		if (stream == null)
		{
			throw new ArgumentNullException("Stream");
		}
		OpenInternal(stream, formatType, null);
	}

	public WordDocument(Stream stream, FormatType type)
		: this()
	{
		Open(stream, type);
	}

	public WordDocument(Stream stream, string password)
		: this()
	{
		FormatType formatType = FormatType.Doc;
		Open(stream, formatType, password);
	}

	public WordDocument(Stream stream, FormatType type, string password)
		: this()
	{
		Open(stream, type, password);
	}

	protected WordDocument(WordDocument doc)
		: this()
	{
		m_standardAsciiFont = doc.StandardAsciiFont;
		m_standardFarEastFont = doc.StandardFarEastFont;
		m_standardNonFarEastFont = doc.StandardNonFarEastFont;
		m_standardBidiFont = doc.m_standardBidiFont;
		m_viewSetup = doc.ViewSetup.Clone(this);
		DefaultTabWidth = doc.DefaultTabWidth;
		ActualFormatType = doc.ActualFormatType;
		if (doc.BuiltinDocumentProperties != null)
		{
			m_builtinProp = doc.BuiltinDocumentProperties.Clone();
		}
		if (doc.CustomDocumentProperties != null)
		{
			m_customProp = doc.CustomDocumentProperties.Clone();
		}
		if (doc.Watermark != null && doc.Watermark.Type != 0)
		{
			Watermark = (Watermark)doc.Watermark.Clone();
		}
		if (doc.Background.Type != 0)
		{
			m_background = doc.Background.Clone();
			m_background.SetOwner(this);
			m_background.UpdateImageRecord(this);
		}
		if (doc.DOP != null)
		{
			m_dop = doc.DOP.Clone();
		}
		if (doc.DefCharFormat != null)
		{
			m_defCharFormat = new WCharacterFormat(this);
			m_defCharFormat.ImportContainer(doc.DefCharFormat);
		}
		if (doc.DefParaFormat != null)
		{
			m_defParaFormat = new WParagraphFormat(this);
			m_defParaFormat.ImportContainer(doc.DefParaFormat);
		}
		foreach (KeyValuePair<string, string> item in doc.FontSubstitutionTable)
		{
			if (!FontSubstitutionTable.ContainsKey(item.Key))
			{
				FontSubstitutionTable.Add(item.Key, item.Value);
			}
			else
			{
				FontSubstitutionTable[item.Key] = item.Value;
			}
		}
		Footnotes = doc.Footnotes.Clone();
		Endnotes = doc.Endnotes.Clone();
		ImportContent(doc);
	}

	private FormatType GetFormatType(string fileName, bool isStorageFile)
	{
		switch ((isStorageFile ? fileName : Path.GetExtension(fileName)).ToLower())
		{
		case ".doc":
		case ".dot":
			return FormatType.Doc;
		case ".docx":
			if (m_actualFormatType == FormatType.StrictDocx)
			{
				return FormatType.StrictDocx;
			}
			return FormatType.Docx;
		case ".odt":
			return FormatType.Odt;
		case ".dotx":
			return FormatType.Dotx;
		case ".docm":
			return FormatType.Docm;
		case ".dotm":
			return FormatType.Dotm;
		case ".txt":
			return FormatType.Txt;
		case ".htm":
		case ".html":
			return FormatType.Html;
		case ".rtf":
			return FormatType.Rtf;
		case ".xml":
			return FormatType.WordML;
		case ".md":
			return FormatType.Markdown;
		default:
			throw new Exception("Cannot recognize current file type");
		}
	}

	public IWParagraph CreateParagraph()
	{
		return new WParagraph(this);
	}

	public void EnsureMinimal()
	{
		if (Sections.Count == 0)
		{
			AddSection().Body.AddParagraph();
			LastSection.PageSetup.HeaderDistance = 36f;
			LastSection.PageSetup.FooterDistance = 36f;
		}
	}

	public IWSection AddSection()
	{
		WSection wSection = new WSection(base.Document);
		Sections.Add(wSection);
		return wSection;
	}

	public IWParagraphStyle AddParagraphStyle(string styleName)
	{
		return AddStyle(StyleType.ParagraphStyle, styleName) as IWParagraphStyle;
	}

	public IWCharacterStyle AddCharacterStyle(string styleName)
	{
		return AddStyle(StyleType.CharacterStyle, styleName) as IWCharacterStyle;
	}

	public ListStyle AddListStyle(ListType listType, string styleName)
	{
		ListStyle listStyle = new ListStyle(this, listType);
		ListStyles.Add(listStyle);
		listStyle.Name = styleName;
		return listStyle;
	}

	public string GetText()
	{
		return new TextConverter().GetText(this);
	}

	public new WordDocument Clone()
	{
		return (WordDocument)CloneImpl();
	}

	public void ImportSection(IWSection section)
	{
		IWSection section2 = section.Clone();
		Sections.Add(section2);
	}

	public void ImportContent(IWordDocument doc)
	{
		ImportContent(doc, importStyles: true);
	}

	public void ImportContent(IWordDocument doc, ImportOptions importOptions)
	{
		(doc as WordDocument).IsCloning = true;
		bool importStyles = ImportStyles;
		ImportOptions importOption = m_importOption;
		bool maintainImportedListCache = Settings.MaintainImportedListCache;
		ImportOptions = importOptions;
		ImportStyles = true;
		Settings.MaintainImportedListCache = false;
		if ((m_importOption & ImportOptions.UseDestinationStyles) != 0)
		{
			ImportStyles = false;
		}
		if ((m_importOption & ImportOptions.KeepTextOnly) != 0)
		{
			ImportDocumentText(doc);
		}
		else
		{
			doc.Sections.CloneTo(Sections);
			CopyBinaryData((doc as WordDocument).MacrosData, ref m_macrosData);
			CopyBinaryData((doc as WordDocument).MacroCommands, ref m_macroCommands);
			m_docxProps = (doc as WordDocument).m_docxProps;
		}
		(doc as WordDocument).IsCloning = false;
		m_importOption = importOption;
		ImportStyles = importStyles;
		Settings.MaintainImportedListCache = maintainImportedListCache;
		Settings.DuplicateListStyleNames = string.Empty;
	}

	private void ImportDocumentText(IWordDocument doc)
	{
		string text = doc.Sections.GetText();
		m_prevClonedEntity = null;
		IWSection iWSection = AddSection();
		string[] array = text.Split('\r');
		for (int i = 0; i < array.Length; i++)
		{
			iWSection.AddParagraph().AppendText(array[i]);
		}
	}

	public void ImportContent(IWordDocument doc, bool importStyles)
	{
		(doc as WordDocument).IsCloning = true;
		bool importStyles2 = ImportStyles;
		ImportOptions importOption = m_importOption;
		bool maintainImportedListCache = Settings.MaintainImportedListCache;
		ImportStyles = importStyles;
		m_importOption = ImportOptions.UseDestinationStyles;
		Settings.MaintainImportedListCache = false;
		doc.Sections.CloneTo(Sections);
		foreach (KeyValuePair<string, string> styleNameId in (doc as WordDocument).StyleNameIds)
		{
			if (!StyleNameIds.ContainsKey(styleNameId.Key))
			{
				StyleNameIds.Add(styleNameId.Key, styleNameId.Value);
			}
		}
		int i = 0;
		for (int count = doc.Styles.Count; i < count; i++)
		{
			Style style = doc.Styles[i] as Style;
			string name = style.Name;
			List<string> styelNames = new List<string>();
			bool isDiffTypeStyleFound = false;
			if (!((Styles as StyleCollection).FindByName(name, style.StyleType, ref styelNames, ref isDiffTypeStyleFound) is Style))
			{
				if (!isDiffTypeStyleFound)
				{
					Styles.Add(style.Clone());
				}
				else if (ImportStylesOnTypeMismatch)
				{
					style.SetStyleName(style.GetUniqueStyleName(name, styelNames));
					Styles.Add(style.Clone());
				}
			}
			styelNames.Clear();
		}
		ListStyle listStyle = null;
		int j = 0;
		for (int count2 = doc.ListStyles.Count; j < count2; j++)
		{
			listStyle = doc.ListStyles[j];
			if (listStyle != null && !ListStyles.HasEquivalentStyle(listStyle))
			{
				ListStyles.Add((ListStyle)listStyle.Clone());
			}
		}
		foreach (KeyValuePair<string, string> item in (doc as WordDocument).FontSubstitutionTable)
		{
			if (!FontSubstitutionTable.ContainsKey(item.Key))
			{
				FontSubstitutionTable.Add(item.Key, item.Value);
			}
			else
			{
				FontSubstitutionTable[item.Key] = item.Value;
			}
		}
		ListOverrideStyle listOverrideStyle = null;
		int k = 0;
		for (int count3 = (doc as WordDocument).ListOverrides.Count; k < count3; k++)
		{
			listOverrideStyle = (doc as WordDocument).ListOverrides[k];
			if (listOverrideStyle != null && !ListOverrides.HasEquivalentStyle(listOverrideStyle))
			{
				ListOverrides.Add((ListOverrideStyle)listOverrideStyle.Clone());
			}
		}
		CopyBinaryData((doc as WordDocument).MacrosData, ref m_macrosData);
		CopyBinaryData((doc as WordDocument).MacroCommands, ref m_macroCommands);
		if ((doc as WordDocument).DefCharFormat != null)
		{
			if (m_defCharFormat == null)
			{
				m_defCharFormat = new WCharacterFormat(m_doc);
			}
			m_defCharFormat.ImportContainer((doc as WordDocument).DefCharFormat);
		}
		if (m_defParaFormat == null && ActualFormatType == FormatType.Docx)
		{
			m_defParaFormat = new WParagraphFormat(m_doc);
			m_defParaFormat.ImportContainer((doc as WordDocument).DefParaFormat);
		}
		if (!m_doc.DocHasThemes && (doc as WordDocument).DocHasThemes)
		{
			m_doc.Themes.FontScheme.FontSchemeName = (doc as WordDocument).Themes.FontScheme.FontSchemeName;
			CopyFontScheme((doc as WordDocument).Themes.FontScheme.MajorFontScheme, m_doc.Themes.FontScheme.MajorFontScheme);
			CopyFontScheme((doc as WordDocument).Themes.FontScheme.MinorFontScheme, m_doc.Themes.FontScheme.MinorFontScheme);
		}
		m_docxProps = (doc as WordDocument).m_docxProps;
		(doc as WordDocument).IsCloning = false;
		ImportStyles = importStyles2;
		m_importOption = importOption;
		Settings.MaintainImportedListCache = maintainImportedListCache;
	}

	private void CopyFontScheme(MajorMinorFontScheme src, MajorMinorFontScheme dest)
	{
		foreach (FontSchemeStruct fontScheme in src.FontSchemeList)
		{
			FontSchemeStruct item = default(FontSchemeStruct);
			item.Name = fontScheme.Name;
			item.Typeface = fontScheme.Typeface;
			item.Charset = fontScheme.Charset;
			item.Panose = fontScheme.Panose;
			item.PitchFamily = fontScheme.PitchFamily;
			dest.FontSchemeList.Add(item);
		}
	}

	public IStyle AddStyle(BuiltinStyle builtinStyle)
	{
		CheckNormalStyle();
		string name = Style.BuiltInToName(builtinStyle);
		IStyle style = base.Document.Styles.FindByName(name);
		if (style == null)
		{
			if (IsBuiltInCharacterStyle(builtinStyle))
			{
				style = (WCharacterStyle)Style.CreateBuiltinCharacterStyle(builtinStyle, base.Document);
				base.Document.Styles.Add(style);
			}
			else
			{
				style = Style.CreateBuiltinStyle(builtinStyle, base.Document);
				base.Document.Styles.Add(style);
				if (builtinStyle != BuiltinStyle.MacroText && builtinStyle != BuiltinStyle.CommentSubject)
				{
					(base.Document.Styles.FindByName(name) as Style).ApplyBaseStyle("Normal");
				}
				UpdateNextStyle(style as Style);
			}
		}
		return style;
	}

	internal IStyle GetBuiltInTableStyle(BuiltinTableStyle builtinTableStyle)
	{
		CheckTableNormalStyle();
		string name = Style.BuiltInToName(builtinTableStyle);
		IStyle style = base.Document.Styles.FindByName(name, StyleType.TableStyle) as IWTableStyle;
		if (style == null)
		{
			style = (IWTableStyle)Style.CreateBuiltinStyle(builtinTableStyle, base.Document);
			if ((style as WTableStyle).StyleId > 10)
			{
				(style as WTableStyle).StyleId = 4094;
			}
			base.Document.Styles.Add(style);
			string text = style.Name.Replace("Accent", "-Accent");
			base.Document.StyleNameIds.Add(text.Replace(" ", ""), style.Name);
			(style as WTableStyle).ApplyBaseStyle("Normal Table");
		}
		return style;
	}

	private void CheckTableNormalStyle()
	{
		WTableStyle wTableStyle = base.Document.Styles.FindByName("Normal Table", StyleType.TableStyle) as WTableStyle;
		if (wTableStyle == null)
		{
			wTableStyle = (WTableStyle)Style.CreateBuiltinStyle(BuiltinTableStyle.TableNormal, base.Document);
			base.Document.Styles.Add(wTableStyle);
			base.Document.StyleNameIds.Add("TableNormal", wTableStyle.Name);
		}
	}

	internal void UpdateNextStyle(Style style)
	{
		if (!style.Name.Contains("List") && style.Name != "No Spacing")
		{
			style.NextStyle = "Normal";
		}
	}

	public void AcceptChanges()
	{
		foreach (WSection section in Sections)
		{
			section.MakeChanges(acceptChanges: true);
		}
		base.Document.Revisions.AcceptAll();
	}

	public void RejectChanges()
	{
		base.Document.Revisions.RejectAll();
	}

	public void Protect(ProtectionType type)
	{
		ResetProtectionTypesValues();
		Protect(type, null);
	}

	public void Protect(ProtectionType type, string password)
	{
		ResetProtectionTypesValues();
		DOP.SetProtection(type, password);
	}

	private void ResetProtectionTypesValues()
	{
		Settings.CryptProviderTypeValue = "rsaAES";
		Settings.CryptAlgorithmTypeValue = "typeAny";
		Settings.CryptAlgorithmClassValue = "hash";
		Settings.CryptAlgorithmSidValue = 14.ToString();
		Settings.CryptSpinCountValue = 100000.ToString();
	}

	public void EncryptDocument(string password)
	{
		if (string.IsNullOrEmpty(password))
		{
			throw new Exception("Password cannot be null or empty!");
		}
		m_password = password;
	}

	public void RemoveEncryption()
	{
		m_password = null;
	}

	internal IStyle AddStyle(StyleType styleType, string styleName)
	{
		if (styleType == StyleType.OtherStyle)
		{
			throw new NotSupportedException();
		}
		IStyle style = null;
		switch (styleType)
		{
		case StyleType.ParagraphStyle:
			style = new WParagraphStyle(base.Document);
			break;
		case StyleType.CharacterStyle:
			style = new WCharacterStyle(base.Document);
			break;
		case StyleType.TableStyle:
			style = new WTableStyle(base.Document);
			break;
		}
		if (style != null)
		{
			if (styleName != null && styleName.Length > 0)
			{
				style.Name = styleName;
			}
			Styles.Add(style);
		}
		return style;
	}

	private void CheckNormalStyle()
	{
		IStyle style = base.Document.Styles.FindByName("Normal", StyleType.ParagraphStyle) as WParagraphStyle;
		if (style == null)
		{
			style = (WParagraphStyle)Style.CreateBuiltinStyle(BuiltinStyle.Normal, base.Document);
			base.Document.Styles.Add(style);
		}
		style = base.Document.Styles.FindByName("Default Paragraph Font", StyleType.CharacterStyle) as WCharacterStyle;
		if (style == null)
		{
			style = (WCharacterStyle)Style.CreateBuiltinCharacterStyle(BuiltinStyle.DefaultParagraphFont, base.Document);
			(style as Style).IsSemiHidden = true;
			(style as Style).UnhideWhenUsed = true;
			base.Document.Styles.Add(style);
		}
	}

	public List<Entity> GetCrossReferenceItems(ReferenceType refernceType)
	{
		if (refernceType == ReferenceType.Bookmark)
		{
			return GetBookmarksValue();
		}
		return new List<Entity>();
	}

	private List<Entity> GetBookmarksValue()
	{
		List<Entity> list = new List<Entity>();
		foreach (Bookmark bookmark in Bookmarks)
		{
			if (bookmark.BookmarkStart != null && bookmark.BookmarkEnd != null && !bookmark.Name.StartsWith("_"))
			{
				list.Add(bookmark.BookmarkStart);
			}
		}
		return list;
	}

	private void OpenDocx(Stream stream)
	{
		DocxParser docxParser = new DocxParser();
		IsOpening = true;
		docxParser.Read(stream, this);
		AddEmptyParagraph();
		IsOpening = false;
	}

	private void OpenWordML(Stream stream)
	{
		DocxParser docxParser = new DocxParser();
		IsOpening = true;
		docxParser.ReadWordML(stream, this);
		IsOpening = false;
	}

	private void SaveDocx(Stream stream)
	{
		SortByZIndex(isFromHTMLExport: false);
		new DocxSerializator().Serialize(stream, this);
	}

	private void SaveODT(Stream stream)
	{
		new DocToODTConverter(m_doc).ConvertToODF(stream);
	}

	private void SaveWordML(Stream stream)
	{
		SortByZIndex(isFromHTMLExport: false);
		new DocxSerializator().SerializeWordML(stream, this);
	}

	private void SaveRtf(Stream stream)
	{
		new RtfWriter().Write(stream, this);
	}

	internal string GetRtfText()
	{
		return new RtfWriter().GetRtfText(this);
	}

	internal void SaveTxt(Stream stream)
	{
		SaveTxt(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
	}

	private void SaveMd(Stream stream)
	{
		SaveMd(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
	}

	private void SaveMd(Stream stream, Encoding encoding)
	{
		StreamWriter streamWriter = new StreamWriter(stream, encoding);
		try
		{
			new WordToMdConversion().ConvertAndWrite(string.Empty, this, streamWriter);
		}
		finally
		{
			streamWriter.Flush();
		}
	}

	public void SaveTxt(Stream stream, Encoding encoding)
	{
		StreamWriter streamWriter = new StreamWriter(stream, encoding);
		try
		{
			new TextConverter().Write(streamWriter, this);
		}
		finally
		{
			streamWriter.Flush();
		}
	}

	internal void OpenTxt(Stream stream)
	{
		Encoding encoding = GetEncoding("Windows-1252");
		if (Utf8Checker.IsUtf8(stream))
		{
			encoding = Encoding.UTF8;
		}
		StreamReader reader = new StreamReader(stream, encoding);
		new TextConverter().Read(reader, this);
	}

	internal void OpenMd(Stream stream)
	{
		IsOpening = true;
		Encoding encoding = GetEncoding("Windows-1252");
		if (Utf8Checker.IsUtf8(stream))
		{
			encoding = Encoding.UTF8;
		}
		StreamReader streamReader = new StreamReader(stream, encoding);
		MarkdownParser markdownParser = new MarkdownParser();
		markdownParser.ParseMd(streamReader, m_mdImportSettings);
		streamReader.Dispose();
		new MdToWordConversion().ConvertToWordDOM(markdownParser.markdownDocument, this);
		IsOpening = false;
	}

	internal void OpenText(string text)
	{
		new TextConverter().Read(text, this);
	}

	internal void OpenHTML(Stream stream, XHTMLValidationType validationType)
	{
		string codePageName = "Windows-1252";
		if (Utf8Checker.IsUtf8(stream))
		{
			codePageName = "utf-8";
		}
		StreamReader streamReader = new StreamReader(UtilityMethods.CloneStream(stream), GetEncoding(codePageName));
		string html = streamReader.ReadToEnd();
		streamReader.Close();
		if (Sections.Count == 0)
		{
			AddSection();
		}
		XHTMLValidateOption = validationType;
		LastSection.PageSetup.Margins.All = 72f;
		IsHTMLImport = true;
		LastSection.Body.InsertXHTML(html, 0);
		DOP.Dop2000.Copts.SplitPgBreakAndParaMark = false;
		DOP.Dop2000.Copts.DontUseIndentAsNumberingTabStop = false;
		DOP.Dop2000.Copts.UseNormalStyleForList = false;
		DOP.Dop2000.Copts.FELineBreak11 = false;
		DOP.Dop2000.Copts.AllowSpaceOfSameStyleInTable = false;
		DOP.Dop2000.Copts.WW11IndentRules = false;
		DOP.Dop2000.Copts.DontAutofitConstrainedTables = false;
		DOP.Dop2000.Copts.AutofitLikeWW11 = false;
		DOP.Dop2000.Copts.HangulWidthLikeWW11 = false;
		DOP.Dop2000.Copts.DontVertAlignCellWithSp = false;
		DOP.Dop2000.Copts.DontBreakConstrainedForcedTables = false;
		DOP.Dop2000.Copts.DontVertAlignInTxbx = false;
		DOP.Dop2000.Copts.Word11KerningPairs = false;
		if (LastSection.Body.Items.Count == 0 || LastSection.Body.Items.LastItem is WTable)
		{
			LastSection.Body.Items.Insert(LastSection.Body.Items.Count, new WParagraph(base.Document));
		}
	}

	public void Open(Stream stream, FormatType formatType, XHTMLValidationType validationType, string baseUrl)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("Stream");
		}
		if (baseUrl == null)
		{
			throw new ArgumentNullException("BaseUrl");
		}
		HtmlBaseUrl = baseUrl;
		OpenInternal(stream, formatType, validationType);
	}

	public void Open(Stream stream, FormatType formatType, XHTMLValidationType validationType)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("Stream");
		}
		OpenInternal(stream, formatType, validationType);
	}

	private void OpenInternal(Stream stream, FormatType formatType, XHTMLValidationType validationType)
	{
		UpdateFormatType(stream, ref formatType);
		ActualFormatType = formatType;
		if (FormatType.Html == formatType)
		{
			Init();
			OpenHTML(stream, validationType);
			RemoveTrailVersionWatermark(formatType);
		}
		else
		{
			Open(stream, formatType);
		}
	}

	public void Open(Stream stream, FormatType formatType)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("Stream");
		}
		OpenInternal(stream, formatType, null);
	}

	public void Open(Stream stream, FormatType formatType, string password)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("Stream");
		}
		if (password == null)
		{
			throw new ArgumentNullException("Password");
		}
		OpenInternal(stream, formatType, password);
	}

	private void OpenInternal(Stream stream, FormatType formatType, string password)
	{
		Init();
		Password = password;
		UpdateFormatType(stream, ref formatType);
		ActualFormatType = formatType;
		switch (formatType)
		{
		case FormatType.Doc:
		case FormatType.Dot:
		{
			DocReaderAdapter docReaderAdapter = new DocReaderAdapter();
			Settings.CompatibilityOptions[CompatibilityOption.SplitPgBreakAndParaMark] = true;
			using (WordReader reader = new WordReader(stream))
			{
				docReaderAdapter.Read(reader, this);
			}
			docReaderAdapter = null;
			break;
		}
		case FormatType.Docx:
		case FormatType.Word2007:
		case FormatType.Word2010:
		case FormatType.Word2013:
		case FormatType.Word2007Dotx:
		case FormatType.Word2010Dotx:
		case FormatType.Word2013Dotx:
		case FormatType.Dotx:
		case FormatType.Word2007Docm:
		case FormatType.Word2010Docm:
		case FormatType.Word2013Docm:
		case FormatType.Docm:
		case FormatType.Word2007Dotm:
		case FormatType.Word2010Dotm:
		case FormatType.Word2013Dotm:
		case FormatType.Dotm:
			OpenDocx(stream);
			break;
		case FormatType.WordML:
			OpenWordML(stream);
			break;
		case FormatType.Rtf:
			OpenRtf(stream);
			break;
		case FormatType.Txt:
			OpenTxt(stream);
			break;
		case FormatType.Markdown:
			OpenMd(stream);
			break;
		case FormatType.Html:
			OpenHTML(stream);
			break;
		default:
			throw new NotSupportedException("DocIO does not support this file format");
		}
		if (formatType != FormatType.Html)
		{
			RemoveTrailVersionWatermark(formatType);
		}
	}

	internal void OpenRtf(Stream stream)
	{
		IsOpening = true;
		Settings.CompatibilityOptions[CompatibilityOption.SplitPgBreakAndParaMark] = true;
		new RtfParser(this, stream).ParseToken();
		IsOpening = false;
	}

	internal void OpenRtf(string rtfText)
	{
		IsOpening = true;
		MemoryStream memoryStream = new MemoryStream();
		StreamWriter streamWriter = new StreamWriter(memoryStream, GetEncoding("ASCII"));
		streamWriter.Write(rtfText);
		rtfText = "";
		streamWriter.Flush();
		memoryStream.Position = 0L;
		new RtfParser(this, memoryStream).ParseToken();
		streamWriter.Dispose();
		IsOpening = false;
	}

	internal void OpenHTML(Stream stream)
	{
		string codePageName = "Windows-1252";
		if (Utf8Checker.IsUtf8(stream))
		{
			codePageName = "utf-8";
		}
		StreamReader streamReader = new StreamReader(UtilityMethods.CloneStream(stream), GetEncoding(codePageName));
		string html = streamReader.ReadToEnd();
		streamReader.Dispose();
		if (Sections.Count == 0)
		{
			AddSection();
		}
		LastSection.PageSetup.Margins.All = 72f;
		LastSection.Body.InsertXHTML(html, 0);
		if (LastSection.Body.Items.Count == 0 || LastSection.Body.Items.LastItem is WTable)
		{
			LastSection.Body.Items.Insert(LastSection.Body.Items.Count, new WParagraph(base.Document));
		}
	}

	private void UpdateFormatType(Stream stream, ref FormatType formatType)
	{
		if (formatType == FormatType.Automatic)
		{
			if (stream is FileStream)
			{
				formatType = GetFormatType((stream as FileStream).Name, isStorageFile: false);
			}
			else
			{
				formatType = FormatType.Doc;
			}
		}
		stream.Position = 0L;
		if (CheckForEncryption(stream))
		{
			using ICompoundFile compoundFile = CreateCompoundFile(stream);
			ICompoundStorage rootStorage = compoundFile.RootStorage;
			SecurityHelper.EncrytionType encryptionType = new SecurityHelper().GetEncryptionType(rootStorage);
			stream.Position = 0L;
			if (encryptionType != SecurityHelper.EncrytionType.None)
			{
				formatType = FormatType.Docx;
				return;
			}
		}
		stream.Position = 0L;
		byte[] array = new byte[5];
		if (stream.Read(array, 0, 5) == 5 && array[0] == 80 && array[1] == 75)
		{
			stream.Position = 0L;
			formatType = FormatType.Docx;
		}
		else if (array[0] == 123 && array[1] == 92 && array[2] == 114 && array[3] == 116 && array[4] == 102)
		{
			stream.Position = 0L;
			formatType = FormatType.Rtf;
		}
		else
		{
			stream.Position = 0L;
			if (DocGen.CompoundFile.DocIO.Net.CompoundFile.CheckHeader(stream))
			{
				stream.Position = 0L;
				formatType = FormatType.Doc;
			}
			else
			{
				try
				{
					XmlReader xmlReader = UtilityMethods.CreateReader(stream);
					xmlReader.MoveToElement();
					if (xmlReader.LocalName == "wordDocument" || xmlReader.LocalName == "package")
					{
						formatType = FormatType.WordML;
					}
					else if (xmlReader.LocalName == "html")
					{
						formatType = FormatType.Html;
					}
				}
				catch (Exception)
				{
				}
			}
		}
		stream.Position = 0L;
	}

	public void Save(Stream stream, FormatType formatType)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("Stream");
		}
		SaveInternal(stream, formatType);
		if (stream.CanSeek)
		{
			stream.Position = 0L;
		}
	}

	internal string GetAsRoman(int number)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(GenerateNumber(ref number, 1000, "M"));
		stringBuilder.Append(GenerateNumber(ref number, 900, "CM"));
		stringBuilder.Append(GenerateNumber(ref number, 500, "D"));
		stringBuilder.Append(GenerateNumber(ref number, 400, "CD"));
		stringBuilder.Append(GenerateNumber(ref number, 100, "C"));
		stringBuilder.Append(GenerateNumber(ref number, 90, "XC"));
		stringBuilder.Append(GenerateNumber(ref number, 50, "L"));
		stringBuilder.Append(GenerateNumber(ref number, 40, "XL"));
		stringBuilder.Append(GenerateNumber(ref number, 10, "X"));
		stringBuilder.Append(GenerateNumber(ref number, 9, "IX"));
		stringBuilder.Append(GenerateNumber(ref number, 5, "V"));
		stringBuilder.Append(GenerateNumber(ref number, 4, "IV"));
		stringBuilder.Append(GenerateNumber(ref number, 1, "I"));
		return stringBuilder.ToString();
	}

	private string GenerateNumber(ref int value, int magnitude, string letter)
	{
		StringBuilder stringBuilder = new StringBuilder();
		while (value >= magnitude)
		{
			value -= magnitude;
			stringBuilder.Append(letter);
		}
		return stringBuilder.ToString();
	}

	private string GetChineseWithinTenThousand(int number, bool isAboveFiveDigit, ListPatternType patternType)
	{
		if (number == 0)
		{
			return "○";
		}
		string text = "";
		string[] array = new string[10] { "○", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
		string[] array2 = new string[4] { "", "十", "百", "千" };
		int num = 0;
		while (number > 0 && num < array2.Length)
		{
			int num2 = number % 10;
			if (patternType == ListPatternType.ChineseCountingThousand)
			{
				text = ((num2 == 0) ? (array[num2] + text) : (array[num2] + array2[num] + text));
			}
			else if (num2 != 0)
			{
				text = ((num2 == 1 && num > 0) ? "" : array[num2]) + array2[num] + text;
			}
			num++;
			number /= 10;
		}
		text = Regex.Replace(text, "○*○", "○");
		text = Regex.Replace(text, "○$", "");
		if (!isAboveFiveDigit && patternType == ListPatternType.ChineseCountingThousand)
		{
			text = Regex.Replace(text, "^一十", "十");
		}
		return text;
	}

	internal string GetChineseExpression(int number, ListPatternType patternType)
	{
		if (number == 0)
		{
			return "○";
		}
		if (number >= 1000000)
		{
			return string.Empty;
		}
		string text = "";
		string[] array = new string[3] { "", "万", "亿" };
		int num = 0;
		bool flag = number > 100000;
		while (number > 0)
		{
			int num2 = 0;
			num2 = ((number <= 10000 || patternType != ListPatternType.ChineseCountingThousand) ? (number % 10000) : (number % 100000));
			if (num2 != 0)
			{
				text = GetChineseWithinTenThousand(num2, flag, patternType) + array[num] + text;
				if (patternType == ListPatternType.ChineseCountingThousand)
				{
					if ((num2 < 1000 && num2 != number) || (flag && num2 < 10000 && num2 != number))
					{
						text = "○" + text;
					}
				}
				else if (num2 / 1000 == 1 && num2 != number)
				{
					text = "一" + text;
				}
			}
			num++;
			number /= 10000;
		}
		return text;
	}

	internal string GetAsLetter(int number)
	{
		if (number <= 0)
		{
			return "";
		}
		int num = number / 26;
		int num2 = number % 26;
		if (num2 == 0)
		{
			num2 = 26;
			num--;
		}
		char c = (char)(64 + num2);
		string text = c.ToString();
		while (num > 0)
		{
			text += c;
			num--;
		}
		return text;
	}

	internal string GetSpanishCardinalTextString(bool cardinalString, string text)
	{
		if (cardinalString)
		{
			text = text.Trim();
			for (int i = 0; i < text.Length; i++)
			{
				if (char.IsLetter(text[i]))
				{
					cardinalString = false;
					break;
				}
			}
			if (cardinalString)
			{
				text = NumberToSpanishWords(int.Parse(text), isCardText: true);
			}
		}
		return text;
	}

	internal string GetCardTextString(bool cardinalString, string text)
	{
		if (cardinalString)
		{
			text = text.Trim();
			for (int i = 0; i < text.Length; i++)
			{
				if (char.IsLetter(text[i]))
				{
					cardinalString = false;
					break;
				}
			}
			if (cardinalString)
			{
				text = NumberToWords(int.Parse(text), isCardText: true);
			}
		}
		return text;
	}

	internal string GetOrdTextString(bool ordinalString, string text)
	{
		if (ordinalString)
		{
			text = text.Trim();
			for (int i = 0; i < text.Length; i++)
			{
				if (char.IsLetter(text[i]))
				{
					ordinalString = false;
					break;
				}
			}
			if (ordinalString)
			{
				text = NumberToWords(int.Parse(text), isCardText: false);
			}
		}
		return text;
	}

	internal string GetSpanishOrdinalTextString(bool ordinalString, string text)
	{
		if (ordinalString)
		{
			text = text.Trim();
			for (int i = 0; i < text.Length; i++)
			{
				if (char.IsLetter(text[i]))
				{
					ordinalString = false;
					break;
				}
			}
			if (ordinalString)
			{
				text = NumberToSpanishWords(int.Parse(text), isCardText: false);
			}
		}
		return text;
	}

	internal string NumberToSpanishWords(int number, bool isCardText)
	{
		if (number == 0 && isCardText)
		{
			return "cero";
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (number / 1000 > 0 && number <= 10000)
		{
			string[] array = new string[11]
			{
				"", "mil", "dos mil", "tres mil", "cuatro mil", "cinco mil", "seis mil", "siete mil", "ocho mil", "nueve mil",
				"diez mil"
			};
			string[] array2 = new string[11]
			{
				"", "milésimo", "dosmilésimo", "tresmilésimo", "cuatromilésimo", "cincomilésimo", "seismilésimo", "sietemilésimo", "ochomilésimo", "nuevemilésimo",
				"diezmilésimo"
			};
			if (isCardText)
			{
				stringBuilder.Append(array[number / 1000]);
			}
			else
			{
				stringBuilder.Append(array2[number / 1000]);
			}
			number %= 1000;
		}
		if (number / 100 > 0)
		{
			if (!string.IsNullOrEmpty(stringBuilder.ToString()))
			{
				stringBuilder.Append(" ");
			}
			string[] array3 = new string[10] { "", "ciento", "doscientos", "trescientos", "cuatrocientos", "quinientos", "seiscientos", "setecientos", "ochocientos", "novecientos" };
			string[] array4 = new string[10] { "", "centésimo", "ducentésimo", "tricentésimo", "cuadringentésimo", "quingentésimo", "sexcentésimo", "septingentésimo", "octingentésimo", "noningentésimo" };
			if (isCardText)
			{
				stringBuilder.Append(array3[number / 100]);
			}
			else
			{
				stringBuilder.Append(array4[number / 100]);
			}
			number %= 100;
		}
		if (number > 0 && number < 100)
		{
			if (!string.IsNullOrEmpty(stringBuilder.ToString()))
			{
				stringBuilder.Append(" ");
			}
			string[] array5 = null;
			array5 = ((!isCardText) ? new string[20]
			{
				"", "primero", "segundo", "tercero", "cuarto", "quinto", "sexto", "séptimo", "octavo", "noveno",
				"décimo", "undécimo", "duodécimo", "decimotercero", "decimocuarto", "decimoquinto", "decimosexto", "decimoséptimo", "decimoctavo", "decimonoveno"
			} : new string[20]
			{
				"", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve",
				"diez", "once", "doce", "trece", "catorce", "quince", "dieciséis", "diecisiete", "dieciocho", "diecinueve"
			});
			string[] array6 = new string[10] { "", "diez", "veinte", "treinta", "cuarenta", "cincuenta", "sesenta", "setenta", "ochenta", "noventa" };
			string[] array7 = new string[10] { "", "décimo", "vigésimo", "trigésimo", "cuadragésimo", "quincuagésimo", "sexagésimo", "septuagésimo", "octogésimo", "nonagésimo" };
			string[] array8 = new string[10] { "", "veintiuno", "veintidós", "veintitrés", "veinticuatro", "veinticinco", "veintiséis", "veintisiete", "veintiocho", "veintinueve" };
			if (number < 20)
			{
				stringBuilder.Append(array5[number]);
			}
			else if (number > 20 && number < 30 && isCardText)
			{
				stringBuilder.Append(array8[number % 10]);
			}
			else
			{
				if (isCardText && number % 10 > 0)
				{
					stringBuilder.Append(array6[number / 10]);
				}
				else if (isCardText && number % 10 == 0)
				{
					stringBuilder.Append(array6[number / 10]);
				}
				if (number % 10 > 0 && !isCardText)
				{
					stringBuilder.Append(array7[number / 10]);
				}
				if (number % 10 == 0 && !isCardText)
				{
					stringBuilder.Append(array7[number / 10]);
				}
				else if (number % 10 > 0)
				{
					if (isCardText)
					{
						stringBuilder.Append(" y " + array5[number % 10]);
					}
					else
					{
						stringBuilder.Append(" " + array5[number % 10]);
					}
				}
			}
		}
		return stringBuilder.ToString();
	}

	internal string NumberToWords(int number, bool isCardText)
	{
		if (number == 0)
		{
			return "zero";
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (number / 1000000 > 0)
		{
			stringBuilder.Append(NumberToWords(number / 1000000, isCardText) + " million ");
			if (!isCardText && number % 10 == 0)
			{
				stringBuilder.Append("th");
			}
			number %= 1000000;
		}
		if (number / 1000 > 0)
		{
			stringBuilder.Append(NumberToWords(number / 1000, isCardText) + " thousand ");
			if (!isCardText && number % 10 == 0)
			{
				stringBuilder.Append("th");
			}
			number %= 1000;
		}
		if (number / 100 > 0)
		{
			stringBuilder.Append(NumberToWords(number / 100, isCardText) + " hundred ");
			if (!isCardText && number % 10 == 0)
			{
				stringBuilder.Append("th");
			}
			number %= 100;
		}
		if (number > 0)
		{
			if (!string.IsNullOrEmpty(stringBuilder.ToString()) && isCardText)
			{
				stringBuilder.Append("and ");
			}
			string[] array = null;
			array = ((!isCardText) ? new string[20]
			{
				"", "first", "second", "third", "fourth", "fifth", "sixth", "seventh", "eighth", "ninth",
				"tenth", "eleventh", "twelfth", "thirteenth", "fourteenth", "fifteenth", "sixteenth", "seventeenth", "eighteenth", "nineteenth"
			} : new string[20]
			{
				"", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine",
				"ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen"
			});
			string[] array2 = new string[10] { "", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };
			string[] array3 = new string[10] { "", "tenth", "twentieth", "thirtieth", "fortieth", "fiftieth", "sixtieth", "seventieth", "eightieth", "ninetieth" };
			if (number < 20)
			{
				stringBuilder.Append(array[number]);
			}
			else
			{
				if (isCardText || number % 10 > 0)
				{
					stringBuilder.Append(array2[number / 10]);
				}
				if (number % 10 == 0 && !isCardText)
				{
					stringBuilder.Append(array3[number / 10]);
				}
				else if (number % 10 > 0)
				{
					stringBuilder.Append("-" + array[number % 10]);
				}
			}
		}
		return stringBuilder.ToString();
	}

	private bool IsTrailParagraph(WParagraph paragraph, bool isText)
	{
		return false;
	}

	internal void RemoveTrailVersionWatermark(FormatType formatType)
	{
		if (formatType == FormatType.Txt)
		{
			if (Sections.Count > 0 && Sections[0].Body.Items.Count > 0 && IsTrailParagraph(Sections[0].Body.Items[0] as WParagraph, isText: true))
			{
				Sections[0].Body.Items.RemoveAt(0);
			}
			if (Sections.Count > 0 && LastSection.Body.Items.Count > 1 && IsTrailParagraph(LastSection.Body.Items[LastSection.Body.Items.Count - 1] as WParagraph, isText: true))
			{
				LastSection.Body.Items.RemoveAt(LastSection.Body.Items.Count - 1);
				if (LastSection.Body.Items.Count > 0 && LastSection.Body.Items[LastSection.Body.Items.Count - 1] is WParagraph && (LastSection.Body.Items[LastSection.Body.Items.Count - 1] as WParagraph).Text == "\"www.syncfusion.com/account/claim-license-key\"")
				{
					LastSection.Body.Items.RemoveAt(LastSection.Body.Items.Count - 1);
				}
			}
			return;
		}
		if (Sections.Count > 0 && Sections[0].Body.Items.Count > 0 && IsTrailParagraph(Sections[0].Body.Items[0] as WParagraph, isText: true))
		{
			Sections[0].Body.Items.RemoveAt(0);
		}
		if (Sections.Count > 0 && LastSection.Body.Items.Count > 0 && IsTrailParagraph(LastSection.Body.Items[LastSection.Body.Items.Count - 1] as WParagraph, isText: true))
		{
			LastSection.Body.Items.RemoveAt(LastSection.Body.Items.Count - 1);
		}
		if (formatType == FormatType.Html)
		{
			if (Sections.Count > 0 && Sections[0].Body.Items.Count > 0 && Sections[0].Body.Items[0] is WParagraph)
			{
				Sections[0].Body.Items.RemoveAt(0);
			}
			return;
		}
		foreach (WSection section in Sections)
		{
			if (section.HeadersFooters.OddHeader.ChildEntities.Count > 0)
			{
				WParagraph paragraph = section.HeadersFooters.OddHeader.ChildEntities[0] as WParagraph;
				if (IsTrailParagraph(paragraph, isText: false))
				{
					section.HeadersFooters.OddHeader.ChildEntities.RemoveAt(0);
				}
			}
			if (section.HeadersFooters.EvenHeader.ChildEntities.Count > 0)
			{
				WParagraph paragraph2 = section.HeadersFooters.EvenHeader.ChildEntities[0] as WParagraph;
				if (IsTrailParagraph(paragraph2, isText: false))
				{
					section.HeadersFooters.EvenHeader.ChildEntities.RemoveAt(0);
				}
			}
			if (section.HeadersFooters.FirstPageHeader.ChildEntities.Count > 0)
			{
				WParagraph paragraph3 = section.HeadersFooters.FirstPageHeader.ChildEntities[0] as WParagraph;
				if (IsTrailParagraph(paragraph3, isText: false))
				{
					section.HeadersFooters.FirstPageHeader.ChildEntities.RemoveAt(0);
				}
			}
		}
	}

	internal void AddTrailVersionWatermarkForODT()
	{
		if (!WriteWarning)
		{
			return;
		}
	}

	internal void AddTrailVersionWatermark(bool isWordToImage)
	{
		if (!WriteWarning)
		{
			return;
		}
		
	}

	internal void AddFirstAndLastParaTrialMessage(bool isWordToImage)
	{
		return;
	}

	private void SaveInternal(Stream stream, FormatType formatType)
	{
		if (stream.CanSeek)
		{
			stream.SetLength(0L);
		}
		if (UpdateFields)
		{
			UpdateDocumentFields(performLayout: false);
		}
		SaveFormatType = formatType;
		switch (formatType)
		{
		default:
			AddTrailVersionWatermark(isWordToImage: false);
			break;
		case FormatType.Odt:
			AddTrailVersionWatermarkForODT();
			break;
		case FormatType.Txt:
		case FormatType.Html:
			break;
		}
		switch (formatType)
		{
		case FormatType.Doc:
		case FormatType.Dot:
		{
			DocWriterAdapter docWriterAdapter = new DocWriterAdapter();
			using (WordWriter wordWriter = new WordWriter(stream))
			{
				if (formatType == FormatType.Dot)
				{
					wordWriter.IsTemplate = true;
				}
				docWriterAdapter.Write(wordWriter, this);
			}
			docWriterAdapter = null;
			break;
		}
		case FormatType.Docx:
		case FormatType.StrictDocx:
		case FormatType.Word2007:
		case FormatType.Word2010:
		case FormatType.Word2013:
		case FormatType.Word2007Dotx:
		case FormatType.Word2010Dotx:
		case FormatType.Word2013Dotx:
		case FormatType.Dotx:
		case FormatType.Word2007Docm:
		case FormatType.Word2010Docm:
		case FormatType.Word2013Docm:
		case FormatType.Docm:
		case FormatType.Word2007Dotm:
		case FormatType.Word2010Dotm:
		case FormatType.Word2013Dotm:
		case FormatType.Dotm:
			SaveDocx(stream);
			break;
		case FormatType.Odt:
			SaveODT(stream);
			break;
		case FormatType.WordML:
			SaveWordML(stream);
			break;
		case FormatType.Rtf:
			SaveRtf(stream);
			break;
		case FormatType.Txt:
			SaveTxt(stream);
			break;
		case FormatType.Markdown:
			SaveMd(stream);
			break;
		case FormatType.Html:
			SaveHTML(stream);
			break;
		case FormatType.Automatic:
			break;
		}
	}

	internal string GetOrdinal(int num, WCharacterFormat characterFormat)
	{
		switch (characterFormat.LocaleIdASCII)
		{
		case 1029:
		case 1031:
		case 1035:
		case 1038:
		case 1044:
		case 1045:
		case 1050:
		case 1055:
		case 1060:
		case 1061:
		case 1069:
		case 2055:
		case 2068:
		case 2074:
		case 3079:
		case 4103:
		case 4122:
		case 5127:
		case 5146:
		case 6170:
		case 8218:
			return num + ".";
		case 1036:
		case 2060:
		case 3084:
		case 4108:
		case 5132:
		case 6156:
		case 8204:
		case 9228:
		case 10252:
		case 11276:
		case 12300:
		case 13324:
		case 14348:
		case 15372:
			if (num == 1)
			{
				return num + "er";
			}
			return num + "e";
		case 1043:
		case 2067:
			return num + "e";
		case 1032:
			return num + "o";
		case 1040:
		case 2064:
			return num + "°";
		case 1034:
		case 1046:
		case 2058:
		case 2070:
		case 3082:
		case 4106:
		case 5130:
		case 6154:
		case 7178:
		case 8202:
		case 9226:
		case 10250:
		case 11274:
		case 12298:
		case 13322:
		case 14346:
		case 15370:
		case 16394:
		case 17418:
		case 18442:
		case 19466:
		case 20490:
		case 21514:
			return num + "º";
		case 1049:
		case 2073:
			return num + "-й";
		case 1053:
		case 2077:
			return GetOrdinalInSwedish(num);
		case 1027:
			return GetOrdinalInCatalan(num);
		case 1030:
			return GetOrdinalInDanish(num);
		default:
			return GetOrdinalInEnglish(num);
		}
	}

	private string GetOrdinalInSwedish(int num)
	{
		if (num == 11 || num == 12)
		{
			return num + ":e";
		}
		if (num % 10 == 1 || num % 10 == 2)
		{
			return num + ":a";
		}
		return num + ":e";
	}

	private string GetOrdinalInCatalan(int num)
	{
		return num switch
		{
			1 => num + ".", 
			2 => num + "n", 
			3 => num + "r", 
			4 => num + "t", 
			14 => num + "èh", 
			_ => num + "è", 
		};
	}

	private string GetOrdinalInDanish(int num)
	{
		if (num == 0)
		{
			return num + "te";
		}
		switch (num % 100)
		{
		case 0:
			return num + "ende";
		case 1:
			return num + "ste";
		case 2:
			return num + "nden";
		case 3:
			return num + "dje";
		case 4:
			return num + "rde";
		case 5:
		case 6:
		case 11:
		case 12:
		case 30:
			return num + "te";
		default:
			return num + "nde";
		}
	}

	private string GetOrdinalInEnglish(int num)
	{
		int num2 = num % 100;
		if ((uint)(num2 - 11) <= 2u)
		{
			return num + "th";
		}
		return (num % 10) switch
		{
			1 => num + "st", 
			2 => num + "nd", 
			3 => num + "rd", 
			_ => num + "th", 
		};
	}

	private void SaveHTML(Stream stream)
	{
		new HTMLExport().SaveAsXhtml(this, stream);
	}

	public new void Close()
	{
		IsClosing = true;
		CloseContent();
		GC.WaitForPendingFinalizers();
		ResetSingleLineReplace();
		m_doc = this;
		Init();
		IsClosing = false;
	}

	public void Dispose()
	{
		Close();
	}

	internal static bool CompareArray(byte[] buffer1, byte[] buffer2)
	{
		bool result = true;
		for (int i = 0; i < buffer1.Length; i++)
		{
			if (buffer1[i] != buffer2[i])
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private void CloseContent()
	{
		CloseSecContent();
		CloseStyles();
		if (m_builtinProp != null)
		{
			m_builtinProp.Close();
			m_builtinProp = null;
		}
		if (FFNStringTable != null)
		{
			FFNStringTable.Close();
			FFNStringTable = null;
		}
		if (m_customProp != null)
		{
			m_customProp.Close();
			m_customProp = null;
		}
		if (m_imageCollection != null)
		{
			m_imageCollection.Clear();
			m_imageCollection = null;
		}
		if (m_oleObjectCollection != null)
		{
			m_oleObjectCollection.Clear();
			m_oleObjectCollection = null;
		}
		if (m_escher != null)
		{
			m_escher.Close();
			m_escher = null;
		}
		if (m_bookmarks != null)
		{
			m_bookmarks.Close();
			m_bookmarks = null;
		}
		if (m_editableRanges != null)
		{
			m_editableRanges.Close();
			m_editableRanges = null;
		}
		if (m_revisions != null)
		{
			m_revisions.Close();
			m_revisions = null;
		}
		if (m_fields != null)
		{
			if (m_fields.m_sortedAutoNumFields != null)
			{
				m_fields.m_sortedAutoNumFields.Clear();
				m_fields.m_sortedAutoNumFields = null;
			}
			if (m_fields.m_sortedAutoNumFieldIndexes != null)
			{
				m_fields.m_sortedAutoNumFieldIndexes.Clear();
				m_fields.m_sortedAutoNumFieldIndexes = null;
			}
			m_fields.Close();
			m_fields = null;
		}
		if (m_txbxItems != null)
		{
			m_txbxItems.Close();
			m_txbxItems = null;
		}
		if (m_footnotes != null)
		{
			m_footnotes.Close();
			m_footnotes = null;
		}
		if (m_endnotes != null)
		{
			m_endnotes.Close();
			m_endnotes = null;
		}
		if (m_Comments != null)
		{
			m_Comments.Close();
			m_Comments = null;
		}
		if (m_CommentsEx != null)
		{
			m_CommentsEx.Close();
			m_CommentsEx = null;
		}
		if (m_mailMerge != null)
		{
			m_mailMerge.Close();
			m_mailMerge = null;
		}
		if (m_viewSetup != null)
		{
			m_viewSetup.Close();
			m_viewSetup = null;
		}
		if (m_watermark != null)
		{
			m_watermark.Close();
			m_watermark = null;
		}
		if (m_background != null)
		{
			m_background.Close();
			m_background = null;
		}
		m_dop = null;
		if (m_grammarSpellingData != null)
		{
			m_grammarSpellingData.Close();
			m_grammarSpellingData = null;
		}
		m_password = null;
		m_macrosData = null;
		m_escherDataContainers = null;
		m_escherContainers = null;
		m_macroCommands = null;
		m_defShapeId = 1;
		m_standardAsciiFont = null;
		m_standardFarEastFont = null;
		m_standardNonFarEastFont = null;
		ThrowExceptionsForUnsupportedElements = false;
		if (m_latentStyles2010 != null)
		{
			m_latentStyles2010.Close();
			m_latentStyles2010 = null;
		}
		if (m_defCharFormat != null)
		{
			m_defCharFormat.Close();
			m_defCharFormat = null;
		}
		if (m_defParaFormat != null)
		{
			m_defParaFormat.Close();
			m_defParaFormat = null;
		}
		if (m_docxPackage != null)
		{
			m_docxPackage.Close();
			m_docxPackage = null;
		}
		if (m_variables != null)
		{
			m_variables.Close();
			m_variables = null;
		}
		m_fontSubstitutionTable = null;
		m_htmlValidationOption = XHTMLValidationType.None;
		if (m_props != null)
		{
			m_props.Close();
			m_props = null;
		}
		if (m_saveOptions != null)
		{
			m_saveOptions.Close();
			m_saveOptions = null;
		}
		if (m_docxProps != null)
		{
			foreach (Stream docxProp in m_docxProps)
			{
				docxProp.Close();
			}
			m_docxProps.Clear();
			m_docxProps = null;
		}
		ImportStyles = true;
		m_nextParaItem = null;
		m_prevBodyItem = null;
		if (m_settings != null)
		{
			m_settings.Close();
			m_settings = null;
		}
		DocHasThemes = false;
		if (m_themes != null)
		{
			m_themes.Close();
			m_themes = null;
		}
		if (m_styleNameIds != null)
		{
			m_styleNameIds.Clear();
			m_styleNameIds = null;
		}
		if (m_fontSubstitutionTable != null)
		{
			m_fontSubstitutionTable.Clear();
			m_fontSubstitutionTable = null;
		}
		m_tableOfContent = null;
		if (m_usedFonts != null)
		{
			m_usedFonts.Clear();
			m_usedFonts = null;
		}
		if (m_vbaProject != null)
		{
			m_vbaProject.Close();
			m_vbaProject = null;
		}
		if (m_vbaProjectSignature != null)
		{
			m_vbaProjectSignature.Close();
			m_vbaProjectSignature = null;
		}
		if (m_vbaProjectSignatureAgile != null)
		{
			m_vbaProjectSignatureAgile.Close();
			m_vbaProjectSignatureAgile = null;
		}
		if (m_CustomUIPartContainer != null)
		{
			m_CustomUIPartContainer.Close();
			m_CustomUIPartContainer = null;
		}
		if (m_UserCustomizationPartContainer != null)
		{
			m_UserCustomizationPartContainer.Close();
			m_UserCustomizationPartContainer = null;
		}
		if (m_CustomXMLContainer != null)
		{
			m_CustomXMLContainer.Close();
			m_CustomXMLContainer = null;
		}
		if (m_vbaData != null)
		{
			m_vbaData.Clear();
			m_vbaData = null;
		}
		if (m_docEvents != null)
		{
			m_docEvents.Clear();
			m_docEvents = null;
		}
		if (m_listStyleNames != null)
		{
			m_listStyleNames.Clear();
			m_listStyleNames = null;
		}
		if (m_AutoShapeCollection != null)
		{
			m_AutoShapeCollection.Clear();
			m_AutoShapeCollection = null;
		}
		if (RenderHelper != null)
		{
			RenderHelper = null;
		}
		AdapterListIDHolder.Instance.Close();
	}

	private void CloseSecContent()
	{
		if (m_sections != null && m_sections.Count > 0)
		{
			for (int i = 0; i < m_sections.Count; i++)
			{
				m_sections[i].Close();
			}
		}
		if (m_sections != null)
		{
			m_sections.Close();
			m_sections = null;
		}
	}

	private void CloseStyles()
	{
		if (m_styles != null)
		{
			int count = m_styles.Count;
			for (int i = 0; i < count; i++)
			{
				(m_styles[i] as Style).Close();
			}
			(m_styles as StyleCollection).InnerList.Clear();
			m_styles = null;
		}
		if (m_listStyles != null)
		{
			int count2 = m_listStyles.Count;
			for (int j = 0; j < count2; j++)
			{
				m_listStyles[j].Close();
			}
			m_listStyles.Close();
			m_listStyles = null;
		}
		if (m_listStyleNames != null)
		{
			m_listStyleNames.Clear();
			m_listStyleNames = null;
		}
		if (m_listOverrides != null)
		{
			int count3 = m_listOverrides.Count;
			for (int k = 0; k < count3; k++)
			{
				m_listOverrides[k].Close();
			}
			m_listOverrides.Close();
			m_listOverrides = null;
		}
	}

	internal Stream[] RenderAsImages(ExportImageFormat type)
	{
		AddTrailVersionWatermark(isWordToImage: true);
		DocumentLayouter.m_dc = new DrawingContext();
		DocumentLayouter.m_dc.FontStreams = FontSettings.FontStreams;
		DocumentLayouter documentLayouter = new DocumentLayouter();
		documentLayouter.Layout(this);
		return documentLayouter.DrawToImage(0, -1, type);
	}

	public Stream[] RenderAsImages()
	{
		AddTrailVersionWatermark(isWordToImage: true);
		return RenderAsImage(0, -1, ExportImageFormat.Png);
	}

	public Stream[] RenderAsImages(int startPageIndex, int numberOfPages)
	{
		AddTrailVersionWatermark(isWordToImage: true);
		return RenderAsImage(startPageIndex, numberOfPages, ExportImageFormat.Png);
	}

	private Stream[] RenderAsImage(int pageIndex, int noOfPages, ExportImageFormat saveFormat)
	{
		RevisionOptions.ShowMarkup = RevisionType.None;
		RevisionOptions.CommentDisplayMode = CommentDisplayMode.Hide;
		SortByZIndex(isFromHTMLExport: false);
		DocumentLayouter.m_dc = new DrawingContext();
		FontSettings.EmbedDocumentFonts(this);
		DocumentLayouter.m_dc.FontStreams = FontSettings.FontStreams;
		DocumentLayouter documentLayouter = new DocumentLayouter();
		documentLayouter.Layout(this);
		Stream[] array = documentLayouter.DrawToImage(pageIndex, noOfPages, saveFormat);
		List<Stream> list = new List<Stream>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null)
			{
				list.Add(array[i]);
			}
		}
		Stream[] array2 = new Stream[list.Count];
		for (int j = 0; j < list.Count; j++)
		{
			array2[j] = list[j];
		}
		documentLayouter.Close();
		return array2;
	}

	public Stream RenderAsImages(int startPageIndex, ExportImageFormat imageFormat)
	{
		AddTrailVersionWatermark(isWordToImage: true);
		Stream[] array = RenderAsImage(startPageIndex, 1, imageFormat);
		if (array == null || array[0] == null)
		{
			return null;
		}
		return array[0];
	}

	public TextSelection Find(Regex pattern)
	{
		foreach (WSection section in Sections)
		{
			foreach (WTextBody childEntity in section.ChildEntities)
			{
				TextSelection textSelection = childEntity.Find(pattern);
				if (textSelection != null)
				{
					return textSelection;
				}
			}
		}
		return null;
	}

	public TextSelection[] FindSingleLine(Regex pattern)
	{
		TextSelection[] array = null;
		foreach (WSection section in Sections)
		{
			array = TextFinder.Instance.FindSingleLine(section.Body, pattern);
			if (array != null)
			{
				break;
			}
		}
		if (array == null)
		{
			TextFinder.Instance.SingleLinePCol.Clear();
		}
		return array;
	}

	public TextSelection Find(string given, bool caseSensitive, bool wholeWord)
	{
		Regex pattern = FindUtils.StringToRegex(given, caseSensitive, wholeWord);
		return Find(pattern);
	}

	public TextSelection[] FindSingleLine(string given, bool caseSensitive, bool wholeWord)
	{
		Regex pattern = FindUtils.StringToRegex(given, caseSensitive, wholeWord);
		return FindSingleLine(pattern);
	}

	public TextSelection[] FindAll(Regex pattern)
	{
		TextSelectionList textSelectionList = null;
		foreach (WSection section in Sections)
		{
			foreach (WTextBody childEntity in section.ChildEntities)
			{
				TextSelectionList textSelectionList2 = childEntity.FindAll(pattern, isDocumentComparison: false, isFromTextbody: false);
				if (textSelectionList2 != null && textSelectionList2.Count > 0)
				{
					if (textSelectionList == null)
					{
						textSelectionList = textSelectionList2;
					}
					else
					{
						textSelectionList.AddRange(textSelectionList2);
					}
				}
			}
		}
		return textSelectionList?.ToArray();
	}

	internal TextSelection[] FindAll(Regex pattern, bool isDocumentComparison)
	{
		TextSelectionList textSelectionList = null;
		foreach (WSection section in Sections)
		{
			foreach (WTextBody childEntity in section.ChildEntities)
			{
				if (isDocumentComparison && childEntity is HeaderFooter)
				{
					continue;
				}
				TextSelectionList textSelectionList2 = childEntity.FindAll(pattern, isDocumentComparison, isFromTextbody: false);
				if (textSelectionList2 != null && textSelectionList2.Count > 0)
				{
					if (textSelectionList == null)
					{
						textSelectionList = textSelectionList2;
					}
					else
					{
						textSelectionList.AddRange(textSelectionList2);
					}
				}
			}
		}
		return textSelectionList?.ToArray();
	}

	public TextSelection[] FindAll(string given, bool caseSensitive, bool wholeWord)
	{
		Regex pattern = FindUtils.StringToRegex(given, caseSensitive, wholeWord);
		return FindAll(pattern);
	}

	public int Replace(Regex pattern, string replace)
	{
		int num = 0;
		foreach (WSection section in Sections)
		{
			foreach (WTextBody childEntity in section.ChildEntities)
			{
				num += childEntity.Replace(pattern, replace);
				if (ReplaceFirst && num > 0)
				{
					return num;
				}
			}
		}
		return num;
	}

	public int Replace(string given, string replace, bool caseSensitive, bool wholeWord)
	{
		Regex pattern = FindUtils.StringToRegex(given, caseSensitive, wholeWord);
		return Replace(pattern, replace);
	}

	public int Replace(string given, TextSelection textSelection, bool caseSensitive, bool wholeWord)
	{
		return Replace(given, textSelection, caseSensitive, wholeWord, saveFormatting: false);
	}

	public int Replace(string given, TextSelection textSelection, bool caseSensitive, bool wholeWord, bool saveFormatting)
	{
		Regex pattern = FindUtils.StringToRegex(given, caseSensitive, wholeWord);
		return Replace(pattern, textSelection, saveFormatting);
	}

	public int Replace(Regex pattern, TextSelection textSelection)
	{
		return Replace(pattern, textSelection, saveFormatting: false);
	}

	public int Replace(Regex pattern, TextSelection textSelection, bool saveFormatting)
	{
		textSelection.CacheRanges();
		int num = 0;
		foreach (WSection section in Sections)
		{
			foreach (WTextBody childEntity in section.ChildEntities)
			{
				num += childEntity.Replace(pattern, textSelection, saveFormatting);
				if (ReplaceFirst && num > 0)
				{
					return num;
				}
			}
		}
		return num;
	}

	public int Replace(string given, TextBodyPart bodyPart, bool caseSensitive, bool wholeWord)
	{
		return Replace(given, bodyPart, caseSensitive, wholeWord, saveFormatting: false);
	}

	public int Replace(string given, TextBodyPart bodyPart, bool caseSensitive, bool wholeWord, bool saveFormatting)
	{
		Regex pattern = FindUtils.StringToRegex(given, caseSensitive, wholeWord);
		return Replace(pattern, bodyPart, saveFormatting);
	}

	public int Replace(Regex pattern, TextBodyPart bodyPart)
	{
		return Replace(pattern, bodyPart, saveFormatting: false);
	}

	public int Replace(Regex pattern, TextBodyPart bodyPart, bool saveFormatting)
	{
		int num = 0;
		foreach (WSection section in Sections)
		{
			foreach (WTextBody childEntity in section.ChildEntities)
			{
				num += childEntity.Replace(pattern, bodyPart, saveFormatting);
				if (ReplaceFirst && num > 0)
				{
					return num;
				}
			}
		}
		return num;
	}

	public int Replace(string given, IWordDocument replaceDoc, bool caseSensitive, bool wholeWord)
	{
		return Replace(given, replaceDoc, caseSensitive, wholeWord, saveFormatting: false);
	}

	public int Replace(string given, IWordDocument replaceDoc, bool caseSensitive, bool wholeWord, bool saveFormatting)
	{
		Regex pattern = FindUtils.StringToRegex(given, caseSensitive, wholeWord);
		return Replace(pattern, replaceDoc, saveFormatting);
	}

	public int Replace(Regex pattern, IWordDocument replaceDoc, bool saveFormatting)
	{
		int num = 0;
		foreach (WSection section in Sections)
		{
			foreach (WTextBody childEntity in section.ChildEntities)
			{
				num += childEntity.Replace(pattern, replaceDoc, saveFormatting);
				if (ReplaceFirst && num > 0)
				{
					return num;
				}
			}
		}
		return num;
	}

	public void UpdateWordCount()
	{
		m_paraCount = (m_wordCount = (m_charCount = 0));
		foreach (WSection section in Sections)
		{
			CalculateForTextBody(section.Body.Items);
		}
		UpdateWordCountForFields();
		BuiltinDocumentProperties.ParagraphCount = m_paraCount;
		BuiltinDocumentProperties.WordCount = m_wordCount;
		BuiltinDocumentProperties.CharCount = m_charCount;
	}

	private void UpdateWordCountForFields()
	{
		int num = 0;
		int num2 = 0;
		if (m_fields != null)
		{
			foreach (WField field in m_fields)
			{
				Entity ownerTextBody = GetOwnerTextBody(field);
				if (ownerTextBody.EntityType == EntityType.HeaderFooter || ownerTextBody.EntityType == EntityType.Footnote)
				{
					continue;
				}
				string text = field.FieldCode;
				string[] array = SplitsText(ref text);
				string[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					if (array2[i] != string.Empty)
					{
						num++;
					}
				}
				if (array[^1] != string.Empty)
				{
					num--;
				}
				text = text.Replace(" ", string.Empty);
				num2 += text.Length;
			}
		}
		if (HasTOC)
		{
			foreach (TableOfContent value in TOC.Values)
			{
				string text2 = "TOC " + value.FormattingString;
				string[] array2 = SplitsText(ref text2);
				for (int i = 0; i < array2.Length; i++)
				{
					if (array2[i] != string.Empty)
					{
						num++;
					}
				}
				text2 = text2.Replace(" ", string.Empty);
				num2 += text2.Length;
			}
		}
		m_wordCount -= num;
		m_charCount -= num2;
	}

	private string[] SplitsText(ref string text)
	{
		char[] array = new char[6] { '\u00a0', '\u200c', '\u200d', '\v', '\t', '\u000e' };
		foreach (char oldChar in array)
		{
			text = text.Replace(oldChar, ' ');
		}
		char[] separator = new char[5] { ' ', '–', '—', '•', '\u0002' };
		return text.Split(separator);
	}

	internal void InternalUpdateWordCount(bool performlayout)
	{
		if (performlayout)
		{
			DocumentLayouter documentLayouter = new DocumentLayouter();
			documentLayouter.UpdatePageFields(this, isUpdateFromWordToPDF: false);
			BuiltinDocumentProperties.PageCount = (PageCount = documentLayouter.Pages.Count);
			documentLayouter.InitLayoutInfo();
			documentLayouter.Close();
		}
		UpdateWordCount();
	}

	public void UpdateDocumentFields()
	{
		UpdateDocumentFields(performLayout: false);
	}

	internal void UpdateDocumentFields(bool performLayout)
	{
		SequenceFieldResults = new Dictionary<string, int>();
		if (performLayout)
		{
			List<WField> list = new List<WField>();
			List<WField> list2 = new List<WField>();
			if (IsContainNumPagesField(list, list2))
			{
				DocumentLayouter documentLayouter = new DocumentLayouter();
				documentLayouter.UpdatePageFields(this, isUpdateFromWordToPDF: false);
				PageCount = documentLayouter.Pages.Count;
				if (list.Count != 0)
				{
					UpdatePageRefFields(list, documentLayouter.BookmarkStartPageNumbers);
				}
				documentLayouter.InitLayoutInfo();
			}
			if (list2.Count > 0)
			{
				UpdateRefFields(list2);
			}
		}
		if (m_fields != null)
		{
			for (int i = 0; i < m_fields.Count; i++)
			{
				m_fields[i].IsUpdated = false;
			}
			List<WSeqField> list3 = new List<WSeqField>();
			for (int j = 0; j < m_fields.Count; j++)
			{
				WField wField = m_fields[j];
				if (!wField.IsUpdated && wField.FieldType != FieldType.FieldRef)
				{
					wField.Update();
					if (wField is WSeqField && !string.IsNullOrEmpty((wField as WSeqField).BookmarkName))
					{
						list3.Add(wField as WSeqField);
					}
					int num = m_fields.InnerList.IndexOf(wField);
					if (j != num)
					{
						j = num;
					}
				}
			}
			UpdateBookmarkSeqField(list3);
			for (int k = 0; k < m_fields.Count; k++)
			{
				WField wField2 = m_fields[k];
				if (!wField2.IsUpdated && wField2.FieldType == FieldType.FieldRef)
				{
					wField2.Update();
				}
				int num2 = m_fields.InnerList.IndexOf(wField2);
				if (k != num2)
				{
					k = num2;
				}
			}
		}
		SequenceFieldResults.Clear();
		SequenceFieldResults = null;
	}

	private void UpdateBookmarkSeqField(List<WSeqField> bookmarkSeqFiled)
	{
		foreach (WSeqField item in bookmarkSeqFiled)
		{
			item.UpdateSequenceFieldResult(GetBookmarkSeqFiledResultNumber(item));
		}
	}

	private string GetBookmarkSeqFiledResultNumber(WSeqField field)
	{
		Bookmark bookmark = Bookmarks.FindByName(field.BookmarkName);
		if (bookmark != null)
		{
			string text = ((bookmark.BookmarkEnd.NextSibling is WSeqField) ? (bookmark.BookmarkEnd.NextSibling as Entity).GetHierarchicalIndex(string.Empty) : bookmark.BookmarkEnd.GetHierarchicalIndex(string.Empty));
			for (int num = m_fields.Count - 1; num >= 0; num--)
			{
				if (m_fields[num].FieldType == FieldType.FieldSequence && !((m_fields[num] as WSeqField).CaptionName != field.CaptionName))
				{
					string hierarchicalIndex = m_fields[num].GetHierarchicalIndex(string.Empty);
					if (hierarchicalIndex == text || field.CompareHierarchicalIndex(hierarchicalIndex, text))
					{
						return m_fields[num].Text;
					}
				}
			}
		}
		return 0.ToString();
	}

	internal DocumentLayouter UpdateDocumentFieldsInOptimalWay()
	{
		if (m_fields != null)
		{
			for (int i = 0; i < m_fields.Count; i++)
			{
				m_fields[i].IsUpdated = false;
			}
			for (int j = 0; j < m_fields.Count; j++)
			{
				if (!m_fields[j].IsUpdated)
				{
					m_fields[j].Update();
				}
			}
		}
		List<WField> list = new List<WField>();
		List<WField> list2 = new List<WField>();
		DocumentLayouter documentLayouter = null;
		if (IsContainNumPagesField(list, list2))
		{
			documentLayouter = new DocumentLayouter();
			documentLayouter.UpdatePageFields(this, isUpdateFromWordToPDF: true);
			PageCount = documentLayouter.Pages.Count;
			if (list.Count != 0)
			{
				UpdatePageRefFields(list, documentLayouter.BookmarkStartPageNumbers);
			}
		}
		if (list2.Count > 0)
		{
			UpdateRefFields(list2);
		}
		return documentLayouter;
	}

	public void UpdateAlternateChunks()
	{
		UpdateAlternateChunk = true;
		bool flag = Sections.Count == 1;
		AlternateChunk alternateChunk = (flag ? GetFirstAltChunkOfSection() : null);
		AlternateChunk alternateChunk2 = (flag ? GetLastAltChunkOfSection() : null);
		WSection wSection = null;
		for (int i = 0; i < Sections.Count; i++)
		{
			WSection wSection2 = Sections[i];
			if (wSection2 == null)
			{
				continue;
			}
			while (wSection2.Body.AlternateChunkCollection.Count > 0)
			{
				AlternateChunk alternateChunk3 = wSection2.Body.AlternateChunkCollection[0];
				if (alternateChunk3 != null && alternateChunk3.GetFormatType(alternateChunk3.ContentExtension))
				{
					if (flag)
					{
						alternateChunk3.IsOwnerDocHavingOneSection = true;
						if (alternateChunk != null && alternateChunk == alternateChunk3)
						{
							alternateChunk3.IsFirstChunk = true;
						}
						if (alternateChunk2 != null && alternateChunk2 == alternateChunk3)
						{
							alternateChunk3.IsLastChunk = true;
						}
						if (alternateChunk3.IsLastChunk && wSection != null)
						{
							alternateChunk3.FirstChunkLastSection = wSection;
						}
					}
					alternateChunk3.Update();
					if (alternateChunk3.FirstChunkLastSection != null)
					{
						wSection = alternateChunk3.FirstChunkLastSection;
					}
				}
				wSection2.Body.AlternateChunkCollection.Remove(alternateChunk3);
			}
		}
		UpdateAlternateChunk = false;
	}

	private AlternateChunk GetFirstAltChunkOfSection()
	{
		WSection wSection = Sections[0];
		if (wSection.Body.AlternateChunkCollection.Count > 0)
		{
			return wSection.Body.AlternateChunkCollection[0];
		}
		return null;
	}

	private AlternateChunk GetLastAltChunkOfSection()
	{
		WSection wSection = Sections[0];
		if (wSection.Body.AlternateChunkCollection.Count > 0)
		{
			return wSection.Body.AlternateChunkCollection[wSection.Body.AlternateChunkCollection.Count - 1];
		}
		return null;
	}

	private void UpdatePageRefFields(List<WField> pagerefFields, Dictionary<Entity, int> bkStartPageNumbers)
	{
		for (int i = 0; i < pagerefFields.Count; i++)
		{
			WField wField = pagerefFields[i];
			bool isHiddenBookmark = false;
			BookmarkStart bookmarkOfCrossRefField = wField.GetBookmarkOfCrossRefField(ref isHiddenBookmark, isReturnHiddenBookmark: true);
			if (bookmarkOfCrossRefField != null && (bkStartPageNumbers.ContainsKey(bookmarkOfCrossRefField) || GetEntityOwnerTextBody(bookmarkOfCrossRefField.OwnerParagraph).Owner is WComment) && wField.FieldType == FieldType.FieldPageRef)
			{
				int num = ((GetEntityOwnerTextBody(bookmarkOfCrossRefField.OwnerParagraph).Owner is WComment) ? 1 : bkStartPageNumbers[bookmarkOfCrossRefField]);
				bool num2 = wField.InternalFieldCode.Contains("\\p");
				string empty = string.Empty;
				if (num2)
				{
					empty = ((!(wField.FieldResult == num.ToString()) || !wField.CompareOwnerTextBody(bookmarkOfCrossRefField)) ? ("on page " + num) : wField.GetPositionValue(bookmarkOfCrossRefField));
					wField.UpdateFieldResult(empty);
				}
				else
				{
					wField.UpdateNumberFormatResult(num.ToString());
				}
			}
		}
	}

	private void UpdateRefFields(List<WField> refFields)
	{
		Dictionary<Entity, string>[] array = new Dictionary<Entity, string>[6];
		Dictionary<Entity, int>[] array2 = new Dictionary<Entity, int>[6];
		Dictionary<Entity, string> dictionary = array[0];
		Dictionary<Entity, int> dictionary2 = array2[0];
		for (int i = 0; i < refFields.Count; i++)
		{
			string[] array3 = refFields[i].InternalFieldCode.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			bool isHiddenBookmark = false;
			BookmarkStart bookmarkOfCrossRefField = refFields[i].GetBookmarkOfCrossRefField(ref isHiddenBookmark, isReturnHiddenBookmark: false);
			if (bookmarkOfCrossRefField == null)
			{
				continue;
			}
			WParagraph ownerParagraph = bookmarkOfCrossRefField.OwnerParagraph;
			WTextBody entityOwnerTextBody = GetEntityOwnerTextBody(ownerParagraph);
			if (entityOwnerTextBody is HeaderFooter)
			{
				UpdateHeaderFooterListValues(array, array2);
				dictionary = array[1];
				dictionary2 = array2[1];
			}
			else if (entityOwnerTextBody.Owner is WTextBox || entityOwnerTextBody.Owner is Shape)
			{
				UpdateShapeListValues(array, array2);
				dictionary = array[2];
				dictionary2 = array2[2];
			}
			else if (entityOwnerTextBody.Owner is WComment)
			{
				UpdateCommentListValues(array, array2);
				dictionary = array[3];
				dictionary2 = array2[3];
			}
			else if (entityOwnerTextBody.Owner is WFootnote && (entityOwnerTextBody.Owner as WFootnote).FootnoteType == FootnoteType.Footnote)
			{
				UpdateFootNoteListValues(array, array2);
				dictionary = array[4];
				dictionary2 = array2[4];
			}
			else if (entityOwnerTextBody.Owner is WFootnote && (entityOwnerTextBody.Owner as WFootnote).FootnoteType == FootnoteType.Endnote)
			{
				UpdateEndNoteListValues(array, array2);
				dictionary = array[5];
				dictionary2 = array2[5];
			}
			else
			{
				UpdateSectionListValues(array, array2);
				dictionary = array[0];
				dictionary2 = array2[0];
			}
			bool flag = false;
			string separator = string.Empty;
			ReferenceKind referenceKind = ReferenceKind.NumberFullContext;
			for (int j = 2; j < array3.Length; j++)
			{
				switch (array3[j].ToLower())
				{
				case "\\p":
					flag = true;
					break;
				case "\\r":
					referenceKind = ReferenceKind.NumberRelativeContext;
					break;
				case "\\n":
					referenceKind = ReferenceKind.NumberNoContext;
					break;
				case "\\w":
					referenceKind = ReferenceKind.NumberFullContext;
					break;
				default:
					separator = array3[j];
					break;
				case "\\h":
				case "\\d":
					break;
				}
			}
			WListFormat listFormatValue = ownerParagraph.GetListFormatValue();
			string text = "0";
			if (listFormatValue != null && listFormatValue.CurrentListStyle != null && dictionary != null && dictionary.ContainsKey(ownerParagraph))
			{
				WListLevel listLevel = ownerParagraph.GetListLevel(listFormatValue);
				switch (referenceKind)
				{
				case ReferenceKind.NumberFullContext:
				case ReferenceKind.NumberRelativeContext:
					text = GetParagraphNumber(refFields[i], bookmarkOfCrossRefField, ownerParagraph, listLevel, separator, referenceKind, dictionary, dictionary2);
					break;
				case ReferenceKind.NumberNoContext:
					text = dictionary[ownerParagraph];
					break;
				}
			}
			text = text.TrimEnd('.');
			if (flag && refFields[i].CompareOwnerTextBody(bookmarkOfCrossRefField))
			{
				refFields[i].UpdateFieldResult(text + " " + refFields[i].GetPositionValue(bookmarkOfCrossRefField));
			}
			else
			{
				refFields[i].UpdateFieldResult(text);
			}
		}
		ClearLists();
	}

	private void UpdateListValue(WTextBody textbody, int index, Dictionary<Entity, string>[] listValueCollections, Dictionary<Entity, int>[] levelNumberCollections)
	{
		foreach (TextBodyItem childEntity in textbody.ChildEntities)
		{
			if (childEntity is WParagraph)
			{
				WListFormat listFormatValue = (childEntity as WParagraph).GetListFormatValue();
				if (listFormatValue != null && listFormatValue.CurrentListStyle != null)
				{
					WListLevel listLevel = (childEntity as WParagraph).GetListLevel(listFormatValue);
					listValueCollections[index].Add(childEntity, UpdateListValue(childEntity as WParagraph, listFormatValue, listLevel));
					levelNumberCollections[index].Add(childEntity, listLevel.LevelNumber);
				}
			}
			else if (childEntity is WTable)
			{
				foreach (WTableRow childEntity2 in (childEntity as WTable).ChildEntities)
				{
					foreach (WTableCell childEntity3 in childEntity2.ChildEntities)
					{
						UpdateListValue(childEntity3, index, listValueCollections, levelNumberCollections);
					}
				}
			}
			else if (childEntity is BlockContentControl)
			{
				BlockContentControl blockContentControl = childEntity as BlockContentControl;
				UpdateListValue(blockContentControl.TextBody, index, listValueCollections, levelNumberCollections);
			}
		}
	}

	private void UpdateHeaderFooterListValues(Dictionary<Entity, string>[] listValueCollections, Dictionary<Entity, int>[] levelNumberCollections)
	{
		if (listValueCollections[1] != null)
		{
			return;
		}
		listValueCollections[1] = new Dictionary<Entity, string>();
		levelNumberCollections[1] = new Dictionary<Entity, int>();
		ClearLists();
		foreach (WSection section in Sections)
		{
			for (int i = 0; i < section.ChildEntities.Count; i++)
			{
				if (section.ChildEntities[i].EntityType == EntityType.HeaderFooter)
				{
					UpdateListValue(section.ChildEntities[i] as WTextBody, 1, listValueCollections, levelNumberCollections);
				}
			}
		}
	}

	private void UpdateShapeListValues(Dictionary<Entity, string>[] listValueCollections, Dictionary<Entity, int>[] levelNumberCollections)
	{
		if (listValueCollections[2] != null)
		{
			return;
		}
		ClearLists();
		listValueCollections[2] = new Dictionary<Entity, string>();
		levelNumberCollections[2] = new Dictionary<Entity, int>();
		foreach (WSection section in Sections)
		{
			foreach (WParagraph paragraph in section.Paragraphs)
			{
				foreach (ParagraphItem childEntity in paragraph.ChildEntities)
				{
					switch (childEntity.EntityType)
					{
					case EntityType.TextBox:
						UpdateListValue((childEntity as WTextBox).TextBoxBody, 2, listValueCollections, levelNumberCollections);
						break;
					case EntityType.Shape:
					case EntityType.AutoShape:
						UpdateListValue((childEntity as Shape).TextBody, 2, listValueCollections, levelNumberCollections);
						break;
					}
				}
			}
		}
	}

	private void UpdateEndNoteListValues(Dictionary<Entity, string>[] listValueCollections, Dictionary<Entity, int>[] levelNumberCollections)
	{
		if (listValueCollections[5] != null)
		{
			return;
		}
		ClearLists();
		listValueCollections[5] = new Dictionary<Entity, string>();
		levelNumberCollections[5] = new Dictionary<Entity, int>();
		foreach (WSection section in Sections)
		{
			foreach (WParagraph paragraph in section.Paragraphs)
			{
				foreach (ParagraphItem childEntity in paragraph.ChildEntities)
				{
					if (childEntity is WFootnote && (childEntity as WFootnote).FootnoteType == FootnoteType.Endnote)
					{
						UpdateListValue((childEntity as WFootnote).TextBody, 5, listValueCollections, levelNumberCollections);
					}
				}
			}
		}
	}

	private void UpdateFootNoteListValues(Dictionary<Entity, string>[] listValueCollections, Dictionary<Entity, int>[] levelNumberCollections)
	{
		if (listValueCollections[4] != null)
		{
			return;
		}
		ClearLists();
		listValueCollections[4] = new Dictionary<Entity, string>();
		levelNumberCollections[4] = new Dictionary<Entity, int>();
		foreach (WSection section in Sections)
		{
			foreach (WParagraph paragraph in section.Paragraphs)
			{
				foreach (ParagraphItem childEntity in paragraph.ChildEntities)
				{
					if (childEntity is WFootnote && (childEntity as WFootnote).FootnoteType == FootnoteType.Footnote)
					{
						UpdateListValue((childEntity as WFootnote).TextBody, 4, listValueCollections, levelNumberCollections);
					}
				}
			}
		}
	}

	private void UpdateCommentListValues(Dictionary<Entity, string>[] listValueCollections, Dictionary<Entity, int>[] levelNumberCollections)
	{
		if (listValueCollections[3] != null)
		{
			return;
		}
		ClearLists();
		listValueCollections[3] = new Dictionary<Entity, string>();
		levelNumberCollections[3] = new Dictionary<Entity, int>();
		foreach (WSection section in Sections)
		{
			foreach (WParagraph paragraph in section.Paragraphs)
			{
				foreach (ParagraphItem childEntity in paragraph.ChildEntities)
				{
					if (childEntity.EntityType == EntityType.Comment)
					{
						UpdateListValue((childEntity as WComment).TextBody, 3, listValueCollections, levelNumberCollections);
					}
				}
			}
		}
	}

	private void UpdateSectionListValues(Dictionary<Entity, string>[] listValueCollections, Dictionary<Entity, int>[] levelNumberCollections)
	{
		if (listValueCollections[0] != null)
		{
			return;
		}
		listValueCollections[0] = new Dictionary<Entity, string>();
		levelNumberCollections[0] = new Dictionary<Entity, int>();
		ClearLists();
		foreach (WSection section in Sections)
		{
			for (int i = 0; i < section.ChildEntities.Count; i++)
			{
				if (section.ChildEntities[i].EntityType == EntityType.TextBody)
				{
					UpdateListValue(section.ChildEntities[i] as WTextBody, 0, listValueCollections, levelNumberCollections);
				}
			}
		}
	}

	private string GetParagraphNumber(WField refFields, BookmarkStart bkStart, WParagraph ownerPara, WListLevel level, string separator, ReferenceKind referencekind, Dictionary<Entity, string> paragraphListValue, Dictionary<Entity, int> paragraphLevelNumbers)
	{
		string text = "0";
		bool flag = false;
		List<Entity> list = new List<Entity>();
		foreach (Entity key in paragraphListValue.Keys)
		{
			list.Add(key);
		}
		int num = list.IndexOf(ownerPara);
		if (level.PatternType == ListPatternType.Bullet)
		{
			text = paragraphListValue[ownerPara];
		}
		else if (referencekind == ReferenceKind.NumberRelativeContext && refFields.CompareOwnerTextBody(bkStart))
		{
			string positionValue = refFields.GetPositionValue(bkStart);
			if (ownerPara == refFields.OwnerParagraph)
			{
				text = paragraphListValue[ownerPara];
			}
			else if (!paragraphListValue.ContainsKey(refFields.OwnerParagraph) && positionValue == "below")
			{
				flag = true;
			}
			else
			{
				int num2 = paragraphLevelNumbers[ownerPara];
				int num3 = -1;
				int num4 = -1;
				for (int i = num + 1; i < list.Count; i++)
				{
					if (paragraphLevelNumbers[list[i]] >= num2)
					{
						continue;
					}
					if (paragraphLevelNumbers[list[i]] == 0)
					{
						if (paragraphListValue[list[i]] != "1.")
						{
							num3 = paragraphLevelNumbers[list[i]];
						}
						num4 = i;
						break;
					}
					num3 = paragraphLevelNumbers[list[i]];
					num4 = i;
				}
				if (num3 == -1 && refFields.GetPositionValue(bkStart) == "above")
				{
					text = paragraphListValue[ownerPara];
				}
				else if (paragraphListValue.ContainsKey(refFields.OwnerParagraph) && positionValue == "below" && list.IndexOf(refFields.OwnerParagraph) < num)
				{
					text = paragraphListValue[ownerPara];
					num2 = paragraphLevelNumbers[ownerPara];
					for (int num5 = num - 1; num5 >= 0; num5--)
					{
						if (paragraphLevelNumbers[list[num5]] == num2 - 1)
						{
							if (list[num5] == refFields.OwnerParagraph)
							{
								break;
							}
							text = paragraphListValue[list[num5]] + ((separator != string.Empty) ? separator : string.Empty) + text;
							num2 = paragraphLevelNumbers[list[num5]];
						}
					}
				}
				else if (paragraphListValue.ContainsKey(refFields.OwnerParagraph) && positionValue == "above" && list.IndexOf(refFields.OwnerParagraph) > num && list.IndexOf(refFields.OwnerParagraph) < num4)
				{
					text = paragraphListValue[ownerPara];
					num2 = paragraphLevelNumbers[ownerPara];
					for (int num6 = num - 1; num6 >= 0; num6--)
					{
						if (paragraphLevelNumbers[list[num6]] == num2 - 1)
						{
							text = paragraphListValue[list[num6]] + ((separator != string.Empty) ? separator : string.Empty) + text;
							num2 = paragraphLevelNumbers[list[num6]];
							if (paragraphLevelNumbers[refFields.OwnerParagraph] == paragraphLevelNumbers[list[num6]])
							{
								break;
							}
						}
					}
				}
				else if (num3 == 0 && positionValue == "above" && paragraphListValue.ContainsKey(refFields.OwnerParagraph) && list.IndexOf(refFields.OwnerParagraph) < num4)
				{
					text = paragraphListValue[ownerPara];
				}
				else
				{
					text = paragraphListValue[ownerPara];
					num2 = paragraphLevelNumbers[ownerPara];
					for (int num7 = num - 1; num7 >= 0; num7--)
					{
						if (paragraphLevelNumbers[list[num7]] == num2 - 1)
						{
							text = paragraphListValue[list[num7]] + ((separator != string.Empty) ? separator : string.Empty) + text;
							num2 = paragraphLevelNumbers[list[num7]];
							if (num3 == num2)
							{
								break;
							}
						}
					}
				}
			}
		}
		else
		{
			flag = true;
		}
		if (level.PatternType != ListPatternType.Bullet && ((referencekind == ReferenceKind.NumberRelativeContext && flag) || referencekind == ReferenceKind.NumberFullContext))
		{
			text = paragraphListValue[ownerPara];
			int num8 = paragraphLevelNumbers[ownerPara];
			if (referencekind == ReferenceKind.NumberRelativeContext)
			{
				separator = string.Empty;
			}
			for (int num9 = num - 1; num9 >= 0; num9--)
			{
				if (paragraphLevelNumbers[list[num9]] == num8 - 1)
				{
					text = paragraphListValue[list[num9]] + ((separator != string.Empty) ? separator : string.Empty) + text;
					num8 = paragraphLevelNumbers[list[num9]];
				}
			}
		}
		return text;
	}

	internal WTextBody GetEntityOwnerTextBody(WParagraph para)
	{
		WTextBody ownerTextBody = para.OwnerTextBody;
		while (ownerTextBody is WTableCell)
		{
			ownerTextBody = (ownerTextBody as WTableCell).OwnerRow.OwnerTable.OwnerTextBody;
			if (ownerTextBody != null && ownerTextBody.Owner is BlockContentControl)
			{
				ownerTextBody = (ownerTextBody.Owner as BlockContentControl).OwnerTextBody;
			}
		}
		if (ownerTextBody != null && ownerTextBody.Owner is BlockContentControl)
		{
			ownerTextBody = (ownerTextBody.Owner as BlockContentControl).OwnerTextBody;
		}
		return ownerTextBody;
	}

	private bool IsContainNumPagesField(List<WField> pagereffields, List<WField> refFields)
	{
		bool result = false;
		if (m_fields != null && m_fields.Count > 0)
		{
			for (int i = 0; i < m_fields.Count; i++)
			{
				if (m_fields[i].FieldType == FieldType.FieldNumPages || m_fields[i].FieldType == FieldType.FieldPage)
				{
					result = true;
				}
				else
				{
					if (m_fields[i].FieldType != FieldType.FieldPageRef && m_fields[i].FieldType != FieldType.FieldRef)
					{
						continue;
					}
					bool isHiddenBookmark = false;
					if (m_fields[i].GetBookmarkOfCrossRefField(ref isHiddenBookmark, isReturnHiddenBookmark: true) != null)
					{
						if (m_fields[i].FieldType == FieldType.FieldRef && (m_fields[i].InternalFieldCode.Contains("\\n") || m_fields[i].InternalFieldCode.Contains("\\w") || m_fields[i].InternalFieldCode.Contains("\\r")))
						{
							refFields.Add(m_fields[i]);
						}
						else if (m_fields[i].FieldType == FieldType.FieldPageRef)
						{
							pagereffields.Add(m_fields[i]);
							result = true;
						}
					}
					else if (!isHiddenBookmark && m_fields[i].FieldType == FieldType.FieldPageRef)
					{
						m_fields[i].UpdateFieldResult("Error! No bookmark name given.");
					}
				}
			}
		}
		return result;
	}

	private void CalculateForTextBody(BodyItemCollection bodyItems)
	{
		foreach (TextBodyItem bodyItem in bodyItems)
		{
			if (bodyItem is WParagraph)
			{
				WParagraph para = bodyItem as WParagraph;
				CalculateForParagraphs(para);
			}
			else if (bodyItem is WTable)
			{
				CalculateForTabls(bodyItem as WTable);
			}
			else if (bodyItem is BlockContentControl && (bodyItem as BlockContentControl).TextBody != null)
			{
				CalculateForTextBody((bodyItem as BlockContentControl).TextBody.Items);
			}
		}
	}

	private string CalculateForParaItems(ParagraphItemCollection paragraphItemCollection)
	{
		string text = string.Empty;
		foreach (ParagraphItem item in paragraphItemCollection)
		{
			switch (item.EntityType)
			{
			case EntityType.TextRange:
			{
				WTextRange wTextRange = item as WTextRange;
				if (!wTextRange.CharacterFormat.Hidden)
				{
					text += wTextRange.Text;
				}
				break;
			}
			case EntityType.Break:
			{
				Break @break = item as Break;
				switch (@break.BreakType)
				{
				case BreakType.LineBreak:
				case BreakType.TextWrappingBreak:
					text += @break.TextRange.Text;
					break;
				case BreakType.ColumnBreak:
					text += '\u000e';
					break;
				}
				break;
			}
			case EntityType.InlineContentControl:
				text += CalculateForParaItems((item as InlineContentControl).ParagraphItems);
				break;
			case EntityType.Footnote:
				text += '\u0002';
				break;
			case EntityType.Symbol:
			{
				WSymbol wSymbol = item as WSymbol;
				text += (char)Convert.ToInt32(wSymbol.CharValue, 16);
				break;
			}
			case EntityType.TextBox:
			{
				WTextBox wTextBox = item as WTextBox;
				if (wTextBox.TextBoxBody != null && wTextBox.TextBoxBody.Items.Count > 0)
				{
					CalculateForTextBody(wTextBox.TextBoxBody.Items);
				}
				break;
			}
			case EntityType.AutoShape:
			{
				Shape shape = item as Shape;
				if (shape.TextBody != null && shape.TextBody.Items.Count > 0)
				{
					CalculateForTextBody(shape.TextBody.Items);
				}
				break;
			}
			case EntityType.GroupShape:
				foreach (ChildShape childShape in (item as GroupShape).ChildShapes)
				{
					if (childShape.TextBody != null && childShape.TextBody.Items.Count > 0)
					{
						CalculateForTextBody(childShape.TextBody.Items);
					}
				}
				break;
			}
		}
		return text;
	}

	private void CalculateForTabls(WTable table)
	{
		foreach (WTableRow row in table.Rows)
		{
			foreach (WTableCell cell in row.Cells)
			{
				CalculateForTextBody(cell.Items);
			}
		}
	}

	private void CalculateForParagraphs(WParagraph para)
	{
		string text = CalculateForParaItems(para.Items);
		WListFormat listFormatValue = para.GetListFormatValue();
		if (listFormatValue != null && listFormatValue.CurrentListStyle != null && listFormatValue.ListType != ListType.NoList)
		{
			bool isPicBullet = false;
			string listText = para.GetListText(isFromTextConverter: true, ref isPicBullet);
			WListLevel listLevel = para.GetListLevel(listFormatValue);
			if (listLevel != null && listLevel.PatternType == ListPatternType.Bullet && !isPicBullet)
			{
				string text2 = listLevel.BulletCharacter.Replace('\uf0b7', ' ');
				int num = text2.Length - listLevel.BulletCharacter.Replace('\uf0b7'.ToString(), string.Empty).Length;
				text = text2 + " " + text;
				m_charCount += num;
			}
			else if (!isPicBullet)
			{
				text = listText + text;
			}
		}
		int wordCount = m_wordCount;
		if (!(para.Text != string.Empty))
		{
			return;
		}
		string[] array = SplitsText(ref text);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != string.Empty)
			{
				m_wordCount++;
			}
		}
		if (m_wordCount - wordCount > 0)
		{
			m_paraCount++;
		}
		if (text.StartsWith("--"))
		{
			for (int j = 2; j < text.Length; j++)
			{
				if (text[j] != '-')
				{
					if (text[j] != ' ')
					{
						m_wordCount++;
					}
					break;
				}
			}
		}
		array = new string[2]
		{
			" ",
			'\u001f'.ToString()
		};
		foreach (string oldValue in array)
		{
			text = text.Replace(oldValue, string.Empty);
		}
		m_charCount += text.Length;
	}

	internal void UpdateTableOfContent()
	{
		if (!HasTOC)
		{
			return;
		}
		List<ParagraphItem> tocParaItems = null;
		TableOfContent tableOfContent = null;
		Dictionary<Entity, int> dictionary = null;
		bool useTCFields = false;
		bool flag = false;
		if (Styles != null && Styles.FindByName("Hyperlink", StyleType.CharacterStyle) == null)
		{
			AddStyle(BuiltinStyle.Hyperlink);
		}
		foreach (TableOfContent value in TOC.Values)
		{
			List<string> tocLinkCharacterStyleNames = value.UpdateTOCStyleLevels();
			TabCollection tabs = value.OwnerParagraph.ParagraphFormat.Tabs;
			if (tabs.Count > 0)
			{
				value.m_tabLeader = tabs[tabs.Count - 1].TabLeader;
			}
			value.RemoveUpdatedTocEntries();
			tocParaItems = value.ParseDocument(tocLinkCharacterStyleNames);
			if (!flag && value.IncludePageNumbers)
			{
				flag = true;
			}
			if (m_tocEntryLastEntity == null)
			{
				m_tocEntryLastEntity = value.m_tocEntryLastEntity;
			}
			tableOfContent = value;
		}
		if (tableOfContent != null)
		{
			_ = tableOfContent.TOCLevels;
			useTCFields = tableOfContent.UseTableEntryFields;
		}
		if (flag && m_tocEntryLastEntity != null)
		{
			dictionary = new DocumentLayouter
			{
				UseTCFields = useTCFields,
				tocParaItems = tocParaItems,
				LastTocEntity = m_tocEntryLastEntity
			}.GetTOCEntryPageNumbers(base.Document);
			if (dictionary != null)
			{
				foreach (TableOfContent value2 in TOC.Values)
				{
					if (value2.IncludePageNumbers)
					{
						value2.UpdatePageNumbers(dictionary);
					}
				}
			}
		}
		ClearLists();
	}

	internal string UpdateListValue(WParagraph paragraph, WListFormat listFormat, WListLevel level)
	{
		if (paragraph.BreakCharacterFormat.IsDeleteRevision)
		{
			return string.Empty;
		}
		string text = listFormat.CustomStyleName;
		if (paragraph.Owner != null && paragraph.Owner.Owner is WTextBox)
		{
			text += "_textbox";
		}
		bool isIncreseStartVal = false;
		if (PreviousListLevel.ContainsKey(text) && PreviousListLevel[text] > level.LevelNumber)
		{
			isIncreseStartVal = true;
		}
		ListOverrideStyle listOverrideStyle = null;
		if (listFormat.LFOStyleName != null && listFormat.LFOStyleName.Length > 0)
		{
			listOverrideStyle = ListOverrides.FindByName(listFormat.LFOStyleName);
		}
		if (listOverrideStyle != null && listOverrideStyle.OverrideLevels.HasOverrideLevel(level.LevelNumber) && listOverrideStyle.OverrideLevels[level.LevelNumber].OverrideStartAtValue && !PreviousListLevelOverrideStyle.Contains(listOverrideStyle.Name))
		{
			EnsureLevelRestart(listFormat, text, fullRestart: true, level);
			PreviousListLevelOverrideStyle.Add(listOverrideStyle.Name);
			isIncreseStartVal = false;
		}
		else if (paragraph.ListFormat.RestartNumbering)
		{
			EnsureLevelRestart(listFormat, text, fullRestart: true, level);
		}
		else if (PreviousListLevel.ContainsKey(text) && level.LevelNumber > PreviousListLevel[text])
		{
			EnsureLevelRestart(listFormat, text, fullRestart: false, level);
		}
		string empty = string.Empty;
		int num = 0;
		int listItemIndex = GetListItemIndex(listFormat, text, level, isIncreseStartVal);
		string numberPrefix = level.NumberPrefix;
		level.NumberPrefix = UpdateNumberPrefix(level.NumberPrefix, listFormat, text);
		string numberSuffix = level.NumberSuffix;
		string text2 = null;
		if (level.NumberSuffix != null && level.NumberSuffix.Contains("%"))
		{
			text2 = level.CheckNumberPrefOrSuf(level.NumberSuffix);
			level.NumberSuffix = UpdateNumberPrefix(text2, listFormat, text);
		}
		empty = level.GetListItemText(listItemIndex, listFormat.ListType, paragraph);
		level.NumberPrefix = numberPrefix;
		num = GetListStartValue(listFormat, text, level);
		if (level.NumberPrefix != null && level.NumberPrefix.Contains("\0"))
		{
			empty = GetListValue(listFormat, text, level, num, listItemIndex);
		}
		if (text2 != null && text2.Contains("\0"))
		{
			empty = empty.Remove(empty.IndexOf("\0", StringComparison.Ordinal));
			empty += GetUpdatedSuffix(listFormat, text, level, listItemIndex);
		}
		level.NumberSuffix = numberSuffix;
		if (level.PatternType == ListPatternType.Bullet)
		{
			empty = level.BulletCharacter;
		}
		else if (string.IsNullOrEmpty(empty) && !string.IsNullOrEmpty(level.LevelText))
		{
			empty = GetListLevelText(level.LevelText);
		}
		if (PreviousListLevel.ContainsKey(text))
		{
			PreviousListLevel[text] = level.LevelNumber;
		}
		else
		{
			PreviousListLevel.Add(text, level.LevelNumber);
		}
		return empty;
	}

	private string GetUpdatedSuffix(WListFormat listFormat, string styleName, WListLevel level, int listItemIndex)
	{
		string result = string.Empty;
		int levelNumber = level.LevelNumber;
		string text = null;
		string[] array = new string[9] { "\0", "\u0001", "\u0002", "\u0003", "\u0004", "\u0005", "\u0006", "\a", "\b" };
		for (int i = 0; i <= levelNumber; i++)
		{
			if (level.NumberSuffix.Contains(array[i]))
			{
				text += i;
			}
		}
		if (Lists.ContainsKey(styleName))
		{
			string text2 = string.Empty;
			Dictionary<int, int> dictionary = Lists[styleName];
			int[] array2 = new int[dictionary.Count];
			dictionary.Keys.CopyTo(array2, 0);
			array2 = SortKeys(array2);
			int num = array2[0];
			WListLevel wListLevel = null;
			bool leadZero = listItemIndex < 9;
			string orderedNumberPrefix = GetOrderedNumberPrefix(level.NumberSuffix);
			for (int j = num; (num == levelNumber) ? (j <= levelNumber) : (j < levelNumber); j++)
			{
				if (text.Contains(j.ToString()) && dictionary.ContainsKey(j))
				{
					wListLevel = listFormat.CurrentListStyle.Levels[j];
					string listPrefixByItsLevel = GetListPrefixByItsLevel(j + 1, orderedNumberPrefix);
					text2 = ((!level.IsLegalStyleNumbering || wListLevel.PatternType == ListPatternType.None) ? (text2 + GetNumberedListValue(wListLevel, dictionary[j] - 1, leadZero, listPrefixByItsLevel)) : (text2 + Convert.ToString(dictionary[j] - 1) + listPrefixByItsLevel));
				}
			}
			result = text2;
		}
		return result;
	}

	private string UpdateNumberPrefix(string numberPrefix, WListFormat listFormat, string styleName)
	{
		if (string.IsNullOrEmpty(numberPrefix) || numberPrefix.Contains("\0"))
		{
			return numberPrefix;
		}
		string[] obj = new string[9] { "\0", "\u0001", "\u0002", "\u0003", "\u0004", "\u0005", "\u0006", "\a", "\b" };
		bool flag = false;
		string[] array = obj;
		foreach (string value in array)
		{
			if (numberPrefix.Contains(value))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			numberPrefix = DocxSerializator.UpdateNumberPrefOrSuf(numberPrefix);
			string text = string.Empty;
			char c = '\0';
			string text2 = numberPrefix;
			for (int i = 0; i < text2.Length; i++)
			{
				char c2 = text2[i];
				if (c == '%')
				{
					bool leadZero = false;
					int num = int.Parse(c2.ToString());
					WListLevel wListLevel = listFormat.CurrentListStyle.Levels[num - 1];
					int num2 = wListLevel.StartAt;
					Dictionary<int, int> dictionary = null;
					if (ListNames.ContainsKey(styleName))
					{
						dictionary = ListNames[styleName];
					}
					if (dictionary.ContainsKey(wListLevel.LevelNumber))
					{
						num2 = dictionary[wListLevel.LevelNumber] - 1;
					}
					if (wListLevel.PatternType == ListPatternType.LeadingZero && wListLevel.StartAt < 10)
					{
						leadZero = true;
					}
					text += GetNumberedListValue(wListLevel, num2, leadZero, "");
				}
				else if (c2 != '%')
				{
					text += c2;
				}
				c = c2;
			}
			return text;
		}
		return numberPrefix;
	}

	private string GetListLevelText(string levelText)
	{
		if (!levelText.Contains("%"))
		{
			return levelText;
		}
		string text = string.Empty;
		bool flag = false;
		for (int i = 0; i < levelText.Length; i++)
		{
			char c = levelText[i];
			if (flag)
			{
				flag = false;
				int result = int.MinValue;
				if (int.TryParse(c.ToString(), out result))
				{
					if (result > 1)
					{
						text += result - 1;
					}
					else if (result == 0)
					{
						return null;
					}
				}
				else
				{
					text = text + "%" + c;
				}
			}
			else if (c == '%')
			{
				flag = true;
			}
			else
			{
				text += c;
			}
		}
		return text;
	}

	internal void ClearLists()
	{
		PreviousListLevel.Clear();
		PreviousListLevelOverrideStyle.Clear();
		Lists.Clear();
		ListNames.Clear();
	}

	private void EnsureLevelRestart(WListFormat format, string styleName, bool fullRestart, WListLevel listLevel)
	{
		if (m_listNames == null)
		{
			return;
		}
		ListOverrideStyle listOverrideStyle = null;
		if (format.LFOStyleName != null && format.LFOStyleName.Length > 0)
		{
			listOverrideStyle = ListOverrides.FindByName(format.LFOStyleName);
		}
		Dictionary<int, int> dictionary = null;
		if (ListNames.ContainsKey(styleName))
		{
			dictionary = ListNames[styleName];
		}
		if (dictionary == null)
		{
			return;
		}
		Dictionary<int, int>.KeyCollection keys = dictionary.Keys;
		IEnumerator enumerator = ((IEnumerable)keys).GetEnumerator();
		int count = ((ICollection)keys).Count;
		int[] array = new int[count];
		int num = 0;
		while (enumerator.MoveNext())
		{
			array[num] = (int)enumerator.Current;
			num++;
		}
		bool flag = false;
		for (int i = 0; i < count; i++)
		{
			if (!fullRestart && (array[i] < listLevel.LevelNumber || format.CurrentListStyle.Levels[array[i]].NoRestartByHigher))
			{
				continue;
			}
			int startAt = format.CurrentListStyle.Levels[listLevel.LevelNumber].StartAt;
			if (listOverrideStyle != null && listOverrideStyle.OverrideLevels.HasOverrideLevel(listLevel.LevelNumber) && Lists.ContainsKey(styleName))
			{
				startAt = listLevel.StartAt;
				if (listOverrideStyle.OverrideLevels[listLevel.LevelNumber].OverrideStartAtValue)
				{
					startAt = listOverrideStyle.OverrideLevels[format.ListLevelNumber].StartAt;
					if (startAt != 1)
					{
						flag = true;
					}
				}
				Dictionary<int, int> dictionary2 = Lists[styleName];
				for (int j = listLevel.LevelNumber; dictionary2.ContainsKey(j); j++)
				{
					dictionary2[j] = 1;
				}
			}
			if (listLevel.LevelNumber == array[i])
			{
				dictionary[array[i]] = startAt;
			}
		}
		if (flag)
		{
			Dictionary<int, int> dictionary3 = Lists[styleName];
			if (dictionary3.ContainsKey(listLevel.LevelNumber) && count > listLevel.LevelNumber)
			{
				dictionary3[listLevel.LevelNumber] = dictionary[array[listLevel.LevelNumber]];
			}
		}
	}

	private int GetListItemIndex(WListFormat format, string styleName, WListLevel listLevel, bool isIncreseStartVal)
	{
		ListOverrideStyle listOverrideStyle = null;
		if (format.LFOStyleName != null && format.LFOStyleName.Length > 0)
		{
			listOverrideStyle = ListOverrides.FindByName(format.LFOStyleName);
		}
		int num = 0;
		Dictionary<int, int> dictionary = null;
		if (ListNames.ContainsKey(styleName))
		{
			dictionary = ListNames[styleName];
		}
		if (dictionary == null)
		{
			Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
			ListNames.Add(styleName, dictionary2);
			int index = listLevel.LevelNumber;
			if (listLevel.LevelText != null && listLevel.LevelText.Length > 1 && listLevel.LevelText.IndexOf("%") == 0 && listLevel.LevelText.IndexOf('.') == 2 && listLevel.LevelNumber == int.Parse(listLevel.LevelText[1].ToString()))
			{
				index = int.Parse(listLevel.LevelText[1].ToString()) - 1;
			}
			num = format.CurrentListStyle.Levels[index].StartAt;
			if (listOverrideStyle != null && listOverrideStyle.OverrideLevels.HasOverrideLevel(listLevel.LevelNumber) && listOverrideStyle.OverrideLevels[listLevel.LevelNumber].OverrideStartAtValue)
			{
				num = listOverrideStyle.OverrideLevels[listLevel.LevelNumber].StartAt;
			}
			dictionary2.Add(listLevel.LevelNumber, num + 1);
			return num - 1;
		}
		if (dictionary.ContainsKey(listLevel.LevelNumber))
		{
			num = dictionary[listLevel.LevelNumber];
			dictionary[listLevel.LevelNumber] = num + 1;
			return num - 1;
		}
		num = format.CurrentListStyle.Levels[listLevel.LevelNumber].StartAt;
		if (listOverrideStyle != null && listOverrideStyle.OverrideLevels.HasOverrideLevel(listLevel.LevelNumber) && listOverrideStyle.OverrideLevels[listLevel.LevelNumber].OverrideStartAtValue)
		{
			num = listOverrideStyle.OverrideLevels[listLevel.LevelNumber].StartAt;
		}
		if (isIncreseStartVal)
		{
			num++;
		}
		dictionary.Add(listLevel.LevelNumber, num + 1);
		return num - 1;
	}

	private int GetListStartValue(WListFormat format, string styleName, WListLevel listLevel)
	{
		if (listLevel != null && listLevel.PatternType == ListPatternType.Bullet)
		{
			return 1;
		}
		if (!Lists.ContainsKey(styleName))
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			Lists.Add(styleName, dictionary);
			if (format.CurrentListStyle != null)
			{
				WListLevel wListLevel = format.CurrentListStyle.Levels[listLevel.LevelNumber];
				ListOverrideStyle listOverrideStyle = null;
				if (format.LFOStyleName != null && format.LFOStyleName.Length > 0)
				{
					listOverrideStyle = ListOverrides.FindByName(format.LFOStyleName);
				}
				int startAt = format.CurrentListStyle.Levels[wListLevel.LevelNumber].StartAt;
				for (int i = 0; i <= wListLevel.LevelNumber; i++)
				{
					if (listOverrideStyle != null && listOverrideStyle.OverrideLevels.HasOverrideLevel(i) && Lists.ContainsKey(styleName))
					{
						if (listOverrideStyle.OverrideLevels[i].OverrideStartAtValue)
						{
							if (listOverrideStyle.OverrideLevels[i].OverrideFormatting && listOverrideStyle.OverrideLevels[i].OverrideListLevel.PatternType != ListPatternType.Bullet)
							{
								dictionary.Add(i, listOverrideStyle.OverrideLevels[i].OverrideListLevel.StartAt + 1);
								if (i == listLevel.LevelNumber)
								{
									startAt = listOverrideStyle.OverrideLevels[i].OverrideListLevel.StartAt;
								}
							}
							else
							{
								dictionary.Add(i, listOverrideStyle.OverrideLevels[i].StartAt + 1);
								if (i == listLevel.LevelNumber)
								{
									startAt = listOverrideStyle.OverrideLevels[i].StartAt;
								}
							}
						}
						else if (listOverrideStyle.OverrideLevels[i].OverrideListLevel.PatternType != ListPatternType.Bullet)
						{
							dictionary.Add(i, format.CurrentListStyle.Levels[i].StartAt + 1);
						}
					}
					else if (format.CurrentListStyle.Levels[i].PatternType != ListPatternType.Bullet)
					{
						dictionary.Add(i, format.CurrentListStyle.Levels[i].StartAt + 1);
					}
				}
				return startAt;
			}
			return 1;
		}
		Dictionary<int, int> dictionary2 = Lists[styleName];
		if (dictionary2.ContainsKey(listLevel.LevelNumber))
		{
			int num = dictionary2[listLevel.LevelNumber];
			dictionary2[listLevel.LevelNumber] = num + 1;
			for (int j = listLevel.LevelNumber; dictionary2.ContainsKey(j + 1); j++)
			{
				dictionary2[j + 1] = 1;
			}
			ResetInbetweenLevels(format, styleName, listLevel, dictionary2);
			return num;
		}
		WListLevel wListLevel2 = format.CurrentListStyle.Levels[listLevel.LevelNumber];
		ListOverrideStyle listOverrideStyle2 = null;
		if (format.LFOStyleName != null && format.LFOStyleName.Length > 0)
		{
			listOverrideStyle2 = ListOverrides.FindByName(format.LFOStyleName);
		}
		int startAt2 = wListLevel2.StartAt;
		for (int k = 0; k <= wListLevel2.LevelNumber; k++)
		{
			if (listOverrideStyle2 != null && listOverrideStyle2.OverrideLevels.HasOverrideLevel(k) && Lists.ContainsKey(styleName))
			{
				if (listOverrideStyle2.OverrideLevels[k].OverrideStartAtValue)
				{
					if (listOverrideStyle2.OverrideLevels[k].OverrideFormatting && listOverrideStyle2.OverrideLevels[k].OverrideListLevel.PatternType != ListPatternType.Bullet && !dictionary2.ContainsKey(k))
					{
						dictionary2.Add(k, listOverrideStyle2.OverrideLevels[k].OverrideListLevel.StartAt + 1);
						if (k == listLevel.LevelNumber)
						{
							startAt2 = listOverrideStyle2.OverrideLevels[k].OverrideListLevel.StartAt;
						}
					}
					else if (!dictionary2.ContainsKey(k))
					{
						dictionary2.Add(k, listOverrideStyle2.OverrideLevels[k].StartAt + 1);
						if (k == listLevel.LevelNumber)
						{
							startAt2 = listOverrideStyle2.OverrideLevels[k].StartAt;
						}
					}
				}
				else if (listOverrideStyle2.OverrideLevels[k].OverrideListLevel.PatternType != ListPatternType.Bullet && !dictionary2.ContainsKey(k))
				{
					dictionary2.Add(k, format.CurrentListStyle.Levels[k].StartAt + 1);
				}
			}
			else if (format.CurrentListStyle.Levels[k].PatternType != ListPatternType.Bullet && !dictionary2.ContainsKey(k))
			{
				dictionary2.Add(k, format.CurrentListStyle.Levels[k].StartAt + 1);
			}
		}
		ResetInbetweenLevels(format, styleName, listLevel, dictionary2);
		return startAt2;
	}

	private void ResetInbetweenLevels(WListFormat format, string styleName, WListLevel listLevel, Dictionary<int, int> lstStyle)
	{
		if (PreviousListLevel[styleName] - listLevel.LevelNumber < -1)
		{
			for (int i = PreviousListLevel[styleName] + 1; i < listLevel.LevelNumber; i++)
			{
				lstStyle[i] = format.CurrentListStyle.Levels[i].StartAt + 1;
			}
		}
	}

	private string GetListValue(WListFormat listFormat, string styleName, WListLevel level, int startAt, int listItemIndex)
	{
		string result = string.Empty;
		int levelNumber = level.LevelNumber;
		string text = null;
		string[] array = new string[9] { "\0", "\u0001", "\u0002", "\u0003", "\u0004", "\u0005", "\u0006", "\a", "\b" };
		for (int i = 0; i <= levelNumber; i++)
		{
			if (level.NumberPrefix.Contains(array[i]))
			{
				text += i;
			}
		}
		if (Lists.ContainsKey(styleName))
		{
			string text2 = string.Empty;
			Dictionary<int, int> dictionary = Lists[styleName];
			int[] array2 = new int[dictionary.Count];
			dictionary.Keys.CopyTo(array2, 0);
			array2 = SortKeys(array2);
			int num = array2[0];
			WListLevel wListLevel = null;
			bool leadZero = listItemIndex < 9;
			string orderedNumberPrefix = GetOrderedNumberPrefix(level.NumberPrefix);
			for (int j = num; (num == levelNumber) ? (j <= levelNumber) : (j < levelNumber); j++)
			{
				if (text.Contains(j.ToString()) && dictionary.ContainsKey(j))
				{
					wListLevel = listFormat.CurrentListStyle.Levels[j];
					string listPrefixByItsLevel = GetListPrefixByItsLevel(j + 1, orderedNumberPrefix);
					text2 = ((!level.IsLegalStyleNumbering || wListLevel.PatternType == ListPatternType.None) ? (text2 + GetNumberedListValue(wListLevel, dictionary[j] - 1, leadZero, listPrefixByItsLevel)) : (text2 + Convert.ToString(dictionary[j] - 1) + listPrefixByItsLevel));
				}
			}
			if (level.PatternType == ListPatternType.LeadingZero && startAt < 10)
			{
				leadZero = true;
			}
			text2 += GetNumberedListValue(level, startAt, leadZero, "");
			string numberPrefix = level.NumberPrefix;
			if (!numberPrefix.StartsWith("\0", StringComparison.Ordinal) && numberPrefix.Contains("\0"))
			{
				numberPrefix = numberPrefix.Substring(0, numberPrefix.IndexOf("\0", StringComparison.Ordinal));
				text2 = numberPrefix + text2;
			}
			result = text2 + level.NumberSuffix;
		}
		return result;
	}

	private string GetOrderedNumberPrefix(string levelNumberPrefix)
	{
		string[] array = new string[9] { "\0", "\u0001", "\u0002", "\u0003", "\u0004", "\u0005", "\u0006", "\a", "\b" };
		char[] array2 = new char[levelNumberPrefix.Length];
		int num = 0;
		string text = levelNumberPrefix;
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			if (Array.IndexOf(array, c.ToString()) != -1)
			{
				if (num == 0 || array2[num - 1] <= c)
				{
					array2[num] = c;
				}
				else
				{
					array2[num] = array2[num - 1];
					array2[num - 1] = c;
				}
				num++;
			}
		}
		string text2 = string.Empty;
		num = 0;
		text = levelNumberPrefix;
		for (int i = 0; i < text.Length; i++)
		{
			string text3 = text[i].ToString();
			if (Array.IndexOf(array, text3) != -1)
			{
				text2 += array2[num];
				num++;
			}
			else
			{
				text2 += text3;
			}
		}
		return text2;
	}

	private string GetListPrefixByItsLevel(int levelNo, string listPrefix)
	{
		string result = null;
		switch (levelNo)
		{
		case 1:
		{
			int num = listPrefix.IndexOf("\0", StringComparison.Ordinal) + 1;
			int num2 = (listPrefix.Contains("\u0001") ? listPrefix.IndexOf("\u0001", StringComparison.Ordinal) : (listPrefix.Contains("\u0002") ? listPrefix.IndexOf("\u0002", StringComparison.Ordinal) : listPrefix.Length));
			result = listPrefix.Substring(num, num2 - num);
			break;
		}
		case 2:
		{
			int num = listPrefix.IndexOf("\u0001", StringComparison.Ordinal) + 1;
			int num2 = (listPrefix.Contains("\u0002") ? listPrefix.IndexOf("\u0002", StringComparison.Ordinal) : listPrefix.Length);
			result = listPrefix.Substring(num, num2 - num);
			break;
		}
		case 3:
		{
			int num = listPrefix.IndexOf("\u0002", StringComparison.Ordinal) + 1;
			int num2 = (listPrefix.Contains("\u0003") ? listPrefix.IndexOf("\u0003", StringComparison.Ordinal) : listPrefix.Length);
			result = listPrefix.Substring(num, num2 - num);
			break;
		}
		case 4:
		{
			int num = listPrefix.IndexOf("\u0003", StringComparison.Ordinal) + 1;
			int num2 = (listPrefix.Contains("\u0004") ? listPrefix.IndexOf("\u0004", StringComparison.Ordinal) : listPrefix.Length);
			result = listPrefix.Substring(num, num2 - num);
			break;
		}
		case 5:
		{
			int num = listPrefix.IndexOf("\u0004", StringComparison.Ordinal) + 1;
			int num2 = (listPrefix.Contains("\u0005") ? listPrefix.IndexOf("\u0005", StringComparison.Ordinal) : listPrefix.Length);
			result = listPrefix.Substring(num, num2 - num);
			break;
		}
		case 6:
		{
			int num = listPrefix.IndexOf("\u0005", StringComparison.Ordinal) + 1;
			int num2 = (listPrefix.Contains("\u0006") ? listPrefix.IndexOf("\u0006", StringComparison.Ordinal) : listPrefix.Length);
			result = listPrefix.Substring(num, num2 - num);
			break;
		}
		case 7:
		{
			int num = listPrefix.IndexOf("\u0006", StringComparison.Ordinal) + 1;
			int num2 = (listPrefix.Contains("\a") ? listPrefix.IndexOf("\a", StringComparison.Ordinal) : listPrefix.Length);
			result = listPrefix.Substring(num, num2 - num);
			break;
		}
		case 8:
		{
			int num = listPrefix.IndexOf("\a", StringComparison.Ordinal) + 1;
			int num2 = (listPrefix.Contains("\b") ? listPrefix.IndexOf("\b", StringComparison.Ordinal) : listPrefix.Length);
			result = listPrefix.Substring(num, num2 - num);
			break;
		}
		}
		return result;
	}

	private string GetNumberedListValue(WListLevel prevLevel, int num, bool leadZero, string listPrefix)
	{
		if (prevLevel.IsLegalStyleNumbering && prevLevel.PatternType != ListPatternType.LeadingZero)
		{
			return Convert.ToString(num) + listPrefix;
		}
		string empty = string.Empty;
		switch (prevLevel.PatternType)
		{
		case ListPatternType.UpLetter:
			return base.Document.GetAsLetter(num).ToUpper() + listPrefix;
		case ListPatternType.LowLetter:
			return base.Document.GetAsLetter(num).ToLower() + listPrefix;
		case ListPatternType.Arabic:
			return Convert.ToString(num) + listPrefix;
		case ListPatternType.UpRoman:
			return base.Document.GetAsRoman(num).ToUpper() + listPrefix;
		case ListPatternType.LowRoman:
			return base.Document.GetAsRoman(num).ToLower() + listPrefix;
		case ListPatternType.Ordinal:
			return base.Document.GetOrdinal(num, prevLevel.CharacterFormat) + listPrefix;
		case ListPatternType.LeadingZero:
			if (leadZero)
			{
				return "0" + Convert.ToString(num) + listPrefix;
			}
			return Convert.ToString(num) + listPrefix;
		case ListPatternType.None:
			return listPrefix;
		default:
			return Convert.ToString(num) + listPrefix;
		}
	}

	private int[] SortKeys(int[] keys)
	{
		for (int i = 0; i < keys.Length - 1; i++)
		{
			for (int j = i + 1; j < keys.Length; j++)
			{
				if (keys[i] > keys[j])
				{
					int num = keys[i];
					keys[i] = keys[j];
					keys[j] = num;
				}
			}
		}
		return keys;
	}

	public int ReplaceSingleLine(string given, string replace, bool caseSensitive, bool wholeWord)
	{
		Regex pattern = FindUtils.StringToRegex(given, caseSensitive, wholeWord);
		return ReplaceSingleLine(pattern, replace);
	}

	public int ReplaceSingleLine(Regex pattern, string replace)
	{
		TextBodyItem startItem = Sections[0].Body.Items[0];
		int num = ReplaceSingleLine(pattern, replace, startItem);
		if (ReplaceFirst && num > 0)
		{
			return num;
		}
		return num + ReplaceHFSingleLine(pattern, replace);
	}

	public int ReplaceSingleLine(string given, TextSelection replacement, bool caseSensitive, bool wholeWord)
	{
		Regex pattern = FindUtils.StringToRegex(given, caseSensitive, wholeWord);
		return ReplaceSingleLine(pattern, replacement);
	}

	public int ReplaceSingleLine(Regex pattern, TextSelection replacement)
	{
		int num = 0;
		TextBodyItem textBodyItem = Sections[0].Body.Items[0];
		for (TextSelection[] array = FindNextSingleLine(textBodyItem, pattern); array != null; array = FindNextSingleLine(textBodyItem, pattern))
		{
			m_nextParaItem = null;
			TextReplacer.Instance.ReplaceSingleLine(array, replacement);
			num++;
			if (ReplaceFirst)
			{
				break;
			}
			if (textBodyItem.Owner == null)
			{
				textBodyItem = Sections[0].Body.Items[0];
			}
		}
		return num;
	}

	public int ReplaceSingleLine(string given, TextBodyPart replacement, bool caseSensitive, bool wholeWord)
	{
		Regex pattern = FindUtils.StringToRegex(given, caseSensitive, wholeWord);
		return ReplaceSingleLine(pattern, replacement);
	}

	public int ReplaceSingleLine(Regex pattern, TextBodyPart replacement)
	{
		int num = 0;
		TextBodyItem startBodyItem = Sections[0].Body.Items[0];
		for (TextSelection[] array = FindNextSingleLine(startBodyItem, pattern); array != null; array = FindNextSingleLine(startBodyItem, pattern))
		{
			m_nextParaItem = null;
			TextReplacer.Instance.ReplaceSingleLine(array, replacement);
			num++;
			if (ReplaceFirst)
			{
				break;
			}
		}
		return num;
	}

	private int ReplaceHFSingleLine(Regex pattern, string replace)
	{
		ResetSingleLineReplace();
		int num = 0;
		foreach (WSection section in Sections)
		{
			foreach (HeaderFooter headersFooter in section.HeadersFooters)
			{
				if (headersFooter.Items.Count > 0)
				{
					num += ReplaceSingleLine(pattern, replace, headersFooter.Items[0]);
				}
			}
		}
		ResetSingleLineReplace();
		return num;
	}

	private void ResetSingleLineReplace()
	{
		ResetFindNext();
		TextFinder.Instance.SingleLinePCol.Clear();
	}

	private int ReplaceSingleLine(Regex pattern, string replace, TextBodyItem startItem)
	{
		int num = 0;
		Entity owner = startItem.Owner;
		for (TextSelection[] array = FindNextSingleLine(startItem, pattern); array != null; array = FindNextSingleLine(startItem, pattern))
		{
			m_nextParaItem = null;
			TextReplacer.Instance.ReplaceSingleLine(array, replace);
			num++;
			if (ReplaceFirst)
			{
				break;
			}
			if (startItem.Owner == null)
			{
				startItem = ((!(owner is HeaderFooter) || (owner as HeaderFooter).Items.Count <= 0) ? (owner as WTextBody).Items[0] : (owner as HeaderFooter).Items[0]);
			}
		}
		return num;
	}

	private bool IsFloatingItemsContainSameZIndexValue()
	{
		for (int i = 0; i < FloatingItems.Count; i++)
		{
			int zOrder = FloatingItems[i].GetZOrder();
			for (int j = 0; j < FloatingItems.Count; j++)
			{
				if (i != j && zOrder == FloatingItems[j].GetZOrder())
				{
					return true;
				}
			}
		}
		return false;
	}

	internal void SortByZIndex(bool isFromHTMLExport)
	{
		if (!isFromHTMLExport && !IsFloatingItemsContainSameZIndexValue())
		{
			return;
		}
		Entity entity = null;
		for (int i = 1; i < FloatingItems.Count; i++)
		{
			for (int j = 0; j < FloatingItems.Count - 1; j++)
			{
				double num = Math.Abs((double)FloatingItems[j + 1].GetZOrder());
				double num2 = Math.Abs((double)FloatingItems[j].GetZOrder());
				if (num < num2 || (num == num2 && FloatingItems[j + 1].IsNeedToSortByItsPosition(FloatingItems[j])))
				{
					entity = FloatingItems[j];
					FloatingItems[j] = FloatingItems[j + 1];
					FloatingItems[j + 1] = entity;
				}
			}
		}
		SetZOrderPosition();
	}

	private void SetZOrderPosition()
	{
		int num = 1024;
		foreach (Entity floatingItem in FloatingItems)
		{
			switch (floatingItem.EntityType)
			{
			case EntityType.TextBox:
				(floatingItem as WTextBox).TextBoxFormat.OrderIndex = (((floatingItem as WTextBox).TextBoxFormat.OrderIndex >= 0) ? num : (-num));
				if ((floatingItem as WTextBox).IsShape && (floatingItem as WTextBox).Shape != null)
				{
					(floatingItem as WTextBox).Shape.ZOrderPosition = (((floatingItem as WTextBox).Shape.ZOrderPosition >= 0) ? num : (-num));
				}
				break;
			case EntityType.Picture:
				(floatingItem as WPicture).OrderIndex = (((floatingItem as WPicture).OrderIndex >= 0) ? num : (-num));
				break;
			case EntityType.Shape:
			case EntityType.AutoShape:
				(floatingItem as Shape).ZOrderPosition = (((floatingItem as Shape).ZOrderPosition >= 0) ? num : (-num));
				break;
			case EntityType.GroupShape:
				(floatingItem as GroupShape).ZOrderPosition = (((floatingItem as GroupShape).ZOrderPosition >= 0) ? num : (-num));
				break;
			case EntityType.Chart:
				(floatingItem as WChart).ZOrderPosition = (((floatingItem as WChart).ZOrderPosition >= 0) ? num : (-num));
				break;
			case EntityType.XmlParaItem:
				(floatingItem as XmlParagraphItem).ZOrderIndex = (((floatingItem as XmlParagraphItem).ZOrderIndex >= 0) ? num : (-num));
				break;
			case EntityType.OleObject:
				if ((floatingItem as WOleObject).OlePicture != null)
				{
					(floatingItem as WOleObject).OlePicture.OrderIndex = (((floatingItem as WOleObject).OlePicture.OrderIndex >= 0) ? num : (-num));
				}
				break;
			}
			num += 1024;
		}
	}

	public TextSelection FindNext(TextBodyItem startTextBodyItem, string given, bool caseSensitive, bool wholeWord)
	{
		Regex pattern = FindUtils.StringToRegex(given, caseSensitive, wholeWord);
		return FindNext(startTextBodyItem, pattern);
	}

	public TextSelection FindNext(TextBodyItem startBodyItem, Regex pattern)
	{
		if (startBodyItem == null)
		{
			throw new ArgumentException("Start body item can't be null", "startBodyItem");
		}
		if (m_prevBodyItem == null)
		{
			m_prevBodyItem = startBodyItem;
		}
		else if (m_prevBodyItem != startBodyItem)
		{
			m_nextParaItem = null;
			m_prevBodyItem = startBodyItem;
		}
		TextSelection textSelection = null;
		if (m_nextParaItem != null && m_nextParaItem.OwnerParagraph != null)
		{
			textSelection = FindNext(pattern);
			if (textSelection != null)
			{
				textSelection.GetAsOneRange();
				UpdateNextItem(textSelection);
				return textSelection;
			}
			startBodyItem = m_nextParaItem.OwnerParagraph.NextTextBodyItem;
			if (startBodyItem == null)
			{
				m_nextParaItem = null;
				return null;
			}
		}
		TextBodyItem textBodyItem = startBodyItem;
		do
		{
			textSelection = textBodyItem.Find(pattern);
			if (CheckSelection(textSelection))
			{
				textSelection.GetAsOneRange();
				UpdateNextItem(textSelection);
				return textSelection;
			}
			textBodyItem = textBodyItem.NextTextBodyItem;
		}
		while (textBodyItem != null);
		return null;
	}

	private TextSelection FindNext(Regex pattern)
	{
		TextSelectionList textSelectionList = m_nextParaItem.OwnerParagraph.FindAll(pattern, isDocumentComparison: false);
		if (textSelectionList.Count > 0)
		{
			int indexInOwnerCollection = m_nextParaItem.GetIndexInOwnerCollection();
			int num = 0;
			foreach (TextSelection item in textSelectionList)
			{
				if (CheckSelection(item))
				{
					item.StartTextRange.GetIndexInOwnerCollection();
					num = item.EndTextRange.GetIndexInOwnerCollection();
					if (indexInOwnerCollection <= num)
					{
						return item;
					}
				}
			}
		}
		return null;
	}

	private void UpdateNextItem(TextSelection selection)
	{
		m_nextParaItem = null;
		WTextRange[] ranges = selection.GetRanges();
		if (ranges != null)
		{
			WTextRange wTextRange = ranges[^1];
			if (wTextRange.NextSibling != null)
			{
				m_nextParaItem = wTextRange.NextSibling as ParagraphItem;
				return;
			}
		}
		TextBodyItem textBodyItem = selection.OwnerParagraph;
		while (textBodyItem.NextTextBodyItem != null)
		{
			textBodyItem = textBodyItem.NextTextBodyItem;
			m_nextParaItem = GetNextItem(textBodyItem);
			if (m_nextParaItem != null)
			{
				break;
			}
		}
	}

	private ParagraphItem GetNextItem(TextBodyItem tbItem)
	{
		if (tbItem == null)
		{
			return null;
		}
		if (tbItem is WTable)
		{
			ParagraphItem paragraphItem = null;
			foreach (WTableRow row in (tbItem as WTable).Rows)
			{
				foreach (WTableCell cell in row.Cells)
				{
					paragraphItem = GetNextItem(cell);
					if (paragraphItem != null)
					{
						return paragraphItem;
					}
				}
			}
		}
		else if (tbItem is BlockContentControl)
		{
			ParagraphItem paragraphItem2 = null;
			paragraphItem2 = GetNextItem((tbItem as BlockContentControl).TextBody);
			if (paragraphItem2 != null)
			{
				return paragraphItem2;
			}
		}
		else
		{
			if ((tbItem as WParagraph).Items.Count > 0)
			{
				return (tbItem as WParagraph).Items[0];
			}
			if (tbItem.NextSibling != null)
			{
				return GetNextItem(tbItem.NextSibling as TextBodyItem);
			}
		}
		return null;
	}

	private ParagraphItem GetNextItem(WTextBody textBody)
	{
		ParagraphItem paragraphItem = null;
		foreach (TextBodyItem item in textBody.Items)
		{
			paragraphItem = GetNextItem(item);
			if (item != null)
			{
				return paragraphItem;
			}
		}
		return null;
	}

	private bool CheckSelection(TextSelection textSel)
	{
		if (textSel != null && textSel.Count > 0)
		{
			return true;
		}
		return false;
	}

	public TextSelection[] FindNextSingleLine(TextBodyItem startTextBodyItem, string given, bool caseSensitive, bool wholeWord)
	{
		Regex pattern = FindUtils.StringToRegex(given, caseSensitive, wholeWord);
		return FindNextSingleLine(startTextBodyItem, pattern);
	}

	public TextSelection[] FindNextSingleLine(TextBodyItem startBodyItem, Regex pattern)
	{
		if (startBodyItem == null)
		{
			throw new ArgumentException("Start body item can't be null", "startBodyItem");
		}
		if (m_prevBodyItem == null)
		{
			m_prevBodyItem = startBodyItem;
		}
		else if (m_prevBodyItem != startBodyItem)
		{
			m_nextParaItem = null;
			m_prevBodyItem = startBodyItem;
		}
		TextSelection[] array = null;
		if (m_nextParaItem == null)
		{
			m_nextParaItem = GetNextItem(startBodyItem);
		}
		array = FindNextSingleLine(pattern);
		if (array != null)
		{
			TextSelection textSelection = array[^1];
			textSelection.GetAsOneRange();
			UpdateNextItem(textSelection);
			return array;
		}
		m_nextParaItem = null;
		TextFinder.Instance.SingleLinePCol.Clear();
		return null;
	}

	private TextSelection[] FindNextSingleLine(Regex pattern)
	{
		if (m_nextParaItem == null)
		{
			return null;
		}
		WParagraph ownerParagraph = m_nextParaItem.OwnerParagraph;
		int indexInOwnerCollection = ownerParagraph.GetIndexInOwnerCollection();
		if (indexInOwnerCollection == 0)
		{
			TextFinder.Instance.SingleLinePCol.Clear();
		}
		int indexInOwnerCollection2 = m_nextParaItem.GetIndexInOwnerCollection();
		TextSelection[] array = TextFinder.Instance.FindInItems(ownerParagraph, pattern, indexInOwnerCollection2, ownerParagraph.Items.Count - 1);
		if (array == null)
		{
			WTextBody ownerTextBody = ownerParagraph.OwnerTextBody;
			if (ownerTextBody != null)
			{
				array = TextFinder.Instance.FindSingleLine(ownerTextBody, pattern, indexInOwnerCollection + 1, ownerTextBody.Items.Count - 1);
			}
			if (array == null)
			{
				TextBodyItem nextTextBodyItem;
				for (nextTextBodyItem = ownerTextBody.Items[ownerTextBody.Items.Count - 1].NextTextBodyItem; nextTextBodyItem != null; nextTextBodyItem = nextTextBodyItem.NextTextBodyItem)
				{
					if (nextTextBodyItem.GetIndexInOwnerCollection() == 0)
					{
						TextFinder.Instance.SingleLinePCol.Clear();
					}
					m_nextParaItem = GetNextItem(nextTextBodyItem);
					if (m_nextParaItem != null)
					{
						break;
					}
				}
				if (nextTextBodyItem != null)
				{
					array = FindNextSingleLine(pattern);
				}
			}
		}
		return array;
	}

	public void ResetFindNext()
	{
		m_nextParaItem = null;
		m_prevBodyItem = null;
	}

	public ParagraphItem CreateParagraphItem(ParagraphItemType itemType)
	{
		return itemType switch
		{
			ParagraphItemType.Break => new Break(this), 
			ParagraphItemType.TextRange => new WTextRange(this), 
			ParagraphItemType.Picture => new WPicture(this), 
			ParagraphItemType.BookmarkStart => new BookmarkStart(this), 
			ParagraphItemType.BookmarkEnd => new BookmarkEnd(this), 
			ParagraphItemType.Field => new WField(this), 
			ParagraphItemType.TextBox => new WTextBox(this), 
			ParagraphItemType.MergeField => new WMergeField(this), 
			ParagraphItemType.EmbedField => new WEmbedField(this), 
			ParagraphItemType.Symbol => new WSymbol(this), 
			ParagraphItemType.FieldMark => new WFieldMark(this), 
			ParagraphItemType.CheckBox => new WCheckBox(this), 
			ParagraphItemType.TextFormField => new WTextFormField(this), 
			ParagraphItemType.DropDownFormField => new WDropDownFormField(this), 
			ParagraphItemType.Comment => new WComment(this), 
			ParagraphItemType.Footnote => new WFootnote(this), 
			ParagraphItemType.ShapeObject => new ShapeObject(this), 
			ParagraphItemType.InlineShapeObject => new InlineShapeObject(this), 
			ParagraphItemType.TOC => new TableOfContent(this), 
			ParagraphItemType.OleObject => new WOleObject(this), 
			ParagraphItemType.InlineContentControl => new InlineContentControl(this), 
			ParagraphItemType.Math => new WMath(this), 
			ParagraphItemType.Chart => new WChart(this), 
			_ => throw new ArgumentException("Invalid type of paragraph item"), 
		};
	}

	protected override object CloneImpl()
	{
		lock (m_threadLocker)
		{
			return new WordDocument(this);
		}
	}

	protected internal WCharacterFormat CreateCharacterFormatImpl()
	{
		return new WCharacterFormat(this);
	}

	protected internal ListStyle CreateListStyleImpl()
	{
		return new ListStyle(this);
	}

	protected internal WListLevel CreateListLevelImpl(ListStyle style)
	{
		return new WListLevel(style);
	}

	protected internal WParagraphFormat CreateParagraphFormatImpl()
	{
		return new WParagraphFormat(this);
	}

	protected internal RowFormat CreateTableFormatImpl()
	{
		return new RowFormat();
	}

	protected internal CellFormat CreateCellFormatImpl()
	{
		return new CellFormat();
	}

	protected internal WTextBoxFormat CreateTextboxFormatImpl()
	{
		return new WTextBoxFormat(this);
	}

	protected internal WTextBoxCollection CreateTextBoxCollectionImpl()
	{
		return new WTextBoxCollection(this);
	}

	protected internal WListFormat CreateListFormatImpl(IWParagraph owner)
	{
		return new WListFormat(owner);
	}

	internal ICompoundFile CreateCompoundFile()
	{
		return new DocGen.CompoundFile.DocIO.Net.CompoundFile();
	}

	internal ICompoundFile CreateCompoundFile(Stream stream)
	{
		return new DocGen.CompoundFile.DocIO.Net.CompoundFile(stream);
	}

	internal bool CheckForEncryption(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		bool result = false;
		if (DocGen.CompoundFile.DocIO.Net.CompoundFile.CheckHeader(stream))
		{
			result = true;
		}
		return result;
	}

	internal void EnsureParagraphStyle(IWParagraph paragraph)
	{
		if (paragraph.StyleName == null)
		{
			if (Styles.FindByName("Normal") == null)
			{
				AddStyle(StyleType.ParagraphStyle, "Normal");
			}
			(paragraph as WParagraph).ApplyStyle("Normal", isDomChanges: false);
		}
	}

	private void AddEmptyParagraph()
	{
		if (LastSection != null && LastSection.Body.ChildEntities.Count > 0 && LastSection.Body.ChildEntities[LastSection.Body.ChildEntities.Count - 1] is WTable)
		{
			LastSection.Body.AddParagraph();
		}
	}

	internal void CloneShapeEscher(WordDocument destDoc, IParagraphItem shapeItem)
	{
		if (Escher == null)
		{
			return;
		}
		if (destDoc.m_escher == null)
		{
			destDoc.m_escher = new EscherClass(destDoc);
		}
		if (shapeItem != null)
		{
			if (shapeItem is IWPicture)
			{
				ClonePictureContainer(destDoc, shapeItem as WPicture);
			}
			else if (shapeItem is IWTextBox)
			{
				CloneTextBoxContainer(destDoc, shapeItem as WTextBox);
			}
			else if (shapeItem is ShapeObject)
			{
				CloneAutoShapeContainer(destDoc, shapeItem as ShapeObject);
			}
			m_defShapeId++;
		}
	}

	internal void CloneProperties(Dictionary<string, Stream> sourceProps, ref Dictionary<string, Stream> destinationProps)
	{
		destinationProps = new Dictionary<string, Stream>();
		foreach (KeyValuePair<string, Stream> sourceProp in sourceProps)
		{
			destinationProps.Add(sourceProp.Key, CloneStream(sourceProp.Value));
		}
	}

	internal void CloneProperties(Dictionary<string, string> sourceProps, ref Dictionary<string, string> destinationProps)
	{
		destinationProps = new Dictionary<string, string>();
		foreach (KeyValuePair<string, string> sourceProp in sourceProps)
		{
			destinationProps.Add(sourceProp.Key, sourceProp.Value);
		}
	}

	internal void CloneProperties(List<Stream> sourceProps, ref List<Stream> destinationProps)
	{
		destinationProps = new List<Stream>();
		foreach (Stream sourceProp in sourceProps)
		{
			destinationProps.Add(CloneStream(sourceProp));
		}
	}

	internal Stream CloneStream(Stream input)
	{
		MemoryStream memoryStream = new MemoryStream();
		byte[] array = new byte[input.Length];
		input.Seek(0L, SeekOrigin.Begin);
		int num = input.Read(array, 0, array.Length);
		if (num > 0)
		{
			memoryStream.Write(array, 0, num);
		}
		return memoryStream;
	}

	internal string GetPasswordValue()
	{
		return m_password;
	}

	internal bool IsNeedToAddLineNumbers()
	{
		foreach (WSection section in Sections)
		{
			if (section.LineNumbersEnabled())
			{
				return true;
			}
		}
		return false;
	}

	internal void InsertWatermark(WatermarkType type)
	{
		ResetWatermark();
		if (type != 0 && m_escher == null)
		{
			m_escher = new EscherClass(this);
		}
		switch (type)
		{
		case WatermarkType.PictureWatermark:
			m_watermark = new PictureWatermark(this);
			break;
		case WatermarkType.TextWatermark:
			m_watermark = new TextWatermark(this);
			break;
		default:
			m_watermark = new Watermark(this, type);
			break;
		}
	}

	internal void ReadBackground()
	{
		if (m_escher != null)
		{
			m_background = new Background(this);
		}
	}

	internal bool HasListStyle()
	{
		if (m_listStyles == null || m_listStyles.Count <= 0)
		{
			return false;
		}
		return true;
	}

	internal void UpdateStartPosOfParaItems(ParagraphItem pItem, int offset)
	{
		int num = pItem.OwnerParagraph.Items.IndexOf(pItem);
		if (!(pItem.Owner is InlineContentControl) && num < 0)
		{
			throw new InvalidOperationException("pItem haven't found in paragraph items");
		}
		int i = num + 1;
		for (int count = pItem.OwnerParagraph.Items.Count; i < count; i++)
		{
			ParagraphItem item = pItem.OwnerParagraph.Items[i];
			UpdateStartPos(item, offset);
		}
	}

	internal void UpdateStartPos(ParagraphItem item, int offset)
	{
		if (item is InlineContentControl)
		{
			UpdateStartPosOfInlineContentControlItems(item as InlineContentControl, 0, offset);
		}
		if (item != null)
		{
			item.StartPos += offset;
		}
	}

	internal void UpdateStartPosOfInlineContentControlItems(InlineContentControl inlineContentControl, int index, int offset)
	{
		while (index < inlineContentControl.ParagraphItems.Count)
		{
			ParagraphItem item = inlineContentControl.ParagraphItems[index];
			UpdateStartPos(item, offset);
			index++;
		}
	}

	internal bool IsDOCX()
	{
		if (ActualFormatType != FormatType.Docx && ActualFormatType != FormatType.Word2007 && ActualFormatType != FormatType.Word2010 && ActualFormatType != FormatType.Word2013 && ActualFormatType != FormatType.Word2007Dotx && ActualFormatType != FormatType.Word2010Dotx)
		{
			return ActualFormatType == FormatType.Word2013Dotx;
		}
		return true;
	}

	private void InitDefaultParagraphFormat()
	{
		m_defParaFormat = new WParagraphFormat(this);
		if (Styles.FindByName("Normal", StyleType.ParagraphStyle) is WParagraphStyle wParagraphStyle && !IsCloning)
		{
			m_defParaFormat.ImportContainer(wParagraphStyle.ParagraphFormat);
			m_defParaFormat.CopyProperties(wParagraphStyle.ParagraphFormat);
		}
	}

	private void Init()
	{
		IsNormalStyleDefined = false;
		IsDefaultParagraphFontStyleDefined = false;
		CloseStyles();
		if (!string.IsNullOrEmpty(WListFormat.m_currentStyleName))
		{
			WListFormat.m_currentStyleName = null;
		}
		if (WListFormat.m_currLevelNumber != 0)
		{
			WListFormat.m_currLevelNumber = 0;
		}
		if (m_mailMerge != null)
		{
			m_mailMerge.Close();
			m_mailMerge = null;
		}
		if (m_viewSetup != null)
		{
			m_viewSetup.Close();
			m_viewSetup = null;
		}
		if (m_builtinProp != null)
		{
			m_builtinProp.Close();
			m_builtinProp = null;
		}
		if (m_customProp != null)
		{
			m_customProp.Close();
			m_customProp = null;
		}
		if (m_txbxItems != null)
		{
			m_txbxItems.Close();
			m_txbxItems = null;
		}
		if (m_background != null)
		{
			m_background.Close();
			m_background = null;
		}
		if (m_dop != null)
		{
			m_dop = null;
		}
		if (m_sections != null)
		{
			m_sections.Close();
			m_sections = null;
		}
		if (m_bookmarks != null)
		{
			m_bookmarks.Close();
			m_bookmarks = null;
		}
		if (m_editableRanges != null)
		{
			m_editableRanges.Close();
			m_editableRanges = null;
		}
		if (m_revisions != null)
		{
			m_revisions.Close();
			m_revisions = null;
		}
		DocHasThemes = false;
		if (m_themes != null)
		{
			m_themes = null;
		}
		if (m_watermark != null)
		{
			m_watermark.Close();
			m_watermark = null;
		}
		if (m_styleNameIds != null)
		{
			m_styleNameIds.Clear();
		}
		if (m_settings != null)
		{
			m_settings.CompatibilityOptions.PropertiesHash.Clear();
		}
		if (m_usedFonts != null)
		{
			m_usedFonts.Clear();
		}
		if (m_fontSubstitutionTable != null)
		{
			m_fontSubstitutionTable.Clear();
		}
		if (m_docxProps != null)
		{
			m_docxProps.Clear();
		}
		if (m_Comments != null)
		{
			m_Comments.Clear();
		}
		if (m_CommentsEx != null)
		{
			m_CommentsEx.Close();
			m_Comments = null;
		}
		if (m_fields != null)
		{
			if (m_fields.m_sortedAutoNumFields != null)
			{
				m_fields.m_sortedAutoNumFields.Clear();
			}
			if (m_fields.m_sortedAutoNumFieldIndexes != null)
			{
				m_fields.m_sortedAutoNumFieldIndexes.Clear();
			}
			m_fields.Clear();
		}
		m_htmlValidationOption = XHTMLValidationType.None;
		ThrowExceptionsForUnsupportedElements = false;
		HasPicture = false;
		WriteProtected = false;
		UpdateFields = false;
		IsEncrypted = false;
		ReplaceFirst = false;
		IsReadOnly = false;
		ImportStyles = true;
		m_defShapeId = 1;
		m_tableOfContent = null;
		m_latentStyles2010 = null;
		m_latentStyles = null;
		m_standardBidiFont = null;
		m_standardNonFarEastFont = null;
		m_standardFarEastFont = null;
		m_standardAsciiFont = null;
		m_macroCommands = null;
		m_macrosData = null;
		m_password = null;
		m_assocStrings = null;
		m_prevBodyItem = null;
		m_nextParaItem = null;
		m_props = null;
		m_variables = null;
		m_docxPackage = null;
		m_grammarSpellingData = null;
		if (m_revisionOptions != null)
		{
			m_revisionOptions = null;
		}
		if (m_fontSettings != null)
		{
			m_fontSettings.Close();
			m_fontSettings = null;
		}
		if (m_imageCollection != null)
		{
			m_imageCollection.Clear();
			m_imageCollection = null;
		}
		if (m_defParaFormat != null)
		{
			m_defParaFormat.Close();
			m_defParaFormat = null;
		}
		if (m_defCharFormat != null)
		{
			m_defCharFormat.Close();
			m_defCharFormat = null;
		}
		if (m_escher != null)
		{
			m_escher.Close();
			m_escher = null;
		}
		if (m_docxLaTeXConveter != null)
		{
			m_docxLaTeXConveter.Close();
			m_docxLaTeXConveter = null;
		}
		RemoveMacros();
		if (ClonedFields != null)
		{
			ClonedFields.Clear();
		}
		if (m_FloatingItems != null)
		{
			m_FloatingItems.Clear();
			m_FloatingItems = null;
		}
		m_notSupportedElementFlag = 0L;
		m_supportedElementFlag_1 = 0;
		m_supportedElementFlag_2 = 0;
		ResetSingleLineReplace();
		TextFinder.Close();
		UnitsConvertor.Close();
	}

	internal bool IsInternalManipulation()
	{
		if (!IsOpening && !IsCloning && !IsHTMLImport)
		{
			return IsSkipFieldDetach;
		}
		return true;
	}

	private void UpdateImportOption()
	{
		ImportOptions importOptions = ImportOptions.UseDestinationStyles;
		if ((m_importOption & ImportOptions.UseDestinationStyles) != 0)
		{
			importOptions = ImportOptions.UseDestinationStyles;
		}
		else if ((m_importOption & ImportOptions.MergeFormatting) != 0)
		{
			importOptions = ImportOptions.MergeFormatting;
		}
		else if ((m_importOption & ImportOptions.KeepTextOnly) != 0)
		{
			importOptions = ImportOptions.KeepTextOnly;
		}
		else if ((m_importOption & ImportOptions.KeepSourceFormatting) != 0)
		{
			importOptions = ImportOptions.KeepSourceFormatting;
		}
		if ((m_importOption & ImportOptions.ListContinueNumbering) != 0)
		{
			importOptions |= ImportOptions.ListContinueNumbering;
		}
		else if ((m_importOption & ImportOptions.ListRestartNumbering) != 0)
		{
			importOptions |= ImportOptions.ListRestartNumbering;
		}
		m_importOption = importOptions;
	}

	private void CopyBinaryData(byte[] srcData, ref byte[] destData)
	{
		if (srcData != null)
		{
			destData = new byte[srcData.Length];
			srcData.CopyTo(destData, 0);
		}
	}

	private void ClonePictureContainer(WordDocument destDoc, WPicture picture)
	{
		int shapeId = picture.ShapeId;
		if (CheckContainer(EscherShapeType.msosptPictureFrame, shapeId))
		{
			WordSubdocument docType = (picture.IsHeaderPicture ? WordSubdocument.HeaderFooter : WordSubdocument.Main);
			m_defShapeId = Escher.CloneContainerBySpid(destDoc, docType, shapeId, m_defShapeId);
			if (m_defShapeId != -1)
			{
				picture.ShapeId = m_defShapeId;
			}
		}
	}

	private void CloneTextBoxContainer(WordDocument destDoc, WTextBox textBox)
	{
		int textBoxShapeID = textBox.TextBoxFormat.TextBoxShapeID;
		if (CheckContainer(EscherShapeType.msosptTextBox, textBoxShapeID))
		{
			WordSubdocument docType = (textBox.TextBoxFormat.IsHeaderTextBox ? WordSubdocument.HeaderFooter : WordSubdocument.Main);
			m_defShapeId = Escher.CloneContainerBySpid(destDoc, docType, textBoxShapeID, m_defShapeId);
			if (m_defShapeId != -1)
			{
				textBox.TextBoxFormat.TextBoxShapeID = m_defShapeId;
			}
		}
	}

	private void CloneAutoShapeContainer(WordDocument destDoc, ShapeObject shapeObj)
	{
		int spid = shapeObj.FSPA.Spid;
		WordSubdocument docType = (shapeObj.IsHeaderAutoShape ? WordSubdocument.HeaderFooter : WordSubdocument.Main);
		m_defShapeId = Escher.CloneContainerBySpid(destDoc, docType, spid, m_defShapeId);
		if (m_defShapeId != -1)
		{
			shapeObj.FSPA.Spid = m_defShapeId;
		}
	}

	private bool CheckContainer(EscherShapeType type, int spid)
	{
		if (!Escher.Containers.ContainsKey(spid) || (Escher.Containers[spid] is MsofbtSpContainer && (Escher.Containers[spid] as MsofbtSpContainer).Shape.ShapeType != type))
		{
			m_defShapeId = -1;
			return false;
		}
		return true;
	}

	private byte[] GetBackGndImage()
	{
		if (Background.Type == BackgroundType.Picture)
		{
			return Background.Picture;
		}
		return null;
	}

	private void SetBackgroundImageValue(byte[] imageBytes)
	{
		Background.Picture = imageBytes;
		Background.Type = BackgroundType.Picture;
	}

	private bool HasTrackedChanges()
	{
		if (m_sections == null || m_sections.Count == 0)
		{
			return false;
		}
		foreach (WSection section in m_sections)
		{
			if (section.SectionFormat.IsChangedFormat || section.HasTrackedChanges())
			{
				return true;
			}
		}
		return false;
	}

	internal bool IsSecurityGranted()
	{
		return false;
	}

	private void CheckFileName(string fileName)
	{
		bool flag = false;
		if (fileName.Length >= 260)
		{
			flag = true;
		}
		if (flag)
		{
			throw new Exception("The file name is too long. The fully qualified file name must be less than 260 characters and the directory name must be less than 248 characters");
		}
	}

	private void ResetWatermark()
	{
		foreach (WSection section in Sections)
		{
			section.HeadersFooters.EvenHeader.WriteWatermark = false;
			section.HeadersFooters.OddHeader.WriteWatermark = false;
			section.HeadersFooters.FirstPageHeader.WriteWatermark = false;
		}
		if (m_watermark != null && m_watermark is PictureWatermark && (m_watermark as PictureWatermark).WordPicture != null && (m_watermark as PictureWatermark).WordPicture.Document != null && (m_watermark as PictureWatermark).WordPicture.ImageRecord != null)
		{
			(m_watermark as PictureWatermark).WordPicture.ImageRecord.OccurenceCount--;
		}
	}

	internal void UpdateHeaderWatermark(Watermark watermark)
	{
		foreach (WSection section in Sections)
		{
			section.HeadersFooters.EvenHeader.WriteWatermark = true;
			section.HeadersFooters.OddHeader.WriteWatermark = true;
			section.HeadersFooters.FirstPageHeader.WriteWatermark = true;
			section.HeadersFooters.EvenHeader.Watermark = watermark;
			section.HeadersFooters.OddHeader.Watermark = watermark;
			section.HeadersFooters.FirstPageHeader.Watermark = watermark;
		}
	}

	private void SetProtection(ProtectionType type)
	{
		if (type != ProtectionType.AllowOnlyFormFields)
		{
			return;
		}
		foreach (WSection section in Sections)
		{
			section.ProtectForm = true;
		}
	}

	protected override void InitXDLSHolder()
	{
		base.XDLSHolder.AddElement("styles", Styles);
		base.XDLSHolder.AddElement("liststyles", ListStyles);
		base.XDLSHolder.AddElement("sections", Sections);
		base.XDLSHolder.AddElement("view-setup", ViewSetup);
		base.XDLSHolder.AddElement("builtin-properties", BuiltinDocumentProperties);
		base.XDLSHolder.AddElement("custom-properties", CustomDocumentProperties);
		base.XDLSHolder.AddElement("list-overrides", ListOverrides);
		base.XDLSHolder.AddElement("background", Background);
		base.XDLSHolder.AddElement("watermark", Watermark);
	}

	protected override void WriteXmlContent(IXDLSContentWriter writer)
	{
		base.WriteXmlContent(writer);
		if (MacrosData != null)
		{
			writer.WriteChildBinaryElement("macros", MacrosData);
		}
		if (MacroCommands != null)
		{
			writer.WriteChildBinaryElement("macros-commands", MacroCommands);
		}
		if (m_escher != null)
		{
			MemoryStream memoryStream = new MemoryStream();
			m_escher.WriteContainersData(memoryStream);
			m_escherDataContainers = memoryStream.ToArray();
			writer.WriteChildBinaryElement("escher-data", m_escherDataContainers);
			memoryStream.Close();
			MemoryStream memoryStream2 = new MemoryStream();
			m_escher.WriteContainers(memoryStream2);
			m_escherContainers = memoryStream2.ToArray();
			writer.WriteChildBinaryElement("escher-containers", m_escherContainers);
			memoryStream2.Close();
			memoryStream = null;
			memoryStream2 = null;
			m_escherDataContainers = null;
			m_escherContainers = null;
		}
		if (m_dop != null)
		{
			MemoryStream memoryStream3 = new MemoryStream();
			m_dop.Write(memoryStream3);
			byte[] value = memoryStream3.ToArray();
			writer.WriteChildBinaryElement("dop-internal", value);
		}
		if (m_grammarSpellingData != null && GrammarSpellingData.PlcfgramData != null && GrammarSpellingData.PlcfsplData != null)
		{
			writer.WriteChildBinaryElement("grammar-data", GrammarSpellingData.PlcfgramData);
			writer.WriteChildBinaryElement("spelling-data", GrammarSpellingData.PlcfsplData);
		}
	}

	protected override bool ReadXmlContent(IXDLSContentReader reader)
	{
		if (reader.TagName == "macros")
		{
			MacrosData = reader.ReadChildBinaryElement();
		}
		if (reader.TagName == "macros-commands")
		{
			MacroCommands = reader.ReadChildBinaryElement();
		}
		if (reader.TagName == "escher-containers")
		{
			m_escherContainers = reader.ReadChildBinaryElement();
		}
		if (reader.TagName == "escher-data")
		{
			m_escherDataContainers = reader.ReadChildBinaryElement();
		}
		if (m_escherDataContainers != null && m_escherContainers != null)
		{
			MemoryStream memoryStream = new MemoryStream(m_escherDataContainers, 0, m_escherDataContainers.Length);
			MemoryStream memoryStream2 = new MemoryStream(m_escherContainers, 0, m_escherContainers.Length);
			m_escher = new EscherClass(memoryStream2, memoryStream, 0L, (int)memoryStream2.Length, this);
			memoryStream.Close();
			memoryStream2.Close();
			memoryStream = null;
			memoryStream2 = null;
			m_escherDataContainers = null;
			m_escherContainers = null;
		}
		if (reader.TagName == "dop-internal")
		{
			MemoryStream memoryStream3 = new MemoryStream(reader.ReadChildBinaryElement());
			m_dop = new DOPDescriptor(memoryStream3, 0, (int)memoryStream3.Length, isTemplate: false);
			memoryStream3.Close();
			memoryStream3 = null;
		}
		if (m_grammarSpellingData == null)
		{
			m_grammarSpellingData = new GrammarSpelling();
		}
		if (reader.TagName == "grammar-data")
		{
			m_grammarSpellingData.PlcfgramData = reader.ReadChildBinaryElement();
		}
		if (reader.TagName == "spelling-data")
		{
			m_grammarSpellingData.PlcfsplData = reader.ReadChildBinaryElement();
		}
		return base.ReadXmlContent(reader);
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		if (m_standardAsciiFont != null)
		{
			writer.WriteValue("StandardAscii", m_standardAsciiFont);
		}
		if (m_standardFarEastFont != null)
		{
			writer.WriteValue("StandardFarEast", m_standardFarEastFont);
		}
		if (m_standardNonFarEastFont != null)
		{
			writer.WriteValue("StandardNonFarEast", m_standardNonFarEastFont);
		}
		if (Watermark != null && Watermark.Type != 0)
		{
			writer.WriteValue("WatermarkType", Watermark.Type);
		}
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("StandardAscii"))
		{
			m_standardAsciiFont = reader.ReadString("StandardAscii");
		}
		if (reader.HasAttribute("StandardFarEast"))
		{
			m_standardFarEastFont = reader.ReadString("StandardFarEast");
		}
		if (reader.HasAttribute("StandardNonFarEast"))
		{
			m_standardNonFarEastFont = reader.ReadString("StandardNonFarEast");
		}
		if (reader.HasAttribute("WatermarkType"))
		{
			WatermarkType type = (WatermarkType)(object)reader.ReadEnum("WatermarkType", typeof(WatermarkType));
			InsertWatermark(type);
		}
	}

	protected override void CreateLayoutInfo()
	{
		m_layoutInfo = new LayoutInfo(ChildrenLayoutDirection.Vertical);
	}

	public void RemoveMacros()
	{
		if (VbaProject != null)
		{
			VbaProject.Close();
			VbaProject = null;
		}
		if (VbaProjectSignature != null)
		{
			VbaProjectSignature.Close();
			VbaProjectSignature = null;
		}
		if (VbaProjectSignatureAgile != null)
		{
			VbaProjectSignatureAgile.Close();
			VbaProjectSignatureAgile = null;
		}
		VbaData.Clear();
		DocEvents.Clear();
	}

	internal void SetTriggerElement(ref long flag, int bitPosition)
	{
		if (!HasElement(flag, bitPosition))
		{
			flag |= 1L << bitPosition;
		}
	}

	internal bool HasElement(long flag, int bitPosition)
	{
		return (flag & (1L << bitPosition)) != 0;
	}

	internal void SetTriggerElement(ref int flag, int bitPosition)
	{
		if (!HasElement(flag, bitPosition))
		{
			flag |= 1 << bitPosition;
		}
	}

	internal bool HasElement(int flag, int bitPosition)
	{
		return (flag & (1 << bitPosition)) != 0;
	}

	internal bool HasDifferentPageSetup()
	{
		for (WSection wSection = Sections[0]; wSection != null; wSection = ((wSection.NextSibling != null) ? (wSection.NextSibling as WSection) : null))
		{
			if (wSection.NextSibling != null && (wSection.PageSetup.FooterDistance != (wSection.NextSibling as WSection).PageSetup.FooterDistance || wSection.PageSetup.HeaderDistance != (wSection.NextSibling as WSection).PageSetup.HeaderDistance))
			{
				return true;
			}
		}
		return false;
	}

	internal void ParagraphItemRevision(ParagraphItem item, RevisionType revisionType, string revAuthorName, DateTime revDateTime, string name, bool isNestedRevision, Revision moveRevision, Revision contentRevision, Stack<Revision> m_trackchangeRevisionDetails)
	{
		Revision revision = null;
		bool isChildRevision = false;
		Revision rowRevision = null;
		WParagraph ownerParagraphValue = item.GetOwnerParagraphValue();
		if (ownerParagraphValue != null && ownerParagraphValue.IsInCell && (ownerParagraphValue.GetOwnerEntity() as WTableCell).OwnerRow.RowFormat.Revisions.Count > 0)
		{
			revision = GetRowRevision(item, revisionType, revAuthorName, ref isChildRevision, ref rowRevision);
		}
		if (moveRevision != null && moveRevision.RevisionType == revisionType && moveRevision.Author == revAuthorName)
		{
			revision = moveRevision;
		}
		if (contentRevision != null && contentRevision.RevisionType == revisionType && contentRevision.Author == revAuthorName)
		{
			revision = contentRevision;
		}
		if (revision == null && !HasRenderableItemBefore(item))
		{
			revision = GetRevisionForFirstParaItem(item, isChildRevision, revisionType, revAuthorName, revDateTime, name, rowRevision, moveRevision);
		}
		else if (revision == null)
		{
			revision = GetRevisionForRemainingParaItem(item, isChildRevision, revisionType, revAuthorName, revDateTime, name, rowRevision, moveRevision, m_trackchangeRevisionDetails);
		}
		if (revision != null)
		{
			bool flag = false;
			foreach (Revision item2 in item.RevisionsInternal)
			{
				if (item2.Author == revision.Author && item2.RevisionType == revision.RevisionType && item2.Date == revision.Date)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				int num = ((item.PreviousSibling != null) ? revision.Range.Items.IndexOf(item.PreviousSibling) : (-1));
				if (!revision.Range.Items.Contains(item))
				{
					revision.Range.Items.Insert((num != -1) ? (num + 1) : revision.Range.Items.Count, item);
				}
				item.RevisionsInternal.Add(revision);
			}
			else if (!revision.Range.Items.Contains(item))
			{
				int num2 = ((item.PreviousSibling != null) ? revision.Range.Items.IndexOf(item.PreviousSibling) : (-1));
				revision.Range.Items.Insert((num2 != -1) ? (num2 + 1) : revision.Range.Items.Count, item);
			}
		}
		if (isNestedRevision)
		{
			return;
		}
		foreach (Revision m_trackchangeRevisionDetail in m_trackchangeRevisionDetails)
		{
			if (m_trackchangeRevisionDetail.RevisionType == RevisionType.Insertions || m_trackchangeRevisionDetail.RevisionType == RevisionType.MoveTo)
			{
				item.SetInsertRev(value: true, m_trackchangeRevisionDetail.Author, m_trackchangeRevisionDetail.Date);
			}
			else if (m_trackchangeRevisionDetail.RevisionType == RevisionType.Deletions || m_trackchangeRevisionDetail.RevisionType == RevisionType.MoveFrom)
			{
				item.SetDeleteRev(value: true, m_trackchangeRevisionDetail.Author, m_trackchangeRevisionDetail.Date);
			}
			ParagraphItemRevision(item, m_trackchangeRevisionDetail.RevisionType, m_trackchangeRevisionDetail.Author, m_trackchangeRevisionDetail.Date, m_trackchangeRevisionDetail.Name, isNestedRevision: true, moveRevision, contentRevision, m_trackchangeRevisionDetails);
		}
	}

	private Revision GetRevisionForRemainingParaItem(ParagraphItem item, bool isChildRevision, RevisionType revisionType, string revAuthorName, DateTime revDateTime, string name, Revision rowRevision, Revision moveRevision, Stack<Revision> m_trackchangeRevisionDetails)
	{
		Revision revision = null;
		if (item.PreviousSibling != null && item.PreviousSibling is Entity)
		{
			Entity entity = item.PreviousSibling as Entity;
			do
			{
				entity = ((!(entity is InlineContentControl)) ? entity : (((entity as InlineContentControl).ParagraphItems.Count > 0) ? (entity as InlineContentControl).ParagraphItems.LastItem : entity));
				while (entity is BookmarkStart || entity is BookmarkEnd || entity is EditableRangeStart || entity is EditableRangeEnd)
				{
					entity = entity.PreviousSibling as Entity;
				}
			}
			while (entity is InlineContentControl && (entity as InlineContentControl).ParagraphItems.Count > 0);
			if (entity is WFieldMark && (entity as WFieldMark).Type == FieldMarkType.FieldEnd)
			{
				entity = (entity as WFieldMark).ParentField;
			}
			if (entity != null && entity.RevisionsInternal.Count > 0)
			{
				if (entity.RevisionsInternal.Count == 1 && (m_trackchangeRevisionDetails == null || m_trackchangeRevisionDetails.Count == 0))
				{
					revision = GetExistingRevision(entity.RevisionsInternal, revisionType, revAuthorName);
				}
				else if (m_trackchangeRevisionDetails != null && m_trackchangeRevisionDetails.Count == entity.RevisionsInternal.Count - 1)
				{
					revision = GetExistingRevision(entity.RevisionsInternal, revisionType, revAuthorName, m_trackchangeRevisionDetails, item);
				}
			}
			if (revision == null)
			{
				revision = CreateRevision(isChildRevision, revisionType, revAuthorName, revDateTime, name, rowRevision, moveRevision, item);
			}
		}
		else
		{
			revision = CreateRevision(isChildRevision, revisionType, revAuthorName, revDateTime, name, rowRevision, moveRevision, item);
		}
		return revision;
	}

	private Revision GetRevisionForFirstParaItem(ParagraphItem item, bool isChildRevision, RevisionType revisionType, string revAuthorName, DateTime revDateTime, string name, Revision rowRevision, Revision moveRevision)
	{
		Revision revision = null;
		if (item.Owner is WParagraph && item.Owner.PreviousSibling != null)
		{
			Entity entity = item.Owner.PreviousSibling as Entity;
			if (entity is WParagraph)
			{
				WParagraph wParagraph = entity as WParagraph;
				if (wParagraph.BreakCharacterFormat.Revisions.Count > 0)
				{
					revision = GetExistingRevision(wParagraph.BreakCharacterFormat.Revisions, revisionType, revAuthorName);
				}
				if (revision == null)
				{
					revision = CreateRevision(isChildRevision, revisionType, revAuthorName, revDateTime, name, rowRevision, moveRevision, item);
				}
			}
			else if (entity is WTable)
			{
				WTable wTable = entity as WTable;
				if (wTable.LastRow != null && wTable.LastRow.RowFormat.Revisions.Count > 0)
				{
					revision = GetExistingRevision(wTable.LastRow.RowFormat.Revisions, revisionType, revAuthorName);
				}
				if (revision == null)
				{
					revision = CreateRevision(isChildRevision, revisionType, revAuthorName, revDateTime, name, rowRevision, moveRevision, item);
				}
			}
			else
			{
				revision = CreateRevision(isChildRevision, revisionType, revAuthorName, revDateTime, name, rowRevision, moveRevision, item);
			}
		}
		else
		{
			revision = CreateRevision(isChildRevision, revisionType, revAuthorName, revDateTime, name, rowRevision, moveRevision, item);
		}
		return revision;
	}

	private Revision GetRowRevision(ParagraphItem item, RevisionType revisionType, string revAuthorName, ref bool isChildRevision, ref Revision rowRevision)
	{
		foreach (Revision revision in (item.GetOwnerParagraphValue().GetOwnerEntity() as WTableCell).OwnerRow.RowFormat.Revisions)
		{
			if (revision.RevisionType == revisionType)
			{
				if (revision.Author == revAuthorName)
				{
					return revision;
				}
				isChildRevision = true;
				rowRevision = revision;
			}
		}
		return null;
	}

	private Revision GetExistingRevision(List<Revision> revisions, RevisionType revisionType, string revAuthorName)
	{
		foreach (Revision revision in revisions)
		{
			if (revision.RevisionType == revisionType && (revision.Author == revAuthorName || (revision.Author == "Unknown" && revAuthorName == string.Empty) || (revision.Author == string.Empty && revAuthorName == "Unknown")))
			{
				return revision;
			}
		}
		return null;
	}

	private Revision GetExistingRevision(List<Revision> prevItemRevisions, RevisionType revisionType, string revAuthorName, Stack<Revision> trackChanges, Entity currItem)
	{
		int num = ((currItem.RevisionsInternal != null) ? currItem.RevisionsInternal.Count : 0);
		Revision revision = null;
		if (prevItemRevisions[num].RevisionType == revisionType && prevItemRevisions[num].Author == revAuthorName)
		{
			revision = prevItemRevisions[num];
		}
		if (revision != null)
		{
			for (int i = num + 1; i < prevItemRevisions.Count; i++)
			{
				foreach (Revision trackChange in trackChanges)
				{
					if (trackChange.RevisionType != prevItemRevisions[i].RevisionType || trackChange.Author != prevItemRevisions[i].Author)
					{
						return null;
					}
				}
			}
		}
		return revision;
	}

	private Revision CreateRevision(bool isChildRevision, RevisionType revisionType, string revAuthorName, DateTime revDateTime, string name, Revision rowRevision, Revision moveRevision, Entity item)
	{
		Revision revision = null;
		if (isChildRevision)
		{
			revision = CreateNewChildRevision(revisionType, revAuthorName, revDateTime, name);
			rowRevision.ChildRevisions.Add(revision);
		}
		else if (moveRevision != null)
		{
			if (moveRevision.RevisionType != revisionType)
			{
				revision = CreateNewRevision(revisionType, revAuthorName, revDateTime, name);
			}
			else
			{
				revision = CreateNewChildRevision(revisionType, revAuthorName, revDateTime, name);
				moveRevision.ChildRevisions.Add(revision);
			}
			moveRevision.Range.Items.Add(item);
		}
		else
		{
			revision = CreateNewRevision(revisionType, revAuthorName, revDateTime, name);
		}
		return revision;
	}

	internal void TableRowRevision(RevisionType revisionType, WTableRow tableRow, WordReaderBase reader)
	{
		Revision revision = null;
		string text = ((ActualFormatType == FormatType.Doc) ? base.Document.GetAuthorName(reader, revisionType == RevisionType.Insertions) : ((revisionType == RevisionType.Formatting) ? tableRow.RowFormat.FormatChangeAuthorName : tableRow.CharacterFormat.AuthorName));
		DateTime dateTime = ((ActualFormatType == FormatType.Doc) ? base.Document.GetDateTime(reader, revisionType == RevisionType.Insertions, tableRow.CharacterFormat) : ((revisionType == RevisionType.Formatting) ? tableRow.RowFormat.FormatChangeDateTime : tableRow.CharacterFormat.RevDateTime));
		if (tableRow.OwnerTable.IsInCell && (tableRow.OwnerTable.OwnerTextBody as WTableCell).OwnerRow.RowFormat.Revisions.Count > 0)
		{
			LinkNestedTableRowRevision(revisionType, tableRow, text, dateTime);
		}
		else if (tableRow.PreviousSibling != null && (tableRow.PreviousSibling as WTableRow).RowFormat.Revisions.Count > 0)
		{
			List<Revision> revisions = (tableRow.PreviousSibling as WTableRow).RowFormat.Revisions;
			revision = GetExistingRevision(revisions, revisionType, text);
			if (revision == null)
			{
				revision = CreateNewRevision(revisionType, text, dateTime, null);
			}
		}
		else if (tableRow.PreviousSibling == null && tableRow.OwnerTable.PreviousSibling != null && tableRow.OwnerTable.PreviousSibling is WParagraph && (tableRow.OwnerTable.PreviousSibling as WParagraph).BreakCharacterFormat.Revisions.Count > 0)
		{
			List<Revision> revisions = (tableRow.OwnerTable.PreviousSibling as WParagraph).BreakCharacterFormat.Revisions;
			revision = GetExistingRevision(revisions, revisionType, text);
			if (revision == null)
			{
				revision = CreateNewRevision(revisionType, text, dateTime, null);
			}
		}
		else if (tableRow.PreviousSibling == null && tableRow.OwnerTable.PreviousSibling != null && tableRow.OwnerTable.PreviousSibling is WTable && (tableRow.OwnerTable.PreviousSibling as WTable).LastRow.RowFormat.Revisions.Count > 0)
		{
			List<Revision> revisions = (tableRow.OwnerTable.PreviousSibling as WTable).LastRow.RowFormat.Revisions;
			revision = GetExistingRevision(revisions, revisionType, text);
			if (revision == null)
			{
				revision = CreateNewRevision(revisionType, text, dateTime, null);
			}
		}
		else
		{
			revision = CreateNewRevision(revisionType, text, dateTime, null);
		}
		if (revision != null)
		{
			tableRow.RowFormat.Revisions.Add(revision);
			revision.Range.InnerList.Add(tableRow.RowFormat);
		}
	}

	private void LinkNestedTableRowRevision(RevisionType revisionType, WTableRow tableRow, string revAuthorName, DateTime revDateTime)
	{
		foreach (Revision revision3 in (tableRow.OwnerTable.OwnerTextBody as WTableCell).OwnerRow.RowFormat.Revisions)
		{
			if (revision3.RevisionType == revisionType)
			{
				if (revision3.Author == revAuthorName)
				{
					tableRow.RowFormat.Revisions.Add(revision3);
					revision3.Range.Items.Add(tableRow.RowFormat);
					continue;
				}
				Revision revision = CreateNewChildRevision(revisionType, revAuthorName, revDateTime, null);
				tableRow.RowFormat.Revisions.Add(revision);
				revision.Range.Items.Add(tableRow.RowFormat);
				revision3.ChildRevisions.Add(revision);
			}
			else
			{
				Revision revision2 = CreateNewRevision(revisionType, revAuthorName, revDateTime, null);
				tableRow.RowFormat.Revisions.Add(revision2);
				revision2.Range.InnerList.Add(tableRow.RowFormat);
			}
		}
	}

	internal Revision CreateNewRevision(RevisionType revisionType, string authorName, DateTime dateTime, string name)
	{
		Revision revision = new Revision(m_doc);
		revision.RevisionType = revisionType;
		if (!string.IsNullOrEmpty(authorName))
		{
			revision.Author = authorName;
		}
		if (dateTime.Year > 1900)
		{
			revision.Date = dateTime;
		}
		if (!string.IsNullOrEmpty(name))
		{
			revision.Name = name;
		}
		Revisions.Add(revision);
		return revision;
	}

	internal Revision CreateNewChildRevision(RevisionType revisionType, string authorName, DateTime dateTime, string name)
	{
		Revision revision = new Revision(m_doc);
		revision.RevisionType = revisionType;
		if (!string.IsNullOrEmpty(authorName))
		{
			revision.Author = authorName;
		}
		if (dateTime.Year > 1900)
		{
			revision.Date = dateTime;
		}
		if (!string.IsNullOrEmpty(name))
		{
			revision.Name = name;
		}
		return revision;
	}

	internal void BreakCharacterFormatRevision(RevisionType revisionType, WCharacterFormat charFormat, Revision moveRevision, WordReaderBase reader)
	{
		Revision revision = null;
		string authorName = charFormat.AuthorName;
		DateTime dateTime = charFormat.RevDateTime;
		string revisionName = charFormat.RevisionName;
		if (reader != null)
		{
			authorName = GetAuthorName(reader, revisionType == RevisionType.Insertions);
			dateTime = GetDateTime(reader, revisionType == RevisionType.Insertions, charFormat);
		}
		if (moveRevision != null)
		{
			LinkMoveRevForBreakCharacterFormat(charFormat, moveRevision, revisionType, authorName, dateTime, revisionName);
			return;
		}
		if (charFormat.OwnerBase is WParagraph && (charFormat.OwnerBase as WParagraph).IsInCell && (((charFormat.OwnerBase as WParagraph).OwnerTextBody.Owner is BlockContentControl) ? (((charFormat.OwnerBase as WParagraph).GetOwnerEntity() as WTableCell).OwnerRow.RowFormat.Revisions.Count > 0) : (((charFormat.OwnerBase as WParagraph).OwnerTextBody as WTableCell).OwnerRow.RowFormat.Revisions.Count > 0)))
		{
			LineTableRevForBreakCharacterFormat(charFormat, moveRevision, revisionType, authorName, dateTime, revisionName);
			return;
		}
		revision = CreateNewRevision(revisionType, authorName, dateTime, revisionName);
		charFormat.Revisions.Add(revision);
		revision.Range.Items.Add(charFormat);
	}

	internal string GetAuthorName(WordReaderBase reader, bool isInsertKey)
	{
		string result = string.Empty;
		short key = (isInsertKey ? reader.CHPXSprms.GetShort(18436, 0) : reader.CHPXSprms.GetShort(18531, 0));
		if (reader.SttbfRMarkAuthorNames != null && reader.SttbfRMarkAuthorNames.Count > 0 && reader.SttbfRMarkAuthorNames.ContainsKey(key))
		{
			result = reader.SttbfRMarkAuthorNames[key];
		}
		return result;
	}

	internal DateTime GetDateTime(WordReaderBase reader, bool isInsertKey, WCharacterFormat charFormat)
	{
		SinglePropertyModifierRecord singlePropertyModifierRecord = (isInsertKey ? reader.CHPXSprms[26629] : reader.CHPXSprms[26724]);
		DateTime result = default(DateTime);
		if (singlePropertyModifierRecord != null)
		{
			result = charFormat.ParseDTTM(singlePropertyModifierRecord.IntValue);
		}
		if (result.Year < 1900)
		{
			result = new DateTime(1900, 1, 1, 0, 0, 0);
		}
		return result;
	}

	private void LineTableRevForBreakCharacterFormat(WCharacterFormat charFormat, Revision moveRevision, RevisionType revisionType, string revAuthorName, DateTime revDateTime, string name)
	{
		foreach (Revision item in ((charFormat.OwnerBase as WParagraph).OwnerTextBody.Owner is BlockContentControl) ? ((charFormat.OwnerBase as WParagraph).GetOwnerEntity() as WTableCell).OwnerRow.RowFormat.Revisions : ((charFormat.OwnerBase as WParagraph).OwnerTextBody as WTableCell).OwnerRow.RowFormat.Revisions)
		{
			if (item.RevisionType == revisionType)
			{
				if (item.Author == revAuthorName)
				{
					charFormat.Revisions.Add(item);
					item.Range.Items.Add(charFormat);
					continue;
				}
				Revision revision = CreateNewChildRevision(revisionType, revAuthorName, revDateTime, name);
				charFormat.Revisions.Add(revision);
				revision.Range.Items.Add(charFormat);
				item.ChildRevisions.Add(revision);
			}
			else
			{
				Revision revision2 = CreateNewRevision(revisionType, revAuthorName, revDateTime, name);
				charFormat.Revisions.Add(revision2);
				revision2.Range.Items.Add(charFormat);
			}
		}
	}

	private void LinkMoveRevForBreakCharacterFormat(WCharacterFormat charFormat, Revision moveRevision, RevisionType revisionType, string revAuthorName, DateTime revDateTime, string name)
	{
		if (moveRevision.RevisionType == revisionType)
		{
			if (moveRevision.Author == revAuthorName)
			{
				charFormat.Revisions.Add(moveRevision);
				moveRevision.Range.Items.Add(charFormat);
				return;
			}
			Revision revision = CreateNewChildRevision(revisionType, revAuthorName, revDateTime, name);
			charFormat.Revisions.Add(revision);
			revision.Range.Items.Add(charFormat);
			moveRevision.ChildRevisions.Add(revision);
		}
		else
		{
			Revision revision2 = CreateNewRevision(revisionType, revAuthorName, revDateTime, name);
			charFormat.Revisions.Add(revision2);
			revision2.Range.Items.Add(charFormat);
		}
	}

	internal void MoveRevisionRanges(Revision sourceRevision, Revision destinationRevision)
	{
		for (int i = 0; i < sourceRevision.Range.InnerList.Count; i++)
		{
			if (sourceRevision.Range.InnerList[i] is FormatBase)
			{
				FormatBase formatBase = sourceRevision.Range.InnerList[i] as FormatBase;
				formatBase.Revisions.Remove(sourceRevision);
				formatBase.Revisions.Add(destinationRevision);
				destinationRevision.Range.Items.Add(formatBase);
			}
			else
			{
				Entity entity = sourceRevision.Range.InnerList[i] as Entity;
				entity.RevisionsInternal.Remove(sourceRevision);
				entity.RevisionsInternal.Add(destinationRevision);
				destinationRevision.Range.Items.Add(entity);
			}
		}
		sourceRevision.Range.Items.Clear();
	}

	internal void ParaFormatChangeRevision(WParagraphFormat paragraphFormat)
	{
		Revision revision = null;
		WParagraph wParagraph = null;
		if (paragraphFormat.OwnerBase is WParagraph)
		{
			wParagraph = paragraphFormat.OwnerBase as WParagraph;
		}
		else
		{
			if (!(paragraphFormat.OwnerBase is Style) || (paragraphFormat.OwnerBase as Style).StyleType != 0)
			{
				return;
			}
			revision = CreateNewRevision(RevisionType.StyleDefinitionChange, paragraphFormat.FormatChangeAuthorName, paragraphFormat.FormatChangeDateTime, null);
		}
		if (wParagraph != null && wParagraph.PreviousSibling != null && wParagraph.PreviousSibling is WParagraph)
		{
			WParagraph wParagraph2 = wParagraph.PreviousSibling as WParagraph;
			if (wParagraph2.ParagraphFormat.Revisions.Count > 0)
			{
				revision = GetSameRevision(wParagraph2.ParagraphFormat, paragraphFormat);
			}
		}
		else if (wParagraph != null && wParagraph.PreviousSibling == null && wParagraph.IsInCell)
		{
			WTableCell ownerTableCell = wParagraph.GetOwnerTableCell(wParagraph.OwnerTextBody);
			if (ownerTableCell != null && ownerTableCell.PreviousSibling != null)
			{
				WTableCell wTableCell = ownerTableCell.PreviousSibling as WTableCell;
				if (wTableCell.LastParagraph != null && wTableCell.LastParagraph.ParagraphFormat.Revisions.Count > 0)
				{
					revision = GetSameRevision(wTableCell.LastParagraph.ParagraphFormat, paragraphFormat);
				}
			}
			else if (ownerTableCell != null && ownerTableCell.OwnerRow.Index == 0 && ownerTableCell.OwnerRow.OwnerTable.PreviousSibling != null && ownerTableCell.OwnerRow.OwnerTable.PreviousSibling is WParagraph && ownerTableCell.OwnerRow.OwnerTable.PreviousSibling is WParagraph wParagraph3 && wParagraph3.ParagraphFormat.Revisions.Count > 0)
			{
				revision = GetSameRevision(wParagraph3.ParagraphFormat, paragraphFormat);
			}
		}
		if (revision == null)
		{
			revision = CreateNewRevision(RevisionType.Formatting, paragraphFormat.FormatChangeAuthorName, paragraphFormat.FormatChangeDateTime, null);
		}
		if (revision != null)
		{
			revision.Range.Items.Add(paragraphFormat);
			paragraphFormat.Revisions.Add(revision);
		}
	}

	internal void SectionFormatChangeRevision(WSection section)
	{
		Revision revision = null;
		string text = (section.SectionFormat.OldPropertiesHash.ContainsKey(5) ? ((string)section.SectionFormat.OldPropertiesHash[5]) : string.Empty);
		DateTime dateTime = (section.SectionFormat.OldPropertiesHash.ContainsKey(6) ? ((DateTime)section.SectionFormat.OldPropertiesHash[6]) : DateTime.MinValue);
		if (section.PreviousSibling != null && section.PreviousSibling is WSection)
		{
			WSection wSection = section.PreviousSibling as WSection;
			if (wSection.SectionFormat.Revisions.Count > 0)
			{
				foreach (Revision revision2 in wSection.SectionFormat.Revisions)
				{
					if (revision2.RevisionType == RevisionType.Formatting && revision2.Author == text && section.SectionFormat.Compare(wSection.SectionFormat))
					{
						revision = revision2;
						break;
					}
				}
			}
		}
		if (revision == null)
		{
			revision = m_doc.CreateNewRevision(RevisionType.Formatting, text, dateTime, null);
		}
		if (revision != null)
		{
			revision.Range.Items.Add(section.SectionFormat);
			section.SectionFormat.Revisions.Add(revision);
		}
	}

	private Revision GetSameRevision(FormatBase previousFormat, FormatBase currentFormat)
	{
		WCharacterFormat wCharacterFormat = ((currentFormat is WCharacterFormat) ? (currentFormat as WCharacterFormat) : null);
		WParagraphFormat wParagraphFormat = ((currentFormat is WParagraphFormat) ? (currentFormat as WParagraphFormat) : null);
		foreach (Revision revision in previousFormat.Revisions)
		{
			bool flag = revision.RevisionType == RevisionType.Formatting;
			if (wCharacterFormat != null && flag && revision.Author == wCharacterFormat.FormatChangeAuthorName && wCharacterFormat.Compare(previousFormat as WCharacterFormat))
			{
				return revision;
			}
			if (wParagraphFormat != null && flag && revision.Author == wParagraphFormat.FormatChangeAuthorName && wParagraphFormat.Compare(previousFormat as WParagraphFormat))
			{
				return revision;
			}
		}
		return null;
	}

	internal void CharFormatChangeRevision(WCharacterFormat charFormat, ParagraphItem item)
	{
		if (charFormat.OwnerBase == null || (charFormat.OwnerBase is Style && (charFormat.OwnerBase as Style).StyleType != 0 && (charFormat.OwnerBase as Style).StyleType != StyleType.CharacterStyle))
		{
			return;
		}
		Revision revision = null;
		string formatChangeAuthorName = charFormat.FormatChangeAuthorName;
		DateTime formatChangeDateTime = charFormat.FormatChangeDateTime;
		if (item != null)
		{
			if (!HasRenderableItemBefore(item))
			{
				revision = GetCharRevisionForFirstParaItem(item, charFormat);
			}
			else if (item.PreviousSibling != null && item.PreviousSibling is Entity)
			{
				Entity entity = item.PreviousSibling as Entity;
				while (entity is BookmarkStart || entity is BookmarkEnd || entity is EditableRangeStart || entity is EditableRangeEnd)
				{
					entity = entity.PreviousSibling as Entity;
				}
				if (entity != null && (entity as ParagraphItem).GetCharFormat().Revisions.Count > 0)
				{
					revision = GetSameRevision((entity as ParagraphItem).GetCharFormat(), charFormat);
				}
			}
		}
		else if (charFormat.OwnerBase is Style && ((charFormat.OwnerBase as Style).StyleType == StyleType.ParagraphStyle || (charFormat.OwnerBase as Style).StyleType == StyleType.CharacterStyle))
		{
			revision = GetCharRevisionForStyle(charFormat, formatChangeAuthorName, formatChangeDateTime);
		}
		else if (charFormat.OwnerBase is WParagraph)
		{
			revision = GetPreviousBreakCharFmtChange(charFormat.OwnerBase as WParagraph, charFormat);
		}
		if (revision == null)
		{
			revision = CreateNewRevision(RevisionType.Formatting, formatChangeAuthorName, formatChangeDateTime, null);
		}
		revision.Range.Items.Add(charFormat);
		charFormat.Revisions.Add(revision);
	}

	private Revision GetCharRevisionForStyle(WCharacterFormat charFormat, string formatRevAuthorName, DateTime formatRevDateTime)
	{
		Revision revision = null;
		Style style = charFormat.OwnerBase as Style;
		if (style.StyleType == StyleType.ParagraphStyle)
		{
			WParagraphFormat paragraphFormat = GetParagraphFormat(style);
			if (paragraphFormat.Revisions.Count > 0)
			{
				using List<Revision>.Enumerator enumerator = paragraphFormat.Revisions.GetEnumerator();
				if (enumerator.MoveNext())
				{
					Revision current = enumerator.Current;
					if (current.Author == formatRevAuthorName)
					{
						revision = current;
					}
					else
					{
						revision = CreateNewRevision(RevisionType.StyleDefinitionChange, formatRevAuthorName, formatRevDateTime, null);
						Revisions.Remove(current);
						MoveRevisionRanges(current, revision);
					}
				}
			}
		}
		return revision;
	}

	private Revision GetCharRevisionForFirstParaItem(ParagraphItem item, WCharacterFormat charFormat)
	{
		Revision result = null;
		WParagraph ownerParagraphValue = item.GetOwnerParagraphValue();
		if (ownerParagraphValue != null && ownerParagraphValue.PreviousSibling is WParagraph)
		{
			if (ownerParagraphValue.PreviousSibling is WParagraph wParagraph && wParagraph.BreakCharacterFormat.Revisions.Count > 0)
			{
				result = GetSameRevision(wParagraph.BreakCharacterFormat, charFormat);
			}
		}
		else
		{
			result = GetPreviousBreakCharFmtChange(ownerParagraphValue, charFormat);
		}
		return result;
	}

	private Revision GetPreviousBreakCharFmtChange(WParagraph paragraph, WCharacterFormat charFormat)
	{
		Revision result = null;
		if (paragraph != null && paragraph.PreviousSibling == null && paragraph.IsInCell)
		{
			WTableCell ownerTableCell = paragraph.GetOwnerTableCell(paragraph.OwnerTextBody);
			if (ownerTableCell != null && ownerTableCell.PreviousSibling != null)
			{
				WTableCell wTableCell = ownerTableCell.PreviousSibling as WTableCell;
				if (wTableCell.LastParagraph != null && wTableCell.LastParagraph.BreakCharacterFormat.Revisions.Count > 0)
				{
					result = GetSameRevision(wTableCell.LastParagraph.BreakCharacterFormat, charFormat);
				}
			}
			else if (ownerTableCell != null && ownerTableCell.OwnerRow.Index == 0 && ownerTableCell.OwnerRow.OwnerTable.PreviousSibling != null && ownerTableCell.OwnerRow.OwnerTable.PreviousSibling is WParagraph && ownerTableCell.OwnerRow.OwnerTable.PreviousSibling is WParagraph wParagraph && wParagraph.BreakCharacterFormat.Revisions.Count > 0)
			{
				result = GetSameRevision(wParagraph.BreakCharacterFormat, charFormat);
			}
		}
		return result;
	}

	private WParagraphFormat GetParagraphFormat(Style style)
	{
		if (style.StyleType != StyleType.TableStyle)
		{
			if (style.StyleType != StyleType.NumberingStyle)
			{
				return (style as WParagraphStyle).ParagraphFormat;
			}
			return (style as WNumberingStyle).ParagraphFormat;
		}
		return (style as WTableStyle).ParagraphFormat;
	}

	internal bool HasRenderableItemBefore(ParagraphItem item)
	{
		Entity entity = item.PreviousSibling as Entity;
		if (entity == null && item.Owner is InlineContentControl)
		{
			entity = (item.Owner as InlineContentControl).PreviousSibling as Entity;
		}
		do
		{
			entity = ((!(entity is InlineContentControl)) ? entity : (((entity as InlineContentControl).ParagraphItems.Count > 0) ? (entity as InlineContentControl).ParagraphItems.LastItem : entity));
			while (entity is BookmarkStart || entity is BookmarkEnd || entity is EditableRangeStart || entity is EditableRangeEnd || entity is WCommentMark)
			{
				entity = entity.PreviousSibling as Entity;
			}
		}
		while (entity is InlineContentControl && (entity as InlineContentControl).ParagraphItems.Count > 0);
		if (entity != null)
		{
			return true;
		}
		return false;
	}

	internal void UpdateTableFormatRevision(WTableRow row)
	{
		Revision rowFormattingRevision = GetRowFormattingRevision(row);
		Revision revision = null;
		WTableRow wTableRow = row.PreviousSibling as WTableRow;
		if (wTableRow != null && row.OwnerTable.PreviousSibling is WTable)
		{
			wTableRow = (row.OwnerTable.PreviousSibling as WTable).LastRow;
		}
		if (wTableRow != null)
		{
			revision = GetRowFormattingRevision(wTableRow);
		}
		if (rowFormattingRevision != null && revision != null && rowFormattingRevision.Author == revision.Author && rowFormattingRevision != revision)
		{
			Revisions.Remove(revision);
			MoveRevisionRanges(revision, rowFormattingRevision);
		}
		if (row.Index != 0 || row.OwnerTable.DocxTableFormat.Format.Revisions.Count <= 0)
		{
			return;
		}
		foreach (Revision revision2 in row.OwnerTable.DocxTableFormat.Format.Revisions)
		{
			if (rowFormattingRevision != null && revision2.RevisionType == rowFormattingRevision.RevisionType)
			{
				Revisions.Remove(revision2);
				MoveRevisionRanges(revision2, rowFormattingRevision);
				break;
			}
			if (rowFormattingRevision == null)
			{
				Revisions.Remove(revision2);
				row.OwnerTable.DocxTableFormat.Format.Revisions.Remove(revision2);
				break;
			}
		}
	}

	internal void UpdateRowFormatRevision(RowFormat rowFormat)
	{
		Revision revision = null;
		string formatChangeAuthorName = rowFormat.FormatChangeAuthorName;
		DateTime formatChangeDateTime = rowFormat.FormatChangeDateTime;
		if (rowFormat.Revisions.Count > 0)
		{
			foreach (Revision revision3 in rowFormat.Revisions)
			{
				if (revision3.RevisionType == RevisionType.Formatting)
				{
					revision = revision3;
					break;
				}
			}
		}
		if (revision != null)
		{
			if (revision.Author != formatChangeAuthorName)
			{
				Revision destinationRevision = CreateNewRevision(RevisionType.Formatting, formatChangeAuthorName, formatChangeDateTime, null);
				Revisions.Remove(revision);
				MoveRevisionRanges(revision, destinationRevision);
			}
		}
		else
		{
			Revision revision2 = CreateNewRevision(RevisionType.Formatting, formatChangeAuthorName, formatChangeDateTime, null);
			rowFormat.Revisions.Add(revision2);
			revision2.Range.Items.Add(rowFormat);
		}
	}

	internal void UpdateCellFormatRevision(WTableCell tableCell)
	{
		Revision revision = null;
		string formatChangeAuthorName = tableCell.CellFormat.FormatChangeAuthorName;
		DateTime formatChangeDateTime = tableCell.CellFormat.FormatChangeDateTime;
		for (int num = tableCell.Index - 1; num >= 0; num--)
		{
			WTableCell wTableCell = tableCell.OwnerRow.Cells[num];
			if (wTableCell.CellFormat.Revisions.Count > 0)
			{
				foreach (Revision revision4 in wTableCell.CellFormat.Revisions)
				{
					if (revision4.RevisionType == RevisionType.Formatting)
					{
						revision = revision4;
						break;
					}
				}
			}
			if (revision != null)
			{
				break;
			}
		}
		if (revision == null && tableCell.OwnerRow.RowFormat.Revisions.Count > 0)
		{
			foreach (Revision revision5 in tableCell.OwnerRow.RowFormat.Revisions)
			{
				if (revision5.RevisionType == RevisionType.Formatting)
				{
					revision = revision5;
					break;
				}
			}
		}
		if (revision != null)
		{
			if (revision.Author == formatChangeAuthorName)
			{
				tableCell.CellFormat.Revisions.Add(revision);
				revision.Range.Items.Add(tableCell.CellFormat);
				return;
			}
			Revision revision2 = CreateNewRevision(RevisionType.Formatting, formatChangeAuthorName, formatChangeDateTime, null);
			tableCell.CellFormat.Revisions.Add(revision2);
			revision2.Range.Items.Add(tableCell.CellFormat);
			Revisions.Remove(revision);
			MoveRevisionRanges(revision, revision2);
		}
		else
		{
			Revision revision3 = CreateNewRevision(RevisionType.Formatting, formatChangeAuthorName, formatChangeDateTime, null);
			tableCell.CellFormat.Revisions.Add(revision3);
			revision3.Range.Items.Add(tableCell.CellFormat);
		}
	}

	private Revision GetRowFormattingRevision(WTableRow row)
	{
		if (row.RowFormat.Revisions.Count > 0)
		{
			foreach (Revision revision in row.RowFormat.Revisions)
			{
				if (revision.RevisionType == RevisionType.Formatting)
				{
					return revision;
				}
			}
		}
		for (int i = 0; i < row.Cells.Count; i++)
		{
			WTableCell wTableCell = row.Cells[i];
			if (wTableCell.CellFormat.Revisions.Count <= 0)
			{
				continue;
			}
			foreach (Revision revision2 in wTableCell.CellFormat.Revisions)
			{
				if (revision2.RevisionType == RevisionType.Formatting)
				{
					return revision2;
				}
			}
		}
		return null;
	}

	internal void UpdateFieldRevision(WField field)
	{
		if (field.RevisionsInternal.Count <= 0)
		{
			return;
		}
		Stack<WField> nestedField = new Stack<WField>();
		foreach (Revision item in field.RevisionsInternal)
		{
			if (field.Range.Items.Count == 0)
			{
				field.UpdateFieldRange();
			}
			WParagraph ownerParagraphValue = field.GetOwnerParagraphValue();
			if (field.FieldEnd.OwnerParagraph != ownerParagraphValue)
			{
				RemoveFieldRevisions(ownerParagraphValue.BreakCharacterFormat.Revisions, null, nestedField, ownerParagraphValue.BreakCharacterFormat, item);
			}
			for (int i = 0; i < field.Range.Items.Count; i++)
			{
				Entity entity = field.Range.Items[i] as Entity;
				if (entity is ParagraphItem)
				{
					RemoveFieldRangeRevision(entity as ParagraphItem, nestedField, item, field);
				}
				else if (entity is TextBodyItem)
				{
					RemoveFieldRangeRevision(entity as TextBodyItem, nestedField, item, field);
				}
			}
			Revision current = null;
			field.Range.Items.Clear();
		}
	}

	private void RemoveFieldRangeRevision(TextBodyItem bodyItemEntity, Stack<WField> nestedField, Revision fieldRevision, WField CurrentField)
	{
		switch (bodyItemEntity.EntityType)
		{
		case EntityType.Paragraph:
		{
			WParagraph wParagraph = bodyItemEntity as WParagraph;
			if (CurrentField.FieldEnd.OwnerParagraph != wParagraph)
			{
				RemoveFieldRevisions(wParagraph.BreakCharacterFormat.Revisions, null, nestedField, wParagraph.BreakCharacterFormat, fieldRevision);
			}
			RemoveFieldRangeRevision(wParagraph.Items, nestedField, fieldRevision, CurrentField);
			break;
		}
		case EntityType.Table:
			RemoveFieldRangeRevision(bodyItemEntity as WTable, nestedField, fieldRevision);
			break;
		case EntityType.BlockContentControl:
		{
			BlockContentControl blockContentControl = bodyItemEntity as BlockContentControl;
			RemoveFieldRangeRevision(blockContentControl.TextBody, nestedField, fieldRevision, CurrentField);
			break;
		}
		}
	}

	private void RemoveFieldRangeRevision(ParagraphItem paraItem, Stack<WField> nestedField, Revision fieldRevision, WField CurrentField)
	{
		if (paraItem is InlineContentControl)
		{
			InlineContentControl inlineContentControl = paraItem as InlineContentControl;
			RemoveFieldRangeRevision(inlineContentControl.ParagraphItems, nestedField, fieldRevision, CurrentField);
		}
		else if (paraItem is WTextBox)
		{
			WTextBox wTextBox = paraItem as WTextBox;
			RemoveFieldRangeRevision(wTextBox.TextBoxBody, nestedField, fieldRevision, CurrentField);
		}
		else if (paraItem is Shape)
		{
			Shape shape = paraItem as Shape;
			RemoveFieldRangeRevision(shape.TextBody, nestedField, fieldRevision, CurrentField);
		}
		else if (paraItem.RevisionsInternal.Count > 0)
		{
			RemoveFieldRevisions(paraItem.RevisionsInternal, paraItem, nestedField, null, fieldRevision);
		}
	}

	private void RemoveFieldRangeRevision(WTextBody textBody, Stack<WField> nestedField, Revision fieldRevision, WField CurrentField)
	{
		for (int i = 0; i < textBody.ChildEntities.Count; i++)
		{
			IEntity entity = textBody.ChildEntities[i];
			switch (entity.EntityType)
			{
			case EntityType.Paragraph:
			{
				WParagraph wParagraph = entity as WParagraph;
				if (CurrentField.FieldEnd.OwnerParagraph != wParagraph)
				{
					RemoveFieldRevisions(wParagraph.BreakCharacterFormat.Revisions, null, nestedField, wParagraph.BreakCharacterFormat, fieldRevision);
				}
				RemoveFieldRangeRevision(wParagraph.Items, nestedField, fieldRevision, CurrentField);
				break;
			}
			case EntityType.Table:
				RemoveFieldRangeRevision(entity as WTable, nestedField, fieldRevision);
				break;
			case EntityType.BlockContentControl:
			{
				BlockContentControl blockContentControl = entity as BlockContentControl;
				RemoveFieldRangeRevision(blockContentControl.TextBody, nestedField, fieldRevision, CurrentField);
				break;
			}
			}
		}
	}

	private void RemoveFieldRangeRevision(ParagraphItemCollection paraItems, Stack<WField> nestedField, Revision fieldRevision, WField CurrentField)
	{
		for (int i = 0; i < paraItems.Count; i++)
		{
			if (paraItems[i] is InlineContentControl)
			{
				InlineContentControl inlineContentControl = paraItems[i] as InlineContentControl;
				RemoveFieldRangeRevision(inlineContentControl.ParagraphItems, nestedField, fieldRevision, CurrentField);
			}
			else if (paraItems[i] is WTextBox)
			{
				WTextBox wTextBox = paraItems[i] as WTextBox;
				RemoveFieldRangeRevision(wTextBox.TextBoxBody, nestedField, fieldRevision, CurrentField);
			}
			else if (paraItems[i] is Shape)
			{
				Shape shape = paraItems[i] as Shape;
				RemoveFieldRangeRevision(shape.TextBody, nestedField, fieldRevision, CurrentField);
			}
			else if (paraItems[i].RevisionsInternal.Count > 0)
			{
				RemoveFieldRevisions(paraItems[i].RevisionsInternal, paraItems[i], nestedField, null, fieldRevision);
			}
		}
	}

	private void RemoveFieldRangeRevision(WTable table, Stack<WField> nestedField, Revision fieldRevision)
	{
		foreach (WTableRow row in table.Rows)
		{
			RemoveFieldRevisions(row.RowFormat.Revisions, null, nestedField, row.RowFormat, fieldRevision);
		}
	}

	private void RemoveFieldRevisions(List<Revision> revisions, Entity entity, Stack<WField> nestedField, FormatBase format, Revision fieldRevision)
	{
		foreach (Revision revision in revisions)
		{
			if ((nestedField.Count == 0 || nestedField.Peek().RevisionsInternal.Count == 0) && fieldRevision != null && fieldRevision.RevisionType == revision.RevisionType)
			{
				if (fieldRevision == revision)
				{
					break;
				}
				if (Revisions.InnerList.Contains(revision))
				{
					Revisions.Remove(revision);
				}
				if (fieldRevision.Author != revision.Author)
				{
					bool flag = false;
					foreach (Revision childRevision in fieldRevision.ChildRevisions)
					{
						if (childRevision == revision)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						fieldRevision.ChildRevisions.Add(revision);
					}
				}
				else if (entity != null)
				{
					entity.RevisionsInternal.Remove(revision);
					entity.RevisionsInternal.Add(fieldRevision);
					fieldRevision.Range.InnerList.Add(entity);
				}
				else if (format is RowFormat)
				{
					MoveRevisionRanges(revision, fieldRevision);
				}
				else
				{
					format.Revisions.Remove(revision);
					format.Revisions.Add(fieldRevision);
					fieldRevision.Range.InnerList.Add(format);
				}
				break;
			}
			if (entity is WField)
			{
				nestedField.Push(entity as WField);
			}
			else if (nestedField.Count > 0 && entity is WFieldMark && (entity as WFieldMark).Type == FieldMarkType.FieldEnd)
			{
				nestedField.Pop();
			}
		}
	}

	internal void UpdateTableRowRevision(WTableRow tableRow)
	{
		if (tableRow.RowFormat.Revisions.Count <= 0)
		{
			return;
		}
		foreach (Revision revision in tableRow.RowFormat.Revisions)
		{
			if (revision.RevisionType == RevisionType.Insertions || revision.RevisionType == RevisionType.Deletions)
			{
				Revision rowRevision = revision;
				for (int i = 0; i < tableRow.Cells.Count; i++)
				{
					WTableCell textBody = tableRow.Cells[i];
					RemoveTableCellRevision(textBody, rowRevision);
				}
				rowRevision = null;
			}
		}
	}

	private void RemoveTableCellRevision(WTextBody textBody, Revision rowRevision)
	{
		for (int i = 0; i < textBody.ChildEntities.Count; i++)
		{
			IEntity entity = textBody.ChildEntities[i];
			switch (entity.EntityType)
			{
			case EntityType.Paragraph:
			{
				WParagraph wParagraph = entity as WParagraph;
				RemoveTableCellRevision(wParagraph.BreakCharacterFormat.Revisions, null, wParagraph.BreakCharacterFormat, rowRevision);
				RemoveTableCellRevision(wParagraph.Items, rowRevision);
				break;
			}
			case EntityType.Table:
				RemoveTableCellRevision(entity as WTable, rowRevision);
				break;
			case EntityType.BlockContentControl:
			{
				BlockContentControl blockContentControl = entity as BlockContentControl;
				RemoveTableCellRevision(blockContentControl.TextBody, rowRevision);
				break;
			}
			}
		}
	}

	private void RemoveTableCellRevision(ParagraphItemCollection paraItems, Revision rowRevision)
	{
		for (int i = 0; i < paraItems.Count; i++)
		{
			if (paraItems[i] is InlineContentControl)
			{
				InlineContentControl inlineContentControl = paraItems[i] as InlineContentControl;
				RemoveTableCellRevision(inlineContentControl.ParagraphItems, rowRevision);
			}
			else if (paraItems[i] is WTextBox)
			{
				WTextBox wTextBox = paraItems[i] as WTextBox;
				RemoveTableCellRevision(wTextBox.TextBoxBody, rowRevision);
			}
			else if (paraItems[i] is Shape)
			{
				Shape shape = paraItems[i] as Shape;
				RemoveTableCellRevision(shape.TextBody, rowRevision);
			}
			else if (paraItems[i].RevisionsInternal.Count > 0)
			{
				RemoveTableCellRevision(paraItems[i].RevisionsInternal, paraItems[i], null, rowRevision);
			}
		}
	}

	private void RemoveTableCellRevision(WTable table, Revision rowRevision)
	{
		foreach (WTableRow row in table.Rows)
		{
			RemoveTableCellRevision(row.RowFormat.Revisions, null, row.RowFormat, rowRevision);
		}
	}

	private void RemoveTableCellRevision(List<Revision> revisions, Entity entity, FormatBase format, Revision rowRevision)
	{
		foreach (Revision revision in revisions)
		{
			if (rowRevision == null || rowRevision.RevisionType != revision.RevisionType)
			{
				continue;
			}
			if (rowRevision == revision)
			{
				break;
			}
			if (Revisions.InnerList.Contains(revision))
			{
				Revisions.Remove(revision);
			}
			if (rowRevision.Author != revision.Author)
			{
				bool flag = false;
				foreach (Revision childRevision in rowRevision.ChildRevisions)
				{
					if (childRevision == revision)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					rowRevision.ChildRevisions.Add(revision);
				}
			}
			else if (entity != null)
			{
				entity.RevisionsInternal.Remove(revision);
				entity.RevisionsInternal.Add(rowRevision);
				rowRevision.Range.InnerList.Add(entity);
			}
			else if (format is RowFormat)
			{
				MoveRevisionRanges(revision, rowRevision);
			}
			else
			{
				format.Revisions.Remove(revision);
				format.Revisions.Add(rowRevision);
				rowRevision.Range.InnerList.Add(format);
			}
			break;
		}
	}

	internal void UpdateLastItemRevision(IWParagraph paragraph, ParagraphItemCollection items)
	{
		if ((paragraph as WParagraph).IsEmptyParagraph())
		{
			LinkEmptyParaBreakCharacterFormat(paragraph, items);
			return;
		}
		List<Revision> revisions = paragraph.BreakCharacterFormat.Revisions;
		Entity entity = items.LastItem;
		while (entity is BookmarkStart || entity is BookmarkEnd || entity is EditableRangeStart || entity is EditableRangeEnd)
		{
			entity = entity.PreviousSibling as Entity;
		}
		if (revisions.Count > 0 && entity != null && entity.RevisionsInternal.Count > 0)
		{
			LinkLastItemWithBreakCharFormat(paragraph, revisions, entity.RevisionsInternal);
		}
		else if (revisions.Count > 0 && entity is WFieldMark && (entity as WFieldMark).Type == FieldMarkType.FieldEnd && (entity as WFieldMark).ParentField.RevisionsInternal.Count > 0)
		{
			LinkLastItemWithBreakCharFormat(paragraph, revisions, (entity as WFieldMark).ParentField.RevisionsInternal);
		}
		else if (revisions.Count > 0 && entity is InlineContentControl)
		{
			LinkContentControlWithBreakCharFormat(entity, revisions);
		}
		WCharacterFormat wCharacterFormat = null;
		if (entity == null || (wCharacterFormat = (entity as ParagraphItem).GetCharFormat()) == null || !wCharacterFormat.HasKey(105) || !paragraph.BreakCharacterFormat.HasKey(105) || revisions.Count <= 0 || wCharacterFormat.Revisions.Count <= 0)
		{
			return;
		}
		for (int i = 0; i > wCharacterFormat.Revisions.Count; i++)
		{
			Revision revision = wCharacterFormat.Revisions[i];
			foreach (Revision item in revisions)
			{
				if (revision != item && revision.RevisionType == item.RevisionType && revision.Author == item.Author && wCharacterFormat.Compare(paragraph.BreakCharacterFormat))
				{
					item.RemoveSelf();
					MoveRevisionRanges(item, revision);
					break;
				}
			}
		}
	}

	private void LinkContentControlWithBreakCharFormat(Entity item, List<Revision> breakRevisions)
	{
		Entity entity = (item as InlineContentControl).ParagraphItems.LastItem;
		do
		{
			entity = ((entity is InlineContentControl) ? (entity as InlineContentControl).ParagraphItems.LastItem : entity);
			while (entity is BookmarkStart || entity is BookmarkEnd || entity is EditableRangeStart || entity is EditableRangeEnd)
			{
				entity = entity.PreviousSibling as Entity;
			}
		}
		while (entity is InlineContentControl);
		if (entity == null || entity.RevisionsInternal.Count <= 0)
		{
			return;
		}
		foreach (Revision item2 in entity.RevisionsInternal)
		{
			foreach (Revision breakRevision in breakRevisions)
			{
				if (item2 != breakRevision && item2.RevisionType == breakRevision.RevisionType && item2.Author == breakRevision.Author)
				{
					breakRevision.RemoveSelf();
					MoveRevisionRanges(breakRevision, item2);
					break;
				}
			}
		}
	}

	private void LinkLastItemWithBreakCharFormat(IWParagraph paragraph, List<Revision> breakRevisions, List<Revision> itemRevisions)
	{
		foreach (Revision itemRevision in itemRevisions)
		{
			foreach (Revision breakRevision in breakRevisions)
			{
				if (itemRevision != breakRevision && itemRevision.RevisionType == breakRevision.RevisionType && (itemRevision.Author == breakRevision.Author || (itemRevision.Author == string.Empty && breakRevision.Author == "Unknown") || (itemRevision.Author == "Unknown" && breakRevision.Author == string.Empty)))
				{
					breakRevision.RemoveSelf();
					MoveRevisionRanges(breakRevision, itemRevision);
					break;
				}
			}
		}
	}

	internal void LinkEmptyParaBreakCharacterFormat(IWParagraph paragraph, ParagraphItemCollection items)
	{
		if (paragraph.BreakCharacterFormat.Revisions.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < paragraph.BreakCharacterFormat.Revisions.Count; i++)
		{
			Revision revision = paragraph.BreakCharacterFormat.Revisions[i];
			if (paragraph.PreviousSibling == null)
			{
				continue;
			}
			Entity entity = paragraph.PreviousSibling as Entity;
			if (entity is WParagraph)
			{
				WParagraph wParagraph = entity as WParagraph;
				if (wParagraph.BreakCharacterFormat.Revisions.Count <= 0)
				{
					continue;
				}
				foreach (Revision revision2 in wParagraph.BreakCharacterFormat.Revisions)
				{
					if (revision2 != revision && revision2.RevisionType == revision.RevisionType && (revision2.Author == revision.Author || (revision2.Author == string.Empty && revision.Author == "Unknown") || (revision2.Author == "Unknown" && revision.Author == string.Empty)) && (revision2.RevisionType != RevisionType.Formatting || paragraph.BreakCharacterFormat.Compare(wParagraph.BreakCharacterFormat)))
					{
						revision.RemoveSelf();
						MoveRevisionRanges(revision, revision2);
						break;
					}
				}
			}
			else
			{
				if (!(entity is WTable))
				{
					continue;
				}
				WTable wTable = entity as WTable;
				if (wTable.LastRow == null || wTable.LastRow.RowFormat.Revisions.Count <= 0)
				{
					continue;
				}
				foreach (Revision revision3 in wTable.LastRow.RowFormat.Revisions)
				{
					if (revision3 != revision && revision3.RevisionType != RevisionType.Formatting && revision3.RevisionType == revision.RevisionType && revision3.Author == revision.Author)
					{
						revision.RemoveSelf();
						MoveRevisionRanges(revision, revision3);
						break;
					}
				}
			}
		}
	}

	internal void RemoveRevisionFromCollection(WTextBody textBody)
	{
		for (int i = 0; i < textBody.ChildEntities.Count; i++)
		{
			IEntity entity = textBody.ChildEntities[i];
			switch (entity.EntityType)
			{
			case EntityType.Paragraph:
			{
				WParagraph wParagraph = entity as WParagraph;
				if (wParagraph.BreakCharacterFormat.Revisions.Count > 0)
				{
					RemoveRevisions(wParagraph.BreakCharacterFormat.Revisions);
				}
				if (wParagraph.ParagraphFormat.Revisions.Count > 0)
				{
					RemoveRevisions(wParagraph.BreakCharacterFormat.Revisions);
				}
				RemoveRevisionFromCollection(wParagraph.Items);
				break;
			}
			case EntityType.Table:
				RemoveRevisionFromCollection(entity as WTable);
				break;
			case EntityType.BlockContentControl:
			{
				BlockContentControl blockContentControl = entity as BlockContentControl;
				RemoveRevisionFromCollection(blockContentControl.TextBody);
				break;
			}
			}
		}
	}

	private void RemoveRevisionFromCollection(ParagraphItemCollection paraItems)
	{
		for (int i = 0; i < paraItems.Count; i++)
		{
			if (paraItems[i] is InlineContentControl)
			{
				InlineContentControl inlineContentControl = paraItems[i] as InlineContentControl;
				RemoveRevisionFromCollection(inlineContentControl.ParagraphItems);
			}
			else if (paraItems[i] is WTextBox)
			{
				WTextBox wTextBox = paraItems[i] as WTextBox;
				RemoveRevisionFromCollection(wTextBox.TextBoxBody);
			}
			else if (paraItems[i] is Shape)
			{
				Shape shape = paraItems[i] as Shape;
				RemoveRevisionFromCollection(shape.TextBody);
			}
			else if (paraItems[i].RevisionsInternal.Count > 0)
			{
				if (paraItems[i] is WTextRange && (paraItems[i] as WTextRange).CharacterFormat.Revisions.Count > 0)
				{
					RemoveRevisions((paraItems[i] as WTextRange).CharacterFormat.Revisions);
				}
				RemoveRevisions(paraItems[i].RevisionsInternal);
			}
		}
	}

	private void RemoveRevisionFromCollection(WTable table)
	{
		if (table.TableFormat.Revisions.Count > 0)
		{
			RemoveRevisions(table.TableFormat.Revisions);
		}
		foreach (WTableRow row in table.Rows)
		{
			if (row.RowFormat.Revisions.Count > 0)
			{
				RemoveRevisions(row.RowFormat.Revisions);
			}
			foreach (WTableCell cell in row.Cells)
			{
				if (cell.CellFormat.Revisions.Count > 0)
				{
					RemoveRevisions(cell.CellFormat.Revisions);
				}
				RemoveRevisionFromCollection(cell);
			}
		}
	}

	private void RemoveRevisions(List<Revision> revisions)
	{
		foreach (Revision revision in revisions)
		{
			if (Revisions.InnerList.Contains(revision))
			{
				Revisions.Remove(revision);
			}
		}
	}

	internal void CharacterFormatChange(WCharacterFormat charFormat, ParagraphItem item, WordReaderBase reader)
	{
		if (charFormat.HasBoolKey(105))
		{
			CharFormatChangeRevision(charFormat, item);
		}
		if (item != null)
		{
			return;
		}
		if (charFormat.m_clonedRevisions != null)
		{
			foreach (Revision clonedRevision in charFormat.m_clonedRevisions)
			{
				if (clonedRevision.RevisionType != RevisionType.Formatting)
				{
					BreakCharacterFormatRevision(clonedRevision.RevisionType, charFormat, null, reader);
				}
			}
			charFormat.m_clonedRevisions.Clear();
			charFormat.m_clonedRevisions = null;
		}
		else
		{
			if (charFormat.IsInsertRevision)
			{
				BreakCharacterFormatRevision(RevisionType.Insertions, charFormat, null, reader);
			}
			if (charFormat.IsDeleteRevision)
			{
				BreakCharacterFormatRevision(RevisionType.Deletions, charFormat, null, reader);
			}
		}
	}

	internal void ParagraphFormatChange(WParagraphFormat paraFormat)
	{
		if (paraFormat.HasBoolKey(65))
		{
			ParaFormatChangeRevision(paraFormat);
		}
	}

	internal void SectionFormatChange(WSection section)
	{
		if (section.SectionFormat.HasKey(4))
		{
			SectionFormatChangeRevision(section);
		}
	}

	internal void UpdateTableRevision(WTable table)
	{
		Revision revision = m_doc.CreateNewRevision(RevisionType.Formatting, table.DocxTableFormat.Format.FormatChangeAuthorName, table.DocxTableFormat.Format.FormatChangeDateTime, null);
		table.DocxTableFormat.Format.Revisions.Add(revision);
		revision.Range.Items.Add(table.DocxTableFormat.Format);
	}

	internal static Encoding GetEncoding(string codePageName)
	{
		try
		{
			return Encoding.GetEncoding(codePageName);
		}
		catch (Exception)
		{
			if (codePageName == "Windows-1252")
			{
				return new Windows1252Encoding();
			}
			return new DocGen.DocIO.DLS.Convertors.ASCIIEncoding();
		}
	}

	public WTableStyle AddTableStyle(string styleName)
	{
		return AddStyle(StyleType.TableStyle, styleName) as WTableStyle;
	}

	public Entity FindItemByProperty(EntityType entityType, string propertyName, string propertyValue)
	{
		string[] propertyNames = new string[1] { propertyName };
		string[] propertyValues = new string[1] { propertyValue };
		return FindItemByProperties(entityType, propertyNames, propertyValues);
	}

	public Entity FindItemByProperties(EntityType entityType, string[] propertyNames, string[] propertyValues)
	{
		if (IsUnSupportedEntityType(entityType))
		{
			return null;
		}
		if (entityType == EntityType.Undefined)
		{
			List<Entity> list = FindWaterMarks(propertyNames, propertyValues, onlyFirstMatch: true);
			if (list.Count > 0)
			{
				return list[0];
			}
			return null;
		}
		List<Entity> list2 = IterateSection(entityType, propertyNames, propertyValues, onlyFirstMatch: true);
		if (list2.Count > 0)
		{
			return list2[0];
		}
		return null;
	}

	public List<Entity> FindAllItemsByProperty(EntityType entityType, string propertyName, string propertyValue)
	{
		string[] propertyNames = new string[1] { propertyName };
		string[] propertyValues = new string[1] { propertyValue };
		return FindAllItemsByProperties(entityType, propertyNames, propertyValues);
	}

	public List<Entity> FindAllItemsByProperties(EntityType entityType, string[] propertyNames, string[] propertyValues)
	{
		if (IsUnSupportedEntityType(entityType))
		{
			return null;
		}
		if (entityType == EntityType.Undefined)
		{
			List<Entity> list = FindWaterMarks(propertyNames, propertyValues, onlyFirstMatch: false);
			if (list.Count > 0)
			{
				return list;
			}
			return null;
		}
		List<Entity> list2 = IterateSection(entityType, propertyNames, propertyValues, onlyFirstMatch: false);
		if (list2.Count > 0)
		{
			return list2;
		}
		return null;
	}

	private List<Entity> IterateSection(EntityType entityType, string[] propertyNames, string[] propertyValues, bool onlyFirstMatch)
	{
		List<Entity> list = new List<Entity>();
		foreach (WSection section in Sections)
		{
			if (IsEntityEquals(section, entityType, propertyNames, propertyValues))
			{
				list.Add(section);
			}
			else
			{
				foreach (WTextBody childEntity in section.ChildEntities)
				{
					if (IsEntityEquals(childEntity, entityType, propertyNames, propertyValues))
					{
						list.Add(childEntity);
						if (onlyFirstMatch)
						{
							return list;
						}
					}
					List<Entity> source = IterateTextBody(childEntity, entityType, propertyNames, propertyValues, onlyFirstMatch);
					AddFindItems(list, source);
					if (list.Count == 1 && onlyFirstMatch)
					{
						return list;
					}
				}
			}
			if (list.Count == 1 && onlyFirstMatch)
			{
				return list;
			}
		}
		return list;
	}

	private List<Entity> IterateTextBody(WTextBody textBody, EntityType entityType, string[] propertyNames, string[] propertyValues, bool onlyFirstMatch)
	{
		List<Entity> list = new List<Entity>();
		for (int i = 0; i < textBody.ChildEntities.Count; i++)
		{
			Entity entity = textBody.ChildEntities[i];
			switch (entity.EntityType)
			{
			case EntityType.Paragraph:
			{
				if (IsEntityEquals(entity, entityType, propertyNames, propertyValues))
				{
					list.Add(entity);
					if (onlyFirstMatch)
					{
						return list;
					}
				}
				WParagraph wParagraph = entity as WParagraph;
				List<Entity> source = IterateParagraph(wParagraph.Items, entityType, propertyNames, propertyValues, onlyFirstMatch);
				AddFindItems(list, source);
				break;
			}
			case EntityType.Table:
			{
				if (IsEntityEquals(entity, entityType, propertyNames, propertyValues))
				{
					list.Add(entity);
					if (onlyFirstMatch)
					{
						return list;
					}
				}
				WTable table = entity as WTable;
				List<Entity> source3 = IterateTable(table, entityType, propertyNames, propertyValues, onlyFirstMatch);
				AddFindItems(list, source3);
				break;
			}
			case EntityType.BlockContentControl:
			{
				if (IsEntityEquals(entity, entityType, propertyNames, propertyValues))
				{
					list.Add(entity);
					if (onlyFirstMatch)
					{
						return list;
					}
				}
				BlockContentControl blockContentControl = entity as BlockContentControl;
				if (IsEntityEquals(blockContentControl.TextBody, entityType, propertyNames, propertyValues))
				{
					list.Add(blockContentControl.TextBody);
					if (onlyFirstMatch)
					{
						return list;
					}
				}
				List<Entity> source2 = IterateTextBody(blockContentControl.TextBody, entityType, propertyNames, propertyValues, onlyFirstMatch);
				AddFindItems(list, source2);
				break;
			}
			case EntityType.AlternateChunk:
				if (IsEntityEquals(entity, entityType, propertyNames, propertyValues))
				{
					list.Add(entity);
				}
				break;
			}
			if (list.Count == 1 && onlyFirstMatch)
			{
				return list;
			}
		}
		return list;
	}

	private List<Entity> IterateParagraph(ParagraphItemCollection paraItems, EntityType entityType, string[] propertyNames, string[] propertyValues, bool onlyFirstMatch)
	{
		List<Entity> list = new List<Entity>();
		for (int i = 0; i < paraItems.Count; i++)
		{
			Entity entity = paraItems[i];
			switch (entity.EntityType)
			{
			case EntityType.TextRange:
			case EntityType.Picture:
			case EntityType.Field:
			case EntityType.FieldMark:
			case EntityType.MergeField:
			case EntityType.SeqField:
			case EntityType.EmbededField:
			case EntityType.ControlField:
			case EntityType.TextFormField:
			case EntityType.DropDownFormField:
			case EntityType.CheckBox:
			case EntityType.BookmarkStart:
			case EntityType.BookmarkEnd:
			case EntityType.Shape:
			case EntityType.Break:
			case EntityType.Symbol:
			case EntityType.TOC:
			case EntityType.XmlParaItem:
			case EntityType.Chart:
			case EntityType.CommentMark:
			case EntityType.OleObject:
			case EntityType.AbsoluteTab:
			case EntityType.EditableRangeStart:
			case EntityType.EditableRangeEnd:
			case EntityType.GroupShape:
			case EntityType.Math:
				if (IsEntityEquals(entity, entityType, propertyNames, propertyValues))
				{
					list.Add(entity);
				}
				break;
			case EntityType.TextBox:
			{
				if (IsEntityEquals(entity, entityType, propertyNames, propertyValues))
				{
					list.Add(entity);
					if (onlyFirstMatch)
					{
						return list;
					}
				}
				WTextBox wTextBox = entity as WTextBox;
				if (IsEntityEquals(wTextBox.TextBoxBody, entityType, propertyNames, propertyValues))
				{
					list.Add(wTextBox.TextBoxBody);
					if (onlyFirstMatch)
					{
						return list;
					}
				}
				List<Entity> source2 = IterateTextBody(wTextBox.TextBoxBody, entityType, propertyNames, propertyValues, onlyFirstMatch);
				AddFindItems(list, source2);
				break;
			}
			case EntityType.AutoShape:
			{
				if (IsEntityEquals(entity, entityType, propertyNames, propertyValues))
				{
					list.Add(entity);
					if (onlyFirstMatch)
					{
						return list;
					}
				}
				Shape shape = entity as Shape;
				if (IsEntityEquals(shape.TextBody, entityType, propertyNames, propertyValues))
				{
					list.Add(shape.TextBody);
					if (onlyFirstMatch)
					{
						return list;
					}
				}
				List<Entity> source3 = IterateTextBody(shape.TextBody, entityType, propertyNames, propertyValues, onlyFirstMatch);
				AddFindItems(list, source3);
				break;
			}
			case EntityType.Footnote:
			{
				if (IsEntityEquals(entity, entityType, propertyNames, propertyValues))
				{
					list.Add(entity);
					if (onlyFirstMatch)
					{
						return list;
					}
				}
				WFootnote wFootnote = entity as WFootnote;
				if (IsEntityEquals(wFootnote.TextBody, entityType, propertyNames, propertyValues))
				{
					list.Add(wFootnote.TextBody);
					if (onlyFirstMatch)
					{
						return list;
					}
				}
				List<Entity> source4 = IterateTextBody(wFootnote.TextBody, entityType, propertyNames, propertyValues, onlyFirstMatch);
				AddFindItems(list, source4);
				break;
			}
			case EntityType.Comment:
			{
				if (IsEntityEquals(entity, entityType, propertyNames, propertyValues))
				{
					list.Add(entity);
					if (onlyFirstMatch)
					{
						return list;
					}
				}
				WComment wComment = entity as WComment;
				if (IsEntityEquals(wComment.TextBody, entityType, propertyNames, propertyValues))
				{
					list.Add(wComment.TextBody);
					if (onlyFirstMatch)
					{
						return list;
					}
				}
				List<Entity> source5 = IterateTextBody(wComment.TextBody, entityType, propertyNames, propertyValues, onlyFirstMatch);
				AddFindItems(list, source5);
				break;
			}
			case EntityType.InlineContentControl:
			{
				if (IsEntityEquals(entity, entityType, propertyNames, propertyValues))
				{
					list.Add(entity);
					if (onlyFirstMatch)
					{
						return list;
					}
				}
				InlineContentControl inlineContentControl = entity as InlineContentControl;
				List<Entity> source = IterateParagraph(inlineContentControl.ParagraphItems, entityType, propertyNames, propertyValues, onlyFirstMatch);
				AddFindItems(list, source);
				break;
			}
			}
			if (list.Count == 1 && onlyFirstMatch)
			{
				return list;
			}
		}
		return list;
	}

	private List<Entity> IterateTable(WTable table, EntityType entityType, string[] propertyNames, string[] propertyValues, bool onlyFirstMatch)
	{
		List<Entity> list = new List<Entity>();
		foreach (WTableRow row in table.Rows)
		{
			if (IsEntityEquals(row, entityType, propertyNames, propertyValues))
			{
				list.Add(row);
				if (onlyFirstMatch)
				{
					return list;
				}
			}
			foreach (WTableCell cell in row.Cells)
			{
				if (IsEntityEquals(cell, entityType, propertyNames, propertyValues))
				{
					list.Add(cell);
					if (onlyFirstMatch)
					{
						return list;
					}
				}
				List<Entity> source = IterateTextBody(cell, entityType, propertyNames, propertyValues, onlyFirstMatch);
				AddFindItems(list, source);
				if (list.Count == 1 && onlyFirstMatch)
				{
					return list;
				}
			}
		}
		return list;
	}

	private bool IsEntityEquals(Entity currentItem, EntityType entityType, string[] propertyNames, string[] propertyValues)
	{
		if (currentItem.EntityType == entityType)
		{
			return IsPropertyValuesEqual(currentItem, propertyNames, propertyValues);
		}
		return false;
	}

	private bool IsUnSupportedEntityType(EntityType entityType)
	{
		if (entityType != 0 && entityType != EntityType.CommentEnd && entityType != EntityType.RowContentControl && entityType != EntityType.CellContentControl && entityType != EntityType.StructureDocumentTag && entityType != EntityType.StructureDocumentTagInline && entityType != EntityType.StructureDocumentTagRow && entityType != EntityType.StructureDocumentTagCell && entityType != EntityType.SDTBlockContent && entityType != EntityType.SDTInlineContent && entityType != EntityType.ChildShape)
		{
			return entityType == EntityType.ChildGroupShape;
		}
		return true;
	}

	private bool IsPropertyValuesEqual(object obj, string[] propertyNames, string[] propertyValues)
	{
		if (propertyNames == null && propertyValues == null)
		{
			return true;
		}
		for (int i = 0; i < propertyNames.Length; i++)
		{
			string text = propertyNames[i];
			object obj2 = obj;
			if (string.IsNullOrEmpty(text))
			{
				continue;
			}
			string[] array = text.Split('.');
			for (int j = 0; j < array.Length; j++)
			{
				PropertyInfo runtimeProperty = obj2.GetType().GetRuntimeProperty(array[j]);
				if (runtimeProperty != null)
				{
					obj2 = runtimeProperty.GetValue(obj2, null);
					if (j == array.Length - 1 && (obj2 == null || obj2.ToString() != propertyValues[i]))
					{
						return false;
					}
					continue;
				}
				return false;
			}
		}
		return true;
	}

	private List<Entity> FindWaterMarks(string[] propertyNames, string[] propertyValues, bool onlyFirstMatch)
	{
		List<Entity> list = new List<Entity>();
		foreach (WSection section in Sections)
		{
			foreach (WTextBody childEntity in section.ChildEntities)
			{
				if (childEntity is HeaderFooter { EntityType: EntityType.HeaderFooter } headerFooter && (headerFooter.Type == HeaderFooterType.OddHeader || headerFooter.Type == HeaderFooterType.EvenHeader || headerFooter.Type == HeaderFooterType.FirstPageHeader) && headerFooter.Watermark != null && headerFooter.Watermark.Type != 0 && (headerFooter.Watermark.Type != WatermarkType.PictureWatermark || (headerFooter.Watermark as PictureWatermark).Picture != null) && IsPropertyValuesEqual(headerFooter.Watermark, propertyNames, propertyValues))
				{
					list.Add(headerFooter.Watermark);
					if (onlyFirstMatch)
					{
						return list;
					}
				}
			}
		}
		return list;
	}

	private List<Entity> AddFindItems(List<Entity> destination, List<Entity> source)
	{
		if (destination == null)
		{
			destination = new List<Entity>();
		}
		destination.AddRange(source);
		return destination;
	}

	internal void UpdateMatchIndex()
	{
		m_matchSectionIndex = m_sectionIndex;
		m_matchBodyItemIndex = m_bodyItemIndex;
		m_matchParaItemIndex = m_paraItemIndex;
	}

	private void MarkNestedField(FieldCollection fieldCollection)
	{
		foreach (WField item in fieldCollection)
		{
			if (!item.IsNestedField)
			{
				item.MarkNestedField();
			}
		}
	}

	internal void UpdateIndex(int bodyItemIndex, int paraItemIndex, int startRangeIndex, int endRangeIndex, int textStartIndex, int textEndIndex = -1)
	{
		m_bodyItemIndex = bodyItemIndex;
		m_paraItemIndex = paraItemIndex;
		m_startRangeIndex = startRangeIndex;
		m_endRangeIndex = endRangeIndex;
		m_textStartIndex = textStartIndex;
		m_textEndIndex = textEndIndex;
	}

	internal void SkipOtherBodyText()
	{
		IsComparing = true;
		foreach (WField field in Comparison.Fields)
		{
			WParagraph ownerParagraph = field.OwnerParagraph;
			WParagraph ownerParagraph2 = field.FieldEnd.OwnerParagraph;
			if (ownerParagraph == ownerParagraph2)
			{
				SetWCStartPos(ownerParagraph, field.Index, field.FieldEnd.Index);
				for (int i = field.FieldEnd.Index + 1; i < ownerParagraph.Items.Count && !(ownerParagraph.Items[i] is WField); i++)
				{
					ownerParagraph.Items[i].m_wcStartPos = ownerParagraph.Items[i - 1].WCEndPos;
				}
				continue;
			}
			foreach (Entity item in field.Range)
			{
				switch (item.EntityType)
				{
				case EntityType.Paragraph:
				{
					WParagraph wParagraph = item as WParagraph;
					if (ownerParagraph2 == wParagraph)
					{
						SetWCStartPos(wParagraph, 0, field.FieldEnd.Index);
						for (int j = field.FieldEnd.Index + 1; j < wParagraph.Items.Count && !(wParagraph.Items[j] is WField); j++)
						{
							wParagraph.Items[j].m_wcStartPos = wParagraph.Items[j - 1].WCEndPos;
						}
					}
					else
					{
						SetWCStartPos(wParagraph, 0, wParagraph.Items.Count - 1);
					}
					break;
				}
				case EntityType.Table:
				{
					WTable bodyItem2 = item as WTable;
					IterateThroughBodyItems(bodyItem2);
					break;
				}
				case EntityType.BlockContentControl:
				{
					BlockContentControl bodyItem = item as BlockContentControl;
					IterateThroughBodyItems(bodyItem);
					break;
				}
				default:
					if (item is ParagraphItem)
					{
						SetWCStartPos((item as ParagraphItem).OwnerParagraph, item.Index, item.Index);
					}
					break;
				}
			}
		}
		foreach (InlineContentControl inlineContentControl in Comparison.InlineContentControls)
		{
			SetWCStartPos(inlineContentControl.OwnerParagraph, inlineContentControl.Index, inlineContentControl.Index);
		}
		foreach (TableOfContent tOC in Comparison.TOCs)
		{
			WParagraph ownerParagraph3 = tOC.OwnerParagraph;
			WParagraph ownerParagraph4 = tOC.TOCField.FieldEnd.OwnerParagraph;
			int index = tOC.Index;
			int index2 = tOC.TOCField.FieldEnd.Index;
			int index3 = ownerParagraph3.GetOwnerSection().Index;
			int index4 = ownerParagraph4.GetOwnerSection().Index;
			if (ownerParagraph3 == ownerParagraph4)
			{
				SetWCStartPos(ownerParagraph3, index, index2);
				for (int k = index2 + 1; k < ownerParagraph3.Items.Count && !(ownerParagraph3.Items[k] is TableOfContent); k++)
				{
					ownerParagraph3.Items[k].m_wcStartPos = ownerParagraph3.Items[k - 1].WCEndPos;
				}
			}
			else if (index3 == index4)
			{
				SetWCStartPosForSection(ownerParagraph3.Index, ownerParagraph4.Index, index3, index, index2);
			}
			else
			{
				if (index3 == index4)
				{
					continue;
				}
				for (int l = index3; l <= index4; l++)
				{
					int endIndex = ((Sections[l].Body.Items[Sections[l].Body.Items.Count - 1] is WParagraph wParagraph2) ? (wParagraph2.Items.Count - 1) : 0);
					if (l == index3)
					{
						SetWCStartPosForSection(ownerParagraph3.Index, Sections[l].Body.Items.Count - 1, l, index, endIndex);
					}
					else if (l == index4)
					{
						SetWCStartPosForSection(0, ownerParagraph4.Index, l, 0, index2);
					}
					else
					{
						SetWCStartPosForSection(0, Sections[l].Body.Items.Count - 1, l, 0, endIndex);
					}
				}
			}
		}
		IsComparing = false;
	}

	private void SetWCStartPosForSection(int beginParaIndex, int endParaIndex, int secIndex, int startIndex, int endIndex)
	{
		for (int i = beginParaIndex; i <= endParaIndex; i++)
		{
			TextBodyItem textBodyItem = Sections[secIndex].Body.Items[i];
			if (i == beginParaIndex && textBodyItem is WParagraph)
			{
				WParagraph wParagraph = (WParagraph)textBodyItem;
				SetWCStartPos(wParagraph, startIndex, wParagraph.Items.Count - 1);
			}
			else if (i == endParaIndex && textBodyItem is WParagraph)
			{
				WParagraph wParagraph2 = textBodyItem as WParagraph;
				SetWCStartPos(wParagraph2, 0, endIndex);
				for (int j = endIndex + 1; j < wParagraph2.Items.Count && !(wParagraph2.Items[j] is TableOfContent); j++)
				{
					wParagraph2.Items[j].m_wcStartPos = wParagraph2.Items[j - 1].WCEndPos;
				}
			}
			else
			{
				IterateThroughBodyItems(textBodyItem);
			}
		}
	}

	private void IterateThroughBodyItems(TextBodyItem bodyItem)
	{
		if (bodyItem is WParagraph)
		{
			WParagraph wParagraph = bodyItem as WParagraph;
			SetWCStartPos(wParagraph, 0, wParagraph.Items.Count - 1);
			return;
		}
		if (bodyItem is WTable)
		{
			foreach (WTableRow row in (bodyItem as WTable).Rows)
			{
				foreach (WTableCell cell in row.Cells)
				{
					foreach (TextBodyItem item in cell.Items)
					{
						IterateThroughBodyItems(item);
					}
				}
			}
			return;
		}
		if (!(bodyItem is BlockContentControl))
		{
			return;
		}
		foreach (TextBodyItem item2 in (bodyItem as BlockContentControl).TextBody.Items)
		{
			IterateThroughBodyItems(item2);
		}
	}

	private void SetWCStartPos(WParagraph para, int startIndex, int endIndex)
	{
		int num = ((startIndex > 0) ? para.Items[startIndex - 1].WCEndPos : (-1));
		for (int i = startIndex; i <= endIndex; i++)
		{
			if (para.Items[i] is InlineContentControl)
			{
				foreach (ParagraphItem paragraphItem in (para.Items[i] as InlineContentControl).ParagraphItems)
				{
					paragraphItem.WCStartPos = ((num != -1) ? num : 0);
				}
			}
			para.Items[i].WCStartPos = ((num != -1) ? num : 0);
		}
	}

	public void Compare(WordDocument document, string author = "Author", DateTime dateTime = default(DateTime), ComparisonOptions comparisonOptions = null)
	{
		WordCompare(document, author, dateTime, comparisonOptions);
	}

	internal void WordCompare(WordDocument document, string author, DateTime dateTime, ComparisonOptions comparisonOptions)
	{
		if (!IsDOCX() || !document.IsDOCX())
		{
			return;
		}
		InitCompareInfo(this, document, author, dateTime, comparisonOptions);
		CompareStylesBetweenDocuments(document);
		int num = document.currSectionIndex;
		while (num < document.Sections.Count && m_sectionIndex < Sections.Count)
		{
			document.currSectionIndex = num;
			_ = m_sectionIndex;
			WSection wSection = document.Sections[document.currSectionIndex];
			wSection.Body.Compare(this);
			if (m_matchSectionIndex != -1 && m_matchSectionIndex < Sections.Count && m_matchSectionIndex != Sections.Count - 1 && wSection.Index != document.Sections.Count - 1)
			{
				WSection wSection2 = Sections[m_matchSectionIndex];
				wSection.CompareHeaderFooter(wSection2);
				if ((document.ComparisonOptions != null && !document.ComparisonOptions.DetectFormatChanges) || !wSection2.CompareSectionFormat(wSection))
				{
					ApplySecFormatRevision(wSection2.SectionFormat, wSection.SectionFormat);
				}
			}
			num = ((document.currSectionIndex != num) ? (document.currSectionIndex - 1) : num);
			num++;
		}
		WSection wSection3 = Sections[Sections.Count - 1];
		document.LastSection.CompareHeaderFooter(wSection3);
		if ((document.ComparisonOptions != null && !document.ComparisonOptions.DetectFormatChanges) || !wSection3.CompareSectionFormat(document.LastSection))
		{
			ApplySecFormatRevision(wSection3.SectionFormat, document.LastSection.SectionFormat);
		}
		_ = m_paraItemIndex;
		_ = m_bodyItemIndex;
		_ = m_sectionIndex;
		EndOfTheDocument(document);
		CompareEmptyParagraphs(LastSection.Body, document.LastSection.Body);
		DisposeCompareInfo(this, document);
	}

	private void ApplySecFormatRevision(WSectionFormat orgSectionFormat, WSectionFormat revSectionFormat)
	{
		orgSectionFormat.CompareProperties(revSectionFormat);
		if (revSectionFormat.Document.ComparisonOptions != null && revSectionFormat.Document.ComparisonOptions.DetectFormatChanges)
		{
			orgSectionFormat.IsChangedFormat = true;
			orgSectionFormat.IsFormattingChange = true;
			orgSectionFormat.FormatChangeAuthorName = base.Document.m_authorName;
			orgSectionFormat.FormatChangeDateTime = base.Document.m_dateTime;
			orgSectionFormat.IsFormattingChange = false;
			orgSectionFormat.Document.SectionFormatChange(LastSection);
		}
	}

	private void InitCompareInfo(WordDocument orgDocument, WordDocument revDocument, string authorName, DateTime dateTime, ComparisonOptions comparisonOptions)
	{
		_ = orgDocument.Comparison;
		_ = revDocument.Comparison;
		AcceptChanges();
		revDocument.AcceptChanges();
		m_authorName = authorName;
		orgDocument.Comparison.AddComparisonCollection(orgDocument);
		DateTime dateTime2 = ((dateTime == default(DateTime)) ? DateTime.Now : dateTime);
		m_dateTime = dateTime2;
		revDocument.m_authorName = authorName;
		revDocument.m_dateTime = dateTime;
		if (comparisonOptions != null)
		{
			revDocument.ComparisonOptions = comparisonOptions;
		}
		else
		{
			revDocument.ComparisonOptions = new ComparisonOptions();
		}
		revDocument.Comparison.CompareImagesInDoc(orgDocument);
		orgDocument.UpdateAlternateChunks();
		revDocument.UpdateAlternateChunks();
		MarkNestedField(revDocument.Fields);
		SkipOtherBodyText();
		if (!orgDocument.DifferentOddAndEvenPages)
		{
			orgDocument.DifferentOddAndEvenPages = revDocument.DifferentOddAndEvenPages;
		}
	}

	private void DisposeCompareInfo(WordDocument orgDocument, WordDocument revDocument)
	{
		if (orgDocument.Comparison != null)
		{
			orgDocument.Comparison.Dispose();
		}
		if (revDocument.Comparison != null)
		{
			revDocument.Comparison.Dispose();
		}
	}

	private void CompareStylesBetweenDocuments(WordDocument revisedDocument)
	{
		bool isComparing = revisedDocument.IsComparing;
		bool isComparing2 = IsComparing;
		revisedDocument.IsComparing = true;
		IsComparing = true;
		for (int i = 0; i < revisedDocument.ListStyles.Count; i++)
		{
			ListStyle listStyle = revisedDocument.ListStyles[i];
			if (ListStyles.FindByName(listStyle.Name) == null)
			{
				ListStyle listStyle2 = listStyle.Clone() as ListStyle;
				Comparison.RevisedDocListStyles.Add(listStyle2.Name);
				ListStyles.Add(listStyle2);
			}
		}
		for (int j = 0; j < revisedDocument.Styles.Count; j++)
		{
			Style style = revisedDocument.Styles[j] as Style;
			if (!(Styles.FindByName(style.Name) is Style style2))
			{
				Style style3 = style.Clone() as Style;
				Styles.Add(style3);
				continue;
			}
			if (((revisedDocument.ComparisonOptions != null && !revisedDocument.ComparisonOptions.DetectFormatChanges) || !style2.CompareStyleBetweenDocuments(style)) && !style2.GetBuiltinStyles().ContainsValue(style2.Name))
			{
				if (style2.StyleType == StyleType.ParagraphStyle)
				{
					WParagraphStyle style4 = Styles.FindByName("Normal") as WParagraphStyle;
					SetDefaultFormat(revisedDocument, style4);
				}
				style2.Remove();
				Styles.Add(style.Clone() as Style);
				continue;
			}
			style2.NextStyle = style.NextStyle;
			style2.LinkStyle = style.LinkStyle;
			style2.UnhideWhenUsed = style.UnhideWhenUsed;
			style2.IsCustom = style.IsCustom;
			style2.IsPrimaryStyle = style.IsPrimaryStyle;
			style2.IsSemiHidden = style.IsSemiHidden;
			style2.TableStyleData = style.TableStyleData;
			if (style.BaseStyle != null && Styles.FindByName(style.BaseStyle.Name) != null && (style2.BaseStyle == null || style2.BaseStyle.Name != style.BaseStyle.Name))
			{
				style2.ApplyBaseStyle(style.BaseStyle.Name);
			}
		}
		ModifyDefFormatsAsRevised(revisedDocument);
		revisedDocument.IsComparing = isComparing;
		IsComparing = isComparing2;
	}

	private void ModifyDefFormatsAsRevised(WordDocument revisedDocument)
	{
		DefCharFormat = new WCharacterFormat(this);
		DefCharFormat.ImportContainer(revisedDocument.DefCharFormat);
		DefParaFormat = new WParagraphFormat(this);
		DefParaFormat.ImportContainer(revisedDocument.DefParaFormat);
	}

	private void SetDefaultFormat(WordDocument revisedDoc, WParagraphStyle style)
	{
		if (!style.CharacterFormat.IsChangedFormat)
		{
			style.CharacterFormat.CompareProperties(revisedDoc.DefCharFormat);
			style.CharacterFormat.IsChangedFormat = true;
			style.CharacterFormat.FormatChangeAuthorName = base.Document.m_authorName;
			style.CharacterFormat.FormatChangeDateTime = base.Document.m_dateTime;
			base.Document.CharacterFormatChange(style.CharacterFormat, null, null);
		}
		if (!style.ParagraphFormat.IsChangedFormat)
		{
			style.ParagraphFormat.CompareProperties(revisedDoc.DefParaFormat);
			style.ParagraphFormat.IsChangedFormat = true;
			style.ParagraphFormat.FormatChangeAuthorName = base.Document.m_authorName;
			style.ParagraphFormat.FormatChangeDateTime = base.Document.m_dateTime;
			base.Document.ParagraphFormatChange(style.ParagraphFormat);
		}
	}

	internal void CompareEmptyParagraphs(WTextBody originalTextBody, WTextBody revisedTextBody)
	{
		Entity entity = revisedTextBody.Items.LastItem;
		int num = originalTextBody.Items.Count;
		while (entity != null && entity is WParagraph)
		{
			WParagraph wParagraph = entity as WParagraph;
			if (wParagraph.Items.Count != 0 && !wParagraph.IsEmptyParagraph())
			{
				break;
			}
			num--;
			Entity entity2 = originalTextBody.Items[num];
			if (entity2 != null && entity2 is WParagraph)
			{
				WParagraph wParagraph2 = entity2 as WParagraph;
				if ((wParagraph2.Items.Count == 0 || wParagraph2.IsEmptyParagraph()) && wParagraph2.BreakCharacterFormat.IsInsertRevision)
				{
					for (TextBodyItem textBodyItem = wParagraph2.PreviousSibling as TextBodyItem; textBodyItem != null; textBodyItem = textBodyItem.PreviousSibling as TextBodyItem)
					{
						if (textBodyItem is WParagraph && (textBodyItem as WParagraph).BreakCharacterFormat.IsDeleteRevision)
						{
							if ((textBodyItem as WParagraph).Items.Count == 0 || (textBodyItem as WParagraph).IsParaHasOnlyBookmark())
							{
								WParagraph revisedParagraph = textBodyItem as WParagraph;
								wParagraph2.CompareParagraphFormats(revisedParagraph);
								originalTextBody.Document.IsComparing = true;
								originalTextBody.Items.RemoveAt(textBodyItem.Index);
								wParagraph2.BreakCharacterFormat.IsInsertRevision = false;
								wParagraph2.RemoveEntityRevision(isNeedToRemoveFormatRev: false);
								originalTextBody.Document.IsComparing = false;
								num--;
							}
							break;
						}
					}
				}
			}
			entity = entity.PreviousSibling as Entity;
		}
	}

	private void EndOfTheDocument(WordDocument revDoc)
	{
		int num = Sections.Count - 1;
		int num2 = Sections[num].Body.Items.Count - 1;
		WParagraph wParagraph = Sections[num].Body.Items[num2] as WParagraph;
		int num3 = ((wParagraph != null && wParagraph.Items.Count > 0) ? wParagraph.Items.Count : 0);
		int num4 = revDoc.Sections.Count - 1;
		int num5 = revDoc.Sections[num4].Body.Items.Count - 1;
		int num6 = ((revDoc.Sections[num4].Body.Items[num5] is WParagraph wParagraph2 && wParagraph2.Items.Count > 0) ? wParagraph2.Items.Count : 0);
		bool flag = false;
		if (m_sectionIndex > num || (m_sectionIndex == num && m_bodyItemIndex > num2) || (m_sectionIndex == num && m_bodyItemIndex == num2 && m_paraItemIndex >= num3 && m_paraItemIndex != 0))
		{
			flag = true;
		}
		bool flag2 = false;
		if (revDoc.m_sectionIndex > num4 || (revDoc.m_sectionIndex == num4 && revDoc.m_bodyItemIndex > num5) || (revDoc.m_sectionIndex == num4 && revDoc.m_bodyItemIndex == num5 && revDoc.m_paraItemIndex >= num6 && revDoc.m_paraItemIndex != 0))
		{
			flag2 = true;
		}
		if (m_sectionIndex > num)
		{
			m_sectionIndex = num;
			m_bodyItemIndex = num2;
			m_paraItemIndex = wParagraph?.Items.Count ?? 0;
		}
		else if ((m_paraItemIndex == 0 && m_bodyItemIndex > 0 && m_bodyItemIndex > Sections[m_sectionIndex].Body.Items.Count - 1 && Sections[m_sectionIndex].Body.Items[m_bodyItemIndex - 1] is WParagraph) || (revDoc.m_paraItemIndex == 0 && revDoc.m_bodyItemIndex > 0 && revDoc.m_bodyItemIndex > revDoc.Sections[revDoc.m_sectionIndex].Body.Items.Count - 1 && revDoc.Sections[revDoc.m_sectionIndex].Body.Items[revDoc.m_bodyItemIndex - 1] is WParagraph))
		{
			m_bodyItemIndex--;
			m_paraItemIndex = (Sections[m_sectionIndex].Body.Items[m_bodyItemIndex] as WParagraph).Items.Count;
			revDoc.m_bodyItemIndex--;
			revDoc.m_paraItemIndex = (revDoc.Sections[revDoc.m_sectionIndex].Body.Items[revDoc.m_bodyItemIndex] as WParagraph).Items.Count;
		}
		if (flag && flag2)
		{
			return;
		}
		if (!flag && flag2)
		{
			DeleteItemsAtDocumentEnd(revDoc);
			return;
		}
		if (flag && !flag2)
		{
			InsertItemsToEndOfTheDocument(revDoc);
			return;
		}
		DeleteItemsAtDocumentEnd(revDoc);
		if (revDoc.m_sectionIndex <= num4 && (revDoc.m_sectionIndex != num4 || revDoc.m_bodyItemIndex <= num5) && (revDoc.m_sectionIndex != num4 || revDoc.m_bodyItemIndex != num5 || revDoc.m_paraItemIndex < num6 || revDoc.m_paraItemIndex == 0))
		{
			InsertItemsToEndOfTheDocument(revDoc);
		}
	}

	private void DeleteItemsAtDocumentEnd(WordDocument revisedDoc)
	{
		int num = Sections.Count - 1;
		int num2 = Sections[num].Body.Items.Count - 1;
		WParagraph wParagraph = Sections[num].Body.Items[num2] as WParagraph;
		int num3 = ((wParagraph != null && wParagraph.Items.Count > 0) ? (wParagraph.Items.Count - 1) : 0);
		if (num != m_matchSectionIndex || num2 != m_matchBodyItemIndex)
		{
			bool isNeedToInsert = false;
			if (wParagraph != null)
			{
				WParagraph obj = Sections[m_sectionIndex].Body.Items[m_bodyItemIndex] as WParagraph;
				int paraItemIndex = m_paraItemIndex;
				Comparison.ApplyDelRevision(this, revisedDoc, num, num2, num3, ref isNeedToInsert, isDocumentEnd: true);
				if (obj != wParagraph || paraItemIndex <= 0)
				{
					wParagraph.ApplyDelRevision(wParagraph, 0, num3 + 1);
				}
			}
			else
			{
				Comparison.ApplyDelRevision(this, revisedDoc, num, num2 + 1, num3, ref isNeedToInsert, isDocumentEnd: true);
			}
		}
		else if (num3 != m_matchParaItemIndex)
		{
			wParagraph.ApplyDelRevision(wParagraph, m_paraItemIndex, num3 + 1);
		}
		m_sectionIndex = num;
		m_bodyItemIndex = ((wParagraph != null) ? num2 : (num2 + 1));
		m_paraItemIndex = wParagraph?.Items.Count ?? 0;
	}

	private void InsertItemsToEndOfTheDocument(WordDocument revisedDocument)
	{
		int num = revisedDocument.Sections.Count - 1;
		int num2 = revisedDocument.Sections[num].Body.ChildEntities.Count - 1;
		WParagraph wParagraph = revisedDocument.Sections[num].Body.Items[num2] as WParagraph;
		int currRevParaItemIndex = ((wParagraph != null && wParagraph.Items.Count > 0) ? wParagraph.Items.Count : 0);
		if (wParagraph == null)
		{
			num2++;
		}
		revisedDocument.Comparison.Insertion(this, currRevParaItemIndex, num2, num, m_paraItemIndex, m_bodyItemIndex, m_sectionIndex);
	}

	internal void MarkInsertRevision(WordDocumentPart documentPart)
	{
		foreach (WSection section in documentPart.Sections)
		{
			foreach (TextBodyItem childEntity in section.Body.ChildEntities)
			{
				if (childEntity is WParagraph)
				{
					(childEntity as WParagraph).AddInsMark();
				}
				else if (childEntity is WTable)
				{
					(childEntity as WTable).AddInsMark();
				}
				else if (childEntity is BlockContentControl)
				{
					(childEntity as BlockContentControl).AddInsMark();
				}
			}
		}
	}

	internal void InsertFieldItems(WParagraph orgPara, WParagraph revPara, ref int itemIndex, ref int paraIndex)
	{
		WField obj = revPara.Items[itemIndex] as WField;
		orgPara.Items.Add(revPara.Items[itemIndex].Clone());
		orgPara.ApplyInsRevision(orgPara.Items[orgPara.Items.Count - 1]);
		itemIndex++;
		foreach (Entity item in obj.Range)
		{
			if (item is ParagraphItem)
			{
				orgPara.Items.Add(item.Clone());
				orgPara.ApplyInsRevision(orgPara.Items[orgPara.Items.Count - 1]);
				itemIndex++;
			}
			else
			{
				TextBodyItem textBodyItem = item.Clone() as TextBodyItem;
				LastSection.Body.Items.Add(textBodyItem);
				textBodyItem.AddInsMark();
				paraIndex++;
			}
		}
	}

	internal WParagraph GetOwnerParagraphToInsertBookmark(Entity entity, bool isStart)
	{
		switch (entity.EntityType)
		{
		case EntityType.Paragraph:
			return entity as WParagraph;
		case EntityType.Table:
		{
			WTable wTable = entity as WTable;
			if (wTable.Rows.Count > 0)
			{
				if (isStart && wTable.Rows[0].Cells.Count > 0 && wTable.Rows[0].Cells[0].Items.Count > 0)
				{
					return GetOwnerParagraphToInsertBookmark(wTable.Rows[0].Cells[0].Items[0], isStart: true);
				}
				if (!isStart && wTable.LastCell != null && wTable.LastCell.Items.Count > 0)
				{
					return GetOwnerParagraphToInsertBookmark(wTable.LastCell.Items[wTable.LastCell.Items.Count - 1], isStart: false);
				}
			}
			break;
		}
		case EntityType.BlockContentControl:
		{
			BlockContentControl blockContentControl = entity as BlockContentControl;
			if (blockContentControl.TextBody.Items.Count > 0)
			{
				if (isStart)
				{
					return GetOwnerParagraphToInsertBookmark(blockContentControl.TextBody.Items[0], isStart: true);
				}
				return GetOwnerParagraphToInsertBookmark(blockContentControl.TextBody.Items[blockContentControl.TextBody.Items.Count - 1], isStart: false);
			}
			break;
		}
		}
		return null;
	}

	internal void RemoveDelMark(ParagraphItem paraItem)
	{
		WCharacterFormat wCharacterFormat = ((!(paraItem is Break)) ? paraItem.GetCharFormat() : (paraItem as Break).CharacterFormat);
		wCharacterFormat.IsDeleteRevision = false;
		if (paraItem is WTextBox)
		{
			foreach (TextBodyItem childEntity in (paraItem as WTextBox).TextBoxBody.ChildEntities)
			{
				if (childEntity is WParagraph)
				{
					(childEntity as WParagraph).RemoveDelMark();
				}
				else if (childEntity is WTable)
				{
					(childEntity as WTable).RemoveDeleteRevision();
				}
			}
		}
		if (paraItem is WField && (paraItem as WField).FieldEnd.OwnerParagraph.Index == (paraItem as WField).OwnerParagraph.Index)
		{
			WParagraph ownerParagraph = paraItem.OwnerParagraph;
			int index = (paraItem as WField).FieldEnd.Index;
			for (int i = paraItem.Index + 1; i <= index; i++)
			{
				ownerParagraph.Items[i].GetCharFormat().IsDeleteRevision = false;
			}
		}
	}

	internal void RemoveInsMark(ParagraphItem paraItem)
	{
		WCharacterFormat wCharacterFormat = ((!(paraItem is Break)) ? paraItem.GetCharFormat() : (paraItem as Break).CharacterFormat);
		wCharacterFormat.IsInsertRevision = false;
		if (paraItem is WTextBox)
		{
			foreach (TextBodyItem childEntity in (paraItem as WTextBox).TextBoxBody.ChildEntities)
			{
				if (childEntity is WParagraph)
				{
					(childEntity as WParagraph).RemoveInsMark();
				}
				else if (childEntity is WTable)
				{
					(childEntity as WTable).RemoveInsertRevision();
				}
			}
		}
		if (paraItem is WField && (paraItem as WField).FieldEnd.OwnerParagraph.Index == (paraItem as WField).OwnerParagraph.Index)
		{
			WParagraph ownerParagraph = paraItem.OwnerParagraph;
			int index = (paraItem as WField).FieldEnd.Index;
			for (int i = paraItem.Index + 1; i <= index; i++)
			{
				ownerParagraph.Items[i].GetCharFormat().IsInsertRevision = false;
			}
		}
	}
}
