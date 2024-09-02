namespace DocGen.DocIO.DLS;

internal class ChildShapeCollection : CollectionImpl
{
	public ChildShape this[int index] => base.InnerList[index] as ChildShape;

	internal ChildShapeCollection(WordDocument doc)
		: base(doc, doc)
	{
	}

	public void Add(ChildShape childShape)
	{
		base.InnerList.Add(childShape);
		if (!base.Document.IsOpening && !childShape.skipPositionUpdate && !(childShape.Owner is ChildGroupShape))
		{
			UpdatePositionForGroupShapeAndChildShape(childShape);
		}
	}

	internal void UpdatePositionForGroupShapeAndChildShape(ChildShape childShape)
	{
		childShape.GetOwnerGroupShape().UpdatePositionForGroupShapeAndChildShape();
	}

	public void RemoveAt(int index)
	{
		base.InnerList.RemoveAt(index);
	}

	public void Clear()
	{
		while (base.InnerList.Count > 0)
		{
			int index = base.InnerList.Count - 1;
			RemoveAt(index);
		}
	}

	public void Remove(ChildShape childShape)
	{
		base.InnerList.Remove(childShape);
	}
}
