using System;
using System.Text;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Barcode;

public class PdfEan13Barcode : PdfUnidimensionalBarcode
{
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

	public PdfEan13Barcode()
	{
	}

	public PdfEan13Barcode(string text)
		: this()
	{
		base.Text = text;
	}

	public new virtual void Draw(PdfGraphics graphics)
	{
		Draw(graphics, base.Location);
	}

	public new virtual void Draw(PdfGraphics graphics, PointF location)
	{
		if (string.IsNullOrEmpty(base.Text) || base.Text.Length > 13)
		{
			throw new BarcodeException("Barcode text should be neither empty nor exceed more than 13 digits");
		}
		float num = 0f;
		if (base.NarrowBarWidth != 0f)
		{
			num = base.NarrowBarWidth;
		}
		float num2;
		float num3;
		if (!size.IsEmpty)
		{
			num2 = base.Size.Width;
			num3 = base.Size.Height;
		}
		else
		{
			num2 = Width;
			if (num != 0f)
			{
				num2 = num * 113f;
			}
			num3 = Height;
		}
		if (barHeightEnabled)
		{
			num2 = num2;
			num3 = base.BarHeight;
		}
		QuietZoneNew = GetQuiteZone();
		float num4 = num2 / 113f;
		PdfBrush brush = new PdfSolidBrush(Color.FromArgb(base.BackColor.A, base.BackColor.R, base.BackColor.G, base.BackColor.B));
		PdfBrush brush2 = PdfBrushes.Black;
		if (base.BarColor.A != 0)
		{
			Color color = Color.FromArgb(base.BarColor.ToArgb());
			if (color != Color.Black)
			{
				brush2 = new PdfSolidBrush(color);
			}
		}
		if (base.Font != null && !isFontModified)
		{
			base.Font.Size = num2 / (float)base.Text.Length;
		}
		PdfFont pdfFont = ((base.Font != null) ? base.Font : new PdfStandardFont(PdfFontFamily.Helvetica, fontSize * Scale));
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		float num5 = 0f;
		float num6 = 0f;
		CalculateChecksumDigit();
		if (stringBuilder2.Length == 12)
		{
			stringBuilder2.AppendFormat("{0}{1}", base.Text, ChecksumDigit);
		}
		else
		{
			stringBuilder2.AppendFormat("{0}{1}", base.Text.Substring(0, 12), ChecksumDigit);
		}
		string text = stringBuilder2.ToString();
		string text2 = "";
		text2 = ConvertLeftPattern(text.Substring(0, 7));
		stringBuilder.AppendFormat("{0}{1}{2}{3}{4}{1}{0}", QuiteZone, LeadTail, text2, Separator, ConvertToDigitPatterns(text.Substring(7), Right));
		string text3 = stringBuilder.ToString();
		float num7 = pdfFont.MeasureString(text3).Height;
		float num8 = location.X;
		float y = location.Y;
		PdfStringFormat pdfStringFormat = new PdfStringFormat();
		pdfStringFormat.Alignment = PdfTextAlignment.Center;
		pdfStringFormat.LineAlignment = PdfVerticalAlignment.Middle;
		switch (textDisplayLocation)
		{
		case TextLocation.Bottom:
			num6 = y + (num3 - num7);
			pdfStringFormat.Alignment = PdfTextAlignment.Justify;
			break;
		case TextLocation.Top:
			num6 = y - num7;
			pdfStringFormat.Alignment = PdfTextAlignment.Center;
			pdfStringFormat.LineAlignment = PdfVerticalAlignment.Top;
			break;
		case TextLocation.None:
			num6 = y - num3 - num7;
			pdfStringFormat.Alignment = PdfTextAlignment.Center;
			break;
		default:
			num6 = y + (num3 - num7);
			break;
		}
		graphics.DrawRectangle(brush, num5 + num8, y, num2, num3);
		num6 += (float)((!base.QuietZone.IsAll && (int)base.QuietZone.Top > 0) ? ((int)base.QuietZone.Top) : 0);
		num5 += (float)((!base.QuietZone.IsAll && (int)base.QuietZone.Left > 0) ? ((int)base.QuietZone.Left) : 0);
		for (int i = 0; i < stringBuilder.Length; i++)
		{
			if (text3.Substring(i, 1) == "1")
			{
				if (num8 == location.X)
				{
					num8 = num5;
				}
				if ((i > 12 && i < 55) || (i > 57 && i < 101))
				{
					graphics.DrawRectangle(brush2, location.X + num5 + (float)QuietZoneNew, y + (float)QuietZoneNew, num4, num3 - num7);
				}
				else
				{
					graphics.DrawRectangle(brush2, location.X + num5 + (float)QuietZoneNew, y + (float)QuietZoneNew, num4, num3);
				}
			}
			num5 += num4;
			if (!base.QuietZone.IsAll && (int)base.QuietZone.Left > 0)
			{
				_ = base.QuietZone.Left;
			}
		}
		if (textDisplayLocation != 0)
		{
			if (textDisplayLocation == TextLocation.Top)
			{
				num5 = num8 - pdfFont.MeasureString(CountryCode.Substring(0, 1)).Width;
				graphics.DrawString(text.Substring(0, 1), pdfFont, brush2, new PointF(location.X + num5 + (float)QuietZoneNew, num6 + (float)QuietZoneNew), pdfStringFormat);
				num5 += pdfFont.MeasureString(text.Substring(0, 1)).Width + 53f * num4 - pdfFont.MeasureString(text.Substring(1, 6)).Width;
				graphics.DrawString(text.Substring(1, 6), pdfFont, brush2, new PointF(location.X + num5 + (float)QuietZoneNew, num6 + (float)QuietZoneNew), pdfStringFormat);
				num5 += pdfFont.MeasureString(text.Substring(1, 6)).Width + 11f * num4;
				graphics.DrawString(text.Substring(7), pdfFont, brush2, new PointF(location.X + num5 + (float)QuietZoneNew, num6 + (float)QuietZoneNew), pdfStringFormat);
			}
			else
			{
				num5 = num8 - pdfFont.MeasureString(CountryCode.Substring(0, 1)).Width;
				graphics.DrawString(text.Substring(0, 1), pdfFont, brush2, new PointF(location.X + num5 + (float)QuietZoneNew, num6 + (float)QuietZoneNew));
				num5 += pdfFont.MeasureString(text.Substring(0, 1)).Width + 43f * num4 - pdfFont.MeasureString(text.Substring(1, 6)).Width;
				graphics.DrawString(text.Substring(1, 6), pdfFont, brush2, new PointF(location.X + num5 + (float)QuietZoneNew, num6 + (float)QuietZoneNew));
				num5 += pdfFont.MeasureString(text.Substring(1, 6)).Width + 11f * num4;
				graphics.DrawString(text.Substring(7), pdfFont, brush2, new PointF(location.X + num5 + (float)QuietZoneNew, num6 + (float)QuietZoneNew));
			}
		}
	}

