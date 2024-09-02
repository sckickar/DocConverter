using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DocGen.Pdf.Security;

internal sealed class Number
{
	internal static readonly int[][] m_lists;

	internal static readonly int[] m_products;

	private const long m_iMask = 4294967295L;

	private const ulong m_uMask = 4294967295uL;

	private static readonly int[] m_zeroMagnitude;

	private static readonly byte[] m_zeroEncoding;

	private static readonly Number[] m_smallConstants;

	public static readonly Number Zero;

	public static readonly Number One;

	public static readonly Number Two;

	public static readonly Number Three;

	public static readonly Number Ten;

	private static readonly byte[] m_bitLengthTable;

	private const int m_c2 = 1;

	private const int m_c8 = 1;

	private const int m_c10 = 19;

	private const int m_c16 = 16;

	private static readonly Number m_r2;

	private static readonly Number m_r2E;

	private static readonly Number m_r8;

	private static readonly Number m_r8E;

	private static readonly Number m_r10;

	private static readonly Number m_r10E;

	private static readonly Number m_r16;

	private static readonly Number m_r16E;

	private static readonly SecureRandomAlgorithm m_rs;

	private static readonly int[] m_eT;

	private const int m_bByte = 8;

	private const int m_bInt = 32;

	private const int m_byteInt = 4;

	private int[] m_magnitude;

	private int m_sign;

	private int m_nBits = -1;

	private int m_nBitLength = -1;

	private int m_quote;

	internal int BitCount
	{
		get
		{
			if (m_nBits == -1)
			{
				if (m_sign < 0)
				{
					m_nBits = Not().BitCount;
				}
				else
				{
					int num = 0;
					for (int i = 0; i < m_magnitude.Length; i++)
					{
						num += BitCnt(m_magnitude[i]);
					}
					m_nBits = num;
				}
			}
			return m_nBits;
		}
	}

	internal int BitLength
	{
		get
		{
			if (m_nBitLength == -1)
			{
				m_nBitLength = ((m_sign != 0) ? CalcBitLength(m_sign, 0, m_magnitude) : 0);
			}
			return m_nBitLength;
		}
	}

	internal int IntValue
	{
		get
		{
			if (m_sign == 0)
			{
				return 0;
			}
			int num = m_magnitude.Length;
			int num2 = m_magnitude[num - 1];
			if (m_sign >= 0)
			{
				return num2;
			}
			return -num2;
		}
	}

	internal long LongValue
	{
		get
		{
			if (m_sign == 0)
			{
				return 0L;
			}
			int num = m_magnitude.Length;
			long num2 = m_magnitude[num - 1] & 0xFFFFFFFFu;
			if (num > 1)
			{
				num2 |= (m_magnitude[num - 2] & 0xFFFFFFFFu) << 32;
			}
			if (m_sign >= 0)
			{
				return num2;
			}
			return -num2;
		}
	}

	internal int SignValue => m_sign;

	static Number()
	{
		m_lists = new int[64][]
		{
			new int[8] { 3, 5, 7, 11, 13, 17, 19, 23 },
			new int[5] { 29, 31, 37, 41, 43 },
			new int[5] { 47, 53, 59, 61, 67 },
			new int[4] { 71, 73, 79, 83 },
			new int[4] { 89, 97, 101, 103 },
			new int[4] { 107, 109, 113, 127 },
			new int[4] { 131, 137, 139, 149 },
			new int[4] { 151, 157, 163, 167 },
			new int[4] { 173, 179, 181, 191 },
			new int[4] { 193, 197, 199, 211 },
			new int[3] { 223, 227, 229 },
			new int[3] { 233, 239, 241 },
			new int[3] { 251, 257, 263 },
			new int[3] { 269, 271, 277 },
			new int[3] { 281, 283, 293 },
			new int[3] { 307, 311, 313 },
			new int[3] { 317, 331, 337 },
			new int[3] { 347, 349, 353 },
			new int[3] { 359, 367, 373 },
			new int[3] { 379, 383, 389 },
			new int[3] { 397, 401, 409 },
			new int[3] { 419, 421, 431 },
			new int[3] { 433, 439, 443 },
			new int[3] { 449, 457, 461 },
			new int[3] { 463, 467, 479 },
			new int[3] { 487, 491, 499 },
			new int[3] { 503, 509, 521 },
			new int[3] { 523, 541, 547 },
			new int[3] { 557, 563, 569 },
			new int[3] { 571, 577, 587 },
			new int[3] { 593, 599, 601 },
			new int[3] { 607, 613, 617 },
			new int[3] { 619, 631, 641 },
			new int[3] { 643, 647, 653 },
			new int[3] { 659, 661, 673 },
			new int[3] { 677, 683, 691 },
			new int[3] { 701, 709, 719 },
			new int[3] { 727, 733, 739 },
			new int[3] { 743, 751, 757 },
			new int[3] { 761, 769, 773 },
			new int[3] { 787, 797, 809 },
			new int[3] { 811, 821, 823 },
			new int[3] { 827, 829, 839 },
			new int[3] { 853, 857, 859 },
			new int[3] { 863, 877, 881 },
			new int[3] { 883, 887, 907 },
			new int[3] { 911, 919, 929 },
			new int[3] { 937, 941, 947 },
			new int[3] { 953, 967, 971 },
			new int[3] { 977, 983, 991 },
			new int[3] { 997, 1009, 1013 },
			new int[3] { 1019, 1021, 1031 },
			new int[3] { 1033, 1039, 1049 },
			new int[3] { 1051, 1061, 1063 },
			new int[3] { 1069, 1087, 1091 },
			new int[3] { 1093, 1097, 1103 },
			new int[3] { 1109, 1117, 1123 },
			new int[3] { 1129, 1151, 1153 },
			new int[3] { 1163, 1171, 1181 },
			new int[3] { 1187, 1193, 1201 },
			new int[3] { 1213, 1217, 1223 },
			new int[3] { 1229, 1231, 1237 },
			new int[3] { 1249, 1259, 1277 },
			new int[3] { 1279, 1283, 1289 }
		};
		m_zeroMagnitude = new int[0];
		m_zeroEncoding = new byte[0];
		m_smallConstants = new Number[17];
		m_bitLengthTable = new byte[256]
		{
			0, 1, 2, 2, 3, 3, 3, 3, 4, 4,
			4, 4, 4, 4, 4, 4, 5, 5, 5, 5,
			5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
			5, 5, 6, 6, 6, 6, 6, 6, 6, 6,
			6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
			6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
			6, 6, 6, 6, 7, 7, 7, 7, 7, 7,
			7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
			7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
			7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
			7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
			7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
			7, 7, 7, 7, 7, 7, 7, 7, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8
		};
		m_rs = new SecureRandomAlgorithm();
		m_eT = new int[8] { 7, 25, 81, 241, 673, 1793, 4609, 2147483647 };
		Zero = new Number(0, m_zeroMagnitude, checkMag: false);
		Zero.m_nBits = 0;
		Zero.m_nBitLength = 0;
		m_smallConstants[0] = Zero;
		for (uint num = 1u; num < m_smallConstants.Length; num++)
		{
			m_smallConstants[num] = CreateUValueOf(num);
		}
		One = m_smallConstants[1];
		Two = m_smallConstants[2];
		Three = m_smallConstants[3];
		Ten = m_smallConstants[10];
		m_r2 = ValueOf(2L);
		m_r2E = m_r2.Pow(1);
		m_r8 = ValueOf(8L);
		m_r8E = m_r8.Pow(1);
		m_r10 = ValueOf(10L);
		m_r10E = m_r10.Pow(19);
		m_r16 = ValueOf(16L);
		m_r16E = m_r16.Pow(16);
		m_products = new int[m_lists.Length];
		for (int i = 0; i < m_lists.Length; i++)
		{
			int[] array = m_lists[i];
			int num2 = array[0];
			for (int j = 1; j < array.Length; j++)
			{
				num2 *= array[j];
			}
			m_products[i] = num2;
		}
	}

