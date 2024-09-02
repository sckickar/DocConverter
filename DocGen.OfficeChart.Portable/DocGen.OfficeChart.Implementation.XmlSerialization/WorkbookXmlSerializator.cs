using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using DocGen.Compression;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Interfaces.XmlSerialization;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation.XmlSerialization;

[XmlSerializator(OfficeXmlSaveType.MSExcel)]
internal class WorkbookXmlSerializator : IXmlSerializator
{
	public enum XmlSerializationCellType
	{
		Number,
		DateTime,
		Boolean,
		String,
		Error
	}

	public const string DEF_VERSION_STRING = "<?xml version=\"1.0\"?>";

	public const string DEF_APPLICATION_STRING = "<?mso-application progid=\"Excel.Sheet\"?>";

	private const string DEF_O_NAMESPACE = "urn:schemas-microsoft-com:office:office";

	private const string DEF_X_NAMESPACE = "urn:schemas-microsoft-com:office:excel";

	private const string DEF_SS_NAMESPACE = "urn:schemas-microsoft-com:office:spreadsheet";

	private const string DEF_HTML_NAMESPACE = "http://www.w3.org/TR/REC-html40";

	private const string DEF_SS_PREF = "ss";

	private const string DEF_HTML_PREF = "html";

	private const string DEF_O_PREF = "o";

	private const string DEF_X_PREF = "x";

	private const string DEF_NAMESPACE_PREF = "xmlns:";

	internal const string DEF_XMLNS_PREF = "xmlns";

	public const string DEF_WORKBOOK_PREF = "Workbook";

	public const string DEF_WORKSHEET_PREF = "Worksheet";

	public const string DEF_NAME_PREF = "Name";

	public const string DEF_TABLE_PREF = "Table";

	public const string DEF_ROW_PREF = "Row";

	public const string DEF_CELL_PREF = "Cell";

	public const string DEF_DATA_PREF = "Data";

	public const string DEF_NAMES_PREF = "Names";

	public const string DEF_NAMEDRANGE_PREF = "NamedRange";

	public const string DEF_STYLES_PREF = "Styles";

	public const string DEF_STYLE_PREF = "Style";

	public const string DEF_FONT_PREF = "Font";

	public const string DEF_PROTECTION_PREF = "Protection";

	public const string DEF_ALIGNMENT_PREF = "Alignment";

	public const string DEF_NUMBERFORMAT_PREF = "NumberFormat";

	public const string DEF_INTERIOR_PREF = "Interior";

	public const string DEF_BORDERS_PREF = "Borders";

	public const string DEF_BORDER_PREF = "Border";

	private const string DEF_AUTOFILTER_PREF = "AutoFilter";

	private const string DEF_AUTOFILTERCOLUMN_PREF = "AutoFilterColumn";

	private const string DEF_AUTOFILTERAND_PREF = "AutoFilterAnd";

	private const string DEF_AUTOFILTERCONDITION_PREF = "AutoFilterCondition";

	private const string DEF_AUTOFILTEROR_PREF = "AutoFilterOr";

	public const string DEF_COMMENT_PREF = "Comment";

	private const string DEF_B_TAG = "<B>";

	private const string DEF_B_END_TAG = "</B>";

	private const string DEF_I_TAG = "<I>";

	private const string DEF_I_END_TAG = "</I>";

	private const string DEF_U_TAG = "<U>";

	private const string DEF_U_END_TAG = "</U>";

	private const string DEF_S_TAG = "<S>";

	private const string DEF_S_END_TAG = "</S>";

	private const string DEF_SUB_TAG = "<Sub>";

	private const string DEF_SUB_END_TAG = "</Sub>";

	private const string DEF_SUP_TAG = "<Sup>";

	private const string DEF_SUP_END_TAG = "</Sup>";

	private const string DEF_FONT_END_TAG = "</Font>";

	private const string DEF_FONT_TAG = "<Font";

	public const string DEF_SPAN_PREF = "Span";

	public const string DEF_COLUMN_PREF = "Column";

	public const string DEF_CONDITIONAL_FORMATTING_PREF = "ConditionalFormatting";

	public const string DEF_CONDITIONAL_PREF = "Condition";

	public const string DEF_QUALIFIER_PREF = "Qualifier";

	public const string DEF_VALUE1_PREF = "Value1";

	public const string DEF_VALUE2_PREF = "Value2";

	public const string DEF_WORKSHEET_OPTIONS_PREF = "WorksheetOptions";

	public const string DEF_PAGE_SETUP_PREF = "PageSetup";

	public const string DEF_FOOTER_PREF = "Footer";

	public const string DEF_HEADER_PREF = "Header";

	public const string DEF_LAYOUT_PREF = "Layout";

	public const string DEF_PAGE_MARGINS_PREF = "PageMargins";

	public const string DEF_PRINT_PREF = "Print";

	public const string DEF_COMMENTS_LAYOUT_PREF = "CommentsLayout";

	public const string DEF_PRINT_ERRORS_PREF = "PrintErrors";

	private const string DEF_FIT_TO_PAGE_PREF = "FitToPage";

	public const string DEF_LEFT_TO_RIGHT_PREF = "LeftToRight";

	public const string DEF_ACTIVE_PANE_PREF = "ActivePane";

	public const string DEF_FIRST_VISIBLE_ROW_PREF = "TopRowVisible";

	public const string DEF_SPLIT_HORIZONTAL_PANE_PREF = "SplitHorizontal";

	public const string DEF_SPLIT_VERTICAL_PANE_PREF = "SplitVertical";

	public const string DEF_TOPROW_BOTTOM_PANE_PREF = "TopRowBottomPane";

	public const string DEF_LEFTCOLUMN_RIGHT_PANE_PREF = "LeftColumnRightPane";

	public const string DEF_FREEZE_PANES_PREF = "FreezePanes";

	public const string DEF_FROZEN_NOSPLIT_PANES_PREF = "FrozenNoSplit";

	public const string DEF_PANES_PREF = "Panes";

	public const string DEF_PANE_PREF = "Pane";

	public const string DEF_NUMBER_PANE_PREF = "Number";

	public const string DEF_ACTIVECOL_PANE_PREF = "ActiveCol";

	public const string DEF_ACTIVEROW_PANE_PREF = "ActiveRow";

	public const string DEF_TABCOLOR_INDEX_PREF = "TabColorIndex";

	public const string DEF_ZOOM_PREF = "Zoom";

	public const string DEF_DISPLAY_GRIDLINES_PREF = "DoNotDisplayGridlines";

	public const string DEF_VISIBLE_PREF = "Visible";

	private const string DEF_DISPLAY_HEADINGS_PREF = "DoNotDisplayHeadings";

	public const string DEF_EXCELWORKBOOK_PREF = "ExcelWorkbook";

	public const string DEF_ACTIVE_SHEET_PREF = "ActiveSheet";

	private const string DEF_SELECTED_PREF = "Selected";

	private const string DEF_SELECTED_SHEETS_PREF = "SelectedSheets";

	public const string DEF_FIRST_VISIBLE_SHEET_PREF = "FirstVisibleSheet";

	public const string DEF_DATAVALIDATION_PREF = "DataValidation";

	public const string DEF_RIGHTTOLEFT_PREF = "RightToLeft";

	public const string DEF_INDEX_PREF = "Index";

	public const string DEF_TYPE_PREF = "Type";

	private const string DEF_TICKED_PREF = "Ticked";

	public const string DEF_FORMULA_PREF = "Formula";

	public const string DEF_REFERSTO_PREF = "RefersTo";

	public const string DEF_ID_PREF = "ID";

	public const string DEF_PARENT_PREF = "Parent";

	public const string DEF_BOLD_PREF = "Bold";

	public const string DEF_FONTNAME_PREF = "FontName";

	public const string DEF_COLOR_PREF = "Color";

