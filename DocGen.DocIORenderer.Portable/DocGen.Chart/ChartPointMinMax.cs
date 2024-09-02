namespace DocGen.Chart;

internal class ChartPointMinMax : IChartPointMinMax
{
	private IChartSeriesModel model;

	private int xIndex;

	public double Max
	{
		get
		{
			if (model.GetEmpty(xIndex))
			{
				return double.MinValue;
			}
			return ChartMinMaxEvaluator.CalcMax(model.GetY(xIndex));
		}
	}

	public double Min
	{
		get
		{
			if (model.GetEmpty(xIndex))
			{
				return double.MaxValue;
			}
			return ChartMinMaxEvaluator.CalcMin(model.GetY(xIndex));
		}
	}

	public double X => model.GetX(xIndex);

	public ChartPointMinMax(IChartSeriesModel model, int xIndex)
	{
		this.model = model;
		this.xIndex = xIndex;
	}

	public double GetY(int index, int yIndex)
	{
		return model.GetY(index)[yIndex];
	}
}
