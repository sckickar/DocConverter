using System;
using System.Text;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation;

internal class SubTotalImpl
{
	private bool m_replace;

	private bool m_pageBreaks;

	private bool m_summaryBelowData;

	private bool m_groupRows;

	private ConsolidationFunction consolidationFunction;

	private int[] m_totalList;

	private int irow;

	private int m_groupBy;

	private int columnCount;

	private RecordTable m_recordTable;

	private string[] columnName;

	private string total = "Total";

	private string grandTotal = "Grand Total";

	private int m_height;

	private OfficeVersion m_version;

	private int blockSize;

	private WorksheetImpl m_worksheet;

	private int m_firstRow;

	private int m_firstColumn;

	private int m_lastRow;

	private int m_lastColumn;

	private int m_noOfsubTotals;

	internal SubTotalImpl(WorksheetImpl worksheet)
	{
		m_groupRows = true;
		m_worksheet = worksheet;
		m_recordTable = worksheet.CellRecords.Table;
		m_height = worksheet.AppImplementation.StandardHeightInRowUnits;
		blockSize = worksheet.AppImplementation.RowStorageAllocationBlockSize;
		m_version = worksheet.Version;
	}

	internal int CalculateSubTotal(int firstRow, int firstColumn, int lastRow, int lastColumn, int groupBy, ConsolidationFunction function, int noOfSubtotal, int[] totalList, bool replace, bool pageBreaks, bool summaryBelowData)
	{
		m_firstRow = firstRow;
		m_firstColumn = firstColumn;
		m_lastRow = lastRow;
		m_lastColumn = lastColumn;
		m_noOfsubTotals = noOfSubtotal;
		m_groupBy = groupBy;
		consolidationFunction = function;
		m_totalList = totalList;
		m_replace = replace;
		m_pageBreaks = pageBreaks;
		m_summaryBelowData = summaryBelowData;
		total = GetEnumerationString(function);
		grandTotal = "Grand " + total;
		columnCount = firstColumn + groupBy;
		bool flag = false;
		for (int num = totalList.Length - 1; num >= 0; num--)
		{
			if (totalList[num] == groupBy)
			{
				flag = true;
			}
		}
		if (flag)
		{
			flag = false;
			for (int num2 = groupBy - 1; num2 >= 0; num2--)
			{
				flag = true;
				for (int num3 = totalList.Length - 1; num3 >= 0; num3--)
				{
					if (totalList[num3] == num2)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					columnCount = firstColumn + num2;
					break;
				}
			}
			if (!flag)
			{
				columnCount = firstColumn;
				m_worksheet.InsertColumn(firstColumn + 1);
				firstColumn++;
				lastColumn++;
			}
		}
		columnName = new string[totalList.Length];
		for (int i = 0; i < totalList.Length; i++)
		{
			columnName[i] = RangeImpl.GetColumnName(totalList[i] + firstColumn + 1);
			totalList[i] += firstColumn;
		}
		irow = firstRow;
		irow = firstRow;
		m_groupBy = firstColumn + groupBy;
		if (!replace)
		{
			summaryBelowData = !HasSubTotal(firstRow);
		}
		else
		{
			for (irow = firstRow; irow <= lastRow; irow++)
			{
				RowStorage orCreateRow = m_recordTable.GetOrCreateRow(irow, m_height, bCreate: false, m_version);
				if (orCreateRow != null)
				{
					for (int j = 0; j < totalList.Length; j++)
					{
						ICellPositionFormat record = orCreateRow.GetRecord(totalList[j], blockSize);
						if (record != null && record.TypeCode == TBIFFRecord.Formula && m_worksheet.GetValue(record, preserveOLEDate: false).StartsWith("=SUBTOTAL("))
						{
							m_worksheet.DeleteRow(irow + 1);
							lastRow--;
							break;
						}
					}
				}
			}
		}
		if (summaryBelowData)
		{
			CreateTotalBelowData();
		}
		else
		{
			CreateTotalAboveData();
		}
		return m_lastRow;
	}

	private int SubTotalColumnIndex(RowStorage rowStorage)
	{
		int num = -1;
		for (int i = m_firstColumn; i <= m_lastColumn; i++)
		{
			ICellPositionFormat record = rowStorage.GetRecord(i, blockSize);
			if (record != null && record.TypeCode != TBIFFRecord.Blank)
			{
				if (record.TypeCode != TBIFFRecord.Formula && num == -1)
				{
					num = i;
				}
				if (record.TypeCode == TBIFFRecord.Formula && m_worksheet.GetValue(record, preserveOLEDate: false).StartsWith("=SUBTOTAL("))
				{
					return num;
				}
			}
		}
		return -1;
	}

