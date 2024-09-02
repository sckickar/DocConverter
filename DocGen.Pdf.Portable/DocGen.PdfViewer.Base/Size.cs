using System;

namespace DocGen.PdfViewer.Base;

internal struct Size : IFormattable
{
	internal double _width;

	internal double _height;

	private static readonly Size s_empty;

	public static Size Empty => s_empty;

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
				throw new InvalidOperationException("Size_CannotModifyEmptySize");
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
				throw new InvalidOperationException("Size_CannotModifyEmptySize");
			}
			if (value >= 0.0)
			{
				_width = value;
				return;
			}
			throw new ArgumentException("Size_WidthCannotBeNegative");
		}
	}

	static Size()
	{
		s_empty = CreateEmptySize();
	}

	public Size(double width, double height)
	{
		if (width < 0.0 || height < 0.0)
		{
			throw new ArgumentException("Size_WidthAndHeightCannotBeNegative");
		}
		_width = width;
		_height = height;
	}

	private static Size CreateEmptySize()
	{
		Size result = default(Size);
		result._width = double.NegativeInfinity;
		result._height = double.NegativeInfinity;
		return result;
	}

	public static bool Equals(Size size1, Size size2)
	{
		if (!size1.IsEmpty)
		{
			if (size1.Width.Equals(size2.Width))
			{
				return size1.Height.Equals(size2.Height);
			}
			return false;
		}
		return size2.IsEmpty;
	}

	public override bool Equals(object o)
	{
		if (o == null || !(o is Size size))
		{
			return false;
		}
		return Equals(this, size);
	}

	public bool Equals(Size value)
	{
		return Equals(this, value);
	}

	public override int GetHashCode()
	{
		if (!IsEmpty)
		{
			double width = Width;
			double height = Height;
			return width.GetHashCode() ^ height.GetHashCode();
		}
		return 0;
	}

	public static bool operator ==(Size size1, Size size2)
	{
		if (size1.Width == size2.Width)
		{
			return size1.Height == size2.Height;
		}
		return false;
	}

	public static explicit operator Vector(Size size)
	{
		return new Vector(size._width, size._height);
	}

	public static explicit operator Point(Size size)
	{
		return new Point(size._width, size._height);
	}

	public static bool operator !=(Size size1, Size size2)
	{
		return !(size1 == size2);
	}

	string IFormattable.ToString(string format, IFormatProvider provider)
	{
		return ToString();
	}
}
