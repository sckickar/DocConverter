using System;

namespace DocGen.Pdf;

internal class MQDecoder
{
	internal static readonly uint[] qe = new uint[47]
	{
		22017u, 13313u, 6145u, 2753u, 1313u, 545u, 22017u, 21505u, 18433u, 14337u,
		12289u, 9217u, 7169u, 5633u, 22017u, 21505u, 20737u, 18433u, 14337u, 13313u,
		12289u, 10241u, 9217u, 8705u, 7169u, 6145u, 5633u, 5121u, 4609u, 4353u,
		2753u, 2497u, 2209u, 1313u, 1089u, 673u, 545u, 321u, 273u, 133u,
		73u, 37u, 21u, 9u, 5u, 1u, 22017u
	};

	internal static readonly int[] nMPS = new int[47]
	{
		1, 2, 3, 4, 5, 38, 7, 8, 9, 10,
		11, 12, 13, 29, 15, 16, 17, 18, 19, 20,
		21, 22, 23, 24, 25, 26, 27, 28, 29, 30,
		31, 32, 33, 34, 35, 36, 37, 38, 39, 40,
		41, 42, 43, 44, 45, 45, 46
	};

	internal static readonly int[] nLPS = new int[47]
	{
		1, 6, 9, 12, 29, 33, 6, 14, 14, 14,
		17, 18, 20, 21, 14, 14, 15, 16, 17, 18,
		19, 19, 20, 21, 22, 23, 24, 25, 26, 27,
		28, 29, 30, 31, 32, 33, 34, 35, 36, 37,
		38, 39, 40, 41, 42, 43, 46
	};

	internal static readonly int[] switchLM = new int[47]
	{
		1, 0, 0, 0, 0, 0, 1, 0, 0, 0,
		0, 0, 0, 0, 1, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0
	};

	internal ByteInputBuffer in_Renamed;

	internal int[] mPS;

	internal int[] I;

	internal uint c;

	internal uint cT;

	internal uint a;

	internal uint b;

	internal bool markerFound;

	internal int[] initStates;

	public virtual int NumCtxts => I.Length;

	public virtual ByteInputBuffer ByteInputBuffer => in_Renamed;

	public MQDecoder(ByteInputBuffer iStream, int nrOfContexts, int[] initStates)
	{
		in_Renamed = iStream;
		I = new int[nrOfContexts];
		mPS = new int[nrOfContexts];
		this.initStates = initStates;
		init();
		resetCtxts();
	}

	internal bool fastDecodeSymbols(int[] bits, int ctxt, uint n)
	{
		int num = I[ctxt];
		uint num2 = qe[num];
		if (num2 < 16384 && n <= (a - (c >> 16) - 1) / num2 && n <= (a - 32768) / num2 + 1)
		{
			a -= n * num2;
			if (a >= 32768)
			{
				bits[0] = mPS[ctxt];
				return true;
			}
			I[ctxt] = nMPS[num];
			if (cT == 0)
			{
				byteIn();
			}
			a <<= 1;
			c <<= 1;
			cT--;
			bits[0] = mPS[ctxt];
			return true;
		}
		uint num3 = a;
		for (int i = 0; i < n; i++)
		{
			num3 -= num2;
			if (c >> 16 < num3)
			{
				if (num3 >= 32768)
				{
					bits[i] = mPS[ctxt];
					continue;
				}
				if (num3 >= num2)
				{
					bits[i] = mPS[ctxt];
					num = nMPS[num];
					num2 = qe[num];
					if (cT == 0)
					{
						byteIn();
					}
					num3 <<= 1;
					c <<= 1;
					cT--;
					continue;
				}
				bits[i] = 1 - mPS[ctxt];
				if (switchLM[num] == 1)
				{
					mPS[ctxt] = 1 - mPS[ctxt];
				}
				num = nLPS[num];
				num2 = qe[num];
				do
				{
					if (cT == 0)
					{
						byteIn();
					}
					num3 <<= 1;
					c <<= 1;
					cT--;
				}
				while (num3 < 32768);
				continue;
			}
			c -= num3 << 16;
			if (num3 < num2)
			{
				num3 = num2;
				bits[i] = mPS[ctxt];
				num = nMPS[num];
				num2 = qe[num];
				if (cT == 0)
				{
					byteIn();
				}
				num3 <<= 1;
				c <<= 1;
				cT--;
				continue;
			}
			num3 = num2;
			bits[i] = 1 - mPS[ctxt];
			if (switchLM[num] == 1)
			{
				mPS[ctxt] = 1 - mPS[ctxt];
			}
			num = nLPS[num];
			num2 = qe[num];
			do
			{
				if (cT == 0)
				{
					byteIn();
				}
				num3 <<= 1;
				c <<= 1;
				cT--;
			}
			while (num3 < 32768);
		}
		a = num3;
		I[ctxt] = num;
		return false;
	}

