using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using DocGen.Drawing;
using DocGen.OfficeChart.Calculate;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation;

internal class ExternWorksheetImpl : CommonObject, IWorksheet, ITabSheet, IParentApplication, ICalcData, IInternalWorksheet, ICloneParent
{
	private CalcEngine m_calcEngine;

	private XCTRecord m_xct = (XCTRecord)BiffRecordFactory.GetRecord(TBIFFRecord.XCT);

	private List<BiffRecordRaw> m_arrRecords = new List<BiffRecordRaw>();

	private ExternWorkbookImpl m_book;

	private string m_strName;

	private CellRecordCollection m_dicRecordsCells;

	private int m_iFirstRow = -1;

	private int m_iFirstColumn = int.MaxValue;

	private int m_iLastRow = -1;

	private int m_iLastColumn = int.MaxValue;

	private Dictionary<string, string> m_dicAdditionalAttributes;

	internal int unknown_formula_name = 9;

	public int Index
	{
		get
		{
			return m_xct.SheetTableIndex;
		}
		set
		{
			m_xct.SheetTableIndex = (ushort)value;
		}
	}

	public ExternWorkbookImpl Workbook => m_book;

	public int ReferenceIndex => m_book.Workbook.AddSheetReference(m_book.Index, Index, Index);

	public Dictionary<string, string> AdditionalAttributes
	{
		get
		{
			return m_dicAdditionalAttributes;
		}
		set
		{
			m_dicAdditionalAttributes = value;
		}
	}

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

	public IRange[] Cells
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool DisplayPageBreaks
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public OfficeSheetProtection Protection
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool ProtectContents
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public OfficeSheetView View
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool ProtectDrawingObjects
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool ProtectScenarios
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
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

	public IRange[] MergedCells
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public INames Names
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public string CodeName
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public IPageSetup PageSetup
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public IRange Range
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public IRange[] Rows
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public IRange[] Columns
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public double StandardHeight
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool StandardHeightFlag
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public double StandardWidth
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public OfficeSheetType Type
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public IRange UsedRange
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public int Zoom
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public int VerticalSplit
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public int HorizontalSplit
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public int FirstVisibleRow
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public int FirstVisibleColumn
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public int ActivePane
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool IsDisplayZeros
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool IsGridLinesVisible
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public OfficeKnownColors GridLineColor
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool IsRowColumnHeadersVisible
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool IsStringsPreserved
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool IsPasswordProtected
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public IRange this[int row, int column] => null;

	public IRange this[int row, int column, int lastRow, int lastColumn] => null;

	public IRange this[string name]
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public IRange this[string name, bool IsR1C1Notation]
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public IRange[] UsedCells
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public IWorksheetCustomProperties CustomProperties
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool UseRangesCache
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool IsFreezePanes
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public IRange SplitCell
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public int TopVisibleRow
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public int LeftVisibleColumn
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool UsedRangeIncludesFormatting
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public IMigrantRange MigrantRange
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public OfficeKnownColors TabColor
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public Color TabColorRGB
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public IOfficeChartShapes Charts
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	IWorkbook ITabSheet.Workbook => m_book.Workbook;

	public IShapes Shapes
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool IsRightToLeft
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool IsSelected
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public int TabIndex
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public string Name
	{
		get
		{
			return m_strName;
		}
		set
		{
			m_strName = value;
		}
	}

	public OfficeWorksheetVisibility Visibility
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public ITextBoxes TextBoxes
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public int DefaultRowHeight => 0;

	public int FirstRow
	{
		get
		{
			return m_iFirstRow;
		}
		set
		{
			m_iFirstRow = value;
		}
	}

	public int FirstColumn
	{
		get
		{
			return m_iFirstColumn;
		}
		set
		{
			m_iFirstColumn = value;
		}
	}

	public int LastRow
	{
		get
		{
			return m_iLastRow;
		}
		set
		{
			m_iLastRow = value;
		}
	}

	public int LastColumn
	{
		get
		{
			return m_iLastColumn;
		}
		set
		{
			m_iLastColumn = value;
		}
	}

	public CellRecordCollection CellRecords
	{
		[DebuggerStepThrough]
		get
		{
			return m_dicRecordsCells;
		}
	}

	public WorkbookImpl ParentWorkbook => m_book.Workbook;

	public OfficeVersion Version => OfficeVersion.Excel2007;

