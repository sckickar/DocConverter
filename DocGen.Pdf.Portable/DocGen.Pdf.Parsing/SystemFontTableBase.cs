namespace DocGen.Pdf.Parsing;

internal abstract class SystemFontTableBase
{
	private readonly SystemFontOpenTypeFontSourceBase fontSource;

	internal long Offset { get; set; }

	protected SystemFontOpenTypeFontReader Reader => fontSource.Reader;

	protected SystemFontOpenTypeFontSourceBase FontSource => fontSource;

	public SystemFontTableBase(SystemFontOpenTypeFontSourceBase fontSource)
	{
		this.fontSource = fontSource;
	}

	public abstract void Read(SystemFontOpenTypeFontReader reader);

	internal virtual void Write(SystemFontFontWriter writer)
	{
	}

	internal virtual void Import(SystemFontOpenTypeFontReader reader)
	{
	}
}
