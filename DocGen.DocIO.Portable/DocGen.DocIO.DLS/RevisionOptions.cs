namespace DocGen.DocIO.DLS;

public class RevisionOptions
{
	private bool m_showRevisionBars;

	private bool m_showRevisionMarks;

	private RevisionColor m_revisionBarsColor = RevisionColor.Red;

	private float m_revisionMarkWidth = 1f;

	private RevisionColor m_insertedTextColor;

	private RevisedTextEffect m_insertedTextEffect = RevisedTextEffect.Underline;

	private RevisedTextEffect m_deletedTextEffect = RevisedTextEffect.StrikeThrough;

	private RevisedTextEffect m_revisedPropertiesEffect;

	private RevisionColor m_deletedTextColor;

	private bool m_showDeletedText;

	private RevisionColor m_revisedPropertiesColor;

	private RevisionType m_showMarkup = RevisionType.None;

	private RevisionType m_showInBalloons = RevisionType.Deletions | RevisionType.Formatting;

	private RevisionBalloonsOptions m_balloonOptions = RevisionBalloonsOptions.Inline;

	private CommentDisplayMode m_commentDisplayMode;

	private RevisionColor m_commentColor = RevisionColor.Red;

	public CommentDisplayMode CommentDisplayMode
	{
		get
		{
			return m_commentDisplayMode;
		}
		set
		{
			m_commentDisplayMode = value;
		}
	}

	public RevisionColor CommentColor
	{
		get
		{
			return m_commentColor;
		}
		set
		{
			m_commentColor = value;
		}
	}

	internal bool ShowRevisionBars
	{
		get
		{
			return m_showRevisionBars;
		}
		set
		{
			m_showRevisionBars = value;
		}
	}

	internal bool ShowRevisionMarks
	{
		get
		{
			return m_showRevisionMarks;
		}
		set
		{
			m_showRevisionMarks = value;
		}
	}

	public RevisionColor RevisionBarsColor
	{
		get
		{
			return m_revisionBarsColor;
		}
		set
		{
			m_revisionBarsColor = value;
		}
	}

	public RevisionColor InsertedTextColor
	{
		get
		{
			return m_insertedTextColor;
		}
		set
		{
			m_insertedTextColor = value;
		}
	}

	internal float RevisionMarkWidth
	{
		get
		{
			return m_revisionMarkWidth;
		}
		set
		{
			m_revisionMarkWidth = value;
		}
	}

	internal RevisedTextEffect InsertedTextEffect
	{
		get
		{
			return m_insertedTextEffect;
		}
		set
		{
			m_insertedTextEffect = value;
		}
	}

	public RevisionColor DeletedTextColor
	{
		get
		{
			return m_deletedTextColor;
		}
		set
		{
			m_deletedTextColor = value;
		}
	}

	internal RevisedTextEffect DeletedTextEffect
	{
		get
		{
			return m_deletedTextEffect;
		}
		set
		{
			m_deletedTextEffect = value;
		}
	}

	public RevisionColor RevisedPropertiesColor
	{
		get
		{
			return m_revisedPropertiesColor;
		}
		set
		{
			m_revisedPropertiesColor = value;
		}
	}

	internal RevisedTextEffect RevisedPropetiesEffect
	{
		get
		{
			return m_revisedPropertiesEffect;
		}
		set
		{
			m_revisedPropertiesEffect = value;
		}
	}

	internal bool ShowDeletedText
	{
		get
		{
			return m_showDeletedText;
		}
		set
		{
			m_showDeletedText = value;
		}
	}

	public RevisionType ShowMarkup
	{
		get
		{
			return m_showMarkup;
		}
		set
		{
			m_showMarkup = value;
			SetTrackChangesOptions();
		}
	}

	public RevisionType ShowInBalloons
	{
		get
		{
			return m_showInBalloons;
		}
		set
		{
			m_showInBalloons = value;
		}
	}

	internal RevisionBalloonsOptions BalloonOptions
	{
		get
		{
			return m_balloonOptions;
		}
		set
		{
			m_balloonOptions = value;
		}
	}

	private void SetTrackChangesOptions()
	{
		if (m_showMarkup != RevisionType.None)
		{
			ShowRevisionBars = true;
		}
		if (m_showMarkup == RevisionType.None)
		{
			ShowRevisionBars = false;
		}
		if ((m_showMarkup & RevisionType.Insertions) == RevisionType.Insertions)
		{
			ShowRevisionMarks = true;
		}
		if ((m_showMarkup & RevisionType.Deletions) == RevisionType.Deletions)
		{
			BalloonOptions |= RevisionBalloonsOptions.Deletions;
		}
		if ((m_showMarkup & RevisionType.Formatting) == RevisionType.Formatting)
		{
			BalloonOptions |= RevisionBalloonsOptions.Formatting;
		}
	}
}
