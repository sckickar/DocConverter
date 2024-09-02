using System;
using DocGen.Drawing;

namespace DocGen.Chart;

internal class ChartLegendMinSizeEventArgs : EventArgs
{
	private bool handled;

	private Size size;

	public bool Handled
	{
		get
		{
			return handled;
		}
		set
		{
			handled = value;
		}
	}

	public Size Size
	{
		get
		{
			return size;
		}
		set
		{
			size = value;
		}
	}

	public ChartLegendMinSizeEventArgs(Size size)
	{
		this.size = size;
	}
}
