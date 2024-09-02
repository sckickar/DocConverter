namespace DocGen.Pdf.Security;

internal abstract class X509Objects
{
	internal const string ID = "2.5.4";

	internal static readonly DerObjectID CommonName = new DerObjectID("2.5.4.3");

	internal static readonly DerObjectID CountryName = new DerObjectID("2.5.4.6");

	internal static readonly DerObjectID LocalityName = new DerObjectID("2.5.4.7");

	internal static readonly DerObjectID StateOrProvinceName = new DerObjectID("2.5.4.8");

	internal static readonly DerObjectID Organization = new DerObjectID("2.5.4.10");

	internal static readonly DerObjectID OrganizationalUnitName = new DerObjectID("2.5.4.11");

	internal static readonly DerObjectID TelephoneNumberID = new DerObjectID("2.5.4.20");

	internal static readonly DerObjectID NameID = new DerObjectID("2.5.4.41");

	internal static readonly DerObjectID IdSha1 = new DerObjectID("1.3.14.3.2.26");

	internal static readonly DerObjectID RipeMD160 = new DerObjectID("1.3.36.3.2.1");

	internal static readonly DerObjectID RipeMD160WithRsaEncryption = new DerObjectID("1.3.36.3.3.1.2");

	internal static readonly DerObjectID IdEARsa = new DerObjectID("2.5.8.1.1");

	internal static readonly DerObjectID IdPkix = new DerObjectID("1.3.6.1.5.5.7");

	internal static readonly DerObjectID IdPE = new DerObjectID(IdPkix?.ToString() + ".1");

	internal static readonly DerObjectID IdAD = new DerObjectID(IdPkix?.ToString() + ".48");

	internal static readonly DerObjectID IdADCAIssuers = new DerObjectID(IdAD?.ToString() + ".2");

	internal static readonly DerObjectID IdADOcsp = new DerObjectID(IdAD?.ToString() + ".1");

	internal static readonly DerObjectID OcspAccessMethod = IdADOcsp;

	internal static readonly DerObjectID CrlAccessMethod = IdADCAIssuers;
}
