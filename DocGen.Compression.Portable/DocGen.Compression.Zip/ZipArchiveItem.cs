using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace DocGen.Compression.Zip;

public class ZipArchiveItem : IDisposable
{
	private string m_strItemName;

	private CompressionMethod m_compressionMethod = CompressionMethod.Deflated;

	private CompressionLevel m_compressionLevel = CompressionLevel.Normal;

	private uint m_uiCrc32;

	private Stream m_streamData;

	private long m_lCompressedSize;

	private long m_lOriginalSize;

	private bool m_bControlStream;

	private bool m_bCompressed;

	private long m_lCrcPosition;

	private int m_iLocalHeaderOffset;

	private GeneralPurposeBitFlags m_options;

	private int m_iExternalAttributes;

	private bool m_bCheckCrc;

	private bool m_bOptimizedDecompress;

	private ZipArchive m_archive;

	private const int MaxAnsiCode = 255;

	private byte[] m_actualCompression;

	private DateTime? m_lastModfied;

	public string ItemName
	{
		get
		{
			return m_strItemName;
		}
		set
		{
			if (value == null || value.Length == 0)
			{
				throw new ArgumentOutOfRangeException("ItemName");
			}
			m_strItemName = value;
		}
	}

	public CompressionMethod CompressionMethod
	{
		get
		{
			return m_compressionMethod;
		}
		set
		{
			m_compressionMethod = value;
		}
	}

	public CompressionLevel CompressionLevel
	{
		get
		{
			return m_compressionLevel;
		}
		set
		{
			if (m_compressionLevel != value)
			{
				if (m_bCompressed)
				{
					DecompressData();
				}
				m_compressionLevel = value;
			}
		}
	}

	[CLSCompliant(false)]
	public uint Crc32 => m_uiCrc32;

	public Stream DataStream
	{
		get
		{
			if (m_bCompressed)
			{
				DecompressData();
			}
			return m_streamData;
		}
	}

	public long CompressedSize => m_lCompressedSize;

	public long OriginalSize => m_lOriginalSize;

	public bool ControlStream => m_bControlStream;

	public bool Compressed => m_bCompressed;

	public FileAttributes ExternalAttributes
	{
		get
		{
			return (FileAttributes)m_iExternalAttributes;
		}
		set
		{
			m_iExternalAttributes = (int)value;
		}
	}

	public bool OptimizedDecompress
	{
		get
		{
			return m_bOptimizedDecompress;
		}
		set
		{
			m_bOptimizedDecompress = value;
		}
	}

	internal DateTime? LastModified
	{
		get
		{
			return m_lastModfied;
		}
		set
		{
			m_lastModfied = value;
		}
	}

	internal ZipArchiveItem(ZipArchive archive)
	{
		m_archive = archive;
	}

	public ZipArchiveItem(ZipArchive archive, string itemName, Stream streamData, bool controlStream, FileAttributes attributes)
		: this(archive)
	{
		m_strItemName = itemName;
		m_bControlStream = controlStream;
		m_streamData = streamData;
		m_iExternalAttributes = (int)attributes;
	}

	public void Update(Stream newDataStream, bool controlStream)
	{
		if (m_streamData != null && m_bControlStream && m_streamData != newDataStream)
		{
			m_streamData.Close();
		}
		m_bControlStream = controlStream;
		m_streamData = newDataStream;
		ResetFlags();
		m_lOriginalSize = newDataStream?.Length ?? 0;
	}

	public void ResetFlags()
	{
		m_lCompressedSize = 0L;
		m_lOriginalSize = 0L;
		m_bCompressed = false;
		m_uiCrc32 = 0u;
	}

