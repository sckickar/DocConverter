using System;

namespace DocGen.Pdf.Security;

internal abstract class BigDigest : IMessageDigest
{
	private int m_length = 128;

	private byte[] m_buf;

	private int m_bufOffset;

	private long m_byte1;

	private long m_byte2;

	private ulong m_h1;

	private ulong m_h2;

	private ulong m_h3;

	private ulong m_h4;

	private ulong m_h5;

	private ulong m_h6;

	private ulong m_h7;

	private ulong m_h8;

	private ulong[] m_word = new ulong[80];

	private int m_wordOffset;

	internal static readonly ulong[] K = new ulong[80]
	{
		4794697086780616226uL, 8158064640168781261uL, 13096744586834688815uL, 16840607885511220156uL, 4131703408338449720uL, 6480981068601479193uL, 10538285296894168987uL, 12329834152419229976uL, 15566598209576043074uL, 1334009975649890238uL,
		2608012711638119052uL, 6128411473006802146uL, 8268148722764581231uL, 9286055187155687089uL, 11230858885718282805uL, 13951009754708518548uL, 16472876342353939154uL, 17275323862435702243uL, 1135362057144423861uL, 2597628984639134821uL,
		3308224258029322869uL, 5365058923640841347uL, 6679025012923562964uL, 8573033837759648693uL, 10970295158949994411uL, 12119686244451234320uL, 12683024718118986047uL, 13788192230050041572uL, 14330467153632333762uL, 15395433587784984357uL,
		489312712824947311uL, 1452737877330783856uL, 2861767655752347644uL, 3322285676063803686uL, 5560940570517711597uL, 5996557281743188959uL, 7280758554555802590uL, 8532644243296465576uL, 9350256976987008742uL, 10552545826968843579uL,
		11727347734174303076uL, 12113106623233404929uL, 14000437183269869457uL, 14369950271660146224uL, 15101387698204529176uL, 15463397548674623760uL, 17586052441742319658uL, 1182934255886127544uL, 1847814050463011016uL, 2177327727835720531uL,
		2830643537854262169uL, 3796741975233480872uL, 4115178125766777443uL, 5681478168544905931uL, 6601373596472566643uL, 7507060721942968483uL, 8399075790359081724uL, 8693463985226723168uL, 9568029438360202098uL, 10144078919501101548uL,
		10430055236837252648uL, 11840083180663258601uL, 13761210420658862357uL, 14299343276471374635uL, 14566680578165727644uL, 15097957966210449927uL, 16922976911328602910uL, 17689382322260857208uL, 500013540394364858uL, 748580250866718886uL,
		1242879168328830382uL, 1977374033974150939uL, 2944078676154940804uL, 3659926193048069267uL, 4368137639120453308uL, 4836135668995329356uL, 5532061633213252278uL, 6448918945643986474uL, 6902733635092675308uL, 7801388544844847127uL
	};

	internal ulong Header1
	{
		get
		{
			return m_h1;
		}
		set
		{
			m_h1 = value;
		}
	}

	internal ulong Header2
	{
		get
		{
			return m_h2;
		}
		set
		{
			m_h2 = value;
		}
	}

	internal ulong Header3
	{
		get
		{
			return m_h3;
		}
		set
		{
			m_h3 = value;
		}
	}

	internal ulong Header4
	{
		get
		{
			return m_h4;
		}
		set
		{
			m_h4 = value;
		}
	}

	internal ulong Header5
	{
		get
		{
			return m_h5;
		}
		set
		{
			m_h5 = value;
		}
	}

	internal ulong Header6
	{
		get
		{
			return m_h6;
		}
		set
		{
			m_h6 = value;
		}
	}

	internal ulong Header7
	{
		get
		{
			return m_h7;
		}
		set
		{
			m_h7 = value;
		}
	}

	internal ulong Header8
	{
		get
		{
			return m_h8;
		}
		set
		{
			m_h8 = value;
		}
	}

	public int ByteLength => m_length;

	public abstract string AlgorithmName { get; }

	public abstract int MessageDigestSize { get; }

	internal BigDigest()
	{
		m_buf = new byte[8];
		Reset();
	}

	internal BigDigest(BigDigest t)
	{
		m_buf = new byte[t.m_buf.Length];
		Array.Copy(t.m_buf, 0, m_buf, 0, t.m_buf.Length);
		m_bufOffset = t.m_bufOffset;
		m_byte1 = t.m_byte1;
		m_byte2 = t.m_byte2;
		m_h1 = t.m_h1;
		m_h2 = t.m_h2;
		m_h3 = t.m_h3;
		m_h4 = t.m_h4;
		m_h5 = t.m_h5;
		m_h6 = t.m_h6;
		m_h7 = t.m_h7;
		m_h8 = t.m_h8;
		Array.Copy(t.m_word, 0, m_word, 0, t.m_word.Length);
		m_wordOffset = t.m_wordOffset;
	}

	public void Update(byte input)
	{
		m_buf[m_bufOffset++] = input;
		if (m_bufOffset == m_buf.Length)
		{
			ProcessWord(m_buf, 0);
			m_bufOffset = 0;
		}
		m_byte1++;
	}

	public void Update(byte[] bytes, int offset, int length)
	{
		while (m_bufOffset != 0 && length > 0)
		{
			Update(bytes[offset]);
			offset++;
			length--;
		}
		while (length > m_buf.Length)
		{
			ProcessWord(bytes, offset);
			offset += m_buf.Length;
			length -= m_buf.Length;
			m_byte1 += m_buf.Length;
		}
		while (length > 0)
		{
			Update(bytes[offset]);
			offset++;
			length--;
		}
	}

