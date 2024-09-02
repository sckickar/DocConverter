using System;
using System.Collections.Generic;
using System.Text;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Barcode;

public class PdfQRBarcode : PdfBidimensionalBarcode
{
	private QRCodeVersion version = QRCodeVersion.Version01;

	internal int noOfModules = 21;

	internal ModuleValue[,] moduleValue;

	internal ModuleValue[,] dataAllocationValues;

	private bool isMixMode;

	private bool mixVersionERC = true;

	private string mixExecutablePart;

	private string mixRemainingPart;

	private int totalBits;

	private int mixDataCount;

	private List<string> text = new List<string>();

	private bool autoTagcheck;

	private List<InputMode> mode = new List<InputMode>();

	internal bool isXdimension;

	private InputMode inputMode;

	private PdfErrorCorrectionLevel errorCorrectionLevel = PdfErrorCorrectionLevel.Low;

	private QRCodeLogo logo;

	private int dataBits;

	private int[] blocks;

	private bool isUserMentionedMode;

	private bool isUserMentionedVersion;

	private bool isUserMentionedErrorCorrectionLevel;

	private bool isEci;

	private int eciAssignmentNumber = 3;

	private PdfQRBarcodeValues qrBarcodeValues;

	private const int dpi = 96;

	private int defaultQuiteZone = 2;

	private bool chooseDefaultMode;

	public QRCodeVersion Version
	{
		get
		{
			return version;
		}
		set
		{
			version = value;
			noOfModules = (int)(version - 1) * 4 + 21;
			if (value != 0)
			{
				isUserMentionedVersion = true;
			}
		}
	}

	public PdfErrorCorrectionLevel ErrorCorrectionLevel
	{
		get
		{
			return errorCorrectionLevel;
		}
		set
		{
			errorCorrectionLevel = value;
			isUserMentionedErrorCorrectionLevel = true;
		}
	}

	public InputMode InputMode
	{
		get
		{
			return inputMode;
		}
		set
		{
			inputMode = value;
			isUserMentionedMode = true;
		}
	}

	public override SizeF Size
	{
		get
		{
			if (base.Size.IsEmpty)
			{
				return GetBarcodeSize();
			}
			return base.Size;
		}
		set
		{
			base.Size = value;
		}
	}

	public QRCodeLogo Logo
	{
		internal get
		{
			return logo;
		}
		set
		{
			logo = value;
			ErrorCorrectionLevel = PdfErrorCorrectionLevel.High;
		}
	}

	public PdfQRBarcode()
	{
		base.XDimension = 1f;
		base.QuietZone.All = 2f;
	}

	public override void Draw(PdfGraphics graphics)
	{
		Draw(graphics, base.Location);
	}

	public override void Draw(PdfGraphics graphics, PointF location)
	{
		GenerateValues();
		int quiteZone = GetQuiteZone();
		PdfBrush pdfBrush = PdfBrushes.Black;
		PdfBrush pdfBrush2 = PdfBrushes.White;
		if (base.BackColor.A != 0)
		{
			Color color = Color.FromArgb(base.BackColor.ToArgb());
			if (color != Color.White)
			{
				pdfBrush2 = new PdfSolidBrush(color);
			}
		}
		if (base.ForeColor.A != 0)
		{
			Color color2 = Color.FromArgb(base.ForeColor.ToArgb());
			if (color2 != Color.Black)
			{
				pdfBrush = new PdfSolidBrush(color2);
			}
		}
		float x = location.X;
		float y = location.Y;
		new PdfUnitConvertor();
		float num = base.XDimension;
		float num2 = 0f;
		float num3 = 0f;
		if (base.Size != SizeF.Empty)
		{
			num2 = Size.Width;
			num3 = Size.Height;
			num = ((!(num2 <= num3)) ? (num3 / (float)(noOfModules + 2 * defaultQuiteZone)) : (num2 / (float)(noOfModules + 2 * defaultQuiteZone)));
		}
		else
		{
			num2 = (float)(noOfModules + 2 * quiteZone) * num;
			num3 = (float)(noOfModules + 2 * quiteZone) * num;
		}
		int num4 = noOfModules + 2 * quiteZone;
		int num5 = noOfModules + 2 * quiteZone;
		y = location.Y + (float)((!base.QuietZone.IsAll && (int)base.QuietZone.Top > 0) ? ((int)base.QuietZone.Top) : 0);
		PdfTemplate pdfTemplate = null;
		if (autoTagcheck)
		{
			pdfTemplate = new PdfTemplate(new SizeF(num2, num3));
		}
		for (int i = 0; i < num4; i++)
		{
			x = location.X + (float)((!base.QuietZone.IsAll && (int)base.QuietZone.Left > 0) ? ((int)base.QuietZone.Left) : 0);
			for (int j = 0; j < num5; j++)
			{
				PdfBrush pdfBrush3 = null;
				if (base.BackColor.A != 0)
				{
					Color color3 = Color.FromArgb(base.BackColor.ToArgb());
					if (color3 != Color.White)
					{
						pdfBrush2 = new PdfSolidBrush(color3);
					}
				}
				if (base.ForeColor.A != 0)
				{
					Color color4 = Color.FromArgb(base.ForeColor.ToArgb());
					if (color4 != Color.Black)
					{
						pdfBrush = new PdfSolidBrush(color4);
					}
				}
				pdfBrush3 = ((!moduleValue[i, j].IsBlack) ? pdfBrush2 : pdfBrush);
				if (dataAllocationValues[j, i].IsFilled && dataAllocationValues[j, i].IsBlack)
				{
					pdfBrush3 = pdfBrush;
				}
				if (autoTagcheck)
				{
					pdfTemplate.Graphics.DrawRectangle(pdfBrush3, x, y, num, num);
				}
				else
				{
					graphics.DrawRectangle(pdfBrush3, x, y, num, num);
				}
				x += num;
			}
			y += num;
		}
		if (autoTagcheck)
		{
			graphics.DrawPdfTemplate(pdfTemplate, location);
		}
		if (Logo != null)
		{
			float num6 = num2 / 3f;
			float num7 = num3 / 3f;
			float x2 = location.X + num2 / 2f - num6 / 2f;
			float y2 = location.Y + num3 / 2f - num7 / 2f;
			PointF point = new PointF(x2, y2);
			SizeF sizeF = new SizeF(num6, num7);
			if (Logo.Image != null)
			{
				graphics.DrawImage(Logo.Image, point, sizeF);
			}
		}
	}

	public void Draw(PdfGraphics graphics, PointF location, SizeF size)
	{
		Draw(graphics, location.X, location.Y, size.Width, size.Height);
	}

