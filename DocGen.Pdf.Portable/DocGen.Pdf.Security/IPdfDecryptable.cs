namespace DocGen.Pdf.Security;

internal interface IPdfDecryptable
{
	bool WasEncrypted { get; }

	bool Decrypted { get; }

	void Decrypt(PdfEncryptor encryptor, long currObjNumber);
}
