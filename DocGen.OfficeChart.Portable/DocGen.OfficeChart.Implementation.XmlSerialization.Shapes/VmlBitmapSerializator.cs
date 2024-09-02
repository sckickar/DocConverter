using System;
using System.Collections.Generic;
using System.Xml;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Shapes;

namespace DocGen.OfficeChart.Implementation.XmlSerialization.Shapes;

internal class VmlBitmapSerializator : HFImageSerializator
{
	public override void Serialize(XmlWriter writer, ShapeImpl shape, WorksheetDataHolder holder, RelationCollection vmlRelations)
	{
		writer.WriteStartElement("shape", "urn:schemas-microsoft-com:vml");
		string value = "#" + $"_x0000_t{shape.InnerSpRecord.Instance}";
		string value2 = $"_x0000_s{shape.ShapeId}";
		writer.WriteAttributeString("id", value2);
		writer.WriteAttributeString("type", value);
		List<string> styleProperties = new List<string>();
		PrepareStyleProperties(styleProperties, shape);
		writer.WriteAttributeString("filled", "t");
		writer.WriteAttributeString("fillcolor", "window [65]");
		if (shape.HasBorder)
		{
			writer.WriteAttributeString("stroked", "t");
		}
		writer.WriteAttributeString("strokecolor", "windowText [64]");
		writer.WriteStartElement("v", "fill", null);
		writer.WriteAttributeString("color2", "window [65]");
		writer.WriteEndElement();
		SerializeImageData(writer, shape, holder, string.Empty, useRawFormat: true, vmlRelations);
		SerializeClientData(writer, shape, "Pict");
		writer.WriteEndElement();
	}

	protected override void SerializeClientDataAdditional(XmlWriter writer, ShapeImpl shape)
	{
		base.SerializeClientDataAdditional(writer, shape);
		writer.WriteElementString("CF", "urn:schemas-microsoft-com:office:excel", "Pict");
		writer.WriteElementString("AutoPict", "urn:schemas-microsoft-com:office:excel", string.Empty);
		BitmapShapeImpl obj = shape as BitmapShapeImpl;
		if (obj.IsDDE)
		{
			writer.WriteElementString("DDE", "urn:schemas-microsoft-com:office:excel", null);
		}
		if (obj.IsCamera)
		{
			writer.WriteElementString("Camera", "urn:schemas-microsoft-com:office:excel", null);
		}
	}

	private void PrepareStyleProperties(List<string> styleProperties, ShapeImpl shape)
	{
		styleProperties.Add("position:absolute");
		AddMeasurement(styleProperties, "margin-left", shape.Left);
		AddMeasurement(styleProperties, "margin-top", shape.Top);
		AddMeasurement(styleProperties, "width", shape.Width);
		AddMeasurement(styleProperties, "height", shape.Height);
	}

	private void AddMeasurement(List<string> styleProperties, string tagName, int size)
	{
		double num = Math.Round(ApplicationImpl.ConvertFromPixel(size, MeasureUnits.Point), 2);
		styleProperties.Add($"{tagName}:{num}pt");
	}

	protected override string SerializePicture(ShapeImpl shape, WorksheetDataHolder holder, bool useRawFormat, RelationCollection relations)
	{
		if (shape == null)
		{
			throw new ArgumentNullException("shape");
		}
		BitmapShapeImpl obj = shape as BitmapShapeImpl;
		Image picture = obj.Picture;
		int blipId = (int)obj.BlipId;
		ImageFormat rawFormat = picture.RawFormat;
		FileDataHolder parentHolder = holder.ParentHolder;
		holder.ParentHolder.RegisterContentTypes(rawFormat);
		string imageItemName = parentHolder.GetImageItemName(blipId - 1);
		string text = relations.GenerateRelationId();
		relations[text] = new Relation("/" + imageItemName, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/image");
		return text;
	}
}
