using System;
using DocGen.Drawing;

namespace DocGen.PdfViewer.Base;

internal struct Rect : IFormattable
{
	internal double _x;

	internal double _y;

	internal double _width;

	internal double _height;

	private static readonly Rect s_empty;

	public double Bottom
	{
		get
		{
			if (!IsEmpty)
			{
				return _y + _height;
			}
			return double.NegativeInfinity;
		}
	}

	public Point BottomLeft => new Point(Left, Bottom);

	public Point BottomRight => new Point(Right, Bottom);

	public static Rect Empty => s_empty;

	public double Height
	{
		get
		{
			return _height;
		}
		set
		{
			if (IsEmpty)
			{
				throw new InvalidOperationException("Rect_CannotModifyEmptyRect");
			}
			if (value >= 0.0)
			{
				_height = value;
				return;
			}
			throw new ArgumentException("Size_HeightCannotBeNegative");
		}
	}

	public bool IsEmpty => _width < 0.0;

	public double Left => _x;

	public Point Location
	{
		get
		{
			return new Point(_x, _y);
		}
		set
		{
			if (!IsEmpty)
			{
				_x = value._x;
				_y = value._y;
				return;
			}
			throw new InvalidOperationException("Rect_CannotModifyEmptyRect");
		}
	}

	public double Right
	{
		get
		{
			if (!IsEmpty)
			{
				return _x + _width;
			}
			return double.NegativeInfinity;
		}
	}

	public Size Size
	{
		get
		{
			if (!IsEmpty)
			{
				return new Size(_width, _height);
			}
			return Size.Empty;
		}
		set
		{
			if (value.IsEmpty)
			{
				this = s_empty;
				return;
			}
			if (!IsEmpty)
			{
				_width = value._width;
				_height = value._height;
				return;
			}
			throw new InvalidOperationException("Rect_CannotModifyEmptyRect");
		}
	}

	public double Top => _y;

	public Point TopLeft => new Point(Left, Top);

	public Point TopRight => new Point(Right, Top);

	public double Width
	{
		get
		{
			return _width;
		}
		set
		{
			if (IsEmpty)
			{
				throw new InvalidOperationException("Rect_CannotModifyEmptyRect");
			}
			if (value >= 0.0)
			{
				_width = value;
				return;
			}
			throw new ArgumentException("Size_WidthCannotBeNegative");
		}
	}

	public double X
	{
		get
		{
			return _x;
		}
		set
		{
			if (!IsEmpty)
			{
				_x = value;
				return;
			}
			throw new InvalidOperationException("Rect_CannotModifyEmptyRect");
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
			if (!IsEmpty)
			{
				_y = value;
				return;
			}
			throw new InvalidOperationException("Rect_CannotModifyEmptyRect");
		}
	}

	public static implicit operator Rectangle(Rect p)
	{
		return new Rectangle((int)p.X, (int)p.Y, (int)p.Width, (int)p.Height);
	}

	public static implicit operator RectangleF(Rect p)
	{
		return new RectangleF((float)p.X, (float)p.Y, (float)p.Width, (float)p.Height);
	}

	static Rect()
	{
		s_empty = CreateEmptyRect();
	}

	public Rect(Point location, Size size)
	{
		if (!size.IsEmpty)
		{
			_x = location._x;
			_y = location._y;
			_width = size._width;
			_height = size._height;
		}
		else
		{
			this = s_empty;
		}
	}

	public Rect(double x, double y, double width, double height)
	{
		if (width < 0.0 || height < 0.0)
		{
			throw new ArgumentException("Size_WidthAndHeightCannotBeNegative");
		}
		_x = x;
		_y = y;
		_width = width;
		_height = height;
	}

	public Rect(Point point1, Point point2)
	{
		_x = Math.Min(point1._x, point2._x);
		_y = Math.Min(point1._y, point2._y);
		_width = Math.Max(Math.Max(point1._x, point2._x) - _x, 0.0);
		_height = Math.Max(Math.Max(point1._y, point2._y) - _y, 0.0);
	}

	public Rect(Size size)
	{
		if (!size.IsEmpty)
		{
			_x = (_y = 0.0);
			_width = size.Width;
			_height = size.Height;
		}
		else
		{
			this = s_empty;
		}
	}

	public bool Contains(Point point)
	{
		return Contains(point._x, point._y);
	}

	public bool Contains(double x, double y)
	{
		if (!IsEmpty)
		{
			return ContainsInternal(x, y);
		}
		return false;
	}

	public bool Contains(Rect rect)
	{
		if (!IsEmpty && !rect.IsEmpty && _x <= rect._x && _y <= rect._y && _x + _width >= rect._x + rect._width)
		{
			return _y + _height >= rect._y + rect._height;
		}
		return false;
	}

	private bool ContainsInternal(double x, double y)
	{
		if (x >= _x && x - _width <= _x && y >= _y)
		{
			return y - _height <= _y;
		}
		return false;
	}

	private static Rect CreateEmptyRect()
	{
		Rect result = default(Rect);
		result._x = double.PositiveInfinity;
		result._y = double.PositiveInfinity;
		result._width = double.NegativeInfinity;
		result._height = double.NegativeInfinity;
		return result;
	}

	public static bool Equals(Rect rect1, Rect rect2)
	{
		if (!rect1.IsEmpty)
		{
			if (rect1.X.Equals(rect2.X) && rect1.Y.Equals(rect2.Y) && rect1.Width.Equals(rect2.Width))
			{
				return rect1.Height.Equals(rect2.Height);
			}
			return false;
		}
		return rect2.IsEmpty;
	}

