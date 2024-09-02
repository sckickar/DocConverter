namespace DocGen.Pdf.Security;

internal interface IMessageDigest
{
	string AlgorithmName { get; }

	int MessageDigestSize { get; }

	int ByteLength { get; }

	void Update(byte input);

	void Update(byte[] bytes, int offset, int length);

	void BlockUpdate(byte[] bytes, int offset, int length);

	int DoFinal(byte[] bytes, int offset);

	void Reset();
}
