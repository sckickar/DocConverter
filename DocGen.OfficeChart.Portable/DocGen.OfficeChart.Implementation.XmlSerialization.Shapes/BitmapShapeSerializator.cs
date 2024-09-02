using System;
using System.Xml;
using DocGen.OfficeChart.Implementation.Shapes;

namespace DocGen.OfficeChart.Implementation.XmlSerialization.Shapes;

internal class BitmapShapeSerializator : DrawingShapeSerializator
{
	public override void Serialize(XmlWriter writer, ShapeImpl shape, WorksheetDataHolder holder, RelationCollection vmlRelations)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (!(shape is BitmapShapeImpl bitmapShapeImpl))
		{
			throw new ArgumentOutOfRangeException("picture");
		}
		_ = holder.ParentHolder;
		string relationId = SerializePictureFile(holder, bitmapShapeImpl, vmlRelations);
		bool num = !(shape.Worksheet is WorksheetImpl);
		string localName;
		string text;
		if (num)
		{
			localName = "relSizeAnchor";
			text = "http://schemas.openxmlformats.org/drawingml/2006/chartDrawing";
		}
		else
		{
			localName = "twoCellAnchor";
			text = "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing";
		}
		writer.WriteStartElement(localName, text);
		if (!num)
		{
			writer.WriteAttributeString("editAs", DrawingShapeSerializator.GetEditAsValue(bitmapShapeImpl));
		}
		SerializeAnchorPoint(writer, "from", shape.LeftColumn, shape.LeftColumnOffset, shape.TopRow, shape.TopRowOffset, shape.Worksheet, text);
		SerializeAnchorPoint(writer, "to", shape.RightColumn, shape.RightColumnOffset, shape.BottomRow, shape.BottomRowOffset, shape.Worksheet, text);
		SerializePicture(writer, bitmapShapeImpl, relationId, holder, text);
		writer.WriteEndElement();
	}

	private void SerializePicture(XmlWriter writer, BitmapShapeImpl picture, string relationId, WorksheetDataHolder holder, string mainNamespace)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (picture == null)
		{
			throw new ArgumentNullException("picture");
		}
		if (relationId == null || relationId.Length == 0)
		{
			throw new ArgumentOutOfRangeException("relationId");
		}
		writer.WriteStartElement("pic", mainNamespace);
		string macro = picture.Macro;
		if (macro != null)
		{
			writer.WriteAttributeString("macro", macro);
		}
		SerializeNonVisualProperties(writer, picture, holder, mainNamespace);
		SerializeBlipFill(writer, picture, relationId, mainNamespace);
		SerializeShapeProperties(writer, picture, mainNamespace);
		writer.WriteEndElement();
		if (mainNamespace == "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing")
		{
			writer.WriteStartElement("clientData", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
			writer.WriteEndElement();
		}
	}

	private void SerializeShapeProperties(XmlWriter writer, BitmapShapeImpl picture, string mainNamespace)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (picture == null)
		{
			throw new ArgumentNullException("picture");
		}
		if (picture.ShapePropertiesStream != null)
		{
			Excel2007Serializator.SerializeStream(writer, picture.ShapePropertiesStream, null);
			return;
		}
		writer.WriteStartElement("spPr", mainNamespace);
		DrawingShapeSerializator.SerializeForm(writer, "http://schemas.openxmlformats.org/drawingml/2006/main", "http://schemas.openxmlformats.org/drawingml/2006/main", 0, 1, 2076450, 1557338, picture);
		SerializePresetGeometry(writer);
		writer.WriteEndElement();
	}

	private void SerializeNonVisualProperties(XmlWriter writer, BitmapShapeImpl picture, WorksheetDataHolder holder, string mainNamespace)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (picture == null)
		{
			throw new ArgumentNullException("picture");
		}
		writer.WriteStartElement("nvPicPr", mainNamespace);
		SerializeNVCanvasProperties(writer, picture, holder, mainNamespace);
		SerializeNVPictureCanvasProperties(writer, picture, mainNamespace);
		writer.WriteEndElement();
	}

	private void SerializeNVPictureCanvasProperties(XmlWriter writer, BitmapShapeImpl picture, string mainNamespace)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (picture == null)
		{
			throw new ArgumentNullException("picture");
		}
		writer.WriteStartElement("cNvPicPr", mainNamespace);
		writer.WriteStartElement("picLocks", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("noChangeAspect", "1");
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	private void SerializeBlipFill(XmlWriter writer, BitmapShapeImpl picture, string relationId, string mainNamespace)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (picture == null)
		{
			throw new ArgumentNullException("picture");
		}
		if (relationId == null || relationId.Length == 0)
		{
			throw new ArgumentOutOfRangeException("relationId");
		}
		writer.WriteStartElement("blipFill", mainNamespace);
		writer.WriteStartElement("blip", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("embed", "http://schemas.openxmlformats.org/officeDocument/2006/relationships", relationId);
		if (picture.BlipSubNodesStream != null)
		{
			Excel2007Serializator.SerializeStream(writer, picture.BlipSubNodesStream, "root");
		}
		writer.WriteEndElement();
		if (picture.SourceRectStream != null)
		{
			Excel2007Serializator.SerializeStream(writer, picture.SourceRectStream, "root");
		}
		else
		{
			writer.WriteStartElement("srcRect", "http://schemas.openxmlformats.org/drawingml/2006/main");
			if (picture.CropBottomOffset != 0)
			{
				writer.WriteAttributeString("b", picture.CropBottomOffset.ToString());
			}
			if (picture.CropLeftOffset != 0)
			{
				writer.WriteAttributeString("l", picture.CropLeftOffset.ToString());
			}
			if (picture.CropRightOffset != 0)
			{
				writer.WriteAttributeString("r", picture.CropRightOffset.ToString());
			}
			if (picture.CropTopOffset != 0)
			{
				writer.WriteAttributeString("t", picture.CropTopOffset.ToString());
			}
			writer.WriteEndElement();
		}
		writer.WriteStartElement("stretch", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteElementString("fillRect", "http://schemas.openxmlformats.org/drawingml/2006/main", string.Empty);
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	public override void SerializeShapeType(XmlWriter writer, Type shapeType)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public string SerializePictureFile(WorksheetDataHolder holder, BitmapShapeImpl picture, RelationCollection vmlRelations)
	{
		if (holder == null)
		{
			throw new ArgumentNullException("holder");
		}
		if (picture == null)
		{
			throw new ArgumentNullException("picture");
		}
		RelationCollection drawingsRelations = holder.DrawingsRelations;
		string empty = string.Empty;
		string text = null;
		if (picture.BlipId != 0)
		{
			empty = "/" + holder.ParentHolder.GetImageItemName((int)(picture.BlipId - 1));
			text = drawingsRelations.FindRelationByTarget(empty);
		}
		else
		{
			empty = "NULL";
		}
		if (text == null)
		{
			text = drawingsRelations.GenerateRelationId();
			drawingsRelations[text] = new Relation(empty, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/image");
		}
		return text;
	}
}
