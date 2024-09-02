namespace DocGen.DocIO.DLS;

public abstract class OwnerHolder
{
	protected WordDocument m_doc;

	private OwnerHolder m_owner;

	public WordDocument Document
	{
		get
		{
			if (m_owner == null)
			{
				return m_doc;
			}
			return m_owner.Document;
		}
	}

	internal OwnerHolder OwnerBase => m_owner;

	public OwnerHolder()
	{
	}

	public OwnerHolder(WordDocument doc)
		: this(doc, null)
	{
	}

	public OwnerHolder(WordDocument doc, OwnerHolder owner)
	{
		m_doc = doc;
		m_owner = owner;
	}

	internal void SetOwner(OwnerHolder owner)
	{
		m_owner = owner;
		if (owner != null)
		{
			m_doc = owner.Document;
		}
	}

	internal void SetOwnerDoc(WordDocument doc)
	{
		m_doc = doc;
	}

	internal void SetOwner(WordDocument doc, OwnerHolder owner)
	{
		m_owner = owner;
		if (owner == null)
		{
			m_doc = doc;
		}
		else
		{
			m_doc = owner.Document;
		}
	}

	internal virtual void OnStateChange(object sender)
	{
		if (m_owner != null)
		{
			m_owner.OnStateChange(sender);
		}
	}

	internal virtual void Close()
	{
		m_doc = null;
		m_owner = null;
	}
}
