namespace DocGen.Pdf.Security;

internal interface EllipticMultiplier
{
	EllipticPoint Multiply(EllipticPoint pointP, Number number, EllipticComp preInfo);
}
