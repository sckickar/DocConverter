using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Interfaces;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class WorksheetsCollection : CollectionBaseEx<IWorksheet>, IWorksheets, IEnumerable, ICloneParent
{
	private Dictionary<string, IWorksheet> m_list = new Dictionary<string, IWorksheet>(StringComparer.CurrentCultureIgnoreCase);

	private WorkbookImpl m_book;

	private bool m_bUseHash = true;

	internal static string[] sheetNameValidators = new string[7] { "*", "?", ":", "/", "[", "]", "\\" };

	public new IWorksheet this[int Index] => base.InnerList[Index];

	public IWorksheet this[string sheetName]
	{
		get
		{
			IWorksheet value = null;
			if (m_bUseHash)
			{
				m_list.TryGetValue(sheetName, out value);
			}
			else
			{
				List<IWorksheet> innerList = base.InnerList;
				StringComparer currentCultureIgnoreCase = StringComparer.CurrentCultureIgnoreCase;
				int i = 0;
				for (int count = innerList.Count; i < count; i++)
				{
					IWorksheet worksheet = innerList[i];
					if (currentCultureIgnoreCase.Compare(worksheet.Name, sheetName) == 0)
					{
						value = worksheet;
						break;
					}
				}
			}
			return value;
		}
	}

	public bool UseRangesCache
	{
		get
		{
			if (base.Count == 0)
			{
				return false;
			}
			List<IWorksheet> innerList = base.InnerList;
			bool useRangesCache = innerList[0].UseRangesCache;
			int i = 1;
			for (int count = innerList.Count; i < count; i++)
			{
				if (innerList[i].UseRangesCache != useRangesCache)
				{
					return false;
				}
			}
			return useRangesCache;
		}
		set
		{
			List<IWorksheet> innerList = base.InnerList;
			int i = 0;
			for (int count = innerList.Count; i < count; i++)
			{
				innerList[i].UseRangesCache = value;
			}
		}
	}

	public bool UseHashForWorksheetLookup
	{
		get
		{
			return m_bUseHash;
		}
		set
		{
			if (m_bUseHash == value)
			{
				return;
			}
			m_bUseHash = value;
			if (value)
			{
				List<IWorksheet> innerList = base.InnerList;
				int i = 0;
				for (int count = base.Count; i < count; i++)
				{
					IWorksheet worksheet = innerList[i];
					m_list.Add(worksheet.Name, worksheet);
				}
			}
			else
			{
				m_list.Clear();
			}
		}
	}

	public bool IsRightToLeft
	{
		get
		{
			List<IWorksheet> innerList = base.InnerList;
			bool isRightToLeft = innerList[0].IsRightToLeft;
			int i = 1;
			for (int count = innerList.Count; i < count; i++)
			{
				if (innerList[i].IsRightToLeft != isRightToLeft || !isRightToLeft)
				{
					return false;
				}
			}
			return isRightToLeft;
		}
		set
		{
			List<IWorksheet> innerList = base.InnerList;
			int i = 0;
			for (int count = innerList.Count; i < count; i++)
			{
				innerList[i].IsRightToLeft = value;
			}
		}
	}

	public WorksheetsCollection(IApplication application, object parent)
		: base(application, parent)
	{
		m_book = FindParent(typeof(WorkbookImpl)) as WorkbookImpl;
		if (m_book == null)
		{
			throw new ArgumentNullException("Can't find parent workbook.");
		}
		m_book.Objects.TabSheetMoved += Objects_TabSheetMoved;
	}

	internal new IWorksheet Add(IWorksheet sheet)
	{
		m_book.Objects.Add((ISerializableNamedObject)sheet);
		base.Add(sheet);
		return sheet;
	}

	protected internal void RemoveLocal(string name)
	{
		IWorksheet item = this[name];
		base.Remove(item);
	}

	public void Move(int iOldIndex, int iNewIndex)
	{
		if (iOldIndex != iNewIndex)
		{
			int count = base.InnerList.Count;
			if (iOldIndex < 0 || iOldIndex >= count)
			{
				throw new ArgumentOutOfRangeException("iOldIndex");
			}
			if (iNewIndex < 0 || iNewIndex >= count)
			{
				throw new ArgumentOutOfRangeException("iNewIndex");
			}
			m_book.Objects.Move(iOldIndex, iNewIndex);
			WorksheetImpl item = this[iOldIndex] as WorksheetImpl;
			base.InnerList.RemoveAt(iOldIndex);
			base.InnerList.Insert(iNewIndex, item);
			int num = Math.Min(iNewIndex, iOldIndex);
			int num2 = Math.Max(iNewIndex, iOldIndex);
			for (int i = num; i <= num2; i++)
			{
				item = this[i] as WorksheetImpl;
				item.Index = i;
			}
		}
	}

	public void UpdateSheetIndex(WorksheetImpl sheet, int iOldRealIndex)
	{
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		int realIndex = sheet.RealIndex;
		int num = 0;
		int num2 = -1;
		ITabSheets tabSheets = m_book.TabSheets;
		if (iOldRealIndex > realIndex)
		{
			num2 = realIndex + 1;
			num = 1;
		}
		else
		{
			if (iOldRealIndex >= realIndex)
			{
				throw new NotImplementedException("Worksheet wasn't moved at all");
			}
			num2 = realIndex - 1;
			num = -1;
		}
		ITabSheet tabSheet = null;
		int num3 = num2;
		while (true)
		{
			ITabSheet tabSheet2 = tabSheets[num3];
			if (tabSheet2 is WorksheetImpl)
			{
				tabSheet = tabSheet2;
				break;
			}
			if (num3 == iOldRealIndex)
			{
				break;
			}
			num3 += num;
		}
		if (tabSheet != null)
		{
			WorksheetImpl obj = (WorksheetImpl)tabSheet;
			int index = sheet.Index;
			int index2 = obj.Index;
			MoveInternal(index, index2);
		}
	}

	private void MoveInternal(int iOldSheetIndex, int iNewSheetIndex)
	{
		if (iOldSheetIndex != iNewSheetIndex)
		{
			int count = base.InnerList.Count;
			if (iOldSheetIndex < 0 || iOldSheetIndex >= count)
			{
				throw new ArgumentOutOfRangeException("iOldIndex");
			}
			if (iNewSheetIndex < 0 || iNewSheetIndex >= count)
			{
				throw new ArgumentOutOfRangeException("iNewIndex");
			}
			WorksheetImpl item = this[iOldSheetIndex] as WorksheetImpl;
			base.InnerList.RemoveAt(iOldSheetIndex);
			base.InnerList.Insert(iNewSheetIndex, item);
			int num = Math.Min(iNewSheetIndex, iOldSheetIndex);
			int num2 = Math.Max(iNewSheetIndex, iOldSheetIndex);
			for (int i = num; i <= num2; i++)
			{
				item = this[i] as WorksheetImpl;
				item.Index = i;
			}
		}
	}

	public IRange FindFirst(string findValue, OfficeFindType flags)
	{
		return FindFirst(findValue, flags, OfficeFindOptions.None);
	}

	public IRange FindFirst(string findValue, OfficeFindType flags, OfficeFindOptions findOptions)
	{
		if (findValue == null)
		{
			return null;
		}
		bool num = (flags & OfficeFindType.Formula) == OfficeFindType.Formula;
		bool flag = (flags & OfficeFindType.Text) == OfficeFindType.Text;
		bool flag2 = (flags & OfficeFindType.FormulaStringValue) == OfficeFindType.FormulaStringValue;
		bool flag3 = (flags & OfficeFindType.Error) == OfficeFindType.Error;
		if (!(num || flag || flag2 || flag3))
		{
			throw new ArgumentException("Parameter flag is not valid.", "flags");
		}
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			IRange range = innerList[i].FindFirst(findValue, flags, findOptions);
			if (range != null)
			{
				return range;
			}
		}
		return null;
	}

	public IRange FindFirst(double findValue, OfficeFindType flags)
	{
		bool num = (flags & OfficeFindType.FormulaValue) == OfficeFindType.FormulaValue;
		bool flag = (flags & OfficeFindType.Number) == OfficeFindType.Number;
		if (!(num || flag))
		{
			throw new ArgumentException("Parameter flag is not valid.", "flags");
		}
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			IRange range = innerList[i].FindFirst(findValue, flags);
			if (range != null)
			{
				return range;
			}
		}
		return null;
	}

	public IRange FindFirst(bool findValue)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			IRange range = innerList[i].FindFirst(findValue);
			if (range != null)
			{
				return range;
			}
		}
		return null;
	}

	public IRange FindFirst(DateTime findValue)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			IRange range = innerList[i].FindFirst(findValue);
			if (range != null)
			{
				return range;
			}
		}
		return null;
	}

	public IRange FindFirst(TimeSpan findValue)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			IRange range = innerList[i].FindFirst(findValue);
			if (range != null)
			{
				return range;
			}
		}
		return null;
	}

	public IRange[] FindAll(string findValue, OfficeFindType flags)
	{
		return FindAll(findValue, flags, OfficeFindOptions.None);
	}

	public IRange[] FindAll(string findValue, OfficeFindType flags, OfficeFindOptions findOptions)
	{
		if (findValue == null)
		{
			return null;
		}
		bool num = (flags & OfficeFindType.Formula) == OfficeFindType.Formula;
		bool flag = (flags & OfficeFindType.Text) == OfficeFindType.Text;
		bool flag2 = (flags & OfficeFindType.FormulaStringValue) == OfficeFindType.FormulaStringValue;
		bool flag3 = (flags & OfficeFindType.Error) == OfficeFindType.Error;
		if (!(num || flag || flag2 || flag3))
		{
			throw new ArgumentException("Parameter flag is not valid.", "flags");
		}
		List<IRange> list = new List<IRange>();
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			IRange[] array = innerList[i].FindAll(findValue, flags, findOptions);
			if (array != null)
			{
				list.AddRange(array);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list.ToArray();
	}

	public IRange[] FindAll(double findValue, OfficeFindType flags)
	{
		bool num = (flags & OfficeFindType.FormulaValue) == OfficeFindType.FormulaValue;
		bool flag = (flags & OfficeFindType.Number) == OfficeFindType.Number;
		if (!(num || flag))
		{
			throw new ArgumentException("Parameter flag is not valid.", "flags");
		}
		List<IRange> list = new List<IRange>();
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			IRange[] array = innerList[i].FindAll(findValue, flags);
			if (array != null)
			{
				list.AddRange(array);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list.ToArray();
	}

	public IRange[] FindAll(bool findValue)
	{
		List<IRange> list = new List<IRange>();
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			IRange[] array = innerList[i].FindAll(findValue);
			if (array != null)
			{
				list.AddRange(array);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list.ToArray();
	}

	public IRange[] FindAll(DateTime findValue)
	{
		List<IRange> list = new List<IRange>();
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			IRange[] array = innerList[i].FindAll(findValue);
			if (array != null)
			{
				list.AddRange(array);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list.ToArray();
	}

	public IRange[] FindAll(TimeSpan findValue)
	{
		List<IRange> list = new List<IRange>();
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			IRange[] array = innerList[i].FindAll(findValue);
			if (array != null)
			{
				list.AddRange(array);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list.ToArray();
	}

	public IWorksheet Add(string sheetName)
	{
		IWorksheet worksheet = base.AppImplementation.CreateWorksheet(this);
		((WorksheetImpl)worksheet).RealIndex = m_book.ObjectCount;
		worksheet.Name = sheetName;
		return Add(worksheet);
	}

	public IWorksheet AddCopy(int sheetIndex)
	{
		return AddCopy(sheetIndex, OfficeWorksheetCopyFlags.CopyAll);
	}

	public IWorksheet AddCopy(int sheetIndex, OfficeWorksheetCopyFlags flags)
	{
		return AddCopy(this[sheetIndex], flags, isLocal: true);
	}

	public IWorksheet AddCopy(IWorksheet sheet)
	{
		return AddCopy(sheet, OfficeWorksheetCopyFlags.CopyAll);
	}

	public IWorksheet AddCopy(IWorksheet sheet, OfficeWorksheetCopyFlags flags)
	{
		return AddCopy(sheet, flags, isLocal: false);
	}

	private IWorksheet AddCopy(IWorksheet sheet, OfficeWorksheetCopyFlags flags, bool isLocal)
	{
		WorksheetImpl worksheetImpl = (WorksheetImpl)sheet;
		OfficeWorksheetVisibility visibility = worksheetImpl.Visibility;
		Dictionary<string, string> dictionary;
		WorksheetImpl worksheetImpl2;
		if (isLocal || sheet.Workbook.Worksheets == this)
		{
			worksheetImpl2 = base.AppImplementation.CreateWorksheet(this);
			dictionary = new Dictionary<string, string>(1);
			worksheetImpl2.Name = CollectionBaseEx<IWorksheet>.GenerateDefaultName(base.List, worksheetImpl.Name + "_");
			dictionary.Add(worksheetImpl.Name, worksheetImpl2.Name);
			Add(worksheetImpl2);
			worksheetImpl2.CopyFrom(worksheetImpl, new Dictionary<string, string>(), dictionary, null, flags);
			return worksheetImpl2;
		}
		OfficeVersion version = sheet.Workbook.Version;
		if (version != m_book.Version && version > m_book.Version)
		{
			throw new InvalidOperationException("Operation is not valid due to a mismatch in the document version");
		}
		OfficeVersion version2 = worksheetImpl.Version;
		if ((worksheetImpl.Workbook as WorkbookImpl).IsConverted && m_book.IsCreated && !m_book.IsConverted)
		{
			worksheetImpl.Workbook.Version = OfficeVersion.Excel97to2003;
		}
		Dictionary<int, int> dicFontIndexes;
		Dictionary<int, int> hashExtFormatIndexes;
		Dictionary<string, string> hashStyleNames = m_book.InnerStyles.Merge(sheet.Workbook, ExcelStyleMergeOptions.CreateDiffName, out dicFontIndexes, out hashExtFormatIndexes);
		WorkbookImpl workbookImpl = (WorkbookImpl)worksheetImpl.Workbook;
		if ((flags & OfficeWorksheetCopyFlags.CopyPalette) != 0)
		{
			workbookImpl.CopyPaletteColorTo(m_book);
		}
		if (workbookImpl.DefaultThemeVersion != null)
		{
			m_book.DefaultThemeVersion = workbookImpl.DefaultThemeVersion;
		}
		dictionary = new Dictionary<string, string>();
		Dictionary<int, int> hashNameIndexes = new Dictionary<int, int>();
		Dictionary<int, int> hashSubBooks = m_book.ExternWorkbooks.AddCopy(workbookImpl.ExternWorkbooks);
		Dictionary<int, int> hashExternSheets = m_book.CopyExternSheets(workbookImpl.ExternSheet, hashSubBooks);
		worksheetImpl2 = AddWorksheet(sheet.Name, dictionary);
		worksheetImpl2.CopyFrom(worksheetImpl, hashStyleNames, dictionary, dicFontIndexes, flags, hashExtFormatIndexes, hashNameIndexes, hashExternSheets);
		if (flags == OfficeWorksheetCopyFlags.CopyShapes)
		{
			CopyControlsData(worksheetImpl, flags);
		}
		OfficeVersion version3 = worksheetImpl2.Version;
		OfficeVersion version4 = m_book.Version;
		if (version3 != version4)
		{
			worksheetImpl2.Version = version4;
		}
		if (visibility != worksheetImpl2.Visibility)
		{
			worksheetImpl2.Visibility = visibility;
		}
		if (sheet.Workbook.Version != version2)
		{
			sheet.Workbook.Version = version2;
		}
		return worksheetImpl2;
	}

	private void CopyControlsData(WorksheetImpl oldSheet, OfficeWorksheetCopyFlags flags)
	{
		if ((flags & OfficeWorksheetCopyFlags.CopyShapes) == 0)
		{
			return;
		}
		WorkbookImpl workbookImpl = oldSheet.Workbook as WorkbookImpl;
		if (!ContainsActiveX(oldSheet))
		{
			return;
		}
		Stream controlsStream = workbookImpl.ControlsStream;
		if (controlsStream != null)
		{
			Stream controlsStream2 = m_book.ControlsStream;
			if (controlsStream2 != null)
			{
				controlsStream.Position = 0L;
				controlsStream2.Position = controlsStream2.Length;
				UtilityMethods.CopyStreamTo(controlsStream, controlsStream2);
			}
			else
			{
				m_book.ControlsStream = controlsStream;
			}
		}
	}

	private bool ContainsActiveX(IWorksheet sheet)
	{
		foreach (ShapeImpl item in sheet.Shapes as ShapesCollection)
		{
			if (item.IsActiveX)
			{
				return true;
			}
		}
		return false;
	}

	public void AddCopy(IWorksheets worksheets)
	{
		AddCopy(worksheets, OfficeWorksheetCopyFlags.CopyAll);
	}

	public void AddCopy(IWorksheets worksheets, OfficeWorksheetCopyFlags flags)
	{
		if (worksheets == null)
		{
			throw new ArgumentNullException("worksheets");
		}
		int count = worksheets.Count;
		WorkbookImpl book = ((WorksheetsCollection)worksheets).m_book;
		if (worksheets == this)
		{
			for (int i = 0; i < count; i++)
			{
				AddCopy(i);
			}
			return;
		}
		WorksheetImpl[] array = new WorksheetImpl[count];
		Dictionary<string, string> hashWorksheetNames = new Dictionary<string, string>();
		Dictionary<int, int> hashNameIndexes = new Dictionary<int, int>();
		Dictionary<int, int> hashSubBooks = m_book.ExternWorkbooks.AddCopy(book.ExternWorkbooks);
		Dictionary<int, int> hashExternSheets = m_book.CopyExternSheets(book.ExternSheet, hashSubBooks);
		Dictionary<int, int> dicFontIndexes;
		Dictionary<int, int> hashExtFormatIndexes;
		Dictionary<string, string> hashStyleNames = m_book.InnerStyles.Merge(worksheets[0].Workbook, ExcelStyleMergeOptions.CreateDiffName, out dicFontIndexes, out hashExtFormatIndexes);
		for (int j = 0; j < count; j++)
		{
			array[j] = AddWorksheet(worksheets[j].Name, hashWorksheetNames);
		}
		if ((flags & OfficeWorksheetCopyFlags.CopyNames) != 0)
		{
			for (int k = 0; k < count; k++)
			{
				array[k].CopyFrom((WorksheetImpl)worksheets[k], hashStyleNames, hashWorksheetNames, dicFontIndexes, OfficeWorksheetCopyFlags.CopyNames, hashExtFormatIndexes, hashNameIndexes, hashExternSheets);
			}
			flags &= ~OfficeWorksheetCopyFlags.CopyNames;
		}
		for (int l = 0; l < count; l++)
		{
			array[l].CopyFrom((WorksheetImpl)worksheets[l], hashStyleNames, hashWorksheetNames, dicFontIndexes, flags, hashExtFormatIndexes, hashNameIndexes);
		}
	}

	private WorksheetImpl AddWorksheet(string strSuggestedName, Dictionary<string, string> hashWorksheetNames)
	{
		WorksheetImpl worksheetImpl = base.AppImplementation.CreateWorksheet(this);
		string text = strSuggestedName;
		if (this[strSuggestedName] != null)
		{
			if (hashWorksheetNames == null)
			{
				hashWorksheetNames = new Dictionary<string, string>();
			}
			text = CollectionBaseEx<IWorksheet>.GenerateDefaultName(base.List, text + "_");
			hashWorksheetNames.Add(strSuggestedName, text);
		}
		worksheetImpl.Name = text;
		Add(worksheetImpl);
		return worksheetImpl;
	}

	public IWorksheet AddCopyBefore(IWorksheet toCopy)
	{
		return AddCopyBefore(toCopy, toCopy);
	}

	public IWorksheet AddCopyBefore(IWorksheet toCopy, IWorksheet sheetAfter)
	{
		if (toCopy == null)
		{
			throw new ArgumentNullException("toCopy");
		}
		if (sheetAfter == null)
		{
			throw new ArgumentNullException("sheetAfter");
		}
		int index = sheetAfter.Index;
		IWorksheet worksheet = AddCopy(toCopy);
		worksheet.Move(index);
		worksheet.Activate();
		return worksheet;
	}

	public IWorksheet AddCopyAfter(IWorksheet toCopy)
	{
		return AddCopyAfter(toCopy, toCopy);
	}

	public IWorksheet AddCopyAfter(IWorksheet toCopy, IWorksheet sheetBefore)
	{
		if (toCopy == null)
		{
			throw new ArgumentNullException("toCopy");
		}
		if (sheetBefore == null)
		{
			throw new ArgumentNullException("sheetBefore");
		}
		int index = sheetBefore.Index;
		IWorksheet worksheet = AddCopy(toCopy);
		worksheet.Move(index + 1);
		worksheet.Activate();
		return worksheet;
	}

	protected override void OnInsertComplete(int index, IWorksheet value)
	{
		(value as WorksheetImpl).NameChanged += sheet_NameChanged;
		if (m_bUseHash)
		{
			m_list[value.Name] = value;
		}
		base.OnInsertComplete(index, value);
	}

	protected override void OnSetComplete(int index, IWorksheet oldValue, IWorksheet newValue)
	{
		(oldValue as WorksheetImpl).NameChanged -= sheet_NameChanged;
		if (m_bUseHash)
		{
			m_list.Remove(oldValue.Name);
			m_list[newValue.Name] = newValue;
		}
		base.OnSetComplete(index, oldValue, newValue);
	}

	protected override void OnRemoveComplete(int index, IWorksheet value)
	{
		(value as WorksheetImpl).NameChanged -= sheet_NameChanged;
		if (m_bUseHash)
		{
			m_list.Remove(value.Name);
		}
		base.OnRemoveComplete(index, value);
	}

	protected override void OnClearComplete()
	{
		base.OnClearComplete();
		m_list.Clear();
	}

	private void sheet_NameChanged(object sender, ValueChangedEventArgs e)
	{
		if (m_bUseHash)
		{
			if (m_list.ContainsKey((string)e.newValue))
			{
				throw new ArgumentException("Name of worksheet must be unique in a workbook.");
			}
			m_list.Remove((string)e.oldValue);
			m_list[(string)e.newValue] = (IWorksheet)sender;
		}
	}

	public IWorksheet Create(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name.Length < 1 || ValidateSheetName(name))
		{
			throw new ArgumentException("Sheet Name is InValid");
		}
		IWorksheet worksheet = base.AppImplementation.CreateWorksheet(this);
		worksheet.Name = name;
		return Add(worksheet);
	}

	public IWorksheet Create()
	{
		IWorksheet worksheet = base.AppImplementation.CreateWorksheet(this);
		int num = base.InnerList.Count;
		string text = "Sheet" + num;
		while (this[text] != null)
		{
			num++;
			text = "Sheet" + num;
		}
		worksheet.Name = text;
		return Add(worksheet);
	}

	public new void Remove(IWorksheet sheet)
	{
		if (!base.InnerList.Contains(sheet))
		{
			throw new ArgumentOutOfRangeException("Worksheets collection does not contain specified worksheet.");
		}
		if (m_book.Objects.Count == 1)
		{
			throw new ArgumentException("Workbook must contains at least one worksheet. You cannot remove last worksheet.", "sheet");
		}
		sheet.Remove();
	}

	public void Remove(string sheetName)
	{
		Remove(this[sheetName]);
	}

	public void Remove(int index)
	{
		Remove(this[index]);
	}

	public void UpdateStringIndexes(List<int> arrNewIndexes)
	{
		List<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			((WorksheetImpl)innerList[i]).UpdateStringIndexes(arrNewIndexes);
		}
	}

	public void InnerRemove(int index)
	{
		int count = base.Count;
		if (index < 0 || index > count - 1)
		{
			throw new ArgumentOutOfRangeException("index", "Value cannot be less than 0 and greater than Count - 1.");
		}
		if (m_book.Objects.Count == 1)
		{
			throw new ArgumentException("Workbook at least must contains one worksheet. You cannot remove last worksheet.", "sheet");
		}
		IWorksheet worksheet = this[index];
		int realIndex = ((ISerializableNamedObject)worksheet).RealIndex;
		RemoveAt(index);
		WorkbookObjectsCollection objects = m_book.Objects;
		objects.RemoveAt(realIndex);
		int i = realIndex;
		for (int count2 = objects.Count; i < count2; i++)
		{
			objects[i].RealIndex = i;
		}
		if (m_book.ActiveSheet == worksheet)
		{
			m_book.SetActiveWorksheet(this[0] as WorksheetBaseImpl);
		}
		for (int j = index + 1; j < count; j++)
		{
			((WorksheetImpl)this[j - 1]).Index = j - 1;
		}
	}

	public void InnerAdd(IWorksheet sheet)
	{
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		base.Add(sheet);
	}

	private static bool ValidateSheetName(string sheetName)
	{
		string[] array = sheetNameValidators;
		foreach (string value in array)
		{
			if (sheetName.Contains(value))
			{
				return true;
			}
		}
		return false;
	}

	private void Objects_TabSheetMoved(object sender, TabSheetMovedEventArgs args)
	{
		ITabSheets obj = (ITabSheets)sender;
		int newIndex = args.NewIndex;
		if (obj[newIndex] is WorksheetImpl sheet)
		{
			int oldIndex = args.OldIndex;
			UpdateSheetIndex(sheet, oldIndex);
		}
	}
}
