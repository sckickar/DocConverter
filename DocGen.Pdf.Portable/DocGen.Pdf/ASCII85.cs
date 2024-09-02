using System;

namespace DocGen.Pdf;

internal class ASCII85
{
	private long[] hex_indices = new long[4] { 16777216L, 65536L, 256L, 1L };

	private long[] base_85_indices = new long[5] { 52200625L, 614125L, 7225L, 85L, 1L };

	private int m_specialCases;

	private int m_returns;

	private int m_dataSize;

	private int m_outputPointer;

	public byte[] decode(byte[] encodedData)
	{
		m_dataSize = encodedData.Length;
		for (int i = 0; i < m_dataSize; i++)
		{
			if (encodedData[i] == 122)
			{
				m_specialCases++;
			}
			else if (encodedData[i] == 10)
			{
				m_returns++;
			}
		}
		byte[] array = new byte[m_dataSize - m_returns + 1 + m_specialCases * 3];
		for (int j = 0; j < m_dataSize; j++)
		{
			long num = 0L;
			int num2 = encodedData[j];
			while (true)
			{
				switch (num2)
				{
				case 10:
				case 13:
					j++;
					num2 = ((j != m_dataSize) ? encodedData[j] : 0);
					continue;
				case 122:
				{
					for (int m = 0; m < 4; m++)
					{
						array[m_outputPointer] = 0;
						m_outputPointer++;
					}
					break;
				}
				default:
				{
					if (m_dataSize - j <= 4 || num2 <= 32 || num2 >= 118)
					{
						break;
					}
					for (int k = 0; k < 5; k++)
					{
						if (j < encodedData.Length)
						{
							num2 = encodedData[j];
						}
						while (num2 == 10 || num2 == 13)
						{
							j++;
							num2 = ((j != m_dataSize) ? encodedData[j] : 0);
						}
						j++;
						if ((num2 > 32 && num2 < 118) || num2 == 126)
						{
							num += (num2 - 33) * base_85_indices[k];
						}
					}
					for (int l = 0; l < 4; l++)
					{
						array[m_outputPointer] = (byte)((num / hex_indices[l]) & 0xFF);
						m_outputPointer++;
					}
					j--;
					break;
				}
				}
				break;
			}
		}
		byte[] array2 = new byte[m_outputPointer];
		Array.Copy(array, 0, array2, 0, m_outputPointer);
		return array2;
	}
}
