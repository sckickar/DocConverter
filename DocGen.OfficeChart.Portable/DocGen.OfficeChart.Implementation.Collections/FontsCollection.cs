using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class FontsCollection : CollectionBaseEx<FontImpl>
{
	private WorkbookImpl m_book;

	private Dictionary<FontImpl, FontImpl> m_hashFonts = new Dictionary<FontImpl, FontImpl>();

	public new IOfficeFont this[int index]
	{
		get
		{
			if (index < 0 || index >= base.InnerList.Count)
			{
				throw new ArgumentOutOfRangeException("index", "Index is out of range.");
			}
			return base.InnerList[index];
		}
	}

	public FontsCollection(IApplication application, object parent)
		: base(application, parent)
	{
		SetParents();
	}

	[CLSCompliant(false)]
	public FontImpl Add(FontImpl font, FontRecord record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		FontImpl font2 = base.AppImplementation.CreateFont(this, font);
		return (FontImpl)Add(font2);
	}

	public IOfficeFont Add(IOfficeFont font)
	{
		if (!(font is FontImpl fontImpl))
		{
			throw new ArgumentException("Can't add font.");
		}
		if (fontImpl.HasParagrapAlign || fontImpl.HasCapOrCharacterSpaceOrKerning)
		{
			ForceAdd(fontImpl);
			return fontImpl;
		}
		if (m_hashFonts.ContainsKey(fontImpl))
		{
			return m_hashFonts[fontImpl];
		}
		ForceAdd(fontImpl);
		return fontImpl;
	}

	public void InsertDefaultFonts()
	{
		FontRecord fontRecord = (FontRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Font);
		fontRecord.FontName = base.AppImplementation.StandardFont;
		fontRecord.FontHeight = (ushort)FontImpl.SizeInTwips(base.AppImplementation.StandardFontSize);
		ForceAdd(base.AppImplementation.CreateFont(this, fontRecord));
		fontRecord = (FontRecord)fontRecord.Clone();
		ForceAdd(base.AppImplementation.CreateFont(this, fontRecord));
		fontRecord = (FontRecord)fontRecord.Clone();
		ForceAdd(base.AppImplementation.CreateFont(this, fontRecord));
		fontRecord = (FontRecord)fontRecord.Clone();
		ForceAdd(base.AppImplementation.CreateFont(this, fontRecord));
		base.InnerList.Add(base.InnerList[0]);
	}

	private void SetParents()
	{
		m_book = FindParent(typeof(WorkbookImpl)) as WorkbookImpl;
		if (m_book == null)
		{
			throw new ArgumentNullException("Can't find parent workbook.");
		}
	}

	public void ForceAdd(FontImpl font)
	{
		if (base.InnerList.Count == 4)
		{
			base.InnerList.Add(base.InnerList[0]);
		}
		font.Index = base.InnerList.Count;
		base.InnerList.Add(font);
		if (!m_hashFonts.ContainsKey(font))
		{
			m_hashFonts.Add(font, font);
		}
	}

	[CLSCompliant(false)]
	public void Serialize(OffsetArrayList records)
	{
		int i = 0;
		for (int count = base.InnerList.Count; i < count; i++)
		{
			if (i != 4)
			{
				base.InnerList[i].Serialize(records);
			}
		}
	}

	public new bool Contains(FontImpl font)
	{
		return m_hashFonts.ContainsKey(font);
	}

	public Dictionary<int, int> AddRange(FontsCollection arrFonts)
	{
		if (arrFonts == null)
		{
			throw new ArgumentNullException("arrFonts");
		}
		if (arrFonts == this)
		{
			return null;
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		int i = 0;
		for (int count = arrFonts.Count; i < count; i++)
		{
			if (i == 4)
			{
				dictionary.Add(i, i);
				continue;
			}
			FontImpl font = arrFonts[i] as FontImpl;
			int value = AddCopy(font);
			dictionary.Add(i, value);
		}
		return dictionary;
	}

	public Dictionary<int, int> AddRange(ICollection<int> colFonts, FontsCollection sourceFonts)
	{
		if (colFonts == null)
		{
			throw new ArgumentNullException("colFonts");
		}
		if (sourceFonts == null)
		{
			throw new ArgumentNullException("sourceFonts");
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (int colFont in colFonts)
		{
			FontImpl fontImpl = (FontImpl)sourceFonts[colFont];
			int index = fontImpl.Index;
			int value = AddCopy(fontImpl);
			dictionary.Add(index, value);
		}
		return dictionary;
	}

	private int AddCopy(FontImpl font)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		font = Add(font, font.Record);
		return font.Index;
	}

	protected override void OnClearComplete()
	{
		m_hashFonts.Clear();
	}

	public FontsCollection Clone(WorkbookImpl parent)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		FontsCollection fontsCollection = new FontsCollection(base.Application, parent);
		List<FontImpl> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			FontImpl fontImpl = innerList[i];
			fontImpl = fontImpl.Clone(fontsCollection);
			if (i != 4)
			{
				fontsCollection.ForceAdd(fontImpl);
			}
		}
		return fontsCollection;
	}

	internal void Dispose()
	{
		foreach (FontImpl inner in base.InnerList)
		{
			inner.Clear();
		}
		base.InnerList.Clear();
	}
}
