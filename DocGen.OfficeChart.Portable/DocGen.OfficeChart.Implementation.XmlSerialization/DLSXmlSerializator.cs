using System;
using System.Globalization;
using System.Xml;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Interfaces.XmlSerialization;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation.XmlSerialization;

[XmlSerializator(OfficeXmlSaveType.DLS)]
internal class DLSXmlSerializator : IXmlSerializator
{
	private const string DEF_DLS_START = "DLS";

	private const string DEF_PROTECTION_ATTRIBUTE = "ProtectionType";

	private const string DEF_PROTECTION_VALUE = "NoProtection";

	private const string DEF_STYLES_START = "styles";

	private const string DEF_STYLE_START = "style";

	private const string DEF_ID_ATTRIBUTE = "id";

	private const string DEF_NAME_ATTRIBUTE = "Name";

	private const string DEF_TYPE_ATTRIBUTE = "type";

	private const string DEF_SECTIONS_START = "sections";

	private const string DEF_SECTION_START = "section";

	private const string DEF_BREAK_CODE_ATTRIBUTE = "BreakCode";

	private const string DEF_PARAGRAPHS_START = "paragraphs";

	private const string DEF_PARAGRAPH_START = "paragraph";

	private const string DEF_ITEMS_START = "items";

	private const string DEF_ITEM_START = "item";

	private const string DEF_ROWS_START = "rows";

	private const string DEF_ROW_START = "row";

	private const string DEF_CELLS_START = "cells";

	private const string DEF_CELL_START = "cell";

	private const string DEF_WIDTH_ATTRIBUTE = "Width";

	private const string DEF_TEXT_RANGE_ATTRIBUTE = "TextRange";

	private const string DEF_TEXT_START = "text";

	private const string DEF_COLUMNS_COUNT_ATTRIBUTE = "ColumnsCount";

	private const string DEF_FORMAT_START = "format";

	private const string DEF_FONT_NAME_ATTRIBUTE = "FontName";

	private const string DEF_FONT_SIZE_ATTRIBUTE = "FontSize";

	private const string DEF_BOLD_ATTRIBUTE = "Bold";

	private const string DEF_ITALIC_ATTRIBUTE = "Italic";

	private const string DEF_UNDERLINE_ATTRIBUTE = "Underline";

	private const string DEF_TEXT_COLOR_ATTRIBUTE = "TextColor";

	private const string DEF_COLOR_PREFIX = "#";

	private const string DEF_UNDERLINE_NONE = "None";

	private const string DEF_UNDERLINE_SINGLE = "Single";

	private const string DEF_UNDERLINE_DOUBLE = "Double";

	private const string DEF_SUBCRIPT = "SubScript";

	private const string DEF_SUPSCRIPT = "SuperScript";

	private const string DEF_NO_SUBSUPERSCIRPT = "None";

	private const string DEF_SUBSUPERSCRIPT_ATTRIBUTE = "SubSuperScript";

	private const string DEF_STRIKEOUT_ATTRIBUTE = "Strike";

	private static readonly string DEF_TRUE_STRING = bool.TrueString.ToLower();

	private const string DEF_TABLE_FORMAT_START = "cell-format";

	private const string DEF_CHARACTER_FORMAT_START = "character-format";

	private const string DEF_BORDERS_START = "borders";

	private const string DEF_BORDER_START = "border";

	private const string DEF_COLOR_ATTRIBUTE = "Color";

	private const string DEF_LINE_WIDTH_ATTRIBUTE = "LineWidth";

	private const string DEF_BORDER_TYPE_ATTRIBUTE = "BorderType";

	private const string DEF_BORDER_WIDTH_NONE = "0";

	private const string DEF_BORDER_TYPE_SIGNLE = "Single";

	private const string DEF_BORDER_TYPE_DOUBLE = "Double";

	private const string DEF_BORDER_TYPE_DOT = "Dot";

	private const string DEF_BORDER_TYPE_DASH_SMALL = "DashSmallGap";

	private const string DEF_BORDER_TYPE_DOT_DASH = "DotDash";

	private const string DEF_BORDER_TYPE_DOT_DOT_DASH = "DotDotDash";

	private const string DEF_BORDER_TYPE_THICK = "Thick";

	private const string DEF_BORDER_TYPE_NONE = "None";

	private const string DEF_PAGE_SETTINGS_START = "page-setup";

	private const string DEF_PAGE_HEIGHT_ATTRIBUTE = "PageHeight";

	private const string DEF_PAGE_WIDTH_ATTRIBUTE = "PageWidth";

