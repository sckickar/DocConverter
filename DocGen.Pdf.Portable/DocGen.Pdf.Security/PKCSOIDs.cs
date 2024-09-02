namespace DocGen.Pdf.Security;

internal abstract class PKCSOIDs
{
	internal const string Pkcs1 = "1.2.840.113549.1.1";

	internal static readonly DerObjectID RsaEncryption = new DerObjectID("1.2.840.113549.1.1.1");

	internal static readonly DerObjectID MD2WithRsaEncryption = new DerObjectID("1.2.840.113549.1.1.2");

	internal static readonly DerObjectID Sha1WithRsaEncryption = new DerObjectID("1.2.840.113549.1.1.5");

	internal static readonly DerObjectID RsaCrlAlgorithmIdntifier = new DerObjectID("1.2.840.113549.1.1.10");

	internal static readonly DerObjectID Sha256WithRsaEncryption = new DerObjectID("1.2.840.113549.1.1.11");

	internal static readonly DerObjectID Sha384WithRsaEncryption = new DerObjectID("1.2.840.113549.1.1.12");

	internal static readonly DerObjectID Sha512WithRsaEncryption = new DerObjectID("1.2.840.113549.1.1.13");

	internal const string EncryptionAlgorithm = "1.2.840.113549.3";

	internal static readonly DerObjectID DesEde3Cbc = new DerObjectID("1.2.840.113549.3.7");

	internal static readonly DerObjectID RC2Cbc = new DerObjectID("1.2.840.113549.3.2");

	internal const string DigestAlgorithm = "1.2.840.113549.2";

	internal const string Pkcs7 = "1.2.840.113549.1.7";

	internal static readonly DerObjectID Data = new DerObjectID("1.2.840.113549.1.7.1");

	internal static readonly DerObjectID SignedData = new DerObjectID("1.2.840.113549.1.7.2");

	internal static readonly DerObjectID EnvelopedData = new DerObjectID("1.2.840.113549.1.7.3");

	internal static readonly DerObjectID SignedAndEnvelopedData = new DerObjectID("1.2.840.113549.1.7.4");

	internal static readonly DerObjectID DigestedData = new DerObjectID("1.2.840.113549.1.7.5");

	internal static readonly DerObjectID EncryptedData = new DerObjectID("1.2.840.113549.1.7.6");

	internal const string Pkcs9 = "1.2.840.113549.1.9";

	internal const string TimeStampCert = "1.2.840.113549.1.9.16";

	internal const string TimeStampCertInformation = "1.2.840.113549.1.9.16.1.4";

	internal const string SigningCert = "1.2.840.113549.1.9.16.2";

	internal static readonly DerObjectID Pkcs9AtEmailAddress = new DerObjectID("1.2.840.113549.1.9.1");

	internal static readonly DerObjectID Pkcs9AtUnstructuredName = new DerObjectID("1.2.840.113549.1.9.2");

	internal static readonly DerObjectID Pkcs9AtMessageDigest = new DerObjectID("1.2.840.113549.1.9.4");

	internal static readonly DerObjectID Pkcs9AtUnstructuredAddress = new DerObjectID("1.2.840.113549.1.9.8");

	internal static readonly DerObjectID Pkcs9AtFriendlyName = new DerObjectID("1.2.840.113549.1.9.20");

	internal static readonly DerObjectID Pkcs9AtLocalKeyID = new DerObjectID("1.2.840.113549.1.9.21");

	internal static readonly DerObjectID Pkcs9AtSigningCertV1 = new DerObjectID("1.2.840.113549.1.9.16.2.12");

	internal static readonly DerObjectID Pkcs9AtSigningCertV2 = new DerObjectID("1.2.840.113549.1.9.16.2.47");

	internal static readonly DerObjectID Pkcs9SignatureTimeStamp = new DerObjectID("1.2.840.113549.1.9.16.2.14");

	internal const string Pkcs12 = "1.2.840.113549.1.12";

	internal const string BagTypes = "1.2.840.113549.1.12.10.1";

	internal static readonly DerObjectID KeyBag = new DerObjectID("1.2.840.113549.1.12.10.1.1");

	internal static readonly DerObjectID Pkcs8ShroudedKeyBag = new DerObjectID("1.2.840.113549.1.12.10.1.2");

	internal static readonly DerObjectID CertBag = new DerObjectID("1.2.840.113549.1.12.10.1.3");

	internal static readonly DerObjectID CrlBag = new DerObjectID("1.2.840.113549.1.12.10.1.4");

	internal static readonly DerObjectID SecretBag = new DerObjectID("1.2.840.113549.1.12.10.1.5");

	internal static readonly DerObjectID SafeContentsBag = new DerObjectID("1.2.840.113549.1.12.10.1.6");

	internal const string Pkcs12PbeIds = "1.2.840.113549.1.12.1";

	internal static readonly DerObjectID PbeWithShaAnd128BitRC4 = new DerObjectID("1.2.840.113549.1.12.1.1");

	internal static readonly DerObjectID PbeWithShaAnd40BitRC4 = new DerObjectID("1.2.840.113549.1.12.1.2");

	internal static readonly DerObjectID PbeWithShaAnd3KeyTripleDesCbc = new DerObjectID("1.2.840.113549.1.12.1.3");

	internal static readonly DerObjectID PbeWithShaAnd2KeyTripleDesCbc = new DerObjectID("1.2.840.113549.1.12.1.4");

	internal static readonly DerObjectID PbeWithShaAnd128BitRC2Cbc = new DerObjectID("1.2.840.113549.1.12.1.5");

	internal static readonly DerObjectID PbewithShaAnd40BitRC2Cbc = new DerObjectID("1.2.840.113549.1.12.1.6");

	internal static readonly DerObjectID IdAlgCms3DesWrap = new DerObjectID("1.2.840.113549.1.9.16.3.6");

	internal static readonly DerObjectID IdAlgCmsRC2Wrap = new DerObjectID("1.2.840.113549.1.9.16.3.7");

	internal const string MessageDigestAlgorithm = "1.2.840.113549.2";

	internal static readonly DerObjectID MD5 = new DerObjectID("1.2.840.113549.25");

	internal static readonly DerObjectID DigestAlgorithmSHA256 = new DerObjectID("1.2.840.113549.29");

	internal static readonly DerObjectID DigestAlgorithmSHA384 = new DerObjectID("1.2.840.113549.210");

	internal static readonly DerObjectID DigestAlgorithmSHA512 = new DerObjectID("1.2.840.113549.211");

	internal static readonly DerObjectID AdobeRevocation = new DerObjectID("1.2.840.113583.1.1.8");

	internal static readonly DerObjectID Tsa = new DerObjectID("1.2.840.113583.1.1.9.1");
}
