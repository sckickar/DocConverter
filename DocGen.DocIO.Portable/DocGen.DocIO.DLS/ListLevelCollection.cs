using System;
using DocGen.DocIO.DLS.XML;

namespace DocGen.DocIO.DLS;

public class ListLevelCollection : XDLSSerializableCollection
{
	public WListLevel this[int index] => (WListLevel)base.InnerList[index];

	internal ListLevelCollection(ListStyle owner)
		: base(owner.Document, owner)
	{
	}

	internal int Add(WListLevel level)
	{
		if (level == null)
		{
			throw new ArgumentNullException("level");
		}
		level.SetOwner(base.OwnerBase);
		return base.InnerList.Add(level);
	}

	internal int IndexOf(WListLevel level)
	{
		return base.InnerList.IndexOf(level);
	}

	internal void Clear()
	{
		base.InnerList.Clear();
	}

	protected override OwnerHolder CreateItem(IXDLSContentReader reader)
	{
		return new WListLevel(base.OwnerBase as ListStyle);
	}

	protected override string GetTagItemName()
	{
		return "level";
	}

	internal bool Compare(ListLevelCollection listLevels)
	{
		if (base.Count != listLevels.Count)
		{
			return false;
		}
		for (int i = 0; i < listLevels.Count; i++)
		{
			if (!listLevels[i].Compare(this[i]))
			{
				return false;
			}
		}
		return true;
	}
}
