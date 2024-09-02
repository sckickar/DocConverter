using System;

namespace DocGen.Chart.Statistics;

internal class BasicStatisticalFormulas
{
	public static double Mean(ChartSeries series)
	{
		double num = 0.0;
		int count = series.Points.Count;
		for (int i = 0; i < count; i++)
		{
			num += series.Points[i].X;
		}
		return num / (double)count;
	}

	public static double Mean(ChartSeries series, int yIndex)
	{
		double num = 0.0;
		int count = series.Points.Count;
		for (int i = 0; i < count; i++)
		{
			num += series.Points[i].YValues[yIndex];
		}
		return num / (double)count;
	}

	public static double VarianceUnbiasedEstimator(ChartSeries series)
	{
		double num = 0.0;
		double num2 = Mean(series);
		int count = series.Points.Count;
		for (int i = 0; i < count; i++)
		{
			double num3 = series.Points[i].X - num2;
			num += num3 * num3;
		}
		return num / (double)(count - 1);
	}

	public static double VarianceUnbiasedEstimator(ChartSeries series, int yIndex)
	{
		double num = 0.0;
		double num2 = Mean(series, yIndex);
		int count = series.Points.Count;
		for (int i = 0; i < count; i++)
		{
			double num3 = series.Points[i].YValues[yIndex] - num2;
			num += num3 * num3;
		}
		return num / (double)(count - 1);
	}

	public static double VarianceBiasedEstimator(ChartSeries series)
	{
		double num = 0.0;
		double num2 = Mean(series);
		int count = series.Points.Count;
		for (int i = 0; i < count; i++)
		{
			double num3 = series.Points[i].X - num2;
			num += num3 * num3;
		}
		return num / (double)count;
	}

	public static double VarianceBiasedEstimator(ChartSeries series, int yIndex)
	{
		double num = 0.0;
		double num2 = Mean(series, yIndex);
		int count = series.Points.Count;
		for (int i = 0; i < count; i++)
		{
			double num3 = series.Points[i].YValues[yIndex] - num2;
			num += num3 * num3;
		}
		return num / (double)count;
	}

	public static double Variance(ChartSeries series, bool sampleVariance)
	{
		if (sampleVariance)
		{
			return VarianceUnbiasedEstimator(series);
		}
		return VarianceBiasedEstimator(series);
	}

	public static double Variance(ChartSeries series, int yIndex, bool sampleVariance)
	{
		if (!sampleVariance)
		{
			return VarianceBiasedEstimator(series, yIndex);
		}
		return VarianceUnbiasedEstimator(series, yIndex);
	}

	public static double StandardDeviation(ChartSeries series, bool sampleVariance)
	{
		return Math.Sqrt(Variance(series, sampleVariance));
	}

	public static double StandardDeviation(ChartSeries series, int yIndex, bool sampleVariance)
	{
		return Math.Sqrt(Variance(series, yIndex, sampleVariance));
	}

	public static double Covariance(ChartSeries series1, ChartSeries series2)
	{
		double num = Mean(series1);
		double num2 = Mean(series2);
		double num3 = 0.0;
		int count = series1.Points.Count;
		int count2 = series2.Points.Count;
		if (count != count2)
		{
			throw new InvalidOperationException("Series have different lengths.");
		}
		for (int i = 0; i < count; i++)
		{
			num3 += (series1.Points[i].X - num) * (series2.Points[i].X - num2);
		}
		return num3 / (double)count;
	}

	public static double Covariance(ChartSeries series1, ChartSeries series2, int yIndex)
	{
		double num = Mean(series1, yIndex);
		double num2 = Mean(series2, yIndex);
		double num3 = 0.0;
		int count = series1.Points.Count;
		int count2 = series2.Points.Count;
		if (count != count2)
		{
			throw new InvalidOperationException("Series have different lengths.");
		}
		for (int i = 0; i < count; i++)
		{
			num3 += (series1.Points[i].YValues[yIndex] - num) * (series2.Points[i].YValues[yIndex] - num2);
		}
		return num3 / (double)count;
	}

