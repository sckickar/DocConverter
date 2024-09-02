using System;

namespace DocGen.Chart.Statistics;

internal static class UtilityFunctions
{
	private static double[] gammaCoefs = new double[6] { 76.18009172947146, -86.50532032941678, 24.01409824083091, -1.231739572450155, 0.001208650973866179, -5.395239384953E-06 };

	private static double gamma = 5.0;

	private static double sqrt2Pi = Math.Sqrt(Math.PI * 2.0);

	private static double epsilon_GammaSer = 1E-07;

	private static int max_ItGammaSer = 1000;

	private static double tiny_gamma = 1E-77;

	private static double epsilon_BetaSer = 1E-16;

	private static double epsilon_BetaInverse = epsilon_BetaSer;

	private static int max_ItBetaSer = 1000;

	private static int max_ItBetaBrent = 1000;

	private static double tiny_beta = 1E-99;

	private static double[] factrl = new double[100]
	{
		1.0, 1.0, 2.0, 6.0, 24.0, 120.0, 720.0, 5040.0, 40320.0, 362880.0,
		3628800.0, 39916800.0, 479001600.0, 6227020800.0, 87178291200.0, 1307674368000.0, 20922789888000.0, 355687428096000.0, 6402373705728000.0, 1.21645100408832E+17,
		2.43290200817664E+18, 5.10909421717094E+19, 1.12400072777761E+21, 2.5852016738885E+22, 6.20448401733239E+23, 1.5511210043331E+25, 4.03291461126606E+26, 1.08888694504184E+28, 3.04888344611714E+29, 8.8417619937397E+30,
		2.65252859812191E+32, 8.22283865417792E+33, 2.63130836933694E+35, 8.68331761881189E+36, 2.95232799039604E+38, 1.03331479663861E+40, 3.71993326789901E+41, 1.37637530912263E+43, 5.23022617466601E+44, 2.03978820811974E+46,
		8.15915283247898E+47, 3.34525266131638E+49, 1.40500611775288E+51, 6.04152630633738E+52, 2.65827157478845E+54, 1.1962222086548E+56, 5.50262215981209E+57, 2.58623241511168E+59, 1.24139155925361E+61, 6.08281864034268E+62,
		3.04140932017134E+64, 1.55111875328738E+66, 8.06581751709439E+67, 4.27488328406003E+69, 2.30843697339241E+71, 1.26964033536583E+73, 7.10998587804863E+74, 4.05269195048772E+76, 2.35056133128288E+78, 1.3868311854569E+80,
		8.32098711274139E+81, 5.07580213877225E+83, 3.14699732603879E+85, 1.98260831540444E+87, 1.26886932185884E+89, 8.24765059208247E+90, 5.44344939077443E+92, 3.64711109181887E+94, 2.48003554243683E+96, 1.71122452428141E+98,
		1.19785716699699E+100, 8.50478588567862E+101, 6.12344583768861E+103, 4.47011546151268E+105, 3.30788544151939E+107, 2.48091408113954E+109, 1.88549470166605E+111, 1.45183092028286E+113, 1.13242811782063E+115, 8.94618213078297E+116,
		7.15694570462638E+118, 5.79712602074737E+120, 4.75364333701284E+122, 3.94552396972066E+124, 3.31424013456535E+126, 2.81710411438055E+128, 2.42270953836727E+130, 2.10775729837953E+132, 1.85482642257398E+134, 1.65079551609085E+136,
		1.48571596448176E+138, 1.3520015276784E+140, 1.24384140546413E+142, 1.15677250708164E+144, 1.08736615665674E+146, 1.03299784882391E+148, 9.91677934870949E+149, 9.61927596824821E+151, 9.42689044888324E+153, 9.33262154439441E+155
	};

