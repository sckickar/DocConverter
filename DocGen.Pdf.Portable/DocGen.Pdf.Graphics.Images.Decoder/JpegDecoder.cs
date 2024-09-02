using System;
using System.IO;
using System.Text;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Graphics.Images.Decoder;

internal class JpegDecoder : ImageDecoder
{
	private const ushort c_SoiMarker = 55551;

	private const ushort c_JfifMarker = 57599;

	private const ushort c_SosMarker = 56063;

	private const ushort c_EoiMarker = 55807;

	private const ushort c_Sof0Marker = 192;

	private const ushort c_Sof1Marker = 193;

	private const ushort c_Sof2Marker = 194;

	private const ushort c_Sof3Marker = 195;

	private const ushort c_Sof5Marker = 197;

	private const ushort c_Sof6Marker = 198;

	private const ushort c_Sof7Marker = 199;

	private const ushort c_Sof9Marker = 201;

	private const ushort c_Sof10Marker = 202;

	private const ushort c_Sof11Marker = 203;

	private const ushort c_Sof13Marker = 205;

	private const ushort c_Sof14Marker = 206;

	private const ushort c_Sof15Marker = 207;

	private byte[] m_jpegHeader = new byte[2] { 255, 216 };

	private PdfStream m_imageStream;

	private bool m_isContainsLittleEndian;

	private readonly byte[] m_jpegSegmentPreambleBytes = new byte[29]
	{
		104, 116, 116, 112, 58, 47, 47, 110, 115, 46,
		97, 100, 111, 98, 101, 46, 99, 111, 109, 47,
		120, 97, 112, 47, 49, 46, 48, 47, 0
	};

	private bool m_enableMetadata;

	private int m_noOfComponents = -1;

	public JpegDecoder(Stream stream)
	{
		base.InternalStream = stream;
		base.Format = ImageType.Jpeg;
		Initialize();
	}

	public JpegDecoder(Stream stream, bool enableMetadata)
	{
		base.InternalStream = stream;
		base.Format = ImageType.Jpeg;
		m_enableMetadata = enableMetadata;
		Initialize();
	}

	public static float[] GetImageResolution(Stream stream)
	{
		float[] array = new float[2] { 96f, 96f };
		byte[] array2 = new byte[stream.Length];
		stream.Read(array2, 0, array2.Length);
		int num = 0;
		int num2 = 2;
		if (BitConverter.ToUInt16(array2, num) == 55551)
		{
			num += num2;
			if ((ushort)(BitConverter.ToUInt16(array2, num) & 0xF0FF) == 57599)
			{
				num += num2;
				ushort num3 = BitConverter.ToUInt16(array2, num);
				num += num2;
				if (num3 >= 16)
				{
					num3 = ((num3 < array2.Length - num) ? num3 : ((ushort)(array2.Length - num)));
					byte[] array3 = new byte[num3 - 2];
					Array.Copy(array2, num, array3, 0, array3.Length);
					string @string = Encoding.UTF8.GetString(array3, 0, 5);
					if (@string == "JFIF\0")
					{
						ushort num4 = (ushort)((array3[8] << 8) | array3[9]);
						ushort num5 = (ushort)((array3[10] << 8) | array3[11]);
						if (num4 > 1)
						{
							array[0] = (int)num4;
						}
						if (num5 > 1)
						{
							array[1] = (int)num5;
						}
					}
					else if (@string == "Exif\0")
					{
						array3 = new byte[num3 - 2];
						Array.Copy(array2, 0, array3, 0, array3.Length);
						MemoryStream memoryStream = new MemoryStream(array3);
						memoryStream.Position = 12L;
						BinaryReader binaryReader = new BinaryReader(memoryStream);
						long position = memoryStream.Position;
						byte[] array4 = binaryReader.ReadBytes(2);
						bool flag = Encoding.UTF8.GetString(array4, 0, array4.Length) == "II";
						byte[] array5 = binaryReader.ReadBytes(2);
						if (flag != BitConverter.IsLittleEndian)
						{
							Array.Reverse(array5);
						}
						BitConverter.ToUInt16(array5, 0);
						array4 = binaryReader.ReadBytes(4);
						if (flag != BitConverter.IsLittleEndian)
						{
							Array.Reverse(array4);
						}
						uint num6 = BitConverter.ToUInt32(array4, 0);
						memoryStream.Position = num6 + position;
						array4 = binaryReader.ReadBytes(2);
						if (flag != BitConverter.IsLittleEndian)
						{
							Array.Reverse(array4);
						}
						ushort num7 = BitConverter.ToUInt16(array4, 0);
						long num8 = 0L;
						long num9 = 0L;
						for (ushort num10 = 0; num10 < num7; num10++)
						{
							array4 = binaryReader.ReadBytes(2);
							if (flag != BitConverter.IsLittleEndian)
							{
								Array.Reverse(array4);
							}
							ushort num11 = BitConverter.ToUInt16(array4, 0);
							if (num11 == 282)
							{
								num8 = memoryStream.Position - 2;
							}
							if (num11 == 283)
							{
								num9 = memoryStream.Position - 2;
							}
							memoryStream.Seek(10L, SeekOrigin.Current);
						}
						if (num8 >= 0)
						{
							memoryStream.Position = num8;
							array4 = binaryReader.ReadBytes(2);
							if (flag != BitConverter.IsLittleEndian)
							{
								Array.Reverse(array4);
							}
							if (BitConverter.ToUInt16(array4, 0) == 282)
							{
								array[0] = GetExifResolution(memoryStream, array4, flag, binaryReader);
							}
						}
						if (num9 >= 0)
						{
							memoryStream.Position = num9;
							array4 = binaryReader.ReadBytes(2);
							if (flag != BitConverter.IsLittleEndian)
							{
								Array.Reverse(array4);
							}
							if (BitConverter.ToUInt16(array4, 0) == 283)
							{
								array[1] = GetExifResolution(memoryStream, array4, flag, binaryReader);
							}
						}
						memoryStream.Dispose();
						memoryStream = null;
						binaryReader.Dispose();
						binaryReader = null;
					}
				}
			}
		}
		return array;
	}