	public static double Correlation(ChartSeries series1, ChartSeries series2)
	{
		double num = Mean(series1);
		double num2 = Mean(series2);
		int count = series1.Points.Count;
		int count2 = series2.Points.Count;
		if (count != count2)
		{
			throw new InvalidOperationException("Series have different lengths.");
		}
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		for (int i = 0; i < count; i++)
		{
			double num6 = series1.Points[i].X - num;
			num4 += num6 * num6;
			double num7 = series2.Points[i].X - num2;
			num5 += num7 * num7;
			num3 += num6 * num7;
		}
		return num3 / Math.Sqrt(num4 * num5);
	}

	public static double Correlation(ChartSeries series1, ChartSeries series2, int yIndex)
	{
		double num = Mean(series1, yIndex);
		double num2 = Mean(series2, yIndex);
		int count = series1.Points.Count;
		int count2 = series2.Points.Count;
		if (count != count2)
		{
			throw new InvalidOperationException("Series have different lengths.");
		}
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		for (int i = 0; i < count; i++)
		{
			double num6 = series1.Points[i].YValues[yIndex] - num;
			num4 += num6 * num6;
			double num7 = series2.Points[i].YValues[yIndex] - num2;
			num5 += num7 * num7;
			num3 += num6 * num7;
		}
		return num3 / Math.Sqrt(num4 * num5);
	}

	public static double Median(ChartSeries series)
	{
		int count = series.Points.Count;
		if (count % 2 == 1)
		{
			return series.Points[(count - 1) / 2].X;
		}
		double x = series.Points[count / 2].X;
		double x2 = series.Points[count / 2 - 1].X;
		return (x + x2) / 2.0;
	}

	public static double Median(ChartSeries series, int yIndex)
	{
		int count = series.Points.Count;
		if (count % 2 == 1)
		{
			return series.Points[(count - 1) / 2].YValues[yIndex];
		}
		double num = series.Points[count / 2].YValues[yIndex];
		double num2 = series.Points[count / 2 - 1].YValues[yIndex];
		return (num + num2) / 2.0;
	}

	public static ZTestResult ZTest(double hypothesizedMeanDifference, double varianceFirstGroup, double varianceSecondGroup, double probability, ChartSeries firstInputSeries, ChartSeries secondInputSeries)
	{
		ZTestResult zTestResult = new ZTestResult();
		zTestResult.firstSeriesMean = Mean(firstInputSeries);
		zTestResult.secondSeriesMean = Mean(secondInputSeries);
		zTestResult.firstSeriesVariance = varianceFirstGroup;
		zTestResult.secondSeriesVariance = varianceSecondGroup;
		int count = firstInputSeries.Points.Count;
		int count2 = secondInputSeries.Points.Count;
		double num = Math.Sqrt(varianceFirstGroup / (double)count + varianceSecondGroup / (double)count2);
		zTestResult.zValue = (zTestResult.firstSeriesMean - zTestResult.secondSeriesMean - hypothesizedMeanDifference) / num;
		zTestResult.zCriticalValueOneTail = UtilityFunctions.InverseNormalDistribution(Math.Max(1.0 - probability, probability));
		zTestResult.zCriticalValueTwoTail = UtilityFunctions.InverseNormalDistribution(Math.Max(1.0 - probability / 2.0, probability / 2.0));
		zTestResult.probabilityZOneTail = UtilityFunctions.NormalDistribution(0.0 - Math.Abs(zTestResult.zValue));
		zTestResult.probabilityZTwoTail = 2.0 * UtilityFunctions.NormalDistribution(0.0 - Math.Abs(zTestResult.zValue));
		return zTestResult;
	}

	public static ZTestResult ZTest(double hypothesizedMeanDifference, double varianceFirstGroup, double varianceSecondGroup, double probability, ChartSeries firstInputSeries, ChartSeries secondInputSeries, int yIndex)
	{
		ZTestResult zTestResult = new ZTestResult();
		zTestResult.firstSeriesMean = Mean(firstInputSeries, yIndex);
		zTestResult.secondSeriesMean = Mean(secondInputSeries, yIndex);
		zTestResult.firstSeriesVariance = varianceFirstGroup;
		zTestResult.secondSeriesVariance = varianceSecondGroup;
		int count = firstInputSeries.Points.Count;
		int count2 = secondInputSeries.Points.Count;
		double num = Math.Sqrt(varianceFirstGroup / (double)count + varianceSecondGroup / (double)count2);
		zTestResult.zValue = (zTestResult.firstSeriesMean - zTestResult.secondSeriesMean - hypothesizedMeanDifference) / num;
		zTestResult.zCriticalValueOneTail = UtilityFunctions.InverseNormalDistribution(Math.Max(1.0 - probability, probability));
		zTestResult.zCriticalValueTwoTail = UtilityFunctions.InverseNormalDistribution(Math.Max(1.0 - probability / 2.0, probability / 2.0));
		zTestResult.probabilityZOneTail = UtilityFunctions.NormalDistribution(0.0 - Math.Abs(zTestResult.zValue));
		zTestResult.probabilityZTwoTail = 2.0 * UtilityFunctions.NormalDistribution(0.0 - Math.Abs(zTestResult.zValue));
		return zTestResult;
	}

