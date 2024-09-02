using System;
using System.Collections.Generic;

namespace DocGen.OfficeChart.Calculate;

internal class LinearRegression : IDisposable
{
	internal CalcEngine Engine;

	private const char BMARKER = '\u0092';

	private string TIC = "\"";

	internal LinearRegression(CalcEngine engine)
	{
		Engine = engine;
	}

	internal void ComputeLinest(double[] y, double[] x, out double m, out double b, out string errorValue)
	{
		errorValue = string.Empty;
		m = 0.0;
		b = 1.0;
		int length = x.GetLength(0);
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		for (int i = 0; i < x.Length; i++)
		{
			if (x[i].ToString() != double.NaN.ToString() && y[i].ToString() != double.NaN.ToString())
			{
				num += x[i] * y[i];
				num2 += x[i];
				num3 += y[i];
				num4 += x[i] * x[i];
			}
			else
			{
				if (Engine.RethrowLibraryComputationExceptions)
				{
					throw new ArgumentException(Engine.FormulaErrorStrings[Engine.bad_formula]);
				}
				errorValue = Engine.ErrorStrings[1].ToString();
			}
		}
		b = (num - num2 * num3 / (double)length) / (num4 - num2 * num2 / (double)length);
		double num5 = 0.0;
		double num6 = 0.0;
		double num7 = 0.0;
		double num8 = 0.0;
		for (int j = 0; j < length; j++)
		{
			num7 += x[j];
			num8 += y[j];
		}
		num7 /= (double)length;
		num8 /= (double)length;
		for (int k = 0; k < length; k++)
		{
			double num9 = x[k] - num7;
			num5 += num9 * (y[k] - num8);
			num6 += num9 * num9;
		}
		m = num8 - num5 / num6 * num7;
	}

	internal void SplitRange(string range, ref string rangeValue, ref string logicalValue)
	{
		for (int i = 0; i < range.Length; i++)
		{
			range = range.Replace('\u0092'.ToString(), string.Empty);
			while (i != range.Length && (char.IsDigit(range[i]) | (range[i] == ':') | (range[i] == '!') | Engine.IsUpper(range[i])))
			{
				rangeValue += range[i++];
			}
			while (i != range.Length && ((range[i] == '"') | char.IsLetter(range[i]) | char.IsDigit(range[i]) | (range[i] == ',') | (range[i] == '~')))
			{
				logicalValue += range[i++];
			}
		}
	}

	internal void PerfromArrayMultiplication(double[,] a, double[,] b, out double[,] mult)
	{
		int length = a.GetLength(0);
		a.GetLength(1);
		int length2 = b.GetLength(0);
		int length3 = b.GetLength(1);
		mult = new double[length, length3];
		for (int i = 0; i <= length - 1; i++)
		{
			for (int j = 0; j <= length3 - 1; j++)
			{
				for (int k = 0; k <= length2 - 1; k++)
				{
					mult[i, j] += a[i, k] * b[k, j];
				}
			}
		}
	}

	internal void ComputeMultipleRegression(double[] y, double[,] X, int row, int col, out double b, out string errorValue, out double[] coefficients)
	{
		errorValue = string.Empty;
		b = 1.0;
		double[,] mult = new double[row, row];
		double[,] array = new double[col, row];
		for (int i = 0; i < col; i++)
		{
			for (int j = 0; j < row; j++)
			{
				array[i, j] = X[j, i];
			}
		}
		PerfromArrayMultiplication(array, X, out mult);
		double[,] iMatrix = new double[mult.GetLength(0), mult.GetLength(1)];
		Engine.GetCofactor(mult, out iMatrix);
		double[,] mult2 = new double[iMatrix.GetLength(0), array.GetLength(1)];
		PerfromArrayMultiplication(iMatrix, array, out mult2);
		int length = mult2.GetLength(0);
		y.GetLength(0);
		int length2 = mult2.GetLength(1);
		coefficients = new double[length];
		for (int k = 0; k <= length - 1; k++)
		{
			for (int l = 0; l <= length2 - 1; l++)
			{
				coefficients[k] += mult2[k, l] * y[l];
			}
		}
		b = coefficients[coefficients.Length - 1];
	}