	private static double[] factrlLn = new double[100]
	{
		0.0, 0.0, 0.693147180559945, 1.79175946922805, 3.17805383034795, 4.78749174278205, 6.5792512120101, 8.52516136106541, 10.6046029027453, 12.8018274800815,
		15.1044125730755, 17.5023078458739, 19.9872144956619, 22.5521638531234, 25.1912211827387, 27.8992713838409, 30.6718601060807, 33.5050734501369, 36.3954452080331, 39.3398841871995,
		42.3356164607535, 45.3801388984769, 48.4711813518352, 51.6066755677644, 54.7847293981123, 58.0036052229805, 61.261701761002, 64.5575386270063, 67.8897431371815, 71.257038967168,
		74.6582363488302, 78.0922235533153, 81.557959456115, 85.0544670175815, 88.5808275421977, 92.1361756036871, 95.7196945421432, 99.3306124547874, 102.968198614514, 106.631760260643,
		110.320639714757, 114.034211781462, 117.771881399745, 121.533081515439, 125.317271149357, 129.123933639127, 132.952575035616, 136.802722637326, 140.673923648234, 144.565743946345,
		148.477766951773, 152.409592584497, 156.360836303079, 160.331128216631, 164.320112263195, 168.327445448428, 172.352797139163, 176.395848406997, 180.456291417544, 184.53382886145,
		188.628173423672, 192.739047287845, 196.86618167289, 201.009316399282, 205.168199482641, 209.342586752537, 213.532241494563, 217.736934113954, 221.95644181913, 226.190548323728,
		230.439043565777, 234.701723442818, 238.978389561834, 243.268849002983, 247.572914096187, 251.890402209723, 256.22113555001, 260.564940971863, 264.921649798553, 269.29109765102,
		273.673124285694, 278.067573440366, 282.47429268763, 286.893133295427, 291.32395009427, 295.766601350761, 300.220948647014, 304.686856765669, 309.164193580147, 313.652829949879,
		318.152639620209, 322.663499126726, 327.185287703775, 331.717887196929, 336.261181979199, 340.815058870799, 345.379407062267, 349.95411804077, 354.539085519441, 359.134205369576
	};

	private static double[] a_inv_norm = new double[6] { -39.69683028665376, 220.9460984245205, -275.9285104469687, 138.357751867269, -30.66479806614716, 2.506628277459239 };

	private static double[] b_inv_norm = new double[5] { -54.47609879822406, 161.5858368580409, -155.6989798598866, 66.80131188771972, -13.28068155288572 };

	private static double[] c_inv_norm = new double[6] { -0.007784894002430293, -0.3223964580411365, -2.400758277161838, -2.549732539343734, 4.374664141464968, 2.938163982698783 };

	private static double[] d_inv_norm = new double[4] { 0.007784695709041462, 0.3224671290700398, 2.445134137142996, 3.754408661907416 };

	private static double p_low_inv_norm = 0.02425;

	private static double p_high_inv_norm = 1.0 - p_low_inv_norm;

	public static double GammaLn(double y)
	{
		double num = y + gamma + 0.5;
		double num2 = y;
		double num3 = 1.000000000190015;
		for (int i = 0; i < gammaCoefs.Length; i++)
		{
			num3 += gammaCoefs[i] / (num2 += 1.0);
		}
		return (y + 0.5) * Math.Log(num) - num + Math.Log(sqrt2Pi * num3 / y);
	}

	public static double Gamma(double y)
	{
		return Math.Exp(GammaLn(y));
	}

	public static double Factorial(int n)
	{
		if (n < factrl.Length)
		{
			return factrl[n];
		}
		return Math.Exp(GammaLn(n + 1));
	}

	public static double FactorialLn(int n)
	{
		if (n < factrlLn.Length)
		{
			return factrlLn[n];
		}
		return GammaLn(n + 1);
	}

	public static double Binomial(int n, int k)
	{
		return Math.Floor(0.5 + Math.Exp(FactorialLn(n) - FactorialLn(k) - FactorialLn(n - k)));
	}

	public static double BetaLn(double a, double b)
	{
		return GammaLn(a) + GammaLn(b) - GammaLn(a + b);
	}

