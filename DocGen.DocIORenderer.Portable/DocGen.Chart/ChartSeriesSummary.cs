using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DocGen.Chart;

internal class ChartSeriesSummary : IChartSeriesSummary
{
	private enum Coordinate
	{
		X,
		Y
	}

	private const string c_textFormat = "MinX: {0}, MaxX: {1}, MinY: {2}, MaxY: {3}";

	private bool m_needUpdate;

	private IChartSeriesModel m_model;

	private IChartSeriesCategory m_categoryModel;

	private double m_minX;

	private double m_minY;

	private double m_maxX;

	private double m_maxY;

	private double[] m_sums;

	public double MaxX
	{
		get
		{
			EnsureRefreshed();
			return m_maxX;
		}
	}

	public double MaxY
	{
		get
		{
			EnsureRefreshed();
			return m_maxY;
		}
	}

	public double MinX
	{
		get
		{
			EnsureRefreshed();
			return m_minX;
		}
	}

	public double MinY
	{
		get
		{
			EnsureRefreshed();
			return m_minY;
		}
	}

	public IChartSeriesModel ModelImpl
	{
		get
		{
			return m_model;
		}
		set
		{
			if (m_model != value)
			{
				if (m_model != null)
				{
					m_model.Changed -= OnModelChanged;
				}
				m_model = value;
				if (m_model != null)
				{
					m_model.Changed += OnModelChanged;
				}
				m_needUpdate = true;
			}
		}
	}

	public IChartSeriesCategory CategoryModel
	{
		get
		{
			return m_categoryModel;
		}
		set
		{
			if (m_categoryModel != value)
			{
				if (m_categoryModel != null)
				{
					m_categoryModel.Changed -= OnModelChanged;
				}
				m_categoryModel = value;
				if (m_categoryModel != null)
				{
					m_categoryModel.Changed += OnModelChanged;
				}
				m_needUpdate = true;
			}
		}
	}

	public double GetYPercentage(int pointIndex)
	{
		return GetYPercentage(pointIndex, 0);
	}

	public double GetYPercentage(int pointIndex, int yIndex)
	{
		EnsureRefreshed();
		if (m_model != null)
		{
			return 100.0 * Math.Max(0.0, m_model.GetY(pointIndex)[yIndex]) / m_sums[yIndex];
		}
		return 0.0;
	}

	public ChartPoint FindValue(double value)
	{
		int num = FindYValue(value, 0, 0, m_model.Count - 1);
		if (num > -1)
		{
			return new ChartPoint(m_model, num, m_categoryModel);
		}
		return null;
	}

	public ChartPoint FindValue(double value, string useValue)
	{
		int index = 0;
		return FindValue(value, useValue, ref index);
	}

	public ChartPoint FindValue(double value, string useValue, ref int index)
	{
		return FindValue(value, useValue, ref index, m_model.Count - 1);
	}

	public ChartPoint FindValue(double value, string useValue, ref int index, int endIndex)
	{
		if (endIndex < index)
		{
			throw new ArgumentOutOfRangeException("index must be less than endIndex.");
		}
		if (index > m_model.Count - 1)
		{
			throw new ArgumentOutOfRangeException("index must be less than points count.");
		}
		if (endIndex > m_model.Count - 1)
		{
			throw new ArgumentOutOfRangeException("endIndex must be less than points count.");
		}
		int index2 = 0;
		if (ProcessUsageString(useValue, out index2) == Coordinate.X)
		{
			index = FindXValue(value, index, endIndex);
		}
		else
		{
			index = FindYValue(value, index2, index, endIndex);
		}
		if (index > -1)
		{
			return new ChartPoint(m_model, index, m_categoryModel);
		}
		return null;
	}

	public ChartPoint FindMinValue()
	{
		int num = FindMinYValue(0, 0, m_model.Count - 1);
		if (num > -1)
		{
			return new ChartPoint(m_model, num, m_categoryModel);
		}
		return null;
	}

	public ChartPoint FindMinValue(string useValue)
	{
		int index = 0;
		return FindMinValue(useValue, ref index);
	}

	public ChartPoint FindMinValue(string useValue, ref int index)
	{
		return FindMinValue(useValue, ref index, m_model.Count - 1);
	}

	public ChartPoint FindMinValue(string useValue, ref int index, int endIndex)
	{
		if (endIndex < index)
		{
			throw new ArgumentOutOfRangeException("index must be less than endIndex.");
		}
		if (index > m_model.Count - 1)
		{
			throw new ArgumentOutOfRangeException("index must be less than points count.");
		}
		if (endIndex > m_model.Count - 1)
		{
			throw new ArgumentOutOfRangeException("endIndex must be less than points count.");
		}
		int index2 = 0;
		if (ProcessUsageString(useValue, out index2) == Coordinate.X)
		{
			index = FindMinXValue(index, endIndex);
		}
		else
		{
			index = FindMinYValue(index2, index, endIndex);
		}
		if (index > -1)
		{
			return new ChartPoint(m_model, index, m_categoryModel);
		}
		return null;
	}

