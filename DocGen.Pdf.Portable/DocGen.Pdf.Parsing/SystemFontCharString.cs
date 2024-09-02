using System.IO;

namespace DocGen.Pdf.Parsing;

internal class SystemFontCharString : SystemFontIndex
{
	private SystemFontType1GlyphData[] glyphOutlines;

	private SystemFontTop top;

	public SystemFontType1GlyphData this[ushort index]
	{
		get
		{
			if (glyphOutlines[index] == null)
			{
				glyphOutlines[index] = ReadGlyphData(base.Reader, base.Offsets[index], GetDataLength(index));
			}
			return glyphOutlines[index];
		}
	}

	public SystemFontCharString(SystemFontTop top, long offset)
		: base(top.File, offset)
	{
		this.top = top;
	}

	public int GetAdvancedWidth(ushort glyphId, int defaultWidth, int nominalWidth)
	{
		SystemFontType1GlyphData systemFontType1GlyphData = this[glyphId];
		if (!systemFontType1GlyphData.HasWidth)
		{
			return defaultWidth;
		}
		return systemFontType1GlyphData.AdvancedWidth + nominalWidth;
	}

	public void GetGlyphOutlines(SystemFontGlyph glyph, double fontSize)
	{
		SystemFontGlyphOutlinesCollection systemFontGlyphOutlinesCollection = this[glyph.GlyphId].Oultlines.Clone();
		SystemFontMatrix fontMatrix = top.FontMatrix;
		fontMatrix.ScaleAppend(fontSize, 0.0 - fontSize, 0.0, 0.0);
		systemFontGlyphOutlinesCollection.Transform(fontMatrix);
		glyph.Outlines = systemFontGlyphOutlinesCollection;
	}

	private SystemFontType1GlyphData ReadGlyphData(SystemFontCFFFontReader reader, uint offset, int length)
	{
		reader.BeginReadingBlock();
		reader.Seek(base.DataOffset + offset, SeekOrigin.Begin);
		byte[] array = new byte[length];
		reader.Read(array, array.Length);
		SystemFontBuildChar systemFontBuildChar = new SystemFontBuildChar(top);
		systemFontBuildChar.Execute(array);
		reader.EndReadingBlock();
		SystemFontGlyphOutlinesCollection outlines = systemFontBuildChar.GlyphOutlines;
		return new SystemFontType1GlyphData(outlines, (ushort?)systemFontBuildChar.Width);
	}

	public override void Read(SystemFontCFFFontReader reader)
	{
		base.Read(reader);
		glyphOutlines = new SystemFontType1GlyphData[base.Count];
	}
}
