namespace DocGen.Pdf.Security;

internal interface IDSASigner
{
	string AlgorithmName { get; }

	void Initialize(bool forSigning, ICipherParam parameters);

	Number[] GenerateSignature(byte[] message);

	bool ValidateSignature(byte[] message, Number r, Number s);
}
