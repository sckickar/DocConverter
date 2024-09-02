using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;
using DocGen.PdfViewer.Base;

namespace DocGen.Pdf.Parsing;

internal class RadialShading : GradientShading
{
	public RadialShading(PdfDictionary dictionary)
		: base(dictionary)
	{
	}

	public RadialShading()
	{
	}

	internal override PdfBrush GetBrushColor(DocGen.PdfViewer.Base.Matrix transformMatrix)
	{
		double num = (base.Coordinate[0] as PdfNumber).FloatValue;
		double num2 = (base.Coordinate[1] as PdfNumber).FloatValue;
		double num3 = (base.Coordinate[2] as PdfNumber).FloatValue;
		double num4 = (base.Coordinate[3] as PdfNumber).FloatValue;
		double num5 = (base.Coordinate[4] as PdfNumber).FloatValue;
		double num6 = (base.Coordinate[5] as PdfNumber).FloatValue;
		double num7 = (base.Domain[0] as PdfNumber).FloatValue;
		double num8 = (base.Domain[1] as PdfNumber).FloatValue;
		if (base.Extented != null)
		{
			_ = (base.Extented[0] as PdfBoolean).Value;
			_ = (base.Extented[1] as PdfBoolean).Value;
		}
		RectangleF rectangleF;
		if (num3 != 0.0)
		{
			rectangleF = new RectangleF((float)(num - num3), (float)(num2 - num3), (float)(num3 * 2.0), (float)(num3 * 2.0));
		}
		else
		{
			rectangleF = new RectangleF((float)(num4 - num6), (float)(num5 - num6), (float)(num6 * 2.0), (float)(num6 * 2.0));
		}
		rectangleF = ((num6 == 0.0) ? new RectangleF((float)(num - num3), (float)(num2 - num3), (float)(num3 * 2.0), (float)(num3 * 2.0)) : new RectangleF((float)(num4 - num6), (float)(num5 - num6), (float)(num6 * 2.0), (float)(num6 * 2.0)));
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddRectangle(rectangleF);
		PathGradientBrush pathGradientBrush = new PathGradientBrush(graphicsPath);
		double num9 = Math.Sqrt((num - num4) * (num - num4) + (num2 - num5) * (num2 - num5));
		double num10 = num8 - num7;
		double num11 = Math.Abs(num8 - num7);
		double num12 = 1.0 / (transformMatrix.Transform(num9 - num11) * 3.0);
		List<PdfColor> list = new List<PdfColor>();
		List<float> list2 = new List<float>();
		if (num12 > 0.0)
		{
			for (double num13 = num7; num13 < num8; num13 += num12)
			{
				list.Add(new PdfColor(GetColor(num13)));
				list2.Add(Convert.ToSingle((num13 - num7) / num10));
			}
			PdfColor[] colors = list.ToArray();
			float[] array = list2.ToArray();
			array[list2.Count - 1] = 1f;
			PdfColorBlend pdfColorBlend = new PdfColorBlend();
			pdfColorBlend.Colors = colors;
			pdfColorBlend.Positions = array;
			pathGradientBrush.InterpolationColors = pdfColorBlend;
			list.Clear();
			list2.Clear();
			return pathGradientBrush;
		}
		return new PdfSolidBrush(Color.Gray);
	}
}
