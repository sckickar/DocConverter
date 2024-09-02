using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DocGen.Drawing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Graphics;

public struct PdfColor
{
	private static Dictionary<int, object> s_rgbStrings = new Dictionary<int, object>();

	private static Dictionary<float, object> s_grayStringsSroke = new Dictionary<float, object>();

	private static Dictionary<float, object> s_grayStringsFill = new Dictionary<float, object>();

	private static PdfColor s_emptyColor = default(PdfColor);

	private const float MaxColourChannelValue = 255f;

	private byte m_red;

	private float m_cyan;

	private byte m_green;

	private float m_magenta;

	private byte m_blue;

	private float m_yellow;

	private float m_black;

	private float m_gray;

	private byte m_alpha;

	private bool m_isFilled;

	public static PdfColor Empty => s_emptyColor;

	public bool IsEmpty => !m_isFilled;

	public byte B
	{
		get
		{
			return m_blue;
		}
		set
		{
			m_blue = value;
			AssignCMYK(m_red, m_green, m_blue);
			m_isFilled = true;
		}
	}

	public float Blue => (float)(int)B / 255f;

	public float C
	{
		get
		{
			return m_cyan;
		}
		set
		{
			if (value < 0f)
			{
				m_cyan = 0f;
			}
			else if (value > 1f)
			{
				m_cyan = 1f;
			}
			else
			{
				m_cyan = value;
			}
			AssignRGB(m_cyan, m_magenta, m_yellow, m_black);
			m_isFilled = true;
		}
	}

	public byte G
	{
		get
		{
			return m_green;
		}
		set
		{
			m_green = value;
			AssignCMYK(m_red, m_green, m_blue);
			m_isFilled = true;
		}
	}

	public float Green => (float)(int)G / 255f;

	public float Gray
	{
		get
		{
			return (float)(m_red + m_green + m_blue) / 765f;
		}
		set
		{
			if (value < 0f)
			{
				m_gray = 0f;
			}
			else if (value > 1f)
			{
				m_gray = 1f;
			}
			else
			{
				m_gray = value;
			}
			R = (byte)(m_gray * 255f);
			G = (byte)(m_gray * 255f);
			B = (byte)(m_gray * 255f);
			AssignCMYK(m_red, m_green, m_blue);
			m_isFilled = true;
		}
	}

	public float K
	{
		get
		{
			return m_black;
		}
		set
		{
			if (value < 0f)
			{
				m_black = 0f;
			}
			else if (value > 1f)
			{
				m_black = 1f;
			}
			else
			{
				m_black = value;
			}
			AssignRGB(m_cyan, m_magenta, m_yellow, m_black);
			m_isFilled = true;
		}
	}

	public float M
	{
		get
		{
			return m_magenta;
		}
		set
		{
			if (value < 0f)
			{
				m_magenta = 0f;
			}
			else if (value > 1f)
			{
				m_magenta = 1f;
			}
			else
			{
				m_magenta = value;
			}
			AssignRGB(m_cyan, m_magenta, m_yellow, m_black);
			m_isFilled = true;
		}
	}

	public byte R
	{
		get
		{
			return m_red;
		}
		set
		{
			m_red = value;
			AssignCMYK(m_red, m_green, m_blue);
			m_isFilled = true;
		}
	}

	public float Red => (float)(int)R / 255f;

	public float Y
	{
		get
		{
			return m_yellow;
		}
		set
		{
			if (value < 0f)
			{
				m_yellow = 0f;
				return;
			}
			if (value > 1f)
			{
				m_yellow = 1f;
			}
			else
			{
				m_yellow = value;
			}
			AssignRGB(m_cyan, m_magenta, m_yellow, m_black);
			m_isFilled = true;
		}
	}

	internal byte A
	{
		get
		{
			return m_alpha;
		}
		set
		{
			if (value < 0)
			{
				m_alpha = 0;
			}
			else if (m_alpha != value)
			{
				m_alpha = value;
			}
			m_isFilled = true;
		}
	}

