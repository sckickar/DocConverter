using System.ComponentModel;

namespace DocGen.Chart;

internal class ChartSeriesIndexedModelAdapter : IChartSeriesModel
{
	private IChartSeriesIndexedModel m_indexedModel;

	public IChartSeriesIndexedModel Inner => m_indexedModel;

	public int Count => m_indexedModel.Count;

	public event ListChangedEventHandler Changed
	{
		add
		{
			m_indexedModel.Changed += value;
		}
		remove
		{
			m_indexedModel.Changed -= value;
		}
	}

	public ChartSeriesIndexedModelAdapter(IChartSeriesIndexedModel model)
	{
		m_indexedModel = model;
	}

	public double GetX(int xIndex)
	{
		return xIndex;
	}

	public double[] GetY(int xIndex)
	{
		return m_indexedModel.GetY(xIndex);
	}

	public bool GetEmpty(int xIndex)
	{
		return m_indexedModel.GetEmpty(xIndex);
	}
}
