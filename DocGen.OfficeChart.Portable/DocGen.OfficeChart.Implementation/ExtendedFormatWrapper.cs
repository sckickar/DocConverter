using System;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Interfaces;

namespace DocGen.OfficeChart.Implementation;

internal class ExtendedFormatWrapper : CommonWrapper, IInternalExtendedFormat, IExtendedFormat, IParentApplication, IXFIndex, IStyle, IOptimizedUpdate, ICloneParent
{
	protected ExtendedFormatImpl m_xFormat;

	protected WorkbookImpl m_book;

	protected FontWrapper m_font;

	private BordersCollection m_borders;

	private InteriorWrapper m_interior;

	public WorkbookImpl Workbook => m_book;

	public OfficePattern FillPattern
	{
		get
		{
			BeforeRead();
			return m_xFormat.FillPattern;
		}
		set
		{
			if (FillPattern != value)
			{
				BeginUpdate();
				m_xFormat.FillPattern = value;
				EndUpdate();
			}
		}
	}

	public int XFormatIndex
	{
		get
		{
			BeforeRead();
			return m_xFormat.XFormatIndex;
		}
	}

	public OfficeKnownColors FillBackground
	{
		get
		{
			BeforeRead();
			return m_xFormat.FillBackground;
		}
		set
		{
			if (FillBackground != value)
			{
				BeginUpdate();
				m_xFormat.FillBackground = value;
				ChangeFillPattern();
				EndUpdate();
			}
		}
	}

	public Color FillBackgroundRGB
	{
		get
		{
			BeforeRead();
			return m_xFormat.FillBackgroundRGB;
		}
		set
		{
			if (FillBackgroundRGB != value)
			{
				BeginUpdate();
				m_xFormat.FillBackgroundRGB = value;
				ChangeFillPattern();
				EndUpdate();
			}
		}
	}

	public OfficeKnownColors FillForeground
	{
		get
		{
			BeforeRead();
			return m_xFormat.FillForeground;
		}
		set
		{
			if (FillForeground != value)
			{
				BeginUpdate();
				m_xFormat.FillForeground = value;
				ChangeFillPattern();
				EndUpdate();
			}
		}
	}

	public Color FillForegroundRGB
	{
		get
		{
			BeforeRead();
			return m_xFormat.FillForegroundRGB;
		}
		set
		{
			if (FillForegroundRGB != value)
			{
				BeginUpdate();
				m_xFormat.FillForegroundRGB = value;
				ChangeFillPattern();
				EndUpdate();
			}
		}
	}

	public int NumberFormatIndex
	{
		get
		{
			BeforeRead();
			return m_xFormat.NumberFormatIndex;
		}
		set
		{
			if (NumberFormatIndex != value)
			{
				BeginUpdate();
				m_xFormat.NumberFormatIndex = value;
				EndUpdate();
				OnNumberFormatChange();
			}
		}
	}

	public OfficeHAlign HorizontalAlignment
	{
		get
		{
			BeforeRead();
			return m_xFormat.HorizontalAlignment;
		}
		set
		{
			if (HorizontalAlignment != value)
			{
				BeginUpdate();
				m_xFormat.HorizontalAlignment = value;
				EndUpdate();
			}
		}
	}

	public bool IncludeAlignment
	{
		get
		{
			BeforeRead();
			return m_xFormat.IncludeAlignment;
		}
		set
		{
			if (IncludeAlignment != value)
			{
				BeginUpdate();
				m_xFormat.IncludeAlignment = value;
				EndUpdate();
			}
		}
	}

	public bool IncludeBorder
	{
		get
		{
			BeforeRead();
			return m_xFormat.IncludeBorder;
		}
		set
		{
			if (IncludeBorder != value)
			{
				BeginUpdate();
				m_xFormat.IncludeBorder = value;
				EndUpdate();
			}
		}
	}

	public bool IncludeFont
	{
		get
		{
			BeforeRead();
			return m_xFormat.IncludeFont;
		}
		set
		{
			if (IncludeFont != value)
			{
				BeginUpdate();
				m_xFormat.IncludeFont = value;
				EndUpdate();
			}
		}
	}

