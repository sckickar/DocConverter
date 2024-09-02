using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart.Renderers;

internal class AccumulationChartsLabel
{
	protected const float MAX_TEXT_WIDTH = 300f;

	private int m_index;

	private RectangleF m_rect;

	private RectangleF m_columnInRect;

	private ChartStyleInfo m_style;

	private ChartPoint m_point;

	private PointF m_connectPoint;

	private PointF m_layerConnectPoint;

	private double m_value;

	private float m_maxTextWidth = 300f;

	private AccumulationChartsLayer m_layer;

	private AccumulationChartsLabelAttachMode m_attachMode;

	private ChartAccumulationLabelPlacement m_labelPlacement = ChartAccumulationLabelPlacement.Center;

	private ChartAccumulationLabelStyle m_labelStyle = ChartAccumulationLabelStyle.OutsideInColumn;

	private float m_verticalPadding;

	private float m_horizontalPadding = 4f;

	private ChartSeries m_series;

	private int m_labelIndex;

	private ChartAccumulationLabelPlacement m_changedLabelPlacement;

	public int Index => m_index;

	public RectangleF Rectangle
	{
		get
		{
			if (LabelStyle == ChartAccumulationLabelStyle.Disabled)
			{
				return RectangleF.Empty;
			}
			return m_rect;
		}
		set
		{
			m_rect = value;
		}
	}

	public ChartStyleInfo Style => m_style;

	public ChartPoint Point => m_point;

	public PointF ConnetcPoint
	{
		get
		{
			return m_connectPoint;
		}
		set
		{
			m_connectPoint = value;
		}
	}

	public PointF NotCorrectPoint
	{
		get
		{
			AccumulationChartsLabelAttachMode attachMode = AttachMode;
			m_layerConnectPoint = PointF.Empty;
			RectangleF rectangleF = RectangleF.Empty;
			switch (attachMode)
			{
			case AccumulationChartsLabelAttachMode.Top:
				rectangleF = m_layer.GetUpDrawingRect();
				rectangleF.Height = 0f;
				break;
			case AccumulationChartsLabelAttachMode.Bottom:
				rectangleF = m_layer.GetDownDrawingRect();
				rectangleF.Y += rectangleF.Height;
				rectangleF.Height = 0f;
				break;
			case AccumulationChartsLabelAttachMode.Center:
			{
				RectangleF upDrawingRect = m_layer.GetUpDrawingRect();
				RectangleF downDrawingRect = m_layer.GetDownDrawingRect();
				rectangleF = new RectangleF((upDrawingRect.X + downDrawingRect.X) / 2f, (upDrawingRect.Top + downDrawingRect.Bottom) / 2f, (upDrawingRect.Width + downDrawingRect.Width) / 2f, 0f);
				break;
			}
			}
			RectangleF rectangleF2 = rectangleF;
			if (m_changedLabelPlacement == ChartAccumulationLabelPlacement.Left)
			{
				m_layerConnectPoint = new PointF(rectangleF2.Left, rectangleF2.Top);
			}
			else if (m_changedLabelPlacement == ChartAccumulationLabelPlacement.Right)
			{
				m_layerConnectPoint = new PointF(rectangleF2.Right, rectangleF2.Top);
			}
			return m_layerConnectPoint;
		}
	}

	public double Value
	{
		get
		{
			return m_value;
		}
		set
		{
			m_value = value;
		}
	}

	public float MaxTextWidth
	{
		get
		{
			return m_maxTextWidth;
		}
		set
		{
			m_maxTextWidth = value;
		}
	}

	public AccumulationChartsLayer Layer => m_layer;

	public AccumulationChartsLabelAttachMode AttachMode => m_attachMode;

	public ChartAccumulationLabelPlacement LabelPlacement
	{
		get
		{
			return m_labelPlacement;
		}
		set
		{
			m_labelPlacement = value;
		}
	}

