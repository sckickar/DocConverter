using System;

namespace DocGen.Chart;

internal class ChartStyleChangedEventArgs : EventArgs
{
	internal enum Type
	{
		Changed,
		Reset
	}

	public const int InvalidIndex = -1;

	private Type type;

	private int xIndex;

	public Type EventType => type;

	public int Index => xIndex;

	public static ChartStyleChangedEventArgs CreateResetEventArgs()
	{
		return new ChartStyleChangedEventArgs(Type.Reset, -1);
	}

	public ChartStyleChangedEventArgs(Type type, int xIndex)
	{
		this.type = type;
		this.xIndex = xIndex;
	}
}
