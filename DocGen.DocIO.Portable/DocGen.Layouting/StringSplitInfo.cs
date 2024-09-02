using System;

namespace DocGen.Layouting;

internal class StringSplitInfo
{
	private int m_firstPos;

	private int m_lastPos;

	public int FirstPos => m_firstPos;

	public int LastPos => m_lastPos;

	public int Length => LastPos - FirstPos + 1;

	private StringSplitInfo()
	{
	}

	public StringSplitInfo(int firstPos, int lastPos)
	{
		if (firstPos < 0)
		{
			throw new ArgumentException("firstPos");
		}
		if (firstPos > lastPos)
		{
			throw new ArgumentException("lastPos");
		}
		m_lastPos = lastPos;
		m_firstPos = firstPos;
	}

	public void Check(int length)
	{
		if (m_firstPos < 0 || m_firstPos > length)
		{
			throw new ArgumentOutOfRangeException("SplitInfo.FirstPos");
		}
		if (m_lastPos < m_firstPos || m_lastPos > length)
		{
			throw new ArgumentOutOfRangeException("SplitInfo.LastPos");
		}
	}

	public void Extend(StringSplitInfo strSplitInfo)
	{
		m_firstPos += strSplitInfo.FirstPos;
		m_lastPos += strSplitInfo.FirstPos;
	}

	public StringSplitInfo GetSplitFirstPart(int position)
	{
		return new StringSplitInfo(m_firstPos, m_firstPos + position - 1);
	}

	public StringSplitInfo GetSplitSecondPart(int position)
	{
		return new StringSplitInfo(m_firstPos + position, m_lastPos);
	}

	public string GetSubstring(string text)
	{
		return text.Substring(m_firstPos, m_lastPos - m_firstPos + 1);
	}
}
