namespace DocGen.Pdf.Security;

internal interface ICipher
{
	string AlgorithmName { get; }

	int BlockSize { get; }

	bool IsBlock { get; }

	void Initialize(bool isEncryption, ICipherParam parameters);

	int ProcessBlock(byte[] inBytes, int inOffset, byte[] outBytes, int outOffset);

	void Reset();
}
