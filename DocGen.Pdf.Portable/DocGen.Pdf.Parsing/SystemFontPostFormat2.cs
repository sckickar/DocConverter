using System.Collections.Generic;

namespace DocGen.Pdf.Parsing;

internal class SystemFontPostFormat2 : SystemFontPost
{
	private Dictionary<string, ushort> glyphNames;

	public SystemFontPostFormat2(SystemFontOpenTypeFontSourceBase fontSource)
		: base(fontSource)
	{
	}

	private void CreateGlyphNamesMapping(ushort[] glyphNameIndex, string[] names)
	{
		glyphNames = new Dictionary<string, ushort>(glyphNameIndex.Length);
		for (int i = 0; i < glyphNameIndex.Length; i++)
		{
			ushort num = glyphNameIndex[i];
			if (num < SystemFontPost.macintoshStandardOrderNames.Length)
			{
				glyphNames[SystemFontPost.macintoshStandardOrderNames[num]] = (ushort)i;
				continue;
			}
			num = (ushort)(num - SystemFontPost.macintoshStandardOrderNames.Length);
			glyphNames[names[num]] = (ushort)i;
		}
	}

	public override ushort GetGlyphId(string name)
	{
		if (glyphNames.TryGetValue(name, out var value))
		{
			return value;
		}
		return 0;
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		base.Read(reader);
		ushort num = reader.ReadUShort();
		ushort[] array = new ushort[num];
		int num2 = SystemFontPost.macintoshStandardOrderGlyphIds.Count - 1;
		for (int i = 0; i < num; i++)
		{
			array[i] = reader.ReadUShort();
			if (array[i] > num2)
			{
				num2 = array[i];
			}
		}
		int num3 = num2 - SystemFontPost.macintoshStandardOrderGlyphIds.Count + 1;
		string[] array2 = new string[num3];
		for (int j = 0; j < num3; j++)
		{
			array2[j] = reader.ReadString();
		}
		CreateGlyphNamesMapping(array, array2);
	}
}
