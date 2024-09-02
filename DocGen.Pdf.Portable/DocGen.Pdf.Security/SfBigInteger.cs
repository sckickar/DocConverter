using System;

namespace DocGen.Pdf.Security;

internal class SfBigInteger
{
	private const int maxLength = 70;

	private uint[] data;

	private int dataLength;

	internal SfBigInteger()
	{
		data = new uint[70];
		dataLength = 1;
	}

	internal SfBigInteger(long value)
	{
		data = new uint[70];
		long num = value;
		dataLength = 0;
		while (value != 0L && dataLength < 70)
		{
			data[dataLength] = (uint)(value & 0xFFFFFFFFu);
			value >>= 32;
			dataLength++;
		}
		if (num > 0)
		{
			if (value != 0L || (data[69] & 0x80000000u) != 0)
			{
				throw new ArithmeticException("Positive overflow in constructor.");
			}
		}
		else if (num < 0 && (value != -1 || (data[dataLength - 1] & 0x80000000u) == 0))
		{
			throw new ArithmeticException("Negative underflow in constructor.");
		}
		if (dataLength == 0)
		{
			dataLength = 1;
		}
	}

	private SfBigInteger(SfBigInteger bi)
	{
		data = new uint[70];
		dataLength = bi.dataLength;
		for (int i = 0; i < dataLength; i++)
		{
			data[i] = bi.data[i];
		}
	}

	private SfBigInteger(uint[] inData)
	{
		dataLength = inData.Length;
		if (dataLength > 70)
		{
			throw new ArithmeticException("Byte overflow in constructor.");
		}
		data = new uint[70];
		int num = dataLength - 1;
		int num2 = 0;
		while (num >= 0)
		{
			data[num2] = inData[num];
			num--;
			num2++;
		}
		while (dataLength > 1 && data[dataLength - 1] == 0)
		{
			dataLength--;
		}
	}

	internal SfBigInteger(byte[] inData)
	{
		dataLength = inData.Length >> 2;
		int num = inData.Length & 3;
		if (num != 0)
		{
			dataLength++;
		}
		if (dataLength > 70)
		{
			throw new ArithmeticException("Byte overflow in constructor.");
		}
		data = new uint[70];
		int num2 = inData.Length - 1;
		int num3 = 0;
		while (num2 >= 3)
		{
			data[num3] = (uint)((inData[num2 - 3] << 24) + (inData[num2 - 2] << 16) + (inData[num2 - 1] << 8) + inData[num2]);
			num2 -= 4;
			num3++;
		}
		switch (num)
		{
		case 1:
			data[dataLength - 1] = inData[0];
			break;
		case 2:
			data[dataLength - 1] = (uint)((inData[0] << 8) + inData[1]);
			break;
		case 3:
			data[dataLength - 1] = (uint)((inData[0] << 16) + (inData[1] << 8) + inData[2]);
			break;
		}
		while (dataLength > 1 && data[dataLength - 1] == 0)
		{
			dataLength--;
		}
	}

	private SfBigInteger(ulong value)
	{
		data = new uint[70];
		dataLength = 0;
		while (value != 0L && dataLength < 70)
		{
			data[dataLength] = (uint)(value & 0xFFFFFFFFu);
			value >>= 32;
			dataLength++;
		}
		if (value != 0L || (data[69] & 0x80000000u) != 0)
		{
			throw new ArithmeticException("Positive overflow in constructor.");
		}
		if (dataLength == 0)
		{
			dataLength = 1;
		}
	}

	private long LongValue()
	{
		long num = 0L;
		num = data[0];
		try
		{
			num |= (long)((ulong)data[1] << 32);
		}
		catch (Exception)
		{
			if ((data[0] & 0x80000000u) != 0)
			{
				num = (int)data[0];
			}
		}
		return num;
	}

	internal int IntValue()
	{
		return (int)data[0];
	}

