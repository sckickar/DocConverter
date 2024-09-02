using System.IO;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Barcode;

internal class PdfQRBarcodeHelper
{
	private PdfQRBarcode m_barcode;

	public SizeF Size
	{
		get
		{
			if (m_barcode.Size.IsEmpty)
			{
				return GetBarcodeSize();
			}
			return m_barcode.Size;
		}
		set
		{
			m_barcode.Size = value;
		}
	}

	public QRCodeLogo Logo
	{
		internal get
		{
			return m_barcode.Logo;
		}
		set
		{
			m_barcode.Logo = value;
		}
	}

	internal PdfQRBarcodeHelper(PdfQRBarcode barcode)
	{
		m_barcode = barcode;
	}

	public Stream ToImage(SizeF size)
	{
		bool flag = !(size == SizeF.Empty);
		m_barcode.GenerateValues();
		float xDimension = m_barcode.XDimension;
		PdfBarcodeQuietZones quietZone = m_barcode.QuietZone;
		int noOfModules = m_barcode.noOfModules;
		ModuleValue[,] moduleValue = m_barcode.moduleValue;
		PdfColor backColor = m_barcode.BackColor;
		ModuleValue[,] dataAllocationValues = m_barcode.dataAllocationValues;
		bool isXdimension = m_barcode.isXdimension;
		PdfColor foreColor = m_barcode.ForeColor;
		if (flag)
		{
			m_barcode.Size = new SizeF(size.Width, size.Height);
		}
		float num = xDimension;
		int quiteZone = m_barcode.GetQuiteZone();
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		if (flag)
		{
			num2 = size.Width;
			num3 = size.Height;
			num = ((!(num2 <= num3)) ? (num3 / (float)(noOfModules + 2 * quiteZone)) : (num2 / (float)(noOfModules + 2 * quiteZone)));
		}
		else
		{
			num2 = (float)(noOfModules + 2 * quiteZone) * num;
			num3 = (float)(noOfModules + 2 * quiteZone) * num;
		}
		float num6 = num4 + (float)((!quietZone.IsAll && (int)quietZone.Left > 0) ? ((int)quietZone.Left) : 0);
		float num7 = num5 + (float)((!quietZone.IsAll && (int)quietZone.Top > 0) ? ((int)quietZone.Top) : 0);
		Bitmap bitmap = new Bitmap((int)num2 + (int)num6, (int)num3 + (int)num7);
		float num8 = 0f;
		float num9 = 0f;
		using (GraphicsHelper graphicsHelper = new GraphicsHelper(bitmap))
		{
			graphicsHelper.Clear(Color.White);
			Color color = Color.White;
			Color color2 = Color.Black;
			num5 += (float)((!quietZone.IsAll && (int)quietZone.Top > 0) ? ((int)quietZone.Top) : 0);
			int num10 = noOfModules + 2 * quiteZone;
			int num11 = noOfModules + 2 * quiteZone;
			num8 = num2 / (float)num10;
			num9 = num3 / (float)num11;
			for (int i = 0; i < num10; i++)
			{
				num4 += (float)((!quietZone.IsAll && (int)quietZone.Left > 0) ? ((int)quietZone.Left) : 0);
				for (int j = 0; j < num11; j++)
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
					Color color5 = ((!moduleValue[i, j].IsBlack) ? color : color2);
					if (dataAllocationValues[j, i].IsFilled && dataAllocationValues[j, i].IsBlack)
					{
						color5 = color2;
					}
					if (flag)
					{
						graphicsHelper.FillRectangle(color5, num4, num5, num8, num9);
					}
					else
					{
						graphicsHelper.FillRectangle(color5, num4, num5, num, num);
					}
					num4 = ((!isXdimension) ? ((!flag) ? (num4 + num) : (num4 + num8)) : (num4 + xDimension));
				}
				num5 = ((!isXdimension) ? ((!flag) ? (num5 + num) : (num5 + num9)) : (num5 + xDimension));
				num4 = 0f;
			}
			if (Logo != null)
			{
				float num12 = num2 / 3f;
				float num13 = num3 / 3f;
				float x = num2 / 2f - num12 / 2f;
				float y = num3 / 2f - num13 / 2f;
				if (Logo.Image != null)
				{
					ImageHelper image = new ImageHelper(Logo.Image.ImageStream.Data);
					graphicsHelper.DrawImage(image, x, y, num12, num13);
				}
			}
		}
		MemoryStream memoryStream = new MemoryStream();
		bitmap.Save(memoryStream, ImageFormat.Png);
		memoryStream.Position = 0L;
		return memoryStream;
	}

	private SizeF GetBarcodeSize()
	{
		int num = 2;
		if (m_barcode.QuietZone.IsAll && m_barcode.QuietZone.All > 0f)
		{
			num = (int)m_barcode.QuietZone.All;
		}
		return new SizeF((float)(m_barcode.noOfModules + 2 * num) * m_barcode.XDimension, (float)(m_barcode.noOfModules + 2 * num) * m_barcode.XDimension);
	}
}
