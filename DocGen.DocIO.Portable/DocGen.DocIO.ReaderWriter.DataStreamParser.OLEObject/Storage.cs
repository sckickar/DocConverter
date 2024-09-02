using System;
using System.Collections.Generic;
using System.IO;
using DocGen.CompoundFile.DocIO;
using DocGen.CompoundFile.DocIO.Net;
using DocGen.DocIO.DLS;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.OLEObject;

internal class Storage
{
	private Dictionary<string, Stream> m_streams = new Dictionary<string, Stream>();

	private List<Storage> m_storages = new List<Storage>();

	private string m_storageName;

	private int m_occurenceCount;

	private Guid m_guid;

	internal string StorageName
	{
		get
		{
			return m_storageName;
		}
		set
		{
			m_storageName = value;
		}
	}

	internal Guid Guid
	{
		get
		{
			return m_guid;
		}
		set
		{
			m_guid = value;
		}
	}

	internal Dictionary<string, Stream> Streams => m_streams;

	internal List<Storage> Storages => m_storages;

	internal int OccurrenceCount
	{
		get
		{
			return m_occurenceCount;
		}
		set
		{
			m_occurenceCount = value;
		}
	}

	internal Storage(string storageName)
	{
		m_storageName = storageName;
	}

	internal void ParseStorages(ICompoundStorage storage)
	{
		string[] storages = storage.Storages;
		foreach (string storageName in storages)
		{
			ICompoundStorage storage2 = storage.OpenStorage(storageName);
			Storage storage3 = new Storage(storageName);
			storage3.ParseStorages(storage2);
			storage3.ParseStreams(storage2);
			Storages.Add(storage3);
		}
	}

	internal void ParseStreams(ICompoundStorage storage)
	{
		string[] streams = storage.Streams;
		foreach (string text in streams)
		{
			CompoundStream compoundStream = storage.OpenStream(text);
			byte[] array = new byte[compoundStream.Length];
			compoundStream.Read(array, 0, array.Length);
			compoundStream.Dispose();
			Streams.Add(text, new MemoryStream(array));
		}
	}

	internal void WriteToStorage(ICompoundStorage storage)
	{
		foreach (Storage storage3 in Storages)
		{
			ICompoundStorage storage2 = storage.CreateStorage(storage3.StorageName);
			storage3.WriteToStorage(storage2);
		}
		foreach (KeyValuePair<string, Stream> stream in Streams)
		{
			CompoundStream compoundStream = storage.CreateStream(stream.Key);
			compoundStream.Write((stream.Value as MemoryStream).ToArray(), 0, (int)stream.Value.Length);
			compoundStream.Flush();
		}
		storage.Flush();
	}

	private byte[] GetByteArray(Stream stream)
	{
		stream.Position = 0L;
		byte[] array = new byte[stream.Length];
		stream.Read(array, 0, array.Length);
		return array;
	}

	internal string CompareStorage(Dictionary<string, Storage> oleObjectCollection)
	{
		bool flag = false;
		foreach (string key in oleObjectCollection.Keys)
		{
			if (oleObjectCollection[key].Streams.Count == Streams.Count)
			{
				foreach (string key2 in Streams.Keys)
				{
					if (!oleObjectCollection[key].Streams.ContainsKey(key2))
					{
						flag = false;
						break;
					}
					byte[] byteArray = GetByteArray(Streams[key2]);
					byte[] byteArray2 = GetByteArray(oleObjectCollection[key].Streams[key2]);
					if (byteArray.Length == byteArray2.Length)
					{
						flag = WordDocument.CompareArray(byteArray, byteArray2);
						continue;
					}
					flag = false;
					break;
				}
			}
			else
			{
				flag = false;
			}
			if (flag)
			{
				return key;
			}
		}
		return string.Empty;
	}

	internal void UpdateGuid(DocGen.CompoundFile.DocIO.Net.CompoundFile cmpFile, int storageIndex, string storageName)
	{
		for (int i = storageIndex; i < cmpFile.Directory.Entries.Count; i++)
		{
			DirectoryEntry directoryEntry = cmpFile.Directory.Entries[i];
			if (directoryEntry.Name == "_" + storageName || directoryEntry.Name == "Root Entry")
			{
				directoryEntry.StorageGuid = Guid;
			}
		}
	}

	internal Storage Clone()
	{
		Storage storage = new Storage(StorageName);
		foreach (KeyValuePair<string, Stream> stream in Streams)
		{
			MemoryStream value = new MemoryStream((stream.Value as MemoryStream).ToArray());
			storage.Streams.Add(stream.Key, value);
		}
		foreach (Storage storage2 in Storages)
		{
			storage.Storages.Add(storage2.Clone());
		}
		return storage;
	}

	internal void Close()
	{
		m_occurenceCount--;
		if (m_occurenceCount > 0)
		{
			return;
		}
		foreach (KeyValuePair<string, Stream> stream in Streams)
		{
			stream.Value.Close();
		}
		Streams.Clear();
		foreach (Storage storage in Storages)
		{
			storage.Close();
		}
		Storages.Clear();
	}
}
