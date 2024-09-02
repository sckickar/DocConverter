using DocGen.Drawing;

namespace DocGen.Pdf;

internal class OutlinePoint
{
	private PointF m_point;

	private byte m_flags;

	public PointF Point
	{
		get
		{
			return m_point;
		}
		set
		{
			m_point = value;
		}
	}

	public byte Flags
	{
		get
		{
			return m_flags;
		}
		set
		{
			m_flags = value;
		}
	}

	public bool IsOnCurve => (Flags & 1) != 0;

	public OutlinePoint(double x, double y, byte flags)
	{
		m_point = new PointF((float)x, (float)y);
		m_flags = flags;
	}

	public OutlinePoint(byte flags)
	{
		m_flags = flags;
	}
}
