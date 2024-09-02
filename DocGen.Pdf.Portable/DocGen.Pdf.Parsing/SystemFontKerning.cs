using System.Collections.Generic;
using DocGen.PdfViewer.Base;

namespace DocGen.Pdf.Parsing;

internal class SystemFontKerning : SystemFontTrueTypeTableBase
{
	private List<SystemFontKerningSubTable> subTables;

	internal override uint Tag => SystemFontTags.KERN_TABLE;

	public SystemFontKerning(SystemFontOpenTypeFontSourceBase fontFile)
		: base(fontFile)
	{
	}

	public SystemFontKerningInfo GetKerning(ushort leftGlyphIndex, ushort rightGlyphIndex)
	{
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		short? num5 = null;
		short? num6 = null;
		short? num7 = null;
		short? num8 = null;
		foreach (SystemFontKerningSubTable subTable in subTables)
		{
			short value = subTable.GetValue(leftGlyphIndex, rightGlyphIndex);
			if (subTable.IsHorizontal)
			{
				if (subTable.HasMinimumValues)
				{
					if (subTable.IsCrossStream)
					{
						num6 = value;
					}
					else
					{
						num5 = value;
					}
				}
				else if (subTable.IsCrossStream)
				{
					num3 = ((!subTable.Override) ? (num3 + (double)value) : ((double)value));
				}
				else
				{
					num = ((!subTable.Override) ? ((double)value) : (num + (double)value));
				}
			}
			else if (subTable.HasMinimumValues)
			{
				if (subTable.IsCrossStream)
				{
					num8 = value;
				}
				else
				{
					num7 = value;
				}
			}
			else if (subTable.IsCrossStream)
			{
				num4 = ((!subTable.Override) ? (num4 + (double)value) : ((double)value));
			}
			else
			{
				num2 = ((!subTable.Override) ? (num2 + (double)value) : ((double)value));
			}
		}
		if (num5.HasValue && (double?)num5 > num)
		{
			num = num5.Value;
		}
		if (num6.HasValue && (double?)num6 > num3)
		{
			num3 = num6.Value;
		}
		if (num7.HasValue && (double?)num7 > num2)
		{
			num2 = num7.Value;
		}
		if (num8.HasValue && (double?)num8 > num4)
		{
			num4 = num8.Value;
		}
		return new SystemFontKerningInfo
		{
			HorizontalKerning = new Point(num, num3),
			VerticalKerning = new Point(num2, num4)
		};
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		reader.ReadUShort();
		ushort num = reader.ReadUShort();
		subTables = new List<SystemFontKerningSubTable>(num);
		for (int i = 0; i < num; i++)
		{
			SystemFontKerningSubTable systemFontKerningSubTable = SystemFontKerningSubTable.ReadSubTable(base.FontSource, reader);
			if (systemFontKerningSubTable != null)
			{
				subTables.Add(systemFontKerningSubTable);
			}
		}
	}

	internal override void Write(SystemFontFontWriter writer)
	{
		if (subTables == null)
		{
			writer.WriteUShort(0);
			return;
		}
		writer.WriteUShort((ushort)subTables.Count);
		for (int i = 0; i < subTables.Count; i++)
		{
			subTables[i].Write(writer);
		}
	}

	internal override void Import(SystemFontOpenTypeFontReader reader)
	{
		ushort num = reader.ReadUShort();
		if (num > 0)
		{
			subTables = new List<SystemFontKerningSubTable>(num);
			for (int i = 0; i < num; i++)
			{
				subTables.Add(SystemFontKerningSubTable.ImportSubTable(base.FontSource, reader));
			}
		}
	}
}