	public static TTestResult TTestEqualVariances(double hypothesizedMeanDifference, double probability, ChartSeries firstInputSeries, ChartSeries secondInputSeries)
	{
		TTestResult tTestResult = new TTestResult();
		tTestResult.degreeOfFreedom = firstInputSeries.Points.Count + secondInputSeries.Points.Count - 2;
		tTestResult.firstSeriesMean = Mean(firstInputSeries);
		tTestResult.secondSeriesMean = Mean(secondInputSeries);
		tTestResult.firstSeriesVariance = VarianceUnbiasedEstimator(firstInputSeries);
		tTestResult.secondSeriesVariance = VarianceUnbiasedEstimator(secondInputSeries);
		int count = firstInputSeries.Points.Count;
		int count2 = secondInputSeries.Points.Count;
		double num = ((double)(count - 1) * tTestResult.firstSeriesVariance + (double)(count2 - 1) * tTestResult.secondSeriesVariance) / tTestResult.degreeOfFreedom;
		double num2 = Math.Sqrt(num / (double)count + num / (double)count2);
		tTestResult.tValue = (tTestResult.firstSeriesMean - tTestResult.secondSeriesMean - hypothesizedMeanDifference) / num2;
		tTestResult.tCriticalValueOneTail = UtilityFunctions.InverseTCumulativeDistribution(Math.Max(1.0 - probability, probability), tTestResult.degreeOfFreedom, oneTail: true);
		tTestResult.tCriticalValueTwoTail = UtilityFunctions.InverseTCumulativeDistribution(Math.Max(1.0 - probability / 2.0, probability / 2.0), tTestResult.degreeOfFreedom, oneTail: true);
		tTestResult.probabilityTOneTail = UtilityFunctions.TCumulativeDistribution(0.0 - Math.Abs(tTestResult.tValue), tTestResult.degreeOfFreedom, oneTail: true);
		tTestResult.probabilityTTwoTail = 2.0 * UtilityFunctions.TCumulativeDistribution(0.0 - Math.Abs(tTestResult.tValue), tTestResult.degreeOfFreedom, oneTail: true);
		return tTestResult;
	}

	public static TTestResult TTestEqualVariances(double hypothesizedMeanDifference, double probability, ChartSeries firstInputSeries, ChartSeries secondInputSeries, int yIndex)
	{
		TTestResult tTestResult = new TTestResult();
		tTestResult.degreeOfFreedom = firstInputSeries.Points.Count + secondInputSeries.Points.Count - 2;
		tTestResult.firstSeriesMean = Mean(firstInputSeries, yIndex);
		tTestResult.secondSeriesMean = Mean(secondInputSeries, yIndex);
		tTestResult.firstSeriesVariance = VarianceUnbiasedEstimator(firstInputSeries, yIndex);
		tTestResult.secondSeriesVariance = VarianceUnbiasedEstimator(secondInputSeries, yIndex);
		int count = firstInputSeries.Points.Count;
		int count2 = secondInputSeries.Points.Count;
		double num = ((double)(count - 1) * tTestResult.firstSeriesVariance + (double)(count2 - 1) * tTestResult.secondSeriesVariance) / tTestResult.degreeOfFreedom;
		double num2 = Math.Sqrt(num / (double)count + num / (double)count2);
		tTestResult.tValue = (tTestResult.firstSeriesMean - tTestResult.secondSeriesMean - hypothesizedMeanDifference) / num2;
		tTestResult.tCriticalValueOneTail = UtilityFunctions.InverseTCumulativeDistribution(Math.Max(1.0 - probability, probability), tTestResult.degreeOfFreedom, oneTail: true);
		tTestResult.tCriticalValueTwoTail = UtilityFunctions.InverseTCumulativeDistribution(Math.Max(1.0 - probability / 2.0, probability / 2.0), tTestResult.degreeOfFreedom, oneTail: true);
		tTestResult.probabilityTOneTail = UtilityFunctions.TCumulativeDistribution(0.0 - Math.Abs(tTestResult.tValue), tTestResult.degreeOfFreedom, oneTail: true);
		tTestResult.probabilityTTwoTail = 2.0 * UtilityFunctions.TCumulativeDistribution(0.0 - Math.Abs(tTestResult.tValue), tTestResult.degreeOfFreedom, oneTail: true);
		return tTestResult;
	}