	public ChartAccumulationLabelStyle LabelStyle
	{
		get
		{
			return m_labelStyle;
		}
		set
		{
			m_labelStyle = value;
			CalcLocation(m_columnInRect);
		}
	}

	internal bool AllowYOffset
	{
		get
		{
			if (LabelStyle != ChartAccumulationLabelStyle.OutsideInColumn)
			{
				return LabelStyle == ChartAccumulationLabelStyle.Disabled;
			}
			return true;
		}
	}

	private float VerticalPadding
	{
		get
		{
			return m_verticalPadding;
		}
		set
		{
			m_verticalPadding = value;
		}
	}

	private float HorizontalPadding
	{
		get
		{
			return m_horizontalPadding;
		}
		set
		{
			m_horizontalPadding = value;
		}
	}

	private ChartSeries Series
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

	private int LabelIndex
	{
		get
		{
			return m_labelIndex;
		}
		set
		{
			m_labelIndex = value;
		}
	}

	public AccumulationChartsLabel(int index, ChartPoint point, ChartStyleInfo style, AccumulationChartsLayer layer, AccumulationChartsLabelAttachMode attachMode)
	{
		m_index = index;
		m_point = point;
		m_style = style;
		m_rect = RectangleF.Empty;
		m_layer = layer;
		m_attachMode = attachMode;
		m_series = null;
		m_labelIndex = 0;
	}

	public AccumulationChartsLabel(int index, ChartPoint point, ChartStyleInfo style, AccumulationChartsLayer layer, AccumulationChartsLabelAttachMode attachMode, ChartSeries series, int LblIndex)
	{
		m_index = index;
		m_point = point;
		m_style = style;
		m_rect = RectangleF.Empty;
		m_layer = layer;
		m_attachMode = attachMode;
		m_series = series;
		m_labelIndex = LblIndex;
	}

	public SizeF CalcSize(Graphics g)
	{
		if (Series != null)
		{
			if (Series.Type == ChartSeriesType.Pyramid)
			{
				if (Series.ConfigItems.PyramidItem.ShowDataBindLabels && Series.XAxis.LabelsImpl != null)
				{
					string text = m_series.XAxis.LabelsImpl.GetLabelAt(LabelIndex).Text;
					Font font = m_series.XAxis.LabelsImpl.GetLabelAt(LabelIndex).Font;
					SizeF sizeF = g.MeasureString(text, font, (int)MaxTextWidth + 1);
					return m_rect.Size = new SizeF(sizeF.Width + 2f * HorizontalPadding, sizeF.Height + 2f * VerticalPadding);
				}
				SizeF sizeF3 = g.MeasureString(m_style.Text, m_style.GdipFont, (int)MaxTextWidth + 1);
				return m_rect.Size = new SizeF(sizeF3.Width + 2f * HorizontalPadding, sizeF3.Height + 2f * VerticalPadding);
			}
			if (Series.ConfigItems.FunnelItem.ShowDataBindLabels && Series.XAxis.LabelsImpl != null)
			{
				string text2 = m_series.XAxis.LabelsImpl.GetLabelAt(LabelIndex).Text;
				Font font2 = m_series.XAxis.LabelsImpl.GetLabelAt(LabelIndex).Font;
				SizeF sizeF5 = g.MeasureString(text2, font2, (int)MaxTextWidth + 1);
				return m_rect.Size = new SizeF(sizeF5.Width + 2f * HorizontalPadding, sizeF5.Height + 2f * VerticalPadding);
			}
			SizeF sizeF7 = g.MeasureString(m_style.Text, m_style.GdipFont, (int)MaxTextWidth + 1);
			return m_rect.Size = new SizeF(sizeF7.Width + 2f * HorizontalPadding, sizeF7.Height + 2f * VerticalPadding);
		}
		SizeF sizeF9 = g.MeasureString(m_style.Text, m_style.GdipFont, (int)MaxTextWidth + 1);
		return m_rect.Size = new SizeF(sizeF9.Width + 2f * HorizontalPadding, sizeF9.Height + 2f * VerticalPadding);
	}

