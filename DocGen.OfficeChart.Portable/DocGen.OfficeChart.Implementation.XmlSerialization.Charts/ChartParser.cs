using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using DocGen.Compression;
using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Implementation.XmlReaders;
using DocGen.OfficeChart.Implementation.XmlReaders.Shapes;
using DocGen.OfficeChart.Implementation.XmlSerialization.Constants;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Interfaces.Charts;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Implementation.XmlSerialization.Charts;

internal class ChartParser
{
	internal const float DefaultTitleSize = 18f;

	private WorkbookImpl m_book;

	private double _appVersion = -1.0;

	internal ExcelEngine engine;

	internal IWorksheet Worksheet
	{
		get
		{
			if (engine == null)
			{
				engine = new ExcelEngine();
				engine.Excel.Workbooks.Create(1);
			}
			return engine.Excel.Workbooks[0].Worksheets[0];
		}
	}

	public ChartParser(WorkbookImpl book)
	{
		m_book = book;
	}

	internal string ApplyNumberFormat(object value, string numberFormat)
	{
		RangeImpl obj = Worksheet["A1"] as RangeImpl;
		obj.Value2 = value;
		obj.NumberFormat = numberFormat;
		return obj.DisplayText;
	}

	public void ParseChart(XmlReader reader, ChartImpl chart, RelationCollection relations, double appVersion)
	{
		bool throwOnUnknownNames = m_book.ThrowOnUnknownNames;
		m_book.ThrowOnUnknownNames = false;
		_appVersion = appVersion;
		ParseChart(reader, chart, relations);
		m_book.ThrowOnUnknownNames = throwOnUnknownNames;
	}

	internal void CalculateShapesPosition(ChartImpl chartImpl, double width, double height)
	{
		width = (int)(width * 96.0 / 72.0);
		height = (int)(height * 96.0 / 72.0);
		if (chartImpl == null || chartImpl.Shapes.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < chartImpl.Shapes.Count; i++)
		{
			ShapeImpl shapeImpl = chartImpl.Shapes[i] as ShapeImpl;
			shapeImpl.ChartShapeX = shapeImpl.StartX * width;
			shapeImpl.ChartShapeY = shapeImpl.StartY * height;
			shapeImpl.ChartShapeWidth = shapeImpl.ToX * width - shapeImpl.StartX * width;
			shapeImpl.ChartShapeHeight = shapeImpl.ToY * height - shapeImpl.StartY * height;
			if (shapeImpl is GroupShapeImpl)
			{
				shapeImpl.ShapeFrame.OffsetX = (long)ApplicationImpl.ConvertFromPixel(shapeImpl.ChartShapeX, MeasureUnits.EMU);
				shapeImpl.ShapeFrame.OffsetY = (long)ApplicationImpl.ConvertFromPixel(shapeImpl.ChartShapeY, MeasureUnits.EMU);
				shapeImpl.ShapeFrame.OffsetCY = (long)ApplicationImpl.ConvertFromPixel(shapeImpl.ChartShapeHeight, MeasureUnits.EMU);
				shapeImpl.ShapeFrame.OffsetCX = (long)ApplicationImpl.ConvertFromPixel(shapeImpl.ChartShapeWidth, MeasureUnits.EMU);
			}
		}
	}

	internal static ChartColor ParseInvertSolidFillFormat(Stream stream, ChartSerieImpl serie)
	{
		ChartColor chartColor = null;
		XmlReader xmlReader = UtilityMethods.CreateReader(stream);
		if (xmlReader.LocalName == "invertSolidFillFmt")
		{
			xmlReader.Read();
			while (xmlReader.LocalName != "invertSolidFillFmt")
			{
				if (xmlReader.LocalName == "solidFill")
				{
					chartColor = ColorExtension.Empty;
					ChartParserCommon.ParseSolidFill(xmlReader, serie.ParentChart.ParentWorkbook.DataHolder.Parser, chartColor);
					break;
				}
				if (xmlReader.LocalName == "spPr")
				{
					xmlReader.Read();
				}
				else
				{
					xmlReader.Skip();
				}
			}
		}
		return chartColor;
	}

