using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.PdfViewer.Base;

namespace DocGen.Pdf.Parsing;

internal class SystemFontGlyphData : SystemFontTrueTypeTableBase
{
	private readonly ushort glyphIndex;

	internal virtual IEnumerable<SystemFontOutlinePoint[]> Contours => SystemFontEnumerable.Empty<SystemFontOutlinePoint[]>();

	internal override uint Tag => SystemFontTags.GLYF_TABLE;

	internal short NumberOfContours { get; set; }

	public ushort GlyphIndex => glyphIndex;

	public Rectangle BoundingRect { get; set; }

	public static SystemFontGlyphData ReadGlyf(SystemFontOpenTypeFontSourceBase fontFile, ushort glyphIndex)
	{
		short num = fontFile.Reader.ReadShort();
		SystemFontGlyphData systemFontGlyphData = ((num == 0) ? new SystemFontGlyphData(fontFile, glyphIndex) : ((num <= 0) ? ((SystemFontGlyphData)new SystemFontCompositeGlyph(fontFile, glyphIndex)) : ((SystemFontGlyphData)new SystemFontSimpleGlyph(fontFile, glyphIndex))));
		systemFontGlyphData.NumberOfContours = num;
		systemFontGlyphData.BoundingRect = new Rect(new DocGen.PdfViewer.Base.Point(fontFile.Reader.ReadShort(), fontFile.Reader.ReadShort()), new DocGen.PdfViewer.Base.Point(fontFile.Reader.ReadShort(), fontFile.Reader.ReadShort()));
		systemFontGlyphData.Read(fontFile.Reader);
		return systemFontGlyphData;
	}

	public SystemFontGlyphData(SystemFontOpenTypeFontSourceBase fontFile, ushort glyphIndex)
		: base(fontFile)
	{
		this.glyphIndex = glyphIndex;
		BoundingRect = Rect.Empty;
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
	}
}
