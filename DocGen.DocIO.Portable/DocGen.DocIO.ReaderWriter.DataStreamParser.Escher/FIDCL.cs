using System.IO;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class FIDCL : BaseWordRecord
{
	internal int m_dgid;

	internal int m_cspidCur;

	internal FIDCL(Stream stream)
	{
		Read(stream);
	}

	internal FIDCL(int dgid, int cspidCur)
	{
		m_dgid = dgid;
		m_cspidCur = cspidCur;
	}

	internal void Write(Stream stream)
	{
		BaseWordRecord.WriteInt32(stream, m_dgid);
		BaseWordRecord.WriteInt32(stream, m_cspidCur);
	}

	internal void Read(Stream stream)
	{
		m_dgid = BaseWordRecord.ReadInt32(stream);
		m_cspidCur = BaseWordRecord.ReadInt32(stream);
	}
}
