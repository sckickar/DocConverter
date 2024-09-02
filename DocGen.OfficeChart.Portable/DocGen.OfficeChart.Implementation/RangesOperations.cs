using System;
using System.Collections.Generic;
using DocGen.Drawing;

namespace DocGen.OfficeChart.Implementation;

internal class RangesOperations
{
	private delegate int SortKeyGetter(Rectangle rect);

	private delegate IList<Rectangle> CombineRectangles(IList<Rectangle> lstRects);

	private const int DEF_MAXIMUM_SPLIT_COUNT = 4;

	private List<Rectangle> m_arrCells;

	public virtual List<Rectangle> CellList
	{
		get
		{
			return m_arrCells;
		}
		set
		{
			m_arrCells = value;
		}
	}

	public RangesOperations()
		: this(new List<Rectangle>())
	{
	}

	public RangesOperations(List<Rectangle> arrCells)
	{
		m_arrCells = arrCells;
	}

	public bool Contains(Rectangle[] arrRanges)
	{
		return Contains(arrRanges, 0);
	}

	public bool Contains(Rectangle[] arrRanges, int iStartIndex)
	{
		if (arrRanges == null)
		{
			return true;
		}
		int num = arrRanges.Length;
		if (num == 0)
		{
			return true;
		}
		if (iStartIndex < 0)
		{
			iStartIndex = 0;
		}
		for (int i = 0; i < num; i++)
		{
			if (!Contains(arrRanges[i], iStartIndex))
			{
				return false;
			}
		}
		return true;
	}

	public bool Contains(IList<Rectangle> arrRanges)
	{
		return Contains(arrRanges, 0);
	}

	public bool Contains(IList<Rectangle> arrRanges, int iStartIndex)
	{
		if (arrRanges == null)
		{
			return true;
		}
		int count = arrRanges.Count;
		if (count == 0)
		{
			return true;
		}
		if (iStartIndex < 0)
		{
			iStartIndex = 0;
		}
		for (int i = 0; i < count; i++)
		{
			if (!Contains(arrRanges[i], iStartIndex))
			{
				return false;
			}
		}
		return true;
	}

	public bool Contains(Rectangle range)
	{
		return Contains(range, 0);
	}

	public bool Contains(Rectangle range, int iStartIndex)
	{
		List<Rectangle> cellList = CellList;
		int i = iStartIndex;
		for (int count = cellList.Count; i < count; i++)
		{
			Rectangle rectRemove = cellList[i];
			IList<Rectangle> list = SplitRectangle(range, rectRemove);
			if (list != null)
			{
				return Contains(list, i);
			}
		}
		return false;
	}

	public int ContainsCount(Rectangle range)
	{
		List<Rectangle> cellList = CellList;
		int num = 0;
		int i = 0;
		for (int count = cellList.Count; i < count; i++)
		{
			Rectangle rectRemove = cellList[i];
			if (SplitRectangle(range, rectRemove) != null)
			{
				num++;
			}
		}
		return num;
	}

	public void AddCells(IList<Rectangle> arrCells)
	{
		if (arrCells != null)
		{
			int i = 0;
			for (int count = arrCells.Count; i < count; i++)
			{
				Rectangle rect = arrCells[i];
				AddRange(rect);
			}
		}
	}

	public void AddRectangles(IList<Rectangle> arrCells)
	{
		if (arrCells != null)
		{
			int i = 0;
			for (int count = arrCells.Count; i < count; i++)
			{
				Rectangle rect = arrCells[i];
				AddRange(rect);
			}
		}
	}

	public void AddRange(IRange range)
	{
		if (range == null)
		{
			throw new ArgumentNullException("range");
		}
		Rectangle[] rectangles = ((ICombinedRange)range).GetRectangles();
		int i = 0;
		for (int num = rectangles.Length; i < num; i++)
		{
			AddRange(rectangles[i]);
		}
	}

	public void AddRange(Rectangle rect)
	{
		List<Rectangle> cellList = CellList;
		int i = 0;
		for (int count = cellList.Count; i < count / 2; i += 2)
		{
			Rectangle curRange = cellList[i];
			if (CheckAndAddRange(ref curRange, cellList[i + 1]))
			{
				cellList.RemoveAt(i);
				cellList.RemoveAt(i);
				cellList.Insert(i, curRange);
			}
		}
		int j = 0;
		for (int count2 = cellList.Count; j < count2; j++)
		{
			Rectangle curRange2 = cellList[j];
			if (CheckAndAddRange(ref curRange2, rect))
			{
				cellList[j] = curRange2;
				return;
			}
			if (curRange2.Contains(rect))
			{
				return;
			}
		}
		cellList.Add(rect);
	}

	public void Clear()
	{
		m_arrCells.Clear();
	}

