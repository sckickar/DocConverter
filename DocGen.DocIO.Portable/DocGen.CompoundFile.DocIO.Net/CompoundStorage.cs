using System;
using System.Collections.Generic;

namespace DocGen.CompoundFile.DocIO.Net;

internal class CompoundStorage : ICompoundStorage, IDisposable, ICompoundItem
{
	private CompoundFile m_parentFile;

	private SortedList<string, ICompoundItem> m_nodes = new SortedList<string, ICompoundItem>(new ItemNamesComparer());

	private DirectoryEntry m_entry;

	private List<string> m_arrStorages = new List<string>();

	private List<string> m_arrStreams = new List<string>();

	private List<int> m_entryIndexes;

	public string[] Streams => m_arrStreams.ToArray();

	public string[] Storages => m_arrStorages.ToArray();

	public string Name => m_entry.Name;

	public DirectoryEntry Entry => m_entry;

	public CompoundStorage(CompoundFile parent, string name, int entryIndex)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		m_parentFile = parent;
		m_entry = new DirectoryEntry(name, DirectoryEntry.EntryType.Storage, entryIndex);
	}

	public CompoundStorage(CompoundFile parentFile, DirectoryEntry entry)
	{
		if (parentFile == null)
		{
			throw new ArgumentNullException("parentFile");
		}
		if (entry == null)
		{
			throw new ArgumentNullException("entry");
		}
		if (entry.Type != DirectoryEntry.EntryType.Storage && entry.Type != DirectoryEntry.EntryType.Root)
		{
			throw new ArgumentOutOfRangeException("entry");
		}
		m_entry = entry;
		m_parentFile = parentFile;
		AddItem(entry.ChildId);
	}

	private void AddItem(int entryIndex)
	{
		if (entryIndex < 0)
		{
			return;
		}
		if (m_entryIndexes == null)
		{
			m_entryIndexes = new List<int>();
		}
		if (m_entryIndexes.Contains(entryIndex))
		{
			throw new NotSupportedException("This file format is not supported");
		}
		m_entryIndexes.Add(entryIndex);
		DirectoryEntry directoryEntry = m_parentFile.Directory.Entries[entryIndex];
		int leftId = directoryEntry.LeftId;
		string name = directoryEntry.Name;
		AddItem(leftId);
		switch (directoryEntry.Type)
		{
		case DirectoryEntry.EntryType.Storage:
			m_nodes.Add(name, new CompoundStorage(m_parentFile, directoryEntry));
			m_arrStorages.Add(name);
			break;
		case DirectoryEntry.EntryType.Stream:
			if (!m_arrStreams.Contains(name))
			{
				CompoundStreamNet value = new CompoundStreamNet(m_parentFile, directoryEntry);
				m_nodes.Add(name, value);
				m_arrStreams.Add(name);
			}
			break;
		default:
			throw new NotImplementedException();
		}
		int rightId = directoryEntry.RightId;
		AddItem(rightId);
	}

	public CompoundStream CreateStream(string streamName)
	{
		if (ContainsStream(streamName) || ContainsStorage(streamName))
		{
			throw new ArgumentOutOfRangeException("streamName", "Object with such name already exists");
		}
		DirectoryEntry entry = m_parentFile.AllocateDirectoryEntry(streamName, DirectoryEntry.EntryType.Stream);
		CompoundStreamNet compoundStreamNet = (m_parentFile.DirectMode ? new CompoundStreamDirect(m_parentFile, entry) : new CompoundStreamNet(m_parentFile, entry));
		m_arrStreams.Add(streamName);
		m_nodes.Add(streamName, compoundStreamNet);
		compoundStreamNet.Open();
		return new CompoundStreamWrapper(compoundStreamNet);
	}

	public CompoundStream OpenStream(string streamName)
	{
		CompoundStreamNet compoundStreamNet = m_nodes[streamName] as CompoundStreamNet;
		compoundStreamNet?.Open();
		return new CompoundStreamWrapper(compoundStreamNet);
	}

	public void DeleteStream(string streamName)
	{
		if (m_nodes[streamName] is CompoundStreamNet compoundStreamNet)
		{
			m_parentFile.RemoveItem(compoundStreamNet.Entry);
			compoundStreamNet.Dispose();
			m_nodes.Remove(streamName);
		}
	}

	public bool ContainsStream(string streamName)
	{
		return m_nodes.ContainsKey(streamName);
	}

	public ICompoundStorage CreateStorage(string storageName)
	{
		if (ContainsStream(storageName) || ContainsStorage(storageName))
		{
			throw new ArgumentOutOfRangeException("streamName", "Object with such name already exists");
		}
		DirectoryEntry directoryEntry = m_parentFile.AllocateDirectoryEntry(storageName, DirectoryEntry.EntryType.Storage);
		DateTime dateCreate = (directoryEntry.DateModify = DateTime.Now);
		directoryEntry.DateCreate = dateCreate;
		CompoundStorage compoundStorage = new CompoundStorage(m_parentFile, directoryEntry);
		m_arrStorages.Add(storageName);
		m_nodes.Add(storageName, compoundStorage);
		compoundStorage.Open();
		return new CompoundStorageWrapper(compoundStorage);
	}

	public ICompoundStorage OpenStorage(string storageName)
	{
		CompoundStorage compoundStorage = m_nodes[storageName] as CompoundStorage;
		compoundStorage?.Open();
		return new CompoundStorageWrapper(compoundStorage);
	}

	private void Open()
	{
	}

	public void DeleteStorage(string storageName)
	{
		if (m_nodes[storageName] is CompoundStorage compoundStorage)
		{
			compoundStorage.Dispose();
			m_nodes.Remove(storageName);
		}
	}

	public void Dispose()
	{
		if (m_parentFile != null)
		{
			m_parentFile = null;
			m_nodes = null;
			m_entry = null;
			if (m_entryIndexes != null)
			{
				m_entryIndexes.Clear();
				m_entryIndexes = null;
			}
			GC.SuppressFinalize(this);
		}
	}

	public bool ContainsStorage(string storageName)
	{
		return m_nodes.ContainsKey(storageName);
	}

	public void Flush()
	{
		m_entry.LeftId = -1;
		foreach (ICompoundItem value in m_nodes.Values)
		{
			value.Flush();
		}
		ICompoundItem compoundItem = null;
		foreach (ICompoundItem value2 in m_nodes.Values)
		{
			if (compoundItem != null)
			{
				compoundItem.Entry.RightId = value2.Entry.EntryId;
				compoundItem.Entry.LeftId = -1;
			}
			else
			{
				m_entry.ChildId = value2.Entry.EntryId;
			}
			compoundItem = value2;
		}
	}

	private void UpdateDirectory(RBTreeNode node)
	{
		object value = node.Value;
		if (value != null)
		{
			DirectoryEntry entry = (value as ICompoundItem).Entry;
			entry.Color = (byte)node.Color;
			entry.LeftId = GetNodeId(node.Left);
			entry.RightId = GetNodeId(node.Right);
			if (m_entry.ChildId < 0)
			{
				m_entry.ChildId = entry.EntryId;
			}
			if (value is CompoundStreamNet compoundStreamNet)
			{
				entry.Size = (uint)compoundStreamNet.Length;
			}
		}
	}

	private int GetNodeId(RBTreeNode node)
	{
		if (node.IsNil)
		{
			return -1;
		}
		return (node.Value as ICompoundItem).Entry.EntryId;
	}

	public void InsertCopy(ICompoundStorage storageToCopy)
	{
		ICompoundStorage compoundStorage = CreateStorage(storageToCopy.Name);
		if (storageToCopy is CompoundStorageWrapper && compoundStorage is CompoundStorageWrapper && (storageToCopy as CompoundStorageWrapper).Entry.StorageGuid != Guid.Empty)
		{
			(compoundStorage as CompoundStorageWrapper).Entry.StorageGuid = new Guid((storageToCopy as CompoundStorageWrapper).Entry.StorageGuid.ToByteArray());
		}
		string[] streams = storageToCopy.Streams;
		int i = 0;
		for (int num = streams.Length; i < num; i++)
		{
			using CompoundStream streamToCopy = storageToCopy.OpenStream(streams[i]);
			compoundStorage.InsertCopy(streamToCopy);
		}
		string[] storages = storageToCopy.Storages;
		int j = 0;
		for (int num2 = storages.Length; j < num2; j++)
		{
			using ICompoundStorage storageToCopy2 = storageToCopy.OpenStorage(storages[j]);
			compoundStorage.InsertCopy(storageToCopy2);
		}
	}

	public void InsertCopy(CompoundStream streamToCopy)
	{
		if (streamToCopy == null)
		{
			throw new ArgumentNullException("streamToCopy");
		}
		CompoundStream compoundStream = CreateStream(streamToCopy.Name);
		byte[] buffer = new byte[32768];
		long position = streamToCopy.Position;
		streamToCopy.Position = 0L;
		int count;
		while ((count = streamToCopy.Read(buffer, 0, 32768)) > 0)
		{
			compoundStream.Write(buffer, 0, count);
		}
		streamToCopy.Position = position;
	}
}
