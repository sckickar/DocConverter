namespace DocGen.Pdf.Security;

internal sealed class NISTOIDs
{
	internal static readonly DerObjectID NistAlgorithm = new DerObjectID("2.16.840.1.101.3.4");

	internal static readonly DerObjectID HashAlgs = NistAlgorithm.Branch("2");

	internal static readonly DerObjectID SHA256 = HashAlgs.Branch("1");

	internal static readonly DerObjectID SHA384 = HashAlgs.Branch("2");

	internal static readonly DerObjectID SHA512 = HashAlgs.Branch("3");

	internal static readonly DerObjectID DSAWithSHA2 = new DerObjectID(NistAlgorithm?.ToString() + ".3");

	internal static readonly DerObjectID DSAWithSHA256 = new DerObjectID(DSAWithSHA2?.ToString() + ".2");

	internal static readonly DerObjectID DSAWithSHA384 = new DerObjectID(DSAWithSHA2?.ToString() + ".3");

	internal static readonly DerObjectID DSAWithSHA512 = new DerObjectID(DSAWithSHA2?.ToString() + ".4");

	internal static readonly DerObjectID TTTAlgorithm = new DerObjectID("1.3.36.3");

	internal static readonly DerObjectID RipeMD160 = new DerObjectID(TTTAlgorithm?.ToString() + ".2.1");

	internal static readonly DerObjectID TTTRsaSignatureAlgorithm = new DerObjectID(TTTAlgorithm?.ToString() + ".3.1");

	internal static readonly DerObjectID RsaSignatureWithRipeMD160 = new DerObjectID(TTTRsaSignatureAlgorithm?.ToString() + ".2");
}
