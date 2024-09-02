using System.Collections.Generic;

namespace DocGen.Pdf.Parsing;

internal class SystemFontFormat0KerningSubTable : SystemFontKerningSubTable
{
	private Dictionary<ushort, Dictionary<ushort, short>> values;

	public SystemFontFormat0KerningSubTable(SystemFontOpenTypeFontSourceBase fontSource)
		: base(fontSource)
	{
	}

	public override short GetValue(ushort leftGlyphIndex, ushort rightGlyphIndex)
	{
		if (!values.TryGetValue(leftGlyphIndex, out Dictionary<ushort, short> value))
		{
			return 0;
		}
		if (!value.TryGetValue(rightGlyphIndex, out var value2))
		{
			return 0;
		}
		return value2;
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		ushort num = reader.ReadUShort();
		values = new Dictionary<ushort, Dictionary<ushort, short>>(num);
		reader.ReadUShort();
		reader.ReadUShort();
		reader.ReadUShort();
		for (int i = 0; i < num; i++)
		{
			ushort key = reader.ReadUShort();
			ushort key2 = reader.ReadUShort();
			short value = reader.ReadShort();
			if (!values.TryGetValue(key, out Dictionary<ushort, short> value2))
			{
				value2 = new Dictionary<ushort, short>();
				values[key] = value2;
			}
			value2[key2] = value;
		}
	}

	internal override void Write(SystemFontFontWriter writer)
	{
		base.Write(writer);
		writer.WriteUShort((ushort)values.Count);
		foreach (ushort key in values.Keys)
		{
			Dictionary<ushort, short> dictionary = values[key];
			writer.WriteUShort(key);
			writer.WriteUShort((ushort)dictionary.Count);
			foreach (ushort key2 in dictionary.Keys)
			{
				writer.WriteUShort(key2);
				writer.WriteShort(dictionary[key2]);
			}
		}
	}

	internal override void Import(SystemFontOpenTypeFontReader reader)
	{
		ushort num = reader.ReadUShort();
		values = new Dictionary<ushort, Dictionary<ushort, short>>(num);
		for (int i = 0; i < num; i++)
		{
			ushort key = reader.ReadUShort();
			ushort num2 = reader.ReadUShort();
			Dictionary<ushort, short> dictionary = new Dictionary<ushort, short>(num2);
			for (int j = 0; j < num2; j++)
			{
				ushort key2 = reader.ReadUShort();
				dictionary[key2] = reader.ReadShort();
			}
			values[key] = dictionary;
		}
	}
}
