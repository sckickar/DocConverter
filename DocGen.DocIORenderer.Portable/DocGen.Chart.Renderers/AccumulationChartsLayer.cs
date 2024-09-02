using System;
using System.Collections;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart.Renderers;

internal class AccumulationChartsLayer
{
	private const int POLYGON_SECTORS = 18;

	private const double D_TO_R = Math.PI / 180.0;

	private const double R_TO_D = 180.0 / Math.PI;

	private const double PI_2 = Math.PI / 2.0;

	private float m_upWidth;

	private float m_downWidth;

	private float m_height;

	private float m_gapRatio;

	private float m_minWidth = 40f;

	private int m_index;

	private PointF m_topCenter;

	private bool m_series3D;

	private float m_offset3DRation = 0.1f;

	private float m_rotationRation = (float)Math.PI / 3f;

	private bool m_topLevel;

	private ChartFunnelMode m_funnelMode = ChartFunnelMode.YIsHeight;

	private ChartFigureBase m_figureBase;

	private ChartSeries m_series;

	private static double s_phongAngle;

	private static ColorBlend s_colorBlend;

	private float m_depth;

	public int Index => m_index;

	public float UpWidth
	{
		get
		{
			return m_upWidth;
		}
		set
		{
			m_upWidth = value;
		}
	}

	public float DownWidth
	{
		get
		{
			return m_downWidth;
		}
		set
		{
			m_downWidth = value;
		}
	}

	public float Height
	{
		get
		{
			return m_height;
		}
		set
		{
			m_height = value;
		}
	}

	public float GapRatio
	{
		get
		{
			return m_gapRatio;
		}
		set
		{
			m_gapRatio = value;
		}
	}

	public float MinWidth
	{
		get
		{
			return m_minWidth;
		}
		set
		{
			m_minWidth = value;
		}
	}

	public PointF TopCenterPoint
	{
		get
		{
			return m_topCenter;
		}
		set
		{
			m_topCenter = value;
		}
	}

	public bool Series3D => m_series3D;

	public bool TopLevel
	{
		get
		{
			return m_topLevel;
		}
		set
		{
			m_topLevel = value;
		}
	}

	public ChartFunnelMode FunnelMode => m_funnelMode;

	public ChartFigureBase FigureBase
	{
		get
		{
			return m_figureBase;
		}
		set
		{
			m_figureBase = value;
		}
	}

	public float RotationRation
	{
		get
		{
			return m_rotationRation;
		}
		set
		{
			m_rotationRation = value;
		}
	}

	public float Offset3DRation
	{
		get
		{
			return m_offset3DRation;
		}
		set
		{
			m_offset3DRation = value;
		}
	}

	public ChartSeries Series
	{
		get
		{
			return m_series;
		}
		set
		{
			m_series = value;
		}
	}

	public float DepthPosition
	{
		get
		{
			return m_depth;
		}
		set
		{
			m_depth = value;
		}
	}

	public float GetGapRatioHeight()
	{
		return (1f - 2f * GapRatio) * Height;
	}

	public double GetAngleTangent()
	{
		double num = GetGapRatioHeight();
		return (double)(UpWidth - DownWidth) / (2.0 * num);
	}

	public RectangleF GetMinDrawingRect()
	{
		return new RectangleF(TopCenterPoint.X - MinWidth / 2f, TopCenterPoint.Y + GapRatio * Height, MinWidth, GetGapRatioHeight());
	}

	public RectangleF GetOuterDrawingRect()
	{
		float num = Math.Max(UpWidth, DownWidth);
		return new RectangleF(TopCenterPoint.X - num / 2f, TopCenterPoint.Y + GapRatio * Height, num, GetGapRatioHeight());
	}

	public RectangleF GetInnerDrawingRect()
	{
		float num = Math.Min(UpWidth, DownWidth);
		return new RectangleF(TopCenterPoint.X - num / 2f, TopCenterPoint.Y + GapRatio * Height, num, GetGapRatioHeight());
	}

	public RectangleF GetDownDrawingRect()
	{
		float num = Math.Max(MinWidth, DownWidth);
		return new RectangleF(TopCenterPoint.X - num / 2f, TopCenterPoint.Y + GapRatio * Height, num, GetGapRatioHeight());
	}

	public RectangleF GetUpDrawingRect()
	{
		float num = Math.Max(MinWidth, UpWidth);
		return new RectangleF(TopCenterPoint.X - num / 2f, TopCenterPoint.Y + GapRatio * Height, num, GetGapRatioHeight());
	}

