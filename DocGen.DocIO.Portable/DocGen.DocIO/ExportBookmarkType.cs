using System;

namespace DocGen.DocIO;

[Flags]
public enum ExportBookmarkType
{
	Bookmarks = 1,
	Headings = 2,
	None = 0
}
