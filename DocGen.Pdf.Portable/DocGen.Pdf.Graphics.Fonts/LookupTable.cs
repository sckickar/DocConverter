using System.Collections.Generic;
using DocGen.Pdf.IO;

namespace DocGen.Pdf.Graphics.Fonts;

internal abstract class LookupTable
{
	private int m_flag;

	private int[] m_offsets;

	private OtfTable m_opentypeFontTable;

	internal int Flag
	{
		get
		{
			return m_flag;
		}
		set
		{
			m_flag = value;
		}
	}

	internal int[] Offsets
	{
		get
		{
			return m_offsets;
		}
		set
		{
			m_offsets = value;
		}
	}

	internal OtfTable OpenTypeFontTable
	{
		get
		{
			return m_opentypeFontTable;
		}
		set
		{
			m_opentypeFontTable = value;
		}
	}

	internal LookupTable(OtfTable otFontTable, int flag, int[] offsets)
	{
		Flag = flag;
		Offsets = offsets;
		OpenTypeFontTable = otFontTable;
	}

	internal GPOSValueRecord ReadGposValueRecord(OtfTable table, int foramt)
	{
		GPOSValueRecord gPOSValueRecord = new GPOSValueRecord();
		if (((uint)foramt & (true ? 1u : 0u)) != 0)
		{
			gPOSValueRecord.XPlacement = table.Reader.ReadInt16() * 1000 / table.m_ttfReader.unitsPerEM;
		}
		if (((uint)foramt & 2u) != 0)
		{
			gPOSValueRecord.YPlacement = table.Reader.ReadInt16() * 1000 / table.m_ttfReader.unitsPerEM;
		}
		if (((uint)foramt & 4u) != 0)
		{
			gPOSValueRecord.XAdvance = table.Reader.ReadInt16() * 1000 / table.m_ttfReader.unitsPerEM;
		}
		if (((uint)foramt & 8u) != 0)
		{
			gPOSValueRecord.YAdvance = table.Reader.ReadInt16() * 1000 / table.m_ttfReader.unitsPerEM;
		}
		if (((uint)foramt & 0x10u) != 0)
		{
			gPOSValueRecord.XPlaDevice = table.Reader.ReadInt16() * 1000 / table.m_ttfReader.unitsPerEM;
		}
		if (((uint)foramt & 0x20u) != 0)
		{
			gPOSValueRecord.YPlaDevice = table.Reader.ReadInt16() * 1000 / table.m_ttfReader.unitsPerEM;
		}
		if (((uint)foramt & 0x40u) != 0)
		{
			gPOSValueRecord.XAdvDevice = table.Reader.ReadInt16() * 1000 / table.m_ttfReader.unitsPerEM;
		}
		if (((uint)foramt & 0x80u) != 0)
		{
			gPOSValueRecord.YAdvDevice = table.Reader.ReadInt16() * 1000 / table.m_ttfReader.unitsPerEM;
		}
		return gPOSValueRecord;
	}

	internal IList<GPOSRecord> ReadMark(OtfTable table, int location)
	{
		table.Reader.Seek(location);
		int num = table.Reader.ReadUInt16();
		int[] array = new int[num];
		int[] array2 = new int[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = table.Reader.ReadUInt16();
			int num2 = table.Reader.ReadInt16();
			array2[i] = location + num2;
		}
		IList<GPOSRecord> list = new List<GPOSRecord>();
		for (int j = 0; j < num; j++)
		{
			GPOSRecord gPOSRecord = new GPOSRecord();
			gPOSRecord.Index = array[j];
			gPOSRecord.Record = ReadGposValueRecords(table, array2[j]);
			list.Add(gPOSRecord);
		}
		return list;
	}

	internal GPOSValueRecord ReadGposValueRecords(OtfTable table, int location)
	{
		if (location != 0)
		{
			table.Reader.Seek(location);
			table.Reader.ReadUInt16();
			return new GPOSValueRecord
			{
				XPlacement = table.Reader.ReadInt16() * 1000 / table.m_ttfReader.unitsPerEM,
				YPlacement = table.Reader.ReadInt16() * 1000 / table.m_ttfReader.unitsPerEM
			};
		}
		return null;
	}

