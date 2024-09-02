using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Tables;

public class PdfLightTable : PdfLayoutElement
{
	private PdfColumnCollection m_columns;

	private PdfRowCollection m_rows;

	internal object m_dataSource;

	private string m_dataMember;

	private PdfLightTableDataSourceType m_dataSourceType;

	private PdfLightTableStyle m_properties;

	private PdfDataSource m_dsParser;

	private bool m_allowRowBreakAcrossPages = true;

	internal PdfLightTableBuiltinStyle m_lightTableBuiltinStyle;

	internal bool m_headerStyle = true;

	internal bool m_bandedRowStyle = true;

	internal bool m_bandedColStyle;

	internal bool m_totalRowStyle;

	internal bool m_firstColumnStyle;

	internal bool m_lastColumnStyle;

	internal bool isBuiltinStyle;

	internal bool isCustomDataSource;

	private bool isColumnProportionalSizing;

	public PdfColumnCollection Columns
	{
		get
		{
			if (m_columns == null)
			{
				m_columns = CreateColumns();
			}
			return m_columns;
		}
	}

	public PdfRowCollection Rows
	{
		get
		{
			if (m_rows == null)
			{
				m_rows = CreateRows();
			}
			return m_rows;
		}
	}

	public bool ColumnProportionalSizing
	{
		get
		{
			return isColumnProportionalSizing;
		}
		set
		{
			isColumnProportionalSizing = value;
		}
	}

