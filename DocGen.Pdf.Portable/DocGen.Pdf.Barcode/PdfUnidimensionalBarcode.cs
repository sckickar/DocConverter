using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Barcode;

public abstract class PdfUnidimensionalBarcode : PdfBarcode
{
	private const float UpcBarWidth = 1.0734f;

	internal int barcodeSpaceCount;

	internal bool isCheckDigitAdded;

	internal bool continuous;

	protected bool check;

	private bool autoTagcheck;

	private Dictionary<char, BarcodeSymbolTable> barcodeSymbols = new Dictionary<char, BarcodeSymbolTable>();

	private Dictionary<string, BarcodeSymbolTable> barcodeSymbolsString = new Dictionary<string, BarcodeSymbolTable>();

	internal TextLocation textDisplayLocation;

	private PdfFont font;

	internal char startSymbol;

	internal char stopSymbol;

	private string validatorExpression = string.Empty;

	private Regex codeValidator;

	private bool showCheckDigit;

	private bool enableCheckDigit;

	internal float interCharacterGap;

	internal float barcodeToTextGapHeight;

	private string barcodeEncodeText = string.Empty;

	private PdfBarcodeTextAlignment textAlignment;

	private bool encodeStartStopSymbols;

	internal bool isFontModified;

	private const int dpi = 96;

	private bool isContainsSmallSize;

	public PdfFont Font
	{
		get
		{
			return font;
		}
		set
		{
			font = value;
			isFontModified = true;
		}
	}

	public TextLocation TextDisplayLocation
	{
		get
		{
			return textDisplayLocation;
		}
		set
		{
			textDisplayLocation = value;
		}
	}

	public bool ShowCheckDigit
	{
		get
		{
			return showCheckDigit;
		}
		set
		{
			showCheckDigit = value;
		}
	}

	public bool EnableCheckDigit
	{
		get
		{
			return enableCheckDigit;
		}
		set
		{
			enableCheckDigit = value;
		}
	}

	public float BarcodeToTextGapHeight
	{
		get
		{
			return barcodeToTextGapHeight;
		}
		set
		{
			if (value < 0f)
			{
				throw new BarcodeException("Text to barcode gap cannot be negative.");
			}
			barcodeToTextGapHeight = value;
		}
	}

	public PdfBarcodeTextAlignment TextAlignment
	{
		get
		{
			return textAlignment;
		}
		set
		{
			textAlignment = value;
		}
	}

	public bool EncodeStartStopSymbols
	{
		get
		{
			return encodeStartStopSymbols;
		}
		set
		{
			encodeStartStopSymbols = value;
		}
	}

	internal Dictionary<char, BarcodeSymbolTable> BarcodeSymbols
	{
		get
		{
			return barcodeSymbols;
		}
		set
		{
			barcodeSymbols = value;
		}
	}

	internal Dictionary<string, BarcodeSymbolTable> BarcodeSymbolsString
	{
		get
		{
			return barcodeSymbolsString;
		}
		set
		{
			barcodeSymbolsString = value;
		}
	}

	internal char StartSymbol
	{
		get
		{
			return startSymbol;
		}
		set
		{
			startSymbol = value;
		}
	}

	internal char StopSymbol
	{
		get
		{
			return stopSymbol;
		}
		set
		{
			stopSymbol = value;
		}
	}

	internal string ValidatorExpression
	{
		get
		{
			return validatorExpression;
		}
		set
		{
			validatorExpression = value;
		}
	}

	internal float IntercharacterGap
	{
		get
		{
			return interCharacterGap;
		}
		set
		{
			interCharacterGap = value;
		}
	}

	public PdfUnidimensionalBarcode()
	{
		startSymbol = '\0';
		stopSymbol = '\0';
		interCharacterGap = 1f;
		barcodeToTextGapHeight = 5f;
		if (PdfDocument.ConformanceLevel == PdfConformanceLevel.None)
		{
			font = new PdfStandardFont(PdfFontFamily.Helvetica, 8f);
		}
		textAlignment = PdfBarcodeTextAlignment.Center;
		textDisplayLocation = TextLocation.Bottom;
		encodeStartStopSymbols = true;
		enableCheckDigit = false;
	}

	public virtual void Draw(PdfGraphics graphics)
	{
		Draw(graphics, base.Location);
	}

