using System;
using System.Collections.Generic;
using SkiaSharp;

namespace DocGen.Drawing.SkiaSharpHelper;

internal sealed class LinearGradientBrush : Brush
{
	private Color[] m_linearColors;

	private ColorBlend m_colorBlend;

	private WrapMode m_wrapMode;

	private RectangleF m_rect;

	private float m_rotation;

	private bool m_isXlsIO;

	internal Color[] LinearColors
	{
		get
		{
			return m_linearColors;
		}
		set
		{
			m_linearColors = value;
		}
	}

	internal ColorBlend InterpolationColors
	{
		get
		{
			return m_colorBlend;
		}
		set
		{
			m_colorBlend = value;
			UpdateLinearGradientBrush();
		}
	}

	internal WrapMode WrapMode
	{
		get
		{
			return m_wrapMode;
		}
		set
		{
			m_wrapMode = value;
		}
	}

	internal LinearGradientBrush(Point point1, Point point2, Color color1, Color color2)
		: base(color1)
	{
		m_linearColors = new Color[2] { color1, color2 };
	}

	internal LinearGradientBrush(Rectangle rect, Color color1, Color color2, LinearGradientMode linearGradientMode)
		: base(color1)
	{
		m_rect = rect;
		m_linearColors = new Color[2] { color1, color2 };
	}

	internal LinearGradientBrush(RectangleF rect, Color color1, Color color2, LinearGradientMode linearGradientMode)
		: base(color1)
	{
		m_rect = rect;
		m_linearColors = new Color[2] { color1, color2 };
	}

	internal LinearGradientBrush(RectangleF rect, Color color1, Color color2, float angle, bool isAngleScaleable)
		: base(Color.White)
	{
		m_rect = rect;
		m_rotation = angle;
		m_linearColors = new Color[2] { color1, color2 };
		base.Shader = SKShader.CreateLinearGradient(GetGradientPoints(m_rotation - 180f, m_rect).GetSKPoint(), GetGradientPoints(m_rotation, m_rect).GetSKPoint(), Extension.GetArrayOfSKColors(m_linearColors), new float[2] { 0f, 1f }, SKShaderTileMode.Clamp);
	}

	internal LinearGradientBrush(RectangleF rect, Color color1, Color color2, float angle, bool isAngleScaleable, bool isXlsIO)
		: base(Color.White)
	{
		m_isXlsIO = isXlsIO;
		m_rect = rect;
		m_rotation = angle;
		m_linearColors = new Color[2] { color1, color2 };
		List<PointF> gradientPointsFromAngle = GetGradientPointsFromAngle(angle, m_rect);
		base.Shader = SKShader.CreateLinearGradient(gradientPointsFromAngle[0].GetSKPoint(), gradientPointsFromAngle[1].GetSKPoint(), Extension.GetArrayOfSKColors(m_linearColors), new float[2] { 0f, 1f }, SKShaderTileMode.Clamp);
	}

	private void UpdateLinearGradientBrush()
	{
		if (m_isXlsIO)
		{
			List<PointF> gradientPointsFromAngle = GetGradientPointsFromAngle(m_rotation, m_rect);
			base.Shader = SKShader.CreateLinearGradient(gradientPointsFromAngle[0].GetSKPoint(), gradientPointsFromAngle[1].GetSKPoint(), Extension.GetArrayOfSKColors(m_colorBlend.Colors), m_colorBlend.Positions, SKShaderTileMode.Clamp);
		}
		else
		{
			base.Shader = SKShader.CreateLinearGradient(GetGradientPoints(m_rotation - 180f, m_rect).GetSKPoint(), GetGradientPoints(m_rotation, m_rect).GetSKPoint(), Extension.GetArrayOfSKColors(m_colorBlend.Colors), m_colorBlend.Positions, SKShaderTileMode.Clamp);
		}
	}

	private PointF GetGradientPointsWithAngle(float xRadius, float yRadius, float angle)
	{
		angle %= 360f;
		float num = (float)Math.Abs(Math.Tan((double)angle * (Math.PI / 180.0)));
		float num2 = (float)Math.Sqrt(Math.Pow(xRadius, 2.0) * Math.Pow(yRadius, 2.0) / (Math.Pow(yRadius, 2.0) + Math.Pow(xRadius, 2.0) * Math.Pow(num, 2.0)));
		float num3 = num2 * num;
		if (angle >= 0f && angle < 90f)
		{
			return new PointF(xRadius + num2, yRadius + num3);
		}
		if (angle >= 90f && angle < 180f)
		{
			return new PointF(xRadius - num2, yRadius + num3);
		}
		if (angle >= 180f && angle < 270f)
		{
			return new PointF(xRadius - num2, yRadius - num3);
		}
		return new PointF(xRadius + num2, yRadius - num3);
	}

