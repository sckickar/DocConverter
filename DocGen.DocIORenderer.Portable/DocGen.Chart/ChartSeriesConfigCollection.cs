using System;
using System.Collections;
using DocGen.ComponentModel;

namespace DocGen.Chart;

internal sealed class ChartSeriesConfigCollection : DictionaryBase
{
	public const string BubbleItemName = "BubbleConfigItem";

	public const string PieItemName = "PieConfigItem";

	public const string RadarItemName = "RadarConfigItem";

	public const string StepItemName = "StepConfigItem";

	public const string ColumnItemName = "ColumnConfigItem";

	public const string FunnelItemName = "FunnelConfigItem";

	public const string PyramidItemName = "PyramidConfigItem";

	public const string FinancialItemName = "FinancialConfigItem";

	public const string GanttItemName = "GanttConfigItem";

	public const string HiLoOpenCloseItemName = "HiLoOpenCloseConfigItem";

	public const string HistogramItemName = "HistogramConfigItem";

	public const string ErrorBarsItemName = "ErrorBarsConfigItem";

	public const string HeatMapItemName = "HeatMapConfigItem";

	public const string BoxAndWhiskerItemName = "BoxAndWhiskerConfigItem";

	public const string LineSegmentName = "LineConfigItem";

	public const string RangeAreaName = "RangeAreaConfigItem";

	private ChartColumnConfigItem m_columnItem;

	private ChartPieConfigItem m_pieItem;

	private ChartRadarConfigItem m_radarItem;

	private ChartStepConfigItem m_stepItem;

	private ChartFunnelConfigItem m_funnelItem;

	private ChartPyramidConfigItem m_pyramidItem;

	private ChartFinancialConfigItem m_financialItem;

	private ChartGanttConfigItem m_ganttItem;

	private ChartHiLoOpenCloseConfigItem m_hiLoOpenCloseItem;

	private ChartHistogramConfigItem m_histogramItem;

	private ChartBubbleConfigItem m_bubbleItem;

	private ChartErrorBarsConfigItem m_errorBars;

	private ChartHeatMapConfigItem m_heatMapItem;

	private ChartLineConfigItem m_linesegment;

	private ChartBoxAndWhiskerConfigItem m_BoxAndWhiskerItem;

	private ChartLineConfigItem m_LineItem;

	private ChartRangeAreaConfigItem m_RangeAreaItem;

	public ChartBubbleConfigItem BubbleItem => m_bubbleItem;

	public ChartPieConfigItem PieItem => m_pieItem;

	public ChartRadarConfigItem RadarItem => m_radarItem;

	public ChartStepConfigItem StepItem => m_stepItem;

	public ChartColumnConfigItem ColumnItem => m_columnItem;

	public ChartFunnelConfigItem FunnelItem => m_funnelItem;

	public ChartPyramidConfigItem PyramidItem => m_pyramidItem;

	public ChartFinancialConfigItem FinancialItem => m_financialItem;

	public ChartGanttConfigItem GanttItem => m_ganttItem;

	public ChartHiLoOpenCloseConfigItem HiLoOpenCloseItem => m_hiLoOpenCloseItem;

	public ChartHistogramConfigItem HistogramItem => m_histogramItem;

	public ChartErrorBarsConfigItem ErrorBars => m_errorBars;

	public ChartBoxAndWhiskerConfigItem BoxAndWhiskerItem => m_BoxAndWhiskerItem;

	public ChartHeatMapConfigItem HeatMapItem => m_heatMapItem;

	public ChartLineConfigItem LineSegment => m_linesegment;

	public ChartLineConfigItem LineItem => m_LineItem;

	public ChartRangeAreaConfigItem RangeAreaItem => m_RangeAreaItem;

	public ChartConfigItem this[string name] => base.Dictionary[name] as ChartConfigItem;

	public event EventHandler Changed;

	public ChartSeriesConfigCollection()
	{
		Add("BubbleConfigItem", new ChartBubbleConfigItem());
		Add("PieConfigItem", new ChartPieConfigItem());
		Add("RadarConfigItem", new ChartRadarConfigItem());
		Add("StepConfigItem", new ChartStepConfigItem());
		Add("ColumnConfigItem", new ChartColumnConfigItem());
		Add("FunnelConfigItem", new ChartFunnelConfigItem());
		Add("PyramidConfigItem", new ChartPyramidConfigItem());
		Add("FinancialConfigItem", new ChartFinancialConfigItem());
		Add("GanttConfigItem", new ChartGanttConfigItem());
		Add("HiLoOpenCloseConfigItem", new ChartHiLoOpenCloseConfigItem());
		Add("HistogramConfigItem", new ChartHistogramConfigItem());
		Add("ErrorBarsConfigItem", new ChartErrorBarsConfigItem());
		Add("HeatMapConfigItem", new ChartHeatMapConfigItem());
		Add("BoxAndWhiskerConfigItem", new ChartBoxAndWhiskerConfigItem());
		Add("LineConfigItem", new ChartLineConfigItem());
		Add("RangeAreaConfigItem", new ChartRangeAreaConfigItem());
	}

	public void Add(string name, ChartConfigItem item)
	{
		base.Dictionary.Add(name, item);
	}