	public void Draw(PdfGraphics graphics, RectangleF rectangle)
	{
		Draw(graphics, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
	}

	public void Draw(PdfGraphics graphics, float a, float b, float width, float height)
	{
		GenerateValues();
		int quiteZone = GetQuiteZone();
		PdfBrush pdfBrush = PdfBrushes.Black;
		PdfBrush pdfBrush2 = PdfBrushes.White;
		if (base.BackColor.A != 0)
		{
			Color color = Color.FromArgb(base.BackColor.ToArgb());
			if (color != Color.White)
			{
				pdfBrush2 = new PdfSolidBrush(color);
			}
		}
		if (base.ForeColor.A != 0)
		{
			Color color2 = Color.FromArgb(base.ForeColor.ToArgb());
			if (color2 != Color.Black)
			{
				pdfBrush = new PdfSolidBrush(color2);
			}
		}
		float num = a;
		float num2 = b;
		new PdfUnitConvertor();
		_ = base.XDimension;
		float num3 = 0f;
		float num4 = 0f;
		int num5 = noOfModules + 2 * quiteZone;
		int num6 = noOfModules + 2 * quiteZone;
		num3 = width / (float)(noOfModules + 2 * defaultQuiteZone);
		num4 = height / (float)(noOfModules + 2 * defaultQuiteZone);
		num2 = b + (float)((!base.QuietZone.IsAll && (int)base.QuietZone.Top > 0) ? ((int)base.QuietZone.Top) : 0);
		PdfTemplate pdfTemplate = null;
		if (autoTagcheck)
		{
			pdfTemplate = new PdfTemplate(new SizeF(width, height));
		}
		for (int i = 0; i < num5; i++)
		{
			num = a + (float)((!base.QuietZone.IsAll && (int)base.QuietZone.Left > 0) ? ((int)base.QuietZone.Left) : 0);
			for (int j = 0; j < num6; j++)
			{
				PdfBrush pdfBrush3 = null;
				if (base.BackColor.A != 0)
				{
					Color color3 = Color.FromArgb(base.BackColor.ToArgb());
					if (color3 != Color.White)
					{
						pdfBrush2 = new PdfSolidBrush(color3);
					}
				}
				if (base.ForeColor.A != 0)
				{
					Color color4 = Color.FromArgb(base.ForeColor.ToArgb());
					if (color4 != Color.Black)
					{
						pdfBrush = new PdfSolidBrush(color4);
					}
				}
				pdfBrush3 = ((!moduleValue[i, j].IsBlack) ? pdfBrush2 : pdfBrush);
				if (dataAllocationValues[j, i].IsFilled && dataAllocationValues[j, i].IsBlack)
				{
					pdfBrush3 = pdfBrush;
				}
				if (autoTagcheck)
				{
					pdfTemplate.Graphics.DrawRectangle(pdfBrush3, num, num2, num3, num4);
				}
				else
				{
					graphics.DrawRectangle(pdfBrush3, num, num2, num3, num4);
				}
				num += num3;
			}
			num2 += num4;
		}
		if (autoTagcheck)
		{
			graphics.DrawPdfTemplate(pdfTemplate, new PointF(a, b));
		}
		if (Logo != null)
		{
			float num7 = width / 3f;
			float num8 = height / 3f;
			float x = a + width / 2f - num7 / 2f;
			float y = b + height / 2f - num8 / 2f;
			PointF point = new PointF(x, y);
			SizeF sizeF = new SizeF(num7, num8);
			if (Logo.Image != null)
			{
				graphics.DrawImage(Logo.Image, point, sizeF);
			}
		}
	}

	public override void Draw(PdfPageBase page, PointF location)
	{
		if (page is PdfPage && (page as PdfPage).Document != null)
		{
			autoTagcheck = (page as PdfPage).Document.AutoTag;
		}
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

	public override void Draw(PdfPageBase page)
	{
		Draw(page, base.Location);
	}

	internal void GenerateValues()
	{
		if (inputMode == InputMode.MixingMode)
		{
			isMixMode = true;
			mixVersionERC = false;
		}
		Initialize();
		if (isMixMode)
		{
			int num = 4;
			int num2 = 10;
			int num3 = 9;
			int num4 = 8;
			int num5 = 10;
			int num6 = 11;
			int num7 = 8;
			while (mixExecutablePart != null)
			{
				if (inputMode == InputMode.NumericMode)
				{
					if (mixExecutablePart.Length % 3 == 0)
					{
						totalBits = totalBits + num + num2 + num5 * (mixExecutablePart.Length / 3);
					}
					else if (mixExecutablePart.Length % 3 == 1)
					{
						totalBits = totalBits + num + num2 + num5 * (mixExecutablePart.Length / 3) + 4;
					}
					else if (mixExecutablePart.Length % 3 == 2)
					{
						totalBits = totalBits + num + num2 + num5 * (mixExecutablePart.Length / 3) + 7;
					}
				}
				if (inputMode == InputMode.AlphaNumericMode)
				{
					if (mixExecutablePart.Length % 2 == 0)
					{
						totalBits = totalBits + num + num3 + num6 * (mixExecutablePart.Length / 2);
					}
					else if (mixExecutablePart.Length % 2 == 1)
					{
						totalBits = totalBits + num + num3 + num6 * (mixExecutablePart.Length / 2) + 6;
					}
				}
				if (inputMode == InputMode.BinaryMode)
				{
					totalBits = totalBits + num + num4 + num7 * mixExecutablePart.Length;
				}
				text.Add(mixExecutablePart);
				mode.Add(inputMode);
				if (mixRemainingPart == null)
				{
					base.Text = "";
					mixExecutablePart = null;
					inputMode = InputMode.MixingMode;
					mixVersionERC = true;
				}
				else
				{
					base.Text = mixRemainingPart;
				}
				Initialize();
			}
		}
		qrBarcodeValues = new PdfQRBarcodeValues(version, errorCorrectionLevel);
		moduleValue = new ModuleValue[noOfModules, noOfModules];
		DrawPDP(0, 0);
		DrawPDP(noOfModules - 7, 0);
		DrawPDP(0, noOfModules - 7);
		DrawTimingPattern();
		if (version != QRCodeVersion.Version01)
		{
			int[] alignmentPatternCoOrdinates = GetAlignmentPatternCoOrdinates();
			int[] array = alignmentPatternCoOrdinates;
			foreach (int num8 in array)
			{
				int[] array2 = alignmentPatternCoOrdinates;
				foreach (int num9 in array2)
				{
					if (!moduleValue[num8, num9].IsPDP)
					{
						DrawAlignmentPattern(num8, num9);
					}
				}
			}
		}
		AllocateFormatAndVersionInformation();
		bool[] array3 = null;
		if (isMixMode)
		{
			List<bool> list = new List<bool>();
			mixDataCount = text.Count;
			int num10 = 0;
			foreach (string item2 in text)
			{
				mixDataCount--;
				base.Text = item2;
				inputMode = mode[num10];
				array3 = EncodeData();
				bool[] array4 = array3;
				foreach (bool item in array4)
				{
					list.Add(item);
				}
				num10++;
			}
			array3 = list.ToArray();
		}
		else
		{
			array3 = EncodeData();
		}
		DataAllocationAndMasking(array3);
		DrawFormatInformation();
		AddQuietZone();
	}

	private void AddQuietZone()
	{
		int quiteZone = GetQuiteZone();
		int num = noOfModules + 2 * quiteZone;
		int num2 = noOfModules + 2 * quiteZone;
		ModuleValue[,] array = new ModuleValue[num, num2];
		ModuleValue[,] array2 = new ModuleValue[num, num2];
		for (int i = 0; i < num2; i++)
		{
			array[0, i] = default(ModuleValue);
			array[0, i].IsBlack = false;
			array[0, i].IsFilled = false;
			array[0, i].IsPDP = false;
			array2[0, i] = default(ModuleValue);
			array2[0, i].IsBlack = false;
			array2[0, i].IsFilled = false;
			array2[0, i].IsPDP = false;
		}
		for (int j = quiteZone; j < num - quiteZone; j++)
		{
			array[j, 0] = default(ModuleValue);
			array[j, 0].IsBlack = false;
			array[j, 0].IsFilled = false;
			array[j, 0].IsPDP = false;
			array2[j, 0] = default(ModuleValue);
			array2[j, 0].IsBlack = false;
			array2[j, 0].IsFilled = false;
			array2[j, 0].IsPDP = false;
			for (int k = quiteZone; k < num2 - quiteZone; k++)
			{
				array[j, k] = moduleValue[j - quiteZone, k - quiteZone];
				array2[j, k] = dataAllocationValues[j - quiteZone, k - quiteZone];
			}
			array[j, num2 - quiteZone] = default(ModuleValue);
			array[j, num2 - quiteZone].IsBlack = false;
			array[j, num2 - quiteZone].IsFilled = false;
			array[j, num2 - quiteZone].IsPDP = false;
			array2[j, num2 - quiteZone] = default(ModuleValue);
			array2[j, num2 - quiteZone].IsBlack = false;
			array2[j, num2 - quiteZone].IsFilled = false;
			array2[j, num2 - quiteZone].IsPDP = false;
		}
		for (int l = 0; l < num2; l++)
		{
			array[num - quiteZone, l] = default(ModuleValue);
			array[num - quiteZone, l].IsBlack = false;
			array[num - quiteZone, l].IsFilled = false;
			array[num - quiteZone, l].IsPDP = false;
			array2[num - quiteZone, l] = default(ModuleValue);
			array2[num - quiteZone, l].IsBlack = false;
			array2[num - quiteZone, l].IsFilled = false;
			array2[num - quiteZone, l].IsPDP = false;
		}
		moduleValue = array;
		dataAllocationValues = array2;
	}

	private void DrawPDP(int x, int y)
	{
		int num = x;
		int num2 = y;
		while (num < x + 7)
		{
			moduleValue[num, y].IsBlack = true;
			moduleValue[num, y].IsFilled = true;
			moduleValue[num, y].IsPDP = true;
			moduleValue[num, y + 6].IsBlack = true;
			moduleValue[num, y + 6].IsFilled = true;
			moduleValue[num, y + 6].IsPDP = true;
			if (y + 7 < noOfModules)
			{
				moduleValue[num, y + 7].IsBlack = false;
				moduleValue[num, y + 7].IsFilled = true;
				moduleValue[num, y + 7].IsPDP = true;
			}
			else if (y - 1 >= 0)
			{
				moduleValue[num, y - 1].IsBlack = false;
				moduleValue[num, y - 1].IsFilled = true;
				moduleValue[num, y - 1].IsPDP = true;
			}
			moduleValue[x, num2].IsBlack = true;
			moduleValue[x, num2].IsFilled = true;
			moduleValue[x, num2].IsPDP = true;
			moduleValue[x + 6, num2].IsBlack = true;
			moduleValue[x + 6, num2].IsFilled = true;
			moduleValue[x + 6, num2].IsPDP = true;
			if (x + 7 < noOfModules)
			{
				moduleValue[x + 7, num2].IsBlack = false;
				moduleValue[x + 7, num2].IsFilled = true;
				moduleValue[x + 7, num2].IsPDP = true;
			}
			else if (x - 1 >= 0)
			{
				moduleValue[x - 1, num2].IsBlack = false;
				moduleValue[x - 1, num2].IsFilled = true;
				moduleValue[x - 1, num2].IsPDP = true;
			}
			num++;
			num2++;
		}
		if (x + 7 < noOfModules && y + 7 < noOfModules)
		{
			moduleValue[x + 7, y + 7].IsBlack = false;
			moduleValue[x + 7, y + 7].IsFilled = true;
			moduleValue[x + 7, y + 7].IsPDP = true;
		}
		else if (x + 7 < noOfModules && y + 7 >= noOfModules)
		{
			moduleValue[x + 7, y - 1].IsBlack = false;
			moduleValue[x + 7, y - 1].IsFilled = true;
			moduleValue[x + 7, y - 1].IsPDP = true;
		}
		else if (x + 7 >= noOfModules && y + 7 < noOfModules)
		{
			moduleValue[x - 1, y + 7].IsBlack = false;
			moduleValue[x - 1, y + 7].IsFilled = true;
			moduleValue[x - 1, y + 7].IsPDP = true;
		}
		x++;
		y++;
		num = x;
		num2 = y;
		while (num < x + 5)
		{
			moduleValue[num, y].IsBlack = false;
			moduleValue[num, y].IsFilled = true;
			moduleValue[num, y].IsPDP = true;
			moduleValue[num, y + 4].IsBlack = false;
			moduleValue[num, y + 4].IsFilled = true;
			moduleValue[num, y + 4].IsPDP = true;
			moduleValue[x, num2].IsBlack = false;
			moduleValue[x, num2].IsFilled = true;
			moduleValue[x, num2].IsPDP = true;
			moduleValue[x + 4, num2].IsBlack = false;
			moduleValue[x + 4, num2].IsFilled = true;
			moduleValue[x + 4, num2].IsPDP = true;
			num++;
			num2++;
		}
		x++;
		y++;
		num = x;
		num2 = y;
		while (num < x + 3)
		{
			moduleValue[num, y].IsBlack = true;
			moduleValue[num, y].IsFilled = true;
			moduleValue[num, y].IsPDP = true;
			moduleValue[num, y + 2].IsBlack = true;
			moduleValue[num, y + 2].IsFilled = true;
			moduleValue[num, y + 2].IsPDP = true;
			moduleValue[x, num2].IsBlack = true;
			moduleValue[x, num2].IsFilled = true;
			moduleValue[x, num2].IsPDP = true;
			moduleValue[x + 2, num2].IsBlack = true;
			moduleValue[x + 2, num2].IsFilled = true;
			moduleValue[x + 2, num2].IsPDP = true;
			num++;
			num2++;
		}
		moduleValue[x + 1, y + 1].IsBlack = true;
		moduleValue[x + 1, y + 1].IsFilled = true;
		moduleValue[x + 1, y + 1].IsPDP = true;
	}

	private void DrawTimingPattern()
	{
		for (int i = 8; i < noOfModules - 8; i += 2)
		{
			moduleValue[i, 6].IsBlack = true;
			moduleValue[i, 6].IsFilled = true;
			moduleValue[i + 1, 6].IsBlack = false;
			moduleValue[i + 1, 6].IsFilled = true;
			moduleValue[6, i].IsBlack = true;
			moduleValue[6, i].IsFilled = true;
			moduleValue[6, i + 1].IsBlack = false;
			moduleValue[6, i + 1].IsFilled = true;
		}
		moduleValue[noOfModules - 8, 8].IsBlack = true;
		moduleValue[noOfModules - 8, 8].IsFilled = true;
	}

	private void DrawAlignmentPattern(int x, int y)
	{
		int num = x - 2;
		int num2 = y - 2;
		while (num < x + 3)
		{
			moduleValue[num, y - 2].IsBlack = true;
			moduleValue[num, y - 2].IsFilled = true;
			moduleValue[num, y + 2].IsBlack = true;
			moduleValue[num, y + 2].IsFilled = true;
			moduleValue[x - 2, num2].IsBlack = true;
			moduleValue[x - 2, num2].IsFilled = true;
			moduleValue[x + 2, num2].IsBlack = true;
			moduleValue[x + 2, num2].IsFilled = true;
			num++;
			num2++;
		}
		num = x - 1;
		num2 = y - 1;
		while (num < x + 2)
		{
			moduleValue[num, y - 1].IsBlack = false;
			moduleValue[num, y - 1].IsFilled = true;
			moduleValue[num, y + 1].IsBlack = false;
			moduleValue[num, y + 1].IsFilled = true;
			moduleValue[x - 1, num2].IsBlack = false;
			moduleValue[x - 1, num2].IsFilled = true;
			moduleValue[x + 1, num2].IsBlack = false;
			moduleValue[x + 1, num2].IsFilled = true;
			num++;
			num2++;
		}
		moduleValue[x, y].IsBlack = true;
		moduleValue[x, y].IsFilled = true;
	}

	private bool[] EncodeData()
	{
		List<bool> list = new List<bool>();
		switch (inputMode)
		{
		case InputMode.NumericMode:
			list.Add(item: false);
			list.Add(item: false);
			list.Add(item: false);
			list.Add(item: true);
			break;
		case InputMode.AlphaNumericMode:
			list.Add(item: false);
			list.Add(item: false);
			list.Add(item: true);
			list.Add(item: false);
			break;
		case InputMode.BinaryMode:
			if (isEci)
			{
				list.Add(item: false);
				list.Add(item: true);
				list.Add(item: true);
				list.Add(item: true);
				bool[] array = StringToBoolArray(eciAssignmentNumber.ToString(), 8);
				foreach (bool item in array)
				{
					list.Add(item);
				}
			}
			list.Add(item: false);
			list.Add(item: true);
			list.Add(item: false);
			list.Add(item: false);
			break;
		}
		int num = 0;
		if (version < QRCodeVersion.Version10)
		{
			switch (inputMode)
			{
			case InputMode.NumericMode:
				num = 10;
				break;
			case InputMode.AlphaNumericMode:
				num = 9;
				break;
			case InputMode.BinaryMode:
				num = 8;
				break;
			}
		}
		else if (version < QRCodeVersion.Version27)
		{
			switch (inputMode)
			{
			case InputMode.NumericMode:
				num = 12;
				break;
			case InputMode.AlphaNumericMode:
				num = 11;
				break;
			case InputMode.BinaryMode:
				num = 16;
				break;
			}
		}
		else
		{
			switch (inputMode)
			{
			case InputMode.NumericMode:
				num = 14;
				break;
			case InputMode.AlphaNumericMode:
				num = 13;
				break;
			case InputMode.BinaryMode:
				num = 16;
				break;
			}
		}
		byte[] bytes = Encoding.UTF8.GetBytes(base.Text);
		bool[] array2 = IntToBoolArray(bytes.Length, num);
		for (int j = 0; j < num; j++)
		{
			list.Add(array2[j]);
		}
		if (inputMode == InputMode.NumericMode)
		{
			char[] array3 = base.Text.ToCharArray();
			string text = "";
			for (int k = 0; k < array3.Length; k++)
			{
				text += array3[k];
				if ((k % 3 == 2 && k != 0) || k == array3.Length - 1)
				{
					bool[] array4 = ((text.ToString().Length == 3) ? StringToBoolArray(text, 10) : ((text.ToString().Length != 2) ? StringToBoolArray(text, 4) : StringToBoolArray(text, 7)));
					text = "";
					bool[] array = array4;
					foreach (bool item2 in array)
					{
						list.Add(item2);
					}
				}
			}
		}
		else if (inputMode == InputMode.AlphaNumericMode)
		{
			char[] array5 = base.Text.ToCharArray();
			string text2 = "";
			int num2 = 0;
			for (int l = 0; l < array5.Length; l++)
			{
				text2 += array5[l];
				if (l % 2 == 0 && l + 1 != array5.Length)
				{
					num2 = 45 * qrBarcodeValues.GetAlphanumericvalues(array5[l]);
				}
				if (l % 2 == 1 && l != 0)
				{
					num2 += qrBarcodeValues.GetAlphanumericvalues(array5[l]);
					bool[] array6 = IntToBoolArray(num2, 11);
					num2 = 0;
					bool[] array = array6;
					foreach (bool item3 in array)
					{
						list.Add(item3);
					}
					text2 = null;
				}
				if (l != 1 && text2 != null && l + 1 == array5.Length && text2.Length == 1)
				{
					num2 = qrBarcodeValues.GetAlphanumericvalues(array5[l]);
					bool[] array7 = IntToBoolArray(num2, 6);
					num2 = 0;
					bool[] array = array7;
					foreach (bool item4 in array)
					{
						list.Add(item4);
					}
				}
			}
		}
		else if (inputMode == InputMode.BinaryMode)
		{
			char[] array8 = base.Text.ToCharArray();
			for (int m = 0; m < array8.Length; m++)
			{
				int number = 0;
				bool flag = false;
				if ((base.Text[m] >= ' ' && base.Text[m] <= '~') || (base.Text[m] < '\u0080' && base.Text[m] > 'ÿ') || base.Text[m] == '\n' || base.Text[m] == '\r')
				{
					number = array8[m];
				}
				else if (base.Text[m] >= '｡' && base.Text[m] <= 'ﾟ')
				{
					number = array8[m] - 65216;
				}
				else if (base.Text[m] >= 'Ё' && base.Text[m] <= 'џ')
				{
					number = array8[m] - 864;
				}
				else if (base.Text[m] >= 'Ą' && base.Text[m] <= 'ž')
				{
					number = Encoding.GetEncoding("ISO-8859-2").GetBytes(base.Text[m].ToString())[0];
				}
				else if (base.Text[m] < 'd')
				{
					number = Encoding.GetEncoding("ISO-8859-1").GetBytes(base.Text[m].ToString())[0];
				}
				else
				{
					base.Text[m].ToString();
					flag = true;
					byte[] bytes2 = Encoding.UTF8.GetBytes(base.Text[m].ToString());
					for (int n = 0; n < bytes2.Length; n++)
					{
						bool[] array = IntToBoolArray(bytes2[n], 8);
						foreach (bool item5 in array)
						{
							list.Add(item5);
						}
					}
				}
				if (!flag)
				{
					bool[] array = IntToBoolArray(number, 8);
					foreach (bool item6 in array)
					{
						list.Add(item6);
					}
				}
			}
		}
		if ((isMixMode && mixDataCount == 0) || !isMixMode)
		{
			for (int num3 = 0; num3 < 4; num3++)
			{
				if (list.Count / 8 == qrBarcodeValues.NumberOfDataCodeWord)
				{
					break;
				}
				list.Add(item: false);
			}
			while (list.Count % 8 != 0)
			{
				list.Add(item: false);
			}
			while (list.Count / 8 != qrBarcodeValues.NumberOfDataCodeWord)
			{
				list.Add(item: true);
				list.Add(item: true);
				list.Add(item: true);
				list.Add(item: false);
				list.Add(item: true);
				list.Add(item: true);
				list.Add(item: false);
				list.Add(item: false);
				if (list.Count / 8 == qrBarcodeValues.NumberOfDataCodeWord)
				{
					break;
				}
				list.Add(item: false);
				list.Add(item: false);
				list.Add(item: false);
				list.Add(item: true);
				list.Add(item: false);
				list.Add(item: false);
				list.Add(item: false);
				list.Add(item: true);
			}
			dataBits = qrBarcodeValues.NumberOfDataCodeWord;
			blocks = qrBarcodeValues.NumberOfErrorCorrectionBlocks;
			int num4 = blocks[0];
			if (blocks.Length == 6)
			{
				num4 = blocks[0] + blocks[3];
			}
			string[][] array9 = new string[num4][];
			List<bool> list2 = list;
			if (blocks.Length == 6)
			{
				int num5 = blocks[0] * blocks[2] * 8;
				list2 = new List<bool>();
				for (int num6 = 0; num6 < num5; num6++)
				{
					list2.Add(list[num6]);
				}
			}
			string[,] array10 = new string[blocks[0], list2.Count / 8 / blocks[0]];
			array10 = CreateBlocks(list2, blocks[0]);
			for (int num7 = 0; num7 < blocks[0]; num7++)
			{
				array9[num7] = SplitCodeWord(array10, num7, list2.Count / 8 / blocks[0]);
			}
			if (blocks.Length == 6)
			{
				list2 = new List<bool>();
				for (int num8 = blocks[0] * blocks[2] * 8; num8 < list.Count; num8++)
				{
					list2.Add(list[num8]);
				}
				string[,] array11 = new string[blocks[0], list2.Count / 8 / blocks[3]];
				array11 = CreateBlocks(list2, blocks[3]);
				int num9 = blocks[0];
				int num10 = 0;
				for (; num9 < num4; num9++)
				{
					array9[num9] = SplitCodeWord(array11, num10++, list2.Count / 8 / blocks[3]);
				}
			}
			list = null;
			list = new List<bool>();
			for (int num11 = 0; num11 < 125; num11++)
			{
				for (int num12 = 0; num12 < num4; num12++)
				{
					for (int num13 = 0; num13 < 8; num13++)
					{
						if (num11 < array9[num12].Length)
						{
							list.Add(array9[num12][num11][num13] == '1');
						}
					}
				}
			}
			PdfErrorCorrectionCodewords pdfErrorCorrectionCodewords = new PdfErrorCorrectionCodewords(version, errorCorrectionLevel);
			dataBits = qrBarcodeValues.NumberOfDataCodeWord;
			int numberOfErrorCorrectingCodeWords = qrBarcodeValues.NumberOfErrorCorrectingCodeWords;
			blocks = qrBarcodeValues.NumberOfErrorCorrectionBlocks;
			if (blocks.Length == 6)
			{
				pdfErrorCorrectionCodewords.DataBits = (dataBits - blocks[3] * blocks[5]) / blocks[0];
			}
			else
			{
				pdfErrorCorrectionCodewords.DataBits = dataBits / blocks[0];
			}
			pdfErrorCorrectionCodewords.ECCW = numberOfErrorCorrectingCodeWords / num4;
			string[][] array12 = new string[num4][];
			int num14 = 0;
			for (int num15 = 0; num15 < blocks[0]; num15++)
			{
				pdfErrorCorrectionCodewords.DC = array9[num14];
				array12[num14++] = pdfErrorCorrectionCodewords.GetERCW();
			}
			if (blocks.Length == 6)
			{
				pdfErrorCorrectionCodewords.DataBits = (dataBits - blocks[0] * blocks[2]) / blocks[3];
				for (int num16 = 0; num16 < blocks[3]; num16++)
				{
					pdfErrorCorrectionCodewords.DC = array9[num14];
					array12[num14++] = pdfErrorCorrectionCodewords.GetERCW();
				}
			}
			if (blocks.Length != 6)
			{
				for (int num17 = 0; num17 < array12[0].Length; num17++)
				{
					for (int num18 = 0; num18 < blocks[0]; num18++)
					{
						for (int num19 = 0; num19 < 8; num19++)
						{
							if (num17 < array12[num18].Length)
							{
								list.Add(array12[num18][num17][num19] == '1');
							}
						}
					}
				}
			}
			else
			{
				for (int num20 = 0; num20 < array12[0].Length; num20++)
				{
					for (int num21 = 0; num21 < num4; num21++)
					{
						for (int num22 = 0; num22 < 8; num22++)
						{
							if (num20 < array12[num21].Length)
							{
								list.Add(array12[num21][num20][num22] == '1');
							}
						}
					}
				}
			}
		}
		return list.ToArray();
	}

	private void DataAllocationAndMasking(bool[] data)
	{
		dataAllocationValues = new ModuleValue[noOfModules, noOfModules];
		int num = 0;
		int num2;
		for (num2 = noOfModules - 1; num2 >= 0; num2 -= 2)
		{
			for (int num3 = noOfModules - 1; num3 >= 0; num3--)
			{
				if (!moduleValue[num2, num3].IsFilled || !moduleValue[num2 - 1, num3].IsFilled)
				{
					if (!moduleValue[num2, num3].IsFilled)
					{
						if (num + 1 < data.Length)
						{
							dataAllocationValues[num2, num3].IsBlack = data[num++];
						}
						if ((num2 + num3) % 3 == 0)
						{
							if (dataAllocationValues[num2, num3].IsBlack)
							{
								dataAllocationValues[num2, num3].IsBlack = true;
							}
							else
							{
								dataAllocationValues[num2, num3].IsBlack = false;
							}
						}
						else if (dataAllocationValues[num2, num3].IsBlack)
						{
							dataAllocationValues[num2, num3].IsBlack = false;
						}
						else
						{
							dataAllocationValues[num2, num3].IsBlack = true;
						}
						dataAllocationValues[num2, num3].IsFilled = true;
					}
					if (!moduleValue[num2 - 1, num3].IsFilled)
					{
						if (num + 1 < data.Length)
						{
							dataAllocationValues[num2 - 1, num3].IsBlack = data[num++];
						}
						if ((num2 - 1 + num3) % 3 == 0)
						{
							if (dataAllocationValues[num2 - 1, num3].IsBlack)
							{
								dataAllocationValues[num2 - 1, num3].IsBlack = true;
							}
							else
							{
								dataAllocationValues[num2 - 1, num3].IsBlack = false;
							}
						}
						else if (dataAllocationValues[num2 - 1, num3].IsBlack)
						{
							dataAllocationValues[num2 - 1, num3].IsBlack = false;
						}
						else
						{
							dataAllocationValues[num2 - 1, num3].IsBlack = true;
						}
						dataAllocationValues[num2 - 1, num3].IsFilled = true;
					}
				}
			}
			num2 -= 2;
			if (num2 == 6)
			{
				num2--;
			}
			for (int i = 0; i < noOfModules; i++)
			{
				if (moduleValue[num2, i].IsFilled && moduleValue[num2 - 1, i].IsFilled)
				{
					continue;
				}
				if (!moduleValue[num2, i].IsFilled)
				{
					if (num + 1 < data.Length)
					{
						dataAllocationValues[num2, i].IsBlack = data[num++];
					}
					if ((num2 + i) % 3 == 0)
					{
						if (dataAllocationValues[num2, i].IsBlack)
						{
							dataAllocationValues[num2, i].IsBlack = true;
						}
						else
						{
							dataAllocationValues[num2, i].IsBlack = false;
						}
					}
					else if (dataAllocationValues[num2, i].IsBlack)
					{
						dataAllocationValues[num2, i].IsBlack = false;
					}
					else
					{
						dataAllocationValues[num2, i].IsBlack = true;
					}
					dataAllocationValues[num2, i].IsFilled = true;
				}
				if (moduleValue[num2 - 1, i].IsFilled)
				{
					continue;
				}
				if (num + 1 < data.Length)
				{
					dataAllocationValues[num2 - 1, i].IsBlack = data[num++];
				}
				if ((num2 - 1 + i) % 3 == 0)
				{
					if (dataAllocationValues[num2 - 1, i].IsBlack)
					{
						dataAllocationValues[num2 - 1, i].IsBlack = true;
					}
					else
					{
						dataAllocationValues[num2 - 1, i].IsBlack = false;
					}
				}
				else if (dataAllocationValues[num2 - 1, i].IsBlack)
				{
					dataAllocationValues[num2 - 1, i].IsBlack = false;
				}
				else
				{
					dataAllocationValues[num2 - 1, i].IsBlack = true;
				}
				dataAllocationValues[num2 - 1, i].IsFilled = true;
			}
		}
		for (int j = 0; j < noOfModules; j++)
		{
			for (int k = 0; k < noOfModules; k++)
			{
				if (!moduleValue[j, k].IsFilled)
				{
					if (dataAllocationValues[j, k].IsBlack)
					{
						dataAllocationValues[j, k].IsBlack = false;
					}
					else
					{
						dataAllocationValues[j, k].IsBlack = true;
					}
				}
			}
		}
	}

	private void DrawFormatInformation()
	{
		byte[] formatInformation = qrBarcodeValues.FormatInformation;
		int num = 0;
		for (int i = 0; i < 7; i++)
		{
			if (i == 6)
			{
				moduleValue[i + 1, 8].IsBlack = formatInformation[num] == 1;
			}
			else
			{
				moduleValue[i, 8].IsBlack = formatInformation[num] == 1;
			}
			moduleValue[8, noOfModules - i - 1].IsBlack = formatInformation[num++] == 1;
		}
		num = 14;
		for (int j = 0; j < 7; j++)
		{
			if (j == 6)
			{
				moduleValue[8, j + 1].IsBlack = formatInformation[num] == 1;
			}
			else
			{
				moduleValue[8, j].IsBlack = formatInformation[num] == 1;
			}
			moduleValue[noOfModules - j - 1, 8].IsBlack = formatInformation[num--] == 1;
		}
		moduleValue[8, 8].IsBlack = formatInformation[7] == 1;
		moduleValue[8, noOfModules - 8].IsBlack = formatInformation[7] == 1;
	}

	private SizeF GetBarcodeSize()
	{
		int num = 2;
		if (base.QuietZone.IsAll && base.QuietZone.All > 0f)
		{
			num = (int)base.QuietZone.All;
		}
		return new SizeF((float)(noOfModules + 2 * num) * base.XDimension, (float)(noOfModules + 2 * num) * base.XDimension);
	}

	private void Initialize()
	{
		bool flag = false;
		if (!isUserMentionedMode)
		{
			chooseDefaultMode = true;
			InputMode inputMode = InputMode.NumericMode;
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			for (int i = 0; i < base.Text.Length; i++)
			{
				if (base.Text[i] < ':' && base.Text[i] > '/')
				{
					if (!isMixMode)
					{
						continue;
					}
					num2++;
					num++;
					if (num >= 7 && inputMode == InputMode.BinaryMode)
					{
						num = 0;
						num2 = 0;
						break;
					}
					if ((inputMode == InputMode.NumericMode || (i == base.Text.Length - 1 && num2 < 10)) && inputMode != InputMode.BinaryMode)
					{
						mixExecutablePart = base.Text.Substring(0, i + 1);
						if (base.Text.Length != 1 && i != base.Text.Length - 1)
						{
							mixRemainingPart = base.Text.Substring(i + 1, base.Text.Length - (i + 1));
						}
						else
						{
							mixRemainingPart = null;
						}
					}
					continue;
				}
				if ((base.Text[i] < '[' && base.Text[i] > '@') || base.Text[i] == '$' || base.Text[i] == '%' || base.Text[i] == '*' || base.Text[i] == '+' || base.Text[i] == '-' || base.Text[i] == '.' || base.Text[i] == '/' || base.Text[i] == ':' || base.Text[i] == ' ')
				{
					if (isMixMode)
					{
						if (num2 >= 10)
						{
							num2 = 0;
							num = 0;
							break;
						}
						num2 = 0;
						num++;
						if (inputMode != InputMode.BinaryMode)
						{
							mixExecutablePart = base.Text.Substring(0, i + 1);
							if (base.Text.Length != 1 && i != base.Text.Length - 1)
							{
								mixRemainingPart = base.Text.Substring(i + 1, base.Text.Length - (i + 1));
							}
							else
							{
								mixRemainingPart = null;
							}
							inputMode = InputMode.AlphaNumericMode;
						}
					}
					else
					{
						inputMode = InputMode.AlphaNumericMode;
					}
					continue;
				}
				if ((base.Text[i] >= '｡' && base.Text[i] <= 'ﾟ') || (base.Text[i] >= 'a' && base.Text[i] <= 'z'))
				{
					if (isMixMode)
					{
						if (num2 >= 5)
						{
							num2 = 0;
							break;
						}
						if (num >= 7)
						{
							num = 0;
							mixExecutablePart = base.Text.Substring(0, i);
							if (base.Text.Length != 0 && i != base.Text.Length)
							{
								mixRemainingPart = base.Text.Substring(i, base.Text.Length - i);
							}
							else
							{
								mixRemainingPart = null;
							}
							break;
						}
						num2 = 0;
						num = 0;
						num3++;
						mixExecutablePart = base.Text.Substring(0, i + 1);
						if (base.Text.Length != 1 && i != base.Text.Length - 1)
						{
							mixRemainingPart = base.Text.Substring(i + 1, base.Text.Length - (i + 1));
						}
						else
						{
							mixRemainingPart = null;
						}
					}
					inputMode = InputMode.BinaryMode;
					if (!isMixMode)
					{
						break;
					}
					continue;
				}
				if (isMixMode)
				{
					if (num2 >= 5)
					{
						num2 = 0;
						break;
					}
					if (num >= 7)
					{
						num = 0;
						mixExecutablePart = base.Text.Substring(0, i);
						if (base.Text.Length != 0 && i != base.Text.Length)
						{
							mixRemainingPart = base.Text.Substring(i, base.Text.Length - i);
						}
						else
						{
							mixRemainingPart = null;
						}
						break;
					}
					num2 = 0;
					num = 0;
					num3++;
					mixExecutablePart = base.Text.Substring(0, i + 1);
					if (base.Text.Length != 1 && i != base.Text.Length - 1)
					{
						mixRemainingPart = base.Text.Substring(i + 1, base.Text.Length - (i + 1));
					}
					else
					{
						mixRemainingPart = null;
					}
				}
				inputMode = InputMode.BinaryMode;
				if (!isMixMode)
				{
					break;
				}
			}
			if (isUserMentionedMode && !isMixMode && inputMode != this.inputMode && (((inputMode == InputMode.AlphaNumericMode || inputMode == InputMode.BinaryMode) && this.inputMode == InputMode.NumericMode) || (inputMode == InputMode.BinaryMode && this.inputMode == InputMode.AlphaNumericMode)))
			{
				throw new BarcodeException("Mode Conflict: Default mode that supports your data is :" + inputMode);
			}
			InputMode = inputMode;
		}
		if (isEci)
		{
			for (int j = 0; j < base.Text.Length; j++)
			{
				if (base.Text[j] < ' ' || base.Text[j] > 'ÿ')
				{
					if (IsCP437Character(base.Text[j]))
					{
						eciAssignmentNumber = 2;
						break;
					}
					if (IsISO8859_2Character(base.Text[j]))
					{
						eciAssignmentNumber = 4;
						break;
					}
					if (IsISO8859_3Character(base.Text[j]))
					{
						eciAssignmentNumber = 5;
						break;
					}
					if (IsISO8859_4Character(base.Text[j]))
					{
						eciAssignmentNumber = 6;
						break;
					}
					if (IsISO8859_5Character(base.Text[j]))
					{
						eciAssignmentNumber = 7;
						break;
					}
					if (IsISO8859_6Character(base.Text[j]))
					{
						eciAssignmentNumber = 8;
						break;
					}
					if (IsISO8859_7Character(base.Text[j]))
					{
						eciAssignmentNumber = 9;
						break;
					}
					if (IsISO8859_8Character(base.Text[j]))
					{
						eciAssignmentNumber = 10;
						break;
					}
					if (IsISO8859_11Character(base.Text[j]))
					{
						eciAssignmentNumber = 13;
						break;
					}
					if (IsWindows1250Character(base.Text[j]))
					{
						eciAssignmentNumber = 21;
						break;
					}
					if (IsWindows1251Character(base.Text[j]))
					{
						eciAssignmentNumber = 22;
						break;
					}
					if (IsWindows1252Character(base.Text[j]))
					{
						eciAssignmentNumber = 23;
						break;
					}
					if (IsWindows1256Character(base.Text[j]))
					{
						eciAssignmentNumber = 24;
						break;
					}
				}
			}
		}
		if (!mixVersionERC)
		{
			return;
		}
		if (isMixMode)
		{
			this.inputMode = InputMode.MixingMode;
		}
		if (!isUserMentionedVersion || version == QRCodeVersion.Auto)
		{
			int[] array = null;
			if (isUserMentionedErrorCorrectionLevel)
			{
				switch (this.inputMode)
				{
				case InputMode.NumericMode:
					switch ((int)errorCorrectionLevel)
					{
					case 7:
						array = PdfQRBarcodeValues.numericDataCapacityLow;
						break;
					case 15:
						array = PdfQRBarcodeValues.numericDataCapacityMedium;
						break;
					case 25:
						array = PdfQRBarcodeValues.numericDataCapacityQuartile;
						break;
					case 30:
						array = PdfQRBarcodeValues.numericDataCapacityHigh;
						break;
					}
					break;
				case InputMode.AlphaNumericMode:
					switch ((int)errorCorrectionLevel)
					{
					case 7:
						array = PdfQRBarcodeValues.alphanumericDataCapacityLow;
						break;
					case 15:
						array = PdfQRBarcodeValues.alphanumericDataCapacityMedium;
						break;
					case 25:
						array = PdfQRBarcodeValues.alphanumericDataCapacityQuartile;
						break;
					case 30:
						array = PdfQRBarcodeValues.alphanumericDataCapacityHigh;
						break;
					}
					break;
				case InputMode.BinaryMode:
					switch ((int)errorCorrectionLevel)
					{
					case 7:
						array = PdfQRBarcodeValues.binaryDataCapacityLow;
						break;
					case 15:
						array = PdfQRBarcodeValues.binaryDataCapacityMedium;
						break;
					case 25:
						array = PdfQRBarcodeValues.binaryDataCapacityQuartile;
						break;
					case 30:
						array = PdfQRBarcodeValues.binaryDataCapacityHigh;
						break;
					}
					break;
				case InputMode.MixingMode:
					switch ((int)errorCorrectionLevel)
					{
					case 7:
						array = PdfQRBarcodeValues.mixedDataCapacityLow;
						break;
					case 15:
						array = PdfQRBarcodeValues.mixedDataCapacityMedium;
						break;
					case 25:
						array = PdfQRBarcodeValues.mixedDataCapacityQuartile;
						break;
					case 30:
						array = PdfQRBarcodeValues.mixedDataCapacityHigh;
						break;
					}
					break;
				}
			}
			else
			{
				errorCorrectionLevel = PdfErrorCorrectionLevel.Medium;
				if (Encoding.UTF8.GetByteCount(base.Text) != base.Text.Length)
				{
					flag = true;
				}
				switch (this.inputMode)
				{
				case InputMode.NumericMode:
					array = PdfQRBarcodeValues.numericDataCapacityMedium;
					break;
				case InputMode.AlphaNumericMode:
					array = PdfQRBarcodeValues.alphanumericDataCapacityMedium;
					break;
				case InputMode.BinaryMode:
					array = PdfQRBarcodeValues.binaryDataCapacityMedium;
					break;
				case InputMode.MixingMode:
					array = PdfQRBarcodeValues.mixedDataCapacityMedium;
					break;
				}
			}
			byte[] bytes = Encoding.UTF8.GetBytes(base.Text);
			int k;
			if (!isMixMode)
			{
				for (k = 0; k < array.Length && array[k] < bytes.Length; k++)
				{
				}
			}
			else
			{
				for (k = 0; k < array.Length && array[k] <= totalBits; k++)
				{
				}
			}
			Version = (QRCodeVersion)(k + 1);
			if (Version > QRCodeVersion.Version40)
			{
				throw new BarcodeException("Text length is greater than the data capacity of error correction level");
			}
		}
		if (isUserMentionedErrorCorrectionLevel)
		{
			int num4 = 0;
			if (this.inputMode == InputMode.AlphaNumericMode)
			{
				num4 = PdfQRBarcodeValues.GetAlphanumericDataCapacity(version, errorCorrectionLevel);
			}
			else if (this.inputMode == InputMode.NumericMode)
			{
				num4 = PdfQRBarcodeValues.GetNumericDataCapacity(version, errorCorrectionLevel);
			}
			if (this.inputMode == InputMode.BinaryMode)
			{
				num4 = PdfQRBarcodeValues.GetBinaryDataCapacity(version, errorCorrectionLevel);
			}
			if (this.inputMode == InputMode.MixingMode)
			{
				num4 = PdfQRBarcodeValues.GetMixedDataCapacity(version, errorCorrectionLevel);
			}
			if (num4 < base.Text.Length && this.inputMode != InputMode.MixingMode)
			{
				if (!chooseDefaultMode)
				{
					throw new BarcodeException("Text length is greater than the version capacity");
				}
				this.inputMode = InputMode.MixingMode;
				isMixMode = true;
				mixVersionERC = false;
				Initialize();
			}
			else if (num4 < totalBits && this.inputMode == InputMode.MixingMode)
			{
				throw new BarcodeException("Text length is greater than the version capacity");
			}
		}
		else
		{
			if (flag)
			{
				return;
			}
			int num5 = 0;
			int num6 = 0;
			int num7 = 0;
			int num8 = 0;
			if (this.inputMode == InputMode.AlphaNumericMode)
			{
				num5 = PdfQRBarcodeValues.GetAlphanumericDataCapacity(version, PdfErrorCorrectionLevel.Low);
				num6 = PdfQRBarcodeValues.GetAlphanumericDataCapacity(version, PdfErrorCorrectionLevel.Medium);
				num7 = PdfQRBarcodeValues.GetAlphanumericDataCapacity(version, PdfErrorCorrectionLevel.Quartile);
				num8 = PdfQRBarcodeValues.GetAlphanumericDataCapacity(version, PdfErrorCorrectionLevel.High);
			}
			else if (this.inputMode == InputMode.NumericMode)
			{
				num5 = PdfQRBarcodeValues.GetNumericDataCapacity(version, PdfErrorCorrectionLevel.Low);
				num6 = PdfQRBarcodeValues.GetNumericDataCapacity(version, PdfErrorCorrectionLevel.Medium);
				num7 = PdfQRBarcodeValues.GetNumericDataCapacity(version, PdfErrorCorrectionLevel.Quartile);
				num8 = PdfQRBarcodeValues.GetNumericDataCapacity(version, PdfErrorCorrectionLevel.High);
			}
			else if (this.inputMode == InputMode.BinaryMode)
			{
				num5 = PdfQRBarcodeValues.GetBinaryDataCapacity(version, PdfErrorCorrectionLevel.Low);
				num6 = PdfQRBarcodeValues.GetBinaryDataCapacity(version, PdfErrorCorrectionLevel.Medium);
				num7 = PdfQRBarcodeValues.GetBinaryDataCapacity(version, PdfErrorCorrectionLevel.Quartile);
				num8 = PdfQRBarcodeValues.GetBinaryDataCapacity(version, PdfErrorCorrectionLevel.High);
			}
			else if (this.inputMode == InputMode.MixingMode)
			{
				num5 = PdfQRBarcodeValues.GetMixedDataCapacity(version, PdfErrorCorrectionLevel.Low);
				num6 = PdfQRBarcodeValues.GetMixedDataCapacity(version, PdfErrorCorrectionLevel.Medium);
				num7 = PdfQRBarcodeValues.GetMixedDataCapacity(version, PdfErrorCorrectionLevel.Quartile);
				num8 = PdfQRBarcodeValues.GetMixedDataCapacity(version, PdfErrorCorrectionLevel.High);
			}
			if (num8 > base.Text.Length)
			{
				errorCorrectionLevel = PdfErrorCorrectionLevel.High;
				return;
			}
			if (num7 > base.Text.Length)
			{
				errorCorrectionLevel = PdfErrorCorrectionLevel.Quartile;
				return;
			}
			if (num6 > base.Text.Length)
			{
				errorCorrectionLevel = PdfErrorCorrectionLevel.Medium;
				return;
			}
			if (num5 <= base.Text.Length)
			{
				throw new BarcodeException("Text length is greater than the version capacity");
			}
			errorCorrectionLevel = PdfErrorCorrectionLevel.Low;
		}
	}

	private string[] SplitCodeWord(string[,] encodeData, int block, int count)
	{
		string[] array = new string[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = encodeData[block, i];
		}
		return array;
	}

	private string[,] CreateBlocks(List<bool> encodeData, int noOfBlocks)
	{
		string[,] array = new string[noOfBlocks, encodeData.Count / 8 / noOfBlocks];
		string text = null;
		int i = 0;
		int num = 0;
		int num2 = 0;
		for (; i < encodeData.Count; i++)
		{
			if (i % 8 == 0 && i != 0)
			{
				array[num2, num] = text;
				text = null;
				num++;
				if (num == encodeData.Count / noOfBlocks / 8)
				{
					num2++;
					num = 0;
				}
			}
			text += (encodeData[i] ? 1 : 0);
		}
		array[num2, num] = text;
		return array;
	}

	private bool[] IntToBoolArray(int number, int noOfBits)
	{
		bool[] array = new bool[noOfBits];
		for (int i = 0; i < noOfBits; i++)
		{
			array[noOfBits - i - 1] = ((number >> i) & 1) == 1;
		}
		return array;
	}

	private bool[] StringToBoolArray(string numberInString, int noOfBits)
	{
		bool[] array = new bool[noOfBits];
		char[] array2 = numberInString.ToCharArray();
		int num = 0;
		for (int i = 0; i < array2.Length; i++)
		{
			num = num * 10 + array2[i] - 48;
		}
		for (int j = 0; j < noOfBits; j++)
		{
			array[noOfBits - j - 1] = ((num >> j) & 1) == 1;
		}
		return array;
	}

	private int[] GetAlignmentPatternCoOrdinates()
	{
		int[] result = null;
		switch ((int)version)
		{
		case 2:
			result = new int[2] { 6, 18 };
			break;
		case 3:
			result = new int[2] { 6, 22 };
			break;
		case 4:
			result = new int[2] { 6, 26 };
			break;
		case 5:
			result = new int[2] { 6, 30 };
			break;
		case 6:
			result = new int[2] { 6, 34 };
			break;
		case 7:
			result = new int[3] { 6, 22, 38 };
			break;
		case 8:
			result = new int[3] { 6, 24, 42 };
			break;
		case 9:
			result = new int[3] { 6, 26, 46 };
			break;
		case 10:
			result = new int[3] { 6, 28, 50 };
			break;
		case 11:
			result = new int[3] { 6, 30, 54 };
			break;
		case 12:
			result = new int[3] { 6, 32, 58 };
			break;
		case 13:
			result = new int[3] { 6, 34, 62 };
			break;
		case 14:
			result = new int[4] { 6, 26, 46, 66 };
			break;
		case 15:
			result = new int[4] { 6, 26, 48, 70 };
			break;
		case 16:
			result = new int[4] { 6, 26, 50, 74 };
			break;
		case 17:
			result = new int[4] { 6, 30, 54, 78 };
			break;
		case 18:
			result = new int[4] { 6, 30, 56, 82 };
			break;
		case 19:
			result = new int[4] { 6, 30, 58, 86 };
			break;
		case 20:
			result = new int[4] { 6, 34, 62, 90 };
			break;
		case 21:
			result = new int[5] { 6, 28, 50, 72, 94 };
			break;
		case 22:
			result = new int[5] { 6, 26, 50, 74, 98 };
			break;
		case 23:
			result = new int[5] { 6, 30, 54, 78, 102 };
			break;
		case 24:
			result = new int[5] { 6, 28, 54, 80, 106 };
			break;
		case 25:
			result = new int[5] { 6, 32, 58, 84, 110 };
			break;
		case 26:
			result = new int[5] { 6, 30, 58, 86, 114 };
			break;
		case 27:
			result = new int[5] { 6, 34, 62, 90, 118 };
			break;
		case 28:
			result = new int[6] { 6, 26, 50, 74, 98, 122 };
			break;
		case 29:
			result = new int[6] { 6, 30, 54, 78, 102, 126 };
			break;
		case 30:
			result = new int[6] { 6, 26, 52, 78, 104, 130 };
			break;
		case 31:
			result = new int[6] { 6, 30, 56, 82, 108, 134 };
			break;
		case 32:
			result = new int[6] { 6, 34, 60, 86, 112, 138 };
			break;
		case 33:
			result = new int[6] { 6, 30, 58, 86, 114, 142 };
			break;
		case 34:
			result = new int[6] { 6, 34, 62, 90, 118, 146 };
			break;
		case 35:
			result = new int[7] { 6, 30, 54, 78, 102, 126, 150 };
			break;
		case 36:
			result = new int[7] { 6, 24, 50, 76, 102, 128, 154 };
			break;
		case 37:
			result = new int[7] { 6, 28, 54, 80, 106, 132, 158 };
			break;
		case 38:
			result = new int[7] { 6, 32, 58, 84, 110, 136, 162 };
			break;
		case 39:
			result = new int[7] { 6, 26, 54, 82, 110, 138, 166 };
			break;
		case 40:
			result = new int[7] { 6, 30, 58, 86, 114, 142, 170 };
			break;
		}
		return result;
	}

	private void AllocateFormatAndVersionInformation()
	{
		for (int i = 0; i < 9; i++)
		{
			moduleValue[8, i].IsFilled = true;
			moduleValue[i, 8].IsFilled = true;
		}
		for (int j = noOfModules - 8; j < noOfModules; j++)
		{
			moduleValue[8, j].IsFilled = true;
			moduleValue[j, 8].IsFilled = true;
		}
		if (version <= QRCodeVersion.Version06)
		{
			return;
		}
		byte[] versionInformation = qrBarcodeValues.VersionInformation;
		int num = 0;
		for (int k = 0; k < 6; k++)
		{
			for (int num2 = 2; num2 >= 0; num2--)
			{
				moduleValue[k, noOfModules - 9 - num2].IsBlack = versionInformation[num] == 1;
				moduleValue[k, noOfModules - 9 - num2].IsFilled = true;
				moduleValue[noOfModules - 9 - num2, k].IsBlack = versionInformation[num++] == 1;
				moduleValue[noOfModules - 9 - num2, k].IsFilled = true;
			}
		}
	}

	internal int GetQuiteZone()
	{
		int result = 2;
		if (base.QuietZone.IsAll && base.QuietZone.All > 0f)
		{
			result = (int)base.QuietZone.All;
		}
		return result;
	}

	private bool IsCP437Character(char inputChar)
	{
		int num = inputChar;
		string value = num.ToString("X");
		if (Array.IndexOf(new string[49]
		{
			"2591", "2592", "2593", "2502", "2524", "2561", "2562", "2556", "2555", "2563",
			"2551", "2557", "255D", "255C", "255B", "2510", "2514", "2534", "252C", "251C",
			"2500", "253C", "255E", "255F", "255A", "2554", "2569", "2566", "2560", "2550",
			"256C", "2567", "2568", "2564", "2565", "2559", "2558", "2552", "2553", "256B",
			"256A", "2518", "250C", "2588", "2584", "258C", "2590", "2580", "25A0"
		}, value) > -1)
		{
			return true;
		}
		return false;
	}

	private bool IsISO8859_2Character(char inputChar)
	{
		int num = inputChar;
		string value = num.ToString("X");
		if (Array.IndexOf(new string[57]
		{
			"104", "2D8", "141", "13D", "15A", "160", "15E", "164", "179", "17D",
			"17B", "105", "2DB", "142", "13E", "15B", "2C7", "161", "15F", "165",
			"17A", "2DD", "17E", "17C", "154", "102", "139", "106", "10C", "118",
			"11A", "10E", "110", "143", "147", "150", "158", "16E", "170", "162",
			"155", "103", "13A", "107", "10D", "119", "11B", "10F", "111", "144",
			"148", "151", "159", "16F", "171", "163", "2D9"
		}, value) > -1)
		{
			return true;
		}
		return false;
	}

	private bool IsISO8859_3Character(char inputChar)
	{
		int num = inputChar;
		string value = num.ToString("X");
		if (Array.IndexOf(new string[26]
		{
			"126", "124", "130", "15E", "11E", "134", "17B", "127", "125", "131",
			"15F", "11F", "135", "17C", "10A", "108", "120", "11C", "16C", "15C",
			"10B", "109", "121", "11D", "16D", "15D"
		}, value) > -1)
		{
			return true;
		}
		return false;
	}

	private bool IsISO8859_4Character(char inputChar)
	{
		int num = inputChar;
		string value = num.ToString("X");
		if (Array.IndexOf(new string[49]
		{
			"104", "138", "156", "128", "13B", "160", "112", "122", "166", "17D",
			"105", "2DB", "157", "129", "13C", "2C7", "161", "113", "123", "167",
			"14A", "17E", "14B", "100", "12E", "10C", "118", "116", "12A", "110",
			"145", "14C", "136", "172", "168", "16A", "101", "12F", "10D", "119",
			"117", "12B", "111", "146", "14D", "137", "173", "169", "16B"
		}, value) > -1)
		{
			return true;
		}
		return false;
	}

	private bool IsISO8859_5Character(char inputChar)
	{
		if (inputChar >= 'Ё' && inputChar <= 'џ' && inputChar != 'Ѝ' && inputChar != 'ѐ' && inputChar != 'ѝ')
		{
			return true;
		}
		return false;
	}

	private bool IsISO8859_6Character(char inputChar)
	{
		if ((inputChar >= 'ء' && inputChar <= 'غ') || (inputChar >= 'ـ' && inputChar <= '\u0652') || inputChar == '؟' || inputChar == '؛' || inputChar == '،')
		{
			return true;
		}
		return false;
	}

	private bool IsISO8859_7Character(char inputChar)
	{
		if ((inputChar >= '\u0384' && inputChar <= 'ώ') || inputChar == 'ͺ')
		{
			return true;
		}
		return false;
	}

	private bool IsISO8859_8Character(char inputChar)
	{
		if (inputChar >= 'א' && inputChar <= 'ת')
		{
			return true;
		}
		return false;
	}

	private bool IsISO8859_11Character(char inputChar)
	{
		if (inputChar >= 'ก' && inputChar <= '๛')
		{
			return true;
		}
		return false;
	}

	private bool IsWindows1250Character(char inputChar)
	{
		int num = inputChar;
		string value = num.ToString("X");
		if (Array.IndexOf(new string[10] { "141", "104", "15E", "17B", "142", "105", "15F", "13D", "13E", "17C" }, value) > -1)
		{
			return true;
		}
		if (inputChar >= 'ء' && inputChar <= 'ي')
		{
			return true;
		}
		return false;
	}

	private bool IsWindows1251Character(char inputChar)
	{
		int num = inputChar;
		string value = num.ToString("X");
		if (Array.IndexOf(new string[30]
		{
			"402", "403", "453", "409", "40A", "40C", "40B", "40F", "452", "459",
			"45A", "45C", "45B", "45F", "40E", "45E", "408", "490", "401", "404",
			"407", "406", "456", "491", "451", "454", "458", "405", "455", "457"
		}, value) > -1)
		{
			return true;
		}
		if (inputChar >= 'А' && inputChar <= 'я')
		{
			return true;
		}
		return false;
	}

	private bool IsWindows1252Character(char inputChar)
	{
		int num = inputChar;
		string value = num.ToString("X");
		if (Array.IndexOf(new string[27]
		{
			"20AC", "201A", "192", "201E", "2026", "2020", "2021", "2C6", "2030", "160",
			"2039", "152", "17D", "2018", "2019", "201C", "201D", "2022", "2013", "2014",
			"2DC", "2122", "161", "203A", "153", "17E", "178"
		}, value) > -1)
		{
			return true;
		}
		return false;
	}

	private bool IsWindows1256Character(char inputChar)
	{
		int num = inputChar;
		string value = num.ToString("X");
		if (Array.IndexOf(new string[13]
		{
			"67E", "679", "152", "686", "698", "688", "6AF", "6A9", "691", "153",
			"6BA", "6BE", "6C1"
		}, value) > -1)
		{
			return true;
		}
		if (inputChar >= 'ء' && inputChar <= 'ي')
		{
			return true;
		}
		return false;
	}
}