	public PointF CalcLocation(RectangleF columnInRect)
	{
		m_columnInRect = columnInRect;
		PointF pointF = PointF.Empty;
		m_layerConnectPoint = PointF.Empty;
		m_connectPoint = PointF.Empty;
		AccumulationChartsLabelAttachMode attachMode = AttachMode;
		ChartAccumulationLabelStyle labelStyle = LabelStyle;
		m_changedLabelPlacement = LabelPlacement;
		if ((labelStyle == ChartAccumulationLabelStyle.OutsideInColumn || labelStyle == ChartAccumulationLabelStyle.Outside) && m_changedLabelPlacement != ChartAccumulationLabelPlacement.Left && m_changedLabelPlacement != ChartAccumulationLabelPlacement.Right)
		{
			m_changedLabelPlacement = ChartAccumulationLabelPlacement.Right;
		}
		if (LabelStyle != 0)
		{
			RectangleF rectangleF = RectangleF.Empty;
			switch (attachMode)
			{
			case AccumulationChartsLabelAttachMode.Top:
				rectangleF = Layer.GetUpDrawingRect();
				rectangleF.Height = 0f;
				if (Layer.Series3D && labelStyle == ChartAccumulationLabelStyle.Inside)
				{
					rectangleF = Layer.GetFullDrawingRect();
					rectangleF.Y = rectangleF.Y + rectangleF.Height - Layer.GetGapRatioHeight();
					rectangleF.Height = 0f;
				}
				break;
			case AccumulationChartsLabelAttachMode.Bottom:
				rectangleF = m_layer.GetDownDrawingRect();
				rectangleF.Y += rectangleF.Height;
				rectangleF.Height = 0f;
				if (Layer.Series3D && labelStyle == ChartAccumulationLabelStyle.Inside)
				{
					rectangleF = Layer.GetFullDrawingRect();
					rectangleF.Y += rectangleF.Height;
					rectangleF.Height = 0f;
				}
				break;
			case AccumulationChartsLabelAttachMode.Center:
			{
				RectangleF upDrawingRect = m_layer.GetUpDrawingRect();
				RectangleF downDrawingRect = m_layer.GetDownDrawingRect();
				rectangleF = new RectangleF((upDrawingRect.X + downDrawingRect.X) / 2f, (upDrawingRect.Top + downDrawingRect.Bottom) / 2f, (upDrawingRect.Width + downDrawingRect.Width) / 2f, 0f);
				if (Layer.Series3D && labelStyle == ChartAccumulationLabelStyle.Inside)
				{
					RectangleF fullDrawingRect = Layer.GetFullDrawingRect();
					fullDrawingRect.Y = fullDrawingRect.Y + fullDrawingRect.Height - Layer.GetGapRatioHeight() / 2f;
					rectangleF.Y = fullDrawingRect.Y;
				}
				break;
			}
			}
			switch (labelStyle)
			{
			case ChartAccumulationLabelStyle.Inside:
				rectangleF.X += rectangleF.Width / 2f;
				rectangleF.Y += rectangleF.Height / 2f;
				rectangleF.Width = 0f;
				rectangleF.Height = 0f;
				break;
			case ChartAccumulationLabelStyle.OutsideInColumn:
				rectangleF = new RectangleF(columnInRect.X, rectangleF.Y, columnInRect.Width, rectangleF.Height);
				break;
			}
			if (m_changedLabelPlacement == ChartAccumulationLabelPlacement.Center)
			{
				pointF = new PointF(rectangleF.X + rectangleF.Width / 2f - m_rect.Size.Width / 2f, rectangleF.Y + rectangleF.Height / 2f - m_rect.Size.Height / 2f);
			}
			else if (m_changedLabelPlacement == ChartAccumulationLabelPlacement.Top)
			{
				pointF = new PointF(rectangleF.X + rectangleF.Width / 2f - m_rect.Size.Width / 2f, rectangleF.Y + rectangleF.Height / 2f - m_rect.Size.Height);
			}
			else if (m_changedLabelPlacement == ChartAccumulationLabelPlacement.Bottom)
			{
				pointF = new PointF(rectangleF.X + rectangleF.Width / 2f - m_rect.Size.Width / 2f, rectangleF.Y + rectangleF.Height / 2f);
			}
			else if (m_changedLabelPlacement == ChartAccumulationLabelPlacement.Left)
			{
				pointF = new PointF(rectangleF.Left - m_rect.Size.Width, rectangleF.Y + rectangleF.Height / 2f - m_rect.Size.Height / 2f);
			}
			else if (m_changedLabelPlacement == ChartAccumulationLabelPlacement.Right)
			{
				pointF = new PointF(rectangleF.Right, rectangleF.Y + rectangleF.Height / 2f - m_rect.Size.Height / 2f);
			}
			m_rect.Location = pointF;
		}
		return pointF;
	}

