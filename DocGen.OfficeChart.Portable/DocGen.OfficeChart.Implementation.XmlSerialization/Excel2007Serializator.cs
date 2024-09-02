using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using DocGen.Compression;
using DocGen.Compression.Zip;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Implementation.XmlReaders;
using DocGen.OfficeChart.Implementation.XmlReaders.Shapes;
using DocGen.OfficeChart.Implementation.XmlSerialization.Constants;
using DocGen.OfficeChart.Implementation.XmlSerialization.Shapes;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Implementation.XmlSerialization;

internal class Excel2007Serializator : IDisposable
{
	public enum CellType
	{
		b,
		e,
		inlineStr,
		n,
		s,
		str
	}

	public enum FormulaType
	{
		array,
		dataTable,
		normal,
		shared
	}

	private delegate void ProtectionAttributeSerializator(XmlWriter writer, string attributeName, OfficeSheetProtection flag, bool defaultValue, OfficeSheetProtection protection);

	private const int MaximumFormulaLength = 8000;

	public const string XmlFileHeading = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\" ?>";

	public const string ContentTypesNamespace = "http://schemas.openxmlformats.org/package/2006/content-types";

	public const string HyperlinkNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/hyperlink";

	public const string RelationNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

	public const string XmlNamespaceMain = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

	public const string WorksheetPartType = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet";

	public const string ChartSheetPartType = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/chartsheet";

	public const string ExtendedPropertiesPartType = "http://schemas.openxmlformats.org/officeDocument/2006/extended-properties";

	public const string CorePropertiesPartType = "http://schemas.openxmlformats.org/package/2006/metadata/core-properties";

	public const string X14Namespace = "http://schemas.microsoft.com/office/spreadsheetml/2009/9/main";

	public const string PivotFieldUri = "{E15A36E0-9728-4e99-A89B-3F7291B0FE68}";

	public const string ExternListUri = "{962EF5D1-5CA2-4c93-8EF4-DBF5C05439D2}";

	public const string SlicerExtensionUri = "{A8765BA9-456A-4dab-B4F3-ACF838C121DE}";

	public const string X14NameSpaceAttribute = "xmlns";

	public const string HideValuesRowAttribute = "hideValuesRow";

	public const string MSNamespaceMain = "http://schemas.microsoft.com/office/excel/2006/main";

	public const string MSNamespaceMainAttribute = "xmlns";

	public const string XMNamespaceMain = "http://schemas.microsoft.com/office/excel/2006/main";

	public const string XMNamespaceMainAttribute = "xmlns";

	public const string x14PivotTableDefinitionAttributes = "pivotTableDefinition";

	public const string SparklineUri = "{05C60535-1F16-4fd2-B633-F4F36F0B64E0}";

	public const string MCPrefix = "mc";

	public const string MCNamespace = "http://schemas.openxmlformats.org/markup-compatibility/2006";

	public const string CorePropertiesPrefix = "cp";

	public const string DublinCorePartType = "http://purl.org/dc/elements/1.1/";

	public const string DublinCorePrefix = "dc";

	public const string DublinCoreTermsPartType = "http://purl.org/dc/terms/";

	public const string DublinCoreTermsPrefix = "dcterms";

	public const string DCMITypePartType = "http://purl.org/dc/dcmitype/";

	public const string DCMITypePrefix = "dcmitype";

	public const string XSIPartType = "http://www.w3.org/2001/XMLSchema-instance";

	public const string XSIPrefix = "xsi";

	public const string CustomPropertiesPartType = "http://schemas.openxmlformats.org/officeDocument/2006/custom-properties";

	public const string DocPropsVTypesPartType = "http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes";

	public const string OleObjectContentType = "application/vnd.openxmlformats-officedocument.oleObject";

	public const string OleObjectFileExtension = "bin";

	public const string DocPropsVTypesPrefix = "vt";

	public const string RelationPrefix = "r";

	public const string X14Prefix = "x14";

	public const string MSPrefix = "xm";

	public const string TypesTagName = "Types";

	public const string ExtensionAttributeName = "Extension";

	public const string DefaultTagName = "Default";

	public const string ContentTypeAttributeName = "ContentType";

	public const string OverrideTagName = "Override";

	public const string PartNameAttributeName = "PartName";

	public const string WorkbookTagName = "workbook";

	public const string SheetsTagName = "sheets";

	public const string SheetTagName = "sheet";

	public const string SheetNameAttribute = "name";

	public const string DefaultWorksheetPathFormat = "worksheets/sheet{0}.xml";

	public const string RelationIdFormat = "rId{0}";

	public const string SheetIdAttribute = "sheetId";

	public const string RelationAttribute = "id";

	public const string RelationIdAttribute = "Id";

	public const string SheetStateAttributeName = "state";

	public const string CalcProperties = "calcPr";

	public const string CalculationId = "calcId";

	public const string TabSelected = "tabSelected";

	public const string DEF_DEFAULT_ROW_DELIMITER = "\r\n";

	public const string StateHidden = "hidden";

	public const string StateVeryHidden = "veryHidden";

	public const string StateVisible = "visible";

	public const string RelationsTagName = "Relationships";

	public const string RelationTagName = "Relationship";

	public const string RelationTypeAttribute = "Type";

	public const string RelationTargetAttribute = "Target";

	public const string RelationTargetModeAttribute = "TargetMode";

	public const string RelationExternalTargetMode = "External";

	public const string MergeCellsXmlTagName = "mergeCells";

	public const string CountAttributeName = "count";

	public const string MergeCellXmlTagName = "mergeCell";

	public const string RefAttributeName = "ref";

	public const string DefinedNamesXmlTagName = "definedNames";

	public const string DefinedNameXmlTagName = "definedName";

	public const string NameAttributeName = "name";

	public const string NameSheetIdAttribute = "localSheetId";

	public const string StyleSheetTagName = "styleSheet";

	public const string SlicerList = "slicerList";

	public const string FontsTagName = "fonts";

	public const string FontTagName = "font";

	public const string FontBoldTagName = "b";

	public const string FontItalicTagName = "i";

	public const string FontUnderlineTagName = "u";

	public const string ValueAttributeName = "val";

	public const string FontSizeTagName = "sz";

	public const string FontStrikeTagName = "strike";

	public const string FontNameTagName = "name";

	public const string ColorTagName = "color";

	public const string ColorIndexedAttributeName = "indexed";

	public const string ColorThemeAttributeName = "theme";

	public const string ColorTintAttributeName = "tint";

	public const string ColorRgbAttribute = "rgb";

	public const string Auto = "auto";

	public const string IndexedColorsTagName = "indexedColors";

	public const string ColorsTagName = "colors";

	public const string RgbColorTagName = "rgbColor";

	public const string MacOSShadowTagName = "shadow";

	public const string FontVerticalAlignmentTagName = "vertAlign";

	public const string FontFamilyTagName = "family";

	public const string FontCharsetTagName = "charset";

	public const string NumberFormatsTagName = "numFmts";

	public const string NumberFormatTagName = "numFmt";

	public const string NumberFormatIdAttributeName = "numFmtId";

	public const string NumberFormatStringAttributeName = "formatCode";

	public const string FillsTagName = "fills";

	public const string FillTagName = "fill";

	public const string PatternFillTagName = "patternFill";

	public const string GradientFillTagName = "gradientFill";

	public const string GradientFillTypeAttributeName = "type";

	public const string GradientFillTypeLinear = "linear";

	public const string GradientFillTypePath = "path";

	public const string LinearGradientDegreeAttributeName = "degree";

	public const string BottomConvergenceAttributeName = "bottom";

	public const string LeftConvergenceAttributeName = "left";

	public const string RightConvergenceAttributeName = "right";

	public const string TopConvergenceAttributeName = "top";

	public const string GradientStopTagName = "stop";

	public const string GradientStopPositionAttributeName = "position";

	public const string PatternAttributeName = "patternType";

	public const string BackgroundColorTagName = "bgColor";

	public const string ForegroundColorTagName = "fgColor";

	public const string BordersTagName = "borders";

	public const string BordersCollectionTagName = "border";

	public const string BorderStyleAttributeName = "style";

	public const string BorderColorTagName = "color";

	public const string WorksheetTagName = "worksheet";

	public const string DimensionTagName = "dimension";

	public const string SheetDataTagName = "sheetData";

	public const string CellTagName = "c";

	public const string CellMetadataIndexAttributeName = "cm";

	public const string ShowPhoneticAttributeName = "ph";

	public const string ReferenceAttributeName = "r";

	public const string StyleIndexAttributeName = "s";

	public const string CellDataTypeAttributeName = "t";

	public const string ValueMetadataIndexAttributeName = "vm";

	public const string FormulaTagName = "f";

	public const string CellValueTagName = "v";

	public const string RichTextInlineTagName = "is";

	public const string RichTextRunPropertiesTagName = "rPr";

	public const string RichTextRunFontTagName = "rFont";

	public const string ColsTagName = "cols";

	public const string ColTagName = "col";

	public const string ColumnMinAttribute = "min";

	public const string ColumnMaxAttribute = "max";

	public const string ColumnWidthAttribute = "width";

	public const string ColumnStyleAttribute = "style";

	public const string ColumnCustomWidthAttribute = "customWidth";

	public const string BestFitAttribute = "bestFit";

	public const string RowTagName = "row";

	public const string RowIndexAttributeName = "r";

	public const string RowHeightAttributeName = "ht";

	public const string RowHiddenAttributeName = "hidden";

	public const string RowCustomFormatAttributeName = "customFormat";

	public const string RowCustomHeightAttributeName = "customHeight";

	public const string RowColumnCollapsedAttribute = "collapsed";

	public const string RowColumnOutlineLevelAttribute = "outlineLevel";

	public const string RowThickBottomAttributeName = "thickBot";

	public const string RowThickTopAttributeName = "thickTop";

	public const string FormulaTypeAttributeName = "t";

	public const string AlwaysCalculateArray = "aca";

	public const string SharedGroupIndexAttributeName = "si";

	public const string RangeOfCellsAttributeName = "ref";

	public const string CommentAuthorsTagName = "authors";

	public const string CommentAuthorTagName = "author";

	public const string CommentListTagName = "commentList";

	public const string CommentTagName = "comment";

	public const string CommentTextTagName = "text";

	public const string CommentsTagName = "comments";

	public const string AuthorIdAttributeName = "authorId";

	public const string DefaultCellDataType = "n";

	public const string NamedStyleXFsTagName = "cellStyleXfs";

	public const string CellFormatXFsTagName = "cellXfs";

	public const string DiffXFsTagName = "dxfs";

	public const string TableStylesTagName = "tableStyles";

	public const string ExtendedFormatTagName = "xf";

	public const string FontIdAttributeName = "fontId";

	public const string FillIdAttributeName = "fillId";

	public const string BorderIdAttributeName = "borderId";

	public const string XFIdAttributeName = "xfId";

	public const string CellStylesTagName = "cellStyles";

	public const string CellStyleTagName = "cellStyle";

	public const string StyleBuiltinIdAttributeName = "builtinId";

	public const string StyleCustomizedAttributeName = "customBuiltin";

	public const string OutlineLevelAttribute = "iLevel";

	public const string IncludeAlignmentAttributeName = "applyAlignment";

	public const string IncludeBorderAttributeName = "applyBorder";

	public const string IncludeFontAttributeName = "applyFont";

	public const string IncludeNumberFormatAttributeName = "applyNumberFormat";

	public const string IncludePatternsAttributeName = "applyFill";

	public const string IncludeProtectionAttributeName = "applyProtection";

	public const string AlignmentTagName = "alignment";

	public const string ProtectionTagName = "protection";

	public const string IndentAttributeName = "indent";

	public const string HAlignAttributeName = "horizontal";

	public const string JustifyLastLineAttributeName = "justifyLastLine";

	public const string ReadingOrderAttributeName = "readingOrder";

	public const string ShrinkToFitAttributeName = "shrinkToFit";

	public const string TextRotationAttributeName = "textRotation";

	public const string WrapTextAttributeName = "wrapText";

	public const string VerticalAttributeName = "vertical";

	public const string HiddenAttributeName = "hidden";

	public const string LockedAttributeName = "locked";

	public const bool HiddenDefaultValue = false;

	public const bool LockedDefaultValue = true;

	public const string QuotePreffixAttributeName = "quotePrefix";

	public const string DiagonalDownAttributeName = "diagonalDown";

	public const string DiagonalUpAttributeName = "diagonalUp";

	public const string SharedStringTableTagName = "sst";

	public const string UniqueStringCountAttributeName = "uniqueCount";

	public const string StringItemTagName = "si";

	public const string TextTagName = "t";

	public const string RichTextRunTagName = "r";

	public const string TemporaryRoot = "root";

	public const string SpaceAttributeName = "space";

	public const string XmlPrefix = "xml";

	public const string PreserveValue = "preserve";

	private const string CellTypeNumber = "n";

	private const string CellTypeString = "s";

	private const string CellTypeBool = "b";

	private const string CellTypeError = "e";

	private const string CellTypeFormulaString = "str";

	private const string CellTypeInlineString = "inlineStr";

	public const string ThemeTagName = "theme";

	internal const string ThemeOverrideTagName = "themeOverride";

	public const string ThemeElementsTagName = "themeElements";

	public const string ColorSchemeTagName = "clrScheme";

	public const string RGBHexColorValueAttributeName = "val";

	public const string SystemColorTagName = "sysClr";

	public const string SystemColorValueAttributeName = "val";

	public const string SystemColorLastColorAttributeName = "lastClr";

	public const string DxfFormattingTagName = "dxf";

	public const string PhoneticPr = "phoneticPr";

	public const string Phonetic = "phonetic";

	internal const string SortState = "sortState";

	public const string HyperlinksTagName = "hyperlinks";

	public const string HyperlinkTagName = "hyperlink";

	public const string DisplayStringAttributeName = "display";

	public const string RelationshipIdAttributeName = "id";

	public const string LocationAttributeName = "location";

	public const string HyperlinkReferenceAttributeName = "ref";

	public const string ToolTipAttributeName = "tooltip";

	public const string SheetLevelPropertiesTagName = "sheetPr";

	public const string PageSetupPropertiesTag = "pageSetUpPr";

	public const string FitToPageAttribute = "fitToPage";

	public const string SheetTabColorTagName = "tabColor";

	public const string SheetOutlinePropertiesTagName = "outlinePr";

	public const string SummaryRowBelow = "summaryBelow";

	public const string SummaryColumnRight = "summaryRight";

	public const string BackgroundImageTagName = "picture";

	public const string FileHyperlinkStartString = "file:///";

	public const string HttpStartString = "http://";

	public const string SheetFormatPropertiesTag = "sheetFormatPr";

	public const string ZeroHeightAttribute = "zeroHeight";

	public const string DefaultRowHeightAttribute = "defaultRowHeight";

	public const string DefaultColumWidthAttribute = "defaultColWidth";

	public const string BaseColWidthAttribute = "baseColWidth";

	public const string ThickBottomAttribute = "thickBottom";

	public const string ThickTopAttribute = "thickTop";

	public const string OutlineLevelColAttribute = "outlineLevelCol";

	public const string OutlineLevelRowAttribute = "outlineLevelRow";

	public const string WorkbookViewsTagName = "bookViews";

	public const string WorkbookViewTagName = "workbookView";

	public const string ActiveSheetIndexAttributeName = "activeTab";

	public const string AutoFilterDateGroupingAttributeName = "autoFilterDateGrouping";

	public const string FirstSheetAttributeName = "firstSheet";

	public const string MinimizedAttributeName = "minimized";

	public const string ShowHorizontalScrollAttributeName = "showHorizontalScroll";

	public const string ShowSheetTabsAttributeName = "showSheetTabs";

	public const string ShowVerticalScrollAttributeName = "showVerticalScroll";

	public const string SheetTabRatioAttributeName = "tabRatio";

	public const string VisibilityAttributeName = "visibility";

	public const string WindowHeightAttributeName = "windowHeight";

	public const string WindowWidthAttributeName = "windowWidth";

	public const string UpperLeftCornerXAttributeName = "xWindow";

	public const string UpperLeftCornerYAttributeName = "yWindow";

	public const string HorizontalPageBreaksTagName = "rowBreaks";

	public const string VerticalPageBreaksTagName = "colBreaks";

	public const string PageBreakCountAttributeName = "count";

	public const string ManualBreakCountAttributeName = "manualBreakCount";

	public const string BreakTagName = "brk";

	public const string IdAttributeName = "id";

	public const string ManualPageBreakAttributeName = "man";

	public const string MaximumAttributeName = "max";

	public const string MinimumAttributeName = "min";

	public const string SheetViewsTag = "sheetViews";

	public const string SheetViewTag = "sheetView";

	public const string ShowZeros = "showZeros";

	public const string WorkbookViewIdAttribute = "workbookViewId";

	public const string SheetZoomScale = "zoomScale";

	public const string ViewTag = "view";

	public const string Layout = "pageLayout";

	public const string PageBreakPreview = "pageBreakPreview";

	public const string Normal = "normal";

	public const string TrueValue = "1";

	public const string FalseValue = "0";

	public const string SparklineColumnValue = "column";

	public const string SparklineWinLossValue = "stacked";

	public const string EmptyCellsGapValue = "gap";

	public const string EmptyCellsZeroValue = "zero";

	public const string EmptyCellsLineValue = "span";

	public const string VerticalCustomTypeValue = "custom";

	public const string VerticalSameTypeValue = "group";

	public const string ShowGridLines = "showGridLines";

	public const string RightToLeft = "rightToLeft";

	public const string SheetGridColor = "defaultGridColor";

	public const string ColorID = "colorId";

	public const string CustomPropertiesTagName = "customProperties";

	public const string CustomPropertyTagName = "customPr";

	public const string IgnoredErrorsTag = "ignoredErrors";

	public const string IgnoredErrorTag = "ignoredError";

	public const string OnCall = "OLEUPDATE_ONCALL";

	public const string Always = "OLEUPDATE_ALWAYS";

	public static ExcelIgnoreError[] ErrorsSequence = new ExcelIgnoreError[7]
	{
		ExcelIgnoreError.EmptyCellReferences,
		ExcelIgnoreError.EvaluateToError,
		ExcelIgnoreError.InconsistentFormula,
		ExcelIgnoreError.NumberAsText,
		ExcelIgnoreError.OmittedCells,
		ExcelIgnoreError.TextDate,
		ExcelIgnoreError.UnlockedFormulaCells
	};

	private static string[] s_arrFormulas = new string[12]
	{
		"if lineDrawn pixelLineWidth 0", "sum @0 1 0", "sum 0 0 @1", "prod @2 1 2", "prod @3 21600 pixelWidth", "prod @3 21600 pixelHeight", "sum @0 0 1", "prod @6 1 2", "prod @7 21600 pixelWidth", "sum @8 21600 0",
		"prod @7 21600 pixelHeight", "sum @10 21600 0"
	};

	private static string s_color = "windowText [{0}]";

	public static string[] ErrorTagsSequence = new string[7] { "emptyCellReference", "evalError", "formula", "numberStoredAsText", "formulaRange", "twoDigitTextYear", "unlockedFormula" };

	public const string RangeReferenceAttribute = "sqref";

	public const string FileVersionTag = "fileVersion";

	public const string RupBuild = "rupBuild";

	public const string LastEdited = "lastEdited";

	public const string LowestEdited = "lowestEdited";

	public const string WorkbookPr = "workbookPr";

	public const string WorkbookDate1904 = "date1904";

	public const string WorkbookPrecision = "fullPrecision";

	public const string ApplicationNameAttribute = "appName";

	public const string ApplicationNameValue = "xl";

