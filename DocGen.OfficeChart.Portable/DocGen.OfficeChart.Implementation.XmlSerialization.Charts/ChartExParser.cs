using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using DocGen.Compression;
using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Implementation.XmlReaders;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Interfaces.Charts;

namespace DocGen.OfficeChart.Implementation.XmlSerialization.Charts;

internal class ChartExParser : ChartParser
{
	public ChartExParser(WorkbookImpl book)
		: base(book)
	{
	}

	internal void ParseChartEx(XmlReader reader, ChartImpl chart, RelationCollection relations)
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
		reader.Read();
		IOfficeChartFrameFormat chartArea = chart.ChartArea;
		chartArea.Interior.UseAutomaticFormat = true;
		chartArea.Border.AutoFormat = true;
		Dictionary<int, ChartExDataCache> dictionary = null;
		Dictionary<int, int> dictionary2 = null;
		while (reader.NodeType != XmlNodeType.EndElement && reader.LocalName != "chartSpace")
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "chartData":
					dictionary = ParseChartExData(reader, chart, relations);
					break;
				case "chart":
					dictionary2 = ParseChartExElement(reader, chart, relations);
					break;
				case "spPr":
				{
					IChartFillObjectGetter objectGetter = new ChartFillObjectGetterAny(chartArea.Border as ChartBorderImpl, chartArea.Interior as ChartInteriorImpl, chartArea.Fill as IInternalFill, chartArea.Shadow as ShadowImpl, chartArea.ThreeD as ThreeDFormatImpl);
					ChartParserCommon.ParseShapeProperties(reader, objectGetter, chart.ParentWorkbook.DataHolder, relations);
					break;
				}
				case "txPr":
					ParseDefaultTextProperties(reader, chart);
					break;
				case "printSettings":
					ParsePrintSettings(reader, chart, relations);
					break;
				case "clrMapOvr":
				{
					MemoryStream memoryStream = new MemoryStream();
					XmlWriter xmlWriter = UtilityMethods.CreateWriter(memoryStream, Encoding.UTF8);
					xmlWriter.WriteNode(reader, defattr: false);
					xmlWriter.Flush();
					chart.m_colorMapOverrideStream = memoryStream;
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
				if (reader.LocalName == "")
				{
					break;
				}
			}
		}
		if (dictionary != null && dictionary2 != null)
		{
			foreach (int key2 in dictionary2.Keys)
			{
				int key = dictionary2[key2];
				dictionary[key].CopyProperties(chart.Series[key2] as ChartSerieImpl, chart.ParentWorkbook);
			}
			dictionary.Clear();
			dictionary = null;
		}
		if (chart != null && chart.Shapes.Count > 0)
		{
			CalculateShapesPosition(chart, chart.Width, chart.Height);
		}
	}

	private Dictionary<int, ChartExDataCache> ParseChartExData(XmlReader reader, ChartImpl chart, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "chartData")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		Dictionary<int, ChartExDataCache> dictionary = new Dictionary<int, ChartExDataCache>();
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName = reader.LocalName;
				if (!(localName == "data"))
				{
					if (localName == "externalData")
					{
						ParseExternalDataAttributes(reader, chart);
					}
					else
					{
						reader.Skip();
					}
					continue;
				}
				KeyValuePair<int, ChartExDataCache> keyValuePair = TryParseChartExDataCache(reader, chart, relations);
				if (keyValuePair.Value != null)
				{
					dictionary.Add(keyValuePair.Key, keyValuePair.Value);
				}
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Read();
		return dictionary;
	}

	private void ParseExternalDataAttributes(XmlReader reader, ChartImpl chart)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "externalData")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		if (reader.MoveToAttribute("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships"))
		{
			chart.ChartExRelationId = reader.Value;
		}
		if (reader.MoveToAttribute("autoUpdate", "http://schemas.microsoft.com/office/drawing/2014/chartex"))
		{
			chart.AutoUpdate = XmlConvertExtension.ToBoolean(reader.Value);
		}
		reader.Skip();
	}

	private KeyValuePair<int, ChartExDataCache> TryParseChartExDataCache(XmlReader reader, ChartImpl chart, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "data")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		KeyValuePair<int, ChartExDataCache> result = default(KeyValuePair<int, ChartExDataCache>);
		if (reader.MoveToAttribute("id"))
		{
			ChartExDataCache chartExDataCache = new ChartExDataCache();
			result = new KeyValuePair<int, ChartExDataCache>(XmlConvertExtension.ToInt32(reader.Value), chartExDataCache);
			reader.MoveToElement();
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					string localName = reader.LocalName;
					if (!(localName == "numDim"))
					{
						if (localName == "strDim")
						{
							ParseDimensionData(reader, chartExDataCache);
						}
						else
						{
							reader.Skip();
						}
					}
					else
					{
						ParseDimensionData(reader, chartExDataCache);
					}
				}
				else
				{
					reader.Skip();
				}
			}
		}
		else
		{
			reader.Skip();
		}
		reader.Read();
		return result;
	}

	private void ParseDimensionData(XmlReader reader, ChartExDataCache cache)
	{
		List<object> list = new List<object>();
		List<object> list2 = new List<object>();
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (cache == null)
		{
			throw new ArgumentNullException("cache");
		}
		if (reader.LocalName != "strDim" && reader.LocalName != "numDim")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		bool flag = false;
		if (reader.MoveToAttribute("type"))
		{
			if (reader.Value == "cat")
			{
				flag = true;
			}
			reader.MoveToElement();
		}
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					string localName = reader.LocalName;
					if (!(localName == "f"))
					{
						if (localName == "lvl")
						{
							ParseChartExLevelElement(reader, cache, flag, list, list2);
						}
						else
						{
							reader.Skip();
						}
						continue;
					}
					if (reader.MoveToAttribute("dir"))
					{
						if (reader.Value.ToLower() == "row")
						{
							if (flag)
							{
								cache.IsRowWiseCategory = true;
							}
							else
							{
								cache.IsRowWiseSeries = true;
							}
						}
						reader.MoveToElement();
					}
					string text = reader.ReadElementContentAsString();
					if (flag)
					{
						cache.CategoryFormula = text;
					}
					else
					{
						cache.SeriesFormula = text;
					}
					Excel2007Parser.SkipWhiteSpaces(reader);
					if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "f")
					{
						reader.Read();
					}
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
		list.Clear();
		list2.Clear();
	}

	private void ParseChartExLevelElement(XmlReader reader, ChartExDataCache cache, bool isCategoryValues, List<object> category, List<object> Value)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (cache == null)
		{
			throw new ArgumentNullException("cache");
		}
		if (reader.LocalName != "lvl")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		List<object> list = new List<object>();
		if (reader.MoveToAttribute("formatCode"))
		{
			if (isCategoryValues)
			{
				cache.CategoriesFormatCode = reader.Value;
			}
			else
			{
				cache.SeriesFormatCode = reader.Value;
			}
		}
		if (reader.MoveToAttribute("ptCount"))
		{
			XmlConvertExtension.ToInt32(reader.Value);
		}
		reader.MoveToElement();
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					if (reader.LocalName == "pt")
					{
						string formatCode = null;
						AddNumericPoint(reader, list, isCategoryValues, ref formatCode);
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
		else
		{
			reader.Skip();
		}
		if (isCategoryValues)
		{
			foreach (object item in list)
			{
				category.Add(item);
			}
			cache.CategoryValues = category.ToArray();
		}
		else
		{
			foreach (object item2 in list)
			{
				Value.Add(item2);
			}
			cache.SeriesValues = Value.ToArray();
		}
		reader.Read();
	}

	private Dictionary<int, int> ParseChartExElement(XmlReader reader, ChartImpl chart, RelationCollection relations)
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
		Dictionary<int, int> result = null;
		MemoryStream memoryStream = new MemoryStream();
		XmlWriter xmlWriter = UtilityMethods.CreateWriter(memoryStream, Encoding.UTF8);
		xmlWriter.WriteStartElement("root");
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "plotArea":
					result = ParseChartExPlotArea(reader, chart, relations, parentHolder.Parser);
					break;
				case "legend":
					chart.HasLegend = true;
					ParseLegend(reader, chart.Legend as ChartLegendImpl, chart, relations);
					break;
				case "title":
				{
					bool? isOverlay = false;
					ushort position = 0;
					TryParsePositioningValues(reader, out isOverlay, out position);
					if (isOverlay.HasValue)
					{
						chart.ChartTitleIncludeInLayout = isOverlay.Value;
					}
					if (position != 0)
					{
						chart.ChartExTitlePosition = position;
					}
					reader.MoveToElement();
					xmlWriter.WriteNode(reader, defattr: false);
					xmlWriter.Flush();
					flag = true;
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
		xmlWriter.WriteEndElement();
		xmlWriter.Flush();
		if (flag)
		{
			chart.ChartTitleArea.Bold = false;
			ChartParserCommon.ParseChartTitleElement(memoryStream, chart.ChartTitleArea as IInternalOfficeChartTextArea, parentHolder, relations, 18f);
			chart.HasTitle = true;
		}
		reader.Read();
		return result;
	}

	private Dictionary<int, int> ParseChartExPlotArea(XmlReader reader, ChartImpl chart, RelationCollection relations, Excel2007Parser excel2007Parser)
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
		reader.Read();
		chart.HasPlotArea = false;
		bool isBorderCornersRound = chart.PlotArea != null && chart.ChartArea.IsBorderCornersRound;
		FileDataHolder parentHolder = chart.DataHolder.ParentHolder;
		int secondaryAxisId = -1;
		List<int> list = new List<int>(3);
		ChartExAxisParser chartExAxisParser = null;
		Dictionary<int, int> result = null;
		while (reader.NodeType != XmlNodeType.EndElement && reader.NodeType != 0)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "spPr":
					if (!chart.HasPlotArea)
					{
						chart.HasPlotArea = true;
						IOfficeChartFrameFormat plotArea = chart.PlotArea;
						chart.ChartArea.IsBorderCornersRound = isBorderCornersRound;
						ChartFillObjectGetterAny objectGetter = new ChartFillObjectGetterAny(plotArea.Border as ChartBorderImpl, plotArea.Interior as ChartInteriorImpl, plotArea.Fill as IInternalFill, plotArea.Shadow as ShadowImpl, plotArea.ThreeD as ThreeDFormatImpl);
						ChartParserCommon.ParseShapeProperties(reader, objectGetter, parentHolder, relations);
					}
					else
					{
						reader.Skip();
					}
					break;
				case "plotAreaRegion":
					result = ParsePlotAreaRegion(reader, chart, relations, excel2007Parser, out secondaryAxisId);
					break;
				case "axis":
				{
					if (chartExAxisParser == null)
					{
						chartExAxisParser = new ChartExAxisParser(chart.ParentWorkbook);
					}
					int num = ParseChartExAxes(reader, secondaryAxisId, list, chartExAxisParser, chart, parentHolder, excel2007Parser, relations);
					if (num != -1 && list.Count < 3)
					{
						list.Add(num);
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
				reader.Skip();
			}
		}
		reader.Read();
		return result;
	}

	private Dictionary<int, int> ParsePlotAreaRegion(XmlReader reader, ChartImpl chart, RelationCollection relations, Excel2007Parser excel2007Parser, out int secondaryAxisId)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "plotAreaRegion")
		{
			throw new XmlException("Unexpected xml tag");
		}
		reader.Read();
		secondaryAxisId = -1;
		int num = chart.Series.Count;
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		while (reader.NodeType != XmlNodeType.EndElement && reader.NodeType != 0)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName = reader.LocalName;
				if (!(localName == "series"))
				{
					if (localName == "plotSurface")
					{
						if (!reader.IsEmptyElement)
						{
							ParsePlotSurface(reader, chart, relations, excel2007Parser);
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
					continue;
				}
				int secondaryAxisId2 = -1;
				int value = ParseChartExSeries(reader, chart, relations, excel2007Parser, out secondaryAxisId2);
				if (secondaryAxisId != secondaryAxisId2)
				{
					secondaryAxisId = secondaryAxisId2;
				}
				if (num < chart.Series.Count)
				{
					dictionary.Add(num, value);
					num++;
				}
			}
			else
			{
				reader.Skip();
			}
		}
		if (chart.Series.Count > 0 && chart.IsHistogramOrPareto && (chart.Series[0].SerieFormat as ChartSerieDataFormatImpl).HistogramAxisFormatProperty != null)
		{
			(chart.PrimaryCategoryAxis as ChartCategoryAxisImpl).HistogramAxisFormatProperty.Clone((chart.Series[0].SerieFormat as ChartSerieDataFormatImpl).HistogramAxisFormatProperty);
		}
		reader.Read();
		return dictionary;
	}

	private int ParseChartExSeries(XmlReader reader, ChartImpl chart, RelationCollection relations, Excel2007Parser excel2007Parser, out int secondaryAxisId)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "series")
		{
			throw new XmlException("Unexpected xml tag");
		}
		ChartFrameFormatImpl paretoLineFormat = null;
		ChartSerieImpl chartSerieImpl = TryParseSeriesFromAttributes(reader, chart, out paretoLineFormat);
		secondaryAxisId = -1;
		int result = 0;
		if (chartSerieImpl != null)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement && reader.NodeType != 0)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					switch (reader.LocalName)
					{
					case "tx":
						if (!reader.IsEmptyElement)
						{
							while (reader.NodeType != XmlNodeType.EndElement)
							{
								if (reader.LocalName == "txData")
								{
									string formula = null;
									chartSerieImpl.Name = ChartParserCommon.ParseFormulaOrValue(reader, out formula);
									if (formula != null && formula != "")
									{
										if (ChartParser.GetRange(chart.ParentWorkbook, formula) is IName { RefersToRange: not null } name)
										{
											chartSerieImpl.Name = "=" + name.RefersToRange.AddressGlobal;
										}
										else
										{
											chartSerieImpl.Name = "=" + formula;
										}
									}
									reader.Read();
								}
								else
								{
									reader.Read();
								}
								if (reader.LocalName == "tx" && reader.NodeType == XmlNodeType.EndElement)
								{
									reader.Read();
									break;
								}
							}
						}
						else
						{
							reader.Skip();
						}
						break;
					case "spPr":
						ParseSeriesProperties(reader, chartSerieImpl, relations);
						break;
					case "dataPt":
						ParseDataPoint(reader, chartSerieImpl, relations);
						break;
					case "dataLabels":
						ParseDataLabels(reader, chartSerieImpl, relations, isChartExSeries: true);
						break;
					case "dataId":
						if (reader.MoveToAttribute("val"))
						{
							result = XmlConvertExtension.ToInt32(reader.Value);
						}
						reader.Skip();
						break;
					case "layoutPr":
						ParseChartExSeriesLayoutProperties(reader, chartSerieImpl, relations);
						break;
					case "axisId":
					{
						int num = -1;
						if (reader.MoveToAttribute("val"))
						{
							num = XmlConvertExtension.ToInt32(reader.Value);
						}
						if (num != -1)
						{
							(chart.PrimaryValueAxis as ChartAxisImpl).AxisId = num;
							if (paretoLineFormat != null)
							{
								secondaryAxisId = num;
							}
						}
						reader.Skip();
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
		else if (paretoLineFormat != null)
		{
			secondaryAxisId = ParseParetoLineFormat(reader, paretoLineFormat, chart.ParentWorkbook.DataHolder, relations);
		}
		else
		{
			reader.Skip();
		}
		return result;
	}

	private int ParseParetoLineFormat(XmlReader reader, ChartFrameFormatImpl paretoLineFormat, FileDataHolder fileDataHolder, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (paretoLineFormat == null)
		{
			throw new ArgumentNullException("Series Data Format");
		}
		if (reader.LocalName != "series")
		{
			throw new XmlException("Unexpected xml tag");
		}
		reader.Read();
		int result = -1;
		while (reader.NodeType != XmlNodeType.EndElement && reader.NodeType != 0)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName = reader.LocalName;
				if (!(localName == "spPr"))
				{
					if (localName == "axisId")
					{
						if (reader.MoveToAttribute("val"))
						{
							result = XmlConvertExtension.ToInt32(reader.Value);
						}
						reader.Skip();
					}
					else
					{
						reader.Skip();
					}
				}
				else
				{
					ChartFillObjectGetterAny objectGetter = new ChartFillObjectGetterAny(paretoLineFormat.Border as ChartBorderImpl, paretoLineFormat.Interior as ChartInteriorImpl, paretoLineFormat.Fill as IInternalFill, paretoLineFormat.Shadow as ShadowImpl, paretoLineFormat.ThreeD as ThreeDFormatImpl);
					ChartParserCommon.ParseShapeProperties(reader, objectGetter, fileDataHolder, relations);
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

	private ChartSerieImpl TryParseSeriesFromAttributes(XmlReader reader, ChartImpl chart, out ChartFrameFormatImpl paretoLineFormat)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "series")
		{
			throw new XmlException("Unexpected xml tag");
		}
		ChartSerieImpl chartSerieImpl = null;
		paretoLineFormat = null;
		int num = -1;
		if (reader.MoveToAttribute("layoutId"))
		{
			OfficeChartType officeChartType;
			if (reader.Value == "clusteredColumn")
			{
				officeChartType = OfficeChartType.Histogram;
				chart.ChartType = OfficeChartType.Histogram;
			}
			else if (reader.Value == "paretoLine")
			{
				officeChartType = OfficeChartType.Pareto;
				chart.ChartType = OfficeChartType.Pareto;
			}
			else if (reader.Value == "boxWhisker")
			{
				officeChartType = OfficeChartType.BoxAndWhisker;
				chart.ChartType = OfficeChartType.BoxAndWhisker;
			}
			else
			{
				if (reader.Value == "regionMap")
				{
					return null;
				}
				officeChartType = (OfficeChartType)Enum.Parse(typeof(OfficeChartType), reader.Value.ToLowerInvariant(), ignoreCase: true);
			}
			if (officeChartType != OfficeChartType.Pareto)
			{
				chartSerieImpl = chart.Series.Add(officeChartType) as ChartSerieImpl;
				chartSerieImpl.Number = -1;
				if (officeChartType == OfficeChartType.Histogram)
				{
					(chartSerieImpl.SerieFormat as ChartSerieDataFormatImpl).HistogramAxisFormatProperty = new HistogramAxisFormat();
				}
			}
		}
		if (reader.MoveToAttribute("ownerIdx") && chartSerieImpl == null)
		{
			num = XmlConvertExtension.ToInt32(reader.Value);
			chart.Series[num].SerieType = OfficeChartType.Pareto;
			paretoLineFormat = (chart.Series[num] as ChartSerieImpl).ParetoLineFormat as ChartFrameFormatImpl;
		}
		if (reader.MoveToAttribute("formatIdx"))
		{
			if (chartSerieImpl != null)
			{
				chartSerieImpl.Number = XmlConvertExtension.ToInt32(reader.Value);
			}
			else if (paretoLineFormat != null)
			{
				(paretoLineFormat.Parent as ChartSerieImpl).ParetoLineFormatIndex = XmlConvertExtension.ToInt32(reader.Value);
			}
		}
		if (reader.MoveToAttribute("hidden"))
		{
			if (chartSerieImpl != null)
			{
				chartSerieImpl.IsSeriesHidden = XmlConvertExtension.ToBoolean(reader.Value);
			}
			else if (paretoLineFormat != null)
			{
				(paretoLineFormat.Parent as ChartSerieImpl).IsParetoLineHidden = XmlConvertExtension.ToBoolean(reader.Value);
			}
		}
		reader.MoveToElement();
		return chartSerieImpl;
	}

	private void ParseChartExSeriesLayoutProperties(XmlReader reader, ChartSerieImpl series, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		if (reader.LocalName != "layoutPr")
		{
			throw new XmlException("Unexpected xml tag");
		}
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			ChartSerieDataFormatImpl chartSerieDataFormatImpl = series.SerieFormat as ChartSerieDataFormatImpl;
			while (reader.NodeType != XmlNodeType.EndElement && reader.NodeType != 0)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					switch (reader.LocalName)
					{
					case "parentLabelLayout":
						if (reader.MoveToAttribute("val"))
						{
							chartSerieDataFormatImpl.TreeMapLabelOption = (TreeMapLabelOption)Enum.Parse(typeof(TreeMapLabelOption), reader.Value, ignoreCase: true);
						}
						reader.Skip();
						break;
					case "visibility":
						ParseChartSeriesVisibility(reader, chartSerieDataFormatImpl);
						break;
					case "aggregation":
						chartSerieDataFormatImpl.IsBinningByCategory = true;
						reader.Skip();
						break;
					case "binning":
						ParseSeriesBinningProperties(reader, chartSerieDataFormatImpl);
						break;
					case "statistics":
						if (reader.MoveToAttribute("quartileMethod"))
						{
							if (reader.Value == "inclusive")
							{
								chartSerieDataFormatImpl.QuartileCalculationType = QuartileCalculation.InclusiveMedian;
							}
							else
							{
								chartSerieDataFormatImpl.QuartileCalculationType = QuartileCalculation.ExclusiveMedian;
							}
						}
						break;
					case "subtotals":
						if (!reader.IsEmptyElement)
						{
							reader.Read();
							while (reader.NodeType != XmlNodeType.EndElement)
							{
								if (reader.LocalName == "idx" && reader.MoveToAttribute("val"))
								{
									int index = XmlConvertExtension.ToInt32(reader.Value);
									series.DataPoints[index].SetAsTotal = true;
									reader.Skip();
								}
								else
								{
									reader.Skip();
								}
								if (reader.LocalName == "subtotals" && reader.NodeType == XmlNodeType.EndElement)
								{
									reader.Read();
									break;
								}
							}
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
		}
		reader.Read();
	}

	private void ParseSeriesBinningProperties(XmlReader reader, ChartSerieDataFormatImpl dataFormat)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (dataFormat == null)
		{
			throw new ArgumentNullException("Serie Data Format");
		}
		if (reader.LocalName != "binning")
		{
			throw new XmlException("Unexpeced xml tag.");
		}
		if (reader.MoveToAttribute("underflow"))
		{
			if (reader.Value != "auto")
			{
				dataFormat.UnderflowBinValue = XmlConvertExtension.ToDouble(reader.Value);
			}
			else
			{
				dataFormat.UnderflowBinValue = 0.0;
				dataFormat.HistogramAxisFormatProperty.IsAutomaticFlowValue = true;
			}
		}
		if (reader.MoveToAttribute("overflow"))
		{
			if (reader.Value != "auto")
			{
				dataFormat.OverflowBinValue = XmlConvertExtension.ToDouble(reader.Value);
			}
			else
			{
				dataFormat.OverflowBinValue = 0.0;
				dataFormat.HistogramAxisFormatProperty.IsAutomaticFlowValue = true;
			}
		}
		if (reader.MoveToAttribute("intervalClosed") && reader.Value == "l")
		{
			dataFormat.IsIntervalClosedinLeft = true;
		}
		reader.MoveToElement();
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement && reader.NodeType != 0)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					string localName = reader.LocalName;
					if (!(localName == "binSize"))
					{
						if (localName == "binCount")
						{
							if (reader.MoveToAttribute("val"))
							{
								dataFormat.NumberOfBins = XmlConvertExtension.ToInt32(reader.Value);
							}
							reader.Skip();
						}
						else
						{
							reader.Skip();
						}
					}
					else
					{
						if (reader.MoveToAttribute("val"))
						{
							dataFormat.BinWidth = XmlConvertExtension.ToDouble(reader.Value);
						}
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

	private void ParseChartSeriesVisibility(XmlReader reader, ChartSerieDataFormatImpl dataFormat)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (dataFormat == null)
		{
			throw new ArgumentNullException("Serie Data Format");
		}
		if (reader.LocalName != "visibility")
		{
			throw new XmlException("Unexpeced xml tag.");
		}
		if (reader.MoveToAttribute("connectorLines"))
		{
			dataFormat.ShowConnectorLines = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("meanLine"))
		{
			dataFormat.ShowMeanLine = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("meanMarker"))
		{
			dataFormat.ShowMeanMarkers = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("outliers"))
		{
			dataFormat.ShowOutlierPoints = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("nonoutliers"))
		{
			dataFormat.ShowInnerPoints = XmlConvertExtension.ToBoolean(reader.Value);
		}
		reader.Skip();
	}

	private void ParsePlotSurface(XmlReader reader, ChartImpl chart, RelationCollection relations, Excel2007Parser excel2007Parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "plotSurface")
		{
			throw new XmlException("Unexpected xml tag");
		}
		bool isBorderCornersRound = chart.PlotArea != null && chart.ChartArea.IsBorderCornersRound;
		FileDataHolder parentHolder = chart.DataHolder.ParentHolder;
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement && reader.NodeType != 0)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				if (reader.LocalName == "spPr")
				{
					chart.HasPlotArea = true;
					IOfficeChartFrameFormat plotArea = chart.PlotArea;
					chart.ChartArea.IsBorderCornersRound = isBorderCornersRound;
					ChartFillObjectGetterAny objectGetter = new ChartFillObjectGetterAny(plotArea.Border as ChartBorderImpl, plotArea.Interior as ChartInteriorImpl, plotArea.Fill as IInternalFill, plotArea.Shadow as ShadowImpl, plotArea.ThreeD as ThreeDFormatImpl);
					ChartParserCommon.ParseShapeProperties(reader, objectGetter, parentHolder, relations);
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

	private int ParseChartExAxes(XmlReader reader, int secondaryAxisId, List<int> hashCodeList, ChartExAxisParser axisParser, ChartImpl chart, FileDataHolder dataHolder, Excel2007Parser excel2007parser, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (reader.LocalName != "axis")
		{
			throw new XmlException("Unexpected xml tag");
		}
		bool? axisIsHidden = null;
		int? axisId = null;
		int result = -1;
		axisParser.ParseAxisCommonAttributes(reader, out axisIsHidden, out axisId);
		if (!reader.IsEmptyElement)
		{
			MemoryStream memoryStream = new MemoryStream();
			XmlWriter xmlWriter = UtilityMethods.CreateWriter(memoryStream, Encoding.UTF8);
			xmlWriter.WriteNode(reader, defattr: false);
			xmlWriter.Flush();
			memoryStream.Position = 0L;
			XmlReader xmlReader = UtilityMethods.CreateReader(memoryStream);
			xmlReader.Read();
			ChartAxisImpl chartAxisImpl = null;
			chartAxisImpl = TryParseAxisFromReader(xmlReader, axisParser, chart, axisId, secondaryAxisId);
			if (chartAxisImpl != null && !hashCodeList.Contains(chartAxisImpl.GetHashCode()))
			{
				if (axisIsHidden.HasValue && axisIsHidden.Value)
				{
					chartAxisImpl.Deleted = true;
				}
				memoryStream.Position = 0L;
				xmlReader = UtilityMethods.CreateReader(memoryStream);
				axisParser.ParseChartExAxis(xmlReader, chartAxisImpl, chart, relations, excel2007parser, dataHolder);
				result = chartAxisImpl.GetHashCode();
			}
			xmlReader.Dispose();
			xmlWriter.Dispose();
			memoryStream.Dispose();
		}
		return result;
	}

	private ChartAxisImpl TryParseAxisFromReader(XmlReader axisReader, ChartExAxisParser axisParser, ChartImpl chart, int? currentAxisId, int secondaryAxisId)
	{
		if (axisReader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		ChartAxisImpl chartAxisImpl = null;
		while (axisReader.NodeType != XmlNodeType.EndElement)
		{
			if (axisReader.NodeType == XmlNodeType.Element)
			{
				string localName = axisReader.LocalName;
				if (!(localName == "valScaling"))
				{
					if (localName == "catScaling")
					{
						chartAxisImpl = chart.PrimaryCategoryAxis as ChartAxisImpl;
						axisParser.ParseAxisAttributes(axisReader, chartAxisImpl, isValueAxis: false);
						axisReader.Skip();
					}
					else
					{
						axisReader.Skip();
					}
				}
				else
				{
					chartAxisImpl = ((!currentAxisId.HasValue || currentAxisId.Value != secondaryAxisId) ? (chart.PrimaryValueAxis as ChartAxisImpl) : (chart.SecondaryValueAxis as ChartAxisImpl));
					axisParser.ParseAxisAttributes(axisReader, chartAxisImpl, isValueAxis: true);
					axisReader.Skip();
				}
			}
			else
			{
				axisReader.Skip();
			}
		}
		return chartAxisImpl;
	}

	internal override void ParseChartDataLabelVisibility(XmlReader reader, ChartDataLabelsImpl dataLabels)
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
		bool flag2 = (dataLabels.IsCategoryName = false);
		bool isSeriesName = (dataLabels.IsValue = flag2);
		dataLabels.IsSeriesName = isSeriesName;
		if (reader.MoveToAttribute("seriesName"))
		{
			dataLabels.IsSeriesName = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("categoryName"))
		{
			dataLabels.IsCategoryName = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("value"))
		{
			dataLabels.IsValue = XmlConvertExtension.ToBoolean(reader.Value);
		}
		reader.Skip();
	}
}