	public event RangeImpl.CellValueChangedEventHandler CellValueChanged;

	public event MissingFunctionEventHandler MissingFunction;

	public event DocGen.OfficeChart.Calculate.ValueChangedEventHandler ValueChanged;

	public ExternWorksheetImpl(IApplication application, ExternWorkbookImpl parent)
		: base(application, parent)
	{
		m_book = parent;
		m_dicRecordsCells = new CellRecordCollection(base.Application, this);
		m_dicAdditionalAttributes = new Dictionary<string, string>();
		m_dicAdditionalAttributes["refreshError"] = "1";
	}

	[CLSCompliant(false)]
	public int Parse(BiffRecordRaw[] arrData, int iOffset)
	{
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		if (iOffset < 0 || iOffset > arrData.Length - 1)
		{
			throw new ArgumentOutOfRangeException("iOffset", "Value cannot be less than 0 and greater than arrData.Length - 1");
		}
		BiffRecordRaw biffRecordRaw = arrData[iOffset];
		if (biffRecordRaw.TypeCode != TBIFFRecord.XCT)
		{
			return iOffset;
		}
		XCTRecord xCTRecord = (XCTRecord)biffRecordRaw;
		iOffset++;
		m_arrRecords.Clear();
		m_arrRecords.Add(xCTRecord);
		int num = 0;
		int cRNCount = xCTRecord.CRNCount;
		while (num < cRNCount)
		{
			biffRecordRaw = arrData[iOffset];
			biffRecordRaw.CheckTypeCode(TBIFFRecord.CRN);
			ParseCRN((CRNRecord)biffRecordRaw);
			m_arrRecords.Add(biffRecordRaw);
			num++;
			iOffset++;
		}
		return iOffset;
	}

	private void ParseCRN(CRNRecord crn)
	{
		int num = crn.Row + 1;
		int num2 = crn.FirstColumn + 1;
		int num3 = 0;
		while (num2 <= crn.LastColumn + 1)
		{
			object obj = crn.Values[num3];
			if (obj is string strValue)
			{
				m_dicRecordsCells.SetNonSSTString(num, num2, 0, strValue);
			}
			else if (obj is double)
			{
				m_dicRecordsCells.SetNumberValue(num, num2, (double)obj, 0);
			}
			else if (obj is bool)
			{
				m_dicRecordsCells.SetBooleanValue(num, num2, (bool)obj, 0);
			}
			else if (obj is byte)
			{
				m_dicRecordsCells.SetErrorValue(num, num2, (byte)obj, 0);
			}
			num2++;
			num3++;
		}
	}

	[CLSCompliant(false)]
	public void Serialize(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		records.Add(m_xct);
		SerializeRows(records);
	}

	private void SerializeRows(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (m_iFirstRow >= 0)
		{
			for (int i = m_iFirstRow; i <= m_iLastRow; i++)
			{
				SerializeRow(i, records);
			}
		}
	}

	private void SerializeRow(int i, OffsetArrayList records)
	{
		RowStorage orCreateRow = WorksheetHelper.GetOrCreateRow(this, i - 1, bCreate: false);
		if (orCreateRow == null)
		{
			return;
		}
		IEnumerator enumerator = orCreateRow.GetEnumerator(m_dicRecordsCells.RecordExtractor);
		CRNRecord cRNRecord = (CRNRecord)BiffRecordFactory.GetRecord(TBIFFRecord.CRN);
		cRNRecord.Row = (ushort)(i - 1);
		int num = -1;
		List<object> values = cRNRecord.Values;
		while (enumerator.MoveNext())
		{
			BiffRecordRaw obj = (BiffRecordRaw)enumerator.Current;
			object value = ((IValueHolder)obj).Value;
			int column = (obj as ICellPositionFormat).Column;
			if (num < 0)
			{
				cRNRecord.FirstColumn = (byte)column;
			}
			else if (num + 1 != column)
			{
				records.Add(cRNRecord);
				cRNRecord = (CRNRecord)BiffRecordFactory.GetRecord(TBIFFRecord.CRN);
				cRNRecord.Row = (ushort)(i - 1);
				cRNRecord.FirstColumn = (byte)column;
				values = cRNRecord.Values;
			}
			values.Add(value);
			cRNRecord.LastColumn = (byte)column;
			num = column;
		}
		if (values.Count > 0)
		{
			records.Add(cRNRecord);
		}
	}

