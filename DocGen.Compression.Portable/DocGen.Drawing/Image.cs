using System;
using System.IO;

namespace DocGen.Drawing;

public class Image
{
	private const string BmpHeader = "BM";

	private const string GifHeader = "GIF8";

	private static readonly byte[] JpegHeader = new byte[2] { 255, 216 };

	private static readonly byte[] PngHeader = new byte[8] { 137, 80, 78, 71, 13, 10, 26, 10 };

	private static readonly byte[] TiffHeader1 = new byte[3] { 73, 73, 42 };

	private static readonly byte[] TiffHeader2 = new byte[3] { 77, 77, 0 };

	private ImageFormat _format;

	private int _height;

	private byte[] _imageData;

	private Stream _stream;

	private int _width;

	private int _horizontalResolution = 96;

	private int _verticalResolution = 96;

	public ImageFormat Format => _format;

	public int Height => _height;

	public byte[] ImageData => _imageData;

	public ImageFormat RawFormat => Format;

	public Size Size => new Size(_width, _height);

	public int Width => _width;

	internal int HorizontalResolution => _horizontalResolution;

	internal int VerticalResolution => _verticalResolution;

	public Image(Stream stream)
	{
		if (!stream.CanRead || !stream.CanSeek)
		{
			throw new ArgumentException("Stream");
		}
		_stream = stream;
		Initialize();
	}

	private bool CheckIfBmp()
	{
		Reset();
		if (ReadString(2).StartsWith("BM"))
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
			_format = ImageFormat.Emf;
			return true;
		}
		Reset();
		if (ReadInt32() == -1698247209)
		{
			_format = ImageFormat.Wmf;
			return true;
		}
		return false;
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

	private bool CheckIfIcon()
	{
		Reset();
		int num = ReadWord();
		int num2 = ReadWord();
		if (num == 0)
		{
			return num2 == 1;
		}
		return false;
	}