	public bool IncludeNumberFormat
	{
		get
		{
			BeforeRead();
			return m_xFormat.IncludeNumberFormat;
		}
		set
		{
			if (IncludeNumberFormat != value)
			{
				BeginUpdate();
				m_xFormat.IncludeNumberFormat = value;
				EndUpdate();
			}
		}
	}

	public bool IncludePatterns
	{
		get
		{
			BeforeRead();
			return m_xFormat.IncludePatterns;
		}
		set
		{
			if (IncludePatterns != value)
			{
				BeginUpdate();
				m_xFormat.IncludePatterns = value;
				EndUpdate();
			}
		}
	}

	public bool IncludeProtection
	{
		get
		{
			BeforeRead();
			return m_xFormat.IncludeProtection;
		}
		set
		{
			if (IncludeProtection != value)
			{
				BeginUpdate();
				m_xFormat.IncludeProtection = value;
				EndUpdate();
			}
		}
	}

	public int IndentLevel
	{
		get
		{
			BeforeRead();
			return m_xFormat.IndentLevel;
		}
		set
		{
			if (IndentLevel != value)
			{
				BeginUpdate();
				m_xFormat.IndentLevel = value;
				EndUpdate();
			}
		}
	}

	public bool FormulaHidden
	{
		get
		{
			BeforeRead();
			return m_xFormat.FormulaHidden;
		}
		set
		{
			if (FormulaHidden != value)
			{
				BeginUpdate();
				m_xFormat.FormulaHidden = value;
				EndUpdate();
			}
		}
	}

	public bool Locked
	{
		get
		{
			BeforeRead();
			return m_xFormat.Locked;
		}
		set
		{
			if (Locked != value)
			{
				BeginUpdate();
				m_xFormat.Locked = value;
				EndUpdate();
			}
		}
	}

	public bool JustifyLast
	{
		get
		{
			BeforeRead();
			return m_xFormat.JustifyLast;
		}
		set
		{
			if (JustifyLast != value)
			{
				BeginUpdate();
				m_xFormat.JustifyLast = value;
				EndUpdate();
			}
		}
	}

	public string NumberFormat
	{
		get
		{
			BeforeRead();
			return m_xFormat.NumberFormat;
		}
		set
		{
			if (NumberFormat != value)
			{
				BeginUpdate();
				m_xFormat.NumberFormat = value;
				EndUpdate();
				OnNumberFormatChange();
			}
		}
	}

	public string NumberFormatLocal
	{
		get
		{
			BeforeRead();
			return m_xFormat.NumberFormatLocal;
		}
		set
		{
			if (NumberFormatLocal != value)
			{
				BeginUpdate();
				m_xFormat.NumberFormatLocal = value;
				EndUpdate();
				OnNumberFormatChange();
			}
		}
	}

	public INumberFormat NumberFormatSettings
	{
		get
		{
			BeforeRead();
			return m_xFormat.NumberFormatSettings;
		}
	}

	public OfficeReadingOrderType ReadingOrder
	{
		get
		{
			BeforeRead();
			return m_xFormat.ReadingOrder;
		}
		set
		{
			if (ReadingOrder != value)
			{
				BeginUpdate();
				m_xFormat.ReadingOrder = value;
				EndUpdate();
			}
		}
	}

	public int Rotation
	{
		get
		{
			BeforeRead();
			return m_xFormat.Rotation;
		}
		set
		{
			if (Rotation != value)
			{
				BeginUpdate();
				m_xFormat.Rotation = value;
				EndUpdate();
			}
		}
	}

	public bool ShrinkToFit
	{
		get
		{
			BeforeRead();
			return m_xFormat.ShrinkToFit;
		}
		set
		{
			if (ShrinkToFit != value)
			{
				BeginUpdate();
				m_xFormat.ShrinkToFit = value;
				EndUpdate();
			}
		}
	}

	public OfficeVAlign VerticalAlignment
	{
		get
		{
			BeforeRead();
			return m_xFormat.VerticalAlignment;
		}
		set
		{
			if (VerticalAlignment != value)
			{
				BeginUpdate();
				m_xFormat.VerticalAlignment = value;
				EndUpdate();
			}
		}
	}

