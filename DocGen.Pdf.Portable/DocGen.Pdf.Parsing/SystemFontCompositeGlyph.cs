using System.Collections.Generic;

namespace DocGen.Pdf.Parsing;

internal class SystemFontCompositeGlyph : SystemFontGlyphData
{
	private List<SystemFontOutlinePoint[]> contours;

	internal override IEnumerable<SystemFontOutlinePoint[]> Contours => contours;

	private static SystemFontOutlinePoint GetTransformedPoint(SystemFontMatrix matrix, SystemFontOutlinePoint point)
	{
		return new SystemFontOutlinePoint(point.Flags)
		{
			Point = matrix.Transform(point.Point)
		};
	}

	public SystemFontCompositeGlyph(SystemFontOpenTypeFontSourceBase fontFile, ushort glyphIndex)
		: base(fontFile, glyphIndex)
	{
	}

	private void AddGlyph(SystemFontGlyphDescription gd)
	{
		SystemFontGlyphData glyphData = base.FontSource.GetGlyphData(gd.GlyphIndex);
		if (glyphData == null)
		{
			return;
		}
		foreach (SystemFontOutlinePoint[] contour in glyphData.Contours)
		{
			SystemFontOutlinePoint[] array = new SystemFontOutlinePoint[contour.Length];
			for (int i = 0; i < contour.Length; i++)
			{
				array[i] = GetTransformedPoint(gd.Transform, contour[i]);
			}
			contours.Add(array);
		}
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		contours = new List<SystemFontOutlinePoint[]>();
		SystemFontGlyphDescription systemFontGlyphDescription;
		do
		{
			systemFontGlyphDescription = new SystemFontGlyphDescription();
			systemFontGlyphDescription.Read(reader);
			AddGlyph(systemFontGlyphDescription);
		}
		while (systemFontGlyphDescription.CheckFlag(5));
	}
}
