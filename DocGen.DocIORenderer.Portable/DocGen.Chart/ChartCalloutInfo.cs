using System.ComponentModel;
using System.Diagnostics;
using DocGen.Drawing;
using DocGen.Styles;

namespace DocGen.Chart;

internal class ChartCalloutInfo : ChartSubStyleInfoBase
{
	private static ChartCalloutInfo c_defaultCalloutInfo;

	private static ChartCalloutInfoStore m_store;

	private float hiddenX;

	private float hiddenY;

	private bool isdragged;

	public static ChartCalloutInfo Default
	{
		get
		{
			if (c_defaultCalloutInfo == null)
			{
				c_defaultCalloutInfo = new ChartCalloutInfo
				{
					Enable = false,
					Text = string.Empty,
					TextOffset = 2.5f,
					Font = new ChartFontInfo(),
					DisplayTextAndFormat = "{1},{2}",
					Position = LabelPosition.Top,
					OffsetX = 0f,
					OffsetY = 0f,
					Color = SystemColors.HighlightText,
					TextColor = SystemColors.WindowText,
					Border = ChartLineInfo.CreateDefault()
				};
			}
			return c_defaultCalloutInfo;
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	[Description("Enable the callout feature.")]
	public bool Enable
	{
		[DebuggerStepThrough]
		get
		{
			return (bool)GetValue(ChartCalloutInfoStore.EnableProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartCalloutInfoStore.EnableProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasEnable
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartCalloutInfoStore.EnableProperty);
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Description("This is associated with a Text property and used for internal purpose.")]
	public string Text
	{
		[DebuggerStepThrough]
		get
		{
			return (string)GetValue(ChartCalloutInfoStore.TextProperty);
		}
		set
		{
			SetValue(ChartCalloutInfoStore.TextProperty, value);
		}
	}

	[Description("The text offset associated with a callout point text.")]
	[Browsable(true)]
	[Category("Appearance")]
	public float TextOffset
	{
		[DebuggerStepThrough]
		get
		{
			return (float)GetValue(ChartCalloutInfoStore.TextOffsetProperty);
		}
		set
		{
			SetValue(ChartCalloutInfoStore.TextOffsetProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasTextOffset
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartCalloutInfoStore.TextOffsetProperty);
		}
	}

	[Description("The OffsetX associated with a callout point position.")]
	[Browsable(true)]
	[Category("Appearance")]
	public float OffsetX
	{
		[DebuggerStepThrough]
		get
		{
			return (float)GetValue(ChartCalloutInfoStore.OffsetXProperty);
		}
		set
		{
			SetValue(ChartCalloutInfoStore.OffsetXProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasOffsetX
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartCalloutInfoStore.OffsetXProperty);
		}
	}

	[Description("The OffsetX associated with a callout point position.")]
	[Browsable(true)]
	[Category("Appearance")]
	public float OffsetY
	{
		[DebuggerStepThrough]
		get
		{
			return (float)GetValue(ChartCalloutInfoStore.OffsetYProperty);
		}
		set
		{
			SetValue(ChartCalloutInfoStore.OffsetYProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasOffsetY
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartCalloutInfoStore.OffsetYProperty);
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Description("The text associated with a ChartPoint.")]
	[Browsable(true)]
	[Category("Appearance")]
	public ChartFontInfo Font
	{
		[DebuggerStepThrough]
		get
		{
			return (ChartFontInfo)GetValue(ChartCalloutInfoStore.FontProperty);
		}
		set
		{
			SetValue(ChartCalloutInfoStore.FontProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasFont
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartCalloutInfoStore.FontProperty);
		}
	}

	[Description("The Display TextAndFormat associated with a ChartPoint.")]
	[Browsable(true)]
	[Category("Appearance")]
	public string DisplayTextAndFormat
	{
		[DebuggerStepThrough]
		get
		{
			return (string)GetValue(ChartCalloutInfoStore.TextFormatProperty);
		}
		set
		{
			SetValue(ChartCalloutInfoStore.TextFormatProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasDisplayTextAndFormat
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartCalloutInfoStore.TextFormatProperty);
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	public LabelPosition Position
	{
		[DebuggerStepThrough]
		get
		{
			return (LabelPosition)GetValue(ChartCalloutInfoStore.PositionProperty);
		}
		set
		{
			SetValue(ChartCalloutInfoStore.PositionProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasPosition
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartCalloutInfoStore.PositionProperty);
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Description("This is associated with the label position placement on mouse dragging.")]
	public float HiddenX
	{
		get
		{
			return hiddenX;
		}
		set
		{
			hiddenX = value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Description("This is associated with the label position placement on mouse dragging.")]
	public float HiddenY
	{
		get
		{
			return hiddenY;
		}
		set
		{
			hiddenY = value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Description("Hidden value to check whether the mouse dragged or not in the point.")]
	public bool IsDragged
	{
		get
		{
			return isdragged;
		}
		set
		{
			isdragged = value;
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	[Description("BackgroundColor to be used with the callout.")]
	public Color Color
	{
		[DebuggerStepThrough]
		get
		{
			return (Color)GetValue(ChartCalloutInfoStore.ColorProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartCalloutInfoStore.ColorProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasColor
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartCalloutInfoStore.ColorProperty);
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	[Description("Color to be used with the callout text.")]
	public Color TextColor
	{
		[DebuggerStepThrough]
		get
		{
			return (Color)GetValue(ChartCalloutInfoStore.TextColorProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartCalloutInfoStore.TextColorProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasTextColor
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartCalloutInfoStore.TextColorProperty);
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
			return (ChartLineInfo)GetValue(ChartCalloutInfoStore.BorderProperty);
		}
		set
		{
			SetValue(ChartCalloutInfoStore.BorderProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasBorder
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartCalloutInfoStore.BorderProperty);
		}
	}

	static ChartCalloutInfo()
	{
		m_store = new ChartCalloutInfoStore();
		c_defaultCalloutInfo = new ChartCalloutInfo
		{
			Enable = false,
			Text = string.Empty,
			TextOffset = 2.5f,
			Font = new ChartFontInfo(),
			DisplayTextAndFormat = "{1},{2}",
			Position = LabelPosition.Top,
			OffsetX = 0f,
			OffsetY = 0f,
			Color = SystemColors.HighlightText,
			TextColor = SystemColors.WindowText,
			Border = ChartLineInfo.CreateDefault()
		};
	}

	[DebuggerStepThrough]
	public ChartCalloutInfo()
		: base(ChartCalloutInfoStore.InitializeStaticData())
	{
	}

	[DebuggerStepThrough]
	public ChartCalloutInfo(StyleInfoSubObjectIdentity identity)
		: base(identity, ChartCalloutInfoStore.InitializeStaticData())
	{
	}

	[DebuggerStepThrough]
	public ChartCalloutInfo(StyleInfoSubObjectIdentity identity, ChartCalloutInfoStore store)
		: base(identity, store)
	{
	}

	[DebuggerStepThrough]
	public void ResetEnable()
	{
		ResetValue(ChartCalloutInfoStore.EnableProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeEnable()
	{
		return HasValue(ChartCalloutInfoStore.EnableProperty);
	}

	[DebuggerStepThrough]
	public void ResetTextOffset()
	{
		ResetValue(ChartCalloutInfoStore.TextOffsetProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeTextOffset()
	{
		return HasValue(ChartCalloutInfoStore.TextOffsetProperty);
	}

	[DebuggerStepThrough]
	public void ResetOffsetX()
	{
		ResetValue(ChartCalloutInfoStore.OffsetXProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeOffsetX()
	{
		return HasValue(ChartCalloutInfoStore.OffsetXProperty);
	}

	[DebuggerStepThrough]
	public void ResetOffsetY()
	{
		ResetValue(ChartCalloutInfoStore.OffsetXProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeOffsetY()
	{
		return HasValue(ChartCalloutInfoStore.OffsetYProperty);
	}

	[DebuggerStepThrough]
	public void ResetFont()
	{
		ResetValue(ChartCalloutInfoStore.FontProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeFont()
	{
		return HasValue(ChartCalloutInfoStore.FontProperty);
	}

	[DebuggerStepThrough]
	public void ResetDisplayTextAndFormat()
	{
		ResetValue(ChartCalloutInfoStore.TextFormatProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeDisplayTextAndFormat()
	{
		return HasValue(ChartCalloutInfoStore.TextFormatProperty);
	}

	[DebuggerStepThrough]
	public void ResetPosition()
	{
		ResetValue(ChartCalloutInfoStore.PositionProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializePosition()
	{
		return HasValue(ChartCalloutInfoStore.PositionProperty);
	}

	[DebuggerStepThrough]
	public void ResetColor()
	{
		ResetValue(ChartCalloutInfoStore.ColorProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeColor()
	{
		return HasValue(ChartCalloutInfoStore.ColorProperty);
	}

	[DebuggerStepThrough]
	public void ResetTextColor()
	{
		ResetValue(ChartCalloutInfoStore.TextColorProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeTextColor()
	{
		return HasValue(ChartCalloutInfoStore.TextColorProperty);
	}

	[DebuggerStepThrough]
	public void ResetBorder()
	{
		ResetValue(ChartCalloutInfoStore.BorderProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeBorder()
	{
		return HasValue(ChartCalloutInfoStore.BorderProperty);
	}

	public override void Dispose()
	{
		c_defaultCalloutInfo = null;
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
			return new ChartCalloutInfo(identity, store as ChartCalloutInfoStore);
		}
		return new ChartCalloutInfo(identity);
	}

	[DebuggerStepThrough]
	public override StyleInfoSubObjectIdentity CreateSubObjectIdentity(StyleInfoProperty sip)
	{
		return new StyleInfoSubObjectIdentity(this, sip);
	}

	[DebuggerStepThrough]
	public override IStyleInfoSubObject MakeCopy(StyleInfoBase newOwner, StyleInfoProperty sip)
	{
		return new ChartCalloutInfo(newOwner.CreateSubObjectIdentity(sip), (ChartCalloutInfoStore)base.Store.Clone());
	}

	protected internal override StyleInfoBase GetDefaultStyle()
	{
		return Default;
	}
}