	public static double Beta(double a, double b)
	{
		return Math.Exp(BetaLn(a, b));
	}

	public static double NormalDistributionDensity(double x, double m, double sigma)
	{
		if (sigma <= 0.0)
		{
			throw new ArgumentException("Standart deviation sigma in NormalDistributionDensity should be sigma > 0.", "sigma - standart deviation.");
		}
		return Math.Exp(NormalDistributionDensityLn(x, m, sigma));
	}

	public static double NormalDistributionDensityLn(double x, double m, double sigma)
	{
		if (sigma <= 0.0)
		{
			throw new ArgumentException("Standart deviation sigma in NormalDistributionDensityLn should be sigma > 0.", "sigma - standart deviation.");
		}
		return (0.0 - (x - m)) * (x - m) / (2.0 * sigma * sigma) - Math.Log(sigma * Math.Sqrt(Math.PI * 2.0));
	}

	public static double Erf(double x)
	{
		if (!(x < 0.0))
		{
			return GammaCumulativeDistribution(0.5, x * x);
		}
		return 0.0 - GammaCumulativeDistribution(0.5, x * x);
	}

	public static double InverseNormalDistribution(double p)
	{
		if (p <= 0.0 && p >= 1.0)
		{
			throw new ArgumentException("Probaility p in InverseNormalDistribution should be in range (0,1).", "p - probability.");
		}
		double num;
		if (0.0 < p && p < p_low_inv_norm)
		{
			num = Math.Sqrt(-2.0 * Math.Log(p));
			return (((((c_inv_norm[0] * num + c_inv_norm[1]) * num + c_inv_norm[2]) * num + c_inv_norm[3]) * num + c_inv_norm[4]) * num + c_inv_norm[5]) / ((((d_inv_norm[0] * num + d_inv_norm[1]) * num + d_inv_norm[2]) * num + d_inv_norm[3]) * num + 1.0);
		}
		if (p_low_inv_norm <= p && p <= p_high_inv_norm)
		{
			num = p - 0.5;
			double num2 = num * num;
			return (((((a_inv_norm[0] * num2 + a_inv_norm[1]) * num2 + a_inv_norm[2]) * num2 + a_inv_norm[3]) * num2 + a_inv_norm[4]) * num2 + a_inv_norm[5]) * num / (((((b_inv_norm[0] * num2 + b_inv_norm[1]) * num2 + b_inv_norm[2]) * num2 + b_inv_norm[3]) * num2 + b_inv_norm[4]) * num2 + 1.0);
		}
		num = Math.Sqrt(-2.0 * Math.Log(1.0 - p));
		return (0.0 - (((((c_inv_norm[0] * num + c_inv_norm[1]) * num + c_inv_norm[2]) * num + c_inv_norm[3]) * num + c_inv_norm[4]) * num + c_inv_norm[5])) / ((((d_inv_norm[0] * num + d_inv_norm[1]) * num + d_inv_norm[2]) * num + d_inv_norm[3]) * num + 1.0);
	}

	public static double NormalDistribution(double x)
	{
		return 0.5 * (1.0 + Erf(1.4142135623730951 * x));
	}

	public static double InverseErf(double x)
	{
		return 0.7071067811865475 * InverseNormalDistribution(0.5 * (x + 1.0));
	}

	public static double GammaCumulativeDistribution(double a, double x)
	{
		if (a <= 0.0)
		{
			throw new ArgumentException("Parameter a in GammaCumulativeDistribution should be a > 0.", "a");
		}
		if (x < 0.0)
		{
			throw new ArgumentException("Parameter x in GammaCumulativeDistribution should be x >= 0.", "x");
		}
		if (x < a + 1.0)
		{
			return GammaCumulativeS(a, x);
		}
		return 1.0 - GammaCumulativeCF(a, x);
	}

