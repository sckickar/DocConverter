using System;
using DocGen.Drawing;

namespace DocGen.Layouting;

internal class UnitsConvertor
{
	internal const int STANDART_DPI = 96;

	private double[] m_Proportions;

	[ThreadStatic]
	private static UnitsConvertor m_instance;

	private double[] Proportions
	{
		get
		{
			if (m_Proportions == null)
			{
				InitDefProporsions();
			}
			return m_Proportions;
		}
	}

	public static UnitsConvertor Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = new UnitsConvertor();
			}
			return m_instance;
		}
	}

	public UnitsConvertor()
	{
		InitDefProporsions();
	}

	public double ConvertUnits(double value, PrintUnits from, PrintUnits to)
	{
		if (from == to)
		{
			return value;
		}
		return ConvertFromPixels(ConvertToPixels(value, from), to);
	}

	public float ConvertToPixels(float value, PrintUnits from)
	{
		if (from == PrintUnits.Pixel)
		{
			return value;
		}
		return (float)((double)value * m_Proportions[(int)from]);
	}

	public double ConvertToPixels(double value, PrintUnits from)
	{
		if (from == PrintUnits.Pixel)
		{
			return value;
		}
		return value * m_Proportions[(int)from];
	}

	public RectangleF ConvertToPixels(RectangleF rect, PrintUnits from)
	{
		float x = ConvertToPixels(rect.X, from);
		float y = ConvertToPixels(rect.Y, from);
		float width = ConvertToPixels(rect.Width, from);
		float height = ConvertToPixels(rect.Height, from);
		return new RectangleF(x, y, width, height);
	}

	public PointF ConvertToPixels(PointF point, PrintUnits from)
	{
		float x = ConvertToPixels(point.X, from);
		float y = ConvertToPixels(point.Y, from);
		return new PointF(x, y);
	}

	public SizeF ConvertToPixels(SizeF size, PrintUnits from)
	{
		float width = ConvertToPixels(size.Width, from);
		float height = ConvertToPixels(size.Height, from);
		return new SizeF(width, height);
	}

	public float ConvertFromPixels(float value, PrintUnits to)
	{
		if (to == PrintUnits.Pixel)
		{
			return value;
		}
		return (float)((double)value / m_Proportions[(int)to]);
	}

	public double ConvertFromPixels(double value, PrintUnits to)
	{
		if (to == PrintUnits.Pixel)
		{
			return value;
		}
		return value / m_Proportions[(int)to];
	}

	public RectangleF ConvertFromPixels(RectangleF rect, PrintUnits to)
	{
		float x = ConvertFromPixels(rect.X, to);
		float y = ConvertFromPixels(rect.Y, to);
		float width = ConvertFromPixels(rect.Width, to);
		float height = ConvertFromPixels(rect.Height, to);
		return new RectangleF(x, y, width, height);
	}

	public PointF ConvertFromPixels(PointF point, PrintUnits to)
	{
		float x = ConvertFromPixels(point.X, to);
		float y = ConvertFromPixels(point.Y, to);
		return new PointF(x, y);
	}

	public SizeF ConvertFromPixels(Size size, PrintUnits to)
	{
		float width = ConvertFromPixels(size.Width, to);
		float height = ConvertFromPixels(size.Height, to);
		return new SizeF(width, height);
	}

	public SizeF ConvertFromPixels(SizeF size, PrintUnits to)
	{
		float width = ConvertFromPixels(size.Width, to);
		float height = ConvertFromPixels(size.Height, to);
		return new SizeF(width, height);
	}

	public float ConvertToPixels(float value, PrintUnits from, float dpi)
	{
		if (from == PrintUnits.Pixel)
		{
			return value;
		}
		double[] proporsion = GetProporsion(dpi);
		return (float)((double)value * proporsion[(int)from]);
	}

	public double ConvertToPixels(double value, PrintUnits from, float dpi)
	{
		if (from == PrintUnits.Pixel)
		{
			return value;
		}
		double[] proporsion = GetProporsion(dpi);
		return value * proporsion[(int)from];
	}

	public float ConvertFromPixels(float value, PrintUnits to, float dpi)
	{
		if (to == PrintUnits.Pixel)
		{
			return value;
		}
		double[] proporsion = GetProporsion(dpi);
		return (float)((double)value / proporsion[(int)to]);
	}

	public double ConvertFromPixels(double value, PrintUnits to, float dpi)
	{
		if (to == PrintUnits.Pixel)
		{
			return value;
		}
		double[] proporsion = GetProporsion(dpi);
		return value / proporsion[(int)to];
	}

	public SizeF ConvertFromPixels(SizeF size, PrintUnits to, float dpi)
	{
		float width = ConvertFromPixels(size.Width, to, dpi);
		float height = ConvertFromPixels(size.Height, to, dpi);
		return new SizeF(width, height);
	}

	private double[] GetProporsion(float dpi)
	{
		return new double[7]
		{
			dpi / 75f,
			dpi / 300f,
			dpi,
			dpi / 25.4f,
			dpi / 2.54f,
			1.0,
			dpi / 72f
		};
	}

	internal void InitDefProporsions()
	{
		double num = 96.0;
		m_Proportions = new double[8]
		{
			num / 75.0,
			num / 300.0,
			num,
			num / 25.399999618530273,
			num / 2.5399999618530273,
			1.0,
			num / 72.0,
			num / 914400.0
		};
	}

	internal static void Close()
	{
		if (m_instance != null)
		{
			m_instance = null;
		}
	}
}
