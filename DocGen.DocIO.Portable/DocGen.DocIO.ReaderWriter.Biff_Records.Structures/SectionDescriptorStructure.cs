using System;

namespace DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

[CLSCompliant(false)]
internal class SectionDescriptorStructure : IDataStructure
{
	private const int DEF_RECORD_SIZE = 12;

	private short m_sInternal;

	private uint m_fcSepx;

	private short m_sInternal2;

	private int m_fcMpr;

	internal short Internal1
	{
		get
		{
			return m_sInternal;
		}
		set
		{
			m_sInternal = value;
		}
	}

	internal short Internal2
	{
		get
		{
			return m_sInternal2;
		}
		set
		{
			m_sInternal2 = value;
		}
	}

	internal uint SepxPosition
	{
		get
		{
			return m_fcSepx;
		}
		set
		{
			m_fcSepx = value;
		}
	}

	internal int MacPrintOffset
	{
		get
		{
			return m_fcMpr;
		}
		set
		{
			m_fcMpr = value;
		}
	}

	public int Length => 12;

	public void Parse(byte[] arrData, int iOffset)
	{
		m_sInternal = ByteConverter.ReadInt16(arrData, ref iOffset);
		m_fcSepx = ByteConverter.ReadUInt32(arrData, ref iOffset);
		m_sInternal2 = ByteConverter.ReadInt16(arrData, ref iOffset);
		m_fcMpr = ByteConverter.ReadInt32(arrData, ref iOffset);
	}

	public int Save(byte[] arrData, int iOffset)
	{
		ByteConverter.WriteInt16(arrData, ref iOffset, m_sInternal);
		ByteConverter.WriteUInt32(arrData, ref iOffset, m_fcSepx);
		ByteConverter.WriteInt16(arrData, ref iOffset, m_sInternal2);
		ByteConverter.WriteInt32(arrData, ref iOffset, m_fcMpr);
		return 12;
	}
}
