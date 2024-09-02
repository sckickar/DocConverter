using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Interfaces;

namespace DocGen.OfficeChart.Implementation;

internal class StyleArrayWrapper : CommonObject, IExtendedFormat, IParentApplication, IStyle, IOptimizedUpdate, IXFIndex
{
	private List<IRange> m_arrRanges = new List<IRange>();

	private WorkbookImpl m_book;

	private IApplication m_application;

	public bool JustifyLast
	{
		get
		{
			throw new NotImplementedException("Not implemented property.");
		}
		set
		{
			throw new NotImplementedException("Not implemented property.");
		}
	}

	public string NumberFormatLocal
	{
		get
		{
			throw new NotImplementedException("Not implemented property.");
		}
		set
		{
			throw new NotImplementedException("Not implemented property.");
		}
	}

	public int XFormatIndex
	{
		get
		{
			int count = m_arrRanges.Count;
			if (count > 0)
			{
				IXFIndex iXFIndex = (IXFIndex)m_arrRanges[0].CellStyle;
				int xFormatIndex = iXFIndex.XFormatIndex;
				for (int i = 1; i < count; i++)
				{
					iXFIndex = (IXFIndex)m_arrRanges[i].CellStyle;
					if (xFormatIndex != iXFIndex.XFormatIndex)
					{
						return int.MinValue;
					}
				}
				return xFormatIndex;
			}
			return int.MinValue;
		}
	}

	public bool HasBorder
	{
		get
		{
			for (int i = 0; i < m_arrRanges.Count; i++)
			{
				if (m_arrRanges[i].CellStyle.HasBorder)
				{
					return true;
				}
			}
			return false;
		}
	}

	public IBorders Borders
	{
		get
		{
			RangeImpl rangeImpl = ((IRange)base.Parent) as RangeImpl;
			if (rangeImpl.IsEntireRow || rangeImpl.IsEntireColumn)
			{
				return new BordersCollectionArrayWrapper(m_arrRanges, m_application);
			}
			return new BordersCollectionArrayWrapper((IRange)base.Parent);
		}
	}

