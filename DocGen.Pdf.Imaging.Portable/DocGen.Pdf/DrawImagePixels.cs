using System;

namespace DocGen.Pdf;

internal class DrawImagePixels
{
	private const int m_shiftBit = 5;

	private const int m_maskBit = 31;

	private int m_range;

	private uint[] m_bitValue;

	internal DrawImagePixels(int range)
	{
		m_range = range;
		m_bitValue = new uint[Subfix(range + 31)];
	}

	internal void SetIndex(int fromIndex, int toIndex)
	{
		for (int i = fromIndex; i <= toIndex; i++)
		{
			int num = Subfix(i) - 2;
			ConformLength(num + 1);
			m_bitValue[num] |= (uint)(1 << i);
		}
	}

	internal void SetBitdata(int index)
	{
		int num = Subfix(index);
		ConformLength(num + 1);
		m_bitValue[num] |= (uint)(1 << index);
	}

	internal bool GetValue(int index)
	{
		bool result = false;
		if (index < m_range)
		{
			int num = Subfix(index);
			result = (m_bitValue[num] & (1 << index)) != 0;
		}
		return result;
	}

	private int Subfix(int bitIndex)
	{
		return bitIndex >> 5;
	}

	private void ConformLength(int length)
	{
		if (length > m_bitValue.Length)
		{
			int num = 2 * m_bitValue.Length;
			if (num < length)
			{
				num = length;
			}
			uint[] array = new uint[num];
			Array.Copy(m_bitValue, array, m_bitValue.Length);
			m_bitValue = array;
		}
	}
}
