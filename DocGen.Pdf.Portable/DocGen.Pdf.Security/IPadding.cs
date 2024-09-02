namespace DocGen.Pdf.Security;

internal interface IPadding
{
	string PaddingName { get; }

	void Initialize(SecureRandomAlgorithm random);

	int AddPadding(byte[] bytes, int offset);

	int Count(byte[] bytes);
}
