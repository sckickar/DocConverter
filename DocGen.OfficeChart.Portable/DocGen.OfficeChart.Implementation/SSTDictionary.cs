using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation;

internal class SSTDictionary : IParseable, IDisposable
{
	private const int DEF_RESERVE_SPACE = 20;

	public const int DEF_EMPTY_STRING_INDEX = -1;

	private const int DEF_EMPTY_COUNT = 2;

	private const int MaxCellLength = 32767;

	private Dictionary<object, int> m_hashKeyToIndex = new Dictionary<object, int>(20);

	private List<object> m_arrStrings = new List<object>(20);

	private SortedList<int, int> m_arrFreeIndexes = new SortedList<int, int>(20);

	private WorkbookImpl m_book;

	private SSTRecord m_sstOriginal;

	private bool m_bParsed = true;

	private TextWithFormat m_tempString = new TextWithFormat(0);

	private List<int> newRefCount;

	private bool m_bUseHash = true;

	public int this[TextWithFormat key]
	{
		get
		{
			Parse();
			return Find(key);
		}
	}

	public TextWithFormat this[int index]
	{
		get
		{
			object sSTContentByIndex = GetSSTContentByIndex(index);
			TextWithFormat textWithFormat = sSTContentByIndex as TextWithFormat;
			if (textWithFormat == null)
			{
				m_tempString.Text = sSTContentByIndex.ToString();
				textWithFormat = m_tempString;
			}
			return textWithFormat;
		}
	}

	public object[] Keys
	{
		get
		{
			int count = Count;
			object[] array = new object[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = m_arrStrings[i];
			}
			return array;
		}
	}

	public int Count
	{
		get
		{
			if (!m_bParsed)
			{
				return (int)m_sstOriginal.NumberOfUniqueStrings;
			}
			return m_arrStrings.Count;
		}
	}

	public WorkbookImpl Workbook => m_book;

