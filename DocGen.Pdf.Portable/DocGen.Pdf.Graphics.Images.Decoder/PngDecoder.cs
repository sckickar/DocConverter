using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Graphics.Images.Decoder;

internal class PngDecoder : ImageDecoder
{
	internal enum PngChunkTypes
	{
		IHDR,
		PLTE,
		IDAT,
		IEND,
		bKGD,
		cHRM,
		gAMA,
		hIST,
		pHYs,
		sBIT,
		tEXt,
		tIME,
		tRNS,
		zTXt,
		sRGB,
		iCCP,
		iTXt,
		Unknown
	}

	internal enum PngFilterTypes
	{
		None,
		Sub,
		Up,
		Average,
		Paeth
	}

	internal enum PngImageTypes
	{
		GreyScale = 0,
		TrueColor = 2,
		IndexedColor = 3,
		GrayScaleWithAlpha = 4,
		TrueColorWithAlpha = 6
	}

	internal struct PngHeader
	{
		public int Width;

		public int Height;

		public int ColorType;

		public int Compression;

		public int BitDepth;

		public PngFilterTypes Filter;

		public int Interlace;
	}

	private static float[] m_decode = new float[6] { 0f, 1f, 0f, 1f, 0f, 1f };

	private int m_currentChunkLength;

	private static int m_currentChunkSize;

	private PngHeader m_header;

	private bool m_bDecodeIdat;

	private bool m_issRGB;

	private int m_bitsPerPixel;

	private long m_idatLength;

	private int m_colors;

	private int m_inputBands;

	private int m_bytesPerPixel;

	private Stream m_iDatStream;

	internal PdfArray m_colorSpace;

	internal bool m_isDecode;

	private byte[] m_maskData;

	private byte[] m_alpha;

	private bool m_shades;

	private Stream m_dataStream;

	private bool m_ideateDecode = true;

	private bool m_enableMetadata;

	private int transparentPixel;

	private int transparentPixelId;

	private int redMask;

	private int greenMask;

	private int blueMask;

	public PngDecoder(Stream stream)
	{
		base.InternalStream = stream;
		base.Format = ImageType.Png;
		Initialize();
	}

	public PngDecoder(Stream stream, bool enableMetadata)
	{
		m_enableMetadata = enableMetadata;
		base.InternalStream = stream;
		base.Format = ImageType.Png;
		Initialize();
	}

	public static float[] GetImageResolution(Stream stream)
	{
		float[] array = new float[2] { 96f, 96f };
		PngChunkTypes header;
		while (ReadNextchunk(out header, stream))
		{
			if (header == PngChunkTypes.pHYs)
			{
				int num = stream.ReadUInt32();
				int num2 = stream.ReadUInt32();
				switch (stream.ReadByte())
				{
				case 1:
					array[0] = (float)Math.Round((float)num * 0.0254f, 2);
					array[1] = (float)Math.Round((float)num2 * 0.0254f, 2);
					break;
				case 2:
					array[0] = (float)Math.Round((float)num * 2.54f, 2);
					array[1] = (float)Math.Round((float)num2 * 2.54f, 2);
					break;
				default:
					array[0] = num;
					array[1] = num2;
					break;
				}
			}
			else
			{
				IgnoreChunk(stream);
			}
		}
		return array;
	}

	private static bool ReadNextchunk(out PngChunkTypes header, Stream stream)
	{
		header = PngChunkTypes.Unknown;
		m_currentChunkSize = stream.ReadUInt32();
		string value = stream.ReadString(4);
		if (Enum.IsDefined(typeof(PngChunkTypes), value))
		{
			header = (PngChunkTypes)Enum.Parse(typeof(PngChunkTypes), value, ignoreCase: true);
			return true;
		}
		if (stream.Length == stream.Position)
		{
			return false;
		}
		return true;
	}

	private static void IgnoreChunk(Stream stream)
	{
		if (m_currentChunkSize > 0)
		{
			stream.Skip(m_currentChunkSize + 4);
		}
	}

