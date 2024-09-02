using System;
using System.Collections;
using System.ComponentModel;

namespace DocGen.Chart;

internal class ChartPointIndexer : IList, ICollection, IEnumerable
{
	private class ChartPointEnumerator : IEnumerator
	{
		private int m_currIndex = -1;

		private IChartSeriesModel m_seriesModel;

		private IChartSeriesCategory m_seriesCategoryModel;

		public object Current => new ChartPoint(m_seriesModel, m_currIndex, m_seriesCategoryModel);

		public ChartPointEnumerator(IChartSeriesModel chartSeriesModel)
		{
			m_seriesModel = chartSeriesModel;
		}

		public bool MoveNext()
		{
			m_currIndex++;
			return m_currIndex < m_seriesModel.Count;
		}

		public void Reset()
		{
			m_currIndex = -1;
		}
	}

	private IChartSeriesModel m_model;

	private IEditableChartSeriesModel m_editableModel;

	private IChartEditableCategory m_editableCategoryModel;

	private IChartSeriesCategory m_categoryModel;

	internal bool isCategory;

	internal IChartSeriesModel SeriesModel
	{
		get
		{
			return m_model;
		}
		set
		{
			m_model = value;
			m_editableModel = value as IEditableChartSeriesModel;
		}
	}

	internal IChartSeriesCategory SeriesCategoryModel
	{
		get
		{
			return m_categoryModel;
		}
		set
		{
			m_categoryModel = value;
			m_editableCategoryModel = value as IChartEditableCategory;
		}
	}

	public ChartPoint this[int xIndex] => new ChartPoint(m_model, xIndex, m_categoryModel);

	public int Count => m_model.Count;

	public bool IsReadOnly => m_editableModel == null;

	bool IList.IsFixedSize => false;

	object IList.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
		}
	}

	bool ICollection.IsSynchronized
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	object ICollection.SyncRoot
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public ChartPointIndexer(ChartSeries series)
	{
		SeriesModel = series.SeriesModelAdapter;
	}

	public ChartPointIndexer(IChartSeriesModel model, IChartSeriesCategory categoryModel)
	{
		SeriesModel = model;
		SeriesCategoryModel = categoryModel;
	}

	private int Add(double x, double[] yValues, bool isEmpty)
	{
		if (m_editableModel != null)
		{
			int count = m_editableModel.Count;
			m_editableModel.Add(x, yValues, isEmpty);
			return count;
		}
		return -1;
	}

	public int Add(double x, params double[] yValues)
	{
		if (m_editableModel != null)
		{
			int count = m_editableModel.Count;
			m_editableModel.Add(x, yValues, isEmpty: false);
			return count;
		}
		return -1;
	}

	public int Add(double x, double y)
	{
		return Add(x, new double[1] { y });
	}

	public int Add(string x, double y)
	{
		return Add(x, new double[1] { y });
	}

	public int Add(string x, params double[] yValues)
	{
		if (m_editableModel != null)
		{
			isCategory = true;
			int count = m_editableModel.Count;
			m_editableModel.Add(count, yValues, isEmpty: false, x);
			return count;
		}
		return -1;
	}

	public int Add(double x, params DateTime[] dates)
	{
		double[] array = new double[dates.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = dates[i].ToOADate();
		}
		return Add(x, array);
	}

	public int Add(double x, DateTime date)
	{
		return Add(x, date.ToOADate());
	}

	public int Add(ChartPoint cp)
	{
		return Add(cp.X, cp.YValues, cp.IsEmpty);
	}

	public int Add(DateTime date, params double[] yValues)
	{
		return Add(date.ToOADate(), yValues);
	}

	public int Add(DateTime date, double y)
	{
		return Add(date.ToOADate(), y);
	}

	public void Clear()
	{
		isCategory = false;
		if (m_editableModel != null)
		{
			m_editableModel.Clear();
		}
	}

	public void Insert(int xIndex, ChartPoint cp)
	{
		if (m_editableModel != null)
		{
			m_editableModel.Insert(xIndex, cp.X, cp.YValues);
		}
	}

	public void Remove(ChartPoint cp)
	{
		RemoveAt(IndexOf(cp));
	}

	[Obsolete("Use RemoveAt method.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void Remove(int xIndex)
	{
		RemoveAt(xIndex);
	}

	public void RemoveAt(int index)
	{
		if (m_editableModel != null)
		{
			m_editableModel.Remove(index);
		}
	}

	public int IndexOf(ChartPoint cp)
	{
		int result = -1;
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			if (m_model.GetX(i) != cp.X)
			{
				continue;
			}
			double[] y = m_model.GetY(i);
			double[] yValues = cp.YValues;
			if (y[0].Equals(yValues[0]))
			{
				result = i;
				break;
			}
			if (y == null || yValues == null || y.Length != yValues.Length)
			{
				continue;
			}
			bool flag = true;
			for (int j = 0; j < y.Length; j++)
			{
				if (y[j] == yValues[j])
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	public IEnumerator GetEnumerator()
	{
		return new ChartPointEnumerator(m_model);
	}

	int IList.Add(object value)
	{
		Add(value as ChartPoint);
		return Count - 1;
	}

	bool IList.Contains(object value)
	{
		return IndexOf(value as ChartPoint) > -1;
	}

	int IList.IndexOf(object value)
	{
		return IndexOf(value as ChartPoint);
	}

	void IList.Insert(int index, object value)
	{
		Insert(index, value as ChartPoint);
	}

	void IList.Remove(object value)
	{
		Remove(value as ChartPoint);
	}

	void ICollection.CopyTo(Array array, int index)
	{
		throw new Exception("The method or operation is not implemented.");
	}
}