	public static double BetaCumulativeDistribution(double a, double b, double x)
	{
		if (x < 0.0 || x > 1.0)
		{
			throw new ArgumentException("Parameter x in BetaCumulativeDistribution should be in range [0,1].", "x");
		}
		if (a <= 0.0)
		{
			throw new ArgumentException("Parameter a in BetaCumulativeDistribution should be a > 0.", "a");
		}
		if (b <= 0.0)
		{
			throw new ArgumentException("Parameter b in BetaCumulativeDistribution should be b > 0.", "b");
		}
		if (x == 1.0)
		{
			return 1.0;
		}
		if (x == 0.0)
		{
			return 0.0;
		}
		double num = (a + 1.0) / (a + b + 2.0);
		double num2 = Math.Exp(0.0 - BetaLn(a, b) - Math.Log(a) - Math.Log(b) + a * Math.Log(x) + b * Math.Log(1.0 - x));
		if (x < num)
		{
			return b * num2 * BtaCumulativeCF(a, b, x);
		}
		return 1.0 - a * num2 * BtaCumulativeCF(b, a, 1.0 - x);
	}

	public static double InverseBetaCumulativeDistribution(double a, double b, double p)
	{
		if (p < 0.0 && p > 1.0)
		{
			throw new ArgumentException("Probaility p in InverseBetaCumulativeDistribution should be in range [0,1].", "p - probability.");
		}
		if (a <= 0.0)
		{
			throw new ArgumentException("Parameter a in InverseBetaCumulativeDistribution should be a > 0.", "a");
		}
		if (b <= 0.0)
		{
			throw new ArgumentException("Parameter b in InverseBetaCumulativeDistribution should be b > 0.", "b");
		}
		if (p == 1.0)
		{
			return 1.0;
		}
		if (p == 0.0)
		{
			return 0.0;
		}
		return InverseBtaCumulativeBrent(a, b, p, 0.0, 1.0, epsilon_BetaInverse);
	}

	public static double TCumulativeDistribution(double tValue, double degreeOfFreedom, bool oneTail)
	{
		if (degreeOfFreedom <= 0.0)
		{
			throw new ArgumentException("Parameter degreeOfFreedom in TCumulativeDistribution should be degreeOfFreedom > 0.", "degreeOfFreedom");
		}
		double x = 1.0 / (1.0 + tValue * tValue / degreeOfFreedom);
		if (oneTail)
		{
			if (tValue > 0.0)
			{
				return 1.0 - 0.5 * BetaCumulativeDistribution(degreeOfFreedom / 2.0, 0.5, x);
			}
			return 0.5 * BetaCumulativeDistribution(degreeOfFreedom / 2.0, 0.5, x);
		}
		return 1.0 - BetaCumulativeDistribution(degreeOfFreedom / 2.0, 0.5, x);
	}

	public static double InverseTCumulativeDistribution(double p, double degreeOfFreedom, bool oneTail)
	{
		if (degreeOfFreedom <= 0.0)
		{
			throw new ArgumentException("Parameter degreeOfFreedom in InverseTCumulativeDistribution should be degreeOfFreedom > 0.", "degreeOfFreedom");
		}
		if (p <= 0.0 && p >= 1.0)
		{
			throw new ArgumentException("Probaility p in InverseTCumulativeDistribution should be in range (0,1).", "p - probability.");
		}
		double num = 0.0;
		if (oneTail)
		{
			if (p > 0.5)
			{
				num = InverseBetaCumulativeDistribution(degreeOfFreedom / 2.0, 0.5, 2.0 * (1.0 - p));
				return Math.Sqrt(degreeOfFreedom * (1.0 / num - 1.0));
			}
			num = InverseBetaCumulativeDistribution(degreeOfFreedom / 2.0, 0.5, 2.0 * p);
			return 0.0 - Math.Sqrt(degreeOfFreedom * (1.0 / num - 1.0));
		}
		num = InverseBetaCumulativeDistribution(degreeOfFreedom / 2.0, 0.5, 1.0 - p);
		return Math.Sqrt(degreeOfFreedom * (1.0 / num - 1.0));
	}

