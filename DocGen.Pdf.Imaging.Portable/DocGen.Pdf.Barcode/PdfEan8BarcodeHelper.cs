using System;
using System.IO;
using SkiaSharp;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Graphics.Fonts;

namespace DocGen.Pdf.Barcode;

internal class PdfEan8BarcodeHelper
{
	private PdfEan8Barcode m_barcode;

	private string[] codesA = new string[10] { "0001101", "0011001", "0010011", "0111101", "0100011", "0110001", "0101111", "0111011", "0110111", "0001011" };

	private string[] codesC = new string[10] { "1110010", "1100110", "1101100", "1000010", "1011100", "1001110", "1010000", "1000100", "1001000", "1110100" };

	private string data;

	private string encodedData;

	private float fontSize = 8f;

	private int quietZonePixel = 4;

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

	internal PdfEan8BarcodeHelper(PdfEan8Barcode barcode)
	{
		m_barcode = barcode;
	}

	public virtual Stream ToImage()
	{
		int i = 0;
		float num = 2f;
		float num2 = 0f;
		float num3 = 0f;
		bool flag = false;
		if (!CheckNumericOnly(m_barcode.Text))
		{
			throw new Exception("Numeric only");
		}
		if (m_barcode.Text.Length == 7)
		{
			data = m_barcode.Text + CheckDigit(m_barcode.Text);
		}
		else
		{
			data = m_barcode.Text.Substring(0, 7) + CheckDigit(m_barcode.Text);
		}
		encodedData = GetEncoding();
		if (!flag)
		{
			num2 = num * (float)encodedData.Length;
			num3 = m_barcode.BarHeight;
		}
		if (!m_barcode.size.IsEmpty)
		{
			num = m_barcode.Size.Width / (float)encodedData.Length;
			num2 = m_barcode.Size.Width;
			num3 = m_barcode.Size.Height;
		}
		else if (m_barcode.size.IsEmpty && flag)
		{
			num2 = num * (float)encodedData.Length;
			num3 = num2 / 2f;
		}
		QuietZoneNew = GetQuiteZone();
		float num4 = num2 / (float)encodedData.Length;
		float num5 = (int)((double)num4 * 0.5);
		if (num4 <= 0f)
		{
			throw new Exception("Image size specified not large enough to draw image");
		}
		float num6 = 0f;
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
		float num7 = 0f;
		float num8 = 0f;
		if (font != null)
		{
			num7 = font.Height;
			num8 = font.Size;
		}
		else
		{
			num8 = sKTypeface.FontWidth;
			num7 = num8;
		}
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
		SKPaint sKPaint2 = new SKPaint();
		sKPaint2.Color = new SKColor(color2.R, color2.G, color2.B, color2.A);
		sKPaint2.StrokeWidth = num4;
		Bitmap bitmap = new Bitmap((int)num2, (int)num3);
		GraphicsHelper graphicsHelper = new GraphicsHelper(bitmap);
		switch (m_barcode.textDisplayLocation)
		{
		case TextLocation.Bottom:
			num6 = (float)bitmap.Height - num7;
			break;
		case TextLocation.Top:
			num6 = 0f;
			break;
		case TextLocation.None:
			num6 = (float)bitmap.Height + num7;
			break;
		}
		graphicsHelper.FillRectangle(color, 0f, 0f, num2, num3);
		float num9 = 0f;
		float num10 = 0f;
		num10 += (float)((!m_barcode.QuietZone.IsAll && (int)m_barcode.QuietZone.Top > 0) ? ((int)m_barcode.QuietZone.Top) : 0);
		num9 += (float)((!m_barcode.QuietZone.IsAll && (int)m_barcode.QuietZone.Left > 0) ? ((int)m_barcode.QuietZone.Left) : 0);
		sKPaint.Color = new SKColor(color2.R, color2.G, color2.B, color2.A);
		for (; i < encodedData.Length; i++)
		{
			if (encodedData[i] == '1')
			{
				graphicsHelper.m_canvas.DrawLine(new SKPoint((float)i * num4 + num5 + (float)(int)num9 + (float)QuietZoneNew, (int)num10 + QuietZoneNew), new SKPoint((float)i * num4 + num5 + (float)(int)num9 + (float)QuietZoneNew, (int)num3 + (int)num10 + QuietZoneNew), sKPaint2);
				SKPaint sKPaint3 = new SKPaint();
				sKPaint3.Style = SKPaintStyle.Fill;
				sKPaint3.Color = new SKColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
				graphicsHelper.m_canvas.DrawRect(QuietZoneNew, num6 + (float)QuietZoneNew, bitmap.Width, num7, sKPaint3);
				RectangleF rectangleF = new RectangleF(QuietZoneNew, num6 + (float)QuietZoneNew, bitmap.Width, num8);
				float left = rectangleF.Left;
				float y = ((m_barcode.textDisplayLocation == TextLocation.Top) ? (rectangleF.Top + rectangleF.Height) : rectangleF.Bottom);
				SKRect bounds = default(SKRect);
				sKPaint.MeasureText(data, ref bounds);
				left = rectangleF.Width / 2f - bounds.MidX;
				graphicsHelper.m_canvas.DrawText(data, left, y, sKPaint);
			}
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

	protected bool CheckNumericOnly(string data)
	{
		if (data != null && long.TryParse(data, out var _))
		{
			return true;
		}
		return false;
	}

	internal string GetEncoding()
	{
		if (data.Length != 8 && data.Length != 7)
		{
			throw new Exception("Invalid data length(Barcode text length should be 7 or 8 digits only)");
		}
		string text = "101";
		for (int i = 0; i < data.Length / 2; i++)
		{
			text += codesA[int.Parse(data[i].ToString())];
		}
		text += "01010";
		for (int j = data.Length / 2; j < data.Length; j++)
		{
			text += codesC[int.Parse(data[j].ToString())];
		}
		return text + "101";
	}

	private string CheckDigit(string data)
	{
		if (data.Length == 7)
		{
			int num = 0;
			int num2 = 0;
			for (int i = 0; i <= 6; i += 2)
			{
				num2 += int.Parse(data.Substring(i, 1)) * 3;
			}
			for (int j = 1; j <= 5; j += 2)
			{
				num += int.Parse(data.Substring(j, 1));
			}
			int num3 = (num + num2) % 10;
			num3 = 10 - num3;
			if (num3 == 10)
			{
				num3 = 0;
			}
			return num3.ToString();
		}
		int num4 = 0;
		int num5 = 0;
		for (int k = 0; k <= 6; k += 2)
		{
			num5 += int.Parse(data.Substring(k, 1)) * 3;
		}
		for (int l = 1; l <= 5; l += 2)
		{
			num4 += int.Parse(data.Substring(l, 1));
		}
		int num6 = (num4 + num5) % 10;
		num6 = 10 - num6;
		if (num6 == 10)
		{
			num6 = 0;
		}
		string text = num6.ToString();
		char c = text[0];
		char c2 = m_barcode.Text[7];
		if (c2 != c)
		{
			throw new Exception("Error calculating check digit");
		}
		return c2.ToString();
	}
}
