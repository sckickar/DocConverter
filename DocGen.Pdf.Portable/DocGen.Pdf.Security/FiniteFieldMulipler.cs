namespace DocGen.Pdf.Security;

internal class FiniteFieldMulipler : EllipticMultiplier
{
	public EllipticPoint Multiply(EllipticPoint pointP, Number number, EllipticComp preInfo)
	{
		Number number2 = number.Multiply(Number.Three);
		EllipticPoint ellipticPoint = pointP.Negate();
		EllipticPoint ellipticPoint2 = pointP;
		for (int num = number2.BitLength - 2; num > 0; num--)
		{
			ellipticPoint2 = ellipticPoint2.Twice();
			bool flag = number2.TestBit(num);
			bool flag2 = number.TestBit(num);
			if (flag != flag2)
			{
				ellipticPoint2 = ellipticPoint2.SumValue(flag ? pointP : ellipticPoint);
			}
		}
		return ellipticPoint2;
	}
}
