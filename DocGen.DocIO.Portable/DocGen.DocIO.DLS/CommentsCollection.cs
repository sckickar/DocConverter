using System;
using System.Collections;
using System.Collections.Generic;

namespace DocGen.DocIO.DLS;

public class CommentsCollection : CollectionImpl
{
	public WComment this[int index] => base.InnerList[index] as WComment;

	public CommentsCollection(WordDocument doc)
		: base(doc, doc)
	{
	}

	public int Counts()
	{
		return base.InnerList.Count;
	}

	public void RemoveAt(int index)
	{
		WComment wComment = base.InnerList[index] as WComment;
		string paraId = (wComment.ChildEntities.LastItem as WParagraph).ParaId;
		List<string> list = new List<string>();
		if (m_doc.CommentsEx.Count > 0)
		{
			foreach (WCommentExtended item in m_doc.CommentsEx)
			{
				if (item.ParentParaId == paraId)
				{
					list.Add(item.ParaId);
				}
			}
		}
		base.InnerList.Remove(wComment);
		if (wComment.Owner is WParagraph)
		{
			wComment.OwnerParagraph.Items.Remove(wComment);
		}
		if (list.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < base.InnerList.Count; i++)
		{
			foreach (string item2 in list)
			{
				if (((base.InnerList[i] as WComment).ChildEntities.LastItem as WParagraph).ParaId == item2)
				{
					WComment wComment2 = base.InnerList[i] as WComment;
					base.InnerList.Remove(wComment2);
					if (wComment2.Owner is WParagraph)
					{
						wComment2.OwnerParagraph.Items.Remove(wComment2);
					}
					i--;
					break;
				}
			}
		}
	}

	public void Clear()
	{
		while (base.InnerList.Count > 0)
		{
			int index = base.InnerList.Count - 1;
			RemoveAt(index);
		}
	}

	internal void Add(WComment comment)
	{
		base.InnerList.Add(comment);
	}

	public void Remove(WComment comment)
	{
		base.InnerList.Remove(comment);
		comment.OwnerParagraph.Items.Remove(comment);
		string paraId = (comment.ChildEntities.LastItem as WParagraph).ParaId;
		List<string> list = new List<string>();
		if (m_doc.CommentsEx.Count > 0)
		{
			foreach (WCommentExtended item in m_doc.CommentsEx)
			{
				if (item.ParentParaId == paraId)
				{
					list.Add(item.ParaId);
				}
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < base.InnerList.Count; i++)
		{
			foreach (string item2 in list)
			{
				if (((base.InnerList[i] as WComment).ChildEntities.LastItem as WParagraph).ParaId == item2)
				{
					WComment wComment = base.InnerList[i] as WComment;
					base.InnerList.Remove(wComment);
					if (wComment.Owner is WParagraph)
					{
						wComment.OwnerParagraph.Items.Remove(wComment);
					}
					i--;
					break;
				}
			}
		}
	}

	internal void SetParentParaIDAndIsResolved()
	{
		if (base.Count <= 0)
		{
			return;
		}
		List<string> list = new List<string>();
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				WComment wComment = (WComment)enumerator.Current;
				if (wComment.ChildEntities.LastItem is WParagraph)
				{
					list.Add((wComment.ChildEntities.LastItem as WParagraph).ParaId);
				}
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
		enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				WComment obj = (WComment)enumerator.Current;
				obj.ParentParaId = obj.SetParentParaIdAndIsResolved(list);
			}
		}
		finally
		{
			IDisposable disposable2 = enumerator as IDisposable;
			if (disposable2 != null)
			{
				disposable2.Dispose();
			}
		}
	}
}
