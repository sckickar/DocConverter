using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace DocGen.Compression.Zip;

public class ZipArchive : IDisposable
{
	public delegate Stream CompressorCreator(Stream outputStream);

	private List<ZipArchiveItem> m_arrItems = new List<ZipArchiveItem>();

	private Dictionary<string, ZipArchiveItem> m_dicItems = new Dictionary<string, ZipArchiveItem>(StringComparer.OrdinalIgnoreCase);

	private IFileNamePreprocessor m_fileNamePreprocessor;

	private bool m_bCheckCrc = true;

	private CompressionLevel m_defaultLevel = CompressionLevel.Best;

	private bool m_netCompression;

	private string m_password;

	private EncryptionAlgorithm m_encryptType;

	public CompressorCreator CreateCompressor;

	public ZipArchiveItem this[int index]
	{
		get
		{
			if (index < 0 || index > m_arrItems.Count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			return m_arrItems[index];
		}
	}

	public ZipArchiveItem this[string itemName]
	{
		get
		{
			m_dicItems.TryGetValue(itemName, out var value);
			return value;
		}
	}

	public int Count
	{
		get
		{
			if (m_arrItems == null)
			{
				return 0;
			}
			return m_arrItems.Count;
		}
	}

	public ZipArchiveItem[] Items
	{
		get
		{
			if (m_arrItems != null)
			{
				return m_arrItems.ToArray();
			}
			throw new ArgumentOutOfRangeException("Items");
		}
	}

	public IFileNamePreprocessor FileNamePreprocessor
	{
		get
		{
			return m_fileNamePreprocessor;
		}
		set
		{
			m_fileNamePreprocessor = value;
		}
	}

	public CompressionLevel DefaultCompressionLevel
	{
		get
		{
			return m_defaultLevel;
		}
		set
		{
			m_defaultLevel = value;
		}
	}

	public bool CheckCrc
	{
		get
		{
			return m_bCheckCrc;
		}
		set
		{
			m_bCheckCrc = value;
		}
	}

	public bool UseNetCompression
	{
		get
		{
			return m_netCompression;
		}
		set
		{
			m_netCompression = value;
		}
	}

	public EncryptionAlgorithm EncryptionAlgorithm
	{
		get
		{
			return m_encryptType;
		}
		internal set
		{
			m_encryptType = value;
		}
	}

	internal string Password
	{
		get
		{
			return m_password;
		}
		set
		{
			m_password = value;
		}
	}

	[CLSCompliant(false)]
	public static long FindValueFromEnd(Stream stream, uint value, int maxCount)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (!stream.CanSeek || !stream.CanRead)
		{
			throw new ArgumentOutOfRangeException("We need to have seekable and readable stream.");
		}
		long length = stream.Length;
		if (length < 4)
		{
			return -1L;
		}
		byte[] array = new byte[4];
		long num = Math.Max(0L, length - maxCount);
		long num3 = (stream.Position = length - 1 - 4);
		stream.Read(array, 0, 4);
		uint num4 = BitConverter.ToUInt32(array, 0);
		bool flag = num4 == value;
		if (!flag)
		{
			while (num3 > num)
			{
				num4 <<= 8;
				num3 = (stream.Position = num3 - 1);
				num4 += (uint)stream.ReadByte();
				if (num4 == value)
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			return -1L;
		}
		return num3;
	}

	public static int ReadInt32(Stream stream)
	{
		byte[] array = new byte[4];
		if (stream.Read(array, 0, 4) != 4)
		{
			throw new ZipException("Unable to read value at the specified position - end of stream was reached.");
		}
		return BitConverter.ToInt32(array, 0);
	}

	public static short ReadInt16(Stream stream)
	{
		byte[] array = new byte[2];
		if (stream.Read(array, 0, 2) != 2)
		{
			throw new ZipException("Unable to read value at the specified position - end of stream was reached.");
		}
		return BitConverter.ToInt16(array, 0);
	}

	public static ushort ReadUInt16(Stream stream)
	{
		byte[] array = new byte[2];
		if (stream.Read(array, 0, 2) != 2)
		{
			throw new ZipException("Unable to read value at the specified position - end of stream was reached.");
		}
		return BitConverter.ToUInt16(array, 0);
	}

	internal static uint ReadUInt32(Stream stream)
	{
		byte[] array = new byte[4];
		if (stream.Read(array, 0, 4) != 4)
		{
			throw new ZipException("Unable to read value at the specified position - end of stream was reached.");
		}
		return BitConverter.ToUInt32(array, 0);
	}

	public ZipArchive()
	{
		CreateCompressor = CreateNativeCompressor;
	}

	private Stream CreateNativeCompressor(Stream outputStream)
	{
		if (m_netCompression)
		{
			return new NetCompressor(CompressionLevel.Best, outputStream);
		}
		return new DeflateStream(outputStream, CompressionMode.Compress, leaveOpen: true);
	}

	public ZipArchiveItem AddItem(string itemName, Stream data, bool bControlStream, FileAttributes attributes)
	{
		itemName = itemName.Replace('\\', '/');
		if (itemName.IndexOf(':') != itemName.LastIndexOf(':'))
		{
			throw new ArgumentOutOfRangeException("ZipItem name contains illegal characters.", "itemName");
		}
		if (m_dicItems.ContainsKey(itemName))
		{
			throw new ArgumentOutOfRangeException("Item " + itemName + " already exists in the archive");
		}
		ZipArchiveItem zipArchiveItem = new ZipArchiveItem(this, itemName, data, bControlStream, attributes);
		zipArchiveItem.CompressionLevel = m_defaultLevel;
		return AddItem(zipArchiveItem);
	}

	public ZipArchiveItem AddItem(ZipArchiveItem item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		m_arrItems.Add(item);
		m_dicItems.Add(item.ItemName, item);
		return item;
	}

	public void RemoveItem(string itemName)
	{
		int num = Find(itemName);
		if (num >= 0)
		{
			RemoveAt(num);
		}
	}

	public void RemoveAt(int index)
	{
		if (index < 0 || index >= m_arrItems.Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		ZipArchiveItem zipArchiveItem = this[index];
		m_arrItems.RemoveAt(index);
		m_dicItems.Remove(zipArchiveItem.ItemName);
	}

	public void Remove(Regex mask)
	{
		int i = 0;
		for (int num = m_arrItems.Count; i < num; i++)
		{
			string itemName = m_arrItems[i].ItemName;
			if (mask.IsMatch(itemName))
			{
				m_arrItems.RemoveAt(i);
				m_dicItems.Remove(itemName);
				i--;
				num--;
			}
		}
	}

	public void UpdateItem(string itemName, Stream newDataStream, bool controlStream)
	{
		(this[itemName] ?? throw new ArgumentOutOfRangeException("itemName", "Cannot find specified item.")).Update(newDataStream, controlStream);
	}

	public void UpdateItem(string itemName, Stream newDataStream, bool controlStream, FileAttributes attributes)
	{
		ZipArchiveItem zipArchiveItem = this[itemName];
		if (zipArchiveItem != null)
		{
			zipArchiveItem.Update(newDataStream, controlStream);
		}
		else
		{
			AddItem(itemName, newDataStream, controlStream, attributes);
		}
	}

	public void UpdateItem(string itemName, byte[] newData)
	{
		ZipArchiveItem obj = this[itemName] ?? throw new ArgumentOutOfRangeException("itemName", "Cannot find specified item.");
		MemoryStream newDataStream = new MemoryStream(newData);
		obj.Update(newDataStream, controlStream: true);
	}

	public void Save(Stream stream, bool closeStream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException();
		}
		Stream stream2 = null;
		if (!stream.CanSeek)
		{
			stream2 = stream;
			stream = new MemoryStream();
		}
		stream.Position = 0L;
		int i = 0;
		for (int count = m_arrItems.Count; i < count; i++)
		{
			m_arrItems[i].Write(stream);
		}
		WriteCentralDirectory(stream);
		if (stream2 != null)
		{
			stream.Position = 0L;
			((MemoryStream)stream).WriteTo(stream2);
			stream.Close();
			stream = stream2;
		}
		if (closeStream)
		{
			stream.Close();
		}
	}

	public void Open(Stream stream, bool closeStream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		_ = new byte[4];
		long num = FindValueFromEnd(stream, 101010256u, 65557);
		if (num < 0)
		{
			throw new ZipException("Can't locate end of central directory record. Possible wrong file format or archive is corrupt.");
		}
		stream.Position = num + 12;
		int num2 = ReadInt32(stream);
		long position = num - num2;
		stream.Position = position;
		ReadCentralDirectoryData(stream);
		ExtractItems(stream);
	}

	public void Close()
	{
		int i = 0;
		for (int count = m_arrItems.Count; i < count; i++)
		{
			m_arrItems[i].Close();
		}
		m_arrItems.Clear();
		m_dicItems.Clear();
		m_dicItems = null;
	}

	public int Find(string itemName)
	{
		int result = -1;
		if (m_dicItems.TryGetValue(itemName, out var value))
		{
			int i = 0;
			for (int count = m_arrItems.Count; i < count; i++)
			{
				if (m_arrItems[i] == value)
				{
					result = i;
					break;
				}
			}
		}
		return result;
	}

	public int Find(Regex itemRegex)
	{
		int result = -1;
		int i = 0;
		for (int count = m_arrItems.Count; i < count; i++)
		{
			string itemName = m_arrItems[i].ItemName;
			if (itemRegex.IsMatch(itemName))
			{
				result = i;
				break;
			}
		}
		return result;
	}

	private void WriteCentralDirectory(Stream stream)
	{
		long position = stream.Position;
		int i = 0;
		for (int count = m_arrItems.Count; i < count; i++)
		{
			m_arrItems[i].WriteFileHeader(stream);
		}
		WriteCentralDirectoryEnd(stream, position);
	}

	private void WriteCentralDirectoryEnd(Stream stream, long directoryStart)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		int value = (int)(stream.Position - directoryStart);
		stream.Write(BitConverter.GetBytes(101010256), 0, 4);
		stream.WriteByte(0);
		stream.WriteByte(0);
		stream.WriteByte(0);
		stream.WriteByte(0);
		byte[] bytes = BitConverter.GetBytes((short)m_arrItems.Count);
		stream.Write(bytes, 0, 2);
		stream.Write(bytes, 0, 2);
		stream.Write(BitConverter.GetBytes(value), 0, 4);
		stream.Write(BitConverter.GetBytes((int)directoryStart), 0, 4);
		stream.WriteByte(0);
		stream.WriteByte(0);
	}

