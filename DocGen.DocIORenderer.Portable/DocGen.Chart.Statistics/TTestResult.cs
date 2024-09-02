namespace DocGen.Chart.Statistics;

internal class TTestResult
{
	internal double degreeOfFreedom;

	internal double firstSeriesMean;

	internal double firstSeriesVariance;

	internal double probabilityTOneTail;

	internal double probabilityTTwoTail;

	internal double secondSeriesMean;

	internal double secondSeriesVariance;

	internal double tCriticalValueOneTail;

	internal double tCriticalValueTwoTail;

	internal double tValue;

	public double DegreeOfFreedom => degreeOfFreedom;

	public double FirstSeriesMean => firstSeriesMean;

	public double FirstSeriesVariance => firstSeriesVariance;

	public double ProbabilityTOneTail => probabilityTOneTail;

	public double ProbabilityTTwoTail => probabilityTTwoTail;

	public double SecondSeriesMean => secondSeriesMean;

	public double SecondSeriesVariance => secondSeriesVariance;

	public double TCriticalValueOneTail => tCriticalValueOneTail;

	public double TCriticalValueTwoTail => tCriticalValueTwoTail;

	public double TValue => tValue;
}
