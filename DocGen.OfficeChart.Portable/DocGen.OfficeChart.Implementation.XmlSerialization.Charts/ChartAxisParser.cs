using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using DocGen.Compression;
using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Implementation.XmlReaders;
using DocGen.OfficeChart.Implementation.XmlReaders.Shapes;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Interfaces.Charts;

namespace DocGen.OfficeChart.Implementation.XmlSerialization.Charts;

internal class ChartAxisParser
{
	private delegate void AxisTagsParser(XmlReader reader, ChartAxisImpl axis, RelationCollection relations);

	public const string DefaultFont = "Calibri";

	public const float DefaultFontSize = 10f;

	private Dictionary<string, OfficeTickLabelPosition> s_dictTickLabelToAttributeValue = new Dictionary<string, OfficeTickLabelPosition>(4);

	internal Dictionary<string, OfficeTickMark> s_dictTickMarkToAttributeValue = new Dictionary<string, OfficeTickMark>(4);

	private WorkbookImpl m_book;

	internal string m_BarAxisId;

	internal bool m_isBarchart;

	public ChartAxisParser()
	{
		if (s_dictTickLabelToAttributeValue.Count == 0)
		{
			s_dictTickLabelToAttributeValue.Add("high", OfficeTickLabelPosition.TickLabelPosition_High);
			s_dictTickLabelToAttributeValue.Add("low", OfficeTickLabelPosition.TickLabelPosition_Low);
			s_dictTickLabelToAttributeValue.Add("nextTo", OfficeTickLabelPosition.TickLabelPosition_NextToAxis);
			s_dictTickLabelToAttributeValue.Add("none", OfficeTickLabelPosition.TickLabelPosition_None);
			s_dictTickMarkToAttributeValue.Add("none", OfficeTickMark.TickMark_None);
			s_dictTickMarkToAttributeValue.Add("in", OfficeTickMark.TickMark_Inside);
			s_dictTickMarkToAttributeValue.Add("out", OfficeTickMark.TickMark_Outside);
			s_dictTickMarkToAttributeValue.Add("cross", OfficeTickMark.TickMark_Cross);
		}
	}

	public ChartAxisParser(WorkbookImpl book)
		: this()
	{
		m_book = book;
		ChartParserCommon.SetWorkbook(book);
	}

	public void ParseDateAxis(XmlReader reader, ChartCategoryAxisImpl axis, RelationCollection relations, OfficeChartType chartType, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (axis == null)
		{
			throw new ArgumentNullException("axis");
		}
		if (reader.LocalName != "dateAx")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		axis.CategoryType = OfficeCategoryType.Time;
		ParseAxisCommon(reader, axis, relations, chartType, parser, DateAxisTagParsing, isBarChart: false, string.Empty);
		reader.Read();
	}

	public void ParseCategoryAxis(XmlReader reader, ChartCategoryAxisImpl axis, RelationCollection relations, OfficeChartType chartType, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (axis == null)
		{
			throw new ArgumentNullException("axis");
		}
		if (reader.LocalName != "catAx")
		{
			throw new XmlException("reader");
		}
		reader.Read();
		axis.IsAutoMajor = true;
		axis.IsAutoMinor = true;
		axis.CategoryType = OfficeCategoryType.Category;
		ParseAxisCommon(reader, axis, relations, chartType, parser, CategoryAxisTagParsing, m_isBarchart, m_BarAxisId);
		reader.Read();
	}

	public void ParseValueAxis(XmlReader reader, ChartValueAxisImpl valueAxis, RelationCollection relations, OfficeChartType chartType, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (valueAxis == null)
		{
			throw new ArgumentNullException("valueAxis");
		}
		if (reader.LocalName != "valAx")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		ParseAxisCommon(reader, valueAxis, relations, chartType, parser, ValueAxisTagParsing, isBarChart: false, m_BarAxisId);
		reader.Read();
	}

	public void ParseSeriesAxis(XmlReader reader, ChartSeriesAxisImpl seriesAxis, RelationCollection relations, OfficeChartType chartType, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (seriesAxis == null)
		{
			throw new ArgumentNullException("seriesAxis");
		}
		if (reader.LocalName != "serAx")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		seriesAxis.AutoTickLabelSpacing = true;
		seriesAxis.AutoTickMarkSpacing = true;
		ParseAxisCommon(reader, seriesAxis, relations, chartType, parser, SeriesAxisTagParsing, isBarChart: false, string.Empty);
		reader.Read();
	}

