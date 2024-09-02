using System;

namespace DocGen.PdfViewer.Base;

internal struct Vector : IFormattable
{
	internal double _x;

	internal double _y;

	public double Length => Math.Sqrt(_x * _x + _y * _y);

	public double LengthSquared => _x * _x + _y * _y;

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

	public Vector(double x, double y)
	{
		_x = x;
		_y = y;
	}

	public static Vector Add(Vector vector1, Vector vector2)
	{
		return new Vector(vector1._x + vector2._x, vector1._y + vector2._y);
	}

	public static Point Add(Vector vector, Point point)
	{
		return new Point(point._x + vector._x, point._y + vector._y);
	}

	public static double AngleBetween(Vector vector1, Vector vector2)
	{
		double y = vector1._x * vector2._y - vector2._x * vector1._y;
		double x = vector1._x * vector2._x + vector1._y * vector2._y;
		return Math.Atan2(y, x) * 57.2957795130823;
	}

	public static double CrossProduct(Vector vector1, Vector vector2)
	{
		return vector1._x * vector2._y - vector1._y * vector2._x;
	}

	public static double Determinant(Vector vector1, Vector vector2)
	{
		return vector1._x * vector2._y - vector1._y * vector2._x;
	}

	public static Vector Divide(Vector vector, double scalar)
	{
		return vector * 1.0 / scalar;
	}

	public static bool Equals(Vector vector1, Vector vector2)
	{
		if (vector1.X.Equals(vector2.X))
		{
			return vector1.Y.Equals(vector2.Y);
		}
		return false;
	}

	public override bool Equals(object o)
	{
		if (o == null || !(o is Vector vector))
		{
			return false;
		}
		return Equals(this, vector);
	}

	public bool Equals(Vector value)
	{
		return Equals(this, value);
	}

	public override int GetHashCode()
	{
		double x = X;
		double y = Y;
		return x.GetHashCode() ^ y.GetHashCode();
	}

	public static Vector Multiply(Vector vector, double scalar)
	{
		return new Vector(vector._x * scalar, vector._y * scalar);
	}

	public static Vector Multiply(double scalar, Vector vector)
	{
		return new Vector(vector._x * scalar, vector._y * scalar);
	}

	public static double Multiply(Vector vector1, Vector vector2)
	{
		return vector1._x * vector2._x + vector1._y * vector2._y;
	}

	public void Negate()
	{
		_x = 0.0 - _x;
		_y = 0.0 - _y;
	}

	public static Vector operator +(Vector vector1, Vector vector2)
	{
		return new Vector(vector1._x + vector2._x, vector1._y + vector2._y);
	}

	public static Point operator +(Vector vector, Point point)
	{
		return new Point(point._x + vector._x, point._y + vector._y);
	}

	public static Vector operator /(Vector vector, double scalar)
	{
		return vector * 1.0 / scalar;
	}

	public static bool operator ==(Vector vector1, Vector vector2)
	{
		if (vector1.X == vector2.X)
		{
			return vector1.Y == vector2.Y;
		}
		return false;
	}

	public static explicit operator Size(Vector vector)
	{
		return new Size(Math.Abs(vector._x), Math.Abs(vector._y));
	}

	public static explicit operator Point(Vector vector)
	{
		return new Point(vector._x, vector._y);
	}

	public static bool operator !=(Vector vector1, Vector vector2)
	{
		return !(vector1 == vector2);
	}

	public static Vector operator *(Vector vector, double scalar)
	{
		return new Vector(vector._x * scalar, vector._y * scalar);
	}

	public static Vector operator *(double scalar, Vector vector)
	{
		return new Vector(vector._x * scalar, vector._y * scalar);
	}

	public static double operator *(Vector vector1, Vector vector2)
	{
		return vector1._x * vector2._x + vector1._y * vector2._y;
	}

	public static Vector operator -(Vector vector1, Vector vector2)
	{
		return new Vector(vector1._x - vector2._x, vector1._y - vector2._y);
	}

	public static Vector operator -(Vector vector)
	{
		return new Vector(0.0 - vector._x, 0.0 - vector._y);
	}

	public static Vector Subtract(Vector vector1, Vector vector2)
	{
		return new Vector(vector1._x - vector2._x, vector1._y - vector2._y);
	}

	string IFormattable.ToString(string format, IFormatProvider formatProvider)
	{
		return ToString();
	}
}
