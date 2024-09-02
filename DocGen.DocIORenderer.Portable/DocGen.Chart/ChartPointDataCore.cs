using System;

namespace DocGen.Chart;

internal class ChartPointDataCore : IChartPointCore, IChartCategoryPointCore
{
	private IChartSeriesModel ds;

	private IChartSeriesCategory cs;

	private int xIndex;

	public double X
	{
		get
		{
			return ds.GetX(xIndex);
		}
		set
		{
			if (IsEditableData())
			{
				GetEditableData().SetX(xIndex, value);
			}
		}
	}

	public double[] Y
	{
		get
		{
			return ds.GetY(xIndex);
		}
		set
		{
			if (IsEditableData())
			{
				GetEditableData().SetY(xIndex, value);
			}
		}
	}

	public bool IsEmpty
	{
		get
		{
			return ds.GetEmpty(xIndex);
		}
		set
		{
			if (IsEditableData())
			{
				GetEditableData().SetEmpty(xIndex, value);
			}
		}
	}

	public string Category
	{
		get
		{
			if (cs == null)
			{
				if (ds is ChartSeriesModel)
				{
					cs = ds as ChartSeriesModel;
				}
				else
				{
					cs = new ChartSeriesModel();
				}
			}
			return cs.GetCategory(xIndex);
		}
		set
		{
			if (IsEditableCategoryData())
			{
				GetEditableCategoryData().SetCategory(xIndex, value);
			}
		}
	}

	public DateTime[] YDates
	{
		get
		{
			double[] y = Y;
			DateTime[] array = new DateTime[y.GetLength(0)];
			for (int i = 0; i < y.GetLength(0); i++)
			{
				array[i] = DateTime.FromOADate(y[i]);
			}
			return array;
		}
		set
		{
			double[] array = new double[value.GetLength(0)];
			for (int i = 0; i < value.GetLength(0); i++)
			{
				array[i] = value[i].ToOADate();
			}
			Y = array;
		}
	}

	public ChartPointDataCore(IChartSeriesModel ds, int xIndex)
	{
		this.ds = ds;
		this.xIndex = xIndex;
	}

	public ChartPointDataCore(IChartSeriesCategory cs, int xIndex)
	{
		this.cs = cs;
		this.xIndex = xIndex;
	}

	private bool IsEditableData()
	{
		if (ds != null)
		{
			return ds is IEditableChartSeriesModel;
		}
		return false;
	}

	private bool IsEditableCategoryData()
	{
		if (cs != null)
		{
			return cs is IChartEditableCategory;
		}
		return false;
	}

	private IEditableChartSeriesModel GetEditableData()
	{
		if (IsEditableData())
		{
			return ds as IEditableChartSeriesModel;
		}
		return null;
	}

	private IChartEditableCategory GetEditableCategoryData()
	{
		if (IsEditableCategoryData())
		{
			return cs as IChartEditableCategory;
		}
		return null;
	}
}