	private PointF GetGradientPoints(float angle, RectangleF bounds)
	{
		PointF pointF = angle.ToString() switch
		{
			"45" => new PointF(1f, 1f), 
			"135" => new PointF(0f, 1f), 
			"225" => new PointF(0f, 0f), 
			"315" => new PointF(1f, 0f), 
			_ => GetGradientPointsWithAngle(0.5f, 0.5f, angle), 
		};
		return new PointF(m_rect.X + m_rect.Width * pointF.X, m_rect.Y + m_rect.Height * pointF.Y);
	}

	private List<PointF> GetGradientPointsFromAngle(float angle, RectangleF bounds)
	{
		float num = (float)(Math.Atan(m_rect.Width / m_rect.Height) * (180.0 / Math.PI));
		angle %= 360f;
		switch (angle.ToString())
		{
		case "45":
			angle = num;
			break;
		case "225":
			angle = num + 180f;
			break;
		case "315":
			angle = 360f - num;
			break;
		case "135":
			angle = 180f - num;
			break;
		}
		PointF item;
		PointF item2;
		if (angle == 0f)
		{
			item = new PointF(bounds.Left, bounds.Top);
			item2 = new PointF(bounds.Right, bounds.Top);
		}
		else if (angle == 90f)
		{
			item = new PointF(bounds.Left, bounds.Top);
			item2 = new PointF(bounds.Left, bounds.Bottom);
		}
		else if (angle == 180f)
		{
			item2 = new PointF(bounds.Left, bounds.Top);
			item = new PointF(bounds.Right, bounds.Top);
		}
		else if (angle == 270f)
		{
			item2 = new PointF(bounds.Left, bounds.Top);
			item = new PointF(bounds.Left, bounds.Bottom);
		}
		else
		{
			double num2 = Math.PI / 180.0;
			double num3 = (double)angle * num2;
			double num4 = Math.Tan(num3);
			float x = bounds.Left + (bounds.Right - bounds.Left) / 2f;
			float y = bounds.Top + (bounds.Bottom - bounds.Top) / 2f;
			PointF pointF = new PointF(x, y);
			x = bounds.Width / 2f * (float)Math.Cos(num3);
			y = (float)(num4 * (double)x);
			x += pointF.X;
			y += pointF.Y;
			PointF point = new PointF(x, y);
			PointF pointF2 = SubPoints(point, pointF);
			PointF point2 = ChoosePoint(angle, bounds);
			float num5 = MulPoints(SubPoints(point2, pointF), pointF2) / MulPoints(pointF2, pointF2);
			item2 = AddPoints(pointF, MulPoint(pointF2, num5));
			item = AddPoints(pointF, MulPoint(pointF2, 0f - num5));
		}
		return new List<PointF> { item, item2 };
	}

	private PointF ChoosePoint(float angle, RectangleF bounds)
	{
		PointF empty = PointF.Empty;
		if (angle < 90f && angle > 0f)
		{
			empty = new PointF(bounds.Right, bounds.Bottom);
		}
		else if (angle < 180f && angle > 90f)
		{
			empty = new PointF(bounds.Left, bounds.Bottom);
		}
		else if (angle < 270f && angle > 180f)
		{
			empty = new PointF(bounds.Left, bounds.Top);
		}
		else
		{
			if (!(angle > 270f))
			{
				throw new ArgumentException("Internal error.");
			}
			empty = new PointF(bounds.Right, bounds.Top);
		}
		return empty;
	}

	internal PointF SubPoints(PointF point1, PointF point2)
	{
		float x = point1.X - point2.X;
		float y = point1.Y - point2.Y;
		return new PointF(x, y);
	}

	private float MulPoints(PointF point1, PointF point2)
	{
		return point1.X * point2.X + point1.Y * point2.Y;
	}

	private static PointF AddPoints(PointF point1, PointF point2)
	{
		float x = point1.X + point2.X;
		float y = point1.Y + point2.Y;
		return new PointF(x, y);
	}

	private static PointF MulPoint(PointF point, float value)
	{
		point.X *= value;
		point.Y *= value;
		return point;
	}
}
