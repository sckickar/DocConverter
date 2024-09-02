namespace DocGen.Pdf.Native;

internal struct CERT_PUBLIC_KEY_INFO
{
	public CRYPT_ALGORITHM_IDENTIFIER Algorithm;

	public CryptographicApiStore PublicKey;
}
