using System.IO;

namespace DocGen.DocIO.DLS;

public class HTMLImportSettings
{
	private byte m_bFlags;

	public bool IsConsiderListStyleAttribute
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal bool AllowUnsupportedCSSProperties
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	public event ImageNodeVisitedEventHandler ImageNodeVisited;

	internal ImageNodeVisitedEventArgs ExecuteImageNodeVisitedEvent(Stream imageStream, string uri)
	{
		ImageNodeVisitedEventArgs imageNodeVisitedEventArgs = new ImageNodeVisitedEventArgs(imageStream, uri);
		if (this.ImageNodeVisited != null)
		{
			this.ImageNodeVisited(this, imageNodeVisitedEventArgs);
		}
		return imageNodeVisitedEventArgs;
	}
}
