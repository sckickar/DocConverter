using System.Collections.Generic;

namespace DocGen.Pdf;

internal class TrueTypeGlyphs : TableBase
{
	private readonly ushort glyphIndex;

	private List<OutlinePoint[]> m_contours;

	private int m_id = 4;

	private short m_numberOfContours;

	internal List<OutlinePoint[]> Contours
	{
		get
		{
			if (m_contours == null)
			{
				m_contours = new List<OutlinePoint[]>();
			}
			return m_contours;
		}
		set
		{
			m_contours = value;
		}
	}

	internal override int Id => m_id;

	internal short NumberOfContours
	{
		get
		{
			return m_numberOfContours;
		}
		set
		{
			m_numberOfContours = value;
		}
	}

	public ushort GlyphIndex => glyphIndex;

	public static TrueTypeGlyphs ReadGlyf(FontFile2 fontFile, ushort glyphIndex)
	{
		short num = fontFile.FontArrayReader.getnextshort();
		TrueTypeGlyphs trueTypeGlyphs = ((num == 0) ? new TrueTypeGlyphs(fontFile, glyphIndex) : ((num <= 0) ? ((TrueTypeGlyphs)new CompositeGlyph(fontFile, glyphIndex)) : ((TrueTypeGlyphs)new SimpleGlyf(fontFile, glyphIndex))));
		trueTypeGlyphs.NumberOfContours = num;
		trueTypeGlyphs.Read(fontFile.FontArrayReader);
		if (trueTypeGlyphs is SimpleGlyf)
		{
			SimpleGlyf simpleGlyf = trueTypeGlyphs as SimpleGlyf;
			trueTypeGlyphs.m_contours = simpleGlyf.Contours;
		}
		else if (trueTypeGlyphs is CompositeGlyph)
		{
			CompositeGlyph compositeGlyph = trueTypeGlyphs as CompositeGlyph;
			trueTypeGlyphs.m_contours = compositeGlyph.Contours;
		}
		return trueTypeGlyphs;
	}

	public TrueTypeGlyphs(FontFile2 fontFile, ushort glyphIndex)
		: base(fontFile)
	{
		this.glyphIndex = glyphIndex;
	}

	public TrueTypeGlyphs(FontFile2 fontFile)
		: base(fontFile)
	{
		glyphIndex = glyphIndex;
	}

	public override void Read(ReadFontArray reader)
	{
	}
}
