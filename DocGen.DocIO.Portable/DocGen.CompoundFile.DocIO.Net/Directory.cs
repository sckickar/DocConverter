using System;
using System.Collections.Generic;
using System.IO;

namespace DocGen.CompoundFile.DocIO.Net;

internal class Directory
{
	private List<DirectoryEntry> m_lstEntries = new List<DirectoryEntry>();

	public List<DirectoryEntry> Entries => m_lstEntries;

	public Directory()
	{
	}

	public Directory(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		int num = 0;
		int num2 = data.Length;
		m_lstEntries = new List<DirectoryEntry>();
		int num3 = 0;
		while (num < num2)
		{
			m_lstEntries.Add(new DirectoryEntry(data, num, num3));
			num += 128;
			num3++;
		}
	}

	public int FindEmpty()
	{
		int result = -1;
		int i = 0;
		for (int count = m_lstEntries.Count; i < count; i++)
		{
			if (m_lstEntries[i].Type == DirectoryEntry.EntryType.Invalid)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	public void Add(DirectoryEntry entry)
	{
		int num = FindEmpty();
		if (num >= 0)
		{
			m_lstEntries[num] = entry;
			entry.EntryId = num;
		}
		else
		{
			m_lstEntries.Add(entry);
		}
	}

	public void Write(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		int i = 0;
		for (int count = m_lstEntries.Count; i < count; i++)
		{
			m_lstEntries[i].Write(stream);
		}
	}
}
