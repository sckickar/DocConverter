namespace DocGen.DocIO.DLS;

internal class ListOverrideStyleCollection : StyleCollection
{
	public new ListOverrideStyle this[int index] => (ListOverrideStyle)base.InnerList[index];

	internal ListOverrideStyleCollection(WordDocument doc)
		: base(doc)
	{
	}

	internal int Add(ListOverrideStyle listOverrideStyle)
	{
		listOverrideStyle.CloneRelationsTo(base.Document, null);
		return base.InnerList.Add(listOverrideStyle);
	}

	public new ListOverrideStyle FindByName(string name)
	{
		return base.FindByName(name) as ListOverrideStyle;
	}

	internal bool HasEquivalentStyle(ListOverrideStyle listOverrideStyle)
	{
		for (int i = 0; i < base.Count; i++)
		{
			if (this[i].Compare(listOverrideStyle))
			{
				return true;
			}
		}
		return false;
	}

	internal ListOverrideStyle GetEquivalentStyle(ListOverrideStyle listOverrideStyle)
	{
		for (int i = 0; i < base.Count; i++)
		{
			ListOverrideStyle listOverrideStyle2 = this[i];
			if (listOverrideStyle2.Compare(listOverrideStyle))
			{
				return listOverrideStyle2;
			}
		}
		return null;
	}
}