	public static double FCumulativeDistribution(double fValue, double firstDegreeOfFreedom, double secondDegreeOfFreedom)
	{
		if (firstDegreeOfFreedom <= 0.0)
		{
			throw new ArgumentException("Parameter firstDegreeOfFreedom in FCumulativeDistribution should be firstDegreeOfFreedom > 0.", "firstDegreeOfFreedom");
		}
		if (secondDegreeOfFreedom <= 0.0)
		{
			throw new ArgumentException("Parameter secondDegreeOfFreedom in FCumulativeDistribution should be secondDegreeOfFreedom > 0.", "secondDegreeOfFreedom");
		}
		if (fValue < 0.0)
		{
			throw new ArgumentException("Probaility fValue in FCumulativeDistribution should be in range [0,infinity).", "fValue");
		}
		if (fValue == 0.0)
		{
			return 1.0;
		}
		double x = secondDegreeOfFreedom / (secondDegreeOfFreedom + fValue * firstDegreeOfFreedom);
		return BetaCumulativeDistribution(secondDegreeOfFreedom / 2.0, firstDegreeOfFreedom / 2.0, x);
	}

	public static double InverseFCumulativeDistribution(double p, double firstDegreeOfFreedom, double secondDegreeOfFreedom)
	{
		if (firstDegreeOfFreedom <= 0.0)
		{
			throw new ArgumentException("Parameter firstDegreeOfFreedom in InverseFCumulativeDistribution should be firstDegreeOfFreedom > 0.", "firstDegreeOfFreedom");
		}
		if (secondDegreeOfFreedom <= 0.0)
		{
			throw new ArgumentException("Parameter secondDegreeOfFreedom in InverseFCumulativeDistribution should be secondDegreeOfFreedom > 0.", "secondDegreeOfFreedom");
		}
		if (p <= 0.0 && p > 1.0)
		{
			throw new ArgumentException("Probaility p in InverseFCumulativeDistribution should be in range (0,1].", "p - probability.");
		}
		if (p == 1.0)
		{
			return 0.0;
		}
		if (p == 0.0)
		{
			return double.PositiveInfinity;
		}
		double num = InverseBetaCumulativeDistribution(secondDegreeOfFreedom / 2.0, firstDegreeOfFreedom / 2.0, p);
		return secondDegreeOfFreedom / firstDegreeOfFreedom * (1.0 / num - 1.0);
	}

	private static double GammaCumulativeS(double a, double x)
	{
		if (x < 0.0)
		{
			return 0.0;
		}
		double num = 1.0 / a;
		double num2 = a;
		double num3 = num;
		for (int i = 1; i <= max_ItGammaSer; i++)
		{
			if (!(Math.Abs(num * epsilon_GammaSer) < Math.Abs(num3)))
			{
				break;
			}
			num2 += 1.0;
			num3 *= x / num2;
			num += num3;
		}
		return num * Math.Exp(0.0 - x + a * Math.Log(x) - GammaLn(a));
	}

	private static double GammaCumulativeCF(double a, double x)
	{
		if (x < 0.0)
		{
			return 0.0;
		}
		double num = x + 1.0 - a;
		double num2 = tiny_gamma;
		double num3 = num2;
		double num4 = 0.0;
		double num5 = 1.0;
		for (int i = 1; i <= max_ItGammaSer; i++)
		{
			num4 = num + num5 * num4;
			if (Math.Abs(num4) < tiny_gamma)
			{
				num4 = tiny_gamma;
			}
			num3 = num + num5 / num3;
			if (Math.Abs(num3) < tiny_gamma)
			{
				num3 = tiny_gamma;
			}
			num4 = 1.0 / num4;
			double num6 = num3 * num4;
			num2 *= num6;
			if (Math.Abs(num6 - 1.0) < epsilon_GammaSer)
			{
				break;
			}
			num += 2.0;
			num5 = (double)(-i) * ((double)i - a);
		}
		return num2 * Math.Exp(0.0 - x + a * Math.Log(x) - GammaLn(a));
	}

