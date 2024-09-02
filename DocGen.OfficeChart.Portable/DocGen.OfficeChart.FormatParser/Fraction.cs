using System;
using System.Collections.Generic;

namespace DocGen.OfficeChart.FormatParser;

internal class Fraction
{
	private const int DEF_MAX_DIGITS = 9;

	private const double DEF_EPS = 1E-09;

	private double m_dNumerator;

	private double m_dDenumerator;

	public double Numerator
	{
		get
		{
			return m_dNumerator;
		}
		set
		{
			m_dNumerator = value;
		}
	}

	public double Denumerator
	{
		get
		{
			return m_dDenumerator;
		}
		set
		{
			m_dDenumerator = value;
		}
	}

	public int DenumeratorLen => (int)Math.Log10(Denumerator) + 1;

	public Fraction(double dNumerator, double dDenumerator)
	{
		if (dDenumerator == 0.0)
		{
			throw new ArgumentOutOfRangeException("dDenumerator");
		}
		m_dNumerator = dNumerator;
		m_dDenumerator = dDenumerator;
	}

	public Fraction(double dNumerator)
		: this(dNumerator, 1.0)
	{
	}

	public static Fraction operator +(Fraction term1, Fraction term2)
	{
		double num = term1.Numerator * term2.Denumerator + term1.Denumerator * term2.Numerator;
		double num2 = term2.Denumerator * term1.Denumerator;
		double maximumCommonDevisor = GetMaximumCommonDevisor(num, num2);
		double dNumerator = num / maximumCommonDevisor;
		num2 /= maximumCommonDevisor;
		return new Fraction(dNumerator, num2);
	}

	public static explicit operator double(Fraction fraction)
	{
		return fraction.Numerator / fraction.Denumerator;
	}

	public static explicit operator Fraction(List<double> arrFraction)
	{
		if (arrFraction == null)
		{
			throw new ArgumentNullException("arrFraction");
		}
		int count = arrFraction.Count;
		Fraction fraction = null;
		if (count > 0)
		{
			double dNumerator = arrFraction[count - 1];
			fraction = new Fraction(dNumerator, 1.0);
			for (int num = count - 2; num >= 0; num--)
			{
				dNumerator = arrFraction[num];
				fraction = fraction.Reverse() + (Fraction)dNumerator;
			}
		}
		return fraction;
	}

	public static explicit operator Fraction(double dValue)
	{
		return new Fraction(dValue);
	}

	public Fraction Reverse()
	{
		double dNumerator = m_dNumerator;
		m_dNumerator = m_dDenumerator;
		m_dDenumerator = dNumerator;
		return this;
	}

	public static Fraction ConvertToFraction(double value, int iDigitsNumber)
	{
		if (iDigitsNumber < 1)
		{
			throw new ArgumentOutOfRangeException("iDigitsNumber");
		}
		iDigitsNumber = Math.Min(iDigitsNumber, 9);
		List<double> list = new List<double>();
		double dLeft = value;
		dLeft = AddNextNumber(list, dLeft);
		Fraction fraction = (Fraction)list;
		double num = GetDelta(fraction, value);
		Fraction fraction2 = fraction;
		while (Math.Abs(dLeft) > 1E-09)
		{
			dLeft = AddNextNumber(list, dLeft);
			fraction2 = (Fraction)list;
			if (fraction2.DenumeratorLen > iDigitsNumber)
			{
				break;
			}
			double delta = GetDelta(fraction2, value);
			if (delta < num)
			{
				fraction = fraction2;
				num = delta;
			}
		}
		return fraction;
	}

	private static double GetMaximumCommonDevisor(double dNumerator, double dDenumerator)
	{
		double num = Math.Round(Math.Max(dNumerator, dDenumerator));
		double num2 = Math.Round(Math.Min(dNumerator, dDenumerator));
		double num3 = num % num2;
		if (num2 == 0.0)
		{
			return 1.0;
		}
		while (num3 != 0.0)
		{
			double num4 = num2;
			num2 = num3;
			num3 = Math.Round(num4 % num2);
		}
		return num2;
	}

	private static double GetDelta(Fraction fraction, double value)
	{
		return Math.Abs((double)fraction - value);
	}

	private static double AddNextNumber(List<double> arrFraction, double dLeft)
	{
		if (Math.Abs(dLeft) < 1E-09)
		{
			return 0.0;
		}
		double num;
		if (arrFraction.Count != 0)
		{
			num = 1.0 / dLeft;
			dLeft = num;
		}
		num = Math.Floor(dLeft);
		arrFraction.Add(num);
		return dLeft - num;
	}

	public override string ToString()
	{
		return m_dNumerator + " / " + m_dDenumerator;
	}
}
