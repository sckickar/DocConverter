using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Xml;
using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Implementation.XmlReaders.Shapes;
using DocGen.OfficeChart.Implementation.XmlSerialization.Constants;
using DocGen.OfficeChart.Interfaces.Charts;

namespace DocGen.OfficeChart.Implementation.XmlSerialization.Charts;

internal class ChartExSerializator
{
	protected Dictionary<OfficeTickMark, string> s_dictTickMarkToAttributeValue = new Dictionary<OfficeTickMark, string>(4);

	internal ChartExSerializator()
	{
		s_dictTickMarkToAttributeValue.Add(OfficeTickMark.TickMark_None, "none");
		s_dictTickMarkToAttributeValue.Add(OfficeTickMark.TickMark_Inside, "in");
		s_dictTickMarkToAttributeValue.Add(OfficeTickMark.TickMark_Outside, "out");
		s_dictTickMarkToAttributeValue.Add(OfficeTickMark.TickMark_Cross, "cross");
	}

	[SecurityCritical]
	internal void SerializeChartEx(XmlWriter writer, ChartImpl chart)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		_ = chart.DataHolder.ParentHolder;
		RelationCollection relations = chart.Relations;
		writer.WriteStartElement("cx", "chartSpace", "http://schemas.microsoft.com/office/drawing/2014/chartex");
		writer.WriteAttributeString("xmlns", "a", null, "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("xmlns", "r", null, "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
		SerializeChartExData(writer, chart);
		SerializeChartElement(writer, chart, relations);
		if (chart.HasChartArea && chart.ChartArea is ChartFrameFormatImpl chartFrameFormatImpl && IsFrameFormatChanged(chartFrameFormatImpl))
		{
			ChartSerializatorCommon.SerializeFrameFormat(writer, chartFrameFormatImpl, chart, chartFrameFormatImpl.IsBorderCornersRound);
		}
		if (chart.DefaultTextProperty != null)
		{
			chart.DefaultTextProperty.Position = 0L;
			ShapeParser.WriteNodeFromStream(writer, chart.DefaultTextProperty);
		}
		if (chart.m_colorMapOverrideStream != null)
		{
			chart.m_colorMapOverrideStream.Position = 0L;
			ShapeParser.WriteNodeFromStream(writer, chart.m_colorMapOverrideStream);
		}
		writer.WriteEndElement();
	}

	[SecurityCritical]
	private void SerializeChartElement(XmlWriter writer, ChartImpl chart, RelationCollection relations)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("chart", "http://schemas.microsoft.com/office/drawing/2014/chartex");
		if (chart.HasTitle && chart.HasTitleInternal)
		{
			SeriliazeChartTextArea(writer, chart.ChartTitleArea as ChartTextAreaImpl, chart, relations, 18.0, "title", chart.HasTitleInternal, isChartTitle: true);
		}
		SerializePlotArea(writer, chart, relations);
		if (chart.HasLegend && chart.Series.Count > 0)
		{
			SerializeLegend(writer, chart.Legend as ChartLegendImpl, chart);
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
		OfficeChartType chartType = chart.ChartType;
		bool isTreeMapOrSunBurst = chart.IsTreeMapOrSunBurst;
		writer.WriteStartElement("plotArea", "http://schemas.microsoft.com/office/drawing/2014/chartex");
		writer.WriteStartElement("plotAreaRegion", "http://schemas.microsoft.com/office/drawing/2014/chartex");
		if (chart.HasPlotArea)
		{
			writer.WriteStartElement("plotSurface", "http://schemas.microsoft.com/office/drawing/2014/chartex");
			IOfficeChartFrameFormat plotArea = chart.PlotArea;
			ChartSerializatorCommon.SerializeFrameFormat(writer, plotArea, chart, chart.ChartArea.IsBorderCornersRound);
			writer.WriteEndElement();
		}
		if (chart.IsHistogramOrPareto && chart.Series.Count > 0)
		{
			HistogramAxisFormat histogramAxisFormatProperty = (chart.Series[0].SerieFormat as ChartSerieDataFormatImpl).HistogramAxisFormatProperty;
			HistogramAxisFormat histogramAxisFormatProperty2 = (chart.PrimaryCategoryAxis as ChartCategoryAxisImpl).HistogramAxisFormatProperty;
			if (histogramAxisFormatProperty != null && !histogramAxisFormatProperty.Equals(histogramAxisFormatProperty2))
			{
				histogramAxisFormatProperty.Clone(histogramAxisFormatProperty2);
			}
		}
		for (int i = 0; i < chart.Series.Count; i++)
		{
			SerializeChartExSeries(writer, chart.Series[i] as ChartSerieImpl, i, chartType, chart, relations);
		}
		if (chartType == OfficeChartType.Pareto)
		{
			SerializeParetoSeries(writer, chart, chartType);
		}
		writer.WriteEndElement();
		if (!isTreeMapOrSunBurst)
		{
			SerializeAxes(writer, chart, chartType, relations);
		}
		writer.WriteEndElement();
	}

