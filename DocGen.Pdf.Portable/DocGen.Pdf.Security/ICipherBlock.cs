namespace DocGen.Pdf.Security;

internal interface ICipherBlock
{
	string AlgorithmName { get; }

	int InputBlock { get; }

	int OutputBlock { get; }

	void Initialize(bool isEncryption, ICipherParam parameters);

	byte[] ProcessBlock(byte[] bytes, int offset, int length);
}
