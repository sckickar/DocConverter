using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;
using DocGen.Styles;

namespace DocGen.Chart;

internal class ChartStyleInfo : ChartStyleInfoBase
{
	private static ChartStyleInfo c_defaultStyle;

	private string m_Url;

	private static ChartStyleInfoStore m_store;

	private ChartStyleInfoCustomPropertiesCollection cpl;

	internal bool IsScatterBorderColor { get; set; }

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public new ChartStyleInfoStore Store => (ChartStyleInfoStore)base.Store;

	public static ChartStyleInfo Default
	{
		get
		{
			if (c_defaultStyle == null)
			{
				new ChartStyleInfo();
				ChartStyleInfo obj = new ChartStyleInfo
				{
					TextColor = SystemColors.WindowText,
					BaseStyle = "Standard",
					AltTagFormat = "",
					Font = ChartFontInfo.Default,
					Border = ChartLineInfo.Default,
					Text = string.Empty,
					TextFormat = string.Empty,
					DisplayText = false,
					DrawTextShape = false,
					TextShape = ChartCustomShapeInfo.Default,
					TextOffset = 2.5f,
					ToolTip = "",
					ToolTipFormat = string.Empty,
					Images = null,
					ImageIndex = -1,
					Symbol = ChartSymbolInfo.Default,
					_System = false,
					_Name = "",
					TextOrientation = ChartTextOrientation.Center,
					DisplayShadow = false
				};
				BrushInfo br = new BrushInfo(ControlPaintExtension.DarkDark(obj.TextColor));
				obj.ShadowInterior = new BrushInfo(100, br);
				obj.ShadowOffset = new Size(3, 2);
				obj.SetValue(ChartStyleInfoStore.HighlightOnMouseOverProperty, false);
				obj.HitTestRadius = 7.5f;
				obj.Label = "";
				obj.PointWidth = 1f;
				obj.SetValue(ChartStyleInfoStore.ElementBordersProperty, ChartBordersInfo.Default);
				obj.RelatedPoints = null;
				c_defaultStyle = obj;
			}
			return (ChartStyleInfo)c_defaultStyle.MemberwiseClone();
		}
	}

	[DefaultValue(typeof(DrawShape), "shape properties")]
	[Category("Appearance")]
	public ChartCustomShapeInfo TextShape
	{
		[DebuggerStepThrough]
		get
		{
			return (ChartCustomShapeInfo)GetValue(ChartStyleInfoStore.TextShapeProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.TextShapeProperty, value);
		}
	}

	[Description("The color of the text that is rendered at a ChartPoint.")]
	[Category("Appearance")]
	public Color TextColor
	{
		get
		{
			return (Color)GetValue(ChartStyleInfoStore.TextColorProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.TextColorProperty, value);
		}
	}

	public bool HasTextColor => HasValue(ChartStyleInfoStore.TextColorProperty);

	[Description("The base style with default settings for appearance of the ChartPoint.")]
	[Category("Style")]
	[Browsable(false)]
	public string BaseStyle
	{
		get
		{
			return (string)GetValue(ChartStyleInfoStore.BaseStyleProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.BaseStyleProperty, value);
		}
	}

	[Description("This is only for ASP.NET. which is used to to define the alt tag format in html page.")]
	[Category("Style")]
	[Browsable(false)]
	public string AltTagFormat
	{
		get
		{
			return (string)GetValue(ChartStyleInfoStore.AltTagFormatProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.AltTagFormatProperty, value);
		}
	}

	public bool HasBaseStyle => HasValue(ChartStyleInfoStore.BaseStyleProperty);

	[Browsable(false)]
	public Font GdipFont
	{
		[DebuggerStepThrough]
		get
		{
			if (HasFont && !Font.IsEmpty)
			{
				return Font.GdipFont;
			}
			if (base.Identity != null && base.Identity.GetBaseStyleNotEmptyExpandable(this, ChartStyleInfoStore.FontProperty) is ChartStyleInfo chartStyleInfo)
			{
				return chartStyleInfo.GdipFont;
			}
			return Default.GdipFont;
		}
	}

	[Description("The font for drawing text.")]
	[Browsable(true)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Category("Appearance")]
	public ChartFontInfo Font
	{
		[DebuggerStepThrough]
		get
		{
			return (ChartFontInfo)GetValue(ChartStyleInfoStore.FontProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.FontProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasFont
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartStyleInfoStore.FontProperty);
		}
	}

