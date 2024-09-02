namespace DocGen.Pdf.Parsing;

internal abstract class SystemFontCFFTable
{
	private readonly SystemFontCFFFontFile file;

	private readonly long offset;

	protected SystemFontCFFFontReader Reader => file.Reader;

	internal SystemFontCFFFontFile File => file;

	public long Offset => offset;

	public SystemFontCFFTable(SystemFontCFFFontFile file, long offset)
	{
		this.file = file;
		this.offset = offset;
	}

	public abstract void Read(SystemFontCFFFontReader reader);
}