	private const string WindowProtection = "windowProtection";

	public const string FunctionGroups = "functionGroups";

	public const string CodeName = "codeName";

	public const string HidePivotFieldList = "hidePivotFieldList";

	private const string SpansTag = "spans";

	public const string Extensionlist = "extLst";

	public const string Extension = "ext";

	public const string CalculateOnOpen = "ca";

	public const string dataTable2DTag = "dt2D";

	public const string dataTableRowTag = "dtr";

	public const string dataTableCellTag = "r1";

	private static readonly char[] allowedChars = new char[3] { '\n', '\r', '\t' };

	private const int FirstVisibleChar = 32;

	public const string TransitionEvaluation = "transitionEvaluation";

	public const string ShowRowColHeaders = "showRowColHeaders";

	private const string VersionValue = "12.0000";

	public const string Properties = "properties";

	public const string DocumentManagement = "documentManagement";

	public const string PCPrefix = "pc";

	public const string PPrefix = "p";

	public const string PropertiesNameSpace = "http://schemas.microsoft.com/office/2006/metadata/properties";

	public const string PartnerControlsNameSpace = "http://schemas.microsoft.com/office/infopath/2007/PartnerControls";

	public const string DefaultThemeVersion = "defaultThemeVersion";

	public const string ConnectionsTag = "connections";

	public const string ConnectionTag = "connection";

	public const string OdbcFileAttribute = "odcFile";

	public const string DataBaseNameAttribute = "name";

	public const string DataBaseTypeAttribute = "type";

	public const string RefreshedVersionAttribute = "refreshedVersion";

	public const string BackGroundAttribute = "background";

	public const string SaveData = "saveData";

	public const string DataBasePrTag = "dbPr";

	public const string CommandTextAttribute = "command";

	public const string CommandTypeAttribute = "commandType";

	public const string ConnectionIdAttribute = "id";

	public const string SourceFile = "sourceFile";

	public const string DescriptionTag = "description";

	public const string Interval = "interval";

	public const string SavePassword = "savePassword";

	public const string OnlyUseConnectionFile = "onlyUseConnectionFile";

	public const string BackgroundRefresh = "backgroundRefresh";

	public const string Credentials = "credentials";

	public const string Deleted = "deleted";

	public const string TextPr = "textPr";

	public const string WebPrTag = "webPr";

	public const string Xml = "xml";

	public const string URL = "url";

	public const string OlapPrTag = "olapPr";

	public const string PivotButton = "pivotButton";

	public const string CustomXmlPartName = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/customXml";

	public const string CustomXmlName = "customXml/item{0}.xml";

	public const string CustomXmlPropertiesName = "customXml/itemProps{0}.xml";

	public const string CustomXmlRelation = "{0}/_rels/item{1}.xml.rels";

	public const string DataStoreItem = "datastoreItem";

	public const string ItemIdAttribute = "ds:itemID";

	public const string CustomXmlSchemaReferences = "schemaRefs";

	public const string CustomXmlSchemaReference = "schemaRef";

	public const string CustomXmlUriAttribute = "ds:uri";

	public const string XmlItemName = "item{0}.xml";

	public const string XmlPropertiesName = "itemProps{0}.xml";

	public const string CustomXmlNameSpace = "http://schemas.openxmlformats.org/officeDocument/2006/customXml";

	public const string ItemPropertiesPrefix = "ds";

	public const string CustomXmlItemID = "itemID";

	public const string ItemPrpertiesUri = "uri";

	public const string CustomXmlItemPropertiesRelation = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/customXmlProps";

	private WorkbookImpl m_book;

	private FormulaUtil m_formulaUtil;

	private RecordExtractor m_recordExtractor;

	private Dictionary<int, ShapeSerializator> m_shapesVmlSerializators = new Dictionary<int, ShapeSerializator>();

	private Dictionary<int, ShapeSerializator> m_shapesHFVmlSerializators = new Dictionary<int, ShapeSerializator>();

	private Dictionary<Type, ShapeSerializator> m_shapesSerializators = new Dictionary<Type, ShapeSerializator>();

	private List<Stream> m_streamsSheetsCF;

	private List<string> m_sheetNames;

	private WorksheetImpl m_worksheetImpl;

	private bool hasTextRotation;

	private char[] SpecialChars = new char[23]
	{
		'\ufffd', '\ufffd', '\ufffd', '#', '%', '(', ')', '-', '+', '.',
		';', '=', '^', '`', '|', '~', '\ufffd', '\ufffd', '\ufffd', '\ufffd',
		'\ufffd', '\ufffd', '\ufffd'
	};

	public Dictionary<int, ShapeSerializator> HFVmlSerializators => m_shapesHFVmlSerializators;

	public Dictionary<int, ShapeSerializator> VmlSerializators => m_shapesVmlSerializators;

	public virtual OfficeVersion Version => OfficeVersion.Excel2007;

	internal WorksheetImpl Worksheet => m_worksheetImpl;

	public Excel2007Serializator(WorkbookImpl book)
	{
		if (book == null)
		{
			throw new ArgumentNullException("book");
		}
		m_book = book;
		m_formulaUtil = new FormulaUtil(m_book.Application, m_book, NumberFormatInfo.InvariantInfo, ',', ';');
		m_recordExtractor = new RecordExtractor();
		m_shapesVmlSerializators.Add(75, new VmlBitmapSerializator());
		m_shapesHFVmlSerializators.Add(75, new HFImageSerializator());
		m_shapesSerializators.Add(typeof(BitmapShapeImpl), new BitmapShapeSerializator());
		m_shapesSerializators.Add(typeof(ChartShapeImpl), new ChartShapeSerializator());
		m_shapesSerializators.Add(typeof(TextBoxShapeImpl), new TextBoxSerializator());
	}

	public void Dispose()
	{
		m_shapesVmlSerializators.Clear();
		m_shapesVmlSerializators = null;
		m_shapesSerializators.Clear();
		m_shapesSerializators = null;
		m_shapesHFVmlSerializators.Clear();
		m_shapesHFVmlSerializators = null;
		if (m_sheetNames != null)
		{
			m_sheetNames.Clear();
			m_sheetNames = null;
		}
	}

