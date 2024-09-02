using System.IO;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.Escher;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class MsofbtDg : BaseEscherRecord
{
	private int m_shapeCount;

	private int m_spidLast;

	internal int DrawingId
	{
		get
		{
			return base.Header.Instance;
		}
		set
		{
			base.Header.Instance = value;
		}
	}

	internal int ShapeCount
	{
		get
		{
			return m_shapeCount;
		}
		set
		{
			m_shapeCount = value;
		}
	}

	internal int SpidLast
	{
		get
		{
			return m_spidLast;
		}
		set
		{
			m_spidLast = value;
		}
	}

	internal MsofbtDg(WordDocument doc)
		: base(MSOFBT.msofbtDg, 0, doc)
	{
		m_shapeCount = 1;
	}

	protected override void ReadRecordData(Stream stream)
	{
		m_shapeCount = BaseWordRecord.ReadInt32(stream);
		m_spidLast = BaseWordRecord.ReadInt32(stream);
	}

	protected override void WriteRecordData(Stream stream)
	{
		BaseWordRecord.WriteInt32(stream, m_shapeCount);
		BaseWordRecord.WriteInt32(stream, m_spidLast);
	}

	internal override BaseEscherRecord Clone()
	{
		MsofbtDg obj = (MsofbtDg)MemberwiseClone();
		obj.m_doc = m_doc;
		return obj;
	}
}
