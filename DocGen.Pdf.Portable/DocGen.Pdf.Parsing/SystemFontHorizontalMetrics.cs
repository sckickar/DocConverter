namespace DocGen.Pdf.Parsing;

internal class SystemFontHorizontalMetrics : SystemFontTrueTypeTableBase
{
	private SystemFontLongHorMetric[] hMetrics;

	private short[] leftSideBearing;

	internal override uint Tag => SystemFontTags.HMTX_TABLE;

	public SystemFontHorizontalMetrics(SystemFontOpenTypeFontSourceBase fontFile)
		: base(fontFile)
	{
	}

	public ushort GetAdvancedWidth(int glyphID)
	{
		if (hMetrics != null)
		{
			if (glyphID < hMetrics.Length)
			{
				return hMetrics[glyphID].AdvanceWidth;
			}
			return hMetrics[hMetrics.Length - 1].AdvanceWidth;
		}
		return 0;
	}

	public short GetLeftSideBearing(int glyphID)
	{
		if (glyphID < hMetrics.Length)
		{
			return hMetrics[glyphID].LSB;
		}
		return leftSideBearing[glyphID - hMetrics.Length];
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		hMetrics = new SystemFontLongHorMetric[base.FontSource.NumberOfHorizontalMetrics];
		for (int i = 0; i < hMetrics.Length; i++)
		{
			SystemFontLongHorMetric systemFontLongHorMetric = new SystemFontLongHorMetric();
			systemFontLongHorMetric.Read(reader);
			hMetrics[i] = systemFontLongHorMetric;
		}
		leftSideBearing = new short[base.FontSource.GlyphCount - base.FontSource.NumberOfHorizontalMetrics];
		for (int j = 0; j < leftSideBearing.Length; j++)
		{
			leftSideBearing[j] = reader.ReadShort();
		}
	}

	internal override void Write(SystemFontFontWriter writer)
	{
		for (int i = 0; i < hMetrics.Length; i++)
		{
			hMetrics[i].Write(writer);
		}
	}

	internal override void Import(SystemFontOpenTypeFontReader reader)
	{
		hMetrics = new SystemFontLongHorMetric[base.FontSource.NumberOfHorizontalMetrics];
		for (int i = 0; i < base.FontSource.NumberOfHorizontalMetrics; i++)
		{
			SystemFontLongHorMetric systemFontLongHorMetric = new SystemFontLongHorMetric();
			systemFontLongHorMetric.Read(reader);
			hMetrics[i] = systemFontLongHorMetric;
		}
	}
}