	internal void Write(Stream outputStream)
	{
		if (m_streamData == null || m_streamData.Length == 0L)
		{
			m_compressionLevel = CompressionLevel.NoCompression;
			m_compressionMethod = CompressionMethod.Stored;
		}
		WriteHeader(outputStream);
		WriteZippedContent(outputStream);
		if (m_archive.Password != null && m_streamData != null)
		{
			byte[] array = new byte[m_lCompressedSize];
			outputStream.Position = (int)(outputStream.Position - m_lCompressedSize);
			for (int i = 0; i < m_lCompressedSize; i++)
			{
				array[i] = (byte)outputStream.ReadByte();
			}
			byte[] array2 = Encrypt(array);
			outputStream.Position = (int)(outputStream.Position - m_lCompressedSize);
			for (int j = 0; j < array2.Length; j++)
			{
				outputStream.WriteByte(array2[j]);
			}
			m_lCompressedSize = array2.Length;
		}
		if (m_uiCrc32 == 0)
		{
			m_bCheckCrc = false;
		}
		if (m_uiCrc32 == 0 && m_archive.Password == null && DataStream != null)
		{
			long num = DataStream.Length;
			byte[] buffer = new byte[4096];
			Stream dataStream = DataStream;
			while (num > 0)
			{
				int num2 = dataStream.Read(buffer, 0, 4096);
				num -= num2;
				m_uiCrc32 = ZipCrc32.ComputeCrc(buffer, 0, num2, m_uiCrc32);
			}
			m_bCheckCrc = true;
		}
		WriteFooter(outputStream);
	}

	internal void Close()
	{
		if (m_streamData != null)
		{
			if (m_bControlStream)
			{
				m_streamData.Close();
			}
			m_streamData = null;
			m_strItemName = null;
		}
	}

	internal void WriteFileHeader(Stream stream)
	{
		stream.Write(BitConverter.GetBytes(33639248), 0, 4);
		stream.Write(BitConverter.GetBytes((short)45), 0, 2);
		if (m_compressionMethod == (CompressionMethod)99)
		{
			stream.Write(BitConverter.GetBytes((short)51), 0, 2);
		}
		else
		{
			stream.Write(BitConverter.GetBytes((short)20), 0, 2);
		}
		if (m_compressionMethod == (CompressionMethod)99 && m_archive.Password != null)
		{
			m_options = (GeneralPurposeBitFlags)1;
		}
		stream.Write(BitConverter.GetBytes((short)m_options), 0, 2);
		stream.Write(BitConverter.GetBytes((short)m_compressionMethod), 0, 2);
		int num = 0;
		num = ((!LastModified.HasValue) ? ConvertDateTime(DateTime.Now) : ConvertDateTime(LastModified.Value));
		stream.Write(new byte[4]
		{
			(byte)((uint)num & 0xFFu),
			(byte)((num & 0xFF00) >> 8),
			(byte)((num & 0xFF0000) >> 16),
			(byte)((num & 0xFF000000u) >> 24)
		}, 0, 4);
		stream.Write(BitConverter.GetBytes(m_uiCrc32), 0, 4);
		stream.Write(BitConverter.GetBytes((int)m_lCompressedSize), 0, 4);
		stream.Write(BitConverter.GetBytes((int)m_lOriginalSize), 0, 4);
		Encoding obj = (((m_options & GeneralPurposeBitFlags.Unicode) != 0) ? Encoding.UTF8 : new LatinEncoding());
		byte[] bytes = obj.GetBytes(m_strItemName);
		int byteCount = obj.GetByteCount(m_strItemName);
		stream.Write(BitConverter.GetBytes((short)byteCount), 0, 2);
		if (m_compressionMethod == (CompressionMethod)99)
		{
			stream.WriteByte(47);
		}
		else
		{
			stream.WriteByte(0);
		}
		stream.WriteByte(0);
		stream.WriteByte(0);
		stream.WriteByte(0);
		stream.WriteByte(0);
		stream.WriteByte(0);
		stream.WriteByte(0);
		stream.WriteByte(0);
		stream.Write(BitConverter.GetBytes(m_iExternalAttributes), 0, 4);
		stream.Write(BitConverter.GetBytes(m_iLocalHeaderOffset), 0, 4);
		stream.Write(bytes, 0, bytes.Length);
		if (m_compressionMethod == (CompressionMethod)99 && m_archive.Password != null)
		{
			stream.Position += 36L;
			WriteEncryptionHeader(stream);
		}
	}

