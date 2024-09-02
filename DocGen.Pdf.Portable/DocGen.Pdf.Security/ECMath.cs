using System;

namespace DocGen.Pdf.Security;

internal class ECMath
{
	internal static EllipticPoint AddCurve(EllipticPoint PCurve, Number number, EllipticPoint QCurve, Number number1)
	{
		EllipticCurves curve = PCurve.Curve;
		if (!curve.Equals(QCurve.Curve))
		{
			throw new ArgumentException("PCurve and QCurve must be on same curve");
		}
		if (curve is Field2MCurves && ((Field2MCurves)curve).IsKOBLITZ)
		{
			return PCurve.Multiply(number).SumValue(QCurve.Multiply(number1));
		}
		return BlockFunction(PCurve, number, QCurve, number1);
	}

	private static EllipticPoint BlockFunction(EllipticPoint PCurve, Number number1, EllipticPoint QCurve, Number number2)
	{
		int num = Math.Max(number1.BitLength, number2.BitLength);
		EllipticPoint value = PCurve.SumValue(QCurve);
		EllipticPoint ellipticPoint = PCurve.Curve.IsInfinity;
		for (int num2 = num - 1; num2 >= 0; num2--)
		{
			ellipticPoint = ellipticPoint.Twice();
			if (number1.TestBit(num2))
			{
				ellipticPoint = ((!number2.TestBit(num2)) ? ellipticPoint.SumValue(PCurve) : ellipticPoint.SumValue(value));
			}
			else if (number2.TestBit(num2))
			{
				ellipticPoint = ellipticPoint.SumValue(QCurve);
			}
		}
		return ellipticPoint;
	}
}
