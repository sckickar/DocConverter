using System.Collections.Generic;

namespace DocGen.Pdf;

internal class CompositeGlyph : TrueTypeGlyphs
{
	private List<OutlinePoint[]> contours;

	internal new List<OutlinePoint[]> Contours => contours;

	private static OutlinePoint GetTransformedPoint(GlyphDescription compostite, OutlinePoint point)
	{
		return new OutlinePoint(point.Flags)
		{
			Point = compostite.Transformpoint(point.Point)
		};
	}

	public CompositeGlyph(FontFile2 fontFile, ushort glyphIndex)
		: base(fontFile, glyphIndex)
	{
	}

	private void AddGlyph(GlyphDescription gd)
	{
		TrueTypeGlyphs trueTypeGlyphs = base.FontSource.readGlyphdata(gd.GlyphIndex);
		if (trueTypeGlyphs == null)
		{
			return;
		}
		OutlinePoint[] array;
		foreach (OutlinePoint[] contour in trueTypeGlyphs.Contours)
		{
			array = new OutlinePoint[contour.Length];
			for (int i = 0; i < contour.Length; i++)
			{
				array[i] = GetTransformedPoint(gd, contour[i]);
			}
			contours.Add(array);
		}
		array = null;
	}

	public override void Read(ReadFontArray reader)
	{
		reader.getnextshort();
		reader.getnextshort();
		reader.getnextshort();
		reader.getnextshort();
		contours = new List<OutlinePoint[]>();
		GlyphDescription glyphDescription;
		do
		{
			glyphDescription = new GlyphDescription();
			glyphDescription.Read(reader);
			AddGlyph(glyphDescription);
		}
		while (glyphDescription.CheckFlag(5));
	}
}