	public void Draw(PdfGraphics graphics, PointF location)
	{
		base.Location = location;
		if (this is PdfGS1Code128Barcode || this is PdfCode128Barcode)
		{
			DrawRevamped(graphics, location);
			return;
		}
		string text = base.Text;
		base.Text = base.Text.Replace("[FNC1]", "");
		if (!Validate(base.Text.Replace("[FNC1]", "")))
		{
			throw new BarcodeException("Barcode Text contains characters that are not accepted by this barcode specification.");
		}
		isCheckDigitAdded = false;
		string text2 = (barcodeEncodeText = GetTextToEncode());
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		if (text2 == null || text2.Length == 0)
		{
			throw new BarcodeException("Barcode Text cannot be null or empty.");
		}
		if (encodeStartStopSymbols && startSymbol != 0 && stopSymbol != 0)
		{
			text2 = startSymbol + text2 + stopSymbol;
		}
		SizeF sizeF = Font.MeasureString(base.Text);
		if (size != SizeF.Empty)
		{
			num4 = base.Size.Width;
			num5 = base.Size.Height;
		}
		else
		{
			num4 = base.QuietZone.Left + base.QuietZone.Right;
			string text3 = text2;
			foreach (char character in text3)
			{
				num4 += GetCharWidth(character) + interCharacterGap;
			}
			num4 = ((!continuous) ? (num4 - interCharacterGap * (float)text2.Length) : ((base.ExtendedText.Length <= 0) ? (num4 - interCharacterGap) : (num4 - interCharacterGap * (float)(base.ExtendedText.Length - base.Text.Length))));
			num5 = base.QuietZone.Top + base.QuietZone.Bottom + base.BarHeight;
			if (textDisplayLocation == TextLocation.Top)
			{
				num5 += sizeF.Height;
			}
			if (!barHeightEnabled)
			{
				base.Size = new SizeF(num4, num5);
			}
		}
		PdfBrush brush = new PdfSolidBrush(Color.FromArgb(base.BackColor.A, base.BackColor.R, base.BackColor.G, base.BackColor.B));
		RectangleF rectangle = default(RectangleF);
		if (textDisplayLocation != 0 && size == SizeF.Empty)
		{
			num5 += BarcodeToTextGapHeight;
		}
		if (textDisplayLocation == TextLocation.Top)
		{
			PointF pointF = location;
			rectangle = new RectangleF(pointF, new SizeF(num4, num5));
		}
		if (textDisplayLocation == TextLocation.Bottom)
		{
			rectangle = new RectangleF(location, new SizeF(num4, num5));
		}
		if (textDisplayLocation == TextLocation.None)
		{
			if (base.QuietZone.IsAll && base.QuietZone.All > 0f)
			{
				num5 -= 2f * base.QuietZone.All;
			}
			else
			{
				float num6 = 0f;
				if (base.QuietZone.Top > 0f || base.QuietZone.Bottom > 0f)
				{
					num6 = base.QuietZone.Top + base.QuietZone.Bottom;
					num5 -= num6;
				}
			}
			rectangle = new RectangleF(location, new SizeF(num4, num5));
		}
		PdfTemplate pdfTemplate = null;
		if (autoTagcheck)
		{
			pdfTemplate = new PdfTemplate(new SizeF(num4, num5));
			pdfTemplate.Graphics.DrawRectangle(brush, rectangle);
		}
		else if (size != SizeF.Empty)
		{
			graphics.DrawRectangle(brush, new RectangleF(location.X, location.Y, size.Width, size.Height));
		}
		else
		{
			graphics.DrawRectangle(brush, rectangle);
		}
		base.Bounds = rectangle;
		if (size != SizeF.Empty)
		{
			GetSizeValue();
			float num7 = 0f;
			string text3 = text2;
			foreach (char character2 in text3)
			{
				num7 += GetCharBarsCount(character2);
			}
			float num8 = num4 / num7;
			if (base.NarrowBarWidth <= 1f || base.NarrowBarWidth >= num8 || num8 >= base.NarrowBarWidth)
			{
				base.NarrowBarWidth = (num4 - (base.QuietZone.Left + base.QuietZone.Right)) / (num7 + (float)barcodeSpaceCount);
				if (this is PdfCodeUpcBarcode)
				{
					IntercharacterGap = base.NarrowBarWidth * 1.0734f;
				}
				else
				{
					IntercharacterGap = base.NarrowBarWidth;
				}
			}
			base.BarHeight = num5 - (base.QuietZone.Top + base.QuietZone.Bottom) - sizeF.Height - BarcodeToTextGapHeight;
			num = base.QuietZone.Left + rectangle.Left;
		}
		else
		{
			num = base.QuietZone.Left + rectangle.Left;
		}
		num3 = 0f + rectangle.Top;
		num3 = ((textDisplayLocation != TextLocation.Top) ? (location.Y + base.QuietZone.Top) : ((base.QuietZone.IsAll && base.QuietZone.All > 0f) ? (location.Y + (rectangle.Height - base.QuietZone.Top - base.BarHeight)) : ((!(base.QuietZone.Bottom > 0f) && (!(base.QuietZone.Top > 0f) || !(base.QuietZone.Bottom > 0f))) ? (location.Y + (rectangle.Height - base.BarHeight)) : ((!(base.QuietZone.Top > 0f) || !(base.QuietZone.Bottom > 0f)) ? (location.Y + (sizeF.Height + BarcodeToTextGapHeight)) : (location.Y + (base.QuietZone.Top + sizeF.Height + BarcodeToTextGapHeight))))));
		for (int j = 0; j < text2.Length; j++)
		{
			char c = text2[j];
			foreach (KeyValuePair<char, BarcodeSymbolTable> barcodeSymbol in BarcodeSymbols)
			{
				BarcodeSymbolTable value = barcodeSymbol.Value;
				if (value.Symbol != c)
				{
					continue;
				}
				byte[] bars = value.Bars;
				RectangleF rectangleF = default(RectangleF);
				for (int k = 0; k < bars.Length; k++)
				{
					float num9 = 0f;
					num9 = ((!(this is PdfCodeUpcBarcode)) ? ((float)(int)bars[k] * base.NarrowBarWidth) : ((float)(int)bars[k] * base.NarrowBarWidth * 1.0734f));
					if (bars[k] == 0)
					{
						num9 = base.NarrowBarWidth;
					}
					rectangleF = ((textDisplayLocation != 0) ? new RectangleF(num, num3, num9, base.BarHeight) : new RectangleF(num, num3, num9, rectangle.Height));
					if (this is PdfCodeUpcBarcode && j > 1 && j <= 7)
					{
						num = ((k % 2 == 0) ? (num + num9) : ((!autoTagcheck) ? (num + PaintRectangle(graphics, rectangleF)) : (num + PaintRectangleTag(pdfTemplate, rectangleF))));
						continue;
					}
					if (k % 2 == 0)
					{
						num = ((!autoTagcheck) ? (num + PaintRectangle(graphics, rectangleF)) : (num + PaintRectangleTag(pdfTemplate, rectangleF)));
						continue;
					}
					num += num9;
					if (this is PdfCodeUpcBarcode && bars.Length % 2 == 0 && j == 1 && k == bars.Length - 1)
					{
						num -= num9;
					}
				}
				if (bars.Length % 2 == 0 && (!(this is PdfCodeUpcBarcode) || bars.Length % 2 != 0 || j != 7))
				{
					continue;
				}
				if (size != SizeF.Empty)
				{
					num += IntercharacterGap;
					if (bars.Length % 2 != 0)
					{
						barcodeSpaceCount--;
					}
				}
				else
				{
					num += IntercharacterGap;
				}
			}
		}
		if (autoTagcheck)
		{
			graphics.DrawPdfTemplate(pdfTemplate, location);
		}
		if (textDisplayLocation != 0)
		{
			base.Text = base.Text.Trim(startSymbol);
			base.Text = base.Text.Trim(stopSymbol);
			PdfStringFormat format = new PdfStringFormat((PdfTextAlignment)textAlignment);
			PdfBrush brush2 = new PdfSolidBrush(Color.FromArgb(base.TextColor.A, base.TextColor.R, base.TextColor.G, base.TextColor.B));
			if (textAlignment == PdfBarcodeTextAlignment.Left)
			{
				num = rectangle.Left + base.QuietZone.Left;
				num2 = rectangle.Width;
			}
			else if (textAlignment == PdfBarcodeTextAlignment.Right)
			{
				num = rectangle.Left;
				num2 = rectangle.Width - base.QuietZone.Right;
			}
			else
			{
				num = rectangle.Left + base.QuietZone.Left;
				num2 = rectangle.Width - (base.QuietZone.Right + base.QuietZone.Left);
			}
			if (!isFontModified && size != SizeF.Empty)
			{
				SizeF sizeF2 = Font.MeasureString(base.Text);
				float num10 = Font.Size;
				int num11 = 0;
				while (sizeF2.Width > num4)
				{
					Font = new PdfStandardFont(PdfFontFamily.Helvetica, num10 -= 1f);
					sizeF2 = Font.MeasureString(base.Text);
					if (sizeF2.Width <= num4)
					{
						Font = new PdfStandardFont(PdfFontFamily.Helvetica, num10);
						break;
					}
					num11++;
				}
			}
			string text4 = string.Empty;
			bool flag = false;
			if (text.Contains("FNC1"))
			{
				string[] array = text.Split(new string[1] { "[FNC1]" }, StringSplitOptions.RemoveEmptyEntries);
				for (int l = 0; l < array.Length; l++)
				{
					flag = true;
					text4 = ((array[l].Contains("(") && array[l].Contains(")")) ? (text4 + array[l]) : (text4 + array[l].Insert(0, "(").Insert(3, ")")));
				}
			}
			if (textDisplayLocation == TextLocation.Top)
			{
				RectangleF layoutRectangle = new RectangleF(new PointF(num, location.Y + base.QuietZone.Top), new SizeF(num2, sizeF.Height));
				if (!flag)
				{
					graphics.DrawString(base.Text, Font, brush2, layoutRectangle, format);
				}
				else
				{
					graphics.DrawString(text4, Font, brush2, layoutRectangle, format);
				}
			}
			else
			{
				RectangleF rectangleF2 = default(RectangleF);
				rectangleF2 = new RectangleF(new PointF(num, location.Y + base.QuietZone.Top + barcodeToTextGapHeight + base.BarHeight), new SizeF(num2, sizeF.Height));
				if (this is PdfCodeUpcBarcode)
				{
					char[] array2 = text2.ToCharArray();
					string text5 = string.Empty;
					for (int m = 0; m < array2.Length; m++)
					{
						if ((m > 1 && m <= 7) || (m > 8 && m <= 14))
						{
							text5 += array2[m];
						}
					}
					graphics.DrawString(text5, Font, brush2, rectangleF2, format);
				}
				else if (!flag)
				{
					graphics.DrawString(base.Text, Font, brush2, rectangleF2, format);
				}
				else
				{
					graphics.DrawString(text4, Font, brush2, rectangleF2, format);
				}
			}
		}
		base.Text = text;
	}

