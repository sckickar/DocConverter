using System;
using System.ComponentModel;
using System.Diagnostics;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;
using DocGen.Styles;

namespace DocGen.Chart;

internal class ChartFontInfo : ChartSubStyleInfoBase
{
	private const int MaxOrientation = 360;

	private const int MaxSize = 512;

	private const float MaxSizeF = 128f;

	private const int MaxUnit = 6;

	private const int AllFlags = int.MaxValue;

	private const char separator = ';';

	private const int FW_BOLD = 700;

	private static ChartFontInfoStore m_store = new ChartFontInfoStore();

	[ThreadStatic]
	private static ChartFontInfo defaultFont;

	private Font _font;

	public static ChartFontInfo Default
	{
		[DebuggerStepThrough]
		get
		{
			if (defaultFont == null)
			{
				defaultFont = new ChartFontInfo();
				Font font = Control.DefaultFont;
				defaultFont.Facename = font.Name;
				defaultFont.Size = font.Size;
				defaultFont.Unit = font.Unit;
				defaultFont.FontStyle = font.Style;
				defaultFont.Orientation = 0;
			}
			return defaultFont;
		}
	}

	[Browsable(false)]
	public Font GdipFont
	{
		get
		{
			Font font = null;
			return (_font != null) ? _font : (_font = GetGdipFont());
		}
		internal set
		{
			_font = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[MergableProperty(true)]
	public FontStyle FontStyle
	{
		[DebuggerStepThrough]
		get
		{
			return GetFontStyle();
		}
		[DebuggerStepThrough]
		set
		{
			SetFontStyle(value);
			try
			{
				TestGdipFont();
			}
			catch (Exception ex)
			{
				_ = ex.InnerException;
				throw new Exception(FontStyle.ToString() + " is not available for " + Facename, ex);
			}
		}
	}

	[Editor("System.Drawing.Design.FontNameEditor, System.Drawing.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[Browsable(true)]
	[Description("Gets or sets the face name of this font object.")]
	[Category("Appearance")]
	public string Facename
	{
		[DebuggerStepThrough]
		get
		{
			return (string)GetValue(ChartFontInfoStore.FacenameProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartFontInfoStore.FacenameProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasName
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartFontInfoStore.FacenameProperty);
		}
	}

	[Browsable(true)]
	[Description("Gets or sets the size in pixels of this font object.")]
	[Category("Appearance")]
	public float Size
	{
		[DebuggerStepThrough]
		get
		{
			return (float)GetShortValue(ChartFontInfoStore.SizeProperty) / 4f;
		}
		[DebuggerStepThrough]
		set
		{
			if (value < 0f || value > 128f)
			{
				throw new ArgumentOutOfRangeException("value", value, $"Size must be between 0 and {128f}.");
			}
			SetValue(ChartFontInfoStore.SizeProperty, (short)(value * 4f));
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasSize
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartFontInfoStore.SizeProperty);
		}
	}

	[Browsable(true)]
	[Description("Gets or sets the orientation of this font object.")]
	[Category("Appearance")]
	public int Orientation
	{
		get
		{
			return GetShortValue(ChartFontInfoStore.OrientationProperty);
		}
		set
		{
			while (value < 0)
			{
				value += 360;
			}
			while (value >= 360)
			{
				value -= 360;
			}
			SetValue(ChartFontInfoStore.OrientationProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasOrientation
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartFontInfoStore.OrientationProperty);
		}
	}

	[Browsable(true)]
	[Description("Gets or sets a value that indicates whether this font object is bold.")]
	[Category("Appearance")]
	public bool Bold
	{
		[DebuggerStepThrough]
		get
		{
			return GetShortValue(ChartFontInfoStore.BoldProperty) != 0;
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartFontInfoStore.BoldProperty, value ? 1 : 0);
			try
			{
				TestGdipFont();
			}
			catch (Exception ex)
			{
				_ = ex.InnerException;
				throw new Exception(FontStyle.ToString() + " is not available for " + Facename, ex);
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasBold
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartFontInfoStore.BoldProperty);
		}
	}

	[Browsable(true)]
	[Description("Gets or sets a value that indicates whether this font object is italic.")]
	[Category("Appearance")]
	public bool Italic
	{
		[DebuggerStepThrough]
		get
		{
			return GetShortValue(ChartFontInfoStore.ItalicProperty) != 0;
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartFontInfoStore.ItalicProperty, value ? 1 : 0);
			try
			{
				TestGdipFont();
			}
			catch (Exception ex)
			{
				_ = ex.InnerException;
				throw new Exception(FontStyle.ToString() + " is not available for " + Facename, ex);
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasItalic
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartFontInfoStore.ItalicProperty);
		}
	}

	[Browsable(true)]
	[Description("Gets or sets a value that indicates whether this font object is underlined.")]
	[Category("Appearance")]
	public bool Underline
	{
		[DebuggerStepThrough]
		get
		{
			return GetShortValue(ChartFontInfoStore.UnderlineProperty) != 0;
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartFontInfoStore.UnderlineProperty, value ? 1 : 0);
			try
			{
				TestGdipFont();
			}
			catch (Exception ex)
			{
				_ = ex.InnerException;
				throw new Exception(FontStyle.ToString() + " is not available for " + Facename, ex);
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasUnderline
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartFontInfoStore.UnderlineProperty);
		}
	}

