using System;
using System.Collections;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal sealed class ChartLabelLayoutManager : IEnumerable
{
	private class RectangleAreaComparer : IComparer
	{
		int IComparer.Compare(object x, object y)
		{
			RectangleF rectangleF = (RectangleF)x;
			RectangleF rectangleF2 = (RectangleF)y;
			float num = rectangleF.Width * rectangleF.Height;
			float num2 = rectangleF2.Width * rectangleF2.Height;
			if (num > num2)
			{
				return -1;
			}
			if (num < num2)
			{
				return 1;
			}
			return 0;
		}
	}

	private RectangleF m_workArea = Rectangle.Empty;

	private ArrayList m_labels = new ArrayList();

	private ArrayList m_currectArea = new ArrayList();

	private SizeF m_minimalLabelSize = SizeF.Empty;

	public SizeF MinimalSize
	{
		get
		{
			return m_minimalLabelSize;
		}
		set
		{
			m_minimalLabelSize = value;
		}
	}

	public int Count => m_labels.Count;

	public ChartLabel this[int index] => (ChartLabel)m_labels[index];

	public ChartLabelLayoutManager(RectangleF workArea)
	{
		m_workArea = workArea;
		m_currectArea.Add(m_workArea);
	}

	public RectangleF AddLabel(ChartLabel label)
	{
		label.Rect = FindFreeSpace(label);
		m_labels.Add(label);
		Exclude(label.Rect);
		return label.Rect;
	}

	public void AddPoint(PointF p)
	{
		Exclude(p);
	}

	public void Clear()
	{
		m_labels.Clear();
		m_currectArea.Clear();
		m_currectArea.Add(m_workArea);
	}

	public void Draw(Graphics g)
	{
		foreach (RectangleF item in m_currectArea)
		{
			g.DrawRectangle(Pens.Red, item.X, item.Y, item.Width, item.Height);
			g.DrawLine(Pens.Red, item.Left, item.Top, item.Right, item.Bottom);
			g.DrawLine(Pens.Red, item.Right, item.Top, item.Left, item.Bottom);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return m_labels.GetEnumerator();
	}

	private void Exclude2(RectangleF rect)
	{
		ArrayList arrayList = new ArrayList();
		foreach (RectangleF item in m_currectArea)
		{
			if (item.IntersectsWith(rect))
			{
				if (item.Left < rect.Left)
				{
					arrayList.Add(new RectangleF(item.Left, item.Top, rect.Left - item.Left, item.Height));
				}
				if (item.Top < rect.Top)
				{
					arrayList.Add(new RectangleF(item.Left, item.Top, item.Width, rect.Top - item.Top));
				}
				if (item.Right > rect.Right)
				{
					arrayList.Add(new RectangleF(rect.Right, item.Top, item.Right - rect.Right, item.Height));
				}
				if (item.Bottom > rect.Bottom)
				{
					arrayList.Add(new RectangleF(item.Left, rect.Bottom, item.Width, item.Bottom - rect.Bottom));
				}
			}
			else
			{
				arrayList.Add(item);
			}
		}
		m_currectArea = arrayList;
	}

	private void Exclude(RectangleF rect)
	{
		ArrayList arrayList = new ArrayList();
		foreach (RectangleF item in m_currectArea)
		{
			if (item.IntersectsWith(rect))
			{
				if (item.Left < rect.Left)
				{
					RectangleF rectangleF2 = new RectangleF(item.Left, item.Top, rect.Left - item.Left, item.Height);
					if (CheckWithMinSize(rectangleF2, arrayList))
					{
						arrayList.Add(rectangleF2);
					}
				}
				if (item.Top < rect.Top)
				{
					RectangleF rectangleF3 = new RectangleF(item.Left, item.Top, item.Width, rect.Top - item.Top);
					if (CheckWithMinSize(rectangleF3, arrayList))
					{
						arrayList.Add(rectangleF3);
					}
				}
				if (item.Right > rect.Right)
				{
					RectangleF rectangleF4 = new RectangleF(rect.Right, item.Top, item.Right - rect.Right, item.Height);
					if (CheckWithMinSize(rectangleF4, arrayList))
					{
						arrayList.Add(rectangleF4);
					}
				}
				if (item.Bottom > rect.Bottom)
				{
					RectangleF rectangleF5 = new RectangleF(item.Left, rect.Bottom, item.Width, item.Bottom - rect.Bottom);
					if (CheckWithMinSize(rectangleF5, arrayList))
					{
						arrayList.Add(rectangleF5);
					}
				}
			}
			else if (CheckWithMinSize(item, arrayList))
			{
				arrayList.Add(item);
			}
			arrayList.Sort(new RectangleAreaComparer());
		}
		m_currectArea = arrayList;
	}

	private void Exclude(PointF p)
	{
		RectangleF rect = new RectangleF(p, Size.Empty);
		Exclude(rect);
	}

	private RectangleF FindFreeSpace(ChartLabel label)
	{
		RectangleF result = RectangleF.Empty;
		double num = double.PositiveInfinity;
		RectangleF rectangleF = new RectangleF(label.ConnectPoint.X + label.Offset.Width, label.ConnectPoint.Y + label.Offset.Height, label.Size.Width, label.Size.Height);
		if (m_workArea.IntersectsWith(rectangleF))
		{
			foreach (RectangleF item in m_currectArea)
			{
				if (item.Contains(rectangleF))
				{
					result = rectangleF;
					break;
				}
				if (item.Width > label.Size.Width && item.Height > label.Size.Height)
				{
					RectangleF rectangleF2 = CalcBestPlace(label, item);
					if (num > CalcRadius(label.ConnectPoint, rectangleF2.Location))
					{
						num = CalcRadius(label.ConnectPoint, rectangleF2.Location);
						result = rectangleF2;
					}
				}
			}
		}
		return result;
	}

	private RectangleF CalcBestPlace(ChartLabel label, RectangleF rect)
	{
		RectangleF empty = RectangleF.Empty;
		RectangleF rectangleF = new RectangleF(label.ConnectPoint.X + label.Offset.Width, label.ConnectPoint.Y + label.Offset.Height, label.Size.Width, label.Size.Height);
		if (rect.Contains(rectangleF))
		{
			empty = rectangleF;
		}
		else
		{
			empty = rectangleF;
			if (empty.Left < rect.Left)
			{
				empty.Offset(rect.Left - empty.Left, 0f);
			}
			if (empty.Top < rect.Top)
			{
				empty.Offset(0f, rect.Top - empty.Top);
			}
			if (empty.Right > rect.Right)
			{
				empty.Offset(rect.Right - empty.Right, 0f);
			}
			if (empty.Bottom > rect.Bottom)
			{
				empty.Offset(0f, rect.Bottom - empty.Bottom);
			}
		}
		return empty;
	}

	private bool CheckWithMinSize(RectangleF rect, ArrayList result)
	{
		if (rect.Width < MinimalSize.Width)
		{
			return false;
		}
		if (rect.Height < MinimalSize.Height)
		{
			return false;
		}
		foreach (RectangleF item in result)
		{
			if (item.Contains(rect))
			{
				return false;
			}
		}
		return true;
	}

	private double CalcRadius(PointF pt1, PointF pt2)
	{
		double num = Math.Abs(pt1.X - pt2.X);
		double num2 = Math.Abs(pt1.Y - pt2.Y);
		return Math.Sqrt(num * num + num2 * num2);
	}

	private PointF CalcCenter(RectangleF rect)
	{
		return new PointF(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
	}
}
