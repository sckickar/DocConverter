using System;
using System.IO;
using System.Text;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records;

internal abstract class DataProvider : IDisposable
{
	public abstract int Capacity { get; }

	public abstract bool IsCleared { get; }

	public DataProvider()
	{
	}

	~DataProvider()
	{
		Dispose(isDisposing: false);
	}

	public bool ReadBit(int iOffset, int iBit)
	{
		if (iBit < 0 || iBit > 7)
		{
			throw new ArgumentOutOfRangeException("iBit", "Bit Position cannot be less than 0 or greater than 7.");
		}
		return (ReadByte(iOffset) & (1 << iBit)) == 1 << iBit;
	}

	public abstract byte ReadByte(int iOffset);

	public bool ReadBoolean(int iOffset)
	{
		return ReadByte(iOffset) != 0;
	}

	public abstract short ReadInt16(int iOffset);

	[CLSCompliant(false)]
	public ushort ReadUInt16(int iOffset)
	{
		return (ushort)ReadInt16(iOffset);
	}

	public abstract int ReadInt32(int iOffset);

	[CLSCompliant(false)]
	public uint ReadUInt32(int iOffset)
	{
		return (uint)ReadInt32(iOffset);
	}

	public abstract long ReadInt64(int iOffset);

	public virtual double ReadDouble(int iOffset)
	{
		return BitConverterGeneral.Int64BitsToDouble(ReadInt64(iOffset));
	}

	public abstract void CopyTo(int iSourceOffset, byte[] arrDestination, int iDestOffset, int iLength);

	public virtual void CopyTo(int iSourceOffset, DataProvider destination, int iDestOffset, int iLength)
	{
		throw new NotImplementedException();
	}

	public abstract void Read(BinaryReader reader, int iOffset, int iLength, byte[] arrBuffer);

	public virtual string ReadString16Bit(int iOffset, out int iFullLength)
	{
		ushort num = ReadUInt16(iOffset);
		iOffset += 2;
		bool flag = ReadBoolean(iOffset);
		iOffset++;
		iFullLength = (flag ? (3 + num * 2) : (3 + num));
		int stringLength = (flag ? (num * 2) : num);
		Encoding encoding = (flag ? Encoding.Unicode : BiffRecordRaw.LatinEncoding);
		return ReadString(iOffset, stringLength, encoding, flag);
	}

	public virtual string ReadString16BitUpdateOffset(ref int iOffset)
	{
		int iFullLength;
		string result = ReadString16Bit(iOffset, out iFullLength);
		iOffset += iFullLength;
		return result;
	}

	public virtual string ReadString8Bit(int iOffset, out int iFullLength)
	{
		ushort num = ReadByte(iOffset);
		iOffset++;
		bool num2 = ReadBoolean(iOffset);
		iOffset++;
		int num3 = (num2 ? (num * 2) : num);
		iFullLength = 2 + num3;
		byte[] array = new byte[num3];
		CopyTo(iOffset, array, 0, num3);
		return (num2 ? Encoding.Unicode : BiffRecordRaw.LatinEncoding).GetString(array, 0, num3);
	}

	public int ReadArray(int iOffset, byte[] arrDest)
	{
		if (arrDest == null)
		{
			throw new ArgumentNullException("arrDest");
		}
		int num = arrDest.Length;
		CopyTo(iOffset, arrDest, 0, num);
		return iOffset + num;
	}

	public int ReadArray(int iOffset, byte[] arrDest, int size)
	{
		if (arrDest == null)
		{
			throw new ArgumentNullException("arrDest");
		}
		CopyTo(iOffset, arrDest, 0, size);
		return iOffset + size;
	}

	public string ReadString(int offset, int iStrLen, out int iBytesInString, bool isByteCounted)
	{
		bool flag = ReadByte(offset) != 0;
		iBytesInString = ((flag && !isByteCounted) ? (iStrLen * 2) : iStrLen);
		byte[] array = new byte[iBytesInString];
		ReadArray(offset + 1, array);
		return (flag ? Encoding.Unicode : BiffRecordRaw.LatinEncoding).GetString(array, 0, array.Length);
	}

	public string ReadStringUpdateOffset(ref int offset, int iStrLen)
	{
		if (iStrLen > 0)
		{
			int iBytesInString;
			string result = ReadString(offset, iStrLen, out iBytesInString, isByteCounted: false);
			offset += iBytesInString + 1;
			return result;
		}
		return string.Empty;
	}

	public abstract string ReadString(int offset, int stringLength, Encoding encoding, bool isUnicode);

