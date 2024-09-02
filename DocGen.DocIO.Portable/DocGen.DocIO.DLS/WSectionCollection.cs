using System;
using System.Collections;

namespace DocGen.DocIO.DLS;

public class WSectionCollection : EntityCollection, IWSectionCollection, IEntityCollectionBase, ICollectionBase, IEnumerable
{
	private static readonly Type[] DEF_ELEMENT_TYPES = new Type[1] { typeof(WSection) };

	public new WSection this[int index] => base.InnerList[index] as WSection;

	protected override Type[] TypesOfElement => DEF_ELEMENT_TYPES;

	public WSectionCollection(WordDocument doc)
		: base(doc, doc)
	{
	}

	internal WSectionCollection()
		: base(null, null)
	{
	}

	public int Add(IWSection section)
	{
		return Add((IEntity)section);
	}

	public int IndexOf(IWSection section)
	{
		return (section as Entity).Index;
	}

	internal string GetText()
	{
		string text = string.Empty;
		WParagraph lastParagraph = base.Document.LastParagraph;
		for (int i = 0; i < base.Count; i++)
		{
			text += this[i].GetText(lastParagraph);
			if (i < base.Count - 1 && !text.EndsWith(ControlChar.ParagraphBreak))
			{
				text += ControlChar.ParagraphBreak;
			}
		}
		return text;
	}
}