	public void BlockUpdate(byte[] bytes, int offset, int length)
	{
		while (m_bufOffset != 0 && length > 0)
		{
			Update(bytes[offset]);
			offset++;
			length--;
		}
		while (length > m_buf.Length)
		{
			ProcessWord(bytes, offset);
			offset += m_buf.Length;
			length -= m_buf.Length;
			m_byte1 += m_buf.Length;
		}
		while (length > 0)
		{
			Update(bytes[offset]);
			offset++;
			length--;
		}
	}

	public void Finish()
	{
		ChangeByteCounts();
		long lowW = m_byte1 << 3;
		long @byte = m_byte2;
		Update(128);
		while (m_bufOffset != 0)
		{
			Update(0);
		}
		ProcessLength(lowW, @byte);
		ProcessBlock();
	}

	public virtual void Reset()
	{
		m_byte1 = 0L;
		m_byte2 = 0L;
		m_bufOffset = 0;
		for (int i = 0; i < m_buf.Length; i++)
		{
			m_buf[i] = 0;
		}
		m_wordOffset = 0;
		Array.Clear(m_word, 0, m_word.Length);
	}

	internal void ProcessWord(byte[] input, int inOff)
	{
		m_word[m_wordOffset] = Asn1Constants.BeToUInt64(input, inOff);
		if (++m_wordOffset == 16)
		{
			ProcessBlock();
		}
	}

	private void ChangeByteCounts()
	{
		if (m_byte1 > 2305843009213693951L)
		{
			m_byte2 += m_byte1 >>> 61;
			m_byte1 &= 2305843009213693951L;
		}
	}

	internal void ProcessLength(long lowW, long hiW)
	{
		if (m_wordOffset > 14)
		{
			ProcessBlock();
		}
		m_word[14] = (ulong)hiW;
		m_word[15] = (ulong)lowW;
	}

	internal void ProcessBlock()
	{
		ChangeByteCounts();
		for (int i = 16; i <= 79; i++)
		{
			m_word[i] = Op6(m_word[i - 2]) + m_word[i - 7] + Op5(m_word[i - 15]) + m_word[i - 16];
		}
		ulong num = m_h1;
		ulong num2 = m_h2;
		ulong num3 = m_h3;
		ulong num4 = m_h4;
		ulong num5 = m_h5;
		ulong num6 = m_h6;
		ulong num7 = m_h7;
		ulong num8 = m_h8;
		int num9 = 0;
		for (int j = 0; j < 10; j++)
		{
			num8 += Op4(num5) + Op1(num5, num6, num7) + K[num9] + m_word[num9++];
			num4 += num8;
			num8 += Op3(num) + Op2(num, num2, num3);
			num7 += Op4(num4) + Op1(num4, num5, num6) + K[num9] + m_word[num9++];
			num3 += num7;
			num7 += Op3(num8) + Op2(num8, num, num2);
			num6 += Op4(num3) + Op1(num3, num4, num5) + K[num9] + m_word[num9++];
			num2 += num6;
			num6 += Op3(num7) + Op2(num7, num8, num);
			num5 += Op4(num2) + Op1(num2, num3, num4) + K[num9] + m_word[num9++];
			num += num5;
			num5 += Op3(num6) + Op2(num6, num7, num8);
			num4 += Op4(num) + Op1(num, num2, num3) + K[num9] + m_word[num9++];
			num8 += num4;
			num4 += Op3(num5) + Op2(num5, num6, num7);
			num3 += Op4(num8) + Op1(num8, num, num2) + K[num9] + m_word[num9++];
			num7 += num3;
			num3 += Op3(num4) + Op2(num4, num5, num6);
			num2 += Op4(num7) + Op1(num7, num8, num) + K[num9] + m_word[num9++];
			num6 += num2;
			num2 += Op3(num3) + Op2(num3, num4, num5);
			num += Op4(num6) + Op1(num6, num7, num8) + K[num9] + m_word[num9++];
			num5 += num;
			num += Op3(num2) + Op2(num2, num3, num4);
		}
		m_h1 += num;
		m_h2 += num2;
		m_h3 += num3;
		m_h4 += num4;
		m_h5 += num5;
		m_h6 += num6;
		m_h7 += num7;
		m_h8 += num8;
		m_wordOffset = 0;
		Array.Clear(m_word, 0, 16);
	}

	private static ulong Op1(ulong x, ulong y, ulong z)
	{
		return (x & y) ^ (~x & z);
	}

	private static ulong Op2(ulong x, ulong y, ulong z)
	{
		return (x & y) ^ (x & z) ^ (y & z);
	}

	private static ulong Op3(ulong x)
	{
		return ((x << 36) | (x >> 28)) ^ ((x << 30) | (x >> 34)) ^ ((x << 25) | (x >> 39));
	}

	private static ulong Op4(ulong x)
	{
		return ((x << 50) | (x >> 14)) ^ ((x << 46) | (x >> 18)) ^ ((x << 23) | (x >> 41));
	}

	private static ulong Op5(ulong x)
	{
		return ((x << 63) | (x >> 1)) ^ ((x << 56) | (x >> 8)) ^ (x >> 7);
	}

	private static ulong Op6(ulong x)
	{
		return ((x << 45) | (x >> 19)) ^ ((x << 3) | (x >> 61)) ^ (x >> 6);
	}

	public abstract int DoFinal(byte[] bytes, int offset);
}