	private void ReadCentralDirectoryData(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		while (ReadInt32(stream) == 33639248)
		{
			ZipArchiveItem zipArchiveItem = new ZipArchiveItem(this);
			zipArchiveItem.ReadCentralDirectoryData(stream);
			m_arrItems.Add(zipArchiveItem);
		}
	}

	private void ExtractItems(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException();
		}
		if (!stream.CanSeek || !stream.CanRead)
		{
			throw new ArgumentOutOfRangeException("stream", "We need seekable and readable stream to parse items.");
		}
		int i = 0;
		for (int count = m_arrItems.Count; i < count; i++)
		{
			ZipArchiveItem zipArchiveItem = m_arrItems[i];
			zipArchiveItem.ReadData(stream, m_bCheckCrc);
			m_dicItems.Add(zipArchiveItem.ItemName, zipArchiveItem);
		}
	}

	public ZipArchive Clone()
	{
		ZipArchive zipArchive = (ZipArchive)MemberwiseClone();
		zipArchive.m_arrItems = new List<ZipArchiveItem>();
		zipArchive.m_dicItems = new Dictionary<string, ZipArchiveItem>();
		int i = 0;
		for (int count = m_arrItems.Count; i < count; i++)
		{
			ZipArchiveItem zipArchiveItem = m_arrItems[i];
			zipArchiveItem = zipArchiveItem.Clone();
			zipArchive.AddItem(zipArchiveItem);
		}
		return zipArchive;
	}

	public void Protect(string password, EncryptionAlgorithm type)
	{
		if (string.IsNullOrEmpty(password))
		{
			throw new ArgumentNullException("password");
		}
		if (type != 0)
		{
			m_encryptType = type;
			m_password = password;
		}
	}

	public void UnProtect()
	{
		m_password = null;
		m_encryptType = EncryptionAlgorithm.None;
	}

	public void Open(Stream stream, bool closeStream, string password)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (string.IsNullOrEmpty(password))
		{
			throw new ArgumentNullException("password");
		}
		m_password = password;
		Open(stream, closeStream);
	}

	public void Dispose()
	{
		if (m_arrItems != null)
		{
			int i = 0;
			for (int count = m_arrItems.Count; i < count; i++)
			{
				m_arrItems[i].Dispose();
			}
		}
		if (m_password != null)
		{
			m_password = null;
		}
		if (m_dicItems != null)
		{
			m_dicItems.Clear();
			m_dicItems = null;
		}
		GC.SuppressFinalize(this);
	}

	~ZipArchive()
	{
		if (m_arrItems != null)
		{
			Dispose();
		}
	}
}
