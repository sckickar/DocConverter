using System;
using System.Collections.Generic;
using DocGen.Drawing;

namespace DocGen.OfficeChart.Implementation;

internal class BorderImplArrayWrapper : CommonObject, IBorder, IParentApplication
{
	private List<IRange> m_arrCells = new List<IRange>();

	private OfficeBordersIndex m_border;

	private WorkbookImpl m_book;

	public OfficeKnownColors Color
	{
		get
		{
			OfficeKnownColors color = m_arrCells[0].Borders[m_border].Color;
			for (int i = 1; i < m_arrCells.Count; i++)
			{
				if (color != m_arrCells[i].Borders[m_border].Color)
				{
					return OfficeKnownColors.Black;
				}
			}
			return color;
		}
		set
		{
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				m_arrCells[i].Borders[m_border].Color = value;
			}
		}
	}

	public ChartColor ColorObject
	{
		get
		{
			ChartColor colorObject = m_arrCells[0].Borders[m_border].ColorObject;
			for (int i = 1; i < m_arrCells.Count; i++)
			{
				if (colorObject != m_arrCells[i].Borders[m_border].ColorObject)
				{
					return null;
				}
			}
			return colorObject;
		}
	}

	public Color ColorRGB
	{
		get
		{
			if (ColorObject == null)
			{
				return m_arrCells[0].Borders[m_border].ColorObject.GetRGB(m_book);
			}
			return ColorObject.GetRGB(m_book);
		}
		set
		{
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				m_arrCells[i].Borders[m_border].ColorRGB = value;
			}
		}
	}

	public OfficeLineStyle LineStyle
	{
		get
		{
			OfficeLineStyle lineStyle = m_arrCells[0].Borders[m_border].LineStyle;
			for (int i = 1; i < m_arrCells.Count; i++)
			{
				if (lineStyle != m_arrCells[i].Borders[m_border].LineStyle)
				{
					return OfficeLineStyle.None;
				}
			}
			return lineStyle;
		}
		set
		{
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				m_arrCells[i].Borders[m_border].LineStyle = value;
			}
		}
	}

	public bool ShowDiagonalLine
	{
		get
		{
			bool showDiagonalLine = m_arrCells[0].Borders[m_border].ShowDiagonalLine;
			for (int i = 1; i < m_arrCells.Count; i++)
			{
				if (showDiagonalLine != m_arrCells[i].Borders[m_border].ShowDiagonalLine)
				{
					return false;
				}
			}
			return showDiagonalLine;
		}
		set
		{
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				m_arrCells[i].Borders[m_border].ShowDiagonalLine = value;
			}
		}
	}

	public BorderImplArrayWrapper(IRange range, OfficeBordersIndex index)
		: base((range as RangeImpl).Application, range)
	{
		m_border = index;
		m_arrCells.AddRange(range.Cells);
		m_book = FindParent(typeof(WorkbookImpl)) as WorkbookImpl;
		if (m_book == null)
		{
			throw new ArgumentNullException("Can't find parent workbook");
		}
	}

	public BorderImplArrayWrapper(List<IRange> lstRange, OfficeBordersIndex index, IApplication application)
		: base(application, lstRange[0])
	{
		m_border = index;
		m_arrCells = lstRange;
		m_book = FindParent(typeof(WorkbookImpl)) as WorkbookImpl;
		if (m_book == null)
		{
			throw new ArgumentNullException("Can't find parent workbook");
		}
	}
}
