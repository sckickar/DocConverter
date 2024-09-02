using System.IO;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.Escher;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class MsofbtClientTextbox : BaseEscherRecord
{
	private int m_txId;

	internal int Txid
	{
		get
		{
			return m_txId;
		}
		set
		{
			m_txId = value;
		}
	}

	public MsofbtClientTextbox(WordDocument doc)
		: base(doc)
	{
		base.Header.Type = MSOFBT.msofbtClientTextbox;
	}

	protected override void ReadRecordData(Stream stream)
	{
		m_txId = BaseWordRecord.ReadInt32(stream);
	}

	protected override void WriteRecordData(Stream stream)
	{
		BaseWordRecord.WriteInt32(stream, m_txId);
	}

	internal override BaseEscherRecord Clone()
	{
		MsofbtClientTextbox obj = (MsofbtClientTextbox)MemberwiseClone();
		obj.m_doc = m_doc;
		return obj;
	}
}
