using System;

namespace DocGen.Pdf.Security;

internal class SHA1MessageDigest : MessageDigest
{
	private const uint c_y1 = 1518500249u;

	private const uint c_y2 = 1859775393u;

	private const uint c_y3 = 2400959708u;

	private const uint c_y4 = 3395469782u;

	private const int c_digestLength = 20;

	private uint m_h1;

	private uint m_h2;

	private uint m_h3;

	private uint m_h4;

	private uint m_h5;

	private uint[] m_x = new uint[80];

	private int m_xOff;

	public override string AlgorithmName => "SHA-1";

	public override int MessageDigestSize => 20;

	internal SHA1MessageDigest()
	{
		Reset();
	}

	internal SHA1MessageDigest(SHA1MessageDigest t)
		: base(t)
	{
		m_h1 = t.m_h1;
		m_h2 = t.m_h2;
		m_h3 = t.m_h3;
		m_h4 = t.m_h4;
		m_h5 = t.m_h5;
		Array.Copy(t.m_x, 0, m_x, 0, t.m_x.Length);
		m_xOff = t.m_xOff;
	}

	internal override void ProcessWord(byte[] input, int inOff)
	{
		m_x[m_xOff] = Asn1Constants.BeToUInt32(input, inOff);
		if (++m_xOff == 16)
		{
			ProcessBlock();
		}
	}

	internal override void ProcessLength(long bitLength)
	{
		if (m_xOff > 14)
		{
			ProcessBlock();
		}
		m_x[14] = (uint)((ulong)bitLength >> 32);
		m_x[15] = (uint)bitLength;
	}

	public override int DoFinal(byte[] bytes, int offset)
	{
		Finish();
		Asn1Constants.UInt32ToBe(m_h1, bytes, offset);
		Asn1Constants.UInt32ToBe(m_h2, bytes, offset + 4);
		Asn1Constants.UInt32ToBe(m_h3, bytes, offset + 8);
		Asn1Constants.UInt32ToBe(m_h4, bytes, offset + 12);
		Asn1Constants.UInt32ToBe(m_h5, bytes, offset + 16);
		Reset();
		return 20;
	}

	public override void Reset()
	{
		base.Reset();
		m_h1 = 1732584193u;
		m_h2 = 4023233417u;
		m_h3 = 2562383102u;
		m_h4 = 271733878u;
		m_h5 = 3285377520u;
		m_xOff = 0;
		Array.Clear(m_x, 0, m_x.Length);
	}

