namespace DocGen.DocIO.DLS;

public class Footnote
{
	private WTextBody m_separator;

	private WTextBody m_continuationSeparator;

	private WTextBody m_continuationNotice;

	private WordDocument m_ownerDoc;

	public WTextBody Separator
	{
		get
		{
			if (m_separator == null || (m_separator.ChildEntities.Count == 0 && !m_ownerDoc.IsOpening && !m_ownerDoc.IsCloning))
			{
				m_separator = new WTextBody(m_ownerDoc, null);
				m_separator.AddParagraph().AppendText('\u0003'.ToString()).CharacterFormat.Special = true;
			}
			return m_separator;
		}
		set
		{
			m_separator = value;
			if (m_separator != null)
			{
				m_separator.SetOwner(m_ownerDoc, null);
			}
		}
	}

	public WTextBody ContinuationSeparator
	{
		get
		{
			if (m_continuationSeparator == null || (m_continuationSeparator.ChildEntities.Count == 0 && !m_ownerDoc.IsOpening && !m_ownerDoc.IsCloning))
			{
				m_continuationSeparator = new WTextBody(m_ownerDoc, null);
				m_continuationSeparator.AddParagraph().AppendText('\u0004'.ToString()).CharacterFormat.Special = true;
			}
			return m_continuationSeparator;
		}
		set
		{
			m_continuationSeparator = value;
			if (m_continuationSeparator != null)
			{
				m_continuationSeparator.SetOwner(m_ownerDoc, null);
			}
		}
	}

	public WTextBody ContinuationNotice
	{
		get
		{
			if (m_continuationNotice == null)
			{
				m_continuationNotice = new WTextBody(m_ownerDoc, null);
			}
			return m_continuationNotice;
		}
		set
		{
			m_continuationNotice = value;
			if (m_continuationNotice != null)
			{
				m_continuationNotice.SetOwner(m_ownerDoc, null);
			}
		}
	}

	public Footnote(WordDocument document)
	{
		m_ownerDoc = document;
	}

	internal Footnote(Footnote footnote)
	{
		m_separator = footnote.Separator.Clone() as WTextBody;
		m_continuationSeparator = footnote.ContinuationSeparator.Clone() as WTextBody;
		m_continuationNotice = footnote.ContinuationNotice.Clone() as WTextBody;
	}

	public Footnote Clone()
	{
		return new Footnote(this);
	}

	internal void SetOwner(WordDocument document)
	{
		m_ownerDoc = document;
		if (m_separator != null)
		{
			m_separator.SetOwner(m_ownerDoc, null);
		}
		if (m_continuationSeparator != null)
		{
			m_continuationSeparator.SetOwner(m_ownerDoc, null);
		}
		if (m_continuationNotice != null)
		{
			m_continuationNotice.SetOwner(m_ownerDoc, null);
		}
	}

	internal void Close()
	{
		if (m_separator != null)
		{
			m_separator.Close();
			m_separator = null;
		}
		if (m_continuationSeparator != null)
		{
			m_continuationSeparator.Close();
			m_continuationSeparator = null;
		}
		if (m_continuationNotice != null)
		{
			m_continuationNotice.Close();
			m_continuationNotice = null;
		}
		m_ownerDoc = null;
	}
}