	internal string ComputeXArg(double[] y, double[] x, string arg, double b, double m, bool padXValues, string errorValue, out double[] coefficients)
	{
		int num = arg.IndexOf(':');
		int num2 = Engine.RowIndex(arg.Substring(0, num));
		int num3 = Engine.RowIndex(arg.Substring(num + 1));
		int num4 = Engine.ColIndex(arg.Substring(0, num));
		int num5 = Engine.ColIndex(arg.Substring(num + 1, arg.Length - num - 1));
		coefficients = new double[num5 + 1];
		int num6 = ((num5 > num4) ? (num5 - num4 + 1) : (num4 - num5 + 1));
		if (num6 == 1)
		{
			if (Engine.RethrowLibraryComputationExceptions)
			{
				throw new ArgumentException(Engine.FormulaErrorStrings[Engine.bad_formula]);
			}
			return Engine.ErrorStrings[2].ToString();
		}
		if (x.Length / num6 == y.Length)
		{
			int num7 = 0;
			double[,] array = new double[y.GetLength(0), num6 + 1];
			for (int i = num4; i <= num5; i++)
			{
				arg = RangeInfo.GetAlphaLabel(i) + num2 + ":" + RangeInfo.GetAlphaLabel(i) + num3;
				x = (padXValues ? new double[y.GetLength(0)] : Engine.GetDoubleArray(arg));
				for (int j = num7; j <= i; j++)
				{
					if (j > 1 && i < num5)
					{
						i++;
						arg = RangeInfo.GetAlphaLabel(i) + num2 + ":" + RangeInfo.GetAlphaLabel(i) + num3;
						x = (padXValues ? new double[y.GetLength(0)] : Engine.GetDoubleArray(arg));
					}
					else if (i == num5)
					{
						continue;
					}
					for (int k = 0; k < y.GetLength(0); k++)
					{
						if (j == 0)
						{
							array[k, j] = 1.0;
						}
						else
						{
							array[k, j] = x[k];
						}
					}
				}
				if (i < num5)
				{
					num7 = i + 1;
					if (padXValues)
					{
						for (num = 0; num < x.Length - 1; num++)
						{
							x[num] = num + 1;
						}
					}
				}
				if (i == num5)
				{
					ComputeMultipleRegression(y, array, y.GetLength(0), num6 + 1, out b, out errorValue, out coefficients);
					if (errorValue != string.Empty)
					{
						return errorValue;
					}
					return b.ToString();
				}
			}
		}
		return b.ToString();
	}

	internal string ComputeXArithmetic(double[] y, double[] x, string arg, double b, double m, bool padXValues, string errorValue, out double[] coefficients)
	{
		string rangeValue = string.Empty;
		string logicalValue = string.Empty;
		SplitRange(arg, ref rangeValue, ref logicalValue);
		coefficients = null;
		if (rangeValue != string.Empty && logicalValue != string.Empty && logicalValue.IndexOfAny(Engine.tokens) > -1)
		{
			logicalValue = logicalValue.Replace('\u0092'.ToString(), string.Empty);
			string[] array = logicalValue.Split(new string[1] { TIC }, StringSplitOptions.RemoveEmptyEntries);
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			int num = -1;
			int num2;
			for (num2 = 0; num2 < array.Length; num2++)
			{
				list.Add(array[num2]);
				num++;
				if (num > 0 && list[num].Length != list[num - 1].Length)
				{
					if (Engine.RethrowLibraryComputationExceptions)
					{
						throw new ArgumentException(Engine.FormulaErrorStrings[Engine.bad_formula]);
					}
					errorValue = Engine.ErrorStrings[1].ToString();
				}
				num2++;
				list2.Add(array[num2]);
			}
			double[] array2 = null;
			for (int i = 0; i < list.Count; i++)
			{
				x = (padXValues ? new double[y.GetLength(0)] : Engine.GetDoubleArray(rangeValue));
				if (padXValues)
				{
					for (int j = 0; j < x.Length - 1; j++)
					{
						x[j] = j + 1;
					}
				}
				string[] array3 = Engine.SplitArgsPreservingQuotedCommas(list[i]);
				double[,] array4 = new double[y.GetLength(0), array3.Length + 1];
				int num3 = 0;
				for (int k = 0; k <= array3.Length - 1; k++)
				{
					if (array2 != null && k == array3.Length - 1)
					{
						x = array2;
					}
					else
					{
						x = (padXValues ? new double[y.GetLength(0)] : Engine.GetDoubleArray(rangeValue));
						if (padXValues)
						{
							for (int l = 0; l < x.Length - 1; l++)
							{
								x[l] = l + 1;
							}
						}
					}
					for (int n = 0; n <= x.Length - 1; n++)
					{
						if (x[n].ToString() != double.NaN.ToString() && y[n].ToString() != double.NaN.ToString())
						{
							x[n] = double.Parse(Engine.ComputedValue("n" + x[n] + "n" + array3[k] + list2[i]));
						}
					}
					if (k == array3.Length - 1 && list2.Count > 1)
					{
						array2 = x;
					}
					if (num3 <= array3.Length)
					{
						for (int num4 = num3; num4 <= k + 1; num4++)
						{
							for (int num5 = 0; num5 < y.GetLength(0); num5++)
							{
								if (num4 == 0)
								{
									array4[num5, num4] = 1.0;
								}
								else
								{
									array4[num5, num4] = x[num5];
								}
							}
						}
						num3 = k + 2;
					}
					if (k == array3.Length - 1 && i == list2.Count - 1)
					{
						ComputeMultipleRegression(y, array4, y.GetLength(0), array4.GetLength(1), out b, out errorValue, out coefficients);
						if (errorValue != string.Empty)
						{
							return errorValue;
						}
						return b.ToString();
					}
				}
			}
		}
		return b.ToString();
	}

	public void Dispose()
	{
		Engine = null;
	}
}