	private static float GetExifResolution(Stream jpegStream, byte[] byteData, bool _isLittleEndian, BinaryReader jpegReader)
	{
		jpegStream.Position += 6L;
		byteData = jpegReader.ReadBytes(4);
		if (_isLittleEndian != BitConverter.IsLittleEndian)
		{
			Array.Reverse(byteData);
		}
		uint num = BitConverter.ToUInt32(byteData, 0);
		jpegStream.Position = num + 12;
		byteData = jpegReader.ReadBytes(4);
		if (_isLittleEndian != BitConverter.IsLittleEndian)
		{
			Array.Reverse(byteData);
		}
		uint num2 = BitConverter.ToUInt32(byteData, 0);
		byteData = jpegReader.ReadBytes(4);
		if (_isLittleEndian != BitConverter.IsLittleEndian)
		{
			Array.Reverse(byteData);
		}
		uint num3 = BitConverter.ToUInt32(byteData, 0);
		return (float)num2 / (float)num3;
	}

	protected override void Initialize()
	{
		base.InternalStream.Position = 0L;
		byte[] array = new byte[base.InternalStream.Length];
		base.InternalStream.Read(array, 0, array.Length);
		base.ImageData = array;
		ReaderHeader();
	}

	private void ReaderHeader()
	{
		base.InternalStream.Position = 0L;
		base.BitsPerComponent = 8;
		int ImageOrientation = 0;
		bool num = CheckForExifData(out ImageOrientation);
		base.JpegDecoderOrientationAngle = 0f;
		if (num)
		{
			switch (ImageOrientation)
			{
			case 1:
				base.JpegDecoderOrientationAngle = 0f;
				break;
			case 3:
				base.JpegDecoderOrientationAngle = 180f;
				break;
			case 6:
				base.JpegDecoderOrientationAngle = 90f;
				break;
			case 8:
				base.JpegDecoderOrientationAngle = 270f;
				break;
			default:
				base.JpegDecoderOrientationAngle = 0f;
				break;
			}
		}
		base.InternalStream.Position = 0L;
		base.BitsPerComponent = 8;
		byte[] array = new byte[base.InternalStream.Length];
		base.InternalStream.Read(array, 0, array.Length);
		long num2 = 4L;
		bool flag = false;
		long num3 = array[num2] * 256 + array[num2 + 1];
		while (num2 < array.Length)
		{
			num2 += num3;
			if (num2 < array.Length && m_enableMetadata)
			{
				if (array[num2 + 1] == 192)
				{
					base.Width = array[num2 + 7] * 256 + array[num2 + 8];
					base.Height = array[num2 + 5] * 256 + array[num2 + 6];
					m_noOfComponents = array[num2 + 9];
					if (base.Width != 0 && base.Height != 0)
					{
						return;
					}
					continue;
				}
				byte b = array[num2 + 1];
				num2 += 2;
				if (num2 < array.Length)
				{
					num3 = array[num2] * 256 + array[num2 + 1];
					if (b == 225 && m_enableMetadata)
					{
						ReadXmpSegment(new MemoryStream(array, (int)num2 + 2, (int)num3 - 2).ToArray());
					}
					continue;
				}
				return;
			}
			flag = true;
			break;
		}
		if (flag)
		{
			base.InternalStream.Position = 0L;
			base.InternalStream.Skip(2);
			ReadExceededJPGImage(base.InternalStream);
		}
	}

