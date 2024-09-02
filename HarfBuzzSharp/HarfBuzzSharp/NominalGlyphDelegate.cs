namespace HarfBuzzSharp;

public delegate bool NominalGlyphDelegate(Font font, object fontData, uint unicode, out uint glyph);