	public RangesOperations GetPart(Rectangle rect, bool remove, int rowIncrement, int columnIncrement)
	{
		RangesOperations rangesOperations = new RangesOperations();
		int i = 0;
		for (int count = m_arrCells.Count; i < count; i++)
		{
			Rectangle rect2 = m_arrCells[i];
			if (UtilityMethods.Intersects(rect2, rect))
			{
				int left = Math.Max(rect2.X, rect.X);
				int top = Math.Max(rect2.Y, rect.Y);
				int right = Math.Min(rect2.Right, rect.Right);
				int bottom = Math.Min(rect2.Bottom, rect.Bottom);
				Rectangle rect3 = Rectangle.FromLTRB(left, top, right, bottom);
				rect3.Offset(columnIncrement, rowIncrement);
				rangesOperations.AddRange(rect3);
			}
		}
		if (remove)
		{
			Remove(rect);
		}
		if (rangesOperations.m_arrCells.Count <= 0)
		{
			return null;
		}
		return rangesOperations;
	}

	private bool CheckAndAddRange(ref Rectangle curRange, Rectangle rangeToAdd)
	{
		bool num = curRange.Left == rangeToAdd.Left && curRange.Right == rangeToAdd.Right;
		bool flag = false;
		if (num)
		{
			if (curRange.Bottom == rangeToAdd.Top - 1)
			{
				curRange.Height = rangeToAdd.Bottom - curRange.Top;
				flag = true;
			}
			else if (rangeToAdd.Bottom == curRange.Top - 1)
			{
				curRange.Height = curRange.Top - rangeToAdd.Bottom + curRange.Height + rangeToAdd.Height;
				curRange.Y = rangeToAdd.Y;
				flag = true;
			}
			else if (curRange.Top == rangeToAdd.Bottom + 1)
			{
				curRange.Y = rangeToAdd.Top;
				flag = true;
			}
		}
		if (!flag && curRange.Top == rangeToAdd.Top && curRange.Bottom == rangeToAdd.Bottom)
		{
			if (curRange.Right == rangeToAdd.Left - 1)
			{
				curRange.Width = rangeToAdd.Right - curRange.Left;
				flag = true;
			}
			else if (rangeToAdd.Right == curRange.Left - 1)
			{
				curRange.Width = curRange.Left - rangeToAdd.Right + curRange.Width + rangeToAdd.Width;
				curRange.X = rangeToAdd.X;
				flag = true;
			}
			else if (curRange.Left == rangeToAdd.Right + 1)
			{
				curRange.X = rangeToAdd.Left;
				flag = true;
			}
		}
		return flag;
	}

	public void Remove(Rectangle[] arrRanges)
	{
		if (arrRanges != null)
		{
			int num = arrRanges.Length;
			for (int i = 0; i < num; i++)
			{
				Rectangle rect = arrRanges[i];
				Remove(rect);
			}
		}
	}

	public RangesOperations Clone()
	{
		RangesOperations rangesOperations = (RangesOperations)MemberwiseClone();
		rangesOperations.m_arrCells = new List<Rectangle>();
		int i = 0;
		for (int count = m_arrCells.Count; i < count; i++)
		{
			rangesOperations.m_arrCells.Add(m_arrCells[i]);
		}
		return rangesOperations;
	}

	private int Remove(Rectangle rect)
	{
		List<Rectangle> cellList = CellList;
		int num = 0;
		int count = cellList.Count;
		List<Rectangle> list = null;
		int num2 = count;
		for (int i = 0; i < num2; i++)
		{
			Rectangle rectangle = cellList[i];
			if (!UtilityMethods.Intersects(rectangle, rect))
			{
				continue;
			}
			int index = num2 - 1;
			Rectangle value = cellList[index];
			cellList[index] = cellList[i];
			cellList[i] = value;
			num++;
			num2--;
			i--;
			IList<Rectangle> list2 = SplitRectangle(rectangle, rect);
			if (list2 != null)
			{
				if (list == null)
				{
					list = new List<Rectangle>(list2);
				}
				else
				{
					list.AddRange(list2);
				}
			}
		}
		if (num > 0)
		{
			cellList.RemoveRange(count - num, num);
		}
		if (list != null)
		{
			AddRectangles(list);
		}
		return num2;
	}

	private IList<Rectangle> SplitRectangle(Rectangle rectSource, Rectangle rectRemove)
	{
		if (UtilityMethods.Intersects(rectRemove, rectSource))
		{
			rectRemove.Intersect(rectSource);
			List<Rectangle> list = new List<Rectangle>(4);
			if (rectSource.Top < rectRemove.Top)
			{
				Rectangle item = Rectangle.FromLTRB(rectSource.Left, rectSource.Top, rectSource.Right, rectRemove.Top - 1);
				list.Add(item);
			}
			if (rectSource.Bottom > rectRemove.Bottom)
			{
				Rectangle item2 = Rectangle.FromLTRB(rectSource.Left, rectRemove.Bottom + 1, rectSource.Right, rectSource.Bottom);
				list.Add(item2);
			}
			if (rectSource.Left < rectRemove.Left)
			{
				Rectangle item3 = Rectangle.FromLTRB(rectSource.Left, rectRemove.Top, rectRemove.Left - 1, rectRemove.Bottom);
				list.Add(item3);
			}
			if (rectSource.Right > rectRemove.Right)
			{
				Rectangle item4 = Rectangle.FromLTRB(rectRemove.Right + 1, rectRemove.Top, rectSource.Right, rectRemove.Bottom);
				list.Add(item4);
			}
			return list;
		}
		return null;
	}

