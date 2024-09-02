using System;
using System.Collections;
using System.ComponentModel;
using DocGen.Chart.Renderers;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

[TypeConverter(typeof(CollectionConverter))]
internal sealed class ChartSeriesCollection : CollectionBase
{
	private const string c_seriesNameFormat = "Series{0}";

	private ArrayList m_visibleList = new ArrayList();

	private ChartPlace[] t_chartPlace;

	private ChartModel m_chartModel;

	private bool m_shouldSort;

	private bool m_sorted;

	private int m_updateCount;

	private bool m_needUpdateVisibleList;

	private bool m_disableStyles;

	public bool ShouldSort
	{
		get
		{
			return m_shouldSort;
		}
		set
		{
			m_shouldSort = value;
			m_sorted = false;
		}
	}

	public bool DisableStyles
	{
		get
		{
			return m_disableStyles;
		}
		set
		{
			m_disableStyles = value;
		}
	}

	public bool Sorted => m_sorted;

	public ChartSeries this[int index] => base.List[index] as ChartSeries;

	public ChartSeries this[string name]
	{
		get
		{
			foreach (ChartSeries item in base.List)
			{
				if (item.Name == name)
				{
					return item;
				}
			}
			return null;
		}
	}

	public int VisibleCount => VisibleList.Count;

	internal IList VisibleList
	{
		get
		{
			if (m_needUpdateVisibleList)
			{
				RefreshVisibleList();
				m_needUpdateVisibleList = false;
			}
			return m_visibleList;
		}
	}

	private bool ShouldUpdate => m_updateCount == 0;

	public event ChartSeriesCollectionChangedEventHandler Changed;

	public ChartSeriesCollection(ChartModel chartModel)
	{
		m_chartModel = chartModel;
	}

	public void BeginUpdate()
	{
		m_updateCount++;
	}

	public void EndUpdate()
	{
		m_updateCount--;
		if (m_updateCount == 0)
		{
			RaiseResetEvent();
		}
	}

	public void Add(ChartSeries series)
	{
		if (!base.List.Contains(series))
		{
			base.List.Add(series);
		}
	}

	public int IndexOf(ChartSeries series)
	{
		return base.List.IndexOf(series);
	}

	public bool Contains(ChartSeries series)
	{
		return base.List.Contains(series);
	}

	public void Insert(int index, ChartSeries series)
	{
		if (!base.List.Contains(series))
		{
			base.List.Insert(index, series);
		}
	}

	public void Remove(ChartSeries series)
	{
		int num = base.List.IndexOf(series);
		if (num >= 0)
		{
			base.List.RemoveAt(num);
		}
	}

