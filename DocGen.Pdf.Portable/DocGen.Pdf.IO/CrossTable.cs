using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.Pdf.Primitives;
using DocGen.Pdf.Security;

namespace DocGen.Pdf.IO;

internal class CrossTable
{
	private struct SubSection
	{
		public int StartNumber;

		public int Count;

		public SubSection(int start, int count)
		{
			StartNumber = start;
			Count = count;
		}

		public SubSection(int count)
		{
			StartNumber = 0;
			Count = count;
		}
	}

	private Stream m_stream;

	private PdfReader m_reader;

	private PdfParser m_parser;

	internal Dictionary<long, ObjectInformation> m_objects;

	private PdfDictionary m_trailer;

	private PdfReferenceHolder m_documentCatalog;

	private long m_startXRef;

	private Dictionary<PdfStream, PdfParser> m_readersTable = new Dictionary<PdfStream, PdfParser>();

	private Dictionary<long, PdfStream> m_archives = new Dictionary<long, PdfStream>();

	private PdfEncryptor m_encryptor;

	private PdfCrossTable m_crossTable;

	internal long m_initialNumberOfSubsection;

	internal long m_initialSubsectionCount;

	internal long m_totalNumberOfSubsection;

	private bool m_isStructureAltered;

	private const int m_generationNumber = 65535;

	internal bool m_isOpenAndRepair;

	private long m_whiteSpace;

	internal bool validateSyntax;

	private Dictionary<long, List<ObjectInformation>> m_allTables = new Dictionary<long, List<ObjectInformation>>();

	internal Dictionary<PdfStream, long[]> archiveIndices = new Dictionary<PdfStream, long[]>();

	private bool m_repair;

	internal long m_trailerPosition = -1L;

	internal bool m_invalidXrefStart;

	internal bool m_closeCompletely;

	private List<long> m_prevOffset;

	private List<long> m_eofOffset;

	internal ObjectInformation this[long index]
	{
		get
		{
			object obj = (m_objects.ContainsKey(index) ? m_objects[index] : null);
			if (obj == null)
			{
				return null;
			}
			return obj as ObjectInformation;
		}
	}

	internal Dictionary<long, List<ObjectInformation>> AllTables => m_allTables;

	public long Count => m_objects.Count;

	public PdfReferenceHolder DocumentCatalog
	{
		get
		{
			if (m_documentCatalog == null)
			{
				IPdfPrimitive pdfPrimitive = Trailer["Root"];
				if (!(pdfPrimitive is PdfReferenceHolder))
				{
					throw new PdfDocumentException("Invalid/Unknown/Unsupported format");
				}
				m_documentCatalog = pdfPrimitive as PdfReferenceHolder;
			}
			return m_documentCatalog;
		}
	}

	internal Stream Stream => m_stream;

	internal long XRefOffset => m_startXRef;

	public PdfReader Reader
	{
		get
		{
			if (m_reader == null)
			{
				m_reader = new PdfReader(m_stream);
			}
			return m_reader;
		}
	}

	public PdfParser Parser
	{
		get
		{
			if (m_parser == null)
			{
				m_parser = new PdfParser(this, Reader, m_crossTable);
			}
			return m_parser;
		}
	}

	internal PdfDictionary Trailer => m_trailer;

	internal bool IsStructureAltered => m_isStructureAltered;

