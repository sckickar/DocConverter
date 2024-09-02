using System.Collections;

namespace DocGen.DocIO.DLS;

public class RevisionCollection : CollectionImpl, ICollectionBase, IEnumerable
{
	public Revision this[int index] => base.InnerList[index] as Revision;

	public void AcceptAll()
	{
		while (base.Count > 0)
		{
			this[0].Accept();
		}
	}

	public void RejectAll()
	{
		while (base.Count > 0)
		{
			this[0].Reject();
		}
	}

	internal void Add(Revision revision)
	{
		base.InnerList.Add(revision);
		revision.Owner = this;
	}

	internal void Remove(Revision revision)
	{
		base.InnerList.Remove(revision);
	}

	internal void CloneItemsTo(RevisionCollection childRevisions)
	{
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			Revision revision = (base.InnerList[i] as Revision).Clone();
			if (revision != null)
			{
				childRevisions.Add(revision);
			}
		}
	}

	internal RevisionCollection(WordDocument doc)
		: base(doc, doc)
	{
	}

	internal override void Close()
	{
		while (base.InnerList.Count > 0)
		{
			int index = base.InnerList.Count - 1;
			this[index].Close();
			Remove(this[index]);
		}
		base.Close();
	}
}