	public void ResetCache()
	{
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				ChartSeries series = (ChartSeries)enumerator.Current;
				ResetCache(series);
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

	public void ResetCache(ChartSeries series)
	{
		if (series.ResetStyles)
		{
			series.StylesImpl.ComposedStyles.ResetCache();
		}
		series.ResetLegend();
		series.UpdateRenderer(ChartUpdateFlags.All);
	}

	public void Sort(IComparer comparer)
	{
		m_sorted = false;
		if (comparer is ComparerByZandY)
		{
			try
			{
				for (int i = 0; i < base.List.Count - 1; i++)
				{
					for (int j = i + 1; j < base.List.Count; j++)
					{
						if (0 > comparer.Compare(base.List[j], base.List[i]))
						{
							object obj = base.List[j];
							object value = base.List[i];
							((ChartSeries)base.List[i]).ZOrder = j;
							base.List[j] = base.List[i];
							((ChartSeries)obj).ZOrder = i;
							base.List[i] = obj;
							base.List[j] = value;
						}
					}
				}
			}
			catch (NotSupportedException)
			{
			}
			m_sorted = true;
			return;
		}
		throw new NotSupportedException("Only ComparerByZandY is supported.");
	}

	protected override void OnClear()
	{
		for (int num = base.List.Count - 1; num >= 0; num--)
		{
			ClearSeries(base.List[num] as ChartSeries);
		}
	}

	protected override void OnClearComplete()
	{
		RaiseCollectionChangedEventHandler(ChartSeriesCollectionChangedEventArgs.CreateResetEvent());
	}

	protected override void OnInsert(int index, object value)
	{
		ChartSeries chartSeries = value as ChartSeries;
		if (chartSeries.ZOrder == -1)
		{
			chartSeries.ZOrder = ((base.Count > 0) ? (this[base.Count - 1].ZOrder + 1) : 0);
		}
		base.OnInsert(index, value);
	}

	protected override void OnInsertComplete(int index, object value)
	{
		m_sorted = false;
		ChartSeries chartSeries = value as ChartSeries;
		WireSeries(chartSeries);
		RaiseCollectionChangedEventHandler(ChartSeriesCollectionChangedEventArgs.CreateAddedEvent(chartSeries));
		if (chartSeries.Points.isCategory)
		{
			ChartAxis actualXAxis = chartSeries.ActualXAxis;
			if (actualXAxis != null && actualXAxis.ValueType != ChartValueType.Category)
			{
				actualXAxis.ValueType = ChartValueType.Category;
			}
		}
	}

	protected override void OnRemoveComplete(int index, object value)
	{
		ResetCache();
		UnwireSeries(value as ChartSeries);
		RaiseCollectionChangedEventHandler(ChartSeriesCollectionChangedEventArgs.CreateRemovedEvent(value as ChartSeries));
	}

	protected override void OnSetComplete(int index, object newValue, object oldValue)
	{
		m_sorted = false;
		UnwireSeries(oldValue as ChartSeries);
		RaiseCollectionChangedEventHandler(ChartSeriesCollectionChangedEventArgs.CreateRemovedEvent(oldValue as ChartSeries));
		WireSeries(newValue as ChartSeries);
		RaiseCollectionChangedEventHandler(ChartSeriesCollectionChangedEventArgs.CreateAddedEvent(newValue as ChartSeries));
	}

	protected override void OnValidate(object value)
	{
		if (!(value is ChartSeries))
		{
			throw new ArgumentException("value should be ChartSeries");
		}
	}

	internal RectangleF GetSeriesClipRectangle(ChartSeries series, RectangleF areaBounds)
	{
		if (series.BaseType == ChartSeriesBaseType.SideBySide || series.BaseType == ChartSeriesBaseType.Independent || series.BaseType == ChartSeriesBaseType.Other)
		{
			return new RectangleF(series.XAxis.Rect.X, series.YAxis.Rect.Y, series.XAxis.Rect.Width, series.YAxis.Rect.Height);
		}
		return areaBounds;
	}

	internal void DrawSeries(Graphics g, IChartAreaHost chart)
	{
		if (!Sorted && ShouldSort)
		{
			m_visibleList.Sort(new ComparerByZandY());
		}
		m_needUpdateVisibleList = true;
		IChartArea chartArea = chart.GetChartArea();
		RectangleF rectangleF = chartArea.RenderBounds;
		foreach (ChartSeries item in base.List)
		{
			ChartAxis yAxis = chartArea.GetYAxis(item);
			ChartAxis xAxis = chartArea.GetXAxis(item);
			item.Renderer.SetBoundsAndRange(chart, rectangleF, xAxis, yAxis);
		}
		RenderViewer renderViewer = new RenderViewer();
		ChartPlace[] array = ChartPlace.CalculateSpace(this, chartArea.DivideArea, chart);
		renderViewer.AxisInverted = chart.RequireInvertedAxes;
		RectangleF? outerRect = null;
		for (int num = array.Length - 1; num >= 0; num--)
		{
			ChartPlace chartPlace = array[num];
			for (int num2 = chartPlace.Series.Count - 1; num2 > -1; num2--)
			{
				ChartSeries chartSeries2 = chartPlace.Series[num2] as ChartSeries;
				if (chartSeries2.Compatible)
				{
					RectangleF bounds = rectangleF;
					ChartAxis yAxis2 = chartArea.GetYAxis(chartSeries2);
					ChartAxis xAxis2 = chartArea.GetXAxis(chartSeries2);
					if (chart.GetChartArea().DivideArea && chartSeries2.BaseType == ChartSeriesBaseType.Single)
					{
						bounds = RenderingHelper.GetBounds(VisibleList.IndexOf(chartSeries2), chartPlace.Series.Count, rectangleF);
					}
					chartSeries2.Renderer.SetBoundsAndRange(chart, bounds, xAxis2, yAxis2);
					GraphicsState state = g.Save();
					if (!m_chartModel.Chart.Series3D && chartSeries2.BaseType != 0)
					{
						g.SetClip(GetSeriesClipRectangle(chartSeries2, rectangleF));
					}
					PieRenderer pieRenderer = ((chartSeries2.Renderer is PieRenderer) ? (chartSeries2.Renderer as PieRenderer) : null);
					if (pieRenderer != null)
					{
						pieRenderer.OuterRect = outerRect;
					}
					chartSeries2.Renderer.Render(g);
					if (pieRenderer != null && pieRenderer.OuterRect.HasValue)
					{
						outerRect = pieRenderer.OuterRect;
					}
					g.Restore(state);
					chartSeries2.Renderer.RenderSeriesNameInDepth(g);
					renderViewer.Add(chartSeries2.Renderer);
				}
			}
			renderViewer.View(g);
			renderViewer.Clear();
		}
		for (int num3 = array.Length - 1; num3 >= 0; num3--)
		{
			foreach (ChartSeries item2 in array[num3].Series)
			{
				if (item2.Compatible)
				{
					GraphicsState state2 = g.Save();
					if (!m_chartModel.Chart.Series3D || item2.BaseType != 0)
					{
						g.SetClip(GetSeriesClipRectangle(item2, rectangleF));
					}
					item2.Renderer.RenderAdornments(g);
					g.Restore(state2);
				}
			}
		}
	}

	internal void DrawSeries(Graphics3D g, IChartAreaHost chart)
	{
		try
		{
			if (t_chartPlace == null)
			{
				t_chartPlace = SetClip(g, chart);
			}
			for (int num = t_chartPlace.Length - 1; num >= 0; num--)
			{
				ChartPlace chartPlace = t_chartPlace[num];
				for (int i = 0; i < chartPlace.Series.Count; i++)
				{
					ChartSeries chartSeries = chartPlace.Series[i] as ChartSeries;
					if (chartSeries.Compatible)
					{
						chartSeries.Renderer.Render(g);
						chartSeries.Renderer.RenderAdornments(g);
					}
				}
			}
		}
		finally
		{
			t_chartPlace = null;
		}
	}

	internal void DrawSeriesNamesInDepth(Graphics3D g, IChartAreaHost chart)
	{
		t_chartPlace = SetClip(g, chart);
		for (int num = t_chartPlace.Length - 1; num >= 0; num--)
		{
			ChartPlace chartPlace = t_chartPlace[num];
			for (int i = 0; i < chartPlace.Series.Count; i++)
			{
				ChartSeries chartSeries = chartPlace.Series[i] as ChartSeries;
				if (chartSeries.Compatible)
				{
					chartSeries.Renderer.RenderSeriesNameInDepth(g);
				}
			}
		}
	}

	internal ChartPlace[] SetClip(Graphics3D g, IChartAreaHost chart)
	{
		if (!Sorted && ShouldSort)
		{
			Sort(new ComparerByZandY());
		}
		ChartPlace[] array = ChartPlace.CalculateSpace(this, chart.GetChartArea().DivideArea, chart);
		for (int num = array.Length - 1; num >= 0; num--)
		{
			ChartPlace chartPlace = array[num];
			for (int i = 0; i < chartPlace.Series.Count; i++)
			{
				ChartSeries chartSeries = chartPlace.Series[i] as ChartSeries;
				if (chartSeries.Compatible)
				{
					RectangleF bounds = chart.GetChartArea().RenderBounds;
					ChartAxis yAxis = chart.GetChartArea().GetYAxis(chartSeries);
					ChartAxis xAxis = chart.GetChartArea().GetXAxis(chartSeries);
					if (chart.GetChartArea().DivideArea && chartSeries.BaseType == ChartSeriesBaseType.Single)
					{
						bounds = RenderingHelper.GetBounds(VisibleList.IndexOf(chartSeries), chartPlace.Series.Count, bounds);
					}
					chartSeries.Renderer.SetBoundsAndRange(chart, bounds, xAxis, yAxis);
				}
			}
		}
		return array;
	}

	private void RaiseCollectionChangedEventHandler(ChartSeriesCollectionChangedEventArgs e)
	{
		m_needUpdateVisibleList = true;
		if (ShouldUpdate && this.Changed != null)
		{
			this.Changed(this, e);
		}
	}

	private void OnSeriesChanged(object sender, EventArgs args)
	{
		m_needUpdateVisibleList = true;
		RaiseCollectionChangedEventHandler(ChartSeriesCollectionChangedEventArgs.CreateChangedEvent(sender as ChartSeries));
	}

	private void OnSeriesDataChanged(object sender, ListChangedEventArgs args)
	{
		if (args.ListChangedType != ListChangedType.ItemAdded)
		{
			ResetCache(sender as ChartSeries);
		}
		RaiseCollectionChangedEventHandler(ChartSeriesCollectionChangedEventArgs.CreateChangedEvent(sender as ChartSeries));
	}

	internal void RaiseResetEvent()
	{
		RaiseCollectionChangedEventHandler(ChartSeriesCollectionChangedEventArgs.CreateResetEvent());
	}

	private void ClearSeries(ChartSeries series)
	{
		series.RemoveChartModel();
		series.DataChanged -= OnSeriesDataChanged;
		series.SeriesChanged -= OnSeriesChanged;
	}

	private void UnwireSeries(ChartSeries series)
	{
		series.ChartModel = null;
		series.DataChanged -= OnSeriesDataChanged;
		series.SeriesChanged -= OnSeriesChanged;
	}

	private void WireSeries(ChartSeries series)
	{
		series.ChartModel = m_chartModel;
		series.DataChanged += OnSeriesDataChanged;
		series.SeriesChanged += OnSeriesChanged;
		ResetCache(series);
	}

	public ChartSeries GetSeriesByVisible(int index)
	{
		return VisibleList[index] as ChartSeries;
	}

	private void RefreshVisibleList()
	{
		m_visibleList.Clear();
		for (int i = 0; i < base.Count; i++)
		{
			if (this[i].Visible && this[i].Compatible)
			{
				m_visibleList.Add(this[i]);
			}
		}
		if (!m_shouldSort)
		{
			m_visibleList.Sort(new ChartSeriesComparerByZOrder());
		}
	}

	internal bool All(Func<ChartSeries, bool> func)
	{
		foreach (ChartSeries item in base.List)
		{
			if (!func(item))
			{
				return false;
			}
		}
		return true;
	}
}
