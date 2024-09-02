using System;
using System.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Graphics.Images.Decoder;

internal class BmpDecoder : ImageDecoder
{
	private const int c_fileHeaderSize = 14;

	private const int c_BitmapHeaderSize = 40;

	private const int c_rMask = 31744;

	private const int c_gMask = 992;

	private const int c_bMask = 31;

	private byte[] m_BmpHeader = new byte[2] { 66, 77 };

	private PdfStream m_imageStream;

	private BitmapFileHeader m_fileHeader;

	private BitmapInfoHeader m_infoHeader;

	private byte[] m_palette;

	private byte[] m_csPalette;

	public BmpDecoder(Stream stream)
	{
		base.InternalStream = stream;
		base.Format = ImageType.Bmp;
		Initialize();
	}

	public static float[] GetImageResolution(Stream stream)
	{
		float[] obj = new float[2] { 96f, 96f };
		byte[] array = new byte[40];
		stream.Position = 14L;
		stream.Read(array, 0, array.Length);
		int num = BitConverter.ToInt32(array, 24);
		int num2 = BitConverter.ToInt32(array, 28);
		obj[0] = (float)Math.Round((float)num * 0.0254f, 2);
		obj[1] = (float)Math.Round((float)num2 * 0.0254f, 2);
		return obj;
	}

	protected override void Initialize()
	{
		base.InternalStream.Position = 0L;
		byte[] array = new byte[base.InternalStream.Length];
		base.InternalStream.Read(array, 0, array.Length);
		base.ImageData = array;
		ReadImage();
	}

	private void ReadImage()
	{
		ReadFileHeader();
		ReadInfoHeader();
		ReadImageData();
	}

	internal override PdfStream GetImageDictionary()
	{
		m_imageStream = new PdfStream();
		m_imageStream.InternalStream = new MemoryStream(base.ImageData);
		m_imageStream.Compress = false;
		m_imageStream["Type"] = new PdfName("XObject");
		m_imageStream["Subtype"] = new PdfName("Image");
		m_imageStream["Width"] = new PdfNumber(base.Width);
		m_imageStream["Height"] = new PdfNumber(base.Height);
		m_imageStream["BitsPerComponent"] = new PdfNumber(base.BitsPerComponent);
		m_imageStream["ColorSpace"] = GetColorSpace();
		return m_imageStream;
	}

	private void ReadFileHeader()
	{
		byte[] array = new byte[14];
		m_fileHeader = default(BitmapFileHeader);
		base.InternalStream.Position = 0L;
		base.InternalStream.Read(array, 0, array.Length);
		m_fileHeader.FileSize = BitConverter.ToInt32(array, 2);
		m_fileHeader.OffSet = BitConverter.ToInt32(array, 10);
	}

	private void ReadInfoHeader()
	{
		byte[] array = new byte[40];
		m_infoHeader = default(BitmapInfoHeader);
		base.InternalStream.Read(array, 0, array.Length);
		m_infoHeader.Size = BitConverter.ToInt32(array, 0);
		m_infoHeader.Width = BitConverter.ToInt32(array, 4);
		m_infoHeader.Height = BitConverter.ToInt32(array, 8);
		m_infoHeader.Planes = BitConverter.ToInt16(array, 12);
		m_infoHeader.BitsPerPixel = BitConverter.ToInt16(array, 14);
		m_infoHeader.Compression = (BitmapCompression)BitConverter.ToInt32(array, 16);
		m_infoHeader.SizeImage = BitConverter.ToInt32(array, 20);
		m_infoHeader.XPelsPerMeter = BitConverter.ToInt32(array, 24);
		m_infoHeader.YPelsPerMeter = BitConverter.ToInt32(array, 28);
		m_infoHeader.ClrUsed = BitConverter.ToInt32(array, 32);
		m_infoHeader.ClrImportant = BitConverter.ToInt32(array, 36);
		base.Height = m_infoHeader.Height;
		base.Width = m_infoHeader.Width;
		base.BitsPerComponent = m_infoHeader.BitsPerPixel;
	}

	private void ReadImageData()
	{
		int num = -1;
		if (m_infoHeader.ClrUsed == 0)
		{
			if (m_infoHeader.BitsPerPixel == 1 || m_infoHeader.BitsPerPixel == 4 || m_infoHeader.BitsPerPixel == 8)
			{
				num = (int)Math.Pow(2.0, m_infoHeader.BitsPerPixel) * 4;
			}
		}
		else
		{
			num = m_infoHeader.ClrUsed * 4;
		}
		if (num > 0)
		{
			m_palette = new byte[num];
			base.InternalStream.Read(m_palette, 0, num);
		}
		switch (m_infoHeader.Compression)
		{
		case BitmapCompression.RGB:
			DecodeRGBImage();
			break;
		case BitmapCompression.RunlengthEncoding8:
		case BitmapCompression.RunlengthEncoding4:
		case BitmapCompression.Bitfield:
			throw new NotImplementedException("The specified image format is not supported.");
		}
	}