	protected override void Initialize()
	{
		PngChunkTypes header;
		while (ReadNextchunk(out header))
		{
			switch (header)
			{
			case PngChunkTypes.IHDR:
				ReadHeader();
				break;
			case PngChunkTypes.IDAT:
				ReadImageData();
				break;
			case PngChunkTypes.sRGB:
				m_issRGB = true;
				IgnoreChunk();
				break;
			case PngChunkTypes.PLTE:
				ReadPLTE();
				break;
			case PngChunkTypes.tRNS:
				ReadTRNS();
				break;
			case PngChunkTypes.tEXt:
			case PngChunkTypes.iTXt:
				if (m_enableMetadata)
				{
					byte[] array = base.InternalStream.ReadByte(m_currentChunkLength);
					using MemoryStream input = new MemoryStream(array);
					BinaryReader binaryReader = new BinaryReader(input);
					byte[] array2 = ReadNullTerminatedBytes(79, binaryReader);
					string @string = Encoding.GetEncoding("ISO-8859-1").GetString(array2, 0, array2.Length);
					if (!(@string == "XML:com.adobe.xmp"))
					{
						break;
					}
					sbyte b = (sbyte)binaryReader.ReadByte();
					sbyte b2 = (sbyte)binaryReader.ReadByte();
					byte[] array3 = ReadNullTerminatedBytes(array.Length, binaryReader);
					byte[] array4 = ReadNullTerminatedBytes(array.Length, binaryReader);
					int num = array.Length - @string.Length - 5 - array3.Length - array4.Length;
					switch (b)
					{
					case 0:
					{
						byte[] array5 = ReadNullTerminatedBytes(num, binaryReader);
						if (base.MetadataStream == null)
						{
							base.MetadataStream = new MemoryStream();
						}
						base.MetadataStream.Write(array5, 0, array5.Length);
						break;
					}
					case 1:
						if (b2 == 0)
						{
							if (base.MetadataStream == null)
							{
								base.MetadataStream = new MemoryStream();
							}
							base.MetadataStream.Write(array, array.Length - num, num);
						}
						break;
					}
				}
				else
				{
					IgnoreChunk();
				}
				break;
			case PngChunkTypes.zTXt:
				if (m_enableMetadata)
				{
					byte[] chunkBytes = base.InternalStream.ReadByte(m_currentChunkLength);
					ReadZTextMetadata(chunkBytes);
				}
				else
				{
					IgnoreChunk();
				}
				break;
			case PngChunkTypes.bKGD:
			case PngChunkTypes.cHRM:
			case PngChunkTypes.gAMA:
			case PngChunkTypes.hIST:
			case PngChunkTypes.pHYs:
			case PngChunkTypes.sBIT:
			case PngChunkTypes.tIME:
			case PngChunkTypes.iCCP:
			case PngChunkTypes.Unknown:
				IgnoreChunk();
				break;
			}
		}
	}

	private void ReadZTextMetadata(byte[] chunkBytes)
	{
		using MemoryStream input = new MemoryStream(chunkBytes);
		BinaryReader binaryReader = new BinaryReader(input);
		byte[] array = ReadNullTerminatedBytes(79, binaryReader);
		string @string = Encoding.GetEncoding("ISO-8859-1").GetString(array, 0, array.Length);
		if (!(@string == "XML:com.adobe.xmp"))
		{
			return;
		}
		sbyte num = (sbyte)binaryReader.ReadByte();
		int num2 = 4;
		int num3 = chunkBytes.Length - @string.Length - num2;
		if (num == 0)
		{
			if (base.MetadataStream == null)
			{
				base.MetadataStream = new MemoryStream();
			}
			base.MetadataStream.Write(chunkBytes, chunkBytes.Length - num3, num3);
		}
	}

	internal byte[] ReadNullTerminatedBytes(int maxLength, BinaryReader reader)
	{
		byte[] array = new byte[maxLength];
		int i;
		for (i = 0; i < array.Length && (array[i] = reader.ReadByte()) != 0; i++)
		{
		}
		if (i == maxLength)
		{
			return array;
		}
		byte[] array2 = new byte[i];
		if (i > 0)
		{
			Array.Copy(array, array2, i);
		}
		return array2;
	}

