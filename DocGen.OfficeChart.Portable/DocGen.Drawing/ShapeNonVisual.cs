using System.Xml;
using DocGen.OfficeChart;

namespace DocGen.Drawing;

internal class ShapeNonVisual
{
	private string attribute;

	private string drawingPros = "cNvPr";

	private string drawingShapeProps;

	private string nonVisual;

	private ShapeImplExt shape;

	public ShapeNonVisual(ShapeImplExt shape, string attribute)
	{
		this.shape = shape;
		this.attribute = attribute;
	}

	private void InitializeShapeType(ExcelAutoShapeType shapeType)
	{
		switch (shapeType)
		{
		case ExcelAutoShapeType.sp:
			nonVisual = "nvSpPr";
			drawingShapeProps = "cNvSpPr";
			break;
		case ExcelAutoShapeType.grpSp:
			nonVisual = "nvGrpSpPr";
			drawingShapeProps = "cNvGrpSpPr";
			break;
		case ExcelAutoShapeType.graphicFrame:
			nonVisual = "nvGraphicFramePr";
			drawingShapeProps = "cNvGraphicFramePr";
			break;
		case ExcelAutoShapeType.cxnSp:
			nonVisual = "nvCxnSpPr";
			drawingShapeProps = "cNvCxnSpPr";
			break;
		case ExcelAutoShapeType.pic:
			nonVisual = "nvPicPr";
			drawingShapeProps = "cNvPicPr";
			break;
		}
	}

	private void SerializeNonVisualDrawingProps(XmlWriter xmlTextWriter)
	{
		int shapeID = shape.ShapeID;
		xmlTextWriter.WriteStartElement(attribute, drawingPros, "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		xmlTextWriter.WriteAttributeString("id", shapeID.ToString());
		if (shape.Name != null && shape.Name.Length > 0)
		{
			xmlTextWriter.WriteAttributeString("name", shape.Name);
		}
		else
		{
			xmlTextWriter.WriteAttributeString("name", $"{shape.AutoShapeType.ToString()} {shapeID}");
		}
		if (shape.Description != null && shape.Description.Length > 0)
		{
			xmlTextWriter.WriteAttributeString("descr", shape.Description);
		}
		if (shape.IsHidden)
		{
			xmlTextWriter.WriteAttributeString("hidden", "1");
		}
		if (shape.Title != null && shape.Title.Length > 0)
		{
			xmlTextWriter.WriteAttributeString("title", shape.Title);
		}
		xmlTextWriter.WriteEndElement();
	}

	private void SerializeNonVisualDrawingShapeProps(XmlWriter xmlTextWriter)
	{
		xmlTextWriter.WriteStartElement(attribute, drawingShapeProps, "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		xmlTextWriter.WriteEndElement();
	}

	internal void Write(XmlWriter xmlTextWriter)
	{
		ExcelAutoShapeType shapeType = shape.ShapeType;
		InitializeShapeType(shapeType);
		xmlTextWriter.WriteStartElement(attribute, nonVisual, "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		SerializeNonVisualDrawingProps(xmlTextWriter);
		SerializeNonVisualDrawingShapeProps(xmlTextWriter);
		xmlTextWriter.WriteEndElement();
	}
}