	public PdfColor(PdfColor color)
	{
		m_red = color.R;
		m_cyan = color.C;
		m_green = color.G;
		m_magenta = color.M;
		m_blue = color.B;
		m_yellow = color.Y;
		m_black = color.K;
		m_gray = color.Gray;
		m_alpha = color.m_alpha;
		m_isFilled = m_alpha != 0;
	}

	public PdfColor(Color color)
		: this(color.A, color.R, color.G, color.B)
	{
		if (color.Equals(Color.Empty))
		{
			m_isFilled = false;
		}
		else if (CompareColours(color, Color.Empty))
		{
			m_isFilled = false;
		}
	}

	public PdfColor(float gray)
	{
		if (gray < 0f)
		{
			gray = 0f;
		}
		if (gray > 1f)
		{
			gray = 1f;
		}
		m_red = (byte)(gray * 255f);
		m_green = (byte)(gray * 255f);
		m_blue = (byte)(gray * 255f);
		m_cyan = gray;
		m_magenta = gray;
		m_yellow = gray;
		m_black = gray;
		m_gray = gray;
		m_alpha = byte.MaxValue;
		m_isFilled = true;
	}

	public PdfColor(byte red, byte green, byte blue)
		: this(byte.MaxValue, red, green, blue)
	{
	}

	internal PdfColor(float red, float green, float blue)
		: this((byte)(red * 255f), (byte)(green * 255f), (byte)(blue * 255f))
	{
	}

	internal PdfColor(byte a, byte red, byte green, byte blue)
	{
		m_black = 0f;
		m_cyan = 0f;
		m_magenta = 0f;
		m_yellow = 0f;
		m_gray = 0f;
		m_red = red;
		m_green = green;
		m_blue = blue;
		m_alpha = a;
		m_isFilled = m_alpha != 0;
		AssignCMYK(red, green, blue);
	}

	public PdfColor(float cyan, float magenta, float yellow, float black)
	{
		m_red = 0;
		m_cyan = cyan;
		m_green = 0;
		m_magenta = magenta;
		m_blue = 0;
		m_yellow = yellow;
		m_black = black;
		m_gray = 0f;
		m_alpha = byte.MaxValue;
		m_isFilled = true;
		AssignRGB(cyan, magenta, yellow, black);
	}

	public int ToArgb()
	{
		return FromRGBColor(R, G, B).ToArgb();
	}

	private static Color FromRGBColor(int r, int g, int b)
	{
		return Color.FromArgb(r, g, b);
	}

	public static implicit operator PdfColor(Color color)
	{
		return new PdfColor(color);
	}

	public static implicit operator Color(PdfColor color)
	{
		return Color.FromArgb(color.A, color.R, color.G, color.B);
	}

	public static bool operator ==(PdfColor colour1, PdfColor colour2)
	{
		return colour1.Equals(colour2);
	}

	public static bool operator !=(PdfColor colour1, PdfColor colour2)
	{
		return !(colour1 == colour2);
	}

	public override bool Equals(object obj)
	{
		if (obj is PdfColor)
		{
			return Equals((PdfColor)obj);
		}
		return base.Equals(obj);
	}

