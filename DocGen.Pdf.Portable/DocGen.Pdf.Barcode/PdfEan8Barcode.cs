using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Barcode;

public class PdfEan8Barcode : PdfUnidimensionalBarcode
{
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

	public PdfEan8Barcode()
	{
	}

	public PdfEan8Barcode(string text)
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
		int i = 0;
		float num = 0f;
		num = ((base.NarrowBarWidth != 0f) ? base.NarrowBarWidth : 2f);
		if (!CheckNumericOnly(base.Text))
		{
			throw new Exception("Numeric only");
		}
		if (base.Text.Length == 7)
		{
			data = base.Text + CheckDigit(base.Text);
		}
		else
		{
			data = base.Text.Substring(0, 7) + CheckDigit(base.Text);
		}
		encodedData = GetEncoding();
		float num2;
		float num3;
		if (!size.IsEmpty)
		{
			num = base.Size.Width / (float)encodedData.Length;
			num2 = base.Size.Width;
			num3 = base.Size.Height;
		}
		else
		{
			num2 = num * (float)encodedData.Length;
			num3 = num2 / 2f;
		}
		if (barHeightEnabled)
		{
			num2 = num * (float)encodedData.Length;
			num3 = base.BarHeight;
		}
		QuietZoneNew = GetQuiteZone();
		float num4 = num2 / (float)encodedData.Length;
		float num5 = (int)((double)num4 * 0.5);
		if (num4 <= 0f)
		{
			throw new Exception("Image size specified not large enough to draw image");
		}
		float x = location.X;
		float y = location.Y;
		PdfFont pdfFont = ((base.Font != null) ? base.Font : new PdfStandardFont(PdfFontFamily.Helvetica, fontSize));
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
		PdfPen pen = new PdfPen(brush2, num4);
		float num6 = 0f;
		PdfStringFormat pdfStringFormat = new PdfStringFormat();
		pdfStringFormat.Alignment = PdfTextAlignment.Center;
		pdfStringFormat.LineAlignment = PdfVerticalAlignment.Middle;
		switch (textDisplayLocation)
		{
		case TextLocation.Bottom:
			num6 = (int)num3 - (int)pdfFont.Height;
			pdfStringFormat.Alignment = PdfTextAlignment.Center;
			break;
		case TextLocation.Top:
			num6 = 0f;
			pdfStringFormat.Alignment = PdfTextAlignment.Center;
			break;
		case TextLocation.None:
			num6 = (int)num3 + (int)pdfFont.Height;
			break;
		}
		graphics.DrawRectangle(brush, x, y, num2, num3);
		float num7 = 0f;
		float num8 = 0f;
		num8 += (float)((!base.QuietZone.IsAll && (int)base.QuietZone.Top > 0) ? ((int)base.QuietZone.Top) : 0);
		num7 += (float)((!base.QuietZone.IsAll && (int)base.QuietZone.Left > 0) ? ((int)base.QuietZone.Left) : 0);
		for (; i < encodedData.Length; i++)
		{
			if (encodedData[i] == '1')
			{
				graphics.DrawLine(pen, new PointF((float)i * num4 + num5 + (float)(int)location.X + (float)(int)num7 + (float)QuietZoneNew, (int)location.Y + (int)num8 + QuietZoneNew), new PointF((float)i * num4 + num5 + (float)(int)location.X + (float)(int)num7 + (float)QuietZoneNew, (int)num3 + (int)location.Y + (int)num8 + QuietZoneNew));
				graphics.DrawRectangle(PdfBrushes.White, new RectangleF(0f + x + (float)QuietZoneNew, num6 + y + (float)QuietZoneNew, num2, pdfFont.Height));
				if (textDisplayLocation != 0)
				{
					graphics.DrawString(data, pdfFont, brush2, new RectangleF(0f + x + (float)QuietZoneNew, y + num6 + (float)QuietZoneNew, num2, pdfFont.Height), pdfStringFormat);
				}
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
		char c2 = base.Text[7];
		if (c2 != c)
		{
			throw new Exception("Error calculating check digit");
		}
		return c2.ToString();
	}
}
