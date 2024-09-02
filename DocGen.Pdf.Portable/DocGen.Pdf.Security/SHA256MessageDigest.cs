using System;

namespace DocGen.Pdf.Security;

internal class SHA256MessageDigest : MessageDigest
{
	private const int c_digestLength = 32;

	private uint m_h1;

	private uint m_h2;

	private uint m_h3;

	private uint m_h4;

	private uint m_h5;

	private uint m_h6;

	private uint m_h7;

	private uint m_h8;

	private uint[] m_x = new uint[64];

	private int m_xOff;

	private static readonly uint[] K = new uint[64]
	{
		1116352408u, 1899447441u, 3049323471u, 3921009573u, 961987163u, 1508970993u, 2453635748u, 2870763221u, 3624381080u, 310598401u,
		607225278u, 1426881987u, 1925078388u, 2162078206u, 2614888103u, 3248222580u, 3835390401u, 4022224774u, 264347078u, 604807628u,
		770255983u, 1249150122u, 1555081692u, 1996064986u, 2554220882u, 2821834349u, 2952996808u, 3210313671u, 3336571891u, 3584528711u,
		113926993u, 338241895u, 666307205u, 773529912u, 1294757372u, 1396182291u, 1695183700u, 1986661051u, 2177026350u, 2456956037u,
		2730485921u, 2820302411u, 3259730800u, 3345764771u, 3516065817u, 3600352804u, 4094571909u, 275423344u, 430227734u, 506948616u,
		659060556u, 883997877u, 958139571u, 1322822218u, 1537002063u, 1747873779u, 1955562222u, 2024104815u, 2227730452u, 2361852424u,
		2428436474u, 2756734187u, 3204031479u, 3329325298u
	};

	public override string AlgorithmName => "SHA-256";

	public override int MessageDigestSize => 32;

	internal SHA256MessageDigest()
	{
		initHs();
	}

	internal SHA256MessageDigest(SHA256MessageDigest t)
		: base(t)
	{
		m_h1 = t.m_h1;
		m_h2 = t.m_h2;
		m_h3 = t.m_h3;
		m_h4 = t.m_h4;
		m_h5 = t.m_h5;
		m_h6 = t.m_h6;
		m_h7 = t.m_h7;
		m_h8 = t.m_h8;
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
		Asn1Constants.UInt32ToBe(m_h6, bytes, offset + 20);
		Asn1Constants.UInt32ToBe(m_h7, bytes, offset + 24);
		Asn1Constants.UInt32ToBe(m_h8, bytes, offset + 28);
		Reset();
		return 32;
	}

	public override void Reset()
	{
		base.Reset();
		initHs();
		m_xOff = 0;
		Array.Clear(m_x, 0, m_x.Length);
	}

	private void initHs()
	{
		m_h1 = 1779033703u;
		m_h2 = 3144134277u;
		m_h3 = 1013904242u;
		m_h4 = 2773480762u;
		m_h5 = 1359893119u;
		m_h6 = 2600822924u;
		m_h7 = 528734635u;
		m_h8 = 1541459225u;
	}

	internal override void ProcessBlock()
	{
		for (int i = 16; i <= 63; i++)
		{
			m_x[i] = Theta1(m_x[i - 2]) + m_x[i - 7] + Theta0(m_x[i - 15]) + m_x[i - 16];
		}
		uint num = m_h1;
		uint num2 = m_h2;
		uint num3 = m_h3;
		uint num4 = m_h4;
		uint num5 = m_h5;
		uint num6 = m_h6;
		uint num7 = m_h7;
		uint num8 = m_h8;
		int num9 = 0;
		for (int j = 0; j < 8; j++)
		{
			num8 += Sum1Ch(num5, num6, num7) + K[num9] + m_x[num9];
			num4 += num8;
			num8 += Sum0Maj(num, num2, num3);
			num9++;
			num7 += Sum1Ch(num4, num5, num6) + K[num9] + m_x[num9];
			num3 += num7;
			num7 += Sum0Maj(num8, num, num2);
			num9++;
			num6 += Sum1Ch(num3, num4, num5) + K[num9] + m_x[num9];
			num2 += num6;
			num6 += Sum0Maj(num7, num8, num);
			num9++;
			num5 += Sum1Ch(num2, num3, num4) + K[num9] + m_x[num9];
			num += num5;
			num5 += Sum0Maj(num6, num7, num8);
			num9++;
			num4 += Sum1Ch(num, num2, num3) + K[num9] + m_x[num9];
			num8 += num4;
			num4 += Sum0Maj(num5, num6, num7);
			num9++;
			num3 += Sum1Ch(num8, num, num2) + K[num9] + m_x[num9];
			num7 += num3;
			num3 += Sum0Maj(num4, num5, num6);
			num9++;
			num2 += Sum1Ch(num7, num8, num) + K[num9] + m_x[num9];
			num6 += num2;
			num2 += Sum0Maj(num3, num4, num5);
			num9++;
			num += Sum1Ch(num6, num7, num8) + K[num9] + m_x[num9];
			num5 += num;
			num += Sum0Maj(num2, num3, num4);
			num9++;
		}
		m_h1 += num;
		m_h2 += num2;
		m_h3 += num3;
		m_h4 += num4;
		m_h5 += num5;
		m_h6 += num6;
		m_h7 += num7;
		m_h8 += num8;
		m_xOff = 0;
		Array.Clear(m_x, 0, 16);
	}

	private static uint Sum1Ch(uint x, uint y, uint z)
	{
		return (((x >> 6) | (x << 26)) ^ ((x >> 11) | (x << 21)) ^ ((x >> 25) | (x << 7))) + ((x & y) ^ (~x & z));
	}

	private static uint Sum0Maj(uint x, uint y, uint z)
	{
		return (((x >> 2) | (x << 30)) ^ ((x >> 13) | (x << 19)) ^ ((x >> 22) | (x << 10))) + ((x & y) ^ (x & z) ^ (y & z));
	}

	private static uint Theta0(uint x)
	{
		return ((x >> 7) | (x << 25)) ^ ((x >> 18) | (x << 14)) ^ (x >> 3);
	}

	private static uint Theta1(uint x)
	{
		return ((x >> 17) | (x << 15)) ^ ((x >> 19) | (x << 13)) ^ (x >> 10);
	}
}
