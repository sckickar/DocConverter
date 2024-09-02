using DocGen.Drawing;

namespace DocGen.Layouting;

internal class LayoutedLineWidget
{
	private PointF m_point1;

	private PointF m_point2;

	private Color m_color;

	private float m_width;

	private bool m_skip;

	internal PointF Point1
	{
		get
		{
			return m_point1;
		}
		set
		{
			m_point1 = value;
		}
	}

	internal PointF Point2
	{
		get
		{
			return m_point2;
		}
		set
		{
			m_point2 = value;
		}
	}

	internal Color Color
	{
		get
		{
			return m_color;
		}
		set
		{
			m_color = value;
		}
	}

	internal float Width
	{
		get
		{
			return m_width;
		}
		set
		{
			m_width = value;
		}
	}

	internal bool Skip
	{
		get
		{
			return m_skip;
		}
		set
		{
			m_skip = value;
		}
	}

	internal LayoutedLineWidget()
	{
	}

	internal LayoutedLineWidget(LayoutedLineWidget srcWidget)
	{
		Point1 = srcWidget.Point1;
		Point2 = srcWidget.Point2;
		Width = srcWidget.Width;
		Skip = srcWidget.Skip;
	}

	internal void ShiftXYPosition(float xPosition, float yPosition)
	{
		Point1 = new PointF(Point1.X + xPosition, Point1.Y + yPosition);
		Point2 = new PointF(Point2.X + xPosition, Point2.Y + yPosition);
	}

	public void Dispose()
	{
	}
}