	public static TTestResult TTestUnEqualVariances(double hypothesizedMeanDifference, double probability, ChartSeries firstInputSeries, ChartSeries secondInputSeries)
	{
		TTestResult tTestResult = new TTestResult();
		tTestResult.firstSeriesMean = Mean(firstInputSeries);
		tTestResult.secondSeriesMean = Mean(secondInputSeries);
		tTestResult.firstSeriesVariance = VarianceUnbiasedEstimator(firstInputSeries);
		tTestResult.secondSeriesVariance = VarianceUnbiasedEstimator(secondInputSeries);
		int count = firstInputSeries.Points.Count;
		int count2 = secondInputSeries.Points.Count;
		double num = tTestResult.firstSeriesVariance / (double)count;
		double num2 = tTestResult.secondSeriesVariance / (double)count2;
		double num3 = num + num2;
		double num4 = Math.Sqrt(num3);
		tTestResult.degreeOfFreedom = num3 * num3 / (num * num / (double)(count - 1) + num2 * num2 / (double)(count2 - 1));
		tTestResult.tValue = (tTestResult.firstSeriesMean - tTestResult.secondSeriesMean - hypothesizedMeanDifference) / num4;
		tTestResult.tCriticalValueOneTail = UtilityFunctions.InverseTCumulativeDistribution(Math.Max(1.0 - probability, probability), tTestResult.degreeOfFreedom, oneTail: true);
		tTestResult.tCriticalValueTwoTail = UtilityFunctions.InverseTCumulativeDistribution(Math.Max(1.0 - probability / 2.0, probability / 2.0), tTestResult.degreeOfFreedom, oneTail: true);
		tTestResult.probabilityTOneTail = UtilityFunctions.TCumulativeDistribution(0.0 - Math.Abs(tTestResult.tValue), tTestResult.degreeOfFreedom, oneTail: true);
		tTestResult.probabilityTTwoTail = 2.0 * UtilityFunctions.TCumulativeDistribution(0.0 - Math.Abs(tTestResult.tValue), tTestResult.degreeOfFreedom, oneTail: true);
		return tTestResult;
	}

	public static TTestResult TTestUnEqualVariances(double hypothesizedMeanDifference, double probability, ChartSeries firstInputSeries, ChartSeries secondInputSeries, int yIndex)
	{
		TTestResult tTestResult = new TTestResult();
		tTestResult.firstSeriesMean = Mean(firstInputSeries, yIndex);
		tTestResult.secondSeriesMean = Mean(secondInputSeries, yIndex);
		tTestResult.firstSeriesVariance = VarianceUnbiasedEstimator(firstInputSeries, yIndex);
		tTestResult.secondSeriesVariance = VarianceUnbiasedEstimator(secondInputSeries, yIndex);
		int count = firstInputSeries.Points.Count;
		int count2 = secondInputSeries.Points.Count;
		double num = tTestResult.firstSeriesVariance / (double)count;
		double num2 = tTestResult.secondSeriesVariance / (double)count2;
		double num3 = num + num2;
		double num4 = Math.Sqrt(num3);
		tTestResult.degreeOfFreedom = num3 * num3 / (num * num / (double)(count - 1) + num2 * num2 / (double)(count2 - 1));
		tTestResult.tValue = (tTestResult.firstSeriesMean - tTestResult.secondSeriesMean - hypothesizedMeanDifference) / num4;
		tTestResult.tCriticalValueOneTail = UtilityFunctions.InverseTCumulativeDistribution(Math.Max(1.0 - probability, probability), tTestResult.degreeOfFreedom, oneTail: true);
		tTestResult.tCriticalValueTwoTail = UtilityFunctions.InverseTCumulativeDistribution(Math.Max(1.0 - probability / 2.0, probability / 2.0), tTestResult.degreeOfFreedom, oneTail: true);
		tTestResult.probabilityTOneTail = UtilityFunctions.TCumulativeDistribution(0.0 - Math.Abs(tTestResult.tValue), tTestResult.degreeOfFreedom, oneTail: true);
		tTestResult.probabilityTTwoTail = 2.0 * UtilityFunctions.TCumulativeDistribution(0.0 - Math.Abs(tTestResult.tValue), tTestResult.degreeOfFreedom, oneTail: true);
		return tTestResult;
	}

