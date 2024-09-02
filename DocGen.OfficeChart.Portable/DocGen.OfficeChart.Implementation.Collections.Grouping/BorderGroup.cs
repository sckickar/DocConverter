using System;
using DocGen.Drawing;

namespace DocGen.OfficeChart.Implementation.Collections.Grouping;

internal class BorderGroup : CommonObject, IBorder, IParentApplication
{
	private OfficeBordersIndex m_index;

	private BordersGroup m_bordersGroup;

	public IBorder this[int index] => m_bordersGroup[index][m_index];

	public int Count => m_bordersGroup.GroupCount;

	public OfficeKnownColors Color
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return OfficeKnownColors.Black;
			}
			OfficeKnownColors color = this[0].Color;
			for (int i = 1; i < count; i++)
			{
				if (color != this[i].Color)
				{
					return OfficeKnownColors.Black;
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

	public ChartColor ColorObject
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public Color ColorRGB
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return ColorExtension.Empty;
			}
			Color colorRGB = this[0].ColorRGB;
			for (int i = 1; i < count; i++)
			{
				if (colorRGB != this[i].ColorRGB)
				{
					return ColorExtension.Empty;
				}
			}
			return colorRGB;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].ColorRGB = value;
			}
		}
	}

	public OfficeLineStyle LineStyle
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return OfficeLineStyle.None;
			}
			OfficeLineStyle lineStyle = this[0].LineStyle;
			for (int i = 1; i < count; i++)
			{
				if (lineStyle != this[i].LineStyle)
				{
					return OfficeLineStyle.None;
				}
			}
			return lineStyle;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].LineStyle = value;
			}
		}
	}

	public bool ShowDiagonalLine
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return false;
			}
			bool showDiagonalLine = this[0].ShowDiagonalLine;
			for (int i = 1; i < count; i++)
			{
				if (showDiagonalLine != this[i].ShowDiagonalLine)
				{
					return false;
				}
			}
			return showDiagonalLine;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].ShowDiagonalLine = value;
			}
		}
	}

	public BorderGroup(IApplication application, object parent, OfficeBordersIndex index)
		: base(application, parent)
	{
		m_index = index;
		FindParents();
	}

	private void FindParents()
	{
		m_bordersGroup = FindParent(typeof(BordersGroup)) as BordersGroup;
		if (m_bordersGroup == null)
		{
			throw new ArgumentOutOfRangeException("parent", "Can't find parent borders group.");
		}
	}
}