	private const string DEF_FOOTER_DISTANCE_ATTRIBUTE = "FooterDistance";

	private const string DEF_HEADER_DISTANCE_ATTRIBUTE = "HeaderDistance";

	private const string DEF_TOP_MARGIN_ATTRIBUTE = "TopMargin";

	private const string DEF_BOTTOM_MARGIN_ATTRIBUTE = "BottomMargin";

	private const string DEF_LEFT_MARGIN_ATTRIBUTE = "LeftMargin";

	private const string DEF_RIGHT_MARGIN_ATTRIBUTE = "RightMargin";

	private const string DEF_PAGE_BREAK_AFTER_ATTRIBUTE = "PageBreakAfter";

	private const string DEF_ORIENTATION_ATTRIBUTE = "Orientation";

	private const string DEF_PARAGRAPH_FORMAT_START = "paragraph-format";

	private const string DEF_HEADERS_FOOTERS_START = "headers-footers";

	private const string DEF_ITEM_TYPE_TABLE = "Table";

	private const string DEF_EVEN_FOOTER_START = "even-footer";

	private const string DEF_ODD_FOOTER_START = "odd-footer";

	private const string DEF_EVEN_HEADER_START = "even-header";

	private const string DEF_ODD_HEADER_START = "odd-header";

	private const string DEF_ROW_HEIGHT_ATTRIBUTE = "RowHeight";

	private const string DEF_TABLE_SHADOW_COLOR_ATTRIBUTE = "ShadingColor";

	private const int DEF_BORDER_WIDTH = 1;

	private const string DEF_HALIGNMENT_ATTRIBUTE = "HrAlignment";

	private const string DEF_VALIGNMENT_ATTRIBUTE = "VAlignment";

	private const string DEF_ALIGN_CENTER = "Center";

	private const string DEF_ALIGN_TOP = "Top";

	private const string DEF_ALIGN_BOTTOM = "Bottom";

	private const string DEF_ALIGN_MIDDLE = "Middle";

	private const string DEF_ALIGN_LEFT = "Left";

	private const string DEF_ALIGN_RIGHT = "Right";

	private const string DEF_ALIGN_JUSTIFY = "Justify";

	private static readonly OfficeBordersIndex[] DEF_DLS_BORDERS = new OfficeBordersIndex[4]
	{
		OfficeBordersIndex.EdgeBottom,
		OfficeBordersIndex.EdgeLeft,
		OfficeBordersIndex.EdgeRight,
		OfficeBordersIndex.EdgeTop
	};

	private static readonly string[] DEF_DLS_BORDER_NAMES = new string[4] { "Bottom", "Left", "Right", "Top" };

	private static readonly CultureInfo DLSCulture = CultureInfo.InvariantCulture;

	private static readonly string DEF_BORDER_WIDTH_HAIR = 0.25.ToString(DLSCulture);

	private static readonly string DEF_BORDER_WIDTH_THIN = 0.5.ToString(DLSCulture);

	private static readonly string DEF_BORDER_WIDTH_MEDIUM = 1.ToString(DLSCulture);

