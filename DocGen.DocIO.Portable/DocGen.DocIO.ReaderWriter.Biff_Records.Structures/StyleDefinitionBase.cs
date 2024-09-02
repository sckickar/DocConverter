using System;

namespace DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

[CLSCompliant(false)]
internal class StyleDefinitionBase : IDataStructure
{
	internal const int DEF_RECORD_SIZE = 12;

	private const int DEF_START_ID = 0;

	private const int DEF_MASK_ID = 4095;

	private const int DEF_BIT_SCRATCH = 12;

	private const int DEF_BIT_INVALID_HEIGHT = 13;

	private const int DEF_BIT_HAS_UPE = 14;

	private const int DEF_BIT_MASS_COPY = 15;

	private const int DEF_MASK_TYPE_CODE = 15;

	private const int DEF_START_TYPE_CODE = 0;

	private const int DEF_MASK_BASE_STYLE = 65520;

	private const int DEF_START_BASE_STYLE = 4;

	private const int DEF_MASK_UPX_NUMBER = 15;

	private const int DEF_START_UPX_NUMBER = 0;

	private const int DEF_MASK_NEXT_STYLE = 65520;

	private const int DEF_START_NEXT_STYLE = 4;

	private const int DEF_BIT_AUTO_REDEFINE = 0;

	private const int DEF_BIT_HIDDEN = 1;

	private const int DEF_BIT_SEMIHIDDEN = 8;

	private const int DEF_BIT_UNHIDEUSED = 11;

	private const int DEF_BIT_QFORMAT = 12;

	private ushort m_usOptions1;

	private ushort m_usOptions2;

	private ushort m_usOptions3;

	private ushort m_usUpeOffset;

	private ushort m_usOptions4;

	private ushort m_usOptions5;

	internal ushort StyleId
	{
		get
		{
			return (ushort)BaseWordRecord.GetBitsByMask(m_usOptions1, 4095, 0);
		}
		set
		{
			m_usOptions1 = (ushort)BaseWordRecord.SetBitsByMask(m_usOptions1, 4095, 0, value);
		}
	}

	internal bool IsScratch
	{
		get
		{
			return BaseWordRecord.GetBit(m_usOptions1, 12);
		}
		set
		{
			m_usOptions1 = (ushort)BaseWordRecord.SetBit(m_usOptions1, 12, value);
		}
	}

	internal bool IsInvalidHeight
	{
		get
		{
			return BaseWordRecord.GetBit(m_usOptions1, 13);
		}
		set
		{
			m_usOptions1 = (ushort)BaseWordRecord.SetBit(m_usOptions1, 13, value);
		}
	}

	internal bool HasUpe
	{
		get
		{
			return BaseWordRecord.GetBit(m_usOptions1, 14);
		}
		set
		{
			m_usOptions1 = (ushort)BaseWordRecord.SetBit(m_usOptions1, 14, value);
		}
	}

	internal bool IsMassCopy
	{
		get
		{
			return BaseWordRecord.GetBit(m_usOptions1, 15);
		}
		set
		{
			m_usOptions1 = (ushort)BaseWordRecord.SetBit(m_usOptions1, 15, value);
		}
	}

	internal ushort TypeCode
	{
		get
		{
			return (ushort)BaseWordRecord.GetBitsByMask(m_usOptions2, 15, 0);
		}
		set
		{
			m_usOptions2 = (ushort)BaseWordRecord.SetBitsByMask(m_usOptions2, 15, 0, value);
		}
	}

	internal ushort BaseStyle
	{
		get
		{
			return (ushort)BaseWordRecord.GetBitsByMask(m_usOptions2, 65520, 4);
		}
		set
		{
			m_usOptions2 = (ushort)BaseWordRecord.SetBitsByMask(m_usOptions2, 65520, 4, value);
		}
	}

	internal ushort UPEOffset
	{
		get
		{
			return m_usUpeOffset;
		}
		set
		{
			m_usUpeOffset = value;
		}
	}

