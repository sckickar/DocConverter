using System;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal class Polygon : Plane3D
{
	protected Vector3D[] m_points;

	protected Pen m_pen;

	protected Pen m_FigurePen;

	protected Brush m_brush;

	protected BrushInfo m_brInfo;

	protected bool isClipPolygon;

	public virtual Vector3D[] Points => m_points;

	public Brush Brush => m_brush;

	public Pen Pen => m_pen;

	public BrushInfo BrushInfo => m_brInfo;

	public bool ClipPolygon
	{
		get
		{
			return isClipPolygon;
		}
		set
		{
			if (isClipPolygon != value)
			{
				isClipPolygon = value;
			}
		}
	}

	public Polygon(Vector3D[] points)
		: base(points[0], points[1], (points.Length < 3) ? points[0] : points[2])
	{
		m_points = points;
		CalcNormal();
	}

	public Polygon(Vector3D[] points, Brush br)
		: this(points)
	{
		m_brush = br;
	}

	public Polygon(Vector3D[] points, Polygon polygon)
		: this(points)
	{
		m_brush = polygon.Brush;
		m_brInfo = polygon.BrushInfo;
		m_pen = polygon.Pen;
		m_FigurePen = polygon.m_FigurePen;
	}

	public Polygon(Vector3D[] points, Brush br, Pen pen)
		: this(points)
	{
		m_brush = br;
		m_pen = pen;
	}

	public Polygon(Vector3D[] points, Pen pen)
		: this(points)
	{
		m_pen = pen;
	}

	public Polygon(Vector3D[] points, BrushInfo br)
		: this(points)
	{
		m_brInfo = br;
	}

	public Polygon(Vector3D[] points, BrushInfo br, Pen pen)
		: this(points)
	{
		m_brInfo = br;
		m_pen = pen;
	}

	public Polygon(Vector3D[] points, BrushInfo br, Pen pen, bool IsPNF)
		: this(points)
	{
		m_brInfo = br;
		if (IsPNF)
		{
			m_FigurePen = pen;
		}
		else
		{
			m_pen = pen;
		}
	}

	public Polygon(Vector3D normal, double d)
		: base(normal, d)
	{
		m_points = null;
		m_brInfo = null;
		m_pen = null;
	}

	public Polygon(Vector3D[] points, bool clipPolygon)
		: this(points)
	{
		m_brInfo = null;
		m_pen = new Pen(Color.White);
		isClipPolygon = clipPolygon;
	}

	public Polygon(Polygon poly)
		: base(poly.Points[0], poly.Points[1], poly.Points[2])
	{
		m_points = new Vector3D[poly.Points.Length];
		for (int i = 0; i < poly.Points.Length; i++)
		{
			Vector3D vector3D = poly.Points[i];
			m_points[i] = new Vector3D(vector3D.X, vector3D.Y, vector3D.Z);
		}
		CalcNormal();
		m_brush = poly.Brush;
		m_brInfo = poly.BrushInfo;
		m_pen = poly.Pen;
		m_FigurePen = poly.m_FigurePen;
		isClipPolygon = poly.isClipPolygon;
	}

	~Polygon()
	{
		m_brInfo = null;
		m_brush = null;
		m_pen = null;
	}

	internal static Polygon CreateRectangle(RectangleF bounds, double z, BrushInfo brushInfo, Pen pen)
	{
		return new Polygon(new Vector3D[4]
		{
			new Vector3D(bounds.Left, bounds.Top, z),
			new Vector3D(bounds.Right, bounds.Top, z),
			new Vector3D(bounds.Right, bounds.Bottom, z),
			new Vector3D(bounds.Left, bounds.Bottom, z)
		}, brushInfo, pen);
	}

	internal virtual Vector3D GetNormal(Matrix3D transform)
	{
		Vector3D vector3D = new Vector3D(double.NaN, double.NaN, double.NaN);
		if (m_points != null)
		{
			vector3D = ChartMath.GetNormal(transform * m_points[0], transform * m_points[1], transform * m_points[2]);
			for (int i = 3; i < m_points.Length; i++)
			{
				if (vector3D.IsValid)
				{
					break;
				}
				Vector3D v = transform * m_points[i];
				Vector3D v2 = transform * m_points[0];
				Vector3D v3 = transform * m_points[i / 2];
				vector3D = ChartMath.GetNormal(v, v2, v3);
			}
		}
		else
		{
			vector3D = transform & m_normal;
			vector3D.Normalize();
		}
		return vector3D;
	}

	public virtual void Draw(Graphics3D g3d)
	{
		if (m_points == null || m_points.Length == 0 || isClipPolygon)
		{
			return;
		}
		Transform3D transform = g3d.Transform;
		PointF[] array = new PointF[m_points.Length];
		GraphicsPath graphicsPath = new GraphicsPath(FillMode.Winding);
		for (int i = 0; i < m_points.Length; i++)
		{
			array[i] = transform.ToScreen(m_points[i]);
		}
		graphicsPath.AddPolygon(array);
		if (g3d.Light)
		{
			Vector3D vector3D = !g3d.LightPosition;
			vector3D.Normalize();
			int coef = (int)((double)g3d.LightCoeficient * (2.0 * Math.Abs(m_normal & vector3D) - 1.0));
			if (m_brInfo != null)
			{
				FillPolygon(g3d.Graphics, m_brInfo, graphicsPath, coef);
			}
			if (m_brush != null)
			{
				FillPolygon(g3d.Graphics, m_brush, graphicsPath, coef);
			}
			if (m_pen != null)
			{
				DrawPolygon(g3d.Graphics, m_pen, graphicsPath, coef);
			}
			if (m_FigurePen != null)
			{
				g3d.Graphics.DrawLine(m_FigurePen, array[0], array[2]);
				g3d.Graphics.DrawLine(m_FigurePen, array[1], array[3]);
			}
		}
		else
		{
			if (m_brInfo != null)
			{
				BrushPaint.FillPath(g3d.Graphics, graphicsPath, m_brInfo);
			}
			if (m_brush != null)
			{
				g3d.Graphics.FillPolygon(m_brush, array);
			}
			if (m_pen != null)
			{
				g3d.Graphics.DrawPolygon(m_pen, array);
			}
			if (m_FigurePen != null)
			{
				g3d.Graphics.DrawLine(m_FigurePen, array[0], array[2]);
				g3d.Graphics.DrawLine(m_FigurePen, array[1], array[3]);
			}
		}
	}

	public override void Transform(Matrix3D matrix3D)
	{
		if (Points != null)
		{
			for (int i = 0; i < Points.Length; i++)
			{
				Points[i] = matrix3D * Points[i];
			}
			CalcNormal();
		}
		else
		{
			base.Transform(matrix3D);
		}
	}

	public virtual Polygon Clone()
	{
		return new Polygon(this);
	}

	protected void CalcNormal()
	{
		CalcNormal(Points[0], Points[1], (Points.Length < 3) ? Points[0] : Points[2]);
		for (int i = 3; i < Points.Length; i++)
		{
			if (!Test())
			{
				break;
			}
			CalcNormal(Points[i], Points[0], Points[i / 2]);
		}
	}

	protected void DrawPolygon(Graphics g, Pen pen, GraphicsPath gp, int coef)
	{
		using Pen pen2 = (Pen)pen.Clone();
		pen2.Color = LigthColor(pen2.Color, coef);
		g.DrawPath(pen2, gp);
	}

	protected void FillPolygon(Graphics g, Brush br, GraphicsPath gp, int coef)
	{
		if (br is SolidBrush)
		{
			using (SolidBrush solidBrush = br.Clone() as SolidBrush)
			{
				solidBrush.Color = LigthColor(solidBrush.Color, coef);
				g.FillPath(solidBrush, gp);
				return;
			}
		}
		if (br is HatchBrush)
		{
			using (HatchBrush brush = new HatchBrush((br as HatchBrush).HatchStyle, LigthColor((br as HatchBrush).ForegroundColor, coef), LigthColor((br as HatchBrush).BackgroundColor, coef)))
			{
				g.FillPath(brush, gp);
				return;
			}
		}
		if (br is LinearGradientBrush)
		{
			using (LinearGradientBrush linearGradientBrush = br as LinearGradientBrush)
			{
				ColorBlend colorBlend = null;
				try
				{
					colorBlend = linearGradientBrush.InterpolationColors;
					int i = 0;
					for (int num = colorBlend.Colors.Length; i < num; i++)
					{
						colorBlend.Colors[i] = LigthColor(colorBlend.Colors[i], coef);
					}
				}
				catch (Exception)
				{
					int j = 0;
					for (int num2 = linearGradientBrush.LinearColors.Length; j < num2; j++)
					{
						linearGradientBrush.LinearColors[j] = LigthColor(linearGradientBrush.LinearColors[j], coef);
					}
				}
				g.FillPath(linearGradientBrush, gp);
				return;
			}
		}
		if (!(br is PathGradientBrush))
		{
			return;
		}
		using PathGradientBrush pathGradientBrush = br.Clone() as PathGradientBrush;
		ColorBlend colorBlend2 = null;
		try
		{
			colorBlend2 = pathGradientBrush.InterpolationColors;
			int k = 0;
			for (int num3 = colorBlend2.Colors.Length; k < num3; k++)
			{
				colorBlend2.Colors[k] = LigthColor(colorBlend2.Colors[k], coef);
			}
		}
		catch (Exception)
		{
			int l = 0;
			for (int num4 = pathGradientBrush.SurroundColors.Length; l < num4; l++)
			{
				pathGradientBrush.SurroundColors[l] = LigthColor(pathGradientBrush.SurroundColors[l], coef);
			}
		}
		g.FillPath(pathGradientBrush, gp);
	}

	protected void FillPolygon(Graphics g, BrushInfo brInfo, GraphicsPath gp, int coef)
	{
		BrushPaint.FillPath(g, gp, DrawingHelper.AddColor(brInfo, coef));
	}

	protected Color LigthColor(Color color, int coef)
	{
		int red = ChartMath.MinMax(color.R + coef, 0, 255);
		int green = ChartMath.MinMax(color.G + coef, 0, 255);
		int blue = ChartMath.MinMax(color.B + coef, 0, 255);
		return Color.FromArgb(color.A, red, green, blue);
	}
}
