using System;

namespace DocGen.Pdf.Security;

internal class ECTanFunction
{
	private static readonly Number degOne = Number.One.Negate();

	private static readonly Number degTwo = Number.Two.Negate();

	private static readonly Number degThree = Number.Three.Negate();

	private static readonly Number mFour = Number.ValueOf(4L);

	public const sbyte WidthValue = 4;

	public const sbyte Power2Width = 16;

	public static readonly ITanuElement[] AlphaZeo = new ITanuElement[9]
	{
		null,
		new ITanuElement(Number.One, Number.Zero),
		null,
		new ITanuElement(degThree, degOne),
		null,
		new ITanuElement(degOne, degOne),
		null,
		new ITanuElement(Number.One, degOne),
		null
	};

	public static readonly sbyte[][] AlphaZeroT = new sbyte[8][]
	{
		null,
		new sbyte[1] { 1 },
		null,
		new sbyte[3] { -1, 0, 1 },
		null,
		new sbyte[3] { 1, 0, 1 },
		null,
		new sbyte[4] { -1, 0, 0, 1 }
	};

	public static readonly ITanuElement[] AlphaOne = new ITanuElement[9]
	{
		null,
		new ITanuElement(Number.One, Number.Zero),
		null,
		new ITanuElement(degThree, Number.One),
		null,
		new ITanuElement(degOne, Number.One),
		null,
		new ITanuElement(Number.One, Number.One),
		null
	};

	public static readonly sbyte[][] AlphaOneT = new sbyte[8][]
	{
		null,
		new sbyte[1] { 1 },
		null,
		new sbyte[3] { -1, 0, 1 },
		null,
		new sbyte[3] { 1, 0, 1 },
		null,
		new sbyte[4] { -1, 0, 0, -1 }
	};

	public static Number Norm(sbyte byteMu, ITanuElement lambdaValue)
	{
		Number number = lambdaValue.num1.Multiply(lambdaValue.num1);
		Number number2 = lambdaValue.num1.Multiply(lambdaValue.num2);
		Number value = lambdaValue.num2.Multiply(lambdaValue.num2).ShiftLeft(1);
		return byteMu switch
		{
			1 => number.Add(number2).Add(value), 
			-1 => number.Subtract(number2).Add(value), 
			_ => throw new ArgumentException("byteMu must be 1 or -1"), 
		};
	}

	public static ITanuElement Round(LargeDecimal lambda0, LargeDecimal lambda1, sbyte byteMu)
	{
		int scale = lambda0.Scale;
		if (lambda1.Scale != scale)
		{
			throw new ArgumentException("lambda0 and lambda1 do not have same scale");
		}
		if (byteMu != 1 && byteMu != -1)
		{
			throw new ArgumentException("byteMu must be 1 or -1");
		}
		Number number = lambda0.Round();
		Number number2 = lambda1.Round();
		LargeDecimal largeDecimal = lambda0.Subtract(number);
		LargeDecimal largeDecimal2 = lambda1.Subtract(number2);
		LargeDecimal largeDecimal3 = largeDecimal.Add(largeDecimal);
		largeDecimal3 = ((byteMu != 1) ? largeDecimal3.Subtract(largeDecimal2) : largeDecimal3.Add(largeDecimal2));
		LargeDecimal largeDecimal4 = largeDecimal2.Add(largeDecimal2).Add(largeDecimal2);
		LargeDecimal value = largeDecimal4.Add(largeDecimal2);
		LargeDecimal largeDecimal5;
		LargeDecimal largeDecimal6;
		if (byteMu == 1)
		{
			largeDecimal5 = largeDecimal.Subtract(largeDecimal4);
			largeDecimal6 = largeDecimal.Add(value);
		}
		else
		{
			largeDecimal5 = largeDecimal.Add(largeDecimal4);
			largeDecimal6 = largeDecimal.Subtract(value);
		}
		sbyte b = 0;
		sbyte b2 = 0;
		if (largeDecimal3.CompareTo(Number.One) >= 0)
		{
			if (largeDecimal5.CompareTo(degOne) < 0)
			{
				b2 = byteMu;
			}
			else
			{
				b = 1;
			}
		}
		else if (largeDecimal6.CompareTo(Number.Two) >= 0)
		{
			b2 = byteMu;
		}
		if (largeDecimal3.CompareTo(degOne) < 0)
		{
			if (largeDecimal5.CompareTo(Number.One) >= 0)
			{
				b2 = (sbyte)(-byteMu);
			}
			else
			{
				b = -1;
			}
		}
		else if (largeDecimal6.CompareTo(degTwo) < 0)
		{
			b2 = (sbyte)(-byteMu);
		}
		Number num = number.Add(Number.ValueOf(b));
		Number num2 = number2.Add(Number.ValueOf(b2));
		return new ITanuElement(num, num2);
	}