	public RectangleF GetFullDrawingRect()
	{
		RectangleF result = GetOuterDrawingRect();
		if (m_series3D)
		{
			RectangleF minDrawingRect = GetMinDrawingRect();
			PointF c = new PointF(minDrawingRect.Right, minDrawingRect.Top);
			PointF pointF = new PointF(minDrawingRect.Right, minDrawingRect.Bottom);
			PointF c2 = new PointF(minDrawingRect.Left, minDrawingRect.Top);
			PointF pointF2 = new PointF(minDrawingRect.Left, minDrawingRect.Bottom);
			PointF b = new PointF(TopCenterPoint.X - UpWidth / 2f, minDrawingRect.Top);
			PointF a = new PointF(TopCenterPoint.X + UpWidth / 2f, minDrawingRect.Top);
			PointF b2 = new PointF(TopCenterPoint.X + DownWidth / 2f, minDrawingRect.Bottom);
			PointF a2 = new PointF(TopCenterPoint.X - DownWidth / 2f, minDrawingRect.Bottom);
			PointF pointF3 = ChartMath.LineSegmentIntersectionPoint(a, b2, c, pointF);
			PointF pointF4 = ChartMath.LineSegmentIntersectionPoint(a2, b, c2, pointF2);
			if (!pointF3.IsEmpty || b2.X < pointF.X)
			{
				b2 = pointF;
			}
			if (!pointF4.IsEmpty || a2.X > pointF2.X)
			{
				a2 = pointF2;
			}
			float num = m_offset3DRation * (a.X - b.X);
			float num2 = m_offset3DRation * (b2.X - a2.X);
			float height = GetGapRatioHeight() + (num + num2) / 2f;
			result = new RectangleF(result.X, result.Y - num / 2f, result.Width, height);
		}
		return result;
	}

	private bool IsWidding()
	{
		if (m_minWidth < Math.Max(m_upWidth, m_downWidth))
		{
			return m_minWidth > Math.Min(m_upWidth, m_downWidth);
		}
		return false;
	}

	private bool NeedTopSide()
	{
		if (m_series3D)
		{
			if (!m_topLevel)
			{
				return m_gapRatio > 0f;
			}
			return true;
		}
		return false;
	}

	static AccumulationChartsLayer()
	{
		ColorBlend colorBlend = new ColorBlend();
		ChartSeriesRenderer.PhongShadingColors(Color.FromArgb(96, Color.Black), Color.FromArgb(96, Color.Black), Color.FromArgb(100, Color.White), s_phongAngle, 30.0, out var colors, out var positions);
		colorBlend.Positions = positions;
		colorBlend.Colors = colors;
		s_colorBlend = colorBlend;
	}

	public AccumulationChartsLayer(int index, PointF topCenterPoint, float height, bool series3D, float offset3DRatio, ChartFunnelMode funnelmode)
		: this(index, topCenterPoint, 0f, 0f, height, 0f, series3D, offset3DRatio, funnelmode)
	{
	}

	public AccumulationChartsLayer(int index, PointF topCenterPoint, float upWidth, float downWidth, float height, float gapRatio, bool series3D, float offset3DRatio, ChartFunnelMode funnelmode)
	{
		m_topCenter = topCenterPoint;
		m_index = index;
		m_downWidth = downWidth;
		m_upWidth = upWidth;
		m_height = height;
		m_gapRatio = gapRatio;
		m_series3D = series3D;
		m_offset3DRation = offset3DRatio;
		m_funnelMode = funnelmode;
	}

