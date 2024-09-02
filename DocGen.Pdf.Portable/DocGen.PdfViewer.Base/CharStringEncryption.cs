namespace DocGen.PdfViewer.Base;

internal class CharStringEncryption : EncryptionStdHelper
{
	public CharStringEncryption(PostScriptParser reader)
		: base(reader, 4330, 4)
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