	public static bool operator <(SfBigInteger b1, SfBigInteger b2)
	{
		int num = 69;
		if ((b1.data[num] & 0x80000000u) != 0 && (b2.data[num] & 0x80000000u) == 0)
		{
			return true;
		}
		if ((b1.data[num] & 0x80000000u) == 0 && (b2.data[num] & 0x80000000u) != 0)
		{
			return false;
		}
		num = ((b1.dataLength > b2.dataLength) ? b1.dataLength : b2.dataLength) - 1;
		while (num >= 0 && b1.data[num] == b2.data[num])
		{
			num--;
		}
		if (num >= 0)
		{
			if (b1.data[num] < b2.data[num])
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public static SfBigInteger operator -(SfBigInteger b1)
	{
		if (b1.dataLength == 1 && b1.data[0] == 0)
		{
			return new SfBigInteger();
		}
		SfBigInteger sfBigInteger = new SfBigInteger(b1);
		for (int i = 0; i < 70; i++)
		{
			sfBigInteger.data[i] = ~b1.data[i];
		}
		long num = 1L;
		int num2 = 0;
		while (num != 0L && num2 < 70)
		{
			long num3 = sfBigInteger.data[num2];
			num3++;
			sfBigInteger.data[num2] = (uint)(num3 & 0xFFFFFFFFu);
			num = num3 >> 32;
			num2++;
		}
		if ((b1.data[69] & 0x80000000u) == (sfBigInteger.data[69] & 0x80000000u))
		{
			throw new ArithmeticException("Overflow in negation.\n");
		}
		sfBigInteger.dataLength = 70;
		while (sfBigInteger.dataLength > 1 && sfBigInteger.data[sfBigInteger.dataLength - 1] == 0)
		{
			sfBigInteger.dataLength--;
		}
		return sfBigInteger;
	}

	public static SfBigInteger operator %(SfBigInteger b1, SfBigInteger b2)
	{
		SfBigInteger outQuotient = new SfBigInteger();
		SfBigInteger sfBigInteger = new SfBigInteger(b1);
		int num = 69;
		bool flag = false;
		if ((b1.data[num] & 0x80000000u) != 0)
		{
			b1 = -b1;
			flag = true;
		}
		if ((b2.data[num] & 0x80000000u) != 0)
		{
			b2 = -b2;
		}
		if (b1 < b2)
		{
			return sfBigInteger;
		}
		if (b2.dataLength == 1)
		{
			SingleByteDivide(b1, b2, outQuotient, sfBigInteger);
		}
		else
		{
			MultiByteDivide(b1, b2, outQuotient, sfBigInteger);
		}
		if (flag)
		{
			return -sfBigInteger;
		}
		return sfBigInteger;
	}

	public static implicit operator SfBigInteger(long value)
	{
		return new SfBigInteger(value);
	}

	public static implicit operator SfBigInteger(ulong value)
	{
		return new SfBigInteger(value);
	}

	public static implicit operator SfBigInteger(int value)
	{
		return new SfBigInteger(value);
	}

	public static implicit operator SfBigInteger(uint value)
	{
		return new SfBigInteger((ulong)value);
	}

	public static SfBigInteger operator --(SfBigInteger b1)
	{
		SfBigInteger sfBigInteger = new SfBigInteger(b1);
		bool flag = true;
		int num = 0;
		while (flag && num < 70)
		{
			long num2 = sfBigInteger.data[num];
			num2--;
			sfBigInteger.data[num] = (uint)(num2 & 0xFFFFFFFFu);
			if (num2 >= 0)
			{
				flag = false;
			}
			num++;
		}
		if (num > sfBigInteger.dataLength)
		{
			sfBigInteger.dataLength = num;
		}
		while (sfBigInteger.dataLength > 1 && sfBigInteger.data[sfBigInteger.dataLength - 1] == 0)
		{
			sfBigInteger.dataLength--;
		}
		int num3 = 69;
		if ((b1.data[num3] & 0x80000000u) != 0 && (sfBigInteger.data[num3] & 0x80000000u) != (b1.data[num3] & 0x80000000u))
		{
			throw new ArithmeticException("Underflow in --.");
		}
		return sfBigInteger;
	}

	public static SfBigInteger operator <<(SfBigInteger b1, int shiftVal)
	{
		SfBigInteger sfBigInteger = new SfBigInteger(b1);
		sfBigInteger.dataLength = ShiftLeft(sfBigInteger.data, shiftVal);
		return sfBigInteger;
	}

	public static SfBigInteger operator *(SfBigInteger b1, SfBigInteger b2)
	{
		int num = 69;
		bool flag = false;
		bool flag2 = false;
		try
		{
			if ((b1.data[num] & 0x80000000u) != 0)
			{
				flag = true;
				b1 = -b1;
			}
			if ((b2.data[num] & 0x80000000u) != 0)
			{
				flag2 = true;
				b2 = -b2;
			}
		}
		catch (Exception)
		{
		}
		SfBigInteger sfBigInteger = new SfBigInteger();
		try
		{
			for (int i = 0; i < b1.dataLength; i++)
			{
				if (b1.data[i] != 0)
				{
					ulong num2 = 0uL;
					int num3 = 0;
					int num4 = i;
					while (num3 < b2.dataLength)
					{
						ulong num5 = (ulong)((long)b1.data[i] * (long)b2.data[num3] + sfBigInteger.data[num4]) + num2;
						sfBigInteger.data[num4] = (uint)(num5 & 0xFFFFFFFFu);
						num2 = num5 >> 32;
						num3++;
						num4++;
					}
					if (num2 != 0L)
					{
						sfBigInteger.data[i + b2.dataLength] = (uint)num2;
					}
				}
			}
		}
		catch (Exception)
		{
			throw new ArithmeticException("Multiplication overflow.");
		}
		sfBigInteger.dataLength = b1.dataLength + b2.dataLength;
		if (sfBigInteger.dataLength > 70)
		{
			sfBigInteger.dataLength = 70;
		}
		while (sfBigInteger.dataLength > 1 && sfBigInteger.data[sfBigInteger.dataLength - 1] == 0)
		{
			sfBigInteger.dataLength--;
		}
		if ((sfBigInteger.data[num] & 0x80000000u) != 0)
		{
			if (flag != flag2 && sfBigInteger.data[num] == 2147483648u)
			{
				if (sfBigInteger.dataLength == 1)
				{
					return sfBigInteger;
				}
				bool flag3 = true;
				for (int j = 0; j < sfBigInteger.dataLength - 1 && flag3; j++)
				{
					if (sfBigInteger.data[j] != 0)
					{
						flag3 = false;
					}
				}
				if (flag3)
				{
					return sfBigInteger;
				}
			}
			throw new ArithmeticException("Multiplication overflow.");
		}
		if (flag != flag2)
		{
			return -sfBigInteger;
		}
		return sfBigInteger;
	}

	private static void MultiByteDivide(SfBigInteger b1, SfBigInteger b2, SfBigInteger outQuotient, SfBigInteger outRemainder)
	{
		uint[] array = new uint[70];
		int num = b1.dataLength + 1;
		uint[] array2 = new uint[num];
		uint num2 = 2147483648u;
		uint num3 = b2.data[b2.dataLength - 1];
		int num4 = 0;
		int num5 = 0;
		while (num2 != 0 && (num3 & num2) == 0)
		{
			num4++;
			num2 >>= 1;
		}
		for (int i = 0; i < b1.dataLength; i++)
		{
			array2[i] = b1.data[i];
		}
		ShiftLeft(array2, num4);
		b2 <<= num4;
		int num6 = num - b2.dataLength;
		int num7 = num - 1;
		ulong num8 = b2.data[b2.dataLength - 1];
		ulong num9 = b2.data[b2.dataLength - 2];
		int num10 = b2.dataLength + 1;
		uint[] array3 = new uint[num10];
		while (num6 > 0)
		{
			ulong num11 = ((ulong)array2[num7] << 32) + array2[num7 - 1];
			ulong num12 = num11 / num8;
			ulong num13 = num11 % num8;
			bool flag = false;
			while (!flag)
			{
				flag = true;
				if (num12 == 4294967296L || num12 * num9 > (num13 << 32) + array2[num7 - 2])
				{
					num12--;
					num13 += num8;
					if (num13 < 4294967296L)
					{
						flag = false;
					}
				}
			}
			for (int j = 0; j < num10; j++)
			{
				array3[j] = array2[num7 - j];
			}
			SfBigInteger sfBigInteger = new SfBigInteger(array3);
			SfBigInteger sfBigInteger2;
			for (sfBigInteger2 = b2 * (long)num12; sfBigInteger2 > sfBigInteger; sfBigInteger2 -= b2)
			{
				num12--;
			}
			SfBigInteger sfBigInteger3 = sfBigInteger - sfBigInteger2;
			for (int k = 0; k < num10; k++)
			{
				array2[num7 - k] = sfBigInteger3.data[b2.dataLength - k];
			}
			array[num5++] = (uint)num12;
			num7--;
			num6--;
		}
		outQuotient.dataLength = num5;
		int l = 0;
		int num14 = outQuotient.dataLength - 1;
		while (num14 >= 0)
		{
			outQuotient.data[l] = array[num14];
			num14--;
			l++;
		}
		for (; l < 70; l++)
		{
			outQuotient.data[l] = 0u;
		}
		while (outQuotient.dataLength > 1 && outQuotient.data[outQuotient.dataLength - 1] == 0)
		{
			outQuotient.dataLength--;
		}
		if (outQuotient.dataLength == 0)
		{
			outQuotient.dataLength = 1;
		}
		outRemainder.dataLength = ShiftRight(array2, num4);
		for (l = 0; l < outRemainder.dataLength; l++)
		{
			outRemainder.data[l] = array2[l];
		}
		for (; l < 70; l++)
		{
			outRemainder.data[l] = 0u;
		}
	}

	public static SfBigInteger operator -(SfBigInteger b1, SfBigInteger b2)
	{
		SfBigInteger sfBigInteger = new SfBigInteger();
		sfBigInteger.dataLength = ((b1.dataLength > b2.dataLength) ? b1.dataLength : b2.dataLength);
		long num = 0L;
		for (int i = 0; i < sfBigInteger.dataLength; i++)
		{
			long num2 = (long)b1.data[i] - (long)b2.data[i] - num;
			sfBigInteger.data[i] = (uint)(num2 & 0xFFFFFFFFu);
			num = ((num2 >= 0) ? 0 : 1);
		}
		if (num != 0L)
		{
			for (int j = sfBigInteger.dataLength; j < 70; j++)
			{
				sfBigInteger.data[j] = uint.MaxValue;
			}
			sfBigInteger.dataLength = 70;
		}
		while (sfBigInteger.dataLength > 1 && sfBigInteger.data[sfBigInteger.dataLength - 1] == 0)
		{
			sfBigInteger.dataLength--;
		}
		int num3 = 69;
		if ((b1.data[num3] & 0x80000000u) != (b2.data[num3] & 0x80000000u) && (sfBigInteger.data[num3] & 0x80000000u) != (b1.data[num3] & 0x80000000u))
		{
			throw new ArithmeticException();
		}
		return sfBigInteger;
	}

	public static bool operator >(SfBigInteger b1, SfBigInteger b2)
	{
		int num = 69;
		if ((b1.data[num] & 0x80000000u) != 0 && (b2.data[num] & 0x80000000u) == 0)
		{
			return false;
		}
		if ((b1.data[num] & 0x80000000u) == 0 && (b2.data[num] & 0x80000000u) != 0)
		{
			return true;
		}
		num = ((b1.dataLength > b2.dataLength) ? b1.dataLength : b2.dataLength) - 1;
		while (num >= 0 && b1.data[num] == b2.data[num])
		{
			num--;
		}
		if (num >= 0)
		{
			if (b1.data[num] > b2.data[num])
			{
				return true;
			}
			return false;
		}
		return false;
	}

	private static int ShiftRight(uint[] buffer, int shiftVal)
	{
		int num = 32;
		int num2 = 0;
		int num3 = buffer.Length;
		while (num3 > 1 && buffer[num3 - 1] == 0)
		{
			num3--;
		}
		for (int num4 = shiftVal; num4 > 0; num4 -= num)
		{
			if (num4 < num)
			{
				num = num4;
				num2 = 32 - num;
			}
			ulong num5 = 0uL;
			for (int num6 = num3 - 1; num6 >= 0; num6--)
			{
				ulong num7 = (ulong)buffer[num6] >> num;
				num7 |= num5;
				num5 = (ulong)buffer[num6] << num2;
				buffer[num6] = (uint)num7;
			}
		}
		while (num3 > 1 && buffer[num3 - 1] == 0)
		{
			num3--;
		}
		return num3;
	}

	private static void SingleByteDivide(SfBigInteger b1, SfBigInteger b2, SfBigInteger outQuotient, SfBigInteger outRemainder)
	{
		uint[] array = new uint[70];
		int num = 0;
		for (int i = 0; i < 70; i++)
		{
			outRemainder.data[i] = b1.data[i];
		}
		outRemainder.dataLength = b1.dataLength;
		while (outRemainder.dataLength > 1 && outRemainder.data[outRemainder.dataLength - 1] == 0)
		{
			outRemainder.dataLength--;
		}
		ulong num2 = b2.data[0];
		int num3 = outRemainder.dataLength - 1;
		ulong num4 = outRemainder.data[num3];
		if (num4 >= num2)
		{
			ulong num5 = num4 / num2;
			array[num++] = (uint)num5;
			outRemainder.data[num3] = (uint)(num4 % num2);
		}
		num3--;
		while (num3 >= 0)
		{
			num4 = ((ulong)outRemainder.data[num3 + 1] << 32) + outRemainder.data[num3];
			ulong num6 = num4 / num2;
			array[num++] = (uint)num6;
			outRemainder.data[num3 + 1] = 0u;
			outRemainder.data[num3--] = (uint)(num4 % num2);
		}
		outQuotient.dataLength = num;
		int j = 0;
		int num7 = outQuotient.dataLength - 1;
		while (num7 >= 0)
		{
			outQuotient.data[j] = array[num7];
			num7--;
			j++;
		}
		for (; j < 70; j++)
		{
			outQuotient.data[j] = 0u;
		}
		while (outQuotient.dataLength > 1 && outQuotient.data[outQuotient.dataLength - 1] == 0)
		{
			outQuotient.dataLength--;
		}
		if (outQuotient.dataLength == 0)
		{
			outQuotient.dataLength = 1;
		}
		while (outRemainder.dataLength > 1 && outRemainder.data[outRemainder.dataLength - 1] == 0)
		{
			outRemainder.dataLength--;
		}
	}

	private static int ShiftLeft(uint[] buffer, int shiftVal)
	{
		int num = 32;
		int num2 = buffer.Length;
		while (num2 > 1 && buffer[num2 - 1] == 0)
		{
			num2--;
		}
		for (int num3 = shiftVal; num3 > 0; num3 -= num)
		{
			if (num3 < num)
			{
				num = num3;
			}
			ulong num4 = 0uL;
			for (int i = 0; i < num2; i++)
			{
				ulong num5 = (ulong)buffer[i] << num;
				num5 |= num4;
				buffer[i] = (uint)(num5 & 0xFFFFFFFFu);
				num4 = num5 >> 32;
			}
			if (num4 != 0L && num2 + 1 <= buffer.Length)
			{
				buffer[num2] = (uint)num4;
				num2++;
			}
		}
		return num2;
	}
}
