using System.Collections.Generic;
using System.IO;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.Escher;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class MsofbtDgg : BaseEscherRecord
{
	private int m_spidMax;

	private int m_shapeCount;

	private int m_drawingCount;

	private List<FIDCL> m_filCls;

	internal int DrawingCount
	{
		get
		{
			return m_drawingCount;
		}
		set
		{
			m_drawingCount = value;
		}
	}

	internal List<FIDCL> Fidcls => m_filCls;

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

	internal int SpidMax
	{
		get
		{
			return m_spidMax;
		}
		set
		{
			m_spidMax = value;
		}
	}

	internal MsofbtDgg(WordDocument doc)
		: base(MSOFBT.msofbtDgg, 0, doc)
	{
		m_filCls = new List<FIDCL>();
		m_shapeCount = 1;
	}

	protected override void ReadRecordData(Stream stream)
	{
		m_spidMax = BaseWordRecord.ReadInt32(stream);
		int num = BaseWordRecord.ReadInt32(stream) - 1;
		m_shapeCount = BaseWordRecord.ReadInt32(stream);
		m_drawingCount = BaseWordRecord.ReadInt32(stream);
		for (int i = 0; i < num; i++)
		{
			m_filCls.Add(new FIDCL(stream));
		}
	}

	protected override void WriteRecordData(Stream stream)
	{
		BaseWordRecord.WriteInt32(stream, m_spidMax);
		BaseWordRecord.WriteInt32(stream, m_filCls.Count + 1);
		BaseWordRecord.WriteInt32(stream, m_shapeCount);
		BaseWordRecord.WriteInt32(stream, m_drawingCount);
		for (int i = 0; i < m_filCls.Count; i++)
		{
			m_filCls[i].Write(stream);
		}
	}

	internal override BaseEscherRecord Clone()
	{
		MsofbtDgg msofbtDgg = (MsofbtDgg)MemberwiseClone();
		msofbtDgg.m_filCls = new List<FIDCL>(m_filCls.Count);
		int i = 0;
		for (int count = m_filCls.Count; i < count; i++)
		{
			msofbtDgg.m_filCls.Add(m_filCls[i]);
		}
		msofbtDgg.m_doc = m_doc;
		return msofbtDgg;
	}
}
