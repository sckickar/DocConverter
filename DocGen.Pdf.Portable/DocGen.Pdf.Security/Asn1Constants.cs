namespace DocGen.Pdf.Security;

internal sealed class Asn1Constants
{
	internal const string BerSequence = "BER Sequence";

	internal const string DerSequence = "DER Sequence";

	internal const string Sequence = "Sequence";

	internal const string Null = "NULL";

	internal const string Empty = "EMPTY";

	internal const string Der = "DER";

	internal const string Ber = "BER";

	internal const string DesEde = "DESede";

	internal const string Des = "DES";

	internal const string RC2 = "RC2";

	internal const string RSA = "RSA";

	internal const string PKCS7 = "PKCS7";

	internal static void UInt32ToBe(uint n, byte[] bs, int off)
	{
		bs[off] = (byte)(n >> 24);
		bs[off + 1] = (byte)(n >> 16);
		bs[off + 2] = (byte)(n >> 8);
		bs[off + 3] = (byte)n;
	}

	internal static uint BeToUInt32(byte[] bs, int off)
	{
		return (uint)((bs[off] << 24) | (bs[off + 1] << 16) | (bs[off + 2] << 8) | bs[off + 3]);
	}

	internal static byte[] UInt64ToBe(ulong n)
	{
		byte[] array = new byte[8];
		UInt64ToBe(n, array, 0);
		return array;
	}

	internal static void UInt32ToBe(uint n, byte[] bs)
	{
		bs[0] = (byte)(n >> 24);
		bs[1] = (byte)(n >> 16);
		bs[2] = (byte)(n >> 8);
		bs[3] = (byte)n;
	}

	internal static void UInt64ToBe(ulong n, byte[] bs)
	{
		UInt32ToBe((uint)(n >> 32), bs);
		UInt32ToBe((uint)n, bs, 4);
	}

	internal static void UInt64ToBe(ulong n, byte[] bs, int off)
	{
		UInt32ToBe((uint)(n >> 32), bs, off);
		UInt32ToBe((uint)n, bs, off + 4);
	}

	internal static uint BeToUInt32(byte[] bs)
	{
		return (uint)((bs[0] << 24) | (bs[1] << 16) | (bs[2] << 8) | bs[3]);
	}

	internal static byte[] UInt32ToLe(uint n)
	{
		byte[] array = new byte[4];
		UInt32ToLe(n, array, 0);
		return array;
	}

	internal static void UInt32ToLe(uint n, byte[] bs)
	{
		bs[0] = (byte)n;
		bs[1] = (byte)(n >> 8);
		bs[2] = (byte)(n >> 16);
		bs[3] = (byte)(n >> 24);
	}

	internal static void UInt32ToLe(uint n, byte[] bs, int off)
	{
		bs[off] = (byte)n;
		bs[off + 1] = (byte)(n >> 8);
		bs[off + 2] = (byte)(n >> 16);
		bs[off + 3] = (byte)(n >> 24);
	}

	internal static byte[] UInt32ToLe(uint[] ns)
	{
		byte[] array = new byte[4 * ns.Length];
		UInt32ToLe(ns, array, 0);
		return array;
	}

	internal static void UInt32ToLe(uint[] ns, byte[] bs, int off)
	{
		for (int i = 0; i < ns.Length; i++)
		{
			UInt32ToLe(ns[i], bs, off);
			off += 4;
		}
	}

	internal static ushort LeToUInt16(byte[] bs)
	{
		return (ushort)(bs[0] | (bs[1] << 8));
	}

	internal static ushort LeToUInt16(byte[] bs, int off)
	{
		return (ushort)(bs[off] | (bs[off + 1] << 8));
	}

	internal static uint LeToUInt32(byte[] bs)
	{
		return (uint)(bs[0] | (bs[1] << 8) | (bs[2] << 16) | (bs[3] << 24));
	}

	internal static uint LeToUInt32(byte[] bs, int off)
	{
		return (uint)(bs[off] | (bs[off + 1] << 8) | (bs[off + 2] << 16) | (bs[off + 3] << 24));
	}

	internal static void LeToUInt32(byte[] bs, int off, uint[] ns)
	{
		for (int i = 0; i < ns.Length; i++)
		{
			ns[i] = LeToUInt32(bs, off);
			off += 4;
		}
	}

	internal static ulong BeToUInt64(byte[] bs)
	{
		uint num = BeToUInt32(bs);
		uint num2 = BeToUInt32(bs, 4);
		return ((ulong)num << 32) | num2;
	}

	internal static ulong BeToUInt64(byte[] bs, int off)
	{
		uint num = BeToUInt32(bs, off);
		uint num2 = BeToUInt32(bs, off + 4);
		return ((ulong)num << 32) | num2;
	}

	public static bool AreEqual(byte[] a, byte[] b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		return HaveSameContents(a, b);
	}

	private static bool HaveSameContents(byte[] a, byte[] b)
	{
		int num = a.Length;
		if (num != b.Length)
		{
			return false;
		}
		while (num != 0)
		{
			num--;
			if (a[num] != b[num])
			{
				return false;
			}
		}
		return true;
	}

	public static int GetHashCode(byte[] data)
	{
		if (data == null)
		{
			return 0;
		}
		int num = data.Length;
		int num2 = num + 1;
		while (--num >= 0)
		{
			num2 *= 257;
			num2 ^= data[num];
		}
		return num2;
	}

	public static byte[] Clone(byte[] data)
	{
		if (data != null)
		{
			return (byte[])data.Clone();
		}
		return null;
	}

	public static int[] Clone(int[] data)
	{
		if (data != null)
		{
			return (int[])data.Clone();
		}
		return null;
	}
}
