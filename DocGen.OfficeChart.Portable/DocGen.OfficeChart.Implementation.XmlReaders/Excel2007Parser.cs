using System;
using System.Collections;
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
using DocGen.OfficeChart.Implementation.Exceptions;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Implementation.XmlReaders.Shapes;
using DocGen.OfficeChart.Implementation.XmlSerialization;
using DocGen.OfficeChart.Implementation.XmlSerialization.Charts;
using DocGen.OfficeChart.Implementation.XmlSerialization.Constants;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

namespace DocGen.OfficeChart.Implementation.XmlReaders;

internal class Excel2007Parser
{
	private delegate OfficeSheetProtection ChecProtectionDelegate(XmlReader reader, string attributeName, OfficeSheetProtection flag, bool defaultValue, OfficeSheetProtection protection);

	internal const byte HLSMax = byte.MaxValue;

	private const byte RGBMax = byte.MaxValue;

	private const double Undefined = 170.0;

	public const int AdditionalProgressItems = 4;

	private const string CarriageReturn = "_x000d_";

	private const string LineFeed = "_x000a_";

	private const string NullChar = "_x0000_";

	private const string BackSpace = "_x0008_";

	private const string Tab = "_x0009_";

	private const string ContentTypeSchema = "contentTypeSchema";

	private const string ContentTypeNameSpace = "http://schemas.microsoft.com/office/2006/metadata/contentType";

	private const string XmlSchemaNameSpace = "http://www.w3.org/2001/XMLSchema";

	private const string ElementName = "element";

	private const string Name = "name";

	private const string DisplayName = "ma:displayName";

	private const string InternalName = "ma:internalName";

	private const string Reference = "ref";

	private const string ComplexContent = "complexContent";

	private WorkbookImpl m_book;

	private FormulaUtil m_formulaUtil;

	private Dictionary<int, ShapeParser> m_dictShapeParsers = new Dictionary<int, ShapeParser>();

	private List<Color> m_lstThemeColors;

	internal Dictionary<string, Color> m_dicThemeColors;

	internal Dictionary<string, string> m_colorMap;

	internal Dictionary<string, Color> m_themeColorOverrideDictionary;

	private Dictionary<string, FontImpl> m_dicMajorFonts;

	private Dictionary<string, FontImpl> m_dicMinorFonts;

	private List<string> m_values = new List<string>();

	private string parentElement = string.Empty;

	private bool m_enableAlternateContent;

	private WorksheetImpl m_workSheet;

	private DrawingParser m_drawingParser;

	private Dictionary<int, List<Point>> m_outlineLevels = new Dictionary<int, List<Point>>();

	private Dictionary<int, int> m_indexAndLevels = new Dictionary<int, int>();

	private int dpiX;

	private int dpiY;

	private int minRow;

	private int maxRow;

	private int minColumn;

	private int maxColumn;

	private readonly int[] DEF_NUMBERFORMAT_INDEXES = new int[8] { 5, 6, 7, 8, 41, 42, 43, 44 };

	internal static bool m_isPresentation;

	public FormulaUtil FormulaUtil => m_formulaUtil;

	internal WorksheetImpl Worksheet => m_workSheet;

	internal WorkbookImpl Workbook => m_book;

	public Excel2007Parser(WorkbookImpl book)
	{
		if (book == null)
		{
			throw new ArgumentNullException("book");
		}
		m_book = book;
		m_formulaUtil = new FormulaUtil(m_book.Application, m_book, NumberFormatInfo.InvariantInfo, ',', ';');
		dpiX = book.AppImplementation.GetdpiX();
		dpiY = book.AppImplementation.GetdpiY();
	}

	public Color GetThemeColor(string colorName)
	{
		if (m_themeColorOverrideDictionary != null && m_themeColorOverrideDictionary.Count > 0)
		{
			return GetThemeColor(colorName, m_themeColorOverrideDictionary, m_colorMap);
		}
		return GetThemeColor(colorName, m_dicThemeColors, m_colorMap);
	}

	public static Color GetThemeColor(string colorName, Dictionary<string, Color> themeColors, Dictionary<string, string> colorMap)
	{
		if (colorName == null || colorName.Length == 0)
		{
			throw new ArgumentOutOfRangeException("colorName");
		}
		if (colorMap != null && colorMap.TryGetValue(colorName, out var value))
		{
			colorName = value;
		}
		Color value2 = ColorExtension.Empty;
		bool flag = themeColors.TryGetValue(colorName, out value2);
		if (!flag && colorName == "bg1")
		{
			colorName = "lt1";
			value2 = GetThemeColor(colorName, themeColors, colorMap);
		}
		else if (!flag && colorName == "bg2")
		{
			colorName = "lt2";
			value2 = GetThemeColor(colorName, themeColors, colorMap);
		}
		return value2;
	}

