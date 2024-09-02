using System.ComponentModel;
using System.Globalization;
using DocGen.Drawing;

namespace DocGen.Chart;

[TypeConverter(typeof(ChartThicknessConverter))]
internal struct ChartThickness
{
	private float m_left;

	private float m_top;

	private float m_right;

	private float m_bottom;

	[Description("Gets the left.")]
	public float Left
	{
		get
		{
			return m_left;
		}
		set
		{
			m_left = value;
		}
	}

	[Description("Gets the top.")]
	public float Top
	{
		get
		{
			return m_top;
		}
		set
		{
			m_top = value;
		}
	}

	[Description("Gets the right.")]
	public float Right
	{
		get
		{
			return m_right;
		}
		set
		{
			m_right = value;
		}
	}

	[Description("Gets the bottom.")]
	public float Bottom
	{
		get
		{
			return m_bottom;
		}
		set
		{
			m_bottom = value;
		}
	}

	public ChartThickness(float value)
	{
		m_top = value;
		m_left = value;
		m_right = value;
		m_bottom = value;
	}

	public ChartThickness(float left, float top, float right, float bottom)
	{
		m_top = top;
		m_left = left;
		m_right = right;
		m_bottom = bottom;
	}

	public static bool operator ==(ChartThickness x, ChartThickness y)
	{
		if (x.m_top == y.m_top && x.m_left == y.m_left && x.m_bottom == y.m_bottom)
		{
			return x.m_right == y.m_right;
		}
		return false;
	}

	public static bool operator !=(ChartThickness x, ChartThickness y)
	{
		if (x.m_top == y.m_top && x.m_left == y.m_left && x.m_bottom == y.m_bottom)
		{
			return x.m_right != y.m_right;
		}
		return true;
	}

	public static ChartThickness Add(ChartThickness x, ChartThickness y)
	{
		return new ChartThickness(x.m_left + y.m_left, x.m_top + y.m_top, x.m_right + y.m_right, x.m_bottom + y.m_bottom);
	}

	public Rectangle Inflate(Rectangle rect)
	{
		rect.X -= (int)m_left;
		rect.Y -= (int)m_top;
		rect.Width += (int)(m_left + m_right);
		rect.Height += (int)(m_top + m_bottom);
		return rect;
	}

	public Size Inflate(Size size)
	{
		size.Width += (int)(m_left + m_right);
		size.Height += (int)(m_top + m_bottom);
		return size;
	}

	public RectangleF Inflate(RectangleF rect)
	{
		rect.X -= m_left;
		rect.Y -= m_top;
		rect.Width += m_left + m_right;
		rect.Height += m_top + m_bottom;
		return rect;
	}

	public SizeF Inflate(SizeF size)
	{
		size.Width += m_left + m_right;
		size.Height += m_top + m_bottom;
		return size;
	}

	public Rectangle Deflate(Rectangle rect)
	{
		rect.X += (int)m_left;
		rect.Y += (int)m_top;
		rect.Width -= (int)(m_left + m_right);
		rect.Height -= (int)(m_top + m_bottom);
		return rect;
	}

	public RectangleF Deflate(RectangleF rect)
	{
		rect.X += m_left;
		rect.Y += m_top;
		rect.Width -= m_left + m_right;
		rect.Height -= m_top + m_bottom;
		return rect;
	}

	public Size Deflate(Size size)
	{
		size.Width -= (int)(m_left + m_right);
		size.Height -= (int)(m_top + m_bottom);
		return size;
	}

	public SizeF Deflate(SizeF size)
	{
		size.Width -= m_left + m_right;
		size.Height -= m_top + m_bottom;
		return size;
	}

	public override bool Equals(object obj)
	{
		if (obj is ChartThickness chartThickness)
		{
			if (m_top == chartThickness.m_top && m_left == chartThickness.m_left && m_bottom == chartThickness.m_bottom)
			{
				return m_right == chartThickness.m_right;
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return m_top.GetHashCode() ^ m_left.GetHashCode() ^ m_right.GetHashCode() ^ m_bottom.GetHashCode();
	}

	public override string ToString()
	{
		return string.Join("; ", m_left.ToString(CultureInfo.InvariantCulture), m_top.ToString(CultureInfo.InvariantCulture), m_right.ToString(CultureInfo.InvariantCulture), m_bottom.ToString(CultureInfo.InvariantCulture));
	}

	public static ChartThickness Parse(string text)
	{
		string[] array = text.Split(';', ',');
		ChartThickness result = default(ChartThickness);
		float.TryParse(array[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out result.m_left);
		float.TryParse(array[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out result.m_top);
		float.TryParse(array[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out result.m_right);
		float.TryParse(array[3].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out result.m_bottom);
		return result;
	}
}
