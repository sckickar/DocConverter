using System;
using System.IO;
using System.Reflection;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS.Entities;

internal class Image : IDisposable
{
	private const string DEF_GIF_HEADER = "GIF8";

	private const int DEF_TIFF_MARKER = 42;

	private Stream m_stream;

	private int m_height;

	private int m_width;

	private long m_Xdpi = 96L;

	private long m_Ydpi = 96L;

	private ImageFormat m_format;

	private byte[] m_pngHeader = new byte[8] { 137, 80, 78, 71, 13, 10, 26, 10 };

	private byte[] m_jpegHeader = new byte[2] { 255, 216 };

	private byte[] m_bmpHeader = new byte[2] { 66, 77 };

	private byte[] m_tiffHeader1 = new byte[2] { 73, 73 };

	private byte[] m_tiffHeader2 = new byte[2] { 77, 77 };

	private byte[] m_imageData;

	public int Width => m_width;

	public int Height => m_height;

	public ImageFormat Format => m_format;

	public Size Size => new Size(m_width, m_height);

	internal byte[] ImageData => m_imageData;

	public ImageFormat RawFormat => m_format;

	internal long HorizontalDpi
	{
		get
		{
			return m_Xdpi;
		}
		set
		{
			m_Xdpi = value;
		}
	}

	internal long VerticalDpi
	{
		get
		{
			return m_Ydpi;
		}
		set
		{
			m_Ydpi = value;
		}
	}

	public bool IsMetafile
	{
		get
		{
			if (RawFormat != ImageFormat.Emf && RawFormat != ImageFormat.Wmf)
			{
				return false;
			}
			return true;
		}
	}

	public Image(Stream stream)
	{
		if (!stream.CanRead || !stream.CanSeek)
		{
			throw new ArgumentException("Stream");
		}
		m_stream = stream;
		Initialize();
		if (m_format == ImageFormat.Unknown)
		{
			throw new ArgumentException("The given image stream is either unsupported or not a valid image stream");
		}
	}

	internal Image()
	{
	}

	private void Initialize()
	{
		if (CheckIfPng())
		{
			m_format = ImageFormat.Png;
			ParsePngImage();
		}
		if (m_format == ImageFormat.Unknown && CheckIfJpeg())
		{
			m_format = ImageFormat.Jpeg;
			ParseJpegImage();
		}
		if (m_format == ImageFormat.Unknown && CheckIfGif())
		{
			m_format = ImageFormat.Gif;
			ParseGifImage();
		}
		if (m_format == ImageFormat.Unknown && CheckIfEmfOrWmf())
		{
			ParseEmfOrWmfImage();
		}
		if (m_format == ImageFormat.Unknown && CheckIfIcon())
		{
			m_format = ImageFormat.Icon;
			ParseIconImage();
		}
		if (m_format == ImageFormat.Unknown && CheckIfBmp())
		{
			m_format = ImageFormat.Bmp;
			ParseBmpImage();
		}
		if (m_format == ImageFormat.Unknown && CheckIfTiff())
		{
			m_format = ImageFormat.Tiff;
			ParseTifImage();
		}
		if (m_format == ImageFormat.Unknown && WordDocument.EnablePartialTrustCode)
		{
			m_format = ImageFormat.Emf;
			ParseEmfOrWmfImage();
		}
		if (m_format == ImageFormat.Unknown)
		{
			m_format = ImageFormat.Emf;
			ParseEmfOrWmfImage();
		}
		if (m_format != 0)
		{
			Reset();
			m_imageData = new byte[m_stream.Length];
			m_stream.Read(m_imageData, 0, m_imageData.Length);
		}
	}

	private bool CheckIfTiff()
	{
		Reset();
		byte[] array = new byte[3];
		m_stream.Read(array, 0, array.Length);
		if (((array[0] == m_tiffHeader1[0] && array[1] == m_tiffHeader1[1]) || (array[0] == m_tiffHeader2[0] && array[1] == m_tiffHeader2[1])) && array[2] == 42)
		{
			return true;
		}
		return false;
	}

	private bool CheckIfBmp()
	{
		Reset();
		for (int i = 0; i < m_bmpHeader.Length; i++)
		{
			if (m_bmpHeader[i] != m_stream.ReadByte())
			{
				return false;
			}
		}
		return true;
	}

