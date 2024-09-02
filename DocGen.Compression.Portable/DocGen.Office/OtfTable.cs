using System;
using System.Collections.Generic;
using System.IO;

namespace DocGen.Office;

internal abstract class OtfTable
{
	private BigEndianReader m_reader;

	private int m_offset;

	private GDEFTable m_gdefTable;

	private IList<LookupTable> m_lookupList;

	private ScriptRecordReader m_otScript;

	private FeatureRecordReader m_otFeature;

	internal TtfReader m_ttfReader;

	internal FeatureRecordReader OTFeature
	{
		get
		{
			return m_otFeature;
		}
		set
		{
			m_otFeature = value;
		}
	}

	internal IList<LookupTable> LookupList
	{
		get
		{
			return m_lookupList;
		}
		set
		{
			m_lookupList = value;
		}
	}

	internal BigEndianReader Reader
	{
		get
		{
			return m_reader;
		}
		set
		{
			m_reader = value;
		}
	}

	internal int Offset
	{
		get
		{
			return m_offset;
		}
		set
		{
			m_offset = value;
		}
	}

	internal GDEFTable GDEFTable
	{
		get
		{
			return m_gdefTable;
		}
		set
		{
			m_gdefTable = value;
		}
	}

	internal abstract LookupTable ReadLookupTable(int lookupType, int lookupFlag, int[] subTableLocations);

	protected internal OtfTable(BigEndianReader reader, int offset, GDEFTable gdef, TtfReader ttfReader)
	{
		m_reader = reader;
		m_offset = offset;
		m_gdefTable = gdef;
		m_ttfReader = ttfReader;
	}

	internal virtual OtfGlyphInfo GetGlyph(int index)
	{
		TtfGlyphInfo ttfGlyphInfo = m_ttfReader.ReadGlyph(index, isOpenType: true);
		return new OtfGlyphInfo(ttfGlyphInfo.CharCode, ttfGlyphInfo.Index, ttfGlyphInfo.Width);
	}

	internal virtual IList<LookupTable> GetLookups(FeatureRecord[] features)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (FeatureRecord featureRecord in features)
		{
			int[] indexes = featureRecord.Indexes;
			foreach (int key in indexes)
			{
				dictionary.Add(key, 1);
			}
		}
		IList<LookupTable> list = new List<LookupTable>();
		foreach (KeyValuePair<int, int> item in dictionary)
		{
			list.Add(m_lookupList[item.Key]);
		}
		return list;
	}

	internal virtual LanguageRecord LanguageRecord(string tag)
	{
		LanguageRecord result = null;
		if (tag != null)
		{
			foreach (ScriptRecord record in m_otScript.Records)
			{
				if (tag.Equals(record.ScriptTag))
				{
					result = record.Language;
					break;
				}
			}
		}
		return result;
	}

	internal int[] ReadUInt16(int size, int location)
	{
		int[] array = new int[size];
		for (int i = 0; i < size; i++)
		{
			int num = Reader.ReadUInt16();
			array[i] = ((num == 0) ? num : (num + location));
		}
		return array;
	}

	internal int[] ReadUInt32(int size)
	{
		return ReadUInt16(size, 0);
	}

	internal virtual void ReadFormatGlyphIds(int[] offsets, IList<ICollection<int>> formatGlyphs)
	{
		foreach (int offset in offsets)
		{
			formatGlyphs.Add(new List<int>(ReadFormat(offset)));
		}
	}

	internal IList<int> ReadFormat(int offset)
	{
		Reader.Seek(offset);
		int num = Reader.ReadInt16();
		IList<int> list;
		switch (num)
		{
		case 1:
		{
			int num3 = Reader.ReadInt16();
			list = new List<int>(num3);
			for (int j = 0; j < num3; j++)
			{
				int item = Reader.ReadInt16();
				list.Add(item);
			}
			break;
		}
		case 2:
		{
			int num2 = Reader.ReadInt16();
			list = new List<int>();
			for (int i = 0; i < num2; i++)
			{
				ReadRecord(Reader, list);
			}
			break;
		}
		default:
			throw new Exception($"Invalid format: {num}");
		}
		return list;
	}

	private void ReadRecord(BigEndianReader reader, IList<int> glyphIds)
	{
		short num = reader.ReadInt16();
		int num2 = reader.ReadInt16();
		reader.ReadInt16();
		for (int i = num; i <= num2; i++)
		{
			glyphIds.Add(i);
		}
	}

	internal virtual LookupSubTableRecord[] ReadSubstLookupRecords(int substCount)
	{
		LookupSubTableRecord[] array = new LookupSubTableRecord[substCount];
		for (int i = 0; i < substCount; i++)
		{
			LookupSubTableRecord lookupSubTableRecord = default(LookupSubTableRecord);
			lookupSubTableRecord.Index = Reader.ReadUInt16();
			lookupSubTableRecord.LookupIndex = Reader.ReadUInt16();
			array[i] = lookupSubTableRecord;
		}
		return array;
	}

	internal virtual FeatureTag[] ReadFeatureTag(int offset)
	{
		int num = Reader.ReadUInt16();
		FeatureTag[] array = new FeatureTag[num];
		for (int i = 0; i < num; i++)
		{
			FeatureTag featureTag = default(FeatureTag);
			featureTag.TagName = Reader.ReadUtf8String(4);
			featureTag.Offset = Reader.ReadUInt16() + offset;
			array[i] = featureTag;
		}
		return array;
	}

	internal void Initialize()
	{
		try
		{
			Reader.Seek(Offset);
			Reader.ReadInt32();
			int num = Reader.ReadUInt16();
			int num2 = Reader.ReadUInt16();
			int num3 = Reader.ReadUInt16();
			m_otScript = new ScriptRecordReader(this, Offset + num);
			m_otFeature = new FeatureRecordReader(this, Offset + num2);
			ReadLookupTables(Offset + num3);
		}
		catch (IOException innerException)
		{
			throw new Exception("Can't read the font file.", innerException);
		}
	}

	private void ReadLookupTables(int offset)
	{
		m_lookupList = new List<LookupTable>();
		Reader.Seek(offset);
		int size = Reader.ReadUInt16();
		int[] array = ReadUInt16(size, offset);
		foreach (int num in array)
		{
			Reader.Seek(num);
			int lookupType = Reader.ReadUInt16();
			int lookupFlag = Reader.ReadUInt16();
			int size2 = Reader.ReadUInt16();
			int[] subTableLocations = ReadUInt16(size2, num);
			m_lookupList.Add(ReadLookupTable(lookupType, lookupFlag, subTableLocations));
		}
	}
}