	public void GetConnectioLinePoints(out PointF p1, out PointF p2)
	{
		p1 = PointF.Empty;
		p2 = PointF.Empty;
		if (LabelStyle != ChartAccumulationLabelStyle.Inside && LabelStyle != ChartAccumulationLabelStyle.Outside)
		{
			CalcConnectionPoint();
			p1 = m_connectPoint;
			p2 = NotCorrectPoint;
		}
	}

	public void TryToAvoidRectangleIntersection(RectangleF r1)
	{
		bool flag = true;
		while (flag)
		{
			RectangleF rectangle = Rectangle;
			if (LabelStyle == ChartAccumulationLabelStyle.OutsideInColumn)
			{
				if (!r1.IsEmpty && rectangle.IntersectsWith(r1))
				{
					m_rect.Location = ((m_series.Type == ChartSeriesType.Pyramid) ? new PointF(rectangle.Left, r1.Bottom) : new PointF(rectangle.Left, r1.Top - rectangle.Height));
				}
			}
			else
			{
				if (LabelStyle == ChartAccumulationLabelStyle.Outside)
				{
					LabelStyle = ChartAccumulationLabelStyle.OutsideInColumn;
					continue;
				}
				if (LabelStyle == ChartAccumulationLabelStyle.Inside && !r1.IsEmpty && rectangle.IntersectsWith(r1))
				{
					LabelStyle = ChartAccumulationLabelStyle.OutsideInColumn;
					continue;
				}
			}
			flag = false;
		}
	}

	public void TryToAvoidLineIntersection(PointF p1, PointF p2)
	{
		if (LabelStyle == ChartAccumulationLabelStyle.Outside)
		{
			LabelStyle = ChartAccumulationLabelStyle.OutsideInColumn;
		}
	}

	public void Draw(Graphics g)
	{
		if (LabelStyle == ChartAccumulationLabelStyle.Disabled)
		{
			return;
		}
		if (LabelStyle != ChartAccumulationLabelStyle.Inside && LabelStyle != ChartAccumulationLabelStyle.Outside)
		{
			CalcConnectionPoint();
			g.DrawLine(Style.GdipPen, m_connectPoint, NotCorrectPoint);
		}
		Brush brush = new SolidBrush(Style.TextColor);
		RectangleF rectangle = Rectangle;
		RectangleF rectangle2 = new RectangleF(rectangle.Left + HorizontalPadding, rectangle.Top + VerticalPadding, rectangle.Width - 2f * HorizontalPadding, rectangle.Height - 2f * VerticalPadding);
		if (Series != null)
		{
			if (Series.ConfigItems.PyramidItem.ShowDataBindLabels && Series.XAxis.LabelsImpl != null)
			{
				string text = m_series.XAxis.LabelsImpl.GetLabelAt(LabelIndex).Text;
				Font font = m_series.XAxis.LabelsImpl.GetLabelAt(LabelIndex).Font;
				g.DrawString(text, font, brush, rectangle2);
			}
			else if (Series.ConfigItems.FunnelItem.ShowDataBindLabels && Series.XAxis.LabelsImpl != null)
			{
				string text2 = m_series.XAxis.LabelsImpl.GetLabelAt(LabelIndex).Text;
				Font font2 = m_series.XAxis.LabelsImpl.GetLabelAt(LabelIndex).Font;
				g.DrawString(text2, font2, brush, rectangle2);
			}
			else
			{
				g.DrawString(Style.Text, Style.GdipFont, brush, rectangle2);
			}
		}
		else
		{
			g.DrawString(Style.Text, Style.GdipFont, brush, rectangle2);
		}
		brush.Dispose();
	}

