using System;

namespace BitMiracle.LibTiff.Classic.Internal;

internal class TiffCIELabToRGB
{
	public const int CIELABTORGB_TABLE_RANGE = 1500;

	private int range;

	private float rstep;

	private float gstep;

	private float bstep;

	private float X0;

	private float Y0;

	private float Z0;

	private TiffDisplay display;

	private float[] Yr2r = new float[1501];

	private float[] Yg2g = new float[1501];

	private float[] Yb2b = new float[1501];

	public void Init(TiffDisplay refDisplay, float[] refWhite)
	{
		range = 1500;
		display = refDisplay;
		double y = 1.0 / (double)display.d_gammaR;
		rstep = (display.d_YCR - display.d_Y0R) / (float)range;
		for (int i = 0; i <= range; i++)
		{
			Yr2r[i] = (float)display.d_Vrwr * (float)Math.Pow((double)i / (double)range, y);
		}
		y = 1.0 / (double)display.d_gammaG;
		gstep = (display.d_YCR - display.d_Y0R) / (float)range;
		for (int j = 0; j <= range; j++)
		{
			Yg2g[j] = (float)display.d_Vrwg * (float)Math.Pow((double)j / (double)range, y);
		}
		y = 1.0 / (double)display.d_gammaB;
		bstep = (display.d_YCR - display.d_Y0R) / (float)range;
		for (int k = 0; k <= range; k++)
		{
			Yb2b[k] = (float)display.d_Vrwb * (float)Math.Pow((double)k / (double)range, y);
		}
		X0 = refWhite[0];
		Y0 = refWhite[1];
		Z0 = refWhite[2];
	}

	public void CIELabToXYZ(int l, int a, int b, out float X, out float Y, out float Z)
	{
		float num = (float)l * 100f / 255f;
		float num2;
		if (num < 8.856f)
		{
			Y = num * Y0 / 903.292f;
			num2 = 7.787f * (Y / Y0) + 0.13793103f;
		}
		else
		{
			num2 = (num + 16f) / 116f;
			Y = Y0 * num2 * num2 * num2;
		}
		float num3 = (float)a / 500f + num2;
		if (num3 < 0.2069f)
		{
			X = X0 * (num3 - 0.13793f) / 7.787f;
		}
		else
		{
			X = X0 * num3 * num3 * num3;
		}
		num3 = num2 - (float)b / 200f;
		if (num3 < 0.2069f)
		{
			Z = Z0 * (num3 - 0.13793f) / 7.787f;
		}
		else
		{
			Z = Z0 * num3 * num3 * num3;
		}
	}

	public void XYZToRGB(float X, float Y, float Z, out int r, out int g, out int b)
	{
		float val = display.d_mat[0][0] * X + display.d_mat[0][1] * Y + display.d_mat[0][2] * Z;
		float val2 = display.d_mat[1][0] * X + display.d_mat[1][1] * Y + display.d_mat[1][2] * Z;
		float val3 = display.d_mat[2][0] * X + display.d_mat[2][1] * Y + display.d_mat[2][2] * Z;
		val = Math.Max(val, display.d_Y0R);
		val2 = Math.Max(val2, display.d_Y0G);
		val3 = Math.Max(val3, display.d_Y0B);
		val = Math.Min(val, display.d_YCR);
		val2 = Math.Min(val2, display.d_YCG);
		val3 = Math.Min(val3, display.d_YCB);
		int val4 = (int)((val - display.d_Y0R) / rstep);
		val4 = Math.Min(range, val4);
		r = rInt(Yr2r[val4]);
		val4 = (int)((val2 - display.d_Y0G) / gstep);
		val4 = Math.Min(range, val4);
		g = rInt(Yg2g[val4]);
		val4 = (int)((val3 - display.d_Y0B) / bstep);
		val4 = Math.Min(range, val4);
		b = rInt(Yb2b[val4]);
		r = Math.Min(r, display.d_Vrwr);
		g = Math.Min(g, display.d_Vrwg);
		b = Math.Min(b, display.d_Vrwb);
	}

	private static int rInt(float R)
	{
		return (int)((R > 0f) ? ((double)R + 0.5) : ((double)R - 0.5));
	}
}
