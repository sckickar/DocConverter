using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;
using DocGen.PdfViewer.Base;

namespace DocGen.Pdf.Parsing;

internal class AxialShading : GradientShading
{
	private bool m_isRectangle;

	private bool m_isCircle;

	private float m_rectangleWidth;

	internal RectangleF ShadingRectangle = RectangleF.Empty;

	public AxialShading(PdfDictionary dictionary)
		: base(dictionary)
	{
	}

	public AxialShading()
	{
	}

	internal override void SetOperatorValues(bool IsRectangle, bool IsCircle, string RectangleWidth)
	{
		m_isRectangle = IsRectangle;
		m_isCircle = IsCircle;
		if (RectangleWidth != null)
		{
			m_rectangleWidth = float.Parse(RectangleWidth);
		}
	}

	internal override PdfBrush GetBrushColor(DocGen.PdfViewer.Base.Matrix transformMatrix)
	{
		try
		{
			double num = 0.0;
			double num2 = 0.0;
			double num3 = 0.0;
			double num4 = 0.0;
			if (m_isRectangle)
			{
				num = (base.Coordinate[0] as PdfNumber).FloatValue;
				num2 = (base.Coordinate[1] as PdfNumber).FloatValue;
				num3 = (base.Coordinate[2] as PdfNumber).FloatValue;
				num4 = (base.Coordinate[3] as PdfNumber).FloatValue;
				if (num < 0.0 && num3 > 0.0 && num2 == 0.0 && num4 == 0.0)
				{
					double num5 = m_rectangleWidth;
					num = (double)(base.Coordinate[0] as PdfNumber).FloatValue * transformMatrix.M11;
					double num6 = num;
					num2 = (double)(base.Coordinate[1] as PdfNumber).FloatValue * transformMatrix.M22;
					num3 = (double)(base.Coordinate[2] as PdfNumber).FloatValue * transformMatrix.M11;
					num4 = (double)(base.Coordinate[3] as PdfNumber).FloatValue * transformMatrix.M22;
					if (num < 0.0 && num3 > 0.0 && num5 > num6)
					{
						num3 = num5;
					}
					else if (num5 > num6)
					{
						num = num5;
					}
					else
					{
						num = (base.Coordinate[0] as PdfNumber).FloatValue;
						num2 = (base.Coordinate[1] as PdfNumber).FloatValue;
						num3 = (base.Coordinate[2] as PdfNumber).FloatValue;
						num4 = (base.Coordinate[3] as PdfNumber).FloatValue;
					}
				}
				else
				{
					num = (base.Coordinate[0] as PdfNumber).FloatValue;
					num2 = (base.Coordinate[1] as PdfNumber).FloatValue;
					num3 = (base.Coordinate[2] as PdfNumber).FloatValue;
					num4 = (base.Coordinate[3] as PdfNumber).FloatValue;
				}
			}
			else if (m_isCircle)
			{
				if (transformMatrix.M21 > 0.0 && transformMatrix.M12 < 0.0 && transformMatrix.M11 < 0.0 && transformMatrix.M22 < 0.0)
				{
					num = (double)(base.Coordinate[0] as PdfNumber).FloatValue * transformMatrix.M11;
					num += num * transformMatrix.M21;
					num2 = (double)(base.Coordinate[1] as PdfNumber).FloatValue * transformMatrix.M22;
					num2 += num2 * transformMatrix.M12;
					num3 = (double)(base.Coordinate[2] as PdfNumber).FloatValue * transformMatrix.M11 + (double)(base.Coordinate[2] as PdfNumber).FloatValue * transformMatrix.M11;
					num3 += num3 * transformMatrix.M21;
					num4 = (double)(base.Coordinate[3] as PdfNumber).FloatValue * transformMatrix.M22 + (double)(base.Coordinate[3] as PdfNumber).FloatValue * transformMatrix.M22;
					num4 += num4 * transformMatrix.M12;
				}
				else
				{
					num = (base.Coordinate[0] as PdfNumber).FloatValue;
					num2 = (base.Coordinate[1] as PdfNumber).FloatValue;
					num3 = (base.Coordinate[2] as PdfNumber).FloatValue;
					num4 = (base.Coordinate[3] as PdfNumber).FloatValue;
				}
			}
			else
			{
				num = (base.Coordinate[0] as PdfNumber).FloatValue;
				num2 = (base.Coordinate[1] as PdfNumber).FloatValue;
				num3 = (base.Coordinate[2] as PdfNumber).FloatValue;
				num4 = (base.Coordinate[3] as PdfNumber).FloatValue;
			}
			double num7 = (base.Domain[0] as PdfNumber).FloatValue;
			double num8 = (base.Domain[1] as PdfNumber).FloatValue;
			if (base.Extented != null)
			{
				_ = (base.Extented[0] as PdfBoolean).Value;
				_ = (base.Extented[1] as PdfBoolean).Value;
			}
			Color color = GetColor(num7);
			Color color2 = GetColor(num8);
			PdfLinearGradientBrush pdfLinearGradientBrush;
			if (base.AlternateColorspace is DeviceN)
			{
				DeviceN deviceN = base.AlternateColorspace as DeviceN;
				if (deviceN.AlternateColorspace is ICCBased iCCBased)
				{
					if ((transformMatrix.M11 != 0.0 || transformMatrix.M22 != 0.0) && iCCBased.Profile.AlternateColorspace is DeviceCMYK)
					{
						num = (double)(base.Coordinate[0] as PdfNumber).FloatValue * transformMatrix.M11;
						num2 = (double)(base.Coordinate[1] as PdfNumber).FloatValue * transformMatrix.M22;
						num3 = (double)(base.Coordinate[2] as PdfNumber).FloatValue * transformMatrix.M11;
						num4 = (double)(base.Coordinate[3] as PdfNumber).FloatValue * transformMatrix.M22;
						if (num3 > 0.0)
						{
							pdfLinearGradientBrush = new PdfLinearGradientBrush(new PointF((float)num, (float)num2), new PointF((float)num3, (float)num4), color, color2);
							ShadingRectangle = pdfLinearGradientBrush.Rectangle;
						}
						num /= transformMatrix.M11;
						num2 /= transformMatrix.M22;
						num3 /= transformMatrix.M11;
						num4 /= transformMatrix.M22;
					}
				}
				else if ((transformMatrix.M11 != 0.0 || transformMatrix.M22 != 0.0) && deviceN.AlternateColorspace is DeviceCMYK)
				{
					num = (double)(base.Coordinate[0] as PdfNumber).FloatValue * transformMatrix.M11;
					num2 = (double)(base.Coordinate[1] as PdfNumber).FloatValue * transformMatrix.M22;
					num3 = (double)(base.Coordinate[2] as PdfNumber).FloatValue * transformMatrix.M11;
					num4 = (double)(base.Coordinate[3] as PdfNumber).FloatValue * transformMatrix.M22;
					if (num3 > 0.0)
					{
						pdfLinearGradientBrush = new PdfLinearGradientBrush(new PointF((float)num, (float)num2), new PointF((float)num3, (float)num4), color, color2);
						ShadingRectangle = pdfLinearGradientBrush.Rectangle;
					}
					num /= transformMatrix.M11;
					num2 /= transformMatrix.M22;
					num3 /= transformMatrix.M11;
					num4 /= transformMatrix.M22;
				}
			}
			if (base.AlternateColorspace is DeviceRGB && (transformMatrix.M11 != 0.0 || transformMatrix.M22 != 0.0) && (base.Coordinate[1] as PdfNumber).FloatValue > 0f && (base.Coordinate[0] as PdfNumber).FloatValue == 0f)
			{
				num = (double)(base.Coordinate[0] as PdfNumber).FloatValue * transformMatrix.M11;
				num2 = (double)(base.Coordinate[1] as PdfNumber).FloatValue * transformMatrix.M22;
				num3 = (double)(base.Coordinate[2] as PdfNumber).FloatValue * transformMatrix.M11;
				num4 = (double)(base.Coordinate[3] as PdfNumber).FloatValue * transformMatrix.M22;
			}
			pdfLinearGradientBrush = new PdfLinearGradientBrush(new PointF((float)num, (float)num2), new PointF((float)num3, (float)num4), color, color2);
			Type0 type = base.Function as Type0;
			bool flag = false;
			if (type != null)
			{
				flag = (type.Size[0] as PdfNumber).IntValue <= 2;
			}
			PdfColor[] colors;
			float[] array;
			if (!flag)
			{
				double d = Math.Sqrt((num - num3) * (num - num3) + (num2 - num4) * (num2 - num4));
				double num9 = num8 - num7;
				double num10 = 1.0 / (transformMatrix.Transform(d) * 3.0);
				List<PdfColor> list = new List<PdfColor>();
				List<float> list2 = new List<float>();
				for (double num11 = num7; num11 < num8; num11 += num10)
				{
					list.Add(new PdfColor(GetColor(num11)));
					list2.Add(Convert.ToSingle((num11 - num7) / num9));
				}
				colors = list.ToArray();
				array = list2.ToArray();
				array[list2.Count - 1] = 1f;
				list.Clear();
				list2.Clear();
			}
			else
			{
				Color color3 = ExecuteLinearInterpolation(color, color2, 0.5f);
				colors = new PdfColor[3]
				{
					new PdfColor(color),
					new PdfColor(color3),
					new PdfColor(color2)
				};
				array = new float[3] { 0f, 0.5f, 1f };
			}
			PdfColorBlend pdfColorBlend = new PdfColorBlend();
			pdfColorBlend.Colors = colors;
			pdfColorBlend.Positions = array;
			pdfLinearGradientBrush.InterpolationColors = pdfColorBlend;
			return pdfLinearGradientBrush;
		}
		catch
		{
			return PdfBrushes.Transparent;
		}
	}

	private Color ExecuteLinearInterpolation(Color color1, Color color2, float factor)
	{
		int red = (int)((1f - factor) * (float)(int)color1.R + factor * (float)(int)color2.R);
		int green = (int)((1f - factor) * (float)(int)color1.G + factor * (float)(int)color2.G);
		int blue = (int)((1f - factor) * (float)(int)color1.B + factor * (float)(int)color2.B);
		return Color.FromArgb(255, red, green, blue);
	}
}
