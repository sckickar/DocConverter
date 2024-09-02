using System;
using System.ComponentModel;
using System.Diagnostics;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;
using DocGen.Styles;

namespace DocGen.Chart;

internal class ChartLineInfo : ChartSubStyleInfoBase
{
	private static ChartLineInfo m_defaultLine;

	private DashCap m_dashCap;

	private static ChartLineInfoStore m_store;

	private Pen m_pen;

	private bool m_updatePen;

	public static ChartLineInfo Default
	{
		[DebuggerStepThrough]
		get
		{
			if (m_defaultLine == null)
			{
				m_defaultLine = CreateDefault();
			}
			return m_defaultLine;
		}
	}

	[Browsable(false)]
	public Pen GdipPen => GetGdipPen();

	[Category("Appearance")]
	public Color Color
	{
		[DebuggerStepThrough]
		get
		{
			return (Color)GetValue(ChartLineInfoStore.ColorProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartLineInfoStore.ColorProperty, value);
		}
	}

	internal DashCap DashCap
	{
		[DebuggerStepThrough]
		get
		{
			return (DashCap)GetValue(ChartLineInfoStore.DashCapProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartLineInfoStore.DashCapProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasColor
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartLineInfoStore.ColorProperty);
		}
	}

	[Browsable(true)]
	[Description("Width of the line.")]
	[Category("Appearance")]
	public float Width
	{
		[DebuggerStepThrough]
		get
		{
			return (float)GetValue(ChartLineInfoStore.WidthProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartLineInfoStore.WidthProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasWidth
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartLineInfoStore.WidthProperty);
		}
	}

	[Browsable(true)]
	[Description("Gets or sets the pen alignment.")]
	[Category("Appearance")]
	public PenAlignment Alignment
	{
		get
		{
			return (PenAlignment)GetValue(ChartLineInfoStore.AlignmentProperty);
		}
		set
		{
			SetValue(ChartLineInfoStore.AlignmentProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasAlignment
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartLineInfoStore.AlignmentProperty);
		}
	}

	[Browsable(true)]
	[Description("Gets or sets the style of the line.")]
	[Category("Appearance")]
	public DashStyle DashStyle
	{
		[DebuggerStepThrough]
		get
		{
			return (DashStyle)GetValue(ChartLineInfoStore.DashStyleProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartLineInfoStore.DashStyleProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasDashStyle
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartLineInfoStore.DashStyleProperty);
		}
	}

	[Browsable(true)]
	[Description("Gets or sets the dash pattern of the line.")]
	[Category("Appearance")]
	public float[] DashPattern
	{
		[DebuggerStepThrough]
		get
		{
			return (float[])GetValue(ChartLineInfoStore.DashPatternProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartLineInfoStore.DashPatternProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasDashPattern
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartLineInfoStore.DashPatternProperty);
		}
	}

	[DebuggerStepThrough]
	public void ResetColor()
	{
		ResetValue(ChartLineInfoStore.ColorProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeColor()
	{
		return HasValue(ChartLineInfoStore.ColorProperty);
	}

	[DebuggerStepThrough]
	public void ResetWidth()
	{
		ResetValue(ChartLineInfoStore.WidthProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeWidth()
	{
		return HasValue(ChartLineInfoStore.WidthProperty);
	}

	[DebuggerStepThrough]
	public void ResetAlignment()
	{
		ResetValue(ChartLineInfoStore.AlignmentProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeAlignment()
	{
		return HasValue(ChartLineInfoStore.AlignmentProperty);
	}

	[DebuggerStepThrough]
	public void ResetDashStyle()
	{
		ResetValue(ChartLineInfoStore.DashStyleProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeDashStyle()
	{
		return HasValue(ChartLineInfoStore.DashStyleProperty);
	}

	[DebuggerStepThrough]
	public void ResetDashPattern()
	{
		ResetValue(ChartLineInfoStore.DashPatternProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeDashPattern()
	{
		return HasValue(ChartLineInfoStore.DashPatternProperty);
	}

	static ChartLineInfo()
	{
		m_store = new ChartLineInfoStore();
		m_defaultLine = CreateDefault();
	}

	[DebuggerStepThrough]
	public ChartLineInfo()
		: base(ChartLineInfoStore.InitializeStaticData())
	{
	}

	[DebuggerStepThrough]
	public ChartLineInfo(StyleInfoSubObjectIdentity identity)
		: base(identity, ChartLineInfoStore.InitializeStaticData())
	{
	}

	[DebuggerStepThrough]
	public ChartLineInfo(StyleInfoSubObjectIdentity identity, ChartLineInfoStore store)
		: base(identity, store)
	{
	}

	protected override void OnStyleChanged(StyleInfoProperty sip)
	{
		base.OnStyleChanged(sip);
		m_updatePen = true;
	}

	protected internal override StyleInfoBase GetDefaultStyle()
	{
		return Default;
	}

	[DebuggerStepThrough]
	public override IStyleInfoSubObject MakeCopy(StyleInfoBase newOwner, StyleInfoProperty sip)
	{
		return new ChartLineInfo(newOwner.CreateSubObjectIdentity(sip), (ChartLineInfoStore)base.Store.Clone());
	}

	public static ChartLineInfo CreateDefault()
	{
		return new ChartLineInfo
		{
			Color = SystemColors.ControlText,
			Width = 1f,
			Alignment = PenAlignment.Center,
			DashStyle = DashStyle.Solid,
			DashPattern = null,
			DashCap = DashCap.Flat
		};
	}

	private Pen GetGdipPen()
	{
		Pen pen = new Pen(Color, Width);
		if (m_pen == null || m_updatePen)
		{
			if (m_pen == null)
			{
				pen = new Pen(Color, Width).Clone() as Pen;
				pen.LineJoin = LineJoin.Round;
				pen.StartCap = LineCap.Round;
				pen.EndCap = LineCap.Round;
				pen.DashCap = DashCap;
			}
			else
			{
				pen = m_pen.Clone() as Pen;
				pen.Color = Color;
				pen.Width = Width;
			}
			pen.Alignment = Alignment;
			DashStyle dashStyle = DashStyle;
			if (dashStyle == DashStyle.Custom)
			{
				if (DashPattern != null && Array.IndexOf(DashPattern, 0f) == -1)
				{
					pen.DashStyle = dashStyle;
					pen.DashPattern = DashPattern;
				}
			}
			else
			{
				pen.DashStyle = dashStyle;
			}
			m_updatePen = false;
		}
		return pen;
	}

	public override void Dispose()
	{
		m_defaultLine = null;
		m_pen = null;
		if (m_store != null)
		{
			m_store.Dispose();
			m_store = null;
		}
	}
}