	public const string DEF_ITALIC_PREF = "Italic";

	public const string DEF_OUTLINE_PREF = "Outline";

	public const string DEF_SHADOW_PREF = "Shadow";

	public const string DEF_SIZE_PREF = "Size";

	public const string DEF_STRIKETHROUGH_PREF = "StrikeThrough";

	public const string DEF_UNDERLINE_PREF = "Underline";

	public const string DEF_PROTECTED_PREF = "Protected";

	public const string DEF_HIDEFORMULA_PREF = "HideFormula";

	public const string DEF_HORIZONTAL_PREF = "Horizontal";

	public const string DEF_INDENT_PREF = "Indent";

	public const string DEF_READINGORDER_PREF = "ReadingOrder";

	public const string DEF_ROTATE_PREF = "Rotate";

	public const string DEF_SHRINKTOFIT_PREF = "ShrinkToFit";

	public const string DEF_VERTICAL_PREF = "Vertical";

	public const string DEF_VERTICALTEXT_PREF = "VerticalText";

	public const string DEF_WRAPTEXT_PREF = "WrapText";

	public const string DEF_FORMAT_PREF = "Format";

	public const string DEF_PATTERNCOLOR_PREF = "PatternColor";

	public const string DEF_PATTERN_PREF = "Pattern";

	public const string DEF_POSITION_PREF = "Position";

	public const string DEF_RANGE_PREF = "Range";

	private const string DEF_OPERATOR_PREF = "Operator";

	private const string DEF_VALUE_PREF = "Value";

	public const string DEF_AUTHOR_PREF = "Author";

	public const string DEF_SHOWALWAYS_PREF = "ShowAlways";

	public const string DEF_DEFAULTCOLUMNWIDTH_PREF = "DefaultColumnWidth";

	public const string DEF_DEFAULTROWHEIGHT_PREF = "DefaultRowHeight";

	public const string DEF_WIDTH_PREF = "Width";

	public const string DEF_HIDDEN_PREF = "Hidden";

	public const string DEF_STYLEID_PREF = "StyleID";

	public const string DEF_AUTOFIT_WIDTH_PREF = "AutoFitWidth";

	public const string DEF_AUTOFIT_HEIGHT_PREF = "AutoFitHeight";

	public const string DEF_HEIGHT_PREF = "Height";

	public const string DEF_FACE_PREF = "Face";

	public const string DEF_LINE_STYLE_PREF = "LineStyle";

	public const string DEF_WEIGHT_PREF = "Weight";

	public const string DEF_VERTICAL_ALIGN_PREF = "VerticalAlign";

	public const string DEF_MERGE_ACROSS_PREF = "MergeAcross";

	public const string DEF_MERGE_DOWN_PREF = "MergeDown";

	public const string DEF_HYPRER_TIP_PREF = "HRefScreenTip";

	public const string DEF_HREF_PREF = "HRef";

	public const string DEF_MARGIN_PREF = "Margin";

	public const string DEF_MARGIN_TOP_PREF = "Top";

	public const string DEF_MARGIN_RIGHT_PREF = "Right";

	public const string DEF_MARGIN_LEFT_PREF = "Left";

	public const string DEF_MARGIN_BOTTOM_PREF = "Bottom";

	public const string DEF_CENTER_HORIZONTAL_PREF = "CenterHorizontal";

	public const string DEF_CENTER_VERTICAL_PREF = "CenterVertical";

	public const string DEF_ORIENTATION_PREF = "Orientation";

	public const string DEF_START_PAGE_NUMBER_PREF = "StartPageNumber";

	public const string DEF_NUMBER_OF_COPIES_PREF = "NumberofCopies";

	public const int DEF_NUMBER_OF_COPIES = 1;

	public const string DEF_HORIZONTAL_RESOLUTION_PREF = "HorizontalResolution";

	public const string DEF_PAPER_SIZE_INDEX_PREF = "PaperSizeIndex";

	public const string DEF_SCALE_PREF = "Scale";

	public const string DEF_FIT_WIDTH_PREF = "FitWidth";

	public const string DEF_FIT_HEIGHT_PREF = "FitHeight";

	public const string DEF_GRIDLINES_PREF = "Gridlines";

	public const string DEF_BLACK_AND_WHITE_PREF = "BlackAndWhite";

	public const string DEF_DRAFT_QUALITY_PREF = "DraftQuality";

	public const string DEF_ROWCOL_HEADINGS_PREF = "RowColHeadings";

	public const string DEF_COLON = ":";

	public const string DEF_SEMICOLON = ";";

	public const string DEF_FONT_COLOR_CF = "color";

	public const string DEF_FONT_STYLE_CF = "font-style";

	public const string DEF_FONT_WEIGHT_CF = "font-weight";

	public const string DEF_FONT_BOLD_CF = "700";

	public const string DEF_FONT_REGULAR_CF = "400";

	private const string DEF_FONT_ITALIC_CF = "font-style:italic;";

	private const string DEF_FONT_STRIKE_CF = "text-line-through:single;";

	public const string DEF_FONT_STRIKETHROUGH_CF = "text-line-through";

	public const string DEF_FONT_STRIKETHROUGH_SINGLE_CF = "single";

	public const string DEF_FONT_UNDERLINE_CF = "text-underline-style";

	public const string DEF_PATTERN_BACKGROUND_CF = "background";

	public const string DEF_PATTERN_FILL_CF = "mso-pattern";

	private const string DEF_BORDER_CF = "border-";

	public const string DEF_BORDERTOP_CF = "border-top";

	public const string DEF_BORDERBOTTOM_CF = "border-bottom";

	public const string DEF_BORDERLEFT_CF = "border-left";

	public const string DEF_BORDERRIGHT_CF = "border-right";

	public static readonly string[] DEF_PATTERN_STRING_CF = new string[19]
	{
		"none", "solid", "gray-50", "gray-75", "gray-25", "horz-stripe", "vert-stripe", "reverse-diag-stripe", "diag-stripe", "diag-cross",
		"thick-diag-cross", "thin-horz-stripe", "thin-vert-stripe", "thin-reverse-diag-stripe", "thin-diag-stripe", "thin-horz-cross", "thin-diag-cross", "gray-125", "gray-0625"
	};

	public static readonly string[] DEF_BORDER_LINE_CF = new string[14]
	{
		"none", ".5pt solid", "1.0pt solid", ".5pt dashed", ".5pt dotted", "1.5pt solid", "2.0pt double", ".5pt hairline", "1.0pt dashed", ".5pt dot-dash",
		"1.0pt dot-dash", ".5pt dot-dot-dash", "1.0pt dot-dot-dash", "1.0pt dot-dash-slanted"
	};

	public static readonly string[] DEF_COMPARISION_OPERATORS_PREF = new string[9] { "None", "Between", "NotBetween", "Equal", "NotEqual", "Greater", "Less", "GreaterOrEqual", "LessOrEqual" };

	private const string DEF_FONT_NAME = "Arial";

	private const string DEF_STYLE_NONE = "None";

	public const int DEF_FONT_SIZE = 8;

	private const int DEF_LEFT_DIAGONAL_BORDER = 5;

	private const int DEF_RIGHT_DIAGONAL_BORDER = 6;

	public const int DEF_STYLE_ZERO = 0;

	public const int DEF_STYLE_ROTATION = 90;

	private const int DEF_STYLE_FONT_SIZE = 10;

	public const int DEF_ROTATION_TEXT = 255;

	private const int DEF_BORDER_INCR = 5;

	public const string DEF_STYLE_NAME = "Default";

	private const string DEF_UNIQUE_STRING = "s";

	private const string DEF_STYLE_ALIGN_NONE = "None";

	private const string DEF_STYLE_ALIGN_SUBSCRIPT = "Subscript";

	private const string DEF_STYLE_ALIGN_SUPERSCRIPT = "Superscript";

