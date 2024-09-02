using System.Collections;

namespace DocGen.Chart;

internal class ChartPlace
{
	private ArrayList m_series = new ArrayList();

	private ChartSeriesType m_seriesType = ChartSeriesType.Custom;

	private int m_placeIndex = -1;

	public ChartSeriesType SeriesType => m_seriesType;

	public int PlaceIndex => m_placeIndex;

	public IList Series => m_series;

	public ChartPlace(int index, ChartSeries series)
	{
		m_placeIndex = index;
		m_series.Add(series);
		m_seriesType = series.Type;
		series.Renderer.Place = index;
	}

	public void Add(ChartSeries series)
	{
		series.Renderer.Place = m_placeIndex;
		m_series.Add(series);
	}

	public void SetSpaceSize(int size)
	{
		foreach (ChartSeries item in m_series)
		{
			item.Renderer.PlaceSize = size;
		}
	}

	public static ChartPlace[] CalculateSpace(ChartSeriesCollection series, bool doManyAreas, IChartAreaHost chart)
	{
		int num = 0;
		ChartPlace chartPlace = null;
		ArrayList arrayList = new ArrayList();
		for (int i = 0; i < series.VisibleCount; i++)
		{
			ChartSeries chartSeries = series.VisibleList[i] as ChartSeries;
			if (series.DisableStyles)
			{
				chartSeries.EnableStyles = !series.DisableStyles;
			}
			chartSeries.Renderer.SetChart(chart);
			if (!chartSeries.Renderer.CanRender())
			{
				continue;
			}
			if (chartSeries.Renderer.FillSpaceType == ChartUsedSpaceType.All)
			{
				if (chartPlace == null && i == 0)
				{
					chartPlace = new ChartPlace(0, chartSeries);
					if (!doManyAreas)
					{
						break;
					}
				}
				else if (doManyAreas)
				{
					chartPlace?.Add(chartSeries);
				}
			}
			else if (chartSeries.Renderer.FillSpaceType == ChartUsedSpaceType.OneForAll)
			{
				bool flag = true;
				foreach (ChartPlace item in arrayList)
				{
					if (item.SeriesType == chartSeries.Type)
					{
						item.Add(chartSeries);
						flag = false;
						break;
					}
				}
				if (flag)
				{
					arrayList.Add(new ChartPlace(num, chartSeries));
					num++;
				}
			}
			else if (chartSeries.Renderer.FillSpaceType == ChartUsedSpaceType.OneForOne)
			{
				arrayList.Add(new ChartPlace(num, chartSeries));
				num++;
			}
		}
		if (chartPlace != null)
		{
			arrayList.Clear();
			arrayList.Add(chartPlace);
			num = 1;
		}
		ChartPlace[] array = new ChartPlace[arrayList.Count];
		for (int j = 0; j < arrayList.Count; j++)
		{
			array[j] = arrayList[j] as ChartPlace;
			array[j].SetSpaceSize(num);
		}
		return array;
	}
}