	internal ushort UpxNumber
	{
		get
		{
			return (ushort)BaseWordRecord.GetBitsByMask(m_usOptions3, 15, 0);
		}
		set
		{
			m_usOptions3 = (ushort)BaseWordRecord.SetBitsByMask(m_usOptions3, 15, 0, value);
		}
	}

	internal ushort NextStyleId
	{
		get
		{
			return (ushort)BaseWordRecord.GetBitsByMask(m_usOptions3, 65520, 4);
		}
		set
		{
			m_usOptions3 = (ushort)BaseWordRecord.SetBitsByMask(m_usOptions3, 65520, 4, value);
		}
	}

	internal bool IsAutoRedefine
	{
		get
		{
			return BaseWordRecord.GetBit(m_usOptions4, 0);
		}
		set
		{
			m_usOptions4 = (ushort)BaseWordRecord.SetBit(m_usOptions4, 0, value);
		}
	}

	internal bool IsHidden
	{
		get
		{
			return BaseWordRecord.GetBit(m_usOptions4, 1);
		}
		set
		{
			m_usOptions4 = (ushort)BaseWordRecord.SetBit(m_usOptions4, 1, value);
		}
	}

	public int Length => 12;

	internal ushort LinkStyleId
	{
		get
		{
			return (ushort)BaseWordRecord.GetBitsByMask(m_usOptions5, 4095, 0);
		}
		set
		{
			m_usOptions5 = (ushort)BaseWordRecord.SetBitsByMask(m_usOptions5, 4095, 0, value);
		}
	}

	internal bool IsSemiHidden
	{
		get
		{
			return BaseWordRecord.GetBit(m_usOptions4, 8);
		}
		set
		{
			m_usOptions4 = (ushort)BaseWordRecord.SetBit(m_usOptions4, 8, value);
		}
	}

	internal bool IsQFormat
	{
		get
		{
			return BaseWordRecord.GetBit(m_usOptions4, 12);
		}
		set
		{
			m_usOptions4 = (ushort)BaseWordRecord.SetBit(m_usOptions4, 12, value);
		}
	}

	internal bool UnhideWhenUsed
	{
		get
		{
			return BaseWordRecord.GetBit(m_usOptions4, 11);
		}
		set
		{
			m_usOptions4 = (ushort)BaseWordRecord.SetBit(m_usOptions4, 11, value);
		}
	}

	internal void Clear()
	{
		m_usOptions1 = 0;
		m_usOptions2 = 0;
		m_usOptions3 = 0;
		m_usOptions4 = 0;
		m_usUpeOffset = 0;
		m_usOptions5 = 0;
	}

	public void Parse(byte[] arrData, int iOffset)
	{
		m_usOptions1 = ByteConverter.ReadUInt16(arrData, ref iOffset);
		m_usOptions2 = ByteConverter.ReadUInt16(arrData, ref iOffset);
		m_usOptions3 = ByteConverter.ReadUInt16(arrData, ref iOffset);
		m_usUpeOffset = ByteConverter.ReadUInt16(arrData, ref iOffset);
		m_usOptions4 = ByteConverter.ReadUInt16(arrData, ref iOffset);
		m_usOptions5 = ByteConverter.ReadUInt16(arrData, ref iOffset);
	}

	public int Save(byte[] arrData, int iOffset)
	{
		ByteConverter.WriteUInt16(arrData, ref iOffset, m_usOptions1);
		ByteConverter.WriteUInt16(arrData, ref iOffset, m_usOptions2);
		ByteConverter.WriteUInt16(arrData, ref iOffset, m_usOptions3);
		ByteConverter.WriteUInt16(arrData, ref iOffset, m_usUpeOffset);
		ByteConverter.WriteUInt16(arrData, ref iOffset, m_usOptions4);
		ByteConverter.WriteUInt16(arrData, ref iOffset, m_usOptions5);
		return 12;
	}
}
