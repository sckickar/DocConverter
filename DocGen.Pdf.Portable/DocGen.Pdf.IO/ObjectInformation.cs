using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.IO;

internal class ObjectInformation
{
	private ObjectType m_type;

	private ArchiveInformation m_archive;

	private PdfParser m_parser;

	private long m_offset;

	private CrossTable m_crossTable;

	public IPdfPrimitive Obj;

	public ObjectType Type => m_type;

	public PdfParser Parser
	{
		get
		{
			if (m_parser == null)
			{
				m_parser = m_crossTable.RetrieveParser(m_archive);
			}
			return m_parser;
		}
	}

	public long Offset
	{
		get
		{
			if (m_offset == 0L)
			{
				PdfParser parser = Parser;
				if (parser != null)
				{
					parser.StartFrom(0L);
					long num = 0L;
					if (Archive != null)
					{
						if (Archive.Archive["N"] is PdfNumber pdfNumber)
						{
							num = pdfNumber.LongValue;
						}
						long[] array = new long[num * 2];
						if (m_crossTable.archiveIndices.ContainsKey(Archive.Archive))
						{
							array = m_crossTable.archiveIndices[Archive.Archive];
						}
						else
						{
							for (int i = 0; i < num; i++)
							{
								if (parser.Simple() is PdfNumber pdfNumber2)
								{
									array[i * 2] = pdfNumber2.LongValue;
								}
								if (parser.Simple() is PdfNumber pdfNumber3)
								{
									array[i * 2 + 1] = pdfNumber3.LongValue;
								}
							}
							m_crossTable.archiveIndices[Archive.Archive] = array;
						}
						long index = Archive.Index;
						if (index * 2 >= array.Length)
						{
							throw new PdfDocumentException("Missing indexes in archive #" + Archive.ArchiveNumber);
						}
						m_offset = array[index * 2 + 1];
						long num2 = (Archive.Archive["First"] as PdfNumber).IntValue;
						m_offset += num2;
					}
				}
			}
			return m_offset;
		}
	}

	public ArchiveInformation Archive => m_archive;

	public static implicit operator long(ObjectInformation oi)
	{
		return oi.Offset;
	}

	public ObjectInformation(ObjectType type, long offset, ArchiveInformation arciveInfo, CrossTable crossTable)
	{
		m_type = type;
		m_offset = offset;
		m_archive = arciveInfo;
		m_crossTable = crossTable;
	}
}
