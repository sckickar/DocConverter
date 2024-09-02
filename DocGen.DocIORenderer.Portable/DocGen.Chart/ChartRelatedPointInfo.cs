using System.ComponentModel;
using System.Diagnostics;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;
using DocGen.Styles;

namespace DocGen.Chart;

internal class ChartRelatedPointInfo : ChartSubStyleInfoBase
{
	private static ChartRelatedPointInfo defaultPoint;

	public static ChartRelatedPointInfo Default
	{
		[DebuggerStepThrough]
		get
		{
			if (defaultPoint == null)
			{
				defaultPoint = new ChartRelatedPointInfo();
				defaultPoint.Color = SystemColors.ControlText;
				defaultPoint.Width = 5f;
				defaultPoint.Alignment = PenAlignment.Center;
				defaultPoint.DashStyle = DashStyle.Solid;
				defaultPoint.DashPattern = null;
				defaultPoint.StartSymbol = new ChartRelatedPointSymbolInfo(ChartSymbolShape.None, -1, Color.White, Size.Empty);
				defaultPoint.Border = new ChartRelatedPointLineInfo();
			}
			return defaultPoint;
		}
	}

	[Browsable(false)]
	public Pen GdipPen => GetGdipPen();

	public int Count => Points?.GetLength(0) ?? 0;

	[Description("Indices of related points.")]
	[Category("Data")]
	public int[] Points
	{
		[DebuggerStepThrough]
		get
		{
			return (int[])GetValue(ChartRelatedPointInfoStore.PointsProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartRelatedPointInfoStore.PointsProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasPoints
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartRelatedPointInfoStore.PointsProperty);
		}
	}

	[Description("Color to be used for any visual representation.")]
	[Category("Appearance")]
	public Color Color
	{
		[DebuggerStepThrough]
		get
		{
			return (Color)GetValue(ChartRelatedPointInfoStore.ColorProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartRelatedPointInfoStore.ColorProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasColor
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartRelatedPointInfoStore.ColorProperty);
		}
	}

	[Description("Width to be used for any visual representation.")]
	[Category("Appearance")]
	public float Width
	{
		[DebuggerStepThrough]
		get
		{
			return (float)GetValue(ChartRelatedPointInfoStore.WidthProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartRelatedPointInfoStore.WidthProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasWidth
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartRelatedPointInfoStore.WidthProperty);
		}
	}

	[Description("Pen alignment to be used for any visual representation.")]
	[Category("Appearance")]
	public PenAlignment Alignment
	{
		get
		{
			return (PenAlignment)GetValue(ChartRelatedPointInfoStore.AlignmentProperty);
		}
		set
		{
			SetValue(ChartRelatedPointInfoStore.AlignmentProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasAlignment
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartRelatedPointInfoStore.AlignmentProperty);
		}
	}

	[Description("Dash style to be used for any visual representation.")]
	[Category("Appearance")]
	public DashStyle DashStyle
	{
		[DebuggerStepThrough]
		get
		{
			return (DashStyle)GetValue(ChartRelatedPointInfoStore.DashStyleProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartRelatedPointInfoStore.DashStyleProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasDashStyle
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartRelatedPointInfoStore.DashStyleProperty);
		}
	}

	[Description("Dash pattern to be used for any visual representation.")]
	[Category("Appearance")]
	public float[] DashPattern
	{
		[DebuggerStepThrough]
		get
		{
			return (float[])GetValue(ChartRelatedPointInfoStore.DashPatternProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartRelatedPointInfoStore.DashPatternProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasDashPattern
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartRelatedPointInfoStore.DashPatternProperty);
		}
	}

	[Description("Start symbol to be used for any visual representation linking this related point with others.")]
	[Category("Appearance")]
	public ChartRelatedPointSymbolInfo StartSymbol
	{
		[DebuggerStepThrough]
		get
		{
			return (ChartRelatedPointSymbolInfo)GetValue(ChartRelatedPointInfoStore.StartSymbolProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartRelatedPointInfoStore.StartSymbolProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasStartSymbol
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartRelatedPointInfoStore.StartSymbolProperty);
		}
	}

	[Description("End symbol to be used for any visual representation linking this related point with others.")]
	[Category("Appearance")]
	public ChartRelatedPointSymbolInfo EndSymbol
	{
		[DebuggerStepThrough]
		get
		{
			return (ChartRelatedPointSymbolInfo)GetValue(ChartRelatedPointInfoStore.EndSymbolProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartRelatedPointInfoStore.EndSymbolProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasEndSymbol
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartRelatedPointInfoStore.EndSymbolProperty);
		}
	}

