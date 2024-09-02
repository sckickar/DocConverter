using System;

namespace DocGen.Pdf.Security;

internal class MessageDigest5 : MessageDigest
{
	private const int m_digestLength = 16;

	private uint m_a;

	private uint m_b;

	private uint m_c;

	private uint m_d;

	private uint[] m_localarray = new uint[16];

	private int m_offset;

	private static readonly int r11 = 7;

	private static readonly int r12 = 12;

	private static readonly int r13 = 17;

	private static readonly int r14 = 22;

	private static readonly int r21 = 5;

	private static readonly int r22 = 9;

	private static readonly int r23 = 14;

	private static readonly int r24 = 20;

	private static readonly int r31 = 4;

	private static readonly int r32 = 11;

	private static readonly int r33 = 16;

	private static readonly int r34 = 23;

	private static readonly int r41 = 6;

	private static readonly int r42 = 10;

	private static readonly int r43 = 15;

	private static readonly int r44 = 21;

	public override string AlgorithmName => "MD5";

	public override int MessageDigestSize => 16;

	public MessageDigest5()
	{
		Reset();
	}

	public MessageDigest5(MessageDigest5 md5)
		: base(md5)
	{
		m_a = md5.m_a;
		m_b = md5.m_b;
		m_c = md5.m_c;
		m_d = md5.m_d;
		Array.Copy(md5.m_localarray, 0, m_localarray, 0, md5.m_localarray.Length);
		m_offset = md5.m_offset;
	}

	internal override void ProcessWord(byte[] input, int inOff)
	{
		m_localarray[m_offset] = Asn1Constants.LeToUInt32(input, inOff);
		if (++m_offset == 16)
		{
			ProcessBlock();
		}
	}

	internal override void ProcessLength(long Length)
	{
		if (m_offset > 14)
		{
			if (m_offset == 15)
			{
				m_localarray[15] = 0u;
			}
			ProcessBlock();
		}
		for (int i = m_offset; i < 14; i++)
		{
			m_localarray[i] = 0u;
		}
		m_localarray[14] = (uint)Length;
		m_localarray[15] = (uint)((ulong)Length >> 32);
	}

