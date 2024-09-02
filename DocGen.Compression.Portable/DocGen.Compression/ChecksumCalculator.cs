using System;

namespace DocGen.Compression;

public class ChecksumCalculator
{
	private const int DEF_CHECKSUM_BIT_OFFSET = 16;

	private const int DEF_CHECKSUM_BASE = 65521;

	private const int DEF_CHECKSUM_ITERATIONSCOUNT = 3800;

	public static void ChecksumUpdate(ref long checksum, byte[] buffer, int offset, int length)
	{
		uint num = (uint)checksum;
		uint num2 = num & 0xFFFFu;
		uint num3 = num >> 16;
		while (length > 0)
		{
			int num4 = Math.Min(length, 3800);
			length -= num4;
			while (--num4 >= 0)
			{
				num2 += (uint)(buffer[offset++] & 0xFF);
				num3 += num2;
			}
			num2 %= 65521;
			num3 %= 65521;
		}
		num = (num3 << 16) | num2;
		checksum = num;
	}

	public static long ChecksumGenerate(byte[] buffer, int offset, int length)
	{
		long checksum = 1L;
		ChecksumUpdate(ref checksum, buffer, offset, length);
		return checksum;
	}
}
