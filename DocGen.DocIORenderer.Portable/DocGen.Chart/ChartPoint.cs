using System;
using System.Text;
using DocGen.Drawing;

namespace DocGen.Chart;

internal class ChartPoint
{
	public static ChartPoint Empty;

	private IChartPointCore m_chartPointCore;

	private IChartCategoryPointCore m_chartCategoryPointCore;

	private DashStyle m_dashStyle;

	public DashCap m_dashCap;

	public DateTime DateX
	{
		get
		{
			return DateTime.FromOADate(m_chartPointCore.X);
		}
		set
		{
			m_chartPointCore.X = value.ToOADate();
		}
	}

	public double X
	{
		get
		{
			return m_chartPointCore.X;
		}
		set
		{
			m_chartPointCore.X = value;
		}
	}

	public double[] YValues
	{
		get
		{
			return m_chartPointCore.Y;
		}
		set
		{
			m_chartPointCore.Y = value;
		}
	}

	public bool IsEmpty
	{
		get
		{
			return m_chartPointCore.IsEmpty;
		}
		set
		{
			m_chartPointCore.IsEmpty = value;
		}
	}

	public string Category
	{
		get
		{
			if (m_chartCategoryPointCore == null)
			{
				m_chartCategoryPointCore = new ChartPointContainedCore(m_chartPointCore.X, m_chartPointCore.Y);
			}
			return m_chartCategoryPointCore.Category;
		}
		set
		{
			m_chartCategoryPointCore.Category = value;
		}
	}

	public DateTime[] GetYValuesAsDateTime()
	{
		double[] yValues = YValues;
		DateTime[] array = new DateTime[yValues.GetLength(0)];
		for (int i = 0; i < yValues.GetLength(0); i++)
		{
			array[i] = DateTime.FromOADate(yValues[i]);
		}
		return array;
	}

	public ChartPoint()
	{
		m_chartPointCore = new ChartPointContainedCore(0.0, new double[1]);
	}

	public ChartPoint(double x, double[] y)
	{
		m_chartPointCore = new ChartPointContainedCore(x, y);
	}

	public ChartPoint(double x, double[] y, string category)
	{
		m_chartCategoryPointCore = (IChartCategoryPointCore)(m_chartPointCore = new ChartPointContainedCore(x, y, category));
	}

	public ChartPoint(double x, double y)
	{
		m_chartPointCore = new ChartPointContainedCore(x, new double[1] { y });
	}

	public ChartPoint(double x, double y, string category)
	{
		m_chartCategoryPointCore = (IChartCategoryPointCore)(m_chartPointCore = new ChartPointContainedCore(x, new double[1] { y }, category));
	}

	public ChartPoint(double x, DateTime[] dates)
	{
		double[] array = new double[dates.GetLength(0)];
		for (int i = 0; i < array.GetLength(0); i++)
		{
			array[i] = dates[i].ToOADate();
		}
		m_chartPointCore = new ChartPointContainedCore(x, array);
	}

	public ChartPoint(double x, DateTime date)
	{
		m_chartPointCore = new ChartPointContainedCore(x, new double[1] { date.ToOADate() });
	}

	public ChartPoint(DateTime date, double[] yValues)
	{
		m_chartPointCore = new ChartPointContainedCore(date.ToOADate(), yValues);
	}

	public ChartPoint(DateTime date, double y)
	{
		m_chartPointCore = new ChartPointContainedCore(date.ToOADate(), new double[1] { y });
	}

	internal ChartPoint(IChartSeriesModel ds, int xIndex, IChartSeriesCategory cs)
	{
		m_chartPointCore = new ChartPointDataCore(ds, xIndex);
		m_chartCategoryPointCore = new ChartPointDataCore(cs, xIndex);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("X : ");
		stringBuilder.Append(X);
		stringBuilder.Append("; Y : ");
		int i = 0;
		for (int num = YValues.Length - 1; i <= num; i++)
		{
			stringBuilder.Append(YValues[i]);
			if (i != num)
			{
				stringBuilder.Append(", ");
			}
		}
		return stringBuilder.ToString();
	}

	internal bool IsVisible()
	{
		if (!IsEmpty && X != double.NaN && YValues.Length >= 1)
		{
			return !HasNAN(YValues);
		}
		return false;
	}

	private bool HasNAN(double[] yValues)
	{
		for (int i = 0; i < yValues.Length; i++)
		{
			if (double.IsNaN(yValues[i]))
			{
				return true;
			}
		}
		return false;
	}
}
