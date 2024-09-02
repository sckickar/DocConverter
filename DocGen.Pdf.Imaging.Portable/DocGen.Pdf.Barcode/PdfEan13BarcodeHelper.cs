using System;
using System.IO;
using System.Text;
using SkiaSharp;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Graphics.Fonts;

namespace DocGen.Pdf.Barcode;

internal class PdfEan13BarcodeHelper
{
	private PdfEan13Barcode m_barcode;

	private float minimumAllowableScale = 0.8f;

	private float maximumAllowableScale = 2f;

	private float fWidth = 150f;

	private float fHeight = 100f;

	private float fontSize = 8f;

	private float scale = 1.5f;

	private int quietZonePixel = 4;

	private string[] OddLeft = new string[10] { "0001101", "0011001", "0010011", "0111101", "0100011", "0110001", "0101111", "0111011", "0110111", "0001011" };

	private string[] EvenLeft = new string[10] { "0100111", "0110011", "0011011", "0100001", "0011101", "0111001", "0000101", "0010001", "0001001", "0010111" };

	private string[] Right = new string[10] { "1110010", "1100110", "1101100", "1000010", "1011100", "1001110", "1010000", "1000100", "1001000", "1110100" };

	private string QuiteZone = "000000000";

	private string LeadTail = "101";

	private string Separator = "01010";

	private string countryCode = "00";

	private string manufacturerCode;

	private string productCode;

	private string checksumDigit;

	internal int QuietZoneNew
	{
		get
		{
			return quietZonePixel;
		}
		set
		{
			quietZonePixel = value;
		}
	}

	internal float MinimumAllowableScale => minimumAllowableScale;

	internal float MaximumAllowableScale => maximumAllowableScale;

	internal float Width => fWidth;

	internal float Height => fHeight;

	internal float FontSize => fontSize;

	internal float Scale
	{
		get
		{
			return scale;
		}
		set
		{
			if (value < minimumAllowableScale || value > maximumAllowableScale)
			{
				throw new Exception("Scale value out of allowable range.  Value must be between " + minimumAllowableScale + " and " + maximumAllowableScale);
			}
			scale = value;
		}
	}

	internal string CountryCode
	{
		get
		{
			return countryCode;
		}
		set
		{
			while (value.Length < 2)
			{
				value = "0" + value;
			}
			countryCode = value;
		}
	}

	internal string ManufacturerCode
	{
		get
		{
			return manufacturerCode;
		}
		set
		{
			manufacturerCode = value;
		}
	}

	internal string ProductCode
	{
		get
		{
			return productCode;
		}
		set
		{
			productCode = value;
		}
	}

	internal string ChecksumDigit
	{
		get
		{
			return checksumDigit;
		}
		set
		{
			int num = Convert.ToInt32(value);
			if (num < 0 || num > 9)
			{
				throw new Exception("The Check Digit must be between 0 and 9.");
			}
			checksumDigit = value;
		}
	}

	internal PdfEan13BarcodeHelper(PdfEan13Barcode barcode)
	{
		m_barcode = barcode;
	}

