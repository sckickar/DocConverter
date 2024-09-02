using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security;
using System.Xml;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Implementation.XmlReaders.Shapes;
using DocGen.OfficeChart.Implementation.XmlSerialization.Constants;
using DocGen.OfficeChart.Implementation.XmlSerialization.Shapes;
using DocGen.OfficeChart.Interfaces.Charts;

namespace DocGen.OfficeChart.Implementation.XmlSerialization.Charts;

internal class ChartSerializator
{
	private delegate void SerializeSeriesDelegate(XmlWriter writer, ChartSerieImpl series);

	public const int DefaultExtentX = 8666049;

	public const int DefaultExtentY = 6293304;

	public int categoryFilter;

	public bool findFilter;

	private bool m_isChartExFallBack;

	private double _appVersion = -1.0;

	internal ChartSerializator(bool value)
	{
		m_isChartExFallBack = value;
	}

	public ChartSerializator()
	{
	}

	[SecurityCritical]
	internal void SerializeChart(XmlWriter writer, ChartImpl chart, string chartItemName, double appVersion)
	{
		_appVersion = appVersion;
		SerializeChart(writer, chart, chartItemName);
	}

	[SecurityCritical]
	internal void SerializeChart(XmlWriter writer, ChartImpl chart, string chartItemName)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("c", "chartSpace", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		writer.WriteAttributeString("xmlns", "a", null, "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("xmlns", "r", null, "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
		ChartSerializatorCommon.SerializeBoolValueTag(writer, "date1904", chart.Workbook.Date1904);
		if (chart.HasChartArea && chart.ChartArea.IsBorderCornersRound)
		{
			ChartSerializatorCommon.SerializeBoolValueTag(writer, "roundedCorners", value: true);
		}
		else
		{
			ChartSerializatorCommon.SerializeBoolValueTag(writer, "roundedCorners", value: false);
		}
		if (chart.AlternateContent != null)
		{
			chart.AlternateContent.Position = 0L;
			ShapeParser.WriteNodeFromStream(writer, chart.AlternateContent);
		}
		else if (((_appVersion != -1.0 && _appVersion <= 12.0) || (chart.Style >= 101 && chart.Style <= 148)) && chart.AlternateContent == null)
		{
			writer.WriteStartElement("mc", "AlternateContent", "http://schemas.openxmlformats.org/markup-compatibility/2006");
			writer.WriteStartElement("mc", "Choice", null);
			writer.WriteAttributeString("Requires", "c14");
			writer.WriteAttributeString("xmlns", "c14", null, "http://schemas.microsoft.com/office/drawing/2007/8/2/chart");
			writer.WriteStartElement("c14", "style", null);
			writer.WriteAttributeString("val", (chart.Style >= 101 && chart.Style <= 148) ? chart.Style.ToString() : "102");
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteStartElement("mc", "Fallback", null);
			ChartSerializatorCommon.SerializeValueTag(writer, "style", (chart.Style >= 101 && chart.Style <= 148) ? (chart.Style - 100).ToString() : "2");
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
		if (chart.Style > 0 && chart.Style <= 48)
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "style", chart.Style.ToString());
		}
		if (chart.InnerProtection != 0)
		{
			writer.WriteElementString("protection", "http://schemas.openxmlformats.org/drawingml/2006/chart", string.Empty);
		}
		if (chart.IsThemeOverridePresent && chart.ColorMapOverrideDictionary.Count > 0)
		{
			SerializeColorMapOverrideTag(writer, chart.ColorMapOverrideDictionary);
		}
		writer.WriteStartElement("chart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		_ = chart.DataHolder.ParentHolder;
		RelationCollection relations = chart.Relations;
		bool flag = false;
		if (chart.HasAutoTitle.HasValue)
		{
			flag = chart.HasAutoTitle.Value;
		}
		if (flag && chart.ChartTitle != null && chart.ChartTitle != string.Empty)
		{
			flag = false;
		}
		if (chart.HasTitle && chart.HasTitleInternal && !flag)
		{
			ChartSerializatorCommon.SerializeTextArea(writer, chart.ChartTitleArea, chart.ParentWorkbook, relations, 18.0);
		}
		if (chart.HasAutoTitle.HasValue)
		{
			writer.WriteStartElement("autoTitleDeleted", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			writer.WriteAttributeString("val", flag ? "1" : "0");
			writer.WriteEndElement();
		}
		else if (!chart.Loading && chart.Series.Count == 1)
		{
			writer.WriteStartElement("autoTitleDeleted", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			writer.WriteAttributeString("val", "0");
			writer.WriteEndElement();
		}
		SerializePivotFormats(writer, chart);
		if (chart.Series.Count > 0)
		{
			SerializeView3D(writer, chart);
		}
		else
		{
			SerializePivotView3D(writer, chart);
		}
		if (chart.SupportWallsAndFloor)
		{
			if (chart.HasFloor)
			{
				SerializeSurface(writer, chart.Floor, "floor", chart);
			}
			if (chart.HasWalls && chart.ParentWorkbook.BeginVersion == 1)
			{
				SerializeSurface(writer, chart.Walls, "sideWall", chart);
				SerializeSurface(writer, chart.Walls, "backWall", chart);
			}
			else
			{
				SerializeSurface(writer, chart.SideWall, "sideWall", chart);
				SerializeSurface(writer, chart.Walls, "backWall", chart);
			}
		}
		SerializePlotArea(writer, chart, relations);
		if (chart.HasLegend)
		{
			SerializeLegend(writer, chart.Legend, chart);
		}
		if (chart.PlotVisibleOnly || (chart.Workbook as WorkbookImpl).IsCreated)
		{
			ChartSerializatorCommon.SerializeBoolValueTag(writer, "plotVisOnly", chart.PlotVisibleOnly);
		}
		if (chart.m_showDlbSOverMax != "")
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "showDLblsOverMax", chart.m_showDlbSOverMax);
		}
		ChartSerializatorCommon.SerializeValueTag(writer, "dispBlanksAs", ((Excel2007ChartPlotEmpty)chart.DisplayBlanksAs).ToString());
		writer.WriteEndElement();
		if (chart.HasChartArea)
		{
			IOfficeChartFrameFormat chartArea = chart.ChartArea;
			if (chartArea != null)
			{
				ChartSerializatorCommon.SerializeFrameFormat(writer, chartArea, chart, chartArea.IsBorderCornersRound);
			}
		}
		SerializeDefaultTextProperties(writer, chart);
		if (!chart.HasExternalWorkbook || chart.IsChartExternalRelation)
		{
			string relationId = "";
			chart.Relations.FindRelationByContentType("http://schemas.openxmlformats.org/officeDocument/2006/relationships/package", out relationId);
			if (string.IsNullOrEmpty(relationId))
			{
				relationId = chart.Relations.GenerateRelationId();
			}
			chart.Relations[relationId] = new Relation("", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/package");
			writer.WriteStartElement("c", "externalData", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			writer.WriteAttributeString("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships", relationId);
			writer.WriteStartElement("c", "autoUpdate", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			writer.WriteAttributeString("val", "0");
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
		SerializeShapes(writer, chart, chartItemName);
		writer.WriteEndElement();
	}

	private static void SerializeColorMapOverrideTag(XmlWriter xmlWriter, Dictionary<string, string> colorMap)
	{
		xmlWriter.WriteStartElement("clrMapOvr", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		foreach (KeyValuePair<string, string> item in colorMap)
		{
			xmlWriter.WriteAttributeString(item.Key, item.Value);
		}
		xmlWriter.WriteEndElement();
	}

	private void SerializeDefaultTextProperties(XmlWriter writer, ChartImpl chart)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		Stream defaultTextProperty = chart.DefaultTextProperty;
		if (defaultTextProperty != null)
		{
			defaultTextProperty.Position = 0L;
			ShapeParser.WriteNodeFromStream(writer, defaultTextProperty);
		}
	}

	private void SerializePivotFormats(XmlWriter writer, ChartImpl chart)
	{
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		Stream pivotFormatsStream = chart.PivotFormatsStream;
		if (pivotFormatsStream != null)
		{
			pivotFormatsStream.Position = 0L;
			ShapeParser.WriteNodeFromStream(writer, pivotFormatsStream);
		}
	}

	[SecurityCritical]
	private void SerializeShapes(XmlWriter writer, ChartImpl chart, string chartItemName)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (!m_isChartExFallBack)
		{
			if (chart.RelationPreservedStreamCollection.ContainsKey("http://schemas.openxmlformats.org/officeDocument/2006/relationships/chartUserShapes"))
			{
				string text = chart.Relations.GenerateRelationId();
				chart.Relations[text] = new Relation("", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/chartUserShapes");
				writer.WriteStartElement("userShapes", "http://schemas.openxmlformats.org/drawingml/2006/chart");
				writer.WriteAttributeString("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships", text);
				writer.WriteEndElement();
			}
			return;
		}
		WorksheetDataHolder dataHolder = chart.DataHolder;
		RelationCollection relations = chart.Relations;
		string id = dataHolder.DrawingsId;
		if (id == null)
		{
			id = (dataHolder.DrawingsId = relations.GenerateRelationId());
			relations[id] = null;
		}
		if (m_isChartExFallBack)
		{
			chart.DataHolder.SerializeChartExFallbackShape(chart, relations, ref id, chartItemName, "application/vnd.openxmlformats-officedocument.drawingml.chartshapes+xml", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/chartUserShapes");
			writer.WriteStartElement("userShapes", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			writer.WriteAttributeString("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships", id);
			writer.WriteEndElement();
		}
		else
		{
			relations.Remove(id);
		}
	}

	private void SerializePrinterSettings(XmlWriter writer, ChartImpl chart, RelationCollection relations)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		IOfficeChartPageSetup pageSetup = chart.PageSetup;
		writer.WriteStartElement("printSettings", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		IPageSetupConstantsProvider constants = new ChartPageSetupConstants();
		Excel2007Serializator.SerializePrintSettings(writer, pageSetup, constants, isChartSettings: true);
		Excel2007Serializator.SerializeVmlHFShapesWorksheetPart(writer, chart, constants, relations);
		chart.DataHolder.SerializeHeaderFooterImages(chart, relations);
		writer.WriteEndElement();
	}

	[SecurityCritical]
	private void SerializeLegend(XmlWriter writer, IOfficeChartLegend legend, ChartImpl chart)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (legend == null)
		{
			throw new ArgumentNullException("legend");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("legend", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		OfficeLegendPosition position = legend.Position;
		if (position != OfficeLegendPosition.NotDocked)
		{
			Excel2007LegendPosition excel2007LegendPosition = (Excel2007LegendPosition)position;
			ChartSerializatorCommon.SerializeValueTag(writer, "legendPos", excel2007LegendPosition.ToString());
		}
		IChartLegendEntries legendEntries = legend.LegendEntries;
		IWorkbook workbook = chart.Workbook;
		int i = 0;
		for (int count = legendEntries.Count; i < count; i++)
		{
			SerializeLegendEntry(writer, legendEntries[i], i, workbook);
		}
		if ((legend as ChartLegendImpl).Layout != null)
		{
			ChartSerializatorCommon.SerializeLayout(writer, legend as ChartLegendImpl);
		}
		if (!legend.IncludeInLayout)
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "overlay", "1");
		}
		else
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "overlay", "0");
		}
		ChartSerializatorCommon.SerializeFrameFormat(writer, legend.FrameFormat, chart, isRoundCorners: false);
		(legend as ChartLegendImpl).IsChartTextArea = true;
		if (((IInternalOfficeChartTextArea)legend.TextArea).ParagraphType == ChartParagraphType.CustomDefault)
		{
			SerializeDefaultTextFormatting(writer, legend.TextArea, workbook, 10.0, isCommonFontSize: false);
		}
		writer.WriteEndElement();
	}

	private void SerializeLegendEntry(XmlWriter writer, IOfficeChartLegendEntry legendEntry, int index, IWorkbook book)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (legendEntry == null)
		{
			throw new ArgumentNullException("legendEntry");
		}
		if (legendEntry.IsDeleted || legendEntry.IsFormatted)
		{
			writer.WriteStartElement("legendEntry", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			ChartSerializatorCommon.SerializeValueTag(writer, "idx", index.ToString());
			if (legendEntry.IsDeleted)
			{
				ChartSerializatorCommon.SerializeBoolValueTag(writer, "delete", value: true);
			}
			if (legendEntry.IsFormatted)
			{
				SerializeDefaultTextFormatting(writer, legendEntry.TextArea, book, 10.0, isCommonFontSize: false);
			}
			writer.WriteEndElement();
		}
	}

	private void SerializePivotView3D(XmlWriter writer, ChartImpl chart)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (chart.IsPivotChart3D)
		{
			string value = ((chart.PivotChartType == OfficeChartType.Surface_3D || chart.PivotChartType == OfficeChartType.Surface_NoColor_3D) ? "15" : "90");
			string value2 = ((chart.PivotChartType == OfficeChartType.Surface_3D || chart.PivotChartType == OfficeChartType.Surface_NoColor_3D) ? "20" : "0");
			string value3 = ((chart.PivotChartType == OfficeChartType.Surface_3D || chart.PivotChartType == OfficeChartType.Surface_NoColor_3D) ? "30" : "0");
			writer.WriteStartElement("view3D", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			ChartSerializatorCommon.SerializeValueTag(writer, "rotX", value);
			ChartSerializatorCommon.SerializeValueTag(writer, "rotY", value2);
			ChartSerializatorCommon.SerializeBoolValueTag(writer, "rAngAx", value: false);
			ChartSerializatorCommon.SerializeValueTag(writer, "perspective", value3);
			writer.WriteEndElement();
		}
	}

	private void SerializeView3D(XmlWriter writer, ChartImpl chart)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (chart.IsChart3D)
		{
			writer.WriteStartElement("view3D", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			ChartFormatImpl chartFormat = chart.ChartFormat;
			if (chartFormat.IsDefaultElevation && (chart.ChartType == OfficeChartType.Pie_3D || chart.ChartType == OfficeChartType.Pie_Exploded_3D))
			{
				ChartSerializatorCommon.SerializeValueTag(writer, "rotX", "30");
			}
			else
			{
				ChartSerializatorCommon.SerializeValueTag(writer, "rotX", chart.Elevation.ToString());
			}
			if (!chart.AutoScaling)
			{
				ChartSerializatorCommon.SerializeValueTag(writer, "hPercent", chart.HeightPercent.ToString());
			}
			if (chartFormat.IsDefaultRotation && (chart.ChartType == OfficeChartType.Pie_3D || chart.ChartType == OfficeChartType.Pie_Exploded_3D))
			{
				ChartSerializatorCommon.SerializeValueTag(writer, "rotY", "0");
			}
			else
			{
				ChartSerializatorCommon.SerializeValueTag(writer, "rotY", chart.Rotation.ToString());
			}
			ChartSerializatorCommon.SerializeValueTag(writer, "depthPercent", chart.DepthPercent.ToString());
			ChartSerializatorCommon.SerializeBoolValueTag(writer, "rAngAx", chart.RightAngleAxes);
			ChartSerializatorCommon.SerializeValueTag(writer, "perspective", (chart.Perspective * 2).ToString());
			writer.WriteEndElement();
		}
	}

	private void SerializeErrorBars(XmlWriter writer, IOfficeChartErrorBars errorBars, string direction, IWorkbook book, ChartSerieImpl series)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (errorBars == null)
		{
			return;
		}
		if (direction == null || direction.Length == 0)
		{
			throw new ArgumentOutOfRangeException("direction");
		}
		writer.WriteStartElement("errBars", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		if (ChartFormatImpl.GetStartSerieType(series.SerieType) != "Bar")
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "errDir", direction);
		}
		OfficeErrorBarInclude include = errorBars.Include;
		string value = include.ToString().ToLower();
		ChartSerializatorCommon.SerializeValueTag(writer, "errBarType", value);
		Excel2007ErrorBarType type = (Excel2007ErrorBarType)errorBars.Type;
		ChartSerializatorCommon.SerializeValueTag(writer, "errValType", type.ToString());
		ChartErrorBarsImpl chartErrorBarsImpl = errorBars as ChartErrorBarsImpl;
		ChartSerializatorCommon.SerializeBoolValueTag(writer, "noEndCap", !errorBars.HasCap);
		if (type == Excel2007ErrorBarType.cust)
		{
			if ((include == OfficeErrorBarInclude.Plus || include == OfficeErrorBarInclude.Both) && (errorBars.PlusRange != null || chartErrorBarsImpl.PlusRangeValues != null))
			{
				writer.WriteStartElement("plus", "http://schemas.openxmlformats.org/drawingml/2006/chart");
				if (!chartErrorBarsImpl.IsPlusNumberLiteral)
				{
					SerializeNumReference(writer, (errorBars.PlusRange as ChartDataRange).Range, chartErrorBarsImpl.PlusRangeValues, series, "plus");
				}
				else
				{
					SerializeDirectlyEntered(writer, chartErrorBarsImpl.PlusRangeValues, isCache: false);
				}
				writer.WriteEndElement();
			}
			if ((include == OfficeErrorBarInclude.Minus || include == OfficeErrorBarInclude.Both) && (errorBars.MinusRange != null || chartErrorBarsImpl.MinusRangeValues != null))
			{
				writer.WriteStartElement("minus", "http://schemas.openxmlformats.org/drawingml/2006/chart");
				if (!chartErrorBarsImpl.IsMinusNumberLiteral)
				{
					SerializeNumReference(writer, (errorBars.MinusRange as ChartDataRange).Range, chartErrorBarsImpl.MinusRangeValues, series, "minus");
				}
				else
				{
					SerializeDirectlyEntered(writer, chartErrorBarsImpl.MinusRangeValues, isCache: false);
				}
				writer.WriteEndElement();
			}
		}
		ChartSerializatorCommon.SerializeValueTag(writer, "val", XmlConvert.ToString(errorBars.NumberValue));
		IOfficeChartBorder border = errorBars.Border;
		if (border != null && !border.AutoFormat)
		{
			writer.WriteStartElement("spPr", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			ChartSerializatorCommon.SerializeLineProperties(writer, border, book);
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	private void SerializeTrendlines(XmlWriter writer, IOfficeChartTrendLines trendlines, IWorkbook book)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (trendlines != null)
		{
			int i = 0;
			for (int count = trendlines.Count; i < count; i++)
			{
				SerializeTrendline(writer, trendlines[i], book);
			}
		}
	}

	private void SerializeTrendline(XmlWriter writer, IOfficeChartTrendLine trendline, IWorkbook book)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (trendline == null)
		{
			throw new ArgumentNullException("trendline");
		}
		writer.WriteStartElement("trendline", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		string name = trendline.Name;
		if (name != null && !trendline.NameIsAuto)
		{
			writer.WriteElementString("name", "http://schemas.openxmlformats.org/drawingml/2006/chart", name);
		}
		IOfficeChartBorder border = trendline.Border;
		if (border != null && !border.AutoFormat)
		{
			writer.WriteStartElement("spPr", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			ChartSerializatorCommon.SerializeLineProperties(writer, trendline.Border, book);
			writer.WriteEndElement();
		}
		ChartSerializatorCommon.SerializeValueTag(writer, "trendlineType", ((Excel2007TrendlineType)trendline.Type).ToString());
		string text = null;
		if (trendline.Type == OfficeTrendLineType.Polynomial)
		{
			text = "order";
		}
		else if (trendline.Type == OfficeTrendLineType.Moving_Average)
		{
			text = "period";
		}
		if (text != null)
		{
			ChartSerializatorCommon.SerializeValueTag(writer, text, trendline.Order.ToString());
		}
		ChartSerializatorCommon.SerializeValueTag(writer, "forward", XmlConvert.ToString(trendline.Forward));
		ChartSerializatorCommon.SerializeValueTag(writer, "backward", XmlConvert.ToString(trendline.Backward));
		if (!trendline.InterceptIsAuto)
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "intercept", XmlConvert.ToString(trendline.Intercept));
		}
		ChartSerializatorCommon.SerializeBoolValueTag(writer, "dispRSqr", trendline.DisplayRSquared);
		ChartSerializatorCommon.SerializeBoolValueTag(writer, "dispEq", trendline.DisplayEquation);
		if (trendline.DisplayRSquared || trendline.DisplayEquation)
		{
			SerializeTrendlineLabel(writer, trendline.DataLabel);
		}
		writer.WriteEndElement();
	}

	private void SerializeTrendlineLabel(XmlWriter writer, IOfficeChartTextArea dataLabelFormat)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (dataLabelFormat == null)
		{
			throw new ArgumentNullException("dataLabelFormat");
		}
		writer.WriteStartElement("trendlineLbl", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		writer.WriteElementString("layout", "http://schemas.openxmlformats.org/drawingml/2006/chart", string.Empty);
		writer.WriteStartElement("numFmt", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		writer.WriteAttributeString("formatCode", "General");
		writer.WriteAttributeString("sourceLinked", "0");
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	[SecurityCritical]
	private void SerializeSurface(XmlWriter writer, IOfficeChartWallOrFloor surface, string mainTagName, ChartImpl chart)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (surface == null)
		{
			throw new ArgumentNullException("surface");
		}
		if (mainTagName == null || mainTagName.Length == 0)
		{
			throw new ArgumentOutOfRangeException("mainTagName");
		}
		ChartWallOrFloorImpl chartWallOrFloorImpl = (ChartWallOrFloorImpl)surface;
		writer.WriteStartElement(mainTagName, "http://schemas.openxmlformats.org/drawingml/2006/chart");
		if (chartWallOrFloorImpl.Thickness != -1 || (chart.Workbook as WorkbookImpl).IsConverted)
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "thickness", chartWallOrFloorImpl.Thickness.ToString());
		}
		if (chartWallOrFloorImpl.HasShapeProperties || (chart.Workbook as WorkbookImpl).IsConverted)
		{
			ChartSerializatorCommon.SerializeFrameFormat(writer, surface, chart, isRoundCorners: false);
		}
		if (chartWallOrFloorImpl.PictureUnit == OfficeChartPictureType.stack)
		{
			writer.WriteStartElement("pictureOptions", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteStartElement("pictureFormat", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteAttributeString("val", chartWallOrFloorImpl.PictureUnit.ToString());
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	[SecurityCritical]
	private void SerializePlotArea(XmlWriter writer, ChartImpl chart, RelationCollection relations)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("plotArea", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		if (chart.PlotArea != null && chart.PlotArea.Layout != null)
		{
			ChartSerializatorCommon.SerializeLayout(writer, chart.PlotArea);
		}
		int count = chart.Series.Count;
		int num = 0;
		int num2 = 0;
		while (num != count)
		{
			num += SerializeMainChartTypeTag(writer, chart, num2);
			num2++;
		}
		if (num == 0)
		{
			if (count == 0 && chart.IsChartCleared && !chart.IsChartBar)
			{
				writer.WriteStartElement(Helper.GetOfficeChartType(chart.ChartType), "http://schemas.openxmlformats.org/drawingml/2006/chart");
			}
			else
			{
				writer.WriteStartElement("barChart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
				ChartSerializatorCommon.SerializeValueTag(writer, "barDir", "col");
				ChartSerializatorCommon.SerializeValueTag(writer, "grouping", "clustered");
			}
			ChartAxisImpl chartAxisImpl = (ChartAxisImpl)chart.PrimaryCategoryAxis;
			ChartSerializatorCommon.SerializeValueTag(writer, "axId", chartAxisImpl.AxisId.ToString());
			chart.SerializedAxisIds.Add(chartAxisImpl.AxisId);
			chartAxisImpl = (ChartAxisImpl)chart.PrimaryValueAxis;
			ChartSerializatorCommon.SerializeValueTag(writer, "axId", chartAxisImpl.AxisId.ToString());
			chart.SerializedAxisIds.Add(chartAxisImpl.AxisId);
			writer.WriteEndElement();
		}
		SerializeAxes(writer, chart, relations);
		SerializeDataTable(writer, chart);
		if (chart.HasPlotArea)
		{
			IOfficeChartFrameFormat plotArea = chart.PlotArea;
			ChartSerializatorCommon.SerializeFrameFormat(writer, plotArea, chart, plotArea.IsBorderCornersRound);
		}
		writer.WriteEndElement();
	}

	private void SerializeBarChart(XmlWriter writer, ChartImpl chart)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("barChart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		string value = (chart.PivotChartType.ToString().Contains("Bar") ? "bar" : "col");
		ChartSerializatorCommon.SerializeValueTag(writer, "barDir", value);
		SerializeChartGrouping(writer, chart);
		ChartSerializatorCommon.SerializeValueTag(writer, "overlap", chart.OverLap.ToString());
	}

	[SecurityCritical]
	private int SerializeBarChart(XmlWriter writer, ChartImpl chart, ChartSerieImpl firstSeries)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("barChart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		int result = SerializeBarChartShared(writer, chart, firstSeries);
		IOfficeChartFormat commonSerieOptions = firstSeries.SerieFormat.CommonSerieOptions;
		int num = 0;
		int num2 = 0;
		bool flag = false;
		bool flag2 = false;
		if ((chart.Workbook as WorkbookImpl).IsCreated)
		{
			if (firstSeries.GapWidth != 0 || firstSeries.Overlap != 0)
			{
				num = firstSeries.GapWidth;
				num2 = firstSeries.Overlap;
				flag = firstSeries.ShowGapWidth;
				flag2 = true;
			}
			if (chart.GapWidth != commonSerieOptions.GapWidth || chart.OverLap != commonSerieOptions.Overlap)
			{
				num = commonSerieOptions.GapWidth;
				num2 = commonSerieOptions.Overlap;
			}
		}
		if (num2 == 0 && !flag2)
		{
			num2 = commonSerieOptions.Overlap;
		}
		if (num == 0 && !flag)
		{
			num = commonSerieOptions.GapWidth;
		}
		ChartSerializatorCommon.SerializeValueTag(writer, "gapWidth", num.ToString());
		if (num2 == -65436)
		{
			num2 = 100;
		}
		if (num2 != 0)
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "overlap", num2.ToString());
		}
		if (commonSerieOptions.HasSeriesLines && (chart.ChartType == OfficeChartType.Bar_Stacked || chart.ChartType == OfficeChartType.Bar_Stacked_100))
		{
			IOfficeChartBorder pieSeriesLine = commonSerieOptions.PieSeriesLine;
			writer.WriteStartElement("serLines", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			writer.WriteStartElement("spPr", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			ChartSerializatorCommon.SerializeLineProperties(writer, pieSeriesLine, chart.Workbook);
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
		if (chart.IsPrimarySecondaryCategory && chart.IsClustered)
		{
			firstSeries.UsePrimaryAxis = true;
		}
		SerializeBarAxisId(writer, chart, firstSeries);
		writer.WriteEndElement();
		return result;
	}

	private void SerializeBarAxisId(XmlWriter writer, ChartImpl chart, ChartSerieImpl firstSeries)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (firstSeries == null)
		{
			throw new ArgumentNullException("firstSeries");
		}
		bool usePrimaryAxis = firstSeries.UsePrimaryAxis;
		ChartAxisImpl chartAxisImpl = (ChartAxisImpl)(usePrimaryAxis ? chart.PrimaryCategoryAxis : chart.SecondaryCategoryAxis);
		if (chartAxisImpl == null)
		{
			throw new ArgumentNullException("axis");
		}
		ChartSerializatorCommon.SerializeValueTag(writer, "axId", chartAxisImpl.AxisId.ToString());
		chart.SerializedAxisIds.Add(chartAxisImpl.AxisId);
		chartAxisImpl = (ChartAxisImpl)(usePrimaryAxis ? chart.PrimaryValueAxis : chart.SecondaryValueAxis);
		ChartSerializatorCommon.SerializeValueTag(writer, "axId", chartAxisImpl.AxisId.ToString());
		chart.SerializedAxisIds.Add(chartAxisImpl.AxisId);
		if (chart.IsSeriesAxisAvail)
		{
			chartAxisImpl = (ChartAxisImpl)chart.PrimarySerieAxis;
			ChartSerializatorCommon.SerializeValueTag(writer, "axId", chartAxisImpl.AxisId.ToString());
		}
	}

	private void SerializeBar3DChart(XmlWriter writer, ChartImpl chart)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("bar3DChart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		string text = chart.PivotChartType.ToString();
		string value = (text.Contains("Bar") ? "bar" : "col");
		ChartSerializatorCommon.SerializeValueTag(writer, "barDir", value);
		SerializeChartGrouping(writer, chart);
		if (text.Contains("Cone") || text.Contains("Cylinder") || text.Contains("Pyramid"))
		{
			OfficeBaseFormat baseFormat;
			OfficeTopFormat topFormat;
			switch (chart.PivotChartType)
			{
			case OfficeChartType.Cone_Clustered:
			case OfficeChartType.Cone_Stacked:
			case OfficeChartType.Cone_Stacked_100:
			case OfficeChartType.Cone_Bar_Clustered:
			case OfficeChartType.Cone_Bar_Stacked:
			case OfficeChartType.Cone_Bar_Stacked_100:
			case OfficeChartType.Cone_Clustered_3D:
				baseFormat = OfficeBaseFormat.Circle;
				topFormat = OfficeTopFormat.Sharp;
				break;
			case OfficeChartType.Pyramid_Clustered:
			case OfficeChartType.Pyramid_Stacked:
			case OfficeChartType.Pyramid_Stacked_100:
			case OfficeChartType.Pyramid_Bar_Clustered:
			case OfficeChartType.Pyramid_Bar_Stacked:
			case OfficeChartType.Pyramid_Bar_Stacked_100:
			case OfficeChartType.Pyramid_Clustered_3D:
				baseFormat = OfficeBaseFormat.Rectangle;
				topFormat = OfficeTopFormat.Sharp;
				break;
			case OfficeChartType.Cylinder_Clustered:
			case OfficeChartType.Cylinder_Stacked:
			case OfficeChartType.Cylinder_Stacked_100:
			case OfficeChartType.Cylinder_Bar_Clustered:
			case OfficeChartType.Cylinder_Bar_Stacked:
			case OfficeChartType.Cylinder_Bar_Stacked_100:
			case OfficeChartType.Cylinder_Clustered_3D:
				baseFormat = OfficeBaseFormat.Circle;
				topFormat = OfficeTopFormat.Straight;
				break;
			default:
				throw new ArgumentException("type");
			}
			SerializeBarShape(writer, baseFormat, topFormat);
		}
	}

	[SecurityCritical]
	private int SerializeBar3DChart(XmlWriter writer, ChartImpl chart, ChartSerieImpl firstSeries)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("bar3DChart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		int result = SerializeBarChartShared(writer, chart, firstSeries);
		ChartSerializatorCommon.SerializeValueTag(writer, "gapWidth", firstSeries.SerieFormat.CommonSerieOptions.GapWidth.ToString());
		SerializeGapDepth(writer, chart);
		SerializeBarShape(writer, chart, firstSeries, isSerieCalled: false);
		SerializeBarAxisId(writer, chart, firstSeries);
		writer.WriteEndElement();
		return result;
	}

	private void SerializeGapDepth(XmlWriter writer, ChartImpl chart)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		ChartSerializatorCommon.SerializeValueTag(writer, "gapDepth", chart.GapDepth.ToString());
	}

	private void SerializeBarShape(XmlWriter writer, OfficeBaseFormat baseFormat, OfficeTopFormat topFormat)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		string value = null;
		switch (topFormat)
		{
		case OfficeTopFormat.Sharp:
			value = ((baseFormat == OfficeBaseFormat.Circle) ? "cone" : "pyramid");
			break;
		case OfficeTopFormat.Trunc:
			value = ((baseFormat == OfficeBaseFormat.Circle) ? "coneToMax" : "pyramidToMax");
			break;
		case OfficeTopFormat.Straight:
			value = ((baseFormat == OfficeBaseFormat.Circle) ? "cylinder" : "box");
			break;
		}
		ChartSerializatorCommon.SerializeValueTag(writer, "shape", value);
	}

	private void SerializeBarShape(XmlWriter writer, ChartImpl chart, ChartSerieImpl firstSeries, bool isSerieCalled)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		IOfficeChartSerieDataFormat officeChartSerieDataFormat = null;
		officeChartSerieDataFormat = ((!isSerieCalled) ? chart.ChartFormat.SerieDataFormat : firstSeries.SerieFormat);
		OfficeTopFormat barShapeTop = officeChartSerieDataFormat.BarShapeTop;
		OfficeBaseFormat barShapeBase = officeChartSerieDataFormat.BarShapeBase;
		string value = null;
		switch (barShapeTop)
		{
		case OfficeTopFormat.Sharp:
			value = ((barShapeBase == OfficeBaseFormat.Circle) ? "cone" : "pyramid");
			break;
		case OfficeTopFormat.Trunc:
			value = ((barShapeBase == OfficeBaseFormat.Circle) ? "coneToMax" : "pyramidToMax");
			break;
		case OfficeTopFormat.Straight:
			value = ((barShapeBase == OfficeBaseFormat.Circle) ? "cylinder" : "box");
			break;
		}
		ChartSerializatorCommon.SerializeValueTag(writer, "shape", value);
	}

	[SecurityCritical]
	private int SerializeBarChartShared(XmlWriter writer, ChartImpl chart, ChartSerieImpl firstSeries)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		string value = (firstSeries.SerieType.ToString().Contains("Bar") ? "bar" : "col");
		ChartSerializatorCommon.SerializeValueTag(writer, "barDir", value);
		SerializeChartGrouping(writer, firstSeries);
		SerializeVaryColors(writer, firstSeries);
		int result = SerializeChartSeries(writer, chart, firstSeries, SerializeBarSeries);
		if (chart.CommonDataPointsCollection != null && chart.CommonDataPointsCollection.ContainsKey(firstSeries.ChartGroup))
		{
			SerializeDataLabels(writer, chart, chart.CommonDataPointsCollection[firstSeries.ChartGroup]);
		}
		return result;
	}

	[SecurityCritical]
	private int SerializeChartSeries(XmlWriter writer, ChartImpl chart, ChartSerieImpl firstSeries, SerializeSeriesDelegate serializator)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (firstSeries == null)
		{
			throw new ArgumentNullException("firstSeries");
		}
		if (serializator == null)
		{
			throw new ArgumentNullException("serializator");
		}
		int chartGroup = firstSeries.ChartGroup;
		IList<IOfficeChartSerie> additionOrder = (chart.Series as ChartSeriesCollection).AdditionOrder;
		IOfficeChartSeries series = chart.Series;
		IList<IOfficeChartSerie> list = ((additionOrder.Count == chart.Series.Count) ? additionOrder : ((IList<IOfficeChartSerie>)chart.Series));
		int seriesIndex = GetSeriesIndex(firstSeries, list, series);
		firstSeries = list[seriesIndex] as ChartSerieImpl;
		List<ChartSerieImpl> list2 = new List<ChartSerieImpl>();
		if (!firstSeries.IsFiltered)
		{
			serializator(writer, firstSeries);
		}
		else
		{
			list2.Add(firstSeries);
		}
		int num = 1;
		int i = seriesIndex + 1;
		for (int count = list.Count; i < count; i++)
		{
			ChartSerieImpl chartSerieImpl = (ChartSerieImpl)list[i];
			if (chartSerieImpl.ChartGroup == chartGroup)
			{
				if (!chartSerieImpl.IsFiltered)
				{
					serializator(writer, chartSerieImpl);
				}
				else
				{
					list2.Add(chartSerieImpl);
				}
				num++;
			}
		}
		if (list2.Count != 0)
		{
			writer.WriteStartElement("extLst", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			writer.WriteStartElement("c", "ext", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			writer.WriteAttributeString("uri", "{02D57815-91ED-43cb-92C2-25804820EDAC}");
			writer.WriteAttributeString("xmlns", "c15", null, "http://schemas.microsoft.com/office/drawing/2012/chart");
			if (categoryFilter == 0)
			{
				findFilter = FindFiltered(list2[0]);
				categoryFilter++;
				if (findFilter)
				{
					UpdateCategoryLabel(list2[0]);
					UpdateFilteredValuesRange(list2[0]);
				}
			}
			for (int j = 0; j < list2.Count; j++)
			{
				SerializeFilteredSeries(writer, list2[j]);
			}
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
		return num;
	}

	[SecurityCritical]
	private void SerializeFilteredSeries(XmlWriter writer, ChartSerieImpl series)
	{
		string seriesType = GetSeriesType(series.StartType);
		writer.WriteStartElement("c15", seriesType, "http://schemas.microsoft.com/office/drawing/2012/chart");
		writer.WriteStartElement("c15", "ser", "http://schemas.microsoft.com/office/drawing/2012/chart");
		SerializeFilterSeries(writer, series);
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	[SecurityCritical]
	private void SerializeFilterSeries(XmlWriter writer, ChartSerieImpl series)
	{
		writer.WriteStartElement("c", "idx", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		writer.WriteAttributeString("val", series.Number.ToString());
		writer.WriteEndElement();
		writer.WriteStartElement("c", "order", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		writer.WriteAttributeString("val", series.Index.ToString());
		writer.WriteEndElement();
		if (series.ParentChart.SeriesNameLevel == OfficeSeriesNameLevel.SeriesNameLevelAll)
		{
			SerializeFilteredText(writer, series);
		}
		SerializeSeriesCommonWithoutEnd(writer, series, isFiltered: true);
		int percent = series.SerieFormat.Percent;
		if (percent != 0)
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "explosion", percent.ToString());
		}
		if ((series.DataPoints.DefaultDataPoint as ChartDataPointImpl).HasDataLabels || (series.DataPoints as ChartDataPointsCollection).CheckDPDataLabels())
		{
			SerializeDataLabels(writer, series.DataPoints.DefaultDataPoint.DataLabels, series);
		}
		SerializeTrendlines(writer, series.TrendLines, series.ParentBook);
		SerializeErrorBars(writer, series);
		if (series.ParentChart.CategoryLabelLevel == OfficeCategoriesLabelLevel.CategoriesLabelLevelAll)
		{
			SerializeFilteredCategory(writer, series, FindFiltered(series));
		}
		SerializeFilteredValues(writer, series, FindFiltered(series));
		if (series.SerieType == OfficeChartType.Bubble || series.SerieType == OfficeChartType.Bubble_3D)
		{
			SerializeSeriesValues(writer, series.BubblesIRange, series.EnteredDirectlyBubbles, "bubbleSize", series);
		}
		if (series.ParentChart.CategoryLabelLevel != 0 || series.ParentChart.SeriesNameLevel != 0)
		{
			writer.WriteStartElement("extLst", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			if (series.ParentChart.SeriesNameLevel != 0)
			{
				SerializeFilteredSeriesOrCategoryName(writer, series, seriesOrcategory: true);
			}
			if (series.CategoryLabelsIRange != null && series.ParentChart.CategoryLabelLevel != 0)
			{
				SerializeFilteredSeriesOrCategoryName(writer, series, seriesOrcategory: false);
			}
			writer.WriteEndElement();
		}
	}

	private void SerializeFilteredText(XmlWriter writer, ChartSerieImpl series)
	{
		if (!series.IsDefaultName)
		{
			if (series.ParentChart.SeriesNameLevel != 0)
			{
				writer.WriteStartElement("c", "tx", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			}
			else
			{
				writer.WriteStartElement("c15", "tx", "http://schemas.microsoft.com/office/drawing/2012/chart");
			}
			string nameOrFormula = series.NameOrFormula;
			if (nameOrFormula.Length > 0 && nameOrFormula[0] == '=')
			{
				SerializeFiltedStringReference(writer, nameOrFormula, series);
			}
			else
			{
				writer.WriteStartElement("v", "http://schemas.openxmlformats.org/drawingml/2006/chart");
				writer.WriteString(nameOrFormula);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}
	}

	private void SerializeFiltedStringReference(XmlWriter writer, string range, ChartSerieImpl series)
	{
		writer.WriteStartElement("c", "strRef", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		writer.WriteStartElement("c", "extLst", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		writer.WriteStartElement("c", "ext", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		writer.WriteAttributeString("uri", "{02D57815-91ED-43cb-92C2-25804820EDAC}");
		writer.WriteStartElement("c15", "formulaRef", "http://schemas.microsoft.com/office/drawing/2012/chart");
		if (range == null)
		{
			throw new ArgumentNullException("range");
		}
		if (range[0] == '=')
		{
			range = UtilityMethods.RemoveFirstCharUnsafe(range);
		}
		if (series.StrRefFormula != null)
		{
			range = series.StrRefFormula;
		}
		writer.WriteElementString("sqref", "http://schemas.microsoft.com/office/drawing/2012/chart", range);
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	private void SerializeFilteredCategory(XmlWriter writer, ChartSerieImpl series, bool categoryfilter)
	{
		if (series.CategoryLabelsIRange == null)
		{
			return;
		}
		if (series.SerieType == OfficeChartType.Bubble || series.SerieType == OfficeChartType.Bubble_3D || series.StartType.Contains("Scatter"))
		{
			writer.WriteStartElement("c", "xVal", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		}
		else
		{
			writer.WriteStartElement("c", "cat", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		}
		writer.WriteStartElement("c", "strRef", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		writer.WriteStartElement("c", "extLst", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		writer.WriteStartElement("c", "ext", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		writer.WriteAttributeString("uri", "{02D57815-91ED-43cb-92C2-25804820EDAC}");
		if (categoryfilter && series.IsFiltered)
		{
			SerializeFilteredFullReference(writer, series, catorval: false);
			writer.WriteStartElement("c15", "formulaRef", "http://schemas.microsoft.com/office/drawing/2012/chart");
			writer.WriteElementString("sqref", "http://schemas.microsoft.com/office/drawing/2012/chart", series.FilteredCategory);
			writer.WriteEndElement();
		}
		else if (series.IsFiltered)
		{
			writer.WriteStartElement("c15", "formulaRef", "http://schemas.microsoft.com/office/drawing/2012/chart");
			string value = null;
			if (series.CategoryLabelsIRange != null)
			{
				value = series.CategoryLabelsIRange.AddressGlobal;
			}
			writer.WriteElementString("sqref", "http://schemas.microsoft.com/office/drawing/2012/chart", value);
			writer.WriteEndElement();
		}
		else
		{
			SerializeFilteredFullReference(writer, series, catorval: false);
		}
		writer.WriteEndElement();
		writer.WriteEndElement();
		if (!series.IsFiltered && categoryfilter)
		{
			writer.WriteElementString("f", "http://schemas.openxmlformats.org/drawingml/2006/chart", series.FilteredCategory);
		}
		writer.WriteElementString("strCache", "http://schemas.openxmlformats.org/drawingml/2006/chart", string.Empty);
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	private void SerializeFilteredValues(XmlWriter writer, ChartSerieImpl series, bool categoryfilter)
	{
		if (series.SerieType == OfficeChartType.Bubble || series.SerieType == OfficeChartType.Bubble_3D || series.StartType.Contains("Scatter"))
		{
			writer.WriteStartElement("c", "yVal", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		}
		else
		{
			writer.WriteStartElement("c", "val", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		}
		writer.WriteStartElement("c", "numRef", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		writer.WriteStartElement("c", "extLst", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		writer.WriteStartElement("c", "ext", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		writer.WriteAttributeString("uri", "{02D57815-91ED-43cb-92C2-25804820EDAC}");
		if (categoryfilter && series.IsFiltered)
		{
			SerializeFilteredFullReference(writer, series, catorval: true);
			writer.WriteStartElement("c15", "formulaRef", "http://schemas.microsoft.com/office/drawing/2012/chart");
			writer.WriteElementString("sqref", "http://schemas.microsoft.com/office/drawing/2012/chart", series.FilteredValue);
			writer.WriteEndElement();
		}
		else if (series.IsFiltered)
		{
			writer.WriteStartElement("c15", "formulaRef", "http://schemas.microsoft.com/office/drawing/2012/chart");
			string value = null;
			if (series.ValuesIRange != null)
			{
				value = series.ValuesIRange.AddressGlobal;
			}
			writer.WriteElementString("sqref", "http://schemas.microsoft.com/office/drawing/2012/chart", value);
			writer.WriteEndElement();
		}
		else
		{
			SerializeFilteredFullReference(writer, series, catorval: true);
		}
		writer.WriteEndElement();
		writer.WriteEndElement();
		if (!series.IsFiltered && categoryfilter)
		{
			writer.WriteElementString("f", "http://schemas.openxmlformats.org/drawingml/2006/chart", series.FilteredValue);
		}
		writer.WriteElementString("numCache", "http://schemas.openxmlformats.org/drawingml/2006/chart", string.Empty);
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	public string GetSeriesType(string Series)
	{
		string[] array = new string[8] { "filteredBarSeries", "filteredAreaSeries", "filteredLineSeries", "filteredPieSeries", "filteredRadarSeries", "filteredScatterSeries", "filteredSurfaceSeries", "filteredBubbleSeries" };
		int num = 0;
		for (num = 0; array.Length > num && !array[num].Contains(Series); num++)
		{
		}
		return array[num % 8];
	}

	private int GetSeriesIndex(ChartSerieImpl firstSeries, IList<IOfficeChartSerie> arrOrderedSeries, IOfficeChartSeries arrSeries)
	{
		int result = -1;
		if (arrSeries == arrOrderedSeries)
		{
			result = firstSeries.Index;
		}
		else
		{
			int i = 0;
			for (int count = arrOrderedSeries.Count; i < count; i++)
			{
				if ((arrOrderedSeries[i] as ChartSerieImpl).ChartGroup == firstSeries.ChartGroup)
				{
					result = i;
					break;
				}
			}
		}
		return result;
	}

	private void SerializeFilteredFullReference(XmlWriter writer, ChartSerieImpl series, bool catorval)
	{
		writer.WriteStartElement("c15", "fullRef", "http://schemas.microsoft.com/office/drawing/2012/chart");
		string value = null;
		if (catorval)
		{
			if (series.ValuesIRange != null)
			{
				value = series.ValuesIRange.AddressGlobal;
			}
		}
		else
		{
			value = series.CategoryLabelsIRange.AddressGlobal;
		}
		writer.WriteElementString("sqref", "http://schemas.microsoft.com/office/drawing/2012/chart", value);
		writer.WriteEndElement();
	}

	private void SerializeChartGrouping(XmlWriter writer, ChartSerieImpl firstSeries)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (firstSeries == null)
		{
			throw new ArgumentNullException("firstSeries");
		}
		OfficeChartType serieType = firstSeries.SerieType;
		string value = (ChartImpl.GetIsClustered(serieType) ? "clustered" : (ChartImpl.GetIs100(serieType) ? "percentStacked" : ((!ChartImpl.GetIsStacked(serieType)) ? "standard" : "stacked")));
		ChartSerializatorCommon.SerializeValueTag(writer, "grouping", value);
	}

	private void SerializeChartGrouping(XmlWriter writer, ChartImpl chart)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("firstSeries");
		}
		OfficeChartType pivotChartType = chart.PivotChartType;
		string value = (ChartImpl.GetIsClustered(pivotChartType) ? "clustered" : (ChartImpl.GetIs100(pivotChartType) ? "percentStacked" : ((!ChartImpl.GetIsStacked(pivotChartType)) ? "standard" : "stacked")));
		ChartSerializatorCommon.SerializeValueTag(writer, "grouping", value);
	}

	private void SerializeArea3DChart(XmlWriter writer, ChartImpl chart)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("area3DChart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		SerializeChartGrouping(writer, chart);
	}

	[SecurityCritical]
	private int SerializeArea3DChart(XmlWriter writer, ChartImpl chart, ChartSerieImpl firstSeries)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("area3DChart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		int result = SerializeAreaChartCommon(writer, chart, firstSeries);
		SerializeBarAxisId(writer, chart, firstSeries);
		writer.WriteEndElement();
		return result;
	}

	private void SerializeAreaChart(XmlWriter writer, ChartImpl chart)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("areaChart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		SerializeChartGrouping(writer, chart);
	}

	[SecurityCritical]
	private int SerializeAreaChart(XmlWriter writer, ChartImpl chart, ChartSerieImpl firstSeries)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("areaChart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		int result = SerializeAreaChartCommon(writer, chart, firstSeries);
		SerializeBarAxisId(writer, chart, firstSeries);
		writer.WriteEndElement();
		return result;
	}

	[SecurityCritical]
	private int SerializeAreaChartCommon(XmlWriter writer, ChartImpl chart, ChartSerieImpl firstSeries)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		ChartFormatImpl chartFormatImpl = (ChartFormatImpl)firstSeries.SerieFormat.CommonSerieOptions;
		SerializeChartGrouping(writer, firstSeries);
		SerializeVaryColors(writer, firstSeries);
		int result = SerializeChartSeries(writer, chart, firstSeries, SerializeAreaSeries);
		if (chart.CommonDataPointsCollection != null && chart.CommonDataPointsCollection.ContainsKey(firstSeries.ChartGroup))
		{
			SerializeDataLabels(writer, chart, chart.CommonDataPointsCollection[firstSeries.ChartGroup]);
		}
		if (chartFormatImpl.HasDropLines)
		{
			IOfficeChartBorder dropLines = chartFormatImpl.DropLines;
			writer.WriteStartElement("dropLines", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			writer.WriteStartElement("spPr", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			ChartSerializatorCommon.SerializeLineProperties(writer, dropLines, chart.Workbook);
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
		return result;
	}

	[SecurityCritical]
	private int SerializeLineChartCommon(XmlWriter writer, ChartImpl chart, ChartSerieImpl firstSeries)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (!chart.IsChartStock)
		{
			SerializeChartGrouping(writer, firstSeries);
			SerializeVaryColors(writer, firstSeries);
		}
		int result = SerializeChartSeries(writer, chart, firstSeries, SerializeLineSeries);
		if (chart.CommonDataPointsCollection != null && chart.CommonDataPointsCollection.ContainsKey(firstSeries.ChartGroup))
		{
			SerializeDataLabels(writer, chart, chart.CommonDataPointsCollection[firstSeries.ChartGroup]);
		}
		return result;
	}

	private void SerializeLine3DChart(XmlWriter writer, ChartImpl chart)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("line3DChart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		SerializeChartGrouping(writer, chart);
		ChartSerializatorCommon.SerializeValueTag(writer, "marker", "1");
	}

	[SecurityCritical]
	private int SerializeLine3DChart(XmlWriter writer, ChartImpl chart, ChartSerieImpl firstSeries)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("line3DChart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		int result = SerializeLineChartCommon(writer, chart, firstSeries);
		IOfficeChartFormat commonSerieOptions = firstSeries.SerieFormat.CommonSerieOptions;
		if (commonSerieOptions.HasDropLines)
		{
			IOfficeChartBorder dropLines = commonSerieOptions.DropLines;
			writer.WriteStartElement("dropLines", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			writer.WriteStartElement("spPr", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			ChartSerializatorCommon.SerializeLineProperties(writer, dropLines, chart.Workbook);
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
		SerializeBarAxisId(writer, chart, firstSeries);
		writer.WriteEndElement();
		return result;
	}

	private void SerializeLineChart(XmlWriter writer, ChartImpl chart)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		string localName = "lineChart";
		writer.WriteStartElement(localName, "http://schemas.openxmlformats.org/drawingml/2006/chart");
		SerializeChartGrouping(writer, chart);
		ChartSerializatorCommon.SerializeValueTag(writer, "marker", "1");
	}

	[SecurityCritical]
	private int SerializeLineChart(XmlWriter writer, ChartImpl chart, ChartSerieImpl firstSeries)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		string localName = (chart.IsChartStock ? "stockChart" : "lineChart");
		writer.WriteStartElement(localName, "http://schemas.openxmlformats.org/drawingml/2006/chart");
		int result = SerializeLineChartCommon(writer, chart, firstSeries);
		ChartSerieDataFormatImpl obj = (ChartSerieDataFormatImpl)firstSeries.SerieFormat;
		ChartFormatImpl chartFormatImpl = (ChartFormatImpl)firstSeries.SerieFormat.CommonSerieOptions;
		if (chartFormatImpl.IsChartChartLine)
		{
			if (chartFormatImpl.HasHighLowLines)
			{
				if (chartFormatImpl.IsChartLineFormat)
				{
					writer.WriteStartElement("hiLowLines", "http://schemas.openxmlformats.org/drawingml/2006/chart");
					writer.WriteStartElement("spPr", "http://schemas.openxmlformats.org/drawingml/2006/chart");
					ChartSerializatorCommon.SerializeLineProperties(writer, chartFormatImpl.HighLowLines, chart.Workbook);
					writer.WriteEndElement();
					writer.WriteEndElement();
				}
				else
				{
					writer.WriteElementString("hiLowLines", "http://schemas.openxmlformats.org/drawingml/2006/chart", string.Empty);
				}
			}
			if (chartFormatImpl.HasDropLines)
			{
				if (chartFormatImpl.IsChartLineFormat)
				{
					writer.WriteStartElement("dropLines", "http://schemas.openxmlformats.org/drawingml/2006/chart");
					writer.WriteStartElement("spPr", "http://schemas.openxmlformats.org/drawingml/2006/chart");
					ChartSerializatorCommon.SerializeLineProperties(writer, chartFormatImpl.DropLines, chart.Workbook);
					writer.WriteEndElement();
					writer.WriteEndElement();
				}
				else
				{
					writer.WriteElementString("dropLines", "http://schemas.openxmlformats.org/drawingml/2006/chart", string.Empty);
				}
			}
		}
		SerializeUpDownBars(writer, chart, firstSeries);
		if (firstSeries != null && firstSeries.DropLinesStream != null)
		{
			firstSeries.DropLinesStream.Position = 0L;
			ShapeParser.WriteNodeFromStream(writer, firstSeries.DropLinesStream, writeNamespaces: true);
		}
		if (obj.IsMarker)
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "marker", "1");
		}
		SerializeBarAxisId(writer, chart, firstSeries);
		writer.WriteEndElement();
		return result;
	}

	[SecurityCritical]
	private int SerializeBubbleChart(XmlWriter writer, ChartImpl chart, ChartSerieImpl firstSeries)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("bubbleChart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		SerializeVaryColors(writer, firstSeries);
		int result = SerializeChartSeries(writer, chart, firstSeries, SerializeBubbleSeries);
		if (chart.CommonDataPointsCollection != null && chart.CommonDataPointsCollection.ContainsKey(firstSeries.ChartGroup))
		{
			SerializeDataLabels(writer, chart, chart.CommonDataPointsCollection[firstSeries.ChartGroup]);
		}
		IOfficeChartFormat commonSerieOptions = firstSeries.SerieFormat.CommonSerieOptions;
		int bubbleScale = commonSerieOptions.BubbleScale;
		if (bubbleScale != 100)
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "bubbleScale", bubbleScale.ToString());
		}
		if (commonSerieOptions.ShowNegativeBubbles)
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "showNegBubbles", "1");
		}
		string value = ((commonSerieOptions.SizeRepresents == ChartBubbleSize.Area) ? "area" : "w");
		ChartSerializatorCommon.SerializeValueTag(writer, "sizeRepresents", value);
		SerializeBarAxisId(writer, chart, firstSeries);
		writer.WriteEndElement();
		return result;
	}

	private void SerializeSurfaceChart(XmlWriter writer, ChartImpl chart)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("surfaceChart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		OfficeChartType pivotChartType = chart.PivotChartType;
		if (pivotChartType == OfficeChartType.Surface_NoColor_3D || pivotChartType == OfficeChartType.Surface_NoColor_Contour)
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "wireframe", "1");
		}
	}

	[SecurityCritical]
	private int SerializeSurfaceChart(XmlWriter writer, ChartImpl chart, ChartSerieImpl firstSeries)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("surfaceChart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		int result = SerializeSurfaceCommon(writer, chart, firstSeries);
		SerializeBarAxisId(writer, chart, firstSeries);
		writer.WriteEndElement();
		return result;
	}

	private void SerializeSurface3DChart(XmlWriter writer, ChartImpl chart)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("surface3DChart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		OfficeChartType pivotChartType = chart.PivotChartType;
		if (pivotChartType == OfficeChartType.Surface_NoColor_3D || pivotChartType == OfficeChartType.Surface_NoColor_Contour)
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "wireframe", "1");
		}
	}

	[SecurityCritical]
	private int SerializeSurface3DChart(XmlWriter writer, ChartImpl chart, ChartSerieImpl firstSeries)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("surface3DChart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		int result = SerializeSurfaceCommon(writer, chart, firstSeries);
		SerializeBarAxisId(writer, chart, firstSeries);
		writer.WriteEndElement();
		return result;
	}

	[SecurityCritical]
	private int SerializeSurfaceCommon(XmlWriter writer, ChartImpl chart, ChartSerieImpl firstSeries)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		OfficeChartType serieType = firstSeries.SerieType;
		if (serieType == OfficeChartType.Surface_NoColor_3D || serieType == OfficeChartType.Surface_NoColor_Contour)
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "wireframe", "1");
		}
		int result = SerializeChartSeries(writer, chart, firstSeries, SerializeBarSeries);
		SerializeBandFormats(writer, chart);
		return result;
	}

	private void SerializeBandFormats(XmlWriter writer, ChartImpl chart)
	{
		Stream preservedBandFormats = chart.PreservedBandFormats;
		if (preservedBandFormats != null && preservedBandFormats.Length > 0)
		{
			preservedBandFormats.Position = 0L;
			ShapeParser.WriteNodeFromStream(writer, preservedBandFormats);
		}
	}

	[SecurityCritical]
	private int SerializeMainChartTypeTag(XmlWriter writer, ChartImpl chart, int groupIndex)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		IOfficeChartSeries series = chart.Series;
		ChartSerieImpl chartSerieImpl = null;
		int i = 0;
		for (int count = series.Count; i < count; i++)
		{
			ChartSerieImpl chartSerieImpl2 = (ChartSerieImpl)series[i];
			if (chartSerieImpl2.ChartGroup == groupIndex)
			{
				chartSerieImpl = chartSerieImpl2;
				break;
			}
		}
		int result = 0;
		if (chartSerieImpl != null)
		{
			switch (chartSerieImpl.SerieType)
			{
			case OfficeChartType.Column_Clustered:
			case OfficeChartType.Column_Stacked:
			case OfficeChartType.Column_Stacked_100:
			case OfficeChartType.Bar_Clustered:
			case OfficeChartType.Bar_Stacked:
			case OfficeChartType.Bar_Stacked_100:
				result = SerializeBarChart(writer, chart, chartSerieImpl);
				break;
			case OfficeChartType.Column_Clustered_3D:
			case OfficeChartType.Column_Stacked_3D:
			case OfficeChartType.Column_Stacked_100_3D:
			case OfficeChartType.Column_3D:
			case OfficeChartType.Bar_Clustered_3D:
			case OfficeChartType.Bar_Stacked_3D:
			case OfficeChartType.Bar_Stacked_100_3D:
			case OfficeChartType.Cylinder_Clustered:
			case OfficeChartType.Cylinder_Stacked:
			case OfficeChartType.Cylinder_Stacked_100:
			case OfficeChartType.Cylinder_Bar_Clustered:
			case OfficeChartType.Cylinder_Bar_Stacked:
			case OfficeChartType.Cylinder_Bar_Stacked_100:
			case OfficeChartType.Cylinder_Clustered_3D:
			case OfficeChartType.Cone_Clustered:
			case OfficeChartType.Cone_Stacked:
			case OfficeChartType.Cone_Stacked_100:
			case OfficeChartType.Cone_Bar_Clustered:
			case OfficeChartType.Cone_Bar_Stacked:
			case OfficeChartType.Cone_Bar_Stacked_100:
			case OfficeChartType.Cone_Clustered_3D:
			case OfficeChartType.Pyramid_Clustered:
			case OfficeChartType.Pyramid_Stacked:
			case OfficeChartType.Pyramid_Stacked_100:
			case OfficeChartType.Pyramid_Bar_Clustered:
			case OfficeChartType.Pyramid_Bar_Stacked:
			case OfficeChartType.Pyramid_Bar_Stacked_100:
			case OfficeChartType.Pyramid_Clustered_3D:
				result = SerializeBar3DChart(writer, chart, chartSerieImpl);
				break;
			case OfficeChartType.Line:
			case OfficeChartType.Line_Stacked:
			case OfficeChartType.Line_Stacked_100:
			case OfficeChartType.Line_Markers:
			case OfficeChartType.Line_Markers_Stacked:
			case OfficeChartType.Line_Markers_Stacked_100:
				result = SerializeLineChart(writer, chart, chartSerieImpl);
				break;
			case OfficeChartType.Line_3D:
				result = SerializeLine3DChart(writer, chart, chartSerieImpl);
				break;
			case OfficeChartType.Pie:
			case OfficeChartType.Pie_Exploded:
				result = SerializePieChart(writer, chart, chartSerieImpl);
				break;
			case OfficeChartType.Pie_3D:
			case OfficeChartType.Pie_Exploded_3D:
				result = SerializePie3DChart(writer, chart, chartSerieImpl);
				break;
			case OfficeChartType.PieOfPie:
			case OfficeChartType.Pie_Bar:
				result = SerializeOfPieChart(writer, chart, chartSerieImpl);
				break;
			case OfficeChartType.Scatter_Markers:
			case OfficeChartType.Scatter_SmoothedLine_Markers:
			case OfficeChartType.Scatter_SmoothedLine:
			case OfficeChartType.Scatter_Line_Markers:
			case OfficeChartType.Scatter_Line:
				result = SerializeScatterChart(writer, chart, chartSerieImpl);
				break;
			case OfficeChartType.Area:
			case OfficeChartType.Area_Stacked:
			case OfficeChartType.Area_Stacked_100:
				result = SerializeAreaChart(writer, chart, chartSerieImpl);
				break;
			case OfficeChartType.Area_3D:
			case OfficeChartType.Area_Stacked_3D:
			case OfficeChartType.Area_Stacked_100_3D:
				result = SerializeArea3DChart(writer, chart, chartSerieImpl);
				break;
			case OfficeChartType.Doughnut:
			case OfficeChartType.Doughnut_Exploded:
				result = SerializeDoughnutChart(writer, chart, chartSerieImpl);
				break;
			case OfficeChartType.Radar:
			case OfficeChartType.Radar_Markers:
			case OfficeChartType.Radar_Filled:
				result = SerializeRadarChart(writer, chart, chartSerieImpl);
				break;
			case OfficeChartType.Surface_3D:
			case OfficeChartType.Surface_NoColor_3D:
				result = SerializeSurface3DChart(writer, chart, chartSerieImpl);
				break;
			case OfficeChartType.Surface_Contour:
			case OfficeChartType.Surface_NoColor_Contour:
				result = SerializeSurfaceChart(writer, chart, chartSerieImpl);
				break;
			case OfficeChartType.Bubble:
			case OfficeChartType.Bubble_3D:
				result = SerializeBubbleChart(writer, chart, chartSerieImpl);
				break;
			case OfficeChartType.Stock_HighLowClose:
			case OfficeChartType.Stock_OpenHighLowClose:
			case OfficeChartType.Stock_VolumeHighLowClose:
			case OfficeChartType.Stock_VolumeOpenHighLowClose:
				result = SerializeStockChart(writer, chart, chartSerieImpl);
				break;
			}
		}
		return result;
	}

	private void SerializeRadarChart(XmlWriter writer, ChartImpl chart)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("radarChart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		ChartSerializatorCommon.SerializeValueTag(writer, "radarStyle", ((Excel2007RadarStyle)chart.PivotChartType).ToString());
	}

	[SecurityCritical]
	private int SerializeRadarChart(XmlWriter writer, ChartImpl chart, ChartSerieImpl firstSeries)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("radarChart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		if (chart.RadarStyle != null)
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "radarStyle", chart.RadarStyle);
		}
		else
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "radarStyle", ((Excel2007RadarStyle)firstSeries.SerieType).ToString());
			SerializeVaryColors(writer, firstSeries);
		}
		int result = SerializeChartSeries(writer, chart, firstSeries, SerializeRadarSeries);
		if (chart.CommonDataPointsCollection != null && chart.CommonDataPointsCollection.ContainsKey(firstSeries.ChartGroup))
		{
			SerializeDataLabels(writer, chart, chart.CommonDataPointsCollection[firstSeries.ChartGroup]);
		}
		SerializeBarAxisId(writer, chart, firstSeries);
		writer.WriteEndElement();
		return result;
	}

	[SecurityCritical]
	private int SerializeScatterChart(XmlWriter writer, ChartImpl chart, ChartSerieImpl firstSeries)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("scatterChart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		ChartSerializatorCommon.SerializeValueTag(writer, "scatterStyle", ((Excel2007ScatterStyle)firstSeries.SerieType).ToString());
		SerializeVaryColors(writer, firstSeries);
		int result = SerializeChartSeries(writer, chart, firstSeries, SerializeScatterSeries);
		if (chart.CommonDataPointsCollection != null && chart.CommonDataPointsCollection.ContainsKey(firstSeries.ChartGroup))
		{
			SerializeDataLabels(writer, chart, chart.CommonDataPointsCollection[firstSeries.ChartGroup]);
		}
		SerializeUpDownBars(writer, chart, firstSeries);
		SerializeBarAxisId(writer, chart, firstSeries);
		writer.WriteEndElement();
		return result;
	}

	private void SerializePieChart(XmlWriter writer, ChartImpl chart)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("pieChart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		ChartSerializatorCommon.SerializeBoolValueTag(writer, "varyColors", value: true);
	}

	[SecurityCritical]
	private int SerializePieChart(XmlWriter writer, ChartImpl chart, ChartSerieImpl firstSeries)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("pieChart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		int result = SerializePieCommon(writer, chart, firstSeries);
		ChartSerializatorCommon.SerializeValueTag(writer, "firstSliceAng", firstSeries.SerieFormat.CommonSerieOptions.FirstSliceAngle.ToString());
		writer.WriteEndElement();
		return result;
	}

	private void SerializePie3DChart(XmlWriter writer, ChartImpl chart)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("pie3DChart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		ChartSerializatorCommon.SerializeBoolValueTag(writer, "varyColors", value: true);
	}

	[SecurityCritical]
	private int SerializePie3DChart(XmlWriter writer, ChartImpl chart, ChartSerieImpl firstSeries)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("pie3DChart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		int result = SerializePieCommon(writer, chart, firstSeries);
		writer.WriteEndElement();
		return result;
	}

	private void SerializeOfPieChart(XmlWriter writer, ChartImpl chart)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("ofPieChart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		string value = ((chart.PivotChartType == OfficeChartType.PieOfPie) ? "pie" : "bar");
		ChartSerializatorCommon.SerializeValueTag(writer, "ofPieType", value);
		ChartSerializatorCommon.SerializeBoolValueTag(writer, "varyColors", value: true);
	}

	[SecurityCritical]
	private int SerializeOfPieChart(XmlWriter writer, ChartImpl chart, ChartSerieImpl firstSeries)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("ofPieChart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		string value = ((firstSeries.SerieType == OfficeChartType.PieOfPie) ? "pie" : "bar");
		ChartSerializatorCommon.SerializeValueTag(writer, "ofPieType", value);
		int result = SerializePieCommon(writer, chart, firstSeries);
		IWorkbook workbook = chart.Workbook;
		IOfficeChartFormat commonSerieOptions = firstSeries.SerieFormat.CommonSerieOptions;
		ChartSerializatorCommon.SerializeValueTag(writer, "gapWidth", commonSerieOptions.GapWidth.ToString());
		int splitValue = commonSerieOptions.SplitValue;
		IOfficeChartBorder pieSeriesLine = commonSerieOptions.PieSeriesLine;
		if (splitValue != 0)
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "splitType", ((Excel2007SplitType)commonSerieOptions.SplitType).ToString());
			ChartSerializatorCommon.SerializeValueTag(writer, "splitPos", splitValue.ToString());
		}
		ChartSerializatorCommon.SerializeValueTag(writer, "secondPieSize", commonSerieOptions.PieSecondSize.ToString());
		writer.WriteStartElement("serLines", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		writer.WriteStartElement("spPr", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		ChartSerializatorCommon.SerializeLineProperties(writer, pieSeriesLine, workbook);
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
		return result;
	}

	[SecurityCritical]
	private int SerializeStockChart(XmlWriter writer, ChartImpl chart, ChartSerieImpl firstSeries)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("stockChart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		IOfficeChartSeries series = chart.Series;
		int i = 0;
		for (int count = series.Count; i < count; i++)
		{
			ChartSerieImpl series2 = (ChartSerieImpl)series[i];
			SerializeLineSeries(writer, series2);
		}
		if (chart.CommonDataPointsCollection != null && chart.CommonDataPointsCollection.ContainsKey(firstSeries.ChartGroup))
		{
			SerializeDataLabels(writer, chart, chart.CommonDataPointsCollection[firstSeries.ChartGroup]);
		}
		ChartFormatImpl chartFormatImpl = chart.PrimaryFormats[0];
		if (chartFormatImpl.IsChartChartLine)
		{
			string localName = "";
			IOfficeChartBorder border = null;
			if (chartFormatImpl.HasHighLowLines)
			{
				localName = "hiLowLines";
				border = chartFormatImpl.HighLowLines;
			}
			else if (chartFormatImpl.HasDropLines)
			{
				localName = "dropLines";
				border = chartFormatImpl.DropLines;
			}
			if (chartFormatImpl.IsChartLineFormat)
			{
				writer.WriteStartElement(localName, "http://schemas.openxmlformats.org/drawingml/2006/chart");
				writer.WriteStartElement("spPr", "http://schemas.openxmlformats.org/drawingml/2006/chart");
				ChartSerializatorCommon.SerializeLineProperties(writer, border, chart.Workbook);
				writer.WriteEndElement();
				writer.WriteEndElement();
			}
			else
			{
				writer.WriteElementString(localName, "http://schemas.openxmlformats.org/drawingml/2006/chart", string.Empty);
			}
		}
		SerializeUpDownBars(writer, chart, firstSeries);
		SerializeBarAxisId(writer, chart, firstSeries);
		writer.WriteEndElement();
		return series.Count;
	}

	private void SerializeHiLowLineProperties(XmlWriter writer, ChartFormatImpl format, ChartImpl chart)
	{
		if (format.IsChartLineFormat)
		{
			writer.WriteStartElement("hiLowLines", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			writer.WriteStartElement("spPr", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			ChartSerializatorCommon.SerializeLineProperties(writer, format.HighLowLineProperties, chart.Workbook);
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
		else
		{
			writer.WriteElementString("hiLowLines", "http://schemas.openxmlformats.org/drawingml/2006/chart", string.Empty);
		}
	}

	private void SerializeDoughnutChart(XmlWriter writer, ChartImpl chart)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("doughnutChart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		ChartSerializatorCommon.SerializeBoolValueTag(writer, "varyColors", value: true);
		ChartSerializatorCommon.SerializeValueTag(writer, "holeSize", "50");
	}

	[SecurityCritical]
	private int SerializeDoughnutChart(XmlWriter writer, ChartImpl chart, ChartSerieImpl firstSeries)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("doughnutChart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		int result = SerializePieCommon(writer, chart, firstSeries);
		IOfficeChartFormat commonSerieOptions = firstSeries.SerieFormat.CommonSerieOptions;
		ChartSerializatorCommon.SerializeValueTag(writer, "firstSliceAng", commonSerieOptions.FirstSliceAngle.ToString());
		ChartSerializatorCommon.SerializeValueTag(writer, "holeSize", commonSerieOptions.DoughnutHoleSize.ToString());
		writer.WriteEndElement();
		return result;
	}

	[SecurityCritical]
	private int SerializePieCommon(XmlWriter writer, ChartImpl chart, ChartSerieImpl firstSeries)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		SerializeVaryColors(writer, firstSeries);
		int result = SerializeChartSeries(writer, chart, firstSeries, SerializePieSeries);
		if (chart.CommonDataPointsCollection != null && chart.CommonDataPointsCollection.ContainsKey(firstSeries.ChartGroup))
		{
			SerializeDataLabels(writer, chart, chart.CommonDataPointsCollection[firstSeries.ChartGroup]);
		}
		return result;
	}

	[SecurityCritical]
	private void SerializeDataLabels(XmlWriter writer, ChartImpl parentChart, ChartDataPointsCollection chartDataPointsCollection)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		writer.WriteStartElement("dLbls", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		if (chartDataPointsCollection.DeninedDPCount > 0)
		{
			foreach (ChartDataPointImpl item in chartDataPointsCollection)
			{
				if (!item.IsDefault && item.HasDataLabels)
				{
					SerializeDataLabel(writer, item.DataLabels, item.Index, parentChart, isCommonDataLabels: false, chartDataPointsCollection.DefaultDataPoint.DataLabels);
				}
			}
		}
		ChartDataLabelsImpl chartDataLabelsImpl = chartDataPointsCollection.DefaultDataPoint.DataLabels as ChartDataLabelsImpl;
		if (chartDataLabelsImpl.NumberFormat != null)
		{
			SerializeNumFormat(writer, chartDataLabelsImpl);
		}
		SerializeDataLabelSettings(writer, chartDataPointsCollection.DefaultDataPoint.DataLabels, parentChart, SerializeLeaderLines: true, isCommonDataLabels: false, chartDataPointsCollection.DefaultDataPoint.DataLabels);
		writer.WriteEndElement();
	}

	[SecurityCritical]
	private void SerializeDataLabels(XmlWriter writer, IOfficeChartDataLabels dataLabels, ChartSerieImpl series)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (dataLabels == null)
		{
			throw new ArgumentNullException("dataLabels");
		}
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		writer.WriteStartElement("dLbls", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		ChartImpl parentChart = series.ParentChart;
		ChartDataPointsCollection chartDataPointsCollection = (ChartDataPointsCollection)series.DataPoints;
		if (chartDataPointsCollection.DeninedDPCount > 0)
		{
			foreach (ChartDataPointImpl item in chartDataPointsCollection)
			{
				if (!item.IsDefault && item.HasDataLabels)
				{
					SerializeDataLabel(writer, item.DataLabels, item.Index, parentChart, isCommonDataLabels: true, dataLabels);
				}
			}
		}
		ChartDataLabelsImpl chartDataLabelsImpl = series.DataPoints.DefaultDataPoint.DataLabels as ChartDataLabelsImpl;
		if (chartDataLabelsImpl.NumberFormat != null)
		{
			SerializeNumFormat(writer, chartDataLabelsImpl);
		}
		SerializeDataLabelSettings(writer, dataLabels, parentChart, SerializeLeaderLines: true, isCommonDataLabels: false, dataLabels);
		writer.WriteEndElement();
	}

	private void SerializeNumFormat(XmlWriter writer, ChartDataLabelsImpl dataLabels)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (dataLabels == null)
		{
			throw new ArgumentNullException("dataLabels");
		}
		writer.WriteStartElement("numFmt", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		writer.WriteAttributeString("formatCode", dataLabels.NumberFormat);
		writer.WriteAttributeString("sourceLinked", Convert.ToInt16(dataLabels.IsSourceLinked).ToString());
		writer.WriteEndElement();
	}

	[SecurityCritical]
	private void SerializeDataLabel(XmlWriter writer, IOfficeChartDataLabels dataLabels, int index, ChartImpl chart, bool isCommonDataLabels, IOfficeChartDataLabels commonDataLabels)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (dataLabels == null)
		{
			throw new ArgumentNullException("dataLabels");
		}
		writer.WriteStartElement("dLbl", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		ChartSerializatorCommon.SerializeValueTag(writer, "idx", index.ToString());
		if (dataLabels is ChartDataLabelsImpl)
		{
			ChartDataLabelsImpl chartDataLabelsImpl = dataLabels as ChartDataLabelsImpl;
			if (chartDataLabelsImpl.NumberFormat != null)
			{
				SerializeNumFormat(writer, chartDataLabelsImpl);
			}
		}
		IOfficeChartLayout officeChartLayout = null;
		if (dataLabels is ChartDataLabelsImpl)
		{
			officeChartLayout = (dataLabels as ChartDataLabelsImpl).Layout;
			if (officeChartLayout != null)
			{
				IOfficeChartManualLayout manualLayout = officeChartLayout.ManualLayout;
				if (manualLayout != null && (manualLayout.LayoutTarget != 0 || manualLayout.LeftMode != 0 || manualLayout.TopMode != 0 || manualLayout.Left != 0.0 || manualLayout.Top != 0.0 || manualLayout.WidthMode != 0 || manualLayout.HeightMode != 0 || manualLayout.Width != 0.0 || manualLayout.Height != 0.0))
				{
					ChartSerializatorCommon.SerializeLayout(writer, dataLabels as ChartDataLabelsImpl);
				}
			}
		}
		IInternalOfficeChartTextArea internalOfficeChartTextArea = dataLabels as IInternalOfficeChartTextArea;
		if (!string.IsNullOrEmpty(internalOfficeChartTextArea.Text))
		{
			WorkbookImpl parentWorkbook = chart.ParentWorkbook;
			if (internalOfficeChartTextArea is ChartTextAreaImpl)
			{
				officeChartLayout = (internalOfficeChartTextArea as ChartTextAreaImpl).Layout;
				if (officeChartLayout != null)
				{
					ChartSerializatorCommon.SerializeLayout(writer, internalOfficeChartTextArea as ChartTextAreaImpl);
				}
			}
			ChartSerializatorCommon.SerializeTextAreaText(writer, internalOfficeChartTextArea, parentWorkbook, 10.0);
		}
		SerializeDataLabelSettings(writer, dataLabels, chart, SerializeLeaderLines: true, isCommonDataLabels, commonDataLabels);
		writer.WriteEndElement();
	}

	[SecurityCritical]
	private void SerializeDataLabelSettings(XmlWriter writer, IOfficeChartDataLabels dataLabels, ChartImpl chart, bool SerializeLeaderLines, bool isCommonDataLabels, IOfficeChartDataLabels commonDataLabels)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (dataLabels == null)
		{
			throw new ArgumentNullException("dataLabels");
		}
		ChartDataLabelsImpl chartDataLabelsImpl = dataLabels as ChartDataLabelsImpl;
		ChartDataPointImpl chartDataPointImpl = dataLabels.Parent as ChartDataPointImpl;
		if (chartDataLabelsImpl.ShowTextProperties)
		{
			ChartSerializatorCommon.SerializeFrameFormat(writer, dataLabels.FrameFormat, chart, isRoundCorners: false);
			if (((ChartDataLabelsImpl)dataLabels).ParagraphType == ChartParagraphType.CustomDefault)
			{
				if (isCommonDataLabels && !chartDataLabelsImpl.ShowSizeProperties)
				{
					dataLabels.Size = commonDataLabels.Size;
					SerializeDefaultTextFormatting(writer, dataLabels, chart.Workbook, dataLabels.Size, isCommonDataLabels);
				}
				else
				{
					SerializeDefaultTextFormatting(writer, dataLabels, chart.Workbook, 10.0, isCommonFontSize: false);
				}
			}
		}
		else if ((dataLabels as ChartDataLabelsImpl).IsDelete)
		{
			ChartSerializatorCommon.SerializeBoolValueTag(writer, "delete", value: true);
		}
		OfficeDataLabelPosition position = dataLabels.Position;
		if (position != 0 && position != OfficeDataLabelPosition.Moved)
		{
			Excel2007DataLabelPos excel2007DataLabelPos = (Excel2007DataLabelPos)position;
			ChartSerializatorCommon.SerializeValueTag(writer, "dLblPos", excel2007DataLabelPos.ToString());
		}
		if (!chartDataLabelsImpl.IsDelete || chartDataPointImpl.IsDefault)
		{
			if (chart.Workbook.Version == OfficeVersion.Excel2007)
			{
				if ((dataLabels as ChartDataLabelsImpl).m_bHasLegendKeyOption || dataLabels.IsLegendKey || chart.DestinationType == OfficeChartType.Scatter_Line_Markers)
				{
					ChartSerializatorCommon.SerializeBoolValueTag(writer, "showLegendKey", dataLabels.IsLegendKey);
				}
				if ((dataLabels as ChartDataLabelsImpl).m_bHasValueOption || dataLabels.IsValue || chart.DestinationType == OfficeChartType.Scatter_Line_Markers)
				{
					ChartSerializatorCommon.SerializeBoolValueTag(writer, "showVal", dataLabels.IsValue);
				}
				if ((dataLabels as ChartDataLabelsImpl).m_bHasCategoryOption || dataLabels.IsCategoryName || chart.DestinationType == OfficeChartType.Scatter_Line_Markers)
				{
					ChartSerializatorCommon.SerializeBoolValueTag(writer, "showCatName", dataLabels.IsCategoryName);
				}
				if ((dataLabels as ChartDataLabelsImpl).m_bHasSeriesOption || dataLabels.IsSeriesName || chart.DestinationType == OfficeChartType.Scatter_Line_Markers)
				{
					ChartSerializatorCommon.SerializeBoolValueTag(writer, "showSerName", dataLabels.IsSeriesName);
				}
				if ((dataLabels as ChartDataLabelsImpl).m_bHasPercentageOption || dataLabels.IsPercentage || chart.DestinationType == OfficeChartType.Scatter_Line_Markers)
				{
					ChartSerializatorCommon.SerializeBoolValueTag(writer, "showPercent", dataLabels.IsPercentage);
				}
				if ((dataLabels as ChartDataLabelsImpl).m_bHasBubbleSizeOption || dataLabels.IsBubbleSize || chart.DestinationType == OfficeChartType.Scatter_Line_Markers)
				{
					ChartSerializatorCommon.SerializeBoolValueTag(writer, "showBubbleSize", dataLabels.IsBubbleSize);
				}
			}
			else
			{
				ChartDataLabelsImpl chartDataLabelsImpl2 = dataLabels as ChartDataLabelsImpl;
				if (chartDataLabelsImpl2.m_bHasLegendKeyOption || chartDataLabelsImpl2.m_bHasValueOption || chartDataLabelsImpl2.m_bHasCategoryOption || chartDataLabelsImpl2.m_bHasSeriesOption || chartDataLabelsImpl2.m_bHasPercentageOption || chartDataLabelsImpl2.m_bHasBubbleSizeOption || chart.DestinationType == OfficeChartType.Scatter_Line_Markers)
				{
					ChartSerializatorCommon.SerializeBoolValueTag(writer, "showLegendKey", dataLabels.IsLegendKey);
					ChartSerializatorCommon.SerializeBoolValueTag(writer, "showVal", dataLabels.IsValue);
					ChartSerializatorCommon.SerializeBoolValueTag(writer, "showCatName", dataLabels.IsCategoryName);
					ChartSerializatorCommon.SerializeBoolValueTag(writer, "showSerName", dataLabels.IsSeriesName);
					ChartSerializatorCommon.SerializeBoolValueTag(writer, "showPercent", dataLabels.IsPercentage);
					ChartSerializatorCommon.SerializeBoolValueTag(writer, "showBubbleSize", dataLabels.IsBubbleSize);
				}
			}
		}
		string delimiter = dataLabels.Delimiter;
		if (delimiter != null)
		{
			writer.WriteElementString("separator", "http://schemas.openxmlformats.org/drawingml/2006/chart", delimiter);
		}
		if (chartDataPointImpl.IsDefault && (SerializeLeaderLines || chartDataLabelsImpl.CheckSerieIsPie || chart.IsChartPie))
		{
			ChartSerieImpl chartSerieImpl = null;
			if ((chartDataPointImpl.Parent as ChartDataPointsCollection).Parent is ChartSerieImpl)
			{
				chartSerieImpl = (chartDataPointImpl.IsDefault ? ((chartDataPointImpl.Parent as ChartDataPointsCollection).Parent as ChartSerieImpl) : null);
			}
			ChartSerializatorCommon.SerializeBoolValueTag(writer, "showLeaderLines", dataLabels.ShowLeaderLines);
			if (chartSerieImpl != null)
			{
				writer.WriteStartElement("c", "extLst", null);
				writer.WriteStartElement("c", "ext", null);
				writer.WriteAttributeString("uri", "{CE6537A1-D6FC-4f65-9D91-7224C49458BB}");
				writer.WriteAttributeString("xmlns", "c15", null, "http://schemas.microsoft.com/office/drawing/2012/chart");
				if (chartDataLabelsImpl.IsValueFromCells)
				{
					writer.WriteStartElement("c15", "showDataLabelsRange", null);
					writer.WriteAttributeString("val", "1");
					writer.WriteEndElement();
				}
				writer.WriteStartElement("c15", "showLeaderLines", null);
				writer.WriteAttributeString("val", (!chartSerieImpl.HasLeaderLines) ? "0" : "1");
				writer.WriteEndElement();
				if (chartSerieImpl.HasLeaderLines)
				{
					writer.WriteStartElement("c15", "leaderLines", null);
					writer.WriteStartElement("spPr", "http://schemas.openxmlformats.org/drawingml/2006/chart");
					FileDataHolder parentHolder = chart.DataHolder.ParentHolder;
					ChartSerializatorCommon.SerializeLineProperties(writer, chartSerieImpl.LeaderLines, parentHolder.Workbook);
					writer.WriteEndElement();
					writer.WriteEndElement();
				}
				writer.WriteEndElement();
				writer.WriteEndElement();
			}
		}
		else if (chartDataLabelsImpl.IsValueFromCells)
		{
			writer.WriteStartElement("c", "extLst", null);
			writer.WriteStartElement("c", "ext", null);
			writer.WriteAttributeString("uri", "{CE6537A1-D6FC-4f65-9D91-7224C49458BB}");
			writer.WriteAttributeString("xmlns", "c15", null, "http://schemas.microsoft.com/office/drawing/2012/chart");
			writer.WriteStartElement("c15", "showDataLabelsRange", null);
			writer.WriteAttributeString("val", "1");
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
	}

	private void SerializeDefaultTextFormatting(XmlWriter writer, IOfficeChartTextArea textFormatting, IWorkbook book, double defaultFontSize, bool isCommonFontSize)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (book == null)
		{
			throw new ArgumentNullException("book");
		}
		if (textFormatting != null)
		{
			writer.WriteStartElement("txPr", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			writer.WriteStartElement("bodyPr", "http://schemas.openxmlformats.org/drawingml/2006/main");
			if (textFormatting.TextRotationAngle != 0)
			{
				writer.WriteAttributeString("rot", (textFormatting.TextRotationAngle * 60000).ToString());
			}
			writer.WriteEndElement();
			writer.WriteStartElement("lstStyle", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteEndElement();
			writer.WriteStartElement("p", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteStartElement("pPr", "http://schemas.openxmlformats.org/drawingml/2006/main");
			ChartSerializatorCommon.SerializeParagraphRunProperites(writer, textFormatting, "defRPr", book, defaultFontSize, isCommonFontSize);
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
	}

	private void SerializeVaryColors(XmlWriter writer, ChartSerieImpl firstSeries)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (firstSeries == null)
		{
			throw new ArgumentNullException("firstSeries");
		}
		bool isVaryColor = firstSeries.SerieFormat.CommonSerieOptions.IsVaryColor;
		ChartSerializatorCommon.SerializeBoolValueTag(writer, "varyColors", isVaryColor);
	}

	[SecurityCritical]
	private void SerializeBarSeries(XmlWriter writer, ChartSerieImpl series)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		SerializeSeriesCommonWithoutEnd(writer, series, series.IsFiltered);
		if ((series.DataPoints.DefaultDataPoint as ChartDataPointImpl).HasDataLabels || (series.DataPoints as ChartDataPointsCollection).CheckDPDataLabels())
		{
			SerializeDataLabels(writer, series.DataPoints.DefaultDataPoint.DataLabels, series);
		}
		SerializeTrendlines(writer, series.TrendLines, series.ParentBook);
		SerializeErrorBars(writer, series);
		if (categoryFilter == 0)
		{
			findFilter = FindFiltered(series);
			categoryFilter++;
			if (findFilter)
			{
				UpdateCategoryLabel(series);
				UpdateFilteredValuesRange(series);
			}
		}
		if (!findFilter)
		{
			if (series.ParentChart.CategoryLabelLevel == OfficeCategoriesLabelLevel.CategoriesLabelLevelAll)
			{
				SerializeSeriesCategory(writer, series);
			}
			SerializeSeriesValues(writer, series);
		}
		else
		{
			if (series.ParentChart.CategoryLabelLevel == OfficeCategoriesLabelLevel.CategoriesLabelLevelAll)
			{
				SerializeFilteredCategory(writer, series, findFilter);
			}
			SerializeFilteredValues(writer, series, findFilter);
		}
		if ((series.ParentChart.Loading && series.HasColumnShape) || series.SerieFormat != null)
		{
			SerializeBarShape(writer, series.ParentChart, series, isSerieCalled: true);
		}
		if (series.ParentChart.CategoryLabelLevel != 0 || series.ParentChart.SeriesNameLevel != 0 || series.GetInvertIfNegative() == true || ((series.DataPoints.DefaultDataPoint as ChartDataPointImpl).HasDataLabels && series.DataPoints.DefaultDataPoint.DataLabels.IsValueFromCells))
		{
			writer.WriteStartElement("extLst", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			if (series.ParentChart.CategoryLabelLevel != 0 || series.ParentChart.SeriesNameLevel != 0)
			{
				if (series.ParentChart.SeriesNameLevel != 0)
				{
					SerializeFilteredSeriesOrCategoryName(writer, series, seriesOrcategory: true);
				}
				if (series.ParentChart.CategoryLabelLevel != 0)
				{
					SerializeFilteredSeriesOrCategoryName(writer, series, seriesOrcategory: false);
				}
			}
			else if (series.GetInvertIfNegative() == true)
			{
				_ = series.m_invertFillFormatStream;
				if (series.GetInvertIfNegative() == true && series.SerieFormat.Fill.FillType == OfficeFillType.SolidColor)
				{
					writer.WriteStartElement("c", "ext", "http://schemas.openxmlformats.org/drawingml/2006/chart");
					writer.WriteAttributeString("uri", "{6F2FDCE9-48DA-4B69-8628-5D25D57E5C99}");
					writer.WriteAttributeString("xmlns", "c14", null, "http://schemas.microsoft.com/office/drawing/2007/8/2/chart");
					SerializeInvertIfNegativeColor(writer, series);
					writer.WriteEndElement();
				}
			}
			if (series.DataPoints.DefaultDataPoint.DataLabels.IsValueFromCells)
			{
				SeriealizeValuesFromCellsRange(writer, series);
			}
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	private void SerializeInvertIfNegativeColor(XmlWriter writer, ChartSerieImpl series)
	{
		writer.WriteStartElement("c14", "invertSolidFillFmt", null);
		writer.WriteStartElement("c14", "spPr", null);
		writer.WriteAttributeString("xmlns", "c14", null, "http://schemas.microsoft.com/office/drawing/2007/8/2/chart");
		ChartSerializatorCommon.SerializeSolidFill(writer, series.InvertIfNegativeColor, isAutoColor: false, series.ParentBook, 1.0 - series.SerieFormat.Fill.Transparency);
		if (series.SerieFormat.HasLineProperties)
		{
			ChartSerializatorCommon.SerializeLineProperties(writer, series.SerieFormat.LineProperties, series.ParentBook);
		}
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	private void SeriealizeValuesFromCellsRange(XmlWriter writer, ChartSerieImpl series)
	{
		IRange valueFromCellsIRange = (series.DataPoints.DefaultDataPoint.DataLabels as ChartDataLabelsImpl).ValueFromCellsIRange;
		if (series.DataPoints.DefaultDataPoint.DataLabels.IsValueFromCells && valueFromCellsIRange != null)
		{
			writer.WriteStartElement("c", "ext", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			writer.WriteAttributeString("uri", "{02D57815-91ED-43cb-92C2-25804820EDAC}");
			writer.WriteAttributeString("xmlns", "c15", null, "http://schemas.microsoft.com/office/drawing/2012/chart");
			writer.WriteStartElement("c15", "datalabelsRange", null);
			writer.WriteStartElement("c15", "f", null);
			writer.WriteString(valueFromCellsIRange.AddressGlobal);
			writer.WriteEndElement();
			serializeDataLabelRangeCache(writer, series.DataLabelCellsValues);
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
	}

	private void serializeDataLabelRangeCache(XmlWriter writer, Dictionary<int, object> values)
	{
		if (values.Count <= 0)
		{
			return;
		}
		writer.WriteStartElement("c15", "dlblRangeCache", null);
		int count = values.Count;
		ChartSerializatorCommon.SerializeValueTag(writer, "ptCount", count.ToString());
		if (count > 0)
		{
			foreach (KeyValuePair<int, object> value in values)
			{
				if (value.Value != null)
				{
					writer.WriteStartElement("pt", "http://schemas.openxmlformats.org/drawingml/2006/chart");
					writer.WriteAttributeString("idx", value.Key.ToString());
					writer.WriteStartElement("v", "http://schemas.openxmlformats.org/drawingml/2006/chart");
					writer.WriteString(ToXmlString(value.Value));
					writer.WriteEndElement();
					writer.WriteEndElement();
				}
			}
		}
		writer.WriteEndElement();
	}

	private bool FindFiltered(ChartSerieImpl series)
	{
		IOfficeChartCategories categories = series.ParentChart.Categories;
		for (int i = 0; i < categories.Count; i++)
		{
			if (categories[i].IsFiltered)
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateFilteredValuesRange(ChartSerieImpl series)
	{
		ChartImpl parentChart = series.ParentChart;
		if (!(parentChart.Categories[0] as ChartCategory).Filter_customize)
		{
			return;
		}
		WorksheetBaseImpl obj = (parentChart.DataRange as ChartDataRange).Range.Worksheet as WorksheetBaseImpl;
		IOfficeChartSeries series2 = parentChart.Series;
		IWorksheet worksheet = obj as IWorksheet;
		IOfficeChartCategories officeChartCategories = parentChart.Categories as ChartCategoryCollection;
		_ = new string[series.ValuesIRange.Count * officeChartCategories.Count];
		int num = 0;
		int num2 = 0;
		string text = string.Empty;
		int column = 0;
		int num3 = 0;
		int num4 = 0;
		IRange range = null;
		for (int i = 0; i < (officeChartCategories[0].Values as ChartDataRange).Range.Count; i++)
		{
			for (int j = 0; j < officeChartCategories.Count; j++)
			{
				if (!officeChartCategories[j].IsFiltered && num == 0)
				{
					num = (officeChartCategories[j].Values as ChartDataRange).Range.Cells[i].Row;
					column = (officeChartCategories[j].Values as ChartDataRange).Range.Cells[i].Column;
				}
				else if (num != 0 && officeChartCategories[j].IsFiltered)
				{
					num3 = (officeChartCategories[j].Values as ChartDataRange).Range.Cells[i].Column;
					num2 = (officeChartCategories[j].Values as ChartDataRange).Range.Cells[i].Row;
				}
				if (num != 0 && num2 != 0)
				{
					range = ((num != num2) ? worksheet.Range[num, column, num2 - 1, num3] : worksheet.Range[num, column, num2, num3 - 1]);
					text = ((text == string.Empty) ? ("(" + range.AddressGlobal) : (text + "," + range.AddressGlobal));
					num4++;
					num = 0;
					num2 = 0;
				}
			}
			if (num != 0)
			{
				num3 = (officeChartCategories[officeChartCategories.Count - 1].Values as ChartDataRange).Range.Cells[i].Column;
				num2 = (officeChartCategories[officeChartCategories.Count - 1].Values as ChartDataRange).Range.Cells[i].Row;
				if (num == num2)
				{
					range = worksheet.Range[num, column, num2, num3];
					num = 0;
					num2 = 0;
				}
				else
				{
					range = worksheet.Range[num, column, num2, num3];
					num = 0;
					num2 = 0;
				}
				text = ((text == string.Empty) ? ("(" + range.AddressGlobal) : (text + "," + range.AddressGlobal));
				num4++;
			}
			text += ")";
			(series2[i] as ChartSerieImpl).FilteredValue = text.Replace("'", "");
			text = string.Empty;
		}
	}

	private void UpdateCategoryLabel(ChartSerieImpl series)
	{
		ChartImpl parentChart = series.ParentChart;
		if (!(parentChart.Categories[0] as ChartCategory).Filter_customize || series.CategoryLabelsIRange == null)
		{
			return;
		}
		WorksheetBaseImpl obj = (parentChart.DataRange as ChartDataRange).Range.Worksheet as WorksheetBaseImpl;
		IOfficeChartSeries series2 = parentChart.Series;
		IWorksheet worksheet = obj as IWorksheet;
		IOfficeChartCategories officeChartCategories = parentChart.Categories as ChartCategoryCollection;
		_ = new string[series.ValuesIRange.Count * officeChartCategories.Count];
		int num = 0;
		int num2 = 0;
		string text = string.Empty;
		int column = 0;
		int num3 = 0;
		int num4 = 0;
		IRange range = null;
		for (int i = 0; i < officeChartCategories.Count; i++)
		{
			if (!officeChartCategories[i].IsFiltered && num == 0)
			{
				num = (officeChartCategories[i].CategoryLabel as ChartDataRange).Range.Cells[i].Row;
				column = (officeChartCategories[i].CategoryLabel as ChartDataRange).Range.Cells[i].Column;
			}
			else if (num != 0 && officeChartCategories[i].IsFiltered)
			{
				num3 = (officeChartCategories[i].CategoryLabel as ChartDataRange).Range.Cells[i].Column;
				num2 = (officeChartCategories[i].CategoryLabel as ChartDataRange).Range.Cells[i].Row;
			}
			if (num != 0 && num2 != 0)
			{
				range = ((num != num2) ? worksheet.Range[num, column, num2 - 1, num3] : worksheet.Range[num, column, num2, num3 - 1]);
				text = ((text == string.Empty) ? ("(" + range.AddressGlobal) : (text + "," + range.AddressGlobal));
				num4++;
				num = 0;
				num2 = 0;
			}
		}
		if (num != 0)
		{
			num3 = (officeChartCategories[officeChartCategories.Count - 1].CategoryLabel as ChartDataRange).Range.Cells[officeChartCategories.Count - 1].Column;
			num2 = (officeChartCategories[officeChartCategories.Count - 1].CategoryLabel as ChartDataRange).Range.Cells[officeChartCategories.Count - 1].Row;
			if (num == num2)
			{
				range = worksheet.Range[num, column, num2, num3];
				num = 0;
				num2 = 0;
			}
			else
			{
				range = worksheet.Range[num, column, num2, num3];
				num = 0;
				num2 = 0;
			}
			text = ((text == string.Empty) ? ("(" + range.AddressGlobal) : (text + "," + range.AddressGlobal));
			num4++;
		}
		text += ")";
		text = text.Replace("'", "");
		for (int j = 0; j < series2.Count; j++)
		{
			(series2[j] as ChartSerieImpl).FilteredCategory = text;
		}
	}

	private void SerializeFilteredSeriesOrCategoryName(XmlWriter writer, ChartSerieImpl series, bool seriesOrcategory)
	{
		if (seriesOrcategory && !series.IsDefaultName)
		{
			writer.WriteStartElement("c", "ext", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			writer.WriteAttributeString("uri", "{02D57815-91ED-43cb-92C2-25804820EDAC}");
			writer.WriteAttributeString("xmlns", "c15", null, "http://schemas.microsoft.com/office/drawing/2012/chart");
			writer.WriteStartElement("c15", "filteredSeriesTitle", "http://schemas.microsoft.com/office/drawing/2012/chart");
			SerializeFilteredText(writer, series);
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
		else if (series.CategoryLabelsIRange != null)
		{
			writer.WriteStartElement("c", "ext", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			writer.WriteAttributeString("uri", "{02D57815-91ED-43cb-92C2-25804820EDAC}");
			writer.WriteAttributeString("xmlns", "c15", null, "http://schemas.microsoft.com/office/drawing/2012/chart");
			writer.WriteStartElement("c15", "filteredCategoryTitle", "http://schemas.microsoft.com/office/drawing/2012/chart");
			SerializeFilteredCategoryName(writer, series);
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
	}

	private void SerializeFilteredCategoryName(XmlWriter writer, ChartSerieImpl series)
	{
		if (series.CategoryLabelsIRange == null)
		{
			return;
		}
		writer.WriteStartElement("c15", "cat", "http://schemas.microsoft.com/office/drawing/2012/chart");
		if (series.CategoryLabelsIRange.HasNumber || series.CategoryLabelsIRange.HasDateTime)
		{
			writer.WriteStartElement("c", "numRef", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		}
		else
		{
			writer.WriteStartElement("c", "strRef", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		}
		writer.WriteStartElement("c", "extLst", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		writer.WriteStartElement("c", "ext", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		writer.WriteAttributeString("uri", "{02D57815-91ED-43cb-92C2-25804820EDAC}");
		writer.WriteStartElement("c15", "formulaRef", "http://schemas.microsoft.com/office/drawing/2012/chart");
		writer.WriteElementString("sqref", "http://schemas.microsoft.com/office/drawing/2012/chart", series.CategoryLabelsIRange.AddressGlobal);
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
		if (series.CategoryLabelsIRange.HasString)
		{
			writer.WriteStartElement("c", "strCache", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			SerializeCategoryTagCacheValues(writer, series, Numberformat: false);
		}
		else if (series.CategoryLabelsIRange.HasNumber || series.CategoryLabelsIRange.HasDateTime)
		{
			writer.WriteStartElement("c", "numCache", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			if (series.CategoriesFormatCode != null)
			{
				writer.WriteElementString("formatCode", "http://schemas.openxmlformats.org/drawingml/2006/chart", series.CategoriesFormatCode);
			}
			else
			{
				writer.WriteElementString("formatCode", "http://schemas.openxmlformats.org/drawingml/2006/chart", series.ParentChart.PrimaryCategoryAxis.NumberFormat);
			}
			SerializeCategoryTagCacheValues(writer, series, Numberformat: true);
		}
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	[SecurityCritical]
	private void SerializePieSeries(XmlWriter writer, ChartSerieImpl series)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		SerializeSeriesCommonWithoutEnd(writer, series, isFiltered: false);
		int percent = series.SerieFormat.Percent;
		if (percent != 0 || series.ParentSeries[0].Equals(series))
		{
			if (percent == 0)
			{
				percent = series.GetCommonSerieFormat().SerieDataFormat.Percent;
			}
			ChartSerializatorCommon.SerializeValueTag(writer, "explosion", percent.ToString());
		}
		if ((series.DataPoints.DefaultDataPoint as ChartDataPointImpl).HasDataLabels || (series.DataPoints as ChartDataPointsCollection).CheckDPDataLabels())
		{
			SerializeDataLabels(writer, series.DataPoints.DefaultDataPoint.DataLabels, series);
		}
		SerializeTrendlines(writer, series.TrendLines, series.ParentBook);
		SerializeErrorBars(writer, series);
		if (categoryFilter == 0)
		{
			findFilter = FindFiltered(series);
			categoryFilter++;
			if (findFilter)
			{
				UpdateCategoryLabel(series);
				UpdateFilteredValuesRange(series);
			}
		}
		if (!findFilter)
		{
			if (series.ParentChart.CategoryLabelLevel == OfficeCategoriesLabelLevel.CategoriesLabelLevelAll)
			{
				SerializeSeriesCategory(writer, series);
			}
			SerializeSeriesValues(writer, series);
		}
		else
		{
			if (series.ParentChart.CategoryLabelLevel == OfficeCategoriesLabelLevel.CategoriesLabelLevelAll)
			{
				SerializeFilteredCategory(writer, series, findFilter);
			}
			SerializeFilteredValues(writer, series, findFilter);
		}
		if (series.ParentChart.CategoryLabelLevel != 0 || series.ParentChart.SeriesNameLevel != 0 || series.DataPoints.DefaultDataPoint.DataLabels.IsValueFromCells)
		{
			writer.WriteStartElement("extLst", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			if (series.ParentChart.CategoryLabelLevel != 0 || series.ParentChart.SeriesNameLevel != 0)
			{
				if (series.ParentChart.SeriesNameLevel != 0)
				{
					SerializeFilteredSeriesOrCategoryName(writer, series, seriesOrcategory: true);
				}
				if (series.ParentChart.CategoryLabelLevel != 0)
				{
					SerializeFilteredSeriesOrCategoryName(writer, series, seriesOrcategory: false);
				}
			}
			if (series.DataPoints.DefaultDataPoint.DataLabels.IsValueFromCells)
			{
				SeriealizeValuesFromCellsRange(writer, series);
			}
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	private void SerializeErrorBars(XmlWriter writer, ChartSerieImpl series)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		IWorkbook parentBook = series.ParentBook;
		if (series.HasErrorBarsX)
		{
			SerializeErrorBars(writer, series.ErrorBarsX, "x", parentBook, series);
		}
		if (series.HasErrorBarsY)
		{
			SerializeErrorBars(writer, series.ErrorBarsY, "y", parentBook, series);
		}
	}

	[SecurityCritical]
	private void SerializeLineSeries(XmlWriter writer, ChartSerieImpl series)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		SerializeSeriesCommonWithoutEnd(writer, series, isFiltered: false);
		if ((series.DataPoints.DefaultDataPoint as ChartDataPointImpl).HasDataLabels || (series.DataPoints as ChartDataPointsCollection).CheckDPDataLabels())
		{
			SerializeDataLabels(writer, series.DataPoints.DefaultDataPoint.DataLabels, series);
		}
		SerializeTrendlines(writer, series.TrendLines, series.ParentBook);
		SerializeErrorBars(writer, series);
		if (categoryFilter == 0)
		{
			findFilter = FindFiltered(series);
			categoryFilter++;
			if (findFilter)
			{
				UpdateCategoryLabel(series);
				UpdateFilteredValuesRange(series);
			}
		}
		if (!findFilter)
		{
			if (series.ParentChart.CategoryLabelLevel == OfficeCategoriesLabelLevel.CategoriesLabelLevelAll)
			{
				SerializeSeriesCategory(writer, series);
			}
			SerializeSeriesValues(writer, series);
		}
		else
		{
			if (series.ParentChart.CategoryLabelLevel == OfficeCategoriesLabelLevel.CategoriesLabelLevelAll)
			{
				SerializeFilteredCategory(writer, series, findFilter);
			}
			SerializeFilteredValues(writer, series, findFilter);
		}
		if (series.ParentChart.CategoryLabelLevel != 0 || series.ParentChart.SeriesNameLevel != 0 || ((series.DataPoints.DefaultDataPoint as ChartDataPointImpl).HasDataLabels && series.DataPoints.DefaultDataPoint.DataLabels.IsValueFromCells))
		{
			writer.WriteStartElement("extLst", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			if (series.ParentChart.CategoryLabelLevel != 0 || series.ParentChart.SeriesNameLevel != 0)
			{
				if (series.ParentChart.SeriesNameLevel != 0)
				{
					SerializeFilteredSeriesOrCategoryName(writer, series, seriesOrcategory: true);
				}
				if (series.ParentChart.CategoryLabelLevel != 0)
				{
					SerializeFilteredSeriesOrCategoryName(writer, series, seriesOrcategory: false);
				}
			}
			if (series.DataPoints.DefaultDataPoint.DataLabels.IsValueFromCells)
			{
				SeriealizeValuesFromCellsRange(writer, series);
			}
			writer.WriteEndElement();
		}
		ChartSerieDataFormatImpl chartSerieDataFormatImpl = (ChartSerieDataFormatImpl)series.SerieFormat;
		ChartSerializatorCommon.SerializeBoolValueTag(writer, "smooth", chartSerieDataFormatImpl.IsSmoothed);
		writer.WriteEndElement();
	}

	[SecurityCritical]
	private void SerializeScatterSeries(XmlWriter writer, ChartSerieImpl series)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		SerializeSeriesCommonWithoutEnd(writer, series, isFiltered: false);
		if ((series.DataPoints.DefaultDataPoint as ChartDataPointImpl).HasDataLabels || (series.DataPoints as ChartDataPointsCollection).CheckDPDataLabels())
		{
			SerializeDataLabels(writer, series.DataPoints.DefaultDataPoint.DataLabels, series);
		}
		SerializeTrendlines(writer, series.TrendLines, series.ParentBook);
		SerializeErrorBars(writer, series);
		SerializeSeriesCategory(writer, series, "xVal");
		SerializeSeriesValues(writer, series, "yVal");
		ChartSerieDataFormatImpl chartSerieDataFormatImpl = (ChartSerieDataFormatImpl)series.SerieFormat;
		ChartSerializatorCommon.SerializeBoolValueTag(writer, "smooth", chartSerieDataFormatImpl.IsSmoothed);
		if (series.ParentChart.CategoryLabelLevel != 0 || series.ParentChart.SeriesNameLevel != 0 || series.DataPoints.DefaultDataPoint.DataLabels.IsValueFromCells)
		{
			writer.WriteStartElement("extLst", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			if (series.ParentChart.CategoryLabelLevel != 0 || series.ParentChart.SeriesNameLevel != 0)
			{
				if (series.ParentChart.SeriesNameLevel != 0)
				{
					SerializeFilteredSeriesOrCategoryName(writer, series, seriesOrcategory: true);
				}
				if (series.ParentChart.CategoryLabelLevel != 0)
				{
					SerializeFilteredSeriesOrCategoryName(writer, series, seriesOrcategory: false);
				}
			}
			else if (series.DataPoints.DefaultDataPoint.DataLabels.IsValueFromCells)
			{
				SeriealizeValuesFromCellsRange(writer, series);
			}
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	[SecurityCritical]
	private void SerializeRadarSeries(XmlWriter writer, ChartSerieImpl series)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		SerializeSeriesCommonWithoutEnd(writer, series, isFiltered: false);
		if ((series.DataPoints.DefaultDataPoint as ChartDataPointImpl).HasDataLabels || (series.DataPoints as ChartDataPointsCollection).CheckDPDataLabels())
		{
			SerializeDataLabels(writer, series.DataPoints.DefaultDataPoint.DataLabels, series);
		}
		if (categoryFilter == 0)
		{
			findFilter = FindFiltered(series);
			categoryFilter++;
			if (findFilter)
			{
				UpdateCategoryLabel(series);
				UpdateFilteredValuesRange(series);
			}
		}
		if (!findFilter)
		{
			if (series.ParentChart.CategoryLabelLevel == OfficeCategoriesLabelLevel.CategoriesLabelLevelAll)
			{
				SerializeSeriesCategory(writer, series);
			}
			SerializeSeriesValues(writer, series);
		}
		else
		{
			if (series.ParentChart.CategoryLabelLevel == OfficeCategoriesLabelLevel.CategoriesLabelLevelAll)
			{
				SerializeFilteredCategory(writer, series, findFilter);
			}
			SerializeFilteredValues(writer, series, findFilter);
		}
		if (series.ParentChart.CategoryLabelLevel != 0 || series.ParentChart.SeriesNameLevel != 0 || series.DataPoints.DefaultDataPoint.DataLabels.IsValueFromCells)
		{
			writer.WriteStartElement("extLst", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			if (series.ParentChart.CategoryLabelLevel != 0 || series.ParentChart.SeriesNameLevel != 0)
			{
				if (series.ParentChart.SeriesNameLevel != 0)
				{
					SerializeFilteredSeriesOrCategoryName(writer, series, seriesOrcategory: true);
				}
				if (series.ParentChart.CategoryLabelLevel != 0)
				{
					SerializeFilteredSeriesOrCategoryName(writer, series, seriesOrcategory: false);
				}
			}
			if (series.DataPoints.DefaultDataPoint.DataLabels.IsValueFromCells)
			{
				SeriealizeValuesFromCellsRange(writer, series);
			}
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	[SecurityCritical]
	private void SerializeBubbleSeries(XmlWriter writer, ChartSerieImpl series)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		SerializeSeriesCommonWithoutEnd(writer, series, isFiltered: false);
		if ((series.DataPoints.DefaultDataPoint as ChartDataPointImpl).HasDataLabels || (series.DataPoints as ChartDataPointsCollection).CheckDPDataLabels())
		{
			SerializeDataLabels(writer, series.DataPoints.DefaultDataPoint.DataLabels, series);
		}
		SerializeTrendlines(writer, series.TrendLines, series.ParentBook);
		SerializeErrorBars(writer, series);
		SerializeSeriesCategory(writer, series, "xVal");
		SerializeSeriesValues(writer, series, "yVal");
		SerializeSeriesValues(writer, series.BubblesIRange, series.EnteredDirectlyBubbles, "bubbleSize", series);
		if (series.SerieType == OfficeChartType.Bubble_3D)
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "bubble3D", "1");
		}
		if (series.ParentChart.CategoryLabelLevel != 0 || series.ParentChart.SeriesNameLevel != 0 || series.DataPoints.DefaultDataPoint.DataLabels.IsValueFromCells)
		{
			writer.WriteStartElement("extLst", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			if (series.ParentChart.CategoryLabelLevel != 0 || series.ParentChart.SeriesNameLevel != 0)
			{
				if (series.ParentChart.SeriesNameLevel != 0)
				{
					SerializeFilteredSeriesOrCategoryName(writer, series, seriesOrcategory: true);
				}
				if (series.ParentChart.CategoryLabelLevel != 0)
				{
					SerializeFilteredSeriesOrCategoryName(writer, series, seriesOrcategory: false);
				}
			}
			if (series.DataPoints.DefaultDataPoint.DataLabels.IsValueFromCells)
			{
				SeriealizeValuesFromCellsRange(writer, series);
			}
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	[SecurityCritical]
	private void SerializeAreaSeries(XmlWriter writer, ChartSerieImpl series)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		SerializeSeriesCommonWithoutEnd(writer, series, isFiltered: false);
		if ((series.DataPoints.DefaultDataPoint as ChartDataPointImpl).HasDataLabels || (series.DataPoints as ChartDataPointsCollection).CheckDPDataLabels())
		{
			SerializeDataLabels(writer, series.DataPoints.DefaultDataPoint.DataLabels, series);
		}
		SerializeTrendlines(writer, series.TrendLines, series.ParentBook);
		SerializeErrorBars(writer, series);
		if (categoryFilter == 0)
		{
			findFilter = FindFiltered(series);
			categoryFilter++;
			if (findFilter)
			{
				UpdateCategoryLabel(series);
				UpdateFilteredValuesRange(series);
			}
		}
		if (!findFilter)
		{
			if (series.ParentChart.CategoryLabelLevel == OfficeCategoriesLabelLevel.CategoriesLabelLevelAll)
			{
				SerializeSeriesCategory(writer, series);
			}
			SerializeSeriesValues(writer, series);
		}
		else
		{
			if (series.ParentChart.CategoryLabelLevel == OfficeCategoriesLabelLevel.CategoriesLabelLevelAll)
			{
				SerializeFilteredCategory(writer, series, findFilter);
			}
			SerializeFilteredValues(writer, series, findFilter);
		}
		if (series.ParentChart.CategoryLabelLevel != 0 || series.ParentChart.SeriesNameLevel != 0 || series.DataPoints.DefaultDataPoint.DataLabels.IsValueFromCells)
		{
			writer.WriteStartElement("extLst", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			if (series.ParentChart.CategoryLabelLevel != 0 || series.ParentChart.SeriesNameLevel != 0)
			{
				if (series.ParentChart.SeriesNameLevel != 0)
				{
					SerializeFilteredSeriesOrCategoryName(writer, series, seriesOrcategory: true);
				}
				if (series.ParentChart.CategoryLabelLevel != 0)
				{
					SerializeFilteredSeriesOrCategoryName(writer, series, seriesOrcategory: false);
				}
			}
			if (series.DataPoints.DefaultDataPoint.DataLabels.IsValueFromCells)
			{
				SeriealizeValuesFromCellsRange(writer, series);
			}
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	[SecurityCritical]
	private void SerializeSeriesCommonWithoutEnd(XmlWriter writer, ChartSerieImpl series, bool isFiltered)
	{
		string tagName = null;
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		if (!isFiltered)
		{
			writer.WriteStartElement("ser", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			ChartSerializatorCommon.SerializeValueTag(writer, "idx", series.Number.ToString());
			ChartSerializatorCommon.SerializeValueTag(writer, "order", series.Index.ToString());
			series.CheckLimits();
			if (!series.IsDefaultName && series.ParentChart.SeriesNameLevel == OfficeSeriesNameLevel.SeriesNameLevelAll)
			{
				string nameOrFormula = series.NameOrFormula;
				writer.WriteStartElement("tx", "http://schemas.openxmlformats.org/drawingml/2006/chart");
				if (nameOrFormula != null && nameOrFormula.Length > 0 && nameOrFormula[0] == '=')
				{
					SerializeStringReference(writer, nameOrFormula, series, hasSeriesName: true, tagName);
				}
				else
				{
					writer.WriteStartElement("v", "http://schemas.openxmlformats.org/drawingml/2006/chart");
					writer.WriteString(nameOrFormula);
					writer.WriteEndElement();
				}
				writer.WriteEndElement();
			}
		}
		ChartSerieDataFormatImpl dataFormatOrNull = ((ChartDataPointImpl)series.DataPoints.DefaultDataPoint).DataFormatOrNull;
		if (dataFormatOrNull != null)
		{
			ChartSerializatorCommon.SerializeFrameFormat(writer, dataFormatOrNull, series.ParentChart, isRoundCorners: false, serializeLineAutoValues: true);
		}
		if (series.GetInvertIfNegative().HasValue)
		{
			string value = ((series.GetInvertIfNegative() == true) ? "1" : "0");
			ChartSerializatorCommon.SerializeValueTag(writer, "invertIfNegative", value);
		}
		SerializeMarker(writer, series);
		ChartDataPointsCollection chartDataPointsCollection = (ChartDataPointsCollection)series.DataPoints;
		if (chartDataPointsCollection.DeninedDPCount <= 0)
		{
			return;
		}
		foreach (ChartDataPointImpl item in chartDataPointsCollection)
		{
			if (!item.IsDefault || item.HasDataPoint)
			{
				SerializeDataPoint(writer, item, series);
			}
		}
	}

	[SecurityCritical]
	private void SerializeDataPoint(XmlWriter writer, ChartDataPointImpl dataPoint, ChartSerieImpl series)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (dataPoint == null)
		{
			throw new ArgumentNullException("dataPoint");
		}
		ChartSerieDataFormatImpl dataFormatOrNull = dataPoint.DataFormatOrNull;
		if (dataFormatOrNull != null && (dataFormatOrNull.IsFormatted || dataFormatOrNull.IsParsed))
		{
			string text = series.SerieType.ToString();
			writer.WriteStartElement("dPt", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			ChartSerializatorCommon.SerializeValueTag(writer, "idx", dataPoint.Index.ToString());
			ChartSerializatorCommon.SerializeBoolValueTag(writer, "bubble3D", dataPoint.Bubble3D);
			if (dataPoint.HasExplosion)
			{
				ChartSerializatorCommon.SerializeValueTag(writer, "explosion", dataPoint.Explosion.ToString());
			}
			if (dataFormatOrNull.IsSupportFill && dataFormatOrNull.HasInterior && (text.IndexOf("Column") != -1 || text.IndexOf("Bar") != -1 || text.IndexOf("Pyramid") != -1 || text.IndexOf("Cylinder") != -1 || text.IndexOf("Cone") != -1))
			{
				ChartSerializatorCommon.SerializeBoolValueTag(writer, "invertIfNegative", dataFormatOrNull.Interior.SwapColorsOnNegative);
			}
			else if (dataFormatOrNull.IsSupportFill && series.ParentChart.IsParsed && (text.IndexOf("Column") != -1 || text.IndexOf("Bar") != -1 || text.IndexOf("Pyramid") != -1 || text.IndexOf("Cylinder") != -1 || text.IndexOf("Cone") != -1))
			{
				ChartSerializatorCommon.SerializeBoolValueTag(writer, "invertIfNegative", (dataPoint.DataFormat.Fill as ChartFillImpl).InvertIfNegative);
			}
			if (!dataFormatOrNull.IsDefault && (dataFormatOrNull.IsDataPointColorParsed || !dataFormatOrNull.IsParsed))
			{
				ChartSerializatorCommon.SerializeFrameFormat(writer, dataFormatOrNull, dataFormatOrNull.ParentChart, isRoundCorners: false);
			}
			else if (series.ParentChart.IsChartPie && !series.SerieFormat.LineProperties.AutoFormat && !series.SerieFormat.Interior.UseAutomaticFormat && (series.SerieFormat.HasInterior || series.SerieFormat.HasShadowProperties || series.SerieFormat.HasLineProperties))
			{
				ChartSerializatorCommon.SerializeFrameFormat(writer, series.SerieFormat, series.ParentChart, isRoundCorners: false);
			}
			if (dataPoint.IsDefaultmarkertype)
			{
				SerializeMarker(writer, dataFormatOrNull);
			}
			writer.WriteEndElement();
		}
	}

	private void SerializeSeriesCategory(XmlWriter writer, ChartSerieImpl series, string tagName)
	{
		SerializeSeriesValues(writer, series.CategoryLabelsIRange, series.EnteredDirectlyCategoryLabels, tagName, series);
	}

	private void SerializeSeriesCategory(XmlWriter writer, ChartSerieImpl series)
	{
		SerializeSeriesValues(writer, series.CategoryLabelsIRange, series.EnteredDirectlyCategoryLabels, "cat", series);
	}

	private void SerializeSeriesValues(XmlWriter writer, ChartSerieImpl series)
	{
		SerializeSeriesValues(writer, series.ValuesIRange, series.EnteredDirectlyValues, "val", series);
	}

	private void SerializeSeriesValues(XmlWriter writer, ChartSerieImpl series, string tagName)
	{
		SerializeSeriesValues(writer, series.ValuesIRange, series.EnteredDirectlyValues, tagName, series);
	}

	private void SerializeSeriesValues(XmlWriter writer, IRange range, object[] values, string tagName, ChartSerieImpl series)
	{
		if (range == null && values == null && series.NumRefFormula == null && series.StrRefFormula == null && series.MulLvlStrRefFormula == null)
		{
			return;
		}
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (tagName == null || tagName.Length == 0)
		{
			throw new ArgumentOutOfRangeException("tagName");
		}
		WorkbookImpl workbookImpl = null;
		if (range != null && range.Worksheet != null)
		{
			workbookImpl = range.Worksheet.Workbook as WorkbookImpl;
		}
		writer.WriteStartElement(tagName, "http://schemas.openxmlformats.org/drawingml/2006/chart");
		if (workbookImpl != null)
		{
			if ((workbookImpl.IsCreated || workbookImpl.IsConverted) && series.MulLvlStrRefFormula == null)
			{
				SerializeNormalReference(writer, range, values, tagName, series);
			}
			else if (workbookImpl.IsLoaded || workbookImpl.IsWorkbookOpening || series.MulLvlStrRefFormula != null)
			{
				bool flag = false;
				bool flag2 = false;
				if (range is ExternalRange)
				{
					flag = (range as ExternalRange).IsNumReference || (range as ExternalRange).IsStringReference;
					flag2 = (range as ExternalRange).IsMultiReference;
				}
				else if (range is RangeImpl)
				{
					flag = (range as RangeImpl).IsNumReference || (range as RangeImpl).IsStringReference;
					flag2 = (range as RangeImpl).IsMultiReference;
				}
				else if (range is NameImpl)
				{
					flag = (range as NameImpl).IsNumReference || (range as NameImpl).IsStringReference;
					flag2 = (range as NameImpl).IsMultiReference;
				}
				if (range != null && !flag && !flag2)
				{
					SerializeNormalReference(writer, range, values, tagName, series);
				}
				else if (range != null && flag)
				{
					SerializeReference(writer, range, values, series, tagName);
				}
				else if (range != null && flag2)
				{
					SerializeMultiLevelStringReference(writer, range, values, series);
				}
				else if (values != null)
				{
					SerializeDirectlyEntered(writer, values, isCache: false);
				}
			}
			else if (range != null && tagName != "cat")
			{
				SerializeReference(writer, range, values, series, tagName);
			}
			else if (range != null && tagName == "cat")
			{
				SerializeMultiLevelStringReference(writer, range, values, series);
			}
		}
		else if (values != null)
		{
			SerializeDirectlyEntered(writer, values, isCache: false);
		}
		else if (series.StrRefFormula != null)
		{
			SerializeFormula(writer, "strRef", series.StrRefFormula);
		}
		else if (series.NumRefFormula != null)
		{
			SerializeFormula(writer, "numRef", series.NumRefFormula);
		}
		else if (series.MulLvlStrRefFormula != null)
		{
			SerializeFormula(writer, "multiLvlStrRef", series.MulLvlStrRefFormula);
		}
		writer.WriteEndElement();
	}

	private void SerializeNormalReference(XmlWriter writer, IRange range, object[] values, string tagName, ChartSerieImpl series)
	{
		bool flag = range != null && !(range is ExternalRange) && range.HasDateTime;
		if (range != null && ((range is RangeImpl && (range as RangeImpl).IsNumReference) || (tagName != "cat" && range is RangeImpl && !(range as RangeImpl).IsStringReference) || flag || (tagName == "cat" && ((series.CategoriesFormatCode != null && (series.CategoriesFormatCode.EndsWith("%") || series.CategoriesFormatCode.ToLowerInvariant().EndsWith("y"))) || (!(range is ExternalRange) && range.NumberFormat != null && range.NumberFormat.ToLowerInvariant().EndsWith("y"))))))
		{
			SerializeReference(writer, range, values, series, tagName);
		}
		else if (range is ExternalRange && (range as ExternalRange).IsNumReference && values != null && (tagName == "val" || tagName == "cat"))
		{
			SerializeNumReference(writer, range, values, series, tagName);
		}
		else if (range != null && (tagName == "cat" || (range is RangeImpl && (range as RangeImpl).IsStringReference)))
		{
			SerializeStringReference(writer, range, series, tagName);
		}
		else if (values != null)
		{
			SerializeDirectlyEntered(writer, values, isCache: false);
		}
	}

	private void SerializeReference(XmlWriter writer, IRange range, object[] rangeValues, ChartSerieImpl series, string tagName)
	{
		if (range == null)
		{
			throw new ArgumentNullException("range");
		}
		_ = series.ParentChart;
		bool flag = false;
		if (tagName != "yVal" && tagName != "val")
		{
			if (rangeValues != null)
			{
				flag = IsStringValue(rangeValues);
			}
			flag = range.HasString;
			if (!flag && range is RangeImpl)
			{
				flag = (range as RangeImpl).IsSingleCellContainsString;
			}
		}
		if (flag && tagName != "yVal" && tagName != "val")
		{
			SerializeStringReference(writer, range, series, tagName);
		}
		else
		{
			SerializeNumReference(writer, range, rangeValues, series, tagName);
		}
	}

	private bool GetStringReference(IRange range)
	{
		WorkbookImpl workbookImpl = range.Worksheet.Workbook as WorkbookImpl;
		if (workbookImpl.IsCreated || workbookImpl.IsConverted)
		{
			return range.HasString;
		}
		if (range is ExternalRange)
		{
			return (range as ExternalRange).IsStringReference;
		}
		if (range is RangeImpl)
		{
			return (range as RangeImpl).IsStringReference;
		}
		if (range is NameImpl)
		{
			return (range as NameImpl).IsStringReference;
		}
		return false;
	}

	private bool IsStringValue(object[] rangeValues)
	{
		if (rangeValues != null)
		{
			for (int i = 0; i < rangeValues.Length; i++)
			{
				if (rangeValues[i] is string)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void SerializeNumReference(XmlWriter writer, IRange range, object[] rangeValues, ChartSerieImpl series, string tagName)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (range == null)
		{
			throw new ArgumentNullException("range");
		}
		ICombinedRange combinedRange = range as ICombinedRange;
		string text = ((combinedRange != null) ? combinedRange.AddressGlobal2007 : range.AddressGlobal);
		if (combinedRange != null && !series.InnerChart.IsAddCopied && !(combinedRange is ExternalRange) && tagName != "plus" && tagName != "minus")
		{
			rangeValues = null;
		}
		if (series.NumRefFormula != null)
		{
			text = series.NumRefFormula;
		}
		writer.WriteStartElement("numRef", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		if (series.InnerChart.IsAddCopied)
		{
			writer.WriteElementString("f", "http://schemas.openxmlformats.org/drawingml/2006/chart", "[]" + text);
		}
		else
		{
			writer.WriteElementString("f", "http://schemas.openxmlformats.org/drawingml/2006/chart", text);
		}
		if (rangeValues == null)
		{
			writer.WriteStartElement("numCache", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			if (tagName == "cat" || tagName == "xVal")
			{
				if (series.CategoriesFormatCode != null)
				{
					writer.WriteElementString("formatCode", "http://schemas.openxmlformats.org/drawingml/2006/chart", series.CategoriesFormatCode);
				}
				else
				{
					writer.WriteElementString("formatCode", "http://schemas.openxmlformats.org/drawingml/2006/chart", series.ParentChart.PrimaryCategoryAxis.NumberFormat);
				}
				SerializeCategoryTagCacheValues(writer, series, Numberformat: true);
			}
			else
			{
				if (series.FormatCode != null)
				{
					writer.WriteElementString("formatCode", "http://schemas.openxmlformats.org/drawingml/2006/chart", series.FormatCode);
				}
				else if (!(series.ValuesIRange is ExternalRange))
				{
					writer.WriteElementString("formatCode", "http://schemas.openxmlformats.org/drawingml/2006/chart", series.ValuesIRange.NumberFormat);
				}
				SerializeNumCacheValues(writer, series, tagName);
				writer.WriteEndElement();
			}
		}
		else
		{
			SerializeDirectlyEntered(writer, rangeValues, series, tagName);
		}
		writer.WriteEndElement();
	}

	private void SerializeNumCacheValues(XmlWriter writer, ChartSerieImpl series, string tagName)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		int i = 0;
		string text = string.Empty;
		IRange range = null;
		object[] array;
		if (tagName == "bubbleSize")
		{
			array = series.EnteredDirectlyBubbles;
			range = series.BubblesIRange;
		}
		else
		{
			array = series.EnteredDirectlyValues;
			range = series.ValuesIRange;
		}
		if (array != null)
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "ptCount", range.Count.ToString());
			for (; i < array.Length; i++)
			{
				if (array[i] == null)
				{
					continue;
				}
				text = ObjectValueToString(array[i]);
				if (text.Length != 0 && !char.IsWhiteSpace(text.ToCharArray()[0]))
				{
					writer.WriteStartElement("pt", "http://schemas.openxmlformats.org/drawingml/2006/chart");
					writer.WriteAttributeString("idx", i.ToString());
					if (series.FormatValueCodes.Count > 0 && series.FormatValueCodes.ContainsKey(i))
					{
						writer.WriteAttributeString("formatCode", series.FormatValueCodes[i]);
					}
					writer.WriteElementString("v", "http://schemas.openxmlformats.org/drawingml/2006/chart", text);
					writer.WriteEndElement();
				}
			}
			return;
		}
		IWorksheet worksheet = range.Worksheet;
		ChartSerializatorCommon.SerializeValueTag(writer, "ptCount", range.Count.ToString());
		for (; i < range.Count; i++)
		{
			if (!(range is ExternalRange))
			{
				IRange range2 = worksheet.Range[range.Cells[i].AddressLocal];
				if (range2.HasDateTime)
				{
					text = ToXmlString(range2.Number);
				}
				else if (range2.HasFormulaDateTime)
				{
					text = ToXmlString(range2.FormulaNumberValue);
				}
				else if (range2.HasNumber)
				{
					text = ObjectValueToString(range2.Value2);
				}
				else if (range2.HasFormulaNumberValue)
				{
					text = ObjectValueToString(range2.FormulaNumberValue);
				}
				else
				{
					text = ObjectValueToString(range2.Value2);
					if (!double.TryParse(text, out var _))
					{
						text = "";
					}
				}
			}
			if (text.Length != 0 && !char.IsWhiteSpace(text.ToCharArray()[0]))
			{
				writer.WriteStartElement("pt", "http://schemas.openxmlformats.org/drawingml/2006/chart");
				writer.WriteAttributeString("idx", i.ToString());
				if (series.FormatValueCodes.Count > 0 && series.FormatValueCodes.ContainsKey(i))
				{
					writer.WriteAttributeString("formatCode", series.FormatValueCodes[i]);
				}
				writer.WriteElementString("v", "http://schemas.openxmlformats.org/drawingml/2006/chart", text);
				writer.WriteEndElement();
			}
		}
	}

	private void SerializeStringReference(XmlWriter writer, IRange range, ChartSerieImpl series, string tagName)
	{
		string range2 = ((range is ICombinedRange combinedRange) ? combinedRange.AddressGlobal2007 : range.AddressGlobal);
		SerializeStringReference(writer, range2, series, hasSeriesName: false, tagName);
	}

	private void SerializeStringReference(XmlWriter writer, string range, ChartSerieImpl series, bool hasSeriesName, string tagName)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (range == null)
		{
			throw new ArgumentNullException("range");
		}
		if (range[0] == '=')
		{
			range = UtilityMethods.RemoveFirstCharUnsafe(range);
		}
		if (!hasSeriesName && series.StrRefFormula != null)
		{
			range = series.StrRefFormula;
		}
		writer.WriteStartElement("strRef", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		writer.WriteElementString("f", "http://schemas.openxmlformats.org/drawingml/2006/chart", range);
		writer.WriteStartElement("strCache", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		if (tagName == "cat" || tagName == "xVal")
		{
			SerializeCategoryTagCacheValues(writer, series, Numberformat: false);
		}
		else
		{
			SerializeTextTagCacheValues(writer, series);
		}
		writer.WriteEndElement();
	}

	private void SerializeTextTagCacheValues(XmlWriter writer, ChartSerieImpl series)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		int num = 0;
		ChartSerializatorCommon.SerializeValueTag(writer, "ptCount", 1.ToString());
		writer.WriteStartElement("pt", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		writer.WriteAttributeString("idx", num.ToString());
		if (!string.IsNullOrEmpty(series.Name))
		{
			writer.WriteElementString("v", "http://schemas.openxmlformats.org/drawingml/2006/chart", series.Name);
		}
		else
		{
			writer.WriteElementString("v", "http://schemas.openxmlformats.org/drawingml/2006/chart", series.SerieName);
		}
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	private void SerializeCategoryTagCacheValues(XmlWriter writer, ChartSerieImpl series, bool Numberformat)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		int i = 0;
		ChartImpl parentChart = series.ParentChart;
		IRange categoryLabelsIRange = series.CategoryLabelsIRange;
		if (series.EnteredDirectlyCategoryLabels != null || parentChart.CategoryLabelValues != null)
		{
			if (series.EnteredDirectlyCategoryLabels == null)
			{
				ChartSerializatorCommon.SerializeValueTag(writer, "ptCount", categoryLabelsIRange.Count.ToString());
				for (; i < parentChart.CategoryLabelValues.Length; i++)
				{
					string text = ToXmlString(parentChart.CategoryLabelValues[i]);
					if (text != "")
					{
						writer.WriteStartElement("pt", "http://schemas.openxmlformats.org/drawingml/2006/chart");
						writer.WriteAttributeString("idx", i.ToString());
						if (series.FormatCategoryCodes.Count > 0 && series.FormatCategoryCodes.ContainsKey(i))
						{
							writer.WriteAttributeString("formatCode", series.FormatCategoryCodes[i]);
						}
						writer.WriteElementString("v", "http://schemas.openxmlformats.org/drawingml/2006/chart", text);
						writer.WriteEndElement();
					}
				}
			}
			else
			{
				ChartSerializatorCommon.SerializeValueTag(writer, "ptCount", categoryLabelsIRange.Count.ToString());
				for (; i < series.EnteredDirectlyCategoryLabels.Length; i++)
				{
					string text2 = ToXmlString(series.EnteredDirectlyCategoryLabels[i]);
					if (text2 != "")
					{
						writer.WriteStartElement("pt", "http://schemas.openxmlformats.org/drawingml/2006/chart");
						writer.WriteAttributeString("idx", i.ToString());
						if (series.FormatCategoryCodes.Count > 0 && series.FormatCategoryCodes.ContainsKey(i))
						{
							writer.WriteAttributeString("formatCode", series.FormatCategoryCodes[i]);
						}
						writer.WriteElementString("v", "http://schemas.openxmlformats.org/drawingml/2006/chart", text2);
						writer.WriteEndElement();
					}
				}
			}
		}
		else
		{
			int count = categoryLabelsIRange.Count;
			_ = categoryLabelsIRange.Row;
			_ = categoryLabelsIRange.Column;
			IWorksheet worksheet = categoryLabelsIRange.Worksheet;
			ChartSerializatorCommon.SerializeValueTag(writer, "ptCount", count.ToString());
			bool hasDateTime = categoryLabelsIRange.HasDateTime;
			for (; i < count; i++)
			{
				string text3 = ((categoryLabelsIRange.Cells[i].HasDateTime && hasDateTime && categoryLabelsIRange.Cells[i].NumberFormat != null && categoryLabelsIRange.Cells[i].NumberFormat.ToLowerInvariant().EndsWith("y")) ? ToXmlString(categoryLabelsIRange.Cells[i].Number) : (categoryLabelsIRange.Cells[i].HasDateTime ? ((!Numberformat) ? ToXmlString(categoryLabelsIRange.Cells[i].DisplayText) : ToXmlString(categoryLabelsIRange.Cells[i].Number)) : (categoryLabelsIRange.Cells[i].HasFormulaDateTime ? categoryLabelsIRange.Cells[i].FormulaNumberValue.ToString() : ((!worksheet.Range[categoryLabelsIRange.Cells[i].AddressLocal].HasFormulaStringValue) ? ObjectValueToString(worksheet.Range[categoryLabelsIRange.Cells[i].AddressLocal].Value2) : ObjectValueToString(worksheet.Range[categoryLabelsIRange.Cells[i].AddressLocal].FormulaStringValue)))));
				if (text3 != "")
				{
					writer.WriteStartElement("pt", "http://schemas.openxmlformats.org/drawingml/2006/chart");
					writer.WriteAttributeString("idx", i.ToString());
					if (series.FormatCategoryCodes.Count > 0 && series.FormatCategoryCodes.ContainsKey(i))
					{
						writer.WriteAttributeString("formatCode", series.FormatCategoryCodes[i]);
					}
					writer.WriteElementString("v", "http://schemas.openxmlformats.org/drawingml/2006/chart", text3);
					writer.WriteEndElement();
				}
			}
		}
		writer.WriteEndElement();
	}

	private string ObjectValueToString(object value)
	{
		if (value is string)
		{
			return value.ToString();
		}
		if (value is int)
		{
			return value.ToString();
		}
		if (value is float num)
		{
			return num.ToString(CultureInfo.InvariantCulture);
		}
		if (value is double num2)
		{
			return num2.ToString(CultureInfo.InvariantCulture);
		}
		if (value is int)
		{
			return value.ToString();
		}
		if (value is decimal num3)
		{
			return num3.ToString(CultureInfo.InvariantCulture);
		}
		return value.ToString();
	}

	private void SerializeMultiLevelStringReference(XmlWriter writer, IRange range, object[] rangeValues, ChartSerieImpl series)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (range == null)
		{
			throw new ArgumentNullException("range");
		}
		string value = ((range is ICombinedRange combinedRange) ? combinedRange.AddressGlobal2007 : range.AddressGlobal);
		if (series.MultiLevelStrCache.Count == 0 && rangeValues != null)
		{
			writer.WriteStartElement("numRef", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			SerializeDirectlyEntered(writer, rangeValues, isCache: true);
		}
		else
		{
			writer.WriteStartElement("multiLvlStrRef", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			writer.WriteElementString("f", "http://schemas.openxmlformats.org/drawingml/2006/chart", value);
			SerializeMultiLevelStringCache(writer, series, range);
		}
		writer.WriteEndElement();
	}

	private void SerializeMultiLevelStringCache(XmlWriter writer, ChartSerieImpl series, IRange range)
	{
		int count = series.MultiLevelStrCache.Count;
		int num = 0;
		writer.WriteStartElement("multiLvlStrCache", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		if (count != 0)
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "ptCount", series.PointCount.ToString());
			for (int i = 0; i < count; i++)
			{
				writer.WriteStartElement("lvl", "http://schemas.openxmlformats.org/drawingml/2006/chart");
				object[] array = series.MultiLevelStrCache[i];
				num = series.MultiLevelStrCache[i].Length;
				for (int j = 0; j < num; j++)
				{
					if (array[j] != null)
					{
						writer.WriteStartElement("pt", "http://schemas.openxmlformats.org/drawingml/2006/chart");
						writer.WriteAttributeString("idx", j.ToString());
						writer.WriteStartElement("v", "http://schemas.openxmlformats.org/drawingml/2006/chart");
						writer.WriteString(ToXmlString(array[j]));
						writer.WriteEndElement();
						writer.WriteEndElement();
					}
				}
				writer.WriteEndElement();
			}
		}
		else if (range != null)
		{
			num = range.LastRow - range.Row;
			int num2 = 0;
			string text = "";
			ChartSerializatorCommon.SerializeValueTag(writer, "ptCount", num.ToString());
			bool hasDateTime = range.HasDateTime;
			for (int num3 = range.LastColumn; num3 >= range.Column; num3--)
			{
				writer.WriteStartElement("lvl", "http://schemas.openxmlformats.org/drawingml/2006/chart");
				for (int k = range.Row; k <= range.LastRow; k++)
				{
					IRange range2 = range[k, num3];
					text = ((range2.HasDateTime && hasDateTime && range2.NumberFormat != null && range2.NumberFormat.ToLowerInvariant().EndsWith("y")) ? ToXmlString(range2.Number) : (range2.HasDateTime ? ToXmlString(range2.Number) : (range2.HasFormulaDateTime ? range2.DisplayText : ((!range2.HasFormulaStringValue) ? ObjectValueToString(range2.Value2) : ObjectValueToString(range2.FormulaStringValue)))));
					if (text != "")
					{
						writer.WriteStartElement("pt", "http://schemas.openxmlformats.org/drawingml/2006/chart");
						writer.WriteAttributeString("idx", num2.ToString());
						writer.WriteStartElement("v", "http://schemas.openxmlformats.org/drawingml/2006/chart");
						writer.WriteString(text);
						writer.WriteEndElement();
						writer.WriteEndElement();
					}
					num2++;
				}
				writer.WriteEndElement();
				num2 = 0;
			}
		}
		writer.WriteEndElement();
	}

	private void SerializeFormula(XmlWriter writer, string tag, string formula)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (tag == null)
		{
			throw new ArgumentNullException("tag");
		}
		if (formula == null)
		{
			throw new ArgumentNullException("formula");
		}
		writer.WriteStartElement(tag, "http://schemas.openxmlformats.org/drawingml/2006/chart");
		writer.WriteElementString("f", "http://schemas.openxmlformats.org/drawingml/2006/chart", formula);
		writer.WriteEndElement();
	}

	[SecurityCritical]
	private void SerializeAxes(XmlWriter writer, ChartImpl chart, RelationCollection relations)
	{
		ChartAxisSerializator chartAxisSerializator = new ChartAxisSerializator();
		if (chart.IsCategoryAxisAvail && Array.IndexOf(chart.SerializedAxisIds.ToArray(), (chart.PrimaryCategoryAxis as ChartAxisImpl).AxisId) >= 0)
		{
			chartAxisSerializator.SerializeAxis(writer, chart.PrimaryCategoryAxis, relations);
		}
		if (chart.IsValueAxisAvail && Array.IndexOf(chart.SerializedAxisIds.ToArray(), (chart.PrimaryValueAxis as ChartAxisImpl).AxisId) >= 0)
		{
			chartAxisSerializator.SerializeAxis(writer, chart.PrimaryValueAxis, relations);
		}
		if (chart.IsSecondaryCategoryAxisAvail && Array.IndexOf(chart.SerializedAxisIds.ToArray(), (chart.SecondaryCategoryAxis as ChartAxisImpl).AxisId) >= 0)
		{
			chartAxisSerializator.SerializeAxis(writer, chart.SecondaryCategoryAxis, relations);
		}
		if (chart.IsSecondaryValueAxisAvail && Array.IndexOf(chart.SerializedAxisIds.ToArray(), (chart.SecondaryValueAxis as ChartAxisImpl).AxisId) >= 0)
		{
			chartAxisSerializator.SerializeAxis(writer, chart.SecondaryValueAxis, relations);
		}
		if (chart.IsSeriesAxisAvail)
		{
			chartAxisSerializator.SerializeAxis(writer, chart.PrimarySerieAxis, relations);
		}
	}

	[SecurityCritical]
	private void SerializePivotAxes(XmlWriter writer, ChartImpl chart, RelationCollection relations)
	{
		ChartAxisSerializator chartAxisSerializator = new ChartAxisSerializator();
		if (chart.IsCategoryAxisAvail)
		{
			chartAxisSerializator.SerializeAxis(writer, chart.PrimaryCategoryAxis, relations);
		}
		if (chart.IsValueAxisAvail)
		{
			chartAxisSerializator.SerializeAxis(writer, chart.PrimaryValueAxis, relations);
		}
		if (chart.IsPivotChart3D)
		{
			chartAxisSerializator.SerializeAxis(writer, chart.PrimarySerieAxis, relations);
		}
	}

	private void SerializeMarker(XmlWriter writer, ChartSerieImpl series)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		ChartSerieDataFormatImpl serieFormat = (ChartSerieDataFormatImpl)series.SerieFormat;
		SerializeMarker(writer, serieFormat);
	}

	private void SerializeMarker(XmlWriter writer, ChartSerieDataFormatImpl serieFormat)
	{
		if (serieFormat.IsMarkerSupported && serieFormat.IsMarker)
		{
			if (serieFormat.IsAutoMarker)
			{
				return;
			}
			writer.WriteStartElement("marker", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			Excel2007ChartMarkerType markerStyle = (Excel2007ChartMarkerType)serieFormat.MarkerStyle;
			if (!serieFormat.m_isMarkerDefaultSymbol)
			{
				ChartSerializatorCommon.SerializeValueTag(writer, "symbol", markerStyle.ToString());
				ChartSerializatorCommon.SerializeValueTag(writer, "size", serieFormat.MarkerSize.ToString());
			}
			if (!serieFormat.MarkerFormat.IsAutoColor)
			{
				writer.WriteStartElement("spPr", "http://schemas.openxmlformats.org/drawingml/2006/chart");
				IWorkbook parentWorkbook = serieFormat.ParentChart.ParentWorkbook;
				if (serieFormat.IsMarkerChanged || !serieFormat.MarkerFormat.IsNotShowInt)
				{
					if (serieFormat.MarkerGradient != null)
					{
						new GradientSerializator().Serialize(writer, serieFormat.MarkerGradient, parentWorkbook);
					}
					else
					{
						double alphavalue = 1.0 - serieFormat.Fill.Transparency;
						ChartSerializatorCommon.SerializeSolidFill(writer, serieFormat.MarkerBackgroundColor, isAutoColor: false, parentWorkbook, alphavalue);
					}
				}
				else if (serieFormat.MarkerFormat.IsNotShowInt)
				{
					writer.WriteElementString("noFill", "http://schemas.openxmlformats.org/drawingml/2006/main", string.Empty);
				}
				if (serieFormat.MarkerLineStream != null)
				{
					serieFormat.MarkerLineStream.Position = 0L;
					ShapeParser.WriteNodeFromStream(writer, serieFormat.MarkerLineStream);
				}
				else if (serieFormat.MarkerFormat.HasLineProperties)
				{
					SerializeLineSettings(writer, serieFormat.MarkerForegroundColor, parentWorkbook, serieFormat.MarkerFormat.IsNotShowBrd, serieFormat.MarkerTransparency);
				}
				if (serieFormat.EffectListStream != null && !serieFormat.IsMarkerChanged)
				{
					ShapeParser.WriteNodeFromStream(writer, serieFormat.EffectListStream);
				}
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}
		else if (serieFormat.IsMarkerSupported && serieFormat.HasMarkerProperties)
		{
			writer.WriteStartElement("marker", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			ChartSerializatorCommon.SerializeValueTag(writer, "symbol", "none");
			writer.WriteEndElement();
		}
	}

	internal static void SerializeLineSettings(XmlWriter writer, Color color, IWorkbook book)
	{
		SerializeLineSettings(writer, color, book, bNoFill: false);
	}

	internal static void SerializeLineSettings(XmlWriter writer, Color color, IWorkbook book, bool bNoFill)
	{
		SerializeLineSettings(writer, color, book, bNoFill, 1.0);
	}

	internal static void SerializeLineSettings(XmlWriter writer, Color color, IWorkbook book, bool bNoFill, double transparency)
	{
		writer.WriteStartElement("ln", "http://schemas.openxmlformats.org/drawingml/2006/main");
		if (!bNoFill)
		{
			ChartSerializatorCommon.SerializeSolidFill(writer, color, isAutoColor: false, book, transparency);
		}
		else
		{
			writer.WriteElementString("noFill", "http://schemas.openxmlformats.org/drawingml/2006/main", string.Empty);
		}
		writer.WriteEndElement();
	}

	[SecurityCritical]
	private void SerializeUpDownBars(XmlWriter writer, ChartImpl chart, ChartSerieImpl firstSeries)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		ChartFormatImpl chartFormatImpl = (ChartFormatImpl)firstSeries.SerieFormat.CommonSerieOptions;
		if (chartFormatImpl.IsDropBar)
		{
			IOfficeChartDropBar firstDropBar = chartFormatImpl.FirstDropBar;
			IOfficeChartDropBar secondDropBar = chartFormatImpl.SecondDropBar;
			writer.WriteStartElement("upDownBars", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			ChartSerializatorCommon.SerializeValueTag(writer, "gapWidth", chartFormatImpl.FirstDropBar.Gap.ToString());
			SerializeDropBar(writer, firstDropBar, "upBars", chart);
			SerializeDropBar(writer, secondDropBar, "downBars", chart);
			writer.WriteEndElement();
		}
	}

	[SecurityCritical]
	private void SerializeDropBar(XmlWriter writer, IOfficeChartDropBar dropBar, string tagName, ChartImpl chart)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (dropBar == null)
		{
			throw new ArgumentNullException("dropBar");
		}
		if (tagName == null || tagName.Length == 0)
		{
			throw new ArgumentOutOfRangeException("tagName");
		}
		writer.WriteStartElement(tagName, "http://schemas.openxmlformats.org/drawingml/2006/chart");
		ChartSerializatorCommon.SerializeFrameFormat(writer, dropBar, chart, isRoundCorners: false);
		writer.WriteEndElement();
	}

	internal void SerializeChartsheet(XmlWriter writer, ChartImpl chart, string drawingRelation)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (drawingRelation == null || drawingRelation.Length == 0)
		{
			throw new ArgumentOutOfRangeException("drawingRelation");
		}
		writer.WriteStartDocument();
		writer.WriteStartElement("chartsheet", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
		writer.WriteAttributeString("xmlns", "r", null, "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
		writer.WriteStartElement("sheetPr");
		string codeName = chart.CodeName;
		if ((chart.ParentWorkbook.HasMacros || chart.HasCodeName) && codeName != null && codeName.Length > 0)
		{
			writer.WriteAttributeString("codeName", codeName);
		}
		writer.WriteEndElement();
		writer.WriteStartElement("sheetViews");
		writer.WriteStartElement("sheetView");
		Excel2007Serializator.SerializeAttribute(writer, "zoomScale", chart.Zoom, 100);
		writer.WriteAttributeString("workbookViewId", "0");
		Excel2007Serializator.SerializeAttribute(writer, "zoomToFit", chart.ZoomToFit, defaultValue: false);
		writer.WriteEndElement();
		writer.WriteEndElement();
		Excel2007Serializator serializator = chart.ParentWorkbook.DataHolder.Serializator;
		serializator.SerializeSheetProtection(writer, chart);
		IPageSetupConstantsProvider constants = new WorksheetPageSetupConstants();
		Excel2007Serializator.SerializePageMargins(writer, chart.PageSetup, constants);
		Excel2007Serializator.SerializePageSetup(writer, chart.PageSetup, constants);
		Excel2007Serializator.SerializeHeaderFooter(writer, chart.PageSetupBase, constants);
		writer.WriteStartElement("drawing");
		writer.WriteAttributeString("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships", drawingRelation);
		writer.WriteEndElement();
		serializator.SerializeVmlShapesWorksheetPart(writer, chart);
		Excel2007Serializator.SerializeVmlHFShapesWorksheetPart(writer, chart, new ChartPageSetupConstants(), null);
		writer.WriteEndElement();
	}

	public void SerializeChartsheetDrawing(XmlWriter writer, ChartImpl chart, string strRelationId)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartDocument(standalone: true);
		writer.WriteStartElement("xdr", "wsDr", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		writer.WriteAttributeString("xmlns", "a", null, "http://schemas.openxmlformats.org/drawingml/2006/main");
		SerializeAbsoluteAnchorChart(writer, chart, strRelationId, isForChartSheet: true);
		writer.WriteEndElement();
	}

	internal static void SerializeAbsoluteAnchorChart(XmlWriter writer, ChartImpl chart, string strRelationId, bool isForChartSheet)
	{
		writer.WriteStartElement("absoluteAnchor", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		writer.WriteStartElement("pos", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		double num = ((chart.Workbook as WorkbookImpl).IsConverted ? 0.0 : chart.XPos);
		double num2 = ((chart.Workbook as WorkbookImpl).IsConverted ? 0.0 : chart.YPos);
		writer.WriteAttributeString("x", XmlConvert.ToString(num));
		writer.WriteAttributeString("y", XmlConvert.ToString(num2));
		writer.WriteEndElement();
		writer.WriteStartElement("ext", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		if (chart.Width <= 0.0)
		{
			writer.WriteAttributeString("cx", 8666049.ToString());
		}
		else
		{
			writer.WriteAttributeString("cx", XmlConvert.ToString(chart.EMUWidth));
		}
		if (chart.Height <= 0.0)
		{
			writer.WriteAttributeString("cy", 6293304.ToString());
		}
		else
		{
			writer.WriteAttributeString("cy", XmlConvert.ToString(chart.EMUHeight));
		}
		writer.WriteEndElement();
		bool num3 = ChartImpl.IsChartExSerieType(chart.ChartType);
		if (num3 && isForChartSheet)
		{
			writer.WriteStartElement("mc", "AlternateContent", "http://schemas.openxmlformats.org/markup-compatibility/2006");
			writer.WriteAttributeString("xmlns", "mc", null, "http://schemas.openxmlformats.org/markup-compatibility/2006");
			writer.WriteStartElement("mc", "Choice", "http://schemas.openxmlformats.org/markup-compatibility/2006");
			writer.WriteAttributeString("xmlns", "cx1", null, "http://schemas.microsoft.com/office/drawing/2015/9/8/chartex");
			writer.WriteAttributeString("Requires", "cx1");
		}
		writer.WriteStartElement("graphicFrame", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		writer.WriteAttributeString("macro", string.Empty);
		writer.WriteStartElement("nvGraphicFramePr", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		writer.WriteStartElement("cNvPr", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		writer.WriteAttributeString("id", "2");
		writer.WriteAttributeString("name", chart.Name);
		writer.WriteEndElement();
		writer.WriteStartElement("cNvGraphicFramePr", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		writer.WriteStartElement("graphicFrameLocks", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("noGrp", "1");
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
		DrawingShapeSerializator.SerializeForm(writer, "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing", "http://schemas.openxmlformats.org/drawingml/2006/main", 0, 0, 0, 0);
		writer.WriteStartElement("graphic", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteStartElement("graphicData", "http://schemas.openxmlformats.org/drawingml/2006/main");
		string[] array = strRelationId.Split(';');
		if (num3 && isForChartSheet)
		{
			writer.WriteAttributeString("uri", "http://schemas.microsoft.com/office/drawing/2014/chartex");
			writer.WriteStartElement("cx", "chart", "http://schemas.microsoft.com/office/drawing/2014/chartex");
			writer.WriteAttributeString("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships", array[0]);
		}
		else
		{
			writer.WriteAttributeString("uri", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			writer.WriteStartElement("c", "chart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			writer.WriteAttributeString("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships", array[0]);
		}
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
		if (num3 && isForChartSheet)
		{
			writer.WriteEndElement();
			SerializeChartExFallBackContentForChartSheet(writer, array[1]);
			writer.WriteEndElement();
		}
		writer.WriteElementString("clientData", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing", string.Empty);
		writer.WriteEndElement();
	}

	private static void SerializeChartExFallBackContentForChartSheet(XmlWriter writer, string relationId)
	{
		writer.WriteStartElement("mc", "Fallback", "http://schemas.openxmlformats.org/markup-compatibility/2006");
		writer.WriteStartElement("graphicFrame", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		writer.WriteAttributeString("macro", string.Empty);
		writer.WriteStartElement("nvGraphicFramePr", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		writer.WriteStartElement("cNvPr", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		writer.WriteAttributeString("id", "0");
		writer.WriteAttributeString("name", "");
		writer.WriteEndElement();
		writer.WriteStartElement("cNvGraphicFramePr", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		writer.WriteStartElement("graphicFrameLocks", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("noGrp", "1");
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
		DrawingShapeSerializator.SerializeForm(writer, "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing", "http://schemas.openxmlformats.org/drawingml/2006/main", 0, 0, 0, 0);
		writer.WriteStartElement("graphic", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteStartElement("graphicData", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("uri", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		writer.WriteStartElement("c", "chart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		writer.WriteAttributeString("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships", relationId);
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	private void SerializeDataTable(XmlWriter writer, ChartImpl chart)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (chart.HasDataTable)
		{
			IOfficeChartDataTable dataTable = chart.DataTable;
			writer.WriteStartElement("dTable", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			ChartSerializatorCommon.SerializeBoolValueTag(writer, "showHorzBorder", dataTable.HasHorzBorder);
			ChartSerializatorCommon.SerializeBoolValueTag(writer, "showVertBorder", dataTable.HasVertBorder);
			ChartSerializatorCommon.SerializeBoolValueTag(writer, "showOutline", dataTable.HasBorders);
			ChartSerializatorCommon.SerializeBoolValueTag(writer, "showKeys", dataTable.ShowSeriesKeys);
			if ((dataTable as ChartDataTableImpl).HasShapeProperties)
			{
				ShapeParser.WriteNodeFromStream(writer, (dataTable as ChartDataTableImpl).shapeStream);
			}
			if (((IInternalOfficeChartTextArea)dataTable.TextArea).ParagraphType == ChartParagraphType.CustomDefault)
			{
				WorkbookImpl parentWorkbook = ((dataTable as ChartDataTableImpl).Parent as ChartImpl).ParentWorkbook;
				SerializeDefaultTextFormatting(writer, dataTable.TextArea, parentWorkbook, 10.0, isCommonFontSize: false);
			}
			writer.WriteEndElement();
		}
	}

	private void SerializeDirectlyEntered(XmlWriter writer, object[] values, bool isCache)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		string text = null;
		if (values.Length != 0)
		{
			text = ((!isCache) ? ((values[0] is string) ? "strLit" : "numLit") : ((values[0] is string) ? "strCache" : "numCache"));
			writer.WriteStartElement(text, "http://schemas.openxmlformats.org/drawingml/2006/chart");
			int num = values.Length;
			ChartSerializatorCommon.SerializeValueTag(writer, "ptCount", num.ToString());
			for (int i = 0; i < num; i++)
			{
				if (values[i] != null)
				{
					writer.WriteStartElement("pt", "http://schemas.openxmlformats.org/drawingml/2006/chart");
					writer.WriteAttributeString("idx", i.ToString());
					writer.WriteStartElement("v", "http://schemas.openxmlformats.org/drawingml/2006/chart");
					writer.WriteString(ToXmlString(values[i]));
					writer.WriteEndElement();
					writer.WriteEndElement();
				}
			}
			writer.WriteEndElement();
		}
		else
		{
			text = ((!isCache) ? "numLit" : "numCache");
			writer.WriteStartElement(text, "http://schemas.openxmlformats.org/drawingml/2006/chart");
			ChartSerializatorCommon.SerializeValueTag(writer, "ptCount", values.Length.ToString());
			writer.WriteEndElement();
		}
	}

	private void SerializeDirectlyEntered(XmlWriter writer, object[] values, ChartSerieImpl series, string tagname)
	{
		string localName = ((values[0] is string && values[0].ToString() != string.Empty) ? "strCache" : "numCache");
		writer.WriteStartElement(localName, "http://schemas.openxmlformats.org/drawingml/2006/chart");
		if (series.CategoriesFormatCode != null && tagname == "cat")
		{
			writer.WriteElementString("formatCode", "http://schemas.openxmlformats.org/drawingml/2006/chart", series.CategoriesFormatCode);
		}
		else
		{
			writer.WriteElementString("formatCode", "http://schemas.openxmlformats.org/drawingml/2006/chart", series.FormatCode);
		}
		ChartSerializatorCommon.SerializeValueTag(writer, "ptCount", values.Length.ToString());
		for (int i = 0; i < values.Length; i++)
		{
			if (values[i] != null)
			{
				writer.WriteStartElement("pt", "http://schemas.openxmlformats.org/drawingml/2006/chart");
				writer.WriteAttributeString("idx", i.ToString());
				writer.WriteStartElement("v", "http://schemas.openxmlformats.org/drawingml/2006/chart");
				writer.WriteString(ToXmlString(values[i]));
				writer.WriteEndElement();
				writer.WriteEndElement();
			}
		}
		writer.WriteEndElement();
	}

	internal static string ToXmlString(object value)
	{
		string text = null;
		if (value == null)
		{
			return "";
		}
		if (value is double)
		{
			return XmlConvert.ToString((double)value);
		}
		if (value is float)
		{
			return XmlConvert.ToString((float)value);
		}
		if (value is decimal)
		{
			return XmlConvert.ToString((decimal)value);
		}
		return value.ToString();
	}
}
