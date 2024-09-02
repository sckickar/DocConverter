using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

[CLSCompliant(false)]
internal class BXStructure : IDataStructure
{
	internal const int DEF_RECORD_SIZE = 13;

	private byte m_btOffset;

	private ParagraphHeight m_height = new ParagraphHeight();

	internal byte Offset
	{
		get
		{
			return m_btOffset;
		}
		set
		{
			m_btOffset = value;
		}
	}

	internal ParagraphHeight Height => m_height;

	public int Length => 13;

	internal BXStructure()
	{
	}

	public void Parse(byte[] arrData, int iOffset)
	{
		m_btOffset = arrData[iOffset];
		iOffset++;
	}

	public int Save(byte[] arrData, int iOffset)
	{
		arrData[iOffset] = m_btOffset;
		iOffset++;
		return m_height.Save(arrData, iOffset) + 1;
	}

	internal void Save(BinaryWriter writer)
	{
		writer.Write(m_btOffset);
		ParagraphHeightStructure structure = m_height.Structure;
		writer.Write(structure.Options);
		writer.Write(structure.Width);
		writer.Write(structure.Height);
	}
}
