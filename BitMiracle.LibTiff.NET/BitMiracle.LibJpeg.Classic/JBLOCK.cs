namespace BitMiracle.LibJpeg.Classic;

internal class JBLOCK
{
	internal short[] data = new short[64];

	public short this[int index]
	{
		get
		{
			return data[index];
		}
		set
		{
			data[index] = value;
		}
	}
}
