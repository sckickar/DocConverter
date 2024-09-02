using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation;

internal class ExtendedFormatImpl : CommonObject, IInternalExtendedFormat, IExtendedFormat, IParentApplication, IComparable, ICloneable, IXFIndex, ICloneParent, IDisposable
{
	internal ushort FONTBOLD = 700;

	internal ushort FONTNORMAL = 400;

	public const int DEF_NO_PARENT_INDEX = 4095;

	public const int TopToBottomRotation = 255;

	public const int MaxTintValue = 32767;

	private ExtendedFormatRecord m_extFormat;

	private ExtendedXFRecord m_xfExt;

	private WorkbookImpl m_book;

	private int m_iXFIndex;

	private ShapeFillImpl m_gradient;

	private ChartColor m_color;

	private ChartColor m_patternColor;

	private ChartColor m_topBorderColor;

	private ChartColor m_bottomBorderColor;

	private ChartColor m_leftBorderColor;

	private ChartColor m_rightBorderColor;

	private ChartColor m_diagonalBorderColor;

	private bool m_hasBorder;

	private bool m_pivotButton;

	public int FontIndex
	{
		get
		{
			if (!IncludeFont)
			{
				return ParentRecord.FontIndex;
			}
			return m_extFormat.FontIndex;
		}
		set
		{
			if (FontIndex != value)
			{
				IncludeFont = true;
				m_extFormat.FontIndex = (ushort)value;
				SetChanged();
			}
		}
	}

	public int XFormatIndex => Index;

	public int NumberFormatIndex
	{
		get
		{
			if (!IncludeNumberFormat)
			{
				return ParentRecord.FormatIndex;
			}
			return m_extFormat.FormatIndex;
		}
		set
		{
			if (!m_book.InnerFormats.Contains(value))
			{
				throw new ArgumentOutOfRangeException("Unknown format index");
			}
			if (NumberFormatIndex != value)
			{
				IncludeNumberFormat = true;
				m_extFormat.FormatIndex = (ushort)value;
			}
			SetChanged();
		}
	}

	public OfficePattern FillPattern
	{
		get
		{
			if (!IncludePatterns)
			{
				return (OfficePattern)ParentRecord.AdtlFillPattern;
			}
			return (OfficePattern)m_extFormat.AdtlFillPattern;
		}
		set
		{
			if (FillPattern != value)
			{
				IncludePatterns = true;
				if (value == OfficePattern.None)
				{
					ColorIndex = (OfficeKnownColors)65;
					PatternColorIndex = OfficeKnownColors.BlackCustom;
				}
				m_extFormat.AdtlFillPattern = (ushort)value;
				SetChanged();
			}
		}
	}

	public OfficeKnownColors FillBackground
	{
		get
		{
			return ColorIndex;
		}
		set
		{
			ColorIndex = value;
		}
	}

	public Color FillBackgroundRGB
	{
		get
		{
			return Color;
		}
		set
		{
			Color = value;
		}
	}

	public OfficeKnownColors FillForeground
	{
		get
		{
			return PatternColorIndex;
		}
		set
		{
			PatternColorIndex = value;
		}
	}

	public Color FillForegroundRGB
	{
		get
		{
			return PatternColor;
		}
		set
		{
			PatternColor = value;
		}
	}

	public OfficeHAlign HorizontalAlignment
	{
		get
		{
			return (IncludeAlignment ? m_extFormat : ParentRecord).HAlignmentType;
		}
		set
		{
			if (HorizontalAlignment != value)
			{
				IncludeAlignment = true;
				m_extFormat.HAlignmentType = value;
				SetChanged();
			}
		}
	}

	public int IndentLevel
	{
		get
		{
			return m_extFormat.Indent;
		}
		set
		{
			if (IndentLevel != value)
			{
				if (value > m_book.MaxIndent)
				{
					throw new ArgumentOutOfRangeException("IndentLevel");
				}
				IncludeAlignment = true;
				m_extFormat.Indent = (byte)value;
				if (HorizontalAlignment == OfficeHAlign.HAlignGeneral)
				{
					HorizontalAlignment = OfficeHAlign.HAlignLeft;
				}
				if (value != 0)
				{
					m_extFormat.Rotation = 0;
				}
				SetChanged();
			}
		}
	}

	public bool FormulaHidden
	{
		get
		{
			return m_extFormat.IsHidden;
		}
		set
		{
			m_extFormat.IsHidden = value;
			SetChanged();
		}
	}

	public bool Locked
	{
		get
		{
			return m_extFormat.IsLocked;
		}
		set
		{
			if (Locked != value)
			{
				IncludeProtection = true;
				m_extFormat.IsLocked = value;
				SetChanged();
			}
		}
	}

	public bool JustifyLast
	{
		get
		{
			return m_extFormat.JustifyLast;
		}
		set
		{
			m_extFormat.JustifyLast = value;
			SetChanged();
		}
	}

	public string NumberFormat
	{
		get
		{
			if (m_book.InnerFormats.Count > 14 && !m_book.InnerFormats.Contains(NumberFormatIndex))
			{
				NumberFormatIndex = 14;
			}
			return m_book.InnerFormats[NumberFormatIndex].FormatString;
		}
		set
		{
			NumberFormatIndex = (ushort)m_book.InnerFormats.FindOrCreateFormat(value);
			SetChanged();
		}
	}

	public string NumberFormatLocal
	{
		get
		{
			return NumberFormat;
		}
		set
		{
			NumberFormat = value;
		}
	}

	public INumberFormat NumberFormatSettings => m_book.InnerFormats[NumberFormatIndex];

	public bool ShrinkToFit
	{
		get
		{
			return m_extFormat.ShrinkToFit;
		}
		set
		{
			if (value != ShrinkToFit)
			{
				IncludeAlignment = true;
				m_extFormat.ShrinkToFit = value;
				SetChanged();
			}
		}
	}

	public bool WrapText
	{
		get
		{
			return m_extFormat.WrapText;
		}
		set
		{
			if (WrapText != value)
			{
				IncludeAlignment = true;
				m_extFormat.WrapText = value;
				SetChanged();
			}
		}
	}

	public OfficeVAlign VerticalAlignment
	{
		get
		{
			return (IncludeAlignment ? m_extFormat : ParentRecord).VAlignmentType;
		}
		set
		{
			if (VerticalAlignment != value)
			{
				IncludeAlignment = true;
				m_extFormat.VAlignmentType = value;
				SetChanged();
			}
		}
	}

