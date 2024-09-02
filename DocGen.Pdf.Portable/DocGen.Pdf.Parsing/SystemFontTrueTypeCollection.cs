using System.Collections.Generic;

namespace DocGen.Pdf.Parsing;

internal class SystemFontTrueTypeCollection
{
	private readonly SystemFontOpenTypeFontReader reader;

	private SystemFontTCCHeader header;

	internal SystemFontOpenTypeFontReader Reader => reader;

	public IEnumerable<SystemFontOpenTypeFontSourceBase> Fonts => header.Fonts;

	public SystemFontTrueTypeCollection(SystemFontOpenTypeFontReader reader)
	{
		this.reader = reader;
	}

	public void Initialzie()
	{
		header = new SystemFontTCCHeader(this);
		header.Read(Reader);
	}
}