	public bool WrapText
	{
		get
		{
			BeforeRead();
			return m_xFormat.WrapText;
		}
		set
		{
			if (WrapText != value)
			{
				BeginUpdate();
				m_xFormat.WrapText = value;
				EndUpdate();
			}
		}
	}

	public IOfficeFont Font
	{
		get
		{
			BeforeRead();
			return m_font;
		}
	}

	public IBorders Borders
	{
		get
		{
			BeforeRead();
			if (m_borders == null)
			{
				m_borders = new BordersCollection(Application, this, this);
			}
			return m_borders;
		}
	}

	public bool IsFirstSymbolApostrophe
	{
		get
		{
			BeforeRead();
			return m_xFormat.IsFirstSymbolApostrophe;
		}
		set
		{
			if (IsFirstSymbolApostrophe != value)
			{
				if (!m_book.IsLoaded && !m_book.IsWorkbookOpening)
				{
					BeginUpdate();
				}
				m_xFormat.IsFirstSymbolApostrophe = value;
				if (!m_book.IsLoaded && !m_book.IsWorkbookOpening)
				{
					EndUpdate();
				}
			}
		}
	}

	public OfficeKnownColors PatternColorIndex
	{
		get
		{
			BeforeRead();
			return m_xFormat.PatternColorIndex;
		}
		set
		{
			if (PatternColorIndex != value || FillPattern == OfficePattern.Gradient)
			{
				BeginUpdate();
				m_xFormat.PatternColorIndex = value;
				ChangeFillPattern();
				EndUpdate();
			}
		}
	}

	public Color PatternColor
	{
		get
		{
			BeforeRead();
			return m_xFormat.PatternColor;
		}
		set
		{
			if (PatternColor != value || PatternColorIndex == OfficeKnownColors.BlackCustom)
			{
				BeginUpdate();
				m_xFormat.PatternColor = value;
				ChangeFillPattern();
				EndUpdate();
			}
		}
	}

	public OfficeKnownColors ColorIndex
	{
		get
		{
			BeforeRead();
			return m_xFormat.ColorIndex;
		}
		set
		{
			if (FillPattern == OfficePattern.Gradient || ColorIndex != value)
			{
				BeginUpdate();
				m_xFormat.ColorIndex = value;
				ChangeFillPattern();
				EndUpdate();
			}
		}
	}

	public Color Color
	{
		get
		{
			BeforeRead();
			return m_xFormat.Color;
		}
		set
		{
			if (Color != value || ColorIndex == (OfficeKnownColors)65)
			{
				BeginUpdate();
				m_xFormat.Color = value;
				ChangeFillPattern();
				EndUpdate();
			}
		}
	}

	public IInterior Interior
	{
		get
		{
			if (m_interior == null)
			{
				m_interior = new InteriorWrapper(m_xFormat);
				m_interior.AfterChangeEvent += WrappedInteriorAfterChangeEvent;
			}
			BeforeRead();
			return m_interior;
		}
	}

	public bool IsModified
	{
		get
		{
			BeforeRead();
			return m_xFormat.IsModified;
		}
	}

	public int FontIndex
	{
		get
		{
			BeforeRead();
			return m_xFormat.FontIndex;
		}
		set
		{
			if (FontIndex != value)
			{
				BeginUpdate();
				m_xFormat.FontIndex = value;
				EndUpdate();
			}
		}
	}

	public ExtendedFormatImpl Wrapped
	{
		get
		{
			BeforeRead();
			return m_xFormat;
		}
	}

	public bool HasBorder => m_xFormat.HasBorder;

	public virtual ChartColor BottomBorderColor
	{
		get
		{
			BeforeRead();
			return m_xFormat.BottomBorderColor;
		}
	}

	public virtual ChartColor TopBorderColor
	{
		get
		{
			BeforeRead();
			return m_xFormat.TopBorderColor;
		}
	}

	public virtual ChartColor LeftBorderColor
	{
		get
		{
			BeforeRead();
			return m_xFormat.LeftBorderColor;
		}
	}