	public static TTestResult TTestPaired(double hypothesizedMeanDifference, double probability, ChartSeries firstInputSeries, ChartSeries secondInputSeries)
	{
		int count = firstInputSeries.Points.Count;
		int count2 = secondInputSeries.Points.Count;
		if (count != count2)
		{
			throw new InvalidOperationException("Series have different lengths.");
		}
		ChartSeries series = DifferenceX(firstInputSeries, secondInputSeries);
		TTestResult tTestResult = new TTestResult();
		double num = Mean(series);
		tTestResult.firstSeriesMean = Mean(firstInputSeries);
		tTestResult.secondSeriesMean = Mean(secondInputSeries);
		double num2 = VarianceUnbiasedEstimator(series);
		tTestResult.firstSeriesVariance = VarianceUnbiasedEstimator(firstInputSeries);
		tTestResult.secondSeriesVariance = VarianceUnbiasedEstimator(secondInputSeries);
		double num3 = Math.Sqrt(num2 / (double)count);
		tTestResult.degreeOfFreedom = count - 1;
		tTestResult.tValue = (num - hypothesizedMeanDifference) / num3;
		tTestResult.tCriticalValueOneTail = UtilityFunctions.InverseTCumulativeDistribution(Math.Max(1.0 - probability, probability), tTestResult.degreeOfFreedom, oneTail: true);
		tTestResult.tCriticalValueTwoTail = UtilityFunctions.InverseTCumulativeDistribution(Math.Max(1.0 - probability / 2.0, probability / 2.0), tTestResult.degreeOfFreedom, oneTail: true);
		tTestResult.probabilityTOneTail = UtilityFunctions.TCumulativeDistribution(0.0 - Math.Abs(tTestResult.tValue), tTestResult.degreeOfFreedom, oneTail: true);
		tTestResult.probabilityTTwoTail = 2.0 * UtilityFunctions.TCumulativeDistribution(0.0 - Math.Abs(tTestResult.tValue), tTestResult.degreeOfFreedom, oneTail: true);
		return tTestResult;
	}

	public static TTestResult TTestPaired(double hypothesizedMeanDifference, double probability, ChartSeries firstInputSeries, ChartSeries secondInputSeries, int yIndex)
	{
		int count = firstInputSeries.Points.Count;
		int count2 = secondInputSeries.Points.Count;
		if (count != count2)
		{
			throw new InvalidOperationException("Series have different lengths.");
		}
		ChartSeries series = DifferenceY(firstInputSeries, secondInputSeries, yIndex);
		TTestResult tTestResult = new TTestResult();
		double num = Mean(series, yIndex);
		tTestResult.firstSeriesMean = Mean(firstInputSeries, yIndex);
		tTestResult.secondSeriesMean = Mean(secondInputSeries, yIndex);
		double num2 = VarianceUnbiasedEstimator(series, yIndex);
		tTestResult.firstSeriesVariance = VarianceUnbiasedEstimator(firstInputSeries, yIndex);
		tTestResult.secondSeriesVariance = VarianceUnbiasedEstimator(secondInputSeries, yIndex);
		double num3 = Math.Sqrt(num2 / (double)count);
		tTestResult.degreeOfFreedom = count - 1;
		tTestResult.tValue = (num - hypothesizedMeanDifference) / num3;
		tTestResult.tCriticalValueOneTail = UtilityFunctions.InverseTCumulativeDistribution(Math.Max(1.0 - probability, probability), tTestResult.degreeOfFreedom, oneTail: true);
		tTestResult.tCriticalValueTwoTail = UtilityFunctions.InverseTCumulativeDistribution(Math.Max(1.0 - probability / 2.0, probability / 2.0), tTestResult.degreeOfFreedom, oneTail: true);
		tTestResult.probabilityTOneTail = UtilityFunctions.TCumulativeDistribution(0.0 - Math.Abs(tTestResult.tValue), tTestResult.degreeOfFreedom, oneTail: true);
		tTestResult.probabilityTTwoTail = 2.0 * UtilityFunctions.TCumulativeDistribution(0.0 - Math.Abs(tTestResult.tValue), tTestResult.degreeOfFreedom, oneTail: true);
		return tTestResult;
	}

