using System;

namespace DocGen.Pdf.Security;

internal class EllipticTNMuliplier : EllipticMultiplier
{
	public EllipticPoint Multiply(EllipticPoint pointP, Number number, EllipticComp preInfo)
	{
		if (!(pointP is Finite2MPoint))
		{
			throw new ArgumentException("Finite2MPoint");
		}
		Finite2MPoint finite2MPoint = (Finite2MPoint)pointP;
		Field2MCurves obj = (Field2MCurves)finite2MPoint.Curve;
		int pointM = obj.PointM;
		sbyte b = (sbyte)obj.ElementA.ToIntValue().IntValue;
		sbyte b2 = obj.MU();
		Number[] numberS = obj.SI();
		ITanuElement lambdaValue = ECTanFunction.MODFun(number, pointM, b, numberS, b2, 10);
		return MultiplyValue(finite2MPoint, lambdaValue, preInfo, b, b2);
	}

	private Finite2MPoint MultiplyValue(Finite2MPoint pointP, ITanuElement lambdaValue, EllipticComp preInfo, sbyte bitA, sbyte pointMU)
	{
		ITanuElement[] alpha = ((bitA != 0) ? ECTanFunction.AlphaOne : ECTanFunction.AlphaZeo);
		Number tw = ECTanFunction.FindTW(pointMU, 4);
		sbyte[] tAdic = ECTanFunction.GetTAdic(pointMU, lambdaValue, 4, Number.ValueOf(16L), tw, alpha);
		return MultiplyTFunction(pointP, tAdic, preInfo);
	}

	private static Finite2MPoint MultiplyTFunction(Finite2MPoint pointP, sbyte[] byteU, EllipticComp preInfo)
	{
		sbyte a = (sbyte)((Field2MCurves)pointP.Curve).ElementA.ToIntValue().IntValue;
		Finite2MPoint[] array;
		if (preInfo == null || !(preInfo is FiniteCompField))
		{
			array = ECTanFunction.FindComp(pointP, a);
			pointP.SetInfo(new FiniteCompField(array));
		}
		else
		{
			array = ((FiniteCompField)preInfo).FindComp();
		}
		Finite2MPoint finite2MPoint = (Finite2MPoint)pointP.Curve.IsInfinity;
		for (int num = byteU.Length - 1; num >= 0; num--)
		{
			finite2MPoint = ECTanFunction.GetTanU(finite2MPoint);
			if (byteU[num] != 0)
			{
				finite2MPoint = ((byteU[num] <= 0) ? finite2MPoint.SubtractSimple(array[-byteU[num]]) : finite2MPoint.AddSimple(array[byteU[num]]));
			}
		}
		return finite2MPoint;
	}
}
