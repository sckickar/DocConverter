using System;

namespace DocGen.DocIO.ReaderWriter.Security;

[CLSCompliant(false)]
internal class MD5Context
{
	private static byte[] PADDING;

	private uint[] m_uiI = new uint[2];

	private uint[] m_buf = new uint[4] { 1732584193u, 4023233417u, 2562383102u, 271733878u };

	private byte[] m_in = new byte[64];

	private byte[] m_digest = new byte[16];

	internal uint[] I => m_uiI;

	internal uint[] Buffer => m_buf;

	internal byte[] InBuffer => m_in;

	internal byte[] Digest => m_digest;

	internal void Update(byte[] inBuf, uint inLen)
	{
		uint[] array = new uint[16];
		int num = (int)((m_uiI[0] >> 3) & 0x3F);
		if (m_uiI[0] + (inLen << 3) < m_uiI[0])
		{
			m_uiI[1]++;
		}
		m_uiI[0] += inLen << 3;
		m_uiI[1] += inLen >> 29;
		for (uint num2 = 0u; num2 < inLen; num2++)
		{
			m_in[num++] = inBuf[num2];
			if (num == 64)
			{
				uint num3 = 0u;
				uint num4 = 0u;
				while (num3 < 16)
				{
					array[num3] = (uint)((m_in[num4 + 3] << 24) | (m_in[num4 + 2] << 16) | (m_in[num4 + 1] << 8) | m_in[num4]);
					num3++;
					num4 += 4;
				}
				Transform(array);
				num = 0;
			}
		}
	}

	internal void FinalValue()
	{
		uint[] array = new uint[16]
		{
			0u,
			0u,
			0u,
			0u,
			0u,
			0u,
			0u,
			0u,
			0u,
			0u,
			0u,
			0u,
			0u,
			0u,
			m_uiI[0],
			m_uiI[1]
		};
		uint num = (m_uiI[0] >> 3) & 0x3Fu;
		uint inLen = ((num < 56) ? (56 - num) : (120 - num));
		Update(PADDING, inLen);
		uint num2 = 0u;
		uint num3 = 0u;
		while (num2 < 14)
		{
			array[num2] = (uint)((m_in[num3 + 3] << 24) | (m_in[num3 + 2] << 16) | (m_in[num3 + 1] << 8) | m_in[num3]);
			num2++;
			num3 += 4;
		}
		Transform(array);
		StoreDigest();
	}

	internal void StoreDigest()
	{
		uint num = 0u;
		uint num2 = 0u;
		while (num < 4)
		{
			m_digest[num2] = (byte)(m_buf[num] & 0xFFu);
			m_digest[num2 + 1] = (byte)((m_buf[num] >> 8) & 0xFFu);
			m_digest[num2 + 2] = (byte)((m_buf[num] >> 16) & 0xFFu);
			m_digest[num2 + 3] = (byte)((m_buf[num] >> 24) & 0xFFu);
			num++;
			num2 += 4;
		}
	}

	private uint F(uint x, uint y, uint z)
	{
		return (x & y) | (~x & z);
	}

	private uint G(uint x, uint y, uint z)
	{
		return (x & z) | (y & ~z);
	}

	private uint H(uint x, uint y, uint z)
	{
		return x ^ y ^ z;
	}

	private uint III(uint x, uint y, uint z)
	{
		return y ^ (x | ~z);
	}

	private uint ROTATE_LEFT(uint x, byte n)
	{
		return (x << (int)n) | (x >> 32 - n);
	}

	private void FF(ref uint a, uint b, uint c, uint d, uint x, byte s, uint ac)
	{
		a += F(b, c, d) + x + ac;
		a = ROTATE_LEFT(a, s);
		a += b;
	}

	private void GG(ref uint a, uint b, uint c, uint d, uint x, byte s, uint ac)
	{
		a += G(b, c, d) + x + ac;
		a = ROTATE_LEFT(a, s);
		a += b;
	}

	private void HH(ref uint a, uint b, uint c, uint d, uint x, byte s, uint ac)
	{
		a += H(b, c, d) + x + ac;
		a = ROTATE_LEFT(a, s);
		a += b;
	}

	private void II(ref uint a, uint b, uint c, uint d, uint x, byte s, uint ac)
	{
		a += III(b, c, d) + x + ac;
		a = ROTATE_LEFT(a, s);
		a += b;
	}

