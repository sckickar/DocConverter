using System.Collections.Generic;
using System.IO;
using DocGen.CompoundFile.DocIO;
using DocGen.CompoundFile.DocIO.Net;
using DocGen.DocIO.ReaderWriter.DataStreamParser.OLEObject;

namespace DocGen.DocIO.ReaderWriter;

internal class StreamsManager
{
	public const string MacrosStorageName = "Macros";

	public const string ObjectPoolStorageName = "ObjectPool";

	private const string c_mainStream = "WordDocument";

	private const string c_dataStream = "Data";

	private const string c_tableStream = "1Table";

	private const string c_summaryInfoStream = "\u0005SummaryInformation";

	private const string c_documentSummaryInfoStream = "\u0005DocumentSummaryInformation";

	private byte[] m_compObjData = new byte[121]
	{
		1, 0, 254, 255, 3, 10, 0, 0, 255, 255,
		255, 255, 6, 9, 2, 0, 0, 0, 0, 0,
		192, 0, 0, 0, 0, 0, 0, 70, 39, 0,
		0, 0, 77, 105, 99, 114, 111, 115, 111, 102,
		116, 32, 79, 102, 102, 105, 99, 101, 32, 87,
		111, 114, 100, 32, 57, 55, 45, 50, 48, 48,
		51, 32, 68, 111, 99, 117, 109, 101, 110, 116,
		0, 10, 0, 0, 0, 77, 83, 87, 111, 114,
		100, 68, 111, 99, 0, 16, 0, 0, 0, 87,
		111, 114, 100, 46, 68, 111, 99, 117, 109, 101,
		110, 116, 46, 56, 0, 244, 57, 178, 113, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0
	};

	private string m_fileName;

	private Stream m_outStream;

	private ICompoundFile m_compoundFile;

	private MemoryStream m_mainStream;

	private MemoryStream m_tableStream;

	private MemoryStream m_dataStream;

	private MemoryStream m_macrosStream;

	private MemoryStream m_summaryInfoStream;

	private MemoryStream m_documentSummaryInfoStream;

	private MemoryStream m_objectPoolStream;

	private BinaryWriter m_mainWriter;

	private BinaryWriter m_tableWriter;

	private BinaryWriter m_dataWriter;

	private BinaryWriter m_summaryInfoWriter;

	private BinaryWriter m_documentSummaryInfoWriter;

	private BinaryReader m_mainReader;

	private BinaryReader m_tableReader;

	private BinaryReader m_dataReader;

	private BinaryReader m_summaryInfoReader;

	private BinaryReader m_documentSummaryInfoReader;

	private bool m_bNetStorage = true;

	internal ICompoundFile CompoundFile => m_compoundFile;

	internal MemoryStream MainStream => m_mainStream;

	internal MemoryStream TableStream => m_tableStream;

	internal MemoryStream DataStream => m_dataStream;

	internal MemoryStream MacrosStream
	{
		get
		{
			return m_macrosStream;
		}
		set
		{
			m_macrosStream = value;
		}
	}

	internal MemoryStream ObjectPoolStream
	{
		get
		{
			return m_objectPoolStream;
		}
		set
		{
			m_objectPoolStream = value;
		}
	}

	internal MemoryStream SummaryInfoStream
	{
		get
		{
			return m_summaryInfoStream;
		}
		set
		{
			m_summaryInfoStream = value;
		}
	}

	internal MemoryStream DocumentSummaryInfoStream
	{
		get
		{
			return m_documentSummaryInfoStream;
		}
		set
		{
			m_documentSummaryInfoStream = value;
		}
	}

	internal BinaryWriter SummaryInfoWriter
	{
		get
		{
			return m_summaryInfoWriter;
		}
		set
		{
			m_summaryInfoWriter = value;
		}
	}

	internal BinaryWriter DocumentSummaryInfoWriter
	{
		get
		{
			return m_documentSummaryInfoWriter;
		}
		set
		{
			m_documentSummaryInfoWriter = value;
		}
	}

	internal BinaryReader SummaryInfoReader
	{
		get
		{
			return m_summaryInfoReader;
		}
		set
		{
			m_summaryInfoReader = value;
		}
	}

	internal BinaryReader DocumentSummaryInfoReader
	{
		get
		{
			return m_documentSummaryInfoReader;
		}
		set
		{
			m_documentSummaryInfoReader = value;
		}
	}

	internal BinaryWriter MainWriter => m_mainWriter;

	internal BinaryWriter TableWriter => m_tableWriter;

	internal BinaryWriter DataWriter => m_dataWriter;

	internal BinaryReader MainReader => m_mainReader;

	internal BinaryReader TableReader => m_tableReader;

	internal BinaryReader DataReader => m_dataReader;