	public ExternWorksheetImpl Clone(object parent)
	{
		ExternWorksheetImpl externWorksheetImpl = (ExternWorksheetImpl)MemberwiseClone();
		m_xct = (XCTRecord)CloneUtils.CloneCloneable(m_xct);
		externWorksheetImpl.SetParent(parent);
		externWorksheetImpl.m_book = (ExternWorkbookImpl)externWorksheetImpl.FindParent(typeof(ExternWorkbookImpl));
		externWorksheetImpl.m_dicRecordsCells = m_dicRecordsCells.Clone(externWorksheetImpl);
		m_arrRecords = CloneUtils.CloneCloneable(m_arrRecords);
		return externWorksheetImpl;
	}

	protected override void OnDispose()
	{
		if (!m_bIsDisposed)
		{
			if (m_dicRecordsCells != null)
			{
				m_dicRecordsCells.Dispose();
				m_dicRecordsCells = null;
			}
			base.OnDispose();
		}
	}

	internal void CacheValues(IRange sourceRange)
	{
		int i = sourceRange.Row;
		for (int lastRow = sourceRange.LastRow; i <= lastRow; i++)
		{
			int j = sourceRange.Column;
			for (int lastColumn = sourceRange.LastColumn; j <= lastColumn; j++)
			{
				IRange range = sourceRange[i, j];
				if (range.HasBoolean)
				{
					m_dicRecordsCells.SetBooleanValue(i, j, range.Boolean, 0);
				}
				else if (range.HasDateTime || range.HasNumber)
				{
					m_dicRecordsCells.SetNumberValue(i, j, range.Number, 0);
				}
				else if (range.HasString)
				{
					m_dicRecordsCells.SetNonSSTString(i, j, 0, range.Text);
				}
				else if (range.IsError)
				{
					m_dicRecordsCells.SetErrorValue(i, j, range.Error);
				}
			}
		}
	}

