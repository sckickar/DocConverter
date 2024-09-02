using System.IO;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class FOPTEComplex : FOPTEBase
{
	private int m_dataLength;

	private byte[] m_data;

	internal byte[] Value
	{
		get
		{
			return m_data;
		}
		set
		{
			m_data = value;
		}
	}

	internal FOPTEComplex(int id, bool isBid, int valueLength)
		: base(id, isBid)
	{
		m_dataLength = valueLength;
	}

	internal void ReadData(Stream stream)
	{
		m_data = new byte[m_dataLength];
		stream.Read(m_data, 0, m_dataLength);
	}

	internal override void Write(Stream stream)
	{
		int id = base.Id;
		id |= (base.IsBid ? 16384 : 0);
		id |= 0x8000;
		BaseWordRecord.WriteInt16(stream, (short)id);
		BaseWordRecord.WriteInt32(stream, m_data.Length);
	}

	internal void WriteData(Stream stream)
	{
		stream.Write(m_data, 0, m_data.Length);
	}

	internal override FOPTEBase Clone()
	{
		FOPTEComplex fOPTEComplex = new FOPTEComplex(base.Id, base.IsBid, m_dataLength);
		fOPTEComplex.m_data = new byte[m_dataLength];
		m_data.CopyTo(fOPTEComplex.m_data, 0);
		return fOPTEComplex;
	}
}