	public static readonly string[] DEF_BORDER_POSITION_STRING = new string[11]
	{
		"", "", "", "", "", "DiagonalLeft", "DiagonalRight", "Left", "Top", "Bottom",
		"Right"
	};

	public static readonly string[] DEF_BORDER_LINE_TYPE_STRING = new string[14]
	{
		"None", "1 Continuous", "2 Continuous", "1 Dash", "1 Dot", "3 Continuous", "3 Double", "Continuous", "2 Dash", "1 DashDot",
		"2 DashDot", "1 DashDotDot", "2 DashDotDot", "2 SlantDashDot"
	};

	private const double DEF_MARGIN = 0.5;

	private const int DEF_SCALE = 100;

	private const int DEF_FIT = 1;

	private const int DEF_ZOOM = 100;

	public static readonly string[] DEF_PRINT_LOCATION_STRING = new string[3] { "InPlace", "NoComments", "SheetEnd" };

	public static readonly string[] DEF_PRINT_ERROR_STRING = new string[4] { "none", "Blank", "Dash", "NA" };

	public static readonly string[] DEF_VISIBLE_STRING = new string[3] { "error", "SheetHidden", "SheetVeryHidden" };

	private const string DEF_XML_TRUE = "1";

	private const string DEF_XML_FALSE = "0";

	private const string DEF_AUTOFILTER_ALL_TYPE = "All";

	public const string DEF_COLOR_STRING = "#";

	private const string DEF_AUTOFILTER_BOTTOM_TYPE = "Bottom";

	private const string DEF_AUTOFILTER_TOP_TYPE = "Top";

	private const string DEF_AUTOFILTER_PERCENT_TYPE = "Percent";

	private const string DEF_AUTOFILTER_BLANKS_TYPE = "Blanks";

	private const string DEF_AUTOFILTER_CUSTOM_TYPE = "Custom";

	private const string DEF_AUTOFILTER_NON_BLANKS_TYPE = "NonBlanks";

	private const double DEF_COLUMN_WIDTH = 48.0;

	public const double DEF_ROW_HEIGHT = 12.75;

	public const double DEF_COLUMN_DIV = 256.0;

	public const double DEF_ROW_DIV = 20.0;

	private const string DEF_DATATIME_MASK = "yyyy-MM-ddTHH:mm:ss";

	private const int DEF_MERGED_STYLE = 5000;

	public static readonly string[] DEF_PATTERN_STRING = new string[19]
	{
		"None", "Solid", "Gray50", "Gray75", "Gray25", "HorzStripe", "VertStripe", "ReverseDiagStripe", "DiagStripe", "DiagCross",
		"ThickDiagCross", "ThinHorzStripe", "ThinVertStripe", "ThinReverseDiagStripe", "ThinDiagStripe", "ThinHorzCross", "ThinDiagCross", "Gray125", "Gray0625"
	};

	private readonly string[] DEF_AUTOFILTER_OPERATION_STRING = new string[7] { "", "LessThan", "Equals", "LessThanOrEqual", "GreaterThan", "DoesNotEqual", "GreaterThanOrEqual" };

	public static readonly string[] DEF_ERRORSTYLE = new string[3] { "Stop", "Warn", "Info" };

	public static readonly string[] DEF_ALLOWTYPE_STRING = new string[8] { "Any", "Whole", "Decimal", "List", "Date", "Time", "TextLength", "Custom" };

	private const string DEF_10_CHAR = "&#10;";

	public const string DEF_BAD_REF = "#REF";

	public const string DEF_BAD_REF_UPDATE = "#REF!";

	public const string DEF_BAD_FORMULA = "=#REF!";

	public const int DEF_MAX_COLUMN = 256;

	public const int DEF_MIN_COLUMN = 0;

	[CLSCompliant(false)]
	public const long DEF_MERGE_COD = 10000000000L;

	private Dictionary<long, int> m_mergeStyles = new Dictionary<long, int>();

	private StringBuilder m_builderStart;

	private StringBuilder m_builderEnd;

	private FormulaUtil m_formulaUtil;

	public static long GetUniqueID(int iSheetIndex, long lCellIndex)
	{
		long num = RangeImpl.GetRowFromCellIndex(lCellIndex);
		long num2 = RangeImpl.GetColumnFromCellIndex(lCellIndex);
		return (num << 32) + (num2 << 16) + iSheetIndex;
	}

	public static int GetSheetIndexByUniqueId(long lUniqueId)
	{
		return (int)(lUniqueId & 0xFFFF);
	}

	public static long GetCellIndexByUniqueId(long lUniqueId)
	{
		int firstRow = (int)((lUniqueId >> 32) & 0xFFFFFFFFu);
		return RangeImpl.GetCellIndex((int)((lUniqueId >> 16) & 0xFFFF), firstRow);
	}