	public static LargeDecimal DivideByN(Number numberA, Number numberS, Number numberVM, sbyte a, int m, int c)
	{
		int num = (m + 5) / 2 + c;
		Number val = numberA.ShiftRight(m - num - 2 + a);
		Number number = numberS.Multiply(val);
		Number val2 = number.ShiftRight(m);
		Number value = numberVM.Multiply(val2);
		Number number2 = number.Add(value);
		Number number3 = number2.ShiftRight(num - c);
		if (number2.TestBit(num - c - 1))
		{
			number3 = number3.Add(Number.One);
		}
		return new LargeDecimal(number3, c);
	}

	public static Finite2MPoint GetTanU(Finite2MPoint pointP)
	{
		if (pointP.IsInfinity)
		{
			return pointP;
		}
		EllipticCurveElements pointX = pointP.PointX;
		EllipticCurveElements pointY = pointP.PointY;
		return new Finite2MPoint(pointP.Curve, pointX.Square(), pointY.Square(), pointP.IsCompressed);
	}

	public static sbyte FindMU(Field2MCurves curve)
	{
		Number number = curve.ElementA.ToIntValue();
		if (number.SignValue == 0)
		{
			return -1;
		}
		if (number.Equals(Number.One))
		{
			return 1;
		}
		throw new ArgumentException("multiplication not possible");
	}

	public static Number[] TraceLC(sbyte byteMu, int numberA, bool div)
	{
		if (byteMu != 1 && byteMu != -1)
		{
			throw new ArgumentException("byteMu must be 1 or -1");
		}
		Number number;
		Number number2;
		if (div)
		{
			number = Number.Two;
			number2 = Number.ValueOf(byteMu);
		}
		else
		{
			number = Number.Zero;
			number2 = Number.One;
		}
		for (int i = 1; i < numberA; i++)
		{
			Number number3 = null;
			number3 = ((byteMu != 1) ? number2.Negate() : number2);
			Number number4 = number3.Subtract(number.ShiftLeft(1));
			number = number2;
			number2 = number4;
		}
		return new Number[2] { number, number2 };
	}

	public static Number FindTW(sbyte byteMu, int w)
	{
		if (w == 4)
		{
			if (byteMu == 1)
			{
				return Number.ValueOf(6L);
			}
			return Number.ValueOf(10L);
		}
		Number[] array = TraceLC(byteMu, w, div: false);
		Number m = Number.Zero.SetBit(w);
		Number val = array[1].ModInverse(m);
		return Number.Two.Multiply(array[0]).Multiply(val).Mod(m);
	}

	public static Number[] FindSI(Field2MCurves curve)
	{
		if (!curve.IsKOBLITZ)
		{
			throw new ArgumentException("si is defined for Koblitz curves only");
		}
		int pointM = curve.PointM;
		int intValue = curve.ElementA.ToIntValue().IntValue;
		sbyte b = curve.MU();
		int intValue2 = curve.NumberY.IntValue;
		int numberA = pointM + 3 - intValue;
		Number[] array = TraceLC(b, numberA, div: false);
		Number number;
		Number number2;
		switch (b)
		{
		case 1:
			number = Number.One.Subtract(array[1]);
			number2 = Number.One.Subtract(array[0]);
			break;
		case -1:
			number = Number.One.Add(array[1]);
			number2 = Number.One.Add(array[0]);
			break;
		default:
			throw new ArgumentException("byteMu must be 1 or -1");
		}
		Number[] array2 = new Number[2];
		switch (intValue2)
		{
		case 2:
			array2[0] = number.ShiftRight(1);
			array2[1] = number2.ShiftRight(1).Negate();
			break;
		case 4:
			array2[0] = number.ShiftRight(2);
			array2[1] = number2.ShiftRight(2).Negate();
			break;
		default:
			throw new ArgumentException("Cofactor");
		}
		return array2;
	}