	private static int GetByteLength(int nBits)
	{
		return (nBits + 8 - 1) / 8;
	}

	private Number(int signum, int[] mag, bool checkMag)
	{
		if (checkMag)
		{
			int i;
			for (i = 0; i < mag.Length && mag[i] == 0; i++)
			{
			}
			if (i == mag.Length)
			{
				m_sign = 0;
				m_magnitude = m_zeroMagnitude;
				return;
			}
			m_sign = signum;
			if (i == 0)
			{
				m_magnitude = mag;
				return;
			}
			m_magnitude = new int[mag.Length - i];
			Array.Copy(mag, i, m_magnitude, 0, m_magnitude.Length);
		}
		else
		{
			m_sign = signum;
			m_magnitude = mag;
		}
	}

	internal Number(string value)
		: this(value, 10)
	{
	}

	internal Number(string str, int radix)
	{
		NumberStyles style;
		int num;
		Number number;
		Number val;
		switch (radix)
		{
		case 2:
			style = NumberStyles.Integer;
			num = 1;
			number = m_r2;
			val = m_r2E;
			break;
		case 8:
			style = NumberStyles.Integer;
			num = 1;
			number = m_r8;
			val = m_r8E;
			break;
		case 10:
			style = NumberStyles.Integer;
			num = 19;
			number = m_r10;
			val = m_r10E;
			break;
		case 16:
			style = NumberStyles.AllowHexSpecifier;
			num = 16;
			number = m_r16;
			val = m_r16E;
			break;
		default:
			throw new FormatException("Invalid base specified. Only bases 2, 8, 10, or 16 allowed");
		}
		int i = 0;
		m_sign = 1;
		if (str[0] == '-')
		{
			if (str.Length == 1)
			{
				throw new FormatException("Invalid length");
			}
			m_sign = -1;
			i = 1;
		}
		for (; i < str.Length && int.Parse(str[i].ToString(), style) == 0; i++)
		{
		}
		if (i >= str.Length)
		{
			m_sign = 0;
			m_magnitude = m_zeroMagnitude;
			return;
		}
		Number number2 = Zero;
		int num2 = i + num;
		if (num2 <= str.Length)
		{
			do
			{
				Number value = CreateUValueOf(ulong.Parse(str.Substring(i, num), style));
				number2 = (radix switch
				{
					2 => number2.ShiftLeft(1), 
					8 => number2.ShiftLeft(3), 
					16 => number2.ShiftLeft(64), 
					_ => number2.Multiply(val), 
				}).Add(value);
				i = num2;
				num2 += num;
			}
			while (num2 <= str.Length);
		}
		if (i < str.Length)
		{
			string text = str.Substring(i);
			Number number3 = CreateUValueOf(ulong.Parse(text, style));
			number2 = ((number2.m_sign <= 0) ? number3 : ((radix != 16) ? number2.Multiply(number.Pow(text.Length)) : number2.ShiftLeft(text.Length << 2)).Add(number3));
		}
		m_magnitude = number2.m_magnitude;
	}

	internal Number(byte[] bytes)
		: this(bytes, 0, bytes.Length)
	{
	}

	internal Number(byte[] bytes, int offset, int length)
	{
		if ((sbyte)bytes[offset] < 0)
		{
			m_sign = -1;
			int num = offset + length;
			int i;
			for (i = offset; i < num && (sbyte)bytes[i] == -1; i++)
			{
			}
			if (i >= num)
			{
				m_magnitude = One.m_magnitude;
				return;
			}
			int num2 = num - i;
			byte[] array = new byte[num2];
			int num3 = 0;
			while (num3 < num2)
			{
				array[num3++] = (byte)(~bytes[i++]);
			}
			while (array[--num3] == byte.MaxValue)
			{
				array[num3] = 0;
			}
			array[num3]++;
			m_magnitude = MakeMagnitude(array, 0, array.Length);
		}
		else
		{
			m_magnitude = MakeMagnitude(bytes, offset, length);
			m_sign = ((m_magnitude.Length != 0) ? 1 : 0);
		}
	}

	private static int[] MakeMagnitude(byte[] bytes, int offset, int length)
	{
		int num = offset + length;
		int i;
		for (i = offset; i < num && bytes[i] == 0; i++)
		{
		}
		if (i >= num)
		{
			return m_zeroMagnitude;
		}
		int num2 = (num - i + 3) / 4;
		int num3 = (num - i) % 4;
		if (num3 == 0)
		{
			num3 = 4;
		}
		if (num2 < 1)
		{
			return m_zeroMagnitude;
		}
		int[] array = new int[num2];
		int num4 = 0;
		int num5 = 0;
		for (int j = i; j < num; j++)
		{
			num4 <<= 8;
			num4 |= bytes[j] & 0xFF;
			num3--;
			if (num3 <= 0)
			{
				array[num5] = num4;
				num5++;
				num3 = 4;
				num4 = 0;
			}
		}
		if (num5 < array.Length)
		{
			array[num5] = num4;
		}
		return array;
	}

	internal Number(int sign, byte[] bytes)
		: this(sign, bytes, 0, bytes.Length)
	{
	}

	internal Number(int sign, byte[] bytes, int offset, int length)
	{
		switch (sign)
		{
		default:
			throw new FormatException("Invalid sign value");
		case 0:
			m_sign = 0;
			m_magnitude = m_zeroMagnitude;
			break;
		case -1:
		case 1:
			m_magnitude = MakeMagnitude(bytes, offset, length);
			m_sign = ((m_magnitude.Length >= 1) ? sign : 0);
			break;
		}
	}

	internal Number(int value, SecureRandomAlgorithm random)
	{
		if (value < 0)
		{
			throw new ArgumentException("Invalid entry. value must be non-negative");
		}
		m_nBits = -1;
		m_nBitLength = -1;
		if (value == 0)
		{
			m_sign = 0;
			m_magnitude = m_zeroMagnitude;
			return;
		}
		int byteLength = GetByteLength(value);
		byte[] array = new byte[byteLength];
		random.NextBytes(array);
		int num = 8 * byteLength - value;
		array[0] &= (byte)(255 >>> num);
		m_magnitude = MakeMagnitude(array, 0, array.Length);
		m_sign = ((m_magnitude.Length >= 1) ? 1 : 0);
	}

	internal Number Absolute()
	{
		if (m_sign < 0)
		{
			return Negate();
		}
		return this;
	}

	internal static int[] AddMagnitudes(int[] a, int[] b)
	{
		int num = a.Length - 1;
		int num2 = b.Length - 1;
		long num3 = 0L;
		while (num2 >= 0)
		{
			num3 += (long)(uint)a[num] + (long)(uint)b[num2--];
			a[num--] = (int)num3;
			num3 >>>= 32;
		}
		if (num3 != 0L)
		{
			while (num >= 0 && ++a[num--] == 0)
			{
			}
		}
		return a;
	}

	internal Number Add(Number value)
	{
		if (m_sign == 0)
		{
			return value;
		}
		if (m_sign != value.m_sign)
		{
			if (value.m_sign == 0)
			{
				return this;
			}
			if (value.m_sign < 0)
			{
				return Subtract(value.Negate());
			}
			return value.Subtract(Negate());
		}
		return AddToMagnitude(value.m_magnitude);
	}

	private Number AddToMagnitude(int[] magToAdd)
	{
		int[] array;
		int[] array2;
		if (m_magnitude.Length < magToAdd.Length)
		{
			array = magToAdd;
			array2 = m_magnitude;
		}
		else
		{
			array = m_magnitude;
			array2 = magToAdd;
		}
		uint num = uint.MaxValue;
		if (array.Length == array2.Length)
		{
			num -= (uint)array2[0];
		}
		bool flag = (uint)array[0] >= num;
		int[] array3;
		if (flag)
		{
			array3 = new int[array.Length + 1];
			array.CopyTo(array3, 1);
		}
		else
		{
			array3 = (int[])array.Clone();
		}
		array3 = AddMagnitudes(array3, array2);
		return new Number(m_sign, array3, flag);
	}

