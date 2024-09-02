using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation;

internal class ExternWorkbookImpl : CommonObject, ICloneParent
{
	private SortedList<int, ExternWorksheetImpl> m_arrSheets = new SortedList<int, ExternWorksheetImpl>();

	private Dictionary<string, ExternWorksheetImpl> m_hashNameToSheet = new Dictionary<string, ExternWorksheetImpl>();

	private ExternNamesCollection m_externNames;

	private SupBookRecord m_supBook;

	private int m_iIndex;

	private WorkbookImpl m_book;

	private string m_strShortName;

	private string m_strProgramId;

	private bool m_isParsed;

	public ExternNamesCollection ExternNames => m_externNames;

	public bool IsInternalReference
	{
		get
		{
			return m_supBook.IsInternalReference;
		}
		set
		{
			m_supBook.IsInternalReference = value;
		}
	}

	public bool IsOleLink
	{
		get
		{
			if (m_externNames != null && m_externNames.Count == 1)
			{
				return m_externNames[0].Record.OleLink;
			}
			return false;
		}
	}

	public int SheetNumber
	{
		get
		{
			return m_supBook.SheetNumber;
		}
		set
		{
			m_supBook.SheetNumber = (ushort)value;
		}
	}

	public string URL
	{
		get
		{
			return m_supBook.URL;
		}
		set
		{
			m_supBook.URL = value;
			InitShortName();
			if (value == null)
			{
				m_arrSheets.Clear();
				m_hashNameToSheet.Clear();
				m_supBook.SheetNames = null;
			}
		}
	}

	public int Index
	{
		get
		{
			return m_iIndex;
		}
		set
		{
			m_iIndex = value;
		}
	}

	public WorkbookImpl Workbook => m_book;

	public string ShortName => m_strShortName;

	public bool IsAddInFunctions
	{
		get
		{
			return m_supBook.IsAddInFunctions;
		}
		set
		{
			m_supBook.IsAddInFunctions = value;
		}
	}

	public SortedList<int, ExternWorksheetImpl> Worksheets => m_arrSheets;

	public string ProgramId
	{
		get
		{
			return m_strProgramId;
		}
		set
		{
			m_strProgramId = value;
		}
	}

	internal bool IsParsed
	{
		get
		{
			return m_isParsed;
		}
		set
		{
			m_isParsed = value;
		}
	}

	public ExternWorkbookImpl(IApplication application, object parent)
		: base(application, parent)
	{
		InitializeVariables();
	}

	private void InitializeVariables()
	{
		FindParents();
		m_externNames = new ExternNamesCollection(base.Application, this);
		m_supBook = (SupBookRecord)BiffRecordFactory.GetRecord(TBIFFRecord.SupBook);
		m_supBook.SheetNames = new List<string>();
		InitShortName();
	}

	public void InsertDefaultWorksheet()
	{
		m_supBook.SheetNames.Add("Sheet1");
		ExternWorksheetImpl externWorksheetImpl = new ExternWorksheetImpl(base.Application, this);
		externWorksheetImpl.Index = 0;
		m_arrSheets.Add(0, externWorksheetImpl);
	}

	private void FindParents()
	{
		m_book = FindParent(typeof(WorkbookImpl)) as WorkbookImpl;
		if (m_book == null)
		{
			throw new ArgumentNullException("m_book");
		}
		m_supBook = (SupBookRecord)CloneUtils.CloneCloneable(m_supBook);
	}

	[CLSCompliant(false)]
	public int Parse(BiffRecordRaw[] arrData, int iOffset)
	{
		throw new NotImplementedException();
	}

	[CLSCompliant(false)]
	public void Serialize(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		SupBookRecord supBookRecord = m_supBook;
		if (!m_supBook.IsInternalReference && supBookRecord.URL != null)
		{
			supBookRecord = (SupBookRecord)m_supBook.Clone();
			supBookRecord.URL = m_book.EncodeName(m_supBook.URL);
		}
		records.Add(supBookRecord);
		m_externNames.Serialize(records);
		if (!IsInternalReference)
		{
			IList<ExternWorksheetImpl> values = m_arrSheets.Values;
			int i = 0;
			for (int count = m_arrSheets.Count; i < count; i++)
			{
				values[i].Serialize(records);
			}
		}
	}

	private void AddExternSheet(ExternWorksheetImpl sheet)
	{
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		int index = sheet.Index;
		m_arrSheets[index] = sheet;
		int count = m_supBook.SheetNames.Count;
		if (index < count)
		{
			string key = m_supBook.SheetNames[index];
			m_hashNameToSheet[key] = sheet;
		}
	}

