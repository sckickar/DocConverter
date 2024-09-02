using System;
using System.Collections.Generic;
using System.IO;
using DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class BookmarkDescriptor
{
	private int DEF_STRUCT_SIZE = 4;

	private int m_bkmkCount;

	private List<BookmarkFirstStructure> m_bkfArr;

	private List<int> m_bklArr;

	private int m_lastEnd;

	internal int BookmarkCount => m_bkfArr.Count;

	private BookmarkFirstStructure[] BkfArray => m_bkfArr.ToArray();

	internal BookmarkDescriptor(Stream stream, int bookmarkCount, int bkfPos, int bkfLength, int bklPos, int bklLength)
	{
		m_bkmkCount = bookmarkCount;
		m_bkfArr = new List<BookmarkFirstStructure>(m_bkmkCount);
		m_bklArr = new List<int>(m_bkmkCount);
		ReadBKF(bkfPos, stream, bkfLength);
		ReadBKL(bklPos, stream, bklLength);
	}

	internal BookmarkDescriptor()
	{
		m_bkmkCount = 0;
		m_bkfArr = new List<BookmarkFirstStructure>(m_bkmkCount);
		for (int i = 0; i < BkfArray.Length; i++)
		{
			m_bkfArr.Add(new BookmarkFirstStructure());
		}
		m_bklArr = new List<int>(m_bkmkCount);
	}

	internal int GetBeginPos(int i)
	{
		return BkfArray[i].BeginPos;
	}

	internal void SetBeginPos(int i, int position)
	{
		BkfArray[i].BeginPos = position;
	}

	internal int GetEndPos(int i)
	{
		short endIndex = BkfArray[i].EndIndex;
		return m_bklArr[endIndex];
	}

	internal void SetEndPos(int i, int position)
	{
		if (BkfArray.Length > i && m_bklArr.Count > m_lastEnd)
		{
			BkfArray[i].EndIndex = (short)m_lastEnd;
			m_bklArr[m_lastEnd] = position;
			m_lastEnd++;
		}
	}

	internal void Save(Stream stream, Fib fib, uint endChar)
	{
		if (BookmarkCount > 0)
		{
			WriteBKF(stream, fib, endChar);
			WriteBKL(stream, fib, endChar);
		}
	}

	internal void Add(int startPos)
	{
		int count = m_bkfArr.Count;
		m_bkfArr.Add(new BookmarkFirstStructure());
		m_bklArr.Add(startPos);
		BkfArray[count].BeginPos = startPos;
	}

	internal bool IsCellGroup(int bookmarkIndex)
	{
		return (BkfArray[bookmarkIndex].Props & 0x8000) >> 15 == 1;
	}

	internal void SetCellGroup(int bookmarkIndex, bool isCellGroup)
	{
		if (BkfArray.Length > bookmarkIndex)
		{
			BkfArray[bookmarkIndex].Props = (isCellGroup ? ((short)BaseWordRecord.SetBitsByMask(BkfArray[bookmarkIndex].Props, 32768, 15, 1)) : ((short)BaseWordRecord.SetBitsByMask(BkfArray[bookmarkIndex].Props, 32768, 15, 0)));
		}
	}

	internal short GetStartCellIndex(int bookmarkIndex)
	{
		return (short)(BkfArray[bookmarkIndex].Props & 0x7F);
	}

	internal void SetStartCellIndex(int bookmarkIndex, int position)
	{
		if (BkfArray.Length > bookmarkIndex)
		{
			BkfArray[bookmarkIndex].Props = (short)BaseWordRecord.SetBitsByMask(BkfArray[bookmarkIndex].Props, 127, position);
		}
	}

	internal short GetEndCellIndex(int bookmarkIndex)
	{
		return (short)((BkfArray[bookmarkIndex].Props & 0x7F00) >> 8);
	}

	internal void SetEndCellIndex(int bookmarkIndex, int position)
	{
		if (BkfArray.Length > bookmarkIndex)
		{
			BkfArray[bookmarkIndex].Props = (short)BaseWordRecord.SetBitsByMask(BkfArray[bookmarkIndex].Props, 32512, 8, position);
		}
	}

	internal void Close()
	{
		if (m_bkfArr != null)
		{
			m_bkfArr.Clear();
			m_bkfArr = null;
		}
		if (m_bklArr != null)
		{
			m_bklArr.Clear();
			m_bklArr = null;
		}
	}

	private void ReadBKL(int bklPos, Stream stream, int bklLength)
	{
		stream.Position = bklPos;
		byte[] array = new byte[m_bkmkCount * 4];
		stream.Read(array, 0, array.Length);
		int[] array2 = new int[m_bkmkCount];
		Buffer.BlockCopy(array, 0, array2, 0, array.Length);
		m_bklArr.AddRange(array2);
		if (stream.Position > bklPos + bklLength)
		{
			throw new StreamReadException("Too many bytes read for BookmarkLimDescriptor");
		}
	}

	private void ReadBKF(int bkfPos, Stream stream, int bkfLength)
	{
		byte[] array = new byte[m_bkmkCount * DEF_STRUCT_SIZE];
		ushort[] array2 = new ushort[m_bkmkCount * 2];
		int[] array3 = new int[m_bkmkCount];
		stream.Position = bkfPos;
		stream.Read(array, 0, m_bkmkCount * DEF_STRUCT_SIZE);
		Buffer.BlockCopy(array, 0, array3, 0, array.Length);
		for (int i = 0; i < m_bkmkCount; i++)
		{
			m_bkfArr.Add(new BookmarkFirstStructure());
			BkfArray[i].BeginPos = array3[i];
		}
		stream.Position += 4L;
		array = new byte[m_bkmkCount * 4];
		array2 = new ushort[m_bkmkCount * 2];
		stream.Read(array, 0, array.Length);
		Buffer.BlockCopy(array, 0, array2, 0, array.Length);
		for (int j = 0; j < m_bkmkCount; j++)
		{
			BkfArray[j].EndIndex = (short)array2[2 * j];
			BkfArray[j].Props = array2[2 * j + 1];
		}
		if (stream.Position > bkfPos + bkfLength)
		{
			throw new StreamReadException("To many bytes read for BookmarkFirstDescriptor");
		}
	}

	private void WriteBKF(Stream stream, Fib fib, uint endChar)
	{
		byte[] array = null;
		_ = new byte[BookmarkCount * DEF_STRUCT_SIZE];
		uint num = (uint)stream.Position;
		for (int i = 0; i < BkfArray.Length; i++)
		{
			array = BkfArray[i].SavePos();
			stream.Write(array, 0, array.Length);
		}
		array = BitConverter.GetBytes(endChar);
		stream.Write(array, 0, array.Length);
		for (int j = 0; j < BookmarkCount; j++)
		{
			array = BkfArray[j].SaveProps();
			stream.Write(array, 0, array.Length);
		}
		uint num2 = (uint)stream.Position;
		if (num2 > num)
		{
			fib.FibRgFcLcb97FcPlcfBkf = num;
			fib.FibRgFcLcb97LcbPlcfBkf = num2 - num;
		}
	}

	private void WriteBKL(Stream stream, Fib fib, uint endChar)
	{
		byte[] array = null;
		byte[] array2 = new byte[BookmarkCount * DEF_STRUCT_SIZE];
		fib.FibRgFcLcb97LcbPlcfBkf = (uint)(stream.Position - fib.FibRgFcLcb97FcPlcfBkf);
		uint num = (uint)stream.Position;
		Buffer.BlockCopy(m_bklArr.ToArray(), 0, array2, 0, array2.Length);
		stream.Write(array2, 0, array2.Length);
		array = BitConverter.GetBytes(endChar);
		stream.Write(array, 0, array.Length);
		uint num2 = (uint)stream.Position;
		if (num2 > num)
		{
			fib.FibRgFcLcb97FcPlcfBkl = num;
			fib.FibRgFcLcb97LcbPlcfBkl = num2 - num;
		}
	}
}
