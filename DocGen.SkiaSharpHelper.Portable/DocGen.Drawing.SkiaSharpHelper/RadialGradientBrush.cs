using System;
using SkiaSharp;

namespace DocGen.Drawing.SkiaSharpHelper;

internal sealed class RadialGradientBrush : Brush
{
	private Color[] m_surroundColors;

	private RectangleF m_rectangle;

	private ColorBlend m_interpolationColors;

	private PointF m_foculaScales;

	private PointF m_centerPoint;

	private GraphicsPath m_path;

	internal Color[] SurroundColor
	{
		get
		{
			return m_surroundColors;
		}
		set
		{
			m_surroundColors = value;
		}
	}

	internal RectangleF Rectangle
	{
		get
		{
			return m_rectangle;
		}
		set
		{
			m_rectangle = value;
		}
	}

	internal ColorBlend InterpolationColors
	{
		get
		{
			return m_interpolationColors;
		}
		set
		{
			m_interpolationColors = value;
			UpdateRadialGradientBrush();
		}
	}

	internal PointF FoculScales
	{
		get
		{
			return m_foculaScales;
		}
		set
		{
			m_foculaScales = value;
		}
	}

	internal PointF CenterPoint
	{
		get
		{
			return m_centerPoint;
		}
		set
		{
			m_centerPoint = value;
			UpdateRadialGradientBrush();
		}
	}

	internal Color CenterColor
	{
		get
		{
			return base.Color;
		}
		set
		{
			base.Color = value;
		}
	}

	internal RadialGradientBrush(GraphicsPath path)
		: base(Color.White)
	{
		m_path = path;
		m_surroundColors = new Color[1] { Color.FromArgb(255, 255, 255, 255) };
		m_rectangle = RectangleF.Empty;
		m_interpolationColors = new ColorBlend();
		m_foculaScales = PointF.Empty;
		m_centerPoint = PointF.Empty;
	}

	internal PointF GetRadialCenterPoint(Rectangle rect, RectangleF bounds)
	{
		PointF empty = PointF.Empty;
		return (rect.Right == 0 && rect.Bottom == 0) ? new PointF(bounds.X + bounds.Width, bounds.Y + bounds.Height) : ((rect.Left == 0 && rect.Bottom == 0) ? new PointF(bounds.X, bounds.Y + bounds.Height) : ((rect.Right == 0 && rect.Top == 0) ? new PointF(bounds.X + bounds.Width, bounds.Y) : ((rect.Left != 0 || rect.Top != 0) ? new PointF(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f) : new PointF(bounds.X, bounds.Y))));
	}

	private void UpdateRadialGradientBrush()
	{
		RectangleF clipRectangle = RenderHelper.GetClipRectangle(m_path.Bounds);
		float num = (float)Math.Sqrt(clipRectangle.Width * clipRectangle.Width + clipRectangle.Height * clipRectangle.Height);
		_ = m_centerPoint;
		if (m_centerPoint.X == 0f && m_centerPoint.Y == 0f)
		{
			m_centerPoint = new PointF(clipRectangle.X + clipRectangle.Width / 2f, clipRectangle.Y + clipRectangle.Height / 2f);
		}
		if (m_centerPoint == new PointF(clipRectangle.X + clipRectangle.Width / 2f, clipRectangle.Y + clipRectangle.Height / 2f))
		{
			num /= 2f;
		}
		base.Shader = SKShader.CreateRadialGradient(m_centerPoint.GetSKPoint(), num, Extension.GetArrayOfSKColors(m_interpolationColors.Colors), m_interpolationColors.Positions, SKShaderTileMode.Clamp);
	}
}
