using System;

namespace DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

[CLSCompliant(false)]
internal struct PropertyModifierStructure
{
	private const int DEF_BIT_COMPLEX = 0;

	private const int DEF_RECORD_SIZE = 2;

	private ushort m_usOptions;

	internal bool IsComplex
	{
		get
		{
			return BaseWordRecord.GetBit(m_usOptions, 0);
		}
		set
		{
			m_usOptions = (ushort)BaseWordRecord.SetBit(m_usOptions, 0, value);
		}
	}

	internal int Length => 2;

	internal void Parse(byte[] arrData, ref int iOffset)
	{
		m_usOptions = ByteConverter.ReadUInt16(arrData, ref iOffset);
	}

	internal void Save(byte[] arrData, ref int iOffset)
	{
		ByteConverter.WriteUInt16(arrData, ref iOffset, m_usOptions);
	}
}
