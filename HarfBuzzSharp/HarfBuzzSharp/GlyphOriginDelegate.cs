namespace HarfBuzzSharp;

public delegate bool GlyphOriginDelegate(Font font, object fontData, uint glyph, out int x, out int y);
