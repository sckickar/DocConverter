using System;
using DocGen.DocIO.Rendering;
using DocGen.Drawing;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

public class WCommentMark : ParagraphItem, ILeafWidget, IWidget
{
	private string m_commentId;

	private CommentMarkType m_markType;

	private WComment m_ownerComment;

	private byte m_bFlags;

	public WComment Comment
	{
		get
		{
			return m_ownerComment;
		}
		internal set
		{
			m_ownerComment = value;
		}
	}

	internal string CommentId
	{
		get
		{
			return m_commentId;
		}
		set
		{
			m_commentId = value;
		}
	}

	public override EntityType EntityType => EntityType.CommentMark;

	public CommentMarkType Type
	{
		get
		{
			return m_markType;
		}
		set
		{
			m_markType = value;
		}
	}

	internal bool IsAfterCellMark
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal WCommentMark(WordDocument doc, string commentId)
		: base(doc)
	{
		m_commentId = commentId;
	}

	internal WCommentMark(WordDocument doc, string commentId, CommentMarkType type)
		: this(doc, commentId)
	{
		m_markType = type;
	}

	protected override object CloneImpl()
	{
		WCommentMark wCommentMark = (WCommentMark)base.CloneImpl();
		if (m_commentId == "")
		{
			if (m_markType == CommentMarkType.CommentStart)
			{
				wCommentMark.CommentId = Convert.ToString(TagIdRandomizer.GetMarkerId(Convert.ToInt32(m_commentId), newId: true));
			}
			else
			{
				wCommentMark.CommentId = Convert.ToString(TagIdRandomizer.GetMarkerId(Convert.ToInt32(m_commentId), newId: false));
			}
		}
		return wCommentMark;
	}

	protected override void CreateLayoutInfo()
	{
		m_layoutInfo = new LayoutInfo();
		if (base.Document.RevisionOptions.CommentDisplayMode == CommentDisplayMode.ShowInBalloons)
		{
			m_layoutInfo.IsSkip = false;
		}
	}

	internal override void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
		m_layoutInfo = null;
		if (this == entity)
		{
			isLastTOCEntry = true;
		}
	}

	SizeF ILeafWidget.Measure(DrawingContext dc)
	{
		return default(SizeF);
	}
}
