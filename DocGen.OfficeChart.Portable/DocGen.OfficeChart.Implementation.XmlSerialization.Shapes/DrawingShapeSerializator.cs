using System;
using System.IO;
using System.Security;
using System.Xml;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Implementation.XmlSerialization.Charts;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

namespace DocGen.OfficeChart.Implementation.XmlSerialization.Shapes;

internal class DrawingShapeSerializator : ShapeSerializator
{
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
		Stream xmlDataStream = shape.XmlDataStream;
		if (xmlDataStream != null && xmlDataStream.Length > 0)
		{
			MsofbtClientAnchor clientAnchor = shape.ClientAnchor;
			if (shape.EnableAlternateContent)
			{
				Excel2007Serializator.WriteAlternateContentHeader(writer);
			}
			if (clientAnchor.OneCellAnchor)
			{
				writer.WriteStartElement("oneCellAnchor", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
			}
			else
			{
				writer.WriteStartElement("twoCellAnchor", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
			}
			SerializeAnchorPoint(writer, "from", shape.LeftColumn, shape.LeftColumnOffset, shape.TopRow, shape.TopRowOffset, shape.Worksheet, "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
			if (clientAnchor.OneCellAnchor)
			{
				writer.WriteStartElement("ext", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
				writer.WriteAttributeString("cx", ((int)ApplicationImpl.ConvertFromPixel(shape.Width, MeasureUnits.EMU)).ToString());
				writer.WriteAttributeString("cy", ((int)ApplicationImpl.ConvertFromPixel(shape.Height, MeasureUnits.EMU)).ToString());
				writer.WriteEndElement();
			}
			else
			{
				SerializeAnchorPoint(writer, "to", shape.RightColumn, shape.RightColumnOffset, shape.BottomRow, shape.BottomRowOffset, shape.Worksheet, "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
			}
			xmlDataStream.Position = 0L;
			XmlReader reader = UtilityMethods.CreateReader(xmlDataStream);
			writer.WriteNode(reader, defattr: false);
			writer.WriteElementString("clientData", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing", string.Empty);
			writer.WriteEndElement();
			if (shape.EnableAlternateContent)
			{
				Excel2007Serializator.WriteAlternateContentFooter(writer);
			}
		}
	}

	public override void SerializeShapeType(XmlWriter writer, Type shapeType)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public static string GetEditAsValue(ShapeImpl shape)
	{
		if (shape == null)
		{
			throw new ArgumentNullException("shape");
		}
		if (shape.IsMoveWithCell)
		{
			if (shape.IsSizeWithCell)
			{
				return "twoCell";
			}
			return "oneCell";
		}
		return "absolute";
	}

	internal void SerializeAnchorPoint(XmlWriter writer, string tagName, int column, int columnOffset, int row, int rowOffset, WorksheetBaseImpl sheet, string drawingsNamespace)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (tagName == null || tagName.Length == 0)
		{
			throw new ArgumentOutOfRangeException("tagName");
		}
		if (sheet is WorksheetImpl)
		{
			SerializeColRowAnchor(writer, tagName, column, columnOffset, row, rowOffset, sheet, drawingsNamespace);
		}
		else
		{
			SerializeXYAnchor(writer, tagName, column, row, drawingsNamespace);
		}
	}

	private void SerializeColRowAnchor(XmlWriter writer, string tagName, int column, int columnOffset, int row, int rowOffset, WorksheetBaseImpl sheet, string drawingsNamespace)
	{
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (tagName == null || tagName.Length == 0)
		{
			throw new ArgumentOutOfRangeException("tagName");
		}
		WorksheetImpl worksheetImpl = sheet as WorksheetImpl;
		double num = 0.0;
		double num2 = 0.0;
		num = worksheetImpl?.GetColumnWidthInPixels(column) ?? 1;
		num = num * (double)columnOffset / 1024.0;
		num = Math.Round(ApplicationImpl.ConvertFromPixel(num, MeasureUnits.EMU));
		column--;
		num2 = worksheetImpl?.GetRowHeightInPixels(row) ?? 1;
		num2 = Math.Round(num2 * (double)rowOffset / 256.0, 1);
		num2 = (int)ApplicationImpl.ConvertFromPixel(num2, MeasureUnits.EMU);
		row--;
		writer.WriteStartElement(tagName, drawingsNamespace);
		writer.WriteElementString("col", drawingsNamespace, column.ToString());
		writer.WriteElementString("colOff", drawingsNamespace, ((int)num).ToString());
		writer.WriteElementString("row", drawingsNamespace, row.ToString());
		writer.WriteElementString("rowOff", drawingsNamespace, ((int)num2).ToString());
		writer.WriteEndElement();
	}

	private void SerializeXYAnchor(XmlWriter writer, string tagName, int column, int row, string drawingsNamespace)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (tagName == null || tagName.Length == 0)
		{
			throw new ArgumentOutOfRangeException("tagName");
		}
		writer.WriteStartElement(tagName, drawingsNamespace);
		if (column > 1000)
		{
			column = 1000;
		}
		string coordinateValue = GetCoordinateValue(column);
		writer.WriteElementString("x", drawingsNamespace, coordinateValue);
		if (row > 1000)
		{
			row = 1000;
		}
		coordinateValue = GetCoordinateValue(row);
		writer.WriteElementString("y", drawingsNamespace, coordinateValue);
		writer.WriteEndElement();
	}

	private string GetCoordinateValue(int coordinate)
	{
		return XmlConvert.ToString((double)coordinate / 1000.0);
	}

	public static void SerializeForm(XmlWriter writer, string xmlOuterNamespace, string xmlInnerNamespace, int x, int y, int cx, int cy)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		writer.WriteStartElement("xfrm", xmlOuterNamespace);
		writer.WriteStartElement("off", xmlInnerNamespace);
		writer.WriteAttributeString("x", x.ToString());
		writer.WriteAttributeString("y", y.ToString());
		writer.WriteEndElement();
		writer.WriteStartElement("ext", xmlInnerNamespace);
		writer.WriteAttributeString("cx", cx.ToString());
		writer.WriteAttributeString("cy", cy.ToString());
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	public static void SerializeForm(XmlWriter writer, string xmlOuterNamespace, string xmlInnerNamespace, int x, int y, int cx, int cy, IShape shape)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		writer.WriteStartElement("xfrm", xmlOuterNamespace);
		if (shape.ShapeRotation != 0)
		{
			writer.WriteAttributeString("rot", (shape.ShapeRotation * 60000).ToString());
		}
		writer.WriteStartElement("off", xmlInnerNamespace);
		writer.WriteAttributeString("x", x.ToString());
		writer.WriteAttributeString("y", y.ToString());
		writer.WriteEndElement();
		writer.WriteStartElement("ext", xmlInnerNamespace);
		writer.WriteAttributeString("cx", cx.ToString());
		writer.WriteAttributeString("cy", cy.ToString());
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	protected void SerializeNVCanvasProperties(XmlWriter writer, ShapeImpl shape, WorksheetDataHolder holder, string drawingsNamespace)
	{
		writer.WriteStartElement("cNvPr", drawingsNamespace);
		writer.WriteAttributeString("id", shape.ShapeId.ToString());
		string name = shape.Name;
		writer.WriteAttributeString("name", name);
		string alternativeText = shape.AlternativeText;
		if (alternativeText != null && alternativeText.Length > 0)
		{
			writer.WriteAttributeString("descr", alternativeText);
		}
		if (!shape.IsShapeVisible)
		{
			writer.WriteAttributeString("hidden", "1");
		}
		if (shape.IsHyperlink)
		{
			writer.WriteStartElement("hlinkClick", "http://schemas.openxmlformats.org/drawingml/2006/main");
			string value = holder.DrawingsRelations.Add(shape.ImageRelation);
			writer.WriteAttributeString("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships", value);
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	protected void SerializePresetGeometry(XmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		writer.WriteStartElement("prstGeom", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("prst", "rect");
		writer.WriteStartElement("avLst", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	[SecurityCritical]
	protected void SerializeFill(XmlWriter writer, ShapeImpl shape, FileDataHolder holder, RelationCollection relations)
	{
		if (shape.HasFill)
		{
			IInternalFill internalFill = (IInternalFill)shape.Fill;
			if (internalFill.Visible)
			{
				ChartSerializatorCommon.SerializeFill(writer, internalFill, holder, relations);
			}
			else
			{
				writer.WriteStartElement("noFill", "http://schemas.openxmlformats.org/drawingml/2006/main");
				writer.WriteEndElement();
			}
		}
		if (shape.HasLineFormat)
		{
			IShapeLineFormat line = shape.Line;
			SerializeLineSettings(writer, line, holder.Workbook);
		}
	}

	internal static void SerializeLineSettings(XmlWriter writer, IShapeLineFormat line, IWorkbook book)
	{
		writer.WriteStartElement("ln", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("w", ((int)(line.Weight * 12700.0)).ToString());
		writer.WriteAttributeString("cmpd", ((Excel2007ShapeLineStyle)line.Style).ToString());
		if (line.Weight >= 0.0 && line.Visible)
		{
			ChartSerializatorCommon.SerializeSolidFill(writer, line.ForeColor, isAutoColor: false, book, 1.0 - line.Transparency);
		}
		else
		{
			writer.WriteElementString("noFill", "http://schemas.openxmlformats.org/drawingml/2006/main", string.Empty);
		}
		ShapeLineFormatImpl shapeLineFormatImpl = line as ShapeLineFormatImpl;
		if (shapeLineFormatImpl.IsRound)
		{
			writer.WriteElementString("round", "http://schemas.openxmlformats.org/drawingml/2006/main", string.Empty);
		}
		if (line.DashStyle != 0)
		{
			string text = ((book as WorksheetImpl).Application as ApplicationImpl).StringEnum.LineDashTypeEnumToXml[line.DashStyle];
			if (text != null && text.Length != 0)
			{
				ChartSerializatorCommon.SerializeValueTag(writer, "prstDash", "http://schemas.openxmlformats.org/drawingml/2006/main", text);
			}
		}
		SerializeArrowProperties(writer, shapeLineFormatImpl, isHead: true);
		SerializeArrowProperties(writer, shapeLineFormatImpl, isHead: false);
		writer.WriteEndElement();
	}

	private static void SerializeArrowProperties(XmlWriter writer, ShapeLineFormatImpl line, bool isHead)
	{
		string text = null;
		OfficeShapeArrowStyle obj;
		OfficeShapeArrowWidth obj2;
		OfficeShapeArrowLength obj3;
		if (isHead)
		{
			text = "headEnd";
			obj = line.BeginArrowHeadStyle;
			obj2 = line.BeginArrowheadWidth;
			obj3 = line.BeginArrowheadLength;
		}
		else
		{
			text = "tailEnd";
			obj = line.EndArrowHeadStyle;
			obj2 = line.EndArrowheadWidth;
			obj3 = line.EndArrowheadLength;
		}
		writer.WriteStartElement(text, "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("type", GetArrowStyle(obj));
		writer.WriteAttributeString("w", GetArrowWidth(obj2));
		writer.WriteAttributeString("len", GetArrowLength(obj3));
		writer.WriteEndElement();
	}

	private static string GetArrowLength(OfficeShapeArrowLength obj4)
	{
		return obj4 switch
		{
			OfficeShapeArrowLength.ArrowHeadShort => "sm", 
			OfficeShapeArrowLength.ArrowHeadMedium => "med", 
			OfficeShapeArrowLength.ArrowHeadLong => "lg", 
			_ => "med", 
		};
	}

	private static string GetArrowWidth(OfficeShapeArrowWidth obj3)
	{
		return obj3 switch
		{
			OfficeShapeArrowWidth.ArrowHeadNarrow => "sm", 
			OfficeShapeArrowWidth.ArrowHeadMedium => "med", 
			OfficeShapeArrowWidth.ArrowHeadWide => "lg", 
			_ => "med", 
		};
	}

	private static string GetArrowStyle(OfficeShapeArrowStyle obj2)
	{
		return obj2 switch
		{
			OfficeShapeArrowStyle.LineNoArrow => "none", 
			OfficeShapeArrowStyle.LineArrow => "triangle", 
			OfficeShapeArrowStyle.LineArrowStealth => "stealth", 
			OfficeShapeArrowStyle.LineArrowDiamond => "diamond", 
			OfficeShapeArrowStyle.LineArrowOval => "oval", 
			OfficeShapeArrowStyle.LineArrowOpen => "arrow", 
			_ => "none", 
		};
	}
}
