using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class ExternBookCollection : CollectionBaseEx<ExternWorkbookImpl>
{
	private const int StdDocumentOptions = 32746;

	private const int DEF_NO_SHEET_INDEX = 65534;

	internal const string DEF_WRONG_URL_NAME = " ";

	private WorkbookImpl m_book;

	private Dictionary<string, ExternWorkbookImpl> m_hashUrlToBook = new Dictionary<string, ExternWorkbookImpl>();

	private Dictionary<string, ExternWorkbookImpl> m_hashShortNameToBook = new Dictionary<string, ExternWorkbookImpl>();

	public new ExternWorkbookImpl this[int index]
	{
		get
		{
			if (index < 0 || index > base.Count)
			{
				throw new ArgumentOutOfRangeException("index", "Value cannot be less than 0 and greater than Count");
			}
			return base.List[index];
		}
	}

	public ExternWorkbookImpl this[string strUrl]
	{
		get
		{
			if (strUrl == null || strUrl.Length == 0)
			{
				return null;
			}
			m_hashUrlToBook.TryGetValue(strUrl, out var value);
			return value;
		}
	}

	public WorkbookImpl ParentWorkbook => m_book;

	public ExternBookCollection(IApplication application, object parent)
		: base(application, parent)
	{
		SetParents();
	}

	[CLSCompliant(false)]
	public void Serialize(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			base.List[i].Serialize(records);
		}
	}

	public new int Add(ExternWorkbookImpl book)
	{
		book.Index = base.List.Count;
		base.Add(book);
		return base.Count - 1;
	}

	public int Add(string fileName)
	{
		return Add(fileName, bAddInFunctions: false);
	}

	public int Add(string fileName, bool bAddInFunctions)
	{
		ExternWorkbookImpl externWorkbookImpl = new ExternWorkbookImpl(base.Application, this);
		externWorkbookImpl.IsInternalReference = false;
		externWorkbookImpl.IsAddInFunctions = true;
		externWorkbookImpl.URL = fileName;
		int num = Add(externWorkbookImpl);
		int sheetNumber = externWorkbookImpl.SheetNumber;
		int firstSheetIndex = ((sheetNumber == 0) ? 65534 : 0);
		int lastSheetIndex = ((sheetNumber == 0) ? 65534 : 0);
		m_book.AddSheetReference(num, firstSheetIndex, lastSheetIndex);
		return num;
	}

	public int Add(string filePath, string fileName, List<string> sheets, string[] names)
	{
		if (fileName == null || fileName.Length == 0)
		{
			throw new ArgumentOutOfRangeException("fileName");
		}
		ExternWorkbookImpl externWorkbookImpl = new ExternWorkbookImpl(base.Application, this);
		externWorkbookImpl.IsInternalReference = false;
		externWorkbookImpl.URL = ((filePath == null) ? fileName : (filePath + fileName));
		int result = Add(externWorkbookImpl);
		int sheetNumber = sheets?.Count ?? 0;
		externWorkbookImpl.SheetNumber = sheetNumber;
		externWorkbookImpl.AddWorksheets(sheets);
		externWorkbookImpl.AddNames(names);
		return result;
	}

	public int AddDDEFile(string fileName)
	{
		ExternWorkbookImpl externWorkbookImpl = new ExternWorkbookImpl(base.Application, this);
		externWorkbookImpl.IsInternalReference = false;
		externWorkbookImpl.URL = fileName;
		int num = Add(externWorkbookImpl);
		externWorkbookImpl.SheetNumber = 0;
		int firstSheetIndex = 65534;
		int lastSheetIndex = 65534;
		m_book.AddSheetReference(num, firstSheetIndex, lastSheetIndex);
		ExternNamesCollection externNames = externWorkbookImpl.ExternNames;
		int index = externNames.Add("StdDocument");
		externNames[index].Record.Options = 32746;
		return num;
	}

	public int InsertSelfSupbook()
	{
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			_ = this[i];
		}
		ExternWorkbookImpl externWorkbookImpl = new ExternWorkbookImpl(base.Application, this);
		externWorkbookImpl.Index = base.List.Count;
		externWorkbookImpl.IsInternalReference = true;
		base.Add(externWorkbookImpl);
		return base.Count - 1;
	}

	public bool ContainsExternName(string strName)
	{
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			if (this[i].ExternNames.Contains(strName))
			{
				return true;
			}
		}
		return false;
	}

	public bool ContainsExternName(string strName, ref int iBookIndex, ref int iNameIndex)
	{
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			ExternWorkbookImpl externWorkbookImpl = this[i];
			iNameIndex = externWorkbookImpl.ExternNames.GetNameIndex(strName);
			if (iNameIndex >= 0)
			{
				iBookIndex = m_book.AddSheetReference(externWorkbookImpl.Index, 65534, 65534);
				return true;
			}
		}
		return false;
	}

	public int GetNameIndexes(string strName, out int iRefIndex)
	{
		iRefIndex = -1;
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			int nameIndex = this[i].ExternNames.GetNameIndex(strName);
			if (nameIndex != -1)
			{
				iRefIndex = nameIndex;
				return i;
			}
		}
		return -1;
	}

	public ExternWorkbookImpl GetBookByShortName(string strShortName)
	{
		if (strShortName == null)
		{
			throw new ArgumentNullException("strShortName");
		}
		if (strShortName.Length == 0)
		{
			throw new ArgumentException("strShortName - string cannot be empty");
		}
		m_hashShortNameToBook.TryGetValue(strShortName, out var value);
		return value;
	}

	private void SetParents()
	{
		m_book = FindParent(typeof(WorkbookImpl)) as WorkbookImpl;
		if (m_book == null)
		{
			throw new ArgumentNullException("m_book");
		}
	}

	public int GetFirstInternalIndex()
	{
		int i = 0;
		for (int count = base.List.Count; i < count; i++)
		{
			if (base.List[i].IsInternalReference)
			{
				return i;
			}
		}
		return -1;
	}

	protected override void OnInsertComplete(int index, ExternWorkbookImpl value)
	{
		base.OnInsertComplete(index, value);
		value.Index = base.List.Count - 1;
		if (value.IsInternalReference)
		{
			return;
		}
		string uRL = value.URL;
		if (uRL != null && uRL != " ")
		{
			if (!m_hashUrlToBook.ContainsKey(uRL) || !m_book.IsWorkbookOpening)
			{
				m_hashUrlToBook.Add(uRL, value);
			}
			string shortName = value.ShortName;
			if (!m_hashShortNameToBook.ContainsKey(shortName))
			{
				m_hashShortNameToBook.Add(shortName, value);
			}
		}
	}

	public Dictionary<int, int> AddCopy(ExternBookCollection subBooks)
	{
		if (subBooks == null)
		{
			throw new ArgumentNullException("subBooks");
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		int firstInternalIndex = GetFirstInternalIndex();
		int i = 0;
		for (int count = subBooks.Count; i < count; i++)
		{
			ExternWorkbookImpl externWorkbookImpl = subBooks[i];
			int num = -1;
			ExternWorkbookImpl externWorkbookImpl2 = this[externWorkbookImpl.URL];
			if (externWorkbookImpl.IsInternalReference && firstInternalIndex >= 0)
			{
				num = firstInternalIndex;
			}
			else if (externWorkbookImpl2 == null)
			{
				externWorkbookImpl = (ExternWorkbookImpl)externWorkbookImpl.Clone(this);
				num = Add(externWorkbookImpl);
			}
			else
			{
				num = externWorkbookImpl2.Index;
			}
			dictionary.Add(i, num);
		}
		return dictionary;
	}

	internal ExternWorkbookImpl FindOrAdd(string strBook, string strBookPath)
	{
		if (strBook == null || strBook.Length == 0)
		{
			throw new ArgumentOutOfRangeException("strBook");
		}
		string key = ((strBookPath == null) ? strBook : (strBookPath + strBook));
		ExternWorkbookImpl externWorkbookImpl = null;
		if (m_hashUrlToBook.ContainsKey(key))
		{
			return m_hashUrlToBook[key];
		}
		if ((strBook == null || strBook.Length == 0) && m_hashShortNameToBook.ContainsKey(strBook))
		{
			return m_hashShortNameToBook[strBook];
		}
		return this[Add(strBookPath, strBook, null, null)];
	}

	internal void Dispose()
	{
		for (int num = base.Count - 1; num >= 0; num--)
		{
			this[num].Dispose();
		}
		Clear();
	}

	internal int Add(string fileName, WorkbookImpl book, IRange sourceRange)
	{
		ExternWorkbookImpl externWorkbookImpl = new ExternWorkbookImpl(base.Application, this);
		externWorkbookImpl.IsInternalReference = false;
		externWorkbookImpl.IsAddInFunctions = false;
		externWorkbookImpl.URL = fileName;
		int num = Add(externWorkbookImpl);
		IWorksheets worksheets = sourceRange.Worksheet.Workbook.Worksheets;
		ExternWorksheetImpl externWorksheetImpl = null;
		string name = sourceRange.Worksheet.Name;
		int i = 0;
		for (int count = worksheets.Count; i < count; i++)
		{
			string name2 = worksheets[i].Name;
			ExternWorksheetImpl externWorksheetImpl2 = externWorkbookImpl.AddWorksheet(name2);
			if (name2 == name)
			{
				externWorksheetImpl = externWorksheetImpl2;
			}
		}
		int sheetNumber = externWorkbookImpl.SheetNumber;
		int firstSheetIndex = ((sheetNumber == 0) ? 65534 : externWorksheetImpl.Index);
		int lastSheetIndex = ((sheetNumber == 0) ? 65534 : externWorksheetImpl.Index);
		externWorksheetImpl.CacheValues(sourceRange);
		m_book.AddSheetReference(num, firstSheetIndex, lastSheetIndex);
		return num;
	}
}
