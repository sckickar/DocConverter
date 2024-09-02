using System;
using System.IO;

namespace DocGen.Office.Markdown;

public class MdImageNodeVisitedEventArgs : EventArgs
{
	private Stream m_imageStream;

	private string m_uri = string.Empty;

	public Stream ImageStream
	{
		get
		{
			return m_imageStream;
		}
		set
		{
			m_imageStream = value;
		}
	}

	public string Uri
	{
		get
		{
			return m_uri;
		}
		internal set
		{
			m_uri = value;
		}
	}

	internal MdImageNodeVisitedEventArgs(Stream imageStream, string uri)
	{
		m_uri = uri;
		m_imageStream = imageStream;
	}
}