	private bool HasSubTotal(int irow)
	{
		RowStorage orCreateRow = m_recordTable.GetOrCreateRow(irow, m_height, bCreate: false, m_version);
		if (orCreateRow != null)
		{
			for (int i = m_firstColumn; i <= m_lastColumn; i++)
			{
				ICellPositionFormat record = orCreateRow.GetRecord(i, blockSize);
				if (record != null && record.TypeCode == TBIFFRecord.Formula && m_worksheet.GetValue(record, preserveOLEDate: false).StartsWith("=SUBTOTAL("))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool HasSubTotal(RowStorage rowStorage)
	{
		for (int i = m_firstColumn; i <= m_lastColumn; i++)
		{
			ICellPositionFormat record = rowStorage.GetRecord(i, blockSize);
			if (record != null && record.TypeCode == TBIFFRecord.Formula && m_worksheet.GetValue(record, preserveOLEDate: false).StartsWith("=SUBTOTAL("))
			{
				return true;
			}
		}
		return false;
	}

	private void CreateTotalBelowData()
	{
		bool flag = false;
		int num = -1;
		string text = "";
		int num2 = -1;
		int num3 = (int)consolidationFunction;
		string value = "=SUBTOTAL(" + num3 + ",";
		IRange range = null;
		string text2 = null;
		for (irow = m_firstRow; irow < m_lastRow; irow++)
		{
			bool flag2 = false;
			RowStorage orCreateRow = m_recordTable.GetOrCreateRow(irow, m_height, bCreate: false, m_version);
			if (orCreateRow == null)
			{
				continue;
			}
			ICellPositionFormat record = orCreateRow.GetRecord(m_groupBy, blockSize);
			if (record == null || record.TypeCode == TBIFFRecord.Blank)
			{
				if (m_replace)
				{
					continue;
				}
				if (m_lastRow == irow)
				{
					flag = true;
				}
				else
				{
					if (SubTotalColumnIndex(orCreateRow) != -1)
					{
						num = irow;
						flag2 = true;
					}
					if (!flag2)
					{
						continue;
					}
				}
			}
			if (!flag2)
			{
				string value2 = m_worksheet.GetValue(record, preserveOLEDate: true);
				if (num2 == -1)
				{
					text = value2;
					num2 = ((irow > m_firstRow) ? m_firstRow : irow);
					continue;
				}
				if (string.Compare(value2, text, StringComparison.OrdinalIgnoreCase) == 0 || num2 + 1 > irow)
				{
					text = value2;
					continue;
				}
				if (!m_replace && HasSubTotal(irow))
				{
					if (num == -1)
					{
						num = irow + 1;
					}
					if (irow != m_lastRow)
					{
						continue;
					}
					flag = true;
				}
			}
			m_worksheet.InsertRow(irow + 1);
			m_lastRow++;
			for (int i = 0; i < m_totalList.Length; i++)
			{
				StringBuilder stringBuilder = new StringBuilder(value);
				stringBuilder.Append(columnName[i]);
				stringBuilder.Append(num2 + 1);
				stringBuilder.Append(':');
				stringBuilder.Append(columnName[i]);
				stringBuilder.Append(irow);
				stringBuilder.Append(')');
				m_worksheet[irow + 1, m_totalList[i] + 1].Formula = stringBuilder.ToString();
			}
			range = m_worksheet[irow + 1, columnCount + 1];
			IRange range2 = m_worksheet[irow, columnCount + 1];
			text2 = (range2.HasDateTime ? range2.Value : ((!range2.HasFormula) ? text : range2.DisplayText));
			if (text2.StartsWith("="))
			{
				range.Formula = text2;
				range2 = m_worksheet[irow - m_noOfsubTotals, columnCount + 1];
				text2 = range2.DisplayText;
			}
			range.Value = text2 + " " + total;
			range.CellStyle.Font.Bold = true;
			if (m_groupRows)
			{
				if (num != -1)
				{
					m_worksheet[num2 + 1, m_firstColumn + 1, num, m_lastColumn + 1].Group(OfficeGroupBy.ByRows, bCollapsed: false);
				}
				else
				{
					m_worksheet[num2 + 1, m_firstColumn + 1, irow, m_lastColumn + 1].Group(OfficeGroupBy.ByRows, bCollapsed: false);
				}
			}
			if (m_pageBreaks)
			{
			}
			text = m_worksheet.GetValue(record, preserveOLEDate: true);
			if (num != -1)
			{
				irow++;
				num2 = irow + 1;
				irow++;
				while (irow < m_lastRow)
				{
					orCreateRow = m_recordTable.GetOrCreateRow(irow, m_height, bCreate: false, m_version);
					if (orCreateRow != null && HasSubTotal(orCreateRow))
					{
						irow++;
						continue;
					}
					num2 = irow;
					break;
				}
				if (irow == m_lastRow)
				{
					flag = true;
				}
				else
				{
					irow--;
				}
			}
			else
			{
				irow++;
				num2 = irow;
			}
			num = -1;
			if (flag)
			{
				break;
			}
		}
		if (!flag && num2 != -1)
		{
			m_worksheet.InsertRow(irow + 1, 1);
			range = m_worksheet[irow + 1, columnCount + 1];
			IRange range3 = m_worksheet[irow, columnCount + 1];
			text2 = (range3.HasDateTime ? range3.Value : ((!range3.HasFormula) ? text : range3.DisplayText));
			if (text2.StartsWith("="))
			{
				range.Formula = text2;
				range3 = m_worksheet[irow - m_noOfsubTotals, columnCount + 1];
				text2 = range3.DisplayText;
			}
			range.Value = text2 + " " + total;
			range.CellStyle.Font.Bold = true;
			if (m_groupRows)
			{
				m_worksheet[num2 + 1, m_firstColumn + 1, irow, m_lastColumn + 1].Group(OfficeGroupBy.ByRows, bCollapsed: false);
			}
			for (int j = 0; j < m_totalList.Length; j++)
			{
				StringBuilder stringBuilder2 = new StringBuilder(value);
				stringBuilder2.Append(columnName[j]);
				stringBuilder2.Append(num2 + 1);
				stringBuilder2.Append(':');
				stringBuilder2.Append(columnName[j]);
				stringBuilder2.Append(irow);
				stringBuilder2.Append(')');
				m_worksheet[irow + 1, m_totalList[j] + 1].Formula = stringBuilder2.ToString();
			}
			irow++;
			if (!m_replace && m_noOfsubTotals > 1)
			{
				irow += m_noOfsubTotals - 1;
			}
			m_worksheet.InsertRow(irow + 1, 1);
			range = m_worksheet[irow + 1, columnCount + 1];
			range.Value = grandTotal;
			range.CellStyle.Font.Bold = true;
			for (int k = 0; k < m_totalList.Length; k++)
			{
				StringBuilder stringBuilder3 = new StringBuilder(value);
				stringBuilder3.Append(columnName[k]);
				stringBuilder3.Append(m_firstRow + 1);
				stringBuilder3.Append(':');
				stringBuilder3.Append(columnName[k]);
				stringBuilder3.Append(m_lastRow + 1);
				stringBuilder3.Append(')');
				m_worksheet[irow + 1, m_totalList[k] + 1].Formula = stringBuilder3.ToString();
			}
		}
		if (m_groupRows && !flag)
		{
			m_worksheet[m_firstRow + 1, m_firstColumn + 1, irow, m_lastColumn + 1].Group(OfficeGroupBy.ByRows, bCollapsed: false);
		}
	}

	private void CreateTotalAboveData()
	{
		bool flag = false;
		int num = -1;
		string text = "";
		int num2 = -1;
		int num3 = (int)consolidationFunction;
		string value = "=SUBTOTAL(" + num3 + ",";
		for (irow = m_lastRow; irow >= m_firstRow; irow--)
		{
			ICellPositionFormat record = m_recordTable.GetOrCreateRow(irow, m_height, bCreate: true, m_version).GetRecord(m_groupBy, blockSize);
			if (record == null || record.TypeCode == TBIFFRecord.Blank)
			{
				continue;
			}
			string value2 = m_worksheet.GetValue(record, preserveOLEDate: true);
			if (num2 == -1)
			{
				text = value2;
				num2 = ((irow < m_lastRow) ? m_lastRow : irow);
			}
			else
			{
				if (value2 == text)
				{
					continue;
				}
				if (!m_replace)
				{
					RowStorage orCreateRow = m_recordTable.GetOrCreateRow(irow, m_height, bCreate: false, m_version);
					bool flag2 = false;
					for (int i = 0; i < m_totalList.Length; i++)
					{
						record = orCreateRow.GetRecord(m_totalList[i], blockSize);
						if (record != null && record.TypeCode == TBIFFRecord.Formula && m_worksheet.GetValue(record, preserveOLEDate: false).StartsWith("=SUBTOTAL("))
						{
							flag2 = true;
							break;
						}
					}
					if (flag2)
					{
						if (num == -1)
						{
							num = irow;
						}
						if (m_summaryBelowData || irow != m_firstRow)
						{
							continue;
						}
						flag = true;
					}
				}
				int num4 = irow + 1;
				num2++;
				m_worksheet.InsertRow(num4 + 1);
				m_lastRow++;
				for (int j = 0; j < m_totalList.Length; j++)
				{
					StringBuilder stringBuilder = new StringBuilder(value);
					stringBuilder.Append(columnName[j]);
					stringBuilder.Append(num4 + 2);
					stringBuilder.Append(':');
					stringBuilder.Append(columnName[j]);
					stringBuilder.Append(num2 + 1);
					stringBuilder.Append(')');
					m_worksheet[num4 + 1, m_totalList[j] + 1].Formula = stringBuilder.ToString();
				}
				IRange range = m_worksheet[num4 + 1, columnCount + 1];
				range.Value = text + " " + total;
				range.CellStyle.Font.Bold = true;
				if (m_groupRows)
				{
					if (num != -1)
					{
						m_worksheet[num + 2 + 1, m_firstColumn + 1, num2 + 1, m_lastColumn + 1].Group(OfficeGroupBy.ByRows, bCollapsed: false);
					}
					else
					{
						m_worksheet[num4 + 1 + 1, m_firstColumn + 1, num2 + 1, m_lastColumn + 1].Group(OfficeGroupBy.ByRows, bCollapsed: false);
					}
				}
				num = -1;
				if (m_pageBreaks)
				{
				}
				text = value2;
				num2 = irow;
				if (flag)
				{
					break;
				}
			}
		}
		irow++;
		if (!flag)
		{
			num2++;
			m_worksheet.InsertRow(irow + 1, 1);
			m_lastRow++;
			IRange range2 = m_worksheet[irow + 1, columnCount + 1];
			range2.Value = text + " " + total;
			range2.CellStyle.Font.Bold = true;
			if (m_groupRows)
			{
				m_worksheet[irow + 1 + 1, m_firstColumn + 1, num2 + 1, m_lastColumn + 1].Group(OfficeGroupBy.ByRows, bCollapsed: false);
			}
			for (int k = 0; k < m_totalList.Length; k++)
			{
				StringBuilder stringBuilder2 = new StringBuilder(value);
				stringBuilder2.Append(columnName[k]);
				stringBuilder2.Append(irow + 2);
				stringBuilder2.Append(':');
				stringBuilder2.Append(columnName[k]);
				stringBuilder2.Append(num2 + 1);
				stringBuilder2.Append(')');
				m_worksheet[irow + 1, m_totalList[k] + 1].Formula = stringBuilder2.ToString();
			}
			if (m_replace)
			{
				m_worksheet.InsertRow(irow + 1, 1);
				m_lastRow++;
				IRange range3 = m_worksheet[irow + 1, columnCount + 1];
				range3.Value = grandTotal;
				range3.CellStyle.Font.Bold = true;
				for (int l = 0; l < m_totalList.Length; l++)
				{
					StringBuilder stringBuilder3 = new StringBuilder(value);
					stringBuilder3.Append(columnName[l]);
					stringBuilder3.Append(m_firstRow + 2);
					stringBuilder3.Append(':');
					stringBuilder3.Append(columnName[l]);
					stringBuilder3.Append(m_lastRow + 1);
					stringBuilder3.Append(')');
					m_worksheet[irow + 1, m_totalList[l] + 1].Formula = stringBuilder3.ToString();
				}
			}
		}
		if (m_groupRows)
		{
			m_worksheet[m_firstRow + 1 + 1, m_firstColumn + 1, m_lastRow + 1, m_lastColumn + 1].Group(OfficeGroupBy.ByRows, bCollapsed: false);
		}
	}

	private static string GetEnumerationString(ConsolidationFunction consolidationFunction)
	{
		switch (consolidationFunction)
		{
		case ConsolidationFunction.CountNums:
		case ConsolidationFunction.Count:
			return ConsolidationFunction.Count.ToString();
		case ConsolidationFunction.Average:
			return ConsolidationFunction.Average.ToString();
		case ConsolidationFunction.Max:
			return ConsolidationFunction.Max.ToString();
		case ConsolidationFunction.Min:
			return ConsolidationFunction.Min.ToString();
		case ConsolidationFunction.Product:
			return ConsolidationFunction.Product.ToString();
		case ConsolidationFunction.StdDev:
			return ConsolidationFunction.StdDev.ToString();
		case ConsolidationFunction.StdDevp:
			return ConsolidationFunction.StdDevp.ToString();
		case ConsolidationFunction.Var:
			return ConsolidationFunction.Var.ToString();
		case ConsolidationFunction.Varp:
			return ConsolidationFunction.Varp.ToString();
		default:
			return "Total";
		}
	}
}
