using System;
using DocGen.Drawing;
using DocGen.OfficeChart.Interfaces;

namespace DocGen.OfficeChart.Implementation.Collections.Grouping;

internal class StyleGroup : CommonObject, IStyle, IExtendedFormat, IParentApplication, IOptimizedUpdate, IXFIndex
{
	private RangeGroup m_rangeGroup;

	private FontGroup m_font;

	private BordersGroup m_borders;

	public IStyle this[int index] => m_rangeGroup[index].CellStyle;

	public int Count => m_rangeGroup.Count;

	public WorkbookImpl Workbook => m_rangeGroup.Workbook;

	public IBorders Borders
	{
		get
		{
			if (m_borders == null)
			{
				m_borders = new BordersGroup(base.Application, this);
			}
			return m_borders;
		}
	}

	public bool BuiltIn
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return false;
			}
			bool builtIn = this[0].BuiltIn;
			for (int i = 1; i < count; i++)
			{
				if (builtIn != this[i].BuiltIn)
				{
					return false;
				}
			}
			return builtIn;
		}
	}

	public OfficePattern FillPattern
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return OfficePattern.None;
			}
			OfficePattern fillPattern = this[0].FillPattern;
			for (int i = 1; i < count; i++)
			{
				if (fillPattern != this[i].FillPattern)
				{
					return OfficePattern.None;
				}
			}
			return fillPattern;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].FillPattern = value;
			}
		}
	}

	public OfficeKnownColors FillBackground
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return OfficeKnownColors.Black;
			}
			OfficeKnownColors fillBackground = this[0].FillBackground;
			for (int i = 1; i < count; i++)
			{
				if (fillBackground != this[i].FillBackground)
				{
					return OfficeKnownColors.Black;
				}
			}
			return fillBackground;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].FillBackground = value;
			}
		}
	}

	public Color FillBackgroundRGB
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return ColorExtension.Empty;
			}
			Color fillBackgroundRGB = this[0].FillBackgroundRGB;
			for (int i = 1; i < count; i++)
			{
				if (fillBackgroundRGB != this[i].FillBackgroundRGB)
				{
					return ColorExtension.Empty;
				}
			}
			return fillBackgroundRGB;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].FillBackgroundRGB = value;
			}
		}
	}

	public OfficeKnownColors FillForeground
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return OfficeKnownColors.Black;
			}
			OfficeKnownColors fillForeground = this[0].FillForeground;
			for (int i = 1; i < count; i++)
			{
				if (fillForeground != this[i].FillForeground)
				{
					return OfficeKnownColors.Black;
				}
			}
			return fillForeground;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].FillForeground = value;
			}
		}
	}

	public Color FillForegroundRGB
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return ColorExtension.Empty;
			}
			Color fillForegroundRGB = this[0].FillForegroundRGB;
			for (int i = 1; i < count; i++)
			{
				if (fillForegroundRGB != this[i].FillForegroundRGB)
				{
					return ColorExtension.Empty;
				}
			}
			return fillForegroundRGB;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].FillForegroundRGB = value;
			}
		}
	}

	public IOfficeFont Font
	{
		get
		{
			if (m_font == null)
			{
				m_font = new FontGroup(base.Application, this);
			}
			return m_font;
		}
	}

	public IInterior Interior
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public bool FormulaHidden
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return false;
			}
			bool formulaHidden = this[0].FormulaHidden;
			for (int i = 1; i < count; i++)
			{
				if (formulaHidden != this[i].FormulaHidden)
				{
					return false;
				}
			}
			return formulaHidden;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].FormulaHidden = value;
			}
		}
	}

	public OfficeHAlign HorizontalAlignment
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return OfficeHAlign.HAlignGeneral;
			}
			OfficeHAlign horizontalAlignment = this[0].HorizontalAlignment;
			for (int i = 1; i < count; i++)
			{
				if (horizontalAlignment != this[i].HorizontalAlignment)
				{
					return OfficeHAlign.HAlignGeneral;
				}
			}
			return horizontalAlignment;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].HorizontalAlignment = value;
			}
		}
	}

	public bool IncludeAlignment
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return false;
			}
			bool includeAlignment = this[0].IncludeAlignment;
			for (int i = 1; i < count; i++)
			{
				if (includeAlignment != this[i].IncludeAlignment)
				{
					return false;
				}
			}
			return includeAlignment;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].IncludeAlignment = value;
			}
		}
	}

	public bool IncludeBorder
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return false;
			}
			bool includeBorder = this[0].IncludeBorder;
			for (int i = 1; i < count; i++)
			{
				if (includeBorder != this[i].IncludeBorder)
				{
					return false;
				}
			}
			return includeBorder;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].IncludeBorder = value;
			}
		}
	}

	public bool IncludeFont
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return false;
			}
			bool includeFont = this[0].IncludeFont;
			for (int i = 1; i < count; i++)
			{
				if (includeFont != this[i].IncludeFont)
				{
					return false;
				}
			}
			return includeFont;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].IncludeFont = value;
			}
		}
	}

	public bool IncludeNumberFormat
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return false;
			}
			bool includeNumberFormat = this[0].IncludeNumberFormat;
			for (int i = 1; i < count; i++)
			{
				if (includeNumberFormat != this[i].IncludeNumberFormat)
				{
					return false;
				}
			}
			return includeNumberFormat;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].IncludeNumberFormat = value;
			}
		}
	}

	public bool IncludePatterns
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return false;
			}
			bool includePatterns = this[0].IncludePatterns;
			for (int i = 1; i < count; i++)
			{
				if (includePatterns != this[i].IncludePatterns)
				{
					return false;
				}
			}
			return includePatterns;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].IncludePatterns = value;
			}
		}
	}

	public bool IncludeProtection
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return false;
			}
			bool includeProtection = this[0].IncludeProtection;
			for (int i = 1; i < count; i++)
			{
				if (includeProtection != this[i].IncludeProtection)
				{
					return false;
				}
			}
			return includeProtection;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].IncludeProtection = value;
			}
		}
	}

	public int IndentLevel
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return int.MinValue;
			}
			int indentLevel = this[0].IndentLevel;
			for (int i = 1; i < count; i++)
			{
				if (indentLevel != this[i].IndentLevel)
				{
					return int.MinValue;
				}
			}
			return indentLevel;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].IndentLevel = value;
			}
		}
	}

	public bool Locked
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return false;
			}
			bool locked = this[0].Locked;
			for (int i = 1; i < count; i++)
			{
				if (locked != this[i].Locked)
				{
					return false;
				}
			}
			return locked;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].Locked = value;
			}
		}
	}

	public string Name
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return null;
			}
			string name = this[0].Name;
			for (int i = 1; i < count; i++)
			{
				if (name != this[i].Name)
				{
					return null;
				}
			}
			return name;
		}
	}

	public string NumberFormat
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return null;
			}
			string numberFormat = this[0].NumberFormat;
			for (int i = 1; i < count; i++)
			{
				if (numberFormat != this[i].NumberFormat)
				{
					return null;
				}
			}
			return numberFormat;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].NumberFormat = value;
			}
		}
	}

	public string NumberFormatLocal
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return null;
			}
			string numberFormatLocal = this[0].NumberFormatLocal;
			for (int i = 1; i < count; i++)
			{
				if (numberFormatLocal != this[i].NumberFormatLocal)
				{
					return null;
				}
			}
			return numberFormatLocal;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].NumberFormatLocal = value;
			}
		}
	}

	public int NumberFormatIndex
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return int.MinValue;
			}
			int numberFormatIndex = this[0].NumberFormatIndex;
			for (int i = 1; i < count; i++)
			{
				if (numberFormatIndex != this[i].NumberFormatIndex)
				{
					return int.MinValue;
				}
			}
			return numberFormatIndex;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].NumberFormatIndex = value;
			}
		}
	}

	public INumberFormat NumberFormatSettings
	{
		get
		{
			if (NumberFormatIndex < 0)
			{
				return null;
			}
			return Workbook.InnerFormats[NumberFormatIndex];
		}
	}

	public int Rotation
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return int.MinValue;
			}
			int rotation = this[0].Rotation;
			for (int i = 1; i < count; i++)
			{
				if (rotation != this[i].Rotation)
				{
					return int.MinValue;
				}
			}
			return rotation;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].Rotation = value;
			}
		}
	}

	public bool ShrinkToFit
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return false;
			}
			bool shrinkToFit = this[0].ShrinkToFit;
			for (int i = 1; i < count; i++)
			{
				if (shrinkToFit != this[i].ShrinkToFit)
				{
					return false;
				}
			}
			return shrinkToFit;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].ShrinkToFit = value;
			}
		}
	}

	public OfficeVAlign VerticalAlignment
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return OfficeVAlign.VAlignTop;
			}
			OfficeVAlign verticalAlignment = this[0].VerticalAlignment;
			for (int i = 1; i < count; i++)
			{
				if (verticalAlignment != this[i].VerticalAlignment)
				{
					return OfficeVAlign.VAlignTop;
				}
			}
			return verticalAlignment;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].VerticalAlignment = value;
			}
		}
	}

	public bool WrapText
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return false;
			}
			bool wrapText = this[0].WrapText;
			for (int i = 1; i < count; i++)
			{
				if (wrapText != this[i].WrapText)
				{
					return false;
				}
			}
			return wrapText;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].WrapText = value;
			}
		}
	}

	public bool IsInitialized
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return false;
			}
			bool isInitialized = this[0].IsInitialized;
			for (int i = 1; i < count; i++)
			{
				if (isInitialized != this[i].IsInitialized)
				{
					return false;
				}
			}
			return isInitialized;
		}
	}

	public OfficeReadingOrderType ReadingOrder
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return OfficeReadingOrderType.Context;
			}
			OfficeReadingOrderType readingOrder = this[0].ReadingOrder;
			for (int i = 1; i < count; i++)
			{
				if (readingOrder != this[i].ReadingOrder)
				{
					return OfficeReadingOrderType.Context;
				}
			}
			return readingOrder;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].ReadingOrder = value;
			}
		}
	}

	public bool IsFirstSymbolApostrophe
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return false;
			}
			bool isFirstSymbolApostrophe = this[0].IsFirstSymbolApostrophe;
			for (int i = 1; i < count; i++)
			{
				if (isFirstSymbolApostrophe != this[i].IsFirstSymbolApostrophe)
				{
					return false;
				}
			}
			return isFirstSymbolApostrophe;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].IsFirstSymbolApostrophe = value;
			}
		}
	}

	public bool JustifyLast
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return false;
			}
			bool justifyLast = this[0].JustifyLast;
			for (int i = 1; i < count; i++)
			{
				if (justifyLast != this[i].JustifyLast)
				{
					return false;
				}
			}
			return justifyLast;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].JustifyLast = value;
			}
		}
	}

	public OfficeKnownColors PatternColorIndex
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return OfficeKnownColors.Black;
			}
			OfficeKnownColors patternColorIndex = this[0].PatternColorIndex;
			for (int i = 1; i < count; i++)
			{
				if (patternColorIndex != this[i].PatternColorIndex)
				{
					return OfficeKnownColors.Black;
				}
			}
			return patternColorIndex;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].PatternColorIndex = value;
			}
		}
	}

	public Color PatternColor
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return ColorExtension.Empty;
			}
			Color patternColor = this[0].PatternColor;
			for (int i = 1; i < count; i++)
			{
				if (patternColor != this[i].PatternColor)
				{
					return ColorExtension.Empty;
				}
			}
			return patternColor;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].PatternColor = value;
			}
		}
	}

	public OfficeKnownColors ColorIndex
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return OfficeKnownColors.Black;
			}
			OfficeKnownColors colorIndex = this[0].ColorIndex;
			for (int i = 1; i < count; i++)
			{
				if (colorIndex != this[i].ColorIndex)
				{
					return OfficeKnownColors.Black;
				}
			}
			return colorIndex;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].ColorIndex = value;
			}
		}
	}

	public Color Color
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return ColorExtension.Empty;
			}
			Color color = this[0].Color;
			for (int i = 1; i < count; i++)
			{
				if (color != this[i].Color)
				{
					return ColorExtension.Empty;
				}
			}
			return color;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].Color = value;
			}
		}
	}

	public bool IsModified
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return false;
			}
			bool isModified = this[0].IsModified;
			for (int i = 1; i < count; i++)
			{
				if (isModified != this[i].IsModified)
				{
					return false;
				}
			}
			return isModified;
		}
	}

	public bool HasBorder
	{
		get
		{
			throw new ArgumentException("No need to implement");
		}
	}

	public int XFormatIndex
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return int.MinValue;
			}
			int xFormatIndex = ((IXFIndex)this[0]).XFormatIndex;
			for (int i = 1; i < count; i++)
			{
				if (xFormatIndex != ((IXFIndex)this[i]).XFormatIndex)
				{
					return int.MinValue;
				}
			}
			return xFormatIndex;
		}
	}

	public StyleGroup(IApplication application, object parent)
		: base(application, parent)
	{
		FindParents();
	}

	private void FindParents()
	{
		m_rangeGroup = FindParent(typeof(RangeGroup)) as RangeGroup;
		if (m_rangeGroup == null)
		{
			throw new ArgumentOutOfRangeException("parent", "Can't find parent range group.");
		}
	}

	public virtual void BeginUpdate()
	{
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			this[i].BeginUpdate();
		}
	}

	public virtual void EndUpdate()
	{
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			this[i].EndUpdate();
		}
	}
}
