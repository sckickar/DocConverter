namespace DocGen.PdfViewer.Base;

internal abstract class EncryptionStdHelper
{
	private const ushort C1 = 52845;

	private const ushort C2 = 22719;

	private readonly int randomBytesCount;

	private readonly PostScriptParser reader;

	private ushort r;

	public int RandomBytesCount => randomBytesCount;

	protected PostScriptParser Reader => reader;

	public EncryptionStdHelper(PostScriptParser reader, ushort r, int n)
	{
		this.reader = reader;
		this.r = r;
		randomBytesCount = n;
	}

	public abstract void Initialize();

	public byte Decrypt(byte cipher)
	{
		byte result = (byte)(cipher ^ (r >> 8));
		r = (ushort)((cipher + r) * 52845 + 22719);
		return result;
	}
}