	internal void DecodeImageData()
	{
		m_isDecode = m_header.Interlace == 1 || m_header.BitDepth == 16 || ((uint)m_header.ColorType & 4u) != 0 || m_shades;
		if (m_isDecode)
		{
			if (((uint)m_header.ColorType & 4u) != 0 || m_shades)
			{
				m_maskData = new byte[base.Width * base.Height];
			}
			if (m_iDatStream != null)
			{
				m_iDatStream.Position = 0L;
				m_dataStream = GetDeflatedData(m_iDatStream);
				m_dataStream.Position = 0L;
			}
			base.ImageData = new byte[m_idatLength];
			ReadDecodeData();
			if (base.ImageData != null && base.ImageData.Length == 0 && m_shades)
			{
				m_ideateDecode = false;
				base.ImageData = (m_iDatStream as MemoryStream).ToArray();
			}
		}
		else
		{
			m_ideateDecode = false;
			base.ImageData = (m_iDatStream as MemoryStream).ToArray();
		}
	}

	private Stream GetDeflatedData(Stream iDatStream)
	{
		byte[] array = (iDatStream as MemoryStream).ToArray();
		DeflateStream deflateStream = new DeflateStream(new MemoryStream(array, 2, array.Length - 6), CompressionMode.Decompress, leaveOpen: true);
		Stream stream = new MemoryStream();
		byte[] array2 = new byte[4096];
		int count;
		while ((count = deflateStream.Read(array2, 0, array2.Length)) > 0)
		{
			stream.Write(array2, 0, count);
		}
		return stream;
	}

	private void ReadDecodeData()
	{
		if (m_header.Interlace != 1)
		{
			DecodeData(0, 0, 1, 1, base.Width, base.Height);
			return;
		}
		DecodeData(0, 0, 8, 8, (base.Width + 7) / 8, (base.Height + 7) / 8);
		DecodeData(4, 0, 8, 8, (base.Width + 3) / 8, (base.Height + 7) / 8);
		DecodeData(0, 4, 4, 8, (base.Width + 3) / 4, (base.Height + 3) / 8);
		DecodeData(2, 0, 4, 4, (base.Width + 1) / 4, (base.Height + 3) / 4);
		DecodeData(0, 2, 2, 4, (base.Width + 1) / 2, (base.Height + 1) / 4);
		DecodeData(1, 0, 2, 2, base.Width / 2, (base.Height + 1) / 2);
		DecodeData(0, 1, 1, 2, base.Width, base.Height / 2);
	}

	private void DecodeData(int xOffset, int yOffset, int xStep, int yStep, int width, int height)
	{
		if (width == 0 || height == 0)
		{
			return;
		}
		int num = (m_inputBands * width * m_header.BitDepth + 7) / 8;
		byte[] array = new byte[num];
		byte[] array2 = new byte[num];
		int num2 = 0;
		int num3 = yOffset;
		while (num2 < height)
		{
			int num4 = m_dataStream.ReadByte();
			Read(m_dataStream, array, 0, num);
			switch ((PngFilterTypes)num4)
			{
			case PngFilterTypes.Sub:
				DecompressSub(array, num, m_bitsPerPixel);
				break;
			case PngFilterTypes.Up:
				DecompressUp(array, array2, num);
				break;
			case PngFilterTypes.Average:
				DecompressAverage(array, array2, num, m_bitsPerPixel);
				break;
			case PngFilterTypes.Paeth:
				DecompressPaeth(array, array2, num, m_bitsPerPixel);
				break;
			default:
				throw new Exception("Unknown PNG filter");
			case PngFilterTypes.None:
				break;
			}
			ProcessPixels(array, xOffset, xStep, num3, width);
			byte[] array3 = array2;
			array2 = array;
			array = array3;
			num2++;
			num3 += yStep;
		}
	}

