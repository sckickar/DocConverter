using System;
using DocGen.Drawing;

namespace DocGen.Chart;

internal struct ChartCMYKColor
{
	private const int BYTE_MAX_VALUE = 255;

	private byte m_c;

	private byte m_m;

	private byte m_y;

	private byte m_k;

	public byte C => m_c;

	public byte M => m_m;

	public byte Y => m_y;

	public byte K => m_k;

	public Color ToRGBColor()
	{
		return ConvertCMYKToRGB(this);
	}

	public static ChartCMYKColor FromRBG(byte r, byte g, byte b)
	{
		int num = 255 - r;
		int num2 = 255 - g;
		int num3 = 255 - b;
		int num4 = Math.Min(num, Math.Min(num2, num3));
		return FromCMYK((byte)(num - num4), (byte)(num2 - num4), (byte)(num3 - num4), (byte)num4);
	}

	public static ChartCMYKColor FromCMYK(byte c, byte m, byte y, byte k)
	{
		ChartCMYKColor result = default(ChartCMYKColor);
		result.m_c = c;
		result.m_m = m;
		result.m_y = y;
		result.m_k = k;
		return result;
	}

	public static ChartCMYKColor ConvertRGBToCMYK(Color rgbColor)
	{
		int num = 255 - rgbColor.R;
		int num2 = 255 - rgbColor.G;
		int num3 = 255 - rgbColor.B;
		int num4 = Math.Min(num, Math.Min(num2, num3));
		return FromCMYK((byte)(num - num4), (byte)(num2 - num4), (byte)(num3 - num4), (byte)num4);
	}

	public static Color ConvertCMYKToRGB(ChartCMYKColor cmykColor)
	{
		int num = cmykColor.m_c + cmykColor.m_k;
		int num2 = cmykColor.m_m + cmykColor.m_k;
		int num3 = cmykColor.m_y + cmykColor.m_k;
		return Color.FromArgb(255 - num, 255 - num2, 255 - num3);
	}
}
