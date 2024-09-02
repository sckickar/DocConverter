using System;
using System.Security;
using System.Xml;
using DocGen.Drawing.OfficeChart;
using DocGen.OfficeChart;

namespace DocGen.Drawing;

internal class AutoShapeSerializator
{
	private AnchorType anchorType;

	private string attribute;

	private string chartNameSpace = "http://schemas.openxmlformats.org/drawingml/2006/chartDrawing";

	private ShapeImplExt shape;

	private int resolution;

	internal AutoShapeSerializator(ShapeImplExt shape)
	{
		this.shape = shape;
		attribute = "xdr";
		anchorType = shape.AnchorType;
		resolution = shape.Worksheet.AppImplementation.GetdpiX();
	}

	private void SerializeAbsoluteAnchor(XmlWriter xmlTextWriter)
	{
		int num = shape.ClientAnchor.Width;
		int offsetValue = shape.ClientAnchor.Height;
		if (num < 911)
		{
			num = 900;
			offsetValue = 600;
		}
		num = Helper.ConvertOffsetToEMU(num, resolution);
		offsetValue = Helper.ConvertOffsetToEMU(offsetValue, resolution);
		xmlTextWriter.WriteStartElement("xdr", "pos", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		xmlTextWriter.WriteAttributeString("x", "0");
		xmlTextWriter.WriteAttributeString("y", "0");
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteStartElement("xdr", "ext", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		xmlTextWriter.WriteAttributeString("cx", Helper.ToString(num));
		xmlTextWriter.WriteAttributeString("cy", Helper.ToString(offsetValue));
		xmlTextWriter.WriteEndElement();
	}

	private void SerializeAnchorPosition(XmlWriter xmlTextWriter)
	{
		switch (anchorType)
		{
		case AnchorType.Absolute:
			SerializeAbsoluteAnchor(xmlTextWriter);
			break;
		case AnchorType.RelSize:
			SerializeRelSizeAnchor(xmlTextWriter);
			break;
		case AnchorType.OneCell:
			SerializeOneCellAnchor(xmlTextWriter);
			break;
		default:
			SerializeTwoCellAnchor(xmlTextWriter);
			break;
		}
	}

	private void SerializeClientData(XmlWriter xmlTextWriter)
	{
		xmlTextWriter.WriteStartElement("xdr", "clientData", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		xmlTextWriter.WriteEndElement();
	}

	[SecurityCritical]
	private void SerializeConnector(XmlWriter xmlTextWriter)
	{
		new ShapePropertiesSerializor(shape, attribute).Write(xmlTextWriter);
		new ShapeStyle(shape, attribute).Write(xmlTextWriter);
	}

	[SecurityCritical]
	private void SerializeGenralShapes(XmlWriter xmlTextWriter)
	{
		new ShapePropertiesSerializor(shape, attribute).Write(xmlTextWriter);
		new ShapeStyle(shape, attribute).Write(xmlTextWriter);
		new TextBody(shape, attribute).Write(xmlTextWriter);
	}

	private void SerializeGraphicFrame(XmlWriter xmlTextWriter)
	{
		throw new NotImplementedException();
	}

	private void SerializeGroupShapes(XmlWriter xmlTextWriter)
	{
		throw new NotImplementedException();
	}

	private void SerializeOneCellAnchor(XmlWriter xmlTextWriter)
	{
		int leftColumn = shape.ClientAnchor.LeftColumn;
		int leftColumnOffset = shape.ClientAnchor.LeftColumnOffset;
		leftColumnOffset = shape.ClientAnchor.CalculateColumnOffset(leftColumn, 0, leftColumn, leftColumnOffset);
		int topRow = shape.ClientAnchor.TopRow;
		int topRowOffset = shape.ClientAnchor.TopRowOffset;
		topRowOffset = shape.ClientAnchor.CalculateRowOffset(topRow, 0, topRow, topRowOffset);
		int width = shape.ClientAnchor.Width;
		int height = shape.ClientAnchor.Height;
		width = Helper.ConvertOffsetToEMU(width, resolution);
		height = Helper.ConvertOffsetToEMU(height, resolution);
		xmlTextWriter.WriteStartElement("xdr", "from", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		xmlTextWriter.WriteStartElement("xdr", "col", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		xmlTextWriter.WriteString(Helper.ToString(leftColumn));
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteStartElement("xdr", "colOff", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		xmlTextWriter.WriteString(Helper.ToString(Helper.ConvertOffsetToEMU(leftColumnOffset, resolution)));
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteStartElement("xdr", "row", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		xmlTextWriter.WriteString(Helper.ToString(topRow));
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteStartElement("xdr", "rowOff", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		xmlTextWriter.WriteString(Helper.ToString(Helper.ConvertOffsetToEMU(topRowOffset, resolution)));
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteStartElement("xdr", "ext", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		xmlTextWriter.WriteAttributeString("cx", Helper.ToString(width));
		xmlTextWriter.WriteAttributeString("cy", Helper.ToString(height));
		xmlTextWriter.WriteEndElement();
	}

	[SecurityCritical]
	private void SerializePicture(XmlWriter xmlTextWriter)
	{
		new ShapePropertiesSerializor(shape, attribute).Write(xmlTextWriter);
		new ShapeStyle(shape, attribute).Write(xmlTextWriter);
	}

	private void SerializeRelSizeAnchor(XmlWriter xmlTextWriter)
	{
		throw new NotImplementedException();
	}

	[SecurityCritical]
	private void SerializeShapeChoices(XmlWriter xmlTextWriter)
	{
		ExcelAutoShapeType shapeType = shape.ShapeType;
		xmlTextWriter.WriteStartElement(attribute, shapeType.ToString(), "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		if (shapeType != 0)
		{
			WriteShapeAttributes(xmlTextWriter, isGeneralShape: false);
		}
		else
		{
			WriteShapeAttributes(xmlTextWriter, isGeneralShape: true);
		}
		SerializeShapeNonVisual(xmlTextWriter);
		switch (shapeType)
		{
		case ExcelAutoShapeType.sp:
			SerializeGenralShapes(xmlTextWriter);
			break;
		case ExcelAutoShapeType.grpSp:
			SerializeGroupShapes(xmlTextWriter);
			break;
		case ExcelAutoShapeType.graphicFrame:
			SerializeGraphicFrame(xmlTextWriter);
			break;
		case ExcelAutoShapeType.cxnSp:
			SerializeConnector(xmlTextWriter);
			break;
		case ExcelAutoShapeType.pic:
			SerializePicture(xmlTextWriter);
			break;
		}
		xmlTextWriter.WriteEndElement();
	}

	private void SerializeShapeNonVisual(XmlWriter xmlTextWriter)
	{
		new ShapeNonVisual(shape, attribute).Write(xmlTextWriter);
	}

	private void SerializeTwoCellAnchor(XmlWriter xmlTextWriter)
	{
		int leftColumn = shape.ClientAnchor.LeftColumn;
		int leftColumnOffset = shape.ClientAnchor.LeftColumnOffset;
		leftColumnOffset = shape.ClientAnchor.CalculateColumnOffset(leftColumn, 0, leftColumn, leftColumnOffset);
		int topRow = shape.ClientAnchor.TopRow;
		int topRowOffset = shape.ClientAnchor.TopRowOffset;
		topRowOffset = shape.ClientAnchor.CalculateRowOffset(topRow, 0, topRow, topRowOffset);
		int rightColumn = shape.ClientAnchor.RightColumn;
		int rightColumnOffset = shape.ClientAnchor.RightColumnOffset;
		rightColumnOffset = shape.ClientAnchor.CalculateColumnOffset(rightColumn, 0, rightColumn, rightColumnOffset);
		int bottomRow = shape.ClientAnchor.BottomRow;
		int bottomRowOffset = shape.ClientAnchor.BottomRowOffset;
		bottomRowOffset = shape.ClientAnchor.CalculateRowOffset(bottomRow, 0, bottomRow, bottomRowOffset);
		xmlTextWriter.WriteStartElement("xdr", "from", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		xmlTextWriter.WriteStartElement("xdr", "col", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		xmlTextWriter.WriteString(Helper.ToString(leftColumn));
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteStartElement("xdr", "colOff", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		xmlTextWriter.WriteString(Helper.ToString(Helper.ConvertOffsetToEMU(leftColumnOffset, resolution)));
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteStartElement("xdr", "row", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		xmlTextWriter.WriteString(Helper.ToString(topRow));
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteStartElement("xdr", "rowOff", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		xmlTextWriter.WriteString(Helper.ToString(Helper.ConvertOffsetToEMU(topRowOffset, resolution)));
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteStartElement("xdr", "to", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		xmlTextWriter.WriteStartElement("xdr", "col", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		xmlTextWriter.WriteString(Helper.ToString(rightColumn));
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteStartElement("xdr", "colOff", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		xmlTextWriter.WriteString(Helper.ToString(Helper.ConvertOffsetToEMU(rightColumnOffset, resolution)));
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteStartElement("xdr", "row", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		xmlTextWriter.WriteString(Helper.ToString(bottomRow));
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteStartElement("xdr", "rowOff", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		xmlTextWriter.WriteString(Helper.ToString(Helper.ConvertOffsetToEMU(bottomRowOffset, resolution)));
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteEndElement();
	}

	[SecurityCritical]
	internal void Write(XmlWriter xmlTextWriter)
	{
		switch (anchorType)
		{
		case AnchorType.Absolute:
			xmlTextWriter.WriteStartElement("xdr", "absoluteAnchor", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
			break;
		case AnchorType.RelSize:
			xmlTextWriter.WriteStartElement("cdr:relSizeAnchor");
			xmlTextWriter.WriteAttributeString("xmlns:cdr", chartNameSpace);
			break;
		case AnchorType.OneCell:
			xmlTextWriter.WriteStartElement("xdr", "oneCellAnchor", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
			break;
		default:
			xmlTextWriter.WriteStartElement("xdr", "twoCellAnchor", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
			break;
		}
		if (shape.ClientAnchor.Placement != PlacementType.MoveAndSize)
		{
			string placementType = Helper.GetPlacementType(shape.ClientAnchor.Placement);
			xmlTextWriter.WriteAttributeString("editAs", placementType);
		}
		SerializeAnchorPosition(xmlTextWriter);
		SerializeShapeChoices(xmlTextWriter);
		SerializeClientData(xmlTextWriter);
		xmlTextWriter.WriteEndElement();
	}

	private void WriteShapeAttributes(XmlWriter xmlTextWriter, bool isGeneralShape)
	{
		string macro = shape.Macro;
		if (macro != null)
		{
			xmlTextWriter.WriteAttributeString("macro", macro);
		}
		if (shape.Published)
		{
			xmlTextWriter.WriteAttributeString("fPublished", "1");
		}
		if (isGeneralShape)
		{
			string textLink = shape.TextLink;
			if (textLink != null)
			{
				xmlTextWriter.WriteAttributeString("textlink", textLink);
			}
		}
	}
}