	[CLSCompliant(false)]
	public TAddr ReadAddr(int offset)
	{
		TAddr result = default(TAddr);
		result.FirstRow = ReadUInt16(offset);
		result.LastRow = ReadUInt16(offset + 2);
		result.FirstCol = ReadUInt16(offset + 4);
		result.LastCol = ReadUInt16(offset + 6);
		return result;
	}

	public Rectangle ReadAddrAsRectangle(int offset)
	{
		int top = ReadUInt16(offset);
		int bottom = ReadUInt16(offset + 2);
		ushort left = ReadUInt16(offset + 4);
		int right = ReadUInt16(offset + 6);
		return Rectangle.FromLTRB(left, top, right, bottom);
	}

	public virtual void WriteInto(BinaryWriter writer, int iOffset, int iSize, byte[] arrBuffer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		int num = iSize;
		int val = arrBuffer.Length;
		while (num > 0)
		{
			int num2 = Math.Min(val, num);
			CopyTo(iOffset, arrBuffer, 0, num2);
			writer.Write(arrBuffer, 0, num2);
			iOffset += num2;
			num -= num2;
		}
	}

	public abstract void WriteByte(int iOffset, byte value);

	public abstract void WriteInt16(int iOffset, short value);

	[CLSCompliant(false)]
	public virtual void WriteUInt16(int iOffset, ushort value)
	{
		throw new NotImplementedException();
	}

	public abstract void WriteInt32(int iOffset, int value);

	public abstract void WriteInt64(int iOffset, long value);

	[CLSCompliant(false)]
	public void WriteUInt32(int iOffset, uint value)
	{
		WriteInt32(iOffset, (int)value);
	}

	public abstract void WriteBit(int offset, bool value, int bitPos);

	public abstract void WriteDouble(int iOffset, double value);

	public void WriteString8BitUpdateOffset(ref int offset, string value)
	{
		WriteByte(offset, (byte)value.Length);
		offset++;
		WriteStringNoLenUpdateOffset(ref offset, value);
	}

	public void WriteString16BitUpdateOffset(ref int offset, string value)
	{
		WriteString16BitUpdateOffset(ref offset, value, isUnicode: true);
	}

	public void WriteString16BitUpdateOffset(ref int offset, string value, bool isUnicode)
	{
		int num = value?.Length ?? 0;
		WriteUInt16(offset, (ushort)num);
		offset += 2;
		WriteStringNoLenUpdateOffset(ref offset, value, isUnicode);
	}

	public int WriteString16Bit(int offset, string value)
	{
		return WriteString16Bit(offset, value, isUnicode: true);
	}

	public int WriteString16Bit(int offset, string value, bool isUnicode)
	{
		int num = offset;
		WriteString16BitUpdateOffset(ref offset, value, isUnicode);
		return offset - num;
	}

	public virtual void WriteStringNoLenUpdateOffset(ref int offset, string value)
	{
		WriteStringNoLenUpdateOffset(ref offset, value, bUnicode: true);
	}

	public abstract void WriteStringNoLenUpdateOffset(ref int offset, string value, bool bUnicode);

	public void WriteBytes(int offset, byte[] data)
	{
		WriteBytes(offset, data, 0, data.Length);
	}

	public abstract void WriteBytes(int offset, byte[] value, int pos, int length);

	[CLSCompliant(false)]
	protected internal void WriteAddr(int offset, TAddr addr)
	{
		WriteUInt16(offset, (ushort)addr.FirstRow);
		WriteUInt16(offset + 2, (ushort)addr.LastRow);
		WriteUInt16(offset + 4, (ushort)addr.FirstCol);
		WriteUInt16(offset + 6, (ushort)addr.LastCol);
	}

	protected internal void WriteAddr(int offset, Rectangle addr)
	{
		WriteUInt16(offset, (ushort)addr.Top);
		WriteUInt16(offset + 2, (ushort)addr.Bottom);
		WriteUInt16(offset + 4, (ushort)addr.Left);
		WriteUInt16(offset + 6, (ushort)addr.Right);
	}

	public abstract void MoveMemory(int iDestOffset, int iSourceOffset, int iMemorySize);

	public abstract void CopyMemory(int iDestOffset, int iSourceOffset, int iMemorySize);

	public abstract int EnsureCapacity(int size);

	public abstract int EnsureCapacity(int size, int forceAdd);

	public abstract void ZeroMemory();

	public abstract void Clear();

	public abstract DataProvider CreateProvider();

	public void Dispose(bool isDisposing)
	{
		if (isDisposing)
		{
			OnDispose();
		}
		GC.SuppressFinalize(this);
	}

	public void Dispose()
	{
		Clear();
		Dispose(isDisposing: true);
	}

	protected virtual void OnDispose()
	{
	}
}