	public bool IncludeAlignment
	{
		get
		{
			bool isNotParentAlignment = m_extFormat.IsNotParentAlignment;
			if (!HasParent)
			{
				return !isNotParentAlignment;
			}
			return isNotParentAlignment;
		}
		set
		{
			if (HasParent)
			{
				if (IncludeAlignment != value)
				{
					if (value && !m_book.IsWorkbookOpening)
					{
						ExtendedFormatRecord parentRecord = ParentRecord;
						m_extFormat.CopyAlignment(parentRecord);
					}
					m_extFormat.IsNotParentAlignment = value;
					SetChanged();
				}
			}
			else
			{
				m_extFormat.IsNotParentAlignment = !value;
				SetChanged();
			}
		}
	}

	public bool IncludeBorder
	{
		get
		{
			bool isNotParentBorder = m_extFormat.IsNotParentBorder;
			if (!HasParent)
			{
				return !isNotParentBorder;
			}
			return isNotParentBorder;
		}
		set
		{
			if (HasParent)
			{
				if (IncludeBorder != value)
				{
					if (value && !m_book.IsWorkbookOpening)
					{
						ExtendedFormatImpl parentFormat = ParentFormat;
						CopyBorders(parentFormat);
					}
					m_extFormat.IsNotParentBorder = value;
					SetChanged();
				}
			}
			else
			{
				m_extFormat.IsNotParentBorder = !value;
				SetChanged();
			}
		}
	}

	public bool IncludeFont
	{
		get
		{
			bool isNotParentFont = m_extFormat.IsNotParentFont;
			if (!HasParent)
			{
				return !isNotParentFont;
			}
			return isNotParentFont;
		}
		set
		{
			if (HasParent)
			{
				if (IncludeFont != value)
				{
					if (value && !m_book.IsWorkbookOpening)
					{
						m_extFormat.FontIndex = ParentRecord.FontIndex;
					}
					m_extFormat.IsNotParentFont = value;
					SetChanged();
				}
			}
			else
			{
				m_extFormat.IsNotParentFont = !value;
				SetChanged();
			}
		}
	}

	public bool IncludeNumberFormat
	{
		get
		{
			bool isNotParentFormat = m_extFormat.IsNotParentFormat;
			if (!HasParent)
			{
				return !isNotParentFormat;
			}
			return isNotParentFormat;
		}
		set
		{
			if (HasParent)
			{
				if (IncludeNumberFormat != value)
				{
					if (value && !m_book.IsWorkbookOpening)
					{
						m_extFormat.FormatIndex = ParentRecord.FormatIndex;
					}
					m_extFormat.IsNotParentFormat = value;
					SetChanged();
				}
			}
			else
			{
				m_extFormat.IsNotParentFormat = !value;
				SetChanged();
			}
		}
	}

	public bool IncludePatterns
	{
		get
		{
			bool isNotParentPattern = m_extFormat.IsNotParentPattern;
			if (!HasParent)
			{
				return !isNotParentPattern;
			}
			return isNotParentPattern;
		}
		set
		{
			if (HasParent)
			{
				if (IncludePatterns != value)
				{
					if (value && !m_book.IsWorkbookOpening)
					{
						ExtendedFormatImpl parentFormat = ParentFormat;
						CopyPatterns(parentFormat);
					}
					m_extFormat.IsNotParentPattern = value;
					SetChanged();
				}
			}
			else
			{
				m_extFormat.IsNotParentPattern = !value;
				SetChanged();
			}
		}
	}

	public bool IncludeProtection
	{
		get
		{
			bool isNotParentCellOptions = m_extFormat.IsNotParentCellOptions;
			if (!HasParent)
			{
				return !isNotParentCellOptions;
			}
			return isNotParentCellOptions;
		}
		set
		{
			if (HasParent)
			{
				if (IncludeProtection != value)
				{
					if (value && !m_book.IsWorkbookOpening)
					{
						ExtendedFormatRecord parentRecord = ParentRecord;
						m_extFormat.CopyProtection(parentRecord);
					}
					m_extFormat.IsNotParentCellOptions = value;
					SetChanged();
				}
			}
			else
			{
				m_extFormat.IsNotParentCellOptions = !value;
				SetChanged();
			}
		}
	}

	public virtual IOfficeFont Font => m_book.InnerFonts[FontIndex];

	public IBorders Borders => new BordersCollection(base.Application, m_book, this);

	public bool IsFirstSymbolApostrophe
	{
		get
		{
			return m_extFormat._123Prefix;
		}
		set
		{
			m_extFormat._123Prefix = value;
			SetChanged();
		}
	}

	public OfficeKnownColors PatternColorIndex
	{
		get
		{
			return PatternColorObject.GetIndexed(m_book);
		}
		set
		{
			if (value != PatternColorIndex)
			{
				IncludePatterns = true;
				PatternColorObject.SetIndexed(value);
				m_extFormat.AdtlFillPattern |= 1;
				SetChanged();
			}
		}
	}

	public Color PatternColor
	{
		get
		{
			return PatternColorObject.GetRGB(m_book);
		}
		set
		{
			IncludePatterns = true;
			PatternColorObject.SetRGB(value, m_book);
			if (m_extFormat.AdtlFillPattern == 0)
			{
				m_extFormat.AdtlFillPattern |= 1;
			}
			SetChanged();
		}
	}

	public ChartColor PatternColorObject => (IncludePatterns ? this : ParentFormat).InnerPatternColor;

	public OfficeKnownColors ColorIndex
	{
		get
		{
			return ColorObject.GetIndexed(m_book);
		}
		set
		{
			if (value != ColorIndex)
			{
				IncludePatterns = true;
				ColorObject.SetIndexed(value, raiseEvent: true, m_book);
				if (m_extFormat.AdtlFillPattern == 0)
				{
					m_extFormat.AdtlFillPattern = 1;
				}
				SetChanged();
			}
		}
	}

	public Color Color
	{
		get
		{
			return ColorObject.GetRGB(m_book);
		}
		set
		{
			IncludePatterns = true;
			ColorObject.SetRGB(value, m_book);
			if (m_extFormat.AdtlFillPattern == 0)
			{
				m_extFormat.AdtlFillPattern = 1;
			}
			SetChanged();
		}
	}

	public ChartColor ColorObject => (IncludePatterns ? this : ParentFormat).InnerColor;

	public bool IsModified
	{
		get
		{
			bool result = false;
			if (HasParent)
			{
				result = IncludeAlignment || IncludeBorder || IncludeFont || IncludeNumberFormat || IncludePatterns || IncludeProtection;
			}
			return result;
		}
	}

