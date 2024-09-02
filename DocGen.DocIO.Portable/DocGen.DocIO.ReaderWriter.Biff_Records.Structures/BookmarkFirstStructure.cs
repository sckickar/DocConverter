using System;

namespace DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

internal class BookmarkFirstStructure
{
	private int m_beginCP;

	private short m_endIndex;

	private int m_props;

	internal int BeginPos
	{
		get
		{
			return m_beginCP;
		}
		set
		{
			m_beginCP = value;
		}
	}

	internal int Props
	{
		get
		{
			return m_props;
		}
		set
		{
			m_props = value;
		}
	}

	internal short EndIndex
	{
		get
		{
			return m_endIndex;
		}
		set
		{
			m_endIndex = value;
		}
	}

	internal byte[] SavePos()
	{
		byte[] array = new byte[4];
		BitConverter.GetBytes(m_beginCP).CopyTo(array, 0);
		return array;
	}

	internal byte[] SaveProps()
	{
		byte[] array = new byte[4];
		BitConverter.GetBytes(m_endIndex).CopyTo(array, 0);
		BitConverter.GetBytes((ushort)m_props).CopyTo(array, 2);
		return array;
	}
}
