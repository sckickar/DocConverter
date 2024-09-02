using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class FieldDescriptor : BaseWordRecord
{
	private byte m_ch;

	private byte m_reserved;

	private byte m_fieldType;

	internal bool HasSeparator
	{
		get
		{
			return (m_fieldType & 0x80) != 0;
		}
		set
		{
			m_fieldType = (byte)BaseWordRecord.SetBit(m_fieldType, 7, value);
		}
	}

	internal bool IsResultDirty
	{
		get
		{
			return (m_fieldType & 4) != 0;
		}
		set
		{
			m_fieldType = (byte)BaseWordRecord.SetBit(m_fieldType, 2, value);
		}
	}

	internal bool IsResultEdited
	{
		get
		{
			return (m_fieldType & 8) != 0;
		}
		set
		{
			m_fieldType = (byte)BaseWordRecord.SetBit(m_fieldType, 3, value);
		}
	}

	internal bool IsLocked
	{
		get
		{
			if (m_ch != 21)
			{
				return false;
			}
			return BaseWordRecord.GetBit(m_fieldType, 4);
		}
		set
		{
			m_fieldType = (byte)BaseWordRecord.SetBit(m_fieldType, 4, value);
		}
	}

	internal bool IsNested
	{
		get
		{
			return (m_fieldType & 0x40) != 0;
		}
		set
		{
			m_fieldType = (byte)BaseWordRecord.SetBit(m_fieldType, 6, value);
		}
	}

	internal FieldType Type
	{
		get
		{
			return (FieldType)m_fieldType;
		}
		set
		{
			m_fieldType = (byte)value;
		}
	}

	internal byte FieldBoundary
	{
		get
		{
			return m_ch;
		}
		set
		{
			m_ch = value;
		}
	}

	internal FieldDescriptor(BinaryReader reader)
	{
		Read(reader);
	}

	internal FieldDescriptor()
	{
	}

	internal void Parse(short sh)
	{
		byte[] bytes = BitConverter.GetBytes(sh);
		m_ch = (byte)(bytes[0] & 0x1Fu);
		m_reserved = (byte)(bytes[0] & 0xE0u);
		m_fieldType = bytes[1];
	}

	internal short Save()
	{
		return BitConverter.ToInt16(new byte[2]
		{
			(byte)(m_ch | m_reserved),
			m_fieldType
		}, 0);
	}

	internal FieldDescriptor Clone()
	{
		return new FieldDescriptor
		{
			HasSeparator = HasSeparator,
			IsNested = IsNested,
			IsResultDirty = IsResultDirty,
			IsResultEdited = IsResultEdited,
			FieldBoundary = FieldBoundary,
			Type = Type
		};
	}

	internal void Read(BinaryReader reader)
	{
		byte[] array = reader.ReadBytes(2);
		m_ch = (byte)(array[0] & 0x1Fu);
		m_reserved = (byte)(array[0] & 0xE0u);
		m_fieldType = array[1];
	}

	internal void Write(Stream stream)
	{
		stream.WriteByte((byte)(m_ch | m_reserved));
		stream.WriteByte(m_fieldType);
	}
}