	public List<ExtendedProperty> Properties
	{
		get
		{
			return m_xfExt.Properties;
		}
		set
		{
			m_xfExt.Properties = value;
			m_xfExt.PropertyCount = (ushort)m_xfExt.Properties.Count;
		}
	}

	public OfficeReadingOrderType ReadingOrder
	{
		get
		{
			return (OfficeReadingOrderType)m_extFormat.ReadingOrder;
		}
		set
		{
			m_extFormat.ReadingOrder = (ushort)value;
			SetChanged();
		}
	}

	public int Rotation
	{
		get
		{
			return m_extFormat.Rotation;
		}
		set
		{
			if (value != Rotation)
			{
				IncludeAlignment = true;
				m_extFormat.Rotation = (ushort)value;
				if (value != 0)
				{
					m_extFormat.Indent = 0;
				}
				SetChanged();
			}
		}
	}

	[CLSCompliant(false)]
	public ExtendedFormatRecord.TXFType XFType
	{
		get
		{
			return m_extFormat.XFType;
		}
		set
		{
			m_extFormat.XFType = value;
			SetChanged();
		}
	}

	public IGradient Gradient
	{
		get
		{
			return m_gradient;
		}
		set
		{
			m_gradient = (ShapeFillImpl)value;
		}
	}

	internal int Index
	{
		get
		{
			return m_iXFIndex;
		}
		set
		{
			m_iXFIndex = value;
		}
	}

