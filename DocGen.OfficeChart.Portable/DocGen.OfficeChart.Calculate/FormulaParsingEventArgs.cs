using System;

namespace DocGen.OfficeChart.Calculate;

internal class FormulaParsingEventArgs : EventArgs
{
	private string text;

	public string Text
	{
		get
		{
			return text;
		}
		set
		{
			text = value;
		}
	}

	public FormulaParsingEventArgs(string text)
	{
		this.text = text;
	}

	public FormulaParsingEventArgs()
	{
	}
}
