using System.Globalization;

namespace DocGen.Drawing;

public struct PointF
{
	private float x;

	private float y;

	public static readonly PointF Empty;

	internal bool IsEmpty
	{
		get
		{
			if (x == 0f)
			{
				return y == 0f;
			}
			return false;
		}
	}

	public float X
	{
		get
		{
			return x;
		}
		set
		{
			x = value;
		}
	}

	public float Y
	{
		get
		{
			return y;
		}
		set
		{
			y = value;
		}
	}

	static PointF()
	{
		Empty = default(PointF);
	}

	public PointF(float x, float y)
	{
		this.x = x;
		this.y = y;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PointF pointF))
		{
			return false;
		}
		if (pointF.x == X)
		{
			return pointF.y == Y;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override string ToString()
	{
		return "{X=" + X.ToString(CultureInfo.CurrentCulture) + ",Y=" + Y.ToString(CultureInfo.CurrentCulture) + "}";
	}

	public static bool operator ==(PointF point1, PointF point2)
	{
		if (point1.X == point2.X)
		{
			return point1.Y == point2.Y;
		}
		return false;
	}

	public static implicit operator PointF(Point point)
	{
		return new PointF(point.X, point.Y);
	}

	public static bool operator !=(PointF point1, PointF point2)
	{
		return !(point1 == point2);
	}

	internal static PointF Add(PointF pt, Size sz)
	{
		return new PointF(pt.X + (float)sz.Width, pt.Y + (float)sz.Height);
	}

	internal static PointF Add(PointF pt, SizeF sz)
	{
		return new PointF(pt.X + sz.Width, pt.Y + sz.Height);
	}
}