	public object DataSource
	{
		get
		{
			return m_dataSource;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("DataSource");
			}
			m_dataSource = value;
			if (m_dataSource is IEnumerable && DataSourceType == PdfLightTableDataSourceType.External)
			{
				SetDataSource();
			}
			else
			{
				m_dsParser = CreateDataSourceConsumer(value);
			}
		}
	}

	public PdfLightTableDataSourceType DataSourceType
	{
		get
		{
			return m_dataSourceType;
		}
		set
		{
			m_dataSourceType = value;
		}
	}

	public PdfLightTableStyle Style
	{
		get
		{
			if (m_properties == null)
			{
				m_properties = new PdfLightTableStyle();
			}
			return m_properties;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Properties");
			}
			m_properties = value;
		}
	}

	internal bool RaiseBeginRowLayout => this.BeginRowLayout != null;

	internal bool RaiseEndRowLayout => this.EndRowLayout != null;

	internal bool RaiseBeginCellLayout => this.BeginCellLayout != null;

	internal bool RaiseEndCellLayout => this.EndCellLayout != null;

	public bool AllowRowBreakAcrossPages
	{
		get
		{
			return m_allowRowBreakAcrossPages;
		}
		set
		{
			m_allowRowBreakAcrossPages = value;
		}
	}

	public event BeginRowLayoutEventHandler BeginRowLayout;

	public event EndRowLayoutEventHandler EndRowLayout;

	public event BeginCellLayoutEventHandler BeginCellLayout;

	public event EndCellLayoutEventHandler EndCellLayout;

	public event QueryNextRowEventHandler QueryNextRow;

	public event QueryColumnCountEventHandler QueryColumnCount;

	public event QueryRowCountEventHandler QueryRowCount;

	public void SetDataSource()
	{
		if (!(m_dataSource is IEnumerable))
		{
			return;
		}
		PropertyInfo[] array = null;
		foreach (object item in m_dataSource as IEnumerable)
		{
			if (item != null)
			{
				array = new List<PropertyInfo>(item.GetType().GetRuntimeProperties()).ToArray();
				PropertyInfo[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					PdfColumn column = new PdfColumn(array2[i].Name);
					Columns.Add(column);
				}
				break;
			}
		}
		_ = Columns.Count;
		List<string> list = new List<string>();
		foreach (object item2 in m_dataSource as IEnumerable)
		{
			PdfRow pdfRow = new PdfRow();
			list = new List<string>();
			PropertyInfo[] array2 = array;
			foreach (PropertyInfo propertyInfo in array2)
			{
				PropertyInfo runtimeProperty = item2.GetType().GetRuntimeProperty(propertyInfo.Name);
				list.Add(Convert.ToString(runtimeProperty.GetValue(item2, null)));
			}
			object[] values = list.ToArray();
			pdfRow.Values = values;
			Rows.Add(pdfRow);
		}
	}

	public void Draw(PdfGraphics graphics, PointF location, float width)
	{
		Draw(graphics, location.X, location.Y, width);
	}

	public void Draw(PdfGraphics graphics, float x, float y, float width)
	{
		RectangleF bounds = new RectangleF(x, y, width, 0f);
		Draw(graphics, bounds);
	}

	public void Draw(PdfGraphics graphics, RectangleF bounds)
	{
		if (m_dataSourceType == PdfLightTableDataSourceType.TableDirect)
		{
			DataSource = FillData();
		}
		new LightTableLayouter(this).Layout(graphics, bounds);
	}

	public new PdfLightTableLayoutResult Draw(PdfPage page, PointF location)
	{
		if (m_dataSourceType == PdfLightTableDataSourceType.TableDirect)
		{
			DataSource = FillData();
		}
		return (PdfLightTableLayoutResult)base.Draw(page, location);
	}

	public PdfLightTableLayoutResult Draw(PdfPage page, PointF location, PdfLightTableLayoutFormat format)
	{
		if (m_dataSourceType == PdfLightTableDataSourceType.TableDirect)
		{
			DataSource = FillData();
		}
		return (PdfLightTableLayoutResult)Draw(page, location, (PdfLayoutFormat)format);
	}

	public new PdfLightTableLayoutResult Draw(PdfPage page, RectangleF bounds)
	{
		if (m_dataSourceType == PdfLightTableDataSourceType.TableDirect)
		{
			DataSource = FillData();
		}
		return (PdfLightTableLayoutResult)base.Draw(page, bounds);
	}

	public PdfLightTableLayoutResult Draw(PdfPage page, RectangleF bounds, PdfLightTableLayoutFormat format)
	{
		if (m_dataSourceType == PdfLightTableDataSourceType.TableDirect)
		{
			DataSource = FillData();
		}
		return (PdfLightTableLayoutResult)Draw(page, bounds, (PdfLayoutFormat)format);
	}

	public new PdfLightTableLayoutResult Draw(PdfPage page, float x, float y)
	{
		if (m_dataSourceType == PdfLightTableDataSourceType.TableDirect)
		{
			DataSource = FillData();
		}
		return (PdfLightTableLayoutResult)base.Draw(page, x, y);
	}

	public PdfLightTableLayoutResult Draw(PdfPage page, float x, float y, PdfLightTableLayoutFormat format)
	{
		if (m_dataSourceType == PdfLightTableDataSourceType.TableDirect)
		{
			DataSource = FillData();
		}
		return (PdfLightTableLayoutResult)Draw(page, x, y, (PdfLayoutFormat)format);
	}

	public PdfLightTableLayoutResult Draw(PdfPage page, float x, float y, float width)
	{
		return Draw(page, x, y, width, null);
	}

	public PdfLightTableLayoutResult Draw(PdfPage page, float x, float y, float width, PdfLightTableLayoutFormat format)
	{
		if (m_dataSourceType == PdfLightTableDataSourceType.TableDirect)
		{
			DataSource = FillData();
		}
		RectangleF layoutRectangle = new RectangleF(x, y, width, 0f);
		return (PdfLightTableLayoutResult)Draw(page, layoutRectangle, (PdfLayoutFormat)format);
	}

	public void ApplyBuiltinStyle(PdfLightTableBuiltinStyle tableStyle)
	{
		isBuiltinStyle = true;
		m_lightTableBuiltinStyle = tableStyle;
	}

	public void ApplyBuiltinStyle(PdfLightTableBuiltinStyle lightTableStyle, PdfLightTableBuiltinStyleSettings lightTableSetting)
	{
		m_headerStyle = lightTableSetting.ApplyStyleForHeaderRow;
		m_totalRowStyle = lightTableSetting.ApplyStyleForLastRow;
		m_lastColumnStyle = lightTableSetting.ApplyStyleForLastColumn;
		m_firstColumnStyle = lightTableSetting.ApplyStyleForFirstColumn;
		m_bandedColStyle = lightTableSetting.ApplyStyleForBandedColumns;
		m_bandedRowStyle = lightTableSetting.ApplyStyleForBandedRows;
		ApplyBuiltinStyle(lightTableStyle);
	}

	public override void Draw(PdfGraphics graphics, float x, float y)
	{
		SizeF clientSize = graphics.ClientSize;
		clientSize.Width -= x;
		clientSize.Height -= y;
		Draw(graphics, x, y, clientSize.Width);
	}

	protected override PdfLayoutResult Layout(PdfLayoutParams param)
	{
		if (param.Bounds.Width < 0f)
		{
			throw new ArgumentOutOfRangeException("Width");
		}
		if (DataSource == null && m_dataSourceType == PdfLightTableDataSourceType.TableDirect)
		{
			DataSource = FillData();
		}
		return (PdfLightTableLayoutResult)new LightTableLayouter(this).Layout(param);
	}

	protected override void DrawInternal(PdfGraphics graphics)
	{
		new LightTableLayouter(this).Layout(graphics, PointF.Empty);
	}

	internal void OnBeginRowLayout(BeginRowLayoutEventArgs args)
	{
		if (RaiseBeginRowLayout)
		{
			this.BeginRowLayout(this, args);
		}
	}

	internal void OnEndRowLayout(EndRowLayoutEventArgs args)
	{
		if (RaiseEndRowLayout)
		{
			this.EndRowLayout(this, args);
		}
	}

	internal void OnBeginCellLayout(BeginCellLayoutEventArgs args)
	{
		if (RaiseBeginCellLayout)
		{
			this.BeginCellLayout(this, args);
		}
	}

	internal void OnEndCellLayout(EndCellLayoutEventArgs args)
	{
		if (RaiseEndCellLayout)
		{
			this.EndCellLayout(this, args);
		}
	}

	internal string[] GetNextRow(ref int index)
	{
		string[] array = new string[0];
		if (m_dsParser != null)
		{
			array = m_dsParser.GetRow(ref index);
		}
		else if (this.QueryNextRow != null)
		{
			array = OnGetNextRow(index);
		}
		else
		{
			array = null;
			if (Rows.Count > index && Rows[index].Values != null)
			{
				if (Rows[index].Values.Length != 0)
				{
					int num = Rows[index].Values.Length;
					array = new string[num];
					for (int i = 0; i < num; i++)
					{
						array[i] = Rows[index].Values[i].ToString();
					}
				}
			}
			else
			{
				array = OnGetNextRow(index);
			}
		}
		return array;
	}

	internal string[] GetColumnCaptions()
	{
		return GetColumnsCaption();
	}

	private PdfDataSource CreateDataSourceConsumer(object value)
	{
		Array array = value as Array;
		DataSet dataSet = value as DataSet;
		DataColumn dataColumn = value as DataColumn;
		DataTable dataTable = value as DataTable;
		DataView dataView = value as DataView;
		IEnumerable enumerable = value as IEnumerable;
		PdfDataSource result = null;
		isCustomDataSource = false;
		if (array != null)
		{
			result = new PdfDataSource(array);
		}
		else if (dataColumn != null)
		{
			result = new PdfDataSource(dataColumn);
		}
		else if (dataTable != null)
		{
			result = new PdfDataSource(dataTable);
		}
		else if (dataView != null)
		{
			result = new PdfDataSource(dataView);
		}
		else if (dataSet != null)
		{
			result = new PdfDataSource(dataSet, m_dataMember);
		}
		else if (enumerable != null)
		{
			result = new PdfDataSource(enumerable);
			isCustomDataSource = true;
		}
		return result;
	}

	private object FillData()
	{
		if (m_dataSource is IEnumerable)
		{
			return 0;
		}
		return FillDataValue();
	}

	private PdfColumnCollection CreateColumns()
	{
		PdfColumnCollection columns = new PdfColumnCollection();
		return CreateColumnCollection(columns);
	}

	private PdfRowCollection CreateRows()
	{
		PdfRowCollection rows = new PdfRowCollection();
		return CreateRowCollection(rows);
	}

	private PdfColumnCollection CreateColumnCollection(PdfColumnCollection columns)
	{
		int num = ((m_dsParser != null) ? m_dsParser.ColumnCount : OnGetColumnNumber());
		for (int i = 0; i < num; i++)
		{
			PdfColumn column = new PdfColumn(10f);
			columns.Add(column);
		}
		return columns;
	}

	private object FillDataValue()
	{
		try
		{
			DataTable dataTable = new DataTable();
			for (int i = 0; i < Columns.Count; i++)
			{
				if (Columns[i].ColumnName != null)
				{
					dataTable.Columns.Add(Columns[i].ColumnName);
				}
				else
				{
					dataTable.Columns.Add(string.Empty);
				}
				Columns[i].m_dataSourceType = m_dataSourceType;
				Columns[i].Width = Columns[i].Width;
				Columns[i].StringFormat = Columns[i].StringFormat;
			}
			foreach (PdfRow row in Rows)
			{
				if (row.Values != null)
				{
					dataTable.Rows.Add(row.Values);
				}
			}
			return dataTable;
		}
		catch (Exception innerException)
		{
			throw new PdfException("Please check whether the number of rows matches the column count.", innerException);
		}
	}

	private PdfRowCollection CreateRowCollection(PdfRowCollection rows)
	{
		int num = ((m_dsParser != null) ? m_dsParser.RowCount : OnGetRowNumber());
		for (int i = 0; i < num; i++)
		{
			PdfRow row = new PdfRow();
			rows.Add(row);
		}
		return rows;
	}

	private string[] GetColumnsCaption()
	{
		PdfColumnCollection columns = Columns;
		string[] array = ((m_dsParser != null) ? m_dsParser.ColumnCaptions : null);
		if (m_dsParser != null)
		{
			for (int i = 0; i < m_dsParser.ColumnCount; i++)
			{
				if (columns[i].ColumnName != null)
				{
					if (array == null)
					{
						array = new string[m_dsParser.ColumnCount];
					}
					array[i] = columns[i].ColumnName;
				}
			}
		}
		else if (columns != null)
		{
			for (int j = 0; j < columns.Count; j++)
			{
				if (columns[j].ColumnName != null)
				{
					if (array == null)
					{
						array = new string[columns.Count];
					}
					array[j] = columns[j].ColumnName;
				}
			}
		}
		return array;
	}

	private string[] OnGetNextRow(int rowIndex)
	{
		string[] result = null;
		if (this.QueryNextRow != null)
		{
			QueryNextRowEventArgs queryNextRowEventArgs = new QueryNextRowEventArgs(Columns.Count, rowIndex);
			if ((Rows != null && Rows.Count > rowIndex) || Rows.Count == 0)
			{
				this.QueryNextRow(this, queryNextRowEventArgs);
			}
			result = queryNextRowEventArgs.RowData;
		}
		return result;
	}

	private int OnGetColumnNumber()
	{
		int num = 0;
		if (this.QueryColumnCount != null)
		{
			QueryColumnCountEventArgs queryColumnCountEventArgs = new QueryColumnCountEventArgs();
			this.QueryColumnCount(this, queryColumnCountEventArgs);
			num = queryColumnCountEventArgs.ColumnCount;
		}
		if (num < 0)
		{
			throw new PdfLightTableException("There is no columns.");
		}
		return num;
	}

	private int OnGetRowNumber()
	{
		int num = 0;
		if (this.QueryRowCount != null)
		{
			QueryRowCountEventArgs queryRowCountEventArgs = new QueryRowCountEventArgs();
			this.QueryRowCount(this, queryRowCountEventArgs);
			num = queryRowCountEventArgs.RowCount;
		}
		if (num < 0)
		{
			throw new PdfLightTableException("There is no Rows.");
		}
		return num;
	}
}
