namespace DocGen.Chart.Statistics;

internal class AnovaResult
{
	internal double deegreeOfFreedomBetweenGroups;

	internal double degreeOfFreedomTotal;

	internal double degreeOfFreedomWithinGroups;

	internal double fCriticalValue;

	internal double fRatio;

	internal double meanSquareVarianceBetweenGroups;

	internal double meanSquareVarianceWithinGroups;

	internal double sumOfSquaresBetweenGroups;

	internal double sumOfSquaresTotal;

	internal double sumOfSquaresWithinGroups;

	public double DegreeOfFreedomBetweenGroups => deegreeOfFreedomBetweenGroups;

	public double DegreeOfFreedomTotal => degreeOfFreedomTotal;

	public double DegreeOfFreedomWithinGroups => degreeOfFreedomWithinGroups;

	public double FCriticalValue => fCriticalValue;

	public double FRatio => fRatio;

	public double MeanSquareVarianceBetweenGroups => meanSquareVarianceBetweenGroups;

	public double MeanSquareVarianceWithinGroups => meanSquareVarianceWithinGroups;

	public double SumOfSquaresBetweenGroups => sumOfSquaresBetweenGroups;

	public double SumOfSquaresTotal => sumOfSquaresTotal;

	public double SumOfSquaresWithinGroups => sumOfSquaresWithinGroups;
}
