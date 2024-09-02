namespace DocGen.Pdf.Security;

internal interface ICipherKeyGen
{
	void Init(KeyGenParam parameters);

	ECCipherKeyParam GenerateKeyPair();
}