	public void decodeSymbols(int[] bits, int[] cX, int n)
	{
		for (int i = 0; i < n; i++)
		{
			int num = cX[i];
			int num2 = I[num];
			uint num3 = qe[num2];
			a -= num3;
			uint num4;
			if (c >> 16 < a)
			{
				if (a >= 32768)
				{
					bits[i] = mPS[num];
					continue;
				}
				num4 = a;
				if (num4 >= num3)
				{
					bits[i] = mPS[num];
					I[num] = nMPS[num2];
					if (cT == 0)
					{
						byteIn();
					}
					num4 <<= 1;
					c <<= 1;
					cT--;
				}
				else
				{
					bits[i] = 1 - mPS[num];
					if (switchLM[num2] == 1)
					{
						mPS[num] = 1 - mPS[num];
					}
					I[num] = nLPS[num2];
					do
					{
						if (cT == 0)
						{
							byteIn();
						}
						num4 <<= 1;
						c <<= 1;
						cT--;
					}
					while (num4 < 32768);
				}
				a = num4;
				continue;
			}
			num4 = a;
			c -= num4 << 16;
			if (num4 < num3)
			{
				num4 = num3;
				bits[i] = mPS[num];
				I[num] = nMPS[num2];
				if (cT == 0)
				{
					byteIn();
				}
				num4 <<= 1;
				c <<= 1;
				cT--;
			}
			else
			{
				num4 = num3;
				bits[i] = 1 - mPS[num];
				if (switchLM[num2] == 1)
				{
					mPS[num] = 1 - mPS[num];
				}
				I[num] = nLPS[num2];
				do
				{
					if (cT == 0)
					{
						byteIn();
					}
					num4 <<= 1;
					c <<= 1;
					cT--;
				}
				while (num4 < 32768);
			}
			a = num4;
		}
	}

	public int decodeSymbol(int context)
	{
		int num = I[context];
		uint num2 = qe[num];
		a -= num2;
		int result;
		if (c >> 16 < a)
		{
			if (a >= 32768)
			{
				result = mPS[context];
			}
			else
			{
				uint num3 = a;
				if (num3 >= num2)
				{
					result = mPS[context];
					I[context] = nMPS[num];
					if (cT == 0)
					{
						byteIn();
					}
					num3 <<= 1;
					c <<= 1;
					cT--;
				}
				else
				{
					result = 1 - mPS[context];
					if (switchLM[num] == 1)
					{
						mPS[context] = 1 - mPS[context];
					}
					I[context] = nLPS[num];
					do
					{
						if (cT == 0)
						{
							byteIn();
						}
						num3 <<= 1;
						c <<= 1;
						cT--;
					}
					while (num3 < 32768);
				}
				a = num3;
			}
		}
		else
		{
			uint num3 = a;
			c -= num3 << 16;
			if (num3 < num2)
			{
				num3 = num2;
				result = mPS[context];
				I[context] = nMPS[num];
				if (cT == 0)
				{
					byteIn();
				}
				num3 <<= 1;
				c <<= 1;
				cT--;
			}
			else
			{
				num3 = num2;
				result = 1 - mPS[context];
				if (switchLM[num] == 1)
				{
					mPS[context] = 1 - mPS[context];
				}
				I[context] = nLPS[num];
				do
				{
					if (cT == 0)
					{
						byteIn();
					}
					num3 <<= 1;
					c <<= 1;
					cT--;
				}
				while (num3 < 32768);
			}
			a = num3;
		}
		return result;
	}

	public virtual bool checkPredTerm()
	{
		if (b != 255 && !markerFound)
		{
			return true;
		}
		if (cT != 0 && !markerFound)
		{
			return true;
		}
		if (cT == 1)
		{
			return false;
		}
		if (cT == 0)
		{
			if (!markerFound)
			{
				b = (uint)in_Renamed.read() & 0xFFu;
				if (b <= 143)
				{
					return true;
				}
			}
			cT = 8u;
		}
		int num = (int)(cT - 1);
		uint num2 = 32768u >> num;
		a -= num2;
		if (c >> 16 < a)
		{
			return true;
		}
		c -= a << 16;
		a = num2;
		do
		{
			if (cT == 0)
			{
				byteIn();
			}
			a <<= 1;
			c <<= 1;
			cT--;
		}
		while (a < 32768);
		return false;
	}

	private void byteIn()
	{
		if (!markerFound)
		{
			if (b == 255)
			{
				b = (uint)in_Renamed.read() & 0xFFu;
				if (b > 143)
				{
					markerFound = true;
					cT = 8u;
				}
				else
				{
					c += 65024 - (b << 9);
					cT = 7u;
				}
			}
			else
			{
				b = (uint)in_Renamed.read() & 0xFFu;
				c += 65280 - (b << 8);
				cT = 8u;
			}
		}
		else
		{
			cT = 8u;
		}
	}

	public void resetCtxt(int c)
	{
		I[c] = initStates[c];
		mPS[c] = 0;
	}

	public void resetCtxts()
	{
		Array.Copy(initStates, 0, I, 0, I.Length);
		ArrayUtil.intArraySet(mPS, 0);
	}

	public void nextSegment(byte[] buf, int off, int len)
	{
		in_Renamed.setByteArray(buf, off, len);
		init();
	}

	private void init()
	{
		markerFound = false;
		b = (uint)in_Renamed.read() & 0xFFu;
		c = (b ^ 0xFF) << 16;
		byteIn();
		c <<= 7;
		cT -= 7u;
		a = 32768u;
	}
}
