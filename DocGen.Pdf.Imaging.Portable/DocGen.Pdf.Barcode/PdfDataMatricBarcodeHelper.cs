using System.IO;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Barcode;

internal class PdfDataMatricBarcodeHelper
{
	private PdfDataMatrixBarcode m_barcode;

	internal PdfDataMatricBarcodeHelper(PdfDataMatrixBarcode barcode)
	{
		m_barcode = barcode;
	}

	private PdfDataMatrixSize FindDataMatrixSize(int width, int height)
	{
		switch (width)
		{
		case 8:
			switch (height)
			{
			case 18:
				return PdfDataMatrixSize.Size8x18;
			case 32:
				return PdfDataMatrixSize.Size8x32;
			}
			break;
		case 10:
			return PdfDataMatrixSize.Size10x10;
		case 14:
			return PdfDataMatrixSize.Size14x14;
		case 12:
			switch (height)
			{
			case 26:
				return PdfDataMatrixSize.Size12x26;
			case 36:
				return PdfDataMatrixSize.Size12x36;
			case 12:
				return PdfDataMatrixSize.Size12x12;
			}
			break;
		case 16:
			switch (height)
			{
			case 16:
				return PdfDataMatrixSize.Size16x16;
			case 36:
				return PdfDataMatrixSize.Size16x36;
			case 48:
				return PdfDataMatrixSize.Size16x48;
			}
			break;
		case 18:
			return PdfDataMatrixSize.Size18x18;
		case 20:
			return PdfDataMatrixSize.Size20x20;
		case 22:
			return PdfDataMatrixSize.Size22x22;
		case 24:
			return PdfDataMatrixSize.Size24x24;
		case 26:
			return PdfDataMatrixSize.Size26x26;
		case 32:
			return PdfDataMatrixSize.Size32x32;
		case 36:
			return PdfDataMatrixSize.Size36x36;
		case 40:
			return PdfDataMatrixSize.Size40x40;
		case 44:
			return PdfDataMatrixSize.Size44x44;
		case 48:
			return PdfDataMatrixSize.Size48x48;
		case 52:
			return PdfDataMatrixSize.Size52x52;
		case 64:
			return PdfDataMatrixSize.Size64x64;
		case 72:
			return PdfDataMatrixSize.Size72x72;
		case 80:
			return PdfDataMatrixSize.Size80x80;
		case 88:
			return PdfDataMatrixSize.Size88x88;
		case 96:
			return PdfDataMatrixSize.Size96x96;
		case 104:
			return PdfDataMatrixSize.Size104x104;
		case 120:
			return PdfDataMatrixSize.Size120x120;
		case 132:
			return PdfDataMatrixSize.Size132x132;
		case 144:
			return PdfDataMatrixSize.Size144x144;
		}
		return PdfDataMatrixSize.Auto;
	}

	public Stream ToImage(SizeF size)
	{
		bool flag = !(size == SizeF.Empty);
		m_barcode.BuildDataMatrix();
		float xDimension = m_barcode.XDimension;
		byte[,] dataMatrixArray = m_barcode.dataMatrixArray;
		int actualRows = m_barcode.ActualRows;
		int actualColumns = m_barcode.ActualColumns;
		PdfColor backColor = m_barcode.BackColor;
		PdfBarcodeQuietZones quietZone = m_barcode.QuietZone;
		PdfColor foreColor = m_barcode.ForeColor;
		int num = (int)new PdfUnitConvertor().ConvertToPixels(xDimension, PdfGraphicsUnit.Point);
		float num2 = 0f;
		float num3 = 0f;
		int num4 = 0;
		int num5 = 0;
		Bitmap bitmap = null;
		float num6 = 0f;
		float num7 = 0f;
		if (!quietZone.IsAll && (int)quietZone.Left > 0)
		{
			_ = quietZone.Left;
		}
		if (!quietZone.IsAll && (int)quietZone.Top > 0)
		{
			_ = quietZone.Top;
		}
		if (m_barcode != null)
		{
			((PdfBidimensionalBarcode)m_barcode).Size = new SizeF(actualColumns, actualRows);
		}
		m_barcode.Size = FindDataMatrixSize(actualColumns, actualRows);
		if (flag)
		{
			bitmap = new Bitmap((int)size.Width, (int)size.Height);
		}
		else
		{
			int width = actualColumns * num;
			int height = actualRows * num;
			bitmap = new Bitmap(width, height);
		}
		using (GraphicsHelper graphicsHelper = new GraphicsHelper(bitmap))
		{
			graphicsHelper.Clear(Color.White);
			Color color = Color.White;
			Color color2 = Color.Black;
			int num8 = actualRows;
			int num9 = actualColumns;
			float num10 = 0f;
			if (flag)
			{
				num6 = size.Width / (float)actualColumns;
				num7 = size.Height / (float)num8;
				graphicsHelper.FillRectangle(Color.White, 0f, 0f, size.Width, size.Height);
				float num11 = 0f;
				float num12 = 0f;
				if (num7 > num6)
				{
					num7 = num6;
					num11 = num6 * (float)actualColumns;
					num12 = num7 * (float)num8;
					num10 = size.Width / 2f - num11 / 2f;
					num3 = size.Height / 2f - num12 / 2f;
				}
				else if (num6 > num7)
				{
					num6 = num7;
					num11 = num6 * (float)actualColumns;
					num12 = num7 * (float)num8;
					num10 = size.Width / 2f - num11 / 2f;
					num3 = size.Height / 2f - num12 / 2f;
				}
			}
			num3 += (float)((!quietZone.IsAll && (int)quietZone.Top > 0) ? ((int)quietZone.Top) : 0);
			for (int i = 0; i < num8; i++)
			{
				num2 = num10 + (float)((!quietZone.IsAll && (int)quietZone.Left > 0) ? ((int)quietZone.Left) : 0);
				num4 = 0;
				num4 = 0;
				for (int j = 0; j < num9; j++)
				{
					if (backColor.A != 0)
					{
						Color color3 = Color.FromArgb(backColor.ToArgb());
						if (color3 != Color.White)
						{
							color = color3;
						}
					}
					if (foreColor.A != 0)
					{
						Color color4 = Color.FromArgb(foreColor.ToArgb());
						if (color4 != Color.Black)
						{
							color2 = color4;
						}
					}
					Color color5 = ((dataMatrixArray[i, j] != 1) ? color : color2);
					if (flag)
					{
						graphicsHelper.FillRectangle(color5, num2, num3, num6, num7);
						num2 += num6;
					}
					else
					{
						graphicsHelper.FillRectangle(color5, num4, num5, num, num);
						num4 += num;
					}
				}
				if (flag)
				{
					num3 += num7;
				}
				else
				{
					num5 += num;
				}
				num2 = 0f;
			}
		}
		MemoryStream memoryStream = new MemoryStream();
		bitmap.Save(memoryStream, ImageFormat.Png);
		memoryStream.Position = 0L;
		return memoryStream;
	}
}
