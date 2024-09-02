using System;

namespace DocGen.Pdf.Security;

internal class RIPEMD160MessageDigest : MessageDigest
{
	private const int m_digestLength = 20;

	private int m_h0;

	private int m_h1;

	private int m_h2;

	private int m_h3;

	private int m_h4;

	private int[] m_x = new int[16];

	private int m_xOffset;

	public override string AlgorithmName => "RIPEMD160";

	public override int MessageDigestSize => 20;

	internal RIPEMD160MessageDigest()
	{
		Reset();
	}

	internal RIPEMD160MessageDigest(RIPEMD160MessageDigest t)
		: base(t)
	{
		m_h0 = t.m_h0;
		m_h1 = t.m_h1;
		m_h2 = t.m_h2;
		m_h3 = t.m_h3;
		m_h4 = t.m_h4;
		Array.Copy(t.m_x, 0, m_x, 0, t.m_x.Length);
		m_xOffset = t.m_xOffset;
	}

	internal override void ProcessWord(byte[] input, int inOff)
	{
		m_x[m_xOffset++] = (input[inOff] & 0xFF) | ((input[inOff + 1] & 0xFF) << 8) | ((input[inOff + 2] & 0xFF) << 16) | ((input[inOff + 3] & 0xFF) << 24);
		if (m_xOffset == 16)
		{
			ProcessBlock();
		}
	}

	internal override void ProcessLength(long bitLength)
	{
		if (m_xOffset > 14)
		{
			ProcessBlock();
		}
		m_x[14] = (int)(bitLength & 0xFFFFFFFFu);
		m_x[15] = (int)(bitLength >>> 32);
	}

	private void UnpackWord(int word, byte[] output, int offset)
	{
		output[offset] = (byte)word;
		output[offset + 1] = (byte)((uint)word >> 8);
		output[offset + 2] = (byte)((uint)word >> 16);
		output[offset + 3] = (byte)((uint)word >> 24);
	}

	public override int DoFinal(byte[] bytes, int offset)
	{
		Finish();
		UnpackWord(m_h0, bytes, offset);
		UnpackWord(m_h1, bytes, offset + 4);
		UnpackWord(m_h2, bytes, offset + 8);
		UnpackWord(m_h3, bytes, offset + 12);
		UnpackWord(m_h4, bytes, offset + 16);
		Reset();
		return 20;
	}

	public override void Reset()
	{
		base.Reset();
		m_h0 = 1732584193;
		m_h1 = -271733879;
		m_h2 = -1732584194;
		m_h3 = 271733878;
		m_h4 = -1009589776;
		m_xOffset = 0;
		for (int i = 0; i != m_x.Length; i++)
		{
			m_x[i] = 0;
		}
	}

	private int GetRightToLeft(int x, int n)
	{
		return (x << n) | (x >>> 32 - n);
	}

	private int GetBitLevelEXOR(int x, int y, int z)
	{
		return x ^ y ^ z;
	}

	private int GetBitlevelMultiplexer(int x, int y, int z)
	{
		return (x & y) | (~x & z);
	}

	private int GetBitlevelNegative(int x, int y, int z)
	{
		return (x | ~y) ^ z;
	}

	private int GetBitlevelDemultiplexer(int x, int y, int z)
	{
		return (x & z) | (y & ~z);
	}

	private int GetBitlevelReverseNegative(int x, int y, int z)
	{
		return x ^ (y | ~z);
	}

