using System;

namespace DocGen.Pdf;

internal struct ColorConvertor
{
	private byte m_alpha;

	private byte m_red;

	private byte m_green;

	private byte m_blue;

	public byte Alpha
	{
		get
		{
			return m_alpha;
		}
		set
		{
			m_alpha = value;
		}
	}

	public byte Red
	{
		get
		{
			return m_red;
		}
		set
		{
			m_red = value;
		}
	}

	public byte Green
	{
		get
		{
			return m_green;
		}
		set
		{
			m_green = value;
		}
	}

	public byte Blue
	{
		get
		{
			return m_blue;
		}
		set
		{
			m_blue = value;
		}
	}

	public ColorConvertor(byte a, byte r, byte g, byte b)
	{
		m_alpha = a;
		m_red = r;
		m_green = g;
		m_blue = b;
	}

	public ColorConvertor(byte r, byte g, byte b)
		: this(byte.MaxValue, r, g, b)
	{
	}

	public static ColorConvertor FromGray(double gray)
	{
		return FromGray(ConvertColorToByte(gray));
	}

	public static ColorConvertor FromGray(int gray, int bits)
	{
		return FromGray(ConvertColorToByte(gray, bits));
	}

	public static ColorConvertor FromGray(byte gray)
	{
		return new ColorConvertor(gray, gray, gray);
	}

	public static ColorConvertor FromCmyk(int cyan, int magenta, int yellow, int black, int bits)
	{
		byte cyan2 = ConvertColorToByte(cyan, bits);
		byte magenta2 = ConvertColorToByte(magenta, bits);
		byte yellow2 = ConvertColorToByte(yellow, bits);
		byte black2 = ConvertColorToByte(black, bits);
		return FromCmyk(cyan2, magenta2, yellow2, black2);
	}

	public static ColorConvertor FromCmyk(double cyan, double magenta, double yellow, double black)
	{
		byte cyan2 = ConvertColorToByte(cyan);
		byte magenta2 = ConvertColorToByte(magenta);
		byte yellow2 = ConvertColorToByte(yellow);
		byte black2 = ConvertColorToByte(black);
		return FromCmyk(cyan2, magenta2, yellow2, black2);
	}

	public static ColorConvertor FromCmyk(byte cyan, byte magenta, byte yellow, byte black)
	{
		byte r = (byte)(255 - Math.Min(255, cyan + black));
		byte g = (byte)(255 - Math.Min(255, magenta + black));
		byte b = (byte)(255 - Math.Min(255, yellow + black));
		return new ColorConvertor(r, g, b);
	}

	public static ColorConvertor FromArgb(int alpha, int red, int green, int blue, int bits)
	{
		byte alpha2 = ConvertColorToByte(alpha, bits);
		byte red2 = ConvertColorToByte(red, bits);
		byte green2 = ConvertColorToByte(green, bits);
		byte blue2 = ConvertColorToByte(blue, bits);
		return FromArgb(alpha2, red2, green2, blue2);
	}

	public static ColorConvertor FromArgb(double alpha, double red, double green, double blue)
	{
		byte alpha2 = ConvertColorToByte(alpha);
		byte red2 = ConvertColorToByte(red);
		byte green2 = ConvertColorToByte(green);
		byte blue2 = ConvertColorToByte(blue);
		return FromArgb(alpha2, red2, green2, blue2);
	}

	public static ColorConvertor FromArgb(byte alpha, byte red, byte green, byte blue)
	{
		return new ColorConvertor(alpha, red, green, blue);
	}

	private static byte ConvertColorToByte(int component, int bits)
	{
		double num = (1 << bits) - 1;
		return ConvertColorToByte((double)component / num);
	}

	public static byte ConvertColorToByte(double component)
	{
		if (component > 1.0)
		{
			return byte.MaxValue;
		}
		if (component < 0.0)
		{
			return 0;
		}
		return (byte)(component * 255.0);
	}

	public byte GetGrayComponent()
	{
		return m_red;
	}

	public int PixelConversion()
	{
		return (m_alpha << 24) | (m_red << 16) | (m_green << 8) | m_blue;
	}
}
