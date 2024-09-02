using System.Collections.Generic;
using System.IO;
using DocGen.DocIO.DLS;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class ContainerCollection : List<object>
{
	private WordDocument m_doc;

	internal ContainerCollection(WordDocument doc)
	{
		m_doc = doc;
	}

	internal int Write(Stream stream)
	{
		long position = stream.Position;
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				BaseEscherRecord baseEscherRecord = (BaseEscherRecord)enumerator.Current;
				if (!(baseEscherRecord is MsofbtClientData) || baseEscherRecord.Header.Version == 15)
				{
					baseEscherRecord.WriteMsofbhWithRecord(stream);
				}
			}
		}
		return (int)(stream.Position - position);
	}

	internal void Read(Stream stream, int length)
	{
		long num = stream.Position + length;
		while (stream.Position < num && stream.Position < stream.Length)
		{
			BaseEscherRecord baseEscherRecord = _MSOFBH.ReadHeaderWithRecord(stream, m_doc);
			if (baseEscherRecord != null)
			{
				Add(baseEscherRecord);
			}
		}
	}
}
