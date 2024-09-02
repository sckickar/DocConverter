namespace DocGen.Pdf;

internal class ByteStreamRenderer
{
	private byte[] data;

	private int position;

	public int Remaining => data.Length - position;

	public ByteStreamRenderer(byte[] data)
	{
		this.data = data;
		position = 0;
	}

	public byte ReadByte()
	{
		byte result = data[position];
		position++;
		return result;
	}

	public void Read(byte[] buffer)
	{
		for (int i = 0; i < buffer.Length; i++)
		{
			buffer[i] = ReadByte();
		}
	}
}
