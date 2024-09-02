using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.Drawing;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class BordersCollectionArrayWrapper : CollectionBaseEx<object>, IBorders, IEnumerable, IParentApplication
{
	private List<IRange> m_arrCells = new List<IRange>();

	private WorkbookImpl m_book;

	private IApplication m_application;

	public OfficeKnownColors Color
	{
		get
		{
			OfficeKnownColors color = m_arrCells[0].Borders.Color;
			for (int i = 1; i < m_arrCells.Count; i++)
			{
				if (m_arrCells[i].Borders.Color != color)
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
				m_arrCells[i].Borders.Color = value;
			}
		}
	}

	public Color ColorRGB
	{
		get
		{
			Color result = m_arrCells[0].Borders.ColorRGB;
			int num = result.ToArgb();
			for (int i = 1; i < m_arrCells.Count; i++)
			{
				if (m_arrCells[i].Borders.ColorRGB.ToArgb() != num)
				{
					result = ColorExtension.Empty;
					break;
				}
			}
			return result;
		}
		set
		{
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				m_arrCells[i].Borders.ColorRGB = value;
			}
		}
	}

	public IBorder this[OfficeBordersIndex Index]
	{
		get
		{
			RangeImpl rangeImpl = ((IRange)base.Parent) as RangeImpl;
			if (rangeImpl.IsEntireRow || rangeImpl.IsEntireColumn)
			{
				return new BorderImplArrayWrapper(m_arrCells, Index, m_application);
			}
			return new BorderImplArrayWrapper((IRange)base.Parent, Index);
		}
	}

	public OfficeLineStyle LineStyle
	{
		get
		{
			OfficeLineStyle lineStyle = m_arrCells[0].Borders.LineStyle;
			for (int i = 1; i < m_arrCells.Count; i++)
			{
				if (lineStyle != m_arrCells[i].Borders.LineStyle)
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
				m_arrCells[i].Borders.LineStyle = value;
			}
		}
	}

	public OfficeLineStyle Value
	{
		get
		{
			return LineStyle;
		}
		set
		{
			LineStyle = value;
		}
	}

	public BordersCollectionArrayWrapper(IRange range)
		: base((range as RangeImpl).Application, (object)range)
	{
		m_arrCells.AddRange(range.Cells);
	}

	public BordersCollectionArrayWrapper(List<IRange> lstRange, IApplication application)
		: base(application, (object)lstRange[0])
	{
		m_arrCells = lstRange;
		m_application = application;
	}

	private void SetParents()
	{
		m_book = FindParent(typeof(WorkbookImpl)) as WorkbookImpl;
		if (m_book == null)
		{
			throw new ArgumentNullException("Can't find parent workbook.");
		}
	}
}
