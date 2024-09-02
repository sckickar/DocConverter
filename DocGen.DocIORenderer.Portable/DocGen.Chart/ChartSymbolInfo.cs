using System.ComponentModel;
using System.Diagnostics;
using DocGen.Drawing;
using DocGen.Styles;

namespace DocGen.Chart;

internal class ChartSymbolInfo : ChartSubStyleInfoBase
{
	private static ChartSymbolInfo c_defaultSymbolInfo;

	private static ChartSymbolInfoStore m_store;

	public static ChartSymbolInfo Default
	{
		get
		{
			if (c_defaultSymbolInfo == null)
			{
				new ChartSymbolInfo();
				c_defaultSymbolInfo = new ChartSymbolInfo
				{
					Color = SystemColors.HighlightText,
					HighlightColor = Color.Transparent,
					DimmedColor = Color.Transparent,
					ImageIndex = -1,
					Size = new Size(10, 10),
					Shape = ChartSymbolShape.None,
					Offset = new Size(0, 0),
					Marker = new ChartMarker(),
					Border = ChartLineInfo.CreateDefault()
				};
			}
			return c_defaultSymbolInfo;
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	[Description("The style of the symbol to be displayed.")]
	public ChartSymbolShape Shape
	{
		[DebuggerStepThrough]
		get
		{
			return (ChartSymbolShape)GetValue(ChartSymbolInfoStore.ShapeProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartSymbolInfoStore.ShapeProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasShape
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartSymbolInfoStore.ShapeProperty);
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	[Description("Index of the image from the associated ChartStyleInfo's ImageList.")]
	public int ImageIndex
	{
		[DebuggerStepThrough]
		get
		{
			return (int)GetValue(ChartSymbolInfoStore.ImageIndexProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartSymbolInfoStore.ImageIndexProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasImageIndex
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartSymbolInfoStore.ImageIndexProperty);
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	[Description("Color to be used with the symbol.")]
	public Color Color
	{
		[DebuggerStepThrough]
		get
		{
			return (Color)GetValue(ChartSymbolInfoStore.ColorProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartSymbolInfoStore.ColorProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasColor
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartSymbolInfoStore.ColorProperty);
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	[Description("Gets or sets the color of the highlighted symbol.")]
	public Color HighlightColor
	{
		[DebuggerStepThrough]
		get
		{
			return (Color)GetValue(ChartSymbolInfoStore.HighlightColorProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartSymbolInfoStore.HighlightColorProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasHighlightColor
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartSymbolInfoStore.HighlightColorProperty);
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	[Description("Color to be used with the symbol.")]
	public Color DimmedColor
	{
		[DebuggerStepThrough]
		get
		{
			return (Color)GetValue(ChartSymbolInfoStore.DimmedColorProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartSymbolInfoStore.DimmedColorProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasDimmedColor
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartSymbolInfoStore.DimmedColorProperty);
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	[Description("Specifies the size of the symbol.")]
	public Size Size
	{
		[DebuggerStepThrough]
		get
		{
			return (Size)GetValue(ChartSymbolInfoStore.SizeProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartSymbolInfoStore.SizeProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasSize
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartSymbolInfoStore.SizeProperty);
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	[Description("Specifies the offset of the symbol.")]
	public Size Offset
	{
		[DebuggerStepThrough]
		get
		{
			return (Size)GetValue(ChartSymbolInfoStore.OffsetProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartSymbolInfoStore.OffsetProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasOffset
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartSymbolInfoStore.OffsetProperty);
		}
	}

	[Description("Line information.")]
	[Browsable(true)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Category("Appearance")]
	public ChartLineInfo Border
	{
		[DebuggerStepThrough]
		get
		{
			return (ChartLineInfo)GetValue(ChartSymbolInfoStore.BorderProperty);
		}
		set
		{
			SetValue(ChartSymbolInfoStore.BorderProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasBorder
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartSymbolInfoStore.BorderProperty);
		}
	}

	[Browsable(false)]
	[Category("Appearance")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("Specifies the size of the symbol.")]
	public ChartMarker Marker
	{
		[DebuggerStepThrough]
		get
		{
			return (ChartMarker)GetValue(ChartSymbolInfoStore.MarkerProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartSymbolInfoStore.MarkerProperty, value);
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasMarker
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartSymbolInfoStore.MarkerProperty);
		}
	}

	static ChartSymbolInfo()
	{
		m_store = new ChartSymbolInfoStore();
		new ChartSymbolInfo();
		c_defaultSymbolInfo = new ChartSymbolInfo
		{
			Color = SystemColors.HighlightText,
			HighlightColor = Color.Transparent,
			DimmedColor = Color.Transparent,
			ImageIndex = -1,
			Size = new Size(10, 10),
			Shape = ChartSymbolShape.None,
			Offset = new Size(0, 0),
			Marker = new ChartMarker(),
			Border = ChartLineInfo.CreateDefault()
		};
	}

	[DebuggerStepThrough]
	public ChartSymbolInfo()
		: base(ChartSymbolInfoStore.InitializeStaticData())
	{
	}

	[DebuggerStepThrough]
	public ChartSymbolInfo(StyleInfoSubObjectIdentity identity)
		: base(identity, ChartSymbolInfoStore.InitializeStaticData())
	{
	}

	[DebuggerStepThrough]
	public ChartSymbolInfo(StyleInfoSubObjectIdentity identity, ChartSymbolInfoStore store)
		: base(identity, store)
	{
	}

	[DebuggerStepThrough]
	public void ResetShape()
	{
		ResetValue(ChartSymbolInfoStore.ShapeProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeShape()
	{
		return HasValue(ChartSymbolInfoStore.ShapeProperty);
	}

	[DebuggerStepThrough]
	public void ResetImageIndex()
	{
		ResetValue(ChartSymbolInfoStore.ImageIndexProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeImageIndex()
	{
		return HasValue(ChartSymbolInfoStore.ImageIndexProperty);
	}

	[DebuggerStepThrough]
	public void ResetColor()
	{
		ResetValue(ChartSymbolInfoStore.ColorProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeColor()
	{
		return HasValue(ChartSymbolInfoStore.ColorProperty);
	}

	[DebuggerStepThrough]
	public void ResetHighlightColor()
	{
		ResetValue(ChartSymbolInfoStore.HighlightColorProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeHighlightColor()
	{
		return HasValue(ChartSymbolInfoStore.HighlightColorProperty);
	}

	[DebuggerStepThrough]
	public void ResetDimmedColor()
	{
		ResetValue(ChartSymbolInfoStore.DimmedColorProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeDimmedColor()
	{
		return HasValue(ChartSymbolInfoStore.DimmedColorProperty);
	}

	[DebuggerStepThrough]
	public void ResetSize()
	{
		ResetValue(ChartSymbolInfoStore.SizeProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeSize()
	{
		return HasValue(ChartSymbolInfoStore.SizeProperty);
	}

	[DebuggerStepThrough]
	public void ResetOffset()
	{
		ResetValue(ChartSymbolInfoStore.OffsetProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeOffset()
	{
		return HasValue(ChartSymbolInfoStore.OffsetProperty);
	}

	[DebuggerStepThrough]
	public void ResetBorder()
	{
		ResetValue(ChartSymbolInfoStore.BorderProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeBorder()
	{
		return HasValue(ChartSymbolInfoStore.BorderProperty);
	}

	[DebuggerStepThrough]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void ResetMarker()
	{
		ResetValue(ChartSymbolInfoStore.MarkerProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeMarker()
	{
		return HasValue(ChartSymbolInfoStore.MarkerProperty);
	}

	public override void Dispose()
	{
		c_defaultSymbolInfo = null;
		if (m_store != null)
		{
			m_store.Dispose();
			m_store = null;
		}
		base.Dispose();
	}

	internal static object CreateObject(StyleInfoSubObjectIdentity identity, object store)
	{
		if (store != null)
		{
			return new ChartSymbolInfo(identity, store as ChartSymbolInfoStore);
		}
		return new ChartSymbolInfo(identity);
	}

	[DebuggerStepThrough]
	public override StyleInfoSubObjectIdentity CreateSubObjectIdentity(StyleInfoProperty sip)
	{
		return new StyleInfoSubObjectIdentity(this, sip);
	}

	[DebuggerStepThrough]
	public override IStyleInfoSubObject MakeCopy(StyleInfoBase newOwner, StyleInfoProperty sip)
	{
		return new ChartSymbolInfo(newOwner.CreateSubObjectIdentity(sip), (ChartSymbolInfoStore)base.Store.Clone());
	}

	protected internal override StyleInfoBase GetDefaultStyle()
	{
		return Default;
	}
}
