using System;
using System.IO;

namespace DocGen.CompoundFile.DocIO.Net;

internal class FileHeader
{
	public const int HeaderSize = 512;

	private const int SignatureSize = 8;

	private static readonly byte[] DefaultSignature = new byte[8] { 208, 207, 17, 224, 161, 177, 26, 225 };

	internal const int ShortSize = 2;

	internal const int IntSize = 4;

	private byte[] m_arrSignature = new byte[8];

	private Guid m_classId;

	private ushort m_usMinorVersion = 62;

	private ushort m_usDllVersion = 3;

	private ushort m_usByteOrder = 65534;

	private ushort m_usSectorShift = 9;

	private ushort m_usMiniSectorShift = 6;

	private ushort m_usReserved;

	private uint m_uiReserved1;

	private uint m_uiReserved2;

	private int m_iFatSectorsNumber;

	private int m_iDirectorySectorStart = -1;

	private int m_iSignature;

	private uint m_uiMiniSectorCutoff = 4096u;

	private int m_iMiniFastStart = -2;

	private int m_iMiniFatNumber;

	private int m_iDifStart = -2;

	private int m_iDifNumber;

	private int[] m_arrFatStart = new int[109];

	public int SectorSize => 1 << (int)m_usSectorShift;

	public ushort MinorVersion => m_usMinorVersion;

	public ushort DllVersion => m_usDllVersion;

	public ushort ByteOrder => m_usByteOrder;

	public ushort SectorShift => m_usSectorShift;

	public ushort MiniSectorShift => m_usMiniSectorShift;

	public ushort Reserved => m_usReserved;

	public uint Reserved1 => m_uiReserved1;

	public uint Reserved2 => m_uiReserved2;

	public int FatSectorsNumber
	{
		get
		{
			return m_iFatSectorsNumber;
		}
		set
		{
			m_iFatSectorsNumber = value;
		}
	}

	public int DirectorySectorStart
	{
		get
		{
			return m_iDirectorySectorStart;
		}
		set
		{
			m_iDirectorySectorStart = value;
		}
	}

	public int Signature => m_iSignature;

	public uint MiniSectorCutoff => m_uiMiniSectorCutoff;

	public int MiniFastStart
	{
		get
		{
			return m_iMiniFastStart;
		}
		set
		{
			m_iMiniFastStart = value;
		}
	}

	public int MiniFatNumber
	{
		get
		{
			return m_iMiniFatNumber;
		}
		set
		{
			m_iMiniFatNumber = value;
		}
	}

	public int DifStart
	{
		get
		{
			return m_iDifStart;
		}
		set
		{
			m_iDifStart = value;
		}
	}

	public int DifNumber
	{
		get
		{
			return m_iDifNumber;
		}
		set
		{
			m_iDifNumber = value;
		}
	}

	public int[] FatStart => m_arrFatStart;

	public FileHeader()
	{
		Buffer.BlockCopy(DefaultSignature, 0, m_arrSignature, 0, 8);
	}

	public FileHeader(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (stream.Length < 512)
		{
			throw new CompoundFileException();
		}
		byte[] array = new byte[512];
		stream.Read(array, 0, 512);
		Buffer.BlockCopy(array, 0, m_arrSignature, 0, 8);
		CheckSignature();
		int num = 8;
		byte[] array2 = new byte[16];
		Buffer.BlockCopy(array, num, array2, 0, 16);
		num += 16;
		m_classId = new Guid(array2);
		m_usMinorVersion = BitConverter.ToUInt16(array, num);
		num += 2;
		m_usDllVersion = BitConverter.ToUInt16(array, num);
		num += 2;
		m_usByteOrder = BitConverter.ToUInt16(array, num);
		num += 2;
		m_usSectorShift = BitConverter.ToUInt16(array, num);
		num += 2;
		m_usMiniSectorShift = BitConverter.ToUInt16(array, num);
		num += 2;
		m_usReserved = BitConverter.ToUInt16(array, num);
		num += 2;
		m_uiReserved1 = BitConverter.ToUInt32(array, num);
		num += 4;
		m_uiReserved2 = BitConverter.ToUInt32(array, num);
		num += 4;
		m_iFatSectorsNumber = BitConverter.ToInt32(array, num);
		num += 4;
		m_iDirectorySectorStart = BitConverter.ToInt32(array, num);
		num += 4;
		m_iSignature = BitConverter.ToInt32(array, num);
		num += 4;
		m_uiMiniSectorCutoff = BitConverter.ToUInt32(array, num);
		num += 4;
		m_iMiniFastStart = BitConverter.ToInt32(array, num);
		num += 4;
		m_iMiniFatNumber = BitConverter.ToInt32(array, num);
		num += 4;
		m_iDifStart = BitConverter.ToInt32(array, num);
		num += 4;
		m_iDifNumber = BitConverter.ToInt32(array, num);
		num += 4;
		Buffer.BlockCopy(array, num, m_arrFatStart, 0, m_arrFatStart.Length * 4);
	}