	[Browsable(true)]
	[Description("Gets or sets a value that indicates whether this font object should draw a horizontal line through the text.")]
	[Category("Appearance")]
	public bool Strikeout
	{
		[DebuggerStepThrough]
		get
		{
			return GetShortValue(ChartFontInfoStore.StrikeoutProperty) != 0;
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartFontInfoStore.StrikeoutProperty, value ? 1 : 0);
			try
			{
				TestGdipFont();
			}
			catch (Exception ex)
			{
				_ = ex.InnerException;
				throw new Exception(FontStyle.ToString() + " is not available for " + Facename, ex);
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasStrikeout
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartFontInfoStore.StrikeoutProperty);
		}
	}

	[Browsable(true)]
	[Description("Gets or sets the graphics unit of this font object.")]
	[Category("Appearance")]
	public GraphicsUnit Unit
	{
		[DebuggerStepThrough]
		get
		{
			return (GraphicsUnit)GetShortValue(ChartFontInfoStore.UnitProperty);
		}
		[DebuggerStepThrough]
		set
		{
			if (!Enum.IsDefined(typeof(GraphicsUnit), value))
			{
				throw new InvalidEnumArgumentException("value", (int)value, typeof(GraphicsUnit));
			}
			SetValue(ChartFontInfoStore.UnitProperty, (short)value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasUnit
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartFontInfoStore.UnitProperty);
		}
	}

	[Browsable(true)]
	[Description("Gets or sets the font family of this font object.")]
	[Category("Appearance")]
	public FontFamily FontFamilyTemplate
	{
		[DebuggerStepThrough]
		get
		{
			return (FontFamily)GetValue(ChartFontInfoStore.FontFamilyTemplateProperty);
		}
		[DebuggerStepThrough]
		set
		{
			SetValue(ChartFontInfoStore.FontFamilyTemplateProperty, value);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool HasFontFamilyTemplate
	{
		[DebuggerStepThrough]
		get
		{
			return HasValue(ChartFontInfoStore.FontFamilyTemplateProperty);
		}
	}

	internal static object CreateObject(StyleInfoSubObjectIdentity identity, object store)
	{
		if (store != null)
		{
			return new ChartFontInfo(identity, store as ChartFontInfoStore);
		}
		return new ChartFontInfo(identity);
	}

	public override void Dispose()
	{
		if (m_store != null)
		{
			m_store.Dispose();
			m_store = null;
		}
		_font = null;
		defaultFont = null;
		base.Dispose();
	}

	[DebuggerStepThrough]
	public ChartFontInfo()
		: base(ChartFontInfoStore.InitializeStaticVariables())
	{
	}

	~ChartFontInfo()
	{
		if (defaultFont != null)
		{
			defaultFont = null;
		}
	}

	[DebuggerStepThrough]
	public ChartFontInfo(StyleInfoSubObjectIdentity identity)
		: base(identity, ChartFontInfoStore.InitializeStaticVariables())
	{
	}

	[DebuggerStepThrough]
	public ChartFontInfo(StyleInfoSubObjectIdentity identity, ChartFontInfoStore store)
		: base(identity, store)
	{
	}

	[DebuggerStepThrough]
	public override IStyleInfoSubObject MakeCopy(StyleInfoBase newOwner, StyleInfoProperty sip)
	{
		return new ChartFontInfo(newOwner.CreateSubObjectIdentity(sip), (ChartFontInfoStore)base.Store.Clone());
	}

	public static float SizeInWorldUnit(Font font)
	{
		if (font.Unit == GraphicsUnit.World)
		{
			return font.Size;
		}
		float sizeInPoints = font.SizeInPoints;
		Font font2 = null;
		try
		{
			font2 = new Font(font.GetFontName(), sizeInPoints * 10f, FontStyle.Regular, GraphicsUnit.World);
		}
		catch (Exception ex)
		{
			_ = ex.InnerException;
			throw ex;
		}
		if (font2 == null)
		{
			font2 = new Font(font.Name, sizeInPoints * 10f, FontStyle.Regular, GraphicsUnit.World);
		}
		float sizeInPoints2 = font2.SizeInPoints;
		font2.Dispose();
		return sizeInPoints * (sizeInPoints * 10f) / sizeInPoints2;
	}

	internal void ResetGdipFont()
	{
		_font = null;
	}

	private Font GetGdipFont()
	{
		FontFamily fontFamilyTemplate = FontFamilyTemplate;
		if (fontFamilyTemplate == null)
		{
			return new Font(Facename, Size, FontStyle, Unit);
		}
		Font font = null;
		try
		{
			font = new Font(fontFamilyTemplate.Name, Size, FontStyle, Unit);
		}
		catch (Exception)
		{
		}
		if (font == null)
		{
			font = new Font(Facename, Size, FontStyle, Unit);
		}
		return font;
	}

	private void TestGdipFont()
	{
	}

	protected override void OnStyleChanged(StyleInfoProperty sip)
	{
		_font = null;
		base.OnStyleChanged(sip);
	}

	protected internal override StyleInfoBase GetDefaultStyle()
	{
		return Default;
	}

	private void SetFontStyle(FontStyle fontStyle)
	{
		Bold = (fontStyle & FontStyle.Bold) != 0;
		Italic = (fontStyle & FontStyle.Italic) != 0;
		Strikeout = (fontStyle & FontStyle.Strikeout) != 0;
		Underline = (fontStyle & FontStyle.Underline) != 0;
	}

	private FontStyle GetFontStyle()
	{
		FontStyle fontStyle = FontStyle.Regular;
		if (Bold)
		{
			fontStyle |= FontStyle.Bold;
		}
		if (Italic)
		{
			fontStyle |= FontStyle.Italic;
		}
		if (Strikeout)
		{
			fontStyle |= FontStyle.Strikeout;
		}
		if (Underline)
		{
			fontStyle |= FontStyle.Underline;
		}
		return fontStyle;
	}

	[DebuggerStepThrough]
	public void ResetName()
	{
		ResetValue(ChartFontInfoStore.FacenameProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected bool ShouldSerializeName()
	{
		return HasValue(ChartFontInfoStore.FacenameProperty);
	}

	[DebuggerStepThrough]
	public void ResetSize()
	{
		ResetValue(ChartFontInfoStore.SizeProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected bool ShouldSerializeSize()
	{
		return HasValue(ChartFontInfoStore.SizeProperty);
	}

	[DebuggerStepThrough]
	public void ResetOrientation()
	{
		ResetValue(ChartFontInfoStore.OrientationProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected bool ShouldSerializeOrientation()
	{
		return HasValue(ChartFontInfoStore.OrientationProperty);
	}

	[DebuggerStepThrough]
	public void ResetBold()
	{
		ResetValue(ChartFontInfoStore.BoldProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected bool ShouldSerializeBold()
	{
		return HasValue(ChartFontInfoStore.BoldProperty);
	}

	[DebuggerStepThrough]
	public void ResetItalic()
	{
		ResetValue(ChartFontInfoStore.ItalicProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected bool ShouldSerializeItalic()
	{
		return HasValue(ChartFontInfoStore.ItalicProperty);
	}

	[DebuggerStepThrough]
	public void ResetUnderline()
	{
		ResetValue(ChartFontInfoStore.UnderlineProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected bool ShouldSerializeUnderline()
	{
		return HasValue(ChartFontInfoStore.UnderlineProperty);
	}

	[DebuggerStepThrough]
	public void ResetStrikeout()
	{
		ResetValue(ChartFontInfoStore.StrikeoutProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected bool ShouldSerializeStrikeout()
	{
		return HasValue(ChartFontInfoStore.StrikeoutProperty);
	}

	[DebuggerStepThrough]
	public void ResetUnit()
	{
		ResetValue(ChartFontInfoStore.UnitProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected bool ShouldSerializeUnit()
	{
		return HasValue(ChartFontInfoStore.UnitProperty);
	}

	[DebuggerStepThrough]
	public void ResetFontFamilyTemplate()
	{
		ResetValue(ChartFontInfoStore.FontFamilyTemplateProperty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected bool ShouldSerializeFontFamilyTemplate()
	{
		return HasValue(ChartFontInfoStore.FontFamilyTemplateProperty);
	}
}