	public virtual Stream ToImage()
	{
		bool flag = false;
		if (string.IsNullOrEmpty(m_barcode.Text) || m_barcode.Text.Length > 13)
		{
			throw new PdfException("Barcode text should be neither empty nor exceed more than 13 digits");
		}
		float num = 0f;
		float num2 = 0f;
		if (!flag)
		{
			num = Width;
			num2 = m_barcode.BarHeight;
		}
		if (!m_barcode.size.IsEmpty)
		{
			num = m_barcode.Size.Width;
			num2 = m_barcode.Size.Height;
		}
		else if (m_barcode.size.IsEmpty && flag)
		{
			num = Width;
			num2 = Height;
		}
		QuietZoneNew = GetQuiteZone();
		float num3 = num / 113f;
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		SKPaint sKPaint = new SKPaint();
		PdfFont font = m_barcode.Font;
		SKTypeface sKTypeface;
		if (font != null && font is PdfTrueTypeFont)
		{
			sKTypeface = SKTypeface.FromStream(((font as PdfTrueTypeFont).InternalFont as UnicodeTrueTypeFont).FontStream);
			sKPaint.TextSize = font.Size;
		}
		else if (font != null && font is PdfStandardFont)
		{
			sKTypeface = SKTypeface.FromFamilyName(font.Name.ToString(), SKFontStyle.Normal);
			sKPaint.TextSize = font.Size;
		}
		else
		{
			sKTypeface = SKTypeface.FromFamilyName("Helvetica", SKFontStyle.Normal);
			sKPaint.TextSize = fontSize;
		}
		sKPaint.Typeface = sKTypeface;
		Color color = Color.FromArgb(m_barcode.BackColor.A, m_barcode.BackColor.R, m_barcode.BackColor.G, m_barcode.BackColor.B);
		Color color2 = Color.Black;
		if (m_barcode.BarColor.A != 0)
		{
			Color color3 = Color.FromArgb(m_barcode.BarColor.ToArgb());
			if (color3 != Color.Black)
			{
				color2 = color3;
			}
		}
		CalculateChecksumDigit();
		if (stringBuilder2.Length == 12)
		{
			stringBuilder2.AppendFormat("{0}{1}", m_barcode.Text, ChecksumDigit);
		}
		else
		{
			stringBuilder2.AppendFormat("{0}{1}", m_barcode.Text.Substring(0, 12), ChecksumDigit);
		}
		string text = stringBuilder2.ToString();
		string text2 = "";
		text2 = ConvertLeftPattern(text.Substring(0, 7));
		stringBuilder.AppendFormat("{0}{1}{2}{3}{4}{1}{0}", QuiteZone, LeadTail, text2, Separator, ConvertToDigitPatterns(text.Substring(7), Right));
		string text3 = stringBuilder.ToString();
		float num4 = 0f;
		float num5 = 0f;
		if (font != null)
		{
			num4 = font.Height;
			num5 = font.Size;
		}
		else
		{
			num5 = sKTypeface.FontWidth;
			num4 = num5;
		}
		float num6 = 0f;
		float num7 = 0f;
		float num8 = m_barcode.Location.X;
		float num9 = m_barcode.Location.Y;
		switch (m_barcode.textDisplayLocation)
		{
		case TextLocation.Bottom:
			num7 = num9 + (num2 - num4);
			break;
		case TextLocation.Top:
			num9 = num4;
			num7 = num9 - num4;
			num2 += num4;
			break;
		case TextLocation.None:
			num7 = num9 - num2 - num4;
			break;
		default:
			num7 = num9 + (num2 - num4);
			break;
		}
		Bitmap bitmap = new Bitmap((int)num, (int)num2 + (int)num4);
		GraphicsHelper graphicsHelper = new GraphicsHelper(bitmap);
		graphicsHelper.FillRectangle(color, 0f, 0f, num, num2 + num4);
		num7 += (float)((!m_barcode.QuietZone.IsAll && (int)m_barcode.QuietZone.Top > 0) ? ((int)m_barcode.QuietZone.Top) : 0);
		num6 += (float)((!m_barcode.QuietZone.IsAll && (int)m_barcode.QuietZone.Left > 0) ? ((int)m_barcode.QuietZone.Left) : 0);
		for (int i = 0; i < stringBuilder.Length; i++)
		{
			if (text3.Substring(i, 1) == "1")
			{
				if (num8 == m_barcode.Location.X)
				{
					num8 = num6;
				}
				if ((i > 12 && i < 55) || (i > 57 && i < 101))
				{
					graphicsHelper.FillRectangle(color2, m_barcode.Location.X + num6 + (float)QuietZoneNew, num9 + (float)QuietZoneNew, num3, num2 - num4);
				}
				else
				{
					graphicsHelper.FillRectangle(color2, m_barcode.Location.X + num6 + (float)QuietZoneNew, num9 + (float)QuietZoneNew, num3, num2);
				}
			}
			num6 += num3;
			if (!m_barcode.QuietZone.IsAll && (int)m_barcode.QuietZone.Left > 0)
			{
				_ = m_barcode.QuietZone.Left;
			}
		}
		sKPaint.Color = new SKColor(color2.R, color2.G, color2.B, color2.A);
		num7 += num5;
		string text4 = CountryCode.Substring(0, 1);
		string text5 = text.Substring(0, 1);
		string text6 = text.Substring(1, 6);
		string text7 = text.Substring(7);
		if (m_barcode.textDisplayLocation == TextLocation.Top)
		{
			num6 = num8 - sKPaint.MeasureText(text4);
			graphicsHelper.m_canvas.DrawText(text5, m_barcode.Location.X + num6 + (float)QuietZoneNew, num7 + (float)QuietZoneNew, sKPaint);
			float num10 = sKPaint.MeasureText(text5);
			float num11 = sKPaint.MeasureText(text6);
			num6 += num10 + 43f * num3 - num11;
			graphicsHelper.m_canvas.DrawText(text6, m_barcode.Location.X + num6 + (float)QuietZoneNew, num7 + (float)QuietZoneNew, sKPaint);
			num6 += num11 + 11f * num3;
			graphicsHelper.m_canvas.DrawText(text7, m_barcode.Location.X + num6 + (float)QuietZoneNew, num7 + (float)QuietZoneNew, sKPaint);
		}
		else
		{
			num6 = num8 - sKPaint.MeasureText(text4);
			float num12 = sKPaint.MeasureText(text5);
			float num13 = sKPaint.MeasureText(text6);
			graphicsHelper.m_canvas.DrawText(text5, num6 + (float)QuietZoneNew, num7 + (float)QuietZoneNew, sKPaint);
			num6 += num12 + 43f * num3 - num13;
			graphicsHelper.m_canvas.DrawText(text6, num6 + (float)QuietZoneNew, num7 + (float)QuietZoneNew, sKPaint);
			num6 += num13 + 11f * num3;
			graphicsHelper.m_canvas.DrawText(text7, num6 + (float)QuietZoneNew, num7 + (float)QuietZoneNew, sKPaint);
		}
		MemoryStream memoryStream = new MemoryStream();
		bitmap.Save(memoryStream, ImageFormat.Png);
		memoryStream.Position = 0L;
		return memoryStream;
	}

