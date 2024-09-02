using System;
using System.IO;
using System.Text;

namespace DocGen.OfficeChart.Parser.Biff_Records;

internal class ByteArrayDataProvider : DataProvider
{
	private byte[] m_arrData;

	public byte[] InternalBuffer => m_arrData;

	public override int Capacity => m_arrData.Length;

	public override bool IsCleared => m_arrData == null;

	public ByteArrayDataProvider()
		: this(new byte[128])
	{
	}

	public ByteArrayDataProvider(byte[] arrData)
	{
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		m_arrData = arrData;
	}

	public override byte ReadByte(int iOffset)
	{
		return m_arrData[iOffset];
	}

	public override short ReadInt16(int iOffset)
	{
		return BitConverter.ToInt16(m_arrData, iOffset);
	}

	public override int ReadInt32(int iOffset)
	{
		return BitConverter.ToInt32(m_arrData, iOffset);
	}

	public override long ReadInt64(int iOffset)
	{
		return BitConverter.ToInt64(m_arrData, iOffset);
	}

	public override void CopyTo(int iSourceOffset, byte[] arrDestination, int iDestOffset, int iLength)
	{
		Buffer.BlockCopy(m_arrData, iSourceOffset, arrDestination, iDestOffset, iLength);
	}

	public override void CopyTo(int iSourceOffset, DataProvider destination, int iDestOffset, int iLength)
	{
		if (iLength > 0)
		{
			destination.WriteBytes(iDestOffset, m_arrData, iSourceOffset, iLength);
		}
	}

	public override void Read(BinaryReader reader, int iOffset, int iLength, byte[] arrBuffer)
	{
		if (iOffset + iLength > m_arrData.Length)
		{
			throw new ArgumentOutOfRangeException();
		}
		reader.Read(m_arrData, iOffset, iLength);
	}

	public override string ReadString(int offset, int stringLength, Encoding encoding, bool isUnicode)
	{
		if (encoding == null)
		{
			encoding = (isUnicode ? Encoding.Unicode : Encoding.UTF8);
		}
		return encoding.GetString(m_arrData, offset, stringLength);
	}

	public override int EnsureCapacity(int size)
	{
		return EnsureCapacity(size, 0);
	}

	public override int EnsureCapacity(int size, int forceAdd)
	{
		int num = ((m_arrData != null) ? m_arrData.Length : 0);
		if (num < size)
		{
			byte[] array = new byte[size];
			if (num > 0 && m_arrData != null)
			{
				Buffer.BlockCopy(m_arrData, 0, array, 0, num);
			}
			m_arrData = array;
		}
		return num;
	}

	public override void ZeroMemory()
	{
		int i = 0;
		for (int num = m_arrData.Length; i < num; i++)
		{
			m_arrData[i] = 0;
		}
	}

	public override void WriteByte(int iOffset, byte value)
	{
		m_arrData[iOffset] = value;
	}

	public override void WriteInt16(int iOffset, short value)
	{
		BitConverter.GetBytes(value).CopyTo(m_arrData, iOffset);
	}

	[CLSCompliant(false)]
	public override void WriteUInt16(int iOffset, ushort value)
	{
		BitConverter.GetBytes(value).CopyTo(m_arrData, iOffset);
	}

	public override void WriteInt32(int iOffset, int value)
	{
		BitConverter.GetBytes(value).CopyTo(m_arrData, iOffset);
	}

	public override void WriteInt64(int iOffset, long value)
	{
		BitConverter.GetBytes(value).CopyTo(m_arrData, iOffset);
	}

	public override void WriteBit(int offset, bool value, int bitPos)
	{
		if (bitPos < 0 || bitPos > 7)
		{
			throw new ArgumentOutOfRangeException("bitPos", "Bit Position can be zero or greater than 7.");
		}
		if (value)
		{
			m_arrData[offset] |= (byte)(1 << bitPos);
		}
		else
		{
			m_arrData[offset] &= (byte)(~(1 << bitPos));
		}
	}

	public override void WriteDouble(int iOffset, double value)
	{
		BitConverter.GetBytes(value).CopyTo(m_arrData, iOffset);
	}

	public override void WriteStringNoLenUpdateOffset(ref int offset, string value, bool unicode)
	{
		if (value != null && value.Length != 0)
		{
			byte[] bytes = (unicode ? Encoding.Unicode : Encoding.UTF8).GetBytes(value);
			m_arrData[offset] = (unicode ? ((byte)1) : ((byte)0));
			offset++;
			int num = bytes.Length;
			Buffer.BlockCopy(bytes, 0, m_arrData, offset, num);
			offset += num;
		}
	}

	public override void WriteBytes(int offset, byte[] value, int pos, int length)
	{
		if (length != 0)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (pos < 0)
			{
				throw new ArgumentOutOfRangeException("pos", "Position cannot be zeroless.");
			}
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException("length", "Length of data to copy must be greater then zero.");
			}
			if (pos + length > value.Length)
			{
				throw new ArgumentOutOfRangeException("value", "Position or length has wrong value.");
			}
			Buffer.BlockCopy(value, pos, m_arrData, offset, length);
		}
	}

	public override void WriteInto(BinaryWriter writer, int iOffset, int iSize, byte[] arrBuffer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		writer.Write(m_arrData, iOffset, iSize);
	}

	internal void SetBuffer(byte[] arrNewBuffer)
	{
		if (arrNewBuffer == null)
		{
			throw new ArgumentNullException("arrNewBuffer");
		}
		m_arrData = arrNewBuffer;
	}

	public override void Clear()
	{
		m_arrData = null;
	}

	public override void MoveMemory(int iDestOffset, int iSourceOffset, int iMemorySize)
	{
		Buffer.BlockCopy(m_arrData, iSourceOffset, m_arrData, iDestOffset, iMemorySize);
	}

	public override void CopyMemory(int iDestOffset, int iSourceOffset, int iMemorySize)
	{
		Buffer.BlockCopy(m_arrData, iSourceOffset, m_arrData, iDestOffset, iMemorySize);
	}

	public override DataProvider CreateProvider()
	{
		return new ByteArrayDataProvider();
	}
}
