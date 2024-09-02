using System;
using System.Globalization;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartDataRange : IOfficeDataRange
{
	private ChartImpl m_chart;

	private IRange m_dataRange;

	private WorksheetImpl m_sheet;

	private bool IsSerieEmpty;

	public int FirstRow => m_dataRange.Row;

	public int LastRow => m_dataRange.LastRow;

	public int FirstColumn => m_dataRange.Column;

	public int LastColumn => m_dataRange.LastColumn;

	internal int Count => m_dataRange.Count;

	internal IRange Range
	{
		get
		{
			return m_dataRange;
		}
		set
		{
			m_dataRange = value;
			if (m_sheet != null && value != null && m_sheet != value.Worksheet)
			{
				m_sheet = value.Worksheet as WorksheetImpl;
			}
		}
	}

	internal WorksheetImpl SheetImpl => m_sheet;

	internal ChartDataRange(IOfficeChart chart)
	{
		m_chart = chart as ChartImpl;
		m_sheet = m_chart.Workbook.Worksheets[m_chart.ActiveSheetIndex] as WorksheetImpl;
	}

	public void SetValue(int rowIndex, int columnIndex, int value)
	{
		if (m_sheet[rowIndex, columnIndex].HasFormulaNumberValue)
		{
			m_sheet[rowIndex, columnIndex].FormulaNumberValue = value;
		}
		else
		{
			m_sheet[rowIndex, columnIndex].Number = value;
		}
		if (!IsSerieEmpty)
		{
			UpdateChartCache();
		}
	}

	private void UpdateChartCache()
	{
		m_chart.CategoryLabelValues = null;
		foreach (ChartSerieImpl item in m_chart.Series)
		{
			item.EnteredDirectlyValues = null;
			item.EnteredDirectlyCategoryLabels = null;
		}
		m_chart.DetectChartType();
		foreach (ChartSerieImpl item2 in m_chart.Series)
		{
			string addressGlobal = ((ChartDataRange)item2.NameRange).Range.AddressGlobal;
			if (addressGlobal != null)
			{
				if (!(item2.NameRange as ChartDataRange).Range.HasFormula)
				{
					item2.NameRangeIRange.Value2 = m_chart.Workbook.ActiveSheet.Range[addressGlobal].Value2;
				}
				if ((item2.NameRange as ChartDataRange).Range.HasFormulaNumberValue)
				{
					item2.NameRangeIRange.Value2 = m_chart.Workbook.ActiveSheet.Range[addressGlobal].FormulaNumberValue;
				}
				else if ((item2.NameRange as ChartDataRange).Range.HasFormulaStringValue)
				{
					item2.NameRangeIRange.Value2 = m_chart.Workbook.ActiveSheet.Range[addressGlobal].FormulaStringValue;
				}
				else if ((item2.NameRange as ChartDataRange).Range.HasFormulaDateTime)
				{
					item2.NameRangeIRange.Value2 = m_chart.Workbook.ActiveSheet.Range[addressGlobal].FormulaDateTime;
				}
			}
			IRange[] cells = item2.ValuesIRange.Cells;
			for (int i = 0; i < cells.Length; i++)
			{
				RangeImpl rangeImpl = (RangeImpl)cells[i];
				addressGlobal = rangeImpl.AddressGlobal;
				if (addressGlobal != null)
				{
					if (!m_chart.Workbook.ActiveSheet.Range[addressGlobal].HasFormula)
					{
						rangeImpl.Value2 = m_chart.Workbook.ActiveSheet.Range[addressGlobal].Value2;
					}
					else if (m_chart.Workbook.ActiveSheet.Range[addressGlobal].HasFormulaNumberValue)
					{
						rangeImpl.Value2 = m_chart.Workbook.ActiveSheet.Range[addressGlobal].FormulaNumberValue;
					}
					else if (m_chart.Workbook.ActiveSheet.Range[addressGlobal].HasFormulaStringValue)
					{
						rangeImpl.Value2 = m_chart.Workbook.ActiveSheet.Range[addressGlobal].FormulaStringValue;
					}
					else if (m_chart.Workbook.ActiveSheet.Range[addressGlobal].HasFormulaDateTime)
					{
						rangeImpl.Value2 = m_chart.Workbook.ActiveSheet.Range[addressGlobal].FormulaDateTime;
					}
				}
			}
		}
		foreach (ChartCategory category in m_chart.Categories)
		{
			IRange[] cells = category.CategoryLabelIRange.Cells;
			for (int i = 0; i < cells.Length; i++)
			{
				RangeImpl rangeImpl2 = (RangeImpl)cells[i];
				string addressGlobal2 = rangeImpl2.AddressGlobal;
				if (addressGlobal2 != null)
				{
					rangeImpl2.Value2 = m_chart.Workbook.ActiveSheet.Range[addressGlobal2].Value2;
				}
			}
		}
		IsSerieEmpty = true;
	}

	public void SetValue(int rowIndex, int columnIndex, double value)
	{
		if (m_sheet[rowIndex, columnIndex].HasFormulaNumberValue)
		{
			m_sheet[rowIndex, columnIndex].FormulaNumberValue = value;
		}
		else
		{
			m_sheet[rowIndex, columnIndex].Number = value;
		}
		if (!IsSerieEmpty)
		{
			UpdateChartCache();
		}
	}

	public void SetValue(int rowIndex, int columnIndex, string value)
	{
		DateTime result;
		if (m_sheet[rowIndex, columnIndex].HasFormulaStringValue)
		{
			if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out result))
			{
				m_sheet[rowIndex, columnIndex].FormulaDateTime = Convert.ToDateTime(value);
			}
			else
			{
				m_sheet[rowIndex, columnIndex].FormulaStringValue = value;
			}
		}
		else if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out result))
		{
			m_sheet[rowIndex, columnIndex].DateTime = Convert.ToDateTime(value);
		}
		else
		{
			m_sheet[rowIndex, columnIndex].Text = value;
		}
		if (!IsSerieEmpty)
		{
			UpdateChartCache();
		}
	}

	public void SetValue(int rowIndex, int columnIndex, object value)
	{
		m_sheet[rowIndex, columnIndex].Value2 = value;
		if (!IsSerieEmpty)
		{
			UpdateChartCache();
		}
	}

	public void SetValue(int rowIndex, int columnIndex, DateTime value)
	{
		if (m_sheet[rowIndex, columnIndex].HasFormulaDateTime)
		{
			m_sheet[rowIndex, columnIndex].FormulaDateTime = value;
		}
		else
		{
			m_sheet[rowIndex, columnIndex].DateTime = value;
		}
		if (!IsSerieEmpty)
		{
			UpdateChartCache();
		}
	}

	public object GetValue(int rowIndex, int columnIndex)
	{
		return GetValue(rowIndex, columnIndex, useFormulaValue: false);
	}

	public object GetValue(int rowIndex, int columnIndex, bool useFormulaValue)
	{
		if (useFormulaValue)
		{
			if (m_sheet[rowIndex, columnIndex].HasFormula)
			{
				return (m_sheet[rowIndex, columnIndex] as RangeImpl).FormulaValue;
			}
			return m_sheet[rowIndex, columnIndex].Value2;
		}
		return m_sheet[rowIndex, columnIndex].Value2;
	}
}