	public override bool Equals(object o)
	{
		if (o == null || !(o is Rect rect))
		{
			return false;
		}
		return Equals(this, rect);
	}

	public bool Equals(Rect value)
	{
		return Equals(this, value);
	}

	public override int GetHashCode()
	{
		if (!IsEmpty)
		{
			double x = X;
			double y = Y;
			double width = Width;
			double height = Height;
			return x.GetHashCode() ^ y.GetHashCode() ^ width.GetHashCode() ^ height.GetHashCode();
		}
		return 0;
	}

	public void Inflate(Size size)
	{
		Inflate(size._width, size._height);
	}

	public void Inflate(double width, double height)
	{
		if (!IsEmpty)
		{
			Rect rect = this;
			rect._x -= width;
			Rect rect2 = this;
			rect2._y -= height;
			Rect rect3 = this;
			rect3._width += width;
			Rect rect4 = this;
			rect4._width += width;
			Rect rect5 = this;
			rect5._height += height;
			Rect rect6 = this;
			rect6._height += height;
			if (_width < 0.0 || _height < 0.0)
			{
				this = s_empty;
			}
			return;
		}
		throw new InvalidOperationException("Rect_CannotCallMethod");
	}

	public static Rect Inflate(Rect rect, Size size)
	{
		rect.Inflate(size._width, size._height);
		return rect;
	}

	public static Rect Inflate(Rect rect, double width, double height)
	{
		rect.Inflate(width, height);
		return rect;
	}

	public void Intersect(Rect rect)
	{
		if (IntersectsWith(rect))
		{
			double num = Math.Max(Left, rect.Left);
			double num2 = Math.Max(Top, rect.Top);
			_width = Math.Max(Math.Min(Right, rect.Right) - num, 0.0);
			_height = Math.Max(Math.Min(Bottom, rect.Bottom) - num2, 0.0);
			_x = num;
			_y = num2;
		}
		else
		{
			this = Empty;
		}
	}

	public static Rect Intersect(Rect rect1, Rect rect2)
	{
		rect1.Intersect(rect2);
		return rect1;
	}

	public bool IntersectsWith(Rect rect)
	{
		if (!IsEmpty && !rect.IsEmpty && rect.Left <= Right && rect.Right >= Left && rect.Top <= Bottom)
		{
			return rect.Bottom >= Top;
		}
		return false;
	}

	public void Offset(Vector offsetVector)
	{
		if (!IsEmpty)
		{
			Rect rect = this;
			rect._x += offsetVector._x;
			Rect rect2 = this;
			rect2._y += offsetVector._y;
			return;
		}
		throw new InvalidOperationException("Rect_CannotCallMethod");
	}

	public void Offset(double offsetX, double offsetY)
	{
		if (!IsEmpty)
		{
			Rect rect = this;
			rect._x += offsetX;
			Rect rect2 = this;
			rect2._y += offsetY;
			return;
		}
		throw new InvalidOperationException("Rect_CannotCallMethod");
	}

	public static Rect Offset(Rect rect, Vector offsetVector)
	{
		rect.Offset(offsetVector.X, offsetVector.Y);
		return rect;
	}

	public static Rect Offset(Rect rect, double offsetX, double offsetY)
	{
		rect.Offset(offsetX, offsetY);
		return rect;
	}

	public static bool operator ==(Rect rect1, Rect rect2)
	{
		if (rect1.X == rect2.X && rect1.Y == rect2.Y && rect1.Width == rect2.Width)
		{
			return rect1.Height == rect2.Height;
		}
		return false;
	}

	public static bool operator !=(Rect rect1, Rect rect2)
	{
		return !(rect1 == rect2);
	}

	public void Scale(double scaleX, double scaleY)
	{
		if (!IsEmpty)
		{
			_x *= scaleX;
			_y *= scaleY;
			_width *= scaleX;
			_height *= scaleY;
			if (scaleX < 0.0)
			{
				_x += _width;
				_width *= -1.0;
			}
			if (scaleY < 0.0)
			{
				_y += _height;
				_height *= -1.0;
			}
		}
	}

	string IFormattable.ToString(string format, IFormatProvider provider)
	{
		return ToString();
	}

	public void Union(Rect rect)
	{
		if (!IsEmpty)
		{
			if (!rect.IsEmpty)
			{
				double num = Math.Min(Left, rect.Left);
				double num2 = Math.Min(Top, rect.Top);
				if (rect.Width == double.PositiveInfinity || Width == double.PositiveInfinity)
				{
					_width = double.PositiveInfinity;
				}
				else
				{
					double num3 = Math.Max(Right, rect.Right);
					_width = Math.Max(num3 - num, 0.0);
				}
				if (rect.Height == double.PositiveInfinity || Height == double.PositiveInfinity)
				{
					_height = double.PositiveInfinity;
				}
				else
				{
					double num4 = Math.Max(Bottom, rect.Bottom);
					_height = Math.Max(num4 - num2, 0.0);
				}
				_x = num;
				_y = num2;
			}
		}
		else
		{
			this = rect;
		}
	}

	public static Rect Union(Rect rect1, Rect rect2)
	{
		rect1.Union(rect2);
		return rect1;
	}

	public void Union(Point point)
	{
		Union(new Rect(point, point));
	}

	public static Rect Union(Rect rect, Point point)
	{
		rect.Union(new Rect(point, point));
		return rect;
	}
}