	private void ParseBmpImage()
	{
		Reset();
		int num = 14;
		byte[] buffer = new byte[num];
		m_stream.Read(buffer, 0, num);
		ReadInt32();
		m_width = ReadInt32();
		m_height = ReadInt32();
	}

	private bool CheckIfIcon()
	{
		Reset();
		int num = ReadWord();
		int num2 = ReadWord();
		if (num == 0 && num2 == 1)
		{
			return true;
		}
		return false;
	}

	private bool CheckIfPng()
	{
		Reset();
		for (int i = 0; i < m_pngHeader.Length; i++)
		{
			if (m_pngHeader[i] != m_stream.ReadByte())
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckIfJpeg()
	{
		Reset();
		for (int i = 0; i < m_jpegHeader.Length; i++)
		{
			if (m_jpegHeader[i] != m_stream.ReadByte())
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckIfGif()
	{
		Reset();
		if (ReadString(6).StartsWith("GIF8"))
		{
			return true;
		}
		return false;
	}

	private bool CheckIfEmfOrWmf()
	{
		Reset();
		if (ReadInt32() == 1)
		{
			m_format = ImageFormat.Emf;
			return true;
		}
		Reset();
		if (ReadInt32() == -1698247209)
		{
			m_format = ImageFormat.Wmf;
			ReadInt32();
			ReadInt32();
			ReadInt16();
			HorizontalDpi = ReadInt16();
			return true;
		}
		Reset();
		int num = ReadInt16();
		ReadInt16();
		int num2 = ReadInt16();
		if ((num == 1 || num == 2) && (num2 == 768 || num2 == 256))
		{
			m_format = ImageFormat.Wmf;
			return true;
		}
		return false;
	}

	private void ParsePngImage()
	{
		while (m_stream.Position < m_stream.Length - 1)
		{
			int num = ReadUInt32();
			if (ReadString(4).Equals("IHDR"))
			{
				m_width = ReadUInt32();
				m_height = ReadUInt32();
				byte[] buffer = new byte[42];
				m_stream.Read(buffer, 0, 42);
				if (ReadString(4).Equals("pHYs"))
				{
					int num2 = ReadUInt32();
					HorizontalDpi = (int)Math.Round((double)num2 * 0.0254);
					int num3 = ReadUInt32();
					VerticalDpi = (int)Math.Round((double)num3 * 0.0254);
				}
				break;
			}
			byte[] buffer2 = new byte[num];
			m_stream.Read(buffer2, 0, num);
		}
	}

	private Stream GetManifestResourceStream(string fileName)
	{
		Assembly assembly = typeof(WPicture).GetTypeInfo().Assembly;
		string[] manifestResourceNames = assembly.GetManifestResourceNames();
		foreach (string text in manifestResourceNames)
		{
			if (text.EndsWith("." + fileName))
			{
				fileName = text;
				break;
			}
		}
		return assembly.GetManifestResourceStream(fileName);
	}

	private void ParseJpegImage()
	{
		Reset();
		byte[] array = new byte[m_stream.Length];
		m_stream.Read(array, 0, (int)m_stream.Length - 1);
		long num = 2L;
		long length = 0L;
		long bytesleft = 0L;
		if (array[num + 1] == 224)
		{
			GenerateJPEGJFIFImage(array, num, length, bytesleft);
		}
		else if (array[num + 1] == 225)
		{
			GenerateJPEGEXIFImage(array, num, length, bytesleft);
		}
		else
		{
			GenerateJPEGNormalImage(array, num, length, bytesleft);
		}
	}

	private void GenerateJPEGNormalImage(byte[] imgData, long index, long length, long bytesleft)
	{
		long num = 4L;
		length = imgData[num] * 256 + imgData[num + 1];
		while (num < imgData.Length)
		{
			num += length;
			if (num < imgData.Length)
			{
				if (imgData[num + 1] == 192 || imgData[num + 1] == 194)
				{
					m_height = imgData[num + 5] * 256 + imgData[num + 6];
					m_width = imgData[num + 7] * 256 + imgData[num + 8];
					break;
				}
				num += 2;
				length = imgData[num] * 256 + imgData[num + 1];
				continue;
			}
			break;
		}
	}

	private void GenerateJPEGJFIFImage(byte[] imgdata, long index, long length, long bytesleft)
	{
		while (index < imgdata.Length)
		{
			if (imgdata[index + 4] == 74 && imgdata[index + 5] == 70 && imgdata[index + 6] == 73 && imgdata[index + 7] == 70 && imgdata[index + 8] == 0)
			{
				App0Resolution(imgdata, index);
				index = 3L;
			}
			if (imgdata[index] == 192 || imgdata[index] == 194)
			{
				m_height = (imgdata[index + 4] << 8) | imgdata[index + 5];
				m_width = (imgdata[index + 6] << 8) | imgdata[index + 7];
				break;
			}
			length = (imgdata[index + 1] << 8) | imgdata[index + 2];
			index += length + 2;
		}
	}

	private void GenerateJPEGEXIFImage(byte[] imgdata, long index, long length, long bytesleft)
	{
		while (index < imgdata.Length)
		{
			if ((imgdata[index + 10] == 73 && imgdata[index + 11] == 73) || (imgdata[index + 10] == 77 && imgdata[index + 11] == 77))
			{
				bytesleft += index;
				bytesleft += 2;
				bytesleft += 6;
				bytesleft += 2;
				long length2 = imgdata[index + 2] * 256 + imgdata[index + 3];
				APP1Resolution(imgdata, length2, index, bytesleft);
				index = 3L;
			}
			if (imgdata[index] == 192 || imgdata[index] == 194)
			{
				m_height = (imgdata[index + 4] << 8) | imgdata[index + 5];
				m_width = (imgdata[index + 6] << 8) | imgdata[index + 7];
				break;
			}
			length = (imgdata[index + 1] << 8) | imgdata[index + 2];
			index += length + 2;
		}
	}

	private void App0Resolution(byte[] imgdata, long index)
	{
		if (imgdata[index + 11] == 1)
		{
			HorizontalDpi = (imgdata[index + 12] << 8) | imgdata[index + 13];
			VerticalDpi = (imgdata[index + 14] << 8) | imgdata[index + 15];
		}
		else if (imgdata[index + 11] == 2)
		{
			HorizontalDpi = (long)((float)((imgdata[index + 12] << 8) | imgdata[index + 13]) * 2.54f);
			VerticalDpi = (long)((float)((imgdata[index + 14] << 8) | imgdata[index + 15]) * 2.54f);
		}
	}

	private void APP1Resolution(byte[] imgdata, long length, long index, long bytesleft)
	{
		while (index < length)
		{
			if (((imgdata[index] == 26) & (imgdata[index + 1] == 1)) && imgdata[index + 2] == 5 && imgdata[index + 3] == 0 && imgdata[index + 4] == 1 && imgdata[index + 5] == 0 && imgdata[index + 6] == 0 && imgdata[index + 7] == 0)
			{
				index = imgdata[index + 8] + imgdata[index + 9] + imgdata[index + 10] + imgdata[index + 11] + bytesleft;
				long num = (imgdata[index + 3] << 24) | (imgdata[index + 2] << 16) | (imgdata[index + 1] << 8) | imgdata[index];
				long num2 = (imgdata[index + 7] << 24) | (imgdata[index + 6] << 16) | (imgdata[index + 5] << 8) | imgdata[index + 4];
				if (num2 != 0L)
				{
					HorizontalDpi = num / num2;
				}
				long num3 = (imgdata[index + 11] << 24) | (imgdata[index + 10] << 16) | (imgdata[index + 9] << 8) | imgdata[index + 8];
				long num4 = (imgdata[index + 15] << 24) | (imgdata[index + 14] << 16) | (imgdata[index + 13] << 8) | imgdata[index + 12];
				if (num4 != 0L)
				{
					VerticalDpi = num3 / num4;
				}
				break;
			}
			if (((imgdata[index] == 1) & (imgdata[index + 1] == 26)) && imgdata[index + 2] == 0 && imgdata[index + 3] == 5 && imgdata[index + 4] == 0 && imgdata[index + 5] == 0 && imgdata[index + 6] == 0 && imgdata[index + 7] == 1)
			{
				index = imgdata[index + 8] + imgdata[index + 9] + imgdata[index + 10] + imgdata[index + 11] + bytesleft;
				long num5 = (imgdata[index] << 24) | (imgdata[index + 1] << 16) | (imgdata[index + 2] << 8) | imgdata[index + 3];
				long num6 = (imgdata[index + 4] << 24) | (imgdata[index + 5] << 16) | (imgdata[index + 6] << 8) | imgdata[index + 7];
				if (num6 != 0L)
				{
					HorizontalDpi = num5 / num6;
				}
				long num7 = (imgdata[index + 8] << 24) | (imgdata[index + 9] << 16) | (imgdata[index + 10] << 8) | imgdata[index + 11];
				long num8 = (imgdata[index + 12] << 24) | (imgdata[index + 13] << 16) | (imgdata[index + 14] << 8) | imgdata[index + 15];
				if (num8 != 0L)
				{
					VerticalDpi = num7 / num8;
				}
				break;
			}
			index++;
		}
	}

	private void ParseGifImage()
	{
		m_width = ReadInt16();
		m_height = ReadInt16();
	}

	private void ParseIconImage()
	{
		Reset();
		byte[] array = new byte[6];
		m_stream.Read(array, 0, array.Length);
		m_width = m_stream.ReadByte();
		m_height = m_stream.ReadByte();
	}

	private void ParseEmfOrWmfImage()
	{
		Reset();
		if (m_format == ImageFormat.Emf)
		{
			byte[] array = new byte[16];
			m_stream.Read(array, 0, array.Length);
			m_width = ReadInt32();
			m_height = ReadInt32();
		}
		else if (Format == ImageFormat.Wmf)
		{
			byte[] array = new byte[10];
			m_stream.Read(array, 0, array.Length);
			m_width = ReadShortLE();
			m_height = ReadShortLE();
		}
	}

	private void ParseTifImage()
	{
		int num = 256;
		int num2 = 257;
		m_stream.Position = 4L;
		int num3 = ReadInt32();
		if (num3 > m_stream.Length)
		{
			throw new Exception("Tiff image file corrupted");
		}
		m_stream.Position = num3 + 2;
		while (num3 < m_stream.Length)
		{
			int num4 = ReadInt16();
			ReadInt16();
			ReadInt32();
			int num5 = ReadInt32();
			if (num4 == num)
			{
				m_width = num5;
			}
			else if (num4 == num2)
			{
				m_height = num5;
			}
			else if (m_height != 0 && m_width != 0)
			{
				break;
			}
		}
	}

	private int ReadUInt32()
	{
		byte[] array = new byte[4];
		m_stream.Read(array, 0, 4);
		return (array[0] << 24) + (array[1] << 16) + (array[2] << 8) + array[3];
	}

	private int ReadInt32()
	{
		byte[] array = new byte[4];
		m_stream.Read(array, 0, 4);
		return array[0] + (array[1] << 8) + (array[2] << 16) + (array[3] << 24);
	}

	private int ReadUInt16()
	{
		byte[] array = new byte[2];
		m_stream.Read(array, 0, 2);
		return (array[0] << 8) + array[1];
	}

	private int ReadInt16()
	{
		byte[] array = new byte[2];
		m_stream.Read(array, 0, 2);
		return array[0] | (array[1] << 8);
	}

	private int ReadWord()
	{
		return (m_stream.ReadByte() + (m_stream.ReadByte() << 8)) & 0xFFFF;
	}

	private int ReadShortLE()
	{
		int num = ReadWord();
		if (num > 32767)
		{
			num = 65536 - num;
		}
		if (num < 0)
		{
			num = -num;
		}
		return num;
	}

	private string ReadString(int len)
	{
		char[] array = new char[len];
		for (int i = 0; i < len; i++)
		{
			array[i] = (char)m_stream.ReadByte();
		}
		return new string(array);
	}

	private void Reset()
	{
		m_stream.Position = 0L;
	}

	internal static Image FromStream(MemoryStream memoryStream)
	{
		return new Image(memoryStream);
	}

	internal void Save(MemoryStream memoryStream, ImageFormat imageFormat)
	{
	}

	public void Dispose()
	{
		m_imageData = null;
		if (m_stream != null)
		{
			m_stream.Dispose();
			m_stream = null;
		}
	}
}
