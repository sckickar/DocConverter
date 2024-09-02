using System;
using System.ComponentModel;

namespace DocGen.Chart;

internal class ChartSeriesChangedEventArgs : EventArgs
{
	internal enum Type
	{
		Reset,
		Inserted,
		Removed,
		Changed
	}

	private Type m_type;

	public Type EventType => m_type;

	internal ChartSeriesChangedEventArgs(Type type)
	{
		m_type = type;
	}

	public static ChartSeriesChangedEventArgs FromListChangedEventArgs(ListChangedEventArgs args, IChartSeriesModel chartData)
	{
		return args.ListChangedType switch
		{
			ListChangedType.ItemAdded => CreateInsertEventArgs(), 
			ListChangedType.ItemChanged => CreateChangedEventArgs(), 
			ListChangedType.ItemDeleted => CreateRemovedEventArgs(), 
			ListChangedType.Reset => CreateResetEventArgs(), 
			_ => null, 
		};
	}

	public override string ToString()
	{
		return $"Type: {m_type}, Index: {-1}";
	}

	internal static ChartSeriesChangedEventArgs CreateResetEventArgs()
	{
		return new ChartSeriesChangedEventArgs(Type.Reset);
	}

	internal static ChartSeriesChangedEventArgs CreateInsertEventArgs()
	{
		return new ChartSeriesChangedEventArgs(Type.Inserted);
	}

	internal static ChartSeriesChangedEventArgs CreateRemovedEventArgs()
	{
		return new ChartSeriesChangedEventArgs(Type.Removed);
	}

	internal static ChartSeriesChangedEventArgs CreateChangedEventArgs()
	{
		return new ChartSeriesChangedEventArgs(Type.Changed);
	}
}