	private void DecodeRGBImage()
	{
		switch (m_infoHeader.BitsPerPixel)
		{
		case 32:
			Decode32BitRGB();
			break;
		case 24:
			Decode24BitRGB();
			break;
		case 16:
			Decode16BitRGB();
			break;
		case 8:
			Decode8bitRGB();
			break;
		case 4:
			Decode4BitRGB();
			break;
		case 1:
			Decode1BitRGB();
			break;
		}
	}

	private bool CheckIfValidBmp()
	{
		if (base.InternalStream.ReadByte() == m_BmpHeader[0])
		{
			return base.InternalStream.ReadByte() == m_BmpHeader[1];
		}
		return false;
	}

	private PdfArray GetColorSpace()
	{
		PdfArray pdfArray = new PdfArray();
		if (m_infoHeader.ClrUsed > 0)
		{
			pdfArray.Add(new PdfName("Indexed"));
			pdfArray.Add(new PdfName("DeviceRGB"));
			pdfArray.Add(new PdfNumber(m_csPalette.Length / 3 - 1));
			pdfArray.Add(new PdfString(m_csPalette));
		}
		else
		{
			pdfArray.Add(new PdfName("DeviceRGB"));
		}
		return pdfArray;
	}

	private void Decode1BitRGB()
	{
		base.ImageData = new byte[(m_infoHeader.Width + 7) / 8 * m_infoHeader.Height];
		int num = 0;
		int num2 = (int)Math.Ceiling((double)m_infoHeader.Width / 8.0);
		int num3 = num2 % 4;
		if (num3 != 0)
		{
			num = 4 - num3;
		}
		int num4 = (num2 + num) * m_infoHeader.Height;
		byte[] array = new byte[num4];
		for (int i = 0; i < num4; i += base.InternalStream.Read(array, i, num4 - i))
		{
		}
		if (m_infoHeader.Height > 0)
		{
			for (int j = 0; j < m_infoHeader.Height; j++)
			{
				Array.Copy(array, num4 - (j + 1) * (num2 + num), base.ImageData, j * num2, num2);
			}
		}
		else
		{
			for (int k = 0; k < m_infoHeader.Height; k++)
			{
				Array.Copy(array, k * (num2 + num), base.ImageData, k * num2, num2);
			}
		}
		SetColorSpacePalette(4);
	}

	private void Decode4BitRGB()
	{
		base.ImageData = new byte[(m_infoHeader.Width + 1) / 2 * m_infoHeader.Height];
		int num = 0;
		int num2 = (int)Math.Ceiling((double)m_infoHeader.Width / 2.0);
		int num3 = num2 % 4;
		if (num3 != 0)
		{
			num = 4 - num3;
		}
		int num4 = (num2 + num) * m_infoHeader.Height;
		byte[] array = new byte[num4];
		base.InternalStream.Read(array, 0, num4);
		if (m_infoHeader.Height > 0)
		{
			for (int i = 0; i < m_infoHeader.Height; i++)
			{
				Array.Copy(array, num4 - (i + 1) * (num2 + num), base.ImageData, i * num2, num2);
			}
		}
		else
		{
			for (int j = 0; j < m_infoHeader.Height; j++)
			{
				Array.Copy(array, j * (num2 + num), base.ImageData, j * num2, num2);
			}
		}
		SetColorSpacePalette(4);
	}

	private void Decode8bitRGB()
	{
		base.ImageData = new byte[m_infoHeader.Width * m_infoHeader.Height];
		int num = 0;
		int num2 = m_infoHeader.Width * 8;
		if (num2 % 32 != 0)
		{
			num = (num2 / 32 + 1) * 32 - num2;
			num = (int)Math.Ceiling((double)num / 8.0);
		}
		int num3 = (m_infoHeader.Width + num) * m_infoHeader.Height;
		byte[] array = new byte[num3];
		for (int i = 0; i < num3; i += base.InternalStream.Read(array, i, num3 - i))
		{
		}
		if (m_infoHeader.Height > 0)
		{
			for (int j = 0; j < m_infoHeader.Height; j++)
			{
				Array.Copy(array, num3 - (j + 1) * (m_infoHeader.Width + num), base.ImageData, j * m_infoHeader.Width, m_infoHeader.Width);
			}
		}
		else
		{
			for (int k = 0; k < m_infoHeader.Height; k++)
			{
				Array.Copy(array, k * (m_infoHeader.Width + num), base.ImageData, k * m_infoHeader.Width, m_infoHeader.Width);
			}
		}
		SetColorSpacePalette(4);
	}

