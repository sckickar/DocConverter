using DocGen.DocIO.DLS;

namespace DocGen.Layouting;

internal class CommentsMarkups : TrackChangesMarkups
{
	private WComment m_comment;

	private float m_extraSpacing;

	internal WComment Comment => m_comment;

	internal string CommentID
	{
		get
		{
			if (Comment.CommentRangeEnd == null)
			{
				return "0";
			}
			return Comment.CommentRangeEnd.CommentId;
		}
	}

	internal float ExtraSpacing
	{
		get
		{
			return m_extraSpacing;
		}
		set
		{
			m_extraSpacing = value;
		}
	}

	internal CommentsMarkups(WordDocument wordDocument, WComment comment)
		: base(wordDocument)
	{
		m_comment = comment;
	}

	private string GetBalloonValueForComments()
	{
		int num = base.Document.Comments.InnerList.IndexOf(Comment) + 1;
		string text = Comment.Format.UserInitials + num;
		if (Comment.Ancestor != null)
		{
			WComment ancestor = Comment.Ancestor;
			text = text + "R" + (base.Document.Comments.InnerList.IndexOf(ancestor) + 1);
		}
		return "Commented [" + text + "]";
	}

	internal void AppendInCommentsBalloon()
	{
		WTextBody wTextBody = Comment.TextBody.Clone() as WTextBody;
		wTextBody.SetOwner(new WSection(base.Document));
		base.ChangedValue = wTextBody;
		float num = 10f;
		string fontName = "Segoe UI";
		if (base.Document.Styles.FindByName("Balloon Text", StyleType.ParagraphStyle) is WParagraphStyle wParagraphStyle)
		{
			num = wParagraphStyle.CharacterFormat.FontSize;
			fontName = wParagraphStyle.CharacterFormat.FontName;
		}
		ApplyCommentsProperties(num);
		if (base.ChangedValue.ChildEntities.Count > 0)
		{
			WParagraph wParagraph = base.ChangedValue.ChildEntities[0] as WParagraph;
			if (wParagraph == null)
			{
				wParagraph = new WParagraph(base.Document);
				base.ChangedValue.ChildEntities.Insert(0, wParagraph);
			}
			WTextRange wTextRange = new WTextRange(base.Document);
			wTextRange.Text = GetBalloonValueForComments() + ": ";
			wTextRange.CharacterFormat.FontSize = num + 1f;
			wTextRange.CharacterFormat.FontName = fontName;
			wTextRange.CharacterFormat.Bold = true;
			wParagraph.ChildEntities.Insert(0, wTextRange);
		}
		else
		{
			base.ChangedValue.AddParagraph();
			IWTextRange iWTextRange = base.ChangedValue.LastParagraph.AppendText(GetBalloonValueForComments() + ": ");
			iWTextRange.CharacterFormat.Bold = true;
			iWTextRange.CharacterFormat.FontSize = num + 1f;
			iWTextRange.CharacterFormat.FontName = fontName;
		}
	}

	private void ApplyCommentsProperties(float fontSize)
	{
		foreach (TextBodyItem childEntity in base.ChangedValue.ChildEntities)
		{
			if (!(childEntity is WParagraph))
			{
				continue;
			}
			WParagraph obj = childEntity as WParagraph;
			obj.ParagraphFormat.AfterSpacing = 0f;
			obj.ParagraphFormat.BeforeSpacing = 0f;
			obj.ParagraphFormat.LineSpacing = 12f;
			obj.ParagraphFormat.LogicalJustification = HorizontalAlignment.Left;
			foreach (ParagraphItem childEntity2 in obj.ChildEntities)
			{
				if (childEntity2 is WTextRange)
				{
					(childEntity2 as WTextRange).CharacterFormat.FontSize = fontSize;
				}
			}
		}
	}
}