	private void SerializeNames(XmlWriter writer, INames names, bool isLocal)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (names == null)
		{
			throw new ArgumentNullException("names");
		}
		int count = names.Count;
		if (count == 0)
		{
			return;
		}
		writer.WriteStartElement("ss", "Names", null);
		for (int i = 0; i < count; i++)
		{
			IName name = names[i];
			if (name.IsLocal == isLocal)
			{
				SerializeName(writer, name);
			}
		}
		writer.WriteEndElement();
	}

	private void SerializeName(XmlWriter writer, IName name)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		writer.WriteStartElement("ss", "NamedRange", null);
		writer.WriteAttributeString("ss", "Name", null, name.Name);
		string refersToR1C = name.RefersToR1C1;
		string text = ((refersToR1C != null && refersToR1C.Length > 0) ? refersToR1C : "=#REF!");
		if (text.IndexOf("#REF") != -1)
		{
			text = "=#REF!";
		}
		writer.WriteAttributeString("ss", "RefersTo", null, text);
		if (!name.Visible)
		{
			writer.WriteAttributeString("ss", "Hidden", null, "1");
		}
		writer.WriteEndElement();
	}

	private void SerializeStyles(XmlWriter writer, ExtendedFormatsCollection extends, List<ExtendedFormatImpl> listToReparse)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (extends == null)
		{
			throw new ArgumentNullException("styles");
		}
		writer.WriteStartElement("ss", "Styles", null);
		int i = 0;
		for (int count = extends.Count; i < count; i++)
		{
			ExtendedFormatImpl extendedFormatImpl = extends[i];
			int parentIndex = extendedFormatImpl.ParentIndex;
			if (extendedFormatImpl.HasParent && parentIndex > extendedFormatImpl.XFormatIndex)
			{
				listToReparse.Add(extendedFormatImpl);
			}
			else
			{
				SerializeStyle(writer, extendedFormatImpl);
			}
		}
		if (listToReparse.Count > 0)
		{
			ReSerializeStyle(writer, listToReparse);
		}
		writer.WriteEndElement();
	}

	private void SerializeStyle(XmlWriter writer, ExtendedFormatImpl format)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		writer.WriteStartElement("ss", "Style", null);
		bool flag = format.HasParent && format.ParentIndex != 0;
		string value = ((format.XFormatIndex == 0) ? "Default" : ("s" + format.XFormatIndex));
		writer.WriteAttributeString("ss", "ID", null, value);
		if (format.XFType == ExtendedFormatRecord.TXFType.XF_CELL && !flag)
		{
			string text = ((StylesCollection)format.Workbook.Styles).GetByXFIndex(format.Index)?.Name;
			if (text != null && text.Length > 0)
			{
				writer.WriteAttributeString("ss", "Name", null, text);
				flag = false;
			}
		}
		if (flag)
		{
			writer.WriteAttributeString("ss", "Parent", null, "s" + format.ParentIndex);
		}
		SerializeStyleElements(writer, format);
		writer.WriteEndElement();
	}

	private void SerializeStyleElements(XmlWriter writer, ExtendedFormatImpl format)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		_ = format.HasParent;
		if (format.IncludeFont)
		{
			SerializeFont(writer, format.Font);
		}
		if (format.IncludeProtection)
		{
			SerializeProtection(writer, format);
		}
		if (format.IncludeAlignment)
		{
			SerializeAlignment(writer, format);
		}
		if (format.IncludeNumberFormat)
		{
			SerializeNumberFormat(writer, format.NumberFormat);
		}
		if (format.IncludePatterns)
		{
			SerializeInterior(writer, format);
		}
		if (format.IncludeBorder)
		{
			SerializeBorders(writer, format.Borders);
		}
	}

	private void SerializeFont(XmlWriter writer, IOfficeFont font)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		writer.WriteStartElement("ss", "Font", null);
		if (font.Bold)
		{
			writer.WriteAttributeString("ss", "Bold", null, "1");
		}
		if (font.FontName != "Arial")
		{
			writer.WriteAttributeString("ss", "FontName", null, font.FontName);
		}
		writer.WriteAttributeString("ss", "Color", null, GetColorString(font.RGBColor));
		if (font.Italic)
		{
			writer.WriteAttributeString("ss", "Italic", null, "1");
		}
		if (font.MacOSOutlineFont)
		{
			writer.WriteAttributeString("ss", "Outline", null, "1");
		}
		if (font.MacOSShadow)
		{
			writer.WriteAttributeString("ss", "Shadow", null, "1");
		}
		if (font.Size != 10.0)
		{
			writer.WriteAttributeString("ss", "Size", null, XmlConvert.ToString(font.Size));
		}
		if (font.Strikethrough)
		{
			writer.WriteAttributeString("ss", "StrikeThrough", null, "1");
		}
		if (font.Underline != 0)
		{
			writer.WriteAttributeString("ss", "Underline", null, font.Underline.ToString());
		}
		string styleFontAlign = GetStyleFontAlign(font);
		if (styleFontAlign != "None")
		{
			writer.WriteAttributeString("ss", "VerticalAlign", null, styleFontAlign);
		}
		writer.WriteEndElement();
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
		writer.WriteStartElement("ss", "Protection", null);
		if (!format.Locked)
		{
			writer.WriteAttributeString("ss", "Protected", null, "0");
		}
		if (format.FormulaHidden)
		{
			writer.WriteAttributeString("x", "HideFormula", null, "1");
		}
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
		writer.WriteStartElement("ss", "Alignment", null);
		if (format.HorizontalAlignment != 0)
		{
			string styleHAlignString = GetStyleHAlignString(format.HorizontalAlignment);
			writer.WriteAttributeString("ss", "Horizontal", null, styleHAlignString);
		}
		if (format.IndentLevel != 0)
		{
			writer.WriteAttributeString("ss", "Indent", null, format.IndentLevel.ToString());
		}
		if (format.ReadingOrder != 0)
		{
			writer.WriteAttributeString("ss", "ReadingOrder", null, format.ReadingOrder.ToString());
		}
		int rotation = format.Rotation;
		if (rotation != 0)
		{
			if (rotation != 255)
			{
				writer.WriteAttributeString("ss", "Rotate", null, ((rotation > 90) ? (90 - rotation) : rotation).ToString());
			}
			else
			{
				writer.WriteAttributeString("ss", "VerticalText", null, "1");
			}
		}
		if (format.ShrinkToFit)
		{
			writer.WriteAttributeString("ss", "ShrinkToFit", null, "1");
		}
		string styleVAlignString = GetStyleVAlignString(format.VerticalAlignment);
		writer.WriteAttributeString("ss", "Vertical", null, styleVAlignString);
		if (format.WrapText)
		{
			writer.WriteAttributeString("ss", "WrapText", null, "1");
		}
		writer.WriteEndElement();
	}

	private void SerializeNumberFormat(XmlWriter writer, string strNumber)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (strNumber == null)
		{
			throw new ArgumentNullException("strNumber");
		}
		writer.WriteStartElement("ss", "NumberFormat", null);
		writer.WriteAttributeString("ss", "Format", null, strNumber);
		writer.WriteEndElement();
	}

	private void SerializeInterior(XmlWriter writer, ExtendedFormatImpl format)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		writer.WriteStartElement("ss", "Interior", null);
		if (!format.IsDefaultColor)
		{
			string colorString = GetColorString(format.Color);
			writer.WriteAttributeString("ss", "Color", null, colorString);
		}
		if (!format.IsDefaultPatternColor)
		{
			string colorString2 = GetColorString(format.PatternColor);
			writer.WriteAttributeString("ss", "PatternColor", null, colorString2);
		}
		if (format.FillPattern != 0)
		{
			string value = DEF_PATTERN_STRING[(int)format.FillPattern];
			writer.WriteAttributeString("ss", "Pattern", null, value);
		}
		writer.WriteEndElement();
	}

	private void SerializeBorders(XmlWriter writer, IBorders borders)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (borders == null)
		{
			throw new ArgumentNullException("borders");
		}
		writer.WriteStartElement("ss", "Borders", null);
		OfficeBordersIndex[] array = new OfficeBordersIndex[6]
		{
			OfficeBordersIndex.DiagonalDown,
			OfficeBordersIndex.DiagonalUp,
			OfficeBordersIndex.EdgeBottom,
			OfficeBordersIndex.EdgeLeft,
			OfficeBordersIndex.EdgeRight,
			OfficeBordersIndex.EdgeTop
		};
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			OfficeBordersIndex officeBordersIndex = array[i];
			IBorder border = borders[officeBordersIndex];
			if (border != null)
			{
				int iBorderIndex = (int)officeBordersIndex;
				SerializeBorder(writer, border, iBorderIndex);
			}
		}
		writer.WriteEndElement();
	}

	private void SerializeBorder(XmlWriter writer, IBorder border, int iBorderIndex)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (border == null)
		{
			throw new ArgumentNullException("border");
		}
		if ((iBorderIndex == 5 || iBorderIndex == 6) && !border.ShowDiagonalLine)
		{
			return;
		}
		writer.WriteStartElement("ss", "Border", null);
		string value = DEF_BORDER_POSITION_STRING[iBorderIndex];
		writer.WriteAttributeString("ss", "Position", null, value);
		string colorString = GetColorString(border.ColorRGB);
		writer.WriteAttributeString("ss", "Color", null, colorString);
		if (border.LineStyle != 0)
		{
			string text = DEF_BORDER_LINE_TYPE_STRING[(int)border.LineStyle];
			int num = text.IndexOf(" ");
			string value2 = ((num != -1) ? text.Substring(num + 1) : text);
			writer.WriteAttributeString("ss", "LineStyle", null, value2);
			if (num != -1)
			{
				writer.WriteAttributeString("ss", "Weight", null, text.Substring(0, num));
			}
		}
		writer.WriteEndElement();
	}

	private void SerializeCell(XmlWriter writer, WorksheetImpl sheet, int iRowIndex)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		CellRecordCollection cellRecords = sheet.CellRecords;
		MergeCellsImpl mergeCells = sheet.MergeCells;
		int num = 0;
		int i = sheet.FirstColumn;
		for (int lastColumn = sheet.LastColumn; i <= lastColumn; i++)
		{
			long cellIndex = RangeImpl.GetCellIndex(i, iRowIndex);
			Rectangle leftTopCell = mergeCells.GetLeftTopCell(new Rectangle(i - 1, iRowIndex - 1, 0, 0));
			bool flag = RangeImpl.GetCellIndex(leftTopCell.X + 1, leftTopCell.Y + 1) == cellIndex;
			MergeCellsRecord.MergedRegion mergedRegion = mergeCells.FindMergedRegion(new Rectangle(i - 1, iRowIndex - 1, 0, 0));
			bool flag2 = cellRecords.Contains(cellIndex) && (mergedRegion == null || flag);
			if (flag2 || flag)
			{
				writer.WriteStartElement("Cell", null);
				if (num + 1 != i)
				{
					writer.WriteAttributeString("ss", "Index", null, i.ToString());
				}
				num = i;
				bool bFormatted = DisableFormatting(writer);
				SerializeCellStyle(writer, cellIndex, flag, cellRecords, sheet);
				SerializeMerge(writer, iRowIndex, i, mergeCells, flag);
				if (flag2 && cellRecords.GetCellRecord(cellIndex).TypeCode != TBIFFRecord.Blank)
				{
					SerializeData(writer, cellRecords, cellIndex);
				}
				writer.WriteEndElement();
				EnableFormatting(writer, bFormatted);
			}
		}
	}

	private void SerializeMerge(XmlWriter writer, int iRowIndex, int i, MergeCellsImpl mergeCells, bool bMerge)
	{
		if (bMerge)
		{
			Rectangle rect = Rectangle.FromLTRB(i - 1, iRowIndex - 1, i - 1, iRowIndex - 1);
			SerializeMergedRange(writer, mergeCells[rect]);
		}
	}

	private void SerializeCellStyle(XmlWriter writer, long index, bool bMerge, CellRecordCollection cells, WorksheetImpl sheet)
	{
		int num = cells.GetExtendedFormatIndex(index);
		if (bMerge)
		{
			long uniqueID = GetUniqueID(sheet.Index, index);
			num = m_mergeStyles[uniqueID];
		}
		WorkbookImpl parentWorkbook = sheet.ParentWorkbook;
		if (num != parentWorkbook.DefaultXFIndex && num != 0 && num != int.MinValue)
		{
			writer.WriteAttributeString("ss", "StyleID", null, "s" + num);
		}
	}

	private void EnableFormatting(XmlWriter writer, bool bFormatted)
	{
		if (bFormatted)
		{
			writer.Settings.Indent = true;
		}
	}

	private bool DisableFormatting(XmlWriter writer)
	{
		bool indent = writer.Settings.Indent;
		writer.Settings.Indent = false;
		return indent;
	}

	private void SerializeData(XmlWriter writer, CellRecordCollection cells, long index)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (cells == null)
		{
			throw new ArgumentNullException("cells");
		}
		NumberFormatInfo numberFormat = CultureInfo.InvariantCulture.NumberFormat;
		string text = cells.GetFormula(index, isR1C1: true, numberFormat);
		IStyle style = null;
		TextWithFormat rtf = null;
		WorksheetImpl worksheetImpl = (WorksheetImpl)cells.Sheet;
		XmlSerializationCellType type;
		string value;
		if (text != null && text.Length > 0)
		{
			text = UpdateFormulaError(text);
			writer.WriteAttributeString("ss", "Formula", null, text);
			type = GetFormulaType(worksheetImpl, index, out value);
		}
		else
		{
			value = GetCellTypeValue(cells, index, out type);
			if (type == XmlSerializationCellType.String)
			{
				style = cells.GetCellStyle(index);
				rtf = worksheetImpl.GetTextWithFormat(index);
			}
		}
		if (text != "=#REF!")
		{
			SerializeData(writer, type, value, style, rtf, cells, index);
		}
	}

	private XmlSerializationCellType GetFormulaType(IWorksheet sheet, long index, out string value)
	{
		int rowFromCellIndex = RangeImpl.GetRowFromCellIndex(index);
		int columnFromCellIndex = RangeImpl.GetColumnFromCellIndex(index);
		IRange range = sheet[rowFromCellIndex, columnFromCellIndex];
		XmlSerializationCellType result;
		if (range.HasFormulaBoolValue)
		{
			result = XmlSerializationCellType.Boolean;
			value = XmlConvert.ToString(range.FormulaBoolValue);
		}
		else if (range.HasFormulaDateTime)
		{
			result = XmlSerializationCellType.DateTime;
			value = XmlConvert.ToString(range.FormulaDateTime, "yyyy-MM-ddTHH:mm:ss");
		}
		else if (range.HasFormulaErrorValue)
		{
			result = XmlSerializationCellType.Error;
			value = range.FormulaErrorValue;
		}
		else if ((value = range.FormulaStringValue) != null)
		{
			result = XmlSerializationCellType.String;
		}
		else
		{
			result = XmlSerializationCellType.Number;
			double formulaNumberValue = range.FormulaNumberValue;
			value = ((!double.IsNaN(formulaNumberValue)) ? XmlConvert.ToString(formulaNumberValue) : null);
		}
		return result;
	}

	private void SerializeData(XmlWriter writer, XmlSerializationCellType cellType, string value, IStyle style, TextWithFormat rtf, CellRecordCollection cells, long cellIndex)
	{
		if (value == null || (cellType == XmlSerializationCellType.String && value.Length == 0))
		{
			return;
		}
		writer.WriteStartElement("Data", null);
		bool flag = false;
		writer.WriteAttributeString("ss", "Type", null, cellType.ToString());
		if (cellType == XmlSerializationCellType.String && value.Length != 0)
		{
			if (style != null && style.IsFirstSymbolApostrophe)
			{
				writer.WriteAttributeString("x", "Ticked", null, "1");
			}
			if (rtf != null && rtf.FormattingRunsCount != 0)
			{
				WorksheetImpl obj = (WorksheetImpl)cells.Sheet;
				writer.WriteAttributeString("xmlns", null, null, "http://www.w3.org/TR/REC-html40");
				cells.GetCellFont(cellIndex);
				_ = obj.ParentWorkbook.InnerFonts;
				flag = true;
			}
		}
		if (!flag)
		{
			if (cellType == XmlSerializationCellType.Boolean)
			{
				value = (XmlConvertExtension.ToBoolean(value) ? "1" : "0");
			}
			writer.WriteString(value);
		}
		writer.WriteEndElement();
	}

	private void SerializeWorksheets(XmlWriter writer, IWorksheets worksheets)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (worksheets == null)
		{
			throw new ArgumentNullException("worksheets");
		}
		int i = 0;
		for (int count = worksheets.Count; i < count; i++)
		{
			SerializeWorksheet(writer, (WorksheetImpl)worksheets[i]);
		}
	}

	private void SerializeWorksheet(XmlWriter writer, WorksheetImpl sheet)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		writer.WriteStartElement("ss", "Worksheet", null);
		writer.WriteAttributeString("ss", "Name", null, sheet.Name);
		if (sheet.IsRightToLeft)
		{
			writer.WriteAttributeString("ss", "RightToLeft", null, "1");
		}
		INames names = sheet.Names;
		if (names.Count > 0)
		{
			SerializeNames(writer, names, isLocal: true);
		}
		SerializeTable(writer, sheet);
		SerializeWorksheetOption(writer, sheet);
		writer.WriteEndElement();
	}

	private void SerializeTable(XmlWriter writer, WorksheetImpl sheet)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		writer.WriteStartElement("ss", "Table", null);
		if (sheet.StandardHeight != 12.75)
		{
			writer.WriteAttributeString("ss", "DefaultRowHeight", null, XmlConvert.ToString(sheet.StandardHeight));
		}
		float num = (float)sheet.Application.ConvertUnits(sheet.ColumnWidthToPixels(sheet.StandardWidth), MeasureUnits.Pixel, MeasureUnits.Point);
		if ((double)num != 48.0)
		{
			writer.WriteAttributeString("ss", "DefaultColumnWidth", null, XmlConvert.ToString(num));
		}
		SerializeColumns(writer, sheet);
		SerializeRows(writer, sheet);
		writer.WriteEndElement();
	}

	private void SerializeColumns(XmlWriter writer, WorksheetImpl worksheet)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (worksheet == null)
		{
			throw new ArgumentNullException("worksheet");
		}
		ColumnInfoRecord[] columnInformation = worksheet.ColumnInformation;
		for (int i = 1; i <= columnInformation.Length - 1; i++)
		{
			ColumnInfoRecord columnInfoRecord = columnInformation[i];
			if (columnInfoRecord == null)
			{
				continue;
			}
			int num = columnInfoRecord.FirstColumn + 1;
			if (num <= 256)
			{
				writer.WriteStartElement("ss", "Column", null);
				writer.WriteAttributeString("ss", "Index", null, num.ToString());
				float num2 = worksheet.ColumnWidthToPixels((double)(int)columnInfoRecord.ColumnWidth / 256.0);
				int num3 = columnInfoRecord.LastColumn - columnInfoRecord.FirstColumn;
				SerializeRowColumnCommonAttributes(writer, columnInfoRecord, columnInfoRecord.LastColumn, worksheet.ParentWorkbook);
				if (num3 > 0)
				{
					writer.WriteAttributeString("ss", "Span", null, num3.ToString());
				}
				writer.WriteAttributeString("ss", "AutoFitWidth", null, "0");
				if ((double)num2 != worksheet.StandardWidth)
				{
					num2 = (float)worksheet.Application.ConvertUnits(num2, MeasureUnits.Pixel, MeasureUnits.Point);
					writer.WriteAttributeString("ss", "Width", null, XmlConvert.ToString(num2));
				}
				writer.WriteEndElement();
			}
		}
	}

	private void SerializeRows(XmlWriter writer, WorksheetImpl sheet)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		CellRecordCollection cellRecords = sheet.CellRecords;
		int num = 0;
		int i = sheet.FirstRow;
		for (int lastRow = sheet.LastRow; i <= lastRow; i++)
		{
			if (cellRecords.ContainsRow(i - 1))
			{
				writer.WriteStartElement("Row");
				if (num + 1 != i)
				{
					writer.WriteAttributeString("ss", "Index", null, i.ToString());
				}
				num = i;
				RowStorage rowStorage = cellRecords.Table.Rows[i - 1];
				if (rowStorage != null)
				{
					writer.WriteAttributeString("ss", "Height", null, XmlConvert.ToString((double)(int)rowStorage.Height / 20.0));
					writer.WriteAttributeString("ss", "AutoFitHeight", null, "0");
					SerializeRowColumnCommonAttributes(writer, rowStorage, rowStorage.LastColumn, sheet.ParentWorkbook);
				}
				SerializeCell(writer, sheet, i);
				writer.WriteEndElement();
			}
		}
	}

	private void SerializeRowColumnCommonAttributes(XmlWriter writer, IOutline record, int iLastIndex, WorkbookImpl book)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		int extendedFormatIndex = record.ExtendedFormatIndex;
		if (extendedFormatIndex != book.DefaultXFIndex && extendedFormatIndex != 0)
		{
			writer.WriteAttributeString("ss", "StyleID", null, "s" + record.ExtendedFormatIndex);
		}
		if (record.IsHidden || record.IsCollapsed)
		{
			writer.WriteAttributeString("ss", "Hidden", null, "1");
		}
	}

	private void SerializeMergedRange(XmlWriter writer, MergeCellsRecord.MergedRegion region)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (region == null)
		{
			throw new ArgumentNullException("region");
		}
		int num = region.RowTo - region.RowFrom;
		int num2 = region.ColumnTo - region.ColumnFrom;
		writer.WriteAttributeString("ss", "MergeDown", null, num.ToString());
		writer.WriteAttributeString("ss", "MergeAcross", null, num2.ToString());
	}

	private void SerializeWorksheetOption(XmlWriter writer, WorksheetImpl sheet)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		writer.WriteStartElement("x", "WorksheetOptions", null);
		if (sheet.Visibility != 0)
		{
			int visibility = (int)sheet.Visibility;
			WriteElement(writer, "x", "Visible", DEF_VISIBLE_STRING[visibility]);
		}
		SerializeWindowTwoProperties(writer, sheet);
		PageSetupImpl pageSetupImpl = (PageSetupImpl)sheet.PageSetup;
		if (pageSetupImpl.IsFitToPage)
		{
			WriteElement(writer, "x", "FitToPage");
		}
		int tabColor = (int)sheet.TabColor;
		if (tabColor != -1)
		{
			WriteElement(writer, "x", "TabColorIndex", tabColor.ToString());
		}
		if (sheet.Zoom != 100)
		{
			WriteElement(writer, "x", "Zoom", sheet.Zoom.ToString());
		}
		SerializePageSetup(writer, pageSetupImpl);
		SerializePanes(writer, sheet);
		if (!pageSetupImpl.IsNotValidSettings)
		{
			SerializePrint(writer, pageSetupImpl);
		}
		writer.WriteEndElement();
	}

	private void SerializePageSetup(XmlWriter writer, IPageSetup pageSetup)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (pageSetup == null)
		{
			throw new ArgumentNullException("pageSetup");
		}
		PageSetupImpl pageSetup2 = (PageSetupImpl)pageSetup;
		writer.WriteStartElement("x", "PageSetup", null);
		SerializeHeaderFooter(writer, pageSetup2, isFooter: true);
		SerializeHeaderFooter(writer, pageSetup2, isFooter: false);
		SerializeLayout(writer, pageSetup2);
		SerializePageMargins(writer, pageSetup2);
		writer.WriteEndElement();
	}

	private void SerializeHeaderFooter(XmlWriter writer, PageSetupImpl pageSetup, bool isFooter)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (pageSetup == null)
		{
			throw new ArgumentNullException("pageSetup");
		}
		double num = (isFooter ? pageSetup.FooterMargin : pageSetup.HeaderMargin);
		bool num2 = num != 0.5;
		string text = (isFooter ? pageSetup.FullFooterString : pageSetup.FullHeaderString);
		bool flag = text != null && text.Length > 0;
		if (num2 || flag)
		{
			string localName = (isFooter ? "Footer" : "Header");
			writer.WriteStartElement("x", localName, null);
			writer.WriteAttributeString("x", "Margin", null, XmlConvert.ToString(num));
			if (flag)
			{
				writer.WriteAttributeString("x", "Data", null, text);
			}
			writer.WriteEndElement();
		}
	}

	private void SerializeLayout(XmlWriter writer, PageSetupImpl pageSetup)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (pageSetup == null)
		{
			throw new ArgumentNullException("pageSetup");
		}
		OfficePageOrientation orientation = pageSetup.Orientation;
		writer.WriteStartElement("x", "Layout", null);
		if (!pageSetup.AutoFirstPageNumber)
		{
			writer.WriteAttributeString("x", "StartPageNumber", null, pageSetup.FirstPageNumber.ToString());
		}
		if (orientation != OfficePageOrientation.Portrait)
		{
			writer.WriteAttributeString("x", "Orientation", null, orientation.ToString());
		}
		if (pageSetup.CenterHorizontally)
		{
			writer.WriteAttributeString("x", "CenterHorizontal", null, "1");
		}
		if (pageSetup.CenterVertically)
		{
			writer.WriteAttributeString("x", "CenterVertical", null, "1");
		}
		writer.WriteEndElement();
	}

	private void SerializePageMargins(XmlWriter writer, PageSetupImpl pageSetup)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (pageSetup == null)
		{
			throw new ArgumentNullException("pageSetup");
		}
		writer.WriteStartElement("x", "PageMargins", null);
		double rightMargin = pageSetup.RightMargin;
		double leftMargin = pageSetup.LeftMargin;
		double topMargin = pageSetup.TopMargin;
		double bottomMargin = pageSetup.BottomMargin;
		if (rightMargin != 0.75)
		{
			writer.WriteAttributeString("x", "Right", null, XmlConvert.ToString(rightMargin));
		}
		if (leftMargin != 0.75)
		{
			writer.WriteAttributeString("x", "Left", null, XmlConvert.ToString(leftMargin));
		}
		if (bottomMargin != 1.0)
		{
			writer.WriteAttributeString("x", "Bottom", null, XmlConvert.ToString(bottomMargin));
		}
		if (topMargin != 1.0)
		{
			writer.WriteAttributeString("x", "Top", null, XmlConvert.ToString(topMargin));
		}
		writer.WriteEndElement();
	}

	private void SerializePanes(XmlWriter writer, WorksheetImpl sheet)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		PaneRecord pane = sheet.Pane;
		List<SelectionRecord> selections = sheet.Selections;
		if (selections != null)
		{
			if (selections.Count > 1)
			{
				WriteElement(writer, "x", "ActivePane", pane.ActivePane.ToString());
			}
			SerializeSelectionPane(writer, selections);
		}
		if (pane != null)
		{
			int horizontalSplit = pane.HorizontalSplit;
			int verticalSplit = pane.VerticalSplit;
			if (sheet.WindowTwo.TopRow != 0)
			{
				WriteElement(writer, "x", "TopRowVisible", sheet.WindowTwo.TopRow.ToString());
			}
			if (horizontalSplit != 0)
			{
				WriteElement(writer, "x", "SplitHorizontal", horizontalSplit.ToString());
				WriteElement(writer, "x", "TopRowBottomPane", pane.FirstRow.ToString());
			}
			if (verticalSplit != 0)
			{
				WriteElement(writer, "x", "SplitVertical", verticalSplit.ToString());
				WriteElement(writer, "x", "LeftColumnRightPane", pane.FirstColumn.ToString());
			}
			WindowTwoRecord windowTwo = sheet.WindowTwo;
			if (windowTwo.IsFreezePanes)
			{
				WriteElement(writer, "x", "FreezePanes");
			}
			if (windowTwo.IsFreezePanesNoSplit)
			{
				WriteElement(writer, "x", "FrozenNoSplit");
			}
		}
	}

	private void SerializeSelectionPane(XmlWriter writer, List<SelectionRecord> arrSelection)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (arrSelection == null)
		{
			throw new ArgumentNullException("arrSelection");
		}
		int count = arrSelection.Count;
		if (count > 4)
		{
			throw new ArgumentOutOfRangeException("Array cannot contain more than 4 selection records");
		}
		writer.WriteStartElement("x", "Panes", null);
		for (int i = 0; i < count; i++)
		{
			SelectionRecord selectionRecord = arrSelection[i];
			writer.WriteStartElement("x", "Pane", null);
			WriteElement(writer, "x", "Number", selectionRecord.Pane.ToString());
			if (selectionRecord.ColumnActiveCell != 0)
			{
				WriteElement(writer, "x", "ActiveCol", selectionRecord.ColumnActiveCell.ToString());
			}
			if (selectionRecord.RowActiveCell != 0)
			{
				WriteElement(writer, "x", "ActiveRow", selectionRecord.RowActiveCell.ToString());
			}
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	private void SerializePrint(XmlWriter writer, IPageSetup pageSetup)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (pageSetup == null)
		{
			throw new ArgumentNullException("pageSetup");
		}
		int paperSize = (int)pageSetup.PaperSize;
		writer.WriteStartElement("x", "Print", null);
		if (pageSetup.Copies != 1)
		{
			WriteElement(writer, "x", "NumberofCopies", pageSetup.Copies.ToString());
		}
		if (pageSetup.PrintQuality <= 32767)
		{
			WriteElement(writer, "x", "HorizontalResolution", pageSetup.PrintQuality.ToString());
		}
		if (pageSetup.PaperSize != OfficePaperSize.PaperLetter)
		{
			WriteElement(writer, "x", "PaperSizeIndex", paperSize.ToString());
		}
		if (pageSetup.IsFitToPage)
		{
			if (pageSetup.FitToPagesWide != 1)
			{
				WriteElement(writer, "x", "FitWidth", pageSetup.FitToPagesWide.ToString());
			}
			if (pageSetup.FitToPagesTall != 1)
			{
				WriteElement(writer, "x", "FitHeight", pageSetup.FitToPagesTall.ToString());
			}
		}
		else if (pageSetup.Zoom != 100)
		{
			WriteElement(writer, "x", "Scale", pageSetup.Zoom.ToString());
		}
		if (pageSetup.PrintGridlines)
		{
			WriteElement(writer, "x", "Gridlines");
		}
		if (pageSetup.BlackAndWhite)
		{
			WriteElement(writer, "x", "BlackAndWhite");
		}
		if (pageSetup.Draft)
		{
			WriteElement(writer, "x", "DraftQuality");
		}
		if (pageSetup.PrintHeadings)
		{
			WriteElement(writer, "x", "RowColHeadings");
		}
		if (pageSetup.PrintComments != OfficePrintLocation.PrintNoComments)
		{
			int printComments = (int)pageSetup.PrintComments;
			WriteElement(writer, "x", "CommentsLayout", DEF_PRINT_LOCATION_STRING[printComments]);
		}
		if (pageSetup.PrintErrors != 0)
		{
			int printErrors = (int)pageSetup.PrintErrors;
			WriteElement(writer, "x", "PrintErrors", DEF_PRINT_ERROR_STRING[printErrors]);
		}
		if (pageSetup.Order == OfficeOrder.OverThenDown)
		{
			WriteElement(writer, "x", "LeftToRight");
		}
		writer.WriteEndElement();
	}

	private void SerializeWindowTwoProperties(XmlWriter writer, WorksheetImpl sheet)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		WindowTwoRecord windowTwo = sheet.WindowTwo;
		if (windowTwo != null)
		{
			if (!windowTwo.IsDisplayGridlines)
			{
				WriteElement(writer, "x", "DoNotDisplayGridlines");
			}
			if (!windowTwo.IsDisplayRowColHeadings)
			{
				WriteElement(writer, "x", "DoNotDisplayHeadings");
			}
			if (windowTwo.IsSelected)
			{
				WriteElement(writer, "x", "Selected");
			}
		}
	}

	private void SerializeExcelWorkbook(XmlWriter writer, WorkbookImpl book)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (book == null)
		{
			throw new ArgumentNullException("book");
		}
		writer.WriteStartElement("x", "ExcelWorkbook", null);
		int index = book.ActiveSheet.Index;
		int count = book.WorksheetGroup.Count;
		if (index > 0)
		{
			WriteElement(writer, "x", "ActiveSheet", index.ToString());
			WriteElement(writer, "x", "FirstVisibleSheet", book.DisplayedTab.ToString());
		}
		if (count > 1)
		{
			WriteElement(writer, "x", "SelectedSheets", count.ToString());
		}
		writer.WriteEndElement();
	}

	private void SerializeDocumentProperties(XmlWriter writer, IWorkbook book)
	{
		throw new NotImplementedException();
	}

	private void SerializeWorkbook(XmlWriter writer, IWorkbook book)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (book == null)
		{
			throw new ArgumentNullException("book");
		}
		_ = (WorkbookImpl)book;
		m_mergeStyles.Clear();
		writer.WriteStartElement("Workbook", null);
		writer.WriteAttributeString("xmlns", null, null, "urn:schemas-microsoft-com:office:spreadsheet");
		writer.WriteAttributeString("xmlns", "ss", null, "urn:schemas-microsoft-com:office:spreadsheet");
		writer.WriteAttributeString("xmlns", "x", null, "urn:schemas-microsoft-com:office:excel");
		writer.WriteAttributeString("xmlns", "o", null, "urn:schemas-microsoft-com:office:office");
		writer.WriteAttributeString("xmlns", "html", null, "http://www.w3.org/TR/REC-html40");
		List<ExtendedFormatImpl> mergedList = GetMergedList(book.Worksheets);
		if (book.Styles.Count > 0)
		{
			SerializeStyles(writer, ((WorkbookImpl)book).InnerExtFormats, mergedList);
		}
		SerializeExcelWorkbook(writer, (WorkbookImpl)book);
		SerializeNames(writer, book.Names, isLocal: false);
		SerializeWorksheets(writer, book.Worksheets);
		m_mergeStyles.Clear();
		writer.WriteEndElement();
	}

	public void Serialize(XmlWriter writer, IWorkbook book)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (book == null)
		{
			throw new ArgumentNullException("book");
		}
		writer.WriteRaw("<?xml version=\"1.0\"?>");
		writer.WriteRaw("<?mso-application progid=\"Excel.Sheet\"?>");
		SerializeWorkbook(writer, book);
	}

	private StringBuilder GetStartBuilder()
	{
		return InitializeBuilder(ref m_builderStart);
	}

	private StringBuilder GetEndBuilder()
	{
		return InitializeBuilder(ref m_builderEnd);
	}

	private StringBuilder InitializeBuilder(ref StringBuilder builder)
	{
		if (builder == null)
		{
			builder = new StringBuilder();
		}
		else
		{
			builder.Length = 0;
		}
		return builder;
	}

	private string GetColorString(Color col)
	{
		return "#" + (col.ToArgb() & 0xFFFFFF).ToString("X6");
	}

	private void AddTagToString(string strOpenTag, string strCloseTag, StringBuilder builderStart, StringBuilder builderEnd)
	{
		if (builderStart == null)
		{
			throw new ArgumentNullException("builderStart");
		}
		if (builderEnd == null)
		{
			throw new ArgumentNullException("builderEnd");
		}
		builderStart.Append(strOpenTag);
		builderEnd.Insert(0, strCloseTag);
	}

	private void AddAttributeToString(string name, string value, StringBuilder builderStart, StringBuilder builderEnd)
	{
		if (builderStart == null)
		{
			throw new ArgumentNullException("builderStart");
		}
		if (builderEnd == null)
		{
			throw new ArgumentNullException("builderEnd");
		}
		builderStart.Append(" ");
		builderStart.Append(name);
		builderStart.Append("=\"");
		builderStart.Append(value);
		builderStart.Append("\" ");
	}

	private string GetBorderString(string strBorder, Color borderCol, OfficeLineStyle style)
	{
		return string.Concat(string.Concat("border-" + strBorder + ":", " ", DEF_BORDER_LINE_CF[(int)style]), " ", GetColorString(borderCol), ";");
	}

	private string GetStyleHAlignString(OfficeHAlign hAlign)
	{
		if (hAlign == OfficeHAlign.HAlignGeneral)
		{
			return "Automatic";
		}
		return hAlign.ToString().Remove(0, 6);
	}

	private string GetStyleVAlignString(OfficeVAlign vAlign)
	{
		return vAlign.ToString().Remove(0, 6);
	}

	private string GetStyleFontAlign(IOfficeFont font)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		string result = "None";
		if (font.Subscript)
		{
			result = "Subscript";
		}
		if (font.Superscript)
		{
			result = "Superscript";
		}
		return result;
	}

	private string GetAutoFilterConditionValue(IAutoFilterCondition cond)
	{
		if (cond == null)
		{
			throw new ArgumentNullException("cond");
		}
		if (cond.DataType == OfficeFilterDataType.String)
		{
			return cond.String;
		}
		if (cond.DataType == OfficeFilterDataType.FloatingPoint)
		{
			return cond.Double.ToString();
		}
		if (cond.DataType == OfficeFilterDataType.ErrorCode)
		{
			return cond.ErrorCode.ToString();
		}
		if (cond.DataType == OfficeFilterDataType.Boolean)
		{
			if (!cond.Boolean)
			{
				return "0";
			}
			return "1";
		}
		throw new ArgumentException("Unassigned conditonal type");
	}

	private string GetCellTypeValue(CellRecordCollection cells, long index, out XmlSerializationCellType type)
	{
		if (cells == null)
		{
			throw new ArgumentNullException("cell");
		}
		string text = cells.GetText(index);
		if (text != null)
		{
			type = XmlSerializationCellType.String;
			return text;
		}
		double numberWithoutFormula = cells.GetNumberWithoutFormula(index);
		if (numberWithoutFormula != double.MinValue)
		{
			type = XmlSerializationCellType.Number;
			return XmlConvert.ToString(numberWithoutFormula);
		}
		DateTime dateTime = cells.GetDateTime(index);
		if (dateTime != DateTime.MinValue)
		{
			type = XmlSerializationCellType.DateTime;
			return XmlConvert.ToString(dateTime, "yyyy-MM-ddTHH:mm:ss");
		}
		string error = cells.GetError(index);
		if (error != null)
		{
			type = XmlSerializationCellType.Error;
			return error;
		}
		if (cells.GetBool(index, out var value))
		{
			type = XmlSerializationCellType.Boolean;
			if (!value)
			{
				return "0";
			}
			return "1";
		}
		throw new ApplicationException("Cell dosn't contain value");
	}

	private void WriteElement(XmlWriter writer, string strPrefix, string strName)
	{
		WriteElement(writer, strPrefix, strName, null);
	}

	private void WriteElement(XmlWriter writer, string strPrefix, string strName, string strValue)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (strName == null || strName.Length == 0)
		{
			throw new ArgumentNullException("strName");
		}
		if (strPrefix == null || strPrefix.Length == 0)
		{
			throw new ArgumentNullException("strPrefix");
		}
		writer.WriteStartElement(strPrefix, strName, null);
		if (strValue != null)
		{
			writer.WriteString(strValue);
		}
		writer.WriteEndElement();
	}

	private void ReSerializeStyle(XmlWriter writer, List<ExtendedFormatImpl> list)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			SerializeStyle(writer, list[i]);
		}
	}

	private List<ExtendedFormatImpl> GetMergedList(IWorksheets sheets)
	{
		List<ExtendedFormatImpl> list = new List<ExtendedFormatImpl>();
		if (sheets == null)
		{
			throw new ArgumentNullException("sheets");
		}
		int i = 0;
		for (int count = sheets.Count; i < count; i++)
		{
			WorksheetImpl worksheetImpl = (WorksheetImpl)sheets[i];
			MergeCellsImpl mergeCells = worksheetImpl.MergeCells;
			IList<ExtendedFormatImpl> mergedExtendedFormats = mergeCells.GetMergedExtendedFormats();
			int j = 0;
			for (int count2 = mergedExtendedFormats.Count; j < count2; j++)
			{
				ExtendedFormatImpl extendedFormatImpl = mergedExtendedFormats[j];
				Rectangle rectangle = mergeCells[j];
				long key = GetUniqueID(lCellIndex: RangeImpl.GetCellIndex(rectangle.X + 1, rectangle.Y + 1), iSheetIndex: worksheetImpl.Index);
				int num = 5000 + m_mergeStyles.Count;
				extendedFormatImpl.Index = (ushort)num;
				m_mergeStyles.Add(key, num);
			}
			list.AddRange(mergedExtendedFormats);
		}
		return list;
	}

	private string UpdateFormulaError(string strFormula)
	{
		if (strFormula == null || strFormula.Length == 0)
		{
			throw new ArgumentNullException("strFormula");
		}
		if (!strFormula.EndsWith("#REF"))
		{
			return strFormula;
		}
		return "=#REF!";
	}

	private string ConvertDataValidationType(ExcelDataType dataValidationType)
	{
		if (dataValidationType <= ExcelDataType.Any)
		{
			return DEF_ALLOWTYPE_STRING[0];
		}
		return DEF_ALLOWTYPE_STRING[(int)dataValidationType];
	}

	private string ConvertDataValidationErrorStyle(ExcelErrorStyle strErrorStyle)
	{
		if (strErrorStyle <= ExcelErrorStyle.Stop)
		{
			return DEF_ERRORSTYLE[0];
		}
		return DEF_ERRORSTYLE[(int)strErrorStyle];
	}
}
