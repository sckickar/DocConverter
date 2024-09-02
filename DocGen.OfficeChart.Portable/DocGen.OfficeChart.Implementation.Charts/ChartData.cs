using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartData : IOfficeChartData
{
	private ChartImpl m_chart;

	private WorksheetImpl m_sheet;

	private bool IsSerieEmpty;

	public IOfficeDataRange this[int firstRow, int firstCol, int lastRow, int lastCol] => new ChartDataRange(m_chart)
	{
		Range = m_sheet[firstRow, firstCol, lastRow, lastCol]
	};

	internal IOfficeDataRange this[IRange range] => new ChartDataRange(m_chart)
	{
		Range = m_sheet[range.Row, range.Column, range.LastRow, range.LastColumn]
	};

	internal WorksheetImpl SheetImpl
	{
		get
		{
			return m_sheet;
		}
		set
		{
			m_sheet = value;
		}
	}

	internal ChartData(IOfficeChart chart)
	{
		m_chart = chart as ChartImpl;
		m_sheet = m_chart.Workbook.Worksheets[0] as WorksheetImpl;
	}

	internal ChartData Clone(IOfficeChart chart)
	{
		ChartData obj = (ChartData)MemberwiseClone();
		obj.m_chart = chart as ChartImpl;
		obj.m_sheet = obj.m_chart.Workbook.Worksheets[0] as WorksheetImpl;
		return obj;
	}

	public void SetValue(int rowIndex, int columnIndex, int value)
	{
		m_sheet[rowIndex, columnIndex].Number = value;
		UpdateDataLabelValues(rowIndex, columnIndex);
		if (!IsSerieEmpty)
		{
			ClearCacheValues();
		}
	}

	public void SetValue(int rowIndex, int columnIndex, double value)
	{
		m_sheet[rowIndex, columnIndex].Number = value;
		UpdateDataLabelValues(rowIndex, columnIndex);
		if (!IsSerieEmpty)
		{
			ClearCacheValues();
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
			else if (string.IsNullOrEmpty(value))
			{
				m_sheet[rowIndex, columnIndex].Text = string.Empty;
			}
			else
			{
				m_sheet[rowIndex, columnIndex].Text = value;
			}
		}
		else if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out result))
		{
			m_sheet[rowIndex, columnIndex].DateTime = Convert.ToDateTime(value);
		}
		else if (string.IsNullOrEmpty(value))
		{
			m_sheet[rowIndex, columnIndex].Text = string.Empty;
		}
		else
		{
			m_sheet[rowIndex, columnIndex].Text = value;
		}
		UpdateDataLabelValues(rowIndex, columnIndex);
		if (!IsSerieEmpty)
		{
			ClearCacheValues();
		}
	}

	public void SetValue(int rowIndex, int columnIndex, object value)
	{
		m_sheet[rowIndex, columnIndex].Value2 = value;
		UpdateDataLabelValues(rowIndex, columnIndex);
		if (!IsSerieEmpty)
		{
			ClearCacheValues();
		}
	}

	private void UpdateDataLabelValues(int rowIndex, int columnIndex)
	{
		foreach (IOfficeChartSerie item in m_chart.Series)
		{
			if ((item as ChartSerieImpl).DataLabelCellsValues != null && (item as ChartSerieImpl).DataLabelCellsValues.Count > 0)
			{
				IOfficeDataRange valueFromCellsRange = item.DataPoints.DefaultDataPoint.DataLabels.ValueFromCellsRange;
				if (valueFromCellsRange != null && ((valueFromCellsRange.FirstRow <= rowIndex && rowIndex <= valueFromCellsRange.LastRow) || (valueFromCellsRange.FirstColumn <= columnIndex && columnIndex <= valueFromCellsRange.LastColumn)))
				{
					item.DataPoints.DefaultDataPoint.DataLabels.ValueFromCellsRange = item.DataPoints.DefaultDataPoint.DataLabels.ValueFromCellsRange;
				}
			}
		}
	}

	public object GetValue(int rowIndex, int columnIndex)
	{
		if (m_chart.Categories != null && m_chart.Categories.Count == 0 && m_chart.Series != null && m_chart.Series.Count == 0)
		{
			return null;
		}
		return m_sheet[rowIndex, columnIndex].Value2;
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
			ClearCacheValues();
		}
	}

	public void SetChartData(object[][] data)
	{
		SetDataRange(data, 1, 1);
		if (!IsSerieEmpty)
		{
			ClearCacheValues();
		}
	}

	public void SetDataRange(object[][] data, int rowIndex, int columnIndex)
	{
		for (int i = 0; i < data.Length; i++)
		{
			for (int j = 0; j < data[i].Length; j++)
			{
				m_sheet[i + rowIndex, j + columnIndex].Value2 = data[i][j];
			}
		}
		m_chart.DataRange = this[rowIndex, columnIndex, data.Length, data[0].Length];
	}

	public void SetDataRange(IEnumerable enumerable, int rowIndex, int columnIndex)
	{
		int num = rowIndex;
		IEnumerator enumerator = enumerable.GetEnumerator();
		if (enumerator == null)
		{
			return;
		}
		bool flag = false;
		List<PropertyInfo> propertyInfo = null;
		List<TypeCode> list = null;
		int num2 = 0;
		enumerator.MoveNext();
		object current = enumerator.Current;
		if (current == null)
		{
			return;
		}
		Type type = current.GetType();
		if (type.Namespace == null || (type.Namespace != null && !type.Namespace.Contains("System")))
		{
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		IMigrantRange migrantRange = m_sheet.MigrantRange;
		list = GetObjectMembersInfo(current, out propertyInfo);
		int num3 = 0;
		int num4 = columnIndex;
		while (num3 < propertyInfo.Count)
		{
			migrantRange.ResetRowColumn(num, num4);
			migrantRange.SetValue(propertyInfo[num3].Name);
			num3++;
			num4++;
		}
		num++;
		do
		{
			current = enumerator.Current;
			if (current == null)
			{
				continue;
			}
			int num5 = 0;
			int num6 = columnIndex;
			while (num5 < propertyInfo.Count)
			{
				PropertyInfo strProperty = propertyInfo[num5];
				migrantRange.ResetRowColumn(num + num2, num6);
				object valueFromProperty = GetValueFromProperty(current, strProperty);
				if (valueFromProperty != null)
				{
					switch (list[num5])
					{
					case TypeCode.String:
					{
						string text = (string)GetValueFromProperty(current, strProperty);
						if (text != null && text.Length != 0)
						{
							migrantRange.SetValue(text);
						}
						break;
					}
					case TypeCode.Int32:
						migrantRange.SetValue((int)valueFromProperty);
						break;
					case TypeCode.Int16:
						migrantRange.SetValue(Convert.ToInt16(valueFromProperty));
						break;
					case TypeCode.Double:
						migrantRange.SetValue((double)valueFromProperty);
						break;
					case TypeCode.Int64:
					case TypeCode.Decimal:
						migrantRange.SetValue(Convert.ToDouble(valueFromProperty));
						break;
					case TypeCode.Boolean:
						migrantRange.SetValue((bool)valueFromProperty);
						break;
					case TypeCode.DateTime:
						migrantRange.SetValue((DateTime)valueFromProperty);
						break;
					}
				}
				num5++;
				num6++;
			}
			num2++;
		}
		while (enumerator.MoveNext());
		m_chart.DataRange = this[rowIndex, columnIndex, num2 + rowIndex, propertyInfo.Count + columnIndex - 1];
	}

	public void Clear()
	{
		IWorkbook workbook = m_chart.Workbook;
		int activeSheetIndex = workbook.ActiveSheetIndex;
		if ((workbook as WorkbookImpl).Objects[activeSheetIndex] is WorksheetImpl worksheetImpl)
		{
			string[] array = worksheetImpl.UsedRange.AddressR1C1Local.Split('R', 'C');
			if (array[1] + array[2] != "00")
			{
				worksheetImpl.UsedRange.Clear();
			}
		}
		if (m_chart.Categories != null)
		{
			m_chart.Categories.Clear();
		}
		if (m_chart.Series != null)
		{
			m_chart.Series.Clear();
		}
		m_chart.IsChartCleared = true;
	}

	private string[] GetHeaderTypes(object[][] data)
	{
		string[] array = new string[data[0].Length];
		for (int i = 0; i < data[0].Length; i++)
		{
			array[i] = data[0][i].GetType().Name;
		}
		return array;
	}

	private List<TypeCode> GetObjectMembersInfo(object obj, out List<PropertyInfo> propertyInfo)
	{
		Type type = obj.GetType();
		List<TypeCode> list = new List<TypeCode>();
		propertyInfo = new List<PropertyInfo>();
		PropertyInfo[] array = type.GetRuntimeProperties().ToArray();
		foreach (PropertyInfo propertyInfo2 in array)
		{
			propertyInfo.Add(propertyInfo2);
			list.Add(TypeExtension.GetTypeCode(propertyInfo2.PropertyType));
		}
		return list;
	}

	private object GetValueFromProperty(object value, PropertyInfo strProperty)
	{
		if (strProperty == null)
		{
			throw new ArgumentOutOfRangeException("Can't find property");
		}
		value = strProperty.GetValue(value, null);
		return value;
	}

	private void ClearCacheValues()
	{
		m_chart.CategoryLabelValues = null;
		foreach (ChartSerieImpl item in m_chart.Series)
		{
			item.EnteredDirectlyCategoryLabels = null;
			item.EnteredDirectlyValues = null;
			if (m_chart.ChartType == OfficeChartType.Bubble || m_chart.ChartType == OfficeChartType.Bubble_3D)
			{
				item.EnteredDirectlyBubbles = null;
			}
		}
		IsSerieEmpty = true;
	}
}
