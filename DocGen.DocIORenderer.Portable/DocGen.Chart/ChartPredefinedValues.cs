using System.ComponentModel;

namespace DocGen.Chart;

[EditorBrowsable(EditorBrowsableState.Never)]
internal static class ChartPredefinedValues
{
	public static ChartPoint[] GetPoints(ChartSeriesType type, int index)
	{
		ChartPoint[] array = null;
		switch (type)
		{
		case ChartSeriesType.Line:
		case ChartSeriesType.Spline:
		case ChartSeriesType.RotatedSpline:
		case ChartSeriesType.StepLine:
			if (index % 3 == 0)
			{
				return ConvertYtoPoints(new double[7] { 15.0, 20.0, 30.0, 45.0, 65.0, 70.0, 75.0 });
			}
			if (index % 3 == 1)
			{
				return ConvertYtoPoints(new double[7] { 45.0, 52.0, 62.0, 55.0, 50.0, 45.0, 30.0 });
			}
			return ConvertYtoPoints(new double[7] { 90.0, 80.0, 75.0, 85.0, 65.0, 55.0, 50.0 });
		case ChartSeriesType.Scatter:
		case ChartSeriesType.Column:
		case ChartSeriesType.Bar:
			if (index % 2 == 0)
			{
				return ConvertYtoPoints(new double[5] { 55.0, 70.0, 80.0, 65.0, 75.0 });
			}
			return ConvertYtoPoints(new double[5] { 70.0, 35.0, 65.0, 25.0, 50.0 });
		case ChartSeriesType.Gantt:
			if (index % 2 == 0)
			{
				return ConvertYtoPoints(new double[5] { 10.0, 12.0, 8.0, 15.0, 13.0 }, new double[5] { 20.0, 22.0, 20.0, 24.0, 21.0 });
			}
			return ConvertYtoPoints(new double[5] { 15.0, 10.0, 13.0, 14.0, 17.0 }, new double[5] { 25.0, 18.0, 21.0, 19.0, 26.0 });
		case ChartSeriesType.StackingBar:
		case ChartSeriesType.StackingBar100:
			if (index % 3 == 0)
			{
				return ConvertYtoPoints(new double[5] { 55.0, 70.0, 80.0, 65.0, 75.0 });
			}
			if (index % 3 == 1)
			{
				return ConvertYtoPoints(new double[5] { 70.0, 35.0, 65.0, 25.0, 50.0 });
			}
			return ConvertYtoPoints(new double[5] { 100.0, 55.0, 35.0, 45.0, 65.0 });
		case ChartSeriesType.Area:
		case ChartSeriesType.SplineArea:
		case ChartSeriesType.StepArea:
			if (index % 2 == 0)
			{
				return ConvertYtoPoints(new double[7] { 55.0, 60.0, 75.0, 45.0, 50.0, 40.0, 30.0 });
			}
			return ConvertYtoPoints(new double[7] { 55.0, 70.0, 80.0, 65.0, 75.0, 70.0, 50.0 });
		case ChartSeriesType.RangeArea:
			if (index % 2 == 0)
			{
				return ConvertYtoPoints(new double[7] { 20.0, 25.0, 25.0, 30.0, 15.0, 20.0, 30.0 }, new double[7] { 70.0, 60.0, 75.0, 70.0, 70.0, 65.0, 55.0 });
			}
			return ConvertYtoPoints(new double[7] { 30.0, 35.0, 40.0, 35.0, 30.0, 35.0, 35.0 }, new double[7] { 80.0, 75.0, 85.0, 75.0, 80.0, 75.0, 75.0 });
		case ChartSeriesType.StackingArea:
		case ChartSeriesType.StackingArea100:
			if (index % 3 == 0)
			{
				return ConvertYtoPoints(new double[7] { 55.0, 60.0, 75.0, 45.0, 50.0, 40.0, 60.0 });
			}
			if (index % 3 == 1)
			{
				return ConvertYtoPoints(new double[7] { 55.0, 70.0, 80.0, 65.0, 75.0, 70.0, 50.0 });
			}
			return ConvertYtoPoints(new double[7] { 100.0, 55.0, 35.0, 45.0, 65.0, 55.0, 50.0 });
		case ChartSeriesType.StackingColumn:
		case ChartSeriesType.StackingColumn100:
			if (index % 3 == 0)
			{
				return ConvertYtoPoints(new double[5] { 55.0, 70.0, 80.0, 65.0, 75.0 });
			}
			if (index % 3 == 1)
			{
				return ConvertYtoPoints(new double[5] { 70.0, 35.0, 65.0, 25.0, 50.0 });
			}
			return ConvertYtoPoints(new double[5] { 90.0, 80.0, 75.0, 85.0, 65.0 });
		case ChartSeriesType.Pie:
		case ChartSeriesType.Funnel:
		case ChartSeriesType.Pyramid:
			return ConvertYtoPoints(new double[5] { 70.0, 35.0, 65.0, 25.0, 50.0 });
		case ChartSeriesType.HiLo:
			if (index % 2 == 0)
			{
				return ConvertYtoPoints(new double[5] { 65.0, 70.0, 68.0, 75.0, 102.0 }, new double[5] { 30.0, 30.0, 15.0, 30.0, 40.0 });
			}
			return ConvertYtoPoints(new double[5] { 56.0, 55.0, 50.0, 65.0, 90.0 }, new double[5] { 30.0, 15.0, 25.0, 45.0, 60.0 });
		case ChartSeriesType.HiLoOpenClose:
		case ChartSeriesType.Candle:
			return ConvertYtoPoints(new double[5] { 65.0, 70.0, 68.0, 75.0, 102.0 }, new double[5] { 30.0, 30.0, 15.0, 30.0, 40.0 }, new double[5] { 56.0, 55.0, 50.0, 65.0, 90.0 }, new double[5] { 40.0, 40.0, 30.0, 45.0, 60.0 });
		case ChartSeriesType.Bubble:
			if (index % 2 == 0)
			{
				return ConvertYtoPoints(new double[5] { 55.0, 70.0, 80.0, 65.0, 75.0 }, new double[5] { 1.0, 2.0, 3.0, 1.0, 7.0 });
			}
			return ConvertYtoPoints(new double[5] { 70.0, 35.0, 65.0, 25.0, 50.0 }, new double[5] { 2.0, 3.0, 7.0, 2.0, 1.0 });
		case ChartSeriesType.Kagi:
		case ChartSeriesType.Renko:
		case ChartSeriesType.ThreeLineBreak:
			return ConvertYtoPoints(new double[20]
			{
				27.0, 25.0, 16.0, 24.0, 19.0, 18.0, 10.0, 15.0, 12.0, 19.0,
				15.0, 12.0, 10.0, 22.0, 13.0, 15.0, 12.0, 10.0, 22.0, 13.0
			});
		case ChartSeriesType.Radar:
		case ChartSeriesType.Polar:
			if (index % 2 == 0)
			{
				return ConvertYtoPoints(new double[5] { 70.0, 35.0, 65.0, 25.0, 50.0 });
			}
			return ConvertYtoPoints(new double[5] { 56.0, 55.0, 50.0, 65.0, 90.0 });
		case ChartSeriesType.ColumnRange:
			if (index % 2 == 0)
			{
				return ConvertYtoPoints(new double[5] { 20.0, 25.0, 45.0, 50.0, 15.0 }, new double[5] { 70.0, 60.0, 75.0, 70.0, 75.0 });
			}
			return ConvertYtoPoints(new double[5] { 30.0, 35.0, 20.0, 35.0, 30.0 }, new double[5] { 80.0, 75.0, 55.0, 85.0, 80.0 });
		case ChartSeriesType.PointAndFigure:
			return ConvertYtoPoints(new double[20]
			{
				27.0, 25.0, 16.0, 24.0, 19.0, 18.0, 10.0, 15.0, 12.0, 19.0,
				15.0, 12.0, 10.0, 22.0, 13.0, 15.0, 12.0, 10.0, 22.0, 13.0
			}, new double[20]
			{
				10.0, 12.0, 13.0, 15.0, 14.0, 15.0, 4.0, 8.0, 5.0, 14.0,
				10.0, 6.0, 8.0, 18.0, 8.0, 12.0, 8.0, 9.0, 19.0, 10.0
			});
		case ChartSeriesType.BoxAndWhisker:
			return ConvertYtoPoints(new double[5] { 10.0, 12.0, 8.0, 15.0, 13.0 }, new double[5] { 20.0, 22.0, 20.0, 24.0, 21.0 }, new double[5] { 30.0, 33.0, 31.0, 34.0, 32.0 }, new double[5] { 40.0, 40.0, 42.0, 45.0, 43.0 }, new double[5] { 50.0, 51.0, 51.0, 52.0, 53.0 });
		case ChartSeriesType.Histogram:
			return ConvertXtoPoints(new double[5] { 100.0, 200.0, 350.0, 450.0, 500.0 });
		case ChartSeriesType.HeatMap:
			return ConvertYtoPoints(new double[5] { 27.0, 25.0, 16.0, 24.0, 19.0 }, new double[5] { 10.0, 12.0, 13.0, 15.0, 14.0 });
		case ChartSeriesType.Tornado:
			if (index % 2 == 0)
			{
				return ConvertYtoPoints(new double[5], new double[5] { 20.0, 22.0, 20.0, 24.0, 21.0 });
			}
			return ConvertYtoPoints(new double[5], new double[5] { -25.0, -18.0, -21.0, -19.0, -26.0 });
		default:
			return new ChartPoint[0];
		}
	}