	public bool Equals(PdfColor colour)
	{
		bool flag = false;
		if (IsEmpty && colour.IsEmpty)
		{
			flag = true;
		}
		if (!IsEmpty || !colour.IsEmpty)
		{
			flag |= m_black != colour.m_black;
			flag |= m_cyan != colour.m_cyan;
			flag |= m_magenta != colour.m_magenta;
			flag |= m_yellow != colour.m_yellow;
			flag |= m_gray != colour.m_gray;
			flag |= m_red != colour.m_red;
			flag |= m_green != colour.m_green;
			flag |= m_blue != colour.m_blue;
			flag |= m_alpha != colour.m_alpha;
		}
		return !flag;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	private string RGBToString(bool ifStroking)
	{
		byte r = R;
		byte g = G;
		byte b = B;
		int num = (r << 16) + (g << 8) + b;
		if (ifStroking)
		{
			num += 16777216;
		}
		string text;
		lock (s_rgbStrings)
		{
			object obj = null;
			if (s_rgbStrings.Count > 0 && s_rgbStrings.ContainsKey(num))
			{
				obj = s_rgbStrings[num];
			}
			if (obj == null)
			{
				float num2 = (float)(int)r / 255f;
				float num3 = (float)(int)g / 255f;
				float num4 = (float)(int)b / 255f;
				text = string.Format(CultureInfo.InvariantCulture, "{0:#0.######} {1:#0.######} {2:#0.######} {3}{4}", num2, num3, num4, ifStroking ? "RG" : "rg", "\r\n");
				s_rgbStrings[num] = text;
			}
			else
			{
				text = obj.ToString();
			}
		}
		return text;
	}

	private string CalRGBToString(bool ifStroking)
	{
		if (R > 1 || R < 0 || G > 1 || G < 0 || G > 1 || G < 0)
		{
			return CalLabToString(ifStroking);
		}
		byte b = Convert.ToByte(R * 255);
		byte b2 = Convert.ToByte(G * 255);
		byte b3 = Convert.ToByte(B * 255);
		int num = (b << 16) + (b2 << 8) + b3;
		if (ifStroking)
		{
			num += 16777216;
		}
		string text = string.Empty;
		lock (s_rgbStrings)
		{
			if (s_rgbStrings.Count > 0 && !s_rgbStrings.ContainsKey(num))
			{
				float num2 = (float)(int)b / 255f;
				float num3 = (float)(int)b2 / 255f;
				float num4 = (float)(int)b3 / 255f;
				text = string.Format(CultureInfo.InvariantCulture, "{0:#0.######} {1:#0.######} {2:#0.######} {3}{4}", num2, num3, num4, ifStroking ? "SC" : "sc", "\r\n");
				s_rgbStrings[num] = text;
			}
			else if (s_rgbStrings.Count > 0 && s_rgbStrings.ContainsKey(num))
			{
				text = s_rgbStrings[num].ToString();
			}
		}
		return text;
	}

	private string CalLabToString(bool ifStroking)
	{
		byte r = R;
		byte g = G;
		byte b = B;
		int num = (r << 16) + (g << 8) + b;
		if (ifStroking)
		{
			num += 16777216;
		}
		string text;
		lock (s_rgbStrings)
		{
			object obj = (s_rgbStrings.ContainsKey(num) ? s_rgbStrings[num] : null);
			if (obj == null)
			{
				float num2 = (int)r;
				float num3 = (int)g;
				float num4 = (int)b;
				text = string.Format(CultureInfo.InvariantCulture, "{0:#0.######} {1:#0.######} {2:#0.######} {3}{4}", num2, num3, num4, ifStroking ? "SC" : "sc", "\r\n");
				s_rgbStrings[num] = text;
			}
			else
			{
				text = obj.ToString();
			}
		}
		return text;
	}

	private string CalGrayscaleToString(bool ifStroking)
	{
		float gray = Gray;
		string text;
		lock (s_grayStringsSroke)
		{
			object obj = ((!ifStroking) ? (s_grayStringsFill.ContainsKey(gray) ? s_grayStringsFill[gray] : null) : (s_grayStringsSroke.ContainsKey(gray) ? s_grayStringsSroke[gray] : null));
			if (obj == null)
			{
				text = string.Format(CultureInfo.InvariantCulture, "{0} {1}{2}", gray, ifStroking ? "SC" : "sc", "\r\n");
				if (ifStroking)
				{
					s_grayStringsSroke[gray] = text;
				}
				else
				{
					s_grayStringsFill[gray] = text;
				}
			}
			else
			{
				text = obj.ToString();
			}
		}
		return text;
	}

	private string IccRGBToString(bool ifStroking)
	{
		if (R > 1 || R < 0 || G > 1 || G < 0 || G > 1 || G < 0)
		{
			return CalLabToString(ifStroking);
		}
		byte b = Convert.ToByte(R * 255);
		byte b2 = Convert.ToByte(G * 255);
		byte b3 = Convert.ToByte(B * 255);
		int num = (b << 16) + (b2 << 8) + b3;
		if (ifStroking)
		{
			num += 16777216;
		}
		string text;
		lock (s_rgbStrings)
		{
			object obj = null;
			if (s_rgbStrings.Count > 0 && s_rgbStrings.ContainsKey(num))
			{
				obj = s_rgbStrings[num];
			}
			if (obj == null)
			{
				float num2 = (float)(int)b / 255f;
				float num3 = (float)(int)b2 / 255f;
				float num4 = (float)(int)b3 / 255f;
				text = string.Format(CultureInfo.InvariantCulture, "{0:#0.######} {1:#0.######} {2:#0.######} {3}{4}", num2, num3, num4, ifStroking ? "SCN" : "scn", "\r\n");
				s_rgbStrings[num] = text;
			}
			else
			{
				text = obj.ToString();
			}
		}
		return text;
	}

	private string CalCMYKToString(bool ifStroking)
	{
		return string.Format(CultureInfo.InvariantCulture, "{0:#0.######} {1:#0.######} {2:#0.######} {3:#0.######} {4}{5}", m_cyan, m_magenta, m_yellow, m_black, ifStroking ? "SCN" : "scn", "\r\n");
	}

	private string IccLabToString(bool ifStroking)
	{
		byte r = R;
		byte g = G;
		byte b = B;
		int num = (r << 16) + (g << 8) + b;
		if (ifStroking)
		{
			num += 16777216;
		}
		string text;
		lock (s_rgbStrings)
		{
			object obj = null;
			if (s_rgbStrings.Count > 0 && s_rgbStrings.ContainsKey(num))
			{
				obj = s_rgbStrings[num];
			}
			if (obj == null)
			{
				float num2 = (int)r;
				float num3 = (int)g;
				float num4 = (int)b;
				text = string.Format(CultureInfo.InvariantCulture, "{0:#0.######} {1:#0.######} {2:#0.######} {3}{4}", num2, num3, num4, ifStroking ? "SC" : "sc", "\r\n");
				s_rgbStrings[num] = text;
			}
			else
			{
				text = obj.ToString();
			}
		}
		return text;
	}

	private string IccGrayscaleToString(bool ifStroking)
	{
		float gray = Gray;
		string text;
		lock (s_grayStringsSroke)
		{
			object obj = ((!ifStroking) ? (s_grayStringsSroke.ContainsKey(gray) ? s_grayStringsFill[gray] : null) : (s_grayStringsSroke.ContainsKey(gray) ? s_grayStringsSroke[gray] : null));
			if (obj == null)
			{
				text = string.Format(CultureInfo.InvariantCulture, "{0} {1}{2}", gray, ifStroking ? "SCN" : "scn", "\r\n");
				if (ifStroking)
				{
					s_grayStringsSroke[gray] = text;
				}
				else
				{
					s_grayStringsFill[gray] = text;
				}
			}
			else
			{
				text = obj.ToString();
			}
		}
		return text;
	}

	internal string IndexedToString(bool ifStroking)
	{
		float num = (int)G;
		string text;
		lock (s_grayStringsSroke)
		{
			object obj = ((!ifStroking) ? (s_grayStringsFill.ContainsKey(num) ? s_grayStringsFill[num] : null) : (s_grayStringsSroke.ContainsKey(num) ? s_grayStringsSroke[num] : null));
			if (obj == null)
			{
				text = string.Format(CultureInfo.InvariantCulture, "{0} {1}{2}", num, ifStroking ? "SC" : "sc", "\r\n");
				if (ifStroking)
				{
					s_grayStringsSroke[num] = text;
				}
				else
				{
					s_grayStringsFill[num] = text;
				}
			}
			else
			{
				text = obj.ToString();
			}
		}
		return text;
	}

	private string GrayscaleToString(bool ifStroking)
	{
		float gray = Gray;
		string text;
		lock (s_grayStringsSroke)
		{
			object obj = ((!ifStroking) ? (s_grayStringsFill.ContainsKey(gray) ? s_grayStringsFill[gray] : null) : (s_grayStringsSroke.ContainsKey(gray) ? s_grayStringsSroke[gray] : null));
			if (obj == null)
			{
				text = string.Format(CultureInfo.InvariantCulture, "{0} {1}{2}", gray, ifStroking ? "G" : "g", "\r\n");
				if (ifStroking)
				{
					s_grayStringsSroke[gray] = text;
				}
				else
				{
					s_grayStringsFill[gray] = text;
				}
			}
			else
			{
				text = obj.ToString();
			}
		}
		return text;
	}

	private string CMYKToString(bool ifStroking)
	{
		return string.Format(CultureInfo.InvariantCulture, "{0:#0.######} {1:#0.######} {2:#0.######} {3:#0.######} {4}{5}", m_cyan, m_magenta, m_yellow, m_black, ifStroking ? "K" : "k", "\r\n");
	}

	private void RGBToStringBuilder(StringBuilder sb, bool stroke)
	{
		float number = (float)(int)R / 255f;
		float number2 = (float)(int)G / 255f;
		float number3 = (float)(int)B / 255f;
		sb.Append(PdfNumber.FloatToString(number));
		sb.Append(" ");
		sb.Append(PdfNumber.FloatToString(number2));
		sb.Append(" ");
		sb.Append(PdfNumber.FloatToString(number3));
		sb.Append(" ");
		if (stroke)
		{
			sb.Append("RG");
		}
		else
		{
			sb.Append("rg");
		}
	}

	private void CMYKToStringBuilder(StringBuilder sb, bool stroke)
	{
		sb.Append(PdfNumber.FloatToString(m_cyan));
		sb.Append(" ");
		sb.Append(PdfNumber.FloatToString(m_magenta));
		sb.Append(" ");
		sb.Append(PdfNumber.FloatToString(m_yellow));
		sb.Append(" ");
		sb.Append(PdfNumber.FloatToString(m_black));
		sb.Append(" ");
		if (stroke)
		{
			sb.Append("K");
		}
		else
		{
			sb.Append("k");
		}
	}

	private void GrayscaleToStringBuilder(StringBuilder sb, bool stroke)
	{
		sb.Append(PdfNumber.FloatToString(Gray));
		sb.Append(" ");
		if (stroke)
		{
			sb.Append("G");
		}
		else
		{
			sb.Append("g");
		}
	}

	internal string ToString(PdfColorSpace colorSpace, bool stroke)
	{
		if (IsEmpty)
		{
			return string.Empty;
		}
		if (PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_X1A2001)
		{
			colorSpace = PdfColorSpace.CMYK;
		}
		return colorSpace switch
		{
			PdfColorSpace.RGB => RGBToString(stroke), 
			PdfColorSpace.GrayScale => GrayscaleToString(stroke), 
			PdfColorSpace.CMYK => CMYKToString(stroke), 
			_ => throw new ArgumentException("Unsupported colour space: " + colorSpace), 
		};
	}

	internal string CalToString(PdfColorSpace colorSpace, bool stroke)
	{
		if (IsEmpty)
		{
			return string.Empty;
		}
		return colorSpace switch
		{
			PdfColorSpace.RGB => CalRGBToString(stroke), 
			PdfColorSpace.GrayScale => CalGrayscaleToString(stroke), 
			PdfColorSpace.CMYK => CMYKToString(stroke), 
			_ => throw new ArgumentException("Unsupported colour space: " + colorSpace), 
		};
	}

	internal string IccColorToString(PdfColorSpace colorSpace, bool stroke)
	{
		if (IsEmpty)
		{
			return string.Empty;
		}
		return colorSpace switch
		{
			PdfColorSpace.RGB => IccRGBToString(stroke), 
			PdfColorSpace.GrayScale => IccGrayscaleToString(stroke), 
			PdfColorSpace.CMYK => CalCMYKToString(stroke), 
			_ => throw new ArgumentException("Unsupported colour space: " + colorSpace), 
		};
	}

	internal void WriteToStringBuilder(StringBuilder sb, PdfColorSpace colorSpace, bool stroke)
	{
		if (sb == null)
		{
			throw new ArgumentNullException("sb");
		}
		if (IsEmpty)
		{
			sb.Append(string.Empty);
			return;
		}
		switch (colorSpace)
		{
		case PdfColorSpace.RGB:
			RGBToStringBuilder(sb, stroke);
			break;
		case PdfColorSpace.GrayScale:
			GrayscaleToStringBuilder(sb, stroke);
			break;
		case PdfColorSpace.CMYK:
			CMYKToStringBuilder(sb, stroke);
			break;
		}
	}

	private void AssignCMYK(byte r, byte g, byte b)
	{
		float num = (float)(int)r / 255f;
		float num2 = (float)(int)g / 255f;
		float num3 = (float)(int)b / 255f;
		float num4 = PdfNumber.Min(1f - num, 1f - num2, 1f - num3);
		float cyan = ((num4 == 1f) ? 0f : ((1f - num - num4) / (1f - num4)));
		float magenta = ((num4 == 1f) ? 0f : ((1f - num2 - num4) / (1f - num4)));
		float yellow = ((num4 == 1f) ? 0f : ((1f - num3 - num4) / (1f - num4)));
		m_black = num4;
		m_cyan = cyan;
		m_magenta = magenta;
		m_yellow = yellow;
	}

	private void AssignRGB(float cyan, float magenta, float yellow, float black)
	{
		float num = black * 255f;
		float val = cyan * (255f - num) + num;
		float val2 = magenta * (255f - num) + num;
		float val3 = yellow * (255f - num) + num;
		m_red = (byte)(255f - Math.Min(255f, val));
		m_green = (byte)(255f - Math.Min(255f, val2));
		m_blue = (byte)(255f - Math.Min(255f, val3));
	}

	private static bool CompareColours(Color color1, Color color2)
	{
		return true & (color1.A == color2.A) & (color1.R == color2.R) & (color1.G == color2.G) & (color1.B == color2.B);
	}

	internal PdfArray ToArray()
	{
		return ToArray(PdfColorSpace.RGB);
	}

	internal PdfArray ToArray(PdfColorSpace colorSpace)
	{
		PdfArray pdfArray = new PdfArray();
		switch (colorSpace)
		{
		case PdfColorSpace.CMYK:
			pdfArray.Add(new PdfNumber(C));
			pdfArray.Add(new PdfNumber(M));
			pdfArray.Add(new PdfNumber(Y));
			pdfArray.Add(new PdfNumber(K));
			break;
		case PdfColorSpace.GrayScale:
			pdfArray.Add(new PdfNumber(Gray));
			break;
		case PdfColorSpace.RGB:
			pdfArray.Add(new PdfNumber(Red));
			pdfArray.Add(new PdfNumber(Green));
			pdfArray.Add(new PdfNumber(Blue));
			break;
		default:
			throw new NotSupportedException("Unsupported colour space.");
		}
		return pdfArray;
	}

	internal static void Clear()
	{
		lock (s_rgbStrings)
		{
			s_rgbStrings.Clear();
		}
		lock (s_grayStringsSroke)
		{
			s_grayStringsSroke.Clear();
			s_grayStringsFill.Clear();
		}
	}
}
