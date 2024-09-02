using System;
using System.Collections;

namespace DocGen.Chart;

internal sealed class ChartIndexedValues
{
	private ChartModel m_chartModel;

	private ArrayList m_indexedValues = new ArrayList();

	private bool m_needUpdate = true;

	public int Count
	{
		get
		{
			EnsureValuesUpdated();
			return m_indexedValues.Count;
		}
	}

	public double this[int index]
	{
		get
		{
			EnsureValuesUpdated();
			return (double)m_indexedValues[index];
		}
	}

	internal ChartIndexedValues(ChartModel model)
	{
		if (model == null)
		{
			throw new ArgumentNullException("model");
		}
		m_chartModel = model;
		m_chartModel.Series.Changed += OnSeriesChanged;
	}

	public double GetIndex(double value)
	{
		EnsureValuesUpdated();
		double num = m_indexedValues.IndexOf(value);
		if (m_indexedValues.Count > 0 && num < 0.0)
		{
			int index = -1;
			int index2 = -1;
			double[] obj = m_indexedValues.ToArray(typeof(double)) as double[];
			ChartMath.GetTwoClosestPoints(obj, value, out index, out index2);
			double num2 = obj[index];
			double num3 = obj[index2];
			if (num3 == num2)
			{
				num3 += 1.0;
			}
			num = (double)index + (value - num2) / (num3 - num2);
		}
		return num;
	}

	public double GetValue(double index)
	{
		EnsureValuesUpdated();
		double result = -1.0;
		int count = m_indexedValues.Count;
		if (count > 0)
		{
			if (index % 1.0 == 0.0 && index > -1.0 && index < (double)m_indexedValues.Count)
			{
				result = (double)m_indexedValues[(int)index];
			}
			else
			{
				int num = (int)ChartMath.MinMax(ChartMath.Round(index, 1.0, up: false), 0.0, count - 1);
				int num2 = (int)ChartMath.MinMax(ChartMath.Round(index, 1.0, up: true), 0.0, count - 1);
				if (num == num2 && count > 1)
				{
					if (num == 0)
					{
						num2++;
					}
					else
					{
						num--;
					}
				}
				double num3 = (double)m_indexedValues[num];
				double num4 = (double)m_indexedValues[num2];
				if (num4 == num3)
				{
					num4 += 1.0;
				}
				result = num3 + (num4 - num3) * (index - (double)num);
			}
		}
		return result;
	}

	private void EnsureValuesUpdated()
	{
		if (!m_needUpdate)
		{
			return;
		}
		m_indexedValues.Clear();
		if (m_chartModel != null)
		{
			foreach (ChartSeries item in m_chartModel.Series)
			{
				foreach (ChartPoint point in item.Points)
				{
					if (m_chartModel.Chart.AllowGapForEmptyPoints)
					{
						if (!m_indexedValues.Contains(point.X))
						{
							m_indexedValues.Add(point.X);
						}
					}
					else if (!m_indexedValues.Contains(point.X) && point.IsVisible())
					{
						m_indexedValues.Add(point.X);
					}
				}
			}
		}
		m_indexedValues.Sort();
		m_needUpdate = false;
	}

	private void OnSeriesChanged(object sender, ChartSeriesCollectionChangedEventArgs e)
	{
		m_needUpdate = true;
	}
}