	public static int GetSeriesCount(ChartSeriesType type)
	{
		int num = 0;
		switch (type)
		{
		case ChartSeriesType.Line:
		case ChartSeriesType.Spline:
		case ChartSeriesType.RotatedSpline:
		case ChartSeriesType.StackingBar:
		case ChartSeriesType.StackingArea:
		case ChartSeriesType.StackingColumn:
		case ChartSeriesType.StackingArea100:
		case ChartSeriesType.StackingBar100:
		case ChartSeriesType.StackingColumn100:
		case ChartSeriesType.StepLine:
			return 3;
		case ChartSeriesType.Scatter:
		case ChartSeriesType.Column:
		case ChartSeriesType.Bar:
		case ChartSeriesType.Gantt:
		case ChartSeriesType.Area:
		case ChartSeriesType.RangeArea:
		case ChartSeriesType.SplineArea:
		case ChartSeriesType.HiLo:
		case ChartSeriesType.Bubble:
		case ChartSeriesType.StepArea:
		case ChartSeriesType.Radar:
		case ChartSeriesType.Polar:
		case ChartSeriesType.ColumnRange:
		case ChartSeriesType.Tornado:
			return 2;
		default:
			return 1;
		}
	}

	private static ChartPoint[] ConvertYtoPoints(params double[][] array)
	{
		ChartPoint[] array2 = new ChartPoint[array[0].Length];
		for (int i = 0; i < array2.Length; i++)
		{
			double[] array3 = new double[array.Length];
			for (int j = 0; j < array3.Length; j++)
			{
				array3[j] = array[j][i];
			}
			array2[i] = new ChartPoint(i + 1, array3);
		}
		return array2;
	}

	private static ChartPoint[] ConvertXtoPoints(double[] xvalues)
	{
		ChartPoint[] array = new ChartPoint[xvalues.Length];
		for (int i = 0; i < xvalues.Length; i++)
		{
			array[i] = new ChartPoint(xvalues[i], 0.0);
		}
		return array;
	}
}