	public ChartPoint FindMaxValue()
	{
		int num = FindMaxYValue(0, 0, m_model.Count - 1);
		if (num > -1)
		{
			return new ChartPoint(m_model, num, m_categoryModel);
		}
		return null;
	}

	public ChartPoint FindMaxValue(string useValue)
	{
		int index = 0;
		return FindMaxValue(useValue, ref index);
	}

	public ChartPoint FindMaxValue(string useValue, ref int index)
	{
		return FindMaxValue(useValue, ref index, m_model.Count - 1);
	}

	public ChartPoint FindMaxValue(string useValue, ref int index, int endIndex)
	{
		if (endIndex < index)
		{
			throw new ArgumentOutOfRangeException("index must be less than endIndex.");
		}
		if (index > m_model.Count - 1)
		{
			throw new ArgumentOutOfRangeException("index must be less than points count.");
		}
		if (endIndex > m_model.Count - 1)
		{
			throw new ArgumentOutOfRangeException("endIndex must be less than points count.");
		}
		int index2 = 0;
		if (ProcessUsageString(useValue, out index2) == Coordinate.X)
		{
			index = FindMaxXValue(index, endIndex);
		}
		else
		{
			index = FindMaxYValue(index2, index, endIndex);
		}
		if (index > -1)
		{
			return new ChartPoint(m_model, index, m_categoryModel);
		}
		return null;
	}

	public void Refresh()
	{
		if (m_model != null)
		{
			m_minX = double.MaxValue;
			m_minY = double.MaxValue;
			m_maxX = double.MinValue;
			m_maxY = double.MinValue;
			List<double> list = new List<double>();
			for (int i = 0; i < m_model.Count; i++)
			{
				double x = m_model.GetX(i);
				double[] y = m_model.GetY(i);
				while (list.Count < y.Length)
				{
					list.Add(0.0);
				}
				for (int j = 0; j < y.Length; j++)
				{
					list[j] += Math.Max(0.0, y[j]);
				}
				m_minX = Math.Min(m_minX, x);
				m_minY = Math.Min(m_minY, ChartMath.Min(y));
				m_maxX = Math.Max(m_maxX, x);
				m_maxY = Math.Max(m_maxY, ChartMath.Max(y));
			}
			m_sums = list.ToArray();
		}
		else
		{
			m_minX = 0.0;
			m_minY = 0.0;
			m_maxX = 0.0;
			m_maxY = 0.0;
			m_sums = null;
		}
	}

	public override string ToString()
	{
		return $"MinX: {MinX}, MaxX: {MaxX}, MinY: {MinY}, MaxY: {MaxY}";
	}

	private int FindXValue(double value, int from, int to)
	{
		for (int i = from; i <= to; i++)
		{
			if (m_model.GetX(i) == value)
			{
				return i;
			}
		}
		return -1;
	}

	private int FindYValue(double value, int yIndex, int from, int to)
	{
		for (int i = from; i <= to; i++)
		{
			double[] y = m_model.GetY(i);
			if (y.Length > yIndex && y[yIndex] == value)
			{
				return i;
			}
		}
		return -1;
	}

	private int FindMinXValue(int from, int to)
	{
		int result = -1;
		double num = double.MaxValue;
		for (int i = from; i <= to; i++)
		{
			double x = m_model.GetX(i);
			if (x < num)
			{
				num = x;
				result = i;
			}
		}
		return result;
	}

	private int FindMaxXValue(int from, int to)
	{
		int result = -1;
		double num = double.MinValue;
		for (int i = from; i <= to; i++)
		{
			double x = m_model.GetX(i);
			if (x > num)
			{
				num = x;
				result = i;
			}
		}
		return result;
	}

	private int FindMinYValue(int yIndex, int from, int to)
	{
		int result = -1;
		double num = double.MaxValue;
		for (int i = from; i <= to; i++)
		{
			double[] y = m_model.GetY(i);
			if (y.Length > yIndex && y[yIndex] < num)
			{
				num = y[yIndex];
				result = i;
			}
		}
		return result;
	}

	private int FindMaxYValue(int yIndex, int from, int to)
	{
		int result = -1;
		double num = double.MinValue;
		for (int i = from; i <= to; i++)
		{
			double[] y = m_model.GetY(i);
			if (y.Length > yIndex && y[yIndex] > num)
			{
				num = y[yIndex];
				result = i;
			}
		}
		return result;
	}

	private void OnModelChanged(object sender, ListChangedEventArgs e)
	{
		m_needUpdate = true;
	}

	private void EnsureRefreshed()
	{
		if (m_needUpdate)
		{
			Refresh();
			m_needUpdate = false;
		}
	}

	private Coordinate ProcessUsageString(string request, out int index)
	{
		if (string.Compare(request, "x", ignoreCase: true) == 0)
		{
			index = -1;
			return Coordinate.X;
		}
		if (request.StartsWith("y", StringComparison.InvariantCultureIgnoreCase))
		{
			if (request.Length == 1)
			{
				index = 0;
			}
			else if (!int.TryParse(request.Substring(1), out index))
			{
				throw new ArgumentException("Incorrect request");
			}
			return Coordinate.Y;
		}
		throw new ArgumentException("Incorrect request");
	}
}