	private void ProcessPixels(byte[] data, int x, int step, int y, int width)
	{
		int num = 0;
		int[] pixel = GetPixel(data);
		if (m_header.ColorType == 0 || m_header.ColorType == 3 || m_header.ColorType == 4)
		{
			num = 1;
		}
		else if (m_header.ColorType == 2 || m_header.ColorType == 6)
		{
			num = 3;
		}
		int num2;
		if (base.ImageData != null && base.ImageData.Length != 0)
		{
			num2 = x;
			int num3 = ((m_header.BitDepth == 16) ? 8 : m_header.BitDepth);
			int bpr = (num * base.Width * num3 + 7) / 8;
			for (int i = 0; i < width; i++)
			{
				SetPixel(base.ImageData, pixel, m_inputBands * i, num, num2, y, m_header.BitDepth, bpr);
				num2 += step;
			}
		}
		if ((m_header.ColorType & 4) == 0 && (!m_shades || 1 == 0))
		{
			return;
		}
		if (((uint)m_header.ColorType & 4u) != 0)
		{
			if (m_header.BitDepth == 16)
			{
				for (int j = 0; j < width; j++)
				{
					int num4 = j * m_inputBands + num;
					pixel[num4] >>>= 8;
				}
			}
			int width2 = base.Width;
			num2 = x;
			for (int i = 0; i < width; i++)
			{
				SetPixel(m_maskData, pixel, m_inputBands * i + num, 1, num2, y, 8, width2);
				num2 += step;
			}
			return;
		}
		int width3 = base.Width;
		int[] array = new int[1];
		num2 = x;
		for (int i = 0; i < width; i++)
		{
			int num5 = pixel[i];
			if (num5 < m_alpha.Length)
			{
				array[0] = m_alpha[num5];
			}
			else
			{
				array[0] = 255;
			}
			SetPixel(m_maskData, array, 0, 1, num2, y, 8, width3);
			num2 += step;
		}
	}