	internal static int BitCnt(int i)
	{
		uint num = (uint)i;
		num -= (num >> 1) & 0x55555555;
		num = (num & 0x33333333) + ((num >> 2) & 0x33333333);
		num = (num + (num >> 4)) & 0xF0F0F0Fu;
		num += num >> 8;
		num += num >> 16;
		return (int)(num & 0x3F);
	}

	private static int CalcBitLength(int sign, int indx, int[] mag)
	{
		while (true)
		{
			if (indx >= mag.Length)
			{
				return 0;
			}
			if (mag[indx] != 0)
			{
				break;
			}
			indx++;
		}
		int num = 32 * (mag.Length - indx - 1);
		int num2 = mag[indx];
		num += BitLen(num2);
		if (sign < 0 && (num2 & -num2) == num2)
		{
			do
			{
				if (++indx >= mag.Length)
				{
					num--;
					break;
				}
			}
			while (mag[indx] == 0);
		}
		return num;
	}

	private static int BitLen(int w)
	{
		uint num = (uint)w >> 24;
		if (num != 0)
		{
			return 24 + m_bitLengthTable[num];
		}
		num = (uint)w >> 16;
		if (num != 0)
		{
			return 16 + m_bitLengthTable[num];
		}
		num = (uint)w >> 8;
		if (num != 0)
		{
			return 8 + m_bitLengthTable[num];
		}
		return m_bitLengthTable[w];
	}

	private bool QuickPow2Check()
	{
		if (m_sign > 0)
		{
			return m_nBits == 1;
		}
		return false;
	}

	internal int CompareTo(object obj)
	{
		return CompareTo((Number)obj);
	}

	private static int CompareTo(int xIndx, int[] x, int yIndx, int[] y)
	{
		while (xIndx != x.Length && x[xIndx] == 0)
		{
			xIndx++;
		}
		while (yIndx != y.Length && y[yIndx] == 0)
		{
			yIndx++;
		}
		return CompareNoLeadingZeroes(xIndx, x, yIndx, y);
	}

	private static int CompareNoLeadingZeroes(int xIndx, int[] x, int yIndx, int[] y)
	{
		int num = x.Length - y.Length - (xIndx - yIndx);
		if (num != 0)
		{
			if (num >= 0)
			{
				return 1;
			}
			return -1;
		}
		while (xIndx < x.Length)
		{
			uint num2 = (uint)x[xIndx++];
			uint num3 = (uint)y[yIndx++];
			if (num2 != num3)
			{
				if (num2 >= num3)
				{
					return 1;
				}
				return -1;
			}
		}
		return 0;
	}

	internal int CompareTo(Number value)
	{
		if (m_sign >= value.m_sign)
		{
			if (m_sign <= value.m_sign)
			{
				if (m_sign != 0)
				{
					return m_sign * CompareNoLeadingZeroes(0, m_magnitude, 0, value.m_magnitude);
				}
				return 0;
			}
			return 1;
		}
		return -1;
	}

	private int[] Divide(int[] x, int[] y)
	{
		int i;
		for (i = 0; i < x.Length && x[i] == 0; i++)
		{
		}
		int j;
		for (j = 0; j < y.Length && y[j] == 0; j++)
		{
		}
		int num = CompareNoLeadingZeroes(i, x, j, y);
		int[] array3;
		if (num > 0)
		{
			int num2 = CalcBitLength(1, j, y);
			int num3 = CalcBitLength(1, i, x);
			int num4 = num3 - num2;
			int k = 0;
			int l = 0;
			int num5 = num2;
			int[] array;
			int[] array2;
			if (num4 > 0)
			{
				array = new int[(num4 >> 5) + 1];
				array[0] = 1 << num4 % 32;
				array2 = ShiftLeft(y, num4);
				num5 += num4;
			}
			else
			{
				array = new int[1] { 1 };
				int num6 = y.Length - j;
				array2 = new int[num6];
				Array.Copy(y, j, array2, 0, num6);
			}
			array3 = new int[array.Length];
			while (true)
			{
				if (num5 < num3 || CompareNoLeadingZeroes(i, x, l, array2) >= 0)
				{
					Subtract(i, x, l, array2);
					AddMagnitudes(array3, array);
					while (x[i] == 0)
					{
						if (++i == x.Length)
						{
							return array3;
						}
					}
					num3 = 32 * (x.Length - i - 1) + BitLen(x[i]);
					if (num3 <= num2)
					{
						if (num3 < num2)
						{
							return array3;
						}
						num = CompareNoLeadingZeroes(i, x, j, y);
						if (num <= 0)
						{
							break;
						}
					}
				}
				num4 = num5 - num3;
				if (num4 == 1)
				{
					int num7 = array2[l] >>> 1;
					uint num8 = (uint)x[i];
					if ((uint)num7 > num8)
					{
						num4++;
					}
				}
				if (num4 < 2)
				{
					ShiftRightOneInPlace(l, array2);
					num5--;
					ShiftRightOneInPlace(k, array);
				}
				else
				{
					ShiftRightInPlace(l, array2, num4);
					num5 -= num4;
					ShiftRightInPlace(k, array, num4);
				}
				for (; array2[l] == 0; l++)
				{
				}
				for (; array[k] == 0; k++)
				{
				}
			}
		}
		else
		{
			array3 = new int[1];
		}
		if (num == 0)
		{
			AddMagnitudes(array3, One.m_magnitude);
			Array.Clear(x, i, x.Length - i);
		}
		return array3;
	}

	internal Number Divide(Number value)
	{
		if (value.m_sign == 0)
		{
			throw new ArithmeticException("Invalid value. Division by zero error");
		}
		if (m_sign == 0)
		{
			return Zero;
		}
		if (value.QuickPow2Check())
		{
			Number number = Absolute().ShiftRight(value.Absolute().BitLength - 1);
			if (value.m_sign != m_sign)
			{
				return number.Negate();
			}
			return number;
		}
		int[] x = (int[])m_magnitude.Clone();
		return new Number(m_sign * value.m_sign, Divide(x, value.m_magnitude), checkMag: true);
	}

