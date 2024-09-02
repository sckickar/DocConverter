using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.Drawing;
using DocGen.OfficeChart.Calculate;
using DocGen.OfficeChart.Interfaces;

namespace DocGen.OfficeChart.Implementation.Collections.Grouping;

internal class WorksheetGroup : CollectionBaseEx<IWorksheet>, IWorksheetGroup, IWorksheet, ITabSheet, IParentApplication, ICalcData, ICloneParent
{
	private CalcEngine m_calcEngine;

	private WorkbookImpl m_book;

	private PageSetupGroup m_pageSetup;

	private IRange m_usedRange;

	private IMigrantRange m_migrantRange;

	private OfficeSheetView m_view;

	internal int unknown_formula_name = 9;

	public bool IsEmpty => base.Count == 0;

	public WorkbookImpl ParentWorkbook => m_book;

	internal CalcEngine CalcEngine
	{
		get
		{
			return m_calcEngine;
		}
		set
		{
			m_calcEngine = value;
		}
	}

	public IWorkbook Workbook => m_book;

	public IRange[] Cells => null;

	public OfficeSheetView View
	{
		get
		{
			return m_view;
		}
		set
		{
			m_view = value;
		}
	}

	public bool DisplayPageBreaks
	{
		get
		{
			IList<IWorksheet> innerList = base.InnerList;
			bool displayPageBreaks = innerList[0].DisplayPageBreaks;
			int i = 1;
			for (int count = innerList.Count; i < count; i++)
			{
				if (innerList[i].DisplayPageBreaks != displayPageBreaks)
				{
					return false;
				}
			}
			return displayPageBreaks;
		}
		set
		{
			IList<IWorksheet> innerList = base.InnerList;
			int i = 0;
			for (int count = innerList.Count; i < count; i++)
			{
				innerList[i].DisplayPageBreaks = value;
			}
		}
	}

	public bool HasOleObject
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public int Index => -1;

	public int TabIndex => -1;

	public bool ProtectDrawingObjects => false;

	public bool ProtectScenarios => false;

	public IRange[] MergedCells => null;

