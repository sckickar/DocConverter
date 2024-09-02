using System.IO;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal abstract class FOPTEBase : BaseWordRecord
{
	private int m_id;

	private bool m_isBid;

	internal int Id
	{
		get
		{
			return m_id;
		}
		set
		{
			m_id = value;
		}
	}

	internal bool IsBid => m_isBid;

	internal FOPTEBase(int id, bool isBid)
	{
		m_id = id;
		m_isBid = isBid;
	}

	internal abstract void Write(Stream stream);

	internal abstract FOPTEBase Clone();
}