	private void Decode16BitRGB()
	{
		int num = 8;
		int num2 = 4;
		int alignment = 0;
		byte[] imageArray = GetImageArray(m_infoHeader.Width, m_infoHeader.Height, 2, ref alignment);
		for (int i = 0; i < m_infoHeader.Height; i++)
		{
			int num3 = i * (m_infoHeader.Width * 2 + alignment);
			int num4 = Invert(i, m_infoHeader.Height);
			for (int j = 0; j < m_infoHeader.Width; j++)
			{
				int startIndex = num3 + j * 2;
				short num5 = BitConverter.ToInt16(imageArray, startIndex);
				byte b = (byte)(((num5 & 0x7C00) >> 11) * num);
				byte b2 = (byte)(((num5 & 0x3E0) >> 5) * num2);
				byte b3 = (byte)((num5 & 0x1F) * num);
				int num6 = (num4 * m_infoHeader.Width + j) * 4;
				base.ImageData[num6] = b;
				base.ImageData[num6 + 1] = b2;
				base.ImageData[num6 + 2] = b3;
				base.ImageData[num6 + 3] = byte.MaxValue;
			}
		}
	}

	private void Decode24BitRGB()
	{
		base.ImageData = new byte[m_infoHeader.Width * m_infoHeader.Height * 3];
		base.BitsPerComponent = 8;
		int num = 0;
		int num2 = m_infoHeader.Width * 24;
		if (num2 % 32 != 0)
		{
			num = (num2 / 32 + 1) * 32 - num2;
			num = (int)Math.Ceiling((double)num / 8.0);
		}
		byte[] array = new byte[(m_infoHeader.Width * 3 + 3) / 4 * 4 * m_infoHeader.Height];
		base.InternalStream.Read(array, 0, array.Length);
		int num3 = 0;
		if (m_infoHeader.Height > 0)
		{
			int num4 = m_infoHeader.Width * m_infoHeader.Height * 3 - 1;
			int num5 = -num;
			for (int i = 0; i < m_infoHeader.Height; i++)
			{
				num3 = num4 - (i + 1) * m_infoHeader.Width * 3 + 1;
				num5 += num;
				for (int j = 0; j < m_infoHeader.Width; j++)
				{
					base.ImageData[num3 + 2] = array[num5++];
					base.ImageData[num3 + 1] = array[num5++];
					base.ImageData[num3] = array[num5++];
					num3 += 3;
				}
			}
		}
		else
		{
			int num5 = -num;
			for (int k = 0; k < m_infoHeader.Height; k++)
			{
				num5 += num;
				for (int l = 0; l < m_infoHeader.Width; l++)
				{
					base.ImageData[num3 + 2] = array[num5++];
					base.ImageData[num3 + 1] = array[num5++];
					base.ImageData[num3] = array[num5++];
					num3 += 3;
				}
			}
		}
		SetColorSpacePalette(4);
	}

	private void Decode32BitRGB()
	{
		int alignment = 0;
		byte[] imageArray = GetImageArray(m_infoHeader.Width, m_infoHeader.Height, 4, ref alignment);
		for (int i = 0; i < m_infoHeader.Height; i++)
		{
			int num = i * (m_infoHeader.Width * 4 + alignment);
			int num2 = Invert(i, m_infoHeader.Height);
			for (int j = 0; j < m_infoHeader.Width; j++)
			{
				int num3 = num + j * 4;
				int num4 = (num2 * m_infoHeader.Width + j) * 4;
				base.ImageData[num4] = imageArray[num3];
				base.ImageData[num4 + 1] = imageArray[num3 + 1];
				base.ImageData[num4 + 2] = imageArray[num3 + 2];
				base.ImageData[num4 + 3] = byte.MaxValue;
			}
		}
		SetColorSpacePalette(4);
	}

	private int Invert(int y, int height)
	{
		int num = 0;
		if (height > 0)
		{
			return height - y - 1;
		}
		return y;
	}

	private byte[] GetImageArray(int width, int height, int bytes, ref int alignment)
	{
		alignment = width * bytes % 4;
		if (alignment != 0)
		{
			alignment = 4 - alignment;
		}
		int num = (width * bytes + alignment) * height;
		byte[] array = new byte[num];
		base.InternalStream.Read(array, 0, num);
		return array;
	}

	private void SetColorSpacePalette(int bpc)
	{
		if (m_palette != null)
		{
			m_csPalette = new byte[m_palette.Length / bpc * 3];
			int num = m_palette.Length / bpc;
			for (int i = 0; i < num; i++)
			{
				int num2 = i * bpc;
				int num3 = i * 3;
				m_csPalette[num3 + 2] = m_palette[num2++];
				m_csPalette[num3 + 1] = m_palette[num2++];
				m_csPalette[num3] = m_palette[num2];
			}
		}
	}

	private void UpdateImageResolution(float[] resolution)
	{
		byte[] array = new byte[40];
		base.InternalStream.Read(array, 0, array.Length);
		int num = BitConverter.ToInt32(array, 24);
		BitConverter.ToInt32(array, 28);
		resolution[0] = (float)Math.Round((float)num * 0.0254f, 2);
		resolution[1] = (float)Math.Round((float)num * 0.0254f, 2);
	}
}
