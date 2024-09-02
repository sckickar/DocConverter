using System.IO;

namespace DocGen.Pdf.Parsing;

internal class SystemFontSubRuleSet : SystemFontTableBase
{
	private ushort[] subRuleOffsets;

	private SystemFontSubRule[] subRules;

	public SystemFontSubRule[] SubRules
	{
		get
		{
			if (subRules == null)
			{
				subRules = new SystemFontSubRule[subRuleOffsets.Length];
				for (int i = 0; i < subRules.Length; i++)
				{
					subRules[i] = ReadSubRule(base.Reader, subRuleOffsets[i]);
				}
			}
			return subRules;
		}
	}

	public SystemFontSubRuleSet(SystemFontOpenTypeFontSourceBase fontFile)
		: base(fontFile)
	{
	}

	private SystemFontSubRule ReadSubRule(SystemFontOpenTypeFontReader reader, ushort offset)
	{
		reader.BeginReadingBlock();
		long offset2 = base.Offset + offset;
		reader.Seek(offset2, SeekOrigin.Begin);
		SystemFontSubRule systemFontSubRule = new SystemFontSubRule(base.FontSource);
		systemFontSubRule.Read(reader);
		systemFontSubRule.Offset = offset2;
		reader.EndReadingBlock();
		return systemFontSubRule;
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		ushort num = reader.ReadUShort();
		subRuleOffsets = new ushort[num];
		for (int i = 0; i < num; i++)
		{
			subRuleOffsets[i] = reader.ReadUShort();
		}
	}
}
