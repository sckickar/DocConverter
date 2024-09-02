using System;
using System.IO;
using DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class BreakDescriptorRecord : BaseWordRecord
{
	private const int DEF_BIT_TABLE_BREAK = 0;

	private const int DEF_BIT_COLUMN_BREAK = 1;

	private const int DEF_BIT_MARKED = 2;

	private const int DEF_BIT_LIMIT_VALID = 3;

	internal const int DEF_RECORD_SIZE = 6;

	private BreakDescriptorStructure m_field = new BreakDescriptorStructure();

	internal short PageDescriptorIndex
	{
		get
		{
			return m_field.ipgd;
		}
		set
		{
			m_field.ipgd = value;
		}
	}

	internal short itxbxs
	{
		get
		{
			return m_field.itxbxs;
		}
		set
		{
			m_field.itxbxs = value;
		}
	}

	internal short CharPosNumber
	{
		get
		{
			return m_field.dcpDepend;
		}
		set
		{
			m_field.dcpDepend = value;
		}
	}

	internal ushort Options => m_field.Options;

	internal byte ColumnIndex
	{
		get
		{
			return m_field.iCol;
		}
		set
		{
			m_field.iCol = value;
		}
	}

	internal bool IsTableBreak
	{
		get
		{
			return BaseWordRecord.GetBit(Options, 0);
		}
		set
		{
			m_field.Options = (byte)BaseWordRecord.SetBit(m_field.Options, 0, value);
		}
	}

	internal bool IsColumnBreak
	{
		get
		{
			return BaseWordRecord.GetBit(Options, 1);
		}
		set
		{
			m_field.Options = (byte)BaseWordRecord.SetBit(m_field.Options, 1, value);
		}
	}

	internal bool IsMarked
	{
		get
		{
			return BaseWordRecord.GetBit(Options, 2);
		}
		set
		{
			m_field.Options = (byte)BaseWordRecord.SetBit(m_field.Options, 1, value);
		}
	}

	internal bool IsLimitValid
	{
		get
		{
			return BaseWordRecord.GetBit(Options, 3);
		}
		set
		{
			m_field.Options = (byte)BaseWordRecord.SetBit(m_field.Options, 3, value);
		}
	}

	internal bool IsTextOverflow
	{
		get
		{
			return BaseWordRecord.GetBit(Options, 2);
		}
		set
		{
			m_field.Options = (byte)BaseWordRecord.SetBit(m_field.Options, 1, value);
		}
	}

	protected override IDataStructure UnderlyingStructure => m_field;

	internal override int Length => 6;

	internal BreakDescriptorRecord()
	{
	}

	internal BreakDescriptorRecord(byte[] data)
		: base(data)
	{
	}

	internal BreakDescriptorRecord(byte[] arrData, int iOffset)
		: base(arrData, iOffset)
	{
	}

	internal BreakDescriptorRecord(byte[] arrData, int iOffset, int iCount)
		: base(arrData, iOffset, iCount)
	{
	}

	internal BreakDescriptorRecord(Stream stream, int iCount)
		: base(stream, iCount)
	{
	}
}
