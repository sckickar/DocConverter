using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class BookmarkNameStringTable
{
	private List<string> m_strArr;

	private int m_bkmkCount;

	internal int BookmarkCount
	{
		get
		{
			return m_bkmkCount;
		}
		set
		{
			m_bkmkCount = value;
		}
	}

	internal string this[int index] => m_strArr[index];

	internal int BookmarkNamesLength
	{
		get
		{
			if (m_strArr != null)
			{
				return m_strArr.Count;
			}
			return 0;
		}
	}

	internal BookmarkNameStringTable(Stream stream, int length)
	{
		int num = 6;
		long position = stream.Position;
		byte[] array = new byte[num];
		stream.Read(array, 0, num);
		m_bkmkCount = BitConverter.ToInt32(array, 2);
		m_strArr = new List<string>(m_bkmkCount);
		for (int i = 0; i < m_bkmkCount; i++)
		{
			array = new byte[2];
			stream.Read(array, 0, 2);
			array = new byte[BitConverter.ToInt16(array, 0) * 2];
			stream.Read(array, 0, array.Length);
			string @string = Encoding.Unicode.GetString(array, 0, array.Length);
			m_strArr.Add(@string);
		}
		if (stream.Position - position > length)
		{
			throw new StreamReadException("");
		}
	}

	internal BookmarkNameStringTable()
	{
		m_bkmkCount = 0;
		m_strArr = new List<string>(m_bkmkCount);
	}

	internal void Close()
	{
		if (m_strArr != null)
		{
			m_strArr.Clear();
			m_strArr = null;
		}
	}

	internal void Save(Stream stream, Fib fib)
	{
		if (m_strArr.Count > 0)
		{
			uint num = (uint)stream.Position;
			byte[] array = new byte[6];
			byte[] bytes = BitConverter.GetBytes(m_strArr.Count);
			bytes.CopyTo(array, 2);
			byte b;
			array[1] = (b = byte.MaxValue);
			array[0] = b;
			stream.Write(array, 0, array.Length);
			m_bkmkCount = m_strArr.Count;
			for (int i = 0; i < m_bkmkCount; i++)
			{
				byte[] bytes2 = Encoding.Unicode.GetBytes(m_strArr[i]);
				bytes = BitConverter.GetBytes((short)(bytes2.Length / 2));
				stream.Write(bytes, 0, bytes.Length);
				stream.Write(bytes2, 0, bytes2.Length);
			}
			uint num2 = (uint)stream.Position;
			if (num2 > num)
			{
				fib.FibRgFcLcb97FcSttbfBkmk = num;
				fib.FibRgFcLcb97LcbSttbfBkmk = num2 - num;
			}
		}
	}

	internal void Add(string name)
	{
		m_strArr.Add(name);
	}

	internal int Find(string name)
	{
		int result = -1;
		for (int i = 0; i < m_strArr.Count; i++)
		{
			if (this[i] == name)
			{
				result = i;
				break;
			}
		}
		return result;
	}
}