	[Description("Border to be used for any visual representation linking this related point with others.")]
	[Category("Appearance")]
	public ChartRelatedPointLineInfo Border
	{
		[DebuggerStepThrough]
		get
		{
			return (ChartRelatedPointLineInfo)GetValue(ChartRelatedPointInfoStore.BorderProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartRelatedPointInfoStore.BorderProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasBorder
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartRelatedPointInfoStore.BorderProperty);
		}
	}

	internal static object CreateObject(StyleInfoSubObjectIdentity identity, object store)
	{
		if (store != null)
		{
			return new ChartRelatedPointInfo(identity, store as ChartRelatedPointInfoStore);
		}
		return new ChartRelatedPointInfo(identity);
	}

	public override void Dispose()
	{
		base.Dispose();
	}

	[DebuggerStepThrough]
	public ChartRelatedPointInfo()
		: base(new ChartRelatedPointInfoStore())
	{
	}

	[DebuggerStepThrough]
	public ChartRelatedPointInfo(StyleInfoSubObjectIdentity identity)
		: base(identity, new ChartRelatedPointInfoStore())
	{
	}

	[DebuggerStepThrough]
	public ChartRelatedPointInfo(StyleInfoSubObjectIdentity identity, ChartRelatedPointInfoStore store)
		: base(identity, store)
	{
	}

	[DebuggerStepThrough]
	public override IStyleInfoSubObject MakeCopy(StyleInfoBase newOwner, StyleInfoProperty sip)
	{
		return new ChartRelatedPointInfo(newOwner.CreateSubObjectIdentity(sip), (ChartRelatedPointInfoStore)base.Store.Clone());
	}

	internal void ResetGdipFont()
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

	protected override void OnStyleChanged(StyleInfoProperty sip)
	{
		base.OnStyleChanged(sip);
	}

	protected internal override StyleInfoBase GetDefaultStyle()
	{
		return Default;
	}

	[DebuggerStepThrough]
	public void ResetPoints()
	{
		ResetValue(ChartRelatedPointInfoStore.PointsProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializePoints()
	{
		return HasValue(ChartRelatedPointInfoStore.PointsProperty);
	}

	[DebuggerStepThrough]
	public void ResetColor()
	{
		ResetValue(ChartRelatedPointInfoStore.ColorProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeColor()
	{
		return HasValue(ChartRelatedPointInfoStore.ColorProperty);
	}

	[DebuggerStepThrough]
	public void ResetWidth()
	{
		ResetValue(ChartRelatedPointInfoStore.WidthProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeWidth()
	{
		return HasValue(ChartRelatedPointInfoStore.WidthProperty);
	}

	[DebuggerStepThrough]
	public void ResetAlignment()
	{
		ResetValue(ChartRelatedPointInfoStore.AlignmentProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeAlignment()
	{
		return HasValue(ChartRelatedPointInfoStore.AlignmentProperty);
	}

	[DebuggerStepThrough]
	public void ResetDashStyle()
	{
		ResetValue(ChartRelatedPointInfoStore.DashStyleProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeDashStyle()
	{
		return HasValue(ChartRelatedPointInfoStore.DashStyleProperty);
	}

	[DebuggerStepThrough]
	public void ResetDashPattern()
	{
		ResetValue(ChartRelatedPointInfoStore.DashPatternProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeDashPattern()
	{
		return HasValue(ChartRelatedPointInfoStore.DashPatternProperty);
	}

	[DebuggerStepThrough]
	public void ResetStartSymbol()
	{
		ResetValue(ChartRelatedPointInfoStore.StartSymbolProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeStartSymbol()
	{
		return HasValue(ChartRelatedPointInfoStore.StartSymbolProperty);
	}

	[DebuggerStepThrough]
	public void ResetEndSymbol()
	{
		ResetValue(ChartRelatedPointInfoStore.EndSymbolProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeEndSymbol()
	{
		return HasValue(ChartRelatedPointInfoStore.EndSymbolProperty);
	}

	[DebuggerStepThrough]
	public void ResetBorder()
	{
		ResetValue(ChartRelatedPointInfoStore.BorderProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeBorder()
	{
		return HasValue(ChartRelatedPointInfoStore.BorderProperty);
	}
}
