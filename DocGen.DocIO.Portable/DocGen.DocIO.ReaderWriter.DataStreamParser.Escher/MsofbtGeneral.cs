using System.IO;
using DocGen.DocIO.DLS;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class MsofbtGeneral : BaseEscherRecord
{
	private byte[] m_data;

	internal byte[] Data
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

	internal MsofbtGeneral(WordDocument doc)
		: base(doc)
	{
	}

	protected override void ReadRecordData(Stream stream)
	{
		m_data = new byte[base.Header.Length];
		stream.Read(m_data, 0, base.Header.Length);
	}

	protected override void WriteRecordData(Stream stream)
	{
		stream.Write(m_data, 0, m_data.Length);
	}

	internal override BaseEscherRecord Clone()
	{
		MsofbtGeneral msofbtGeneral = new MsofbtGeneral(m_doc);
		msofbtGeneral.m_data = new byte[m_data.Length];
		m_data.CopyTo(msofbtGeneral.m_data, 0);
		msofbtGeneral.Header = base.Header.Clone();
		msofbtGeneral.m_doc = m_doc;
		return msofbtGeneral;
	}

	internal override void Close()
	{
		base.Close();
		m_data = null;
	}
}
