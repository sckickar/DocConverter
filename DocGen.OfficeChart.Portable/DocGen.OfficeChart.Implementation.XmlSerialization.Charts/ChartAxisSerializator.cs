using System;
using System.Collections.Generic;
using System.Security;
using System.Xml;
using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Implementation.XmlReaders.Shapes;

namespace DocGen.OfficeChart.Implementation.XmlSerialization.Charts;

internal class ChartAxisSerializator
{
	public const int TextRotationMultiplier = 60000;

	private static Dictionary<OfficeTickLabelPosition, string> s_dictTickLabelToAttributeValue;

	private static Dictionary<OfficeTickMark, string> s_dictTickMarkToAttributeValue;

	static ChartAxisSerializator()
	{
		s_dictTickLabelToAttributeValue = new Dictionary<OfficeTickLabelPosition, string>(4);
		s_dictTickMarkToAttributeValue = new Dictionary<OfficeTickMark, string>(4);
		s_dictTickLabelToAttributeValue.Add(OfficeTickLabelPosition.TickLabelPosition_High, "high");
		s_dictTickLabelToAttributeValue.Add(OfficeTickLabelPosition.TickLabelPosition_Low, "low");
		s_dictTickLabelToAttributeValue.Add(OfficeTickLabelPosition.TickLabelPosition_NextToAxis, "nextTo");
		s_dictTickLabelToAttributeValue.Add(OfficeTickLabelPosition.TickLabelPosition_None, "none");
		s_dictTickMarkToAttributeValue.Add(OfficeTickMark.TickMark_None, "none");
		s_dictTickMarkToAttributeValue.Add(OfficeTickMark.TickMark_Inside, "in");
		s_dictTickMarkToAttributeValue.Add(OfficeTickMark.TickMark_Outside, "out");
		s_dictTickMarkToAttributeValue.Add(OfficeTickMark.TickMark_Cross, "cross");
	}

