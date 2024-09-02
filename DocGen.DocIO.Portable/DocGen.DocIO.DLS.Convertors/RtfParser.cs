using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DocGen.CompoundFile.DocIO;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS.Convertors;

internal class RtfParser
{
	internal class TempShapeProperty
	{
		internal string m_drawingFieldName;

		internal string m_drawingFieldValue;

		internal TempShapeProperty(string drawingFieldName, string drawingFieldValue)
		{
			m_drawingFieldName = drawingFieldName;
			m_drawingFieldValue = drawingFieldValue;
		}
	}

	internal class TextFormat
	{
		private float m_position;

		private float m_scaling;

		private float m_charcterSpacing;

		internal ThreeState Bold;

		internal ThreeState Italic;

		internal ThreeState Underline;

		internal ThreeState Strike;

		internal ThreeState DoubleStrike;

		internal ThreeState Emboss;

		internal ThreeState Engrave;

		internal SubSuperScript m_subSuperScript;

		internal Color FontColor;

		internal Color BackColor;

		internal Color ForeColor;

		private Color m_highlightColor;

		internal string FontFamily;

		internal float FontSize;

		internal HorizontalAlignment TextAlign;

		internal BuiltinStyle Style;

		internal ThreeState Bidi;

		internal ThreeState AllCaps;

		internal ThreeState SmallCaps;

		internal UnderlineStyle m_underlineStyle;

		internal bool Shadow;

		internal bool IsHiddenText;

		internal bool SpecVanish;

		internal string CharacterStyleName;

		private short m_localIdASCII;

		private short m_localIdForEast;

		private short m_lidBi;

		internal BreakClearType m_BreakClear;

		internal ThreeState complexScript;

		internal ThreeState boldBidi;

		internal ThreeState italicBidi;

		internal short LocalIdASCII
		{
			get
			{
				return m_localIdASCII;
			}
			set
			{
				m_localIdASCII = value;
			}
		}

		internal short LocalIdForEast
		{
			get
			{
				return m_localIdForEast;
			}
			set
			{
				m_localIdForEast = value;
			}
		}

		internal float CharacterSpacing
		{
			get
			{
				return m_charcterSpacing;
			}
			set
			{
				m_charcterSpacing = value;
			}
		}

		internal short LidBi
		{
			get
			{
				return m_lidBi;
			}
			set
			{
				m_lidBi = value;
			}
		}

		internal float Position
		{
			get
			{
				return m_position;
			}
			set
			{
				m_position = value;
			}
		}

		internal float Scaling
		{
			get
			{
				return m_scaling;
			}
			set
			{
				m_scaling = value;
			}
		}

		internal Color HighlightColor
		{
			get
			{
				return m_highlightColor;
			}
			set
			{
				m_highlightColor = value;
			}
		}

		internal TextFormat()
		{
			FontSize = 0f;
			Bold = ThreeState.False;
			Italic = ThreeState.False;
			Underline = ThreeState.False;
			Strike = ThreeState.False;
			DoubleStrike = ThreeState.False;
			Emboss = ThreeState.False;
			Engrave = ThreeState.False;
			FontColor = Color.Empty;
			BackColor = Color.Empty;
			ForeColor = Color.Empty;
			HighlightColor = Color.Empty;
			FontFamily = string.Empty;
			TextAlign = HorizontalAlignment.Left;
			Style = BuiltinStyle.Normal;
			Bidi = ThreeState.Unknown;
			m_underlineStyle = UnderlineStyle.None;
			m_subSuperScript = SubSuperScript.None;
			AllCaps = ThreeState.False;
			SmallCaps = ThreeState.False;
			CharacterStyleName = string.Empty;
			Position = 0f;
			LocalIdASCII = 1033;
			LocalIdForEast = 1033;
			LidBi = 1025;
			Scaling = 100f;
		}

		public TextFormat Clone()
		{
			return (TextFormat)MemberwiseClone();
		}
	}

	internal class SecionFormat
	{
		private float m_leftMargin = 72f;

		private float m_rightMargin = 72f;

		private float m_topMargin = 72f;

		private float m_bottomMargin = 72f;

		internal float HeaderDistance = Convert.ToSingle(36);

		internal float FooterDistance = Convert.ToSingle(36);

		internal bool DifferentFirstPage;

		internal bool DifferentOddAndEvenPage;

		internal bool IsFrontPageBorder;

		internal float DefaultTabWidth = Convert.ToSingle(36);

		internal PageAlignment VertAlignment;

		internal SizeF pageSize = PageSize.Letter;

		internal PageOrientation m_pageOrientation;

		private int m_firstPageTray;

		private int m_otherPagesTray;

		internal float LeftMargin
		{
			get
			{
				return m_leftMargin;
			}
			set
			{
				m_leftMargin = value;
			}
		}

		internal float RightMargin
		{
			get
			{
				return m_rightMargin;
			}
			set
			{
				m_rightMargin = value;
			}
		}

		internal float TopMargin
		{
			get
			{
				return m_topMargin;
			}
			set
			{
				m_topMargin = value;
			}
		}

		internal float BottomMargin
		{
			get
			{
				return m_bottomMargin;
			}
			set
			{
				m_bottomMargin = value;
			}
		}

		internal int FirstPageTray
		{
			get
			{
				return m_firstPageTray;
			}
			set
			{
				m_firstPageTray = value;
			}
		}

		internal int OtherPagesTray
		{
			get
			{
				return m_otherPagesTray;
			}
			set
			{
				m_otherPagesTray = value;
			}
		}
	}

	internal class PictureFormat
	{
		internal float Height;

		internal float Width;

		internal float HeightScale;

		internal float WidthScale;

		internal int PicW;

		internal int picH;

		internal int Zorder;

		internal string Rotation = string.Empty;
	}

	internal class ShapeFormat
	{
		internal float m_width;

		internal float m_height;

		internal float m_left;

		internal float m_right;

		internal float m_top;

		internal float m_bottom;

		internal float m_horizPosition;

		internal float m_vertPosition;

		internal int m_uniqueId;

		internal int m_zOrder;

		internal bool m_isBelowText;

		internal bool m_isInHeader;

		internal bool m_isLockAnchor;

		internal ShapeHorizontalAlignment m_horizAlignment;

		internal TextWrappingType m_textWrappingType;

		internal TextWrappingStyle m_textWrappingStyle;

		internal VerticalOrigin m_vertOrgin;

		internal HorizontalOrigin m_horizOrgin;

		private SizeF m_size;

		internal SizeF Size => GetSizeValue();

		private SizeF GetSizeValue()
		{
			m_width = (m_right - m_left) / 20f;
			m_height = (m_bottom - m_top) / 20f;
			m_size = new SizeF(m_width, m_height);
			return m_size;
		}
	}

	internal enum ThreeState
	{
		False,
		True,
		Unknown
	}

	private const string c_groupStart = "{";

	private const string c_groupEnd = "}";

	private const string c_controlStart = "\\";

	private const string c_space = " ";

	private const string c_carriegeReturn = "\r";

	private const string c_newLine = "\n";

	private const string c_semiColon = ";";

	private RtfLexer m_lexer;

	private RtfReader m_rtfReader;

	private string m_token;

	private string m_previousToken;

	private string m_previousTokenKey;

	private string m_previousTokenValue;

	private string m_previousControlString;

	private bool m_bIsContinousList;

	private bool m_bIsPreviousList;

	private RtfTableType m_currentTableType = RtfTableType.None;

	private Dictionary<string, RtfFont> m_fontTable = new Dictionary<string, RtfFont>();

	private Dictionary<int, RtfColor> m_colorTable = new Dictionary<int, RtfColor>();

	private Dictionary<int, CellFormat> m_cellFormatTable = new Dictionary<int, CellFormat>();

	private IWParagraph m_currParagraph;

	private IWSection m_currSection;

	private WordDocument m_document;

	private TextFormat m_currTextFormat;

	private ShapeFormat m_currShapeFormat;

	private PictureFormat m_picFormat;

	private Stack<string> m_stack = new Stack<string>();

	private Stack<string> m_pictureOrShapeStack = new Stack<string>();

	private Stack<string> m_groupShapeStack = new Stack<string>();

	private Stack<string> m_destStack = new Stack<string>();

	private Stack<string> m_headerFooterStack = new Stack<string>();

	private Stack<WCommentMark> commentstack = new Stack<WCommentMark>();

	private Stack<Dictionary<int, CellFormat>> m_CellFormatStack = new Stack<Dictionary<int, CellFormat>>();

	private Stack<Dictionary<int, CellFormat>> m_prevCellFormatStack = new Stack<Dictionary<int, CellFormat>>();

	private RtfFont m_rtfFont;

	private RtfTokenType m_tokenType;

	private RtfTokenType m_prevTokenType;

	private RtfColor m_rtfColorTable;

	private bool m_bIsBorderTop;

	private bool m_bIsBorderBottom;

	private bool m_bIsBorderLeft;

	private bool m_bIsBorderRight;

	private bool m_bIsBorderDiagonalDown;

	private bool m_bIsBorderDiagonalUp;

	private bool m_bIsPictureOrShape;

	private bool m_bIsShape;

	private bool m_bIsHorizontalBorder;

	private bool m_bIsVerticalBorder;

	private bool m_bIsFallBackImage;

	private bool m_bIsShapeResult;

	private int m_bShapeResultStackCount;

	private IWPicture m_currPicture;

	private Shape m_currShape;

	private WTextBox m_currTextBox;

	private IWTextRange tr;

	private bool m_bIsDocumentInfo;

	private bool m_bIsShapePicture;

	private SecionFormat m_secFormat;

	private ListStyle m_currListStyle;

	private ListOverrideStyle m_currListOverrideStyle;

	private WListLevel m_currListLevel;

	private int m_currLevelIndex = -1;

	private Dictionary<string, ListStyle> m_listTable = new Dictionary<string, ListStyle>();

	private Dictionary<string, string> m_listOverrideTable = new Dictionary<string, string>();

	private Dictionary<string, IWParagraphStyle> m_styleTable = new Dictionary<string, IWParagraphStyle>();

	private Dictionary<string, WCharacterStyle> m_charStyleTable = new Dictionary<string, WCharacterStyle>();

	private string m_currStyleName;

	private bool m_bIsListText;

	private bool m_bIsList;

	private bool isPnStartUpdate;

	private IWTable m_currTable;

	private WTableRow m_currRow;

	private WTableCell m_currCell;

	private int m_currCellFormatIndex = -1;

	private bool m_bIsRow;

	private CellFormat m_currCellFormat;

	private RowFormat m_currRowFormat;

	private bool m_bIsGroupShape;

	private bool m_bIsBookmarkStart;

	private bool m_bIsBookmarkEnd;

	private bool m_bIsHeader;

	private bool m_bIsFooter;

	private bool m_bIsLevelText;

	private WTextBody m_textBody;

	private int m_previousLevel;

	private int m_currentLevel;

	private bool m_bIsCustomProperties;

	private string m_currPropertyName;

	private object m_currPropertyValue;

	private DocGen.CompoundFile.DocIO.PropertyType m_currPropertyType;

	private bool m_bInTable;

	private Stack<WTextBody> m_nestedTextBody = new Stack<WTextBody>();

	private Stack<WTable> m_nestedTable = new Stack<WTable>();

	private bool m_bCellFinished;

	private Column m_currColumn;

	private IWParagraphStyle m_currStyle;

	private WCharacterStyle m_currCharStyle;

	private string m_currStyleID;

	private int m_secCount;

	private Dictionary<int, TabFormat> m_tabCollection = new Dictionary<int, TabFormat>();

	private TabFormat m_currTabFormat;

	private int m_tabCount;

	private bool m_bIsLinespacingRule;

	private bool m_bIsAccentChar;

	private int m_currCellBoundary;

	private int m_currRowLeftIndent;

	private Stack<RowFormat> m_currRowFormatStack = new Stack<RowFormat>();

	private Stack<RowFormat> m_prevRowFormatStack = new Stack<RowFormat>();

	private Stack<string> m_backgroundCollectionStack = new Stack<string>();

	private bool m_bIsBackgroundCollection;

	private bool m_bIsDefaultSectionFormat;

	private SecionFormat m_defaultSectionFormat;

	private string m_styleName;

	private Stack<string> m_listLevelStack = new Stack<string>();

	private bool m_bIsListLevel;

	private IWParagraph m_prevParagraph;

	private TextFormat m_prevTextFormat;

	private WParagraphFormat m_listLevelParaFormat;

	private WCharacterFormat m_listLevelCharFormat;

	private int m_pnLevelNumber = -1;

	private FormFieldData m_currentFormField;

	private Stack<int> m_unicodeCountStack = new Stack<int>();

	private int m_unicodeCount;

	private int m_currColorIndex = -1;

	private Stack<Dictionary<int, TabFormat>> m_tabFormatStack = new Stack<Dictionary<int, TabFormat>>();

	private Stack<TextFormat> m_textFormatStack = new Stack<TextFormat>();

	private Stack<WParagraphFormat> m_paragraphFormatStack = new Stack<WParagraphFormat>();

	private WParagraphFormat m_currParagraphFormat;

	private Stack<string> m_rtfCollectionStack = new Stack<string>();

	private Stack<string> m_shapeInstructionStack = new Stack<string>();

	private Stack<string> m_shapeTextStack = new Stack<string>();

	private Stack<WTextBody> m_shapeTextBody = new Stack<WTextBody>();

	private Stack<Dictionary<string, object>> m_ownerShapeTextbodyStack = new Stack<Dictionary<string, object>>();

	private Stack<WParagraph> m_shapeParagraph = new Stack<WParagraph>();

	private Stack<Dictionary<string, bool>> m_shapeFlagStack = new Stack<Dictionary<string, bool>>();

	private Stack<Dictionary<string, object>> m_shapeTextbodyStack = new Stack<Dictionary<string, object>>();

	private List<TempShapeProperty> m_drawingFields = new List<TempShapeProperty>();

	private bool m_bIsShapeInstruction;

	private bool m_bIsShapeText;

	private bool m_bIsShapePictureAdded;

	private Stack<string> m_objectStack = new Stack<string>();

	private bool m_bIsObject;

	private bool m_bIsStandardPictureSizeNeedToBePreserved;

	private string m_drawingFieldName;

	private string m_drawingFieldValue;

	private Stack<int> m_fieldResultGroupStack = new Stack<int>();

	private Stack<int> m_fieldInstructionGroupStack = new Stack<int>();

	private Stack<int> m_fieldGroupStack = new Stack<int>();

	private Stack<WField> m_fieldCollectionStack = new Stack<WField>();

	private Stack<string> m_formFieldDataStack = new Stack<string>();

	private string m_currentFieldGroupData;

	private Stack<FieldGroupType> m_fieldGroupTypeStack = new Stack<FieldGroupType>();

	private string m_defaultCodePage;

	private int m_defaultFontIndex;

	private bool m_bIsRowBorderTop;

	private bool m_bIsRowBorderBottom;

	private bool m_bIsRowBorderLeft;

	private bool m_bIsRowBorderRight;

	private float m_leftcellspace;

	private float m_rightcellspace;

	private float m_bottomcellspace;

	private float m_topcellspace;

	private bool m_bIsWord97StylePadding;

	private bool isWrapPolygon;

	private int m_currenttrleft;

	private Dictionary<string, WComment> m_comments;

	private List<string> m_commRangeStartId;

	private WComment m_currComment;

	private string m_commAtnText;

	private bool m_isCommentRangeStart;

	private bool m_isCommentReference;

	private bool m_isCommentOwnerParaIsCell;

	private Stack<string> m_commentGroupCollection = new Stack<string>();

	private bool m_isLevelTextLengthRead;

	private bool m_isFirstPlaceHolderRead;

	private bool m_isPibName;

	private bool m_isPibFlags;

	private string m_href = "";

	private string m_externalLink = "";

	private bool m_isImageHyperlink;

	private string m_linkType = "";

	private string m_uniqueStyleID;

	private Tokens token;

	private Groups group;

	private List<Groups> groupOrder;

	private bool isNested;

	private bool isSpecialCharacter;

	private bool isPlainTagPresent;

	private bool isPardTagpresent;

	private bool m_isDistFromLeft;

	private string m_DistFromLeftVal;

	private RtfFont m_previousRtfFont;

	private int m_listoverrideLevelCount;

	private bool m_isOverrrideListLevel;

	private string m_currListId;

	private bool istblindtypeDefined;

	private float tblindValue;

	private bool istrpaddltypeDefined;

	private float trpaddlValue;

	private WParagraph inCompleteParagraph;

	private byte m_bFlags;

	private bool IsStyleSheet
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

	private bool m_isPnNextList
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

	private bool IsSectionBreak
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

	internal string DefaultCodePage
	{
		get
		{
			if (m_defaultCodePage == null)
			{
				return "windows-1252";
			}
			return m_defaultCodePage;
		}
		set
		{
			m_defaultCodePage = value;
		}
	}

	internal int DefaultFontIndex
	{
		get
		{
			return m_defaultFontIndex;
		}
		set
		{
			m_defaultFontIndex = value;
		}
	}

	public bool IsDestinationControlWord
	{
		get
		{
			if (m_destStack.Count > 0)
			{
				return true;
			}
			return false;
		}
	}

	public bool IsFormFieldGroup
	{
		get
		{
			if (m_formFieldDataStack.Count > 0)
			{
				return true;
			}
			return false;
		}
	}

	public bool IsFieldGroup
	{
		get
		{
			if (m_fieldGroupStack.Count > 0)
			{
				return true;
			}
			return false;
		}
	}

	public int CurrentLevel => m_currentLevel;

	public int PreviousLevel => m_previousLevel;

	public WTextBody CurrentTextBody => m_textBody;

	protected IWParagraph CurrentPara
	{
		get
		{
			if (m_currParagraph == null)
			{
				m_currParagraph = new WParagraph(m_document);
			}
			return m_currParagraph;
		}
		set
		{
			m_currParagraph = value;
		}
	}

	protected Column CurrColumn
	{
		get
		{
			if (m_currColumn == null)
			{
				m_currColumn = (CurrentSection as WSection).AddColumn(1f, 1f, isOpening: true);
			}
			return m_currColumn;
		}
		set
		{
			m_currColumn = value;
		}
	}

	protected IWSection CurrentSection
	{
		get
		{
			if (m_currSection == null)
			{
				m_currSection = m_document.AddSection();
				m_currSection.PageSetup.EqualColumnWidth = true;
				m_textBody = m_currSection.Body;
			}
			return m_currSection;
		}
	}

	protected ListStyle CurrListStyle
	{
		get
		{
			return m_currListStyle;
		}
		set
		{
			m_currListStyle = value;
		}
	}

	protected ListOverrideStyle CurrListOverrideStyle
	{
		get
		{
			return m_currListOverrideStyle;
		}
		set
		{
			m_currListOverrideStyle = value;
		}
	}

	private WComment CurrentComment
	{
		get
		{
			return m_currComment;
		}
		set
		{
			m_currComment = value;
		}
	}

	private List<string> CommentStartIdList
	{
		get
		{
			if (m_commRangeStartId == null)
			{
				m_commRangeStartId = new List<string>();
			}
			return m_commRangeStartId;
		}
	}

	private string CommentLinkText
	{
		get
		{
			return m_commAtnText;
		}
		set
		{
			m_commAtnText = value;
		}
	}

	private Dictionary<string, WComment> Comments
	{
		get
		{
			if (m_comments == null)
			{
				m_comments = new Dictionary<string, WComment>();
			}
			return m_comments;
		}
	}

	protected WListLevel CurrListLevel
	{
		get
		{
			return m_currListLevel;
		}
		set
		{
			m_currListLevel = value;
		}
	}

	private bool IsLevelTextLengthRead
	{
		get
		{
			return m_isLevelTextLengthRead;
		}
		set
		{
			m_isLevelTextLengthRead = value;
		}
	}

	public RtfFont CurrRtfFont
	{
		get
		{
			return m_rtfFont;
		}
		set
		{
			m_rtfFont = value;
		}
	}

	public RtfColor CurrColorTable
	{
		get
		{
			return m_rtfColorTable;
		}
		set
		{
			m_rtfColorTable = value;
		}
	}

	public IWTable CurrTable
	{
		get
		{
			return m_currTable;
		}
		set
		{
			m_currTable = value;
		}
	}

	public WTableRow CurrRow
	{
		get
		{
			return m_currRow;
		}
		set
		{
			m_currRow = value;
		}
	}

	public WTableCell CurrCell
	{
		get
		{
			return m_currCell;
		}
		set
		{
			m_currCell = value;
		}
	}

	public CellFormat CurrCellFormat
	{
		get
		{
			if (m_currCellFormat == null)
			{
				m_currCellFormat = new CellFormat();
			}
			return m_currCellFormat;
		}
		set
		{
			m_currCellFormat = value;
		}
	}

	public RowFormat CurrRowFormat
	{
		get
		{
			if (m_currRowFormat == null)
			{
				m_currRowFormat = new RowFormat(m_document);
			}
			return m_currRowFormat;
		}
		set
		{
			m_currRowFormat = value;
		}
	}

	public TabFormat CurrTabFormat
	{
		get
		{
			if (m_currTabFormat == null)
			{
				m_currTabFormat = new TabFormat();
			}
			return m_currTabFormat;
		}
		set
		{
			m_currTabFormat = value;
		}
	}

	public RtfParser(WordDocument document, Stream stream)
	{
		m_rtfReader = new RtfReader(stream);
		m_lexer = new RtfLexer(m_rtfReader);
		m_document = document;
		m_currTextFormat = new TextFormat();
		m_currParagraphFormat = new WParagraphFormat(m_document);
		m_secFormat = new SecionFormat();
		CurrTable = null;
		m_currRow = null;
		m_currCell = null;
	}

	public void ParseToken()
	{
		int num = 0;
		groupOrder = new List<Groups>();
		InitDefaultCompatibilityOptions();
		m_token = m_lexer.ReadNextToken(m_previousTokenKey, m_bIsLevelText);
		while (m_rtfReader.Position <= m_rtfReader.Length)
		{
			if (m_token == "{")
			{
				group = new Groups();
				groupOrder.Add(group);
				num++;
				m_tokenType = RtfTokenType.GroupStart;
				ParseGroupStart();
				if (m_isCommentReference)
				{
					m_commentGroupCollection.Push(m_token);
				}
			}
			else if (m_token == "}")
			{
				num--;
				if (m_previousToken == "par")
				{
					if (!IsPnListStyleDefined(groupOrder[groupOrder.Count - 1]))
					{
						m_isPnNextList = true;
					}
					else
					{
						m_bIsList = false;
					}
					if (isNested)
					{
						PlainCount(groupOrder[num]);
					}
					groupOrder.RemoveRange(num, 1);
				}
				else if (num != 0)
				{
					groupOrder[num - 1].ChildElements.Add(groupOrder[num]);
					groupOrder.RemoveRange(num, 1);
				}
				m_tokenType = RtfTokenType.GroupEnd;
				ParseGroupEnd();
				if (m_isCommentReference)
				{
					if (m_commentGroupCollection.Count != 0)
					{
						m_commentGroupCollection.Pop();
					}
					else
					{
						m_isCommentReference = false;
						if (!m_textBody.ChildEntities.Contains(CurrentPara))
						{
							m_textBody.ChildEntities.Add(CurrentPara);
						}
						if (m_isCommentOwnerParaIsCell)
						{
							m_textBody = m_currCell;
						}
						else
						{
							m_textBody = CurrentSection.Body;
						}
						CurrentPara = m_textBody.LastParagraph;
						CurrentPara.ChildEntities.Add(CurrentComment);
						if (m_textFormatStack.Count > 1)
						{
							m_textFormatStack.Pop();
						}
						CurrentComment = null;
					}
				}
				if (m_rtfCollectionStack.Count == 0)
				{
					break;
				}
			}
			else if (m_token == ";")
			{
				if (m_previousToken == "colortbl")
				{
					m_currColorIndex = 0;
				}
				if (m_bIsLevelText)
				{
					m_bIsLevelText = false;
					IsLevelTextLengthRead = false;
					m_isFirstPlaceHolderRead = false;
				}
				m_tokenType = RtfTokenType.Unknown;
				m_lexer.CurrRtfTokenType = RtfTokenType.Unknown;
				if (m_currentTableType == RtfTableType.ColorTable && m_previousTokenKey == "blue")
				{
					AddColorTableEntry();
				}
				else if (m_currentTableType == RtfTableType.FontTable)
				{
					AddFontTableEntry();
				}
				else if (m_currentTableType == RtfTableType.StyleSheet)
				{
					AddStyleSheetEntry();
				}
				else
				{
					ParseDocumentElement(m_token);
				}
			}
			else if (StartsWithExt(m_token, "\\"))
			{
				group = groupOrder[groupOrder.Count - 1];
				m_tokenType = RtfTokenType.ControlWord;
				ParseControlStart();
			}
			else if (m_token == "\r" || m_token == "\n" || m_token == string.Empty)
			{
				m_tokenType = RtfTokenType.Unknown;
			}
			else
			{
				ParseDocumentElement(m_token);
			}
			if (m_previousToken == string.Empty && m_prevTokenType == RtfTokenType.ControlWord && (m_token == "\r" || m_token == "\n"))
			{
				ParseParagraphEnd();
			}
			if (m_token != null && m_token != "\r" && m_token != "\n" && m_token != " ")
			{
				m_previousControlString = m_token;
			}
			if (m_token != null && m_tokenType == RtfTokenType.ControlWord && m_token != " ")
			{
				m_previousToken = m_token;
			}
			if (m_token == "emdash" || m_token == "endash")
			{
				m_prevTokenType = RtfTokenType.Text;
			}
			else if (m_prevTokenType != RtfTokenType.Text || (!(m_token == "\r") && !(m_token == "\n")))
			{
				m_prevTokenType = m_tokenType;
			}
			m_token = m_lexer.ReadNextToken(m_previousTokenKey, m_bIsLevelText);
			SetParsedElementFlag(m_token);
			if (m_token.Contains("\\stylesheet") && m_document.Styles.Count > 0 && IsStyleSheet)
			{
				SkipGroup();
			}
			if (m_token.Contains("\\colortbl") && m_colorTable.Count > 0)
			{
				SkipGroup();
			}
			if (m_token.Contains("\\macpict"))
			{
				SkipGroup();
			}
			if (m_token.Contains("\\footnote"))
			{
				SkipGroup();
			}
			if (m_token.Contains("\\txfieldtext") && m_previousControlString == "*")
			{
				SkipGroup();
			}
			if (m_token.Contains("\\shprslt") && ((m_currShape != null && m_currShape.AutoShapeType != AutoShapeType.Unknown) || m_currTextBox != null))
			{
				SkipGroup();
				if (m_currShape != null)
				{
					AddAdjustValues();
				}
				if (m_currShape != null && m_currShape.EffectList.Count != 0 && m_currShape.EffectList[0].IsShadowEffect)
				{
					AddShadowDirectionandDistance();
				}
				if (m_currTextBox != null && m_currTextBox.Shape != null)
				{
					SetDefaultValuesForShapeTextBox();
				}
			}
			if (m_token.Contains("\\shpgrp"))
			{
				SkipGroup();
			}
			if (m_token.Contains("\\formfield"))
			{
				m_currentFormField = new FormFieldData();
				m_formFieldDataStack.Push("{");
			}
			if (StartsWithExt(m_token, "\\jpegblip") || StartsWithExt(m_token, "\\wmetafile") || StartsWithExt(m_token, "\\pngblip") || StartsWithExt(m_token, "\\emfblip") || StartsWithExt(m_token, "\\macpict") || StartsWithExt(m_token, "\\objdata"))
			{
				m_lexer.IsImageBytes = true;
			}
			if (StartsWithExt(m_token, "\\dibitmap"))
			{
				m_lexer.IsImageBytes = true;
				SkipGroup();
				m_lexer.IsImageBytes = false;
			}
			if (StartsWithExt(m_token, "\\wmetafile"))
			{
				m_bIsStandardPictureSizeNeedToBePreserved = true;
			}
		}
		if (m_previousToken != "sv" && isWrapPolygon)
		{
			isWrapPolygon = false;
		}
		m_lexer.Close();
		m_currentLevel = 0;
		m_bInTable = false;
		if (m_textBody != null && m_textBody.Items.LastItem != CurrentSection.Body.Items.LastItem)
		{
			ParseParagraphEnd();
		}
		if (CurrentPara.Items.Count > 0)
		{
			CurrentSection.Paragraphs.Add(CurrentPara);
			if (m_paragraphFormatStack.Count > 0)
			{
				CopyParagraphFormatting(m_currParagraphFormat, CurrentPara.ParagraphFormat);
			}
		}
		if (CurrentSection.Body.ChildEntities.Count > 0)
		{
			AddNewSection(CurrentSection);
			ApplySectionFormatting();
		}
		Close();
	}

	private void PlainCount(Groups group)
	{
		Tokens tokens = null;
		int num = 0;
		foreach (Groups childElement in group.ChildElements)
		{
			if (childElement is Tokens)
			{
				tokens = childElement as Tokens;
				if (tokens.TokenName == "plain" && !m_bIsListText)
				{
					num++;
				}
				else if (tokens.TokenName == "atrfend")
				{
					num--;
				}
			}
		}
		while (num > 0 && m_textFormatStack.Count > 1)
		{
			m_textFormatStack.Pop();
			num--;
		}
		if (m_textFormatStack.Count > 0)
		{
			m_currTextFormat = m_textFormatStack.Peek();
		}
	}

	private bool IsPnListStyleDefined(Groups group)
	{
		Tokens tokens = null;
		bool flag = false;
		for (int i = 0; i < group.ChildElements.Count; i++)
		{
			Groups groups = group.ChildElements[i];
			if (!flag)
			{
				foreach (Groups childElement in groups.ChildElements)
				{
					if (childElement is Tokens)
					{
						tokens = childElement as Tokens;
						if (tokens.TokenName == "pnlvlbody" || tokens.TokenName == "pnlvlcont" || tokens.TokenName == "pnlvlblt")
						{
							flag = true;
						}
					}
				}
				continue;
			}
			return true;
		}
		return flag;
	}

	private bool IsPnListStyleDefinedExisting(Groups group)
	{
		Tokens tokens = null;
		bool flag = false;
		int num = 0;
		int num2 = 0;
		int num3 = group.ChildElements.Count - 1;
		while (num3 > 0 && num <= 1 && num2 <= 0)
		{
			Groups groups = group.ChildElements[num3];
			if (!flag)
			{
				if (group.ChildElements[num3] is Tokens)
				{
					tokens = group.ChildElements[num3] as Tokens;
					if (tokens.TokenName == "ilvl" || tokens.TokenName == "ls")
					{
						flag = false;
						break;
					}
					if (tokens.TokenName == "pnlvlbody")
					{
						flag = true;
						break;
					}
					if (tokens.TokenName == "par")
					{
						num++;
					}
				}
				foreach (Groups childElement in groups.ChildElements)
				{
					if (childElement is Tokens)
					{
						tokens = childElement as Tokens;
						if (tokens.TokenName == "ilvl" || tokens.TokenName == "ls")
						{
							flag = false;
							num2++;
							break;
						}
						if (tokens.TokenName == "pnlvlbody")
						{
							flag = true;
							num2++;
							break;
						}
						if (tokens.TokenName == "par")
						{
							num++;
						}
					}
				}
			}
			num3--;
		}
		return flag;
	}

	private void InitDefaultCompatibilityOptions()
	{
		m_document.Settings.SetCompatibilityModeValue(CompatibilityMode.Word2007);
		m_document.DOP.Dop2000.Copts.DontUseHTMLParagraphAutoSpacing = true;
		m_document.DOP.Dop2000.Copts.DontBreakWrappedTables = true;
		m_document.DOP.Dop2000.Copts.Copts80.Copts60.NoSpaceRaiseLower = false;
	}

	public void Close()
	{
		m_lexer = null;
		m_rtfReader = null;
		m_previousToken = null;
		m_previousTokenKey = null;
		m_previousTokenValue = null;
		m_fontTable.Clear();
		m_listOverrideTable.Clear();
		m_listTable.Clear();
		m_colorTable.Clear();
		m_styleTable.Clear();
		m_fontTable = null;
		m_listOverrideTable = null;
		m_listTable = null;
		m_colorTable = null;
		m_styleTable = null;
		m_tabFormatStack.Clear();
		m_tabFormatStack = null;
		m_stack.Clear();
		m_stack = null;
		m_textFormatStack.Clear();
		m_textFormatStack = null;
		m_unicodeCountStack.Clear();
		m_unicodeCountStack = null;
		m_shapeInstructionStack.Clear();
		m_shapeInstructionStack = null;
		m_shapeTextStack.Clear();
		m_shapeTextStack = null;
		m_shapeTextBody.Clear();
		m_shapeTextBody = null;
		m_shapeParagraph.Clear();
		m_shapeParagraph = null;
		m_rtfCollectionStack.Clear();
		m_rtfCollectionStack = null;
		m_prevRowFormatStack.Clear();
		m_prevRowFormatStack = null;
		m_prevCellFormatStack.Clear();
		m_prevCellFormatStack = null;
		m_pictureOrShapeStack.Clear();
		m_pictureOrShapeStack = null;
		m_groupShapeStack.Clear();
		m_groupShapeStack = null;
		m_shapeFlagStack.Clear();
		m_shapeFlagStack = null;
		m_listLevelStack.Clear();
		m_listLevelStack = null;
		m_headerFooterStack.Clear();
		m_headerFooterStack = null;
		m_formFieldDataStack.Clear();
		m_formFieldDataStack = null;
		m_fieldResultGroupStack.Clear();
		m_fieldResultGroupStack = null;
		m_fieldGroupTypeStack.Clear();
		m_fieldGroupTypeStack = null;
		m_fieldGroupStack.Clear();
		m_fieldGroupStack = null;
		m_fieldCollectionStack.Clear();
		m_fieldCollectionStack = null;
		m_destStack.Clear();
		m_destStack = null;
		m_currRowFormatStack.Clear();
		m_currRowFormatStack = null;
		m_CellFormatStack.Clear();
		m_CellFormatStack = null;
		groupOrder.Clear();
		groupOrder = null;
		m_drawingFields.Clear();
		m_drawingFields = null;
		m_shapeTextbodyStack.Clear();
		m_shapeTextbodyStack = null;
		m_ownerShapeTextbodyStack.Clear();
		m_ownerShapeTextbodyStack = null;
	}

	private void AddFontTableEntry()
	{
		bool flag = false;
		foreach (KeyValuePair<string, RtfFont> item in m_fontTable)
		{
			if (item.Key == m_rtfFont.FontID)
			{
				flag = true;
			}
		}
		if (m_rtfFont.FontName == null || m_rtfFont.FontID == null)
		{
			return;
		}
		if (flag)
		{
			m_fontTable[m_rtfFont.FontID].FontName = m_rtfFont.FontName;
		}
		else
		{
			m_fontTable.Add(m_rtfFont.FontID, m_rtfFont);
		}
		if (m_rtfFont.AlternateFontName != null)
		{
			if (m_document.FontSubstitutionTable.ContainsKey(m_rtfFont.FontName))
			{
				m_document.FontSubstitutionTable[m_rtfFont.FontName] = m_rtfFont.AlternateFontName;
			}
			else
			{
				m_document.FontSubstitutionTable.Add(m_rtfFont.FontName, m_rtfFont.AlternateFontName);
			}
		}
	}

	private void AddColorTableEntry()
	{
		m_colorTable.Add(++m_currColorIndex, m_rtfColorTable);
		m_rtfColorTable = new RtfColor();
	}

	private void AddStyleSheetEntry()
	{
		if (m_styleName == null || (m_styleName != null && m_styleName.Length == 0))
		{
			m_styleName = "Style" + Guid.NewGuid();
		}
		if (m_currStyle == null && m_currStyleID == null && m_currParagraphFormat != null)
		{
			m_currStyleID = string.Empty;
			m_currStyle = new WParagraphStyle(m_document);
		}
		if (m_currStyle != null)
		{
			Dictionary<string, string> builtinStyles = (m_currStyle as Style).GetBuiltinStyles();
			if (builtinStyles.ContainsKey(m_styleName.ToLower()))
			{
				m_styleName = builtinStyles[m_styleName.ToLower()];
			}
			(m_currStyle as Style).SetStyleName(m_styleName);
			Style style = IsStylePresent(m_currStyle.Name, StyleType.ParagraphStyle);
			if (style == null)
			{
				IWParagraphStyle iWParagraphStyle = m_document.AddParagraphStyle(m_currStyle.Name);
				CopyParagraphFormatting(m_currParagraphFormat, iWParagraphStyle.ParagraphFormat);
				UpdateTabsCollection(iWParagraphStyle.ParagraphFormat);
				CopyTextFormatToCharFormat(iWParagraphStyle.CharacterFormat, m_currTextFormat);
				if (m_styleTable != null && m_currStyle != null && m_currStyleID != null && !m_styleTable.ContainsKey(m_currStyleID))
				{
					m_styleTable.Add(m_currStyleID, m_currStyle);
				}
			}
			else
			{
				m_currStyle = style as IWParagraphStyle;
				CopyParagraphFormatting(m_currParagraphFormat, m_currStyle.ParagraphFormat);
				UpdateTabsCollection(m_currStyle.ParagraphFormat);
				CopyTextFormatToCharFormat(m_currStyle.CharacterFormat, m_currTextFormat);
			}
		}
		if (m_currCharStyle != null)
		{
			m_currCharStyle.SetStyleName(m_styleName);
			CopyTextFormatToCharFormat(m_currCharStyle.CharacterFormat, m_currTextFormat);
			if (IsStylePresent(m_currCharStyle.Name, StyleType.CharacterStyle) == null)
			{
				m_document.Styles.Add(m_currCharStyle);
				if (m_charStyleTable != null && m_currCharStyle != null && m_currStyleID != null && !m_charStyleTable.ContainsKey(m_currStyleID))
				{
					m_charStyleTable.Add(m_currStyleID, m_currCharStyle);
				}
			}
		}
		m_styleName = string.Empty;
		m_currStyle = null;
		m_currCharStyle = null;
	}

	private Style IsStylePresent(string styleName, StyleType styleType)
	{
		foreach (Style style in m_document.Styles)
		{
			if (style.Name == styleName && style.StyleType == styleType)
			{
				return style;
			}
		}
		return null;
	}

	private void ParseControlStart()
	{
		m_bIsAccentChar = false;
		m_lexer.CurrRtfTokenType = RtfTokenType.ControlWord;
		if (m_token.EndsWith("?"))
		{
			m_token = m_token.TrimEnd('?');
		}
		m_token = m_token.Trim();
		m_token = m_token.Substring(1);
		if (m_token == "\\" || m_token == "{" || m_token == "}")
		{
			ParseDocumentElement(m_token);
		}
		if (m_token == "*" && !IsDestinationControlWord)
		{
			m_destStack.Push("{");
		}
		string[] array = SeperateToken(m_token);
		token = new Tokens();
		token.TokenName = array[0];
		token.TokenValue = array[1];
		group.ChildElements.Add(token);
		if (array[0] != null && (StartsWithExt(array[0], "atnid") || StartsWithExt(array[0], "atnauthor")))
		{
			array = SeparateAnnotationToken(array);
		}
		if (array[0] != null && StartsWithExt(array[0], "atnparent"))
		{
			array[0] = "atnparent";
		}
		if (StartsWithExt(m_token, "cellx"))
		{
			if (!m_bIsBorderLeft && CurrCellFormat.Borders.Left.BorderType == BorderStyle.None)
			{
				CurrCellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
			}
			if (!m_bIsBorderRight && CurrCellFormat.Borders.Right.BorderType == BorderStyle.None)
			{
				CurrCellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
			}
			if (!m_bIsBorderTop && CurrCellFormat.Borders.Top.BorderType == BorderStyle.None)
			{
				CurrCellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
			}
			if (!m_bIsBorderBottom && CurrCellFormat.Borders.Bottom.BorderType == BorderStyle.None)
			{
				CurrCellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;
			}
		}
		if (m_token == "defshp")
		{
			m_bIsFallBackImage = true;
		}
		ParseControlWords(m_token, array[0], array[1], array[2]);
		m_previousTokenKey = array[0];
		m_previousTokenValue = array[1];
	}

	private string[] SeparateAnnotationToken(string[] value)
	{
		string text = string.Empty;
		if (StartsWithExt(value[0], "atnid"))
		{
			text = value[0].Substring(0, "atnid".Length);
		}
		if (StartsWithExt(value[0], "atnauthor"))
		{
			text = value[0].Substring(0, "atnauthor".Length);
		}
		int length = value[0].Length - 1 - (text.Length - 1);
		int length2 = text.Length;
		value[1] = value[0].Substring(length2, length) + value[1] + value[2];
		value[0] = text;
		return value;
	}

	private bool IsNestedGroup()
	{
		if (m_currentTableType != RtfTableType.None || m_bIsListText || m_bIsDocumentInfo || m_bIsCustomProperties)
		{
			return true;
		}
		return false;
	}

	private void ParseGroupStart()
	{
		m_lexer.CurrRtfTokenType = RtfTokenType.GroupStart;
		if (IsNestedGroup())
		{
			m_stack.Push("{");
		}
		if (m_bIsPictureOrShape)
		{
			m_pictureOrShapeStack.Push("{");
		}
		if (IsDestinationControlWord)
		{
			m_destStack.Push("{");
		}
		if (m_bIsHeader || m_bIsFooter)
		{
			m_headerFooterStack.Push("{");
		}
		if (IsFieldGroup)
		{
			if (m_fieldResultGroupStack.Count > 0)
			{
				m_fieldResultGroupStack.Push(m_fieldResultGroupStack.Pop() + 1);
			}
			if (m_fieldInstructionGroupStack.Count > 0)
			{
				m_fieldInstructionGroupStack.Push(m_fieldInstructionGroupStack.Pop() + 1);
			}
			if (m_fieldGroupStack.Count > 0)
			{
				m_fieldGroupStack.Push(m_fieldGroupStack.Pop() + 1);
			}
		}
		if (m_bIsListLevel)
		{
			m_listLevelStack.Push("\\");
		}
		if (m_bIsBackgroundCollection)
		{
			m_backgroundCollectionStack.Push("\\");
		}
		if (m_currentTableType == RtfTableType.None)
		{
			m_textFormatStack.Push(m_currTextFormat.Clone());
			m_currTextFormat = m_textFormatStack.Peek();
			m_tabFormatStack.Push(new Dictionary<int, TabFormat>(m_tabCollection));
			m_tabCollection = m_tabFormatStack.Peek();
			WParagraphFormat wParagraphFormat = new WParagraphFormat(m_document);
			if (m_paragraphFormatStack.Count > 0)
			{
				CopyParagraphFormatting(m_currParagraphFormat, wParagraphFormat);
			}
			m_paragraphFormatStack.Push(wParagraphFormat);
			m_currParagraphFormat = m_paragraphFormatStack.Peek();
		}
		if (m_rtfCollectionStack.Count > 0)
		{
			m_rtfCollectionStack.Push("\\");
		}
		if (m_bIsShapeInstruction)
		{
			m_shapeInstructionStack.Push("{");
		}
		if (m_bIsShapeText)
		{
			m_shapeTextStack.Push("{");
		}
		if (m_bIsGroupShape)
		{
			m_groupShapeStack.Push("{");
		}
		if (m_bIsObject)
		{
			m_objectStack.Push("{");
		}
		if (IsFormFieldGroup)
		{
			m_formFieldDataStack.Push("{");
		}
	}

	private void ParseGroupEnd()
	{
		m_lexer.CurrRtfTokenType = RtfTokenType.GroupEnd;
		if (m_unicodeCountStack.Count > 0)
		{
			m_unicodeCountStack.Pop();
		}
		m_unicodeCount = 0;
		if (IsNestedGroup())
		{
			m_stack.Pop();
			if (m_stack.Count == 0)
			{
				if (m_bIsListText)
				{
					m_bIsListText = false;
					CopyParagraphFormatting(CurrentPara.ParagraphFormat, m_listLevelParaFormat);
					CopyTextFormatToCharFormat(m_listLevelCharFormat, m_currTextFormat);
					if (m_currTextFormat.FontFamily == string.Empty)
					{
						ResetListFontName(m_listLevelCharFormat);
					}
					CurrentPara = m_prevParagraph;
					m_currTextFormat = m_prevTextFormat;
					if (m_previousRtfFont != null)
					{
						CurrRtfFont = m_previousRtfFont;
					}
					m_previousRtfFont = null;
				}
				m_bIsDocumentInfo = false;
				m_bIsCustomProperties = false;
				if (m_currentTableType == RtfTableType.FontTable && m_rtfFont != null && m_rtfFont.FontID != null && !m_fontTable.ContainsKey(m_rtfFont.FontID))
				{
					AddFontTableEntry();
				}
				m_currentTableType = RtfTableType.None;
				m_lexer.CurrRtfTableType = RtfTableType.None;
			}
		}
		if (IsDestinationControlWord)
		{
			m_destStack.Pop();
		}
		if (m_currentTableType == RtfTableType.StyleSheet)
		{
			m_tabCollection.Clear();
			m_tabCount = 0;
			m_tabFormatStack.Clear();
			m_tabFormatStack.Push(new Dictionary<int, TabFormat>(m_tabCollection));
		}
		if (m_currentTableType == RtfTableType.FontTable && m_rtfFont != null && m_rtfFont.FontID != null && !m_fontTable.ContainsKey(m_rtfFont.FontID))
		{
			AddFontTableEntry();
		}
		if (m_bIsHeader || m_bIsFooter)
		{
			m_headerFooterStack.Pop();
			if (m_headerFooterStack.Count == 0)
			{
				if (m_currParagraph != null && m_currParagraph.ChildEntities.Count != 0)
				{
					ParseParagraphEnd();
				}
				if (m_currTable != null)
				{
					ProcessTableInfo(isShapeTextEnd: false);
				}
				if (inCompleteParagraph != null)
				{
					int num;
					for (num = 0; num < inCompleteParagraph.ChildEntities.Count; num++)
					{
						CurrentPara.ChildEntities.Add(inCompleteParagraph.ChildEntities[num]);
						num--;
					}
					inCompleteParagraph = null;
				}
				m_bIsHeader = false;
				m_bIsFooter = false;
				m_textBody = CurrentSection.Body;
				m_currTextFormat = new TextFormat();
				m_textFormatStack.Clear();
				m_textFormatStack.Push(m_currTextFormat);
				m_tabFormatStack.Clear();
				m_paragraphFormatStack.Clear();
			}
		}
		if (IsFieldGroup)
		{
			ParseGroupEndWithinFieldGroup();
		}
		if (m_bIsListLevel)
		{
			if (m_listLevelStack.Count > 0)
			{
				m_listLevelStack.Pop();
			}
			if (m_listLevelStack.Count == 0)
			{
				CopyParagraphFormatting(CurrentPara.ParagraphFormat, CurrListLevel.ParagraphFormat);
				CopyTextFormatToCharFormat(CurrListLevel.CharacterFormat, m_currTextFormat);
				CurrentPara = null;
				m_currTextFormat = new TextFormat();
				m_bIsListLevel = false;
			}
			if (m_bIsLevelText)
			{
				m_bIsLevelText = false;
				IsLevelTextLengthRead = false;
				m_isFirstPlaceHolderRead = false;
			}
		}
		if (m_bIsShapeText)
		{
			m_shapeTextStack.Pop();
			if (m_shapeTextStack.Count == 0)
			{
				if (m_currParagraph.ChildEntities.Count > 0)
				{
					if (IsFieldGroup && m_currentFieldGroupData != string.Empty)
					{
						ParseFieldGroupData(m_currentFieldGroupData);
						m_currentFieldGroupData = string.Empty;
					}
					ParseParagraphEnd();
					isPlainTagPresent = false;
					isPardTagpresent = false;
				}
				m_bIsShapeText = false;
				if (IsFieldGroup && m_currentFieldGroupData != string.Empty)
				{
					ParseFieldGroupData(m_currentFieldGroupData);
					m_currentFieldGroupData = string.Empty;
				}
				ResetParagraphFormat(m_currParagraphFormat);
				ResetCharacterFormat(CurrentPara.BreakCharacterFormat);
				CopyParagraphFormatting(m_currParagraphFormat, CurrentPara.ParagraphFormat);
				ProcessTableInfo(isShapeTextEnd: true);
				if (m_previousLevel != m_currentLevel)
				{
					m_previousLevel = m_currentLevel;
				}
				if (CurrCell != null)
				{
					CopyTextFormatToCharFormat(CurrCell.CharacterFormat, m_currTextFormat);
				}
				m_bCellFinished = true;
				m_currParagraph = new WParagraph(m_document);
				ParseRowEnd(isShapeTextEnd: true);
				IWTable currTable = m_currTable;
				bool num2 = m_currTable != null;
				int count = m_nestedTable.Count;
				ProcessTableInfo(isShapeTextEnd: true);
				bool num3 = num2 && m_currTable == null;
				ResetShapeTextbodyStack();
				if (num3 || count > m_nestedTable.Count)
				{
					m_previousLevel = m_currentLevel;
				}
				WTextBody wTextBody = ((m_textBody != null && m_textBody.ChildEntities.LastItem is WTable) ? (m_textBody.ChildEntities.LastItem as WTable).LastCell : null);
				if (currTable != null && m_textBody != null && currTable == m_textBody.ChildEntities.LastItem && m_textBody.ChildEntities.LastItem is WTable && (m_textBody.ChildEntities.LastItem as WTable).Rows.Count > 1)
				{
					MoveItemsToShape(wTextBody);
					(m_textBody.ChildEntities.LastItem as WTable).LastRow.RemoveSelf();
				}
				else if (wTextBody != null)
				{
					MoveItemsToShape(wTextBody);
					m_textBody.ChildEntities.LastItem.RemoveSelf();
				}
			}
		}
		if (m_bIsPictureOrShape)
		{
			if (m_pictureOrShapeStack.Count > 0)
			{
				m_pictureOrShapeStack.Pop();
				m_bIsShapeResult = m_bIsShapeResult && m_bShapeResultStackCount <= m_pictureOrShapeStack.Count;
			}
			if (m_pictureOrShapeStack.Count == 0)
			{
				m_bIsPictureOrShape = false;
				m_lexer.IsImageBytes = false;
				m_bIsFallBackImage = false;
				if (m_ownerShapeTextbodyStack.Count > 0)
				{
					ResetOwnerShapeStack();
				}
			}
		}
		if (m_bIsGroupShape)
		{
			if (m_groupShapeStack.Count > 0)
			{
				m_groupShapeStack.Pop();
			}
			if (m_groupShapeStack.Count == 0)
			{
				m_bIsGroupShape = false;
			}
		}
		if (m_bIsBackgroundCollection)
		{
			if (m_backgroundCollectionStack.Count > 0)
			{
				m_backgroundCollectionStack.Pop();
			}
			if (m_backgroundCollectionStack.Count == 0)
			{
				m_bIsBackgroundCollection = false;
			}
		}
		if (m_currentTableType == RtfTableType.None)
		{
			if (m_textFormatStack.Count > 1)
			{
				m_textFormatStack.Pop();
			}
			if (m_textFormatStack.Count > 0)
			{
				m_currTextFormat = m_textFormatStack.Peek();
			}
			if (m_tabFormatStack.Count > 1)
			{
				m_tabFormatStack.Pop();
			}
			if (m_tabFormatStack.Count > 0)
			{
				m_tabCollection = m_tabFormatStack.Peek();
			}
			if (m_paragraphFormatStack.Count > 1)
			{
				m_paragraphFormatStack.Pop();
			}
			if (m_paragraphFormatStack.Count > 0)
			{
				m_currParagraphFormat = m_paragraphFormatStack.Peek();
			}
		}
		if (m_drawingFieldName != null && m_drawingFieldValue != null)
		{
			if (m_drawingFieldName == "shapeType" && m_drawingFieldValue != "75" && m_drawingFieldValue != "202" && !m_bIsGroupShape)
			{
				AutoShapeType autoShapeType = GetAutoShapeType(m_drawingFieldValue);
				m_currShape = CurrentPara.AppendShape(autoShapeType, m_currShapeFormat.m_width, m_currShapeFormat.m_height);
				m_bIsShape = true;
				m_currShape.WrapFormat.AllowOverlap = true;
				m_currShape.LineFormat.m_Weight = 0.75f;
				ApplyShapeFormatting(m_currShape, m_picFormat, m_currShapeFormat);
				ParseDrawingFields();
			}
			if (m_drawingFieldName == "shapeType" && m_drawingFieldValue == "202" && !m_bIsGroupShape)
			{
				m_currTextBox = CurrentPara.AppendTextBox(m_currShapeFormat.m_width, m_currShapeFormat.m_height) as WTextBox;
				m_currTextBox.TextBoxFormat.AllowOverlap = true;
				m_currTextBox.Shape = new Shape(m_document, AutoShapeType.Rectangle);
				m_currTextBox.Shape.Height = m_currShapeFormat.m_height;
				m_currTextBox.Shape.Width = m_currShapeFormat.m_width;
				m_currTextBox.Shape.WrapFormat.AllowOverlap = true;
				ApplyShapeFormatting(m_currTextBox.Shape, m_picFormat, m_currShapeFormat);
				ApplyTextBoxFormatting(m_currTextBox, m_picFormat, m_currShapeFormat);
				ParseDrawingFields();
			}
			if (m_currShape == null)
			{
				m_bIsShapePicture = (!(m_drawingFieldName == "fHorizRule") || !(m_drawingFieldValue == "1")) && m_bIsShapePicture;
			}
			if ((m_currPicture != null && m_bIsPictureOrShape && m_bIsShapePicture) || ((m_currShape != null || m_currTextBox != null) && m_bIsShape))
			{
				ParseShapeToken(m_drawingFieldName, m_drawingFieldName, m_drawingFieldValue);
				ApplyTextBoxFormatsToShape();
			}
			else if (!string.IsNullOrEmpty(m_drawingFieldName) && !string.IsNullOrEmpty(m_drawingFieldValue) && m_drawingFieldName.ToLower() == "shapetype")
			{
				if (int.TryParse(m_drawingFieldValue, out var result))
				{
					SetShapeElementsFlag(result);
				}
			}
			else if (m_currShape == null && m_currTextBox == null && m_currPicture == null)
			{
				m_drawingFields.Add(new TempShapeProperty(m_drawingFieldName, m_drawingFieldValue));
			}
			m_drawingFieldValue = null;
			m_drawingFieldName = null;
		}
		if (m_rtfCollectionStack.Count > 0)
		{
			m_rtfCollectionStack.Pop();
		}
		if (m_bIsShapeInstruction)
		{
			m_shapeInstructionStack.Pop();
			if (m_shapeInstructionStack.Count == 0)
			{
				m_bIsShapeInstruction = false;
			}
		}
		if (m_bIsObject)
		{
			m_objectStack.Pop();
			if (m_objectStack.Count == 0)
			{
				m_bIsObject = false;
			}
		}
		if (IsFormFieldGroup)
		{
			m_formFieldDataStack.Pop();
			if (m_formFieldDataStack.Count == 0)
			{
				WriteFormFieldProperties();
			}
		}
	}

	private void ParseDrawingFields()
	{
		foreach (TempShapeProperty drawingField in m_drawingFields)
		{
			ParseShapeToken(drawingField.m_drawingFieldName, drawingField.m_drawingFieldName, drawingField.m_drawingFieldValue);
			ApplyTextBoxFormatsToShape();
		}
		m_drawingFields.Clear();
	}

	private void ParsePictureDrawingFields()
	{
		foreach (TempShapeProperty drawingField in m_drawingFields)
		{
			ParsePictureToken(drawingField.m_drawingFieldName, drawingField.m_drawingFieldName, drawingField.m_drawingFieldValue);
		}
		m_drawingFields.Clear();
	}

	private void MoveItemsToShape(WTextBody textBody)
	{
		if (m_currShape != null)
		{
			foreach (Entity childEntity in textBody.ChildEntities)
			{
				m_currShape.TextBody.ChildEntities.Add(childEntity.Clone());
			}
			return;
		}
		if (m_currTextBox == null)
		{
			return;
		}
		foreach (Entity childEntity2 in textBody.ChildEntities)
		{
			m_currTextBox.TextBoxBody.ChildEntities.Add(childEntity2.Clone());
		}
	}

	private void ParseGroupEndWithinFieldGroup()
	{
		if (m_currentFieldGroupData != string.Empty)
		{
			ParseFieldGroupData(m_currentFieldGroupData);
			m_currentFieldGroupData = string.Empty;
		}
		EnsureFieldSubGroupEnd(FieldGroupType.FieldResult);
		EnsureFieldSubGroupEnd(FieldGroupType.FieldInstruction);
		EnsureFieldGroupEnd();
	}

	private void EnsureFieldSubGroupEnd(FieldGroupType fieldGroupType)
	{
		switch (fieldGroupType)
		{
		case FieldGroupType.FieldInstruction:
		{
			if (m_fieldInstructionGroupStack.Count <= 0)
			{
				break;
			}
			int num = m_fieldInstructionGroupStack.Pop();
			num--;
			if (num == 0)
			{
				if (m_fieldGroupTypeStack.Count > 0 && m_fieldGroupTypeStack.Peek() == fieldGroupType)
				{
					m_fieldGroupTypeStack.Pop();
				}
			}
			else
			{
				m_fieldInstructionGroupStack.Push(num);
			}
			break;
		}
		case FieldGroupType.FieldResult:
		{
			if (m_fieldResultGroupStack.Count <= 0)
			{
				break;
			}
			int num = m_fieldResultGroupStack.Pop();
			num--;
			if (num == 0)
			{
				if (m_fieldGroupTypeStack.Count > 0 && m_fieldGroupTypeStack.Peek() == fieldGroupType)
				{
					m_fieldGroupTypeStack.Pop();
				}
			}
			else
			{
				m_fieldResultGroupStack.Push(num);
			}
			break;
		}
		}
	}

	private void EnsureFieldGroupEnd()
	{
		int num = m_fieldGroupStack.Pop();
		num--;
		if (num == 0)
		{
			if (m_fieldCollectionStack.Count > 0)
			{
				_ = CurrentPara.Items.Count;
				_ = 0;
				WFieldMark wFieldMark = new WFieldMark(m_document);
				wFieldMark.Type = FieldMarkType.FieldEnd;
				CurrentPara.ChildEntities.Add(wFieldMark);
				m_fieldCollectionStack.Peek().FieldEnd = wFieldMark;
			}
			if (m_fieldCollectionStack.Count > 0)
			{
				WField wField = m_fieldCollectionStack.Pop();
				if (wField.FieldType == FieldType.FieldUnknown)
				{
					wField.UpdateUnknownFieldType(null);
				}
				else if (wField.FieldType != FieldType.FieldTOC)
				{
					string fieldCodeForUnknownFieldType = wField.GetFieldCodeForUnknownFieldType();
					wField.UpdateFieldCode(fieldCodeForUnknownFieldType);
				}
			}
		}
		else
		{
			m_fieldGroupStack.Push(num);
		}
	}

	private void WriteFormFieldProperties()
	{
		WFormField wFormField = null;
		if (m_fieldCollectionStack.Count > 0)
		{
			wFormField = m_fieldCollectionStack.Peek() as WFormField;
		}
		if (wFormField != null)
		{
			ApplyFormFieldProperties(wFormField);
			switch (wFormField.FormFieldType)
			{
			case FormFieldType.TextInput:
				ApplyTextFormFieldProperties(wFormField as WTextFormField);
				break;
			case FormFieldType.DropDown:
				ApplyDropDownFormFieldProperties(wFormField as WDropDownFormField);
				break;
			case FormFieldType.CheckBox:
				ApplyCheckboxPorperties(wFormField as WCheckBox);
				break;
			}
		}
	}

	private string RemoveDelimiterSpace(string token)
	{
		if ((!StartsWithExt(m_previousControlString, "u") || m_previousControlString.Length <= 1 || !char.IsNumber(m_previousControlString[1]) || !(m_previousTokenKey == "u")) && (m_previousControlString == "}" || m_lexer.CurrRtfTokenType == RtfTokenType.Text || StartsWithExt(m_token, "u") || StartsWithExt(m_previousControlString, "'") || (m_tokenType == RtfTokenType.Unknown && !m_bIsListText && token != null && !m_bIsBackgroundCollection)))
		{
			return token;
		}
		if (m_token.Length > 1)
		{
			if (m_tokenType != 0 && !isSpecialCharacter)
			{
				token = token.Substring(1, token.Length - 1);
			}
			return token;
		}
		return null;
	}

	private bool IsPictureToken()
	{
		if (m_bIsPictureOrShape && m_bIsShapePicture && m_lexer.IsImageBytes)
		{
			return true;
		}
		return false;
	}

	private void ParseDocumentElement(string m_token)
	{
		if (StartsWithExt(m_token, " "))
		{
			m_token = RemoveDelimiterSpace(m_token);
		}
		if (!string.IsNullOrEmpty(m_token) && !string.IsNullOrEmpty(m_previousControlString) && StartsWithExt(m_previousControlString, "u") && ((m_previousControlString.Length > 1 && m_previousTokenKey == "u" && char.IsNumber(m_previousControlString[1])) || (m_previousControlString.Length > 2 && m_previousTokenKey == "u-" && char.IsNumber(m_previousControlString[2]))))
		{
			if (m_unicodeCountStack.Count > 0)
			{
				if (m_token.Length >= m_unicodeCount)
				{
					m_token = m_token.Substring(m_unicodeCount);
					m_unicodeCount = 0;
				}
				else
				{
					m_token = string.Empty;
					m_unicodeCount -= m_token.Length;
				}
			}
			else
			{
				m_token = m_token.Substring(1);
			}
		}
		if (m_bIsBackgroundCollection && IsPictureToken() && !m_bIsGroupShape)
		{
			ParseImageBytes();
		}
		if (m_bIsListText || m_token == null || m_bIsBackgroundCollection)
		{
			return;
		}
		m_tokenType = RtfTokenType.Text;
		m_lexer.CurrRtfTokenType = RtfTokenType.Text;
		if (IsPictureToken() && !m_bIsGroupShape)
		{
			ParseImageBytes();
		}
		else if (m_bIsBookmarkStart)
		{
			CurrentPara.AppendBookmarkStart(m_token);
			m_bIsBookmarkStart = false;
		}
		else if (m_bIsBookmarkEnd)
		{
			if (!(CurrentPara.ChildEntities.LastItem is BookmarkEnd) || !((CurrentPara.ChildEntities.LastItem as BookmarkEnd).Name == m_token))
			{
				CurrentPara.AppendBookmarkEnd(m_token);
			}
			m_bIsBookmarkEnd = false;
		}
		else if (m_bIsCustomProperties)
		{
			ParseCustomDocumentProperties(m_token);
		}
		else if (m_bIsDocumentInfo)
		{
			ParseBuiltInDocumentProperties(m_token);
		}
		else if (IsFormFieldGroup && m_currentFormField != null)
		{
			ParseFormFieldDestinationWords(m_token);
		}
		else if (IsFieldGroup && !m_bIsPictureOrShape && m_previousToken != "sn" && m_previousToken != "sv")
		{
			m_currentFieldGroupData += m_token;
		}
		else if (m_isCommentRangeStart && !m_bIsPictureOrShape && !m_lexer.IsImageBytes)
		{
			CommentLinkText = m_token;
			if (!isSpecialCharacter)
			{
				m_token = GetEncodedString(m_token);
			}
			else
			{
				isSpecialCharacter = false;
			}
			tr = CurrentPara.AppendText(m_token);
			CopyTextFormatToCharFormat(tr.CharacterFormat, m_currTextFormat);
			if (!isPlainTagPresent && m_document.LastParagraph != null && IsDefaultTextFormat(m_currTextFormat))
			{
				if (m_document.LastParagraph.BreakCharacterFormat.HasValue(106))
				{
					m_document.LastParagraph.BreakCharacterFormat.PropertiesHash.Remove(106);
				}
				CurrentPara.BreakCharacterFormat.CopyFormat(m_document.LastParagraph.BreakCharacterFormat);
				tr.CharacterFormat.CopyProperties(m_document.LastParagraph.BreakCharacterFormat);
			}
			CurrentPara.Items.Add(tr);
		}
		else if (m_isCommentReference && !m_bIsPictureOrShape && !m_bIsDocumentInfo)
		{
			if (m_bIsList && CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel == null && CurrentPara.ListFormat.CurrentListStyle == null)
			{
				CurrentPara.ListFormat.ContinueListNumbering();
			}
			if (!isSpecialCharacter)
			{
				m_token = GetEncodedString(m_token);
			}
			else
			{
				isSpecialCharacter = false;
			}
			tr = CurrentPara.AppendText(m_token);
			CopyTextFormatToCharFormat(tr.CharacterFormat, m_currTextFormat);
			if (!isPlainTagPresent && m_document.LastParagraph != null && IsDefaultTextFormat(m_currTextFormat))
			{
				if (m_document.LastParagraph.BreakCharacterFormat.HasValue(106))
				{
					m_document.LastParagraph.BreakCharacterFormat.PropertiesHash.Remove(106);
				}
				CurrentPara.BreakCharacterFormat.CopyFormat(m_document.LastParagraph.BreakCharacterFormat);
				tr.CharacterFormat.CopyProperties(m_document.LastParagraph.BreakCharacterFormat);
			}
		}
		else if (m_currentTableType == RtfTableType.None && (!IsDestinationControlWord || (m_bIsShapeText && m_previousTokenKey != "pntxta" && m_previousTokenKey != "pntxtb")) && !m_bIsDocumentInfo && !m_bIsPictureOrShape)
		{
			if (!string.IsNullOrEmpty(m_previousToken) && StartsWithExt(m_previousToken, "'") && m_bIsAccentChar)
			{
				m_bIsAccentChar = false;
			}
			if (m_prevTokenType == RtfTokenType.Text && tr != null)
			{
				if (!isSpecialCharacter)
				{
					m_token = GetEncodedString(m_token);
				}
				else
				{
					isSpecialCharacter = false;
				}
				tr.Text += m_token;
			}
			else
			{
				if (m_bIsList && CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel == null && CurrentPara.ListFormat.CurrentListStyle == null)
				{
					CurrentPara.ListFormat.ContinueListNumbering();
				}
				if (!isSpecialCharacter)
				{
					m_token = GetEncodedString(m_token);
				}
				else
				{
					isSpecialCharacter = false;
				}
				tr = CurrentPara.AppendText(m_token);
				if (tr.CharacterFormat.BaseFormat != null && tr.CharacterFormat.BaseFormat.OwnerBase is WParagraphStyle)
				{
					ResetTextBackgroundColor(tr.CharacterFormat);
				}
				CopyTextFormatToCharFormat(tr.CharacterFormat, m_currTextFormat);
				if (!isPlainTagPresent && m_document.LastParagraph != null && IsDefaultTextFormat(m_currTextFormat))
				{
					if (m_document.LastParagraph.BreakCharacterFormat.HasValue(106))
					{
						m_document.LastParagraph.BreakCharacterFormat.PropertiesHash.Remove(106);
					}
					CurrentPara.BreakCharacterFormat.CopyFormat(m_document.LastParagraph.BreakCharacterFormat);
					tr.CharacterFormat.CopyProperties(m_document.LastParagraph.BreakCharacterFormat);
				}
			}
			m_bIsBookmarkEnd = false;
		}
		else if (IsDestinationControlWord && m_previousTokenKey == "pntxta" && CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel != null)
		{
			CurrentPara.ListFormat.CurrentListLevel.NumberSuffix = m_token;
		}
		else if (IsDestinationControlWord && m_previousTokenKey == "pntxtb" && CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel != null)
		{
			CurrentPara.ListFormat.CurrentListLevel.NumberPrefix = m_token;
		}
		else if (m_currentTableType == RtfTableType.FontTable && !IsDestinationControlWord)
		{
			m_rtfFont.FontName += m_token;
		}
		else if (m_currentTableType == RtfTableType.FontTable && m_previousTokenKey == "falt")
		{
			m_rtfFont.AlternateFontName += m_token;
		}
		else if (m_currentTableType == RtfTableType.StyleSheet)
		{
			if (string.IsNullOrEmpty(m_styleName))
			{
				m_styleName += m_token.Trim();
			}
			else
			{
				m_styleName += m_token;
			}
		}
		else if (m_previousToken == "sn" && m_bIsPictureOrShape && m_bIsShapePicture)
		{
			m_drawingFieldName = m_token;
			if (m_token == "pWrapPolygonVertices")
			{
				isWrapPolygon = true;
			}
			if (m_token == "pibName")
			{
				m_isPibName = true;
			}
			if (m_token == "pibFlags")
			{
				m_isPibFlags = true;
			}
			if (m_token == "dxWrapDistLeft")
			{
				m_isDistFromLeft = true;
			}
		}
		else if (m_previousToken == "sv" && m_drawingFieldName != null && m_bIsPictureOrShape && m_bIsShapePicture)
		{
			if (isWrapPolygon)
			{
				m_drawingFieldValue += m_token;
			}
			else
			{
				m_drawingFieldValue = m_token;
			}
			if (m_isPibName)
			{
				m_externalLink = m_token;
				m_isPibName = false;
			}
			if (m_isPibFlags)
			{
				m_linkType = m_token;
				m_isPibFlags = false;
			}
			if (m_isDistFromLeft)
			{
				m_DistFromLeftVal = m_token;
				m_isDistFromLeft = false;
			}
		}
		else if (m_isImageHyperlink && m_drawingFieldName == "pihlShape" && m_bIsPictureOrShape && m_bIsShapePicture && (m_previousToken == "hlfr" || m_previousToken == "hlsrc"))
		{
			m_href = m_token;
			m_isImageHyperlink = false;
		}
	}

	private bool IsDefaultTextFormat(TextFormat m_currTextFormat)
	{
		TextFormat textFormat = new TextFormat();
		if (m_currTextFormat.AllCaps == textFormat.AllCaps && m_currTextFormat.BackColor == textFormat.BackColor && m_currTextFormat.Bidi == textFormat.Bidi && m_currTextFormat.Bold == textFormat.Bold && m_currTextFormat.CharacterSpacing == textFormat.CharacterSpacing && m_currTextFormat.CharacterStyleName == textFormat.CharacterStyleName && m_currTextFormat.DoubleStrike == textFormat.DoubleStrike && m_currTextFormat.Emboss == textFormat.Emboss && m_currTextFormat.Engrave == textFormat.Engrave && m_currTextFormat.FontColor == textFormat.FontColor && m_currTextFormat.FontFamily == textFormat.FontFamily && m_currTextFormat.FontSize == textFormat.FontSize && m_currTextFormat.ForeColor == textFormat.ForeColor && m_currTextFormat.HighlightColor == textFormat.HighlightColor && m_currTextFormat.Italic == textFormat.Italic && m_currTextFormat.LidBi == textFormat.LidBi && m_currTextFormat.LocalIdASCII == textFormat.LocalIdASCII && m_currTextFormat.LocalIdForEast == textFormat.LocalIdForEast && m_currTextFormat.m_subSuperScript == textFormat.m_subSuperScript && m_currTextFormat.m_underlineStyle == textFormat.m_underlineStyle && m_currTextFormat.Position == textFormat.Position && m_currTextFormat.Scaling == textFormat.Scaling && m_currTextFormat.Shadow == textFormat.Shadow && m_currTextFormat.SmallCaps == textFormat.SmallCaps && m_currTextFormat.SpecVanish == textFormat.SpecVanish && m_currTextFormat.Strike == textFormat.Strike && m_currTextFormat.Style == textFormat.Style && m_currTextFormat.TextAlign == textFormat.TextAlign && m_currTextFormat.Underline == textFormat.Underline)
		{
			return true;
		}
		return false;
	}

	private string GetEncodedString(string m_token)
	{
		m_token.ToCharArray();
		Encoding encoding = GetEncoding();
		string text = null;
		if (encoding.WebName == "gb2312")
		{
			byte[] bytes = GetEncoding("Windows-1252").GetBytes(m_token);
			text = encoding.GetString(bytes, 0, bytes.Length);
		}
		else
		{
			for (int i = 0; i < m_token.Length; i++)
			{
				byte[] bytes2 = BitConverter.GetBytes((int)m_token[i]);
				string @string = encoding.GetString(bytes2, 0, bytes2.Length);
				@string = @string.Replace("\0", "");
				text += @string;
			}
		}
		if (text != null)
		{
			return m_token = text;
		}
		return m_token;
	}

	private Encoding GetEncoding()
	{
		try
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			return Encoding.GetEncoding(GetCodePage());
		}
		catch (Exception)
		{
			return new Windows1252Encoding();
		}
	}

	private Encoding GetEncoding(string codePage)
	{
		try
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			return Encoding.GetEncoding(codePage);
		}
		catch (Exception)
		{
			return new Windows1252Encoding();
		}
	}

	private void ResetTextBackgroundColor(WCharacterFormat sourceFormat)
	{
		if (sourceFormat.TextBackgroundColor != Color.Empty && !sourceFormat.HasKey(9))
		{
			sourceFormat.TextBackgroundColor = Color.Empty;
		}
	}

	private WTextRange GetFieldCodeTextRange(WField field, string fieldCode)
	{
		WTextRange wTextRange = new WTextRange(m_document);
		wTextRange.Text = fieldCode;
		wTextRange.ApplyCharacterFormat(field.CharacterFormat);
		return wTextRange;
	}

	private void ParseFieldGroupData(string token)
	{
		FieldType fieldType = FieldType.FieldUnknown;
		if (m_fieldGroupTypeStack.Count > 0 && m_fieldGroupTypeStack.Peek() == FieldGroupType.FieldInstruction && m_fieldCollectionStack.Count < m_fieldGroupStack.Count)
		{
			fieldType = GetFieldType(token.Trim());
		}
		switch (fieldType)
		{
		case FieldType.FieldFormTextInput:
		{
			WTextFormField field = new WTextFormField(m_document);
			ApplyFieldProperties(field, token, fieldType);
			m_document.SetTriggerElement(ref m_document.m_supportedElementFlag_2, 14);
			return;
		}
		case FieldType.FieldFormDropDown:
		{
			WDropDownFormField field7 = new WDropDownFormField(m_document);
			ApplyFieldProperties(field7, token, fieldType);
			m_document.SetTriggerElement(ref m_document.m_supportedElementFlag_1, 15);
			return;
		}
		case FieldType.FieldFormCheckBox:
		{
			WCheckBox field6 = new WCheckBox(m_document);
			ApplyFieldProperties(field6, token, fieldType);
			m_document.SetTriggerElement(ref m_document.m_supportedElementFlag_1, 9);
			return;
		}
		case FieldType.FieldMergeField:
		{
			WMergeField field5 = new WMergeField(m_document);
			ApplyFieldProperties(field5, token, fieldType);
			return;
		}
		case FieldType.FieldSequence:
		{
			WSeqField field4 = new WSeqField(m_document);
			ApplyFieldProperties(field4, token, fieldType);
			return;
		}
		case FieldType.FieldIf:
		{
			WIfField field3 = new WIfField(m_document);
			ApplyFieldProperties(field3, token, fieldType);
			return;
		}
		case FieldType.FieldUnknown:
			ParseUnknownField(token, fieldType);
			return;
		case FieldType.FieldTOC:
			ParseTOCField(token, fieldType);
			return;
		case FieldType.FieldShape:
		{
			WField field2 = new WField(m_document);
			ApplyFieldProperties(field2, token, fieldType);
			return;
		}
		}
		WField field8 = new WField(m_document);
		ApplyFieldProperties(field8, token, fieldType);
		if (fieldType == FieldType.FieldHyperlink)
		{
			m_document.SetTriggerElement(ref m_document.m_supportedElementFlag_1, 23);
		}
	}

	private void ParseTOCField(string token, FieldType fieldType)
	{
		TableOfContent tableOfContent = new TableOfContent(m_document, token);
		m_fieldCollectionStack.Push(tableOfContent.TOCField);
		m_document.TOC.Add(tableOfContent.TOCField, tableOfContent);
		CurrentPara.ChildEntities.Add(tableOfContent);
		CurrentPara.ChildEntities.Add(GetFieldCodeTextRange(tableOfContent.TOCField, token));
		m_document.SetTriggerElement(ref m_document.m_supportedElementFlag_2, 15);
	}

	private void ParseUnknownField(string token, FieldType fieldType)
	{
		if (!(m_previousToken != "datafield") || m_fieldGroupTypeStack.Count == 0)
		{
			return;
		}
		switch (m_fieldGroupTypeStack.Peek())
		{
		case FieldGroupType.FieldInstruction:
			if (m_fieldCollectionStack.Count < m_fieldGroupStack.Count)
			{
				if (token != null && !(token == string.Empty))
				{
					WField field = new WField(m_document);
					ApplyFieldProperties(field, token, fieldType);
				}
			}
			else if (m_fieldCollectionStack.Count > 0 && !m_bIsGroupShape)
			{
				AppendTextRange(token);
			}
			break;
		case FieldGroupType.FieldResult:
			if (!m_bIsGroupShape)
			{
				AppendTextRange(token);
			}
			break;
		}
	}

	private void AppendTextRange(string token)
	{
		WTextRange wTextRange = new WTextRange(m_document);
		wTextRange.Text = token;
		CurrentPara.ChildEntities.Add(wTextRange);
		m_document.SetTriggerElement(ref m_document.m_supportedElementFlag_2, 12);
		CopyTextFormatToCharFormat(wTextRange.CharacterFormat, m_currTextFormat);
	}

	private void ReplaceWfieldWithWMergeFieldObject()
	{
		WMergeField wMergeField = new WMergeField(m_document);
		string internalFieldCode = m_fieldCollectionStack.Peek().InternalFieldCode;
		wMergeField.FieldType = FieldType.FieldMergeField;
		CopyTextFormatToCharFormat(wMergeField.CharacterFormat, m_currTextFormat);
		if (m_currParagraph.Items.LastItem.EntityType == EntityType.Field && (m_currParagraph.Items.LastItem as WField).FieldType == FieldType.FieldMergeField)
		{
			m_currParagraph.Items.Remove(m_currParagraph.Items.LastItem);
			m_currParagraph.Items.Add(wMergeField);
			wMergeField.FieldCode = internalFieldCode;
			m_fieldCollectionStack.Pop();
			m_fieldCollectionStack.Push(wMergeField);
		}
	}

	private void ApplyFieldProperties(WField field, string token, FieldType fieldType)
	{
		field.FieldType = fieldType;
		CopyTextFormatToCharFormat(field.CharacterFormat, m_currTextFormat);
		CurrentPara.ChildEntities.Add(field);
		CurrentPara.ChildEntities.Add(GetFieldCodeTextRange(field, token));
		m_fieldCollectionStack.Push(field);
	}

	private void ParseFormFieldDestinationWords(string token)
	{
		m_token = m_token.TrimStart();
		string previousToken = m_previousToken;
		if (previousToken == null)
		{
			return;
		}
		switch (previousToken.Length)
		{
		case 9:
			switch (previousToken[2])
			{
			case 'd':
				if (previousToken == "ffdeftext")
				{
					m_currentFormField.DefaultText = token;
				}
				break;
			case 'e':
				if (previousToken == "ffexitmcr")
				{
					m_currentFormField.MacroOnExit = token;
				}
				break;
			}
			break;
		case 10:
			switch (previousToken[2])
			{
			case 'h':
				if (previousToken == "ffhelptext")
				{
					m_currentFormField.HelpText = token;
				}
				break;
			case 's':
				if (previousToken == "ffstattext")
				{
					m_currentFormField.StatusHelpText = token;
				}
				break;
			case 'e':
				if (previousToken == "ffentrymcr")
				{
					m_currentFormField.MarcoOnStart = token;
				}
				break;
			}
			break;
		case 6:
			if (previousToken == "ffname")
			{
				m_currentFormField.Name = token;
			}
			break;
		case 8:
			if (previousToken == "ffformat")
			{
				m_currentFormField.StringFormat = token;
			}
			break;
		case 3:
			if (previousToken == "ffl")
			{
				m_currentFormField.DropDownItems.Add(token);
			}
			break;
		case 4:
		case 5:
		case 7:
			break;
		}
	}

	private void ParseImageBytes()
	{
		if (StartsWithExt(m_previousToken, "blipuid") || StartsWithExt(m_previousToken, "dximageuri"))
		{
			if (m_previousControlString == "}")
			{
				AppendPictureToParagraph(m_token);
			}
		}
		else
		{
			AppendPictureToParagraph(m_token);
		}
	}

	private void ParseCustomDocumentProperties(string m_token)
	{
		string previousToken = m_previousToken;
		if (!(previousToken == "propname"))
		{
			if (!(previousToken == "staticval"))
			{
				return;
			}
			switch (m_currPropertyType)
			{
			case DocGen.CompoundFile.DocIO.PropertyType.Int16:
			case DocGen.CompoundFile.DocIO.PropertyType.Int32:
			case DocGen.CompoundFile.DocIO.PropertyType.Int:
				m_currPropertyValue = Convert.ToInt32(m_token);
				break;
			case DocGen.CompoundFile.DocIO.PropertyType.Double:
				m_currPropertyValue = Convert.ToDouble(m_token);
				break;
			case DocGen.CompoundFile.DocIO.PropertyType.Bool:
				m_currPropertyValue = Convert.ToBoolean(Convert.ToInt32(m_token));
				break;
			case DocGen.CompoundFile.DocIO.PropertyType.DateTime:
				try
				{
					CultureInfo cultureInfo = (CultureInfo)CultureInfo.InvariantCulture.Clone();
					if (cultureInfo.Calendar is GregorianCalendar || cultureInfo.Calendar is EastAsianLunisolarCalendar || cultureInfo.Calendar is JulianCalendar || cultureInfo.Calendar is ChineseLunisolarCalendar)
					{
						cultureInfo.DateTimeFormat.Calendar.TwoDigitYearMax = 2029;
					}
					m_currPropertyValue = Convert.ToDateTime(m_token, cultureInfo);
				}
				catch
				{
					string s = m_token.Replace('-', '/');
					string[] formats = new string[4] { "dd/MM/yyyy", "dd/M/yyyy", "dd/MM/yy", "dd/M/yy" };
					if (DateTime.TryParseExact(s, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
					{
						m_currPropertyValue = result;
					}
				}
				break;
			default:
				m_currPropertyValue = m_token;
				break;
			}
			if (m_currPropertyName != null && m_currPropertyValue != null)
			{
				m_document.CustomDocumentProperties.Add(m_currPropertyName, m_currPropertyValue);
				m_document.CustomDocumentProperties[m_currPropertyName].PropertyType = m_currPropertyType;
			}
			m_currPropertyName = null;
			m_currPropertyValue = null;
		}
		else
		{
			m_currPropertyName += m_token;
		}
	}

	private void ParseBuiltInDocumentProperties(string m_token)
	{
		string previousToken = m_previousToken;
		if (previousToken == null)
		{
			return;
		}
		switch (previousToken.Length)
		{
		case 8:
			switch (previousToken[0])
			{
			case 'c':
				if (previousToken == "category")
				{
					m_document.BuiltinDocumentProperties.Category += m_token;
				}
				break;
			case 'o':
				if (previousToken == "operator")
				{
					m_document.BuiltinDocumentProperties.Author += m_token;
				}
				break;
			case 'k':
				if (previousToken == "keywords")
				{
					m_document.BuiltinDocumentProperties.Keywords += m_token;
				}
				break;
			}
			break;
		case 7:
			switch (previousToken[0])
			{
			case 'd':
				if (previousToken == "doccomm")
				{
					m_document.BuiltinDocumentProperties.Comments += m_token;
				}
				break;
			case 'm':
				if (previousToken == "manager")
				{
					m_document.BuiltinDocumentProperties.Manager += m_token;
				}
				break;
			case 'c':
				if (previousToken == "company")
				{
					m_document.BuiltinDocumentProperties.Company += m_token;
				}
				break;
			case 's':
				if (previousToken == "subject")
				{
					m_document.BuiltinDocumentProperties.Subject += m_token;
				}
				break;
			}
			break;
		case 5:
			if (previousToken == "title")
			{
				m_document.BuiltinDocumentProperties.Title += m_token;
			}
			break;
		case 6:
			break;
		}
	}

	private FieldType GetFieldType(string token)
	{
		string text = ((!token.Contains(" ")) ? token : token[..token.IndexOf(" ")]);
		return FieldTypeDefiner.GetFieldType(text.Trim());
	}

	private string GetFormattingString(string fieldInstruction, string fieldTypeString)
	{
		_ = string.Empty;
		return fieldInstruction.Replace(fieldTypeString, string.Empty).Trim();
	}

	private void ApplyDropDownFormFieldProperties(WDropDownFormField dropDownFormField)
	{
		dropDownFormField.DefaultDropDownValue = m_currentFormField.Ffdefres;
		for (int i = 0; i < m_currentFormField.DropDownItems.Count; i++)
		{
			if (m_currentFormField.DropDownItems[i].Text != null && m_currentFormField.DropDownItems[i].Text != string.Empty)
			{
				dropDownFormField.DropDownItems.Add(m_currentFormField.DropDownItems[i].Text);
			}
		}
	}

	private void ApplyTextFormFieldProperties(WTextFormField textField)
	{
		if (m_currentFormField.DefaultText == null && m_currentFormField.MaxLength < "\u2002\u2002\u2002\u2002\u2002".Length && m_currentFormField.MaxLength != 0)
		{
			textField.DefaultText = string.Empty;
		}
		else
		{
			textField.DefaultText = ((m_currentFormField.DefaultText != null) ? m_currentFormField.DefaultText : "\u2002\u2002\u2002\u2002\u2002");
		}
		if (m_currentFormField.MaxLength > 0)
		{
			textField.MaximumLength = m_currentFormField.MaxLength;
		}
		textField.StringFormat = ((m_currentFormField.StringFormat != null) ? m_currentFormField.StringFormat : string.Empty);
	}

	private void ApplyCheckboxPorperties(WCheckBox checkbox)
	{
		checkbox.SizeType = m_currentFormField.CheckboxSizeType;
		checkbox.SetCheckBoxSizeValue(m_currentFormField.CheckboxSize);
		checkbox.DefaultCheckBoxValue = m_currentFormField.Ffdefres == 1;
		checkbox.Checked = m_currentFormField.Ffres == 1 || (m_currentFormField.Ffres == 25 && checkbox.DefaultCheckBoxValue);
	}

	private void ApplyFormFieldProperties(WFormField formField)
	{
		if (m_currentFormField.Name != null && m_document.Bookmarks[m_currentFormField.Name] == null)
		{
			formField.Name = m_currentFormField.Name;
		}
		formField.Help = m_currentFormField.HelpText;
		formField.StatusBarHelp = m_currentFormField.StatusHelpText;
		formField.MacroOnStart = m_currentFormField.MarcoOnStart;
		formField.MacroOnEnd = m_currentFormField.MacroOnExit;
		formField.Enabled = m_currentFormField.Enabled;
		formField.CalculateOnExit = m_currentFormField.CalculateOnExit;
	}

	private bool IsSupportedPicture()
	{
		if (m_bIsObject || m_bIsGroupShape || m_bIsFallBackImage)
		{
			return false;
		}
		if (m_bIsShape && m_bIsShapePictureAdded)
		{
			return false;
		}
		return true;
	}

	private void AppendPictureToParagraph(string token)
	{
		byte[] imageByteArray = GetImageByteArray(token);
		if (m_bIsBackgroundCollection)
		{
			m_document.Background.ImageBytes = imageByteArray;
			m_document.Background.Type = BackgroundType.Picture;
			return;
		}
		if (IsSupportedPicture())
		{
			m_currPicture = CurrentPara.AppendPicture(imageByteArray);
			m_document.SetTriggerElement(ref m_document.m_supportedElementFlag_1, 31);
			ParsePictureDrawingFields();
			CopyTextFormatToCharFormat(m_currPicture.CharacterFormat, m_currTextFormat);
			ApplyPictureFormatting(m_currPicture, m_picFormat);
			m_bIsShapePictureAdded = true;
			if (!string.IsNullOrEmpty(m_href))
			{
				(m_currPicture as WPicture).Href = m_href;
			}
			if (!string.IsNullOrEmpty(m_externalLink))
			{
				(m_currPicture as WPicture).ExternalLink = m_externalLink;
				if (imageByteArray != null)
				{
					(m_currPicture as WPicture).HasImageRecordReference = true;
				}
			}
			if (!string.IsNullOrEmpty(m_linkType))
			{
				(m_currPicture as WPicture).LinkType = m_linkType;
			}
			if (!string.IsNullOrEmpty(m_DistFromLeftVal))
			{
				(m_currPicture as WPicture).DistanceFromLeft = (float)GetIntValue(m_DistFromLeftVal) / 12700f;
				m_DistFromLeftVal = null;
			}
		}
		else if (!m_bIsGroupShape && m_bIsFallBackImage && (m_currShape != null || m_currTextBox != null))
		{
			WPicture wPicture = new WPicture(m_document);
			wPicture.LoadImage(imageByteArray);
			CopyTextFormatToCharFormat(wPicture.CharacterFormat, m_currTextFormat);
			ApplyPictureFormatting(wPicture, m_picFormat);
			if (!string.IsNullOrEmpty(m_DistFromLeftVal))
			{
				wPicture.DistanceFromLeft = (float)GetIntValue(m_DistFromLeftVal) / 12700f;
				m_DistFromLeftVal = null;
			}
			if (m_currShape != null)
			{
				m_currShape.FallbackPic = wPicture;
			}
			else
			{
				m_currTextBox.IsShape = true;
				m_currTextBox.Shape.FallbackPic = wPicture;
			}
		}
		m_bIsStandardPictureSizeNeedToBePreserved = false;
	}

	private float GetRotationAngle(float rotation)
	{
		if (rotation >= 360f || rotation <= -360f)
		{
			rotation %= 360f;
		}
		if (rotation < 0f)
		{
			rotation = 360f + rotation;
		}
		return rotation;
	}

	private int GetBufferSize(int bufferSize)
	{
		byte[] rtfData = m_rtfReader.RtfData;
		bool flag = false;
		char value = (char)rtfData[m_rtfReader.Position + bufferSize];
		while (Array.IndexOf(m_lexer.m_delimeters, value) != -1)
		{
			flag = true;
			bufferSize--;
			value = (char)rtfData[m_rtfReader.Position + bufferSize];
		}
		if (!flag)
		{
			return bufferSize;
		}
		return bufferSize + 1;
	}

	private byte[] GetImageByteArray(string token)
	{
		if (StartsWithExt(token, "bin") || (StartsWithExt(m_previousToken, "bin") && m_previousToken != "bin"))
		{
			string[] array = SeperateToken(m_token);
			if (StartsWithExt(m_previousToken, "bin"))
			{
				array = SeperateToken(m_previousToken);
			}
			int num = Convert.ToInt32(array[1]);
			byte[] array2 = new byte[num];
			int sourceIndex = ((StartsWithExt(token, "bin") && m_lexer.m_prevChar == ' ') ? m_rtfReader.Position : (m_rtfReader.Position - 1));
			Array.Copy(m_rtfReader.RtfData, sourceIndex, array2, 0, num);
			m_rtfReader.Position += GetBufferSize(num);
			return array2;
		}
		token = token.Replace(ControlChar.CarriegeReturn, "");
		token = token.Replace(ControlChar.LineFeed, "");
		token = token.Replace(" ", "");
		token = token.ToUpper();
		byte[] bytes = m_rtfReader.Encoding.GetBytes(token);
		byte[] array3 = new byte[bytes.Length / 2];
		int num2 = 0;
		int num3 = 0;
		while (num3 < bytes.Length / 2)
		{
			int num4 = ((bytes[num2] > 57) ? (bytes[num2] - 55) : (bytes[num2] - 48));
			int num5 = ((bytes[num2 + 1] > 57) ? (bytes[num2 + 1] - 55) : (bytes[num2 + 1] - 48));
			array3[num3++] = (byte)((num4 << 4) | num5);
			num2 += 2;
		}
		m_lexer.IsImageBytes = false;
		return array3;
	}

	private void ApplyPictureFormatting(IWPicture currPicture, PictureFormat pictureFormat)
	{
		if (pictureFormat.HeightScale <= 0f)
		{
			pictureFormat.HeightScale = 100f;
		}
		if (pictureFormat.WidthScale <= 0f)
		{
			pictureFormat.WidthScale = 100f;
		}
		if (m_bIsStandardPictureSizeNeedToBePreserved)
		{
			if (m_bIsShape && m_currShapeFormat.Size != default(SizeF))
			{
				SizeF size = m_currShapeFormat.Size;
				pictureFormat.Height = size.Height;
				pictureFormat.Width = size.Width;
			}
			else
			{
				if (pictureFormat.Height <= 0f || pictureFormat.Height > 1584f)
				{
					pictureFormat.Height = 216f;
				}
				if (pictureFormat.Width <= 0f || pictureFormat.Width > 1584f)
				{
					pictureFormat.Width = 216f;
				}
			}
		}
		if (!m_bIsShape || m_currShapeFormat.Size == default(SizeF))
		{
			if (pictureFormat.Height > 0f && pictureFormat.Height <= 1584f)
			{
				(currPicture as WPicture).m_size.Height = pictureFormat.Height;
			}
			if (pictureFormat.Width > 0f && pictureFormat.Width <= 1584f)
			{
				(currPicture as WPicture).m_size.Width = pictureFormat.Width;
			}
			if (pictureFormat.HeightScale > 0f)
			{
				(currPicture as WPicture).SetHeightScaleValue(pictureFormat.HeightScale);
			}
			if (pictureFormat.WidthScale > 0f)
			{
				(currPicture as WPicture).SetWidthScaleValue(pictureFormat.WidthScale);
			}
		}
		else
		{
			(currPicture as WPicture).m_size.Height = m_currShapeFormat.Size.Height;
			(currPicture as WPicture).m_size.Width = m_currShapeFormat.Size.Width;
		}
		if (m_bIsShapePicture)
		{
			currPicture.HorizontalOrigin = m_currShapeFormat.m_horizOrgin;
			currPicture.VerticalOrigin = m_currShapeFormat.m_vertOrgin;
			currPicture.HorizontalAlignment = m_currShapeFormat.m_horizAlignment;
			(currPicture as WPicture).SetTextWrappingStyleValue(m_currShapeFormat.m_textWrappingStyle);
			currPicture.TextWrappingType = m_currShapeFormat.m_textWrappingType;
			currPicture.VerticalPosition = m_currShapeFormat.m_vertPosition;
			currPicture.HorizontalPosition = m_currShapeFormat.m_horizPosition;
			currPicture.IsBelowText = m_currShapeFormat.m_isBelowText;
			(currPicture as WPicture).OrderIndex = m_picFormat.Zorder * 1024;
		}
		if (string.IsNullOrEmpty(m_picFormat.Rotation))
		{
			return;
		}
		float rotation = float.Parse(m_picFormat.Rotation) / 65536f;
		WPicture wPicture = m_currPicture as WPicture;
		wPicture.Rotation = GetRotationAngle(rotation);
		if ((wPicture.Rotation > 44f && wPicture.Rotation < 135f) || (wPicture.Rotation > 224f && wPicture.Rotation < 315f))
		{
			float height = wPicture.Height;
			wPicture.Height = wPicture.Width;
			wPicture.Width = height;
			float num = Math.Abs(wPicture.Height - wPicture.Width) / 2f;
			if (wPicture.Height > wPicture.Width)
			{
				wPicture.HorizontalPosition += num;
				wPicture.VerticalPosition -= num;
			}
			if (wPicture.Height < wPicture.Width)
			{
				wPicture.VerticalPosition += num;
				wPicture.HorizontalPosition -= num;
			}
		}
		m_picFormat.Rotation = null;
	}

	private AutoShapeType GetAutoShapeType(string shapeValue)
	{
		return shapeValue switch
		{
			"1" => AutoShapeType.Rectangle, 
			"2" => AutoShapeType.RoundedRectangle, 
			"3" => AutoShapeType.Oval, 
			"4" => AutoShapeType.Diamond, 
			"5" => AutoShapeType.IsoscelesTriangle, 
			"6" => AutoShapeType.RightTriangle, 
			"7" => AutoShapeType.Parallelogram, 
			"8" => AutoShapeType.Trapezoid, 
			"9" => AutoShapeType.Hexagon, 
			"10" => AutoShapeType.Octagon, 
			"11" => AutoShapeType.Cross, 
			"12" => AutoShapeType.Star5Point, 
			"13" => AutoShapeType.RightArrow, 
			"14" => AutoShapeType.RightArrow, 
			"15" => AutoShapeType.Pentagon, 
			"16" => AutoShapeType.Cube, 
			"17" => AutoShapeType.RoundedRectangularCallout, 
			"18" => AutoShapeType.Star16Point, 
			"19" => AutoShapeType.Arc, 
			"20" => AutoShapeType.Line, 
			"21" => AutoShapeType.Plaque, 
			"22" => AutoShapeType.Can, 
			"23" => AutoShapeType.Donut, 
			"32" => AutoShapeType.StraightConnector, 
			"33" => AutoShapeType.BentConnector2, 
			"34" => AutoShapeType.ElbowConnector, 
			"35" => AutoShapeType.BentConnector4, 
			"36" => AutoShapeType.BentConnector5, 
			"37" => AutoShapeType.CurvedConnector2, 
			"38" => AutoShapeType.CurvedConnector, 
			"39" => AutoShapeType.CurvedConnector4, 
			"40" => AutoShapeType.CurvedConnector5, 
			"41" => AutoShapeType.LineCallout1NoBorder, 
			"42" => AutoShapeType.LineCallout2NoBorder, 
			"43" => AutoShapeType.LineCallout3NoBorder, 
			"44" => AutoShapeType.LineCallout1AccentBar, 
			"45" => AutoShapeType.LineCallout2AccentBar, 
			"46" => AutoShapeType.LineCallout3AccentBar, 
			"47" => AutoShapeType.LineCallout1, 
			"48" => AutoShapeType.LineCallout2, 
			"49" => AutoShapeType.LineCallout3, 
			"50" => AutoShapeType.LineCallout1BorderAndAccentBar, 
			"51" => AutoShapeType.LineCallout2BorderAndAccentBar, 
			"52" => AutoShapeType.LineCallout3BorderAndAccentBar, 
			"53" => AutoShapeType.DownRibbon, 
			"54" => AutoShapeType.UpRibbon, 
			"55" => AutoShapeType.Chevron, 
			"56" => AutoShapeType.RegularPentagon, 
			"57" => AutoShapeType.NoSymbol, 
			"58" => AutoShapeType.Star8Point, 
			"59" => AutoShapeType.Star16Point, 
			"60" => AutoShapeType.Star32Point, 
			"61" => AutoShapeType.RectangularCallout, 
			"62" => AutoShapeType.RoundedRectangularCallout, 
			"63" => AutoShapeType.OvalCallout, 
			"64" => AutoShapeType.Wave, 
			"65" => AutoShapeType.FoldedCorner, 
			"66" => AutoShapeType.LeftArrow, 
			"67" => AutoShapeType.DownArrow, 
			"68" => AutoShapeType.UpArrow, 
			"69" => AutoShapeType.LeftRightArrow, 
			"70" => AutoShapeType.UpDownArrow, 
			"71" => AutoShapeType.Explosion1, 
			"72" => AutoShapeType.Explosion2, 
			"73" => AutoShapeType.LightningBolt, 
			"74" => AutoShapeType.Heart, 
			"76" => AutoShapeType.QuadArrow, 
			"77" => AutoShapeType.LeftArrowCallout, 
			"78" => AutoShapeType.RightArrowCallout, 
			"79" => AutoShapeType.UpDownArrowCallout, 
			"80" => AutoShapeType.DownArrowCallout, 
			"81" => AutoShapeType.LeftRightArrowCallout, 
			"82" => AutoShapeType.UpDownArrowCallout, 
			"83" => AutoShapeType.QuadArrowCallout, 
			"84" => AutoShapeType.Bevel, 
			"85" => AutoShapeType.LeftBracket, 
			"86" => AutoShapeType.RightBracket, 
			"87" => AutoShapeType.LeftBrace, 
			"88" => AutoShapeType.RightBrace, 
			"89" => AutoShapeType.LeftUpArrow, 
			"90" => AutoShapeType.BentUpArrow, 
			"91" => AutoShapeType.BentArrow, 
			"93" => AutoShapeType.StripedRightArrow, 
			"94" => AutoShapeType.NotchedRightArrow, 
			"95" => AutoShapeType.BlockArc, 
			"92" => AutoShapeType.Star24Point, 
			"96" => AutoShapeType.SmileyFace, 
			"97" => AutoShapeType.VerticalScroll, 
			"98" => AutoShapeType.HorizontalScroll, 
			"99" => AutoShapeType.CircularArrow, 
			"100" => AutoShapeType.CircularArrow, 
			"101" => AutoShapeType.UTurnArrow, 
			"102" => AutoShapeType.CurvedRightArrow, 
			"103" => AutoShapeType.CurvedLeftArrow, 
			"104" => AutoShapeType.CurvedUpArrow, 
			"105" => AutoShapeType.CurvedDownArrow, 
			"106" => AutoShapeType.CloudCallout, 
			"107" => AutoShapeType.CurvedDownRibbon, 
			"108" => AutoShapeType.CurvedUpRibbon, 
			"109" => AutoShapeType.FlowChartProcess, 
			"110" => AutoShapeType.FlowChartDecision, 
			"111" => AutoShapeType.FlowChartData, 
			"112" => AutoShapeType.FlowChartPredefinedProcess, 
			"113" => AutoShapeType.FlowChartInternalStorage, 
			"114" => AutoShapeType.FlowChartDocument, 
			"115" => AutoShapeType.FlowChartMultiDocument, 
			"116" => AutoShapeType.FlowChartTerminator, 
			"117" => AutoShapeType.FlowChartPreparation, 
			"118" => AutoShapeType.FlowChartManualInput, 
			"119" => AutoShapeType.FlowChartManualOperation, 
			"120" => AutoShapeType.FlowChartConnector, 
			"121" => AutoShapeType.FlowChartCard, 
			"122" => AutoShapeType.FlowChartPunchedTape, 
			"123" => AutoShapeType.FlowChartSummingJunction, 
			"124" => AutoShapeType.FlowChartOr, 
			"125" => AutoShapeType.FlowChartCollate, 
			"126" => AutoShapeType.FlowChartSort, 
			"127" => AutoShapeType.FlowChartExtract, 
			"128" => AutoShapeType.FlowChartMerge, 
			"130" => AutoShapeType.FlowChartStoredData, 
			"131" => AutoShapeType.FlowChartSequentialAccessStorage, 
			"132" => AutoShapeType.FlowChartMagneticDisk, 
			"133" => AutoShapeType.FlowChartDirectAccessStorage, 
			"134" => AutoShapeType.FlowChartDisplay, 
			"135" => AutoShapeType.FlowChartDelay, 
			"176" => AutoShapeType.FlowChartAlternateProcess, 
			"177" => AutoShapeType.FlowChartOffPageConnector, 
			"178" => AutoShapeType.LineCallout1NoBorder, 
			"179" => AutoShapeType.LineCallout1AccentBar, 
			"180" => AutoShapeType.LineCallout1, 
			"181" => AutoShapeType.LineCallout1BorderAndAccentBar, 
			"182" => AutoShapeType.LeftRightUpArrow, 
			"183" => AutoShapeType.Sun, 
			"184" => AutoShapeType.Moon, 
			"185" => AutoShapeType.DoubleBracket, 
			"186" => AutoShapeType.DoubleBrace, 
			"187" => AutoShapeType.Star4Point, 
			"188" => AutoShapeType.DoubleWave, 
			_ => AutoShapeType.Unknown, 
		};
	}

	private void ApplyShapeFormatting(Shape currShape, PictureFormat pictureFormat, ShapeFormat shapeFormat)
	{
		if (pictureFormat.HeightScale <= 0f)
		{
			pictureFormat.HeightScale = 100f;
		}
		if (pictureFormat.WidthScale <= 0f)
		{
			pictureFormat.WidthScale = 100f;
		}
		if (m_bIsStandardPictureSizeNeedToBePreserved)
		{
			if (m_bIsShape && m_currShapeFormat.Size != default(SizeF))
			{
				SizeF size = m_currShapeFormat.Size;
				shapeFormat.m_height = size.Height;
				shapeFormat.m_width = size.Width;
			}
			else
			{
				if (shapeFormat.m_height <= 0f || shapeFormat.m_height > 1584f)
				{
					shapeFormat.m_height = 216f;
				}
				if (shapeFormat.m_width <= 0f || shapeFormat.m_width > 1584f)
				{
					shapeFormat.m_width = 216f;
				}
			}
		}
		if (!m_bIsShape || m_currShapeFormat.Size == default(SizeF))
		{
			if (shapeFormat.m_height > 0f && shapeFormat.m_height <= 1584f)
			{
				currShape.Height = shapeFormat.m_height;
			}
			if (shapeFormat.m_width > 0f && shapeFormat.m_width <= 1584f)
			{
				currShape.Width = shapeFormat.m_width;
			}
			if (pictureFormat.HeightScale > 0f)
			{
				currShape.HeightScale = pictureFormat.HeightScale;
			}
			if (pictureFormat.WidthScale > 0f)
			{
				currShape.WidthScale = pictureFormat.WidthScale;
			}
		}
		else
		{
			if (shapeFormat.m_height > 0f && shapeFormat.m_height <= 1584f)
			{
				currShape.Height = m_currShapeFormat.Size.Height;
			}
			if (shapeFormat.m_width > 0f && shapeFormat.m_width <= 1584f)
			{
				currShape.Width = m_currShapeFormat.Size.Width;
			}
		}
		if (m_bIsShapePicture)
		{
			currShape.LeftEdgeExtent = m_currShapeFormat.m_left;
			currShape.RightEdgeExtent = m_currShapeFormat.m_right;
			currShape.TopEdgeExtent = m_currShapeFormat.m_top;
			currShape.BottomEdgeExtent = m_currShapeFormat.m_bottom;
			currShape.HorizontalOrigin = m_currShapeFormat.m_horizOrgin;
			currShape.VerticalOrigin = m_currShapeFormat.m_vertOrgin;
			currShape.HorizontalAlignment = m_currShapeFormat.m_horizAlignment;
			currShape.WrapFormat.SetTextWrappingStyleValue(m_currShapeFormat.m_textWrappingStyle);
			currShape.WrapFormat.TextWrappingType = m_currShapeFormat.m_textWrappingType;
			currShape.VerticalPosition = m_currShapeFormat.m_vertPosition;
			currShape.HorizontalPosition = m_currShapeFormat.m_horizPosition;
			currShape.IsBelowText = m_currShapeFormat.m_isBelowText;
			currShape.LockAnchor = m_currShapeFormat.m_isLockAnchor;
			currShape.ZOrderPosition = m_currShapeFormat.m_zOrder;
			currShape.ShapeID = m_currShapeFormat.m_uniqueId;
			currShape.IsLineStyleInline = true;
			if (currShape.FillFormat.Color != Color.Empty)
			{
				currShape.IsFillStyleInline = true;
				currShape.FillFormat.Fill = true;
			}
			if (currShape.WrapFormat.TextWrappingStyle == TextWrappingStyle.Square || currShape.WrapFormat.TextWrappingStyle == TextWrappingStyle.Tight)
			{
				currShape.WrapFormat.DistanceLeft = 9f;
				currShape.WrapFormat.DistanceRight = 9f;
			}
		}
	}

	private void ApplyTextBoxFormatting(WTextBox textBox, PictureFormat pictureFormat, ShapeFormat shapeFormat)
	{
		if (pictureFormat.HeightScale <= 0f)
		{
			pictureFormat.HeightScale = 100f;
		}
		if (pictureFormat.WidthScale <= 0f)
		{
			pictureFormat.WidthScale = 100f;
		}
		if (m_bIsStandardPictureSizeNeedToBePreserved)
		{
			if (m_bIsShape && m_currShapeFormat.Size != default(SizeF))
			{
				SizeF size = m_currShapeFormat.Size;
				shapeFormat.m_height = size.Height;
				shapeFormat.m_width = size.Width;
			}
			else
			{
				if (shapeFormat.m_height <= 0f || shapeFormat.m_height > 1584f)
				{
					shapeFormat.m_height = 216f;
				}
				if (shapeFormat.m_width <= 0f || shapeFormat.m_width > 1584f)
				{
					shapeFormat.m_width = 216f;
				}
			}
		}
		if (!m_bIsShape || m_currShapeFormat.Size == default(SizeF))
		{
			if (shapeFormat.m_height > 0f && shapeFormat.m_height <= 1584f)
			{
				textBox.TextBoxFormat.Height = shapeFormat.m_height;
			}
			if (shapeFormat.m_width > 0f && shapeFormat.m_width <= 1584f)
			{
				textBox.TextBoxFormat.Width = shapeFormat.m_width;
			}
		}
		else
		{
			if (shapeFormat.m_height > 0f && shapeFormat.m_height <= 1584f)
			{
				textBox.TextBoxFormat.Height = m_currShapeFormat.Size.Height;
			}
			if (shapeFormat.m_width > 0f && shapeFormat.m_width <= 1584f)
			{
				textBox.TextBoxFormat.Width = m_currShapeFormat.Size.Width;
			}
		}
		if (m_bIsShapePicture)
		{
			textBox.TextBoxFormat.HorizontalOrigin = m_currShapeFormat.m_horizOrgin;
			textBox.TextBoxFormat.VerticalOrigin = m_currShapeFormat.m_vertOrgin;
			textBox.TextBoxFormat.HorizontalAlignment = m_currShapeFormat.m_horizAlignment;
			textBox.TextBoxFormat.SetTextWrappingStyleValue(m_currShapeFormat.m_textWrappingStyle);
			textBox.TextBoxFormat.TextWrappingType = m_currShapeFormat.m_textWrappingType;
			textBox.TextBoxFormat.VerticalPosition = m_currShapeFormat.m_vertPosition;
			textBox.TextBoxFormat.HorizontalPosition = m_currShapeFormat.m_horizPosition;
			textBox.TextBoxFormat.IsBelowText = m_currShapeFormat.m_isBelowText;
			textBox.TextBoxFormat.TextBoxShapeID = m_currShapeFormat.m_uniqueId;
			textBox.TextBoxFormat.OrderIndex = m_currShapeFormat.m_zOrder;
			if (textBox.TextBoxFormat.TextWrappingStyle == TextWrappingStyle.Square || textBox.TextBoxFormat.TextWrappingStyle == TextWrappingStyle.Tight || textBox.TextBoxFormat.TextWrappingStyle == TextWrappingStyle.Through)
			{
				textBox.TextBoxFormat.WrapDistanceLeft = 9f;
				textBox.TextBoxFormat.WrapDistanceRight = 9f;
			}
			if (textBox.Shape != null && (textBox.TextBoxFormat.TextWrappingStyle == TextWrappingStyle.Behind || textBox.Shape.LockAnchor))
			{
				textBox.IsShape = true;
			}
			textBox.TextBoxFormat.InternalMargin.Left = 7.2f;
			textBox.TextBoxFormat.InternalMargin.Right = 7.2f;
			textBox.TextBoxFormat.InternalMargin.Top = 3.6f;
			textBox.TextBoxFormat.InternalMargin.Bottom = 3.6f;
		}
	}

	private void ApplyTextBoxFormatsToShape()
	{
		if (m_currTextBox != null && m_currTextBox.IsShape && m_currTextBox.Shape != null)
		{
			Shape shape = m_currTextBox.Shape;
			if (m_currTextBox.TextBoxFormat.AutoFit)
			{
				shape.TextFrame.ShapeAutoFit = true;
				shape.TextFrame.NoAutoFit = false;
				shape.TextFrame.NormalAutoFit = false;
			}
		}
	}

	private void CopyParagraphFormatting(WParagraphFormat sourceParaFormat, WParagraphFormat destParaFormat)
	{
		destParaFormat.ImportContainer(sourceParaFormat);
		destParaFormat.CopyFormat(sourceParaFormat);
		if (m_bIsLinespacingRule)
		{
			destParaFormat.SetPropertyValue(52, sourceParaFormat.LineSpacing);
			destParaFormat.LineSpacingRule = sourceParaFormat.LineSpacingRule;
		}
	}

	private void ParseControlWords(string token, string tokenKey, string tokenValue, string tokenValue2)
	{
		if (m_currentTableType != RtfTableType.None)
		{
			switch (m_currentTableType)
			{
			case RtfTableType.FontTable:
				ParseFontTable(m_token, tokenKey, tokenValue);
				break;
			case RtfTableType.ColorTable:
				ParseColorTable(m_token, tokenKey, tokenValue);
				break;
			case RtfTableType.StyleSheet:
				ParseFormattingToken(token, tokenKey, tokenValue, tokenValue2);
				break;
			case RtfTableType.ListTable:
			case RtfTableType.ListOverrideTable:
				ParseListTable(m_token, tokenKey, tokenValue, tokenValue2);
				break;
			}
			return;
		}
		switch (tokenKey)
		{
		case "rtf":
			if (m_rtfCollectionStack.Count < 1)
			{
				m_rtfCollectionStack.Push("\\");
			}
			else
			{
				isNested = true;
			}
			break;
		case "fonttbl":
			m_rtfFont = new RtfFont();
			m_currentTableType = RtfTableType.FontTable;
			m_lexer.CurrRtfTableType = RtfTableType.FontTable;
			m_stack.Push("{");
			break;
		case "stylesheet":
			IsStyleSheet = true;
			m_document.HasStyleSheets = true;
			m_currentTableType = RtfTableType.StyleSheet;
			m_lexer.CurrRtfTableType = RtfTableType.StyleSheet;
			m_document.DefCharFormat = new WCharacterFormat(m_document);
			m_stack.Push("{");
			break;
		case "listtable":
			m_currentTableType = RtfTableType.ListTable;
			m_lexer.CurrRtfTableType = RtfTableType.ListTable;
			m_stack.Push("{");
			break;
		case "listoverridetable":
			m_currentTableType = RtfTableType.ListOverrideTable;
			m_lexer.CurrRtfTableType = RtfTableType.ListOverrideTable;
			m_stack.Push("{");
			break;
		case "colortbl":
			m_currentTableType = RtfTableType.ColorTable;
			m_lexer.CurrRtfTableType = RtfTableType.ColorTable;
			m_rtfColorTable = new RtfColor();
			m_stack.Push("{");
			break;
		case "info":
			m_bIsDocumentInfo = true;
			m_stack.Push("{");
			break;
		case "userprops":
			if (!IsNestedGroup())
			{
				m_stack.Push("{");
			}
			m_bIsCustomProperties = true;
			break;
		case "ansicpg":
			if (IsSupportedCodePage(Convert.ToInt32(tokenValue)))
			{
				DefaultCodePage = GetSupportedCodePage(Convert.ToInt32(tokenValue));
			}
			break;
		case "deff":
		case "adeff":
			DefaultFontIndex = Convert.ToInt32(tokenValue);
			break;
		case "htmautsp":
			m_document.DOP.Dop2000.Copts.DontUseHTMLParagraphAutoSpacing = false;
			break;
		case "spltpgpar":
			m_document.Settings.CompatibilityOptions[CompatibilityOption.SplitPgBreakAndParaMark] = false;
			break;
		case "nobrkwrptbl":
			m_document.DOP.Dop2000.Copts.DontBreakWrappedTables = false;
			break;
		case "noextrasprl":
			m_document.DOP.Dop2000.Copts.Copts80.Copts60.NoSpaceRaiseLower = true;
			break;
		case "hyphauto":
			m_document.DOP.AutoHyphen = tokenValue == "1";
			break;
		default:
			ParseFormattingToken(token, tokenKey, tokenValue, tokenValue2);
			break;
		}
	}

	private void SkipGroup()
	{
		Stack<string> stack = new Stack<string>();
		stack.Push("{");
		while (stack.Count > 0)
		{
			m_token = m_lexer.ReadNextToken(m_previousTokenKey, m_bIsLevelText);
			if (m_token == "{")
			{
				stack.Push("{");
			}
			if (m_token == "}")
			{
				stack.Pop();
			}
		}
		m_token = "}";
	}

	private string ParseBulletChar(string token)
	{
		string result = string.Empty;
		string s = token.Replace("'", string.Empty);
		if (token.Length >= 3)
		{
			string text = token.Substring(0, 3);
			switch (text)
			{
			case "'00":
			case "'01":
			case "'02":
			case "'03":
			case "'04":
			case "'05":
			case "'06":
			case "'07":
			case "'08":
				result = token.Replace(text, string.Empty);
				result = (s = result.Replace("'", string.Empty));
				break;
			}
		}
		if (int.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result2))
		{
			result = ((result2 != 149) ? ((char)result2).ToString() : '•'.ToString());
		}
		return result;
	}

	private void ParseListTable(string token, string tokenKey, string tokenValue, string tokenValue2)
	{
		string text = null;
		if (m_bIsLevelText && CurrListLevel.PatternType != ListPatternType.Bullet)
		{
			string text2 = "";
			if (StartsWithExt(m_previousToken, "leveltemplateid") || (m_previousToken == "leveltext" && !StartsWithExt(token, "leveltemplateid")))
			{
				IsLevelTextLengthRead = true;
				CurrListLevel.NumberPrefix = string.Empty;
				CurrListLevel.NumberSuffix = string.Empty;
				if (token.Length > 3)
				{
					CurrListLevel.NumberPrefix = token.Substring(3, token.Length - 3);
				}
			}
			else if (IsLevelTextLengthRead)
			{
				if (m_isFirstPlaceHolderRead)
				{
					if (m_previousToken.Length > 3)
					{
						text2 = m_previousToken.Substring(3, m_previousToken.Length - 3);
					}
					if (StartsWithExt(token, "'01") && StartsWithExt(m_previousToken, "'00"))
					{
						WListLevel currListLevel = CurrListLevel;
						currListLevel.NumberPrefix = currListLevel.NumberPrefix + '\0' + text2;
					}
					else if (StartsWithExt(token, "'02") && StartsWithExt(m_previousToken, "'01"))
					{
						WListLevel currListLevel2 = CurrListLevel;
						currListLevel2.NumberPrefix = currListLevel2.NumberPrefix + '\u0001' + text2;
					}
					else if (StartsWithExt(token, "'03") && StartsWithExt(m_previousToken, "'02"))
					{
						WListLevel currListLevel3 = CurrListLevel;
						currListLevel3.NumberPrefix = currListLevel3.NumberPrefix + '\u0002' + text2;
					}
					else if (StartsWithExt(token, "'04") && StartsWithExt(m_previousToken, "'03"))
					{
						WListLevel currListLevel4 = CurrListLevel;
						currListLevel4.NumberPrefix = currListLevel4.NumberPrefix + '\u0003' + text2;
					}
					else if (StartsWithExt(token, "'05") && StartsWithExt(m_previousToken, "'04"))
					{
						WListLevel currListLevel5 = CurrListLevel;
						currListLevel5.NumberPrefix = currListLevel5.NumberPrefix + '\u0004' + text2;
					}
					else if (StartsWithExt(token, "'06") && StartsWithExt(m_previousToken, "'05"))
					{
						WListLevel currListLevel6 = CurrListLevel;
						currListLevel6.NumberPrefix = currListLevel6.NumberPrefix + '\u0005' + text2;
					}
					else if (StartsWithExt(token, "'07") && StartsWithExt(m_previousToken, "'06"))
					{
						WListLevel currListLevel7 = CurrListLevel;
						currListLevel7.NumberPrefix = currListLevel7.NumberPrefix + '\u0006' + text2;
					}
					else if (StartsWithExt(token, "'08") && StartsWithExt(m_previousToken, "'07"))
					{
						WListLevel currListLevel8 = CurrListLevel;
						currListLevel8.NumberPrefix = currListLevel8.NumberPrefix + '\a' + text2;
					}
				}
				m_isFirstPlaceHolderRead = true;
				if (token.Length > 3)
				{
					CurrListLevel.NumberSuffix = token.Substring(3, token.Length - 3);
				}
				else
				{
					CurrListLevel.NumberSuffix = string.Empty;
				}
			}
		}
		else if (m_bIsLevelText && CurrListLevel != null && CurrListLevel.PatternType == ListPatternType.Bullet)
		{
			string text3 = ParseBulletChar(token);
			if (!string.IsNullOrEmpty(text3))
			{
				CurrListLevel.BulletCharacter = text3;
			}
		}
		switch (tokenKey)
		{
		case "list":
		{
			CurrListStyle = new ListStyle(m_document);
			string text4 = Guid.NewGuid().ToString();
			text = (m_currStyleName = "ListStyle" + text4);
			CurrListStyle.Name = text;
			m_currLevelIndex = -1;
			break;
		}
		case "ls":
			if (m_currentTableType == RtfTableType.ListOverrideTable)
			{
				if (CurrListOverrideStyle != null)
				{
					m_listOverrideTable.Add(token, CurrListOverrideStyle.Name);
				}
				if (m_listTable.ContainsKey(m_currListId))
				{
					m_document.ListStyleNames.Add(token, m_listTable[m_currListId].Name);
				}
			}
			break;
		case "listlevel":
			if (m_currentTableType == RtfTableType.ListTable)
			{
				ParselistLevelStart();
			}
			else if (m_currentTableType == RtfTableType.ListOverrideTable)
			{
				m_isOverrrideListLevel = true;
			}
			break;
		case "lfolevel":
			m_isOverrrideListLevel = false;
			ParselistLevelStart();
			break;
		case "listoverrideformat":
			if (CurrListLevel.OwnerBase is OverrideLevelFormat)
			{
				(CurrListLevel.OwnerBase as OverrideLevelFormat).OverrideFormatting = true;
			}
			break;
		case "levelfollow":
			switch (Convert.ToInt32(tokenValue))
			{
			case 0:
				CurrListLevel.FollowCharacter = FollowCharacterType.Tab;
				break;
			case 1:
				CurrListLevel.FollowCharacter = FollowCharacterType.Space;
				break;
			case 2:
				CurrListLevel.FollowCharacter = FollowCharacterType.Nothing;
				break;
			}
			break;
		case "levelstartat":
			if (m_currentTableType == RtfTableType.ListTable)
			{
				CurrListLevel.StartAt = Convert.ToInt32(tokenValue);
			}
			else if (m_currentTableType == RtfTableType.ListOverrideTable)
			{
				if (m_isOverrrideListLevel)
				{
					CurrListLevel.StartAt = Convert.ToInt32(tokenValue);
				}
				else if (CurrListLevel.OwnerBase is OverrideLevelFormat && (CurrListLevel.OwnerBase as OverrideLevelFormat).OverrideStartAtValue)
				{
					(CurrListLevel.OwnerBase as OverrideLevelFormat).StartAt = Convert.ToInt32(tokenValue);
				}
			}
			break;
		case "levelnfcn":
		case "levelnfc":
		{
			int num = Convert.ToInt32(tokenValue);
			if (num == 23)
			{
				if (CurrListLevel.LevelNumber == 0)
				{
					CurrListStyle.ListType = ListType.Bulleted;
				}
				CurrListLevel.PatternType = ListPatternType.Bullet;
				break;
			}
			if (CurrListLevel.LevelNumber == 0)
			{
				CurrListStyle.ListType = ListType.Numbered;
			}
			switch (num)
			{
			case 0:
				CurrListLevel.PatternType = ListPatternType.Arabic;
				break;
			case 1:
				CurrListLevel.PatternType = ListPatternType.UpRoman;
				break;
			case 2:
				CurrListLevel.PatternType = ListPatternType.LowRoman;
				break;
			case 3:
				CurrListLevel.PatternType = ListPatternType.UpLetter;
				break;
			case 4:
				CurrListLevel.PatternType = ListPatternType.LowLetter;
				break;
			case 5:
				CurrListLevel.PatternType = ListPatternType.Ordinal;
				break;
			case 6:
				CurrListLevel.PatternType = ListPatternType.Number;
				break;
			case 7:
				CurrListLevel.PatternType = ListPatternType.OrdinalText;
				break;
			case 22:
				CurrListLevel.PatternType = ListPatternType.LeadingZero;
				break;
			case 255:
				CurrListLevel.PatternType = ListPatternType.None;
				break;
			default:
				CurrListLevel.PatternType = ListPatternType.Arabic;
				break;
			}
			break;
		}
		case "leveljc":
			switch (Convert.ToInt32(tokenValue))
			{
			case 0:
				CurrListLevel.NumberAlignment = ListNumberAlignment.Left;
				break;
			case 1:
				CurrListLevel.NumberAlignment = ListNumberAlignment.Center;
				break;
			case 2:
				CurrListLevel.NumberAlignment = ListNumberAlignment.Right;
				break;
			}
			break;
		case "levelnorestart":
			switch (Convert.ToInt32(tokenValue))
			{
			case 1:
				CurrListLevel.NoRestartByHigher = true;
				break;
			case 0:
				CurrListLevel.NoRestartByHigher = false;
				break;
			}
			break;
		case "lin-":
		case "li-":
		{
			float num4 = ExtractTwipsValue(tokenValue);
			CurrListLevel.ParagraphFormat.SetPropertyValue(2, 0f - num4);
			CurrListLevel.TextPosition = 0f - num4;
			break;
		}
		case "li":
		case "lin":
		{
			float num4 = ExtractTwipsValue(tokenValue);
			CurrListLevel.ParagraphFormat.SetPropertyValue(2, num4);
			CurrListLevel.TextPosition = num4;
			break;
		}
		case "fi":
		{
			float num3 = ExtractTwipsValue(tokenValue);
			CurrListLevel.ParagraphFormat.SetPropertyValue(5, num3);
			break;
		}
		case "fi-":
		{
			float num2 = ExtractTwipsValue(tokenValue);
			CurrListLevel.ParagraphFormat.SetPropertyValue(5, 0f - num2);
			break;
		}
		case "listid-":
		case "listid":
			if (m_currentTableType == RtfTableType.ListTable)
			{
				m_listTable.Add(token, CurrListStyle);
				ListStyle destListStyle = m_document.AddListStyle(m_listTable[token].ListType, m_listTable[token].Name);
				CopyListStyle(m_listTable[token], destListStyle);
			}
			else if (m_currentTableType == RtfTableType.ListOverrideTable)
			{
				m_currListId = token;
				CurrListOverrideStyle = null;
			}
			break;
		case "listoverridecount":
			m_listoverrideLevelCount = Convert.ToInt32(tokenValue);
			if (m_listoverrideLevelCount > 0)
			{
				text = "LfoStyle_" + Guid.NewGuid();
				CurrListOverrideStyle = new ListOverrideStyle(m_document);
				m_document.ListOverrides.Add(CurrListOverrideStyle);
				CurrListOverrideStyle.Name = text;
				m_currLevelIndex = -1;
			}
			break;
		case "listoverridestartat":
			if (CurrListLevel.OwnerBase is OverrideLevelFormat)
			{
				(CurrListLevel.OwnerBase as OverrideLevelFormat).OverrideStartAtValue = true;
			}
			break;
		case "levelold":
			CurrListLevel.Word6Legacy = tokenValue != "0";
			break;
		case "levelspace":
		{
			float tabSpaceAfter = ExtractTwipsValue(tokenValue);
			if (CurrListLevel.Word6Legacy)
			{
				CurrListLevel.TabSpaceAfter = tabSpaceAfter;
			}
			break;
		}
		case "tx":
			CurrListLevel.TabSpaceAfter = ExtractTwipsValue(tokenValue);
			break;
		case "af":
		case "f":
		{
			foreach (KeyValuePair<string, RtfFont> item in m_fontTable)
			{
				string key = item.Key;
				if (!(SeperateToken(key)[1] == tokenValue))
				{
					continue;
				}
				CurrRtfFont = item.Value;
				if (CurrListStyle.ListType == ListType.Bulleted)
				{
					if (m_previousTokenKey == "hich")
					{
						CurrListLevel.CharacterFormat.FontNameNonFarEast = CurrRtfFont.FontName;
					}
					else if (m_previousTokenKey == "dbch")
					{
						CurrListLevel.CharacterFormat.FontNameFarEast = CurrRtfFont.FontName;
					}
					else
					{
						CurrListLevel.CharacterFormat.FontName = CurrRtfFont.FontName;
					}
				}
				else
				{
					CurrListLevel.CharacterFormat.FontName = CurrRtfFont.FontName;
					CurrListLevel.CharacterFormat.FontNameAscii = CurrRtfFont.FontName;
					CurrListLevel.CharacterFormat.FontNameBidi = CurrRtfFont.FontName;
					CurrListLevel.CharacterFormat.FontNameFarEast = CurrRtfFont.FontName;
					CurrListLevel.CharacterFormat.FontNameNonFarEast = CurrRtfFont.FontName;
				}
			}
			break;
		}
		case "fs":
			if (tokenValue != null)
			{
				CurrListLevel.CharacterFormat.SetPropertyValue(3, float.Parse(tokenValue, CultureInfo.InvariantCulture) / 2f);
			}
			break;
		case "b":
			if (tokenValue != null && Convert.ToInt32(tokenValue) == 0)
			{
				CurrListLevel.CharacterFormat.Bold = false;
				m_currTextFormat.Bold = ThreeState.False;
			}
			else
			{
				CurrListLevel.CharacterFormat.Bold = true;
				m_currTextFormat.Bold = ThreeState.True;
			}
			break;
		case "u-":
		{
			int num = 65536 - Convert.ToInt32(tokenValue);
			CurrListLevel.BulletCharacter = ((char)num).ToString();
			break;
		}
		case "leveltext":
			m_bIsLevelText = true;
			break;
		case "u":
			CurrListLevel.BulletCharacter = ((char)Convert.ToInt32(tokenValue)).ToString();
			break;
		default:
			ParseFormattingToken(token, tokenKey, tokenValue, tokenValue2);
			break;
		}
	}

	private void ParselistLevelStart()
	{
		m_listLevelStack.Push("{");
		m_bIsListLevel = true;
		m_currLevelIndex++;
		if (m_currentTableType == RtfTableType.ListTable)
		{
			CurrListLevel = new WListLevel(CurrListStyle);
			m_currLevelIndex = CurrListStyle.Levels.Add(CurrListLevel);
		}
		else if (m_currentTableType == RtfTableType.ListOverrideTable)
		{
			if (m_listoverrideLevelCount > m_currLevelIndex)
			{
				OverrideLevelFormat overrideLevelFormat = new OverrideLevelFormat(m_document);
				m_currLevelIndex = CurrListOverrideStyle.OverrideLevels.Add(m_currLevelIndex, overrideLevelFormat);
				CurrListLevel = overrideLevelFormat.OverrideListLevel;
			}
			else
			{
				CurrListLevel = new WListLevel(m_document);
			}
		}
		m_bIsLevelText = false;
		IsLevelTextLengthRead = false;
		m_isFirstPlaceHolderRead = false;
		CurrentPara = new WParagraph(m_document);
		m_currTextFormat = new TextFormat();
	}

	private void CopyCharacterFormatting(WCharacterFormat sourceFormat, WCharacterFormat destFormat)
	{
		if (sourceFormat.HasValue(3))
		{
			destFormat.SetPropertyValue(3, sourceFormat.FontSize);
		}
		if (sourceFormat.HasValue(1))
		{
			destFormat.TextColor = sourceFormat.TextColor;
		}
		if (sourceFormat.HasValue(127))
		{
			destFormat.Scaling = sourceFormat.Scaling;
		}
		if (sourceFormat.HasValue(2) && sourceFormat.FontName != "Times New Roman")
		{
			destFormat.FontName = sourceFormat.FontName;
			if (sourceFormat.FontName == "Monotype Corsiva" || sourceFormat.FontName == "Brush Script MT")
			{
				destFormat.Italic = true;
			}
		}
		if (sourceFormat.HasValue(68) && sourceFormat.FontNameAscii != "Times New Roman")
		{
			destFormat.FontNameAscii = sourceFormat.FontNameAscii;
		}
		if (sourceFormat.HasValue(61) && sourceFormat.FontNameBidi != "Times New Roman")
		{
			destFormat.FontNameBidi = sourceFormat.FontNameBidi;
		}
		if (sourceFormat.HasValue(69) && sourceFormat.FontNameFarEast != "Times New Roman")
		{
			destFormat.FontNameFarEast = sourceFormat.FontNameFarEast;
		}
		if (sourceFormat.HasValue(70) && sourceFormat.FontNameNonFarEast != "Times New Roman")
		{
			destFormat.FontNameNonFarEast = sourceFormat.FontNameNonFarEast;
		}
		if (sourceFormat.HasValue(4))
		{
			destFormat.Bold = sourceFormat.Bold;
		}
		if (sourceFormat.HasValue(5))
		{
			destFormat.Italic = sourceFormat.Italic;
		}
		if (sourceFormat.HasValue(7) && sourceFormat.UnderlineStyle != 0)
		{
			destFormat.UnderlineStyle = sourceFormat.UnderlineStyle;
		}
		if (sourceFormat.HasValue(63))
		{
			destFormat.HighlightColor = sourceFormat.HighlightColor;
		}
		if (sourceFormat.HasValue(50))
		{
			destFormat.Shadow = sourceFormat.Shadow;
		}
		if (sourceFormat.HasValue(18))
		{
			destFormat.SetPropertyValue(18, sourceFormat.CharacterSpacing);
		}
		if (sourceFormat.HasValue(14))
		{
			destFormat.DoubleStrike = sourceFormat.DoubleStrike;
		}
		if (sourceFormat.HasValue(51))
		{
			destFormat.Emboss = sourceFormat.Emboss;
		}
		if (sourceFormat.HasValue(52))
		{
			destFormat.Engrave = sourceFormat.Engrave;
		}
		if (sourceFormat.HasValue(10))
		{
			destFormat.SubSuperScript = sourceFormat.SubSuperScript;
		}
		if (sourceFormat.HasValue(9))
		{
			destFormat.TextBackgroundColor = sourceFormat.TextBackgroundColor;
		}
		if (sourceFormat.HasValue(77))
		{
			destFormat.ForeColor = sourceFormat.ForeColor;
		}
		if (sourceFormat.HasValue(54))
		{
			destFormat.AllCaps = sourceFormat.AllCaps;
		}
		if (sourceFormat.Bidi)
		{
			destFormat.Bidi = true;
			destFormat.FontNameBidi = sourceFormat.FontNameBidi;
			destFormat.SetPropertyValue(62, sourceFormat.FontSizeBidi);
		}
		if (sourceFormat.HasValue(59))
		{
			destFormat.BoldBidi = sourceFormat.BoldBidi;
		}
		if (sourceFormat.HasValue(109))
		{
			destFormat.FieldVanish = sourceFormat.FieldVanish;
		}
		if (sourceFormat.HasValue(53))
		{
			destFormat.Hidden = sourceFormat.Hidden;
		}
		if (sourceFormat.HasValue(24))
		{
			destFormat.SpecVanish = sourceFormat.SpecVanish;
		}
		if (sourceFormat.HasValue(55))
		{
			destFormat.SmallCaps = sourceFormat.SmallCaps;
		}
	}

	private void CopyListStyle(ListStyle sourceListStyle, ListStyle destListStyle)
	{
		destListStyle.ListType = sourceListStyle.ListType;
		destListStyle.Name = sourceListStyle.Name;
		for (int i = 0; i < sourceListStyle.Levels.Count; i++)
		{
			destListStyle.Levels[i].ParagraphFormat.SetPropertyValue(2, sourceListStyle.Levels[i].ParagraphFormat.LeftIndent);
			destListStyle.Levels[i].ParagraphFormat.SetPropertyValue(5, sourceListStyle.Levels[i].ParagraphFormat.FirstLineIndent);
			WCharacterFormat characterFormat = destListStyle.Levels[i].CharacterFormat;
			WCharacterFormat characterFormat2 = sourceListStyle.Levels[i].CharacterFormat;
			if (characterFormat2.HasValue(2))
			{
				characterFormat.FontName = characterFormat2.FontName;
			}
			if (characterFormat2.HasValue(68))
			{
				characterFormat.FontNameAscii = characterFormat2.FontNameAscii;
			}
			if (characterFormat2.HasValue(61))
			{
				characterFormat.FontNameBidi = characterFormat2.FontNameBidi;
			}
			if (characterFormat2.HasValue(69))
			{
				characterFormat.FontNameFarEast = characterFormat2.FontNameFarEast;
			}
			if (characterFormat2.HasValue(70))
			{
				characterFormat.FontNameNonFarEast = characterFormat2.FontNameNonFarEast;
			}
			if (characterFormat2.HasValue(3))
			{
				characterFormat.SetPropertyValue(3, characterFormat2.FontSize);
			}
			if (characterFormat2.HasValue(4))
			{
				characterFormat.Bold = characterFormat2.Bold;
			}
			if (sourceListStyle.Levels[i].BulletCharacter != null)
			{
				destListStyle.Levels[i].BulletCharacter = sourceListStyle.Levels[i].BulletCharacter;
			}
			destListStyle.Levels[i].FollowCharacter = sourceListStyle.Levels[i].FollowCharacter;
			if (sourceListStyle.Levels[i].NoLevelText)
			{
				destListStyle.Levels[i].NoLevelText = sourceListStyle.Levels[i].NoLevelText;
			}
			if (sourceListStyle.Levels[i].NoRestartByHigher)
			{
				destListStyle.Levels[i].NoRestartByHigher = sourceListStyle.Levels[i].NoRestartByHigher;
			}
			destListStyle.Levels[i].NumberAlignment = sourceListStyle.Levels[i].NumberAlignment;
			destListStyle.Levels[i].NumberPosition = sourceListStyle.Levels[i].NumberPosition;
			if (sourceListStyle.Levels[i].NumberPrefix != null)
			{
				destListStyle.Levels[i].NumberPrefix = sourceListStyle.Levels[i].NumberPrefix;
			}
			if (sourceListStyle.Levels[i].NumberSuffix != null)
			{
				destListStyle.Levels[i].NumberSuffix = sourceListStyle.Levels[i].NumberSuffix;
			}
			destListStyle.Levels[i].PatternType = sourceListStyle.Levels[i].PatternType;
			destListStyle.Levels[i].StartAt = sourceListStyle.Levels[i].StartAt;
			destListStyle.Levels[i].TabSpaceAfter = sourceListStyle.Levels[i].TabSpaceAfter;
			destListStyle.Levels[i].TextPosition = sourceListStyle.Levels[i].TextPosition;
		}
	}

	private void ParsePageNumberingToken(string token, string tokenKey, string tokenValue)
	{
		switch (tokenKey)
		{
		case "pgnstarts":
			CurrentSection.PageSetup.PageStartingNumber = Convert.ToInt32(tokenValue);
			break;
		case "pgnrestart":
			CurrentSection.PageSetup.RestartPageNumbering = true;
			CurrentSection.PageSetup.PageStartingNumber = 1;
			break;
		case "pgndec":
			CurrentSection.PageSetup.PageNumberStyle = PageNumberStyle.Arabic;
			break;
		case "pgnucrm":
			CurrentSection.PageSetup.PageNumberStyle = PageNumberStyle.RomanUpper;
			break;
		case "pgnlcrm":
			CurrentSection.PageSetup.PageNumberStyle = PageNumberStyle.RomanLower;
			break;
		case "pgnucltr":
			CurrentSection.PageSetup.PageNumberStyle = PageNumberStyle.LetterUpper;
			break;
		case "pgnlcltr":
			CurrentSection.PageSetup.PageNumberStyle = PageNumberStyle.LetterLower;
			break;
		default:
			CurrentSection.PageSetup.PageNumberStyle = PageNumberStyle.Arabic;
			break;
		}
	}

	private void ParseLineNumberingToken(string token, string tokenKey, string tokenValue)
	{
		if (tokenKey == null)
		{
			return;
		}
		switch (tokenKey.Length)
		{
		case 5:
			if (tokenKey == "linex")
			{
				CurrentSection.PageSetup.SetPageSetupProperty("LineNumberingDistanceFromText", Convert.ToSingle(tokenValue));
			}
			break;
		case 10:
			if (tokenKey == "linestarts")
			{
				CurrentSection.PageSetup.SetPageSetupProperty("LineNumberingStartValue", Convert.ToInt32(tokenValue));
			}
			break;
		case 9:
			if (tokenKey == "lineppage")
			{
				CurrentSection.PageSetup.SetPageSetupProperty("LineNumberingMode", LineNumberingMode.RestartPage);
			}
			break;
		case 8:
			if (tokenKey == "linecont")
			{
				CurrentSection.PageSetup.SetPageSetupProperty("LineNumberingMode", LineNumberingMode.Continuous);
			}
			break;
		case 11:
			if (tokenKey == "linerestart")
			{
				CurrentSection.PageSetup.SetPageSetupProperty("LineNumberingMode", LineNumberingMode.RestartSection);
			}
			break;
		case 7:
			if (tokenKey == "linemod")
			{
				CurrentSection.PageSetup.SetPageSetupProperty("LineNumberingStep", Convert.ToInt32(tokenValue));
			}
			break;
		case 4:
			if (tokenKey == "line")
			{
				Break @break = null;
				@break = ((m_currTextFormat == null || m_currTextFormat.m_BreakClear != BreakClearType.All) ? CurrentPara.AppendBreak(BreakType.LineBreak) : CurrentPara.AppendBreak(BreakType.TextWrappingBreak));
				CopyTextFormatToCharFormat(@break.CharacterFormat, m_currTextFormat);
			}
			break;
		case 6:
			break;
		}
	}

	private void ParseFontTable(string token, string tokenKey, string tokenValue)
	{
		switch (tokenKey)
		{
		case "f":
		case "af":
			if (m_rtfFont != null && m_rtfFont.FontID != null && !m_fontTable.ContainsKey(m_rtfFont.FontID))
			{
				AddFontTableEntry();
			}
			m_rtfFont = new RtfFont();
			m_rtfFont.FontID = token;
			m_rtfFont.FontNumber = Convert.ToInt32(tokenValue);
			return;
		case "fcharset":
			if (m_rtfFont != null)
			{
				m_rtfFont.FontCharSet = Convert.ToInt16(tokenValue);
				return;
			}
			break;
		}
		if (StartsWithExt(token, "'"))
		{
			ParseSpecialCharacters(token);
		}
	}

	private void ParseColorTable(string token, string tokenKey, string tokenValue)
	{
		switch (tokenKey)
		{
		case "red":
			m_rtfColorTable.RedN = Convert.ToInt32(tokenValue);
			break;
		case "green":
			m_rtfColorTable.GreenN = Convert.ToInt32(tokenValue);
			break;
		case "blue":
			m_rtfColorTable.BlueN = Convert.ToInt32(tokenValue);
			break;
		}
	}

	private void ResetListFontName(WCharacterFormat listCharFormat)
	{
		listCharFormat.FontName = "Times New Roman";
		listCharFormat.FontNameAscii = "Times New Roman";
		listCharFormat.FontNameBidi = "Times New Roman";
		listCharFormat.FontNameFarEast = "Times New Roman";
		listCharFormat.FontNameNonFarEast = "Times New Roman";
	}

	private bool IsParaInShapeResult(string token)
	{
		if (token != "par")
		{
			return true;
		}
		if (!m_bIsShapeResult)
		{
			return false;
		}
		return true;
	}

	private void ParseFormattingToken(string token, string tokenKey, string tokenValue, string tokenValue2)
	{
		float num = 0f;
		if (m_bIsPictureOrShape && !m_bIsShapePicture)
		{
			ParsePictureToken(token, tokenKey, tokenValue);
		}
		if (m_bIsPictureOrShape && m_bIsShapePicture && IsParaInShapeResult(token))
		{
			if (token.Contains("pic") || StartsWithExt(token, "bin"))
			{
				ParsePictureToken(token, tokenKey, tokenValue);
			}
			else
			{
				ParseShapeToken(token, tokenKey, tokenValue);
			}
			return;
		}
		if (tokenKey == "bin" && !m_bIsPictureOrShape && !m_bIsShapePicture && !string.IsNullOrEmpty(tokenValue))
		{
			m_rtfReader.Position += int.Parse(tokenValue);
			return;
		}
		if (StartsWithExt(token, "line"))
		{
			ParseLineNumberingToken(token, tokenKey, tokenValue);
			return;
		}
		if (StartsWithExt(token, "chpgn"))
		{
			WField wField = m_currParagraph.AppendField("", FieldType.FieldPage) as WField;
			CopyTextFormatToCharFormat(wField.CharacterFormat, m_currTextFormat);
			if (wField.NextSibling is WTextRange)
			{
				(wField.NextSibling as WTextRange).ApplyCharacterFormat(wField.CharacterFormat);
			}
			return;
		}
		if (StartsWithExt(token, "pgn"))
		{
			ParsePageNumberingToken(token, tokenKey, tokenValue);
			return;
		}
		if (StartsWithExt(token, "brdr"))
		{
			ParseParagraphBorders(token, tokenKey, tokenValue);
			return;
		}
		if (StartsWithExt(token, "vert"))
		{
			ParsePageVerticalAlignment(token, tokenKey, tokenValue);
			return;
		}
		if (m_bIsCustomProperties)
		{
			if (StartsWithExt(m_token, "proptype"))
			{
				m_currPropertyType = (DocGen.CompoundFile.DocIO.PropertyType)Convert.ToInt64(tokenValue);
			}
			return;
		}
		if (m_previousTokenKey == "pntxtb" && CurrentPara != null && CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel != null)
		{
			string text = ParseBulletChar(token);
			if (!string.IsNullOrEmpty(text))
			{
				CurrentPara.ListFormat.CurrentListLevel.BulletCharacter = text;
			}
			return;
		}
		switch (tokenKey)
		{
		case "s":
			if (m_currentTableType == RtfTableType.StyleSheet)
			{
				m_currStyleID = m_token;
				m_currStyle = new WParagraphStyle(m_document);
				m_currParagraph = new WParagraph(m_document);
				m_currParagraphFormat = new WParagraphFormat(m_document);
				m_currTextFormat = new TextFormat();
				m_styleName = string.Empty;
				break;
			}
			{
				foreach (KeyValuePair<string, IWParagraphStyle> item in m_styleTable)
				{
					if (item.Key == m_token)
					{
						if (!m_bIsListText)
						{
							ResetParagraphFormat(m_currParagraphFormat);
							ResetCharacterFormat(CurrentPara.BreakCharacterFormat);
						}
						(CurrentPara as WParagraph).ApplyStyle(item.Value.Name, isDomChanges: false);
					}
				}
				break;
			}
		case "cs":
			if (m_currentTableType == RtfTableType.StyleSheet)
			{
				m_currStyleID = m_token;
				m_currCharStyle = new WCharacterStyle(m_document);
				m_currTextFormat = new TextFormat();
				m_styleName = string.Empty;
				break;
			}
			{
				foreach (KeyValuePair<string, WCharacterStyle> item2 in m_charStyleTable)
				{
					if (item2.Key == m_token)
					{
						m_currTextFormat.CharacterStyleName = item2.Value.Name;
					}
				}
				break;
			}
		case "cols":
		{
			float equalColumnWidth = GetEqualColumnWidth(Convert.ToInt32(tokenValue));
			for (int i = 0; i < Convert.ToInt32(tokenValue); i++)
			{
				(CurrentSection as WSection).AddColumn(equalColumnWidth, 36f, isOpening: true);
			}
			if (CurrentSection.Columns.Count > 0)
			{
				CurrColumn = CurrentSection.Columns[0];
			}
			break;
		}
		case "colno":
			if (CurrentSection.Columns.Count >= Convert.ToInt32(tokenValue))
			{
				CurrColumn = CurrentSection.Columns[Convert.ToInt32(tokenValue) - 1];
			}
			break;
		case "colsx":
		case "colsr":
			CurrColumn.Space = ExtractTwipsValue(tokenValue);
			break;
		case "colw":
			CurrentSection.PageSetup.EqualColumnWidth = false;
			CurrColumn.Width = ExtractTwipsValue(tokenValue);
			break;
		case "viewkind":
			switch (Convert.ToInt32(tokenValue))
			{
			case 0:
				m_document.ViewSetup.DocumentViewType = DocumentViewType.None;
				break;
			case 1:
				m_document.ViewSetup.DocumentViewType = DocumentViewType.NormalLayout;
				break;
			case 2:
				m_document.ViewSetup.DocumentViewType = DocumentViewType.OutlineLayout;
				break;
			case 3:
				m_document.ViewSetup.DocumentViewType = DocumentViewType.PrintLayout;
				break;
			case 4:
				m_document.ViewSetup.DocumentViewType = DocumentViewType.WebLayout;
				break;
			case 5:
				m_document.ViewSetup.DocumentViewType = DocumentViewType.OutlineLayout;
				break;
			}
			break;
		case "viewscale":
			m_document.ViewSetup.SetZoomPercentValue(Convert.ToInt32(tokenValue));
			break;
		case "viewzk":
			switch (Convert.ToInt32(tokenValue))
			{
			case 0:
				m_document.ViewSetup.ZoomType = ZoomType.None;
				break;
			case 1:
				m_document.ViewSetup.ZoomType = ZoomType.FullPage;
				break;
			case 2:
				m_document.ViewSetup.ZoomType = ZoomType.PageWidth;
				break;
			case 3:
				m_document.ViewSetup.ZoomType = ZoomType.TextFit;
				break;
			}
			break;
		case "viewbksp":
			m_document.Settings.DisplayBackgrounds = Convert.ToInt32(tokenValue) == 1;
			break;
		case "facingp":
			m_secFormat.DifferentOddAndEvenPage = true;
			break;
		case "lndscpsxn":
			m_secFormat.m_pageOrientation = PageOrientation.Landscape;
			break;
		case "titlepg":
			CurrentSection.PageSetup.DifferentFirstPage = true;
			CurrentSection.HeadersFooters.LinkToPrevious = true;
			break;
		case "header":
		case "headerr":
			m_bIsHeader = true;
			m_headerFooterStack.Push("{");
			m_textBody = CurrentSection.HeadersFooters.OddHeader;
			m_document.SetTriggerElement(ref m_document.m_supportedElementFlag_1, 22);
			m_textBody.Items.Clear();
			if (CurrentPara.ChildEntities.Count > 0)
			{
				inCompleteParagraph = new WParagraph(m_document);
				int num15;
				for (num15 = 0; num15 < CurrentPara.ChildEntities.Count; num15++)
				{
					inCompleteParagraph.ChildEntities.Add(CurrentPara.ChildEntities[num15]);
					num15--;
				}
			}
			break;
		case "headerl":
			m_bIsHeader = true;
			m_headerFooterStack.Push("{");
			m_textBody = CurrentSection.HeadersFooters.EvenHeader;
			m_document.SetTriggerElement(ref m_document.m_supportedElementFlag_1, 22);
			m_textBody.Items.Clear();
			if (CurrentPara.ChildEntities.Count > 0)
			{
				inCompleteParagraph = new WParagraph(m_document);
				int num12;
				for (num12 = 0; num12 < CurrentPara.ChildEntities.Count; num12++)
				{
					inCompleteParagraph.ChildEntities.Add(CurrentPara.ChildEntities[num12]);
					num12--;
				}
			}
			break;
		case "headerf":
			m_bIsHeader = true;
			m_headerFooterStack.Push("{");
			m_textBody = CurrentSection.HeadersFooters.FirstPageHeader;
			m_document.SetTriggerElement(ref m_document.m_supportedElementFlag_1, 22);
			m_textBody.Items.Clear();
			if (CurrentPara.ChildEntities.Count > 0)
			{
				inCompleteParagraph = new WParagraph(m_document);
				int num10;
				for (num10 = 0; num10 < CurrentPara.ChildEntities.Count; num10++)
				{
					inCompleteParagraph.ChildEntities.Add(CurrentPara.ChildEntities[num10]);
					num10--;
				}
			}
			break;
		case "footerl":
			if (m_currTable == null)
			{
				m_bIsFooter = true;
				m_headerFooterStack.Push("{");
				m_textBody = CurrentSection.HeadersFooters.EvenFooter;
				m_document.SetTriggerElement(ref m_document.m_supportedElementFlag_1, 20);
				m_textBody.Items.Clear();
			}
			else
			{
				SkipGroup();
				if (m_token == "}")
				{
					m_tokenType = RtfTokenType.GroupEnd;
					ParseGroupEnd();
				}
			}
			break;
		case "footerf":
			if (m_currTable == null)
			{
				m_bIsFooter = true;
				m_headerFooterStack.Push("{");
				m_textBody = CurrentSection.HeadersFooters.FirstPageFooter;
				m_document.SetTriggerElement(ref m_document.m_supportedElementFlag_1, 20);
				m_textBody.Items.Clear();
			}
			else
			{
				SkipGroup();
				if (m_token == "}")
				{
					m_tokenType = RtfTokenType.GroupEnd;
					ParseGroupEnd();
				}
			}
			break;
		case "footer":
		case "footerr":
			if (m_currTable == null)
			{
				m_bIsFooter = true;
				m_headerFooterStack.Push("{");
				m_textBody = CurrentSection.HeadersFooters.OddFooter;
				m_document.SetTriggerElement(ref m_document.m_supportedElementFlag_1, 20);
				m_textBody.Items.Clear();
			}
			else
			{
				SkipGroup();
				if (m_token == "}")
				{
					m_tokenType = RtfTokenType.GroupEnd;
					ParseGroupEnd();
				}
			}
			break;
		case "atrfstart":
			if (tokenValue != null)
			{
				WCommentMark wCommentMark4 = new WCommentMark(m_document, tokenValue);
				wCommentMark4.Type = CommentMarkType.CommentStart;
				wCommentMark4.CommentId = tokenValue;
				CommentStartIdList.Add(tokenValue);
				if (CurrentPara != null)
				{
					CurrentPara.Items.Add(wCommentMark4);
				}
				m_isCommentRangeStart = true;
				commentstack.Push(wCommentMark4);
			}
			break;
		case "atrfend":
			if (tokenValue == null)
			{
				break;
			}
			if (CurrentPara != null && CommentLinkText != string.Empty && m_textFormatStack.Count > 0)
			{
				m_currTextFormat = m_textFormatStack.Pop();
			}
			CommentLinkText = string.Empty;
			if (CommentStartIdList != null && CommentStartIdList.Contains(tokenValue))
			{
				WCommentMark wCommentMark3 = new WCommentMark(m_document, tokenValue, CommentMarkType.CommentEnd);
				wCommentMark3.CommentId = tokenValue;
				if (CurrentPara != null)
				{
					CurrentPara.Items.Add(wCommentMark3);
				}
				commentstack.Push(wCommentMark3);
			}
			m_isCommentRangeStart = false;
			break;
		case "atnid":
			if (CurrentComment == null)
			{
				CurrentComment = new WComment(m_document);
			}
			if (tokenValue != null)
			{
				if (tokenValue != null)
				{
					CurrentComment.Format.UserInitials = tokenValue;
				}
			}
			break;
		case "atnauthor":
			if (tokenValue != null)
			{
				if (CurrentComment == null)
				{
					CurrentComment = new WComment(m_document);
				}
				if (tokenValue != null)
				{
					CurrentComment.Format.User = tokenValue;
				}
			}
			break;
		case "annotation":
			if (tokenValue != null)
			{
				if (CurrentComment == null)
				{
					CurrentComment = new WComment(m_document);
				}
				CurrentComment.Format.TagBkmk = tokenValue;
				m_textBody.Items.Add(CurrentPara);
				m_document.SetTriggerElement(ref m_document.m_notSupportedElementFlag, 4);
				m_document.SetTriggerElement(ref m_document.m_supportedElementFlag_1, 10);
				if (m_textBody == m_currCell)
				{
					m_isCommentOwnerParaIsCell = true;
				}
				else
				{
					m_isCommentOwnerParaIsCell = false;
				}
				m_textBody = CurrentComment.TextBody;
				CurrentPara = new WParagraph(m_document);
				if (commentstack.Count > 0)
				{
					WCommentMark wCommentMark = commentstack.Peek();
					if (wCommentMark.Type == CommentMarkType.CommentEnd)
					{
						CurrentComment.CommentRangeEnd = wCommentMark;
						wCommentMark.Comment = CurrentComment;
						commentstack.Pop();
					}
				}
				if (commentstack.Count > 0)
				{
					WCommentMark wCommentMark2 = commentstack.Peek();
					if (wCommentMark2.Type == CommentMarkType.CommentStart)
					{
						CurrentComment.CommentRangeStart = wCommentMark2;
						wCommentMark2.Comment = CurrentComment;
						commentstack.Pop();
					}
				}
			}
			m_isCommentReference = true;
			break;
		case "atnref":
			if (tokenValue != null)
			{
				CurrentComment.Format.TagBkmk = tokenValue;
				m_isCommentReference = true;
				CommentLinkText = string.Empty;
			}
			break;
		case "pard":
			isPardTagpresent = true;
			if (!m_bIsListText)
			{
				ParseParagraphStart();
				m_isPnNextList = true;
			}
			break;
		case "phmrg":
			m_currParagraphFormat.FrameHorizontalPos = 1;
			break;
		case "phcol":
			m_currParagraphFormat.FrameHorizontalPos = 0;
			break;
		case "phpg":
			m_currParagraphFormat.FrameHorizontalPos = 2;
			break;
		case "pvmrg":
			m_currParagraphFormat.FrameVerticalPos = 0;
			break;
		case "pvpg":
			m_currParagraphFormat.FrameVerticalPos = 1;
			break;
		case "pvpara":
			m_currParagraphFormat.FrameVerticalPos = 2;
			break;
		case "absw":
			if (tokenValue != null)
			{
				m_currParagraphFormat.FrameWidth = ExtractTwipsValue(tokenValue);
			}
			break;
		case "absh-":
			if (tokenValue != null)
			{
				m_currParagraphFormat.FrameHeight = ExtractTwipsValue(tokenValue);
			}
			break;
		case "absh":
			if (tokenValue != null)
			{
				float num17 = float.Parse(tokenValue, NumberStyles.Number, CultureInfo.InvariantCulture);
				if (num17 != 0f)
				{
					m_currParagraphFormat.FrameHeight = (float)((short)num17 | 0x8000) / 20f;
				}
			}
			break;
		case "posnegx-":
			if (tokenValue != null)
			{
				m_currParagraphFormat.FrameX = SetFramePositions(tokenValue, isNeg: true, isXValue: true);
			}
			break;
		case "posx":
			if (tokenValue != null)
			{
				m_currParagraphFormat.FrameX = SetFramePositions(tokenValue, isNeg: false, isXValue: true);
			}
			break;
		case "posxc":
			m_currParagraphFormat.FrameX = -4f;
			break;
		case "posxi":
			m_currParagraphFormat.FrameX = -12f;
			break;
		case "posxo":
			m_currParagraphFormat.FrameX = -16f;
			break;
		case "posxr":
			m_currParagraphFormat.FrameX = -8f;
			break;
		case "posxl":
			m_currParagraphFormat.FrameX = 0f;
			break;
		case "posnegy-":
			if (tokenValue != null)
			{
				m_currParagraphFormat.FrameY = SetFramePositions(tokenValue, isNeg: true, isXValue: false);
			}
			break;
		case "posy":
			if (tokenValue != null)
			{
				m_currParagraphFormat.FrameY = SetFramePositions(tokenValue, isNeg: false, isXValue: false);
			}
			break;
		case "posyil":
			m_currParagraphFormat.FrameY = 0f;
			break;
		case "posyt":
			m_currParagraphFormat.FrameY = -4f;
			break;
		case "posyc":
			m_currParagraphFormat.FrameY = -8f;
			break;
		case "posyb":
			m_currParagraphFormat.FrameY = -12f;
			break;
		case "posyin":
			m_currParagraphFormat.FrameY = -16f;
			break;
		case "posyout":
			m_currParagraphFormat.FrameY = -20f;
			break;
		case "dfrmtxtx":
			if (tokenValue != null)
			{
				m_currParagraphFormat.FrameHorizontalDistanceFromText = ExtractTwipsValue(tokenValue);
			}
			break;
		case "dfrmtxty":
			if (tokenValue != null)
			{
				m_currParagraphFormat.FrameVerticalDistanceFromText = ExtractTwipsValue(tokenValue);
			}
			break;
		case "nowrap":
			m_currParagraphFormat.WrapFrameAround = FrameWrapMode.None;
			break;
		case "wrapdefault":
			m_currParagraphFormat.WrapFrameAround = FrameWrapMode.Auto;
			break;
		case "wraptight":
			m_currParagraphFormat.WrapFrameAround = FrameWrapMode.Tight;
			break;
		case "wrapthrough":
			m_currParagraphFormat.WrapFrameAround = FrameWrapMode.Through;
			break;
		case "wraparound":
			m_currParagraphFormat.WrapFrameAround = FrameWrapMode.Around;
			break;
		case "plain":
			isPlainTagPresent = true;
			m_currTextFormat = new TextFormat();
			if (!m_bIsListText)
			{
				m_textFormatStack.Push(m_currTextFormat);
			}
			break;
		case "par":
			if (IsFieldGroup && m_currentFieldGroupData != string.Empty)
			{
				ParseFieldGroupData(m_currentFieldGroupData);
				m_currentFieldGroupData = string.Empty;
			}
			if (CurrentPara != null)
			{
				(CurrentPara as WParagraph).HasParRTFTag = true;
			}
			ParseParagraphEnd();
			isPlainTagPresent = false;
			isPardTagpresent = false;
			break;
		case "rtlch":
			m_currTextFormat.Bidi = ThreeState.True;
			break;
		case "ltrch":
			m_currTextFormat.Bidi = ThreeState.False;
			break;
		case "expnd":
		case "expndtw":
			if (tokenValue != null)
			{
				m_currTextFormat.CharacterSpacing = ((tokenKey == "expnd") ? ExtractQuaterPointsValue(tokenValue) : ExtractTwipsValue(tokenValue));
			}
			break;
		case "expnd-":
		case "expndtw-":
			if (tokenValue != null)
			{
				m_currTextFormat.CharacterSpacing = ((tokenKey == "expnd-") ? (0f - ExtractQuaterPointsValue(tokenValue)) : (0f - ExtractTwipsValue(tokenValue)));
			}
			break;
		case "shad":
			if (tokenValue != null && Convert.ToInt32(tokenValue) == 0)
			{
				m_currTextFormat.Shadow = false;
			}
			else
			{
				m_currTextFormat.Shadow = true;
			}
			break;
		case "sa":
		{
			float num20 = 0f;
			if (tokenValue != null)
			{
				num20 = ExtractTwipsValue(tokenValue);
			}
			m_currParagraphFormat.SetPropertyValue(9, num20);
			break;
		}
		case "saauto":
			if (Convert.ToInt32(tokenValue) == 1)
			{
				m_currParagraphFormat.SpaceAfterAuto = true;
			}
			break;
		case "sb":
		{
			float num21 = ExtractTwipsValue(tokenValue);
			m_currParagraphFormat.SetPropertyValue(8, num21);
			break;
		}
		case "sbauto":
			if (Convert.ToInt32(tokenValue) == 1)
			{
				m_currParagraphFormat.SpaceBeforeAuto = true;
			}
			break;
		case "fs":
		{
			float fontSize = 0f;
			if (tokenValue != null)
			{
				fontSize = float.Parse(tokenValue, CultureInfo.InvariantCulture) / 2f;
			}
			m_currTextFormat.FontSize = fontSize;
			if (tokenValue2 != null)
			{
				m_token = tokenValue2;
				ParseDocumentElement(tokenValue2);
			}
			break;
		}
		case "sl":
		{
			m_bIsLinespacingRule = true;
			float num19 = ExtractTwipsValue(tokenValue);
			if (num19 != 0f)
			{
				m_currParagraphFormat.SetPropertyValue(52, num19);
				m_currParagraphFormat.LineSpacingRule = LineSpacingRule.AtLeast;
			}
			else
			{
				m_currParagraphFormat.SetPropertyValue(52, 12f);
				m_currParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
			}
			break;
		}
		case "sl-":
		{
			m_bIsLinespacingRule = true;
			float num19 = ExtractTwipsValue(tokenValue);
			if (num19 != 0f)
			{
				m_currParagraphFormat.SetPropertyValue(52, 0f - num19);
				m_currParagraphFormat.LineSpacingRule = LineSpacingRule.Exactly;
			}
			break;
		}
		case "slmult":
		{
			m_bIsLinespacingRule = true;
			int num18 = Convert.ToInt32(tokenValue);
			if (m_currParagraphFormat.LineSpacing < 0f)
			{
				m_currParagraphFormat.LineSpacingRule = LineSpacingRule.Exactly;
				m_currParagraphFormat.SetPropertyValue(52, 0f - m_currParagraphFormat.LineSpacing);
				break;
			}
			switch (num18)
			{
			case 1:
				m_currParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
				break;
			case 0:
				m_currParagraphFormat.LineSpacingRule = LineSpacingRule.AtLeast;
				break;
			}
			break;
		}
		case "f":
			foreach (KeyValuePair<string, RtfFont> item3 in m_fontTable)
			{
				if (tokenValue2 != null)
				{
					token = tokenKey + tokenValue;
				}
				if (item3.Key == token)
				{
					CurrRtfFont = item3.Value;
					m_currTextFormat.FontFamily = CurrRtfFont.FontName.Trim();
				}
			}
			if (tokenValue2 != null)
			{
				m_token = tokenValue2;
				ParseDocumentElement(tokenValue2);
			}
			break;
		case "cf":
			if (Convert.ToInt64(tokenValue) == 0L)
			{
				m_currTextFormat.FontColor = Color.Black;
			}
			else
			{
				long num16 = Convert.ToInt64(tokenValue);
				CurrColorTable = new RtfColor();
				foreach (KeyValuePair<int, RtfColor> item4 in m_colorTable)
				{
					if (item4.Key == num16)
					{
						CurrColorTable = item4.Value;
					}
				}
				ApplyColorTable(CurrColorTable);
			}
			if (tokenValue2 != null)
			{
				m_token = tokenValue2;
				ParseDocumentElement(tokenValue2);
			}
			break;
		case "u":
		{
			if (m_unicodeCountStack.Count > 0)
			{
				m_unicodeCount = m_unicodeCountStack.Peek();
			}
			string text2 = ((char)Convert.ToInt32(tokenValue)).ToString();
			isSpecialCharacter = true;
			ParseDocumentElement(text2);
			isSpecialCharacter = false;
			break;
		}
		case "bullet":
		{
			string text3 = '•'.ToString();
			tr = CurrentPara.AppendText(text3);
			CopyTextFormatToCharFormat(tr.CharacterFormat, m_currTextFormat);
			break;
		}
		case "u*":
		{
			Encoding encoding = GetEncoding();
			byte[] bytes = BitConverter.GetBytes(Convert.ToInt16(tokenValue));
			string text2 = encoding.GetString(bytes, 0, bytes.Length);
			text2 = text2.Replace("\0", "");
			isSpecialCharacter = true;
			ParseDocumentElement(text2);
			isSpecialCharacter = false;
			break;
		}
		case "u-":
		{
			if (m_unicodeCountStack.Count > 0)
			{
				m_unicodeCount = m_unicodeCountStack.Peek();
			}
			string text2 = ((char)(65536 - Convert.ToInt32(tokenValue))).ToString();
			isSpecialCharacter = true;
			ParseDocumentElement(text2);
			isSpecialCharacter = false;
			break;
		}
		case "uc":
			m_unicodeCountStack.Push(Convert.ToInt32(tokenValue));
			break;
		case "ql":
			m_currParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
			break;
		case "qc":
			m_currParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;
			break;
		case "qr":
			m_currParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;
			break;
		case "qj":
			m_currParagraphFormat.HorizontalAlignment = HorizontalAlignment.Justify;
			break;
		case "qd":
			m_currParagraphFormat.HorizontalAlignment = HorizontalAlignment.Distribute;
			break;
		case "qk":
			switch (Convert.ToByte(tokenValue))
			{
			case 0:
				m_currParagraphFormat.HorizontalAlignment = HorizontalAlignment.JustifyLow;
				break;
			case 10:
				m_currParagraphFormat.HorizontalAlignment = HorizontalAlignment.JustifyMedium;
				break;
			default:
				m_currParagraphFormat.HorizontalAlignment = HorizontalAlignment.JustifyHigh;
				break;
			}
			break;
		case "qt":
			m_currParagraphFormat.HorizontalAlignment = HorizontalAlignment.ThaiJustify;
			break;
		case "tx":
			if (!m_bIsListLevel && !m_bIsListText)
			{
				float num14 = ExtractTwipsValue(tokenValue);
				if ((double)num14 > -1584.05 && (double)num14 < 1584.05)
				{
					CurrTabFormat.TabPosition = num14;
					m_tabCollection.Add(m_tabCollection.Count + 1, m_currTabFormat);
				}
				CurrTabFormat = new TabFormat();
			}
			break;
		case "noline":
			m_currParagraphFormat.SuppressLineNumbers = true;
			break;
		case "tqc":
			if (!m_bIsListText && !m_bIsListLevel)
			{
				CurrTabFormat.TabJustification = TabJustification.Centered;
			}
			break;
		case "tqr":
			if (!m_bIsListText && !m_bIsListLevel)
			{
				CurrTabFormat.TabJustification = TabJustification.Right;
			}
			break;
		case "tqdec":
			if (!m_bIsListText && !m_bIsListLevel)
			{
				CurrTabFormat.TabJustification = TabJustification.Decimal;
			}
			break;
		case "tb":
			if (!m_bIsListText && !m_bIsListLevel)
			{
				CurrTabFormat.TabJustification = TabJustification.Bar;
				CurrTabFormat.TabPosition = ExtractTwipsValue(tokenValue);
			}
			break;
		case "tlmdot":
		case "tldot":
			if (!m_bIsListText && !m_bIsListLevel)
			{
				CurrTabFormat.TabLeader = TabLeader.Dotted;
			}
			break;
		case "tleq":
		case "tlhyph":
			if (!m_bIsListText && !m_bIsListLevel)
			{
				CurrTabFormat.TabLeader = TabLeader.Hyphenated;
			}
			break;
		case "tlul":
			if (!m_bIsListText && !m_bIsListLevel)
			{
				CurrTabFormat.TabLeader = TabLeader.Single;
			}
			break;
		case "tlth":
			if (!m_bIsListText && !m_bIsListLevel)
			{
				CurrTabFormat.TabLeader = TabLeader.Heavy;
			}
			break;
		case "tab":
			if (IsFieldGroup && m_currentFieldGroupData != string.Empty)
			{
				ParseFieldGroupData(m_currentFieldGroupData);
				m_currentFieldGroupData = string.Empty;
			}
			if (m_currentTableType == RtfTableType.StyleSheet || m_bIsListText || m_bIsListLevel)
			{
				break;
			}
			m_tabCount++;
			if (m_tabCollection.Count == 0 || m_tabCount > m_tabCollection.Count)
			{
				tr = CurrentPara.AppendText(ControlChar.Tab);
				CopyTextFormatToCharFormat(tr.CharacterFormat, m_currTextFormat);
				break;
			}
			SortTabCollection();
			{
				foreach (KeyValuePair<int, TabFormat> item5 in m_tabCollection)
				{
					if (item5.Key == m_tabCount)
					{
						CurrentPara.ParagraphFormat.Tabs.AddTab(item5.Value.TabPosition, item5.Value.TabJustification, item5.Value.TabLeader);
						tr = CurrentPara.AppendText(ControlChar.Tab);
						CopyTextFormatToCharFormat(tr.CharacterFormat, m_currTextFormat);
					}
				}
				break;
			}
		case "fi-":
			num = ExtractTwipsValue(tokenValue);
			m_currParagraphFormat.SetPropertyValue(5, 0f - num);
			break;
		case "fi":
			num = ExtractTwipsValue(tokenValue);
			m_currParagraphFormat.SetPropertyValue(5, num);
			break;
		case "li":
		{
			float num7 = ExtractTwipsValue(tokenValue);
			if (m_currParagraphFormat.Bidi)
			{
				m_currParagraphFormat.SetPropertyValue(3, num7);
			}
			else
			{
				m_currParagraphFormat.SetPropertyValue(2, num7);
			}
			break;
		}
		case "li-":
			num = ExtractTwipsValue(tokenValue);
			m_currParagraphFormat.SetPropertyValue(2, 0f - num);
			break;
		case "ri":
		{
			float num6 = ExtractTwipsValue(tokenValue);
			if (m_currParagraphFormat.Bidi)
			{
				m_currParagraphFormat.SetPropertyValue(2, num6);
			}
			else
			{
				m_currParagraphFormat.SetPropertyValue(3, num6);
			}
			break;
		}
		case "ri-":
			num = ExtractTwipsValue(tokenValue);
			m_currParagraphFormat.SetPropertyValue(3, 0f - num);
			break;
		case "indmirror":
			m_currParagraphFormat.MirrorIndents = true;
			break;
		case "hyphpar":
			if (GetIntValue(tokenValue) == 0)
			{
				m_currParagraphFormat.SuppressAutoHyphens = true;
			}
			break;
		case "keep":
			m_currParagraphFormat.Keep = true;
			break;
		case "keepn":
			m_currParagraphFormat.KeepFollow = true;
			break;
		case "outlinelevel":
		case "level":
			ParseOutLineLevel(token, tokenKey, tokenValue);
			break;
		case "pagebb":
			m_currParagraphFormat.PageBreakBefore = true;
			break;
		case "contextualspace":
			m_currParagraphFormat.ContextualSpacing = true;
			break;
		case "widctlpar":
			m_currParagraphFormat.WidowControl = true;
			break;
		case "nowidctlpar":
			m_currParagraphFormat.WidowControl = false;
			break;
		case "nowwrap":
			m_currParagraphFormat.WordWrap = false;
			break;
		case "page":
			CurrentPara.AppendBreak(BreakType.PageBreak);
			break;
		case "column":
			CurrentPara.AppendBreak(BreakType.ColumnBreak);
			break;
		case "shading":
			m_currParagraphFormat.TextureStyle = GetTextureStyle(Convert.ToInt32(tokenValue));
			break;
		case "bgcross":
			m_currParagraphFormat.TextureStyle = TextureStyle.TextureCross;
			break;
		case "bgdkcross":
			m_currParagraphFormat.TextureStyle = TextureStyle.TextureDarkCross;
			break;
		case "bgdkdcross":
			m_currParagraphFormat.TextureStyle = TextureStyle.TextureDarkDiagonalCross;
			break;
		case "bgdkbdiag":
			m_currParagraphFormat.TextureStyle = TextureStyle.TextureDarkDiagonalDown;
			break;
		case "bgdkfdiag":
			m_currParagraphFormat.TextureStyle = TextureStyle.TextureDarkDiagonalUp;
			break;
		case "bgdkhoriz":
			m_currParagraphFormat.TextureStyle = TextureStyle.TextureDarkHorizontal;
			break;
		case "bgdkvert":
			m_currParagraphFormat.TextureStyle = TextureStyle.TextureDarkVertical;
			break;
		case "bgdcross":
			m_currParagraphFormat.TextureStyle = TextureStyle.TextureDiagonalCross;
			break;
		case "bgbdiag":
			m_currParagraphFormat.TextureStyle = TextureStyle.TextureDiagonalDown;
			break;
		case "bgfdiag":
			m_currParagraphFormat.TextureStyle = TextureStyle.TextureDiagonalUp;
			break;
		case "bghoriz":
			m_currParagraphFormat.TextureStyle = TextureStyle.TextureHorizontal;
			break;
		case "bgvert":
			m_currParagraphFormat.TextureStyle = TextureStyle.TextureVertical;
			break;
		case "caps":
			if (tokenValue != null && Convert.ToInt32(tokenValue) == 0)
			{
				m_currTextFormat.AllCaps = ThreeState.False;
			}
			else
			{
				m_currTextFormat.AllCaps = ThreeState.True;
			}
			break;
		case "scaps":
			if (tokenValue != null && Convert.ToInt32(tokenValue) == 0)
			{
				m_currTextFormat.SmallCaps = ThreeState.False;
			}
			else
			{
				m_currTextFormat.SmallCaps = ThreeState.True;
			}
			break;
		case "charscalex":
			if (tokenValue != null)
			{
				m_currTextFormat.Scaling = Convert.ToInt32(tokenValue);
			}
			break;
		case "b":
			if (tokenValue != null && Convert.ToInt32(tokenValue) == 0)
			{
				m_currTextFormat.Bold = ThreeState.False;
			}
			else if (m_currTextFormat.complexScript == ThreeState.True)
			{
				m_currTextFormat.boldBidi = ThreeState.True;
			}
			else
			{
				m_currTextFormat.Bold = ThreeState.True;
			}
			break;
		case "i":
			if (tokenValue != null && Convert.ToInt32(tokenValue) == 0)
			{
				m_currTextFormat.Italic = ThreeState.False;
			}
			else if (m_currTextFormat.complexScript == ThreeState.True)
			{
				m_currTextFormat.italicBidi = ThreeState.True;
			}
			else
			{
				m_currTextFormat.Italic = ThreeState.True;
			}
			break;
		case "ul":
			if (tokenValue != null && Convert.ToInt32(tokenValue) == 0)
			{
				m_currTextFormat.Underline = ThreeState.False;
				m_currTextFormat.m_underlineStyle = UnderlineStyle.None;
			}
			else
			{
				m_currTextFormat.Underline = ThreeState.True;
				m_currTextFormat.m_underlineStyle = UnderlineStyle.Single;
			}
			break;
		case "ulnone":
			m_currTextFormat.m_underlineStyle = UnderlineStyle.None;
			m_currTextFormat.Underline = ThreeState.False;
			break;
		case "uldb":
			if (tokenValue != null && Convert.ToInt32(tokenValue) == 0)
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.None;
			}
			else
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.Double;
			}
			break;
		case "uld":
			if (tokenValue != null && Convert.ToInt32(tokenValue) == 0)
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.None;
			}
			else
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.Dotted;
			}
			break;
		case "uldash":
			if (tokenValue != null && Convert.ToInt32(tokenValue) == 0)
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.None;
			}
			else
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.Dash;
			}
			break;
		case "uldashd":
			if (tokenValue != null && Convert.ToInt32(tokenValue) == 0)
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.None;
			}
			else
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.DotDash;
			}
			break;
		case "uldashdd":
			if (tokenValue != null && Convert.ToInt32(tokenValue) == 0)
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.None;
			}
			else
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.DotDotDash;
			}
			break;
		case "ulwave":
			if (tokenValue != null && Convert.ToInt32(tokenValue) == 0)
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.None;
			}
			else
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.Wavy;
			}
			break;
		case "ulhwave":
			if (tokenValue != null && Convert.ToInt32(tokenValue) == 0)
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.None;
			}
			else
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.WavyHeavy;
			}
			break;
		case "ulldash":
			if (tokenValue != null && Convert.ToInt32(tokenValue) == 0)
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.None;
			}
			else
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.DashLong;
			}
			break;
		case "ulth":
			if (tokenValue != null && Convert.ToInt32(tokenValue) == 0)
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.None;
			}
			else
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.Thick;
			}
			break;
		case "ulthd":
			if (tokenValue != null && Convert.ToInt32(tokenValue) == 0)
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.None;
			}
			else
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.DottedHeavy;
			}
			break;
		case "ululdbwave":
			if (tokenValue != null && Convert.ToInt32(tokenValue) == 0)
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.None;
			}
			else
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.WavyDouble;
			}
			break;
		case "ulw":
			if (tokenValue != null && Convert.ToInt32(tokenValue) == 0)
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.None;
			}
			else
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.Words;
			}
			break;
		case "ulthldash":
			if (tokenValue != null && Convert.ToInt32(tokenValue) == 0)
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.None;
			}
			else
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.DashLongHeavy;
			}
			break;
		case "ulthdashdd":
			if (tokenValue != null && Convert.ToInt32(tokenValue) == 0)
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.None;
			}
			else
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.DotDotDashHeavy;
			}
			break;
		case "ulthdashd":
			if (tokenValue != null && Convert.ToInt32(tokenValue) == 0)
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.None;
			}
			else
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.DotDashHeavy;
			}
			break;
		case "ulthdash":
			if (tokenValue != null && Convert.ToInt32(tokenValue) == 0)
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.None;
			}
			else
			{
				m_currTextFormat.m_underlineStyle = UnderlineStyle.DashHeavy;
			}
			break;
		case "sub":
			m_currTextFormat.m_subSuperScript = SubSuperScript.SubScript;
			break;
		case "super":
			m_currTextFormat.m_subSuperScript = SubSuperScript.SuperScript;
			break;
		case "nosupersub":
			m_currTextFormat.m_subSuperScript = SubSuperScript.None;
			break;
		case "up":
			if (tokenValue != null)
			{
				m_currTextFormat.Position = (float)Convert.ToInt32(tokenValue) / 2f;
			}
			else
			{
				m_currTextFormat.Position = 3f;
			}
			break;
		case "lang":
			if (tokenValue != null)
			{
				m_currTextFormat.LocalIdASCII = Convert.ToInt16(tokenValue);
			}
			break;
		case "langfenp":
			if (tokenValue != null)
			{
				m_currTextFormat.LocalIdForEast = Convert.ToInt16(tokenValue);
			}
			break;
		case "langfe":
			if (tokenValue != null)
			{
				m_currTextFormat.LidBi = Convert.ToInt16(tokenValue);
			}
			break;
		case "dn":
			if (tokenValue != null)
			{
				m_currTextFormat.Position = 0f - (float)Convert.ToInt32(tokenValue) / 2f;
			}
			else
			{
				m_currTextFormat.Position = -3f;
			}
			break;
		case "strike":
			if (tokenValue != null && Convert.ToInt32(tokenValue) == 0)
			{
				m_currTextFormat.Strike = ThreeState.False;
			}
			else
			{
				m_currTextFormat.Strike = ThreeState.True;
			}
			break;
		case "striked":
			switch (Convert.ToInt32(tokenValue))
			{
			case 1:
				m_currTextFormat.DoubleStrike = ThreeState.True;
				break;
			case 0:
				m_currTextFormat.DoubleStrike = ThreeState.False;
				break;
			}
			break;
		case "embo":
			if (tokenValue != null && Convert.ToInt32(tokenValue) == 0)
			{
				m_currTextFormat.Emboss = ThreeState.False;
			}
			else
			{
				m_currTextFormat.Emboss = ThreeState.True;
			}
			break;
		case "impr":
			if (tokenValue != null && Convert.ToInt32(tokenValue) == 0)
			{
				m_currTextFormat.Engrave = ThreeState.False;
			}
			else
			{
				m_currTextFormat.Engrave = ThreeState.True;
			}
			break;
		case "sectd":
			ParseSectionStart();
			IsSectionBreak = true;
			break;
		case "sbknone":
			CurrentSection.BreakCode = SectionBreakCode.NoBreak;
			IsSectionBreak = true;
			break;
		case "sbkcol":
			CurrentSection.BreakCode = SectionBreakCode.NewColumn;
			IsSectionBreak = true;
			break;
		case "sbkpage":
			CurrentSection.BreakCode = SectionBreakCode.NewPage;
			IsSectionBreak = true;
			break;
		case "sbkeven":
			CurrentSection.BreakCode = SectionBreakCode.EvenPage;
			IsSectionBreak = true;
			break;
		case "sbkodd":
			CurrentSection.BreakCode = SectionBreakCode.Oddpage;
			IsSectionBreak = true;
			break;
		case "sect":
			if (m_bIsShapeText)
			{
				CurrentPara.AppendBreak(BreakType.PageBreak);
				break;
			}
			ResetParagraphFormat(m_currParagraphFormat);
			ResetCharacterFormat(CurrentPara.BreakCharacterFormat);
			CopyParagraphFormatting(m_currParagraphFormat, CurrentPara.ParagraphFormat);
			ProcessTableInfo(isShapeTextEnd: false);
			AddNewParagraph(CurrentPara);
			AddNewSection(CurrentSection);
			ApplySectionFormatting();
			m_currSection = new WSection(m_document);
			m_textBody = m_currSection.Body;
			if (m_paragraphFormatStack.Count > 0)
			{
				m_paragraphFormatStack.Clear();
				m_currParagraphFormat = new WParagraphFormat(m_document);
				m_paragraphFormatStack.Push(m_currParagraphFormat);
			}
			isPlainTagPresent = false;
			isPardTagpresent = false;
			break;
		case "binfsxn":
			if (tokenValue != null)
			{
				m_secFormat.FirstPageTray = (int)Math.Floor(float.Parse(tokenValue, CultureInfo.InvariantCulture));
			}
			break;
		case "binsxn":
			if (tokenValue != null)
			{
				m_secFormat.OtherPagesTray = (int)Math.Floor(float.Parse(tokenValue, CultureInfo.InvariantCulture));
			}
			break;
		case "marglsxn":
		case "margl":
			m_secFormat.LeftMargin = ExtractTwipsValue(tokenValue);
			break;
		case "margtsxn":
		case "margt":
			m_secFormat.TopMargin = ExtractTwipsValue(tokenValue);
			break;
		case "margrsxn":
		case "margr":
			m_secFormat.RightMargin = ExtractTwipsValue(tokenValue);
			break;
		case "margbsxn":
		case "margb":
			m_secFormat.BottomMargin = ExtractTwipsValue(tokenValue);
			break;
		case "gutter":
			CurrentSection.PageSetup.Margins.Gutter = ExtractTwipsValue(tokenValue);
			break;
		case "gutterprl":
			m_document.DOP.GutterAtTop = true;
			break;
		case "margmirror":
			m_document.MultiplePage = MultiplePage.MirrorMargins;
			break;
		case "bookfold":
			m_document.MultiplePage = MultiplePage.BookFold;
			break;
		case "bookfoldrev":
			m_document.MultiplePage = MultiplePage.ReverseBookFold;
			break;
		case "twoonone":
			m_document.MultiplePage = MultiplePage.TwoPagesPerSheet;
			break;
		case "bookfoldsheets":
			m_document.SheetsPerBooklet = (int)ExtractTwipsValue(tokenValue);
			break;
		case "pgwsxn":
		case "paperw":
			m_secFormat.pageSize.Width = ExtractTwipsValue(tokenValue);
			break;
		case "pghsxn":
		case "paperh":
			m_secFormat.pageSize.Height = ExtractTwipsValue(tokenValue);
			break;
		case "headery":
			m_secFormat.HeaderDistance = ExtractTwipsValue(tokenValue);
			break;
		case "footery":
			m_secFormat.FooterDistance = ExtractTwipsValue(tokenValue);
			break;
		case "deftab":
			m_secFormat.DefaultTabWidth = ExtractTwipsValue(tokenValue);
			break;
		case "lquote":
		{
			char c4 = '‘';
			if (m_prevTokenType == RtfTokenType.Text && tr != null)
			{
				tr.Text += c4;
				break;
			}
			tr = CurrentPara.AppendText(c4.ToString());
			CopyTextFormatToCharFormat(tr.CharacterFormat, m_currTextFormat);
			break;
		}
		case "rquote":
		{
			char c3 = '’';
			if (m_prevTokenType == RtfTokenType.Text && tr != null && !IsFieldGroup)
			{
				tr.Text += c3;
				break;
			}
			if (IsFieldGroup)
			{
				m_currentFieldGroupData += c3;
				break;
			}
			tr = CurrentPara.AppendText(c3.ToString());
			CopyTextFormatToCharFormat(tr.CharacterFormat, m_currTextFormat);
			break;
		}
		case "rdblquote":
		case "ldblquote":
			tr = CurrentPara.AppendText("\"");
			CopyTextFormatToCharFormat(tr.CharacterFormat, m_currTextFormat);
			break;
		case "endash":
		{
			char c2 = '–';
			if (m_prevTokenType == RtfTokenType.Text && tr != null)
			{
				tr.Text += c2;
				break;
			}
			tr = CurrentPara.AppendText(c2.ToString());
			CopyTextFormatToCharFormat(tr.CharacterFormat, m_currTextFormat);
			break;
		}
		case "emdash":
		{
			char c = '—';
			if (m_prevTokenType == RtfTokenType.Text && tr != null)
			{
				tr.Text += c;
				break;
			}
			tr = CurrentPara.AppendText(c.ToString());
			CopyTextFormatToCharFormat(tr.CharacterFormat, m_currTextFormat);
			break;
		}
		case "chcbpat":
		{
			int num5 = Convert.ToInt32(tokenValue);
			CurrColorTable = new RtfColor();
			foreach (KeyValuePair<int, RtfColor> item6 in m_colorTable)
			{
				if (item6.Key == num5)
				{
					CurrColorTable = item6.Value;
					m_currTextFormat.BackColor = Color.FromArgb(CurrColorTable.RedN, CurrColorTable.GreenN, CurrColorTable.BlueN);
				}
			}
			if (tokenValue2 != null)
			{
				m_token = tokenValue2;
				ParseDocumentElement(tokenValue2);
			}
			break;
		}
		case "chcfpat":
		{
			int num4 = Convert.ToInt32(tokenValue);
			CurrColorTable = new RtfColor();
			foreach (KeyValuePair<int, RtfColor> item7 in m_colorTable)
			{
				if (item7.Key == num4)
				{
					CurrColorTable = item7.Value;
					m_currTextFormat.ForeColor = Color.FromArgb(CurrColorTable.RedN, CurrColorTable.GreenN, CurrColorTable.BlueN);
				}
			}
			if (tokenValue2 != null)
			{
				m_token = tokenValue2;
				ParseDocumentElement(tokenValue2);
			}
			break;
		}
		case "cbpat":
		{
			int num3 = Convert.ToInt32(tokenValue);
			CurrColorTable = new RtfColor();
			{
				foreach (KeyValuePair<int, RtfColor> item8 in m_colorTable)
				{
					if (item8.Key == num3)
					{
						CurrColorTable = item8.Value;
						m_currParagraphFormat.BackColor = Color.FromArgb(CurrColorTable.RedN, CurrColorTable.GreenN, CurrColorTable.BlueN);
					}
				}
				break;
			}
		}
		case "cfpat":
		{
			int num2 = Convert.ToInt32(tokenValue);
			CurrColorTable = new RtfColor();
			{
				foreach (KeyValuePair<int, RtfColor> item9 in m_colorTable)
				{
					if (item9.Key == num2)
					{
						CurrColorTable = item9.Value;
						m_currParagraphFormat.ForeColor = Color.FromArgb(CurrColorTable.RedN, CurrColorTable.GreenN, CurrColorTable.BlueN);
					}
				}
				break;
			}
		}
		case "highlight":
		{
			int key = Convert.ToInt32(tokenValue);
			CurrColorTable = new RtfColor();
			if (m_colorTable.ContainsKey(key))
			{
				CurrColorTable = m_colorTable[key];
				m_currTextFormat.HighlightColor = Color.FromArgb(CurrColorTable.RedN, CurrColorTable.GreenN, CurrColorTable.BlueN);
				if (m_currTextFormat.HighlightColor.ToArgb() == Color.Green.ToArgb())
				{
					m_currTextFormat.HighlightColor = Color.DarkGreen;
				}
			}
			else
			{
				m_currTextFormat.HighlightColor = Color.Empty;
			}
			break;
		}
		case "nonshppict":
			m_bIsShapePicture = false;
			m_bIsShape = false;
			if (!m_bIsPictureOrShape)
			{
				m_pictureOrShapeStack.Push("{");
				m_bIsPictureOrShape = true;
				m_bIsShapePictureAdded = false;
				m_currPicture = null;
				m_picFormat = new PictureFormat();
			}
			break;
		case "shppict":
		{
			m_bIsShapePicture = true;
			m_currShapeFormat = new ShapeFormat();
			bool flag = false;
			if (!m_bIsPictureOrShape)
			{
				if (m_pictureOrShapeStack.Count > 0)
				{
					AddOwnerShapeTextStack();
					flag = true;
				}
				m_pictureOrShapeStack.Push("{");
				m_bIsPictureOrShape = true;
				m_bIsShapePictureAdded = false;
				m_currPicture = null;
				m_picFormat = new PictureFormat();
				if ((m_currShape != null || m_currTextBox != null) && !flag && !m_bIsFallBackImage)
				{
					m_currShape = null;
					m_currTextBox = null;
				}
			}
			break;
		}
		case "shp":
			m_pictureOrShapeStack.Push("{");
			m_bIsShape = true;
			m_bIsShapePictureAdded = false;
			m_bIsShapePicture = true;
			m_currShapeFormat = new ShapeFormat();
			m_currPicture = null;
			m_bIsPictureOrShape = true;
			m_picFormat = new PictureFormat();
			m_currShape = null;
			m_currTextBox = null;
			break;
		case "pict":
			if (!m_bIsPictureOrShape)
			{
				m_bIsShapePicture = true;
				m_bIsShape = false;
				m_currShapeFormat = new ShapeFormat();
				m_currPicture = null;
				m_pictureOrShapeStack.Push("{");
				m_bIsPictureOrShape = true;
				m_picFormat = new PictureFormat();
				m_bIsFallBackImage = false;
			}
			break;
		case "shpgrp":
			m_bIsGroupShape = true;
			m_groupShapeStack.Push("{");
			break;
		case "object":
			m_bIsObject = true;
			m_objectStack.Push("{");
			break;
		case "bkmkstart":
			m_bIsBookmarkStart = true;
			break;
		case "bkmkend":
			m_bIsBookmarkEnd = true;
			break;
		case "pntext":
		case "listtext":
			ParseListTextStart();
			break;
		case "ls":
			if (!IsDestinationControlWord || (m_destStack.Count == 2 && m_bIsShapeText && m_shapeTextStack.Count > 0))
			{
				ApplyListFormatting(token, tokenKey, tokenValue, CurrentPara.ListFormat);
				if (m_listOverrideTable.ContainsKey(token))
				{
					CurrentPara.ListFormat.LFOStyleName = m_listOverrideTable[token];
				}
				if (m_currentTableType == RtfTableType.None)
				{
					m_bIsList = true;
				}
			}
			m_bIsContinousList = false;
			break;
		case "ltrpar":
			m_currParagraphFormat.Bidi = false;
			break;
		case "rtlpar":
			m_currParagraphFormat.Bidi = true;
			break;
		case "lin":
			m_currParagraphFormat.SetPropertyValue(2, ExtractTwipsValue(tokenValue));
			break;
		case "rin":
			m_currParagraphFormat.SetPropertyValue(3, ExtractTwipsValue(tokenValue));
			break;
		case "ilvl":
			if (!IsDestinationControlWord && (Convert.ToInt32(tokenValue) < 9 || Convert.ToInt32(tokenValue) > 12))
			{
				CurrentPara.ListFormat.ListLevelNumber = Convert.ToInt32(tokenValue);
			}
			break;
		case "pn":
			m_pnLevelNumber = 0;
			break;
		case "pnlvlblt":
			if (m_currentTableType == RtfTableType.None)
			{
				m_bIsList = true;
			}
			CurrentPara.ListFormat.ApplyDefBulletStyle();
			CurrentPara.ListFormat.ListLevelNumber = m_pnLevelNumber;
			CurrentPara.ListFormat.CurrentListLevel.PatternType = ListPatternType.Bullet;
			CurrentPara.ListFormat.CurrentListLevel.Word6Legacy = true;
			ResetListFontName(CurrentPara.ListFormat.CurrentListLevel.CharacterFormat);
			m_bIsContinousList = false;
			break;
		case "pnf":
		{
			foreach (KeyValuePair<string, RtfFont> item10 in m_fontTable)
			{
				if (item10.Key == token)
				{
					CurrRtfFont = item10.Value;
					CurrentPara.ListFormat.CurrentListLevel.CharacterFormat.FontName = CurrRtfFont.FontName;
				}
			}
			break;
		}
		case "pnlvlbody":
			if (m_document.ListStyles.Count > 0 && m_isPnNextList)
			{
				if (!IsPnListStyleDefinedExisting(groupOrder[0]))
				{
					m_isPnNextList = true;
				}
				else
				{
					m_isPnNextList = false;
				}
			}
			if (m_isPnNextList)
			{
				m_uniqueStyleID = Guid.NewGuid().ToString();
				m_isPnNextList = false;
				string styleName = "Numbered" + m_uniqueStyleID;
				ListStyle listStyle = new ListStyle(m_document, ListType.Numbered);
				isPnStartUpdate = true;
				listStyle = m_document.AddListStyle(listStyle.ListType, styleName);
			}
			if (m_currentTableType == RtfTableType.None)
			{
				m_bIsList = true;
			}
			if ((m_bIsPreviousList && m_bIsContinousList) || !m_bIsContinousList)
			{
				if (m_uniqueStyleID != null)
				{
					string styleName2 = "Numbered" + m_uniqueStyleID;
					CurrentPara.ListFormat.ApplyStyle(styleName2);
				}
				else
				{
					CurrentPara.ListFormat.ApplyDefNumberedStyle();
				}
				CurrentPara.ListFormat.ListLevelNumber = m_pnLevelNumber;
				CurrentPara.ListFormat.CurrentListLevel.NumberSuffix = string.Empty;
				CurrentPara.ListFormat.CurrentListLevel.NumberAlignment = ListNumberAlignment.Left;
				CurrentPara.ListFormat.CurrentListLevel.Word6Legacy = true;
				m_bIsContinousList = false;
			}
			else if (m_bIsContinousList)
			{
				CurrentPara.ListFormat.ContinueListNumbering();
				m_bIsContinousList = false;
			}
			break;
		case "pnlvl":
			if (CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel != null)
			{
				CurrentPara.ListFormat.ListLevelNumber = Convert.ToInt32(tokenValue);
			}
			break;
		case "pnstart":
			if (isPnStartUpdate && CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel != null)
			{
				CurrentPara.ListFormat.CurrentListLevel.StartAt = Convert.ToInt32(tokenValue);
			}
			isPnStartUpdate = false;
			break;
		case "pndec":
			if (CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel != null)
			{
				CurrentPara.ListFormat.CurrentListLevel.PatternType = ListPatternType.Arabic;
			}
			break;
		case "pnlcrm":
			if (CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel != null)
			{
				CurrentPara.ListFormat.CurrentListLevel.PatternType = ListPatternType.LowRoman;
			}
			break;
		case "pnucrm":
			if (CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel != null)
			{
				CurrentPara.ListFormat.CurrentListLevel.PatternType = ListPatternType.UpRoman;
			}
			break;
		case "pnucltr":
			if (CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel != null)
			{
				CurrentPara.ListFormat.CurrentListLevel.PatternType = ListPatternType.UpLetter;
			}
			break;
		case "pnlcltr":
			if (CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel != null)
			{
				CurrentPara.ListFormat.CurrentListLevel.PatternType = ListPatternType.LowLetter;
			}
			break;
		case "pnord":
			if (CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel != null)
			{
				CurrentPara.ListFormat.CurrentListLevel.PatternType = ListPatternType.Ordinal;
			}
			break;
		case "pnordt":
			if (CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel != null)
			{
				CurrentPara.ListFormat.CurrentListLevel.PatternType = ListPatternType.OrdinalText;
			}
			break;
		case "pntxta.":
			if (CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel != null)
			{
				CurrentPara.ListFormat.CurrentListLevel.NumberSuffix = ".";
			}
			break;
		case "pnindent":
			if (CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel != null)
			{
				CurrentPara.ListFormat.CurrentListLevel.ParagraphFormat.SetPropertyValue(2, ExtractTwipsValue(tokenValue));
				CurrentPara.ListFormat.CurrentListLevel.TextPosition = ExtractTwipsValue(tokenValue);
			}
			break;
		case "pnsp":
			if (CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel != null)
			{
				CurrentPara.ListFormat.CurrentListLevel.TabSpaceAfter = ExtractTwipsValue(tokenValue);
			}
			break;
		case "pnb*":
		case "pnb":
			if (CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel != null)
			{
				CurrentPara.ListFormat.CurrentListLevel.CharacterFormat.Bold = true;
			}
			break;
		case "pni*":
		case "pni":
			if (CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel != null)
			{
				CurrentPara.ListFormat.CurrentListLevel.CharacterFormat.Italic = true;
			}
			break;
		case "pncaps*":
		case "pncaps":
			if (CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel != null)
			{
				CurrentPara.ListFormat.CurrentListLevel.CharacterFormat.AllCaps = true;
			}
			break;
		case "pnul*":
		case "pnul":
			if (CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel != null)
			{
				CurrentPara.ListFormat.CurrentListLevel.CharacterFormat.UnderlineStyle = UnderlineStyle.Single;
			}
			break;
		case "pnuld*":
		case "pnuld":
			if (CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel != null)
			{
				CurrentPara.ListFormat.CurrentListLevel.CharacterFormat.UnderlineStyle = UnderlineStyle.Dotted;
			}
			break;
		case "pnuldash*":
		case "pnuldash":
			if (CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel != null)
			{
				CurrentPara.ListFormat.CurrentListLevel.CharacterFormat.UnderlineStyle = UnderlineStyle.Dash;
			}
			break;
		case "pnulwave*":
		case "pnulwave":
			if (CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel != null)
			{
				CurrentPara.ListFormat.CurrentListLevel.CharacterFormat.UnderlineStyle = UnderlineStyle.Wavy;
			}
			break;
		case "pnuldb*":
		case "pnuldb":
			if (CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel != null)
			{
				CurrentPara.ListFormat.CurrentListLevel.CharacterFormat.UnderlineStyle = UnderlineStyle.Double;
			}
			break;
		case "pnulth*":
		case "pnulth":
			if (CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel != null)
			{
				CurrentPara.ListFormat.CurrentListLevel.CharacterFormat.UnderlineStyle = UnderlineStyle.Thick;
			}
			break;
		case "pnulnone*":
		case "pnulnone":
			if (CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel != null)
			{
				CurrentPara.ListFormat.CurrentListLevel.CharacterFormat.UnderlineStyle = UnderlineStyle.None;
			}
			break;
		case "pnfs":
			if (CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel != null)
			{
				CurrentPara.ListFormat.CurrentListLevel.CharacterFormat.SetPropertyValue(3, (float)Convert.ToInt32(tokenValue) / 2f);
			}
			break;
		case "pnqc":
			if (CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel != null)
			{
				CurrentPara.ListFormat.CurrentListLevel.NumberAlignment = ListNumberAlignment.Center;
			}
			break;
		case "pnql":
			if (CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel != null)
			{
				CurrentPara.ListFormat.CurrentListLevel.NumberAlignment = ListNumberAlignment.Left;
			}
			break;
		case "pnqr":
			if (CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel != null)
			{
				CurrentPara.ListFormat.CurrentListLevel.NumberAlignment = ListNumberAlignment.Right;
			}
			break;
		case "pnlvlcont":
			if (CurrentPara.ListFormat != null && CurrentPara.ListFormat.CurrentListLevel != null)
			{
				CurrentPara.ListFormat.ContinueListNumbering();
				CurrentPara.ListFormat.CurrentListLevel.NoLevelText = true;
			}
			if (m_bIsPreviousList)
			{
				m_bIsContinousList = true;
			}
			break;
		case "brsp":
		{
			float space = ExtractTwipsValue(tokenValue);
			if (!m_bIsRow)
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.Space = space;
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.Space = space;
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.Space = space;
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.Space = space;
				}
			}
			break;
		}
		case "background":
			m_bIsBackgroundCollection = true;
			m_backgroundCollectionStack.Push("{");
			break;
		case "trowd":
			ParseRowStart(isFromShape: false);
			break;
		case "trql":
			CurrRowFormat.HorizontalAlignment = RowAlignment.Left;
			break;
		case "trqr":
			CurrRowFormat.HorizontalAlignment = RowAlignment.Right;
			break;
		case "trqc":
			CurrRowFormat.HorizontalAlignment = RowAlignment.Center;
			break;
		case "ltrrow":
			m_bIsRow = true;
			break;
		case "rtlrow":
			CurrRowFormat.Bidi = true;
			break;
		case "tsrowd":
			CurrRowFormat.Bidi = false;
			break;
		case "row":
		case "nestrow":
			ParseRowEnd(isShapeTextEnd: false);
			break;
		case "clmgf":
			CurrCellFormat.HorizontalMerge = CellMerge.Start;
			break;
		case "clmrg":
			CurrCellFormat.HorizontalMerge = CellMerge.Continue;
			break;
		case "cellx":
		case "cellx-":
			ParseCellBoundary(token, tokenKey, tokenValue);
			break;
		case "cell":
		case "nestcell":
			if (IsFieldGroup && m_currentFieldGroupData != string.Empty)
			{
				ParseFieldGroupData(m_currentFieldGroupData);
				m_currentFieldGroupData = string.Empty;
			}
			ResetParagraphFormat(m_currParagraphFormat);
			ResetCharacterFormat(CurrentPara.BreakCharacterFormat);
			CopyParagraphFormatting(m_currParagraphFormat, CurrentPara.ParagraphFormat);
			m_bInTable = true;
			ProcessTableInfo(isShapeTextEnd: false);
			AddNewParagraph(CurrentPara);
			if (CurrCell != null)
			{
				CopyTextFormatToCharFormat(CurrCell.CharacterFormat, m_currTextFormat);
			}
			m_bCellFinished = true;
			m_currParagraph = new WParagraph(m_document);
			break;
		case "intbl":
			if (!string.IsNullOrEmpty(tokenValue) && tokenValue == "0")
			{
				m_currentLevel = 0;
			}
			else
			{
				m_bInTable = true;
				m_bIsRow = true;
				m_currentLevel = 1;
			}
			if (m_bIsShape && m_shapeTextStack.Count > 0)
			{
				int num13 = (int)m_shapeTextbodyStack.Peek()["m_currentLevel"];
				m_currentLevel = num13 + 2;
			}
			break;
		case "itap":
			if (!m_bIsShape || m_shapeTextStack.Count <= 0)
			{
				m_currentLevel = Convert.ToInt32(tokenValue);
			}
			else if (m_currentLevel <= Convert.ToInt32(tokenValue))
			{
				m_currentLevel = Convert.ToInt32(tokenValue) + 1;
			}
			break;
		case "tdfrmtxtLeft":
			CurrRowFormat.Positioning.DistanceFromLeft = ExtractTwipsValue(tokenValue);
			break;
		case "tdfrmtxtRight":
			CurrRowFormat.Positioning.DistanceFromRight = ExtractTwipsValue(tokenValue);
			break;
		case "tdfrmtxtTop":
			CurrRowFormat.Positioning.DistanceFromTop = ExtractTwipsValue(tokenValue);
			break;
		case "tdfrmtxtBottom":
			CurrRowFormat.Positioning.DistanceFromBottom = ExtractTwipsValue(tokenValue);
			break;
		case "tphcol":
			CurrRowFormat.Positioning.HorizRelationTo = HorizontalRelation.Column;
			break;
		case "tphmrg":
			CurrRowFormat.Positioning.HorizRelationTo = HorizontalRelation.Margin;
			break;
		case "tphpg":
			CurrRowFormat.Positioning.HorizRelationTo = HorizontalRelation.Page;
			break;
		case "tposnegx-":
		case "tposx":
			num = ExtractTwipsValue(tokenValue);
			CurrRowFormat.Positioning.HorizPosition = ((tokenKey == "tposx") ? num : (0f - num));
			break;
		case "tposxc":
			CurrRowFormat.Positioning.HorizPositionAbs = HorizontalPosition.Center;
			break;
		case "tposxi":
			CurrRowFormat.Positioning.HorizPositionAbs = HorizontalPosition.Inside;
			break;
		case "tposxl":
			CurrRowFormat.Positioning.HorizPositionAbs = HorizontalPosition.Left;
			break;
		case "tposxo":
			CurrRowFormat.Positioning.HorizPositionAbs = HorizontalPosition.Outside;
			break;
		case "tposxr":
			CurrRowFormat.Positioning.HorizPositionAbs = HorizontalPosition.Right;
			break;
		case "tposnegy-":
		case "tposy":
			num = ExtractTwipsValue(tokenValue);
			CurrRowFormat.Positioning.VertPosition = ((tokenKey == "tposy") ? num : (0f - num));
			break;
		case "tposyb":
			CurrRowFormat.Positioning.VertPositionAbs = VerticalPosition.Bottom;
			break;
		case "tposyc":
			CurrRowFormat.Positioning.VertPositionAbs = VerticalPosition.Center;
			break;
		case "tposyin":
			CurrRowFormat.Positioning.VertPositionAbs = VerticalPosition.Inside;
			break;
		case "tposyout":
			CurrRowFormat.Positioning.VertPositionAbs = VerticalPosition.Outside;
			break;
		case "tposyt":
			CurrRowFormat.Positioning.VertPositionAbs = VerticalPosition.Top;
			break;
		case "tpvmrg":
			CurrRowFormat.Positioning.VertRelationTo = VerticalRelation.Margin;
			break;
		case "tpvpara":
			CurrRowFormat.Positioning.VertRelationTo = VerticalRelation.Paragraph;
			break;
		case "tpvpg":
			CurrRowFormat.Positioning.VertRelationTo = VerticalRelation.Page;
			break;
		case "taprtl":
			CurrRowFormat.Bidi = true;
			break;
		case "trhdr":
			CurrRowFormat.IsHeaderRow = true;
			break;
		case "trkeep":
			CurrRowFormat.IsBreakAcrossPages = false;
			break;
		case "trrh":
			num = ExtractTwipsValue(tokenValue);
			CurrRowFormat.Height = num;
			break;
		case "trrh-":
			num = ExtractTwipsValue(tokenValue);
			CurrRowFormat.Height = 0f - num;
			break;
		case "trpaddb":
			num = ExtractTwipsValue(tokenValue);
			CurrRowFormat.Paddings.Bottom = num;
			break;
		case "trpaddl":
			trpaddlValue = ExtractTwipsValue(tokenValue);
			break;
		case "trpaddr":
			num = ExtractTwipsValue(tokenValue);
			CurrRowFormat.Paddings.Right = num;
			break;
		case "trpaddt":
			num = ExtractTwipsValue(tokenValue);
			CurrRowFormat.Paddings.Top = num;
			break;
		case "trpaddfb":
			if (Convert.ToInt32(tokenValue) == 0)
			{
				CurrRowFormat.Paddings.Bottom = 0f;
			}
			break;
		case "trpaddft":
			if (Convert.ToInt32(tokenValue) == 0)
			{
				CurrRowFormat.Paddings.Top = 0f;
			}
			break;
		case "trpaddfr":
			if (Convert.ToInt32(tokenValue) == 0)
			{
				CurrRowFormat.Paddings.Right = 0f;
			}
			break;
		case "trpaddfl":
			if (Convert.ToInt32(tokenValue) == 0)
			{
				CurrRowFormat.Paddings.Left = 0f;
			}
			else
			{
				istrpaddltypeDefined = true;
			}
			break;
		case "trspdb":
			num = ExtractTwipsValue(tokenValue);
			m_bottomcellspace = num;
			break;
		case "trspdl":
			num = ExtractTwipsValue(tokenValue);
			m_leftcellspace = num;
			break;
		case "trspdr":
			num = ExtractTwipsValue(tokenValue);
			m_rightcellspace = num;
			break;
		case "trspdt":
			num = ExtractTwipsValue(tokenValue);
			m_topcellspace = num;
			break;
		case "trspdfb":
			if (Convert.ToInt32(tokenValue) == 3)
			{
				CurrRowFormat.CellSpacing = m_bottomcellspace;
			}
			break;
		case "trspdft":
			if (Convert.ToInt32(tokenValue) == 3)
			{
				CurrRowFormat.CellSpacing = m_topcellspace;
			}
			break;
		case "trspdfl":
			if (Convert.ToInt32(tokenValue) == 3)
			{
				CurrRowFormat.CellSpacing = m_leftcellspace;
			}
			break;
		case "trspdfr":
			if (Convert.ToInt32(tokenValue) == 3)
			{
				CurrRowFormat.CellSpacing = m_rightcellspace;
			}
			break;
		case "tabsnoovrlp":
			if (CurrRowFormat.WrapTextAround && Convert.ToInt32(tokenValue) == 1)
			{
				CurrRowFormat.Positioning.AllowOverlap = false;
			}
			break;
		case "trgaph":
		{
			num = ExtractTwipsValue(tokenValue);
			Paddings paddings = CurrRowFormat.Paddings;
			float left = (CurrRowFormat.Paddings.Right = num);
			paddings.Left = left;
			m_bIsWord97StylePadding = true;
			break;
		}
		case "trleft":
			m_currenttrleft = Convert.ToInt32(tokenValue);
			num = ExtractTwipsValue(tokenValue);
			CurrRowFormat.LeftIndent = num;
			if (m_bIsWord97StylePadding)
			{
				m_currRowLeftIndent = (int)((double)CurrRowFormat.LeftIndent * 20.0);
				CurrRowFormat.BeforeWidth = num;
			}
			m_bIsWord97StylePadding = false;
			m_currRowFormat.IsLeftIndentDefined = true;
			break;
		case "trleft-":
			m_currenttrleft = -Convert.ToInt32(tokenValue);
			num = 0f - ExtractTwipsValue(tokenValue);
			CurrRowFormat.LeftIndent = num;
			if (m_bIsWord97StylePadding)
			{
				m_currRowLeftIndent = (int)((double)CurrRowFormat.LeftIndent * 20.0);
			}
			m_bIsWord97StylePadding = false;
			break;
		case "tblindtype":
			istblindtypeDefined = true;
			break;
		case "tblind":
			num = ExtractTwipsValue(tokenValue);
			m_currRowLeftIndent = Convert.ToInt32(tokenValue);
			tblindValue = num;
			m_currRowFormat.IsLeftIndentDefined = true;
			break;
		case "tblind-":
			num = ExtractTwipsValue(tokenValue);
			m_currRowLeftIndent = -Convert.ToInt32(tokenValue);
			tblindValue = 0f - num;
			m_currRowFormat.IsLeftIndentDefined = true;
			break;
		case "trautofit":
			if (tokenValue == "1")
			{
				CurrRowFormat.IsAutoResized = true;
			}
			break;
		case "trftsWidth":
			CurrRowFormat.PreferredWidth.WidthType = (FtsWidth)Convert.ToInt32(tokenValue);
			if (CurrRowFormat.PreferredWidth.WidthType == FtsWidth.Auto)
			{
				CurrRowFormat.PreferredWidth.Width = 0f;
			}
			if (CurrRowFormat.PreferredWidth.WidthType == FtsWidth.Percentage)
			{
				CurrRowFormat.PreferredWidth.Width = 0f;
			}
			if (m_previousTokenKey == "trwWidth" && CurrRowFormat.PreferredWidth.WidthType == FtsWidth.Percentage)
			{
				CurrRowFormat.PreferredWidth.Width = (float)GetIntValue(m_previousTokenValue) / 50f;
			}
			break;
		case "trwWidth":
			if (CurrRowFormat.PreferredWidth.WidthType == FtsWidth.Percentage)
			{
				CurrRowFormat.PreferredWidth.Width = (float)GetIntValue(tokenValue) / 50f;
			}
			else if (CurrRowFormat.PreferredWidth.WidthType == FtsWidth.Point)
			{
				CurrRowFormat.PreferredWidth.Width = (float)GetIntValue(tokenValue) / 20f;
			}
			break;
		case "trftsWidthB":
			CurrRowFormat.GridBeforeWidth.WidthType = (FtsWidth)Convert.ToInt32(tokenValue);
			break;
		case "trwWidthB":
			if (CurrRowFormat.GridBeforeWidth.WidthType == FtsWidth.Percentage)
			{
				CurrRowFormat.GridBeforeWidth.Width = (float)GetIntValue(tokenValue) / 50f;
			}
			else if (CurrRowFormat.GridBeforeWidth.WidthType == FtsWidth.Point)
			{
				CurrRowFormat.GridBeforeWidth.Width = (float)GetIntValue(tokenValue) / 20f;
			}
			break;
		case "trftsWidthA":
			CurrRowFormat.GridAfterWidth.WidthType = (FtsWidth)Convert.ToInt32(tokenValue);
			break;
		case "trwWidthA":
			if (CurrRowFormat.GridAfterWidth.WidthType == FtsWidth.Percentage)
			{
				CurrRowFormat.GridAfterWidth.Width = (float)GetIntValue(tokenValue) / 50f;
			}
			else if (CurrRowFormat.GridAfterWidth.WidthType == FtsWidth.Point)
			{
				CurrRowFormat.GridAfterWidth.Width = (float)GetIntValue(tokenValue) / 20f;
			}
			break;
		case "clFitText":
			CurrCellFormat.FitText = true;
			break;
		case "clNoWrap":
			CurrCellFormat.TextWrap = false;
			break;
		case "clpadt":
			CurrCellFormat.Paddings.Left = ExtractTwipsValue(tokenValue);
			break;
		case "clpadl":
			CurrCellFormat.Paddings.Top = ExtractTwipsValue(tokenValue);
			break;
		case "clpadb":
			CurrCellFormat.Paddings.Bottom = ExtractTwipsValue(tokenValue);
			break;
		case "clpadr":
			CurrCellFormat.Paddings.Right = ExtractTwipsValue(tokenValue);
			break;
		case "clhidemark":
			CurrCellFormat.HideMark = true;
			break;
		case "clftsWidth":
			CurrCellFormat.PreferredWidth.WidthType = (FtsWidth)Convert.ToInt32(tokenValue);
			if (CurrCellFormat.PreferredWidth.WidthType == FtsWidth.Percentage && m_previousTokenKey == "clwWidth")
			{
				CurrCellFormat.PreferredWidth.Width = (float)GetIntValue(m_previousTokenValue) / 50f;
			}
			break;
		case "clwWidth":
			CurrCellFormat.PreferredWidth.Width = ExtractTwipsValue(tokenValue);
			if (CurrCellFormat.PreferredWidth.WidthType == FtsWidth.Percentage)
			{
				CurrCellFormat.PreferredWidth.Width = (float)GetIntValue(tokenValue) / 50f;
			}
			break;
		case "clvertalt":
			CurrCellFormat.VerticalAlignment = VerticalAlignment.Top;
			break;
		case "clvertalc":
			CurrCellFormat.VerticalAlignment = VerticalAlignment.Middle;
			break;
		case "clvertalb":
			CurrCellFormat.VerticalAlignment = VerticalAlignment.Bottom;
			break;
		case "cltxlrtb":
			CurrCellFormat.TextDirection = TextDirection.Horizontal;
			break;
		case "cltxtbrl":
			CurrCellFormat.TextDirection = TextDirection.VerticalTopToBottom;
			break;
		case "cltxbtlr":
			CurrCellFormat.TextDirection = TextDirection.VerticalBottomToTop;
			break;
		case "cltxlrtbv":
			CurrCellFormat.TextDirection = TextDirection.HorizontalFarEast;
			break;
		case "cltxtbrlv":
			CurrCellFormat.TextDirection = TextDirection.VerticalFarEast;
			break;
		case "clbrdrt":
			CurrCellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
			m_bIsBorderTop = true;
			m_bIsBorderBottom = false;
			m_bIsBorderLeft = false;
			m_bIsBorderRight = false;
			m_bIsBorderDiagonalUp = false;
			m_bIsBorderDiagonalDown = false;
			m_bIsRowBorderBottom = false;
			m_bIsRowBorderLeft = false;
			m_bIsRowBorderRight = false;
			m_bIsRowBorderTop = false;
			break;
		case "clbrdrr":
			CurrCellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
			m_bIsBorderRight = true;
			m_bIsBorderTop = false;
			m_bIsBorderBottom = false;
			m_bIsBorderLeft = false;
			m_bIsBorderDiagonalUp = false;
			m_bIsBorderDiagonalDown = false;
			m_bIsRowBorderBottom = false;
			m_bIsRowBorderLeft = false;
			m_bIsRowBorderRight = false;
			m_bIsRowBorderTop = false;
			break;
		case "clbrdrl":
			CurrCellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
			m_bIsBorderLeft = true;
			m_bIsBorderRight = false;
			m_bIsBorderTop = false;
			m_bIsBorderBottom = false;
			m_bIsBorderDiagonalUp = false;
			m_bIsBorderDiagonalDown = false;
			m_bIsRowBorderBottom = false;
			m_bIsRowBorderLeft = false;
			m_bIsRowBorderRight = false;
			m_bIsRowBorderTop = false;
			break;
		case "clbrdrb":
			CurrCellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;
			m_bIsBorderBottom = true;
			m_bIsBorderLeft = false;
			m_bIsBorderRight = false;
			m_bIsBorderTop = false;
			m_bIsBorderDiagonalDown = false;
			m_bIsBorderDiagonalUp = false;
			m_bIsRowBorderBottom = false;
			m_bIsRowBorderLeft = false;
			m_bIsRowBorderRight = false;
			m_bIsRowBorderTop = false;
			break;
		case "cldglu":
			CurrCellFormat.Borders.DiagonalUp.BorderType = BorderStyle.Cleared;
			m_bIsBorderDiagonalDown = true;
			m_bIsBorderDiagonalUp = false;
			m_bIsBorderLeft = false;
			m_bIsBorderRight = false;
			m_bIsBorderTop = false;
			m_bIsBorderBottom = false;
			m_bIsRowBorderBottom = false;
			m_bIsRowBorderLeft = false;
			m_bIsRowBorderRight = false;
			m_bIsRowBorderTop = false;
			break;
		case "cldgll":
			CurrCellFormat.Borders.DiagonalDown.BorderType = BorderStyle.Cleared;
			m_bIsBorderDiagonalUp = true;
			m_bIsBorderDiagonalDown = true;
			m_bIsBorderLeft = false;
			m_bIsBorderRight = false;
			m_bIsBorderTop = false;
			m_bIsBorderBottom = false;
			m_bIsRowBorderBottom = false;
			m_bIsRowBorderLeft = false;
			m_bIsRowBorderRight = false;
			m_bIsRowBorderTop = false;
			break;
		case "trbrdrt":
			m_bIsRowBorderTop = true;
			m_bIsRowBorderBottom = false;
			m_bIsRowBorderLeft = false;
			m_bIsRowBorderRight = false;
			m_bIsBorderBottom = false;
			m_bIsBorderLeft = false;
			m_bIsBorderRight = false;
			m_bIsBorderTop = false;
			m_bIsBorderDiagonalDown = false;
			m_bIsBorderDiagonalUp = false;
			break;
		case "trbrdrr":
			m_bIsRowBorderRight = true;
			m_bIsRowBorderTop = false;
			m_bIsRowBorderBottom = false;
			m_bIsRowBorderLeft = false;
			m_bIsBorderBottom = false;
			m_bIsBorderLeft = false;
			m_bIsBorderRight = false;
			m_bIsBorderTop = false;
			m_bIsBorderDiagonalDown = false;
			m_bIsBorderDiagonalUp = false;
			break;
		case "trbrdrl":
			m_bIsRowBorderLeft = true;
			m_bIsRowBorderRight = false;
			m_bIsRowBorderTop = false;
			m_bIsRowBorderBottom = false;
			m_bIsBorderBottom = false;
			m_bIsBorderLeft = false;
			m_bIsBorderRight = false;
			m_bIsBorderTop = false;
			m_bIsBorderDiagonalDown = false;
			m_bIsBorderDiagonalUp = false;
			break;
		case "trbrdrb":
			m_bIsRowBorderBottom = true;
			m_bIsRowBorderLeft = false;
			m_bIsRowBorderRight = false;
			m_bIsRowBorderTop = false;
			m_bIsBorderBottom = false;
			m_bIsBorderLeft = false;
			m_bIsBorderRight = false;
			m_bIsBorderTop = false;
			m_bIsBorderDiagonalDown = false;
			m_bIsBorderDiagonalUp = false;
			break;
		case "trbrdrh":
			m_bIsHorizontalBorder = true;
			m_bIsVerticalBorder = false;
			break;
		case "trbrdrv":
			m_bIsVerticalBorder = true;
			m_bIsHorizontalBorder = false;
			m_bIsRowBorderRight = false;
			break;
		case "clcbpat":
		case "clcbpatraw":
		{
			int num9 = Convert.ToInt32(tokenValue);
			CurrColorTable = new RtfColor();
			{
				foreach (KeyValuePair<int, RtfColor> item11 in m_colorTable)
				{
					if (item11.Key == num9)
					{
						CurrColorTable = item11.Value;
						CurrCellFormat.BackColor = Color.FromArgb(CurrColorTable.RedN, CurrColorTable.GreenN, CurrColorTable.BlueN);
					}
				}
				break;
			}
		}
		case "clcfpat":
		case "clcfpatraw":
		{
			int num8 = Convert.ToInt32(tokenValue);
			{
				foreach (KeyValuePair<int, RtfColor> item12 in m_colorTable)
				{
					if (item12.Key == num8)
					{
						CurrColorTable = item12.Value;
						CurrCellFormat.ForeColor = Color.FromArgb(CurrColorTable.RedN, CurrColorTable.GreenN, CurrColorTable.BlueN);
					}
				}
				break;
			}
		}
		case "clshdngraw":
		case "clshdng":
			CurrCellFormat.TextureStyle = GetTextureStyle(Convert.ToInt32(tokenValue));
			break;
		case "clbghoriz":
		case "rawclbghoriz":
			CurrCellFormat.TextureStyle = TextureStyle.TextureHorizontal;
			break;
		case "clbgvert":
		case "rawclbgvert":
			CurrCellFormat.TextureStyle = TextureStyle.TextureVertical;
			break;
		case "clbgfdiag":
		case "rawclbgfdiag":
			CurrCellFormat.TextureStyle = TextureStyle.TextureDiagonalDown;
			break;
		case "clbgbdiag":
		case "rawclbgbdiag":
			CurrCellFormat.TextureStyle = TextureStyle.TextureDiagonalUp;
			break;
		case "clbgcross":
		case "rawclbgcross":
			CurrCellFormat.TextureStyle = TextureStyle.TextureCross;
			break;
		case "clbgdcross":
		case "rawclbgdcross":
			CurrCellFormat.TextureStyle = TextureStyle.TextureDiagonalCross;
			break;
		case "clbgdkhor":
		case "rawclbgdkhor":
			CurrCellFormat.TextureStyle = TextureStyle.TextureDarkHorizontal;
			break;
		case "clbgdkvert":
		case "rawclbgdkvert":
			CurrCellFormat.TextureStyle = TextureStyle.TextureDarkVertical;
			break;
		case "clbgdkfdiag":
		case "rawclbgdkfdiag":
			CurrCellFormat.TextureStyle = TextureStyle.TextureDarkDiagonalDown;
			break;
		case "clbgdkbdiag":
		case "rawclbgdkbdiag":
			CurrCellFormat.TextureStyle = TextureStyle.TextureDarkDiagonalUp;
			break;
		case "clbgdkcross":
		case "rawclbgdkcross":
			CurrCellFormat.TextureStyle = TextureStyle.TextureDarkCross;
			break;
		case "clbgdkdcross":
		case "rawclbgdkdcross":
			CurrCellFormat.TextureStyle = TextureStyle.TextureDarkDiagonalCross;
			break;
		case "clvmgf":
			CurrCellFormat.VerticalMerge = CellMerge.Start;
			break;
		case "clvmrg":
			CurrCellFormat.VerticalMerge = CellMerge.Continue;
			break;
		case "field":
			if (!IsFieldGroup)
			{
				m_currentFieldGroupData = string.Empty;
				m_fieldInstructionGroupStack = new Stack<int>();
				m_fieldResultGroupStack = new Stack<int>();
				m_fieldGroupStack = new Stack<int>();
			}
			if (m_fieldGroupStack.Count > 0)
			{
				m_fieldGroupStack.Push(m_fieldGroupStack.Pop() - 1);
			}
			m_fieldGroupStack.Push(1);
			break;
		case "fldinst":
			if (m_fieldInstructionGroupStack.Count > 0)
			{
				m_fieldInstructionGroupStack.Push(m_fieldInstructionGroupStack.Pop() - 1);
			}
			m_fieldInstructionGroupStack.Push(1);
			m_fieldGroupTypeStack.Push(FieldGroupType.FieldInstruction);
			break;
		case "fldrslt":
			if (m_fieldCollectionStack.Count > 0)
			{
				WFieldMark wFieldMark = new WFieldMark(m_document);
				wFieldMark.Type = FieldMarkType.FieldSeparator;
				CurrentPara.ChildEntities.Add(wFieldMark);
				m_fieldCollectionStack.Peek().FieldSeparator = wFieldMark;
			}
			if (m_fieldResultGroupStack.Count > 0)
			{
				m_fieldResultGroupStack.Push(m_fieldResultGroupStack.Pop() - 1);
			}
			m_fieldResultGroupStack.Push(1);
			m_fieldGroupTypeStack.Push(FieldGroupType.FieldResult);
			break;
		case "wpfldparam":
		case "fldtitle":
			m_fieldGroupTypeStack.Push(FieldGroupType.FieldInvalid);
			break;
		case "v":
			if (tokenValue != null && Convert.ToInt32(tokenValue) == 0)
			{
				m_currTextFormat.IsHiddenText = false;
			}
			else
			{
				m_currTextFormat.IsHiddenText = true;
			}
			break;
		case "spv":
			if (tokenValue != null && Convert.ToInt32(tokenValue) == 0)
			{
				m_currTextFormat.SpecVanish = false;
			}
			else
			{
				m_currTextFormat.SpecVanish = true;
			}
			break;
		case "fftype":
			if (m_currentFormField != null)
			{
				switch (Convert.ToInt32(tokenValue))
				{
				case 0:
					m_currentFormField.FormFieldType = FormFieldType.TextInput;
					break;
				case 1:
					m_currentFormField.FormFieldType = FormFieldType.CheckBox;
					break;
				case 2:
					m_currentFormField.FormFieldType = FormFieldType.DropDown;
					break;
				}
			}
			break;
		case "ffprot":
			if (m_currentFormField != null && tokenValue == "1")
			{
				m_currentFormField.Enabled = false;
			}
			break;
		case "ffsize":
			if (m_currentFormField != null)
			{
				if (tokenValue == null || tokenValue == "1")
				{
					m_currentFormField.CheckboxSizeType = CheckBoxSizeType.Exactly;
				}
				else
				{
					m_currentFormField.CheckboxSizeType = CheckBoxSizeType.Auto;
				}
			}
			break;
		case "ffrecalc":
			if (m_currentFormField != null)
			{
				if (tokenValue == null || tokenValue == "1")
				{
					m_currentFormField.CalculateOnExit = true;
				}
				else
				{
					m_currentFormField.CalculateOnExit = false;
				}
			}
			break;
		case "ffhaslistbox":
			if (m_currentFormField != null)
			{
				if (tokenValue == null || tokenValue == "1")
				{
					m_currentFormField.IsListBox = true;
					m_currentFormField.DropDownItems = new WDropDownCollection(m_document);
				}
				else
				{
					m_currentFormField.IsListBox = false;
				}
			}
			break;
		case "ffmaxlen":
			if (m_currentFormField != null && tokenValue != null && tokenValue != string.Empty)
			{
				m_currentFormField.MaxLength = Convert.ToInt32(tokenValue);
			}
			break;
		case "ffhps":
			if (m_currentFormField != null && tokenValue != null && tokenValue != string.Empty && m_currentFormField.CheckboxSizeType == CheckBoxSizeType.Exactly)
			{
				m_currentFormField.CheckboxSize = Convert.ToInt32(tokenValue) / 2;
			}
			break;
		case "ffres":
			if (m_currentFormField != null && tokenValue != null && tokenValue != string.Empty)
			{
				m_currentFormField.Ffres = Convert.ToInt32(tokenValue);
			}
			break;
		case "ffdefres":
			if (m_currentFormField != null && tokenValue != null && tokenValue != string.Empty)
			{
				m_currentFormField.Ffdefres = Convert.ToInt32(tokenValue);
			}
			break;
		case "title":
			m_document.BuiltinDocumentProperties.Title = "";
			break;
		case "category":
			m_document.BuiltinDocumentProperties.Category = "";
			break;
		case "doccomm":
			m_document.BuiltinDocumentProperties.Comments = "";
			break;
		case "operator":
			m_document.BuiltinDocumentProperties.Author = "";
			break;
		case "manager":
			m_document.BuiltinDocumentProperties.Manager = "";
			break;
		case "company":
			m_document.BuiltinDocumentProperties.Company = "";
			break;
		case "keywords":
			m_document.BuiltinDocumentProperties.Keywords = "";
			break;
		case "subject":
			m_document.BuiltinDocumentProperties.Subject = "";
			break;
		case "lbr":
			if (m_currTextFormat != null)
			{
				switch (tokenValue)
				{
				case "1":
					m_currTextFormat.m_BreakClear = BreakClearType.Left;
					break;
				case "2":
					m_currTextFormat.m_BreakClear = BreakClearType.Right;
					break;
				case "3":
					m_currTextFormat.m_BreakClear = BreakClearType.All;
					break;
				default:
					m_currTextFormat.m_BreakClear = BreakClearType.None;
					break;
				}
			}
			break;
		case "fcs":
			if (tokenValue != null && Convert.ToInt32(tokenValue) == 0)
			{
				m_currTextFormat.complexScript = ThreeState.False;
			}
			else
			{
				m_currTextFormat.complexScript = ThreeState.True;
			}
			break;
		case "ab":
			if (m_currTextFormat.complexScript == ThreeState.True)
			{
				m_currTextFormat.boldBidi = ThreeState.True;
			}
			else
			{
				m_currTextFormat.Bold = ThreeState.True;
			}
			break;
		case "ai":
			if (m_currTextFormat.complexScript == ThreeState.True)
			{
				m_currTextFormat.italicBidi = ThreeState.True;
			}
			else
			{
				m_currTextFormat.Italic = ThreeState.True;
			}
			break;
		case "nocxsptable":
			m_document.Settings.CompatibilityOptions[CompatibilityOption.AllowSpaceOfSameStyleInTable] = false;
			break;
		default:
			ParseSpecialCharacters(token);
			break;
		case "atnparent":
			break;
		case "cufi":
			break;
		case "culi":
			break;
		case "curi":
			break;
		case "clpadfl":
			break;
		case "clpadft":
			break;
		case "clpadfb":
			break;
		case "clpadfr":
			break;
		}
	}

	private float SetFramePositions(string tokenValue, bool isNeg, bool isXValue)
	{
		float num = (isNeg ? (0f - ExtractTwipsValue(tokenValue)) : ExtractTwipsValue(tokenValue));
		float num2 = (isNeg ? (-GetIntValue(tokenValue)) : GetIntValue(tokenValue));
		if (isXValue)
		{
			if (m_currParagraphFormat.IsFrameXAlign(num2))
			{
				num += 0.05f;
			}
		}
		else if (m_currParagraphFormat.IsFrameYAlign(num2))
		{
			num += 0.05f;
		}
		return num;
	}

	private void ResetParagraphFormat(WParagraphFormat sourceParaFormat)
	{
		if (!sourceParaFormat.HasKey(8))
		{
			sourceParaFormat.SetPropertyValue(8, 0f);
		}
		if (!sourceParaFormat.HasKey(9))
		{
			sourceParaFormat.SetPropertyValue(9, 0f);
		}
		if (!sourceParaFormat.HasKey(31))
		{
			sourceParaFormat.Bidi = false;
		}
		if (!sourceParaFormat.HasKey(22))
		{
			sourceParaFormat.ColumnBreakAfter = false;
		}
		if (!sourceParaFormat.HasKey(92))
		{
			sourceParaFormat.ContextualSpacing = false;
		}
		if (!sourceParaFormat.HasKey(5))
		{
			sourceParaFormat.SetPropertyValue(5, 0f);
		}
		if (!sourceParaFormat.HasKey(0))
		{
			sourceParaFormat.HorizontalAlignment = HorizontalAlignment.Left;
		}
		if (!sourceParaFormat.HasKey(6))
		{
			sourceParaFormat.Keep = false;
		}
		if (!sourceParaFormat.HasKey(10))
		{
			sourceParaFormat.KeepFollow = false;
		}
		if (!sourceParaFormat.HasKey(2))
		{
			sourceParaFormat.SetPropertyValue(2, 0f);
		}
		if (!sourceParaFormat.HasKey(52))
		{
			sourceParaFormat.SetPropertyValue(52, 12f);
		}
		if (!sourceParaFormat.HasKey(53))
		{
			sourceParaFormat.LineSpacingRule = LineSpacingRule.Multiple;
		}
		if (!sourceParaFormat.HasKey(56))
		{
			sourceParaFormat.OutlineLevel = OutlineLevel.BodyText;
		}
		if (!sourceParaFormat.HasKey(13))
		{
			sourceParaFormat.PageBreakAfter = false;
		}
		if (!sourceParaFormat.HasKey(12))
		{
			sourceParaFormat.PageBreakBefore = false;
		}
		if (!sourceParaFormat.HasKey(3))
		{
			sourceParaFormat.SetPropertyValue(3, 0f);
		}
		if (!sourceParaFormat.HasKey(55))
		{
			sourceParaFormat.SpaceAfterAuto = false;
		}
		if (!sourceParaFormat.HasKey(54))
		{
			sourceParaFormat.SpaceBeforeAuto = false;
		}
		if (!sourceParaFormat.HasKey(11))
		{
			sourceParaFormat.WidowControl = false;
		}
		if (!sourceParaFormat.HasKey(89))
		{
			sourceParaFormat.WordWrap = true;
		}
		if (!sourceParaFormat.HasKey(83))
		{
			sourceParaFormat.FrameHorizontalDistanceFromText = 0f;
		}
		if (!sourceParaFormat.HasKey(84))
		{
			sourceParaFormat.FrameVerticalDistanceFromText = 0f;
		}
		if (!sourceParaFormat.HasKey(76))
		{
			sourceParaFormat.FrameWidth = 0f;
		}
		if (!sourceParaFormat.HasKey(77))
		{
			sourceParaFormat.FrameHeight = 0f;
		}
		if (!sourceParaFormat.HasKey(74))
		{
			sourceParaFormat.FrameY = 0f;
		}
		if (!sourceParaFormat.HasKey(73))
		{
			sourceParaFormat.FrameX = 0f;
		}
		if (!sourceParaFormat.HasKey(72))
		{
			sourceParaFormat.FrameVerticalPos = 0;
		}
		if (!sourceParaFormat.HasKey(71))
		{
			sourceParaFormat.FrameHorizontalPos = 0;
		}
		if (!sourceParaFormat.HasKey(88))
		{
			sourceParaFormat.WrapFrameAround = FrameWrapMode.Auto;
		}
		if (!sourceParaFormat.HasKey(21))
		{
			sourceParaFormat.BackColor = Color.Empty;
		}
		if (!sourceParaFormat.HasKey(32))
		{
			sourceParaFormat.ForeColor = Color.Empty;
		}
		if (!sourceParaFormat.HasKey(33))
		{
			sourceParaFormat.TextureStyle = TextureStyle.TextureNone;
		}
		ResetBorders(sourceParaFormat);
	}

	private void ResetBorders(WParagraphFormat sourceParaFormat)
	{
		Borders borders = sourceParaFormat.Borders;
		if (!borders.Top.IsBorderDefined)
		{
			ResetBorder(sourceParaFormat.Borders.Top);
		}
		if (!borders.Left.IsBorderDefined)
		{
			ResetBorder(sourceParaFormat.Borders.Left);
		}
		if (!borders.Bottom.IsBorderDefined)
		{
			ResetBorder(sourceParaFormat.Borders.Bottom);
		}
		if (!borders.Right.IsBorderDefined)
		{
			ResetBorder(sourceParaFormat.Borders.Right);
		}
	}

	private void ResetBorder(Border border)
	{
		border.LineWidth = 0f;
		border.Color = Color.Empty;
		border.BorderType = BorderStyle.None;
		border.Shadow = false;
		border.Space = 0f;
	}

	private void ResetCharacterFormat(WCharacterFormat sourceFormat)
	{
		if (sourceFormat.FontSize != 10f && !sourceFormat.HasKey(3))
		{
			sourceFormat.SetPropertyValue(3, 10f);
		}
		if (sourceFormat.Scaling != 100f && !sourceFormat.HasKey(127))
		{
			sourceFormat.Scaling = 100f;
		}
		if (sourceFormat.TextColor != Color.Empty && !sourceFormat.HasKey(1))
		{
			sourceFormat.TextColor = Color.Empty;
		}
		if (sourceFormat.FontName != "Times New Roman" && !sourceFormat.HasKey(2))
		{
			sourceFormat.FontName = "Times New Roman";
		}
		if (sourceFormat.Bold && !sourceFormat.HasKey(4))
		{
			sourceFormat.Bold = false;
		}
		if (sourceFormat.Italic && !sourceFormat.HasKey(5))
		{
			sourceFormat.Italic = false;
		}
		if (sourceFormat.UnderlineStyle != 0 && !sourceFormat.HasKey(7))
		{
			sourceFormat.UnderlineStyle = UnderlineStyle.None;
		}
		if (sourceFormat.HighlightColor != Color.Empty && !sourceFormat.HasKey(63))
		{
			sourceFormat.HighlightColor = Color.Empty;
		}
		if (sourceFormat.Shadow && !sourceFormat.HasKey(50))
		{
			sourceFormat.Shadow = false;
		}
		if (sourceFormat.CharacterSpacing != 0f && !sourceFormat.HasKey(18))
		{
			sourceFormat.SetPropertyValue(18, 0f);
		}
		if (sourceFormat.DoubleStrike && !sourceFormat.HasKey(14))
		{
			sourceFormat.DoubleStrike = false;
		}
		if (sourceFormat.Emboss && !sourceFormat.HasKey(51))
		{
			sourceFormat.Emboss = false;
		}
		if (sourceFormat.Engrave && !sourceFormat.HasKey(52))
		{
			sourceFormat.Engrave = false;
		}
		if (sourceFormat.SubSuperScript != 0 && !sourceFormat.HasKey(10))
		{
			sourceFormat.SubSuperScript = SubSuperScript.None;
		}
		if (sourceFormat.TextBackgroundColor != Color.Empty && !sourceFormat.HasKey(9))
		{
			sourceFormat.TextBackgroundColor = Color.Empty;
		}
		if (sourceFormat.ForeColor != Color.Empty && !sourceFormat.HasKey(77))
		{
			sourceFormat.ForeColor = Color.Empty;
		}
		if (sourceFormat.AllCaps && !sourceFormat.HasKey(54))
		{
			sourceFormat.AllCaps = false;
		}
		if (sourceFormat.BoldBidi && !sourceFormat.HasKey(59))
		{
			sourceFormat.BoldBidi = false;
		}
		if (sourceFormat.FieldVanish && !sourceFormat.HasKey(109))
		{
			sourceFormat.FieldVanish = false;
		}
		if (sourceFormat.Hidden && !sourceFormat.HasKey(53))
		{
			sourceFormat.Hidden = false;
		}
		if (sourceFormat.SpecVanish && !sourceFormat.HasKey(24))
		{
			sourceFormat.SpecVanish = false;
		}
		if (sourceFormat.SmallCaps && !sourceFormat.HasKey(55))
		{
			sourceFormat.SmallCaps = false;
		}
	}

	private float GetEqualColumnWidth(int columnCount)
	{
		return (m_secFormat.pageSize.Width - (m_secFormat.LeftMargin + m_secFormat.RightMargin) - (float)(36 * (columnCount - 1))) / (float)columnCount;
	}

	private void ParseSpecialCharacters(string token)
	{
		string text = null;
		if (!IsDestinationControlWord || IsFieldGroup || m_bIsShapeText)
		{
			if (StartsWithExt(token, "'"))
			{
				if (!IsAccentCharacterNeedToBeOmitted())
				{
					m_bIsAccentChar = true;
					text = GetAccentCharacter(token);
					if (text == " ")
					{
						m_tokenType = RtfTokenType.Text;
						m_lexer.CurrRtfTokenType = RtfTokenType.Text;
					}
				}
				else if (token.Length > 3)
				{
					text = token.Substring(3, token.Length - 3);
				}
			}
			else if (StartsWithExt(token, "_"))
			{
				text = token.Replace("_", '\u001e'.ToString());
			}
			else if (StartsWithExt(token, "~"))
			{
				text = token.Replace("~", '\u00a0'.ToString());
			}
			else if (StartsWithExt(token, "-"))
			{
				text = token.Replace("-", '\u001f'.ToString());
			}
			else if (StartsWithExt(token, ":"))
			{
				text = token;
			}
			else if (StartsWithExt(token, "zw") && m_tokenType == RtfTokenType.ControlWord)
			{
				switch (token)
				{
				case "zwbo":
				case "zwnj":
					text = '\u200c'.ToString();
					break;
				case "zwnbo":
				case "zwj":
					text = '\u200d'.ToString();
					break;
				}
			}
		}
		if (text != null)
		{
			isSpecialCharacter = true;
			ParseDocumentElement(text);
			isSpecialCharacter = false;
		}
		if (StartsWithExt(token, "zw") && m_tokenType == RtfTokenType.Text)
		{
			m_tokenType = RtfTokenType.ControlWord;
			m_lexer.CurrRtfTokenType = RtfTokenType.ControlWord;
		}
	}

	private bool IsAccentCharacterNeedToBeOmitted()
	{
		if (m_unicodeCount > 0 && --m_unicodeCount >= 0)
		{
			return true;
		}
		return false;
	}

	private TextureStyle GetTextureStyle(int textureValue)
	{
		return textureValue switch
		{
			500 => TextureStyle.Texture5Percent, 
			250 => TextureStyle.Texture2Pt5Percent, 
			750 => TextureStyle.Texture7Pt5Percent, 
			1000 => TextureStyle.Texture10Percent, 
			1250 => TextureStyle.Texture12Pt5Percent, 
			1500 => TextureStyle.Texture15Percent, 
			1750 => TextureStyle.Texture17Pt5Percent, 
			2000 => TextureStyle.Texture20Percent, 
			2250 => TextureStyle.Texture22Pt5Percent, 
			2500 => TextureStyle.Texture25Percent, 
			2750 => TextureStyle.Texture27Pt5Percent, 
			3000 => TextureStyle.Texture30Percent, 
			3250 => TextureStyle.Texture32Pt5Percent, 
			3500 => TextureStyle.Texture35Percent, 
			3750 => TextureStyle.Texture37Pt5Percent, 
			4000 => TextureStyle.Texture40Percent, 
			4250 => TextureStyle.Texture42Pt5Percent, 
			4500 => TextureStyle.Texture45Percent, 
			4750 => TextureStyle.Texture47Pt5Percent, 
			5000 => TextureStyle.Texture50Percent, 
			5250 => TextureStyle.Texture52Pt5Percent, 
			5500 => TextureStyle.Texture55Percent, 
			5750 => TextureStyle.Texture57Pt5Percent, 
			6000 => TextureStyle.Texture60Percent, 
			6250 => TextureStyle.Texture62Pt5Percent, 
			6500 => TextureStyle.Texture65Percent, 
			6750 => TextureStyle.Texture67Pt5Percent, 
			7000 => TextureStyle.Texture70Percent, 
			7250 => TextureStyle.Texture72Pt5Percent, 
			7500 => TextureStyle.Texture75Percent, 
			7750 => TextureStyle.Texture77Pt5Percent, 
			8000 => TextureStyle.Texture80Percent, 
			8250 => TextureStyle.Texture82Pt5Percent, 
			8500 => TextureStyle.Texture85Percent, 
			8750 => TextureStyle.Texture87Pt5Percent, 
			9000 => TextureStyle.Texture90Percent, 
			9250 => TextureStyle.Texture92Pt5Percent, 
			9500 => TextureStyle.Texture95Percent, 
			9750 => TextureStyle.Texture97Pt5Percent, 
			10000 => TextureStyle.TextureSolid, 
			_ => TextureStyle.TextureNone, 
		};
	}

	private void ParseListTextStart()
	{
		m_stack.Push("{");
		if (m_bIsList)
		{
			CurrentPara.ListFormat.ContinueListNumbering();
			CopyParagraphFormatting(m_currParagraphFormat, CurrentPara.ParagraphFormat);
		}
		m_previousRtfFont = CurrRtfFont.Clone();
		m_prevParagraph = CurrentPara;
		m_prevTextFormat = m_currTextFormat.Clone();
		m_currTextFormat.Underline = ThreeState.Unknown;
		m_currTextFormat.m_underlineStyle = UnderlineStyle.None;
		m_currTextFormat.FontFamily = string.Empty;
		CurrentPara = new WParagraph(m_document);
		m_listLevelCharFormat = new WCharacterFormat(m_document);
		m_listLevelParaFormat = new WParagraphFormat(m_document);
		m_bIsListText = true;
	}

	private void ParseParagraphEnd()
	{
		if (CurrentPara.ListFormat.CurrentListStyle != null && m_bIsList)
		{
			if (m_listLevelParaFormat != null)
			{
				CopyParagraphFormatting(m_listLevelParaFormat, CurrentPara.ListFormat.CurrentListLevel.ParagraphFormat);
			}
			if (m_listLevelCharFormat != null)
			{
				CopyCharacterFormatting(m_listLevelCharFormat, CurrentPara.ListFormat.CurrentListLevel.CharacterFormat);
			}
		}
		ResetParagraphFormat(m_currParagraphFormat);
		ResetCharacterFormat(CurrentPara.BreakCharacterFormat);
		CopyParagraphFormatting(m_currParagraphFormat, CurrentPara.ParagraphFormat);
		if (!isPardTagpresent && (m_textBody == null || m_textBody.Count == 0) && m_document.LastParagraph != null && m_secCount > 0)
		{
			CurrentPara.ParagraphFormat.CopyFormat(m_document.LastParagraph.ParagraphFormat);
			CurrentPara.BreakCharacterFormat.CopyFormat(m_document.LastParagraph.BreakCharacterFormat);
		}
		m_tabCount = 0;
		IWParagraph currentPara = CurrentPara;
		if (!IsFieldGroup && (!m_bIsShapeText || !(m_previousToken != "nonesttables")) && (IsDestinationControlWord || !(m_previousToken != "nonesttables")) && (!m_isCommentReference || !(m_previousToken != "nonesttables")))
		{
			return;
		}
		if (m_previousToken == "row" && m_previousLevel == m_currentLevel && m_bInTable && m_currentLevel <= 1)
		{
			m_bInTable = false;
			m_currentLevel = 0;
		}
		ProcessTableInfo(isShapeTextEnd: false);
		AddNewParagraph(CurrentPara);
		m_currParagraph = new WParagraph(m_document);
		if (currentPara != null && currentPara.StyleName != null)
		{
			if (!m_bIsListText)
			{
				CurrentPara.ParagraphFormat.SetDefaultProperties();
				CurrentPara.BreakCharacterFormat.SetDefaultProperties();
			}
			(m_currParagraph as WParagraph).ApplyStyle(currentPara.StyleName, isDomChanges: false);
		}
	}

	private void ParseParagraphStart()
	{
		m_bIsLinespacingRule = false;
		m_tabCount = 0;
		m_tabCollection.Clear();
		CurrTabFormat = new TabFormat();
		if (m_bIsList)
		{
			m_bIsPreviousList = true;
		}
		else
		{
			m_bIsPreviousList = false;
		}
		m_bIsList = false;
		if (CurrentPara.Items.Count > 0)
		{
			List<Entity> list = new List<Entity>();
			foreach (Entity item in CurrentPara.Items)
			{
				list.Add(item);
			}
			CurrentPara = new WParagraph(m_document);
			foreach (Entity item2 in list)
			{
				CurrentPara.Items.Add(item2);
			}
			list.Clear();
			list = null;
		}
		else
		{
			CurrentPara = new WParagraph(m_document);
		}
		m_currParagraphFormat = new WParagraphFormat(m_document);
		if (m_paragraphFormatStack.Count > 0)
		{
			m_paragraphFormatStack.Pop();
		}
		m_paragraphFormatStack.Push(m_currParagraphFormat);
		m_bIsBorderTop = false;
		m_bIsBorderBottom = false;
		m_bIsBorderLeft = false;
		m_bIsBorderRight = false;
		if (!m_bIsShape || m_shapeTextStack.Count <= 0)
		{
			m_currentLevel = 0;
		}
	}

	private void ParseSectionStart()
	{
		if (!m_bIsDefaultSectionFormat)
		{
			m_bIsDefaultSectionFormat = true;
			m_secFormat.HeaderDistance = Convert.ToSingle(36);
			m_secFormat.FooterDistance = Convert.ToSingle(36);
			m_defaultSectionFormat = new SecionFormat();
			CopySectionFormat(m_secFormat, m_defaultSectionFormat);
		}
		if (m_bIsHeader || m_bIsFooter || m_bIsRow)
		{
			return;
		}
		if (m_previousToken == "sect")
		{
			m_secCount++;
			m_currSection = new WSection(m_document);
			m_textBody = m_currSection.Body;
			m_secFormat = new SecionFormat();
			if (m_bIsDefaultSectionFormat)
			{
				CopySectionFormat(m_defaultSectionFormat, m_secFormat);
			}
			CurrColumn = null;
		}
		else
		{
			CurrColumn = null;
			CurrentSection.Columns.InnerList.Clear();
			CurrentSection.BreakCode = SectionBreakCode.NewPage;
		}
		CurrentSection.PageSetup.EqualColumnWidth = true;
	}

	private void CopySectionFormat(SecionFormat sourceFormat, SecionFormat destFormat)
	{
		destFormat.BottomMargin = sourceFormat.BottomMargin;
		destFormat.DefaultTabWidth = sourceFormat.DefaultTabWidth;
		destFormat.DifferentFirstPage = sourceFormat.DifferentFirstPage;
		destFormat.DifferentOddAndEvenPage = sourceFormat.DifferentOddAndEvenPage;
		destFormat.FooterDistance = sourceFormat.FooterDistance;
		destFormat.HeaderDistance = sourceFormat.HeaderDistance;
		destFormat.IsFrontPageBorder = sourceFormat.IsFrontPageBorder;
		destFormat.LeftMargin = sourceFormat.LeftMargin;
		destFormat.m_pageOrientation = sourceFormat.m_pageOrientation;
		destFormat.pageSize = sourceFormat.pageSize;
		destFormat.RightMargin = sourceFormat.RightMargin;
		destFormat.TopMargin = sourceFormat.TopMargin;
		destFormat.VertAlignment = sourceFormat.VertAlignment;
		destFormat.FirstPageTray = sourceFormat.FirstPageTray;
		destFormat.OtherPagesTray = sourceFormat.OtherPagesTray;
	}

	private void ParseRowStart(bool isFromShape)
	{
		m_bIsRow = true;
		m_bInTable = true;
		istblindtypeDefined = false;
		tblindValue = 0f;
		istrpaddltypeDefined = false;
		trpaddlValue = 0f;
		m_currCellFormatIndex = -1;
		CurrCellFormat = new CellFormat();
		if (isFromShape && m_currentLevel > 0)
		{
			m_currentLevel++;
		}
		if (m_currentLevel <= 1)
		{
			m_CellFormatStack.Clear();
			m_currRowFormatStack.Clear();
		}
		if (m_currentLevel > 1 && m_currentLevel <= m_CellFormatStack.Count)
		{
			m_CellFormatStack.Pop();
		}
		if (m_currentLevel > 1 && m_currentLevel <= m_currRowFormatStack.Count)
		{
			m_currRowFormatStack.Pop();
		}
		m_cellFormatTable = new Dictionary<int, CellFormat>();
		m_CellFormatStack.Push(m_cellFormatTable);
		CurrRowFormat = new RowFormat(m_document);
		m_currRowFormatStack.Push(CurrRowFormat);
	}

	private void ParseRowEnd(bool isShapeTextEnd)
	{
		m_bIsRow = false;
		m_bInTable = false;
		int num = 0;
		m_prevCellFormatStack = new Stack<Dictionary<int, CellFormat>>(m_CellFormatStack.ToArray());
		m_prevRowFormatStack = new Stack<RowFormat>(m_currRowFormatStack.ToArray());
		if (m_currRowFormatStack.Count > 0)
		{
			CurrRowFormat = m_currRowFormatStack.Pop();
		}
		if (m_currTable != null)
		{
			if (m_currTable.LastRow.PreviousSibling is WTableRow wTableRow && (wTableRow.RowFormat.Positioning.HorizPosition != CurrRowFormat.Positioning.HorizPosition || wTableRow.RowFormat.Positioning.VertPosition != CurrRowFormat.Positioning.VertPosition))
			{
				WTextBody wTextBody = m_nestedTextBody.Peek();
				WTableRow row = m_currTable.LastRow.Clone();
				m_currTable.Rows.Remove(m_currTable.LastRow);
				wTextBody.Items.Add(m_currTable);
				if (CurrentSection.PageSetup.Margins.Bottom != m_secFormat.BottomMargin)
				{
					ApplySectionFormatting();
				}
				if (!m_currTable.TableFormat.IsAutoResized && m_currTable.FirstRow.RowFormat.IsAutoResized)
				{
					m_currTable.TableFormat.IsAutoResized = m_currTable.FirstRow.RowFormat.IsAutoResized;
				}
				(m_currTable as WTable).IsTableGridUpdated = false;
				(m_currTable as WTable).UpdateGridSpan();
				m_currTable = new WTable(m_document);
				m_currTable.Rows.Add(row);
			}
			ApplyRowFormatting(m_currTable.LastRow, CurrRowFormat);
			CopyTextFormatToCharFormat(m_currTable.LastRow.CharacterFormat, m_currTextFormat);
		}
		m_cellFormatTable = m_CellFormatStack.Pop();
		if (!isShapeTextEnd)
		{
			foreach (KeyValuePair<int, CellFormat> item in m_cellFormatTable)
			{
				if (m_currTable != null && num < m_currTable.LastRow.Cells.Count)
				{
					ApplyCellFormatting(m_currTable.LastRow.Cells[num], item.Value);
				}
				num++;
			}
		}
		CurrRowFormat = new RowFormat(m_document);
		if (m_currParagraph != null)
		{
			SetDefaultValue(m_currParagraph.ParagraphFormat);
		}
		m_currCellFormatIndex = -1;
		m_currRow = null;
		m_leftcellspace = 0f;
		m_rightcellspace = 0f;
		m_bottomcellspace = 0f;
		m_topcellspace = 0f;
		istblindtypeDefined = false;
		tblindValue = 0f;
		istrpaddltypeDefined = false;
		trpaddlValue = 0f;
		m_CellFormatStack = new Stack<Dictionary<int, CellFormat>>(m_prevCellFormatStack.ToArray());
		m_currRowFormatStack = new Stack<RowFormat>(m_prevRowFormatStack.ToArray());
		if (m_currentLevel > 1)
		{
			m_currRowFormatStack.Pop();
			m_CellFormatStack.Pop();
			if (m_nestedTable.Count > 0)
			{
				m_currentLevel--;
			}
			else
			{
				m_currentLevel = 0;
			}
		}
		if (PreviousLevel == CurrentLevel)
		{
			m_currentLevel = 0;
		}
	}

	private void ParseCellBoundary(string token, string tokenKey, string tokenValue)
	{
		int currCellBoundary = m_currCellBoundary;
		if (tokenKey.EndsWith("-"))
		{
			m_currCellBoundary = -Convert.ToInt32(tokenValue);
		}
		else
		{
			m_currCellBoundary = Convert.ToInt32(tokenValue);
		}
		m_currCellFormatIndex++;
		m_cellFormatTable = m_CellFormatStack.Pop();
		if (m_cellFormatTable.Count > 0)
		{
			CurrCellFormat.CellWidth = ExtractTwipsValue((m_currCellBoundary - currCellBoundary).ToString());
		}
		else
		{
			CurrCellFormat.CellWidth = ExtractTwipsValue((m_currCellBoundary - m_currenttrleft).ToString());
		}
		m_cellFormatTable.Add(m_currCellFormatIndex, CurrCellFormat);
		m_CellFormatStack.Push(m_cellFormatTable);
		CurrCellFormat = new CellFormat();
		m_bIsBorderTop = false;
		m_bIsBorderRight = false;
		m_bIsBorderLeft = false;
		m_bIsBorderBottom = false;
	}

	private bool GetTextWrapAround(RowFormat.TablePositioning positioning)
	{
		if (positioning.HasValue(64))
		{
			return true;
		}
		if (positioning.HasValue(62))
		{
			return positioning.HorizPosition != 0f;
		}
		if (positioning.HasValue(65))
		{
			if (positioning.VertRelationTo == VerticalRelation.Paragraph)
			{
				return true;
			}
			if (positioning.HasValue(63))
			{
				return positioning.VertPosition != 0f;
			}
		}
		else if (positioning.HasValue(63))
		{
			return positioning.VertPosition != 0f;
		}
		return false;
	}

	private string GetAccentCharacter(string token)
	{
		int result = 0;
		string text = "";
		int length = token.Length;
		string text2 = ((length > 2) ? token.Substring(1, 2) : token.Substring(1, 1));
		string text3 = string.Empty;
		if (text2 != "3f")
		{
			bool flag = false;
			if (m_currTextFormat.FontFamily.Length == 0 && m_currentTableType != 0)
			{
				foreach (KeyValuePair<string, RtfFont> item in m_fontTable)
				{
					if (item.Key.Split('f')[^1] == DefaultFontIndex.ToString())
					{
						flag = true;
						CurrRtfFont = item.Value;
						m_currTextFormat.FontFamily = CurrRtfFont.FontName.Trim();
						break;
					}
				}
			}
			int.TryParse(text2, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result);
			Encoding encoding = GetEncoding();
			if (!IsSingleByte() && ((!(m_previousTokenKey == "u") && !(m_previousTokenKey == "u-")) || !char.IsNumber(m_previousTokenValue[0])))
			{
				short fontCharSet = CurrRtfFont.FontCharSet;
				int position = m_rtfReader.Position;
				string text4 = m_lexer.ReadNextToken(m_previousTokenKey, m_bIsLevelText);
				string text5 = text4;
				while (text4 == "\n" || text4 == "\r")
				{
					text4 = m_lexer.ReadNextToken(m_previousTokenKey, m_bIsLevelText);
				}
				if (text4.StartsWith("\\'") && text4.Length > 3)
				{
					string[] array = SeperateToken(text4);
					m_previousTokenKey = array[0];
					m_previousTokenValue = array[1];
					text4 = text4.Trim();
					text4 = text4.Substring(1);
					text2 = text4.Substring(1, 2) + text2;
					text3 = text4.Substring(3);
					result = int.Parse(text2, NumberStyles.HexNumber);
				}
				else
				{
					m_lexer.m_prevChar = text5[0];
					m_rtfReader.Position = position;
					m_lexer.m_bIsReadNewChar = false;
					if (text4 == "{" && (fontCharSet != 78 || fontCharSet != 128 || fontCharSet != 130))
					{
						CurrRtfFont.FontCharSet = 0;
						encoding = GetEncoding();
						CurrRtfFont.FontCharSet = fontCharSet;
					}
				}
			}
			byte[] bytes = BitConverter.GetBytes((short)result);
			text = encoding.GetString(bytes, 0, bytes.Length);
			text = text.Replace("\0", "");
			if (flag)
			{
				m_currTextFormat.FontFamily = string.Empty;
			}
		}
		else if (m_previousTokenKey == "u" || m_previousTokenKey == "u-")
		{
			text = ((char)Convert.ToInt32(m_previousTokenValue)).ToString();
		}
		if (length > 3)
		{
			text += token.Substring(3, length - 3);
		}
		return text + text3;
	}

	private string GetCodePage()
	{
		switch (GetFontCharSet())
		{
		case 0:
		case 1:
			return "Windows-1252";
		case 77:
			return "macintosh";
		case 78:
			return "x-mac-japanese";
		case 79:
			return "x-mac-korean";
		case 80:
			return "x-mac-chinesesimp";
		case 81:
		case 82:
			return "x-mac-chinesetrad";
		case 83:
			return "x-mac-hebrew";
		case 84:
			return "x-mac-arabic";
		case 85:
			return "x-mac-greek";
		case 86:
			return "x-mac-turkish";
		case 87:
			return "x-mac-thai";
		case 88:
			return "x-mac-ce";
		case 89:
			return "x-mac-cyrillic";
		case 128:
			return "shift_jis";
		case 129:
			return "ks_c_5601-1987";
		case 130:
			return "Johab";
		case 134:
			return "gb2312";
		case 136:
			return "big5";
		case 161:
			return "windows-1253";
		case 162:
			return "windows-1254";
		case 163:
			return "windows-1258";
		case 177:
			return "windows-1255";
		case 178:
		case 179:
		case 180:
		case 181:
			return "windows-1256";
		case 186:
			return "windows-1257";
		case 204:
			return "windows-1251";
		case 222:
			return "windows-874";
		case 238:
			return "windows-1250";
		case 254:
			return "IBM437";
		case 255:
			return "ibm850";
		default:
			return DefaultCodePage;
		}
	}

	private bool IsSingleByte()
	{
		switch (GetFontCharSet())
		{
		case 78:
		case 79:
		case 80:
		case 81:
		case 82:
		case 128:
		case 129:
		case 130:
		case 134:
		case 136:
			return false;
		default:
			return true;
		}
	}

	private int GetFontCharSet()
	{
		if (m_currentTableType == RtfTableType.FontTable && m_lexer.CurrRtfTableType == RtfTableType.FontTable)
		{
			_ = CurrRtfFont.FontCharSet;
			return CurrRtfFont.FontCharSet;
		}
		if ((CurrListLevel == null || !(CurrListLevel.CharacterFormat.FontName == CurrRtfFont.FontName)) && (m_currTextFormat == null || CurrRtfFont == null || !(m_currTextFormat.FontFamily == CurrRtfFont.FontName)))
		{
			return 1;
		}
		return CurrRtfFont.FontCharSet;
	}

	private bool IsSupportedCodePage(int codePage)
	{
		if (codePage <= 20273)
		{
			switch (codePage)
			{
			case 37:
			case 437:
			case 500:
			case 708:
			case 720:
			case 737:
			case 775:
			case 850:
			case 852:
			case 855:
			case 857:
			case 858:
			case 860:
			case 861:
			case 862:
			case 863:
			case 864:
			case 865:
			case 866:
			case 869:
			case 870:
			case 874:
			case 875:
			case 932:
			case 936:
			case 949:
			case 950:
			case 1026:
			case 1047:
			case 1140:
			case 1141:
			case 1142:
			case 1143:
			case 1144:
			case 1145:
			case 1146:
			case 1147:
			case 1148:
			case 1149:
			case 1200:
			case 1201:
			case 1250:
			case 1251:
			case 1252:
			case 1253:
			case 1254:
			case 1255:
			case 1256:
			case 1257:
			case 1258:
			case 1361:
			case 10000:
			case 10001:
			case 10002:
			case 10003:
			case 10004:
			case 10005:
			case 10006:
			case 10007:
			case 10008:
			case 10010:
			case 10017:
			case 10021:
			case 10029:
			case 10079:
			case 10081:
			case 10082:
			case 12000:
			case 12001:
			case 20000:
			case 20001:
			case 20002:
			case 20003:
			case 20004:
			case 20005:
			case 20105:
			case 20106:
			case 20107:
			case 20108:
			case 20127:
			case 20261:
			case 20269:
			case 20273:
				break;
			default:
				goto IL_04e3;
			}
		}
		else
		{
			switch (codePage)
			{
			case 20277:
			case 20278:
			case 20280:
			case 20284:
			case 20285:
			case 20290:
			case 20297:
			case 20420:
			case 20423:
			case 20424:
			case 20833:
			case 20838:
			case 20866:
			case 20871:
			case 20880:
			case 20905:
			case 20924:
			case 20932:
			case 20936:
			case 20949:
			case 21025:
			case 21866:
			case 28591:
			case 28592:
			case 28593:
			case 28594:
			case 28595:
			case 28596:
			case 28597:
			case 28598:
			case 28599:
			case 28603:
			case 28605:
			case 29001:
			case 38598:
			case 50220:
			case 50221:
			case 50222:
			case 50225:
			case 50227:
			case 51932:
			case 51936:
			case 51949:
			case 52936:
			case 54936:
			case 57002:
			case 57003:
			case 57004:
			case 57005:
			case 57006:
			case 57007:
			case 57008:
			case 57009:
			case 57010:
			case 57011:
			case 65000:
			case 65001:
				break;
			default:
				goto IL_04e3;
			}
		}
		return true;
		IL_04e3:
		return false;
	}

	private string GetSupportedCodePage(int codePage)
	{
		return codePage switch
		{
			37 => "IBM037", 
			437 => "IBM437", 
			500 => "IBM500", 
			708 => "ASMO-708", 
			720 => "DOS-720", 
			737 => "ibm737", 
			775 => "ibm775", 
			850 => "ibm850", 
			852 => "ibm852", 
			855 => "IBM855", 
			857 => "ibm857", 
			858 => "IBM00858", 
			860 => "IBM860", 
			861 => "ibm861", 
			862 => "DOS-862", 
			863 => "IBM863", 
			864 => "IBM864", 
			865 => "IBM865", 
			866 => "cp866", 
			869 => "ibm869", 
			870 => "IBM870", 
			874 => "windows-874", 
			875 => "cp875", 
			932 => "shift_jis", 
			936 => "gb2312", 
			949 => "ks_c_5601-1987", 
			950 => "big5", 
			1026 => "IBM1026", 
			1047 => "IBM01047", 
			1140 => "IBM01140", 
			1141 => "IBM01141", 
			1142 => "IBM01142", 
			1143 => "IBM01143", 
			1144 => "IBM01144", 
			1145 => "IBM01145", 
			1146 => "IBM01146", 
			1147 => "IBM01147", 
			1148 => "IBM01148", 
			1149 => "IBM01149", 
			1200 => "utf-16", 
			1201 => "unicodeFFFE", 
			1250 => "windows-1250", 
			1251 => "windows-1251", 
			1252 => "windows-1252", 
			1253 => "windows-1253", 
			1254 => "windows-1254", 
			1255 => "windows-1255", 
			1256 => "windows-1256", 
			1257 => "windows-1257", 
			1258 => "windows-1258", 
			1361 => "Johab", 
			10000 => "macintosh", 
			10001 => "x-mac-japanese", 
			10002 => "x-mac-chinesetrad", 
			10003 => "x-mac-korean", 
			10004 => "x-mac-arabic", 
			10005 => "x-mac-hebrew", 
			10006 => "x-mac-greek", 
			10007 => "x-mac-cyrillic", 
			10008 => "x-mac-chinesesimp", 
			10010 => "x-mac-romanian", 
			10017 => "x-mac-ukrainian", 
			10021 => "x-mac-thai", 
			10029 => "x-mac-ce", 
			10079 => "x-mac-icelandic", 
			10081 => "x-mac-turkish", 
			10082 => "x-mac-croatian", 
			12000 => "utf-32", 
			12001 => "utf-32BE", 
			20000 => "x-Chinese_CNS", 
			20001 => "x-cp20001", 
			20002 => "x_Chinese-Eten", 
			20003 => "x-cp20003", 
			20004 => "x-cp20004", 
			20005 => "x-cp20005", 
			20105 => "x-IA5", 
			20106 => "x-IA5-German", 
			20107 => "x-IA5-Swedish", 
			20108 => "x-IA5-Norwegian", 
			20127 => "us-ascii", 
			20261 => "x-cp20261", 
			20269 => "x-cp20269", 
			20273 => "IBM273", 
			20277 => "IBM277", 
			20278 => "IBM278", 
			20280 => "IBM280", 
			20284 => "IBM284", 
			20285 => "IBM285", 
			20290 => "IBM290", 
			20297 => "IBM297", 
			20420 => "IBM420", 
			20423 => "IBM423", 
			20424 => "IBM424", 
			20833 => "x-EBCDIC-KoreanExtended", 
			20838 => "IBM-Thai", 
			20866 => "koi8-r", 
			20871 => "IBM871", 
			20880 => "IBM880", 
			20905 => "IBM905", 
			20924 => "IBM00924", 
			20932 => "EUC-JP", 
			20936 => "x-cp20936", 
			20949 => "x-cp20949", 
			21025 => "cp1025", 
			21866 => "koi8-u", 
			28591 => "iso-8859-1", 
			28592 => "iso-8859-2", 
			28593 => "iso-8859-3", 
			28594 => "iso-8859-4", 
			28595 => "iso-8859-5", 
			28596 => "iso-8859-6", 
			28597 => "iso-8859-7", 
			28598 => "iso-8859-8", 
			28599 => "iso-8859-9", 
			28603 => "iso-8859-13", 
			28605 => "iso-8859-15", 
			29001 => "x-Europa", 
			38598 => "iso-8859-8-i", 
			50220 => "iso-2022-jp", 
			50221 => "csISO2022JP", 
			50222 => "iso-2022-jp", 
			50225 => "iso-2022-kr", 
			50227 => "x-cp50227", 
			51932 => "euc-jp", 
			51936 => "EUC-CN", 
			51949 => "euc-kr", 
			52936 => "hz-gb-2312", 
			54936 => "GB18030", 
			57002 => "x-iscii-de", 
			57003 => "x-iscii-be", 
			57004 => "x-iscii-ta", 
			57005 => "x-iscii-te", 
			57006 => "x-iscii-as", 
			57007 => "x-iscii-or", 
			57008 => "x-iscii-ka", 
			57009 => "x-iscii-ma", 
			57010 => "x-iscii-gu", 
			57011 => "x-iscii-pa", 
			65000 => "utf-7", 
			65001 => "utf-8", 
			_ => "Windows-1252", 
		};
	}

	private void SetDefaultValue(WParagraphFormat paragraphFormat)
	{
		paragraphFormat.SetPropertyValue(9, 0f);
		paragraphFormat.SetPropertyValue(8, 0f);
		paragraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
		paragraphFormat.SetPropertyValue(2, 0f);
		paragraphFormat.SetPropertyValue(3, 0f);
		paragraphFormat.BackColor = Color.Empty;
		paragraphFormat.ForeColor = Color.Empty;
		paragraphFormat.SetPropertyValue(52, 0f);
		paragraphFormat.LineSpacingRule = LineSpacingRule.AtLeast;
		paragraphFormat.TextureStyle = TextureStyle.TextureNone;
	}

	private void ProcessTableInfo(bool isShapeTextEnd)
	{
		if (m_bInTable && m_currentLevel == 0)
		{
			m_currentLevel = 1;
		}
		PrepareTableInfo prepareTableInfo = new PrepareTableInfo(m_bInTable, m_currentLevel, m_previousLevel);
		WTextBody item = ((m_textBody != null) ? m_textBody : CurrentSection.Body);
		if (prepareTableInfo.InTable && prepareTableInfo.State != PrepareTableState.LeaveTable)
		{
			if (m_currRow == null)
			{
				if (m_currTable == null)
				{
					m_currTable = new WTable(m_document);
				}
				m_currRow = m_currTable.AddRow(isCopyFormat: false, autoPopulateCells: false);
				m_currCell = m_currRow.AddCell(isCopyFormat: false);
				m_textBody = m_currCell;
			}
			else if (m_bCellFinished)
			{
				m_currCell = m_currTable.LastRow.AddCell();
				m_textBody = m_currCell;
			}
		}
		if (m_bCellFinished)
		{
			m_bCellFinished = false;
		}
		switch (prepareTableInfo.State)
		{
		case PrepareTableState.EnterTable:
			if (prepareTableInfo.PrevLevel == 0)
			{
				m_nestedTextBody.Push(item);
			}
			EnsureUpperTable(prepareTableInfo.Level);
			break;
		case PrepareTableState.LeaveTable:
			EnsureLowerTable(prepareTableInfo.Level, isShapeTextEnd);
			break;
		}
	}

	private void EnsureLowerTable(int level, bool isShapeTextEnd)
	{
		if (m_currTable != null)
		{
			float beforeWidth = (m_currTable.ChildEntities[0] as WTableRow).RowFormat.BeforeWidth;
			bool flag = false;
			for (int i = 0; i < m_currTable.ChildEntities.Count; i++)
			{
				float beforeWidth2 = (CurrTable.ChildEntities[i] as WTableRow).RowFormat.BeforeWidth;
				if (beforeWidth == beforeWidth2)
				{
					flag = true;
					continue;
				}
				flag = false;
				break;
			}
			if (flag)
			{
				for (int j = 0; j < m_currTable.ChildEntities.Count; j++)
				{
					(CurrTable.ChildEntities[j] as WTableRow).RowFormat.BeforeWidth = 0f;
				}
			}
		}
		while (m_nestedTable.Count > level)
		{
			m_nestedTable.Pop();
		}
		if (!m_currTable.FirstRow.RowFormat.IsLeftIndentDefined)
		{
			m_currTable.TableFormat.LeftIndent = 0f;
		}
		else if (m_currTable.TableFormat.LeftIndent != m_currTable.FirstRow.RowFormat.LeftIndent)
		{
			m_currTable.TableFormat.LeftIndent = m_currTable.FirstRow.RowFormat.LeftIndent;
		}
		if (level == 0)
		{
			m_textBody = m_nestedTextBody.Pop();
			for (int k = 0; k < m_currTable.Rows.Count; k++)
			{
				for (int l = 0; l < m_currTable.Rows[k].Cells.Count; l++)
				{
					WTableCell wTableCell = m_currTable.Rows[k].Cells[l];
					if (wTableCell.CellFormat.HorizontalMerge == CellMerge.Start && wTableCell.Width < wTableCell.PreferredWidth.Width && wTableCell.NextSibling is WTableCell && (wTableCell.NextSibling as WTableCell).CellFormat.HorizontalMerge == CellMerge.Continue && wTableCell.PreferredWidth.Width == wTableCell.Width + (wTableCell.NextSibling as WTableCell).Width)
					{
						m_currTable.Rows[k].Cells.RemoveAt(l + 1);
					}
				}
			}
			if (m_currTable != null && !isShapeTextEnd)
			{
				if (CurrentSection.PageSetup.Margins.Bottom != m_secFormat.BottomMargin)
				{
					ApplySectionFormatting();
				}
				if (!m_currTable.TableFormat.IsAutoResized && m_currTable.FirstRow.RowFormat.IsAutoResized)
				{
					m_currTable.TableFormat.IsAutoResized = m_currTable.FirstRow.RowFormat.IsAutoResized;
				}
				(m_currTable as WTable).IsTableGridUpdated = false;
				(m_currTable as WTable).UpdateGridSpan();
			}
			m_textBody.Items.Add(m_currTable);
			m_document.SetTriggerElement(ref m_document.m_supportedElementFlag_2, 11);
			m_document.SetTriggerElement(ref m_document.m_supportedElementFlag_2, 10);
			m_document.SetTriggerElement(ref m_document.m_supportedElementFlag_2, 16);
			m_currTable = null;
			m_currRow = null;
			m_currCell = null;
			m_previousLevel = level;
		}
		else
		{
			WTable entity = m_currTable as WTable;
			m_currTable = m_nestedTable.Pop();
			m_currRow = m_currTable.LastRow;
			m_currCell = m_currTable.LastCell;
			m_textBody = m_currTable.LastCell;
			m_textBody.Items.Add(entity);
		}
		if (m_currTable != null && !isShapeTextEnd)
		{
			(m_currTable as WTable).UpdateGridSpan();
		}
	}

	private void EnsureUpperTable(int level)
	{
		while (m_nestedTable.Count < level - 1)
		{
			if (m_currTable != null)
			{
				m_nestedTable.Push(m_currTable as WTable);
			}
			m_currTable = new WTable(m_document);
			m_currRow = m_currTable.AddRow(isCopyFormat: false, autoPopulateCells: false);
			m_currCell = m_currRow.AddCell(isCopyFormat: false);
			m_textBody = m_currCell;
		}
	}

	private void ApplyListFormatting(string token, string tokenKey, string tokenValue, WListFormat listFormat)
	{
		if (m_document.ListStyleNames.ContainsKey(token))
		{
			string text = m_document.ListStyleNames[token];
			m_document.ListStyles.FindByName(text);
			if (text != null)
			{
				listFormat.ApplyStyle(text);
				listFormat.ListLevelNumber = 0;
			}
		}
	}

	private void ApplySectionFormatting()
	{
		if (m_secFormat.BottomMargin >= 0f)
		{
			CurrentSection.PageSetup.Margins.Bottom = m_secFormat.BottomMargin;
		}
		if (m_secFormat.LeftMargin >= 0f)
		{
			CurrentSection.PageSetup.Margins.Left = m_secFormat.LeftMargin;
		}
		if (m_secFormat.RightMargin >= 0f)
		{
			CurrentSection.PageSetup.Margins.Right = m_secFormat.RightMargin;
		}
		if (m_secFormat.TopMargin >= 0f)
		{
			CurrentSection.PageSetup.Margins.Top = m_secFormat.TopMargin;
		}
		if (m_secFormat.pageSize.Width > 0f && m_secFormat.pageSize.Height > 0f)
		{
			CurrentSection.PageSetup.PageSize = m_secFormat.pageSize;
		}
		CurrentSection.PageSetup.SetPageSetupProperty("HeaderDistance", m_secFormat.HeaderDistance);
		CurrentSection.PageSetup.SetPageSetupProperty("FooterDistance", m_secFormat.FooterDistance);
		m_document.DefaultTabWidth = m_secFormat.DefaultTabWidth;
		CurrentSection.PageSetup.VerticalAlignment = m_secFormat.VertAlignment;
		if (m_secFormat.m_pageOrientation != 0)
		{
			CurrentSection.PageSetup.Orientation = m_secFormat.m_pageOrientation;
		}
		m_document.DifferentOddAndEvenPages = m_secFormat.DifferentOddAndEvenPage;
		if (m_secFormat.FirstPageTray > 0)
		{
			CurrentSection.PageSetup.FirstPageTray = (PrinterPaperTray)m_secFormat.FirstPageTray;
		}
		if (m_secFormat.OtherPagesTray > 0)
		{
			CurrentSection.PageSetup.OtherPagesTray = (PrinterPaperTray)m_secFormat.OtherPagesTray;
		}
	}

	private void ParseShapeToken(string token, string tokenKey, string tokenValue)
	{
		if (!m_bIsShapePicture || m_bIsGroupShape)
		{
			return;
		}
		switch (tokenKey)
		{
		case "posh":
			switch (tokenValue)
			{
			case "1":
				if (m_currPicture != null)
				{
					m_currPicture.HorizontalAlignment = ShapeHorizontalAlignment.Left;
				}
				else if (m_currShape != null)
				{
					m_currShape.HorizontalAlignment = ShapeHorizontalAlignment.Left;
				}
				else
				{
					m_currTextBox.TextBoxFormat.HorizontalAlignment = ShapeHorizontalAlignment.Left;
				}
				break;
			case "2":
				if (m_currPicture != null)
				{
					m_currPicture.HorizontalAlignment = ShapeHorizontalAlignment.Center;
				}
				else if (m_currShape != null)
				{
					m_currShape.HorizontalAlignment = ShapeHorizontalAlignment.Center;
				}
				else
				{
					m_currTextBox.TextBoxFormat.HorizontalAlignment = ShapeHorizontalAlignment.Center;
				}
				break;
			case "3":
				if (m_currPicture != null)
				{
					m_currPicture.HorizontalAlignment = ShapeHorizontalAlignment.Right;
				}
				else if (m_currShape != null)
				{
					m_currShape.HorizontalAlignment = ShapeHorizontalAlignment.Right;
				}
				else
				{
					m_currTextBox.TextBoxFormat.HorizontalAlignment = ShapeHorizontalAlignment.Right;
				}
				break;
			case "4":
				if (m_currPicture != null)
				{
					m_currPicture.HorizontalAlignment = ShapeHorizontalAlignment.Inside;
				}
				else if (m_currShape != null)
				{
					m_currShape.HorizontalAlignment = ShapeHorizontalAlignment.Inside;
				}
				else
				{
					m_currTextBox.TextBoxFormat.HorizontalAlignment = ShapeHorizontalAlignment.Inside;
				}
				break;
			case "5":
				if (m_currPicture != null)
				{
					m_currPicture.HorizontalAlignment = ShapeHorizontalAlignment.Outside;
				}
				else if (m_currShape != null)
				{
					m_currShape.HorizontalAlignment = ShapeHorizontalAlignment.Outside;
				}
				else
				{
					m_currTextBox.TextBoxFormat.HorizontalAlignment = ShapeHorizontalAlignment.Outside;
				}
				break;
			}
			break;
		case "posv":
			switch (tokenValue)
			{
			case "1":
				if (m_currPicture != null)
				{
					m_currPicture.VerticalAlignment = ShapeVerticalAlignment.Top;
				}
				else if (m_currShape != null)
				{
					m_currShape.VerticalAlignment = ShapeVerticalAlignment.Top;
				}
				else
				{
					m_currTextBox.TextBoxFormat.VerticalAlignment = ShapeVerticalAlignment.Top;
				}
				break;
			case "2":
				if (m_currPicture != null)
				{
					m_currPicture.VerticalAlignment = ShapeVerticalAlignment.Center;
				}
				else if (m_currShape != null)
				{
					m_currShape.VerticalAlignment = ShapeVerticalAlignment.Center;
				}
				else
				{
					m_currTextBox.TextBoxFormat.VerticalAlignment = ShapeVerticalAlignment.Center;
				}
				break;
			case "3":
				if (m_currPicture != null)
				{
					m_currPicture.VerticalAlignment = ShapeVerticalAlignment.Bottom;
				}
				else if (m_currShape != null)
				{
					m_currShape.VerticalAlignment = ShapeVerticalAlignment.Bottom;
				}
				else
				{
					m_currTextBox.TextBoxFormat.VerticalAlignment = ShapeVerticalAlignment.Bottom;
				}
				break;
			case "4":
				if (m_currPicture != null)
				{
					m_currPicture.VerticalAlignment = ShapeVerticalAlignment.Inside;
				}
				else if (m_currShape != null)
				{
					m_currShape.VerticalAlignment = ShapeVerticalAlignment.Inside;
				}
				else
				{
					m_currTextBox.TextBoxFormat.VerticalAlignment = ShapeVerticalAlignment.Inside;
				}
				break;
			case "5":
				if (m_currPicture != null)
				{
					m_currPicture.VerticalAlignment = ShapeVerticalAlignment.Outside;
				}
				else if (m_currShape != null)
				{
					m_currShape.VerticalAlignment = ShapeVerticalAlignment.Outside;
				}
				else
				{
					m_currTextBox.TextBoxFormat.VerticalAlignment = ShapeVerticalAlignment.Outside;
				}
				break;
			}
			break;
		case "posrelh":
		{
			if (tokenValue == null)
			{
				break;
			}
			int i = tokenValue.Length;
			if (i != 1)
			{
				break;
			}
			switch (tokenValue[0])
			{
			case '0':
				if (m_currPicture != null)
				{
					m_currPicture.HorizontalOrigin = HorizontalOrigin.Margin;
				}
				else if (m_currShape != null)
				{
					m_currShape.HorizontalOrigin = HorizontalOrigin.Margin;
				}
				else
				{
					m_currTextBox.TextBoxFormat.HorizontalOrigin = HorizontalOrigin.Margin;
				}
				break;
			case '1':
				if (m_currPicture != null)
				{
					m_currPicture.HorizontalOrigin = HorizontalOrigin.Page;
				}
				else if (m_currShape != null)
				{
					m_currShape.HorizontalOrigin = HorizontalOrigin.Page;
				}
				else
				{
					m_currTextBox.TextBoxFormat.HorizontalOrigin = HorizontalOrigin.Page;
				}
				break;
			case '2':
				if (m_currPicture != null)
				{
					m_currPicture.HorizontalOrigin = HorizontalOrigin.Column;
				}
				else if (m_currShape != null)
				{
					m_currShape.HorizontalOrigin = HorizontalOrigin.Column;
				}
				else
				{
					m_currTextBox.TextBoxFormat.HorizontalOrigin = HorizontalOrigin.Column;
				}
				break;
			case '3':
				if (m_currPicture != null)
				{
					m_currPicture.HorizontalOrigin = HorizontalOrigin.Character;
				}
				else if (m_currShape != null)
				{
					m_currShape.HorizontalOrigin = HorizontalOrigin.Character;
				}
				else
				{
					m_currTextBox.TextBoxFormat.HorizontalOrigin = HorizontalOrigin.Character;
				}
				break;
			case '4':
				if (m_currPicture != null)
				{
					m_currPicture.HorizontalOrigin = HorizontalOrigin.LeftMargin;
				}
				else if (m_currShape != null)
				{
					m_currShape.HorizontalOrigin = HorizontalOrigin.LeftMargin;
				}
				else
				{
					m_currTextBox.TextBoxFormat.HorizontalOrigin = HorizontalOrigin.LeftMargin;
				}
				break;
			case '5':
				if (m_currPicture != null)
				{
					m_currPicture.HorizontalOrigin = HorizontalOrigin.RightMargin;
				}
				else if (m_currShape != null)
				{
					m_currShape.HorizontalOrigin = HorizontalOrigin.RightMargin;
				}
				else
				{
					m_currTextBox.TextBoxFormat.HorizontalOrigin = HorizontalOrigin.RightMargin;
				}
				break;
			case '6':
				if (m_currPicture != null)
				{
					m_currPicture.HorizontalOrigin = HorizontalOrigin.InsideMargin;
				}
				else if (m_currShape != null)
				{
					m_currShape.HorizontalOrigin = HorizontalOrigin.InsideMargin;
				}
				else
				{
					m_currTextBox.TextBoxFormat.HorizontalOrigin = HorizontalOrigin.InsideMargin;
				}
				break;
			case '7':
				if (m_currPicture != null)
				{
					m_currPicture.HorizontalOrigin = HorizontalOrigin.OutsideMargin;
				}
				else if (m_currShape != null)
				{
					m_currShape.HorizontalOrigin = HorizontalOrigin.OutsideMargin;
				}
				else
				{
					m_currTextBox.TextBoxFormat.HorizontalOrigin = HorizontalOrigin.OutsideMargin;
				}
				break;
			}
			break;
		}
		case "posrelv":
		{
			if (tokenValue == null)
			{
				break;
			}
			int i = tokenValue.Length;
			if (i != 1)
			{
				break;
			}
			switch (tokenValue[0])
			{
			case '0':
				if (m_currPicture != null)
				{
					m_currPicture.VerticalOrigin = VerticalOrigin.Margin;
				}
				else if (m_currShape != null)
				{
					m_currShape.VerticalOrigin = VerticalOrigin.Margin;
				}
				else
				{
					m_currTextBox.TextBoxFormat.VerticalOrigin = VerticalOrigin.Margin;
				}
				break;
			case '1':
				if (m_currPicture != null)
				{
					m_currPicture.VerticalOrigin = VerticalOrigin.Page;
				}
				else if (m_currShape != null)
				{
					m_currShape.VerticalOrigin = VerticalOrigin.Page;
				}
				else
				{
					m_currTextBox.TextBoxFormat.VerticalOrigin = VerticalOrigin.Page;
				}
				break;
			case '2':
				if (m_currPicture != null)
				{
					m_currPicture.VerticalOrigin = VerticalOrigin.Paragraph;
				}
				else if (m_currShape != null)
				{
					m_currShape.VerticalOrigin = VerticalOrigin.Paragraph;
				}
				else
				{
					m_currTextBox.TextBoxFormat.VerticalOrigin = VerticalOrigin.Paragraph;
				}
				break;
			case '3':
				if (m_currPicture != null)
				{
					m_currPicture.VerticalOrigin = VerticalOrigin.Line;
				}
				else if (m_currShape != null)
				{
					m_currShape.VerticalOrigin = VerticalOrigin.Line;
				}
				else
				{
					m_currTextBox.TextBoxFormat.VerticalOrigin = VerticalOrigin.Line;
				}
				break;
			case '4':
				if (m_currPicture != null)
				{
					m_currPicture.VerticalOrigin = VerticalOrigin.TopMargin;
				}
				else if (m_currShape != null)
				{
					m_currShape.VerticalOrigin = VerticalOrigin.TopMargin;
				}
				else
				{
					m_currTextBox.TextBoxFormat.VerticalOrigin = VerticalOrigin.TopMargin;
				}
				break;
			case '5':
				if (m_currPicture != null)
				{
					m_currPicture.VerticalOrigin = VerticalOrigin.BottomMargin;
				}
				else if (m_currShape != null)
				{
					m_currShape.VerticalOrigin = VerticalOrigin.BottomMargin;
				}
				else
				{
					m_currTextBox.TextBoxFormat.VerticalOrigin = VerticalOrigin.BottomMargin;
				}
				break;
			case '6':
				if (m_currPicture != null)
				{
					m_currPicture.VerticalOrigin = VerticalOrigin.InsideMargin;
				}
				else if (m_currShape != null)
				{
					m_currShape.VerticalOrigin = VerticalOrigin.InsideMargin;
				}
				else
				{
					m_currTextBox.TextBoxFormat.VerticalOrigin = VerticalOrigin.InsideMargin;
				}
				break;
			case '7':
				if (m_currPicture != null)
				{
					m_currPicture.VerticalOrigin = VerticalOrigin.OutsideMargin;
				}
				else if (m_currShape != null)
				{
					m_currShape.VerticalOrigin = VerticalOrigin.OutsideMargin;
				}
				else
				{
					m_currTextBox.TextBoxFormat.VerticalOrigin = VerticalOrigin.OutsideMargin;
				}
				break;
			}
			break;
		}
		case "fLayoutInCell":
			if (tokenValue == "1")
			{
				if (m_currPicture != null)
				{
					(m_currPicture as WPicture).LayoutInCell = true;
				}
				else if (m_currShape != null)
				{
					m_currShape.LayoutInCell = true;
				}
				else
				{
					m_currTextBox.TextBoxFormat.AllowInCell = true;
				}
			}
			else if (m_currPicture != null)
			{
				(m_currPicture as WPicture).LayoutInCell = false;
			}
			else if (m_currShape != null)
			{
				m_currShape.LayoutInCell = false;
			}
			else
			{
				m_currTextBox.TextBoxFormat.AllowInCell = false;
			}
			break;
		case "shprslt":
			m_bIsShapeResult = true;
			m_bShapeResultStackCount = m_pictureOrShapeStack.Count - 1;
			m_bIsShapePicture = true;
			break;
		case "nonshppict":
			m_bIsShapePicture = false;
			break;
		case "shppict":
			m_bIsShapePicture = true;
			break;
		case "shpwr":
			switch (Convert.ToInt32(tokenValue))
			{
			case 1:
				m_currShapeFormat.m_textWrappingStyle = TextWrappingStyle.TopAndBottom;
				break;
			case 2:
				m_currShapeFormat.m_textWrappingStyle = TextWrappingStyle.Square;
				break;
			case 3:
				m_currShapeFormat.m_textWrappingStyle = TextWrappingStyle.InFrontOfText;
				break;
			case 4:
				m_currShapeFormat.m_textWrappingStyle = TextWrappingStyle.Tight;
				break;
			case 5:
				m_currShapeFormat.m_textWrappingStyle = TextWrappingStyle.Through;
				break;
			case 6:
				m_currShapeFormat.m_textWrappingStyle = TextWrappingStyle.Behind;
				break;
			}
			break;
		case "shpwrk":
			switch (Convert.ToInt32(tokenValue))
			{
			case 0:
				m_currShapeFormat.m_textWrappingType = TextWrappingType.Both;
				break;
			case 1:
				m_currShapeFormat.m_textWrappingType = TextWrappingType.Left;
				break;
			case 2:
				m_currShapeFormat.m_textWrappingType = TextWrappingType.Right;
				break;
			case 3:
				m_currShapeFormat.m_textWrappingType = TextWrappingType.Largest;
				break;
			}
			break;
		case "shpbypara":
			m_currShapeFormat.m_vertOrgin = VerticalOrigin.Paragraph;
			break;
		case "shpbymargin":
			m_currShapeFormat.m_vertOrgin = VerticalOrigin.Margin;
			break;
		case "shpbypage":
			m_currShapeFormat.m_vertOrgin = VerticalOrigin.Page;
			break;
		case "shpbxpage":
			m_currShapeFormat.m_horizOrgin = HorizontalOrigin.Page;
			break;
		case "shpbxmargin":
			m_currShapeFormat.m_horizOrgin = HorizontalOrigin.Margin;
			break;
		case "shpbxcolumn":
			m_currShapeFormat.m_horizOrgin = HorizontalOrigin.Column;
			break;
		case "shpleft":
			m_currShapeFormat.m_horizPosition = ExtractTwipsValue(tokenValue);
			m_currShapeFormat.m_left = GetIntValue(tokenValue);
			break;
		case "shpleft-":
		{
			float num4 = ExtractTwipsValue(tokenValue);
			m_currShapeFormat.m_horizPosition = 0f - num4;
			num4 = GetIntValue(tokenValue);
			m_currShapeFormat.m_left = 0f - num4;
			break;
		}
		case "shptop":
			m_currShapeFormat.m_vertPosition = ExtractTwipsValue(tokenValue);
			m_currShapeFormat.m_top = GetIntValue(tokenValue);
			break;
		case "shptop-":
		{
			float num3 = ExtractTwipsValue(tokenValue);
			m_currShapeFormat.m_vertPosition = 0f - num3;
			num3 = GetIntValue(tokenValue);
			m_currShapeFormat.m_top = 0f - num3;
			break;
		}
		case "shpright":
			m_currShapeFormat.m_right = GetIntValue(tokenValue);
			break;
		case "shpright-":
		{
			float num2 = GetIntValue(tokenValue);
			m_currShapeFormat.m_right = 0f - num2;
			break;
		}
		case "shpbottom":
			m_currShapeFormat.m_bottom = GetIntValue(tokenValue);
			break;
		case "shpbottom-":
		{
			float num = GetIntValue(tokenValue);
			m_currShapeFormat.m_bottom = 0f - num;
			break;
		}
		case "shpfblwtxt":
			if (Convert.ToInt32(tokenValue) == 1)
			{
				m_currShapeFormat.m_isBelowText = true;
				if (m_currShapeFormat.m_textWrappingStyle == TextWrappingStyle.Inline)
				{
					m_currShapeFormat.m_textWrappingStyle = TextWrappingStyle.Behind;
				}
			}
			else
			{
				m_currShapeFormat.m_isBelowText = false;
				if (m_currShapeFormat.m_textWrappingStyle == TextWrappingStyle.Inline)
				{
					m_currShapeFormat.m_textWrappingStyle = TextWrappingStyle.InFrontOfText;
				}
			}
			break;
		case "shpfhdr":
			if (Convert.ToInt32(tokenValue) == 1)
			{
				m_currShapeFormat.m_isInHeader = true;
			}
			else
			{
				m_currShapeFormat.m_isInHeader = false;
			}
			break;
		case "shplockanchor":
			m_currShapeFormat.m_isLockAnchor = true;
			break;
		case "shpz":
			m_currShapeFormat.m_zOrder = GetIntValue(tokenValue);
			m_picFormat.Zorder = GetIntValue(tokenValue);
			break;
		case "shplid":
			m_currShapeFormat.m_uniqueId = GetIntValue(tokenValue);
			if (m_document.maxShapeId < m_currShapeFormat.m_uniqueId)
			{
				m_document.maxShapeId = m_currShapeFormat.m_uniqueId;
			}
			break;
		case "shpinst":
			m_bIsShapeInstruction = true;
			m_shapeInstructionStack.Push("{");
			break;
		case "pWrapPolygonVertices":
		{
			if (m_currPicture != null)
			{
				(m_currPicture as WPicture).WrapPolygon.Vertices.Clear();
			}
			else if (m_currShape != null)
			{
				m_currShape.WrapFormat.WrapPolygon.Vertices.Clear();
			}
			else
			{
				m_currTextBox.TextBoxFormat.WrapPolygon.Vertices.Clear();
			}
			string[] array = m_drawingFieldValue.Split('(', ')');
			foreach (string text in array)
			{
				if (text.Contains(","))
				{
					string[] array2 = text.Split(',');
					float x = float.Parse(array2[0], CultureInfo.InvariantCulture);
					float y = float.Parse(array2[1], CultureInfo.InvariantCulture);
					if (m_currPicture != null)
					{
						(m_currPicture as WPicture).WrapPolygon.Vertices.Add(new PointF(x, y));
					}
					else if (m_currShape != null)
					{
						m_currShape.WrapFormat.WrapPolygon.Vertices.Add(new PointF(x, y));
					}
					else
					{
						m_currTextBox.TextBoxFormat.WrapPolygon.Vertices.Add(new PointF(x, y));
					}
				}
			}
			m_drawingFieldValue = string.Empty;
			break;
		}
		case "object":
			m_bIsObject = true;
			m_objectStack.Push("\\");
			break;
		case "hlfr":
		case "hlsrc":
			m_isImageHyperlink = true;
			break;
		case "fAllowOverlap":
			if (tokenValue == "1")
			{
				if (m_currPicture != null)
				{
					(m_currPicture as WPicture).AllowOverlap = true;
				}
				else if (m_currShape != null)
				{
					m_currShape.WrapFormat.AllowOverlap = true;
				}
				else
				{
					m_currTextBox.TextBoxFormat.AllowOverlap = true;
				}
			}
			else if (m_currPicture != null)
			{
				(m_currPicture as WPicture).AllowOverlap = false;
			}
			else if (m_currShape != null)
			{
				m_currShape.WrapFormat.AllowOverlap = false;
			}
			else
			{
				m_currTextBox.TextBoxFormat.AllowOverlap = false;
			}
			break;
		case "shptxt":
			AddShapeTextbodyStack();
			ParseRowStart(isFromShape: true);
			ClearPreviousTextbody();
			m_bIsShapeText = true;
			m_bIsPictureOrShape = false;
			m_shapeTextStack.Push("{");
			break;
		default:
			ParseShapeToken(token, tokenValue);
			break;
		}
	}

	private void ParseShapeToken(string token, string tokenValue)
	{
		if ((token.ToLower().Contains("shadow") || token.Contains("3D")) && ((m_currShape != null && m_currShape.EffectList.Count == 0) || (m_currTextBox != null && m_currTextBox.Shape != null && m_currTextBox.Shape.EffectList.Count == 0)))
		{
			EffectFormat effectFormat = new EffectFormat(m_currShape);
			effectFormat.IsEffectListItem = true;
			if (tokenValue != "0")
			{
				effectFormat.IsShadowEffect = true;
			}
			effectFormat.ShadowFormat.m_type = "outerShdw";
			effectFormat.ShadowFormat.ShadowOffset2X = 0f;
			effectFormat.ShadowFormat.ShadowOffset2Y = 0f;
			effectFormat.ShadowFormat.Color = Color.FromArgb(255, 128, 128, 128);
			if (token.Contains("3D"))
			{
				effectFormat.IsShadowEffect = false;
			}
			if (m_currShape != null)
			{
				m_currShape.EffectList.Add(effectFormat);
			}
			else if (m_currTextBox.Shape != null)
			{
				m_currTextBox.Shape.EffectList.Add(effectFormat);
			}
		}
		if (token.Contains("3D") && ((m_currShape != null && m_currShape.EffectList.Count == 1) || (m_currTextBox != null && m_currTextBox.Shape != null && m_currTextBox.Shape.EffectList.Count == 1)))
		{
			Shape shape = ((m_currShape == null) ? m_currTextBox.Shape : m_currShape);
			EffectFormat effectFormat2 = new EffectFormat(shape);
			effectFormat2.IsEffectListItem = true;
			effectFormat2.IsShapeProperties = true;
			effectFormat2.IsSceneProperties = true;
			if (tokenValue != "0")
			{
				shape.IsShapePropertiesInline = true;
				shape.IsScenePropertiesInline = true;
			}
			shape.EffectList.Add(effectFormat2);
		}
		if ((m_currShape == null && m_currTextBox == null) || token == null)
		{
			return;
		}
		switch (token.Length)
		{
		case 13:
			switch (token[7])
			{
			default:
				return;
			case 'I':
				if (token == "fPseudoInline" && tokenValue == "1")
				{
					if (m_currShape != null)
					{
						m_currShape.WrapFormat.TextWrappingStyle = TextWrappingStyle.Inline;
					}
					else
					{
						m_currTextBox.TextBoxFormat.TextWrappingStyle = TextWrappingStyle.Inline;
					}
				}
				return;
			case 'i':
				if (token == "dyWrapDistTop")
				{
					double.TryParse(tokenValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var result8);
					result8 = Math.Round(result8 / 12700.0, 2);
					if (m_currShape != null)
					{
						m_currShape.WrapFormat.DistanceTop = (float)result8;
					}
					else
					{
						m_currTextBox.TextBoxFormat.WrapDistanceTop = (float)result8;
					}
				}
				return;
			case 'k':
				if (!(token == "fillBackColor"))
				{
					if (token == "lineBackColor")
					{
						if (m_currShape != null)
						{
							m_currShape.LineFormat.ForeColor = Color.FromArgb(GetIntValue(tokenValue));
						}
						else
						{
							m_currTextBox.Shape.LineFormat.ForeColor = Color.FromArgb(GetIntValue(tokenValue));
						}
					}
				}
				else if (m_currShape != null)
				{
					m_currShape.FillFormat.ForeColor = Color.FromArgb(GetIntValue(tokenValue));
				}
				else
				{
					m_currTextBox.Shape.FillFormat.ForeColor = Color.FromArgb(GetIntValue(tokenValue));
				}
				return;
			case 'n':
				if (!(token == "lineJoinStyle"))
				{
					return;
				}
				switch (tokenValue)
				{
				case "0":
					if (m_currShape != null)
					{
						m_currShape.LineFormat.LineJoin = DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.LineJoin.Bevel;
					}
					else
					{
						m_currTextBox.Shape.LineFormat.LineJoin = DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.LineJoin.Bevel;
					}
					break;
				case "1":
					if (m_currShape != null)
					{
						m_currShape.LineFormat.LineJoin = DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.LineJoin.Miter;
					}
					else
					{
						m_currTextBox.Shape.LineFormat.LineJoin = DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.LineJoin.Miter;
					}
					break;
				case "2":
					if (m_currShape != null)
					{
						m_currShape.LineFormat.LineJoin = DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.LineJoin.Round;
					}
					else
					{
						m_currTextBox.Shape.LineFormat.LineJoin = DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.LineJoin.Round;
					}
					break;
				}
				return;
			case 'p':
				if (token == "shadowOpacity")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[0].ShadowFormat.Transparency = 0.5f;
					}
					else
					{
						m_currTextBox.Shape.EffectList[0].ShadowFormat.Transparency = 0.5f;
					}
				}
				return;
			case 'f':
				if (!(token == "shadowOffsetX"))
				{
					if (token == "shadowOffsetY")
					{
						if (m_currShape != null)
						{
							m_currShape.EffectList[0].ShadowFormat.ShadowOffsetY = GetIntValue(tokenValue);
						}
						else
						{
							m_currTextBox.Shape.EffectList[0].ShadowFormat.ShadowOffsetY = GetIntValue(tokenValue);
						}
					}
				}
				else if (m_currShape != null)
				{
					m_currShape.EffectList[0].ShadowFormat.ShadowOffsetX = GetIntValue(tokenValue);
				}
				else
				{
					m_currTextBox.Shape.EffectList[0].ShadowFormat.ShadowOffsetX = GetIntValue(tokenValue);
				}
				return;
			case 'r':
				if (!(token == "shadowOriginX"))
				{
					if (token == "shadowOriginY")
					{
						if (m_currShape != null)
						{
							m_currShape.EffectList[0].ShadowFormat.OriginY = GetIntValue(tokenValue);
						}
						else
						{
							m_currTextBox.Shape.EffectList[0].ShadowFormat.OriginY = GetIntValue(tokenValue);
						}
					}
				}
				else if (m_currShape != null)
				{
					m_currShape.EffectList[0].ShadowFormat.OriginX = GetIntValue(tokenValue);
				}
				else
				{
					m_currTextBox.Shape.EffectList[0].ShadowFormat.OriginX = GetIntValue(tokenValue);
				}
				return;
			case 't':
				if (token == "fillRectRight")
				{
					float result3 = 0f;
					float.TryParse(tokenValue, NumberStyles.Number, CultureInfo.InvariantCulture, out result3);
					if (m_currShape != null)
					{
						m_currShape.FillFormat.FillRectangle.RightOffset = result3;
					}
					else
					{
						m_currTextBox.Shape.FillFormat.FillRectangle.RightOffset = result3;
					}
				}
				return;
			case 'u':
				if (token == "c3DDiffuseAmt")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.Diffusity = GetIntValue(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.Diffusity = GetIntValue(tokenValue);
					}
				}
				return;
			case 'h':
				if (token == "fc3DLightFace")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.HasLightRigEffect = GetIntValue(tokenValue) == 1;
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.HasLightRigEffect = GetIntValue(tokenValue) == 1;
					}
				}
				return;
			case 'e':
				if (!(token == "c3DRenderMode"))
				{
					return;
				}
				switch (tokenValue)
				{
				case "0":
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.ExtrusionRenderMode = ExtrusionRenderMode.Solid;
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.ExtrusionRenderMode = ExtrusionRenderMode.Solid;
					}
					break;
				case "1":
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.ExtrusionRenderMode = ExtrusionRenderMode.Wireframe;
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.ExtrusionRenderMode = ExtrusionRenderMode.Wireframe;
					}
					break;
				case "2":
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.ExtrusionRenderMode = ExtrusionRenderMode.BoundingCube;
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.ExtrusionRenderMode = ExtrusionRenderMode.BoundingCube;
					}
					break;
				}
				return;
			case 'w':
				switch (token)
				{
				case "c3DXViewpoint":
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.ViewPointX = float.Parse(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.ViewPointX = float.Parse(tokenValue);
					}
					break;
				case "c3DYViewpoint":
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.ViewPointY = float.Parse(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.ViewPointY = float.Parse(tokenValue);
					}
					break;
				case "c3DZViewpoint":
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.ViewPointZ = float.Parse(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.ViewPointZ = float.Parse(tokenValue);
					}
					break;
				}
				return;
			case 'A':
				if (token == "c3DSkewAmount")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.SkewAmount = float.Parse(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.SkewAmount = float.Parse(tokenValue);
					}
				}
				return;
			case 'l':
				if (token == "fc3DFillHarsh")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.LightHarsh2 = GetIntValue(tokenValue) == 1;
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.LightHarsh2 = GetIntValue(tokenValue) == 1;
					}
				}
				return;
			case '0':
				break;
			}
			if (!(token == "adjust10Value"))
			{
				break;
			}
			goto IL_4d05;
		case 8:
			switch (token[7])
			{
			case 'n':
				if (token == "rotation")
				{
					SetRotationValue(tokenValue);
				}
				break;
			case 'z':
			{
				if (!(token == "pctHoriz"))
				{
					break;
				}
				float result7 = float.MaxValue;
				float.TryParse(tokenValue, NumberStyles.Number, CultureInfo.InvariantCulture, out result7);
				if (result7 != float.MaxValue)
				{
					if (m_currShape != null)
					{
						m_currShape.IsRelativeWidth = true;
						m_currShape.RelativeWidth = (float)Math.Round(result7 / 10f, 2);
						m_currShape.RelativeWidthHorizontalOrigin = HorizontalOrigin.Page;
					}
					else
					{
						m_currTextBox.TextBoxFormat.WidthRelativePercent = (float)Math.Round(result7 / 10f, 2);
						m_currTextBox.TextBoxFormat.WidthOrigin = WidthOrigin.Page;
					}
				}
				break;
			}
			case 'h':
				if (!(token == "sizerelh"))
				{
					break;
				}
				if (m_currShape != null)
				{
					m_currShape.IsRelativeWidth = true;
				}
				switch (tokenValue)
				{
				case "0":
					if (m_currShape != null)
					{
						m_currShape.RelativeWidthHorizontalOrigin = HorizontalOrigin.Margin;
					}
					else
					{
						m_currTextBox.TextBoxFormat.WidthOrigin = WidthOrigin.Margin;
					}
					break;
				case "1":
					if (m_currShape != null)
					{
						m_currShape.RelativeWidthHorizontalOrigin = HorizontalOrigin.Page;
					}
					else
					{
						m_currTextBox.TextBoxFormat.WidthOrigin = WidthOrigin.Margin;
					}
					break;
				case "2":
					if (m_currShape != null)
					{
						m_currShape.RelativeWidthHorizontalOrigin = HorizontalOrigin.LeftMargin;
					}
					else
					{
						m_currTextBox.TextBoxFormat.WidthOrigin = WidthOrigin.LeftMargin;
					}
					break;
				case "3":
					if (m_currShape != null)
					{
						m_currShape.RelativeWidthHorizontalOrigin = HorizontalOrigin.RightMargin;
					}
					else
					{
						m_currTextBox.TextBoxFormat.WidthOrigin = WidthOrigin.RightMargin;
					}
					break;
				case "4":
					if (m_currShape != null)
					{
						m_currShape.RelativeWidthHorizontalOrigin = HorizontalOrigin.InsideMargin;
					}
					else
					{
						m_currTextBox.TextBoxFormat.WidthOrigin = WidthOrigin.InsideMargin;
					}
					break;
				case "5":
					if (m_currShape != null)
					{
						m_currShape.RelativeWidthHorizontalOrigin = HorizontalOrigin.OutsideMargin;
					}
					else
					{
						m_currTextBox.TextBoxFormat.WidthOrigin = WidthOrigin.OutsideMargin;
					}
					break;
				}
				break;
			case 'v':
				if (!(token == "sizerelv"))
				{
					break;
				}
				if (m_currShape != null)
				{
					m_currShape.IsRelativeHeight = true;
				}
				switch (tokenValue)
				{
				case "0":
					if (m_currShape != null)
					{
						m_currShape.RelativeHeightVerticalOrigin = VerticalOrigin.Margin;
					}
					else
					{
						m_currTextBox.TextBoxFormat.HeightOrigin = HeightOrigin.Margin;
					}
					break;
				case "1":
					if (m_currShape != null)
					{
						m_currShape.RelativeHeightVerticalOrigin = VerticalOrigin.Page;
					}
					else
					{
						m_currTextBox.TextBoxFormat.HeightOrigin = HeightOrigin.Page;
					}
					break;
				case "2":
					if (m_currShape != null)
					{
						m_currShape.RelativeHeightVerticalOrigin = VerticalOrigin.TopMargin;
					}
					else
					{
						m_currTextBox.TextBoxFormat.HeightOrigin = HeightOrigin.TopMargin;
					}
					break;
				case "3":
					if (m_currShape != null)
					{
						m_currShape.RelativeHeightVerticalOrigin = VerticalOrigin.BottomMargin;
					}
					else
					{
						m_currTextBox.TextBoxFormat.HeightOrigin = HeightOrigin.BottomMargin;
					}
					break;
				case "4":
					if (m_currShape != null)
					{
						m_currShape.RelativeHeightVerticalOrigin = VerticalOrigin.InsideMargin;
					}
					else
					{
						m_currTextBox.TextBoxFormat.HeightOrigin = HeightOrigin.InsideMargin;
					}
					break;
				case "5":
					if (m_currShape != null)
					{
						m_currShape.RelativeHeightVerticalOrigin = VerticalOrigin.OutsideMargin;
					}
					else
					{
						m_currTextBox.TextBoxFormat.HeightOrigin = HeightOrigin.OutsideMargin;
					}
					break;
				}
				break;
			case 'e':
				if (!(token == "fillType"))
				{
					if (!(token == "lineType"))
					{
						break;
					}
					switch (tokenValue)
					{
					case "0":
						if (m_currShape != null)
						{
							m_currShape.LineFormat.LineFormatType = LineFormatType.Solid;
						}
						else
						{
							m_currTextBox.Shape.LineFormat.LineFormatType = LineFormatType.Solid;
						}
						break;
					case "1":
						if (m_currShape != null)
						{
							m_currShape.LineFormat.LineFormatType = LineFormatType.Patterned;
						}
						else
						{
							m_currTextBox.Shape.LineFormat.LineFormatType = LineFormatType.Patterned;
						}
						break;
					case "2":
						if (m_currShape != null)
						{
							m_currShape.LineFormat.LineFormatType = LineFormatType.Gradient;
						}
						else
						{
							m_currTextBox.Shape.LineFormat.LineFormatType = LineFormatType.Gradient;
						}
						break;
					case "3":
						if (m_currShape != null)
						{
							m_currShape.LineFormat.LineFormatType = LineFormatType.Gradient;
						}
						else
						{
							m_currTextBox.Shape.LineFormat.LineFormatType = LineFormatType.Gradient;
						}
						break;
					}
					break;
				}
				if (m_currShape != null)
				{
					m_currShape.FillFormat.Fill = true;
				}
				else
				{
					m_currTextBox.Shape.FillFormat.Fill = true;
				}
				switch (tokenValue)
				{
				case "0":
					if (m_currShape != null)
					{
						m_currShape.FillFormat.FillType = FillType.FillSolid;
					}
					else
					{
						m_currTextBox.Shape.FillFormat.FillType = FillType.FillSolid;
					}
					break;
				case "1":
					if (m_currShape != null)
					{
						m_currShape.FillFormat.FillType = FillType.FillPatterned;
					}
					else
					{
						m_currTextBox.Shape.FillFormat.FillType = FillType.FillPatterned;
					}
					break;
				case "2":
					if (m_currShape != null)
					{
						m_currShape.FillFormat.FillType = FillType.FillTextured;
					}
					else
					{
						m_currTextBox.Shape.FillFormat.FillType = FillType.FillTextured;
					}
					break;
				case "3":
					if (m_currShape != null)
					{
						m_currShape.FillFormat.FillType = FillType.FillPicture;
					}
					else
					{
						m_currTextBox.Shape.FillFormat.FillType = FillType.FillPicture;
					}
					break;
				case "5":
					if (m_currShape != null)
					{
						m_currShape.FillFormat.FillType = FillType.FillGradient;
					}
					else
					{
						m_currTextBox.Shape.FillFormat.FillType = FillType.FillGradient;
					}
					break;
				case "6":
					if (m_currShape != null)
					{
						m_currShape.FillFormat.FillType = FillType.FillGradient;
					}
					else
					{
						m_currTextBox.Shape.FillFormat.FillType = FillType.FillGradient;
					}
					break;
				case "7":
					if (m_currShape != null)
					{
						m_currShape.FillFormat.FillType = FillType.FillGradient;
					}
					else
					{
						m_currTextBox.Shape.FillFormat.FillType = FillType.FillGradient;
					}
					break;
				case "9":
					if (m_currShape != null)
					{
						m_currShape.FillFormat.FillType = FillType.FillBackground;
					}
					else
					{
						m_currTextBox.Shape.FillFormat.FillType = FillType.FillBackground;
					}
					break;
				}
				if (m_currShape != null)
				{
					m_currShape.FillFormat.FillType = (FillType)GetIntValue(tokenValue);
				}
				else
				{
					m_currTextBox.Shape.FillFormat.FillType = (FillType)GetIntValue(tokenValue);
				}
				break;
			case 't':
				if (!(token == "WrapText") || m_currTextBox == null)
				{
					break;
				}
				switch (tokenValue)
				{
				case "0":
					m_currTextBox.TextBoxFormat.TextWrappingStyle = TextWrappingStyle.Square;
					break;
				case "1":
					m_currTextBox.TextBoxFormat.TextWrappingStyle = TextWrappingStyle.Tight;
					break;
				case "2":
					if (m_currTextBox.Shape != null)
					{
						m_currTextBox.IsShape = true;
						m_currTextBox.Shape.TextFrame.NoWrap = true;
					}
					break;
				case "3":
					m_currTextBox.TextBoxFormat.TextWrappingStyle = TextWrappingStyle.TopAndBottom;
					break;
				case "4":
					m_currTextBox.TextBoxFormat.TextWrappingStyle = TextWrappingStyle.Through;
					break;
				}
				break;
			case 'X':
				if (token == "c3DFillX")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.LightRigRotation2X = float.Parse(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.LightRigRotation2X = float.Parse(tokenValue);
					}
				}
				break;
			case 'Y':
				if (token == "c3DFillY")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.LightRigRotation2Y = float.Parse(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.LightRigRotation2Y = float.Parse(tokenValue);
					}
				}
				break;
			case 'Z':
				if (token == "c3DFillZ")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.LightRigRotation2Z = float.Parse(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.LightRigRotation2Z = float.Parse(tokenValue);
					}
				}
				break;
			}
			break;
		case 6:
			switch (token[5])
			{
			case 'V':
				if (token == "fFlipV")
				{
					if (m_currShape != null)
					{
						m_currShape.FlipVertical = tokenValue == "1";
					}
					else
					{
						m_currTextBox.TextBoxFormat.FlipVertical = tokenValue == "1";
					}
				}
				break;
			case 'H':
				if (token == "fFlipH")
				{
					if (m_currShape != null)
					{
						m_currShape.FlipHorizontal = tokenValue == "1";
					}
					else
					{
						m_currTextBox.TextBoxFormat.FlipHorizontal = tokenValue == "1";
					}
				}
				break;
			}
			break;
		case 14:
			switch (token[0])
			{
			case 'd':
				if (token == "dxWrapDistLeft")
				{
					double.TryParse(tokenValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var result6);
					result6 = Math.Round(result6 / 12700.0, 2);
					if (m_currShape != null)
					{
						m_currShape.WrapFormat.DistanceLeft = (float)result6;
					}
					else
					{
						m_currTextBox.TextBoxFormat.WrapDistanceLeft = (float)result6;
					}
				}
				break;
			case 'l':
				if (token == "lineMiterLimit")
				{
					if (m_currShape != null)
					{
						m_currShape.LineFormat.MiterJoinLimit = tokenValue;
					}
					else
					{
						m_currTextBox.Shape.LineFormat.MiterJoinLimit = tokenValue;
					}
				}
				break;
			case 'f':
				if (token == "fillRectBottom")
				{
					float result3 = 0f;
					float.TryParse(tokenValue, NumberStyles.Number, CultureInfo.InvariantCulture, out result3);
					if (m_currShape != null)
					{
						m_currShape.FillFormat.FillRectangle.BottomOffset = result3;
					}
					else
					{
						m_currTextBox.Shape.FillFormat.FillRectangle.BottomOffset = result3;
					}
				}
				break;
			case 'c':
				if (token == "c3DSpecularAmt")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.Specularity = GetIntValue(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.Specularity = GetIntValue(tokenValue);
					}
				}
				break;
			}
			break;
		case 15:
			switch (token[10])
			{
			case 'R':
				if (token == "dxWrapDistRight")
				{
					double.TryParse(tokenValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var result5);
					result5 = Math.Round(result5 / 12700.0, 2);
					if (m_currShape != null)
					{
						m_currShape.WrapFormat.DistanceRight = (float)result5;
					}
					else
					{
						m_currTextBox.TextBoxFormat.WrapDistanceRight = (float)result5;
					}
				}
				break;
			case 'u':
				if (token == "fBehindDocument" && tokenValue == "1")
				{
					if (m_currShape != null && m_currShape.WrapFormat.TextWrappingStyle == TextWrappingStyle.InFrontOfText)
					{
						m_currShape.WrapFormat.TextWrappingStyle = TextWrappingStyle.Behind;
					}
					else if (m_currTextBox != null && m_currTextBox.TextBoxFormat.TextWrappingStyle == TextWrappingStyle.InFrontOfText)
					{
						m_currTextBox.TextBoxFormat.TextWrappingStyle = TextWrappingStyle.Behind;
					}
				}
				break;
			case 'S':
				if (!(token == "lineEndCapStyle"))
				{
					break;
				}
				if (m_currTextBox != null && m_currTextBox.Shape != null)
				{
					m_currTextBox.IsShape = true;
				}
				switch (tokenValue)
				{
				case "0":
					if (m_currShape != null)
					{
						m_currShape.LineFormat.LineCap = DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.LineCap.Round;
					}
					else if (m_currTextBox.Shape != null)
					{
						m_currTextBox.Shape.LineFormat.LineCap = DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.LineCap.Round;
					}
					break;
				case "1":
					if (m_currShape != null)
					{
						m_currShape.LineFormat.LineCap = DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.LineCap.Square;
					}
					else if (m_currTextBox.Shape != null)
					{
						m_currTextBox.Shape.LineFormat.LineCap = DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.LineCap.Square;
					}
					break;
				case "2":
					if (m_currShape != null)
					{
						m_currShape.LineFormat.LineCap = DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.LineCap.Flat;
					}
					else if (m_currTextBox.Shape != null)
					{
						m_currTextBox.Shape.LineFormat.LineCap = DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.LineCap.Flat;
					}
					break;
				}
				break;
			case 'a':
				if (token == "fillBackOpacity")
				{
					if (m_currShape != null)
					{
						m_currShape.FillFormat.SecondaryOpacity = (float)Math.Round(1f - float.Parse(tokenValue) / 65536f, 2) * 100f;
					}
					else
					{
						m_currTextBox.Shape.FillFormat.SecondaryOpacity = (float)Math.Round(1f - float.Parse(tokenValue) / 65536f, 2) * 100f;
					}
				}
				break;
			case 'l':
				if (token == "shadowHighlight")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[0].ShadowFormat.Color2 = Color.FromArgb(int.Parse(tokenValue));
						m_currShape.EffectList[0].ShadowFormat.Color2 = Color.FromArgb(m_currShape.EffectList[0].ShadowFormat.Color2.B, m_currShape.EffectList[0].ShadowFormat.Color2.G, m_currShape.EffectList[0].ShadowFormat.Color2.R);
					}
					else
					{
						m_currTextBox.Shape.EffectList[0].ShadowFormat.Color2 = Color.FromArgb(int.Parse(tokenValue));
						m_currTextBox.Shape.EffectList[0].ShadowFormat.Color2 = Color.FromArgb(m_currShape.EffectList[0].ShadowFormat.Color2.B, m_currShape.EffectList[0].ShadowFormat.Color2.G, m_currShape.EffectList[0].ShadowFormat.Color2.R);
					}
				}
				break;
			case 'c':
				if (token == "fshadowObscured")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[0].ShadowFormat.Obscured = tokenValue == "1";
					}
					else
					{
						m_currTextBox.Shape.EffectList[0].ShadowFormat.Obscured = tokenValue == "1";
					}
				}
				break;
			case 'P':
				if (token == "c3DExtrudePlane" && !string.IsNullOrEmpty(tokenValue) && tokenValue.Length > 2 && Enum.IsDefined(typeof(ExtrusionPlane), char.ToUpper(tokenValue[0]) + tokenValue.Substring(1)))
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.ExtrusionPlane = (ExtrusionPlane)Enum.Parse(typeof(ExtrusionPlane), tokenValue, ignoreCase: true);
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.ExtrusionPlane = (ExtrusionPlane)Enum.Parse(typeof(ExtrusionPlane), tokenValue, ignoreCase: true);
					}
				}
				break;
			case 'o':
				if (token == "fFitShapeToText" && m_currTextBox != null)
				{
					m_currTextBox.TextBoxFormat.AutoFit = tokenValue == "1";
				}
				break;
			case 'n':
				if (token == "c3DKeyIntensity")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.LightLevel = float.Parse(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.LightLevel = float.Parse(tokenValue);
					}
				}
				break;
			case 'e':
				switch (token)
				{
				case "shadowScaleYToX":
					if (m_currShape != null)
					{
						m_currShape.EffectList[0].ShadowFormat.HorizontalSkewAngle = (short)GetIntValue(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[0].ShadowFormat.HorizontalSkewAngle = (short)GetIntValue(tokenValue);
					}
					break;
				case "shadowScaleXToY":
					if (m_currShape != null)
					{
						m_currShape.EffectList[0].ShadowFormat.VerticalSkewAngle = (short)GetIntValue(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[0].ShadowFormat.VerticalSkewAngle = (short)GetIntValue(tokenValue);
					}
					break;
				case "shadowScaleYToY":
					if (m_currShape != null)
					{
						m_currShape.EffectList[0].ShadowFormat.VerticalScalingFactor = double.Parse(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[0].ShadowFormat.VerticalScalingFactor = double.Parse(tokenValue);
					}
					break;
				case "shadowScaleXToX":
					if (m_currShape != null)
					{
						m_currShape.EffectList[0].ShadowFormat.HorizontalScalingFactor = double.Parse(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[0].ShadowFormat.HorizontalScalingFactor = double.Parse(tokenValue);
					}
					break;
				}
				break;
			}
			break;
		case 16:
			switch (token[15])
			{
			case 'm':
				if (token == "dyWrapDistBottom")
				{
					double.TryParse(tokenValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var result4);
					result4 = Math.Round(result4 / 12700.0, 2);
					if (m_currShape != null)
					{
						m_currShape.WrapFormat.DistanceBottom = (float)result4;
					}
					else
					{
						m_currTextBox.TextBoxFormat.WrapDistanceBottom = (float)result4;
					}
				}
				break;
			case 'd':
				if (token == "lineEndArrowhead" && m_currShape != null)
				{
					switch (tokenValue)
					{
					case "0":
						m_currShape.LineFormat.EndArrowheadStyle = ArrowheadStyle.ArrowheadNone;
						break;
					case "1":
						m_currShape.LineFormat.EndArrowheadStyle = ArrowheadStyle.ArrowheadTriangle;
						break;
					case "2":
						m_currShape.LineFormat.EndArrowheadStyle = ArrowheadStyle.ArrowheadStealth;
						break;
					case "3":
						m_currShape.LineFormat.EndArrowheadStyle = ArrowheadStyle.ArrowheadDiamond;
						break;
					case "4":
						m_currShape.LineFormat.EndArrowheadStyle = ArrowheadStyle.ArrowheadOval;
						break;
					case "5":
						m_currShape.LineFormat.EndArrowheadStyle = ArrowheadStyle.ArrowheadOpen;
						break;
					}
				}
				break;
			case 'X':
				if (token == "c3DRotationAxisX")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.RotationX = GetIntValue(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.RotationX = GetIntValue(tokenValue);
					}
				}
				break;
			case 'Y':
				if (token == "c3DRotationAxisY")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.RotationY = GetIntValue(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.RotationY = GetIntValue(tokenValue);
					}
				}
				break;
			case 'Z':
				if (token == "c3DRotationAxisZ")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.RotationZ = GetIntValue(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.RotationZ = GetIntValue(tokenValue);
					}
				}
				break;
			case 'e':
				if (token == "c3DRotationAngle")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.OrientationAngle = GetIntValue(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.OrientationAngle = GetIntValue(tokenValue);
					}
				}
				break;
			case 's':
				if (token == "c3DEdgeThickness")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.Edge = (float)GetIntValue(tokenValue) / 12700f;
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.Edge = (float)GetIntValue(tokenValue) / 12700f;
					}
				}
				break;
			case 'y':
				if (token == "c3DFillIntensity")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.LightLevel2 = float.Parse(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.LightLevel2 = float.Parse(tokenValue);
					}
				}
				break;
			}
			break;
		case 11:
			switch (token[7])
			{
			default:
				return;
			case 'W':
				if (token == "fEditedWrap")
				{
					if (m_currShape != null)
					{
						m_currShape.WrapFormat.WrapPolygon.Edited = tokenValue == "1";
					}
					else
					{
						m_currTextBox.TextBoxFormat.WrapPolygon.Edited = tokenValue == "1";
					}
				}
				return;
			case 'z':
			{
				if (!(token == "pctHorizPos"))
				{
					return;
				}
				float result = float.MaxValue;
				float.TryParse(tokenValue, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
				if (m_currShape != null)
				{
					m_currShape.IsRelativeHorizontalPosition = true;
					if (result != float.MaxValue)
					{
						m_currShape.RelativeHorizontalPosition = (float)Math.Round(result / 10f, 2);
					}
					m_currShape.RelativeHorizontalOrigin = m_currShape.HorizontalOrigin;
				}
				else if (m_currTextBox != null || m_currTextBox.Shape != null)
				{
					m_currTextBox.IsShape = true;
					m_currTextBox.Shape.IsRelativeHorizontalPosition = true;
					if (result != float.MaxValue)
					{
						m_currTextBox.Shape.RelativeHorizontalPosition = (float)Math.Round(result / 10f, 2);
					}
					m_currTextBox.Shape.RelativeHorizontalOrigin = m_currTextBox.TextBoxFormat.HorizontalOrigin;
				}
				return;
			}
			case 'r':
				if (token == "fStandardHR")
				{
					if (m_currShape != null)
					{
						m_currShape.UseStandardColorHR = tokenValue == "1";
					}
					else
					{
						m_currTextBox.Shape.UseStandardColorHR = tokenValue == "1";
					}
				}
				return;
			case 'c':
				if (!(token == "fillOpacity"))
				{
					if (token == "lineOpacity" && m_currShape != null)
					{
						m_currShape.LineFormat.Transparency = GetIntValue(tokenValue);
					}
				}
				else if (m_currShape != null)
				{
					m_currShape.FillFormat.Transparency = (float)Math.Round(1f - float.Parse(tokenValue) / 65536f, 2) * 100f;
				}
				else if (m_currTextBox != null && m_currTextBox.Shape != null)
				{
					m_currTextBox.IsShape = true;
					m_currTextBox.Shape.FillFormat.Transparency = (float)Math.Round(1f - float.Parse(tokenValue) / 65536f, 2) * 100f;
				}
				return;
			case 'h':
				if (!(token == "lineDashing") || tokenValue == null)
				{
					return;
				}
				switch (tokenValue.Length)
				{
				case 1:
					switch (tokenValue[0])
					{
					case '0':
						if (m_currShape != null)
						{
							m_currShape.LineFormat.DashStyle = LineDashing.Solid;
						}
						else
						{
							m_currTextBox.TextBoxFormat.LineDashing = LineDashing.Solid;
						}
						break;
					case '1':
						if (m_currShape != null)
						{
							m_currShape.LineFormat.DashStyle = LineDashing.Dash;
						}
						else
						{
							m_currTextBox.TextBoxFormat.LineDashing = LineDashing.Dash;
						}
						break;
					case '2':
						if (m_currShape != null)
						{
							m_currShape.LineFormat.DashStyle = LineDashing.Dot;
							break;
						}
						m_currTextBox.IsShape = true;
						m_currTextBox.TextBoxFormat.LineDashing = LineDashing.Dot;
						break;
					case '3':
						if (m_currShape != null)
						{
							m_currShape.LineFormat.DashStyle = LineDashing.DashDotGEL;
						}
						else
						{
							m_currTextBox.TextBoxFormat.LineDashing = LineDashing.DashDotGEL;
						}
						break;
					case '4':
						if (m_currShape != null)
						{
							m_currShape.LineFormat.DashStyle = LineDashing.DashDotDot;
						}
						else
						{
							m_currTextBox.TextBoxFormat.LineDashing = LineDashing.DashDotDot;
						}
						break;
					case '5':
						if (m_currShape != null)
						{
							m_currShape.LineFormat.DashStyle = LineDashing.DotGEL;
						}
						else
						{
							m_currTextBox.TextBoxFormat.LineDashing = LineDashing.DotGEL;
						}
						break;
					case '6':
						if (m_currShape != null)
						{
							m_currShape.LineFormat.DashStyle = LineDashing.DashGEL;
						}
						else
						{
							m_currTextBox.TextBoxFormat.LineDashing = LineDashing.DashGEL;
						}
						break;
					case '7':
						if (m_currShape != null)
						{
							m_currShape.LineFormat.DashStyle = LineDashing.LongDashGEL;
						}
						else
						{
							m_currTextBox.TextBoxFormat.LineDashing = LineDashing.LongDashGEL;
						}
						break;
					case '8':
						if (m_currShape != null)
						{
							m_currShape.LineFormat.DashStyle = LineDashing.DashDotGEL;
						}
						else
						{
							m_currTextBox.TextBoxFormat.LineDashing = LineDashing.DashDotGEL;
						}
						break;
					case '9':
						if (m_currShape != null)
						{
							m_currShape.LineFormat.DashStyle = LineDashing.LongDashDotGEL;
						}
						else
						{
							m_currTextBox.TextBoxFormat.LineDashing = LineDashing.LongDashDotGEL;
						}
						break;
					}
					break;
				case 2:
					if (tokenValue == "10")
					{
						if (m_currShape != null)
						{
							m_currShape.LineFormat.DashStyle = LineDashing.LongDashDotDotGEL;
						}
						else
						{
							m_currTextBox.TextBoxFormat.LineDashing = LineDashing.LongDashDotDotGEL;
						}
					}
					break;
				}
				return;
			case 'o':
				if (token == "shadowColor")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[0].ShadowFormat.Color = Color.FromArgb(int.Parse(tokenValue));
						m_currShape.EffectList[0].ShadowFormat.Color = Color.FromArgb(m_currShape.EffectList[0].ShadowFormat.Color.B, m_currShape.EffectList[0].ShadowFormat.Color.G, m_currShape.EffectList[0].ShadowFormat.Color.R);
					}
					else
					{
						m_currTextBox.Shape.EffectList[0].ShadowFormat.Color = Color.FromArgb(int.Parse(tokenValue));
						m_currTextBox.Shape.EffectList[0].ShadowFormat.Color = Color.FromArgb(m_currShape.EffectList[0].ShadowFormat.Color.B, m_currShape.EffectList[0].ShadowFormat.Color.G, m_currShape.EffectList[0].ShadowFormat.Color.R);
					}
				}
				return;
			case 't':
				if (token == "fillRectTop")
				{
					float result3 = 0f;
					float.TryParse(tokenValue, NumberStyles.Number, CultureInfo.InvariantCulture, out result3);
					if (m_currShape != null)
					{
						m_currShape.FillFormat.FillRectangle.TopOffset = result3;
					}
					else
					{
						m_currTextBox.Shape.FillFormat.FillRectangle.TopOffset = result3;
					}
				}
				return;
			case 'i':
				if (token == "dxTextRight")
				{
					if (m_currTextBox != null)
					{
						m_currTextBox.TextBoxFormat.InternalMargin.Right = (float)Math.Round(float.Parse(tokenValue) / 12700f, 2);
					}
					else if (m_currShape != null)
					{
						m_currShape.TextFrame.InternalMargin.Right = (float)Math.Round(float.Parse(tokenValue) / 12700f, 2);
					}
				}
				return;
			case 'a':
				break;
			}
			if (!(token == "adjustValue"))
			{
				break;
			}
			goto IL_4d05;
		case 7:
			switch (token[6])
			{
			case 'n':
				if (token == "fHidden" && tokenValue == "1")
				{
					if (m_currShape != null)
					{
						m_currShape.Visible = false;
					}
					else
					{
						m_currTextBox.Visible = false;
					}
				}
				break;
			case 't':
			{
				if (!(token == "pctVert"))
				{
					break;
				}
				float result2 = float.MaxValue;
				float.TryParse(tokenValue, NumberStyles.Number, CultureInfo.InvariantCulture, out result2);
				if (result2 != float.MaxValue)
				{
					if (m_currShape != null)
					{
						m_currShape.IsRelativeHeight = true;
						m_currShape.RelativeHeight = (float)Math.Round(result2 / 10f, 2);
						m_currShape.RelativeHeightVerticalOrigin = VerticalOrigin.Page;
					}
					else
					{
						m_currTextBox.TextBoxFormat.HeightRelativePercent = (float)Math.Round(result2 / 10f, 2);
						m_currTextBox.TextBoxFormat.HeightOrigin = HeightOrigin.Page;
					}
				}
				break;
			}
			case 'R':
				if (!(token == "alignHR"))
				{
					break;
				}
				switch (tokenValue)
				{
				case "0":
					if (m_currShape != null)
					{
						m_currShape.HorizontalAlignment = ShapeHorizontalAlignment.Left;
					}
					else
					{
						m_currTextBox.TextBoxFormat.HorizontalAlignment = ShapeHorizontalAlignment.Left;
					}
					break;
				case "1":
					if (m_currShape != null)
					{
						m_currShape.HorizontalAlignment = ShapeHorizontalAlignment.Center;
					}
					else
					{
						m_currTextBox.TextBoxFormat.HorizontalAlignment = ShapeHorizontalAlignment.Center;
					}
					break;
				case "2":
					if (m_currShape != null)
					{
						m_currShape.HorizontalAlignment = ShapeHorizontalAlignment.Right;
					}
					else
					{
						m_currTextBox.TextBoxFormat.HorizontalAlignment = ShapeHorizontalAlignment.Right;
					}
					break;
				}
				break;
			case 'w':
				if (token == "fShadow")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[0].ShadowFormat.Visible = tokenValue == "1";
					}
					else
					{
						m_currTextBox.Shape.EffectList[0].ShadowFormat.Visible = tokenValue == "1";
					}
				}
				break;
			case 'X':
				if (token == "c3DKeyX")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.LightRigRotationX = float.Parse(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.LightRigRotationX = float.Parse(tokenValue);
					}
				}
				break;
			case 'Y':
				if (token == "c3DKeyY")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.LightRigRotationY = float.Parse(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.LightRigRotationY = float.Parse(tokenValue);
					}
				}
				break;
			case 'Z':
				if (token == "c3DKeyZ")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.LightRigRotationZ = float.Parse(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.LightRigRotationZ = float.Parse(tokenValue);
					}
				}
				break;
			case 'd':
				if (token == "fFilled" && tokenValue == "0")
				{
					if (m_currTextBox != null)
					{
						m_currTextBox.Shape.FillFormat.Fill = false;
						m_currTextBox.TextBoxFormat.FillColor = Color.Empty;
					}
					else
					{
						m_currShape.FillFormat.Fill = false;
						m_currShape.FillFormat.Color = Color.Empty;
					}
				}
				break;
			}
			break;
		case 10:
			switch (token[1])
			{
			case 'c':
			{
				if (!(token == "pctVertPos"))
				{
					break;
				}
				float result = float.MaxValue;
				float.TryParse(tokenValue, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
				if (m_currShape != null)
				{
					m_currShape.IsRelativeVerticalPosition = true;
					if (result != float.MaxValue)
					{
						m_currShape.RelativeVerticalPosition = (float)Math.Round(result / 10f, 2);
					}
					m_currShape.RelativeVerticalOrigin = m_currShape.VerticalOrigin;
					break;
				}
				m_currTextBox.IsShape = true;
				m_currTextBox.Shape.IsRelativeVerticalPosition = true;
				if (result != float.MaxValue)
				{
					m_currTextBox.Shape.RelativeVerticalPosition = (float)Math.Round(result / 10f, 2);
				}
				m_currTextBox.Shape.RelativeVerticalOrigin = m_currTextBox.TextBoxFormat.VerticalOrigin;
				break;
			}
			case 'H':
				if (token == "fHorizRule")
				{
					if (m_currShape != null)
					{
						m_currShape.IsHorizontalRule = tokenValue == "1";
					}
					else
					{
						m_currTextBox.Shape.IsHorizontalRule = tokenValue == "1";
					}
				}
				break;
			case 'N':
				if (token == "fNoShadeHR")
				{
					if (m_currShape != null)
					{
						m_currShape.UseNoShadeHR = tokenValue == "1";
					}
					else
					{
						m_currTextBox.Shape.UseNoShadeHR = tokenValue == "1";
					}
				}
				break;
			case 'h':
				if (!(token == "shadowType"))
				{
					break;
				}
				switch (tokenValue)
				{
				case "1":
					if (m_currShape != null)
					{
						m_currShape.EffectList[0].ShadowFormat.ShadowType = ShadowType.Double;
					}
					else
					{
						m_currTextBox.Shape.EffectList[0].ShadowFormat.ShadowType = ShadowType.Double;
					}
					break;
				case "2":
					if (m_currShape != null)
					{
						m_currShape.EffectList[0].ShadowFormat.ShadowType = ShadowType.Perspective;
					}
					else
					{
						m_currTextBox.Shape.EffectList[0].ShadowFormat.ShadowType = ShadowType.Perspective;
					}
					break;
				case "3":
					if (m_currShape != null)
					{
						m_currShape.EffectList[0].ShadowFormat.ShadowType = ShadowType.ShapeRelative;
					}
					else
					{
						m_currTextBox.Shape.EffectList[0].ShadowFormat.ShadowType = ShadowType.ShapeRelative;
					}
					break;
				case "4":
					if (m_currShape != null)
					{
						m_currShape.EffectList[0].ShadowFormat.ShadowType = ShadowType.DrawingRelative;
					}
					else
					{
						m_currTextBox.Shape.EffectList[0].ShadowFormat.ShadowType = ShadowType.DrawingRelative;
					}
					break;
				case "5":
					if (m_currShape != null)
					{
						m_currShape.EffectList[0].ShadowFormat.ShadowType = ShadowType.Emboss;
					}
					else
					{
						m_currTextBox.Shape.EffectList[0].ShadowFormat.ShadowType = ShadowType.Emboss;
					}
					break;
				default:
					if (m_currShape != null)
					{
						m_currShape.EffectList[0].ShadowFormat.ShadowType = ShadowType.Single;
					}
					else
					{
						m_currTextBox.Shape.EffectList[0].ShadowFormat.ShadowType = ShadowType.Single;
					}
					break;
				}
				break;
			case 'x':
				if (token == "dxTextLeft")
				{
					if (m_currTextBox != null)
					{
						m_currTextBox.TextBoxFormat.InternalMargin.Left = (float)Math.Round(float.Parse(tokenValue) / 12700f, 2);
					}
					else if (m_currShape != null)
					{
						m_currShape.TextFrame.InternalMargin.Left = (float)Math.Round(float.Parse(tokenValue) / 12700f, 2);
					}
				}
				break;
			case '3':
				if (!(token == "c3DOriginX"))
				{
					if (token == "c3DOriginY")
					{
						if (m_currShape != null)
						{
							m_currShape.EffectList[1].ThreeDFormat.ViewPointOriginY = float.Parse(tokenValue);
						}
						else
						{
							m_currTextBox.Shape.EffectList[1].ThreeDFormat.ViewPointOriginY = float.Parse(tokenValue);
						}
					}
				}
				else if (m_currShape != null)
				{
					m_currShape.EffectList[1].ThreeDFormat.ViewPointOriginX = float.Parse(tokenValue);
				}
				else
				{
					m_currTextBox.Shape.EffectList[1].ThreeDFormat.ViewPointOriginX = float.Parse(tokenValue);
				}
				break;
			case 'n':
				if (token == "anchorText" && m_currTextBox != null && m_currTextBox.Shape != null)
				{
					m_currTextBox.IsShape = true;
					switch (tokenValue)
					{
					case "0":
						m_currTextBox.Shape.TextFrame.TextVerticalAlignment = VerticalAlignment.Top;
						break;
					case "1":
						m_currTextBox.Shape.TextFrame.TextVerticalAlignment = VerticalAlignment.Middle;
						break;
					case "2":
						m_currTextBox.Shape.TextFrame.TextVerticalAlignment = VerticalAlignment.Bottom;
						break;
					}
				}
				break;
			}
			break;
		case 17:
			switch (token[3])
			{
			case 'e':
				if (token == "lineEndArrowWidth" && m_currShape != null)
				{
					switch (tokenValue)
					{
					case "0":
						m_currShape.LineFormat.EndArrowheadWidth = LineEndWidth.NarrowArrow;
						break;
					case "1":
						m_currShape.LineFormat.EndArrowheadWidth = LineEndWidth.MediumWidthArrow;
						break;
					case "2":
						m_currShape.LineFormat.EndArrowheadWidth = LineEndWidth.WideArrow;
						break;
					}
				}
				break;
			case 'E':
				if (!(token == "c3DExtrudeForward"))
				{
					if (token == "c3DExtrusionColor")
					{
						if (m_currShape != null)
						{
							m_currShape.EffectList[1].ThreeDFormat.HasExtrusionColor = true;
							m_currShape.EffectList[1].ThreeDFormat.ExtrusionColor = Color.FromArgb(int.Parse(tokenValue));
							m_currShape.EffectList[1].ThreeDFormat.ExtrusionColor = Color.FromArgb(m_currShape.EffectList[1].ThreeDFormat.ExtrusionColor.B, m_currShape.EffectList[1].ThreeDFormat.ExtrusionColor.G, m_currShape.EffectList[1].ThreeDFormat.ExtrusionColor.R);
						}
						else
						{
							m_currTextBox.Shape.EffectList[1].ThreeDFormat.HasExtrusionColor = true;
							m_currTextBox.Shape.EffectList[1].ThreeDFormat.ExtrusionColor = Color.FromArgb(int.Parse(tokenValue));
							m_currTextBox.Shape.EffectList[1].ThreeDFormat.ExtrusionColor = Color.FromArgb(m_currShape.EffectList[1].ThreeDFormat.ExtrusionColor.B, m_currShape.EffectList[1].ThreeDFormat.ExtrusionColor.G, m_currShape.EffectList[1].ThreeDFormat.ExtrusionColor.R);
						}
					}
				}
				else if (m_currShape != null)
				{
					m_currShape.EffectList[1].ThreeDFormat.ForeDepth = (float)GetIntValue(tokenValue) / 12700f;
				}
				else
				{
					m_currTextBox.Shape.EffectList[1].ThreeDFormat.ForeDepth = (float)GetIntValue(tokenValue) / 12700f;
				}
				break;
			case 'Y':
				if (token == "c3DYRotationAngle")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.RotationAngleY = GetIntValue(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.RotationAngleY = GetIntValue(tokenValue);
					}
				}
				break;
			case 'X':
				if (token == "c3DXRotationAngle")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.RotationAngleX = GetIntValue(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.RotationAngleX = GetIntValue(tokenValue);
					}
				}
				break;
			}
			break;
		case 18:
			switch (token[7])
			{
			case 'A':
				if (token == "lineEndArrowLength" && m_currShape != null)
				{
					switch (tokenValue)
					{
					case "0":
						m_currShape.LineFormat.EndArrowheadLength = LineEndLength.ShortArrow;
						break;
					case "1":
						m_currShape.LineFormat.EndArrowheadLength = LineEndLength.MediumLenArrow;
						break;
					case "2":
						m_currShape.LineFormat.EndArrowheadLength = LineEndLength.LongArrow;
						break;
					}
				}
				break;
			case 'r':
				if (token == "lineStartArrowhead" && m_currShape != null)
				{
					switch (tokenValue)
					{
					case "0":
						m_currShape.LineFormat.BeginArrowheadStyle = ArrowheadStyle.ArrowheadNone;
						break;
					case "1":
						m_currShape.LineFormat.BeginArrowheadStyle = ArrowheadStyle.ArrowheadTriangle;
						break;
					case "2":
						m_currShape.LineFormat.BeginArrowheadStyle = ArrowheadStyle.ArrowheadStealth;
						break;
					case "3":
						m_currShape.LineFormat.BeginArrowheadStyle = ArrowheadStyle.ArrowheadDiamond;
						break;
					case "4":
						m_currShape.LineFormat.BeginArrowheadStyle = ArrowheadStyle.ArrowheadOval;
						break;
					case "5":
						m_currShape.LineFormat.BeginArrowheadStyle = ArrowheadStyle.ArrowheadOpen;
						break;
					}
				}
				break;
			case 'u':
				if (token == "c3DExtrudeBackward")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.BackDepth = (float)GetIntValue(tokenValue) / 12700f;
						m_currShape.EffectList[1].ThreeDFormat.ExtrusionHeight = m_currShape.EffectList[1].ThreeDFormat.BackDepth;
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.BackDepth = (float)GetIntValue(tokenValue) / 12700f;
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.ExtrusionHeight = m_currShape.EffectList[1].ThreeDFormat.BackDepth;
					}
				}
				break;
			case 't':
				switch (token)
				{
				case "c3DRotationCenterX":
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.RotationCenterX = GetIntValue(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.RotationCenterX = GetIntValue(tokenValue);
					}
					break;
				case "c3DRotationCenterY":
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.RotationCenterY = GetIntValue(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.RotationCenterY = GetIntValue(tokenValue);
					}
					break;
				case "c3DRotationCenterZ":
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.RotationCenterZ = GetIntValue(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.RotationCenterZ = GetIntValue(tokenValue);
					}
					break;
				}
				break;
			case 'e':
				if (token == "shadowPerspectiveX" || token == "shadowPerspectiveY")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[0].ShadowFormat.ShadowPerspectiveMatrix = tokenValue;
					}
					else
					{
						m_currTextBox.Shape.EffectList[0].ShadowFormat.ShadowPerspectiveMatrix = tokenValue;
					}
				}
				break;
			}
			break;
		case 19:
			switch (token[18])
			{
			case 'h':
				if (token == "lineStartArrowWidth")
				{
					switch (tokenValue)
					{
					case "0":
						m_currShape.LineFormat.BeginArrowheadWidth = LineEndWidth.NarrowArrow;
						break;
					case "1":
						m_currShape.LineFormat.BeginArrowheadWidth = LineEndWidth.MediumWidthArrow;
						break;
					case "2":
						m_currShape.LineFormat.BeginArrowheadWidth = LineEndWidth.WideArrow;
						break;
					}
				}
				break;
			case 'X':
				if (token == "shadowSecondOffsetX")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[0].ShadowFormat.ShadowOffset2X = GetIntValue(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[0].ShadowFormat.ShadowOffset2X = GetIntValue(tokenValue);
					}
				}
				break;
			case 'Y':
				if (token == "shadowSecondOffsetY")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[0].ShadowFormat.ShadowOffset2Y = GetIntValue(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[0].ShadowFormat.ShadowOffset2Y = GetIntValue(tokenValue);
					}
				}
				break;
			case 'y':
				if (token == "c3DAmbientIntensity")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.Brightness = float.Parse(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.Brightness = float.Parse(tokenValue);
					}
				}
				break;
			}
			break;
		case 5:
			switch (token[0])
			{
			case 'p':
			{
				if (!(token == "pctHR"))
				{
					break;
				}
				float num = (float)Math.Round(double.Parse(tokenValue) / 10.0);
				if (num != 0f)
				{
					if (m_currShape != null)
					{
						m_currShape.WidthScale = num;
					}
					else
					{
						m_currTextBox.Shape.WidthScale = num;
					}
				}
				break;
			}
			case 'f':
				if (!(token == "fLine"))
				{
					break;
				}
				if (m_currShape != null)
				{
					m_currShape.LineFormat.Line = tokenValue == "1";
					if (!m_currShape.LineFormat.Line)
					{
						m_currShape.LineFormat.Color = Color.Empty;
					}
					break;
				}
				m_currTextBox.TextBoxFormat.NoLine = !(tokenValue == "1");
				if (m_currTextBox.TextBoxFormat.NoLine && m_currTextBox.Shape != null)
				{
					m_currTextBox.IsShape = true;
					m_currTextBox.TextBoxFormat.LineColor = Color.Empty;
					m_currTextBox.Shape.LineFormat.Color = Color.Empty;
				}
				break;
			}
			break;
		case 9:
			switch (token[4])
			{
			case 'C':
				if (!(token == "fillColor"))
				{
					if (token == "lineColor")
					{
						if (m_currShape != null)
						{
							m_currShape.LineFormat.Color = Color.FromArgb(int.Parse(tokenValue));
							m_currShape.LineFormat.Color = Color.FromArgb(m_currShape.LineFormat.Color.B, m_currShape.LineFormat.Color.G, m_currShape.LineFormat.Color.R);
						}
						else
						{
							m_currTextBox.TextBoxFormat.LineColor = Color.FromArgb(GetIntValue(tokenValue));
							m_currTextBox.TextBoxFormat.LineColor = Color.FromArgb(m_currTextBox.TextBoxFormat.LineColor.B, m_currTextBox.TextBoxFormat.LineColor.G, m_currTextBox.TextBoxFormat.LineColor.R);
						}
					}
				}
				else if (m_currShape != null)
				{
					m_currShape.FillFormat.Fill = true;
					m_currShape.FillFormat.IsDefaultFill = false;
					m_currShape.IsFillStyleInline = true;
					m_currShape.FillFormat.Color = Color.FromArgb(int.Parse(tokenValue));
					m_currShape.FillFormat.Color = Color.FromArgb(m_currShape.FillFormat.Color.B, m_currShape.FillFormat.Color.G, m_currShape.FillFormat.Color.R);
				}
				else
				{
					m_currTextBox.TextBoxFormat.FillColor = Color.FromArgb(int.Parse(tokenValue));
					m_currTextBox.TextBoxFormat.FillColor = Color.FromArgb(m_currTextBox.TextBoxFormat.FillColor.B, m_currTextBox.TextBoxFormat.FillColor.G, m_currTextBox.TextBoxFormat.FillColor.R);
				}
				break;
			case 'S':
				if (!(token == "lineStyle"))
				{
					break;
				}
				switch (tokenValue)
				{
				case "0":
					if (m_currShape != null)
					{
						m_currShape.LineFormat.Style = LineStyle.Single;
					}
					else
					{
						m_currTextBox.TextBoxFormat.LineStyle = TextBoxLineStyle.Simple;
					}
					break;
				case "1":
					if (m_currShape != null)
					{
						m_currShape.LineFormat.Style = LineStyle.ThinThin;
					}
					else
					{
						m_currTextBox.TextBoxFormat.LineStyle = TextBoxLineStyle.Double;
					}
					break;
				case "2":
					if (m_currShape != null)
					{
						m_currShape.LineFormat.Style = LineStyle.ThickThin;
					}
					else
					{
						m_currTextBox.TextBoxFormat.LineStyle = TextBoxLineStyle.ThickThin;
					}
					break;
				case "3":
					if (m_currShape != null)
					{
						m_currShape.LineFormat.Style = LineStyle.ThinThick;
					}
					else
					{
						m_currTextBox.TextBoxFormat.LineStyle = TextBoxLineStyle.ThinThick;
					}
					break;
				case "4":
					if (m_currShape != null)
					{
						m_currShape.LineFormat.Style = LineStyle.ThickBetweenThin;
					}
					else
					{
						m_currTextBox.TextBoxFormat.LineStyle = TextBoxLineStyle.Triple;
					}
					break;
				}
				break;
			case 'W':
			{
				if (!(token == "lineWidth"))
				{
					break;
				}
				double.TryParse(tokenValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var result9);
				result9 = Math.Round(result9 / 12700.0, 2);
				if (m_currShape != null)
				{
					m_currShape.LineFormat.Weight = (float)result9;
					if (m_currShape.LineFormat.LineFormatType == (LineFormatType)0)
					{
						m_currShape.LineFormat.LineFormatType = LineFormatType.Solid;
					}
				}
				else
				{
					m_currTextBox.TextBoxFormat.LineWidth = (float)result9;
				}
				break;
			}
			case 'F':
				if (token == "fillFocus")
				{
					if (m_currShape != null)
					{
						m_currShape.FillFormat.Focus = float.Parse(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.FillFormat.Focus = float.Parse(tokenValue);
					}
				}
				break;
			case 'x':
				if (token == "dyTextTop")
				{
					if (m_currTextBox != null)
					{
						m_currTextBox.TextBoxFormat.InternalMargin.Top = (float)Math.Round(float.Parse(tokenValue) / 12700f, 2);
					}
					else if (m_currShape != null)
					{
						m_currShape.TextFrame.InternalMargin.Top = (float)Math.Round(float.Parse(tokenValue) / 12700f, 2);
					}
				}
				break;
			case 'e':
				if (token == "scaleText" && m_currTextBox != null)
				{
					m_currTextBox.CharacterFormat.Scaling = float.Parse(tokenValue);
				}
				break;
			case 'A':
				if (token == "fillAngle")
				{
					if (m_currShape != null)
					{
						m_currShape.FillFormat.TextureHorizontalScale = float.Parse(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.FillFormat.TextureHorizontalScale = float.Parse(tokenValue);
					}
				}
				break;
			}
			break;
		case 12:
			switch (token[6])
			{
			case 'c':
				if (token == "fillRectLeft")
				{
					float result3 = 0f;
					float.TryParse(tokenValue, NumberStyles.Number, CultureInfo.InvariantCulture, out result3);
					if (m_currShape != null)
					{
						m_currShape.FillFormat.FillRectangle.LeftOffset = result3;
					}
					else
					{
						m_currTextBox.Shape.FillFormat.FillRectangle.LeftOffset = result3;
					}
				}
				return;
			case 'B':
				if (token == "dyTextBottom")
				{
					if (m_currTextBox != null)
					{
						m_currTextBox.TextBoxFormat.InternalMargin.Bottom = (float)Math.Round(float.Parse(tokenValue) / 12700f, 2);
					}
					else if (m_currShape != null)
					{
						m_currShape.TextFrame.InternalMargin.Bottom = (float)Math.Round(float.Parse(tokenValue) / 12700f, 2);
					}
				}
				return;
			case 'n':
				if (token == "c3DShininess")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.Shininess = GetIntValue(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.Shininess = GetIntValue(tokenValue);
					}
				}
				return;
			case 't':
				if (token == "fc3DMetallic")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.Metal = GetIntValue(tokenValue) == 1;
						m_currShape.EffectList[1].ThreeDFormat.PresetMaterialType = ((tokenValue == "1") ? PresetMaterialType.Metal : PresetMaterialType.LegacyMatte);
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.Metal = GetIntValue(tokenValue) == 1;
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.PresetMaterialType = ((tokenValue == "1") ? PresetMaterialType.Metal : PresetMaterialType.LegacyMatte);
					}
				}
				return;
			case 'x':
				if (token == "txflTextFlow" && m_currTextBox != null)
				{
					switch (tokenValue)
					{
					case "1":
						m_currTextBox.TextBoxFormat.TextDirection = TextDirection.VerticalFarEast;
						break;
					case "2":
						m_currTextBox.TextBoxFormat.TextDirection = TextDirection.VerticalBottomToTop;
						break;
					case "3":
						m_currTextBox.TextBoxFormat.TextDirection = TextDirection.VerticalTopToBottom;
						break;
					case "4":
						m_currTextBox.TextBoxFormat.TextDirection = TextDirection.HorizontalFarEast;
						break;
					case "5":
						m_currTextBox.TextBoxFormat.TextDirection = TextDirection.Vertical;
						break;
					default:
						m_currTextBox.TextBoxFormat.TextDirection = TextDirection.Horizontal;
						break;
					}
				}
				return;
			case 'w':
				if (token == "c3DSkewAngle")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.SkewAngle = float.Parse(tokenValue);
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.SkewAngle = float.Parse(tokenValue);
					}
				}
				return;
			case 'y':
				if (token == "fc3DKeyHarsh")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.LightHarsh = GetIntValue(tokenValue) == 1;
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.LightHarsh = GetIntValue(tokenValue) == 1;
					}
				}
				return;
			case '2':
				if (!(token == "adjust2Value"))
				{
					return;
				}
				break;
			case '3':
				if (!(token == "adjust3Value"))
				{
					return;
				}
				break;
			case '4':
				if (!(token == "adjust4Value"))
				{
					return;
				}
				break;
			case '5':
				if (!(token == "adjust5Value"))
				{
					return;
				}
				break;
			case '6':
				if (!(token == "adjust6Value"))
				{
					return;
				}
				break;
			case '7':
				if (!(token == "adjust7Value"))
				{
					return;
				}
				break;
			case '8':
				if (!(token == "adjust8Value"))
				{
					return;
				}
				break;
			case '9':
				if (!(token == "adjust9Value"))
				{
					return;
				}
				break;
			case 'S':
				if (token == "fInnerShadow")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[0].ShadowFormat.m_type = ((tokenValue == "0") ? "outerShdw" : "innerShdw");
					}
					else
					{
						m_currTextBox.Shape.EffectList[0].ShadowFormat.m_type = ((tokenValue == "0") ? "outerShdw" : "innerShdw");
					}
				}
				return;
			case 'r':
				if (token == "fc3DParallel")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.HasCameraEffect = true;
						m_currShape.EffectList[1].ThreeDFormat.CameraPresetType = ((tokenValue == "1") ? CameraPresetType.LegacyObliqueLeft : CameraPresetType.PerspectiveBelow);
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.HasCameraEffect = true;
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.CameraPresetType = ((tokenValue == "1") ? CameraPresetType.LegacyObliqueLeft : CameraPresetType.PerspectiveBelow);
					}
				}
				return;
			default:
				return;
			}
			goto IL_4d05;
		case 21:
			switch (token[1])
			{
			case 'R':
				if (token == "fRecolorFillAsPicture")
				{
					if (m_currShape != null)
					{
						m_currShape.FillFormat.ReColor = tokenValue == "1";
					}
					else
					{
						m_currTextBox.Shape.FillFormat.ReColor = tokenValue == "1";
					}
				}
				break;
			case 'c':
				if (token == "fc3DUseExtrusionColor")
				{
					if (m_currShape != null)
					{
						m_currShape.EffectList[1].ThreeDFormat.HasExtrusionColor = GetIntValue(tokenValue) == 1;
					}
					else
					{
						m_currTextBox.Shape.EffectList[1].ThreeDFormat.HasExtrusionColor = GetIntValue(tokenValue) == 1;
					}
				}
				break;
			}
			break;
		case 20:
			if (token == "lineStartArrowLength" && m_currShape != null)
			{
				switch (tokenValue)
				{
				case "0":
					m_currShape.LineFormat.BeginArrowheadLength = LineEndLength.ShortArrow;
					break;
				case "1":
					m_currShape.LineFormat.BeginArrowheadLength = LineEndLength.MediumLenArrow;
					break;
				case "2":
					m_currShape.LineFormat.BeginArrowheadLength = LineEndLength.LongArrow;
					break;
				}
			}
			break;
		case 23:
			if (token == "c3DExtrusionColorExtMod")
			{
				if (m_currShape != null)
				{
					m_currShape.EffectList[1].ThreeDFormat.ColorMode = tokenValue;
				}
				else
				{
					m_currTextBox.Shape.EffectList[1].ThreeDFormat.ColorMode = tokenValue;
				}
			}
			break;
		case 3:
			if (token == "f3D")
			{
				if (m_currShape != null)
				{
					m_currShape.EffectList[1].ThreeDFormat.Visible = GetIntValue(tokenValue) == 1;
					m_currShape.EffectList[1].ThreeDFormat.HasBevelTop = true;
					m_currShape.EffectList[1].ThreeDFormat.HasBevelBottom = true;
					m_currShape.EffectList[1].ThreeDFormat.BevelBottomHeight = 1f;
					m_currShape.EffectList[1].ThreeDFormat.BevelBottomWidth = 1f;
					m_currShape.EffectList[1].ThreeDFormat.BevelTopHeight = 1f;
					m_currShape.EffectList[1].ThreeDFormat.BevelTopWidth = 1f;
					m_currShape.EffectList[1].ThreeDFormat.PresetMaterialType = PresetMaterialType.LegacyMatte;
					m_currShape.EffectList[1].ThreeDFormat.ExtrusionHeight = 36f;
					m_currShape.EffectList[1].ThreeDFormat.HasLightRigEffect = true;
					m_currShape.EffectList[1].ThreeDFormat.LightRigType = LightRigType.LegacyFlat3;
					m_currShape.EffectList[1].ThreeDFormat.LightRigDirection = LightRigDirection.T;
				}
				else
				{
					m_currTextBox.Shape.EffectList[1].ThreeDFormat.Visible = GetIntValue(tokenValue) == 1;
					m_currTextBox.Shape.EffectList[1].ThreeDFormat.HasBevelTop = true;
					m_currTextBox.Shape.EffectList[1].ThreeDFormat.HasBevelBottom = true;
					m_currTextBox.Shape.EffectList[1].ThreeDFormat.BevelBottomHeight = Math.Abs(1);
					m_currTextBox.Shape.EffectList[1].ThreeDFormat.BevelBottomWidth = 1f;
					m_currTextBox.Shape.EffectList[1].ThreeDFormat.BevelTopHeight = 1f;
					m_currTextBox.Shape.EffectList[1].ThreeDFormat.BevelTopWidth = 1f;
					m_currTextBox.Shape.EffectList[1].ThreeDFormat.PresetMaterialType = PresetMaterialType.LegacyMatte;
					m_currTextBox.Shape.EffectList[1].ThreeDFormat.ExtrusionHeight = 36f;
					m_currTextBox.Shape.EffectList[1].ThreeDFormat.HasLightRigEffect = true;
					m_currTextBox.Shape.EffectList[1].ThreeDFormat.LightRigType = LightRigType.LegacyFlat3;
					m_currTextBox.Shape.EffectList[1].ThreeDFormat.LightRigDirection = LightRigDirection.T;
				}
			}
			break;
		case 22:
			if (token == "fc3DRotationCenterAuto")
			{
				if (m_currShape != null)
				{
					m_currShape.EffectList[1].ThreeDFormat.AutoRotationCenter = GetIntValue(tokenValue) == 1;
				}
				else
				{
					m_currTextBox.Shape.EffectList[1].ThreeDFormat.AutoRotationCenter = GetIntValue(tokenValue) == 1;
				}
			}
			break;
		case 4:
			break;
			IL_4d05:
			if (m_currShape.ShapeGuide.ContainsKey(token))
			{
				m_currShape.ShapeGuide[token] = tokenValue;
			}
			else
			{
				m_currShape.ShapeGuide.Add(token, tokenValue);
			}
			break;
		}
	}

	private void SetDefaultValuesForShapeTextBox()
	{
		m_currTextBox.Shape.LineFormat.Color = m_currTextBox.TextBoxFormat.LineColor;
		m_currTextBox.Shape.LineFormat.Weight = m_currTextBox.TextBoxFormat.LineWidth;
		m_currTextBox.Shape.LineFormat.DashStyle = m_currTextBox.TextBoxFormat.LineDashing;
		m_currTextBox.Shape.LineFormat.Style = GetLineStyle();
		m_currTextBox.Shape.FillFormat.Color = m_currTextBox.TextBoxFormat.FillColor;
		if (m_currTextBox.Shape.LineFormat.LineCap != 0)
		{
			m_currTextBox.Shape.LineFormat.LineCap = DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.LineCap.Flat;
		}
		m_currTextBox.Shape.TextFrame.InternalMargin.Right = m_currTextBox.TextBoxFormat.InternalMargin.Right;
		m_currTextBox.Shape.TextFrame.InternalMargin.Left = m_currTextBox.TextBoxFormat.InternalMargin.Left;
		m_currTextBox.Shape.TextFrame.InternalMargin.Top = m_currTextBox.TextBoxFormat.InternalMargin.Top;
		m_currTextBox.Shape.TextFrame.InternalMargin.Bottom = m_currTextBox.TextBoxFormat.InternalMargin.Bottom;
	}

	private LineStyle GetLineStyle()
	{
		return m_currTextBox.TextBoxFormat.LineStyle switch
		{
			TextBoxLineStyle.Simple => LineStyle.Single, 
			TextBoxLineStyle.Double => LineStyle.StyleMixed, 
			TextBoxLineStyle.Triple => LineStyle.ThickBetweenThin, 
			TextBoxLineStyle.ThickThin => LineStyle.ThickThin, 
			TextBoxLineStyle.ThinThick => LineStyle.ThinThick, 
			_ => LineStyle.Single, 
		};
	}

	private void SetRotationValue(string rotationValue)
	{
		if (m_currShape != null)
		{
			m_currShape.Rotation = float.Parse(rotationValue) / 65536f;
			if (m_currShape.AutoShapeType == AutoShapeType.ElbowConnector || m_currShape.AutoShapeType == AutoShapeType.CurvedConnector)
			{
				m_currShape.Rotation = Math.Abs(m_currShape.Rotation);
			}
			if ((m_currShape.Rotation >= 44f && m_currShape.Rotation <= 134f) || (m_currShape.Rotation >= 225f && m_currShape.Rotation <= 314f))
			{
				float height = m_currShape.Height;
				m_currShape.Height = m_currShape.Width;
				m_currShape.Width = height;
				float num = Math.Abs(m_currShape.Height - m_currShape.Width) / 2f;
				if (m_currShape.Height > m_currShape.Width)
				{
					m_currShape.HorizontalPosition += num;
					m_currShape.VerticalPosition -= num;
				}
				if (m_currShape.Height < m_currShape.Width)
				{
					m_currShape.VerticalPosition += num;
					m_currShape.HorizontalPosition -= num;
				}
			}
		}
		else
		{
			m_currTextBox.TextBoxFormat.Rotation = float.Parse(rotationValue) / 65536f;
		}
	}

	private void AddShadowDirectionandDistance()
	{
		double num = m_currShape.EffectList[0].ShadowFormat.ShadowOffsetX;
		double num2 = m_currShape.EffectList[0].ShadowFormat.ShadowOffsetY;
		double num3 = num / 12700.0;
		double num4 = num2 / 12700.0;
		m_currShape.EffectList[0].ShadowFormat.Distance = Math.Sqrt(Math.Pow(Math.Abs(num3), 2.0) + Math.Pow(Math.Abs(num4), 2.0));
		double x = 0.0 - num3;
		double num5 = Math.Atan2(0.0 - num4, x) * (180.0 / Math.PI);
		if (num < 0.0 && num2 < 0.0)
		{
			double a = 180.0 + Math.Abs(num5);
			m_currShape.EffectList[0].ShadowFormat.Direction = Math.Round(a);
		}
		else if (num > 0.0 && num2 < 0.0)
		{
			double num6 = 0.0;
			num6 = ((!(num5 > 0.0)) ? (360.0 - Math.Abs(num5)) : (180.0 + Math.Abs(num5)));
			m_currShape.EffectList[0].ShadowFormat.Direction = Math.Round(num6);
		}
		else if (num < 0.0 && num2 > 0.0)
		{
			double num7 = 0.0;
			num7 = ((!(num5 > 0.0)) ? (180.0 - Math.Abs(num5)) : Math.Abs(num5));
			m_currShape.EffectList[0].ShadowFormat.Direction = Math.Round(num7);
		}
		else if (num > 0.0 && num2 > 0.0)
		{
			double num8 = 0.0;
			num8 = ((!(num5 > 0.0)) ? (180.0 - Math.Abs(num5)) : Math.Abs(num5));
			m_currShape.EffectList[0].ShadowFormat.Direction = Math.Round(num8);
		}
	}

	private void AddAdjustValues()
	{
		switch (m_currShape.AutoShapeType)
		{
		case AutoShapeType.ElbowConnector:
		case AutoShapeType.CurvedConnector:
		{
			using Dictionary<string, string>.Enumerator enumerator = m_currShape.ShapeGuide.GetEnumerator();
			if (enumerator.MoveNext())
			{
				KeyValuePair<string, string> current7 = enumerator.Current;
				double num54 = ((!current7.Key.Contains("adjust")) ? ((double)int.Parse(current7.Value.Substring(4))) : ((double)int.Parse(current7.Value)));
				double num55 = Math.Round(Math.Abs(num54) * 4.62962963);
				if (num54 < 0.0)
				{
					num55 = 0.0 - num55;
				}
				if (m_currShape.ShapeGuide.Count > 1 && current7.Key.Contains("adjustValue"))
				{
					m_currShape.ShapeGuide.Clear();
					m_currShape.ShapeGuide.Add("adj1", "val " + num55);
				}
				else
				{
					m_currShape.ShapeGuide.Clear();
					m_currShape.ShapeGuide.Add("adj1", "val 50000");
				}
			}
			break;
		}
		case AutoShapeType.RoundedRectangle:
		case AutoShapeType.DoubleBracket:
		case AutoShapeType.Plaque:
			if (m_currShape.ShapeGuide.Count != 0)
			{
				using (Dictionary<string, string>.Enumerator enumerator = m_currShape.ShapeGuide.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						double num105 = int.Parse(enumerator.Current.Value);
						double num106 = Math.Round(Math.Abs(num105) * 4.62962963);
						if (num105 < 0.0)
						{
							num106 = 0.0;
						}
						m_currShape.ShapeGuide.Clear();
						m_currShape.ShapeGuide.Add("adj", "val " + num106);
					}
					break;
				}
			}
			m_currShape.ShapeGuide.Add("adj", "val 16667");
			break;
		case AutoShapeType.Octagon:
			if (m_currShape.ShapeGuide.Count != 0)
			{
				using (Dictionary<string, string>.Enumerator enumerator = m_currShape.ShapeGuide.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						double num100 = int.Parse(enumerator.Current.Value);
						double num101 = Math.Round(Math.Abs(num100) * 4.62962963);
						if (num100 < 0.0)
						{
							num101 = 0.0;
						}
						m_currShape.ShapeGuide.Clear();
						m_currShape.ShapeGuide.Add("adj", "val " + num101);
					}
					break;
				}
			}
			m_currShape.ShapeGuide.Add("adj", "val 29287");
			break;
		case AutoShapeType.IsoscelesTriangle:
		case AutoShapeType.Moon:
			if (m_currShape.ShapeGuide.Count != 0)
			{
				using (Dictionary<string, string>.Enumerator enumerator = m_currShape.ShapeGuide.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						double num61 = int.Parse(enumerator.Current.Value);
						double num62 = Math.Round(Math.Abs(num61) * 4.62962963);
						if (num61 < 0.0)
						{
							num62 = 0.0;
						}
						m_currShape.ShapeGuide.Clear();
						m_currShape.ShapeGuide.Add("adj", "val " + num62);
					}
					break;
				}
			}
			m_currShape.ShapeGuide.Add("adj", "val 50000");
			break;
		case AutoShapeType.Cross:
		case AutoShapeType.Cube:
		case AutoShapeType.Sun:
			if (m_currShape.ShapeGuide.Count != 0)
			{
				using (Dictionary<string, string>.Enumerator enumerator = m_currShape.ShapeGuide.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						double num81 = int.Parse(enumerator.Current.Value);
						double num82 = Math.Round(Math.Abs(num81) * 4.62962963);
						if (num81 < 0.0)
						{
							num82 = 0.0;
						}
						m_currShape.ShapeGuide.Clear();
						m_currShape.ShapeGuide.Add("adj", "val " + num82);
					}
					break;
				}
			}
			m_currShape.ShapeGuide.Add("adj", "val 25000");
			break;
		case AutoShapeType.Bevel:
			if (m_currShape.ShapeGuide.Count != 0)
			{
				using (Dictionary<string, string>.Enumerator enumerator = m_currShape.ShapeGuide.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						double num9 = int.Parse(enumerator.Current.Value);
						double num10 = Math.Round(Math.Abs(num9) * 4.62962963);
						if (num9 < 0.0)
						{
							num10 = 0.0;
						}
						m_currShape.ShapeGuide.Clear();
						m_currShape.ShapeGuide.Add("adj", "val " + num10);
					}
					break;
				}
			}
			m_currShape.ShapeGuide.Add("adj", "val 12500");
			break;
		case AutoShapeType.DoubleBrace:
			if (m_currShape.ShapeGuide.Count != 0)
			{
				using (Dictionary<string, string>.Enumerator enumerator = m_currShape.ShapeGuide.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						double num16 = int.Parse(enumerator.Current.Value);
						double num17 = Math.Round(Math.Abs(num16) * 4.62962963);
						if (num16 < 0.0)
						{
							num17 = 0.0;
						}
						m_currShape.ShapeGuide.Clear();
						m_currShape.ShapeGuide.Add("adj", "val " + num17);
					}
					break;
				}
			}
			m_currShape.ShapeGuide.Add("adj", "val 8333");
			break;
		case AutoShapeType.FoldedCorner:
			if (m_currShape.ShapeGuide.Count != 0)
			{
				using (Dictionary<string, string>.Enumerator enumerator = m_currShape.ShapeGuide.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						double num69 = Math.Abs(int.Parse(enumerator.Current.Value));
						double num70 = Math.Abs(21600.0 - num69);
						num70 = Math.Round(num70 * 4.62962963);
						m_currShape.ShapeGuide.Clear();
						m_currShape.ShapeGuide.Add("adj", "val " + num70);
					}
					break;
				}
			}
			m_currShape.ShapeGuide.Add("adj", "val 12500");
			break;
		case AutoShapeType.SmileyFace:
			if (m_currShape.ShapeGuide.Count != 0)
			{
				using (Dictionary<string, string>.Enumerator enumerator = m_currShape.ShapeGuide.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						double num119 = Math.Abs(int.Parse(enumerator.Current.Value));
						double num120 = 17520.0 - num119;
						num120 = Math.Round(4653.0 - num120 * 4.62962963);
						m_currShape.ShapeGuide.Clear();
						m_currShape.ShapeGuide.Add("adj", "val " + num120);
					}
					break;
				}
			}
			m_currShape.ShapeGuide.Add("adj", "val 4653");
			break;
		case AutoShapeType.Parallelogram:
			if (m_currShape.ShapeGuide.Count != 0)
			{
				using Dictionary<string, string>.Enumerator enumerator = m_currShape.ShapeGuide.GetEnumerator();
				if (enumerator.MoveNext())
				{
					double num66 = Math.Round((double)(float)int.Parse(enumerator.Current.Value) * 4.62962963);
					m_currShape.ShapeGuide.Clear();
					m_currShape.ShapeGuide.Add("adj", "val " + num66);
				}
				break;
			}
			if (m_currShape.Width > m_currShape.Height)
			{
				float num67 = m_currShape.Width - m_currShape.Height;
				double num68 = Math.Round(25000f / m_currShape.Height * num67 + 25000f);
				m_currShape.ShapeGuide.Add("adj", "val " + num68);
			}
			else
			{
				m_currShape.ShapeGuide.Add("adj", "val 25000");
			}
			break;
		case AutoShapeType.Hexagon:
			if (m_currShape.ShapeGuide.Count != 0)
			{
				if (m_currShape.Width > m_currShape.Height)
				{
					using Dictionary<string, string>.Enumerator enumerator = m_currShape.ShapeGuide.GetEnumerator();
					if (enumerator.MoveNext())
					{
						KeyValuePair<string, string> current15 = enumerator.Current;
						float num94 = m_currShape.Width - m_currShape.Height;
						float num95 = int.Parse(current15.Value);
						double num96 = Math.Round((4.62962963 / (double)m_currShape.Height * (double)num94 + 4.62962963) * (double)num95);
						m_currShape.ShapeGuide.Clear();
						m_currShape.ShapeGuide.Add("adj", "val " + num96);
					}
				}
				else
				{
					using Dictionary<string, string>.Enumerator enumerator = m_currShape.ShapeGuide.GetEnumerator();
					if (enumerator.MoveNext())
					{
						double num97 = Math.Round((double)(float)int.Parse(enumerator.Current.Value) * 4.62962963);
						m_currShape.ShapeGuide.Clear();
						m_currShape.ShapeGuide.Add("adj", "val " + num97);
					}
				}
			}
			else if (m_currShape.Width > m_currShape.Height)
			{
				float num98 = m_currShape.Width - m_currShape.Height;
				double num99 = Math.Round(25000f / m_currShape.Height * num98 + 25000f);
				m_currShape.ShapeGuide.Add("adj", "val " + num99);
			}
			else
			{
				m_currShape.ShapeGuide.Add("adj", "val 25000");
			}
			m_currShape.ShapeGuide.Add("vf", "val 115470");
			break;
		case AutoShapeType.Can:
			if (m_currShape.ShapeGuide.Count != 0)
			{
				using Dictionary<string, string>.Enumerator enumerator = m_currShape.ShapeGuide.GetEnumerator();
				if (enumerator.MoveNext())
				{
					double num37 = Math.Round((double)(float)int.Parse(enumerator.Current.Value) * 4.62962963);
					m_currShape.ShapeGuide.Clear();
					m_currShape.ShapeGuide.Add("adj", "val " + num37);
				}
				break;
			}
			if (m_currShape.Height > m_currShape.Width)
			{
				float num38 = m_currShape.Height - m_currShape.Width;
				double num39 = Math.Round(25000f / m_currShape.Width * num38 + 25000f);
				m_currShape.ShapeGuide.Add("adj", "val " + num39);
			}
			else
			{
				m_currShape.ShapeGuide.Add("adj", "val 25000");
			}
			break;
		case AutoShapeType.LeftBracket:
		case AutoShapeType.RightBracket:
			if (m_currShape.ShapeGuide.Count != 0)
			{
				using Dictionary<string, string>.Enumerator enumerator = m_currShape.ShapeGuide.GetEnumerator();
				if (enumerator.MoveNext())
				{
					double num77 = Math.Round((double)(float)int.Parse(enumerator.Current.Value) * 4.62962963);
					m_currShape.ShapeGuide.Clear();
					m_currShape.ShapeGuide.Add("adj", "val " + num77);
				}
				break;
			}
			if (m_currShape.Height > m_currShape.Width)
			{
				float num78 = m_currShape.Height - m_currShape.Width;
				double num79 = Math.Round(8333f / m_currShape.Width * num78 + 8333f);
				m_currShape.ShapeGuide.Add("adj", "val " + num79);
			}
			else
			{
				m_currShape.ShapeGuide.Add("adj", "val 8333");
			}
			break;
		case AutoShapeType.LeftBrace:
		case AutoShapeType.RightBrace:
			if (m_currShape.ShapeGuide.Count != 0)
			{
				double num107 = 0.0;
				double num108 = 50000.0;
				if (m_currShape.Height > m_currShape.Width)
				{
					foreach (KeyValuePair<string, string> item in m_currShape.ShapeGuide)
					{
						if (item.Key == "adjustValue")
						{
							float num109 = m_currShape.Height - m_currShape.Width;
							float num110 = int.Parse(item.Value);
							num107 = Math.Round((4.62962963 / (double)m_currShape.Width * (double)num109 + 4.62962963) * (double)num110);
						}
						else
						{
							num108 = Math.Round((double)(float)int.Parse(item.Value) * 4.62962963);
						}
					}
				}
				else
				{
					foreach (KeyValuePair<string, string> item2 in m_currShape.ShapeGuide)
					{
						double num111 = Math.Round((double)(float)int.Parse(item2.Value) * 4.62962963);
						if (item2.Key == "adjustValue")
						{
							num107 = num111;
						}
						else
						{
							num108 = num111;
						}
					}
				}
				m_currShape.ShapeGuide.Clear();
				m_currShape.ShapeGuide.Add("adj1", "val " + num107);
				m_currShape.ShapeGuide.Add("adj2", "val " + num108);
			}
			else
			{
				if (m_currShape.Height > m_currShape.Width)
				{
					float num112 = m_currShape.Height - m_currShape.Width;
					double num113 = Math.Round(8333f / m_currShape.Width * num112 + 8333f);
					m_currShape.ShapeGuide.Add("adj1", "val " + num113);
				}
				else
				{
					m_currShape.ShapeGuide.Add("adj1", "val 8333");
				}
				m_currShape.ShapeGuide.Add("adj2", "val 50000");
			}
			break;
		case AutoShapeType.Donut:
			if (m_currShape.ShapeGuide.Count != 0)
			{
				if (m_currShape.Height != m_currShape.Width)
				{
					break;
				}
				using Dictionary<string, string>.Enumerator enumerator = m_currShape.ShapeGuide.GetEnumerator();
				if (enumerator.MoveNext())
				{
					double num80 = Math.Round((double)(float)int.Parse(enumerator.Current.Value) * 4.63);
					m_currShape.ShapeGuide.Clear();
					m_currShape.ShapeGuide.Add("adj", "val " + num80);
				}
				break;
			}
			if (m_currShape.Height == m_currShape.Width)
			{
				m_currShape.ShapeGuide.Add("adj", "val 25000");
			}
			break;
		case AutoShapeType.NoSymbol:
			if (m_currShape.ShapeGuide.Count != 0)
			{
				if (m_currShape.Height != m_currShape.Width)
				{
					break;
				}
				using Dictionary<string, string>.Enumerator enumerator = m_currShape.ShapeGuide.GetEnumerator();
				if (enumerator.MoveNext())
				{
					double num47 = Math.Round((double)(float)int.Parse(enumerator.Current.Value) * 4.63);
					m_currShape.ShapeGuide.Clear();
					m_currShape.ShapeGuide.Add("adj", "val " + num47);
				}
				break;
			}
			if (m_currShape.Height == m_currShape.Width)
			{
				m_currShape.ShapeGuide.Add("adj", "val 12500");
			}
			break;
		case AutoShapeType.Star4Point:
			if (m_currShape.ShapeGuide.Count != 0)
			{
				using (Dictionary<string, string>.Enumerator enumerator = m_currShape.ShapeGuide.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						double num14 = Math.Round((double)(float)int.Parse(enumerator.Current.Value) * 4.62962963);
						double num15 = 50000.0 - num14;
						m_currShape.ShapeGuide.Clear();
						m_currShape.ShapeGuide.Add("adj", "val " + num15);
					}
					break;
				}
			}
			m_currShape.ShapeGuide.Add("adj", "val 12500");
			break;
		case AutoShapeType.Star8Point:
			if (m_currShape.ShapeGuide.Count != 0)
			{
				using (Dictionary<string, string>.Enumerator enumerator = m_currShape.ShapeGuide.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						double num12 = Math.Round((double)(float)int.Parse(enumerator.Current.Value) * 4.62962963);
						double num13 = 50000.0 - num12;
						m_currShape.ShapeGuide.Clear();
						m_currShape.ShapeGuide.Add("adj", "val " + num13);
					}
					break;
				}
			}
			m_currShape.ShapeGuide.Add("adj", "val 38250");
			break;
		case AutoShapeType.Star16Point:
		case AutoShapeType.Star24Point:
		case AutoShapeType.Star32Point:
			if (m_currShape.ShapeGuide.Count != 0)
			{
				using (Dictionary<string, string>.Enumerator enumerator = m_currShape.ShapeGuide.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						double num114 = Math.Round((double)(float)int.Parse(enumerator.Current.Value) * 4.62962963);
						double num115 = 50000.0 - num114;
						m_currShape.ShapeGuide.Clear();
						m_currShape.ShapeGuide.Add("adj", "val " + num115);
					}
					break;
				}
			}
			m_currShape.ShapeGuide.Add("adj", "val 37500");
			break;
		case AutoShapeType.UpRibbon:
			if (m_currShape.ShapeGuide.Count != 0)
			{
				double num90 = 12500.0;
				double num91 = 50000.0;
				foreach (KeyValuePair<string, string> item3 in m_currShape.ShapeGuide)
				{
					float num92 = int.Parse(item3.Value);
					if (item3.Key == "adjustValue")
					{
						double num93 = (double)Math.Abs(2700f - num92) * 9.25925926;
						num91 = Math.Round(75000.0 - num93);
					}
					else
					{
						num90 = Math.Round((double)(21600f - num92) * 4.62962963);
					}
				}
				m_currShape.ShapeGuide.Clear();
				m_currShape.ShapeGuide.Add("adj1", "val " + num90);
				m_currShape.ShapeGuide.Add("adj2", "val " + num91);
			}
			else
			{
				m_currShape.ShapeGuide.Add("adj1", "val 12500");
				m_currShape.ShapeGuide.Add("adj2", "val 50000");
			}
			break;
		case AutoShapeType.DownRibbon:
			if (m_currShape.ShapeGuide.Count != 0)
			{
				double num83 = 12500.0;
				double num84 = 50000.0;
				foreach (KeyValuePair<string, string> item4 in m_currShape.ShapeGuide)
				{
					float num85 = int.Parse(item4.Value);
					double num86 = (double)Math.Abs(2700f - num85) * 9.25925926;
					num86 = Math.Round(75000.0 - num86);
					if (item4.Key == "adjustValue")
					{
						num84 = num86;
					}
					else
					{
						num83 = num86;
					}
				}
				m_currShape.ShapeGuide.Clear();
				m_currShape.ShapeGuide.Add("adj1", "val " + num83);
				m_currShape.ShapeGuide.Add("adj2", "val " + num84);
			}
			else
			{
				m_currShape.ShapeGuide.Add("adj1", "val 12500");
				m_currShape.ShapeGuide.Add("adj2", "val 50000");
			}
			break;
		case AutoShapeType.CurvedUpRibbon:
			if (m_currShape.ShapeGuide.Count != 0)
			{
				double num48 = 25000.0;
				double num49 = 50000.0;
				double num50 = 12500.0;
				foreach (KeyValuePair<string, string> item5 in m_currShape.ShapeGuide)
				{
					float num51 = int.Parse(item5.Value);
					if (item5.Key == "adjustValue")
					{
						double num52 = (double)Math.Abs(2700f - num51) * 9.25925926;
						num49 = Math.Round(75000.0 - num52);
					}
					else if (item5.Key == "adjust2Value")
					{
						double num53 = (double)(num51 - 12600f) * 4.62962963;
						num48 = Math.Round(41667.0 - num53);
					}
					else
					{
						num50 = Math.Round((double)num51 * 4.62962963);
					}
				}
				m_currShape.ShapeGuide.Clear();
				m_currShape.ShapeGuide.Add("adj1", "val " + num48);
				m_currShape.ShapeGuide.Add("adj2", "val " + num49);
				m_currShape.ShapeGuide.Add("adj3", "val " + num50);
			}
			else
			{
				m_currShape.ShapeGuide.Add("adj1", "val 25000");
				m_currShape.ShapeGuide.Add("adj2", "val 50000");
				m_currShape.ShapeGuide.Add("adj3", "val 12500");
			}
			break;
		case AutoShapeType.CurvedDownRibbon:
			if (m_currShape.ShapeGuide.Count != 0)
			{
				double num27 = 25000.0;
				double num28 = 50000.0;
				double num29 = 12500.0;
				foreach (KeyValuePair<string, string> item6 in m_currShape.ShapeGuide)
				{
					float num30 = int.Parse(item6.Value);
					if (item6.Key == "adjustValue")
					{
						double num31 = (double)Math.Abs(2700f - num30) * 9.25925926;
						num28 = Math.Round(75000.0 - num31);
					}
					else if (item6.Key == "adjust2Value")
					{
						double num32 = (double)(9000f - num30) * 4.62962963;
						num27 = Math.Round(41667.0 - num32);
					}
					else
					{
						double num33 = (double)(20925f - num30) * 4.62962963;
						num29 = Math.Round(3125.0 + num33);
					}
				}
				m_currShape.ShapeGuide.Clear();
				m_currShape.ShapeGuide.Add("adj1", "val " + num27);
				m_currShape.ShapeGuide.Add("adj2", "val " + num28);
				m_currShape.ShapeGuide.Add("adj3", "val " + num29);
			}
			else
			{
				m_currShape.ShapeGuide.Add("adj1", "val 25000");
				m_currShape.ShapeGuide.Add("adj2", "val 50000");
				m_currShape.ShapeGuide.Add("adj3", "val 12500");
			}
			break;
		case AutoShapeType.VerticalScroll:
		case AutoShapeType.HorizontalScroll:
			if (m_currShape.ShapeGuide.Count != 0)
			{
				using (Dictionary<string, string>.Enumerator enumerator = m_currShape.ShapeGuide.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						double num11 = Math.Round((double)(float)int.Parse(enumerator.Current.Value) * 4.62962963);
						m_currShape.ShapeGuide.Clear();
						m_currShape.ShapeGuide.Add("adj", "val " + num11);
					}
					break;
				}
			}
			m_currShape.ShapeGuide.Add("adj", "val 12500");
			break;
		case AutoShapeType.Wave:
			if (m_currShape.ShapeGuide.Count != 0)
			{
				double num116 = 13005.0;
				double num117 = 0.0;
				foreach (KeyValuePair<string, string> item7 in m_currShape.ShapeGuide)
				{
					float num118 = int.Parse(item7.Value);
					if (item7.Key == "adjustValue")
					{
						num116 = Math.Round((double)num118 * 4.62962963);
						continue;
					}
					num117 = Math.Round((double)Math.Abs(10800f - num118) * 4.62962963);
					if (num118 < 10800f)
					{
						num117 = 0.0 - num117;
					}
				}
				m_currShape.ShapeGuide.Clear();
				m_currShape.ShapeGuide.Add("adj1", "val " + num116);
				m_currShape.ShapeGuide.Add("adj2", "val " + num117);
			}
			else
			{
				m_currShape.ShapeGuide.Add("adj1", "val 13005");
				m_currShape.ShapeGuide.Add("adj2", "val 0");
			}
			break;
		case AutoShapeType.DoubleWave:
			if (m_currShape.ShapeGuide.Count != 0)
			{
				double num102 = 13005.0;
				double num103 = 0.0;
				foreach (KeyValuePair<string, string> item8 in m_currShape.ShapeGuide)
				{
					float num104 = int.Parse(item8.Value);
					if (item8.Key == "adjustValue")
					{
						num102 = Math.Round((double)num104 * 4.62962963);
						continue;
					}
					num103 = Math.Round((double)Math.Abs(10800f - num104) * 4.62962963);
					if (num104 < 10800f)
					{
						num103 = 0.0 - num103;
					}
				}
				m_currShape.ShapeGuide.Clear();
				m_currShape.ShapeGuide.Add("adj1", "val " + num102);
				m_currShape.ShapeGuide.Add("adj2", "val " + num103);
			}
			else
			{
				m_currShape.ShapeGuide.Add("adj1", "val 6500");
				m_currShape.ShapeGuide.Add("adj2", "val 0");
			}
			break;
		case AutoShapeType.RectangularCallout:
		case AutoShapeType.OvalCallout:
		case AutoShapeType.CloudCallout:
			if (m_currShape.ShapeGuide.Count != 0)
			{
				double num87 = -43570.0;
				double num88 = 70000.0;
				foreach (KeyValuePair<string, string> item9 in m_currShape.ShapeGuide)
				{
					double num89 = Math.Round((double)(float)int.Parse(item9.Value) * 4.62962963);
					num89 -= 50000.0;
					if (item9.Key == "adjustValue")
					{
						num87 = num89;
					}
					else
					{
						num88 = num89;
					}
				}
				m_currShape.ShapeGuide.Clear();
				m_currShape.ShapeGuide.Add("adj1", "val " + num87);
				m_currShape.ShapeGuide.Add("adj2", "val " + num88);
			}
			else
			{
				m_currShape.ShapeGuide.Add("adj1", "val -43750");
				m_currShape.ShapeGuide.Add("adj2", "val 70000");
			}
			break;
		case AutoShapeType.RoundedRectangularCallout:
			if (m_currShape.ShapeGuide.Count != 0)
			{
				double num71 = -43570.0;
				double num72 = 70000.0;
				foreach (KeyValuePair<string, string> item10 in m_currShape.ShapeGuide)
				{
					double num73 = Math.Round((double)(float)int.Parse(item10.Value) * 4.62962963);
					num73 -= 50000.0;
					if (item10.Key == "adjustValue")
					{
						num71 = num73;
					}
					else
					{
						num72 = num73;
					}
				}
				m_currShape.ShapeGuide.Clear();
				m_currShape.ShapeGuide.Add("adj1", "val " + num71);
				m_currShape.ShapeGuide.Add("adj2", "val " + num72);
			}
			else
			{
				m_currShape.ShapeGuide.Add("adj1", "val -43750");
				m_currShape.ShapeGuide.Add("adj2", "val 70000");
			}
			m_currShape.ShapeGuide.Add("adj3", "val 16667");
			break;
		case AutoShapeType.LineCallout1:
		case AutoShapeType.LineCallout1NoBorder:
		case AutoShapeType.LineCallout1AccentBar:
		case AutoShapeType.LineCallout1BorderAndAccentBar:
			if (m_currShape.ShapeGuide.Count != 0)
			{
				double num56 = 0.0;
				double num57 = -8333.0;
				double num58 = 0.0;
				double num59 = -8333.0;
				foreach (KeyValuePair<string, string> item11 in m_currShape.ShapeGuide)
				{
					double num60 = Math.Round((double)(float)int.Parse(item11.Value) * 4.62962963);
					if (item11.Key == "adjustValue")
					{
						num59 = num60;
					}
					else if (item11.Key == "adjust2Value")
					{
						num58 = num60;
					}
					else if (item11.Key == "adjust3Value")
					{
						num57 = num60;
					}
					else if (item11.Key == "adjust4Value")
					{
						num56 = num60;
					}
				}
				m_currShape.ShapeGuide.Clear();
				m_currShape.ShapeGuide.Add("adj1", "val " + num56);
				m_currShape.ShapeGuide.Add("adj2", "val " + num57);
				m_currShape.ShapeGuide.Add("adj3", "val " + num58);
				m_currShape.ShapeGuide.Add("adj4", "val " + num59);
			}
			else
			{
				m_currShape.ShapeGuide.Add("adj1", "val 18750");
				m_currShape.ShapeGuide.Add("adj2", "val -8333");
				m_currShape.ShapeGuide.Add("adj3", "val 112500");
				m_currShape.ShapeGuide.Add("adj4", "val -38333");
			}
			break;
		case AutoShapeType.LineCallout2:
		case AutoShapeType.LineCallout2AccentBar:
		case AutoShapeType.LineCallout2NoBorder:
		case AutoShapeType.LineCallout2BorderAndAccentBar:
			if (m_currShape.ShapeGuide.Count != 0)
			{
				double num40 = 18750.0;
				double num41 = -8333.0;
				double num42 = 18750.0;
				double num43 = -16667.0;
				double num44 = 112500.0;
				double num45 = -46667.0;
				foreach (KeyValuePair<string, string> item12 in m_currShape.ShapeGuide)
				{
					double num46 = Math.Round((double)(float)int.Parse(item12.Value) * 4.62962963);
					if (item12.Key == "adjustValue")
					{
						num45 = num46;
					}
					else if (item12.Key == "adjust2Value")
					{
						num44 = num46;
					}
					else if (item12.Key == "adjust3Value")
					{
						num43 = num46;
					}
					else if (item12.Key == "adjust4Value")
					{
						num42 = num46;
					}
					else if (item12.Key == "adjust5Value")
					{
						num41 = num46;
					}
					else if (item12.Key == "adjust6Value")
					{
						num40 = num46;
					}
				}
				m_currShape.ShapeGuide.Clear();
				m_currShape.ShapeGuide.Add("adj1", "val " + num40);
				m_currShape.ShapeGuide.Add("adj2", "val " + num41);
				m_currShape.ShapeGuide.Add("adj3", "val " + num42);
				m_currShape.ShapeGuide.Add("adj4", "val " + num43);
				m_currShape.ShapeGuide.Add("adj5", "val " + num44);
				m_currShape.ShapeGuide.Add("adj6", "val " + num45);
			}
			else
			{
				m_currShape.ShapeGuide.Add("adj1", "val 18750");
				m_currShape.ShapeGuide.Add("adj2", "val -8333");
				m_currShape.ShapeGuide.Add("adj3", "val 18750");
				m_currShape.ShapeGuide.Add("adj4", "val -16667");
				m_currShape.ShapeGuide.Add("adj5", "val 112500");
				m_currShape.ShapeGuide.Add("adj6", "val -46667");
			}
			break;
		case AutoShapeType.LineCallout3:
		case AutoShapeType.LineCallout3AccentBar:
		case AutoShapeType.LineCallout3NoBorder:
		case AutoShapeType.LineCallout3BorderAndAccentBar:
		{
			if (m_currShape.ShapeGuide.Count == 0)
			{
				break;
			}
			double num18 = 18750.0;
			double num19 = 108333.0;
			double num20 = 18750.0;
			double num21 = 116667.0;
			double num22 = 100000.0;
			double num23 = 116667.0;
			double num24 = 112917.0;
			double num25 = 108333.0;
			foreach (KeyValuePair<string, string> item13 in m_currShape.ShapeGuide)
			{
				double num26 = Math.Round((double)(float)int.Parse(item13.Value) * 4.62962963);
				if (item13.Key == "adjustValue")
				{
					num25 = num26;
				}
				else if (item13.Key == "adjust2Value")
				{
					num24 = num26;
				}
				else if (item13.Key == "adjust3Value")
				{
					num23 = num26;
				}
				else if (item13.Key == "adjust4Value")
				{
					num22 = num26;
				}
				else if (item13.Key == "adjust5Value")
				{
					num21 = num26;
				}
				else if (item13.Key == "adjust6Value")
				{
					num20 = num26;
				}
				else if (item13.Key == "adjust7Value")
				{
					num19 = num26;
				}
				else if (item13.Key == "adjust8Value")
				{
					num18 = num26;
				}
			}
			m_currShape.ShapeGuide.Clear();
			m_currShape.ShapeGuide.Add("adj1", "val " + num18);
			m_currShape.ShapeGuide.Add("adj2", "val " + num19);
			m_currShape.ShapeGuide.Add("adj3", "val " + num20);
			m_currShape.ShapeGuide.Add("adj4", "val " + num21);
			m_currShape.ShapeGuide.Add("adj5", "val " + num22);
			m_currShape.ShapeGuide.Add("adj6", "val " + num23);
			m_currShape.ShapeGuide.Add("adj7", "val " + num24);
			m_currShape.ShapeGuide.Add("adj8", "val " + num25);
			break;
		}
		case AutoShapeType.RightArrow:
		case AutoShapeType.DownArrow:
			if (m_currShape.ShapeGuide.Count != 0)
			{
				double num = 50000.0;
				double num2 = 25000.0;
				foreach (KeyValuePair<string, string> item14 in m_currShape.ShapeGuide)
				{
					float num3 = int.Parse(item14.Value);
					if (m_currShape.Width > m_currShape.Height)
					{
						if (item14.Key == "adjustValue")
						{
							_ = m_currShape.Width;
							_ = m_currShape.Height;
							double num4 = 100000f + 100000f / m_currShape.Height;
							double num5 = (double)num3 * 4.62962963;
							num2 = Math.Round(num4 + num5);
						}
						else
						{
							num = Math.Round(100000.0 - (double)num3 * 9.25925924);
						}
					}
					else if (item14.Key == "adjustValue")
					{
						num2 = Math.Round(100000.0 - (double)num3 * 4.62962963);
					}
					else
					{
						num = Math.Round(100000.0 - (double)num3 * 9.25925924);
					}
				}
				m_currShape.ShapeGuide.Clear();
				m_currShape.ShapeGuide.Add("adj1", "val " + num);
				m_currShape.ShapeGuide.Add("adj2", "val " + num2);
			}
			else
			{
				double num6 = 50000.0;
				double num7 = 25000.0;
				if (m_currShape.Width > m_currShape.Height)
				{
					float num8 = m_currShape.Width - m_currShape.Height;
					num7 = Math.Round(25000f / m_currShape.Height * num8 + 25000f);
				}
				m_currShape.ShapeGuide.Add("adj1", "val " + num6);
				m_currShape.ShapeGuide.Add("adj2", "val " + num7);
			}
			break;
		}
	}

	private void AddOwnerShapeTextStack()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("m_currShape", m_currShape);
		dictionary.Add("m_currTextBox", m_currTextBox);
		Stack<string> stack = new Stack<string>();
		string[] array = new string[m_pictureOrShapeStack.Count];
		m_pictureOrShapeStack.CopyTo(array, 0);
		for (int num = array.Length - 1; num >= 0; num--)
		{
			stack.Push(array[num]);
		}
		dictionary.Add("m_pictureOrShapeStack", stack);
		m_ownerShapeTextbodyStack.Push(dictionary);
		m_pictureOrShapeStack.Clear();
	}

	private void AddShapeTextbodyStack()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("m_currParagraph", (m_currParagraph == null) ? null : m_currParagraph);
		dictionary.Add("isPardTagpresent", isPardTagpresent);
		dictionary.Add("m_bIsPictureOrShape", m_bIsPictureOrShape);
		dictionary.Add("m_bInTable", m_bInTable);
		dictionary.Add("m_previousLevel", m_previousLevel);
		dictionary.Add("m_currentLevel", m_currentLevel);
		dictionary.Add("m_bIsRow", m_bIsRow);
		dictionary.Add("m_textFormatStack", (m_textFormatStack == null) ? null : new Stack<TextFormat>(m_textFormatStack.ToArray()));
		m_shapeTextbodyStack.Push(dictionary);
	}

	private void ClearPreviousTextbody()
	{
		m_currParagraph = null;
		isPardTagpresent = false;
		m_textFormatStack.Clear();
	}

	private void ResetOwnerShapeStack()
	{
		Dictionary<string, object> dictionary = m_ownerShapeTextbodyStack.Pop();
		m_currShape = dictionary["m_currShape"] as Shape;
		m_currTextBox = dictionary["m_currTextBox"] as WTextBox;
		Stack<string> stack = dictionary["m_pictureOrShapeStack"] as Stack<string>;
		string[] array = new string[stack.Count];
		stack.CopyTo(array, 0);
		for (int num = array.Length - 1; num >= 0; num--)
		{
			m_pictureOrShapeStack.Push(array[num]);
		}
		stack.Clear();
	}

	private void ResetShapeTextbodyStack()
	{
		Dictionary<string, object> dictionary = m_shapeTextbodyStack.Pop();
		m_currParagraph = dictionary["m_currParagraph"] as IWParagraph;
		isPardTagpresent = (bool)dictionary["isPardTagpresent"];
		m_bIsPictureOrShape = (bool)dictionary["m_bIsPictureOrShape"];
		m_previousLevel = (int)dictionary["m_previousLevel"];
		m_currentLevel = (int)dictionary["m_currentLevel"];
		m_bInTable = (bool)dictionary["m_bInTable"];
		m_bIsRow = (bool)dictionary["m_bIsRow"];
		m_textFormatStack = new Stack<TextFormat>((dictionary["m_textFormatStack"] as Stack<TextFormat>).ToArray());
	}

	private void ParsePictureToken(string token, string tokenKey, string tokenValue)
	{
		if (m_bIsShapePicture && !m_bIsGroupShape)
		{
			switch (tokenKey)
			{
			case "picscalex":
				m_picFormat.WidthScale = Convert.ToSingle(tokenValue);
				break;
			case "picscaley":
				m_picFormat.HeightScale = Convert.ToSingle(tokenValue);
				break;
			case "picwgoal":
				m_picFormat.Width = ExtractTwipsValue(tokenValue);
				break;
			case "pichgoal":
				m_picFormat.Height = ExtractTwipsValue(tokenValue);
				break;
			case "picw":
				m_picFormat.PicW = GetIntValue(tokenValue);
				break;
			case "pich":
				m_picFormat.picH = GetIntValue(tokenValue);
				break;
			case "rotation":
				m_picFormat.Rotation = tokenValue;
				break;
			case "object":
				m_bIsObject = true;
				m_objectStack.Push("\\");
				break;
			}
			if ((tokenKey == "bin" || m_previousTokenKey == "bin") && tokenValue != null)
			{
				AppendPictureToParagraph(token);
			}
		}
	}

	private int GetIntValue(string tokenValue)
	{
		int.TryParse(tokenValue, out var result);
		return result;
	}

	private void ParsePageVerticalAlignment(string token, string tokenKey, string tokenValue)
	{
		switch (tokenKey)
		{
		case "vertal":
		case "vertalb":
			m_secFormat.VertAlignment = PageAlignment.Bottom;
			break;
		case "vertalt":
			m_secFormat.VertAlignment = PageAlignment.Top;
			break;
		case "vertalc":
			m_secFormat.VertAlignment = PageAlignment.Middle;
			break;
		case "vertalj":
			m_secFormat.VertAlignment = PageAlignment.Justified;
			break;
		}
	}

	private void ParseOutLineLevel(string token, string tokenKey, string tokenValue)
	{
		switch (Convert.ToInt32(tokenValue))
		{
		case 0:
			m_currParagraphFormat.OutlineLevel = OutlineLevel.Level1;
			break;
		case 1:
			m_currParagraphFormat.OutlineLevel = OutlineLevel.Level2;
			break;
		case 2:
			m_currParagraphFormat.OutlineLevel = OutlineLevel.Level3;
			break;
		case 3:
			m_currParagraphFormat.OutlineLevel = OutlineLevel.Level4;
			break;
		case 4:
			m_currParagraphFormat.OutlineLevel = OutlineLevel.Level5;
			break;
		case 5:
			m_currParagraphFormat.OutlineLevel = OutlineLevel.Level6;
			break;
		case 6:
			m_currParagraphFormat.OutlineLevel = OutlineLevel.Level7;
			break;
		case 7:
			m_currParagraphFormat.OutlineLevel = OutlineLevel.Level8;
			break;
		case 8:
			m_currParagraphFormat.OutlineLevel = OutlineLevel.Level9;
			break;
		default:
			m_currParagraphFormat.OutlineLevel = OutlineLevel.BodyText;
			break;
		}
	}

	private void ParseParagraphBorders(string token, string tokenKey, string tokenValue)
	{
		switch (tokenKey)
		{
		case "brdrtbl":
			if (m_bIsRow)
			{
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.BorderType = BorderStyle.Cleared;
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.BorderType = BorderStyle.Cleared;
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.BorderType = BorderStyle.Cleared;
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.BorderType = BorderStyle.Cleared;
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.BorderType = BorderStyle.Cleared;
				}
			}
			break;
		case "brdrt":
			m_bIsBorderTop = true;
			m_bIsBorderBottom = false;
			m_bIsBorderLeft = false;
			m_bIsBorderRight = false;
			break;
		case "brdrb":
			m_bIsBorderBottom = true;
			m_bIsBorderLeft = false;
			m_bIsBorderRight = false;
			m_bIsBorderTop = false;
			break;
		case "brdrl":
			m_bIsBorderLeft = true;
			m_bIsBorderRight = false;
			m_bIsBorderTop = false;
			m_bIsBorderBottom = false;
			break;
		case "brdrr":
			m_bIsBorderRight = true;
			m_bIsBorderBottom = false;
			m_bIsBorderLeft = false;
			m_bIsBorderTop = false;
			break;
		case "box":
			m_bIsBorderTop = true;
			m_bIsBorderBottom = true;
			m_bIsBorderLeft = true;
			m_bIsBorderRight = true;
			break;
		case "brdrsh":
			if (m_bIsRow)
			{
				if (m_bIsHorizontalBorder)
				{
					CurrRowFormat.Borders.Horizontal.Shadow = true;
				}
				if (m_bIsVerticalBorder)
				{
					CurrRowFormat.Borders.Vertical.Shadow = true;
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.Shadow = true;
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.Shadow = true;
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.Shadow = true;
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.Shadow = true;
				}
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.Shadow = true;
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.Shadow = true;
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.Shadow = true;
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.Shadow = true;
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.Shadow = true;
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.Shadow = true;
				}
			}
			else
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.Shadow = true;
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.Shadow = true;
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.Shadow = true;
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.Shadow = true;
				}
			}
			break;
		case "brdrs":
			if (m_bIsRow && !m_previousToken.StartsWith("brdrt") && !m_previousToken.StartsWith("brdrr") && !m_previousToken.StartsWith("brdrl") && !m_previousToken.StartsWith("brdrb"))
			{
				if (m_bIsHorizontalBorder)
				{
					CurrRowFormat.Borders.Horizontal.BorderType = BorderStyle.Single;
				}
				if (m_bIsVerticalBorder)
				{
					CurrRowFormat.Borders.Vertical.BorderType = BorderStyle.Single;
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.BorderType = BorderStyle.Single;
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.BorderType = BorderStyle.Single;
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.BorderType = BorderStyle.Single;
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.BorderType = BorderStyle.Single;
				}
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.BorderType = BorderStyle.Single;
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.BorderType = BorderStyle.Single;
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.BorderType = BorderStyle.Single;
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.BorderType = BorderStyle.Single;
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.BorderType = BorderStyle.Single;
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.BorderType = BorderStyle.Single;
				}
			}
			else
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.BorderType = BorderStyle.Single;
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.BorderType = BorderStyle.Single;
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.BorderType = BorderStyle.Single;
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.BorderType = BorderStyle.Single;
				}
			}
			break;
		case "brdrth":
			if (m_bIsRow)
			{
				if (m_bIsHorizontalBorder)
				{
					CurrRowFormat.Borders.Horizontal.BorderType = BorderStyle.Thick;
				}
				if (m_bIsVerticalBorder)
				{
					CurrRowFormat.Borders.Vertical.BorderType = BorderStyle.Thick;
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.BorderType = BorderStyle.Thick;
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.BorderType = BorderStyle.Thick;
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.BorderType = BorderStyle.Thick;
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.BorderType = BorderStyle.Thick;
				}
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.BorderType = BorderStyle.Thick;
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.BorderType = BorderStyle.Thick;
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.BorderType = BorderStyle.Thick;
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.BorderType = BorderStyle.Thick;
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.BorderType = BorderStyle.Thick;
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.BorderType = BorderStyle.Thick;
				}
			}
			else
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.BorderType = BorderStyle.Thick;
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.BorderType = BorderStyle.Thick;
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.BorderType = BorderStyle.Thick;
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.BorderType = BorderStyle.Thick;
				}
			}
			break;
		case "brdrdb":
			if (m_bIsRow)
			{
				if (m_bIsHorizontalBorder)
				{
					CurrRowFormat.Borders.Horizontal.BorderType = BorderStyle.Double;
				}
				if (m_bIsVerticalBorder)
				{
					CurrRowFormat.Borders.Vertical.BorderType = BorderStyle.Double;
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.BorderType = BorderStyle.Double;
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.BorderType = BorderStyle.Double;
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.BorderType = BorderStyle.Double;
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.BorderType = BorderStyle.Double;
				}
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.BorderType = BorderStyle.Double;
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.BorderType = BorderStyle.Double;
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.BorderType = BorderStyle.Double;
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.BorderType = BorderStyle.Double;
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.BorderType = BorderStyle.Double;
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.BorderType = BorderStyle.Double;
				}
			}
			else
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.BorderType = BorderStyle.Double;
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.BorderType = BorderStyle.Double;
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.BorderType = BorderStyle.Double;
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.BorderType = BorderStyle.Double;
				}
			}
			break;
		case "brdrdot":
			if (m_bIsRow)
			{
				if (m_bIsHorizontalBorder)
				{
					CurrRowFormat.Borders.Horizontal.BorderType = BorderStyle.Dot;
				}
				if (m_bIsVerticalBorder)
				{
					CurrRowFormat.Borders.Vertical.BorderType = BorderStyle.Dot;
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.BorderType = BorderStyle.Dot;
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.BorderType = BorderStyle.Dot;
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.BorderType = BorderStyle.Dot;
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.BorderType = BorderStyle.Dot;
				}
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.BorderType = BorderStyle.Dot;
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.BorderType = BorderStyle.Dot;
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.BorderType = BorderStyle.Dot;
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.BorderType = BorderStyle.Dot;
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.BorderType = BorderStyle.Dot;
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.BorderType = BorderStyle.Dot;
				}
			}
			else
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.BorderType = BorderStyle.Dot;
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.BorderType = BorderStyle.Dot;
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.BorderType = BorderStyle.Dot;
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.BorderType = BorderStyle.Dot;
				}
			}
			break;
		case "brdrdashsm":
			if (m_bIsRow)
			{
				if (m_bIsHorizontalBorder)
				{
					CurrRowFormat.Borders.Horizontal.BorderType = BorderStyle.DashSmallGap;
				}
				if (m_bIsVerticalBorder)
				{
					CurrRowFormat.Borders.Vertical.BorderType = BorderStyle.DashSmallGap;
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.BorderType = BorderStyle.DashSmallGap;
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.BorderType = BorderStyle.DashSmallGap;
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.BorderType = BorderStyle.DashSmallGap;
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.BorderType = BorderStyle.DashSmallGap;
				}
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.BorderType = BorderStyle.DashSmallGap;
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.BorderType = BorderStyle.DashSmallGap;
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.BorderType = BorderStyle.DashSmallGap;
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.BorderType = BorderStyle.DashSmallGap;
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.BorderType = BorderStyle.DashSmallGap;
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.BorderType = BorderStyle.DashSmallGap;
				}
			}
			else
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.BorderType = BorderStyle.DashSmallGap;
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.BorderType = BorderStyle.DashSmallGap;
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.BorderType = BorderStyle.DashSmallGap;
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.BorderType = BorderStyle.DashSmallGap;
				}
			}
			break;
		case "brdrdash":
			if (m_bIsRow)
			{
				if (m_bIsHorizontalBorder)
				{
					CurrRowFormat.Borders.Horizontal.BorderType = BorderStyle.DashLargeGap;
				}
				if (m_bIsVerticalBorder)
				{
					CurrRowFormat.Borders.Vertical.BorderType = BorderStyle.DashLargeGap;
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.BorderType = BorderStyle.DashLargeGap;
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.BorderType = BorderStyle.DashLargeGap;
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.BorderType = BorderStyle.DashLargeGap;
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.BorderType = BorderStyle.DashLargeGap;
				}
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.BorderType = BorderStyle.DashLargeGap;
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.BorderType = BorderStyle.DashLargeGap;
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.BorderType = BorderStyle.DashLargeGap;
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.BorderType = BorderStyle.DashLargeGap;
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.BorderType = BorderStyle.DashLargeGap;
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.BorderType = BorderStyle.DashLargeGap;
				}
			}
			else
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.BorderType = BorderStyle.DashLargeGap;
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.BorderType = BorderStyle.DashLargeGap;
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.BorderType = BorderStyle.DashLargeGap;
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.BorderType = BorderStyle.DashLargeGap;
				}
			}
			break;
		case "brdrdashdd":
			if (m_bIsRow)
			{
				if (m_bIsHorizontalBorder)
				{
					CurrRowFormat.Borders.Horizontal.BorderType = BorderStyle.DotDotDash;
				}
				if (m_bIsVerticalBorder)
				{
					CurrRowFormat.Borders.Vertical.BorderType = BorderStyle.DotDotDash;
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.BorderType = BorderStyle.DotDotDash;
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.BorderType = BorderStyle.DotDotDash;
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.BorderType = BorderStyle.DotDotDash;
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.BorderType = BorderStyle.DotDotDash;
				}
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.BorderType = BorderStyle.DotDotDash;
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.BorderType = BorderStyle.DotDotDash;
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.BorderType = BorderStyle.DotDotDash;
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.BorderType = BorderStyle.DotDotDash;
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.BorderType = BorderStyle.DotDotDash;
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.BorderType = BorderStyle.DotDotDash;
				}
			}
			else
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.BorderType = BorderStyle.DotDotDash;
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.BorderType = BorderStyle.DotDotDash;
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.BorderType = BorderStyle.DotDotDash;
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.BorderType = BorderStyle.DotDotDash;
				}
			}
			break;
		case "brdrtnthmg":
			if (m_bIsRow)
			{
				if (m_bIsHorizontalBorder)
				{
					CurrRowFormat.Borders.Horizontal.BorderType = BorderStyle.ThickThinMediumGap;
				}
				if (m_bIsVerticalBorder)
				{
					CurrRowFormat.Borders.Vertical.BorderType = BorderStyle.ThickThinMediumGap;
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.BorderType = BorderStyle.ThickThinMediumGap;
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.BorderType = BorderStyle.ThickThinMediumGap;
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.BorderType = BorderStyle.ThickThinMediumGap;
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.BorderType = BorderStyle.ThickThinMediumGap;
				}
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.BorderType = BorderStyle.ThickThinMediumGap;
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.BorderType = BorderStyle.ThickThinMediumGap;
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.BorderType = BorderStyle.ThickThinMediumGap;
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.BorderType = BorderStyle.ThickThinMediumGap;
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.BorderType = BorderStyle.ThickThinMediumGap;
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.BorderType = BorderStyle.ThickThinMediumGap;
				}
			}
			else
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.BorderType = BorderStyle.ThickThinMediumGap;
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.BorderType = BorderStyle.ThickThinMediumGap;
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.BorderType = BorderStyle.ThickThinMediumGap;
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.BorderType = BorderStyle.ThickThinMediumGap;
				}
			}
			break;
		case "brdrtnthsg":
			if (m_bIsRow)
			{
				if (m_bIsHorizontalBorder)
				{
					CurrRowFormat.Borders.Horizontal.BorderType = BorderStyle.ThinThinSmallGap;
				}
				if (m_bIsVerticalBorder)
				{
					CurrRowFormat.Borders.Vertical.BorderType = BorderStyle.ThinThinSmallGap;
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.BorderType = BorderStyle.ThinThinSmallGap;
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.BorderType = BorderStyle.ThinThinSmallGap;
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.BorderType = BorderStyle.ThinThinSmallGap;
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.BorderType = BorderStyle.ThinThinSmallGap;
				}
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.BorderType = BorderStyle.ThinThinSmallGap;
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.BorderType = BorderStyle.ThinThinSmallGap;
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.BorderType = BorderStyle.ThinThinSmallGap;
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.BorderType = BorderStyle.ThinThinSmallGap;
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.BorderType = BorderStyle.ThinThinSmallGap;
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.BorderType = BorderStyle.ThinThinSmallGap;
				}
			}
			else
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.BorderType = BorderStyle.ThinThinSmallGap;
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.BorderType = BorderStyle.ThinThinSmallGap;
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.BorderType = BorderStyle.ThinThinSmallGap;
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.BorderType = BorderStyle.ThinThinSmallGap;
				}
			}
			break;
		case "brdrtnthtnsg":
			if (m_bIsRow)
			{
				if (m_bIsHorizontalBorder)
				{
					CurrRowFormat.Borders.Horizontal.BorderType = BorderStyle.ThinThickThinSmallGap;
				}
				if (m_bIsVerticalBorder)
				{
					CurrRowFormat.Borders.Vertical.BorderType = BorderStyle.ThinThickThinSmallGap;
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.BorderType = BorderStyle.ThinThickThinSmallGap;
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.BorderType = BorderStyle.ThinThickThinSmallGap;
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.BorderType = BorderStyle.ThinThickThinSmallGap;
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.BorderType = BorderStyle.ThinThickThinSmallGap;
				}
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.BorderType = BorderStyle.ThinThickThinSmallGap;
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.BorderType = BorderStyle.ThinThickThinSmallGap;
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.BorderType = BorderStyle.ThinThickThinSmallGap;
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.BorderType = BorderStyle.ThinThickThinSmallGap;
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.BorderType = BorderStyle.ThinThickThinSmallGap;
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.BorderType = BorderStyle.ThinThickThinSmallGap;
				}
			}
			else
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.BorderType = BorderStyle.ThinThickThinSmallGap;
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.BorderType = BorderStyle.ThinThickThinSmallGap;
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.BorderType = BorderStyle.ThinThickThinSmallGap;
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.BorderType = BorderStyle.ThinThickThinSmallGap;
				}
			}
			break;
		case "brdrthtnmg":
			if (m_bIsRow)
			{
				if (m_bIsHorizontalBorder)
				{
					CurrRowFormat.Borders.Horizontal.BorderType = BorderStyle.ThickThinMediumGap;
				}
				if (m_bIsVerticalBorder)
				{
					CurrRowFormat.Borders.Vertical.BorderType = BorderStyle.ThickThinMediumGap;
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.BorderType = BorderStyle.ThinThickMediumGap;
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.BorderType = BorderStyle.ThinThickMediumGap;
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.BorderType = BorderStyle.ThinThickMediumGap;
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.BorderType = BorderStyle.ThinThickMediumGap;
				}
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.BorderType = BorderStyle.ThinThickMediumGap;
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.BorderType = BorderStyle.ThinThickMediumGap;
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.BorderType = BorderStyle.ThinThickMediumGap;
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.BorderType = BorderStyle.ThinThickMediumGap;
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.BorderType = BorderStyle.ThinThickMediumGap;
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.BorderType = BorderStyle.ThinThickMediumGap;
				}
			}
			else
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.BorderType = BorderStyle.ThinThickMediumGap;
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.BorderType = BorderStyle.ThinThickMediumGap;
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.BorderType = BorderStyle.ThinThickMediumGap;
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.BorderType = BorderStyle.ThinThickMediumGap;
				}
			}
			break;
		case "brdrtnthlg":
			if (m_bIsRow)
			{
				if (m_bIsHorizontalBorder)
				{
					CurrRowFormat.Borders.Horizontal.BorderType = BorderStyle.ThickThinLargeGap;
				}
				if (m_bIsVerticalBorder)
				{
					CurrRowFormat.Borders.Vertical.BorderType = BorderStyle.ThickThinLargeGap;
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.BorderType = BorderStyle.ThickThinLargeGap;
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.BorderType = BorderStyle.ThickThinLargeGap;
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.BorderType = BorderStyle.ThickThinLargeGap;
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.BorderType = BorderStyle.ThickThinLargeGap;
				}
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.BorderType = BorderStyle.ThickThinLargeGap;
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.BorderType = BorderStyle.ThickThinLargeGap;
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.BorderType = BorderStyle.ThickThinLargeGap;
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.BorderType = BorderStyle.ThickThinLargeGap;
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.BorderType = BorderStyle.ThickThinLargeGap;
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.BorderType = BorderStyle.ThickThinLargeGap;
				}
			}
			else
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.BorderType = BorderStyle.ThickThinLargeGap;
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.BorderType = BorderStyle.ThickThinLargeGap;
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.BorderType = BorderStyle.ThickThinLargeGap;
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.BorderType = BorderStyle.ThickThinLargeGap;
				}
			}
			break;
		case "brdrthtnlg":
			if (m_bIsRow)
			{
				if (m_bIsHorizontalBorder)
				{
					CurrRowFormat.Borders.Horizontal.BorderType = BorderStyle.ThinThickLargeGap;
				}
				if (m_bIsVerticalBorder)
				{
					CurrRowFormat.Borders.Vertical.BorderType = BorderStyle.ThinThickLargeGap;
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.BorderType = BorderStyle.ThinThickLargeGap;
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.BorderType = BorderStyle.ThinThickLargeGap;
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.BorderType = BorderStyle.ThinThickLargeGap;
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.BorderType = BorderStyle.ThinThickLargeGap;
				}
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.BorderType = BorderStyle.ThinThickLargeGap;
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.BorderType = BorderStyle.ThinThickLargeGap;
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.BorderType = BorderStyle.ThinThickLargeGap;
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.BorderType = BorderStyle.ThinThickLargeGap;
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.BorderType = BorderStyle.ThinThickLargeGap;
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.BorderType = BorderStyle.ThinThickLargeGap;
				}
			}
			else
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.BorderType = BorderStyle.ThinThickLargeGap;
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.BorderType = BorderStyle.ThinThickLargeGap;
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.BorderType = BorderStyle.ThinThickLargeGap;
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.BorderType = BorderStyle.ThinThickLargeGap;
				}
			}
			break;
		case "brdremboss":
			if (m_bIsRow)
			{
				if (m_bIsHorizontalBorder)
				{
					CurrRowFormat.Borders.Horizontal.BorderType = BorderStyle.Emboss3D;
				}
				if (m_bIsVerticalBorder)
				{
					CurrRowFormat.Borders.Vertical.BorderType = BorderStyle.Emboss3D;
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.BorderType = BorderStyle.Emboss3D;
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.BorderType = BorderStyle.Emboss3D;
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.BorderType = BorderStyle.Emboss3D;
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.BorderType = BorderStyle.Emboss3D;
				}
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.BorderType = BorderStyle.Emboss3D;
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.BorderType = BorderStyle.Emboss3D;
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.BorderType = BorderStyle.Emboss3D;
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.BorderType = BorderStyle.Emboss3D;
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.BorderType = BorderStyle.Emboss3D;
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.BorderType = BorderStyle.Emboss3D;
				}
			}
			else
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.BorderType = BorderStyle.Emboss3D;
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.BorderType = BorderStyle.Emboss3D;
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.BorderType = BorderStyle.Emboss3D;
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.BorderType = BorderStyle.Emboss3D;
				}
			}
			break;
		case "brdrengrave":
			if (m_bIsRow)
			{
				if (m_bIsHorizontalBorder)
				{
					CurrRowFormat.Borders.Horizontal.BorderType = BorderStyle.Engrave3D;
				}
				if (m_bIsVerticalBorder)
				{
					CurrRowFormat.Borders.Vertical.BorderType = BorderStyle.Engrave3D;
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.BorderType = BorderStyle.Engrave3D;
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.BorderType = BorderStyle.Engrave3D;
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.BorderType = BorderStyle.Engrave3D;
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.BorderType = BorderStyle.Engrave3D;
				}
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.BorderType = BorderStyle.Engrave3D;
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.BorderType = BorderStyle.Engrave3D;
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.BorderType = BorderStyle.Engrave3D;
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.BorderType = BorderStyle.Engrave3D;
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.BorderType = BorderStyle.Engrave3D;
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.BorderType = BorderStyle.Engrave3D;
				}
			}
			else
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.BorderType = BorderStyle.Engrave3D;
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.BorderType = BorderStyle.Engrave3D;
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.BorderType = BorderStyle.Engrave3D;
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.BorderType = BorderStyle.Engrave3D;
				}
			}
			break;
		case "brdrnone":
		case "brdrnil":
			if (m_bIsRow)
			{
				if (m_bIsHorizontalBorder)
				{
					CurrRowFormat.Borders.Horizontal.BorderType = BorderStyle.Cleared;
				}
				if (m_bIsVerticalBorder)
				{
					CurrRowFormat.Borders.Vertical.BorderType = BorderStyle.Cleared;
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.BorderType = BorderStyle.Cleared;
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.BorderType = BorderStyle.Cleared;
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.BorderType = BorderStyle.Cleared;
				}
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.BorderType = BorderStyle.Cleared;
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.BorderType = BorderStyle.Cleared;
				}
			}
			else
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.BorderType = BorderStyle.None;
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.BorderType = BorderStyle.None;
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.BorderType = BorderStyle.None;
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.BorderType = BorderStyle.None;
				}
			}
			break;
		case "brdrtnthtnmg":
			if (m_bIsRow)
			{
				if (m_bIsHorizontalBorder)
				{
					CurrRowFormat.Borders.Horizontal.BorderType = BorderStyle.ThickThickThinMediumGap;
				}
				if (m_bIsVerticalBorder)
				{
					CurrRowFormat.Borders.Vertical.BorderType = BorderStyle.ThickThickThinMediumGap;
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.BorderType = BorderStyle.ThickThickThinMediumGap;
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.BorderType = BorderStyle.ThickThickThinMediumGap;
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.BorderType = BorderStyle.ThickThickThinMediumGap;
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.BorderType = BorderStyle.ThickThickThinMediumGap;
				}
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.BorderType = BorderStyle.ThickThickThinMediumGap;
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.BorderType = BorderStyle.ThickThickThinMediumGap;
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.BorderType = BorderStyle.ThickThickThinMediumGap;
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.BorderType = BorderStyle.ThickThickThinMediumGap;
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.BorderType = BorderStyle.ThickThickThinMediumGap;
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.BorderType = BorderStyle.ThickThickThinMediumGap;
				}
			}
			else
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.BorderType = BorderStyle.ThickThickThinMediumGap;
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.BorderType = BorderStyle.ThickThickThinMediumGap;
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.BorderType = BorderStyle.ThickThickThinMediumGap;
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.BorderType = BorderStyle.ThickThickThinMediumGap;
				}
			}
			break;
		case "brdrhair":
			if (m_bIsRow)
			{
				if (m_bIsHorizontalBorder)
				{
					CurrRowFormat.Borders.Horizontal.BorderType = BorderStyle.Hairline;
				}
				if (m_bIsVerticalBorder)
				{
					CurrRowFormat.Borders.Vertical.BorderType = BorderStyle.Hairline;
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.BorderType = BorderStyle.Hairline;
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.BorderType = BorderStyle.Hairline;
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.BorderType = BorderStyle.Hairline;
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.BorderType = BorderStyle.Hairline;
				}
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.BorderType = BorderStyle.Hairline;
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.BorderType = BorderStyle.Hairline;
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.BorderType = BorderStyle.Hairline;
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.BorderType = BorderStyle.Hairline;
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.BorderType = BorderStyle.Hairline;
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.BorderType = BorderStyle.Hairline;
				}
			}
			else
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.BorderType = BorderStyle.Hairline;
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.BorderType = BorderStyle.Hairline;
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.BorderType = BorderStyle.Hairline;
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.BorderType = BorderStyle.Hairline;
				}
			}
			break;
		case "brdrdashd":
			if (m_bIsRow)
			{
				if (m_bIsHorizontalBorder)
				{
					CurrRowFormat.Borders.Horizontal.BorderType = BorderStyle.DotDash;
				}
				if (m_bIsVerticalBorder)
				{
					CurrRowFormat.Borders.Vertical.BorderType = BorderStyle.DotDash;
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.BorderType = BorderStyle.DotDash;
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.BorderType = BorderStyle.DotDash;
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.BorderType = BorderStyle.DotDash;
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.BorderType = BorderStyle.DotDash;
				}
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.BorderType = BorderStyle.DotDash;
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.BorderType = BorderStyle.DotDash;
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.BorderType = BorderStyle.DotDash;
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.BorderType = BorderStyle.DotDash;
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.BorderType = BorderStyle.DotDash;
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.BorderType = BorderStyle.DotDash;
				}
			}
			else
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.BorderType = BorderStyle.DotDash;
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.BorderType = BorderStyle.DotDash;
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.BorderType = BorderStyle.DotDash;
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.BorderType = BorderStyle.DotDash;
				}
			}
			break;
		case "brdrtriple":
			if (m_bIsRow)
			{
				if (m_bIsHorizontalBorder)
				{
					CurrRowFormat.Borders.Horizontal.BorderType = BorderStyle.Triple;
				}
				if (m_bIsVerticalBorder)
				{
					CurrRowFormat.Borders.Vertical.BorderType = BorderStyle.Triple;
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.BorderType = BorderStyle.Triple;
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.BorderType = BorderStyle.Triple;
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.BorderType = BorderStyle.Triple;
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.BorderType = BorderStyle.Triple;
				}
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.BorderType = BorderStyle.Triple;
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.BorderType = BorderStyle.Triple;
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.BorderType = BorderStyle.Triple;
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.BorderType = BorderStyle.Triple;
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.BorderType = BorderStyle.Triple;
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.BorderType = BorderStyle.Triple;
				}
			}
			else
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.BorderType = BorderStyle.Triple;
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.BorderType = BorderStyle.Triple;
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.BorderType = BorderStyle.Triple;
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.BorderType = BorderStyle.Triple;
				}
			}
			break;
		case "brdrthtnsg":
			if (m_bIsRow)
			{
				if (m_bIsHorizontalBorder)
				{
					CurrRowFormat.Borders.Horizontal.BorderType = BorderStyle.ThinThickSmallGap;
				}
				if (m_bIsVerticalBorder)
				{
					CurrRowFormat.Borders.Vertical.BorderType = BorderStyle.ThinThickSmallGap;
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.BorderType = BorderStyle.ThinThickSmallGap;
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.BorderType = BorderStyle.ThinThickSmallGap;
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.BorderType = BorderStyle.ThinThickSmallGap;
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.BorderType = BorderStyle.ThinThickSmallGap;
				}
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.BorderType = BorderStyle.ThinThickSmallGap;
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.BorderType = BorderStyle.ThinThickSmallGap;
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.BorderType = BorderStyle.ThinThickSmallGap;
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.BorderType = BorderStyle.ThinThickSmallGap;
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.BorderType = BorderStyle.ThinThickSmallGap;
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.BorderType = BorderStyle.ThinThickSmallGap;
				}
			}
			else
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.BorderType = BorderStyle.ThinThickSmallGap;
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.BorderType = BorderStyle.ThinThickSmallGap;
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.BorderType = BorderStyle.ThinThickSmallGap;
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.BorderType = BorderStyle.ThinThickSmallGap;
				}
			}
			break;
		case "brdrtnthtnlg":
			if (m_bIsRow)
			{
				if (m_bIsHorizontalBorder)
				{
					CurrRowFormat.Borders.Horizontal.BorderType = BorderStyle.ThinThickThinLargeGap;
				}
				if (m_bIsVerticalBorder)
				{
					CurrRowFormat.Borders.Vertical.BorderType = BorderStyle.ThinThickThinLargeGap;
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.BorderType = BorderStyle.ThinThickThinLargeGap;
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.BorderType = BorderStyle.ThinThickThinLargeGap;
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.BorderType = BorderStyle.ThinThickSmallGap;
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.BorderType = BorderStyle.ThinThickSmallGap;
				}
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.BorderType = BorderStyle.ThinThickThinLargeGap;
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.BorderType = BorderStyle.ThinThickThinLargeGap;
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.BorderType = BorderStyle.ThinThickThinLargeGap;
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.BorderType = BorderStyle.ThinThickThinLargeGap;
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.BorderType = BorderStyle.ThinThickThinLargeGap;
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.BorderType = BorderStyle.ThinThickThinLargeGap;
				}
			}
			else
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.BorderType = BorderStyle.ThinThickThinLargeGap;
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.BorderType = BorderStyle.ThinThickThinLargeGap;
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.BorderType = BorderStyle.ThinThickThinLargeGap;
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.BorderType = BorderStyle.ThinThickThinLargeGap;
				}
			}
			break;
		case "brdrwavy":
			if (m_bIsRow)
			{
				if (m_bIsHorizontalBorder)
				{
					CurrRowFormat.Borders.Horizontal.BorderType = BorderStyle.Wave;
				}
				if (m_bIsVerticalBorder)
				{
					CurrRowFormat.Borders.Vertical.BorderType = BorderStyle.Wave;
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.BorderType = BorderStyle.Wave;
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.BorderType = BorderStyle.Wave;
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.BorderType = BorderStyle.Wave;
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.BorderType = BorderStyle.Wave;
				}
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.BorderType = BorderStyle.Wave;
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.BorderType = BorderStyle.Wave;
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.BorderType = BorderStyle.Wave;
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.BorderType = BorderStyle.Wave;
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.BorderType = BorderStyle.Wave;
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.BorderType = BorderStyle.Wave;
				}
			}
			else
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.BorderType = BorderStyle.Wave;
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.BorderType = BorderStyle.Wave;
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.BorderType = BorderStyle.Wave;
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.BorderType = BorderStyle.Wave;
				}
			}
			break;
		case "brdrwavydb":
			if (m_bIsRow)
			{
				if (m_bIsHorizontalBorder)
				{
					CurrRowFormat.Borders.Horizontal.BorderType = BorderStyle.DoubleWave;
				}
				if (m_bIsVerticalBorder)
				{
					CurrRowFormat.Borders.Vertical.BorderType = BorderStyle.DoubleWave;
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.BorderType = BorderStyle.DoubleWave;
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.BorderType = BorderStyle.DoubleWave;
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.BorderType = BorderStyle.DoubleWave;
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.BorderType = BorderStyle.DoubleWave;
				}
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.BorderType = BorderStyle.DoubleWave;
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.BorderType = BorderStyle.DoubleWave;
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.BorderType = BorderStyle.DoubleWave;
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.BorderType = BorderStyle.DoubleWave;
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.BorderType = BorderStyle.DoubleWave;
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.BorderType = BorderStyle.DoubleWave;
				}
			}
			else
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.BorderType = BorderStyle.DoubleWave;
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.BorderType = BorderStyle.DoubleWave;
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.BorderType = BorderStyle.DoubleWave;
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.BorderType = BorderStyle.DoubleWave;
				}
			}
			break;
		case "brdrdashdotstr":
			if (m_bIsRow)
			{
				if (m_bIsHorizontalBorder)
				{
					CurrRowFormat.Borders.Horizontal.BorderType = BorderStyle.DashDotStroker;
				}
				if (m_bIsVerticalBorder)
				{
					CurrRowFormat.Borders.Vertical.BorderType = BorderStyle.DashDotStroker;
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.BorderType = BorderStyle.DashDotStroker;
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.BorderType = BorderStyle.DashDotStroker;
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.BorderType = BorderStyle.DashDotStroker;
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.BorderType = BorderStyle.DashDotStroker;
				}
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.BorderType = BorderStyle.DashDotStroker;
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.BorderType = BorderStyle.DashDotStroker;
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.BorderType = BorderStyle.DashDotStroker;
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.BorderType = BorderStyle.DashDotStroker;
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.BorderType = BorderStyle.DashDotStroker;
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.BorderType = BorderStyle.DashDotStroker;
				}
			}
			else
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.BorderType = BorderStyle.DashDotStroker;
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.BorderType = BorderStyle.DashDotStroker;
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.BorderType = BorderStyle.DashDotStroker;
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.BorderType = BorderStyle.DashDotStroker;
				}
			}
			break;
		case "brdrinset":
			if (m_bIsRow)
			{
				if (m_bIsHorizontalBorder)
				{
					CurrRowFormat.Borders.Horizontal.BorderType = BorderStyle.Inset;
				}
				if (m_bIsVerticalBorder)
				{
					CurrRowFormat.Borders.Vertical.BorderType = BorderStyle.Inset;
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.BorderType = BorderStyle.Inset;
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.BorderType = BorderStyle.Inset;
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.BorderType = BorderStyle.Inset;
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.BorderType = BorderStyle.Inset;
				}
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.BorderType = BorderStyle.Inset;
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.BorderType = BorderStyle.Inset;
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.BorderType = BorderStyle.Inset;
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.BorderType = BorderStyle.Inset;
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.BorderType = BorderStyle.Inset;
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.BorderType = BorderStyle.Inset;
				}
			}
			else
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.BorderType = BorderStyle.Inset;
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.BorderType = BorderStyle.Inset;
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.BorderType = BorderStyle.Inset;
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.BorderType = BorderStyle.Inset;
				}
			}
			break;
		case "brdroutset":
			if (m_bIsRow)
			{
				if (m_bIsHorizontalBorder)
				{
					CurrRowFormat.Borders.Horizontal.BorderType = BorderStyle.Outset;
				}
				if (m_bIsVerticalBorder)
				{
					CurrRowFormat.Borders.Vertical.BorderType = BorderStyle.Outset;
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.BorderType = BorderStyle.Outset;
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.BorderType = BorderStyle.Outset;
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.BorderType = BorderStyle.Outset;
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.BorderType = BorderStyle.Outset;
				}
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.BorderType = BorderStyle.Outset;
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.BorderType = BorderStyle.Outset;
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.BorderType = BorderStyle.Outset;
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.BorderType = BorderStyle.Outset;
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.BorderType = BorderStyle.Outset;
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.BorderType = BorderStyle.Outset;
				}
			}
			else
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.BorderType = BorderStyle.Outset;
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.BorderType = BorderStyle.Outset;
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.BorderType = BorderStyle.Outset;
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.BorderType = BorderStyle.Outset;
				}
			}
			break;
		case "brdrw":
		{
			float lineWidth = ExtractTwipsValue(tokenValue);
			if (m_bIsRow)
			{
				if (m_bIsHorizontalBorder)
				{
					CurrRowFormat.Borders.Horizontal.LineWidth = lineWidth;
				}
				if (m_bIsVerticalBorder)
				{
					CurrRowFormat.Borders.Vertical.LineWidth = lineWidth;
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.LineWidth = lineWidth;
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.LineWidth = lineWidth;
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.LineWidth = lineWidth;
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.LineWidth = lineWidth;
				}
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.LineWidth = lineWidth;
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.LineWidth = lineWidth;
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.LineWidth = lineWidth;
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.LineWidth = lineWidth;
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.LineWidth = lineWidth;
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.LineWidth = lineWidth;
				}
			}
			else
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.LineWidth = lineWidth;
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.LineWidth = lineWidth;
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.LineWidth = lineWidth;
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.LineWidth = lineWidth;
				}
			}
			break;
		}
		case "brdrcf":
		{
			int num = Convert.ToInt32(tokenValue);
			CurrColorTable = new RtfColor();
			foreach (KeyValuePair<int, RtfColor> item in m_colorTable)
			{
				if (item.Key == num)
				{
					CurrColorTable = item.Value;
				}
			}
			if (m_bIsRow)
			{
				if (m_bIsHorizontalBorder)
				{
					CurrRowFormat.Borders.Horizontal.Color = Color.FromArgb(CurrColorTable.RedN, CurrColorTable.GreenN, CurrColorTable.BlueN);
				}
				if (m_bIsVerticalBorder)
				{
					CurrRowFormat.Borders.Vertical.Color = Color.FromArgb(CurrColorTable.RedN, CurrColorTable.GreenN, CurrColorTable.BlueN);
				}
				if (m_bIsRowBorderBottom)
				{
					CurrRowFormat.Borders.Bottom.Color = Color.FromArgb(CurrColorTable.RedN, CurrColorTable.GreenN, CurrColorTable.BlueN);
				}
				if (m_bIsRowBorderLeft)
				{
					CurrRowFormat.Borders.Left.Color = Color.FromArgb(CurrColorTable.RedN, CurrColorTable.GreenN, CurrColorTable.BlueN);
				}
				if (m_bIsRowBorderTop)
				{
					CurrRowFormat.Borders.Top.Color = Color.FromArgb(CurrColorTable.RedN, CurrColorTable.GreenN, CurrColorTable.BlueN);
				}
				if (m_bIsRowBorderRight)
				{
					CurrRowFormat.Borders.Right.Color = Color.FromArgb(CurrColorTable.RedN, CurrColorTable.GreenN, CurrColorTable.BlueN);
				}
				if (m_bIsBorderBottom)
				{
					CurrCellFormat.Borders.Bottom.Color = Color.FromArgb(CurrColorTable.RedN, CurrColorTable.GreenN, CurrColorTable.BlueN);
				}
				if (m_bIsBorderLeft)
				{
					CurrCellFormat.Borders.Left.Color = Color.FromArgb(CurrColorTable.RedN, CurrColorTable.GreenN, CurrColorTable.BlueN);
				}
				if (m_bIsBorderTop)
				{
					CurrCellFormat.Borders.Top.Color = Color.FromArgb(CurrColorTable.RedN, CurrColorTable.GreenN, CurrColorTable.BlueN);
				}
				if (m_bIsBorderRight)
				{
					CurrCellFormat.Borders.Right.Color = Color.FromArgb(CurrColorTable.RedN, CurrColorTable.GreenN, CurrColorTable.BlueN);
				}
				if (m_bIsBorderDiagonalDown)
				{
					CurrCellFormat.Borders.DiagonalDown.Color = Color.FromArgb(CurrColorTable.RedN, CurrColorTable.GreenN, CurrColorTable.BlueN);
				}
				if (m_bIsBorderDiagonalUp)
				{
					CurrCellFormat.Borders.DiagonalUp.Color = Color.FromArgb(CurrColorTable.RedN, CurrColorTable.GreenN, CurrColorTable.BlueN);
				}
			}
			else
			{
				if (m_bIsBorderBottom)
				{
					m_currParagraphFormat.Borders.Bottom.Color = Color.FromArgb(CurrColorTable.RedN, CurrColorTable.GreenN, CurrColorTable.BlueN);
				}
				if (m_bIsBorderLeft)
				{
					m_currParagraphFormat.Borders.Left.Color = Color.FromArgb(CurrColorTable.RedN, CurrColorTable.GreenN, CurrColorTable.BlueN);
				}
				if (m_bIsBorderTop)
				{
					m_currParagraphFormat.Borders.Top.Color = Color.FromArgb(CurrColorTable.RedN, CurrColorTable.GreenN, CurrColorTable.BlueN);
				}
				if (m_bIsBorderRight)
				{
					m_currParagraphFormat.Borders.Right.Color = Color.FromArgb(CurrColorTable.RedN, CurrColorTable.GreenN, CurrColorTable.BlueN);
				}
			}
			break;
		}
		}
	}

	private void ApplyCellFormatting(WTableCell cell, CellFormat cellFormat)
	{
		if (cellFormat.Paddings.HasKey(2) || cellFormat.Paddings.HasKey(1) || cellFormat.Paddings.HasKey(4) || cellFormat.Paddings.HasKey(3))
		{
			cell.CellFormat.SamePaddingsAsTable = false;
		}
		else
		{
			cell.CellFormat.SamePaddingsAsTable = true;
		}
		cell.CellFormat.BackColor = cellFormat.BackColor;
		ApplyBorder(cell.CellFormat.Borders.Left, cellFormat.Borders.Left);
		ApplyBorder(cell.CellFormat.Borders.Right, cellFormat.Borders.Right);
		ApplyBorder(cell.CellFormat.Borders.Top, cellFormat.Borders.Top);
		ApplyBorder(cell.CellFormat.Borders.Bottom, cellFormat.Borders.Bottom);
		ApplyBorder(cell.CellFormat.Borders.DiagonalDown, cellFormat.Borders.DiagonalDown);
		ApplyBorder(cell.CellFormat.Borders.DiagonalUp, cellFormat.Borders.DiagonalUp);
		cell.CellFormat.CellWidth = cellFormat.CellWidth;
		cell.CellFormat.FitText = cellFormat.FitText;
		cell.CellFormat.ForeColor = cellFormat.ForeColor;
		cell.CellFormat.PreferredWidth.WidthType = cellFormat.PreferredWidth.WidthType;
		if (cellFormat.PropertiesHash.ContainsKey(13))
		{
			cell.CellFormat.PreferredWidth.Width = cellFormat.PreferredWidth.Width;
		}
		else
		{
			cell.CellFormat.PreferredWidth.Width = cellFormat.CellWidth;
			cell.CellFormat.PreferredWidth.WidthType = FtsWidth.Point;
		}
		if (cellFormat.Paddings.HasKey(3))
		{
			cell.CellFormat.Paddings.Bottom = cellFormat.Paddings.Bottom;
		}
		if (cellFormat.Paddings.HasKey(4))
		{
			cell.CellFormat.Paddings.Right = cellFormat.Paddings.Right;
		}
		if (cellFormat.Paddings.HasKey(1))
		{
			cell.CellFormat.Paddings.Left = cellFormat.Paddings.Left;
		}
		if (cellFormat.Paddings.HasKey(2))
		{
			cell.CellFormat.Paddings.Top = cellFormat.Paddings.Top;
		}
		cell.CellFormat.TextDirection = cellFormat.TextDirection;
		cell.CellFormat.TextWrap = cellFormat.TextWrap;
		cell.CellFormat.VerticalAlignment = cellFormat.VerticalAlignment;
		cell.CellFormat.VerticalMerge = cellFormat.VerticalMerge;
		cell.CellFormat.HorizontalMerge = cellFormat.HorizontalMerge;
		cell.CellFormat.TextureStyle = cellFormat.TextureStyle;
		cell.CellFormat.HideMark = cellFormat.HideMark;
	}

	private void ApplyRowFormatting(WTableRow row, RowFormat rowFormat)
	{
		if (rowFormat.HasValue(103))
		{
			row.RowFormat.IsAutoResized = rowFormat.IsAutoResized;
		}
		else
		{
			row.RowFormat.IsAutoResized = false;
		}
		if (rowFormat.HasValue(121))
		{
			row.RowFormat.Hidden = rowFormat.Hidden;
		}
		if (rowFormat.HasValue(108))
		{
			row.RowFormat.BackColor = rowFormat.BackColor;
		}
		if (rowFormat.HasValue(104))
		{
			row.RowFormat.Bidi = rowFormat.Bidi;
		}
		if (rowFormat.HasValue(52))
		{
			row.RowFormat.CellSpacing = rowFormat.CellSpacing;
		}
		if (rowFormat.HasValue(105))
		{
			RowAlignment rowAlignment = rowFormat.HorizontalAlignment;
			if (row.RowFormat.Bidi)
			{
				if (rowFormat.HorizontalAlignment == RowAlignment.Left)
				{
					rowAlignment = RowAlignment.Right;
				}
				else if (rowAlignment == RowAlignment.Right)
				{
					rowAlignment = RowAlignment.Left;
				}
			}
			row.RowFormat.HorizontalAlignment = rowAlignment;
		}
		if (rowFormat.HasValue(2))
		{
			if (rowFormat.Height < 0f)
			{
				row.RowFormat.Height = 0f - rowFormat.Height;
				row.HeightType = TableRowHeightType.Exactly;
			}
			else
			{
				row.RowFormat.Height = rowFormat.Height;
			}
		}
		if (rowFormat.HasValue(107))
		{
			row.RowFormat.IsHeaderRow = rowFormat.IsHeaderRow;
		}
		if (rowFormat.HasValue(106))
		{
			row.RowFormat.IsBreakAcrossPages = rowFormat.IsBreakAcrossPages;
		}
		if (rowFormat.HasValue(110))
		{
			row.RowFormat.TextureStyle = rowFormat.TextureStyle;
		}
		if (rowFormat.HasValue(68))
		{
			row.RowFormat.Positioning.DistanceFromLeft = rowFormat.Positioning.DistanceFromLeft;
		}
		if (rowFormat.HasValue(69))
		{
			row.RowFormat.Positioning.DistanceFromRight = rowFormat.Positioning.DistanceFromRight;
		}
		if (rowFormat.HasValue(66))
		{
			row.RowFormat.Positioning.DistanceFromTop = rowFormat.Positioning.DistanceFromTop;
		}
		if (rowFormat.HasValue(67))
		{
			row.RowFormat.Positioning.DistanceFromTop = rowFormat.Positioning.DistanceFromBottom;
		}
		if (rowFormat.HasValue(62))
		{
			row.RowFormat.Positioning.HorizPosition = rowFormat.Positioning.HorizPosition;
		}
		if (rowFormat.HasValue(64))
		{
			row.RowFormat.Positioning.HorizRelationTo = rowFormat.Positioning.HorizRelationTo;
		}
		if (rowFormat.HasValue(63))
		{
			row.RowFormat.Positioning.VertPosition = rowFormat.Positioning.VertPosition;
		}
		if (rowFormat.HasValue(65))
		{
			row.RowFormat.Positioning.VertRelationTo = rowFormat.Positioning.VertRelationTo;
		}
		if (rowFormat.HasValue(70))
		{
			row.RowFormat.Positioning.AllowOverlap = rowFormat.Positioning.AllowOverlap;
		}
		row.RowFormat.Paddings.Left = (istrpaddltypeDefined ? trpaddlValue : rowFormat.Paddings.Left);
		row.RowFormat.Paddings.Right = rowFormat.Paddings.Right;
		row.RowFormat.Paddings.Top = rowFormat.Paddings.Top;
		row.RowFormat.Paddings.Bottom = rowFormat.Paddings.Bottom;
		if (istblindtypeDefined)
		{
			row.RowFormat.LeftIndent = (float)Math.Round(tblindValue, 2);
		}
		else
		{
			row.RowFormat.LeftIndent = (float)Math.Round(rowFormat.LeftIndent, 2) + ((!(m_token != "nestrow")) ? 0f : ((m_cellFormatTable.Count > 0 && m_cellFormatTable[0].Paddings.HasKey(1)) ? m_cellFormatTable[0].Paddings.Left : row.RowFormat.Paddings.Left));
		}
		if (rowFormat.HasValue(14))
		{
			row.RowFormat.GridBeforeWidth.Width = rowFormat.GridBeforeWidth.Width;
		}
		if (rowFormat.HasValue(13))
		{
			row.RowFormat.GridBeforeWidth.WidthType = rowFormat.GridBeforeWidth.WidthType;
		}
		if (rowFormat.HasValue(16))
		{
			row.RowFormat.GridAfterWidth.Width = rowFormat.GridAfterWidth.Width;
		}
		if (rowFormat.HasValue(15))
		{
			row.RowFormat.GridAfterWidth.WidthType = rowFormat.GridAfterWidth.WidthType;
		}
		if (rowFormat.HasValue(12))
		{
			CurrTable.TableFormat.PreferredWidth.Width = rowFormat.PreferredWidth.Width;
		}
		if (rowFormat.HasValue(11))
		{
			CurrTable.TableFormat.PreferredWidth.WidthType = rowFormat.PreferredWidth.WidthType;
		}
		if (CurrTable.FirstRow == row && rowFormat.IsLeftIndentDefined)
		{
			row.RowFormat.IsLeftIndentDefined = true;
		}
		ApplyBorder(row.RowFormat.Borders.Left, rowFormat.Borders.Left);
		ApplyBorder(row.RowFormat.Borders.Right, rowFormat.Borders.Right);
		ApplyBorder(row.RowFormat.Borders.Top, rowFormat.Borders.Top);
		ApplyBorder(row.RowFormat.Borders.Bottom, rowFormat.Borders.Bottom);
		ApplyBorder(row.RowFormat.Borders.Horizontal, rowFormat.Borders.Horizontal);
		ApplyBorder(row.RowFormat.Borders.Vertical, rowFormat.Borders.Vertical);
		row.RowFormat.BeforeWidth = rowFormat.BeforeWidth;
	}

	private void ApplyBorder(Border destBorder, Border sourceBorder)
	{
		destBorder.IsRead = true;
		if (sourceBorder.BorderType == BorderStyle.Cleared || sourceBorder.BorderType == BorderStyle.None)
		{
			destBorder.BorderType = sourceBorder.BorderType;
		}
		else
		{
			destBorder.BorderType = sourceBorder.BorderType;
			destBorder.Color = sourceBorder.Color;
			destBorder.LineWidth = sourceBorder.LineWidth;
		}
		destBorder.IsRead = false;
	}

	private void AddNewParagraph(IWParagraph newParagraph)
	{
		UpdateTabsCollection(newParagraph.ParagraphFormat);
		if (newParagraph.ListFormat.ListType != ListType.NoList && !newParagraph.ListFormat.IsEmptyList && newParagraph.ListFormat.CurrentListLevel != null)
		{
			float tabSpaceAfter = newParagraph.ListFormat.CurrentListLevel.TabSpaceAfter;
			if (tabSpaceAfter != 0f && !newParagraph.ParagraphFormat.Tabs.HasTabPosition(tabSpaceAfter))
			{
				Tab tab = new Tab(m_document);
				tab.DeletePosition = tabSpaceAfter * 20f;
				newParagraph.ParagraphFormat.Tabs.AddTab(tab);
				newParagraph.ParagraphFormat.Tabs.SortTabs();
			}
		}
		if (m_currSection == null)
		{
			m_currSection = m_document.AddSection();
			m_textBody = m_currSection.Body;
		}
		if (m_textBody == null)
		{
			m_textBody = m_currSection.Body;
		}
		m_textBody.Items.Add(newParagraph);
		m_previousLevel = m_currentLevel;
		CopyTextFormatToCharFormat(newParagraph.BreakCharacterFormat, m_currTextFormat);
		m_currParagraph = null;
	}

	private void UpdateTabsCollection(WParagraphFormat paraFormat)
	{
		if (m_tabCollection.Count > 0 && paraFormat.Tabs.Count == 0)
		{
			foreach (KeyValuePair<int, TabFormat> item in m_tabCollection)
			{
				paraFormat.Tabs.AddTab(item.Value.TabPosition, item.Value.TabJustification, item.Value.TabLeader);
			}
		}
		else if (m_tabCollection.Count > paraFormat.Tabs.Count)
		{
			for (int i = paraFormat.Tabs.Count + 1; i <= m_tabCollection.Count; i++)
			{
				if (m_tabCollection.ContainsKey(i))
				{
					paraFormat.Tabs.AddTab(m_tabCollection[i].TabPosition, m_tabCollection[i].TabJustification, m_tabCollection[i].TabLeader);
				}
			}
		}
		if (m_currentTableType == RtfTableType.None)
		{
			UpdateDeleteTabsCollection(paraFormat, paraFormat.BaseFormat as WParagraphFormat);
		}
	}

	private void UpdateDeleteTabsCollection(WParagraphFormat destFormat, WParagraphFormat baseFormat)
	{
		bool flag = false;
		while (baseFormat != null)
		{
			for (int i = 0; i < baseFormat.Tabs.Count; i++)
			{
				for (int j = 0; j < destFormat.Tabs.Count; j++)
				{
					if (baseFormat.Tabs[i].Position == destFormat.Tabs[j].Position)
					{
						flag = true;
					}
				}
				if (!flag)
				{
					destFormat.Tabs.AddTab().DeletePosition = baseFormat.Tabs[i].Position * 20f;
				}
				flag = false;
			}
			baseFormat = baseFormat.BaseFormat as WParagraphFormat;
		}
	}

	private void AddNewSection(IWSection newSection)
	{
		if (newSection.Columns.Count == 0)
		{
			CurrColumn = new Column(m_document);
			CurrColumn.Space = 36f;
			newSection.Columns.Add(CurrColumn, isOpening: true);
		}
		if (!IsSectionBreak)
		{
			newSection.BreakCode = m_document.LastSection.BreakCode;
		}
		IsSectionBreak = false;
		if (newSection.Owner == null)
		{
			m_document.ChildEntities.Add(newSection);
		}
	}

	private float ExtractTwipsValue(string nValue)
	{
		return Convert.ToSingle((double)GetIntValue(nValue) / 20.0);
	}

	private float ExtractQuaterPointsValue(string nValue)
	{
		return Convert.ToSingle((double)GetIntValue(nValue) / 4.0);
	}

	private void SortTabCollection()
	{
		for (int i = 1; i < m_tabCollection.Count; i++)
		{
			for (int j = i + 1; j < m_tabCollection.Count + 1; j++)
			{
				if (m_tabCollection[i].TabPosition > m_tabCollection[j].TabPosition)
				{
					TabFormat value = m_tabCollection[i];
					m_tabCollection[i] = m_tabCollection[j];
					m_tabCollection[j] = value;
				}
			}
		}
	}

	private string[] SeperateToken(string token)
	{
		string[] array = new string[3];
		for (int i = 0; i < token.Length; i++)
		{
			char c = token[i];
			if (char.IsDigit(c) && array[2] == null)
			{
				array[1] += c;
			}
			else if (array[1] == null)
			{
				array[0] += c;
			}
			else if (array[1] != null)
			{
				array[2] += c;
			}
		}
		return array;
	}

	private string GetFontIndex(string fontFamily, WCharacterFormat charFormat)
	{
		if (fontFamily.Length <= 0 && charFormat.OwnerBase is WTextRange && !string.IsNullOrEmpty((charFormat.OwnerBase as WTextRange).Text) && groupOrder[groupOrder.Count - 1].ChildElements.Count > 0 && groupOrder[0].ChildElements[0] is Tokens && (groupOrder[0].ChildElements[0] as Tokens).TokenName == "rtf")
		{
			for (int num = groupOrder.Count - 1; num >= 0; num--)
			{
				Groups groups = groupOrder[num];
				for (int num2 = groups.ChildElements.Count - 1; num2 > 0; num2--)
				{
					if (groups.ChildElements[num2] is Tokens)
					{
						Tokens tokens = groups.ChildElements[num2] as Tokens;
						if (tokens.TokenName == "af" && groups.ChildElements[num2 - 1] is Tokens && (groups.ChildElements[num2 - 1] as Tokens).TokenName == "loch" && (groups.ChildElements[num2 - 1] as Tokens).TokenValue == null)
						{
							return tokens.TokenValue;
						}
						if (tokens.TokenName == "plain" && tokens.TokenValue == null)
						{
							return DefaultFontIndex.ToString();
						}
					}
				}
			}
		}
		return DefaultFontIndex.ToString();
	}

	private bool IsCharFormatValueEqual(WCharacterFormat styleCharacterFormat, int key, string textFormatValue)
	{
		if (styleCharacterFormat != null && styleCharacterFormat.PropertiesHash.ContainsKey(key))
		{
			return styleCharacterFormat.PropertiesHash[key].ToString().ToLower() == textFormatValue.ToLower();
		}
		return false;
	}

	private void CopyTextFormatToCharFormat(WCharacterFormat charFormat, TextFormat textFormat)
	{
		WCharacterFormat styleCharacterFormat = null;
		if (m_currParagraph != null && (m_currParagraph as WParagraph).ParaStyle != null && charFormat.OwnerBase is WTextRange)
		{
			styleCharacterFormat = (m_currParagraph as WParagraph).ParaStyle.CharacterFormat;
		}
		if (textFormat.FontSize > 0f)
		{
			charFormat.SetPropertyValue(3, textFormat.FontSize);
			charFormat.SetPropertyValue(62, textFormat.FontSize);
		}
		if (!string.IsNullOrEmpty(textFormat.CharacterStyleName) || !IsCharFormatValueEqual(styleCharacterFormat, 1, textFormat.FontColor.ToString()))
		{
			charFormat.TextColor = textFormat.FontColor;
		}
		string fontIndex = GetFontIndex(textFormat.FontFamily, charFormat);
		if (textFormat.FontFamily.Length > 0)
		{
			string text = (charFormat.FontNameBidi = textFormat.FontFamily);
			string text3 = (charFormat.FontNameNonFarEast = text);
			string text5 = (charFormat.FontNameFarEast = text3);
			string fontName = (charFormat.FontNameAscii = text5);
			charFormat.FontName = fontName;
		}
		else if (m_currentTableType == RtfTableType.None)
		{
			foreach (KeyValuePair<string, RtfFont> item in m_fontTable)
			{
				if (item.Key.Split('f')[^1] == fontIndex)
				{
					charFormat.FontName = item.Value.FontName;
				}
			}
		}
		if (textFormat.CharacterStyleName != string.Empty)
		{
			charFormat.CharStyleName = textFormat.CharacterStyleName;
		}
		charFormat.LocaleIdASCII = textFormat.LocalIdASCII;
		charFormat.LocaleIdFarEast = textFormat.LocalIdForEast;
		charFormat.LocaleIdBidi = textFormat.LidBi;
		charFormat.SetPropertyValue(17, textFormat.Position);
		charFormat.Scaling = textFormat.Scaling;
		if (textFormat.Bold != ThreeState.Unknown && (!string.IsNullOrEmpty(textFormat.CharacterStyleName) || !IsCharFormatValueEqual(styleCharacterFormat, 4, textFormat.Bold.ToString())))
		{
			if (textFormat.Bold == ThreeState.True)
			{
				charFormat.Bold = true;
			}
			else
			{
				charFormat.Bold = false;
			}
		}
		if (textFormat.Bidi != ThreeState.Unknown)
		{
			if (textFormat.Bidi == ThreeState.True)
			{
				charFormat.Bidi = true;
			}
			else
			{
				charFormat.Bidi = false;
			}
		}
		if (textFormat.CharacterSpacing != 0f)
		{
			charFormat.CharacterSpacing = textFormat.CharacterSpacing;
		}
		if (textFormat.Italic != ThreeState.Unknown && (!string.IsNullOrEmpty(textFormat.CharacterStyleName) || !IsCharFormatValueEqual(styleCharacterFormat, 5, textFormat.Italic.ToString())))
		{
			if (textFormat.Italic == ThreeState.True)
			{
				charFormat.Italic = true;
			}
			else
			{
				charFormat.Italic = false;
			}
		}
		if (textFormat.Underline != ThreeState.Unknown)
		{
			if (textFormat.Underline == ThreeState.True)
			{
				charFormat.UnderlineStyle = UnderlineStyle.Single;
			}
			else
			{
				charFormat.UnderlineStyle = UnderlineStyle.None;
			}
		}
		if (textFormat.BackColor != Color.Empty)
		{
			charFormat.TextBackgroundColor = textFormat.BackColor;
		}
		if (textFormat.ForeColor != Color.Empty)
		{
			charFormat.ForeColor = textFormat.ForeColor;
		}
		if (textFormat.HighlightColor != Color.Empty)
		{
			charFormat.HighlightColor = textFormat.HighlightColor;
		}
		charFormat.UnderlineStyle = textFormat.m_underlineStyle;
		if (textFormat.Shadow)
		{
			charFormat.Shadow = textFormat.Shadow;
		}
		if (textFormat.IsHiddenText)
		{
			charFormat.Hidden = true;
		}
		if (textFormat.SpecVanish)
		{
			charFormat.SpecVanish = true;
		}
		if (textFormat.m_subSuperScript != 0)
		{
			charFormat.SubSuperScript = textFormat.m_subSuperScript;
		}
		if (textFormat.m_BreakClear != 0)
		{
			charFormat.BreakClear = textFormat.m_BreakClear;
		}
		if (textFormat.Strike != ThreeState.Unknown)
		{
			if (textFormat.Strike == ThreeState.True)
			{
				charFormat.Strikeout = true;
			}
			else
			{
				charFormat.Strikeout = false;
			}
		}
		if (textFormat.DoubleStrike != ThreeState.Unknown)
		{
			if (textFormat.DoubleStrike == ThreeState.True)
			{
				charFormat.DoubleStrike = true;
			}
			else
			{
				charFormat.DoubleStrike = false;
			}
		}
		if (textFormat.Emboss != ThreeState.Unknown)
		{
			if (textFormat.Emboss == ThreeState.True)
			{
				charFormat.Emboss = true;
			}
			else
			{
				charFormat.Emboss = false;
			}
		}
		if (textFormat.Engrave != ThreeState.Unknown)
		{
			if (textFormat.Engrave == ThreeState.True)
			{
				charFormat.Engrave = true;
			}
			else
			{
				charFormat.Engrave = false;
			}
		}
		if (textFormat.AllCaps != ThreeState.Unknown)
		{
			if (textFormat.AllCaps == ThreeState.True)
			{
				charFormat.AllCaps = true;
			}
			else if (textFormat.AllCaps == ThreeState.False)
			{
				charFormat.AllCaps = false;
			}
		}
		if (textFormat.SmallCaps != ThreeState.Unknown)
		{
			if (textFormat.SmallCaps == ThreeState.True)
			{
				charFormat.SmallCaps = true;
			}
			else if (textFormat.SmallCaps == ThreeState.False)
			{
				charFormat.SmallCaps = false;
			}
		}
		if (textFormat.complexScript != ThreeState.Unknown)
		{
			if (textFormat.complexScript == ThreeState.True)
			{
				charFormat.ComplexScript = true;
			}
			else if (textFormat.complexScript == ThreeState.False)
			{
				charFormat.ComplexScript = false;
			}
		}
		if (textFormat.boldBidi != ThreeState.Unknown)
		{
			if (textFormat.boldBidi == ThreeState.True)
			{
				charFormat.BoldBidi = true;
			}
			else if (textFormat.boldBidi == ThreeState.False)
			{
				charFormat.BoldBidi = false;
			}
		}
		if (textFormat.italicBidi != ThreeState.Unknown)
		{
			if (textFormat.italicBidi == ThreeState.True)
			{
				charFormat.ItalicBidi = true;
			}
			else if (textFormat.italicBidi == ThreeState.False)
			{
				charFormat.ItalicBidi = false;
			}
		}
	}

	private void ApplyParagraphFont(RtfFont rtfFontTable)
	{
		m_currTextFormat.FontFamily = rtfFontTable.FontName;
	}

	private void ApplyColorTable(RtfColor rtfColor)
	{
		m_currTextFormat.FontColor = Color.FromArgb(rtfColor.RedN, rtfColor.GreenN, rtfColor.BlueN);
	}

	internal bool StartsWithExt(string text, string value)
	{
		return text.StartsWith(value);
	}

	private void SetParsedElementFlag(string token)
	{
		token = Regex.Replace(token, "[\\d+$]", string.Empty);
		string text = token.Trim().ToLower();
		if (text == null)
		{
			return;
		}
		switch (text.Length)
		{
		case 6:
			switch (text[5])
			{
			case 'a':
				if (text == "\\chdpa")
				{
					m_document.SetTriggerElement(ref m_document.m_notSupportedElementFlag, 0);
				}
				break;
			case 'l':
				if (text == "\\chdpl")
				{
					m_document.SetTriggerElement(ref m_document.m_notSupportedElementFlag, 13);
				}
				break;
			case 'h':
				if (text == "\\mmath")
				{
					m_document.SetTriggerElement(ref m_document.m_notSupportedElementFlag, 19);
				}
				break;
			}
			break;
		case 7:
			switch (text[3])
			{
			case 't':
				if (text == "\\chtime")
				{
					m_document.SetTriggerElement(ref m_document.m_notSupportedElementFlag, 3);
				}
				break;
			case 'd':
				if (text == "\\chdate")
				{
					m_document.SetTriggerElement(ref m_document.m_notSupportedElementFlag, 11);
				}
				break;
			case 'v':
				if (text == "\\revtbl")
				{
					m_document.SetTriggerElement(ref m_document.m_notSupportedElementFlag, 30);
				}
				break;
			case 'j':
				if (text == "\\object")
				{
					m_document.SetTriggerElement(ref m_document.m_supportedElementFlag_1, 29);
				}
				break;
			}
			break;
		case 8:
			switch (text[1])
			{
			case 's':
				if (text == "\\sectnum")
				{
					m_document.SetTriggerElement(ref m_document.m_notSupportedElementFlag, 7);
				}
				break;
			case 'l':
				if (text == "\\linemod")
				{
					m_document.SetTriggerElement(ref m_document.m_supportedElementFlag_1, 18);
				}
				break;
			case 'b':
				if (text == "\\bkmkend")
				{
					m_document.SetTriggerElement(ref m_document.m_supportedElementFlag_1, 4);
				}
				break;
			}
			break;
		case 3:
			switch (text[2])
			{
			default:
				return;
			case 'b':
				if (!(text == "\\tb"))
				{
					return;
				}
				break;
			case 'x':
				if (!(text == "\\tx"))
				{
					return;
				}
				break;
			}
			m_document.SetTriggerElement(ref m_document.m_supportedElementFlag_2, 1);
			break;
		case 10:
			if (text == "\\bkmkstart")
			{
				m_document.SetTriggerElement(ref m_document.m_supportedElementFlag_1, 5);
			}
			break;
		case 5:
			if (text == "\\line")
			{
				m_document.SetTriggerElement(ref m_document.m_supportedElementFlag_1, 6);
			}
			break;
		case 4:
			if (text == "\\tab")
			{
				m_document.SetTriggerElement(ref m_document.m_supportedElementFlag_2, 9);
			}
			break;
		case 9:
			break;
		}
	}

	private void SetShapeElementsFlag(int shapeTypeValue)
	{
		switch (shapeTypeValue)
		{
		case 136:
			m_document.SetTriggerElement(ref m_document.m_supportedElementFlag_2, 17);
			break;
		case 202:
			m_document.SetTriggerElement(ref m_document.m_supportedElementFlag_2, 13);
			break;
		}
		if ((shapeTypeValue >= 24 && shapeTypeValue <= 31) || (shapeTypeValue >= 137 && shapeTypeValue <= 165))
		{
			m_document.SetTriggerElement(ref m_document.m_notSupportedElementFlag, 31);
		}
		else
		{
			m_document.SetTriggerElement(ref m_document.m_notSupportedElementFlag, 25);
		}
	}
}