	private int ConvertDateTime(DateTime time)
	{
		time = time.ToLocalTime();
		ushort num = (ushort)((time.Day & 0x1F) | ((time.Month << 5) & 0x1E0) | ((time.Year - 1980 << 9) & 0xFE00));
		ushort num2 = (ushort)(((uint)(time.Second / 2) & 0x1Fu) | ((uint)(time.Minute << 5) & 0x7E0u) | ((uint)(time.Hour << 11) & 0xF800u));
		return (num << 16) | num2;
	}

	private DateTime ConvertToDateTime(int dateTimeValue)
	{
		uint num = (uint)(dateTimeValue >> 16);
		int num2 = (int)(dateTimeValue & num);
		ushort num3 = (ushort)((num2 & 0x1F) * 2);
		ushort num4 = (ushort)((uint)(num2 & 0x7E0) >> 5);
		ushort num5 = (ushort)((uint)(num2 & 0xF800) >> 11);
		ushort num6 = (ushort)(num & 0x1Fu);
		ushort num7 = (ushort)((num & 0x1E0) >> 5);
		ushort num8 = (ushort)(((num & 0xFE00) >> 9) + 1980);
		if (num8 <= 0 || num8 > 9999 || num7 <= 0 || num7 > 12 || num6 <= 0 || !CheckValidDate(num8, num7, num6) || num5 < 0 || num5 > 24 || num4 < 0 || num4 > 60 || num3 < 0 || num3 > 60)
		{
			return DateTime.Now;
		}
		if (num3 == 60)
		{
			num4++;
			num3 = 0;
		}
		if (num4 >= 60)
		{
			num5++;
			num4 -= 60;
		}
		if (num5 >= 24)
		{
			DateTime dateTime = new DateTime(num8, num7, num6);
			dateTime.AddDays(1.0);
			num6 = (ushort)dateTime.Day;
			num7 = (ushort)dateTime.Month;
			num8 = (ushort)dateTime.Year;
			num5 -= 24;
		}
		return new DateTime(num8, num7, num6, num5, num4, num3);
	}

	internal bool CheckValidDate(ushort year, ushort month, ushort day)
	{
		switch (month)
		{
		case 1:
			return day <= 31;
		case 2:
			if ((year % 4 == 0 && year % 100 != 0) || year % 400 == 0)
			{
				return day <= 29;
			}
			return day <= 28;
		case 3:
			return day <= 31;
		case 4:
			return day <= 30;
		case 5:
			return day <= 31;
		case 6:
			return day <= 30;
		case 7:
			return day <= 31;
		case 8:
			return day <= 31;
		case 9:
			return day <= 30;
		case 10:
			return day <= 31;
		case 11:
			return day <= 30;
		case 12:
			return day <= 31;
		default:
			return false;
		}
	}

	internal void ReadCentralDirectoryData(Stream stream)
	{
		stream.Position += 4L;
		m_options = (GeneralPurposeBitFlags)ZipArchive.ReadInt16(stream);
		if (m_options == (GeneralPurposeBitFlags)1 && m_archive.Password == null)
		{
			throw new Exception("Password required");
		}
		if (m_options == (GeneralPurposeBitFlags)1)
		{
			m_archive.EncryptionAlgorithm = EncryptionAlgorithm.ZipCrypto;
		}
		m_compressionMethod = (CompressionMethod)ZipArchive.ReadInt16(stream);
		m_bCheckCrc = m_compressionMethod != (CompressionMethod)99;
		m_bCompressed = true;
		int dateTimeValue = ZipArchive.ReadInt32(stream);
		LastModified = ConvertToDateTime(dateTimeValue);
		m_uiCrc32 = (uint)ZipArchive.ReadInt32(stream);
		m_lCompressedSize = ZipArchive.ReadInt32(stream);
		m_lOriginalSize = ZipArchive.ReadInt32(stream);
		int num = ZipArchive.ReadInt16(stream);
		int num2 = ZipArchive.ReadInt16(stream);
		int num3 = ZipArchive.ReadInt16(stream);
		stream.Position += 4L;
		m_iExternalAttributes = ZipArchive.ReadInt32(stream);
		m_iLocalHeaderOffset = ZipArchive.ReadInt32(stream);
		byte[] array = new byte[num];
		stream.Read(array, 0, num);
		Encoding encoding = (((m_options & GeneralPurposeBitFlags.Unicode) != 0) ? Encoding.UTF8 : new LatinEncoding());
		m_strItemName = encoding.GetString(array, 0, array.Length);
		m_strItemName = m_strItemName.Replace("\\", "/");
		if (m_strItemName.ToLower().StartsWith("customxml"))
		{
			m_strItemName = "customXml" + m_strItemName.Remove(0, 9);
		}
		stream.Position += num2 + num3;
		if (m_options != 0)
		{
			m_options = (GeneralPurposeBitFlags)0;
		}
	}

