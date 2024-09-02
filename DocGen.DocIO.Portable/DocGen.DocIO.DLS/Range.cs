using System.Collections;

namespace DocGen.DocIO.DLS;

internal class Range : CollectionImpl
{
	internal IList Items => base.InnerList;

	internal Range(WordDocument doc, OwnerHolder owner)
		: base(doc, owner)
	{
	}

	internal Range()
	{
	}

	internal void CloneItemsTo(Range range)
	{
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			if (base.InnerList[i] is Entity value)
			{
				range.Items.Add(value);
			}
		}
	}

	internal bool ContainTextBodyItems()
	{
		bool result = false;
		foreach (Entity item in Items)
		{
			if (item is WParagraph || item is WTable)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	internal int GetLastParagraphItemIndex()
	{
		int result = 0;
		foreach (Entity item in Items)
		{
			if (item is WParagraph || item is WTable)
			{
				break;
			}
			result = Items.IndexOf(item);
		}
		return result;
	}
}