	public string Name
	{
		get
		{
			return null;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public INames Names
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public string CodeName
	{
		get
		{
			return null;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public IPageSetup PageSetup
	{
		get
		{
			if (m_pageSetup == null)
			{
				m_pageSetup = new PageSetupGroup(base.Application, this);
			}
			return m_pageSetup;
		}
	}

	public IRange Range => UsedRange;

	public IRange[] Rows => null;

	public IRange[] Columns => null;

	public double StandardHeight
	{
		get
		{
			return 0.0;
		}
		set
		{
		}
	}

	public bool StandardHeightFlag
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public double StandardWidth
	{
		get
		{
			return 0.0;
		}
		set
		{
		}
	}

	public OfficeSheetType Type
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public IRange UsedRange
	{
		get
		{
			if (IsEmpty)
			{
				return null;
			}
			WorksheetImpl obj = (WorksheetImpl)base.InnerList[0];
			int firstRow = obj.FirstRow;
			int lastRow = obj.LastRow;
			int firstColumn = obj.FirstColumn;
			int lastColumn = obj.LastColumn;
			if (m_usedRange == null || m_usedRange.Row != firstRow || m_usedRange.Column != firstColumn || m_usedRange.LastRow != lastRow || m_usedRange.LastColumn != lastColumn)
			{
				m_usedRange = new RangeGroup(base.Application, this, firstRow, firstColumn, lastRow, lastColumn);
			}
			return m_usedRange;
		}
	}

	public int Zoom
	{
		get
		{
			IList<IWorksheet> innerList = base.InnerList;
			int zoom = innerList[0].Zoom;
			int i = 1;
			for (int count = innerList.Count; i < count; i++)
			{
				if (innerList[i].Zoom != zoom)
				{
					return int.MinValue;
				}
			}
			return zoom;
		}
		set
		{
			IList<IWorksheet> innerList = base.InnerList;
			int i = 0;
			for (int count = innerList.Count; i < count; i++)
			{
				innerList[i].Zoom = value;
			}
		}
	}

	public OfficeWorksheetVisibility Visibility
	{
		get
		{
			IList<IWorksheet> innerList = base.InnerList;
			OfficeWorksheetVisibility visibility = innerList[0].Visibility;
			int i = 1;
			for (int count = innerList.Count; i < count; i++)
			{
				if (innerList[i].Visibility != visibility)
				{
					return OfficeWorksheetVisibility.Visible;
				}
			}
			return visibility;
		}
		set
		{
			IList<IWorksheet> innerList = base.InnerList;
			int i = 0;
			for (int count = innerList.Count; i < count; i++)
			{
				innerList[i].Visibility = value;
			}
		}
	}

	public int VerticalSplit
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public int HorizontalSplit
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public int FirstVisibleRow
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public int FirstVisibleColumn
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public int ActivePane
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public bool IsDisplayZeros
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public bool IsGridLinesVisible
	{
		get
		{
			IList<IWorksheet> innerList = base.InnerList;
			bool isGridLinesVisible = innerList[0].IsGridLinesVisible;
			int i = 1;
			for (int count = innerList.Count; i < count; i++)
			{
				if (innerList[i].IsGridLinesVisible != isGridLinesVisible)
				{
					return false;
				}
			}
			return isGridLinesVisible;
		}
		set
		{
			IList<IWorksheet> innerList = base.InnerList;
			int i = 0;
			for (int count = innerList.Count; i < count; i++)
			{
				innerList[i].IsGridLinesVisible = value;
			}
		}
	}

	public OfficeKnownColors GridLineColor
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public bool IsRowColumnHeadersVisible
	{
		get
		{
			IList<IWorksheet> innerList = base.InnerList;
			bool isRowColumnHeadersVisible = innerList[0].IsRowColumnHeadersVisible;
			int i = 1;
			for (int count = innerList.Count; i < count; i++)
			{
				if (innerList[i].IsRowColumnHeadersVisible != isRowColumnHeadersVisible)
				{
					return false;
				}
			}
			return isRowColumnHeadersVisible;
		}
		set
		{
			IList<IWorksheet> innerList = base.InnerList;
			int i = 0;
			for (int count = innerList.Count; i < count; i++)
			{
				innerList[i].IsRowColumnHeadersVisible = value;
			}
		}
	}

	public bool IsStringsPreserved
	{
		get
		{
			IList<IWorksheet> innerList = base.InnerList;
			bool isStringsPreserved = innerList[0].IsStringsPreserved;
			int i = 1;
			for (int count = innerList.Count; i < count; i++)
			{
				if (innerList[i].IsStringsPreserved != isStringsPreserved)
				{
					return false;
				}
			}
			return isStringsPreserved;
		}
		set
		{
			IList<IWorksheet> innerList = base.InnerList;
			int i = 0;
			for (int count = innerList.Count; i < count; i++)
			{
				innerList[i].IsStringsPreserved = value;
			}
		}
	}

	public bool IsPasswordProtected => false;

	public IRange this[int row, int column] => null;

	public IRange this[int row, int column, int lastRow, int lastColumn] => null;

	public IRange this[string name] => null;

	public IRange this[string name, bool IsR1C1Notation] => null;

	public IRange[] UsedCells
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public IWorksheetCustomProperties CustomProperties
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public IMigrantRange MigrantRange
	{
		get
		{
			if (m_migrantRange == null)
			{
				CreateMigrantRange();
			}
			return m_migrantRange;
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
			IList<IWorksheet> innerList = base.InnerList;
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
			IList<IWorksheet> innerList = base.InnerList;
			int i = 0;
			for (int count = innerList.Count; i < count; i++)
			{
				innerList[i].UseRangesCache = value;
			}
		}
	}

	public OfficeSheetProtection Protection
	{
		get
		{
			throw new NotSupportedException("This property doesnot support in this case.");
		}
	}

	public bool ProtectContents
	{
		get
		{
			throw new NotSupportedException("This property doesnot supported yet.");
		}
	}

	public int TopVisibleRow
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public int LeftVisibleColumn
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public bool UsedRangeIncludesFormatting
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public bool IsFreezePanes
	{
		get
		{
			IList<IWorksheet> innerList = base.InnerList;
			int i = 0;
			for (int count = innerList.Count; i < count; i++)
			{
				if (!innerList[i].IsFreezePanes)
				{
					return false;
				}
			}
			return true;
		}
	}

	public IRange SplitCell
	{
		get
		{
			throw new NotImplementedException("Split Cell");
		}
	}

	public OfficeKnownColors TabColor
	{
		get
		{
			IList<IWorksheet> innerList = base.InnerList;
			OfficeKnownColors tabColor = innerList[0].TabColor;
			int i = 1;
			for (int count = innerList.Count; i < count; i++)
			{
				if (innerList[i].TabColor != tabColor)
				{
					return OfficeKnownColors.Black;
				}
			}
			return tabColor;
		}
		set
		{
			IList<IWorksheet> innerList = base.InnerList;
			int i = 0;
			for (int count = innerList.Count; i < count; i++)
			{
				innerList[i].TabColor = value;
			}
		}
	}

	public Color TabColorRGB
	{
		get
		{
			IList<IWorksheet> innerList = base.InnerList;
			Color tabColorRGB = innerList[0].TabColorRGB;
			int i = 1;
			for (int count = innerList.Count; i < count; i++)
			{
				if (innerList[i].TabColorRGB != tabColorRGB)
				{
					return ColorExtension.Empty;
				}
			}
			return tabColorRGB;
		}
		set
		{
		}
	}

	public IOfficeChartShapes Charts
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public IShapes Shapes
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public bool IsRightToLeft
	{
		get
		{
			IList<IWorksheet> innerList = base.InnerList;
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
			IList<IWorksheet> innerList = base.InnerList;
			int i = 0;
			for (int count = innerList.Count; i < count; i++)
			{
				innerList[i].IsRightToLeft = value;
			}
		}
	}

	public bool IsSelected => true;

	public ITextBoxes TextBoxes
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public event RangeImpl.CellValueChangedEventHandler CellValueChanged;

	public event MissingFunctionEventHandler MissingFunction;

	public event DocGen.OfficeChart.Calculate.ValueChangedEventHandler ValueChanged;

	public WorksheetGroup(IApplication application, object parent)
		: base(application, parent)
	{
		FindParents();
		base.Inserted += WorksheetGroup_Inserted;
		base.Removing += WorksheetGroup_Removing;
		base.Clearing += WorksheetGroup_Clearing;
		IWorksheets worksheets = m_book.Worksheets;
		int i = 0;
		for (int count = worksheets.Count; i < count; i++)
		{
			IWorksheet worksheet = worksheets[i];
			if (worksheet.IsSelected)
			{
				base.Add(worksheet);
			}
		}
	}

	private void FindParents()
	{
		m_book = FindParent(typeof(WorkbookImpl)) as WorkbookImpl;
		if (m_book == null)
		{
			throw new ArgumentOutOfRangeException("parent", "Can't find parent workbook");
		}
	}

	public override object Clone(object parent)
	{
		WorksheetGroup worksheetGroup = new WorksheetGroup(base.Application, parent);
		IList<IWorksheet> innerList = base.InnerList;
		IList<IWorksheet> innerList2 = worksheetGroup.InnerList;
		WorkbookObjectsCollection objects = worksheetGroup.m_book.Objects;
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			int realIndex = ((WorksheetBaseImpl)innerList[i]).RealIndex;
			innerList2.Add(objects[realIndex] as IWorksheet);
		}
		return worksheetGroup;
	}

	public int Add(ITabSheet sheet)
	{
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (sheet.Workbook != m_book)
		{
			throw new ArgumentOutOfRangeException("sheet", "Worksheets from different workbooks can't be grouped.");
		}
		if (sheet.IsSelected && !m_book.IsWorkbookOpening)
		{
			return -1;
		}
		if (((WorksheetBaseImpl)sheet).WindowTwo.IsPaged)
		{
			m_book.SetActiveWorksheet(sheet as WorksheetBaseImpl);
		}
		if (sheet is IWorksheet)
		{
			base.Add(sheet as IWorksheet);
			return base.Count - 1;
		}
		return -1;
	}

	public void Remove(ITabSheet sheet)
	{
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (base.Count > 1)
		{
			base.Remove(sheet as IWorksheet);
			WorksheetBaseImpl activeWorksheet = base.List[0] as WorksheetBaseImpl;
			m_book.SetActiveWorksheet(activeWorksheet);
			base.AppImplementation.SetActiveWorksheet(activeWorksheet);
		}
	}

	public void Select(ITabSheet sheet)
	{
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		Clear();
		Add(sheet);
	}

	private void CreateMigrantRange()
	{
		m_migrantRange = new MigrantRangeGroup(base.Application, this);
	}

	public object GetValueRowCol(int row, int col)
	{
		IRange range = this[row, col];
		if (range.HasFormula)
		{
			return range.Formula;
		}
		return range.Value;
	}

	public void SetValueRowCol(object value, int row, int col)
	{
		if (value != null)
		{
			SetValue(row, col, value.ToString());
		}
	}

	public void WireParentObject()
	{
	}

	internal void EnableSheetCalculations()
	{
		if (CalcEngine != null)
		{
			return;
		}
		CalcEngine = new CalcEngine(this);
		CalcEngine.PreserveFormula = true;
		int sheetFamilyID = CalcEngine.CreateSheetFamilyID();
		string text = "!";
		foreach (IWorksheet worksheet in ParentWorkbook.Worksheets)
		{
			if ((worksheet as WorksheetImpl).CalcEngine == null)
			{
				(worksheet as WorksheetImpl).CalcEngine = new CalcEngine(worksheet);
			}
			CalcEngine.RegisterGridAsSheet(worksheet.Name, worksheet, sheetFamilyID);
			(worksheet as WorksheetImpl).CalcEngine.UnknownFunction += CalcEngine_UnknownFunction;
			text = text + worksheet.Name + "!";
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (IName name in ParentWorkbook.Names)
		{
			if (name.Scope.Length > 0 && text.IndexOf("!" + name.Scope + "!") > -1)
			{
				dictionary.Add((name.Scope + "!" + name.Name).ToUpper(), name.Value.Replace("'", ""));
			}
			else
			{
				dictionary.Add(name.Name.ToUpper(), name.Value.Replace("'", ""));
			}
		}
		Dictionary<object, object> dictionary2 = new Dictionary<object, object>();
		if (dictionary != null)
		{
			foreach (string key in dictionary.Keys)
			{
				dictionary2.Add(key.ToUpper(), dictionary[key]);
			}
		}
		foreach (IWorksheet worksheet2 in ParentWorkbook.Worksheets)
		{
			(worksheet2 as WorksheetImpl).CalcEngine.NamedRanges = dictionary2;
		}
	}

	internal void DisableSheetCalculations()
	{
		if (CalcEngine == null || ParentWorkbook == null || ParentWorkbook.Worksheets == null)
		{
			return;
		}
		foreach (IWorksheet worksheet in ParentWorkbook.Worksheets)
		{
			if ((worksheet as WorksheetImpl).CalcEngine != null)
			{
				(worksheet as WorksheetImpl).CalcEngine.UnknownFunction -= CalcEngine_UnknownFunction;
				(worksheet as WorksheetImpl).CalcEngine.Dispose();
			}
			(worksheet as WorksheetImpl).CalcEngine = null;
		}
	}

	internal void Calculate()
	{
		throw new NotImplementedException();
	}

	private void CalcEngine_UnknownFunction(object sender, UnknownFunctionEventArgs args)
	{
		if (this.MissingFunction != null && CalcEngine != null)
		{
			MissingFunctionEventArgs missingFunctionEventArgs = new MissingFunctionEventArgs();
			missingFunctionEventArgs.MissingFunctionName = args.MissingFunctionName;
			missingFunctionEventArgs.CellLocation = args.CellLocation;
			this.MissingFunction(this, missingFunctionEventArgs);
		}
	}

	internal void OnValueChanged(int row, int col, string value)
	{
		if (this.ValueChanged != null)
		{
			DocGen.OfficeChart.Calculate.ValueChangedEventArgs e = new DocGen.OfficeChart.Calculate.ValueChangedEventArgs(row, col, value);
			this.ValueChanged(this, e);
		}
	}

	public void Activate()
	{
		throw new NotSupportedException();
	}

	public void CopyToClipboard()
	{
		throw new NotSupportedException();
	}

	void IWorksheet.Clear()
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].Clear();
		}
	}

	public void ClearData()
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].ClearData();
		}
	}

	public bool Contains(int iRow, int iColumn)
	{
		IList<IWorksheet> innerList = base.InnerList;
		bool flag = innerList[0].Contains(iRow, iColumn);
		if (!flag)
		{
			return flag;
		}
		int i = 1;
		for (int count = innerList.Count; i < count; i++)
		{
			if (innerList[i].Contains(iRow, iColumn) != flag)
			{
				return false;
			}
		}
		return flag;
	}

	public IRanges CreateRangesCollection()
	{
		return null;
	}

	public void CreateNamedRanges(string namedRange, string referRange, bool vertical)
	{
		throw new NotImplementedException();
	}

	public bool IsColumnVisible(int columnIndex)
	{
		IList<IWorksheet> innerList = base.InnerList;
		bool flag = innerList[0].IsColumnVisible(columnIndex);
		int i = 1;
		for (int count = innerList.Count; i < count; i++)
		{
			if (innerList[i].IsColumnVisible(columnIndex) != flag)
			{
				return false;
			}
		}
		return flag;
	}

	public void ShowColumn(int columnIndex, bool isVisible)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].ShowColumn(columnIndex, isVisible);
		}
	}

	public void HideColumn(int columnIndex)
	{
		ShowColumn(columnIndex, isVisible: false);
	}

	public void HideRow(int rowIndex)
	{
		ShowRow(rowIndex, isVisible: false);
	}

	public bool IsRowVisible(int rowIndex)
	{
		IList<IWorksheet> innerList = base.InnerList;
		bool flag = innerList[0].IsRowVisible(rowIndex);
		int i = 1;
		for (int count = innerList.Count; i < count; i++)
		{
			if (innerList[i].IsRowVisible(rowIndex) != flag)
			{
				return false;
			}
		}
		return flag;
	}

	public void ShowRow(int rowIndex, bool isVisible)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].ShowRow(rowIndex, isVisible);
		}
	}

	public void ShowRange(IRange range, bool isVisible)
	{
		foreach (IWorksheet item in (IEnumerable<IWorksheet>)base.InnerList)
		{
			item.ShowRange(range, isVisible);
		}
	}

	public void ShowRange(RangesCollection ranges, bool isVisible)
	{
		if (ranges.Count == 0)
		{
			return;
		}
		foreach (IRange range in ranges)
		{
			ShowRange(range, isVisible);
		}
	}

	public void ShowRange(IRange[] ranges, bool isVisible)
	{
		if (ranges.Length != 0)
		{
			RangesCollection rangesCollection = new RangesCollection(base.Application, this);
			foreach (IRange range in ranges)
			{
				rangesCollection.Add(range);
			}
			ShowRange(rangesCollection, isVisible);
		}
	}

	public void InsertRow(int index)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].InsertRow(index);
		}
	}

	public void InsertRow(int iRowIndex, int iRowCount)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].InsertRow(iRowIndex, iRowCount);
		}
	}

	public void InsertRow(int iRowIndex, int iRowCount, OfficeInsertOptions insertOptions)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].InsertRow(iRowIndex, iRowCount, insertOptions);
		}
	}

	public void InsertColumn(int index)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].InsertColumn(index);
		}
	}

	public void InsertColumn(int iColumnIndex, int iColumnCount)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].InsertColumn(iColumnIndex, iColumnCount);
		}
	}

	public void InsertColumn(int iColumnIndex, int iColumnCount, OfficeInsertOptions options)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].InsertColumn(iColumnIndex, iColumnCount, options);
		}
	}

	public void DeleteRow(int index)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].DeleteRow(index);
		}
	}

	public void DeleteRow(int index, int count)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count2 = innerList.Count; i < count2; i++)
		{
			innerList[i].DeleteRow(index, count);
		}
	}

	public void DeleteColumn(int index)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].DeleteColumn(index);
		}
	}

	public void DeleteColumn(int index, int count)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count2 = innerList.Count; i < count2; i++)
		{
			innerList[i].DeleteColumn(index, count);
		}
	}

	public int ImportArray(object[] arrObject, int firstRow, int firstColumn, bool isVertical)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int result = 0;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			result = innerList[i].ImportArray(arrObject, firstRow, firstColumn, isVertical);
		}
		return result;
	}

	public int ImportArray(string[] arrString, int firstRow, int firstColumn, bool isVertical)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int result = 0;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			result = innerList[i].ImportArray(arrString, firstRow, firstColumn, isVertical);
		}
		return result;
	}

	public int ImportArray(int[] arrInt, int firstRow, int firstColumn, bool isVertical)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int result = 0;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			result = innerList[i].ImportArray(arrInt, firstRow, firstColumn, isVertical);
		}
		return result;
	}

	public int ImportArray(double[] arrDouble, int firstRow, int firstColumn, bool isVertical)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int result = 0;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			result = innerList[i].ImportArray(arrDouble, firstRow, firstColumn, isVertical);
		}
		return result;
	}

	public int ImportArray(DateTime[] arrDateTime, int firstRow, int firstColumn, bool isVertical)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int result = 0;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			result = innerList[i].ImportArray(arrDateTime, firstRow, firstColumn, isVertical);
		}
		return result;
	}

	public int ImportArray(object[,] arrObject, int firstRow, int firstColumn)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int result = 0;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			result = innerList[i].ImportArray(arrObject, firstRow, firstColumn);
		}
		return result;
	}

	public int ImportData(IEnumerable arrObject, int firstRow, int firstColumn, bool includeHeader)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int result = 0;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			result = innerList[i].ImportData(arrObject, firstRow, firstColumn, includeHeader);
		}
		return result;
	}

	public void RemovePanes()
	{
		throw new NotSupportedException();
	}

	public void Protect(string password)
	{
		throw new NotSupportedException();
	}

	public void Protect(string password, OfficeSheetProtection options)
	{
		throw new NotSupportedException();
	}

	public void Unprotect(string password)
	{
		throw new NotSupportedException();
	}

	public IRange IntersectRanges(IRange range1, IRange range2)
	{
		return null;
	}

	public IRange MergeRanges(IRange range1, IRange range2)
	{
		return null;
	}

	public void AutofitRow(int rowIndex)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].AutofitRow(rowIndex);
		}
	}

	public void AutofitColumn(int colIndex)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].AutofitColumn(colIndex);
		}
	}

	public void Replace(string oldValue, string newValue)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].Replace(oldValue, newValue);
		}
	}

	public void Replace(string oldValue, double newValue)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].Replace(oldValue, newValue);
		}
	}

	public void Replace(string oldValue, DateTime newValue)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].Replace(oldValue, newValue);
		}
	}

	public void Replace(string oldValue, string[] newValues, bool isVertical)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].Replace(oldValue, newValues, isVertical);
		}
	}

	public void Replace(string oldValue, int[] newValues, bool isVertical)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].Replace(oldValue, newValues, isVertical);
		}
	}

	public void Replace(string oldValue, double[] newValues, bool isVertical)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].Replace(oldValue, newValues, isVertical);
		}
	}

	public void Remove()
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].Remove();
		}
		Clear();
	}

	public void Move(int iNewIndex)
	{
		throw new NotSupportedException();
	}

	public int ColumnWidthToPixels(double widthInChars)
	{
		return base.List[0].ColumnWidthToPixels(widthInChars);
	}

	public double PixelsToColumnWidth(int pixels)
	{
		return base.List[0].PixelsToColumnWidth(pixels);
	}

	public void SetColumnWidth(int iColumnIndex, double value)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].SetColumnWidth(iColumnIndex, value);
		}
	}

	public void SetColumnWidthInPixels(int iColumnIndex, int value)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].SetColumnWidthInPixels(iColumnIndex, value);
		}
	}

	public void SetColumnWidthInPixels(int iStartColumnIndex, int iCount, int value)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].SetColumnWidthInPixels(iStartColumnIndex, iCount, value);
		}
	}

	public void SetRowHeight(int iRow, double value)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].SetRowHeight(iRow, value);
		}
	}

	public void SetRowHeightInPixels(int iRowIndex, double value)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].SetRowHeightInPixels(iRowIndex, value);
		}
	}

	public void SetRowHeightInPixels(int iStartRowIndex, int iCount, double value)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].SetRowHeightInPixels(iStartRowIndex, iCount, value);
		}
	}

	public double GetColumnWidth(int iColumnIndex)
	{
		IList<IWorksheet> innerList = base.InnerList;
		double num = innerList[0].GetColumnWidth(iColumnIndex);
		int i = 1;
		for (int count = innerList.Count; i < count; i++)
		{
			if (innerList[i].GetColumnWidth(iColumnIndex) != num)
			{
				num = double.NaN;
				break;
			}
		}
		return num;
	}

	public int GetColumnWidthInPixels(int iColumnIndex)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int columnWidthInPixels = innerList[0].GetColumnWidthInPixels(iColumnIndex);
		int i = 1;
		for (int count = innerList.Count; i < count; i++)
		{
			if (innerList[i].GetColumnWidthInPixels(iColumnIndex) != columnWidthInPixels)
			{
				return int.MinValue;
			}
		}
		return columnWidthInPixels;
	}

	public double GetRowHeight(int iRowIndex)
	{
		IList<IWorksheet> innerList = base.InnerList;
		double rowHeight = innerList[0].GetRowHeight(iRowIndex);
		int i = 1;
		for (int count = innerList.Count; i < count; i++)
		{
			if (innerList[i].GetRowHeight(iRowIndex) != rowHeight)
			{
				return double.NaN;
			}
		}
		return rowHeight;
	}

	public int GetRowHeightInPixels(int iRowIndex)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int rowHeightInPixels = innerList[0].GetRowHeightInPixels(iRowIndex);
		int i = 1;
		for (int count = innerList.Count; i < count; i++)
		{
			if (innerList[i].GetRowHeightInPixels(iRowIndex) != rowHeightInPixels)
			{
				return int.MinValue;
			}
		}
		return rowHeightInPixels;
	}

	public IRange FindFirst(string findValue, OfficeFindType flags)
	{
		throw new NotImplementedException();
	}

	public IRange FindFirst(string findValue, OfficeFindType flags, OfficeFindOptions findOptions)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange FindStringStartsWith(string findValue, OfficeFindType flags)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange FindStringStartsWith(string findValue, OfficeFindType flags, bool ignoreCase)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange FindStringEndsWith(string findValue, OfficeFindType flags)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange FindStringEndsWith(string findValue, OfficeFindType flags, bool ignoreCase)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange FindFirst(double findValue, OfficeFindType flags)
	{
		throw new NotImplementedException();
	}

	public IRange FindFirst(bool findValue)
	{
		throw new NotImplementedException();
	}

	public IRange FindFirst(DateTime findValue)
	{
		throw new NotImplementedException();
	}

	public IRange FindFirst(TimeSpan findValue)
	{
		throw new NotImplementedException();
	}

	public IRange[] FindAll(string findValue, OfficeFindType flags)
	{
		throw new NotImplementedException();
	}

	public IRange[] FindAll(string findValue, OfficeFindType flags, OfficeFindOptions findOptions)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange[] FindAll(double findValue, OfficeFindType flags)
	{
		throw new NotImplementedException();
	}

	public IRange[] FindAll(bool findValue)
	{
		throw new NotImplementedException();
	}

	public IRange[] FindAll(DateTime findValue)
	{
		throw new NotImplementedException();
	}

	public IRange[] FindAll(TimeSpan findValue)
	{
		throw new NotImplementedException();
	}

	public void SaveAs(string fileName, string separator)
	{
		throw new NotSupportedException();
	}

	public void SaveAs(string fileName, string separator, Encoding encoding)
	{
		throw new NotSupportedException();
	}

	public void SaveAs(Stream stream, string separator)
	{
		throw new NotSupportedException();
	}

	public void SaveAs(Stream stream, string separator, Encoding encoding)
	{
		throw new NotSupportedException();
	}

	public void SetDefaultColumnStyle(int iColumnIndex, IStyle defaultStyle)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].SetDefaultColumnStyle(iColumnIndex, defaultStyle);
		}
	}

	public void SetDefaultColumnStyle(int iStartColumnIndex, int iEndColumnIndex, IStyle defaultStyle)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].SetDefaultColumnStyle(iStartColumnIndex, iEndColumnIndex, defaultStyle);
		}
	}

	public void SetDefaultRowStyle(int iRowIndex, IStyle defaultStyle)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].SetDefaultRowStyle(iRowIndex, defaultStyle);
		}
	}

	public void SetDefaultRowStyle(int iStartRowIndex, int iEndRowIndex, IStyle defaultStyle)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].SetDefaultRowStyle(iStartRowIndex, iEndRowIndex, defaultStyle);
		}
	}

	public IStyle GetDefaultRowStyle(int iRowIndex)
	{
		return null;
	}

	public IStyle GetDefaultColumnStyle(int iColumnIndex)
	{
		return null;
	}

	public void FreeRange(IRange range)
	{
		throw new NotImplementedException();
	}

	public void FreeRange(int iRow, int iColumn)
	{
		throw new NotImplementedException();
	}

	public void SetValue(int iRow, int iColumn, string value)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].SetValue(iRow, iColumn, value);
		}
	}

	public void SetNumber(int iRow, int iColumn, double value)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].SetNumber(iRow, iColumn, value);
		}
	}

	public void SetBoolean(int iRow, int iColumn, bool value)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].SetBoolean(iRow, iColumn, value);
		}
	}

	public void SetText(int iRow, int iColumn, string value)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].SetText(iRow, iColumn, value);
		}
	}

	public void SetFormula(int iRow, int iColumn, string value)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].SetFormula(iRow, iColumn, value);
		}
	}

	public void SetError(int iRow, int iColumn, string value)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].SetError(iRow, iColumn, value);
		}
	}

	public void SetBlank(int iRow, int iColumn)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].SetBlank(iRow, iColumn);
		}
	}

	public void SetFormulaNumberValue(int iRow, int iColumn, double value)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].SetFormulaNumberValue(iRow, iColumn, value);
		}
	}

	public void SetFormulaErrorValue(int iRow, int iColumn, string value)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].SetFormulaErrorValue(iRow, iColumn, value);
		}
	}

	public void SetFormulaBoolValue(int iRow, int iColumn, bool value)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].SetFormulaBoolValue(iRow, iColumn, value);
		}
	}

	public void SetFormulaStringValue(int iRow, int iColumn, string value)
	{
		IList<IWorksheet> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			innerList[i].SetFormulaStringValue(iRow, iColumn, value);
		}
	}

	public string GetText(int row, int column)
	{
		throw new NotSupportedException();
	}

	public double GetNumber(int row, int column)
	{
		throw new NotSupportedException();
	}

	public string GetFormula(int row, int column, bool bR1C1)
	{
		throw new NotSupportedException();
	}

	public string GetError(int row, int column)
	{
		throw new NotSupportedException();
	}

	public bool GetBoolean(int row, int column)
	{
		throw new NotSupportedException();
	}

	public string GetFormulaStringValue(int row, int column)
	{
		throw new NotSupportedException();
	}

	public double GetFormulaNumberValue(int row, int column)
	{
		throw new NotSupportedException();
	}

	public string GetFormulaErrorValue(int row, int column)
	{
		throw new NotSupportedException();
	}

	public bool GetFormulaBoolValue(int row, int column)
	{
		throw new NotSupportedException();
	}

	public void Select()
	{
	}

	public void Unselect()
	{
	}

	private void WorksheetGroup_Inserted(object sender, CollectionChangeEventArgs<IWorksheet> args)
	{
		WorksheetBaseImpl worksheetBaseImpl = args.Value as WorksheetBaseImpl;
		worksheetBaseImpl.SelectTab();
		m_book.WindowOne.NumSelectedTabs = (ushort)base.Count;
		if (base.Count == 1)
		{
			m_book.WindowOne.SelectedTab = (ushort)worksheetBaseImpl.RealIndex;
		}
	}

	private void WorksheetGroup_Removing(object sender, CollectionChangeEventArgs<IWorksheet> args)
	{
		if (base.Count == 1)
		{
			throw new ApplicationException("Can't deselect all worksheets.");
		}
		args.Value.Unselect();
	}

	private void WorksheetGroup_Clearing()
	{
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			(base.List[i] as WorksheetBaseImpl).Unselect(bCheckNumber: false);
		}
		m_book.WindowOne.NumSelectedTabs = 0;
	}

	protected override void OnClear()
	{
		base.OnClear();
		if (m_book == null)
		{
			m_book = null;
		}
		if (m_pageSetup != null)
		{
			m_pageSetup.Dispose();
		}
		if (m_usedRange != null)
		{
			m_usedRange = null;
		}
	}
}