	public void Remove(string name)
	{
		base.Dictionary.Remove(name);
	}

	protected override void OnInsertComplete(object key, object value)
	{
		ChangeConfig(key.ToString(), value);
		BroadcastChange();
		WireItem(value);
	}

	protected override void OnRemove(object key, object value)
	{
		UnwireItem(value);
	}

	protected override void OnRemoveComplete(object key, object value)
	{
		BroadcastChange();
	}

	protected override void OnSet(object key, object oldValue, object newValue)
	{
		UnwireItem(oldValue);
	}

	protected override void OnSetComplete(object key, object oldValue, object newValue)
	{
		ChangeConfig(key.ToString(), newValue);
		BroadcastChange();
		WireItem(newValue);
	}

	protected override void OnClear()
	{
		foreach (ChartConfigItem value in base.Dictionary.Values)
		{
			UnwireItem(value);
		}
		m_columnItem = null;
		m_pieItem = null;
		m_radarItem = null;
		m_stepItem = null;
		m_funnelItem = null;
		m_pyramidItem = null;
		m_financialItem = null;
		m_ganttItem = null;
		m_hiLoOpenCloseItem = null;
		m_histogramItem = null;
		m_bubbleItem = null;
		m_LineItem = null;
	}

	protected override void OnClearComplete()
	{
		BroadcastChange();
	}

	private void WireItem(object value)
	{
		(value as ChartConfigItem).PropertyChanged += OnContainedObjectPropertyChanged;
	}

	private void UnwireItem(object value)
	{
		(value as ChartConfigItem).PropertyChanged -= OnContainedObjectPropertyChanged;
	}

	private void OnContainedObjectPropertyChanged(object sender, SyncfusionPropertyChangedEventArgs args)
	{
		BroadcastChange();
	}

	private void BroadcastChange()
	{
		if (this.Changed != null)
		{
			this.Changed(this, EventArgs.Empty);
		}
	}

	private void ChangeConfig(string key, object newValue)
	{
		if (key == null)
		{
			return;
		}
		switch (key.Length)
		{
		case 16:
			switch (key[0])
			{
			case 'B':
				if (key == "BubbleConfigItem")
				{
					m_bubbleItem = newValue as ChartBubbleConfigItem;
				}
				break;
			case 'C':
				if (key == "ColumnConfigItem")
				{
					m_columnItem = newValue as ChartColumnConfigItem;
				}
				break;
			case 'F':
				if (key == "FunnelConfigItem")
				{
					m_funnelItem = newValue as ChartFunnelConfigItem;
				}
				break;
			case 'D':
			case 'E':
				break;
			}
			break;
		case 15:
			switch (key[0])
			{
			case 'R':
				if (key == "RadarConfigItem")
				{
					m_radarItem = newValue as ChartRadarConfigItem;
				}
				break;
			case 'G':
				if (key == "GanttConfigItem")
				{
					m_ganttItem = newValue as ChartGanttConfigItem;
				}
				break;
			}
			break;
		case 14:
			switch (key[0])
			{
			case 'S':
				if (key == "StepConfigItem")
				{
					m_stepItem = newValue as ChartStepConfigItem;
				}
				break;
			case 'L':
				if (key == "LineConfigItem")
				{
					m_linesegment = newValue as ChartLineConfigItem;
					m_LineItem = newValue as ChartLineConfigItem;
				}
				break;
			}
			break;
		case 17:
			switch (key[0])
			{
			case 'P':
				if (key == "PyramidConfigItem")
				{
					m_pyramidItem = newValue as ChartPyramidConfigItem;
				}
				break;
			case 'H':
				if (key == "HeatMapConfigItem")
				{
					m_heatMapItem = newValue as ChartHeatMapConfigItem;
				}
				break;
			}
			break;
		case 19:
			switch (key[0])
			{
			case 'F':
				if (key == "FinancialConfigItem")
				{
					m_financialItem = newValue as ChartFinancialConfigItem;
				}
				break;
			case 'H':
				if (key == "HistogramConfigItem")
				{
					m_histogramItem = newValue as ChartHistogramConfigItem;
				}
				break;
			case 'E':
				if (key == "ErrorBarsConfigItem")
				{
					m_errorBars = newValue as ChartErrorBarsConfigItem;
				}
				break;
			case 'R':
				if (key == "RangeAreaConfigItem")
				{
					m_RangeAreaItem = newValue as ChartRangeAreaConfigItem;
				}
				break;
			}
			break;
		case 23:
			switch (key[0])
			{
			case 'H':
				if (key == "HiLoOpenCloseConfigItem")
				{
					m_hiLoOpenCloseItem = newValue as ChartHiLoOpenCloseConfigItem;
				}
				break;
			case 'B':
				if (key == "BoxAndWhiskerConfigItem")
				{
					m_BoxAndWhiskerItem = newValue as ChartBoxAndWhiskerConfigItem;
				}
				break;
			}
			break;
		case 13:
			if (key == "PieConfigItem")
			{
				m_pieItem = newValue as ChartPieConfigItem;
			}
			break;
		case 18:
		case 20:
		case 21:
		case 22:
			break;
		}
	}
}