	internal Number[] DivideAndRemainder(Number value)
	{
		if (value.m_sign == 0)
		{
			throw new ArithmeticException("Invalid value. Division by zero error");
		}
		Number[] array = new Number[2];
		if (m_sign == 0)
		{
			array[0] = Zero;
			array[1] = Zero;
		}
		else if (value.QuickPow2Check())
		{
			int n = value.Absolute().BitLength - 1;
			Number number = Absolute().ShiftRight(n);
			int[] mag = LastNBits(n);
			array[0] = ((value.m_sign == m_sign) ? number : number.Negate());
			array[1] = new Number(m_sign, mag, checkMag: true);
		}
		else
		{
			int[] array2 = (int[])m_magnitude.Clone();
			int[] mag2 = Divide(array2, value.m_magnitude);
			array[0] = new Number(m_sign * value.m_sign, mag2, checkMag: true);
			array[1] = new Number(m_sign, array2, checkMag: true);
		}
		return array;
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is Number number))
		{
			return false;
		}
		if (m_sign == number.m_sign)
		{
			return IsEqualMagnitude(number);
		}
		return false;
	}

	private bool IsEqualMagnitude(Number x)
	{
		_ = x.m_magnitude;
		if (m_magnitude.Length != x.m_magnitude.Length)
		{
			return false;
		}
		for (int i = 0; i < m_magnitude.Length; i++)
		{
			if (m_magnitude[i] != x.m_magnitude[i])
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		int num = m_magnitude.Length;
		if (m_magnitude.Length != 0)
		{
			num ^= m_magnitude[0];
			if (m_magnitude.Length > 1)
			{
				num ^= m_magnitude[m_magnitude.Length - 1];
			}
		}
		if (m_sign >= 0)
		{
			return num;
		}
		return ~num;
	}

	private Number Inc()
	{
		if (m_sign == 0)
		{
			return One;
		}
		if (m_sign < 0)
		{
			return new Number(-1, doSubBigLil(m_magnitude, One.m_magnitude), checkMag: true);
		}
		return AddToMagnitude(One.m_magnitude);
	}

	internal bool IsProbablePrime(int certainty)
	{
		if (certainty <= 0)
		{
			return true;
		}
		Number number = Absolute();
		if (!number.TestBit(0))
		{
			return number.Equals(Two);
		}
		if (number.Equals(One))
		{
			return false;
		}
		return number.CheckProbablePrime(certainty, m_rs);
	}

	private bool CheckProbablePrime(int certainty, SecureRandomAlgorithm random)
	{
		int num = Math.Min(BitLength - 1, m_lists.Length);
		for (int i = 0; i < num; i++)
		{
			int num2 = Remainder(m_products[i]);
			int[] array = m_lists[i];
			foreach (int num3 in array)
			{
				if (num2 % num3 == 0)
				{
					if (BitLength < 16)
					{
						return IntValue == num3;
					}
					return false;
				}
			}
		}
		return RabinMillerTest(certainty, random);
	}

	internal bool RabinMillerTest(int certainty, SecureRandomAlgorithm random)
	{
		int lowestSetBitMaskFirst = GetLowestSetBitMaskFirst(-2);
		Number e = ShiftRight(lowestSetBitMaskFirst);
		Number number = One.ShiftLeft(32 * m_magnitude.Length).Remainder(this);
		Number number2 = Subtract(number);
		while (true)
		{
			Number number3 = new Number(BitLength, random);
			if (number3.m_sign == 0 || number3.CompareTo(this) >= 0 || number3.IsEqualMagnitude(number) || number3.IsEqualMagnitude(number2))
			{
				continue;
			}
			Number number4 = ModPowMonty(number3, e, this, convert: false);
			if (!number4.Equals(number))
			{
				int num = 0;
				while (!number4.Equals(number2))
				{
					if (++num == lowestSetBitMaskFirst)
					{
						return false;
					}
					number4 = ModPowMonty(number4, Two, this, convert: false);
					if (number4.Equals(number))
					{
						return false;
					}
				}
			}
			certainty -= 2;
			if (certainty <= 0)
			{
				break;
			}
		}
		return true;
	}

	internal Number Mod(Number m)
	{
		Number number = Remainder(m);
		if (number.m_sign < 0)
		{
			return number.Add(m);
		}
		return number;
	}

	internal Number ModInverse(Number m)
	{
		ExtEuclid(Remainder(m), m, out Number u1Out);
		if (u1Out.m_sign < 0)
		{
			return u1Out.Add(m);
		}
		return u1Out;
	}

	private static int ModInverse32(int d)
	{
		int num = d + (((d + 1) & 4) << 1);
		num *= 2 - d * num;
		num *= 2 - d * num;
		return num * (2 - d * num);
	}

	private static Number ExtEuclid(Number a, Number b, out Number u1Out)
	{
		Number number = One;
		Number number2 = a;
		Number number3 = Zero;
		Number number4 = b;
		while (number4.m_sign > 0)
		{
			Number[] array = number2.DivideAndRemainder(number4);
			Number n = number3.Multiply(array[0]);
			Number number5 = number.Subtract(n);
			number = number3;
			number3 = number5;
			number2 = number4;
			number4 = array[1];
		}
		u1Out = number;
		return number2;
	}

	internal Number ModPow(Number e, Number m)
	{
		if (m.m_sign < 1)
		{
			throw new ArithmeticException("Invalid modulus. Negative value identified");
		}
		if (m.Equals(One))
		{
			return Zero;
		}
		if (e.m_sign == 0)
		{
			return One;
		}
		if (m_sign == 0)
		{
			return Zero;
		}
		bool num = e.m_sign < 0;
		if (num)
		{
			e = e.Negate();
		}
		Number number = Mod(m);
		if (!e.Equals(One))
		{
			number = ((((uint)m.m_magnitude[m.m_magnitude.Length - 1] & (true ? 1u : 0u)) != 0) ? ModPowMonty(number, e, m, convert: true) : ModPowBarrett(number, e, m));
		}
		if (num)
		{
			number = number.ModInverse(m);
		}
		return number;
	}

	private static Number ModPowBarrett(Number b, Number e, Number m)
	{
		int num = m.m_magnitude.Length;
		Number mr = One.ShiftLeft(num + 1 << 5);
		Number yu = One.ShiftLeft(num << 6).Divide(m);
		int i = 0;
		for (int bitLength = e.BitLength; bitLength > m_eT[i]; i++)
		{
		}
		int num2 = 1 << i;
		Number[] array = new Number[num2];
		array[0] = b;
		Number number = ReduceBarrett(b.Square(), m, mr, yu);
		for (int j = 1; j < num2; j++)
		{
			array[j] = ReduceBarrett(array[j - 1].Multiply(number), m, mr, yu);
		}
		int[] windowList = GetWindowList(e.m_magnitude, i);
		int num3 = windowList[0];
		int num4 = num3 & 0xFF;
		int num5 = num3 >> 8;
		Number number2;
		if (num4 == 1)
		{
			number2 = number;
			num5--;
		}
		else
		{
			number2 = array[num4 >> 1];
		}
		int num6 = 1;
		while ((num3 = windowList[num6++]) != -1)
		{
			num4 = num3 & 0xFF;
			int num7 = num5 + m_bitLengthTable[num4];
			for (int k = 0; k < num7; k++)
			{
				number2 = ReduceBarrett(number2.Square(), m, mr, yu);
			}
			number2 = ReduceBarrett(number2.Multiply(array[num4 >> 1]), m, mr, yu);
			num5 = num3 >> 8;
		}
		for (int l = 0; l < num5; l++)
		{
			number2 = ReduceBarrett(number2.Square(), m, mr, yu);
		}
		return number2;
	}

	private static Number ReduceBarrett(Number x, Number m, Number mr, Number yu)
	{
		int bitLength = x.BitLength;
		int bitLength2 = m.BitLength;
		if (bitLength < bitLength2)
		{
			return x;
		}
		if (bitLength - bitLength2 > 1)
		{
			int num = m.m_magnitude.Length;
			Number number = x.DivideWords(num - 1).Multiply(yu).DivideWords(num + 1);
			Number number2 = x.RemainderWords(num + 1);
			Number n = number.Multiply(m).RemainderWords(num + 1);
			x = number2.Subtract(n);
			if (x.m_sign < 0)
			{
				x = x.Add(mr);
			}
		}
		while (x.CompareTo(m) >= 0)
		{
			x = x.Subtract(m);
		}
		return x;
	}

	private static Number ModPowMonty(Number b, Number e, Number m, bool convert)
	{
		int num = m.m_magnitude.Length;
		int num2 = 32 * num;
		bool flag = m.BitLength + 2 <= num2;
		uint mQuote = (uint)m.GetMQuote();
		if (convert)
		{
			b = b.ShiftLeft(num2).Remainder(m);
		}
		int[] a = new int[num + 1];
		int[] array = b.m_magnitude;
		if (array.Length < num)
		{
			int[] array2 = new int[num];
			array.CopyTo(array2, num - array.Length);
			array = array2;
		}
		int i = 0;
		if (e.m_magnitude.Length > 1 || e.BitCount > 2)
		{
			for (int bitLength = e.BitLength; bitLength > m_eT[i]; i++)
			{
			}
		}
		int num3 = 1 << i;
		int[][] array3 = new int[num3][];
		array3[0] = array;
		int[] array4 = Asn1Constants.Clone(array);
		SquareMonty(a, array4, m.m_magnitude, mQuote, flag);
		for (int j = 1; j < num3; j++)
		{
			array3[j] = Asn1Constants.Clone(array3[j - 1]);
			MultiplyMonty(a, array3[j], array4, m.m_magnitude, mQuote, flag);
		}
		int[] windowList = GetWindowList(e.m_magnitude, i);
		int num4 = windowList[0];
		int num5 = num4 & 0xFF;
		int num6 = num4 >> 8;
		int[] array5;
		if (num5 == 1)
		{
			array5 = array4;
			num6--;
		}
		else
		{
			array5 = Asn1Constants.Clone(array3[num5 >> 1]);
		}
		int num7 = 1;
		while ((num4 = windowList[num7++]) != -1)
		{
			num5 = num4 & 0xFF;
			int num8 = num6 + m_bitLengthTable[num5];
			for (int k = 0; k < num8; k++)
			{
				SquareMonty(a, array5, m.m_magnitude, mQuote, flag);
			}
			MultiplyMonty(a, array5, array3[num5 >> 1], m.m_magnitude, mQuote, flag);
			num6 = num4 >> 8;
		}
		for (int l = 0; l < num6; l++)
		{
			SquareMonty(a, array5, m.m_magnitude, mQuote, flag);
		}
		if (convert)
		{
			MontgomeryReduce(array5, m.m_magnitude, mQuote);
		}
		else if (flag && CompareTo(0, array5, 0, m.m_magnitude) >= 0)
		{
			Subtract(0, array5, 0, m.m_magnitude);
		}
		return new Number(1, array5, checkMag: true);
	}

	private static int[] GetWindowList(int[] mag, int extraBits)
	{
		int num = mag[0];
		int num2 = BitLen(num);
		int[] array = new int[((mag.Length - 1 << 5) + num2) / (1 + extraBits) + 2];
		int num3 = 0;
		int num4 = 33 - num2;
		num <<= num4;
		int num5 = 1;
		int num6 = 1 << extraBits;
		int num7 = 0;
		int num8 = 0;
		while (true)
		{
			if (num4 < 32)
			{
				if (num5 < num6)
				{
					num5 = (num5 << 1) | (num >>> 31);
				}
				else if (num < 0)
				{
					array[num3++] = CreateWindowEntry(num5, num7);
					num5 = 1;
					num7 = 0;
				}
				else
				{
					num7++;
				}
				num <<= 1;
				num4++;
			}
			else
			{
				if (++num8 == mag.Length)
				{
					break;
				}
				num = mag[num8];
				num4 = 0;
			}
		}
		array[num3++] = CreateWindowEntry(num5, num7);
		array[num3] = -1;
		return array;
	}

	private static int CreateWindowEntry(int mult, int zeroes)
	{
		while ((mult & 1) == 0)
		{
			mult >>= 1;
			zeroes++;
		}
		return mult | (zeroes << 8);
	}

	private static int[] Square(int[] w, int[] x)
	{
		int num = w.Length - 1;
		ulong num4;
		for (int num2 = x.Length - 1; num2 > 0; num2--)
		{
			ulong num3 = (uint)x[num2];
			num4 = num3 * num3 + (uint)w[num];
			w[num] = (int)num4;
			num4 >>= 32;
			for (int num5 = num2 - 1; num5 >= 0; num5--)
			{
				ulong num6 = num3 * (uint)x[num5];
				num4 += ((ulong)(uint)w[--num] & 0xFFFFFFFFuL) + (uint)((int)num6 << 1);
				w[num] = (int)num4;
				num4 = (num4 >> 32) + (num6 >> 31);
			}
			num4 += (uint)w[--num];
			w[num] = (int)num4;
			if (--num >= 0)
			{
				w[num] = (int)(num4 >> 32);
			}
			num += num2;
		}
		num4 = (uint)x[0];
		num4 = num4 * num4 + (uint)w[num];
		w[num] = (int)num4;
		if (--num >= 0)
		{
			w[num] += (int)(num4 >> 32);
		}
		return w;
	}

	private static int[] Multiply(int[] x, int[] y, int[] z)
	{
		int num = z.Length;
		if (num < 1)
		{
			return x;
		}
		int num2 = x.Length - y.Length;
		do
		{
			long num3 = z[--num] & 0xFFFFFFFFu;
			long num4 = 0L;
			if (num3 != 0L)
			{
				for (int num5 = y.Length - 1; num5 >= 0; num5--)
				{
					num4 += num3 * (y[num5] & 0xFFFFFFFFu) + (x[num2 + num5] & 0xFFFFFFFFu);
					x[num2 + num5] = (int)num4;
					num4 >>>= 32;
				}
			}
			num2--;
			if (num2 >= 0)
			{
				x[num2] = (int)num4;
			}
		}
		while (num > 0);
		return x;
	}

	private int GetMQuote()
	{
		if (m_quote != 0)
		{
			return m_quote;
		}
		int d = -m_magnitude[m_magnitude.Length - 1];
		return m_quote = ModInverse32(d);
	}

	private static void MontgomeryReduce(int[] x, int[] m, uint mDash)
	{
		int num = m.Length;
		for (int num2 = num - 1; num2 >= 0; num2--)
		{
			uint num3 = (uint)x[num - 1];
			ulong num4 = num3 * mDash;
			ulong num5 = num4 * (uint)m[num - 1] + num3;
			num5 >>= 32;
			for (int num6 = num - 2; num6 >= 0; num6--)
			{
				num5 += num4 * (uint)m[num6] + (uint)x[num6];
				x[num6 + 1] = (int)num5;
				num5 >>= 32;
			}
			x[0] = (int)num5;
		}
		if (CompareTo(0, x, 0, m) >= 0)
		{
			Subtract(0, x, 0, m);
		}
	}

	private static void MultiplyMonty(int[] a, int[] x, int[] y, int[] m, uint mDash, bool smallMontyModulus)
	{
		int num = m.Length;
		if (num == 1)
		{
			x[0] = (int)MultiplyMontyNIsOne((uint)x[0], (uint)y[0], (uint)m[0], mDash);
			return;
		}
		uint num2 = (uint)y[num - 1];
		ulong num3 = (uint)x[num - 1];
		ulong num4 = num3 * num2;
		ulong num5 = (uint)(int)num4 * mDash;
		ulong num6 = num5 * (uint)m[num - 1];
		num4 += (uint)num6;
		num4 = (num4 >> 32) + (num6 >> 32);
		for (int num7 = num - 2; num7 >= 0; num7--)
		{
			ulong num8 = num3 * (uint)y[num7];
			num6 = num5 * (uint)m[num7];
			num4 += (num8 & 0xFFFFFFFFu) + (uint)num6;
			a[num7 + 2] = (int)num4;
			num4 = (num4 >> 32) + (num8 >> 32) + (num6 >> 32);
		}
		a[1] = (int)num4;
		a[0] = (int)(num4 >> 32);
		for (int num9 = num - 2; num9 >= 0; num9--)
		{
			uint num10 = (uint)a[num];
			ulong num11 = (uint)x[num9];
			ulong num12 = num11 * num2;
			ulong num13 = (num12 & 0xFFFFFFFFu) + num10;
			ulong num14 = (uint)(int)num13 * mDash;
			ulong num15 = num14 * (uint)m[num - 1];
			num13 += (uint)num15;
			num13 = (num13 >> 32) + (num12 >> 32) + (num15 >> 32);
			for (int num16 = num - 2; num16 >= 0; num16--)
			{
				num12 = num11 * (uint)y[num16];
				num15 = num14 * (uint)m[num16];
				num13 += (num12 & 0xFFFFFFFFu) + (uint)num15 + (uint)a[num16 + 1];
				a[num16 + 2] = (int)num13;
				num13 = (num13 >> 32) + (num12 >> 32) + (num15 >> 32);
			}
			num13 += (uint)a[0];
			a[1] = (int)num13;
			a[0] = (int)(num13 >> 32);
		}
		if (!smallMontyModulus && CompareTo(0, a, 0, m) >= 0)
		{
			Subtract(0, a, 0, m);
		}
		Array.Copy(a, 1, x, 0, num);
	}

	private static void SquareMonty(int[] a, int[] x, int[] m, uint mDash, bool smallMontyModulus)
	{
		int num = m.Length;
		if (num == 1)
		{
			uint num2 = (uint)x[0];
			x[0] = (int)MultiplyMontyNIsOne(num2, num2, (uint)m[0], mDash);
			return;
		}
		ulong num3 = (uint)x[num - 1];
		ulong num4 = num3 * num3;
		ulong num5 = (uint)(int)num4 * mDash;
		ulong num6 = num5 * (uint)m[num - 1];
		num4 += (uint)num6;
		num4 = (num4 >> 32) + (num6 >> 32);
		for (int num7 = num - 2; num7 >= 0; num7--)
		{
			ulong num8 = num3 * (uint)x[num7];
			num6 = num5 * (uint)m[num7];
			num4 += (num6 & 0xFFFFFFFFu) + (uint)((int)num8 << 1);
			a[num7 + 2] = (int)num4;
			num4 = (num4 >> 32) + (num8 >> 31) + (num6 >> 32);
		}
		a[1] = (int)num4;
		a[0] = (int)(num4 >> 32);
		for (int num9 = num - 2; num9 >= 0; num9--)
		{
			uint num10 = (uint)a[num];
			ulong num11 = num10 * mDash;
			ulong num12 = num11 * (uint)m[num - 1] + num10;
			num12 >>= 32;
			for (int num13 = num - 2; num13 > num9; num13--)
			{
				num12 += num11 * (uint)m[num13] + (uint)a[num13 + 1];
				a[num13 + 2] = (int)num12;
				num12 >>= 32;
			}
			ulong num14 = (uint)x[num9];
			ulong num15 = num14 * num14;
			ulong num16 = num11 * (uint)m[num9];
			num12 += (num15 & 0xFFFFFFFFu) + (uint)num16 + (uint)a[num9 + 1];
			a[num9 + 2] = (int)num12;
			num12 = (num12 >> 32) + (num15 >> 32) + (num16 >> 32);
			for (int num17 = num9 - 1; num17 >= 0; num17--)
			{
				ulong num18 = num14 * (uint)x[num17];
				ulong num19 = num11 * (uint)m[num17];
				num12 += (num19 & 0xFFFFFFFFu) + (uint)((int)num18 << 1) + (uint)a[num17 + 1];
				a[num17 + 2] = (int)num12;
				num12 = (num12 >> 32) + (num18 >> 31) + (num19 >> 32);
			}
			num12 += (uint)a[0];
			a[1] = (int)num12;
			a[0] = (int)(num12 >> 32);
		}
		if (!smallMontyModulus && CompareTo(0, a, 0, m) >= 0)
		{
			Subtract(0, a, 0, m);
		}
		Array.Copy(a, 1, x, 0, num);
	}

	private static uint MultiplyMontyNIsOne(uint x, uint y, uint m, uint mDash)
	{
		ulong num = (ulong)x * (ulong)y;
		uint num2 = (uint)(int)num * mDash;
		ulong num3 = m;
		ulong num4 = num3 * num2;
		num += (uint)num4;
		num = (num >> 32) + (num4 >> 32);
		if (num > num3)
		{
			num -= num3;
		}
		return (uint)num;
	}

	internal Number Multiply(Number val)
	{
		if (val == this)
		{
			return Square();
		}
		if ((m_sign & val.m_sign) == 0)
		{
			return Zero;
		}
		if (val.QuickPow2Check())
		{
			Number number = ShiftLeft(val.Absolute().BitLength - 1);
			if (val.m_sign <= 0)
			{
				return number.Negate();
			}
			return number;
		}
		if (QuickPow2Check())
		{
			Number number2 = val.ShiftLeft(Absolute().BitLength - 1);
			if (m_sign <= 0)
			{
				return number2.Negate();
			}
			return number2;
		}
		int[] array = new int[m_magnitude.Length + val.m_magnitude.Length];
		Multiply(array, m_magnitude, val.m_magnitude);
		return new Number(m_sign ^ val.m_sign ^ 1, array, checkMag: true);
	}

	internal Number Square()
	{
		if (m_sign == 0)
		{
			return Zero;
		}
		if (QuickPow2Check())
		{
			return ShiftLeft(Absolute().BitLength - 1);
		}
		int num = m_magnitude.Length << 1;
		if (m_magnitude[0] >>> 16 == 0)
		{
			num--;
		}
		int[] array = new int[num];
		Square(array, m_magnitude);
		return new Number(1, array, checkMag: false);
	}

	internal Number Negate()
	{
		if (m_sign == 0)
		{
			return this;
		}
		return new Number(-m_sign, m_magnitude, checkMag: false);
	}

	internal Number Not()
	{
		return Inc().Negate();
	}

	internal Number Pow(int exp)
	{
		if (exp <= 0)
		{
			if (exp < 0)
			{
				throw new ArithmeticException("Invalid exponent. Negative value identified");
			}
			return One;
		}
		if (m_sign == 0)
		{
			return this;
		}
		if (QuickPow2Check())
		{
			long num = (long)exp * (long)(BitLength - 1);
			if (num > int.MaxValue)
			{
				throw new ArithmeticException("Result too large");
			}
			return One.ShiftLeft((int)num);
		}
		Number number = One;
		Number number2 = this;
		while (true)
		{
			if ((exp & 1) == 1)
			{
				number = number.Multiply(number2);
			}
			exp >>= 1;
			if (exp == 0)
			{
				break;
			}
			number2 = number2.Multiply(number2);
		}
		return number;
	}

	private int Remainder(int m)
	{
		long num = 0L;
		for (int i = 0; i < m_magnitude.Length; i++)
		{
			long num2 = (uint)m_magnitude[i];
			num = ((num << 32) | num2) % m;
		}
		return (int)num;
	}

	private static int[] Remainder(int[] x, int[] y)
	{
		int i;
		for (i = 0; i < x.Length && x[i] == 0; i++)
		{
		}
		int j;
		for (j = 0; j < y.Length && y[j] == 0; j++)
		{
		}
		int num = CompareNoLeadingZeroes(i, x, j, y);
		if (num > 0)
		{
			int num2 = CalcBitLength(1, j, y);
			int num3 = CalcBitLength(1, i, x);
			int num4 = num3 - num2;
			int k = 0;
			int num5 = num2;
			int[] array;
			if (num4 > 0)
			{
				array = ShiftLeft(y, num4);
				num5 += num4;
			}
			else
			{
				int num6 = y.Length - j;
				array = new int[num6];
				Array.Copy(y, j, array, 0, num6);
			}
			while (true)
			{
				if (num5 < num3 || CompareNoLeadingZeroes(i, x, k, array) >= 0)
				{
					Subtract(i, x, k, array);
					while (x[i] == 0)
					{
						if (++i == x.Length)
						{
							return x;
						}
					}
					num3 = 32 * (x.Length - i - 1) + BitLen(x[i]);
					if (num3 <= num2)
					{
						if (num3 < num2)
						{
							return x;
						}
						num = CompareNoLeadingZeroes(i, x, j, y);
						if (num <= 0)
						{
							break;
						}
					}
				}
				num4 = num5 - num3;
				if (num4 == 1)
				{
					int num7 = array[k] >>> 1;
					uint num8 = (uint)x[i];
					if ((uint)num7 > num8)
					{
						num4++;
					}
				}
				if (num4 < 2)
				{
					ShiftRightOneInPlace(k, array);
					num5--;
				}
				else
				{
					ShiftRightInPlace(k, array, num4);
					num5 -= num4;
				}
				for (; array[k] == 0; k++)
				{
				}
			}
		}
		if (num == 0)
		{
			Array.Clear(x, i, x.Length - i);
		}
		return x;
	}

	internal Number Remainder(Number n)
	{
		if (n.m_sign == 0)
		{
			throw new ArithmeticException("Invalid entry. Division by zero error");
		}
		if (m_sign == 0)
		{
			return Zero;
		}
		if (n.m_magnitude.Length == 1)
		{
			int num = n.m_magnitude[0];
			if (num > 0)
			{
				if (num == 1)
				{
					return Zero;
				}
				int num2 = Remainder(num);
				if (num2 != 0)
				{
					return new Number(m_sign, new int[1] { num2 }, checkMag: false);
				}
				return Zero;
			}
		}
		if (CompareNoLeadingZeroes(0, m_magnitude, 0, n.m_magnitude) < 0)
		{
			return this;
		}
		int[] mag;
		if (n.QuickPow2Check())
		{
			mag = LastNBits(n.Absolute().BitLength - 1);
		}
		else
		{
			mag = (int[])m_magnitude.Clone();
			mag = Remainder(mag, n.m_magnitude);
		}
		return new Number(m_sign, mag, checkMag: true);
	}

	private int[] LastNBits(int n)
	{
		if (n < 1)
		{
			return m_zeroMagnitude;
		}
		int val = (n + 32 - 1) / 32;
		val = Math.Min(val, m_magnitude.Length);
		int[] array = new int[val];
		Array.Copy(m_magnitude, m_magnitude.Length - val, array, 0, val);
		int num = (val << 5) - n;
		if (num > 0)
		{
			array[0] &= -1 >>> num;
		}
		return array;
	}

	private Number DivideWords(int w)
	{
		int num = m_magnitude.Length;
		if (w >= num)
		{
			return Zero;
		}
		int[] array = new int[num - w];
		Array.Copy(m_magnitude, 0, array, 0, num - w);
		return new Number(m_sign, array, checkMag: false);
	}

	private Number RemainderWords(int w)
	{
		int num = m_magnitude.Length;
		if (w >= num)
		{
			return this;
		}
		int[] array = new int[w];
		Array.Copy(m_magnitude, num - w, array, 0, w);
		return new Number(m_sign, array, checkMag: false);
	}

	private static int[] ShiftLeft(int[] mag, int n)
	{
		int num = n >>> 5;
		int num2 = n & 0x1F;
		int num3 = mag.Length;
		int[] array;
		if (num2 == 0)
		{
			array = new int[num3 + num];
			mag.CopyTo(array, 0);
		}
		else
		{
			int num4 = 0;
			int num5 = 32 - num2;
			int num6 = mag[0] >>> num5;
			if (num6 != 0)
			{
				array = new int[num3 + num + 1];
				array[num4++] = num6;
			}
			else
			{
				array = new int[num3 + num];
			}
			int num7 = mag[0];
			for (int i = 0; i < num3 - 1; i++)
			{
				int num8 = mag[i + 1];
				array[num4++] = (num7 << num2) | (num8 >>> num5);
				num7 = num8;
			}
			array[num4] = mag[num3 - 1] << num2;
		}
		return array;
	}

	internal Number ShiftLeft(int n)
	{
		if (m_sign == 0 || m_magnitude.Length == 0)
		{
			return Zero;
		}
		if (n == 0)
		{
			return this;
		}
		if (n < 0)
		{
			return ShiftRight(-n);
		}
		Number number = new Number(m_sign, ShiftLeft(m_magnitude, n), checkMag: true);
		if (m_nBits != -1)
		{
			number.m_nBits = ((m_sign > 0) ? m_nBits : (m_nBits + n));
		}
		if (m_nBitLength != -1)
		{
			number.m_nBitLength = m_nBitLength + n;
		}
		return number;
	}

	private static void ShiftRightInPlace(int start, int[] mag, int n)
	{
		int num = (n >>> 5) + start;
		int num2 = n & 0x1F;
		int num3 = mag.Length - 1;
		if (num != start)
		{
			int num4 = num - start;
			for (int num5 = num3; num5 >= num; num5--)
			{
				mag[num5] = mag[num5 - num4];
			}
			for (int num6 = num - 1; num6 >= start; num6--)
			{
				mag[num6] = 0;
			}
		}
		if (num2 != 0)
		{
			int num7 = 32 - num2;
			int num8 = mag[num3];
			for (int num9 = num3; num9 > num; num9--)
			{
				int num10 = mag[num9 - 1];
				mag[num9] = (num8 >>> num2) | (num10 << num7);
				num8 = num10;
			}
			mag[num] >>>= num2;
		}
	}

	private static void ShiftRightOneInPlace(int start, int[] mag)
	{
		int num = mag.Length;
		int num2 = mag[num - 1];
		while (--num > start)
		{
			int num3 = mag[num - 1];
			mag[num] = (num2 >>> 1) | (num3 << 31);
			num2 = num3;
		}
		mag[start] >>>= 1;
	}

	internal Number ShiftRight(int n)
	{
		if (n == 0)
		{
			return this;
		}
		if (n < 0)
		{
			return ShiftLeft(-n);
		}
		if (n >= BitLength)
		{
			if (m_sign >= 0)
			{
				return Zero;
			}
			return One.Negate();
		}
		int num = BitLength - n + 31 >> 5;
		int[] array = new int[num];
		int num2 = n >> 5;
		int num3 = n & 0x1F;
		if (num3 == 0)
		{
			Array.Copy(m_magnitude, 0, array, 0, array.Length);
		}
		else
		{
			int num4 = 32 - num3;
			int num5 = m_magnitude.Length - 1 - num2;
			for (int num6 = num - 1; num6 >= 0; num6--)
			{
				array[num6] = m_magnitude[num5--] >>> num3;
				if (num5 >= 0)
				{
					array[num6] |= m_magnitude[num5] << num4;
				}
			}
		}
		return new Number(m_sign, array, checkMag: false);
	}

	private static int[] Subtract(int xStart, int[] x, int yStart, int[] y)
	{
		int num = x.Length;
		int num2 = y.Length;
		int num3 = 0;
		do
		{
			long num4 = (x[--num] & 0xFFFFFFFFu) - (y[--num2] & 0xFFFFFFFFu) + num3;
			x[num] = (int)num4;
			num3 = (int)(num4 >> 63);
		}
		while (num2 > yStart);
		if (num3 != 0)
		{
			while (--x[--num] == -1)
			{
			}
		}
		return x;
	}

	internal Number Subtract(Number n)
	{
		if (n.m_sign == 0)
		{
			return this;
		}
		if (m_sign == 0)
		{
			return n.Negate();
		}
		if (m_sign != n.m_sign)
		{
			return Add(n.Negate());
		}
		int num = CompareNoLeadingZeroes(0, m_magnitude, 0, n.m_magnitude);
		if (num == 0)
		{
			return Zero;
		}
		Number number;
		Number number2;
		if (num < 0)
		{
			number = n;
			number2 = this;
		}
		else
		{
			number = this;
			number2 = n;
		}
		return new Number(m_sign * num, doSubBigLil(number.m_magnitude, number2.m_magnitude), checkMag: true);
	}

	private static int[] doSubBigLil(int[] bigMag, int[] lilMag)
	{
		int[] x = (int[])bigMag.Clone();
		return Subtract(0, x, 0, lilMag);
	}

	internal byte[] ToByteArray()
	{
		return ToByteArray(unsigned: false);
	}

	internal byte[] ToByteArrayUnsigned()
	{
		return ToByteArray(unsigned: true);
	}

	private byte[] ToByteArray(bool unsigned)
	{
		if (m_sign == 0)
		{
			if (!unsigned)
			{
				return new byte[1];
			}
			return m_zeroEncoding;
		}
		byte[] array = new byte[GetByteLength((unsigned && m_sign > 0) ? BitLength : (BitLength + 1))];
		int num = m_magnitude.Length;
		int num2 = array.Length;
		if (m_sign > 0)
		{
			while (num > 1)
			{
				uint num3 = (uint)m_magnitude[--num];
				array[--num2] = (byte)num3;
				array[--num2] = (byte)(num3 >> 8);
				array[--num2] = (byte)(num3 >> 16);
				array[--num2] = (byte)(num3 >> 24);
			}
			uint num4;
			for (num4 = (uint)m_magnitude[0]; num4 > 255; num4 >>= 8)
			{
				array[--num2] = (byte)num4;
			}
			array[--num2] = (byte)num4;
		}
		else
		{
			bool flag = true;
			while (num > 1)
			{
				uint num5 = (uint)(~m_magnitude[--num]);
				if (flag)
				{
					flag = ++num5 == 0;
				}
				array[--num2] = (byte)num5;
				array[--num2] = (byte)(num5 >> 8);
				array[--num2] = (byte)(num5 >> 16);
				array[--num2] = (byte)(num5 >> 24);
			}
			uint num6 = (uint)m_magnitude[0];
			if (flag)
			{
				num6--;
			}
			while (num6 > 255)
			{
				array[--num2] = (byte)(~num6);
				num6 >>= 8;
			}
			array[--num2] = (byte)(~num6);
			if (num2 > 0)
			{
				array[--num2] = byte.MaxValue;
			}
		}
		return array;
	}

	public override string ToString()
	{
		return ToString(10);
	}

	public string ToString(int radix)
	{
		switch (radix)
		{
		default:
			throw new FormatException("Invalid entry. Only bases 2, 8, 10, 16 are allowed");
		case 2:
		case 8:
		case 10:
		case 16:
		{
			if (m_magnitude == null)
			{
				return "null";
			}
			if (m_sign == 0)
			{
				return "0";
			}
			int i;
			for (i = 0; i < m_magnitude.Length && m_magnitude[i] == 0; i++)
			{
			}
			if (i == m_magnitude.Length)
			{
				return "0";
			}
			StringBuilder stringBuilder = new StringBuilder();
			if (m_sign == -1)
			{
				stringBuilder.Append('-');
			}
			switch (radix)
			{
			case 2:
			{
				int num9 = i;
				stringBuilder.Append(Convert.ToString(m_magnitude[num9], 2));
				while (++num9 < m_magnitude.Length)
				{
					AppendZeroExtendedString(stringBuilder, Convert.ToString(m_magnitude[num9], 2), 32);
				}
				break;
			}
			case 8:
			{
				int num5 = 1073741823;
				Number number2 = Absolute();
				int num6 = number2.BitLength;
				List<string> list2 = new List<string>();
				while (num6 > 30)
				{
					list2.Add(Convert.ToString(number2.IntValue & num5, 8));
					number2 = number2.ShiftRight(30);
					num6 -= 30;
				}
				stringBuilder.Append(Convert.ToString(number2.IntValue, 8));
				for (int num7 = list2.Count - 1; num7 >= 0; num7--)
				{
					AppendZeroExtendedString(stringBuilder, list2[num7], 10);
				}
				break;
			}
			case 16:
			{
				int num8 = i;
				stringBuilder.Append(Convert.ToString(m_magnitude[num8], 16));
				while (++num8 < m_magnitude.Length)
				{
					AppendZeroExtendedString(stringBuilder, Convert.ToString(m_magnitude[num8], 16), 8);
				}
				break;
			}
			case 10:
			{
				Number number = Absolute();
				if (number.BitLength < 64)
				{
					stringBuilder.Append(Convert.ToString(number.LongValue, radix));
					break;
				}
				long num = long.MaxValue / radix;
				long num2 = radix;
				int num3 = 1;
				while (num2 <= num)
				{
					num2 *= radix;
					num3++;
				}
				Number value = ValueOf(num2);
				List<string> list = new List<string>();
				while (number.CompareTo(value) >= 0)
				{
					Number[] array = number.DivideAndRemainder(value);
					list.Add(Convert.ToString(array[1].LongValue, radix));
					number = array[0];
				}
				stringBuilder.Append(Convert.ToString(number.LongValue, radix));
				for (int num4 = list.Count - 1; num4 >= 0; num4--)
				{
					AppendZeroExtendedString(stringBuilder, list[num4], num3);
				}
				break;
			}
			}
			return stringBuilder.ToString();
		}
		}
	}

	private static void AppendZeroExtendedString(StringBuilder sb, string s, int minLength)
	{
		for (int i = s.Length; i < minLength; i++)
		{
			sb.Append('0');
		}
		sb.Append(s);
	}

	private static Number CreateUValueOf(ulong value)
	{
		int num = (int)(value >> 32);
		int num2 = (int)value;
		if (num != 0)
		{
			return new Number(1, new int[2] { num, num2 }, checkMag: false);
		}
		if (num2 != 0)
		{
			Number number = new Number(1, new int[1] { num2 }, checkMag: false);
			if ((num2 & -num2) == num2)
			{
				number.m_nBits = 1;
			}
			return number;
		}
		return Zero;
	}

	private static Number CreateValueOf(long value)
	{
		if (value < 0)
		{
			if (value == long.MinValue)
			{
				return CreateValueOf(~value).Not();
			}
			return CreateValueOf(-value).Negate();
		}
		return CreateUValueOf((ulong)value);
	}

	public static Number ValueOf(long value)
	{
		if (value >= 0 && value < m_smallConstants.Length)
		{
			return m_smallConstants[value];
		}
		return CreateValueOf(value);
	}

	private int GetLowestSetBitMaskFirst(int firstWordMask)
	{
		int num = m_magnitude.Length;
		int num2 = 0;
		uint num3 = (uint)(m_magnitude[--num] & firstWordMask);
		while (num3 == 0)
		{
			num3 = (uint)m_magnitude[--num];
			num2 += 32;
		}
		while ((num3 & 0xFF) == 0)
		{
			num3 >>= 8;
			num2 += 8;
		}
		while ((num3 & 1) == 0)
		{
			num3 >>= 1;
			num2++;
		}
		return num2;
	}

	internal bool TestBit(int value)
	{
		if (value < 0)
		{
			throw new ArithmeticException("Invalid entry. Bit position can not be less than zero");
		}
		if (m_sign < 0)
		{
			return !Not().TestBit(value);
		}
		int num = value / 32;
		if (num >= m_magnitude.Length)
		{
			return false;
		}
		return ((m_magnitude[m_magnitude.Length - 1 - num] >> value % 32) & 1) > 0;
	}

	internal Number Or(Number value)
	{
		if (m_sign == 0)
		{
			return value;
		}
		if (value.m_sign == 0)
		{
			return this;
		}
		int[] array = ((m_sign > 0) ? m_magnitude : Add(One).m_magnitude);
		int[] array2 = ((value.m_sign > 0) ? value.m_magnitude : value.Add(One).m_magnitude);
		bool flag = m_sign < 0 || value.m_sign < 0;
		int[] array3 = new int[Math.Max(array.Length, array2.Length)];
		int num = array3.Length - array.Length;
		int num2 = array3.Length - array2.Length;
		for (int i = 0; i < array3.Length; i++)
		{
			int num3 = ((i >= num) ? array[i - num] : 0);
			int num4 = ((i >= num2) ? array2[i - num2] : 0);
			if (m_sign < 0)
			{
				num3 = ~num3;
			}
			if (value.m_sign < 0)
			{
				num4 = ~num4;
			}
			array3[i] = num3 | num4;
			if (flag)
			{
				array3[i] = ~array3[i];
			}
		}
		Number number = new Number(1, array3, checkMag: true);
		if (flag)
		{
			number = number.Not();
		}
		return number;
	}

	internal Number SetBit(int value)
	{
		if (value < 0)
		{
			throw new ArithmeticException("Invalid entry. Bit position can not be less than zero");
		}
		if (TestBit(value))
		{
			return this;
		}
		if (m_sign > 0 && value < BitLength - 1)
		{
			return FlipExistingBit(value);
		}
		return Or(One.ShiftLeft(value));
	}

	private Number FlipExistingBit(int value)
	{
		int[] array = (int[])m_magnitude.Clone();
		array[array.Length - 1 - (value >> 5)] ^= 1 << value;
		return new Number(m_sign, array, checkMag: false);
	}

	internal int GetLowestSetBit()
	{
		if (m_sign == 0)
		{
			return -1;
		}
		return GetLowestSetBitMaskFirst(-1);
	}
}
