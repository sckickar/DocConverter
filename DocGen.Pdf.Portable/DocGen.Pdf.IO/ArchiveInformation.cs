using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.IO;

internal class ArchiveInformation
{
	private long m_archiveNumber;

	private long m_index;

	private PdfStream m_archive;

	private GetArchive m_getArchive;

	public PdfStream Archive
	{
		get
		{
			if (m_archive == null)
			{
				m_archive = m_getArchive(m_archiveNumber);
			}
			return m_archive;
		}
	}

	public long Index => m_index;

	internal long ArchiveNumber => m_archiveNumber;

	public ArchiveInformation(long arcNum, long index, GetArchive getArchive)
	{
		m_archiveNumber = arcNum;
		m_index = index;
		m_getArchive = getArchive;
	}
}