	public static ITanuElement MODFun(Number numberA, int m, sbyte a, Number[] numberS, sbyte byteMu, sbyte c)
	{
		Number number = ((byteMu != 1) ? numberS[0].Subtract(numberS[1]) : numberS[0].Add(numberS[1]));
		Number numberVM = TraceLC(byteMu, m, div: true)[1];
		LargeDecimal lambda = DivideByN(numberA, numberS[0], numberVM, a, m, c);
		LargeDecimal lambda2 = DivideByN(numberA, numberS[1], numberVM, a, m, c);
		ITanuElement tanuElement = Round(lambda, lambda2, byteMu);
		Number num = numberA.Subtract(number.Multiply(tanuElement.num1)).Subtract(Number.ValueOf(2L).Multiply(numberS[1]).Multiply(tanuElement.num2));
		Number num2 = numberS[1].Multiply(tanuElement.num1).Subtract(numberS[0].Multiply(tanuElement.num2));
		return new ITanuElement(num, num2);
	}

	public static Finite2MPoint MultiplyFromTnaf(Finite2MPoint pointP, sbyte[] u)
	{
		Finite2MPoint finite2MPoint = (Finite2MPoint)((Field2MCurves)pointP.Curve).IsInfinity;
		for (int num = u.Length - 1; num >= 0; num--)
		{
			finite2MPoint = GetTanU(finite2MPoint);
			if (u[num] == 1)
			{
				finite2MPoint = finite2MPoint.AddSimple(pointP);
			}
			else if (u[num] == -1)
			{
				finite2MPoint = finite2MPoint.SubtractSimple(pointP);
			}
		}
		return finite2MPoint;
	}

	public static sbyte[] GetTAdic(sbyte byteMu, ITanuElement lambdaValue, sbyte width, Number pow2w, Number tw, ITanuElement[] alpha)
	{
		if (byteMu != 1 && byteMu != -1)
		{
			throw new ArgumentException("byteMu must be 1 or -1");
		}
		int bitLength = Norm(byteMu, lambdaValue).BitLength;
		sbyte[] array = new sbyte[(bitLength > 30) ? (bitLength + 4 + width) : (34 + width)];
		Number value = pow2w.ShiftRight(1);
		Number number = lambdaValue.num1;
		Number number2 = lambdaValue.num2;
		int num = 0;
		while (!number.Equals(Number.Zero) || !number2.Equals(Number.Zero))
		{
			if (number.TestBit(0))
			{
				Number number3 = number.Add(number2.Multiply(tw)).Mod(pow2w);
				sbyte b = (array[num] = ((number3.CompareTo(value) < 0) ? ((sbyte)number3.IntValue) : ((sbyte)number3.Subtract(pow2w).IntValue)));
				bool flag = true;
				if (b < 0)
				{
					flag = false;
					b = (sbyte)(-b);
				}
				if (flag)
				{
					number = number.Subtract(alpha[b].num1);
					number2 = number2.Subtract(alpha[b].num2);
				}
				else
				{
					number = number.Add(alpha[b].num1);
					number2 = number2.Add(alpha[b].num2);
				}
			}
			else
			{
				array[num] = 0;
			}
			Number number4 = number;
			number = ((byteMu != 1) ? number2.Subtract(number.ShiftRight(1)) : number2.Add(number.ShiftRight(1)));
			number2 = number4.ShiftRight(1).Negate();
			num++;
		}
		return array;
	}

	public static Finite2MPoint[] FindComp(Finite2MPoint pointP, sbyte a)
	{
		Finite2MPoint[] array = new Finite2MPoint[16]
		{
			null, pointP, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null
		};
		sbyte[][] array2 = ((a != 0) ? AlphaOneT : AlphaZeroT);
		int num = array2.Length;
		for (int i = 3; i < num; i += 2)
		{
			array[i] = MultiplyFromTnaf(pointP, array2[i]);
		}
		return array;
	}
}
