using System;
using System.Text;

namespace DocGen.Pdf.Security;

internal class PdfIntArray
{
	private int[] intValues;

	public int BitLength
	{
		get
		{
			int length = GetLength();
			if (length == 0)
			{
				return 0;
			}
			int num = length - 1;
			uint num2 = (uint)intValues[num];
			int num3 = (num << 5) + 1;
			if (num2 > 65535)
			{
				if (num2 > 16777215)
				{
					num3 += 24;
					num2 >>= 24;
				}
				else
				{
					num3 += 16;
					num2 >>= 16;
				}
			}
			else if (num2 > 255)
			{
				num3 += 8;
				num2 >>= 8;
			}
			while (num2 > 1)
			{
				num3++;
				num2 >>= 1;
			}
			return num3;
		}
	}

	public int Length => intValues.Length;

	public PdfIntArray(int length)
	{
		intValues = new int[length];
	}

	private PdfIntArray(int[] values)
	{
		intValues = values;
	}

	public PdfIntArray(Number bigInterger, int minimumLength)
	{
		if (bigInterger.SignValue == -1)
		{
			throw new ArgumentException("Only positive Integers allowed", "bigint");
		}
		if (bigInterger.SignValue == 0)
		{
			intValues = new int[1];
			return;
		}
		byte[] array = bigInterger.ToByteArrayUnsigned();
		int num = array.Length;
		int num2 = (num + 3) / 4;
		intValues = new int[Math.Max(num2, minimumLength)];
		int num3 = num % 4;
		int num4 = 0;
		if (0 < num3)
		{
			int num5 = array[num4++];
			while (num4 < num3)
			{
				num5 = (num5 << 8) | array[num4++];
			}
			intValues[--num2] = num5;
		}
		while (num2 > 0)
		{
			int num6 = array[num4++];
			for (int i = 1; i < 4; i++)
			{
				num6 = (num6 << 8) | array[num4++];
			}
			intValues[--num2] = num6;
		}
	}

	public int GetLength()
	{
		int num = intValues.Length;
		if (num < 1)
		{
			return 0;
		}
		if (intValues[0] != 0)
		{
			while (intValues[--num] == 0)
			{
			}
			return num + 1;
		}
		do
		{
			if (intValues[--num] != 0)
			{
				return num + 1;
			}
		}
		while (num > 0);
		return 0;
	}

	private int[] ResizedValues(int length)
	{
		int[] array = new int[length];
		int num = intValues.Length;
		int length2 = ((num < length) ? num : length);
		Array.Copy(intValues, 0, array, 0, length2);
		return array;
	}

	public Number ToBigInteger()
	{
		int length = GetLength();
		if (length == 0)
		{
			return Number.Zero;
		}
		int num = intValues[length - 1];
		byte[] array = new byte[4];
		int num2 = 0;
		bool flag = false;
		for (int num3 = 3; num3 >= 0; num3--)
		{
			byte b = (byte)((uint)num >> 8 * num3);
			if (flag || b != 0)
			{
				flag = true;
				array[num2++] = b;
			}
		}
		byte[] array2 = new byte[4 * (length - 1) + num2];
		for (int i = 0; i < num2; i++)
		{
			array2[i] = array[i];
		}
		for (int num4 = length - 2; num4 >= 0; num4--)
		{
			for (int num5 = 3; num5 >= 0; num5--)
			{
				array2[num2++] = (byte)((uint)intValues[num4] >> 8 * num5);
			}
		}
		return new Number(1, array2);
	}

	public void ShiftLeft()
	{
		int num = GetLength();
		if (num == 0)
		{
			return;
		}
		if (intValues[num - 1] < 0)
		{
			num++;
			if (num > intValues.Length)
			{
				intValues = ResizedValues(intValues.Length + 1);
			}
		}
		bool flag = false;
		for (int i = 0; i < num; i++)
		{
			bool num2 = intValues[i] < 0;
			intValues[i] <<= 1;
			if (flag)
			{
				intValues[i] |= 1;
			}
			flag = num2;
		}
	}

	public PdfIntArray ShiftLeft(int number)
	{
		int length = GetLength();
		if (length == 0)
		{
			return this;
		}
		if (number == 0)
		{
			return this;
		}
		if (number > 31)
		{
			throw new ArgumentException("bit shift is not possible");
		}
		int[] array = new int[length + 1];
		int num = 32 - number;
		array[0] = intValues[0] << number;
		for (int i = 1; i < length; i++)
		{
			array[i] = (intValues[i] << number) | (intValues[i - 1] >>> num);
		}
		array[length] = intValues[length - 1] >>> num;
		return new PdfIntArray(array);
	}

