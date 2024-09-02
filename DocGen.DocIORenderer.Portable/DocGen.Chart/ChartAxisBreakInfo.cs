using System;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal sealed class ChartAxisBreakInfo
{
	private ChartBreakLineType m_lineType = ChartBreakLineType.Wave;

	private Color m_lineColor = Color.Black;

	private Color m_spacingColor = Color.White;

	private DashStyle m_lineStyle;

	private float m_lineWidth = 1f;

	private double m_lineSpacing = 1.0;

	public ChartBreakLineType LineType
	{
		get
		{
			return m_lineType;
		}
		set
		{
			if (m_lineType != value)
			{
				m_lineType = value;
				OnChanged(EventArgs.Empty);
			}
		}
	}

	public Color LineColor
	{
		get
		{
			return m_lineColor;
		}
		set
		{
			if (m_lineColor != value)
			{
				m_lineColor = value;
				OnChanged(EventArgs.Empty);
			}
		}
	}

	public DashStyle LineStyle
	{
		get
		{
			return m_lineStyle;
		}
		set
		{
			if (m_lineStyle != value)
			{
				m_lineStyle = value;
				OnChanged(EventArgs.Empty);
			}
		}
	}

	public float LineWidth
	{
		get
		{
			return m_lineWidth;
		}
		set
		{
			if (m_lineWidth != value)
			{
				m_lineWidth = value;
				OnChanged(EventArgs.Empty);
			}
		}
	}

	public double LineSpacing
	{
		get
		{
			return m_lineSpacing;
		}
		set
		{
			value = ChartMath.MinMax(value, 0.0, 10.0);
			if (m_lineSpacing != value)
			{
				m_lineSpacing = value;
				OnChanged(EventArgs.Empty);
			}
		}
	}

	public Color SpacingColor
	{
		get
		{
			return m_spacingColor;
		}
		set
		{
			if (m_spacingColor != value)
			{
				m_spacingColor = value;
				OnChanged(EventArgs.Empty);
			}
		}
	}

	public event EventHandler Changed;

	internal void DrawBreakLine(Graphics g, PointF from, PointF to)
	{
		float num = to.X - from.X;
		float num2 = to.Y - from.Y;
		float num3 = (float)Math.Sqrt(num * num + num2 * num2);
		float num4 = (float)m_lineSpacing / 2f;
		float num5 = num4 * num2 / num3;
		float num6 = num4 * num / num3;
		PointF[] array = null;
		switch (m_lineType)
		{
		case ChartBreakLineType.Straight:
			array = new PointF[4]
			{
				from,
				new PointF(from.X + 0.25f * num, from.Y + 0.25f * num2),
				new PointF(from.X + 0.75f * num, from.Y + 0.75f * num2),
				to
			};
			break;
		case ChartBreakLineType.Wave:
			array = RenderingHelper.GetWaveBeziersPoints(from, to, 10, 3f);
			break;
		case ChartBreakLineType.Randomize:
			array = RenderingHelper.GetRendomBeziersPoints(from, to, 10, 3f);
			break;
		}
		if (m_lineSpacing > 0.0)
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			PointF[] array2 = new PointF[array.Length];
			PointF[] array3 = new PointF[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = new PointF(array[i].X - num5, array[i].Y - num6);
				array3[array.Length - i - 1] = new PointF(array[i].X + num5, array[i].Y + num6);
			}
			graphicsPath.AddBeziers(array2);
			graphicsPath.AddBeziers(array3);
			using (SolidBrush brush = new SolidBrush(m_spacingColor))
			{
				g.FillPath(brush, graphicsPath);
			}
			using Pen pen = GetPen();
			g.DrawBeziers(pen, array2);
			g.DrawBeziers(pen, array3);
			return;
		}
		using Pen pen2 = GetPen();
		g.DrawBeziers(pen2, array);
	}

	internal void DrawBreakLine(Graphics3D g3d, PointF from, PointF to)
	{
		float num = to.X - from.X;
		float num2 = to.Y - from.Y;
		float num3 = (float)Math.Sqrt(num * num + num2 * num2);
		float num4 = (float)m_lineSpacing / 2f;
		float num5 = num4 * num2 / num3;
		float num6 = num4 * num / num3;
		PointF[] array = null;
		switch (m_lineType)
		{
		case ChartBreakLineType.Straight:
			array = new PointF[4]
			{
				from,
				new PointF(from.X + 0.25f * num, from.Y + 0.25f * num2),
				new PointF(from.X + 0.75f * num, from.Y + 0.75f * num2),
				to
			};
			break;
		case ChartBreakLineType.Wave:
			array = RenderingHelper.GetWaveBeziersPoints(from, to, 10, 3f);
			break;
		case ChartBreakLineType.Randomize:
			array = RenderingHelper.GetRendomBeziersPoints(from, to, 10, 3f);
			break;
		}
		Plane3D plane = new Plane3D(new Vector3D(0.0, 0.0, 1.0), 0.0);
		Pen pen = GetPen();
		if (m_lineSpacing > 0.0)
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			GraphicsPath graphicsPath2 = new GraphicsPath();
			GraphicsPath graphicsPath3 = new GraphicsPath();
			PointF[] array2 = new PointF[array.Length];
			PointF[] array3 = new PointF[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = new PointF(array[i].X - num5, array[i].Y - num6);
				array3[array.Length - i - 1] = new PointF(array[i].X + num5, array[i].Y + num6);
			}
			graphicsPath.AddBeziers(array2);
			graphicsPath.AddBeziers(array3);
			graphicsPath2.AddBeziers(array2);
			graphicsPath3.AddBeziers(array3);
			Path3D path3D = Path3D.FromGraphicsPath(graphicsPath, plane, 0.0, new SolidBrush(m_spacingColor), null);
			Path3D path3D2 = Path3D.FromGraphicsPath(graphicsPath2, plane, 0.0, null, pen);
			Path3D path3D3 = Path3D.FromGraphicsPath(graphicsPath3, plane, 0.0, null, pen);
			g3d.AddPolygon(new Path3DCollect(new Polygon[3] { path3D, path3D2, path3D3 }));
		}
		else
		{
			GraphicsPath graphicsPath4 = new GraphicsPath();
			graphicsPath4.AddBeziers(array);
			g3d.AddPolygon(Path3D.FromGraphicsPath(graphicsPath4, plane, 0.0, null, pen));
		}
	}

	private Pen GetPen()
	{
		return new Pen(m_lineColor, m_lineWidth)
		{
			DashStyle = m_lineStyle
		};
	}

	private void OnChanged(EventArgs args)
	{
		if (this.Changed != null)
		{
			this.Changed(this, args);
		}
	}
}