	private void ReadXmpSegment(byte[] bytes)
	{
		if (IsXmpSegment(bytes))
		{
			int num = m_jpegSegmentPreambleBytes.Length;
			if (base.MetadataStream == null)
			{
				base.MetadataStream = new MemoryStream();
			}
			base.MetadataStream.Write(bytes, num, bytes.Length - num);
		}
	}

	private bool IsXmpSegment(byte[] bytes)
	{
		int num = m_jpegSegmentPreambleBytes.Length;
		bool result = true;
		for (int i = 0; i < num; i++)
		{
			if (bytes[i] != m_jpegSegmentPreambleBytes[i])
			{
				result = false;
				break;
			}
		}
		return result;
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
		m_imageStream["Filter"] = new PdfName("DCTDecode");
		m_imageStream["ColorSpace"] = new PdfName(GetColorSpace());
		m_imageStream["DecodeParms"] = GetDecodeParams();
		return m_imageStream;
	}

	private bool CheckForExifData(out int ImageOrientation)
	{
		ImageOrientation = 0;
		Stream internalStream = base.InternalStream;
		BinaryReader binaryReader = new BinaryReader(internalStream);
		if (ConvertToUShort(ReadJpegBytes(2, binaryReader)) != 65496)
		{
			return false;
		}
		byte b = 0;
		byte b2;
		while ((b2 = binaryReader.ReadByte()) == byte.MaxValue && (b = binaryReader.ReadByte()) != 225)
		{
			int num = ConvertToUShort(ReadJpegBytes(2, binaryReader)) - 2;
			long num2 = internalStream.Position + num;
			internalStream.Seek(num, SeekOrigin.Current);
			if (internalStream.Position != num2 || internalStream.Position > internalStream.Length)
			{
				return false;
			}
		}
		if (b2 != byte.MaxValue || b != 225)
		{
			return false;
		}
		ConvertToUShort(ReadJpegBytes(2, binaryReader));
		byte[] array = ReadJpegBytes(4, binaryReader);
		if (Encoding.UTF8.GetString(array, 0, array.Length) != "Exif")
		{
			return false;
		}
		if (ConvertToUShort(ReadJpegBytes(2, binaryReader)) != 0)
		{
			return false;
		}
		long position = internalStream.Position;
		byte[] array2 = ReadJpegBytes(2, binaryReader);
		m_isContainsLittleEndian = Encoding.UTF8.GetString(array2, 0, array2.Length) == "II";
		if (ConvertToUShort(ReadJpegBytes(2, binaryReader)) != 42)
		{
			return false;
		}
		uint num3 = ConvertToUint(ReadJpegBytes(4, binaryReader));
		internalStream.Position = num3 + position;
		ushort num4 = ConvertToUShort(ReadJpegBytes(2, binaryReader));
		long num5 = 0L;
		for (ushort num6 = 0; num6 < num4; num6++)
		{
			if (ConvertToUShort(ReadJpegBytes(2, binaryReader)) == 274)
			{
				num5 = internalStream.Position - 2;
			}
			internalStream.Seek(10L, SeekOrigin.Current);
		}
		if (num5 >= 0)
		{
			internalStream.Position = num5;
			if (ConvertToUShort(ReadJpegBytes(2, binaryReader)) != 274)
			{
				return false;
			}
			ConvertToUShort(ReadJpegBytes(2, binaryReader));
			ConvertToUint(ReadJpegBytes(4, binaryReader));
			byte[] array3 = ReadJpegBytes(4, binaryReader);
			int num7 = 0;
			byte[] array4 = array3;
			foreach (byte b3 in array4)
			{
				if (b3 != 0)
				{
					num7 = b3;
					break;
				}
			}
			if (num7 == 0)
			{
				return false;
			}
			ImageOrientation = num7;
		}
		return true;
	}

