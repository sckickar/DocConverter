namespace DocGen.Pdf.Parsing;

internal abstract class SystemFontTrueTypeTableBase : SystemFontTableBase
{
	internal abstract uint Tag { get; }

	public SystemFontTrueTypeTableBase(SystemFontOpenTypeFontSourceBase fontSource)
		: base(fontSource)
	{
	}
}
