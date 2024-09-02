using System.IO;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.Escher;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class MsofbtSpgr : BaseEscherRecord
{
	private int m_rectLeft;

	private int m_rectTop;

	private int m_rectRight;

	private int m_rectBottom;

	internal MsofbtSpgr(WordDocument doc)
		: base(MSOFBT.msofbtSpgr, 1, doc)
	{
	}

	protected override void ReadRecordData(Stream stream)
	{
		m_rectLeft = BaseWordRecord.ReadInt32(stream);
		m_rectTop = BaseWordRecord.ReadInt32(stream);
		m_rectRight = BaseWordRecord.ReadInt32(stream);
		m_rectBottom = BaseWordRecord.ReadInt32(stream);
	}

	protected override void WriteRecordData(Stream stream)
	{
		BaseWordRecord.WriteInt32(stream, m_rectLeft);
		BaseWordRecord.WriteInt32(stream, m_rectTop);
		BaseWordRecord.WriteInt32(stream, m_rectRight);
		BaseWordRecord.WriteInt32(stream, m_rectBottom);
	}

	internal override BaseEscherRecord Clone()
	{
		MsofbtSpgr obj = (MsofbtSpgr)MemberwiseClone();
		obj.m_doc = m_doc;
		return obj;
	}
}