	internal override void ProcessBlock()
	{
		int h;
		int num = (h = m_h0);
		int h2;
		int num2 = (h2 = m_h1);
		int h3;
		int num3 = (h3 = m_h2);
		int h4;
		int num4 = (h4 = m_h3);
		int h5;
		int num5 = (h5 = m_h4);
		num = GetRightToLeft(num + GetBitLevelEXOR(num2, num3, num4) + m_x[0], 11) + num5;
		num3 = GetRightToLeft(num3, 10);
		num5 = GetRightToLeft(num5 + GetBitLevelEXOR(num, num2, num3) + m_x[1], 14) + num4;
		num2 = GetRightToLeft(num2, 10);
		num4 = GetRightToLeft(num4 + GetBitLevelEXOR(num5, num, num2) + m_x[2], 15) + num3;
		num = GetRightToLeft(num, 10);
		num3 = GetRightToLeft(num3 + GetBitLevelEXOR(num4, num5, num) + m_x[3], 12) + num2;
		num5 = GetRightToLeft(num5, 10);
		num2 = GetRightToLeft(num2 + GetBitLevelEXOR(num3, num4, num5) + m_x[4], 5) + num;
		num4 = GetRightToLeft(num4, 10);
		num = GetRightToLeft(num + GetBitLevelEXOR(num2, num3, num4) + m_x[5], 8) + num5;
		num3 = GetRightToLeft(num3, 10);
		num5 = GetRightToLeft(num5 + GetBitLevelEXOR(num, num2, num3) + m_x[6], 7) + num4;
		num2 = GetRightToLeft(num2, 10);
		num4 = GetRightToLeft(num4 + GetBitLevelEXOR(num5, num, num2) + m_x[7], 9) + num3;
		num = GetRightToLeft(num, 10);
		num3 = GetRightToLeft(num3 + GetBitLevelEXOR(num4, num5, num) + m_x[8], 11) + num2;
		num5 = GetRightToLeft(num5, 10);
		num2 = GetRightToLeft(num2 + GetBitLevelEXOR(num3, num4, num5) + m_x[9], 13) + num;
		num4 = GetRightToLeft(num4, 10);
		num = GetRightToLeft(num + GetBitLevelEXOR(num2, num3, num4) + m_x[10], 14) + num5;
		num3 = GetRightToLeft(num3, 10);
		num5 = GetRightToLeft(num5 + GetBitLevelEXOR(num, num2, num3) + m_x[11], 15) + num4;
		num2 = GetRightToLeft(num2, 10);
		num4 = GetRightToLeft(num4 + GetBitLevelEXOR(num5, num, num2) + m_x[12], 6) + num3;
		num = GetRightToLeft(num, 10);
		num3 = GetRightToLeft(num3 + GetBitLevelEXOR(num4, num5, num) + m_x[13], 7) + num2;
		num5 = GetRightToLeft(num5, 10);
		num2 = GetRightToLeft(num2 + GetBitLevelEXOR(num3, num4, num5) + m_x[14], 9) + num;
		num4 = GetRightToLeft(num4, 10);
		num = GetRightToLeft(num + GetBitLevelEXOR(num2, num3, num4) + m_x[15], 8) + num5;
		num3 = GetRightToLeft(num3, 10);
		h = GetRightToLeft(h + GetBitlevelReverseNegative(h2, h3, h4) + m_x[5] + 1352829926, 8) + h5;
		h3 = GetRightToLeft(h3, 10);
		h5 = GetRightToLeft(h5 + GetBitlevelReverseNegative(h, h2, h3) + m_x[14] + 1352829926, 9) + h4;
		h2 = GetRightToLeft(h2, 10);
		h4 = GetRightToLeft(h4 + GetBitlevelReverseNegative(h5, h, h2) + m_x[7] + 1352829926, 9) + h3;
		h = GetRightToLeft(h, 10);
		h3 = GetRightToLeft(h3 + GetBitlevelReverseNegative(h4, h5, h) + m_x[0] + 1352829926, 11) + h2;
		h5 = GetRightToLeft(h5, 10);
		h2 = GetRightToLeft(h2 + GetBitlevelReverseNegative(h3, h4, h5) + m_x[9] + 1352829926, 13) + h;
		h4 = GetRightToLeft(h4, 10);
		h = GetRightToLeft(h + GetBitlevelReverseNegative(h2, h3, h4) + m_x[2] + 1352829926, 15) + h5;
		h3 = GetRightToLeft(h3, 10);
		h5 = GetRightToLeft(h5 + GetBitlevelReverseNegative(h, h2, h3) + m_x[11] + 1352829926, 15) + h4;
		h2 = GetRightToLeft(h2, 10);
		h4 = GetRightToLeft(h4 + GetBitlevelReverseNegative(h5, h, h2) + m_x[4] + 1352829926, 5) + h3;
		h = GetRightToLeft(h, 10);
		h3 = GetRightToLeft(h3 + GetBitlevelReverseNegative(h4, h5, h) + m_x[13] + 1352829926, 7) + h2;
		h5 = GetRightToLeft(h5, 10);
		h2 = GetRightToLeft(h2 + GetBitlevelReverseNegative(h3, h4, h5) + m_x[6] + 1352829926, 7) + h;
		h4 = GetRightToLeft(h4, 10);
		h = GetRightToLeft(h + GetBitlevelReverseNegative(h2, h3, h4) + m_x[15] + 1352829926, 8) + h5;
		h3 = GetRightToLeft(h3, 10);
		h5 = GetRightToLeft(h5 + GetBitlevelReverseNegative(h, h2, h3) + m_x[8] + 1352829926, 11) + h4;
		h2 = GetRightToLeft(h2, 10);
		h4 = GetRightToLeft(h4 + GetBitlevelReverseNegative(h5, h, h2) + m_x[1] + 1352829926, 14) + h3;
		h = GetRightToLeft(h, 10);
		h3 = GetRightToLeft(h3 + GetBitlevelReverseNegative(h4, h5, h) + m_x[10] + 1352829926, 14) + h2;
		h5 = GetRightToLeft(h5, 10);
		h2 = GetRightToLeft(h2 + GetBitlevelReverseNegative(h3, h4, h5) + m_x[3] + 1352829926, 12) + h;
		h4 = GetRightToLeft(h4, 10);
		h = GetRightToLeft(h + GetBitlevelReverseNegative(h2, h3, h4) + m_x[12] + 1352829926, 6) + h5;
		h3 = GetRightToLeft(h3, 10);
		num5 = GetRightToLeft(num5 + GetBitlevelMultiplexer(num, num2, num3) + m_x[7] + 1518500249, 7) + num4;
		num2 = GetRightToLeft(num2, 10);
		num4 = GetRightToLeft(num4 + GetBitlevelMultiplexer(num5, num, num2) + m_x[4] + 1518500249, 6) + num3;
		num = GetRightToLeft(num, 10);
		num3 = GetRightToLeft(num3 + GetBitlevelMultiplexer(num4, num5, num) + m_x[13] + 1518500249, 8) + num2;
		num5 = GetRightToLeft(num5, 10);
		num2 = GetRightToLeft(num2 + GetBitlevelMultiplexer(num3, num4, num5) + m_x[1] + 1518500249, 13) + num;
		num4 = GetRightToLeft(num4, 10);
		num = GetRightToLeft(num + GetBitlevelMultiplexer(num2, num3, num4) + m_x[10] + 1518500249, 11) + num5;
		num3 = GetRightToLeft(num3, 10);
		num5 = GetRightToLeft(num5 + GetBitlevelMultiplexer(num, num2, num3) + m_x[6] + 1518500249, 9) + num4;
		num2 = GetRightToLeft(num2, 10);
		num4 = GetRightToLeft(num4 + GetBitlevelMultiplexer(num5, num, num2) + m_x[15] + 1518500249, 7) + num3;
		num = GetRightToLeft(num, 10);
		num3 = GetRightToLeft(num3 + GetBitlevelMultiplexer(num4, num5, num) + m_x[3] + 1518500249, 15) + num2;
		num5 = GetRightToLeft(num5, 10);
		num2 = GetRightToLeft(num2 + GetBitlevelMultiplexer(num3, num4, num5) + m_x[12] + 1518500249, 7) + num;
		num4 = GetRightToLeft(num4, 10);
		num = GetRightToLeft(num + GetBitlevelMultiplexer(num2, num3, num4) + m_x[0] + 1518500249, 12) + num5;
		num3 = GetRightToLeft(num3, 10);
		num5 = GetRightToLeft(num5 + GetBitlevelMultiplexer(num, num2, num3) + m_x[9] + 1518500249, 15) + num4;
		num2 = GetRightToLeft(num2, 10);
		num4 = GetRightToLeft(num4 + GetBitlevelMultiplexer(num5, num, num2) + m_x[5] + 1518500249, 9) + num3;
		num = GetRightToLeft(num, 10);
		num3 = GetRightToLeft(num3 + GetBitlevelMultiplexer(num4, num5, num) + m_x[2] + 1518500249, 11) + num2;
		num5 = GetRightToLeft(num5, 10);
		num2 = GetRightToLeft(num2 + GetBitlevelMultiplexer(num3, num4, num5) + m_x[14] + 1518500249, 7) + num;
		num4 = GetRightToLeft(num4, 10);
		num = GetRightToLeft(num + GetBitlevelMultiplexer(num2, num3, num4) + m_x[11] + 1518500249, 13) + num5;
		num3 = GetRightToLeft(num3, 10);
		num5 = GetRightToLeft(num5 + GetBitlevelMultiplexer(num, num2, num3) + m_x[8] + 1518500249, 12) + num4;
		num2 = GetRightToLeft(num2, 10);
		h5 = GetRightToLeft(h5 + GetBitlevelDemultiplexer(h, h2, h3) + m_x[6] + 1548603684, 9) + h4;
		h2 = GetRightToLeft(h2, 10);
		h4 = GetRightToLeft(h4 + GetBitlevelDemultiplexer(h5, h, h2) + m_x[11] + 1548603684, 13) + h3;
		h = GetRightToLeft(h, 10);
		h3 = GetRightToLeft(h3 + GetBitlevelDemultiplexer(h4, h5, h) + m_x[3] + 1548603684, 15) + h2;
		h5 = GetRightToLeft(h5, 10);
		h2 = GetRightToLeft(h2 + GetBitlevelDemultiplexer(h3, h4, h5) + m_x[7] + 1548603684, 7) + h;
		h4 = GetRightToLeft(h4, 10);
		h = GetRightToLeft(h + GetBitlevelDemultiplexer(h2, h3, h4) + m_x[0] + 1548603684, 12) + h5;
		h3 = GetRightToLeft(h3, 10);
		h5 = GetRightToLeft(h5 + GetBitlevelDemultiplexer(h, h2, h3) + m_x[13] + 1548603684, 8) + h4;
		h2 = GetRightToLeft(h2, 10);
		h4 = GetRightToLeft(h4 + GetBitlevelDemultiplexer(h5, h, h2) + m_x[5] + 1548603684, 9) + h3;
		h = GetRightToLeft(h, 10);
		h3 = GetRightToLeft(h3 + GetBitlevelDemultiplexer(h4, h5, h) + m_x[10] + 1548603684, 11) + h2;
		h5 = GetRightToLeft(h5, 10);
		h2 = GetRightToLeft(h2 + GetBitlevelDemultiplexer(h3, h4, h5) + m_x[14] + 1548603684, 7) + h;
		h4 = GetRightToLeft(h4, 10);
		h = GetRightToLeft(h + GetBitlevelDemultiplexer(h2, h3, h4) + m_x[15] + 1548603684, 7) + h5;
		h3 = GetRightToLeft(h3, 10);
		h5 = GetRightToLeft(h5 + GetBitlevelDemultiplexer(h, h2, h3) + m_x[8] + 1548603684, 12) + h4;
		h2 = GetRightToLeft(h2, 10);
		h4 = GetRightToLeft(h4 + GetBitlevelDemultiplexer(h5, h, h2) + m_x[12] + 1548603684, 7) + h3;
		h = GetRightToLeft(h, 10);
		h3 = GetRightToLeft(h3 + GetBitlevelDemultiplexer(h4, h5, h) + m_x[4] + 1548603684, 6) + h2;
		h5 = GetRightToLeft(h5, 10);
		h2 = GetRightToLeft(h2 + GetBitlevelDemultiplexer(h3, h4, h5) + m_x[9] + 1548603684, 15) + h;
		h4 = GetRightToLeft(h4, 10);
		h = GetRightToLeft(h + GetBitlevelDemultiplexer(h2, h3, h4) + m_x[1] + 1548603684, 13) + h5;
		h3 = GetRightToLeft(h3, 10);
		h5 = GetRightToLeft(h5 + GetBitlevelDemultiplexer(h, h2, h3) + m_x[2] + 1548603684, 11) + h4;
		h2 = GetRightToLeft(h2, 10);
		num4 = GetRightToLeft(num4 + GetBitlevelNegative(num5, num, num2) + m_x[3] + 1859775393, 11) + num3;
		num = GetRightToLeft(num, 10);
		num3 = GetRightToLeft(num3 + GetBitlevelNegative(num4, num5, num) + m_x[10] + 1859775393, 13) + num2;
		num5 = GetRightToLeft(num5, 10);
		num2 = GetRightToLeft(num2 + GetBitlevelNegative(num3, num4, num5) + m_x[14] + 1859775393, 6) + num;
		num4 = GetRightToLeft(num4, 10);
		num = GetRightToLeft(num + GetBitlevelNegative(num2, num3, num4) + m_x[4] + 1859775393, 7) + num5;
		num3 = GetRightToLeft(num3, 10);
		num5 = GetRightToLeft(num5 + GetBitlevelNegative(num, num2, num3) + m_x[9] + 1859775393, 14) + num4;
		num2 = GetRightToLeft(num2, 10);
		num4 = GetRightToLeft(num4 + GetBitlevelNegative(num5, num, num2) + m_x[15] + 1859775393, 9) + num3;
		num = GetRightToLeft(num, 10);
		num3 = GetRightToLeft(num3 + GetBitlevelNegative(num4, num5, num) + m_x[8] + 1859775393, 13) + num2;
		num5 = GetRightToLeft(num5, 10);
		num2 = GetRightToLeft(num2 + GetBitlevelNegative(num3, num4, num5) + m_x[1] + 1859775393, 15) + num;
		num4 = GetRightToLeft(num4, 10);
		num = GetRightToLeft(num + GetBitlevelNegative(num2, num3, num4) + m_x[2] + 1859775393, 14) + num5;
		num3 = GetRightToLeft(num3, 10);
		num5 = GetRightToLeft(num5 + GetBitlevelNegative(num, num2, num3) + m_x[7] + 1859775393, 8) + num4;
		num2 = GetRightToLeft(num2, 10);
		num4 = GetRightToLeft(num4 + GetBitlevelNegative(num5, num, num2) + m_x[0] + 1859775393, 13) + num3;
		num = GetRightToLeft(num, 10);
		num3 = GetRightToLeft(num3 + GetBitlevelNegative(num4, num5, num) + m_x[6] + 1859775393, 6) + num2;
		num5 = GetRightToLeft(num5, 10);
		num2 = GetRightToLeft(num2 + GetBitlevelNegative(num3, num4, num5) + m_x[13] + 1859775393, 5) + num;
		num4 = GetRightToLeft(num4, 10);
		num = GetRightToLeft(num + GetBitlevelNegative(num2, num3, num4) + m_x[11] + 1859775393, 12) + num5;
		num3 = GetRightToLeft(num3, 10);
		num5 = GetRightToLeft(num5 + GetBitlevelNegative(num, num2, num3) + m_x[5] + 1859775393, 7) + num4;
		num2 = GetRightToLeft(num2, 10);
		num4 = GetRightToLeft(num4 + GetBitlevelNegative(num5, num, num2) + m_x[12] + 1859775393, 5) + num3;
		num = GetRightToLeft(num, 10);
		h4 = GetRightToLeft(h4 + GetBitlevelNegative(h5, h, h2) + m_x[15] + 1836072691, 9) + h3;
		h = GetRightToLeft(h, 10);
		h3 = GetRightToLeft(h3 + GetBitlevelNegative(h4, h5, h) + m_x[5] + 1836072691, 7) + h2;
		h5 = GetRightToLeft(h5, 10);
		h2 = GetRightToLeft(h2 + GetBitlevelNegative(h3, h4, h5) + m_x[1] + 1836072691, 15) + h;
		h4 = GetRightToLeft(h4, 10);
		h = GetRightToLeft(h + GetBitlevelNegative(h2, h3, h4) + m_x[3] + 1836072691, 11) + h5;
		h3 = GetRightToLeft(h3, 10);
		h5 = GetRightToLeft(h5 + GetBitlevelNegative(h, h2, h3) + m_x[7] + 1836072691, 8) + h4;
		h2 = GetRightToLeft(h2, 10);
		h4 = GetRightToLeft(h4 + GetBitlevelNegative(h5, h, h2) + m_x[14] + 1836072691, 6) + h3;
		h = GetRightToLeft(h, 10);
		h3 = GetRightToLeft(h3 + GetBitlevelNegative(h4, h5, h) + m_x[6] + 1836072691, 6) + h2;
		h5 = GetRightToLeft(h5, 10);
		h2 = GetRightToLeft(h2 + GetBitlevelNegative(h3, h4, h5) + m_x[9] + 1836072691, 14) + h;
		h4 = GetRightToLeft(h4, 10);
		h = GetRightToLeft(h + GetBitlevelNegative(h2, h3, h4) + m_x[11] + 1836072691, 12) + h5;
		h3 = GetRightToLeft(h3, 10);
		h5 = GetRightToLeft(h5 + GetBitlevelNegative(h, h2, h3) + m_x[8] + 1836072691, 13) + h4;
		h2 = GetRightToLeft(h2, 10);
		h4 = GetRightToLeft(h4 + GetBitlevelNegative(h5, h, h2) + m_x[12] + 1836072691, 5) + h3;
		h = GetRightToLeft(h, 10);
		h3 = GetRightToLeft(h3 + GetBitlevelNegative(h4, h5, h) + m_x[2] + 1836072691, 14) + h2;
		h5 = GetRightToLeft(h5, 10);
		h2 = GetRightToLeft(h2 + GetBitlevelNegative(h3, h4, h5) + m_x[10] + 1836072691, 13) + h;
		h4 = GetRightToLeft(h4, 10);
		h = GetRightToLeft(h + GetBitlevelNegative(h2, h3, h4) + m_x[0] + 1836072691, 13) + h5;
		h3 = GetRightToLeft(h3, 10);
		h5 = GetRightToLeft(h5 + GetBitlevelNegative(h, h2, h3) + m_x[4] + 1836072691, 7) + h4;
		h2 = GetRightToLeft(h2, 10);
		h4 = GetRightToLeft(h4 + GetBitlevelNegative(h5, h, h2) + m_x[13] + 1836072691, 5) + h3;
		h = GetRightToLeft(h, 10);
		num3 = GetRightToLeft(num3 + GetBitlevelDemultiplexer(num4, num5, num) + m_x[1] - 1894007588, 11) + num2;
		num5 = GetRightToLeft(num5, 10);
		num2 = GetRightToLeft(num2 + GetBitlevelDemultiplexer(num3, num4, num5) + m_x[9] - 1894007588, 12) + num;
		num4 = GetRightToLeft(num4, 10);
		num = GetRightToLeft(num + GetBitlevelDemultiplexer(num2, num3, num4) + m_x[11] - 1894007588, 14) + num5;
		num3 = GetRightToLeft(num3, 10);
		num5 = GetRightToLeft(num5 + GetBitlevelDemultiplexer(num, num2, num3) + m_x[10] - 1894007588, 15) + num4;
		num2 = GetRightToLeft(num2, 10);
		num4 = GetRightToLeft(num4 + GetBitlevelDemultiplexer(num5, num, num2) + m_x[0] - 1894007588, 14) + num3;
		num = GetRightToLeft(num, 10);
		num3 = GetRightToLeft(num3 + GetBitlevelDemultiplexer(num4, num5, num) + m_x[8] - 1894007588, 15) + num2;
		num5 = GetRightToLeft(num5, 10);
		num2 = GetRightToLeft(num2 + GetBitlevelDemultiplexer(num3, num4, num5) + m_x[12] - 1894007588, 9) + num;
		num4 = GetRightToLeft(num4, 10);
		num = GetRightToLeft(num + GetBitlevelDemultiplexer(num2, num3, num4) + m_x[4] - 1894007588, 8) + num5;
		num3 = GetRightToLeft(num3, 10);
		num5 = GetRightToLeft(num5 + GetBitlevelDemultiplexer(num, num2, num3) + m_x[13] - 1894007588, 9) + num4;
		num2 = GetRightToLeft(num2, 10);
		num4 = GetRightToLeft(num4 + GetBitlevelDemultiplexer(num5, num, num2) + m_x[3] - 1894007588, 14) + num3;
		num = GetRightToLeft(num, 10);
		num3 = GetRightToLeft(num3 + GetBitlevelDemultiplexer(num4, num5, num) + m_x[7] - 1894007588, 5) + num2;
		num5 = GetRightToLeft(num5, 10);
		num2 = GetRightToLeft(num2 + GetBitlevelDemultiplexer(num3, num4, num5) + m_x[15] - 1894007588, 6) + num;
		num4 = GetRightToLeft(num4, 10);
		num = GetRightToLeft(num + GetBitlevelDemultiplexer(num2, num3, num4) + m_x[14] - 1894007588, 8) + num5;
		num3 = GetRightToLeft(num3, 10);
		num5 = GetRightToLeft(num5 + GetBitlevelDemultiplexer(num, num2, num3) + m_x[5] - 1894007588, 6) + num4;
		num2 = GetRightToLeft(num2, 10);
		num4 = GetRightToLeft(num4 + GetBitlevelDemultiplexer(num5, num, num2) + m_x[6] - 1894007588, 5) + num3;
		num = GetRightToLeft(num, 10);
		num3 = GetRightToLeft(num3 + GetBitlevelDemultiplexer(num4, num5, num) + m_x[2] - 1894007588, 12) + num2;
		num5 = GetRightToLeft(num5, 10);
		h3 = GetRightToLeft(h3 + GetBitlevelMultiplexer(h4, h5, h) + m_x[8] + 2053994217, 15) + h2;
		h5 = GetRightToLeft(h5, 10);
		h2 = GetRightToLeft(h2 + GetBitlevelMultiplexer(h3, h4, h5) + m_x[6] + 2053994217, 5) + h;
		h4 = GetRightToLeft(h4, 10);
		h = GetRightToLeft(h + GetBitlevelMultiplexer(h2, h3, h4) + m_x[4] + 2053994217, 8) + h5;
		h3 = GetRightToLeft(h3, 10);
		h5 = GetRightToLeft(h5 + GetBitlevelMultiplexer(h, h2, h3) + m_x[1] + 2053994217, 11) + h4;
		h2 = GetRightToLeft(h2, 10);
		h4 = GetRightToLeft(h4 + GetBitlevelMultiplexer(h5, h, h2) + m_x[3] + 2053994217, 14) + h3;
		h = GetRightToLeft(h, 10);
		h3 = GetRightToLeft(h3 + GetBitlevelMultiplexer(h4, h5, h) + m_x[11] + 2053994217, 14) + h2;
		h5 = GetRightToLeft(h5, 10);
		h2 = GetRightToLeft(h2 + GetBitlevelMultiplexer(h3, h4, h5) + m_x[15] + 2053994217, 6) + h;
		h4 = GetRightToLeft(h4, 10);
		h = GetRightToLeft(h + GetBitlevelMultiplexer(h2, h3, h4) + m_x[0] + 2053994217, 14) + h5;
		h3 = GetRightToLeft(h3, 10);
		h5 = GetRightToLeft(h5 + GetBitlevelMultiplexer(h, h2, h3) + m_x[5] + 2053994217, 6) + h4;
		h2 = GetRightToLeft(h2, 10);
		h4 = GetRightToLeft(h4 + GetBitlevelMultiplexer(h5, h, h2) + m_x[12] + 2053994217, 9) + h3;
		h = GetRightToLeft(h, 10);
		h3 = GetRightToLeft(h3 + GetBitlevelMultiplexer(h4, h5, h) + m_x[2] + 2053994217, 12) + h2;
		h5 = GetRightToLeft(h5, 10);
		h2 = GetRightToLeft(h2 + GetBitlevelMultiplexer(h3, h4, h5) + m_x[13] + 2053994217, 9) + h;
		h4 = GetRightToLeft(h4, 10);
		h = GetRightToLeft(h + GetBitlevelMultiplexer(h2, h3, h4) + m_x[9] + 2053994217, 12) + h5;
		h3 = GetRightToLeft(h3, 10);
		h5 = GetRightToLeft(h5 + GetBitlevelMultiplexer(h, h2, h3) + m_x[7] + 2053994217, 5) + h4;
		h2 = GetRightToLeft(h2, 10);
		h4 = GetRightToLeft(h4 + GetBitlevelMultiplexer(h5, h, h2) + m_x[10] + 2053994217, 15) + h3;
		h = GetRightToLeft(h, 10);
		h3 = GetRightToLeft(h3 + GetBitlevelMultiplexer(h4, h5, h) + m_x[14] + 2053994217, 8) + h2;
		h5 = GetRightToLeft(h5, 10);
		num2 = GetRightToLeft(num2 + GetBitlevelReverseNegative(num3, num4, num5) + m_x[4] - 1454113458, 9) + num;
		num4 = GetRightToLeft(num4, 10);
		num = GetRightToLeft(num + GetBitlevelReverseNegative(num2, num3, num4) + m_x[0] - 1454113458, 15) + num5;
		num3 = GetRightToLeft(num3, 10);
		num5 = GetRightToLeft(num5 + GetBitlevelReverseNegative(num, num2, num3) + m_x[5] - 1454113458, 5) + num4;
		num2 = GetRightToLeft(num2, 10);
		num4 = GetRightToLeft(num4 + GetBitlevelReverseNegative(num5, num, num2) + m_x[9] - 1454113458, 11) + num3;
		num = GetRightToLeft(num, 10);
		num3 = GetRightToLeft(num3 + GetBitlevelReverseNegative(num4, num5, num) + m_x[7] - 1454113458, 6) + num2;
		num5 = GetRightToLeft(num5, 10);
		num2 = GetRightToLeft(num2 + GetBitlevelReverseNegative(num3, num4, num5) + m_x[12] - 1454113458, 8) + num;
		num4 = GetRightToLeft(num4, 10);
		num = GetRightToLeft(num + GetBitlevelReverseNegative(num2, num3, num4) + m_x[2] - 1454113458, 13) + num5;
		num3 = GetRightToLeft(num3, 10);
		num5 = GetRightToLeft(num5 + GetBitlevelReverseNegative(num, num2, num3) + m_x[10] - 1454113458, 12) + num4;
		num2 = GetRightToLeft(num2, 10);
		num4 = GetRightToLeft(num4 + GetBitlevelReverseNegative(num5, num, num2) + m_x[14] - 1454113458, 5) + num3;
		num = GetRightToLeft(num, 10);
		num3 = GetRightToLeft(num3 + GetBitlevelReverseNegative(num4, num5, num) + m_x[1] - 1454113458, 12) + num2;
		num5 = GetRightToLeft(num5, 10);
		num2 = GetRightToLeft(num2 + GetBitlevelReverseNegative(num3, num4, num5) + m_x[3] - 1454113458, 13) + num;
		num4 = GetRightToLeft(num4, 10);
		num = GetRightToLeft(num + GetBitlevelReverseNegative(num2, num3, num4) + m_x[8] - 1454113458, 14) + num5;
		num3 = GetRightToLeft(num3, 10);
		num5 = GetRightToLeft(num5 + GetBitlevelReverseNegative(num, num2, num3) + m_x[11] - 1454113458, 11) + num4;
		num2 = GetRightToLeft(num2, 10);
		num4 = GetRightToLeft(num4 + GetBitlevelReverseNegative(num5, num, num2) + m_x[6] - 1454113458, 8) + num3;
		num = GetRightToLeft(num, 10);
		num3 = GetRightToLeft(num3 + GetBitlevelReverseNegative(num4, num5, num) + m_x[15] - 1454113458, 5) + num2;
		num5 = GetRightToLeft(num5, 10);
		num2 = GetRightToLeft(num2 + GetBitlevelReverseNegative(num3, num4, num5) + m_x[13] - 1454113458, 6) + num;
		num4 = GetRightToLeft(num4, 10);
		h2 = GetRightToLeft(h2 + GetBitLevelEXOR(h3, h4, h5) + m_x[12], 8) + h;
		h4 = GetRightToLeft(h4, 10);
		h = GetRightToLeft(h + GetBitLevelEXOR(h2, h3, h4) + m_x[15], 5) + h5;
		h3 = GetRightToLeft(h3, 10);
		h5 = GetRightToLeft(h5 + GetBitLevelEXOR(h, h2, h3) + m_x[10], 12) + h4;
		h2 = GetRightToLeft(h2, 10);
		h4 = GetRightToLeft(h4 + GetBitLevelEXOR(h5, h, h2) + m_x[4], 9) + h3;
		h = GetRightToLeft(h, 10);
		h3 = GetRightToLeft(h3 + GetBitLevelEXOR(h4, h5, h) + m_x[1], 12) + h2;
		h5 = GetRightToLeft(h5, 10);
		h2 = GetRightToLeft(h2 + GetBitLevelEXOR(h3, h4, h5) + m_x[5], 5) + h;
		h4 = GetRightToLeft(h4, 10);
		h = GetRightToLeft(h + GetBitLevelEXOR(h2, h3, h4) + m_x[8], 14) + h5;
		h3 = GetRightToLeft(h3, 10);
		h5 = GetRightToLeft(h5 + GetBitLevelEXOR(h, h2, h3) + m_x[7], 6) + h4;
		h2 = GetRightToLeft(h2, 10);
		h4 = GetRightToLeft(h4 + GetBitLevelEXOR(h5, h, h2) + m_x[6], 8) + h3;
		h = GetRightToLeft(h, 10);
		h3 = GetRightToLeft(h3 + GetBitLevelEXOR(h4, h5, h) + m_x[2], 13) + h2;
		h5 = GetRightToLeft(h5, 10);
		h2 = GetRightToLeft(h2 + GetBitLevelEXOR(h3, h4, h5) + m_x[13], 6) + h;
		h4 = GetRightToLeft(h4, 10);
		h = GetRightToLeft(h + GetBitLevelEXOR(h2, h3, h4) + m_x[14], 5) + h5;
		h3 = GetRightToLeft(h3, 10);
		h5 = GetRightToLeft(h5 + GetBitLevelEXOR(h, h2, h3) + m_x[0], 15) + h4;
		h2 = GetRightToLeft(h2, 10);
		h4 = GetRightToLeft(h4 + GetBitLevelEXOR(h5, h, h2) + m_x[3], 13) + h3;
		h = GetRightToLeft(h, 10);
		h3 = GetRightToLeft(h3 + GetBitLevelEXOR(h4, h5, h) + m_x[9], 11) + h2;
		h5 = GetRightToLeft(h5, 10);
		h2 = GetRightToLeft(h2 + GetBitLevelEXOR(h3, h4, h5) + m_x[11], 11) + h;
		h4 = GetRightToLeft(h4, 10);
		h4 += num3 + m_h1;
		m_h1 = m_h2 + num4 + h5;
		m_h2 = m_h3 + num5 + h;
		m_h3 = m_h4 + num + h2;
		m_h4 = m_h0 + num2 + h3;
		m_h0 = h4;
		m_xOffset = 0;
		for (int i = 0; i != m_x.Length; i++)
		{
			m_x[i] = 0;
		}
	}
}
