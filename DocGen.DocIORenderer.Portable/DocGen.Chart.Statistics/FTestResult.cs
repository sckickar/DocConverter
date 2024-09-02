namespace DocGen.Chart.Statistics;

internal class FTestResult
{
	internal double firstSeriesMean;

	internal double firstSeriesVariance;

	internal double probabilityFOneTail;

	internal double secondSeriesMean;

	internal double secondSeriesVariance;

	internal double fCriticalValueOneTail;

	internal double fValue;

	public double FirstSeriesMean => firstSeriesMean;

	public double FirstSeriesVariance => firstSeriesVariance;

	public double ProbabilityFOneTail => probabilityFOneTail;

	public double SecondSeriesMean => secondSeriesMean;

	public double SecondSeriesVariance => secondSeriesVariance;

	public double FCriticalValueOneTail => fCriticalValueOneTail;

	public double FValue => fValue;
}
