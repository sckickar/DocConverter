using System.IO;

namespace DocGen.Office.Markdown;

public class MdImportSettings
{
	internal bool IsEventSubscribed => this.ImageNodeVisited != null;

	public event MdImageNodeVisitedEventHandler ImageNodeVisited;

	internal MdImageNodeVisitedEventArgs ExecuteImageNodeVisitedEvent(Stream imageStream, string uri)
	{
		MdImageNodeVisitedEventArgs mdImageNodeVisitedEventArgs = new MdImageNodeVisitedEventArgs(imageStream, uri);
		if (this.ImageNodeVisited != null)
		{
			this.ImageNodeVisited(this, mdImageNodeVisitedEventArgs);
		}
		return mdImageNodeVisitedEventArgs;
	}
}
