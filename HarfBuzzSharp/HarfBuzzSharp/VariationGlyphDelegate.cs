namespace HarfBuzzSharp;

public delegate bool VariationGlyphDelegate(Font font, object fontData, uint unicode, uint variationSelector, out uint glyph);
