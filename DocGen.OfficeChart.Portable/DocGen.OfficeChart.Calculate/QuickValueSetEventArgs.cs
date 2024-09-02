using System;

namespace DocGen.OfficeChart.Calculate;

internal class QuickValueSetEventArgs : EventArgs
{
	private FormulaInfoSetAction action;

	private string id;

	private string val;

	internal FormulaInfoSetAction Action
	{
		get
		{
			return action;
		}
		set
		{
			action = value;
		}
	}

	public string Key
	{
		get
		{
			return id;
		}
		set
		{
			id = value;
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

	internal QuickValueSetEventArgs(string key, string value, FormulaInfoSetAction action)
	{
		id = key;
		val = value;
		this.action = action;
	}
}