	private byte[] ReadJpegBytes(int byteCount, BinaryReader reader)
	{
		byte[] array = reader.ReadBytes(byteCount);
		if (array.Length != byteCount)
		{
			throw new EndOfStreamException();
		}
		return array;
	}

	private ushort ConvertToUShort(byte[] data)
	{
		if (m_isContainsLittleEndian != BitConverter.IsLittleEndian)
		{
			Array.Reverse(data);
		}
		return BitConverter.ToUInt16(data, 0);
	}

	private uint ConvertToUint(byte[] data)
	{
		if (m_isContainsLittleEndian != BitConverter.IsLittleEndian)
		{
			Array.Reverse(data);
		}
		return BitConverter.ToUInt32(data, 0);
	}

	private string GetColorSpace()
	{
		string result = "DeviceRGB";
		if (m_noOfComponents == 1 || m_noOfComponents == 3 || m_noOfComponents == 4)
		{
			switch (m_noOfComponents)
			{
			case 1:
				return "DeviceGray";
			case 3:
				return "DeviceRGB";
			case 4:
				return "DeviceCMYK";
			}
		}
		else
		{
			int num = 0;
			int num2 = 2;
			if (BitConverter.ToUInt16(base.ImageData, num) == 55551)
			{
				num += num2;
				if ((ushort)(BitConverter.ToUInt16(base.ImageData, num) & 0xF0FF) == 57599)
				{
					while (true)
					{
						num += num2;
						int num3 = BitConverter.ToUInt16(base.ImageData, num);
						num3 = (num3 >> 8) | ((num3 << 8) & 0xFFFF);
						num += num3;
						switch (BitConverter.ToUInt16(base.ImageData, num))
						{
						default:
							continue;
						case 56063:
							num += num2 + 2;
							switch (base.ImageData[num])
							{
							case 1:
								return "DeviceGray";
							case 3:
								return "DeviceRGB";
							case 4:
								return "DeviceCMYK";
							}
							continue;
						case 55807:
							break;
						}
						break;
					}
				}
			}
		}
		return result;
	}

	private void ReadExceededJPGImage(Stream stream)
	{
		bool flag = true;
		while (flag)
		{
			switch (GetMarker(stream))
			{
			case 192:
			case 193:
			case 194:
			case 195:
			case 197:
			case 198:
			case 199:
			case 201:
			case 202:
			case 203:
			case 205:
			case 206:
			case 207:
				stream.ReadByte();
				stream.ReadByte();
				stream.ReadByte();
				base.Height = ReadNextTwoBytes(stream);
				base.Width = ReadNextTwoBytes(stream);
				m_noOfComponents = stream.ReadByte();
				flag = false;
				break;
			default:
				SkipStream(stream);
				break;
			}
		}
	}

	private ushort GetMarker(Stream stream)
	{
		int num = 0;
		int num2 = 32;
		num2 = stream.ReadByte();
		while (num2 != 0 && num2 != 255)
		{
			num++;
			num2 = stream.ReadByte();
		}
		do
		{
			num2 = (ushort)stream.ReadByte();
		}
		while (num2 == 255);
		if (num != 0)
		{
			throw new Exception("Error decoding JPEG image");
		}
		num2 = Convert.ToInt16(num2);
		return (ushort)num2;
	}

	private void SkipStream(Stream stream)
	{
		int num = ReadNextTwoBytes(stream);
		if (num < 2)
		{
			throw new Exception("Error decoding JPEG image");
		}
		for (num -= 2; num > 0; num--)
		{
			stream.ReadByte();
		}
	}

	private int ReadNextTwoBytes(Stream stream)
	{
		return (stream.ReadByte() << 8) | stream.ReadByte();
	}

	private PdfDictionary GetDecodeParams()
	{
		return new PdfDictionary
		{
			["Columns"] = new PdfNumber(base.Width),
			["BlackIs1"] = new PdfBoolean(value: true),
			["K"] = new PdfNumber(-1),
			["Predictor"] = new PdfNumber(15),
			["BitsPerComponent"] = new PdfNumber(base.BitsPerComponent)
		};
	}
}