	public Polygon Draw3D()
	{
		Path3DCollect path3DCollect = null;
		if (LabelStyle != 0)
		{
			Brush br = new SolidBrush(Style.TextColor);
			RectangleF rectangle = Rectangle;
			RectangleF rectangleF = new RectangleF(rectangle.Left + HorizontalPadding, rectangle.Top + VerticalPadding, rectangle.Width - 2f * HorizontalPadding, rectangle.Height - 2f * VerticalPadding);
			new GraphicsPath();
			Pseudo3DText paths = new Pseudo3DText(Style.Text, Style.GdipFont, br, new Vector3D(rectangleF.X, rectangleF.Y, m_layer.DepthPosition));
			if (Series != null)
			{
				if (Series.ConfigItems.PyramidItem.ShowDataBindLabels && Series.XAxis.LabelsImpl != null)
				{
					string text = m_series.XAxis.LabelsImpl.GetLabelAt(LabelIndex).Text;
					Font font = m_series.XAxis.LabelsImpl.GetLabelAt(LabelIndex).Font;
					paths = new Pseudo3DText(text, font, br, new Vector3D(rectangleF.X, rectangleF.Y, m_layer.DepthPosition));
				}
				else if (Series.ConfigItems.FunnelItem.ShowDataBindLabels && Series.XAxis.LabelsImpl != null)
				{
					string text2 = m_series.XAxis.LabelsImpl.GetLabelAt(LabelIndex).Text;
					Font font2 = m_series.XAxis.LabelsImpl.GetLabelAt(LabelIndex).Font;
					paths = new Pseudo3DText(text2, font2, br, new Vector3D(rectangleF.X, rectangleF.Y, m_layer.DepthPosition));
				}
			}
			path3DCollect = new Path3DCollect(paths);
			if (LabelStyle != ChartAccumulationLabelStyle.Inside && LabelStyle != ChartAccumulationLabelStyle.Outside)
			{
				CalcConnectionPoint();
				Vector3D vector3D = new Vector3D(m_connectPoint.X, m_connectPoint.Y, m_layer.DepthPosition);
				Vector3D vector3D2 = new Vector3D(NotCorrectPoint.X, NotCorrectPoint.Y, m_layer.DepthPosition);
				Vector3D vector3D3 = new Vector3D(NotCorrectPoint.X, NotCorrectPoint.Y, m_layer.DepthPosition);
				path3DCollect.Add(new Polygon(new Vector3D[3] { vector3D, vector3D2, vector3D3 }, Style.GdipPen));
			}
		}
		return path3DCollect;
	}

	private PointF CalcConnectionPoint()
	{
		RectangleF rectangle = Rectangle;
		if (m_changedLabelPlacement == ChartAccumulationLabelPlacement.Left)
		{
			m_connectPoint = new PointF(rectangle.Right, rectangle.Top + rectangle.Height / 2f);
		}
		else if (m_changedLabelPlacement == ChartAccumulationLabelPlacement.Right)
		{
			m_connectPoint = new PointF(rectangle.Left, rectangle.Top + rectangle.Height / 2f);
		}
		return m_connectPoint;
	}
}