	public void Serialize(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		byte[] array = new byte[512];
		Buffer.BlockCopy(m_arrSignature, 0, array, 0, 8);
		int num = 8;
		Buffer.BlockCopy(m_classId.ToByteArray(), 0, array, num, 16);
		num += 16;
		WriteUInt16(array, num, m_usMinorVersion);
		num += 2;
		WriteUInt16(array, num, m_usDllVersion);
		num += 2;
		WriteUInt16(array, num, m_usByteOrder);
		num += 2;
		WriteUInt16(array, num, m_usSectorShift);
		num += 2;
		WriteUInt16(array, num, m_usMiniSectorShift);
		num += 2;
		WriteUInt16(array, num, m_usReserved);
		num += 2;
		WriteUInt32(array, num, m_uiReserved1);
		num += 4;
		WriteUInt32(array, num, m_uiReserved2);
		num += 4;
		WriteInt32(array, num, m_iFatSectorsNumber);
		num += 4;
		WriteInt32(array, num, m_iDirectorySectorStart);
		num += 4;
		WriteInt32(array, num, m_iSignature);
		num += 4;
		WriteUInt32(array, num, m_uiMiniSectorCutoff);
		num += 4;
		WriteInt32(array, num, m_iMiniFastStart);
		num += 4;
		WriteInt32(array, num, m_iMiniFatNumber);
		num += 4;
		WriteInt32(array, num, m_iDifStart);
		num += 4;
		WriteInt32(array, num, m_iDifNumber);
		num += 4;
		Buffer.BlockCopy(m_arrFatStart, 0, array, num, m_arrFatStart.Length * 4);
		stream.Write(array, 0, 512);
	}

	public static bool CheckSignature(Stream stream)
	{
		bool result = false;
		if (stream != null)
		{
			byte[] array = new byte[8];
			long position = stream.Position;
			if (stream.Read(array, 0, 8) == 8)
			{
				result = CheckSignature(array);
			}
			stream.Position = position;
		}
		return result;
	}

	private void CheckSignature()
	{
		if (!CheckSignature(m_arrSignature))
		{
			throw new CompoundFileException("Wrong signature");
		}
	}

	private static bool CheckSignature(byte[] arrSignature)
	{
		bool result = false;
		if (arrSignature != null && arrSignature.Length == 8)
		{
			result = true;
			for (int i = 0; i < 8; i++)
			{
				if (arrSignature[i] != DefaultSignature[i])
				{
					result = false;
					break;
				}
			}
		}
		return result;
	}

	private void WriteUInt16(byte[] buffer, int offset, ushort value)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		buffer[offset] = (byte)(value & 0xFFu);
		buffer[offset + 1] = (byte)((value & 0xFF00) >> 8);
	}

	private void WriteUInt32(byte[] buffer, int offset, uint value)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		buffer[offset] = (byte)(value & 0xFFu);
		value >>= 8;
		buffer[offset + 1] = (byte)(value & 0xFFu);
		value >>= 8;
		buffer[offset + 2] = (byte)(value & 0xFFu);
		value >>= 8;
		buffer[offset + 3] = (byte)(value & 0xFFu);
		value >>= 8;
	}

	private void WriteInt32(byte[] buffer, int offset, int value)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		buffer[offset] = (byte)((uint)value & 0xFFu);
		value >>= 8;
		buffer[offset + 1] = (byte)((uint)value & 0xFFu);
		value >>= 8;
		buffer[offset + 2] = (byte)((uint)value & 0xFFu);
		value >>= 8;
		buffer[offset + 3] = (byte)((uint)value & 0xFFu);
		value >>= 8;
	}

	internal void Write(Stream stream)
	{
		byte[] array = new byte[512];
		Buffer.BlockCopy(m_arrSignature, 0, array, 0, 8);
		int num = 8;
		Buffer.BlockCopy(m_classId.ToByteArray(), 0, array, num, 16);
		num += 16;
		Buffer.BlockCopy(BitConverter.GetBytes(m_usMinorVersion), 0, array, num, 2);
		num += 2;
		Buffer.BlockCopy(BitConverter.GetBytes(m_usDllVersion), 0, array, num, 2);
		num += 2;
		Buffer.BlockCopy(BitConverter.GetBytes(m_usByteOrder), 0, array, num, 2);
		num += 2;
		Buffer.BlockCopy(BitConverter.GetBytes(m_usSectorShift), 0, array, num, 2);
		num += 2;
		Buffer.BlockCopy(BitConverter.GetBytes(m_usMiniSectorShift), 0, array, num, 2);
		num += 2;
		Buffer.BlockCopy(BitConverter.GetBytes(m_usReserved), 0, array, num, 2);
		num += 2;
		Buffer.BlockCopy(BitConverter.GetBytes(m_uiReserved1), 0, array, num, 4);
		num += 4;
		Buffer.BlockCopy(BitConverter.GetBytes(m_uiReserved2), 0, array, num, 4);
		num += 4;
		Buffer.BlockCopy(BitConverter.GetBytes(m_iFatSectorsNumber), 0, array, num, 4);
		num += 4;
		Buffer.BlockCopy(BitConverter.GetBytes(m_iDirectorySectorStart), 0, array, num, 4);
		num += 4;
		Buffer.BlockCopy(BitConverter.GetBytes(m_iSignature), 0, array, num, 4);
		num += 4;
		Buffer.BlockCopy(BitConverter.GetBytes(m_uiMiniSectorCutoff), 0, array, num, 4);
		num += 4;
		Buffer.BlockCopy(BitConverter.GetBytes(m_iMiniFastStart), 0, array, num, 4);
		num += 4;
		Buffer.BlockCopy(BitConverter.GetBytes(m_iMiniFatNumber), 0, array, num, 4);
		num += 4;
		Buffer.BlockCopy(BitConverter.GetBytes(m_iDifStart), 0, array, num, 4);
		num += 4;
		Buffer.BlockCopy(BitConverter.GetBytes(m_iDifNumber), 0, array, num, 4);
		num += 4;
		Buffer.BlockCopy(m_arrFatStart, 0, array, num, m_arrFatStart.Length * 4);
		stream.Position = 0L;
		stream.Write(array, 0, 512);
	}

	internal long GetSectorOffset(int sectorIndex)
	{
		return (sectorIndex << (int)m_usSectorShift) + 512;
	}

	internal long GetSectorOffset(int sectorIndex, int headerSize)
	{
		return (sectorIndex << (int)m_usSectorShift) + headerSize;
	}
}
