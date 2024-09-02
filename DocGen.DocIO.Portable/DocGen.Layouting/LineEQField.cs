using DocGen.Drawing;

namespace DocGen.Layouting;

internal class LineEQField : LayoutedEQFields
{
	private PointF m_point1;

	private PointF m_point2;

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
}
