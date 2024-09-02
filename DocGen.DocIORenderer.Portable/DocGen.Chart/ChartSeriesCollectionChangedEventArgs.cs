using System;

namespace DocGen.Chart;

internal class ChartSeriesCollectionChangedEventArgs : EventArgs
{
	private readonly ChartSeriesCollectionChangeType m_changeType;

	private readonly ChartSeries m_series;

	public ChartSeriesCollectionChangeType ChangeType => m_changeType;

	public ChartSeries Series => m_series;

	public ChartSeriesCollectionChangedEventArgs(ChartSeriesCollectionChangeType changeType)
	{
		m_changeType = changeType;
	}

	public ChartSeriesCollectionChangedEventArgs(ChartSeriesCollectionChangeType changeType, ChartSeries series)
	{
		m_changeType = changeType;
		m_series = series;
	}

	public static ChartSeriesCollectionChangedEventArgs CreateAddedEvent(ChartSeries series)
	{
		return new ChartSeriesCollectionChangedEventArgs(ChartSeriesCollectionChangeType.Added, series);
	}

	public static ChartSeriesCollectionChangedEventArgs CreateChangedEvent(ChartSeries series)
	{
		return new ChartSeriesCollectionChangedEventArgs(ChartSeriesCollectionChangeType.Changed);
	}

	public static ChartSeriesCollectionChangedEventArgs CreateRemovedEvent(ChartSeries series)
	{
		return new ChartSeriesCollectionChangedEventArgs(ChartSeriesCollectionChangeType.Removed, series);
	}

	public static ChartSeriesCollectionChangedEventArgs CreateResetEvent()
	{
		return new ChartSeriesCollectionChangedEventArgs(ChartSeriesCollectionChangeType.Reset);
	}
}
