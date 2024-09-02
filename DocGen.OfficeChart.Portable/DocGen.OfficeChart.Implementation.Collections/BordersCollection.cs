using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.OfficeChart.Interfaces;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class BordersCollection : CollectionBaseEx<IBorder>, IBorders, IEnumerable, IParentApplication
{
	private WorkbookImpl m_book;

	private bool m_bIsEmptyBorder = true;

	public OfficeKnownColors Color
	{
		get
		{
			OfficeKnownColors color = base.InnerList[0].Color;
			for (int i = 1; i < base.Count; i++)
			{
				if (color != base.InnerList[i].Color)
				{
					return OfficeKnownColors.Black;
				}
			}
			return color;
		}
		set
		{
			for (int i = 0; i < base.Count; i++)
			{
				base.InnerList[i].Color = value;
			}
		}
	}

	public Color ColorRGB
	{
		get
		{
			Color result = base.InnerList[0].ColorRGB;
			int num = result.ToArgb();
			for (int i = 1; i < base.Count; i++)
			{
				if (num != base.InnerList[i].ColorRGB.ToArgb())
				{
					result = ColorExtension.Empty;
					break;
				}
			}
			return result;
		}
		set
		{
			for (int i = 0; i < base.Count; i++)
			{
				base.InnerList[i].ColorRGB = value;
			}
		}
	}

	public IBorder this[OfficeBordersIndex index] => index switch
	{
		OfficeBordersIndex.DiagonalDown => base.InnerList[0], 
		OfficeBordersIndex.DiagonalUp => base.InnerList[1], 
		OfficeBordersIndex.EdgeBottom => base.InnerList[2], 
		OfficeBordersIndex.EdgeLeft => base.InnerList[3], 
		OfficeBordersIndex.EdgeRight => base.InnerList[4], 
		OfficeBordersIndex.EdgeTop => base.InnerList[5], 
		_ => null, 
	};

	public OfficeLineStyle LineStyle
	{
		get
		{
			OfficeLineStyle lineStyle = base.InnerList[0].LineStyle;
			for (int i = 1; i < base.Count; i++)
			{
				if (lineStyle != base.InnerList[i].LineStyle)
				{
					return OfficeLineStyle.None;
				}
			}
			return lineStyle;
		}
		set
		{
			for (int i = 0; i < base.Count; i++)
			{
				base.InnerList[i].LineStyle = value;
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

	internal bool IsEmptyBorder
	{
		get
		{
			return m_bIsEmptyBorder;
		}
		set
		{
			m_bIsEmptyBorder = value;
		}
	}

	internal BordersCollection(IApplication application, object parent, bool bAddEmpty)
		: base(application, parent)
	{
		SetParents();
		if (bAddEmpty)
		{
			base.InnerList.AddRange(new BorderImpl[6]);
		}
	}

	public BordersCollection(IApplication application, object parent, IInternalExtendedFormat wrap)
		: this(application, parent, bAddEmpty: false)
	{
		base.InnerList.Add(new BorderImpl(application, this, wrap, OfficeBordersIndex.DiagonalDown));
		base.InnerList.Add(new BorderImpl(application, this, wrap, OfficeBordersIndex.DiagonalUp));
		base.InnerList.Add(new BorderImpl(application, this, wrap, OfficeBordersIndex.EdgeBottom));
		base.InnerList.Add(new BorderImpl(application, this, wrap, OfficeBordersIndex.EdgeLeft));
		base.InnerList.Add(new BorderImpl(application, this, wrap, OfficeBordersIndex.EdgeRight));
		base.InnerList.Add(new BorderImpl(application, this, wrap, OfficeBordersIndex.EdgeTop));
	}

	public override bool Equals(object obj)
	{
		if (!(obj is BordersCollection bordersCollection))
		{
			return false;
		}
		List<IBorder> innerList = base.InnerList;
		List<IBorder> innerList2 = bordersCollection.InnerList;
		bool flag = true;
		int i = 0;
		for (int count = base.Count; i < count && flag; i++)
		{
			if (!innerList[i].Equals(innerList2[i]))
			{
				flag = false;
				break;
			}
		}
		return flag;
	}

	public override int GetHashCode()
	{
		int num = 0;
		List<IBorder> innerList = base.InnerList;
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			num ^= innerList[i].GetHashCode();
		}
		return num;
	}

	private void SetParents()
	{
		m_book = FindParent(typeof(WorkbookImpl)) as WorkbookImpl;
		if (m_book == null)
		{
			throw new ArgumentNullException("Can't find parent workbook.");
		}
	}

	internal void SetBorder(OfficeBordersIndex index, IBorder border)
	{
		switch (index)
		{
		case OfficeBordersIndex.DiagonalDown:
			base.InnerList[0] = border;
			break;
		case OfficeBordersIndex.DiagonalUp:
			base.InnerList[1] = border;
			break;
		case OfficeBordersIndex.EdgeBottom:
			base.InnerList[2] = border;
			break;
		case OfficeBordersIndex.EdgeLeft:
			base.InnerList[3] = border;
			break;
		case OfficeBordersIndex.EdgeRight:
			base.InnerList[4] = border;
			break;
		case OfficeBordersIndex.EdgeTop:
			base.InnerList[5] = border;
			break;
		default:
			Add(border);
			break;
		}
	}

	internal void Dispose()
	{
		foreach (BorderImpl inner in base.InnerList)
		{
			inner.Clear();
		}
	}
}
