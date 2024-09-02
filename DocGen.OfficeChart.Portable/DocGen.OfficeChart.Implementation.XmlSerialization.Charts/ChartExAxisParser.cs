using System;
using System.IO;
using System.Text;
using System.Xml;
using DocGen.Compression;
using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Implementation.XmlReaders;
using DocGen.OfficeChart.Implementation.XmlReaders.Shapes;
using DocGen.OfficeChart.Interfaces.Charts;

namespace DocGen.OfficeChart.Implementation.XmlSerialization.Charts;

internal class ChartExAxisParser : ChartAxisParser
{
	public ChartExAxisParser(WorkbookImpl book)
		: base(book)
	{
	}

	internal void ParseChartExAxis(XmlReader reader, ChartAxisImpl axis, ChartImpl chart, RelationCollection relations, Excel2007Parser parser, FileDataHolder dataHolder)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (axis == null)
		{
			throw new ArgumentNullException("axis");
		}
		if (reader.LocalName != "axis")
		{
			throw new XmlException("Unexpected xml tag");
		}
		reader.Read();
		axis.Visible = true;
		axis.SetDefaultFont("Calibri", 10f);
		axis.MajorTickMark = OfficeTickMark.TickMark_None;
		axis.MinorTickMark = OfficeTickMark.TickMark_None;
		bool flag = false;
		bool flag2 = false;
		MemoryStream memoryStream = new MemoryStream();
		XmlWriter xmlWriter = UtilityMethods.CreateWriter(memoryStream, Encoding.UTF8);
		xmlWriter.WriteStartElement("root");
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "title":
					xmlWriter.WriteNode(reader, defattr: false);
					xmlWriter.WriteEndElement();
					xmlWriter.Flush();
					flag = true;
					break;
				case "units":
					ParseDisplayUnits(reader, axis as ChartValueAxisImpl, relations);
					break;
				case "majorGridlines":
					axis.HasMajorGridLines = true;
					ParseGridlines(reader, axis.MajorGridLines, dataHolder, relations);
					break;
				case "minorGridlines":
					axis.HasMinorGridLines = true;
					ParseGridlines(reader, axis.MinorGridLines, dataHolder, relations);
					break;
				case "majorTickMarks":
					axis.MajorTickMark = ParseTickMark(reader);
					break;
				case "minorTickMarks":
					axis.MinorTickMark = ParseTickMark(reader);
					break;
				case "spPr":
					if (!reader.IsEmptyElement)
					{
						ChartInteriorImpl chartInteriorImpl = axis.FrameFormat.Interior as ChartInteriorImpl;
						chartInteriorImpl.UseAutomaticFormat = false;
						ChartFillImpl fill = axis.FrameFormat.Fill as ChartFillImpl;
						IChartFillObjectGetter objectGetter = new ChartFillObjectGetterAny(axis.FrameFormat.Border as ChartBorderImpl, chartInteriorImpl, fill, axis.ShadowProperties as ShadowImpl, axis.FrameFormat.ThreeD as ThreeDFormatImpl);
						ChartParserCommon.ParseShapeProperties(reader, objectGetter, dataHolder, relations);
						axis.AssignBorderReference(axis.FrameFormat.Border);
					}
					else
					{
						reader.Skip();
					}
					break;
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
				case "tickLabels":
					flag2 = true;
					reader.Skip();
					break;
				case "numFmt":
					ParseNumberFormat(reader, axis);
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
		if (flag)
		{
			IInternalOfficeChartTextArea textArea = axis.TitleArea as IInternalOfficeChartTextArea;
			ChartParserCommon.ParseChartTitleElement(memoryStream, textArea, dataHolder, relations, 10f);
		}
		if (!flag2)
		{
			axis.TickLabelPosition = OfficeTickLabelPosition.TickLabelPosition_None;
		}
	}

	private void ParseDisplayUnits(XmlReader reader, ChartValueAxisImpl valueAxis, RelationCollection relations)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (valueAxis == null)
		{
			throw new ArgumentNullException("valueAxis");
		}
		if (reader.LocalName != "units")
		{
			throw new XmlException();
		}
		valueAxis.HasDisplayUnitLabel = true;
		if (reader.MoveToAttribute("unit"))
		{
			Excel2007ChartDisplayUnit excel2007ChartDisplayUnit;
			if (reader.Value == "percentage")
			{
				excel2007ChartDisplayUnit = Excel2007ChartDisplayUnit.hundreds;
				valueAxis.NumberFormat = "0%";
				valueAxis.m_isDisplayUnitPercentage = true;
			}
			else
			{
				excel2007ChartDisplayUnit = (Excel2007ChartDisplayUnit)Enum.Parse(typeof(Excel2007ChartDisplayUnit), reader.Value, ignoreCase: false);
			}
			if (excel2007ChartDisplayUnit == Excel2007ChartDisplayUnit.hundreds)
			{
				valueAxis.DisplayUnit = OfficeChartDisplayUnit.Custom;
			}
			else
			{
				valueAxis.DisplayUnit = (OfficeChartDisplayUnit)excel2007ChartDisplayUnit;
			}
		}
		reader.MoveToElement();
		if (!reader.IsEmptyElement)
		{
			MemoryStream memoryStream = new MemoryStream();
			XmlWriter xmlWriter = UtilityMethods.CreateWriter(memoryStream, Encoding.UTF8);
			xmlWriter.WriteStartElement("root");
			xmlWriter.WriteNode(reader, defattr: false);
			xmlWriter.WriteEndElement();
			xmlWriter.Flush();
			IInternalOfficeChartTextArea textArea = valueAxis.DisplayUnitLabel as IInternalOfficeChartTextArea;
			FileDataHolder dataHolder = valueAxis.ParentChart.ParentWorkbook.DataHolder;
			ChartParserCommon.ParseChartTitleElement(memoryStream, textArea, dataHolder, relations, 10f);
		}
		else
		{
			reader.Skip();
		}
	}

	internal void ParseAxisCommonAttributes(XmlReader reader, out bool? axisIsHidden, out int? axisId)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "axis")
		{
			throw new XmlException("Unexpected xml tag");
		}
		axisId = null;
		axisIsHidden = null;
		if (reader.MoveToAttribute("hidden"))
		{
			axisIsHidden = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("id"))
		{
			axisId = XmlConvertExtension.ToInt32(reader.Value);
		}
		reader.MoveToElement();
	}

	internal void ParseAxisAttributes(XmlReader reader, ChartAxisImpl axis, bool isValueAxis)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "valScaling" && reader.LocalName != "catScaling")
		{
			throw new XmlException("Unexpected xml tag");
		}
		if (axis == null)
		{
			throw new ArgumentNullException("axis");
		}
		if (isValueAxis)
		{
			ChartValueAxisImpl chartValueAxisImpl = axis as ChartValueAxisImpl;
			bool isAutoMax = (chartValueAxisImpl.IsAutoMin = true);
			chartValueAxisImpl.IsAutoMax = isAutoMax;
			isAutoMax = (chartValueAxisImpl.IsAutoMajor = true);
			chartValueAxisImpl.IsAutoMinor = isAutoMax;
			if (reader.MoveToAttribute("min") && reader.Value != "auto")
			{
				chartValueAxisImpl.MinimumValue = XmlConvertExtension.ToDouble(reader.Value);
			}
			if (reader.MoveToAttribute("max") && reader.Value != "auto")
			{
				chartValueAxisImpl.MaximumValue = XmlConvertExtension.ToDouble(reader.Value);
			}
			if (reader.MoveToAttribute("minorUnit") && reader.Value != "auto")
			{
				chartValueAxisImpl.MinorUnit = XmlConvertExtension.ToDouble(reader.Value);
			}
			if (reader.MoveToAttribute("majorUnit") && reader.Value != "auto")
			{
				chartValueAxisImpl.MajorUnit = XmlConvertExtension.ToDouble(reader.Value);
			}
		}
		else if (reader.MoveToAttribute("gapWidth") && reader.Value != "auto")
		{
			double num = XmlConvertExtension.ToDouble(reader.Value);
			axis.ParentChart.PrimaryFormats[0].GapWidth = (int)Math.Round(num * 100.0);
		}
		reader.MoveToElement();
	}

	private OfficeTickMark ParseTickMark(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		OfficeTickMark result = OfficeTickMark.TickMark_Cross;
		if (reader.MoveToAttribute("type"))
		{
			result = s_dictTickMarkToAttributeValue[reader.Value];
		}
		return result;
	}
}
