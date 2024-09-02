using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class WorkbookNamesCollection : CollectionBaseEx<IName>, INames, IEnumerable
{
	private char[] SpecialChars = new char[23]
	{
		'!', '@', '#', '$', '%', '^', '&', '*', '(', ')',
		'-', '=', '+', ']', '}', '[', '{', ';', ':', '/',
		'.', '>', '<'
	};

	private Dictionary<string, IName> m_hashNameToIName = new Dictionary<string, IName>();

	private WorkbookImpl m_book;

	private bool m_bWorkNamesChanged;

	public new IName this[int index]
	{
		get
		{
			if (index < 0 || index >= base.List.Count)
			{
				throw new ArgumentOutOfRangeException($"index is {index}, Count is {base.List.Count}");
			}
			int num = 0;
			for (int i = 0; i < base.List.Count; i++)
			{
				NameImpl nameImpl = base.List[i] as NameImpl;
				if (!nameImpl.IsDeleted)
				{
					if (num == index)
					{
						return nameImpl;
					}
					num++;
				}
			}
			return null;
		}
	}

	public IName this[string name]
	{
		get
		{
			m_hashNameToIName.TryGetValue(name, out var value);
			if (value == null)
			{
				return null;
			}
			if (!(value as NameImpl).IsDeleted)
			{
				return value;
			}
			return null;
		}
	}

	public IWorksheet ParentWorksheet => null;

	int INames.Count => GetKnownNamedCount();

	public new int Count => base.List.Count;

	public bool IsWorkbookNamesChanged
	{
		get
		{
			return m_bWorkNamesChanged;
		}
		set
		{
			if (!m_book.IsWorkbookOpening)
			{
				m_bWorkNamesChanged = value;
			}
		}
	}

	public WorkbookNamesCollection(IApplication application, object parent)
		: base(application, parent)
	{
		SetParent();
	}

	public IName Add(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name.Length == 0)
		{
			throw new ArgumentException("name");
		}
		if (!IsValidName(name, m_book) || char.IsNumber(name[0]))
		{
			throw new ArgumentException("This is not a valid name. Name should not be same as the cell name.");
		}
		m_hashNameToIName.TryGetValue(name, out var value);
		if (value != null && !value.IsLocal)
		{
			(value as NameImpl).IsDeleted = false;
			return value;
		}
		NameImpl nameImpl = new NameImpl(base.Application, m_book, name, base.List.Count);
		Add(nameImpl);
		return nameImpl;
	}

	private void CheckInvalidCharacters(string name)
	{
		string value = "?";
		if (name.IndexOfAny(SpecialChars) != -1 || name.Contains("\"") || name.StartsWith(value))
		{
			throw new ArgumentException("Contains invalid characters");
		}
		if (char.IsNumber(name[0]))
		{
			throw new ArgumentException("Contains invalid characters");
		}
		int num = 0;
		int num2 = 0;
		bool flag = false;
		int length = name.Length;
		foreach (char c in name)
		{
			if (char.IsLetter(c))
			{
				num++;
			}
			else if (char.IsNumber(c))
			{
				num2++;
			}
		}
		if (char.IsLetter(name[length - 1]) || name.EndsWith(value))
		{
			flag = true;
		}
		if (num <= 3 && num2 > 0 && !flag)
		{
			throw new ArgumentException("Contains invalid characters");
		}
	}

	protected override void OnClearComplete()
	{
		m_hashNameToIName.Clear();
	}

	public IName Add(string name, IRange namedRange)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (namedRange == null)
		{
			throw new ArgumentNullException("namedRange");
		}
		if (name.Length == 0)
		{
			throw new ArgumentException("name");
		}
		if (!IsValidName(name, m_book))
		{
			throw new ArgumentException("This is not a valid name. Name should not be same as the cell name.");
		}
		NameImpl nameImpl = new NameImpl(base.Application, this, name, namedRange, base.List.Count);
		Add(nameImpl);
		return nameImpl;
	}

	internal static bool IsValidName(string name, WorkbookImpl book)
	{
		bool bR1C = FormulaUtil.IsR1C1(name);
		bool flag = FormulaUtil.IsCell(name, bR1C, out var strRow, out var strColumn);
		if (strRow == null || strColumn == null)
		{
			return true;
		}
		int columnIndex = RangeImpl.GetColumnIndex(strColumn);
		int num = Convert.ToInt32(strRow);
		flag = ((flag && num < book.MaxRowCount && columnIndex < book.MaxColumnCount && num != 0) ? true : false);
		return !flag;
	}

	internal void Validate()
	{
		using IEnumerator<IName> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			NameImpl nameImpl = (NameImpl)enumerator.Current;
			if (!IsValidName(nameImpl.Name, m_book))
			{
				throw new Exception("Named Range " + nameImpl.Name + " is not supported in this version");
			}
		}
	}

	public new IName Add(IName name)
	{
		NameImpl nameImpl = name as NameImpl;
		bool isExternName = nameImpl.IsExternName;
		if (!m_book.IsWorkbookOpening && !isExternName && !nameImpl.IsLocal && m_hashNameToIName.ContainsKey(name.Name))
		{
			throw new ArgumentException("Name of the Name object must be unique.");
		}
		AddLocal(name);
		IsWorkbookNamesChanged = true;
		return name;
	}

	public void Remove(string name)
	{
		if (m_hashNameToIName.TryGetValue(name, out var value))
		{
			value.Delete();
		}
	}

	public new void RemoveAt(int index)
	{
		if (index < 0 || index > Count - 1)
		{
			throw new ArgumentOutOfRangeException("index", "Value cannot be less than 0 and greater than Count - 1.");
		}
		IName name = this[index];
		if (m_hashNameToIName.ContainsValue(name))
		{
			m_hashNameToIName.Remove(name.Name);
		}
		IList<IName> list = base.List;
		list.RemoveAt(index);
		IsWorkbookNamesChanged = true;
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		int i = index;
		for (int count = list.Count; i < count; i++)
		{
			NameImpl nameImpl = (NameImpl)list[i];
			dictionary.Add(nameImpl.Index, i);
			nameImpl.SetIndex(i);
		}
	}

	public void Remove(int[] arrIndexes)
	{
		List<int> list = new List<int>(arrIndexes);
		list.Sort();
		int count = base.List.Count;
		for (int num = list.Count - 1; num >= 0; num--)
		{
			int num2 = list[num];
			if (num2 < 0 || num2 >= count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			IName name = base.List[num2];
			m_hashNameToIName.Remove(name.Name);
			base.RemoveAt(num2);
			IsWorkbookNamesChanged = true;
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		int i = list[0];
		for (int count2 = Count; i < count2; i++)
		{
			NameImpl nameImpl = (NameImpl)base.List[i];
			dictionary.Add(nameImpl.Index, i);
			nameImpl.SetIndex(i);
		}
		m_book.UpdateNamedRangeIndexes(dictionary);
	}

	public bool Contains(string name)
	{
		if (m_hashNameToIName.ContainsKey(name))
		{
			return !(m_hashNameToIName[name] as NameImpl).IsDeleted;
		}
		return false;
	}

	internal bool Contains(string name, bool isFormulaNamedrange)
	{
		if (isFormulaNamedrange)
		{
			if (m_hashNameToIName.ContainsKey(name))
			{
				return !(m_hashNameToIName[name] as NameImpl).IsDeleted;
			}
			return false;
		}
		if (m_hashNameToIName.ContainsKey(name) && !(m_hashNameToIName[name] as NameImpl).IsDeleted)
		{
			return !(m_hashNameToIName[name] as NameImpl).m_isFormulaNamedRange;
		}
		return false;
	}

	public void InsertRow(int iRowIndex, int iRowCount, string strSheetName)
	{
		InsertRemoveRowColumn(strSheetName, iRowIndex, bIsRemove: false, bIsRow: true, iRowCount);
	}

	public void RemoveRow(int iRowIndex, string strSheetName)
	{
		InsertRemoveRowColumn(strSheetName, iRowIndex, bIsRemove: true, bIsRow: true, 1);
	}

	public void RemoveRow(int iRowIndex, string strSheetName, int count)
	{
		InsertRemoveRowColumn(strSheetName, iRowIndex, bIsRemove: true, bIsRow: true, count);
	}

	public void InsertColumn(int iColumnIndex, int iCount, string strSheetName)
	{
		InsertRemoveRowColumn(strSheetName, iColumnIndex, bIsRemove: false, bIsRow: false, iCount);
	}

	public void RemoveColumn(int iColumnIndex, string strSheetName)
	{
		RemoveColumn(iColumnIndex, strSheetName, 1);
	}

	public void RemoveColumn(int iColumnIndex, string strSheetName, int count)
	{
		InsertRemoveRowColumn(strSheetName, iColumnIndex, bIsRemove: true, bIsRow: false, count);
	}

	[CLSCompliant(false)]
	public IName Add(NameRecord name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		NameImpl nameImpl = new NameImpl(base.Application, this, name.Name, base.List.Count);
		Add(nameImpl);
		nameImpl.Parse(name);
		return nameImpl;
	}

	[CLSCompliant(false)]
	public void AddRange(NameRecord[] names)
	{
		if (names == null)
		{
			throw new ArgumentNullException("names");
		}
		int i = 0;
		for (int num = names.Length; i < num; i++)
		{
			Add(names[i]);
		}
	}

	[CLSCompliant(false)]
	public void Serialize(OffsetArrayList records)
	{
		if (Count != 0)
		{
			if (records == null)
			{
				throw new ArgumentNullException("records");
			}
			SortForSerialization();
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				((NameImpl)base.InnerList[i]).Serialize(records);
			}
		}
	}

	public void AddLocal(IName name)
	{
		AddLocal(name, bAddInGlobalNamesHash: true);
	}

	public void AddLocal(IName name, bool bAddInGlobalNamesHash)
	{
		((NameImpl)name).SetIndex(Count);
		if (bAddInGlobalNamesHash)
		{
			base.Add(name);
		}
		else
		{
			base.InnerList.Add(name);
		}
		IsWorkbookNamesChanged = true;
	}

	internal void SortForSerialization()
	{
		if (m_bWorkNamesChanged)
		{
			GetSortedWorksheets();
			SetIndexesWithoutEvent();
			IsWorkbookNamesChanged = false;
		}
	}

	private SortedList<string, object> GetSortedWorksheets()
	{
		IWorksheets worksheets = m_book.Worksheets;
		int count = worksheets.Count;
		SortedList<string, object> sortedList = new SortedList<string, object>(count);
		for (int i = 0; i < count; i++)
		{
			sortedList.Add(worksheets[i].Name, null);
		}
		return sortedList;
	}

	private int[] GetNewIndexes(SortedList<string, object> list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		int count = Count;
		List<IName> list2 = new List<IName>(base.InnerList);
		int[] array = new int[count];
		int num = 0;
		bool[] array2 = new bool[count];
		int count2 = list.Count;
		string strGlobalName = list.Keys[count2 - 1] + "_1";
		for (int i = 0; i < count; i++)
		{
			if (array2[i])
			{
				continue;
			}
			NameImpl nameImpl = (NameImpl)list2[i];
			_ = nameImpl.Worksheet;
			SortedList<string, NameImpl> sortedList = FindSameNames(nameImpl, strGlobalName);
			int count3 = sortedList.Count;
			IList<NameImpl> values = sortedList.Values;
			for (int j = 0; j < count3; j++)
			{
				NameImpl nameImpl2 = values[j];
				if (num < base.InnerList.Count)
				{
					base.InnerList[num] = nameImpl2;
					int index = nameImpl2.Index;
					if (!array2[index])
					{
						array[index] = num;
						array2[index] = true;
						num++;
					}
					continue;
				}
				throw new ApplicationException();
			}
		}
		return array;
	}

	private SortedList<string, NameImpl> FindSameNames(NameImpl name, string strGlobalName)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		SortedList<string, NameImpl> sortedList = new SortedList<string, NameImpl>();
		string name2 = name.Name;
		IWorksheets worksheets = m_book.Worksheets;
		int indexOrGlobal = name.Record.IndexOrGlobal;
		IWorksheet worksheet = name.Worksheet;
		int i = 0;
		for (int count = worksheets.Count; i < count; i++)
		{
			IWorksheet worksheet2 = worksheets[i];
			if (worksheet != worksheet2)
			{
				AddNameToList(sortedList, worksheet2.Names, name2, worksheet2.Name);
			}
		}
		if (indexOrGlobal != 0)
		{
			AddNameToList(sortedList, m_book.Names, name2, strGlobalName);
			strGlobalName = worksheet.Name;
		}
		sortedList.Add(strGlobalName, name);
		return sortedList;
	}

	private void AddNameToList(SortedList<string, NameImpl> list, INames names, string strName, string strSheetName)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (names == null)
		{
			throw new ArgumentNullException("names");
		}
		if (strName == null)
		{
			throw new ArgumentNullException("strName");
		}
		if (strName.Length == 0)
		{
			throw new ArgumentException("strName - string cannot be empty");
		}
		NameImpl nameImpl = (NameImpl)names[strName];
		if (nameImpl != null)
		{
			list.Add(strSheetName, nameImpl);
		}
	}

	private void UpdateIndexes(int[] arrNewIndex)
	{
		if (arrNewIndex == null)
		{
			throw new ArgumentNullException("arrNewIndex");
		}
		m_book.UpdateNamedRangeIndexes(arrNewIndex);
	}

	private void SetIndexesWithoutEvent()
	{
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			((NameImpl)base.InnerList[i]).SetIndex(i, bRaiseEvent: false);
		}
	}

	public void ParseNames()
	{
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			((IParseable)this[i]).Parse();
		}
	}

	public int AddFunctions(string strFunctionName)
	{
		NameRecord nameRecord = (NameRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Name);
		nameRecord.IsFunctionOrCommandMacro = true;
		nameRecord.IsNameFunction = true;
		nameRecord.IsNameCommand = true;
		nameRecord.Name = strFunctionName;
		return (Add(nameRecord) as NameImpl).Index;
	}

	[CLSCompliant(false)]
	public NameRecord GetNameRecordByIndex(int index)
	{
		if (index < 0 || index >= Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return ((NameImpl)this[index]).Record;
	}

	public IName AddCopy(IName nameToCopy, IWorksheet destSheet, Dictionary<int, int> hashExternSheetIndexes, IDictionary hashNewWorksheetNames)
	{
		if (nameToCopy == null)
		{
			throw new ArgumentNullException("nameToCopy");
		}
		if (destSheet == null)
		{
			throw new ArgumentNullException("destSheet");
		}
		string name = nameToCopy.Name;
		IName name2 = null;
		m_book.AddSheetReference(destSheet);
		NameImpl obj = (NameImpl)nameToCopy;
		NameRecord nameRecord = obj.Record.Clone() as NameRecord;
		WorkbookImpl workbook = obj.Workbook;
		WorksheetNamesCollection.UpdateReferenceIndexes(nameRecord, workbook, hashNewWorksheetNames, hashExternSheetIndexes, m_book);
		if (Contains(name))
		{
			nameRecord.IndexOrGlobal = (ushort)(destSheet.Index + 1);
			return ((WorksheetNamesCollection)destSheet.Names).Add(nameRecord, bAddInGlobalNamesHash: false);
		}
		return Add(nameRecord);
	}

	private void SetReferenceIndex(NameRecord name, int iRefIndex)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		Ptg[] formulaTokens = name.FormulaTokens;
		int i = 0;
		for (int num = formulaTokens.Length; i < num; i++)
		{
			if (formulaTokens[i] is IReference reference)
			{
				reference.RefIndex = (ushort)iRefIndex;
			}
		}
	}

	public override object Clone(object parent)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		WorkbookNamesCollection obj = (WorkbookNamesCollection)base.Clone(parent);
		obj.m_bWorkNamesChanged = m_bWorkNamesChanged;
		return obj;
	}

	protected override void OnInsertComplete(int index, IName value)
	{
		NameImpl nameImpl = (NameImpl)value;
		base.OnInsertComplete(index, value);
		if (!nameImpl.IsBuiltIn && !nameImpl.IsExternName && !nameImpl.IsLocal)
		{
			m_hashNameToIName[nameImpl.Name] = nameImpl;
		}
	}

	public void ConvertFullRowColumnNames(OfficeVersion version)
	{
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			((NameImpl)GetNameByIndex(i)).ConvertFullRowColumnName(version);
		}
	}

	internal IName GetNameByIndex(int index)
	{
		return base.List[index];
	}

	private void SetParent()
	{
		m_book = FindParent(typeof(WorkbookImpl)) as WorkbookImpl;
		if (m_book == null)
		{
			throw new ArgumentNullException("NamesCollection has no parent Workbook.");
		}
	}

	private void InsertRemoveRowColumn(string strSheetName, int index, bool bIsRemove, bool bIsRow, int iCount)
	{
		int num = (bIsRow ? m_book.MaxRowCount : m_book.MaxColumnCount);
		if (index < 1 || index > num)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (strSheetName == null)
		{
			throw new ArgumentNullException("strSheetName");
		}
		if (strSheetName.Length == 0)
		{
			throw new ArgumentException("strSheetName");
		}
		List<IName> innerList = base.InnerList;
		int count = innerList.Count;
		if (iCount == 0)
		{
			return;
		}
		IsWorkbookNamesChanged = true;
		index--;
		int i = 0;
		for (int num2 = count; i < num2; i++)
		{
			NameImpl nameImpl = (NameImpl)innerList[i];
			NameRecord record = nameImpl.Record;
			Ptg[] formulaTokens = record.FormulaTokens;
			if (formulaTokens == null)
			{
				continue;
			}
			int j = 0;
			for (int num3 = formulaTokens.Length; j < num3; j++)
			{
				if (formulaTokens[j] is IRangeGetterToken rangeGetterToken && (!(rangeGetterToken is IReference reference) || !m_book.IsExternalReference(reference.RefIndex)))
				{
					if (nameImpl != null && nameImpl.Worksheet != null && (nameImpl.Worksheet.ParseDataOnDemand || nameImpl.Worksheet.ParseOnDemand))
					{
						nameImpl.Worksheet.ParseData(null);
					}
					Ptg ptg = InsertRemoveRow(rangeGetterToken, strSheetName, index, bIsRemove, bIsRow, iCount, nameImpl.Worksheet);
					if (ptg == null)
					{
						ptg = rangeGetterToken.ConvertToError();
					}
					formulaTokens[j] = ptg;
				}
			}
			record.FormulaTokens = formulaTokens;
		}
	}

	private Ptg InsertRemoveRow(IRangeGetterToken token, string strSheetName, int index, bool bIsRemove, bool bIsRow, int iCount, IWorksheet sheet)
	{
		if (token == null)
		{
			throw new ArgumentNullException("token");
		}
		MergeCellsRecord.MergedRegion mergedRegion = null;
		Ptg result = (Ptg)token;
		IRange range = token.GetRange(m_book, sheet);
		if (range != null && range.Worksheet.Name == strSheetName)
		{
			Rectangle rectangle = token.GetRectangle();
			MergeCellsRecord.MergedRegion region = new MergeCellsRecord.MergedRegion(rectangle.Top, rectangle.Bottom, rectangle.Left, rectangle.Right);
			result = null;
			mergedRegion = (bIsRow ? MergeCellsImpl.InsertRemoveRow(region, index, bIsRemove, iCount, m_book) : MergeCellsImpl.InsertRemoveColumn(region, index, bIsRemove, iCount, m_book));
			result = ((mergedRegion == null) ? null : (result = token.UpdateRectangle(mergedRegion.GetRectangle())));
		}
		return result;
	}

	public void MarkUsedReferences(bool[] usedItems)
	{
		List<IName> innerList = base.InnerList;
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			FormulaUtil.MarkUsedReferences(((NameImpl)innerList[i]).Record.FormulaTokens, usedItems);
		}
	}

	public void UpdateReferenceIndexes(int[] arrUpdatedIndexes)
	{
		List<IName> innerList = base.InnerList;
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			NameRecord record = ((NameImpl)innerList[i]).Record;
			Ptg[] formulaTokens = record.FormulaTokens;
			if (FormulaUtil.UpdateReferenceIndexes(formulaTokens, arrUpdatedIndexes))
			{
				record.FormulaTokens = formulaTokens;
			}
		}
	}

	private int GetKnownNamedCount()
	{
		int num = 0;
		foreach (IName item in base.List)
		{
			if (item.RefersToRange != null)
			{
				num++;
			}
		}
		return num;
	}
}