	private static readonly string DEF_BORDER_WIDTH_THICK = 2.25.ToString(DLSCulture);

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
		writer.WriteStartElement("DLS");
		writer.WriteAttributeString("ProtectionType", "NoProtection");
		SerializeStyles(writer, book);
		SerializeDocumentProperties(writer, book);
		SerializeSections(writer, book);
		writer.WriteEndElement();
	}

	private void SerializeStyles(XmlWriter writer, IWorkbook book)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (book == null)
		{
			throw new ArgumentNullException("book");
		}
		writer.WriteStartElement("styles");
		writer.WriteStartElement("style");
		writer.WriteAttributeString("id", "0");
		writer.WriteAttributeString("Name", "Normal");
		writer.WriteAttributeString("type", "ParagraphStyle");
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	private void SerializeDocumentProperties(XmlWriter writer, IWorkbook book)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (book == null)
		{
			throw new ArgumentNullException("book");
		}
	}

	private void SerializeSections(XmlWriter writer, IWorkbook book)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (book == null)
		{
			throw new ArgumentNullException("book");
		}
		writer.WriteStartElement("sections");
		IWorksheets worksheets = book.Worksheets;
		int i = 0;
		for (int count = worksheets.Count; i < count; i++)
		{
			SerializeWorksheet(writer, book.Worksheets[i]);
		}
		writer.WriteEndElement();
	}

	private void SerializeWorksheet(XmlWriter writer, IWorksheet sheet)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (!((WorksheetImpl)sheet).IsEmpty)
		{
			writer.WriteStartElement("section");
			writer.WriteAttributeString("BreakCode", "NewPage");
			SerializePageSettings(writer, (PageSetupImpl)sheet.PageSetup);
			double dPageWidth = SerializeParagraphs(writer, sheet);
			SerializeHeaderFooter(writer, sheet, dPageWidth);
			writer.WriteEndElement();
		}
	}

	private void SerializePageSettings(XmlWriter writer, PageSetupImpl pageSetup)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (pageSetup == null)
		{
			throw new ArgumentNullException("pageSetup");
		}
		writer.WriteStartElement("page-setup");
		WriteAttribute(writer, "PageHeight", pageSetup.PageHeight);
		WriteAttribute(writer, "PageWidth", pageSetup.PageWidth);
		WriteAttribute(writer, "FooterDistance", pageSetup.FooterMargin, MeasureUnits.Inch);
		WriteAttribute(writer, "HeaderDistance", pageSetup.FooterMargin, MeasureUnits.Inch);
		WriteAttribute(writer, "TopMargin", pageSetup.FooterMargin, MeasureUnits.Inch);
		WriteAttribute(writer, "BottomMargin", pageSetup.BottomMargin, MeasureUnits.Inch);
		WriteAttribute(writer, "LeftMargin", pageSetup.LeftMargin, MeasureUnits.Inch);
		WriteAttribute(writer, "RightMargin", pageSetup.RightMargin, MeasureUnits.Inch);
		writer.WriteAttributeString("Orientation", pageSetup.Orientation.ToString());
		writer.WriteEndElement();
	}

	private double SerializeParagraphs(XmlWriter writer, IWorksheet sheet)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		writer.WriteStartElement("paragraphs");
		INames names = sheet.Names;
		IRange range = (names.Contains(PageSetupImpl.DEF_AREA_XlS) ? names[PageSetupImpl.DEF_AREA_XlS].RefersToRange : sheet.UsedRange);
		MeasurePageArea(range, out var pageSize, out var headingsSize);
		OfficeOrder order = sheet.PageSetup.Order;
		if (range is RangesCollection)
		{
			RangesCollection rangesCollection = (RangesCollection)range;
			int i = 0;
			for (int count = rangesCollection.Count; i < count; i++)
			{
				IRange range2 = rangesCollection[i];
				SerializeRange(writer, range2, pageSize, headingsSize, order);
			}
		}
		else
		{
			SerializeRange(writer, range, pageSize, headingsSize, order);
		}
		writer.WriteEndElement();
		return pageSize.Width;
	}

	private void MeasurePageArea(IRange printArea, out SizeF pageSize, out SizeF headingsSize)
	{
		pageSize = new SizeF(0f, 0f);
		headingsSize = new SizeF(0f, 0f);
		if (printArea == null)
		{
			throw new ArgumentNullException("printArea");
		}
		IWorksheet worksheet = printArea.Worksheet;
		PageSetupImpl pageSetupImpl = (PageSetupImpl)worksheet.PageSetup;
		double value = pageSetupImpl.LeftMargin + pageSetupImpl.RightMargin;
		value = ApplicationImpl.ConvertUnitsStatic(value, MeasureUnits.Inch, MeasureUnits.Point);
		pageSize.Width = (float)(pageSetupImpl.PageWidth - value);
		value = pageSetupImpl.TopMargin + pageSetupImpl.BottomMargin;
		value = ApplicationImpl.ConvertUnitsStatic(value, MeasureUnits.Inch, MeasureUnits.Point);
		pageSize.Height = (float)(pageSetupImpl.PageHeight - value);
		_ = pageSetupImpl.Order;
		if (pageSetupImpl.PrintHeadings)
		{
			FontImpl wrapped = ((FontWrapper)worksheet.Workbook.Styles["Normal"].Font).Wrapped;
			string strValue = printArea.LastRow.ToString();
			headingsSize.Width = wrapped.MeasureString(strValue).Width;
			headingsSize.Width = (float)ApplicationImpl.ConvertUnitsStatic(headingsSize.Width, MeasureUnits.Pixel, MeasureUnits.Point);
			headingsSize.Height = (float)worksheet.StandardHeight;
			pageSize.Width -= headingsSize.Width;
			pageSize.Height -= headingsSize.Height;
		}
	}

	private void SerializeRange(XmlWriter writer, IRange range, SizeF pageSize, SizeF headingsSize, OfficeOrder pageOrder)
	{
		int iFirstRow = -1;
		int iFirstCol = -1;
		int iLastRow = -1;
		int iLastCol = -1;
		int iRowId = -1;
		IWorksheet worksheet = range.Worksheet;
		_ = worksheet.PageSetup;
		int num = 0;
		while (FillNextPage(range, ref iFirstRow, ref iFirstCol, ref iLastRow, ref iLastCol, pageSize, pageOrder))
		{
			int num2 = iLastCol - iFirstCol + 1;
			if (headingsSize.Width > 0f)
			{
				num2++;
			}
			writer.WriteStartElement("paragraph");
			writer.WriteAttributeString("id", num.ToString());
			writer.WriteStartElement("paragraph-format");
			SerializeBoolAttribute(writer, "PageBreakAfter", bValue: true);
			writer.WriteEndElement();
			writer.WriteStartElement("items");
			writer.WriteStartElement("item");
			writer.WriteAttributeString("id", "0");
			writer.WriteAttributeString("type", "Table");
			writer.WriteAttributeString("ColumnsCount", num2.ToString());
			WriteEmptyBorders(writer);
			writer.WriteStartElement("rows");
			if (headingsSize.Height > 0f)
			{
				iRowId = SerializeHeadingsRow(writer, worksheet, headingsSize, iFirstCol, iLastCol, iRowId);
			}
			for (int i = iFirstRow; i <= iLastRow; i++)
			{
				iRowId = SerializeRow(writer, worksheet, i, iFirstCol, iLastCol, iRowId, headingsSize);
			}
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteEndElement();
			num++;
		}
	}

	private void WriteEmptyBorders(XmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		writer.WriteStartElement("cell-format");
		writer.WriteStartElement("borders");
		int i = 0;
		for (int num = DEF_DLS_BORDER_NAMES.Length; i < num; i++)
		{
			string localName = DEF_DLS_BORDER_NAMES[i];
			writer.WriteStartElement(localName);
			writer.WriteAttributeString("BorderType", "None");
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	private void WriteHeadingsBorders(XmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		writer.WriteStartElement("cell-format");
		writer.WriteStartElement("borders");
		int i = 0;
		for (int num = DEF_DLS_BORDER_NAMES.Length; i < num; i++)
		{
			string localName = DEF_DLS_BORDER_NAMES[i];
			writer.WriteStartElement(localName);
			writer.WriteAttributeString("BorderType", "Single");
			writer.WriteAttributeString("Width", "1");
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	private bool FillNextPage(IRange range, ref int iFirstRow, ref int iFirstCol, ref int iLastRow, ref int iLastCol, SizeF pageSize, OfficeOrder pageOrder)
	{
		if (range == null)
		{
			throw new ArgumentNullException("range");
		}
		if (iLastRow == range.LastRow && iLastCol == range.LastColumn)
		{
			return false;
		}
		if (iFirstRow == -1)
		{
			iFirstRow = range.Row;
			iFirstCol = range.Column;
		}
		else if (pageOrder == OfficeOrder.DownThenOver)
		{
			if (iLastRow == range.LastRow)
			{
				iFirstRow = range.Row;
				iFirstCol = iLastCol + 1;
			}
			else
			{
				iFirstRow = iLastRow + 1;
			}
		}
		else if (iLastCol == range.LastColumn)
		{
			iFirstCol = range.Column;
			iFirstRow = iLastRow + 1;
		}
		else
		{
			iFirstCol = iLastCol + 1;
		}
		iLastRow = Math.Min(GetMaxRow(range.Worksheet, iFirstRow, pageSize.Height), range.LastRow);
		iLastCol = Math.Min(GetMaxColumn(range.Worksheet, iFirstCol, pageSize.Width), range.LastColumn);
		return true;
	}

	private int GetMaxColumn(IWorksheet sheet, int iFirstColumn, double dPageWidth)
	{
		if (dPageWidth <= 0.0)
		{
			throw new ArgumentOutOfRangeException("dPageWidth");
		}
		if (iFirstColumn <= 0)
		{
			throw new ArgumentOutOfRangeException("iFirstColumn");
		}
		double num = 0.0;
		int num2 = iFirstColumn - 1;
		int result = iFirstColumn;
		double columnWidth;
		for (; num <= dPageWidth; num += columnWidth)
		{
			result = num2;
			num2++;
			if (num2 > sheet.Workbook.MaxColumnCount)
			{
				break;
			}
			columnWidth = GetColumnWidth(sheet, num2);
		}
		return result;
	}

	private int GetMaxRow(IWorksheet sheet, int iFirstRow, double dPageHeight)
	{
		if (dPageHeight <= 0.0)
		{
			throw new ArgumentOutOfRangeException("dPageHeight");
		}
		if (iFirstRow <= 0)
		{
			throw new ArgumentOutOfRangeException("iFirstRow");
		}
		double num = 0.0;
		int num2 = iFirstRow - 1;
		int result = iFirstRow;
		int maxRowCount = sheet.Workbook.MaxRowCount;
		double num3;
		for (; num <= dPageHeight; num += num3)
		{
			result = num2;
			num2++;
			if (num2 > maxRowCount)
			{
				break;
			}
			num3 = ApplicationImpl.ConvertUnitsStatic(sheet.GetRowHeightInPixels(num2), MeasureUnits.Pixel, MeasureUnits.Point);
		}
		return result;
	}

	private int SerializeRow(XmlWriter writer, IWorksheet sheet, int iRow, int iFirstCol, int iLastCol, int iRowId, SizeF headingsSize)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		double num = sheet.GetRowHeightInPixels(iRow);
		if (num > 0.0)
		{
			iRowId++;
			num = ApplicationImpl.ConvertUnitsStatic(num, MeasureUnits.Pixel, MeasureUnits.Point);
			writer.WriteStartElement("row");
			writer.WriteAttributeString("id", iRowId.ToString());
			WriteAttribute(writer, "RowHeight", num);
			writer.WriteStartElement("cells");
			int num2 = 0;
			float width = headingsSize.Width;
			if (width > 0f)
			{
				SerializeCell(writer, width, iRow.ToString(), num2);
				num2++;
			}
			int num3 = iFirstCol;
			while (num3 <= iLastCol)
			{
				SerializeCell(writer, sheet, iRow, num3, num2);
				num3++;
				num2++;
			}
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
		return iRowId;
	}

	private int SerializeHeadingsRow(XmlWriter writer, IWorksheet sheet, SizeF headingsSize, int iFirstColumn, int iLastColumn, int iRowId)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		float height = headingsSize.Height;
		float width = headingsSize.Width;
		if (height > 0f)
		{
			iRowId++;
			height = (float)ApplicationImpl.ConvertUnitsStatic(height, MeasureUnits.Pixel, MeasureUnits.Point);
			writer.WriteStartElement("row");
			writer.WriteAttributeString("id", iRowId.ToString());
			WriteAttribute(writer, "RowHeight", height);
			writer.WriteStartElement("cells");
			int num = 0;
			if (width > 0f)
			{
				SerializeCell(writer, width, string.Empty, num);
				num++;
			}
			int num2 = iFirstColumn;
			while (num2 <= iLastColumn)
			{
				string columnName = RangeImpl.GetColumnName(num2);
				double columnWidth = GetColumnWidth(sheet, num2);
				SerializeCell(writer, columnWidth, columnName, num);
				num2++;
				num++;
			}
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
		return iRowId;
	}

	private void SerializeCell(XmlWriter writer, IWorksheet sheet, int iRow, int iColumn, int iCellId)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		WorksheetImpl worksheetImpl = (WorksheetImpl)sheet;
		int xFIndex = worksheetImpl.GetXFIndex(iRow, iColumn);
		ExtendedFormatImpl xFormat = worksheetImpl.ParentWorkbook.InnerExtFormats[xFIndex];
		double columnWidth = GetColumnWidth(sheet, iColumn);
		writer.WriteStartElement("cell");
		writer.WriteAttributeString("id", iCellId.ToString());
		WriteAttribute(writer, "Width", columnWidth);
		if (sheet.Contains(iRow, iColumn))
		{
			long cellIndex = RangeImpl.GetCellIndex(iColumn, iRow);
			RichTextString rTFString = worksheetImpl.CellRecords.GetRTFString(cellIndex, bAutofitRows: false);
			SerializeRichTextString(writer, rTFString, xFormat);
		}
		SerializeTableFormat(writer, xFormat);
		writer.WriteEndElement();
	}

	private void SerializeCell(XmlWriter writer, double dWidth, string strCellValue, int iCellId)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (strCellValue == null)
		{
			throw new ArgumentNullException("strCellValue");
		}
		writer.WriteStartElement("cell");
		writer.WriteAttributeString("id", iCellId.ToString());
		WriteAttribute(writer, "Width", dWidth);
		writer.WriteStartElement("paragraphs");
		writer.WriteStartElement("paragraph");
		writer.WriteStartElement("paragraph-format");
		writer.WriteAttributeString("HrAlignment", "Center");
		writer.WriteEndElement();
		writer.WriteStartElement("items");
		writer.WriteStartElement("item");
		writer.WriteAttributeString("type", "TextRange");
		writer.WriteStartElement("text");
		writer.WriteString(strCellValue);
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
		WriteHeadingsBorders(writer);
		writer.WriteEndElement();
	}

	private void SerializeTableFormat(XmlWriter writer, ExtendedFormatImpl xFormat)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (xFormat == null)
		{
			throw new ArgumentNullException("xFormat");
		}
		writer.WriteStartElement("cell-format");
		string vAlignment = GetVAlignment(xFormat.VerticalAlignment);
		WriteAttribute(writer, "VAlignment", vAlignment);
		if (!xFormat.IsDefaultColor)
		{
			string colorString = GetColorString(xFormat.Color);
			writer.WriteAttributeString("ShadingColor", colorString);
		}
		SerializeBorders(writer, xFormat);
		writer.WriteEndElement();
	}

	private void SerializeBorders(XmlWriter writer, ExtendedFormatImpl xFormat)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (xFormat == null)
		{
			throw new ArgumentNullException("xFormat");
		}
		writer.WriteStartElement("borders");
		BorderImpl borderImpl = null;
		int i = 0;
		for (int num = DEF_DLS_BORDERS.Length; i < num; i++)
		{
			OfficeBordersIndex borderIndex = DEF_DLS_BORDERS[i];
			if (borderImpl == null)
			{
				borderImpl = new BorderImpl(xFormat.Application, xFormat, xFormat, borderIndex);
			}
			else
			{
				borderImpl.BorderIndex = borderIndex;
			}
			writer.WriteStartElement(DEF_DLS_BORDER_NAMES[i]);
			if (borderImpl.LineStyle != 0)
			{
				writer.WriteAttributeString("Color", GetColorString(borderImpl.ColorRGB));
				writer.WriteAttributeString("LineWidth", GetLineWidth(borderImpl));
			}
			writer.WriteAttributeString("BorderType", GetBorderType(borderImpl));
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	private string GetVAlignment(OfficeVAlign align)
	{
		switch (align)
		{
		case OfficeVAlign.VAlignTop:
			return "Top";
		case OfficeVAlign.VAlignBottom:
			return "Bottom";
		case OfficeVAlign.VAlignCenter:
		case OfficeVAlign.VAlignDistributed:
			return "Middle";
		case OfficeVAlign.VAlignJustify:
			return null;
		default:
			throw new ArgumentOutOfRangeException("align");
		}
	}

	private string GetHAlignment(OfficeHAlign align)
	{
		switch (align)
		{
		case OfficeHAlign.HAlignCenter:
		case OfficeHAlign.HAlignCenterAcrossSelection:
			return "Center";
		case OfficeHAlign.HAlignLeft:
			return "Left";
		case OfficeHAlign.HAlignJustify:
			return "Justify";
		case OfficeHAlign.HAlignRight:
			return "Right";
		default:
			return null;
		}
	}

	private string GetLineWidth(IBorder border)
	{
		switch (border.LineStyle)
		{
		case OfficeLineStyle.Hair:
			return DEF_BORDER_WIDTH_HAIR;
		case OfficeLineStyle.Thin:
		case OfficeLineStyle.Dashed:
		case OfficeLineStyle.Dotted:
		case OfficeLineStyle.Double:
		case OfficeLineStyle.Dash_dot:
		case OfficeLineStyle.Dash_dot_dot:
		case OfficeLineStyle.Slanted_dash_dot:
			return DEF_BORDER_WIDTH_THIN;
		case OfficeLineStyle.Medium:
		case OfficeLineStyle.Medium_dashed:
		case OfficeLineStyle.Medium_dash_dot:
		case OfficeLineStyle.Medium_dash_dot_dot:
			return DEF_BORDER_WIDTH_MEDIUM;
		case OfficeLineStyle.Thick:
			return DEF_BORDER_WIDTH_THICK;
		default:
			return "0";
		}
	}

	private string GetBorderType(IBorder border)
	{
		switch (border.LineStyle)
		{
		case OfficeLineStyle.Thin:
		case OfficeLineStyle.Medium:
		case OfficeLineStyle.Hair:
			return "Single";
		case OfficeLineStyle.Double:
			return "Double";
		case OfficeLineStyle.Dotted:
			return "Dot";
		case OfficeLineStyle.Dashed:
		case OfficeLineStyle.Medium_dashed:
			return "DashSmallGap";
		case OfficeLineStyle.Dash_dot:
		case OfficeLineStyle.Medium_dash_dot:
		case OfficeLineStyle.Slanted_dash_dot:
			return "DotDash";
		case OfficeLineStyle.Dash_dot_dot:
		case OfficeLineStyle.Medium_dash_dot_dot:
			return "DotDotDash";
		case OfficeLineStyle.Thick:
			return "Thick";
		default:
			return "None";
		}
	}

	private void SerializeRichTextString(XmlWriter writer, RichTextString rtfString, ExtendedFormatImpl xFormat)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (rtfString == null)
		{
			return;
		}
		writer.WriteStartElement("paragraphs");
		writer.WriteStartElement("paragraph");
		writer.WriteAttributeString("id", "0");
		writer.WriteStartElement("paragraph-format");
		string hAlignment = GetHAlignment(xFormat.HorizontalAlignment);
		WriteAttribute(writer, "HrAlignment", hAlignment);
		writer.WriteEndElement();
		writer.WriteStartElement("items");
		TextWithFormat textObject = rtfString.TextObject;
		int formattingRunsCount = textObject.FormattingRunsCount;
		string text = textObject.Text;
		FontsCollection innerFonts = rtfString.Workbook.InnerFonts;
		int num = 0;
		if (formattingRunsCount > 0)
		{
			int length = textObject.Text.Length;
			int num2 = 0;
			int positionByIndex = textObject.GetPositionByIndex(0);
			if (positionByIndex != 0)
			{
				string strText = text.Substring(0, positionByIndex);
				WriteText(writer, strText, rtfString.DefaultFont, num);
				num++;
			}
			while (num2 < formattingRunsCount)
			{
				int fontByIndex = textObject.GetFontByIndex(num2);
				positionByIndex = textObject.GetPositionByIndex(num2);
				int num3 = ((num2 != formattingRunsCount - 1) ? textObject.GetPositionByIndex(num2 + 1) : length);
				string strText = text.Substring(positionByIndex, num3 - positionByIndex);
				IOfficeFont font = innerFonts[fontByIndex];
				WriteText(writer, strText, font, num);
				num2++;
				num++;
			}
		}
		else
		{
			WriteText(writer, text, rtfString.DefaultFont, num);
		}
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	private void WriteText(XmlWriter writer, string strText, IOfficeFont font, int id)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (strText == null)
		{
			throw new ArgumentNullException("strText");
		}
		writer.WriteStartElement("item");
		writer.WriteAttributeString("id", id.ToString());
		writer.WriteAttributeString("type", "TextRange");
		if (font != null && strText.Length > 0)
		{
			WriteFont(writer, font);
		}
		writer.WriteStartElement("text");
		writer.WriteString(strText);
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	private void WriteFont(XmlWriter writer, IOfficeFont font)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		writer.WriteStartElement("character-format");
		writer.WriteAttributeString("FontName", font.FontName.ToString());
		writer.WriteAttributeString("FontSize", font.Size.ToString());
		writer.WriteAttributeString("TextColor", GetColorString(font.RGBColor));
		SerializeBoolAttribute(writer, "Bold", font.Bold);
		SerializeBoolAttribute(writer, "Italic", font.Italic);
		writer.WriteAttributeString("Underline", GetUnderlineString(font.Underline));
		writer.WriteAttributeString("SubSuperScript", GetSubSuperScript(font));
		SerializeBoolAttribute(writer, "Strike", font.Strikethrough);
		writer.WriteEndElement();
	}

	private void SerializeBoolAttribute(XmlWriter writer, string strAttributeName, bool bValue)
	{
		if (bValue)
		{
			if (writer == null)
			{
				throw new ArgumentNullException("writer");
			}
			if (strAttributeName == null)
			{
				throw new ArgumentNullException("strAttributeName");
			}
			if (strAttributeName.Length == 0)
			{
				throw new ArgumentException("strAttributeName - string cannot be empty.");
			}
			writer.WriteAttributeString(strAttributeName, DEF_TRUE_STRING);
		}
	}

	private string GetColorString(Color color)
	{
		return "#" + color.ToArgb().ToString("X");
	}

	private string GetUnderlineString(OfficeUnderline underline)
	{
		switch (underline)
		{
		case OfficeUnderline.Double:
		case OfficeUnderline.DoubleAccounting:
			return "Double";
		case OfficeUnderline.Single:
		case OfficeUnderline.SingleAccounting:
			return "Single";
		default:
			return "None";
		}
	}

	private string GetSubSuperScript(IOfficeFont font)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (font.Subscript)
		{
			return "SubScript";
		}
		if (font.Superscript)
		{
			return "SuperScript";
		}
		return "None";
	}

	private void WriteAttribute(XmlWriter writer, string strAttributeName, double value, MeasureUnits units)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (strAttributeName == null)
		{
			throw new ArgumentNullException("strAttributeName");
		}
		if (strAttributeName.Length == 0)
		{
			throw new ArgumentException("strAttributeName - string cannot be empty.");
		}
		value = ApplicationImpl.ConvertUnitsStatic(value, units, MeasureUnits.Point);
		string value2 = value.ToString(DLSCulture);
		writer.WriteAttributeString(strAttributeName, value2);
	}

	private void WriteAttribute(XmlWriter writer, string strAttributeName, string strValue)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (strAttributeName == null)
		{
			throw new ArgumentNullException("strAttributeName");
		}
		if (strAttributeName.Length == 0)
		{
			throw new ArgumentException("strAttributeName - string cannot be empty.");
		}
		if (strValue != null && strValue.Length != 0)
		{
			writer.WriteAttributeString(strAttributeName, strValue);
		}
	}

	private void WriteAttribute(XmlWriter writer, string strAttributeName, double value)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (strAttributeName == null)
		{
			throw new ArgumentNullException("strAttributeName");
		}
		if (strAttributeName.Length == 0)
		{
			throw new ArgumentException("strAttributeName - string cannot be empty.");
		}
		string value2 = value.ToString(DLSCulture);
		writer.WriteAttributeString(strAttributeName, value2);
	}

	private void SerializeHeaderFooter(XmlWriter writer, IWorksheet sheet, double dPageWidth)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		writer.WriteStartElement("headers-footers");
		IPageSetup pageSetup = sheet.PageSetup;
		string[] array = new string[3] { pageSetup.LeftHeader, pageSetup.RightHeader, pageSetup.CenterHeader };
		writer.WriteStartElement("odd-header");
		SerializeHeaderFooter(writer, array, dPageWidth);
		writer.WriteEndElement();
		array[0] = pageSetup.LeftFooter;
		array[1] = pageSetup.RightFooter;
		array[2] = pageSetup.CenterFooter;
		writer.WriteStartElement("odd-footer");
		SerializeHeaderFooter(writer, array, dPageWidth);
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	private void SerializeHeaderFooter(XmlWriter writer, string[] arrValues, double dPageWidth)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (arrValues == null)
		{
			throw new ArgumentNullException("arrValues");
		}
		int num = arrValues.Length;
		if (num == 0)
		{
			return;
		}
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			num2 += arrValues[i].Length;
		}
		if (num2 != 0)
		{
			writer.WriteStartElement("paragraphs");
			writer.WriteStartElement("paragraph");
			writer.WriteStartElement("items");
			writer.WriteStartElement("item");
			writer.WriteAttributeString("id", "0");
			writer.WriteAttributeString("type", "Table");
			writer.WriteAttributeString("ColumnsCount", num.ToString());
			WriteEmptyBorders(writer);
			writer.WriteStartElement("rows");
			writer.WriteStartElement("row");
			writer.WriteAttributeString("id", "0");
			writer.WriteStartElement("cells");
			double value = dPageWidth / (double)num;
			for (int j = 0; j < num; j++)
			{
				writer.WriteStartElement("cell");
				writer.WriteAttributeString("id", j.ToString());
				WriteAttribute(writer, "Width", value);
				writer.WriteStartElement("paragraphs");
				writer.WriteStartElement("paragraph");
				writer.WriteAttributeString("id", "0");
				writer.WriteStartElement("items");
				writer.WriteStartElement("item");
				writer.WriteAttributeString("id", "0");
				writer.WriteAttributeString("type", "TextRange");
				writer.WriteStartElement("text");
				writer.WriteString(arrValues[j]);
				writer.WriteEndElement();
				writer.WriteEndElement();
				writer.WriteEndElement();
				writer.WriteEndElement();
				writer.WriteEndElement();
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
	}

	private double GetColumnWidth(IWorksheet sheet, int iColumn)
	{
		return ApplicationImpl.ConvertUnitsStatic(sheet.GetColumnWidthInPixels(iColumn), MeasureUnits.Pixel, MeasureUnits.Point) + 1.0;
	}
}
