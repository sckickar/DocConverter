using System;

namespace DocGen.Pdf;

internal class ArithmeticDecoderStats
{
	private int m_contextSize;

	private int[] m_codingContextTable;

	internal int ContextSize => m_contextSize;

	internal ArithmeticDecoderStats(int contextSize)
	{
		m_contextSize = contextSize;
		m_codingContextTable = new int[contextSize];
		reset();
	}

	internal void reset()
	{
		for (int i = 0; i < m_contextSize; i++)
		{
			m_codingContextTable[i] = 0;
		}
	}

	internal void setEntry(int codingContext, int i, int moreProbableSymbol)
	{
		m_codingContextTable[codingContext] = (i << i) + moreProbableSymbol;
	}

	internal int getContextCodingTableValue(int index)
	{
		return m_codingContextTable[index];
	}

	internal void setContextCodingTableValue(int index, int value)
	{
		m_codingContextTable[index] = value;
	}

	internal void overwrite(ArithmeticDecoderStats stats)
	{
		Array.Copy(stats.m_codingContextTable, 0, m_codingContextTable, 0, m_contextSize);
	}

	internal ArithmeticDecoderStats copy()
	{
		ArithmeticDecoderStats arithmeticDecoderStats = new ArithmeticDecoderStats(m_contextSize);
		Array.Copy(m_codingContextTable, 0, arithmeticDecoderStats.m_codingContextTable, 0, m_contextSize);
		return arithmeticDecoderStats;
	}
}