	public new void Draw(PdfGraphics graphics, PointF location, SizeF size)
	{
		Draw(graphics, location.X, location.Y, size.Width, size.Height);
	}

	public new void Draw(PdfGraphics graphics, RectangleF rectangle)
	{
		Draw(graphics, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
	}

	public new void Draw(PdfGraphics graphics, float x, float y, float width, float height)
	{
		base.Size = new SizeF(width, height);
		base.Location = new PointF(x, y);
		Draw(graphics);
	}

	public new void Draw(PdfPageBase page, float x, float y, float width, float height)
	{
		base.Size = new SizeF(width, height);
		base.Location = new PointF(x, y);
		Draw(page);
	}

	public new void Draw(PdfPageBase page, PointF location, SizeF size)
	{
		Draw(page, location.X, location.Y, size.Width, size.Height);
	}

	public new void Draw(PdfPageBase page, RectangleF rectangle)
	{
		Draw(page, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
	}

	public new virtual void Draw(PdfPageBase page)
	{
		Draw(page, base.Location);
	}

	public new virtual void Draw(PdfPageBase page, PointF location)
	{
		Draw(page.Graphics, location);
	}

	internal int GetQuiteZone()
	{
		int result = 0;
		if (base.QuietZone.IsAll && base.QuietZone.All > 0f)
		{
			result = (int)base.QuietZone.All;
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
		string text = base.Text;
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
				base.Text = text;
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
