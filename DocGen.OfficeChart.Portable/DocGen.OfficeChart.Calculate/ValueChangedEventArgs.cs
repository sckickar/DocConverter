using System;

namespace DocGen.OfficeChart.Calculate;

internal class ValueChangedEventArgs : EventArgs
{
	private int col;

	private int row;

	private string val;

	public int ColIndex
	{
		get
		{
			return col;
		}
		set
		{
			col = value;
		}
	}

	public int RowIndex
	{
		get
		{
			return row;
		}
		set
		{
			row = value;
		}
	}

	public string Value
	{
		get
		{
			return val;
		}
		set
		{
			val = value;
		}
	}

	public ValueChangedEventArgs(int row, int col, string value)
	{
		this.row = row;
		this.col = col;
		val = value;
	}
}