	internal PdfEncryptor Encryptor
	{
		get
		{
			return m_encryptor;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("m_encryptor");
			}
			m_encryptor = value;
		}
	}

	internal List<long> PrevOffset
	{
		get
		{
			if (m_prevOffset == null)
			{
				m_prevOffset = new List<long>();
			}
			return m_prevOffset;
		}
	}

	internal List<long> EofOffset
	{
		get
		{
			return m_eofOffset;
		}
		set
		{
			m_eofOffset = value;
		}
	}

	public CrossTable(Stream docStream, PdfCrossTable crossTable)
	{
		if (docStream == null)
		{
			throw new ArgumentNullException("docStream");
		}
		if (!docStream.CanSeek || !docStream.CanRead)
		{
			throw new PdfDocumentException("Ivalid stream.");
		}
		if (crossTable == null)
		{
			throw new ArgumentNullException("crossTable");
		}
		if (crossTable.loadedPdfDocument != null)
		{
			validateSyntax = true;
		}
		m_isOpenAndRepair = crossTable.isOpenAndRepair;
		m_repair = crossTable.isRepair;
		m_stream = docStream;
		int num = CheckJunk();
		if (num < 0)
		{
			throw new PdfException("Could not find valid signature (%PDF-).");
		}
		m_crossTable = crossTable;
		m_objects = new Dictionary<long, ObjectInformation>();
		PdfReader pdfReader = Reader;
		PdfParser pdfParser = Parser;
		pdfReader.Position = num;
		pdfReader.SkipWS();
		m_whiteSpace = pdfReader.Position;
		long num2 = pdfReader.Seek(0L, SeekOrigin.End);
		CheckStartxref();
		long num3 = pdfReader.SearchBack("%%EOF");
		if (num3 != -1 && !m_isOpenAndRepair)
		{
			if (num2 != num3 + 5)
			{
				pdfReader.Position = num3 + 5;
				string nextToken = pdfReader.GetNextToken();
				if (nextToken != string.Empty && nextToken[0] != 0 && nextToken[0] != '0')
				{
					MemoryStream memoryStream = new MemoryStream();
					m_stream.Position = 0L;
					byte[] array = new byte[num3 + 5];
					m_stream.Read(array, 0, array.Length);
					memoryStream.Write(array, 0, array.Length);
					pdfReader = new PdfReader(memoryStream);
					pdfParser = new PdfParser(this, pdfReader, m_crossTable);
					m_reader = pdfReader;
					m_parser = pdfParser;
				}
			}
		}
		else
		{
			if (validateSyntax)
			{
				PdfException item = new PdfException("The document does not contain EOF.");
				crossTable.loadedPdfDocument.pdfException.Add(item);
			}
			if (num3 == -1)
			{
				pdfReader.Position = num2;
			}
		}
		long position = pdfReader.Position;
		num2 = pdfReader.SearchBack("startxref");
		long position2 = num2;
		bool flag = false;
		if (num2 < 0)
		{
			pdfReader.Position = position;
			string text = "startxref";
			for (int i = 0; i < text.Length; i++)
			{
				string token = text.Remove(i, 1);
				num2 = pdfReader.SearchBack(token);
				if (num2 > 0)
				{
					break;
				}
				pdfReader.Position = position;
			}
			pdfReader.Position += "startxref".Length;
			num2 = Convert.ToInt64(pdfReader.ReadLine());
			if (!crossTable.isOpenAndRepair)
			{
				throw new Exception("Document has corrupted cross reference table");
			}
			pdfParser.RebuildXrefTable(m_objects, this);
			m_isStructureAltered = true;
			long num4 = pdfReader.SearchBack("xref");
			if (num4 != -1)
			{
				num2 = num4;
			}
			pdfParser.SetOffset(num2);
		}
		else
		{
			pdfParser.SetOffset(num2);
			num2 = pdfParser.StartXRef();
			if (pdfParser.ForceRebuild)
			{
				crossTable.isRepair = false;
				crossTable.isOpenAndRepair = true;
				m_isOpenAndRepair = true;
				m_invalidXrefStart = true;
				pdfParser.RebuildXrefTable(m_objects, this);
				m_isStructureAltered = true;
				if (m_trailerPosition != -1)
				{
					num2 = m_trailerPosition;
				}
			}
			else
			{
				m_startXRef = num2;
				pdfParser.SetOffset(num2);
				if (m_whiteSpace != 0L)
				{
					long num5 = 0L;
					num5 = pdfReader.SearchForward("xref");
					if (num5 == -1)
					{
						flag = false;
						num2 += m_whiteSpace;
						m_stream.Position = num2;
					}
					else
					{
						num2 = num5;
						pdfParser.SetOffset(num2);
						flag = true;
					}
				}
			}
		}
		string text2 = string.Empty;
		if (!pdfParser.ForceRebuild)
		{
			text2 = pdfReader.ReadLine();
		}
		if (!pdfParser.ForceRebuild && !text2.Contains("xref") && !text2.Contains("obj") && !flag)
		{
			long position3 = pdfReader.Position;
			string text3 = pdfReader.ReadLine();
			if (text3.Contains("xref"))
			{
				text2 = text3;
				num2 = position3;
			}
			else if (crossTable.isOpenAndRepair)
			{
				pdfReader.Position = position2;
			}
			else
			{
				pdfReader.Position = position3;
			}
		}
		if (!pdfParser.ForceRebuild && crossTable.isRepair && !text2.Contains("xref") && !text2.Contains("obj") && !flag)
		{
			crossTable.isRepair = false;
			crossTable.isOpenAndRepair = true;
			m_isOpenAndRepair = true;
			m_invalidXrefStart = true;
		}
		if (!pdfParser.ForceRebuild && !text2.Contains("xref") && !text2.Contains("obj") && !flag && !crossTable.isRepair)
		{
			if (m_invalidXrefStart)
			{
				pdfParser.RebuildXrefTable(m_objects, this);
				m_isStructureAltered = true;
			}
			else
			{
				if (num2 > m_stream.Length)
				{
					num2 = m_stream.Length;
					pdfReader.Position = num2;
					num2 = pdfReader.SearchBack("startxref");
				}
				long num6 = pdfReader.SearchBack("xref");
				if (num6 != -1)
				{
					num2 = num6;
				}
				if (crossTable.isOpenAndRepair)
				{
					if (num6 == -1)
					{
						num6 = pdfReader.SearchForward("xref");
						if (num6 != -1)
						{
							num2 = num6;
						}
					}
					pdfParser.RebuildXrefTable(m_objects, this);
					m_isStructureAltered = true;
				}
			}
			if (m_trailerPosition != -1)
			{
				num2 = m_trailerPosition;
			}
			pdfParser.SetOffset(num2);
		}
		else if (!pdfParser.ForceRebuild && m_isOpenAndRepair)
		{
			if (text2 != "xref")
			{
				long num7 = pdfReader.SearchBack("xref");
				if (num7 != -1)
				{
					num2 = num7;
				}
				if (num7 == -1)
				{
					num7 = pdfReader.SearchForward("xref");
					if (num7 != -1)
					{
						num2 = num7;
					}
				}
			}
			pdfParser.RebuildXrefTable(m_objects, this);
			m_isStructureAltered = true;
			pdfParser.SetOffset(num2);
			flag = false;
		}
		pdfReader.Position = num2;
		try
		{
			m_trailer = pdfParser.ParseXRefTable(m_objects, this) as PdfDictionary;
		}
		catch (Exception)
		{
			throw new Exception("Invalid cross reference table.");
		}
		PdfDictionary pdfDictionary = m_trailer;
		long num8 = -1L;
		List<long> list = new List<long>();
		while (pdfDictionary.ContainsKey("Prev"))
		{
			if (m_whiteSpace != 0L || m_isOpenAndRepair)
			{
				(pdfDictionary["Prev"] as PdfNumber).IntValue += (int)m_whiteSpace;
				m_isStructureAltered = true;
			}
			num2 = (pdfDictionary["Prev"] as PdfNumber).IntValue;
			PrevOffset.Add(num2);
			PdfReader pdfReader2 = new PdfReader(m_reader.Stream)
			{
				Position = num2
			};
			num8 = num2;
			if (!pdfReader2.GetNextToken().Equals("xref"))
			{
				string nextToken2 = pdfReader2.GetNextToken();
				int result = 0;
				if (int.TryParse(nextToken2, out result) && result >= 0 && pdfReader2.GetNextToken().Equals("obj"))
				{
					pdfParser.SetOffset(num2);
					pdfDictionary = pdfParser.ParseXRefTable(m_objects, this) as PdfDictionary;
					continue;
				}
				pdfParser.RebuildXrefTable(m_objects, this);
				m_isStructureAltered = true;
				break;
			}
			pdfParser.SetOffset(num2);
			pdfDictionary = pdfParser.ParseXRefTable(m_objects, this) as PdfDictionary;
			if (pdfDictionary.ContainsKey("Size") && m_trailer.ContainsKey("Size") && (pdfDictionary["Size"] as PdfNumber).IntValue > (m_trailer["Size"] as PdfNumber).IntValue)
			{
				(m_trailer["Size"] as PdfNumber).IntValue = (pdfDictionary["Size"] as PdfNumber).IntValue;
			}
			if (m_isOpenAndRepair && pdfDictionary != null && pdfDictionary.ContainsKey("Prev"))
			{
				if ((pdfDictionary["Prev"] as PdfNumber).IntValue == num8)
				{
					break;
				}
			}
			else if (pdfDictionary != null && pdfDictionary.ContainsKey("Prev") && (pdfDictionary["Prev"] as PdfNumber).IntValue == num8)
			{
				throw new Exception("Trailer Prev offset is located in the same crosstable section");
			}
			if (list.Contains(num2))
			{
				throw new Exception("Invalid cross reference table.");
			}
			list.Add(num2);
		}
		list.Clear();
		if (m_whiteSpace != 0 && flag)
		{
			long[] array2 = new long[m_objects.Count];
			m_objects.Keys.CopyTo(array2, 0);
			foreach (long key in array2)
			{
				ObjectInformation objectInformation = m_objects[key];
				m_objects[key] = new ObjectInformation(ObjectType.Normal, objectInformation.Offset + m_whiteSpace, null, this);
			}
			m_isStructureAltered = true;
		}
		else if (m_whiteSpace != 0L && m_whiteSpace > 0 && !m_isOpenAndRepair && !m_isStructureAltered && !pdfDictionary.ContainsKey("Prev"))
		{
			m_isStructureAltered = true;
		}
		if (m_isOpenAndRepair && m_trailer.ContainsKey("Prev") && m_isStructureAltered)
		{
			m_trailer.Remove("Prev");
		}
		if (crossTable.isRepair)
		{
			ReadAllObjects(m_objects, pdfParser);
		}
	}

	private void ReadAllObjects(Dictionary<long, ObjectInformation> objects, PdfParser parser)
	{
		foreach (KeyValuePair<long, ObjectInformation> @object in objects)
		{
			ObjectInformation value = @object.Value;
			if (value != null && value.Type == ObjectType.Normal)
			{
				parser.SeekOffset(value.Offset);
			}
		}
	}

	internal CrossTable(Stream docStream, PdfCrossTable crossTable, bool isFdf)
	{
		m_stream = docStream;
		m_crossTable = crossTable;
		m_objects = new Dictionary<long, ObjectInformation>();
	}

	private void CheckStartxref()
	{
		long num = 1024L;
		long num2 = m_stream.Length - num;
		if (num2 < 1)
		{
			num2 = 1L;
		}
		byte[] array = new byte[num];
		while (num2 > 0)
		{
			m_stream.Seek(num2, SeekOrigin.Begin);
			m_stream.Read(array, 0, (int)num);
			if (Encoding.UTF8.GetString(array, 0, array.Length).LastIndexOf("startxref") >= 0)
			{
				break;
			}
			num2 = num2 - num + 9;
		}
		if (num2 < 0 && !m_crossTable.isOpenAndRepair)
		{
			throw new Exception("Cannot load document, the document structure has been corrupted.");
		}
	}

	public IPdfPrimitive GetObject(IPdfPrimitive pointer)
	{
		if (pointer == null)
		{
			throw new ArgumentNullException("pointer");
		}
		if (pointer is PdfReference)
		{
			PdfReference pdfReference = pointer as PdfReference;
			ObjectInformation objectInformation = this[pdfReference.ObjNum];
			if (objectInformation == null)
			{
				return new PdfNull();
			}
			if (m_crossTable.Encrypted)
			{
				objectInformation.Parser.Encrypted = true;
			}
			PdfParser parser = objectInformation.Parser;
			long offset = objectInformation.Offset;
			PdfReference pdfReference2 = pointer as PdfReference;
			bool flag = false;
			if (pdfReference2 != null)
			{
				flag = pdfReference2.IsDisposed;
			}
			IPdfPrimitive pdfPrimitive;
			if (objectInformation.Obj != null && !flag)
			{
				pdfPrimitive = objectInformation.Obj;
			}
			else if (objectInformation.Archive == null || flag)
			{
				pdfPrimitive = parser.Parse(offset);
			}
			else
			{
				pdfPrimitive = GetObject(parser, offset);
				if (Encryptor != null)
				{
					if (pdfPrimitive is PdfDictionary)
					{
						PdfDictionary obj = pdfPrimitive as PdfDictionary;
						obj.IsDecrypted = true;
						foreach (IPdfPrimitive value in obj.Items.Values)
						{
							if (value is PdfString)
							{
								(value as PdfString).IsParentDecrypted = true;
							}
						}
					}
					if (pdfPrimitive is PdfArray)
					{
						foreach (object item in pdfPrimitive as PdfArray)
						{
							if (item is PdfString)
							{
								if (objectInformation.Type == ObjectType.Packed)
								{
									(item as PdfString).IsPacked = true;
								}
							}
							else
							{
								if (!(item is PdfArray))
								{
									continue;
								}
								foreach (object item2 in item as PdfArray)
								{
									if (item2 is PdfString && objectInformation.Type == ObjectType.Packed)
									{
										(item2 as PdfString).IsPacked = true;
									}
								}
							}
						}
					}
					if (pdfPrimitive is IPdfDecryptable pdfDecryptable)
					{
						pdfDecryptable.Decrypt(Encryptor, pdfReference.ObjNum);
					}
				}
			}
			objectInformation.Obj = pdfPrimitive;
			return pdfPrimitive;
		}
		return pointer;
	}

	public byte[] GetStream(IPdfPrimitive streamRef)
	{
		if (streamRef == null)
		{
			throw new ArgumentNullException("streamRef");
		}
		if (GetObject(streamRef) is PdfStream pdfStream)
		{
			return pdfStream.Data;
		}
		return null;
	}

	internal void ParseNewTable(PdfStream stream, Dictionary<long, ObjectInformation> hashTable)
	{
		if (stream == null)
		{
			throw new PdfDocumentException("Invalid/Unknown/Unsupported format");
		}
		stream.Decompress();
		List<SubSection> sections = GetSections(stream);
		int startIndex = 0;
		foreach (SubSection item in sections)
		{
			startIndex = ParseSubsection(stream, item, hashTable, startIndex);
		}
	}

	internal void ParseSubsection(PdfParser parser, Dictionary<long, ObjectInformation> table)
	{
		PdfNumber pdfNumber = parser.Simple() as PdfNumber;
		m_initialNumberOfSubsection = pdfNumber.IntValue;
		pdfNumber = parser.Simple() as PdfNumber;
		m_totalNumberOfSubsection = pdfNumber.IntValue;
		m_initialSubsectionCount = m_initialNumberOfSubsection;
		for (int i = 0; i < m_totalNumberOfSubsection; i++)
		{
			pdfNumber = parser.Simple() as PdfNumber;
			long longValue = pdfNumber.LongValue;
			pdfNumber = parser.Simple() as PdfNumber;
			int intValue = pdfNumber.IntValue;
			if (parser.GetObjectFlag() == 'n')
			{
				ObjectInformation objectInformation = new ObjectInformation(ObjectType.Normal, longValue, null, this);
				long num = 0L;
				num = ((m_initialSubsectionCount != m_initialNumberOfSubsection) ? (m_initialSubsectionCount + i) : (m_initialNumberOfSubsection + i));
				if (!table.ContainsKey(num))
				{
					table[num] = objectInformation;
				}
				AddTables(num, objectInformation);
			}
			else if (m_initialNumberOfSubsection != 0L && longValue == 0L && intValue == 65535)
			{
				m_initialNumberOfSubsection--;
				if (i == 0 && m_initialNumberOfSubsection == 0L)
				{
					m_initialSubsectionCount = m_initialNumberOfSubsection;
				}
			}
		}
	}

	private void AddTables(long objectOffset, ObjectInformation oi)
	{
		if (m_allTables.ContainsKey(objectOffset))
		{
			m_allTables[objectOffset].Add(oi);
			return;
		}
		List<ObjectInformation> list = new List<ObjectInformation>();
		list.Add(oi);
		m_allTables.Add(objectOffset, list);
	}

	internal PdfParser RetrieveParser(ArchiveInformation archive)
	{
		if (archive == null)
		{
			return m_parser;
		}
		PdfStream archive2 = archive.Archive;
		PdfParser pdfParser = null;
		if (m_readersTable.Count > 0 && m_readersTable.ContainsKey(archive2))
		{
			pdfParser = m_readersTable[archive2];
		}
		if (pdfParser == null && archive2 != null)
		{
			PdfReader reader = new PdfReader(new MemoryStream(archive2.Data, writable: false));
			pdfParser = new PdfParser(this, reader, m_crossTable);
			m_readersTable[archive2] = pdfParser;
		}
		return pdfParser;
	}

	private PdfStream RetrieveArchive(long archiveNumber)
	{
		PdfStream pdfStream = null;
		if (m_archives.ContainsKey(archiveNumber))
		{
			pdfStream = m_archives[archiveNumber];
		}
		if (pdfStream == null)
		{
			ObjectInformation objectInformation = this[archiveNumber];
			pdfStream = objectInformation.Parser.Parse(objectInformation.Offset) as PdfStream;
			if (Encryptor != null && !Encryptor.EncryptOnlyAttachment)
			{
				pdfStream.Decrypt(Encryptor, archiveNumber);
			}
			if (pdfStream != null)
			{
				pdfStream.Decompress();
				m_archives[archiveNumber] = pdfStream;
			}
		}
		return pdfStream;
	}

	private List<SubSection> GetSections(PdfStream stream)
	{
		List<SubSection> list = new List<SubSection>();
		int num = 0;
		num = (stream["Size"] as PdfNumber).IntValue;
		if (num == 0)
		{
			throw new PdfDocumentException("Invalid/Unknown/Unsupported format");
		}
		IPdfPrimitive pdfPrimitive = stream["Index"];
		if (pdfPrimitive == null)
		{
			list.Add(new SubSection(num));
		}
		else
		{
			if (!(GetObject(pdfPrimitive) is PdfArray pdfArray))
			{
				throw new PdfDocumentException("Invalid/Unknown/Unsupported format");
			}
			if (((uint)pdfArray.Count & (true ? 1u : 0u)) != 0)
			{
				throw new PdfDocumentException("Invalid/Unknown/Unsupported format");
			}
			int num2;
			for (num2 = 0; num2 < pdfArray.Count; num2++)
			{
				int num3 = 0;
				int num4 = 0;
				num3 = (pdfArray[num2] as PdfNumber).IntValue;
				num2++;
				num4 = (pdfArray[num2] as PdfNumber).IntValue;
				list.Add(new SubSection(num3, num4));
			}
		}
		return list;
	}

	private int ParseSubsection(PdfStream stream, SubSection subsection, Dictionary<long, ObjectInformation> table, int startIndex)
	{
		int result = startIndex;
		PdfArray pdfArray = GetObject(stream["W"]) as PdfArray;
		long num = pdfArray.Count;
		long[] array = new long[num];
		for (int i = 0; i < num; i++)
		{
			if (pdfArray[i] is PdfNumber pdfNumber)
			{
				array[i] = pdfNumber.LongValue;
			}
		}
		long[] array2 = new long[num];
		byte[] data = stream.Data;
		int j = 0;
		for (int count = subsection.Count; j < count; j++)
		{
			for (long num2 = 0L; num2 < num; num2++)
			{
				long num3 = 0L;
				if (num2 == 0L)
				{
					num3 = ((array[num2] <= 0) ? 1 : 0);
				}
				for (int k = 0; k < array[num2]; k++)
				{
					num3 <<= 8;
					num3 += data[result++];
				}
				array2[num2] = num3;
			}
			long offset = 0L;
			ArchiveInformation arciveInfo = null;
			if (array2[0] == 1)
			{
				offset = ((m_whiteSpace == 0L && !m_isOpenAndRepair) ? array2[1] : (array2[1] + m_whiteSpace));
			}
			else if (array2[0] == 2)
			{
				arciveInfo = new ArchiveInformation(array2[1], array2[2], RetrieveArchive);
			}
			ObjectInformation objectInformation = null;
			if (array2[0] != 0L)
			{
				objectInformation = new ObjectInformation((ObjectType)array2[0], offset, arciveInfo, this);
			}
			if (objectInformation != null)
			{
				long num4 = subsection.StartNumber + j;
				if (!table.ContainsKey(num4))
				{
					table[num4] = objectInformation;
				}
				AddTables(num4, objectInformation);
			}
		}
		return result;
	}

	private IPdfPrimitive GetObject(PdfParser parser, long position)
	{
		IPdfPrimitive result = null;
		if (parser != null)
		{
			parser.StartFrom(position);
			result = parser.Simple();
		}
		return result;
	}

	private int CheckJunk()
	{
		long num = 1024L;
		long num2 = 0L;
		int num3 = 0;
		byte[] array = null;
		do
		{
			array = new byte[num];
			m_stream.Position = num2;
			int num4 = (int)num;
			if (m_stream.Length - num2 < num4)
			{
				num4 = (int)(m_stream.Length - num2);
			}
			m_stream.Read(array, 0, num4);
			num3 = PdfString.ByteToString(array).IndexOf("%PDF-");
			if (num3 != -1 && (m_isOpenAndRepair || m_repair))
			{
				num3 = (int)num2 + num3;
			}
			num2 = m_stream.Position;
		}
		while (num3 < 0 && Stream.Position != Stream.Length);
		m_stream.Position = 0L;
		return num3;
	}

	internal void Dispose()
	{
		if (m_archives != null)
		{
			foreach (KeyValuePair<long, PdfStream> archive in m_archives)
			{
				archive.Value.Dispose();
			}
		}
		if (m_allTables != null)
		{
			m_allTables.Clear();
			m_allTables = null;
		}
		if (archiveIndices != null)
		{
			archiveIndices.Clear();
			archiveIndices = null;
		}
		m_archives.Clear();
		m_documentCatalog = null;
		if (m_closeCompletely)
		{
			m_stream.Dispose();
		}
		if (m_trailer != null)
		{
			if (m_trailer is PdfStream)
			{
				(m_trailer as PdfStream).Dispose();
			}
			else
			{
				m_trailer.Clear();
			}
		}
		m_parser = null;
		m_reader = null;
		if (m_objects != null)
		{
			m_objects.Clear();
			m_objects = null;
		}
		if (m_prevOffset != null)
		{
			m_prevOffset.Clear();
			m_prevOffset = null;
		}
		if (m_eofOffset != null)
		{
			m_eofOffset.Clear();
			m_eofOffset = null;
		}
	}
}