	internal StreamsManager(Stream stream, bool createNewStorage)
	{
		if (createNewStorage)
		{
			InitStreams();
			m_outStream = stream;
			if (m_bNetStorage)
			{
				m_compoundFile = new DocGen.CompoundFile.DocIO.Net.CompoundFile();
			}
		}
		else
		{
			LoadStg(stream);
		}
	}

	internal void LoadStg(Stream stream)
	{
		if (m_bNetStorage)
		{
			m_compoundFile = new DocGen.CompoundFile.DocIO.Net.CompoundFile(stream);
		}
		LoadStreams();
	}

	internal void LoadTableStream(string tableStreamName)
	{
		if (m_bNetStorage)
		{
			m_tableStream = LoadStreamFromCompound(tableStreamName);
		}
		m_tableReader = new BinaryReader(m_tableStream);
	}

	internal void LoadSummaryInfoStream()
	{
		if (m_bNetStorage && m_compoundFile.RootStorage.ContainsStream("\u0005SummaryInformation"))
		{
			m_summaryInfoStream = LoadStreamFromCompound("\u0005SummaryInformation");
		}
		if (m_summaryInfoStream != null)
		{
			m_summaryInfoReader = new BinaryReader(m_summaryInfoStream);
		}
	}

	internal void LoadDocumentSummaryInfoStream()
	{
		if (m_bNetStorage && m_compoundFile.RootStorage.ContainsStream("\u0005DocumentSummaryInformation"))
		{
			m_documentSummaryInfoStream = LoadStreamFromCompound("\u0005DocumentSummaryInformation");
		}
		if (m_documentSummaryInfoStream != null)
		{
			m_documentSummaryInfoReader = new BinaryReader(m_documentSummaryInfoStream);
		}
	}

	internal void UpdateStreams(MemoryStream mainStream, MemoryStream tableStream, MemoryStream dataStream)
	{
		m_mainStream = mainStream;
		m_tableStream = tableStream;
		m_dataStream = dataStream;
		if (m_dataStream != null)
		{
			m_dataReader = new BinaryReader(m_dataStream);
		}
	}

	internal void WriteSubStorage(MemoryStream stream, string storageName)
	{
		DocGen.CompoundFile.DocIO.Net.CompoundFile compoundFile = new DocGen.CompoundFile.DocIO.Net.CompoundFile(stream);
		ICompoundStorage storageToCopy = compoundFile.RootStorage.OpenStorage(storageName);
		m_compoundFile.RootStorage.InsertCopy(storageToCopy);
		compoundFile.Dispose();
		stream.Dispose();
	}

	internal void SaveStg(Dictionary<string, Storage> oleObjectCollection)
	{
		SaveStream("WordDocument", m_mainStream);
		SaveStream("1Table", m_tableStream);
		SaveCompObjStream();
		if (m_dataStream.Length != 0L)
		{
			SaveStream("Data", m_dataStream);
		}
		if (m_macrosStream != null)
		{
			WriteSubStorage(m_macrosStream, "Macros");
		}
		if (oleObjectCollection.Count > 0)
		{
			DocGen.CompoundFile.DocIO.Net.CompoundFile compoundFile = new DocGen.CompoundFile.DocIO.Net.CompoundFile();
			ICompoundStorage compoundStorage = compoundFile.RootStorage.CreateStorage("ObjectPool");
			int num = 2;
			foreach (string key in oleObjectCollection.Keys)
			{
				ICompoundStorage storage = compoundStorage.CreateStorage("_" + key);
				oleObjectCollection[key].UpdateGuid(compoundFile, num, key);
				num++;
				oleObjectCollection[key].WriteToStorage(storage);
			}
			compoundFile.Flush();
			MemoryStream memoryStream = new MemoryStream((compoundFile.BaseStream as MemoryStream).ToArray());
			compoundFile.Dispose();
			WriteSubStorage(memoryStream, "ObjectPool");
			memoryStream.Dispose();
		}
		if (m_summaryInfoStream != null && m_summaryInfoStream.Length != 0L)
		{
			SaveStream("\u0005SummaryInformation", m_summaryInfoStream);
		}
		if (m_documentSummaryInfoStream != null && m_documentSummaryInfoStream.Length != 0L)
		{
			SaveStream("\u0005DocumentSummaryInformation", m_documentSummaryInfoStream);
		}
		if (m_outStream != null && m_bNetStorage)
		{
			m_compoundFile.Save(m_outStream);
			m_outStream.Flush();
		}
		CloseStg();
	}