	private static double BtaCumulativeCF(double a, double b, double x)
	{
		if (x < 0.0 || x > 1.0)
		{
			return 0.0;
		}
		double num = 1.0;
		double num2 = tiny_beta;
		double num3 = num2;
		double num4 = 0.0;
		double num5 = 1.0;
		for (int i = 1; i <= max_ItBetaSer; i++)
		{
			num4 = num + num5 * num4;
			if (Math.Abs(num4) < tiny_beta)
			{
				num4 = tiny_beta;
			}
			num3 = num + num5 / num3;
			if (Math.Abs(num3) < tiny_beta)
			{
				num3 = tiny_beta;
			}
			num4 = 1.0 / num4;
			double num6 = num3 * num4;
			num2 *= num6;
			if (Math.Abs(num6 - 1.0) < epsilon_BetaSer)
			{
				break;
			}
			int num7 = i / 2;
			double num8 = a + (double)num7;
			double num9 = num8 + (double)num7;
			num5 = ((i % 2 != 1) ? ((double)num7 * (b - (double)num7) * x / (num9 * (num9 - 1.0))) : ((0.0 - num8) * (num8 + b) * x / (num9 * (num9 + 1.0))));
		}
		return num2;
	}

	private static double InverseBtaCumulativeBrent(double aa, double bb, double prblty, double x1, double x2, double tol)
	{
		double num = x1;
		double num2 = x2;
		double num3 = x2;
		double num4 = 0.0;
		double num5 = 0.0;
		double num6 = BetaCumulativeDistribution(aa, bb, num) - prblty;
		double num7 = BetaCumulativeDistribution(aa, bb, num2) - prblty;
		double num8 = num7;
		for (int i = 1; i <= max_ItBetaBrent; i++)
		{
			if ((num7 > 0.0 && num8 > 0.0) || (num7 < 0.0 && num8 < 0.0))
			{
				num3 = num;
				num8 = num6;
				num5 = (num4 = num2 - num);
			}
			if (Math.Abs(num8) < Math.Abs(num7))
			{
				num = num2;
				num2 = num3;
				num3 = num;
				num6 = num7;
				num7 = num8;
				num8 = num6;
			}
			double num9 = 2.0 * epsilon_BetaSer * Math.Abs(num2) + 0.5 * tol;
			double num10 = 0.5 * (num3 - num2);
			if (Math.Abs(num10) <= num9 || num7 == 0.0)
			{
				return num2;
			}
			if (Math.Abs(num5) >= num9 && Math.Abs(num6) > Math.Abs(num7))
			{
				double num11 = num7 / num6;
				double num13;
				double num12;
				if (num == num3)
				{
					num12 = 2.0 * num10 * num11;
					num13 = 1.0 - num11;
				}
				else
				{
					num13 = num6 / num8;
					double num14 = num7 / num8;
					num12 = num11 * (2.0 * num10 * num13 * (num13 - num14) - (num2 - num) * (num14 - 1.0));
					num13 = (num13 - 1.0) * (num14 - 1.0) * (num11 - 1.0);
				}
				if (num12 > 0.0)
				{
					num13 = 0.0 - num13;
				}
				num12 = Math.Abs(num12);
				double num15 = 3.0 * num10 * num13 - Math.Abs(num9 * num13);
				double num16 = Math.Abs(num5 * num13);
				if (2.0 * num12 < ((num15 < num16) ? num15 : num16))
				{
					num5 = num4;
					num4 = num12 / num13;
				}
				else
				{
					num4 = num10;
					num5 = num4;
				}
			}
			else
			{
				num4 = num10;
				num5 = num4;
			}
			num = num2;
			num6 = num7;
			num2 += num4;
			num7 = BetaCumulativeDistribution(aa, bb, num2) - prblty;
		}
		return 0.0;
	}
}
