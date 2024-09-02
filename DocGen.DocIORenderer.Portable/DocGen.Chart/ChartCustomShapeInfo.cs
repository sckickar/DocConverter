using System.ComponentModel;
using System.Diagnostics;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;
using DocGen.Styles;

namespace DocGen.Chart;

internal class ChartCustomShapeInfo : ChartSubStyleInfoBase
{
	private static ChartCustomShapeInfo c_defaultShapeInfo;

	private static ChartCustomShapeInfoStore m_store;

	public static ChartCustomShapeInfo Default => c_defaultShapeInfo;

	[Browsable(true)]
	[Category("Appearance")]
	[Description("Color to be used with the symbol.")]
	public Color Color
	{
		[DebuggerStepThrough]
		get
		{
			return (Color)GetValue(ChartCustomShapeInfoStore.ColorProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartCustomShapeInfoStore.ColorProperty, value);
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	[DefaultValue("Square")]
	[Description("The style of the shape to be displayed. It will support the limitted shape(Square, Circle, Hexagon, Pentagon) draw around the custom point")]
	public ChartCustomShape Type
	{
		[DebuggerStepThrough]
		get
		{
			return (ChartCustomShape)GetValue(ChartCustomShapeInfoStore.ShapeTypeProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartCustomShapeInfoStore.ShapeTypeProperty, value);
		}
	}

	[DefaultValue(typeof(ChartLineInfo), "Draw border of shape")]
	[Category("Appearance")]
	public ChartLineInfo Border
	{
		[DebuggerStepThrough]
		get
		{
			return (ChartLineInfo)GetValue(ChartCustomShapeInfoStore.BorderProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartCustomShapeInfoStore.BorderProperty, value);
		}
	}

	[DefaultValue(typeof(float), "border width of shape")]
	[Category("Appearance")]
	public float BorderWidth
	{
		[DebuggerStepThrough]
		get
		{
			return (float)GetValue(ChartCustomShapeInfoStore.BorderWidthProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartCustomShapeInfoStore.BorderWidthProperty, value);
		}
	}

	[DefaultValue(typeof(Color), "border color of shape")]
	[Category("Appearance")]
	public Color BorderColor
	{
		[DebuggerStepThrough]
		get
		{
			return (Color)GetValue(ChartCustomShapeInfoStore.BorderColorProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartCustomShapeInfoStore.BorderColorProperty, value);
		}
	}

	public Pen BorderGdiPen
	{
		[DebuggerStepThrough]
		get
		{
			return new Pen(BorderColor, BorderWidth);
		}
	}

	static ChartCustomShapeInfo()
	{
		m_store = new ChartCustomShapeInfoStore();
		new ChartCustomShapeInfo();
		c_defaultShapeInfo = new ChartCustomShapeInfo
		{
			Color = SystemColors.HighlightText,
			Type = ChartCustomShape.Square,
			Border = ChartLineInfo.CreateDefault(),
			BorderColor = SystemColors.ControlText,
			BorderWidth = 1f
		};
	}

	[DebuggerStepThrough]
	public ChartCustomShapeInfo()
		: base(ChartCustomShapeInfoStore.InitializeStaticVariables())
	{
	}

	[DebuggerStepThrough]
	public ChartCustomShapeInfo(StyleInfoSubObjectIdentity identity)
		: base(identity, ChartCustomShapeInfoStore.InitializeStaticVariables())
	{
	}

	[DebuggerStepThrough]
	public ChartCustomShapeInfo(StyleInfoSubObjectIdentity identity, ChartCustomShapeInfoStore store)
		: base(identity, store)
	{
	}

	public override void Dispose()
	{
		if (c_defaultShapeInfo != null)
		{
			c_defaultShapeInfo.Border.Dispose();
			c_defaultShapeInfo = null;
		}
		if (m_store != null)
		{
			m_store.Dispose();
			m_store = null;
		}
		base.Dispose();
	}

	protected internal override StyleInfoBase GetDefaultStyle()
	{
		return c_defaultShapeInfo;
	}
}