	public bool BuiltIn
	{
		get
		{
			bool flag = false;
			bool flag2 = true;
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				IRange range = m_arrRanges[i];
				if (flag2)
				{
					flag = range.CellStyle.BuiltIn;
					flag2 = false;
				}
				else if (range.CellStyle.BuiltIn != flag)
				{
					return false;
				}
			}
			return flag;
		}
	}

	public OfficePattern FillPattern
	{
		get
		{
			OfficePattern officePattern = OfficePattern.None;
			bool flag = true;
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				IRange range = m_arrRanges[i];
				if (flag)
				{
					officePattern = range.CellStyle.FillPattern;
					flag = false;
				}
				else if (range.CellStyle.FillPattern != officePattern)
				{
					return OfficePattern.None;
				}
			}
			return officePattern;
		}
		set
		{
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				m_arrRanges[i].CellStyle.FillPattern = value;
			}
		}
	}

	public OfficeKnownColors FillBackground
	{
		get
		{
			OfficeKnownColors officeKnownColors = OfficeKnownColors.Black;
			bool flag = true;
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				IRange range = m_arrRanges[i];
				if (flag)
				{
					officeKnownColors = range.CellStyle.FillBackground;
					flag = false;
				}
				else if (range.CellStyle.FillBackground != officeKnownColors)
				{
					return OfficeKnownColors.Black;
				}
			}
			return officeKnownColors;
		}
		set
		{
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				m_arrRanges[i].CellStyle.FillBackground = value;
			}
		}
	}

	public Color FillBackgroundRGB
	{
		get
		{
			return m_book.GetPaletteColor(FillBackground);
		}
		set
		{
			FillBackground = m_book.GetNearestColor(value);
		}
	}

	public OfficeKnownColors FillForeground
	{
		get
		{
			OfficeKnownColors officeKnownColors = OfficeKnownColors.Black;
			bool flag = true;
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				IRange range = m_arrRanges[i];
				if (flag)
				{
					officeKnownColors = range.CellStyle.FillForeground;
					flag = false;
				}
				else if (range.CellStyle.FillForeground != officeKnownColors)
				{
					return OfficeKnownColors.Black;
				}
			}
			return officeKnownColors;
		}
		set
		{
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				m_arrRanges[i].CellStyle.FillForeground = value;
			}
		}
	}

	public Color FillForegroundRGB
	{
		get
		{
			return m_book.GetPaletteColor(FillForeground);
		}
		set
		{
			FillForeground = m_book.GetNearestColor(value);
		}
	}

	public IOfficeFont Font
	{
		get
		{
			IOfficeFont officeFont = null;
			bool flag = true;
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				IRange range = m_arrRanges[i];
				if (flag)
				{
					officeFont = range.CellStyle.Font;
					flag = false;
				}
				else if (range.CellStyle.Font != officeFont)
				{
					if ((IRange)base.Parent is RangeImpl rangeImpl && (rangeImpl.IsEntireRow || rangeImpl.IsEntireColumn))
					{
						return new FontArrayWrapper(m_arrRanges, m_application);
					}
					return new FontArrayWrapper((IRange)base.Parent);
				}
			}
			return officeFont;
		}
	}

	public bool FormulaHidden
	{
		get
		{
			bool flag = false;
			bool flag2 = true;
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				IRange range = m_arrRanges[i];
				if (flag2)
				{
					flag = range.CellStyle.FormulaHidden;
					flag2 = false;
				}
				else if (range.CellStyle.FormulaHidden != flag)
				{
					return false;
				}
			}
			return flag;
		}
		set
		{
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				m_arrRanges[i].CellStyle.FormulaHidden = value;
			}
		}
	}

	public OfficeHAlign HorizontalAlignment
	{
		get
		{
			OfficeHAlign officeHAlign = OfficeHAlign.HAlignGeneral;
			bool flag = true;
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				IRange range = m_arrRanges[i];
				if (flag)
				{
					officeHAlign = range.CellStyle.HorizontalAlignment;
					flag = false;
				}
				else if (range.CellStyle.HorizontalAlignment != officeHAlign)
				{
					return OfficeHAlign.HAlignGeneral;
				}
			}
			return officeHAlign;
		}
		set
		{
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				m_arrRanges[i].CellStyle.HorizontalAlignment = value;
			}
		}
	}

	public bool IncludeAlignment
	{
		get
		{
			bool flag = false;
			bool flag2 = true;
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				IRange range = m_arrRanges[i];
				if (flag2)
				{
					flag = range.CellStyle.IncludeAlignment;
					flag2 = false;
				}
				else if (range.CellStyle.IncludeAlignment != flag)
				{
					return false;
				}
			}
			return flag;
		}
		set
		{
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				m_arrRanges[i].CellStyle.IncludeAlignment = value;
			}
		}
	}

	public bool IncludeBorder
	{
		get
		{
			bool flag = false;
			bool flag2 = true;
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				IRange range = m_arrRanges[i];
				if (flag2)
				{
					flag = range.CellStyle.IncludeBorder;
					flag2 = false;
				}
				else if (range.CellStyle.IncludeBorder != flag)
				{
					return false;
				}
			}
			return flag;
		}
		set
		{
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				m_arrRanges[i].CellStyle.IncludeBorder = value;
			}
		}
	}

	public bool IncludeFont
	{
		get
		{
			bool flag = false;
			bool flag2 = true;
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				IRange range = m_arrRanges[i];
				if (flag2)
				{
					flag = range.CellStyle.IncludeFont;
					flag2 = false;
				}
				else if (range.CellStyle.IncludeFont != flag)
				{
					return false;
				}
			}
			return flag;
		}
		set
		{
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				m_arrRanges[i].CellStyle.IncludeFont = value;
			}
		}
	}

	public bool IncludeNumberFormat
	{
		get
		{
			bool flag = false;
			bool flag2 = true;
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				IRange range = m_arrRanges[i];
				if (flag2)
				{
					flag = range.CellStyle.IncludeNumberFormat;
					flag2 = false;
				}
				else if (range.CellStyle.IncludeNumberFormat != flag)
				{
					return false;
				}
			}
			return flag;
		}
		set
		{
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				m_arrRanges[i].CellStyle.IncludeNumberFormat = value;
			}
		}
	}

	public bool IncludePatterns
	{
		get
		{
			bool flag = false;
			bool flag2 = true;
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				IRange range = m_arrRanges[i];
				if (flag2)
				{
					flag = range.CellStyle.IncludePatterns;
					flag2 = false;
				}
				else if (range.CellStyle.IncludePatterns != flag)
				{
					return false;
				}
			}
			return flag;
		}
		set
		{
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				m_arrRanges[i].CellStyle.IncludePatterns = value;
			}
		}
	}

	public bool IncludeProtection
	{
		get
		{
			bool flag = false;
			bool flag2 = true;
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				IRange range = m_arrRanges[i];
				if (flag2)
				{
					flag = range.CellStyle.IncludeProtection;
					flag2 = false;
				}
				else if (range.CellStyle.IncludeProtection != flag)
				{
					return false;
				}
			}
			return flag;
		}
		set
		{
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				m_arrRanges[i].CellStyle.IncludeProtection = value;
			}
		}
	}

	public int IndentLevel
	{
		get
		{
			int num = 0;
			bool flag = true;
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				IRange range = m_arrRanges[i];
				if (flag)
				{
					num = range.CellStyle.IndentLevel;
					flag = false;
				}
				else if (range.CellStyle.IndentLevel != num)
				{
					return 0;
				}
			}
			return num;
		}
		set
		{
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				m_arrRanges[i].CellStyle.IndentLevel = value;
			}
		}
	}

	public bool IsInitialized
	{
		get
		{
			bool hasStyle = ((RangeImpl)m_arrRanges[0]).HasStyle;
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				if (m_arrRanges[i].HasStyle != hasStyle)
				{
					return false;
				}
			}
			return hasStyle;
		}
	}

	public bool Locked
	{
		get
		{
			bool flag = false;
			bool flag2 = true;
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				IRange range = m_arrRanges[i];
				if (flag2)
				{
					flag = range.CellStyle.Locked;
					flag2 = false;
				}
				else if (range.CellStyle.Locked != flag)
				{
					return false;
				}
			}
			return flag;
		}
		set
		{
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				m_arrRanges[i].CellStyle.Locked = value;
			}
		}
	}

	public string Name
	{
		get
		{
			string text = null;
			bool flag = true;
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				IRange range = m_arrRanges[i];
				if (flag)
				{
					text = range.CellStyle.Name;
					flag = false;
				}
				else if (range.CellStyle.Name != text)
				{
					return null;
				}
			}
			return text;
		}
	}

	public string NumberFormat
	{
		get
		{
			string text = null;
			bool flag = true;
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				IRange range = m_arrRanges[i];
				if (flag)
				{
					text = range.CellStyle.NumberFormat;
					flag = false;
				}
				else if (range.CellStyle.NumberFormat != text)
				{
					return null;
				}
			}
			return text;
		}
		set
		{
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				m_arrRanges[i].CellStyle.NumberFormat = value;
			}
		}
	}

	public int NumberFormatIndex
	{
		get
		{
			int num = int.MinValue;
			bool flag = true;
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				IRange range = m_arrRanges[i];
				if (flag)
				{
					num = range.CellStyle.NumberFormatIndex;
					flag = false;
				}
				else if (range.CellStyle.NumberFormatIndex != num)
				{
					return int.MinValue;
				}
			}
			return num;
		}
		set
		{
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				m_arrRanges[i].CellStyle.NumberFormatIndex = value;
			}
		}
	}

	public INumberFormat NumberFormatSettings
	{
		get
		{
			if (NumberFormatIndex <= 0)
			{
				return null;
			}
			return m_arrRanges[0].CellStyle.NumberFormatSettings;
		}
	}

	public int Rotation
	{
		get
		{
			if (m_arrRanges.Count == 0)
			{
				return 0;
			}
			int rotation = m_arrRanges[0].CellStyle.Rotation;
			int i = 1;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				if (m_arrRanges[i].CellStyle.Rotation != rotation)
				{
					return int.MinValue;
				}
			}
			return rotation;
		}
		set
		{
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				m_arrRanges[i].CellStyle.Rotation = value;
			}
		}
	}

	public bool ShrinkToFit
	{
		get
		{
			bool flag = false;
			bool flag2 = true;
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				IRange range = m_arrRanges[i];
				if (flag2)
				{
					flag = range.CellStyle.ShrinkToFit;
					flag2 = false;
				}
				else if (range.CellStyle.ShrinkToFit != flag)
				{
					return false;
				}
			}
			return flag;
		}
		set
		{
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				m_arrRanges[i].CellStyle.ShrinkToFit = value;
			}
		}
	}

	public OfficeVAlign VerticalAlignment
	{
		get
		{
			OfficeVAlign officeVAlign = OfficeVAlign.VAlignBottom;
			bool flag = true;
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				IRange range = m_arrRanges[i];
				if (flag)
				{
					officeVAlign = range.CellStyle.VerticalAlignment;
					flag = false;
				}
				else if (range.CellStyle.VerticalAlignment != officeVAlign)
				{
					return OfficeVAlign.VAlignBottom;
				}
			}
			return officeVAlign;
		}
		set
		{
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				m_arrRanges[i].CellStyle.VerticalAlignment = value;
			}
		}
	}

	public bool WrapText
	{
		get
		{
			bool flag = false;
			bool flag2 = true;
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				IRange range = m_arrRanges[i];
				if (flag2)
				{
					flag = range.CellStyle.WrapText;
					flag2 = false;
				}
				else if (range.CellStyle.WrapText != flag)
				{
					return false;
				}
			}
			return flag;
		}
		set
		{
			List<int> list = new List<int>();
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				IRange range = m_arrRanges[i];
				range.CellStyle.WrapText = value;
				int row = range.Row;
				IWorksheet worksheet = range.Worksheet;
				if (!list.Contains(row))
				{
					if (!WorksheetHelper.GetOrCreateRow(worksheet as IInternalWorksheet, row - 1, bCreate: false).IsBadFontHeight && !(worksheet.Workbook as WorkbookImpl).IsWorkbookOpening)
					{
						worksheet.AutofitRow(row);
					}
					list.Add(row);
				}
			}
		}
	}

	public OfficeReadingOrderType ReadingOrder
	{
		get
		{
			if (m_arrRanges == null)
			{
				throw new ApplicationException("Blank collection");
			}
			List<IRange> arrRanges = m_arrRanges;
			OfficeReadingOrderType readingOrder = arrRanges[0].CellStyle.ReadingOrder;
			if (readingOrder == OfficeReadingOrderType.Context)
			{
				return OfficeReadingOrderType.Context;
			}
			int i = 1;
			for (int count = arrRanges.Count; i < count; i++)
			{
				IRange range = arrRanges[i];
				if (readingOrder != range.CellStyle.ReadingOrder)
				{
					return OfficeReadingOrderType.Context;
				}
			}
			return readingOrder;
		}
		set
		{
			if (m_arrRanges == null)
			{
				throw new ApplicationException("Blank collection");
			}
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				m_arrRanges[i].CellStyle.ReadingOrder = value;
			}
		}
	}

	public bool IsFirstSymbolApostrophe
	{
		get
		{
			bool flag = true;
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				if (flag)
				{
					break;
				}
				flag = m_arrRanges[i].CellStyle.IsFirstSymbolApostrophe;
			}
			return flag;
		}
		set
		{
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				m_arrRanges[i].CellStyle.IsFirstSymbolApostrophe = value;
			}
		}
	}

	public OfficeKnownColors PatternColorIndex
	{
		get
		{
			OfficeKnownColors officeKnownColors = OfficeKnownColors.Black;
			bool flag = true;
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				IRange range = m_arrRanges[i];
				if (flag)
				{
					officeKnownColors = range.CellStyle.PatternColorIndex;
					flag = false;
				}
				else if (range.CellStyle.PatternColorIndex != officeKnownColors)
				{
					return OfficeKnownColors.Black;
				}
			}
			return officeKnownColors;
		}
		set
		{
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				m_arrRanges[i].CellStyle.PatternColorIndex = value;
			}
		}
	}

	public Color PatternColor
	{
		get
		{
			Color result = ColorExtension.Empty;
			bool flag = true;
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				IRange range = m_arrRanges[i];
				if (flag)
				{
					result = range.CellStyle.PatternColor;
					flag = false;
				}
				else if (range.CellStyle.PatternColor.ToArgb() != result.ToArgb())
				{
					return ColorExtension.Empty;
				}
			}
			return result;
		}
		set
		{
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				m_arrRanges[i].CellStyle.PatternColor = value;
			}
		}
	}

	public OfficeKnownColors ColorIndex
	{
		get
		{
			OfficeKnownColors officeKnownColors = OfficeKnownColors.Black;
			bool flag = true;
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				IRange range = m_arrRanges[i];
				if (flag)
				{
					officeKnownColors = range.CellStyle.ColorIndex;
					flag = false;
				}
				else if (range.CellStyle.ColorIndex != officeKnownColors)
				{
					return OfficeKnownColors.Black;
				}
			}
			return officeKnownColors;
		}
		set
		{
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				m_arrRanges[i].CellStyle.ColorIndex = value;
			}
		}
	}

	public Color Color
	{
		get
		{
			Color result = ColorExtension.Empty;
			bool flag = true;
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				IRange range = m_arrRanges[i];
				if (flag)
				{
					result = range.CellStyle.Color;
					flag = false;
				}
				else if (range.CellStyle.Color.ToArgb() != result.ToArgb())
				{
					return ColorExtension.Empty;
				}
			}
			return result;
		}
		set
		{
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				m_arrRanges[i].CellStyle.Color = value;
			}
		}
	}

	public IInterior Interior
	{
		get
		{
			IInterior interior = null;
			bool flag = true;
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				IRange range = m_arrRanges[i];
				if (flag)
				{
					interior = range.CellStyle.Interior;
					flag = false;
				}
				else if (range.CellStyle.Interior != interior)
				{
					return new InteriorArrayWrapper((IRange)base.Parent);
				}
			}
			return interior;
		}
	}

	public bool IsModified
	{
		get
		{
			bool flag = true;
			int i = 0;
			for (int count = m_arrRanges.Count; i < count; i++)
			{
				if (flag)
				{
					break;
				}
				flag = m_arrRanges[i].CellStyle.IsModified;
			}
			return flag;
		}
	}

	public StyleArrayWrapper(IRange range)
		: base((range as RangeImpl).Application, range)
	{
		m_arrRanges.AddRange(range.Cells);
		IWorksheet worksheet = range.Worksheet;
		m_book = worksheet.Workbook as WorkbookImpl;
		m_application = (range as RangeImpl).Application;
	}

	public StyleArrayWrapper(IApplication application, List<IRange> LstRange, IWorksheet worksheet)
		: base(application, LstRange[0])
	{
		m_arrRanges = LstRange;
		m_book = worksheet.Workbook as WorkbookImpl;
		m_application = application;
	}

	public virtual void BeginUpdate()
	{
		int i = 0;
		for (int count = m_arrRanges.Count; i < count; i++)
		{
			m_arrRanges[i].CellStyle.BeginUpdate();
		}
	}

	public virtual void EndUpdate()
	{
		int i = 0;
		for (int count = m_arrRanges.Count; i < count; i++)
		{
			m_arrRanges[i].CellStyle.EndUpdate();
		}
	}
}
