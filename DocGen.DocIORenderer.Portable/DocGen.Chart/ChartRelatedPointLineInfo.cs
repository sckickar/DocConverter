using System.ComponentModel;
using System.Diagnostics;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

[ImmutableObject(true)]
internal class ChartRelatedPointLineInfo
{
	private Color color;

	private float width;

	private PenAlignment alignment;

	private DashStyle dashStyle;

	private float[] dashPattern;

	[Browsable(false)]
	public Pen GdipPen => GetGdipPen();

	[Description("Color of the line.")]
	[Category("Appearance")]
	public Color Color
	{
		[DebuggerStepThrough]
		get
		{
			return color;
		}
	}

	[Description("Width of the line.")]
	[Category("Appearance")]
	public float Width
	{
		[DebuggerStepThrough]
		get
		{
			return width;
		}
	}

	[Description("Pen alignment of the line.")]
	[Category("Appearance")]
	public PenAlignment Alignment => alignment;

	[Description("Dash style of the line.")]
	[Category("Appearance")]
	public DashStyle DashStyle
	{
		[DebuggerStepThrough]
		get
		{
			return dashStyle;
		}
	}

	[Description("Dash pattern of the line.")]
	[Category("Appearance")]
	public float[] DashPattern
	{
		[DebuggerStepThrough]
		get
		{
			return dashPattern;
		}
	}

	public ChartRelatedPointLineInfo(Color color, float width, PenAlignment alignment, DashStyle dashStyle, float[] dashPattern)
	{
		this.color = color;
		this.width = width;
		this.alignment = alignment;
		this.dashStyle = dashStyle;
		this.dashPattern = dashPattern;
	}

	public ChartRelatedPointLineInfo(Color color, float width)
		: this(color, width, PenAlignment.Center, DashStyle.Solid, null)
	{
	}

	public ChartRelatedPointLineInfo(Color color)
		: this(color, 5f, PenAlignment.Center, DashStyle.Solid, null)
	{
	}

	public ChartRelatedPointLineInfo()
		: this(SystemColors.ControlText, 5f, PenAlignment.Center, DashStyle.Solid, null)
	{
	}

	private Pen GetGdipPen()
	{
		using Pen pen = new Pen(Color, Width);
		pen.DashStyle = DashStyle;
		pen.Alignment = Alignment;
		if (DashStyle == DashStyle.Custom && DashPattern != null)
		{
			pen.DashPattern = DashPattern;
		}
		return pen;
	}
}
