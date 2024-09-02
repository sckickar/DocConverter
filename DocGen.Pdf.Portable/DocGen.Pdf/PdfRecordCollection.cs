using System.Collections;
using System.Collections.Generic;

namespace DocGen.Pdf;

internal class PdfRecordCollection : IEnumerable
{
	private List<PdfRecord> m_recordCollection;

	internal List<PdfRecord> RecordCollection
	{
		get
		{
			return m_recordCollection;
		}
		set
		{
			m_recordCollection = value;
		}
	}

	internal PdfRecordCollection()
	{
		m_recordCollection = new List<PdfRecord>();
	}

	public void Add(PdfRecord record)
	{
		m_recordCollection.Add(record);
	}

	internal void Remove(PdfRecord record)
	{
		m_recordCollection.Remove(record);
	}

	public IEnumerator GetEnumerator()
	{
		return m_recordCollection.GetEnumerator();
	}
}
