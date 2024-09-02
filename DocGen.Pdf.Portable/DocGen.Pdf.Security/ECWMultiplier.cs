using System;

namespace DocGen.Pdf.Security;

internal class ECWMultiplier : EllipticMultiplier
{
	public sbyte[] CheckBitValue(sbyte width, Number number)
	{
		sbyte[] array = new sbyte[number.BitLength + 1];
		short num = (short)(1 << (int)width);
		Number m = Number.ValueOf(num);
		int num2 = 0;
		int num3 = 0;
		while (number.SignValue > 0)
		{
			if (number.TestBit(0))
			{
				Number number2 = number.Mod(m);
				if (number2.TestBit(width - 1))
				{
					array[num2] = (sbyte)(number2.IntValue - num);
				}
				else
				{
					array[num2] = (sbyte)number2.IntValue;
				}
				number = number.Subtract(Number.ValueOf(array[num2]));
				num3 = num2;
			}
			else
			{
				array[num2] = 0;
			}
			number = number.ShiftRight(1);
			num2++;
		}
		num3++;
		sbyte[] array2 = new sbyte[num3];
		Array.Copy(array, 0, array2, 0, num3);
		return array2;
	}

	public EllipticPoint Multiply(EllipticPoint pointP, Number number, EllipticComp preInfo)
	{
		EllipticWComp ellipticWComp = ((preInfo == null || !(preInfo is EllipticWComp)) ? new EllipticWComp() : ((EllipticWComp)preInfo));
		int bitLength = number.BitLength;
		sbyte width;
		int num;
		if (bitLength < 13)
		{
			width = 2;
			num = 1;
		}
		else if (bitLength < 41)
		{
			width = 3;
			num = 2;
		}
		else if (bitLength < 121)
		{
			width = 4;
			num = 4;
		}
		else if (bitLength < 337)
		{
			width = 5;
			num = 8;
		}
		else if (bitLength < 897)
		{
			width = 6;
			num = 16;
		}
		else if (bitLength < 2305)
		{
			width = 7;
			num = 32;
		}
		else
		{
			width = 8;
			num = 127;
		}
		int num2 = 1;
		EllipticPoint[] array = ellipticWComp.FindComp();
		EllipticPoint ellipticPoint = ellipticWComp.FindTwice();
		if (array == null)
		{
			array = new EllipticPoint[1] { pointP };
		}
		else
		{
			num2 = array.Length;
		}
		if (ellipticPoint == null)
		{
			ellipticPoint = pointP.Twice();
		}
		if (num2 < num)
		{
			EllipticPoint[] sourceArray = array;
			array = new EllipticPoint[num];
			Array.Copy(sourceArray, 0, array, 0, num2);
			for (int i = num2; i < num; i++)
			{
				array[i] = ellipticPoint.SumValue(array[i - 1]);
			}
		}
		sbyte[] array2 = CheckBitValue(width, number);
		int num3 = array2.Length;
		EllipticPoint ellipticPoint2 = pointP.Curve.IsInfinity;
		for (int num4 = num3 - 1; num4 >= 0; num4--)
		{
			ellipticPoint2 = ellipticPoint2.Twice();
			if (array2[num4] != 0)
			{
				ellipticPoint2 = ((array2[num4] <= 0) ? ellipticPoint2.Subtract(array[(-array2[num4] - 1) / 2]) : ellipticPoint2.SumValue(array[(array2[num4] - 1) / 2]));
			}
		}
		ellipticWComp.SetComp(array);
		ellipticWComp.TwicePoint(ellipticPoint);
		pointP.SetInfo(ellipticWComp);
		return ellipticPoint2;
	}
}
