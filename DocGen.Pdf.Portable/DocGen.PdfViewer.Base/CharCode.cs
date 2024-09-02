using System;
using System.Text;

namespace DocGen.PdfViewer.Base;

internal struct CharCode
{
	private readonly byte[] bytes;

	private int intValue;

	public int BytesCount
	{
		get
		{
			if (bytes == null)
			{
				return 0;
			}
			return bytes.Length;
		}
	}

	public byte[] Bytes => bytes;

	public int IntValue
	{
		get
		{
			return intValue;
		}
		set
		{
			intValue = value;
		}
	}

	public bool IsEmpty => bytes == null;

	public CharCode(byte[] bytes)
	{
		this.bytes = bytes;
		intValue = BytesAssistant.GetInt(this.bytes);
	}

	public CharCode(byte b)
	{
		bytes = new byte[1];
		bytes[0] = b;
		intValue = b;
	}

	public CharCode(ushort us)
	{
		byte[] array = BitConverter.GetBytes(us);
		bytes = new byte[array.Length];
		int num = array.Length - 1;
		for (int i = 0; i < array.Length; i++)
		{
			bytes[num - i] = array[i];
		}
		intValue = us;
	}

	public CharCode(int ii)
	{
		byte[] array = BitConverter.GetBytes(ii);
		bytes = new byte[array.Length];
		int num = array.Length - 1;
		for (int i = 0; i < array.Length; i++)
		{
			bytes[num - i] = array[i];
		}
		intValue = ii;
	}

	private static void InsureCharCodes(CharCode left, CharCode right)
	{
		if (left.bytes == null || right.bytes == null)
		{
			throw new ArgumentException("Bytes cannot be null.");
		}
		if (left.bytes.Length != right.bytes.Length)
		{
			throw new InvalidOperationException("Cannot compare CharCodes with different length.");
		}
	}

	public static CharCode operator ++(CharCode cc)
	{
		byte[] array = (byte[])cc.bytes.Clone();
		int num = 1;
		for (int num2 = array.Length - 1; num2 >= 0; num2--)
		{
			int num3 = array[num2] + num;
			num = num3 / 256;
			array[num2] = (byte)(num3 % 256);
			if (num == 0)
			{
				break;
			}
		}
		if (num > 0)
		{
			throw new OverflowException();
		}
		return new CharCode(array);
	}

	public static bool operator <(CharCode left, CharCode right)
	{
		InsureCharCodes(left, right);
		for (int i = 0; i < left.bytes.Length; i++)
		{
			if (left.bytes[i] > right.bytes[i])
			{
				return false;
			}
			if (left.bytes[i] == right.bytes[i] && left.IntValue == right.IntValue)
			{
				return false;
			}
		}
		return true;
	}

	public static bool operator ==(CharCode left, CharCode right)
	{
		InsureCharCodes(left, right);
		for (int i = 0; i < left.bytes.Length; i++)
		{
			if (left.bytes[i] != right.bytes[i])
			{
				return false;
			}
		}
		return true;
	}

	public static bool operator >(CharCode left, CharCode right)
	{
		InsureCharCodes(left, right);
		for (int i = 0; i < left.bytes.Length; i++)
		{
			if (left.bytes[i] <= right.bytes[i])
			{
				return false;
			}
		}
		return true;
	}

	public static bool operator !=(CharCode left, CharCode right)
	{
		return !(left == right);
	}

	public static bool operator <=(CharCode left, CharCode right)
	{
		if (!(left < right))
		{
			return left == right;
		}
		return true;
	}

	public static bool operator >=(CharCode left, CharCode right)
	{
		if (!(left > right))
		{
			return left == right;
		}
		return true;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("<");
		for (int i = 0; i < bytes.Length; i++)
		{
			stringBuilder.AppendFormat("{0:X2}", bytes[i]);
		}
		stringBuilder.Append("> ");
		stringBuilder.Append(GetHashCode());
		return stringBuilder.ToString();
	}

	public override bool Equals(object obj)
	{
		if (bytes == null)
		{
			return false;
		}
		if (obj is CharCode charCode)
		{
			if (BytesCount == charCode.BytesCount)
			{
				return this == charCode;
			}
			return false;
		}
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return IntValue;
	}
}
