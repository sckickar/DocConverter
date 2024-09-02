using System;
using System.Collections;
using DocGen.Drawing;

namespace DocGen.OfficeChart.Implementation.Collections.Grouping;

internal class BordersGroup : CollectionBaseEx<object>, IBorders, IEnumerable, IParentApplication
{
	private StyleGroup m_style;

	public new IBorders this[int index] => m_style[index].Borders;

	public int GroupCount => m_style.Count;

	public OfficeKnownColors Color
	{
		get
		{
			int groupCount = GroupCount;
			if (groupCount == 0)
			{
				return OfficeKnownColors.Black;
			}
			OfficeKnownColors color = this[0].Color;
			for (int i = 1; i < groupCount; i++)
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
			for (int groupCount = GroupCount; i < groupCount; i++)
			{
				this[i].Color = value;
			}
		}
	}

	public Color ColorRGB
	{
		get
		{
			int groupCount = GroupCount;
			if (groupCount == 0)
			{
				return ColorExtension.Empty;
			}
			Color colorRGB = this[0].ColorRGB;
			for (int i = 1; i < groupCount; i++)
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
			for (int groupCount = GroupCount; i < groupCount; i++)
			{
				this[i].ColorRGB = value;
			}
		}
	}

	int IBorders.Count
	{
		get
		{
			int groupCount = GroupCount;
			if (groupCount == 0)
			{
				return int.MinValue;
			}
			int count = this[0].Count;
			for (int i = 1; i < groupCount; i++)
			{
				if (count != this[i].Count)
				{
					return int.MinValue;
				}
			}
			return count;
		}
	}

	public IBorder this[OfficeBordersIndex Index] => null;

	public OfficeLineStyle LineStyle
	{
		get
		{
			int groupCount = GroupCount;
			if (groupCount == 0)
			{
				return OfficeLineStyle.None;
			}
			OfficeLineStyle lineStyle = this[0].LineStyle;
			for (int i = 1; i < groupCount; i++)
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
			for (int groupCount = GroupCount; i < groupCount; i++)
			{
				this[i].LineStyle = value;
			}
		}
	}

	public OfficeLineStyle Value
	{
		get
		{
			int groupCount = GroupCount;
			if (groupCount == 0)
			{
				return OfficeLineStyle.None;
			}
			OfficeLineStyle value = this[0].Value;
			for (int i = 1; i < groupCount; i++)
			{
				if (value != this[i].Value)
				{
					return OfficeLineStyle.None;
				}
			}
			return value;
		}
		set
		{
			int i = 0;
			for (int groupCount = GroupCount; i < groupCount; i++)
			{
				this[i].Value = value;
			}
		}
	}

	public BordersGroup(IApplication application, object parent)
		: base(application, parent)
	{
		FindParents();
		base.InnerList.Add(new BorderGroup(application, this, OfficeBordersIndex.DiagonalDown));
		base.InnerList.Add(new BorderGroup(application, this, OfficeBordersIndex.DiagonalUp));
		base.InnerList.Add(new BorderGroup(application, this, OfficeBordersIndex.EdgeBottom));
		base.InnerList.Add(new BorderGroup(application, this, OfficeBordersIndex.EdgeLeft));
		base.InnerList.Add(new BorderGroup(application, this, OfficeBordersIndex.EdgeRight));
		base.InnerList.Add(new BorderGroup(application, this, OfficeBordersIndex.EdgeTop));
	}

	private void FindParents()
	{
		m_style = FindParent(typeof(StyleGroup)) as StyleGroup;
		if (m_style == null)
		{
			throw new ArgumentOutOfRangeException("parent", "Can't find parent style group.");
		}
	}
}
