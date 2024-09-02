namespace HarfBuzzSharp;

public delegate bool GlyphContourPointDelegate(Font font, object fontData, uint glyph, uint pointIndex, out int x, out int y);
