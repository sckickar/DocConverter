namespace DocGen.Pdf.Native;

internal struct CertInformation
{
	public int Version;

	public CryptographicApiStore SerialNumber;

	public CRYPT_ALGORITHM_IDENTIFIER SignatureAlgorithm;

	public CryptographicApiStore Issuer;

	public FileTime NotBefore;

	public FileTime NotAfter;

	public CryptographicApiStore Subject;

	public CERT_PUBLIC_KEY_INFO SubjectPublicKeyInfo;

	public CryptographicApiStore IssuerUniqueId;

	public CryptographicApiStore SubjectUniqueId;

	public int ExtensionCount;

	public PCERT_EXTENSION Extension;
}