	public virtual ChartColor RightBorderColor
	{
		get
		{
			BeforeRead();
			return m_xFormat.RightBorderColor;
		}
	}

	public ChartColor DiagonalBorderColor
	{
		get
		{
			BeforeRead();
			return m_xFormat.DiagonalBorderColor;
		}
	}

	public virtual OfficeLineStyle LeftBorderLineStyle
	{
		get
		{
			BeforeRead();
			return m_xFormat.LeftBorderLineStyle;
		}
		set
		{
			BeginUpdate();
			m_xFormat.LeftBorderLineStyle = value;
			EndUpdate();
		}
	}

	public virtual OfficeLineStyle RightBorderLineStyle
	{
		get
		{
			BeforeRead();
			return m_xFormat.RightBorderLineStyle;
		}
		set
		{
			BeginUpdate();
			m_xFormat.RightBorderLineStyle = value;
			EndUpdate();
		}
	}

	public virtual OfficeLineStyle TopBorderLineStyle
	{
		get
		{
			BeforeRead();
			return m_xFormat.TopBorderLineStyle;
		}
		set
		{
			BeginUpdate();
			m_xFormat.TopBorderLineStyle = value;
			EndUpdate();
		}
	}

	public virtual OfficeLineStyle BottomBorderLineStyle
	{
		get
		{
			BeforeRead();
			return m_xFormat.BottomBorderLineStyle;
		}
		set
		{
			BeginUpdate();
			m_xFormat.BottomBorderLineStyle = value;
			EndUpdate();
		}
	}

	public OfficeLineStyle DiagonalUpBorderLineStyle
	{
		get
		{
			BeforeRead();
			return m_xFormat.DiagonalUpBorderLineStyle;
		}
		set
		{
			bool flag = false;
			if (DiagonalUpBorderLineStyle != value)
			{
				BeginUpdate();
				m_xFormat.DiagonalUpBorderLineStyle = value;
				flag = true;
			}
			if (!m_xFormat.DiagonalUpVisible && value != 0)
			{
				if (!flag)
				{
					BeginUpdate();
				}
				m_xFormat.DiagonalUpVisible = true;
				flag = true;
			}
			if (flag)
			{
				EndUpdate();
			}
		}
	}

	public OfficeLineStyle DiagonalDownBorderLineStyle
	{
		get
		{
			BeforeRead();
			return m_xFormat.DiagonalDownBorderLineStyle;
		}
		set
		{
			bool flag = false;
			if (DiagonalDownBorderLineStyle != value)
			{
				BeginUpdate();
				m_xFormat.DiagonalDownBorderLineStyle = value;
				flag = true;
			}
			if (!m_xFormat.DiagonalDownVisible)
			{
				if (!flag)
				{
					BeginUpdate();
				}
				m_xFormat.DiagonalDownVisible = true;
				flag = true;
			}
			if (flag)
			{
				EndUpdate();
			}
		}
	}

	public bool DiagonalUpVisible
	{
		get
		{
			BeforeRead();
			return m_xFormat.DiagonalUpVisible;
		}
		set
		{
			if (DiagonalUpVisible != value)
			{
				BeginUpdate();
				m_xFormat.DiagonalUpVisible = value;
				EndUpdate();
			}
		}
	}

	public bool DiagonalDownVisible
	{
		get
		{
			BeforeRead();
			return m_xFormat.DiagonalDownVisible;
		}
		set
		{
			if (DiagonalDownVisible != value)
			{
				BeginUpdate();
				m_xFormat.DiagonalDownVisible = value;
				EndUpdate();
			}
		}
	}

	public IApplication Application => m_xFormat.Application;

	public object Parent => m_xFormat.Parent;

	public bool BuiltIn => GetStyle().BuiltIn;

	public string Name
	{
		get
		{
			BeforeRead();
			int parentIndex = m_xFormat.Record.ParentIndex;
			StyleImpl styleImpl = m_book.InnerStyles.GetByXFIndex(parentIndex);
			if (styleImpl == null)
			{
				styleImpl = m_book.InnerStyles["Normal"] as StyleImpl;
				m_xFormat.ParentIndex = (ushort)styleImpl.Index;
			}
			return styleImpl.Name;
		}
	}

