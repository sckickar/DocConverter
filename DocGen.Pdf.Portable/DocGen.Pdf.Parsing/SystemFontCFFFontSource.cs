using System;

namespace DocGen.Pdf.Parsing;

internal class SystemFontCFFFontSource : SystemFontFontSource
{
	private readonly SystemFontCFFFontFile file;

	private readonly SystemFontTop top;

	internal SystemFontCFFFontFile File => file;

	internal SystemFontCFFFontReader Reader => file.Reader;

	public override string FontFamily => top.FamilyName;

	public override bool IsBold
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override bool IsItalic
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override short Ascender
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override short Descender
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public bool UsesCIDFontOperators => top.UsesCIDFontOperators;

	public SystemFontCFFFontSource(SystemFontCFFFontFile file, SystemFontTop top)
	{
		this.file = file;
		this.top = top;
	}

	public ushort GetGlyphId(string name)
	{
		return top.GetGlyphId(name);
	}

	public ushort GetGlyphId(ushort cid)
	{
		return top.GetGlyphId(cid);
	}

	public string GetGlyphName(ushort cid)
	{
		return top.GetGlyphName(cid);
	}

	public override void GetAdvancedWidth(SystemFontGlyph glyph)
	{
		glyph.AdvancedWidth = (double)(int)top.GetAdvancedWidth(glyph.GlyphId) / 1000.0;
	}

	public override void GetGlyphOutlines(SystemFontGlyph glyph, double fontSize)
	{
		top.GetGlyphOutlines(glyph, fontSize);
	}
}