	[CLSCompliant(false)]
	public SSTRecord OriginalSST
	{
		get
		{
			return m_sstOriginal;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("OriginalSST");
			}
			m_bParsed = false;
			m_sstOriginal = value;
			_ = m_sstOriginal.NumberOfUniqueStrings;
		}
	}

	public bool UseHashForSearching
	{
		get
		{
			return m_bUseHash;
		}
		set
		{
			if (m_bUseHash != value)
			{
				m_bUseHash = value;
				if (!value)
				{
					m_hashKeyToIndex.Clear();
				}
				else if (m_bParsed)
				{
					FillHash();
				}
			}
		}
	}

	public int ActiveCount
	{
		get
		{
			if (!m_bParsed)
			{
				return (int)m_sstOriginal.NumberOfUniqueStrings;
			}
			return m_arrStrings.Count - m_arrFreeIndexes.Count;
		}
	}

	public SSTDictionary(WorkbookImpl book)
	{
		m_book = book;
		newRefCount = new List<int>();
	}

	public object GetSSTContentByIndex(int index)
	{
		if (index < 0 || index >= Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (!m_bParsed)
		{
			return m_sstOriginal.Strings[index];
		}
		return m_arrStrings[index];
	}

	public void Clear()
	{
		if (m_book != null)
		{
			if (m_bParsed)
			{
				m_arrStrings.Clear();
				m_hashKeyToIndex.Clear();
			}
			else
			{
				m_sstOriginal = null;
				m_bParsed = true;
			}
		}
	}

	public Dictionary<int, object> GetStringIndexes(string value)
	{
		Dictionary<int, object> dictionary = new Dictionary<int, object>();
		object[] array = (m_bParsed ? null : m_sstOriginal.Strings);
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			if (!m_arrFreeIndexes.ContainsKey(i))
			{
				object obj = (m_bParsed ? m_arrStrings[i] : array[i]);
				if (((obj is TextWithFormat textWithFormat) ? textWithFormat.Text : ((string)obj)).IndexOf(value, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
				{
					dictionary.Add(i, null);
				}
			}
		}
		return dictionary;
	}

	public void AddIncrease(int index)
	{
		if (index != -1)
		{
			int refCount = GetRefCount(index);
			SetRefCount(index, refCount + 1);
		}
	}

	public int AddIncrease(object key)
	{
		return AddIncrease(key, bIncrease: true);
	}

	public int AddIncrease(object key, bool bIncrease)
	{
		if (bIncrease)
		{
			Parse();
		}
		if (m_hashKeyToIndex.TryGetValue(key, out var value))
		{
			value = m_hashKeyToIndex[key];
			if (bIncrease)
			{
				AddIncrease(value);
			}
			else
			{
				m_arrStrings.Add(key);
			}
		}
		else
		{
			CheckLength(key);
			int count = (bIncrease ? 1 : 0);
			if (m_arrFreeIndexes.Count == 0)
			{
				value = m_arrStrings.Count;
				m_arrStrings.Add(key);
				if (m_bUseHash)
				{
					m_hashKeyToIndex[key] = value;
				}
				if (bIncrease)
				{
					SetRefCount(value, count);
				}
			}
			else
			{
				value = m_arrFreeIndexes.Values[0];
				m_arrStrings[value] = key;
				if (m_bUseHash)
				{
					m_hashKeyToIndex[key] = value;
				}
				m_arrFreeIndexes.RemoveAt(0);
				SetRefCount(value, count);
			}
		}
		return value;
	}

	private void CheckLength(object key)
	{
		string text = key as string;
		int num = 0;
		if (text != null)
		{
			num = text.Length;
		}
		else if (key != null)
		{
			num = (key as TextWithFormat).Text.Length;
		}
		if (m_book.Version == OfficeVersion.Excel97to2003 && num > 32767)
		{
			throw new ArgumentOutOfRangeException("Text length cannot be more than " + 32767);
		}
	}

	public void RemoveDecrease(object key)
	{
		Parse();
		int num = Find(key);
		if (num == -1)
		{
			throw new ArgumentException("Dictionary does not contain specified string: '" + key?.ToString() + "'");
		}
		RemoveDecrease(num);
	}

	public void RemoveDecrease(int iIndex)
	{
		if (iIndex < 0 || iIndex >= Count)
		{
			throw new ArgumentOutOfRangeException("iIndex");
		}
		int refCount = GetRefCount(iIndex);
		refCount--;
		SetRefCount(iIndex, refCount);
		if (refCount <= 0)
		{
			Parse();
			if (m_bUseHash)
			{
				m_hashKeyToIndex.Remove(m_arrStrings[iIndex]);
			}
			m_arrFreeIndexes[iIndex] = iIndex;
			m_arrStrings[iIndex] = 0;
		}
	}

	public void DecreaseOnly(int index)
	{
		if (index < 0 || index > m_arrStrings.Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		int refCount = GetRefCount(index);
		SetRefCount(index, refCount - 1);
	}

	public bool Contains(object key)
	{
		Parse();
		return Find(key) != -1;
	}

	[CLSCompliant(false)]
	public void Serialize(OffsetArrayList records)
	{
		if (m_bParsed)
		{
			Defragment();
		}
		SaveIntoRecords(records);
	}

	public int GetStringCount(int index)
	{
		if (index == -1)
		{
			return 2;
		}
		Parse();
		return GetRefCount(index);
	}

	public TextWithFormat GetStringByIndex(int index)
	{
		if (index == -1)
		{
			throw new NotImplementedException();
		}
		return this[index];
	}

	public int AddCopy(int index, SSTDictionary sourceSST, Dictionary<int, int> dicFontIndexes)
	{
		if (sourceSST == null)
		{
			throw new ArgumentNullException("sourceSST");
		}
		Parse();
		sourceSST.Parse();
		object obj = sourceSST.m_arrStrings[index];
		if (obj is TextWithFormat textWithFormat)
		{
			obj = textWithFormat.Clone(dicFontIndexes);
		}
		return AddIncrease(obj, bIncrease: true);
	}

	public List<int> StartWith(string strStart)
	{
		if (strStart == null)
		{
			throw new ArgumentNullException("strStart");
		}
		if (strStart.Length == 0)
		{
			throw new ArgumentException("strStart - string cannot be empty.");
		}
		List<int> list = new List<int>();
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			if (this[i].Text.StartsWith(strStart))
			{
				list.Add(i);
			}
		}
		return list;
	}

	public object Clone(WorkbookImpl book)
	{
		SSTDictionary sSTDictionary = (SSTDictionary)MemberwiseClone();
		sSTDictionary.m_book = book;
		sSTDictionary.m_hashKeyToIndex = CloneUtils.CloneHash(m_hashKeyToIndex);
		sSTDictionary.m_arrStrings = CloneUtils.CloneCloneable(m_arrStrings);
		if (m_arrFreeIndexes != null)
		{
			sSTDictionary.m_arrFreeIndexes = new SortedList<int, int>(m_arrFreeIndexes);
		}
		sSTDictionary.m_sstOriginal = (SSTRecord)CloneUtils.CloneCloneable(m_sstOriginal);
		sSTDictionary.newRefCount = CloneUtils.CloneCloneable(newRefCount);
		return sSTDictionary;
	}

	public void UpdateRefCounts()
	{
		int count = Count;
		for (int i = 0; i < count; i++)
		{
			newRefCount.Add(0);
		}
	}

	public void RemoveUnnecessaryStrings()
	{
		Parse();
		m_book.ReAddAllStrings();
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			if (GetRefCount(i) != 0)
			{
				continue;
			}
			object obj = m_arrStrings[i];
			if (obj != null)
			{
				if (m_bUseHash)
				{
					m_hashKeyToIndex.Remove(obj);
				}
				m_arrStrings[i] = null;
				m_arrFreeIndexes[i] = i;
			}
		}
		Defragment();
	}

	private void MoveStrings(int iStartIndex, int iEndIndex, int iDecreaseValue, List<int> arrNewIndexes)
	{
		for (int i = iStartIndex + 1; i < iEndIndex; i++)
		{
			object obj = m_arrStrings[i];
			int num = i - iDecreaseValue;
			m_arrStrings[num] = obj;
			if (m_bUseHash)
			{
				m_hashKeyToIndex[obj] = num;
			}
			arrNewIndexes[i] = num;
		}
	}

	private void Defragment()
	{
		int count = Count;
		int count2 = m_arrFreeIndexes.Count;
		if (count2 > 0)
		{
			int iStartIndex = m_arrFreeIndexes.Values[0];
			List<int> list = new List<int>(count + 1);
			for (int i = 0; i < count; i++)
			{
				list.Add(i);
			}
			IList<int> values = m_arrFreeIndexes.Values;
			int j = 1;
			for (int count3 = m_arrFreeIndexes.Count; j < count3; j++)
			{
				int num = values[j];
				MoveStrings(iStartIndex, num, j, list);
				iStartIndex = num;
			}
			MoveStrings(iStartIndex, count, m_arrFreeIndexes.Count, list);
			iStartIndex = count - count2;
			m_arrStrings.RemoveRange(iStartIndex, count2);
			m_arrFreeIndexes.Clear();
			m_book.UpdateStringIndexes(list);
		}
	}

	private void SaveIntoRecords(OffsetArrayList records)
	{
		SSTRecord sSTRecord;
		int num;
		if (m_bParsed)
		{
			sSTRecord = (SSTRecord)BiffRecordFactory.GetRecord(TBIFFRecord.SST);
			sSTRecord.Strings = Keys;
			num = (int)(sSTRecord.NumberOfStrings = (uint)m_arrStrings.Count);
		}
		else
		{
			sSTRecord = m_sstOriginal;
			num = (int)sSTRecord.NumberOfStrings;
		}
		records.Add(sSTRecord);
		int num2 = num / 126;
		num2 = ((num2 < 8) ? 8 : num2);
		ExtSSTRecord extSSTRecord = (ExtSSTRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ExtSST);
		extSSTRecord.StringPerBucket = (ushort)num2;
		extSSTRecord.SSTInfo = new ExtSSTInfoSubRecord[sSTRecord.NumberOfUniqueStrings / num2 + 1];
		extSSTRecord.SST = sSTRecord;
		for (int i = 0; i < extSSTRecord.SSTInfo.Length; i++)
		{
			extSSTRecord.SSTInfo[i] = new ExtSSTInfoSubRecord();
		}
		records.Add(extSSTRecord);
	}

	private void SetRefCount(int index, int count)
	{
		if (newRefCount.Count > index)
		{
			newRefCount[index] = count;
		}
		else
		{
			newRefCount.Add(count);
		}
	}

	private int GetRefCount(int index)
	{
		if (newRefCount.Count > index)
		{
			return newRefCount[index];
		}
		return 0;
	}

	private int Find(TextWithFormat key)
	{
		int result = -1;
		if (m_bUseHash)
		{
			if (m_hashKeyToIndex.ContainsKey(key))
			{
				result = m_hashKeyToIndex[key];
			}
			else if (key.FormattingRunsCount == 0 && m_hashKeyToIndex.ContainsKey(key.Text))
			{
				result = m_hashKeyToIndex[key.Text];
			}
		}
		else
		{
			int i = 0;
			for (int count = m_arrStrings.Count; i < count; i++)
			{
				object obj = m_arrStrings[i];
				if (obj != null && ((key.FormattingRunsCount == 0 && obj is string && key.Text == (string)obj) || key.CompareTo(obj) == 0))
				{
					result = i;
					break;
				}
			}
		}
		return result;
	}

	private int Find(object key)
	{
		int result = -1;
		if (m_bUseHash)
		{
			if (m_hashKeyToIndex.ContainsKey(key))
			{
				result = m_hashKeyToIndex[key];
			}
		}
		else
		{
			int i = 0;
			for (int count = m_arrStrings.Count; i < count; i++)
			{
				object obj = m_arrStrings[i];
				if (obj != null && obj.Equals(key))
				{
					result = i;
					break;
				}
			}
		}
		return result;
	}

	private void FillHash()
	{
		int i = 0;
		for (int count = m_arrStrings.Count; i < count; i++)
		{
			object obj = m_arrStrings[i];
			if (obj != null)
			{
				m_hashKeyToIndex[obj] = i;
			}
		}
	}

	public void Parse()
	{
		if (m_bParsed)
		{
			return;
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		object[] strings = m_sstOriginal.Strings;
		for (int i = 0; i < strings.Length; i++)
		{
			int num = AddIncrease(strings[i], bIncrease: false);
			if (i != num)
			{
				dictionary.Add(i, num);
			}
		}
		if (dictionary.Count > 0)
		{
			UpdateLabelSSTIndexes(dictionary);
		}
		m_bParsed = true;
		m_sstOriginal = null;
	}

	internal void UpdateLabelSSTIndexes(Dictionary<int, int> dictUpdatedIndexes)
	{
		IWorksheets worksheets = m_book.Worksheets;
		int i = 0;
		for (int count = worksheets.Count; i < count; i++)
		{
			((WorksheetImpl)worksheets[i]).UpdateLabelSSTIndexes(dictUpdatedIndexes, AddIncrease);
		}
	}

	internal int GetLabelSSTCount()
	{
		int num = 0;
		if (m_bParsed)
		{
			foreach (KeyValuePair<object, int> item in m_hashKeyToIndex)
			{
				num += GetRefCount(item.Value);
			}
		}
		return num;
	}

	public void Dispose()
	{
		if (m_book != null)
		{
			m_hashKeyToIndex = null;
			m_arrStrings = null;
			m_arrFreeIndexes = null;
			m_book = null;
			m_sstOriginal = null;
			m_tempString = null;
			newRefCount.Clear();
			newRefCount = null;
			GC.SuppressFinalize(this);
		}
	}

	~SSTDictionary()
	{
		Dispose();
	}
}