	internal void SetCellRecords(CellRecordCollection cellRecords)
	{
		m_dicRecordsCells.Dispose();
		m_dicRecordsCells = cellRecords;
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

	internal void Calculate()
	{
		throw new NotImplementedException();
	}

	public void CopyToClipboard()
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void Clear()
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void ClearData()
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public bool Contains(int iRow, int iColumn)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRanges CreateRangesCollection()
	{
		return base.AppImplementation.CreateRangesCollection(this);
	}

	public void CreateNamedRanges(string namedRange, string referRange, bool vertical)
	{
		throw new NotImplementedException();
	}

	public bool IsColumnVisible(int columnIndex)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void ShowColumn(int columnIndex, bool isVisible)
	{
		throw new Exception("The method or operation is not implemented.");
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
		throw new Exception("The method or operation is not implemented.");
	}

	public void ShowRow(int rowIndex, bool isVisible)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void ShowRange(IRange range, bool isVisible)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void ShowRange(RangesCollection ranges, bool isVisible)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void ShowRange(IRange[] ranges, bool isVisible)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void InsertRow(int index)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void InsertRow(int iRowIndex, int iRowCount)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void InsertRow(int iRowIndex, int iRowCount, OfficeInsertOptions insertOptions)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void InsertColumn(int index)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void InsertColumn(int iColumnIndex, int iColumnCount)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void InsertColumn(int iColumnIndex, int iColumnCount, OfficeInsertOptions insertOptions)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void DeleteRow(int index)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void DeleteRow(int index, int count)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void DeleteColumn(int index)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void DeleteColumn(int index, int count)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public int ImportArray(object[] arrObject, int firstRow, int firstColumn, bool isVertical)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public int ImportArray(string[] arrString, int firstRow, int firstColumn, bool isVertical)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public int ImportArray(int[] arrInt, int firstRow, int firstColumn, bool isVertical)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public int ImportArray(double[] arrDouble, int firstRow, int firstColumn, bool isVertical)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public int ImportArray(DateTime[] arrDateTime, int firstRow, int firstColumn, bool isVertical)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public int ImportArray(object[,] arrObject, int firstRow, int firstColumn)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public int ImportData(IEnumerable arrObject, int firstRow, int firstColumn, bool includeHeader)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void RemovePanes()
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void Protect(string password)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void Protect(string password, OfficeSheetProtection options)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void Unprotect(string password)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange IntersectRanges(IRange range1, IRange range2)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange MergeRanges(IRange range1, IRange range2)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void AutofitRow(int rowIndex)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void AutofitColumn(int colIndex)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void Replace(string oldValue, string newValue)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void Replace(string oldValue, double newValue)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void Replace(string oldValue, DateTime newValue)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void Replace(string oldValue, string[] newValues, bool isVertical)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void Replace(string oldValue, int[] newValues, bool isVertical)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void Replace(string oldValue, double[] newValues, bool isVertical)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void Remove()
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void Move(int iNewIndex)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public int ColumnWidthToPixels(double widthInChars)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public double PixelsToColumnWidth(int pixels)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void SetColumnWidth(int iColumnIndex, double value)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void SetColumnWidthInPixels(int iColumnIndex, int value)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void SetColumnWidthInPixels(int iStartColumnIndex, int iCount, int value)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void SetRowHeight(int iRow, double value)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void SetRowHeightInPixels(int iRowIndex, double value)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void SetRowHeightInPixels(int iStartRowIndex, int iCount, double value)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public double GetColumnWidth(int iColumnIndex)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public int GetColumnWidthInPixels(int iColumnIndex)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public double GetRowHeight(int iRow)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public int GetRowHeightInPixels(int iRowIndex)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange FindFirst(string findValue, OfficeFindType flags)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange FindFirst(string findValue, OfficeFindType flags, OfficeFindOptions findOptions)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange FindFirst(double findValue, OfficeFindType flags)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange FindFirst(bool findValue)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange FindFirst(DateTime findValue)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange FindFirst(TimeSpan findValue)
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

	public IRange[] FindAll(string findValue, OfficeFindType flags)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange[] FindAll(string findValue, OfficeFindType flags, OfficeFindOptions findOptions)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange[] FindAll(double findValue, OfficeFindType flags)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange[] FindAll(bool findValue)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange[] FindAll(DateTime findValue)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IRange[] FindAll(TimeSpan findValue)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void SaveAs(string fileName, string separator)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void SaveAs(string fileName, string separator, Encoding encoding)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void SaveAs(Stream stream, string separator)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void SaveAs(Stream stream, string separator, Encoding encoding)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void SetDefaultColumnStyle(int iColumnIndex, IStyle defaultStyle)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void SetDefaultColumnStyle(int iStartColumnIndex, int iEndColumnIndex, IStyle defaultStyle)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void SetDefaultRowStyle(int iRowIndex, IStyle defaultStyle)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void SetDefaultRowStyle(int iStartRowIndex, int iEndRowIndex, IStyle defaultStyle)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IStyle GetDefaultColumnStyle(int iColumnIndex)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public IStyle GetDefaultRowStyle(int iRowIndex)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void FreeRange(IRange range)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void FreeRange(int iRow, int iColumn)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void SetValue(int iRow, int iColumn, string value)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void SetNumber(int iRow, int iColumn, double value)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void SetBoolean(int iRow, int iColumn, bool value)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void SetText(int iRow, int iColumn, string value)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void SetFormula(int iRow, int iColumn, string value)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void SetError(int iRow, int iColumn, string value)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void SetBlank(int iRow, int iColumn)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void SetFormulaNumberValue(int iRow, int iColumn, double value)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void SetFormulaErrorValue(int iRow, int iColumn, string value)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void SetFormulaBoolValue(int iRow, int iColumn, bool value)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void SetFormulaStringValue(int iRow, int iColumn, string value)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public string GetText(int row, int column)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public double GetNumber(int row, int column)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public string GetFormula(int row, int column, bool bR1C1)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public string GetError(int row, int column)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public bool GetBoolean(int row, int column)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public bool GetFormulaBoolValue(int row, int column)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public string GetFormulaErrorValue(int row, int column)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public double GetFormulaNumberValue(int row, int column)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public string GetFormulaStringValue(int row, int column)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void Activate()
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void Select()
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public void Unselect()
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public bool IsArrayFormula(long index)
	{
		return false;
	}

	public IInternalWorksheet GetClonedObject(Dictionary<string, string> hashNewNames, WorkbookImpl book)
	{
		int index = m_book.Index;
		int index2 = Index;
		return book.ExternWorkbooks[index].Worksheets[index2];
	}

	object ICloneParent.Clone(object parent)
	{
		return Clone(parent);
	}
}