	private void Transform(uint[] inn)
	{
		uint a = m_buf[0];
		uint a2 = m_buf[1];
		uint a3 = m_buf[2];
		uint a4 = m_buf[3];
		FF(ref a, a2, a3, a4, inn[0], 7, 3614090360u);
		FF(ref a4, a, a2, a3, inn[1], 12, 3905402710u);
		FF(ref a3, a4, a, a2, inn[2], 17, 606105819u);
		FF(ref a2, a3, a4, a, inn[3], 22, 3250441966u);
		FF(ref a, a2, a3, a4, inn[4], 7, 4118548399u);
		FF(ref a4, a, a2, a3, inn[5], 12, 1200080426u);
		FF(ref a3, a4, a, a2, inn[6], 17, 2821735955u);
		FF(ref a2, a3, a4, a, inn[7], 22, 4249261313u);
		FF(ref a, a2, a3, a4, inn[8], 7, 1770035416u);
		FF(ref a4, a, a2, a3, inn[9], 12, 2336552879u);
		FF(ref a3, a4, a, a2, inn[10], 17, 4294925233u);
		FF(ref a2, a3, a4, a, inn[11], 22, 2304563134u);
		FF(ref a, a2, a3, a4, inn[12], 7, 1804603682u);
		FF(ref a4, a, a2, a3, inn[13], 12, 4254626195u);
		FF(ref a3, a4, a, a2, inn[14], 17, 2792965006u);
		FF(ref a2, a3, a4, a, inn[15], 22, 1236535329u);
		GG(ref a, a2, a3, a4, inn[1], 5, 4129170786u);
		GG(ref a4, a, a2, a3, inn[6], 9, 3225465664u);
		GG(ref a3, a4, a, a2, inn[11], 14, 643717713u);
		GG(ref a2, a3, a4, a, inn[0], 20, 3921069994u);
		GG(ref a, a2, a3, a4, inn[5], 5, 3593408605u);
		GG(ref a4, a, a2, a3, inn[10], 9, 38016083u);
		GG(ref a3, a4, a, a2, inn[15], 14, 3634488961u);
		GG(ref a2, a3, a4, a, inn[4], 20, 3889429448u);
		GG(ref a, a2, a3, a4, inn[9], 5, 568446438u);
		GG(ref a4, a, a2, a3, inn[14], 9, 3275163606u);
		GG(ref a3, a4, a, a2, inn[3], 14, 4107603335u);
		GG(ref a2, a3, a4, a, inn[8], 20, 1163531501u);
		GG(ref a, a2, a3, a4, inn[13], 5, 2850285829u);
		GG(ref a4, a, a2, a3, inn[2], 9, 4243563512u);
		GG(ref a3, a4, a, a2, inn[7], 14, 1735328473u);
		GG(ref a2, a3, a4, a, inn[12], 20, 2368359562u);
		HH(ref a, a2, a3, a4, inn[5], 4, 4294588738u);
		HH(ref a4, a, a2, a3, inn[8], 11, 2272392833u);
		HH(ref a3, a4, a, a2, inn[11], 16, 1839030562u);
		HH(ref a2, a3, a4, a, inn[14], 23, 4259657740u);
		HH(ref a, a2, a3, a4, inn[1], 4, 2763975236u);
		HH(ref a4, a, a2, a3, inn[4], 11, 1272893353u);
		HH(ref a3, a4, a, a2, inn[7], 16, 4139469664u);
		HH(ref a2, a3, a4, a, inn[10], 23, 3200236656u);
		HH(ref a, a2, a3, a4, inn[13], 4, 681279174u);
		HH(ref a4, a, a2, a3, inn[0], 11, 3936430074u);
		HH(ref a3, a4, a, a2, inn[3], 16, 3572445317u);
		HH(ref a2, a3, a4, a, inn[6], 23, 76029189u);
		HH(ref a, a2, a3, a4, inn[9], 4, 3654602809u);
		HH(ref a4, a, a2, a3, inn[12], 11, 3873151461u);
		HH(ref a3, a4, a, a2, inn[15], 16, 530742520u);
		HH(ref a2, a3, a4, a, inn[2], 23, 3299628645u);
		II(ref a, a2, a3, a4, inn[0], 6, 4096336452u);
		II(ref a4, a, a2, a3, inn[7], 10, 1126891415u);
		II(ref a3, a4, a, a2, inn[14], 15, 2878612391u);
		II(ref a2, a3, a4, a, inn[5], 21, 4237533241u);
		II(ref a, a2, a3, a4, inn[12], 6, 1700485571u);
		II(ref a4, a, a2, a3, inn[3], 10, 2399980690u);
		II(ref a3, a4, a, a2, inn[10], 15, 4293915773u);
		II(ref a2, a3, a4, a, inn[1], 21, 2240044497u);
		II(ref a, a2, a3, a4, inn[8], 6, 1873313359u);
		II(ref a4, a, a2, a3, inn[15], 10, 4264355552u);
		II(ref a3, a4, a, a2, inn[6], 15, 2734768916u);
		II(ref a2, a3, a4, a, inn[13], 21, 1309151649u);
		II(ref a, a2, a3, a4, inn[4], 6, 4149444226u);
		II(ref a4, a, a2, a3, inn[11], 10, 3174756917u);
		II(ref a3, a4, a, a2, inn[2], 15, 718787259u);
		II(ref a2, a3, a4, a, inn[9], 21, 3951481745u);
		m_buf[0] += a;
		m_buf[1] += a2;
		m_buf[2] += a3;
		m_buf[3] += a4;
	}

	static MD5Context()
	{
		byte[] array = new byte[64];
		array[0] = 128;
		PADDING = array;
	}
}