	public static FTestResult FTest(double probability, ChartSeries firstInputSeries, ChartSeries secondInputSeries)
	{
		int count = firstInputSeries.Points.Count;
		int count2 = secondInputSeries.Points.Count;
		if (count != count2)
		{
			throw new InvalidOperationException("Series have different lengths.");
		}
		FTestResult fTestResult = new FTestResult();
		fTestResult.firstSeriesMean = Mean(firstInputSeries);
		fTestResult.secondSeriesMean = Mean(secondInputSeries);
		fTestResult.firstSeriesVariance = VarianceUnbiasedEstimator(firstInputSeries);
		fTestResult.secondSeriesVariance = VarianceUnbiasedEstimator(secondInputSeries);
		fTestResult.fValue = fTestResult.firstSeriesVariance / fTestResult.secondSeriesVariance;
		fTestResult.fCriticalValueOneTail = UtilityFunctions.InverseFCumulativeDistribution(1.0 - probability, count - 1, count2 - 1);
		fTestResult.probabilityFOneTail = 1.0 - UtilityFunctions.FCumulativeDistribution(fTestResult.fValue, count - 1, count2 - 1);
		return fTestResult;
	}

	public static FTestResult FTest(double probability, ChartSeries firstInputSeries, ChartSeries secondInputSeries, int yIndex)
	{
		int count = firstInputSeries.Points.Count;
		int count2 = secondInputSeries.Points.Count;
		if (count != count2)
		{
			throw new InvalidOperationException("Series have different lengths.");
		}
		FTestResult fTestResult = new FTestResult();
		fTestResult.firstSeriesMean = Mean(firstInputSeries, yIndex);
		fTestResult.secondSeriesMean = Mean(secondInputSeries, yIndex);
		fTestResult.firstSeriesVariance = VarianceUnbiasedEstimator(firstInputSeries, yIndex);
		fTestResult.secondSeriesVariance = VarianceUnbiasedEstimator(secondInputSeries, yIndex);
		fTestResult.fValue = fTestResult.firstSeriesVariance / fTestResult.secondSeriesVariance;
		fTestResult.fCriticalValueOneTail = UtilityFunctions.InverseFCumulativeDistribution(1.0 - probability, count - 1, count2 - 1);
		fTestResult.probabilityFOneTail = 1.0 - UtilityFunctions.FCumulativeDistribution(fTestResult.fValue, count - 1, count2 - 1);
		return fTestResult;
	}

