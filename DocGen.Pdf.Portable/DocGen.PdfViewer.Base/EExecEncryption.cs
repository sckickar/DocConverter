namespace DocGen.PdfViewer.Base;

internal class EExecEncryption : EncryptionStdHelper
{
	public EExecEncryption(PostScriptParser reader)
		: base(reader, 55665, 4)
	{
	}

	public override void Initialize()
	{
		for (int i = 0; i < base.RandomBytesCount; i++)
		{
			base.Reader.Read();
		}
	}
}
