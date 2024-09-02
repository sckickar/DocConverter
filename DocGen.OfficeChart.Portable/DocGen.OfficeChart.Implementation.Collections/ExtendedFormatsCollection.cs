using System;
using System.Collections.Generic;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class ExtendedFormatsCollection : CollectionBaseEx<ExtendedFormatImpl>
{
	private const int DEF_DEFAULT_COUNT = 21;

	private Dictionary<ExtendedFormatImpl, ExtendedFormatImpl> m_hashFormats = new Dictionary<ExtendedFormatImpl, ExtendedFormatImpl>();

	public new ExtendedFormatImpl this[int index]
	{
		get
		{
			if (index < 0 || index >= base.Count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			return base.InnerList[index];
		}
	}

	public WorkbookImpl ParentWorkbook => this[0].Workbook;

	public ExtendedFormatsCollection(IApplication application, object parent)
		: base(application, parent)
	{
	}

	public new ExtendedFormatImpl Add(ExtendedFormatImpl format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		bool flag = true;
		if (m_hashFormats.ContainsKey(format))
		{
			ExtendedFormatImpl extendedFormatImpl = format;
			format = m_hashFormats[format];
			if (ParentWorkbook.Version == OfficeVersion.Excel97to2003 && format.Index != 0 && format.Index < 15)
			{
				format = extendedFormatImpl;
			}
			else
			{
				flag = false;
			}
		}
		else
		{
			WorkbookImpl workbookImpl = ((base.Count > 0) ? ParentWorkbook : format.Workbook);
			if (base.Count >= workbookImpl.MaxXFCount)
			{
				throw new ApplicationException("Maximum number of extended formats exceeded.");
			}
			m_hashFormats.Add(format, format);
		}
		int value2;
		if (flag)
		{
			format.Index = (ushort)base.List.Count;
			base.Add(format);
			int value;
			if (!format.Workbook.m_xfCellCount.ContainsKey(format.Index))
			{
				format.Workbook.m_xfCellCount.Add(format.Index, 1);
			}
			else if (format.Workbook.m_xfCellCount.TryGetValue(format.Index, out value))
			{
				format.Workbook.m_xfCellCount[format.Index] = value + 1;
			}
		}
		else if (format.Workbook.m_xfCellCount.ContainsKey(format.Index) && format.Workbook.m_xfCellCount.TryGetValue(format.Index, out value2))
		{
			format.Workbook.m_xfCellCount[format.Index] = value2 + 1;
		}
		return format;
	}

	public ExtendedFormatImpl ForceAdd(ExtendedFormatImpl format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (!m_hashFormats.ContainsKey(format))
		{
			m_hashFormats.Add(format, format);
		}
		format.Index = (ushort)base.List.Count;
		base.Add(format);
		return format;
	}

	public int Import(ExtendedFormatImpl format, Dictionary<int, int> hashExtFormatIndexes)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (hashExtFormatIndexes == null)
		{
			throw new ArgumentNullException("hashExtFormatIndexes");
		}
		int index = format.Index;
		if (base.Count > index && this[index] == format)
		{
			return index;
		}
		ExtendedFormatImpl extendedFormatImpl = format.TypedClone(this);
		int num = extendedFormatImpl.ParentIndex;
		if (hashExtFormatIndexes.ContainsKey(num))
		{
			num = hashExtFormatIndexes[num];
		}
		extendedFormatImpl.ParentIndex = num;
		extendedFormatImpl = Add(extendedFormatImpl);
		return extendedFormatImpl.Index;
	}

	public Dictionary<int, int> Merge(IList<ExtendedFormatImpl> arrXFormats, out Dictionary<int, int> dicFontIndexes)
	{
		if (arrXFormats == null)
		{
			throw new ArgumentNullException("arrXFormats");
		}
		dicFontIndexes = null;
		if (arrXFormats == this)
		{
			return null;
		}
		int count = arrXFormats.Count;
		if (count == 0)
		{
			return null;
		}
		_ = new int[count];
		_ = new bool[count];
		WorkbookImpl workbook = arrXFormats[0].Workbook;
		if (!(FindParent(typeof(WorkbookImpl)) is WorkbookImpl workbookImpl))
		{
			throw new ArgumentNullException("Can't find destination workbook.");
		}
		dicFontIndexes = workbookImpl.InnerFonts.AddRange(workbook.InnerFonts);
		Dictionary<int, int> dicFormatIndexes = workbookImpl.InnerFormats.Merge(workbook.InnerFormats);
		return Merge(arrXFormats, dicFontIndexes, dicFormatIndexes);
	}

	public Dictionary<int, int> Merge(IList<ExtendedFormatImpl> arrXFormats)
	{
		if (arrXFormats == null)
		{
			throw new ArgumentNullException("arrXFormats");
		}
		Dictionary<int, int> fontIndexes = GetFontIndexes(arrXFormats);
		Dictionary<int, int> formatIndexes = GetFormatIndexes(arrXFormats);
		return Merge(arrXFormats, fontIndexes, formatIndexes);
	}

	public void AddIndex(Dictionary<int, object> hashToAdd, IList<ExtendedFormatImpl> arrXFormats, int index)
	{
		if (hashToAdd == null)
		{
			throw new ArgumentNullException("hashToAdd");
		}
		if (arrXFormats == null)
		{
			throw new ArgumentNullException("arrXFormats");
		}
		if (!hashToAdd.ContainsKey(index))
		{
			hashToAdd.Add(index, null);
			ExtendedFormatImpl extendedFormatImpl = this[index];
			arrXFormats.Add(extendedFormatImpl);
			if (extendedFormatImpl.HasParent)
			{
				AddIndex(hashToAdd, arrXFormats, extendedFormatImpl.ParentIndex);
			}
		}
	}

	public ExtendedFormatImpl GatherTwoFormats(int iFirstXF, int iEndXF)
	{
		if (iFirstXF >= base.Count || iFirstXF < 0)
		{
			throw new ArgumentOutOfRangeException("iFirstXF");
		}
		if (iEndXF >= base.Count || iEndXF < 0)
		{
			throw new ArgumentOutOfRangeException("iEndXF");
		}
		if (iFirstXF == iEndXF)
		{
			return (ExtendedFormatImpl)this[iFirstXF].Clone();
		}
		ExtendedFormatImpl extendedFormatImpl = this[iFirstXF];
		ExtendedFormatImpl extendedFormatImpl2 = this[iEndXF];
		ExtendedFormatImpl obj = (ExtendedFormatImpl)extendedFormatImpl.Clone();
		IBorder border;
		IBorder border2;
		if (extendedFormatImpl2.IncludeBorder)
		{
			border = extendedFormatImpl2.Borders[OfficeBordersIndex.EdgeRight];
			border2 = extendedFormatImpl2.Borders[OfficeBordersIndex.EdgeBottom];
		}
		else
		{
			ExtendedFormatImpl extendedFormatImpl3 = this[extendedFormatImpl2.ParentIndex];
			border = extendedFormatImpl3.Borders[OfficeBordersIndex.EdgeRight];
			border2 = extendedFormatImpl3.Borders[OfficeBordersIndex.EdgeBottom];
		}
		IBorder border3 = obj.Borders[OfficeBordersIndex.EdgeRight];
		border3.ColorObject.CopyFrom(border.ColorObject, callEvent: true);
		border3.LineStyle = border.LineStyle;
		IBorder border4 = obj.Borders[OfficeBordersIndex.EdgeBottom];
		border4.ColorObject.CopyFrom(border2.ColorObject, callEvent: true);
		border4.LineStyle = border2.LineStyle;
		return obj;
	}

	public new Dictionary<int, int> RemoveAt(int xfIndex)
	{
		int count = base.Count;
		if (xfIndex < 0 || xfIndex > count)
		{
			return null;
		}
		SortedList<int, ExtendedFormatImpl> sortedList = new SortedList<int, ExtendedFormatImpl>();
		ExtendedFormatImpl key = this[xfIndex];
		m_hashFormats.Remove(key);
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		base.InnerList[xfIndex] = null;
		dictionary[xfIndex] = 0;
		for (int i = 0; i < count; i++)
		{
			key = this[i];
			if (key != null && key.ParentIndex == xfIndex)
			{
				m_hashFormats.Remove(key);
				key.ParentIndex = 0;
				key.SynchronizeWithParent();
				sortedList.Add(i, null);
			}
		}
		int count2 = sortedList.Count;
		IList<int> keys = sortedList.Keys;
		for (int j = 0; j < count2; j++)
		{
			int num = keys[j];
			key = this[num];
			if (key != null)
			{
				if (m_hashFormats.TryGetValue(key, out var value))
				{
					sortedList[num] = value;
					base.InnerList[num] = null;
				}
				else
				{
					m_hashFormats.Add(key, key);
					sortedList[num] = key;
				}
			}
		}
		count = base.Count;
		int num2 = 0;
		for (int k = 0; k < count; k++)
		{
			key = this[k];
			if (key != null)
			{
				int num3 = k - num2;
				dictionary.Add(k, num3);
				key.Index = (ushort)num3;
			}
			else
			{
				num2++;
			}
		}
		keys = sortedList.Keys;
		for (int l = 0; l < count2; l++)
		{
			int key2 = keys[l];
			key = sortedList[key2];
			dictionary[key2] = key.Index;
		}
		for (int m = 0; m < count; m++)
		{
			key = this[m];
			if (key != null)
			{
				int parentIndex = key.ParentIndex;
				if (parentIndex != 4095 && dictionary.ContainsKey(m))
				{
					key.ParentIndex = (ushort)dictionary[parentIndex];
				}
			}
		}
		for (int n = 0; n < count; n++)
		{
			key = this[n];
			if (key == null)
			{
				base.InnerList.RemoveAt(n);
				count--;
				n--;
			}
		}
		return dictionary;
	}

	public override object Clone(object parent)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		ExtendedFormatsCollection extendedFormatsCollection = (ExtendedFormatsCollection)base.Clone(parent);
		List<ExtendedFormatImpl> innerList = extendedFormatsCollection.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			ExtendedFormatImpl extendedFormatImpl = innerList[i];
			if (!m_hashFormats.ContainsKey(extendedFormatImpl))
			{
				m_hashFormats.Add(extendedFormatImpl, extendedFormatImpl);
			}
			extendedFormatImpl.Index = i;
		}
		return extendedFormatsCollection;
	}

	public void SetMaxCount(int maxCount)
	{
		if (base.Count <= 0)
		{
			return;
		}
		WorkbookImpl workbook = this[0].Workbook;
		int maxXFCount = workbook.MaxXFCount;
		List<ExtendedFormatImpl> innerList = base.InnerList;
		int count = innerList.Count;
		if (count >= maxCount)
		{
			innerList.RemoveRange(maxCount - 1, count - maxCount);
		}
		int i = 0;
		for (int count2 = base.Count; i < count2; i++)
		{
			ExtendedFormatImpl extendedFormatImpl = innerList[i];
			if (extendedFormatImpl.ParentIndex == maxXFCount)
			{
				extendedFormatImpl.ParentIndex = maxCount;
			}
		}
		workbook.UpdateXFIndexes(maxCount);
	}

	internal void SetXF(int iXFIndex, ExtendedFormatImpl format)
	{
		if (iXFIndex >= base.Count)
		{
			throw new ArgumentOutOfRangeException("iXFIndex");
		}
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		ExtendedFormatImpl key = this[iXFIndex];
		m_hashFormats.Remove(key);
		base.InnerList[iXFIndex] = format;
		m_hashFormats.Add(format, format);
	}

	private Dictionary<int, int> Merge(IList<ExtendedFormatImpl> arrXFormats, Dictionary<int, int> dicFontIndexes, Dictionary<int, int> dicFormatIndexes)
	{
		if (arrXFormats == null)
		{
			throw new ArgumentNullException("arrXFormats");
		}
		if (dicFontIndexes == null)
		{
			throw new ArgumentNullException("dicFontIndexes");
		}
		if (dicFormatIndexes == null)
		{
			throw new ArgumentNullException("dicFormatIndexes");
		}
		int count = arrXFormats.Count;
		Dictionary<int, int> dictionary = new Dictionary<int, int>(count);
		for (int i = 0; i < count; i++)
		{
			ExtendedFormatImpl extendedFormatImpl = arrXFormats[i];
			int index = extendedFormatImpl.Index;
			if (!dictionary.ContainsKey(index))
			{
				Merge(extendedFormatImpl, dictionary, dicFontIndexes, dicFormatIndexes);
			}
		}
		return dictionary;
	}

	private void Merge(ExtendedFormatImpl format, Dictionary<int, int> hashResult, Dictionary<int, int> dicFontIndexes, Dictionary<int, int> dicFormatIndexes)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (hashResult == null)
		{
			throw new ArgumentNullException("hashResult");
		}
		ExtendedFormatsCollection parentCollection = format.ParentCollection;
		int num = format.ParentIndex;
		if (format.HasParent)
		{
			if (!hashResult.ContainsKey(num))
			{
				Merge(parentCollection[num], hashResult, dicFontIndexes, dicFormatIndexes);
			}
			num = hashResult[num];
		}
		int index = format.Index;
		ExtendedFormatImpl extendedFormatImpl = format.TypedClone(this);
		int formatIndex = format.Record.FormatIndex;
		int num2 = format.Record.FontIndex;
		if (format.HasParent)
		{
			extendedFormatImpl.ParentIndex = num;
		}
		if (num2 >= dicFontIndexes.Count)
		{
			num2 = 0;
		}
		extendedFormatImpl.Record.FontIndex = (ushort)dicFontIndexes[num2];
		if (dicFormatIndexes != null && dicFormatIndexes.ContainsKey(formatIndex))
		{
			formatIndex = dicFormatIndexes[formatIndex];
			extendedFormatImpl.Record.FormatIndex = (ushort)formatIndex;
		}
		extendedFormatImpl = Add(extendedFormatImpl);
		hashResult.Add(index, extendedFormatImpl.Index);
	}

	private Dictionary<int, int> GetFontIndexes(IList<ExtendedFormatImpl> arrXFormats)
	{
		if (arrXFormats == null)
		{
			throw new ArgumentNullException("arrXFormats");
		}
		Dictionary<int, object> dictionary = new Dictionary<int, object>();
		int i = 0;
		for (int count = arrXFormats.Count; i < count; i++)
		{
			ExtendedFormatImpl extendedFormatImpl = arrXFormats[i];
			dictionary[extendedFormatImpl.FontIndex] = -1;
		}
		WorkbookImpl workbook = arrXFormats[0].Workbook;
		return ((WorkbookImpl)FindParent(typeof(WorkbookImpl))).InnerFonts.AddRange(dictionary.Keys, workbook.InnerFonts);
	}

	private Dictionary<int, int> GetFormatIndexes(IList<ExtendedFormatImpl> arrXFormats)
	{
		if (arrXFormats == null)
		{
			throw new ArgumentNullException("arrXFormats");
		}
		int count = arrXFormats.Count;
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		if (count == 0)
		{
			return dictionary;
		}
		int i = 0;
		for (int count2 = arrXFormats.Count; i < count2; i++)
		{
			ExtendedFormatImpl extendedFormatImpl = arrXFormats[i];
			dictionary[extendedFormatImpl.NumberFormatIndex] = -1;
		}
		WorkbookImpl workbook = arrXFormats[0].Workbook;
		return ((WorkbookImpl)FindParent(typeof(WorkbookImpl))).InnerFormats.AddRange(dictionary, workbook.InnerFormats);
	}

	protected override void OnClearComplete()
	{
		m_hashFormats.Clear();
	}

	internal void Dispose()
	{
		foreach (ExtendedFormatImpl inner in base.InnerList)
		{
			inner.clearAll();
		}
		base.InnerList.Clear();
	}
}