	public Region Draw(Graphics g, BrushInfo brushInfo, Pen pen)
	{
		bool flag = IsWidding();
		bool flag2 = m_series3D && (TopLevel || GapRatio > 0f);
		float num = Math.Max(m_minWidth, m_upWidth) / 2f;
		float num2 = Math.Max(m_minWidth, m_downWidth) / 2f;
		float num3 = m_minWidth / 2f;
		float x = TopCenterPoint.X;
		float num4 = TopCenterPoint.Y + GapRatio * Height;
		float num5 = num4 + GetGapRatioHeight();
		float gapRatioHeight = GetGapRatioHeight();
		float num6 = num4 + gapRatioHeight * (m_upWidth - m_minWidth) / (m_upWidth - m_downWidth);
		float num7 = m_offset3DRation * num;
		float num8 = m_offset3DRation * num2;
		float num9 = m_offset3DRation * num3;
		GraphicsPath graphicsPath = new GraphicsPath();
		GraphicsPath graphicsPath2 = new GraphicsPath();
		GraphicsPath graphicsPath3 = new GraphicsPath();
		RectangleF rectangleF = new RectangleF(x - num, num4 - num7, 2f * num, 2f * num7);
		RectangleF rect = new RectangleF(x - num2, num5 - num8, 2f * num2, 2f * num8);
		if (m_figureBase == ChartFigureBase.Circle)
		{
			if (m_series3D && num > 0f && num7 > 0f)
			{
				graphicsPath.AddArc(rectangleF, 0f, 180f);
				if (flag2)
				{
					graphicsPath2.AddEllipse(rectangleF);
				}
			}
			else
			{
				graphicsPath.AddLine(x + num, num4, x - num, num4);
			}
			if (flag)
			{
				graphicsPath.AddLine(x - num, num4, x - num3, num6);
			}
			if (m_series3D && num2 > 0f && num8 > 0f)
			{
				graphicsPath.AddArc(rect, 180f, -180f);
			}
			else
			{
				graphicsPath.AddLine(x - num2, num5, x + num2, num5);
			}
			if (flag)
			{
				graphicsPath.AddLine(x + num2, num5, x + num3, num6);
			}
			graphicsPath.CloseFigure();
		}
		else if (m_series3D)
		{
			float num10 = (float)Math.Sin(m_rotationRation);
			float num11 = (float)Math.Cos(m_rotationRation);
			float num12 = Math.Abs(num11);
			num10 /= num12;
			num11 /= num12;
			PointF pointF = new PointF(x + num11 * num, num4);
			PointF pointF2 = new PointF(x + num10 * num, num4 + num11 * num7);
			PointF pointF3 = new PointF(x - num11 * num, num4 + num10 * num7);
			PointF pointF4 = new PointF(x - num10 * num, num4 - num11 * num7);
			PointF pointF5 = new PointF(x + num11 * num2, num5);
			PointF pointF6 = new PointF(x + num10 * num2, num5 + num11 * num8);
			PointF pointF7 = new PointF(x - num11 * num2, num5 + num10 * num8);
			if (flag2)
			{
				graphicsPath2.AddPolygon(new PointF[4] { pointF, pointF2, pointF3, pointF4 });
			}
			if (flag)
			{
				PointF pointF8 = new PointF(x + num11 * num3, num6 - num10 * num9);
				PointF pointF9 = new PointF(x + num10 * num3, num6 + num11 * num9);
				PointF pointF10 = new PointF(x - num11 * num3, num6 + num10 * num9);
				graphicsPath.AddPolygon(new PointF[6] { pointF3, pointF2, pointF9, pointF6, pointF7, pointF10 });
				graphicsPath.AddPolygon(new PointF[6] { pointF, pointF2, pointF9, pointF6, pointF5, pointF8 });
				graphicsPath3.AddPolygon(new PointF[6] { pointF, pointF2, pointF9, pointF6, pointF5, pointF8 });
			}
			else
			{
				graphicsPath.AddPolygon(new PointF[4] { pointF3, pointF2, pointF6, pointF7 });
				graphicsPath.AddPolygon(new PointF[4] { pointF, pointF2, pointF6, pointF5 });
				graphicsPath3.AddPolygon(new PointF[4] { pointF, pointF2, pointF6, pointF5 });
			}
		}
		else if (flag)
		{
			graphicsPath.AddPolygon(new PointF[6]
			{
				new PointF(x - num, num4),
				new PointF(x + num, num4),
				new PointF(x + num3, num6),
				new PointF(x + num2, num5),
				new PointF(x - num2, num5),
				new PointF(x - num3, num6)
			});
		}
		else
		{
			graphicsPath.AddPolygon(new PointF[4]
			{
				new PointF(x - num, num4),
				new PointF(x + num, num4),
				new PointF(x + num2, num5),
				new PointF(x - num2, num5)
			});
		}
		BrushPaint.FillPath(g, graphicsPath, brushInfo);
		if (m_figureBase == ChartFigureBase.Circle)
		{
			if (m_series.ChartModel.ColorModel.AllowGradient)
			{
				RectangleF bounds = graphicsPath.GetBounds();
				if (bounds.Height > 0f && bounds.Width > 0f)
				{
					using LinearGradientBrush linearGradientBrush = new LinearGradientBrush(graphicsPath.GetBounds(), Color.Black, Color.White, LinearGradientMode.Horizontal);
					linearGradientBrush.InterpolationColors = s_colorBlend;
					g.FillPath(linearGradientBrush, graphicsPath);
				}
			}
		}
		else if (m_series3D)
		{
			using SolidBrush brush = new SolidBrush(Color.FromArgb(160, Color.Black));
			g.FillPath(brush, graphicsPath3);
		}
		g.DrawPath(pen, graphicsPath);
		if (flag2)
		{
			BrushPaint.FillPath(g, graphicsPath2, brushInfo);
			g.DrawPath(pen, graphicsPath2);
		}
		return null;
	}

