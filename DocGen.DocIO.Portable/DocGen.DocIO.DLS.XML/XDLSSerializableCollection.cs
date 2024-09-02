using System.Collections;

namespace DocGen.DocIO.DLS.XML;

public abstract class XDLSSerializableCollection : CollectionImpl, IXDLSSerializableCollection, IEnumerable
{
	string IXDLSSerializableCollection.TagItemName => GetTagItemName();

	protected XDLSSerializableCollection(WordDocument doc, OwnerHolder owner)
		: base(doc, owner)
	{
	}

	IXDLSSerializable IXDLSSerializableCollection.AddNewItem(IXDLSContentReader reader)
	{
		OwnerHolder ownerHolder = CreateItem(reader);
		if (ownerHolder != null)
		{
			base.InnerList.Add(ownerHolder);
			ownerHolder.SetOwner(base.OwnerBase);
		}
		return ownerHolder as IXDLSSerializable;
	}

	internal virtual void CloneToImpl(CollectionImpl coll)
	{
		foreach (XDLSSerializableBase inner in base.InnerList)
		{
			object obj = inner.CloneInt();
			coll.InnerList.Add(obj);
			if (obj is OwnerHolder)
			{
				(obj as OwnerHolder).SetOwner(coll.OwnerBase);
			}
		}
	}

	protected abstract string GetTagItemName();

	protected abstract OwnerHolder CreateItem(IXDLSContentReader reader);
}