	public void ParseContentTypes(XmlReader reader, IDictionary<string, string> contentDefaults, IDictionary<string, string> contentOverrides)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (contentDefaults == null)
		{
			throw new ArgumentNullException("contentDefaults");
		}
		if (contentOverrides == null)
		{
			throw new ArgumentNullException("contentOverrides");
		}
		while (reader.NodeType != XmlNodeType.Element && !reader.EOF)
		{
			reader.Read();
		}
		if (reader.EOF)
		{
			throw new XmlException("Cannot locate appropriate xml tag");
		}
		if (reader.LocalName == "Types" && reader.NamespaceURI == "http://schemas.openxmlformats.org/package/2006/content-types")
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					string localName = reader.LocalName;
					if (!(localName == "Default"))
					{
						if (!(localName == "Override"))
						{
							throw new NotImplementedException(reader.LocalName);
						}
						ParseDictionaryEntry(reader, contentOverrides, "PartName", "ContentType");
					}
					else
					{
						ParseDictionaryEntry(reader, contentDefaults, "Extension", "ContentType");
					}
					reader.Skip();
				}
				else
				{
					reader.Read();
				}
			}
			return;
		}
		throw new XmlException("Cannot locate appropriate xml tag");
	}

	public void ParseWorkbook(XmlReader reader, RelationCollection relations, FileDataHolder holder, string bookPath, Stream streamStart, Stream streamEnd, ref List<Dictionary<string, string>> lstBookViews, Stream functionGroups)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (streamStart == null)
		{
			throw new ArgumentNullException("streamStart");
		}
		if (streamEnd == null)
		{
			throw new ArgumentNullException("streamEnd");
		}
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.LocalName == "workbook")
		{
			bool bAdd = false;
			StreamWriter textWriter = new StreamWriter(streamStart);
			XmlWriter writer = UtilityMethods.CreateWriter(textWriter);
			writer.WriteStartElement("root", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
			reader.Read();
			int iActiveSheetIndex = 0;
			int iDisplayedTab = 0;
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					switch (reader.LocalName)
					{
					case "definedNames":
						m_book.ActiveSheetIndex = iActiveSheetIndex;
						ParseNamedRanges(reader);
						SwitchStreams(ref bAdd, ref writer, ref textWriter, streamEnd);
						break;
					case "sheets":
						ParseSheetsOptions(reader, relations, holder, bookPath);
						SwitchStreams(ref bAdd, ref writer, ref textWriter, streamEnd);
						break;
					case "bookViews":
						lstBookViews = ParseBookViews(reader, out iActiveSheetIndex, out iDisplayedTab);
						SwitchStreams(ref bAdd, ref writer, ref textWriter, streamEnd);
						break;
					case "calcPr":
						ParseCalcProperties(reader);
						break;
					case "externalReferences":
						ParseExternalLinksWorkbookPart(reader);
						break;
					case "workbookProtection":
						ParseWorkbookProtection(reader);
						break;
					case "fileVersion":
						ParseFileVersion(reader, m_book.DataHolder.FileVersion);
						break;
					case "functionGroups":
						UtilityMethods.CreateWriter(functionGroups, Encoding.UTF8).WriteNode(reader, defattr: false);
						break;
					case "workbookPr":
						ParseWorkbookPr(reader);
						break;
					case "pivotCaches":
						ParsePivotCaches(reader);
						break;
					default:
						writer.WriteNode(reader, defattr: false);
						break;
					}
				}
				else
				{
					reader.Read();
				}
			}
			m_book.ActiveSheetIndex = iActiveSheetIndex;
			m_book.DisplayedTab = iDisplayedTab;
			writer.WriteEndElement();
			writer.Flush();
			return;
		}
		throw new XmlException("Unexpected xml tag: " + reader.LocalName);
	}

	private static void ParseDocumentManagmentSchema(XmlReader reader, ref List<string> m_values)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "element")
			{
				string attribute = reader.GetAttribute("ref");
				if (attribute != null && attribute.Length > 0)
				{
					m_values.Add(attribute.Split(':')[1]);
				}
			}
			reader.Read();
		}
	}

	private void ParseChildElements(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		List<Stream> list = new List<Stream>();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Whitespace)
			{
				reader.Read();
			}
			if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "element" && reader.GetAttribute("name") != null)
			{
				list.Add(ShapeParser.ReadNodeAsStream(reader));
			}
			reader.Read();
		}
		if (!m_book.m_childElements.ContainsKey(parentElement) && list.Count > 0)
		{
			m_book.m_childElements.Add(parentElement, list);
		}
	}

	public void ParsePivotTables()
	{
		foreach (WorksheetImpl item in m_book.Worksheets as WorksheetsCollection)
		{
			item.DataHolder.ParsePivotTable(item);
		}
	}

	public void ParseWorksheets(Dictionary<int, int> dictUpdatedSSTIndexes, bool parseOnDemand)
	{
		ITabSheets objects = m_book.Objects;
		ApplicationImpl appImplementation = m_book.AppImplementation;
		int num = objects.Count + 4;
		appImplementation.RaiseProgressEvent(4L, num);
		appImplementation.IsFormulaParsed = false;
		int i = 0;
		for (int count = objects.Count; i < count; i++)
		{
			WorksheetBaseImpl worksheetBaseImpl = (WorksheetBaseImpl)objects[i];
			if (worksheetBaseImpl is WorksheetImpl)
			{
				(worksheetBaseImpl as WorksheetImpl).ParseDataOnDemand = parseOnDemand;
			}
			else if (worksheetBaseImpl is ChartImpl)
			{
				continue;
			}
			worksheetBaseImpl.ParseData(dictUpdatedSSTIndexes);
			worksheetBaseImpl.IsSaved = false;
			appImplementation.RaiseProgressEvent(i + 4 + 1, num);
		}
		appImplementation.IsFormulaParsed = true;
	}

	private void ParsePivotCaches(XmlReader reader)
	{
		if (reader.LocalName != "pivotCaches")
		{
			throw new XmlException();
		}
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					if (reader.LocalName == "pivotCache")
					{
						ParsePivotCache(reader);
					}
					else
					{
						reader.Skip();
					}
				}
				else
				{
					reader.Skip();
				}
			}
		}
		reader.Read();
	}

	private void ParsePivotCache(XmlReader reader)
	{
		if (reader.LocalName != "pivotCache")
		{
			throw new XmlException();
		}
		string cacheId = null;
		string relationId = null;
		if (reader.MoveToAttribute("cacheId"))
		{
			cacheId = reader.Value;
		}
		if (reader.MoveToAttribute("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships"))
		{
			relationId = reader.Value;
		}
		m_book.DataHolder.RegisterCache(cacheId, relationId);
		reader.MoveToElement();
		reader.Skip();
	}

	private void ParseFileVersion(XmlReader reader, FileVersion fileVersion)
	{
		if (reader.LocalName != "fileVersion")
		{
			throw new XmlException();
		}
		fileVersion.ApplicationName = (reader.MoveToAttribute("appName") ? (fileVersion.ApplicationName = reader.Value) : null);
		fileVersion.BuildVersion = (reader.MoveToAttribute("rupBuild") ? reader.Value : null);
		fileVersion.LowestEdited = (reader.MoveToAttribute("lowestEdited") ? reader.Value : null);
		fileVersion.LastEdited = (reader.MoveToAttribute("lastEdited") ? reader.Value : null);
		fileVersion.CodeName = (reader.MoveToAttribute("codeName") ? reader.Value : null);
		reader.MoveToElement();
		reader.Skip();
	}

	private void ParseWorkbookPr(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentException("reader");
		}
		if (reader.LocalName != "workbookPr")
		{
			throw new XmlException();
		}
		if (reader.MoveToAttribute("date1904"))
		{
			m_book.Date1904 = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("codeName"))
		{
			m_book.CodeName = reader.Value;
		}
		if (reader.MoveToAttribute("hidePivotFieldList"))
		{
			m_book.HidePivotFieldList = !XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("defaultThemeVersion"))
		{
			m_book.DefaultThemeVersion = reader.Value;
		}
		reader.Skip();
	}

	private void ParseCalcProperties(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentException("reader");
		}
		if (reader.LocalName != "calcPr")
		{
			throw new XmlException();
		}
		if (reader.MoveToAttribute("fullPrecision"))
		{
			m_book.PrecisionAsDisplayed = !XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("calcId"))
		{
			m_book.DataHolder.CalculationId = reader.Value;
		}
		reader.Skip();
	}

	private void ParseWorkbookProtection(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "workbookProtection")
		{
			throw new XmlException();
		}
		bool flag = false;
		bool flag2 = false;
		ushort isPassword = 0;
		if (reader.MoveToAttribute("workbookPassword"))
		{
			isPassword = ushort.Parse(reader.Value, NumberStyles.HexNumber, CultureInfo.CurrentCulture);
		}
		if (reader.MoveToAttribute("lockStructure"))
		{
			flag = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("lockWindows"))
		{
			flag2 = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("workbookAlgorithmName"))
		{
			m_book.AlgorithmName = reader.Value;
		}
		if (reader.MoveToAttribute("workbookSaltValue"))
		{
			m_book.SaltValue = Convert.FromBase64String(reader.Value);
		}
		if (reader.MoveToAttribute("workbookHashValue"))
		{
			m_book.HashValue = Convert.FromBase64String(reader.Value);
		}
		if (reader.MoveToAttribute("workbookSpinCount"))
		{
			m_book.SpinCount = Convert.ToUInt32(reader.Value);
		}
		reader.Read();
		if (flag || flag2)
		{
			m_book.Protect(flag2, flag);
		}
		m_book.Password.IsPassword = isPassword;
	}

	private List<Dictionary<string, string>> ParseBookViews(XmlReader reader, out int iActiveSheetIndex, out int iDisplayedTab)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		reader.Read();
		iActiveSheetIndex = 0;
		iDisplayedTab = 0;
		List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
		Dictionary<string, string> item;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.LocalName == "workbookView")
			{
				item = ParseWorkbookView(reader);
				list.Add(item);
			}
			reader.Skip();
		}
		item = list[0];
		if (item.TryGetValue("activeTab", out var value))
		{
			iActiveSheetIndex = XmlConvertExtension.ToInt32(value);
		}
		if (item.TryGetValue("firstSheet", out value))
		{
			iDisplayedTab = XmlConvertExtension.ToInt32(value);
		}
		reader.Skip();
		return list;
	}

	private Dictionary<string, string> ParseWorkbookView(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		int i = 0;
		for (int attributeCount = reader.AttributeCount; i < attributeCount; i++)
		{
			reader.MoveToAttribute(i);
			dictionary.Add(reader.Name, reader.Value);
		}
		return dictionary;
	}

	public void ParseSheet(XmlReader reader, WorksheetImpl sheet, string strParentPath, ref MemoryStream streamStart, ref MemoryStream streamCF, List<int> arrStyles, Dictionary<string, object> dictItemsToRemove, Dictionary<int, int> dictUpdatedSSTIndexes)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (streamStart == null)
		{
			throw new ArgumentNullException("streamStart");
		}
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.LocalName != "worksheet")
		{
			throw new XmlException("worksheet tag was not found.");
		}
		reader.Read();
		m_workSheet = sheet;
		ParseSheetBeforeData(reader, sheet, streamStart, arrStyles);
		m_book.AppImplementation.IsFormulaParsed = false;
		if (reader.LocalName == "sheetData")
		{
			ParseSheetData(reader, sheet, arrStyles, "c");
		}
		m_book.AppImplementation.IsFormulaParsed = true;
		if (dictUpdatedSSTIndexes != null)
		{
			sheet.UpdateLabelSSTIndexes(dictUpdatedSSTIndexes, sheet.ParentWorkbook.InnerSST.AddIncrease);
		}
	}

	private void ParseSheetBeforeData(XmlReader reader, WorksheetImpl sheet, Stream streamStart, List<int> arrStyles)
	{
		while (!reader.EOF && reader.NodeType != XmlNodeType.EndElement && reader.LocalName != "sheetData")
		{
			switch (reader.LocalName)
			{
			case "cols":
				ParseColumns(reader, sheet, arrStyles);
				reader.Read();
				continue;
			case "sheetPr":
				ParseSheetLevelProperties(reader, sheet);
				continue;
			case "sheetViews":
				ParseSheetViews(reader, sheet);
				continue;
			case "dimension":
				reader.Skip();
				continue;
			case "sheetFormatPr":
				ExtractDefaultRowHeight(reader, sheet);
				ExtractZeroHeight(reader, sheet);
				reader.MoveToElement();
				break;
			}
			reader.Skip();
		}
	}

	private void ParseSheetViews(XmlReader reader, WorksheetBaseImpl sheet)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (reader.LocalName != "sheetViews")
		{
			throw new XmlException("Wrong xml tag");
		}
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.LocalName == "sheetView")
				{
					ParseSheetView(reader, sheet);
				}
				else
				{
					reader.Skip();
				}
			}
		}
		reader.Read();
	}

	private void ParseSheetView(XmlReader reader, WorksheetBaseImpl sheetBase)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheetBase == null)
		{
			throw new ArgumentNullException("sheetBase");
		}
		if (reader.LocalName != "sheetView")
		{
			throw new XmlException("Wrong xml tag");
		}
		WorksheetImpl worksheetImpl = sheetBase as WorksheetImpl;
		if (reader.MoveToAttribute("showGridLines"))
		{
			worksheetImpl.IsGridLinesVisible = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("topLeftCell"))
		{
			worksheetImpl.TopLeftCell = worksheetImpl[reader.Value];
		}
		if (reader.MoveToAttribute("showZeros"))
		{
			worksheetImpl.IsDisplayZeros = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("showRowColHeaders"))
		{
			worksheetImpl.IsRowColumnHeadersVisible = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("rightToLeft"))
		{
			worksheetImpl.IsRightToLeft = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("zoomScale"))
		{
			sheetBase.Zoom = XmlConvertExtension.ToInt32(reader.Value);
		}
		if (reader.MoveToAttribute("zoomToFit"))
		{
			(sheetBase as ChartImpl).ZoomToFit = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("defaultGridColor"))
		{
			worksheetImpl.WindowTwo.IsDefaultHeader = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("colorId"))
		{
			worksheetImpl.GridLineColor = (OfficeKnownColors)XmlConvertExtension.ToInt32(reader.Value);
		}
		if (reader.MoveToAttribute("view"))
		{
			switch (reader.Value)
			{
			case "pageLayout":
				worksheetImpl.View = OfficeSheetView.PageLayout;
				break;
			case "pageBreakPreview":
				worksheetImpl.View = OfficeSheetView.PageBreakPreview;
				worksheetImpl.WindowTwo.IsSavedInPageBreakPreview = true;
				break;
			case "normal":
				worksheetImpl.View = OfficeSheetView.Normal;
				break;
			}
		}
		if (reader.MoveToAttribute("tabSelected") && reader.Value != "0")
		{
			sheetBase.Select();
		}
		reader.MoveToElement();
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				string localName = reader.LocalName;
				if (!(localName == "pane"))
				{
					if (localName == "selection")
					{
						ParseSelection(reader, worksheetImpl);
					}
					else
					{
						reader.Skip();
					}
				}
				else if (reader.IsEmptyElement && !reader.HasAttributes)
				{
					reader.Read();
				}
				else
				{
					ParsePane(reader, worksheetImpl);
				}
			}
		}
		reader.Read();
	}

	private void ParseSelection(XmlReader reader, WorksheetImpl sheet)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "selection")
		{
			throw new XmlException();
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (reader.MoveToAttribute("pane"))
		{
			sheet.Pane.ActivePane = (ushort)GetPaneType(reader.Value);
		}
		if (reader.MoveToAttribute("activeCell"))
		{
			string value = reader.Value;
			sheet.SetActiveCell(sheet.Range[value], updateApplication: false);
		}
		reader.MoveToElement();
		reader.Skip();
	}

	private Pane.ActivePane GetPaneType(string value)
	{
		return Pane.PaneStrings[value];
	}

	private void ParsePane(XmlReader reader, WorksheetImpl sheet)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (reader.LocalName != "pane")
		{
			throw new XmlException("Wrong xml tag");
		}
		if (reader.MoveToAttribute("xSplit"))
		{
			sheet.VerticalSplit = XmlConvertExtension.ToInt32(reader.Value);
		}
		if (reader.MoveToAttribute("ySplit"))
		{
			sheet.HorizontalSplit = (int)XmlConvertExtension.ToDouble(reader.Value);
		}
		if (reader.MoveToAttribute("topLeftCell"))
		{
			string value = reader.Value;
			sheet.PaneFirstVisible = sheet[value];
		}
		if (reader.MoveToAttribute("activePane"))
		{
			Pane.ActivePane activePane = (Pane.ActivePane)Enum.Parse(typeof(Pane.ActivePane), reader.Value, ignoreCase: false);
			sheet.ActivePane = (int)activePane;
		}
		if (reader.MoveToAttribute("state"))
		{
			WindowTwoRecord windowTwo = sheet.WindowTwo;
			ParsePaneState(windowTwo, reader.Value);
		}
	}

	private void ParsePaneState(WindowTwoRecord windowTwo, string state)
	{
		if (windowTwo == null)
		{
			throw new ArgumentNullException("windowTwo");
		}
		if (state == null)
		{
			throw new ArgumentNullException("state");
		}
		switch (state)
		{
		case "frozen":
			windowTwo.IsFreezePanes = true;
			windowTwo.IsFreezePanesNoSplit = true;
			break;
		case "frozenSplit":
			windowTwo.IsFreezePanes = true;
			windowTwo.IsFreezePanesNoSplit = false;
			break;
		case "split":
			windowTwo.IsFreezePanes = false;
			windowTwo.IsFreezePanesNoSplit = false;
			break;
		default:
			throw new XmlException();
		}
	}

	public void ParseChartsheet(XmlReader reader, ChartImpl chart)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.LocalName != "chartsheet")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		PageSetupBaseImpl pageSetupBase = chart.PageSetupBase;
		pageSetupBase.IsNotValidSettings = true;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "sheetPr":
					ParseSheetLevelProperties(reader, chart);
					break;
				case "sheetViews":
					ParseSheetViews(reader, chart);
					break;
				case "pageMargins":
				{
					bool isNotValidSettings = pageSetupBase.IsNotValidSettings;
					ParsePageMargins(reader, chart.PageSetup, new WorksheetPageSetupConstants());
					pageSetupBase.IsNotValidSettings = isNotValidSettings;
					break;
				}
				case "pageSetup":
					ParsePageSetup(reader, chart.PageSetupBase);
					break;
				case "headerFooter":
				{
					bool isNotValidSettings = pageSetupBase.IsNotValidSettings;
					ParseHeaderFooter(reader, chart.PageSetupBase);
					pageSetupBase.IsNotValidSettings = isNotValidSettings;
					break;
				}
				case "drawing":
					ParseChartDrawing(reader, chart);
					break;
				case "legacyDrawing":
					ParseLegacyDrawing(reader, chart);
					break;
				case "legacyDrawingHF":
					ParseLegacyDrawingHF(reader, chart, null);
					break;
				case "sheetProtection":
					ParseSheetProtection(reader, chart, "content");
					break;
				default:
					reader.Skip();
					break;
				}
			}
			else
			{
				reader.Skip();
			}
		}
	}

	private void ParseChartDrawing(XmlReader reader, ChartImpl chart)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "drawing")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		if (!reader.MoveToAttribute("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships"))
		{
			throw new XmlException();
		}
		bool flag = false;
		string value = reader.Value;
		Relation relation = chart.m_dataHolder.Relations[value];
		chart.m_dataHolder.Relations.Remove(value);
		if (relation == null)
		{
			throw new XmlException();
		}
		_ = relation.Target;
		FileDataHolder parentHolder = chart.m_dataHolder.ParentHolder;
		string strItemPath = chart.m_dataHolder.ArchiveItem.ItemName;
		FileDataHolder.SeparateItemName(strItemPath, out var path);
		XmlReader xmlReader = parentHolder.CreateReader(relation, path, out strItemPath);
		string correspondingRelations = FileDataHolder.GetCorrespondingRelations(strItemPath);
		RelationCollection relations = parentHolder.ParseRelations(correspondingRelations);
		while (xmlReader.LocalName != "chart")
		{
			if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.LocalName == "absoluteAnchor")
			{
				Size size = ParseAbsoluteAnchorExtent(xmlReader);
				chart.Width = size.Width;
				chart.Height = size.Height;
			}
			else if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.LocalName == "AlternateContent")
			{
				xmlReader.Read();
				SkipWhiteSpaces(xmlReader);
				if (xmlReader.LocalName == "Choice" && IsChartExChoice(xmlReader))
				{
					flag = true;
				}
			}
			else
			{
				xmlReader.Read();
			}
		}
		ParseChartTag(xmlReader, chart, relations, parentHolder, strItemPath, flag);
		if (flag)
		{
			TryRemoveChartSheetFallBackRelations(xmlReader, chart, strItemPath, parentHolder, relations);
		}
	}

	private void TryRemoveChartSheetFallBackRelations(XmlReader reader, ChartImpl chart, string drawingItemName, FileDataHolder holder, RelationCollection relations)
	{
		bool flag = false;
		bool flag2 = false;
		while (reader.NodeType != 0 && !flag2)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName = reader.LocalName;
				if (!(localName == "Fallback"))
				{
					if (localName == "graphicData")
					{
						if (flag)
						{
							reader.Read();
							SkipWhiteSpaces(reader);
							if (reader.LocalName == "chart")
							{
								if (!reader.MoveToAttribute("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships"))
								{
									throw new XmlException();
								}
								string value = reader.Value;
								Relation relation = relations[value];
								FileDataHolder.SeparateItemName(drawingItemName, out var path);
								holder.GetItem(relation, path, out var strItemPath);
								string correspondingRelations = FileDataHolder.GetCorrespondingRelations(strItemPath);
								RelationCollection relationCollection = holder.ParseRelations(correspondingRelations);
								ZipArchive archive = holder.Archive;
								foreach (KeyValuePair<string, Relation> item in relationCollection)
								{
									holder.GetItem(item.Value, path, out var strItemPath2);
									string correspondingRelations2 = FileDataHolder.GetCorrespondingRelations(strItemPath2);
									archive.RemoveItem(strItemPath2);
									archive.RemoveItem(correspondingRelations2);
								}
								relationCollection.Clear();
								archive.RemoveItem(strItemPath);
								archive.RemoveItem(correspondingRelations);
								relations.Remove(value);
							}
							flag2 = true;
						}
						else
						{
							reader.Read();
						}
					}
					else
					{
						reader.Read();
					}
				}
				else
				{
					flag = true;
					reader.Read();
				}
			}
			else
			{
				reader.Read();
			}
		}
	}

	private bool IsChartExChoice(XmlReader reader)
	{
		bool result = false;
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "Choice")
		{
			throw new XmlException("Unexpected xml tag");
		}
		if (reader.MoveToAttribute("Requires"))
		{
			string value = reader.Value;
			if (reader.MoveToAttribute("xmlns:" + value) && reader.Value.ToLower().Contains("chartex"))
			{
				result = true;
			}
		}
		return result;
	}

	private Size ParseAbsoluteAnchorExtent(XmlReader reader)
	{
		while (reader.LocalName != "ext")
		{
			reader.Read();
		}
		return ParseExtent(reader);
	}

	private void ParseChartTag(XmlReader reader, ChartImpl chart, RelationCollection relations, FileDataHolder dataHolder, string itemName, bool isChartEx)
	{
		if (!reader.MoveToAttribute("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships"))
		{
			throw new XmlException();
		}
		string value = reader.Value;
		Relation relation = relations[value];
		FileDataHolder.SeparateItemName(itemName, out var path);
		Relation relation2 = null;
		if (relation == null)
		{
			return;
		}
		string strItemPath;
		XmlReader xmlReader = dataHolder.CreateReader(relation, path, out strItemPath);
		string correspondingRelations = FileDataHolder.GetCorrespondingRelations(strItemPath);
		RelationCollection relationCollection = dataHolder.ParseRelations(correspondingRelations);
		if (relationCollection != null)
		{
			relationCollection.RemoveByContentType("http://schemas.microsoft.com/office/2011/relationships/chartColorStyle");
			relationCollection.RemoveByContentType("http://schemas.microsoft.com/office/2011/relationships/chartStyle");
			IEnumerator enumerator = relationCollection.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, Relation> keyValuePair = (KeyValuePair<string, Relation>)enumerator.Current;
				chart.Relations[keyValuePair.Key] = keyValuePair.Value;
				if (keyValuePair.Value.Type == "http://schemas.openxmlformats.org/officeDocument/2006/relationships/themeOverride")
				{
					relation2 = keyValuePair.Value;
				}
			}
			chart.Relations.ItemPath = relationCollection.ItemPath;
		}
		if (relation2 != null)
		{
			xmlReader = dataHolder.CreateReader(relation2, "xl/workbook.xml", out var _);
			while (xmlReader.NodeType != XmlNodeType.Element)
			{
				xmlReader.Read();
			}
			if (xmlReader.LocalName == "themeOverride")
			{
				xmlReader.Read();
				while (xmlReader.NodeType != XmlNodeType.EndElement)
				{
					if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.LocalName == "clrScheme")
					{
						MemoryStream memoryStream = new MemoryStream();
						XmlWriter xmlWriter = UtilityMethods.CreateWriter(memoryStream, Encoding.UTF8);
						xmlWriter.WriteNode(xmlReader, defattr: false);
						xmlWriter.Flush();
						chart.m_themeOverrideStream = memoryStream;
					}
					else
					{
						xmlReader.Skip();
					}
				}
			}
		}
		if (isChartEx)
		{
			new ChartExParser(m_book).ParseChartEx(xmlReader, chart, relationCollection);
		}
		else
		{
			new ChartParser(m_book).ParseChart(xmlReader, chart, relationCollection);
		}
		dataHolder.Archive.RemoveItem(itemName);
		relations.Remove(value);
	}

	private void ExtractDefaultRowHeight(XmlReader reader, WorksheetImpl sheet)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (reader.MoveToAttribute("defaultColWidth"))
		{
			double num2 = (sheet.DefaultColumnWidth = XmlConvertExtension.ToDouble(reader.Value));
			num2 = (double)(int)Math.Round(num2 * 256.0) / 256.0;
			sheet.StandardWidth = ((num2 > 0.0) ? num2 : sheet.StandardWidth);
			double defaultWidth = DocGen.Drawing.Helper.ParseDouble(reader.Value);
			SetDefaultColumnWidth(defaultWidth, bool_2: false, sheet);
		}
		if (reader.MoveToAttribute("defaultRowHeight"))
		{
			sheet.StandardHeight = XmlConvertExtension.ToDouble(reader.Value);
		}
		sheet.CustomHeight = false;
		if (reader.MoveToAttribute("customHeight"))
		{
			sheet.CustomHeight = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("outlineLevelCol"))
		{
			sheet.OutlineLevelColumn = XmlConvert.ToByte(reader.Value);
		}
		if (reader.MoveToAttribute("outlineLevelRow"))
		{
			sheet.OutlineLevelRow = XmlConvertExtension.ToByte(reader.Value);
		}
		if (reader.MoveToAttribute("baseColWidth"))
		{
			sheet.BaseColumnWidth = XmlConvertExtension.ToInt16(reader.Value);
		}
		if (reader.MoveToAttribute("thickBottom"))
		{
			sheet.IsThickBottom = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("thickTop"))
		{
			sheet.IsThickTop = XmlConvertExtension.ToBoolean(reader.Value);
		}
		reader.MoveToElement();
	}

	private void SetDefaultColumnWidth(double defaultWidth, bool bool_2, WorksheetImpl worksheet)
	{
		if (Math.Abs(defaultWidth - 0.0) < 0.0001)
		{
			worksheet.Columnss.Width = defaultWidth;
			return;
		}
		int fontCalc = worksheet.GetAppImpl().GetFontCalc2();
		double num = defaultWidth * (double)fontCalc;
		if (bool_2)
		{
			num += 10.0;
		}
		if (num > 5.0)
		{
			worksheet.Columnss.Width = (num - 5.0) / (double)fontCalc;
			return;
		}
		worksheet.Columnss.Width = 0.0;
		ColumnCollection columnss = worksheet.Columnss;
		if (columnss.column == null)
		{
			columnss.GetOrCreateColumn().SetWidth((int)(num + 0.5));
		}
	}

	private void ExtractZeroHeight(XmlReader reader, WorksheetImpl sheet)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (reader.MoveToAttribute("zeroHeight"))
		{
			sheet.IsZeroHeight = XmlConvertExtension.ToBoolean(reader.Value);
			(sheet.PageSetup as PageSetupImpl).DefaultRowHeightFlag = false;
		}
		reader.MoveToElement();
	}

	public void ParseMergedCells(XmlReader reader, WorksheetImpl sheet)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (reader.NodeType != XmlNodeType.Element || !(reader.LocalName == "mergeCells"))
		{
			return;
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				ParseMergeRegion(reader, sheet);
			}
			else
			{
				reader.Read();
			}
		}
		reader.Read();
	}

	public void ParseNamedRanges(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.NodeType != XmlNodeType.Element || !(reader.LocalName == "definedNames"))
		{
			return;
		}
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			List<string> list = new List<string>();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					string item = ParseNamedRange(reader);
					list.Add(item);
				}
				reader.Read();
			}
			INames names = m_book.Names;
			m_book.AppImplementation.IsFormulaParsed = false;
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				NameImpl nameImpl = (NameImpl)names[i];
				if (list[i].LastIndexOf('!') == 0)
				{
					nameImpl.IsCommon = true;
					if (names[i].Scope == "Workbook" && m_book.ActiveSheet != null)
					{
						list[i] = m_book.ActiveSheet.Name + list[i];
					}
					else
					{
						list[i] = names[i].Scope + list[i];
					}
				}
				nameImpl.SetValue(m_formulaUtil.ParseString(list[i]));
			}
		}
		m_book.AppImplementation.IsFormulaParsed = true;
		reader.Read();
	}

	public List<int> ParseStyles(XmlReader reader, ref Stream streamDxfs)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.LocalName != "styleSheet")
		{
			throw new XmlException("Unexpected xml tag " + reader.LocalName);
		}
		List<int> list = null;
		List<BordersCollection> arrBorders = null;
		List<FillImpl> arrFills = null;
		List<int> list2 = null;
		List<int> result = null;
		Dictionary<int, int> arrNumberFormatIndexes = null;
		reader.Read();
		if (reader.NodeType == XmlNodeType.None)
		{
			m_book.InsertDefaultFonts();
			m_book.InsertDefaultValues();
			return null;
		}
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			bool flag = true;
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "numFmts":
					arrNumberFormatIndexes = ParseNumberFormats(reader);
					break;
				case "fonts":
					list = ParseFonts(reader);
					break;
				case "fills":
					arrFills = ParseFills(reader);
					break;
				case "borders":
					arrBorders = ParseBorders(reader);
					break;
				case "cellStyleXfs":
					list2 = ParseNamedStyles(reader, list, arrFills, arrBorders, arrNumberFormatIndexes);
					break;
				case "cellXfs":
					result = ParseCellFormats(reader, list, arrFills, arrBorders, list2, arrNumberFormatIndexes);
					break;
				case "cellStyles":
					ParseStyles(reader, list2);
					break;
				case "dxfs":
				{
					streamDxfs = new MemoryStream();
					XmlWriter xmlWriter = UtilityMethods.CreateWriter(new StreamWriter(streamDxfs));
					xmlWriter.WriteNode(reader, defattr: false);
					xmlWriter.Flush();
					flag = false;
					break;
				}
				case "tableStyles":
					m_book.CustomTableStylesStream = ShapeParser.ReadNodeAsStream(reader);
					flag = false;
					break;
				case "colors":
					ParseColors(reader);
					break;
				case "extLst":
					ParseBookExtensions(reader);
					flag = false;
					break;
				default:
					throw new NotImplementedException(reader.LocalName);
				}
			}
			if (flag)
			{
				reader.Read();
			}
		}
		if (m_book.InnerStyles.Count == 0)
		{
			List<StyleRecord> arrStyles = new List<StyleRecord>();
			m_book.PrepareStyles(bIgnoreStyles: false, arrStyles, null);
		}
		ExtendedFormatImpl format = ((ExtendedFormatWrapper)m_book.InnerStyles["Normal"]).Wrapped.CreateChildFormat();
		format = m_book.InnerExtFormats.Add(format);
		m_book.DefaultXFIndex = format.Index;
		return result;
	}

	private void ParseBookExtensions(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "extLst")
		{
			throw new XmlException();
		}
		Stream extensionStream = ShapeParser.ReadNodeAsStream(reader, writeNamespaces: true);
		m_book.DataHolder.ExtensionStream = extensionStream;
	}

	public Dictionary<int, int> ParseSST(XmlReader reader, bool parseOnDemand)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.LocalName != "sst")
		{
			throw new XmlException("reader");
		}
		if (reader.IsEmptyElement)
		{
			return null;
		}
		m_book.SSTStream = ShapeParser.ReadNodeAsStream(reader);
		if (parseOnDemand)
		{
			m_book.ParseOnDemand = parseOnDemand;
		}
		m_book.SSTStream.Position = 0L;
		XmlReader xmlReader = UtilityMethods.CreateReader(m_book.SSTStream);
		xmlReader.Read();
		int num = 0;
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		SSTDictionary innerSST = m_book.InnerSST;
		while (xmlReader.NodeType != XmlNodeType.EndElement && xmlReader.NodeType != 0)
		{
			if (xmlReader.LocalName == "si")
			{
				int num2 = ParseStringItem(xmlReader);
				if (num != num2)
				{
					dictionary[num] = num2;
				}
				num++;
			}
			xmlReader.Skip();
		}
		innerSST.UpdateRefCounts();
		return dictionary;
	}

	public int ParseStringItem(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "si" && reader.LocalName != "is")
		{
			throw new XmlException("reader");
		}
		bool setCount = reader.LocalName == "is";
		int result = -1;
		reader.Read();
		if (reader.IsEmptyElement)
		{
			result = m_book.InnerSST.AddIncrease(string.Empty, bIncrease: false);
			reader.Skip();
		}
		else
		{
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				string localName = reader.LocalName;
				if (!(localName == "t"))
				{
					if (localName == "r")
					{
						result = ParseRichTextRun(reader);
					}
					else
					{
						reader.Skip();
					}
				}
				else
				{
					result = ParseText(reader, setCount);
				}
			}
		}
		return result;
	}

	public int ParseStringItem(XmlReader reader, out string text)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "si" && reader.LocalName != "is")
		{
			throw new XmlException("reader");
		}
		bool setCount = reader.LocalName == "is";
		int result = -1;
		reader.Read();
		text = string.Empty;
		if (reader.IsEmptyElement)
		{
			result = m_book.InnerSST.AddIncrease(string.Empty, bIncrease: false);
			reader.Skip();
		}
		else
		{
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				string localName = reader.LocalName;
				if (!(localName == "t"))
				{
					if (localName == "r")
					{
						result = ParseRichTextRun(reader);
					}
					else
					{
						reader.Skip();
					}
				}
				else
				{
					result = ParseText(reader, setCount, out text);
				}
			}
		}
		return result;
	}

	public void ParseVmlShapes(XmlReader reader, ShapeCollectionBase shapes, RelationCollection relations, string parentItemPath)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (shapes == null)
		{
			throw new ArgumentNullException("shapes");
		}
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
			if (reader.NodeType == XmlNodeType.None)
			{
				break;
			}
		}
		if (reader.LocalName != "xml")
		{
			throw new XmlException("Unexpected tag");
		}
		reader.Read();
		Dictionary<string, ShapeImpl> dictShapeIdToShape = new Dictionary<string, ShapeImpl>();
		Stream stream = null;
		while (reader.NodeType != XmlNodeType.EndElement && !reader.EOF)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "shapetype":
					ParseShapeType(reader, shapes, dictShapeIdToShape, stream);
					break;
				case "shape":
					if (reader.MoveToAttribute("type"))
					{
						ParseShape(reader, dictShapeIdToShape, relations, parentItemPath);
					}
					else
					{
						ParseShapeWithoutType(reader, shapes, relations, parentItemPath);
					}
					break;
				case "shapelayout":
					stream = ShapeParser.ReadNodeAsStream(reader);
					stream.Position = 0L;
					break;
				default:
					reader.Skip();
					break;
				}
			}
			else
			{
				reader.Read();
			}
		}
	}

	public RelationCollection ParseRelations(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		RelationCollection relationCollection = new RelationCollection();
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.LocalName != "Relationships")
		{
			throw new XmlException("Unexpected tag " + reader.LocalName);
		}
		reader.Read();
		if (reader.NodeType != 0)
		{
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					if (!(reader.LocalName == "Relationship"))
					{
						throw new XmlException("Unexpected tag " + reader.Value);
					}
					ParseRelation(reader, relationCollection);
				}
				reader.Read();
			}
		}
		return relationCollection;
	}

	public Dictionary<string, string> ParseSheetData(XmlReader reader, IInternalWorksheet sheet, List<int> arrStyles, string cellTag)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (reader.LocalName != "sheetData")
		{
			throw new XmlException("reader");
		}
		reader.MoveToElement();
		Dictionary<string, string> dictionary = null;
		if (m_outlineLevels == null)
		{
			m_outlineLevels = new Dictionary<int, List<Point>>();
		}
		if (sheet is WorksheetImpl && (sheet as WorksheetImpl).RowOutlineLevels == null)
		{
			(sheet as WorksheetImpl).RowOutlineLevels = new Dictionary<int, List<Point>>();
		}
		if (reader.MoveToFirstAttribute())
		{
			dictionary = new Dictionary<string, string>();
			dictionary.Add(reader.LocalName, reader.Value);
			while (reader.MoveToNextAttribute())
			{
				dictionary.Add(reader.LocalName, reader.Value);
			}
			reader.MoveToElement();
		}
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			int generatedRowIndex = 1;
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.LocalName == "row")
				{
					generatedRowIndex = ParseRow(reader, sheet, arrStyles, cellTag, generatedRowIndex);
					generatedRowIndex++;
				}
				reader.Skip();
			}
		}
		reader.Read();
		if (sheet is WorksheetImpl && m_indexAndLevels != null && m_indexAndLevels.Count > 0)
		{
			m_indexAndLevels = null;
			m_outlineLevels = null;
		}
		return dictionary;
	}

	public void ParseComments(XmlReader reader, WorksheetImpl sheet)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		do
		{
			reader.Read();
		}
		while (reader.NodeType != XmlNodeType.Element);
		if (reader.LocalName == "comments")
		{
			reader.Read();
		}
		List<string> arrAuthors = null;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName = reader.LocalName;
				if (!(localName == "authors"))
				{
					if (!(localName == "commentList"))
					{
						throw new XmlException("Unexpected xml tag.");
					}
					ParseCommentList(reader, arrAuthors, sheet);
				}
				else
				{
					arrAuthors = ParseAuthors(reader);
				}
			}
			else
			{
				reader.Skip();
			}
		}
	}

	internal void ParseDrawings(XmlReader reader, WorksheetBaseImpl sheet, string drawingsPath, List<string> lstRelationIds, Dictionary<string, object> dictItemsToRemove, bool isChartShape)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (lstRelationIds == null)
		{
			throw new ArgumentNullException("lstRelationdIds");
		}
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.LocalName != "wsDr" && reader.LocalName != "userShapes")
		{
			throw new XmlException("Unexpected xml tag " + reader.LocalName);
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		reader.Read();
		if (reader.NodeType == XmlNodeType.None)
		{
			return;
		}
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName = reader.LocalName;
				m_drawingParser = new DrawingParser();
				m_drawingParser.anchorName = localName;
				switch (localName)
				{
				case "twoCellAnchor":
				case "oneCellAnchor":
				case "relSizeAnchor":
					ParseTwoCellAnchor(reader, sheet, drawingsPath, lstRelationIds, dictItemsToRemove, isChartShape);
					break;
				case "AlternateContent":
					ParseAlternateContent(reader, sheet, drawingsPath, lstRelationIds, dictItemsToRemove);
					break;
				case "absoluteAnchor":
				{
					double num = 0.0;
					double num2 = 0.0;
					double num3 = 0.0;
					double num4 = 0.0;
					MemoryStream data = null;
					while (reader.LocalName != "graphicData" && reader.LocalName != "pic" && reader.LocalName != "sp" && reader.LocalName != "cxnSp" && reader.LocalName != "grpSp")
					{
						if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "absoluteAnchor")
						{
							while (reader.LocalName != "ext")
							{
								if (reader.LocalName == "pos")
								{
									if (reader.MoveToAttribute("x"))
									{
										num = Math.Max(0, reader.ReadContentAsInt());
									}
									if (reader.MoveToAttribute("y"))
									{
										num2 = Math.Max(0, reader.ReadContentAsInt());
									}
									m_drawingParser.posX = DocGen.Drawing.Helper.ConvertEmuToOffset((int)num, dpiX);
									m_drawingParser.posY = DocGen.Drawing.Helper.ConvertEmuToOffset((int)num2, dpiX);
								}
								reader.Read();
							}
							if (reader.MoveToAttribute("cx"))
							{
								num3 = int.Parse(reader.Value);
							}
							if (reader.MoveToAttribute("cy"))
							{
								num4 = int.Parse(reader.Value);
							}
							m_drawingParser.extCX = DocGen.Drawing.Helper.ConvertEmuToOffset((int)num3, dpiX);
							m_drawingParser.extCY = DocGen.Drawing.Helper.ConvertEmuToOffset((int)num4, dpiX);
							continue;
						}
						if (reader.LocalName == "AlternateContent")
						{
							ShapeImpl shapeImpl = CreateShape(reader, sheet, ref data, drawingsPath, lstRelationIds);
							shapeImpl.XmlDataStream = data;
							shapeImpl.IsEquationShape = true;
							if (m_enableAlternateContent)
							{
								shapeImpl.EnableAlternateContent = m_enableAlternateContent;
							}
							shapeImpl.IsAbsoluteAnchor = true;
							shapeImpl.Left = (int)ApplicationImpl.ConvertToPixels(num, MeasureUnits.EMU);
							shapeImpl.Top = (int)ApplicationImpl.ConvertToPixels(num2, MeasureUnits.EMU);
							shapeImpl.Width = (int)ApplicationImpl.ConvertToPixels(num3, MeasureUnits.EMU);
							shapeImpl.Height = (int)ApplicationImpl.ConvertToPixels(num4, MeasureUnits.EMU);
							break;
						}
						reader.Read();
					}
					if (reader.LocalName == "pic")
					{
						ShapeImpl shapeImpl2 = ParsePicture(reader, sheet, drawingsPath, lstRelationIds, dictItemsToRemove);
						shapeImpl2.IsAbsoluteAnchor = true;
						shapeImpl2.Left = (int)ApplicationImpl.ConvertToPixels(num, MeasureUnits.EMU);
						shapeImpl2.Top = (int)ApplicationImpl.ConvertToPixels(num2, MeasureUnits.EMU);
						shapeImpl2.Width = (int)ApplicationImpl.ConvertToPixels(num3, MeasureUnits.EMU);
						shapeImpl2.Height = (int)ApplicationImpl.ConvertToPixels(num4, MeasureUnits.EMU);
					}
					else if (reader.LocalName == "graphicData" || reader.LocalName == "sp" || reader.LocalName == "grpSp")
					{
						MemoryStream memoryStream = ReadSingleNodeIntoStream(reader);
						ShapeImpl shapeImpl3 = TryParseChart(memoryStream, sheet, drawingsPath, isChartEx: false);
						if (shapeImpl3 == null)
						{
							shapeImpl3 = new ShapeImpl(sheet.Application, sheet.InnerShapes);
							sheet.InnerShapes.AddShape(shapeImpl3);
							shapeImpl3.IsAbsoluteAnchor = true;
							if (shapeImpl3.preservedPictureStreams == null)
							{
								shapeImpl3.preservedPictureStreams = new List<Stream>();
							}
							shapeImpl3.preservedPictureStreams.Add(memoryStream);
							shapeImpl3.Left = (int)ApplicationImpl.ConvertToPixels(num, MeasureUnits.EMU);
							shapeImpl3.Top = (int)ApplicationImpl.ConvertToPixels(num2, MeasureUnits.EMU);
							shapeImpl3.Width = (int)ApplicationImpl.ConvertToPixels(num3, MeasureUnits.EMU);
							shapeImpl3.Height = (int)ApplicationImpl.ConvertToPixels(num4, MeasureUnits.EMU);
						}
						else
						{
							shapeImpl3.IsAbsoluteAnchor = true;
							memoryStream = null;
							(shapeImpl3 as ChartShapeImpl).ChartObject.EMUWidth = num3;
							(shapeImpl3 as ChartShapeImpl).ChartObject.EMUHeight = num4;
							(shapeImpl3 as IOfficeChart).XPos = num;
							(shapeImpl3 as IOfficeChart).YPos = num2;
						}
					}
					else if (reader.LocalName == "cxnSp")
					{
						m_drawingParser.preFix = reader.Prefix;
						m_drawingParser.shapeType = reader.LocalName;
						ShapeImpl shapeImpl4 = CreateShape(reader, sheet, ref data, drawingsPath, lstRelationIds);
						shapeImpl4.Left = (int)ApplicationImpl.ConvertToPixels(num, MeasureUnits.EMU);
						shapeImpl4.Top = (int)ApplicationImpl.ConvertToPixels(num2, MeasureUnits.EMU);
						shapeImpl4.Width = (int)ApplicationImpl.ConvertToPixels(num3, MeasureUnits.EMU);
						shapeImpl4.Height = (int)ApplicationImpl.ConvertToPixels(num4, MeasureUnits.EMU);
					}
					while (reader.LocalName != "absoluteAnchor")
					{
						reader.Read();
					}
					reader.Read();
					break;
				}
				default:
					reader.Skip();
					break;
				}
			}
			else
			{
				reader.Skip();
			}
		}
	}

	private void ParseAlternateContent(XmlReader reader, WorksheetBaseImpl sheet, string drawingsPath, List<string> lstRelationIds, Dictionary<string, object> dictItemsToRemove)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (lstRelationIds == null)
		{
			throw new ArgumentNullException("lstRelationIds");
		}
		reader.Read();
		m_enableAlternateContent = true;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				if (reader.LocalName == "Choice")
				{
					ParseChoice(reader, sheet, drawingsPath, lstRelationIds, dictItemsToRemove);
				}
				else
				{
					reader.Skip();
				}
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Read();
	}

	private void ParseAlternateContent(XmlReader reader, WorksheetImpl sheet)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		sheet.HasAlternateContent = true;
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				if (reader.LocalName == "Choice")
				{
					ParseChoice(reader, sheet);
				}
				else
				{
					reader.Skip();
				}
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Read();
	}

	private void ParseChoice(XmlReader reader, WorksheetBaseImpl sheet, string drawingsPath, List<string> lstRelationIds, Dictionary<string, object> dictItemsToRemove)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (lstRelationIds == null)
		{
			throw new ArgumentNullException("lstRelationIds");
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "twoCellAnchor":
				case "oneCellAnchor":
				case "relSizeAnchor":
					ParseTwoCellAnchor(reader, sheet, drawingsPath, lstRelationIds, dictItemsToRemove, isChartShape: false);
					break;
				default:
					reader.Skip();
					break;
				}
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Read();
	}

	private void ParseChoice(XmlReader reader, WorksheetImpl sheet)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				if (reader.LocalName == "controls")
				{
					ParseControls(reader, sheet);
				}
				else
				{
					reader.Skip();
				}
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Read();
	}

	internal static Stream ParseMiscSheetElements(XmlReader reader)
	{
		Stream stream = new MemoryStream();
		XmlWriter xmlWriter = UtilityMethods.CreateWriter(stream, Encoding.UTF8);
		xmlWriter.WriteStartElement("root", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
		while (!reader.EOF)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "mergeCells":
				case "autoFilter":
				case "hyperlinks":
				case "phoneticPr":
				case "oleObjects":
				case "ignoredErrors":
				case "legacyDrawing":
				case "dataValidations":
				case "legacyDrawingHF":
				case "sheetProtection":
				case "drawing":
				case "picture":
				case "headerFooter":
				case "printOptions":
				case "pageSetup":
				case "rowBreaks":
				case "colBreaks":
				case "sortState":
				case "AlternateContent":
				case "customProperties":
				case "conditionalFormatting":
				case "pageMargins":
				case "controls":
				case "extLst":
					xmlWriter.WriteNode(reader, defattr: false);
					break;
				default:
					reader.Skip();
					break;
				}
			}
			else
			{
				reader.Skip();
			}
		}
		xmlWriter.WriteEndElement();
		xmlWriter.Flush();
		stream.Position = 0L;
		return stream;
	}

	private void ParseExtensionlist(XmlReader reader, WorksheetImpl sheet)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (reader.LocalName != "extLst")
		{
			throw new XmlException();
		}
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					if (reader.LocalName == "ext")
					{
						ParseExt(sheet, reader);
					}
					else
					{
						reader.Skip();
					}
				}
				else
				{
					reader.Skip();
				}
			}
		}
		reader.Read();
	}

	private void ParseExt(WorksheetImpl sheet, XmlReader reader)
	{
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "ext")
		{
			throw new XmlException();
		}
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					string localName = reader.LocalName;
					if (!(localName == "slicerList"))
					{
						if (localName == "conditionalFormattings")
						{
							sheet.DataHolder.m_cfsStream = new MemoryStream();
							sheet.DataHolder.m_cfsStream = ShapeParser.ReadNodeAsStream(reader);
						}
						else
						{
							reader.Skip();
						}
					}
					else
					{
						Stream worksheetSlicerStream = ShapeParser.ReadNodeAsStream(reader);
						sheet.WorksheetSlicerStream = worksheetSlicerStream;
					}
				}
				else
				{
					reader.Skip();
				}
			}
		}
		reader.Read();
	}

	private void ParseControls(XmlReader reader, WorksheetImpl sheet)
	{
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "controls")
		{
			throw new XmlException();
		}
		WorksheetDataHolder dataHolder = sheet.DataHolder;
		MemoryStream memoryStream = new MemoryStream();
		XmlWriter xmlWriter = UtilityMethods.CreateWriter(memoryStream, Encoding.UTF8);
		xmlWriter.WriteNode(reader, defattr: false);
		xmlWriter.Flush();
		dataHolder.ControlsStream = memoryStream;
	}

	private void ParseSheetProtection(XmlReader reader, WorksheetBaseImpl sheet, string protectContentTag)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (reader.LocalName != "sheetProtection")
		{
			throw new XmlException();
		}
		OfficeSheetProtection officeSheetProtection = OfficeSheetProtection.None;
		ushort num = 0;
		if (reader.MoveToAttribute("password"))
		{
			num = ushort.Parse(reader.Value, NumberStyles.AllowHexSpecifier);
		}
		if (reader.MoveToAttribute("algorithmName"))
		{
			sheet.AlgorithmName = reader.Value;
		}
		if (reader.MoveToAttribute("saltValue"))
		{
			sheet.SaltValue = Convert.FromBase64String(reader.Value);
		}
		if (reader.MoveToAttribute("hashValue"))
		{
			sheet.HashValue = Convert.FromBase64String(reader.Value);
		}
		if (reader.MoveToAttribute("spinCount"))
		{
			sheet.SpinCount = Convert.ToUInt32(reader.Value);
		}
		bool protectContents = false;
		if (reader.MoveToAttribute(protectContentTag))
		{
			protectContents = XmlConvertExtension.ToBoolean(reader.Value);
			if (num == 0)
			{
				num = 1;
			}
		}
		string[] array = Protection.ProtectionAttributes;
		ChecProtectionDelegate checProtectionDelegate = CheckProtectionAttribute;
		if (sheet is ChartImpl)
		{
			array = Protection.ChartProtectionAttributes;
			checProtectionDelegate = CheckChartProtectionAttribute;
		}
		int i = 0;
		for (int num2 = array.Length; i < num2; i++)
		{
			officeSheetProtection = checProtectionDelegate(reader, array[i], Protection.ProtectionFlags[i], Protection.DefaultValues[i], officeSheetProtection);
		}
		sheet.Protect(num, officeSheetProtection);
		sheet.ProtectContents = protectContents;
		reader.Read();
	}

	private OfficeSheetProtection CheckChartProtectionAttribute(XmlReader reader, string attributeName, OfficeSheetProtection flag, bool defaultValue, OfficeSheetProtection protection)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (attributeName == null || attributeName.Length == 0)
		{
			throw new ArgumentOutOfRangeException("attributeName");
		}
		bool flag2 = defaultValue;
		if (reader.MoveToAttribute(attributeName))
		{
			flag2 = XmlConvertExtension.ToBoolean(reader.Value);
		}
		protection = ((!flag2) ? (protection & ~flag) : (protection | flag));
		return protection;
	}

	private OfficeSheetProtection CheckProtectionAttribute(XmlReader reader, string attributeName, OfficeSheetProtection flag, bool defaultValue, OfficeSheetProtection protection)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (attributeName == null || attributeName.Length == 0)
		{
			throw new ArgumentOutOfRangeException("attributeName");
		}
		bool flag2 = defaultValue;
		if (reader.MoveToAttribute(attributeName))
		{
			flag2 = XmlConvertExtension.ToBoolean(reader.Value);
		}
		protection = (flag2 ? (protection & ~flag) : (protection | flag));
		return protection;
	}

	private void ParseIgnoreError(XmlReader reader, WorksheetImpl sheet)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (reader.LocalName != "ignoredErrors")
		{
			throw new XmlException();
		}
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "ignoredError")
				{
					ExtractIgnoredError(reader, sheet);
				}
				else
				{
					reader.Read();
				}
			}
		}
		reader.Read();
	}

	private void ExtractIgnoredError(XmlReader reader, WorksheetImpl sheet)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (reader.LocalName != "ignoredError")
		{
			throw new XmlException();
		}
		ExcelIgnoreError excelIgnoreError = ExcelIgnoreError.None;
		string text = null;
		int i = 0;
		for (int attributeCount = reader.AttributeCount; i < attributeCount; i++)
		{
			reader.MoveToAttribute(i);
			if (reader.LocalName == "sqref")
			{
				text = reader.Value;
			}
			else if (XmlConvertExtension.ToBoolean(reader.Value))
			{
				int num = Array.IndexOf(Excel2007Serializator.ErrorTagsSequence, reader.LocalName);
				if (num >= 0)
				{
					excelIgnoreError |= Excel2007Serializator.ErrorsSequence[num];
				}
			}
		}
		if (text == null)
		{
			throw new XmlException();
		}
		reader.MoveToElement();
		reader.Read();
	}

	private void ParseCustomWorksheetProperties(XmlReader reader, WorksheetImpl sheet)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (reader.LocalName != "customProperties")
		{
			throw new XmlException();
		}
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "customPr")
				{
					ParseCustomProperty(reader, sheet);
				}
				else
				{
					reader.Skip();
				}
			}
		}
		reader.Read();
	}

	private void ParseCustomProperty(XmlReader reader, WorksheetImpl sheet)
	{
	}

	private static int GetParsedXmlValue(string xmlValue)
	{
		if (!xmlValue.StartsWith("-"))
		{
			return (int)XmlConvertExtension.ToUInt32(xmlValue);
		}
		return XmlConvertExtension.ToInt32(xmlValue);
	}

	private string GetPropertyData(string id, WorksheetDataHolder dataHolder)
	{
		RelationCollection relations = dataHolder.Relations;
		Relation relation = relations[id];
		relations.Remove(id);
		string itemName = dataHolder.ArchiveItem.ItemName;
		itemName = itemName;
		itemName = itemName.Replace('\\', '/');
		byte[] data = dataHolder.ParentHolder.GetData(relation, itemName, removeItem: true);
		return Encoding.Unicode.GetString(data, 0, data.Length);
	}

	public static void ParseLegacyDrawingHF(XmlReader reader, WorksheetBaseImpl sheet, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (reader.LocalName != "legacyDrawingHF")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		if (reader.MoveToAttribute("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships"))
		{
			string value = reader.Value;
			WorksheetDataHolder dataHolder = sheet.DataHolder;
			dataHolder.VmlHFDrawingsId = value;
			dataHolder.ParseVmlShapes(sheet.HeaderFooterShapes, value, relations);
			reader.MoveToElement();
			reader.Skip();
			return;
		}
		throw new XmlException("Wrong xml format");
	}

	private void ParseDrawings(XmlReader reader, WorksheetBaseImpl sheet, Dictionary<string, object> dictItemsToRemove)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (reader.LocalName != "drawing")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		if (reader.MoveToAttribute("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships"))
		{
			string value = reader.Value;
			WorksheetDataHolder dataHolder = sheet.DataHolder;
			dataHolder.ParseDrawings(sheet, value, dictItemsToRemove, isChartShape: false);
			dataHolder.DrawingsId = value;
			reader.MoveToElement();
			reader.Skip();
			return;
		}
		throw new XmlException("Wrong xml format");
	}

	private void ParseLegacyDrawing(XmlReader reader, WorksheetBaseImpl sheet)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (reader.LocalName != "legacyDrawing")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		if (reader.MoveToAttribute("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships"))
		{
			string value = reader.Value;
			WorksheetDataHolder dataHolder = sheet.DataHolder;
			dataHolder.ParseVmlShapes(sheet.InnerShapes, value, null);
			dataHolder.VmlDrawingsId = value;
			reader.MoveToElement();
			reader.Skip();
			return;
		}
		throw new XmlException("Wrong xml format");
	}

	private void ParseTwoCellAnchor(XmlReader reader, WorksheetBaseImpl sheet, string drawingsPath, List<string> lstRelationIds, Dictionary<string, object> dictItemsToRemove, bool isChartShape)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (lstRelationIds == null)
		{
			throw new ArgumentNullException("lstRelationIds");
		}
		_ = reader.LocalName == "relSizeAnchor";
		_ = reader.LocalName == "oneCellAnchor";
		string text = null;
		if (reader.MoveToAttribute("editAs"))
		{
			text = reader.Value;
			m_drawingParser.placement = text;
		}
		else
		{
			text = "twoCell";
		}
		reader.Read();
		Rectangle rectangle = default(Rectangle);
		Rectangle rectangle2 = default(Rectangle);
		ShapeImpl shapeImpl = null;
		MemoryStream data = null;
		new Size(-1, -1);
		double posX = 0.0;
		double posY = 0.0;
		double posX2 = 0.0;
		double posY2 = 0.0;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "from":
					rectangle = ParseAnchorPoint(reader, isChartShape, ref posX, ref posY);
					m_drawingParser.leftColumn = rectangle.X;
					m_drawingParser.leftColumnOffset = DocGen.Drawing.Helper.ConvertEmuToOffset(rectangle.Width, dpiX);
					m_drawingParser.topRow = rectangle.Y;
					m_drawingParser.topRowOffset = DocGen.Drawing.Helper.ConvertEmuToOffset(rectangle.Height, dpiY);
					break;
				case "to":
					rectangle2 = ParseAnchorPoint(reader, isChartShape, ref posX2, ref posY2);
					m_drawingParser.rightColumn = rectangle2.X;
					m_drawingParser.rightColumnOffset = DocGen.Drawing.Helper.ConvertEmuToOffset(rectangle2.Width, dpiX);
					m_drawingParser.bottomRow = rectangle2.Y;
					m_drawingParser.bottomRowOffset = DocGen.Drawing.Helper.ConvertEmuToOffset(rectangle2.Height, dpiY);
					break;
				case "ext":
					ParseExtent(reader);
					break;
				case "pic":
					isChartShape = false;
					reader.Skip();
					break;
				case "clientData":
					reader.Skip();
					break;
				case "sp":
				case "cxnSp":
					m_drawingParser.preFix = reader.Prefix;
					m_drawingParser.shapeType = reader.LocalName;
					shapeImpl = CreateShape(reader, sheet, ref data, drawingsPath, lstRelationIds);
					if (m_enableAlternateContent)
					{
						shapeImpl.EnableAlternateContent = m_enableAlternateContent;
					}
					break;
				case "AlternateContent":
					shapeImpl = CreateShape(reader, sheet, ref data, drawingsPath, lstRelationIds);
					shapeImpl.XmlDataStream = data;
					shapeImpl.IsEquationShape = true;
					if (m_enableAlternateContent)
					{
						shapeImpl.EnableAlternateContent = m_enableAlternateContent;
					}
					break;
				case "grpSp":
					shapeImpl = ParseGroupShape(reader, sheet, drawingsPath, lstRelationIds, dictItemsToRemove);
					if (shapeImpl != null && shapeImpl is GroupShapeImpl)
					{
						(shapeImpl as GroupShapeImpl).LayoutGroupShape();
						(shapeImpl as GroupShapeImpl).SetUpdatedChildOffset();
					}
					break;
				default:
					data = ReadSingleNodeIntoStream(reader);
					shapeImpl = TryParseShape(data, sheet, drawingsPath);
					if (shapeImpl == null)
					{
						shapeImpl = new ShapeImpl(sheet.Application, sheet.InnerShapes);
						sheet.InnerShapes.AddShape(shapeImpl);
					}
					else
					{
						data = null;
					}
					break;
				case "graphicFrame":
					data = ReadSingleNodeIntoStream(reader);
					shapeImpl = TryParseChart(data, sheet, drawingsPath, isChartEx: false);
					if (shapeImpl == null)
					{
						shapeImpl = new ShapeImpl(sheet.Application, sheet.InnerShapes);
						sheet.InnerShapes.AddShape(shapeImpl);
					}
					else
					{
						data = null;
					}
					break;
				}
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Read();
		if (isChartShape)
		{
			shapeImpl.StartX = posX;
			shapeImpl.StartY = posY;
			shapeImpl.ToX = posX2;
			shapeImpl.ToY = posY2;
		}
		if (shapeImpl != null && shapeImpl.ShapeType != OfficeShapeType.AutoShape)
		{
			shapeImpl.XmlDataStream = data;
		}
		m_enableAlternateContent = false;
	}

	internal GroupShapeImpl ParseGroupShape(XmlReader reader, WorksheetBaseImpl sheet, string drawingsPath, List<string> lstRelationIds, Dictionary<string, object> dictItemsToRemove)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (lstRelationIds == null)
		{
			throw new ArgumentNullException("lstRelationIds");
		}
		List<ShapeImpl> list = new List<ShapeImpl>();
		MemoryStream data = null;
		string name = string.Empty;
		string alternativeText = string.Empty;
		int shapeId = 0;
		bool flag = false;
		ShapeImpl shapeImpl = null;
		ShapesCollection innerShapes = sheet.InnerShapes;
		GroupShapeImpl groupShapeImpl = new GroupShapeImpl(sheet.Application, innerShapes);
		bool flag2 = false;
		MemoryStream memoryStream = new MemoryStream();
		XmlWriter xmlWriter = UtilityMethods.CreateWriter(memoryStream, Encoding.UTF8);
		xmlWriter.WriteStartElement("root");
		reader.Read();
		while (!(reader.LocalName == "grpSp") || reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "grpSp":
				{
					ShapeImpl item2 = ParseGroupShape(reader, sheet, drawingsPath, lstRelationIds, dictItemsToRemove);
					list.Add(item2);
					break;
				}
				case "nvGrpSpPr":
					reader.Read();
					break;
				case "grpSpPr":
					xmlWriter.WriteNode(reader, defattr: false);
					xmlWriter.Flush();
					flag2 = true;
					break;
				case "cNvPr":
					if (reader.LocalName == "cNvPr")
					{
						if (reader.MoveToAttribute("name"))
						{
							name = reader.Value;
						}
						if (reader.MoveToAttribute("id") && int.TryParse(reader.Value, out var result))
						{
							shapeId = result;
						}
						if (reader.MoveToAttribute("hidden"))
						{
							flag = XmlConvertExtension.ToBoolean(reader.Value);
						}
						if (reader.MoveToAttribute("descr"))
						{
							alternativeText = reader.Value;
						}
					}
					reader.Read();
					break;
				case "pic":
					reader.Skip();
					break;
				case "clientData":
					reader.Skip();
					break;
				case "cxnSp":
				case "sp":
					m_drawingParser = new DrawingParser();
					m_drawingParser.anchorName = "Group";
					m_drawingParser.preFix = reader.Prefix;
					m_drawingParser.shapeType = reader.LocalName;
					shapeImpl = CreateShape(reader, sheet, ref data, drawingsPath, lstRelationIds);
					if (shapeImpl is TextBoxShapeImpl)
					{
						shapeImpl.SetPostion((shapeImpl as TextBoxShapeImpl).Coordinates2007.X, (shapeImpl as TextBoxShapeImpl).Coordinates2007.Y, (shapeImpl as TextBoxShapeImpl).Coordinates2007.Width, (shapeImpl as TextBoxShapeImpl).Coordinates2007.Height);
						shapeImpl.ShapeFrame.SetAnchor(shapeImpl.ShapeRotation * 60000, (shapeImpl as TextBoxShapeImpl).Coordinates2007.X, (shapeImpl as TextBoxShapeImpl).Coordinates2007.Y, (shapeImpl as TextBoxShapeImpl).Coordinates2007.Width, (shapeImpl as TextBoxShapeImpl).Coordinates2007.Height);
					}
					else
					{
						shapeImpl.SetPostion(m_drawingParser.posX, m_drawingParser.posY, m_drawingParser.extCX, m_drawingParser.extCY);
						shapeImpl.ShapeRotation = (int)m_drawingParser.shapeRotation;
						shapeImpl.ShapeFrame.SetAnchor((int)m_drawingParser.shapeRotation * 60000, m_drawingParser.posX, m_drawingParser.posY, m_drawingParser.extCX, m_drawingParser.extCY);
					}
					if (m_enableAlternateContent)
					{
						shapeImpl.EnableAlternateContent = m_enableAlternateContent;
					}
					if (shapeImpl != null)
					{
						list.Add(shapeImpl);
					}
					if (shapeImpl.ShapeType != OfficeShapeType.AutoShape)
					{
						shapeImpl.XmlDataStream = data;
						break;
					}
					(shapeImpl as AutoShapeImpl).FlipHorizontal = m_drawingParser.FlipHorizontal;
					(shapeImpl as AutoShapeImpl).FlipVertical = m_drawingParser.FlipVertical;
					break;
				case "AlternateContent":
					m_drawingParser = new DrawingParser();
					m_drawingParser.anchorName = "Group";
					m_enableAlternateContent = true;
					while (reader.NodeType != XmlNodeType.EndElement)
					{
						if (reader.NodeType == XmlNodeType.Element)
						{
							string localName = reader.LocalName;
							if (!(localName == "sp"))
							{
								if (localName == "grpSp")
								{
									ShapeImpl item = ParseGroupShape(reader, sheet, drawingsPath, lstRelationIds, dictItemsToRemove);
									list.Add(item);
								}
								else
								{
									reader.Read();
								}
								continue;
							}
							shapeImpl = CreateShape(reader, sheet, ref data, drawingsPath, lstRelationIds);
							shapeImpl.XmlDataStream = data;
							shapeImpl.IsEquationShape = true;
							shapeImpl.EnableAlternateContent = true;
							if (shapeImpl is TextBoxShapeImpl)
							{
								shapeImpl.SetPostion((shapeImpl as TextBoxShapeImpl).Coordinates2007.X, (shapeImpl as TextBoxShapeImpl).Coordinates2007.Y, (shapeImpl as TextBoxShapeImpl).Coordinates2007.Width, (shapeImpl as TextBoxShapeImpl).Coordinates2007.Height);
								shapeImpl.ShapeFrame.SetAnchor(shapeImpl.ShapeRotation * 60000, (shapeImpl as TextBoxShapeImpl).Coordinates2007.X, (shapeImpl as TextBoxShapeImpl).Coordinates2007.Y, (shapeImpl as TextBoxShapeImpl).Coordinates2007.Width, (shapeImpl as TextBoxShapeImpl).Coordinates2007.Height);
							}
							else
							{
								shapeImpl.SetPostion(m_drawingParser.posX, m_drawingParser.posY, m_drawingParser.extCX, m_drawingParser.extCY);
								shapeImpl.ShapeRotation = (int)m_drawingParser.shapeRotation;
								shapeImpl.ShapeFrame.SetAnchor((int)m_drawingParser.shapeRotation * 60000, m_drawingParser.posX, m_drawingParser.posY, m_drawingParser.extCX, m_drawingParser.extCY);
							}
							if (shapeImpl != null)
							{
								list.Add(shapeImpl);
								shapeImpl.XmlDataStream = data;
							}
							if (shapeImpl.ShapeType == OfficeShapeType.AutoShape)
							{
								(shapeImpl as AutoShapeImpl).FlipHorizontal = m_drawingParser.FlipHorizontal;
								(shapeImpl as AutoShapeImpl).FlipVertical = m_drawingParser.FlipVertical;
							}
						}
						else
						{
							reader.Skip();
						}
					}
					break;
				case "graphicFrame":
					data = ReadSingleNodeIntoStream(reader);
					shapeImpl = TryParseChart(data, sheet, drawingsPath, isChartEx: false);
					if (shapeImpl == null)
					{
						shapeImpl = new ShapeImpl(sheet.Application, sheet.InnerShapes);
						sheet.InnerShapes.AddShape(shapeImpl);
					}
					else
					{
						data = null;
					}
					if (shapeImpl != null)
					{
						list.Add(shapeImpl);
					}
					break;
				default:
					reader.Skip();
					break;
				}
			}
			else
			{
				reader.Read();
			}
		}
		if (reader.NodeType == XmlNodeType.EndElement)
		{
			reader.Read();
		}
		if (list.Count > 0)
		{
			groupShapeImpl = innerShapes.AddGroupShape(groupShapeImpl, list.ToArray());
		}
		groupShapeImpl.Name = name;
		groupShapeImpl.ShapeId = shapeId;
		groupShapeImpl.IsShapeVisible = !flag;
		groupShapeImpl.AlternativeText = alternativeText;
		xmlWriter.WriteEndElement();
		xmlWriter.Flush();
		if (flag2)
		{
			memoryStream.Position = 0L;
			XmlReader xmlReader = UtilityMethods.CreateReader(memoryStream);
			xmlReader.Read();
			if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.LocalName == "grpSpPr" && !xmlReader.IsEmptyElement)
			{
				ParseGroupShapeProperties(xmlReader, groupShapeImpl);
			}
			xmlReader.Dispose();
			memoryStream.Dispose();
		}
		return groupShapeImpl;
	}

	private void ParseGroupShapeProperties(XmlReader reader, GroupShapeImpl groupShape)
	{
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "xfrm":
					if (reader.MoveToAttribute("rot"))
					{
						groupShape.ShapeRotation = (int)Math.Round((double)Convert.ToInt64(reader.Value) / 60000.0);
						reader.MoveToElement();
					}
					if (reader.MoveToAttribute("flipH"))
					{
						groupShape.FlipHorizontal = XmlConvertExtension.ToBoolean(reader.Value);
					}
					if (reader.MoveToAttribute("flipV"))
					{
						groupShape.FlipVertical = XmlConvertExtension.ToBoolean(reader.Value);
					}
					ParseForm(reader, groupShape);
					break;
				case "noFill":
					groupShape.Fill.Visible = false;
					(groupShape.Fill as ShapeFillImpl).SetInnerShapesFillVisible();
					reader.Skip();
					break;
				case "solidFill":
				{
					Stream stream4 = ShapeParser.ReadNodeAsStream(reader);
					stream4.Position = 0L;
					XmlReader reader5 = UtilityMethods.CreateReader(stream4);
					IInternalFill internalFill2 = groupShape.Fill as IInternalFill;
					ChartParserCommon.ParseSolidFill(reader5, this, internalFill2.ForeColorObject);
					(groupShape.Fill as ShapeFillImpl).SetInnerShapes(internalFill2.ForeColorObject, "ForeColorObject");
					break;
				}
				case "ln":
				{
					Stream stream3 = ShapeParser.ReadNodeAsStream(reader);
					stream3.Position = 0L;
					XmlReader reader4 = UtilityMethods.CreateReader(stream3);
					ShapeLineFormatImpl border = groupShape.Line as ShapeLineFormatImpl;
					TextBoxShapeParser.ParseLineProperties(reader4, border, bRoundCorners: false, this);
					break;
				}
				case "gradFill":
				{
					Stream stream2 = ShapeParser.ReadNodeAsStream(reader);
					stream2.Position = 0L;
					XmlReader reader3 = UtilityMethods.CreateReader(stream2);
					IInternalFill obj = groupShape.Fill as IInternalFill;
					obj.FillType = OfficeFillType.Gradient;
					obj.PreservedGradient = ChartParserCommon.ParseGradientFill(reader3, this);
					break;
				}
				case "pattFill":
				{
					Stream stream = ShapeParser.ReadNodeAsStream(reader);
					stream.Position = 0L;
					XmlReader reader2 = UtilityMethods.CreateReader(stream);
					IInternalFill internalFill = groupShape.Fill as IInternalFill;
					internalFill.FillType = OfficeFillType.Pattern;
					ChartParserCommon.ParsePatternFill(reader2, internalFill, this);
					(groupShape.Fill as ShapeFillImpl).SetInnerShapes(internalFill.Pattern, "Pattern");
					(groupShape.Fill as ShapeFillImpl).SetInnerShapes(internalFill.ForeColorObject, "ForeColorObject");
					(groupShape.Fill as ShapeFillImpl).SetInnerShapes(internalFill.BackColorObject, "BackColorObject");
					break;
				}
				case "scene3d":
					groupShape.PreservedElements.Add("Scene3d", ShapeParser.ReadNodeAsStream(reader));
					break;
				case "sp3d":
					groupShape.PreservedElements.Add("Sp3d", ShapeParser.ReadNodeAsStream(reader));
					break;
				default:
					reader.Skip();
					break;
				}
			}
			else
			{
				reader.Read();
			}
		}
		if (reader.NodeType == XmlNodeType.EndElement)
		{
			reader.Read();
		}
	}

	private void ParseForm(XmlReader reader, GroupShapeImpl groupShape)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					switch (reader.LocalName)
					{
					case "off":
						if (reader.MoveToAttribute("x"))
						{
							num = ((!(2147483647.0 < XmlConvertExtension.ToDouble(reader.Value))) ? XmlConvertExtension.ToInt32(reader.Value) : int.MaxValue);
						}
						if (reader.MoveToAttribute("y"))
						{
							num2 = ((!(2147483647.0 < XmlConvertExtension.ToDouble(reader.Value))) ? XmlConvertExtension.ToInt32(reader.Value) : int.MaxValue);
						}
						break;
					case "ext":
						if (reader.MoveToAttribute("cx"))
						{
							num3 = ((!(2147483647.0 < XmlConvertExtension.ToDouble(reader.Value))) ? XmlConvertExtension.ToInt32(reader.Value) : int.MaxValue);
						}
						if (reader.MoveToAttribute("cy"))
						{
							num4 = ((!(2147483647.0 < XmlConvertExtension.ToDouble(reader.Value))) ? XmlConvertExtension.ToInt32(reader.Value) : int.MaxValue);
						}
						break;
					case "chOff":
						if (reader.MoveToAttribute("x"))
						{
							num5 = ((!(2147483647.0 < XmlConvertExtension.ToDouble(reader.Value))) ? XmlConvertExtension.ToInt32(reader.Value) : int.MaxValue);
						}
						if (reader.MoveToAttribute("y"))
						{
							num6 = ((!(2147483647.0 < XmlConvertExtension.ToDouble(reader.Value))) ? XmlConvertExtension.ToInt32(reader.Value) : int.MaxValue);
						}
						break;
					case "chExt":
						if (reader.MoveToAttribute("cx"))
						{
							num7 = ((!(2147483647.0 < XmlConvertExtension.ToDouble(reader.Value))) ? XmlConvertExtension.ToInt32(reader.Value) : int.MaxValue);
						}
						if (reader.MoveToAttribute("cy"))
						{
							num8 = ((!(2147483647.0 < XmlConvertExtension.ToDouble(reader.Value))) ? XmlConvertExtension.ToInt32(reader.Value) : int.MaxValue);
						}
						break;
					default:
						reader.Skip();
						break;
					}
				}
				else
				{
					reader.Skip();
				}
			}
		}
		groupShape.LeftDouble = ((ApplicationImpl.ConvertToPixels(num, MeasureUnits.EMU) >= 0.0) ? ApplicationImpl.ConvertToPixels(num, MeasureUnits.EMU) : 0.0);
		groupShape.TopDouble = ((ApplicationImpl.ConvertToPixels(num2, MeasureUnits.EMU) >= 0.0) ? ApplicationImpl.ConvertToPixels(num2, MeasureUnits.EMU) : 0.0);
		groupShape.WidthDouble = ((ApplicationImpl.ConvertToPixels(num3, MeasureUnits.EMU) >= 0.0) ? ApplicationImpl.ConvertToPixels(num3, MeasureUnits.EMU) : 0.0);
		groupShape.HeightDouble = ((ApplicationImpl.ConvertToPixels(num4, MeasureUnits.EMU) >= 0.0) ? ApplicationImpl.ConvertToPixels(num4, MeasureUnits.EMU) : 0.0);
		groupShape.ShapeFrame.SetAnchor(groupShape.ShapeRotation * 60000, num, num2, num3, num4);
		groupShape.ShapeFrame.SetChildAnchor(num5, num6, num7, num8);
		reader.Read();
	}

	private ShapeImpl CreateShape(XmlReader reader, WorksheetBaseImpl sheet, ref MemoryStream data, string drawingsPath, List<string> lstRelationIds)
	{
		data = ReadSingleNodeIntoStream(reader);
		data.Position = 0L;
		bool flag = false;
		reader = UtilityMethods.CreateReader(data);
		OfficeShapeType officeShapeType = OfficeShapeType.Unknown;
		ShapeImpl shapeImpl = null;
		string text = null;
		int? num = null;
		string text2 = null;
		string text3 = null;
		bool locksText = true;
		if (reader.MoveToAttribute("macro"))
		{
			text3 = reader.Value;
			reader.MoveToElement();
		}
		if (reader.MoveToAttribute("textlink"))
		{
			text2 = reader.Value;
		}
		if (reader.MoveToAttribute("fLocksText"))
		{
			locksText = reader.ReadContentAsBoolean();
		}
		while (reader.NodeType != 0)
		{
			reader.Read();
			if (reader.LocalName == "Choice" && IsChartExChoice(reader))
			{
				reader.Read();
				MemoryStream data2 = ReadSingleNodeIntoStream(reader);
				shapeImpl = TryParseChart(data2, sheet, drawingsPath, isChartEx: true);
				officeShapeType = OfficeShapeType.Chart;
			}
			else
			{
				if (reader.NodeType != XmlNodeType.Element)
				{
					continue;
				}
				if (reader.LocalName == "cNvSpPr")
				{
					if (reader.MoveToAttribute("txBox") && XmlConvertExtension.ToBoolean(reader.Value))
					{
						officeShapeType = OfficeShapeType.TextBox;
						reader.Read();
						if (reader.LocalName == "spLocks" && reader.MoveToAttribute("noChangeAspect"))
						{
							reader.ReadContentAsBoolean();
						}
						break;
					}
				}
				else if (reader.LocalName == "cNvPr")
				{
					if (reader.MoveToAttribute("id") && int.TryParse(reader.Value, out var result))
					{
						num = result;
						m_drawingParser.id = result;
					}
					if (reader.MoveToAttribute("name"))
					{
						m_drawingParser.name = reader.Value;
						text = reader.Value;
					}
					if (reader.MoveToAttribute("descr"))
					{
						m_drawingParser.descr = reader.Value;
					}
					if (reader.MoveToAttribute("hidden"))
					{
						m_drawingParser.IsHidden = XmlConvertExtension.ToBoolean(reader.Value);
					}
					if (reader.MoveToAttribute("title"))
					{
						m_drawingParser.tittle = reader.Value;
					}
				}
				if (reader.LocalName == "xfrm")
				{
					if (reader.MoveToAttribute("rot"))
					{
						string value = reader.Value;
						if (value != null && value.Length > 0)
						{
							double shapeRotation = double.Parse(value, CultureInfo.InvariantCulture) / 60000.0;
							m_drawingParser.shapeRotation = shapeRotation;
						}
					}
					else
					{
						m_drawingParser.shapeRotation = 0.0;
					}
					if (reader.MoveToAttribute("flipH"))
					{
						m_drawingParser.FlipHorizontal = XmlConvertExtension.ToBoolean(reader.Value);
					}
					else
					{
						m_drawingParser.FlipHorizontal = false;
					}
					if (reader.MoveToAttribute("flipV"))
					{
						m_drawingParser.FlipVertical = XmlConvertExtension.ToBoolean(reader.Value);
					}
					else
					{
						m_drawingParser.FlipVertical = false;
					}
					ParseForm(reader);
				}
				if ((reader.LocalName == "prstGeom" || reader.LocalName == "custGeom") && !m_enableAlternateContent && reader.LocalName == "prstGeom")
				{
					string shapeString = "";
					if (reader.MoveToAttribute("prst"))
					{
						shapeString = reader.Value;
					}
					AutoShapeType autoShapeType = AutoShapeHelper.GetAutoShapeType(AutoShapeHelper.GetAutoShapeConstant(shapeString));
					if (autoShapeType != AutoShapeType.Unknown)
					{
						officeShapeType = OfficeShapeType.AutoShape;
						m_drawingParser.autoShapeType = autoShapeType;
						reader.MoveToElement();
						reader.Read();
					}
					SkipWhiteSpaces(reader);
					if (reader.LocalName == "avLst")
					{
						m_drawingParser.CustGeomStream = ShapeParser.ReadNodeAsStream(reader);
					}
				}
				if (reader.LocalName == "solidFill")
				{
					m_drawingParser.FillStream = ShapeParser.ReadNodeAsStream(reader);
					break;
				}
				if (reader.LocalName == "Fallback")
				{
					reader.Skip();
				}
				if (reader.LocalName == "slicer")
				{
					flag = true;
					officeShapeType = OfficeShapeType.TextBox;
				}
			}
		}
		data.Position = 0L;
		reader = UtilityMethods.CreateReader(data);
		switch (officeShapeType)
		{
		case OfficeShapeType.TextBox:
		{
			ITextBoxShapeEx textBoxShapeEx = sheet.Shapes.AddTextBox();
			if (text2 != null && text2.Length > 0)
			{
				textBoxShapeEx.TextLink = $"={text2}";
			}
			shapeImpl = (ShapeImpl)textBoxShapeEx;
			TextBoxShapeParser.ParseTextBox(textBoxShapeEx, reader, this, lstRelationIds);
			if (num.HasValue)
			{
				shapeImpl.ShapeId = num.Value;
			}
			if (text3 != null)
			{
				shapeImpl.MacroName = text3;
				shapeImpl.OnAction = text3;
			}
			break;
		}
		case OfficeShapeType.Unknown:
		{
			shapeImpl = new ShapeImpl(sheet.Application, sheet.InnerShapes);
			if (flag)
			{
				shapeImpl.IsSlicer = true;
			}
			shapeImpl.OnAction = text3;
			AutoShapeImpl autoShapeImpl = CheckShapeIsFreeForm(m_drawingParser.CustGeomStream, sheet, reader, lstRelationIds);
			if (autoShapeImpl != null)
			{
				shapeImpl = autoShapeImpl;
			}
			if (num.HasValue)
			{
				shapeImpl.ShapeId = num.Value;
			}
			if (text != null)
			{
				shapeImpl.Name = text;
			}
			if (m_drawingParser.FillStream != null && m_drawingParser.FillStream.Length > 0)
			{
				shapeImpl.PreservedElements.Add("solidFill", m_drawingParser.FillStream);
			}
			sheet.InnerShapes.AddShape(shapeImpl);
			autoShapeImpl = null;
			break;
		}
		case OfficeShapeType.AutoShape:
		{
			AutoShapeImpl autoShapeImpl2 = new AutoShapeImpl(sheet.Application, sheet.InnerShapes);
			autoShapeImpl2.OnAction = text3;
			if (m_workSheet != null)
			{
				m_drawingParser.AddShape(autoShapeImpl2, m_workSheet);
			}
			else
			{
				m_drawingParser.AddShape(autoShapeImpl2, sheet);
			}
			ParseAutoShape(autoShapeImpl2, reader);
			sheet.InnerShapes.Add(autoShapeImpl2);
			autoShapeImpl2.ShapeExt.Logger.ResetFlag();
			if (autoShapeImpl2.Fill.Visible && (autoShapeImpl2.Fill.FillType == OfficeFillType.Texture || autoShapeImpl2.Fill.FillType == OfficeFillType.Picture))
			{
				autoShapeImpl2.Fill.Visible = true;
			}
			autoShapeImpl2.ShapeExt.Macro = text3;
			autoShapeImpl2.ShapeExt.TextLink = text2;
			autoShapeImpl2.ShapeExt.LocksText = locksText;
			autoShapeImpl2.ShapeExt.Coordinates = new Rectangle(m_drawingParser.posX, m_drawingParser.posY, m_drawingParser.extCX, m_drawingParser.extCY);
			shapeImpl = autoShapeImpl2;
			break;
		}
		case OfficeShapeType.Chart:
			if (num.HasValue)
			{
				shapeImpl.ShapeId = num.Value;
			}
			if (text != null)
			{
				shapeImpl.Name = text;
			}
			break;
		}
		return shapeImpl;
	}

	private AutoShapeImpl CheckShapeIsFreeForm(Stream custGeomStream, WorksheetBaseImpl sheet, XmlReader reader, List<string> lstRelationIds)
	{
		if (custGeomStream != null && custGeomStream.Length > 0)
		{
			AutoShapeImpl autoShapeImpl = new AutoShapeImpl(sheet.Application, sheet.InnerShapes);
			m_drawingParser.AddShape(autoShapeImpl, sheet);
			autoShapeImpl.ShapeType = OfficeShapeType.AutoShape;
			autoShapeImpl.IsCustomGeometry = true;
			ParseAutoShape(autoShapeImpl, reader);
			return autoShapeImpl;
		}
		return null;
	}

	private void ParseAutoShape(AutoShapeImpl autoShape, XmlReader reader)
	{
		if (autoShape == null)
		{
			throw new ArgumentNullException("autoShape");
		}
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		bool flag = false;
		reader.Read();
		while (reader.NodeType != 0)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "txBody":
				{
					Stream stream2 = ShapeParser.ReadNodeAsStream(reader);
					stream2.Position = 0L;
					autoShape.ShapeExt.PreservedElements.Add("TextBody", stream2);
					ParseRichText(UtilityMethods.CreateReader(stream2), autoShape, this);
					break;
				}
				case "spPr":
					ParseProperties(reader, autoShape);
					break;
				case "style":
				{
					Stream stream = ShapeParser.ReadNodeAsStream(reader);
					flag = true;
					autoShape.ShapeExt.PreservedElements.Add("Style", stream);
					XmlReader xmlReader = UtilityMethods.CreateReader(stream, "lnRef");
					ShapeLineFormatImpl shapeLineFormatImpl = autoShape.Line as ShapeLineFormatImpl;
					int num = -1;
					if (xmlReader.MoveToAttribute("idx"))
					{
						num = int.Parse(xmlReader.Value);
					}
					if ((!autoShape.ShapeExt.PreservedElements.ContainsKey("Line") || !shapeLineFormatImpl.IsNoFill) && !shapeLineFormatImpl.IsSolidFill)
					{
						switch (num)
						{
						case 0:
							shapeLineFormatImpl.Visible = false;
							break;
						default:
							shapeLineFormatImpl.Visible = true;
							break;
						case -1:
							break;
						}
					}
					shapeLineFormatImpl.DefaultLineStyleIndex = num;
					xmlReader = UtilityMethods.CreateReader(stream, "fillRef");
					if (!autoShape.IsNoFill && !autoShape.IsFill && xmlReader.MoveToAttribute("idx"))
					{
						if (int.Parse(xmlReader.Value) == 0)
						{
							autoShape.ShapeExt.Fill.Visible = false;
						}
						else
						{
							autoShape.ShapeExt.Fill.Visible = true;
						}
					}
					break;
				}
				default:
					reader.Skip();
					break;
				}
			}
			else
			{
				reader.Read();
			}
		}
		if (autoShape != null && !flag && !autoShape.IsFill && !autoShape.IsNoFill)
		{
			autoShape.ShapeExt.Fill.Visible = false;
		}
	}

	private void ParseProperties(XmlReader reader, AutoShapeImpl autoShape)
	{
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "noFill":
					autoShape.ShapeExt.Fill.Visible = false;
					autoShape.IsNoFill = true;
					reader.Skip();
					break;
				case "solidFill":
				{
					Stream stream4 = ShapeParser.ReadNodeAsStream(reader);
					stream4.Position = 0L;
					autoShape.ShapeExt.PreservedElements.Add("Fill", stream4);
					XmlReader reader5 = UtilityMethods.CreateReader(stream4);
					IInternalFill fill3 = autoShape.ShapeExt.Fill;
					ChartParserCommon.ParseSolidFill(reader5, this, fill3.ForeColorObject);
					break;
				}
				case "ln":
				{
					Stream stream3 = ShapeParser.ReadNodeAsStream(reader);
					stream3.Position = 0L;
					autoShape.ShapeExt.PreservedElements.Add("Line", stream3);
					XmlReader reader4 = UtilityMethods.CreateReader(stream3);
					ShapeLineFormatImpl line = autoShape.ShapeExt.Line;
					TextBoxShapeParser.ParseLineProperties(reader4, line, bRoundCorners: false, this);
					break;
				}
				case "gradFill":
				{
					Stream stream2 = ShapeParser.ReadNodeAsStream(reader);
					stream2.Position = 0L;
					autoShape.ShapeExt.PreservedElements.Add("Fill", stream2);
					XmlReader reader3 = UtilityMethods.CreateReader(stream2);
					ShapeFillImpl fill2 = autoShape.ShapeExt.Fill;
					((IOfficeFill)fill2).FillType = OfficeFillType.Gradient;
					((IInternalFill)fill2).PreservedGradient = ChartParserCommon.ParseGradientFill(reader3, this);
					break;
				}
				case "blipFill":
					autoShape.ShapeExt.PreservedElements.Add("Fill", ShapeParser.ReadNodeAsStream(reader));
					break;
				case "pattFill":
				{
					Stream stream = ShapeParser.ReadNodeAsStream(reader);
					stream.Position = 0L;
					autoShape.ShapeExt.PreservedElements.Add("Fill", stream);
					XmlReader reader2 = UtilityMethods.CreateReader(stream);
					IInternalFill fill = autoShape.ShapeExt.Fill;
					fill.FillType = OfficeFillType.Pattern;
					ChartParserCommon.ParsePatternFill(reader2, fill, this);
					break;
				}
				case "grpFill":
					autoShape.ShapeExt.PreservedElements.Add("Fill", ShapeParser.ReadNodeAsStream(reader));
					break;
				case "effectLst":
					autoShape.ShapeExt.PreservedElements.Add("Effect", ShapeParser.ReadNodeAsStream(reader));
					break;
				case "effectDag":
					autoShape.ShapeExt.PreservedElements.Add("Effect", ShapeParser.ReadNodeAsStream(reader));
					break;
				case "scene3d":
					autoShape.ShapeExt.PreservedElements.Add("Scene3d", ShapeParser.ReadNodeAsStream(reader));
					break;
				case "sp3d":
					autoShape.ShapeExt.PreservedElements.Add("Sp3d", ShapeParser.ReadNodeAsStream(reader));
					break;
				default:
					reader.Skip();
					break;
				}
			}
			else
			{
				reader.Read();
			}
		}
	}

	private static void ParseRichText(XmlReader reader, AutoShapeImpl autoShape, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (autoShape == null)
		{
			throw new ArgumentNullException("textBox");
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName = reader.LocalName;
				if (!(localName == "bodyPr"))
				{
					if (localName == "p")
					{
						ParseParagraphs(reader, autoShape, parser);
					}
					else
					{
						reader.Skip();
					}
				}
				else
				{
					ParseBodyProperties(reader, autoShape.TextFrameInternal.TextBodyProperties);
				}
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Read();
	}

	internal static void ParseBodyProperties(XmlReader reader, TextBodyPropertiesHolder TextBodyProperties)
	{
		if (reader.MoveToAttribute("vertOverflow"))
		{
			TextBodyProperties.TextVertOverflowType = DocGen.Drawing.Helper.GetVerticalFlowType(reader.Value);
		}
		if (reader.MoveToAttribute("horzOverflow"))
		{
			TextBodyProperties.TextHorzOverflowType = DocGen.Drawing.Helper.GetHorizontalFlowType(reader.Value);
		}
		if (reader.MoveToAttribute("vert"))
		{
			TextBodyProperties.TextDirection = DocGen.Drawing.Helper.SetTextDirection(reader.Value);
		}
		if (reader.MoveToAttribute("wrap"))
		{
			TextBodyProperties.WrapTextInShape = reader.Value != "none";
		}
		if (reader.MoveToAttribute("lIns"))
		{
			TextBodyProperties.SetLeftMargin(DocGen.Drawing.Helper.ParseInt(reader.Value));
			TextBodyProperties.IsAutoMargins = false;
		}
		if (reader.MoveToAttribute("tIns"))
		{
			TextBodyProperties.SetTopMargin(DocGen.Drawing.Helper.ParseInt(reader.Value));
			TextBodyProperties.IsAutoMargins = false;
		}
		if (reader.MoveToAttribute("rIns"))
		{
			TextBodyProperties.SetRightMargin(DocGen.Drawing.Helper.ParseInt(reader.Value));
			TextBodyProperties.IsAutoMargins = false;
		}
		if (reader.MoveToAttribute("bIns"))
		{
			TextBodyProperties.SetBottomMargin(DocGen.Drawing.Helper.ParseInt(reader.Value));
			TextBodyProperties.IsAutoMargins = false;
		}
		if (reader.MoveToAttribute("numCol"))
		{
			TextBodyProperties.Number = DocGen.Drawing.Helper.ParseInt(reader.Value);
		}
		if (reader.MoveToAttribute("spcCol"))
		{
			TextBodyProperties.SpacingPt = (int)((double)DocGen.Drawing.Helper.ParseInt(reader.Value) / 12700.0);
		}
		string anchorType = "t";
		bool anchorCtrl = false;
		if (reader.MoveToAttribute("anchor"))
		{
			anchorType = reader.Value;
		}
		if (reader.MoveToAttribute("anchorCtr"))
		{
			anchorCtrl = XmlConvertExtension.ToBoolean(reader.Value);
		}
		DocGen.Drawing.Helper.SetAnchorPosition(TextBodyProperties, anchorType, anchorCtrl);
		reader.MoveToElement();
		if (reader.LocalName == "bodyPr" && !reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					string localName = reader.LocalName;
					if (!(localName == "spAutoFit"))
					{
						if (localName == "prstTxWarp")
						{
							if (reader.MoveToAttribute("prst"))
							{
								if (reader.Value == "textPlain")
								{
									TextBodyProperties.PresetWrapTextInShape = true;
								}
								reader.Skip();
							}
						}
						else
						{
							reader.Skip();
						}
					}
					else
					{
						TextBodyProperties.IsAutoSize = true;
						reader.Read();
					}
				}
				else
				{
					SkipWhiteSpaces(reader);
				}
			}
			reader.Read();
		}
		else
		{
			reader.Skip();
		}
	}

	private static void ParseParagraphs(XmlReader reader, AutoShapeImpl autoShape, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (autoShape == null)
		{
			throw new ArgumentNullException("textBox");
		}
		if (reader.LocalName != "p")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		RichTextString richTextString = autoShape.ShapeExt.TextFrame.TextRange.RichText as RichTextString;
		string text = richTextString.Text;
		if (text != null && text.Length != 0 && !text.EndsWith("\n"))
		{
			richTextString.AddText("\n", richTextString.GetFont(text.Length - 1));
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName = reader.LocalName;
				if (!(localName == "r"))
				{
					if (localName == "endParaRPr")
					{
						TextBoxShapeParser.ParseParagraphEnd(reader, richTextString, parser);
					}
					else
					{
						reader.Skip();
					}
				}
				else
				{
					TextBoxShapeParser.ParseParagraphRun(reader, richTextString, parser, null);
				}
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Read();
	}

	private static void ParseParagraphRun(XmlReader reader, AutoShapeImpl autoShape)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (autoShape == null)
		{
			throw new ArgumentNullException("textArea");
		}
		if (reader.LocalName != "r")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		string text = null;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				if (reader.LocalName == "t")
				{
					text = reader.ReadElementContentAsString();
				}
				else
				{
					reader.Skip();
				}
			}
			else
			{
				reader.Skip();
			}
		}
		if (text == null || text.Length == 0)
		{
			text = "\n";
		}
		string text2 = autoShape.ShapeExt.TextFrame.TextRange.Text;
		autoShape.ShapeExt.TextFrame.TextRange.Text = text2 + text;
		reader.Read();
	}

	private void ParseForm(XmlReader reader)
	{
		if (reader.IsEmptyElement)
		{
			return;
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName = reader.LocalName;
				if (!(localName == "off"))
				{
					if (localName == "ext")
					{
						if (reader.MoveToAttribute("cx"))
						{
							m_drawingParser.extCX = XmlConvertExtension.ToInt32(reader.Value);
						}
						if (reader.MoveToAttribute("cy"))
						{
							m_drawingParser.extCY = XmlConvertExtension.ToInt32(reader.Value);
						}
					}
					else
					{
						reader.Skip();
					}
				}
				else
				{
					if (reader.MoveToAttribute("x"))
					{
						m_drawingParser.posX = XmlConvertExtension.ToInt32(reader.Value);
					}
					if (reader.MoveToAttribute("y"))
					{
						m_drawingParser.posY = XmlConvertExtension.ToInt32(reader.Value);
					}
				}
			}
			else
			{
				reader.Skip();
			}
		}
	}

	private ShapeImpl TryParseChart(MemoryStream data, WorksheetBaseImpl sheet, string drawingPath, bool isChartEx)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		data.Position = 0L;
		XmlReader xmlReader = UtilityMethods.CreateReader(data);
		xmlReader.Read();
		string text = null;
		string s = "0";
		while (xmlReader.LocalName != "chart" && xmlReader.NodeType != 0)
		{
			if (xmlReader.LocalName == "cNvPr" && xmlReader.MoveToAttribute("name"))
			{
				text = xmlReader.Value;
				if (xmlReader.MoveToAttribute("id"))
				{
					s = xmlReader.Value;
				}
			}
			xmlReader.Read();
		}
		ChartShapeImpl chartShapeImpl = null;
		if (xmlReader.LocalName == "chart")
		{
			chartShapeImpl = (ChartShapeImpl)sheet.Charts.Add();
			ChartImpl chartObject = chartShapeImpl.ChartObject;
			WorksheetDataHolder dataHolder = sheet.DataHolder;
			FileDataHolder parentHolder = dataHolder.ParentHolder;
			RelationCollection drawingsRelations = dataHolder.DrawingsRelations;
			chartObject.DataHolder = dataHolder;
			ParseChartTag(xmlReader, chartObject, drawingsRelations, parentHolder, drawingPath, isChartEx);
			chartObject.DataHolder = null;
			if (dataHolder.DrawingsRelations.Count == 0 && drawingsRelations.Count != 0)
			{
				dataHolder.AssignDrawingrelation(drawingsRelations);
			}
			if (text != null)
			{
				chartShapeImpl.Name = text;
			}
		}
		if (chartShapeImpl != null)
		{
			chartShapeImpl.ShapeId = int.Parse(s);
		}
		return chartShapeImpl;
	}

	private ShapeImpl TryParseShape(MemoryStream data, WorksheetBaseImpl sheet, string drawingPath)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		data.Position = 0L;
		XmlReader xmlReader = UtilityMethods.CreateReader(data);
		xmlReader.Read();
		string text = null;
		string s = "0";
		ChartShapeImpl chartShapeImpl = null;
		ShapeImpl shapeImpl = sheet.InnerShapes.AddShape(new ShapeImpl(sheet.Application, sheet.InnerShapes));
		bool flag = false;
		while (!(xmlReader.LocalName == "grpSp") || xmlReader.NodeType != XmlNodeType.EndElement)
		{
			switch (xmlReader.LocalName)
			{
			case "grpSp":
			{
				Stream item3 = ReadSingleNodeIntoStream(xmlReader);
				if (shapeImpl.preservedShapeStreams == null)
				{
					shapeImpl.preservedShapeStreams = new List<Stream>();
				}
				shapeImpl.preservedShapeStreams.Add(item3);
				flag = true;
				break;
			}
			case "nvGrpSpPr":
			{
				Stream item5 = ReadSingleNodeIntoStream(xmlReader);
				if (shapeImpl.preservedShapeStreams == null)
				{
					shapeImpl.preservedShapeStreams = new List<Stream>();
				}
				shapeImpl.preservedShapeStreams.Add(item5);
				flag = true;
				break;
			}
			case "slicer":
				if (chartShapeImpl == null)
				{
					chartShapeImpl = new ChartShapeImpl(sheet.Application, shapeImpl);
				}
				if (text != null)
				{
					chartShapeImpl.Name = text;
				}
				if (chartShapeImpl != null)
				{
					chartShapeImpl.ShapeId = int.Parse(s);
				}
				shapeImpl.GraphicFrameStream = ShapeParser.ReadNodeAsStream(xmlReader);
				chartShapeImpl.GraphicFrameStream = shapeImpl.GraphicFrameStream;
				shapeImpl.ChildShapes.Add(chartShapeImpl);
				break;
			case "grpSpPr":
			{
				Stream item4 = ReadSingleNodeIntoStream(xmlReader);
				if (shapeImpl.preservedShapeStreams == null)
				{
					shapeImpl.preservedShapeStreams = new List<Stream>();
				}
				shapeImpl.preservedShapeStreams.Add(item4);
				flag = true;
				break;
			}
			case "cNvPr":
				if (xmlReader.LocalName == "cNvPr" && xmlReader.MoveToAttribute("name"))
				{
					text = xmlReader.Value;
					if (xmlReader.MoveToAttribute("id"))
					{
						s = xmlReader.Value;
					}
				}
				break;
			case "pic":
			{
				Stream item6 = ReadSingleNodeIntoStream(xmlReader);
				if (shapeImpl.preservedPictureStreams == null)
				{
					shapeImpl.preservedPictureStreams = new List<Stream>();
				}
				shapeImpl.preservedPictureStreams.Add(item6);
				flag = true;
				break;
			}
			case "sp":
			{
				Stream item2 = ReadSingleNodeIntoStream(xmlReader);
				if (shapeImpl.preservedShapeStreams == null)
				{
					shapeImpl.preservedShapeStreams = new List<Stream>();
				}
				shapeImpl.preservedShapeStreams.Add(item2);
				flag = true;
				break;
			}
			case "chart":
				if (xmlReader.LocalName == "chart")
				{
					if (chartShapeImpl == null)
					{
						chartShapeImpl = new ChartShapeImpl(sheet.Application, shapeImpl);
					}
					shapeImpl.ChildShapes.Add(chartShapeImpl);
					ChartImpl chartObject = chartShapeImpl.ChartObject;
					WorksheetDataHolder dataHolder = sheet.DataHolder;
					FileDataHolder parentHolder = dataHolder.ParentHolder;
					RelationCollection drawingsRelations = dataHolder.DrawingsRelations;
					chartObject.DataHolder = dataHolder;
					ParseChartTag(xmlReader, chartObject, drawingsRelations, parentHolder, drawingPath, isChartEx: false);
					chartObject.DataHolder = null;
					if (text != null)
					{
						chartShapeImpl.Name = text;
					}
				}
				if (chartShapeImpl != null)
				{
					chartShapeImpl.ShapeId = int.Parse(s);
				}
				break;
			case "off":
				chartShapeImpl = new ChartShapeImpl(sheet.Application, shapeImpl);
				if (xmlReader.LocalName == "off")
				{
					if (xmlReader.MoveToAttribute("x"))
					{
						chartShapeImpl.OffsetX = XmlConvertExtension.ToInt32(xmlReader.Value);
					}
					if (xmlReader.MoveToAttribute("y"))
					{
						chartShapeImpl.OffsetY = XmlConvertExtension.ToInt32(xmlReader.Value);
					}
				}
				break;
			case "ext":
				if (chartShapeImpl == null)
				{
					chartShapeImpl = new ChartShapeImpl(sheet.Application, shapeImpl);
				}
				if (xmlReader.LocalName == "ext")
				{
					if (xmlReader.MoveToAttribute("cx"))
					{
						chartShapeImpl.ExtentsX = XmlConvertExtension.ToInt32(xmlReader.Value);
					}
					if (xmlReader.MoveToAttribute("cy"))
					{
						chartShapeImpl.ExtentsY = XmlConvertExtension.ToInt32(xmlReader.Value);
					}
				}
				break;
			case "cxnSp":
			{
				Stream item = ReadSingleNodeIntoStream(xmlReader);
				if (shapeImpl.preservedInnerCnxnShapeStreams == null)
				{
					shapeImpl.preservedInnerCnxnShapeStreams = new List<Stream>();
				}
				shapeImpl.preservedInnerCnxnShapeStreams.Add(item);
				flag = true;
				break;
			}
			}
			if (!flag)
			{
				xmlReader.Read();
			}
			flag = false;
		}
		return shapeImpl;
	}

	internal MemoryStream ReadSingleNodeIntoStream(XmlReader reader)
	{
		MemoryStream memoryStream = new MemoryStream();
		XmlWriter xmlWriter = UtilityMethods.CreateWriter(memoryStream, Encoding.UTF8);
		xmlWriter.WriteNode(reader, defattr: false);
		xmlWriter.Flush();
		return memoryStream;
	}

	private Size ParseExtent(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		int num = -1;
		int num2 = -1;
		if (reader.MoveToAttribute("cx"))
		{
			num = int.Parse(reader.Value);
		}
		if (reader.MoveToAttribute("cy"))
		{
			num2 = int.Parse(reader.Value);
		}
		if (m_drawingParser != null)
		{
			m_drawingParser.cx = DocGen.Drawing.Helper.ConvertEmuToOffset(num, dpiX);
			m_drawingParser.cy = DocGen.Drawing.Helper.ConvertEmuToOffset(num2, dpiX);
		}
		num = (int)Math.Round(ApplicationImpl.ConvertToPixels(num, MeasureUnits.EMU));
		num2 = (int)Math.Round(ApplicationImpl.ConvertToPixels(num2, MeasureUnits.EMU));
		return new Size(num, num2);
	}

	private void ParseEditAsValue(ShapeImpl shape, string editAs)
	{
		if (editAs != null)
		{
			if (shape == null)
			{
				throw new ArgumentNullException("shape");
			}
			switch (editAs)
			{
			case "twoCell":
				shape.IsMoveWithCell = true;
				shape.IsSizeWithCell = true;
				break;
			case "oneCell":
				shape.IsMoveWithCell = true;
				shape.IsSizeWithCell = false;
				break;
			case "absolute":
				shape.IsMoveWithCell = false;
				shape.IsSizeWithCell = false;
				break;
			default:
				throw new XmlException();
			}
		}
	}

	private void SetAnchor(ShapeImpl shape, Rectangle fromRect, Rectangle toRect, Size shapeExtent, bool bRelative, bool bOneCellAnchor)
	{
		if (shape == null)
		{
			throw new ArgumentNullException("shape");
		}
		MsofbtClientAnchor msofbtClientAnchor = SetAnchor(shape, fromRect, toRect);
		if (shapeExtent.Width < 0)
		{
			shape.UpdateHeight();
			shape.UpdateWidth();
		}
		else
		{
			shape.Width = shapeExtent.Width;
			shape.Height = shapeExtent.Height;
			shape.HasExtent = true;
		}
		if (bOneCellAnchor)
		{
			msofbtClientAnchor.OneCellAnchor = true;
		}
	}

	private MsofbtClientAnchor SetAnchor(ShapeImpl shape, Rectangle fromRect, Rectangle toRect)
	{
		if (shape == null)
		{
			throw new ArgumentNullException("shape");
		}
		IWorksheet sheet = shape.Worksheet as WorksheetImpl;
		fromRect = NormalizeAnchor(fromRect, sheet);
		toRect = NormalizeAnchor(toRect, sheet);
		MsofbtClientAnchor clientAnchor = shape.ClientAnchor;
		clientAnchor.LeftColumn = fromRect.Left;
		clientAnchor.LeftOffset = fromRect.Width;
		clientAnchor.TopRow = fromRect.Top;
		clientAnchor.TopOffset = fromRect.Height;
		clientAnchor.RightColumn = toRect.Left;
		clientAnchor.RightOffset = toRect.Width;
		clientAnchor.BottomRow = toRect.Top;
		clientAnchor.BottomOffset = toRect.Height;
		shape.EvaluateTopLeftPosition();
		return clientAnchor;
	}

	private Rectangle NormalizeAnchor(Rectangle anchorPoint, IWorksheet sheet)
	{
		if (sheet == null)
		{
			return anchorPoint;
		}
		int num = 0;
		int num2 = 0;
		int num3 = anchorPoint.Left + 1;
		int num4 = anchorPoint.Top + 1;
		if (num4 > sheet.Workbook.MaxRowCount)
		{
			anchorPoint.Y = sheet.Workbook.MaxRowCount - 1;
			num2 = 256;
		}
		else
		{
			double value = anchorPoint.Height;
			value = ApplicationImpl.ConvertToPixels(value, MeasureUnits.EMU);
			num2 = sheet.GetRowHeightInPixels(num4);
			num2 = ((num2 != 0) ? ((int)Math.Round(value * 256.0 / (double)num2)) : 0);
		}
		if (num3 > sheet.Workbook.MaxColumnCount)
		{
			anchorPoint.X = sheet.Workbook.MaxColumnCount - 1;
			num = 1024;
		}
		else
		{
			double value2 = anchorPoint.Width;
			value2 = ApplicationImpl.ConvertToPixels(value2, MeasureUnits.EMU);
			num = sheet.GetColumnWidthInPixels(num3);
			num = ((num != 0) ? ((int)Math.Round(value2 * 1024.0 / (double)num)) : 0);
		}
		anchorPoint.Width = num;
		anchorPoint.Height = num2;
		return anchorPoint;
	}

	private Rectangle ParseAnchorPoint(XmlReader reader, bool isChartShape, ref double posX, ref double posY)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		Rectangle result = ParseAnchorValue(reader, isChartShape, ref posX, ref posY);
		reader.Read();
		return result;
	}

	private Rectangle ParseAnchorValue(XmlReader reader, bool isChartShape, ref double posX, ref double posY)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		Rectangle result = default(Rectangle);
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName = reader.LocalName;
				string s = reader.ReadElementContentAsString();
				switch (localName)
				{
				case "col":
					result.X = XmlConvertExtension.ToInt32(s);
					break;
				case "colOff":
					result.Width = XmlConvertExtension.ToInt32(s);
					break;
				case "row":
					result.Y = XmlConvertExtension.ToInt32(s);
					break;
				case "rowOff":
					result.Height = XmlConvertExtension.ToInt32(s);
					break;
				case "x":
					posX = XmlConvertExtension.ToDouble(s);
					result.X = (int)(XmlConvertExtension.ToDouble(s) * 1000.0);
					break;
				case "y":
					posY = XmlConvertExtension.ToDouble(s);
					result.Y = (int)(XmlConvertExtension.ToDouble(s) * 1000.0);
					break;
				default:
					throw new XmlException("Unexpected xml tag.");
				}
			}
			else
			{
				reader.Skip();
			}
		}
		return result;
	}

	private ShapeImpl ParsePicture(XmlReader reader, WorksheetBaseImpl sheet, string drawingsPath, List<string> lstRelationIds, Dictionary<string, object> dictItemsToRemove)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (lstRelationIds == null)
		{
			throw new ArgumentNullException("lstRelationIds");
		}
		if (reader.LocalName != "pic")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		BitmapShapeImpl bitmapShapeImpl = new BitmapShapeImpl(sheet.Application, sheet.InnerShapes);
		WorksheetDataHolder dataHolder = sheet.DataHolder;
		RelationCollection drawingsRelations = dataHolder.DrawingsRelations;
		FileDataHolder parentHolder = dataHolder.ParentHolder;
		if (reader.MoveToAttribute("macro"))
		{
			bitmapShapeImpl.Macro = reader.Value;
			reader.MoveToElement();
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "nvPicPr":
					ParsePictureProperties(reader, bitmapShapeImpl, drawingsRelations, drawingsPath, parentHolder, lstRelationIds, dictItemsToRemove);
					break;
				case "blipFill":
					ParseBlipFill(reader, bitmapShapeImpl, drawingsRelations, drawingsPath, parentHolder, lstRelationIds, dictItemsToRemove);
					break;
				case "spPr":
					ParseShapeProperties(reader, bitmapShapeImpl);
					break;
				default:
					reader.Skip();
					break;
				}
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Read();
		sheet.InnerShapes.AddPicture(bitmapShapeImpl);
		return bitmapShapeImpl;
	}

	private void ParseShapeProperties(XmlReader reader, ShapeImpl shape)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (shape == null)
		{
			throw new ArgumentNullException("shape");
		}
		if (reader.LocalName != "spPr")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		if (shape is BitmapShapeImpl bitmapShapeImpl)
		{
			Stream stream = new MemoryStream();
			XmlWriter xmlWriter = UtilityMethods.CreateWriter(stream, Encoding.UTF8);
			xmlWriter.WriteNode(reader, defattr: false);
			xmlWriter.Flush();
			bitmapShapeImpl.ShapePropertiesStream = stream;
		}
		else
		{
			reader.Skip();
		}
	}

	private void ParseBlipFill(XmlReader reader, BitmapShapeImpl shape, RelationCollection relations, string parentPath, FileDataHolder holder, List<string> lstRelationIds, Dictionary<string, object> dictItemsToRemove)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (shape == null)
		{
			throw new ArgumentNullException("shape");
		}
		if (lstRelationIds == null)
		{
			throw new ArgumentNullException("lstRelationIds");
		}
		if (reader.LocalName != "blipFill")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "blip":
					ParseBlipTag(reader, shape, relations, parentPath, holder, lstRelationIds, dictItemsToRemove);
					break;
				case "srcRect":
				{
					MemoryStream memoryStream = new MemoryStream();
					XmlWriter xmlWriter = UtilityMethods.CreateWriter(memoryStream, Encoding.UTF8);
					xmlWriter.WriteNode(reader, defattr: false);
					xmlWriter.Flush();
					shape.SourceRectStream = memoryStream;
					memoryStream.Position = 0L;
					XmlReader xmlReader = UtilityMethods.CreateReader(memoryStream);
					if (xmlReader.MoveToAttribute("l"))
					{
						shape.CropLeftOffset = Convert.ToInt32(xmlReader.Value);
					}
					if (xmlReader.MoveToAttribute("t"))
					{
						shape.CropTopOffset = Convert.ToInt32(xmlReader.Value);
					}
					if (xmlReader.MoveToAttribute("r"))
					{
						shape.CropRightOffset = Convert.ToInt32(xmlReader.Value);
					}
					if (xmlReader.MoveToAttribute("b"))
					{
						shape.CropBottomOffset = Convert.ToInt32(xmlReader.Value);
					}
					break;
				}
				case "stretch":
				case "tile":
					reader.Skip();
					break;
				default:
					throw new XmlException("Unexpected xml tag.");
				}
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Read();
	}

	private void ParseBlipTag(XmlReader reader, BitmapShapeImpl shape, RelationCollection relations, string strParentPath, FileDataHolder holder, List<string> lstRelationIds, Dictionary<string, object> dictItemsToRemove)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (shape == null)
		{
			throw new ArgumentNullException("shape");
		}
		if (relations == null)
		{
			throw new ArgumentNullException("relations");
		}
		if (strParentPath == null)
		{
			throw new ArgumentNullException("strParentPath");
		}
		if (holder == null)
		{
			throw new ArgumentNullException("holder");
		}
		if (lstRelationIds == null)
		{
			throw new ArgumentNullException("lstRelationIds");
		}
		if (reader.MoveToAttribute("embed", "http://schemas.openxmlformats.org/officeDocument/2006/relationships"))
		{
			string value = reader.Value;
			Relation relation = relations[value];
			lstRelationIds.Add(value);
			if (relation == null)
			{
				throw new XmlException("Cannot find required relation");
			}
			ZipArchiveItem zipArchiveItem = holder[relation, strParentPath];
			if (relation.Target != "NULL")
			{
				Image image = holder.GetImage(zipArchiveItem.ItemName);
				shape.Picture = image;
				((WorkbookImpl)shape.Workbook).ShapesData.Pictures[(int)(shape.BlipId - 1)].PicturePath = zipArchiveItem.ItemName;
				dictItemsToRemove[zipArchiveItem.ItemName] = null;
			}
		}
		reader.MoveToElement();
		if (!reader.IsEmptyElement)
		{
			MemoryStream memoryStream = new MemoryStream();
			XmlWriter xmlWriter = UtilityMethods.CreateWriter(memoryStream, Encoding.UTF8);
			xmlWriter.WriteStartElement("root");
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.LocalName == "clrChange")
				{
					shape.HasTransparency = true;
				}
				xmlWriter.WriteNode(reader, defattr: false);
			}
			xmlWriter.WriteEndElement();
			xmlWriter.Flush();
			shape.BlipSubNodesStream = memoryStream;
		}
		reader.Skip();
	}

	private void ParsePictureProperties(XmlReader reader, ShapeImpl shape, RelationCollection relations, string strParentPath, FileDataHolder holder, List<string> lstRelationIds, Dictionary<string, object> dictItemsToRemove)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (shape == null)
		{
			throw new ArgumentNullException("shape");
		}
		if (relations == null)
		{
			throw new ArgumentNullException("relations");
		}
		if (strParentPath == null)
		{
			throw new ArgumentNullException("strParentPath");
		}
		if (holder == null)
		{
			throw new ArgumentNullException("holder");
		}
		if (lstRelationIds == null)
		{
			throw new ArgumentNullException("lstRelationIds");
		}
		if (reader.LocalName != "nvPicPr")
		{
			throw new XmlException("Unexcpected xml tag");
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "cNvPr":
					ParseNVCanvasProperties(reader, shape);
					break;
				case "cNvPicPr":
					ParseNVPictureCanvas(reader, shape);
					break;
				case "hlinkClick":
					ParseClickHyperlink(reader, shape, relations, strParentPath, holder, lstRelationIds, dictItemsToRemove);
					break;
				default:
					throw new XmlException("Unexpected xml tag.");
				}
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Read();
	}

	private void ParseNVPictureCanvas(XmlReader reader, ShapeImpl shape)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (shape == null)
		{
			throw new ArgumentNullException("shape");
		}
		if (reader.LocalName != "cNvPicPr")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Skip();
	}

	public static void ParseNVCanvasProperties(XmlReader reader, IShape shape)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (shape == null)
		{
			throw new ArgumentNullException("shape");
		}
		if (reader.LocalName != "cNvPr")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		shape.IsShapeVisible = true;
		if (reader.MoveToAttribute("id"))
		{
			((ShapeImpl)shape).ShapeId = XmlConvertExtension.ToInt32(reader.Value);
		}
		if (reader.MoveToAttribute("name"))
		{
			shape.Name = reader.Value;
		}
		if (reader.MoveToAttribute("descr"))
		{
			shape.AlternativeText = reader.Value;
		}
		if (reader.MoveToAttribute("hidden"))
		{
			shape.IsShapeVisible = !XmlConvertExtension.ToBoolean(reader.Value);
		}
		reader.MoveToElement();
		reader.Read();
		if (reader.NodeType == XmlNodeType.EndElement)
		{
			reader.Read();
		}
	}

	private void ParseClickHyperlink(XmlReader reader, ShapeImpl shape, RelationCollection relations, string strParentPath, FileDataHolder holder, List<string> lstRelationIds, Dictionary<string, object> dictItemsToRemove)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (shape == null)
		{
			throw new ArgumentNullException("shape");
		}
		if (relations == null)
		{
			throw new ArgumentNullException("relations");
		}
		if (strParentPath == null)
		{
			throw new ArgumentNullException("strParentPath");
		}
		if (holder == null)
		{
			throw new ArgumentNullException("holder");
		}
		if (lstRelationIds == null)
		{
			throw new ArgumentNullException("lstRelationIds");
		}
		if (reader.MoveToAttribute("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships"))
		{
			string value = reader.Value;
			Relation imageRelation = relations[value];
			lstRelationIds.Add(value);
			shape.ImageRelation = imageRelation;
			shape.IsHyperlink = true;
			reader.Skip();
		}
		reader.Skip();
	}

	private void ParseCommentList(XmlReader reader, List<string> arrAuthors, WorksheetImpl sheet)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (arrAuthors == null)
		{
			throw new ArgumentNullException("arrAuthors");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (reader.LocalName != "commentList")
		{
			throw new XmlException("Unexpected tag");
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "comment")
			{
				ParseComment(reader, arrAuthors, sheet);
			}
			else
			{
				reader.Skip();
			}
		}
	}

	private void ParseComment(XmlReader reader, IList<string> authors, WorksheetImpl sheet)
	{
		reader.Skip();
	}

	private List<string> ParseAuthors(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "authors")
		{
			throw new XmlException();
		}
		List<string> list = new List<string>();
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "author")
			{
				list.Add(reader.ReadElementContentAsString());
			}
			else
			{
				reader.Read();
			}
		}
		reader.Read();
		return list;
	}

	private void ParseShape(XmlReader reader, Dictionary<string, ShapeImpl> dictShapeIdToShape, RelationCollection relations, string parentItemPath)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (dictShapeIdToShape == null)
		{
			throw new ArgumentNullException("dictShapeIdToShape");
		}
		if (parentItemPath == null || parentItemPath.Length == 0)
		{
			throw new ArgumentOutOfRangeException("parentItemPath");
		}
		if (!reader.MoveToAttribute("type"))
		{
			throw new XmlException();
		}
		string key = UtilityMethods.RemoveFirstCharUnsafe(reader.Value);
		if (!dictShapeIdToShape.TryGetValue(key, out var value))
		{
			reader.Skip();
			return;
		}
		int num = -1;
		num = ((!reader.MoveToAttribute("spt")) ? value.InnerSpRecord.Instance : int.Parse(reader.Value));
		if (!m_dictShapeParsers.TryGetValue(num, out var value2))
		{
			throw new XmlException();
		}
		reader.MoveToElement();
		MemoryStream memoryStream = new MemoryStream();
		XmlWriter xmlWriter = UtilityMethods.CreateWriter(memoryStream, Encoding.UTF8);
		xmlWriter.WriteNode(reader, defattr: false);
		xmlWriter.Flush();
		memoryStream.Position = 0L;
		reader = UtilityMethods.CreateReader(memoryStream);
		if (!value2.ParseShape(reader, value, relations, parentItemPath))
		{
			memoryStream.Position = 0L;
			reader = UtilityMethods.CreateReader(memoryStream);
			int instance = value.Instance;
			Stream xmlTypeStream = value.XmlTypeStream;
			value = new ShapeImpl(value.Application, value.Parent);
			value.VmlShape = true;
			value.XmlTypeStream = xmlTypeStream;
			value.SetInstance(instance);
			value2.ParseShape(reader, value, relations, parentItemPath);
		}
	}

	private void ParseShapeType(XmlReader reader, ShapeCollectionBase shapes, Dictionary<string, ShapeImpl> dictShapeIdToShape, Stream layoutStream)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (shapes == null)
		{
			throw new ArgumentNullException("shapes");
		}
		if (dictShapeIdToShape == null)
		{
			throw new ArgumentNullException("dictShapeIdToShape");
		}
		string text = null;
		string text2 = null;
		if (reader.MoveToAttribute("id"))
		{
			text = reader.Value;
		}
		if (reader.MoveToAttribute("spt", "urn:schemas-microsoft-com:office:office"))
		{
			text2 = reader.Value;
			if (text == null || text2 == null)
			{
				throw new XmlException();
			}
			int num = int.Parse(text2);
			reader.MoveToElement();
			if (!m_dictShapeParsers.TryGetValue(num, out var value))
			{
				value = new UnknownVmlShapeParser();
				m_dictShapeParsers[num] = value;
			}
			if (layoutStream != null)
			{
				shapes.ShapeLayoutStream = layoutStream;
			}
			ShapeImpl shapeImpl = value.ParseShapeType(reader, shapes);
			shapeImpl.SetInstance(num);
			shapeImpl.VmlShape = true;
			if (!dictShapeIdToShape.ContainsKey(text))
			{
				dictShapeIdToShape.Add(text, shapeImpl);
			}
		}
	}

	private void ParseShapeWithoutType(XmlReader reader, ShapeCollectionBase shapes, RelationCollection relations, string parentItemPath)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (parentItemPath == null || parentItemPath.Length == 0)
		{
			throw new ArgumentOutOfRangeException("parentItemPath");
		}
		if (reader.MoveToAttribute("type"))
		{
			throw new XmlException("shape type exists");
		}
		int num = -1;
		if (reader.MoveToAttribute("spt"))
		{
			num = int.Parse(reader.Value);
		}
		if (!m_dictShapeParsers.TryGetValue(num, out var value))
		{
			m_dictShapeParsers[num] = value;
		}
		ShapeImpl shapeImpl = new ShapeImpl(shapes.Application, shapes);
		shapeImpl.SetInstance(num);
		shapeImpl.VmlShape = true;
		reader.MoveToElement();
		MemoryStream memoryStream = new MemoryStream();
		XmlWriter xmlWriter = UtilityMethods.CreateWriter(memoryStream, Encoding.UTF8);
		xmlWriter.WriteNode(reader, defattr: false);
		xmlWriter.Flush();
		memoryStream.Position = 0L;
		reader = UtilityMethods.CreateReader(memoryStream);
		if (!value.ParseShape(reader, shapeImpl, relations, parentItemPath))
		{
			memoryStream.Position = 0L;
			reader = UtilityMethods.CreateReader(memoryStream);
			int instance = shapeImpl.Instance;
			Stream xmlTypeStream = shapeImpl.XmlTypeStream;
			shapeImpl = new ShapeImpl(shapeImpl.Application, shapeImpl.Parent);
			shapeImpl.VmlShape = true;
			shapeImpl.XmlTypeStream = xmlTypeStream;
			shapeImpl.SetInstance(instance);
			value.ParseShape(reader, shapeImpl, relations, parentItemPath);
		}
	}

	private int ParseRichTextRun(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "r")
		{
			throw new XmlException("reader");
		}
		SSTDictionary innerSST = m_book.InnerSST;
		TextWithFormat key = ParseTextWithFormat(reader, "si");
		return innerSST.AddIncrease(key, bIncrease: false);
	}

	private TextWithFormat ParseTextWithFormat(XmlReader reader, string closingTagName)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (closingTagName == null || closingTagName.Length == 0)
		{
			throw new ArgumentOutOfRangeException("closingTagName");
		}
		TextWithFormat textWithFormat = new TextWithFormat();
		while (reader.LocalName != closingTagName && reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "r")
			{
				ParseFormattingRun(reader, textWithFormat);
			}
			else
			{
				reader.Skip();
			}
		}
		return textWithFormat;
	}

	private void ParseFormattingRun(XmlReader reader, TextWithFormat textWithFormat)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textWithFormat == null)
		{
			throw new ArgumentNullException("textWithFormat");
		}
		int num = -1;
		int num2 = -1;
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement || reader.LocalName != "r")
		{
			if (reader.LocalName == "rPr")
			{
				num = ParseFont(reader, null);
				reader.Skip();
			}
			else if (reader.LocalName == "t")
			{
				if (!reader.IsEmptyElement)
				{
					num2 = textWithFormat.Text.Length;
					reader.Read();
					string value = reader.Value;
					value = value.Replace("\r", string.Empty);
					textWithFormat.Text += value;
					reader.Skip();
					if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "t")
					{
						reader.Skip();
					}
				}
				else
				{
					reader.Skip();
				}
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Skip();
		if (num >= 0 && num2 >= 0)
		{
			textWithFormat.SetTextFontIndex(num2, textWithFormat.Text.Length - 1, num);
		}
	}

	private int ParseText(XmlReader reader, bool setCount)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "t")
		{
			throw new XmlException("reader");
		}
		SSTDictionary innerSST = m_book.InnerSST;
		bool flag = false;
		if (reader.XmlSpace == XmlSpace.Preserve)
		{
			flag = true;
		}
		reader.Read();
		string text = XmlConvert.DecodeName(reader.Value);
		text = text.Replace("\r", string.Empty);
		if (flag)
		{
			TextWithFormat textWithFormat = new TextWithFormat();
			textWithFormat.IsPreserved = true;
			textWithFormat.Text = text;
			reader.Skip();
			reader.Skip();
			return innerSST.AddIncrease(textWithFormat, bIncrease: false);
		}
		int result = innerSST.AddIncrease(text, setCount);
		reader.Skip();
		if (!string.IsNullOrEmpty(text))
		{
			reader.Skip();
		}
		return result;
	}

	private int ParseText(XmlReader reader, bool setCount, out string text)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "t")
		{
			throw new XmlException("reader");
		}
		SSTDictionary innerSST = m_book.InnerSST;
		reader.Read();
		string key = (text = XmlConvert.DecodeName(reader.Value)).Replace("\r", string.Empty);
		int result = innerSST.AddIncrease(key, setCount);
		reader.Skip();
		reader.Skip();
		return result;
	}

	private List<int> ParseNamedStyles(XmlReader reader, List<int> arrFontIndexes, List<FillImpl> arrFills, List<BordersCollection> arrBorders, Dictionary<int, int> arrNumberFormatIndexes)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (arrFontIndexes == null)
		{
			throw new ArgumentNullException("arrFontIndexes");
		}
		if (arrFills == null)
		{
			throw new ArgumentNullException("arrFills");
		}
		if (arrBorders == null)
		{
			throw new ArgumentNullException("arrBorders");
		}
		if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "cellStyleXfs")
		{
			throw new XmlException("Unexpected xml tag " + reader.LocalName);
		}
		if (reader.IsEmptyElement)
		{
			return null;
		}
		reader.Read();
		List<int> list = new List<int>();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				ExtendedFormatImpl extendedFormatImpl = ParseExtendedFormat(reader, arrFontIndexes, arrFills, arrBorders, null, null, arrNumberFormatIndexes);
				extendedFormatImpl.Record.ParentIndex = (ushort)m_book.MaxXFCount;
				extendedFormatImpl = m_book.InnerExtFormats.ForceAdd(extendedFormatImpl);
				list.Add(extendedFormatImpl.Index);
			}
			reader.Read();
		}
		return list;
	}

	private List<int> ParseCellFormats(XmlReader reader, List<int> arrNewFontIndexes, List<FillImpl> arrFills, List<BordersCollection> arrBorders, List<int> namedStyleIndexes, Dictionary<int, int> arrNumberFormatIndexes)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (namedStyleIndexes == null)
		{
			m_book.InsertDefaultFonts();
			m_book.InsertDefaultValues();
		}
		if (arrNewFontIndexes == null)
		{
			throw new ArgumentNullException("arrNewFontIndexes");
		}
		if (arrFills == null)
		{
			throw new ArgumentNullException("arrFills");
		}
		if (arrBorders == null)
		{
			throw new ArgumentNullException("arrBorders");
		}
		if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "cellXfs")
		{
			throw new XmlException("Unexpected xml tag " + reader.LocalName);
		}
		if (reader.IsEmptyElement)
		{
			return null;
		}
		reader.Read();
		List<int> list = new List<int>();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				ExtendedFormatImpl format = ParseExtendedFormat(reader, arrNewFontIndexes, arrFills, arrBorders, namedStyleIndexes, false, arrNumberFormatIndexes);
				format = m_book.InnerExtFormats.Add(format);
				list.Add(format.Index);
			}
			reader.Read();
		}
		if (!arrBorders[0].IsEmptyBorder)
		{
			arrBorders.RemoveAt(0);
		}
		return list;
	}

	private void ParseStyles(XmlReader reader, List<int> arrNamedStyleIndexes)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (arrNamedStyleIndexes == null)
		{
			m_book.InsertDefaultFonts();
			m_book.InsertDefaultValues();
		}
		if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "cellStyles")
		{
			throw new XmlException("Unexpected xml element " + reader.LocalName);
		}
		reader.Read();
		m_book.InnerStyles.Clear();
		List<int> validate = new List<int>();
		while (reader.NodeType != XmlNodeType.EndElement || reader.LocalName != "cellStyles")
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				if (!(reader.LocalName == "cellStyle"))
				{
					throw new XmlException("Unexpected xml tag " + reader.LocalName);
				}
				ParseStyle(reader, arrNamedStyleIndexes, ref validate);
			}
			reader.Read();
		}
	}

	private static string GetASCIIString(string plainText)
	{
		string[] array = plainText.Split(new char[1] { '_' }, StringSplitOptions.RemoveEmptyEntries);
		List<string> list = new List<string>();
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (text.StartsWith("x"))
			{
				list.Add(ConvertHextoAscii(text));
			}
			else
			{
				list.Add(text);
			}
		}
		return string.Join("", list.ToArray()).Replace("\0", string.Empty);
	}

	private static string ConvertHextoAscii(string HexString)
	{
		HexString = HexString.TrimStart(new char[1] { 'x' });
		string text = "";
		for (int i = 0; i < HexString.Length; i += 2)
		{
			if (HexString.Length >= i + 2)
			{
				HexString.Substring(i, 2);
				text += Convert.ToChar(Convert.ToUInt32(HexString.Substring(i, 2), 16));
			}
		}
		return text;
	}

	private void ParseStyle(XmlReader reader, List<int> arrNamedStyleIndexes, ref List<int> validate)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (arrNamedStyleIndexes == null)
		{
			throw new ArgumentNullException("arrNamedStyleIndexes");
		}
		if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "cellStyle")
		{
			throw new XmlException("Unexpected xml item " + reader.LocalName);
		}
		StyleRecord styleRecord = (StyleRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Style);
		if (reader.MoveToAttribute("name"))
		{
			string value = reader.Value;
			if (value.Length > 255)
			{
				styleRecord.IsAsciiConverted = true;
				styleRecord.StyleName = GetASCIIString(value);
				styleRecord.StyleNameCache = value;
			}
			else
			{
				styleRecord.StyleName = value;
			}
		}
		if (reader.MoveToAttribute("xfId"))
		{
			int index = XmlConvertExtension.ToInt32(reader.Value);
			styleRecord.ExtendedFormatIndex = (ushort)arrNamedStyleIndexes[index];
			if (m_book.Version == OfficeVersion.Excel97to2003)
			{
				styleRecord.DefXFIndex = 0;
			}
			else
			{
				styleRecord.DefXFIndex = ushort.MaxValue;
			}
		}
		if (reader.MoveToAttribute("builtinId"))
		{
			int num = XmlConvertExtension.ToInt32(reader.Value);
			styleRecord.BuildInOrNameLen = (byte)num;
			styleRecord.IsBuildInStyle = true;
		}
		if (reader.MoveToAttribute("customBuiltin"))
		{
			styleRecord.IsBuiltIncustomized = ParseBoolean(reader, "customBuiltin", defaultValue: false);
		}
		if (reader.MoveToAttribute("iLevel"))
		{
			styleRecord.OutlineStyleLevel = XmlConvertExtension.ToByte(reader.Value);
		}
		if (!validate.Contains(styleRecord.ExtendedFormatIndex) || styleRecord.IsBuildInStyle)
		{
			validate.Add(styleRecord.ExtendedFormatIndex);
			m_book.InnerStyles.Add(styleRecord);
		}
	}

	private ExtendedFormatImpl ParseExtendedFormat(XmlReader reader, List<int> arrFontIndexes, List<FillImpl> arrFills, List<BordersCollection> arrBorders, List<int> namedStyleIndexes, bool? includeDefault, Dictionary<int, int> arrNumberFormatIndexes)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (arrFontIndexes == null)
		{
			throw new ArgumentNullException("arrFontIndexes");
		}
		if (arrFills == null)
		{
			throw new ArgumentNullException("arrFills");
		}
		if (arrBorders == null)
		{
			throw new ArgumentNullException("arrBorders");
		}
		if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "xf")
		{
			throw new XmlException("Unexpected xml tag " + reader.LocalName);
		}
		ExtendedFormatImpl extendedFormatImpl = new ExtendedFormatImpl(m_book.Application, m_book);
		ExtendedFormatRecord record = extendedFormatImpl.Record;
		_ = extendedFormatImpl.XFRecord;
		if (reader.MoveToAttribute("xfId"))
		{
			int index = XmlConvertExtension.ToUInt16(reader.Value);
			if (namedStyleIndexes != null)
			{
				record.ParentIndex = (ushort)namedStyleIndexes[index];
			}
			record.XFType = ExtendedFormatRecord.TXFType.XF_STYLE;
		}
		else if (namedStyleIndexes != null)
		{
			record.ParentIndex = (ushort)namedStyleIndexes[0];
			record.XFType = ExtendedFormatRecord.TXFType.XF_STYLE;
		}
		else
		{
			record.ParentIndex = (ushort)m_book.MaxXFCount;
			record.XFType = ExtendedFormatRecord.TXFType.XF_CELL;
		}
		ParseFontFillBorder(reader, extendedFormatImpl, arrFontIndexes, arrFills, arrBorders);
		if (reader.MoveToAttribute("numFmtId"))
		{
			int num = XmlConvertExtension.ToUInt16(reader.Value);
			if (arrNumberFormatIndexes != null && arrNumberFormatIndexes.ContainsKey(num))
			{
				record.FormatIndex = (ushort)arrNumberFormatIndexes[num];
			}
			else
			{
				record.FormatIndex = (ushort)num;
			}
		}
		if (reader.MoveToAttribute("pivotButton"))
		{
			extendedFormatImpl.PivotButton = XmlConvertExtension.ToBoolean(reader.Value);
		}
		ParseIncludeAttributes(reader, extendedFormatImpl, includeDefault, out var hasAlignment, arrNumberFormatIndexes);
		extendedFormatImpl.HorizontalAlignment = OfficeHAlign.HAlignGeneral;
		extendedFormatImpl.VerticalAlignment = OfficeVAlign.VAlignBottom;
		ParseAlignmentAndProtection(reader, extendedFormatImpl, hasAlignment);
		return m_book.AddExtendedProperties(extendedFormatImpl);
	}

	private void ParseAlignmentAndProtection(XmlReader reader, ExtendedFormatImpl format, bool hasAlignment)
	{
		ExtendedFormatRecord record = format.Record;
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		reader.MoveToElement();
		if (reader.IsEmptyElement)
		{
			return;
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement || reader.LocalName != "xf")
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName = reader.LocalName;
				if (!(localName == "alignment"))
				{
					if (!(localName == "protection"))
					{
						throw new NotImplementedException(reader.LocalName);
					}
					ParseProtection(reader, record);
				}
				else
				{
					ParseAlignment(reader, record);
					if (!hasAlignment)
					{
						format.IncludeAlignment = true;
					}
				}
			}
			reader.Read();
		}
	}

	private void ParseAlignment(XmlReader reader, ExtendedFormatRecord record)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "alignment")
		{
			throw new XmlException();
		}
		if (reader.MoveToAttribute("horizontal"))
		{
			string value = reader.Value;
			record.HAlignmentType = (OfficeHAlign)Enum.Parse(typeof(Excel2007HAlign), value, ignoreCase: true);
		}
		if (reader.MoveToAttribute("indent"))
		{
			record.Indent = XmlConvertExtension.ToByte(reader.Value);
		}
		if (reader.MoveToAttribute("justifyLastLine"))
		{
			record.JustifyLast = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("readingOrder"))
		{
			record.ReadingOrder = XmlConvertExtension.ToUInt16(reader.Value);
		}
		if (reader.MoveToAttribute("shrinkToFit"))
		{
			record.ShrinkToFit = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("textRotation"))
		{
			record.Rotation = XmlConvertExtension.ToUInt16(reader.Value);
		}
		if (reader.MoveToAttribute("wrapText"))
		{
			record.WrapText = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("vertical"))
		{
			string value2 = reader.Value;
			record.VAlignmentType = (OfficeVAlign)Enum.Parse(typeof(Excel2007VAlign), value2, ignoreCase: true);
		}
	}

	private void ParseProtection(XmlReader reader, ExtendedFormatRecord record)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "protection")
		{
			throw new XmlException("Unable to locate necessary xml tag");
		}
		if (reader.MoveToAttribute("hidden"))
		{
			record.IsHidden = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("locked"))
		{
			record.IsLocked = XmlConvertExtension.ToBoolean(reader.Value);
		}
	}

	private void ParseIncludeAttributes(XmlReader reader, ExtendedFormatImpl format, bool? defaultValue, out bool hasAlignment, Dictionary<int, int> arrNumberFormatIndexes)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		hasAlignment = false;
		ExtendedFormatRecord record = format.Record;
		if (reader.MoveToAttribute("applyAlignment"))
		{
			format.IncludeAlignment = XmlConvertExtension.ToBoolean(reader.Value);
			if (format.IncludeAlignment)
			{
				hasAlignment = true;
			}
		}
		else if (defaultValue.HasValue)
		{
			record.IsNotParentAlignment = defaultValue.Value;
		}
		if (reader.MoveToAttribute("applyBorder"))
		{
			format.IncludeBorder = XmlConvertExtension.ToBoolean(reader.Value);
		}
		else if (defaultValue.HasValue && record.BorderIndex < 1)
		{
			record.IsNotParentBorder = defaultValue.Value;
		}
		else if (record.BorderIndex > 0)
		{
			format.IncludeBorder = true;
		}
		if (reader.MoveToAttribute("applyFont"))
		{
			format.IncludeFont = XmlConvertExtension.ToBoolean(reader.Value);
		}
		else if (defaultValue.HasValue && record.FontIndex < 1)
		{
			record.IsNotParentFont = defaultValue.Value;
		}
		else if (record.FontIndex > 0)
		{
			format.IncludeFont = true;
		}
		if (reader.MoveToAttribute("applyNumberFormat"))
		{
			if (arrNumberFormatIndexes != null && arrNumberFormatIndexes.ContainsValue(record.FormatIndex))
			{
				format.IncludeNumberFormat = true;
			}
			else
			{
				format.IncludeNumberFormat = XmlConvertExtension.ToBoolean(reader.Value);
			}
		}
		else if (defaultValue.HasValue)
		{
			record.IsNotParentFormat = defaultValue.Value;
			if (record.FormatIndex > 0)
			{
				format.IncludeNumberFormat = true;
			}
		}
		if (reader.MoveToAttribute("applyFill"))
		{
			format.IncludePatterns = XmlConvertExtension.ToBoolean(reader.Value);
		}
		else if (defaultValue.HasValue && record.FillIndex < 1)
		{
			record.IsNotParentPattern = defaultValue.Value;
		}
		else if (record.FillIndex > 0)
		{
			format.IncludePatterns = true;
		}
		if (reader.MoveToAttribute("applyProtection"))
		{
			format.IncludeProtection = XmlConvertExtension.ToBoolean(reader.Value);
		}
		else if (defaultValue.HasValue)
		{
			record.IsNotParentCellOptions = defaultValue.Value;
		}
		defaultValue = false;
		if (reader.MoveToAttribute("quotePrefix"))
		{
			format.IsFirstSymbolApostrophe = XmlConvertExtension.ToBoolean(reader.Value);
		}
		else if (defaultValue.HasValue)
		{
			record._123Prefix = defaultValue.Value;
		}
	}

	private void ParseFontFillBorder(XmlReader reader, ExtendedFormatImpl extendedFormat, List<int> arrFontIndexes, List<FillImpl> arrFills, List<BordersCollection> arrBorders)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (extendedFormat == null)
		{
			throw new ArgumentNullException("extendedFormat");
		}
		if (arrFontIndexes == null)
		{
			throw new ArgumentNullException("arrFontIndexes");
		}
		if (arrFills == null)
		{
			throw new ArgumentNullException("arrFills");
		}
		if (arrBorders == null)
		{
			throw new ArgumentNullException("arrBorders");
		}
		ExtendedFormatRecord record = extendedFormat.Record;
		if (reader.MoveToAttribute("fontId"))
		{
			int index = XmlConvertExtension.ToInt32(reader.Value);
			record.FontIndex = (ushort)arrFontIndexes[index];
		}
		if (reader.MoveToAttribute("fillId"))
		{
			int num = XmlConvertExtension.ToInt32(reader.Value);
			record.FillIndex = (ushort)num;
			CopyFillSettings(arrFills[num], extendedFormat);
		}
		if (!reader.MoveToAttribute("borderId"))
		{
			return;
		}
		int num2 = XmlConvertExtension.ToInt32(reader.Value);
		record.BorderIndex = (ushort)num2;
		if (num2 > 0)
		{
			extendedFormat.HasBorder = true;
		}
		if (num2 == arrBorders.Count)
		{
			num2 = arrBorders.Count - 1;
		}
		if (num2 == -1)
		{
			return;
		}
		BordersCollection bordersCollection = arrBorders[num2];
		if (num2 == 0 && !bordersCollection.IsEmptyBorder)
		{
			for (int i = 1; i < arrBorders.Count; i++)
			{
				BordersCollection bordersCollection2 = arrBorders[i];
				if (bordersCollection2.IsEmptyBorder)
				{
					CopyBorderSettings(bordersCollection2, extendedFormat);
					break;
				}
			}
		}
		else
		{
			CopyBorderSettings(bordersCollection, extendedFormat);
		}
	}

	private void CopyBorderSettings(BordersCollection borders, ExtendedFormatImpl format)
	{
		if (borders == null)
		{
			throw new ArgumentNullException("borders");
		}
		if (format == null)
		{
			throw new ArgumentNullException("record");
		}
		ExtendedFormatRecord record = format.Record;
		IBorder border = borders[OfficeBordersIndex.EdgeLeft];
		if (border != null && (format.IncludeBorder || BordersDifferent(border, format.LeftBorderColor, format.LeftBorderLineStyle)))
		{
			format.IncludeBorder = true;
			format.LeftBorderColor.CopyFrom(border.ColorObject, callEvent: true);
			format.LeftBorderLineStyle = border.LineStyle;
		}
		else if (border != null)
		{
			record.BorderLeft = border.LineStyle;
		}
		border = borders[OfficeBordersIndex.EdgeRight];
		if (border != null && (format.IncludeBorder || BordersDifferent(border, format.RightBorderColor, format.RightBorderLineStyle)))
		{
			format.IncludeBorder = true;
			format.RightBorderColor.CopyFrom(border.ColorObject, callEvent: true);
			format.RightBorderLineStyle = border.LineStyle;
		}
		else if (border != null)
		{
			record.BorderRight = border.LineStyle;
		}
		border = borders[OfficeBordersIndex.EdgeTop];
		if (border != null && (format.IncludeBorder || BordersDifferent(border, format.TopBorderColor, format.TopBorderLineStyle)))
		{
			format.IncludeBorder = true;
			format.TopBorderColor.CopyFrom(border.ColorObject, callEvent: true);
			format.TopBorderLineStyle = border.LineStyle;
		}
		else if (border != null)
		{
			record.BorderTop = border.LineStyle;
		}
		border = borders[OfficeBordersIndex.EdgeBottom];
		if (border != null && (format.IncludeBorder || BordersDifferent(border, format.BottomBorderColor, format.BottomBorderLineStyle)))
		{
			format.IncludeBorder = true;
			format.BottomBorderColor.CopyFrom(border.ColorObject, callEvent: true);
			format.BottomBorderLineStyle = border.LineStyle;
		}
		else if (border != null)
		{
			record.BorderBottom = border.LineStyle;
		}
		border = borders[OfficeBordersIndex.DiagonalDown];
		if (border != null)
		{
			if (format.IncludeBorder || BordersDifferent(border, format.DiagonalBorderColor, format.DiagonalDownBorderLineStyle))
			{
				format.IncludeBorder = true;
				format.DiagonalBorderColor.CopyFrom(border.ColorObject, callEvent: true);
				format.DiagonalDownBorderLineStyle = border.LineStyle;
			}
			else
			{
				record.DiagonalLineStyle = (ushort)border.LineStyle;
				record.DiagonalFromTopLeft = border.ShowDiagonalLine;
			}
			format.DiagonalDownVisible = border.ShowDiagonalLine;
		}
		border = borders[OfficeBordersIndex.DiagonalUp];
		if (border != null)
		{
			if (format.IncludeBorder || BordersDifferent(border, format.DiagonalBorderColor, format.DiagonalUpBorderLineStyle))
			{
				format.IncludeBorder = true;
				format.DiagonalBorderColor.CopyFrom(border.ColorObject, callEvent: true);
				format.DiagonalUpBorderLineStyle = border.LineStyle;
			}
			else
			{
				record.DiagonalLineStyle = (ushort)border.LineStyle;
				record.DiagonalFromBottomLeft = border.ShowDiagonalLine;
			}
			format.DiagonalUpVisible = border.ShowDiagonalLine;
		}
	}

	private static bool BordersDifferent(IBorder border, ChartColor color, OfficeLineStyle lineStyle)
	{
		ChartColor colorObject = border.ColorObject;
		if (colorObject.ColorType != color.ColorType && colorObject.Value == color.Value)
		{
			return border.LineStyle != lineStyle;
		}
		return true;
	}

	private static void ParseRelation(XmlReader reader, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (relations == null)
		{
			throw new ArgumentNullException("relations");
		}
		string id = null;
		string type = null;
		string text = null;
		bool isExternal = false;
		string text2 = "styles.xml";
		if (reader.MoveToAttribute("Id"))
		{
			id = reader.Value;
		}
		if (reader.MoveToAttribute("Type"))
		{
			type = reader.Value;
		}
		if (reader.MoveToAttribute("Target"))
		{
			text = reader.Value;
			if (text.ToLower().Contains(text2))
			{
				text = text2;
			}
		}
		if (reader.MoveToAttribute("TargetMode"))
		{
			isExternal = reader.Value == "External";
		}
		Relation value = new Relation(text, type, isExternal);
		relations[id] = value;
	}

	private void ParseSheetsOptions(XmlReader reader, RelationCollection relations, FileDataHolder holder, string bookPath)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (relations == null)
		{
			throw new ArgumentNullException("relations");
		}
		if (reader.LocalName != "sheets")
		{
			throw new XmlException("Unexpected tag name " + reader.LocalName);
		}
		m_book.Objects.Clear();
		m_book.InnerWorksheets.Clear();
		reader.Read();
		int num = 0;
		while (reader.NodeType != XmlNodeType.EndElement || reader.LocalName != "sheets")
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				if (reader.LocalName == "sheet")
				{
					ParseWorkbookSheetEntry(reader, relations, holder, bookPath, ++num);
				}
				else
				{
					reader.Skip();
				}
			}
			else
			{
				reader.Read();
			}
		}
		reader.Read();
	}

	private void ParseWorkbookSheetEntry(XmlReader reader, RelationCollection relations, FileDataHolder holder, string bookPath, int sheetRelationIdCount)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (relations == null)
		{
			throw new ArgumentNullException("relations");
		}
		string text = null;
		string text2 = null;
		string text3 = null;
		string sheetId = null;
		if (reader.MoveToAttribute("name"))
		{
			text = reader.Value;
		}
		if (reader.MoveToAttribute("state"))
		{
			text2 = reader.Value;
		}
		if (reader.MoveToAttribute("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships"))
		{
			text3 = reader.Value;
		}
		if (reader.MoveToAttribute("sheetId"))
		{
			sheetId = reader.Value;
		}
		if (text3 == "" && text2 == "veryHidden")
		{
			return;
		}
		Relation relation = relations[text3];
		if (relation == null)
		{
			relation = new Relation($"worksheets/sheet{sheetRelationIdCount}.xml", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet");
			string text4 = $"rId{relations.Count}";
			relations[text4] = relation;
			text3 = text4;
		}
		WorksheetBaseImpl worksheetBaseImpl = null;
		string type = relation.Type;
		if (!(type == "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"))
		{
			if (!(type == "http://schemas.openxmlformats.org/officeDocument/2006/relationships/chartsheet"))
			{
				throw new XmlException("Unknown part type: " + relation.Type);
			}
			worksheetBaseImpl = (WorksheetBaseImpl)m_book.InnerCharts.Add(text);
			worksheetBaseImpl.PageSetupBase.Orientation = OfficePageOrientation.Landscape;
		}
		else
		{
			worksheetBaseImpl = (WorksheetBaseImpl)m_book.InnerWorksheets.Add(text);
		}
		worksheetBaseImpl.DataHolder = new WorksheetDataHolder(holder, relation, bookPath);
		worksheetBaseImpl.m_dataHolder.RelationId = text3;
		worksheetBaseImpl.m_dataHolder.SheetId = sheetId;
		worksheetBaseImpl.IsSaved = true;
		bool num = !(worksheetBaseImpl.Workbook as WorkbookImpl).IsWorkbookOpening;
		if (num)
		{
			(worksheetBaseImpl.Workbook as WorkbookImpl).IsWorkbookOpening = true;
		}
		SetVisibilityState(worksheetBaseImpl, text2);
		if (num)
		{
			(worksheetBaseImpl.Workbook as WorkbookImpl).IsWorkbookOpening = false;
		}
		relations.Remove(text3);
	}

	private void SetVisibilityState(WorksheetBaseImpl sheet, string strVisibility)
	{
		if (strVisibility != null)
		{
			switch (strVisibility)
			{
			case "hidden":
				sheet.Visibility = OfficeWorksheetVisibility.Hidden;
				break;
			case "veryHidden":
				sheet.Visibility = OfficeWorksheetVisibility.StrongHidden;
				break;
			case "visible":
				sheet.Visibility = OfficeWorksheetVisibility.Visible;
				break;
			default:
				throw new ArgumentException("Unknown visibility state type");
			}
		}
	}

	private void ParseDictionaryEntry(XmlReader reader, IDictionary<string, string> dictionary, string keyAttribute, string valueAttribute)
	{
		string text = null;
		string text2 = null;
		if (reader.MoveToAttribute(keyAttribute))
		{
			text = reader.Value;
		}
		if (reader.MoveToAttribute(valueAttribute))
		{
			text2 = reader.Value;
		}
		if (text == null || text2 == null)
		{
			throw new XmlReadingException("Unable to parse dictionary entry item from Content type");
		}
		dictionary.Add(text, text2);
	}

	private void ParseMergeRegion(XmlReader reader, WorksheetImpl sheet)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (reader.LocalName == "mergeCell")
		{
			if (!reader.MoveToAttribute("ref"))
			{
				throw new Exception("Unsupported merged cells format");
			}
			string value = reader.Value;
			(sheet.Range[value] as RangeImpl).MergeWithoutCheck();
			reader.Skip();
		}
	}

	private string ParseNamedRange(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		string text = null;
		string text2 = null;
		bool flag = false;
		int num = -1;
		bool flag2 = false;
		if (reader.LocalName == "definedName")
		{
			if (!reader.MoveToAttribute("name"))
			{
				throw new ApplicationException("Cannot find name for named range");
			}
			text = reader.Value;
			if (reader.MoveToAttribute("localSheetId"))
			{
				flag = true;
				num = int.Parse(reader.Value);
			}
			if (reader.MoveToAttribute("hidden"))
			{
				flag2 = XmlConvertExtension.ToBoolean(reader.Value);
			}
			WorksheetImpl worksheetImpl = (flag ? (m_book.Objects[num] as WorksheetImpl) : null);
			IName name = ((!flag && worksheetImpl == null) ? m_book.Names.Add(text) : worksheetImpl.Names.Add(text));
			reader.Read();
			text2 = reader.Value;
			m_book.HasApostrophe = text2.Contains("'");
			NameImpl nameImpl = (NameImpl)name;
			nameImpl.Visible = !flag2;
			if (flag)
			{
				nameImpl.Record.IndexOrGlobal = (ushort)(num + 1);
			}
			reader.Skip();
		}
		return text2;
	}

	private FormatImpl ParseDxfNumberFormat(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "numFmt")
		{
			throw new XmlException("Unexpected tag " + reader.LocalName);
		}
		int num = -1;
		string text = null;
		if (reader.MoveToAttribute("formatCode"))
		{
			text = reader.Value;
			if (reader.MoveToAttribute("numFmtId"))
			{
				num = Convert.ToInt32(reader.Value);
				if (text != string.Empty)
				{
					if (!m_book.InnerFormats.ContainsFormat(text))
					{
						if (Array.IndexOf(DEF_NUMBERFORMAT_INDEXES, num) < 0)
						{
							num = 163 + m_book.InnerFormats.Count - 36 + 1;
						}
					}
					else
					{
						num = m_book.InnerFormats[text].Index;
					}
				}
				m_book.InnerFormats.Add(num, text);
				return m_book.InnerFormats[num];
			}
			throw new XmlException("numFmtId wasn't found");
		}
		throw new XmlException("formatCode wasn't found");
	}

	private List<int> ParseFonts(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		List<int> list = new List<int>();
		if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "fonts")
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					ParseFont(reader, list);
				}
				reader.Read();
			}
		}
		return list;
	}

	private int ParseFont(XmlReader reader, List<int> fontIndexes)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		FontImpl font = (FontImpl)m_book.CreateFont(null, bAddToCollection: false);
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType == XmlNodeType.Whitespace)
			{
				reader.Read();
			}
			ParseFontSettings(reader, font);
		}
		font = (FontImpl)m_book.InnerFonts.Add(font);
		fontIndexes?.Add(font.Index);
		return font.Index;
	}

	private FontImpl ParseFont(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		FontImpl fontImpl = new FontImpl(m_book.Application, m_book);
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			ParseFontSettings(reader, fontImpl);
		}
		return fontImpl;
	}

	private void ParseFontSettings(XmlReader reader, FontImpl font)
	{
		while (reader.NodeType != XmlNodeType.EndElement || (reader.LocalName != "font" && reader.LocalName != "rPr"))
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "b":
					font.Bold = ParseBoolean(reader, "val", defaultValue: true);
					break;
				case "i":
					font.Italic = ParseBoolean(reader, "val", defaultValue: true);
					break;
				case "rFont":
				case "name":
					font.FontName = ParseValue(reader, "val");
					break;
				case "sz":
				{
					string s = ParseValue(reader, "val");
					font.Size = double.Parse(s, CultureInfo.InvariantCulture);
					break;
				}
				case "strike":
					font.Strikethrough = ParseBoolean(reader, "val", defaultValue: true);
					break;
				case "u":
				{
					string text = ParseValue(reader, "val");
					int underline;
					if (text == null)
					{
						OfficeUnderline officeUnderline2 = (font.Underline = OfficeUnderline.Single);
						underline = (int)officeUnderline2;
					}
					else
					{
						underline = (int)(OfficeUnderline)Enum.Parse(typeof(OfficeUnderline), text, ignoreCase: true);
					}
					font.Underline = (OfficeUnderline)underline;
					break;
				}
				case "vertAlign":
				{
					string value = ParseValue(reader, "val");
					font.VerticalAlignment = (OfficeFontVerticalAlignment)Enum.Parse(typeof(OfficeFontVerticalAlignment), value, ignoreCase: true);
					break;
				}
				case "shadow":
					font.MacOSShadow = ParseBoolean(reader, "val", defaultValue: true);
					break;
				case "color":
					font.ColorObject.CopyFrom(ParseColor(reader), callEvent: true);
					break;
				case "charset":
					font.CharSet = ParseCharSet(reader);
					break;
				case "family":
					font.Family = ParseFamily(reader);
					break;
				}
				reader.Read();
			}
			else
			{
				reader.Skip();
			}
		}
	}

	private byte ParseFamily(XmlReader reader)
	{
		byte result = 0;
		if (reader.MoveToAttribute("val"))
		{
			result = byte.Parse(reader.Value);
		}
		return result;
	}

	private byte ParseCharSet(XmlReader reader)
	{
		byte result = 1;
		if (reader.MoveToAttribute("val"))
		{
			result = byte.Parse(reader.Value);
		}
		return result;
	}

	private Dictionary<int, int> ParseNumberFormats(XmlReader reader)
	{
		Dictionary<int, int> result = new Dictionary<int, int>();
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "numFmts")
		{
			throw new XmlException("Unexpected tag " + reader.LocalName);
		}
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement || reader.LocalName != "numFmts")
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					ParseNumberFormat(reader, result);
				}
				reader.Read();
			}
		}
		return result;
	}

	private void ParseNumberFormat(XmlReader reader, Dictionary<int, int> result)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		int num = -1;
		string text = null;
		if (reader.MoveToAttribute("formatCode"))
		{
			text = reader.Value;
			if (reader.MoveToAttribute("numFmtId"))
			{
				num = Convert.ToInt32(reader.Value);
				int num2 = num;
				if (text != string.Empty)
				{
					if (!m_book.InnerFormats.ContainsFormat(text))
					{
						if (Array.IndexOf(DEF_NUMBERFORMAT_INDEXES, num) < 0)
						{
							num2 = 163 + m_book.InnerFormats.Count - 36 + 1;
						}
					}
					else
					{
						num2 = m_book.InnerFormats[text].Index;
					}
					result.Add(num, num2);
					num = num2;
				}
				m_book.InnerFormats.Add(num, text);
				m_book.InnerFormats.HasNumberFormats = true;
				return;
			}
			throw new XmlException("numFmtId wasn't found");
		}
		throw new XmlException("formatCode wasn't found");
	}

	private ChartColor ParseColor(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		ChartColor chartColor = new ChartColor(OfficeKnownColors.BlackCustom);
		ParseColor(reader, chartColor);
		return chartColor;
	}

	private void ParseColor(XmlReader reader, ChartColor color)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.MoveToAttribute("indexed"))
		{
			OfficeKnownColors value = (OfficeKnownColors)Convert.ToInt32(reader.Value);
			color.SetIndexed(value, raiseEvent: true, m_book);
			return;
		}
		Color empty = ColorExtension.Empty;
		double dTintValue = 0.0;
		if (reader.MoveToAttribute("tint"))
		{
			dTintValue = XmlConvertExtension.ToDouble(reader.Value);
		}
		if (reader.MoveToAttribute("rgb"))
		{
			empty = ColorExtension.FromArgb(int.Parse(reader.Value, NumberStyles.HexNumber));
			color.SetRGB(empty, m_book, dTintValue);
			color.ColorType = ColorType.RGB;
		}
		else if (reader.MoveToAttribute("theme"))
		{
			int themeIndex = Convert.ToInt32(reader.Value);
			color.SetTheme(themeIndex, m_book, dTintValue);
		}
	}

	private void ParseFillColor(XmlReader reader, ChartColor color)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.MoveToAttribute("indexed"))
		{
			OfficeKnownColors value = (OfficeKnownColors)Convert.ToInt32(reader.Value);
			color.SetIndexed(value, raiseEvent: true, m_book);
			return;
		}
		Color empty = ColorExtension.Empty;
		double dTintValue = 0.0;
		if (reader.MoveToAttribute("auto"))
		{
			int themeIndex = 0;
			color.SetTheme(themeIndex, m_book, dTintValue);
		}
		if (reader.MoveToAttribute("tint"))
		{
			dTintValue = XmlConvertExtension.ToDouble(reader.Value);
		}
		if (reader.MoveToAttribute("rgb"))
		{
			empty = ColorExtension.FromArgb(int.Parse(reader.Value, NumberStyles.HexNumber));
			color.SetRGB(empty, m_book, dTintValue);
			color.ColorType = ColorType.RGB;
		}
		else if (reader.MoveToAttribute("theme"))
		{
			int themeIndex2 = Convert.ToInt32(reader.Value);
			color.SetTheme(themeIndex2, m_book, dTintValue);
		}
	}

	public static Color ConvertColorByTint(Color color, double dTint)
	{
		ConvertRGBtoHLS(color, out var dHue, out var dLuminance, out var dSaturation);
		if (dTint < 0.0)
		{
			dLuminance *= 1.0 + dTint;
		}
		if (dTint > 0.0)
		{
			dLuminance = dLuminance * (1.0 - dTint) + (255.0 - 255.0 * (1.0 - dTint));
		}
		ConvertHLSToRGB(dHue, dLuminance, dSaturation);
		return ConvertHLSToRGB(dHue, dLuminance, dSaturation);
	}

	internal static Color ConvertColorByTintBlip(Color color, double dTint)
	{
		int[] array = new int[3] { color.R, color.G, color.B };
		for (int i = 0; i < 3; i++)
		{
			double num = CalcDouble(array[i]);
			double doubleValue = num * (1.0 + (1.0 - dTint));
			if (dTint > 0.0)
			{
				doubleValue = num * (1.0 - (1.0 - dTint)) + (1.0 - dTint);
			}
			array[i] = CalcInt(doubleValue);
		}
		return Color.FromArgb(color.A, (byte)array[0], (byte)array[1], (byte)array[2]);
	}

	internal static double CalcDouble(int integer)
	{
		double num = (double)integer / 255.0;
		if (num < 0.0)
		{
			return 0.0;
		}
		if (num <= 0.04045)
		{
			return num / 12.92;
		}
		if (!(num <= 1.0))
		{
			return 1.0;
		}
		return Math.Pow((num + 0.055) / 1.055, 2.4);
	}

	internal static int CalcInt(double doubleValue)
	{
		double num = ((doubleValue < 0.0) ? 0.0 : ((doubleValue <= 0.0031308) ? (doubleValue * 12.92) : ((!(doubleValue < 1.0)) ? 1.0 : (1.055 * Math.Pow(doubleValue, 5.0 / 12.0) - 0.055))));
		return (int)Math.Round(num * 255.0, 0);
	}

	public static void ConvertRGBtoHLS(Color color, out double dHue, out double dLuminance, out double dSaturation)
	{
		dHue = 0.0;
		dLuminance = 0.0;
		dSaturation = 0.0;
		byte r = color.R;
		byte g = color.G;
		byte b = color.B;
		byte b2 = Math.Min(r, Math.Min(g, b));
		byte b3 = Math.Max(r, Math.Max(g, b));
		double num = b3 - b2;
		double num2 = b3 + b2;
		dLuminance = (num2 * 255.0 + 255.0) / 510.0;
		if (b3 == b2)
		{
			dSaturation = 0.0;
			dHue = 170.0;
		}
		else
		{
			if (dLuminance <= 127.0)
			{
				dSaturation = (num * 255.0 + num2 / 2.0) / num2;
			}
			else
			{
				dSaturation = (num * 255.0 + (510.0 - num2) / 2.0) / (510.0 - num2);
			}
			double num3 = ((double)((b3 - r) * 42) + num / 2.0) / num;
			double num4 = ((double)((b3 - g) * 42) + num / 2.0) / num;
			double num5 = ((double)((b3 - b) * 42) + num / 2.0) / num;
			if (r == b3)
			{
				dHue = num5 - num4;
			}
			else if (g == b3)
			{
				dHue = 85.0 + num3 - num5;
			}
			else
			{
				dHue = 170.0 + num4 - num3;
			}
			if (dHue < 0.0)
			{
				dHue += 255.0;
			}
			if (dHue > 255.0)
			{
				dHue -= 255.0;
			}
		}
		if (dSaturation < 0.0)
		{
			dSaturation = 0.0;
		}
		if (dSaturation > 255.0)
		{
			dSaturation = 255.0;
		}
		if (dLuminance < 0.0)
		{
			dLuminance = 0.0;
		}
		if (dLuminance > 255.0)
		{
			dLuminance = 255.0;
		}
	}

	public static Color ConvertHLSToRGB(double dHue, double dLuminance, double dSaturation)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		dHue /= 255.0;
		dLuminance /= 255.0;
		dSaturation /= 255.0;
		if (dSaturation == 0.0)
		{
			num = (byte)(dLuminance * 255.0);
			num2 = num;
			num3 = num;
		}
		else
		{
			double num4 = ((!(dLuminance < 0.5)) ? (dLuminance + dSaturation - dLuminance * dSaturation) : (dLuminance * (1.0 + dSaturation)));
			double dN = 2.0 * dLuminance - num4;
			double num5 = 0.0;
			double num6 = HueToRGB(dN, num4, dHue + 0.33);
			double num7 = HueToRGB(dN, num4, dHue);
			num5 = HueToRGB(dN, num4, dHue - 0.33);
			num = (int)Math.Round(255.0 * num6);
			num2 = (int)Math.Round(255.0 * num7);
			num3 = (int)Math.Round(255.0 * num5);
		}
		if (num < 0)
		{
			num = 0;
		}
		if (num2 < 0)
		{
			num2 = 0;
		}
		if (num3 < 0)
		{
			num3 = 0;
		}
		if (num2 > 255)
		{
			num = 255;
		}
		if (num2 > 255)
		{
			num2 = 255;
		}
		if (num3 > 255)
		{
			num3 = 255;
		}
		return Color.FromArgb(0, (byte)num, (byte)num2, (byte)num3);
	}

	public static double HueToRGB(double dN1, double dN2, double dHue)
	{
		if (dHue < 0.0)
		{
			dHue += 1.0;
		}
		if (dHue > 1.0)
		{
			dHue -= 1.0;
		}
		if (dHue < 1.0 / 6.0)
		{
			return dN1 + (dN2 - dN1) * 6.0 * dHue;
		}
		if (dHue < 0.5)
		{
			return dN2;
		}
		if (dHue < 2.0 / 3.0)
		{
			return dN1 + (dN2 - dN1) * (2.0 / 3.0 - dHue) * 6.0;
		}
		return dN1;
	}

	private bool ParseBoolean(XmlReader reader, string valueAttribute, bool defaultValue)
	{
		bool result = defaultValue;
		if (reader.MoveToAttribute(valueAttribute))
		{
			result = XmlConvertExtension.ToBoolean(reader.Value);
		}
		return result;
	}

	private string ParseValue(XmlReader reader, string valueAttribute)
	{
		string result = null;
		if (reader.MoveToAttribute(valueAttribute))
		{
			result = reader.Value;
		}
		return result;
	}

	private List<FillImpl> ParseFills(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		List<FillImpl> list = new List<FillImpl>();
		if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "fills")
		{
			reader.Read();
			if (reader.NodeType == XmlNodeType.Whitespace)
			{
				reader.Read();
			}
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					FillImpl item = ParseFill(reader, swapColors: true);
					list.Add(item);
				}
				reader.Read();
			}
		}
		return list;
	}

	private FillImpl ParseFill(XmlReader reader, bool swapColors)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "fill")
		{
			throw new XmlException("Wrong tag name " + reader.LocalName);
		}
		reader.Read();
		if (reader.NodeType == XmlNodeType.Whitespace)
		{
			reader.Read();
		}
		FillImpl fillImpl = null;
		string localName = reader.LocalName;
		if (!(localName == "patternFill"))
		{
			if (!(localName == "gradientFill"))
			{
				throw new ArgumentException("Unexpected tag  " + reader.LocalName);
			}
			fillImpl = ParseGradientFill(reader);
		}
		else
		{
			fillImpl = ParsePatternFill(reader, swapColors);
		}
		if (reader.NodeType == XmlNodeType.Whitespace)
		{
			reader.Read();
		}
		return fillImpl;
	}

	private FillImpl ParseGradientFill(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "gradientFill")
		{
			throw new XmlException("Unexpected tag " + reader.LocalName);
		}
		FillImpl fillImpl = null;
		fillImpl = ((!reader.MoveToAttribute("type") || !(reader.Value == "path")) ? ParseLinearGradientType(reader) : ParsePathGradientType(reader));
		reader.Skip();
		return fillImpl;
	}

	private FillImpl ParsePathGradientType(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		FillImpl fillImpl = new FillImpl();
		fillImpl.FillType = OfficeFillType.Gradient;
		double num = ParseAttributeValue(reader, "top");
		double num2 = ParseAttributeValue(reader, "bottom");
		double num3 = ParseAttributeValue(reader, "left");
		double num4 = ParseAttributeValue(reader, "right");
		if (num == 0.5 && num2 == 0.5 && num3 == 0.5 && num4 == 0.5)
		{
			fillImpl.GradientStyle = OfficeGradientStyle.FromCenter;
			fillImpl.GradientVariant = OfficeGradientVariants.ShadingVariants_1;
		}
		else if (num == 1.0 && num2 == 1.0 && num3 == 1.0 && num4 == 1.0)
		{
			fillImpl.GradientStyle = OfficeGradientStyle.FromCorner;
			fillImpl.GradientVariant = OfficeGradientVariants.ShadingVariants_4;
		}
		else if (num == 1.0 && num2 == 1.0)
		{
			fillImpl.GradientStyle = OfficeGradientStyle.FromCorner;
			fillImpl.GradientVariant = OfficeGradientVariants.ShadingVariants_3;
		}
		else if (num3 == 1.0 && num4 == 1.0)
		{
			fillImpl.GradientStyle = OfficeGradientStyle.FromCorner;
			fillImpl.GradientVariant = OfficeGradientVariants.ShadingVariants_2;
		}
		else if (double.IsNaN(num) && double.IsNaN(num2) && double.IsNaN(num3) && double.IsNaN(num4))
		{
			fillImpl.GradientStyle = OfficeGradientStyle.FromCorner;
			fillImpl.GradientVariant = OfficeGradientVariants.ShadingVariants_1;
		}
		reader.Read();
		List<ChartColor> list = ParseStopColors(reader);
		fillImpl.PatternColorObject.CopyFrom(list[0], callEvent: true);
		fillImpl.ColorObject.CopyFrom(list[1], callEvent: true);
		return fillImpl;
	}

	private List<ChartColor> ParseStopColors(XmlReader reader)
	{
		List<ChartColor> list = new List<ChartColor>();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.LocalName == "stop")
			{
				reader.Read();
				list.Add(ParseColor(reader));
				reader.Skip();
			}
			reader.Skip();
		}
		return list;
	}

	private FillImpl ParseLinearGradientType(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		FillImpl fillImpl = new FillImpl();
		fillImpl.FillType = OfficeFillType.Gradient;
		double num = ParseAttributeValue(reader, "degree");
		reader.Read();
		List<ChartColor> list = ParseStopColors(reader);
		fillImpl.PatternColorObject.CopyFrom(list[0], callEvent: true);
		fillImpl.ColorObject.CopyFrom(list[1], callEvent: true);
		if (list.Count == 3)
		{
			fillImpl.GradientVariant = OfficeGradientVariants.ShadingVariants_3;
			switch ((!double.IsNaN(num)) ? ((int)num) : 0)
			{
			case 90:
				fillImpl.GradientStyle = OfficeGradientStyle.Horizontal;
				break;
			case 0:
				fillImpl.GradientStyle = OfficeGradientStyle.Vertical;
				break;
			case 45:
				fillImpl.GradientStyle = OfficeGradientStyle.DiagonalUp;
				break;
			case 135:
				fillImpl.GradientStyle = OfficeGradientStyle.DiagonalDown;
				break;
			default:
				throw new ArgumentException("Unsupported degree value");
			}
		}
		else
		{
			SetGradientStyleVariant(fillImpl, num);
		}
		return fillImpl;
	}

	private void SetGradientStyleVariant(FillImpl fill, double dDegree)
	{
		switch ((!double.IsNaN(dDegree)) ? ((int)dDegree) : 0)
		{
		case 90:
			fill.GradientStyle = OfficeGradientStyle.Horizontal;
			fill.GradientVariant = OfficeGradientVariants.ShadingVariants_1;
			break;
		case 270:
			fill.GradientStyle = OfficeGradientStyle.Horizontal;
			fill.GradientVariant = OfficeGradientVariants.ShadingVariants_2;
			break;
		case 0:
			fill.GradientStyle = OfficeGradientStyle.Vertical;
			fill.GradientVariant = OfficeGradientVariants.ShadingVariants_1;
			break;
		case 180:
			fill.GradientStyle = OfficeGradientStyle.Vertical;
			fill.GradientVariant = OfficeGradientVariants.ShadingVariants_2;
			break;
		case 45:
			fill.GradientStyle = OfficeGradientStyle.DiagonalUp;
			fill.GradientVariant = OfficeGradientVariants.ShadingVariants_1;
			break;
		case 225:
			fill.GradientStyle = OfficeGradientStyle.DiagonalUp;
			fill.GradientVariant = OfficeGradientVariants.ShadingVariants_2;
			break;
		case 135:
			fill.GradientStyle = OfficeGradientStyle.DiagonalDown;
			fill.GradientVariant = OfficeGradientVariants.ShadingVariants_1;
			break;
		case 315:
			fill.GradientStyle = OfficeGradientStyle.DiagonalDown;
			fill.GradientVariant = OfficeGradientVariants.ShadingVariants_2;
			break;
		default:
			throw new ArgumentException("Unsupported degree value");
		}
	}

	private double ParseAttributeValue(XmlReader reader, string strAttributeName)
	{
		if (!reader.MoveToAttribute(strAttributeName))
		{
			return double.NaN;
		}
		return XmlConvertExtension.ToDouble(reader.Value);
	}

	private FillImpl ParsePatternFill(XmlReader reader, bool swapColors)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "patternFill")
		{
			throw new XmlException("Unexpected tag " + reader.LocalName);
		}
		FillImpl fillImpl = new FillImpl();
		if (reader.MoveToAttribute("patternType"))
		{
			fillImpl.Pattern = ConvertStringToPattern(reader.Value);
		}
		if (reader.NodeType == XmlNodeType.Whitespace)
		{
			reader.Read();
		}
		reader.Read();
		ChartColor chartColor = null;
		ChartColor chartColor2 = null;
		while (reader.NodeType != XmlNodeType.EndElement || reader.LocalName != "fill")
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName = reader.LocalName;
				if (!(localName == "bgColor"))
				{
					if (localName == "fgColor")
					{
						chartColor2 = new ChartColor((OfficeKnownColors)65);
						ParseColor(reader, chartColor2);
					}
				}
				else
				{
					chartColor = new ChartColor(OfficeKnownColors.BlackCustom);
					ParseFillColor(reader, chartColor);
				}
			}
			reader.Read();
		}
		if (reader.LocalName == "patternFill")
		{
			reader.Read();
		}
		if (swapColors && fillImpl.Pattern != OfficePattern.Solid)
		{
			ChartColor chartColor3 = chartColor2;
			chartColor2 = chartColor;
			chartColor = chartColor3;
		}
		if (chartColor2 != null)
		{
			fillImpl.ColorObject.CopyFrom(chartColor2, callEvent: true);
		}
		else if (chartColor == null || fillImpl.Pattern != OfficePattern.Solid)
		{
			fillImpl.ColorObject.SetIndexed((OfficeKnownColors)65);
		}
		if (chartColor != null)
		{
			fillImpl.PatternColorObject.CopyFrom(chartColor, callEvent: true);
		}
		else if (chartColor2 == null || fillImpl.Pattern != OfficePattern.Solid)
		{
			fillImpl.PatternColorObject.SetIndexed(OfficeKnownColors.BlackCustom);
		}
		return fillImpl;
	}

	private static OfficePattern ConvertStringToPattern(string value)
	{
		return (OfficePattern)(Excel2007Pattern)Enum.Parse(typeof(Excel2007Pattern), value, ignoreCase: true);
	}

	private List<BordersCollection> ParseBorders(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		List<BordersCollection> list = new List<BordersCollection>();
		if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "borders")
		{
			throw new XmlException("Unexpected xml tag " + reader.LocalName);
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				if (!(reader.LocalName == "border"))
				{
					throw new XmlException("Unexpected xml tag " + reader.LocalName);
				}
				BordersCollection item = ParseBordersCollection(reader);
				list.Add(item);
			}
			else
			{
				reader.Read();
			}
		}
		return list;
	}

	private BordersCollection ParseBordersCollection(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "border")
		{
			throw new XmlException("Unexpected xml tag " + reader.LocalName);
		}
		BordersCollection bordersCollection = new BordersCollection(m_book.Application, m_book, bAddEmpty: true);
		bool showDiagonalLine = false;
		bool showDiagonalLine2 = false;
		if (reader.MoveToAttribute("diagonalUp"))
		{
			showDiagonalLine = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("diagonalDown"))
		{
			showDiagonalLine2 = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.IsEmptyElement)
		{
			reader.Read();
		}
		else
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					Excel2007BorderIndex borderIndex;
					BorderSettingsHolder borderSettingsHolder = ParseBorder(reader, out borderIndex);
					if (borderIndex == Excel2007BorderIndex.diagonal)
					{
						BorderSettingsHolder borderSettingsHolder2 = (BorderSettingsHolder)borderSettingsHolder.Clone();
						borderSettingsHolder.ShowDiagonalLine = showDiagonalLine;
						borderSettingsHolder2.ShowDiagonalLine = showDiagonalLine2;
						bordersCollection.SetBorder(OfficeBordersIndex.DiagonalUp, borderSettingsHolder);
						bordersCollection.SetBorder(OfficeBordersIndex.DiagonalDown, borderSettingsHolder2);
					}
					else
					{
						OfficeBordersIndex index = (OfficeBordersIndex)borderIndex;
						bordersCollection.SetBorder(index, borderSettingsHolder);
					}
					if (bordersCollection.IsEmptyBorder && borderSettingsHolder != null && !borderSettingsHolder.IsEmptyBorder)
					{
						bordersCollection.IsEmptyBorder = false;
					}
				}
				reader.Read();
			}
			reader.Read();
		}
		return bordersCollection;
	}

	private BorderSettingsHolder ParseBorder(XmlReader reader, out Excel2007BorderIndex borderIndex)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.NodeType != XmlNodeType.Element)
		{
			throw new XmlException("Unexpected node type " + reader.NodeType);
		}
		borderIndex = (Excel2007BorderIndex)Enum.Parse(typeof(Excel2007BorderIndex), reader.LocalName, ignoreCase: true);
		BorderSettingsHolder borderSettingsHolder = new BorderSettingsHolder();
		bool flag = false;
		if (reader.IsEmptyElement)
		{
			flag = true;
		}
		if (reader.MoveToAttribute("style"))
		{
			Excel2007BorderLineStyle lineStyle = (Excel2007BorderLineStyle)Enum.Parse(typeof(Excel2007BorderLineStyle), reader.Value, ignoreCase: true);
			borderSettingsHolder.LineStyle = (OfficeLineStyle)lineStyle;
			borderSettingsHolder.IsEmptyBorder = false;
		}
		if (!flag && !reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement || reader.LocalName == "color")
			{
				if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "color")
				{
					ParseColor(reader, borderSettingsHolder.ColorObject);
				}
				reader.Read();
			}
		}
		return borderSettingsHolder;
	}

	private int ParseRow(XmlReader reader, IInternalWorksheet sheet, List<int> arrStyles, string cellTag, int generatedRowIndex)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (reader.LocalName != "row")
		{
			throw new XmlException("reader");
		}
		int num = 0;
		int num2 = m_book.DefaultXFIndex;
		int num3 = sheet.DefaultRowHeight;
		bool flag = false;
		bool flag2 = false;
		if (reader.MoveToAttribute("r"))
		{
			generatedRowIndex = (num = XmlConvertExtension.ToInt32(reader.Value));
			if (num == 0)
			{
				generatedRowIndex = (num = 1);
			}
		}
		else
		{
			num = generatedRowIndex;
		}
		RowStorage orCreateRow = WorksheetHelper.GetOrCreateRow(sheet, num - 1, bCreate: true);
		List<int> list = new List<int>();
		if (reader.MoveToAttribute("collapsed"))
		{
			orCreateRow.IsCollapsed = XmlConvertExtension.ToBoolean(reader.Value);
		}
		flag = reader.MoveToAttribute("customFormat") && XmlConvertExtension.ToBoolean(reader.Value);
		if (reader.MoveToAttribute("customHeight"))
		{
			orCreateRow.IsBadFontHeight = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("outlineLevel"))
		{
			orCreateRow.OutlineLevel = XmlConvertExtension.ToUInt16(reader.Value);
			m_indexAndLevels.Add(generatedRowIndex, orCreateRow.OutlineLevel);
		}
		if (reader.MoveToAttribute("ht"))
		{
			double num4 = XmlConvertExtension.ToDouble(reader.Value);
			if (num4 > 409.5)
			{
				num4 = 409.5;
			}
			num3 = (int)(num4 * 20.0);
			orCreateRow.HasRowHeight = true;
		}
		if (reader.MoveToAttribute("s") && arrStyles.Count > XmlConvertExtension.ToInt32(reader.Value))
		{
			num2 = arrStyles[XmlConvertExtension.ToInt32(reader.Value)];
		}
		if (reader.MoveToAttribute("hidden"))
		{
			orCreateRow.IsHidden = XmlConvertExtension.ToBoolean(reader.Value);
			orCreateRow.IsCollapsed = XmlConvertExtension.ToBoolean(reader.Value);
		}
		else
		{
			orCreateRow.IsCollapsed = false;
		}
		if (reader.MoveToAttribute("thickBot"))
		{
			orCreateRow.IsSpaceBelowRow = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("thickTop"))
		{
			orCreateRow.IsSpaceAboveRow = XmlConvertExtension.ToBoolean(reader.Value);
		}
		orCreateRow.ExtendedFormatIndex = (ushort)num2;
		orCreateRow.Height = (ushort)num3;
		if (sheet.FirstRow < 0 || sheet.FirstRow > num)
		{
			sheet.FirstRow = num;
		}
		if (sheet.LastRow < num)
		{
			sheet.LastRow = num;
		}
		reader.MoveToElement();
		int num5 = 1;
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.LocalName == cellTag)
				{
					num5 = ParseCell(reader, sheet, arrStyles, num, num5, list);
					num5++;
				}
				reader.Skip();
			}
		}
		else if (orCreateRow.IsHidden)
		{
			sheet.CellRecords.SetBlank(num, num5, m_book.DefaultXFIndex);
		}
		foreach (int item in list)
		{
			if (!flag2)
			{
				flag2 = m_book.GetExtFormat(item).WrapText;
				continue;
			}
			break;
		}
		orCreateRow.IsFormatted = flag;
		orCreateRow.IsWrapText = flag2;
		return generatedRowIndex;
	}

	private int ParseCell(XmlReader reader, IInternalWorksheet sheet, List<int> arrStyles, int rowIndex, int columnIndex, List<int> cellStyleIndex)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		bool flag = false;
		int iRow = rowIndex;
		int iColumn = columnIndex;
		int num = m_book.DefaultXFIndex;
		Excel2007Serializator.CellType cellType = Excel2007Serializator.CellType.n;
		CellRecordCollection cellRecords = sheet.CellRecords;
		if (reader.MoveToAttribute("r"))
		{
			RangeImpl.CellNameToRowColumn(reader.Value, out iRow, out iColumn);
		}
		if (reader.MoveToAttribute("s"))
		{
			num = arrStyles[XmlConvertExtension.ToInt32(reader.Value)];
			if (!cellStyleIndex.Contains(num))
			{
				cellStyleIndex.Add(num);
			}
		}
		if (reader.MoveToAttribute("t"))
		{
			cellType = GetCellType(reader.Value);
		}
		reader.MoveToElement();
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.LocalName == "is")
				{
					string text = string.Empty;
					int num2 = ParseStringItem(reader, out text);
					SSTDictionary innerSST = m_book.InnerSST;
					cellRecords.SetSingleStringValue(iRow, iColumn, num, num2);
					if (text != null)
					{
						string cellName = RangeImpl.GetCellName(iColumn, iRow);
						Worksheet.InlineStrings.Add(cellName, text);
						if (!m_book.HasInlineStrings)
						{
							m_book.HasInlineStrings = true;
						}
					}
					if (num2 == 1 || num2 == 2)
					{
						innerSST.RemoveDecrease(num2);
					}
					flag = true;
				}
				if (reader.LocalName == "f")
				{
					ParseFormula(reader, sheet, iRow, iColumn, num, cellType);
					flag = true;
				}
				if (reader.LocalName == "is")
				{
					reader.Skip();
				}
				if (reader.NodeType == XmlNodeType.EndElement)
				{
					continue;
				}
				if (reader.LocalName == "v")
				{
					bool flag2 = reader.IsEmptyElement;
					if (!flag2)
					{
						reader.Read();
					}
					if (reader.NodeType == XmlNodeType.EndElement)
					{
						flag2 = true;
					}
					else
					{
						if (WorksheetHelper.HasFormulaRecord(sheet, iRow, iColumn))
						{
							SetFormulaValue(sheet, cellType, reader.Value, iRow, iColumn);
						}
						else
						{
							SetCellRecord(cellType, reader.Value, cellRecords, iRow, iColumn, num);
						}
						flag = true;
					}
					if (!flag2)
					{
						reader.Skip();
					}
				}
				reader.Skip();
			}
		}
		if (!flag)
		{
			if (cellRecords.Sheet is ExternWorksheetImpl)
			{
				SetCellRecord(Excel2007Serializator.CellType.n, "0", cellRecords, iRow, iColumn, num);
			}
			else
			{
				cellRecords.SetBlank(iRow, iColumn, num);
			}
		}
		return iColumn;
	}

	private static Excel2007Serializator.CellType GetCellType(string cellType)
	{
		return cellType switch
		{
			"b" => Excel2007Serializator.CellType.b, 
			"e" => Excel2007Serializator.CellType.e, 
			"inlineStr" => Excel2007Serializator.CellType.inlineStr, 
			"n" => Excel2007Serializator.CellType.n, 
			"s" => Excel2007Serializator.CellType.s, 
			"str" => Excel2007Serializator.CellType.str, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private void ParseFormula(XmlReader reader, IInternalWorksheet sheet, int iRow, int iCol, int iXFIndex, Excel2007Serializator.CellType cellType)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (reader.LocalName != "f")
		{
			throw new XmlException("reader");
		}
		string strCellsRange = null;
		string text = "=";
		uint uiSharedGroupIndex = 0u;
		Excel2007Serializator.FormulaType formulaType = Excel2007Serializator.FormulaType.normal;
		CellRecordCollection cellRecords = sheet.CellRecords;
		bool flag = false;
		if (reader.MoveToAttribute("t"))
		{
			formulaType = (Excel2007Serializator.FormulaType)Enum.Parse(typeof(Excel2007Serializator.FormulaType), reader.Value, ignoreCase: false);
		}
		if (reader.MoveToAttribute("si"))
		{
			uiSharedGroupIndex = XmlConvertExtension.ToUInt32(reader.Value);
		}
		if (reader.MoveToAttribute("ref"))
		{
			strCellsRange = reader.Value;
		}
		if (reader.MoveToAttribute("ca"))
		{
			flag = XmlConvertExtension.ToBoolean(reader.Value);
		}
		reader.MoveToElement();
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			if (reader.NodeType != XmlNodeType.EndElement)
			{
				text += reader.Value;
				reader.Skip();
			}
		}
		switch (formulaType)
		{
		case Excel2007Serializator.FormulaType.normal:
			text = UtilityMethods.RemoveFirstCharUnsafe(text);
			if (text.Length > 0)
			{
				FormulaRecord formulaRecord = (FormulaRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Formula);
				if (!text.StartsWith("[") || cellType != Excel2007Serializator.CellType.e)
				{
					formulaRecord.ParsedExpression = m_formulaUtil.ParseString(text, sheet, null);
				}
				else
				{
					(sheet as WorksheetImpl).m_formulaString = text;
				}
				formulaRecord.Row = iRow - 1;
				formulaRecord.Column = iCol - 1;
				formulaRecord.ExtendedFormatIndex = (ushort)iXFIndex;
				formulaRecord.CalculateOnOpen = flag;
				cellRecords.SetCellRecord(iRow, iCol, formulaRecord);
			}
			break;
		case Excel2007Serializator.FormulaType.array:
			SetArrayFormula(sheet as WorksheetImpl, text, strCellsRange, iXFIndex);
			break;
		case Excel2007Serializator.FormulaType.shared:
			SetSharedFormula(sheet as WorksheetImpl, text, strCellsRange, uiSharedGroupIndex, iRow, iCol, iXFIndex, flag);
			break;
		}
		reader.Skip();
	}

	private void ParsePalette(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "indexedColors")
		{
			throw new XmlException("Cannot locate tag indexedColors");
		}
		reader.Read();
		int num = 0;
		while (reader.NodeType != XmlNodeType.EndElement || reader.LocalName != "indexedColors")
		{
			if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "rgbColor")
			{
				reader.MoveToAttribute("rgb");
				Color color = ColorExtension.FromArgb(int.Parse(reader.Value, NumberStyles.HexNumber));
				m_book.SetPaletteColor(num, color);
				num++;
			}
			reader.Read();
		}
	}

	private void ParseColors(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "colors")
		{
			throw new XmlException("Cannot locate tag colors");
		}
		if (reader.IsEmptyElement)
		{
			return;
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "indexedColors")
			{
				ParsePalette(reader);
			}
			reader.Read();
		}
	}

	private void ParseColumns(XmlReader reader, WorksheetImpl sheet, List<int> arrStyles)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (arrStyles == null)
		{
			throw new ArgumentNullException("arrStyles");
		}
		if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "cols")
		{
			throw new XmlException("Unable to locate xml tag cols");
		}
		if (m_outlineLevels == null)
		{
			m_outlineLevels = new Dictionary<int, List<Point>>();
		}
		if (m_indexAndLevels == null)
		{
			m_indexAndLevels = new Dictionary<int, int>();
		}
		if (sheet != null && sheet.ColumnOutlineLevels == null)
		{
			sheet.ColumnOutlineLevels = new Dictionary<int, List<Point>>();
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement || reader.LocalName != "cols")
		{
			if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "col")
			{
				ParseColumn(reader, sheet, arrStyles);
			}
			reader.Read();
		}
		if (sheet != null && m_indexAndLevels != null && m_indexAndLevels.Count > 0)
		{
			m_indexAndLevels.Clear();
			m_outlineLevels = null;
		}
	}

	private void ParseColumn(XmlReader reader, WorksheetImpl sheet, List<int> arrStyles)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (arrStyles == null)
		{
			throw new ArgumentNullException("arrStyles");
		}
		if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "col")
		{
			throw new XmlException("Unable to locate xml tag col");
		}
		ColumnInfoRecord columnInfoRecord = (ColumnInfoRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ColumnInfo);
		if (reader.MoveToAttribute("min"))
		{
			columnInfoRecord.FirstColumn = (ushort)(XmlConvertExtension.ToUInt16(reader.Value) - 1);
		}
		if (reader.MoveToAttribute("max"))
		{
			columnInfoRecord.LastColumn = (ushort)(XmlConvertExtension.ToUInt16(reader.Value) - 1);
		}
		if (reader.MoveToAttribute("width"))
		{
			int num = (int)Math.Round(XmlConvertExtension.ToDouble(reader.Value) * 256.0);
			columnInfoRecord.ColumnWidth = (ushort)num;
		}
		if (reader.MoveToAttribute("style"))
		{
			int index = int.Parse(reader.Value);
			index = arrStyles[index];
			columnInfoRecord.ExtendedFormatIndex = (ushort)index;
		}
		else
		{
			columnInfoRecord.ExtendedFormatIndex = (ushort)m_book.DefaultXFIndex;
		}
		if (reader.MoveToAttribute("bestFit"))
		{
			columnInfoRecord.IsBestFit = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("phonetic"))
		{
			columnInfoRecord.IsPhenotic = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("customWidth"))
		{
			columnInfoRecord.IsUserSet = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("collapsed"))
		{
			columnInfoRecord.IsCollapsed = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("outlineLevel"))
		{
			columnInfoRecord.OutlineLevel = XmlConvertExtension.ToUInt16(reader.Value);
			for (int i = columnInfoRecord.FirstColumn; i <= columnInfoRecord.LastColumn; i++)
			{
				m_indexAndLevels.Add(i + 1, columnInfoRecord.OutlineLevel);
			}
		}
		if (reader.MoveToAttribute("hidden"))
		{
			columnInfoRecord.IsHidden = XmlConvertExtension.ToBoolean(reader.Value);
			columnInfoRecord.IsCollapsed = XmlConvertExtension.ToBoolean(reader.Value);
		}
		else
		{
			columnInfoRecord.IsCollapsed = false;
		}
		sheet.ParseColumnInfo(columnInfoRecord, bIgnoreStyles: false);
	}

	private int ParseColumnInfoRecord(XmlReader xmlTextReader, WorksheetImpl worksheet, int index)
	{
		if (!xmlTextReader.HasAttributes)
		{
			xmlTextReader.Skip();
			return index;
		}
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		short num4 = -1;
		double width = -1.0;
		bool flag = false;
		bool isHidden = false;
		bool collapsedInfo = false;
		bool bestFitInfo = false;
		xmlTextReader.MoveToElement();
		while (xmlTextReader.MoveToNextAttribute())
		{
			string localName = xmlTextReader.LocalName;
			if (localName == null)
			{
				continue;
			}
			if (DocGen.Drawing.Helper.columnAttributes == null)
			{
				DocGen.Drawing.Helper.columnAttributes = new Dictionary<string, int>(9)
				{
					{ "min", 0 },
					{ "max", 1 },
					{ "width", 2 },
					{ "style", 3 },
					{ "hidden", 4 },
					{ "customWidth", 5 },
					{ "outlineLevel", 6 },
					{ "collapsed", 7 },
					{ "bestFit", 8 }
				};
			}
			if (DocGen.Drawing.Helper.columnAttributes.TryGetValue(localName, out var value))
			{
				switch (value)
				{
				case 0:
					num = DocGen.Drawing.Helper.ParseInt(xmlTextReader.Value) - 1;
					num2 = num;
					break;
				case 1:
					num2 = DocGen.Drawing.Helper.ParseInt(xmlTextReader.Value) - 1;
					break;
				case 2:
					width = DocGen.Drawing.Helper.ParseDouble(xmlTextReader.Value);
					flag = true;
					break;
				case 3:
					num3 = DocGen.Drawing.Helper.ParseInt(xmlTextReader.Value);
					break;
				case 4:
					isHidden = DocGen.Drawing.Helper.ParseBoolen(xmlTextReader.Value);
					break;
				case 6:
					num4 = DocGen.Drawing.Helper.ParseShort(xmlTextReader.Value);
					break;
				case 7:
					collapsedInfo = DocGen.Drawing.Helper.ParseBoolen(xmlTextReader.Value);
					break;
				case 8:
					bestFitInfo = DocGen.Drawing.Helper.ParseBoolen(xmlTextReader.Value);
					break;
				}
			}
		}
		xmlTextReader.MoveToElement();
		ColumnCollection columnss = worksheet.Columnss;
		double width2 = columnss.Width;
		double width3 = (flag ? worksheet.CharacterWidth(width) : width2);
		int styleIndex = 15;
		if (num3 != -1)
		{
			object obj = null;
			if (obj != null)
			{
				styleIndex = (int)obj;
			}
		}
		for (int i = num; i <= num2; i++)
		{
			Column column = null;
			if (num2 >= 16383)
			{
				column = columnss.GetOrCreateColumn();
				column.SetMinColumnIndex(num);
			}
			else if (num >= index)
			{
				column = columnss.AddColumn(i);
			}
			if (flag)
			{
				column.Width = width3;
			}
			column.SetStyleIndex(styleIndex);
			if (num4 != -1)
			{
				column.SetOutLineLevel((byte)num4);
			}
			column.IsHidden = isHidden;
			column.SetCollapsedInfo(collapsedInfo);
			column.SetBestFitInfo(bestFitInfo);
			if (num2 >= 16383)
			{
				break;
			}
		}
		if (index <= num2)
		{
			return num2;
		}
		return index;
	}

	private void SwitchStreams(ref bool bAdd, ref XmlWriter writer, ref StreamWriter textWriter, Stream streamEnd)
	{
		if (!bAdd)
		{
			bAdd = true;
			writer.WriteEndElement();
			writer.Flush();
			textWriter = new StreamWriter(streamEnd);
			writer = UtilityMethods.CreateWriter(textWriter);
			writer.WriteStartElement("root", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
		}
	}

	public List<Color> ParseThemes(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.LocalName != "theme")
		{
			throw new XmlException("reader");
		}
		if (reader.IsEmptyElement)
		{
			return null;
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.LocalName == "themeElements")
			{
				m_lstThemeColors = ParseThemeElements(reader, isThemeOverride: false);
			}
			reader.Skip();
		}
		m_book.MajorFonts = m_dicMajorFonts;
		m_book.MinorFonts = m_dicMinorFonts;
		return m_lstThemeColors;
	}

	public List<Color> ParseThemeElements(XmlReader reader, bool isThemeOverride)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.IsEmptyElement)
		{
			return null;
		}
		List<Color> result = new List<Color>();
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName = reader.LocalName;
				if (!(localName == "clrScheme"))
				{
					if (localName == "fontScheme")
					{
						ParseFontScheme(reader);
					}
					continue;
				}
				Dictionary<string, Color> dicThemeColors = null;
				result = ParseThemeColors(reader, out dicThemeColors);
				if (isThemeOverride)
				{
					m_themeColorOverrideDictionary = dicThemeColors;
				}
				else
				{
					m_dicThemeColors = dicThemeColors;
				}
			}
			else
			{
				reader.Skip();
			}
		}
		return result;
	}

	private void ParseFontScheme(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName = reader.LocalName;
				if (!(localName == "majorFont"))
				{
					if (localName == "minorFont")
					{
						ParseMinorFont(reader, out m_dicMinorFonts);
					}
					else
					{
						reader.Skip();
					}
				}
				else
				{
					ParseMajorFont(reader, out m_dicMajorFonts);
				}
			}
			else
			{
				reader.Skip();
			}
		}
	}

	private void ParseMinorFont(XmlReader reader, out Dictionary<string, FontImpl> dicMinorFonts)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		reader.Read();
		FontImpl fontImpl = null;
		dicMinorFonts = new Dictionary<string, FontImpl>();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "latin":
					fontImpl = GetFont(reader);
					dicMinorFonts.Add("latin", fontImpl);
					break;
				case "ea":
					fontImpl = GetFont(reader);
					dicMinorFonts.Add("ea", fontImpl);
					break;
				case "cs":
					fontImpl = GetFont(reader);
					dicMinorFonts.Add("cs", fontImpl);
					break;
				default:
					reader.Skip();
					break;
				}
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Read();
	}

	private void ParseMajorFont(XmlReader reader, out Dictionary<string, FontImpl> dicMajorFonts)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		reader.Read();
		FontImpl fontImpl = null;
		dicMajorFonts = new Dictionary<string, FontImpl>();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "latin":
					fontImpl = GetFont(reader);
					dicMajorFonts.Add("lt", fontImpl);
					break;
				case "ea":
					fontImpl = GetFont(reader);
					dicMajorFonts.Add("ea", fontImpl);
					break;
				case "cs":
					fontImpl = GetFont(reader);
					dicMajorFonts.Add("cs", fontImpl);
					break;
				default:
					reader.Skip();
					break;
				}
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Read();
	}

	public FontImpl GetFont(XmlReader reader)
	{
		FontImpl fontImpl = null;
		if (reader.MoveToAttribute("typeface"))
		{
			fontImpl = (FontImpl)m_book.CreateFont(null, bAddToCollection: false);
			fontImpl.FontName = reader.Value;
		}
		return fontImpl;
	}

	public static void SkipWhiteSpaces(XmlReader reader)
	{
		while (reader.NodeType == XmlNodeType.Whitespace)
		{
			reader.Read();
		}
	}

	public List<Color> ParseThemeColors(XmlReader reader, out Dictionary<string, Color> dicThemeColors)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		dicThemeColors = null;
		if (reader.IsEmptyElement)
		{
			return null;
		}
		reader.Read();
		List<Color> list = new List<Color>();
		dicThemeColors = new Dictionary<string, Color>();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName = reader.LocalName;
				reader.Read();
				SkipWhiteSpaces(reader);
				Color color = ColorExtension.Black;
				if (reader.LocalName == "srgbClr")
				{
					if (reader.MoveToAttribute("val"))
					{
						color = ColorExtension.FromArgb(int.Parse(reader.Value, NumberStyles.HexNumber));
					}
					reader.Skip();
					SkipWhiteSpaces(reader);
				}
				else if (reader.LocalName == "sysClr")
				{
					if (reader.MoveToAttribute("val"))
					{
						color = ColorExtension.FromName(reader.Value);
					}
					reader.Skip();
					SkipWhiteSpaces(reader);
				}
				list.Add(color);
				dicThemeColors.Add(localName, color);
				reader.Skip();
			}
			else
			{
				reader.Skip();
			}
		}
		list.Reverse(0, 2);
		list.Reverse(2, 2);
		dicThemeColors.Add("tx1", list[1]);
		dicThemeColors.Add("tx2", list[3]);
		reader.Read();
		return list;
	}

	public List<DxfImpl> ParseDxfCollection(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "dxfs")
		{
			throw new XmlException("reader");
		}
		List<DxfImpl> list = new List<DxfImpl>();
		reader.Read();
		if (reader.NodeType != 0)
		{
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.LocalName == "dxf")
				{
					list.Add(ParseDxfStyle(reader));
				}
				else
				{
					reader.Skip();
				}
			}
		}
		return list;
	}

	private DxfImpl ParseDxfStyle(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		DxfImpl dxfImpl = new DxfImpl();
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			switch (reader.LocalName)
			{
			case "numFmt":
				dxfImpl.FormatRecord = ParseDxfNumberFormat(reader);
				reader.Skip();
				break;
			case "fill":
				dxfImpl.Fill = ParseFill(reader, swapColors: false);
				reader.Skip();
				break;
			case "font":
				dxfImpl.Font = ParseFont(reader);
				reader.Skip();
				break;
			case "border":
				dxfImpl.Borders = ParseBordersCollection(reader);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Read();
		return dxfImpl;
	}

	private void UpdateCFRange(string address, WorksheetImpl worksheet)
	{
		if (m_book.IsWorkbookOpening || m_book.Saving || !worksheet.UsedRangeIncludesCF)
		{
			return;
		}
		string[] array = address.Split(' ');
		foreach (string name in array)
		{
			IRange range = worksheet.Range[name];
			if (minRow > range.Row || minRow == 0)
			{
				minRow = range.Row;
			}
			if (minColumn > range.Column || minColumn == 0)
			{
				minColumn = range.Column;
			}
			if (maxColumn < range.LastColumn || maxColumn == 0)
			{
				maxColumn = range.LastColumn;
			}
			if (maxRow < range.LastRow || maxRow == 0)
			{
				maxRow = range.LastRow;
			}
		}
	}

	public static void ParsePrintOptions(XmlReader reader, IPageSetupBase pageSetup)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (pageSetup == null)
		{
			throw new ArgumentNullException("pageSetup");
		}
		if (reader.LocalName != "printOptions")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		if (pageSetup is PageSetupImpl pageSetupImpl)
		{
			pageSetupImpl.PrintGridlines = reader.MoveToAttribute("gridLines") && XmlConvertExtension.ToBoolean(reader.Value);
			pageSetupImpl.PrintGridlines |= reader.MoveToAttribute("gridLinesSet") && XmlConvertExtension.ToBoolean(reader.Value);
			pageSetupImpl.PrintHeadings = reader.MoveToAttribute("headings") && XmlConvertExtension.ToBoolean(reader.Value);
		}
		pageSetup.CenterHorizontally = reader.MoveToAttribute("horizontalCentered") && XmlConvertExtension.ToBoolean(reader.Value);
		pageSetup.CenterVertically = reader.MoveToAttribute("verticalCentered") && XmlConvertExtension.ToBoolean(reader.Value);
		reader.Read();
	}

	public static void ParsePageMargins(XmlReader reader, IPageSetupBase pageSetup, IPageSetupConstantsProvider constants)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (pageSetup == null)
		{
			throw new ArgumentNullException("pageSetup");
		}
		if (reader.LocalName != constants.PageMarginsTag)
		{
			throw new XmlException("Unexpected xml tag.");
		}
		if (reader.MoveToAttribute(constants.LeftMargin))
		{
			pageSetup.LeftMargin = XmlConvertExtension.ToDouble(reader.Value);
		}
		if (reader.MoveToAttribute(constants.RightMargin))
		{
			pageSetup.RightMargin = XmlConvertExtension.ToDouble(reader.Value);
		}
		if (reader.MoveToAttribute(constants.TopMargin))
		{
			pageSetup.TopMargin = XmlConvertExtension.ToDouble(reader.Value);
		}
		if (reader.MoveToAttribute(constants.BottomMargin))
		{
			pageSetup.BottomMargin = XmlConvertExtension.ToDouble(reader.Value);
		}
		if (reader.MoveToAttribute(constants.HeaderMargin))
		{
			pageSetup.HeaderMargin = XmlConvertExtension.ToDouble(reader.Value);
		}
		if (reader.MoveToAttribute(constants.FooterMargin))
		{
			pageSetup.FooterMargin = XmlConvertExtension.ToDouble(reader.Value);
		}
		reader.MoveToElement();
		reader.Skip();
	}

	public static void ParsePageSetup(XmlReader reader, PageSetupBaseImpl pageSetup)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (pageSetup == null)
		{
			throw new ArgumentNullException("pageSetup");
		}
		if (reader.LocalName != "pageSetup")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		if (reader.MoveToAttribute("paperSize"))
		{
			pageSetup.PaperSize = (OfficePaperSize)XmlConvertExtension.ToInt32(reader.Value);
		}
		else
		{
			pageSetup.PaperSize = OfficePaperSize.PaperLetter;
		}
		if (reader.MoveToAttribute("scale"))
		{
			int num = XmlConvertExtension.ToInt32(reader.Value);
			num = ((num > 400) ? 400 : num);
			num = ((num < 10) ? 10 : num);
			pageSetup.Zoom = num;
		}
		else
		{
			pageSetup.Zoom = 100;
		}
		if (reader.MoveToAttribute("firstPageNumber"))
		{
			uint num2 = XmlConvertExtension.ToUInt32(reader.Value);
			pageSetup.FirstPageNumber = (short)num2;
		}
		if (reader.MoveToAttribute("fitToWidth"))
		{
			pageSetup.FitToPagesWide = XmlConvertExtension.ToInt32(reader.Value);
		}
		if (reader.MoveToAttribute("fitToHeight"))
		{
			pageSetup.FitToPagesTall = XmlConvertExtension.ToInt32(reader.Value);
		}
		if (reader.MoveToAttribute("pageOrder"))
		{
			pageSetup.Order = (OfficeOrder)Enum.Parse(typeof(OfficeOrder), reader.Value, ignoreCase: true);
		}
		if (reader.MoveToAttribute("orientation") && (reader.Value.ToUpper() == "LANDSCAPE" || reader.Value.ToUpper() == "PORTRAIT"))
		{
			pageSetup.Orientation = (OfficePageOrientation)Enum.Parse(typeof(OfficePageOrientation), reader.Value, ignoreCase: true);
		}
		if (reader.MoveToAttribute("blackAndWhite"))
		{
			pageSetup.BlackAndWhite = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("draft"))
		{
			pageSetup.Draft = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("cellComments"))
		{
			pageSetup.PrintComments = StringToPrintComments(reader.Value);
		}
		if (reader.MoveToAttribute("useFirstPageNumber"))
		{
			pageSetup.AutoFirstPageNumber = !XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("errors"))
		{
			pageSetup.PrintErrors = StringToPrintErrors(reader.Value);
		}
		if (reader.MoveToAttribute("horizontalDpi"))
		{
			pageSetup.HResolution = GetParsedXmlValue(reader.Value);
		}
		if (reader.MoveToAttribute("verticalDpi"))
		{
			pageSetup.VResolution = GetParsedXmlValue(reader.Value);
		}
		if (reader.MoveToAttribute("copies"))
		{
			pageSetup.Copies = XmlConvertExtension.ToInt32(reader.Value);
		}
		if (pageSetup is PageSetupImpl pageSetupImpl && reader.MoveToAttribute("id"))
		{
			pageSetupImpl.RelationId = reader.Value;
		}
		reader.MoveToElement();
		reader.Skip();
	}

	public static void ParseHeaderFooter(XmlReader reader, PageSetupBaseImpl pageSetup)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (pageSetup == null)
		{
			throw new ArgumentNullException("pageSetup");
		}
		if (reader.LocalName != "headerFooter")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		pageSetup.AlignHFWithPageMargins = true;
		if (reader.MoveToAttribute("scaleWithDoc"))
		{
			pageSetup.HFScaleWithDoc = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("alignWithMargins"))
		{
			pageSetup.AlignHFWithPageMargins = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("differentFirst"))
		{
			pageSetup.DifferentFirstPageHF = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("differentOddEven"))
		{
			pageSetup.DifferentOddAndEvenPagesHF = XmlConvertExtension.ToBoolean(reader.Value);
		}
		reader.MoveToElement();
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					string localName = reader.LocalName;
					if (!(localName == "oddHeader"))
					{
						if (localName == "oddFooter")
						{
							string fullFooterString = reader.ReadElementContentAsString();
							pageSetup.FullFooterString = fullFooterString;
						}
						else
						{
							reader.Skip();
						}
					}
					else
					{
						string fullHeaderString = reader.ReadElementContentAsString();
						pageSetup.FullHeaderString = fullHeaderString;
					}
				}
				else
				{
					reader.Skip();
				}
			}
		}
		reader.Read();
	}

	private static OfficePrintLocation StringToPrintComments(string printLocation)
	{
		return printLocation switch
		{
			"asDisplayed" => OfficePrintLocation.PrintInPlace, 
			"none" => OfficePrintLocation.PrintNoComments, 
			"atEnd" => OfficePrintLocation.PrintSheetEnd, 
			_ => throw new ArgumentOutOfRangeException("printLocation"), 
		};
	}

	private static OfficePrintErrors StringToPrintErrors(string printErrors)
	{
		return printErrors switch
		{
			"blank" => OfficePrintErrors.PrintErrorsBlank, 
			"dash" => OfficePrintErrors.PrintErrorsDash, 
			"displayed" => OfficePrintErrors.PrintErrorsDisplayed, 
			"NA" => OfficePrintErrors.PrintErrorsNA, 
			_ => throw new ArgumentOutOfRangeException("printLocation"), 
		};
	}

	private void ParseHyperlinks(XmlReader reader, WorksheetImpl sheet)
	{
	}

	private void ParseSheetLevelProperties(XmlReader reader, WorksheetBaseImpl sheet)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (reader.MoveToAttribute("codeName"))
		{
			sheet.CodeName = reader.Value;
		}
		if (reader.MoveToAttribute("transitionEvaluation"))
		{
			sheet.IsTransitionEvaluation = XmlConvertExtension.ToBoolean(reader.Value);
		}
		reader.MoveToElement();
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					switch (reader.LocalName)
					{
					case "tabColor":
						ParseColor(reader, sheet.TabColorObject);
						break;
					case "outlinePr":
						ParseOutlineProperites(reader, sheet.PageSetupBase as IPageSetup);
						break;
					case "pageSetUpPr":
						ParsePageSetupProperties(reader, sheet.PageSetupBase as IPageSetup);
						break;
					default:
						reader.Skip();
						break;
					}
				}
				else
				{
					reader.Skip();
				}
			}
		}
		reader.Read();
	}

	private void ParsePageSetupProperties(XmlReader reader, IPageSetup pageSetup)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (pageSetup == null)
		{
			throw new ArgumentNullException("pageSetup");
		}
		if (reader.LocalName != "pageSetUpPr")
		{
			throw new XmlException();
		}
		if (reader.MoveToAttribute("fitToPage"))
		{
			pageSetup.IsFitToPage = XmlConvertExtension.ToBoolean(reader.Value);
		}
		reader.Read();
	}

	private void ParseOutlineProperites(XmlReader reader, IPageSetup pageSetup)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (pageSetup == null)
		{
			throw new ArgumentNullException("pageSetup");
		}
		if (reader.MoveToAttribute("summaryBelow"))
		{
			pageSetup.IsSummaryRowBelow = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("summaryRight"))
		{
			pageSetup.IsSummaryColumnRight = XmlConvertExtension.ToBoolean(reader.Value);
		}
		reader.MoveToElement();
		reader.Skip();
	}

	private void ParseBackgroundImage(XmlReader reader, WorksheetImpl sheet, string strParentPath)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (strParentPath == null)
		{
			throw new ArgumentNullException("strParentPath");
		}
		reader.Skip();
	}

	public string ParseItemProperties(XmlReader reader, ref List<string> schemas)
	{
		string result = null;
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		new List<string>();
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		while (reader.NodeType != XmlNodeType.EndElement && reader.NodeType != 0)
		{
			if (reader.NodeType != XmlNodeType.Element)
			{
				continue;
			}
			string localName = reader.LocalName;
			if (!(localName == "datastoreItem"))
			{
				if (localName == "schemaRefs")
				{
					ParseschemaReference(reader, ref schemas);
					reader.Read();
				}
			}
			else
			{
				if (reader.MoveToAttribute("ds:itemID"))
				{
					result = reader.Value;
				}
				reader.Read();
			}
		}
		return result;
	}

	private void ParseschemaReference(XmlReader reader, ref List<string> schemas)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "schemaRefs")
		{
			throw new XmlException("Wrong xml tag");
		}
		schemas = new List<string>();
		if (reader.IsEmptyElement)
		{
			return;
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.LocalName == "schemaRef")
			{
				ParseSchemaRef(reader, ref schemas);
			}
			else
			{
				reader.Skip();
			}
		}
	}

	private void ParseSchemaRef(XmlReader reader, ref List<string> schemas)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "schemaRef")
		{
			throw new XmlException("Wrong xml tag");
		}
		if (reader.MoveToAttribute("ds:uri"))
		{
			string value = reader.Value;
			schemas.Add(value);
		}
		reader.Read();
	}

	internal bool ParseExternalLink(XmlReader reader, RelationCollection relations)
	{
		bool result = true;
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		if (reader.LocalName != "externalLink")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		switch (reader.LocalName)
		{
		case "externalBook":
			ParseExternalWorkbook(reader, relations);
			break;
		case "oleLink":
			ParseOleObjectLink(reader, relations);
			break;
		case "ddeLink":
			reader.Skip();
			result = false;
			break;
		default:
			throw new XmlException("Unsupported xml tag");
		}
		return result;
	}

	private void ParseOleObjectLink(XmlReader reader, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "oleLink")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		string text = null;
		if (reader.MoveToAttribute("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships"))
		{
			text = reader.Value;
		}
		ExternWorkbookImpl externWorkbookImpl = CreateExternBook(relations, text, null);
		int index = externWorkbookImpl.ExternNames.Add("'");
		if (reader.MoveToAttribute("progId"))
		{
			externWorkbookImpl.ProgramId = reader.Value;
		}
		ExternNameRecord record = externWorkbookImpl.ExternNames[index].Record;
		record.OleLink = true;
		record.Ole = false;
		record.WantPicture = true;
		record.WantAdvise = true;
		record.BuiltIn = false;
		string text2 = Uri.UnescapeDataString(relations[text].Target);
		if (text2.StartsWith("file:///"))
		{
			text2 = text2.Substring("file:///".Length);
		}
		externWorkbookImpl.URL = text2;
		reader.Skip();
	}

	private void ParseExternalWorkbook(XmlReader reader, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "externalBook")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		string strUrlId = null;
		if (reader.MoveToAttribute("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships"))
		{
			strUrlId = reader.Value;
		}
		List<string> list = null;
		ExternWorkbookImpl externBook = null;
		if (!reader.IsEmptyElement)
		{
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					switch (reader.LocalName)
					{
					case "sheetNames":
						list = ParseSheetNames(reader);
						externBook = CreateExternBook(relations, strUrlId, list);
						break;
					case "sheetDataSet":
						ParseSheetDataSet(reader, externBook);
						break;
					case "definedNames":
						ParseExternalDefinedNames(reader, externBook);
						break;
					case "alternateUrls":
						reader.Skip();
						break;
					default:
						throw new XmlException("Unexpected xml tag.");
					}
				}
				else
				{
					reader.Read();
				}
			}
		}
		reader.Read();
	}

	private void ParseExternalDefinedNames(XmlReader reader, ExternWorkbookImpl externBook)
	{
		if (reader.LocalName != "definedNames")
		{
			throw new XmlException();
		}
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					if (reader.LocalName == "definedName")
					{
						ParseExternalName(reader, externBook);
					}
					else
					{
						reader.Skip();
					}
				}
				else
				{
					reader.Skip();
				}
			}
		}
		reader.Read();
	}

	private void ParseExternalName(XmlReader reader, ExternWorkbookImpl externBook)
	{
		if (reader.LocalName != "definedName")
		{
			throw new XmlException();
		}
		string name = null;
		string refersTo = null;
		if (reader.MoveToAttribute("name"))
		{
			name = reader.Value;
		}
		if (reader.MoveToAttribute("refersTo"))
		{
			refersTo = reader.Value;
		}
		int index = externBook.ExternNames.Add(name);
		externBook.ExternNames[index].RefersTo = refersTo;
		if (reader.MoveToAttribute("sheetId"))
		{
			externBook.ExternNames[index].sheetId = Convert.ToInt32(reader.Value);
		}
	}

	private void ParseSheetDataSet(XmlReader reader, ExternWorkbookImpl externBook)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (externBook == null)
		{
			throw new ArgumentNullException("externBook");
		}
		if (reader.LocalName != "sheetDataSet")
		{
			throw new XmlException();
		}
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					if (reader.LocalName == "sheetData")
					{
						ParseExternalSheetData(reader, externBook);
					}
					else
					{
						reader.Skip();
					}
				}
				else
				{
					reader.Skip();
				}
			}
		}
		reader.Skip();
	}

	private void ParseExternalSheetData(XmlReader reader, ExternWorkbookImpl externBook)
	{
		if (reader == null)
		{
			throw new ArgumentException("reader");
		}
		if (externBook == null)
		{
			throw new ArgumentNullException("externBook");
		}
		if (reader.LocalName != "sheetData")
		{
			throw new XmlException();
		}
		if (!reader.MoveToAttribute("sheetId"))
		{
			throw new XmlException();
		}
		int key = XmlConvertExtension.ToInt32(reader.Value);
		ExternWorksheetImpl externWorksheetImpl = externBook.Worksheets[key];
		reader.MoveToElement();
		externWorksheetImpl.AdditionalAttributes = ParseSheetData(reader, externWorksheetImpl, null, "cell");
	}

	private ExternWorkbookImpl CreateExternBook(RelationCollection relations, string strUrlId, List<string> arrSheetNames)
	{
		string text = relations[strUrlId].Target;
		if (text.StartsWith("file:///"))
		{
			text = text.Substring("file:///".Length);
		}
		text = Uri.UnescapeDataString(text);
		string text2 = text.Split('/')[^1];
		string filePath = text.Substring(0, text.Length - text2.Length);
		int index = m_book.ExternWorkbooks.Add(filePath, text2, arrSheetNames, null);
		return m_book.ExternWorkbooks[index];
	}

	private List<string> ParseSheetNames(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "sheetNames")
		{
			throw new XmlException("Unexpected xml tag");
		}
		List<string> list = new List<string>();
		if (!reader.IsEmptyElement)
		{
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "sheetName")
				{
					if (!reader.MoveToAttribute("val"))
					{
						throw new XmlException();
					}
					string value = reader.Value;
					list.Add(value);
				}
				reader.Read();
			}
		}
		reader.Read();
		return list;
	}

	private void ParseExternalLinksWorkbookPart(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "externalReferences")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		bool isEmptyElement = reader.IsEmptyElement;
		reader.Read();
		if (isEmptyElement)
		{
			return;
		}
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "externalReference")
			{
				ParseExternalLinkWorkbookPart(reader);
			}
			else
			{
				reader.Read();
			}
		}
		reader.Read();
	}

	private void ParseExternalLinkWorkbookPart(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "externalReference")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		if (!reader.MoveToAttribute("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships"))
		{
			throw new XmlException();
		}
		string value = reader.Value;
		m_book.DataHolder.ParseExternalLink(value);
	}

	public void Dispose()
	{
		m_dictShapeParsers.Clear();
		m_dictShapeParsers = null;
		m_values.Clear();
		m_values = null;
		m_drawingParser = null;
		m_formulaUtil.Dispose();
		m_formulaUtil = null;
		if (m_lstThemeColors != null)
		{
			m_lstThemeColors.Clear();
			m_lstThemeColors = null;
		}
		if (m_dicThemeColors != null)
		{
			m_dicThemeColors.Clear();
			m_dicThemeColors = null;
		}
		if (m_themeColorOverrideDictionary != null)
		{
			m_themeColorOverrideDictionary.Clear();
			m_themeColorOverrideDictionary = null;
		}
		if (m_dicMajorFonts != null)
		{
			m_dicMajorFonts.Clear();
			m_dicMajorFonts = null;
		}
		if (m_dicMinorFonts != null)
		{
			m_dicMinorFonts.Clear();
			m_dicMinorFonts = null;
		}
	}

	private void SetCellRecord(Excel2007Serializator.CellType type, string strValue, CellRecordCollection cells, int iRow, int iColumn, int iXFIndex)
	{
		if (strValue != null && strValue.Length != 0)
		{
			if (cells == null)
			{
				throw new ArgumentNullException("cells");
			}
			switch (type)
			{
			case Excel2007Serializator.CellType.n:
				cells.SetNumberValue(iRow, iColumn, XmlConvertExtension.ToDouble(strValue), iXFIndex);
				break;
			case Excel2007Serializator.CellType.b:
				cells.SetBooleanValue(iRow, iColumn, XmlConvertExtension.ToBoolean(strValue), iXFIndex);
				break;
			case Excel2007Serializator.CellType.e:
				cells.SetErrorValue(iRow, iColumn, strValue, iXFIndex);
				break;
			case Excel2007Serializator.CellType.inlineStr:
			case Excel2007Serializator.CellType.s:
				cells.SetSingleStringValue(iRow, iColumn, iXFIndex, XmlConvertExtension.ToInt32(strValue));
				break;
			case Excel2007Serializator.CellType.str:
				cells.SetNonSSTString(iRow, iColumn, iXFIndex, strValue);
				break;
			}
		}
	}

	private void SetFormulaValue(IInternalWorksheet sheet, Excel2007Serializator.CellType cellType, string strValue, int iRowIndex, int iColumnIndex)
	{
		if (strValue == null)
		{
			throw new NullReferenceException("strValue");
		}
		switch (cellType)
		{
		case Excel2007Serializator.CellType.b:
			sheet.SetFormulaBoolValue(iRowIndex, iColumnIndex, XmlConvertExtension.ToBoolean(strValue));
			break;
		case Excel2007Serializator.CellType.e:
			if (strValue != string.Empty)
			{
				sheet.SetFormulaErrorValue(iRowIndex, iColumnIndex, strValue);
			}
			break;
		case Excel2007Serializator.CellType.n:
			sheet.SetFormulaNumberValue(iRowIndex, iColumnIndex, XmlConvertExtension.ToDouble(strValue));
			break;
		case Excel2007Serializator.CellType.str:
			sheet.SetFormulaStringValue(iRowIndex, iColumnIndex, strValue);
			break;
		case Excel2007Serializator.CellType.inlineStr:
		case Excel2007Serializator.CellType.s:
			break;
		}
	}

	private void SetArrayFormula(WorksheetImpl sheet, string strFormulaString, string strCellsRange, int iXFIndex)
	{
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (strFormulaString == null)
		{
			throw new ArgumentNullException("strFormulaString");
		}
		if (strCellsRange == null)
		{
			throw new ArgumentNullException("strCellRange");
		}
		ArrayRecord arrayRecord = (ArrayRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Array);
		strFormulaString = UtilityMethods.RemoveFirstCharUnsafe(strFormulaString);
		arrayRecord.Formula = m_formulaUtil.ParseString(strFormulaString, sheet, null);
		string strRow = null;
		string strColumn = null;
		string strRow2 = null;
		string strColumn2 = null;
		if (m_formulaUtil.IsCellRange(strCellsRange, bR1C1: false, out strRow, out strColumn, out strRow2, out strColumn2))
		{
			arrayRecord.FirstRow = Convert.ToInt32(strRow) - 1;
			arrayRecord.FirstColumn = RangeImpl.GetColumnIndex(strColumn) - 1;
			arrayRecord.LastRow = Convert.ToInt32(strRow2) - 1;
			arrayRecord.LastColumn = RangeImpl.GetColumnIndex(strColumn2) - 1;
		}
		else
		{
			int iRow = 0;
			int iColumn = 0;
			RangeImpl.CellNameToRowColumn(strCellsRange, out iRow, out iColumn);
			arrayRecord.FirstRow = iRow - 1;
			arrayRecord.FirstColumn = iColumn - 1;
			arrayRecord.LastRow = iRow - 1;
			arrayRecord.LastColumn = iColumn - 1;
		}
		((RangeImpl)sheet.Range[strCellsRange]).SetFormulaArrayRecord(arrayRecord, iXFIndex);
	}

	private void SetSharedFormula(WorksheetImpl sheet, string strFormulaString, string strCellsRange, uint uiSharedGroupIndex, int iRow, int iCol, int iXFIndex, bool bCalculateOnOpen)
	{
		CellRecordCollection cellRecords = sheet.CellRecords;
		if (strCellsRange != null && strFormulaString != null)
		{
			string strRow = null;
			string strColumn = null;
			string strRow2 = null;
			string strColumn2 = null;
			SharedFormulaRecord sharedFormulaRecord = (SharedFormulaRecord)BiffRecordFactory.GetRecord(TBIFFRecord.SharedFormula2);
			bool flag = false;
			if (!(flag = m_formulaUtil.IsCellRange(strCellsRange, bR1C1: false, out strRow, out strColumn, out strRow2, out strColumn2)) && (flag = FormulaUtil.IsCell(strCellsRange, bR1C1: false, out strRow, out strColumn)))
			{
				strRow2 = strRow;
				strColumn2 = strColumn;
			}
			if (flag)
			{
				int firstRow = Convert.ToInt32(strRow);
				int columnIndex = RangeImpl.GetColumnIndex(strColumn);
				sharedFormulaRecord.FirstRow = firstRow;
				sharedFormulaRecord.FirstColumn = columnIndex;
				sharedFormulaRecord.LastRow = Convert.ToInt32(strRow2);
				sharedFormulaRecord.LastColumn = RangeImpl.GetColumnIndex(strColumn2);
				strFormulaString = UtilityMethods.RemoveFirstCharUnsafe(strFormulaString);
				sharedFormulaRecord.Formula = m_formulaUtil.ParseSharedString(strFormulaString, iRow, iCol, sheet);
				RecordTable table = cellRecords.Table;
				_ = table.SharedFormulas.Count;
				table.AddSharedFormula(0, (int)uiSharedGroupIndex, sharedFormulaRecord);
			}
		}
		FormulaRecord formulaRecord = (FormulaRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Formula);
		SharedFormulaRecord shared = cellRecords.Table.SharedFormulas[(int)uiSharedGroupIndex];
		formulaRecord.ParsedExpression = FormulaUtil.ConvertSharedFormulaTokens(shared, sheet.ParentWorkbook, iRow - 1, iCol - 1);
		formulaRecord.Row = iRow - 1;
		formulaRecord.Column = iCol - 1;
		formulaRecord.ExtendedFormatIndex = (ushort)iXFIndex;
		formulaRecord.CalculateOnOpen = bCalculateOnOpen;
		cellRecords.SetCellRecord(iRow, iCol, formulaRecord);
	}

	private ExcelDataType ConvertDataValidationType(string dataValidationType)
	{
		if (dataValidationType == null || dataValidationType == string.Empty)
		{
			throw new ArgumentNullException("strErrorStyle");
		}
		return dataValidationType switch
		{
			"custom" => ExcelDataType.Formula, 
			"date" => ExcelDataType.Date, 
			"decimal" => ExcelDataType.Decimal, 
			"list" => ExcelDataType.User, 
			"none" => ExcelDataType.Any, 
			"textLength" => ExcelDataType.TextLength, 
			"time" => ExcelDataType.Time, 
			"whole" => ExcelDataType.Integer, 
			_ => throw new ArgumentOutOfRangeException("dataValidationType"), 
		};
	}

	private ExcelErrorStyle ConvertDataValidationErrorStyle(string strErrorStyle)
	{
		if (strErrorStyle == null || strErrorStyle == string.Empty)
		{
			throw new ArgumentNullException("strErrorStyle");
		}
		return strErrorStyle switch
		{
			"information" => ExcelErrorStyle.Info, 
			"stop" => ExcelErrorStyle.Stop, 
			"warning" => ExcelErrorStyle.Warning, 
			_ => throw new ArgumentOutOfRangeException("strErrorStyle"), 
		};
	}

	private ExcelDataValidationComparisonOperator ConvertDataValidationOperator(string strOperator)
	{
		if (strOperator == null || strOperator == string.Empty)
		{
			throw new ArgumentNullException("strOperator");
		}
		return strOperator switch
		{
			"between" => ExcelDataValidationComparisonOperator.Between, 
			"equal" => ExcelDataValidationComparisonOperator.Equal, 
			"greaterThan" => ExcelDataValidationComparisonOperator.Greater, 
			"greaterThanOrEqual" => ExcelDataValidationComparisonOperator.GreaterOrEqual, 
			"lessThan" => ExcelDataValidationComparisonOperator.Less, 
			"lessThanOrEqual" => ExcelDataValidationComparisonOperator.LessOrEqual, 
			"notBetween" => ExcelDataValidationComparisonOperator.NotBetween, 
			"notEqual" => ExcelDataValidationComparisonOperator.NotEqual, 
			_ => throw new ArgumentOutOfRangeException("strOperator"), 
		};
	}

	private TAddr[] GetRangesForDataValidation(string strRange)
	{
		if (strRange == null || strRange == string.Empty)
		{
			throw new ArgumentNullException("strRange");
		}
		string[] array = strRange.Split(' ');
		List<TAddr> list = new List<TAddr>();
		string[] array2 = array;
		foreach (string strRange2 in array2)
		{
			list.Add(GetRangeForDVOrAF(strRange2));
		}
		return list.ToArray();
	}

	private TAddr GetRangeForDVOrAF(string strRange)
	{
		if (strRange == null || strRange == string.Empty)
		{
			throw new ArgumentNullException("strRange");
		}
		string strRow = string.Empty;
		string strColumn = string.Empty;
		string strRow2 = string.Empty;
		string strColumn2 = string.Empty;
		TAddr result = default(TAddr);
		if (FormulaUtil.IsCell(strRange, bR1C1: false, out strRow, out strColumn))
		{
			int num = Convert.ToInt32(strRow) - 1;
			int num2 = RangeImpl.GetColumnIndex(strColumn) - 1;
			result = new TAddr(num, num2, num, num2);
		}
		else if (m_formulaUtil.IsCellRange(strRange, bR1C1: false, out strRow, out strColumn, out strRow2, out strColumn2))
		{
			int iFirstRow = Convert.ToInt32(strRow) - 1;
			int iFirstCol = RangeImpl.GetColumnIndex(strColumn) - 1;
			int iLastRow = Convert.ToInt32(strRow2) - 1;
			int iLastCol = RangeImpl.GetColumnIndex(strColumn2) - 1;
			result = new TAddr(iFirstRow, iFirstCol, iLastRow, iLastCol);
		}
		return result;
	}

	private OfficeFilterCondition ConvertAutoFormatFilterCondition(string strCondition)
	{
		return strCondition switch
		{
			"equal" => OfficeFilterCondition.Equal, 
			"greaterThan" => OfficeFilterCondition.Greater, 
			"greaterThanOrEqual" => OfficeFilterCondition.GreaterOrEqual, 
			"lessThan" => OfficeFilterCondition.Less, 
			"lessThanOrEqual" => OfficeFilterCondition.LessOrEqual, 
			"notEqual" => OfficeFilterCondition.NotEqual, 
			_ => throw new ArgumentOutOfRangeException("strCondition"), 
		};
	}

	private ExcelCFType ConvertCFType(string strType, out bool bIsSupportedType)
	{
		bIsSupportedType = true;
		switch (strType)
		{
		case "cellIs":
			return ExcelCFType.CellValue;
		case "beginsWith":
		case "endsWith":
		case "containsText":
		case "notContainsText":
			return ExcelCFType.SpecificText;
		case "expression":
			return ExcelCFType.Formula;
		case "dataBar":
			return ExcelCFType.DataBar;
		case "iconSet":
			return ExcelCFType.IconSet;
		case "colorScale":
			return ExcelCFType.ColorScale;
		case "containsBlanks":
			return ExcelCFType.Blank;
		case "notContainsBlanks":
			return ExcelCFType.NoBlank;
		case "containsErrors":
			return ExcelCFType.ContainsErrors;
		case "notContainsErrors":
			return ExcelCFType.NotContainsErrors;
		case "timePeriod":
			return ExcelCFType.TimePeriod;
		default:
			bIsSupportedType = false;
			return ExcelCFType.CellValue;
		}
	}

	private ExcelComparisonOperator ConvertCFOperator(string strOperator, out bool bIsSupportedOperator)
	{
		bIsSupportedOperator = true;
		return strOperator switch
		{
			"between" => ExcelComparisonOperator.Between, 
			"equal" => ExcelComparisonOperator.Equal, 
			"greaterThan" => ExcelComparisonOperator.Greater, 
			"greaterThanOrEqual" => ExcelComparisonOperator.GreaterOrEqual, 
			"lessThan" => ExcelComparisonOperator.Less, 
			"lessThanOrEqual" => ExcelComparisonOperator.LessOrEqual, 
			"notContains" => ExcelComparisonOperator.NotContainsText, 
			"notBetween" => ExcelComparisonOperator.NotBetween, 
			"notEqual" => ExcelComparisonOperator.NotEqual, 
			"beginsWith" => ExcelComparisonOperator.BeginsWith, 
			"containsText" => ExcelComparisonOperator.ContainsText, 
			"endsWith" => ExcelComparisonOperator.EndsWith, 
			_ => throw new ArgumentOutOfRangeException("strOperator"), 
		};
	}

	private CFTimePeriods ConvertCFTimePeriods(string timePeriod, out bool bIsSupportedTimePeriod)
	{
		bIsSupportedTimePeriod = true;
		return timePeriod switch
		{
			"yesterday" => CFTimePeriods.Yesterday, 
			"today" => CFTimePeriods.Today, 
			"tomorrow" => CFTimePeriods.Tomorrow, 
			"last7Days" => CFTimePeriods.Last7Days, 
			"lastWeek" => CFTimePeriods.LastWeek, 
			"thisWeek" => CFTimePeriods.ThisWeek, 
			"nextWeek" => CFTimePeriods.NextWeek, 
			"lastMonth" => CFTimePeriods.LastMonth, 
			"thisMonth" => CFTimePeriods.ThisMonth, 
			"nextMonth" => CFTimePeriods.NextMonth, 
			_ => throw new ArgumentOutOfRangeException("timePeriod"), 
		};
	}

	private string GetReaderElementValue(XmlReader reader)
	{
		if (reader.IsEmptyElement)
		{
			reader.Read();
			return string.Empty;
		}
		reader.Read();
		string result;
		if (reader.NodeType != XmlNodeType.EndElement)
		{
			result = reader.Value;
			reader.Skip();
		}
		else
		{
			result = string.Empty;
		}
		reader.Skip();
		return result;
	}

	internal Color ConvertColorByShade(Color result, double shade)
	{
		byte alpha = (byte)((double)(int)result.A * shade);
		byte red = (byte)((double)(int)result.R * shade);
		byte green = (byte)((double)(int)result.G * shade);
		byte blue = (byte)((double)(int)result.B * shade);
		return Color.FromArgb(alpha, red, green, blue);
	}

	internal Color ConvertColorByShadeBlip(Color result, double shade)
	{
		int[] array = new int[3] { result.R, result.G, result.B };
		for (int i = 0; i < 3; i++)
		{
			double num = CalcDouble(array[i]) * shade;
			if (num < 0.0)
			{
				num = 0.0;
			}
			else if (num > 1.0)
			{
				num = 1.0;
			}
			array[i] = CalcInt(num);
		}
		return Color.FromArgb(result.A, (byte)array[0], (byte)array[1], (byte)array[2]);
	}

	private string ConvertToASCII(string value)
	{
		value = value.Replace("_x000a_", "\n");
		value = value.Replace("_x000d_", "\r");
		value = value.Replace("_x0009_", "\t");
		value = value.Replace("_x0008_", "\b");
		value = value.Replace("_x0000_", "\0");
		return value;
	}

	public static void CopyFillSettings(FillImpl fill, ExtendedFormatImpl extendedFormat)
	{
		if (fill.FillType == OfficeFillType.Gradient)
		{
			extendedFormat.Gradient = new ShapeFillImpl(extendedFormat.Application, extendedFormat, OfficeFillType.Gradient);
			IGradient gradient = extendedFormat.Gradient;
			gradient.GradientStyle = fill.GradientStyle;
			gradient.GradientVariant = fill.GradientVariant;
			gradient.BackColorObject.CopyFrom(fill.PatternColorObject, callEvent: true);
			gradient.ForeColorObject.CopyFrom(fill.ColorObject, callEvent: true);
			extendedFormat.Record.AdtlFillPattern = 4000;
		}
		else
		{
			extendedFormat.IncludePatterns = true;
			extendedFormat.ColorObject.CopyFrom(fill.ColorObject, callEvent: true);
			extendedFormat.PatternColorObject.CopyFrom(fill.PatternColorObject, callEvent: true);
			extendedFormat.FillPattern = fill.Pattern;
		}
	}

	internal List<Color> ParseThemeOverideColors(ChartImpl chart)
	{
		XmlReader reader = UtilityMethods.CreateReader(chart.m_themeOverrideStream);
		Dictionary<string, Color> dicThemeColors = null;
		return ParseThemeColors(reader, out dicThemeColors);
	}
}