	internal override void ProcessBlock()
	{
		uint a = m_a;
		uint b = m_b;
		uint c = m_c;
		uint d = m_d;
		a = RotateLeft(a + F1(b, c, d) + m_localarray[0] + 3614090360u, r11) + b;
		d = RotateLeft(d + F1(a, b, c) + m_localarray[1] + 3905402710u, r12) + a;
		c = RotateLeft(c + F1(d, a, b) + m_localarray[2] + 606105819, r13) + d;
		b = RotateLeft(b + F1(c, d, a) + m_localarray[3] + 3250441966u, r14) + c;
		a = RotateLeft(a + F1(b, c, d) + m_localarray[4] + 4118548399u, r11) + b;
		d = RotateLeft(d + F1(a, b, c) + m_localarray[5] + 1200080426, r12) + a;
		c = RotateLeft(c + F1(d, a, b) + m_localarray[6] + 2821735955u, r13) + d;
		b = RotateLeft(b + F1(c, d, a) + m_localarray[7] + 4249261313u, r14) + c;
		a = RotateLeft(a + F1(b, c, d) + m_localarray[8] + 1770035416, r11) + b;
		d = RotateLeft(d + F1(a, b, c) + m_localarray[9] + 2336552879u, r12) + a;
		c = RotateLeft(c + F1(d, a, b) + m_localarray[10] + 4294925233u, r13) + d;
		b = RotateLeft(b + F1(c, d, a) + m_localarray[11] + 2304563134u, r14) + c;
		a = RotateLeft(a + F1(b, c, d) + m_localarray[12] + 1804603682, r11) + b;
		d = RotateLeft(d + F1(a, b, c) + m_localarray[13] + 4254626195u, r12) + a;
		c = RotateLeft(c + F1(d, a, b) + m_localarray[14] + 2792965006u, r13) + d;
		b = RotateLeft(b + F1(c, d, a) + m_localarray[15] + 1236535329, r14) + c;
		a = RotateLeft(a + F2(b, c, d) + m_localarray[1] + 4129170786u, r21) + b;
		d = RotateLeft(d + F2(a, b, c) + m_localarray[6] + 3225465664u, r22) + a;
		c = RotateLeft(c + F2(d, a, b) + m_localarray[11] + 643717713, r23) + d;
		b = RotateLeft(b + F2(c, d, a) + m_localarray[0] + 3921069994u, r24) + c;
		a = RotateLeft(a + F2(b, c, d) + m_localarray[5] + 3593408605u, r21) + b;
		d = RotateLeft(d + F2(a, b, c) + m_localarray[10] + 38016083, r22) + a;
		c = RotateLeft(c + F2(d, a, b) + m_localarray[15] + 3634488961u, r23) + d;
		b = RotateLeft(b + F2(c, d, a) + m_localarray[4] + 3889429448u, r24) + c;
		a = RotateLeft(a + F2(b, c, d) + m_localarray[9] + 568446438, r21) + b;
		d = RotateLeft(d + F2(a, b, c) + m_localarray[14] + 3275163606u, r22) + a;
		c = RotateLeft(c + F2(d, a, b) + m_localarray[3] + 4107603335u, r23) + d;
		b = RotateLeft(b + F2(c, d, a) + m_localarray[8] + 1163531501, r24) + c;
		a = RotateLeft(a + F2(b, c, d) + m_localarray[13] + 2850285829u, r21) + b;
		d = RotateLeft(d + F2(a, b, c) + m_localarray[2] + 4243563512u, r22) + a;
		c = RotateLeft(c + F2(d, a, b) + m_localarray[7] + 1735328473, r23) + d;
		b = RotateLeft(b + F2(c, d, a) + m_localarray[12] + 2368359562u, r24) + c;
		a = RotateLeft(a + F3(b, c, d) + m_localarray[5] + 4294588738u, r31) + b;
		d = RotateLeft(d + F3(a, b, c) + m_localarray[8] + 2272392833u, r32) + a;
		c = RotateLeft(c + F3(d, a, b) + m_localarray[11] + 1839030562, r33) + d;
		b = RotateLeft(b + F3(c, d, a) + m_localarray[14] + 4259657740u, r34) + c;
		a = RotateLeft(a + F3(b, c, d) + m_localarray[1] + 2763975236u, r31) + b;
		d = RotateLeft(d + F3(a, b, c) + m_localarray[4] + 1272893353, r32) + a;
		c = RotateLeft(c + F3(d, a, b) + m_localarray[7] + 4139469664u, r33) + d;
		b = RotateLeft(b + F3(c, d, a) + m_localarray[10] + 3200236656u, r34) + c;
		a = RotateLeft(a + F3(b, c, d) + m_localarray[13] + 681279174, r31) + b;
		d = RotateLeft(d + F3(a, b, c) + m_localarray[0] + 3936430074u, r32) + a;
		c = RotateLeft(c + F3(d, a, b) + m_localarray[3] + 3572445317u, r33) + d;
		b = RotateLeft(b + F3(c, d, a) + m_localarray[6] + 76029189, r34) + c;
		a = RotateLeft(a + F3(b, c, d) + m_localarray[9] + 3654602809u, r31) + b;
		d = RotateLeft(d + F3(a, b, c) + m_localarray[12] + 3873151461u, r32) + a;
		c = RotateLeft(c + F3(d, a, b) + m_localarray[15] + 530742520, r33) + d;
		b = RotateLeft(b + F3(c, d, a) + m_localarray[2] + 3299628645u, r34) + c;
		a = RotateLeft(a + F4(b, c, d) + m_localarray[0] + 4096336452u, r41) + b;
		d = RotateLeft(d + F4(a, b, c) + m_localarray[7] + 1126891415, r42) + a;
		c = RotateLeft(c + F4(d, a, b) + m_localarray[14] + 2878612391u, r43) + d;
		b = RotateLeft(b + F4(c, d, a) + m_localarray[5] + 4237533241u, r44) + c;
		a = RotateLeft(a + F4(b, c, d) + m_localarray[12] + 1700485571, r41) + b;
		d = RotateLeft(d + F4(a, b, c) + m_localarray[3] + 2399980690u, r42) + a;
		c = RotateLeft(c + F4(d, a, b) + m_localarray[10] + 4293915773u, r43) + d;
		b = RotateLeft(b + F4(c, d, a) + m_localarray[1] + 2240044497u, r44) + c;
		a = RotateLeft(a + F4(b, c, d) + m_localarray[8] + 1873313359, r41) + b;
		d = RotateLeft(d + F4(a, b, c) + m_localarray[15] + 4264355552u, r42) + a;
		c = RotateLeft(c + F4(d, a, b) + m_localarray[6] + 2734768916u, r43) + d;
		b = RotateLeft(b + F4(c, d, a) + m_localarray[13] + 1309151649, r44) + c;
		a = RotateLeft(a + F4(b, c, d) + m_localarray[4] + 4149444226u, r41) + b;
		d = RotateLeft(d + F4(a, b, c) + m_localarray[11] + 3174756917u, r42) + a;
		c = RotateLeft(c + F4(d, a, b) + m_localarray[2] + 718787259, r43) + d;
		b = RotateLeft(b + F4(c, d, a) + m_localarray[9] + 3951481745u, r44) + c;
		m_a += a;
		m_b += b;
		m_c += c;
		m_d += d;
		m_offset = 0;
	}

	public override int DoFinal(byte[] bytes, int offset)
	{
		Finish();
		Asn1Constants.UInt32ToLe(m_a, bytes, offset);
		Asn1Constants.UInt32ToLe(m_b, bytes, offset + 4);
		Asn1Constants.UInt32ToLe(m_c, bytes, offset + 8);
		Asn1Constants.UInt32ToLe(m_d, bytes, offset + 12);
		Reset();
		return 16;
	}

	private uint RotateLeft(uint x, int n)
	{
		return (x << n) | (x >> 32 - n);
	}

	private uint F1(uint u, uint v, uint w)
	{
		return (u & v) | (~u & w);
	}

	private uint F2(uint u, uint v, uint w)
	{
		return (u & w) | (v & ~w);
	}

	private uint F3(uint u, uint v, uint w)
	{
		return u ^ v ^ w;
	}

	private uint F4(uint u, uint v, uint w)
	{
		return v ^ (u | ~w);
	}

	public override void Reset()
	{
		base.Reset();
		m_a = 1732584193u;
		m_b = 4023233417u;
		m_c = 2562383102u;
		m_d = 271733878u;
		m_offset = 0;
		for (int i = 0; i != m_localarray.Length; i++)
		{
			m_localarray[i] = 0u;
		}
	}
}
