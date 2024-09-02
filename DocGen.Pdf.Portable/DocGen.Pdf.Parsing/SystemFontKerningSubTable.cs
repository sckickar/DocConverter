using System;
using System.IO;

namespace DocGen.Pdf.Parsing;

internal abstract class SystemFontKerningSubTable : SystemFontTableBase
{
	public ushort Coverage { get; private set; }

	public bool IsHorizontal => GetBit(0);

	public bool HasMinimumValues => GetBit(1);

	public bool IsCrossStream => GetBit(2);

	public bool Override => GetBit(3);

	internal static SystemFontKerningSubTable ReadSubTable(SystemFontOpenTypeFontSourceBase fontSource, SystemFontOpenTypeFontReader reader)
	{
		long position = reader.Position;
		reader.ReadUShort();
		ushort num = reader.ReadUShort();
		ushort num2 = reader.ReadUShort();
		if (BitConverter.GetBytes(num2)[1] == 0)
		{
			SystemFontFormat0KerningSubTable systemFontFormat0KerningSubTable = new SystemFontFormat0KerningSubTable(fontSource);
			systemFontFormat0KerningSubTable.Coverage = num2;
			systemFontFormat0KerningSubTable.Read(reader);
			return systemFontFormat0KerningSubTable;
		}
		reader.Seek(position + num, SeekOrigin.Begin);
		return null;
	}

	internal static SystemFontKerningSubTable ImportSubTable(SystemFontOpenTypeFontSourceBase fontSource, SystemFontOpenTypeFontReader reader)
	{
		ushort num = reader.ReadUShort();
		if (BitConverter.GetBytes(num)[1] == 0)
		{
			SystemFontFormat0KerningSubTable systemFontFormat0KerningSubTable = new SystemFontFormat0KerningSubTable(fontSource);
			systemFontFormat0KerningSubTable.Coverage = num;
			systemFontFormat0KerningSubTable.Import(reader);
			return systemFontFormat0KerningSubTable;
		}
		return null;
	}

	public SystemFontKerningSubTable(SystemFontOpenTypeFontSourceBase fontSource)
		: base(fontSource)
	{
	}

	private bool GetBit(byte bit)
	{
		return SystemFontBitsHelper.GetBit(Coverage, bit);
	}

	public abstract short GetValue(ushort leftGlyphIndex, ushort rightGlyphIndex);

	internal override void Write(SystemFontFontWriter writer)
	{
		writer.WriteUShort(Coverage);
	}
}
