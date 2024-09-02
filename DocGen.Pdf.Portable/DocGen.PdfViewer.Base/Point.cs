using System;
using DocGen.Drawing;

namespace DocGen.PdfViewer.Base;

internal struct Point : IFormattable
{
	internal double _x;

	internal double _y;

	public double X
	{
		get
		{
			return _x;
		}
		set
		{
			_x = value;
		}
	}

	public double Y
	{
		get
		{
			return _y;
		}
		set
		{
			_y = value;
		}
	}

	public static implicit operator DocGen.Drawing.Point(Point p)
	{
		return new DocGen.Drawing.Point((int)p.X, (int)p.Y);
	}

	public static implicit operator PointF(Point p)
	{
		return new PointF((float)p.X, (float)p.Y);
	}

	public static implicit operator Point(DocGen.Drawing.Point p)
	{
		return new Point(p.X, p.Y);
	}

	public static implicit operator Point(PointF p)
	{
		return new Point(p.X, p.Y);
	}

	public Point(double x, double y)
	{
		_x = x;
		_y = y;
	}

	public static Point Add(Point point, Vector vector)
	{
		return new Point(point._x + vector._x, point._y + vector._y);
	}

	public static bool Equals(Point point1, Point point2)
	{
		if (point1.X.Equals(point2.X))
		{
			return point1.Y.Equals(point2.Y);
		}
		return false;
	}

	public override bool Equals(object o)
	{
		if (o == null || !(o is Point point))
		{
			return false;
		}
		return Equals(this, point);
	}

	public bool Equals(Point value)
	{
		return Equals(this, value);
	}

	public override int GetHashCode()
	{
		double x = X;
		double y = Y;
		return x.GetHashCode() ^ y.GetHashCode();
	}

	public void Offset(double offsetX, double offsetY)
	{
		Point point = this;
		point._x += offsetX;
		Point point2 = this;
		point2._y += offsetY;
	}

	public static Point operator +(Point point, Vector vector)
	{
		return new Point(point._x + vector._x, point._y + vector._y);
	}

	public static bool operator ==(Point point1, Point point2)
	{
		if (point1.X == point2.X)
		{
			return point1.Y == point2.Y;
		}
		return false;
	}

	public static explicit operator Size(Point point)
	{
		return new Size(Math.Abs(point._x), Math.Abs(point._y));
	}

	public static explicit operator Vector(Point point)
	{
		return new Vector(point._x, point._y);
	}

	public static bool operator !=(Point point1, Point point2)
	{
		return !(point1 == point2);
	}

	public static Point operator -(Point point, Vector vector)
	{
		return new Point(point._x - vector._x, point._y - vector._y);
	}

	public static Vector operator -(Point point1, Point point2)
	{
		return new Vector(point1._x - point2._x, point1._y - point2._y);
	}

	public static Point Subtract(Point point, Vector vector)
	{
		return new Point(point._x - vector._x, point._y - vector._y);
	}

	public static Vector Subtract(Point point1, Point point2)
	{
		return new Vector(point1._x - point2._x, point1._y - point2._y);
	}

	string IFormattable.ToString(string format, IFormatProvider provider)
	{
		return ToString();
	}
}
