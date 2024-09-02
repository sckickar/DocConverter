using System.Collections.Generic;

namespace DocGen.OfficeChart.Implementation;

internal class ColumnCollection : List<Column>
{
	internal Column column;

	private double defaultWidth;

	private WorksheetImpl workSheet;

	internal double Width
	{
		get
		{
			return defaultWidth;
		}
		set
		{
			defaultWidth = value;
		}
	}

	internal ColumnCollection(WorksheetImpl workSheet, double defaultWidth)
	{
		this.workSheet = workSheet;
		this.defaultWidth = defaultWidth;
	}

	public Column GetColumnByIndex(int index)
	{
		return base[index];
	}

	internal Column GetOrCreateColumn()
	{
		if (column == null)
		{
			column = new Column(0, workSheet, defaultWidth);
		}
		return column;
	}

	public double GetWidth(int colIndex, bool isDefaultWidth)
	{
		if (column == null || column.Index > colIndex)
		{
			return defaultWidth;
		}
		if (isDefaultWidth)
		{
			return column.defaultWidth;
		}
		if (!column.IsHidden)
		{
			return column.defaultWidth;
		}
		return 0.0;
	}

	public int GetWidth(int minCol, int maxCol, bool isDefaultWidth, bool isLayout)
	{
		bool flag = workSheet.View == OfficeSheetView.PageLayout;
		double num = ((!isLayout || !flag) ? 1.0 : 1.05);
		int num2 = 0;
		for (int i = minCol; i <= maxCol; i++)
		{
			double width = defaultWidth;
			if (column != null && column.Index <= i)
			{
				width = ((!isDefaultWidth) ? (column.IsHidden ? 0.0 : column.defaultWidth) : column.defaultWidth);
			}
			num2 += WorksheetImpl.CharacterWidth(width, workSheet.GetAppImpl());
		}
		return (int)((double)num2 * num + 0.5);
	}

	internal Column AddColumn(int index)
	{
		Column column = new Column((short)index, workSheet, defaultWidth);
		Add(column);
		return column;
	}

	public bool GetColumnIndex(int columnIndex, out int arrIndex)
	{
		if (base.Count == 0)
		{
			arrIndex = 0;
			return false;
		}
		int num = 0;
		int num2 = base.Count - 1;
		int num3 = 0;
		Column column = null;
		while (num <= num2)
		{
			num3 = (num + num2) / 2;
			column = base[num3];
			if (column.Index == columnIndex)
			{
				arrIndex = num3;
				return true;
			}
			if (column.Index < columnIndex)
			{
				num = num3 + 1;
			}
			else
			{
				num2 = num3 - 1;
			}
		}
		if (column.Index < columnIndex)
		{
			arrIndex = num3 + 1;
		}
		else
		{
			arrIndex = num3;
		}
		return false;
	}
}
