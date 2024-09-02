using System;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter;
using DocGen.DocIO.ReaderWriter.Escher;

[CLSCompliant(false)]
internal class DocInfo
{
	private Fib m_fib;

	private WPTablesData m_tablesData;

	private WordFKPData m_fkpData;

	private WordImageWriter m_imageWriter;

	internal Fib Fib => m_fib;

	internal WPTablesData TablesData => m_tablesData;

	internal WordFKPData FkpData => m_fkpData;

	internal WordImageWriter ImageWriter => m_imageWriter;

	internal DocInfo(StreamsManager streamsManager)
	{
		m_fib = new Fib();
		m_tablesData = new WPTablesData(m_fib);
		m_fkpData = new WordFKPData(m_fib, m_tablesData);
		m_imageWriter = new WordImageWriter(streamsManager.DataStream);
	}

	internal WordImageReader GetImageReader(StreamsManager streamsManager, int offset, WordDocument doc)
	{
		return new WordImageReader(streamsManager.DataStream, offset, doc);
	}

	internal void Close()
	{
		if (m_fib != null)
		{
			m_fib.Encoding = null;
			m_fib = null;
		}
		if (m_tablesData != null)
		{
			m_tablesData.Close();
			m_tablesData = null;
		}
		if (m_fkpData != null)
		{
			m_fkpData.Close();
			m_fkpData = null;
		}
		if (m_imageWriter != null)
		{
			m_imageWriter.Close();
			m_imageWriter = null;
		}
	}
}
