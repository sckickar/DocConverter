using System;
using SkiaSharp;
using DocGen.Drawing.DocIOHelper;

namespace DocGen.Drawing.SkiaSharpHelper;

internal class Pen : IClone, IDisposable, IPen
{
	private SKPaint m_sKPaint;

	private DashStyle m_dashStyle;

	private float[] m_comoundArray;

	private float[] m_dashPattern;

	private LineCap m_startCap;

	private LineCap m_endCap;

	private DashCap m_dashCap;

	private PenAlignment m_alignment;

	private Brush m_brush;

	internal SKPaint SKPaint => m_sKPaint;

	internal Brush Brush
	{
		get
		{
			return m_brush;
		}
		set
		{
			m_brush = value;
		}
	}

	public DashStyle DashStyle
	{
		get
		{
			return m_dashStyle;
		}
		set
		{
			m_dashStyle = value;
		}
	}

	public float[] CompoundArray
	{
		get
		{
			return m_comoundArray;
		}
		set
		{
			m_comoundArray = value;
		}
	}

	public float[] DashPattern
	{
		get
		{
			return m_dashPattern;
		}
		set
		{
			m_dashPattern = value;
		}
	}

	public Color Color
	{
		get
		{
			return Extension.GetColor(m_sKPaint.Color);
		}
		set
		{
			m_sKPaint.Color = Extension.GetSKColor(value);
		}
	}

	public LineJoin LineJoin
	{
		get
		{
			return GetStrokeJoin(m_sKPaint.StrokeJoin);
		}
		set
		{
			m_sKPaint.StrokeJoin = GetStrokeJoin(value);
		}
	}

	public DashCap DashCap
	{
		get
		{
			return m_dashCap;
		}
		set
		{
			m_dashCap = value;
		}
	}

	public LineCap EndCap
	{
		get
		{
			return m_endCap;
		}
		set
		{
			m_endCap = value;
			m_sKPaint.StrokeCap = GetStrokeCap(m_endCap);
		}
	}

	public LineCap StartCap
	{
		get
		{
			return m_startCap;
		}
		set
		{
			m_startCap = value;
			m_sKPaint.StrokeCap = GetStrokeCap(m_startCap);
		}
	}

	public float Width
	{
		get
		{
			return m_sKPaint.StrokeWidth;
		}
		set
		{
			m_sKPaint.StrokeWidth = value;
		}
	}

	public PenAlignment Alignment
	{
		get
		{
			return m_alignment;
		}
		set
		{
			m_alignment = value;
		}
	}

	public Pen(Brush brush)
		: this(brush, brush.StrokeWidth)
	{
	}

	public Pen(Brush brush, float width)
		: this(brush.Color, width)
	{
	}

	public Pen(Color color)
		: this(color, 1f)
	{
	}

	public Pen(Color textColor, float size)
	{
		m_sKPaint = new SKPaint();
		m_sKPaint.Color = new SKColor(textColor.R, textColor.G, textColor.B, textColor.A);
		m_sKPaint.IsStroke = true;
		m_sKPaint.StrokeWidth = size;
		m_sKPaint.IsAntialias = true;
	}

	private LineJoin GetStrokeJoin(SKStrokeJoin lineJoin)
	{
		return lineJoin switch
		{
			SKStrokeJoin.Bevel => LineJoin.Bevel, 
			SKStrokeJoin.Round => LineJoin.Round, 
			_ => LineJoin.Miter, 
		};
	}

	private SKStrokeJoin GetStrokeJoin(LineJoin lineJoin)
	{
		return lineJoin switch
		{
			LineJoin.Bevel => SKStrokeJoin.Bevel, 
			LineJoin.Round => SKStrokeJoin.Round, 
			_ => SKStrokeJoin.Miter, 
		};
	}

	private SKStrokeCap GetStrokeCap(LineCap lineCap)
	{
		if (lineCap == LineCap.Round)
		{
			return SKStrokeCap.Round;
		}
		return SKStrokeCap.Butt;
	}

	public object Clone()
	{
		Pen pen = MemberwiseClone() as Pen;
		if (m_sKPaint != null)
		{
			pen.m_sKPaint = m_sKPaint.Clone();
		}
		return pen;
	}

	public void Dispose()
	{
		if (m_sKPaint != null)
		{
			m_sKPaint.Dispose();
		}
	}
}
