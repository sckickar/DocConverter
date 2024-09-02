namespace DocGen.PdfViewer.Base;

internal class PostScriptStrHelper
{
	private readonly char[] value;

	public string Value => value.ToString();

	public int Capacity => value.Length;

	public char this[int index]
	{
		get
		{
			return value[index];
		}
		set
		{
			this.value[index] = value;
		}
	}

	public PostScriptStrHelper(string str)
	{
		value = str.ToCharArray();
	}

	public PostScriptStrHelper(int capacity)
	{
		value = new char[capacity];
	}

	public byte[] ToByteArray()
	{
		byte[] array = new byte[value.Length];
		for (int i = 0; i < value.Length; i++)
		{
			array[i] = (byte)value[i];
		}
		return array;
	}

	public override string ToString()
	{
		return new string(value);
	}
}
