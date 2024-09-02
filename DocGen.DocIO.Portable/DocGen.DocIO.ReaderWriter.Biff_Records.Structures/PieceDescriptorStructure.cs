using System;

namespace DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

[CLSCompliant(false)]
internal class PieceDescriptorStructure : IDataStructure
{
	private const int DEF_PRM_OFFSET = 6;

	private const int DEF_RECORD_SIZE = 8;

	private ushort m_usOptions;

	private uint m_fc;

	private PropertyModifierStructure m_prm;

	internal ushort Options
	{
		get
		{
			return m_usOptions;
		}
		set
		{
			m_usOptions = value;
		}
	}

	internal uint FileOffset
	{
		get
		{
			return m_fc;
		}
		set
		{
			m_fc = value;
		}
	}

	internal PropertyModifierStructure PropertyModifier
	{
		get
		{
			return m_prm;
		}
		set
		{
			m_prm = value;
		}
	}

	public int Length => 6 + m_prm.Length;

	public void Parse(byte[] arrData, int iOffset)
	{
		m_usOptions = ByteConverter.ReadUInt16(arrData, ref iOffset);
		m_fc = ByteConverter.ReadUInt32(arrData, ref iOffset);
		m_prm.Parse(arrData, ref iOffset);
	}

	public int Save(byte[] arrData, int iOffset)
	{
		ByteConverter.WriteUInt16(arrData, ref iOffset, m_usOptions);
		ByteConverter.WriteUInt32(arrData, ref iOffset, m_fc);
		m_prm.Save(arrData, ref iOffset);
		return 8;
	}
}
