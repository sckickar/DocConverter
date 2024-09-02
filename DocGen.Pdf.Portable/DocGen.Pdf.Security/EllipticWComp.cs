namespace DocGen.Pdf.Security;

internal class EllipticWComp : EllipticComp
{
	private EllipticPoint[] compBitValue;

	private EllipticPoint twicePoint;

	internal EllipticPoint[] FindComp()
	{
		return compBitValue;
	}

	internal void SetComp(EllipticPoint[] compBitValue)
	{
		this.compBitValue = compBitValue;
	}

	internal EllipticPoint FindTwice()
	{
		return twicePoint;
	}

	internal void TwicePoint(EllipticPoint twiceP)
	{
		twicePoint = twiceP;
	}
}