	public void OptimizeStorage()
	{
		SortAndTryAdd(TopValueGetter, LeftValueGetter, CombineSameRowRectangles);
		SortAndTryAdd(LeftValueGetter, TopValueGetter, CombineSameColumnRectangles);
	}

	private void SortAndTryAdd(SortKeyGetter topLevelKeyGetter, SortKeyGetter lowLevelKeyGetter, CombineRectangles combine)
	{
		if (m_arrCells.Count > 1)
		{
			int count = m_arrCells.Count;
			int num;
			do
			{
				num = count;
				Dictionary<int, SortedList<int, Rectangle>> dictionary = SortBy(topLevelKeyGetter, lowLevelKeyGetter);
				Clear();
				OptimizeAndAdd(dictionary, combine);
				count = m_arrCells.Count;
			}
			while (num != count);
		}
	}

	private void OptimizeAndAdd(Dictionary<int, SortedList<int, Rectangle>> dictionary, CombineRectangles combine)
	{
		foreach (int key in dictionary.Keys)
		{
			IList<Rectangle> values = dictionary[key].Values;
			values = combine(values);
			int i = 0;
			for (int count = values.Count; i < count; i++)
			{
				AddRange(values[i]);
			}
		}
	}

	private int TopValueGetter(Rectangle rect)
	{
		return rect.Top;
	}

	private int LeftValueGetter(Rectangle rect)
	{
		return rect.Left;
	}

	private Dictionary<int, SortedList<int, Rectangle>> SortBy(SortKeyGetter keyGetter, SortKeyGetter secondLevelKeyGetter)
	{
		Dictionary<int, SortedList<int, Rectangle>> dictionary = new Dictionary<int, SortedList<int, Rectangle>>();
		int i = 0;
		for (int count = m_arrCells.Count; i < count; i++)
		{
			Rectangle rectangle = m_arrCells[i];
			int key = keyGetter(rectangle);
			if (!dictionary.TryGetValue(key, out var value))
			{
				value = new SortedList<int, Rectangle>();
				dictionary.Add(key, value);
			}
			if (!value.ContainsKey(secondLevelKeyGetter(rectangle)))
			{
				value.Add(secondLevelKeyGetter(rectangle), rectangle);
			}
		}
		return dictionary;
	}

	private IList<Rectangle> CombineSameRowRectangles(IList<Rectangle> lstRects)
	{
		if (lstRects == null || lstRects.Count == 0)
		{
			return lstRects;
		}
		List<Rectangle> list = new List<Rectangle>();
		list.Add(lstRects[0]);
		int i = 1;
		for (int count = lstRects.Count; i < count; i++)
		{
			int index = list.Count - 1;
			Rectangle rectangle = list[index];
			Rectangle item = lstRects[i];
			if (rectangle.Top == item.Top && rectangle.Bottom == item.Bottom && rectangle.Right + 1 == item.Left)
			{
				rectangle = Rectangle.FromLTRB(rectangle.Left, rectangle.Top, item.Right, rectangle.Bottom);
				list[index] = rectangle;
			}
			else
			{
				list.Add(item);
			}
		}
		return list;
	}

	private IList<Rectangle> CombineSameColumnRectangles(IList<Rectangle> lstRects)
	{
		if (lstRects == null || lstRects.Count == 0)
		{
			return lstRects;
		}
		List<Rectangle> list = new List<Rectangle>();
		list.Add(lstRects[0]);
		int i = 1;
		for (int count = lstRects.Count; i < count; i++)
		{
			int index = list.Count - 1;
			Rectangle rectangle = list[index];
			Rectangle item = lstRects[i];
			if (rectangle.Left == item.Left && rectangle.Right == item.Right && rectangle.Bottom + 1 == item.Top)
			{
				rectangle = Rectangle.FromLTRB(rectangle.Left, rectangle.Top, item.Right, item.Bottom);
				list[index] = rectangle;
			}
			else
			{
				list.Add(item);
			}
		}
		return list;
	}

	internal void Offset(int iRowDelta, int iColumnDelta, WorkbookImpl book)
	{
		int i = 0;
		for (int count = m_arrCells.Count; i < count; i++)
		{
			Rectangle value = m_arrCells[i];
			value.Offset(iColumnDelta, iRowDelta);
			if (value.Y >= book.MaxRowCount)
			{
				value.Y = book.MaxRowCount - 1;
			}
			if (value.Bottom >= book.MaxRowCount)
			{
				value.Height -= value.Bottom - book.MaxRowCount + 1;
			}
			if (value.X >= book.MaxColumnCount)
			{
				value.X = book.MaxColumnCount - 1;
			}
			if (value.Right >= book.MaxColumnCount)
			{
				value.Width -= value.Right - book.MaxColumnCount + 1;
			}
			m_arrCells[i] = value;
		}
	}

	public void SetLength(int maxLength)
	{
		if (m_arrCells.Count > maxLength)
		{
			m_arrCells.RemoveRange(maxLength, m_arrCells.Count - maxLength);
		}
	}
}
