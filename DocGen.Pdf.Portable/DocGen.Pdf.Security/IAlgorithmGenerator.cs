namespace DocGen.Pdf.Security;

internal interface IAlgorithmGenerator
{
	void AddMaterial(byte[] bytes);

	void AddMaterial(long value);

	void FillNextBytes(byte[] bytes);

	void FillNextBytes(byte[] bytes, int start, int length);
}