	internal override void ProcessBlock()
	{
		for (int i = 16; i < 80; i++)
		{
			uint num = m_x[i - 3] ^ m_x[i - 8] ^ m_x[i - 14] ^ m_x[i - 16];
			m_x[i] = (num << 1) | (num >> 31);
		}
		uint num2 = m_h1;
		uint num3 = m_h2;
		uint num4 = m_h3;
		uint num5 = m_h4;
		uint num6 = m_h5;
		int num7 = 0;
		for (int j = 0; j < 4; j++)
		{
			num6 += ((num2 << 5) | (num2 >> 27)) + F(num3, num4, num5) + m_x[num7++] + 1518500249;
			num3 = (num3 << 30) | (num3 >> 2);
			num5 += ((num6 << 5) | (num6 >> 27)) + F(num2, num3, num4) + m_x[num7++] + 1518500249;
			num2 = (num2 << 30) | (num2 >> 2);
			num4 += ((num5 << 5) | (num5 >> 27)) + F(num6, num2, num3) + m_x[num7++] + 1518500249;
			num6 = (num6 << 30) | (num6 >> 2);
			num3 += ((num4 << 5) | (num4 >> 27)) + F(num5, num6, num2) + m_x[num7++] + 1518500249;
			num5 = (num5 << 30) | (num5 >> 2);
			num2 += ((num3 << 5) | (num3 >> 27)) + F(num4, num5, num6) + m_x[num7++] + 1518500249;
			num4 = (num4 << 30) | (num4 >> 2);
		}
		for (int k = 0; k < 4; k++)
		{
			num6 += ((num2 << 5) | (num2 >> 27)) + H(num3, num4, num5) + m_x[num7++] + 1859775393;
			num3 = (num3 << 30) | (num3 >> 2);
			num5 += ((num6 << 5) | (num6 >> 27)) + H(num2, num3, num4) + m_x[num7++] + 1859775393;
			num2 = (num2 << 30) | (num2 >> 2);
			num4 += ((num5 << 5) | (num5 >> 27)) + H(num6, num2, num3) + m_x[num7++] + 1859775393;
			num6 = (num6 << 30) | (num6 >> 2);
			num3 += ((num4 << 5) | (num4 >> 27)) + H(num5, num6, num2) + m_x[num7++] + 1859775393;
			num5 = (num5 << 30) | (num5 >> 2);
			num2 += ((num3 << 5) | (num3 >> 27)) + H(num4, num5, num6) + m_x[num7++] + 1859775393;
			num4 = (num4 << 30) | (num4 >> 2);
		}
		for (int l = 0; l < 4; l++)
		{
			num6 += (uint)((int)(((num2 << 5) | (num2 >> 27)) + G(num3, num4, num5) + m_x[num7++]) + -1894007588);
			num3 = (num3 << 30) | (num3 >> 2);
			num5 += (uint)((int)(((num6 << 5) | (num6 >> 27)) + G(num2, num3, num4) + m_x[num7++]) + -1894007588);
			num2 = (num2 << 30) | (num2 >> 2);
			num4 += (uint)((int)(((num5 << 5) | (num5 >> 27)) + G(num6, num2, num3) + m_x[num7++]) + -1894007588);
			num6 = (num6 << 30) | (num6 >> 2);
			num3 += (uint)((int)(((num4 << 5) | (num4 >> 27)) + G(num5, num6, num2) + m_x[num7++]) + -1894007588);
			num5 = (num5 << 30) | (num5 >> 2);
			num2 += (uint)((int)(((num3 << 5) | (num3 >> 27)) + G(num4, num5, num6) + m_x[num7++]) + -1894007588);
			num4 = (num4 << 30) | (num4 >> 2);
		}
		for (int m = 0; m < 4; m++)
		{
			num6 += (uint)((int)(((num2 << 5) | (num2 >> 27)) + H(num3, num4, num5) + m_x[num7++]) + -899497514);
			num3 = (num3 << 30) | (num3 >> 2);
			num5 += (uint)((int)(((num6 << 5) | (num6 >> 27)) + H(num2, num3, num4) + m_x[num7++]) + -899497514);
			num2 = (num2 << 30) | (num2 >> 2);
			num4 += (uint)((int)(((num5 << 5) | (num5 >> 27)) + H(num6, num2, num3) + m_x[num7++]) + -899497514);
			num6 = (num6 << 30) | (num6 >> 2);
			num3 += (uint)((int)(((num4 << 5) | (num4 >> 27)) + H(num5, num6, num2) + m_x[num7++]) + -899497514);
			num5 = (num5 << 30) | (num5 >> 2);
			num2 += (uint)((int)(((num3 << 5) | (num3 >> 27)) + H(num4, num5, num6) + m_x[num7++]) + -899497514);
			num4 = (num4 << 30) | (num4 >> 2);
		}
		m_h1 += num2;
		m_h2 += num3;
		m_h3 += num4;
		m_h4 += num5;
		m_h5 += num6;
		m_xOff = 0;
		Array.Clear(m_x, 0, 16);
	}

	private uint F(uint u, uint v, uint w)
	{
		return (u & v) | (~u & w);
	}

	private uint H(uint u, uint v, uint w)
	{
		return u ^ v ^ w;
	}

	private uint G(uint u, uint v, uint w)
	{
		return (u & v) | (u & w) | (v & w);
	}
}
