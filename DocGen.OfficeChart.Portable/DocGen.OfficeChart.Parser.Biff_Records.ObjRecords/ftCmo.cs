using System;

namespace DocGen.OfficeChart.Parser.Biff_Records.ObjRecords;

[CLSCompliant(false)]
internal class ftCmo : ObjSubRecord
{
	private const int DEF_CHANGE_COLOR_MASK = 256;

	private TObjType m_ObjType;

	private ushort m_usId;

	private ushort m_usOptions;

	private byte[] m_reserved = new byte[12];

	private bool m_bBadLength;

	public bool Locked
	{
		get
		{
			return (m_usOptions & 1) > 0;
		}
		set
		{
			if (value)
			{
				m_usOptions |= 1;
			}
			else
			{
				m_usOptions &= 65534;
			}
		}
	}

	public bool Printable
	{
		get
		{
			return (m_usOptions & 0x10) > 0;
		}
		set
		{
			if (value)
			{
				m_usOptions |= 16;
			}
			else
			{
				m_usOptions &= 65519;
			}
		}
	}

	public bool AutoFill
	{
		get
		{
			return (m_usOptions & 0x2000) > 0;
		}
		set
		{
			if (value)
			{
				m_usOptions |= 8192;
			}
			else
			{
				m_usOptions &= 57343;
			}
		}
	}

	public bool AutoLine
	{
		get
		{
			return (m_usOptions & 0x4000) > 0;
		}
		set
		{
			if (value)
			{
				m_usOptions |= 16384;
			}
			else
			{
				m_usOptions &= 49151;
			}
		}
	}

	public bool ChangeColor
	{
		get
		{
			return (m_usOptions & 0x100) > 0;
		}
		set
		{
			if (value)
			{
				m_usOptions |= 256;
			}
			else
			{
				m_usOptions &= 65279;
			}
		}
	}

	public ushort ID
	{
		get
		{
			return m_usId;
		}
		set
		{
			m_usId = value;
		}
	}

	public TObjType ObjectType
	{
		get
		{
			return m_ObjType;
		}
		set
		{
			m_ObjType = value;
		}
	}

	public byte[] Reserved
	{
		get
		{
			return m_reserved;
		}
		internal set
		{
			m_reserved = value;
		}
	}

	public ushort Options
	{
		get
		{
			return m_usOptions;
		}
		internal set
		{
			m_usOptions = value;
		}
	}

	public ftCmo()
		: base(TObjSubRecordType.ftCmo)
	{
	}

	public ftCmo(TObjSubRecordType type, ushort length, byte[] buffer)
		: base(type, length, buffer)
	{
	}

	protected override void Parse(byte[] buffer)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (buffer.Length == 0)
		{
			m_bBadLength = true;
			return;
		}
		m_ObjType = (TObjType)BitConverter.ToInt16(buffer, 0);
		m_usId = BitConverter.ToUInt16(buffer, 2);
		m_usOptions = BitConverter.ToUInt16(buffer, 4);
		int num = 0;
		int num2 = 6;
		while (num2 < 18)
		{
			m_reserved[num] = buffer[num2];
			num2++;
			num++;
		}
	}

	public override void FillArray(DataProvider provider, int iOffset)
	{
		if (!m_bBadLength)
		{
			provider.WriteInt16(iOffset, (short)base.Type);
			iOffset += 2;
			provider.WriteInt16(iOffset, 18);
			iOffset += 2;
			provider.WriteInt16(iOffset, (short)m_ObjType);
			iOffset += 2;
			provider.WriteUInt16(iOffset, m_usId);
			iOffset += 2;
			provider.WriteUInt16(iOffset, m_usOptions);
			iOffset += 2;
			provider.WriteBytes(iOffset, m_reserved, 0, m_reserved.Length);
		}
		else
		{
			provider.WriteInt16(iOffset, (short)base.Type);
			iOffset += 2;
			provider.WriteInt16(iOffset, 0);
		}
	}

	public override object Clone()
	{
		ftCmo obj = (ftCmo)base.Clone();
		obj.m_reserved = CloneUtils.CloneByteArray(m_reserved);
		return obj;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		if (m_bBadLength)
		{
			return 4;
		}
		return 22;
	}
}
