namespace DocGen.Pdf.Parsing;

internal class SystemFontCoverageFormat2 : SystemFontCoverage
{
	private SystemFontRangeRecord[] rangeRecords;

	public SystemFontCoverageFormat2(SystemFontOpenTypeFontSourceBase fontFile)
		: base(fontFile)
	{
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		ushort num = reader.ReadUShort();
		rangeRecords = new SystemFontRangeRecord[num];
		for (int i = 0; i < num; i++)
		{
			rangeRecords[i] = new SystemFontRangeRecord();
			rangeRecords[i].Read(reader);
		}
	}

	internal override void Write(SystemFontFontWriter writer)
	{
		writer.WriteUShort(2);
		writer.WriteUShort((ushort)rangeRecords.Length);
		for (int i = 0; i < rangeRecords.Length; i++)
		{
			rangeRecords[i].Write(writer);
		}
	}

	public override int GetCoverageIndex(ushort glyphIndex)
	{
		SystemFontRangeRecord[] array = rangeRecords;
		foreach (SystemFontRangeRecord systemFontRangeRecord in array)
		{
			if (systemFontRangeRecord.Start <= glyphIndex && glyphIndex <= systemFontRangeRecord.End)
			{
				return systemFontRangeRecord.GetCoverageIndex(glyphIndex);
			}
		}
		return -1;
	}
}
