using System;
using System.Collections;
using System.Collections.Generic;

namespace DocGen.DocIO.DLS;

internal class WTableColumnCollection : CollectionImpl
{
	private new IWordDocument m_doc;

	internal new IWordDocument Document => m_doc;

	internal WTableColumn this[int index] => base.InnerList[index] as WTableColumn;

	internal WTableColumnCollection(WordDocument doc)
		: base(doc, doc)
	{
		m_doc = doc;
	}

	internal void AddColumns(float offset)
	{
		WTableColumn wTableColumn = new WTableColumn();
		wTableColumn.EndOffset = offset;
		base.InnerList.Add(wTableColumn);
	}

	internal void UpdateColumns(int currentColumnIndex, int columnSpan, float preferredWidthEndOffset, ColumnSizeInfo sizeInfo, Dictionary<int, float> preferredWidths, bool isGridAfterColumn)
	{
		if (preferredWidths.ContainsKey(columnSpan - 1))
		{
			if (preferredWidths[columnSpan - 1] < preferredWidthEndOffset)
			{
				preferredWidths[columnSpan - 1] = preferredWidthEndOffset;
			}
		}
		else
		{
			preferredWidths.Add(columnSpan - 1, preferredWidthEndOffset);
		}
		if (!isGridAfterColumn && columnSpan - currentColumnIndex == 1 && sizeInfo.MinimumWordWidth > (base.InnerList[columnSpan - 1] as WTableColumn).MinimumWordWidth)
		{
			(base.InnerList[columnSpan - 1] as WTableColumn).MinimumWordWidth = sizeInfo.MinimumWordWidth;
		}
		if (!isGridAfterColumn && columnSpan - currentColumnIndex == 1 && sizeInfo.MaximumWordWidth > (base.InnerList[columnSpan - 1] as WTableColumn).MaximumWordWidth)
		{
			(base.InnerList[columnSpan - 1] as WTableColumn).MaximumWordWidth = sizeInfo.MaximumWordWidth;
			(base.InnerList[columnSpan - 1] as WTableColumn).HasMaximumWordWidth = ((!(base.InnerList[columnSpan - 1] as WTableColumn).HasMaximumWordWidth) ? sizeInfo.HasMaximumWordWidth : (base.InnerList[columnSpan - 1] as WTableColumn).HasMaximumWordWidth);
		}
		if (!isGridAfterColumn && columnSpan - currentColumnIndex == 1 && sizeInfo.MaxParaWidth > (base.InnerList[columnSpan - 1] as WTableColumn).MaxParaWidth)
		{
			(base.InnerList[columnSpan - 1] as WTableColumn).MaxParaWidth = sizeInfo.MaxParaWidth;
		}
		if (columnSpan - currentColumnIndex == 1 && sizeInfo.MinimumWidth > (base.InnerList[columnSpan - 1] as WTableColumn).MinimumWidth)
		{
			(base.InnerList[columnSpan - 1] as WTableColumn).MinimumWidth = sizeInfo.MinimumWidth;
		}
	}

	internal void UpdatePreferredWidhToColumns(Dictionary<int, float> preferredWidths)
	{
		for (int num = base.InnerList.Count - 1; num >= 0; num--)
		{
			WTableColumn wTableColumn = base.InnerList[num] as WTableColumn;
			if (preferredWidths.ContainsKey(num))
			{
				float num2 = preferredWidths[num] - GetPreviousColumnWidth(num, preferredWidths);
				if (num2 > 0f)
				{
					wTableColumn.PreferredWidth = num2;
				}
			}
		}
		preferredWidths.Clear();
	}

	private float GetPreviousColumnWidth(int columnIndex, Dictionary<int, float> preferredWidths)
	{
		for (int num = columnIndex - 1; num >= 0; num--)
		{
			if (preferredWidths.ContainsKey(num))
			{
				return preferredWidths[num];
			}
		}
		return 0f;
	}

	internal int IndexOf(WTableColumn ownerColumn)
	{
		return base.InnerList.IndexOf(ownerColumn);
	}

	internal int IndexOf(float offSet)
	{
		int result = -1;
		foreach (WTableColumn inner in base.InnerList)
		{
			if (inner.EndOffset == offSet)
			{
				return base.InnerList.IndexOf(inner);
			}
		}
		return result;
	}

	internal bool Contains(float offSet)
	{
		foreach (WTableColumn inner in base.InnerList)
		{
			if (inner.EndOffset == offSet)
			{
				return true;
			}
		}
		return false;
	}

	internal void InsertColumn(int coulmnIndex, float offset)
	{
		WTableColumn wTableColumn = new WTableColumn();
		wTableColumn.EndOffset = offset;
		base.InnerList.Insert(coulmnIndex, wTableColumn);
	}

	internal void RemoveColumn(int columnIndex)
	{
		base.InnerList.RemoveAt(columnIndex);
	}

	internal void ResetColumns()
	{
		base.InnerList.Clear();
	}

	internal WTableColumnCollection Clone()
	{
		WTableColumnCollection wTableColumnCollection = new WTableColumnCollection(m_doc as WordDocument);
		foreach (WTableColumn inner in base.InnerList)
		{
			wTableColumnCollection.AddColumns(inner.EndOffset);
		}
		return wTableColumnCollection;
	}

	internal void AutoFitColumns(float containerWidth, float preferredTableWidth, bool isAutoWidth, bool forceAutoFitToContent, WTable table)
	{
		if (forceAutoFitToContent && (m_doc as WordDocument).IsDOCX() && !table.IsInCell && table.HasOnlyParagraphs && GetTotalWidth(3) < preferredTableWidth)
		{
			for (int i = 0; i < base.InnerList.Count; i++)
			{
				WTableColumn obj = base.InnerList[i] as WTableColumn;
				obj.PreferredWidth = obj.MaxParaWidth;
			}
			return;
		}
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		for (int j = 0; j < base.InnerList.Count; j++)
		{
			WTableColumn wTableColumn = base.InnerList[j] as WTableColumn;
			num += ((wTableColumn.PreferredWidth > wTableColumn.MaximumWordWidth) ? wTableColumn.PreferredWidth : wTableColumn.MaximumWordWidth);
			num2 += ((wTableColumn.PreferredWidth > wTableColumn.MinimumWordWidth) ? wTableColumn.PreferredWidth : wTableColumn.MinimumWordWidth);
			float num4 = ((wTableColumn.PreferredWidth == 0f) ? wTableColumn.MinimumWordWidth : ((wTableColumn.PreferredWidth > wTableColumn.MinimumWordWidth) ? wTableColumn.PreferredWidth : wTableColumn.MinimumWordWidth));
			float num5 = wTableColumn.MaximumWordWidth - num4;
			num3 += ((num5 > 0f) ? num5 : 0f);
		}
		if (Math.Round(num, 2) <= Math.Round(preferredTableWidth, 2))
		{
			for (int k = 0; k < base.InnerList.Count; k++)
			{
				WTableColumn wTableColumn2 = base.InnerList[k] as WTableColumn;
				if (wTableColumn2.PreferredWidth < wTableColumn2.MaximumWordWidth)
				{
					wTableColumn2.PreferredWidth = wTableColumn2.MaximumWordWidth;
				}
			}
			if (!isAutoWidth)
			{
				FitColumns(containerWidth, preferredTableWidth, isAutoWidth: false, table, isTableGridCorrupts: false);
			}
			return;
		}
		if (!isAutoWidth)
		{
			float totalWidth = GetTotalWidth(1);
			containerWidth = ((!(preferredTableWidth < totalWidth)) ? preferredTableWidth : ((totalWidth < containerWidth) ? totalWidth : containerWidth));
		}
		if (Math.Round(num2, 2) <= Math.Round(preferredTableWidth, 2) || Math.Round(num2, 2) <= Math.Round(containerWidth))
		{
			float num6 = ((containerWidth > preferredTableWidth) ? containerWidth : preferredTableWidth);
			num6 -= num2;
			for (int l = 0; l < base.InnerList.Count; l++)
			{
				WTableColumn wTableColumn3 = base.InnerList[l] as WTableColumn;
				if (wTableColumn3.PreferredWidth < wTableColumn3.MinimumWordWidth)
				{
					wTableColumn3.PreferredWidth = wTableColumn3.MinimumWordWidth;
				}
				float num7 = wTableColumn3.MaximumWordWidth - wTableColumn3.PreferredWidth;
				num7 = ((num7 > 0f) ? num7 : 0f);
				float num8 = num6 * (num7 / num3);
				wTableColumn3.PreferredWidth += (IsNaNOrInfinity(num8) ? 0f : num8);
			}
		}
		else
		{
			float totalWidth2 = GetTotalWidth(1);
			float totalWidth3 = GetTotalWidth(4);
			float num9 = ((totalWidth3 < containerWidth) ? (containerWidth - totalWidth3) : 0f);
			for (int m = 0; m < base.InnerList.Count; m++)
			{
				WTableColumn wTableColumn4 = base.InnerList[m] as WTableColumn;
				float num10 = num9 * wTableColumn4.MinimumWordWidth / totalWidth2;
				num10 = (IsNaNOrInfinity(num10) ? 0f : num10);
				wTableColumn4.PreferredWidth = wTableColumn4.MinimumWidth + num10;
			}
		}
	}