	public void SerializeContentTypes(XmlWriter writer, IDictionary<string, string> contentDefaults, IDictionary<string, string> contentOverrides)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (contentDefaults == null)
		{
			throw new ArgumentNullException("contentDefaults");
		}
		if (contentOverrides == null)
		{
			throw new ArgumentNullException("contentOverrides");
		}
		writer.WriteStartDocument(standalone: true);
		writer.WriteStartElement("Types", "http://schemas.openxmlformats.org/package/2006/content-types");
		SerializeDictionary(writer, contentDefaults, "Default", "Extension", "ContentType", null);
		bool flag = false;
		foreach (KeyValuePair<string, string> contentOverride in contentOverrides)
		{
			if (contentOverride.Key.Contains("xl/styles.xml"))
			{
				if (contentOverride.Key.Split('/').Length > 3 && flag)
				{
					contentOverrides.Remove(contentOverride.Key);
					break;
				}
				flag = true;
			}
		}
		SerializeDictionary(writer, contentOverrides, "Override", "PartName", "ContentType", new AddSlashPreprocessor());
		writer.WriteEndElement();
	}

	private void SerializeCalculation(XmlWriter writer)
	{
		writer.WriteStartElement("calcPr");
		bool flag = !m_book.PrecisionAsDisplayed;
		SerializeAttribute(writer, "fullPrecision", flag, !flag);
		writer.WriteAttributeString("calcId", m_book.DataHolder.CalculationId);
		writer.WriteEndElement();
	}

	private void SerializeWorkbookPr(XmlWriter writer)
	{
		_ = m_book.Date1904;
		writer.WriteStartElement("workbookPr");
		SerializeAttribute(writer, "date1904", m_book.Date1904, defaultValue: false);
		if (m_book.CodeName != null)
		{
			writer.WriteAttributeString("codeName", m_book.CodeName);
		}
		string defaultThemeVersion = m_book.DefaultThemeVersion;
		if (defaultThemeVersion != null && defaultThemeVersion.Length > 0)
		{
			writer.WriteAttributeString("defaultThemeVersion", defaultThemeVersion);
		}
		else if (m_book.IsCreated)
		{
			if (m_book.Version == OfficeVersion.Excel2013)
			{
				writer.WriteAttributeString("defaultThemeVersion", "153222");
			}
			else
			{
				writer.WriteAttributeString("defaultThemeVersion", "124226");
			}
		}
		if (!m_book.HidePivotFieldList)
		{
			writer.WriteAttributeString("hidePivotFieldList", "1");
		}
		writer.WriteEndElement();
	}

	private void SerializePivotCache(XmlWriter writer, string cacheId, string relationId)
	{
		writer.WriteStartElement("pivotCache");
		writer.WriteAttributeString("cacheId", cacheId);
		writer.WriteAttributeString("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships", relationId);
		writer.WriteEndElement();
	}

	private void SerializeFileVersion(XmlWriter writer, FileVersion fileVersion)
	{
		writer.WriteStartElement("fileVersion");
		if (fileVersion.ApplicationName != null)
		{
			writer.WriteAttributeString("appName", fileVersion.ApplicationName);
		}
		if (fileVersion.LastEdited != null)
		{
			writer.WriteAttributeString("lastEdited", fileVersion.LastEdited);
		}
		if (fileVersion.LowestEdited != null)
		{
			writer.WriteAttributeString("lowestEdited", fileVersion.LowestEdited);
		}
		if (fileVersion.BuildVersion != null)
		{
			writer.WriteAttributeString("rupBuild", fileVersion.BuildVersion);
		}
		if (fileVersion.CodeName != null)
		{
			writer.WriteAttributeString("codeName", fileVersion.CodeName);
		}
		writer.WriteEndElement();
	}

	private void SerializeWorkbookProtection(XmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (m_book.IsWindowProtection || m_book.IsCellProtection)
		{
			writer.WriteStartElement("workbookProtection");
			PasswordRecord password = m_book.Password;
			if (password != null && password.IsPassword != 0)
			{
				writer.WriteAttributeString("workbookPassword", password.IsPassword.ToString("X4"));
			}
			SerializeAttribute(writer, "lockStructure", m_book.IsCellProtection, defaultValue: false);
			SerializeAttribute(writer, "lockWindows", m_book.IsWindowProtection, defaultValue: false);
			writer.WriteEndElement();
		}
	}

	public void SerializeMerges(XmlWriter writer, MergeCellsImpl mergedCells)
	{
		if (mergedCells == null)
		{
			return;
		}
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		List<Rectangle> mergedRegions = mergedCells.MergedRegions;
		if (mergedRegions != null && mergedRegions.Count != 0)
		{
			int count = mergedRegions.Count;
			writer.WriteStartElement("mergeCells");
			writer.WriteAttributeString("count", count.ToString());
			for (int i = 0; i < count; i++)
			{
				Rectangle rect = mergedRegions[i];
				MergeCellsRecord.MergedRegion region = mergedCells.RectangleToMergeRegion(rect);
				writer.WriteStartElement("mergeCell");
				writer.WriteAttributeString("ref", GetRangeName(region));
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}
	}

	public void SerializeNamedRanges(XmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		WorkbookNamesCollection innerNamesColection = m_book.InnerNamesColection;
		int num = innerNamesColection?.Count ?? 0;
		if (num <= 0)
		{
			return;
		}
		innerNamesColection.SortForSerialization();
		writer.WriteStartElement("definedNames");
		for (int i = 0; i < num; i++)
		{
			NameImpl nameImpl = (NameImpl)innerNamesColection[i];
			if (nameImpl != null && !nameImpl.Record.IsFunctionOrCommandMacro)
			{
				SerializeNamedRange(writer, nameImpl);
			}
		}
		writer.WriteEndElement();
	}

	public Dictionary<int, int> SerializeStyles(XmlWriter writer, ref Stream streamDxfs)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		writer.WriteStartDocument(standalone: true);
		writer.WriteStartElement("styleSheet", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
		writer.WriteAttributeString("xmlns", "mc", null, "http://schemas.openxmlformats.org/markup-compatibility/2006");
		writer.WriteAttributeString("Ignorable", "http://schemas.openxmlformats.org/markup-compatibility/2006", "x14ac");
		writer.WriteAttributeString("xmlns", "x14ac", null, "http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac");
		SerializeNumberFormats(writer);
		SerializeFonts(writer);
		int[] arrFillIndexes = SerializeFills(writer);
		int[] arrBorderIndexes = SerializeBorders(writer);
		_ = m_book.InnerExtFormats.Count;
		Dictionary<int, int> hashNewParentIndexes = SerializeNamedStyleXFs(writer, arrFillIndexes, arrBorderIndexes);
		Dictionary<int, int> result = SerializeNotNamedXFs(writer, arrFillIndexes, arrBorderIndexes, hashNewParentIndexes);
		SerializeStyles(writer, hashNewParentIndexes);
		SerializeStream(writer, streamDxfs);
		SerializeStream(writer, m_book.CustomTableStylesStream);
		SerializeColors(writer);
		Stream extensionStream = m_book.DataHolder.ExtensionStream;
		if (extensionStream != null)
		{
			extensionStream.Position = 0L;
			ShapeParser.WriteNodeFromStream(writer, extensionStream, writeNamespaces: true);
		}
		writer.WriteEndElement();
		return result;
	}

	public void SerializeRelations(XmlWriter writer, RelationCollection relations, WorksheetDataHolder holder)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (relations == null || relations.Count == 0)
		{
			return;
		}
		writer.WriteStartDocument(standalone: true);
		writer.WriteStartElement("Relationships", "http://schemas.openxmlformats.org/package/2006/relationships");
		foreach (KeyValuePair<string, Relation> relation in relations)
		{
			if (holder != null)
			{
				if (Array.IndexOf(holder.ParentHolder.Archive.Items, relation.Value.Target, 0, holder.ParentHolder.Archive.Items.Length) >= 0 || (!(relation.Value.Type == "http://schemas.microsoft.com/office/2011/relationships/chartColorStyle") && !(relation.Value.Type == "http://schemas.microsoft.com/office/2011/relationships/chartStyle")))
				{
					SerializeRelation(writer, relation.Key, relation.Value);
				}
			}
			else
			{
				SerializeRelation(writer, relation.Key, relation.Value);
			}
		}
		writer.WriteEndElement();
	}

	internal void serializeSheet(XmlWriter writer, WorksheetImpl sheet, Stream sheetStream, Dictionary<int, int> hashXFIndexes, Stream sheetAfterDataStream, Dictionary<string, string> extraAttributes, bool isCustomFile)
	{
		m_worksheetImpl = sheet;
		writer.WriteStartElement("worksheet", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
		writer.WriteAttributeString("xmlns", "r", null, "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
		writer.WriteAttributeString("xmlns", "x14", null, "http://schemas.microsoft.com/office/spreadsheetml/2009/9/main");
		writer.WriteAttributeString("xmlns", "mc", null, "http://schemas.openxmlformats.org/markup-compatibility/2006");
		if (isCustomFile && extraAttributes != null)
		{
			foreach (KeyValuePair<string, string> extraAttribute in extraAttributes)
			{
				if (extraAttribute.Key != "xmlns" && extraAttribute.Key != "xmlns:r" && extraAttribute.Key != "xmlns:x14" && extraAttribute.Key != "xmlns:mc")
				{
					writer.WriteAttributeString(extraAttribute.Key, extraAttribute.Value);
				}
			}
		}
		if (isCustomFile)
		{
			SerializeSheetlevelProperties(writer, sheet);
		}
		SerializeDimensions(writer, m_worksheetImpl);
		if (isCustomFile)
		{
			SerializeSheetViews(writer, sheet);
			SerializeSheetFormatProperties(writer, sheet);
			SerializeColumns(writer, sheet, hashXFIndexes);
		}
		SerializeSheetData(writer, sheet.CellRecords, hashXFIndexes, "c", null, isSpansNeeded: true);
		if (!isCustomFile || sheetAfterDataStream == null || sheetAfterDataStream.Length <= 0)
		{
			return;
		}
		sheetAfterDataStream.Position = 0L;
		XmlReader xmlReader = UtilityMethods.CreateReader(sheetAfterDataStream);
		while (xmlReader.Name == "root")
		{
			xmlReader.Read();
			if (xmlReader.EOF)
			{
				break;
			}
		}
		while (!xmlReader.EOF && (xmlReader.Name != "root" || xmlReader.NodeType != XmlNodeType.EndElement))
		{
			writer.WriteNode(xmlReader, defattr: false);
		}
	}

	public void SerializeWorksheet(XmlWriter writer, WorksheetImpl sheet, Stream streamStart, Stream streamConFormats, Dictionary<int, int> hashXFIndexes, Stream streamExtCondFormats)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (streamStart == null)
		{
			throw new ArgumentNullException("streamStart");
		}
		m_worksheetImpl = sheet;
		writer.WriteStartDocument(standalone: true);
		writer.WriteStartElement("worksheet", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
		writer.WriteAttributeString("xmlns", "r", null, "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
		writer.WriteAttributeString("xmlns", "x14", null, "http://schemas.microsoft.com/office/spreadsheetml/2009/9/main");
		writer.WriteAttributeString("xmlns", "mc", null, "http://schemas.openxmlformats.org/markup-compatibility/2006");
		SerializeSheetlevelProperties(writer, sheet);
		SerializeDimensions(writer, sheet);
		SerializeSheetViews(writer, sheet);
		SerializeSheetFormatProperties(writer, sheet);
		SerializeColumns(writer, sheet, hashXFIndexes);
		SerializeSheetData(writer, sheet.CellRecords, hashXFIndexes, "c", null, isSpansNeeded: true);
		SerializeSheetProtection(writer, sheet);
		SerializeMerges(writer, sheet.MergeCells);
		Stream data = ((streamConFormats != null && streamConFormats.Length != 0L) ? streamConFormats : GetWorksheetCFStream(sheet.Index));
		SerializeStream(writer, data);
		IPageSetupConstantsProvider constants = new WorksheetPageSetupConstants();
		SerializePrintSettings(writer, sheet.PageSetup, constants, isChartSettings: false);
		SerializePagebreaks(writer, sheet);
		SerializeDrawingsWorksheetPart(writer, sheet);
		SerializeVmlShapesWorksheetPart(writer, sheet);
		SerializeVmlHFShapesWorksheetPart(writer, sheet, constants, null);
		SerilizeBackgroundImage(writer, sheet);
		SerializeControls(writer, sheet);
		if (sheet.Version != 0 && sheet.Version != OfficeVersion.Excel2007)
		{
			new Excel2010Serializator(m_book).SerilaizeExtensions(writer, sheet);
		}
		writer.WriteEndElement();
	}

	private void SerializeSheetFormatProperties(XmlWriter writer, WorksheetImpl sheet)
	{
		writer.WriteStartElement("sheetFormatPr");
		if (sheet.DefaultColumnWidth != 8.43)
		{
			writer.WriteAttributeString("defaultColWidth", XmlConvert.ToString(sheet.DefaultColumnWidth));
		}
		else if (sheet.StandardWidth != 8.43)
		{
			writer.WriteAttributeString("defaultColWidth", XmlConvert.ToString(sheet.StandardWidth));
		}
		if (m_worksheetImpl.IsZeroHeight)
		{
			writer.WriteAttributeString("zeroHeight", XmlConvert.ToString(value: true));
		}
		if (sheet.CustomHeight)
		{
			writer.WriteAttributeString("customHeight", XmlConvert.ToString(value: true));
		}
		writer.WriteAttributeString("defaultRowHeight", XmlConvert.ToString(sheet.StandardHeight));
		if (sheet.OutlineLevelColumn > 0)
		{
			writer.WriteAttributeString("outlineLevelCol", XmlConvert.ToString(sheet.OutlineLevelColumn));
		}
		if (sheet.OutlineLevelRow > 0)
		{
			writer.WriteAttributeString("outlineLevelRow", XmlConvert.ToString(sheet.OutlineLevelRow));
		}
		if (sheet.BaseColumnWidth != 8)
		{
			writer.WriteAttributeString("baseColWidth", XmlConvert.ToString(sheet.BaseColumnWidth));
		}
		if (sheet.IsThickTop)
		{
			writer.WriteAttributeString("thickTop", XmlConvert.ToString(value: true));
		}
		if (sheet.IsThickBottom)
		{
			writer.WriteAttributeString("thickBottom", XmlConvert.ToString(value: true));
		}
		writer.WriteEndElement();
	}

	protected virtual void SerilaizeExtensions(XmlWriter writer, WorksheetImpl sheet)
	{
	}

	private void SerializeControls(XmlWriter writer, WorksheetImpl sheet)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		Stream controlsStream = sheet.DataHolder.ControlsStream;
		if (controlsStream != null)
		{
			bool num = HasAlternateContent(sheet.Shapes);
			if (num && sheet.HasAlternateContent)
			{
				WriteAlternateContentControlsHeader(writer);
			}
			controlsStream.Position = 0L;
			XmlReader reader = UtilityMethods.CreateReader(controlsStream);
			writer.WriteNode(reader, defattr: false);
			writer.Flush();
			if (num && sheet.HasAlternateContent)
			{
				WriteAlternateContentFooter(writer);
			}
		}
	}

	public static void WriteAlternateContentFooter(XmlWriter writer)
	{
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	public static void WriteAlternateContentHeader(XmlWriter writer)
	{
		writer.WriteStartElement("mc", "AlternateContent", "http://schemas.openxmlformats.org/markup-compatibility/2006");
		writer.WriteStartElement("mc", "Choice", "http://schemas.openxmlformats.org/markup-compatibility/2006");
		writer.WriteAttributeString("xmlns", "a14", null, "http://schemas.microsoft.com/office/drawing/2010/main");
		writer.WriteAttributeString("Requires", "a14");
	}

	public static void WriteAlternateContentControlsHeader(XmlWriter writer)
	{
		writer.WriteStartElement("mc", "AlternateContent", "http://schemas.openxmlformats.org/markup-compatibility/2006");
		writer.WriteAttributeString("xmlns", "mc", null, "http://schemas.openxmlformats.org/markup-compatibility/2006");
		writer.WriteStartElement("mc", "Choice", "http://schemas.openxmlformats.org/markup-compatibility/2006");
		writer.WriteAttributeString("Requires", "x14");
	}

	public void SerializeSheetProtection(XmlWriter writer, WorksheetBaseImpl sheet)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		OfficeSheetProtection innerProtection = sheet.InnerProtection;
		bool flag = sheet is ChartImpl;
		if (!sheet.ProtectContents && (!flag || innerProtection == OfficeSheetProtection.None))
		{
			return;
		}
		writer.WriteStartElement("sheetProtection");
		if (sheet.IsPasswordProtected)
		{
			if (sheet.AlgorithmName != null)
			{
				writer.WriteAttributeString("algorithmName", sheet.AlgorithmName);
				writer.WriteAttributeString("hashValue", Convert.ToBase64String(sheet.HashValue));
				writer.WriteAttributeString("saltValue", Convert.ToBase64String(sheet.SaltValue));
				writer.WriteAttributeString("spinCount", sheet.SpinCount.ToString());
			}
			else if (sheet.Password.IsPassword != 1)
			{
				string value = sheet.Password.IsPassword.ToString("X");
				writer.WriteAttributeString("password", value);
			}
		}
		ProtectionAttributeSerializator protectionAttributeSerializator;
		string[] array;
		bool[] array2;
		if (!flag)
		{
			protectionAttributeSerializator = SerializeProtectionAttribute;
			array = Protection.ProtectionAttributes;
			array2 = Protection.DefaultValues;
		}
		else
		{
			protectionAttributeSerializator = SerializeChartProtectionAttribute;
			array = Protection.ChartProtectionAttributes;
			array2 = Protection.ChartDefaultValues;
		}
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			protectionAttributeSerializator(writer, array[i], Protection.ProtectionFlags[i], array2[i], innerProtection);
		}
		writer.WriteEndElement();
	}

	private void SerializeProtectionAttribute(XmlWriter writer, string attributeName, OfficeSheetProtection flag, bool defaultValue, OfficeSheetProtection protection)
	{
		bool value = (protection & flag) == 0;
		SerializeAttribute(writer, attributeName, value, defaultValue);
	}

	private void SerializeChartProtectionAttribute(XmlWriter writer, string attributeName, OfficeSheetProtection flag, bool defaultValue, OfficeSheetProtection protection)
	{
		bool value = (protection & flag) != 0;
		SerializeAttribute(writer, attributeName, value, defaultValue);
	}

	private void SerializeIgnoreErrors(XmlWriter writer, WorksheetImpl sheet)
	{
	}

	private void SerializeWorksheetProperty(XmlWriter writer, WorksheetImpl sheet, ICustomProperty property, int counter)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (property == null)
		{
			throw new ArgumentNullException("property");
		}
		WorksheetDataHolder dataHolder = sheet.DataHolder;
		FileDataHolder parentHolder = dataHolder.ParentHolder;
		RelationCollection relations = dataHolder.Relations;
		writer.WriteStartElement("customPr");
		writer.WriteAttributeString("name", property.Name);
		ZipArchiveItem item;
		string value = parentHolder.PrepareNewItem("xl/customProperty", "bin", "application/vnd.openxmlformats-officedocument.spreadsheetml.customProperty", relations, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/customProperty", ref counter, out item);
		writer.WriteAttributeString("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships", value);
		WritePropertyValue(sheet, item, property);
		writer.WriteEndElement();
	}

	private void WritePropertyValue(WorksheetImpl sheet, ZipArchiveItem item, ICustomProperty property)
	{
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		if (property == null)
		{
			throw new ArgumentNullException("property");
		}
		byte[] bytes = Encoding.Unicode.GetBytes(property.Value);
		item.DataStream.Write(bytes, 0, bytes.Length);
	}

	private Stream GetWorksheetCFStream(int iSheetIndex)
	{
		if (m_streamsSheetsCF == null)
		{
			return null;
		}
		return m_streamsSheetsCF[iSheetIndex];
	}

	public void SerializeCommentNotes(XmlWriter writer, WorksheetImpl sheet)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		writer.WriteStartDocument(standalone: true);
		writer.WriteStartElement("comments", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
		writer.WriteEndElement();
		WorksheetDataHolder dataHolder = sheet.DataHolder;
		if (dataHolder.CommentNotesId == null)
		{
			dataHolder.CommentNotesId = dataHolder.Relations.GenerateRelationId();
		}
	}

	public void SerializeVmlShapes(XmlWriter writer, ShapeCollectionBase shapes, WorksheetDataHolder holder, Dictionary<int, ShapeSerializator> dictSerializators, RelationCollection vmlRelations)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (shapes == null)
		{
			throw new ArgumentNullException("shapes");
		}
		writer.WriteStartElement("xml");
		writer.WriteAttributeString("xmlns", "v", null, "urn:schemas-microsoft-com:vml");
		writer.WriteAttributeString("xmlns", "o", null, "urn:schemas-microsoft-com:office:office");
		writer.WriteAttributeString("xmlns", "x", null, "urn:schemas-microsoft-com:office:excel");
		UniqueInstanceTypeList uniqueInstanceTypeList = new UniqueInstanceTypeList();
		Dictionary<Stream, object> dictionary = new Dictionary<Stream, object>();
		int i = 0;
		for (int count = shapes.Count; i < count; i++)
		{
			ShapeImpl shapeImpl = shapes[i] as ShapeImpl;
			shapeImpl.PrepareForSerialization();
			if (!shapeImpl.VmlShape)
			{
				continue;
			}
			if (shapeImpl.XmlTypeStream == null)
			{
				uniqueInstanceTypeList.AddShape(shapeImpl);
				continue;
			}
			dictionary[shapeImpl.XmlTypeStream] = null;
			if (shapeImpl.ImageRelation != null)
			{
				vmlRelations[shapeImpl.ImageRelationId] = shapeImpl.ImageRelation;
			}
		}
		if (shapes.ShapeLayoutStream != null)
		{
			Stream shapeLayoutStream = shapes.ShapeLayoutStream;
			shapeLayoutStream.Position = 0L;
			ShapeParser.WriteNodeFromStream(writer, shapeLayoutStream);
		}
		foreach (KeyValuePair<int, Type> item in uniqueInstanceTypeList.UniquePairs())
		{
			int key = item.Key;
			Type value = item.Value;
			if (dictSerializators.TryGetValue(key, out var value2))
			{
				value2.SerializeShapeType(writer, value);
			}
		}
		foreach (Stream key2 in dictionary.Keys)
		{
			key2.Position = 0L;
			XmlReader reader = UtilityMethods.CreateReader(key2);
			writer.WriteNode(reader, defattr: false);
		}
		int j = 0;
		for (int count2 = shapes.Count; j < count2; j++)
		{
			ShapeImpl shapeImpl2 = (ShapeImpl)shapes[j];
			shapeImpl2.PrepareForSerialization();
			int instance = shapeImpl2.Instance;
			if (shapeImpl2.VmlShape)
			{
				if ((shapeImpl2.XmlDataStream == null || shapeImpl2.EnableAlternateContent) && dictSerializators.TryGetValue(instance, out var value3))
				{
					value3.Serialize(writer, shapeImpl2, holder, vmlRelations);
				}
				else if (shapeImpl2.XmlDataStream != null)
				{
					Stream xmlDataStream = shapeImpl2.XmlDataStream;
					xmlDataStream.Position = 0L;
					XmlReader reader2 = UtilityMethods.CreateReader(xmlDataStream);
					writer.WriteNode(reader2, defattr: false);
				}
			}
		}
		writer.WriteEndElement();
		writer.Flush();
	}

	public void SerializeDrawings(XmlWriter writer, ShapesCollection shapes, WorksheetDataHolder holder)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (shapes == null)
		{
			throw new ArgumentNullException("shapes");
		}
		if (shapes.Count - shapes.WorksheetBase.VmlShapesCount <= 0 && !HasAlternateContent(shapes))
		{
			return;
		}
		bool flag = shapes.Worksheet == null;
		string text = null;
		string prefix = null;
		string localName;
		string text2;
		string localName2;
		if (flag)
		{
			localName = "cdr";
			text2 = "http://schemas.openxmlformats.org/drawingml/2006/chartDrawing";
			localName2 = "userShapes";
			text = "http://schemas.openxmlformats.org/drawingml/2006/chart";
		}
		else
		{
			localName = "xdr";
			text2 = "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing";
			prefix = "xdr";
			localName2 = "wsDr";
			text = text2;
		}
		writer.WriteStartDocument(standalone: true);
		writer.WriteStartElement(prefix, localName2, text);
		writer.WriteAttributeString("xmlns", localName, null, text2);
		writer.WriteAttributeString("xmlns", "a", null, "http://schemas.openxmlformats.org/drawingml/2006/main");
		int num = 0;
		int i = 0;
		for (int count = shapes.Count; i < count; i++)
		{
			ShapeImpl shapeImpl = (ShapeImpl)shapes[i];
			if (shapeImpl.ShapeType == OfficeShapeType.AutoShape)
			{
				Serializator serializator = new Serializator();
				AutoShapeImpl autoShapeImpl = shapeImpl as AutoShapeImpl;
				if (autoShapeImpl.ShapeExt.ShapeID <= 0)
				{
					autoShapeImpl.ShapeExt.ShapeID = i + 1;
				}
				serializator.AddShape(autoShapeImpl.ShapeExt, writer);
			}
			else if (!shapeImpl.VmlShape || shapeImpl.EnableAlternateContent)
			{
				if (m_shapesSerializators.TryGetValue(shapeImpl.GetType(), out var value))
				{
					value.Serialize(writer, shapeImpl, holder, holder.DrawingsRelations);
				}
				else if ((!shapeImpl.VmlShape || shapeImpl.EnableAlternateContent) && shapeImpl.XmlDataStream != null && !flag)
				{
					new DrawingShapeSerializator().Serialize(writer, shapeImpl, holder, holder.DrawingsRelations);
				}
				else if (Worksheet.preservedStreams != null || shapeImpl.preservedCnxnShapeStreams != null || shapeImpl.preservedPictureStreams != null || shapeImpl.preservedShapeStreams != null)
				{
					SerializeShape(writer, shapeImpl, holder, holder.DrawingsRelations, num);
					num++;
				}
			}
		}
		writer.WriteEndElement();
	}

	public void SerializeShape(XmlWriter writer, ShapeImpl shape, WorksheetDataHolder holder, RelationCollection vmlRelations, int index)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (shape == null)
		{
			throw new ArgumentNullException("shape");
		}
		if (holder == null)
		{
			throw new ArgumentNullException("holder");
		}
		shape.GetType();
		writer.WriteStartElement("twoCellAnchor", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		DrawingShapeSerializator drawingShapeSerializator = new DrawingShapeSerializator();
		drawingShapeSerializator.SerializeAnchorPoint(writer, "from", shape.LeftColumn, shape.LeftColumnOffset, shape.TopRow, shape.TopRowOffset, shape.Worksheet, "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		drawingShapeSerializator.SerializeAnchorPoint(writer, "to", shape.RightColumn, shape.RightColumnOffset, shape.BottomRow, shape.BottomRowOffset, shape.Worksheet, "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		if (shape.preservedCnxnShapeStreams != null)
		{
			for (int i = 0; i < shape.preservedCnxnShapeStreams.Count; i++)
			{
				SerializeStream(writer, shape.preservedCnxnShapeStreams[i]);
			}
		}
		if (shape.GraphicFrameStream != null)
		{
			ChartShapeImpl chartShapeImpl = (ChartShapeImpl)shape.ChildShapes[0];
			ChartImpl chartObject = chartShapeImpl.ChartObject;
			ChartShapeSerializator chartShapeSerializator = new ChartShapeSerializator();
			chartShapeSerializator.SerializeChartFile(holder, chartObject, out var _);
			writer.WriteStartElement("graphicFrame", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
			writer.WriteAttributeString("macro", string.Empty);
			chartShapeSerializator.SerializeNonVisualGraphicFrameProperties(writer, chartShapeImpl, holder);
			DrawingShapeSerializator.SerializeForm(writer, "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing", "http://schemas.openxmlformats.org/drawingml/2006/main", chartShapeImpl.OffsetX, chartShapeImpl.OffsetY, chartShapeImpl.ExtentsX, chartShapeImpl.ExtentsY);
			chartShapeSerializator.SerializeSlicerGraphics(writer, chartShapeImpl);
			shape.ChildShapes.Remove(chartShapeImpl);
		}
		else if (shape.preservedShapeStreams != null || shape.preservedPictureStreams != null || shape.ChildShapes.Count > 0)
		{
			writer.WriteStartElement("grpSp", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
			index *= 2;
			if (Worksheet.preservedStreams != null && index < Worksheet.preservedStreams.Count)
			{
				SerializeStream(writer, Worksheet.preservedStreams[index]);
				SerializeStream(writer, Worksheet.preservedStreams[index + 1]);
			}
			if (shape.preservedShapeStreams != null)
			{
				for (int j = 0; j < shape.preservedShapeStreams.Count; j++)
				{
					SerializeStream(writer, shape.preservedShapeStreams[j]);
				}
			}
			if (shape.preservedPictureStreams != null)
			{
				for (int k = 0; k < shape.preservedPictureStreams.Count; k++)
				{
					SerializeStream(writer, shape.preservedPictureStreams[k]);
				}
			}
			if (shape.preservedInnerCnxnShapeStreams != null)
			{
				for (int l = 0; l < shape.preservedInnerCnxnShapeStreams.Count; l++)
				{
					SerializeStream(writer, shape.preservedInnerCnxnShapeStreams[l]);
				}
			}
			for (int m = 0; m < shape.ChildShapes.Count; m++)
			{
				ChartShapeImpl chartShapeImpl2 = (ChartShapeImpl)shape.ChildShapes[m];
				ChartImpl chartObject2 = chartShapeImpl2.ChartObject;
				ChartShapeSerializator chartShapeSerializator2 = new ChartShapeSerializator();
				string chartFileName2;
				string strRelationId = chartShapeSerializator2.SerializeChartFile(holder, chartObject2, out chartFileName2);
				chartShapeSerializator2.SerializeChartProperties(writer, chartShapeImpl2, strRelationId, holder, isGroupShape: true);
				holder.SerializeRelations(chartObject2.Relations, chartFileName2.Substring(1), null);
			}
			writer.WriteEndElement();
		}
		writer.WriteElementString("clientData", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing", string.Empty);
		writer.WriteEndElement();
	}

	public static bool HasAlternateContent(IShapes shapes)
	{
		foreach (ShapeImpl shape in shapes)
		{
			if (shape.EnableAlternateContent)
			{
				return true;
			}
		}
		return false;
	}

	public RelationCollection SerializeLinkItem(XmlWriter writer, ExternWorkbookImpl book)
	{
		if (book.IsAddInFunctions || book.IsInternalReference)
		{
			return null;
		}
		if (!book.IsOleLink)
		{
			return SerializeExternalLink(writer, book);
		}
		return SerializeOleObjectLink(writer, book);
	}

	public RelationCollection SerializeExternalLink(XmlWriter writer, ExternWorkbookImpl book)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (book == null)
		{
			throw new ArgumentNullException("book");
		}
		if (book.IsInternalReference || book.IsAddInFunctions)
		{
			return null;
		}
		RelationCollection relationCollection = new RelationCollection();
		string text = relationCollection.GenerateRelationId();
		string text2 = ConvertAddressString(book.URL);
		bool flag = true;
		if (!text2.StartsWith("file:///") && !text2.StartsWith("http://") && text2[0] != '/')
		{
			if (!text2.Contains(":\\"))
			{
				text2.StartsWith("\\");
			}
			if (flag)
			{
				text2 = "file:///" + text2;
			}
		}
		string type = (flag ? "http://schemas.openxmlformats.org/officeDocument/2006/relationships/externalLinkPath" : "http://schemas.microsoft.com/office/2006/relationships/xlExternalLinkPath/xlPathMissing");
		relationCollection[text] = new Relation(text2, type, isExternal: true);
		writer.WriteStartElement("externalLink", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
		writer.WriteStartElement("externalBook");
		writer.WriteAttributeString("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships", text);
		SerializeSheetNames(writer, book);
		SerializeExternNames(writer, book);
		SerializeSheetDataSet(writer, book);
		writer.WriteEndElement();
		writer.WriteEndElement();
		return relationCollection;
	}

	public RelationCollection SerializeOleObjectLink(XmlWriter writer, ExternWorkbookImpl book)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (book == null)
		{
			throw new ArgumentNullException("book");
		}
		RelationCollection relationCollection = new RelationCollection();
		string text = relationCollection.GenerateRelationId();
		string text2 = ConvertAddressString(book.URL);
		bool flag = true;
		if (!text2.StartsWith("file:///") && !text2.StartsWith("http://") && text2[0] != '/')
		{
			if (!text2.Contains(":\\"))
			{
				text2.StartsWith("\\");
			}
			if (flag)
			{
				text2 = "file:///" + text2;
			}
		}
		string type = (flag ? "http://schemas.openxmlformats.org/officeDocument/2006/relationships/oleObject" : "http://schemas.microsoft.com/office/2006/relationships/xlExternalLinkPath/xlPathMissing");
		relationCollection[text] = new Relation(text2, type, isExternal: true);
		writer.WriteStartDocument(standalone: true);
		writer.WriteStartElement("externalLink", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
		writer.WriteStartElement("oleLink");
		writer.WriteAttributeString("xmlns", "r", null, "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
		writer.WriteAttributeString("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships", text);
		writer.WriteAttributeString("progId", book.ProgramId);
		writer.WriteStartElement("oleItems");
		writer.WriteStartElement("oleItem");
		writer.WriteAttributeString("name", "'");
		writer.WriteAttributeString("advise", "1");
		writer.WriteAttributeString("preferPic", "1");
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
		return relationCollection;
	}

	private void SerializeSheetDataSet(XmlWriter writer, ExternWorkbookImpl book)
	{
		int count = book.Worksheets.Count;
		if (count <= 0)
		{
			return;
		}
		writer.WriteStartElement("sheetDataSet");
		for (int i = 0; i < count; i++)
		{
			ExternWorksheetImpl externWorksheetImpl = book.Worksheets.Values[i];
			Dictionary<string, string> dictionary = externWorksheetImpl.AdditionalAttributes;
			if (dictionary == null)
			{
				dictionary = new Dictionary<string, string>();
			}
			dictionary["sheetId"] = externWorksheetImpl.Index.ToString();
			SerializeSheetData(writer, externWorksheetImpl.CellRecords, null, "cell", dictionary, isSpansNeeded: false);
		}
		writer.WriteEndElement();
	}

	private void SerializeSheetNames(XmlWriter writer, ExternWorkbookImpl book)
	{
		int sheetNumber = book.SheetNumber;
		if (sheetNumber > 0)
		{
			writer.WriteStartElement("sheetNames");
			for (int i = 0; i < sheetNumber; i++)
			{
				string sheetName = book.GetSheetName(i);
				writer.WriteStartElement("sheetName");
				writer.WriteAttributeString("val", sheetName);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}
	}

	private void SerializeExternNames(XmlWriter writer, ExternWorkbookImpl book)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (book == null)
		{
			throw new ArgumentNullException("book");
		}
		ExternNamesCollection externNames = book.ExternNames;
		int count = externNames.Count;
		if (count > 0)
		{
			writer.WriteStartElement("definedNames");
			for (int i = 0; i < count; i++)
			{
				SerializeExternName(writer, externNames[i]);
			}
			writer.WriteEndElement();
		}
	}

	private void SerializeExternName(XmlWriter writer, ExternNameImpl externName)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (externName == null)
		{
			throw new ArgumentNullException("externName");
		}
		_ = externName.Record;
		writer.WriteStartElement("definedName");
		string empty = string.Empty;
		empty = ((!externName.Name.Contains("\0")) ? externName.Name : externName.Name.Replace("\0", string.Empty));
		writer.WriteAttributeString("name", empty);
		if (externName.RefersTo != null)
		{
			writer.WriteAttributeString("refersTo", externName.RefersTo);
		}
		int sheetId = externName.sheetId;
		writer.WriteAttributeString("sheetId", sheetId.ToString());
		writer.WriteEndElement();
	}

	private void SerializeDrawingsWorksheetPart(XmlWriter writer, WorksheetImpl sheet)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (sheet.Shapes.Count - sheet.VmlShapesCount > 0 || HasAlternateContent(sheet.Shapes))
		{
			WorksheetDataHolder dataHolder = sheet.DataHolder;
			string text = dataHolder.DrawingsId;
			if (text == null)
			{
				text = (dataHolder.DrawingsId = dataHolder.Relations.GenerateRelationId());
				dataHolder.Relations[text] = null;
			}
			writer.WriteStartElement("drawing");
			writer.WriteAttributeString("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships", text);
			writer.WriteEndElement();
		}
	}

	public void SerializeVmlShapesWorksheetPart(XmlWriter writer, WorksheetBaseImpl sheet)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		WorksheetDataHolder dataHolder = sheet.DataHolder;
		string text = dataHolder.VmlDrawingsId;
		if (text == null)
		{
			text = (dataHolder.VmlDrawingsId = dataHolder.Relations.GenerateRelationId());
			dataHolder.Relations[text] = null;
		}
		writer.WriteStartElement("legacyDrawing");
		writer.WriteAttributeString("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships", text);
		writer.WriteEndElement();
	}

	public static void SerializeVmlHFShapesWorksheetPart(XmlWriter writer, WorksheetBaseImpl sheet, IPageSetupConstantsProvider constants, RelationCollection relations)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		HeaderFooterShapeCollection innerHeaderFooterShapes = sheet.InnerHeaderFooterShapes;
		if (innerHeaderFooterShapes == null || innerHeaderFooterShapes.Count == 0)
		{
			return;
		}
		WorksheetDataHolder dataHolder = sheet.DataHolder;
		string text = dataHolder.VmlHFDrawingsId;
		if (text == null)
		{
			if (relations == null)
			{
				relations = dataHolder.Relations;
			}
			text = (dataHolder.VmlHFDrawingsId = relations.GenerateRelationId());
			relations[text] = null;
		}
		writer.WriteStartElement("legacyDrawingHF");
		writer.WriteAttributeString("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships", text);
		writer.WriteEndElement();
	}

	private void SerializeDimensions(XmlWriter writer, WorksheetImpl sheet)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet.FirstRow > 0 && sheet.FirstColumn > 0 && sheet.LastColumn <= sheet.Workbook.MaxColumnCount)
		{
			writer.WriteStartElement("dimension");
			writer.WriteAttributeString("ref", sheet.UsedRange.AddressLocal);
			writer.WriteEndElement();
		}
	}

	private void SerializeSheetViews(XmlWriter writer, WorksheetImpl sheet)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		writer.WriteStartElement("sheetViews");
		writer.WriteStartElement("sheetView");
		IWorkbook workbook = sheet.Workbook;
		SerializeAttribute(writer, "windowProtection", workbook.IsWindowProtection, defaultValue: false);
		IRange topLeftCell = sheet.TopLeftCell;
		if (sheet.IsFreezePanes && topLeftCell != null && (topLeftCell.Row != 1 || topLeftCell.Column != 1))
		{
			writer.WriteAttributeString("topLeftCell", topLeftCell.AddressLocal);
		}
		SerializeAttribute(writer, "showGridLines", sheet.IsGridLinesVisible, defaultValue: true);
		SerializeAttribute(writer, "showRowColHeaders", sheet.IsRowColumnHeadersVisible, defaultValue: true);
		SerializeAttribute(writer, "showZeros", sheet.IsDisplayZeros, defaultValue: true);
		SerializeAttribute(writer, "zoomScale", sheet.Zoom, 100);
		SerializeAttribute(writer, "rightToLeft", sheet.IsRightToLeft, defaultValue: false);
		if (!sheet.WindowTwo.IsDefaultHeader)
		{
			writer.WriteAttributeString("defaultGridColor", "0");
			writer.WriteAttributeString("colorId", ((int)sheet.GridLineColor).ToString());
		}
		if (sheet.WindowTwo.IsSavedInPageBreakPreview)
		{
			writer.WriteAttributeString("view", "pageBreakPreview");
		}
		else
		{
			string value = "normal";
			if (sheet.View == OfficeSheetView.PageLayout)
			{
				value = "pageLayout";
			}
			writer.WriteAttributeString("view", value);
		}
		writer.WriteAttributeString("workbookViewId", "0");
		SerializePane(writer, sheet);
		SerializeSelection(writer, sheet);
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	private void SerializeSelection(XmlWriter writer, WorksheetImpl sheet)
	{
		IRange activeCell = sheet.GetActiveCell();
		if (activeCell != null)
		{
			string addressLocal = activeCell.AddressLocal;
			writer.WriteStartElement("selection");
			if (sheet.Pane != null)
			{
				writer.WriteAttributeString("pane", ((Pane.ActivePane)GetActivePane(sheet.Pane)).ToString());
			}
			writer.WriteAttributeString("activeCell", addressLocal);
			writer.WriteAttributeString("sqref", addressLocal);
			writer.WriteEndElement();
		}
	}

	private void SerializePane(XmlWriter writer, WorksheetImpl sheet)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (!sheet.IsFreezePanes && sheet.VerticalSplit == 0 && sheet.HorizontalSplit == 0)
		{
			return;
		}
		PaneRecord pane = sheet.Pane;
		if (pane != null && (pane.VerticalSplit > 0 || pane.HorizontalSplit > 0))
		{
			writer.WriteStartElement("pane");
			SerializeAttribute(writer, "xSplit", pane.VerticalSplit, 0);
			SerializeAttribute(writer, "ySplit", pane.HorizontalSplit, 0);
			string cellName = RangeImpl.GetCellName(pane.FirstColumn + 1, pane.FirstRow + 1);
			writer.WriteAttributeString("topLeftCell", cellName);
			string value = ((Pane.ActivePane)pane.ActivePane).ToString();
			writer.WriteAttributeString("activePane", value);
			WindowTwoRecord windowTwo = sheet.WindowTwo;
			if (windowTwo.IsFreezePanes && !windowTwo.IsFreezePanesNoSplit)
			{
				string value2 = "frozenSplit";
				writer.WriteAttributeString("state", value2);
			}
			else if (windowTwo.IsFreezePanes && windowTwo.IsFreezePanesNoSplit)
			{
				string value2 = "frozen";
				writer.WriteAttributeString("state", value2);
			}
			else
			{
				string value2 = "split";
			}
			writer.WriteEndElement();
		}
	}

	private void SerializeStream(XmlWriter writer, Stream data)
	{
		SerializeStream(writer, data, "root");
	}

	public static void SerializeStream(XmlWriter writer, Stream data, string strRootName)
	{
		if (data != null && data.Length > 0)
		{
			data.Position = 0L;
			XmlReader xmlReader = UtilityMethods.CreateReader(data);
			while (xmlReader.Name == strRootName || xmlReader.Name == "root")
			{
				xmlReader.Read();
			}
			while (!xmlReader.EOF && ((xmlReader.Name != strRootName && xmlReader.Name != "root") || xmlReader.NodeType != XmlNodeType.EndElement))
			{
				writer.WriteNode(xmlReader, defattr: false);
			}
		}
	}

	private void SerializeRelation(XmlWriter writer, string key, Relation relation)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (relation == null)
		{
			throw new ArgumentNullException("relation");
		}
		writer.WriteStartElement("Relationship");
		writer.WriteAttributeString("Id", key);
		writer.WriteAttributeString("Type", relation.Type);
		writer.WriteAttributeString("Target", relation.Target);
		if (relation.IsExternal)
		{
			writer.WriteAttributeString("TargetMode", "External");
		}
		writer.WriteEndElement();
	}

	private void SerializeSheets(XmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		writer.WriteStartElement("sheets");
		ITabSheets tabSheets = m_book.TabSheets;
		int i = 0;
		for (int count = tabSheets.Count; i < count; i++)
		{
			if (((WorksheetBaseImpl)tabSheets[i]).m_dataHolder != null)
			{
				SerializeSheetTag(writer, tabSheets[i]);
			}
		}
		writer.WriteEndElement();
	}

	private void SerializeSheetTag(XmlWriter writer, ITabSheet sheet)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		WorksheetDataHolder dataHolder = ((WorksheetBaseImpl)sheet).m_dataHolder;
		string text = dataHolder?.SheetId;
		if (text == null)
		{
			text = GenerateSheetId();
			if (dataHolder != null)
			{
				dataHolder.SheetId = text;
			}
		}
		writer.WriteStartElement("sheet");
		writer.WriteAttributeString("name", sheet.Name);
		writer.WriteAttributeString("sheetId", text);
		writer.WriteAttributeString("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships", dataHolder.RelationId);
		Worksheet2007Visibility visibility = (Worksheet2007Visibility)sheet.Visibility;
		if (visibility != 0)
		{
			string value = visibility.ToString();
			value = LowerFirstLetter(value);
			writer.WriteAttributeString("state", value);
		}
		writer.WriteEndElement();
	}

	private string GenerateSheetId()
	{
		WorkbookObjectsCollection objects = m_book.Objects;
		int num = 0;
		int i = 0;
		for (int count = objects.Count; i < count; i++)
		{
			WorksheetDataHolder dataHolder = ((WorksheetBaseImpl)objects[i]).DataHolder;
			if (dataHolder != null)
			{
				string sheetId = dataHolder.SheetId;
				if (sheetId != null && int.TryParse(sheetId, out var result) && result > num)
				{
					num = result;
				}
			}
		}
		return (num + 1).ToString();
	}

	private string GetRangeName(MergeCellsRecord.MergedRegion region)
	{
		if (region == null)
		{
			throw new ArgumentNullException("region");
		}
		string cellName = RangeImpl.GetCellName(region.ColumnFrom + 1, region.RowFrom + 1);
		string cellName2 = RangeImpl.GetCellName(region.ColumnTo + 1, region.RowTo + 1);
		return cellName + ":" + cellName2;
	}

	private void SerializeNamedRange(XmlWriter writer, IName name)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		string text = ((NameImpl)name).GetValue(m_formulaUtil);
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		writer.WriteStartElement("definedName");
		writer.WriteAttributeString("name", m_book.RemoveInvalidXmlCharacters(name.Name));
		if (name.IsLocal)
		{
			WorksheetImpl worksheet = ((NameImpl)name).Worksheet;
			string value = GetLocalSheetIndex(worksheet).ToString();
			writer.WriteAttributeString("localSheetId", value);
		}
		else if (name is NameImpl && (name as NameImpl).IsQueryTableRange)
		{
			writer.WriteAttributeString("localSheetId", (name as NameImpl).SheetIndex.ToString());
		}
		SerializeAttribute(writer, "hidden", !name.Visible, defaultValue: false);
		if (text == null)
		{
			text = "#NAME?";
		}
		if (!m_book.HasApostrophe && !CheckSheetName(text))
		{
			text = text.Replace("'", "");
		}
		if (m_sheetNames == null)
		{
			m_sheetNames = new List<string>();
			if (Worksheet != null)
			{
				for (int i = 0; i < Worksheet.Workbook.Worksheets.Count; i++)
				{
					m_sheetNames.Add(Worksheet.Workbook.Worksheets[i].Name);
				}
			}
			else if (name.Worksheet != null)
			{
				for (int j = 0; j < name.Worksheet.Workbook.Worksheets.Count; j++)
				{
					m_sheetNames.Add(name.Worksheet.Workbook.Worksheets[j].Name);
				}
			}
		}
		if (text.StartsWith("#REF"))
		{
			text = "#REF!";
		}
		if (text != null && text.Contains("!$") && !text.Contains("#REF"))
		{
			string text2 = text.Substring(0, text.IndexOf("!$"));
			if (CheckSpecialCharacters(text2) && m_sheetNames.Contains(text2))
			{
				text = text.Replace(text2, "'" + text2 + "'");
			}
		}
		if ((name as NameImpl).IsCommon)
		{
			text = text.Substring(text.IndexOf("!"));
			writer.WriteString(text);
		}
		else
		{
			writer.WriteString(text);
		}
		writer.WriteEndElement();
	}

	private int GetLocalSheetIndex(WorksheetImpl sheet)
	{
		int num = -1;
		ITabSheets tabSheets = m_book.TabSheets;
		for (int i = 0; i < tabSheets.Count; i++)
		{
			if (tabSheets[i].Name == sheet.Name)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			throw new ArgumentException("Invalid Sheet");
		}
		return num;
	}

	private bool CheckSheetName(string strNameValue)
	{
		char[] array = strNameValue.ToCharArray();
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			if (!char.IsLetterOrDigit(array[i]))
			{
				return true;
			}
		}
		return false;
	}

	private void SerializeFonts(XmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		FontsCollection innerFonts = m_book.InnerFonts;
		int count = innerFonts.Count;
		writer.WriteStartElement("fonts");
		writer.WriteAttributeString("count", count.ToString());
		for (int i = 0; i < count; i++)
		{
			IOfficeFont font = innerFonts[i];
			SerializeFont(writer, font, "font");
		}
		writer.WriteEndElement();
	}

	private void SerializeFont(XmlWriter writer, IOfficeFont font, string strElement)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		writer.WriteStartElement(strElement);
		if (font.Bold)
		{
			writer.WriteElementString("b", string.Empty);
		}
		if (font.VerticalAlignment != 0)
		{
			writer.WriteStartElement("vertAlign");
			writer.WriteAttributeString("val", font.VerticalAlignment.ToString().ToLower());
			writer.WriteEndElement();
		}
		if (font.Italic)
		{
			writer.WriteElementString("i", string.Empty);
		}
		OfficeUnderline underline = font.Underline;
		if (underline != 0)
		{
			writer.WriteStartElement("u");
			string text = underline.ToString();
			text = char.ToLower(text[0]) + UtilityMethods.RemoveFirstCharUnsafe(text);
			writer.WriteAttributeString("val", text);
			writer.WriteEndElement();
		}
		if (font.Strikethrough)
		{
			writer.WriteElementString("strike", string.Empty);
		}
		writer.WriteStartElement("sz");
		writer.WriteAttributeString("val", XmlConvert.ToString(font.Size));
		writer.WriteEndElement();
		if (font.Color != (OfficeKnownColors)32767)
		{
			SerializeFontColor(writer, "color", (font as IInternalFont).Font.ColorObject);
		}
		string localName = "name";
		if (strElement == "rPr")
		{
			localName = "rFont";
		}
		writer.WriteStartElement(localName);
		writer.WriteAttributeString("val", font.FontName);
		writer.WriteEndElement();
		int charSet = ((FontImpl)font).CharSet;
		if (charSet != 1)
		{
			writer.WriteStartElement("charset");
			writer.WriteAttributeString("val", charSet.ToString());
			writer.WriteEndElement();
		}
		if (font.MacOSShadow)
		{
			writer.WriteElementString("shadow", string.Empty);
		}
		writer.WriteEndElement();
	}

	private void SerializeFontColor(XmlWriter writer, string tagName, ChartColor color)
	{
		writer.WriteStartElement(tagName);
		switch (color.ColorType)
		{
		case ColorType.Indexed:
			writer.WriteAttributeString("indexed", color.Value.ToString());
			break;
		case ColorType.RGB:
			writer.WriteAttributeString("rgb", color.Value.ToString("X8"));
			break;
		case ColorType.Theme:
			writer.WriteAttributeString("theme", color.Value.ToString());
			break;
		}
		SerializeAttribute(writer, "tint", color.Tint, 0.0);
		writer.WriteEndElement();
	}

	private void SerializeNumberFormats(XmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		List<FormatRecord> usedFormats = m_book.InnerFormats.GetUsedFormats(OfficeVersion.Excel2007);
		int count = usedFormats.Count;
		if (count != 0)
		{
			writer.WriteStartElement("numFmts");
			writer.WriteAttributeString("count", count.ToString());
			for (int i = 0; i < count; i++)
			{
				SerializeNumberFormat(writer, usedFormats[i]);
			}
			writer.WriteEndElement();
		}
	}

	private void SerializeNumberFormat(XmlWriter writer, FormatRecord format)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		writer.WriteStartElement("numFmt");
		writer.WriteAttributeString("numFmtId", format.Index.ToString());
		string value = format.FormatString;
		if (format.FormatString.Equals("Standard"))
		{
			value = "General";
		}
		writer.WriteAttributeString("formatCode", value);
		writer.WriteEndElement();
	}

	private int[] SerializeFills(XmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		Dictionary<FillImpl, int> dictionary = new Dictionary<FillImpl, int>();
		ExtendedFormatsCollection innerExtFormats = m_book.InnerExtFormats;
		int count = innerExtFormats.Count;
		int[] array = new int[count];
		FillImpl[] array2 = new FillImpl[count];
		int num = -1;
		for (int i = 0; i < count; i++)
		{
			FillImpl fillImpl;
			switch (num)
			{
			case -1:
				fillImpl = new FillImpl();
				fillImpl.Pattern = OfficePattern.None;
				fillImpl.PatternColorObject.SetIndexed(OfficeKnownColors.BlackCustom);
				fillImpl.ColorObject.SetIndexed((OfficeKnownColors)65);
				break;
			case 0:
				fillImpl = new FillImpl();
				fillImpl.Pattern = OfficePattern.Percent10;
				fillImpl.PatternColorObject.SetIndexed(OfficeKnownColors.BlackCustom);
				fillImpl.ColorObject.SetIndexed((OfficeKnownColors)65);
				break;
			default:
				fillImpl = new FillImpl(innerExtFormats[i]);
				break;
			}
			if (dictionary.ContainsKey(fillImpl))
			{
				array[i] = dictionary[fillImpl];
				continue;
			}
			num = dictionary.Count;
			dictionary.Add(fillImpl, num);
			if (num >= array2.Length)
			{
				Array.Resize(ref array2, num + 1);
			}
			array2[num] = fillImpl;
			array[i] = num;
			if (num == 0 || num == 1)
			{
				i--;
			}
		}
		writer.WriteStartElement("fills");
		writer.WriteAttributeString("count", (num + 1).ToString());
		for (int j = 0; j <= num; j++)
		{
			SerializeFill(writer, array2[j]);
		}
		writer.WriteEndElement();
		return array;
	}

	internal void SerializeFill(XmlWriter writer, FillImpl fill)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (fill == null)
		{
			throw new ArgumentNullException("fill");
		}
		writer.WriteStartElement("fill");
		if (fill.Pattern == OfficePattern.Gradient)
		{
			SerializeGradientFill(writer, fill);
		}
		else
		{
			SerializePatternFill(writer, fill);
		}
		writer.WriteEndElement();
	}

	private void SerializePatternFill(XmlWriter writer, FillImpl fill)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (fill == null)
		{
			throw new ArgumentNullException("fill");
		}
		writer.WriteStartElement("patternFill");
		writer.WriteAttributeString("patternType", ConvertPatternToString(fill.Pattern));
		if (fill.Pattern == OfficePattern.Solid)
		{
			SerializeColorObject(writer, "fgColor", fill.ColorObject);
			SerializeColorObject(writer, "bgColor", fill.PatternColorObject);
		}
		else
		{
			ChartColor patternColorObject = fill.PatternColorObject;
			if (patternColorObject.ColorType != ColorType.Indexed || patternColorObject.GetIndexed(m_book) != (OfficeKnownColors)65)
			{
				SerializeColorObject(writer, "fgColor", patternColorObject);
			}
			patternColorObject = fill.ColorObject;
			if (patternColorObject.ColorType != ColorType.Indexed || patternColorObject.GetIndexed(m_book) != OfficeKnownColors.BlackCustom)
			{
				SerializeColorObject(writer, "bgColor", patternColorObject);
			}
		}
		writer.WriteEndElement();
	}

	private void SerializeGradientFill(XmlWriter writer, FillImpl fill)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (fill == null)
		{
			throw new ArgumentNullException("fill");
		}
		writer.WriteStartElement("gradientFill");
		OfficeGradientStyle gradientStyle = fill.GradientStyle;
		if (gradientStyle == OfficeGradientStyle.FromCenter || gradientStyle == OfficeGradientStyle.FromCorner)
		{
			SerializeFromCenterCornerGradientFill(writer, fill);
		}
		else
		{
			SerializeDegreeGradientFill(writer, fill);
		}
		writer.WriteEndElement();
	}

	private void SerializeDegreeGradientFill(XmlWriter writer, FillImpl fill)
	{
		OfficeGradientStyle gradientStyle = fill.GradientStyle;
		OfficeGradientVariants gradientVariant = fill.GradientVariant;
		double value = 0.0;
		if (gradientVariant == OfficeGradientVariants.ShadingVariants_3)
		{
			switch (gradientStyle)
			{
			case OfficeGradientStyle.Horizontal:
				value = 90.0;
				break;
			case OfficeGradientStyle.DiagonalUp:
				value = 45.0;
				break;
			case OfficeGradientStyle.DiagonalDown:
				value = 135.0;
				break;
			default:
				throw new ArgumentException("Unknown gradient style");
			case OfficeGradientStyle.Vertical:
				break;
			}
			SerializeAttribute(writer, "degree", value, 0.0);
			SerializeStopColorElements(writer, 0.0, fill.ColorObject);
			SerializeStopColorElements(writer, 0.5, fill.PatternColorObject);
			SerializeStopColorElements(writer, 1.0, fill.ColorObject);
		}
		else
		{
			SerializeAttribute(writer, "degree", gradientStyle switch
			{
				OfficeGradientStyle.Horizontal => (gradientVariant == OfficeGradientVariants.ShadingVariants_1) ? 90 : 270, 
				OfficeGradientStyle.Vertical => (gradientVariant != OfficeGradientVariants.ShadingVariants_1) ? 180 : 0, 
				OfficeGradientStyle.DiagonalUp => (gradientVariant == OfficeGradientVariants.ShadingVariants_1) ? 45 : 225, 
				OfficeGradientStyle.DiagonalDown => (gradientVariant == OfficeGradientVariants.ShadingVariants_1) ? 135 : 315, 
				_ => throw new ArgumentException("Unknown gradient style"), 
			}, 0.0);
			SerializeStopColorElements(writer, 0.0, fill.ColorObject);
			SerializeStopColorElements(writer, 1.0, fill.PatternColorObject);
		}
	}

	private void SerializeFromCenterCornerGradientFill(XmlWriter writer, FillImpl fill)
	{
		OfficeGradientStyle gradientStyle = fill.GradientStyle;
		OfficeGradientVariants gradientVariant = fill.GradientVariant;
		SerializeAttribute(writer, "type", "path", string.Empty);
		double value = double.MinValue;
		double value2 = double.MinValue;
		double value3 = double.MinValue;
		double value4 = double.MinValue;
		if (gradientStyle == OfficeGradientStyle.FromCenter)
		{
			value = (value2 = (value3 = (value4 = 0.5)));
		}
		else
		{
			switch (gradientVariant)
			{
			case OfficeGradientVariants.ShadingVariants_2:
				value3 = (value4 = 1.0);
				break;
			case OfficeGradientVariants.ShadingVariants_3:
				value = (value2 = 1.0);
				break;
			case OfficeGradientVariants.ShadingVariants_4:
				value = (value2 = (value3 = (value4 = 1.0)));
				break;
			default:
				throw new ArgumentException("Unknown gradient variant");
			case OfficeGradientVariants.ShadingVariants_1:
				break;
			}
		}
		SerializeAttribute(writer, "top", value, double.MinValue);
		SerializeAttribute(writer, "bottom", value2, double.MinValue);
		SerializeAttribute(writer, "left", value3, double.MinValue);
		SerializeAttribute(writer, "right", value4, double.MinValue);
		SerializeStopColorElements(writer, 0.0, fill.ColorObject);
		SerializeStopColorElements(writer, 1.0, fill.PatternColorObject);
	}

	private void SerializeStopColorElements(XmlWriter writer, double dPosition, ChartColor color)
	{
		writer.WriteStartElement("stop");
		SerializeAttribute(writer, "position", dPosition, double.MinValue);
		SerializeColorObject(writer, "color", color);
		writer.WriteEndElement();
	}

	private string ConvertPatternToString(OfficePattern pattern)
	{
		Excel2007Pattern excel2007Pattern = (Excel2007Pattern)pattern;
		return excel2007Pattern.ToString();
	}

	private int[] SerializeBorders(XmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		Dictionary<BordersCollection, int> dictionary = new Dictionary<BordersCollection, int>();
		ExtendedFormatsCollection innerExtFormats = m_book.InnerExtFormats;
		int count = innerExtFormats.Count;
		int[] array = new int[count];
		BordersCollection[] array2 = new BordersCollection[count];
		int num = -1;
		for (int i = 0; i < count; i++)
		{
			BordersCollection bordersCollection = null;
			if (num == -1)
			{
				WorkbookImpl workbookImpl = new WorkbookImpl(m_book.Application, m_book, OfficeVersion.Excel2007);
				BordersCollection bordersCollection2 = new BordersCollection(workbookImpl.Application, workbookImpl, bAddEmpty: true);
				ExtendedFormatWrapper impl = new ExtendedFormatWrapper(workbookImpl, 0);
				bordersCollection2.InnerList.Clear();
				bordersCollection2.InnerList.Add(new BorderImpl(workbookImpl.Application, workbookImpl, impl, OfficeBordersIndex.DiagonalDown));
				bordersCollection2.InnerList.Add(new BorderImpl(workbookImpl.Application, workbookImpl, impl, OfficeBordersIndex.DiagonalUp));
				bordersCollection2.InnerList.Add(new BorderImpl(workbookImpl.Application, workbookImpl, impl, OfficeBordersIndex.EdgeBottom));
				bordersCollection2.InnerList.Add(new BorderImpl(workbookImpl.Application, workbookImpl, impl, OfficeBordersIndex.EdgeLeft));
				bordersCollection2.InnerList.Add(new BorderImpl(workbookImpl.Application, workbookImpl, impl, OfficeBordersIndex.EdgeRight));
				bordersCollection2.InnerList.Add(new BorderImpl(workbookImpl.Application, workbookImpl, impl, OfficeBordersIndex.EdgeTop));
				bordersCollection = bordersCollection2;
			}
			else
			{
				bordersCollection = (BordersCollection)innerExtFormats[i].Borders;
			}
			if (dictionary.ContainsKey(bordersCollection))
			{
				array[i] = dictionary[bordersCollection];
				continue;
			}
			num = dictionary.Count;
			dictionary.Add(bordersCollection, num);
			array2[num] = bordersCollection;
			array[i] = num;
			if (num == 0)
			{
				i--;
			}
		}
		writer.WriteStartElement("borders");
		writer.WriteAttributeString("count", (num + 1).ToString());
		for (int j = 0; j <= num; j++)
		{
			SerializeBordersCollection(writer, array2[j]);
		}
		writer.WriteEndElement();
		return array;
	}

	private void SerializeIndexedColor(XmlWriter writer, string tagName, OfficeKnownColors color)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (tagName == null || tagName.Length == 0)
		{
			throw new ArgumentOutOfRangeException("tagName");
		}
		writer.WriteStartElement(tagName);
		if (color > (OfficeKnownColors)65)
		{
			writer.WriteAttributeString("auto", "1");
		}
		else
		{
			int num = (int)color;
			writer.WriteAttributeString("indexed", num.ToString());
		}
		writer.WriteEndElement();
	}

	public void SerializeRgbColor(XmlWriter writer, string tagName, ChartColor color)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (tagName == null || tagName.Length == 0)
		{
			throw new ArgumentOutOfRangeException("tagName");
		}
		int value = color.Value;
		writer.WriteStartElement(tagName);
		writer.WriteAttributeString("rgb", value.ToString("X8"));
		SerializeAttribute(writer, "tint", color.Tint, 0.0);
		writer.WriteEndElement();
	}

	private void SerializeThemeColor(XmlWriter writer, string tagName, ChartColor color)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (tagName == null || tagName.Length == 0)
		{
			throw new ArgumentOutOfRangeException("tagName");
		}
		writer.WriteStartElement(tagName);
		writer.WriteAttributeString("theme", color.Value.ToString());
		SerializeAttribute(writer, "tint", color.Tint, 0.0);
		writer.WriteEndElement();
	}

	private void SerializeColorObject(XmlWriter writer, string tagName, ChartColor color)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (tagName == null || tagName.Length == 0)
		{
			throw new ArgumentOutOfRangeException("tagName");
		}
		switch (color.ColorType)
		{
		case ColorType.Indexed:
			SerializeIndexedColor(writer, tagName, (OfficeKnownColors)color.Value);
			break;
		case ColorType.RGB:
			SerializeRgbColor(writer, tagName, color);
			break;
		case ColorType.Theme:
			SerializeThemeColor(writer, tagName, color);
			break;
		default:
			throw new NotImplementedException();
		}
	}

	private void SerializeBordersCollection(XmlWriter writer, BordersCollection borders)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (borders == null)
		{
			throw new ArgumentNullException("borders");
		}
		writer.WriteStartElement("border");
		SerializeAttribute(writer, "diagonalUp", borders[OfficeBordersIndex.DiagonalUp].ShowDiagonalLine, defaultValue: false);
		SerializeAttribute(writer, "diagonalDown", borders[OfficeBordersIndex.DiagonalDown].ShowDiagonalLine, defaultValue: false);
		SerializeBorder(writer, (BorderImpl)borders[OfficeBordersIndex.EdgeLeft]);
		SerializeBorder(writer, (BorderImpl)borders[OfficeBordersIndex.EdgeRight]);
		SerializeBorder(writer, (BorderImpl)borders[OfficeBordersIndex.EdgeTop]);
		SerializeBorder(writer, (BorderImpl)borders[OfficeBordersIndex.EdgeBottom]);
		SerializeBorder(writer, (BorderImpl)borders[OfficeBordersIndex.DiagonalUp]);
		writer.WriteEndElement();
	}

	private void SerializeBorder(XmlWriter writer, BorderImpl border)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (border == null)
		{
			throw new ArgumentNullException("border");
		}
		string borderTag = GetBorderTag(border.BorderIndex);
		if (borderTag != null)
		{
			writer.WriteStartElement(borderTag);
			if (border.LineStyle != 0)
			{
				writer.WriteAttributeString("style", GetBorderLineStyle(border));
				SerializeColorObject(writer, "color", border.ColorObject);
			}
			writer.WriteEndElement();
		}
	}

	private static string GetBorderTag(OfficeBordersIndex borderIndex)
	{
		string result = null;
		Excel2007BorderIndex excel2007BorderIndex = (Excel2007BorderIndex)borderIndex;
		if (excel2007BorderIndex != Excel2007BorderIndex.none)
		{
			result = excel2007BorderIndex.ToString();
		}
		return result;
	}

	private string GetBorderLineStyle(BorderImpl border)
	{
		return LowerFirstLetter(((Excel2007BorderLineStyle)border.LineStyle).ToString());
	}

	private Dictionary<int, int> SerializeNamedStyleXFs(XmlWriter writer, int[] arrFillIndexes, int[] arrBorderIndexes)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (arrFillIndexes == null)
		{
			throw new ArgumentNullException("arrFillIndexes");
		}
		if (arrBorderIndexes == null)
		{
			throw new ArgumentNullException("arrBorderIndexes");
		}
		writer.WriteStartElement("cellStyleXfs");
		ExtendedFormatsCollection innerExtFormats = m_book.InnerExtFormats;
		writer.WriteAttributeString("count", innerExtFormats.Count.ToString());
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		int num = 0;
		int i = 0;
		for (int count = innerExtFormats.Count; i < count; i++)
		{
			ExtendedFormatImpl extendedFormatImpl = innerExtFormats[i];
			if (!extendedFormatImpl.HasParent)
			{
				SerializeExtendedFormat(writer, arrFillIndexes, arrBorderIndexes, extendedFormatImpl, null, defaultApplyValue: true);
				dictionary.Add(extendedFormatImpl.Index, num);
				num++;
			}
		}
		writer.WriteEndElement();
		return dictionary;
	}

	private Dictionary<int, int> SerializeNotNamedXFs(XmlWriter writer, int[] arrFillIndexes, int[] arrBorderIndexes, Dictionary<int, int> hashNewParentIndexes)
	{
		ExtendedFormatsCollection innerExtFormats = m_book.InnerExtFormats;
		int count = innerExtFormats.Count;
		_ = m_book.InnerStyles.Count;
		writer.WriteStartElement("cellXfs");
		int num = 0;
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		for (int i = 0; i < count; i++)
		{
			ExtendedFormatImpl extendedFormatImpl = innerExtFormats[i];
			if (extendedFormatImpl.HasParent)
			{
				dictionary.Add(extendedFormatImpl.Index, num);
				SerializeExtendedFormat(writer, arrFillIndexes, arrBorderIndexes, extendedFormatImpl, hashNewParentIndexes, defaultApplyValue: false);
				num++;
			}
		}
		writer.WriteEndElement();
		return dictionary;
	}

	private void SerializeExtendedFormat(XmlWriter writer, int[] arrFillIndexes, int[] arrBorderIndexes, ExtendedFormatImpl format, Dictionary<int, int> newParentIndexes, bool defaultApplyValue)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (arrFillIndexes == null)
		{
			throw new ArgumentNullException("arrFillIndexes");
		}
		if (arrBorderIndexes == null)
		{
			throw new ArgumentNullException("arrBorderIndexes");
		}
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		int index = format.Index;
		writer.WriteStartElement("xf");
		writer.WriteAttributeString("numFmtId", format.NumberFormatIndex.ToString());
		writer.WriteAttributeString("fontId", format.FontIndex.ToString());
		writer.WriteAttributeString("fillId", arrFillIndexes[index].ToString());
		writer.WriteAttributeString("borderId", arrBorderIndexes[index].ToString());
		SerializeAttribute(writer, "pivotButton", format.PivotButton, defaultValue: false);
		if (format.HasParent)
		{
			if (!newParentIndexes.TryGetValue(format.ParentIndex, out var value))
			{
				value = format.ParentIndex;
			}
			if (newParentIndexes.Count - 1 < value)
			{
				writer.WriteAttributeString("xfId", newParentIndexes[0].ToString());
			}
			else
			{
				writer.WriteAttributeString("xfId", value.ToString());
			}
		}
		SerializeAttribute(writer, "applyAlignment", format.IncludeAlignment, defaultApplyValue);
		SerializeAttribute(writer, "applyBorder", format.IncludeBorder, defaultApplyValue);
		SerializeAttribute(writer, "applyFont", format.IncludeFont, defaultApplyValue);
		SerializeAttribute(writer, "applyNumberFormat", format.IncludeNumberFormat, defaultApplyValue);
		SerializeAttribute(writer, "applyFill", format.IncludePatterns, defaultApplyValue);
		SerializeAttribute(writer, "applyProtection", format.IncludeProtection, defaultApplyValue);
		SerializeAttribute(writer, "quotePrefix", format.IsFirstSymbolApostrophe, defaultValue: false);
		SerializeAlignment(writer, format);
		SerializeProtection(writer, format);
		writer.WriteEndElement();
	}

	private void SerializeAlignment(XmlWriter writer, ExtendedFormatImpl format)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (!IsDefaultAlignment(format))
		{
			writer.WriteStartElement("alignment");
			if (format.HorizontalAlignment != 0)
			{
				string value = ((Excel2007HAlign)format.HorizontalAlignment).ToString();
				writer.WriteAttributeString("horizontal", value);
			}
			if (format.VerticalAlignment != OfficeVAlign.VAlignBottom)
			{
				string value2 = ((Excel2007VAlign)format.VerticalAlignment).ToString();
				writer.WriteAttributeString("vertical", value2);
			}
			SerializeAttribute(writer, "textRotation", format.Rotation, 0);
			SerializeAttribute(writer, "wrapText", format.WrapText, defaultValue: false);
			SerializeAttribute(writer, "indent", format.IndentLevel, 0);
			SerializeAttribute(writer, "justifyLastLine", format.JustifyLast, defaultValue: false);
			SerializeAttribute(writer, "shrinkToFit", format.ShrinkToFit, defaultValue: false);
			SerializeAttribute(writer, "readingOrder", (int)format.ReadingOrder, 0);
			writer.WriteEndElement();
		}
	}

	private bool IsDefaultAlignment(ExtendedFormatImpl format)
	{
		if (format.HorizontalAlignment == OfficeHAlign.HAlignGeneral && format.IndentLevel == 0 && !format.JustifyLast && format.ReadingOrder == OfficeReadingOrderType.Context && !format.ShrinkToFit && format.Rotation == 0 && !format.WrapText)
		{
			return format.VerticalAlignment == OfficeVAlign.VAlignBottom;
		}
		return false;
	}

	private void SerializeProtection(XmlWriter writer, ExtendedFormatImpl format)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (format.FormulaHidden || !format.Locked)
		{
			writer.WriteStartElement("protection");
			SerializeAttribute(writer, "hidden", format.FormulaHidden, defaultValue: false);
			SerializeAttribute(writer, "locked", format.Locked, defaultValue: true);
			writer.WriteEndElement();
		}
	}

	private void SerializeStyles(XmlWriter writer, Dictionary<int, int> hashNewParentIndexes)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (hashNewParentIndexes == null)
		{
			throw new ArgumentNullException("hashNewParentIndexes");
		}
		StylesCollection innerStyles = m_book.InnerStyles;
		int count = innerStyles.Count;
		writer.WriteStartElement("cellStyles");
		writer.WriteAttributeString("count", count.ToString());
		for (int i = 0; i < count; i++)
		{
			StyleImpl styleImpl = (StyleImpl)innerStyles[i];
			StyleExtRecord styleExt = styleImpl.StyleExt;
			if (styleExt == null || ((styleImpl.BuiltIn || !styleExt.IsBuildInStyle) && !styleExt.IsHidden))
			{
				SerializeStyle(writer, styleImpl, hashNewParentIndexes);
			}
		}
		writer.WriteEndElement();
	}

	private void SerializeStyle(XmlWriter writer, StyleImpl style, Dictionary<int, int> hashNewParentIndexes)
	{
		if (writer == null)
		{
			throw new ArgumentNullException();
		}
		if (style == null)
		{
			throw new ArgumentNullException("style");
		}
		if (hashNewParentIndexes == null)
		{
			throw new ArgumentNullException("hashNewParentIndexes");
		}
		writer.WriteStartElement("cellStyle");
		if (style.IsAsciiConverted)
		{
			writer.WriteAttributeString("name", style.StyleNameCache);
		}
		else
		{
			writer.WriteAttributeString("name", style.Name);
		}
		writer.WriteAttributeString("xfId", hashNewParentIndexes[style.XFormatIndex].ToString());
		if (style.BuiltIn)
		{
			StyleRecord record = style.Record;
			writer.WriteAttributeString("builtinId", record.BuildInOrNameLen.ToString());
			if (record.OutlineStyleLevel != byte.MaxValue)
			{
				writer.WriteAttributeString("iLevel", record.OutlineStyleLevel.ToString());
			}
		}
		writer.WriteEndElement();
	}

	private void SerializeDictionary(XmlWriter writer, IDictionary<string, string> toSerialize, string tagName, string keyAttributeName, string valueAttributeName, IFileNamePreprocessor keyPreprocessor)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (toSerialize == null)
		{
			throw new ArgumentNullException("toSerialize");
		}
		if (tagName == null || tagName.Length == 0)
		{
			throw new ArgumentOutOfRangeException("tagName");
		}
		if (keyAttributeName == null || keyAttributeName.Length == 0)
		{
			throw new ArgumentOutOfRangeException("keyAttributeName");
		}
		if (valueAttributeName == null || valueAttributeName.Length == 0)
		{
			throw new ArgumentOutOfRangeException("valueAttributeName");
		}
		foreach (KeyValuePair<string, string> item in toSerialize)
		{
			string key = item.Key;
			string value = item.Value;
			writer.WriteStartElement(tagName);
			writer.WriteAttributeString(keyAttributeName, key);
			writer.WriteAttributeString(valueAttributeName, value);
			writer.WriteEndElement();
		}
	}

	public static string LowerFirstLetter(string value)
	{
		return char.ToLower(value[0]) + value.Remove(0, 1);
	}

	internal static void SerializeAttribute(XmlWriter writer, string attributeName, bool value, bool defaultValue)
	{
		if (value != defaultValue)
		{
			string value2 = (value ? "1" : "0");
			writer.WriteAttributeString(attributeName, value2);
		}
	}

	internal static void SerializeBool(XmlWriter writer, string attributeName, bool value)
	{
		string value2 = (value ? "1" : "0");
		writer.WriteAttributeString(attributeName, value2);
	}

	internal static void SerializeAttribute(XmlWriter writer, string attributeName, int value, int defaultValue)
	{
		if (value != defaultValue)
		{
			string value2 = value.ToString();
			writer.WriteAttributeString(attributeName, value2);
		}
	}

	internal static void SerializeAttribute(XmlWriter writer, string attributeName, double value, double defaultValue)
	{
		SerializeAttribute(writer, attributeName, value, defaultValue, null);
	}

	internal static void SerializeAttribute(XmlWriter writer, string attributeName, double value, double defaultValue, string attributeNamespace)
	{
		if (value != defaultValue)
		{
			string value2 = XmlConvert.ToString(value);
			writer.WriteAttributeString(attributeName, attributeNamespace, value2);
		}
	}

	internal static void SerializeAttribute(XmlWriter writer, string attributeName, string value, string defaultValue)
	{
		if (value != defaultValue)
		{
			writer.WriteAttributeString(attributeName, value);
		}
	}

	internal static void SerializeAttribute(XmlWriter writer, string attributeName, Enum value, Enum defaultValue)
	{
		if (value != defaultValue)
		{
			writer.WriteAttributeString(attributeName, LowerFirstLetter(value.ToString()));
		}
	}

	protected static void SerializeElementString(XmlWriter writer, string elementName, string value, string defaultValue)
	{
		if (value != defaultValue)
		{
			writer.WriteElementString(elementName, value);
		}
	}

	private static void SerializeElementString(XmlWriter writer, string elementName, string value, string defaultValue, string prefix)
	{
		if (value != defaultValue)
		{
			writer.WriteElementString(prefix, elementName, null, value);
		}
	}

	private static void SerializeElementString(XmlWriter writer, string elementName, int value, int defaultValue)
	{
		if (value != defaultValue)
		{
			string value2 = value.ToString();
			writer.WriteElementString(elementName, value2);
		}
	}

	public void SeiralizeSheet(XmlWriter writer, WorksheetBaseImpl sheet)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		throw new NotImplementedException();
	}

	public void SerializeSheetData(XmlWriter writer, CellRecordCollection cells, Dictionary<int, int> hashNewParentIndexes, string cellTag, Dictionary<string, string> additionalAttributes, bool isSpansNeeded)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (cells == null)
		{
			throw new ArgumentNullException("cells");
		}
		writer.WriteStartElement("sheetData");
		int i = cells.FirstRow;
		for (int lastRow = cells.LastRow; i <= lastRow; i++)
		{
			if (cells.ContainsRow(i - 1))
			{
				RowStorage row = cells.Table.Rows[i - 1];
				SerializeRow(writer, row, cells, i - 1, hashNewParentIndexes, cellTag, isSpansNeeded);
			}
		}
		writer.WriteEndElement();
	}

	private void SerializeAttributes(XmlWriter writer, Dictionary<string, string> additionalAttributes)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (additionalAttributes == null || additionalAttributes.Count == 0)
		{
			return;
		}
		foreach (KeyValuePair<string, string> additionalAttribute in additionalAttributes)
		{
			writer.WriteAttributeString(additionalAttribute.Key, additionalAttribute.Value);
		}
	}

	private void SerializeRow(XmlWriter writer, RowStorage row, CellRecordCollection cells, int iRowIndex, Dictionary<int, int> hashNewParentIndexes, string cellTag, bool isSpansNeeded)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (row == null)
		{
			throw new ArgumentNullException("row");
		}
		writer.WriteStartElement("row");
		SerializeAttribute(writer, "r", (iRowIndex + 1).ToString(), string.Empty);
		if (isSpansNeeded && row.FirstColumn >= 0)
		{
			string value = row.FirstColumn + 1 + ":" + (row.LastColumn + 1);
			SerializeAttribute(writer, "spans", value, string.Empty);
		}
		if (hashNewParentIndexes != null && hashNewParentIndexes.Count > 0)
		{
			if (hashNewParentIndexes.ContainsKey(row.ExtendedFormatIndex) && row.ExtendedFormatIndex != m_book.DefaultXFIndex)
			{
				SerializeAttribute(writer, "s", hashNewParentIndexes[row.ExtendedFormatIndex], 0);
			}
			SerializeAttribute(writer, "customFormat", row.IsFormatted, defaultValue: false);
			SerializeAttribute(writer, "ht", (double)(int)row.Height / 20.0, m_worksheetImpl.StandardHeight);
		}
		SerializeAttribute(writer, "collapsed", row.IsCollapsed, defaultValue: false);
		SerializeAttribute(writer, "customHeight", row.IsBadFontHeight, defaultValue: false);
		SerializeAttribute(writer, "hidden", row.IsHidden, defaultValue: false);
		SerializeAttribute(writer, "outlineLevel", row.OutlineLevel, 0);
		SerializeAttribute(writer, "thickTop", row.IsSpaceAboveRow, defaultValue: false);
		SerializeAttribute(writer, "thickBot", row.IsSpaceBelowRow, defaultValue: false);
		SerializeCells(writer, row, cells, hashNewParentIndexes, cellTag);
		writer.WriteEndElement();
	}

	private void SerializeCells(XmlWriter writer, RowStorage row, CellRecordCollection cells, Dictionary<int, int> hashNewParentIndexes, string cellTag)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (row == null)
		{
			throw new ArgumentNullException("row");
		}
		if (cells == null)
		{
			throw new ArgumentNullException("cells");
		}
		RowStorageEnumerator rowStorageEnumerator = row.GetEnumerator(m_recordExtractor) as RowStorageEnumerator;
		while (rowStorageEnumerator.MoveNext())
		{
			BiffRecordRaw biffRecordRaw = rowStorageEnumerator.Current as BiffRecordRaw;
			switch (biffRecordRaw.TypeCode)
			{
			case TBIFFRecord.MulRK:
				SerializeMulRKRecordValues(writer, (MulRKRecord)biffRecordRaw, hashNewParentIndexes);
				break;
			case TBIFFRecord.MulBlank:
				SerializeMulBlankRecord(writer, (MulBlankRecord)biffRecordRaw, hashNewParentIndexes);
				break;
			case TBIFFRecord.Blank:
			{
				BlankRecord blankRecord = (BlankRecord)biffRecordRaw;
				SerializeBlankCell(writer, blankRecord.Row + 1, blankRecord.Column + 1, blankRecord.ExtendedFormatIndex, hashNewParentIndexes);
				break;
			}
			default:
				SerializeCell(writer, biffRecordRaw, rowStorageEnumerator, cells, hashNewParentIndexes, cellTag);
				break;
			}
		}
	}

	private void SerializeCell(XmlWriter writer, BiffRecordRaw record, RowStorageEnumerator rowStorageEnumerator, CellRecordCollection cells, Dictionary<int, int> hashNewParentIndexes, string cellTag)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		if (cells == null)
		{
			throw new ArgumentNullException("cells");
		}
		if (rowStorageEnumerator == null)
		{
			throw new ArgumentNullException("rowStorageEnumerator");
		}
		writer.WriteStartElement(cellTag);
		ICellPositionFormat cellPositionFormat = record as ICellPositionFormat;
		string cellName = RangeImpl.GetCellName(cellPositionFormat.Column + 1, cellPositionFormat.Row + 1);
		SerializeAttribute(writer, "r", cellName, null);
		if (hashNewParentIndexes != null && hashNewParentIndexes.Count > 0)
		{
			int extendedFormatIndex = cellPositionFormat.ExtendedFormatIndex;
			extendedFormatIndex = ((!hashNewParentIndexes.ContainsKey(extendedFormatIndex)) ? (extendedFormatIndex - 1) : hashNewParentIndexes[extendedFormatIndex]);
			SerializeAttribute(writer, "s", extendedFormatIndex, 0);
		}
		string strCellType;
		CellType cellType = GetCellDataType(record, out strCellType);
		string value = null;
		if (Worksheet.InlineStrings.TryGetValue(cellName, out value))
		{
			cellType = CellType.inlineStr;
			strCellType = "inlineStr";
		}
		SerializeAttribute(writer, "t", strCellType, "n");
		if (record.TypeCode == TBIFFRecord.Formula)
		{
			FormulaRecord formulaRecord = (FormulaRecord)record;
			ArrayRecord arrayRecord;
			if ((arrayRecord = rowStorageEnumerator.GetArrayRecord()) != null)
			{
				SerializeArrayFormula(writer, arrayRecord);
			}
			else
			{
				SerializeSimpleFormula(writer, formulaRecord, cells);
			}
			SerializeFormulaValue(writer, formulaRecord, cellType, rowStorageEnumerator);
		}
		SerializeCellValue(writer, record, cellType, value);
		writer.WriteEndElement();
	}

	private void SerializeBlankCell(XmlWriter writer, int iRowIndex, int iColumnIndex, int iXFIndex, Dictionary<int, int> hashNewParentIndexes)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (hashNewParentIndexes == null)
		{
			throw new ArgumentNullException("hashNewParentIndexes");
		}
		writer.WriteStartElement("c");
		SerializeAttribute(writer, "r", RangeImpl.GetCellName(iColumnIndex, iRowIndex), string.Empty);
		if (!hashNewParentIndexes.TryGetValue(iXFIndex, out var value))
		{
			value = ((!hashNewParentIndexes.ContainsKey(iXFIndex - 1)) ? (iXFIndex - 1) : m_book.DefaultXFIndex);
		}
		SerializeAttribute(writer, "s", value, 0);
		writer.WriteEndElement();
	}

	private void SerializeMulBlankRecord(XmlWriter writer, MulBlankRecord mulBlankRecord, Dictionary<int, int> hashNewParentIndexes)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (mulBlankRecord == null)
		{
			throw new ArgumentNullException("mulBlankRecord");
		}
		if (hashNewParentIndexes == null)
		{
			throw new ArgumentNullException("hashNewParentIndexes");
		}
		int iRowIndex = mulBlankRecord.Row + 1;
		int num = mulBlankRecord.FirstColumn + 1;
		List<ushort> extendedFormatIndexes = mulBlankRecord.ExtendedFormatIndexes;
		int i = 0;
		for (int count = extendedFormatIndexes.Count; i < count; i++)
		{
			SerializeBlankCell(writer, iRowIndex, num + i, Convert.ToInt32(extendedFormatIndexes[i]), hashNewParentIndexes);
		}
	}

	private void SerializeMulRKRecordValues(XmlWriter writer, MulRKRecord mulRKRecord, Dictionary<int, int> hashNewParentIndexes)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (mulRKRecord == null)
		{
			throw new ArgumentNullException("mulRkRecord");
		}
		if (hashNewParentIndexes == null)
		{
			throw new ArgumentNullException("hashNewParentIndexes");
		}
		int firstRow = mulRKRecord.Row + 1;
		int num = mulRKRecord.FirstColumn + 1;
		List<MulRKRecord.RkRec> records = mulRKRecord.Records;
		int i = 0;
		for (int count = records.Count; i < count; i++)
		{
			writer.WriteStartElement("c");
			MulRKRecord.RkRec rkRec = records[i];
			SerializeAttribute(writer, "r", RangeImpl.GetCellName(num + i, firstRow), string.Empty);
			int extFormatIndex = rkRec.ExtFormatIndex;
			int value = 0;
			if (hashNewParentIndexes.ContainsKey(extFormatIndex))
			{
				value = hashNewParentIndexes[extFormatIndex];
			}
			SerializeAttribute(writer, "s", value, 0);
			writer.WriteElementString("v", XmlConvert.ToString(rkRec.RkNumber));
			writer.WriteEndElement();
		}
	}

	private void SerializeArrayFormula(XmlWriter writer, ArrayRecord arrayRecord)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (arrayRecord == null)
		{
			throw new ArgumentNullException("arrayRecord");
		}
		writer.WriteStartElement("f");
		writer.WriteAttributeString("t", FormulaType.array.ToString());
		writer.WriteAttributeString("aca", "true");
		string text = m_formulaUtil.ParsePtgArray(arrayRecord.Formula);
		if (text.Length > 8000)
		{
			throw new ApplicationException("Formula length is too big. Maximum formula length is " + 8000 + ".");
		}
		string addressLocal = RangeImpl.GetAddressLocal(arrayRecord.FirstRow + 1, arrayRecord.FirstColumn + 1, arrayRecord.LastRow + 1, arrayRecord.LastColumn + 1);
		writer.WriteAttributeString("ref", addressLocal);
		writer.WriteString(text);
		writer.WriteEndElement();
	}

	private void SerializeSimpleFormula(XmlWriter writer, FormulaRecord formulaRecord, CellRecordCollection cells)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (formulaRecord == null)
		{
			throw new ArgumentNullException("formulaRecord");
		}
		if (cells == null)
		{
			throw new ArgumentNullException("cells");
		}
		string empty = string.Empty;
		if (formulaRecord.Formula != null && formulaRecord.Formula.Length == 0)
		{
			empty = (cells.Sheet as WorksheetImpl).m_formulaString;
		}
		else
		{
			Ptg ptg = formulaRecord.Formula[0];
			if (ptg.TokenCode == FormulaToken.tExp)
			{
				ControlPtg controlPtg = ptg as ControlPtg;
				if (cells.Table.Rows[controlPtg.RowIndex].HasFormulaArrayRecord(controlPtg.ColumnIndex))
				{
					return;
				}
			}
			m_formulaUtil.CheckFormulaVersion(formulaRecord.Formula);
			empty = m_formulaUtil.ParsePtgArray(formulaRecord.Formula, 0, 0, bR1C1: false, null, bRemoveSheetNames: false, isForSerialization: true, cells.Sheet);
		}
		if (empty[0] == '=')
		{
			empty = UtilityMethods.RemoveFirstCharUnsafe(empty);
		}
		if (empty.Length > 8000)
		{
			throw new ApplicationException("Formula length is too big. Maximum formula length is " + 8000 + ".");
		}
		writer.WriteStartElement("f");
		bool value = !m_formulaUtil.HasExternalReference(formulaRecord.Formula) || formulaRecord.CalculateOnOpen;
		SerializeAttribute(writer, "ca", value, defaultValue: false);
		writer.WriteString(empty);
		writer.WriteEndElement();
	}

	private void SerializeCellValue(XmlWriter writer, BiffRecordRaw record, CellType cellType, string inlineValue)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		switch (record.TypeCode)
		{
		case TBIFFRecord.BoolErr:
		{
			BoolErrRecord boolErrRecord = (BoolErrRecord)record;
			string value = ((!boolErrRecord.IsErrorCode) ? boolErrRecord.BoolOrError.ToString() : FormulaUtil.ErrorCodeToName[boolErrRecord.BoolOrError]);
			writer.WriteElementString("v", value);
			break;
		}
		case TBIFFRecord.Number:
		case TBIFFRecord.RK:
			writer.WriteElementString("v", XmlConvert.ToString(((IDoubleValue)record).DoubleValue));
			break;
		case TBIFFRecord.LabelSST:
			if (cellType == CellType.inlineStr)
			{
				writer.WriteStartElement("is");
				writer.WriteElementString("t", inlineValue);
				writer.WriteEndElement();
			}
			else
			{
				writer.WriteElementString("v", (record as LabelSSTRecord).SSTIndex.ToString());
			}
			break;
		case TBIFFRecord.Label:
			writer.WriteElementString("v", ((LabelRecord)record).Label);
			break;
		}
	}

	private void SerializeFormulaValue(XmlWriter writer, FormulaRecord record, CellType cellType, RowStorageEnumerator rowStorageEnumerator)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		if (rowStorageEnumerator == null)
		{
			throw new ArgumentNullException("rowStorageEnumerator");
		}
		switch (cellType)
		{
		case CellType.b:
		{
			string value = (record.BooleanValue ? "1" : "0");
			writer.WriteElementString("v", value);
			break;
		}
		case CellType.e:
		{
			string value2 = FormulaUtil.ErrorCodeToName[record.ErrorValue];
			writer.WriteElementString("v", value2);
			break;
		}
		case CellType.str:
			writer.WriteElementString("v", rowStorageEnumerator.GetFormulaStringValue());
			break;
		case CellType.n:
			if (!double.IsNaN(record.DoubleValue))
			{
				writer.WriteElementString("v", XmlConvert.ToString(record.DoubleValue));
			}
			break;
		case CellType.inlineStr:
		case CellType.s:
			break;
		}
	}

	private CellType GetCellDataType(BiffRecordRaw record, out string strCellType)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		CellType result;
		switch (record.TypeCode)
		{
		case TBIFFRecord.BoolErr:
			if (((BoolErrRecord)record).IsErrorCode)
			{
				result = CellType.e;
				strCellType = "e";
			}
			else
			{
				result = CellType.b;
				strCellType = "b";
			}
			break;
		case TBIFFRecord.MulRK:
		case TBIFFRecord.Number:
		case TBIFFRecord.RK:
			result = CellType.n;
			strCellType = "n";
			break;
		case TBIFFRecord.RString:
		case TBIFFRecord.LabelSST:
			result = CellType.s;
			strCellType = "s";
			break;
		case TBIFFRecord.Label:
			result = CellType.str;
			strCellType = "str";
			break;
		case TBIFFRecord.Formula:
		{
			FormulaRecord formulaRecord = (FormulaRecord)record;
			if (formulaRecord.IsBool)
			{
				result = CellType.b;
				strCellType = "b";
			}
			else if (formulaRecord.IsError)
			{
				result = CellType.e;
				strCellType = "e";
			}
			else if (formulaRecord.HasString)
			{
				result = CellType.str;
				strCellType = "str";
			}
			else
			{
				result = CellType.n;
				strCellType = "n";
			}
			break;
		}
		default:
			throw new NotImplementedException("type");
		}
		return result;
	}

	public void SerializeSST(XmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (m_book.HasInlineStrings && m_book.SSTStream != null)
		{
			m_book.SSTStream.Position = 0L;
			ShapeParser.WriteNodeFromStream(writer, m_book.SSTStream);
			return;
		}
		SSTDictionary innerSST = m_book.InnerSST;
		writer.WriteStartDocument();
		writer.WriteStartElement("sst", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
		int count = innerSST.Count;
		int labelSSTCount = innerSST.GetLabelSSTCount();
		writer.WriteAttributeString("uniqueCount", count.ToString());
		writer.WriteAttributeString("count", labelSSTCount.ToString());
		for (int i = 0; i < count; i++)
		{
			object sSTContentByIndex = innerSST.GetSSTContentByIndex(i);
			SerializeStringItem(writer, sSTContentByIndex);
		}
		writer.WriteEndElement();
	}

	private void SerializeStringItem(XmlWriter writer, object objTextOrString)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (objTextOrString == null)
		{
			throw new ArgumentNullException("text");
		}
		writer.WriteStartElement("si");
		TextWithFormat textWithFormat = objTextOrString as TextWithFormat;
		if (textWithFormat != null && textWithFormat.FormattingRunsCount > 0)
		{
			SerializeRichTextRun(writer, textWithFormat);
		}
		else
		{
			string text = ((textWithFormat == null) ? objTextOrString.ToString() : textWithFormat.Text);
			if (!text.Contains("\r\n"))
			{
				text = text.Replace("\n", "\r\n");
			}
			int length = text.Length;
			writer.WriteStartElement("t");
			if ((length > 0 && (text[0] == ' ' || text[length - 1] == ' ')) || (textWithFormat != null && textWithFormat.IsPreserved))
			{
				writer.WriteAttributeString("xml", "space", null, "preserve");
			}
			text = PrepareString(text);
			text = ReplaceWrongChars(text);
			writer.WriteString(text);
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	private string ReplaceWrongChars(string strText)
	{
		StringBuilder stringBuilder = new StringBuilder();
		_ = strText?.Length;
		char[] array = strText.ToCharArray();
		foreach (char c in array)
		{
			int num = c;
			if ((num < 32 && Array.IndexOf(allowedChars, c) < 0) || char.IsSurrogate(c))
			{
				stringBuilder.Append(string.Format("_x{0}_", num.ToString("X4")));
			}
			else
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	private string PrepareString(string text)
	{
		StringBuilder stringBuilder = new StringBuilder(text);
		int num = 0;
		int num2 = 0;
		while (num2 < text.Length)
		{
			int num3 = text.IndexOf("_x", num2);
			if (num3 == -1)
			{
				break;
			}
			num3 += 2;
			int num4 = text.IndexOf("_", num3);
			if (num4 == -1)
			{
				break;
			}
			if (num4 - num3 == 4 && IsHexa(text.Substring(num3, 4)))
			{
				stringBuilder.Insert(num3 - 2 + num, "_x005F");
				num += "_x005F".Length;
			}
			num2 = num4;
		}
		return stringBuilder.ToString();
	}

	private static bool IsHexa(string value)
	{
		int result;
		return int.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result);
	}

	private void SerializeRichTextRun(XmlWriter writer, TextWithFormat text)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		text.Defragment();
		FontsCollection innerFonts = m_book.InnerFonts;
		SortedList<int, int> formattingRuns = text.FormattingRuns;
		string text2 = text.Text;
		string strString = string.Empty;
		int iFontIndex = -1;
		int num = 0;
		int num2 = 0;
		int length = text2.Length;
		foreach (KeyValuePair<int, int> item in formattingRuns)
		{
			num = item.Key - num2;
			if (length >= num)
			{
				strString = text2.Substring(num2, num);
			}
			SerializeRichTextRunSingleEntry(writer, innerFonts, strString, iFontIndex);
			iFontIndex = item.Value;
			num2 += num;
		}
		if (length >= num2)
		{
			strString = text2.Substring(num2);
		}
		SerializeRichTextRunSingleEntry(writer, innerFonts, strString, iFontIndex);
	}

	private void SerializeRichTextRunSingleEntry(XmlWriter writer, FontsCollection fonts, string strString, int iFontIndex)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (fonts == null)
		{
			throw new ArgumentNullException("fonts");
		}
		if (strString == null)
		{
			throw new ArgumentNullException("strString");
		}
		if (!strString.Contains("\r\n"))
		{
			strString = strString.Replace("\n", "\r\n");
		}
		writer.WriteStartElement("r");
		if (iFontIndex != -1)
		{
			IOfficeFont font = fonts[iFontIndex];
			SerializeFont(writer, font, "rPr");
		}
		writer.WriteStartElement("t");
		int length = strString.Length;
		if (length > 0)
		{
			char c = strString[length - 1];
			if (strString[0] == ' ' || c == ' ' || strString.StartsWith("\r\n") || strString.EndsWith("\r\n") || strString.EndsWith("\t") || strString.StartsWith("\t"))
			{
				writer.WriteAttributeString("xml", "space", null, "preserve");
			}
		}
		writer.WriteValue(strString);
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	private void SerializeColors(XmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (!IsPaletteDefault())
		{
			writer.WriteStartElement("colors");
			SerializePalette(writer);
			writer.WriteEndElement();
		}
	}

	private void SerializePalette(XmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		writer.WriteStartElement("indexedColors");
		Color[] palette = m_book.Palette;
		int i = 0;
		for (int num = palette.Length; i < num; i++)
		{
			Color color = palette[i];
			SerializeRgbColor(writer, "rgbColor", color);
		}
		writer.WriteEndElement();
	}

	private bool IsPaletteDefault()
	{
		List<Color> innerPalette = m_book.InnerPalette;
		Color[] dEF_PALETTE = WorkbookImpl.DEF_PALETTE;
		bool result = true;
		int i = 0;
		for (int count = innerPalette.Count; i < count; i++)
		{
			Color color = innerPalette[i];
			Color color2 = dEF_PALETTE[i];
			if (color.ToArgb() != color2.ToArgb())
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private void SerializeColumns(XmlWriter writer, WorksheetImpl sheet, Dictionary<int, int> dicStyles)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (dicStyles == null)
		{
			throw new ArgumentNullException("dicStyles");
		}
		ColumnInfoRecord[] columnInformation = sheet.ColumnInformation;
		bool flag = true;
		double standardWidth = sheet.StandardWidth;
		int i = 1;
		for (int maxColumnCount = m_book.MaxColumnCount; i <= maxColumnCount; i++)
		{
			ColumnInfoRecord columnInfoRecord = columnInformation[i];
			if (columnInfoRecord != null)
			{
				if (flag)
				{
					writer.WriteStartElement("cols");
				}
				i = SerializeColumn(writer, columnInfoRecord, dicStyles, standardWidth, sheet);
				flag = false;
			}
		}
		if (!flag)
		{
			writer.WriteEndElement();
		}
	}

	private int SerializeColumn(XmlWriter writer, ColumnInfoRecord columnInfo, Dictionary<int, int> dicStyles, double defaultWidth, WorksheetImpl sheet)
	{
		if (columnInfo == null)
		{
			return int.MaxValue;
		}
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (dicStyles == null)
		{
			throw new ArgumentNullException("dicStyles");
		}
		int result = FindSameColumns(sheet, columnInfo.FirstColumn + 1);
		writer.WriteStartElement("col");
		writer.WriteAttributeString("min", (columnInfo.FirstColumn + 1).ToString());
		writer.WriteAttributeString("max", result.ToString());
		double num = (double)(int)columnInfo.ColumnWidth / 256.0;
		if (num > (double)sheet.MaxColumnWidth)
		{
			writer.WriteAttributeString("width", sheet.MaxColumnWidth.ToString(NumberFormatInfo.InvariantInfo));
		}
		else
		{
			writer.WriteAttributeString("width", num.ToString(NumberFormatInfo.InvariantInfo));
		}
		if (columnInfo.ExtendedFormatIndex != sheet.ParentWorkbook.DefaultXFIndex)
		{
			int extendedFormatIndex = columnInfo.ExtendedFormatIndex;
			if (!dicStyles.TryGetValue(extendedFormatIndex, out var value))
			{
				value = extendedFormatIndex;
			}
			writer.WriteAttributeString("style", value.ToString());
		}
		SerializeAttribute(writer, "hidden", columnInfo.IsHidden, defaultValue: false);
		SerializeAttribute(writer, "bestFit", columnInfo.IsBestFit, defaultValue: false);
		SerializeAttribute(writer, "phonetic", columnInfo.IsPhenotic, defaultValue: false);
		SerializeAttribute(writer, "customWidth", columnInfo.IsUserSet || defaultWidth != num, defaultValue: false);
		SerializeAttribute(writer, "collapsed", columnInfo.IsCollapsed, defaultValue: false);
		SerializeAttribute(writer, "outlineLevel", columnInfo.OutlineLevel, 0);
		writer.WriteEndElement();
		return result;
	}

	private int FindSameColumns(WorksheetImpl sheet, int iColumnIndex)
	{
		ColumnInfoRecord[] columnInformation = sheet.ColumnInformation;
		ColumnInfoRecord columnInfoRecord = columnInformation[iColumnIndex];
		while (iColumnIndex < m_book.MaxColumnCount)
		{
			int num = iColumnIndex + 1;
			ColumnInfoRecord columnInfoRecord2 = columnInformation[num];
			if (columnInfoRecord2 == null || columnInfoRecord2.ExtendedFormatIndex != columnInfoRecord.ExtendedFormatIndex || columnInfoRecord2.ColumnWidth != columnInfoRecord.ColumnWidth || columnInfoRecord2.IsCollapsed != columnInfoRecord.IsCollapsed || columnInfoRecord2.IsHidden != columnInfoRecord.IsHidden || columnInfoRecord2.OutlineLevel != columnInfoRecord.OutlineLevel)
			{
				break;
			}
			iColumnIndex++;
		}
		return iColumnIndex;
	}

	private string GetDVTypeName(ExcelDataType dataType)
	{
		return dataType switch
		{
			ExcelDataType.Any => "none", 
			ExcelDataType.Date => "date", 
			ExcelDataType.Decimal => "decimal", 
			ExcelDataType.Formula => "custom", 
			ExcelDataType.Integer => "whole", 
			ExcelDataType.TextLength => "textLength", 
			ExcelDataType.Time => "time", 
			ExcelDataType.User => "list", 
			_ => throw new ArgumentOutOfRangeException("dataType"), 
		};
	}

	private string GetDVErrorStyleType(ExcelErrorStyle errorStyle)
	{
		return errorStyle switch
		{
			ExcelErrorStyle.Info => "information", 
			ExcelErrorStyle.Stop => "stop", 
			ExcelErrorStyle.Warning => "warning", 
			_ => throw new ArgumentOutOfRangeException("errorStyle"), 
		};
	}

	private string GetDVCompareOperatorType(ExcelDataValidationComparisonOperator compareOperator)
	{
		return compareOperator switch
		{
			ExcelDataValidationComparisonOperator.Between => "between", 
			ExcelDataValidationComparisonOperator.Equal => "equal", 
			ExcelDataValidationComparisonOperator.Greater => "greaterThan", 
			ExcelDataValidationComparisonOperator.GreaterOrEqual => "greaterThanOrEqual", 
			ExcelDataValidationComparisonOperator.Less => "lessThan", 
			ExcelDataValidationComparisonOperator.LessOrEqual => "lessThanOrEqual", 
			ExcelDataValidationComparisonOperator.NotBetween => "notBetween", 
			ExcelDataValidationComparisonOperator.NotEqual => "notEqual", 
			_ => throw new ArgumentOutOfRangeException("compareOperator"), 
		};
	}

	private string GetAFConditionOperatorName(OfficeFilterCondition filterCondition)
	{
		return filterCondition switch
		{
			OfficeFilterCondition.Equal => "equal", 
			OfficeFilterCondition.Greater => "greaterThan", 
			OfficeFilterCondition.GreaterOrEqual => "greaterThanOrEqual", 
			OfficeFilterCondition.Less => "lessThan", 
			OfficeFilterCondition.LessOrEqual => "lessThanOrEqual", 
			OfficeFilterCondition.NotEqual => "notEqual", 
			_ => throw new ArgumentOutOfRangeException("filterCondition"), 
		};
	}

	private ushort GetActivePane(PaneRecord paneRecord)
	{
		if (paneRecord != null)
		{
			if (paneRecord.VerticalSplit == 0 && paneRecord.HorizontalSplit == 0)
			{
				paneRecord.ActivePane = 3;
			}
			else if (paneRecord.VerticalSplit == 0)
			{
				paneRecord.ActivePane = 2;
			}
			else if (paneRecord.HorizontalSplit == 0)
			{
				paneRecord.ActivePane = 1;
			}
		}
		return paneRecord.ActivePane;
	}

	private string GetAFFilterValue(IAutoFilterCondition autoFilterCondition)
	{
		switch (autoFilterCondition.DataType)
		{
		case OfficeFilterDataType.String:
			return autoFilterCondition.String;
		case OfficeFilterDataType.FloatingPoint:
			return autoFilterCondition.Double.ToString();
		case OfficeFilterDataType.Boolean:
			if (!autoFilterCondition.Boolean)
			{
				return "0";
			}
			return "1";
		case OfficeFilterDataType.ErrorCode:
			return FormulaUtil.ErrorCodeToName[autoFilterCondition.ErrorCode];
		default:
			throw new ArgumentOutOfRangeException("dataType");
		}
	}

	internal string GetCFComparisonOperatorName(ExcelComparisonOperator comparisonOperator)
	{
		return comparisonOperator switch
		{
			ExcelComparisonOperator.Between => "between", 
			ExcelComparisonOperator.BeginsWith => "beginsWith", 
			ExcelComparisonOperator.ContainsText => "containsText", 
			ExcelComparisonOperator.EndsWith => "endsWith", 
			ExcelComparisonOperator.NotContainsText => "notContains", 
			ExcelComparisonOperator.Equal => "equal", 
			ExcelComparisonOperator.Greater => "greaterThan", 
			ExcelComparisonOperator.GreaterOrEqual => "greaterThanOrEqual", 
			ExcelComparisonOperator.Less => "lessThan", 
			ExcelComparisonOperator.LessOrEqual => "lessThanOrEqual", 
			ExcelComparisonOperator.None => "notContains", 
			ExcelComparisonOperator.NotBetween => "notBetween", 
			ExcelComparisonOperator.NotEqual => "notEqual", 
			_ => throw new ArgumentOutOfRangeException("filterCondition"), 
		};
	}

	internal string GetCFTimePeriodType(CFTimePeriods timePeriod)
	{
		return timePeriod switch
		{
			CFTimePeriods.Today => "today", 
			CFTimePeriods.Yesterday => "yesterday", 
			CFTimePeriods.Tomorrow => "tomorrow", 
			CFTimePeriods.Last7Days => "last7Days", 
			CFTimePeriods.LastWeek => "lastWeek", 
			CFTimePeriods.ThisWeek => "thisWeek", 
			CFTimePeriods.NextWeek => "nextWeek", 
			CFTimePeriods.LastMonth => "lastMonth", 
			CFTimePeriods.ThisMonth => "thisMonth", 
			CFTimePeriods.NextMonth => "nextMonth", 
			_ => throw new ArgumentOutOfRangeException("timePeriod"), 
		};
	}

	internal string GetCFType(ExcelCFType typeCF, ExcelComparisonOperator compOperator)
	{
		return typeCF switch
		{
			ExcelCFType.CellValue => "cellIs", 
			ExcelCFType.SpecificText => compOperator switch
			{
				ExcelComparisonOperator.BeginsWith => "beginsWith", 
				ExcelComparisonOperator.ContainsText => "containsText", 
				ExcelComparisonOperator.EndsWith => "endsWith", 
				ExcelComparisonOperator.NotContainsText => "notContainsText", 
				_ => throw new ArgumentException("ComOperator"), 
			}, 
			ExcelCFType.Formula => "expression", 
			ExcelCFType.DataBar => "dataBar", 
			ExcelCFType.IconSet => "iconSet", 
			ExcelCFType.ColorScale => "colorScale", 
			ExcelCFType.Blank => "containsBlanks", 
			ExcelCFType.NoBlank => "notContainsBlanks", 
			ExcelCFType.ContainsErrors => "containsErrors", 
			ExcelCFType.NotContainsErrors => "notContainsErrors", 
			ExcelCFType.TimePeriod => "timePeriod", 
			_ => throw new ArgumentOutOfRangeException("typeCF"), 
		};
	}

	public static void SerializePrintSettings(XmlWriter writer, IPageSetupBase pageSetup, IPageSetupConstantsProvider constants, bool isChartSettings)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (pageSetup == null)
		{
			throw new ArgumentNullException("pageSetup");
		}
		if (isChartSettings)
		{
			SerializePrintOptions(writer, pageSetup, constants);
			SerializeHeaderFooter(writer, pageSetup, constants);
			SerializePageMargins(writer, pageSetup, constants);
			SerializePageSetup(writer, pageSetup, constants);
		}
		else
		{
			SerializePrintOptions(writer, pageSetup, constants);
			SerializePageMargins(writer, pageSetup, constants);
			SerializePageSetup(writer, pageSetup, constants);
			SerializeHeaderFooter(writer, pageSetup, constants);
		}
	}

	private static void SerializePrintOptions(XmlWriter writer, IPageSetupBase pageSetup, IPageSetupConstantsProvider constants)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (pageSetup == null)
		{
			throw new ArgumentNullException("pageSetup");
		}
		IPageSetup pageSetup2 = pageSetup as IPageSetup;
		bool flag = pageSetup2?.PrintGridlines ?? false;
		bool flag2 = pageSetup2?.PrintHeadings ?? false;
		if (flag || flag2 || pageSetup.CenterHorizontally || pageSetup.CenterVertically)
		{
			writer.WriteStartElement("printOptions", constants.Namespace);
			SerializeAttribute(writer, "gridLines", flag, defaultValue: false);
			SerializeAttribute(writer, "headings", flag2, defaultValue: false);
			SerializeAttribute(writer, "horizontalCentered", pageSetup.CenterHorizontally, defaultValue: false);
			SerializeAttribute(writer, "verticalCentered", pageSetup.CenterVertically, defaultValue: false);
			writer.WriteEndElement();
		}
	}

	public static void SerializePageMargins(XmlWriter writer, IPageSetupBase pageSetup, IPageSetupConstantsProvider constants)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (pageSetup == null)
		{
			throw new ArgumentNullException("pageSetup");
		}
		if (constants == null)
		{
			throw new ArgumentNullException("constants");
		}
		ValidatePageMargins(pageSetup as PageSetupBaseImpl);
		writer.WriteStartElement(constants.PageMarginsTag, constants.Namespace);
		SerializeAttribute(writer, constants.LeftMargin, pageSetup.LeftMargin, double.MinValue);
		SerializeAttribute(writer, constants.RightMargin, pageSetup.RightMargin, double.MinValue);
		SerializeAttribute(writer, constants.TopMargin, pageSetup.TopMargin, double.MinValue);
		SerializeAttribute(writer, constants.BottomMargin, pageSetup.BottomMargin, double.MinValue);
		SerializeAttribute(writer, constants.HeaderMargin, pageSetup.HeaderMargin, double.MinValue);
		SerializeAttribute(writer, constants.FooterMargin, pageSetup.FooterMargin, double.MinValue);
		writer.WriteEndElement();
	}

	private static void ValidatePageMargins(PageSetupBaseImpl pageSetup)
	{
		if (pageSetup.dictPaperHeight.ContainsKey(pageSetup.PaperSize) && pageSetup.dictPaperWidth.ContainsKey(pageSetup.PaperSize))
		{
			double num = pageSetup.dictPaperWidth[pageSetup.PaperSize];
			double num2 = pageSetup.dictPaperHeight[pageSetup.PaperSize];
			if (pageSetup.LeftMargin + pageSetup.RightMargin > num)
			{
				throw new ArgumentException("Left Margin and Right Margin size exceeds the allowed size");
			}
			if (pageSetup.TopMargin + pageSetup.BottomMargin > num2)
			{
				throw new ArgumentException("Top Margin and Bottom Margin size exceeds the allowed size");
			}
		}
	}

	public static void SerializePageSetup(XmlWriter writer, IPageSetupBase pageSetup, IPageSetupConstantsProvider constants)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (pageSetup == null)
		{
			throw new ArgumentNullException("pageSetup");
		}
		if ((pageSetup as PageSetupBaseImpl).IsNotValidSettings)
		{
			return;
		}
		writer.WriteStartElement("pageSetup", constants.Namespace);
		SerializeAttribute(writer, "paperSize", (int)pageSetup.PaperSize, 1);
		if (pageSetup.Zoom != 0)
		{
			SerializeAttribute(writer, "scale", pageSetup.Zoom, 100);
		}
		SerializeAttribute(writer, "firstPageNumber", (uint)pageSetup.FirstPageNumber, 1.0);
		PageSetupImpl pageSetupImpl = pageSetup as PageSetupImpl;
		if (pageSetupImpl != null)
		{
			SerializeAttribute(writer, "fitToWidth", pageSetupImpl.FitToPagesWide, 1);
			SerializeAttribute(writer, "fitToHeight", pageSetupImpl.FitToPagesTall, 1);
		}
		if (pageSetup.Order.ToString() != OfficeOrder.DownThenOver.ToString())
		{
			writer.WriteAttributeString("pageOrder", LowerFirstLetter(pageSetup.Order.ToString()));
		}
		SerializeAttribute(writer, "orientation", pageSetup.Orientation, (OfficePageOrientation)0);
		SerializeAttribute(writer, "blackAndWhite", pageSetup.BlackAndWhite, defaultValue: false);
		SerializeAttribute(writer, "draft", pageSetup.Draft, defaultValue: false);
		string value = PrintCommentsToString(pageSetup.PrintComments);
		SerializeAttribute(writer, "cellComments", value, "none");
		SerializeAttribute(writer, "useFirstPageNumber", !pageSetup.AutoFirstPageNumber, defaultValue: false);
		string value2 = PrintErrorsToString(pageSetup.PrintErrors);
		SerializeAttribute(writer, "errors", value2, "displayed");
		PageSetupBaseImpl pageSetupBaseImpl = (PageSetupBaseImpl)pageSetup;
		if (pageSetupBaseImpl.HResolution > 0)
		{
			SerializeAttribute(writer, "horizontalDpi", pageSetupBaseImpl.HResolution, 600);
		}
		if (pageSetupBaseImpl.VResolution > 0)
		{
			SerializeAttribute(writer, "verticalDpi", pageSetupBaseImpl.VResolution, 600);
		}
		if (!pageSetupBaseImpl.IsNotValidSettings)
		{
			SerializeAttribute(writer, "copies", pageSetup.Copies, 1);
		}
		if (pageSetupImpl != null)
		{
			string relationId = pageSetupImpl.RelationId;
			if (relationId != null)
			{
				writer.WriteAttributeString("id", relationId);
			}
		}
		writer.WriteEndElement();
	}

	internal static void SerializeHeaderFooter(XmlWriter writer, IPageSetupBase pageSetup, IPageSetupConstantsProvider constants)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (pageSetup == null)
		{
			throw new ArgumentNullException("pageSetup");
		}
		PageSetupBaseImpl pageSetupBaseImpl = (PageSetupBaseImpl)pageSetup;
		string fullHeaderString = pageSetupBaseImpl.FullHeaderString;
		string fullFooterString = pageSetupBaseImpl.FullFooterString;
		if ((fullHeaderString != null && fullHeaderString.Length > 0) || (fullFooterString != null && fullFooterString.Length > 0))
		{
			writer.WriteStartElement("headerFooter", constants.Namespace);
			SerializeBool(writer, "scaleWithDoc", pageSetupBaseImpl.HFScaleWithDoc);
			SerializeBool(writer, "alignWithMargins", pageSetupBaseImpl.AlignHFWithPageMargins);
			SerializeBool(writer, "differentFirst", pageSetupBaseImpl.DifferentFirstPageHF);
			SerializeBool(writer, "differentOddEven", pageSetupBaseImpl.DifferentOddAndEvenPagesHF);
			writer.WriteElementString("oddHeader", constants.Namespace, fullHeaderString);
			writer.WriteElementString("oddFooter", constants.Namespace, fullFooterString);
			writer.WriteEndElement();
		}
	}

	private static string PrintCommentsToString(OfficePrintLocation printLocation)
	{
		string text = null;
		return printLocation switch
		{
			OfficePrintLocation.PrintInPlace => "asDisplayed", 
			OfficePrintLocation.PrintNoComments => "none", 
			OfficePrintLocation.PrintSheetEnd => "atEnd", 
			_ => throw new ArgumentOutOfRangeException("printLocation"), 
		};
	}

	private static string PrintErrorsToString(OfficePrintErrors printErrors)
	{
		string text = null;
		return printErrors switch
		{
			OfficePrintErrors.PrintErrorsBlank => "blank", 
			OfficePrintErrors.PrintErrorsDash => "dash", 
			OfficePrintErrors.PrintErrorsDisplayed => "displayed", 
			OfficePrintErrors.PrintErrorsNA => "NA", 
			_ => throw new ArgumentOutOfRangeException("printLocation"), 
		};
	}

	private string ConvertAddressString(string strAdress)
	{
		return strAdress?.Replace(" ", "%20");
	}

	private void SerializeSheetlevelProperties(XmlWriter writer, WorksheetImpl sheet)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		writer.WriteStartElement("sheetPr");
		SerializeAttribute(writer, "transitionEvaluation", sheet.IsTransitionEvaluation, defaultValue: false);
		OfficeKnownColors tabColor = sheet.TabColor;
		string codeName = sheet.CodeName;
		if ((m_book.HasMacros && codeName != null && codeName.Length > 0) || sheet.HasCodeName)
		{
			writer.WriteAttributeString("codeName", codeName);
		}
		if (tabColor != (OfficeKnownColors)(-1))
		{
			writer.WriteStartElement("tabColor");
			int num = (int)tabColor;
			writer.WriteAttributeString("indexed", num.ToString());
			writer.WriteEndElement();
		}
		IPageSetup pageSetup = sheet.PageSetup;
		if (!pageSetup.IsSummaryColumnRight || !pageSetup.IsSummaryRowBelow)
		{
			writer.WriteStartElement("outlinePr");
			SerializeAttribute(writer, "summaryRight", pageSetup.IsSummaryColumnRight, defaultValue: true);
			SerializeAttribute(writer, "summaryBelow", pageSetup.IsSummaryRowBelow, defaultValue: true);
			writer.WriteEndElement();
		}
		if (pageSetup.IsFitToPage)
		{
			writer.WriteStartElement("pageSetUpPr");
			SerializeAttribute(writer, "fitToPage", pageSetup.IsFitToPage, defaultValue: false);
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	private void SerilizeBackgroundImage(XmlWriter writer, WorksheetImpl sheet)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
	}

	protected virtual void SerializeAppVersion(XmlWriter writer)
	{
		SerializeElementString(writer, "AppVersion", "12.0000", null);
	}

	private void SerializeCreatedModifiedTimeElement(XmlWriter writer, string tagName, DateTime dateTime)
	{
		if (dateTime.Date != DateTime.MinValue)
		{
			writer.WriteStartElement("dcterms", tagName, null);
			writer.WriteAttributeString("xsi", "type", null, "dcterms:W3CDTF");
			string data = dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
			writer.WriteRaw(data);
			writer.WriteEndElement();
		}
	}

	private void SerializeBookViews(XmlWriter writer, List<Dictionary<string, string>> lstBookViews)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		Dictionary<string, string> dictionary = ((lstBookViews == null) ? new Dictionary<string, string>() : lstBookViews[0]);
		ChangeCreateAttributeValue(dictionary, "activeTab", m_book.ActiveSheetIndex);
		ChangeCreateAttributeValue(dictionary, "firstSheet", m_book.DisplayedTab);
		if (lstBookViews == null && dictionary.Count != 0)
		{
			lstBookViews = new List<Dictionary<string, string>>();
			lstBookViews.Add(dictionary);
		}
		if (lstBookViews == null)
		{
			writer.WriteStartElement("bookViews");
			writer.WriteStartElement("workbookView");
			writer.WriteEndElement();
			writer.WriteEndElement();
			return;
		}
		writer.WriteStartElement("bookViews");
		foreach (Dictionary<string, string> lstBookView in lstBookViews)
		{
			SerializeWorkbookView(writer, lstBookView);
		}
		writer.WriteEndElement();
	}

	private void SerializeWorkbookView(XmlWriter writer, Dictionary<string, string> dicView)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (dicView == null)
		{
			throw new ArgumentNullException("dicView");
		}
		writer.WriteStartElement("workbookView");
		foreach (KeyValuePair<string, string> item in dicView)
		{
			SerializeAttribute(writer, item.Key, item.Value, string.Empty);
		}
		writer.WriteEndElement();
	}

	private void ChangeCreateAttributeValue(Dictionary<string, string> dicBookView, string strAttributeName, int iNewValue)
	{
		if (dicBookView.ContainsKey(strAttributeName))
		{
			if (iNewValue != 0)
			{
				dicBookView[strAttributeName] = iNewValue.ToString();
			}
			else
			{
				dicBookView.Remove(strAttributeName);
			}
		}
		else if (iNewValue != 0)
		{
			dicBookView.Add(strAttributeName, iNewValue.ToString());
		}
	}

	private void SerializeBookExternalLinks(XmlWriter writer, RelationCollection relations)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		ExternBookCollection externWorkbooks = m_book.ExternWorkbooks;
		_ = m_book.Worksheets;
		bool flag = true;
		if (externWorkbooks.Count != 0)
		{
			int i = 0;
			for (int count = externWorkbooks.Count; i < count; i++)
			{
				ExternWorkbookImpl externWorkbookImpl = externWorkbooks[i];
				if (!externWorkbookImpl.IsInternalReference && !string.IsNullOrEmpty(externWorkbookImpl.URL) && !externWorkbookImpl.IsAddInFunctions)
				{
					if (flag)
					{
						flag = false;
						writer.WriteStartElement("externalReferences");
					}
					SerializeLink(externWorkbookImpl, writer, relations);
				}
			}
		}
		if (m_book.PreservedExternalLinks.Count > 0)
		{
			foreach (string preservedExternalLink in m_book.PreservedExternalLinks)
			{
				writer.WriteStartElement("externalReference");
				writer.WriteAttributeString("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships", preservedExternalLink);
				writer.WriteEndElement();
			}
		}
		if (!flag)
		{
			writer.WriteEndElement();
		}
	}

	private void SerializeLink(ExternWorkbookImpl externBook, XmlWriter writer, RelationCollection relations)
	{
		if (externBook == null)
		{
			throw new ArgumentNullException("externBook");
		}
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		string text = m_book.DataHolder.SerializeExternalLink(externBook);
		writer.WriteStartElement("externalReference");
		string text2 = relations.GenerateRelationId();
		relations[text2] = new Relation("/" + text, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/externalLink");
		writer.WriteAttributeString("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships", text2);
		writer.WriteEndElement();
	}

	private void SerializePagebreaks(XmlWriter writer, IWorksheet sheet)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
	}

	private void SerializeSinglePagebreak(XmlWriter writer, int iRowColumn, int iStart, int iEnd, ExcelPageBreak type)
	{
		writer.WriteStartElement("brk");
		SerializeAttribute(writer, "id", iRowColumn, 0);
		SerializeAttribute(writer, "min", iStart, 0);
		SerializeAttribute(writer, "max", iEnd, 0);
		SerializeAttribute(writer, "man", type == ExcelPageBreak.PageBreakManual, defaultValue: false);
		writer.WriteEndElement();
	}

	public static void SerializeExtent(XmlWriter writer, Size extent)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		int width = extent.Width;
		int height = extent.Height;
		width = (int)ApplicationImpl.ConvertFromPixel(width, MeasureUnits.EMU);
		height = (int)ApplicationImpl.ConvertFromPixel(height, MeasureUnits.EMU);
		writer.WriteStartElement("ext", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		if (width <= 0)
		{
			width = 8666049;
		}
		if (height <= 0)
		{
			height = 6293304;
		}
		writer.WriteAttributeString("cx", width.ToString());
		writer.WriteAttributeString("cy", height.ToString());
		writer.WriteEndElement();
	}

	private bool CheckSpecialCharacters(string sheetName)
	{
		if (sheetName.StartsWith("'") && sheetName.EndsWith("'"))
		{
			return false;
		}
		char[] specialChars = SpecialChars;
		foreach (char value in specialChars)
		{
			if (sheetName.IndexOf(value) != -1)
			{
				return true;
			}
		}
		return false;
	}

	internal void SerializeWorksheets(ZipArchive archive, IWorkbook workbook, ChartImpl chart, int workSheetIndex, Dictionary<int, int> styleIndex, bool defaultExcelFile)
	{
		int num = -1;
		if (!defaultExcelFile)
		{
			try
			{
				for (num = 0; num < workbook.Worksheets.Count; num++)
				{
					WorksheetImpl worksheetImpl = workbook.Worksheets[num] as WorksheetImpl;
					if (worksheetImpl.IsParsed && !worksheetImpl.ParseDataOnDemand)
					{
						ZipArchiveItem zipArchiveItem = archive[worksheetImpl.ArchiveItemName];
						if (zipArchiveItem != null)
						{
							Dictionary<string, string> extraAttributes = null;
							Stream worksheetStreamData = GetWorksheetStreamData(zipArchiveItem.DataStream, extraAttributes);
							MemoryStream memoryStream = new MemoryStream();
							using (XmlWriter writer = XmlWriter.Create(memoryStream))
							{
								serializeSheet(writer, worksheetImpl, memoryStream, styleIndex, worksheetStreamData, extraAttributes, isCustomFile: true);
							}
							archive.RemoveItem(worksheetImpl.ArchiveItemName);
							archive.AddItem(worksheetImpl.ArchiveItemName, memoryStream, bControlStream: true, DocGen.Compression.FileAttributes.Archive);
						}
					}
				}
				return;
			}
			catch (Exception)
			{
				return;
			}
		}
		SerializeDefaultExcelWorksheet(archive, workbook, chart, workSheetIndex, styleIndex);
	}

	private Stream GetWorksheetStreamData(Stream stream, Dictionary<string, string> extraAttributes)
	{
		extraAttributes = new Dictionary<string, string>();
		stream.Position = 0L;
		XmlReader xmlReader = UtilityMethods.CreateReader(stream);
		Stream result = null;
		if (xmlReader == null)
		{
			throw new ArgumentNullException("reader");
		}
		while (xmlReader.NodeType != XmlNodeType.Element && xmlReader.LocalName != "worksheet" && !xmlReader.EOF)
		{
			xmlReader.Read();
		}
		if (xmlReader.HasAttributes)
		{
			while (xmlReader.MoveToNextAttribute())
			{
				extraAttributes.Add(xmlReader.Prefix + ":" + xmlReader.LocalName, xmlReader.Value);
			}
			xmlReader.MoveToElement();
		}
		if (!xmlReader.EOF)
		{
			xmlReader.Read();
			result = Excel2007Parser.ParseMiscSheetElements(xmlReader);
		}
		return result;
	}

	internal void SerializeDefaultExcelWorksheet(ZipArchive archive, IWorkbook workbook, ChartImpl chart, int workSheetIndex, Dictionary<int, int> styleIndex)
	{
		archive.RemoveItem("xl/worksheets/sheet" + (chart.Workbook.ActiveSheetIndex + 1) + ".xml");
		MemoryStream memoryStream = new MemoryStream();
		using (XmlWriter writer = XmlWriter.Create(memoryStream))
		{
			try
			{
				serializeSheet(writer, chart.Workbook.Worksheets[workSheetIndex] as WorksheetImpl, memoryStream, styleIndex, null, null, isCustomFile: false);
			}
			catch
			{
				serializeSheet(writer, chart.Workbook.Worksheets[0] as WorksheetImpl, memoryStream, styleIndex, null, null, isCustomFile: false);
			}
		}
		if (chart.Workbook.ActiveSheetIndex != -1)
		{
			archive.AddItem("xl/worksheets/sheet" + (chart.Workbook.ActiveSheetIndex + 1) + ".xml", memoryStream, bControlStream: true, DocGen.Compression.FileAttributes.Archive);
		}
		else
		{
			archive.AddItem("xl/worksheets/sheet1.xml", memoryStream, bControlStream: true, DocGen.Compression.FileAttributes.Archive);
		}
	}
}