	internal void ReadData(Stream stream, bool checkCrc)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		stream.Position = m_iLocalHeaderOffset;
		m_bCheckCrc = checkCrc;
		ReadLocalHeader(stream);
		ReadCompressedData(stream);
	}

	private void ReadCompressedData(Stream stream)
	{
		if (m_lCompressedSize > 0)
		{
			MemoryStream memoryStream = new MemoryStream();
			int num2 = (memoryStream.Capacity = (int)m_lCompressedSize);
			byte[] buffer = new byte[4096];
			while (num2 > 0)
			{
				int num3 = Math.Min(num2, 4096);
				if (stream.Read(buffer, 0, num3) != num3)
				{
					throw new ZipException("End of file reached - wrong file format or file is corrupt.");
				}
				memoryStream.Write(buffer, 0, num3);
				num2 -= num3;
			}
			if (m_archive.Password != null)
			{
				byte[] array = new byte[memoryStream.Length];
				array = memoryStream.ToArray();
				memoryStream = new MemoryStream(Decrypt(array));
			}
			m_streamData = memoryStream;
			m_bControlStream = true;
		}
		else if (m_lCompressedSize < 0)
		{
			MemoryStream memoryStream2 = new MemoryStream();
			int num4 = 0;
			bool flag = true;
			while (flag)
			{
				if ((num4 = stream.ReadByte()) == 80)
				{
					stream.Position--;
					int num5 = ZipArchive.ReadInt32(stream);
					if (num5 == 33639248 || num5 == 33639248)
					{
						flag = false;
					}
					stream.Position -= 3L;
				}
				if (flag)
				{
					memoryStream2.WriteByte((byte)num4);
				}
			}
			m_streamData = memoryStream2;
			m_lCompressedSize = m_streamData.Length;
			m_bControlStream = true;
		}
		else if (m_lCompressedSize == 0L)
		{
			m_streamData = new MemoryStream();
		}
	}

	private void ReadLocalHeader(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (ZipArchive.ReadInt32(stream) != 67324752)
		{
			throw new ZipException("Can't find local header signature - wrong file format or file is corrupt.");
		}
		stream.Position += 22L;
		int num = ZipArchive.ReadInt16(stream);
		int num2 = ZipArchive.ReadUInt16(stream);
		if (m_compressionMethod == (CompressionMethod)99)
		{
			stream.Position += num + 8;
			m_archive.EncryptionAlgorithm = (EncryptionAlgorithm)stream.ReadByte();
			m_actualCompression = new byte[2];
			stream.Read(m_actualCompression, 0, 2);
		}
		else if (num2 > 2)
		{
			stream.Position += num;
			if (ZipArchive.ReadInt16(stream) == 23)
			{
				throw new Exception("UnSupported");
			}
			stream.Position += num2 - 2;
		}
		else
		{
			stream.Position += num + num2;
		}
	}

	private void DecompressData()
	{
		if (!m_bCompressed)
		{
			return;
		}
		if (m_compressionMethod == CompressionMethod.Deflated)
		{
			if (m_lOriginalSize > 0)
			{
				m_streamData.Position = 0L;
				if (m_bOptimizedDecompress)
				{
					DecompressDataMemoryOptimized();
				}
				else
				{
					DecompressDataOrdinary();
				}
			}
			else
			{
				m_streamData.Position = 0L;
				if (m_streamData.Length > 0)
				{
					DecompressDataOld();
				}
			}
			m_bCompressed = false;
		}
		else
		{
			if (m_compressionMethod != 0)
			{
				throw new NotSupportedException("Compression type: " + m_compressionMethod.ToString() + " is not supported");
			}
			m_bCompressed = false;
		}
	}

	private void DecompressDataOld()
	{
		CompressedStreamReader compressedStreamReader = new CompressedStreamReader(m_streamData, bNoWrap: true);
		MemoryStream memoryStream = new MemoryStream();
		if (m_lOriginalSize > 0)
		{
			memoryStream.Capacity = (int)m_lOriginalSize;
		}
		byte[] buffer = new byte[4096];
		int count;
		while ((count = compressedStreamReader.Read(buffer, 0, 4096)) > 0)
		{
			memoryStream.Write(buffer, 0, count);
		}
		if (m_bControlStream)
		{
			m_streamData.Close();
		}
		m_lOriginalSize = memoryStream.Length;
		m_bControlStream = true;
		m_streamData = memoryStream;
		memoryStream.SetLength(m_lOriginalSize);
		memoryStream.Capacity = (int)m_lOriginalSize;
		if (m_bCheckCrc)
		{
			CheckCrc(memoryStream.ToArray());
		}
		m_streamData.Position = 0L;
	}

	private void DecompressDataMemoryOptimized()
	{
		m_streamData = new DeflateStream(m_streamData, CompressionMode.Decompress, m_bControlStream);
	}

	private void DecompressDataOrdinary()
	{
		DeflateStream deflateStream = new DeflateStream(m_streamData, CompressionMode.Decompress, leaveOpen: true);
		MemoryStream memoryStream = new MemoryStream();
		if (m_lOriginalSize > 0)
		{
			memoryStream.Capacity = (int)m_lOriginalSize;
		}
		byte[] buffer = new byte[4096];
		bool flag = false;
		int count;
		while ((count = deflateStream.Read(buffer, 0, 4096)) > 0)
		{
			flag = true;
			memoryStream.Write(buffer, 0, count);
		}
		deflateStream.Dispose();
		if (!flag)
		{
			m_streamData.Position = 0L;
			DecompressDataOld();
			return;
		}
		if (m_bControlStream)
		{
			m_streamData.Close();
		}
		if (m_lOriginalSize < 0)
		{
			m_lOriginalSize = memoryStream.Length;
		}
		m_bControlStream = true;
		m_streamData = memoryStream;
		memoryStream.SetLength(m_lOriginalSize);
		memoryStream.Capacity = (int)m_lOriginalSize;
		if (m_bCheckCrc)
		{
			CheckCrc(memoryStream.ToArray());
		}
		m_streamData.Position = 0L;
	}

	private void WriteHeader(Stream outputStream)
	{
		m_iLocalHeaderOffset = (int)outputStream.Position;
		outputStream.Write(BitConverter.GetBytes(67324752), 0, 4);
		if (m_archive.Password != null)
		{
			outputStream.Write(BitConverter.GetBytes((short)51), 0, 2);
		}
		else
		{
			outputStream.Write(BitConverter.GetBytes((short)20), 0, 2);
		}
		if (!IsIBM437Encoding(ItemName))
		{
			m_options |= GeneralPurposeBitFlags.Unicode;
		}
		if (m_archive.Password != null)
		{
			m_options = (GeneralPurposeBitFlags)1;
			EncryptionAlgorithm encryptionAlgorithm = m_archive.EncryptionAlgorithm;
			if ((uint)(encryptionAlgorithm - 1) <= 2u && m_streamData != null)
			{
				m_compressionMethod = (CompressionMethod)99;
				m_uiCrc32 = 0u;
			}
		}
		outputStream.Write(BitConverter.GetBytes((short)m_options), 0, 2);
		outputStream.Write(BitConverter.GetBytes((short)m_compressionMethod), 0, 2);
		int num = 0;
		num = ((!LastModified.HasValue) ? ConvertDateTime(DateTime.Now) : ConvertDateTime(LastModified.Value));
		outputStream.Write(new byte[4]
		{
			(byte)((uint)num & 0xFFu),
			(byte)((num & 0xFF00) >> 8),
			(byte)((num & 0xFF0000) >> 16),
			(byte)((num & 0xFF000000u) >> 24)
		}, 0, 4);
		m_lCrcPosition = outputStream.Position;
		outputStream.Write(BitConverter.GetBytes(m_uiCrc32), 0, 4);
		outputStream.Write(BitConverter.GetBytes((int)m_lCompressedSize), 0, 4);
		outputStream.Write(BitConverter.GetBytes((int)m_lOriginalSize), 0, 4);
		Encoding obj = (((m_options & GeneralPurposeBitFlags.Unicode) != 0) ? Encoding.UTF8 : new LatinEncoding());
		int byteCount = obj.GetByteCount(m_strItemName);
		outputStream.Write(BitConverter.GetBytes((short)byteCount), 0, 2);
		if (m_compressionMethod == (CompressionMethod)99 && m_archive.Password != null)
		{
			outputStream.WriteByte(11);
		}
		else
		{
			outputStream.WriteByte(0);
		}
		outputStream.WriteByte(0);
		byte[] bytes = obj.GetBytes(m_strItemName);
		outputStream.Write(bytes, 0, bytes.Length);
		if (m_actualCompression != null && !m_bCompressed && m_compressionMethod == (CompressionMethod)99)
		{
			m_actualCompression = BitConverter.GetBytes((short)8);
		}
		if (m_compressionMethod == (CompressionMethod)99 && m_archive.Password != null)
		{
			WriteEncryptionHeader(outputStream);
		}
	}

	private void WriteZippedContent(Stream outputStream)
	{
		long num = ((m_streamData != null) ? m_streamData.Length : 0);
		if (num <= 0)
		{
			return;
		}
		long position = outputStream.Position;
		if (m_bCompressed || m_compressionMethod == CompressionMethod.Stored)
		{
			m_streamData.Position = 0L;
			byte[] buffer = new byte[4096];
			while (num > 0)
			{
				int num2 = m_streamData.Read(buffer, 0, 4096);
				outputStream.Write(buffer, 0, num2);
				num -= num2;
				if (m_compressionMethod == CompressionMethod.Stored && m_uiCrc32 == 0)
				{
					m_uiCrc32 = ZipCrc32.ComputeCrc(buffer, 0, num2, m_uiCrc32);
				}
			}
		}
		else if (m_compressionMethod == CompressionMethod.Deflated || m_compressionMethod == (CompressionMethod)99)
		{
			m_lOriginalSize = num;
			m_streamData.Position = 0L;
			m_uiCrc32 = 0u;
			byte[] buffer2 = new byte[4096];
			Stream stream = m_archive.CreateCompressor(outputStream);
			while (num > 0)
			{
				int num3 = m_streamData.Read(buffer2, 0, 4096);
				stream.Write(buffer2, 0, num3);
				num -= num3;
				m_uiCrc32 = ZipCrc32.ComputeCrc(buffer2, 0, num3, m_uiCrc32);
			}
			stream.Flush();
			stream.Close();
		}
		m_lCompressedSize = outputStream.Position - position;
	}

	private void WriteFooter(Stream outputStream)
	{
		if (outputStream == null)
		{
			throw new ArgumentNullException("outputStream");
		}
		long position = outputStream.Position;
		outputStream.Position = m_lCrcPosition;
		outputStream.Write(BitConverter.GetBytes(m_uiCrc32), 0, 4);
		outputStream.Write(BitConverter.GetBytes((int)m_lCompressedSize), 0, 4);
		outputStream.Write(BitConverter.GetBytes((int)m_lOriginalSize), 0, 4);
		outputStream.Position = position;
	}

	private void CheckCrc()
	{
		m_streamData.Position = 0L;
		if (ZipCrc32.ComputeCrc(m_streamData, (int)m_lOriginalSize) != m_uiCrc32)
		{
			throw new ZipException("Wrong Crc value.");
		}
	}

	private void CheckCrc(byte[] arrData)
	{
		if (ZipCrc32.ComputeCrc(arrData, 0, (int)m_lOriginalSize, 0u) != m_uiCrc32)
		{
			throw new ZipException("Wrong Crc value.");
		}
	}

	public ZipArchiveItem Clone()
	{
		ZipArchiveItem obj = (ZipArchiveItem)MemberwiseClone();
		obj.m_streamData = CloneStream(m_streamData);
		return obj;
	}

	public static Stream CloneStream(Stream stream)
	{
		if (stream == null)
		{
			return null;
		}
		long position = stream.Position;
		MemoryStream memoryStream = new MemoryStream((int)stream.Length);
		stream.Position = 0L;
		byte[] buffer = new byte[32768];
		int count;
		while ((count = stream.Read(buffer, 0, 32768)) != 0)
		{
			memoryStream.Write(buffer, 0, count);
		}
		stream.Position = position;
		memoryStream.Position = position;
		return memoryStream;
	}

	private bool CheckForLatin(string text)
	{
		char[] array = null;
		array = text.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if ((array[i] >= '\0' && array[i] <= '\u007f') || (array[i] >= '\u0080' && array[i] <= 'ÿ') || (array[i] >= 'Ā' && array[i] <= 'ſ') || (array[i] >= 'ƀ' && array[i] <= 'ɏ') || (array[i] >= 'Ḁ' && array[i] <= 'ỿ') || (array[i] >= 'Ⱡ' && array[i] <= 'Ɀ') || (array[i] >= '\ua720' && array[i] <= 'ꟿ'))
			{
				return true;
			}
		}
		return false;
	}

	public void Dispose()
	{
		if (m_strItemName != null)
		{
			Close();
			m_strItemName = null;
		}
		if (m_actualCompression != null)
		{
			m_actualCompression = null;
		}
		GC.SuppressFinalize(this);
	}

	~ZipArchiveItem()
	{
		Dispose();
	}

	internal void WriteEncryptionHeader(Stream stream)
	{
		byte[] aesEncryptionHeader = SecurityConstants.AesEncryptionHeader;
		stream.Write(aesEncryptionHeader, 0, aesEncryptionHeader.Length);
		stream.WriteByte((byte)m_archive.EncryptionAlgorithm);
		if (m_actualCompression == null)
		{
			m_actualCompression = BitConverter.GetBytes((short)8);
		}
		stream.Write(m_actualCompression, 0, m_actualCompression.Length);
	}

	internal byte[] Encrypt(byte[] plainData)
	{
		if (m_archive.EncryptionAlgorithm == EncryptionAlgorithm.ZipCrypto)
		{
			return new ZipCrypto(m_streamData, m_archive.Password, m_uiCrc32).Encrypt(plainData);
		}
		return new Aes(m_archive.EncryptionAlgorithm, m_archive.Password).Encrypt(plainData);
	}

	internal byte[] Decrypt(byte[] cipherData)
	{
		if (m_compressionMethod != (CompressionMethod)99)
		{
			return new ZipCrypto(m_archive.Password, m_uiCrc32).Decrypt(cipherData);
		}
		byte[] result = new Aes(m_archive.EncryptionAlgorithm, m_archive.Password).Decrypt(cipherData);
		m_compressionMethod = (CompressionMethod)BitConverter.ToInt16(m_actualCompression, 0);
		return result;
	}

	private bool IsIBM437Encoding(string fileName)
	{
		if (fileName == null || fileName == string.Empty)
		{
			throw new ArgumentException("fileName");
		}
		LatinEncoding latinEncoding = new LatinEncoding();
		byte[] bytes = latinEncoding.GetBytes(fileName.ToCharArray());
		return latinEncoding.GetString(bytes, 0, bytes.Length) == fileName;
	}

	internal static byte[] CreateRandom(int length)
	{
		if (length <= 0)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		byte[] array = new byte[length];
		Random random = new Random((int)DateTime.Now.Ticks);
		int maxValue = 256;
		for (int i = 0; i < length; i++)
		{
			array[i] = (byte)random.Next(maxValue);
		}
		return array;
	}
}
