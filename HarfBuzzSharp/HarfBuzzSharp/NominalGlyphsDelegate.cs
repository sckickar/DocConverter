using System;

namespace HarfBuzzSharp;

public delegate uint NominalGlyphsDelegate(Font font, object fontData, uint count, ReadOnlySpan<uint> codepoints, Span<uint> glyphs);
