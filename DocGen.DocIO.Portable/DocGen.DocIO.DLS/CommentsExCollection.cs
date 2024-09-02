namespace DocGen.DocIO.DLS;

internal class CommentsExCollection : CollectionImpl
{
	internal WCommentExtended this[int index] => base.InnerList[index] as WCommentExtended;

	internal CommentsExCollection(WordDocument doc)
		: base(doc, doc)
	{
	}

	internal void Add(WCommentExtended commentEx)
	{
		base.InnerList.Add(commentEx);
	}
}
