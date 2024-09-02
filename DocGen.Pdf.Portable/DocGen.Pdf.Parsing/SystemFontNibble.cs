namespace DocGen.Pdf.Parsing;

internal class SystemFontNibble
{
	private readonly byte value;

	public static SystemFontNibble[] GetNibbles(byte b)
	{
		byte[] bits = SystemFontBitsHelper.GetBits(b);
		SystemFontNibble[] array = new SystemFontNibble[2];
		array[1] = new SystemFontNibble(SystemFontBitsHelper.ToByte(bits, 0, 4));
		array[0] = new SystemFontNibble(SystemFontBitsHelper.ToByte(bits, 4, 4));
		return array;
	}

	public static bool operator ==(SystemFontNibble left, SystemFontNibble right)
	{
		return left?.Equals(right) ?? ((object)right == null);
	}

	public static bool operator !=(SystemFontNibble left, SystemFontNibble right)
	{
		return !(left == right);
	}

	public SystemFontNibble(byte value)
	{
		this.value = value;
	}

	public override bool Equals(object obj)
	{
		SystemFontNibble systemFontNibble = obj as SystemFontNibble;
		if (!(systemFontNibble == null))
		{
			return value == systemFontNibble.value;
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