	public int IndexOf(string strSheetName)
	{
		if (strSheetName == null || strSheetName.Length == 0)
		{
			return -1;
		}
		if (!m_hashNameToSheet.TryGetValue(strSheetName, out var value))
		{
			return -1;
		}
		return value.Index;
	}

	public void saveAsHtml(string FileName)
	{
	}

	public int GetNewIndex(int iNameIndex)
	{
		return m_externNames.GetNewIndex(iNameIndex);
	}

	public object Clone(object parent)
	{
		ExternWorkbookImpl externWorkbookImpl = (ExternWorkbookImpl)MemberwiseClone();
		externWorkbookImpl.SetParent(parent);
		externWorkbookImpl.FindParents();
		externWorkbookImpl.m_arrSheets = new SortedList<int, ExternWorksheetImpl>();
		IList<int> keys = m_arrSheets.Keys;
		IList<ExternWorksheetImpl> values = m_arrSheets.Values;
		int i = 0;
		for (int count = m_arrSheets.Count; i < count; i++)
		{
			_ = keys[i];
			ExternWorksheetImpl externWorksheetImpl = values[i];
			externWorksheetImpl = externWorksheetImpl.Clone(externWorkbookImpl);
			externWorkbookImpl.AddExternSheet(externWorksheetImpl);
		}
		externWorkbookImpl.m_externNames = (ExternNamesCollection)m_externNames.Clone(this);
		return externWorkbookImpl;
	}

	public string GetSheetName(int index)
	{
		switch (index)
		{
		case 65535:
			return "#REF";
		case 65534:
			index = 0;
			break;
		}
		return m_supBook.SheetNames[index];
	}

	private void InitShortName()
	{
		string uRL = m_supBook.URL;
		m_strShortName = ((uRL != null) ? GetFileName(m_supBook.URL) : null);
	}

	private static string GetFileName(string strUrl)
	{
		if (strUrl == null || strUrl.Length == 0)
		{
			return strUrl;
		}
		int num = strUrl.LastIndexOf('\\');
		int length = strUrl.Length;
		int num2 = 0;
		if (num > 0)
		{
			num2 = num + 1;
		}
		return strUrl.Substring(num2, length - num2);
	}

	private static string GetFileNameWithoutExtension(string strUrl)
	{
		if (strUrl == null || strUrl.Length == 0)
		{
			return strUrl;
		}
		int num = strUrl.LastIndexOf('\\');
		int num2 = strUrl.LastIndexOf('.');
		int num3 = 0;
		int num4 = strUrl.Length;
		if (num > 0)
		{
			num3 = num + 1;
		}
		if (num2 > num3)
		{
			num4 = num2;
		}
		return strUrl.Substring(num3, num4 - num3);
	}

	public void AddWorksheets(List<string> sheets)
	{
		int num = sheets?.Count ?? 0;
		if (num != 0)
		{
			for (int i = 0; i < num; i++)
			{
				AddWorksheet(sheets[i]);
			}
		}
	}

	public void AddWorksheets(string[] sheets)
	{
		int num = ((sheets != null) ? sheets.Length : 0);
		if (num != 0)
		{
			for (int i = 0; i < num; i++)
			{
				AddWorksheet(sheets[i]);
			}
		}
	}

	public ExternWorksheetImpl AddWorksheet(string sheetName)
	{
		if (sheetName == null)
		{
			throw new ArgumentOutOfRangeException("sheetName");
		}
		ExternWorksheetImpl externWorksheetImpl = new ExternWorksheetImpl(base.Application, this);
		int key = (externWorksheetImpl.Index = m_arrSheets.Count);
		externWorksheetImpl.Name = sheetName;
		m_arrSheets.Add(key, externWorksheetImpl);
		m_hashNameToSheet.Add(sheetName, externWorksheetImpl);
		m_supBook.SheetNames.Add(sheetName);
		return externWorksheetImpl;
	}

	public void AddNames(string[] names)
	{
		int num = ((names != null) ? names.Length : 0);
		for (int i = 0; i < num; i++)
		{
			AddName(names[i]);
		}
	}

	public void AddName(string name)
	{
		m_externNames.Add(name);
	}

	internal int FindOrAddSheet(string sheetName)
	{
		if (sheetName == null || sheetName.Length == 0)
		{
			throw new ArgumentOutOfRangeException("sheetName");
		}
		if (!m_hashNameToSheet.TryGetValue(sheetName, out var value))
		{
			value = AddWorksheet(sheetName);
		}
		return value.Index;
	}

	protected override void OnDispose()
	{
		if (m_bIsDisposed)
		{
			return;
		}
		if (m_arrSheets != null)
		{
			foreach (ExternWorksheetImpl value in m_arrSheets.Values)
			{
				value.Dispose();
			}
			m_arrSheets.Clear();
			m_arrSheets = null;
		}
		base.OnDispose();
	}
}