	public Polygon[] Draw3D(BrushInfo brushInfo, Pen pen)
	{
		ArrayList arrayList = new ArrayList();
		int num = ((m_figureBase == ChartFigureBase.Circle) ? 18 : 4);
		double num2 = Math.PI * 2.0 / (double)num;
		bool flag = IsWidding();
		float num3 = m_minWidth / 2f;
		float num4 = Math.Max(m_upWidth, m_minWidth) / 2f;
		float num5 = Math.Max(m_downWidth, m_minWidth) / 2f;
		Vector3D vector3D = new Vector3D(m_topCenter.X, m_topCenter.Y + GapRatio * Height, m_depth);
		Vector3D vector3D2 = new Vector3D(vector3D.X, vector3D.Y + (double)GetGapRatioHeight(), vector3D.Z);
		Vector3D vector3D3 = Vector3D.Empty;
		if (flag)
		{
			vector3D3 = new Vector3D(vector3D.X, vector3D.Y + (double)(m_minWidth * Height / (m_upWidth - m_downWidth)), vector3D.Z);
		}
		Vector3D[] array = new Vector3D[num];
		Vector3D[] array2 = new Vector3D[num];
		Polygon[] array3 = new Polygon[num];
		Polygon[] array4 = (flag ? new Polygon[num] : null);
		for (int i = 0; i < num; i++)
		{
			double num6 = Math.Sin((double)i * num2);
			double num7 = Math.Cos((double)i * num2);
			double num8 = Math.Sin((double)(i + 1) * num2);
			double num9 = Math.Cos((double)(i + 1) * num2);
			Vector3D vector3D4 = vector3D + new Vector3D(num7 * (double)num4, 0.0, num6 * (double)num4);
			Vector3D vector3D5 = vector3D2 + new Vector3D(num7 * (double)num5, 0.0, num6 * (double)num5);
			Vector3D vector3D6 = vector3D2 + new Vector3D(num9 * (double)num5, 0.0, num8 * (double)num5);
			Vector3D vector3D7 = vector3D + new Vector3D(num9 * (double)num4, 0.0, num8 * (double)num4);
			if (flag)
			{
				Vector3D vector3D8 = vector3D3 + new Vector3D(num7 * (double)num3, 0.0, num6 * (double)num3);
				Vector3D vector3D9 = vector3D3 + new Vector3D(num9 * (double)num3, 0.0, num8 * (double)num3);
				array4[i] = new Polygon(new Vector3D[4] { vector3D4, vector3D8, vector3D9, vector3D7 }, brushInfo, null);
				array3[i] = new Polygon(new Vector3D[4] { vector3D8, vector3D5, vector3D6, vector3D9 }, brushInfo, null);
			}
			else
			{
				array3[i] = new Polygon(new Vector3D[4] { vector3D4, vector3D5, vector3D6, vector3D7 }, brushInfo, null);
			}
			array[i] = vector3D4;
			array2[i] = vector3D5;
		}
		arrayList.Add(new Polygon(array, brushInfo, pen));
		arrayList.Add(new Polygon(array2, brushInfo, pen));
		if (flag)
		{
			Vector3D vector3D10 = vector3D3 + new Vector3D(num3, 0.0, num3);
			Vector3D vector3D11 = vector3D3 + new Vector3D(0f - num3, 0.0, num3);
			Vector3D vector3D12 = vector3D3 + new Vector3D(0f - num3, 0.0, 0f - num3);
			Vector3D vector3D13 = vector3D3 + new Vector3D(num3, 0.0, 0f - num3);
			arrayList.Add(new Polygon(new Vector3D[4] { vector3D10, vector3D11, vector3D12, vector3D13 }));
			arrayList.AddRange(array4);
		}
		arrayList.AddRange(array3);
		return (Polygon[])arrayList.ToArray(typeof(Polygon));
	}
}
