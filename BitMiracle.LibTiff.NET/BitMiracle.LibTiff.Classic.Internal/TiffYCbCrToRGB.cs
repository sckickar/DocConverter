using System;

namespace BitMiracle.LibTiff.Classic.Internal;

internal class TiffYCbCrToRGB
{
	private const int clamptabOffset = 256;

	private const int SHIFT = 16;

	private const int ONE_HALF = 32768;

	private byte[] clamptab;

	private int[] Cr_r_tab;

	private int[] Cb_b_tab;

	private int[] Cr_g_tab;

	private int[] Cb_g_tab;

	private int[] Y_tab;

	public TiffYCbCrToRGB()
	{
		clamptab = new byte[1024];
		Cr_r_tab = new int[256];
		Cb_b_tab = new int[256];
		Cr_g_tab = new int[256];
		Cb_g_tab = new int[256];
		Y_tab = new int[256];
	}

	public void Init(float[] luma, float[] refBlackWhite)
	{
		Array.Clear(clamptab, 0, 256);
		for (int i = 0; i < 256; i++)
		{
			clamptab[256 + i] = (byte)i;
		}
		int num = 512 + 512;
		for (int j = 512; j < num; j++)
		{
			clamptab[j] = byte.MaxValue;
		}
		float num2 = luma[0];
		float num3 = luma[1];
		float num4 = luma[2];
		float num5 = 2f - 2f * num2;
		int num6 = fix(num5);
		int num7 = -fix(num2 * num5 / num3);
		float num8 = 2f - 2f * num4;
		int num9 = fix(num8);
		int num10 = -fix(num4 * num8 / num3);
		int num11 = 0;
		int num12 = -128;
		while (num11 < 256)
		{
			int num13 = code2V(num12, refBlackWhite[4] - 128f, refBlackWhite[5] - 128f, 127f);
			int num14 = code2V(num12, refBlackWhite[2] - 128f, refBlackWhite[3] - 128f, 127f);
			Cr_r_tab[num11] = num6 * num13 + 32768 >> 16;
			Cb_b_tab[num11] = num9 * num14 + 32768 >> 16;
			Cr_g_tab[num11] = num7 * num13;
			Cb_g_tab[num11] = num10 * num14 + 32768;
			Y_tab[num11] = code2V(num12 + 128, refBlackWhite[0], refBlackWhite[1], 255f);
			num11++;
			num12++;
		}
	}

	public void YCbCrtoRGB(int Y, int Cb, int Cr, out int r, out int g, out int b)
	{
		Y = hiClamp(Y, 255);
		Cb = clamp(Cb, 0, 255);
		Cr = clamp(Cr, 0, 255);
		r = clamptab[256 + Y_tab[Y] + Cr_r_tab[Cr]];
		g = clamptab[256 + Y_tab[Y] + (Cb_g_tab[Cb] + Cr_g_tab[Cr] >> 16)];
		b = clamptab[256 + Y_tab[Y] + Cb_b_tab[Cb]];
	}

	private static int fix(float x)
	{
		return (int)((double)(x * 65536f) + 0.5);
	}

	private static int code2V(int c, float RB, float RW, float CR)
	{
		return (int)((float)(c - (int)RB) * CR / (((int)(RW - RB) != 0) ? (RW - RB) : 1f));
	}

	private static int clamp(int f, int min, int max)
	{
		if (f >= min)
		{
			if (f <= max)
			{
				return f;
			}
			return max;
		}
		return min;
	}

	private static int hiClamp(int f, int max)
	{
		if (f <= max)
		{
			return f;
		}
		return max;
	}
}
