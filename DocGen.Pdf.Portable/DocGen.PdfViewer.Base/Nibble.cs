namespace DocGen.PdfViewer.Base;

internal class Nibble
{
	private readonly byte value;

	public static Nibble[] GetNibbles(byte b)
	{
		byte[] bits = BitsAssistant.GetBits(b);
		Nibble[] array = new Nibble[2];
		array[1] = new Nibble(BitsAssistant.ToByte(bits, 0, 4));
		array[0] = new Nibble(BitsAssistant.ToByte(bits, 4, 4));
		return array;
	}

	public static bool operator ==(Nibble left, Nibble right)
	{
		return left?.Equals(right) ?? ((object)right == null);
	}

	public static bool operator !=(Nibble left, Nibble right)
	{
		return !(left == right);
	}

	public Nibble(byte value)
	{
		this.value = value;
	}

	public override bool Equals(object obj)
	{
		Nibble nibble = obj as Nibble;
		if (!(nibble == null))
		{
			return value == nibble.value;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return 17 * 23 + value.GetHashCode();
	}

	public override string ToString()
	{
		return $"{value:X}";
	}
}
