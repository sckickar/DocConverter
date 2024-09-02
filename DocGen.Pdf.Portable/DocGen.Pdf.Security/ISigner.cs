namespace DocGen.Pdf.Security;

internal interface ISigner
{
	string AlgorithmName { get; }

	void Initialize(bool isSigning, ICipherParam parameters);

	void Update(byte input);

	void BlockUpdate(byte[] bytes, int offset, int length);

	byte[] GenerateSignature();

	bool ValidateSignature(byte[] bytes);

	void Reset();
}
