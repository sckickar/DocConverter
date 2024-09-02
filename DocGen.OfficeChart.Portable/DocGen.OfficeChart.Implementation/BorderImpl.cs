using System;
using DocGen.Drawing;
using DocGen.OfficeChart.Interfaces;

namespace DocGen.OfficeChart.Implementation;

internal class BorderImpl : CommonObject, IBorder, IParentApplication, IDisposable
{
	public const int DEF_MAXBADCOLOR = 8;

	public const int DEF_BADCOLOR_INCREMENT = 64;

	private OfficeBordersIndex m_border;

	private IInternalExtendedFormat m_format;

	public OfficeKnownColors Color
	{
		get
		{
			return ColorObject.GetIndexed(m_format.Workbook);
		}
		set
		{
			value = NormalizeColor(value);
			if (m_format is CellStyle)
			{
				(m_format as CellStyle).AskAdjacent = false;
			}
			m_format.BeginUpdate();
			ColorObject.SetIndexed(value);
			m_format.EndUpdate();
		}
	}

	public ChartColor ColorObject => m_border switch
	{
		OfficeBordersIndex.EdgeTop => m_format.TopBorderColor, 
		OfficeBordersIndex.EdgeBottom => m_format.BottomBorderColor, 
		OfficeBordersIndex.EdgeLeft => m_format.LeftBorderColor, 
		OfficeBordersIndex.EdgeRight => m_format.RightBorderColor, 
		OfficeBordersIndex.DiagonalDown => m_format.DiagonalBorderColor, 
		OfficeBordersIndex.DiagonalUp => m_format.DiagonalBorderColor, 
		_ => throw new ArgumentOutOfRangeException("Border index"), 
	};

	public Color ColorRGB
	{
		get
		{
			return ColorObject.GetRGB(Workbook);
		}
		set
		{
			if (m_format is CellStyle)
			{
				(m_format as CellStyle).AskAdjacent = false;
			}
			m_format.BeginUpdate();
			ColorObject.SetRGB(value, Workbook);
			m_format.EndUpdate();
		}
	}

	public OfficeLineStyle LineStyle
	{
		get
		{
			return m_border switch
			{
				OfficeBordersIndex.EdgeTop => m_format.TopBorderLineStyle, 
				OfficeBordersIndex.EdgeBottom => m_format.BottomBorderLineStyle, 
				OfficeBordersIndex.EdgeLeft => m_format.LeftBorderLineStyle, 
				OfficeBordersIndex.EdgeRight => m_format.RightBorderLineStyle, 
				OfficeBordersIndex.DiagonalDown => m_format.DiagonalDownBorderLineStyle, 
				OfficeBordersIndex.DiagonalUp => m_format.DiagonalUpBorderLineStyle, 
				_ => throw new ArgumentOutOfRangeException("Border index"), 
			};
		}
		set
		{
			switch (m_border)
			{
			case OfficeBordersIndex.EdgeTop:
				m_format.TopBorderLineStyle = value;
				break;
			case OfficeBordersIndex.EdgeBottom:
				m_format.BottomBorderLineStyle = value;
				break;
			case OfficeBordersIndex.EdgeLeft:
				m_format.LeftBorderLineStyle = value;
				break;
			case OfficeBordersIndex.EdgeRight:
				m_format.RightBorderLineStyle = value;
				break;
			case OfficeBordersIndex.DiagonalDown:
				m_format.DiagonalDownBorderLineStyle = value;
				break;
			case OfficeBordersIndex.DiagonalUp:
				m_format.DiagonalUpBorderLineStyle = value;
				break;
			default:
				throw new ArgumentOutOfRangeException("Border index");
			}
		}
	}

	public bool ShowDiagonalLine
	{
		get
		{
			return m_border switch
			{
				OfficeBordersIndex.DiagonalDown => m_format.DiagonalDownVisible, 
				OfficeBordersIndex.DiagonalUp => m_format.DiagonalUpVisible, 
				_ => false, 
			};
		}
		set
		{
			switch (m_border)
			{
			case OfficeBordersIndex.DiagonalDown:
				m_format.DiagonalDownVisible = value;
				break;
			case OfficeBordersIndex.DiagonalUp:
				m_format.DiagonalUpVisible = value;
				break;
			}
		}
	}

	internal OfficeBordersIndex BorderIndex
	{
		get
		{
			return m_border;
		}
		set
		{
			m_border = value;
		}
	}

	private WorkbookImpl Workbook => m_format.Workbook;

	private BorderImpl(IApplication application, object parent)
		: base(application, parent)
	{
	}

	private BorderImpl(IApplication application, object parent, OfficeBordersIndex borderIndex)
		: this(application, parent)
	{
		m_border = borderIndex;
	}

	public BorderImpl(IApplication application, object parent, IInternalExtendedFormat impl, OfficeBordersIndex borderIndex)
		: this(application, parent, borderIndex)
	{
		m_format = impl;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is BorderImpl borderImpl))
		{
			return false;
		}
		if (borderImpl.m_border == m_border && borderImpl.ShowDiagonalLine == ShowDiagonalLine && borderImpl.LineStyle == LineStyle)
		{
			return borderImpl.ColorObject == ColorObject;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return m_border.GetHashCode() ^ ShowDiagonalLine.GetHashCode() ^ LineStyle.GetHashCode() ^ ColorObject.GetHashCode();
	}

	public void CopyFrom(IBorder baseBorder)
	{
		ColorObject.CopyFrom(baseBorder.ColorObject, callEvent: true);
		LineStyle = baseBorder.LineStyle;
	}

	private void NormalizeColor()
	{
		if (LineStyle != 0 && ColorObject.ColorType == ColorType.Indexed)
		{
			ChartColor colorObject = ColorObject;
			OfficeKnownColors indexed = colorObject.GetIndexed(null);
			indexed = NormalizeColor(indexed);
			colorObject.SetIndexed(indexed);
		}
	}

	public static OfficeKnownColors NormalizeColor(OfficeKnownColors color)
	{
		int num = (int)color;
		if (num == 0)
		{
			num += 64;
			color = (OfficeKnownColors)num;
		}
		return (OfficeKnownColors)num;
	}

	public BorderImpl Clone(StyleImpl newFormat)
	{
		BorderImpl obj = MemberwiseClone() as BorderImpl;
		obj.m_format = newFormat;
		return obj;
	}

	void IDisposable.Dispose()
	{
		GC.SuppressFinalize(this);
	}

	internal void Clear()
	{
		m_format.TopBorderColor.Dispose();
		m_format.BottomBorderColor.Dispose();
		m_format.LeftBorderColor.Dispose();
		m_format.RightBorderColor.Dispose();
		m_format.DiagonalBorderColor.Dispose();
		m_format.DiagonalBorderColor.Dispose();
	}
}
