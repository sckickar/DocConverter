namespace DocGen.Chart;

internal class ChartPointContainedCore : IChartPointCore, IChartCategoryPointCore
{
	private double x;

	private double[] y;

	private bool isEmpty;

	private string category;

	public double X
	{
		get
		{
			return x;
		}
		set
		{
			x = value;
		}
	}

	public double[] Y
	{
		get
		{
			return y;
		}
		set
		{
			y = value;
		}
	}

	public bool IsEmpty
	{
		get
		{
			return isEmpty;
		}
		set
		{
			isEmpty = value;
		}
	}

	public string Category
	{
		get
		{
			return category;
		}
		set
		{
			category = value;
		}
	}

	public ChartPointContainedCore(double x, double[] y)
	{
		this.x = x;
		this.y = y;
	}

	public ChartPointContainedCore(double x, double[] y, string category)
	{
		this.x = x;
		this.y = y;
		this.category = category;
	}

	public double GetY(int yIndex)
	{
		return y[yIndex];
	}
}