	public void Draw(PdfGraphics graphics, PointF location, SizeF size)
	{
		Draw(graphics, location.X, location.Y, size.Width, size.Height);
	}

	public void Draw(PdfGraphics graphics, RectangleF rectangle)
	{
		Draw(graphics, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
	}

	public void Draw(PdfGraphics graphics, float x, float y, float width, float height)
	{
		if (this is PdfGS1Code128Barcode || this is PdfCode128CBarcode)
		{
			DrawRevamped(graphics, x, y, width, height);
			return;
		}
		if (this is PdfCode128Barcode)
		{
			DrawRevamped(graphics, x, y, width, height);
			return;
		}
		string text = base.Text;
		if (!Validate(base.Text))
		{
			throw new BarcodeException("Barcode Text contains characters that are not accepted by this barcode specification.");
		}
		string text2 = GetTextToEncode();
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		barcodeSpaceCount = 0;
		float num6 = base.NarrowBarWidth;
		if (text2 == null || text2.Length == 0)
		{
			throw new BarcodeException("Barcode Text cannot be null or empty.");
		}
		if (encodeStartStopSymbols && startSymbol != 0 && stopSymbol != 0)
		{
			text2 = startSymbol + text2 + stopSymbol;
		}
		SizeF sizeF = Font.MeasureString(base.Text);
		num4 = width;
		num5 = height;
		base.Location = new PointF(x, y);
		base.Size = new SizeF(num4, num5);
		PdfBrush brush = new PdfSolidBrush(Color.FromArgb(base.BackColor.A, base.BackColor.R, base.BackColor.G, base.BackColor.B));
		RectangleF rectangle = default(RectangleF);
		if (textDisplayLocation == TextLocation.Top)
		{
			if (size == SizeF.Empty)
			{
				num5 += BarcodeToTextGapHeight;
			}
			rectangle = new RectangleF(x, y, num4, num5);
		}
		if (textDisplayLocation == TextLocation.Bottom)
		{
			if (size == SizeF.Empty)
			{
				num5 += BarcodeToTextGapHeight;
			}
			rectangle = new RectangleF(x, y, num4, num5);
		}
		if (textDisplayLocation == TextLocation.None)
		{
			if (base.QuietZone.IsAll && base.QuietZone.All > 0f)
			{
				num5 -= 2f * base.QuietZone.All;
			}
			else
			{
				float num7 = 0f;
				if (base.QuietZone.Top > 0f || base.QuietZone.Bottom > 0f)
				{
					num7 = base.QuietZone.Top + base.QuietZone.Bottom;
					num5 -= num7;
				}
			}
			rectangle = new RectangleF(x, y, num4, num5);
		}
		PdfTemplate pdfTemplate = null;
		if (autoTagcheck)
		{
			pdfTemplate = new PdfTemplate(new SizeF(width, height));
			pdfTemplate.Graphics.DrawRectangle(brush, rectangle);
		}
		else if (size != SizeF.Empty)
		{
			graphics.DrawRectangle(brush, new RectangleF(x, y, size.Width, size.Height));
		}
		else
		{
			graphics.DrawRectangle(brush, rectangle);
		}
		base.Bounds = rectangle;
		GetSizeValue();
		float num8 = 0f;
		string text3 = text2;
		foreach (char character in text3)
		{
			num8 += GetCharBarsCount(character);
		}
		float num9 = num4 / num8;
		if (base.NarrowBarWidth <= 1f || base.NarrowBarWidth >= num9 || (num9 >= base.NarrowBarWidth && base.NarrowBarWidth != num9))
		{
			num6 = (num4 - (base.QuietZone.Left + base.QuietZone.Right)) / (num8 + (float)barcodeSpaceCount);
			if (this is PdfCodeUpcBarcode)
			{
				IntercharacterGap = num6 * 1.0734f;
			}
			else
			{
				IntercharacterGap = num6;
			}
		}
		base.BarHeight = num5 - (base.QuietZone.Top + base.QuietZone.Bottom) - sizeF.Height - BarcodeToTextGapHeight;
		num = base.QuietZone.Left + rectangle.Left;
		num3 = 0f + rectangle.Top;
		num3 = ((textDisplayLocation != TextLocation.Top) ? (y + base.QuietZone.Top) : ((base.QuietZone.IsAll && base.QuietZone.All > 0f) ? (y + (rectangle.Height - base.QuietZone.Top - base.BarHeight)) : ((!(base.QuietZone.Bottom > 0f) && (!(base.QuietZone.Top > 0f) || !(base.QuietZone.Bottom > 0f))) ? (y + (rectangle.Height - base.BarHeight)) : ((!(base.QuietZone.Top > 0f) || !(base.QuietZone.Bottom > 0f)) ? (y + (sizeF.Height + BarcodeToTextGapHeight)) : (y + (base.QuietZone.Top + sizeF.Height + BarcodeToTextGapHeight))))));
		for (int j = 0; j < text2.Length; j++)
		{
			char c = text2[j];
			foreach (KeyValuePair<char, BarcodeSymbolTable> barcodeSymbol in BarcodeSymbols)
			{
				BarcodeSymbolTable value = barcodeSymbol.Value;
				if (value.Symbol != c)
				{
					continue;
				}
				byte[] bars = value.Bars;
				RectangleF rectangleF = default(RectangleF);
				for (int k = 0; k < bars.Length; k++)
				{
					float num10 = 0f;
					num10 = ((!(this is PdfCodeUpcBarcode)) ? ((float)(int)bars[k] * num6) : ((float)(int)bars[k] * num6 * 1.0734f));
					rectangleF = ((textDisplayLocation != 0) ? new RectangleF(num, num3, num10, base.BarHeight) : new RectangleF(num, num3, num10, rectangle.Height));
					if (this is PdfCodeUpcBarcode && j > 1 && j <= 7)
					{
						num = ((k % 2 == 0) ? (num + num10) : ((!autoTagcheck) ? (num + PaintRectangle(graphics, rectangleF)) : (num + PaintRectangleTag(pdfTemplate, rectangleF))));
						continue;
					}
					if (k % 2 == 0)
					{
						num = ((!autoTagcheck) ? (num + PaintRectangle(graphics, rectangleF)) : (num + PaintRectangleTag(pdfTemplate, rectangleF)));
						continue;
					}
					num += num10;
					if (this is PdfCodeUpcBarcode && bars.Length % 2 == 0 && j == 1 && k == bars.Length - 1)
					{
						num -= num10;
					}
				}
				if (bars.Length % 2 != 0 || (this is PdfCodeUpcBarcode && bars.Length % 2 == 0 && j == 7))
				{
					num += IntercharacterGap;
				}
			}
		}
		if (autoTagcheck)
		{
			graphics.DrawPdfTemplate(pdfTemplate, new PointF(x, y));
		}
		if (textDisplayLocation != 0)
		{
			base.Text = base.Text.Trim(startSymbol);
			base.Text = base.Text.Trim(stopSymbol);
			PdfStringFormat format = new PdfStringFormat((PdfTextAlignment)textAlignment);
			PdfBrush brush2 = new PdfSolidBrush(Color.FromArgb(base.TextColor.A, base.TextColor.R, base.TextColor.G, base.TextColor.B));
			if (textAlignment == PdfBarcodeTextAlignment.Left)
			{
				num = rectangle.Left + base.QuietZone.Left;
				num2 = rectangle.Width;
			}
			else if (textAlignment == PdfBarcodeTextAlignment.Right)
			{
				num = rectangle.Left;
				num2 = rectangle.Width - base.QuietZone.Right;
			}
			else
			{
				num = rectangle.Left + base.QuietZone.Left;
				num2 = rectangle.Width - (base.QuietZone.Right + base.QuietZone.Left);
			}
			if (!isFontModified)
			{
				SizeF sizeF2 = Font.MeasureString(base.Text);
				float num11 = Font.Size;
				int num12 = 0;
				while (sizeF2.Width > num4)
				{
					Font = new PdfStandardFont(PdfFontFamily.Helvetica, num11 -= 1f);
					sizeF2 = Font.MeasureString(base.Text);
					if (sizeF2.Width <= num4)
					{
						Font = new PdfStandardFont(PdfFontFamily.Helvetica, num11);
						break;
					}
					num12++;
				}
			}
			if (textDisplayLocation == TextLocation.Top)
			{
				graphics.DrawString(layoutRectangle: new RectangleF(new PointF(num, y + base.QuietZone.Top), new SizeF(num2, sizeF.Height)), s: base.Text, font: Font, brush: brush2, format: format);
			}
			else
			{
				RectangleF rectangleF2 = default(RectangleF);
				rectangleF2 = new RectangleF(new PointF(num, y + base.QuietZone.Top + barcodeToTextGapHeight + base.BarHeight), new SizeF(num2, sizeF.Height));
				if (this is PdfCodeUpcBarcode)
				{
					char[] array = text2.ToCharArray();
					string text4 = string.Empty;
					for (int l = 0; l < array.Length; l++)
					{
						if ((l > 1 && l <= 7) || (l > 8 && l <= 14))
						{
							text4 += array[l];
						}
					}
					graphics.DrawString(text4, Font, brush2, rectangleF2, format);
				}
				else
				{
					graphics.DrawString(base.Text, Font, brush2, rectangleF2, format);
				}
			}
		}
		base.Text = text;
	}

	public virtual void Draw(PdfPageBase page, PointF location)
	{
		Draw(page.Graphics, location);
	}

	public void Draw(PdfPageBase page, PointF location, SizeF size)
	{
		Draw(page, location.X, location.Y, size.Width, size.Height);
	}

	public void Draw(PdfPageBase page, RectangleF rectangle)
	{
		Draw(page, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
	}

	public void Draw(PdfPageBase page, float x, float y, float width, float height)
	{
		if (page is PdfPage && (page as PdfPage).Document != null)
		{
			autoTagcheck = (page as PdfPage).Document.AutoTag;
		}
		Draw(page.Graphics, x, y, width, height);
	}

	public virtual void Draw(PdfPageBase page)
	{
		if (page is PdfPage && (page as PdfPage).Document != null)
		{
			autoTagcheck = (page as PdfPage).Document.AutoTag;
		}
		Draw(page, base.Location);
	}

	internal void DrawRevamped(PdfGraphics graphics, PointF location)
	{
		string text = base.Text;
		isCheckDigitAdded = false;
		PdfBarcodeQuietZones quietZone = base.QuietZone;
		if (!(this is PdfGS1Code128Barcode) && !Validate(base.Text.Replace("(", "").Replace(")", "").Replace("[FNC1]", "")) && !(this is PdfCode128Barcode))
		{
			throw new BarcodeException("Barcode Text contains characters that are not accepted by this barcode specification.");
		}
		if (base.NarrowBarWidth < 1f)
		{
			base.NarrowBarWidth = 1f;
		}
		List<byte[]> textToEncodeList = GetTextToEncodeList();
		string text2 = string.Empty;
		bool flag = false;
		if (base.Text.Contains("FNC1"))
		{
			string[] array = base.Text.Split(new string[1] { "[FNC1]" }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < array.Length; i++)
			{
				flag = true;
				text2 = ((array[i].Contains("(") && array[i].Contains(")")) ? (text2 + array[i]) : (text2 + array[i].Insert(0, "(").Insert(3, ")")));
			}
		}
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		if (base.Text == null || base.Text.Length == 0)
		{
			throw new BarcodeException("Barcode Text cannot be null or empty.");
		}
		SizeF sizeF = Font.MeasureString(base.Text);
		if (size != SizeF.Empty && isCustomSize)
		{
			int num6 = 0;
			foreach (byte[] item in textToEncodeList)
			{
				foreach (byte b in item)
				{
					num6 += b;
				}
			}
			if (base.NarrowBarWidth <= 1f || base.NarrowBarWidth >= base.Size.Width / (float)num6 || base.Size.Width / (float)num6 >= base.NarrowBarWidth)
			{
				base.NarrowBarWidth = (base.Size.Width - (base.QuietZone.Left + base.QuietZone.Right)) / (float)num6;
			}
			base.BarHeight = base.Size.Height - base.QuietZone.Top - base.QuietZone.Bottom - sizeF.Height - BarcodeToTextGapHeight;
			num4 = base.Size.Width;
			num5 = base.Size.Height;
		}
		else
		{
			num4 = base.QuietZone.Left + base.QuietZone.Right;
			int num7 = 0;
			foreach (byte[] item2 in textToEncodeList)
			{
				foreach (byte b2 in item2)
				{
					num4 += (float)(int)b2 * base.NarrowBarWidth;
					num7 += b2;
				}
			}
			num5 = base.QuietZone.Top + base.QuietZone.Bottom + base.BarHeight + sizeF.Height + BarcodeToTextGapHeight;
			if (TextDisplayLocation == TextLocation.None)
			{
				num5 -= sizeF.Height + BarcodeToTextGapHeight;
			}
			if (!barHeightEnabled)
			{
				base.Size = new SizeF(num4, num5);
			}
		}
		PdfBrush brush = new PdfSolidBrush(Color.FromArgb(base.BackColor.ToArgb()));
		graphics.DrawRectangle(brush, new RectangleF(location, new SizeF(num4, num5)));
		RectangleF rectangleF = default(RectangleF);
		if (TextDisplayLocation == TextLocation.Top)
		{
			PointF pointF = location;
			if (base.QuietZone.IsAll && base.QuietZone.All > 0f)
			{
				num5 -= base.QuietZone.All;
			}
			rectangleF = new RectangleF(pointF, new SizeF(num4, num5));
		}
		else if (TextDisplayLocation == TextLocation.Bottom)
		{
			if (base.QuietZone.IsAll && base.QuietZone.All > 0f)
			{
				num5 -= base.QuietZone.All;
			}
			rectangleF = new RectangleF(location, new SizeF(num4, num5));
		}
		else
		{
			if (base.QuietZone.IsAll && base.QuietZone.All > 0f)
			{
				num5 -= 2f * base.QuietZone.All;
			}
			else
			{
				float num8 = 0f;
				if (base.QuietZone.Top > 0f || base.QuietZone.Bottom > 0f)
				{
					num8 = base.QuietZone.Top + base.QuietZone.Bottom;
					num5 -= num8;
				}
			}
			rectangleF = new RectangleF(location, new SizeF(num4, num5));
		}
		Color.FromArgb(base.BackColor.ToArgb());
		graphics.DrawRectangle(new PdfSolidBrush(base.BackColor), rectangleF);
		base.Bounds = rectangleF;
		if (base.Location != PointF.Empty)
		{
			base.Location = base.Bounds.Location;
		}
		if (size == SizeF.Empty)
		{
			size = base.Bounds.Size;
		}
		num = base.QuietZone.Left + rectangleF.Left;
		num3 = 0f + rectangleF.Top;
		num3 = ((TextDisplayLocation != TextLocation.Top) ? (location.Y + base.QuietZone.Top) : ((!(base.QuietZone.Bottom > 0f) || (base.QuietZone.IsAll && base.QuietZone.All > 0f)) ? (location.Y + (rectangleF.Height - base.BarHeight)) : ((!(base.QuietZone.Top > 0f)) ? (location.Y + (sizeF.Height + BarcodeToTextGapHeight)) : (location.Y + (base.QuietZone.Top + sizeF.Height + BarcodeToTextGapHeight)))));
		foreach (byte[] item3 in textToEncodeList)
		{
			int num9 = 0;
			RectangleF rectangleF2 = default(RectangleF);
			byte[] current = item3;
			for (int j = 0; j < current.Length; j++)
			{
				float num10 = (float)(int)current[j] * base.NarrowBarWidth;
				rectangleF2 = ((textDisplayLocation != 0) ? new RectangleF(num, num3, num10, base.BarHeight) : new RectangleF(num, num3, num10, rectangleF.Height));
				num = ((num9 % 2 != 0) ? (num + num10) : (num + PaintToImage(graphics, rectangleF2)));
				num9++;
			}
		}
		if (TextDisplayLocation != 0)
		{
			base.Text = base.Text.Trim(startSymbol);
			base.Text = base.Text.Trim(stopSymbol);
			PdfStringFormat pdfStringFormat = new PdfStringFormat();
			pdfStringFormat.LineAlignment = PdfVerticalAlignment.Middle;
			if (TextAlignment == PdfBarcodeTextAlignment.Left)
			{
				pdfStringFormat.Alignment = PdfTextAlignment.Left;
			}
			else if (TextAlignment == PdfBarcodeTextAlignment.Right)
			{
				pdfStringFormat.Alignment = PdfTextAlignment.Right;
			}
			else
			{
				pdfStringFormat.Alignment = PdfTextAlignment.Center;
			}
			Color.FromArgb(base.TextColor.ToArgb());
			if (TextAlignment == PdfBarcodeTextAlignment.Left)
			{
				num = rectangleF.Left + base.QuietZone.Left;
				num2 = rectangleF.Width;
			}
			else if (TextAlignment == PdfBarcodeTextAlignment.Right)
			{
				num = rectangleF.Left;
				num2 = rectangleF.Width - base.QuietZone.Right;
			}
			else
			{
				num = rectangleF.Left + base.QuietZone.Left;
				num2 = rectangleF.Width - (base.QuietZone.Right + base.QuietZone.Left);
			}
			if (!isFontModified && size != SizeF.Empty)
			{
				SizeF sizeF2 = Font.MeasureString(base.Text);
				float num11 = Font.Size;
				int num12 = 0;
				while (sizeF2.Width > num4)
				{
					Font = new PdfStandardFont(PdfFontFamily.Helvetica, num11 -= 1f);
					sizeF2 = Font.MeasureString(base.Text);
					if (sizeF2.Width <= num4)
					{
						Font = new PdfStandardFont(PdfFontFamily.Helvetica, num11);
						break;
					}
					num12++;
				}
			}
			if (textDisplayLocation == TextLocation.Top)
			{
				RectangleF layoutRectangle = new RectangleF(new PointF(num, location.Y + base.QuietZone.Top), new SizeF(num2, sizeF.Height));
				layoutRectangle.Y = ((layoutRectangle.Y < 0f) ? 0f : layoutRectangle.Y);
				if (!flag)
				{
					graphics.DrawString(base.Text, Font, new PdfSolidBrush(base.TextColor), layoutRectangle, pdfStringFormat);
				}
				else
				{
					graphics.DrawString(text2, Font, new PdfSolidBrush(base.TextColor), layoutRectangle, pdfStringFormat);
				}
			}
			else
			{
				RectangleF layoutRectangle2 = new RectangleF(new PointF(num, location.Y + base.QuietZone.Top + BarcodeToTextGapHeight + base.BarHeight), new SizeF(num2, sizeF.Height));
				if (!flag)
				{
					graphics.DrawString(base.Text, Font, new PdfSolidBrush(base.TextColor), layoutRectangle2, pdfStringFormat);
				}
				else
				{
					graphics.DrawString(text2, Font, new PdfSolidBrush(base.TextColor), layoutRectangle2, pdfStringFormat);
				}
			}
		}
		base.QuietZone = quietZone;
		base.Text = text;
	}

	internal void DrawRevamped(PdfPageBase page, PointF location)
	{
		DrawRevamped(page.Graphics, location);
	}

	internal void DrawRevamped(PdfGraphics graphics, float x, float y, float width, float height)
	{
		string text = base.Text;
		isCheckDigitAdded = false;
		PdfBarcodeQuietZones quietZone = base.QuietZone;
		if (!Validate(base.Text.Replace("(", "").Replace(")", "")) && !(this is PdfCode128Barcode))
		{
			throw new BarcodeException("Barcode Text contains characters that are not accepted by this barcode specification.");
		}
		if (base.NarrowBarWidth < 1f)
		{
			base.NarrowBarWidth = 1f;
		}
		List<byte[]> textToEncodeList = GetTextToEncodeList();
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		if (base.Text == null || base.Text.Length == 0)
		{
			throw new BarcodeException("Barcode Text cannot be null or empty.");
		}
		SizeF sizeF = Font.MeasureString(base.Text);
		int num4 = 0;
		foreach (byte[] item in textToEncodeList)
		{
			foreach (byte b in item)
			{
				num4 += b;
			}
		}
		if (base.NarrowBarWidth <= 1f || base.NarrowBarWidth >= width / (float)num4)
		{
			base.NarrowBarWidth = (width - (base.QuietZone.Left + base.QuietZone.Right)) / (float)num4;
		}
		base.BarHeight = height - base.QuietZone.Top - base.QuietZone.Bottom - sizeF.Height - BarcodeToTextGapHeight;
		base.Location = new PointF(x, y);
		base.Size = new SizeF(width, height);
		Color color = Color.FromArgb(base.BackColor.ToArgb());
		PdfBrush brush = new PdfSolidBrush(color);
		graphics.DrawRectangle(brush, x, y, width, height);
		RectangleF rectangleF = default(RectangleF);
		if (TextDisplayLocation == TextLocation.Top)
		{
			if (size == SizeF.Empty)
			{
				height += BarcodeToTextGapHeight;
			}
			if (base.QuietZone.IsAll && base.QuietZone.All > 0f)
			{
				height -= base.QuietZone.All;
			}
			rectangleF = new RectangleF(x, y, width, height);
		}
		else if (TextDisplayLocation == TextLocation.Bottom)
		{
			if (size == SizeF.Empty)
			{
				height += BarcodeToTextGapHeight;
			}
			if (base.QuietZone.IsAll && base.QuietZone.All > 0f)
			{
				height -= base.QuietZone.All;
			}
			rectangleF = new RectangleF(x, y, width, height);
		}
		else
		{
			if (base.QuietZone.IsAll && base.QuietZone.All > 0f)
			{
				height -= 2f * base.QuietZone.All;
			}
			else
			{
				float num5 = 0f;
				if (base.QuietZone.Top > 0f || base.QuietZone.Bottom > 0f)
				{
					num5 = base.QuietZone.Top + base.QuietZone.Bottom;
					height -= num5;
				}
			}
			rectangleF = new RectangleF(x, y, width, height);
		}
		Color.FromArgb(base.BackColor.ToArgb());
		new PdfSolidBrush(color);
		graphics.DrawRectangle(new PdfSolidBrush(base.BackColor), rectangleF);
		base.Bounds = rectangleF;
		num = base.QuietZone.Left + rectangleF.Left;
		num3 = 0f + rectangleF.Top;
		num3 = ((TextDisplayLocation != TextLocation.Top) ? (y + base.QuietZone.Top) : ((!(base.QuietZone.Bottom > 0f) || (base.QuietZone.IsAll && base.QuietZone.All > 0f)) ? (y + (rectangleF.Height - base.BarHeight)) : ((!(base.QuietZone.Top > 0f)) ? (y + (sizeF.Height + BarcodeToTextGapHeight)) : (y + (base.QuietZone.Top + sizeF.Height + BarcodeToTextGapHeight)))));
		foreach (byte[] item2 in textToEncodeList)
		{
			int num6 = 0;
			RectangleF rectangleF2 = default(RectangleF);
			byte[] current = item2;
			for (int i = 0; i < current.Length; i++)
			{
				float num7 = (float)(int)current[i] * base.NarrowBarWidth;
				rectangleF2 = ((textDisplayLocation != 0) ? new RectangleF(num, num3, num7, base.BarHeight) : new RectangleF(num, num3, num7, rectangleF.Height));
				num = ((num6 % 2 != 0) ? (num + num7) : (num + PaintToImage(graphics, rectangleF2)));
				num6++;
			}
		}
		if (TextDisplayLocation != 0)
		{
			base.Text = base.Text.Trim(startSymbol);
			base.Text = base.Text.Trim(stopSymbol);
			PdfStringFormat pdfStringFormat = new PdfStringFormat();
			pdfStringFormat.LineAlignment = PdfVerticalAlignment.Middle;
			if (TextAlignment == PdfBarcodeTextAlignment.Left)
			{
				pdfStringFormat.Alignment = PdfTextAlignment.Left;
			}
			else if (TextAlignment == PdfBarcodeTextAlignment.Right)
			{
				pdfStringFormat.Alignment = PdfTextAlignment.Right;
			}
			else
			{
				pdfStringFormat.Alignment = PdfTextAlignment.Center;
			}
			new PdfSolidBrush(Color.FromArgb(base.TextColor.ToArgb()));
			if (TextAlignment == PdfBarcodeTextAlignment.Left)
			{
				num = rectangleF.Left + base.QuietZone.Left;
				num2 = rectangleF.Width;
			}
			else if (TextAlignment == PdfBarcodeTextAlignment.Right)
			{
				num = rectangleF.Left;
				num2 = rectangleF.Width - base.QuietZone.Right;
			}
			else
			{
				num = rectangleF.Left + base.QuietZone.Left;
				num2 = rectangleF.Width - (base.QuietZone.Right + base.QuietZone.Left);
			}
			if (!isFontModified)
			{
				SizeF sizeF2 = Font.MeasureString(base.Text);
				float num8 = Font.Size;
				int num9 = 0;
				while (sizeF2.Width > width)
				{
					Font = new PdfStandardFont(PdfFontFamily.Helvetica, num8 -= 1f);
					sizeF2 = Font.MeasureString(base.Text);
					if (sizeF2.Width <= width)
					{
						Font = new PdfStandardFont(PdfFontFamily.Helvetica, num8);
						break;
					}
					num9++;
				}
			}
			if (textDisplayLocation != TextLocation.Top)
			{
				graphics.DrawString(layoutRectangle: new RectangleF(new PointF(num, y + base.QuietZone.Top + BarcodeToTextGapHeight + base.BarHeight), new SizeF(num2, sizeF.Height)), s: base.Text, font: Font, brush: new PdfSolidBrush(base.TextColor), format: pdfStringFormat);
			}
			else
			{
				RectangleF layoutRectangle2 = new RectangleF(new PointF(num, y + base.QuietZone.Top), new SizeF(num2, sizeF.Height));
				layoutRectangle2.Y = ((layoutRectangle2.Y < 0f) ? 0f : layoutRectangle2.Y);
				graphics.DrawString(base.Text, Font, new PdfSolidBrush(base.TextColor), layoutRectangle2, pdfStringFormat);
			}
		}
		base.QuietZone = quietZone;
		base.Text = text;
	}

	internal void DrawRevamped(PdfPageBase page, float x, float y, float width, float height)
	{
		DrawRevamped(page.Graphics, x, y, width, height);
	}

	internal float PaintToImage(PdfGraphics g, RectangleF barRect)
	{
		g.DrawRectangle(new PdfSolidBrush(base.BarColor), barRect);
		return barRect.Width;
	}

	internal float GetCharBarsCount(char character)
	{
		float num = 0f;
		foreach (KeyValuePair<char, BarcodeSymbolTable> barcodeSymbol in BarcodeSymbols)
		{
			BarcodeSymbolTable value = barcodeSymbol.Value;
			if (value.Symbol != character)
			{
				continue;
			}
			byte[] bars = value.Bars;
			for (int i = 0; i < bars.Length; i++)
			{
				if (!check && bars.Length % 2 != 0)
				{
					continuous = true;
				}
				num += (float)(int)bars[i];
				check = true;
			}
			if (size != SizeF.Empty && bars.Length % 2 != 0)
			{
				barcodeSpaceCount++;
			}
			return num;
		}
		return 0f;
	}

	protected internal override bool Validate(string data)
	{
		codeValidator = new Regex(validatorExpression);
		return codeValidator.Match(data).Success;
	}

	protected internal override SizeF GetSizeValue()
	{
		float num = GetHeight();
		return new SizeF(BarcodeWidth(), num);
	}

	internal float BarcodeWidth()
	{
		string text = base.Text;
		if (EnableCheckDigit && isCheckDigitAdded)
		{
			isCheckDigitAdded = false;
		}
		string empty = string.Empty;
		empty = ((!(barcodeEncodeText != string.Empty)) ? GetTextToEncode() : barcodeEncodeText);
		isCheckDigitAdded = false;
		base.ExtendedText = "";
		base.Text = text;
		if (empty == null || empty.Length == 0)
		{
			throw new BarcodeException("Barcode Text cannot be null or empty.");
		}
		if (encodeStartStopSymbols && startSymbol != 0 && stopSymbol != 0)
		{
			empty = startSymbol + empty + stopSymbol;
		}
		float num = 0f;
		float num2 = base.QuietZone.Left + base.QuietZone.Right;
		string text2 = empty;
		foreach (char character in text2)
		{
			num2 += GetCharWidth(character) + interCharacterGap;
		}
		text2 = empty;
		foreach (char c in text2)
		{
			foreach (KeyValuePair<char, BarcodeSymbolTable> barcodeSymbol in BarcodeSymbols)
			{
				BarcodeSymbolTable value = barcodeSymbol.Value;
				if (value.Symbol == c)
				{
					byte[] bars = value.Bars;
					for (int j = 0; j < bars.Length; j++)
					{
						float num3 = (float)(int)bars[j] * base.NarrowBarWidth;
						num += num3;
					}
					if (bars.Length % 2 != 0)
					{
						num += IntercharacterGap;
					}
				}
			}
		}
		return num + (base.QuietZone.Left + base.QuietZone.Right);
	}

	protected internal virtual void GetExtendedTextValue()
	{
	}

	protected internal virtual char[] CalculateCheckDigit()
	{
		return null;
	}

	internal float GetCharWidth(char character)
	{
		float num = 0f;
		foreach (KeyValuePair<char, BarcodeSymbolTable> barcodeSymbol in BarcodeSymbols)
		{
			BarcodeSymbolTable value = barcodeSymbol.Value;
			if (value.Symbol != character)
			{
				continue;
			}
			byte[] bars = value.Bars;
			for (int i = 0; i < bars.Length; i++)
			{
				if (!check && bars.Length % 2 != 0)
				{
					continuous = true;
				}
				num += (float)(int)bars[i] * base.NarrowBarWidth;
				check = true;
			}
			return num;
		}
		return 0f;
	}

	internal virtual string GetTextToEncode()
	{
		base.Text = base.Text.Replace("[FNC1]", "");
		base.Text = base.Text.Replace("[FNC2]", "ñ");
		base.Text = base.Text.Replace("[FNC3]", "ò");
		if (!Validate(base.Text))
		{
			throw new BarcodeException("Barcode text contains characters that are not accepted by this barcode specification.");
		}
		if (!EnableCheckDigit)
		{
			GetExtendedTextValue();
		}
		char[] array = CalculateCheckDigit();
		string text = (base.ExtendedText.Equals(string.Empty) ? base.Text.Trim('*') : base.ExtendedText.Trim('*'));
		if (isCheckDigitAdded || !EnableCheckDigit)
		{
			return text;
		}
		if (array == null || array.Length == 0)
		{
			return text;
		}
		if (!(this is PdfCodeUpcBarcode))
		{
			if (EnableCheckDigit && array[^1] != 0 && !isCheckDigitAdded)
			{
				char[] array2 = array;
				foreach (char c in array2)
				{
					text += c;
				}
			}
		}
		else if (this is PdfCodeUpcBarcode)
		{
			text = new string(array);
		}
		if (ShowCheckDigit && array[^1] != 0 && !isCheckDigitAdded)
		{
			if (text[text.Length - 1] != array[^1])
			{
				char[] array2 = array;
				foreach (char c2 in array2)
				{
					text += c2;
				}
			}
			isCheckDigitAdded = true;
			if (base.ExtendedText.Equals(string.Empty))
			{
				text = base.Text;
				char[] array2 = array;
				foreach (char c3 in array2)
				{
					text += c3;
				}
			}
			else
			{
				text = base.ExtendedText;
				char[] array2 = array;
				foreach (char c4 in array2)
				{
					text += c4;
				}
			}
		}
		if (ShowCheckDigit)
		{
			char[] array2 = array;
			foreach (char c5 in array2)
			{
				base.Text += c5;
			}
		}
		isCheckDigitAdded = true;
		return text;
	}

	internal virtual List<byte[]> GetTextToEncodeList()
	{
		List<byte[]> list = new List<byte[]>();
		list.Add(new byte[0]);
		return list;
	}

	protected virtual float PaintRectangle(PdfPageBase page, RectangleF barRect)
	{
		return PaintRectangle(page.Graphics, barRect);
	}

	protected virtual float PaintRectangle(PdfGraphics graphics, RectangleF barRect)
	{
		PdfBrush brush = new PdfSolidBrush(Color.FromArgb(base.BarColor.A, base.BarColor.R, base.BarColor.G, base.BarColor.B));
		graphics.DrawRectangle(brush, barRect);
		return barRect.Width;
	}

	protected virtual float PaintRectangleTag(PdfTemplate template, RectangleF barRect)
	{
		PdfBrush brush = new PdfSolidBrush(Color.FromArgb(base.BarColor.A, base.BarColor.R, base.BarColor.G, base.BarColor.B));
		template.Graphics.DrawRectangle(brush, barRect);
		return barRect.Width;
	}

	private float GetHeight()
	{
		float num = base.QuietZone.Top + base.QuietZone.Bottom + base.BarHeight;
		SizeF sizeF = Font.MeasureString(base.Text);
		if (textDisplayLocation == TextLocation.Bottom || textDisplayLocation == TextLocation.Top)
		{
			num += sizeF.Height + BarcodeToTextGapHeight;
		}
		return num;
	}
}
