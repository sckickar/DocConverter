using System.IO;
using System.Security;
using System.Xml;
using DocGen.OfficeChart;
using DocGen.OfficeChart.Implementation.XmlReaders.Shapes;
using DocGen.OfficeChart.Implementation.XmlSerialization;
using DocGen.OfficeChart.Implementation.XmlSerialization.Charts;
using DocGen.OfficeChart.Implementation.XmlSerialization.Shapes;
using DocGen.OfficeChart.Interfaces;

namespace DocGen.Drawing;

internal class ShapePropertiesSerializor
{
	private string attribute;

	private ShapeImplExt shape;

	private int dpiX;

	private int dpiY;

	public ShapePropertiesSerializor(ShapeImplExt shape, string attribute)
	{
		this.shape = shape;
		this.attribute = attribute;
		dpiX = shape.Worksheet.AppImplementation.GetdpiX();
		dpiY = shape.Worksheet.AppImplementation.GetdpiY();
	}

	private void SerializeEffectProperties(XmlWriter xmlTextWriter)
	{
		if (shape.PreservedElements.TryGetValue("Effect", out var value) && value != null && value.Length > 0)
		{
			value.Position = 0L;
			ShapeParser.WriteNodeFromStream(xmlTextWriter, value);
		}
	}

	[SecurityCritical]
	private void SerializeFillProperties(XmlWriter xmlTextWriter)
	{
		Stream value;
		if (!shape.Fill.Visible)
		{
			xmlTextWriter.WriteStartElement("a", "noFill", "http://schemas.openxmlformats.org/drawingml/2006/main");
			xmlTextWriter.WriteEndElement();
		}
		else if (shape.Logger.GetPreservedItem(PreservedFlag.Fill))
		{
			IInternalFill fill = shape.Fill;
			FileDataHolder parentHolder = shape.Worksheet.DataHolder.ParentHolder;
			ChartSerializatorCommon.SerializeFill(xmlTextWriter, fill, parentHolder, shape.Relations);
		}
		else if (shape.PreservedElements.TryGetValue("Fill", out value) && value != null && value.Length > 0)
		{
			value.Position = 0L;
			ShapeParser.WriteNodeFromStream(xmlTextWriter, value);
		}
	}

	private void SerializeGemoerty(XmlWriter xmlTextWriter)
	{
		xmlTextWriter.WriteStartElement("a", "prstGeom", "http://schemas.openxmlformats.org/drawingml/2006/main");
		AutoShapeConstant autoShapeConstant = AutoShapeHelper.GetAutoShapeConstant(shape.AutoShapeType);
		if (autoShapeConstant != AutoShapeConstant.Index_187)
		{
			xmlTextWriter.WriteAttributeString("prst", AutoShapeHelper.GetAutoShapeString(autoShapeConstant));
		}
		else
		{
			xmlTextWriter.WriteAttributeString("prst", "rect");
		}
		if (shape.PreservedElements.TryGetValue("avLst", out var value))
		{
			if (value != null && value.Length > 0)
			{
				value.Position = 0L;
				ShapeParser.WriteNodeFromStream(xmlTextWriter, value);
			}
		}
		else
		{
			xmlTextWriter.WriteStartElement("a", "avLst", "http://schemas.openxmlformats.org/drawingml/2006/main");
			xmlTextWriter.WriteEndElement();
		}
		xmlTextWriter.WriteEndElement();
	}

	private void SerializeLineProperties(XmlWriter xmlTextWriter)
	{
		Stream value;
		if (shape.Logger.GetPreservedItem(PreservedFlag.Line))
		{
			IShapeLineFormat line = shape.Line;
			DrawingShapeSerializator.SerializeLineSettings(xmlTextWriter, line, shape.Worksheet.Workbook);
		}
		else if (shape.PreservedElements.TryGetValue("Line", out value) && value != null && value.Length > 0)
		{
			value.Position = 0L;
			ShapeParser.WriteNodeFromStream(xmlTextWriter, value);
		}
	}

	private void SerializeScence3d(XmlWriter xmlTextWriter)
	{
		if (shape.PreservedElements.TryGetValue("Scene3d", out var value) && value != null && value.Length > 0)
		{
			value.Position = 0L;
			ShapeParser.WriteNodeFromStream(xmlTextWriter, value);
		}
	}

	private void SerializeShape3d(XmlWriter xmlTextWriter)
	{
		if (shape.PreservedElements.TryGetValue("Sp3d", out var value) && value != null && value.Length > 0)
		{
			value.Position = 0L;
			ShapeParser.WriteNodeFromStream(xmlTextWriter, value);
		}
	}

	private void SerializeTransformation(XmlWriter xmlTextWriter)
	{
		xmlTextWriter.WriteStartElement("a", "xfrm", "http://schemas.openxmlformats.org/drawingml/2006/main");
		if (shape.Rotation != 0.0 && shape.Rotation > 0.0)
		{
			xmlTextWriter.WriteAttributeString("rot", Helper.ToString(shape.Rotation * 60000.0));
		}
		if (shape.FlipHorizontal)
		{
			xmlTextWriter.WriteAttributeString("flipH", "1");
		}
		if (shape.FlipVertical)
		{
			xmlTextWriter.WriteAttributeString("flipV", "1");
		}
		xmlTextWriter.WriteStartElement("a", "off", "http://schemas.openxmlformats.org/drawingml/2006/main");
		xmlTextWriter.WriteAttributeString("x", "0");
		xmlTextWriter.WriteAttributeString("y", "0");
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteStartElement("a", "ext", "http://schemas.openxmlformats.org/drawingml/2006/main");
		xmlTextWriter.WriteAttributeString("cx", "0");
		xmlTextWriter.WriteAttributeString("cy", "0");
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteEndElement();
	}

	[SecurityCritical]
	internal void Write(XmlWriter xmlTextWriter)
	{
		xmlTextWriter.WriteStartElement(attribute, "spPr", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		SerializeTransformation(xmlTextWriter);
		SerializeGemoerty(xmlTextWriter);
		SerializeFillProperties(xmlTextWriter);
		SerializeLineProperties(xmlTextWriter);
		SerializeEffectProperties(xmlTextWriter);
		SerializeScence3d(xmlTextWriter);
		SerializeShape3d(xmlTextWriter);
		xmlTextWriter.WriteEndElement();
	}
}