	public Stream ToImage(SizeF size)
	{
		m_barcode.Size = new SizeF(size.Width, size.Height);
		return ToImage();
	}

	internal int GetQuiteZone()
	{
		int result = 0;
		if (m_barcode.QuietZone.IsAll && m_barcode.QuietZone.All > 0f)
		{
			result = (int)m_barcode.QuietZone.All;
		}
		return result;
	}

	private string ConvertLeftPattern(string sLeft)
	{
		return sLeft.Substring(0, 1) switch
		{
			"0" => CountryCode0(sLeft.Substring(1)), 
			"1" => CountryCode1(sLeft.Substring(1)), 
			"2" => CountryCode2(sLeft.Substring(1)), 
			"3" => CountryCode3(sLeft.Substring(1)), 
			"4" => CountryCode4(sLeft.Substring(1)), 
			"5" => CountryCode5(sLeft.Substring(1)), 
			"6" => CountryCode6(sLeft.Substring(1)), 
			"7" => CountryCode7(sLeft.Substring(1)), 
			"8" => CountryCode8(sLeft.Substring(1)), 
			"9" => CountryCode9(sLeft.Substring(1)), 
			_ => "", 
		};
	}

	private string CountryCode0(string sLeft)
	{
		return ConvertToDigitPatterns(sLeft, OddLeft);
	}

