using System;

namespace DocGen.CompoundFile.DocIO.Net;

internal class CompoundStorageWrapper : ICompoundStorage, IDisposable
{
	private CompoundStorage m_storage;

	public string[] Streams => m_storage.Streams;

	public string[] Storages => m_storage.Storages;

	public string Name => m_storage.Name;

	public DirectoryEntry Entry => m_storage.Entry;

	public CompoundStorageWrapper(CompoundStorage wrapped)
	{
		m_storage = wrapped;
	}

	public void Dispose()
	{
		if (m_storage != null)
		{
			m_storage = null;
			GC.SuppressFinalize(this);
		}
	}

	public CompoundStream CreateStream(string streamName)
	{
		return m_storage.CreateStream(streamName);
	}

	public CompoundStream OpenStream(string streamName)
	{
		return m_storage.OpenStream(streamName);
	}

	public void DeleteStream(string streamName)
	{
		m_storage.DeleteStream(streamName);
	}

	public bool ContainsStream(string streamName)
	{
		return m_storage.ContainsStream(streamName);
	}

	public ICompoundStorage CreateStorage(string storageName)
	{
		return m_storage.CreateStorage(storageName);
	}

	public ICompoundStorage OpenStorage(string storageName)
	{
		return m_storage.OpenStorage(storageName);
	}

	public void DeleteStorage(string storageName)
	{
		m_storage.DeleteStorage(storageName);
	}

	public bool ContainsStorage(string storageName)
	{
		return m_storage.ContainsStorage(storageName);
	}

	public void Flush()
	{
		m_storage.Flush();
	}

	public void InsertCopy(ICompoundStorage storageToCopy)
	{
		m_storage.InsertCopy(storageToCopy);
	}

	internal void UpdateStorageGuid(ICompoundStorage storageToCopy)
	{
		m_storage.Entry.StorageGuid = (storageToCopy as CompoundStorageWrapper).m_storage.Entry.StorageGuid;
	}

	public void InsertCopy(CompoundStream streamToCopy)
	{
		m_storage.InsertCopy(streamToCopy);
	}
}