	public void AddShifted(PdfIntArray values, int shift)
	{
		int length = values.GetLength();
		int num = length + shift;
		if (num > intValues.Length)
		{
			intValues = ResizedValues(num);
		}
		for (int i = 0; i < length; i++)
		{
			intValues[i + shift] ^= values.intValues[i];
		}
	}

	public bool TestBit(int number)
	{
		int num = number >> 5;
		int num2 = number & 0x1F;
		int num3 = 1 << num2;
		return (intValues[num] & num3) != 0;
	}

	public void FlipBit(int number)
	{
		int num = number >> 5;
		int num2 = number & 0x1F;
		int num3 = 1 << num2;
		intValues[num] ^= num3;
	}

	public void SetBit(int number)
	{
		int num = number >> 5;
		int num2 = number & 0x1F;
		int num3 = 1 << num2;
		intValues[num] |= num3;
	}

	public PdfIntArray Multiply(PdfIntArray values, int value)
	{
		int num = value + 31 >> 5;
		if (intValues.Length < num)
		{
			intValues = ResizedValues(num);
		}
		PdfIntArray pdfIntArray = new PdfIntArray(values.ResizedValues(values.Length + 1));
		PdfIntArray pdfIntArray2 = new PdfIntArray(value + value + 31 >> 5);
		int num2 = 1;
		for (int i = 0; i < 32; i++)
		{
			for (int j = 0; j < num; j++)
			{
				if ((intValues[j] & num2) != 0)
				{
					pdfIntArray2.AddShifted(pdfIntArray, j);
				}
			}
			num2 <<= 1;
			pdfIntArray.ShiftLeft();
		}
		return pdfIntArray2;
	}

	public void Reduce(int value, int[] redPol)
	{
		for (int num = value + value - 2; num >= value; num--)
		{
			if (TestBit(num))
			{
				int num2 = num - value;
				FlipBit(num2);
				FlipBit(num);
				int num3 = redPol.Length;
				while (--num3 >= 0)
				{
					FlipBit(redPol[num3] + num2);
				}
			}
		}
		intValues = ResizedValues(value + 31 >> 5);
	}

	public PdfIntArray Square(int value)
	{
		int[] array = new int[16]
		{
			0, 1, 4, 5, 16, 17, 20, 21, 64, 65,
			68, 69, 80, 81, 84, 85
		};
		int num = value + 31 >> 5;
		if (intValues.Length < num)
		{
			intValues = ResizedValues(num);
		}
		PdfIntArray pdfIntArray = new PdfIntArray(num + num);
		for (int i = 0; i < num; i++)
		{
			int num2 = 0;
			for (int j = 0; j < 4; j++)
			{
				num2 >>>= 8;
				int num3 = (intValues[i] >>> j * 4) & 0xF;
				int num4 = array[num3] << 24;
				num2 |= num4;
			}
			pdfIntArray.intValues[i + i] = num2;
			num2 = 0;
			int num5 = intValues[i] >>> 16;
			for (int k = 0; k < 4; k++)
			{
				num2 >>>= 8;
				int num6 = (num5 >>> k * 4) & 0xF;
				int num7 = array[num6] << 24;
				num2 |= num7;
			}
			pdfIntArray.intValues[i + i + 1] = num2;
		}
		return pdfIntArray;
	}

	public override bool Equals(object o)
	{
		if (!(o is PdfIntArray))
		{
			return false;
		}
		PdfIntArray pdfIntArray = (PdfIntArray)o;
		int length = GetLength();
		if (pdfIntArray.GetLength() != length)
		{
			return false;
		}
		for (int i = 0; i < length; i++)
		{
			if (intValues[i] != pdfIntArray.intValues[i])
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		int num = GetLength();
		int num2 = num;
		while (--num >= 0)
		{
			num2 *= 17;
			num2 ^= intValues[num];
		}
		return num2;
	}

	internal PdfIntArray Copy()
	{
		return new PdfIntArray((int[])intValues.Clone());
	}

	public override string ToString()
	{
		int length = GetLength();
		if (length == 0)
		{
			return "0";
		}
		StringBuilder stringBuilder = new StringBuilder(Convert.ToString(intValues[length - 1], 2));
		for (int num = length - 2; num >= 0; num--)
		{
			string text = Convert.ToString(intValues[num], 2);
			for (int i = text.Length; i < 8; i++)
			{
				text = "0" + text;
			}
			stringBuilder.Append(text);
		}
		return stringBuilder.ToString();
	}
}