	public bool IsInitialized
	{
		get
		{
			BeforeRead();
			string name = m_book.AppImplementation.DefaultStyleNames[0];
			return !StylesCollection.CompareStyles(this, m_book.Styles[name]);
		}
	}

	public event EventHandler NumberFormatChanged;

	public ExtendedFormatWrapper(WorkbookImpl book)
	{
		m_book = book;
	}

	public ExtendedFormatWrapper(WorkbookImpl book, int iXFIndex)
		: this(book)
	{
		SetFormatIndex(iXFIndex);
	}

	public void ChangeFillPattern()
	{
		OfficePattern fillPattern = m_xFormat.FillPattern;
		if (fillPattern == OfficePattern.None || fillPattern == OfficePattern.Gradient)
		{
			m_xFormat.FillPattern = OfficePattern.Solid;
			m_xFormat.Gradient = null;
		}
	}

	public void SetFormatIndex(int index)
	{
		if (m_xFormat == null || m_xFormat.Index != index)
		{
			m_xFormat = m_book.InnerExtFormats[index];
			if (m_book.InnerFonts.Count <= m_xFormat.FontIndex)
			{
				m_xFormat.FontIndex = 0;
			}
			int fontIndex = m_xFormat.FontIndex;
			FontImpl wrapped = m_book.InnerFonts[fontIndex] as FontImpl;
			if (m_font == null)
			{
				m_font = new FontWrapper();
				m_font.AfterChangeEvent += WrappedFontAfterChangeEvent;
			}
			m_font.Wrapped = wrapped;
		}
	}

	public void UpdateFont()
	{
		m_font.Wrapped = (FontImpl)m_xFormat.Font;
	}

	protected virtual void SetParents(object parent)
	{
		m_book = CommonObject.FindParent(parent, typeof(WorkbookImpl)) as WorkbookImpl;
		if (m_book == null)
		{
			throw new ArgumentNullException("Workbook", "Can't find parent workbook");
		}
	}

	protected void SetChanged()
	{
		m_book.SetChanged();
	}

	private void WrappedFontAfterChangeEvent(object sender, EventArgs e)
	{
		FontIndex = m_font.FontIndex;
	}

	private void WrappedInteriorAfterChangeEvent(object sender, EventArgs e)
	{
		BeginUpdate();
		m_xFormat = m_interior.Wrapped;
		EndUpdate();
	}

	protected void OnNumberFormatChange()
	{
		if (this.NumberFormatChanged != null)
		{
			this.NumberFormatChanged(this, EventArgs.Empty);
		}
	}

	public override object Clone(object parent)
	{
		ExtendedFormatWrapper obj = (ExtendedFormatWrapper)base.Clone(parent);
		obj.m_book = CommonObject.FindParent(parent, typeof(WorkbookImpl)) as WorkbookImpl;
		if (obj.m_book == null)
		{
			throw new ArgumentOutOfRangeException("parent", "Can't find parent workbook.");
		}
		obj.m_borders = null;
		obj.m_xFormat = null;
		obj.m_font = null;
		obj.SetFormatIndex(m_xFormat.Index);
		return obj;
	}

	protected virtual void BeforeRead()
	{
	}

	private IStyle GetStyle()
	{
		int index = (m_xFormat.HasParent ? m_xFormat.ParentIndex : m_xFormat.XFormatIndex);
		return m_book.InnerStyles.GetByXFIndex(index);
	}

	public override void BeginUpdate()
	{
		base.BeginUpdate();
	}

	public override void EndUpdate()
	{
		base.EndUpdate();
		if (base.BeginCallsCount == 0)
		{
			SetChanged();
		}
	}

	internal void Dispose()
	{
		this.NumberFormatChanged = null;
		m_xFormat.clearAll();
		m_font.Dispose();
		if (m_borders != null)
		{
			m_borders.Clear();
		}
		if (m_interior != null)
		{
			m_interior.Dispose();
		}
	}
}