	[SecurityCritical]
	public void SerializeAxis(XmlWriter writer, IOfficeChartAxis axis, RelationCollection relations)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (axis == null)
		{
			return;
		}
		switch (axis.AxisType)
		{
		case OfficeAxisType.Category:
		{
			ChartCategoryAxisImpl chartCategoryAxisImpl = (ChartCategoryAxisImpl)axis;
			if (!chartCategoryAxisImpl.IsChartBubbleOrScatter)
			{
				if (chartCategoryAxisImpl.CategoryType == OfficeCategoryType.Time)
				{
					SerializeDateAxis(writer, chartCategoryAxisImpl);
				}
				else
				{
					SerializeCategoryAxis(writer, chartCategoryAxisImpl);
				}
			}
			else
			{
				SerializeValueAxis(writer, (ChartValueAxisImpl)axis, relations);
			}
			break;
		}
		case OfficeAxisType.Value:
			SerializeValueAxis(writer, (ChartValueAxisImpl)axis, relations);
			break;
		case OfficeAxisType.Serie:
			SerializeSeriesAxis(writer, (ChartSeriesAxisImpl)axis);
			break;
		default:
			throw new NotSupportedException();
		}
	}

	[SecurityCritical]
	private void SerializeDateAxis(XmlWriter writer, ChartCategoryAxisImpl axis)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (axis == null)
		{
			throw new ArgumentNullException("axis");
		}
		writer.WriteStartElement("dateAx", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		SerializeAxisCommon(writer, axis);
		if (axis.CategoryType == OfficeCategoryType.Automatic)
		{
			ChartSerializatorCommon.SerializeBoolValueTag(writer, "auto", value: true);
		}
		else
		{
			ChartSerializatorCommon.SerializeBoolValueTag(writer, "auto", value: false);
		}
		string value = ((axis.LabelAlign == AxisLabelAlignment.Left) ? "l" : ((axis.LabelAlign == AxisLabelAlignment.Right) ? "r" : "ctr"));
		ChartSerializatorCommon.SerializeValueTag(writer, "lblAlgn", value);
		ChartSerializatorCommon.SerializeValueTag(writer, "lblOffset", axis.Offset.ToString());
		if (!axis.IsAutoMajor)
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "majorUnit", XmlConvert.ToString(axis.MajorUnit));
		}
		if (!axis.MajorUnitScaleIsAuto || (axis.ParentChart.Workbook as WorkbookImpl).IsConverted)
		{
			string value2 = ConvertDateUnitToString(axis.MajorUnitScale);
			ChartSerializatorCommon.SerializeValueTag(writer, "majorTimeUnit", value2);
		}
		if (!axis.IsAutoMinor)
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "minorUnit", XmlConvert.ToString(axis.MinorUnit));
		}
		if (!axis.MinorUnitScaleIsAuto || (axis.ParentChart.Workbook as WorkbookImpl).IsConverted)
		{
			string value2 = ConvertDateUnitToString(axis.MinorUnitScale);
			ChartSerializatorCommon.SerializeValueTag(writer, "minorTimeUnit", value2);
		}
		if (!axis.BaseUnitIsAuto)
		{
			string value3 = ConvertDateUnitToString(axis.BaseUnit);
			ChartSerializatorCommon.SerializeValueTag(writer, "baseTimeUnit", value3);
		}
		writer.WriteEndElement();
	}

	private string ConvertDateUnitToString(OfficeChartBaseUnit baseUnit)
	{
		return baseUnit.ToString().ToLower() + "s";
	}

	[SecurityCritical]
	private void SerializeCategoryAxis(XmlWriter writer, ChartCategoryAxisImpl axis)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (axis == null)
		{
			throw new ArgumentNullException("axis");
		}
		writer.WriteStartElement("catAx", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		SerializeAxisCommon(writer, axis);
		if (axis.CategoryType == OfficeCategoryType.Automatic)
		{
			ChartSerializatorCommon.SerializeBoolValueTag(writer, "auto", value: true);
		}
		else
		{
			ChartSerializatorCommon.SerializeBoolValueTag(writer, "auto", value: false);
		}
		string value = ((axis.LabelAlign == AxisLabelAlignment.Left) ? "l" : ((axis.LabelAlign == AxisLabelAlignment.Right) ? "r" : "ctr"));
		ChartSerializatorCommon.SerializeValueTag(writer, "lblAlgn", value);
		ChartSerializatorCommon.SerializeValueTag(writer, "lblOffset", axis.Offset.ToString());
		ChartSerializatorCommon.SerializeBoolValueTag(writer, "noMultiLvlLbl", axis.NoMultiLevelLabel);
		if (!axis.IsAutoMajor || !axis.AutoTickLabelSpacing)
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "tickLblSkip", axis.TickLabelSpacing.ToString());
		}
		if (!axis.IsAutoMinor || !axis.AutoTickMarkSpacing)
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "tickMarkSkip", axis.TickMarkSpacing.ToString());
		}
		writer.WriteEndElement();
	}

	[SecurityCritical]
	private void SerializeValueAxis(XmlWriter writer, ChartValueAxisImpl valueAxis, RelationCollection relations)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (valueAxis == null)
		{
			throw new ArgumentNullException("valueAxis");
		}
		writer.WriteStartElement("valAx", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		SerializeAxisCommon(writer, valueAxis);
		ChartImpl parentChart = valueAxis.ParentChart;
		string value = ((valueAxis.IsPrimary ? parentChart.PrimaryCategoryAxis : parentChart.SecondaryCategoryAxis).IsBetween ? "between" : "midCat");
		ChartSerializatorCommon.SerializeValueTag(writer, "crossBetween", value);
		if (!valueAxis.IsAutoMajor)
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "majorUnit", XmlConvert.ToString(valueAxis.MajorUnit));
		}
		if (!valueAxis.IsAutoMinor)
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "minorUnit", XmlConvert.ToString(valueAxis.MinorUnit));
		}
		SerializeDisplayUnit(writer, valueAxis, relations);
		writer.WriteEndElement();
	}

	[SecurityCritical]
	private void SerializeSeriesAxis(XmlWriter writer, ChartSeriesAxisImpl seriesAxis)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (seriesAxis == null)
		{
			throw new ArgumentNullException("seriesAxis");
		}
		writer.WriteStartElement("serAx", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		SerializeAxisCommon(writer, seriesAxis);
		if (!seriesAxis.AutoTickLabelSpacing)
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "tickLblSkip", seriesAxis.TickLabelSpacing.ToString());
		}
		if (!seriesAxis.AutoTickMarkSpacing)
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "tickMarkSkip", seriesAxis.TickMarkSpacing.ToString());
		}
		writer.WriteEndElement();
	}

	[SecurityCritical]
	private void SerializeAxisCommon(XmlWriter writer, ChartAxisImpl axis)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (axis == null)
		{
			throw new ArgumentNullException("axis");
		}
		ChartImpl parentChart = axis.ParentChart;
		FileDataHolder parentHolder = parentChart.DataHolder.ParentHolder;
		RelationCollection relations = parentChart.Relations;
		ChartSerializatorCommon.SerializeValueTag(writer, "axId", axis.AxisId.ToString());
		SerializeScaling(writer, axis);
		ChartSerializatorCommon.SerializeBoolValueTag(writer, "delete", axis.Deleted);
		SerializeAxisPosition(writer, axis);
		SerializeGridlines(writer, axis);
		if (axis.HasAxisTitle)
		{
			ChartSerializatorCommon.SerializeTextArea(writer, axis.TitleArea, parentChart.ParentWorkbook, relations, 10.0);
		}
		bool isPrimary = axis.IsPrimary;
		if (axis.isNumber)
		{
			SerializeNumberFormat(writer, axis);
		}
		else if (axis.AxisType == OfficeAxisType.Value && (isPrimary ? axis.ParentChart.PrimaryFormats : axis.ParentChart.SecondaryFormats).IsPercentStackedAxis)
		{
			writer.WriteStartElement("numFmt", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			writer.WriteAttributeString("formatCode", "0%");
			writer.WriteAttributeString("sourceLinked", "1");
			writer.WriteEndElement();
		}
		SerializeTickMark(writer, "majorTickMark", axis.MajorTickMark);
		SerializeTickMark(writer, "minorTickMark", axis.MinorTickMark);
		SerializeTickLabel(writer, axis);
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
		else if ((axis.FrameFormat.HasLineProperties || (axis.Border as ChartBorderImpl).HasLineProperties) && !axis.Border.AutoFormat)
		{
			writer.WriteStartElement("spPr", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			ChartSerializatorCommon.SerializeLineProperties(writer, axis.Border, parentHolder.Workbook);
			if (axis.Shadow.ShadowInnerPresets != 0 || axis.Shadow.ShadowOuterPresets != 0 || axis.Shadow.ShadowPerspectivePresets != 0)
			{
				ChartSerializatorCommon.SerializeShadow(writer, axis.Shadow, axis.Shadow.HasCustomShadowStyle);
			}
			writer.WriteEndElement();
		}
		if (axis.ParagraphType == ChartParagraphType.CustomDefault || !axis.IsAutoTextRotation)
		{
			SerializeTextSettings(writer, axis);
		}
		SerializeCrossAxis(writer, axis);
		string value = "autoZero";
		string tagName = "crosses";
		if (GetPairAxis(axis) is ChartValueAxisImpl chartValueAxisImpl)
		{
			if (chartValueAxisImpl.IsMaxCross)
			{
				value = "max";
			}
			else if (!chartValueAxisImpl.IsAutoCross)
			{
				value = XmlConvert.ToString(chartValueAxisImpl.CrossesAt);
				tagName = "crossesAt";
			}
		}
		ChartSerializatorCommon.SerializeValueTag(writer, tagName, value);
	}

	public static IOfficeChartAxis GetPairAxis(ChartAxisImpl axis)
	{
		IOfficeChartAxis result = null;
		if (axis != null)
		{
			ChartImpl parentChart = axis.ParentChart;
			switch (axis.AxisType)
			{
			case OfficeAxisType.Category:
				result = (axis.IsPrimary ? parentChart.PrimaryValueAxis : parentChart.SecondaryValueAxis);
				break;
			case OfficeAxisType.Value:
				result = (axis.IsPrimary ? parentChart.PrimaryCategoryAxis : parentChart.SecondaryCategoryAxis);
				break;
			}
		}
		return result;
	}

	private void SerializeCrossAxis(XmlWriter writer, ChartAxisImpl axis)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (axis == null)
		{
			throw new ArgumentNullException("axis");
		}
		ChartImpl parentChart = axis.ParentChart;
		bool isPrimary = axis.IsPrimary;
		ChartSerializatorCommon.SerializeValueTag(writer, "crossAx", (axis.AxisType switch
		{
			OfficeAxisType.Category => (ChartAxisImpl)(isPrimary ? parentChart.PrimaryValueAxis : parentChart.SecondaryValueAxis), 
			OfficeAxisType.Serie => (ChartAxisImpl)parentChart.PrimaryValueAxis, 
			OfficeAxisType.Value => (ChartAxisImpl)(isPrimary ? parentChart.PrimaryCategoryAxis : parentChart.SecondaryCategoryAxis), 
			_ => throw new InvalidOperationException(), 
		}).AxisId.ToString());
	}

	private void SerializeTickMark(XmlWriter writer, string tagName, OfficeTickMark tickMark)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (tagName == null || tagName.Length == 0)
		{
			throw new ArgumentException("tagName");
		}
		string value = s_dictTickMarkToAttributeValue[tickMark];
		ChartSerializatorCommon.SerializeValueTag(writer, tagName, value);
	}

	private void SerializeTickLabel(XmlWriter writer, ChartAxisImpl axis)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (axis == null)
		{
			throw new ArgumentNullException("axis");
		}
		string value = s_dictTickLabelToAttributeValue[axis.TickLabelPosition];
		ChartSerializatorCommon.SerializeValueTag(writer, "tickLblPos", value);
	}

	private void SerializeNumberFormat(XmlWriter writer, ChartAxisImpl axis)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (axis == null)
		{
			throw new ArgumentNullException("axis");
		}
		writer.WriteStartElement("numFmt", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		writer.WriteAttributeString("formatCode", axis.NumberFormat);
		bool isSourceLinked = axis.IsSourceLinked;
		Excel2007Serializator.SerializeAttribute(writer, "sourceLinked", isSourceLinked, !isSourceLinked);
		writer.WriteEndElement();
	}

	private void SerializeGridlines(XmlWriter writer, ChartAxisImpl axis)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (axis == null)
		{
			throw new ArgumentNullException("axis");
		}
		WorkbookImpl parentWorkbook = axis.ParentChart.ParentWorkbook;
		if (axis.HasMajorGridLines)
		{
			SerializeGridlines(writer, axis.MajorGridLines, "majorGridlines", parentWorkbook);
		}
		if (axis.HasMinorGridLines)
		{
			SerializeGridlines(writer, axis.MinorGridLines, "minorGridlines", parentWorkbook);
		}
	}

	private void SerializeGridlines(XmlWriter writer, IOfficeChartGridLine gridLines, string tagName, IWorkbook book)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (gridLines == null)
		{
			throw new ArgumentNullException("gridLines");
		}
		if (tagName == null || tagName.Length == 0)
		{
			throw new ArgumentOutOfRangeException("tagName");
		}
		writer.WriteStartElement(tagName, "http://schemas.openxmlformats.org/drawingml/2006/chart");
		IOfficeChartBorder border = gridLines.Border;
		if (border != null && !border.AutoFormat)
		{
			writer.WriteStartElement("spPr", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			ChartSerializatorCommon.SerializeLineProperties(writer, border, book);
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	private void SerializeAxisPosition(XmlWriter writer, ChartAxisImpl axis)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (axis == null)
		{
			throw new ArgumentNullException("valueAxis");
		}
		string text = (axis.AxisPosition.HasValue ? axis.AxisPosition.ToString() : null);
		bool isPrimary = axis.IsPrimary;
		bool isBarChartAxes = (isPrimary ? axis.ParentChart.PrimaryFormats : axis.ParentChart.SecondaryFormats).IsBarChartAxes;
		if (text == null)
		{
			switch (axis.AxisType)
			{
			case OfficeAxisType.Category:
				text = ((!isPrimary) ? (isBarChartAxes ? "r" : "t") : (isBarChartAxes ? "l" : "b"));
				break;
			case OfficeAxisType.Value:
				text = ((!isPrimary) ? (isBarChartAxes ? "t" : "r") : (isBarChartAxes ? "b" : "l"));
				break;
			case OfficeAxisType.Serie:
				text = (isPrimary ? "b" : "t");
				break;
			}
		}
		if (text != null)
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "axPos", text);
		}
	}

	private void SerializeScaling(XmlWriter writer, ChartAxisImpl axis)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (axis == null)
		{
			throw new ArgumentNullException("axis");
		}
		writer.WriteStartElement("scaling", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		ChartValueAxisImpl chartValueAxisImpl = axis as ChartValueAxisImpl;
		if (chartValueAxisImpl != null && chartValueAxisImpl.IsLogScale)
		{
			ChartSerializatorCommon.SerializeValueTag(writer, "logBase", XmlConvert.ToString(chartValueAxisImpl.LogBase));
		}
		string value = (axis.ReversePlotOrder ? "maxMin" : "minMax");
		ChartSerializatorCommon.SerializeValueTag(writer, "orientation", value);
		if (chartValueAxisImpl != null)
		{
			if (!chartValueAxisImpl.IsAutoMax)
			{
				ChartSerializatorCommon.SerializeValueTag(writer, "max", XmlConvert.ToString(chartValueAxisImpl.MaximumValue));
			}
			if (!chartValueAxisImpl.IsAutoMin)
			{
				ChartSerializatorCommon.SerializeValueTag(writer, "min", XmlConvert.ToString(chartValueAxisImpl.MinimumValue));
			}
		}
		writer.WriteEndElement();
	}

	[SecurityCritical]
	private void SerializeDisplayUnit(XmlWriter writer, ChartValueAxisImpl axis, RelationCollection relations)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		OfficeChartDisplayUnit displayUnit = axis.DisplayUnit;
		if (displayUnit == OfficeChartDisplayUnit.None)
		{
			return;
		}
		bool hasDisplayUnitLabel = axis.HasDisplayUnitLabel;
		writer.WriteStartElement("dispUnits", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		if (displayUnit != OfficeChartDisplayUnit.Custom)
		{
			Excel2007ChartDisplayUnit excel2007ChartDisplayUnit = (Excel2007ChartDisplayUnit)displayUnit;
			string value = excel2007ChartDisplayUnit.ToString();
			ChartSerializatorCommon.SerializeValueTag(writer, "builtInUnit", value);
		}
		else
		{
			string value = XmlConvert.ToString(axis.DisplayUnitCustom);
			ChartSerializatorCommon.SerializeValueTag(writer, "custUnit", value);
		}
		if (hasDisplayUnitLabel)
		{
			ChartImpl parentChart = axis.ParentChart;
			WorkbookImpl parentWorkbook = parentChart.ParentWorkbook;
			IOfficeChartTextArea displayUnitLabel = axis.DisplayUnitLabel;
			ChartTextAreaImpl chartTextAreaImpl = displayUnitLabel as ChartTextAreaImpl;
			writer.WriteStartElement("dispUnitsLbl", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			_ = (displayUnitLabel.Layout as ChartLayoutImpl).IsManualLayout;
			ChartSerializatorCommon.SerializeLayout(writer, displayUnitLabel);
			if (axis.DisplayUnitLabel.Text != null)
			{
				ChartSerializatorCommon.SerializeTextAreaText(writer, displayUnitLabel, parentWorkbook, axis.DisplayUnitLabel.Size);
			}
			ChartSerializatorCommon.SerializeFrameFormat(writer, displayUnitLabel.FrameFormat, parentChart, isRoundCorners: false);
			if (chartTextAreaImpl.ParagraphType == ChartParagraphType.CustomDefault)
			{
				SerializeTextSettings(writer, parentWorkbook, displayUnitLabel, !chartTextAreaImpl.HasTextRotation, chartTextAreaImpl.TextRotationAngle, axis.LabelTextAlign);
			}
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	private void SerializeTextSettings(XmlWriter writer, ChartAxisImpl axis)
	{
		if (axis == null)
		{
			throw new ArgumentNullException("axis");
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
			SerializeTextSettings(writer, axis.ParentChart.ParentWorkbook, axis.Font, axis.IsAutoTextRotation, axis.TextRotationAngle, axis.LabelTextAlign);
		}
	}

	private void SerializeTextSettings(XmlWriter writer, IWorkbook book, IOfficeFont font, bool isAutoTextRotation, int rotationAngle, AxisLabelAlignment labelTextAlign)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		writer.WriteStartElement("txPr", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		writer.WriteStartElement("bodyPr", "http://schemas.openxmlformats.org/drawingml/2006/main");
		if (!isAutoTextRotation)
		{
			writer.WriteAttributeString("rot", (rotationAngle * 60000).ToString());
		}
		writer.WriteEndElement();
		writer.WriteStartElement("p", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteStartElement("pPr", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("algn", labelTextAlign switch
		{
			AxisLabelAlignment.Justify => "just", 
			AxisLabelAlignment.Right => "r", 
			AxisLabelAlignment.Left => "l", 
			_ => "ctr", 
		});
		ChartSerializatorCommon.SerializeParagraphRunProperites(writer, font, "defRPr", book, 10.0);
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
	}
}
