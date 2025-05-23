namespace DocGen.Pdf.Security;

internal interface IBufferedCipher
{
	string AlgorithmName { get; }

	int BlockSize { get; }

	void Initialize(bool forEncryption, ICipherParam parameters);

	int GetOutputSize(int inputLen);

	int GetUpdateOutputSize(int inputLen);

	byte[] ProcessByte(byte input);

	int ProcessByte(byte input, byte[] output, int outOff);

	byte[] ProcessBytes(byte[] input);

	byte[] ProcessBytes(byte[] input, int inOff, int length);

	int ProcessBytes(byte[] input, byte[] output, int outOff);

	int ProcessBytes(byte[] input, int inOff, int length, byte[] output, int outOff);

	byte[] DoFinal();

	byte[] DoFinal(byte[] input);

	byte[] DoFinal(byte[] input, int inOff, int length);

	int DoFinal(byte[] output, int outOff);

	int DoFinal(byte[] input, byte[] output, int outOff);

	int DoFinal(byte[] input, int inOff, int length, byte[] output, int outOff);

	void Reset();
}