	private void ParseAxisCommon(XmlReader reader, ChartAxisImpl axis, RelationCollection chartItemRelations, OfficeChartType chartType, Excel2007Parser parser, AxisTagsParser unknownTagParser, bool isBarChart, string axisId)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (axis == null)
		{
			throw new ArgumentNullException("axis");
		}
		ChartImpl parentChart = axis.ParentChart;
		FileDataHolder parentHolder = parentChart.DataHolder.ParentHolder;
		RelationCollection relations = parentChart.Relations;
		bool flag = true;
		ChartAxisScale chartAxisScale = null;
		int axisId2 = -1;
		bool? flag2 = null;
		axis.IsChartFont = true;
		axis.Font.FontName = "Calibri";
		axis.Font.Size = 10.0;
		axis.IsChartFont = false;
		axis.IsDefaultTextSettings = true;
		bool flag3 = false;
		MemoryStream memoryStream = new MemoryStream();
		XmlWriter xmlWriter = UtilityMethods.CreateWriter(memoryStream, Encoding.UTF8);
		xmlWriter.WriteStartElement("root");
		while (reader.NodeType != XmlNodeType.EndElement && flag)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "axId":
					axisId2 = ChartParserCommon.ParseIntValueTag(reader);
					break;
				case "scaling":
					chartAxisScale = ParseScaling(reader);
					break;
				case "delete":
					flag2 = !ChartParserCommon.ParseBoolValueTag(reader);
					break;
				case "axPos":
				{
					string text = ParseAxisPosition(reader, axis);
					ChartAxisPos value = (ChartAxisPos)Enum.Parse(typeof(ChartAxisPos), text, ignoreCase: false);
					if (chartType == OfficeChartType.Scatter_Markers && isBarChart && (text == "l" || text == "r"))
					{
						axis = (axis.IsPrimary ? axis.ParentChart.PrimaryCategoryAxis : axis.ParentChart.SecondaryCategoryAxis) as ChartValueAxisImpl;
						axis.Visible = true;
					}
					else if (!axisId.Contains(axisId2.ToString()) && ((chartType == OfficeChartType.Scatter_Markers && !isBarChart) || chartType == OfficeChartType.Bubble) && (text == "b" || text == "t"))
					{
						axis = (axis.IsPrimary ? axis.ParentChart.PrimaryCategoryAxis : axis.ParentChart.SecondaryCategoryAxis) as ChartValueAxisImpl;
						axis.Visible = true;
					}
					axis.AxisPosition = value;
					break;
				}
				case "majorGridlines":
					axis.HasMajorGridLines = true;
					ParseGridlines(reader, axis.MajorGridLines, parentHolder, chartItemRelations);
					break;
				case "minorGridlines":
					axis.HasMinorGridLines = true;
					ParseGridlines(reader, axis.MinorGridLines, parentHolder, chartItemRelations);
					break;
				case "title":
					xmlWriter.WriteNode(reader, defattr: false);
					xmlWriter.Flush();
					flag3 = true;
					break;
				case "numFmt":
					ParseNumberFormat(reader, axis);
					break;
				case "majorTickMark":
					axis.MajorTickMark = ParseTickMark(reader);
					break;
				case "minorTickMark":
					axis.MinorTickMark = ParseTickMark(reader);
					break;
				case "tickLblPos":
					ParseTickLabel(reader, axis);
					break;
				case "crossAx":
					ParseCrossAxis(reader, axis);
					break;
				case "crosses":
					ParseCrossesTag(reader, axis);
					break;
				case "crossesAt":
					if (ChartAxisSerializator.GetPairAxis(axis) is ChartValueAxisImpl chartValueAxisImpl)
					{
						chartValueAxisImpl.CrossesAt = ChartParserCommon.ParseDoubleValueTag(reader);
					}
					else
					{
						(axis as ChartSeriesAxisImpl).CrossesAt = ChartParserCommon.ParseIntValueTag(reader);
					}
					break;
				case "spPr":
				{
					ChartInteriorImpl chartInteriorImpl = axis.FrameFormat.Interior as ChartInteriorImpl;
					chartInteriorImpl.UseAutomaticFormat = false;
					ChartFillImpl fill = axis.FrameFormat.Fill as ChartFillImpl;
					IChartFillObjectGetter objectGetter = new ChartFillObjectGetterAny(axis.FrameFormat.Border as ChartBorderImpl, chartInteriorImpl, fill, axis.ShadowProperties as ShadowImpl, axis.FrameFormat.ThreeD as ThreeDFormatImpl);
					ChartParserCommon.ParseShapeProperties(reader, objectGetter, parentHolder, chartItemRelations);
					axis.AssignBorderReference(axis.FrameFormat.Border);
					break;
				}
				case "txPr":
				{
					axis.ParagraphType = ChartParagraphType.CustomDefault;
					Stream stream = ShapeParser.ReadNodeAsStream(reader);
					stream.Position = 0L;
					axis.TextStream = stream;
					XmlReader reader2 = UtilityMethods.CreateReader(stream);
					ParseTextSettings(reader2, axis, parser);
					break;
				}
				case "lblAlgn":
					switch (ChartParserCommon.ParseValueTag(reader))
					{
					case "l":
						axis.LabelAlign = AxisLabelAlignment.Left;
						break;
					case "r":
						axis.LabelAlign = AxisLabelAlignment.Right;
						break;
					default:
						axis.LabelAlign = AxisLabelAlignment.Center;
						break;
					}
					break;
				default:
					if (unknownTagParser != null)
					{
						unknownTagParser(reader, axis, relations);
					}
					else
					{
						reader.Skip();
					}
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
		if (flag3)
		{
			IInternalOfficeChartTextArea internalOfficeChartTextArea = axis.TitleArea as IInternalOfficeChartTextArea;
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
							ChartParserCommon.ParseDefaultTextFormatting(xmlReader, internalOfficeChartTextArea, parentHolder.Parser, 10.0);
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
				ChartParserCommon.ParseTextArea(xmlReader2, internalOfficeChartTextArea, parentHolder, relations, 10f);
			}
			xmlReader2.Dispose();
			xmlReader.Dispose();
			memoryStream.Dispose();
			if (internalOfficeChartTextArea is ChartTextAreaImpl && !internalOfficeChartTextArea.HasTextRotation && (axis.AxisPosition == ChartAxisPos.l || axis.AxisPosition == ChartAxisPos.r))
			{
				internalOfficeChartTextArea.TextRotationAngle = -90;
			}
		}
		axis.AxisId = axisId2;
		if (flag2.HasValue)
		{
			axis.Deleted = !flag2.Value;
		}
		chartAxisScale?.CopyTo(axis as IScalable);
	}

	internal void ParseTextSettings(XmlReader reader, ChartAxisImpl axis, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (axis == null)
		{
			throw new ArgumentNullException("axis");
		}
		if (reader.LocalName != "txPr")
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
					if (!(localName == "bodyPr"))
					{
						if (localName == "p")
						{
							ParseAxisParagraphs(reader, axis, parser);
						}
						else
						{
							reader.Skip();
						}
					}
					else
					{
						ParseBodyProperties(reader, axis);
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

	private void ParseAxisParagraphs(XmlReader reader, ChartAxisImpl axis, Excel2007Parser parser)
	{
		while (!(reader.LocalName == "p") || reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "defRPr")
			{
				bool flag = false;
				if (reader.IsEmptyElement)
				{
					flag = true;
				}
				TextSettings defaultSettings = ChartParserCommon.ParseDefaultParagraphProperties(reader, parser, 10f, parser.Workbook);
				ChartParserCommon.CopyDefaultSettings(axis.Font as IInternalFont, defaultSettings, parser.Workbook);
				if (flag)
				{
					axis.IsDefaultTextSettings = true;
				}
			}
			else if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "pPr")
			{
				if (reader.MoveToAttribute("algn"))
				{
					switch (reader.Value.ToString())
					{
					case "l":
						axis.LabelTextAlign = AxisLabelAlignment.Left;
						break;
					case "r":
						axis.LabelTextAlign = AxisLabelAlignment.Right;
						break;
					case "just":
						axis.LabelTextAlign = AxisLabelAlignment.Justify;
						break;
					default:
						axis.LabelTextAlign = AxisLabelAlignment.Center;
						break;
					}
				}
				reader.Read();
			}
			else
			{
				reader.Read();
			}
		}
		reader.Read();
	}

	private void ParseBodyProperties(XmlReader reader, ChartAxisImpl axis)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (axis == null)
		{
			throw new ArgumentNullException("axis");
		}
		if (reader.LocalName != "bodyPr")
		{
			throw new XmlException();
		}
		if (reader.MoveToAttribute("rot"))
		{
			axis.TextRotationAngle = XmlConvertExtension.ToInt32(reader.Value) / 60000;
		}
		if (reader.MoveToAttribute("vert"))
		{
			axis.m_textRotation = ParseTextRotation(reader);
		}
		reader.MoveToElement();
		reader.Skip();
	}

	private Excel2007TextRotation ParseTextRotation(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "vert")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		Excel2007TextRotation excel2007TextRotation = Excel2007TextRotation.horz;
		return reader.Value switch
		{
			"wordArtVert" => Excel2007TextRotation.wordArtVert, 
			"vert" => Excel2007TextRotation.vert, 
			"vert270" => Excel2007TextRotation.vert270, 
			"eaVert" => Excel2007TextRotation.vert, 
			"mongolianVert" => Excel2007TextRotation.mongolianVert, 
			"wordArtVertRtl" => Excel2007TextRotation.wordArtVertRtl, 
			_ => Excel2007TextRotation.horz, 
		};
	}

	private void ParseCrossesTag(XmlReader reader, ChartAxisImpl axis)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (axis == null)
		{
			throw new ArgumentNullException("axis");
		}
		if (reader.LocalName != "crosses")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		if (ChartParserCommon.ParseValueTag(reader) == "max")
		{
			(ChartAxisSerializator.GetPairAxis(axis) as ChartValueAxisImpl).IsMaxCross = true;
		}
		if (axis.ParentChart != null && axis.ParentChart.ParentWorkbook != null && axis.ParentChart.ParentWorkbook.IsWorkbookOpening)
		{
			if (axis.AxisType == OfficeAxisType.Category)
			{
				(ChartAxisSerializator.GetPairAxis(axis) as ChartValueAxisImpl).IsChangeAutoCrossInLoading = true;
			}
			else if (axis.AxisType == OfficeAxisType.Value)
			{
				(ChartAxisSerializator.GetPairAxis(axis) as ChartCategoryAxisImpl).IsChangeAutoCrossInLoading = true;
			}
		}
	}

	private void ParseCrossAxis(XmlReader reader, ChartAxisImpl axis)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (axis == null)
		{
			throw new ArgumentNullException("axis");
		}
		if (reader.LocalName != "crossAx")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Skip();
	}

	private OfficeTickMark ParseTickMark(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		string key = ChartParserCommon.ParseValueTag(reader);
		return s_dictTickMarkToAttributeValue[key];
	}

	private void ParseTickLabel(XmlReader reader, ChartAxisImpl axis)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (axis == null)
		{
			throw new ArgumentNullException("axis");
		}
		if (reader.LocalName != "tickLblPos")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		string key = ChartParserCommon.ParseValueTag(reader);
		axis.TickLabelPosition = s_dictTickLabelToAttributeValue[key];
	}

	internal void ParseNumberFormat(XmlReader reader, ChartAxisImpl axis)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (axis == null)
		{
			throw new ArgumentNullException("axis");
		}
		if (reader.LocalName != "numFmt")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		if (reader.MoveToAttribute("formatCode"))
		{
			axis.NumberFormat = reader.Value;
		}
		if (reader.MoveToAttribute("sourceLinked"))
		{
			axis.IsSourceLinked = XmlConvertExtension.ToBoolean(reader.Value);
		}
		reader.Read();
	}

	internal void ParseGridlines(XmlReader reader, IOfficeChartGridLine gridLines, FileDataHolder dataHolder, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (gridLines == null)
		{
			throw new ArgumentNullException("gridLines");
		}
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "spPr")
				{
					ChartFillObjectGetterAny objectGetter = new ChartFillObjectGetterAny(gridLines.Border as ChartBorderImpl, null, null, gridLines.Shadow as ShadowImpl, gridLines.ThreeD as ThreeDFormatImpl);
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

	private string ParseAxisPosition(XmlReader reader, ChartAxisImpl axis)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (axis == null)
		{
			throw new ArgumentNullException("valueAxis");
		}
		return ChartParserCommon.ParseValueTag(reader);
	}

	private ChartAxisScale ParseScaling(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "scaling")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		ChartAxisScale chartAxisScale = new ChartAxisScale();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "logBase":
					chartAxisScale.LogBase = ChartParserCommon.ParseDoubleValueTag(reader);
					chartAxisScale.LogScale = true;
					break;
				case "orientation":
					if (ChartParserCommon.ParseValueTag(reader) == "maxMin")
					{
						chartAxisScale.Reversed = true;
					}
					break;
				case "max":
					chartAxisScale.MaximumValue = ChartParserCommon.ParseDoubleValueTag(reader);
					break;
				case "min":
					chartAxisScale.MinimumValue = ChartParserCommon.ParseDoubleValueTag(reader);
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
		return chartAxisScale;
	}

	private void ParseDisplayUnit(XmlReader reader, ChartValueAxisImpl valueAxis, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (valueAxis == null)
		{
			throw new ArgumentNullException("valueAxis");
		}
		if (reader.LocalName != "dispUnits")
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
					case "builtInUnit":
						ParseBuiltInDisplayUnit(reader, valueAxis);
						break;
					case "dispUnitsLbl":
					{
						valueAxis.HasDisplayUnitLabel = true;
						IInternalOfficeChartTextArea textArea = valueAxis.DisplayUnitLabel as IInternalOfficeChartTextArea;
						FileDataHolder dataHolder = valueAxis.ParentChart.ParentWorkbook.DataHolder;
						ChartParserCommon.ParseTextArea(reader, textArea, dataHolder, relations);
						break;
					}
					case "custUnit":
						valueAxis.DisplayUnitCustom = ChartParserCommon.ParseDoubleValueTag(reader);
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

	private void ParseBuiltInDisplayUnit(XmlReader reader, ChartValueAxisImpl valueAxis)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (valueAxis == null)
		{
			throw new ArgumentNullException("valueAxis");
		}
		string value = ChartParserCommon.ParseValueTag(reader);
		Excel2007ChartDisplayUnit displayUnit = (Excel2007ChartDisplayUnit)Enum.Parse(typeof(Excel2007ChartDisplayUnit), value, ignoreCase: false);
		valueAxis.DisplayUnit = (OfficeChartDisplayUnit)displayUnit;
	}

	private void CategoryAxisTagParsing(XmlReader reader, ChartAxisImpl axis, RelationCollection relations)
	{
		if (reader.NodeType == XmlNodeType.Element)
		{
			ChartCategoryAxisImpl chartCategoryAxisImpl = axis as ChartCategoryAxisImpl;
			switch (reader.LocalName)
			{
			case "lblOffset":
				if (reader.MoveToAttribute("val"))
				{
					chartCategoryAxisImpl.Offset = ChartParserCommon.ParseIntValueTag(reader);
					break;
				}
				chartCategoryAxisImpl.Offset = 100;
				reader.Skip();
				break;
			case "tickLblSkip":
				chartCategoryAxisImpl.TickLabelSpacing = ChartParserCommon.ParseIntValueTag(reader);
				break;
			case "tickMarkSkip":
				chartCategoryAxisImpl.TickMarkSpacing = ChartParserCommon.ParseIntValueTag(reader);
				break;
			case "auto":
				chartCategoryAxisImpl.CategoryType = (ChartParserCommon.ParseBoolValueTag(reader) ? OfficeCategoryType.Automatic : OfficeCategoryType.Category);
				break;
			case "majorUnit":
				chartCategoryAxisImpl.MajorUnit = ChartParserCommon.ParseDoubleValueTag(reader);
				break;
			case "minorUnit":
				chartCategoryAxisImpl.MinorUnit = ChartParserCommon.ParseDoubleValueTag(reader);
				break;
			case "noMultiLvlLbl":
				chartCategoryAxisImpl.NoMultiLevelLabel = ChartParserCommon.ParseBoolValueTag(reader);
				chartCategoryAxisImpl.m_showNoMultiLvlLbl = true;
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

	private void DateAxisTagParsing(XmlReader reader, ChartAxisImpl axis, RelationCollection relations)
	{
		if (reader.NodeType == XmlNodeType.Element)
		{
			ChartCategoryAxisImpl chartCategoryAxisImpl = axis as ChartCategoryAxisImpl;
			switch (reader.LocalName)
			{
			case "lblOffset":
				chartCategoryAxisImpl.Offset = ChartParserCommon.ParseIntValueTag(reader);
				break;
			case "majorUnit":
				chartCategoryAxisImpl.MajorUnit = ChartParserCommon.ParseDoubleValueTag(reader);
				break;
			case "minorUnit":
				chartCategoryAxisImpl.MinorUnit = ChartParserCommon.ParseDoubleValueTag(reader);
				break;
			case "minorTimeUnit":
			{
				string baseUnitScale = ChartParserCommon.ParseValueTag(reader);
				((ChartCategoryAxisImpl)axis).MinorUnitScale = GetChartBaseUnitFromString(baseUnitScale);
				((ChartCategoryAxisImpl)axis).MinorUnitScaleIsAuto = false;
				break;
			}
			case "majorTimeUnit":
			{
				string baseUnitScale = ChartParserCommon.ParseValueTag(reader);
				((ChartCategoryAxisImpl)axis).MajorUnitScale = GetChartBaseUnitFromString(baseUnitScale);
				((ChartCategoryAxisImpl)axis).MajorUnitScaleIsAuto = false;
				break;
			}
			case "baseTimeUnit":
			{
				string baseUnitScale = ChartParserCommon.ParseValueTag(reader);
				((ChartCategoryAxisImpl)axis).BaseUnit = GetChartBaseUnitFromString(baseUnitScale);
				((ChartCategoryAxisImpl)axis).BaseUnitIsAuto = false;
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

	private void ValueAxisTagParsing(XmlReader reader, ChartAxisImpl axis, RelationCollection relations)
	{
		if (reader.NodeType == XmlNodeType.Element)
		{
			ChartValueAxisImpl chartValueAxisImpl = axis as ChartValueAxisImpl;
			switch (reader.LocalName)
			{
			case "crossBetween":
			{
				string text = ChartParserCommon.ParseValueTag(reader);
				ChartImpl parentChart = chartValueAxisImpl.ParentChart;
				(chartValueAxisImpl.IsPrimary ? parentChart.PrimaryCategoryAxis : parentChart.SecondaryCategoryAxis).IsBetween = text == "between";
				break;
			}
			case "majorUnit":
				chartValueAxisImpl.SetMajorUnit(ChartParserCommon.ParseDoubleValueTag(reader));
				break;
			case "minorUnit":
				chartValueAxisImpl.SetMinorUnit(ChartParserCommon.ParseDoubleValueTag(reader));
				break;
			case "dispUnits":
				ParseDisplayUnit(reader, chartValueAxisImpl, relations);
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

	private void SeriesAxisTagParsing(XmlReader reader, ChartAxisImpl axis, RelationCollection relations)
	{
		if (reader.NodeType == XmlNodeType.Element)
		{
			ChartSeriesAxisImpl chartSeriesAxisImpl = axis as ChartSeriesAxisImpl;
			string localName = reader.LocalName;
			if (!(localName == "tickLblSkip"))
			{
				if (localName == "tickMarkSkip")
				{
					chartSeriesAxisImpl.TickMarkSpacing = ChartParserCommon.ParseIntValueTag(reader);
				}
				else
				{
					reader.Skip();
				}
			}
			else
			{
				chartSeriesAxisImpl.TickLabelSpacing = ChartParserCommon.ParseIntValueTag(reader);
			}
		}
		else
		{
			reader.Skip();
		}
	}

	private OfficeChartBaseUnit GetChartBaseUnitFromString(string baseUnitScale)
	{
		baseUnitScale = PrepareBaseUnitScale(baseUnitScale);
		return (OfficeChartBaseUnit)Enum.Parse(typeof(OfficeChartBaseUnit), baseUnitScale, ignoreCase: false);
	}

	private string PrepareBaseUnitScale(string baseUnitScale)
	{
		baseUnitScale = RemoveCharUnSafeAtLast(baseUnitScale);
		return char.ToUpper(baseUnitScale[0]) + baseUnitScale.Substring(1);
	}

	private string RemoveCharUnSafeAtLast(string baseUnitScale)
	{
		return baseUnitScale.Substring(0, baseUnitScale.Length - 1);
	}
}