	internal IList<GPOSValueRecord[]> ReadBaseArray(OtfTable table, int classCount, int location)
	{
		IList<GPOSValueRecord[]> list = new List<GPOSValueRecord[]>();
		table.Reader.Seek(location);
		int num = table.Reader.ReadUInt16();
		int[] locations = table.ReadUInt16(num * classCount, location);
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			list.Add(ReadAnchorArray(table, locations, num2, num2 + classCount));
			num2 += classCount;
		}
		return list;
	}

	internal GPOSValueRecord[] ReadAnchorArray(OtfTable tableReader, int[] locations, int left, int right)
	{
		GPOSValueRecord[] array = new GPOSValueRecord[right - left];
		for (int i = left; i < right; i++)
		{
			array[i - left] = ReadGposValueRecords(tableReader, locations[i]);
		}
		return array;
	}

	internal IList<IList<GPOSValueRecord[]>> ReadLigatureArray(OtfTable tableReader, int classCount, int location)
	{
		IList<IList<GPOSValueRecord[]>> list = new List<IList<GPOSValueRecord[]>>();
		tableReader.Reader.Seek(location);
		int num = tableReader.Reader.ReadUInt16();
		int[] array = tableReader.ReadUInt16(num, location);
		for (int i = 0; i < num; i++)
		{
			int num2 = array[i];
			IList<GPOSValueRecord[]> list2 = new List<GPOSValueRecord[]>();
			tableReader.Reader.Seek(num2);
			int num3 = tableReader.Reader.ReadUInt16();
			int[] locations = tableReader.ReadUInt16(classCount * num3, num2);
			int num4 = 0;
			for (int j = 0; j < num3; j++)
			{
				list2.Add(ReadAnchorArray(tableReader, locations, num4, num4 + classCount));
				num4 += classCount;
			}
			list.Add(list2);
		}
		return list;
	}

	internal CDEFTable GetTable(BigEndianReader reader, int offset)
	{
		CDEFTable cDEFTable = new CDEFTable();
		reader.Seek(offset);
		switch (reader.ReadUInt16())
		{
		case 1:
		{
			ushort num4 = reader.ReadUInt16();
			int num5 = reader.ReadInt16();
			int num6 = num4 + num5;
			for (int k = num4; k < num6; k++)
			{
				int value2 = reader.ReadInt16();
				cDEFTable.Records.Add(k, value2);
			}
			break;
		}
		case 2:
		{
			int num = reader.ReadUInt16();
			for (int i = 0; i < num; i++)
			{
				ushort num2 = reader.ReadUInt16();
				int num3 = reader.ReadUInt16();
				int value = reader.ReadUInt16();
				for (int j = num2; j <= num3; j++)
				{
					if (cDEFTable.Records.ContainsKey(j))
					{
						cDEFTable.Records[j] = value;
					}
					else
					{
						cDEFTable.Records.Add(j, value);
					}
				}
			}
			break;
		}
		default:
			cDEFTable = null;
			break;
		}
		return cDEFTable;
	}

	internal abstract bool ReplaceGlyph(OtfGlyphInfoList glyphList);

	internal abstract void ReadSubTable(int offset);

	internal virtual bool ReplaceGlyphs(OtfGlyphInfoList glyphInfoList)
	{
		bool flag = false;
		glyphInfoList.Index = glyphInfoList.Start;
		while (glyphInfoList.Index < glyphInfoList.End && glyphInfoList.Index >= glyphInfoList.Start)
		{
			flag = ReplaceGlyph(glyphInfoList) || flag;
		}
		return flag;
	}

	internal virtual bool IsSubstitute(int index)
	{
		return false;
	}

	internal virtual void ReadSubTables()
	{
		int[] offsets = Offsets;
		foreach (int offset in offsets)
		{
			ReadSubTable(offset);
		}
	}
}
