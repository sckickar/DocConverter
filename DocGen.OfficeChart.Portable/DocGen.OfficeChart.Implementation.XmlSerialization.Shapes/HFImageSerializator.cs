using System;
using System.Globalization;
using System.Security;
using System.Xml;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Shapes;

namespace DocGen.OfficeChart.Implementation.XmlSerialization.Shapes;

internal class HFImageSerializator : ShapeSerializator
{
	private static string[] s_arrFormulas = new string[12]
	{
		"if lineDrawn pixelLineWidth 0", "sum @0 1 0", "sum 0 0 @1", "prod @2 1 2", "prod @3 21600 pixelWidth", "prod @3 21600 pixelHeight", "sum @0 0 1", "prod @6 1 2", "prod @7 21600 pixelWidth", "sum @8 21600 0",
		"prod @7 21600 pixelHeight", "sum @10 21600 0"
	};

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
		string value = "#" + $"_x0000_t{shape.InnerSpRecord.Instance}";
		writer.WriteStartElement("shape", "urn:schemas-microsoft-com:vml");
		writer.WriteAttributeString("id", shape.Name);
		writer.WriteAttributeString("type", value);
		BitmapShapeImpl bitmapShapeImpl = (BitmapShapeImpl)shape;
		_ = bitmapShapeImpl.Worksheet.PageSetupBase;
		int width = GetWidth(bitmapShapeImpl);
		int height = GetHeight(bitmapShapeImpl);
		string value2 = ((bitmapShapeImpl.PreserveStyleString == null || bitmapShapeImpl.PreserveStyleString.Length <= 0) ? string.Format(CultureInfo.InvariantCulture.NumberFormat, "width:{0}pt;height:{1}pt", ApplicationImpl.ConvertFromPixel(width, MeasureUnits.Point), ApplicationImpl.ConvertFromPixel(height, MeasureUnits.Point)) : bitmapShapeImpl.PreserveStyleString);
		writer.WriteAttributeString("style", value2);
		SerializeImageData(writer, shape, holder, null, useRawFormat: false, holder.HFDrawingsRelations);
		writer.WriteEndElement();
	}

	protected virtual int GetWidth(BitmapShapeImpl bitmap)
	{
		return bitmap.LeftColumn;
	}

	protected virtual int GetHeight(BitmapShapeImpl bitmap)
	{
		return bitmap.TopRow;
	}

	public override void SerializeShapeType(XmlWriter writer, Type shapeType)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		writer.WriteStartElement("shapetype", "urn:schemas-microsoft-com:vml");
		string value = $"_x0000_t{75}";
		writer.WriteAttributeString("id", value);
		writer.WriteAttributeString("coordsize", "21600,21600");
		writer.WriteAttributeString("spt", "urn:schemas-microsoft-com:office:office", 75.ToString());
		writer.WriteAttributeString("preferrelative", "urn:schemas-microsoft-com:office:office", "t");
		writer.WriteAttributeString("path", "m@4@5l@4@11@9@11@9@5xe");
		writer.WriteAttributeString("filled", "f");
		writer.WriteAttributeString("stroked", "f");
		writer.WriteStartElement("stroke", "urn:schemas-microsoft-com:vml");
		writer.WriteAttributeString("joinstyle", "miter");
		writer.WriteEndElement();
		writer.WriteStartElement("formulas", "urn:schemas-microsoft-com:vml");
		int i = 0;
		for (int num = s_arrFormulas.Length; i < num; i++)
		{
			writer.WriteStartElement("f", "urn:schemas-microsoft-com:vml");
			writer.WriteAttributeString("eqn", s_arrFormulas[i]);
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
		writer.WriteStartElement("path", "urn:schemas-microsoft-com:vml");
		writer.WriteAttributeString("extrusionok", "urn:schemas-microsoft-com:office:office", "f");
		writer.WriteAttributeString("gradientshapeok", "t");
		writer.WriteAttributeString("connecttype", "urn:schemas-microsoft-com:office:office", "rect");
		writer.WriteEndElement();
		writer.WriteStartElement("lock", "urn:schemas-microsoft-com:office:office");
		writer.WriteAttributeString("ext", "urn:schemas-microsoft-com:vml", "edit");
		writer.WriteAttributeString("aspectratio", "t");
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	[SecurityCritical]
	protected void SerializeImageData(XmlWriter writer, ShapeImpl shape, WorksheetDataHolder holder, string title, bool useRawFormat, RelationCollection relations)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (shape == null)
		{
			throw new ArgumentNullException("shape");
		}
		writer.WriteStartElement("imagedata", "urn:schemas-microsoft-com:vml");
		string value = SerializePicture(shape, holder, useRawFormat, relations);
		writer.WriteAttributeString("relid", "urn:schemas-microsoft-com:office:office", value);
		if (title != null)
		{
			writer.WriteAttributeString("title", "urn:schemas-microsoft-com:office:office", title);
		}
		writer.WriteEndElement();
	}

	[SecurityCritical]
	protected virtual string SerializePicture(ShapeImpl shape, WorksheetDataHolder holder, bool useRawFormat, RelationCollection relations)
	{
		if (shape == null)
		{
			throw new ArgumentNullException("shape");
		}
		Image picture = (shape as BitmapShapeImpl).Picture;
		ImageFormat rawFormat = picture.RawFormat;
		FileDataHolder parentHolder = holder.ParentHolder;
		holder.ParentHolder.RegisterContentTypes(rawFormat);
		string text = parentHolder.SaveImage(picture, rawFormat, null);
		string text2 = relations.GenerateRelationId();
		relations[text2] = new Relation("/" + text, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/image");
		return text2;
	}

	[SecurityCritical]
	private string SerializePicture(ShapeImpl shape, WorksheetDataHolder holder)
	{
		if (shape == null)
		{
			throw new ArgumentNullException("shape");
		}
		return SerializePicture(shape, holder, useRawFormat: false, holder.HFDrawingsRelations);
	}
}
