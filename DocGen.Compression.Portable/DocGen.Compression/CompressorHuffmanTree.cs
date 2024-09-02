using System;

namespace DocGen.Compression;

public class CompressorHuffmanTree
{
	private short[] m_CodeFrequences;

	private short[] m_Codes;

	private byte[] m_CodeLengths;

	private int[] m_LengthCounts;

	private int m_CodeMinimumCount;

	private int m_CodeCount;

	private int m_MaximumLength;

	private CompressedStreamWriter m_Writer;

	public int TreeLength => m_CodeCount;

	public byte[] CodeLengths => m_CodeLengths;

	public short[] CodeFrequences => m_CodeFrequences;

	public CompressorHuffmanTree(CompressedStreamWriter writer, int iElementsCount, int iMinimumCodes, int iMaximumLength)
	{
		m_Writer = writer;
		m_CodeMinimumCount = iMinimumCodes;
		m_MaximumLength = iMaximumLength;
		m_CodeFrequences = new short[iElementsCount];
		m_LengthCounts = new int[iMaximumLength];
	}

	public void Reset()
	{
		for (int i = 0; i < m_CodeFrequences.Length; i++)
		{
			m_CodeFrequences[i] = 0;
		}
		m_Codes = null;
		m_CodeLengths = null;
	}

	public void WriteCodeToStream(int code)
	{
		m_Writer.PendingBufferWriteBits(m_Codes[code] & 0xFFFF, m_CodeLengths[code]);
	}

	public void CheckEmpty()
	{
		for (int i = 0; i < m_CodeFrequences.Length; i++)
		{
		}
	}

	public void SetStaticCodes(short[] codes, byte[] lengths)
	{
		m_Codes = (short[])codes.Clone();
		m_CodeLengths = (byte[])lengths.Clone();
	}

	public void BuildCodes()
	{
		int[] array = new int[m_MaximumLength];
		m_Codes = new short[m_CodeCount];
		int num = 0;
		for (int i = 0; i < m_MaximumLength; i++)
		{
			array[i] = num;
			num += m_LengthCounts[i] << 15 - i;
		}
		for (int j = 0; j < m_CodeCount; j++)
		{
			int num2 = m_CodeLengths[j];
			if (num2 > 0)
			{
				m_Codes[j] = Utils.BitReverse(array[num2 - 1]);
				array[num2 - 1] += 1 << 16 - num2;
			}
		}
	}

	private void BuildLength(int[] childs)
	{
		m_CodeLengths = new byte[m_CodeFrequences.Length];
		int num = childs.Length / 2;
		int num2 = (num + 1) / 2;
		int num3 = 0;
		for (int i = 0; i < m_MaximumLength; i++)
		{
			m_LengthCounts[i] = 0;
		}
		int[] array = new int[num];
		array[num - 1] = 0;
		for (int num4 = num - 1; num4 >= 0; num4--)
		{
			int num5 = 2 * num4 + 1;
			if (childs[num5] != -1)
			{
				int num6 = array[num4] + 1;
				if (num6 > m_MaximumLength)
				{
					num6 = m_MaximumLength;
					num3++;
				}
				array[childs[num5 - 1]] = (array[childs[num5]] = num6);
			}
			else
			{
				int num7 = array[num4];
				m_LengthCounts[num7 - 1]++;
				m_CodeLengths[childs[num5 - 1]] = (byte)array[num4];
			}
		}
		if (num3 == 0)
		{
			return;
		}
		int num8 = m_MaximumLength - 1;
		while (true)
		{
			if (m_LengthCounts[--num8] != 0)
			{
				do
				{
					m_LengthCounts[num8]--;
					m_LengthCounts[++num8]++;
					num3 -= 1 << m_MaximumLength - 1 - num8;
				}
				while (num3 > 0 && num8 < m_MaximumLength - 1);
				if (num3 <= 0)
				{
					break;
				}
			}
		}
		m_LengthCounts[m_MaximumLength - 1] += num3;
		m_LengthCounts[m_MaximumLength - 2] -= num3;
		int num9 = 2 * num2;
		for (int num10 = m_MaximumLength; num10 != 0; num10--)
		{
			int num11 = m_LengthCounts[num10 - 1];
			while (num11 > 0)
			{
				int num12 = 2 * childs[num9++];
				if (childs[num12 + 1] == -1)
				{
					m_CodeLengths[childs[num12]] = (byte)num10;
					num11--;
				}
			}
		}
	}