	internal void CloseStg()
	{
		if (m_outStream != null)
		{
			m_outStream = null;
		}
		if (m_compoundFile != null)
		{
			m_compoundFile.Dispose();
			m_compoundFile = null;
		}
		if (m_mainStream != null)
		{
			m_mainStream.Close();
			m_mainStream = null;
		}
		if (m_tableStream != null)
		{
			m_tableStream.Close();
			m_tableStream = null;
		}
		if (m_dataStream != null)
		{
			m_dataStream.Close();
			m_dataStream = null;
		}
		if (m_macrosStream != null)
		{
			m_macrosStream.Close();
			m_macrosStream = null;
		}
		if (m_objectPoolStream != null)
		{
			m_objectPoolStream.Close();
			m_objectPoolStream = null;
		}
		if (m_summaryInfoStream != null)
		{
			m_summaryInfoStream.Close();
			m_summaryInfoStream = null;
		}
		if (m_documentSummaryInfoStream != null)
		{
			m_documentSummaryInfoStream.Close();
			m_documentSummaryInfoStream = null;
		}
		if (m_mainWriter != null)
		{
			m_mainWriter.Dispose();
		}
		if (m_tableWriter != null)
		{
			m_tableWriter.Dispose();
		}
		if (m_dataWriter != null)
		{
			m_dataWriter.Dispose();
		}
		if (m_summaryInfoWriter != null)
		{
			m_summaryInfoWriter.Dispose();
		}
		if (m_documentSummaryInfoWriter != null)
		{
			m_documentSummaryInfoWriter.Dispose();
		}
		if (m_mainReader != null)
		{
			m_mainReader.Dispose();
		}
		if (m_tableReader != null)
		{
			m_tableReader.Dispose();
		}
		if (m_dataReader != null)
		{
			m_dataReader.Dispose();
		}
		if (m_summaryInfoReader != null)
		{
			m_summaryInfoReader.Dispose();
		}
		if (m_documentSummaryInfoReader != null)
		{
			m_documentSummaryInfoReader.Dispose();
		}
	}

	private void SaveStream(string name, MemoryStream stream)
	{
		if (m_bNetStorage)
		{
			using (CompoundStream compoundStream = m_compoundFile.RootStorage.CreateStream(name))
			{
				byte[] array = stream.ToArray();
				compoundStream.Write(array, 0, array.Length);
			}
		}
	}

	private void SaveCompObjStream()
	{
		if (m_bNetStorage)
		{
			using (CompoundStream compoundStream = m_compoundFile.RootStorage.CreateStream("\u0001CompObj"))
			{
				compoundStream.Write(m_compObjData, 0, m_compObjData.Length);
			}
		}
	}

	private void InitStreams()
	{
		m_mainStream = new MemoryStream(4095);
		m_mainWriter = new BinaryWriter(m_mainStream);
		m_tableStream = new MemoryStream(4095);
		m_tableWriter = new BinaryWriter(m_mainStream);
		m_dataStream = new MemoryStream();
		m_dataWriter = new BinaryWriter(m_dataStream);
		m_documentSummaryInfoStream = new MemoryStream();
		m_documentSummaryInfoWriter = new BinaryWriter(m_documentSummaryInfoStream);
		m_summaryInfoStream = new MemoryStream();
		m_summaryInfoWriter = new BinaryWriter(m_summaryInfoStream);
	}

	private void LoadStreams()
	{
		if (m_bNetStorage)
		{
			m_mainStream = LoadStreamFromCompound("WordDocument");
			if (m_compoundFile.RootStorage.ContainsStream("Data"))
			{
				m_dataStream = LoadStreamFromCompound("Data");
			}
			m_macrosStream = ReadSubStorage("Macros");
			m_objectPoolStream = ReadSubStorage("ObjectPool");
		}
		m_mainReader = new BinaryReader(m_mainStream);
		if (m_dataStream != null)
		{
			m_dataReader = new BinaryReader(m_dataStream);
		}
	}

	private MemoryStream LoadStreamFromCompound(string name)
	{
		byte[] buffer = null;
		using (CompoundStream compoundStream = m_compoundFile.RootStorage.OpenStream(name))
		{
			int num = (int)compoundStream.Length;
			buffer = new byte[num];
			compoundStream.Read(buffer, 0, num);
		}
		return new MemoryStream(buffer);
	}

	private MemoryStream LoadSubStorage(string name)
	{
		return null;
	}

	private MemoryStream ReadSubStorage(string stgName)
	{
		if (m_compoundFile.RootStorage.ContainsStorage(stgName))
		{
			ICompoundStorage storageToCopy = m_compoundFile.RootStorage.OpenStorage(stgName);
			DocGen.CompoundFile.DocIO.Net.CompoundFile compoundFile = new DocGen.CompoundFile.DocIO.Net.CompoundFile();
			compoundFile.RootStorage.InsertCopy(storageToCopy);
			compoundFile.Flush();
			MemoryStream memoryStream = new MemoryStream();
			compoundFile.BaseStream.CopyTo(memoryStream);
			memoryStream.Position = 0L;
			compoundFile.Dispose();
			return memoryStream;
		}
		return null;
	}
}
