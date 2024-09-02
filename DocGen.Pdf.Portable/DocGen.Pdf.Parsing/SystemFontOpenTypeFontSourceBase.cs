using System;

namespace DocGen.Pdf.Parsing;

internal abstract class SystemFontOpenTypeFontSourceBase : SystemFontFontSource
{
	private readonly SystemFontOpenTypeGlyphScaler scaler;

	private readonly SystemFontOpenTypeFontReader reader;

	internal abstract SystemFontOutlines Outlines { get; }

	internal SystemFontOpenTypeFontReader Reader => reader;

	internal abstract SystemFontCFFFontSource CFF { get; }

	internal abstract SystemFontCMap CMap { get; }

	internal abstract SystemFontHead Head { get; }

	internal abstract SystemFontHorizontalHeader HHea { get; }

	internal abstract SystemFontHorizontalMetrics HMtx { get; }

	internal abstract SystemFontKerning Kern { get; }

	internal abstract SystemFontGlyphSubstitution GSub { get; }

	internal abstract ushort GlyphCount { get; }

	internal ushort NumberOfHorizontalMetrics => HHea.NumberOfHMetrics;

	internal SystemFontOpenTypeGlyphScaler Scaler => scaler;

	public override short Ascender => HHea.Ascender;

	public override short Descender => HHea.Descender;

	public SystemFontOpenTypeFontSourceBase(SystemFontOpenTypeFontReader reader)
	{
		this.reader = reader;
		scaler = new SystemFontOpenTypeGlyphScaler(this);
	}

	public SystemFontGlyph GetGlyphMetrics(ushort glyphId, ushort previousGlyphId, double fontSize)
	{
		SystemFontGlyph glyphMetrics = GetGlyphMetrics(glyphId, fontSize);
		SystemFontKerningInfo kerning = Kern.GetKerning(previousGlyphId, glyphId);
		glyphMetrics.HorizontalKerning = scaler.FUnitsPointToPixels(kerning.HorizontalKerning, fontSize);
		glyphMetrics.VerticalKerning = scaler.FUnitsPointToPixels(kerning.VerticalKerning, fontSize);
		return glyphMetrics;
	}

	public SystemFontGlyph GetGlyphMetrics(ushort glyphId, double fontSize)
	{
		SystemFontGlyph systemFontGlyph = new SystemFontGlyph();
		systemFontGlyph.GlyphId = glyphId;
		scaler.ScaleGlyphMetrics(systemFontGlyph, fontSize);
		return systemFontGlyph;
	}

	public override void GetAdvancedWidth(SystemFontGlyph glyph)
	{
		scaler.ScaleGlyphMetrics(glyph, 1.0);
	}

	internal SystemFontScript GetScript(uint tag)
	{
		return GSub.GetScript(tag);
	}

	internal SystemFontFeature GetFeature(ushort index)
	{
		return GSub.GetFeature(index);
	}

	internal SystemFontLookup GetLookup(ushort index)
	{
		return GSub.GetLookup(index);
	}

	internal abstract SystemFontGlyphData GetGlyphData(ushort glyphID);

	public ushort GetGlyphId(ushort ch)
	{
		return CMap.GetGlyphId(ch);
	}

	public double GetLineHeight(double fontSize)
	{
		double units = Math.Abs(HHea.Ascender) + Math.Abs(HHea.Descender) + HHea.LineGap;
		return Scaler.FUnitsToPixels(units, fontSize);
	}

	public double GetBaselineOffset(double fontSize)
	{
		double units = Math.Abs(HHea.Ascender) + Math.Abs(HHea.LineGap);
		return Scaler.FUnitsToPixels(units, fontSize);
	}

	public override void GetGlyphOutlines(SystemFontGlyph glyph, double fontSize)
	{
		scaler.GetScaleGlyphOutlines(glyph, fontSize);
	}

	internal void Write(SystemFontFontWriter writer)
	{
		writer.WriteString(FontFamily);
		ushort num = 0;
		if (IsBold)
		{
			num = (ushort)(num | 1u);
		}
		if (IsItalic)
		{
			num = (ushort)(num | 2u);
		}
		writer.WriteUShort(num);
		writer.WriteUShort(GlyphCount);
		Head.Write(writer);
		CMap.Write(writer);
		HHea.Write(writer);
		HMtx.Write(writer);
		Kern.Write(writer);
		GSub.Write(writer);
	}
}
