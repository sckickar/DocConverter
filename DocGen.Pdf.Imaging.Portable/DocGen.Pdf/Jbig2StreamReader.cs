namespace DocGen.Pdf;

internal class Jbig2StreamReader
{
	private byte[] m_data;

	private int m_bitPointer = 7;

	internal int bytePointer;

	internal Jbig2StreamReader(byte[] data)
	{
		m_data = data;
	}

	internal short ReadByte()
	{
		return (short)(m_data[bytePointer++] & 0xFF);
	}

	internal void ReadByte(short[] buf)
	{
		for (int i = 0; i < buf.Length; i++)
		{
			if (bytePointer < m_data.Length)
			{
				buf[i] = (short)(m_data[bytePointer++] & 0xFF);
			}
		}
	}

	internal int ReadBit()
	{
		short num = ReadByte();
		short num2 = (short)(1 << m_bitPointer);
		int result = (num & num2) >> m_bitPointer;
		m_bitPointer--;
		if (m_bitPointer == -1)
		{
			m_bitPointer = 7;
			return result;
		}
		MovePointer(-1);
		return result;
	}

	internal int ReadBits(int num)
	{
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			num2 = (num2 << 1) | ReadBit();
		}
		return num2;
	}

	internal void MovePointer(int ammount)
	{
		bytePointer += ammount;
	}

	internal void ConsumeRemainingBits()
	{
		if (m_bitPointer != 7)
		{
			ReadBits(m_bitPointer + 1);
		}
	}

	internal bool Getfinished()
	{
		return bytePointer == m_data.Length;
	}
}