	private bool CheckIfJpeg()
	{
		Reset();
		byte[] jpegHeader = JpegHeader;
		for (int i = 0; i < jpegHeader.Length; i++)
		{
			if (jpegHeader[i] != _stream.ReadByte())
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckIfPng()
	{
		Reset();
		byte[] pngHeader = PngHeader;
		for (int i = 0; i < pngHeader.Length; i++)
		{
			if (pngHeader[i] != _stream.ReadByte())
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckIfTiff1()
	{
		Reset();
		byte[] array = new byte[3];
		_stream.Read(array, 0, array.Length);
		if (array[0] == TiffHeader1[0] && array[1] == TiffHeader1[1] && array[2] == TiffHeader1[2])
		{
			return true;
		}
		return false;
	}

	private bool CheckIfTiff2()
	{
		Reset();
		byte[] array = new byte[3];
		_stream.Read(array, 0, array.Length);
		if (array[0] == TiffHeader2[0] && array[1] == TiffHeader2[1] && array[2] == TiffHeader2[2])
		{
			return true;
		}
		return false;
	}

	private void Initialize()
	{
		if (CheckIfPng())
		{
			_format = ImageFormat.Png;
			ParsePngImage();
		}
		if (_format == ImageFormat.Unknown && CheckIfJpeg())
		{
			_format = ImageFormat.Jpeg;
			ParseJpegImage();
		}
		if (_format == ImageFormat.Unknown && CheckIfGif())
		{
			_format = ImageFormat.Gif;
			ParseGifImage();
		}
		if (_format == ImageFormat.Unknown && CheckIfEmfOrWmf())
		{
			ParseEmfOrWmfImage();
		}
		if (_format == ImageFormat.Unknown && CheckIfIcon())
		{
			_format = ImageFormat.Icon;
			ParseIconImage();
		}
		if (_format == ImageFormat.Unknown && CheckIfBmp())
		{
			_format = ImageFormat.Bmp;
			ParseBmpImage();
		}
		if (_format == ImageFormat.Unknown && CheckIfTiff1())
		{
			_format = ImageFormat.Tiff;
			ParseTiffImage();
		}
		if (_format == ImageFormat.Unknown && CheckIfTiff2())
		{
			_format = ImageFormat.Tiff;
		}
		Reset();
		_imageData = new byte[_stream.Length];
		_stream.Read(_imageData, 0, _imageData.Length);
	}

	private void ParseBmpImage()
	{
		_stream.Position = 18L;
		_width = ReadInt32();
		_height = ReadInt32();
	}

	private void ParseTiffImage()
	{
		int num = 256;
		int num2 = 257;
		_stream.Position = 4L;
		int num3 = ReadInt32();
		if (num3 > _stream.Length)
		{
			throw new Exception("Tiff image file corrupted");
		}
		_stream.Position = num3 + 2;
		while (num3 < _stream.Length)
		{
			int num4 = ReadInt16();
			ReadInt16();
			ReadInt32();
			int num5 = ReadInt32();
			if (num4 == num)
			{
				_width = num5;
			}
			else if (num4 == num2)
			{
				_height = num5;
			}
			else if (_height != 0 && _width != 0)
			{
				break;
			}
		}
	}

	private void ParseEmfOrWmfImage()
	{
		Reset();
		if (_format == ImageFormat.Emf)
		{
			byte[] array = new byte[16];
			_stream.Read(array, 0, array.Length);
			_width = ReadInt32();
			_height = ReadInt32();
		}
		else if (Format == ImageFormat.Wmf)
		{
			byte[] array = new byte[10];
			_stream.Read(array, 0, array.Length);
			_width = ReadShortLe();
			_height = ReadShortLe();
		}
	}

	private void ParseGifImage()
	{
		_width = ReadInt16();
		_height = ReadInt16();
	}

	private void ParseIconImage()
	{
		Reset();
		byte[] array = new byte[6];
		_stream.Read(array, 0, array.Length);
		_width = _stream.ReadByte();
		_height = _stream.ReadByte();
	}

	private void ParseJpegImage()
	{
		Reset();
		byte[] array = new byte[_stream.Length];
		_stream.Read(array, 0, array.Length);
		long num = 4L;
		if ((array[num + 2] != 74 || array[num + 3] != 70 || array[num + 4] != 73 || array[num + 5] != 70 || array[num + 6] != 0) && (array[0] != byte.MaxValue || array[1] != 216 || array[^2] != byte.MaxValue || array[^1] != 217))
		{
			return;
		}
		if (array[num + 8] == 1)
		{
			if (array[num + 9] == 0)
			{
				_horizontalResolution = (array[num + 10] | array[num + 11]) * 96;
				_verticalResolution = (array[num + 12] | array[num + 13]) * 96;
			}
			else if (array[num + 9] == 1)
			{
				_horizontalResolution = array[num + 10] | array[num + 11];
				_verticalResolution = array[num + 12] | array[num + 13];
			}
			else if (array[num + 9] == 2)
			{
				_horizontalResolution = (int)Math.Round((double)(array[num + 10] | array[num + 11]) * 0.0254);
				_verticalResolution = (int)Math.Round((double)(array[num + 10] | array[num + 11]) * 0.0254);
			}
		}
		long num2 = array[num] * 256 + array[num + 1];
		while (num < array.Length && num + num2 < array.Length)
		{
			num += num2;
			if (array[num + 1] == 192 || array[num + 1] == 194)
			{
				_height = array[num + 5] * 256 + array[num + 6];
				_width = array[num + 7] * 256 + array[num + 8];
				break;
			}
			num += 2;
			num2 = array[num] * 256 + array[num + 1];
		}
	}

	private void ParsePngImage()
	{
		while (true)
		{
			int num = ReadUInt32();
			if (ReadString(4).Equals("IHDR"))
			{
				break;
			}
			byte[] buffer = new byte[num];
			_stream.Read(buffer, 0, num);
		}
		_width = ReadUInt32();
		_height = ReadUInt32();
		byte[] buffer2 = new byte[42];
		_stream.Read(buffer2, 0, 42);
		if (ReadString(4).Equals("pHYs"))
		{
			int num2 = ReadUInt32();
			_horizontalResolution = (int)Math.Round((double)num2 * 0.0254);
			int num3 = ReadUInt32();
			_verticalResolution = (int)Math.Round((double)num3 * 0.0254);
		}
	}

	private int ReadInt16()
	{
		byte[] array = new byte[2];
		_stream.Read(array, 0, 2);
		return array[0] | (array[1] << 8);
	}

	private int ReadInt32()
	{
		byte[] array = new byte[4];
		_stream.Read(array, 0, 4);
		return array[0] + (array[1] << 8) + (array[2] << 16) + (array[3] << 24);
	}

	private int ReadShortLe()
	{
		int num = ReadWord();
		if (num > 32767)
		{
			num -= 65536;
		}
		return num;
	}

	private string ReadString(int len)
	{
		char[] array = new char[len];
		for (int i = 0; i < len; i++)
		{
			array[i] = (char)_stream.ReadByte();
		}
		return new string(array);
	}

	private int ReadUInt32()
	{
		byte[] array = new byte[4];
		_stream.Read(array, 0, 4);
		return (array[0] << 24) + (array[1] << 16) + (array[2] << 8) + array[3];
	}

	private int ReadWord()
	{
		return (_stream.ReadByte() + (_stream.ReadByte() << 8)) & 0xFFFF;
	}

	private void Reset()
	{
		_stream.Position = 0L;
	}

	internal void Close()
	{
		_imageData = null;
		if (_stream != null)
		{
			_stream.Dispose();
			_stream = null;
		}
	}

	public static Image FromStream(Stream stream)
	{
		return new Image(stream);
	}

	public Image Clone()
	{
		return (Image)MemberwiseClone();
	}

	public static Image FromStream(Stream stream, bool p, bool p3)
	{
		return new Image(stream);
	}

	internal void Save(MemoryStream stream, ImageFormat imageFormat)
	{
		if (imageFormat != _format)
		{
			throw new NotSupportedException();
		}
		stream.Write(_imageData, 0, _imageData.Length);
	}

	internal void Dispose()
	{
		_imageData = null;
		_stream = null;
	}
}
