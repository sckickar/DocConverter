using System;
using System.ComponentModel;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

[TypeConverter(typeof(ExpandableObjectConverter))]
internal class LineInfo
{
	private Color backColor = Color.Black;

	private Brush brush;

	private DashStyle dashStyle;

	private Color foreColor = Color.Black;

	private Pen pen = new Pen(Color.Black);

	private PenType penType;

	private float width = 1f;

	[DefaultValue(typeof(Color), "Black")]
	[NotifyParentProperty(true)]
	[Description("Specifies the backcolor that is to be associated with the line.")]
	public Color BackColor
	{
		get
		{
			return backColor;
		}
		set
		{
			if (backColor != value)
			{
				backColor = value;
				RefreshPen();
			}
		}
	}

	[DefaultValue(null)]
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Description("Gets the brush information that is to be used with the line.")]
	public Brush Brush => brush;

	[DefaultValue(DashStyle.Solid)]
	[NotifyParentProperty(true)]
	[Description("Specifies the style of the line.")]
	public DashStyle DashStyle
	{
		get
		{
			return dashStyle;
		}
		set
		{
			if (dashStyle != value && value != DashStyle.Custom)
			{
				dashStyle = value;
				RefreshPen();
			}
		}
	}

	[DefaultValue(typeof(Color), "Black")]
	[NotifyParentProperty(true)]
	[Description("Specifies the forecolor of the line.")]
	public Color ForeColor
	{
		get
		{
			return foreColor;
		}
		set
		{
			if (foreColor != value)
			{
				foreColor = value;
				RefreshPen();
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Description("Returns the pen used to render the line.")]
	public Pen Pen => pen;

	[DefaultValue(PenType.SolidColor)]
	[NotifyParentProperty(true)]
	[Description("Specifies the type of pen that is to be used with the line.")]
	public PenType PenType
	{
		get
		{
			return penType;
		}
		set
		{
			if (penType != value)
			{
				penType = value;
				RefreshPen();
			}
		}
	}

	[DefaultValue(1f)]
	[NotifyParentProperty(true)]
	[Description("Specifies the width of the line.")]
	public float Width
	{
		get
		{
			return width;
		}
		set
		{
			if (width != value)
			{
				width = value;
				RefreshPen();
			}
		}
	}

	public event EventHandler SettingsChanged;

	protected virtual void OnSettingsChanged(EventArgs e)
	{
		if (this.SettingsChanged != null)
		{
			this.SettingsChanged(this, e);
		}
	}

	protected void ResetBackColor()
	{
		BackColor = Color.Black;
	}

	protected void ResetForeColor()
	{
		ForeColor = Color.Black;
	}

	protected bool ShouldSerializeBackColor()
	{
		if (BackColor == Color.Black)
		{
			return false;
		}
		return true;
	}

	protected bool ShouldSerializeForeColor()
	{
		if (ForeColor == Color.Black)
		{
			return false;
		}
		return true;
	}

	private void RefreshPen()
	{
		switch (penType)
		{
		case PenType.HatchFill:
			brush = new HatchBrush(HatchStyle.Angle, foreColor, backColor);
			break;
		case PenType.LinearGradient:
			brush = new LinearGradientBrush(new Point(0, 0), new Point(100, 0), foreColor, backColor);
			break;
		default:
			brush = new SolidBrush(foreColor);
			break;
		}
		pen = new Pen(brush, width);
		pen.DashStyle = dashStyle;
		pen.LineJoin = LineJoin.Bevel;
		pen.Alignment = PenAlignment.Center;
		OnSettingsChanged(EventArgs.Empty);
	}
}
