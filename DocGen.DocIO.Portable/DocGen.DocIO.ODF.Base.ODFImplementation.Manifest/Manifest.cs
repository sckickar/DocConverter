using System.Collections.Generic;

namespace DocGen.DocIO.ODF.Base.ODFImplementation.Manifest;

internal class Manifest
{
	private List<FileEntry> m_files;

	internal List<FileEntry> Files
	{
		get
		{
			if (m_files == null)
			{
				m_files = new List<FileEntry>();
			}
			return m_files;
		}
		set
		{
			m_files = value;
		}
	}
}