	public void ParseChart(XmlReader reader, ChartImpl chart, RelationCollection relations)
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
		if (reader.LocalName != "chartSpace")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		chart.IsChartParsed = true;
		reader.Read();
		IOfficeChartFrameFormat chartArea = chart.ChartArea;
		chartArea.Interior.UseAutomaticFormat = true;
		chartArea.Border.AutoFormat = true;
		Stream value = null;
		bool num = chart.RelationPreservedStreamCollection.TryGetValue("http://schemas.openxmlformats.org/officeDocument/2006/relationships/themeOverride", out value);
		Excel2007Parser parser = chart.ParentWorkbook.DataHolder.Parser;
		if (num && value != null)
		{
			chart.IsThemeOverridePresent = true;
			XmlReader reader2 = UtilityMethods.CreateReader(value);
			parser.ParseThemeElements(reader2, isThemeOverride: true);
		}
		else
		{
			parser.m_themeColorOverrideDictionary = null;
		}
		while (reader.NodeType != XmlNodeType.EndElement && reader.LocalName != "chartSpace")
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "clrMapOvr":
					chart.ColorMapOverrideDictionary = ParseColorMapOverrideTag(reader);
					break;
				case "chart":
					ParseChartElement(reader, chart, relations);
					break;
				case "roundedCorners":
					chart.HasChartArea = true;
					chart.ChartArea.IsBorderCornersRound = ChartParserCommon.ParseBoolValueTag(reader);
					break;
				case "spPr":
				{
					IChartFillObjectGetter objectGetter = new ChartFillObjectGetterAny(chartArea.Border as ChartBorderImpl, chartArea.Interior as ChartInteriorImpl, chartArea.Fill as IInternalFill, chartArea.Shadow as ShadowImpl, chartArea.ThreeD as ThreeDFormatImpl);
					ChartParserCommon.ParseShapeProperties(reader, objectGetter, chart.ParentWorkbook.DataHolder, relations);
					break;
				}
				case "style":
					chart.Style = ChartParserCommon.ParseIntValueTag(reader);
					break;
				case "userShapes":
					ParseUserShapes(reader, chart, relations);
					break;
				case "pivotSource":
					ParsePivotSource(reader, chart);
					break;
				case "printSettings":
					ParsePrintSettings(reader, chart, relations);
					break;
				case "extLst":
					ParseExtensionList(reader, chart);
					break;
				case "AlternateContent":
				{
					Stream stream = ShapeParser.ReadNodeAsStream(reader);
					int style = chart.Style;
					ParseStyleIdFromAlternateContent(stream, chart);
					stream.Position = 0L;
					if (style == chart.Style)
					{
						chart.AlternateContent = stream;
					}
					break;
				}
				case "txPr":
					ParseDefaultTextProperties(reader, chart);
					break;
				case "externalData":
					reader.Skip();
					break;
				default:
					reader.Read();
					break;
				}
			}
			else
			{
				reader.Skip();
			}
		}
		if (chart != null && chart.Shapes.Count > 0)
		{
			CalculateShapesPosition(chart, chart.Width, chart.Height);
		}
		chart.DetectIsInRowOnParsing();
		if ((chart.DataRange as ChartDataRange).Range != null && chart.Categories.Count > 0)
		{
			IRange serieRange = null;
			chart.IsSeriesInRows = DetectIsInRow((chart.Series[0].Values as ChartDataRange).Range);
			GetSerieOrAxisRange((chart.DataRange as ChartDataRange).Range, chart.IsSeriesInRows, out serieRange, 0);
			int num2 = 0;
			ChartDataRange chartDataRange = chart.Series[0].CategoryLabels as ChartDataRange;
			if (chartDataRange.Range != null)
			{
				num2 = ((!chart.IsSeriesInRows) ? (chartDataRange.Range.LastColumn - chartDataRange.Range.Column) : (chartDataRange.Range.LastRow - chartDataRange.Range.Row));
			}
			GetSerieOrAxisRange(serieRange, !chart.IsSeriesInRows, out serieRange, num2);
			int count = serieRange.Count / (chart.Series[0].Values as ChartDataRange).Range.Count;
			int num3 = 0;
			num3 = (((chart.Series[0].CategoryLabels as ChartDataRange).Range == null) ? (chart.Series[0].Values as ChartDataRange).Range.Count : ((chart.Series[0].CategoryLabels as ChartDataRange).Range.Count / (num2 + 1)));
			for (int i = 0; i < num3; i++)
			{
				IRange categoryRange = ChartImpl.GetCategoryRange(serieRange, out serieRange, count, chart.IsSeriesInRows);
				(chart.Categories[i] as ChartCategory).CategoryLabelIRange = (chart.Series[0].CategoryLabels as ChartDataRange).Range;
				(chart.Categories[i] as ChartCategory).ValuesIRange = categoryRange;
				if ((chart.Categories[0].CategoryLabel as ChartDataRange).Range != null)
				{
					(chart.Categories[i] as ChartCategory).Name = (chart.Categories[0].CategoryLabel as ChartDataRange).Range.Cells[i].Text;
					continue;
				}
				(chart.Categories[i] as ChartCategory).Name = (i + 1).ToString();
				if (chart.Legend != null && chart.Legend.LegendEntries != null && chart.Legend.LegendEntries.Count > i && chart.Legend.LegendEntries[i].TextArea != null)
				{
					chart.Legend.LegendEntries[i].TextArea.Text = (chart.Categories[i] as ChartCategory).Name;
					chart.Legend.LegendEntries[i].IsFormatted = false;
				}
			}
		}
		else if (chart.Series != null && chart.Series.Count > 0 && chart.Series[0].Values != null && (chart.Series[0].CategoryLabels as ChartDataRange).Range != null && chart.Categories.Count > 0)
		{
			IRange range = (chart.Series[0].CategoryLabels as ChartDataRange).Range;
			int count2 = range.Count;
			int row = range.Row;
			int column = range.Column;
			IRange range2 = null;
			bool flag = false;
			if (range != null && range.GetType() != typeof(ExternalRange) && range.Worksheet != null)
			{
				flag = true;
				if (!chart.IsSeriesInRows && range.LastRow >= row + chart.Categories.Count - 1)
				{
					range2 = range[row, column, row + chart.Categories.Count - 1, column];
				}
				else if (range.LastColumn >= column + chart.Categories.Count - 1)
				{
					range2 = range[row, column, row, column + chart.Categories.Count - 1];
				}
			}
			for (int j = 0; j < chart.Categories.Count; j++)
			{
				(chart.Categories[j] as ChartCategory).CategoryLabelIRange = range;
				(chart.Categories[j] as ChartCategory).ValuesIRange = (chart.Series[0].Values as ChartDataRange).Range;
				if (flag && range2 != null && j < count2)
				{
					(chart.Categories[j] as ChartCategory).Name = range2.Cells[j].DisplayText;
				}
			}
		}
		if (chart.Series.Count != 0 && (chart.Series[0] as ChartSerieImpl).FilteredValue != null)
		{
			FindFilter(chart.Categories, (chart.Series[0] as ChartSerieImpl).FilteredValue, (chart.Series[0].Values as ChartDataRange).Range.AddressGlobal, chart.Series[0], chart.IsSeriesInRows);
		}
		reader.Read();
		if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "chartSpace")
		{
			ChartParserCommon.clear();
		}
		ChartCategoryAxisImpl chartCategoryAxisImpl = chart.PrimaryCategoryAxis as ChartCategoryAxisImpl;
		if (chartCategoryAxisImpl.IsChartBubbleOrScatter)
		{
			chartCategoryAxisImpl.SwapAxisValues();
		}
		if (chart.IsSecondaryCategoryAxisAvail)
		{
			chartCategoryAxisImpl = chart.SecondaryCategoryAxis as ChartCategoryAxisImpl;
			if (chartCategoryAxisImpl.IsChartBubbleOrScatter)
			{
				chartCategoryAxisImpl.SwapAxisValues();
			}
		}
		if ((chart.DataRange as ChartDataRange).SheetImpl.Name != (chart.ChartData as ChartData).SheetImpl.Name)
		{
			(chart.ChartData as ChartData).SheetImpl = (chart.DataRange as ChartDataRange).SheetImpl;
		}
	}

	internal static Dictionary<string, string> ParseColorMapOverrideTag(XmlReader reader)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		while (reader.MoveToNextAttribute())
		{
			if (reader.Name != "xmlns:a")
			{
				dictionary.Add(reader.Name, reader.Value);
			}
		}
		return dictionary;
	}

	private void ChangeKeyToChartGroup(ChartImpl chart)
	{
		Dictionary<int, ChartDataPointsCollection> dictionary = new Dictionary<int, ChartDataPointsCollection>();
		foreach (int key2 in chart.CommonDataPointsCollection.Keys)
		{
			int key = 0;
			for (int i = 0; i < chart.Series.Count; i++)
			{
				ChartSerieImpl chartSerieImpl = chart.Series[i] as ChartSerieImpl;
				if (chartSerieImpl.Number == key2)
				{
					key = chartSerieImpl.ChartGroup;
					break;
				}
			}
			if (!dictionary.ContainsKey(key))
			{
				dictionary.Add(key, chart.CommonDataPointsCollection[key2]);
			}
		}
		chart.CommonDataPointsCollection = dictionary;
	}

	private void ParseDataLabels(XmlReader reader, ChartImpl chart, RelationCollection relations, int index)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "dLbls")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		if (chart.CommonDataPointsCollection == null)
		{
			chart.CommonDataPointsCollection = new Dictionary<int, ChartDataPointsCollection>();
		}
		if (!chart.CommonDataPointsCollection.ContainsKey(index))
		{
			ChartDataPointsCollection value = new ChartDataPointsCollection(chart.Application, chart);
			chart.CommonDataPointsCollection.Add(index, value);
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					if (reader.LocalName == "dLbl")
					{
						ParseDataLabel(reader, chart, relations, index);
						continue;
					}
					IOfficeChartDataLabels dataLabels = chart.CommonDataPointsCollection[index].DefaultDataPoint.DataLabels;
					FileDataHolder dataHolder = chart.ParentWorkbook.DataHolder;
					Excel2007Parser parser = dataHolder.Parser;
					ParseDataLabelSettings(reader, dataLabels, parser, dataHolder, relations, isChartExSeries: false);
				}
				else
				{
					reader.Skip();
				}
			}
			reader.Read();
		}
		else
		{
			reader.Skip();
		}
	}

	private void ParseDataLabel(XmlReader reader, ChartImpl chart, RelationCollection relations, int index)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("series");
		}
		if (reader.LocalName != "dLbl")
		{
			throw new XmlException("Unexpeced xml tag.");
		}
		reader.Read();
		IOfficeChartDataLabels officeChartDataLabels = null;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "idx":
				{
					int index2 = ChartParserCommon.ParseIntValueTag(reader);
					officeChartDataLabels = chart.CommonDataPointsCollection[index][index2].DataLabels;
					(officeChartDataLabels as ChartDataLabelsImpl).ShowTextProperties = false;
					break;
				}
				case "layout":
					(officeChartDataLabels as ChartDataLabelsImpl).Layout = new ChartLayoutImpl(m_book.Application, officeChartDataLabels as ChartDataLabelsImpl, chart);
					ChartParserCommon.ParseChartLayout(reader, (officeChartDataLabels as ChartDataLabelsImpl).Layout);
					break;
				case "delete":
				{
					bool isDelete = ChartParserCommon.ParseBoolValueTag(reader);
					(officeChartDataLabels as ChartDataLabelsImpl).IsDelete = isDelete;
					break;
				}
				default:
				{
					FileDataHolder dataHolder = chart.ParentWorkbook.DataHolder;
					Excel2007Parser parser = dataHolder.Parser;
					ParseDataLabelSettings(reader, officeChartDataLabels, parser, dataHolder, relations, isChartExSeries: false);
					break;
				}
				}
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Read();
	}

	private void ParseStyleIdFromAlternateContent(Stream stream, ChartImpl chart)
	{
		XmlReader xmlReader = UtilityMethods.CreateReader(stream);
		if (!(xmlReader.LocalName == "AlternateContent"))
		{
			return;
		}
		xmlReader.Read();
		Excel2007Parser.SkipWhiteSpaces(xmlReader);
		while (xmlReader.NodeType != 0)
		{
			if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.LocalName == "style")
			{
				chart.Style = ChartParserCommon.ParseIntValueTag(xmlReader);
				break;
			}
			if (xmlReader.NodeType != XmlNodeType.EndElement || (!(xmlReader.LocalName == "Choice") && !(xmlReader.LocalName == "AlternateContent")))
			{
				xmlReader.Read();
				continue;
			}
			break;
		}
	}

	internal void ParseDefaultTextProperties(XmlReader reader, ChartImpl chart)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "txPr")
		{
			throw new XmlException("Unexpected tag name");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		chart.DefaultTextProperty = ShapeParser.ReadNodeAsStream(reader);
		chart.DefaultTextProperty.Position = 0L;
		XmlReader xmlReader = UtilityMethods.CreateReader(chart.DefaultTextProperty);
		if (xmlReader.LocalName != "txPr")
		{
			throw new XmlException();
		}
		if (!xmlReader.IsEmptyElement)
		{
			xmlReader.Read();
			while (xmlReader.NodeType != XmlNodeType.EndElement)
			{
				if (xmlReader.NodeType == XmlNodeType.Element)
				{
					string localName = xmlReader.LocalName;
					if (!(localName == "bodyPr"))
					{
						if (localName == "p")
						{
							ParserChartParagraphs(xmlReader, chart);
						}
						else
						{
							xmlReader.Skip();
						}
					}
					else
					{
						ParseChartBodyProperties(xmlReader, chart);
					}
				}
				else
				{
					xmlReader.Skip();
				}
			}
		}
		xmlReader.Read();
	}

	private void ParseChartBodyProperties(XmlReader reader, ChartImpl chart)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "bodyPr")
		{
			throw new XmlException();
		}
		reader.MoveToElement();
		reader.Skip();
	}

	private void ParserChartParagraphs(XmlReader reader, ChartImpl chart)
	{
		FileDataHolder parentHolder = chart.DataHolder.ParentHolder;
		while (!(reader.LocalName == "p") || reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "defRPr")
			{
				TextSettings defaultSettings = ChartParserCommon.ParseDefaultParagraphProperties(reader, parentHolder.Parser, parentHolder.Workbook);
				ChartParserCommon.CopyDefaultSettings(chart.Font as IInternalFont, defaultSettings, parentHolder.Workbook);
				CheckDefaultTextSettings(chart);
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void CheckDefaultTextSettings(ChartImpl chart)
	{
		foreach (ChartAxisImpl item in new List<ChartAxisImpl>
		{
			(ChartAxisImpl)chart.PrimaryCategoryAxis,
			(ChartAxisImpl)chart.PrimaryValueAxis,
			(ChartAxisImpl)chart.SecondaryValueAxis,
			(ChartAxisImpl)chart.SecondaryCategoryAxis
		})
		{
			if (item != null && item.IsDefaultTextSettings)
			{
				item.Font = (FontWrapper)((FontWrapper)chart.Font).Clone(chart);
				item.IsChartFont = true;
				item.IsDefaultTextSettings = true;
			}
		}
	}

	private void ParseExtensionList(XmlReader reader, ChartImpl chart)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.IsEmptyElement)
		{
			reader.Read();
			return;
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				if (reader.LocalName == "ext")
				{
					ParseExtension(reader, chart);
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

	private void ParseExtension(XmlReader reader, ChartImpl chart)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				if (reader.LocalName == "pivotOptions")
				{
					ParsePivotOptions(reader, chart);
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

	private void ParsePivotOptions(XmlReader reader, ChartImpl chart)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "pivotOptions")
		{
			throw new XmlException("Unexpected tag name");
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "dropZoneCategories":
					if (reader.MoveToAttribute("val"))
					{
						chart.ShowAxisFieldButtons = true;
					}
					break;
				case "dropZoneData":
					if (reader.MoveToAttribute("val"))
					{
						chart.ShowValueFieldButtons = true;
					}
					break;
				case "dropZoneFilter":
					if (reader.MoveToAttribute("val"))
					{
						chart.ShowReportFilterFieldButtons = true;
					}
					break;
				case "dropZoneSeries":
					if (reader.MoveToAttribute("val"))
					{
						chart.ShowLegendFieldButtons = true;
					}
					break;
				case "dropZonesVisible":
					if (reader.MoveToAttribute("val"))
					{
						chart.ShowAllFieldButtons = true;
					}
					break;
				default:
					reader.Read();
					break;
				}
			}
			else
			{
				reader.Read();
			}
		}
		if (reader.LocalName == "pivotOptions")
		{
			reader.Read();
		}
	}

	internal void ParsePrintSettings(XmlReader reader, ChartImpl chart, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "printSettings")
		{
			throw new XmlException();
		}
		ChartPageSetupConstants constants = new ChartPageSetupConstants();
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			IPageSetupBase pageSetup = chart.PageSetup;
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					switch (reader.LocalName)
					{
					case "printOptions":
						Excel2007Parser.ParsePrintOptions(reader, pageSetup);
						break;
					case "pageMargins":
						Excel2007Parser.ParsePageMargins(reader, pageSetup, constants);
						break;
					case "pageSetup":
						Excel2007Parser.ParsePageSetup(reader, (PageSetupBaseImpl)pageSetup);
						break;
					case "headerFooter":
						Excel2007Parser.ParseHeaderFooter(reader, (PageSetupBaseImpl)pageSetup);
						break;
					case "legacyDrawingHF":
						Excel2007Parser.ParseLegacyDrawingHF(reader, chart, relations);
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

	private void ParsePivotSource(XmlReader reader, ChartImpl chart)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "pivotSource")
		{
			throw new XmlException("Unexpected xml tag");
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				if (reader.LocalName == "name")
				{
					string text2 = (chart.PreservedPivotSource = reader.ReadElementContentAsString());
					if (text2 != null)
					{
						chart.ShowAllFieldButtons = false;
						chart.ShowAxisFieldButtons = false;
						chart.ShowLegendFieldButtons = false;
						chart.ShowReportFilterFieldButtons = false;
						chart.ShowValueFieldButtons = false;
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
		reader.Read();
	}

	private void ParseUserShapes(XmlReader reader, ChartImpl chart, RelationCollection relations)
	{
		reader.MoveToAttribute("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
		string value = reader.Value;
		Dictionary<string, object> dictItemsToRemove = new Dictionary<string, object>();
		Relation drawingRelation = relations[value];
		chart.DataHolder.ParseDrawings(chart, drawingRelation, dictItemsToRemove, isChartShape: true);
	}

	private void ParseChartElement(XmlReader reader, ChartImpl chart, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "chart")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		FileDataHolder parentHolder = chart.DataHolder.ParentHolder;
		bool flag = false;
		MemoryStream memoryStream = new MemoryStream();
		XmlWriter xmlWriter = UtilityMethods.CreateWriter(memoryStream, Encoding.UTF8);
		xmlWriter.WriteStartElement("root");
		Chart3DRecord chart3D = null;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "view3D":
					chart3D = ParseView3D(reader, chart);
					break;
				case "plotArea":
					ParsePlotArea(reader, chart, relations, parentHolder.Parser);
					break;
				case "legend":
					chart.HasLegend = true;
					ParseLegend(reader, chart.Legend, chart, relations);
					break;
				case "floor":
					ParseSurface(reader, chart.Floor, parentHolder, relations);
					break;
				case "sideWall":
					ParseSurface(reader, chart.SideWall, parentHolder, relations);
					break;
				case "backWall":
					ParseSurface(reader, chart.Walls, parentHolder, relations);
					break;
				case "dispBlanksAs":
					if (reader.MoveToAttribute("val"))
					{
						chart.DisplayBlanksAs = (OfficeChartPlotEmpty)Enum.Parse(typeof(Excel2007ChartPlotEmpty), reader.Value.ToString(), ignoreCase: true);
					}
					break;
				case "title":
					xmlWriter.WriteNode(reader, defattr: false);
					xmlWriter.Flush();
					flag = true;
					break;
				case "autoTitleDeleted":
					ParseAutoTitleDeleted(reader, chart);
					break;
				case "pivotFmts":
					ParsePivotFormats(reader, chart);
					break;
				case "plotVisOnly":
					chart.PlotVisibleOnly = ChartParserCommon.ParseBoolValueTag(reader);
					break;
				case "showDLblsOverMax":
					if (reader.MoveToAttribute("val"))
					{
						chart.m_showDlbSOverMax = ChartParserCommon.ParseValueTag(reader);
					}
					else
					{
						reader.Skip();
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
		xmlWriter.WriteEndElement();
		xmlWriter.Flush();
		if (flag)
		{
			IInternalOfficeChartTextArea internalOfficeChartTextArea = chart.ChartTitleArea as IInternalOfficeChartTextArea;
			memoryStream.Position = 0L;
			XmlReader xmlReader = UtilityMethods.CreateReader(memoryStream);
			xmlReader.Read();
			if (xmlReader.LocalName == "title")
			{
				xmlReader.Read();
				while (xmlReader.NodeType != XmlNodeType.EndElement)
				{
					if (xmlReader.NodeType == XmlNodeType.Element)
					{
						if (xmlReader.LocalName == "txPr")
						{
							((ChartTextAreaImpl)internalOfficeChartTextArea).ParagraphType = ChartParagraphType.CustomDefault;
							ChartParserCommon.ParseDefaultTextFormatting(xmlReader, internalOfficeChartTextArea, parentHolder.Parser, 18.0);
							((ChartTextAreaImpl)internalOfficeChartTextArea).IsTextParsed = true;
						}
						else
						{
							xmlReader.Skip();
						}
					}
					else
					{
						xmlReader.Skip();
					}
				}
			}
			memoryStream.Position = 0L;
			XmlReader xmlReader2 = UtilityMethods.CreateReader(memoryStream);
			xmlReader2.Read();
			if (xmlReader2.LocalName == "title" && !xmlReader2.IsEmptyElement)
			{
				ChartParserCommon.SetWorkbook(m_book);
				ChartParserCommon.ParseTextArea(xmlReader2, internalOfficeChartTextArea, parentHolder, relations, 18f);
				chart.HasTitle = true;
			}
			xmlReader2.Dispose();
			xmlReader.Dispose();
			memoryStream.Dispose();
		}
		reader.Read();
		Set3DSettings(chart, chart3D);
	}

	internal void TryParsePositioningValues(XmlReader reader, out bool? isOverlay, out ushort position)
	{
		isOverlay = null;
		position = 0;
		if (reader.MoveToAttribute("align"))
		{
			ChartExPositionAlignment chartExPositionAlignment = (ChartExPositionAlignment)Enum.Parse(typeof(ChartExPositionAlignment), reader.Value, ignoreCase: false);
			position |= (ushort)chartExPositionAlignment;
		}
		if (reader.MoveToAttribute("pos"))
		{
			ChartExSidePosition chartExSidePosition = (ChartExSidePosition)Enum.Parse(typeof(ChartExSidePosition), reader.Value, ignoreCase: false);
			position |= (ushort)chartExSidePosition;
		}
		if (reader.MoveToAttribute("overlay"))
		{
			isOverlay = XmlConvertExtension.ToBoolean(reader.Value);
		}
		reader.MoveToElement();
	}

	private void ParseAutoTitleDeleted(XmlReader reader, ChartImpl chart)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.MoveToAttribute("val"))
		{
			chart.HasAutoTitle = XmlConvertExtension.ToBoolean(reader.Value);
		}
		reader.Read();
	}

	private void ParsePivotFormats(XmlReader reader, ChartImpl chart)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		chart.PivotFormatsStream = ShapeParser.ReadNodeAsStream(reader);
	}

	private void Set3DSettings(ChartImpl chart, Chart3DRecord chart3D)
	{
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (!(chart3D == null) && chart.Series.Count != 0)
		{
			if (!chart3D.IsDefaultElevation)
			{
				chart.Elevation = chart3D.ElevationAngle;
			}
			chart.HeightPercent = chart3D.Height;
			chart.AutoScaling = chart3D.IsAutoScaled;
			if (!chart3D.IsDefaultRotation)
			{
				chart.Rotation = chart3D.RotationAngle;
			}
			chart.DepthPercent = chart3D.Depth;
			chart.Perspective = chart3D.DistanceFromEye;
			chart.RightAngleAxes = chart3D.IsPerspective;
		}
	}

	internal void ParseLegend(XmlReader reader, IOfficeChartLegend legend, ChartImpl chart, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (legend == null)
		{
			throw new ArgumentNullException("legend");
		}
		if (reader.LocalName != "legend")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		bool isEmptyElement = reader.IsEmptyElement;
		if (this is ChartExParser)
		{
			bool? isOverlay = null;
			ushort position = 0;
			TryParsePositioningValues(reader, out isOverlay, out position);
			if (isOverlay.HasValue)
			{
				legend.IncludeInLayout = isOverlay.Value;
			}
			if (position != 0)
			{
				(legend as ChartLegendImpl).ChartExPosition = position;
			}
			legend.Position = GetChartLegendPosition(position);
		}
		reader.Read();
		Excel2007Parser parser = chart.ParentWorkbook.DataHolder.Parser;
		(legend as ChartLegendImpl).IsChartTextArea = true;
		legend.TextArea.FontName = "Calibri";
		legend.TextArea.Size = 10.0;
		(legend as ChartLegendImpl).IsChartTextArea = false;
		if (!isEmptyElement)
		{
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					switch (reader.LocalName)
					{
					case "legendPos":
					{
						string value = ChartParserCommon.ParseValueTag(reader);
						legend.Position = (OfficeLegendPosition)(Excel2007LegendPosition)Enum.Parse(typeof(Excel2007LegendPosition), value, ignoreCase: false);
						break;
					}
					case "legendEntry":
						ParseLegendEntry(reader, legend, parser);
						break;
					case "txPr":
					{
						ChartLegendImpl chartLegendImpl = legend as ChartLegendImpl;
						chartLegendImpl.IsChartTextArea = true;
						IInternalOfficeChartTextArea internalOfficeChartTextArea = legend.TextArea as IInternalOfficeChartTextArea;
						ParseDefaultTextFormatting(reader, internalOfficeChartTextArea, parser);
						if (chartLegendImpl.TextArea.Size != 10.0 || (internalOfficeChartTextArea as ChartTextAreaImpl).ShowSizeProperties)
						{
							chartLegendImpl.IsDefaultTextSettings = false;
						}
						chartLegendImpl.IsChartTextArea = false;
						break;
					}
					case "spPr":
					{
						IOfficeChartFrameFormat frameFormat = legend.FrameFormat;
						ChartFillObjectGetterAny objectGetter = new ChartFillObjectGetterAny(frameFormat.Border as ChartBorderImpl, frameFormat.Interior as ChartInteriorImpl, frameFormat.Fill as IInternalFill, frameFormat.Shadow as ShadowImpl, frameFormat.ThreeD as ThreeDFormatImpl);
						FileDataHolder dataHolder = chart.ParentWorkbook.DataHolder;
						ChartParserCommon.ParseShapeProperties(reader, objectGetter, dataHolder, relations);
						break;
					}
					case "layout":
						(legend as ChartLegendImpl).Layout = new ChartLayoutImpl(m_book.Application, legend as ChartLegendImpl, chart);
						ChartParserCommon.ParseChartLayout(reader, (legend as ChartLegendImpl).Layout);
						break;
					case "overlay":
						legend.IncludeInLayout = !ChartParserCommon.ParseBoolValueTag(reader);
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

	private OfficeLegendPosition GetChartLegendPosition(ushort position)
	{
		OfficeLegendPosition result = OfficeLegendPosition.NotDocked;
		ushort num = 240;
		ChartExPositionAlignment chartExPositionAlignment = (ChartExPositionAlignment)(position & num);
		num = 15;
		ChartExSidePosition chartExSidePosition = (ChartExSidePosition)(position & num);
		switch (chartExPositionAlignment)
		{
		case ChartExPositionAlignment.ctr:
			switch (chartExSidePosition)
			{
			case ChartExSidePosition.b:
				result = OfficeLegendPosition.Bottom;
				break;
			case ChartExSidePosition.l:
				result = OfficeLegendPosition.Left;
				break;
			case ChartExSidePosition.r:
				result = OfficeLegendPosition.Right;
				break;
			case ChartExSidePosition.t:
				result = OfficeLegendPosition.Top;
				break;
			}
			break;
		case ChartExPositionAlignment.min:
			if (chartExSidePosition == ChartExSidePosition.r)
			{
				result = OfficeLegendPosition.Corner;
			}
			break;
		}
		return result;
	}

	internal static IRange GetRange(WorkbookImpl workbook, string formula)
	{
		FormulaUtil formulaUtil = workbook.DataHolder.Parser.FormulaUtil;
		if (!ChartImpl.TryAndModifyToValidFormula(formula))
		{
			return null;
		}
		Ptg[] array = formulaUtil.ParseString(formula);
		IWorksheet sheet = ((workbook.Worksheets.Count > 0) ? workbook.Worksheets[0] : null);
		IRangeGetter rangeGetter = null;
		IRange result = null;
		if (array.Length == 1)
		{
			if (!(array[0] is IRangeGetter rangeGetter2))
			{
				return null;
			}
			result = rangeGetter2.GetRange(workbook, sheet);
		}
		else
		{
			IRangeGetter rangeGetter3 = array[0] as IRangeGetter;
			IRange range = rangeGetter3.GetRange(workbook, sheet);
			if (range != null)
			{
				IRanges ranges = range.Worksheet.CreateRangesCollection();
				ranges.Add(range);
				for (int i = 1; i < array.Length; i++)
				{
					if (!array[i].IsOperation && array[i] is IRangeGetter rangeGetter4)
					{
						range = rangeGetter4.GetRange(workbook, sheet);
						if (range != null)
						{
							ranges.Add(range);
						}
					}
				}
				result = ((ranges.Count < 1) ? null : ranges);
			}
		}
		return result;
	}

	private void ParseLegendEntry(XmlReader reader, IOfficeChartLegend legend, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (legend == null)
		{
			throw new ArgumentNullException("legend");
		}
		if (reader.LocalName != "legendEntry")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		int iIndex = 0;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "idx":
					iIndex = int.Parse(ChartParserCommon.ParseValueTag(reader));
					break;
				case "delete":
				{
					bool isDeleted = ChartParserCommon.ParseBoolValueTag(reader);
					legend.LegendEntries[iIndex].IsDeleted = isDeleted;
					break;
				}
				case "txPr":
				{
					IInternalOfficeChartTextArea textFormatting = legend.LegendEntries[iIndex].TextArea as IInternalOfficeChartTextArea;
					ParseDefaultTextFormatting(reader, textFormatting, parser);
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
		reader.Read();
	}

	private Chart3DRecord ParseView3D(XmlReader reader, ChartImpl chart)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "view3D")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		Chart3DRecord chart3DRecord = (Chart3DRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Chart3D);
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "rotX":
				{
					string s6 = ChartParserCommon.ParseValueTag(reader);
					chart3DRecord.ElevationAngle = short.Parse(s6);
					break;
				}
				case "hPercent":
				{
					chart3DRecord.IsAutoScaled = false;
					string s5 = ChartParserCommon.ParseValueTag(reader);
					chart3DRecord.Height = ushort.Parse(s5);
					break;
				}
				case "rotY":
				{
					string s4 = ChartParserCommon.ParseValueTag(reader);
					chart3DRecord.RotationAngle = ushort.Parse(s4);
					break;
				}
				case "depthPercent":
				{
					string s3 = ChartParserCommon.ParseValueTag(reader);
					chart3DRecord.Depth = ushort.Parse(s3);
					break;
				}
				case "rAngAx":
				{
					string s2 = ChartParserCommon.ParseValueTag(reader);
					chart3DRecord.IsPerspective = XmlConvertExtension.ToBoolean(s2);
					break;
				}
				case "perspective":
				{
					string s = ChartParserCommon.ParseValueTag(reader);
					chart3DRecord.DistanceFromEye = (ushort)(int.Parse(s) / 2);
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
		reader.Read();
		return chart3DRecord;
	}

	private void ParseErrorBars(XmlReader reader, ChartSerieImpl series, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		if (reader.LocalName != "errBars")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		IOfficeChartErrorBars officeChartErrorBars = null;
		WorkbookImpl parentBook = series.ParentBook;
		FileDataHolder dataHolder = parentBook.DataHolder;
		object[] values = null;
		ChartErrorBarsImpl chartErrorBarsImpl = null;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "errDir":
					if (ChartParserCommon.ParseValueTag(reader) == "x")
					{
						series.HasErrorBarsX = true;
						officeChartErrorBars = series.ErrorBarsX;
					}
					else
					{
						series.HasErrorBarsY = true;
						officeChartErrorBars = series.ErrorBarsY;
					}
					chartErrorBarsImpl = officeChartErrorBars as ChartErrorBarsImpl;
					break;
				case "errBarType":
				{
					string value2 = ChartParserCommon.ParseValueTag(reader);
					if (officeChartErrorBars == null)
					{
						series.HasErrorBarsY = true;
						officeChartErrorBars = series.ErrorBarsY;
					}
					officeChartErrorBars.Include = (OfficeErrorBarInclude)Enum.Parse(typeof(OfficeErrorBarInclude), value2, ignoreCase: true);
					break;
				}
				case "errValType":
				{
					string value = ChartParserCommon.ParseValueTag(reader);
					Excel2007ErrorBarType type = (Excel2007ErrorBarType)Enum.Parse(typeof(Excel2007ErrorBarType), value, ignoreCase: false);
					officeChartErrorBars.Type = (OfficeErrorBarType)type;
					break;
				}
				case "noEndCap":
					officeChartErrorBars.HasCap = !ChartParserCommon.ParseBoolValueTag(reader);
					break;
				case "plus":
				{
					IRange range2 = ParseErrorBarRange(reader, parentBook, out values, officeChartErrorBars, series, "plus");
					if (!((ChartErrorBarsImpl)officeChartErrorBars).IsPlusNumberLiteral && officeChartErrorBars.Include != OfficeErrorBarInclude.Minus)
					{
						officeChartErrorBars.PlusRange = (series.ParentChart.ChartData as ChartData)[range2];
					}
					if (chartErrorBarsImpl == null && officeChartErrorBars != null)
					{
						chartErrorBarsImpl = officeChartErrorBars as ChartErrorBarsImpl;
					}
					chartErrorBarsImpl.PlusRangeValues = values;
					break;
				}
				case "minus":
				{
					IRange range = ParseErrorBarRange(reader, parentBook, out values, officeChartErrorBars, series, "minus");
					if (!((ChartErrorBarsImpl)officeChartErrorBars).IsMinusNumberLiteral && officeChartErrorBars.Include != OfficeErrorBarInclude.Plus)
					{
						officeChartErrorBars.MinusRange = (series.ParentChart.ChartData as ChartData)[range];
					}
					if (chartErrorBarsImpl == null && officeChartErrorBars != null)
					{
						chartErrorBarsImpl = officeChartErrorBars as ChartErrorBarsImpl;
					}
					chartErrorBarsImpl.MinusRangeValues = values;
					break;
				}
				case "val":
					officeChartErrorBars.NumberValue = ChartParserCommon.ParseDoubleValueTag(reader);
					break;
				case "spPr":
				{
					ChartFillObjectGetterAny objectGetter = new ChartFillObjectGetterAny(officeChartErrorBars.Border as ChartBorderImpl, null, null, officeChartErrorBars.Shadow as ShadowImpl, officeChartErrorBars.Chart3DOptions as ThreeDFormatImpl);
					ChartParserCommon.ParseShapeProperties(reader, objectGetter, dataHolder, relations);
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
		CheckCustomErrorBarType(officeChartErrorBars);
		reader.Read();
	}

	private void CheckCustomErrorBarType(IOfficeChartErrorBars errorBars)
	{
		if (errorBars.Type == OfficeErrorBarType.Custom && (!((ChartErrorBarsImpl)errorBars).IsPlusNumberLiteral || !((ChartErrorBarsImpl)errorBars).IsMinusNumberLiteral) && errorBars.MinusRange != null && errorBars.MinusRange != null)
		{
			errorBars.Include = OfficeErrorBarInclude.Both;
		}
	}

	private IRange ParseErrorBarRange(XmlReader reader, IWorkbook book, out object[] values, IOfficeChartErrorBars errorBars, ChartSerieImpl series, string tagName)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		bool flag = false;
		if (reader.LocalName == "plus")
		{
			flag = true;
		}
		reader.Read();
		string text = null;
		values = null;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "numRef")
			{
				text = ParseNumReference(reader, out values, series, tagName);
			}
			else if (reader.LocalName == "numLit")
			{
				if (flag)
				{
					((ChartErrorBarsImpl)errorBars).IsPlusNumberLiteral = true;
				}
				else
				{
					((ChartErrorBarsImpl)errorBars).IsMinusNumberLiteral = true;
				}
				_ = string.Empty;
				values = ParseDirectlyEnteredValues(reader, series, "numLit");
			}
		}
		reader.Read();
		if (text != null)
		{
			FormulaUtil formulaUtil = (book as WorkbookImpl).DataHolder.Parser.FormulaUtil;
			if (!ChartImpl.TryAndModifyToValidFormula(text))
			{
				return null;
			}
			return (formulaUtil.ParseString(text)[0] as IRangeGetter).GetRange(book, book.Worksheets[0]);
		}
		return null;
	}

	private void ParseTrendlines(XmlReader reader, ChartSerieImpl series, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		while (reader.LocalName == "trendline")
		{
			ParseTrendline(reader, series, relations);
			while (reader.NodeType != XmlNodeType.EndElement && reader.NodeType != XmlNodeType.Element)
			{
				reader.Read();
			}
		}
	}

	private void ParseTrendline(XmlReader reader, ChartSerieImpl series, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		if (reader.LocalName != "trendline")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		IOfficeChartTrendLine officeChartTrendLine = series.TrendLines.Add();
		FileDataHolder dataHolder = series.ParentBook.DataHolder;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "name":
					officeChartTrendLine.Name = reader.ReadElementContentAsString();
					break;
				case "spPr":
				{
					ChartFillObjectGetterAny objectGetter = new ChartFillObjectGetterAny(officeChartTrendLine.Border as ChartBorderImpl, null, null, officeChartTrendLine.Shadow as ShadowImpl, officeChartTrendLine.Chart3DOptions as ThreeDFormatImpl);
					ChartParserCommon.ParseShapeProperties(reader, objectGetter, dataHolder, relations);
					break;
				}
				case "trendlineType":
				{
					string value = ChartParserCommon.ParseValueTag(reader);
					Excel2007TrendlineType type = (Excel2007TrendlineType)Enum.Parse(typeof(Excel2007TrendlineType), value, ignoreCase: false);
					officeChartTrendLine.Type = (OfficeTrendLineType)type;
					break;
				}
				case "period":
				case "order":
					officeChartTrendLine.Order = ChartParserCommon.ParseIntValueTag(reader);
					break;
				case "forward":
					officeChartTrendLine.Forward = ChartParserCommon.ParseDoubleValueTag(reader);
					break;
				case "backward":
					officeChartTrendLine.Backward = ChartParserCommon.ParseDoubleValueTag(reader);
					break;
				case "intercept":
					officeChartTrendLine.Intercept = ChartParserCommon.ParseDoubleValueTag(reader);
					break;
				case "dispRSqr":
					officeChartTrendLine.DisplayRSquared = ChartParserCommon.ParseBoolValueTag(reader);
					break;
				case "dispEq":
					officeChartTrendLine.DisplayEquation = ChartParserCommon.ParseBoolValueTag(reader);
					break;
				case "trendlineLbl":
					ParseTrendlineLabel(reader, officeChartTrendLine);
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

	private void ParseTrendlineLabel(XmlReader reader, IOfficeChartTrendLine trendline)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (trendline == null)
		{
			throw new ArgumentNullException("trendline");
		}
		if (reader.LocalName != "trendlineLbl")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				reader.Skip();
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Read();
	}

	private void ParseSurface(XmlReader reader, IOfficeChartWallOrFloor surface, FileDataHolder dataHolder, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (surface == null)
		{
			throw new ArgumentNullException("surface");
		}
		((ChartWallOrFloorImpl)surface).HasShapeProperties = false;
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					switch (reader.LocalName)
					{
					case "spPr":
					{
						((ChartWallOrFloorImpl)surface).HasShapeProperties = true;
						IChartFillObjectGetter objectGetter = new ChartFillObjectGetterAny(surface.LineProperties as ChartBorderImpl, surface.Interior as ChartInteriorImpl, surface.Fill as IInternalFill, surface.Shadow as ShadowImpl, surface.ThreeD as ThreeDFormatImpl);
						ChartParserCommon.ParseShapeProperties(reader, objectGetter, dataHolder, relations);
						break;
					}
					case "thickness":
					{
						string s = ChartParserCommon.ParseValueTag(reader);
						((ChartWallOrFloorImpl)surface).Thickness = (uint)int.Parse(s);
						break;
					}
					case "pictureOptions":
						reader.Read();
						_ = reader.LocalName == "pictureFormat";
						if (ChartParserCommon.ParseValueTag(reader) == OfficeChartPictureType.stack.ToString())
						{
							((ChartWallOrFloorImpl)surface).PictureUnit = OfficeChartPictureType.stack;
						}
						reader.Skip();
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

	private void ParsePlotArea(XmlReader reader, ChartImpl chart, RelationCollection relations, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "plotArea")
		{
			throw new XmlException("Unexpected xml tag");
		}
		Stream stream = ShapeParser.ReadNodeAsStream(reader);
		stream.Position = 0L;
		reader = UtilityMethods.CreateReader(stream);
		reader.Read();
		ParsePlotAreaAxes(reader, chart, relations, parser);
		stream.Position = 0L;
		reader = UtilityMethods.CreateReader(stream);
		reader.Read();
		ParsePlotAreaGeneral(reader, chart, relations, parser);
	}

	private void ParsePlotAreaGeneral(XmlReader reader, ChartImpl chart, RelationCollection relations, Excel2007Parser parser)
	{
		IOfficeChartFrameFormat plotArea = chart.PlotArea;
		FileDataHolder parentHolder = chart.DataHolder.ParentHolder;
		Dictionary<int, int> dictSeriesAxis = new Dictionary<int, int>();
		bool isBorderCornersRound = chart.PlotArea != null && chart.PlotArea.IsBorderCornersRound;
		chart.HasPlotArea = false;
		List<int> list = null;
		int num = 0;
		int num2 = 0;
		while (reader.NodeType != XmlNodeType.EndElement && reader.NodeType != 0)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				if (reader.LocalName == "catAx")
				{
					num++;
				}
				else if (reader.LocalName == "valAx")
				{
					num2++;
				}
				switch (reader.LocalName)
				{
				case "layout":
					if (chart.PlotArea == null)
					{
						chart.PlotArea = new ChartPlotAreaImpl(m_book.Application, chart);
					}
					chart.PlotArea.Layout = new ChartLayoutImpl(m_book.Application, chart.PlotArea, chart);
					ChartParserCommon.ParseChartLayout(reader, chart.PlotArea.Layout);
					break;
				case "dTable":
					ParseDataTable(reader, chart);
					break;
				case "spPr":
				{
					chart.HasPlotArea = true;
					plotArea = chart.PlotArea;
					chart.PlotArea.IsBorderCornersRound = isBorderCornersRound;
					ChartFillObjectGetterAny objectGetter = new ChartFillObjectGetterAny(plotArea.Border as ChartBorderImpl, plotArea.Interior as ChartInteriorImpl, plotArea.Fill as IInternalFill, plotArea.Shadow as ShadowImpl, plotArea.ThreeD as ThreeDFormatImpl);
					ChartParserCommon.ParseShapeProperties(reader, objectGetter, parentHolder, relations);
					break;
				}
				case "barChart":
					ParseBarChart(reader, chart, relations, dictSeriesAxis);
					break;
				case "bar3DChart":
					ParseBar3DChart(reader, chart, relations, dictSeriesAxis);
					break;
				case "areaChart":
					ParseAreaChart(reader, chart, relations, dictSeriesAxis);
					break;
				case "area3DChart":
					ParseArea3DChart(reader, chart, relations, dictSeriesAxis);
					break;
				case "lineChart":
					if (list == null)
					{
						list = new List<int>();
					}
					ParseLineChart(reader, chart, relations, dictSeriesAxis, parser, list);
					break;
				case "line3DChart":
					ParseLine3DChart(reader, chart, relations, dictSeriesAxis, parser);
					break;
				case "bubble3D":
				case "bubbleChart":
					ParseBubbleChart(reader, chart, relations, dictSeriesAxis);
					break;
				case "surfaceChart":
					ParseSurfaceChart(reader, chart, relations, dictSeriesAxis);
					break;
				case "surface3DChart":
					ParseSurfaceChart(reader, chart, relations, dictSeriesAxis);
					break;
				case "radarChart":
					ParseRadarChart(reader, chart, relations, dictSeriesAxis, parser);
					break;
				case "scatterChart":
					ParseScatterChart(reader, chart, relations, dictSeriesAxis, parser);
					break;
				case "pieChart":
					ParsePieChart(reader, chart, relations, dictSeriesAxis);
					break;
				case "pie3DChart":
					ParsePie3DChart(reader, chart, relations, dictSeriesAxis);
					break;
				case "doughnutChart":
					ParseDoughnutChart(reader, chart, relations, dictSeriesAxis);
					break;
				case "ofPieChart":
					ParseOfPieChart(reader, chart, relations, dictSeriesAxis);
					break;
				case "stockChart":
					ParseStockChart(reader, chart, relations, dictSeriesAxis, parser);
					chart.IsStock = true;
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
		if (num > 1)
		{
			chart.IsPrimarySecondaryCategory = true;
		}
		if (num2 > 1)
		{
			chart.IsPrimarySecondaryValue = true;
		}
		ChartSeriesCollection chartSeriesCollection = (ChartSeriesCollection)chart.Series;
		ChartSeriesCollection source = (chart.Series as ChartSeriesCollection).Clone(chart.Series.Parent) as ChartSeriesCollection;
		chart.ChartSerieGroupsBeforesorting = from x in source
			where !x.IsFiltered
			group x by (x as ChartSerieImpl).ChartGroup;
		for (int i = 0; i < chartSeriesCollection.Count; i++)
		{
			(chartSeriesCollection[i] as ChartSerieImpl).ExistingOrder = i;
		}
		chartSeriesCollection.ResortSeries(dictSeriesAxis, list);
		if (chart.Series.Count > 0 && chart.Series[0].SerieType == OfficeChartType.Bubble)
		{
			chart.CheckIsBubble3D();
		}
		if (chart.CommonDataPointsCollection != null && chart.CommonDataPointsCollection.Count > 0)
		{
			ChangeKeyToChartGroup(chart);
		}
		if (chart.Series == null || chart.Series.Count <= 0)
		{
			return;
		}
		for (int j = 0; j < chart.Series.Count; j++)
		{
			ChartSerieImpl chartSerieImpl = chart.Series[j] as ChartSerieImpl;
			if (chartSerieImpl.DropLinesStream != null)
			{
				XmlReader xmlReader = UtilityMethods.CreateReader(chartSerieImpl.DropLinesStream);
				ParseLines(xmlReader, chart, chartSerieImpl, xmlReader.LocalName);
				xmlReader.Dispose();
				chartSerieImpl.DropLinesStream.Dispose();
				chartSerieImpl.DropLinesStream = null;
			}
		}
	}

	private void ParsePlotAreaAxes(XmlReader reader, ChartImpl chart, RelationCollection relations, Excel2007Parser parser)
	{
		ChartAxisParser chartAxisParser = new ChartAxisParser(m_book);
		_ = chart.PlotArea;
		_ = chart.DataHolder.ParentHolder;
		new Dictionary<int, int>();
		int num = 0;
		OfficeChartType officeChartType = chart.ChartType;
		int num2 = 0;
		bool isBarchart = false;
		string axisId = string.Empty;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "valAx":
				{
					bool flag = num <= 1;
					chart.CreateNecessaryAxes(flag);
					ChartValueAxisImpl valueAxis = (ChartValueAxisImpl)(flag ? chart.PrimaryValueAxis : chart.SecondaryValueAxis);
					if (num > num2 * 2 - 1 && officeChartType == OfficeChartType.Scatter_Markers)
					{
						officeChartType = OfficeChartType.Combination_Chart;
					}
					chartAxisParser.m_BarAxisId = axisId;
					chartAxisParser.ParseValueAxis(reader, valueAxis, relations, officeChartType, parser);
					num++;
					break;
				}
				case "serAx":
				{
					chart.CreateNecessaryAxes(bPrimary: true);
					ChartSeriesAxisImpl chartSeriesAxisImpl = (ChartSeriesAxisImpl)chart.PrimarySerieAxis;
					if (chartSeriesAxisImpl == null)
					{
						chartSeriesAxisImpl = chart.CreatePrimarySeriesAxis();
					}
					chartAxisParser.ParseSeriesAxis(reader, chartSeriesAxisImpl, relations, officeChartType, parser);
					break;
				}
				case "catAx":
				{
					bool flag = num <= 1;
					chart.CreateNecessaryAxes(flag);
					ChartCategoryAxisImpl axis2 = (ChartCategoryAxisImpl)((num <= 1) ? chart.PrimaryCategoryAxis : chart.SecondaryCategoryAxis);
					chartAxisParser.m_BarAxisId = axisId;
					chartAxisParser.m_isBarchart = isBarchart;
					chartAxisParser.ParseCategoryAxis(reader, axis2, relations, officeChartType, parser);
					num++;
					break;
				}
				case "dateAx":
				{
					bool flag = num <= 1;
					chart.CreateNecessaryAxes(flag);
					ChartCategoryAxisImpl axis = (ChartCategoryAxisImpl)(flag ? chart.PrimaryCategoryAxis : chart.SecondaryCategoryAxis);
					chartAxisParser.ParseDateAxis(reader, axis, relations, officeChartType, parser);
					num++;
					break;
				}
				case "bubbleChart":
					officeChartType = OfficeChartType.Bubble;
					reader.Skip();
					break;
				case "scatterChart":
					officeChartType = OfficeChartType.Scatter_Markers;
					num2++;
					reader.Skip();
					break;
				case "barChart":
					isBarchart = ParseBarDir(reader, ref axisId);
					reader.Skip();
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

	private bool ParseBarDir(XmlReader reader, ref string axisId)
	{
		bool result = false;
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName = reader.LocalName;
				if (!(localName == "barDir"))
				{
					if (localName == "axId")
					{
						axisId += ChartParserCommon.ParseValueTag(reader);
					}
					else
					{
						reader.Skip();
					}
				}
				else if (ChartParserCommon.ParseValueTag(reader).Contains("bar"))
				{
					result = true;
				}
			}
			else
			{
				reader.Skip();
			}
		}
		return result;
	}

	private void ParseBarChart(XmlReader reader, ChartImpl chart, RelationCollection relations, Dictionary<int, int> dictSeriesAxis)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "barChart")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		List<ChartSerieImpl> lstSeries = new List<ChartSerieImpl>();
		string shape = null;
		IOfficeChartSerie officeChartSerie = ParseBarChartShared(reader, chart, relations, is3D: false, lstSeries, out shape);
		int? num = null;
		int? num2 = null;
		bool secondary = false;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "gapWidth":
				{
					string s = ChartParserCommon.ParseValueTag(reader);
					num = int.Parse(s);
					if (officeChartSerie != null)
					{
						(officeChartSerie as ChartSerieImpl).GapWidth = num.Value;
						(officeChartSerie as ChartSerieImpl).ShowGapWidth = true;
					}
					chart.GapWidth = int.Parse(s);
					break;
				}
				case "overlap":
				{
					string s2 = ChartParserCommon.ParseValueTag(reader);
					num2 = int.Parse(s2);
					if (officeChartSerie != null)
					{
						(officeChartSerie as ChartSerieImpl).Overlap = num2.Value;
					}
					chart.OverLap = int.Parse(s2);
					if (num2 == 100)
					{
						num2 = -65436;
					}
					break;
				}
				case "serLines":
					ParseLines(reader, chart, officeChartSerie as ChartSerieImpl, reader.LocalName);
					break;
				case "axId":
					if (ParseAxisId(reader, lstSeries, dictSeriesAxis))
					{
						secondary = true;
					}
					break;
				case "extLst":
					ParseFilteredSeries(reader, chart, relations, is3D: true, officeChartSerie.SerieType, secondary);
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
		IOfficeChartFormat officeChartFormat = officeChartSerie?.SerieFormat.CommonSerieOptions;
		if (num.HasValue && officeChartSerie != null)
		{
			officeChartFormat.GapWidth = num.Value;
		}
		if (num2.HasValue && officeChartSerie != null)
		{
			officeChartFormat.Overlap = num2.Value;
		}
		reader.Read();
	}

	private IOfficeChartSerie ParseFilteredSeries(XmlReader reader, ChartImpl chart, RelationCollection relations, bool is3D, OfficeChartType SeriesType, bool secondary)
	{
		FileDataHolder parentHolder = chart.DataHolder.ParentHolder;
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		_ = string.Empty;
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		_ = string.Empty;
		if (reader.LocalName != "extLst")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		ChartSerieImpl chartSerieImpl = null;
		if (reader.LocalName == "ext")
		{
			reader.Read();
			while (reader.LocalName != "extLst" && reader.NodeType != XmlNodeType.EndElement)
			{
				reader.Read();
				if (reader.LocalName == "ser")
				{
					switch (SeriesType)
					{
					case OfficeChartType.Area:
					case OfficeChartType.Area_Stacked:
					case OfficeChartType.Area_Stacked_100:
					case OfficeChartType.Area_3D:
					case OfficeChartType.Area_Stacked_3D:
					case OfficeChartType.Area_Stacked_100_3D:
						chartSerieImpl = ParseAreaSeries(reader, chart, SeriesType, relations, !secondary);
						break;
					case OfficeChartType.Line:
					case OfficeChartType.Line_Stacked:
					case OfficeChartType.Line_Stacked_100:
					case OfficeChartType.Line_Markers:
					case OfficeChartType.Line_Markers_Stacked:
					case OfficeChartType.Line_Markers_Stacked_100:
					case OfficeChartType.Line_3D:
						chartSerieImpl = ParseLineSeries(reader, chart, SeriesType, relations, parentHolder.Parser);
						break;
					case OfficeChartType.Pie:
					case OfficeChartType.Pie_3D:
					case OfficeChartType.PieOfPie:
					case OfficeChartType.Pie_Exploded:
					case OfficeChartType.Pie_Exploded_3D:
					case OfficeChartType.Pie_Bar:
						chartSerieImpl = ParsePieSeries(reader, chart, SeriesType, relations);
						break;
					case OfficeChartType.Radar:
					case OfficeChartType.Radar_Markers:
					case OfficeChartType.Radar_Filled:
						chartSerieImpl = ParseRadarSeries(reader, chart, SeriesType, relations, parentHolder.Parser);
						break;
					case OfficeChartType.Scatter_Markers:
					case OfficeChartType.Scatter_SmoothedLine_Markers:
					case OfficeChartType.Scatter_SmoothedLine:
					case OfficeChartType.Scatter_Line_Markers:
					case OfficeChartType.Scatter_Line:
						chartSerieImpl = ParseScatterSeries(reader, chart, SeriesType, relations, parentHolder.Parser);
						break;
					case OfficeChartType.Surface_3D:
					case OfficeChartType.Surface_NoColor_3D:
					case OfficeChartType.Surface_Contour:
					case OfficeChartType.Surface_NoColor_Contour:
						chartSerieImpl = ParseSurfaceSeries(reader, chart, SeriesType, relations);
						break;
					case OfficeChartType.Bubble:
					case OfficeChartType.Bubble_3D:
						chartSerieImpl = ParseBubbleSeries(reader, chart, relations);
						break;
					default:
						chartSerieImpl = ParseBarSeries(reader, chart, SeriesType, relations);
						break;
					}
				}
				if (secondary && chartSerieImpl.ParentChart.Series.Count > 1)
				{
					chartSerieImpl.UsePrimaryAxis = !secondary;
				}
				chartSerieImpl.IsFiltered = true;
				reader.Skip();
			}
			reader.Read();
			reader.Read();
		}
		return chartSerieImpl;
	}

	private bool ParseAxisId(XmlReader reader, List<ChartSerieImpl> lstSeries, Dictionary<int, int> dictSeriesAxis)
	{
		int num = ChartParserCommon.ParseIntValueTag(reader);
		int count = lstSeries.Count;
		bool result = false;
		if (count > 0)
		{
			ChartImpl parentChart = lstSeries[0].ParentChart;
			int axisId = (parentChart.PrimaryValueAxis as ChartAxisImpl).AxisId;
			int axisId2 = (parentChart.PrimaryCategoryAxis as ChartAxisImpl).AxisId;
			result = num != axisId && num != axisId2;
			for (int i = 0; i < count; i++)
			{
				int index = lstSeries[i].Index;
				dictSeriesAxis[index] = num;
			}
		}
		lstSeries.Clear();
		return result;
	}

	public void ParseBar3DChart(XmlReader reader, ChartImpl chart, RelationCollection relations, Dictionary<int, int> dictSeriesAxis)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "bar3DChart")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		List<ChartSerieImpl> lstSeries = new List<ChartSerieImpl>();
		string shape = null;
		ChartSerieImpl chartSerieImpl = ParseBarChartShared(reader, chart, relations, is3D: true, lstSeries, out shape);
		int? num = null;
		if (shape != null && chartSerieImpl != null)
		{
			ParseBarShape(shape, chartSerieImpl.GetCommonSerieFormat().SerieDataFormat);
		}
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element && chartSerieImpl != null)
			{
				switch (reader.LocalName)
				{
				case "gapWidth":
				{
					string s = ChartParserCommon.ParseValueTag(reader);
					num = int.Parse(s);
					chart.GapWidth = num.Value;
					chart.ShowGapWidth = true;
					break;
				}
				case "gapDepth":
					if (reader.MoveToAttribute("val"))
					{
						chart.GapDepth = ChartParserCommon.ParseIntValueTag(reader);
					}
					else
					{
						reader.Skip();
					}
					break;
				case "axId":
					ParseAxisId(reader, lstSeries, dictSeriesAxis);
					break;
				case "extLst":
					ParseFilteredSeries(reader, chart, relations, is3D: true, chartSerieImpl.SerieType, secondary: false);
					break;
				case "shape":
					shape = ChartParserCommon.ParseValueTag(reader);
					ParseBarShape(shape, chart.ChartFormat.SerieDataFormat);
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
		if (num.HasValue)
		{
			chartSerieImpl.SerieFormat.CommonSerieOptions.GapWidth = num.Value;
		}
		foreach (ChartSerieImpl item in (ChartSeriesCollection)chart.Series)
		{
			if (!item.HasColumnShape)
			{
				item.SerieFormat.BarShapeBase = item.GetCommonSerieFormat().SerieDataFormat.BarShapeBase;
				item.SerieFormat.BarShapeTop = item.GetCommonSerieFormat().SerieDataFormat.BarShapeTop;
			}
		}
		reader.Read();
	}

	private void ParseBarShape(XmlReader reader, ChartSerieImpl firstSeries)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (firstSeries == null)
		{
			throw new ArgumentNullException("firstSeries");
		}
		string value = ChartParserCommon.ParseValueTag(reader);
		ParseBarShape(value, firstSeries.SerieFormat);
	}

	private void ParseBarShape(string value, IOfficeChartSerieDataFormat dataFormat)
	{
		if (value != null)
		{
			if (dataFormat == null)
			{
				throw new ArgumentNullException("dataFormat");
			}
			switch (value)
			{
			case "cone":
				dataFormat.BarShapeTop = OfficeTopFormat.Sharp;
				dataFormat.BarShapeBase = OfficeBaseFormat.Circle;
				break;
			case "pyramid":
				dataFormat.BarShapeTop = OfficeTopFormat.Sharp;
				dataFormat.BarShapeBase = OfficeBaseFormat.Rectangle;
				break;
			case "coneToMax":
				dataFormat.BarShapeTop = OfficeTopFormat.Trunc;
				dataFormat.BarShapeBase = OfficeBaseFormat.Circle;
				break;
			case "pyramidToMax":
				dataFormat.BarShapeTop = OfficeTopFormat.Trunc;
				dataFormat.BarShapeBase = OfficeBaseFormat.Rectangle;
				break;
			case "cylinder":
				dataFormat.BarShapeTop = OfficeTopFormat.Straight;
				dataFormat.BarShapeBase = OfficeBaseFormat.Circle;
				break;
			case "box":
				dataFormat.BarShapeTop = OfficeTopFormat.Straight;
				dataFormat.BarShapeBase = OfficeBaseFormat.Rectangle;
				break;
			default:
				throw new XmlException();
			}
		}
	}

	private ChartSerieImpl ParseBarChartShared(XmlReader reader, ChartImpl chart, RelationCollection relations, bool is3D, List<ChartSerieImpl> lstSeries, out string shape)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		bool flag = true;
		ChartSerieImpl series = null;
		string text = null;
		bool isVaryColor = _appVersion == 0.0 || _appVersion > 12.0;
		string text2 = null;
		shape = null;
		MemoryStream memoryStream = new MemoryStream();
		XmlWriter xmlWriter = UtilityMethods.CreateWriter(memoryStream, Encoding.UTF8);
		xmlWriter.WriteStartElement("root");
		Stream stream = null;
		int? num = null;
		while (flag)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "barDir":
					text = ChartParserCommon.ParseValueTag(reader);
					break;
				case "grouping":
					text2 = ChartParserCommon.ParseValueTag(reader);
					break;
				case "varyColors":
					isVaryColor = ChartParserCommon.ParseBoolValueTag(reader);
					break;
				case "shape":
					shape = ChartParserCommon.ParseValueTag(reader);
					break;
				case "ser":
					xmlWriter.WriteNode(reader, defattr: false);
					xmlWriter.Flush();
					break;
				case "dLbls":
					if (!reader.IsEmptyElement)
					{
						stream = ShapeParser.ReadNodeAsStream(reader);
					}
					else
					{
						reader.Skip();
					}
					break;
				case "gapWidth":
					if (memoryStream.Length == 0L)
					{
						string s = ChartParserCommon.ParseValueTag(reader);
						chart.GapWidth = new int?(int.Parse(s)).Value;
						chart.ShowGapWidth = true;
					}
					else
					{
						flag = false;
					}
					break;
				case "overlap":
					reader.Skip();
					break;
				default:
					if (!is3D && memoryStream.Length == 0L)
					{
						OfficeChartType pivotBarSeriesType = GetPivotBarSeriesType(text, text2, shape, is3D);
						ParseFilterSecondaryAxis(reader, pivotBarSeriesType, is3D, lstSeries, chart, relations, ref series);
					}
					flag = false;
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
		memoryStream.Position = 0L;
		XmlReader xmlReader = UtilityMethods.CreateReader(memoryStream);
		if (!xmlReader.IsEmptyElement)
		{
			xmlReader.Read();
			ParseSeries(xmlReader, text, text2, shape, is3D, lstSeries, chart, relations, ref series);
			if (series != null)
			{
				series.Grouping = text2;
			}
			series.SerieFormat.CommonSerieOptions.IsVaryColor = isVaryColor;
		}
		xmlReader.Dispose();
		xmlWriter.Dispose();
		if (stream != null && series != null)
		{
			stream.Position = 0L;
			XmlReader xmlReader2 = UtilityMethods.CreateReader(stream);
			ParseDataLabels(xmlReader2, chart, relations, series.Number);
			xmlReader2.Dispose();
		}
		return series;
	}

	private ChartSerieImpl ParseFilterSecondaryAxis(XmlReader reader, OfficeChartType seriesType, bool is3D, List<ChartSerieImpl> lstSeries, ChartImpl chart, RelationCollection relations, ref ChartSerieImpl series)
	{
		ChartSerieImpl result = null;
		int? num = null;
		new Dictionary<int, int>();
		IOfficeChartSerie officeChartSerie = null;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType != XmlNodeType.Element)
			{
				continue;
			}
			switch (reader.LocalName)
			{
			case "gapWidth":
			{
				string s = ChartParserCommon.ParseValueTag(reader);
				chart.GapWidth = new int?(int.Parse(s)).Value;
				chart.ShowGapWidth = true;
				break;
			}
			case "overlap":
			{
				string s2 = ChartParserCommon.ParseValueTag(reader);
				chart.OverLap = int.Parse(s2);
				break;
			}
			case "axId":
				reader.Skip();
				break;
			case "extLst":
				officeChartSerie = ParseFilteredSeries(reader, chart, relations, is3D: true, seriesType, secondary: true);
				if (chart.Series.Count > 1)
				{
					officeChartSerie.UsePrimaryAxis = false;
				}
				break;
			default:
				reader.Skip();
				break;
			}
		}
		series = officeChartSerie as ChartSerieImpl;
		return result;
	}

	private void ParseSeries(XmlReader reader, string strDirection, string strGrouping, string shape, bool is3D, List<ChartSerieImpl> lstSeries, ChartImpl chart, RelationCollection relations, ref ChartSerieImpl series)
	{
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "ser")
			{
				OfficeChartType pivotBarSeriesType = GetPivotBarSeriesType(strDirection, strGrouping, shape, is3D);
				ChartSerieImpl chartSerieImpl = ParseBarSeries(reader, chart, pivotBarSeriesType, relations);
				if (series == null)
				{
					series = chartSerieImpl;
				}
				lstSeries.Add(chartSerieImpl);
			}
			else
			{
				reader.Read();
			}
		}
	}

	private void FindFilter(IOfficeChartCategories categories, string filteredcategory, string fullreference, IOfficeChartSerie series1, bool isseries)
	{
		IRange range = FindRange(series1, fullreference);
		int num = (isseries ? range.Column : range.Row);
		int num2 = (isseries ? range.LastColumn : range.LastRow);
		filteredcategory = filteredcategory.Trim('(');
		filteredcategory = filteredcategory.Trim(')');
		string[] array = filteredcategory.Split(',');
		int[] array2 = new int[range.Count];
		int num3 = 0;
		for (int i = 0; i < array.Length; i++)
		{
			IRange range2 = FindRange(series1, array[i]);
			int num4 = (isseries ? range2.Column : range2.Row);
			int num5 = (isseries ? range2.LastColumn : range2.LastRow);
			for (int j = num4; j <= num5; j++)
			{
				array2[num3] = j;
				num3++;
			}
		}
		num3 = 0;
		int num6 = 0;
		for (int k = num; k <= num2; k++)
		{
			if (k != array2[num3] && array2[num3] != 0)
			{
				categories[num6].IsFiltered = true;
			}
			if (k == array2[num3] && array2[num3] != 0)
			{
				num3++;
			}
			else
			{
				categories[num6].IsFiltered = true;
			}
			num6++;
		}
	}

	private IRange FindRange(IOfficeChartSerie series1, string strValue)
	{
		IRange range = null;
		bool isNumReference = true;
		bool isStringReference = false;
		bool isMultiReference = false;
		ChartSerieImpl chartSerieImpl = series1 as ChartSerieImpl;
		_ = chartSerieImpl.ParentBook;
		if (strValue != null)
		{
			WorkbookImpl parentBook = chartSerieImpl.ParentBook;
			FormulaUtil formulaUtil = parentBook.DataHolder.Parser.FormulaUtil;
			if (!ChartImpl.TryAndModifyToValidFormula(strValue))
			{
				return null;
			}
			range = (formulaUtil.ParseString(strValue)[0] as IRangeGetter).GetRange(parentBook, parentBook.Worksheets[0]);
			if (range != null)
			{
				if (range is ExternalRange)
				{
					(range as ExternalRange).IsNumReference = isNumReference;
					(range as ExternalRange).IsStringReference = isStringReference;
					(range as ExternalRange).IsMultiReference = isMultiReference;
				}
				else if (range is RangeImpl)
				{
					(range as RangeImpl).IsNumReference = isNumReference;
					(range as RangeImpl).IsStringReference = isStringReference;
					(range as RangeImpl).IsMultiReference = isMultiReference;
				}
				else if (range is NameImpl)
				{
					(range as NameImpl).IsNumReference = isNumReference;
					(range as NameImpl).IsStringReference = isStringReference;
					(range as NameImpl).IsMultiReference = isMultiReference;
				}
			}
		}
		return range;
	}

	public IRange GetSerieOrAxisRange(IRange range, bool bIsInRow, out IRange serieRange, int CategoryLabelCount)
	{
		if (range == null)
		{
			throw new ArgumentNullException("range");
		}
		int num = (bIsInRow ? range.Row : range.Column);
		int num2 = (bIsInRow ? range.LastRow : range.LastColumn);
		int num3 = (bIsInRow ? range.Column : range.Row);
		int num4 = (bIsInRow ? range.LastColumn : range.LastRow);
		int num5 = -1;
		bool flag = false;
		for (int i = num3; i < num4; i++)
		{
			if (flag && CategoryLabelCount == 0)
			{
				break;
			}
			IRange range2 = (bIsInRow ? range[num2, i] : range[i, num2]);
			flag = range2.HasNumber || range2.IsBlank || range2.HasFormula;
			if (!flag)
			{
				num5 = i;
			}
			if (CategoryLabelCount != 0)
			{
				CategoryLabelCount--;
			}
		}
		if (num5 == -1)
		{
			serieRange = range;
			return null;
		}
		IRange range3 = (bIsInRow ? range[num, num3, num2, num5] : range[num3, num, num5, num2]);
		serieRange = (bIsInRow ? range[range.Row, range3.LastColumn + 1, range.LastRow, range.LastColumn] : range[range3.LastRow + 1, range.Column, range.LastRow, range.LastColumn]);
		return range3;
	}

	private bool DetectIsInRow(IRange range)
	{
		if (range == null)
		{
			return true;
		}
		int num = range.LastRow - range.Row;
		int num2 = range.LastColumn - range.Column;
		return num <= num2;
	}

	private OfficeChartType GetBarSeriesType(string direction, string grouping, bool is3D, string shape)
	{
		string text = null;
		if (!is3D)
		{
			text = ((direction == "bar") ? "Bar" : "Column");
		}
		else
		{
			text = ((shape == "box") ? "Column" : "Cone");
			if (direction == "bar")
			{
				text += "_Bar";
			}
		}
		return (OfficeChartType)Enum.Parse(value: grouping switch
		{
			"clustered" => text + "_Clustered", 
			"percentStacked" => text + "_Stacked_100", 
			"stacked" => text + "_Stacked", 
			_ => (!is3D) ? (text + "_Clustered") : (text + "_Clustered_3D"), 
		}, enumType: typeof(OfficeChartType), ignoreCase: false);
	}

	private OfficeChartType GetPivotBarSeriesType(string direction, string grouping, string shape, bool is3D)
	{
		string text = null;
		string[] array = new string[3] { "Cone", "Cylinder", "Pyramid" };
		if (shape != null)
		{
			if (Array.IndexOf(array, shape) == -1)
			{
				text = ((direction == "bar") ? "Bar" : "Column");
			}
		}
		else
		{
			text = ((!(direction == "bar")) ? ((direction == "col" && shape != null) ? shape : "Column") : ((shape == null) ? "Bar" : (shape + "_Bar")));
		}
		return (OfficeChartType)Enum.Parse(value: grouping switch
		{
			"clustered" => (!is3D || (shape != null && Array.IndexOf(array, shape) != -1)) ? (text + "_Clustered") : (text + "_Clustered_3D"), 
			"standard" => (!is3D || (shape != null && Array.IndexOf(array, shape) != -1)) ? (text + "_Clustered_3D") : (text + "_3D"), 
			"percentStacked" => (!is3D || (shape != null && Array.IndexOf(array, shape) != -1)) ? (text + "_Stacked_100") : (text + "_Stacked_100_3D"), 
			"stacked" => (!is3D || (shape != null && Array.IndexOf(array, shape) != -1)) ? (text + "_Stacked") : (text + "_Stacked_3D"), 
			_ => (!is3D) ? (text + "_Clustered") : (text + "_Clustered_3D"), 
		}, enumType: typeof(OfficeChartType), ignoreCase: true);
	}

	private OfficeChartType GetAreaSeriesType(string grouping, bool is3D)
	{
		string text = "Area";
		switch (grouping)
		{
		case "percentStacked":
			text += "_Stacked_100";
			break;
		case "stacked":
			text += "_Stacked";
			break;
		}
		if (is3D)
		{
			text += "_3D";
		}
		return (OfficeChartType)Enum.Parse(typeof(OfficeChartType), text, ignoreCase: false);
	}

	private OfficeChartType GetLineSeriesType(string grouping, bool is3D)
	{
		string text = "Line";
		if (!is3D)
		{
			if (!(grouping == "percentStacked"))
			{
				if (grouping == "stacked")
				{
					text += "_Stacked";
				}
			}
			else
			{
				text += "_Stacked_100";
			}
			return (OfficeChartType)Enum.Parse(typeof(OfficeChartType), text, ignoreCase: false);
		}
		return OfficeChartType.Line_3D;
	}

	private void ParseArea3DChart(XmlReader reader, ChartImpl chart, RelationCollection relations, Dictionary<int, int> dictSeriesAxis)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "area3DChart")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		List<ChartSerieImpl> lstSeries = new List<ChartSerieImpl>();
		ParseAreaChartCommon(reader, chart, b3D: true, relations, lstSeries, isPrimary: true);
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName = reader.LocalName;
				if (!(localName == "axId"))
				{
					if (localName == "extLst")
					{
						ParseFilteredSeries(reader, chart, relations, is3D: true, chart.ChartType, secondary: false);
					}
					else
					{
						reader.Skip();
					}
				}
				else
				{
					ParseAxisId(reader, lstSeries, dictSeriesAxis);
				}
			}
			else
			{
				reader.Read();
			}
		}
		reader.Read();
	}

	private void ParseAreaChart(XmlReader reader, ChartImpl chart, RelationCollection relations, Dictionary<int, int> dictSeriesAxis)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "areaChart")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		bool secondary = false;
		List<ChartSerieImpl> lstSeries = new List<ChartSerieImpl>();
		bool axisType = GetAxisType(chart, ref reader);
		ParseAreaChartCommon(reader, chart, b3D: false, relations, lstSeries, !axisType);
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.LocalName == "axId")
			{
				if (ParseAxisId(reader, lstSeries, dictSeriesAxis))
				{
					secondary = true;
				}
			}
			else if (reader.LocalName == "extLst")
			{
				if (chart.Series.Count > 0 && chart.Series[0] != null)
				{
					ParseFilteredSeries(reader, chart, relations, is3D: true, chart.Series[0].SerieType, secondary);
				}
			}
			else
			{
				reader.Skip();
			}
		}
	}

	private bool GetAxisType(ChartImpl chart, ref XmlReader reader)
	{
		MemoryStream memoryStream = new MemoryStream();
		XmlWriter xmlWriter = UtilityMethods.CreateWriter(memoryStream, Encoding.UTF8);
		xmlWriter.WriteStartElement("root");
		bool result = false;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.LocalName == "axId")
			{
				string localName = reader.LocalName;
				int num = ChartParserCommon.ParseIntValueTag(reader);
				int axisId = (chart.PrimaryValueAxis as ChartAxisImpl).AxisId;
				int axisId2 = (chart.PrimaryCategoryAxis as ChartAxisImpl).AxisId;
				result = num != axisId && num != axisId2;
				ChartSerializatorCommon.SerializeValueTag(xmlWriter, localName, num.ToString());
			}
			else if (reader.NodeType == XmlNodeType.Element)
			{
				xmlWriter.WriteNode(reader, defattr: false);
			}
			else
			{
				reader.Skip();
			}
		}
		xmlWriter.WriteEndElement();
		xmlWriter.Flush();
		reader.Read();
		memoryStream.Position = 0L;
		reader = UtilityMethods.CreateReader(memoryStream);
		reader.Read();
		return result;
	}

	private void ParseAreaChartCommon(XmlReader reader, ChartImpl chart, bool b3D, RelationCollection relations, List<ChartSerieImpl> lstSeries, bool isPrimary)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		bool flag = true;
		string grouping = null;
		ChartSerieImpl series = null;
		while (reader.NodeType != XmlNodeType.EndElement && flag)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				OfficeChartType areaSeriesType;
				switch (reader.LocalName)
				{
				case "grouping":
					grouping = ChartParserCommon.ParseValueTag(reader);
					continue;
				case "varyColors":
					ChartParserCommon.ParseBoolValueTag(reader);
					continue;
				case "ser":
					areaSeriesType = GetAreaSeriesType(grouping, b3D);
					series = ParseAreaSeries(reader, chart, areaSeriesType, relations, isPrimary);
					lstSeries.Add(series);
					continue;
				case "dLbls":
					if (!reader.IsEmptyElement && series != null)
					{
						ParseDataLabels(reader, chart, relations, series.Number);
					}
					else
					{
						reader.Skip();
					}
					continue;
				case "dropLines":
					ParseLines(reader, chart, series, reader.LocalName);
					continue;
				}
				if (lstSeries.Count != 0)
				{
					flag = false;
					continue;
				}
				areaSeriesType = GetLineSeriesType(grouping, b3D);
				ParseFilterSecondaryAxis(reader, areaSeriesType, b3D, lstSeries, chart, relations, ref series);
				flag = false;
			}
			else
			{
				reader.Read();
			}
		}
	}

	private ChartSerieImpl ParseLineChartCommon(XmlReader reader, ChartImpl chart, bool is3D, RelationCollection relations, List<ChartSerieImpl> lstSeries, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		bool flag = true;
		string grouping = null;
		ChartSerieImpl chartSerieImpl = null;
		ChartSerieImpl series = null;
		OfficeChartType officeChartType = OfficeChartType.Line;
		while (reader.NodeType != XmlNodeType.EndElement && flag)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "grouping":
					grouping = ChartParserCommon.ParseValueTag(reader);
					continue;
				case "varyColors":
					ChartParserCommon.ParseBoolValueTag(reader);
					continue;
				case "ser":
					officeChartType = GetLineSeriesType(grouping, is3D);
					series = ParseLineSeries(reader, chart, officeChartType, relations, parser);
					lstSeries.Add(series);
					if (chartSerieImpl == null)
					{
						chartSerieImpl = series;
					}
					continue;
				case "dLbls":
					if (!reader.IsEmptyElement && series != null)
					{
						ParseDataLabels(reader, chart, relations, series.Number);
					}
					else
					{
						reader.Skip();
					}
					continue;
				case "dropLines":
					if (chartSerieImpl != null)
					{
						chartSerieImpl.DropLinesStream = ShapeParser.ReadNodeAsStream(reader, writeNamespaces: true);
					}
					else
					{
						reader.Skip();
					}
					continue;
				}
				if (lstSeries.Count != 0)
				{
					flag = false;
					continue;
				}
				if (reader.LocalName == "axId")
				{
					reader.Skip();
					continue;
				}
				officeChartType = GetLineSeriesType(grouping, is3D);
				ParseFilterSecondaryAxis(reader, officeChartType, is3D, lstSeries, chart, relations, ref series);
				flag = false;
			}
			else
			{
				reader.Skip();
			}
		}
		return chartSerieImpl;
	}

	private void ParseLine3DChart(XmlReader reader, ChartImpl chart, RelationCollection relations, Dictionary<int, int> dictSeriesAxis, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "line3DChart")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		List<ChartSerieImpl> lstSeries = new List<ChartSerieImpl>();
		ParseLineChartCommon(reader, chart, is3D: true, relations, lstSeries, parser);
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.LocalName == "axId")
			{
				ParseAxisId(reader, lstSeries, dictSeriesAxis);
			}
			else if (reader.LocalName == "extLst")
			{
				ParseFilteredSeries(reader, chart, relations, is3D: true, OfficeChartType.Line_3D, secondary: false);
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Read();
	}

	private void ParseLineChart(XmlReader reader, ChartImpl chart, RelationCollection relations, Dictionary<int, int> dictSeriesAxis, Excel2007Parser parser, List<int> markerArray)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		chart.LineChartCount++;
		reader.Read();
		List<ChartSerieImpl> lstSeries = new List<ChartSerieImpl>();
		ChartSerieImpl chartSerieImpl = ParseLineChartCommon(reader, chart, is3D: false, relations, lstSeries, parser);
		bool secondary = false;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "upDownBars":
					ParseUpDownBars(reader, chartSerieImpl, relations);
					break;
				case "marker":
					if (ChartParserCommon.ParseBoolValueTag(reader) && chartSerieImpl != null)
					{
						_ = ((ChartSerieDataFormatImpl)chartSerieImpl.SerieFormat).MarkerFormat;
						if (!chartSerieImpl.SerieType.ToString().Contains("_Markers"))
						{
							markerArray.Add(chartSerieImpl.Index);
						}
					}
					break;
				case "hiLowLines":
				case "dropLines":
					ParseLines(reader, chart, chartSerieImpl, reader.LocalName);
					break;
				case "axId":
					if (ParseAxisId(reader, lstSeries, dictSeriesAxis))
					{
						secondary = true;
					}
					break;
				case "extLst":
					ParseFilteredSeries(reader, chart, relations, is3D: true, chartSerieImpl.SerieType, secondary);
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

	private void ParseBubbleChart(XmlReader reader, ChartImpl chart, RelationCollection relations, Dictionary<int, int> dictSeriesAxis)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "bubbleChart")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		ChartSerieImpl chartSerieImpl = null;
		List<ChartSerieImpl> list = new List<ChartSerieImpl>();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "varyColors":
					ChartParserCommon.ParseBoolValueTag(reader);
					break;
				case "ser":
					chartSerieImpl = ParseBubbleSeries(reader, chart, relations);
					list.Add(chartSerieImpl);
					break;
				case "bubbleScale":
				{
					int bubbleScale = ChartParserCommon.ParseIntValueTag(reader);
					chartSerieImpl.SerieFormat.CommonSerieOptions.BubbleScale = bubbleScale;
					break;
				}
				case "showNegBubbles":
				{
					bool showNegativeBubbles = ChartParserCommon.ParseBoolValueTag(reader);
					chartSerieImpl.SerieFormat.CommonSerieOptions.ShowNegativeBubbles = showNegativeBubbles;
					break;
				}
				case "sizeRepresents":
				{
					string text = ChartParserCommon.ParseValueTag(reader);
					chartSerieImpl.SerieFormat.CommonSerieOptions.SizeRepresents = ((text == "area") ? ChartBubbleSize.Area : ChartBubbleSize.Width);
					break;
				}
				case "axId":
					ParseAxisId(reader, list, dictSeriesAxis);
					break;
				case "extLst":
					ParseFilteredSeries(reader, chart, relations, is3D: false, chartSerieImpl.SerieType, secondary: false);
					break;
				case "dLbls":
					if (!reader.IsEmptyElement && chartSerieImpl != null)
					{
						ParseDataLabels(reader, chart, relations, chartSerieImpl.Number);
					}
					else
					{
						reader.Skip();
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
		reader.Read();
	}

	private void ParseSurfaceChart(XmlReader reader, ChartImpl chart, RelationCollection relations, Dictionary<int, int> dictSeriesAxis)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		bool is3D;
		if (reader.LocalName == "surfaceChart")
		{
			is3D = false;
		}
		else
		{
			if (!(reader.LocalName == "surface3DChart"))
			{
				throw new XmlException("Unexpected xml tag.");
			}
			is3D = true;
		}
		reader.Read();
		List<ChartSerieImpl> lstSeries = new List<ChartSerieImpl>();
		ParseSurfaceCommon(reader, chart, is3D, relations, lstSeries);
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.LocalName == "axId")
			{
				ParseAxisId(reader, lstSeries, dictSeriesAxis);
			}
			else if (reader.LocalName == "extLst")
			{
				ParseFilteredSeries(reader, chart, relations, is3D, chart.ChartType, secondary: false);
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Skip();
	}

	private void ParseSurfaceCommon(XmlReader reader, ChartImpl chart, bool is3D, RelationCollection relations, List<ChartSerieImpl> lstSeries)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		OfficeChartType seriesType = OfficeChartType.Surface_3D;
		bool flag = true;
		bool bWireframe = false;
		while (reader.NodeType != XmlNodeType.EndElement && flag)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "wireframe":
					bWireframe = ChartParserCommon.ParseBoolValueTag(reader);
					break;
				case "ser":
				{
					seriesType = GetSurfaceSeriesType(bWireframe, is3D);
					ChartSerieImpl item = ParseSurfaceSeries(reader, chart, seriesType, relations);
					lstSeries.Add(item);
					break;
				}
				case "bandFmts":
					ParseBandFormats(reader, chart);
					break;
				case "extLst":
					ParseFilteredSeries(reader, chart, relations, is3D: true, seriesType, secondary: false);
					break;
				default:
					flag = false;
					break;
				}
			}
			else
			{
				reader.Skip();
			}
		}
	}

	private void ParseBandFormats(XmlReader reader, ChartImpl chart)
	{
		chart.PreservedBandFormats = ShapeParser.ReadNodeAsStream(reader);
	}

	private OfficeChartType GetSurfaceSeriesType(bool bWireframe, bool is3D)
	{
		if (bWireframe)
		{
			return is3D ? OfficeChartType.Surface_NoColor_3D : OfficeChartType.Surface_NoColor_Contour;
		}
		return is3D ? OfficeChartType.Surface_3D : OfficeChartType.Surface_Contour;
	}

	private void ParseRadarChart(XmlReader reader, ChartImpl chart, RelationCollection relations, Dictionary<int, int> dictSeriesAxis, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "radarChart")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		OfficeChartType officeChartType = OfficeChartType.Radar;
		bool flag = false;
		List<ChartSerieImpl> list = new List<ChartSerieImpl>();
		bool secondary = false;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "radarStyle":
				{
					string text = ChartParserCommon.ParseValueTag(reader);
					officeChartType = (OfficeChartType)(Excel2007RadarStyle)Enum.Parse(typeof(Excel2007RadarStyle), text, ignoreCase: false);
					chart.RadarStyle = text;
					break;
				}
				case "varyColors":
					flag = ChartParserCommon.ParseBoolValueTag(reader);
					break;
				case "ser":
				{
					if (list.Count == 1 && list[0].SerieType != officeChartType)
					{
						officeChartType = list[0].SerieType;
						chart.m_bIsRadarTypeChanged = true;
					}
					ChartSerieImpl chartSerieImpl = ParseRadarSeries(reader, chart, officeChartType, relations, parser);
					list.Add(chartSerieImpl);
					if (flag)
					{
						chartSerieImpl.SerieFormat.CommonSerieOptions.IsVaryColor = true;
					}
					break;
				}
				case "axId":
					if (ParseAxisId(reader, list, dictSeriesAxis))
					{
						secondary = true;
					}
					break;
				case "extLst":
					ParseFilteredSeries(reader, chart, relations, is3D: false, officeChartType, secondary);
					break;
				case "dLbls":
					if (!reader.IsEmptyElement && list.Count > 0)
					{
						ParseDataLabels(reader, chart, relations, list[0].Number);
					}
					else
					{
						reader.Skip();
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
		reader.Read();
	}

	private void ParseScatterChart(XmlReader reader, ChartImpl chart, RelationCollection relations, Dictionary<int, int> dictSeriesAxis, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "scatterChart")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		OfficeChartType seriesType = OfficeChartType.Scatter_Markers;
		bool flag = true;
		bool secondary = false;
		ChartSerieImpl chartSerieImpl = null;
		List<ChartSerieImpl> list = new List<ChartSerieImpl>();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "scatterStyle":
				{
					string value = ChartParserCommon.ParseValueTag(reader);
					seriesType = (OfficeChartType)(Excel2007ScatterStyle)Enum.Parse(typeof(Excel2007ScatterStyle), value, ignoreCase: false);
					break;
				}
				case "varyColors":
					flag = ChartParserCommon.ParseBoolValueTag(reader);
					break;
				case "ser":
					chartSerieImpl = ParseScatterSeries(reader, chart, seriesType, relations, parser);
					list.Add(chartSerieImpl);
					if (flag)
					{
						chartSerieImpl.SerieFormat.CommonSerieOptions.IsVaryColor = true;
					}
					break;
				case "upDownBars":
					ParseUpDownBars(reader, chartSerieImpl, relations);
					break;
				case "axId":
					if (ParseAxisId(reader, list, dictSeriesAxis))
					{
						secondary = true;
					}
					break;
				case "extLst":
					ParseFilteredSeries(reader, chart, relations, is3D: false, seriesType, secondary);
					break;
				case "dLbls":
					if (!reader.IsEmptyElement && list.Count > 0)
					{
						ParseDataLabels(reader, chart, relations, list[0].Number);
					}
					else
					{
						reader.Skip();
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
		reader.Read();
	}

	private void ParsePieChart(XmlReader reader, ChartImpl chart, RelationCollection relations, Dictionary<int, int> dictSeriesAxis)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "pieChart")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		List<ChartSerieImpl> lstSeries = new List<ChartSerieImpl>();
		ChartSerieImpl chartSerieImpl = ParsePieCommon(reader, chart, OfficeChartType.Pie, relations, lstSeries);
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element && chartSerieImpl != null)
			{
				string localName = reader.LocalName;
				if (!(localName == "firstSliceAng"))
				{
					if (localName == "axId")
					{
						ParseAxisId(reader, lstSeries, dictSeriesAxis);
					}
					else
					{
						reader.Skip();
					}
				}
				else
				{
					chartSerieImpl.SerieFormat.CommonSerieOptions.FirstSliceAngle = ChartParserCommon.ParseIntValueTag(reader);
				}
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Read();
	}

	private void ParsePie3DChart(XmlReader reader, ChartImpl chart, RelationCollection relations, Dictionary<int, int> dictSeriesAxis)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "pie3DChart")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		List<ChartSerieImpl> lstSeries = new List<ChartSerieImpl>();
		ParsePieCommon(reader, chart, OfficeChartType.Pie_3D, relations, lstSeries);
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.LocalName == "axId")
			{
				ParseAxisId(reader, lstSeries, dictSeriesAxis);
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Read();
	}

	private void ParseOfPieChart(XmlReader reader, ChartImpl chart, RelationCollection relations, Dictionary<int, int> dictSeriesAxis)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "ofPieChart")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		OfficeChartType seriesType = OfficeChartType.PieOfPie;
		ChartSerieImpl chartSerieImpl = null;
		List<ChartSerieImpl> lstSeries = new List<ChartSerieImpl>();
		int? num = null;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "ofPieType":
					seriesType = ((ChartParserCommon.ParseValueTag(reader) == "pie") ? OfficeChartType.PieOfPie : OfficeChartType.Pie_Bar);
					break;
				case "varyColors":
				case "ser":
				case "dLbls":
					chartSerieImpl = ParsePieCommon(reader, chart, seriesType, relations, lstSeries);
					break;
				case "gapWidth":
					num = ChartParserCommon.ParseIntValueTag(reader);
					break;
				case "splitType":
				{
					string value = ChartParserCommon.ParseValueTag(reader);
					Excel2007SplitType splitType = (Excel2007SplitType)Enum.Parse(typeof(Excel2007SplitType), value, ignoreCase: false);
					if (chartSerieImpl != null)
					{
						chartSerieImpl.SerieFormat.CommonSerieOptions.SplitType = (OfficeSplitType)splitType;
					}
					else
					{
						reader.Skip();
					}
					break;
				}
				case "splitPos":
					if (chartSerieImpl != null)
					{
						chartSerieImpl.SerieFormat.CommonSerieOptions.SplitValue = ChartParserCommon.ParseIntValueTag(reader);
					}
					else
					{
						reader.Skip();
					}
					break;
				case "secondPieSize":
					if (chartSerieImpl != null)
					{
						chartSerieImpl.SerieFormat.CommonSerieOptions.PieSecondSize = ChartParserCommon.ParseIntValueTag(reader);
					}
					else
					{
						reader.Skip();
					}
					break;
				case "axId":
					ParseAxisId(reader, lstSeries, dictSeriesAxis);
					break;
				case "serLines":
					if (!reader.IsEmptyElement)
					{
						reader.Read();
						reader.Read();
						while (reader.NodeType != XmlNodeType.EndElement)
						{
							if (reader.NodeType == XmlNodeType.Element)
							{
								if (reader.LocalName == "ln")
								{
									if (chartSerieImpl != null)
									{
										ChartBorderImpl border = chartSerieImpl.SerieFormat.CommonSerieOptions.PieSeriesLine as ChartBorderImpl;
										ChartParserCommon.ParseLineProperties(reader, border, chartSerieImpl.ParentBook.DataHolder.Parser);
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
						if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "spPr")
						{
							reader.Read();
						}
						reader.Read();
					}
					else
					{
						reader.Skip();
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
		if (num.HasValue && chartSerieImpl != null)
		{
			chartSerieImpl.SerieFormat.CommonSerieOptions.GapWidth = num.Value;
		}
		else
		{
			reader.Skip();
		}
		reader.Read();
	}

	private void ParseStockChart(XmlReader reader, ChartImpl chart, RelationCollection relations, Dictionary<int, int> dictSeriesAxis, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "stockChart")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		ChartSerieImpl chartSerieImpl = null;
		List<ChartSerieImpl> list = new List<ChartSerieImpl>();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "ser":
					chartSerieImpl = ParseLineSeries(reader, chart, OfficeChartType.Line, relations, parser);
					list.Add(chartSerieImpl);
					break;
				case "hiLowLines":
				case "dropLines":
					ParseLines(reader, chart, chartSerieImpl, reader.LocalName);
					break;
				case "upDownBars":
					ParseUpDownBars(reader, chartSerieImpl, relations);
					break;
				case "axId":
					ParseAxisId(reader, list, dictSeriesAxis);
					break;
				case "extLst":
					ParseFilteredSeries(reader, chart, relations, is3D: true, chartSerieImpl.SerieType, secondary: false);
					break;
				case "dLbls":
					if (!reader.IsEmptyElement && chartSerieImpl != null)
					{
						ParseDataLabels(reader, chart, relations, chartSerieImpl.Number);
					}
					else
					{
						reader.Skip();
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
		reader.Read();
	}

	private void ParseLines(XmlReader reader, ChartImpl chart, ChartSerieImpl series, string lineStyle)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		ChartFormatImpl chartFormatImpl = null;
		chartFormatImpl = ((series == null) ? ((series.SerieFormat.CommonSerieOptions as ChartFormatCollection).ContainsIndex(0) ? (series.SerieFormat.CommonSerieOptions as ChartFormatCollection)[0] : null) : ((ChartFormatImpl)series.SerieFormat.CommonSerieOptions));
		if (chartFormatImpl != null)
		{
			switch (lineStyle)
			{
			case "hiLowLines":
				chartFormatImpl.HasHighLowLines = true;
				break;
			case "dropLines":
				chartFormatImpl.HasDropLines = true;
				break;
			case "serLines":
				chartFormatImpl.HasSeriesLines = true;
				break;
			}
			if (!reader.IsEmptyElement)
			{
				reader.Read();
				reader.Read();
				while (reader.NodeType != XmlNodeType.EndElement)
				{
					if (reader.NodeType == XmlNodeType.Element)
					{
						if (reader.LocalName == "ln")
						{
							ChartBorderImpl chartBorderImpl = new ChartBorderImpl(chart.Application, chartFormatImpl);
							ChartParserCommon.ParseLineProperties(reader, chartBorderImpl, series.ParentBook.DataHolder.Parser);
							switch (lineStyle)
							{
							case "hiLowLines":
								chartFormatImpl.HighLowLines = chartBorderImpl;
								break;
							case "dropLines":
								chartFormatImpl.DropLines = chartBorderImpl;
								break;
							case "serLines":
								chartFormatImpl.PieSeriesLine = chartBorderImpl;
								break;
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
				if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "spPr")
				{
					reader.Read();
				}
				reader.Read();
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

	private void ParseDoughnutChart(XmlReader reader, ChartImpl chart, RelationCollection relations, Dictionary<int, int> dictSeriesAxis)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "doughnutChart")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		List<ChartSerieImpl> lstSeries = new List<ChartSerieImpl>();
		IOfficeChartFormat officeChartFormat = null;
		ChartSerieImpl chartSerieImpl = ParsePieCommon(reader, chart, OfficeChartType.Doughnut, relations, lstSeries);
		if (chartSerieImpl != null)
		{
			officeChartFormat = ((reader.NodeType != XmlNodeType.EndElement) ? chartSerieImpl.SerieFormat.CommonSerieOptions : null);
		}
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element && chartSerieImpl != null)
			{
				switch (reader.LocalName)
				{
				case "firstSliceAng":
					officeChartFormat.FirstSliceAngle = ChartParserCommon.ParseIntValueTag(reader);
					break;
				case "holeSize":
					officeChartFormat.DoughnutHoleSize = ChartParserCommon.ParseIntValueTag(reader);
					break;
				case "axId":
					ParseAxisId(reader, lstSeries, dictSeriesAxis);
					break;
				case "extLst":
					ParseFilteredSeries(reader, chart, relations, is3D: true, chartSerieImpl.SerieType, secondary: false);
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

	private ChartSerieImpl ParsePieCommon(XmlReader reader, ChartImpl chart, OfficeChartType seriesType, RelationCollection relations, List<ChartSerieImpl> lstSeries)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		bool flag = true;
		ChartSerieImpl chartSerieImpl = null;
		bool isVaryColor = false;
		while (reader.NodeType != XmlNodeType.EndElement && flag)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "varyColors":
					isVaryColor = ChartParserCommon.ParseBoolValueTag(reader);
					break;
				case "ser":
					chartSerieImpl = ParsePieSeries(reader, chart, seriesType, relations);
					chartSerieImpl.SerieFormat.CommonSerieOptions.IsVaryColor = isVaryColor;
					lstSeries.Add(chartSerieImpl);
					break;
				case "dLbls":
					if (chartSerieImpl != null && !reader.IsEmptyElement)
					{
						ParseDataLabels(reader, chart, relations, chartSerieImpl.Number);
					}
					else
					{
						reader.Skip();
					}
					break;
				default:
					flag = false;
					break;
				}
			}
			else
			{
				reader.Skip();
			}
		}
		return chartSerieImpl;
	}

	internal void ParseDataLabels(XmlReader reader, ChartSerieImpl series, RelationCollection relations, bool isChartExSeries)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		if (isChartExSeries)
		{
			if (reader.LocalName != "dataLabels")
			{
				throw new XmlException("Unexpected xml tag.");
			}
			if (reader.MoveToAttribute("pos"))
			{
				series.DataPoints.DefaultDataPoint.DataLabels.Position = (OfficeDataLabelPosition)(Excel2007DataLabelPos)Enum.Parse(typeof(Excel2007DataLabelPos), reader.Value, ignoreCase: false);
			}
			series.DataPoints.DefaultDataPoint.DataLabels.FrameFormat.Interior.UseAutomaticFormat = true;
			series.DataPoints.DefaultDataPoint.DataLabels.FrameFormat.Border.AutoFormat = true;
		}
		else if (reader.LocalName != "dLbls")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				if (reader.LocalName == "dLbl")
				{
					ParseDataLabel(reader, series, relations, isChartExSeries);
					continue;
				}
				IOfficeChartDataLabels dataLabels = series.DataPoints.DefaultDataPoint.DataLabels;
				FileDataHolder dataHolder = series.ParentBook.DataHolder;
				Excel2007Parser parser = dataHolder.Parser;
				if (isChartExSeries)
				{
					dataLabels.IsLegendKey = false;
				}
				ParseDataLabelSettings(reader, dataLabels, parser, dataHolder, relations, isChartExSeries: false);
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Read();
	}

	private void ParseDataLabel(XmlReader reader, ChartSerieImpl series, RelationCollection relations, bool isChartExSeries)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		IOfficeChartDataLabels officeChartDataLabels = null;
		if (isChartExSeries)
		{
			if (reader.LocalName != "dataLabel")
			{
				throw new XmlException("Unexpeced xml tag.");
			}
			if (reader.MoveToAttribute("idx"))
			{
				int index = XmlConvertExtension.ToInt32(reader.Value);
				officeChartDataLabels = series.DataPoints[index].DataLabels;
				(officeChartDataLabels as ChartDataLabelsImpl).ShowTextProperties = false;
			}
			if (officeChartDataLabels != null && reader.MoveToAttribute("pos"))
			{
				officeChartDataLabels.Position = (OfficeDataLabelPosition)(Excel2007DataLabelPos)Enum.Parse(typeof(Excel2007DataLabelPos), reader.Value, ignoreCase: false);
			}
		}
		else if (reader.LocalName != "dLbl")
		{
			throw new XmlException("Unexpeced xml tag.");
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "idx":
				{
					int index2 = ChartParserCommon.ParseIntValueTag(reader);
					officeChartDataLabels = series.DataPoints[index2].DataLabels;
					(officeChartDataLabels as ChartDataLabelsImpl).ShowTextProperties = false;
					break;
				}
				case "layout":
					(officeChartDataLabels as ChartDataLabelsImpl).Layout = new ChartLayoutImpl(m_book.Application, officeChartDataLabels as ChartDataLabelsImpl, series.Parent);
					ChartParserCommon.ParseChartLayout(reader, (officeChartDataLabels as ChartDataLabelsImpl).Layout);
					break;
				case "delete":
				{
					bool isDelete = ChartParserCommon.ParseBoolValueTag(reader);
					(officeChartDataLabels as ChartDataLabelsImpl).IsDelete = isDelete;
					break;
				}
				case "numFmt":
					ChartParserCommon.ParseNumberFormat(reader, officeChartDataLabels);
					break;
				default:
				{
					FileDataHolder dataHolder = series.ParentBook.DataHolder;
					Excel2007Parser parser = dataHolder.Parser;
					ParseDataLabelSettings(reader, officeChartDataLabels, parser, dataHolder, relations, isChartExSeries);
					break;
				}
				}
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Read();
	}

	private void ParseDataLabelSettings(XmlReader reader, IOfficeChartDataLabels dataLabels, Excel2007Parser parser, FileDataHolder holder, RelationCollection relations, bool isChartExSeries)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (dataLabels == null)
		{
			throw new ArgumentNullException("dataLabels");
		}
		(dataLabels as IInternalOfficeChartTextArea).Size = 10.0;
		ChartDataLabelsImpl chartDataLabelsImpl = dataLabels as ChartDataLabelsImpl;
		chartDataLabelsImpl.ShowTextProperties = false;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "dLblPos":
				{
					string value = ChartParserCommon.ParseValueTag(reader);
					Excel2007DataLabelPos position = (Excel2007DataLabelPos)Enum.Parse(typeof(Excel2007DataLabelPos), value, ignoreCase: false);
					dataLabels.Position = (OfficeDataLabelPosition)position;
					break;
				}
				case "showLegendKey":
					if (!(dataLabels as ChartDataLabelsImpl).m_bHasLegendKeyOption)
					{
						dataLabels.IsLegendKey = ChartParserCommon.ParseBoolValueTag(reader);
					}
					else
					{
						reader.Skip();
					}
					break;
				case "showLeaderLines":
				{
					bool showLeaderLines = ChartParserCommon.ParseBoolValueTag(reader);
					dataLabels.ShowLeaderLines = showLeaderLines;
					break;
				}
				case "extLst":
					ParseDataLabelsExtensionList(reader, chartDataLabelsImpl, holder, relations);
					break;
				case "leaderLines":
					ParseLeaderLines(reader, chartDataLabelsImpl, holder, relations);
					break;
				case "showVal":
					if (!(dataLabels as ChartDataLabelsImpl).m_bHasValueOption)
					{
						dataLabels.IsValue = ChartParserCommon.ParseBoolValueTag(reader);
					}
					else
					{
						reader.Skip();
					}
					break;
				case "showCatName":
					if (!(dataLabels as ChartDataLabelsImpl).m_bHasCategoryOption)
					{
						dataLabels.IsCategoryName = ChartParserCommon.ParseBoolValueTag(reader);
					}
					else
					{
						reader.Skip();
					}
					break;
				case "showPercent":
					if (!(dataLabels as ChartDataLabelsImpl).m_bHasPercentageOption)
					{
						dataLabels.IsPercentage = ChartParserCommon.ParseBoolValueTag(reader);
					}
					else
					{
						reader.Skip();
					}
					break;
				case "showBubbleSize":
					if (!(dataLabels as ChartDataLabelsImpl).m_bHasBubbleSizeOption)
					{
						dataLabels.IsBubbleSize = ChartParserCommon.ParseBoolValueTag(reader);
					}
					else
					{
						reader.Skip();
					}
					break;
				case "showSerName":
					if (!(dataLabels as ChartDataLabelsImpl).m_bHasSeriesOption)
					{
						dataLabels.IsSeriesName = ChartParserCommon.ParseBoolValueTag(reader);
					}
					else
					{
						reader.Skip();
					}
					break;
				case "separator":
					dataLabels.Delimiter = reader.ReadElementContentAsString();
					break;
				case "txPr":
				{
					IInternalOfficeChartTextArea textFormatting = dataLabels as IInternalOfficeChartTextArea;
					ParseDefaultTextFormatting(reader, textFormatting, parser);
					(dataLabels as ChartDataLabelsImpl).ShowTextProperties = true;
					IOfficeChartDataPoint officeChartDataPoint = (IOfficeChartDataPoint)(dataLabels as ChartDataLabelsImpl).Parent;
					if (!officeChartDataPoint.IsDefault || (officeChartDataPoint.DataLabels as ChartDataLabelsImpl).TextArea.Text == null)
					{
						break;
					}
					ChartTextAreaImpl textArea = (officeChartDataPoint.DataLabels as ChartDataLabelsImpl).TextArea;
					foreach (ChartDataPointImpl item in (ChartDataPointsCollection)officeChartDataPoint.Parent)
					{
						if (item.HasDataLabels && (item.DataLabels as ChartDataLabelsImpl).ParagraphType != ChartParagraphType.CustomDefault)
						{
							(item.DataLabels as ChartDataLabelsImpl).TextArea = textArea;
						}
					}
					break;
				}
				case "numFmt":
					ChartParserCommon.ParseNumberFormat(reader, dataLabels);
					break;
				case "delete":
				{
					bool isDelete = ChartParserCommon.ParseBoolValueTag(reader);
					(dataLabels as ChartDataLabelsImpl).IsDelete = isDelete;
					break;
				}
				case "spPr":
				{
					IChartFillObjectGetter objectGetter = new ChartFillObjectGetterAny(dataLabels.FrameFormat.Border as ChartBorderImpl, dataLabels.FrameFormat.Interior as ChartInteriorImpl, dataLabels.FrameFormat.Fill as IInternalFill, dataLabels.FrameFormat.Shadow as ShadowImpl, dataLabels.FrameFormat.ThreeD as ThreeDFormatImpl);
					ChartParserCommon.ParseShapeProperties(reader, objectGetter, holder, relations);
					(dataLabels as ChartDataLabelsImpl).ShowTextProperties = true;
					break;
				}
				case "visibility":
					ParseChartDataLabelVisibility(reader, chartDataLabelsImpl);
					break;
				default:
					if (isChartExSeries)
					{
						dataLabels.IsCategoryName = false;
						dataLabels.IsValue = true;
						reader.Skip();
					}
					else
					{
						ChartParserCommon.ParseTextAreaTag(reader, dataLabels as IInternalOfficeChartTextArea, relations, holder, 10f);
					}
					break;
				}
			}
			else
			{
				reader.Skip();
			}
		}
	}

	internal void ParseDataLabelsExtensionList(XmlReader reader, ChartDataLabelsImpl dataLabels, FileDataHolder holder, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (dataLabels == null)
		{
			throw new ArgumentNullException("dataLabels");
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
						ParseDataLabelsExtension(reader, dataLabels, holder, relations);
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

	private void ParseDataLabelsExtension(XmlReader reader, ChartDataLabelsImpl dataLabels, FileDataHolder holder, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (dataLabels == null)
		{
			throw new ArgumentNullException("table");
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
					switch (reader.LocalName)
					{
					case "showLeaderLines":
						dataLabels.Serie.HasLeaderLines = ChartParserCommon.ParseBoolValueTag(reader);
						break;
					case "leaderLines":
						ParseLeaderLines(reader, dataLabels, holder, relations);
						break;
					case "showDataLabelsRange":
						dataLabels.IsValueFromCells = ChartParserCommon.ParseBoolValueTag(reader);
						break;
					case "layout":
						ChartParserCommon.ParseChartLayout(reader, dataLabels.Layout);
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

	private void ParseLeaderLines(XmlReader reader, ChartDataLabelsImpl dataLabels, FileDataHolder holder, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (dataLabels == null)
		{
			throw new ArgumentNullException("table");
		}
		if (reader.LocalName != "leaderLines")
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
					if (reader.LocalName == "spPr")
					{
						IChartFillObjectGetter objectGetter = new ChartFillObjectGetterAny(dataLabels.Serie.LeaderLines as ChartBorderImpl, null, null, null, null);
						ChartParserCommon.ParseShapeProperties(reader, objectGetter, holder, relations);
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

	private ChartSerieImpl ParseBarSeries(XmlReader reader, ChartImpl chart, OfficeChartType seriesType, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		ChartSerieImpl chartSerieImpl = (ChartSerieImpl)chart.Series.Add(seriesType);
		ParseSeriesCommonWithoutEnd(reader, chartSerieImpl, relations);
		string text = null;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				object[] values;
				switch (reader.LocalName)
				{
				case "dLbls":
					ParseDataLabels(reader, chartSerieImpl, relations, isChartExSeries: false);
					break;
				case "trendline":
					ParseTrendlines(reader, chartSerieImpl, relations);
					break;
				case "shape":
					text = ChartParserCommon.ParseValueTag(reader);
					ParseBarShape(text, chartSerieImpl.SerieFormat);
					chartSerieImpl.HasColumnShape = true;
					break;
				case "errBars":
					ParseErrorBars(reader, chartSerieImpl, relations);
					break;
				case "cat":
					chartSerieImpl.CategoryLabelsIRange = ParseSeriesValues(reader, chartSerieImpl, out values, isValueAxis: false, "cat");
					if (values != null && (chartSerieImpl.CategoryLabelsIRange == null || chartSerieImpl.CategoryLabelsIRange.Count < values.Length))
					{
						chartSerieImpl.IsValidCategoryRange = false;
					}
					if (values != null)
					{
						chartSerieImpl.EnteredDirectlyCategoryLabels = values;
					}
					break;
				case "val":
					chartSerieImpl.ValuesIRange = ParseSeriesValues(reader, chartSerieImpl, out values, isValueAxis: true, "val");
					if (values != null && (chartSerieImpl.ValuesIRange == null || chartSerieImpl.ValuesIRange.Count < values.Length))
					{
						chartSerieImpl.IsValidValueRange = false;
					}
					if (values != null && chartSerieImpl.ValuesIRange == null)
					{
						chartSerieImpl.EnteredDirectlyValues = values;
					}
					break;
				case "dPt":
					ParseDataPoint(reader, chartSerieImpl, relations);
					break;
				case "extLst":
					ParseFilteredSeriesOrCategoryName(reader, chartSerieImpl);
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
		return chartSerieImpl;
	}

	private void ParseFilteredSeriesOrCategoryName(XmlReader reader, ChartSerieImpl series)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "extLst")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		if (reader.NodeType != XmlNodeType.Element)
		{
			Excel2007Parser.SkipWhiteSpaces(reader);
		}
		if (!(reader.LocalName == "ext"))
		{
			return;
		}
		while (reader.LocalName != "extLst")
		{
			if (reader.LocalName == "ext" && reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.LocalName == "ext" && reader.LocalName != "ext")
				{
					series.extentsStream = ShapeParser.ReadNodeAsStream(reader);
				}
				reader.Read();
				if (reader.LocalName == "invertSolidFillFmt")
				{
					series.m_invertFillFormatStream = ShapeParser.ReadNodeAsStream(reader);
				}
				if (reader.LocalName == "filteredCategoryTitle" || reader.LocalName == "filteredSeriesTitle")
				{
					reader.Read();
				}
				if (reader.LocalName == "tx")
				{
					ParseSeriesText(reader, series);
					series.ParentChart.SeriesNameLevel = OfficeSeriesNameLevel.SeriesNameLevelNone;
					reader.Skip();
					reader.Read();
				}
				else if (reader.LocalName == "cat")
				{
					series.CategoryLabelsIRange = ParseSeriesValues(reader, series, out var values, isValueAxis: false, "cat");
					if (values != null)
					{
						series.EnteredDirectlyCategoryLabels = values;
					}
					series.ParentChart.CategoryLabelLevel = OfficeCategoriesLabelLevel.CategoriesLabelLevelNone;
					reader.Skip();
					reader.Read();
				}
				else if (reader.LocalName == "datalabelsRange")
				{
					ParseDatalabelsRange(reader, series);
				}
			}
			else
			{
				reader.Read();
			}
		}
		reader.Read();
	}

	private void ParseDatalabelsRange(XmlReader reader, ChartSerieImpl series)
	{
		if (reader == null)
		{
			throw new ArgumentException("Xmlreader");
		}
		if (series == null)
		{
			throw new ArgumentException("Series");
		}
		string text = null;
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType != XmlNodeType.Element)
			{
				continue;
			}
			string localName = reader.LocalName;
			if (!(localName == "f"))
			{
				if (localName == "dlblRangeCache")
				{
					ParseDatalabelRangeCache(reader, series);
				}
				else
				{
					reader.Skip();
				}
			}
			else
			{
				text = reader.ReadElementContentAsString();
			}
		}
		WorkbookImpl parentBook = series.ParentBook;
		if (parentBook != null && !string.IsNullOrEmpty(text))
		{
			(series.DataPoints.DefaultDataPoint.DataLabels as ChartDataLabelsImpl).ValueFromCellsIRange = GetRange(parentBook, text);
		}
		reader.Read();
	}

	private void ParseDatalabelRangeCache(XmlReader reader, ChartSerieImpl series)
	{
		if (reader == null)
		{
			throw new ArgumentException("XmlReader");
		}
		reader.Read();
		string formatCode = null;
		Dictionary<int, object> dictionary = new Dictionary<int, object>();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType != XmlNodeType.Element)
			{
				continue;
			}
			string localName = reader.LocalName;
			if (!(localName == "pt"))
			{
				if (localName == "ptCount")
				{
					if (reader.MoveToAttribute("val"))
					{
						XmlConvertExtension.ToInt32(reader.Value);
					}
					reader.Read();
				}
				else
				{
					reader.Skip();
				}
			}
			else
			{
				AddNumericPoint(reader, dictionary, isString: true, ref formatCode);
			}
		}
		if (series != null && dictionary != null)
		{
			series.DataLabelCellsValues = dictionary;
		}
		reader.Read();
	}

	private ChartSerieImpl ParseSurfaceSeries(XmlReader reader, ChartImpl chart, OfficeChartType seriesType, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		ChartSerieImpl chartSerieImpl = (ChartSerieImpl)chart.Series.Add(seriesType);
		ParseSeriesCommonWithoutEnd(reader, chartSerieImpl, relations);
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				object[] values;
				switch (reader.LocalName)
				{
				case "cat":
					chartSerieImpl.CategoryLabelsIRange = ParseSeriesValues(reader, chartSerieImpl, out values, isValueAxis: false, "cat");
					if (values != null && (chartSerieImpl.CategoryLabelsIRange == null || chartSerieImpl.CategoryLabelsIRange.Count < values.Length))
					{
						chartSerieImpl.IsValidCategoryRange = false;
					}
					if (values != null)
					{
						chartSerieImpl.EnteredDirectlyCategoryLabels = values;
					}
					break;
				case "val":
					chartSerieImpl.ValuesIRange = ParseSeriesValues(reader, chartSerieImpl, out values, isValueAxis: true, "val");
					if (values != null && (chartSerieImpl.ValuesIRange == null || chartSerieImpl.ValuesIRange.Count < values.Length))
					{
						chartSerieImpl.IsValidValueRange = false;
					}
					if (values != null && chartSerieImpl.ValuesIRange == null)
					{
						chartSerieImpl.EnteredDirectlyValues = values;
					}
					break;
				case "extLst":
					ParseFilteredSeriesOrCategoryName(reader, chartSerieImpl);
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
		return chartSerieImpl;
	}

	private ChartSerieImpl ParsePieSeries(XmlReader reader, ChartImpl chart, OfficeChartType seriesType, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		ChartSerieImpl chartSerieImpl = (ChartSerieImpl)chart.Series.Add(seriesType);
		ParseSeriesCommonWithoutEnd(reader, chartSerieImpl, relations);
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				object[] values;
				switch (reader.LocalName)
				{
				case "explosion":
					chartSerieImpl.SerieFormat.Percent = ChartParserCommon.ParseIntValueTag(reader);
					break;
				case "dLbls":
					ParseDataLabels(reader, chartSerieImpl, relations, isChartExSeries: false);
					break;
				case "trendline":
					ParseTrendlines(reader, chartSerieImpl, relations);
					break;
				case "errBars":
					ParseErrorBars(reader, chartSerieImpl, relations);
					break;
				case "cat":
					chartSerieImpl.CategoryLabelsIRange = ParseSeriesValues(reader, chartSerieImpl, out values, isValueAxis: false, "cat");
					if (values != null && (chartSerieImpl.CategoryLabelsIRange == null || chartSerieImpl.CategoryLabelsIRange.Count < values.Length))
					{
						chartSerieImpl.IsValidCategoryRange = false;
					}
					if (values != null)
					{
						chartSerieImpl.EnteredDirectlyCategoryLabels = values;
					}
					break;
				case "val":
					chartSerieImpl.ValuesIRange = ParseSeriesValues(reader, chartSerieImpl, out values, isValueAxis: true, "val");
					if (values != null && (chartSerieImpl.ValuesIRange == null || chartSerieImpl.ValuesIRange.Count < values.Length))
					{
						chartSerieImpl.IsValidValueRange = false;
					}
					if (values != null && chartSerieImpl.ValuesIRange == null)
					{
						chartSerieImpl.EnteredDirectlyValues = values;
					}
					break;
				case "dPt":
					ParseDataPoint(reader, chartSerieImpl, relations);
					break;
				case "extLst":
					ParseFilteredSeriesOrCategoryName(reader, chartSerieImpl);
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
		return chartSerieImpl;
	}

	internal virtual void ParseChartDataLabelVisibility(XmlReader reader, ChartDataLabelsImpl dataLabels)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (dataLabels == null)
		{
			throw new ArgumentNullException("data labels");
		}
		if (reader.LocalName != "visibility")
		{
			throw new XmlException("Unexpeced xml tag.");
		}
		reader.Skip();
	}

	private ChartSerieImpl ParseLineSeries(XmlReader reader, ChartImpl chart, OfficeChartType seriesType, RelationCollection relations, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		ChartSerieImpl chartSerieImpl = (ChartSerieImpl)chart.Series.Add(seriesType);
		ParseSeriesCommonWithoutEnd(reader, chartSerieImpl, relations);
		FileDataHolder dataHolder = chartSerieImpl.ParentBook.DataHolder;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				object[] values;
				switch (reader.LocalName)
				{
				case "spPr":
				{
					ChartFillObjectGetterAny objectGetter = new ChartFillObjectGetterAny(chartSerieImpl.SerieFormat.LineProperties as ChartBorderImpl, null, null, chartSerieImpl.SerieFormat.Shadow as ShadowImpl, chartSerieImpl.SerieFormat.ThreeD as ThreeDFormatImpl);
					ChartParserCommon.ParseShapeProperties(reader, objectGetter, dataHolder, relations);
					break;
				}
				case "smooth":
				{
					bool isSmoothedLine = ChartParserCommon.ParseBoolValueTag(reader);
					ChartSerieDataFormatImpl chartSerieDataFormatImpl = (ChartSerieDataFormatImpl)chartSerieImpl.SerieFormat;
					if (chartSerieDataFormatImpl != null)
					{
						chartSerieDataFormatImpl.IsSmoothedLine = isSmoothedLine;
					}
					break;
				}
				case "marker":
					ParseMarker(reader, chartSerieImpl, parser);
					break;
				case "dLbls":
					ParseDataLabels(reader, chartSerieImpl, relations, isChartExSeries: false);
					break;
				case "trendline":
					ParseTrendlines(reader, chartSerieImpl, relations);
					break;
				case "errBars":
					ParseErrorBars(reader, chartSerieImpl, relations);
					break;
				case "cat":
					chartSerieImpl.CategoryLabelsIRange = ParseSeriesValues(reader, chartSerieImpl, out values, isValueAxis: false, "cat");
					if (values != null && (chartSerieImpl.CategoryLabelsIRange == null || chartSerieImpl.CategoryLabelsIRange.Count < values.Length))
					{
						chartSerieImpl.IsValidCategoryRange = false;
					}
					if (values != null)
					{
						chartSerieImpl.EnteredDirectlyCategoryLabels = values;
					}
					break;
				case "val":
					chartSerieImpl.ValuesIRange = ParseSeriesValues(reader, chartSerieImpl, out values, isValueAxis: true, "val");
					if (values != null && (chartSerieImpl.ValuesIRange == null || chartSerieImpl.ValuesIRange.Count < values.Length))
					{
						chartSerieImpl.IsValidValueRange = false;
					}
					if (values != null && chartSerieImpl.ValuesIRange == null)
					{
						chartSerieImpl.EnteredDirectlyValues = values;
					}
					break;
				case "dPt":
					ParseDataPoint(reader, chartSerieImpl, relations);
					break;
				case "extLst":
					ParseFilteredSeriesOrCategoryName(reader, chartSerieImpl);
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
		return chartSerieImpl;
	}

	private ChartSerieImpl ParseScatterSeries(XmlReader reader, ChartImpl chart, OfficeChartType seriesType, RelationCollection relations, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		ChartSerieImpl chartSerieImpl = (ChartSerieImpl)chart.Series.Add(seriesType);
		ParseSeriesCommonWithoutEnd(reader, chartSerieImpl, relations);
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				object[] values;
				switch (reader.LocalName)
				{
				case "spPr":
				{
					ChartSerieDataFormatImpl dataFormat = (ChartSerieDataFormatImpl)chartSerieImpl.SerieFormat;
					FileDataHolder dataHolder = chartSerieImpl.ParentBook.DataHolder;
					ChartFillObjectGetter objectGetter = new ChartFillObjectGetter(dataFormat);
					ChartParserCommon.ParseShapeProperties(reader, objectGetter, dataHolder, relations);
					break;
				}
				case "marker":
					ParseMarker(reader, chartSerieImpl, parser);
					break;
				case "dLbls":
					ParseDataLabels(reader, chartSerieImpl, relations, isChartExSeries: false);
					break;
				case "trendline":
					ParseTrendlines(reader, chartSerieImpl, relations);
					break;
				case "errBars":
					ParseErrorBars(reader, chartSerieImpl, relations);
					break;
				case "xVal":
				{
					chartSerieImpl.CategoryLabelsIRange = ParseSeriesValues(reader, chartSerieImpl, out values, isValueAxis: false, "cat");
					NameImpl nameImpl2 = chartSerieImpl.CategoryLabelsIRange as NameImpl;
					if (chartSerieImpl.CategoriesFormatCode != null && chartSerieImpl.CategoryLabelsIRange != null && !(chartSerieImpl.CategoryLabelsIRange is ExternalRange) && ((nameImpl2 != null && nameImpl2.RefersToRange != null) || chartSerieImpl.CategoryLabelsIRange is RangeImpl))
					{
						chartSerieImpl.CategoryLabelsIRange.NumberFormat = chartSerieImpl.CategoriesFormatCode;
					}
					if (values != null && (chartSerieImpl.CategoryLabelsIRange == null || chartSerieImpl.CategoryLabelsIRange.Count < values.Length))
					{
						chartSerieImpl.IsValidCategoryRange = false;
					}
					if (values != null)
					{
						chartSerieImpl.EnteredDirectlyCategoryLabels = values;
					}
					break;
				}
				case "yVal":
				{
					chartSerieImpl.ValuesIRange = ParseSeriesValues(reader, chartSerieImpl, out values, isValueAxis: false, "val");
					NameImpl nameImpl = chartSerieImpl.ValuesIRange as NameImpl;
					if (chartSerieImpl.FormatCode != null && chartSerieImpl.ValuesIRange != null && !(chartSerieImpl.ValuesIRange is ExternalRange) && ((nameImpl != null && nameImpl.RefersToRange != null) || chartSerieImpl.ValuesIRange is RangeImpl))
					{
						chartSerieImpl.ValuesIRange.NumberFormat = chartSerieImpl.FormatCode;
					}
					if (values != null && (chartSerieImpl.ValuesIRange == null || chartSerieImpl.ValuesIRange.Count < values.Length))
					{
						chartSerieImpl.IsValidValueRange = false;
					}
					if (values != null)
					{
						chartSerieImpl.EnteredDirectlyValues = values;
					}
					break;
				}
				case "smooth":
				{
					bool isSmoothedLine = ChartParserCommon.ParseBoolValueTag(reader);
					((ChartSerieDataFormatImpl)chartSerieImpl.SerieFormat).IsSmoothedLine = isSmoothedLine;
					break;
				}
				case "dPt":
					ParseDataPoint(reader, chartSerieImpl, relations);
					break;
				case "extLst":
					ParseFilteredSeriesOrCategoryName(reader, chartSerieImpl);
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
		return chartSerieImpl;
	}

	private ChartSerieImpl ParseRadarSeries(XmlReader reader, ChartImpl chart, OfficeChartType seriesType, RelationCollection relations, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		ChartSerieImpl chartSerieImpl = (ChartSerieImpl)chart.Series.Add(seriesType);
		ParseSeriesCommonWithoutEnd(reader, chartSerieImpl, relations);
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				object[] values;
				switch (reader.LocalName)
				{
				case "marker":
					ParseMarker(reader, chartSerieImpl, parser);
					break;
				case "dLbls":
					ParseDataLabels(reader, chartSerieImpl, relations, isChartExSeries: false);
					break;
				case "cat":
					chartSerieImpl.CategoryLabelsIRange = ParseSeriesValues(reader, chartSerieImpl, out values, isValueAxis: false, "cat");
					if (chartSerieImpl.CategoriesFormatCode != null && chartSerieImpl.CategoryLabelsIRange != null && !(chartSerieImpl.CategoryLabelsIRange is ExternalRange))
					{
						chartSerieImpl.CategoryLabelsIRange.NumberFormat = chartSerieImpl.CategoriesFormatCode;
					}
					if (values != null && (chartSerieImpl.CategoryLabelsIRange == null || chartSerieImpl.CategoryLabelsIRange.Count < values.Length))
					{
						chartSerieImpl.IsValidCategoryRange = false;
					}
					if (values != null)
					{
						chartSerieImpl.EnteredDirectlyCategoryLabels = values;
					}
					break;
				case "val":
					chartSerieImpl.ValuesIRange = ParseSeriesValues(reader, chartSerieImpl, out values, isValueAxis: true, "val");
					if (chartSerieImpl.FormatCode != null && chartSerieImpl.ValuesIRange != null && !(chartSerieImpl.ValuesIRange is ExternalRange))
					{
						chartSerieImpl.ValuesIRange.NumberFormat = chartSerieImpl.FormatCode;
					}
					if (values != null && (chartSerieImpl.ValuesIRange == null || chartSerieImpl.ValuesIRange.Count < values.Length))
					{
						chartSerieImpl.IsValidValueRange = false;
					}
					if (values != null && chartSerieImpl.ValuesIRange == null)
					{
						chartSerieImpl.EnteredDirectlyValues = values;
					}
					break;
				case "dPt":
					ParseDataPoint(reader, chartSerieImpl, relations);
					break;
				case "extLst":
					ParseFilteredSeriesOrCategoryName(reader, chartSerieImpl);
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
		return chartSerieImpl;
	}

	private ChartSerieImpl ParseBubbleSeries(XmlReader reader, ChartImpl chart, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "ser")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		ChartSerieImpl chartSerieImpl = (ChartSerieImpl)chart.Series.Add(OfficeChartType.Bubble);
		ParseSeriesCommonWithoutEnd(reader, chartSerieImpl, relations);
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				object[] values;
				switch (reader.LocalName)
				{
				case "dLbls":
					ParseDataLabels(reader, chartSerieImpl, relations, isChartExSeries: false);
					break;
				case "trendline":
					ParseTrendlines(reader, chartSerieImpl, relations);
					break;
				case "errBars":
					ParseErrorBars(reader, chartSerieImpl, relations);
					break;
				case "xVal":
					chartSerieImpl.CategoryLabelsIRange = ParseSeriesValues(reader, chartSerieImpl, out values, isValueAxis: false, "xVal");
					if (values != null && (chartSerieImpl.CategoryLabelsIRange == null || chartSerieImpl.CategoryLabelsIRange.Count < values.Length))
					{
						chartSerieImpl.IsValidCategoryRange = false;
					}
					if (values != null)
					{
						chartSerieImpl.EnteredDirectlyCategoryLabels = values;
					}
					break;
				case "yVal":
					chartSerieImpl.ValuesIRange = ParseSeriesValues(reader, chartSerieImpl, out values, isValueAxis: false, "yVal");
					if (values != null && (chartSerieImpl.ValuesIRange == null || chartSerieImpl.ValuesIRange.Count < values.Length))
					{
						chartSerieImpl.IsValidValueRange = false;
					}
					if (values != null)
					{
						chartSerieImpl.EnteredDirectlyValues = values;
					}
					break;
				case "bubbleSize":
					chartSerieImpl.BubblesIRange = ParseSeriesValues(reader, chartSerieImpl, out values, isValueAxis: false, "bubbleSize");
					if (values != null)
					{
						chartSerieImpl.EnteredDirectlyBubbles = values;
					}
					break;
				case "bubble3D":
					if (ChartParserCommon.ParseBoolValueTag(reader))
					{
						chartSerieImpl.SerieFormat.Is3DBubbles = true;
					}
					break;
				case "dPt":
					ParseDataPoint(reader, chartSerieImpl, relations);
					break;
				case "extLst":
					ParseFilteredSeriesOrCategoryName(reader, chartSerieImpl);
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
		return chartSerieImpl;
	}

	private ChartSerieImpl ParseAreaSeries(XmlReader reader, ChartImpl chart, OfficeChartType seriesType, RelationCollection relations, bool isPrimary)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		OfficeChartType officeChartType = chart.ChartType;
		if (officeChartType == seriesType || officeChartType == OfficeChartType.Combination_Chart)
		{
			officeChartType = seriesType;
		}
		ChartSerieImpl chartSerieImpl = (ChartSerieImpl)chart.Series.Add(officeChartType);
		if (!isPrimary & (chartSerieImpl.ParentSeries.Count > 1))
		{
			chartSerieImpl.UsePrimaryAxis = isPrimary;
		}
		if (officeChartType != seriesType)
		{
			chartSerieImpl.SerieType = seriesType;
		}
		ParseSeriesCommonWithoutEnd(reader, chartSerieImpl, relations);
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				object[] values;
				switch (reader.LocalName)
				{
				case "dLbls":
					ParseDataLabels(reader, chartSerieImpl, relations, isChartExSeries: false);
					break;
				case "trendline":
					ParseTrendlines(reader, chartSerieImpl, relations);
					break;
				case "errBars":
					ParseErrorBars(reader, chartSerieImpl, relations);
					break;
				case "cat":
					chartSerieImpl.CategoryLabelsIRange = ParseSeriesValues(reader, chartSerieImpl, out values, isValueAxis: false, "cat");
					if (values != null && (chartSerieImpl.CategoryLabelsIRange == null || chartSerieImpl.CategoryLabelsIRange.Count < values.Length))
					{
						chartSerieImpl.IsValidCategoryRange = false;
					}
					if (values != null)
					{
						chartSerieImpl.EnteredDirectlyCategoryLabels = values;
					}
					break;
				case "val":
					chartSerieImpl.ValuesIRange = ParseSeriesValues(reader, chartSerieImpl, out values, isValueAxis: true, "val");
					if (values != null && (chartSerieImpl.ValuesIRange == null || chartSerieImpl.ValuesIRange.Count < values.Length))
					{
						chartSerieImpl.IsValidValueRange = false;
					}
					if (values != null && chartSerieImpl.ValuesIRange == null)
					{
						chartSerieImpl.EnteredDirectlyValues = values;
					}
					break;
				case "dPt":
					ParseDataPoint(reader, chartSerieImpl, relations);
					break;
				case "extLst":
					ParseFilteredSeriesOrCategoryName(reader, chartSerieImpl);
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
		return chartSerieImpl;
	}

	private void ParseSeriesCommonWithoutEnd(XmlReader reader, ChartSerieImpl series, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		if (reader.LocalName != "ser")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		bool flag = true;
		while (reader.NodeType != XmlNodeType.EndElement && flag)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "idx":
				{
					string s2 = ChartParserCommon.ParseValueTag(reader);
					series.Number = int.Parse(s2);
					break;
				}
				case "order":
				{
					string s = ChartParserCommon.ParseValueTag(reader);
					series.Index = int.Parse(s);
					break;
				}
				case "tx":
					ParseSeriesText(reader, series);
					break;
				case "spPr":
					ParseSeriesProperties(reader, series, relations);
					break;
				case "invertIfNegative":
					if (reader.MoveToAttribute("val"))
					{
						series.SetInvertIfNegative(XmlConvertExtension.ToBoolean(reader.Value));
					}
					else
					{
						reader.Skip();
					}
					break;
				default:
					flag = false;
					break;
				}
			}
			else
			{
				reader.Skip();
			}
		}
	}

	private void ParseSeriesText(XmlReader reader, ChartSerieImpl series)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		if (reader.LocalName != "tx")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (series.IsDefaultName && reader.NodeType == XmlNodeType.Element)
				{
					string localName = reader.LocalName;
					if (!(localName == "v"))
					{
						if (localName == "strRef")
						{
							string text = ParseStringReference(reader, series);
							if (!string.IsNullOrEmpty(text))
							{
								series.Name = "=" + text;
								if (!string.IsNullOrEmpty(series.Name) && series.Name[0] == '=')
								{
									series.Name = series.SerieName;
								}
							}
							text = ((series.SerieName != null) ? series.SerieName : series.Name);
							if (series.NameRangeIRange == null)
							{
								series.Name = series.SerieName;
							}
						}
						else
						{
							reader.Skip();
						}
					}
					else
					{
						series.Name = reader.ReadElementContentAsString();
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

	private void ParseDirectlyEnteredStringValue(XmlReader reader, ChartSerieImpl series)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		reader.Read();
		new List<object>();
		if (reader.NodeType != XmlNodeType.EndElement)
		{
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					if (reader.LocalName == "pt")
					{
						series.SerieName = AddStringPoint(reader);
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

	private string AddStringPoint(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "pt")
		{
			throw new XmlException();
		}
		string result = null;
		if (reader.MoveToAttribute("idx"))
		{
			XmlConvertExtension.ToInt32(reader.Value);
		}
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					if (reader.LocalName == "v")
					{
						result = reader.ReadElementContentAsString();
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
		return result;
	}

	internal void ParseDataPoint(XmlReader reader, ChartSerieImpl series, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		if (reader.LocalName != "dPt" && reader.LocalName != "dataPt")
		{
			throw new XmlException();
		}
		if (!reader.IsEmptyElement)
		{
			FileDataHolder dataHolder = series.ParentChart.ParentWorkbook.DataHolder;
			bool num = this is ChartExParser;
			IOfficeChartDataPoint officeChartDataPoint = null;
			if (num && reader.MoveToAttribute("idx"))
			{
				officeChartDataPoint = series.DataPoints[XmlConvertExtension.ToInt32(reader.Value)];
				(officeChartDataPoint as ChartDataPointImpl).HasDataPoint = true;
			}
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					switch (reader.LocalName)
					{
					case "idx":
					{
						int index = int.Parse(ChartParserCommon.ParseValueTag(reader));
						officeChartDataPoint = series.DataPoints[index];
						(officeChartDataPoint as ChartDataPointImpl).HasDataPoint = true;
						break;
					}
					case "spPr":
					{
						if (officeChartDataPoint == null)
						{
							throw new XmlException();
						}
						ChartSerieDataFormatImpl chartSerieDataFormatImpl2 = officeChartDataPoint.DataFormat as ChartSerieDataFormatImpl;
						chartSerieDataFormatImpl2.HasLineProperties = true;
						chartSerieDataFormatImpl2.HasInterior = true;
						chartSerieDataFormatImpl2.IsParsed = true;
						ChartFillObjectGetterAny objectGetter = new ChartFillObjectGetterAny(chartSerieDataFormatImpl2.LineProperties as ChartBorderImpl, chartSerieDataFormatImpl2.Interior as ChartInteriorImpl, chartSerieDataFormatImpl2.Fill as IInternalFill, chartSerieDataFormatImpl2.Shadow as ShadowImpl, chartSerieDataFormatImpl2.ThreeD as ThreeDFormatImpl);
						ChartParserCommon.ParseShapeProperties(reader, objectGetter, dataHolder, relations);
						chartSerieDataFormatImpl2.IsDataPointColorParsed = true;
						break;
					}
					case "marker":
					{
						ChartSerieDataFormatImpl chartSerieDataFormatImpl3 = officeChartDataPoint.DataFormat as ChartSerieDataFormatImpl;
						ParseMarker(reader, chartSerieDataFormatImpl3, dataHolder.Parser);
						officeChartDataPoint.IsDefaultmarkertype = true;
						chartSerieDataFormatImpl3.IsParsed = true;
						break;
					}
					case "invertIfNegative":
					{
						if (!reader.MoveToAttribute("val"))
						{
							break;
						}
						ChartSerieDataFormatImpl chartSerieDataFormatImpl = officeChartDataPoint.DataFormat as ChartSerieDataFormatImpl;
						if (chartSerieDataFormatImpl.IsSupportFill)
						{
							series.ParentChart.IsParsed = true;
							(chartSerieDataFormatImpl.Fill as ChartFillImpl).InvertIfNegative = XmlConvertExtension.ToBoolean(reader.Value);
							if (chartSerieDataFormatImpl.Interior != null)
							{
								(chartSerieDataFormatImpl.Interior as ChartInteriorImpl).SwapColorsOnNegative = XmlConvertExtension.ToBoolean(reader.Value);
							}
							series.ParentChart.IsParsed = false;
							chartSerieDataFormatImpl.IsParsed = true;
						}
						break;
					}
					case "bubble3D":
						(officeChartDataPoint as ChartDataPointImpl).Bubble3D = ChartParserCommon.ParseBoolValueTag(reader);
						(officeChartDataPoint.DataFormat as ChartSerieDataFormatImpl).IsParsed = true;
						break;
					case "explosion":
						(officeChartDataPoint as ChartDataPointImpl).Explosion = ChartParserCommon.ParseIntValueTag(reader);
						(officeChartDataPoint.DataFormat as ChartSerieDataFormatImpl).IsParsed = true;
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
		reader.Read();
	}

	private IRange ParseSeriesValues(XmlReader reader, ChartSerieImpl series, out object[] values, bool isValueAxis, string tagName)
	{
		ChartImpl parentChart = series.ParentChart;
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		reader.Read();
		string text = null;
		string Filteredcategory = null;
		string filteredvalue = null;
		values = null;
		bool isNumReference = false;
		bool isStringReference = false;
		bool isMultiReference = false;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "numRef":
					isNumReference = true;
					text = ParseNumReference(reader, out values, out filteredvalue, series, parentChart, tagName);
					if (text != null && text.Split(',').Length > 1)
					{
						series.NumRefFormula = text;
					}
					if (values != null && tagName != "bubbleSize" && tagName != "cat")
					{
						series.EnteredDirectlyValues = values;
					}
					series.FilteredValue = filteredvalue;
					break;
				case "strRef":
					parentChart.IsStringRef = true;
					isStringReference = true;
					text = ParseStringReference(reader, out Filteredcategory, out values, parentChart, series, tagName);
					if (text != null && text.Split(',').Length > 1)
					{
						series.StrRefFormula = text;
					}
					series.FilteredCategory = Filteredcategory;
					break;
				case "multiLvlStrRef":
					isMultiReference = true;
					text = ParseMultiLevelStringReference(reader, series);
					if (text != null)
					{
						series.MulLvlStrRefFormula = text;
					}
					break;
				case "numLit":
					values = ParseDirectlyEnteredValues(reader, series, tagName);
					break;
				case "strLit":
					values = ParseDirectlyEnteredValues(reader, series, tagName);
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
		if (text != null && text.StartsWith("(") && text.EndsWith(")"))
		{
			text = text.Substring(1, text.Length - 2);
		}
		_ = series.ParentBook.Worksheets[0];
		IRange range = null;
		if (text != null)
		{
			WorkbookImpl parentBook = series.ParentBook;
			FormulaUtil formulaUtil = parentBook.DataHolder.Parser.FormulaUtil;
			if (!ChartImpl.TryAndModifyToValidFormula(text))
			{
				return null;
			}
			range = (formulaUtil.ParseString(text)[0] as IRangeGetter).GetRange(parentBook, parentBook.Worksheets[0]);
			if (range != null)
			{
				if (range is ExternalRange)
				{
					(range as ExternalRange).IsNumReference = isNumReference;
					(range as ExternalRange).IsStringReference = isStringReference;
					(range as ExternalRange).IsMultiReference = isMultiReference;
				}
				else if (range is RangeImpl)
				{
					(range as RangeImpl).IsNumReference = isNumReference;
					(range as RangeImpl).IsStringReference = isStringReference;
					(range as RangeImpl).IsMultiReference = isMultiReference;
				}
				else if (range is NameImpl)
				{
					(range as NameImpl).IsNumReference = isNumReference;
					(range as NameImpl).IsStringReference = isStringReference;
					(range as NameImpl).IsMultiReference = isMultiReference;
				}
			}
		}
		if (range != null && isValueAxis)
		{
			int count = parentChart.Categories.Count;
			if (count < range.Count)
			{
				for (int i = count; i < range.Count; i++)
				{
					(parentChart.Categories as ChartCategoryCollection).Add();
				}
			}
		}
		return range;
	}

	private string ParseNumReference(XmlReader reader, out object[] values, ChartSerieImpl series, string tagName)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "numRef")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		string text = null;
		values = null;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "f":
					if (text == null)
					{
						text = reader.ReadElementContentAsString();
					}
					else
					{
						reader.Skip();
					}
					break;
				case "numCache":
					values = ParseDirectlyEnteredValues(reader, series, tagName);
					if (values.Length == 0)
					{
						values = null;
					}
					break;
				case "extLst":
					reader.Read();
					if (!(reader.LocalName == "ext"))
					{
						break;
					}
					reader.Read();
					if (reader.LocalName == "fullRef")
					{
						reader.Read();
						if (reader.LocalName == "sqref")
						{
							text = reader.ReadElementContentAsString();
							reader.Skip();
						}
						reader.Skip();
					}
					if (reader.LocalName == "formulaRef")
					{
						reader.Read();
						if (reader.LocalName == "sqref")
						{
							text = reader.ReadElementContentAsString();
							reader.Skip();
						}
						reader.Skip();
					}
					reader.Read();
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
		return text;
	}

	private string ParseNumReference(XmlReader reader, out object[] values, out string filteredvalue, ChartSerieImpl series, ChartImpl chart, string tagName)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "numRef")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		string text = null;
		filteredvalue = null;
		values = null;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "f":
					if (text == null)
					{
						text = reader.ReadElementContentAsString();
						if (chart.CategoryFormula == null && !string.IsNullOrEmpty(text))
						{
							chart.CategoryFormula = text;
						}
					}
					else
					{
						filteredvalue = reader.ReadElementContentAsString();
					}
					break;
				case "numCache":
					values = ParseDirectlyEnteredValues(reader, series, tagName);
					if (values.Length == 0)
					{
						values = null;
					}
					break;
				case "extLst":
					reader.Read();
					if (!(reader.LocalName == "ext"))
					{
						break;
					}
					reader.Read();
					if (reader.LocalName == "fullRef")
					{
						reader.Read();
						if (reader.LocalName == "sqref")
						{
							text = reader.ReadElementContentAsString();
							reader.Skip();
						}
						if (reader.LocalName != "formulaRef")
						{
							reader.Skip();
						}
					}
					if (reader.LocalName == "formulaRef")
					{
						reader.Read();
						if (reader.LocalName == "sqref")
						{
							if (text == null)
							{
								text = reader.ReadElementContentAsString();
							}
							else
							{
								filteredvalue = reader.ReadElementContentAsString();
							}
							reader.Skip();
						}
						reader.Skip();
					}
					reader.Read();
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
		return text;
	}

	private string ParseStringReference(XmlReader reader, ChartSerieImpl series)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "strRef")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		string text = null;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "f":
					if (text == null)
					{
						text = reader.ReadElementContentAsString();
					}
					else
					{
						reader.Skip();
					}
					break;
				case "strCache":
					ParseDirectlyEnteredStringValue(reader, series);
					break;
				case "extLst":
					reader.Read();
					if (!(reader.LocalName == "ext"))
					{
						break;
					}
					reader.Read();
					if (reader.LocalName == "fullRef")
					{
						reader.Read();
						if (reader.LocalName == "sqref")
						{
							text = reader.ReadElementContentAsString();
							reader.Skip();
						}
						reader.Skip();
					}
					if (reader.LocalName == "formulaRef")
					{
						reader.Read();
						if (reader.LocalName == "sqref")
						{
							text = reader.ReadElementContentAsString();
							reader.Skip();
						}
						reader.Skip();
					}
					reader.Read();
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
		return text;
	}

	private string ParseStringReference(XmlReader reader, out string Filteredcategory, out object[] values, ChartImpl chart, ChartSerieImpl series, string tagName)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "strRef")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		Filteredcategory = string.Empty;
		reader.Read();
		string text = null;
		values = null;
		bool flag = false;
		ChartDataRange chartDataRange = chart.Series[0].CategoryLabels as ChartDataRange;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "f":
					if (text == null)
					{
						text = reader.ReadElementContentAsString();
						if (chart.CategoryFormula == null && !string.IsNullOrEmpty(text))
						{
							chart.CategoryFormula = text;
							flag = true;
						}
					}
					else
					{
						Filteredcategory = reader.ReadElementContentAsString();
					}
					break;
				case "strCache":
					if (chart.CategoryFormula != text || flag || text == null || (chartDataRange != null && chartDataRange.Range is NameImpl))
					{
						values = ParseDirectlyEnteredValues(reader, series, tagName);
						if (chart.CategoryLabelValues == null)
						{
							chart.CategoryLabelValues = values;
						}
						if (values.Length == 0)
						{
							values = null;
						}
					}
					else
					{
						values = null;
						reader.Skip();
					}
					break;
				case "extLst":
					reader.Read();
					if (!(reader.LocalName == "ext"))
					{
						break;
					}
					reader.Read();
					if (reader.LocalName == "fullRef")
					{
						reader.Read();
						if (reader.LocalName == "sqref")
						{
							text = reader.ReadElementContentAsString();
							reader.Skip();
						}
						if (reader.LocalName != "formulaRef")
						{
							reader.Skip();
						}
					}
					if (reader.LocalName == "formulaRef")
					{
						reader.Read();
						if (reader.LocalName == "sqref")
						{
							if (text == null)
							{
								text = reader.ReadElementContentAsString();
							}
							else
							{
								Filteredcategory = reader.ReadElementContentAsString();
							}
							reader.Skip();
						}
						reader.Skip();
					}
					reader.Read();
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
		return text;
	}

	private string ParseMultiLevelStringReference(XmlReader reader, ChartSerieImpl series)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "multiLvlStrRef")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		string result = null;
		while (!reader.LocalName.Equals("multiLvlStrRef"))
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName = reader.LocalName;
				if (!(localName == "f"))
				{
					if (localName == "multiLvlStrCache")
					{
						series.MultiLevelStrCache = ParseMultiLevelStringCache(reader, series, "multiLvlStrCache");
					}
					else
					{
						reader.Skip();
					}
				}
				else
				{
					result = reader.ReadElementContentAsString();
				}
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Read();
		return result;
	}

	private Dictionary<int, object[]> ParseMultiLevelStringCache(XmlReader reader, ChartSerieImpl series, string tagName)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		int num = 0;
		reader.Read();
		Dictionary<int, object[]> dictionary = new Dictionary<int, object[]>();
		List<object> list = new List<object>();
		int num2 = 0;
		bool flag = true;
		if (reader.NodeType == XmlNodeType.EndElement)
		{
			return dictionary;
		}
		while (flag)
		{
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					switch (reader.LocalName)
					{
					case "pt":
					{
						string formatCode = null;
						int num3 = AddNumericPoint(reader, list, flag, ref formatCode);
						if (!string.IsNullOrEmpty(formatCode) && tagName == "val")
						{
							series.FormatValueCodes.Add(num3, formatCode);
						}
						else if (!string.IsNullOrEmpty(formatCode) && tagName == "cat")
						{
							series.FormatCategoryCodes.Add(num3, formatCode);
						}
						if (num3 != -1 && num3 > num)
						{
							do
							{
								list.Insert(num, null);
								num++;
							}
							while (num != num3 && num3 > num);
						}
						num++;
						break;
					}
					case "ptCount":
						if (reader.MoveToAttribute("val"))
						{
							series.PointCount = XmlConvertExtension.ToInt32(reader.Value);
						}
						break;
					case "lvl":
						reader.Read();
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
			if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName.Equals("multiLvlStrCache"))
			{
				flag = false;
			}
			if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName.Equals("lvl"))
			{
				dictionary.Add(num2, list.ToArray());
				list.Clear();
				num = 0;
				num2++;
				reader.Read();
			}
		}
		reader.Read();
		return dictionary;
	}

	private void ParseMarker(XmlReader reader, ChartSerieImpl series, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		if (reader.LocalName != "marker")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		IOfficeChartSerieDataFormat serieFormat = series.SerieFormat;
		ParseMarker(reader, serieFormat, parser);
	}

	private void ParseMarker(XmlReader reader, IOfficeChartSerieDataFormat dataFormat, Excel2007Parser parser)
	{
		bool isAutoColor = true;
		bool flag = true;
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			Excel2007ChartMarkerType excel2007ChartMarkerType = Excel2007ChartMarkerType.none;
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					switch (reader.LocalName)
					{
					case "symbol":
					{
						string value = ChartParserCommon.ParseValueTag(reader);
						excel2007ChartMarkerType = (Excel2007ChartMarkerType)Enum.Parse(typeof(Excel2007ChartMarkerType), value, ignoreCase: false);
						flag = false;
						dataFormat.MarkerStyle = (OfficeChartMarkerType)excel2007ChartMarkerType;
						(dataFormat as ChartSerieDataFormatImpl).HasMarkerProperties = true;
						isAutoColor = (dataFormat as ChartSerieDataFormatImpl).IsAutoMarker;
						break;
					}
					case "size":
						dataFormat.MarkerSize = ChartParserCommon.ParseIntValueTag(reader);
						break;
					case "spPr":
						isAutoColor = false;
						ParseMarkerFill(reader, dataFormat, parser);
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
		if (flag)
		{
			(dataFormat as ChartSerieDataFormatImpl).m_isMarkerDefaultSymbol = true;
		}
		((ChartSerieDataFormatImpl)dataFormat).MarkerFormat.IsAutoColor = isAutoColor;
		reader.Read();
	}

	private void ParseMarkerFill(XmlReader reader, IOfficeChartSerieDataFormat serieDataFormat, Excel2007Parser parser)
	{
		string localName = reader.LocalName;
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			ChartSerieDataFormatImpl chartSerieDataFormatImpl = serieDataFormat as ChartSerieDataFormatImpl;
			while (reader.NodeType != XmlNodeType.EndElement && reader.LocalName != localName)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					switch (reader.LocalName)
					{
					case "solidFill":
					{
						int Alpha = 100000;
						ChartParserCommon.ParseSolidFill(reader, parser, chartSerieDataFormatImpl.MarkerBackColorObject, out Alpha);
						chartSerieDataFormatImpl.Fill.Transparency = 1.0 - (double)Alpha / 100000.0;
						break;
					}
					case "gradFill":
					{
						GradientStops gradientStops = ChartParserCommon.ParseGradientFill(reader, parser);
						ChartMarkerFormatRecord markerFormat = chartSerieDataFormatImpl.MarkerFormat;
						markerFormat.IsAutoColor = false;
						markerFormat.IsNotShowInt = false;
						markerFormat.IsNotShowBrd = false;
						chartSerieDataFormatImpl.MarkerBackColorObject.CopyFrom(gradientStops[0].ColorObject, callEvent: true);
						chartSerieDataFormatImpl.MarkerGradient = gradientStops;
						break;
					}
					case "ln":
						chartSerieDataFormatImpl.MarkerFormat.FlagOptions |= 2;
						chartSerieDataFormatImpl.MarkerFormat.IsNotShowBrd = !ParseMarkerLine(reader, chartSerieDataFormatImpl.MarkerForeColorObject, parser, chartSerieDataFormatImpl);
						break;
					case "noFill":
						chartSerieDataFormatImpl.MarkerFormat.IsNotShowInt = true;
						reader.Read();
						break;
					case "effectLst":
						chartSerieDataFormatImpl.EffectListStream = ShapeParser.ReadNodeAsStream(reader);
						chartSerieDataFormatImpl.EffectListStream.Position = 0L;
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

	private bool ParseMarkerLine(XmlReader reader, ChartColor color, Excel2007Parser parser, ChartSerieDataFormatImpl format)
	{
		bool result = false;
		int Alpha = 100000;
		Stream stream = ShapeParser.ReadNodeAsStream(reader);
		stream.Position = 0L;
		reader = UtilityMethods.CreateReader(stream);
		format.MarkerLineStream = stream;
		if (reader.MoveToAttribute("w"))
		{
			format.MarkerLineWidth = (double)int.Parse(reader.Value) / 12700.0;
			reader.MoveToElement();
		}
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					if (reader.LocalName == "solidFill")
					{
						ChartParserCommon.ParseSolidFill(reader, parser, color, out Alpha);
						format.MarkerTransparency = (float)Alpha / 100000f;
						result = true;
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
		return result;
	}

	private void ParseUpDownBars(XmlReader reader, ChartSerieImpl series, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		if (reader.LocalName != "upDownBars")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		ChartFormatImpl chartFormatImpl = (ChartFormatImpl)series.SerieFormat.CommonSerieOptions;
		FileDataHolder dataHolder = series.ParentBook.DataHolder;
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "gapWidth":
					chartFormatImpl.FirstDropBar.Gap = ChartParserCommon.ParseIntValueTag(reader);
					break;
				case "upBars":
				{
					IOfficeChartDropBar firstDropBar = chartFormatImpl.FirstDropBar;
					ParseDropBar(reader, firstDropBar, dataHolder, relations);
					break;
				}
				case "downBars":
				{
					IOfficeChartDropBar secondDropBar = chartFormatImpl.SecondDropBar;
					ParseDropBar(reader, secondDropBar, dataHolder, relations);
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
		reader.Read();
	}

	private void ParseDropBar(XmlReader reader, IOfficeChartDropBar dropBar, FileDataHolder dataHolder, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (dropBar == null)
		{
			throw new ArgumentNullException("dropBar");
		}
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "spPr")
				{
					ChartFillObjectGetterAny objectGetter = new ChartFillObjectGetterAny(dropBar.LineProperties as ChartBorderImpl, dropBar.Interior as ChartInteriorImpl, dropBar.Fill as IInternalFill, dropBar.Shadow as ShadowImpl, dropBar.ThreeD as ThreeDFormatImpl);
					ChartParserCommon.ParseShapeProperties(reader, objectGetter, dataHolder, relations);
				}
				else
				{
					reader.Skip();
				}
			}
		}
		reader.Read();
	}

	private void ParseDataTable(XmlReader reader, ChartImpl chart)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "dTable")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		Excel2007Parser parser = chart.ParentWorkbook.DataHolder.Parser;
		chart.HasDataTable = true;
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			IOfficeChartDataTable dataTable = chart.DataTable;
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					switch (reader.LocalName)
					{
					case "showHorzBorder":
						dataTable.HasHorzBorder = ChartParserCommon.ParseBoolValueTag(reader);
						break;
					case "showVertBorder":
						dataTable.HasVertBorder = ChartParserCommon.ParseBoolValueTag(reader);
						break;
					case "showOutline":
						dataTable.HasBorders = ChartParserCommon.ParseBoolValueTag(reader);
						break;
					case "showKeys":
						dataTable.ShowSeriesKeys = ChartParserCommon.ParseBoolValueTag(reader);
						break;
					case "txPr":
					{
						IInternalOfficeChartTextArea textFormatting = dataTable.TextArea as IInternalOfficeChartTextArea;
						ParseDefaultTextFormatting(reader, textFormatting, parser);
						break;
					}
					case "spPr":
						(dataTable as ChartDataTableImpl).HasShapeProperties = true;
						(dataTable as ChartDataTableImpl).shapeStream = chart.ParentWorkbook.DataHolder.Parser.ReadSingleNodeIntoStream(reader);
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

	internal void ParseSeriesProperties(XmlReader reader, ChartSerieImpl series, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		if (reader.LocalName != "spPr")
		{
			throw new XmlException("Unexpected xml tag");
		}
		ChartSerieDataFormatImpl dataFormat = (ChartSerieDataFormatImpl)series.SerieFormat;
		FileDataHolder parentHolder = series.ParentChart.DataHolder.ParentHolder;
		ChartFillObjectGetter objectGetter = new ChartFillObjectGetter(dataFormat);
		ChartParserCommon.ParseShapeProperties(reader, objectGetter, parentHolder, relations);
	}

	private void ParseDefaultTextFormatting(XmlReader reader, IInternalOfficeChartTextArea textFormatting, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textFormatting == null)
		{
			throw new ArgumentNullException("textFormatting");
		}
		if (reader.LocalName != "txPr")
		{
			throw new XmlException("Unexpected xml tag");
		}
		textFormatting.ParagraphType = ChartParagraphType.CustomDefault;
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "bodyPr":
					if (reader.MoveToAttribute("rot"))
					{
						textFormatting.TextRotationAngle = XmlConvertExtension.ToInt32(reader.Value) / 60000;
					}
					reader.Skip();
					break;
				case "defRPr":
					ChartParserCommon.ParseParagraphRunProperites(reader, textFormatting, parser, null);
					while (reader.LocalName != "txPr")
					{
						reader.Read();
					}
					break;
				case "p":
					while (reader.NodeType != XmlNodeType.EndElement && reader.LocalName != "txPr" && reader.LocalName != "defRPr")
					{
						reader.Read();
					}
					if (!(reader.LocalName == "defRPr"))
					{
						break;
					}
					if (!(this is ChartExParser) || !(textFormatting.Parent is ChartLegendImpl))
					{
						ChartParserCommon.ParseParagraphRunProperites(reader, textFormatting, parser, null);
					}
					while (reader.LocalName != "txPr")
					{
						if (reader.NodeType != XmlNodeType.EndElement)
						{
							if (reader.LocalName == "endParaRPr")
							{
								if (this is ChartExParser && textFormatting.Parent is ChartLegendImpl && textFormatting != null)
								{
									textFormatting.Font.Size = 9.0;
									ChartParserCommon.ParseParagraphRunProperites(reader, textFormatting, parser, null);
								}
								reader.Read();
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
		reader.Read();
	}

	private object[] ParseDirectlyEnteredValues(XmlReader reader, ChartSerieImpl series, string tagName)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		bool flag = false;
		bool flag2 = false;
		int num = 0;
		if (reader.LocalName == "strLit" || reader.LocalName == "strCache")
		{
			flag = true;
		}
		if (reader.LocalName == "numLit")
		{
			flag2 = true;
		}
		int num2 = 0;
		reader.Read();
		List<object> list = new List<object>();
		if (reader.NodeType == XmlNodeType.EndElement)
		{
			return list.ToArray();
		}
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "formatCode":
					if (tagName == "cat" || tagName == "xVal")
					{
						series.CategoriesFormatCode = reader.ReadElementContentAsString();
					}
					else
					{
						series.FormatCode = reader.ReadElementContentAsString();
					}
					break;
				case "pt":
				{
					string formatCode = null;
					int num3 = AddNumericPoint(reader, list, flag, ref formatCode);
					if (!string.IsNullOrEmpty(formatCode) && tagName == "val")
					{
						series.FormatValueCodes.Add(num3, formatCode);
					}
					else if (!string.IsNullOrEmpty(formatCode) && tagName == "cat")
					{
						series.FormatCategoryCodes.Add(num3, formatCode);
					}
					if (num3 != -1 && num3 > num2)
					{
						do
						{
							if (flag)
							{
								list.Insert(num2, "");
							}
							else
							{
								list.Insert(num2, null);
							}
							num2++;
						}
						while (num2 != num3 && num3 > num2);
					}
					num2++;
					break;
				}
				case "ptCount":
					if (reader.MoveToAttribute("val"))
					{
						num = XmlConvertExtension.ToInt32(reader.Value);
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
		if (flag && list.Count == 0 && num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				list.Add("");
			}
		}
		if (!flag2 && series.ParentChart.HasExternalWorkbook && num > list.Count)
		{
			for (int j = list.Count; j < num; j++)
			{
				list.Add("");
			}
		}
		reader.Read();
		return list.ToArray();
	}

	internal int AddNumericPoint(XmlReader reader, List<object> list, bool isString, ref string formatCode)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (reader.LocalName != "pt")
		{
			throw new XmlException();
		}
		int result = -1;
		if (reader.MoveToAttribute("idx"))
		{
			result = XmlConvertExtension.ToInt32(reader.Value);
		}
		if (reader.MoveToAttribute("formatCode"))
		{
			formatCode = reader.Value;
		}
		if (this is ChartExParser)
		{
			reader.MoveToElement();
			list.Add(ReadXmlValue(reader, isString));
			Excel2007Parser.SkipWhiteSpaces(reader);
		}
		else if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					if (reader.LocalName == "v")
					{
						list.Add(ReadXmlValue(reader, isString));
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
		return result;
	}

	internal int AddNumericPoint(XmlReader reader, Dictionary<int, object> list, bool isString, ref string formatCode)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (reader.LocalName != "pt")
		{
			throw new XmlException();
		}
		int num = -1;
		if (reader.MoveToAttribute("idx"))
		{
			num = XmlConvertExtension.ToInt32(reader.Value);
		}
		if (reader.MoveToAttribute("formatCode"))
		{
			formatCode = reader.Value;
		}
		if (this is ChartExParser)
		{
			reader.MoveToElement();
			list.Add(num, ReadXmlValue(reader, isString));
			Excel2007Parser.SkipWhiteSpaces(reader);
		}
		else if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					if (reader.LocalName == "v")
					{
						list.Add(num, ReadXmlValue(reader, isString));
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
		return num;
	}

	private object ReadXmlValue(XmlReader reader, bool isString)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		string text = reader.ReadElementContentAsString();
		if (!isString && !string.IsNullOrEmpty(text))
		{
			return XmlConvertExtension.ToDouble(text);
		}
		return text;
	}
}
