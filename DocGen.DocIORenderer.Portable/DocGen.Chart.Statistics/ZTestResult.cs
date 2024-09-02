namespace DocGen.Chart.Statistics;

internal class ZTestResult
{
	internal double firstSeriesMean;

	internal double firstSeriesVariance;

	internal double probabilityZOneTail;

	internal double probabilityZTwoTail;

	internal double secondSeriesMean;

	internal double secondSeriesVariance;

	internal double zCriticalValueOneTail;

	internal double zCriticalValueTwoTail;

	internal double zValue;

	public double FirstSeriesMean => firstSeriesMean;

	public double FirstSeriesVariance => firstSeriesVariance;

	public double ProbabilityZOneTail => probabilityZOneTail;

	public double ProbabilityZTwoTail => probabilityZTwoTail;

	public double SecondSeriesMean => secondSeriesMean;

	public double SecondSeriesVariance => secondSeriesVariance;

	public double ZCriticalValueOneTail => zCriticalValueOneTail;

	public double ZCriticalValueTwoTail => zCriticalValueTwoTail;

	public double ZValue => zValue;
}