	public static AnovaResult Anova(double probability, ChartSeries[] inputSeries)
	{
		int num = inputSeries.Length;
		int count = inputSeries[0].Points.Count;
		for (int i = 0; i < num; i++)
		{
			if (count != inputSeries[i].Points.Count)
			{
				throw new InvalidOperationException("Series have different lengths.");
			}
		}
		AnovaResult anovaResult = new AnovaResult();
		anovaResult.degreeOfFreedomTotal = count * num - 1;
		anovaResult.degreeOfFreedomWithinGroups = num * (count - 1);
		anovaResult.deegreeOfFreedomBetweenGroups = num - 1;
		ChartSeries chartSeries = new ChartSeries();
		for (int j = 0; j < num; j++)
		{
			chartSeries.Points.Add(VarianceUnbiasedEstimator(inputSeries[j]), 0.0);
		}
		anovaResult.meanSquareVarianceWithinGroups = Mean(chartSeries);
		anovaResult.sumOfSquaresWithinGroups = anovaResult.DegreeOfFreedomWithinGroups * anovaResult.meanSquareVarianceWithinGroups;
		ChartSeries chartSeries2 = new ChartSeries();
		for (int k = 0; k < num; k++)
		{
			chartSeries2.Points.Add(Mean(inputSeries[k]), 0.0);
		}
		anovaResult.meanSquareVarianceBetweenGroups = (double)count * VarianceUnbiasedEstimator(chartSeries2);
		anovaResult.sumOfSquaresBetweenGroups = (double)(num - 1) * anovaResult.meanSquareVarianceBetweenGroups;
		anovaResult.sumOfSquaresTotal = anovaResult.sumOfSquaresBetweenGroups + anovaResult.sumOfSquaresWithinGroups;
		anovaResult.fRatio = anovaResult.meanSquareVarianceBetweenGroups / anovaResult.meanSquareVarianceWithinGroups;
		anovaResult.fCriticalValue = UtilityFunctions.InverseFCumulativeDistribution(probability, anovaResult.deegreeOfFreedomBetweenGroups, anovaResult.degreeOfFreedomWithinGroups);
		return anovaResult;
	}

	public static AnovaResult Anova(double probability, ChartSeries[] inputSeries, int yIndex)
	{
		int num = inputSeries.Length;
		int count = inputSeries[0].Points.Count;
		for (int i = 0; i < num; i++)
		{
			if (count != inputSeries[i].Points.Count)
			{
				throw new InvalidOperationException("Series have different lengths.");
			}
		}
		AnovaResult anovaResult = new AnovaResult();
		anovaResult.degreeOfFreedomTotal = count * num - 1;
		anovaResult.degreeOfFreedomWithinGroups = num * (count - 1);
		anovaResult.deegreeOfFreedomBetweenGroups = num - 1;
		ChartSeries chartSeries = new ChartSeries();
		for (int j = 0; j < num; j++)
		{
			chartSeries.Points.Add(VarianceUnbiasedEstimator(inputSeries[j], yIndex), 0.0);
		}
		anovaResult.meanSquareVarianceWithinGroups = Mean(chartSeries, yIndex);
		anovaResult.sumOfSquaresWithinGroups = anovaResult.DegreeOfFreedomWithinGroups * anovaResult.meanSquareVarianceWithinGroups;
		ChartSeries chartSeries2 = new ChartSeries();
		for (int k = 0; k < num; k++)
		{
			chartSeries2.Points.Add(Mean(inputSeries[k], yIndex), 0.0);
		}
		anovaResult.meanSquareVarianceBetweenGroups = (double)count * VarianceUnbiasedEstimator(chartSeries2, yIndex);
		anovaResult.sumOfSquaresBetweenGroups = (double)(num - 1) * anovaResult.meanSquareVarianceBetweenGroups;
		anovaResult.sumOfSquaresTotal = anovaResult.sumOfSquaresBetweenGroups + anovaResult.sumOfSquaresWithinGroups;
		anovaResult.fRatio = anovaResult.meanSquareVarianceBetweenGroups / anovaResult.meanSquareVarianceWithinGroups;
		anovaResult.fCriticalValue = UtilityFunctions.InverseFCumulativeDistribution(probability, anovaResult.deegreeOfFreedomBetweenGroups, anovaResult.degreeOfFreedomWithinGroups);
		return anovaResult;
	}

	private static ChartSeries DifferenceX(ChartSeries series1, ChartSeries series2)
	{
		int count = series1.Points.Count;
		_ = series2.Points.Count;
		ChartSeries chartSeries = new ChartSeries();
		for (int i = 0; i < count; i++)
		{
			chartSeries.Points.Add(series1.Points[i].X - series2.Points[i].X, 0.0);
		}
		return chartSeries;
	}

	private static ChartSeries DifferenceY(ChartSeries series1, ChartSeries series2, int yIndex)
	{
		int count = series1.Points.Count;
		_ = series2.Points.Count;
		ChartSeries chartSeries = new ChartSeries();
		for (int i = 0; i < count; i++)
		{
			chartSeries.Points.Add(series1.Points[i].YValues[yIndex] - series2.Points[i].YValues[yIndex], 0.0);
		}
		return chartSeries;
	}
}
