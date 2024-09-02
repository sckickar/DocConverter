using System.ComponentModel;

namespace DocGen.Chart;

internal class ChartSeriesModelAdapter : IChartSeriesModel, IEditableChartSeriesModel, IChartSeriesCategory, IChartEditableCategory
{
	private ChartSeries m_series;

	public virtual int Count => m_series.SeriesModel.Count;

	public event ListChangedEventHandler Changed
	{
		add
		{
		}
		remove
		{
		}
	}

	public ChartSeriesModelAdapter(ChartSeries series)
	{
		m_series = series;
	}

	public virtual double GetX(int xIndex)
	{
		return m_series.SeriesModel.GetX(xIndex);
	}

	public virtual double[] GetY(int xIndex)
	{
		return m_series.SeriesModel.GetY(xIndex);
	}

	public virtual bool GetEmpty(int xIndex)
	{
		return m_series.SeriesModel.GetEmpty(xIndex);
	}

	public virtual string GetCategory(int xIndex)
	{
		return m_series.CategoryModel.GetCategory(xIndex);
	}

	public void Add(double x, double[] yValues)
	{
		if (m_series.IsEditableData())
		{
			m_series.GetEditableData().Add(x, yValues);
		}
	}

	public void Add(double x, double[] yValues, string category)
	{
		if (m_series.IsEditableData())
		{
			m_series.GetEditableData().Add(x, yValues, category);
		}
	}

	public void Add(double x, double[] yValues, bool isEmpty)
	{
		if (m_series.IsEditableData())
		{
			m_series.GetEditableData().Add(x, yValues, isEmpty);
		}
	}

	public void Add(double x, double[] yValues, bool isEmpty, string category)
	{
		if (m_series.IsEditableData())
		{
			m_series.GetEditableData().Add(x, yValues, isEmpty, category);
		}
	}

	public void Insert(int xIndex, double x, double[] yValues)
	{
		if (m_series.IsEditableData())
		{
			m_series.GetEditableData().Insert(xIndex, x, yValues);
		}
	}

	public void Insert(int xIndex, double x, double[] yValues, string category)
	{
		if (m_series.IsEditableData())
		{
			m_series.GetEditableData().Insert(xIndex, x, yValues, category);
		}
	}

	public void SetX(int xIndex, double value)
	{
		if (m_series.IsEditableData())
		{
			m_series.GetEditableData().SetX(xIndex, value);
		}
	}

	public void SetY(int xIndex, double[] yValues)
	{
		if (m_series.IsEditableData())
		{
			m_series.GetEditableData().SetY(xIndex, yValues);
		}
	}

	public void SetCategory(int xIndex, string category)
	{
		if (m_series.IsEditableData())
		{
			m_series.GetCategoryEditableData().SetCategory(xIndex, category);
		}
	}

	public void SetEmpty(int xIndex, bool isEmpty)
	{
		if (m_series.IsEditableData())
		{
			m_series.GetEditableData().SetEmpty(xIndex, isEmpty);
		}
	}

	public void Remove(int xIndex)
	{
		if (m_series.IsEditableData())
		{
			m_series.GetEditableData().Remove(xIndex);
		}
	}

	public void Clear()
	{
		if (m_series.IsEditableData())
		{
			m_series.GetEditableData().Clear();
		}
	}

	public void Add(string category)
	{
	}
}
