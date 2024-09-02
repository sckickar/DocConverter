namespace DocGen.Pdf.Security;

internal abstract class OcspConstants
{
	internal const string OcspId = "1.3.6.1.5.5.7.48.1";

	public static readonly DerObjectID Ocsp = new DerObjectID("1.3.6.1.5.5.7.48.1");

	public static readonly DerObjectID OcspBasic = new DerObjectID("1.3.6.1.5.5.7.48.1.1");

	public static readonly DerObjectID OcspNonce = new DerObjectID(Ocsp?.ToString() + ".2");

	public static readonly DerObjectID OcspCrl = new DerObjectID(Ocsp?.ToString() + ".3");

	public static readonly DerObjectID OcspResponse = new DerObjectID(Ocsp?.ToString() + ".4");

	public static readonly DerObjectID OcspNocheck = new DerObjectID(Ocsp?.ToString() + ".5");

	public static readonly DerObjectID OcspArchiveCutoff = new DerObjectID(Ocsp?.ToString() + ".6");

	public static readonly DerObjectID OcspServiceLocator = new DerObjectID(Ocsp?.ToString() + ".7");
}
