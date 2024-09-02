using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal sealed class ChartLabel
{
	private SizeF m_size;

	private SizeF m_offset;

	private PointF m_connectPoint;

	private PointF m_symbolPoint;

	private RectangleF m_rect;

	public SizeF Size => m_size;

	public SizeF Offset => m_offset;

	public PointF ConnectPoint => m_connectPoint;

	public PointF SymbolPoint => m_symbolPoint;

	public RectangleF Rect
	{
		get
		{
			return m_rect;
		}
		set
		{
			m_rect = value;
		}
	}

	public ChartLabel(PointF connectPoint, PointF symbolPoint, SizeF size, SizeF offset)
	{
		m_symbolPoint = symbolPoint;
		m_connectPoint = connectPoint;
		m_size = size;
		m_offset = offset;
		m_rect = RectangleF.Empty;
	}

	public void DrawPointingLine(Graphics g, ChartStyleInfo style, ChartSeries mm_series)
	{
		if (m_rect.IsEmpty)
		{
			return;
		}
		PointF[] array = new PointF[8]
		{
			m_rect.Location,
			new PointF((m_rect.Right + m_rect.Left) / 2f, m_rect.Top),
			new PointF(m_rect.Right, m_rect.Top),
			new PointF(m_rect.Right, (m_rect.Top + m_rect.Bottom) / 2f),
			new PointF(m_rect.Right, m_rect.Bottom),
			new PointF((m_rect.Right + m_rect.Left) / 2f, m_rect.Bottom),
			new PointF(m_rect.Left, m_rect.Bottom),
			new PointF(m_rect.Left, (m_rect.Top + m_rect.Bottom) / 2f)
		};
		int num = 0;
		float num2 = float.MaxValue;
		for (int i = 0; i < array.Length; i++)
		{
			float num3 = ChartMath.DistanceBetweenPoints(m_symbolPoint, array[i]);
			if (num2 > num3)
			{
				num2 = num3;
				num = i;
			}
		}
		PointF pt = array[num];
		if (num == array.Length - 1)
		{
			_ = ref array[0];
		}
		else
		{
			_ = ref array[num + 1];
		}
		if (num == 0)
		{
			_ = ref array[^1];
		}
		else
		{
			_ = ref array[num - 1];
		}
		if (!m_rect.Contains(m_symbolPoint) && num2 > 10f)
		{
			Pen pen = new Pen(mm_series.SmartLabelsBorderColor, mm_series.SmartLabelsBorderWidth);
			g.DrawLine(pen, m_symbolPoint, pt);
			g.DrawRectangle(pen, Rectangle.Round(m_rect));
		}
	}
}
