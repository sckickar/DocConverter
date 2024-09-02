using System;

namespace HarfBuzzSharp;

public delegate void GlyphAdvancesDelegate(Font font, object fontData, uint count, ReadOnlySpan<uint> glyphs, Span<int> advances);
