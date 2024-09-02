using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace DocGen.Pdf.Parsing;

internal class SystemFontGlyphsSequence : IEnumerable<SystemFontGlyphInfo>, IEnumerable
{
	private readonly List<SystemFontGlyphInfo> store;

	public SystemFontGlyphInfo this[int index] => store[index];

	public int Count => store.Count;

	public SystemFontGlyphsSequence()
	{
		store = new List<SystemFontGlyphInfo>();
	}

	public SystemFontGlyphsSequence(int capacity)
	{
		store = new List<SystemFontGlyphInfo>(capacity);
	}

	public void Add(SystemFontGlyphInfo glyphInfo)
	{
		store.Add(glyphInfo);
	}

	public void Add(ushort glyphID, SystemFontGlyphForm form)
	{
		store.Add(new SystemFontGlyphInfo(glyphID, form));
	}

	public void Add(ushort glyphId)
	{
		store.Add(new SystemFontGlyphInfo(glyphId));
	}

	public void AddRange(IEnumerable<ushort> glyphIDs)
	{
		foreach (ushort glyphID in glyphIDs)
		{
			Add(glyphID);
		}
	}

	public void AddRange(IEnumerable<SystemFontGlyphInfo> glyphIDs)
	{
		store.AddRange(glyphIDs);
	}

	public SystemFontGlyphForm GetGlyphForm(int index)
	{
		return store[index].Form;
	}

	public IEnumerator<SystemFontGlyphInfo> GetEnumerator()
	{
		return store.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return store.GetEnumerator();
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(store[0]);
		for (int i = 1; i < store.Count; i++)
		{
			stringBuilder.Append(" ");
			stringBuilder.Append(store[i]);
		}
		return stringBuilder.ToString();
	}
}
