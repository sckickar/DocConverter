using System;

namespace DocGen.Pdf.Security;

internal class RWAlgorithmGenerator : IAlgorithmGenerator
{
	private IAlgorithmGenerator m_generator;

	private byte[] m_window;

	private int m_windowCount;

	internal RWAlgorithmGenerator(IAlgorithmGenerator generator, int windowSize)
	{
		if (generator == null)
		{
			throw new ArgumentNullException("generator");
		}
		if (windowSize < 2)
		{
			throw new ArgumentException("Invalid size. Window size must be at least 2");
		}
		m_generator = generator;
		m_window = new byte[windowSize];
	}

	public virtual void AddMaterial(byte[] bytes)
	{
		lock (this)
		{
			m_windowCount = 0;
			m_generator.AddMaterial(bytes);
		}
	}

	public virtual void AddMaterial(long value)
	{
		lock (this)
		{
			m_windowCount = 0;
			m_generator.AddMaterial(value);
		}
	}

	public virtual void FillNextBytes(byte[] bytes)
	{
		doNextBytes(bytes, 0, bytes.Length);
	}

	public virtual void FillNextBytes(byte[] bytes, int start, int length)
	{
		doNextBytes(bytes, start, length);
	}

	private void doNextBytes(byte[] bytes, int start, int length)
	{
		lock (this)
		{
			int num = 0;
			while (num < length)
			{
				if (m_windowCount < 1)
				{
					m_generator.FillNextBytes(m_window, 0, m_window.Length);
					m_windowCount = m_window.Length;
				}
				bytes[start + num++] = m_window[--m_windowCount];
			}
		}
	}
}