	[SecurityCritical]
	private void SerializeAxes(XmlWriter writer, ChartImpl chart, OfficeChartType chartType, RelationCollection relations)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		SerializeAxis(writer, chart.PrimaryCategoryAxis as ChartValueAxisImpl, chart, relations, 0);
		if (chartType != OfficeChartType.Funnel)
		{
			SerializeAxis(writer, chart.PrimaryValueAxis as ChartValueAxisImpl, chart, relations, 1);
			if (chartType == OfficeChartType.Pareto)
			{
				SerializeAxis(writer, chart.SecondaryValueAxis as ChartValueAxisImpl, chart, relations, 2);
			}
		}
	}

	[SecurityCritical]
	private void SerializeAxis(XmlWriter writer, ChartValueAxisImpl axis, ChartImpl chart, RelationCollection relations, int axisId)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (axis == null)
		{
			throw new ArgumentNullException("axis");
		}
		writer.WriteStartElement("axis", "http://schemas.microsoft.com/office/drawing/2014/chartex");
		writer.WriteAttributeString("id", axisId.ToString());
		if (axis.Deleted)
		{
			writer.WriteAttributeString("hidden", "1");
		}
		if (axisId == 0)
		{
			writer.WriteStartElement("catScaling", "http://schemas.microsoft.com/office/drawing/2014/chartex");
			double num = (double)chart.PrimaryFormats[0].GapWidth / 100.0;
			writer.WriteAttributeString("gapWidth", XmlConvert.ToString(num));
			writer.WriteEndElement();
		}
		else
		{
			writer.WriteStartElement("valScaling", "http://schemas.microsoft.com/office/drawing/2014/chartex");
			if (!axis.IsAutoMin)
			{
				writer.WriteAttributeString("min", XmlConvert.ToString(axis.MinimumValue));
			}
			if (!axis.IsAutoMax)
			{
				writer.WriteAttributeString("max", XmlConvert.ToString(axis.MaximumValue));
			}
			if (!axis.IsAutoMinor)
			{
				writer.WriteAttributeString("minorUnit", XmlConvert.ToString(axis.MinorUnit));
			}
			if (!axis.IsAutoMajor)
			{
				writer.WriteAttributeString("majorUnit", XmlConvert.ToString(axis.MajorUnit));
			}
			writer.WriteEndElement();
		}
		SerializeAxisCommon(writer, axis, chart, relations, axisId);
		writer.WriteEndElement();
	}

	[SecurityCritical]
	private void SerializeAxisCommon(XmlWriter writer, ChartValueAxisImpl axis, ChartImpl chart, RelationCollection relations, int axisId)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (axis == null)
		{
			throw new ArgumentNullException("axis");
		}
		if (axis.HasAxisTitle)
		{
			SeriliazeChartTextArea(writer, axis.TitleArea as ChartTextAreaImpl, chart, relations, 10.0, "title", isNotAuto: true, isChartTitle: false);
		}
		SerializeDisplayUnit(writer, axis, chart, relations, axisId);
		if (axis.HasMajorGridLines)
		{
			writer.WriteStartElement("majorGridlines", "http://schemas.microsoft.com/office/drawing/2014/chartex");
			ChartSerializatorCommon.SerializeFrameFormat(writer, axis.MajorGridLines, chart, isRoundCorners: false);
			writer.WriteEndElement();
		}
		if (axis.HasMinorGridLines)
		{
			writer.WriteStartElement("minorGridlines", "http://schemas.microsoft.com/office/drawing/2014/chartex");
			ChartSerializatorCommon.SerializeFrameFormat(writer, axis.MinorGridLines, chart, isRoundCorners: false);
			writer.WriteEndElement();
		}
		writer.WriteStartElement("majorTickMarks", "http://schemas.microsoft.com/office/drawing/2014/chartex");
		writer.WriteAttributeString("type", s_dictTickMarkToAttributeValue[axis.MajorTickMark]);
		writer.WriteEndElement();
		writer.WriteStartElement("minorTickMarks", "http://schemas.microsoft.com/office/drawing/2014/chartex");
		writer.WriteAttributeString("type", s_dictTickMarkToAttributeValue[axis.MinorTickMark]);
		writer.WriteEndElement();
		if (axis.TickLabelPosition != 0)
		{
			writer.WriteElementString("tickLabels", "http://schemas.microsoft.com/office/drawing/2014/chartex", "");
		}
		if (axis.isNumber && (chart.ChartType != OfficeChartType.Pareto || axisId != 2 || !axis.m_isDisplayUnitPercentage))
		{
			writer.WriteStartElement("numFmt", "http://schemas.microsoft.com/office/drawing/2014/chartex");
			writer.WriteAttributeString("formatCode", axis.NumberFormat);
			writer.WriteAttributeString("sourceLinked", axis.IsSourceLinked ? "1" : "0");
			writer.WriteEndElement();
		}
	}

	[SecurityCritical]
	private void SerializeAxisShapeAndTextProperties(XmlWriter writer, ChartValueAxisImpl axis, ChartImpl chart, RelationCollection relations)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (axis == null)
		{
			throw new ArgumentNullException("axis");
		}
		FileDataHolder parentHolder = chart.DataHolder.ParentHolder;
		if (axis.FrameFormat.HasInterior && axis.FrameFormat.Interior.Pattern != 0)
		{
			if (axis.FrameFormat.Shadow.ShadowInnerPresets == Office2007ChartPresetsInner.NoShadow && axis.FrameFormat.Shadow.ShadowOuterPresets == Office2007ChartPresetsOuter.NoShadow && axis.FrameFormat.Shadow.ShadowPerspectivePresets == Office2007ChartPresetsPerspective.NoShadow && (axis.Shadow.ShadowInnerPresets != 0 || axis.Shadow.ShadowOuterPresets != 0 || axis.Shadow.ShadowPerspectivePresets != 0))
			{
				if (axis.Shadow.ShadowInnerPresets != 0)
				{
					axis.FrameFormat.Shadow.ShadowInnerPresets = axis.Shadow.ShadowInnerPresets;
				}
				else if (axis.Shadow.ShadowOuterPresets != 0)
				{
					axis.FrameFormat.Shadow.ShadowOuterPresets = axis.Shadow.ShadowOuterPresets;
				}
				else if (axis.Shadow.ShadowPerspectivePresets != 0)
				{
					axis.FrameFormat.Shadow.ShadowPerspectivePresets = axis.Shadow.ShadowPerspectivePresets;
				}
				axis.FrameFormat.Shadow.Angle = axis.Shadow.Angle;
				axis.FrameFormat.Shadow.Blur = axis.Shadow.Blur;
				axis.FrameFormat.Shadow.Distance = axis.Shadow.Distance;
				axis.FrameFormat.Shadow.HasCustomShadowStyle = axis.Shadow.HasCustomShadowStyle;
				axis.FrameFormat.Shadow.ShadowColor = axis.Shadow.ShadowColor;
				if (axis.Shadow.ShadowInnerPresets == Office2007ChartPresetsInner.NoShadow && axis.Shadow.Size != 0)
				{
					axis.FrameFormat.Shadow.Size = axis.Shadow.Size;
				}
				axis.FrameFormat.Shadow.Transparency = axis.Shadow.Transparency;
			}
			ChartSerializatorCommon.SerializeFrameFormat(writer, axis.FrameFormat, axis.ParentChart, isRoundCorners: false);
		}
		else if (axis.FrameFormat.HasLineProperties || ((axis.Border as ChartBorderImpl).HasLineProperties && !(axis.Border as ChartBorderImpl).AutoFormat))
		{
			writer.WriteStartElement("spPr", "http://schemas.microsoft.com/office/drawing/2014/chartex");
			ChartSerializatorCommon.SerializeLineProperties(writer, axis.Border, parentHolder.Workbook);
			if (axis.Shadow.ShadowInnerPresets != 0 || axis.Shadow.ShadowOuterPresets != 0 || axis.Shadow.ShadowPerspectivePresets != 0)
			{
				ChartSerializatorCommon.SerializeShadow(writer, axis.Shadow, axis.Shadow.HasCustomShadowStyle);
			}
			writer.WriteEndElement();
		}
		if ((axis.ParagraphType != ChartParagraphType.CustomDefault || axis.TextStream == null) && axis.IsAutoTextRotation && axis.IsDefaultTextSettings)
		{
			return;
		}
		if (axis.IsDefaultTextSettings)
		{
			if (axis.TextStream != null)
			{
				axis.TextStream.Position = 0L;
				ShapeParser.WriteNodeFromStream(writer, axis.TextStream);
			}
		}
		else
		{
			ChartSerializatorCommon.SerializeDefaultTextFormatting(writer, axis.Font, chart.ParentWorkbook, 10.0, axis.IsAutoTextRotation, axis.TextRotationAngle, axis.m_textRotation, "http://schemas.microsoft.com/office/drawing/2014/chartex", isChartExText: false, isEndParagraph: false);
		}
	}

	[SecurityCritical]
	private void SerializeDisplayUnit(XmlWriter writer, ChartValueAxisImpl axis, ChartImpl chart, RelationCollection relations, int axisId)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (axis == null)
		{
			throw new ArgumentNullException("axis");
		}
		if (axis.HasDisplayUnitLabel && axis.DisplayUnit != 0)
		{
			writer.WriteStartElement("units", "http://schemas.microsoft.com/office/drawing/2014/chartex");
			if (axis.DisplayUnit != OfficeChartDisplayUnit.Custom)
			{
				writer.WriteAttributeString("unit", ((Excel2007ChartDisplayUnit)axis.DisplayUnit).ToString());
			}
			else if (axisId == 2)
			{
				if (axis.m_isDisplayUnitPercentage && chart.ChartType == OfficeChartType.Pareto)
				{
					writer.WriteAttributeString("unit", "percentage");
				}
				else
				{
					writer.WriteAttributeString("unit", Excel2007ChartDisplayUnit.hundreds.ToString());
				}
			}
			if (axis.DisplayUnitLabel.Text != null)
			{
				SeriliazeChartTextArea(writer, axis.DisplayUnitLabel as ChartTextAreaImpl, chart, relations, 10.0, "unitsLabel", isNotAuto: true, isChartTitle: false);
			}
			writer.WriteEndElement();
		}
		else if (axisId == 2)
		{
			writer.WriteStartElement("units", "http://schemas.microsoft.com/office/drawing/2014/chartex");
			writer.WriteAttributeString("unit", Excel2007ChartDisplayUnit.hundreds.ToString());
			writer.WriteEndElement();
		}
	}

	[SecurityCritical]
	private void SerializeParetoSeries(XmlWriter writer, ChartImpl chart, OfficeChartType chartType)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		for (int i = 0; i < chart.Series.Count; i++)
		{
			ChartSerieImpl chartSerieImpl = chart.Series[i] as ChartSerieImpl;
			writer.WriteStartElement("series", "http://schemas.microsoft.com/office/drawing/2014/chartex");
			SerializeSerieAttributes(writer, chartSerieImpl, chartType, i, isPareto: true);
			writer.WriteStartElement("axisId", "http://schemas.microsoft.com/office/drawing/2014/chartex");
			writer.WriteAttributeString("val", "2");
			writer.WriteEndElement();
			ChartSerializatorCommon.SerializeFrameFormat(writer, chartSerieImpl.ParetoLineFormat, chart, isRoundCorners: false);
			writer.WriteEndElement();
		}
	}

	[SecurityCritical]
	private void SerializeChartExSeries(XmlWriter writer, ChartSerieImpl serie, int serieDataIndex, OfficeChartType chartType, ChartImpl chart, RelationCollection relations)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (serie == null)
		{
			throw new ArgumentNullException("serie");
		}
		ChartDataPointImpl obj = (ChartDataPointImpl)serie.DataPoints.DefaultDataPoint;
		ChartSerieDataFormatImpl dataFormatOrNull = obj.DataFormatOrNull;
		writer.WriteStartElement("series", "http://schemas.microsoft.com/office/drawing/2014/chartex");
		SerializeSerieAttributes(writer, serie, chartType, serieDataIndex, isPareto: false);
		SerializeSeriesName(writer, serie);
		if (dataFormatOrNull != null)
		{
			ChartSerializatorCommon.SerializeFrameFormat(writer, dataFormatOrNull, serie.ParentChart, isRoundCorners: false, serializeLineAutoValues: true);
		}
		SerializeDataPointsSettings(writer, serie);
		if (obj.HasDataLabels || (serie.DataPoints as ChartDataPointsCollection).CheckDPDataLabels())
		{
			SerializeDataLabels(writer, serie, chart, relations);
		}
		writer.WriteStartElement("dataId", "http://schemas.microsoft.com/office/drawing/2014/chartex");
		writer.WriteAttributeString("val", serieDataIndex.ToString());
		writer.WriteEndElement();
		SerializeLayoutProperties(writer, serie, chart, relations);
		if (chartType == OfficeChartType.Pareto)
		{
			writer.WriteStartElement("axisId", "http://schemas.microsoft.com/office/drawing/2014/chartex");
			writer.WriteAttributeString("val", "1");
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	private void SerializeLayoutProperties(XmlWriter writer, ChartSerieImpl serie, ChartImpl chart, RelationCollection relations)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (serie == null)
		{
			throw new ArgumentNullException("serie");
		}
		OfficeChartType chartType = chart.ChartType;
		ChartSerieDataFormatImpl chartSerieDataFormatImpl = serie.SerieFormat as ChartSerieDataFormatImpl;
		bool isHistogramOrPareto = chart.IsHistogramOrPareto;
		writer.WriteStartElement("layoutPr", "http://schemas.microsoft.com/office/drawing/2014/chartex");
		if (chartType == OfficeChartType.TreeMap)
		{
			writer.WriteStartElement("parentLabelLayout", "http://schemas.microsoft.com/office/drawing/2014/chartex");
			writer.WriteAttributeString("val", chartSerieDataFormatImpl.TreeMapLabelOption.ToString().ToLower());
			writer.WriteEndElement();
		}
		if (chartType == OfficeChartType.BoxAndWhisker || chartType == OfficeChartType.WaterFall)
		{
			writer.WriteStartElement("visibility", "http://schemas.microsoft.com/office/drawing/2014/chartex");
			if (chartType == OfficeChartType.WaterFall)
			{
				writer.WriteAttributeString("connectorLines", chartSerieDataFormatImpl.ShowConnectorLines ? "1" : "0");
			}
			else
			{
				writer.WriteAttributeString("meanLine", chartSerieDataFormatImpl.ShowMeanLine ? "1" : "0");
				writer.WriteAttributeString("meanMarker", chartSerieDataFormatImpl.ShowMeanMarkers ? "1" : "0");
				writer.WriteAttributeString("outliers", chartSerieDataFormatImpl.ShowOutlierPoints ? "1" : "0");
				writer.WriteAttributeString("nonoutliers", chartSerieDataFormatImpl.ShowInnerPoints ? "1" : "0");
			}
			writer.WriteEndElement();
		}
		if (isHistogramOrPareto)
		{
			SerializeBinningProperties(writer, chartSerieDataFormatImpl, chart);
		}
		if (chartType == OfficeChartType.BoxAndWhisker)
		{
			writer.WriteStartElement("statistics", "http://schemas.microsoft.com/office/drawing/2014/chartex");
			writer.WriteAttributeString("quartileMethod", (chartSerieDataFormatImpl.QuartileCalculationType == QuartileCalculation.InclusiveMedian) ? "inclusive" : "exclusive");
			writer.WriteEndElement();
		}
		if (chartType == OfficeChartType.WaterFall)
		{
			SerializeSubTotalIndexes(writer, serie);
		}
		writer.WriteEndElement();
	}

	private void SerializeSubTotalIndexes(XmlWriter writer, ChartSerieImpl serie)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (serie == null)
		{
			throw new ArgumentNullException("serie");
		}
		ChartDataPointsCollection chartDataPointsCollection = (ChartDataPointsCollection)serie.DataPoints;
		if (chartDataPointsCollection.DeninedDPCount <= 0)
		{
			return;
		}
		List<int> list = new List<int>(chartDataPointsCollection.DeninedDPCount);
		foreach (ChartDataPointImpl item in chartDataPointsCollection)
		{
			if ((!item.IsDefault || item.HasDataPoint) && item.SetAsTotal)
			{
				list.Add(item.Index);
			}
		}
		if (list.Count > 0)
		{
			writer.WriteStartElement("subtotals", "http://schemas.microsoft.com/office/drawing/2014/chartex");
			for (int i = 0; i < list.Count; i++)
			{
				writer.WriteStartElement("idx", "http://schemas.microsoft.com/office/drawing/2014/chartex");
				writer.WriteAttributeString("val", list[i].ToString());
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}
	}

	private void SerializeBinningProperties(XmlWriter writer, ChartSerieDataFormatImpl dataFormat, ChartImpl chart)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (dataFormat == null)
		{
			throw new ArgumentNullException("dataFormat");
		}
		HistogramAxisFormat histogramAxisFormat = ((dataFormat.HistogramAxisFormatProperty != null) ? dataFormat.HistogramAxisFormatProperty : (chart.PrimaryCategoryAxis as ChartCategoryAxisImpl).HistogramAxisFormatProperty);
		if (histogramAxisFormat.IsBinningByCategory)
		{
			writer.WriteElementString("aggregation", "http://schemas.microsoft.com/office/drawing/2014/chartex", string.Empty);
			return;
		}
		writer.WriteStartElement("binning", "http://schemas.microsoft.com/office/drawing/2014/chartex");
		writer.WriteAttributeString("intervalClosed", histogramAxisFormat.IsIntervalClosedinLeft ? "l" : "r");
		if ((histogramAxisFormat.FlagOptions & 0x20) == 32)
		{
			if (!histogramAxisFormat.IsNotAutomaticUnderFlowValue)
			{
				writer.WriteAttributeString("underflow", "auto");
			}
			else
			{
				writer.WriteAttributeString("underflow", XmlConvert.ToString(histogramAxisFormat.UnderflowBinValue));
			}
		}
		if ((histogramAxisFormat.FlagOptions & 0x10) == 16)
		{
			if (!histogramAxisFormat.IsNotAutomaticOverFlowValue)
			{
				writer.WriteAttributeString("overflow", "auto");
			}
			else
			{
				writer.WriteAttributeString("overflow", XmlConvert.ToString(histogramAxisFormat.OverflowBinValue));
			}
		}
		if (!histogramAxisFormat.HasAutomaticBins)
		{
			if ((histogramAxisFormat.FlagOptions & 8) == 8)
			{
				writer.WriteStartElement("binCount", "http://schemas.microsoft.com/office/drawing/2014/chartex");
				writer.WriteAttributeString("val", histogramAxisFormat.NumberOfBins.ToString());
				writer.WriteEndElement();
			}
			else if ((histogramAxisFormat.FlagOptions & 4) == 4)
			{
				writer.WriteStartElement("binSize", "http://schemas.microsoft.com/office/drawing/2014/chartex");
				writer.WriteAttributeString("val", XmlConvert.ToString(histogramAxisFormat.BinWidth));
				writer.WriteEndElement();
			}
		}
		writer.WriteEndElement();
	}

	[SecurityCritical]
	private void SerializeDataLabels(XmlWriter writer, ChartSerieImpl serie, ChartImpl chart, RelationCollection relations)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (serie == null)
		{
			throw new ArgumentNullException("serie");
		}
		ChartImpl parentChart = serie.ParentChart;
		ChartDataPointsCollection chartDataPointsCollection = (ChartDataPointsCollection)serie.DataPoints;
		ChartDataLabelsImpl chartDataLabelsImpl = serie.DataPoints.DefaultDataPoint.DataLabels as ChartDataLabelsImpl;
		writer.WriteStartElement("dataLabels", "http://schemas.microsoft.com/office/drawing/2014/chartex");
		if (chartDataLabelsImpl.Position != 0 && chartDataLabelsImpl.Position != OfficeDataLabelPosition.Moved)
		{
			writer.WriteAttributeString("pos", ((Excel2007DataLabelPos)chartDataLabelsImpl.Position).ToString());
		}
		SerializeDataLabelSettings(writer, chartDataLabelsImpl, parentChart, relations);
		List<int> list = null;
		if (chartDataPointsCollection.DeninedDPCount > 0)
		{
			list = new List<int>();
			foreach (ChartDataPointImpl item in chartDataPointsCollection)
			{
				if (item.IsDefault || !item.HasDataLabels)
				{
					continue;
				}
				ChartDataLabelsImpl chartDataLabelsImpl2 = item.DataLabels as ChartDataLabelsImpl;
				if (chartDataLabelsImpl2.IsDelete)
				{
					list.Add(item.Index);
					continue;
				}
				writer.WriteStartElement("dataLabel", "http://schemas.microsoft.com/office/drawing/2014/chartex");
				writer.WriteAttributeString("idx", item.Index.ToString());
				if (chartDataLabelsImpl2.Position != 0 && chartDataLabelsImpl2.Position != OfficeDataLabelPosition.Moved)
				{
					writer.WriteAttributeString("pos", ((Excel2007DataLabelPos)chartDataLabelsImpl2.Position).ToString());
				}
				SerializeDataLabelSettings(writer, chartDataLabelsImpl2, parentChart, relations);
				writer.WriteEndElement();
			}
		}
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				writer.WriteStartElement("dataLabelHidden", "http://schemas.microsoft.com/office/drawing/2014/chartex");
				writer.WriteAttributeString("idx", list[i].ToString());
				writer.WriteEndElement();
			}
		}
		writer.WriteEndElement();
	}

	[SecurityCritical]
	private void SerializeDataLabelSettings(XmlWriter writer, ChartDataLabelsImpl dataLabels, ChartImpl parentChart, RelationCollection relations)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (dataLabels == null)
		{
			throw new ArgumentNullException("dataLabels");
		}
		if (dataLabels.NumberFormat != null)
		{
			SerializeNumFormat(writer, dataLabels);
		}
		writer.WriteStartElement("visibility", "http://schemas.microsoft.com/office/drawing/2014/chartex");
		if (dataLabels.IsValue)
		{
			writer.WriteAttributeString("value", "1");
		}
		if (dataLabels.IsSeriesName)
		{
			writer.WriteAttributeString("seriesName", "1");
		}
		if (dataLabels.IsCategoryName || parentChart.ChartType == OfficeChartType.TreeMap || parentChart.ChartType == OfficeChartType.SunBurst)
		{
			writer.WriteAttributeString("categoryName", "1");
		}
		writer.WriteEndElement();
		if (dataLabels.Delimiter != null)
		{
			writer.WriteElementString("separator", "http://schemas.microsoft.com/office/drawing/2014/chartex", dataLabels.Delimiter);
		}
		ChartFrameFormatImpl format = dataLabels.FrameFormat as ChartFrameFormatImpl;
		if (IsFrameFormatChanged(format))
		{
			ChartSerializatorCommon.SerializeFrameFormat(writer, dataLabels.FrameFormat, parentChart, isRoundCorners: false);
		}
		if (dataLabels.ShowTextProperties)
		{
			ChartSerializatorCommon.SerializeDefaultTextFormatting(writer, dataLabels, parentChart.ParentWorkbook, 10.0, isAutoTextRotation: true, 0, Excel2007TextRotation.horz, "http://schemas.microsoft.com/office/drawing/2014/chartex", isChartExText: false, isEndParagraph: true);
		}
	}

	private bool IsFrameFormatChanged(ChartFrameFormatImpl format)
	{
		if (((format.HasInterior || format.HasLineProperties) && (!format.Interior.UseAutomaticFormat || !format.LineProperties.AutoFormat)) || format.HasShadowProperties || format.Has3dProperties)
		{
			return true;
		}
		return false;
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
		writer.WriteStartElement("numFmt", "http://schemas.microsoft.com/office/drawing/2014/chartex");
		writer.WriteAttributeString("formatCode", dataLabels.NumberFormat);
		bool isSourceLinked = dataLabels.IsSourceLinked;
		if (!isSourceLinked)
		{
			writer.WriteAttributeString("sourceLinked", Convert.ToInt16(isSourceLinked).ToString());
		}
		writer.WriteEndElement();
	}

	[SecurityCritical]
	private void SerializeDataPointsSettings(XmlWriter writer, ChartSerieImpl serie)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (serie == null)
		{
			throw new ArgumentNullException("serie");
		}
		ChartDataPointsCollection chartDataPointsCollection = (ChartDataPointsCollection)serie.DataPoints;
		if (chartDataPointsCollection.DeninedDPCount <= 0)
		{
			return;
		}
		foreach (ChartDataPointImpl item in chartDataPointsCollection)
		{
			if (item.IsDefault && !item.HasDataPoint)
			{
				continue;
			}
			ChartSerieDataFormatImpl dataFormatOrNull = item.DataFormatOrNull;
			if (dataFormatOrNull != null && (dataFormatOrNull.IsFormatted || dataFormatOrNull.IsParsed))
			{
				writer.WriteStartElement("dataPt", "http://schemas.microsoft.com/office/drawing/2014/chartex");
				writer.WriteAttributeString("idx", item.Index.ToString());
				if (dataFormatOrNull.HasInterior || dataFormatOrNull.HasShadowProperties || dataFormatOrNull.HasLineProperties)
				{
					ChartSerializatorCommon.SerializeFrameFormat(writer, dataFormatOrNull, dataFormatOrNull.ParentChart, isRoundCorners: false);
				}
				writer.WriteEndElement();
			}
		}
	}

	private void SerializeSeriesName(XmlWriter writer, ChartSerieImpl serie)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (serie == null)
		{
			throw new ArgumentNullException("serie");
		}
		if (serie.IsDefaultName || ((serie.NameRange == null || serie.NameRange is ExternalRange) && (serie.Name == null || !(serie.Name != ""))))
		{
			return;
		}
		writer.WriteStartElement("tx", "http://schemas.microsoft.com/office/drawing/2014/chartex");
		writer.WriteStartElement("txData", "http://schemas.microsoft.com/office/drawing/2014/chartex");
		if (serie.NameRange != null && !(serie.NameRange is ExternalRange))
		{
			writer.WriteStartElement("f", "http://schemas.microsoft.com/office/drawing/2014/chartex");
			if ((serie.NameRange as ChartDataRange).Range != null)
			{
				writer.WriteString((serie.NameRange as ChartDataRange).Range.AddressGlobal);
			}
			writer.WriteEndElement();
		}
		if (!string.IsNullOrEmpty(serie.Name))
		{
			writer.WriteStartElement("v", "http://schemas.microsoft.com/office/drawing/2014/chartex");
			writer.WriteString(serie.Name);
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	private void SerializeSerieAttributes(XmlWriter writer, ChartSerieImpl serie, OfficeChartType chartType, int serieIndex, bool isPareto)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (serie == null)
		{
			throw new ArgumentNullException("serie");
		}
		OfficeChartType officeChartType = chartType;
		string value = officeChartType.ToString().ToLowerInvariant();
		switch (chartType)
		{
		case OfficeChartType.Pareto:
			value = ((!isPareto) ? "clusteredColumn" : "paretoLine");
			break;
		case OfficeChartType.Histogram:
			value = "clusteredColumn";
			break;
		case OfficeChartType.BoxAndWhisker:
			value = "boxWhisker";
			break;
		}
		writer.WriteAttributeString("layoutId", value);
		if (isPareto && serie.IsParetoLineHidden)
		{
			writer.WriteAttributeString("hidden", "1");
		}
		if (!isPareto && serie.IsSeriesHidden)
		{
			writer.WriteAttributeString("hidden", "1");
		}
		if (isPareto)
		{
			writer.WriteAttributeString("ownerIdx", serieIndex.ToString());
		}
		if (isPareto)
		{
			if (serie.ParetoLineFormatIndex != -1)
			{
				writer.WriteAttributeString("formatIdx", serie.ParetoLineFormatIndex.ToString());
			}
		}
		else if (serie.Number != -1)
		{
			writer.WriteAttributeString("formatIdx", serie.Number.ToString());
		}
	}

	[SecurityCritical]
	private void SerializeLegend(XmlWriter writer, ChartLegendImpl chartLegend, ChartImpl chart)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chartLegend == null)
		{
			throw new ArgumentNullException("chartLegend");
		}
		writer.WriteStartElement("legend", "http://schemas.microsoft.com/office/drawing/2014/chartex");
		OfficeLegendPosition position = chartLegend.Position;
		ushort num = chartLegend.ChartExPosition;
		if (position != OfficeLegendPosition.NotDocked)
		{
			num = 0;
			switch (position)
			{
			case OfficeLegendPosition.Top:
				num = (ushort)(num | 0x42u);
				break;
			case OfficeLegendPosition.Right:
				num = (ushort)(num | 0x44u);
				break;
			case OfficeLegendPosition.Left:
				num = (ushort)(num | 0x41u);
				break;
			case OfficeLegendPosition.Bottom:
				num = (ushort)(num | 0x48u);
				break;
			case OfficeLegendPosition.Corner:
				num = (ushort)(num | 0x14u);
				break;
			}
		}
		SerializeTextElementAttributes(writer, num, chartLegend.IncludeInLayout);
		ChartSerializatorCommon.SerializeFrameFormat(writer, chartLegend.FrameFormat, chart, isRoundCorners: false);
		chartLegend.IsChartTextArea = true;
		if (((IInternalOfficeChartTextArea)chartLegend.TextArea).ParagraphType == ChartParagraphType.CustomDefault)
		{
			ChartSerializatorCommon.SerializeDefaultTextFormatting(writer, chartLegend.TextArea, chart.ParentWorkbook, 10.0, isAutoTextRotation: true, 0, Excel2007TextRotation.horz, "http://schemas.microsoft.com/office/drawing/2014/chartex", isChartExText: false, isEndParagraph: true);
		}
		writer.WriteEndElement();
	}

	[SecurityCritical]
	private void SeriliazeChartTextArea(XmlWriter writer, ChartTextAreaImpl chartTextArea, ChartImpl chart, RelationCollection relations, double defaultFontSize, string parentElement, bool isNotAuto, bool isChartTitle)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chartTextArea == null)
		{
			throw new ArgumentNullException("chartTextArea");
		}
		writer.WriteStartElement(parentElement, "http://schemas.microsoft.com/office/drawing/2014/chartex");
		bool flag = false;
		if (isChartTitle)
		{
			SerializeTextElementAttributes(writer, chart.ChartExTitlePosition, chart.ChartTitleIncludeInLayout);
		}
		if (isNotAuto)
		{
			chartTextArea.ShowBoldProperties = true;
			FileDataHolder dataHolder = chart.ParentWorkbook.DataHolder;
			if (chartTextArea.HasText)
			{
				flag = SerializeTextAreaText(writer, chartTextArea, chart, defaultFontSize);
			}
			ChartSerializatorCommon.SerializeFrameFormat(writer, chartTextArea.FrameFormat, dataHolder, relations, isRoundCorners: false, serilaizeLineAutoValues: false);
			if (!flag && chartTextArea.ParagraphType == ChartParagraphType.CustomDefault)
			{
				ChartSerializatorCommon.SerializeDefaultTextFormatting(writer, chartTextArea, chart.ParentWorkbook, defaultFontSize, isAutoTextRotation: true, 0, Excel2007TextRotation.horz, "http://schemas.microsoft.com/office/drawing/2014/chartex", isChartExText: true, isEndParagraph: false);
			}
		}
		writer.WriteEndElement();
	}

	private void SerializeTextElementAttributes(XmlWriter writer, ushort position, bool isLayout)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		writer.WriteAttributeString("overlay", isLayout ? "1" : "0");
		if (position == 0)
		{
			writer.WriteAttributeString("align", ChartExPositionAlignment.ctr.ToString());
			writer.WriteAttributeString("pos", ChartExSidePosition.t.ToString());
			return;
		}
		ushort num = 240;
		ChartExPositionAlignment chartExPositionAlignment = (ChartExPositionAlignment)(position & num);
		num = 15;
		ChartExSidePosition chartExSidePosition = (ChartExSidePosition)(position & num);
		writer.WriteAttributeString("align", chartExPositionAlignment.ToString());
		writer.WriteAttributeString("pos", chartExSidePosition.ToString());
	}

	private bool SerializeTextAreaText(XmlWriter writer, ChartTextAreaImpl chartTextArea, ChartImpl chart, double defaultFontSize)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chartTextArea == null)
		{
			throw new ArgumentNullException("chartTextArea");
		}
		bool result = false;
		writer.WriteStartElement("tx", "http://schemas.microsoft.com/office/drawing/2014/chartex");
		if (chartTextArea.ChartAlRuns != null && chartTextArea.ChartAlRuns.Runs != null && chartTextArea.ChartAlRuns.Runs.Length != 0)
		{
			ChartSerializatorCommon.SerializeRichText(writer, chartTextArea, chart.ParentWorkbook, "rich", defaultFontSize);
			result = true;
		}
		else
		{
			writer.WriteStartElement("txData", "http://schemas.microsoft.com/office/drawing/2014/chartex");
			if (chartTextArea.IsFormula)
			{
				writer.WriteStartElement("f", "http://schemas.microsoft.com/office/drawing/2014/chartex");
				writer.WriteString(chartTextArea.Text);
				writer.WriteEndElement();
			}
			else
			{
				writer.WriteStartElement("v", "http://schemas.microsoft.com/office/drawing/2014/chartex");
				writer.WriteString(chartTextArea.Text);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
		return result;
	}

	private void SerializeChartExData(XmlWriter writer, ChartImpl chart)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		OfficeChartType chartType = chart.ChartType;
		writer.WriteStartElement("chartData", "http://schemas.microsoft.com/office/drawing/2014/chartex");
		string text = chart.Relations.GenerateRelationId();
		chart.Relations[text] = new Relation("", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/package");
		if (chart.ParentWorkbook.IsLoaded && chart.ChartExRelationId != null)
		{
			writer.WriteStartElement("externalData", "http://schemas.microsoft.com/office/drawing/2014/chartex");
			writer.WriteAttributeString("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships", text);
			if (chart.AutoUpdate.HasValue)
			{
				string value = (chart.AutoUpdate.Value ? "1" : "0");
				writer.WriteAttributeString("autoUpdate", "http://schemas.microsoft.com/office/drawing/2014/chartex", value);
			}
			writer.WriteEndElement();
		}
		else if (chart.ParentWorkbook.IsCreated)
		{
			writer.WriteStartElement("externalData", "http://schemas.microsoft.com/office/drawing/2014/chartex");
			writer.WriteAttributeString("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships", text);
			writer.WriteAttributeString("autoUpdate", "http://schemas.microsoft.com/office/drawing/2014/chartex", "0");
			writer.WriteEndElement();
		}
		for (int i = 0; i < chart.Series.Count; i++)
		{
			SerializeIndividualChartSerieData(writer, chart.Series[i] as ChartSerieImpl, i, chartType);
		}
		writer.WriteEndElement();
	}

	private void SerializeIndividualChartSerieData(XmlWriter writer, ChartSerieImpl serie, int index, OfficeChartType chartType)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (serie == null)
		{
			throw new ArgumentNullException("serie");
		}
		bool isTreeMapOrSunBurst = serie.InnerChart.IsTreeMapOrSunBurst;
		writer.WriteStartElement("data", "http://schemas.microsoft.com/office/drawing/2014/chartex");
		writer.WriteAttributeString("id", index.ToString());
		writer.WriteStartElement("numDim", "http://schemas.microsoft.com/office/drawing/2014/chartex");
		string value = (isTreeMapOrSunBurst ? "size" : "val");
		writer.WriteAttributeString("type", value);
		SerializeDimensionData(writer, serie.Values, serie.EnteredDirectlyValues, serie.IsRowWiseSeries, serie.FormatCode, isCategoryRange: false, isTreeMapOrSunBurst);
		writer.WriteEndElement();
		if (serie.CategoryLabels == null && serie.EnteredDirectlyCategoryLabels != null)
		{
			if (serie.EnteredDirectlyCategoryLabels.Length != 0 && serie.EnteredDirectlyCategoryLabels[0] is string)
			{
				writer.WriteStartElement("strDim", "http://schemas.microsoft.com/office/drawing/2014/chartex");
			}
			else
			{
				writer.WriteStartElement("numDim", "http://schemas.microsoft.com/office/drawing/2014/chartex");
			}
		}
		else
		{
			writer.WriteStartElement("strDim", "http://schemas.microsoft.com/office/drawing/2014/chartex");
		}
		writer.WriteAttributeString("type", "cat");
		SerializeDimensionData(writer, serie.CategoryLabels, serie.EnteredDirectlyCategoryLabels, serie.IsRowWiseCategory, serie.CategoriesFormatCode, isCategoryRange: true, isTreeMapOrSunBurst);
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	private void SerializeDimensionData(XmlWriter writer, IOfficeDataRange range, object[] directValues, bool isInRow, string formatCode, bool isCategoryRange, bool isTreeMaporSunburst)
	{
		if (range != null)
		{
			writer.WriteStartElement("f", "http://schemas.microsoft.com/office/drawing/2014/chartex");
			if (!(range is IRanges))
			{
				writer.WriteAttributeString("dir", isInRow ? "row" : "col");
			}
			if (range is NameImpl)
			{
				writer.WriteString((range as NameImpl).Name);
			}
			else if ((range as ChartDataRange).Range != null)
			{
				writer.WriteString((range as ChartDataRange).Range.AddressGlobal);
			}
			writer.WriteEndElement();
		}
		if (directValues == null && (range as ChartDataRange).Range == null)
		{
			return;
		}
		int i = 0;
		IRange range2 = (range as ChartDataRange).Range;
		IWorksheet worksheet = range2.Worksheet;
		for (int num = (range as ChartDataRange).Range.Columns.Count(); num > 0; num--)
		{
			int count = (range as ChartDataRange).Range.Columns[num - 1].Count;
			List<int> list = new List<int>();
			int num2 = 0;
			IRange[] cells = (range as ChartDataRange).Range.Columns[num - 1].Cells;
			foreach (IRange range3 in cells)
			{
				if (range3.Value == "" || range3.Value == null)
				{
					list.Add(num2);
				}
				num2++;
			}
			writer.WriteStartElement("lvl", "http://schemas.microsoft.com/office/drawing/2014/chartex");
			writer.WriteAttributeString("ptCount", count.ToString());
			formatCode = ((formatCode != null && formatCode != string.Empty) ? formatCode : "General");
			if (!isCategoryRange)
			{
				writer.WriteAttributeString("formatCode", formatCode);
			}
			if (directValues != null)
			{
				int num3 = directValues.Length;
				for (int k = 0; k < count; k++)
				{
					if (!list.Contains(k))
					{
						if (i < num3 && directValues[i] != null)
						{
							writer.WriteStartElement("cx", "pt", null);
							writer.WriteAttributeString("idx", k.ToString());
							writer.WriteString(ChartSerializator.ToXmlString(directValues[i]));
							writer.WriteEndElement();
						}
						i++;
					}
				}
				writer.WriteEndElement();
			}
			else if ((range as ChartDataRange).Range != null)
			{
				for (; i < count; i++)
				{
					string text = ObjectValueToString(worksheet.Range[range2.Cells[i].AddressLocal].Value2);
					if (text != null && text != "")
					{
						writer.WriteStartElement("cx", "pt", null);
						writer.WriteAttributeString("idx", i.ToString());
						writer.WriteString(text);
						writer.WriteEndElement();
					}
				}
				writer.WriteEndElement();
			}
		}
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
		writer.WriteStartElement("printSettings", "http://schemas.microsoft.com/office/drawing/2014/chartex");
		IPageSetupConstantsProvider constants = new ChartPageSetupConstants();
		Excel2007Serializator.SerializePrintSettings(writer, pageSetup, constants, isChartSettings: true);
		Excel2007Serializator.SerializeVmlHFShapesWorksheetPart(writer, chart, constants, relations);
		chart.DataHolder.SerializeHeaderFooterImages(chart, relations);
		writer.WriteEndElement();
	}
}
