namespace DocGen.Pdf.Parsing;

internal class SystemFontGlyphInfo
{
	public ushort GlyphId { get; private set; }

	public SystemFontGlyphForm Form { get; private set; }

	public SystemFontGlyphInfo(ushort glyphId)
	{
		GlyphId = glyphId;
		Form = SystemFontGlyphForm.Undefined;
	}

	public SystemFontGlyphInfo(ushort glyphID, SystemFontGlyphForm form)
	{
		GlyphId = glyphID;
		Form = form;
	}

	public override string ToString()
	{
		return $"{GlyphId}({Form});";
	}
}