	[Browsable(false)]
	public Pen GdipPen
	{
		[DebuggerStepThrough]
		get
		{
			if (HasBorder && !Border.IsEmpty)
			{
				return Border.GdipPen;
			}
			if (base.Identity != null && base.Identity.GetBaseStyleNotEmptyExpandable(this, ChartStyleInfoStore.BorderProperty) is ChartStyleInfo chartStyleInfo)
			{
				return chartStyleInfo.GdipPen;
			}
			return Default.GdipPen;
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
			return (ChartLineInfo)GetValue(ChartStyleInfoStore.BorderProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.BorderProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasBorder
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartStyleInfoStore.BorderProperty);
		}
	}

	[Description("Lets you specify a solid backcolor, gradient, or pattern style with both back and forecolor for a ChartPoint's background.")]
	[Browsable(true)]
	[Category("Appearance")]
	public BrushInfo Interior
	{
		[DebuggerStepThrough]
		get
		{
			return (BrushInfo)GetValue(ChartStyleInfoStore.InteriorProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.InteriorProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasInterior
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartStyleInfoStore.InteriorProperty);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public ChartStyleInfoCustomPropertiesCollection CustomProperties
	{
		get
		{
			if (cpl == null)
			{
				cpl = new ChartStyleInfoCustomPropertiesCollection(this);
			}
			return cpl;
		}
	}

	[Description("The text associated with a ChartPoint.")]
	[Browsable(true)]
	[Category("Appearance")]
	public string Text
	{
		[DebuggerStepThrough]
		get
		{
			return (string)GetValue(ChartStyleInfoStore.TextProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.TextProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasText
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartStyleInfoStore.TextProperty);
		}
	}

	[Description("Specifies the ToolTip to be displayed for the associated ChartPoint.")]
	[Browsable(true)]
	[Category("Appearance")]
	public string ToolTip
	{
		[DebuggerStepThrough]
		get
		{
			return (string)GetValue(ChartStyleInfoStore.ToolTipProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.ToolTipProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasToolTip
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartStyleInfoStore.ToolTipProperty);
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	public string ToolTipFormat
	{
		[DebuggerStepThrough]
		get
		{
			return (string)GetValue(ChartStyleInfoStore.ToolTipFormatProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.ToolTipFormatProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasToolTipFormat
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartStyleInfoStore.ToolTipFormatProperty);
		}
	}

	[Description("The imagelist associated with this ChartPoint. The ImageIndex property will be used in conjunction to determine the image displayed.")]
	[Browsable(true)]
	[Category("Appearance")]
	public ChartImageCollection Images
	{
		[DebuggerStepThrough]
		get
		{
			return (ChartImageCollection)GetValue(ChartStyleInfoStore.ImagesProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.ImagesProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasImages
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartStyleInfoStore.ImagesProperty);
		}
	}

	[Description("Specifies the image index to be used from the image list associated with this ChartPoint.")]
	[Browsable(true)]
	[Category("Appearance")]
	public int ImageIndex
	{
		[DebuggerStepThrough]
		get
		{
			return (int)GetValue(ChartStyleInfoStore.ImageIndexProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.ImageIndexProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasImageIndex
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartStyleInfoStore.ImageIndexProperty);
		}
	}

	[Description("Specifies the attributes of the symbol that will be displayed at the ChartPoint.")]
	[Browsable(true)]
	[Category("Appearance")]
	public ChartSymbolInfo Symbol
	{
		[DebuggerStepThrough]
		get
		{
			return (ChartSymbolInfo)GetValue(ChartStyleInfoStore.SymbolProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.SymbolProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasSymbol => HasValue(ChartStyleInfoStore.SymbolProperty);

	[Description("Specifies the attributes of the Callout that will be displayed at the ChartPoint.")]
	[Browsable(true)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Category("Appearance")]
	public ChartCalloutInfo Callout
	{
		[DebuggerStepThrough]
		get
		{
			return (ChartCalloutInfo)GetValue(ChartStyleInfoStore.CalloutProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.CalloutProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasCallout => HasValue(ChartStyleInfoStore.CalloutProperty);

	[Browsable(true)]
	internal bool _System
	{
		[DebuggerStepThrough]
		get
		{
			return (bool)GetValue(ChartStyleInfoStore.SystemProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.SystemProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	internal bool _HasSystem
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartStyleInfoStore.SystemProperty);
		}
	}

	[Browsable(true)]
	internal string _Name
	{
		[DebuggerStepThrough]
		get
		{
			return (string)GetValue(ChartStyleInfoStore.NameProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.NameProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	internal bool _HasName
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartStyleInfoStore.NameProperty);
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	public ChartTextOrientation TextOrientation
	{
		[DebuggerStepThrough]
		get
		{
			return (ChartTextOrientation)GetValue(ChartStyleInfoStore.TextOrientationProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.TextOrientationProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasTextOrientation
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartStyleInfoStore.TextOrientationProperty);
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	public bool DisplayShadow
	{
		[DebuggerStepThrough]
		get
		{
			return (bool)GetValue(ChartStyleInfoStore.DisplayShadowProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.DisplayShadowProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasDisplayShadow
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartStyleInfoStore.DisplayShadowProperty);
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	public Size ShadowOffset
	{
		[DebuggerStepThrough]
		get
		{
			return (Size)GetValue(ChartStyleInfoStore.ShadowOffsetProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.ShadowOffsetProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasShadowOffset
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartStyleInfoStore.ShadowOffsetProperty);
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	public BrushInfo ShadowInterior
	{
		[DebuggerStepThrough]
		get
		{
			return (BrushInfo)GetValue(ChartStyleInfoStore.ShadowInteriorProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.ShadowInteriorProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasShadowInterior
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartStyleInfoStore.ShadowInteriorProperty);
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	[Obsolete("This property isn't used anymore.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool HighlightOnMouseOver
	{
		[DebuggerStepThrough]
		get
		{
			return (bool)GetValue(ChartStyleInfoStore.HighlightOnMouseOverProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.HighlightOnMouseOverProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Obsolete("This property isn't used anymore.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool HasHighlightOnMouseOver
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartStyleInfoStore.HighlightOnMouseOverProperty);
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	public BrushInfo HighlightInterior
	{
		[DebuggerStepThrough]
		get
		{
			return (BrushInfo)GetValue(ChartStyleInfoStore.HighlightInteriorProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.HighlightInteriorProperty, value);
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	public BrushInfo DimmedInterior
	{
		[DebuggerStepThrough]
		get
		{
			return (BrushInfo)GetValue(ChartStyleInfoStore.DimmedInteriorProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.DimmedInteriorProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasHighlightInterior
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartStyleInfoStore.HighlightInteriorProperty);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasDimmedInterior
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartStyleInfoStore.DimmedInteriorProperty);
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	public float HitTestRadius
	{
		[DebuggerStepThrough]
		get
		{
			return (float)GetValue(ChartStyleInfoStore.HitTestRadiusProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.HitTestRadiusProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasHitTestRadius
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartStyleInfoStore.HitTestRadiusProperty);
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	public string Label
	{
		[DebuggerStepThrough]
		get
		{
			return (string)GetValue(ChartStyleInfoStore.LabelProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.LabelProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasLabel
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartStyleInfoStore.LabelProperty);
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	public string TextFormat
	{
		[DebuggerStepThrough]
		get
		{
			return (string)GetValue(ChartStyleInfoStore.TextFormatProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.TextFormatProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasTextFormat
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartStyleInfoStore.TextFormatProperty);
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	public StringFormat Format
	{
		[DebuggerStepThrough]
		get
		{
			return (StringFormat)GetValue(ChartStyleInfoStore.FormatProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.FormatProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasFormat
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartStyleInfoStore.FormatProperty);
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	public bool DisplayText
	{
		[DebuggerStepThrough]
		get
		{
			return (bool)GetValue(ChartStyleInfoStore.DisplayTextProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.DisplayTextProperty, value);
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	public bool DrawTextShape
	{
		[DebuggerStepThrough]
		get
		{
			return (bool)GetValue(ChartStyleInfoStore.DrawTextShapeProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.DrawTextShapeProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasDisplayText
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartStyleInfoStore.DisplayTextProperty);
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	public float PointWidth
	{
		[DebuggerStepThrough]
		get
		{
			return (float)GetValue(ChartStyleInfoStore.PointWidthProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.PointWidthProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasPointWidth
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartStyleInfoStore.PointWidthProperty);
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	public float TextOffset
	{
		[DebuggerStepThrough]
		get
		{
			return (float)GetValue(ChartStyleInfoStore.TextOffsetProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.TextOffsetProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasTextOffset
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartStyleInfoStore.TextOffsetProperty);
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	public ChartRelatedPointInfo RelatedPoints
	{
		[DebuggerStepThrough]
		get
		{
			return (ChartRelatedPointInfo)GetValue(ChartStyleInfoStore.RelatedPointsProperty);
		}
		set
		{
			SetValue(ChartStyleInfoStore.RelatedPointsProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasRelatedPoints
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartStyleInfoStore.RelatedPointsProperty);
		}
	}

	[Description("Gets or Sets the url.")]
	[Browsable(true)]
	public string Url
	{
		[DebuggerStepThrough]
		get
		{
			return (string)GetValue(ChartStyleInfoStore.UrlProperty);
		}
		set
		{
			if (m_Url != value)
			{
				m_Url = value;
			}
			if (m_Url.StartsWith("www."))
			{
				m_Url = m_Url.Insert(0, "http://");
			}
			SetValue(ChartStyleInfoStore.UrlProperty, m_Url);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasUrl
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartStyleInfoStore.UrlProperty);
		}
	}

	static ChartStyleInfo()
	{
		c_defaultStyle = null;
		m_store = new ChartStyleInfoStore();
		new ChartStyleInfo();
		ChartStyleInfo obj = new ChartStyleInfo
		{
			TextColor = SystemColors.WindowText,
			BaseStyle = "Standard",
			AltTagFormat = "",
			Font = ChartFontInfo.Default,
			Border = ChartLineInfo.Default,
			Text = string.Empty,
			TextFormat = string.Empty,
			DisplayText = false,
			DrawTextShape = false,
			TextShape = ChartCustomShapeInfo.Default,
			TextOffset = 2.5f,
			ToolTip = "",
			ToolTipFormat = string.Empty,
			Images = null,
			ImageIndex = -1,
			Symbol = ChartSymbolInfo.Default,
			_System = false,
			_Name = "",
			TextOrientation = ChartTextOrientation.Center,
			DisplayShadow = false
		};
		BrushInfo br = new BrushInfo(ControlPaintExtension.DarkDark(obj.TextColor));
		obj.ShadowInterior = new BrushInfo(100, br);
		obj.ShadowOffset = new Size(3, 2);
		obj.SetValue(ChartStyleInfoStore.HighlightOnMouseOverProperty, false);
		obj.HitTestRadius = 7.5f;
		obj.Label = "";
		obj.PointWidth = 1f;
		obj.SetValue(ChartStyleInfoStore.ElementBordersProperty, ChartBordersInfo.Default);
		obj.RelatedPoints = null;
		c_defaultStyle = obj;
	}

	[DebuggerStepThrough]
	public ChartStyleInfo()
		: base(ChartStyleInfoStore.InitializeStaticVariables())
	{
	}

	[DebuggerStepThrough]
	public ChartStyleInfo(ChartStyleInfo style)
		: base(style.Store)
	{
	}

	[DebuggerStepThrough]
	public ChartStyleInfo(ChartStyleInfoStore store)
		: base(store)
	{
	}

	[DebuggerStepThrough]
	public ChartStyleInfo(StyleInfoIdentityBase identity)
		: base(identity, ChartStyleInfoStore.InitializeStaticVariables())
	{
	}

	[DebuggerStepThrough]
	public ChartStyleInfo(StyleInfoIdentityBase identity, ChartStyleInfoStore store)
		: base(identity, store)
	{
	}

	public void CopyFrom(IStyleInfo iStyle)
	{
		if (!StyleInfoBase.EqualsObject(Store, iStyle.Store))
		{
			OnStyleChanging(null);
			Store.ModifyStyleKeepChanges(iStyle.Store, StyleModifyType.Copy);
			OnStyleChanged(null);
		}
	}

	public override void Dispose()
	{
		if (c_defaultStyle != null)
		{
			c_defaultStyle.Font.Dispose();
			c_defaultStyle.TextShape.Dispose();
			c_defaultStyle = null;
		}
		if (m_store != null)
		{
			m_store.Dispose();
			m_store = null;
		}
		base.Dispose();
	}

	[DebuggerStepThrough]
	public override StyleInfoSubObjectIdentity CreateSubObjectIdentity(StyleInfoProperty sip)
	{
		return new StyleInfoSubObjectIdentity(this, sip);
	}

	protected internal override StyleInfoBase GetDefaultStyle()
	{
		return Default;
	}

	public void WriteXmlSchema(XmlWriter xw)
	{
		new XmlSerializer(typeof(ChartStyleInfo)).Serialize(xw, this);
	}

	public void ResetTextColor()
	{
		ResetValue(ChartStyleInfoStore.TextColorProperty);
	}

	private bool ShouldSerializeTextColor()
	{
		return HasValue(ChartStyleInfoStore.TextColorProperty);
	}

	public void ResetBaseStyle()
	{
		ResetValue(ChartStyleInfoStore.BaseStyleProperty);
	}

	private bool ShouldSerializeBaseStyle()
	{
		return HasValue(ChartStyleInfoStore.BaseStyleProperty);
	}

	[DebuggerStepThrough]
	public void ResetFont()
	{
		ResetValue(ChartStyleInfoStore.FontProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeFont()
	{
		return HasValue(ChartStyleInfoStore.FontProperty);
	}

	[DebuggerStepThrough]
	public void ResetBorder()
	{
		ResetValue(ChartStyleInfoStore.BorderProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeBorder()
	{
		return HasValue(ChartStyleInfoStore.BorderProperty);
	}

	[DebuggerStepThrough]
	public void ResetInterior()
	{
		ResetValue(ChartStyleInfoStore.InteriorProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeInterior()
	{
		return HasValue(ChartStyleInfoStore.InteriorProperty);
	}

	private bool ShouldSerializeCustomProperties()
	{
		return CustomProperties.Count > 0;
	}

	[DebuggerStepThrough]
	public void ResetText()
	{
		ResetValue(ChartStyleInfoStore.TextProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeText()
	{
		return HasValue(ChartStyleInfoStore.TextProperty);
	}

	[DebuggerStepThrough]
	public void ResetToolTip()
	{
		ResetValue(ChartStyleInfoStore.ToolTipProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeToolTip()
	{
		return HasValue(ChartStyleInfoStore.ToolTipProperty);
	}

	[DebuggerStepThrough]
	public void ResetToolTipFormat()
	{
		ResetValue(ChartStyleInfoStore.ToolTipFormatProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeToolTipFormat()
	{
		return HasValue(ChartStyleInfoStore.ToolTipFormatProperty);
	}

	[DebuggerStepThrough]
	public void ResetImages()
	{
		ResetValue(ChartStyleInfoStore.ImagesProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeImages()
	{
		return HasValue(ChartStyleInfoStore.ImagesProperty);
	}

	[DebuggerStepThrough]
	public void ResetImageIndex()
	{
		ResetValue(ChartStyleInfoStore.ImageIndexProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeImageIndex()
	{
		return HasValue(ChartStyleInfoStore.ImageIndexProperty);
	}

	[DebuggerStepThrough]
	public void ResetSymbol()
	{
		ResetValue(ChartStyleInfoStore.SymbolProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeSymbol()
	{
		return HasValue(ChartStyleInfoStore.SymbolProperty);
	}

	[DebuggerStepThrough]
	public void ResetCallout()
	{
		ResetValue(ChartStyleInfoStore.CalloutProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeCallout()
	{
		return HasValue(ChartStyleInfoStore.CalloutProperty);
	}

	[DebuggerStepThrough]
	public void ResetSystem()
	{
		ResetValue(ChartStyleInfoStore.SystemProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeSystem()
	{
		return HasValue(ChartStyleInfoStore.SystemProperty);
	}

	[DebuggerStepThrough]
	public void ResetName()
	{
		ResetValue(ChartStyleInfoStore.NameProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeName()
	{
		return HasValue(ChartStyleInfoStore.NameProperty);
	}

	[DebuggerStepThrough]
	public void ResetTextOrientation()
	{
		ResetValue(ChartStyleInfoStore.TextOrientationProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeTextOrientation()
	{
		return HasValue(ChartStyleInfoStore.TextOrientationProperty);
	}

	[DebuggerStepThrough]
	public void ResetDisplayShadow()
	{
		ResetValue(ChartStyleInfoStore.DisplayShadowProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeDisplayShadow()
	{
		return HasValue(ChartStyleInfoStore.DisplayShadowProperty);
	}

	[DebuggerStepThrough]
	public void ResetShadowOffset()
	{
		ResetValue(ChartStyleInfoStore.ShadowOffsetProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeShadowOffset()
	{
		return HasValue(ChartStyleInfoStore.ShadowOffsetProperty);
	}

	[DebuggerStepThrough]
	public void ResetShadowInterior()
	{
		ResetValue(ChartStyleInfoStore.ShadowInteriorProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeShadowInterior()
	{
		return HasValue(ChartStyleInfoStore.ShadowInteriorProperty);
	}

	[DebuggerStepThrough]
	[Obsolete("This method isn't used anymore.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void ResetHighlightOnMouseOver()
	{
		ResetValue(ChartStyleInfoStore.HighlightOnMouseOverProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeHighlightOnMouseOver()
	{
		return HasValue(ChartStyleInfoStore.HighlightOnMouseOverProperty);
	}

	[DebuggerStepThrough]
	[Obsolete("Use ResetHighlightInterior")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void ResetHihglightInterior()
	{
		ResetValue(ChartStyleInfoStore.HighlightInteriorProperty);
	}

	[DebuggerStepThrough]
	public void ResetHighlightInterior()
	{
		ResetValue(ChartStyleInfoStore.HighlightInteriorProperty);
	}

	[DebuggerStepThrough]
	public void ResetDimmedInterior()
	{
		ResetValue(ChartStyleInfoStore.DimmedInteriorProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeHighlightInterior()
	{
		return HasValue(ChartStyleInfoStore.HighlightInteriorProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeDimmedInterior()
	{
		return HasValue(ChartStyleInfoStore.DimmedInteriorProperty);
	}

	[DebuggerStepThrough]
	public void ResetHitTestRadius()
	{
		ResetValue(ChartStyleInfoStore.HitTestRadiusProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeHitTestRadius()
	{
		return HasValue(ChartStyleInfoStore.HitTestRadiusProperty);
	}

	[DebuggerStepThrough]
	public void ResetLabel()
	{
		ResetValue(ChartStyleInfoStore.LabelProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeLabel()
	{
		return HasValue(ChartStyleInfoStore.LabelProperty);
	}

	[DebuggerStepThrough]
	public void ResetTextFormat()
	{
		ResetValue(ChartStyleInfoStore.TextFormatProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeTextFormat()
	{
		return HasValue(ChartStyleInfoStore.TextFormatProperty);
	}

	[DebuggerStepThrough]
	public void ResetFormat()
	{
		ResetValue(ChartStyleInfoStore.FormatProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeFormat()
	{
		return HasValue(ChartStyleInfoStore.FormatProperty);
	}

	[DebuggerStepThrough]
	public void ResetDisplayText()
	{
		ResetValue(ChartStyleInfoStore.DisplayTextProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeDisplayText()
	{
		return HasValue(ChartStyleInfoStore.DisplayTextProperty);
	}

	[DebuggerStepThrough]
	public void ResetPointWidth()
	{
		ResetValue(ChartStyleInfoStore.PointWidthProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializePointWidth()
	{
		return HasValue(ChartStyleInfoStore.PointWidthProperty);
	}

	[DebuggerStepThrough]
	public void ResetTextOffset()
	{
		ResetValue(ChartStyleInfoStore.TextOffsetProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeTextOffset()
	{
		return HasValue(ChartStyleInfoStore.TextOffsetProperty);
	}

	[DebuggerStepThrough]
	public void ResetRelatedPoints()
	{
		ResetValue(ChartStyleInfoStore.RelatedPointsProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeRelatedPoints()
	{
		return HasValue(ChartStyleInfoStore.RelatedPointsProperty);
	}

	[DebuggerStepThrough]
	public void ResetUrl()
	{
		ResetValue(ChartStyleInfoStore.UrlProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	private bool ShouldSerializeUrl()
	{
		return HasValue(ChartStyleInfoStore.UrlProperty);
	}
}
