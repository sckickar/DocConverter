using System;
using System.Xml;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Implementation.XmlSerialization.Charts;

namespace DocGen.OfficeChart.Implementation.XmlSerialization;

internal class GradientSerializator
{
	public void Serialize(XmlWriter writer, GradientStops gradientStops, IWorkbook book)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (gradientStops == null)
		{
			throw new ArgumentNullException("gradientStops");
		}
		writer.WriteStartElement("gradFill", "http://schemas.openxmlformats.org/drawingml/2006/main");
		SerializeGradientStops(writer, gradientStops, book);
		writer.WriteEndElement();
	}

	private void SerializeGradientStops(XmlWriter writer, GradientStops gradientStops, IWorkbook book)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (gradientStops == null)
		{
			throw new ArgumentNullException("gradientStops");
		}
		writer.WriteStartElement("gsLst", "http://schemas.openxmlformats.org/drawingml/2006/main");
		int i = 0;
		for (int count = gradientStops.Count; i < count; i++)
		{
			SerializeGradientStop(writer, gradientStops[i], book);
		}
		writer.WriteEndElement();
		if (gradientStops.GradientType == GradientType.Liniar)
		{
			writer.WriteStartElement("lin", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteAttributeString("ang", gradientStops.Angle.ToString());
			writer.WriteAttributeString("scaled", "1");
			writer.WriteEndElement();
		}
		else
		{
			writer.WriteStartElement("path", "http://schemas.openxmlformats.org/drawingml/2006/main");
			string value = gradientStops.GradientType.ToString().ToLower();
			writer.WriteAttributeString("path", value);
			Rectangle fillToRect = gradientStops.FillToRect;
			if (fillToRect.Left != 0 || fillToRect.Top != 0 || fillToRect.Right != 0 || fillToRect.Bottom != 0)
			{
				writer.WriteStartElement("fillToRect", "http://schemas.openxmlformats.org/drawingml/2006/main");
				int left = fillToRect.Left;
				if (left != 0)
				{
					writer.WriteAttributeString("l", left.ToString());
				}
				int top = fillToRect.Top;
				if (top != 0)
				{
					writer.WriteAttributeString("t", top.ToString());
				}
				if (fillToRect.Right != 0)
				{
					writer.WriteAttributeString("r", fillToRect.Right.ToString());
				}
				if (fillToRect.Bottom != 0)
				{
					writer.WriteAttributeString("b", fillToRect.Bottom.ToString());
				}
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}
		Rectangle tileRect = gradientStops.TileRect;
		if (tileRect.Left != 0 || tileRect.Top != 0 || tileRect.Right != 0 || tileRect.Bottom != 0)
		{
			writer.WriteStartElement("tileRect", "http://schemas.openxmlformats.org/drawingml/2006/main");
			int right = tileRect.Right;
			if (right != 0)
			{
				writer.WriteAttributeString("r", right.ToString());
			}
			int bottom = tileRect.Bottom;
			if (bottom != 0)
			{
				writer.WriteAttributeString("b", bottom.ToString());
			}
			int left2 = tileRect.Left;
			if (left2 != 0)
			{
				writer.WriteAttributeString("l", left2.ToString());
			}
			int top2 = tileRect.Top;
			if (top2 != 0)
			{
				writer.WriteAttributeString("t", top2.ToString());
			}
			writer.WriteEndElement();
		}
	}

	private void SerializeGradientStop(XmlWriter writer, GradientStopImpl gradientStop, IWorkbook book)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (gradientStop == null)
		{
			throw new ArgumentNullException("gradientStop");
		}
		writer.WriteStartElement("gs", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("pos", gradientStop.Position.ToString());
		if (!gradientStop.ColorObject.IsSchemeColor)
		{
			ChartSerializatorCommon.SerializeRgbColor(writer, gradientStop.ColorObject.GetRGB(book), gradientStop.Transparency, gradientStop.Tint, gradientStop.Shade);
		}
		else
		{
			SerializeSchemeColor(writer, gradientStop, book);
		}
		writer.WriteEndElement();
	}

	private void SerializeSchemeColor(XmlWriter writer, GradientStopImpl gradienstop, IWorkbook book)
	{
		int num = 100000;
		writer.WriteStartElement("schemeClr", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("val", gradienstop.ColorObject.SchemaName);
		ChartColor colorObject = gradienstop.ColorObject;
		if (colorObject.Tint > 0.0)
		{
			ChartSerializatorCommon.SerializeDoubleValueTag(writer, "tint", "http://schemas.openxmlformats.org/drawingml/2006/main", colorObject.Tint);
		}
		if (colorObject.Saturation > 0.0)
		{
			ChartSerializatorCommon.SerializeDoubleValueTag(writer, "satMod", "http://schemas.openxmlformats.org/drawingml/2006/main", colorObject.Saturation);
		}
		if (colorObject.Luminance > 0.0)
		{
			ChartSerializatorCommon.SerializeDoubleValueTag(writer, "lumMod", "http://schemas.openxmlformats.org/drawingml/2006/main", colorObject.Luminance);
		}
		if (colorObject.LuminanceOffSet > 0.0)
		{
			ChartSerializatorCommon.SerializeDoubleValueTag(writer, "lumOff", "http://schemas.openxmlformats.org/drawingml/2006/main", colorObject.LuminanceOffSet);
		}
		if (gradienstop.Transparency != 100000 && gradienstop.Transparency >= 0 && gradienstop.Transparency <= num)
		{
			ChartSerializatorCommon.SerializeDoubleValueTag(writer, "alpha", "http://schemas.openxmlformats.org/drawingml/2006/main", gradienstop.Transparency);
		}
		if (gradienstop.Shade > 0)
		{
			ChartSerializatorCommon.SerializeDoubleValueTag(writer, "shade", "http://schemas.openxmlformats.org/drawingml/2006/main", gradienstop.Shade);
		}
		writer.WriteEndElement();
	}
}
