using System;
using System.IO;
using System.Text;

namespace DocGen.CompoundFile.DocIO.Net;

internal class DirectoryEntry
{
	public enum EntryType
	{
		Invalid,
		Storage,
		Stream,
		LockBytes,
		Property,
		Root
	}

	public const int SizeInFile = 128;

	private const int StreamNameSize = 64;

	private string m_strName;

	private EntryType m_entryType;

	private byte m_color = 1;

	private int m_leftId = -1;

	private int m_rightId = -1;

	private int m_childId = -1;

	private Guid m_storageGuid;

	private int m_iStorageFlags;

	private DateTime m_dateCreate;

	private DateTime m_dateModify;

	private int m_iStartSector = -2;

	private uint m_uiSize;

	private int m_iReserved;

	private int m_iEntryId;

	public int LastSector = -1;

	public int LastOffset = -1;

	public string Name
	{
		get
		{
			return m_strName;
		}
		set
		{
			m_strName = value;
		}
	}

	public EntryType Type
	{
		get
		{
			return m_entryType;
		}
		set
		{
			m_entryType = value;
		}
	}

	public byte Color
	{
		get
		{
			return m_color;
		}
		set
		{
			m_color = value;
		}
	}

	public int LeftId
	{
		get
		{
			return m_leftId;
		}
		set
		{
			m_leftId = value;
		}
	}

	public int RightId
	{
		get
		{
			return m_rightId;
		}
		set
		{
			m_rightId = value;
		}
	}

	public int ChildId
	{
		get
		{
			return m_childId;
		}
		set
		{
			m_childId = value;
		}
	}

	public Guid StorageGuid
	{
		get
		{
			return m_storageGuid;
		}
		set
		{
			m_storageGuid = value;
		}
	}

	public int StorageFlags
	{
		get
		{
			return m_iStorageFlags;
		}
		set
		{
			m_iStorageFlags = value;
		}
	}

	public DateTime DateCreate
	{
		get
		{
			return m_dateCreate;
		}
		set
		{
			m_dateCreate = value;
		}
	}

	public DateTime DateModify
	{
		get
		{
			return m_dateModify;
		}
		set
		{
			m_dateModify = value;
		}
	}

	public int StartSector
	{
		get
		{
			return m_iStartSector;
		}
		set
		{
			m_iStartSector = value;
		}
	}

	public uint Size
	{
		get
		{
			return m_uiSize;
		}
		set
		{
			m_uiSize = value;
		}
	}

	public int Reserved
	{
		get
		{
			return m_iReserved;
		}
		set
		{
		}
	}

	public int EntryId
	{
		get
		{
			return m_iEntryId;
		}
		internal set
		{
			m_iEntryId = value;
		}
	}

	public DirectoryEntry(string name, EntryType type, int entryId)
	{
		m_strName = name;
		m_entryType = type;
		m_iEntryId = entryId;
		m_dateModify = (m_dateCreate = DateTime.Now);
	}

	public DirectoryEntry(byte[] data, int offset, int entryId)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (offset < 0 || offset >= data.Length)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		m_iEntryId = entryId;
		int count = BitConverter.ToUInt16(data, offset + 64);
		m_strName = Encoding.Unicode.GetString(data, offset, count);
		count = m_strName.Length;
		if (count > 0 && m_strName[count - 1] == '\0')
		{
			m_strName = m_strName.Substring(0, count - 1);
		}
		offset += 66;
		m_entryType = (EntryType)data[offset];
		offset++;
		m_color = data[offset];
		offset++;
		m_leftId = BitConverter.ToInt32(data, offset);
		offset += 4;
		m_rightId = BitConverter.ToInt32(data, offset);
		offset += 4;
		m_childId = BitConverter.ToInt32(data, offset);
		offset += 4;
		byte[] array = new byte[16];
		Buffer.BlockCopy(data, offset, array, 0, 16);
		m_storageGuid = new Guid(array);
		offset += 16;
		m_iStorageFlags = BitConverter.ToInt32(data, offset);
		offset += 4;
		long num = BitConverter.ToInt64(data, offset);
		long num2 = DateTime.MaxValue.ToFileTime();
		if (num >= 0 && num <= num2)
		{
			m_dateCreate = DateTime.FromFileTime(num);
		}
		offset += 8;
		num = BitConverter.ToInt64(data, offset);
		if (num >= 0 && num <= num2)
		{
			m_dateModify = DateTime.FromFileTime(num);
		}
		offset += 8;
		m_iStartSector = BitConverter.ToInt32(data, offset);
		offset += 4;
		m_uiSize = BitConverter.ToUInt32(data, offset);
		offset += 4;
		m_iReserved = BitConverter.ToInt32(data, offset);
		offset += 4;
	}

	public void Write(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		long position = stream.Position;
		if (m_entryType == EntryType.Invalid)
		{
			m_leftId = (m_rightId = (m_childId = -1));
		}
		byte[] bytes = Encoding.Unicode.GetBytes(m_strName);
		stream.Write(bytes, 0, bytes.Length);
		stream.WriteByte(0);
		stream.WriteByte(0);
		stream.Position = position + 64;
		bytes = BitConverter.GetBytes((short)(bytes.Length + 2));
		stream.Write(bytes, 0, 2);
		stream.WriteByte((byte)m_entryType);
		stream.WriteByte(m_color);
		bytes = BitConverter.GetBytes(m_leftId);
		stream.Write(bytes, 0, 4);
		bytes = BitConverter.GetBytes(m_rightId);
		stream.Write(bytes, 0, 4);
		bytes = BitConverter.GetBytes(m_childId);
		stream.Write(bytes, 0, 4);
		if (Type == EntryType.Root && m_storageGuid.CompareTo(Guid.Empty) == 0)
		{
			m_storageGuid = new Guid("00020820-0000-0000-c000-000000000046");
		}
		bytes = m_storageGuid.ToByteArray();
		stream.Write(bytes, 0, bytes.Length);
		bytes = BitConverter.GetBytes(m_iStorageFlags);
		stream.Write(bytes, 0, 4);
		bytes = BitConverter.GetBytes(0L);
		stream.Write(bytes, 0, bytes.Length);
		bytes = BitConverter.GetBytes(0L);
		stream.Write(bytes, 0, bytes.Length);
		bytes = BitConverter.GetBytes(m_iStartSector);
		stream.Write(bytes, 0, 4);
		bytes = BitConverter.GetBytes(m_uiSize);
		stream.Write(bytes, 0, 4);
		bytes = BitConverter.GetBytes(m_iReserved);
		stream.Write(bytes, 0, 4);
	}
}
