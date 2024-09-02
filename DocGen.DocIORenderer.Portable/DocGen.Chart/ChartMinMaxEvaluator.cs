namespace DocGen.Chart;

internal class ChartMinMaxEvaluator
{
	public static double CalcMin(double[] values)
	{
		int length = values.GetLength(0);
		double num = double.MaxValue;
		for (int i = 0; i < length; i++)
		{
			if (num > values[i])
			{
				num = values[i];
			}
		}
		return num;
	}

	public static double CalcMax(double[] values)
	{
		int length = values.GetLength(0);
		double num = double.MinValue;
		for (int i = 0; i < length; i++)
		{
			if (num < values[i])
			{
				num = values[i];
			}
		}
		return num;
	}
}
