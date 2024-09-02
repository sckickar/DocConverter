using System;
using System.Collections.Generic;

namespace DocGen.Pdf.Security;

internal class RandomGroup : IRandom, IDisposable
{
	private sealed class SourceEntry
	{
		internal readonly IRandom m_source;

		internal readonly long m_startByte;

		internal readonly long m_endByte;

		internal readonly int m_index;

		internal SourceEntry(int index, IRandom source, long offset)
		{
			m_index = index;
			m_source = source;
			m_startByte = offset;
			m_endByte = offset + source.Length - 1;
		}

		internal long OffsetN(long absoluteOffset)
		{
			return absoluteOffset - m_startByte;
		}
	}

	private readonly SourceEntry[] m_sources;

	private SourceEntry m_cse;

	private readonly long m_size;

	public virtual long Length => m_size;

	internal RandomGroup(ICollection<IRandom> sources)
	{
		m_sources = new SourceEntry[sources.Count];
		long num = 0L;
		int num2 = 0;
		foreach (IRandom source in sources)
		{
			m_sources[num2] = new SourceEntry(num2, source, num);
			num2++;
			num += source.Length;
		}
		m_size = num;
		m_cse = m_sources[sources.Count - 1];
		SourceInUse(m_cse.m_source);
	}

	protected internal int GetStartIndex(long offset)
	{
		if (offset >= m_cse.m_startByte)
		{
			return m_cse.m_index;
		}
		return 0;
	}

	private SourceEntry GetEntry(long offset)
	{
		if (offset >= m_size)
		{
			return null;
		}
		if (offset >= m_cse.m_startByte && offset <= m_cse.m_endByte)
		{
			return m_cse;
		}
		SourceReleased(m_cse.m_source);
		for (int i = GetStartIndex(offset); i < m_sources.Length; i++)
		{
			if (offset >= m_sources[i].m_startByte && offset <= m_sources[i].m_endByte)
			{
				m_cse = m_sources[i];
				SourceInUse(m_cse.m_source);
				return m_cse;
			}
		}
		return null;
	}

	protected internal virtual void SourceReleased(IRandom source)
	{
	}

	protected internal virtual void SourceInUse(IRandom source)
	{
	}

	public virtual int Get(long position)
	{
		SourceEntry entry = GetEntry(position);
		return entry?.m_source.Get(entry.OffsetN(position)) ?? (-1);
	}

	public virtual int Get(long position, byte[] bytes, int off, int len)
	{
		SourceEntry entry = GetEntry(position);
		if (entry == null)
		{
			return -1;
		}
		long num = entry.OffsetN(position);
		int num2 = len;
		while (num2 > 0 && entry != null && num <= entry.m_source.Length)
		{
			int num3 = entry.m_source.Get(num, bytes, off, num2);
			if (num3 == -1)
			{
				break;
			}
			off += num3;
			position += num3;
			num2 -= num3;
			num = 0L;
			entry = GetEntry(position);
		}
		if (num2 != len)
		{
			return len - num2;
		}
		return -1;
	}

	public virtual void Close()
	{
		SourceEntry[] sources = m_sources;
		for (int i = 0; i < sources.Length; i++)
		{
			sources[i].m_source.Close();
		}
	}

	public virtual void Dispose()
	{
		Close();
	}
}
