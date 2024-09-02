using System;
using System.Collections;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal class ChartSeriesPath : ChartSegment
{
	private class ChartPrimitive
	{
		public GraphicsPath Path;

		public Pen Pen;

		public Brush Brush;

		public BrushInfo BrushInfo;

		public string BoxName;
	}

	private ArrayList m_primitives = new ArrayList();

	public ChartSeriesPath()
		: this(null, null, null)
	{
	}

	public ChartSeriesPath(GraphicsPath gp, BrushInfo br, Pen pen)
	{
		AddPrimitive(gp, pen, br);
	}

	public void AddPrimitive(GraphicsPath gp, Pen pen, BrushInfo brushInfo)
	{
		ChartPrimitive chartPrimitive = new ChartPrimitive();
		chartPrimitive.Path = gp;
		chartPrimitive.Pen = pen;
		chartPrimitive.Brush = null;
		chartPrimitive.BrushInfo = brushInfo;
		m_primitives.Add(chartPrimitive);
	}

	public void AddPrimitive(GraphicsPath gp, Pen pen, BrushInfo brushInfo, string boxName)
	{
		ChartPrimitive chartPrimitive = new ChartPrimitive();
		chartPrimitive.Path = gp;
		chartPrimitive.Pen = pen;
		chartPrimitive.Brush = null;
		chartPrimitive.BrushInfo = brushInfo;
		chartPrimitive.BoxName = boxName;
		m_primitives.Add(chartPrimitive);
	}

	public override void Draw(Graphics g)
	{
		foreach (ChartPrimitive primitive in m_primitives)
		{
			if (primitive.Path == null)
			{
				continue;
			}
			if (primitive.Brush != null)
			{
				g.FillPath(primitive.Brush, primitive.Path);
			}
			else if (primitive.BrushInfo != null && !primitive.BrushInfo.IsEmpty)
			{
				if (primitive.BoxName != null)
				{
					ColorBlend colorBlend = new ColorBlend();
					Color[] colors;
					float[] positions;
					if (primitive.BoxName == "BoxRight")
					{
						ChartSeriesRenderer.PhongShadingColors(Color.FromArgb(144, primitive.BrushInfo.BackColor), Color.FromArgb(144, Color.Black), Color.FromArgb(200, Color.Black), Math.PI / 4.0, 30.0, out colors, out positions);
					}
					else
					{
						ChartSeriesRenderer.PhongShadingColors(Color.FromArgb(200, primitive.BrushInfo.BackColor), Color.FromArgb(144, primitive.BrushInfo.BackColor), Color.FromArgb(100, Color.Black), Math.PI / 4.0, 30.0, out colors, out positions);
					}
					colorBlend.Positions = positions;
					colorBlend.Colors = colors;
					ColorBlend interpolationColors = colorBlend;
					using LinearGradientBrush linearGradientBrush = new LinearGradientBrush(new Rectangle(0, 0, 1, 1), Color.Black, Color.White, LinearGradientMode.Vertical);
					linearGradientBrush.InterpolationColors = interpolationColors;
					g.FillPath(linearGradientBrush, primitive.Path);
				}
				else
				{
					BrushPaint.FillPath(g, primitive.Path, primitive.BrushInfo);
				}
			}
			if (primitive.Pen != null)
			{
				g.DrawPath(primitive.Pen, primitive.Path);
			}
		}
	}

	public void Draw(ChartGraph cg)
	{
		foreach (ChartPrimitive primitive in m_primitives)
		{
			if (primitive.Path != null)
			{
				if (primitive.Brush != null)
				{
					cg.DrawPath(primitive.Brush, primitive.Pen, primitive.Path);
				}
				else
				{
					cg.DrawPath(primitive.BrushInfo, primitive.Pen, primitive.Path);
				}
			}
		}
	}

	public void Draw(ChartGraph cg, string g)
	{
		if (cg == null)
		{
			return;
		}
		foreach (ChartPrimitive primitive in m_primitives)
		{
			if (primitive.Path == null)
			{
				continue;
			}
			if (primitive.Brush != null)
			{
				cg.DrawPath(primitive.Brush, primitive.Pen, primitive.Path);
				continue;
			}
			if (primitive.BoxName == "BoxCenter")
			{
				cg.DrawPath(primitive.BrushInfo, primitive.Pen, primitive.Path);
				continue;
			}
			ColorBlend colorBlend = new ColorBlend();
			Color[] colors;
			float[] positions;
			if (primitive.BoxName == "BoxRight")
			{
				ChartSeriesRenderer.PhongShadingColors(Color.FromArgb(144, primitive.BrushInfo.BackColor), Color.FromArgb(144, Color.Black), Color.FromArgb(200, Color.Black), Math.PI / 4.0, 30.0, out colors, out positions);
			}
			else
			{
				ChartSeriesRenderer.PhongShadingColors(Color.FromArgb(200, primitive.BrushInfo.BackColor), Color.FromArgb(144, primitive.BrushInfo.BackColor), Color.FromArgb(100, Color.Black), Math.PI / 4.0, 30.0, out colors, out positions);
			}
			colorBlend.Positions = positions;
			colorBlend.Colors = colors;
			ColorBlend interpolationColors = colorBlend;
			cg.DrawPath(primitive.BrushInfo, primitive.Pen, primitive.Path);
			using LinearGradientBrush linearGradientBrush = new LinearGradientBrush(new Rectangle(0, 0, 1, 1), Color.Black, Color.White, LinearGradientMode.Vertical);
			linearGradientBrush.InterpolationColors = interpolationColors;
			cg.DrawPath(linearGradientBrush, primitive.Pen, primitive.Path);
		}
	}
}
