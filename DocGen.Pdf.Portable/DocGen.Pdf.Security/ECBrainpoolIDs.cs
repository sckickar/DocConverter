namespace DocGen.Pdf.Security;

internal sealed class ECBrainpoolIDs
{
	public static readonly DerObjectID Algorithm = new DerObjectID("1.3.36.3");

	public static readonly DerObjectID EllipticSign = new DerObjectID(Algorithm?.ToString() + ".3.2");

	public static readonly DerObjectID EllipticSignSignWithSha1 = new DerObjectID(EllipticSign?.ToString() + ".1");

	public static readonly DerObjectID EllipticSignWithRipeMD160 = new DerObjectID(EllipticSign?.ToString() + ".2");

	public static readonly DerObjectID ECBrainpool = new DerObjectID(Algorithm?.ToString() + ".3.2.8");

	public static readonly DerObjectID EllipticCurve = new DerObjectID(ECBrainpool?.ToString() + ".1");

	public static readonly DerObjectID V1 = new DerObjectID(EllipticCurve?.ToString() + ".1");

	public static readonly DerObjectID BrainpoolP160R1 = new DerObjectID(V1?.ToString() + ".1");

	public static readonly DerObjectID BrainpoolP160T1 = new DerObjectID(V1?.ToString() + ".2");

	public static readonly DerObjectID BrainpoolP192R1 = new DerObjectID(V1?.ToString() + ".3");

	public static readonly DerObjectID BrainpoolP192T1 = new DerObjectID(V1?.ToString() + ".4");

	public static readonly DerObjectID BrainpoolP224R1 = new DerObjectID(V1?.ToString() + ".5");

	public static readonly DerObjectID BrainpoolP224T1 = new DerObjectID(V1?.ToString() + ".6");

	public static readonly DerObjectID BrainpoolP256R1 = new DerObjectID(V1?.ToString() + ".7");

	public static readonly DerObjectID BrainpoolP256T1 = new DerObjectID(V1?.ToString() + ".8");

	public static readonly DerObjectID BrainpoolP320R1 = new DerObjectID(V1?.ToString() + ".9");

	public static readonly DerObjectID BrainpoolP320T1 = new DerObjectID(V1?.ToString() + ".10");

	public static readonly DerObjectID BrainpoolP384R1 = new DerObjectID(V1?.ToString() + ".11");

	public static readonly DerObjectID BrainpoolP384T1 = new DerObjectID(V1?.ToString() + ".12");

	public static readonly DerObjectID BrainpoolP512R1 = new DerObjectID(V1?.ToString() + ".13");

	public static readonly DerObjectID BrainpoolP512T1 = new DerObjectID(V1?.ToString() + ".14");

	private ECBrainpoolIDs()
	{
	}
}
