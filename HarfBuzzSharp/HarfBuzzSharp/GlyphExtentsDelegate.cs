namespace HarfBuzzSharp;

public delegate bool GlyphExtentsDelegate(Font font, object fontData, uint glyph, out GlyphExtents extents);