	internal float GetTotalWidth(byte type)
	{
		float num = 0f;
		for (int i = 0; i < base.InnerList.Count; i++)
		{
			WTableColumn wTableColumn = base.InnerList[i] as WTableColumn;
			num += type switch
			{
				3 => wTableColumn.MaxParaWidth, 
				2 => wTableColumn.MaximumWordWidth, 
				1 => wTableColumn.MinimumWordWidth, 
				0 => wTableColumn.PreferredWidth, 
				_ => wTableColumn.MinimumWidth, 
			};
		}
		return num;
	}

	internal bool IsNaNOrInfinity(float value)
	{
		if (!float.IsNaN(value))
		{
			return float.IsInfinity(value);
		}
		return true;
	}

	internal void FitColumns(float containerWidth, float preferredTableWidth, bool isAutoWidth, WTable table, bool isTableGridCorrupts)
	{
		float totalWidth = GetTotalWidth(0);
		float num = preferredTableWidth;
		float ownerWidth = table.GetOwnerWidth();
		float totalWidth2 = GetTotalWidth(3);
		WTableColumnCollection tableGrid = table.TableGrid;
		float totalMaximumWordWidth = (table.IsNeedToRecalculate ? tableGrid.GetTotalWidth(2) : 0f);
		float totalCellPreferredWidth = 0f;
		float totalSpaceToExpand = 0f;
		float totalCellPrefWidth = 0f;
		List<float> calculatedColumnWidth = null;
		int num2 = table.MaximumCellCount();
		List<int> pctCellWidthRowIndexes = null;
		bool flag = false;
		if (isAutoWidth)
		{
			Entity entity = ((!table.IsInCell && table.Owner != null && !(table.Owner is WTextBox)) ? table.GetOwnerSection(table) : null);
			num = ((entity == null || table.TableFormat.WrapTextAround || !table.TableFormat.IsAutoResized || !(totalWidth > containerWidth) || !((entity as WSection).PageSetup.Margins.Left + table.IndentFromLeft + totalWidth > (entity as WSection).PageSetup.PageSize.Width)) ? totalWidth : containerWidth);
		}
		if (totalWidth > preferredTableWidth && !isAutoWidth && (table.PreferredTableWidth.WidthType != FtsWidth.Percentage || ((m_doc as WordDocument).IsDOCX() && (m_doc as WordDocument).Settings.CompatibilityMode != CompatibilityMode.Word2013)))
		{
			num = totalWidth;
		}
		if (table.IsNeedToRecalculate && totalMaximumWordWidth <= ownerWidth)
		{
			if (table.TableFormat.PreferredWidth.WidthType == FtsWidth.Percentage)
			{
				RecalculatePctTable(table, tableGrid, totalMaximumWordWidth, num);
			}
			else
			{
				SetMaxWordWidthAsPreferredWidth(tableGrid);
			}
		}
		else if ((m_doc as WordDocument).IsDOCX() && !table.IsInCell && table.TableFormat.IsAutoResized && table.TableFormat.PreferredWidth.WidthType == FtsWidth.Percentage && !table.HasAutoPreferredCellWidth && !table.HasPercentPreferredCellWidth && !table.HasPointPreferredCellWidth && table.HasNonePreferredCellWidth && table.HasOnlyParagraphs && table.MaximumCellCount() == base.Count && num < ownerWidth && totalWidth > totalWidth2)
		{
			float num3 = num - totalWidth2;
			if (num3 > 0f)
			{
				{
					IEnumerator enumerator = GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							WTableColumn obj = (WTableColumn)enumerator.Current;
							float num4 = obj.MaxParaWidth / totalWidth2;
							obj.PreferredWidth = obj.MaxParaWidth + num4 * num3;
						}
					}
					finally
					{
						IDisposable disposable = enumerator as IDisposable;
						if (disposable != null)
						{
							disposable.Dispose();
						}
					}
				}
			}
		}
		else if (IsResizeColumnsToPrefTableWidth(table, tableGrid, ownerWidth, ref totalMaximumWordWidth, ref totalSpaceToExpand) || IsResizeHtmlTableColumnsBasedOnContent(table, tableGrid, ownerWidth, ref totalMaximumWordWidth, ref totalSpaceToExpand))
		{
			ResizeColumnsToPrefTableWidth(tableGrid, totalMaximumWordWidth, totalSpaceToExpand);
			table.IsTableGridVerified = true;
		}
		else if (IsResizeColumnsToClientWidth(table, tableGrid, ownerWidth, ref totalMaximumWordWidth))
		{
			ResizeColumnsToClientWidth(tableGrid, totalMaximumWordWidth, ownerWidth);
			table.IsTableGridVerified = true;
		}
		else if (IsExpandColumnsBasedOnParaWidth(table, tableGrid, ownerWidth))
		{
			ExpandColumnsBasedOnParaWidth(tableGrid);
			table.IsTableGridVerified = true;
		}
		else if (IsExpandColumnsToClientWidth(table, tableGrid, ownerWidth, ref totalMaximumWordWidth))
		{
			ExpandColumnsToClientWidth(tableGrid, totalMaximumWordWidth, ownerWidth);
			table.IsTableGridVerified = true;
		}
		else if (IsTableExceedClientWidth(table, tableGrid, ownerWidth) || (table.IsHtmlTableExceedsClientWidth() && table.MaximumCellCount() == tableGrid.Count && tableGrid.GetTotalWidth(2) > table.PreferredTableWidth.Width) || (table.IsNeedToRecalculate && IsAutoTableUsesNestedTableWidth(table, tableGrid, num2)) || IsTableExceedsTableWidth(table, tableGrid, ownerWidth))
		{
			SetMaxWordWidthAsPreferredWidth(tableGrid);
			table.IsNeedToRecalculate = false;
			table.IsTableGridVerified = true;
			foreach (WTable recalculateTable in table.RecalculateTables)
			{
				if (recalculateTable.IsColumnNotHaveEnoughWidth(tableGrid[recalculateTable.GetOwnerTableCell().Index].PreferredWidth, isConsiderPointsValue: true))
				{
					WTableColumnCollection tableGrid2 = recalculateTable.TableGrid;
					SetMaxWordWidthAsPreferredWidth(tableGrid2);
					recalculateTable.SetCellWidthAsColumnPreferredWidth(recalculateTable, tableGrid2);
					recalculateTable.UpdateRowBeforeAfter(recalculateTable);
					tableGrid2.UpdateEndOffset();
					recalculateTable.IsTableGridVerified = true;
				}
			}
		}
		else if (IsResizeColumnBasedOnMaxParaWidth(table, tableGrid, ownerWidth))
		{
			ResizeColumnBasedOnMaxParaWidth(tableGrid, totalWidth2, ownerWidth);
			table.IsTableGridVerified = true;
		}
		else if (IsAutoTableResizeColumnsBasedOnMaxParaWidth(table, tableGrid, ownerWidth, totalWidth))
		{
			SetMaxParaWidthAsPreferredWidth(tableGrid);
			table.IsTableGridVerified = true;
		}
		else if (!table.IsInCell && table.Owner != null && !(table.Owner.OwnerBase is WTextBox) && (IsResizeColumnBasedOnCellPreferredWidth(table, tableGrid, ownerWidth, ref totalCellPreferredWidth, ref calculatedColumnWidth, num2, totalWidth) || IsResizeColumnBasedOnCellPrefAndMaxWordWidth(table, tableGrid, ownerWidth, ref calculatedColumnWidth) || IsPctTableNotHaveEnoughMaxWordWidth(table, tableGrid, num, ref calculatedColumnWidth)))
		{
			ResizeColumnsBasedOnCalculatedColumnWidth(tableGrid, calculatedColumnWidth);
			table.IsTableGridVerified = true;
		}
		else if (!table.IsInCell && IsAutoTableResizeColumnsBasedOnCellPctPrefWidth(table, ref pctCellWidthRowIndexes, tableGrid, ownerWidth, num2))
		{
			ResizeColumnsBasedOnPctCellPrefWidth(tableGrid, pctCellWidthRowIndexes, table, ownerWidth);
			table.IsTableGridVerified = true;
		}
		else if (IsAutoTableUsesCellPrefWidth(table, tableGrid, num2, ownerWidth))
		{
			tableGrid[0].PreferredWidth = (float)table.Rows[0].Cells[0].CellFormat.PropertiesHash[14];
			tableGrid[1].PreferredWidth = ownerWidth - tableGrid[0].PreferredWidth;
			table.IsTableGridVerified = true;
		}
		else if (IsPctTableUseMaxWordWidth(table, tableGrid, ownerWidth))
		{
			SetMaxWordWidthAsPreferredWidth(tableGrid);
			table.IsTableGridVerified = true;
			flag = true;
		}
		else if (IsPointTableResizeColumnsByCellPrefAndMaxWordWidth(table, tableGrid, ownerWidth, num2))
		{
			ResizePointTableColumnsByCellPrefAndMaxWordWidth(table, tableGrid, num, num2);
			table.IsTableGridVerified = true;
		}
		else if (isTableGridCorrupts && IsPointTableExceedsTableWidth(table, tableGrid, ownerWidth, num2, ref calculatedColumnWidth, ref totalMaximumWordWidth, ref totalCellPrefWidth))
		{
			ResizePointTableToTableWidth(tableGrid, calculatedColumnWidth, totalMaximumWordWidth, table.PreferredTableWidth.Width, totalCellPrefWidth);
			table.IsTableGridVerified = true;
		}
		else if (Math.Round(totalWidth, 2) != Math.Round(num, 2))
		{
			if (table.IsNeedToRecalculate && totalMaximumWordWidth > ownerWidth)
			{
				ResizeColumnsToClientWidth(tableGrid, totalMaximumWordWidth, ownerWidth);
				foreach (WTable recalculateTable2 in table.RecalculateTables)
				{
					WTableColumnCollection tableGrid3 = recalculateTable2.TableGrid;
					totalMaximumWordWidth = tableGrid3.GetTotalWidth(2);
					if (totalMaximumWordWidth > tableGrid[recalculateTable2.GetOwnerTableCell().Index].PreferredWidth)
					{
						ResizeColumnsToClientWidth(tableGrid3, tableGrid3.GetTotalWidth(2), tableGrid[recalculateTable2.GetOwnerTableCell().Index].PreferredWidth);
						recalculateTable2.SetCellWidthAsColumnPreferredWidth(recalculateTable2, tableGrid3);
						recalculateTable2.UpdateRowBeforeAfter(recalculateTable2);
						tableGrid3.UpdateEndOffset();
					}
				}
				if (table.m_recalculateTables != null)
				{
					table.m_recalculateTables.Clear();
					table.m_recalculateTables = null;
				}
			}
			else if (IsPointTableResizeColBasedOnCellPrefWidth(table, num2, tableGrid, ref calculatedColumnWidth, ref totalCellPreferredWidth, num))
			{
				ResizeColumnsBasedOnCalculatedColumnWidth(tableGrid, calculatedColumnWidth);
				table.IsTableGridVerified = true;
				flag = true;
			}
			else
			{
				float num5 = num / totalWidth;
				num5 = (IsNaNOrInfinity(num5) ? 1f : num5);
				for (int i = 0; i < base.InnerList.Count; i++)
				{
					WTableColumn wTableColumn = base.InnerList[i] as WTableColumn;
					wTableColumn.PreferredWidth = num5 * wTableColumn.PreferredWidth;
				}
			}
		}
		if (table.IsInCell && !flag)
		{
			ChecksNestedTableNeedToRecalculate(table, tableGrid, ownerWidth, totalWidth, preferredTableWidth);
		}
	}

	private bool IsResizeColumnBasedOnCellPrefAndMaxWordWidth(WTable table, WTableColumnCollection columns, float clientWidth, ref List<float> calculatedColumnWidth)
	{
		float totalWidth = columns.GetTotalWidth(2);
		WordDocument document = table.Document;
		if (document != null && document.IsDOCX() && table.TableFormat.IsAutoResized && !table.TableFormat.WrapTextAround && table.HasOnlyHorizontalText && table.TableFormat.PreferredWidth.Width == 0f && table.HasPointPreferredCellWidth && !table.HasPercentPreferredCellWidth && !table.HasAutoPreferredCellWidth && !table.HasNonePreferredCellWidth && Math.Round(clientWidth, 2) > Math.Round(totalWidth, 2))
		{
			int num = table.MaxCellCountWithoutSpannedCells(table);
			if (num != 0)
			{
				float totalCellPreferredWidth = 0f;
				List<float> maxPrefCellWidthOfColumns = table.GetMaxPrefCellWidthOfColumns(ref totalCellPreferredWidth, num);
				if (totalCellPreferredWidth < totalWidth)
				{
					float totalColumnWidth = 0f;
					calculatedColumnWidth = GetColumnsWidthBasedOnCellPrefAndMaxWordWidth(ref totalColumnWidth, columns, maxPrefCellWidthOfColumns);
					if (totalColumnWidth <= clientWidth)
					{
						return true;
					}
				}
			}
			if (calculatedColumnWidth != null)
			{
				calculatedColumnWidth.Clear();
				calculatedColumnWidth = null;
			}
		}
		return false;
	}

	private List<float> GetColumnsWidthBasedOnCellPrefAndMaxWordWidth(ref float totalColumnWidth, WTableColumnCollection columns, List<float> maxCellPrefWidth)
	{
		List<float> list = new List<float>();
		for (int i = 0; i < columns.Count; i++)
		{
			float num = 0f;
			float num2 = columns[i].MaximumWordWidth + 0.1f;
			num = ((!(maxCellPrefWidth[i] > num2)) ? num2 : maxCellPrefWidth[i]);
			list.Add(num);
			totalColumnWidth += num;
		}
		return list;
	}

	private void RecalculatePctTable(WTable table, WTableColumnCollection columns, float totalMaximumWordWidth, float tableWidth)
	{
		float num = tableWidth - totalMaximumWordWidth;
		if (num < 0f)
		{
			ResizeColumnsToClientWidth(columns, totalMaximumWordWidth, tableWidth);
		}
		else
		{
			ResizeColumnsToPrefTableWidth(columns, totalMaximumWordWidth, num);
		}
		table.IsTableGridVerified = true;
		foreach (WTable recalculateTable in table.RecalculateTables)
		{
			WTableColumnCollection tableGrid = recalculateTable.TableGrid;
			totalMaximumWordWidth = tableGrid.GetTotalWidth(2);
			WTableColumn wTableColumn = columns[recalculateTable.GetOwnerTableCell().Index];
			float num2 = wTableColumn.PreferredWidth - wTableColumn.MinimumWidth;
			num = num2 - totalMaximumWordWidth;
			if (num < 0f)
			{
				ResizeColumnsToClientWidth(tableGrid, totalMaximumWordWidth, num2);
			}
			else
			{
				ResizeColumnsToPrefTableWidth(tableGrid, totalMaximumWordWidth, num);
			}
			recalculateTable.SetCellWidthAsColumnPreferredWidth(recalculateTable, tableGrid);
			recalculateTable.IsTableGridUpdated = true;
			recalculateTable.IsTableGridVerified = true;
			recalculateTable.UpdateRowBeforeAfter(recalculateTable);
			tableGrid.UpdateEndOffset();
		}
		if (table.m_recalculateTables != null)
		{
			table.m_recalculateTables.Clear();
			table.m_recalculateTables = null;
		}
	}

	private bool IsPointTableExceedsTableWidth(WTable table, WTableColumnCollection columns, float clientWidth, int maxCellCount, ref List<float> maxCellPrefWidth, ref float totalMaximumWordWidth, ref float totalCellPrefWidth)
	{
		if ((m_doc as WordDocument).IsDOCX() && table.TableFormat.IsAutoResized && !table.IsInCell && table.HasOnlyParagraphs && table.PreferredTableWidth.WidthType == FtsWidth.Point && columns.Count == maxCellCount && table.HasPointPreferredCellWidth && !table.HasPercentPreferredCellWidth && !table.HasNonePreferredCellWidth && !table.HasAutoPreferredCellWidth)
		{
			maxCellPrefWidth = table.GetMaxPrefCellWidthOfColumns(ref totalCellPrefWidth, maxCellCount);
			if (totalCellPrefWidth > table.PreferredTableWidth.Width)
			{
				totalMaximumWordWidth = columns.GetTotalWidth(2);
				if (totalMaximumWordWidth < table.PreferredTableWidth.Width)
				{
					return true;
				}
				if (maxCellPrefWidth != null)
				{
					maxCellPrefWidth = new List<float>();
					maxCellPrefWidth = null;
					totalCellPrefWidth = 0f;
				}
			}
		}
		return false;
	}

	private void ResizePointTableToTableWidth(WTableColumnCollection columns, List<float> maxCellPrefWidth, float totalMaximumWordWidth, float tableWidth, float totalCellPrefWidth)
	{
		SetMaxWordWidthAsPreferredWidth(columns);
		float num = tableWidth - totalMaximumWordWidth;
		float num2 = totalCellPrefWidth - totalMaximumWordWidth;
		if (!(num > 0f))
		{
			return;
		}
		int num3 = 0;
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				WTableColumn wTableColumn = (WTableColumn)enumerator.Current;
				float num4 = (maxCellPrefWidth[num3] - wTableColumn.MaximumWordWidth) / num2;
				wTableColumn.PreferredWidth = wTableColumn.MaximumWordWidth + num4 * num;
				num3++;
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
	}

	private bool IsPointTableResizeColumnsByCellPrefAndMaxWordWidth(WTable table, WTableColumnCollection columns, float clientWidth, int maxCellCount)
	{
		if ((m_doc as WordDocument).IsDOCX() && (m_doc as WordDocument).Settings.CompatibilityMode == CompatibilityMode.Word2013 && table.TableFormat.IsAutoResized && !table.IsInCell && table.HasOnlyParagraphs && (!(table.Owner is WTextBody) || !((table.Owner as WTextBody).Owner is BlockContentControl)) && table.PreferredTableWidth.WidthType == FtsWidth.Point && (int)table.PreferredTableWidth.Width > (int)clientWidth && columns.Count == maxCellCount && table.IsColumnNotHaveEnoughWidth(clientWidth, isConsiderPointsValue: false) && table.HasPointPreferredCellWidth && !table.HasPercentPreferredCellWidth && !table.HasNonePreferredCellWidth)
		{
			return !table.HasAutoPreferredCellWidth;
		}
		return false;
	}

	internal bool IsPctTableNotHaveEnoughMaxWordWidth(WTable table, WTableColumnCollection columns, float tableWidth, ref List<float> calculatedColumnWidth)
	{
		if (m_doc is WordDocument && (m_doc as WordDocument).IsDOCX() && table.TableFormat.IsAutoResized && table.PreferredTableWidth.WidthType == FtsWidth.Percentage && !table.IsInCell && table.HasOnlyParagraphs && !table.HasPointPreferredCellWidth && table.HasPercentPreferredCellWidth && !table.HasAutoPreferredCellWidth && !table.HasNonePreferredCellWidth && table.IsColumnsNotHaveEnoughPreferredWidthForWordWidth(columns, tableWidth) && !table.HasMergeCellsInTable(isNeedToConsidervMerge: false))
		{
			calculatedColumnWidth = new List<float>();
			List<int> list = new List<int>();
			float num = 0f;
			for (int i = 0; i < columns.Count; i++)
			{
				if (Math.Round(columns[i].MaximumWordWidth, 1) <= Math.Round(columns[i].PreferredWidth, 1))
				{
					calculatedColumnWidth.Add(columns[i].PreferredWidth);
					list.Add(i);
					num += columns[i].PreferredWidth;
				}
				else
				{
					calculatedColumnWidth.Add(columns[i].MaximumWordWidth);
					num += columns[i].MaximumWordWidth;
				}
			}
			if (num > tableWidth)
			{
				float num2 = (num - tableWidth) / (float)list.Count;
				foreach (int item in list)
				{
					calculatedColumnWidth[item] -= num2;
					if (calculatedColumnWidth[item] < columns[item].MaximumWordWidth)
					{
						calculatedColumnWidth.Clear();
						calculatedColumnWidth = null;
						break;
					}
				}
			}
			else
			{
				calculatedColumnWidth.Clear();
				calculatedColumnWidth = null;
			}
		}
		return calculatedColumnWidth != null;
	}

	private void ResizePointTableColumnsByCellPrefAndMaxWordWidth(WTable table, WTableColumnCollection columns, float tableWidth, int maxCellCount)
	{
		float totalCellPreferredWidth = 0f;
		List<float> maxPrefCellWidthOfColumns = table.GetMaxPrefCellWidthOfColumns(ref totalCellPreferredWidth, maxCellCount);
		List<int> list = new List<int>();
		foreach (WTableColumn column in columns)
		{
			int num = columns.IndexOf(column);
			if (maxPrefCellWidthOfColumns[num] > column.MaximumWordWidth)
			{
				list.Add(num);
				column.PreferredWidth = maxPrefCellWidthOfColumns[num];
			}
			else
			{
				column.PreferredWidth = column.MaximumWordWidth;
			}
		}
		if (list.Count == 0)
		{
			ResizeColumnsToClientWidth(columns, columns.GetTotalWidth(2), tableWidth);
			return;
		}
		float num2 = columns.GetTotalWidth(0) - tableWidth;
		float num3 = 0f;
		foreach (int item in list)
		{
			num3 += columns[item].PreferredWidth - columns[item].MaximumWordWidth;
		}
		foreach (int item2 in list)
		{
			float num4 = columns[item2].PreferredWidth - columns[item2].MaximumWordWidth;
			float num5 = num4 - num2 * (num4 / num3);
			columns[item2].PreferredWidth = num5 + columns[item2].MaximumWordWidth;
		}
	}

	private bool IsPctTableUseMaxWordWidth(WTable table, WTableColumnCollection columns, float clientWidth)
	{
		if ((m_doc as WordDocument).IsDOCX() && table.TableFormat.IsAutoResized && table.PreferredTableWidth.WidthType == FtsWidth.Percentage && table.PreferredTableWidth.Width >= 100f && columns.GetTotalWidth(2) > clientWidth)
		{
			if (table.IsInCell)
			{
				WTable ownerTable = table.GetOwnerTable();
				if (!ownerTable.IsInCell && ownerTable.TableFormat.LeftIndent > 0f && ownerTable.TableFormat.IsAutoResized && ownerTable.PreferredTableWidth.WidthType == FtsWidth.Percentage)
				{
					return ownerTable.PreferredTableWidth.Width >= 100f;
				}
				return false;
			}
			return table.TableFormat.LeftIndent > 0f;
		}
		return false;
	}

	private bool IsPointTableResizeColBasedOnCellPrefWidth(WTable table, int maxCellCount, WTableColumnCollection columns, ref List<float> calculatedColumnWidth, ref float totalCellPreferredWidth, float tableWidth)
	{
		if (m_doc is WordDocument && (m_doc as WordDocument).IsDOCX() && table.TableFormat.IsAutoResized && table.TableFormat.PreferredWidth.WidthType == FtsWidth.Point && table.TableFormat.PreferredWidth.Width > 0f && !table.HasAutoPreferredCellWidth && !table.HasPercentPreferredCellWidth && table.HasPointPreferredCellWidth && !table.HasNonePreferredCellWidth && maxCellCount == columns.Count && !table.HasMergeCellsInTable(isNeedToConsidervMerge: true))
		{
			return CheckPreferredWidth(ref calculatedColumnWidth, table, ref totalCellPreferredWidth, maxCellCount, tableWidth, columns);
		}
		return false;
	}

	private bool CheckPreferredWidth(ref List<float> calculatedColumnWidth, WTable table, ref float totalCellPreferredWidth, int maxCellCount, float tableWidth, WTableColumnCollection columns)
	{
		calculatedColumnWidth = table.GetMaxPrefCellWidthOfColumns(ref totalCellPreferredWidth, maxCellCount);
		if (totalCellPreferredWidth >= tableWidth && calculatedColumnWidth.Count == columns.Count && IsEnoughCalculatedWidth(columns, calculatedColumnWidth, totalCellPreferredWidth, tableWidth))
		{
			return true;
		}
		if (calculatedColumnWidth != null)
		{
			calculatedColumnWidth = new List<float>();
			calculatedColumnWidth = null;
		}
		return false;
	}

	private bool IsTableExceedsTableWidth(WTable table, WTableColumnCollection columns, float clientWidth)
	{
		if ((m_doc as WordDocument).IsDOCX() && !table.IsInCell && table.TableFormat.IsAutoResized && table.TableFormat.PreferredWidth.WidthType == FtsWidth.Point && table.TableFormat.PreferredWidth.Width != 0f && table.TableFormat.PreferredWidth.Width < clientWidth && table.MaximumCellCount() == columns.Count && table.IsColumnNotHaveEnoughWidth(clientWidth, isConsiderPointsValue: true) && !table.HasPointPreferredCellWidth && !table.HasPercentPreferredCellWidth)
		{
			float totalWidth = GetTotalWidth(2);
			if (totalWidth < clientWidth)
			{
				return totalWidth > table.TableFormat.PreferredWidth.Width;
			}
			return false;
		}
		return false;
	}

	private bool IsAutoTableResizeColumnsBasedOnCellPctPrefWidth(WTable table, ref List<int> pctCellWidthRowIndexes, WTableColumnCollection columns, float clientWidth, int maxCellCount)
	{
		if ((m_doc as WordDocument).IsDOCX() && table.TableFormat.IsAutoResized && table.TableFormat.PreferredWidth.WidthType == FtsWidth.Auto && table.HasOnlyParagraphs && table.TableFormat.PreferredWidth.Width == 0f && !table.HasAutoPreferredCellWidth && table.HasPercentPreferredCellWidth && table.HasPointPreferredCellWidth && !table.HasNonePreferredCellWidth && maxCellCount == columns.Count && !table.HasMergeCellsInTable(isNeedToConsidervMerge: true) && Math.Round(clientWidth, 2) > Math.Round(GetTotalWidth(2), 2))
		{
			pctCellWidthRowIndexes = GetPctCellPrefWidthRowIndexes(table, columns, clientWidth);
			if (IsSamePctValues(pctCellWidthRowIndexes, table))
			{
				pctCellWidthRowIndexes.RemoveRange(1, pctCellWidthRowIndexes.Count - 1);
			}
			if (pctCellWidthRowIndexes != null)
			{
				return pctCellWidthRowIndexes.Count > 0;
			}
			return false;
		}
		return false;
	}

	private List<int> GetPctCellPrefWidthRowIndexes(WTable table, WTableColumnCollection columns, float clientWidth)
	{
		List<int> list = new List<int>();
		foreach (WTableRow row in table.Rows)
		{
			float num = 0f;
			for (int i = 0; i < row.Cells.Count && row.Cells[i].PreferredWidth.WidthType == FtsWidth.Percentage; i++)
			{
				if (i == row.Cells.Count - 1)
				{
					list.Add(row.Index);
				}
				num += row.Cells[i].PreferredWidth.Width;
				if (clientWidth * row.Cells[i].PreferredWidth.Width / 100f < columns[i].MaximumWordWidth)
				{
					return null;
				}
			}
			if (num > 100f)
			{
				return null;
			}
		}
		return list;
	}

	private void ResizeColumnsBasedOnPctCellPrefWidth(WTableColumnCollection columns, List<int> rowsContainingPercentCell, WTable table, float clientWidth)
	{
		if (rowsContainingPercentCell.Count != 1)
		{
			return;
		}
		WTableRow wTableRow = table.Rows[rowsContainingPercentCell[0]];
		float num = 0f;
		foreach (WTableCell cell in wTableRow.Cells)
		{
			num += cell.PreferredWidth.Width;
		}
		if (num == 100f)
		{
			for (int i = 0; i < wTableRow.Cells.Count; i++)
			{
				columns[i].PreferredWidth = clientWidth * wTableRow.Cells[i].PreferredWidth.Width / 100f;
			}
		}
		else if (num < 100f)
		{
			float num2 = 100f - num;
			for (int j = 0; j < wTableRow.Cells.Count; j++)
			{
				float num3 = num2 * (wTableRow.Cells[j].PreferredWidth.Width / num);
				columns[j].PreferredWidth = clientWidth * (wTableRow.Cells[j].PreferredWidth.Width / 100f) + clientWidth * (num3 / 100f);
			}
		}
	}

	private bool IsResizeColumnBasedOnCellPreferredWidth(WTable table, WTableColumnCollection columns, float clientWidth, ref float totalCellPreferredWidth, ref List<float> calculatedColumnWidth, int maxCellCount, float totalColumnWidth)
	{
		WordDocument document = table.Document;
		if (document != null && document.IsDOCX() && document.Settings.CompatibilityMode == CompatibilityMode.Word2013 && table.TableFormat.IsAutoResized && table.HasOnlyParagraphs && ((table.TableFormat.PreferredWidth.WidthType == FtsWidth.Auto && table.TableFormat.PreferredWidth.Width == 0f && (table.IsColumnNotHaveEnoughWidth(clientWidth, isConsiderPointsValue: true) || Math.Round(clientWidth, 2) < Math.Round(totalColumnWidth, 2))) || (table.TableFormat.PreferredWidth.WidthType == FtsWidth.Percentage && table.TableFormat.PreferredWidth.Width == 100f)) && !table.HasAutoPreferredCellWidth && !table.HasPercentPreferredCellWidth && table.HasPointPreferredCellWidth && !table.HasNonePreferredCellWidth && maxCellCount == columns.Count && Math.Round(clientWidth, 2) > Math.Round(columns.GetTotalWidth(2), 2) && !table.HasMergeCellsInTable(isNeedToConsidervMerge: true) && table.TableFormat.LeftIndent == 0f)
		{
			calculatedColumnWidth = table.GetMaxPrefCellWidthOfColumns(ref totalCellPreferredWidth, maxCellCount);
			if (Math.Round(totalCellPreferredWidth) >= Math.Round(clientWidth) && columns.Count == calculatedColumnWidth.Count && IsEnoughCalculatedWidth(columns, calculatedColumnWidth, totalCellPreferredWidth, clientWidth))
			{
				return true;
			}
		}
		if (calculatedColumnWidth != null)
		{
			calculatedColumnWidth = new List<float>();
			calculatedColumnWidth = null;
		}
		return false;
	}

	private bool IsEnoughCalculatedWidth(WTableColumnCollection columns, List<float> calculatedColumnWidth, float totalCellPreferredWidth, float clientWidth)
	{
		if (columns.Count > 0)
		{
			float num = 0f;
			if (totalCellPreferredWidth > clientWidth)
			{
				num = totalCellPreferredWidth - clientWidth;
			}
			for (int i = 0; i < columns.Count; i++)
			{
				calculatedColumnWidth[i] -= num * (calculatedColumnWidth[i] / totalCellPreferredWidth);
				if (columns[i].MaximumWordWidth > calculatedColumnWidth[i])
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	private void ResizeColumnsBasedOnCalculatedColumnWidth(WTableColumnCollection columns, List<float> calculatedColumnWidth)
	{
		for (int i = 0; i < columns.Count; i++)
		{
			columns[i].PreferredWidth = calculatedColumnWidth[i];
		}
	}

	private bool IsAutoTableResizeColumnsBasedOnMaxParaWidth(WTable table, WTableColumnCollection columns, float clientWidth, float totalColumnWidth)
	{
		if ((m_doc as WordDocument).IsDOCX() && !table.IsInCell && table.TableFormat.IsAutoResized && table.TableFormat.PreferredWidth.WidthType == FtsWidth.Auto && table.HasOnlyParagraphs && table.TableFormat.PreferredWidth.Width == 0f && table.HasAutoPreferredCellWidth && !table.HasPercentPreferredCellWidth && !table.HasPointPreferredCellWidth && !table.HasNonePreferredCellWidth && (double)totalColumnWidth < Math.Round(columns.GetTotalWidth(3), 2))
		{
			return IsMaxParaWidthExceedsPreferredWidth(clientWidth, columns);
		}
		return false;
	}

	private void SetMaxParaWidthAsPreferredWidth(WTableColumnCollection columns)
	{
		foreach (WTableColumn column in columns)
		{
			column.PreferredWidth = (float)Math.Round(column.MaxParaWidth, 1);
		}
	}

	private bool IsResizeColumnBasedOnMaxParaWidth(WTable table, WTableColumnCollection columns, float clientWidth)
	{
		if ((m_doc as WordDocument).ActualFormatType == FormatType.Docx && (m_doc as WordDocument).Settings.CompatibilityMode == CompatibilityMode.Word2013 && table.TableFormat.IsAutoResized && !table.IsInCell && table.PreferredTableWidth.WidthType == FtsWidth.Percentage && table.PreferredTableWidth.Width == 100f && table.HasOnlyParagraphs && (table.HasPointPreferredCellWidth || table.HasPercentPreferredCellWidth) && !table.HasNonePreferredCellWidth && !table.HasAutoPreferredCellWidth && Math.Round(clientWidth, 2) > Math.Round(columns.GetTotalWidth(3), 2) && Math.Round(columns.GetTotalWidth(0), 2) > Math.Round(clientWidth, 2))
		{
			return IsMaxParaWidthExceedsPreferredWidth(clientWidth, columns);
		}
		return false;
	}

	private bool IsMaxParaWidthExceedsPreferredWidth(float clientWidth, WTableColumnCollection columns)
	{
		if (Math.Round(columns.GetTotalWidth(3), 2) <= Math.Round(clientWidth, 2))
		{
			foreach (WTableColumn column in columns)
			{
				if (column.PreferredWidth < column.MaxParaWidth)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void ResizeColumnBasedOnMaxParaWidth(WTableColumnCollection columns, float totalMaxParaWidth, float clientWidth)
	{
		float num = clientWidth - totalMaxParaWidth;
		foreach (WTableColumn column in columns)
		{
			float num2 = column.MaxParaWidth / totalMaxParaWidth;
			column.PreferredWidth = column.MaxParaWidth + num * num2;
		}
	}

	private bool IsTableExceedClientWidth(WTable table, WTableColumnCollection columns, float clientWidth)
	{
		if ((m_doc as WordDocument).IsDOCX() && table.TableFormat.IsAutoResized && !table.IsInCell && table.MaximumCellCount() == columns.Count && table.PreferredTableWidth.WidthType == FtsWidth.Point && table.HasOnlyParagraphs && table.HasPointPreferredCellWidth && !table.HasPercentPreferredCellWidth && !table.HasNonePreferredCellWidth && table.PreferredTableWidth.Width > clientWidth)
		{
			return columns.GetTotalWidth(2) > table.PreferredTableWidth.Width;
		}
		return false;
	}

	private void SetMaxWordWidthAsPreferredWidth(WTableColumnCollection columns)
	{
		foreach (WTableColumn column in columns)
		{
			column.PreferredWidth = column.MaximumWordWidth;
		}
	}

	private void ChecksNestedTableNeedToRecalculate(WTable nestedTable, WTableColumnCollection columns, float clientWidth, float totalColumnWidth, float tableWidth)
	{
		WTableCell ownerTableCell = nestedTable.GetOwnerTableCell();
		WTable wTable = ((ownerTableCell == null) ? null : ((ownerTableCell.OwnerRow != null) ? ownerTableCell.OwnerRow.OwnerTable : null));
		if (wTable == null)
		{
			return;
		}
		WTableColumnCollection tableGrid = wTable.TableGrid;
		float ownerWidth = wTable.GetOwnerWidth();
		int num = nestedTable.MaximumCellCount();
		int num2 = wTable.MaximumCellCount();
		WTableCell wTableCell = null;
		WTable wTable2 = null;
		WTableColumnCollection wTableColumnCollection = null;
		float maxCellCount = 0f;
		float totalCellPreferredWidth = 0f;
		List<float> calculatedColumnWidth = null;
		bool flag = nestedTable.GetOwnerTableCell().GridSpan == 1 && nestedTable.GetOwnerTableCell().CellFormat.HorizontalMerge == CellMerge.None;
		if (wTable.IsInCell)
		{
			wTableCell = wTable.GetOwnerTableCell();
			wTable2 = ((wTableCell == null) ? null : ((wTableCell.OwnerRow != null) ? wTableCell.OwnerRow.OwnerTable : null));
			if (wTable2 == null)
			{
				return;
			}
			wTableColumnCollection = wTable2.TableGrid;
			ownerWidth = wTable2.GetOwnerWidth();
			maxCellCount = wTable2.MaximumCellCount();
		}
		if (flag && (m_doc as WordDocument).IsDOCX() && nestedTable.HasOnlyParagraphs && !wTable.IsInCell && wTable.TableFormat.IsAutoResized && nestedTable.TableFormat.IsAutoResized && ((nestedTable.TableFormat.PreferredWidth.Width == 0f && nestedTable.TableFormat.PreferredWidth.WidthType == FtsWidth.Auto && wTable.TableFormat.PreferredWidth.Width == 0f && wTable.TableFormat.PreferredWidth.WidthType == FtsWidth.Auto && nestedTable.HasNonePreferredCellWidth && nestedTable.HasPointPreferredCellWidth && !nestedTable.HasPercentPreferredCellWidth && !nestedTable.HasAutoPreferredCellWidth && wTable.HasNonePreferredCellWidth && wTable.HasPointPreferredCellWidth && !wTable.HasPercentPreferredCellWidth && !wTable.HasAutoPreferredCellWidth) || IsNestedPctTableUsesMaxWordWidth(nestedTable, columns, clientWidth, wTable)) && num == columns.Count && num2 == tableGrid.Count)
		{
			SetMaxWordWidthAsPreferredWidth(columns);
			wTable.IsNeedToRecalculate = true;
			wTable.RecalculateTables.Add(nestedTable);
		}
		else if (flag && (m_doc as WordDocument).IsDOCX() && nestedTable.HasOnlyParagraphs && !wTable.IsInCell && wTable.TableFormat.IsAutoResized && nestedTable.TableFormat.IsAutoResized && nestedTable.TableFormat.PreferredWidth.WidthType == FtsWidth.Point && wTable.TableFormat.PreferredWidth.WidthType == FtsWidth.Point && nestedTable.TableFormat.PreferredWidth.Width > clientWidth && wTable.TableFormat.PreferredWidth.Width < ownerWidth && nestedTable.HasNonePreferredCellWidth && !nestedTable.HasPointPreferredCellWidth && !nestedTable.HasPercentPreferredCellWidth && !nestedTable.HasAutoPreferredCellWidth && wTable.HasNonePreferredCellWidth && !wTable.HasPointPreferredCellWidth && !wTable.HasPercentPreferredCellWidth && !wTable.HasAutoPreferredCellWidth && num == columns.Count && num2 == tableGrid.Count)
		{
			wTable.RecalculateTables.Add(nestedTable);
		}
		else if (flag && (m_doc as WordDocument).IsDOCX() && !wTable.IsInCell && !nestedTable.HasOnlyParagraphs && wTable.TableFormat.IsAutoResized && nestedTable.TableFormat.IsAutoResized && nestedTable.TableFormat.PreferredWidth.Width > 0f && nestedTable.TableFormat.PreferredWidth.WidthType == FtsWidth.Point && wTable.TableFormat.PreferredWidth.Width > 0f && wTable.TableFormat.PreferredWidth.Width <= wTable.GetOwnerWidth() && wTable.TableFormat.PreferredWidth.WidthType == FtsWidth.Point && nestedTable.HasNonePreferredCellWidth && !nestedTable.HasPercentPreferredCellWidth && !nestedTable.HasAutoPreferredCellWidth && wTable.HasNonePreferredCellWidth && !wTable.HasPointPreferredCellWidth && !wTable.HasPercentPreferredCellWidth && !wTable.HasAutoPreferredCellWidth && num == columns.Count && num2 == tableGrid.Count)
		{
			foreach (WTableColumn column in columns)
			{
				column.PreferredWidth = column.MaximumWordWidth;
			}
			wTable.IsNeedToRecalculate = true;
		}
		else if (flag && (m_doc as WordDocument).IsDOCX() && nestedTable.HasOnlyParagraphs && !wTable.IsInCell && wTable.TableFormat.IsAutoResized && nestedTable.TableFormat.IsAutoResized && nestedTable.TableFormat.PreferredWidth.Width > 0f && nestedTable.TableFormat.PreferredWidth.WidthType == FtsWidth.Point && wTable.TableFormat.PreferredWidth.Width > 0f && wTable.TableFormat.PreferredWidth.WidthType == FtsWidth.Point && !nestedTable.HasNonePreferredCellWidth && nestedTable.HasPointPreferredCellWidth && !nestedTable.HasPercentPreferredCellWidth && !nestedTable.HasAutoPreferredCellWidth && !wTable.HasNonePreferredCellWidth && wTable.HasPointPreferredCellWidth && !wTable.HasPercentPreferredCellWidth && !wTable.HasAutoPreferredCellWidth && num == columns.Count && num2 == tableGrid.Count && clientWidth < nestedTable.TableFormat.PreferredWidth.Width && wTable.TableFormat.Borders.NoBorder)
		{
			wTable.IsNeedToRecalculate = true;
		}
		else if (flag && (wTable.IsInCell ? IsAutoTableUsesCellPrefWidth(wTable2, wTableColumnCollection, maxCellCount, ownerWidth) : IsAutoTableUsesCellPrefWidth(wTable, tableGrid, num2, ownerWidth)))
		{
			if (!wTable.IsInCell)
			{
				tableGrid[0].PreferredWidth = (float)wTable.Rows[0].Cells[0].CellFormat.PropertiesHash[14];
				tableGrid[1].PreferredWidth = ownerWidth - tableGrid[0].PreferredWidth;
			}
			else
			{
				wTableColumnCollection[0].PreferredWidth = (float)wTable2.Rows[0].Cells[0].CellFormat.PropertiesHash[14];
				wTableColumnCollection[1].PreferredWidth = ownerWidth - wTableColumnCollection[0].PreferredWidth;
			}
			List<int> pctCellWidthRowIndexes = null;
			if (!wTable.IsInCell && IsResizeColumnBasedOnCellPreferredWidth(nestedTable, columns, tableGrid[ownerTableCell.Index].PreferredWidth, ref totalCellPreferredWidth, ref calculatedColumnWidth, num, -1f) && !HasLeftorRightPadding(ownerTableCell))
			{
				ResizeColumnsBasedOnCalculatedColumnWidth(columns, calculatedColumnWidth);
			}
			else if (IsNestedTableUsesOwnerWidth(wTable, tableGrid, num2) && IsAutoTableResizeColumnsBasedOnCellPctPrefWidth(nestedTable, ref pctCellWidthRowIndexes, columns, wTableColumnCollection[wTableCell.Index].PreferredWidth, num) && !HasLeftorRightPadding(wTableCell) && !HasLeftorRightPadding(ownerTableCell))
			{
				ResizeColumnsBasedOnPctCellPrefWidth(columns, pctCellWidthRowIndexes, nestedTable, wTableColumnCollection[wTableCell.Index].PreferredWidth);
			}
			else if (!wTable.IsInCell && IsNestedTableUsesOwnerWidth(nestedTable, columns, num) && !HasLeftorRightPadding(ownerTableCell))
			{
				columns[0].PreferredWidth = tableGrid[ownerTableCell.Index].PreferredWidth;
			}
		}
		else if (flag && IsAutoTableUsesNestedTableWidth(wTable, tableGrid, num2) && nestedTable.TableFormat.IsAutoResized && nestedTable.HasOnlyParagraphs && nestedTable.TableFormat.PreferredWidth.WidthType == FtsWidth.Auto && nestedTable.TableFormat.PreferredWidth.Width == 0f && !nestedTable.HasAutoPreferredCellWidth && !nestedTable.HasPercentPreferredCellWidth && nestedTable.HasPointPreferredCellWidth && !nestedTable.HasNonePreferredCellWidth && num == columns.Count && !nestedTable.HasMergeCellsInTable(isNeedToConsidervMerge: true) && !HasLeftorRightPadding(ownerTableCell) && (ownerTableCell.Index == 0 || wTable.IsNeedToRecalculate))
		{
			int index = ownerTableCell.Index;
			float totalCellPrefWidth = 0f;
			List<float> comparedMaxCellPrefWidth = GetComparedMaxCellPrefWidth(wTable, num2, ref totalCellPrefWidth);
			float totalCellPrefWidth2 = 0f;
			List<float> comparedMaxCellPrefWidth2 = GetComparedMaxCellPrefWidth(nestedTable, num, ref totalCellPrefWidth2);
			clientWidth = ((index == 0) ? totalCellPrefWidth2 : tableGrid[ownerTableCell.Index].PreferredWidth);
			if (comparedMaxCellPrefWidth[index] < totalCellPrefWidth2 && totalCellPrefWidth2 >= clientWidth && clientWidth > columns.GetTotalWidth(2) && IsEnoughCalculatedWidth(columns, comparedMaxCellPrefWidth2, totalCellPrefWidth2, clientWidth))
			{
				if (index == 0)
				{
					tableGrid[0].PreferredWidth = totalCellPrefWidth2;
					tableGrid[1].PreferredWidth = ownerWidth - totalCellPrefWidth2;
				}
				ResizeColumnsBasedOnCalculatedColumnWidth(columns, comparedMaxCellPrefWidth2);
				wTable.IsNeedToRecalculate = true;
			}
		}
		else if (flag && (IsAutoTableUsesCellPointPrefWidth(nestedTable, columns, clientWidth, ref totalCellPreferredWidth, ref calculatedColumnWidth, num) || IsResizeColumnBasedOnCellPrefAndMaxWordWidth(nestedTable, columns, clientWidth, ref calculatedColumnWidth)))
		{
			ResizeColumnsBasedOnCalculatedColumnWidth(columns, calculatedColumnWidth);
			nestedTable.IsTableGridVerified = true;
		}
		else if (Math.Round(totalColumnWidth, 2) != Math.Round(tableWidth, 2) && IsNestedTableShrinkToClientWidth(nestedTable, wTable, totalColumnWidth, tableWidth))
		{
			float num3 = tableWidth / totalColumnWidth;
			num3 = (IsNaNOrInfinity(num3) ? 1f : num3);
			for (int i = 0; i < base.InnerList.Count; i++)
			{
				WTableColumn wTableColumn = base.InnerList[i] as WTableColumn;
				wTableColumn.PreferredWidth = num3 * wTableColumn.PreferredWidth;
			}
		}
	}

	private bool IsNestedTableShrinkToClientWidth(WTable table, WTable ownerTable, float totalColumnWidth, float clientWidth)
	{
		if ((m_doc as WordDocument).IsDOCX() && (m_doc as WordDocument).Settings.CompatibilityMode == CompatibilityMode.Word2013 && ownerTable.TableFormat.IsAutoResized && ownerTable.TableFormat.PreferredWidth.WidthType == FtsWidth.Percentage && ownerTable.TableFormat.PreferredWidth.Width >= 100f && table.TableFormat.PreferredWidth.WidthType == FtsWidth.None && table.TableFormat.PreferredWidth.Width == 0f && !table.HasPercentPreferredCellWidth)
		{
			return totalColumnWidth > clientWidth;
		}
		return false;
	}

	private bool IsNestedPctTableUsesMaxWordWidth(WTable table, WTableColumnCollection columns, float clientWidth, WTable ownerTable)
	{
		if ((m_doc as WordDocument).IsDOCX() && table.TableFormat.IsAutoResized && ownerTable.TableFormat.IsAutoResized && table.TableFormat.PreferredWidth.WidthType == FtsWidth.Percentage && table.TableFormat.PreferredWidth.Width == 100f && ownerTable.TableFormat.PreferredWidth.WidthType == FtsWidth.Percentage && ownerTable.TableFormat.PreferredWidth.Width == 100f)
		{
			return IsColumnNotHaveEnoughMaxWordWidth(columns);
		}
		return false;
	}

	private bool IsColumnNotHaveEnoughMaxWordWidth(WTableColumnCollection columns)
	{
		foreach (WTableColumn column in columns)
		{
			if (column.PreferredWidth < column.MinimumWordWidth)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsAutoTableUsesCellPointPrefWidth(WTable table, WTableColumnCollection columns, float clientWidth, ref float totalCellPreferredWidth, ref List<float> calculatedColumnWidth, int maxCellCount)
	{
		if ((m_doc as WordDocument).IsDOCX() && table.TableFormat.IsAutoResized && table.TableFormat.PreferredWidth.WidthType == FtsWidth.Auto && table.TableFormat.PreferredWidth.Width == 0f && !table.HasMergeCellsInTable(isNeedToConsidervMerge: true) && !table.HasAutoPreferredCellWidth && !table.HasPercentPreferredCellWidth && table.HasPointPreferredCellWidth && !table.HasNonePreferredCellWidth && clientWidth >= columns.GetTotalWidth(0))
		{
			calculatedColumnWidth = table.GetMaxPrefCellWidthOfColumns(ref totalCellPreferredWidth, maxCellCount);
			if (columns.Count == calculatedColumnWidth.Count && clientWidth >= totalCellPreferredWidth && IsCalculatedWidthEnoughForMaxWordWidth(calculatedColumnWidth, columns))
			{
				return true;
			}
		}
		if (calculatedColumnWidth != null)
		{
			calculatedColumnWidth = new List<float>();
			calculatedColumnWidth = null;
		}
		return false;
	}

	private bool IsCalculatedWidthEnoughForMaxWordWidth(List<float> calculateWidth, WTableColumnCollection columns)
	{
		for (int i = 0; i < columns.Count; i++)
		{
			if (calculateWidth[i] < columns[i].MaximumWordWidth)
			{
				return false;
			}
		}
		return true;
	}

	private List<float> GetComparedMaxCellPrefWidth(WTable table, int maxCellCount, ref float totalCellPrefWidth)
	{
		List<float> list = new List<float>();
		for (int i = 0; i < maxCellCount; i++)
		{
			float num = 0f;
			for (int j = 0; j < table.Rows.Count; j++)
			{
				if (i < table.Rows[j].Cells.Count)
				{
					WTableCell wTableCell = table.Rows[j].Cells[i];
					float num2 = ((wTableCell.PreferredWidth.Width > wTableCell.CellFormat.CellWidth) ? wTableCell.PreferredWidth.Width : wTableCell.CellFormat.CellWidth);
					if (num < num2)
					{
						num = num2;
					}
				}
			}
			list.Add(num);
			totalCellPrefWidth += num;
		}
		return list;
	}

	internal bool IsAutoTableUsesNestedTableWidth(WTable table, WTableColumnCollection columns, int maxCellCount)
	{
		if ((m_doc as WordDocument).IsDOCX() && (m_doc as WordDocument).Settings.CompatibilityMode == CompatibilityMode.Word2013 && table != null && !table.IsInCell && table.TableFormat.IsAutoResized && table.TableFormat.PreferredWidth.WidthType == FtsWidth.Auto && table.TableFormat.PreferredWidth.Width == 0f && !table.HasAutoPreferredCellWidth && !table.HasPercentPreferredCellWidth && table.HasPointPreferredCellWidth && !table.HasNonePreferredCellWidth && maxCellCount == columns.Count && !table.HasMergeCellsInTable(isNeedToConsidervMerge: true))
		{
			return maxCellCount == 2;
		}
		return false;
	}

	private bool IsAutoTableUsesCellPrefWidth(WTable table, WTableColumnCollection columns, float maxCellCount, float clientWidth)
	{
		if ((m_doc as WordDocument).IsDOCX() && (m_doc as WordDocument).Settings.CompatibilityMode == CompatibilityMode.Word2013 && table != null && !table.IsInCell && table.TableFormat.IsAutoResized && table.TableFormat.PreferredWidth.WidthType == FtsWidth.Auto && table.TableFormat.PreferredWidth.Width == 0f && !table.HasAutoPreferredCellWidth && !table.HasPercentPreferredCellWidth && table.HasPointPreferredCellWidth && table.HasNonePreferredCellWidth && maxCellCount == (float)columns.Count && !table.HasMergeCellsInTable(isNeedToConsidervMerge: true) && table.Rows.Count == 1 && table.Rows[0].Cells.Count == 2 && table.Rows[0].Cells[0].CellFormat.PropertiesHash.ContainsKey(14) && !table.Rows[0].Cells[1].CellFormat.PropertiesHash.ContainsKey(14))
		{
			return (float)table.Rows[0].Cells[0].CellFormat.PropertiesHash[14] < clientWidth;
		}
		return false;
	}

	private bool IsNestedTableUsesOwnerWidth(WTable table, WTableColumnCollection columns, int maxCellCount)
	{
		if (table.IsInCell && table.TableFormat.IsAutoResized && table.TableFormat.PreferredWidth.WidthType == FtsWidth.Auto && table.TableFormat.PreferredWidth.Width == 0f && !table.HasAutoPreferredCellWidth && !table.HasPercentPreferredCellWidth && !table.HasPointPreferredCellWidth && table.HasNonePreferredCellWidth && maxCellCount == columns.Count && !table.HasMergeCellsInTable(isNeedToConsidervMerge: true))
		{
			return maxCellCount == 1;
		}
		return false;
	}

	internal bool HasLeftorRightPadding(WTableCell ownerCell)
	{
		CellFormat cellFormat = ownerCell.CellFormat;
		RowFormat rowFormat = ownerCell.OwnerRow.RowFormat;
		RowFormat tableFormat = ownerCell.OwnerRow.OwnerTable.TableFormat;
		if (cellFormat.Paddings.HasKey(1) || cellFormat.Paddings.HasKey(4))
		{
			if (cellFormat.Paddings.Left == 0f)
			{
				return cellFormat.Paddings.Right != 0f;
			}
			return true;
		}
		if (rowFormat.Paddings.HasKey(1) || rowFormat.Paddings.HasKey(4))
		{
			if (rowFormat.Paddings.Left == 0f)
			{
				return rowFormat.Paddings.Right != 0f;
			}
			return true;
		}
		if (tableFormat.Paddings.HasKey(1) || tableFormat.Paddings.HasKey(4))
		{
			if (tableFormat.Paddings.Left == 0f)
			{
				return tableFormat.Paddings.Right != 0f;
			}
			return true;
		}
		return false;
	}

	private bool IsSamePctValues(List<int> rowIndexes, WTable table)
	{
		if (rowIndexes == null)
		{
			return false;
		}
		for (int i = 1; i < rowIndexes.Count; i++)
		{
			int index = rowIndexes[i];
			for (int j = 0; j < table.Rows[0].Cells.Count; j++)
			{
				if (table.Rows[rowIndexes[0]].Cells[j].PreferredWidth.Width != table.Rows[index].Cells[j].PreferredWidth.Width)
				{
					return false;
				}
			}
		}
		return true;
	}

	private bool IsExpandColumnsBasedOnParaWidth(WTable table, WTableColumnCollection columns, float clientWidth)
	{
		if ((m_doc as WordDocument).IsDOCX() && !table.IsInCell && table.TableFormat.IsAutoResized && table.TableFormat.PreferredWidth.WidthType == FtsWidth.None && table.TableFormat.PreferredWidth.Width == 0f && table.HasOnlyParagraphs && table.MaximumCellCount() == columns.Count && table.IsColumnNotHaveEnoughWidth(clientWidth, isConsiderPointsValue: true) && table.HasPointPreferredCellWidth && table.HasNonePreferredCellWidth && !table.HasPercentPreferredCellWidth && !table.HasAutoPreferredCellWidth && GetTotalWidth(3) < clientWidth)
		{
			float num = 0f;
			foreach (WTableColumn column in columns)
			{
				num = ((!(column.PreferredWidth > column.MaxParaWidth)) ? (num + (column.MaxParaWidth + 0.1f)) : (num + column.PreferredWidth));
			}
			return num <= clientWidth;
		}
		return false;
	}

	private void ExpandColumnsBasedOnParaWidth(WTableColumnCollection columns)
	{
		foreach (WTableColumn column in columns)
		{
			if (column.PreferredWidth < column.MaxParaWidth)
			{
				column.PreferredWidth = column.MaxParaWidth + 0.1f;
			}
		}
	}

	private bool IsExpandColumnsToClientWidth(WTable table, WTableColumnCollection columns, float clientWidth, ref float totalMaximumWordWidth)
	{
		if ((m_doc as WordDocument).IsDOCX() && !table.IsInCell && table.TableFormat.IsAutoResized && table.TableFormat.PreferredWidth.WidthType == FtsWidth.None && table.TableFormat.PreferredWidth.Width == 0f && table.HasOnlyParagraphs && table.MaximumCellCount() == columns.Count && table.IsColumnNotHaveEnoughWidth(clientWidth, isConsiderPointsValue: true) && table.HasPointPreferredCellWidth && table.HasNonePreferredCellWidth && !table.HasPercentPreferredCellWidth && !table.HasAutoPreferredCellWidth)
		{
			totalMaximumWordWidth = GetTotalWidth(2);
			return totalMaximumWordWidth < clientWidth;
		}
		return false;
	}

	private void ExpandColumnsToClientWidth(WTableColumnCollection columns, float totalMaximumWordWidth, float clientWidth)
	{
		float num = 0f;
		foreach (WTableColumn column in columns)
		{
			if (Math.Round(column.PreferredWidth, 1) <= Math.Round(column.MaximumWordWidth, 1))
			{
				column.PreferredWidth = column.MaximumWordWidth;
				num += column.MaximumWordWidth;
			}
		}
		float num2 = clientWidth - num;
		float num3 = totalMaximumWordWidth - num;
		foreach (WTableColumn column2 in columns)
		{
			if (column2.PreferredWidth != column2.MaximumWordWidth)
			{
				float num4 = column2.MaximumWordWidth / num3;
				column2.PreferredWidth = num2 * num4;
			}
		}
	}

	private bool IsResizeColumnsToClientWidth(WTable table, WTableColumnCollection columns, float clientWidth, ref float totalMaximumWordWidth)
	{
		if ((m_doc as WordDocument).IsDOCX() && !table.IsInCell && table.TableFormat.IsAutoResized && (table.TableFormat.PreferredWidth.WidthType == FtsWidth.None || (table.TableFormat.PreferredWidth.WidthType == FtsWidth.Auto && columns.IsAllColumnsPrefWidthEqual(columns))) && table.TableFormat.PreferredWidth.Width == 0f && table.MaximumCellCount() == base.Count && table.HasOnlyParagraphs && !table.HasPercentPreferredCellWidth && !table.HasAutoPreferredCellWidth && !table.HasNonePreferredCellWidth && table.HasPointPreferredCellWidth)
		{
			totalMaximumWordWidth = columns.GetTotalWidth(2);
			return totalMaximumWordWidth > clientWidth;
		}
		return false;
	}

	private bool IsAllColumnsPrefWidthEqual(WTableColumnCollection columns)
	{
		foreach (WTableColumn column in columns)
		{
			if (column.PreferredWidth != columns[0].PreferredWidth)
			{
				return false;
			}
		}
		return true;
	}

	private void ResizeColumnsToClientWidth(WTableColumnCollection columns, float totalMaximumWordWidth, float clientWidth)
	{
		foreach (WTableColumn column in columns)
		{
			float num = column.MaximumWordWidth / totalMaximumWordWidth;
			column.PreferredWidth = clientWidth * num;
		}
	}

	private bool IsResizeHtmlTableColumnsBasedOnContent(WTable table, WTableColumnCollection columns, float clientWidth, ref float totalMaximumWordWidth, ref float totalSpaceToExpand)
	{
		if ((m_doc as WordDocument).IsDOCX() && table.IsFromHTML && !table.HasAutoPreferredCellWidth && !table.HasNonePreferredCellWidth && !table.HasPercentPreferredCellWidth && table.HasPointPreferredCellWidth && !table.IsInCell && table.TableFormat.IsAutoResized && table.PreferredTableWidth.WidthType == FtsWidth.Point && table.PreferredTableWidth.Width < clientWidth && table.IsColumnNotHaveEnoughWidth(clientWidth, isConsiderPointsValue: true))
		{
			totalMaximumWordWidth = GetTotalWidth(2);
			if (table.PreferredTableWidth.Width > totalMaximumWordWidth)
			{
				totalSpaceToExpand = table.PreferredTableWidth.Width - totalMaximumWordWidth;
				return true;
			}
		}
		return false;
	}

	private bool IsResizeColumnsToPrefTableWidth(WTable table, WTableColumnCollection columns, float clientWidth, ref float totalMaximumWordWidth, ref float totalSpaceToExpand)
	{
		WSection wSection = table.GetOwnerSection(table) as WSection;
		float num = 0f;
		if (table.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 && table.PreferredTableWidth.Width > 0f && table.PreferredTableWidth.WidthType == FtsWidth.Percentage && table.TableFormat.IsAutoResized && !table.IsInCell && wSection != null && table.MaximumCellCount() == table.TableGrid.Count && !table.HasPercentPreferredCellWidth && (table.HasPointPreferredCellWidth || table.HasNonePreferredCellWidth || table.HasAutoPreferredCellWidth) && table.IsColumnNotHaveEnoughWidth(clientWidth, isConsiderPointsValue: true))
		{
			num = clientWidth * table.PreferredTableWidth.Width / 100f;
		}
		if (num > 0f && num > wSection.PageSetup.ClientWidth && num < wSection.PageSetup.PageSize.Width)
		{
			totalMaximumWordWidth = GetTotalWidth(2);
			float num2 = 0f;
			float leftPad = 0f;
			float rightPad = 0f;
			table.CalculatePaddingOfTableWidth(ref leftPad, ref rightPad);
			num2 += leftPad + rightPad;
			if (table.TableFormat.CellSpacing > 0f)
			{
				num2 += table.TableFormat.CellSpacing * 2f + table.TableFormat.Borders.Left.GetLineWidthValue();
				num2 += table.TableFormat.CellSpacing * 2f + table.TableFormat.Borders.Right.GetLineWidthValue();
			}
			totalSpaceToExpand = num - totalMaximumWordWidth;
			return totalSpaceToExpand > num2;
		}
		return false;
	}

	private void ResizeColumnsToPrefTableWidth(WTableColumnCollection columns, float totalMaximumWordWidth, float totalSpaceToExpand)
	{
		foreach (WTableColumn column in columns)
		{
			float num = column.MaximumWordWidth / totalMaximumWordWidth;
			column.PreferredWidth = column.MaximumWordWidth + num * totalSpaceToExpand;
		}
	}

	internal float GetCellWidth(int columnIndex, int columnSpan)
	{
		float num = 0f;
		for (int i = 0; i < columnSpan; i++)
		{
			num += (base.InnerList[i + columnIndex] as WTableColumn).PreferredWidth;
		}
		return num;
	}

	internal void UpdateEndOffset()
	{
		float num = 0f;
		for (int i = 0; i < base.InnerList.Count; i++)
		{
			WTableColumn wTableColumn = base.InnerList[i] as WTableColumn;
			num = ((i != 0) ? (num + wTableColumn.PreferredWidth * 20f) : (wTableColumn.PreferredWidth * 20f));
			wTableColumn.EndOffset = (float)Math.Round(num);
		}
	}

	internal void ValidateColumnWidths()
	{
		for (int i = 0; i < base.InnerList.Count; i++)
		{
			if (i == 0)
			{
				(base.InnerList[i] as WTableColumn).PreferredWidth = (base.InnerList[i] as WTableColumn).EndOffset / 20f;
			}
			else
			{
				(base.InnerList[i] as WTableColumn).PreferredWidth = ((base.InnerList[i] as WTableColumn).EndOffset - (base.InnerList[i - 1] as WTableColumn).EndOffset) / 20f;
			}
		}
	}
}
