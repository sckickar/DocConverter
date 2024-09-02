using System;
using System.Collections.Generic;
using System.IO;
using SkiaSharp;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Graphics.Fonts;

namespace DocGen.Pdf.Barcode;

internal class PdfUnidimensionalBarcodeHelper
{
	private const float UpcBarWidth = 1.0734f;

	internal int barcodeSpaceCount;

	private PdfUnidimensionalBarcode m_barcode;

	private bool isContainsSmallSize;

	internal PdfUnidimensionalBarcodeHelper(PdfUnidimensionalBarcode barcode)
	{
		m_barcode = barcode;
	}

	public virtual Stream ToImage()
	{
		PdfBarcodeQuietZones pdfBarcodeQuietZones = new PdfBarcodeQuietZones();
		barcodeSpaceCount = 0;
		float num = 1.0734f;
		isContainsSmallSize = false;
		if (m_barcode.size != SizeF.Empty)
		{
			m_barcode.NarrowBarWidth = 1f;
			if (m_barcode.BarcodeWidth() > m_barcode.size.Width)
			{
				isContainsSmallSize = true;
			}
		}
		string text = m_barcode.Text;
		pdfBarcodeQuietZones.Left = PointToPixelConverter(m_barcode.QuietZone.Left);
		pdfBarcodeQuietZones.Right = PointToPixelConverter(m_barcode.QuietZone.Right);
		pdfBarcodeQuietZones.Top = PointToPixelConverter(m_barcode.QuietZone.Top);
		pdfBarcodeQuietZones.Bottom = PointToPixelConverter(m_barcode.QuietZone.Bottom);
		_ = m_barcode.isCheckDigitAdded;
		float num2 = PointToPixelConverter(m_barcode.NarrowBarWidth);
		bool encodeStartStopSymbols = m_barcode.EncodeStartStopSymbols;
		PdfColor backColor = m_barcode.BackColor;
		char startSymbol = m_barcode.StartSymbol;
		char stopSymbol = m_barcode.StopSymbol;
		PdfFont font = m_barcode.Font;
		float num3 = PointToPixelConverter(m_barcode.interCharacterGap);
		_ = m_barcode.continuous;
		string extendedText = m_barcode.ExtendedText;
		float num4 = PointToPixelConverter(m_barcode.BarHeight);
		float num5 = PointToPixelConverter(m_barcode.BarcodeToTextGapHeight);
		TextLocation textDisplayLocation = m_barcode.TextDisplayLocation;
		PdfBarcodeTextAlignment textAlignment = m_barcode.TextAlignment;
		bool isFontModified = m_barcode.isFontModified;
		Dictionary<char, BarcodeSymbolTable> barcodeSymbols = m_barcode.BarcodeSymbols;
		PdfColor textColor = m_barcode.TextColor;
		m_barcode.barcodeSpaceCount = 0;
		if (m_barcode is PdfGS1Code128Barcode || m_barcode is PdfCode128Barcode)
		{
			return ToImageRevamped();
		}
		PointF empty = PointF.Empty;
		string text2 = text;
		PdfBarcodeQuietZones pdfBarcodeQuietZones2 = pdfBarcodeQuietZones;
		if (!m_barcode.Validate(text))
		{
			throw new Exception("Barcode Text contains characters that are not accepted by this barcode specification.");
		}
		if (num2 < 1f)
		{
			num2 = 1f;
			m_barcode.NarrowBarWidth = 1f;
		}
		string text3 = m_barcode.GetTextToEncode();
		float num6 = 0f;
		float num7 = 0f;
		float num8 = 0f;
		float num9 = 0f;
		float num10 = 0f;
		if (text3 == null || text3.Length == 0)
		{
			throw new Exception("Barcode Text cannot be null or empty.");
		}
		if (encodeStartStopSymbols && startSymbol != 0 && stopSymbol != 0)
		{
			text3 = startSymbol + text3 + stopSymbol;
		}
		SKPaint sKPaint = new SKPaint();
		SKTypeface typeface;
		if (font != null && font is PdfTrueTypeFont)
		{
			typeface = SKTypeface.FromStream(((font as PdfTrueTypeFont).InternalFont as UnicodeTrueTypeFont).FontStream);
			sKPaint.TextSize = font.Size;
		}
		else if (font != null && font is PdfStandardFont)
		{
			typeface = SKTypeface.FromFamilyName(font.Name.ToString(), SKFontStyle.Normal);
			sKPaint.TextSize = font.Size;
		}
		else
		{
			typeface = SKTypeface.FromFamilyName("Helvetica", SKFontStyle.Normal);
		}
		sKPaint.Typeface = typeface;
		SizeF sizeF = font.MeasureString(text);
		SizeF sizeF2 = new SizeF(sizeF.Width, sizeF.Height);
		if (m_barcode.size != SizeF.Empty)
		{
			num9 = PointToPixelConverter(m_barcode.size.Width);
			num10 = PointToPixelConverter(m_barcode.size.Height);
			sizeF2.Height = PointToPixelConverter(sizeF2.Height);
			sizeF2.Width = PointToPixelConverter(sizeF2.Width);
		}
		else
		{
			sizeF2.Height = font.Height;
			num9 = pdfBarcodeQuietZones.Left + pdfBarcodeQuietZones.Right;
			string text4 = text3;
			foreach (char character in text4)
			{
				num9 += m_barcode.GetCharWidth(character) + num3;
			}
			num9 = (m_barcode.continuous ? (num9 - ((extendedText.Length > 0) ? (num3 * (float)(extendedText.Length - text.Length)) : num3)) : (num9 - num3 * (float)text3.Length));
			num10 = pdfBarcodeQuietZones.Top + pdfBarcodeQuietZones.Bottom + num4 + sizeF2.Height + num5;
			if (textDisplayLocation == TextLocation.None)
			{
				num10 -= sizeF2.Height + num5;
			}
		}
		new PdfSolidBrush(Color.FromArgb(backColor.ToArgb()));
		Bitmap bitmap = new Bitmap((int)num9, (int)num10);
		num = PointToPixelConverter(1.0734f);
		GraphicsHelper g = new GraphicsHelper(bitmap);
		g.FillRectangle(new PdfColor(Color.White), 0f, 0f, bitmap.Width, bitmap.Height);
		RectangleF rectangleF = default(RectangleF);
		if (textDisplayLocation == TextLocation.Top || textDisplayLocation == TextLocation.Bottom)
		{
			if (m_barcode.size == SizeF.Empty)
			{
				num10 += num5;
			}
			rectangleF = new RectangleF(empty, new SizeF(num9, num10));
		}
		else
		{
			if (pdfBarcodeQuietZones.IsAll && pdfBarcodeQuietZones.All > 0f)
			{
				num10 -= 2f * pdfBarcodeQuietZones.All;
			}
			else
			{
				float num11 = 0f;
				if (pdfBarcodeQuietZones.Top > 0f || pdfBarcodeQuietZones.Bottom > 0f)
				{
					num11 = pdfBarcodeQuietZones.Top + pdfBarcodeQuietZones.Bottom;
					num10 -= num11;
				}
			}
			rectangleF = new RectangleF(empty, new SizeF(num9, num10));
		}
		Color color = Color.FromArgb(backColor.ToArgb());
		if (m_barcode.size != SizeF.Empty)
		{
			if (isContainsSmallSize)
			{
				g.FillRectangle(color, empty.X, empty.Y, PointToPixelConverter(m_barcode.size.Width), PointToPixelConverter(m_barcode.size.Height));
			}
			else
			{
				g.FillRectangle(color, empty.X, empty.Y, m_barcode.size.Width, m_barcode.size.Height);
			}
		}
		else
		{
			g.FillRectangle(color, rectangleF.X, rectangleF.Y, rectangleF.Width, rectangleF.Height);
		}
		m_barcode.Bounds = new RectangleF(new PointF(empty.X, empty.Y), new SizeF(num9, num10));
		if (m_barcode.size != SizeF.Empty)
		{
			new SizeF(num9, num10);
			float num12 = 0f;
			string text4 = text3;
			foreach (char character2 in text4)
			{
				num12 += m_barcode.GetCharBarsCount(character2);
			}
			float num13 = num9 / num12;
			if (num2 <= 1f || num2 >= num13 || num13 >= num2)
			{
				num2 = (num9 - (pdfBarcodeQuietZones.Left + pdfBarcodeQuietZones.Right)) / (num12 + (float)m_barcode.barcodeSpaceCount);
				num3 = num2;
			}
			num4 = num10 - (pdfBarcodeQuietZones.Top + pdfBarcodeQuietZones.Bottom) - sizeF2.Height - num5;
			num6 = pdfBarcodeQuietZones.Left + rectangleF.Left;
		}
		else
		{
			num6 = pdfBarcodeQuietZones.Left + rectangleF.Left;
		}
		num8 = rectangleF.Top;
		num8 = ((textDisplayLocation != TextLocation.Top) ? (empty.Y + pdfBarcodeQuietZones.Top) : ((pdfBarcodeQuietZones.IsAll && pdfBarcodeQuietZones.All > 0f) ? (empty.Y + (rectangleF.Height - pdfBarcodeQuietZones.Top - num4)) : ((!(pdfBarcodeQuietZones.Bottom > 0f) && (!(pdfBarcodeQuietZones.Top > 0f) || !(pdfBarcodeQuietZones.Bottom > 0f))) ? (empty.Y + (rectangleF.Height - num4)) : ((!(pdfBarcodeQuietZones.Top > 0f) || !(pdfBarcodeQuietZones.Bottom > 0f)) ? (empty.Y + (sizeF2.Height + num5)) : (empty.Y + (pdfBarcodeQuietZones.Top + sizeF2.Height + num5))))));
		foreach (char c in text3)
		{
			foreach (KeyValuePair<char, BarcodeSymbolTable> item in barcodeSymbols)
			{
				BarcodeSymbolTable value = item.Value;
				if (value.Symbol != c)
				{
					continue;
				}
				byte[] bars = value.Bars;
				RectangleF rectangleF2 = default(RectangleF);
				for (int k = 0; k < bars.Length; k++)
				{
					float num14 = 0f;
					num14 = (float)(int)bars[k] * num2;
					if (bars[k] == 0)
					{
						num14 = num2;
					}
					rectangleF2 = ((textDisplayLocation != 0) ? new RectangleF(num6, num8, num14, num4) : new RectangleF(num6, num8, num14, rectangleF.Height));
					num6 = ((k % 2 != 0) ? (num6 + num14) : (num6 + PaintToImage(ref g, rectangleF2, m_barcode)));
				}
				if (bars.Length % 2 == 0)
				{
					continue;
				}
				if (m_barcode.size != SizeF.Empty)
				{
					num6 += num3;
					if (bars.Length % 2 != 0)
					{
						barcodeSpaceCount--;
					}
				}
				else
				{
					num6 += num3;
				}
			}
		}
		if (textDisplayLocation != 0)
		{
			text = text.Trim(m_barcode.startSymbol);
			text = text.Trim(m_barcode.stopSymbol);
			Color color2 = Color.FromArgb(textColor.ToArgb());
			switch (textAlignment)
			{
			case PdfBarcodeTextAlignment.Left:
				num6 = rectangleF.Left + pdfBarcodeQuietZones.Left;
				num7 = rectangleF.Width;
				break;
			case PdfBarcodeTextAlignment.Right:
				num6 = rectangleF.Left;
				num7 = rectangleF.Width - pdfBarcodeQuietZones.Right;
				break;
			default:
				num6 = rectangleF.Left + pdfBarcodeQuietZones.Left;
				num7 = rectangleF.Width - (pdfBarcodeQuietZones.Right + pdfBarcodeQuietZones.Left);
				break;
			}
			if (!isFontModified && m_barcode.size != SizeF.Empty)
			{
				SizeF sizeF3 = g.MeasureString(text, sKPaint);
				sizeF3.Height = PointToPixelConverter(sizeF2.Height);
				sizeF3.Width = PointToPixelConverter(sizeF2.Width);
				float num15 = sKPaint.TextSize;
				int num16 = 0;
				while (sizeF3.Width > num9)
				{
					SKPaint sKPaint2 = new SKPaint();
					sKPaint2.Typeface = SKTypeface.FromFamilyName("Helvetica", SKFontStyle.Normal);
					num15 = (sKPaint2.TextSize = num15 - 1f);
					sizeF3 = g.MeasureString(text, sKPaint2);
					if (sizeF3.Width <= num9)
					{
						sKPaint = sKPaint2;
						break;
					}
					num16++;
				}
			}
			sKPaint.Color = new SKColor(color2.R, color2.G, color2.B, color2.A);
			RectangleF rectangleF3;
			if (textDisplayLocation == TextLocation.Top)
			{
				rectangleF3 = new RectangleF(num6, empty.Y + pdfBarcodeQuietZones.Top, num7, sizeF2.Height);
				rectangleF3.Y = ((rectangleF3.Y < 0f) ? 0f : rectangleF3.Y);
			}
			else
			{
				rectangleF3 = new RectangleF(num6, empty.Y + pdfBarcodeQuietZones.Top + num5 + num4, num7, sizeF2.Height);
			}
			float x = rectangleF3.Left;
			SKRect bounds = default(SKRect);
			sKPaint.MeasureText(m_barcode.Text, ref bounds);
			float y = ((textDisplayLocation == TextLocation.Top) ? (rectangleF3.Top + rectangleF3.Height) : (rectangleF3.Bottom - rectangleF3.Height - bounds.MidY));
			switch (textAlignment)
			{
			case PdfBarcodeTextAlignment.Right:
			{
				SKRect bounds3 = default(SKRect);
				sKPaint.MeasureText(text, ref bounds3);
				x = rectangleF3.Right - bounds3.Width;
				break;
			}
			case PdfBarcodeTextAlignment.Center:
			{
				SKRect bounds2 = default(SKRect);
				sKPaint.MeasureText(text, ref bounds2);
				x = rectangleF3.Width / 2f - bounds2.MidX;
				break;
			}
			}
			g.m_canvas.DrawText(text, x, y, sKPaint);
		}
		pdfBarcodeQuietZones = pdfBarcodeQuietZones2;
		text = text2;
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

	private float PointToPixelConverter(float value)
	{
		float result = value;
		if (isContainsSmallSize)
		{
			result = new PdfUnitConvertor(300f).ConvertToPixels(value, PdfGraphicsUnit.Point);
		}
		return result;
	}

	internal Stream ToImageRevamped()
	{
		PointF empty = PointF.Empty;
		string text = m_barcode.Text;
		m_barcode.isCheckDigitAdded = false;
		PdfBarcodeQuietZones quietZone = m_barcode.QuietZone;
		PdfFont font = m_barcode.Font;
		if (!m_barcode.Validate(m_barcode.Text.Replace("(", "").Replace(")", "").Replace(" ", "")))
		{
			throw new Exception("Barcode Text contains characters that are not accepted by this barcode specification.");
		}
		if (m_barcode.NarrowBarWidth < 1f)
		{
			m_barcode.NarrowBarWidth = 1f;
		}
		List<byte[]> textToEncodeList = m_barcode.GetTextToEncodeList();
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		if (m_barcode.Text == null || m_barcode.Text.Length == 0)
		{
			throw new Exception("Barcode Text cannot be null or empty.");
		}
		SKPaint sKPaint = new SKPaint();
		SKTypeface typeface;
		if (font != null && font is PdfTrueTypeFont)
		{
			typeface = SKTypeface.FromStream(((font as PdfTrueTypeFont).InternalFont as UnicodeTrueTypeFont).FontStream);
			sKPaint.TextSize = font.Size;
		}
		else if (font != null && font is PdfStandardFont)
		{
			typeface = SKTypeface.FromFamilyName(font.Name.ToString(), SKFontStyle.Normal);
			sKPaint.TextSize = font.Size;
		}
		else
		{
			typeface = SKTypeface.FromFamilyName("Helvetica", SKFontStyle.Normal);
		}
		sKPaint.Typeface = typeface;
		SizeF sizeF = font.MeasureString(m_barcode.Text);
		if (m_barcode.size != SizeF.Empty)
		{
			sizeF.Height = font.Height;
			int num6 = 0;
			foreach (byte[] item in textToEncodeList)
			{
				foreach (byte b in item)
				{
					num6 += b;
				}
			}
			m_barcode.NarrowBarWidth = (m_barcode.Size.Width - (m_barcode.QuietZone.Left + m_barcode.QuietZone.Right)) / (float)num6;
			m_barcode.BarHeight = m_barcode.Size.Height - m_barcode.QuietZone.Top - m_barcode.QuietZone.Bottom - sizeF.Height - m_barcode.BarcodeToTextGapHeight;
			num4 = m_barcode.Size.Width;
			num5 = m_barcode.Size.Height;
		}
		else
		{
			sizeF.Height = font.Height;
			num4 = m_barcode.QuietZone.Left + m_barcode.QuietZone.Right;
			foreach (byte[] item2 in textToEncodeList)
			{
				foreach (byte b2 in item2)
				{
					num4 += (float)(int)b2 * m_barcode.NarrowBarWidth;
				}
			}
			num5 = m_barcode.QuietZone.Top + m_barcode.QuietZone.Bottom + m_barcode.BarHeight + sizeF.Height + m_barcode.BarcodeToTextGapHeight;
			if (m_barcode.TextDisplayLocation == TextLocation.None)
			{
				num5 -= sizeF.Height + m_barcode.BarcodeToTextGapHeight;
			}
			if (!m_barcode.barHeightEnabled)
			{
				m_barcode.Size = new SizeF(num4, num5);
			}
		}
		Color color = Color.FromArgb(m_barcode.BackColor.ToArgb());
		Bitmap bitmap = new Bitmap((int)num4, (int)num5);
		GraphicsHelper g = new GraphicsHelper(bitmap);
		g.FillRectangle(color, 0f, 0f, bitmap.Width, bitmap.Height);
		RectangleF rectangleF = default(RectangleF);
		if (m_barcode.TextDisplayLocation == TextLocation.Top)
		{
			PointF location = empty;
			if (m_barcode.QuietZone.IsAll && m_barcode.QuietZone.All > 0f)
			{
				num5 -= m_barcode.QuietZone.All;
			}
			rectangleF = new RectangleF(location, new SizeF(num4, num5));
		}
		else if (m_barcode.TextDisplayLocation == TextLocation.Bottom)
		{
			if (m_barcode.QuietZone.IsAll && m_barcode.QuietZone.All > 0f)
			{
				num5 -= m_barcode.QuietZone.All;
			}
			rectangleF = new RectangleF(empty, new SizeF(num4, num5));
		}
		else
		{
			if (m_barcode.QuietZone.IsAll && m_barcode.QuietZone.All > 0f)
			{
				num5 -= 2f * m_barcode.QuietZone.All;
			}
			else
			{
				float num7 = 0f;
				if (m_barcode.QuietZone.Top > 0f || m_barcode.QuietZone.Bottom > 0f)
				{
					num7 = m_barcode.QuietZone.Top + m_barcode.QuietZone.Bottom;
					num5 -= num7;
				}
			}
			rectangleF = new RectangleF(empty, new SizeF(num4, num5));
		}
		SizeF sizeF2 = new SizeF(m_barcode.size.Width, m_barcode.size.Height);
		Color color2 = Color.FromArgb(m_barcode.BackColor.ToArgb());
		if (sizeF2 != SizeF.Empty)
		{
			g.FillRectangle(color2, empty.X, empty.Y, sizeF2.Width, sizeF2.Height);
		}
		else
		{
			g.FillRectangle(color2, rectangleF.X, rectangleF.Y, rectangleF.Width, rectangleF.Height);
		}
		m_barcode.Bounds = new RectangleF(rectangleF.X, rectangleF.Y, rectangleF.Width, rectangleF.Height);
		num = m_barcode.QuietZone.Left + rectangleF.Left;
		num3 = 0f + rectangleF.Top;
		num3 = ((m_barcode.TextDisplayLocation != TextLocation.Top) ? (empty.Y + m_barcode.QuietZone.Top) : ((!(m_barcode.QuietZone.Bottom > 0f) || (m_barcode.QuietZone.IsAll && m_barcode.QuietZone.All > 0f)) ? (empty.Y + (rectangleF.Height - m_barcode.BarHeight)) : ((!(m_barcode.QuietZone.Top > 0f)) ? (empty.Y + (sizeF.Height + m_barcode.BarcodeToTextGapHeight)) : (empty.Y + (m_barcode.QuietZone.Top + sizeF.Height + m_barcode.BarcodeToTextGapHeight)))));
		RectangleF rectangleF2 = default(RectangleF);
		foreach (byte[] item3 in textToEncodeList)
		{
			int num8 = 0;
			byte[] current = item3;
			for (int i = 0; i < current.Length; i++)
			{
				float num9 = (float)(int)current[i] * m_barcode.NarrowBarWidth;
				rectangleF2 = ((m_barcode.TextDisplayLocation != 0) ? new RectangleF(num, num3, num9, m_barcode.BarHeight) : new RectangleF(num, num3, num9, rectangleF.Height));
				num = ((num8 % 2 != 0) ? (num + num9) : (num + PaintToImage(ref g, rectangleF2, m_barcode)));
				num8++;
			}
		}
		if (m_barcode.TextDisplayLocation != 0)
		{
			m_barcode.Text = m_barcode.Text.Trim(m_barcode.startSymbol);
			m_barcode.Text = m_barcode.Text.Trim(m_barcode.stopSymbol);
			Color.FromArgb(m_barcode.TextColor.ToArgb());
			if (m_barcode.TextAlignment == PdfBarcodeTextAlignment.Left)
			{
				num = rectangleF.Left + m_barcode.QuietZone.Left;
				num2 = rectangleF.Width;
			}
			else if (m_barcode.TextAlignment == PdfBarcodeTextAlignment.Right)
			{
				num = rectangleF.Left;
				num2 = rectangleF.Width - m_barcode.QuietZone.Right;
			}
			else
			{
				num = rectangleF.Left + m_barcode.QuietZone.Left;
				num2 = rectangleF.Width - (m_barcode.QuietZone.Right + m_barcode.QuietZone.Left);
			}
			if (!m_barcode.isFontModified && m_barcode.size != SizeF.Empty)
			{
				SizeF sizeF3 = g.MeasureString(m_barcode.Text, sKPaint);
				float num10 = sKPaint.TextSize;
				int num11 = 0;
				while (sizeF3.Width > num4)
				{
					SKPaint sKPaint2 = new SKPaint();
					sKPaint2.Typeface = SKTypeface.FromFamilyName("Helvetica", SKFontStyle.Normal);
					num10 = (sKPaint2.TextSize = num10 - 1f);
					sizeF3 = g.MeasureString(m_barcode.Text, sKPaint2);
					if (sizeF3.Width <= num4)
					{
						sKPaint = sKPaint2;
						break;
					}
					num11++;
				}
			}
			RectangleF rectangleF3;
			if (m_barcode.textDisplayLocation == TextLocation.Top)
			{
				rectangleF3 = new RectangleF(new PointF(num, empty.Y + m_barcode.QuietZone.Top), new SizeF(num2, sizeF.Height));
				rectangleF3.Y = ((rectangleF3.Y < 0f) ? 0f : rectangleF3.Y);
			}
			else
			{
				float num13 = sKPaint.MeasureText(m_barcode.Text);
				float num14 = sKPaint.TextSize;
				int num15 = 0;
				while (num13 > num4)
				{
					SKPaint sKPaint3 = new SKPaint();
					sKPaint3.Typeface = SKTypeface.FromFamilyName("Helvetica", SKFontStyle.Normal);
					num14 = (sKPaint3.TextSize = num14 - 1f);
					num13 = sKPaint3.MeasureText(m_barcode.Text);
					if (num13 <= num4)
					{
						sKPaint = sKPaint3;
					}
					num2 = num13;
					num15++;
				}
				rectangleF3 = new RectangleF(new PointF(num, empty.Y + m_barcode.QuietZone.Top + m_barcode.BarcodeToTextGapHeight + m_barcode.BarHeight), new SizeF(num2, sizeF.Height));
			}
			float x = rectangleF3.Left;
			SKRect bounds = default(SKRect);
			sKPaint.MeasureText(m_barcode.Text, ref bounds);
			float y = ((m_barcode.TextDisplayLocation == TextLocation.Top) ? (rectangleF3.Top + rectangleF3.Height) : (rectangleF3.Bottom - rectangleF3.Height - bounds.MidY));
			if (m_barcode.TextAlignment == PdfBarcodeTextAlignment.Right)
			{
				SKRect bounds2 = default(SKRect);
				sKPaint.MeasureText(m_barcode.Text, ref bounds2);
				x = rectangleF3.Right - bounds2.Width;
			}
			else if (m_barcode.TextAlignment == PdfBarcodeTextAlignment.Center)
			{
				SKRect bounds3 = default(SKRect);
				sKPaint.MeasureText(m_barcode.Text, ref bounds3);
				x = rectangleF3.Width / 2f - bounds3.MidX;
			}
			g.m_canvas.DrawText(m_barcode.Text, x, y, sKPaint);
		}
		m_barcode.QuietZone = quietZone;
		m_barcode.Text = text;
		MemoryStream memoryStream = new MemoryStream();
		bitmap.Save(memoryStream, ImageFormat.Png);
		memoryStream.Position = 0L;
		return memoryStream;
	}

	internal float PaintToImage(ref GraphicsHelper g, RectangleF barRect, PdfUnidimensionalBarcode barcode)
	{
		Color color = Color.FromArgb(m_barcode.BarColor.ToArgb());
		g.FillRectangle(new PdfColor(color), barRect.X, barRect.Y, barRect.Width, barRect.Height);
		return barRect.Width;
	}

	internal virtual List<byte[]> GetTextToEncodeList()
	{
		List<byte[]> list = new List<byte[]>();
		list.Add(new byte[0]);
		return list;
	}
}
