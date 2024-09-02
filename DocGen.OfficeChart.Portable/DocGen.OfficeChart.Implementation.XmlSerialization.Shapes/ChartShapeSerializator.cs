using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Xml;
using DocGen.Compression;
using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Implementation.XmlReaders.Shapes;
using DocGen.OfficeChart.Implementation.XmlSerialization.Charts;

namespace DocGen.OfficeChart.Implementation.XmlSerialization.Shapes;

internal class ChartShapeSerializator : DrawingShapeSerializator
{
	public const string ChartItemPath = "/xl/charts/chart{0}.xml";

	internal const string ChartExItemPath = "/xl/charts/chartEx{0}.xml";

	internal const string StyleItemPath = "/xl/charts/style{0}.xml";

	internal const string ColorsItemPath = "/xl/charts/colors{0}.xml";

	public override void Serialize(XmlWriter writer, ShapeImpl shape, WorksheetDataHolder holder, RelationCollection vmlRelations)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (shape == null)
		{
			throw new ArgumentNullException("shape");
		}
		if (holder == null)
		{
			throw new ArgumentNullException("holder");
		}
		ChartShapeImpl chartShapeImpl = (ChartShapeImpl)shape;
		ChartImpl chartObject = chartShapeImpl.ChartObject;
		if (chartObject.Relations.Count != 0)
		{
			foreach (KeyValuePair<string, Relation> relation in chartObject.Relations)
			{
				if (relation.Value.Target.Contains("drawing"))
				{
					chartObject.Relations.Remove(relation.Key);
					break;
				}
			}
		}
		if (chartObject.DataHolder == null && holder != null)
		{
			chartObject.DataHolder = holder;
		}
		string chartFileName;
		string strRelationId = SerializeChartFile(holder, chartObject, out chartFileName);
		if (!shape.IsAbsoluteAnchor)
		{
			writer.WriteStartElement("twoCellAnchor", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
			SerializeAnchorPoint(writer, "from", shape.LeftColumn, shape.LeftColumnOffset, shape.TopRow, shape.TopRowOffset, shape.Worksheet, "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
			SerializeAnchorPoint(writer, "to", shape.RightColumn, shape.RightColumnOffset, shape.BottomRow, shape.BottomRowOffset, shape.Worksheet, "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
			SerializeChartProperties(writer, chartShapeImpl, strRelationId, holder, isGroupShape: false);
			writer.WriteEndElement();
		}
		else
		{
			ChartSerializator.SerializeAbsoluteAnchorChart(writer, chartObject, strRelationId, isForChartSheet: true);
		}
		holder.SerializeRelations(chartObject.Relations, chartFileName.Substring(1), holder);
	}

	[SecurityCritical]
	internal string SerializeChartFile(WorksheetDataHolder holder, ChartImpl chart, out string chartFileName)
	{
		if (holder == null)
		{
			throw new ArgumentNullException("holder");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		MemoryStream memoryStream = new MemoryStream();
		StreamWriter streamWriter = new StreamWriter(memoryStream);
		XmlWriter xmlWriter = UtilityMethods.CreateWriter(streamWriter);
		bool num = ChartImpl.IsChartExSerieType(chart.ChartType);
		if (num)
		{
			chartFileName = GetChartExFileName(holder, chart);
		}
		else
		{
			chartFileName = GetChartFileName(holder, chart);
		}
		FileDataHolder parentHolder = holder.ParentHolder;
		if (chart.DataHolder == null)
		{
			parentHolder.CreateDataHolder(chart, chartFileName);
		}
		if (num)
		{
			new ChartExSerializator().SerializeChartEx(xmlWriter, chart);
		}
		else
		{
			new ChartSerializator().SerializeChart(xmlWriter, chart, chartFileName);
		}
		xmlWriter.Flush();
		streamWriter.Flush();
		string itemName = UtilityMethods.RemoveFirstCharUnsafe(chartFileName);
		parentHolder.Archive.UpdateItem(itemName, memoryStream, controlStream: true, DocGen.Compression.FileAttributes.Archive);
		string text = holder.DrawingsRelations.GenerateRelationId();
		holder.DrawingsRelations[text] = new Relation(chartFileName, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/chart");
		parentHolder.OverriddenContentTypes[chartFileName] = "application/vnd.openxmlformats-officedocument.drawingml.chart+xml";
		return text;
	}

	internal static string GetChartExFileName(WorksheetDataHolder holder, ChartImpl chart)
	{
		if (holder == null)
		{
			throw new ArgumentNullException("holder");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		return $"/xl/charts/chartEx{++holder.ParentHolder.LastChartExIndex}.xml";
	}

	public static string GetChartFileName(WorksheetDataHolder holder, ChartImpl chart)
	{
		if (holder == null)
		{
			throw new ArgumentNullException("holder");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		return $"/xl/charts/chart{++holder.ParentHolder.LastChartIndex}.xml";
	}

	internal void SerializeChartProperties(XmlWriter writer, ChartShapeImpl chart, string strRelationId, WorksheetDataHolder holder, bool isGroupShape)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (strRelationId == null || strRelationId.Length == 0)
		{
			throw new ArgumentOutOfRangeException("strRelationId");
		}
		if (holder == null)
		{
			throw new ArgumentNullException("holder");
		}
		bool flag = ChartImpl.IsChartExSerieType(chart.ChartType);
		if (flag)
		{
			writer.WriteStartElement("mc", "AlternateContent", "http://schemas.openxmlformats.org/markup-compatibility/2006");
			writer.WriteAttributeString("xmlns", "mc", null, "http://schemas.openxmlformats.org/markup-compatibility/2006");
			writer.WriteStartElement("mc", "Choice", "http://schemas.openxmlformats.org/markup-compatibility/2006");
			writer.WriteAttributeString("xmlns", "cx1", null, "http://schemas.microsoft.com/office/drawing/2015/9/8/chartex");
			writer.WriteAttributeString("Requires", "cx1");
		}
		writer.WriteStartElement("graphicFrame", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		writer.WriteAttributeString("macro", string.Empty);
		SerializeNonVisualGraphicFrameProperties(writer, chart, holder);
		if (!isGroupShape || flag)
		{
			DrawingShapeSerializator.SerializeForm(writer, "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing", "http://schemas.openxmlformats.org/drawingml/2006/main", 0, 0, 0, 0);
		}
		else
		{
			DrawingShapeSerializator.SerializeForm(writer, "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing", "http://schemas.openxmlformats.org/drawingml/2006/main", chart.OffsetX, chart.OffsetY, chart.ExtentsX, chart.ExtentsY);
		}
		SerializeGraphics(writer, chart, strRelationId, flag);
		writer.WriteEndElement();
		if (!isGroupShape && !flag)
		{
			writer.WriteElementString("clientData", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing", string.Empty);
		}
		if (flag)
		{
			writer.WriteEndElement();
			writer.WriteStartElement("mc", "Fallback", "http://schemas.openxmlformats.org/markup-compatibility/2006");
			holder.SerializeChartExFallBackShapeContent(writer, isChartSheet: false);
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteElementString("clientData", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing", string.Empty);
		}
	}

	private void SerializeGraphics(XmlWriter writer, ChartShapeImpl chart, string strRelationId, bool isChartEx)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (strRelationId == null || strRelationId.Length == 0)
		{
			throw new ArgumentOutOfRangeException("strRelationId");
		}
		writer.WriteStartElement("graphic", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteStartElement("graphicData", "http://schemas.openxmlformats.org/drawingml/2006/main");
		if (isChartEx)
		{
			writer.WriteAttributeString("uri", "http://schemas.microsoft.com/office/drawing/2014/chartex");
			writer.WriteStartElement("cx", "chart", "http://schemas.microsoft.com/office/drawing/2014/chartex");
		}
		else
		{
			writer.WriteAttributeString("uri", "http://schemas.openxmlformats.org/drawingml/2006/chart");
			writer.WriteStartElement("c", "chart", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		}
		writer.WriteAttributeString("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships", strRelationId);
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	internal void SerializeSlicerGraphics(XmlWriter writer, ChartShapeImpl shape)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (shape == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("graphic", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteStartElement("graphicData", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("uri", "http://schemas.microsoft.com/office/drawing/2010/slicer");
		shape.GraphicFrameStream.Position = 0L;
		ShapeParser.WriteNodeFromStream(writer, shape.GraphicFrameStream);
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	internal void SerializeNonVisualGraphicFrameProperties(XmlWriter writer, ChartShapeImpl chart, WorksheetDataHolder holder)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		writer.WriteStartElement("nvGraphicFramePr", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		SerializeNVCanvasProperties(writer, chart, holder, "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		writer.WriteElementString("cNvGraphicFramePr", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing", string.Empty);
		writer.WriteEndElement();
	}
}