	public void BuildTree()
	{
		int num = m_CodeFrequences.Length;
		int[] array = new int[num];
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < num; i++)
		{
			int num4 = m_CodeFrequences[i];
			if (num4 != 0)
			{
				int num5 = num2++;
				int num6;
				while (num5 > 0 && m_CodeFrequences[array[num6 = (num5 - 1) / 2]] > num4)
				{
					array[num5] = array[num6];
					num5 = num6;
				}
				array[num5] = i;
				num3 = i;
			}
		}
		while (num2 < 2)
		{
			array[num2++] = ((num3 < 2) ? (++num3) : 0);
		}
		m_CodeCount = Math.Max(num3 + 1, m_CodeMinimumCount);
		int num7 = num2;
		int[] array2 = new int[4 * num2 - 2];
		int[] array3 = new int[2 * num2 - 1];
		for (int j = 0; j < num2; j++)
		{
			int num8 = array[j];
			int num9 = 2 * j;
			array2[num9] = num8;
			array2[num9 + 1] = -1;
			array3[j] = m_CodeFrequences[num8] << 8;
			array[j] = j;
		}
		do
		{
			int num10 = array[0];
			int num11 = array[--num2];
			int num12 = array3[num11];
			int num13 = 0;
			int num14;
			for (num14 = 1; num14 < num2; num14 = num13 * 2 + 1)
			{
				if (num14 + 1 < num2 && array3[array[num14]] > array3[array[num14 + 1]])
				{
					num14++;
				}
				array[num13] = array[num14];
				num13 = num14;
			}
			while ((num14 = num13) > 0 && array3[array[num13 = (num14 - 1) / 2]] > num12)
			{
				array[num14] = array[num13];
			}
			array[num14] = num11;
			int num15 = array[0];
			num11 = num7++;
			array2[2 * num11] = num10;
			array2[2 * num11 + 1] = num15;
			int num16 = Math.Min(array3[num10] & 0xFF, array3[num15] & 0xFF);
			num12 = (array3[num11] = array3[num10] + array3[num15] - num16 + 1);
			num13 = 0;
			for (num14 = 1; num14 < num2; num14 = num13 * 2 + 1)
			{
				if (num14 + 1 < num2 && array3[array[num14]] > array3[array[num14 + 1]])
				{
					num14++;
				}
				array[num13] = array[num14];
				num13 = num14;
			}
			while ((num14 = num13) > 0 && array3[array[num13 = (num14 - 1) / 2]] > num12)
			{
				array[num14] = array[num13];
			}
			array[num14] = num11;
		}
		while (num2 > 1);
		if (array[0] != array2.Length / 2 - 1)
		{
			throw new ApplicationException("Heap invariant violated");
		}
		BuildLength(array2);
	}

	public int GetEncodedLength()
	{
		int num = 0;
		for (int i = 0; i < m_CodeFrequences.Length; i++)
		{
			num += m_CodeFrequences[i] * m_CodeLengths[i];
		}
		return num;
	}

	public void CalcBLFreq(CompressorHuffmanTree blTree)
	{
		int num = -1;
		int num2 = 0;
		while (num2 < m_CodeCount)
		{
			int num3 = 1;
			int num4 = m_CodeLengths[num2];
			int num5;
			int num6;
			if (num4 == 0)
			{
				num5 = 138;
				num6 = 3;
			}
			else
			{
				num5 = 6;
				num6 = 3;
				if (num != num4)
				{
					blTree.m_CodeFrequences[num4]++;
					num3 = 0;
				}
			}
			num = num4;
			num2++;
			while (num2 < m_CodeCount && num == m_CodeLengths[num2])
			{
				num2++;
				if (++num3 >= num5)
				{
					break;
				}
			}
			if (num3 < num6)
			{
				blTree.m_CodeFrequences[num] += (short)num3;
			}
			else if (num != 0)
			{
				blTree.m_CodeFrequences[16]++;
			}
			else if (num3 <= 10)
			{
				blTree.m_CodeFrequences[17]++;
			}
			else
			{
				blTree.m_CodeFrequences[18]++;
			}
		}
	}

	public void WriteTree(CompressorHuffmanTree blTree)
	{
		int num = -1;
		int num2 = 0;
		while (num2 < m_CodeCount)
		{
			int num3 = 1;
			int num4 = m_CodeLengths[num2];
			int num5;
			int num6;
			if (num4 == 0)
			{
				num5 = 138;
				num6 = 3;
			}
			else
			{
				num5 = 6;
				num6 = 3;
				if (num != num4)
				{
					blTree.WriteCodeToStream(num4);
					num3 = 0;
				}
			}
			num = num4;
			num2++;
			while (num2 < m_CodeCount && num == m_CodeLengths[num2])
			{
				num2++;
				if (++num3 >= num5)
				{
					break;
				}
			}
			if (num3 < num6)
			{
				while (num3-- > 0)
				{
					blTree.WriteCodeToStream(num);
				}
			}
			else if (num != 0)
			{
				blTree.WriteCodeToStream(16);
				m_Writer.PendingBufferWriteBits(num3 - 3, 2);
			}
			else if (num3 <= 10)
			{
				blTree.WriteCodeToStream(17);
				m_Writer.PendingBufferWriteBits(num3 - 3, 3);
			}
			else
			{
				blTree.WriteCodeToStream(18);
				m_Writer.PendingBufferWriteBits(num3 - 11, 7);
			}
		}
	}
}
