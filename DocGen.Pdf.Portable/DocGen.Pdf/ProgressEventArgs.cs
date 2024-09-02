using System;

namespace DocGen.Pdf;

public class ProgressEventArgs
{
	private int m_total;

	private int m_current;

	private int m_pageProcessed = -1;

	private int m_changedPages;

	public int Total => m_total;

	public int Current => m_current;

	public float Progress => ((m_pageProcessed != -1) ? ((float)m_pageProcessed) : ((float)Current)) / (float)((m_changedPages > 0) ? m_changedPages : Total);

	internal ProgressEventArgs(int current, int total)
	{
		if (total <= 0)
		{
			throw new ArgumentOutOfRangeException("total", "Total is less then or equal to zero.");
		}
		if (current < 0)
		{
			throw new ArgumentOutOfRangeException("current", "Current can't be less then zero.");
		}
		m_current = current;
		m_total = total;
	}

	internal ProgressEventArgs(int current, int total, int processed)
		: this(current, total)
	{
		m_pageProcessed = processed;
	}

	internal ProgressEventArgs(int current, int total, int processed, int changed)
		: this(current, total, processed)
	{
		m_changedPages = changed;
	}

	private ProgressEventArgs()
	{
	}
}
