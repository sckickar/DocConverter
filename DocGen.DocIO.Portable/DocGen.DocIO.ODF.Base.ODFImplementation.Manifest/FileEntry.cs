namespace DocGen.DocIO.ODF.Base.ODFImplementation.Manifest;

internal class FileEntry
{
	private string m_path;

	private string m_mediaType;

	internal string Path
	{
		get
		{
			return m_path;
		}
		set
		{
			m_path = value;
		}
	}

	internal string MediaType
	{
		get
		{
			return m_mediaType;
		}
		set
		{
			m_mediaType = value;
		}
	}
}
