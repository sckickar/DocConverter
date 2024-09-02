namespace DocGen.Pdf.Security;

internal class FiniteCompField : EllipticComp
{
	private readonly Finite2MPoint[] compBitValue;

	internal FiniteCompField(Finite2MPoint[] compBitValue)
	{
		this.compBitValue = compBitValue;
	}

	internal Finite2MPoint[] FindComp()
	{
		return compBitValue;
	}
}