	private string CountryCode1(string sLeft)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(0, 1), OddLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(1, 1), OddLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(2, 1), EvenLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(3, 1), OddLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(4, 1), EvenLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(5, 1), EvenLeft));
		return stringBuilder.ToString();
	}

	private string CountryCode2(string sLeft)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(0, 1), OddLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(1, 1), OddLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(2, 1), EvenLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(3, 1), EvenLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(4, 1), OddLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(5, 1), EvenLeft));
		return stringBuilder.ToString();
	}

	private string CountryCode3(string sLeft)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(0, 1), OddLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(1, 1), OddLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(2, 1), EvenLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(3, 1), EvenLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(4, 1), EvenLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(5, 1), OddLeft));
		return stringBuilder.ToString();
	}

	private string CountryCode4(string sLeft)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(0, 1), OddLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(1, 1), EvenLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(2, 1), OddLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(3, 1), OddLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(4, 1), EvenLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(5, 1), EvenLeft));
		return stringBuilder.ToString();
	}

	private string CountryCode5(string sLeft)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(0, 1), OddLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(1, 1), EvenLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(2, 1), EvenLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(3, 1), OddLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(4, 1), OddLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(5, 1), EvenLeft));
		return stringBuilder.ToString();
	}

	private string CountryCode6(string sLeft)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(0, 1), OddLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(1, 1), EvenLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(2, 1), EvenLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(3, 1), EvenLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(4, 1), OddLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(5, 1), OddLeft));
		return stringBuilder.ToString();
	}

	private string CountryCode7(string sLeft)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(0, 1), OddLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(1, 1), EvenLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(2, 1), OddLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(3, 1), EvenLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(4, 1), OddLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(5, 1), EvenLeft));
		return stringBuilder.ToString();
	}

	private string CountryCode8(string sLeft)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(0, 1), OddLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(1, 1), EvenLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(2, 1), OddLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(3, 1), EvenLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(4, 1), EvenLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(5, 1), OddLeft));
		return stringBuilder.ToString();
	}

	private string CountryCode9(string sLeft)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(0, 1), OddLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(1, 1), EvenLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(2, 1), EvenLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(3, 1), OddLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(4, 1), EvenLeft));
		stringBuilder.Append(ConvertToDigitPatterns(sLeft.Substring(5, 1), OddLeft));
		return stringBuilder.ToString();
	}

	private string ConvertToDigitPatterns(string inputNumber, string[] patterns)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		for (int i = 0; i < inputNumber.Length; i++)
		{
			num = Convert.ToInt32(inputNumber.Substring(i, 1));
			stringBuilder.Append(patterns[num]);
		}
		return stringBuilder.ToString();
	}

	internal void CalculateChecksumDigit()
	{
		string text = m_barcode.Text;
		int num = 0;
		int num2 = 0;
		_ = text.Length;
		if (text.Length == 13)
		{
			for (int num3 = text.Length - 1; num3 >= 1; num3--)
			{
				num2 = Convert.ToInt32(text.Substring(num3 - 1, 1));
				num = ((num3 % 2 != 0) ? (num + num2) : (num + num2 * 3));
			}
			char c = ((10 - num % 10) % 10).ToString()[0];
			char c2 = text[12];
			if (c2 != c)
			{
				char[] array = text.ToCharArray();
				array[12] = c;
				text = new string(array);
				m_barcode.Text = text;
				ChecksumDigit = c.ToString();
			}
			else
			{
				ChecksumDigit = c2.ToString();
			}
		}
		else
		{
			for (int num4 = text.Length; num4 >= 1; num4--)
			{
				num2 = Convert.ToInt32(text.Substring(num4 - 1, 1));
				num = ((num4 % 2 != 0) ? (num + num2) : (num + num2 * 3));
			}
			ChecksumDigit = ((10 - num % 10) % 10).ToString();
		}
	}
}
