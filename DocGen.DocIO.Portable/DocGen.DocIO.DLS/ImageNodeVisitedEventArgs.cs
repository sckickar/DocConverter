using System;
using System.IO;

namespace DocGen.DocIO.DLS;

public class ImageNodeVisitedEventArgs : EventArgs
{
	private Stream m_imageStream;

	private string m_uri;

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
			if (m_uri != null)
			{
				return m_uri;
			}
			return string.Empty;
		}
		set
		{
			m_uri = value;
		}
	}

	internal ImageNodeVisitedEventArgs(Stream imageStream, string uri)
	{
		m_uri = uri;
		m_imageStream = imageStream;
	}
}
