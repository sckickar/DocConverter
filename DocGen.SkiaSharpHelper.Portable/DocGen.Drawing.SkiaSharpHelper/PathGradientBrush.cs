using SkiaSharp;

namespace DocGen.Drawing.SkiaSharpHelper;

internal sealed class PathGradientBrush : Brush
{
	private Color[] m_surroundColors;

	private RectangleF m_rectangle;

	private ColorBlend m_interpolationColors;

	private PointF m_focusScales;

	private PointF m_centerPoint;

	private GraphicsPath m_path;

	internal Color[] SurroundColors
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

	internal RectangleF Rectangle => m_rectangle;

	internal ColorBlend InterpolationColors
	{
		get
		{
			return m_interpolationColors;
		}
		set
		{
			m_interpolationColors = value;
			UpdatePathGradientBrush();
		}
	}

	internal PointF FocusScales
	{
		get
		{
			return m_focusScales;
		}
		set
		{
			m_focusScales = value;
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
			UpdatePathGradientBrush();
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

	internal PathGradientBrush(GraphicsPath path)
		: base(Color.White)
	{
		m_path = path;
		m_surroundColors = new Color[1] { Color.FromArgb(255, 255, 255, 255) };
		m_rectangle = RectangleF.Empty;
		m_interpolationColors = new ColorBlend();
		m_focusScales = PointF.Empty;
		m_centerPoint = PointF.Empty;
	}

	private void UpdatePathGradientBrush()
	{
		RectangleF clipRectangle = RenderHelper.GetClipRectangle(m_path.Bounds);
		_ = m_centerPoint;
		if (m_centerPoint.X == 0f && m_centerPoint.Y == 0f)
		{
			m_centerPoint = new PointF(clipRectangle.X + clipRectangle.Width / 2f, clipRectangle.Y + clipRectangle.Height / 2f);
		}
		base.Shader = SKShader.CreateRadialGradient(m_centerPoint.GetSKPoint(), clipRectangle.Width / 2f, Extension.GetArrayOfSKColors(Extension.GetReversedColors(m_interpolationColors.Colors)), m_interpolationColors.Positions, SKShaderTileMode.Clamp);
	}
}
