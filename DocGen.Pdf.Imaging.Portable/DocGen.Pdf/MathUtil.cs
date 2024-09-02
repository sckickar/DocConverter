using System;

namespace DocGen.Pdf;

internal class MathUtil
{
	public static int log2(int x)
	{
		if (x <= 0)
		{
			throw new ArgumentException(x + " <= 0");
		}
		int num = x;
		int num2 = -1;
		while (num > 0)
		{
			num >>= 1;
			num2++;
		}
		return num2;
	}

	public static int lcm(int x1, int x2)
	{
		if (x1 <= 0 || x2 <= 0)
		{
			throw new ArgumentException("Cannot compute the least common multiple of two numbers if one, at least,is negative.");
		}
		int num;
		int num2;
		if (x1 > x2)
		{
			num = x1;
			num2 = x2;
		}
		else
		{
			num = x2;
			num2 = x1;
		}
		for (int i = 1; i <= num2; i++)
		{
			if (num * i % num2 == 0)
			{
				return i * num;
			}
		}
		return -1;
	}

	public static int lcm(int[] x)
	{
		_ = x.Length;
		_ = 2;
		int num = lcm(x[^1], x[^2]);
		for (int num2 = x.Length - 3; num2 >= 0; num2--)
		{
			if (x[num2] <= 0)
			{
				throw new ArgumentException("Cannot compute the least common multiple of several numbers where one, at least,is negative.");
			}
			num = lcm(num, x[num2]);
		}
		return num;
	}

	public static int gcd(int x1, int x2)
	{
		if (x1 < 0 || x2 < 0)
		{
			throw new ArgumentException("Cannot compute the GCD if one integer is negative.");
		}
		int num;
		int num2;
		if (x1 > x2)
		{
			num = x1;
			num2 = x2;
		}
		else
		{
			num = x2;
			num2 = x1;
		}
		if (num2 == 0)
		{
			return 0;
		}
		int num3 = num2;
		while (num3 != 0)
		{
			int num4 = num % num3;
			num = num3;
			num3 = num4;
		}
		return num;
	}

	public static int gcd(int[] x)
	{
		_ = x.Length;
		_ = 2;
		int num = gcd(x[^1], x[^2]);
		for (int num2 = x.Length - 3; num2 >= 0; num2--)
		{
			if (x[num2] < 0)
			{
				throw new ArgumentException("Cannot compute the least common multiple of several numbers where one, at least,is negative.");
			}
			num = gcd(num, x[num2]);
		}
		return num;
	}
}