	[CLSCompliant(false)]
	public ExtendedFormatRecord Record
	{
		get
		{
			return m_extFormat;
		}
		protected set
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			m_extFormat = value;
		}
	}

	[CLSCompliant(false)]
	public ExtendedXFRecord XFRecord
	{
		get
		{
			return m_xfExt;
		}
		protected set
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			m_xfExt = value;
		}
	}

	internal int ParentIndex
	{
		get
		{
			return m_extFormat.ParentIndex;
		}
		set
		{
			m_extFormat.ParentIndex = (ushort)value;
		}
	}

	public WorkbookImpl Workbook => m_book;

	protected internal ExtendedFormatsCollection ParentCollection => m_book.InnerExtFormats;

	public ChartColor BottomBorderColor => (IncludeBorder ? this : ParentFormat).InnerBottomBorderColor;

	public ChartColor TopBorderColor => (IncludeBorder ? this : ParentFormat).InnerTopBorderColor;

	public ChartColor LeftBorderColor => (IncludeBorder ? this : ParentFormat).InnerLeftBorderColor;

	public ChartColor RightBorderColor => (IncludeBorder ? this : ParentFormat).InnerRightBorderColor;

	public ChartColor DiagonalBorderColor => (IncludeBorder ? this : ParentFormat).InnerDiagonalBorderColor;

	public OfficeLineStyle LeftBorderLineStyle
	{
		get
		{
			return (IncludeBorder ? m_extFormat : ParentRecord).BorderLeft;
		}
		set
		{
			if (LeftBorderLineStyle != value)
			{
				IncludeBorder = true;
				m_extFormat.SetWorkbook(m_book);
				m_extFormat.BorderLeft = value;
				if (value == OfficeLineStyle.None)
				{
					CheckAndUpdateHasBorder(m_extFormat);
					LeftBorderColor.SetIndexed(OfficeKnownColors.BlackCustom, raiseEvent: false);
				}
				else
				{
					m_hasBorder = true;
				}
				LeftBorderColor.Normalize();
				SetChanged();
			}
		}
	}

	public OfficeLineStyle RightBorderLineStyle
	{
		get
		{
			return (IncludeBorder ? m_extFormat : ParentRecord).BorderRight;
		}
		set
		{
			if (RightBorderLineStyle != value)
			{
				IncludeBorder = true;
				m_extFormat.SetWorkbook(m_book);
				m_extFormat.BorderRight = value;
				if (value == OfficeLineStyle.None)
				{
					CheckAndUpdateHasBorder(m_extFormat);
					RightBorderColor.SetIndexed(OfficeKnownColors.BlackCustom, raiseEvent: false);
				}
				else
				{
					m_hasBorder = true;
				}
				RightBorderColor.Normalize();
				SetChanged();
			}
		}
	}

	public OfficeLineStyle TopBorderLineStyle
	{
		get
		{
			return (IncludeBorder ? m_extFormat : ParentRecord).BorderTop;
		}
		set
		{
			if (TopBorderLineStyle != value)
			{
				IncludeBorder = true;
				m_extFormat.SetWorkbook(m_book);
				m_extFormat.BorderTop = value;
				if (value == OfficeLineStyle.None)
				{
					CheckAndUpdateHasBorder(m_extFormat);
					TopBorderColor.SetIndexed(OfficeKnownColors.BlackCustom, raiseEvent: false);
				}
				else
				{
					m_hasBorder = true;
				}
				TopBorderColor.Normalize();
				SetChanged();
			}
		}
	}

	public OfficeLineStyle BottomBorderLineStyle
	{
		get
		{
			return (IncludeBorder ? m_extFormat : ParentRecord).BorderBottom;
		}
		set
		{
			if (BottomBorderLineStyle != value)
			{
				IncludeBorder = true;
				m_extFormat.SetWorkbook(m_book);
				m_extFormat.BorderBottom = value;
				if (value == OfficeLineStyle.None)
				{
					CheckAndUpdateHasBorder(m_extFormat);
					BottomBorderColor.SetIndexed(OfficeKnownColors.BlackCustom, raiseEvent: false);
				}
				else
				{
					m_hasBorder = true;
				}
				BottomBorderColor.Normalize();
				SetChanged();
			}
		}
	}

	public OfficeLineStyle DiagonalUpBorderLineStyle
	{
		get
		{
			return (OfficeLineStyle)(IncludeBorder ? m_extFormat : ParentRecord).DiagonalLineStyle;
		}
		set
		{
			if (DiagonalUpBorderLineStyle != value)
			{
				IncludeBorder = true;
				m_extFormat.DiagonalLineStyle = (ushort)value;
				if (value == OfficeLineStyle.None)
				{
					CheckAndUpdateHasBorder(m_extFormat);
					DiagonalBorderColor.SetIndexed(OfficeKnownColors.BlackCustom, raiseEvent: true);
				}
				else
				{
					m_hasBorder = true;
				}
				DiagonalBorderColor.Normalize();
				SetChanged();
			}
			if (!m_extFormat.DiagonalFromBottomLeft)
			{
				IncludeBorder = true;
				m_extFormat.DiagonalFromBottomLeft = true;
				SetChanged();
			}
		}
	}

	public OfficeLineStyle DiagonalDownBorderLineStyle
	{
		get
		{
			if (!IncludeBorder)
			{
				_ = ParentRecord;
			}
			else
			{
				_ = m_extFormat;
			}
			return (OfficeLineStyle)m_extFormat.DiagonalLineStyle;
		}
		set
		{
			if (DiagonalDownBorderLineStyle != value)
			{
				IncludeBorder = true;
				m_extFormat.DiagonalLineStyle = (ushort)value;
				if (value == OfficeLineStyle.None)
				{
					CheckAndUpdateHasBorder(m_extFormat);
					DiagonalBorderColor.SetIndexed(OfficeKnownColors.BlackCustom, raiseEvent: true);
				}
				else
				{
					m_hasBorder = true;
				}
				DiagonalBorderColor.Normalize();
				SetChanged();
			}
			if (!m_extFormat.DiagonalFromTopLeft)
			{
				IncludeBorder = true;
				m_extFormat.DiagonalFromTopLeft = true;
				SetChanged();
			}
		}
	}

	public bool DiagonalUpVisible
	{
		get
		{
			return (IncludeBorder ? m_extFormat : ParentRecord).DiagonalFromBottomLeft;
		}
		set
		{
			if (DiagonalUpVisible != value)
			{
				IncludeBorder = true;
				m_extFormat.DiagonalFromBottomLeft = value;
				SetChanged();
			}
		}
	}

	public bool DiagonalDownVisible
	{
		get
		{
			return (IncludeBorder ? m_extFormat : ParentRecord).DiagonalFromTopLeft;
		}
		set
		{
			if (DiagonalDownVisible != value)
			{
				IncludeBorder = true;
				m_extFormat.DiagonalFromTopLeft = value;
				SetChanged();
			}
		}
	}

	public bool HasParent => ParentIndex != m_book.MaxXFCount;

	public bool IsDefaultColor => ColorIndex == (OfficeKnownColors)65;

	public bool IsDefaultPatternColor => PatternColorIndex == OfficeKnownColors.BlackCustom;

	private ExtendedFormatRecord ParentRecord
	{
		get
		{
			if (!HasParent)
			{
				return m_extFormat;
			}
			return ((ExtendedFormatImpl)m_book.GetExtFormat(ParentIndex)).Record;
		}
	}

	private ExtendedFormatImpl ParentFormat
	{
		get
		{
			if (!HasParent)
			{
				return this;
			}
			return (ExtendedFormatImpl)m_book.GetExtFormat(ParentIndex);
		}
	}

	public FormatImpl NumberFormatObject
	{
		get
		{
			int numberFormatIndex = NumberFormatIndex;
			return m_book.InnerFormats[numberFormatIndex];
		}
	}

	public bool HasBorder
	{
		get
		{
			return m_hasBorder;
		}
		set
		{
			m_hasBorder = value;
		}
	}

	internal bool PivotButton
	{
		get
		{
			return m_pivotButton;
		}
		set
		{
			m_pivotButton = value;
		}
	}

	protected ChartColor InnerColor => m_color;

	protected ChartColor InnerPatternColor => m_patternColor;

	protected ChartColor InnerTopBorderColor => m_topBorderColor;

	protected ChartColor InnerBottomBorderColor => m_bottomBorderColor;

	protected ChartColor InnerLeftBorderColor => m_leftBorderColor;

	protected ChartColor InnerRightBorderColor => m_rightBorderColor;

	protected ChartColor InnerDiagonalBorderColor => m_diagonalBorderColor;

	private bool CompareProperties(ExtendedFormatImpl parent)
	{
		throw new NotImplementedException();
	}

	internal void SetChanged()
	{
		m_book.Saved = false;
	}

	public void CopyTo(ExtendedFormatImpl twin)
	{
		if (twin == null)
		{
			throw new ArgumentNullException("twin");
		}
		twin.m_book = m_book;
		m_extFormat.CopyTo(twin.m_extFormat);
		m_xfExt.CopyTo(twin.m_xfExt);
	}

	public ExtendedFormatImpl CreateChildFormat()
	{
		return CreateChildFormat(bRegister: true);
	}

	public ExtendedFormatImpl CreateChildFormat(bool bRegister)
	{
		ExtendedFormatImpl extendedFormatImpl = this;
		if (extendedFormatImpl.Record.XFType == ExtendedFormatRecord.TXFType.XF_CELL)
		{
			extendedFormatImpl = m_book.CreateExtFormatWithoutRegister(extendedFormatImpl);
			extendedFormatImpl.Record.XFType = ExtendedFormatRecord.TXFType.XF_STYLE;
			extendedFormatImpl.ParentIndex = Index;
			if (bRegister)
			{
				extendedFormatImpl = m_book.RegisterExtFormat(extendedFormatImpl);
			}
		}
		return extendedFormatImpl;
	}

	public ExtendedFormatImpl CreateChildFormat(ExtendedFormatImpl oldFormat)
	{
		ExtendedFormatRecord.TXFType xFType = Record.XFType;
		ExtendedFormatImpl extendedFormatImpl = CreateChildFormat();
		if (xFType == ExtendedFormatRecord.TXFType.XF_CELL)
		{
			ExtendedFormatImpl extendedFormatImpl2 = (ExtendedFormatImpl)extendedFormatImpl.Clone();
			bool flag = false;
			if (!IncludeAlignment)
			{
				CopyAlignment(oldFormat, extendedFormatImpl2, bSetFlag: false);
				extendedFormatImpl2.IncludeAlignment = true;
				flag = true;
			}
			if (!IncludeBorder)
			{
				CopyBorders(oldFormat, extendedFormatImpl2, bSetFlag: false);
				extendedFormatImpl2.IncludeBorder = true;
				flag = true;
			}
			if (!IncludeFont)
			{
				CopyFont(oldFormat, extendedFormatImpl2, bSetFlag: false);
				extendedFormatImpl2.IncludeFont = true;
				flag = true;
			}
			if (!IncludeNumberFormat)
			{
				CopyFormat(oldFormat, extendedFormatImpl2, bSetFlag: false);
				extendedFormatImpl2.IncludeNumberFormat = true;
				flag = true;
			}
			if (!IncludePatterns)
			{
				CopyPatterns(oldFormat, extendedFormatImpl2, bSetFlag: false);
				extendedFormatImpl2.IncludePatterns = true;
				flag = true;
			}
			if (!IncludeProtection)
			{
				CopyProtection(oldFormat, extendedFormatImpl2, bSetFlag: false);
				extendedFormatImpl2.IncludeProtection = true;
				flag = true;
			}
			if (flag)
			{
				extendedFormatImpl = m_book.InnerExtFormats.Add(extendedFormatImpl2);
			}
		}
		return extendedFormatImpl;
	}

	public void SynchronizeWithParent()
	{
		ExtendedFormatRecord parentRecord = ParentRecord;
		if (!IncludeAlignment)
		{
			m_extFormat.CopyAlignment(parentRecord);
		}
		if (!IncludeBorder)
		{
			CopyBorders(ParentFormat);
		}
		if (!IncludeFont)
		{
			m_extFormat.FontIndex = parentRecord.FontIndex;
		}
		if (!IncludeNumberFormat)
		{
			m_extFormat.FormatIndex = parentRecord.FormatIndex;
		}
		if (!IncludePatterns)
		{
			CopyPatterns(ParentFormat);
		}
		if (!IncludeProtection)
		{
			m_extFormat.CopyProtection(parentRecord);
		}
	}

	private void CopyBorders(ExtendedFormatImpl source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		m_extFormat.CopyBorders(source.m_extFormat);
		m_topBorderColor.CopyFrom(source.m_topBorderColor, callEvent: false);
		m_bottomBorderColor.CopyFrom(source.m_bottomBorderColor, callEvent: false);
		m_leftBorderColor.CopyFrom(source.m_leftBorderColor, callEvent: false);
		m_rightBorderColor.CopyFrom(source.m_rightBorderColor, callEvent: false);
		m_diagonalBorderColor.CopyFrom(source.m_diagonalBorderColor, callEvent: false);
	}

	private void CopyPatterns(ExtendedFormatImpl source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		m_extFormat.CopyPatterns(m_extFormat);
		if (!(m_color == null) || !m_book.IsWorkbookOpening)
		{
			m_color.CopyFrom(source.m_color, callEvent: false);
			m_patternColor.CopyFrom(source.m_patternColor, callEvent: false);
		}
	}

	public void BeginUpdate()
	{
	}

	public void EndUpdate()
	{
	}

	public ExtendedFormatImpl(IApplication application, object parent)
		: base(application, parent)
	{
		FindParents();
		m_extFormat = (ExtendedFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ExtendedFormat);
		m_xfExt = (ExtendedXFRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ExtendedXFRecord);
		Parse(m_extFormat, m_xfExt);
	}

	private ExtendedFormatImpl(IApplication application, object parent, BiffReader reader)
		: this(application, parent)
	{
		Parse(reader);
	}

	[CLSCompliant(false)]
	public ExtendedFormatImpl(IApplication application, object parent, BiffRecordRaw[] data, int position)
		: this(application, parent)
	{
		Parse(data, position);
	}

	public ExtendedFormatImpl(IApplication application, object parent, List<BiffRecordRaw> data, int position)
		: this(application, parent)
	{
		Parse(data, position);
	}

	[CLSCompliant(false)]
	public ExtendedFormatImpl(IApplication application, object parent, ExtendedFormatRecord format, ExtendedXFRecord xfExt)
		: this(application, parent, format, xfExt, bInitializeColors: true)
	{
	}

	[CLSCompliant(false)]
	public ExtendedFormatImpl(IApplication application, object parent, ExtendedFormatRecord format, ExtendedXFRecord xfext, bool bInitializeColors)
		: base(application, parent)
	{
		FindParents();
		Parse(format, xfext, bInitializeColors);
	}

	private void FindParents()
	{
		m_book = FindParent(typeof(WorkbookImpl)) as WorkbookImpl;
		if (m_book == null)
		{
			throw new ArgumentNullException("Can't find parent workbook.");
		}
	}

	[CLSCompliant(false)]
	protected void Parse(BiffReader reader)
	{
	}

	[CLSCompliant(false)]
	protected void Parse(IList<BiffRecordRaw> data, int position)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (position < 0 || position > data.Count - 1)
		{
			throw new ArgumentOutOfRangeException("position", "Value cannot be less than 0 and greater than data.Length - 1.");
		}
		Parse((ExtendedFormatRecord)data[position], (ExtendedXFRecord)data[position]);
	}

	[CLSCompliant(false)]
	protected void Parse(ExtendedFormatRecord format, ExtendedXFRecord xfExt)
	{
		Parse(format, xfExt, isInitializeColors: true);
	}

	[CLSCompliant(false)]
	protected void Parse(ExtendedFormatRecord format, ExtendedXFRecord xfExt, bool isInitializeColors)
	{
		m_extFormat = format;
		m_xfExt = xfExt;
		if (m_extFormat.FontIndex > m_book.InnerFonts.Count)
		{
			throw new ApplicationException("Extended Format record FontIndex field has wrong value");
		}
		Index = (ushort)m_book.InnerExtFormats.Count;
		if (isInitializeColors || !m_book.IsWorkbookOpening || m_book.Version != 0 || IncludePatterns || !HasParent)
		{
			InitializeColors();
			CopyColors(xfExt);
		}
		CheckAndUpdateHasBorder(format);
	}

	public void UpdateFromParent()
	{
		ExtendedFormatImpl parentFormat = ParentFormat;
		if (m_book.Version == OfficeVersion.Excel97to2003 && !IncludePatterns && HasParent)
		{
			if ((ushort)parentFormat.FillBackground != m_extFormat.FillBackground || (ushort)parentFormat.FillForeground != m_extFormat.FillForeground)
			{
				ushort fillForeground = m_extFormat.FillForeground;
				ushort fillBackground = m_extFormat.FillBackground;
				IncludePatterns = true;
				m_extFormat.FillForeground = fillForeground;
				m_extFormat.FillBackground = fillBackground;
			}
			InitializeColors();
		}
	}

	public void UpdateFromCurrentExtendedFormat(ExtendedFormatImpl CurrXF)
	{
		ShapeFillImpl gradient = null;
		if (CurrXF == null)
		{
			throw new ArgumentNullException("CurrentXF");
		}
		ExtendedFormatImpl extendedFormatImpl = CurrXF;
		ExtendedFormatRecord format = (ExtendedFormatRecord)extendedFormatImpl.Record.Clone();
		ExtendedXFRecord xfExt = extendedFormatImpl.XFRecord.CloneObject();
		ExtendedFormatImpl extendedFormatImpl2 = CurrXF;
		CurrXF = new ExtendedFormatImpl(base.Application, this, format, xfExt);
		CurrXF.ColorObject.CopyFrom(extendedFormatImpl2.ColorObject, callEvent: false);
		CurrXF.PatternColorObject.CopyFrom(extendedFormatImpl2.PatternColorObject, callEvent: false);
		CurrXF.Gradient = gradient;
		CurrXF.BottomBorderColor.CopyFrom(extendedFormatImpl2.BottomBorderColor, callEvent: false);
		CurrXF.TopBorderColor.CopyFrom(extendedFormatImpl2.TopBorderColor, callEvent: false);
		CurrXF.LeftBorderColor.CopyFrom(extendedFormatImpl2.LeftBorderColor, callEvent: false);
		CurrXF.RightBorderColor.CopyFrom(extendedFormatImpl2.RightBorderColor, callEvent: false);
		CurrXF.DiagonalBorderColor.CopyFrom(extendedFormatImpl2.DiagonalBorderColor, callEvent: false);
		CurrXF.Font.RGBColor = extendedFormatImpl2.Font.RGBColor;
		CurrXF.IndentLevel = extendedFormatImpl2.IndentLevel;
	}

	protected void InitializeColors()
	{
		m_color = new ChartColor((OfficeKnownColors)m_extFormat.FillBackground);
		m_color.AfterChange += UpdateColor;
		m_patternColor = new ChartColor((OfficeKnownColors)m_extFormat.FillForeground);
		m_patternColor.AfterChange += UpdatePatternColor;
		m_topBorderColor = new ChartColor((OfficeKnownColors)m_extFormat.TopBorderPaletteIndex);
		m_topBorderColor.AfterChange += UpdateTopBorderColor;
		m_bottomBorderColor = new ChartColor((OfficeKnownColors)m_extFormat.BottomBorderPaletteIndex);
		m_bottomBorderColor.AfterChange += UpdateBottomBorderColor;
		m_leftBorderColor = new ChartColor((OfficeKnownColors)m_extFormat.LeftBorderPaletteIndex);
		m_leftBorderColor.AfterChange += UpdateLeftBorderColor;
		m_rightBorderColor = new ChartColor((OfficeKnownColors)m_extFormat.RightBorderPaletteIndex);
		m_rightBorderColor.AfterChange += UpdateRightBorderColor;
		m_diagonalBorderColor = new ChartColor((OfficeKnownColors)m_extFormat.DiagonalLineColor);
		m_diagonalBorderColor.AfterChange += UpdateDiagonalBorderColor;
	}

	internal void UpdateColor()
	{
		m_extFormat.FillBackground = (ushort)m_color.GetIndexed(m_book);
	}

	internal void UpdatePatternColor()
	{
		m_extFormat.FillForeground = (ushort)m_patternColor.GetIndexed(m_book);
	}

	internal void UpdateTopBorderColor()
	{
		m_extFormat.TopBorderPaletteIndex = (ushort)m_topBorderColor.GetIndexed(m_book);
	}

	internal void UpdateBottomBorderColor()
	{
		m_extFormat.BottomBorderPaletteIndex = (ushort)m_bottomBorderColor.GetIndexed(m_book);
	}

	internal void UpdateLeftBorderColor()
	{
		m_extFormat.LeftBorderPaletteIndex = (ushort)m_leftBorderColor.GetIndexed(m_book);
	}

	internal void UpdateRightBorderColor()
	{
		m_extFormat.RightBorderPaletteIndex = (ushort)m_rightBorderColor.GetIndexed(m_book);
	}

	internal void UpdateDiagonalBorderColor()
	{
		m_extFormat.DiagonalLineColor = (ushort)m_diagonalBorderColor.GetIndexed(m_book);
	}

	[CLSCompliant(false)]
	public void Serialize(OffsetArrayList records, uint[] crcCache)
	{
		ExtendedFormatRecord extendedFormatRecord = (ExtendedFormatRecord)m_extFormat.Clone();
		extendedFormatRecord.FillBackground = (ushort)ColorIndex;
		extendedFormatRecord.FillForeground = (ushort)PatternColorIndex;
		CheckAndCorrectFormatRecord(extendedFormatRecord);
		records.Add(extendedFormatRecord);
		byte[] data = extendedFormatRecord.Data;
		m_book.crcValue = m_book.CalculateCRC(m_book.crcValue, data, crcCache);
	}

	[CLSCompliant(false)]
	protected void CheckAndCorrectFormatRecord(ExtendedFormatRecord record)
	{
		if (ParentIndex == 0)
		{
			ExtendedFormatRecord extFormat = ((ExtendedFormatImpl)m_book.GetExtFormat(0)).m_extFormat;
			if (!record.IsNotParentAlignment)
			{
				record.CopyAlignment(extFormat);
			}
			if (!record.IsNotParentBorder)
			{
				record.CopyBorders(extFormat);
			}
			if (!record.IsNotParentCellOptions)
			{
				record.CopyProtection(extFormat);
			}
			if (!record.IsNotParentFont)
			{
				record.FontIndex = extFormat.FontIndex;
			}
			if (!record.IsNotParentFormat)
			{
				record.FormatIndex = extFormat.FormatIndex;
			}
			if (!record.IsNotParentPattern)
			{
				record.CopyPatterns(extFormat);
			}
		}
	}

	[CLSCompliant(false)]
	public void SerializeXFormat(OffsetArrayList records)
	{
		if (m_xfExt != null)
		{
			ExtendedXFRecord value = m_xfExt.CloneObject();
			records.Add(value);
		}
	}

	private void CopyColors(ExtendedXFRecord xfExt)
	{
		if (xfExt.Properties.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < xfExt.Properties.Count; i++)
		{
			ExtendedProperty extendedProperty = xfExt.Properties[i];
			Color color = m_book.ConvertRGBAToARGB(m_book.UIntToColor(extendedProperty.ColorValue));
			ColorType colorType = extendedProperty.ColorType;
			if (colorType != ColorType.RGB && colorType != ColorType.Theme && extendedProperty.Indent <= 15)
			{
				continue;
			}
			switch (extendedProperty.Type)
			{
			case CellPropertyExtensionType.BackColor:
				if (colorType == ColorType.Theme)
				{
					extendedProperty.Tint /= 32767.0;
					ColorObject.SetTheme((int)extendedProperty.ColorValue, Workbook, extendedProperty.Tint);
				}
				else if (FillPattern == OfficePattern.Solid)
				{
					PatternColorObject.ColorType = extendedProperty.ColorType;
					PatternColor = color;
				}
				else
				{
					ColorObject.ColorType = extendedProperty.ColorType;
					Color = color;
				}
				break;
			case CellPropertyExtensionType.ForeColor:
				if (colorType == ColorType.Theme)
				{
					extendedProperty.Tint /= 32767.0;
					ColorObject.SetTheme((int)extendedProperty.ColorValue, Workbook, extendedProperty.Tint);
				}
				else if (FillPattern == OfficePattern.Solid)
				{
					ColorObject.ColorType = extendedProperty.ColorType;
					Color = color;
				}
				else
				{
					PatternColorObject.ColorType = extendedProperty.ColorType;
					PatternColor = color;
				}
				break;
			case CellPropertyExtensionType.TopBorderColor:
				if (colorType == ColorType.RGB)
				{
					Borders[OfficeBordersIndex.EdgeTop].ColorObject.ColorType = extendedProperty.ColorType;
					Borders[OfficeBordersIndex.EdgeTop].ColorRGB = color;
				}
				break;
			case CellPropertyExtensionType.BottomBorderColor:
				if (colorType == ColorType.RGB)
				{
					Borders[OfficeBordersIndex.EdgeBottom].ColorObject.ColorType = extendedProperty.ColorType;
					Borders[OfficeBordersIndex.EdgeBottom].ColorRGB = color;
				}
				break;
			case CellPropertyExtensionType.LeftBorderColor:
				if (colorType == ColorType.RGB)
				{
					Borders[OfficeBordersIndex.EdgeLeft].ColorObject.ColorType = extendedProperty.ColorType;
					Borders[OfficeBordersIndex.EdgeLeft].ColorRGB = color;
				}
				break;
			case CellPropertyExtensionType.RightBorderColor:
				if (colorType == ColorType.RGB)
				{
					Borders[OfficeBordersIndex.EdgeRight].ColorObject.ColorType = extendedProperty.ColorType;
					Borders[OfficeBordersIndex.EdgeRight].ColorRGB = color;
				}
				break;
			case CellPropertyExtensionType.DiagonalCellBorder:
				if (colorType == ColorType.RGB)
				{
					Borders[OfficeBordersIndex.DiagonalUp].ColorObject.ColorType = extendedProperty.ColorType;
					Borders[OfficeBordersIndex.DiagonalUp].ColorRGB = color;
					Borders[OfficeBordersIndex.DiagonalDown].ColorObject.ColorType = extendedProperty.ColorType;
					Borders[OfficeBordersIndex.DiagonalDown].ColorRGB = color;
				}
				break;
			case CellPropertyExtensionType.TextColor:
				if (colorType == ColorType.RGB)
				{
					Font.RGBColor = color;
				}
				break;
			case CellPropertyExtensionType.TextIndentationLevel:
				if (colorType != ColorType.Theme)
				{
					IndentLevel = extendedProperty.Indent;
				}
				break;
			}
		}
	}

	public int CompareTo(object obj)
	{
		if (!(obj is ExtendedFormatImpl))
		{
			throw new ArgumentException("Can only compare types with the same type", "obj");
		}
		ExtendedFormatImpl twin = (ExtendedFormatImpl)obj;
		return CompareTo(twin);
	}

	public int CompareTo(ExtendedFormatImpl twin)
	{
		CheckAndCorrectFormatRecord(m_extFormat);
		twin.CheckAndCorrectFormatRecord(twin.m_extFormat);
		byte[] data = m_extFormat.Data;
		byte[] data2 = twin.m_extFormat.Data;
		int i = 0;
		for (int num = Math.Min(data.Length, data2.Length); i < num; i++)
		{
			int result;
			if ((result = data[i].CompareTo(data2[i])) != 0)
			{
				return result;
			}
		}
		return data.Length - data2.Length;
	}

	public int CompareToWithoutIndex(ExtendedFormatImpl twin)
	{
		int num = 1;
		num = ((m_gradient == null) ? ((twin.Gradient != null) ? 1 : 0) : m_gradient.CompareTo(twin.Gradient));
		if (num == 0)
		{
			num = ((!(m_color == twin.m_color) || !(m_patternColor == twin.m_patternColor) || PivotButton != twin.PivotButton) ? 1 : 0);
		}
		if (num != 0 || m_extFormat.CompareTo(twin.m_extFormat) != 0)
		{
			return 1;
		}
		return 0;
	}

	public override int GetHashCode()
	{
		if (m_gradient == null)
		{
			return m_extFormat.GetHashCode();
		}
		return m_extFormat.GetHashCode() ^ m_gradient.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ExtendedFormatImpl twin))
		{
			return false;
		}
		return CompareToWithoutIndex(twin) == 0;
	}

	public static void CopyFromTo(ExtendedFormatImpl childFormat, ExtendedFormatImpl parentFormat, bool bSetFlag)
	{
		CopyAlignment(childFormat, parentFormat, bSetFlag);
		CopyBorders(childFormat, parentFormat, bSetFlag);
		CopyFont(childFormat, parentFormat, bSetFlag);
		CopyFormat(childFormat, parentFormat, bSetFlag);
		CopyPatterns(childFormat, parentFormat, bSetFlag);
		CopyProtection(childFormat, parentFormat, bSetFlag);
	}

	private static void CopyAlignment(ExtendedFormatImpl childFormat, ExtendedFormatImpl parentFormat, bool bSetFlag)
	{
		parentFormat.VerticalAlignment = childFormat.VerticalAlignment;
		parentFormat.HorizontalAlignment = childFormat.HorizontalAlignment;
		parentFormat.WrapText = childFormat.WrapText;
		parentFormat.IndentLevel = childFormat.IndentLevel;
		parentFormat.Rotation = childFormat.Rotation;
		parentFormat.ReadingOrder = childFormat.ReadingOrder;
		if (bSetFlag)
		{
			childFormat.IncludeAlignment = false;
		}
	}

	private static void CopyBorders(ExtendedFormatImpl childFormat, ExtendedFormatImpl parentFormat, bool bSetFlag)
	{
		parentFormat.Record.BorderBottom = childFormat.Record.BorderBottom;
		parentFormat.Record.BorderLeft = childFormat.Record.BorderLeft;
		parentFormat.Record.BorderRight = childFormat.Record.BorderRight;
		parentFormat.Record.BorderTop = childFormat.Record.BorderTop;
		parentFormat.Record.DiagonalFromBottomLeft = childFormat.Record.DiagonalFromBottomLeft;
		parentFormat.Record.DiagonalFromTopLeft = childFormat.Record.DiagonalFromTopLeft;
		parentFormat.Record.DiagonalLineStyle = childFormat.Record.DiagonalLineStyle;
		parentFormat.TopBorderColor.CopyFrom(childFormat.TopBorderColor, callEvent: true);
		parentFormat.BottomBorderColor.CopyFrom(childFormat.BottomBorderColor, callEvent: true);
		parentFormat.LeftBorderColor.CopyFrom(childFormat.LeftBorderColor, callEvent: true);
		parentFormat.RightBorderColor.CopyFrom(childFormat.RightBorderColor, callEvent: true);
		parentFormat.DiagonalBorderColor.CopyFrom(childFormat.DiagonalBorderColor, callEvent: true);
		if (bSetFlag)
		{
			childFormat.IncludeBorder = false;
		}
	}

	private static void CopyFont(ExtendedFormatImpl childFormat, ExtendedFormatImpl parentFormat, bool bSetFlag)
	{
		parentFormat.FontIndex = childFormat.FontIndex;
		if (bSetFlag)
		{
			childFormat.IncludeFont = false;
		}
	}

	private static void CopyFormat(ExtendedFormatImpl childFormat, ExtendedFormatImpl parentFormat, bool bSetFlag)
	{
		parentFormat.NumberFormat = childFormat.NumberFormat;
		if (bSetFlag)
		{
			childFormat.IncludeNumberFormat = false;
		}
	}

	private static void CopyPatterns(ExtendedFormatImpl childFormat, ExtendedFormatImpl parentFormat, bool bSetFlag)
	{
		parentFormat.ColorObject.CopyFrom(childFormat.ColorObject, callEvent: true);
		parentFormat.PatternColorObject.CopyFrom(childFormat.PatternColorObject, callEvent: true);
		parentFormat.FillPattern = childFormat.FillPattern;
		if (bSetFlag)
		{
			childFormat.IncludePatterns = false;
		}
	}

	private static void CopyProtection(ExtendedFormatImpl childFormat, ExtendedFormatImpl parentFormat, bool bSetFlag)
	{
		parentFormat.FormulaHidden = childFormat.FormulaHidden;
		parentFormat.Locked = childFormat.Locked;
		if (bSetFlag)
		{
			childFormat.IncludeProtection = false;
		}
	}

	protected internal void CopyColorsFrom(ExtendedFormatImpl format)
	{
		m_color.CopyFrom(format.m_color, callEvent: false);
		m_patternColor.CopyFrom(format.m_patternColor, callEvent: false);
		m_topBorderColor.CopyFrom(format.m_topBorderColor, callEvent: false);
		m_bottomBorderColor.CopyFrom(format.m_bottomBorderColor, callEvent: false);
		m_leftBorderColor.CopyFrom(format.m_leftBorderColor, callEvent: false);
		m_rightBorderColor.CopyFrom(format.m_rightBorderColor, callEvent: false);
		m_diagonalBorderColor.CopyFrom(format.m_diagonalBorderColor, callEvent: false);
	}

	public object Clone()
	{
		return TypedClone(this);
	}

	public ExtendedFormatImpl TypedClone(object parent)
	{
		ExtendedFormatImpl extendedFormatImpl = MemberwiseClone() as ExtendedFormatImpl;
		extendedFormatImpl.m_extFormat = m_extFormat.Clone() as ExtendedFormatRecord;
		extendedFormatImpl.m_xfExt = m_xfExt.CloneObject();
		extendedFormatImpl.Index = 65535;
		if (parent != extendedFormatImpl.Parent)
		{
			extendedFormatImpl.SetParent(parent);
			extendedFormatImpl.FindParents();
		}
		if (extendedFormatImpl.m_gradient != null)
		{
			extendedFormatImpl.m_gradient = m_gradient.Clone(extendedFormatImpl);
		}
		if (ParentIndex == m_book.MaxXFCount)
		{
			extendedFormatImpl.ParentIndex = extendedFormatImpl.m_book.MaxXFCount;
		}
		extendedFormatImpl.InitializeColors();
		extendedFormatImpl.m_color.CopyFrom(m_color, callEvent: false);
		extendedFormatImpl.m_patternColor.CopyFrom(m_patternColor, callEvent: false);
		extendedFormatImpl.m_topBorderColor.CopyFrom(m_topBorderColor, callEvent: false);
		extendedFormatImpl.m_bottomBorderColor.CopyFrom(m_bottomBorderColor, callEvent: false);
		extendedFormatImpl.m_leftBorderColor.CopyFrom(m_leftBorderColor, callEvent: false);
		extendedFormatImpl.m_rightBorderColor.CopyFrom(m_rightBorderColor, callEvent: false);
		extendedFormatImpl.m_diagonalBorderColor.CopyFrom(m_diagonalBorderColor, callEvent: false);
		return extendedFormatImpl;
	}

	object ICloneParent.Clone(object parent)
	{
		return TypedClone(parent);
	}

	public void Clear()
	{
		m_gradient = null;
		m_xfExt = null;
		m_extFormat = null;
		m_color = null;
		m_patternColor = null;
		m_topBorderColor = null;
		m_bottomBorderColor = null;
		m_leftBorderColor = null;
		m_rightBorderColor = null;
		m_diagonalBorderColor = null;
		Dispose();
	}

	void IDisposable.Dispose()
	{
		GC.SuppressFinalize(this);
	}

	internal void clearAll()
	{
		if (m_gradient != null)
		{
			m_gradient.Clear();
		}
		if (m_color != null)
		{
			m_color.Dispose();
		}
		if (m_patternColor != null)
		{
			m_patternColor.Dispose();
		}
		if (m_topBorderColor != null)
		{
			m_topBorderColor.Dispose();
		}
		if (m_bottomBorderColor != null)
		{
			m_bottomBorderColor.Dispose();
		}
		if (m_leftBorderColor != null)
		{
			m_leftBorderColor.Dispose();
		}
		if (m_rightBorderColor != null)
		{
			m_rightBorderColor.Dispose();
		}
		if (m_diagonalBorderColor != null)
		{
			m_diagonalBorderColor.Dispose();
		}
		if (m_extFormat != null)
		{
			m_extFormat = null;
		}
		Clear();
	}

	private void CheckAndUpdateHasBorder(ExtendedFormatRecord record)
	{
		if ((record.BorderBottom | record.BorderLeft | record.BorderTop | record.BorderRight) > OfficeLineStyle.None || record.DiagonalLineStyle > 0)
		{
			m_hasBorder = true;
		}
		else
		{
			m_hasBorder = false;
		}
	}
}