	private int[] GetPixel(byte[] data)
	{
		if (m_header.BitDepth == 8)
		{
			int[] array = new int[data.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = data[i] & 0xFF;
			}
			return array;
		}
		if (m_header.BitDepth == 16)
		{
			int[] array2 = new int[data.Length / 2];
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j] = ((data[j * 2] & 0xFF) << 8) + (data[j * 2 + 1] & 0xFF);
			}
			return array2;
		}
		int[] array3 = new int[data.Length * 8 / m_header.BitDepth];
		int num = 0;
		int num2 = 8 / m_header.BitDepth;
		int num3 = (1 << m_header.BitDepth) - 1;
		for (int k = 0; k < data.Length; k++)
		{
			for (int num4 = num2 - 1; num4 >= 0; num4--)
			{
				int num5 = m_header.BitDepth * num4;
				int num6 = data[k];
				array3[num++] = ((num5 < 1) ? num6 : (num6 >>> num5)) & num3;
			}
		}
		return array3;
	}

	private void SetPixel(byte[] imageData, int[] data, int offset, int size, int x, int y, int bitDepth, int bpr)
	{
		switch (bitDepth)
		{
		case 8:
		{
			int num4 = bpr * y + size * x;
			for (int j = 0; j < size; j++)
			{
				imageData[num4 + j] = (byte)data[j + offset];
			}
			break;
		}
		case 16:
		{
			int num3 = bpr * y + size * x;
			for (int i = 0; i < size; i++)
			{
				imageData[num3 + i] = (byte)(data[i + offset] >> 8);
			}
			break;
		}
		default:
		{
			int num = bpr * y + x / (8 / bitDepth);
			int num2 = data[offset] << 8 - bitDepth * (x % (8 / bitDepth)) - bitDepth;
			imageData[num] |= (byte)num2;
			break;
		}
		}
	}

	private void Read(Stream stream, byte[] data, int offset, int count)
	{
		while (count > 0)
		{
			int num = stream.Read(data, offset, count);
			if (num <= 0)
			{
				throw new IOException("Insufficient data");
			}
			count -= num;
			offset += num;
		}
	}

	private void DecompressSub(byte[] data, int count, int bpp)
	{
		for (int i = bpp; i < count; i++)
		{
			int num = 0;
			num = data[i] & 0xFF;
			num += data[i - bpp] & 0xFF;
			data[i] = (byte)num;
		}
	}

	private void DecompressUp(byte[] data, byte[] pData, int count)
	{
		for (int i = 0; i < count; i++)
		{
			int num = data[i] & 0xFF;
			int num2 = pData[i] & 0xFF;
			data[i] = (byte)(num + num2);
		}
	}

	private void DecompressAverage(byte[] data, byte[] pData, int count, int bpp)
	{
		for (int i = 0; i < bpp; i++)
		{
			int num = data[i] & 0xFF;
			int num2 = pData[i] & 0xFF;
			data[i] = (byte)(num + num2 / 2);
		}
		for (int j = bpp; j < count; j++)
		{
			int num = data[j] & 0xFF;
			int num3 = data[j - bpp] & 0xFF;
			int num2 = pData[j] & 0xFF;
			data[j] = (byte)(num + (num3 + num2) / 2);
		}
	}

	private int PaethPredictor(int a, int b, int c)
	{
		int num = a + b - c;
		int num2 = Math.Abs(num - a);
		int num3 = Math.Abs(num - b);
		int num4 = Math.Abs(num - c);
		if (num2 <= num3 && num2 <= num4)
		{
			return a;
		}
		if (num3 <= num4)
		{
			return b;
		}
		return c;
	}

	private void DecompressPaeth(byte[] data, byte[] pData, int count, int bpp)
	{
		for (int i = 0; i < bpp; i++)
		{
			int num = data[i] & 0xFF;
			int num2 = pData[i] & 0xFF;
			data[i] = (byte)(num + num2);
		}
		for (int j = bpp; j < count; j++)
		{
			int num = data[j] & 0xFF;
			int a = data[j - bpp] & 0xFF;
			int num2 = pData[j] & 0xFF;
			int c = pData[j - bpp] & 0xFF;
			data[j] = (byte)(num + PaethPredictor(a, num2, c));
		}
	}

	private void ReadPLTE()
	{
		if (m_header.ColorType == 3)
		{
			PdfString pdfString = new PdfString(base.InternalStream.ReadByte(m_currentChunkLength));
			pdfString.IsColorSpace = true;
			m_colorSpace = new PdfArray();
			m_colorSpace.Add(new PdfName("Indexed"));
			m_colorSpace.Add(GetPngColorSpace());
			m_colorSpace.Add(new PdfNumber(m_currentChunkLength / 3 - 1));
			m_colorSpace.Add(pdfString);
		}
		else
		{
			IgnoreChunk();
		}
	}

	private void ReadTRNS()
	{
		if (m_header.ColorType == 3)
		{
			byte[] array = base.InternalStream.ReadByte(m_currentChunkLength);
			m_alpha = new byte[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				m_alpha[i] = array[i];
				int num = array[i] & 0xFF;
				if (num == 0)
				{
					transparentPixel++;
					transparentPixelId = i;
				}
				if (num != 0 && num != 255)
				{
					m_shades = true;
				}
			}
		}
		else if (m_header.ColorType == 2)
		{
			byte[] array2 = base.InternalStream.ReadByte(m_currentChunkLength);
			if (array2.Length > 5)
			{
				redMask = array2[1];
				greenMask = array2[3];
				blueMask = array2[5];
			}
		}
		else
		{
			IgnoreChunk();
		}
	}

	private IPdfPrimitive GetPngColorSpace()
	{
		if (!m_issRGB)
		{
			if ((m_header.ColorType & 2) == 0)
			{
				return new PdfName("DeviceGray");
			}
			return new PdfName("DeviceRGB");
		}
		PdfArray pdfArray = new PdfArray();
		PdfDictionary pdfDictionary = new PdfDictionary();
		PdfArray primitive = new PdfArray
		{
			new PdfNumber(1),
			new PdfNumber(1),
			new PdfNumber(1)
		};
		pdfDictionary.SetProperty("Gamma", new PdfArray
		{
			new PdfNumber(2.2f),
			new PdfNumber(2.2f),
			new PdfNumber(2.2f)
		});
		if (m_issRGB)
		{
			float num = 0.3127f;
			float num2 = 0.329f;
			float num3 = 0.64f;
			float num4 = 0.33f;
			float num5 = 0.3f;
			float num6 = 0.6f;
			float num7 = 0.15f;
			float num8 = 0.06f;
			float num9 = num2 * ((num5 - num7) * num4 - (num3 - num7) * num6 + (num3 - num5) * num8);
			float num10 = num4 * ((num5 - num7) * num2 - (num - num7) * num6 + (num - num5) * num8) / num9;
			float num11 = num10 * num3 / num4;
			float num12 = num10 * ((1f - num3) / num4 - 1f);
			float num13 = (0f - num6) * ((num3 - num7) * num2 - (num - num7) * num4 + (num - num3) * num8) / num9;
			float num14 = num13 * num5 / num6;
			float num15 = num13 * ((1f - num5) / num6 - 1f);
			float num16 = num8 * ((num3 - num5) * num2 - (num - num5) * num2 + (num - num3) * num6) / num9;
			float num17 = num16 * num7 / num8;
			float num18 = num16 * ((1f - num7) / num8 - 1f);
			float value = num11 + num14 + num17;
			float value2 = 1f;
			float value3 = num12 + num15 + num18;
			primitive = new PdfArray
			{
				new PdfNumber(value),
				new PdfNumber(value2),
				new PdfNumber(value3)
			};
			pdfDictionary.SetProperty("Matrix", new PdfArray
			{
				new PdfNumber(num11),
				new PdfNumber(num10),
				new PdfNumber(num12),
				new PdfNumber(num14),
				new PdfNumber(num13),
				new PdfNumber(num15),
				new PdfNumber(num17),
				new PdfNumber(num16),
				new PdfNumber(num18)
			});
		}
		pdfDictionary.SetProperty("WhitePoint", primitive);
		pdfArray.Add(new PdfName("CalRGB"));
		pdfArray.Add(pdfDictionary);
		return pdfArray;
	}

	private void ReadImageData()
	{
		byte[] array = new byte[m_currentChunkLength];
		base.InternalStream.Read(array, 0, m_currentChunkLength);
		if (m_iDatStream == null)
		{
			m_iDatStream = new MemoryStream();
		}
		m_iDatStream.Write(array, 0, array.Length);
		base.InternalStream.Skip(4);
	}

	internal void InitializeBase()
	{
		base.Width = m_header.Width;
		base.Height = m_header.Height;
		base.BitsPerComponent = m_header.BitDepth;
	}

	internal override PdfStream GetImageDictionary()
	{
		PdfStream pdfStream = new PdfStream();
		pdfStream.InternalStream = new MemoryStream(base.ImageData);
		if (m_isDecode && m_ideateDecode)
		{
			pdfStream.Compress = true;
		}
		else
		{
			pdfStream.Compress = false;
		}
		pdfStream["Type"] = new PdfName("XObject");
		pdfStream["Subtype"] = new PdfName("Image");
		pdfStream["Width"] = new PdfNumber(base.Width);
		pdfStream["Height"] = new PdfNumber(base.Height);
		if (base.BitsPerComponent == 16)
		{
			pdfStream["BitsPerComponent"] = new PdfNumber(8);
		}
		else
		{
			pdfStream["BitsPerComponent"] = new PdfNumber(base.BitsPerComponent);
		}
		if (!m_isDecode || !m_ideateDecode)
		{
			pdfStream["Filter"] = new PdfName("FlateDecode");
		}
		if ((m_header.ColorType & 2) == 0)
		{
			pdfStream["ColorSpace"] = new PdfName("DeviceGray");
		}
		else
		{
			pdfStream["ColorSpace"] = new PdfName("DeviceRGB");
		}
		if (!m_isDecode || (m_shades && !m_ideateDecode))
		{
			pdfStream["DecodeParms"] = GetDecodeParams();
		}
		SetMask(pdfStream);
		return pdfStream;
	}

	private void SetMask(PdfStream imageStream)
	{
		if (m_maskData != null && m_maskData.Length != 0)
		{
			PdfStream pdfStream = new PdfStream();
			pdfStream.InternalStream = new MemoryStream(m_maskData);
			pdfStream["Type"] = new PdfName("XObject");
			pdfStream["Subtype"] = new PdfName("Image");
			pdfStream["Width"] = new PdfNumber(base.Width);
			pdfStream["Height"] = new PdfNumber(base.Height);
			if (base.BitsPerComponent == 16)
			{
				pdfStream["BitsPerComponent"] = new PdfNumber(8);
			}
			else
			{
				pdfStream["BitsPerComponent"] = new PdfNumber(base.BitsPerComponent);
			}
			pdfStream["ColorSpace"] = new PdfName("DeviceGray");
			imageStream.SetProperty("SMask", new PdfReferenceHolder(pdfStream));
		}
		else if (!m_shades && transparentPixel == 1)
		{
			imageStream.SetProperty("Mask", new PdfArray(new int[2] { transparentPixelId, transparentPixelId }));
		}
		else if (m_header.ColorType == 2 && redMask != 0 && greenMask != 0 && blueMask != 0 && m_header.BitDepth != 16)
		{
			PdfArray pdfArray = new PdfArray();
			pdfArray.Add(new PdfNumber(redMask));
			pdfArray.Add(new PdfNumber(redMask));
			pdfArray.Add(new PdfNumber(greenMask));
			pdfArray.Add(new PdfNumber(greenMask));
			pdfArray.Add(new PdfNumber(blueMask));
			pdfArray.Add(new PdfNumber(blueMask));
			imageStream.SetProperty("Mask", pdfArray);
		}
	}

	private PdfDictionary GetDecodeParams()
	{
		return new PdfDictionary
		{
			["Columns"] = new PdfNumber(base.Width),
			["Colors"] = new PdfNumber(m_colors),
			["Predictor"] = new PdfNumber(15),
			["BitsPerComponent"] = new PdfNumber(base.BitsPerComponent)
		};
	}

	internal void Dispose()
	{
		if (m_iDatStream != null)
		{
			m_iDatStream.Dispose();
		}
		if (m_dataStream != null)
		{
			m_dataStream.Dispose();
		}
		m_iDatStream = null;
		m_dataStream = null;
		m_maskData = null;
		base.ImageData = null;
		base.InternalStream = null;
	}

	private bool ReadNextchunk(out PngChunkTypes header)
	{
		header = PngChunkTypes.Unknown;
		m_currentChunkLength = base.InternalStream.ReadUInt32();
		string value = base.InternalStream.ReadString(4);
		if (Enum.IsDefined(typeof(PngChunkTypes), value))
		{
			header = (PngChunkTypes)Enum.Parse(typeof(PngChunkTypes), value, ignoreCase: true);
			return true;
		}
		if (base.InternalStream.Length == base.InternalStream.Position)
		{
			return false;
		}
		return true;
	}

	private void ReadHeader()
	{
		m_header.Width = base.InternalStream.ReadUInt32();
		m_header.Height = base.InternalStream.ReadUInt32();
		m_header.BitDepth = base.InternalStream.ReadByte();
		m_header.ColorType = base.InternalStream.ReadByte();
		m_header.Compression = base.InternalStream.ReadByte();
		m_header.Filter = (PngFilterTypes)base.InternalStream.ReadByte();
		m_header.Interlace = base.InternalStream.ReadByte();
		m_bDecodeIdat = (m_header.ColorType & 4) != 0;
		m_colors = ((m_header.ColorType == 3 || (m_header.ColorType & 2) == 0) ? 1 : 3);
		m_bytesPerPixel = ((m_header.BitDepth != 16) ? 1 : 2);
		InitializeBase();
		SetBitsPerPixel();
		base.InternalStream.Skip(4);
	}

	private void SetBitsPerPixel()
	{
		m_bitsPerPixel = ((m_header.BitDepth != 16) ? 1 : 2);
		if (m_header.ColorType == 0)
		{
			m_idatLength = (base.BitsPerComponent * base.Width + 7) / 8 * base.Height;
			m_inputBands = 1;
		}
		else if (m_header.ColorType == 2)
		{
			m_idatLength = base.Width * base.Height * 3;
			m_inputBands = 3;
			m_bitsPerPixel *= 3;
		}
		else if (m_header.ColorType == 3)
		{
			if (m_header.Interlace == 1)
			{
				m_idatLength = (m_header.BitDepth * base.Width + 7) / 8 * base.Height;
			}
			m_inputBands = 1;
			m_bitsPerPixel = 1;
		}
		else if (m_header.ColorType == 4)
		{
			m_idatLength = base.Width * base.Height;
			m_inputBands = 2;
			m_bitsPerPixel *= 2;
		}
		else if (m_header.ColorType == 6)
		{
			m_idatLength = base.Width * 3 * base.Height;
			m_inputBands = 4;
			m_bitsPerPixel *= 4;
		}
	}

	private void IgnoreChunk()
	{
		if (m_currentChunkLength > 0)
		{
			base.InternalStream.Skip(m_currentChunkLength + 4);
		}
	}
}
