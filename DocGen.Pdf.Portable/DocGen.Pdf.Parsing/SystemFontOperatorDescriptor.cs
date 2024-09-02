using System.Text;

namespace DocGen.Pdf.Parsing;

internal class SystemFontOperatorDescriptor
{
	internal const byte TwoByteOperatorFirstByte = 12;

	private readonly byte[] value;

	private readonly object defaultValue;

	private int hashCode;

	public object DefaultValue => defaultValue;

	public SystemFontOperatorDescriptor(byte b0)
	{
		value = new byte[1];
		value[0] = b0;
		CalculateHashCode();
	}

	public SystemFontOperatorDescriptor(byte[] bytes)
	{
		value = bytes;
		CalculateHashCode();
	}

	public SystemFontOperatorDescriptor(byte b0, object defaultValue)
		: this(b0)
	{
		this.defaultValue = defaultValue;
	}

	public SystemFontOperatorDescriptor(byte[] bytes, object defaultValue)
		: this(bytes)
	{
		this.defaultValue = defaultValue;
	}

	private void CalculateHashCode()
	{
		hashCode = 17;
		for (int i = 0; i < value.Length; i++)
		{
			hashCode = hashCode * 23 + value[i];
		}
	}

	public override bool Equals(object obj)
	{
		if (!(obj is SystemFontOperatorDescriptor systemFontOperatorDescriptor) || value.Length != systemFontOperatorDescriptor.value.Length)
		{
			return false;
		}
		for (int i = 0; i < value.Length; i++)
		{
			if (value[i] != systemFontOperatorDescriptor.value[i])
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		return hashCode;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		byte[] array = value;
		foreach (int num in array)
		{
			stringBuilder.Append(num);
			stringBuilder.Append(" ");
		}
		return stringBuilder.ToString().Trim();
	}
}
